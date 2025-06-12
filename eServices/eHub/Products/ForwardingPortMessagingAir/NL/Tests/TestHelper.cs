using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Tests
{
	public class TestHelper
	{
		public enum SchemaTestType
		{
			Assemble,
			Disassemble
		}

		public static void VerifyFlatFile2XMLWithSchema(string inputFlatFilePath, string expectedOutputXmlFileResourceName, string schemaFilePath)
		{
			VerifySchema(SchemaTestType.Disassemble, inputFlatFilePath, expectedOutputXmlFileResourceName, schemaFilePath);
		}

		public static void VerifyXML2FlatFileWithSchema(string inputFlatFilePath, string expectedOutputXmlFileResourceName, string schemaFilePath)
		{
			VerifySchema(SchemaTestType.Assemble, inputFlatFilePath, expectedOutputXmlFileResourceName, schemaFilePath);
		}

		static void VerifySchema(SchemaTestType testType, string inputFlatFilePath, string expectedOutputXmlFileResourceName, string schemaFilePath)
		{
			AssertFileExist(schemaFilePath);
			AssertFileExist(inputFlatFilePath);
			var fileType = testType == SchemaTestType.Assemble ? ".txt" : ".xml";
			string actualOutputXMLFilePath = Path.GetTempPath() + Guid.NewGuid() + fileType;

			try
			{
				string errorMsgs = string.Empty;
				errorMsgs = TryConvertSchemaCore(testType, inputFlatFilePath, actualOutputXMLFilePath, schemaFilePath);

				if (errorMsgs.Length > 0)
				{
					Assert.Fail(errorMsgs);
				}

				AssertFileExist(actualOutputXMLFilePath);

				switch (testType)
				{
					case SchemaTestType.Assemble:
						AssertContent_FlatFile(actualOutputXMLFilePath, expectedOutputXmlFileResourceName);
						break;
					case SchemaTestType.Disassemble:
						AssertContent_XML(actualOutputXMLFilePath, expectedOutputXmlFileResourceName);
						break;
					default:
						throw new Exception("Unknown test type");
				}
			}
			finally
			{
				DeleteOutputXMLFile(actualOutputXMLFilePath);
				DeleteOutputXMLFile(inputFlatFilePath);
				DeleteOutputXMLFile(schemaFilePath);
			}
		}

		public static string GetFileWithEmbeddedResource(string resourceName)
		{
			string tempFileName = Path.GetTempFileName();
			using (Stream reader = GetEmbeddedResource(resourceName))
			{
				using (Stream writer = new FileStream(tempFileName, FileMode.Create))
				{
					AddStream(reader, writer);
				}
			}

			return tempFileName;
		}

		public static void AddStream(Stream reader, Stream writer)
		{
			byte[] buffer = new byte[32 * 1024];
			while (true)
			{
				int read = reader.Read(buffer, 0, buffer.Length);
				writer.Write(buffer, 0, read);
				if (read != buffer.Length)
					break;
			}
			writer.Flush();
		}

		public static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		public static string GetResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public static void AssertFileExist(string filePath)
		{
			if (!File.Exists(filePath))
			{
				Assert.Fail("File is not found: " + filePath);
			}
		}

		public static void DeleteOutputXMLFile(string outputXMLFilePath)
		{
			try
			{
				File.Delete(outputXMLFilePath);
			}
			catch { }
		}

		public static void AssertContent_XML(string actualFilePath, string expectedResourceName)
		{
			using (Stream actual = File.OpenRead(actualFilePath))
			{
				using (Stream expected = ResourceHelper.GetEmbeddedResource(Assembly.GetExecutingAssembly(), expectedResourceName))
				{
					MapResult result = (new XmlDiffTool()).Execute(actual, expected);
					string msg = "Below files' contents are not same: \r\nActual file path: " + actualFilePath + "\r\nExpected resource name: " + expectedResourceName;
					//Assert.IsTrue(result.Success, msg);
					MapTester.AssertSuccess(result, actualFilePath, expectedResourceName);
				}
			}
		}

		private static void AssertContent_FlatFile(string actualFilePath, string expectedResourceName)
		{
			using (Stream actual = File.OpenRead(actualFilePath), expected = ResourceHelper.GetEmbeddedResource(Assembly.GetExecutingAssembly(), expectedResourceName))
			{
				using (StreamReader actualReader = new StreamReader(actual), expectedReader = new StreamReader(expected))
				{
					string[] expectedLines = expectedReader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
					string[] actualLines = actualReader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

					Assert.AreEqual<int>(expectedLines.Length, actualLines.Length, string.Format("The actual output has {0} lines while expecting {1} lines.\r\nActua output file: {2}", actualLines.Length, expectedLines.Length, actualFilePath));
					for (int i = 0; i < expectedLines.Length; i++)
					{
						Assert.AreEqual<string>(expectedLines[i], actualLines[i], string.Format("{0}Mismatch On Line <{1}>:{0}Expect: <{2}>{0}Actual: <{3}>{0}", Environment.NewLine, i + 1, expectedLines[i], actualLines[i]));
					}
				}
			}
		}

		public static string TryConvertSchemaCore(SchemaTestType testType, string flatFilePath, string outputXMLFilePath, string schemaFilePath)
		{
			var errorMsgs = new StringBuilder();
			string toolsPath;

			if (TryGetPipelineTool(out toolsPath, testType))
			{
				try
				{
					ProcessStartInfo startInfo = new ProcessStartInfo(toolsPath);
					startInfo.WindowStyle = ProcessWindowStyle.Hidden;
					startInfo.Arguments = flatFilePath + " -bs " + schemaFilePath + " -m " + outputXMLFilePath;

					bool success = false;

					using (Process myProcess = Process.Start(startInfo))
					{
						success = myProcess.WaitForExit(10000); //Wait 10 seconds at the most.
					}

					if (!success)
					{
						errorMsgs.AppendLine("<CallFFDasmToConvertFlatFiletoXML> timed out.");
					}
				}
				catch (Exception e)
				{
					errorMsgs.AppendLine(e.Message);
				}
			}
			else
			{
				errorMsgs.AppendLine("Could not find the BizTalk pipeline tools");
			}

			return errorMsgs.ToString();
		}
		public static bool TryGetPipelineTool(out string installedFFDasmPath, SchemaTestType testType)
		{
			string toolName = string.Empty;
			switch (testType)
			{
				case SchemaTestType.Assemble:
					toolName = "FFAsm.exe";
					break;
				case SchemaTestType.Disassemble:
					toolName = "FFDasm.exe";
					break;
				default:
					throw new Exception("Unknown test type");
			}

			installedFFDasmPath = Path.Combine(Environment.GetEnvironmentVariable("BTSINSTALLPATH"), "SDK\\Utilities\\PipelineTools", toolName);
			return File.Exists(installedFFDasmPath);
		}

		public static string FormattedStringFromList(List<string> list)
		{
			var sb = new StringBuilder();

			foreach (var each in list)
			{
				sb.AppendLine(each);
			}

			return sb.ToString();
		}
	}
}
