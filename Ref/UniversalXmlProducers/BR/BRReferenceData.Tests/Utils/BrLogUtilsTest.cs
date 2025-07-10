using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class BrLogUtilsTest
	{
		[Test]
		public void TestAddAndGetValue()
		{
			BrLogUtils.Instance.AddLog(Constants.BRNcmXlsLastUpdateDate, "10/10/2021");

			Assert.AreEqual("10/10/2021", BrLogUtils.Instance.GetKeyValueAsString(Constants.BRNcmXlsLastUpdateDate));
		}

		[Test]
		public void TestLogFileCreation()
		{
			BrLogUtils.Instance.AddLog("Test", "Test_value");

			Assert.IsTrue(File.Exists(BrLogUtils.Instance.LogFilePath), $"The test was unable to create the Log file on the path: {BrLogUtils.Instance.LogFilePath}");
		}

		[SetUp]
		[TearDown]
		public void DeleteLog()
		{
			File.Delete(BrLogUtils.Instance.LogFilePath);
		}
	}
}
