using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class ExchangeRateUpdateInfoTest
	{
		class ExchangeRateUpdateInfoForTest : ExchangeRateUpdateInfo
		{
			public ExchangeRateUpdateInfoForTest(updateInfoBean info, string filename) : base(info, filename) { }

			protected override IWebClient GetWebClientWrapper()
			{
				var mock = new Mock<IWebClient>();
				var responseBytes = Convert.FromBase64String("5bmj5YilCeW5tAnmnIgJ5pesCeiyt+mAsgkJ6LOj5Ye6DQpBUlMJMTA5CTAzCTIJMC40NyAgICAJMC40OCAgICANClRXRAkxMDkJMDMJMgkxLjAgICAgIAkxLjAgICAgIA0K");
				mock.Setup(x => x.DownloadData(It.IsAny<string>())).Returns(responseBytes);
				return mock.Object;
			}
		}

		class ExchangeRateUpdateInfoForFallbackTest : ExchangeRateUpdateInfo
		{
			public ExchangeRateUpdateInfoForFallbackTest(updateInfoBean info, string filename) : base(info, filename) { }

			protected override IWebClient GetWebClientWrapper()
			{
				var mock = new Mock<IWebClient>();
				var responseBytesTxt = Convert.FromBase64String("5bmj5YilCeW5tAnmnIgJ5pesCeiyt+mAsgkJ6LOj5Ye6DQpBUlMJMTA5CTAzCTIJMC40NyAgICAJMC40OCAgICANClRXRAkxMDkJMDMJMgkxLjAgICAgIAkxLjAgICAgIA0K");
				mock.Setup(x => x.DownloadData("GetFromTXT")).Returns(responseBytesTxt);
				mock.Setup(x => x.DownloadData("GetFromTXTFail")).Throws(new System.Exception());
				var responseBytesJson = Convert.FromBase64String("ew0KCSJzdGFydCI6IjIwMjAtMDMtMTEiLA0KCSJlbmQiOiIyMDIwLTAzLTIwIiwNCgkiaXRlbXMiOlsNCgkJew0KCQkJImNvZGUiOiJBUlMiLA0KCQkJImJ1eVZhbHVlIjoiMC40OSIsDQoJCQkic2VsbFZhbHVlIjoiMC41Ig0KCQl9LHsNCgkJCSJjb2RlIjoiVFdEIiwNCgkJCSJidXlWYWx1ZSI6IjEiLA0KCQkJInNlbGxWYWx1ZSI6IjEiDQoJCX0NCgldDQp9");
				mock.Setup(x => x.DownloadData("GetFromJSON")).Returns(responseBytesJson);
				return mock.Object;
			}
		}

		[Test]
		public void TestFallbackToJSON()
		{
			SystemContext.Now = () => new DateTime(2021, 11, 30, 10, 55, 15);
			var fileName = Path.Combine(Utility.TempDirectory, string.Format("{0}.xml", Guid.NewGuid().ToString()));
			var info = new updateInfoBean();
			info.version = "2018.07.16";
			info.downloadURL = string.Join(";", new string[] { "GetFromTXTFail", "GetFromJSON" });
			var updateInfo = new ExchangeRateUpdateInfoForFallbackTest(info, fileName);
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			var path = Path.Combine(FolderHelper.GetBinFolder(), "doc/ExchangeRate/ExchangeRateFromJSON.xml");
			var expectedXml = XDocument.Load(path);
			Assert.AreEqual(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		public void TestExecute()
		{
			var standardErrorPath = @".\StandardError.txt";
			Console.SetError(new StreamWriter(standardErrorPath));

			SystemContext.Now = () => new DateTime(2021, 11, 30, 10, 55, 15);
			var fileName = Path.Combine(Utility.TempDirectory, string.Format("{0}.xml", Guid.NewGuid().ToString()));
			var info = new updateInfoBean();
			info.version = "2018.07.16";
			info.downloadURL = "GetFromTXT";
			var updateInfo = new ExchangeRateUpdateInfoForTest(info, fileName);
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			var path = Path.Combine(FolderHelper.GetBinFolder(), "doc/ExchangeRate/ExchangeRateFromTXT.xml");
			var expectedXml = XDocument.Load(path);
			Assert.AreEqual(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);

			Console.Error.Close();
			var standardErrorReader = new StreamReader(standardErrorPath);
			Assert.IsEmpty(standardErrorReader.ReadToEnd(), "standard Error must be empty.");
			standardErrorReader.Close();
		}

		[Test]
		[SetCulture("en-US")]
		public void TestExecuteWhenUS()
		{
			TestExecute();
		}

		[Test]
		[SetCulture("fr-FR")]
		public void TestExecuteWhenFR()
		{
			TestExecute();
		}

		[Test]
		public void TestPublicationTimeShouldEqualToCurrentTime()
		{
			SystemContext.Now = () => new DateTime(2021, 11, 30, 09, 45, 21);
			var fileName = Path.Combine(Utility.TempDirectory, string.Format("{0}.xml", Guid.NewGuid().ToString()));
			var info = new updateInfoBean();
			info.downloadURL = "GetFromTXT";
			var updateInfo = new ExchangeRateUpdateInfoForTest(info, fileName);
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			Assert.AreEqual("2021-11-30T09:45:21", actualXml.Descendants("PublicationTime").First().Value);
		}
	}
}
