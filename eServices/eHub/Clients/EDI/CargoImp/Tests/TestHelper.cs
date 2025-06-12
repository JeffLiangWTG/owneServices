using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Clients.EDI.Schemas.CargoImp.Tests
{
	public class TestHelper
	{
		public static void VerifyFlatFile2XMLWithSchema(string inputFlatFilePath, string expectedOutputXMLFileResourceName, string schemaFilePath)
		{
			AssertFileExist(schemaFilePath);
			AssertFileExist(inputFlatFilePath);
			string actualOutputXMLFilePath = Path.GetTempPath() + Guid.NewGuid() + ".xml";

			try
			{
				string errorMsgs = string.Empty;
				errorMsgs = CallFFDasmToConvertFlatFileToXML(inputFlatFilePath, actualOutputXMLFilePath, schemaFilePath);

				if (errorMsgs.Length > 0)
				{
					Assert.Fail(errorMsgs);
				}

				AssertFileExist(actualOutputXMLFilePath);

				AssertFileContentSameAsExpected(actualOutputXMLFilePath, expectedOutputXMLFileResourceName);
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
				if (read != buffer.Length) break;
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

		public static void AssertFileContentSameAsExpected(string actualFilePath, string expectedResourceName)
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

		public static string CallFFDasmToConvertFlatFileToXML(string flatFilePath, string outputXMLFilePath, string schemaFilePath)
		{
			var errorMsgs = new StringBuilder();
			string installedFFDasmPath;

			if (TryGetFFDasmTool(out installedFFDasmPath))
			{
				try
				{
					ProcessStartInfo startInfo = new ProcessStartInfo(installedFFDasmPath);
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

		public static bool TryGetFFDasmTool(out string installedFFDasmPath)
		{
            installedFFDasmPath = Path.Combine(Environment.GetEnvironmentVariable("BTSINSTALLPATH"), "SDK\\Utilities\\PipelineTools\\FFDasm.exe");
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
