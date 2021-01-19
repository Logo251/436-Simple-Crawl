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
			int hasTraversed = 0;										//Number of times the program has crawled.
			string address = null;										//Full address of a URL.
			string addressModifier = null;							//The modifier in a URL, would be "/hello/" would be a modifier in "http://test.com/hello/").
			int numHops = -1;												//The number of hops we are allowed by the user.
			List<string> visitedWebsites = new List<string>(); //Used to store the links we made.
			bool goodAddressFound = false;							//Used for when we have found the next good address in our crawl.
			bool foundTLD = false;                             //This is used to indicate when we are nearing the end of the base URL. Becomes true when we have found a TLD
																				//(.com, .org, ect), otherwise false.
			var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }); //The HTTPClient we are using.


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

			//Remove the trailing '/' from the inital address following code requirements.
			if (address.EndsWith("/")) { address = address.Remove(address.Length); }

			//Logic to traverse.
			while (hasTraversed < numHops)
			{
				//Process the address given to extract base address and modifier
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

				//Add the URL to the list we've visited.
				visitedWebsites.Add(address);

				//This adds one to the URL since we crawled one.
				numHops++;

            client.BaseAddress = new Uri(address);
            HttpResponseMessage response = client.GetAsync(addressModifier).Result;
            response.EnsureSuccessStatusCode();
				Console.WriteLine(address + addressModifier);

            //Parse the response of the webpage.
            string result = response.Content.ReadAsStringAsync().Result;
            result = result.ToLower(); //Set all characters to lower to reduce oddness.
            for (int i = 0; i < result.Length; i++) { if (i == ' ' || i == '\n') { result.Remove(i, 1); } } //This line goes through every character of the string, and removes spaces and new lines.

				//Find a href
				goodAddressFound = false;
				while (goodAddressFound == false)
				{
					//Cut the website results down to the first href.
					result = result.Substring(result.IndexOf("href=\""));
               {
						//extract potential URL
						address = result.Split('\"')[1];

						//Check if address is a real one.
						if(address != "javascript" && address[0] != '#')
                  {
							//Add http to attempt to make an invalid url valid to httpclient.
							if (!result.Contains("http")) { address = "http://" + address; }

							//Check if this URL works.
							Uri testURI = new Uri(address);
							if(testURI.IsWellFormedOriginalString() == true)
							{
								if (address.EndsWith("/")) { address = address.Remove(address.Length); } //Removes trailing '/' according to requirements.
								if (!visitedWebsites.Contains(address))
								{
									goodAddressFound = true;
								}
							}
							//If this is not true the loop starts again, which cuts another substring and processes it.
                  }
               }
            }
			}
		}
	}
}