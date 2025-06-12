using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.BizTalk.TestTools.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	internal static class TestHelper
	{
		internal static string GetSchemaInTempPath(TestableSchemaBase schema, string filename)
		{
			var schemaFilePath = Path.Combine(Path.GetTempPath(), filename);
			if (!File.Exists(schemaFilePath))
			{
				using (var writer = new StreamWriter(schemaFilePath, false, Encoding.Unicode))
				{
					writer.Write(schema.XmlContent);
				}
			}

			return schemaFilePath;
		}

		internal enum SchemaTestType
		{
			Assemble,
			Disassemble
		}

		internal static bool TestSchema(SchemaTestType testType, string inputResource, string expectOutputResource, string schemaFilePath, params string[] relevantTempFiles)
		{

			var inputTempFilePath = GetFileWithEmbeddedResource(inputResource);
			AssertFileExist(inputTempFilePath);
			AssertFileExist(schemaFilePath);
			string actualOutputPath = Path.GetTempPath() + Guid.NewGuid() + ((testType == SchemaTestType.Assemble)?".txt":".xml");

			try
			{
				string errorMsgs;
				errorMsgs = CallSDKToConvert(inputTempFilePath, actualOutputPath, schemaFilePath, testType);

				if (errorMsgs.Length > 0)
				{
					Assert.Fail(errorMsgs);
				}

				AssertFileExist(actualOutputPath);

				switch (testType)
				{
					case SchemaTestType.Assemble:
						AssertFlatFileContentSameAsExpected(actualOutputPath, expectOutputResource);
						break;
					case SchemaTestType.Disassemble:
						AssertXmlContentSameAsExpected(actualOutputPath, expectOutputResource);
						break;
					default:
						throw new Exception("Unknown test type");
				}
			}
			finally
			{
				DeleteFile(actualOutputPath);
				DeleteFile(inputTempFilePath);
				DeleteFile(schemaFilePath);
				foreach (var tempFile in relevantTempFiles)
				{
					DeleteFile(tempFile);
				}
			}


			return false;
		}

		private static string CallSDKToConvert(string inputPath, string outputPath, string schemaFilePath, SchemaTestType testType)
		{
			var errorMsgs = new StringBuilder();
			string installedFFDasmPath;

			if (TryGetSDKTool(out installedFFDasmPath, testType))
			{
				try
				{
					ProcessStartInfo startInfo = new ProcessStartInfo(installedFFDasmPath);
					startInfo.WorkingDirectory = Path.GetTempPath();
					startInfo.WindowStyle = ProcessWindowStyle.Hidden;
					startInfo.Arguments = inputPath + " -bs " + schemaFilePath + " -m " + outputPath;

					bool success = false;

					using (Process myProcess = Process.Start(startInfo))
					{
						success = myProcess.WaitForExit(10000); //Wait 10 seconds at the most.
					}

					if (!success)
					{
						errorMsgs.AppendLine("<CallSDKToConvertXMLtoFlatFile> timed out.");
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

		private static void AssertXmlContentSameAsExpected(string actualFilePath, string expectedResourceName)
		{
			using (Stream actual = File.OpenRead(actualFilePath))
			{
				using (Stream expected = ResourceHelper.GetEmbeddedResource(Assembly.GetExecutingAssembly(), expectedResourceName))
				{
					MapResult result = (new XmlDiffTool()).Execute(actual, expected);
					string msg = "Below files' contents are not same: \r\nActual file path: " + actualFilePath + "\r\nExpected resource name: " + expectedResourceName;
					MapTester.AssertSuccess(result, actualFilePath, expectedResourceName);
				}
			}
		}

		private static void AssertFlatFileContentSameAsExpected(string actualFilePath, string expectedResourceName)
		{
			using (Stream actual = File.OpenRead(actualFilePath), expected = ResourceHelper.GetEmbeddedResource(Assembly.GetExecutingAssembly(), expectedResourceName))
			{
				using (StreamReader actualReader = new StreamReader(actual), expectedReader = new StreamReader(expected))
				{
					string[] expectedLines = expectedReader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
					string[] actualLines = actualReader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

					Assert.AreEqual<int>(expectedLines.Length, actualLines.Length, string.Format("The actual output has {0} lines while expecting {1} lines.", actualLines.Length, expectedLines.Length));
					for (int i = 0; i < expectedLines.Length; i++)
					{
						Assert.AreEqual<string>(expectedLines[i], actualLines[i], string.Format("{0}Mismatch On Line <{1}>:{0}Expect: <{2}>{0}Actual: <{3}>{0}", Environment.NewLine, i + 1, expectedLines[i], actualLines[i]));
					}
				}
			}
		}

		private static void AssertFileExist(string filePath)
		{
			if (!File.Exists(filePath))
			{
				Assert.Fail("File is not found: " + filePath);
			}
		}

		private static void DeleteFile(string outputXMLFilePath)
		{
			try
			{
				File.Delete(outputXMLFilePath);
			}
			catch { }
		}
        
		private static bool TryGetSDKTool(out string installedSDKPath, SchemaTestType testType)
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

            installedSDKPath = Path.Combine(Environment.GetEnvironmentVariable("BTSINSTALLPATH"), "SDK\\Utilities\\PipelineTools", toolName);
            return File.Exists(installedSDKPath);
        }

		private static string GetFileWithEmbeddedResource(string resourceName)
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

		private static void AddStream(Stream reader, Stream writer)
		{
			byte[] buffer = new byte[32 * 1024];
			while (true)
			{
				int read = reader.Read(buffer, 0, buffer.Length);
				writer.Write(buffer, 0, read);
				if (read != buffer.Length) break;
			}
			writer.Flush();
		}

		private static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

	}
}
