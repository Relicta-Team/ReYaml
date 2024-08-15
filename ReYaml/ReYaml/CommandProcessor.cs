using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReYaml
{
	/// <summary>
	/// Класс обработки команд от движка Армы.
	/// </summary>
	static class CommandProcessor
	{
		/// <summary>
		/// Commands handler from RVEngine. This function called in main game thread.
		/// !!! This function should not throw unhandled exceptions, otherwise the application will crash
		/// </summary>
		/// <param name="output">Data for game in string repr</param>
		/// <param name="outputSize"></param>
		/// <param name="function">Command name</param>
		/// <param name="args">Arguments of strings</param>
		internal static void ParseCommand(StringBuilder output, int outputSize, string function, string[] args)
		{
			/*
			 if (function == "parse_string")
			{
			    if (args.Length != 1) return;
				output.Append(ParseString(args[0]));
			}
			 */
		}

		static StringBuilder ParseString(string str) { return null; }
	}
}
