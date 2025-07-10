using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.AU.CMR;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ReferenceFileDownloaderTest : TestCaseWithFactory
	{
		[TestDate(2005, 08, 03)]
		public void TestDownloadMainFile()
		{
			var referenceFileDownloader = new ReferenceFileDownloader(Factory);
			ReferenceFile mainFile = referenceFileDownloader.DownloadMainFile();

			Assert("Not empty", mainFile.Data != ZBlob.Empty);
			ZString mainFileString = mainFile.Data.ToAscii();
			AssertEquals("P1-MAIN.tar.gz-2005-08-03", mainFileString);
		}

		[TestDate(2005, 08, 03)]
		public void TestDownloadLatestChangeFile()
		{
			var referenceFileDownloader = new ReferenceFileDownloader(Factory);
			ReferenceFile changeFile = referenceFileDownloader.DownloadLatestChangeFile();

			Assert("Not empty", changeFile.Data != ZBlob.Empty);
			ZString changeFileString = changeFile.Data.ToAscii();
			AssertEquals("P1-CHNG.tar.gz-2005-08-03", changeFileString);
		}

		[TestDate(2005, 08, 03)]
		public void TestDownloadChangeFile()
		{
			var downloader = new ReferenceFileDownloader(Factory);
			ReferenceFile changeFile1 = downloader.DownloadChangeFile(1);

			Assert("Not empty", changeFile1.Data != ZBlob.Empty);
			ZString changeFile1String = changeFile1.Data.ToAscii();
			AssertEquals("P1-CHNG.tar.gz-2005-08-02", changeFile1String);
		}

		[TestDate(2005, 08, 03)]
		public void TestDownloadTestingMainFile()
		{
			var referenceFileDownloader = new ReferenceFileDownloader(Factory);
			ReferenceFile mainFile = referenceFileDownloader.DownloadTestingMainFile();

			Assert("Not empty", mainFile.Data != ZBlob.Empty);
			ZString mainFileString = mainFile.Data.ToAscii();
			AssertEquals("Q1-MAIN.tar.gz-2005-08-03", mainFileString);
			AssertEquals("main-testing.tar.gz", mainFile.Name);
		}

		[TestDate(2005, 08, 03)]
		public void TestDownloadTestingLatestChangeFile()
		{
			var referenceFileDownloader = new ReferenceFileDownloader(Factory);
			ReferenceFile changeFile = referenceFileDownloader.DownloadTestingLatestChangeFile();

			Assert("Not empty", changeFile.Data != ZBlob.Empty);
			ZString changeFileString = changeFile.Data.ToAscii();
			AssertEquals("Q1-CHNG.tar.gz-2005-08-03", changeFileString);
			AssertEquals("change-testing.tar.gz", changeFile.Name);
		}

		[ExpectException(typeof(ReferenceFileDownloaderException))]
		public void TestDownloadChangeFileNumberTooBig()
		{
			var downloader = new ReferenceFileDownloader(Factory);
			downloader.DownloadChangeFile(6);
		}

		[ExpectException(typeof(ReferenceFileDownloaderException))]
		public void TestDownloadChangeFileNumberTooSmall()
		{
			var downloader = new ReferenceFileDownloader(Factory);
			downloader.DownloadChangeFile(0);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var configType = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfigType>();
			configType.ZRT_ConfigCode = "AURefURL";
			configType.ZRT_Description = "AU Customs Reference data URL";
			configType.ZRT_LongDescription = "URL root address to source CMR reference files for Australian Customs.";

			var config = Factory.New<Enterprise.Integration.Customs.Shared.IRefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode;
			config.ZRC_StringValue = "https://www.ccf.border.gov.au/reference";
			config.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			config.ZRC_EndDate = new ZDateTime(2079, 01, 01);

			Factory.Save();
		}
	}
}
