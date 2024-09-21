using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ReYaml
{
	class Program
	{
		static void Main(string[] args)
		{
			//input file 
			/*
			string path = "..\\..\\test.yml";
			string content = File.ReadAllText(path);
			Debug.Assert(content != "");

			


			StringBuilder output = new StringBuilder();
			CommandProcessor.ParseCommand(output, 100000, "parse_string", new string[] { content,"" });
			File.WriteAllText(path + "_out.yml", output.ToString());
            Console.WriteLine(output);
			
			*/
			

			/*
			 Types:
				[] - array
				123.456 - scalar
				true|false - bool
				"hello ""world"" my firend"	- string
				createHashMapFromArray[["key1","val1"],["key2",123]] - hashmap
			 Tests:
				in========>
				A   :      Hello
				B:
				  - c
				  - d
				  - e: [1,2,3]
			    C: |
				  multi
			      line

				out========>
				"createHashMapFromArray[
					[""A"",""Hello""],
					[""B"",
						[""c"",
						""d"", 
							createHashMapFromArray[
								[""e"",[1,2,3]]
							] 
						]
					],
					["C",""multi
line""]
				]"
			 */
		}
	}
}
