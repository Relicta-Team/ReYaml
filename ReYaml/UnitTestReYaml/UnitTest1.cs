using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.IO;
using ReYaml;
using System.Diagnostics;

namespace UnitTestReYaml
{
	[TestClass]
	public class UnitTest1
	{
		private static StringBuilder getbuffer()
        {
			return (StringBuilder)typeof(CommandProcessor).GetField("partialBuffer", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(0);
		}


		private void test_buffer_size(string textData,string parsedData,int size)
        {
			Assert.AreEqual(0, getbuffer().Length, "Buffer must be empty");
			
			var output = new StringBuilder();
			
			CommandProcessor.ParseCommand(output, size, "parse_string", new string[] { textData, "" });

			if (parsedData.Length > size/2)
            {
				Assert.AreEqual("$PART$", output.ToString(), $"Unexpected part token for buffsize {size/2}, outlen: {output.Length}");
            } else
            {
				Assert.AreEqual(parsedData, output.ToString(), $"Result missmatch for buffsize {size/2}; Parsed data len {parsedData.Length}");
				return;
            }

			string finalOut = "";
			StringBuilder comDat = new StringBuilder();
			do
			{
				comDat.Clear();
				CommandProcessor.ParseCommand(comDat, size, "has_parts", new string[] { });
				if (comDat.ToString() == "True")
				{
					comDat.Clear();
					var bufflen = getbuffer().Length;
					CommandProcessor.ParseCommand(comDat, size, "next_read", new string[] { "" });
					finalOut += comDat;
					int midSize = size / 2;
					if (bufflen < midSize)
					{
						Assert.AreEqual(0, getbuffer().Length);
					}
					else
					{
						Assert.AreEqual(bufflen, getbuffer().Length + midSize);
					}

				}
				else
				{
					Assert.Fail("Empty buffer");
				}

			} while (getbuffer().Length > 0);

			Assert.AreEqual(parsedData, finalOut, $"Result missmatch for buffsize {size}; PARSED ({parsedData.Length}) != OUTPUT ({finalOut.Length})");
		}

		[TestMethod]
		public void PartialParsingTest()
		{
			var textData = File.ReadAllText("..\\..\\test.yml",Encoding.UTF8);
			var parsedData = File.ReadAllText("..\\..\\parsed.txt", Encoding.UTF8);

			//removing \r char from parsed file (if contains)
			if (parsedData.Contains("\r"))
			{
				parsedData = parsedData.Replace("\r", "");
			}
			int minSize = 10;
			for (int i = minSize; i < (textData.Length + minSize); i++)
            {
				test_buffer_size(textData,parsedData,i);
            }

		}
	}
}
