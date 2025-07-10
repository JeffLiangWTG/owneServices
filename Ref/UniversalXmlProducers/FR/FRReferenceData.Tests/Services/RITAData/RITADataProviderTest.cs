using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests
{
	[TestFixture]
	class RITADataProviderTest
	{
		[Test]
		public void DomainsToCheckForUpdate()
		{
			Assert.That(RITADataProvider.DomainsToCheckForUpdate.SequenceEqual(new string[] { RITADataProvider.taxationDomain, RITADataProvider.antiDumpingDomain, RITADataProvider.customsDutiesDomain, RITADataProvider.prohibitionDomain, RITADataProvider.domFeesDomain, RITADataProvider.statisticsDomain }));
		}

		[Test]
		public void GroupedChapters()
		{
			var providerMock = new Mock<RITADataProvider>(new RITADataDownloader());
			var tariff1 = new RefCusTariff() { ZZ1_TariffCode = "0100000001", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff2 = new RefCusTariff() { ZZ1_TariffCode = "0100000002", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff3 = new RefCusTariff() { ZZ1_TariffCode = "0200000001", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff4 = new RefCusTariff() { ZZ1_TariffCode = "0200000002", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff5 = new RefCusTariff() { ZZ1_TariffCode = "0200000003", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff6 = new RefCusTariff() { ZZ1_TariffCode = "0300000001", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff7 = new RefCusTariff() { ZZ1_TariffCode = "0300000002", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff8 = new RefCusTariff() { ZZ1_TariffCode = "0300000003", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff9 = new RefCusTariff() { ZZ1_TariffCode = "0300000004", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff10 = new RefCusTariff() { ZZ1_TariffCode = "0300000005", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff11 = new RefCusTariff() { ZZ1_TariffCode = "0300000006", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff12 = new RefCusTariff() { ZZ1_TariffCode = "0400000001", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff13 = new RefCusTariff() { ZZ1_TariffCode = "0400000002", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var tariff14 = new RefCusTariff() { ZZ1_TariffCode = "0500000001", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };

			var setOfEuTariffs = new RefCusTariff[] { tariff1, tariff2, tariff3, tariff4, tariff5, tariff6, tariff7, tariff8, tariff9, tariff10, tariff11, tariff12, tariff13, tariff14 };
			providerMock.Setup(x => x.GetEUDeclarableTariffs(It.IsAny<DateTime>())).Returns(setOfEuTariffs);
			providerMock.Setup(x => x.GetEUDeclarableTariffsThatDay(It.IsAny<DateTime>())).CallBase();
			var provider = providerMock.Object;
			var result = provider.GetGroupedChapters(DateTime.Today, 10);
			Assert.That(result.Count == 2);
			Assert.That(result[0].SequenceEqual(new string[] { "01", "02", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "90", "91", "92", "93", "94", "95", "96", "97", "98", "99" }));
			Assert.That(result[1].SequenceEqual(new string[] { "03" }));
		}

		[Test]
		public void GetUpdatedOrNewTariffsThatDayIncludesNonDeclarableTariffs()
		{
			var setOfEuTariffs = new RefCusTariff[] { };
			var updatedTariffsAsProvidedByCustoms = new string[] { "9999999999" };

			var providerMock = new Mock<RITADataProvider>(new RITADataDownloader());
			providerMock.Setup(x => x.GetUpdatedFRTariffs(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(updatedTariffsAsProvidedByCustoms);
			providerMock.Setup(x => x.GetEUDeclarableTariffs(It.IsAny<DateTime>())).Returns(setOfEuTariffs);
			providerMock.Setup(x => x.GetEUDeclarableTariffsThatDay(It.IsAny<DateTime>())).CallBase();
			var provider = providerMock.Object;

			var expectedList = new string[]
			{
				"9999999999",
			};
			var actualList = provider.GetUpdatedOrNewFRTariffsThatDay(DateTime.Today);
			Assert.That(actualList.SequenceEqual(expectedList));
		}

		[Test]
		public void GetUpdatedOrNewTariffsThatDayIncludesChildren()
		{
			var setOfEuTariffs = new[]
			{
				new RefCusTariff { ZZ1_TariffCode = "1100000080" },
				new RefCusTariff { ZZ1_TariffCode = "1100000090" },
				new RefCusTariff { ZZ1_TariffCode = "2211000080" },
				new RefCusTariff { ZZ1_TariffCode = "2211000090" },
				new RefCusTariff { ZZ1_TariffCode = "3322110080" },
				new RefCusTariff { ZZ1_TariffCode = "3322110090" },
				new RefCusTariff { ZZ1_TariffCode = "4433221180" },
				new RefCusTariff { ZZ1_TariffCode = "4433221190" },
				new RefCusTariff { ZZ1_TariffCode = "5544332290" },
				new RefCusTariff { ZZ1_TariffCode = "9999999999" }
			};

			var updatedTariffsAsProvidedByCustoms = new string[] { "1100000000", "2211000000", "3322110000", "4433221100", "5544332290" };

			var providerMock = new Mock<RITADataProvider>(new RITADataDownloader());
			providerMock.Setup(x => x.GetUpdatedFRTariffs(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(updatedTariffsAsProvidedByCustoms);
			providerMock.Setup(x => x.GetEUDeclarableTariffs(It.IsAny<DateTime>())).Returns(setOfEuTariffs);
			providerMock.Setup(x => x.GetEUDeclarableTariffsThatDay(It.IsAny<DateTime>())).CallBase();
			var provider = providerMock.Object;

			var expectedList = new string[]
			{
				"1100000080",
				"1100000090",
				"2211000080",
				"2211000090",
				"3322110080",
				"3322110090",
				"4433221180",
				"4433221190",
				"5544332290",
			};

			var actualList = provider.GetUpdatedOrNewFRTariffsThatDay(DateTime.Today);

			Assert.That(Array.TrueForAll(expectedList, x => actualList.Contains(x)));
		}

		[Test]
		public void GetTariffRoot()
		{
			var tariffList = new string[] { "1100000000", "2211000000", "3322110000", "4433221100", "5544332290", "0000000010" };
			var expectedList = new string[] { "11", "2211", "332211", "44332211", "5544332290", "0000000010" };
			var actualList = tariffList.Select(x => RITADataProvider.GetTariffRoot(x)).ToArray();

			Assert.That(actualList.SequenceEqual(expectedList));
		}

		[Test]
		public void GetUpdatedOrNewTariffsThatDay()
		{
			var updatedTariff = new RefCusTariff() { ZZ1_TariffCode = "1111111111", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var notUpdatedTariff = new RefCusTariff() { ZZ1_TariffCode = "2222222222", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };

			var setOfEuTariffs = new RefCusTariff[] { updatedTariff, notUpdatedTariff };

			var providerMock = new Mock<RITADataProvider>(new RITADataDownloader());
			providerMock.Setup(x => x.GetEUDeclarableTariffs(It.IsAny<DateTime>())).Returns(setOfEuTariffs);
			providerMock.Setup(x => x.GetUpdatedFRTariffs(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "1111111111" });
			providerMock.Setup(x => x.GetEUDeclarableTariffsThatDay(It.IsAny<DateTime>())).CallBase();

			var provider = providerMock.Object;

			Assert.That(provider.GetUpdatedOrNewFRTariffsThatDay(DateTime.Today).Length == 1);
			Assert.That(provider.GetUpdatedOrNewFRTariffsThatDay(DateTime.Today).Contains("1111111111"));
		}

		[Test]
		public void GetListOfDeclarableTariffThatDay()
		{
			var tariffMissingDigits = new RefCusTariff() { ZZ1_TariffCode = "0000", ZZ1_StartDate = DateTime.Today.AddDays(-1), ZZ1_EndDate = DateTime.Today.AddDays(1) };
			var tariffOk = new RefCusTariff() { ZZ1_TariffCode = "3333333333", ZZ1_StartDate = DateTime.Today.AddDays(-100), ZZ1_EndDate = DateTime.Today.AddDays(100) };
			var setOfEuTariffs = new RefCusTariff[] { tariffMissingDigits, tariffOk };

			var providerMock = new Mock<RITADataProvider>(new RITADataDownloader());
			providerMock.Setup(x => x.GetEUDeclarableTariffs(It.IsAny<DateTime>())).Returns(setOfEuTariffs);
			providerMock.Setup(x => x.GetEUDeclarableTariffsThatDay(It.IsAny<DateTime>())).CallBase();
			var provider = providerMock.Object;
			var euDeclarableTariffsThatDay = provider.GetEUDeclarableTariffsThatDay(DateTime.Today);

			Assert.That(euDeclarableTariffsThatDay.SequenceEqual(new string[] { "3333333333" }));
		}

		[Test]
		public void BuildRequest()
		{
			var result = RITADataProvider.BuildRequest("0000000000", RITADataProvider.importMeasure);
			Assert.IsTrue(result.Contains("<ltFlux>0</ltFlux>"));
			Assert.IsTrue(result.Contains("<ltNomenclature>0000000000</ltNomenclature>"));
			Assert.IsTrue(result.Contains(" <listeReglementationMesureXml"));

			result = RITADataProvider.BuildRequest("0000000000", RITADataProvider.importCondition);
			Assert.IsTrue(result.Contains("<ltFlux>0</ltFlux>"));
			Assert.IsTrue(result.Contains("<ltNomenclature>0000000000</ltNomenclature>"));
			Assert.IsTrue(result.Contains(" <listeReglementationConditionXml"));

			result = RITADataProvider.BuildRequest("0000000000", RITADataProvider.exportMeasure);
			Assert.IsTrue(result.Contains("<ltFlux>1</ltFlux>"));
			Assert.IsTrue(result.Contains("<ltNomenclature>0000000000</ltNomenclature>"));
			Assert.IsTrue(result.Contains(" <listeReglementationMesureXml"));

			result = RITADataProvider.BuildRequest("0000000000", RITADataProvider.exportCondition);
			Assert.IsTrue(result.Contains("<ltFlux>1</ltFlux>"));
			Assert.IsTrue(result.Contains("<ltNomenclature>0000000000</ltNomenclature>"));
			Assert.IsTrue(result.Contains(" <listeReglementationConditionXml"));

			result = RITADataProvider.BuildRequest("0000000000", RITADataProvider.nomenclature);
			Assert.IsTrue(result.Contains("<nomenclatureRenvoiXml"));
			Assert.IsTrue(result.Contains("<ltChapitre>0000000000</ltChapitre>"));

			result = RITADataProvider.BuildRequest("0000000000", "");
			Assert.IsTrue(string.IsNullOrEmpty(result));
		}

		[Test]
		public void DownloadTariffRegulationReturningFunctionalError()
		{
			var testDownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.DownloadDirectory = testDownloadDirectory;

			try
			{
				var downloaderMock = new Mock<RITADataDownloader>();
				downloaderMock.Setup(x => x.PostRequestAndGetResponse(It.IsAny<string>())).Returns(Task.FromResult("<ltTypeMessage>ERM</ltTypeMessage><ltDescription>BLABLA</ltDescription>"));

				var provider = new RITADataProvider(downloaderMock.Object);
				var result = provider.DownloadFRTariffRegulation("0100000000");
			}
			catch (Exception e)
			{
				Assert.That(e.InnerException.Message == "Downloaded file 0100000000_MI.XML reported a functional error: BLABLA");
			}
		}

		[Test]
		public void DownloadTariffRegulation()
		{
			var testDownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.DownloadDirectory = testDownloadDirectory;

			Assert.IsFalse(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_CE.XML")));
			Assert.IsFalse(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_CI.XML")));
			Assert.IsFalse(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_ME.XML")));
			Assert.IsFalse(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_MI.XML")));

			try
			{
				var downloaderMock = new Mock<RITADataDownloader>();
				downloaderMock.Setup(x => x.PostRequestAndGetResponse(It.IsAny<string>())).Returns(Task.FromResult("FakeContent"));

				var provider = new RITADataProvider(downloaderMock.Object);

				var result = provider.DownloadFRTariffRegulation("0100000000");

				Assert.IsTrue(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_CE.XML")));
				Assert.IsTrue(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_CI.XML")));
				Assert.IsTrue(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_ME.XML")));
				Assert.IsTrue(File.Exists(Path.Combine(testDownloadDirectory, "0100000000_MI.XML")));
			}
			finally
			{
				File.Delete(Path.Combine(testDownloadDirectory, "0100000000_CE.XML"));
				File.Delete(Path.Combine(testDownloadDirectory, "0100000000_CI.XML"));
				File.Delete(Path.Combine(testDownloadDirectory, "0100000000_ME.XML"));
				File.Delete(Path.Combine(testDownloadDirectory, "0100000000_MI.XML"));
			}
		}

		[Test]
		public void GetUpdateWebPage()
		{
			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.Setup(x => x.GetWebPage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("<!DOCTYPE html>\r\n\r\n<html lang=\"fr\">\r\n\r\n<head>\r\n<script type=\"text/javascript\">\r\n\t$(function() {\r\n\t      \r\n\t});\r\n</script>");

			var provider = new RITADataProvider(downloaderMock.Object);
			var result = provider.GetTariffUpdateWebPage(new string[] { "01", "02" }, DateTime.Today.AddDays(-60).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture).Replace(" ", "%2F"), DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture).Replace(" ", "%2F"), RITADataProvider.taxationDomain);
			var expectedContent = "<!DOCTYPE html>\r\n\r\n<html lang=\"fr\">\r\n\r\n<head>\r\n<script type=\"text/javascript\">\r\n\t$(function() {\r\n\t      \r\n\t});\r\n</script>";
			Assert.IsTrue(result.Contains(expectedContent));
		}

		[Test]
		public void GetListOfFRTradeGroups()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");

			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.Setup(x => x.GetFRTradeGroupsWebPage()).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FRTadeGroupWebPage.html")));
			downloaderMock.Setup(x => x.GetFRTradeGroupCountriesWebPage(It.IsAny<string>())).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FR1005TadeGroupCountriesWebPage.html")));
			downloaderMock.Setup(x => x.GetFRCountriesWebPage()).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FRCountriesWebPage.html")));

			var provider = new RITADataProvider(downloaderMock.Object);

			var result = provider.GetFRTradeGroups();
			Assert.AreEqual(59, result.Count());
			Assert.AreEqual("1005", result.Cast<RefCusTradeGroup>().First().ZZA_TradeGroup);
			Assert.AreEqual("Surveillance statistique", result.Cast<RefCusTradeGroup>().First().ZZA_Description);
			Assert.AreEqual("01-01-2005", result.Cast<RefCusTradeGroup>().First().ZZA_StartDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
			Assert.AreEqual(211, result.Cast<RefCusTradeGroup>().First().RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AD", result.Cast<RefCusTradeGroup>().First().RefCusTradeGroupCountries.First().ZZB_RN_NKTradeGroupCountryCode);
			Assert.AreEqual("Andorre", result.Cast<RefCusTradeGroup>().First().RefCusTradeGroupCountries.First().ZZB_Description);
			Assert.AreEqual("01-07-1991", result.Cast<RefCusTradeGroup>().First().RefCusTradeGroupCountries.First().ZZB_StartDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
		}

		[Test]
		public void GetListOfFRConditionTypes()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");

			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.Setup(x => x.GetFRConditionTypesWebPage()).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FRConditionTypesWebPage.html")));

			var provider = new RITADataProvider(downloaderMock.Object);

			var result = provider.GetFRConditionTypes();
			Assert.AreEqual(51, result.Count);
			Assert.AreEqual("AAN", result.Cast<RefCusConditionType>().First().ZX2_ConditionType);
			Assert.AreEqual("Alimentation animale", result.Cast<RefCusConditionType>().First().ZX2_Description);
		}

		[Test]
		public void GetListOfFRCountriesAsTradeGroups()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");

			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.Setup(x => x.GetFRCountriesWebPage()).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FRCountriesWebPage.html")));

			var provider = new RITADataProvider(downloaderMock.Object);
			var result = provider.GetFRCountriesAsTradeGroups();
			Assert.AreEqual(251, result.Count());
			Assert.AreEqual("AD", result.Cast<RefCusTradeGroup>().First().ZZA_TradeGroup);
			Assert.AreEqual(1, result.Cast<RefCusTradeGroup>().First().RefCusTradeGroupCountries.Length);
			Assert.AreEqual("AD", result.Cast<RefCusTradeGroup>().First().RefCusTradeGroupCountries.First().ZZB_RN_NKTradeGroupCountryCode);
		}

		[Test]
		public void GetApplicationTerritories()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");

			var provider = new RITADataProvider(new RITADataDownloader());
			var result = RITADataProvider.GetApplicationTerritories();
			Assert.AreEqual(10, result.Count);
			var codeList = result.Select(x => x.ZZA_TradeGroup).ToList();
			Assert.Contains("DPDOM", codeList);
			Assert.Contains("METRO", codeList);
			Assert.Contains("CONTI", codeList);
			Assert.Contains("CORSE", codeList);
			Assert.Contains("GUADE", codeList);
			Assert.Contains("GUYAN", codeList);
			Assert.Contains("MARTI", codeList);
			Assert.Contains("MAYOT", codeList);
			Assert.Contains("REUNI", codeList);
			Assert.Contains("MGPRE", codeList);
		}
	}
}
