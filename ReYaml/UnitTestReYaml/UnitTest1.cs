using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.IO;
using ReYaml;

namespace UnitTestReYaml
{
	[TestClass]
	public class UnitTest1
	{
		private static StringBuilder getbuffer()
        {
			return (StringBuilder)typeof(CommandProcessor).GetField("partialBuffer", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(0);
		}


		private void test_buffer_size(int size)
        {
			Assert.AreEqual(0, getbuffer().Length, "Buffer must be empty");

			int outSize = size;
			var output = new StringBuilder();
			var textData = File.ReadAllText("..\\..\\test.yml");
			var parsedData = File.ReadAllText("..\\..\\parsed.txt");
			CommandProcessor.ParseCommand(output, outSize, "parse_string", new string[] { textData });

			if (output.Length > outSize)
            {
				Assert.AreEqual("$PART$", output.ToString(), $"Unexpected part token for buffsize {size}, outlen: {output.Length}");
            } else
            {
				Assert.AreEqual(parsedData, output.ToString(), $"Result missmatch for buffsize {outSize}");
				return;
            }

			string finalOut = "";
			StringBuilder comDat = new StringBuilder();
			do
			{
				comDat.Clear();
				CommandProcessor.ParseCommand(comDat, outSize, "has_parts", new string[] { });
				if (comDat.ToString() == "True")
				{
					comDat.Clear();
					var bufflen = getbuffer().Length;
					CommandProcessor.ParseCommand(comDat, outSize, "next_read", new string[] { });
					finalOut += comDat;
					if (bufflen < outSize)
					{
						Assert.AreEqual(0, getbuffer().Length);
					}
					else
					{
						Assert.AreEqual(bufflen, getbuffer().Length + outSize);
					}

				}
				else
				{
					Assert.Fail("Empty buffer");
				}

			} while (getbuffer().Length > 0);

			Assert.AreEqual(parsedData, finalOut, $"Result missmatch for buffsize {outSize}");
		}

		[TestMethod]
		public void PartialParsingTest()
		{
			var textData = File.ReadAllText("..\\..\\test.yml");
			for (int i = 10; i < textData.Length + 10; i++)
            {
				test_buffer_size(i);
            }

		}
	}
}
