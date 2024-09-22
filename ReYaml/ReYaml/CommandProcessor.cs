using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Core;

namespace ReYaml
{
	/// <summary>
	/// Класс обработки команд от движка Армы.
	/// </summary>
	public static class CommandProcessor
	{
		private static StringBuilder partialBuffer = new StringBuilder();

		internal static bool debugPrinter = false;

		internal static void debugPrint(string data)
		{
			if (debugPrinter)
			{
				Console.WriteLine("REYAML_EXT_DEBUG:" + data);
			}
		}

		/// <summary>
		/// Commands handler from RVEngine. This function called in main game thread.
		/// !!! This function should not throw unhandled exceptions, otherwise the application will crash
		/// </summary>
		/// <param name="output">Data for game in string repr</param>
		/// <param name="outputSize"></param>
		/// <param name="function">Command name</param>
		/// <param name="args">Arguments of strings</param>
		public static void ParseCommand(StringBuilder output, int outputSize, string function, string[] args)
		{
			//fixed size in 2.16 -> 20480
			//utf8 ru-letter -> 2 bytes
			int semiOutputSize = (int)Math.Floor((double)(outputSize / 2));

			if (debugPrinter)
			{
				debugPrint($"Handle command (args {args.Length}): {function}");
				for (int i = 0; i < (args.Length); i++)
				{
					debugPrint($"Arg {i}: {args[i]}");
				}
			}

			if (function == "parse_string")
			{
				if (args.Length < 2) return;
				try
				{
					string charReplacer = args[1];
					bool useReplacer = charReplacer != "";
					string content = args[0];
					string postfixMapper = "";
					if (args.Length > 2)
                    {
						postfixMapper = args[2];
                    }

					if (useReplacer)
					{
						content = content.Replace(charReplacer, "\"");
					}

					var d = ParseString(content,ref postfixMapper);
					if (d.Length > semiOutputSize) //размер строки больше половины размера выхода
					{
						partialBuffer = d;
						output.Append("$PART$");
						return;
					}
					if (useReplacer)
					{
						output.Append(d.ToString().EncodingToRV().Replace("\"", charReplacer));
					} else
					{
						output.Append(d.ToString().EncodingToRV());
					}
				}
				catch (YamlDotNet.Core.YamlException yex)
				{
					output.Append($"$EX$:{yex.Message.EncodingToRV()} (line {yex.Start.Line} at col {yex.Start.Column})".EncodingToRV());
				}
				catch (Exception ex)
				{
					output.Append($"$EX$:{ex}".EncodingToRV());
				}
			}
			else if (function == "has_parts")
			{
				output.Append(partialBuffer.Length > 0);
			}
			else if (function == "next_read")
			{
				if (args.Length != 1)
				{
					output.Append($"$EX$:Command {function} wrong param count: {args.Length}");
					return;
				}

				string charReplacer = args[0];
				bool useReplacer = charReplacer != "";
				if (useReplacer)
				{
					output.Append(
						partialBuffer.ToString(0, Math.Min(semiOutputSize, partialBuffer.Length))
							.EncodingToRV().Replace("\"", charReplacer)
					);
				} else
				{
					output.Append(
						partialBuffer.ToString(0, Math.Min(semiOutputSize, partialBuffer.Length))
							.EncodingToRV()
					);
				}

				partialBuffer.Remove(0, Math.Min(semiOutputSize, partialBuffer.Length));
				return;
			} else if (function == "free_parts") {
				partialBuffer.Clear();
				output.Append(partialBuffer.Length == 0);
			} else if (function == "get_left_parts_count") {
				if (partialBuffer.Length == 0)
				{
					output.Append(0);
					return;
				}
				output.Append(Math.Ceiling((double)partialBuffer.Length / semiOutputSize));
			} else if (function == "set_debug")
			{
				if (args.Length != 1)
				{
					output.Append($"$EX$:{function} args size missmatch -> {args.Length}");
					return;
				}
				debugPrinter = args[0] == "true";
				output.Append(debugPrinter);
			} else
			{
				output.Append($"$EX$:No command found: {function}");
			}

        }

		static IDeserializer desBld = null;

		static StringBuilder ParseString(string str,ref string postfix) {
			//var des = new YamlDotNet.Serialization.Deserializer();
			//var data = des.Deserialize(str);

			var mergingParser = new MergingParser(new Parser(new StringReader(str)));
			if (desBld==null)
            {
				desBld = new DeserializerBuilder().Build();
			}
			var data = desBld.Deserialize(mergingParser);

			StringBuilder sb = new StringBuilder();
			if (data == null) throw new NullReferenceException("Unhandled error on parse data");
			convertDataToGame(sb,data, ref postfix);
			return sb;
		}

		private static Regex patternNum = new Regex(@"^((((\$|0x)[0-9a-fA-F]+)|-?(\.[0-9]+))|-?(\b[0-9]+(\.[0-9]+|[eE][-+]?[0-9]+)?))$");
		private static Regex patternBool = new Regex(@"^(y|Y|yes|Yes|YES|true|True|TRUE|on|On|ON|false|n|N|no|No|NO|False|FALSE|off|Off|OFF)$");
		private static Regex patternBoolTrue = new Regex(@"^(y|Y|yes|Yes|YES|true|True|TRUE|on|On|ON)$");

		static void convertDataToGame(StringBuilder sb, object val,ref string postfixMapper)
        {
			if (val is Dictionary<object,object>)
            {
				bool usePostfix = postfixMapper != string.Empty;
				var valp = (Dictionary<object, object>)val;
				//map struct: chmp[PAIR1, PAIR2..]
#if ISDLL
				if (usePostfix)
					sb.Append("[");
				else
					sb.Append("createHashMapFromArray[");
#else
				if (usePostfix)
					sb.Append("[\n");
				else
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
					convertDataToGame(sb,kp.Key, ref postfixMapper);
					sb.Append(",");
					convertDataToGame(sb, kp.Value, ref postfixMapper);
					sb.Append("]");

					iter++;
				}
				sb.Append("]");
				if (usePostfix) sb.Append(postfixMapper);
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
					convertDataToGame(sb, el,ref postfixMapper);
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
					sb.Append("\"");
					sb.Append(valp.Replace("\"","\"\""));
					sb.Append("\"");
                }
            }
			else if (val == null)
			{
				sb.Append("null");
			}
			else
            {
				throw new Exception($"unknown val type: {val.GetType()}");
            }
        }
	}
}
