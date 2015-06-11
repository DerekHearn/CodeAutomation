using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAutomation
{
	class Program
	{
		static void Main(string[] args)
		{
			//looks at the services in Service.svc.cs and pulls up a list of all 
			// publicilly usable params
			//new LookAtServices().work();

			//will open up a txt file with generated code
			BuildStructs.buildStructsFromBL(
				//struct name
				"LiveBroadcastBroadcasterStruct",
				//sourceFile
				"C:/Users/Devin/Documents/GitHub/APPI.Meetball/Structs/GetEventStruct.cs");

			//will open up a txt file with generated code
			BuildStructs.buildStructsFromBL(
				//struct name
				"GetLiveBroadcastStruct",
				//sourceFile
				"C:/Users/Devin/Documents/GitHub/APPI.Meetball/Structs/GetEventStruct.cs");
		}
	}
}
