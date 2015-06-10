﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAutomation
{
	public class BuildStructs
	{
		public static void buildStructsFromBL(string structName, string sourceFile)
		{
			var sb = new StringBuilder();
			var searchingFor = "public struct " + structName;

			var s = new Struct(structName);

			using (var streamReader = File.OpenText(sourceFile))
			{
				var foundStruct = false;

				while (!streamReader.EndOfStream)
				{
					var line = streamReader.ReadLine();
					
					if (foundStruct)
					{
						//found the next struct
						if (line.Contains("public struct"))
						{
							//we don't need it
							break;
						}
						else
						{
							if (line.Contains("public"))
							{
								if (!line.Contains(';'))
									break;
								//found a member
								var m = Member.extractMember(line);
								s.members.Add(m);
							}
						}
					}
					else
					{
						if (line.Contains(searchingFor))
						{
							//found the public struct [structName]
							foundStruct = true;
						}
					}
				}
			}



			var file = "C:/Users/Devin/Desktop/stuff/structBuilder_tmp.txt";

			File.WriteAllText(file, s.toCSharp());
			Process.Start(file);

		}

		private class Struct
		{
			public string origionalName;
			public string name;
			public List<Member> members;

			public Struct(string name)
			{
				members = new List<Member>();
				this.name = name;
				this.origionalName = name;
			}

			public string toCSharp()
			{
				var sb = new StringBuilder();

				//build decleration
				sb.Append("[DataContract]");
				sb.Append(Environment.NewLine);
				sb.Append("public struct ");
				sb.Append(name);
				sb.Append(Environment.NewLine);
				sb.Append('{');
				sb.Append(Environment.NewLine);

				//build members
				for (int i = 0; i < members.Count; i++)
				{
					sb.Append(members[i].toCSharp());
					sb.Append(Environment.NewLine);
				}

				//build constructor
				sb.Append("\tpublic ");
				sb.Append(name);
				sb.Append('(');
				for (int i = 0; i < members.Count; i++)
				{
					sb.Append(members[i].type);
					sb.Append(" ");
					sb.Append(members[i].name);
					if(i+1 < members.Count)
						sb.Append(", ");
				}
				sb.Append(')');
				sb.Append(Environment.NewLine);
				sb.Append("\t{");
				sb.Append(Environment.NewLine);

				for (int i = 0; i < members.Count; i++)
				{
					sb.Append("\t\tthis.");
					sb.Append(members[i].name);
					sb.Append(" = ");
					
					//in case member is a DateTime we need to convert it to a Date string
					if (members[i].type == "DateTime")
					{
						sb.Append("Date.ToString(");
						sb.Append(members[i].name);
						sb.Append(")");
					}
					else
						sb.Append(members[i].name);

					sb.Append(";");
					sb.Append(Environment.NewLine);
				}

				sb.Append("\t}");
				sb.Append(Environment.NewLine);
				sb.Append(Environment.NewLine);

				//build old struct to new struct constructor
				sb.Append("\tpublic ");
				sb.Append(name);
				sb.Append("(Meetball.Structs.");
				sb.Append(origionalName);
				sb.Append(" item)");
				sb.Append(Environment.NewLine);
				sb.Append("\t\t: this(");

				for (int i = 0; i < members.Count; i++)
				{
					sb.Append("item.");
					sb.Append(members[i].origionalName);
					if (i + 1 < members.Count)
						sb.Append(", ");
				}

				sb.Append(") { }");
				sb.Append(Environment.NewLine);
				sb.Append(Environment.NewLine);


				//build implicit operation for old struct to new struct
				sb.Append("\tpublic static implicit operator ");
				sb.Append(name);
				sb.Append("(Meetball.Structs.");
				sb.Append(origionalName);
				sb.Append(" item)");
				sb.Append(Environment.NewLine);
				sb.Append("\t{");
				sb.Append(Environment.NewLine);
				sb.Append("\t\treturn new ");
				sb.Append(name);
				sb.Append("(item);");
				sb.Append(Environment.NewLine);
				sb.Append("\t}");
				sb.Append(Environment.NewLine);
				sb.Append(Environment.NewLine);

				sb.Append("\tpublic static ");
				sb.Append(name);
				sb.Append(" cast (Meetball.Structs.");
				sb.Append(origionalName);
				sb.Append(" items[])");
				sb.Append(Environment.NewLine);
				sb.Append("\t{");
				sb.Append(Environment.NewLine);
				sb.Append("\t\tvar length = items != null ? items.Length : 0;");
				sb.Append(Environment.NewLine);
				sb.Append("\t\tvar array = new ");
				sb.Append(name);
				sb.Append("[length];");
				sb.Append(Environment.NewLine);
				sb.Append("\t\tfor (int i = 0; i < length; i++)");
				sb.Append(Environment.NewLine);
				sb.Append("\t\t{");
				sb.Append(Environment.NewLine);
				sb.Append("\t\t\tarray[i] = items[i];");
				sb.Append(Environment.NewLine);	
				sb.Append("\t\t}");
				sb.Append(Environment.NewLine);
				sb.Append(Environment.NewLine);
				sb.Append("\t\treturn array;");
				sb.Append(Environment.NewLine);
				sb.Append("\t}");
				sb.Append(Environment.NewLine);
				sb.Append('}');

				return sb.ToString();
			}
		}

		private class Member
		{
			public string origionalName;
			public string type;
			public string name;

			public Member(string type, string origionalName)
			{
				this.type = type;
				this.name = origionalName;
				this.origionalName = origionalName;
			}

			public string toCSharp()
			{
				var sb = new StringBuilder();
				sb.Append("\t[DataMember]");
				sb.Append(Environment.NewLine);
				sb.Append("\tpublic ");
				sb.Append(type == "DateTime" ? "string" : type);
				sb.Append(' ');
				sb.Append(name);
				sb.Append(';');
				sb.Append(Environment.NewLine);
				return sb.ToString();
			}

			public static Member extractMember(string line)
			{
				//\t\tpublic_type_name;
				string type = "";
				var charList = new List<char>();
				string name = "";
				int phase = 0;

				string publicStr = "public ";
				int publicStrLen = publicStr.Length;
				int lineLenMinusOne = line.Length -1;

				for (int i = 0; i < line.Length; i++)
				{
					//search for public
					if (phase == 0)
					{
						if (publicStrLen + i < lineLenMinusOne)
						{
							if (line[i] == 'p' && line[i + 1] == 'u' && line[i + 2] == 'b'
								&& line[i + 3] == 'l' && line[i + 4] == 'i' && line[i + 5] == 'c'
								&& line[i + 6] == ' ')
							{
								//set to location of first letter of the type
								i += 7;

								//enter next phase
								phase++;
							}
						}
					}

					//type phase
					if (phase == 1)
					{
						if (line[i] == ' ')
						{
							//set type string
							type = new string(charList.ToArray());
							charList.Clear();

							//set index to spot after space
							i++;

							//enter next phase
							phase++;
						}
						else
						{
							charList.Add(line[i]);
						}
					}

					//name phase
					if (phase == 2)
					{
						if (line[i] == ';')
						{
							name = new string(charList.ToArray());
							break;
						}
						else
						{
							charList.Add(line[i]);
						}
					}
				}

				return new Member(type, name);
			}
		}
		
	}
}
