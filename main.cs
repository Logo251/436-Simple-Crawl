using System;        //Used for console out.
using System.Net;
using System.Net.Http;  //Used for httpclient.

namespace SimpleCrawler
{
   class mainClass
	{
		static void Main(string[] args)
		{
			//Local Variables
			string address = null;                 //Full address of a URL.
			string addressModifier = null;            //The modifier in a URL, would be "/hello/" would be a modifier in "http://test.com/hello/").
			bool foundTLD = false;                 //This is used to indicate when we are nearing the end of the base URL. Becomes true when we have found a TLD (.com, .org, ect),
																	//otherwise false.
			var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }); //The HTTPClient we are using.
			try { int numHops = int.Parse(args[1]); } //The number  of hops our crawler will make at most.
			catch (Exception e)
			{
				Console.WriteLine("Number of hops allowed was not in acceptable format.");
				return; //Turn off the program since we have bad input, no reason to continue.
			}


			//TODO:Will loop around here


			//Remove the trailing '/' following code requirements.
			if(address.EndsWith("/")) { address = address.Remove(address.Length); }

			//Process the address given to extract base address and modifier
			foundTLD = false;
			for (int i = 0; i < address.Length; i++)
         {
				if(address[i] == '.') { foundTLD = true; }
				if(foundTLD == true && address[i] == '/')	//This triggers when we found the / ending the base url, such as the / in "google.com/"
            {
					addressModifier = address.Substring(i, address.Length - i); //Remove the modifier component of the base url.
					address = address.Substring(0, i); //Set address to be the base url component, for example "http://google.com".
            }
         }

			client.BaseAddress = new Uri(address);
			HttpResponseMessage response = client.GetAsync(addressModifier).Result;
			response.EnsureSuccessStatusCode();
			string result = response.Content.ReadAsStringAsync().Result;
			Console.WriteLine("Result: " + result);
			Console.ReadKey();
		}
	}
}