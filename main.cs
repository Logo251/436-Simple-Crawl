using System;				//Used for console out.
using System.Net;			
using System.Net.Http;  //Used for httpclient.
using System.Collections.Generic;

namespace SimpleCrawler
{
   class mainClass
	{
		static void Main(string[] args)
		{
			//Local Variables
			string address = null;										//Full address of a URL.
			int numHops = -1;												//The number of hops we are allowed by the user.
			List<string> visitedWebsites = new List<string>(); //Used to store the links we made.

			//Attempt to assign address to the given argument.
			try { address = args[0]; } //The number  of hops our crawler will make at most.
			catch (Exception e)
			{
				Console.WriteLine("Address given allowed was not in acceptable format.");
				return; //Turn off the program since we have bad input, no reason to continue.
			}

			//Attempt to assign numHops to the given argument.
			try { numHops = int.Parse(args[1]); } //The number  of hops our crawler will make at most.
			catch (Exception e)
			{
				Console.WriteLine("Number of hops allowed was not in acceptable format.");
				return; //Turn off the program since we have bad input, no reason to continue.
			}

			//Start the recursion.
			Crawl(address, numHops, visitedWebsites);

			//Print the results of the recursion.
			for(int i = 0; i < visitedWebsites.Count; i++)
         {
				Console.WriteLine(i + ": " + visitedWebsites[i]);
         }
			Console.ReadKey();
		}

      private static bool Crawl(string address, int numHops, List<string> visitedWebsites)
      {
			//Local Variables
			var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }); //The HTTPClient we are using.
			string addressModifier = null;	//The modifier in a URL, would be "/hello/" would be a modifier in "http://test.com/hello/").
			bool lowerLevel = false;         //This is used to store the lower level of recursion's signal to exit recursion.
			bool foundTLD = false;           //This is used to indicate when we are nearing the end of the base URL. Becomes true when we have found a TLD
														//(.com, .org, ect), otherwise false.

			//If we've hopped the sufficient amount then exit.
			if (visitedWebsites.Count > numHops) { return true; }

			//Remove the trailing '/' from the inital address following code requirements.
			if (address.EndsWith("/")) { address = address.Remove(address.Length - 1); } //-1 since starts at 0.

			//Check if this address has already been visited. If it has, we can skip this and go back a recursion.
			//This should not be much more inefficient than Compare() because it checks through each item to my knowledge.
			for (int i = 0; i < visitedWebsites.Count; i++)
			{
				//the remove is used to remove the inital http/https to ensure we don't visit the same site twice.
				visitedWebsites[i].TrimStart('h', 't', 't', 'p', 's');
				if (visitedWebsites[i].TrimStart('h', 't', 'p', 's') == address.TrimStart('h', 't', 'p', 's')) { return false; }
			}
			
			//Process the address given to extract base address and modifier for the client.
			foundTLD = false;
			for (int i = 0; i < address.Length; i++)
			{
				if (address[i] == '.') { foundTLD = true; }
				if (foundTLD == true && address[i] == '/')   //This triggers when we found the / ending the base url, such as the / in "google.com/"
				{
					addressModifier = address.Substring(i, address.Length - i); //Remove the modifier component of the base url.
					address = address.Substring(0, i); //Set address to be the base url component, for example "http://google.com".
				}
			}

			//Take the given address and access it.
			client.BaseAddress = new Uri(address);
			HttpResponseMessage response = client.GetAsync(addressModifier).Result;
			response.EnsureSuccessStatusCode();

			//Parse the response of the webpage.
			string result = response.Content.ReadAsStringAsync().Result;

			//Since we're commited to this URL now, add to the list we've found.
			visitedWebsites.Add(address + addressModifier);

			//This line goes through every character of the string, and removes spaces and new lines.
			for (int i = 0; i < result.Length; i++) { if (i == ' ' || i == '\n') { result.Remove(i, 1); } }

			//Start breaking down the response.
			while (visitedWebsites.Count <= numHops && lowerLevel == false)
			{
				//Cut the website results down to the current first href.
				if (result.Contains("href"))
				{
					result = result.Substring(result.IndexOf("href"));

					//extract potential URL
					address = result.Split('\"')[1]; //According to the reference webpage in the requirements page and what I've seen, the URL is always surrounded by quotes.

					//Clean up result in case we need another pass.
					result = result.Remove(0, 4); //Removes href.

					//Check if address is a real one.
					if (address.Length > 4 && address.Substring(0, 4).Contains("http")) //According to requirements we only need to evaluate hrefs with references to absolute urls that have http or https.
					{

						//Check if this URL works.
						if (checkAddress(address))
						{
							lowerLevel = (Crawl(address, numHops, visitedWebsites));
						}
					}
				}
				else //At this point there's no hrefs left and we need to break loop.
            {
					lowerLevel = true;
            }
			}
			return lowerLevel;
		}

      private static bool checkAddress(string address)
      {
			Uri testURI = new Uri(address);
			if (testURI.IsWellFormedOriginalString() == true)
			{
				return true;
			}
			return false;
		}
   }
}