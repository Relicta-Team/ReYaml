using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using System.IO;
using YamlDotNet.Serialization;

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
			
			if (function == "parse_string")
			{
			    if (args.Length != 1) return;
                try
                {
					var d = ParseString(args[0]);
					output.Append(d);
                }
                catch (YamlDotNet.Core.YamlException yex)
                {
					output.Append($"$EX$:{yex.Message} (line {yex.Start.Line} at col {yex.Start.Column})");
                }
				catch (Exception ex)
                {
					output.Append($"$EX$:{ex}");
                }
            }

        }

		static StringBuilder ParseString(string str) {
			var des = new YamlDotNet.Serialization.Deserializer();
			var data = des.Deserialize(str);

			StringBuilder sb = new StringBuilder();
			if (data == null) throw new NullReferenceException("Unhandled error on parse data");
			convertDataToGame(sb,data);
			return sb;
		}

		private static Regex patternNum = new Regex(@"^((((\$|0x)[0-9a-fA-F]+)|(\.[0-9]+))|(\b[0-9]+(\.[0-9]+|[eE][-+]?[0-9]+)?))$");
		private static Regex patternBool = new Regex(@"^(y|Y|yes|Yes|YES|true|True|TRUE|on|On|ON|false|n|N|no|No|NO|False|FALSE|off|Off|OFF)$");
		private static Regex patternBoolTrue = new Regex(@"^(y|Y|yes|Yes|YES|true|True|TRUE|on|On|ON)$");

		static void convertDataToGame(StringBuilder sb, object val)
        {
			
			if (val is Dictionary<object,object>)
            {
				var valp = (Dictionary<object, object>)val;
				//map struct: chmp[PAIR1, PAIR2..]
#if ISDLL
				sb.Append("createHashMapFromArray[");
#else
				sb.Append("CHMP[\n");
#endif
				var iter = 0;
				foreach(var kp in valp)
                {
					if (iter > 0)
                    {
						sb.Append(",");
#if !ISDLL
						sb.Append("\n");
#endif
					}

					//kv-pair: [KEY,VAL]
					sb.Append("[");
					convertDataToGame(sb,kp.Key);
					sb.Append(",");
					convertDataToGame(sb, kp.Value);
					sb.Append("]");

					iter++;
				}
				sb.Append("]");
#if !ISDLL
				sb.Append("\n");
#endif
			}
			else if (val is List<object>)
			{
				var valp = (List<object>)val;
				var iter = 0;
				//list struct
				sb.Append("[");
#if !ISDLL
				sb.Append("\n");
#endif
				foreach(var el in valp)
                {
					if (iter > 0)
                    {
						sb.Append(",");
#if !ISDLL
						sb.Append("\n");
#endif
					}
					convertDataToGame(sb, el);
					iter++;
                }
				sb.Append("]");

#if !ISDLL
				sb.Append("\n");
#endif

			}
			else if (val is string)
            {
				//include booleans, and numbers
				var valp = (string)val;
				if (patternNum.IsMatch(valp))
                {
					sb.Append(valp);
                } else if (patternBool.IsMatch(valp))
                {
					var isTrue = patternBoolTrue.IsMatch(valp);
					sb.Append(isTrue.ToString().ToLower());
                } else
                {
					sb.Append(valp.FormatToRV());
                }
            }
			else
            {
				throw new Exception($"unknown val type: {val.GetType()}");
            }

            Console.WriteLine();
        }
	}
}
