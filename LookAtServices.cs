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

		public void lookAtServiceDurations()
		{
			DurationsToText(GetDurations(100));
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

		private Dictionary<String, List<double>> GetDurations(int fromXDaysAgo)
		{
			var utcNow = DateTime.UtcNow;
			var dictAll = new Dictionary<String, List<double>>();
			var tasks = new Task<Dictionary<String, List<double>>>[fromXDaysAgo];

			for (int i = 0; i < fromXDaysAgo; i++)
			{
				var tmp = i;
				var afewweeksago = utcNow.AddDays(-(i + 1));
				var limit = utcNow.AddDays(-i);
				tasks[i] = Task<Dictionary<String, List<double>>>.Factory.StartNew(() =>
				{
					try
					{
						Console.WriteLine("{0} started  the DB call", Task.CurrentId);
						var result = DBDataAccess.MethodLog
						.Find(x => x.CallDate > afewweeksago && x.CallDate < limit)
						.OrderBy(x => x.Duration).ToArray();

						var dict = new Dictionary<String, List<double>>();
						for (int j = 0; j < result.Length; j++)
						{
							if (dict.ContainsKey(result[j].MethodLogName))
							{
								dict[result[j].MethodLogName].Add(result[j].Duration.Value);
							}
							else
							{
								var list = new List<double>();
								list.Add(result[j].Duration.Value);
								dict.Add(result[j].MethodLogName, list);
							}
						}

						return dict;
					}
					catch (Exception e)
					{
						Console.WriteLine("{0} had an exception", Task.CurrentId);
						return new Dictionary<string, List<double>>();

					}
				});

			}

			while (tasks.Length > 0)
			{
				int index = 0;

				index = Task.WaitAny(tasks);

				Console.WriteLine("Task {0} finished", tasks[index].Id);

				var dict = tasks[index].Result;
				var keys = dict.Keys.ToArray();

				if (tasks[index].Exception != null)
					Console.WriteLine(tasks[index].Exception.InnerException.Message);

				for (int i = 0; i < keys.Length; i++)
				{
					if (dictAll.ContainsKey(keys[i]))
					{
						dictAll[keys[i]].AddRange(dict[keys[i]]);
					}
					else
					{
						dictAll.Add(keys[i], dict[keys[i]]);
					}
				}

				var temp = new Task<Dictionary<string, List<double>>>[tasks.Length - 1];
				var counter = 0;
				for (int i = 0; i < tasks.Length; i++)
				{
					if (i != index)
					{
						temp[counter++] = tasks[i];
					}
				}
				tasks = temp;
			}

			return dictAll;
		}

		private void DurationsToText(Dictionary<String, List<double>> dict)
		{
			var list2 = new List<KeyValuePair<string, KeyValuePair<double, int>>>();

			foreach (string key in dict.Keys)
			{
				double mean = dict[key]
					.OrderBy(x=> x).ToArray()
					[(int)(dict[key].Count / 2)];

				list2.Add(new KeyValuePair<string, KeyValuePair<double, int>>(
					key, 
					new KeyValuePair<double, int>(
						mean,
						dict[key].Count)));
				
			}

			var sb = new StringBuilder();

			var array = list2.OrderBy(x => x.Value.Key).ToArray();
			var s = "Order by mean Duration";
			sb.Append(s);
			Console.Write(s);
			for (int i = 0; i < array.Length; i++)
			{
				s = String.Format("\n{0} - {1} - {2}", array[i].Key, array[i].Value.Key, array[i].Value.Value);
				sb.Append(s);
				Console.Write(s);
			}

			var durationMean = Environment.CurrentDirectory + "\\durationMean.txt";
			File.WriteAllText(durationMean, sb.ToString().Replace("\n", Environment.NewLine));
			Process.Start(durationMean);
			sb.Clear();

			s = "Order by call amount";
			sb.Append(s);
			Console.Write("\n" + s);
			array = list2.OrderBy(x => x.Value.Value).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				s = String.Format("\n{0} - {1} - {2}", array[i].Key, array[i].Value.Key, array[i].Value.Value);
				sb.Append(s);
				Console.Write(s);
			}

			var callRyou = Environment.CurrentDirectory + "\\callRyou.txt";
			File.WriteAllText(callRyou, sb.ToString().Replace("\n", Environment.NewLine));
			Process.Start(callRyou);
		}
	}
}
