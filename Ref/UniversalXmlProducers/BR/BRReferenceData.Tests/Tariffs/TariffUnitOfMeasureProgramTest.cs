using System.IO;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	public class TariffUnitOfMeasureProgramTest
	{
		[Test]
		public void TestGenerationWhenLogDateIsNull()
		{
			Assert.IsTrue(HSNTariffUOMProgram.IsGenerateXMLFile("15/10/2021"));
		}

		[Test]
		public void TestWhenLogDateIsEquals()
		{
			BrLogUtils.Instance.AddLog(Constants.BRNcmXlsLastUpdateDate, "10/10/2021");
			Assert.IsFalse(HSNTariffUOMProgram.IsGenerateXMLFile(BrLogUtils.Instance.GetKeyValueAsString(Constants.BRNcmXlsLastUpdateDate)));
		}

		[Test]
		public void TestWhenLogDateIsNotEquals()
		{
			BrLogUtils.Instance.AddLog(Constants.BRNcmXlsLastUpdateDate, "10/10/2021");
			Assert.IsTrue(HSNTariffUOMProgram.IsGenerateXMLFile("20/10/2021"));
		}

		[SetUp]
		[TearDown]
		public void DeleteLogFile()
		{
			File.Delete(BrLogUtils.Instance.LogFilePath);
		}
	}
}
