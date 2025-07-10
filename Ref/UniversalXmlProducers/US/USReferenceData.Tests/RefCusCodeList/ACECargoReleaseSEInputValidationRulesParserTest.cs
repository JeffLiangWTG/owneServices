using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class ACECargoReleaseSEInputValidationRulesParserTest
	{
		[Test]
		public void DownloadAndConvertCodesToXMLFile()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.CargoReleaseConditionCodesMarch92020.pdf"))
			{
				var resultXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.TestOutputFile.xml");
				var clientMock = new Mock<IDownLoadService>();
				clientMock.Setup(x => x.DownloadFile(string.Empty, ApplicationConfig.Instance.CustomsBorderProtectionGoverment, DownloadFilePath)).Returns(true);
				SaveToFileOfDownloadPath(webServiceMockStream);

				var codesParser = (ACECargoReleaseSEInputValidationRulesParser)Activator.CreateInstance(typeof(ACECargoReleaseSEInputValidationRulesParser), clientMock.Object, new DateTime(2020, 03, 11, 00, 00, 00));
				codesParser.DownloadAndConvertCodesToXMLFile(TestOutputFilePath);
				Assert.That(resultXML, Is.EqualTo(File.ReadAllText(Path.Combine(TestOutputFilePath, ExpectedTestFileName))));
			}
		}

		[Test]
		public void FileDownloadFailed()
		{
			var clientMock = new Mock<IDownLoadService>();
			clientMock.Setup(x => x.DownloadFile(string.Empty, string.Empty, DownloadFilePath)).Returns(false);

			var codesParser = (ACECargoReleaseSEInputValidationRulesParser)Activator.CreateInstance(typeof(ACECargoReleaseSEInputValidationRulesParser), clientMock.Object, new DateTime(2020, 02, 20, 20, 20, 20));
			var error = codesParser.DownloadAndConvertCodesToXMLFile(null);
			Assert.That(error, Does.Contain($"Unable to download file from the website. Processing failed."));
		}

		void SaveToFileOfDownloadPath(Stream webServiceMockStream)
		{
			byte[] srcBuf = new byte[webServiceMockStream.Length];
			webServiceMockStream.Read(srcBuf, 0, srcBuf.Length);
			webServiceMockStream.Seek(0, SeekOrigin.Begin);
			using (var fs = new FileStream(DownloadFilePath, FileMode.Create, FileAccess.Write))
			{
				fs.Write(srcBuf, 0, srcBuf.Length);
				fs.Close();
			}
		}

		string DownloadFilePath => Path.Combine(Path.GetTempPath(), "Cargo Release Condition Codes.pdf");

		string TestOutputFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"RefCusCodeList\TestFiles");

		protected virtual string ExpectedTestFileName => "Cargo_Release_Condition_Codes.xml";

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}
}
