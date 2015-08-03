using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBDataAccess;
using System.IO;
using System.Diagnostics;

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
				"VenueType",
				//sourceFile
				"C:/Users/Devin/Documents/GitHub/APPI.Meetball/Structs/Venue.cs");
			
			//will open up a txt file with generated code
			//BuildStructs.buildStructsFromBL(
			//	//struct name
			//	"UserInfoStruct",
			//	//sourceFile
			//	"C:/Users/Devin/Documents/GitHub/APPI.Meetball/AppUser/UserInfoStruct.cs");

			//opens up text files with 
			//new LookAtServices().lookAtServiceDurations();
		}
	}
}
