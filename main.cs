using System;								//Used for console out.
using System.Net;							//Used for network components to aid httpClient.
using System.Net.Http;					//Used for httpClient.
using System.Collections.Generic;	//Used for list.
using System.Threading;					//Used for sleep.

namespace SimpleCrawler
{
	class mainClass
	{
		static void Main(string[] args)
		{
			//Local Variables
			string address = null;                             //Full address of a URL.
			int numHops = -1;                                  //The number of hops we are allowed by the user.
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

			//Print the website results of the recursion.
			//Starts at once since we're supposed to only print the hop URLs.
			for (int i = 1; i < (visitedWebsites.Count - 1); i++)
			{
				Console.WriteLine(i + ": " + visitedWebsites[i]);
			}
			//Print the HTML page.
			Console.WriteLine(visitedWebsites[visitedWebsites.Count - 1]);

			if (visitedWebsites.Count == 0)
			{
				Console.WriteLine("The inital link given was invalid and therefore could not be expanded upon.");
			}
			Console.ReadKey();
		}

		private static bool Crawl(string address, int numHops, List<string> visitedWebsites)
		{
			//Local Variables
			var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }); //The HTTPClient we are using.
			string addressModifier = null;   //The modifier in a URL, would be "/hello/" would be a modifier in "http://test.com/hello/").
			string stringStorage;				//Used to store a string temporarily.
			bool lowerLevel = false;         //This is used to store the lower level of recursion's signal to exit recursion based on errors.
														//If we need to exit before we've visited all the websites it returns true.
			bool foundTLD = false;           //This is used to indicate when we are nearing the end of the base URL. Becomes true when we have found a TLD
														//(.com, .org, ect), otherwise false.

			//Remove the trailing '/' from the inital address following code requirements.
			if (address.EndsWith("/")) { address = address.Remove(address.Length - 1); } //-1 since starts at 0.

			//Check if this address has already been visited. If it has, we can skip this and go back a recursion.
			//This should not be much more inefficient than Compare() because it checks through each item to my knowledge.
			for (int i = 0; i < visitedWebsites.Count; i++)
			{
				//the remove is used to remove the inital http/https to ensure we don't visit the same site twice.
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
			try { client.BaseAddress = new Uri(address); }
			catch (Exception e)
			{
				return false;
			}

			//Create the client.
			HttpResponseMessage response = client.GetAsync(addressModifier).Result;

			//Deal with error codes to ensure the link is good. 
			try { response.EnsureSuccessStatusCode(); }
			catch (Exception e)
			{
				if (e.Message.Contains("500"))
            {
					//Retry 3 times as the teacher says, I give a 1 second gap for it to try.
					int i = 0;
					while (i < 3 && (int)response.StatusCode == 500)
					{
						Thread.Sleep(1000);
						response = client.GetAsync(addressModifier).Result;
						i++;
					}
				}
				
				//If we are here this means that an issue occurred, and since HTTPClient handles 300s this means its a 400, which is simply leave and choose another url.
				if((int)response.StatusCode != 200)
				{
					//This covers the condition that the first given site is bad, meaning we won't have a fallback to explore, thus visitedWebsites does not get populated.
					if(visitedWebsites.Count == 0) {
						visitedWebsites.Add(response.Content.ReadAsStringAsync().Result); }
					return false;
				}
			}

			//Add to the list of websites we've found.
			visitedWebsites.Add(address + addressModifier);

			//Parse the response of the webpage.
			string result = response.Content.ReadAsStringAsync().Result;

         //Start breaking down the response.
         while (visitedWebsites.Count <= numHops && lowerLevel == false)
			{
				//Cut the website results down to the current first a href.
				if (result.Contains("<a") && result.Contains("href=") && result.Contains(">"))
				{
					//cut to the first <a
					result = result.Remove(0, result.IndexOf("<a"));

					//Find the other end of the tag.
					stringStorage = result.Substring(0, result.IndexOf('>'));

					//Clean up result in case we need another pass.
					result = result.Remove(0, result.IndexOf('>')); //Removes ahref.

					if (stringStorage.Contains("href="))
					{
						stringStorage = stringStorage.Substring(stringStorage.IndexOf("href="), stringStorage.Length - stringStorage.IndexOf("href=") - 1);

						//extract potential URL
						try { address = stringStorage.Split('\"')[1]; } //According to the reference webpage in the requirements page and what I've seen, the URL is always surrounded by quotes.
																						//This should work, but in the case that it doesn't due to something weird, don't want the program to die.
						catch (Exception e) { }

						//Check if address is a real one.
						if (address.Length > 4 && address.Substring(0, 4).Contains("http")) //According to requirements we only need to evaluate hrefs with references to absolute urls that have http or https.
						{

							//Start the next level of recursion.
							lowerLevel = (Crawl(address, numHops, visitedWebsites));

							if (visitedWebsites.Count == numHops + 1) //numhops checks if we have added a site already.
							{
								//This is here since we're at the end of the program and we're supposed to report the last page as text.
								visitedWebsites.Add(response.Content.ReadAsStringAsync().Result);
							}
						}
					}
				}
				else //At this point there's no hrefs left and we need to break loop.
				{
					//This is here since we've encountered a page without any accessible embedded references.
					visitedWebsites.Add(response.Content.ReadAsStringAsync().Result);
					lowerLevel = true;
				}
			}
			return lowerLevel;
		}
	}
}