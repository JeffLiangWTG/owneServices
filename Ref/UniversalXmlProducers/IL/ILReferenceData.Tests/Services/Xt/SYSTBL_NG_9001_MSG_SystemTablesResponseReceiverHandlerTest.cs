using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Shared;
using NUnit.Framework;
using IDateTimeProvider = CargoWise.RefDbRepo.Common.Utils.IDateTimeProvider;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Services
{
	[TestFixture]
	sealed class SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerTest
	{
		[Test]
		public void TestPreProcessResponse()
		{
			var handler = new SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerForTest(new DateTimeProvider());
			var body = "<SYSTBL_NG_9001_MSG_SystemTablesResponse xmlns=\"http://malam.com/customs/SystemTables/SYSTBL_NG_9001_MSG_SystemTablesResponse\"><TableAsDataSetTableData><TableData><Row><Column1>Value1</Column1></Row></TableData></TableAsDataSetTableData></SYSTBL_NG_9001_MSG_SystemTablesResponse>";
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(body);
				writer.Flush();
				var result = handler.PreProcessResponseCoreForTest(stream);
				Assert.AreEqual("<SYSTBL_NG_9001_MSG_SystemTablesResponse xmlns=\"http://malam.com/customs/SystemTables/SYSTBL_NG_9001_MSG_SystemTablesResponse\"><TableAsDataSetTableData><TableData><Row><Column1>Value1</Column1></Row></TableData></TableAsDataSetTableData></SYSTBL_NG_9001_MSG_SystemTablesResponse>", result.Replace("\r\n", ""));
			}
		}

		[Test]
		public void TestProcessResponse()
		{
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			var handler = new SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerForTest(new DateTimeProvider());

			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			string message = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableResponse_2012.xml");

			XmlSerializer xmlSerializer = new XmlSerializer(typeof(SYSTBL_NG_9001_MSG_SystemTablesResponse));
			using (var stringReader = new StringReader(message))
			using (var xmlReader = new XmlTextReader(stringReader))
			{
				var response = (SYSTBL_NG_9001_MSG_SystemTablesResponse)xmlSerializer.Deserialize(xmlReader);
				var publicationDate = DateTime.ParseExact("2024-07-18 13:00:00", "yyyy-MM-dd HH:mm:ss", null);
				response.lastmodifiedDate = publicationDate;
				response.lastmodifiedDateSpecified = true;

				using (var stream = new MemoryStream())
				{
					xmlSerializer.Serialize(stream, response);
					stream.Seek(0, SeekOrigin.Begin);
					using (var sr = new StreamReader(stream))
					{
						handler.ProcessResponseCoreForTest(sr.ReadToEnd());
					}
				}
			}

			var outputFileName = "IL_FAC.xml";
			var outputFilePath = Path.Combine(ApplicationConfig.Instance.OutputDirectory, outputFileName);
			Assert.True(File.Exists(outputFilePath));
			var actualFileContent = File.ReadAllText(outputFilePath).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var expectedFileContent = TestHelper.GetManifestResourceStream($"CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.{outputFileName}").Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);
		}

		[Test]
		public void TestHandleMessage_TableAsDataSetTableData()
		{
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			var handler = new SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerBase(new DateTimeProvider(), new Logger());
			var message = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.SystemTableResponse_2012_TableAsDataSetTableData_WithSoapEnvelope.xml");
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var metaDataHelper = new MetaDataHelper(new Dictionary<string, string>() { { "refexternal", "1001" } });
			var xTInternalMsgID = 1002;
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms))
			{
				sw.Write(message);
				sw.Flush();

				ms.Position = 0;
				var (success, _) = handler.ProcessMessage(metaDataHelper, ms, xTInternalMsgID);
				Assert.True(success);
			}

			var outputFileName = "IL_FAC.xml";
			var outputFilePath = Path.Combine(ApplicationConfig.Instance.OutputDirectory, outputFileName);
			Assert.True(File.Exists(outputFilePath));
			var actualFileContent = File.ReadAllText(outputFilePath).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			var expectedFileContent = TestHelper.GetManifestResourceStream($"CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.{outputFileName}").Replace("\r\n", string.Empty).Replace("\n", string.Empty);
			Assert.AreEqual(expectedFileContent, actualFileContent);
		}
	}

	public class SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerForTest : SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerBase
	{
		public SYSTBL_NG_9001_MSG_SystemTablesResponseReceiverHandlerForTest(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider, new Logger())
		{
		}

		public string PreProcessResponseCoreForTest(Stream response)
		{
			return PreProcessResponseCore(response);
		}

		public void ProcessResponseCoreForTest(string response)
		{
			ProcessResponseCore(response);
		}

		public (bool Success, string ErrorMessage) ProcessMessageForTest(MetaDataHelper metaDataHelper, Stream payload, long xTInternalMsgID)
		{
			return ProcessMessage(metaDataHelper, payload, xTInternalMsgID);
		}
	}
}
