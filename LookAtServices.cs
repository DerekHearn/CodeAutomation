using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAutomation
{
	public class LookAtServices
	{
		/// <summary>
		/// looks at the services in Service.svc.cs and pulls up a list of all 
		/// publicilly usable params
		/// </summary>
		public void work()
		{
			var serviceFile = "C:/Users/Devin/Documents/GitHub/APPI.Services/Service.svc.cs";
			var text = File.ReadAllText(serviceFile);

			var dict = ParseOutIdentifiers(text);

			var file = "C:/Users/Devin/Desktop/stuff/ServiceParamList_tmp.txt";
			File.WriteAllText(file, buildOutputText(dict));

			//open up editor to view text
			Process.Start(file);
		}

		/// <summary>
		/// Key of dictionary is a key value pair of type and param name
		/// the values represent number of times that unique value pair was found
		/// </summary>
		private Dictionary<KeyValuePair<string, string>, int> ParseOutIdentifiers(string text)
		{
			bool logged = false;
			bool pub = false;
			bool open = false;
			int openIndex = 0;

			// Key of dictionary is a key value pair of type and param name
			// the values represent number of times that unique value pair was found
			var dict = new Dictionary<KeyValuePair<string, string>, int>();

			var paramList = new List<string>();

			for (int i = 0; i < text.Length; i++)
			{
				//found the "[Lo" of "[Logged]"
				if (logged)
				{
					//found the "public" of the service method decleration
					if (pub)
					{
						//found the '(' of the parameter area? 
						if (open)
						{
							//search for the closing paran
							if (text[i] == ')')
							{
								open = false;
								pub = false;
								logged = false;
								//only bother if there are parans ignore ( ) or ()
								if (i - openIndex > 2)
								{
									//pull out the substring to work with individually
									var parans = text.Substring(openIndex + 1, i - openIndex - 1).Trim()
										//seperate the type from the identifier
										.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

									foreach (string param in parans)
									{
										paramList.Add(param.Trim());
										var parts = param.Split(' ');
										if (parts.Length == 2)
										{
											var pair = new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim());
											//count up found params
											if (dict.ContainsKey(pair))
												dict[pair]++;
											else
												dict.Add(pair, 1);
										}
									}
								}
							}
						}
						else
						{
							//search for the '(' of the parameter area
							if (text[i] == '(')
							{
								open = true;
								openIndex = i;
							}
						}
					}
					else
					{
						//search for the "public" of the service method decleration
						if (i + 7 < text.Length &&
							text[i] == 'p' && text[i + 1] == 'u' &&
							text[i + 2] == 'b' && text[i + 3] == 'l' &&
							text[i + 4] == 'i' && text[i + 5] == 'c')
						{
							pub = true;
						}
					}
				}
				else
				{
					//search for the "[Lo" of "[Logged]"
					if(i + 3 < text.Length &&
						text[i] == '[' && text[i+1] == 'L' && text[i+2] == 'o')
					{
						logged = true;
					}
				}
			}

			return dict;
		}

		/// <summary>
		/// alphabetical list of parameters
		/// </summary>
		private string buildOutputText(Dictionary<KeyValuePair<string, string>, int> dict)
		{
			var sb = new StringBuilder();

			foreach (KeyValuePair<string, string> key
				in
				//order by param name - alphabetical
				dict.Keys.OrderBy(param => param.Value))
			{
				//key is the type
				sb.Append(key.Key);
				sb.Append(' ');

				//value is the identifier
				sb.Append(key.Value);
				sb.Append(" - ");

				//the int represents the count of 
				sb.Append(dict[key]);
				sb.Append(Environment.NewLine);
			}

			return sb.ToString();
		}
	}
}
