using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class AppConfigTest
	{
		[Test]
		public void TestOutputDirectory()
		{
			Assert.That(@"..\..\UXmlFiles", Is.EqualTo(AppConfig.Shared.OutputDirectory));
		}

		[Test]
		public void TestExchangeRateBaseUrl()
		{
			Assert.That("https://www.customs.go.jp/english/kawase/index_e.htm", Is.EqualTo(AppConfig.ExchangeRate.Url));
		}

		[Test]
		public void TestExchangeRatePdfFileUrlPrefix()
		{
			Assert.That("https://www.customs.go.jp/english/kawase/", Is.EqualTo(AppConfig.ExchangeRate.PdfFileUrlPrefix));
		}

		[Test]
		public void TestNaccsBaseUrl()
		{
			Assert.That("https://bbs.naccscenter.com", Is.EqualTo(AppConfig.NACCS.BaseUrl));
		}

		[Test]
		public void TestNaccsCodeListsBaseUrl()
		{
			Assert.That("https://bbs.naccscenter.com/naccs/dfw/web/system/code/", Is.EqualTo(AppConfig.NACCS.CodeLists.BaseUrl));
		}

		[Test]
		public void TestNaccsCodeListsExportApprovalCertificateCsvFileDownloadUrl()
		{
			Assert.That("https://bbs.naccscenter.com/naccs/dfw/web/data/code/naccs/yushutusho1.csv", Is.EqualTo(AppConfig.NACCS.CodeLists.ExportApprovalCertificateCsvFileDownloadUrl));
		}

		[Test]
		public void TestNaccsCodeListsJPNACCSTariffNeedParallel()
		{
			Assert.That("True", Is.EqualTo(AppConfig.NACCS.CodeLists.JPNACCSTariffNeedParallel));
		}

		[Test]
		public void TestJPNACCS98TariffDownloadUrl()
		{
			Assert.That("https://bbs.naccscenter.com/naccs/dfw/web/data/code/naccs/hcode-98.csv", Is.EqualTo(AppConfig.NACCS.CodeLists.JPNACCS98TariffDownloadUrl));
		}

		[Test]
		public void TestJPNACCS99TariffDownloadUrl()
		{
			Assert.That("https://bbs.naccscenter.com/naccs/dfw/web/data/code/naccs/shogaku1.csv", Is.EqualTo(AppConfig.NACCS.CodeLists.JPNACCS99TariffDownloadUrl));
		}
	}
}
