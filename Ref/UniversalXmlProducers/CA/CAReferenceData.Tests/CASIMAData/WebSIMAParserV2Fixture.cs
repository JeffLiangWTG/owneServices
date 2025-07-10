using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CASIMAData
{
	[TestFixture]
	class WebSIMAParserV2Fixture
	{
		const string simaUrl = @"https://www.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/menu-eng.html";
		const string simaBaseUrl = @"https://www.cbsa-asfc.gc.ca";

		[Test]
		public void ParseWindtowersNoExceptionWhenNoAhref()
		{
			var client = new Mock<IWebDriverHelper>();
			client.Setup(x => x.GetWebPage(simaUrl, It.IsAny<int>())).Returns(html18);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/wt-eng.html", It.IsAny<int>())).Returns(html17);

			var parser = new WebSIMAParserV2(client.Object, extractor);

			var results = parser.GetWebSIMAText(simaUrl, simaBaseUrl).ToArray();
			Assert.AreEqual(2, results[0].ClassificationNumbers.Length);
			Assert.AreEqual("October 18, 2023", results[0].DeterminationDate);
			Assert.AreEqual(string.Empty, results[0].CBSAReferenceNumber);

			Assert.AreEqual(new[] { "CN" }, results[0].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[0].Duties[0].DutyType);
			Assert.AreEqual("july 20, 2023", results[0].Duties[0].EffectiveDate);
			Assert.AreEqual("0.428 * VFD", results[0].Duties[0].DutyValue);
		}

		[Test]
		public void ParseSteelgratingHasDifferentTableForRate()
		{
			var client = new Mock<IWebDriverHelper>();
			client.Setup(x => x.GetWebPage(simaUrl, It.IsAny<int>())).Returns(html19);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/sg-eng.html", It.IsAny<int>())).Returns(html31Steelgrating);

			var parser = new WebSIMAParserV2(client.Object, extractor);

			var results = parser.GetWebSIMAText(simaUrl, simaBaseUrl).ToArray();
			Assert.AreEqual(9, results[0].ClassificationNumbers.Length);
			Assert.AreEqual("March 21, 2011", results[0].DeterminationDate);
			Assert.AreEqual("AD1389", results[0].CBSAReferenceNumber);

			Assert.AreEqual(new[] { "CN" }, results[0].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[0].Duties[0].DutyType);
			Assert.AreEqual("2015-07-14", results[0].Duties[0].EffectiveDate);
			Assert.AreEqual("0.85 * VFD", results[0].Duties[0].DutyValue);

			Assert.AreEqual(new[] { "CN" }, results[0].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[0].Duties[1].DutyType);
			Assert.AreEqual("2015-07-14", results[0].Duties[0].EffectiveDate);
			Assert.AreEqual("13064.00 * [TNE]", results[0].Duties[1].DutyValue);
		}

		[Test]
		public void ParseOldFormat()
		{
			var client = new Mock<IWebDriverHelper>();
			client.Setup(x => x.GetWebPage(simaUrl, It.IsAny<int>())).Returns(html1);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/ae-eng.html", It.IsAny<int>())).Returns(html2);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/crs-eng.html", It.IsAny<int>())).Returns(html3);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/cswp1-eng.html", It.IsAny<int>())).Returns(html4);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/hp-eng.html", It.IsAny<int>())).Returns(html5);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/octg4-eng.html", It.IsAny<int>())).Returns(html6);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/uds-eng.html", It.IsAny<int>())).Returns(html7);

			var parser = new WebSIMAParserV2(client.Object, extractor);

			var results = parser.GetWebSIMAText(simaUrl, simaBaseUrl).ToArray();

			//Oil country tubular goods 4 (OCTG4)
			Assert.AreEqual(new[] { "AT" }, results[4].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[4].Duties[0].DutyType);
			Assert.AreEqual("0.91 * VFD", results[4].Duties[0].DutyValue);
			Assert.AreEqual("october 25, 2021", results[4].Duties[0].EffectiveDate);

			//Upholstered domestic seating (China and Vietnam)
			Assert.AreEqual(new[] { "CN" }, results[5].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[5].Duties[0].DutyType);
			Assert.AreEqual("1.88 * VFD", results[5].Duties[0].DutyValue);

			Assert.AreEqual(new[] { "VN" }, results[5].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[5].Duties[1].DutyType);
			Assert.AreEqual("1.795 * VFD", results[5].Duties[1].DutyValue);

			Assert.AreEqual(new[] { "CN" }, results[5].Duties[2].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[5].Duties[2].DutyType);
			Assert.AreEqual("1390.65 * [NMB]", results[5].Duties[2].DutyValue);
			Assert.AreEqual("CNY", results[5].Duties[2].DutyCurrency);

			Assert.AreEqual(new[] { "VN" }, results[5].Duties[3].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[5].Duties[3].DutyType);
			Assert.AreEqual("1914726.79 * [NMB]", results[5].Duties[3].DutyValue);
			Assert.AreEqual("VND", results[5].Duties[3].DutyCurrency);

			//Aluminum Extrusions
			Assert.AreEqual(3, results[0].ClassificationNumbers.Length);
			Assert.AreEqual("February 16, 2009", results[0].DeterminationDate);
			Assert.AreEqual("AD1379", results[0].CBSAReferenceNumber);
			Assert.AreEqual("Aluminum Extrusions", results[0].Description);
			Assert.AreEqual("7604.10.00.30", results[0].ClassificationNumbers[0]);
			Assert.AreEqual("7604.10.00.40", results[0].ClassificationNumbers[1]);
			Assert.AreEqual("7604.21.00.10", results[0].ClassificationNumbers[2]);
			Assert.AreEqual(new[] { "CN" }, results[0].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[0].Duties[0].DutyType);
			Assert.AreEqual("2012-02-20", results[0].Duties[0].EffectiveDate);
			Assert.AreEqual("1.01 * VFD", results[0].Duties[0].DutyValue);
			Assert.AreEqual(new[] { "CN" }, results[0].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[0].Duties[1].DutyType);
			Assert.AreEqual("2012-02-20", results[0].Duties[1].EffectiveDate);
			Assert.AreEqual("15.84 * [KGM]", results[0].Duties[1].DutyValue);

			//Cold-rolled Steel
			Assert.AreEqual(3, results[1].ClassificationNumbers.Length);
			Assert.AreEqual("7209.15.00.00", results[1].ClassificationNumbers[0]);
			Assert.AreEqual("7209.16.00.40", results[1].ClassificationNumbers[1]);
			Assert.AreEqual("7209.16.00.90", results[1].ClassificationNumbers[2]);
			Assert.AreEqual("October 31, 2018", results[1].DeterminationDate);
			Assert.AreEqual("CRS2018", results[1].CBSAReferenceNumber);
			Assert.AreEqual("Cold-rolled Steel", results[1].Description);

			Assert.AreEqual(new[] { "CN" }, results[1].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[1].Duties[0].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[0].EffectiveDate);
			Assert.AreEqual("0.919 * VFD", results[1].Duties[0].DutyValue);

			Assert.AreEqual(new[] { "KR" }, results[1].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[1].Duties[1].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[1].EffectiveDate);
			Assert.AreEqual("0.53 * VFD", results[1].Duties[1].DutyValue);

			Assert.AreEqual(new[] { "VN" }, results[1].Duties[2].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[1].Duties[2].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[2].EffectiveDate);
			Assert.AreEqual("0.992 * VFD", results[1].Duties[2].DutyValue);

			Assert.AreEqual(new[] { "CN" }, results[1].Duties[3].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[1].Duties[3].DutyType);
			Assert.AreEqual("CNY", results[1].Duties[3].DutyCurrency);
			Assert.AreEqual("December 22, 2018", results[1].Duties[3].EffectiveDate);
			Assert.AreEqual("506 * [TNE]", results[1].Duties[3].DutyValue);

			Assert.AreEqual(new[] { "KR" }, results[1].Duties[4].CountryOfOriginOrExport);
			Assert.AreEqual("KRW", results[1].Duties[4].DutyCurrency);
			Assert.AreEqual("CVD", results[1].Duties[4].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[4].EffectiveDate);
			Assert.AreEqual("86733 * [TNE]", results[1].Duties[4].DutyValue);

			Assert.AreEqual(new[] { "VN" }, results[1].Duties[5].CountryOfOriginOrExport);
			Assert.AreEqual("VFD", results[1].Duties[5].DutyCurrency);
			Assert.AreEqual("CVD", results[1].Duties[5].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[5].EffectiveDate);
			Assert.AreEqual("2607988 * [TNE]", results[1].Duties[5].DutyValue);

			// Carbon Steel Welded Pipe (CSWP1)
			Assert.AreEqual(3, results[2].ClassificationNumbers.Length);
			Assert.AreEqual(new[] { "CN" }, results[2].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[2].Duties[0].DutyType);
			Assert.AreEqual("2011-02-14", results[2].Duties[0].EffectiveDate);
			Assert.AreEqual("1.79 * VFD", results[2].Duties[0].DutyValue);
			Assert.AreEqual(new[] { "CN" }, results[2].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[2].Duties[1].DutyType);
			Assert.AreEqual("2011-02-14", results[2].Duties[1].EffectiveDate);
			Assert.AreEqual("5280 * [TNE]", results[2].Duties[1].DutyValue);
			Assert.AreEqual("CNY", results[2].Duties[1].DutyCurrency);

			//Heavy plate
			Assert.AreEqual(7, results[3].ClassificationNumbers.Length);
			Assert.AreEqual("January 7, 2021", results[3].DeterminationDate);
			Assert.AreEqual("HP2020", results[3].CBSAReferenceNumber);
			Assert.AreEqual("Heavy Plate", results[3].Description);
			Assert.AreEqual("7208.51.00.10", results[3].ClassificationNumbers[0]);
			Assert.AreEqual("7208.51.00.93", results[3].ClassificationNumbers[1]);
			Assert.AreEqual("7208.51.00.94", results[3].ClassificationNumbers[2]);
			Assert.AreEqual("7208.51.00.95", results[3].ClassificationNumbers[3]);
			Assert.AreEqual("7208.52.00.10", results[3].ClassificationNumbers[4]);
			Assert.AreEqual("7208.52.00.93", results[3].ClassificationNumbers[5]);
			Assert.AreEqual("7208.52.00.96", results[3].ClassificationNumbers[6]);
			Assert.AreEqual(3, results[3].Duties.Length);
			Assert.AreEqual(new[] { "TP" }, results[3].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[3].Duties[0].DutyType);
			Assert.AreEqual("October 9, 2020", results[3].Duties[0].EffectiveDate);
			Assert.AreEqual("0.97 * VFD", results[3].Duties[0].DutyValue);
			Assert.AreEqual(new[] { "DE" }, results[3].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[3].Duties[1].DutyType);
			Assert.AreEqual("October 9, 2020", results[3].Duties[1].EffectiveDate);
			Assert.AreEqual("0.638 * VFD", results[3].Duties[1].DutyValue);
			Assert.AreEqual(new[] { "TR" }, results[3].Duties[2].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[3].Duties[2].DutyType);
			Assert.AreEqual("October 9, 2020", results[3].Duties[2].EffectiveDate);
			Assert.AreEqual("0.258 * VFD", results[3].Duties[2].DutyValue);
		}

		[Test]
		public void ParseMattresses()
		{
			//https://www.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/mat-eng.html
			var client = new Mock<IWebDriverHelper>();
			client.Setup(x => x.GetWebPage(simaUrl, It.IsAny<int>())).Returns(htmlIndexMattresses);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/mat-eng.html", It.IsAny<int>())).Returns(htmlMattresses);

			var parser = new WebSIMAParserV2(client.Object, extractor);
			var results = parser.GetWebSIMAText(simaUrl, simaBaseUrl).ToArray();

			// Mattresses (MAT)
			Assert.AreEqual("October 5, 2022", results[0].DeterminationDate);
			Assert.AreEqual("MAT2022", results[0].CBSAReferenceNumber);
			Assert.AreEqual("Mattresses (mat)", results[0].Description);
			Assert.AreEqual(4, results[0].ClassificationNumbers.Length);
			Assert.AreEqual("9404.21.00.00", results[0].ClassificationNumbers[0]);

			Assert.AreEqual("ADD", results[0].Duties[0].DutyType);
			Assert.AreEqual("CN", results[0].Duties[0].CountryOfOriginOrExport[0], "China");
			Assert.AreEqual("1.466 * VFD", results[0].Duties[0].DutyValue);
			Assert.AreEqual("CVD", results[0].Duties[1].DutyType);
			Assert.AreEqual("CN", results[0].Duties[1].CountryOfOriginOrExport[0], "China");
			Assert.AreEqual("178.61 * [PCE]", results[0].Duties[1].DutyValue);
		}

		[Test]
		public void ParseWholePotatoes()
		{
			var client = new Mock<IWebDriverHelper>();
			client.Setup(x => x.GetWebPage(simaUrl, It.IsAny<int>())).Returns(html81WP);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/wp-eng.html", It.IsAny<int>())).Returns(html15);

			var parser = new WebSIMAParserV2(client.Object, extractor);
			var results = parser.GetWebSIMAText(simaUrl, simaBaseUrl).ToArray();

			// Whole potatoes (POT)
			Assert.AreEqual("March 20, 1986", results[0].DeterminationDate);
			Assert.AreEqual("AD0689", results[0].CBSAReferenceNumber);
			Assert.AreEqual("Whole Potatoes (pot)", results[0].Description);
			Assert.AreEqual(1, results[0].ClassificationNumbers.Length);
			Assert.AreEqual("0701.90.00.20", results[0].ClassificationNumbers[0]);

			Assert.AreEqual("ADD", results[0].Duties[0].DutyType);
			Assert.AreEqual("US", results[0].Duties[0].CountryOfOriginOrExport[0], "united-states");
			Assert.AreEqual("UNDEFINED", results[0].Duties[0].DutyValue);
		}


		[Test]
		public void ParseNewFormat()
		{
			var client = new Mock<IWebDriverHelper>();
			client.Setup(x => x.GetWebPage(simaUrl, It.IsAny<int>())).Returns(html8);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/ae-eng.html", It.IsAny<int>())).Returns(html9);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/crs-eng.html", It.IsAny<int>())).Returns(html10);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/cswp1-eng.html", It.IsAny<int>())).Returns(html11);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/hp-eng.html", It.IsAny<int>())).Returns(html12);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/octg1-eng.html", It.IsAny<int>())).Returns(html13);
			client.Setup(x => x.GetWebPage(simaBaseUrl + "/sima-lmsi/mif-mev/uds-eng.html", It.IsAny<int>())).Returns(html14);

			var parser = new WebSIMAParserV2(client.Object, extractor);

			var results = parser.GetWebSIMAText(simaUrl, simaBaseUrl).ToArray();

			//Aluminum Extrusions
			Assert.AreEqual(7, results[0].ClassificationNumbers.Length);
			Assert.AreEqual("February 16, 2009", results[0].DeterminationDate);
			Assert.AreEqual("AD1379", results[0].CBSAReferenceNumber);
			Assert.AreEqual("Aluminum Extrusions (ae)", results[0].Description);
			Assert.AreEqual("7604.10.00.30", results[0].ClassificationNumbers[0]);
			Assert.AreEqual("7604.10.00.40", results[0].ClassificationNumbers[1]);
			Assert.AreEqual("7604.29.00.11", results[0].ClassificationNumbers[2]);
			Assert.AreEqual("7604.29.00.19", results[0].ClassificationNumbers[3]);
			Assert.AreEqual("7608.10.00.90", results[0].ClassificationNumbers[4]);
			Assert.AreEqual("7608.20.00.00", results[0].ClassificationNumbers[5]);
			Assert.AreEqual("7610.90.90.90", results[0].ClassificationNumbers[6]);
			Assert.AreEqual(new[] { "CN" }, results[0].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[0].Duties[0].DutyType);
			Assert.AreEqual("1.01 * VFD", results[0].Duties[0].DutyValue);
			Assert.AreEqual(new[] { "CN" }, results[0].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[0].Duties[1].DutyType);
			Assert.AreEqual("2012-02-20", results[0].Duties[1].EffectiveDate);
			Assert.AreEqual("15.84 * [KGM]", results[0].Duties[1].DutyValue);

			//Cold-rolled Steel
			Assert.AreEqual(3, results[1].ClassificationNumbers.Length);
			Assert.AreEqual("7209.15.00.00", results[1].ClassificationNumbers[0]);
			Assert.AreEqual("7209.16.00.40", results[1].ClassificationNumbers[1]);
			Assert.AreEqual("7209.16.00.90", results[1].ClassificationNumbers[2]);
			Assert.AreEqual("October 31, 2018", results[1].DeterminationDate);
			Assert.AreEqual("CRS2018", results[1].CBSAReferenceNumber);
			Assert.AreEqual("Cold-rolled Steel (crs)", results[1].Description);

			Assert.AreEqual(new[] { "CN" }, results[1].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[1].Duties[0].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[0].EffectiveDate);
			Assert.AreEqual("0.919 * VFD", results[1].Duties[0].DutyValue);

			Assert.AreEqual(new[] { "KR" }, results[1].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[1].Duties[1].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[1].EffectiveDate);
			Assert.AreEqual("0.53 * VFD", results[1].Duties[1].DutyValue);

			Assert.AreEqual(new[] { "VN" }, results[1].Duties[2].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[1].Duties[2].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[2].EffectiveDate);
			Assert.AreEqual("0.992 * VFD", results[1].Duties[2].DutyValue);

			Assert.AreEqual(new[] { "CN" }, results[1].Duties[3].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[1].Duties[3].DutyType);
			Assert.AreEqual("CNY", results[1].Duties[3].DutyCurrency);
			Assert.AreEqual("December 22, 2018", results[1].Duties[3].EffectiveDate);
			Assert.AreEqual("506 * [TNE]", results[1].Duties[3].DutyValue);

			Assert.AreEqual(new[] { "KR" }, results[1].Duties[4].CountryOfOriginOrExport);
			Assert.AreEqual("KRW", results[1].Duties[4].DutyCurrency);
			Assert.AreEqual("CVD", results[1].Duties[4].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[4].EffectiveDate);
			Assert.AreEqual("86733 * [TNE]", results[1].Duties[4].DutyValue);

			Assert.AreEqual(new[] { "VN" }, results[1].Duties[5].CountryOfOriginOrExport);
			Assert.AreEqual("VFD", results[1].Duties[5].DutyCurrency);
			Assert.AreEqual("CVD", results[1].Duties[5].DutyType);
			Assert.AreEqual("December 22, 2018", results[1].Duties[5].EffectiveDate);
			Assert.AreEqual("2607988 * [TNE]", results[1].Duties[5].DutyValue);

			// Carbon Steel Welded Pipe (CSWP1)
			Assert.AreEqual(24, results[2].ClassificationNumbers.Length);
			Assert.AreEqual(new[] { "CN" }, results[2].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[2].Duties[0].DutyType);
			Assert.AreEqual("2011-02-14", results[2].Duties[0].EffectiveDate);
			Assert.AreEqual("1.79 * VFD", results[2].Duties[0].DutyValue);
			Assert.AreEqual(new[] { "CN" }, results[2].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[2].Duties[1].DutyType);
			Assert.AreEqual("2011-02-14", results[2].Duties[1].EffectiveDate);
			Assert.AreEqual("5280 * [TNE]", results[2].Duties[1].DutyValue);
			Assert.AreEqual("CNY", results[2].Duties[1].DutyCurrency);

			//Heavy plate
			Assert.AreEqual(3, results[3].ClassificationNumbers.Length);
			Assert.AreEqual("January 7, 2021", results[3].DeterminationDate);
			Assert.AreEqual("HP2020", results[3].CBSAReferenceNumber);
			Assert.AreEqual("Heavy Plate (hp)", results[3].Description);
			Assert.AreEqual("7208.51.00.11", results[3].ClassificationNumbers[0]);
			Assert.AreEqual("7208.51.00.12", results[3].ClassificationNumbers[1]);
			Assert.AreEqual("7208.51.00.19", results[3].ClassificationNumbers[2]);
			Assert.AreEqual(2, results[3].Duties.Length);
			Assert.AreEqual(new[] { "TP" }, results[3].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[3].Duties[0].DutyType);
			Assert.AreEqual("0.806 * VFD", results[3].Duties[0].DutyValue);
			Assert.AreEqual(new[] { "DE" }, results[3].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[3].Duties[1].DutyType);
			Assert.AreEqual("0.686 * VFD", results[3].Duties[1].DutyValue);

			//Oil country tubular goods 1 (OCTG1)
			Assert.AreEqual("Oil Country Tubular Goods (octg1)", results[4].Description);
			Assert.AreEqual(new[] { "CN" }, results[4].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[4].Duties[0].DutyType);
			Assert.AreEqual("1.669 * VFD", results[4].Duties[0].DutyValue);
			Assert.AreEqual(new[] { "CN" }, results[4].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[4].Duties[1].DutyType);
			Assert.AreEqual("4070 * [TNE]", results[4].Duties[1].DutyValue);

			//Upholstered domestic seating (China and Vietnam)
			Assert.AreEqual(new[] { "CN" }, results[5].Duties[0].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[5].Duties[0].DutyType);
			Assert.AreEqual("1.88 * VFD", results[5].Duties[0].DutyValue);

			Assert.AreEqual(new[] { "VN" }, results[5].Duties[1].CountryOfOriginOrExport);
			Assert.AreEqual("ADD", results[5].Duties[1].DutyType);
			Assert.AreEqual("1.795 * VFD", results[5].Duties[1].DutyValue);

			Assert.AreEqual(new[] { "CN" }, results[5].Duties[2].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[5].Duties[2].DutyType);
			Assert.AreEqual("1390.65 * [NMB]", results[5].Duties[2].DutyValue);
			Assert.AreEqual("CNY", results[5].Duties[2].DutyCurrency);

			Assert.AreEqual(new[] { "VN" }, results[5].Duties[3].CountryOfOriginOrExport);
			Assert.AreEqual("CVD", results[5].Duties[3].DutyType);
			Assert.AreEqual("1914726.79 * [NMB]", results[5].Duties[3].DutyValue);
			Assert.AreEqual("VND", results[5].Duties[3].DutyCurrency);

			Assert.AreEqual(new DateTime(2023, 08, 08), parser.PublishedDate);
		}


		[SetUp]
		public void Setup()
		{
			var helper = new Mock<IHttpClientHelper>();
			helper.Setup(x => x.GetMatchedEntityCodesAsync("XXX", It.IsAny<string[]>()))
				.Returns<string, string[]>((url, inputs) =>
				{
					if (inputs[0].Contains("china") || inputs[0].Contains("China"))
					{
						return Task.FromResult(new[] { "CN" });
					}
					else if (inputs[0].Contains("Korea"))
					{
						return Task.FromResult(new[] { "KR" });
					}
					else if (inputs[0].Contains("Vietnam"))
					{
						return Task.FromResult(new[] { "VN" });
					}
					else if (inputs[0].Contains("Chinese Taipei") || inputs[0].Contains("Chinese&nbsp;Taipei"))
					{
						return Task.FromResult(new[] { "TP" });
					}
					else if (inputs[0].Contains("Germany"))
					{
						return Task.FromResult(new[] { "DE" });
					}
					else if (inputs[0].Contains("Turkey"))
					{
						return Task.FromResult(new[] { "TR" });
					}
					else if (inputs[0].Contains("united-states"))
					{
						return Task.FromResult(new[] { "US" });
					}
					else if (inputs[0].Contains("Austria") || inputs[0].Contains("austria"))
					{
						return Task.FromResult(new[] { "AT" });
					}
					return Task.FromResult(new string[0]);
				}
			);

			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "renminbi" }))
				.Returns(Task.FromResult(new string[] { "CNY" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "chinese renminbi" }))
				.Returns(Task.FromResult(new string[] { "CNY" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "south korean won" }))
				.Returns(Task.FromResult(new string[] { "KRW" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "southkorean won" }))
				.Returns(Task.FromResult(new string[] { "KRW" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "vietnamese dong" }))
				.Returns(Task.FromResult(new string[] { "VFD" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "kg" }))
				.Returns(Task.FromResult(new string[] { "KGM" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "kilogram" }))
				.Returns(Task.FromResult(new string[] { "KGM" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "metric tonne" }))
				.Returns(Task.FromResult(new string[] { "TNE" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "metric ton" }))
				.Returns(Task.FromResult(new string[] { "TNE" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "piece" }))
				.Returns(Task.FromResult(new string[] { "PCE" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "nmb" }))
				.Returns(Task.FromResult(new string[] { "NMB" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "unit" }))
				.Returns(Task.FromResult(new string[] { "NMB" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "100kg" }))
				.Returns(Task.FromResult(new string[] { "DTN" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "square metre" }))
				.Returns(Task.FromResult(new string[] { "MTK" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "square meter" }))
				.Returns(Task.FromResult(new string[] { "MTK" }));
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "watt" }))
				.Returns(Task.FromResult(new string[] { "WTT" }));


			extractor = new SIMATextExtractor(helper.Object, "XXX");
		}
		SIMATextExtractor extractor;

		#region html1
		const string html1 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">
<!-- MainContentStart -->
<h1 class=""mrgn-tp-md"" id=""wb-cont"">Measures in Force</h1>
<div class=""row"">
	<div class=""col-md-4 col-xs-12 pull-right"">
		<section class=""lnkbx"">
			<h2>Related links</h2>
			<ul>
				<li><a href=""/sima-lmsi/mif-mev/ior-eng.html"">Importer Obligations</a></li>
				<li><a href=""/sima-lmsi/mif-mev/mif-mev-stats-eng.html"">Import Statistics</a></li>
				<li><a href=""/sima-lmsi/mif-mev/hist-eng.html"">Historical listing of SIMA cases</a></li>
				<li><a href=""/sima-lmsi/mos-sdm/menu-eng.html"">Normal Values: Making Representations to the CBSA</a></li>
				<li><a href=""/sima-lmsi/contact-eng.html#report"">Report Duty Evasion</a></li>
			</ul>
		</section>
	</div>
	<section class=""col-md-8 col-xs-12 pull-left"">
		<p>This is a list of goods currently subject to anti-dumping or countervailing measures pursuant to the <i>Special Import Measures Act</i> (SIMA). It is updated as necessary to reflect the current status of duty liability.</p>
		
		<div class=""alert alert-info""> 
			<p><b>Note:</b> If there is any discrepancy between the SIMA finding or order and the information in this Measures in Force list, the finding or order takes precedence.</p>
		</div>
	</section>
</div>

<div id=""wb-auto-1_wrapper"" class=""dataTables_wrapper no-footer""><div class=""top""><div id=""wb-auto-1_filter"" class=""dataTables_filter""><label>Filter items<input type=""search"" class="""" placeholder="""" aria-controls=""wb-auto-1""></label></div><div class=""dataTables_info"" id=""wb-auto-1_info"" role=""status"" aria-live=""polite"">Showing 1 to 40 of 40 entries</div></div><table class=""wb-tables table table-striped wb-init wb-tables-inited dataTable no-footer"" data-wb-tables=""{&quot;columnDefs&quot;:[{&quot;visible&quot;:false,&quot;targets&quot;:2},{&quot;orderable&quot;:false,&quot;targets&quot;:1}],&quot;paging&quot;:false}"" id=""wb-auto-1"" aria-describedby=""wb-auto-1_info"" role=""grid"" style=""width: 1138px;"">
	<colgroup>
		<col class=""col-md-4"">
		<col>
	</colgroup>
	<thead>
		<tr role=""row""><th scope=""col"" class=""sorting_asc"" tabindex=""0"" aria-controls=""wb-auto-1"" rowspan=""1"" colspan=""1"" aria-sort=""ascending"" aria-label=""Case: activate for descending sort"" style=""width: 363px;"">Case<span class=""sorting-cnt""><span class=""sorting-icons""></span></span></th><th scope=""col"" class=""sorting_disabled"" rowspan=""1"" colspan=""1"" aria-label=""Case type"" style=""width: 743px;"">Case type<span class=""sorting-cnt""><span class=""sorting-icons""></span></span></th></tr>
	</thead>
	<tbody>
	<tr role=""row"">
			<td class=""sorting_1""><a href=""/sima-lmsi/mif-mev/ae-eng.html"">Aluminum extrusions</a></td>
			<td>Dumping &amp; subsidy: China</td>
	</tr>
	<tr role=""row"">
			<td class=""sorting_1""><a href=""/sima-lmsi/mif-mev/crs-eng.html"">Cold-rolled Steel</a></td>
			<td>Dumping &amp; subsidy: China, South Korea, Vietnam</td>
			
	</tr>
	<tr role=""row"">
			<td class=""sorting_1""><a href=""/sima-lmsi/mif-mev/cswp1-eng.html"">Carbon steel welded pipe&nbsp;2 (CSWP&nbsp;2)</a></td>
			<td>Dumping: Chinese Taipei, India, Oman, South Korea, Thailand, United Arab Emirates<br>
			Subsidy: India</td>
	</tr>
	<tr role=""row"">
			<td class=""sorting_1""><a href=""/sima-lmsi/mif-mev/hp-eng.html"">Heavy plate</a></td>
			<td>Dumping: Chinese Taipei, Germany, and Turkey</td>
	</tr>
	<tr role=""row"">
		<td class=""wb-label sorting_1""><a href = ""/sima-lmsi/mif-mev/octg4-eng.html""> Oil country tubular goods&nbsp;4 (OCTG4)</a></td>
		<td>Dumping: Austria</td>
	</tr>
	<tr role=""row"">
		<td class=""sorting_1""><a href = ""/sima-lmsi/mif-mev/uds-eng.html""> Upholstered domestic seating (UDS)</a></td>
		<td>Dumping &amp; subsidy: China and Vietnam</td>
	</tr>
	<tr>
		<td><a href=""/sima-lmsi/mif-mev/wp-eng.html"">Whole potatoes (POT)</a></td>
		<td>Dumping: United States</td>
	</tr>
	</tbody>
</table><div class=""bottom""></div><div class=""clear""></div></div>
<div class=""alert alert-info mrgn-tp-lg""> 
	<p><a href=""/sima-lmsi/menu-eng.html"">SIMA Resources, Case Activities</a> to find out more about all of CBSA’s SIMA investigative proceedings.</p>
</div>
<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2019-07-19</time></dd>
</dl>
</main>";
		#endregion

		#region html2
		const string html2 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">
<!-- MainContentStart -->
<h1 class=""mrgn-tp-md"" id=""wb-cont"">Certain Aluminum Extrusions<br>
<span style=""font-weight:normal"">Dumping &amp; Subsidizing (China)</span></h1>
<dl class=""dl-horizontal"">
	<dt>Product Information</dt>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Product Definition</h2>
		<p>The subject goods are defined as:</p>
		<div class=""well"">
			<p class=""mrgn-bttm-0"">""Aluminum extrusions produced via an extrusion process, of alloys having metallic elements falling within the alloy designations published by The Aluminum Association commencing with 1, 2, 3, 5, 6 or 7 (or proprietary or other certifying body equivalents), with the finish being as extruded (mill), mechanical, anodized or painted or otherwise coated, whether or not worked, having a wall thickness greater than 0.5 mm., with a maximum weight per meter of 22 kilograms and a profile or cross-section which fits within a circle having a diameter of 254 mm., originating in/or exported from the People's Republic of China.""</p>
		</div>
	</dd>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Exclusions</h2>
		<ul>
			<li>aluminum extrusions produced from either a 6063 or a 6005 alloy type with a T6 temper designation, in various lengths, with a powder coat finish on both the interior and the exterior surfaces of the extrusion, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard, ""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels"", for use in exterior railing systems;</li>
			<li>aluminum extrusions produced from a 6063 alloy type with a T5 temper designation, having a length of 3.66 m, with a powder coat finish, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard, ""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels"", for use as head rails and bottom rails in fabric window shades and blinds where the fabric has a cross-sectional honeycomb or ""cellular"" construction;</li>
			<li>aluminum extrusions produced from a 6063 alloy type with a T5 temper designation and forming part of the Vario System™ 20, 30, 40, 45 and 60 series line of profiles, or equivalent, having a length of either 4.5 or 5.8 m and a straightness tolerance of +/-1.5 mm or less per 6.0 m of length, for use in those parts of mechanical systems and automated machinery, such as gantry systems and conveyors, where precise linear movement is required;</li>
			<li>aluminum extrusions produced from either a 6063 or a 6463 alloy type, having a length of 3 m, with a hand-applied gold and silver leaf finish, for use as picture frame mouldings;</li>
			<li>aluminum extrusions produced from a 6063 alloy type with either a T5 or a T6 temper designation, having a length of between 20 and 33 ft. (between 6.10 and 10.06 m), with a powder coat finish, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard (""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels""), for use in window frames;</li>
			<li>heat sinks imported under tariff item No. 8473.30.90 and weighing 700 g or less; and</li>
			<li>aluminum extrusions produced by China Square Industrial Ltd. from either a 6063 or a 6463 alloy type with a T5 temper designation, with a profile or cross-section which fits within a circle having a diameter of 100 mm, for use by MAAX Bath Inc. in the assembly of its shower enclosures, specifically identified in the Appendix of the Determination and reasons issued by the Canadian International Trade Tribunal on February 10, 2011, in Inquiry No. NQ-2008-003R. The list of these excluded products can be found at the following link:<a href=""http://www.citt.gc.ca/en/node/6412#_Toc387403752"">http://www.citt.gc.ca/en/node/6412#_Toc387403752</a></li>
		</ul>
		<p>For the excluded products listed above that require a finish which is ""certified to meet the American Architectural Manufacturers Association AAMA 2603 standard"", the importer must be able to provide evidence that their goods meet that standard. </p>
	</dd>
	<dt>Investigation Information</dt>
	<dd>
		<p>The dates of the investigative proceedings and findings concerning this case are:</p>
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date </th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-de-eng.html"">Initiation of Investigation</a></td>
					<td>August 18, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-pd-eng.html"">Preliminary Determination</a></td>
					<td>November 17, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-fd-eng.html"">Final Determination</a></td>
					<td>February 16, 2009</td>
				</tr>
				<tr>
					<td><a href=""http://www.citt.gc.ca/dumping/inquirie/findings/archive_nq2i003_e"">Canadian International Trade Tribunal's Finding</a></td>
					<td>March 17, 2009</td>
				</tr>
				<tr>
					<td><a href=""http://www.citt.gc.ca/en/dumping/inquirie/findings/archive_nq2i003r_e"">Tribunal's Remand Determination</a></td>
					<td>February 10, 2011</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/ad1379/ad1379-ri11-nc-eng.html"">Re-Investigation</a></td>
					<td>February 20, 2012</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/rr2013-003/rr2013-003-e13-de-eng.html"">Expiry Review Determination</a></td>
					<td>October 18, 2013</td>
				</tr>
				<tr>
					<td><a href=""http://www.citt.gc.ca/en/node/6412"">Canadian International Trade Tribunal's Order</a></td>
					<td>March 17, 2014</td>
				</tr>
			</tbody>
		</table>
	</dd>
	<dt>Tariff Classification Numbers</dt>
	<dd>
		<p>The subject goods are usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7604.10.00.30</li>
			<li>7604.10.00.40</li>
			<li>7604.21.00.10</li>
		</ul>
		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
	</dd>
	<dt>Duty Liability<br>
	(Anti-dumping duties)</dt>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Country of Origin or Export: China</h2>
		<p>Effective on imports of subject goods released by the CBSA on or after 2012-02-20:</p>
		<p>Information regarding the normal values of subject goods should be obtained from the exporter. For a list of exporters in China who currently have normal values, please consult the <b><a href=""/sima-lmsi/ri-re/ad1379/ad1379-ri11-nc-eng.html"">CBSA Notice of Conclusion of Re-investigation</a></b>.</p>
		<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own normal values, the anti-dumping duty is equal to 101% of the export price.</p>
	</dd>
	<dt>Duty Liability<br>
	(Countervailing duties)</dt>
	<dd>
		<h2 class=""h5 mrgn-tp-sm"">Country of Origin or Export: China</h2>
		<p>Effective on imports of subject goods released by the CBSA on or after 2012-02-20:</p>
		<p>For a list of exporters in China who currently have a specific amount of subsidy, please consult the <b><a href=""/sima-lmsi/ri-re/ad1379/ad1379-ri11-nc-eng.html"">CBSA Notice of Conclusion of Re-investigation</a></b>.</p>
		<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own amount of subsidy, the countervailing duty is equal to 15.84 Renminbi per kilogram.</p>
	</dd>
	<dt>Disclosure of Normal Values and Amounts of Subsidy </dt>
	<dd>
		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a> and from the finding of the CITT. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act to Importers.</i></p>
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
	</dd>
	<dt>Information Required on Customs Documents</dt>
	<dd>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap/menu-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		<p>The import documentation should clearly indicate the following:</p>
		<ul>
			<li>Confirmation whether the product is subject to anti-dumping and countervailing duty</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description of the goods, including a physical description of the type/shape of the extrusions, alloy, finish, and whether the goods fall within the scope of subject goods:</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (including, the unit of measure)</li>
			<li>Unit selling price, total selling price</li>
			<li>Currency of settlement used (e.g., US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g., FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada from the point of direct shipment (including, the inland and ocean freight, insurance, etc.).</li>
			<li>The amount of any export taxes applicable to the goods.</li>
		</ul>
	</dd>
	<dt>Appeal Decisions Relating to Subjectivity</dt>
	<dd>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
	</dd>
	<dt>Email for Duty Assessment Questions</dt>
	<dd>
		<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>
	</dd>
	<dt>CBSA Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>Dumping file #: 4214-22</li>
			<li>Dumping case #: AD1379</li>
			<li>Subsidy file #: 4218-26</li>
			<li>Subsidy case #: CV124</li>
		</ul>
	</dd>
	<dt>CITT Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>NQ-2008-003</li>
			<li>NQ-2008-003R</li>
			<li>RR-2013-003</li>
		</ul>
	</dd>
</dl>
<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2019-06-28</time></dd>
</dl>
</main>";
		#endregion

		#region html31 
		const string html31Steelgrating = @"
<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Steel grating: Measures in force</h1>
</header>

<p>Dumping&nbsp;and subsidizing (China)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>SG</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
		<div class=""well"">
			<p>""Carbon steel bar grating and alloy steel bar grating, consisting of load-bearing pieces and cross pieces, produced as standard grating or heavy-duty grating, in panel form, whether galvanized, painted, coated, clad or plated, originating in/or exported from the People's Republic of China.""</p>
		</div>

	<h3>Exclusions</h3>
		
		<ul>
			<li>expanded metal grating comprised of a single piece or coil of sheet or thin plate steel that has been slit and expanded and not consisting of welding or joining of multiple pieces of steel; and</li>
			<li>plank-type safety grating comprised of a single piece or coil of sheet or thin plate steel, typically in thickness of 10 to 18 gauge, pierced and cold formed and without welding or joining of multiple pieces of steel.</li>
		</ul>
</section>
<section>
	<h2>Investigation information</h2>
		<p>The dates of the investigative proceedings and findings concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/ad1389/ad1389-i10-de-eng.html"">Initiation of Investigation</a></td>
					<td>September 20, 2010</td>
				</tr>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/ad1389/ad1389-i10-pd-eng.html"">Preliminary Determination</a></td>
					<td>December 20, 2010</td>
				</tr>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/ad1389/ad1389-i10-fd-eng.html"">Final Determination</a></td>
					<td>March 21, 2011</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353488/index.do"">Canadian International Trade Tribunal's Finding</a></td>
					<td>April 19, 2011</td>
				</tr>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/ri-re/ad1389/ad1389-ri15-nc-eng.html"">Re-investigation</a></td>
					<td>July 14, 2015</td>
				</tr>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/er-rre/rr2015-001/rr2015-001-de-eng.html"">Expiry review determination</a></td>
					<td>December 10, 2015</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/354558/index.do"">Canadian International Trade Tribunal's Order </a></td>
					<td>April 18, 2016</td>
				</tr> 
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/er-rre/sg2021/sg2021-de-eng.html"">Expiry review determination</a></td>
					<td><time class=""nowrap"" datetime=""2021-09-09"">September 9, 2021</time></td>
				</tr>
			</tbody> 
		</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
		<p>The subject goods are usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7308.90.00.10</li>
			<li>7308.90.00.20</li>
			<li>7308.90.00.30</li>
			<li>7308.90.00.40</li>
			<li>7308.90.00.50</li>
			<li>7308.90.00.60</li>
			<li>7308.90.00.95</li>
			<li>7308.90.00.96</li>
			<li>7308.90.00.99</li>
		</ul>
		
		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods. For more information on the tariff classification numbers, please refer to the <a href=""https://www.cbsa-asfc.gc.ca/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">harmonized commodity description and coding system</a>.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">Anti-dumping</span> duties)</h2>

	<h3>Country of origin or export: China</h3>
		
		<p>Effective on imports of subject goods released by the CBSA on or after <time class=""nowrap"" datetime=""2015-07-14"">2015-07-14</time>:</p>

		<p>No exporters in China received normal values in the most recent re-investigation.</p>

		<p>The liability for anti-dumping duty results from the proceedings conducted under SIMA and from the CITT finding. Given that no exporters or producers provided a response to the CBSA’s Requests for Information, normal values will therefore be determined by a ministerial specification under SIMA.</p>

		<p>For importations of subject goods originating in/or exported from China, the anti-dumping duty is listed in the table below:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Anti-dumping duty<sup>1</sup></th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>All exporters</td>
					<td class=""text-center"">85.0%</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class=""small"" colspan=""2""><sup>1</sup>Expressed as a percentage of export price</td>
				</tr>
			</tfoot>
		</table>
</section>
<section>
	<h2>Duty liability<br>
	(Countervailing duties)</h2>

	<h3>Country of origin or export: China</h3>
		
		<p>Effective on imports of subject goods released by the CBSA on or after <time class=""nowrap"" datetime=""2015-07-14"">2015-07-14</time>:</p>

		<p>No exporters in China received a specific amount of subsidy in the most recent re-investigation.</p>

		<p>The liability for countervailing duty results from the proceedings conducted under SIMA and from the CITT finding. Given that no exporters or producers provided a response to the CBSA’s Requests for Information, amounts of subsidy will therefore be determined by a ministerial specification under SIMA.</p>

		<p>For importations of subject goods originating in/or exported from China, the countervailing duty is listed in the table below:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Amount of subsidy CNY/TNE</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>All exporters</td>
					<td class=""text-center"">13,064.00</td>
				</tr>
			</tbody>
		</table>
</section>
<section>
	<h2>Disclosure of normal values and amounts of subsidy</h2>
		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a> and from the finding of the CITT. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""https://www.cbsa-asfc.gc.ca/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act to Importers.</i></p>
		
		<p>For information on duty assessment, refer to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information required on customs documents</h2>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li>Confirmation whether the product is subject to anti-dumping duty and/or countervailing duties</li>
			<li>exporter ID</li>
			<li>name and address of producer/manufacturer</li>
			<li>name and address of vendor (if different from the producer)</li>
			<li>customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>full product description of the goods, including:
				<ul>
					<li>Model ID</li>
					<li>Model description</li>
					<li>Product type (i.e. mat, stair, etc.);</li>
					<li>Grade;</li>
					<li>Material (carbon steel, stainless steel, alloy steel);</li>
					<li>Coating;</li>
					<li>Surface Finish (plain, serrated, other (specify));</li>
					<li>Nosing (if applicable)</li>
					<li>Indicate the type of nosing (dimpled, corrugated, abrasive, etc.)</li>
				</ul>
			</li>
			<li>date of sale, date of shipment</li>
			<li>quantity (including, the unit of measure)</li>
			<li>unit selling price, total selling price</li>
			<li>currency of settlement used (e.g., US$, CDN$, etc.)</li>
			<li>terms and conditions of sale (e.g., FOB, CIF, etc.)</li>
			<li>all costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada from the point of direct shipment (including, the inland and ocean freight, insurance, etc.)</li>
			<li>the amount of any Chinese export taxes applicable to the goods.</li>
		</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>
		<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca?subject=Steel%20grating"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>

		<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CITT reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>NQ-2010-002</li>
			<li>RR-2015-001</li>
			<li>RR-2020-005</li>
		</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""https://www.cbsa-asfc.gc.ca/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc@cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&amp;Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/sg-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5 wb-init wb-share-inited"" data-wb-share=""{&quot;lnkClass&quot;: &quot;btn btn-default btn-block&quot;}"" id=""wb-auto-4""><section id=""shr-pg0"" class=""shr-pg mfp-hide modal-dialog modal-content overlay-def""><header class=""modal-header""><h2 class=""modal-title"">Share this page</h2></header><div class=""modal-body""><ul class=""list-unstyled colcount-xs-2""><li><a href=""https://www.blogger.com/blog_this.pyra?t=&amp;u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;n=Steel%20grating%3A%20Measures%20in%20force"" class=""shr-lnk blogger btn btn-default"" rel=""noreferrer noopener"">Blogger</a></li><li><a href=""https://www.diigo.com/post?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;title=Steel%20grating%3A%20Measures%20in%20force"" class=""shr-lnk diigo btn btn-default"" rel=""noreferrer noopener"">Diigo</a></li><li><a href=""mailto:?to=&amp;subject=Steel%20grating%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html%0A"" class=""shr-lnk email btn btn-default"" rel=""noreferrer noopener"">Email</a></li><li><a href=""https://www.facebook.com/sharer.php?u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;t=Steel%20grating%3A%20Measures%20in%20force"" class=""shr-lnk facebook btn btn-default"" rel=""noreferrer noopener"">Facebook</a></li><li><a href=""https://mail.google.com/mail/?view=cm&amp;fs=1&amp;tf=1&amp;to=&amp;su=Steel%20grating%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html%0A"" class=""shr-lnk gmail btn btn-default"" rel=""noreferrer noopener"">Gmail</a></li><li><a href=""https://www.linkedin.com/shareArticle?mini=true&amp;url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;title=Steel%20grating%3A%20Measures%20in%20force&amp;ro=false&amp;summary=&amp;source="" class=""shr-lnk linkedin btn btn-default"" rel=""noreferrer noopener"">LinkedIn®</a></li><li><a href=""https://www.myspace.com/Modules/PostTo/Pages/?u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;t=Steel%20grating%3A%20Measures%20in%20force"" class=""shr-lnk myspace btn btn-default"" rel=""noreferrer noopener"">MySpace</a></li><li><a href=""https://www.pinterest.com/pin/create/button/?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;media=&amp;description=Steel%20grating%3A%20Measures%20in%20force"" class=""shr-lnk pinterest btn btn-default"" rel=""noreferrer noopener"">Pinterest</a></li><li><a href=""https://reddit.com/submit?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;title=Steel%20grating%3A%20Measures%20in%20force"" class=""shr-lnk reddit btn btn-default"" rel=""noreferrer noopener"">reddit</a></li><li><a href=""https://tinyurl.com/create.php?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html"" class=""shr-lnk tinyurl btn btn-default"" rel=""noreferrer noopener"">TinyURL</a></li><li><a href=""https://www.tumblr.com/share/link?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html&amp;name=Steel%20grating%3A%20Measures%20in%20force&amp;description="" class=""shr-lnk tumblr btn btn-default"" rel=""noreferrer noopener"">tumblr</a></li><li><a href=""https://twitter.com/intent/tweet?text=Steel%20grating%3A%20Measures%20in%20force&amp;url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html"" class=""shr-lnk twitter btn btn-default"" rel=""noreferrer noopener"">Twitter</a></li><li><a href=""https://api.whatsapp.com/send?text=Steel%20grating%3A%20Measures%20in%20force%0A%0Ahttps%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html"" class=""shr-lnk whatsapp btn btn-default"" rel=""noreferrer noopener"">Whatsapp</a></li><li><a href=""https://compose.mail.yahoo.com/?to=&amp;subject=Steel%20grating%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fsg-eng.html%0A"" class=""shr-lnk yahoomail btn btn-default"" rel=""noreferrer noopener"">Yahoo! Mail</a></li></ul><p class=""col-sm-12 shr-dscl"">No endorsement of any products or services is expressed or implied.</p><div class=""clearfix""></div></div></section><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/sg-eng.html#shr-pg0"" aria-controls=""shr-pg0"" class=""shr-opn wb-lbx btn btn-default btn-block wb-lbx-inited wb-init"" id=""wb-auto-5""><span class=""glyphicon glyphicon-share""></span>Share this page</a></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2023-07-06</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html3
		const string html3 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">

<!-- MainContentStart -->

<h1 class=""mrgn-tp-md"" id=""wb-cont"">Certain Cold-rolled Steel<br>
<span style=""font-weight:normal"">Dumping &amp; Subsidizing (China, South Korea, Vietnam)</span></h1>

<dl class=""dl-horizontal"">
	<dt>Product Information</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Product Definition</h3>
		
		<p>The subject goods are defined as:</p>
		
		<div class=""well"">
			<p class=""mrgn-bttm-0"">Cold-reduced flat-rolled sheet products of carbon steel (alloy and non-alloy), in coils or cut lengths, in thicknesses up to 0.142 inches (3.61 mm) and widths up to 73 inches (1854 mm) inclusive, originating in or exported from the People’s Republic of China, the Republic of Korea, and the Socialist Republic of Vietnam, and excluding:</p>

			<ol class=""lst-lwr-alph"">
				<li>organic coated (including pre-paint and laminate) and metallic coated steel;</li>
				<li>steel products for use in the manufacture of passenger automobiles, buses, trucks, ambulances or hearses or chassis therefor, or parts thereof, or accessories or parts thereof;</li>
				<li>steel products for use in the manufacture of aeronautic products;</li>
				<li>perforated steel;</li>
				<li>stainless steel;</li>
				<li>silicon electrical steel; and </li>
				<li>tool steel.</li>
			</ol>
		</div>
	</dd>
	
	<dt>Investigations Information</dt>
	<dd>
		<p>The dates of the investigative proceedings and finding concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/crs2018/crs2018-ni-eng.html"">Initiation of Investigations</a></td>
					<td>May 25, 2018</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/crs2018/crs2018-np-eng.html"">Preliminary Determinations</a></td>
					<td>August 23, 2018</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/crs2018/crs2018-nf-eng.html"">Final Determinations</a></td>
					<td>October 31, 2018</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/407833/index.do"">Canadian International Trade Tribunal’s Finding</a></td>
					<td>December 21, 2018</td>
				</tr>
			</tbody> 
		</table>
	</dd>
	
	<dt>Tariff Classification Numbers</dt>
	<dd>
		<p>The goods in question are usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7209.15.00.00</li>
		</ul>

		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7209.16.00.40</li>
			<li>7209.16.00.90</li>
		</ul>

		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
	</dd>
	
	<dt>Duty Liability<br>
	(Anti-dumping Duties &amp; Countervailing Duties)</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Country of Origin or Export: China, South Korea, Vietnam</h3>
	
		<p>Effective on imports of subject goods released by the CBSA on or after December 22, 2018.</p>

		<p>No exporters in China, South Korea or Vietnam received normal values or a specific amount of subsidy following the conclusion of the investigation. For further information please consult the <a href=""/sima-lmsi/i-e/crs2018/crs2018-nf-eng.html"">CBSA Notice of Final Determinations</a>.</p>

		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under SIMA and from the CITT finding. Given that no exporters or producers provided a response to the CBSA’s RFIs, normal values and amount of subsidy will therefore be determined by a ministerial specification under SIMA.</p>

		<p>For importations of subject goods originating in/or exported from China, South Korea or Vietnam, the anti-dumping duty is listed in the table below:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of Origin or Export</th>
					<th class=""text-center"" scope=""col"">Anti-dumping Duty*</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China - All Exporters</td>
					<td class=""text-right"">91.9%</td>
				</tr>
				<tr>
					<td>South Korea - All Exporters</td>
					<td class=""text-right"">53.0%</td>
				</tr>
				<tr>
					<td>Vietnam - All Exporters</td>
					<td class=""text-right"">99.2%</td>
				</tr>
			</tbody>
		</table>
		<p class=""small"">*As a percentage of export price.</p>

		<p>For importations of subject goods originating in/or exported from China, South Korea or Vietnam, the countervailing duty is listed in the table below:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of Origin or Export</th>
					<th class=""text-center"" scope=""col"">Amount of Subsidy per Metric Tonne</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China - All Exporters</td>
					<td class=""text-right"">506 Chinese Renminbi</td>
				</tr>
				<tr>
					<td>South Korea - All Exporters</td>
					<td class=""text-right"">86,733 South Korean Won</td>
				</tr>
				<tr>
					<td>Vietnam - All Exporters</td>
					<td class=""text-right"">2,607,988 Vietnamese Dong</td>
				</tr>
			</tbody>
		</table>
		
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
	</dd>
	
	<dt>Information Required on Customs Documents</dt>
	<dd>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap/menu-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>

		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>		
			<li>Confirmation whether the product is subject to anti-dumping and countervailing duties</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description of the goods, including:
				<ul>
					<li>Product quality (primes, seconds, etc.)</li>
					<li>Product form (coils, sheet, etc.)</li>
					<li>Steel grade</li>
					<li>Width</li>
					<li>Nominal thickness</li>
					<li>Minimum thickness</li>
					<li>Annealing </li>
					<li>Surface (standard, semi-critical, critical, surface exposed, etc.)</li>
					<li>Finishing (rough matte, regular matte, light matte, etc.)</li>
					<li>Packaging </li>
					<li>Tolerance requirement (width and flatness requirements)</li>
					<li>For automotive use (is product designed specifically for automotive manufacturing?)</li>
				</ul>
			</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
			<li>The amount of any export taxes applicable to the goods</li>
		</ul>
	</dd>
	
	<dt>Appeal Decisions Relating to Subjectivity</dt>
	<dd>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
	</dd>
	
	<dt>Email for Duty Assessment Questions</dt>
	<dd>
		<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca?subject=Cold-rolled Steel"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>
	</dd>
	
	<dt>CBSA Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>CRS 2018 IN</li>
		</ul>
	</dd>
	
	<dt>CITT Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>NQ-2018-002</li>
			<li>PI-2018-002</li>
		</ul>
	</dd>
</dl>

<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2019-12-19</time></dd>
</dl>
</main>";
		#endregion

		#region html4
		const string html4 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">

<!-- MainContentStart -->

<h1 class=""mrgn-tp-md"" id=""wb-cont"">Carbon Steel Welded Pipe (CSWP 1)<br>
<span style=""font-weight:normal"">Dumping &amp; Subsidizing (China)</span></h1>

<dl class=""dl-horizontal"">
	<dt>Product Information</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Product Definition</h3>
		
		<p>The subject goods are defined as:</p>
		
		<div class=""well"">
			<p class=""mrgn-bttm-0"">""Carbon steel welded pipe, commonly identified as standard pipe, in the nominal size range of ½&nbsp;inch up to and including 6&nbsp;inches (12.7&nbsp;mm to 168.3&nbsp;mm in outside diameter) inclusive, in various forms and finishes, usually supplied to meet ASTM&nbsp;A53, ASTM&nbsp;A135, ASTM&nbsp;A252, ASTM&nbsp;A589, ASTM&nbsp;A795, ASTM&nbsp;F1083 or Commercial Quality, or AWWA&nbsp;C200-97 or equivalent specifications, including water well casing, piling pipe, sprinkler pipe and fencing pipe, but excluding oil and gas line pipe made to API specifications exclusively, originating in/or exported from the People's Republic of China.""</p>
		</div>
	</dd>
	
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Exclusions</h3>
		
		<ul>
			<li>carbon steel welded pipe in nominal pipe sizes of 1 inch, meeting the requirements of specification ASTM A53, Grade B, Schedule 10, with a black or galvanized finish, and with plain ends, for use in fire protection applications;</li>
			<li>carbon steel welded pipe in nominal pipe sizes of ½ inch to 2 inches inclusive, produced using the electric resistance welding process and meeting the requirements of specification ASTM A53, Grade A, for use in the production of carbon steel pipe nipples; and</li>
			<li>carbon steel welded pipe in nominal pipe sizes of ½ inch to 6 inches inclusive, dual-stencilled to meet the requirements of both specification ASTM A252, Grades 1 to 3, and specification API 5L, with bevelled ends and in random lengths, for use as foundation piles.</li>
		</ul>
	</dd>
	
	<dt>Investigation Information</dt>
	<dd>
		<p>The dates of the investigative proceedings and findings concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date </th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Initiation of Investigation </td>
					<td>January 23, 2008</td>
				</tr>
				<tr>
					<td>Preliminary Determination</td>
					<td>April 22, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1373/ad1373-i08-fd-eng.html"">Final Determination</a></td>
					<td>July 21, 2008</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353467/index.do"">Canadian International Trade Tribunal's Finding</a></td>
					<td>August 20, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/ad1373/ad1373-ri10-nc-eng.html"">Re-Investigation</a></td>
					<td>February 14, 2011</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/rr2012-003/rr2012-003-e12-de-eng.html"">Expiry Review Determination</a></td>
					<td>April 4, 2013</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353820/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>August 19, 2013</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/cswp12018/cswp12018-de-eng.html"">Expiry Review Determination</a></td>
					<td>November 2, 2018</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/417109/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>March 28, 2019</td>
				</tr>
			</tbody>
		</table>
	</dd>
	
	<dt>Tariff Classification Numbers</dt>
	<dd>
		<p>The subject goods are usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7306.30.00.10</li>
			<li>7306.30.00.20</li>
			<li>7306.30.00.30</li>
		</ul>
		
		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
	</dd>
	
	<dt>Duty Liability<br>
	(Anti-dumping &amp; Countervailing duties)</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Country of Origin or Export: China</h3>
		
		<p>Effective on imports of subject goods released by the CBSA on or after 2011-02-14:</p>

		<p>No exporters in China received normal values or a specific amount of subsidy in the most recent re-investigation, please consult <a href=""/sima-lmsi/ri-re/ad1373/ad1373-ri10-nc-eng.html"">CBSA Notice of Conclusion of Re-investigation</a>.</p>

		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under SIMA and from the CITT order. Given that no exporters or producers provided a response to the CBSA’s RFI, normal values and amount of subsidy will therefore be determined by a ministerial specification under SIMA.</p>

		<p>For importations of subject goods originating in/or exported from China, the anti-dumping duty is equal to 179% of the export price, as specified by the Minister.</p>

		<p>For importations of subject goods originating in/or exported from China, the countervailing duty is equal to 5,280 Renminbi per metric tonne, as specified by the Minister.</p>

		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
	</dd>
	
	<dt>Information Required on Customs Documents </dt>
	<dd>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap/menu-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li>Harmonized System (HS) classification number</li>
			<li>Confirmation whether the product is subject to anti-dumping and countervailing duties</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description 
				<ul>
					<li>Product name and/or number</li>
					<li>Product grade and specification</li>
					<li>Dimension (nominal size) and wall thickness</li>
					<li>Pipe finish</li>
					<li>End finish</li>
				</ul>
			</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure – e.g. kg, metric tonne)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.) and,</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.).</li>
		</ul>
	</dd>
	
	<dt>Appeal Decisions Relating to Subjectivity</dt>
	<dd>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
	</dd>
	
	<dt>Email for Duty Assessment Questions</dt>
	<dd>
		<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca?subject=Carbon Steel Welded Pipe (CSWP 1)"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>
	</dd>
	
	<dt>CBSA Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>Dumping file #: 4214-16</li>
			<li>Dumping case #: AD1373</li>
			<li>Subsidy file #: 4218-24</li>
			<li>Subsidy case #: CV123</li>
		</ul>
	</dd>
	
	<dt>CITT Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>NQ-2008-001</li>
			<li>RR-2012-003</li>
		</ul>
	</dd>
</dl>

<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2019-12-11</time></dd>
</dl>
</main>";
		#endregion

		#region html5
		const string html5 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">

<!-- MainContentStart -->

<h1 class=""mrgn-tp-md"" id=""wb-cont"">Heavy plate<br />
<span style=""font-weight:normal"">Dumping (Chinese Taipei, Germany, and Turkey)</span></h1>

<dl class=""dl-horizontal"">
	<dt>Product definition</dt>
	<dd>
		<p>The subject goods are defined as:</p>
		
		<div class=""well"">
			<p class=""mrgn-bttm-0"">Hot&#8209;rolled carbon steel plate and high&#8209;strength low&#8209;alloy steel plate not further manufactured than hot&#8209;rolled, heat&#8209;treated or not, in cut lengths, in widths greater than 72 inches (+/&#8209; 1829 mm) to 152 inches (+/&#8209; 3,860 mm) inclusive, and thicknesses from 0.375 inches (+/&#8209; 9.525 mm) up to and including 4.5 inches (+/&#8209; 114.3 mm) (with all dimensions being plus or minus allowable tolerances contained in the applicable standards), but excluding:</p>

			<ul>
				<li>plate in coil form, and</li>
				<li>plate having a rolled, raised figure at regular intervals on the surface (also known as floor plate).</li>
			</ul>

			<p class=""mrgn-bttm-0"">For greater certainty, the subject goods include steel plate which contains alloys greater than required by recognized industry standards, provided the steel does not meet recognized industry standards for an alloy&#8209;grade steel plate.</p>
		</div>
	</dd>
	
	<dt>Investigation information</dt>
	<dd>
		<p>The dates of the investigative proceedings and finding concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr>
					<th>Action</th>
					<th>Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/hp2020/hp2020-ni-eng.html"">Initiation of investigation</a></td>
					<td>May 27, 2020</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/hp2020/hp2020-np-eng.html"">Preliminary decisions</a></td>
					<td>October 9, 2020</td>
				</tr>
				<tr>
					<td>Final determination</td>
					<td>January 7, 2021</td>
				</tr>
				<tr>
					<td>Canadian International Trade Tribunal's finding</td>
					<td>February 5, 2021</td>
				</tr>
			</tbody>
		</table>
	</dd>
	
	<dt>Tariff classification numbers</dt>
	<dd>
		<p>The subject goods are usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7208.51.00.10</li>
			<li>7208.51.00.93</li>
			<li>7208.51.00.94</li>
			<li>7208.51.00.95</li>
			<li>7208.52.00.10</li>
			<li>7208.52.00.93</li>
			<li>7208.52.00.96</li>
		</ul>
		
		<p>Please note that these tariff classification numbers may apply to goods which are not subject to the <i>Special Import Measures Act</i> (SIMA) measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under tariff classification numbers that are not listed.</p>

		<p>Refer to the product definition for the authoritative details regarding the subject goods. For more information on the tariff classification numbers, please refer to the CBSA's <a href=""/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">website</a>.</p>
	</dd>
	
	<dt>Duty liability<br>
	(provisional anti-dumping duties)</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Country of origin or export: Chinese Taipei, Germany, Turkey</h3>
		
		<p>Provisional anti&#8209;dumping duty is payable on subject goods that are released from the CBSA during the period commencing October 9, 2020, and ending on the earlier of the day the dumping investigation is terminated, the day on which the Canadian International Trade Tribunal (CITT) makes an order or finding, or the day an undertaking is accepted.</p>

		<p>For information regarding the rates of provisional anti&#8209;dumping duty, please consult the CBSA’s <a href=""/sima-lmsi/i-e/hp2020/hp2020-np-eng.html"">notice of preliminary decisions</a>.</p>

		<p>For importations of subject goods for which the exporter has not been issued a specific rate, the rates of anti&#8209;dumping duty are equal to:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of origin or export</th>
					<th class=""text-center"" scope=""col"">Provisional anti-dumping duty<sup><span class=""wb-inv"">Footnote </span>1</sup></th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Chinese Taipei</td>
					<td>97.0%</td>
				</tr>
				<tr>
					<td>Germany</td>
					<td>63.8%</td>
				</tr>
				<tr>
					<td>Turkey</td>
					<td>25.8%</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td colspan=""2""><sup><span class=""wb-inv"">Footnote </span>1</sup> As a percentage of export price.</td>
				</tr>
			</tfoot>
		</table>
	</dd>
	
	<dt>Disclosure of normal values</dt>
	<dd>
		<p>The liability for provisional anti&#8209;dumping results from the proceeding conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a>. For more information, please consult <a href=""/publications/dm-md/d14/d14-1-7-eng.html"">Memorandum D14&#8209;1&#8209;7</a>, Assessment and Payment of Duties Under the Special Import Measures Act.</p>
		
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
	</dd>
	
	<dt>Information required on customs documents</dt>
	<dd>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li><b>Confirmation whether the product is subject to provisional duties</b></li>
			<li>Name and address of producer/manufacturer</li>
			<li>Name and location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer’s name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description of the goods, including: 
				<ul>
					<li>Specification – Identify the specification of the product exported to Canada</li>
					<li>Grade – Identify the grade of the product exported to Canada</li>
					<li>Heat treatment - Indicate whether or not the product exported to Canada was heat treated</li>
					<li>Vacuum degassed – Indicate if the product is vacuum degassed (yes/no)</li>
					<li>Product quality – Indicate if the product is prime or secondary product</li>
					<li>Thickness – Indicate the thickness of the product exported to Canada (specified in inches)</li>
					<li>Width – Indicate the width of the product exported to Canada (specified in inches)</li>
					<li>Length – Indicate the length of the product exported to Canada (specified in inches)</li>
				</ul>
			</li>
		</ul>
		<p>Other relevant characteristics:</p>
		<ul>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g., US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g., FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
			<li>The amount of any export taxes applicable to the goods</li>
		</ul>
	</dd>

	<dt>Appeal decisions relating to subjectivity</dt>
	<dd>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
	</dd>
	
	<dt>Email for duty assessment questions</dt>
	<dd>
		<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Concrete Reinforcing Bar 2"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>
	</dd>
	
	<dt>CBSA Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>HP 2020 IN</li>
		</ul>
	</dd>
	
	<dt>CITT Reference Number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>PI&#8209;2020&#8209;001</li>
		</ul>
	</dd>
</dl>

<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2020-10-09</time></dd>
</dl>
</main>";
		#endregion

		#region html6
		const string html6 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">

<!-- MainContentStart -->

<h1 class=""mrgn-tp-md"" id=""wb-cont"">Certain oil country tubular goods 4 (OCTG 4)<br>
<span style=""font-weight:normal"">Dumping (Austria)</span></h1>

<dl class=""dl-horizontal"">
	<dt>Measure in Force code</dt>
	<dd class=""text-uppercase"">octg4</dd>

	<dt>Product information</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Product definition</h3>
		
		<p>The subject goods are defined as:</p>
		
		<div class=""well"">
			<p class=""mrgn-bttm-0"">Oil country tubular goods, which are casing, tubing and green tubes made of carbon or alloy steel, welded or seamless, heat treated or not heat treated, regardless of end finish, having an outside diameter from 2 ⅜ inches to 13 ⅜ inches (60.3 mm to 339.7 mm), meeting or supplied to meet American Petroleum Institute specification 5CT or equivalent and/or enhanced proprietary standards, in all grades, excluding drill pipe, pup joints, couplings, coupling stock and stainless steel casing, tubing or green tubes containing 10.5 percent or more by weight of chromium, originating in or exported from Austria.</p>
		</div>
	</dd>
	
	<dt>Investigation information</dt>
	<dd>
		<p>The dates of the investigative proceedings and findings concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/octg42021/octg42021-in-eng.html"">Initiation of investigation</a></td>
					<td><time datetime=""2021-07-07"">July 7, 2021</time></td>
				</tr>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/octg42021/octg42021-np-eng.html"">Preliminary determination</a></td>
					<td><time datetime=""2021-10-25"">October 25, 2021</time></td>
				</tr>
			</tbody>
		</table>
	</dd>
	
	<dt>Tariff classification numbers</dt>
	<dd>
		<p>The subject goods are usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7304.29.00.11</li>
			<li>7304.29.00.19</li>
			<li>7304.29.00.21</li>
			<li>7304.29.00.29</li>
			<li>7304.29.00.31</li>
			<li>7304.29.00.39</li>
			<li>7304.29.00.41</li>
			<li>7304.29.00.49</li>
			<li>7304.29.00.51</li>
			<li>7304.29.00.59</li>
			<li>7304.29.00.61</li>
			<li>7304.29.00.69</li>
			<li>7304.29.00.71</li>
			<li>7304.29.00.79</li>
			<li>7306.29.00.11</li>
			<li>7306.29.00.19</li>
			<li>7306.29.00.21</li>
			<li>7306.29.00.31</li>
			<li>7306.29.00.29</li>
			<li>7306.29.00.39</li>
			<li>7306.29.00.61</li>
			<li>7306.29.00.69</li>
		</ul>
		
		<p>The above‑listed tariff classifications cover both subject and non‑subject goods and are for convenience of reference only.</p>

		<p>Please note that these tariff classification numbers may apply to goods which are not subject to the <i>Special Import Measures Act</i> (SIMA) measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under tariff classification numbers that are not listed.</p>

		<p>Refer to the product definition for the authoritative details regarding the subject goods. For more information on the tariff classification numbers, please refer to the <a href=""https://www.cbsa-asfc.gc.ca/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">harmonized commodity description and coding system</a>.</p>
	</dd>
	
	<dt>Duty liability<br>
	(Provisional anti-dumping duties)</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Country of origin or export: Austria</h3>
		
		<p>Provisional anti‑dumping duty is payable on subject goods that are released from the CBSA during the period commencing October 25, 2021, and ending on the earlier of the day the dumping investigation is terminated, the day on which the Canadian International Trade Tribunal (CITT) makes an order or finding, or the day an undertaking is accepted.</p>

		<p>For importations of subject goods the rates of anti‑dumping duty are equal to:</p>

		<table class=""table table-bordered mrgn-bttm-sm"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Originating in or exported from</th>
					<th class=""text-center"" scope=""col"">Provisional anti‑dumping duty<br>
						<span class=""noBold"">(as a percentage of export price)</span></th>
				</tr>
			</thead>
			<tbody>
				<tr class=""active"">
					<td colspan=""2"">Austria</td>
				</tr>
				<tr>
					<td>Voestalpine Tubulars GmbH &amp; Co KG</td>
					<td>35.1%</td>
				</tr>
				<tr>
					<td>All other exporters</td>
					<td>91.0%</td>
				</tr>
			</tbody>
		</table>
	</dd>
	
	<dt>Information required on customs documents</dt>
	<dd>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/amps-rsap/menu-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li><b>Confirmation whether the product is subject to provisional duties</b></li>
			<li>Name and address of producer/manufacturer</li>
			<li>Location of plant/mill of production </li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description of the goods, including:
				<ul>
					<li>Product type (i.e. welded casing, seamless tubing, etc.)</li>
					<li>Grade</li>
					<li>Specification (i.e. API 5CT or other proprietary standard)</li>
					<li>Outside diameter</li>
					<li>Gauge (nominal weight in lb/ft or kg/m)</li>
					<li>Length</li>
					<li>End finish (plain end, API Standard, semi‑premium, premium)</li>
				</ul>
			</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (metric tonnes)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.)</li>
			<li>The amount of any export taxes applicable to the goods</li>
		</ul>
	</dd>

	<dt>Appeal decisions relating to subjectivity</dt>
	<dd>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
	</dd>
	
	<dt>Email for subjectivity opinions and duty assessment questions</dt>
	<dd>
		<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca?subject=Oil%20Country%20Tubular%20Goods%204%20(OCTG%204)"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>

		<p><span class=""text-uppercase""><b>Important:</b></span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
	</dd>
	
	<dt>CBSA reference number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>OCTG4 2021 IN</li>
		</ul>
	</dd>
	
	<dt>CITT reference number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>PI‑2021‑004</li>
		</ul>
	</dd>
</dl>

<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2021-10-29</time></dd>
</dl>
</main>";
		#endregion

		#region html7
		const string html7 = @"<main role=""main"" property=""mainContentOfPage"" class=""container"">

<!-- MainContentStart -->

<h1 class=""mrgn-tp-md"" id=""wb-cont"">Upholstered domestic seating<br>
<span style=""font-weight:normal"">Dumping and subsidy (China and Vietnam)</span></h1>

<dl class=""dl-horizontal"">
	<dt>Measure in Force code</dt>
	<dd class=""text-uppercase"">uds</dd>

	<dt>Product definition</dt>
	<dd>
		<p>The subject goods are defined as:</p>
		
		<div class=""well"">
			<p class=""mrgn-bttm-0"">Upholstered seating for domestic purposes originating in or exported from the People’s Republic of China and the Socialist Republic of Vietnam, whether motion (including reclining, swivel and other motion features) or stationary, whether upholstered with a covering of leather (either full or partial), fabric (including leather substitutes) or both, including, but not limited to seating such as sofas, chairs, loveseats, sofa beds, day beds, futons, ottomans, stools and home theatre seating (HTS).</p>

			<p class=""mrgn-bttm-0"">Excluding:</p>

			<ol class=""lst-lwr-alph"">
				<li>Stationary (i.e. non motion) seating upholstered only with fabric (rather than leather), even if the fabric is a leather‑substitute (such as leather like or leather‑look polyurethane or vinyl)</li>
				<li>dining table chairs or benches (with or without arms) that are manufactured for dining room end‑use, which are commonly paired with dining table sets</li>
				<li>upholstered stools with a seating height greater than 24 inches (commonly referred to as “bar stools” or “counter stools”), with or without backs, and/or foldable</li>
				<li>seating manufactured for outdoor use (e.g. patio or swing chairs)</li>
				<li>bean bag seating and</li>
				<li>foldable or stackable seating</li>
			</ol>

			<p class=""mrgn-bttm-0"">For greater certainty, the product definition includes:</p>

			<ol class=""lst-lwr-alph"">
				<li>Upholstered motion seating with reclining, swivel, rocking, zero gravity, gliding, adjustable headrest, massage functions or similar functions</li>
				<li>seating with frames constructed from metal, wood or both</li>
				<li>seating produced as sectional items or parts of sectional items</li>
				<li>seating with or without arms, whether part of sectional items or not and</li>
				<li>foot rests and foot stools (with or without storage)</li>
			</ol>
		</div>

		<p>On September 2, 2021, the Canadian International Trade Tribunal excluded the following products from its finding:</p>

		<ol>
			<li>Specialized reclining massage chairs, not intended to be used for general seating purposes, with padded seat, headrest, back, and footrest, and containing built‑in motorized mechanical components that operate by way of computerized controls to provide a full body massage for a single person, including to the head and/or neck, shoulders, back, buttocks, arms, and legs and/or feet</li>
			<li>Medical lift chairs containing electric motion mechanisms and motorized positioning controls, designed to carefully lift, lower and tilt (by raising or lowering the base and back of the seating) the occupant, and otherwise adjust the occupant’s seating position by adjusting one or more of the headrest, footrest, and seat; designed, manufactured, and tested to meet or exceed the requirements of Health Canada’s Medical Devices Regulations (SOR/98‑282) applicable thereto and conforming with the following, or equivalent, standards and testing methodologies: EN12182, ANSI/AAMI/ISO10993, ANSI/AAMI/ES60601‑1, CAL117, BSEN1021, ISO8191, ANSI/AAMI/ES60601‑1‑2, ISO14971</li>
			<li>Height‑adjustable ergonomic gaming chairs for use with a desk and intended to be used primarily while playing video games, upholstered in leather or a leather‑substitute, with armrests, headrests, lumbar support pillows, five‑star swivel bases, and wheels or castors</li>
		</ol>
	</dd>
	
	<dt>Investigations information</dt>
	<dd>
		<p>The dates of the investigative proceedings and finding concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/uds2020/uds2020-in-eng.html"">Initiation of investigations</a></td>
					<td><time datetime=""2020-12-21"">December 21, 2020</time></td>
				</tr>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/uds2020/uds2020-pd-eng.html"">Preliminary determinations</a></td>
					<td><time datetime=""2021-05-05"">May 5, 2021</time></td>
				</tr>
				<tr>
					<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/uds2020/uds2020-fd-eng.html"">Final determination</a></td>
					<td><time datetime=""2021-08-03"">August 3, 2021</time></td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/512053/index.do"">Canadian International Trade Tribunal Finding</a></td>
					<td><time datetime=""2021-09-02"">September 2, 2021</time></td>
				</tr>
			</tbody>
		</table>
	</dd>
	
	<dt>Tariff classification numbers</dt>
	<dd>
		<p>The subject goods are usually imported under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>9401.40.00.00</li>
			<li>9401.61.10.10</li>
			<li>9401.61.10.90</li>
			<li>9401.71.10.10</li>
			<li>9401.71.10.90</li>
		</ul>
		
		<p>Please note that these tariff classification numbers may apply to goods which are not subject to the <i>Special Import Measures Act</i> (SIMA) measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under tariff classification numbers that are not listed.</p>

		<p>Refer to the product definition for the authoritative details regarding the subject goods. For more information on the tariff classification numbers, please refer to the <a href=""https://www.cbsa-asfc.gc.ca/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">harmonized commodity description and coding system</a>.</p>
	</dd>
	
	<dt>Duty liability<br>
	(Anti‑dumping duties)</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Country of origin or export: China and Vietnam</h3>
		
		<p>Information regarding the normal values of subject goods should be obtained from the exporter. The following table identifies the exporters who currently have been issued normal values:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Cooperative since</th>
					<th class=""text-center"" scope=""col"">Last revised</th>
				</tr>
			</thead>
			<tbody>
				<tr class=""active"">
					<td colspan=""3"">China</td>
				</tr>
				<tr>
					<td>Anji Cozy Home Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Anji Hengrui Furniture Co.,Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Anji Hengyi Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Anji UES Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Dongguan Tianhang Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Foshan DOB Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Foshan Xingpeichong Huitong Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Gu Jia Intelligent Household Jiaxing Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Haining Fanmei Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HaiNing Happy Leather Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Haining Kendy Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Haining Nicelink Home Furnishings Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>(Hangzhou) Huatong Industries Inc.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Henglin Home Furnishings Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HHC Changzhou Corp.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HTL Furniture (China) Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HTL Furniture (Huai An) Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Jason Furniture (Hangzhou) Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Jiaxing Motion Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Jiaxing Vitra Electrical Technology Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Man Wah Furniture Manufacturing (Huizhou) Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Megain Furniture (Dong Guan) Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Natuzzi (China) Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Ruihao Furniture MFG Co., Ltd</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Shanghai Trayton Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Trayton Furniture (Jiaxing) Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>UE Furniture Co., Ltd</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Vanguard Industrial JiaXing Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Violino Furniture (Shenzhen) Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Botai Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Chuanyang Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Happy Smart Furnishings Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Kuka Merlin Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr class=""active"">
					<td colspan=""3"">Vietnam</td>
				</tr>
				<tr>
					<td>Delancey Street Furniture Vietnam Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Koda Saigon Co. Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Motomotion Vietnam Limited Company</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Timberland Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>UE Furniture Vietnam Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Vietnam Hang Phong Furniture Company Limited</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Wanek Furniture Co., Ltd.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Wendelbo SEA JSC</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr class=""active"">
					<td colspan=""3"">Other</td>
				</tr>
				<tr>
					<td>Ashley Furniture Industries, LLC</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Restoration Hardware, Inc.</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Wendelbo Interiors A/S</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
			</tbody>
		</table>

		<p>For importations of subject goods for which the exporter has not been issued its own normal values, the rates of anti‑dumping duty are equal to:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of origin or export</th>
					<th class=""text-center"" scope=""col"">Margin of dumping rate for all other exporters<sup>1</sup></th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China</td>
					<td class=""text-right"">188.0%</td>
				</tr>
				<tr>
					<td>Vietnam</td>
					<td class=""text-right"">179.5%</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class=""small"" colspan=""2""><sup>1</sup>As a percentage of export price</td>
				</tr>
			</tfoot>
		</table>

		<p>As a result of the Canadian International Trade Tribunal (CITT) finding on September 2, 2021, model-specific normal values for future shipments of subject goods from exporters of certain upholstered domestic seating will be determined pursuant to sections 15 to 23 of the Special Import Measures Act (SIMA) where sufficient information to allow this determination is available to the CBSA.  Subject goods for which model-specific normal values have not been established will be determined based on the export price plus an amount equal to the exporter’s weighted average margin of dumping determined at the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/uds2020/uds2020-nf-eng.html"">final determination</a>. Normal values for any exporter not listed will be determined pursuant to section 29 of SIMA based on the export price as determined under section 24, 25 or 29 of SIMA, plus an amount equal to 188.0% for China or 179.5% for Vietnam of that export price.</p>

		<p>It should be noted that while this methodology will be applied for the purpose of assessing an amount of anti-dumping duty at the time of importation, the normal value and export price of such goods may subsequently be re-determined by a designated officer where a request for redetermination is made, or where the designated officer deems it advisable. Such redeterminations could result in retroactive changes to the amount of anti-dumping duty assessed.</p>
	</dd>
	
	<dt>Duty liability<br>
	(Countervailing duties)</dt>
	<dd>
		<h3 class=""h5 mrgn-tp-0"">Country of origin or export: China and Vietnam</h3>
		
		<p>The following table identifies the exporters who currently have a specific amount of subsidy:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Amount of subsidy per piece</th>
					<th class=""text-center"" scope=""col"">Cooperative since</th>
					<th class=""text-center"" scope=""col"">Last revised</th>
				</tr>
			</thead>
			<tbody>
				<tr class=""active"">
					<td colspan=""4"">China</td>
				</tr>
				<tr>
					<td>Anji Cozy Home Co., Ltd.</td>
					<td class=""text-center"">15.14 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Anji Hengrui Furniture Co.,Ltd.</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Anji Hengyi Furniture Co., Ltd.</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Anji UES Furniture Co., Ltd.</td>
					<td class=""text-center"">27.21 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Dongguan Tianhang Furniture Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Foshan DOB Furniture Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Foshan Xingpeichong Huitong Furniture Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Gu Jia Intelligent Household Jiaxing Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Haining Fanmei Furniture Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HaiNing Happy Leather Furniture Co., Ltd.</td>
					<td class=""text-center"">103.03 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Haining Kendy Furniture Co., Ltd.</td>
					<td class=""text-center"">854.23 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Haining Nicelink Home Furnishings Co., Ltd.</td>
					<td class=""text-center"">53.24 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>(Hangzhou) Huatong Industries Inc.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Henglin Home Furnishings Co., Ltd.</td>
					<td class=""text-center"">20.56 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HHC Changzhou Corp.</td>
					<td class=""text-center"">47.02 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HTL Furniture (China) Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>HTL Furniture (Huai An) Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Jason Furniture (Hangzhou) Co., Ltd.</td>
					<td class=""text-center"">21.04 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Jiaxing Motion Furniture Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Jiaxing Vitra Electrical Technology Co., Ltd.</td>
					<td class=""text-center"">17.17 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Man Wah Furniture Manufacturing (Huizhou) Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Megain Furniture (Dong Guan) Co., Ltd.</td>
					<td class=""text-center"">37.36 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Natuzzi (China) Ltd.</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Ruihao Furniture MFG Co., Ltd</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Shanghai Trayton Furniture Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Trayton Furniture (Jiaxing) Co., Ltd.</td>
					<td class=""text-center"">181.18 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>UE Furniture Co., Ltd</td>
					<td class=""text-center"">22.84 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Vanguard Industrial JiaXing Co., Ltd.</td>
					<td class=""text-center"">25.26 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Violino Furniture (Shenzhen) Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Botai Furniture Co., Ltd.</td>
					<td class=""text-center"">15.70 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Chuanyang Furniture Co., Ltd.</td>
					<td class=""text-center"">12.19 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Happy Smart Furnishings Co., Ltd.</td>
					<td class=""text-center"">148.47 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Zhejiang Kuka Merlin Furniture Co., Ltd.</td>
					<td class=""text-center"">19.36 CNY</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr class=""active"">
					<td colspan=""4"">Vietnam</td>
				</tr>
				<tr>
					<td>Delancey Street Furniture Vietnam Co., Ltd.</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Koda Saigon Co. Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Motomotion Vietnam Limited Company</td>
					<td class=""text-center"">173,163.43 VND</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Timberland Co., Ltd.</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>UE Furniture Vietnam Co., Ltd.</td>
					<td class=""text-center"">‑<sup>1</sup></td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Vietnam Hang Phong Furniture Company Limited</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Wanek Furniture Co., Ltd.</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Wendelbo SEA JSC</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr class=""active"">
					<td colspan=""4"">Other</td>
				</tr>
				<tr>
					<td>Ashley Furniture Industries, LLC</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Restoration Hardware, Inc.</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
				<tr>
					<td>Wendelbo Interiors A/S</td>
					<td class=""text-center"">‑</td>
					<td class=""text-center"">2021‑08</td>
					<td class=""text-center"">2021‑08</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class=""small"" colspan=""2""><sup>1</sup>Pursuant to section 2(1) of the Special Import Measures Act (SIMA), an amount of subsidy of less than 1% of the export price of the goods is insignificant for a developed country and of less than 2% of the export price of the goods for a developing country.</td>
				</tr>
			</tfoot>
		</table>

		<p>The CBSA, pursuant to paragraph 41(1)(a) of SIMA, terminated the investigation of subsidizing of certain upholstered domestic furniture originating in or exported from China by Anji Hengrui Furniture Co., Ltd., Anji Hengyi Furniture Co., Ltd., Dongguan Tianhang Furniture Co., Ltd., Foshan DOB Furniture Co., Ltd., Foshan Xingpeichong Huitong Furniture Co., Ltd., Gu Jia Intelligent Household Jiaxing Co., Ltd., Haining Fanmei Furniture Co., Ltd., (Hangzhou) Huatong Industries Inc., HTL Furniture (China) Co., Ltd., HTL Furniture (Huai An) Co., Ltd., Jiaxing Motion Furniture Co., Ltd., Man Wah Furniture Manufacturing (Huizhou) Co., Ltd., Natuzzi (China) Ltd., Ruihao Furniture MFG Co., Ltd, Shanghai Trayton Furniture Co., Ltd., Violino Furniture (Shenzhen) Ltd., and in respect of certain upholstered domestic seating originating in or exported from Vietnam by Delancey Street Furniture Vietnam Co., Ltd., Koda Saigon Co. Ltd., Timberland Co., Ltd., UE Vietnam Co., Ltd., Vietnam Hang Phong Furniture Company Limited, Wanek Furniture Co., Ltd., and Wendelbo SEA JSC, as there was no subsidizing or the amounts of subsidy were insignificant.</p>

		<p>For importations of subject goods for which the exporter has not been issued a specific rate, the rates of countervailing duty are equal to:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of origin or export</th>
					<th class=""text-center"" scope=""col"">Countervailing duty rate for all other exporters<sup>1</sup></th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China</td>
					<td class=""text-right"">1,390.65 CNY</td>
				</tr>
				<tr>
					<td>Vietnam</td>
					<td class=""text-right"">1,914,726.79 VND</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class=""small"" colspan=""2""><sup>1</sup>Amount of subsidy per piece</td>
				</tr>
			</tfoot>
		</table>
	</dd>
	
	<dt>Disclosure of normal values and amounts of subsidy</dt>
	<dd>
		<p>The liability for anti‑dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a>. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti‑dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need‑to‑know basis in accordance with the provisions of <a href=""https://www.cbsa-asfc.gc.ca/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14‑1‑2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act to Importers</i>.</p>
		
		<p>For information on duty assessment, refer to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/self-auto-eng.html"">guide for self‑assessing SIMA duties</a>.</p>
	</dd>
	
	<dt>Information required on customs documents</dt>
	<dd>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li>Confirmation whether the product is subject to provisional duties</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Name and location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer’s name and address</li>
			<li>Canadian importer’s name and address (if different from the customer)</li>
			<li>Full product description of the goods, including:
				<ul>
					<li>Product type</li>
					<li>Length</li>
					<li>Cover material</li>
					<li>Maximum foam density of seat cushions</li>
					<li>% memory foam used in seat cushion</li>
					<li>Feathers</li>
					<li>Number of power recline</li>
					<li>Number of power headrest</li>
					<li>Number of power lumbar</li>
					<li>Number of manual recline</li>
					<li>Number of manual headrest</li>
					<li>Number of other motion mechanisms</li>
					<li>Additional movement types</li>
					<li>Number of USB outlets</li>
					<li>Number of AC power outlets</li>
					<li>Heating</li>
					<li>Cooling</li>
					<li>Lights (including LEDs)</li>
					<li>Massage—Mechanical</li>
					<li>Massage—Air bladder</li>
					<li>Vibration (e.g. for movie and gaming effects)</li>
					<li>Number of speakers</li>
					<li>Number of cup holders</li>
					<li>Storage</li>
				</ul>
			</li>
		</ul>

		<h3 class=""h5"">Other relevant characteristics</h3>
		<ul>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
			<li>The amount of any export taxes applicable to the goods</li>
		</ul>
	</dd>

	<dt>Appeal decisions relating to subjectivity</dt>
	<dd>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/appeals-eng.html"">SIMA appeals</a>.</p>
	</dd>
	
	<dt>Email for subjectivity opinions and duty assessment questions</dt>
	<dd>
		<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca?subject=Upholstered%20domestic%20seating"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>

		<p><span class=""text-uppercase""><b>Important:</b></span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
	</dd>
	
	<dt>CBSA reference number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>UDS 2020 IN</li>
		</ul>
	</dd>
	
	<dt>CITT reference number(s)</dt>
	<dd>
		<ul class=""list-unstyled"">
			<li>PI‑2020‑007</li>
		</ul>
	</dd>
</dl>

<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2021-10-15</time></dd>
</dl>
</main>
";
		#endregion

		#region html8
		const string html8 = @"<table class=""wb-tables table table-striped"" data-wb-tables='{""columnDefs"":[{""visible"":false,""targets"":2},{""orderable"":false,""targets"":1}],""paging"":false}'>
	<colgroup>
		<col class=""col-md-4"">
		<col>
		<col>
	</colgroup>
	<thead>
		<tr>
			<th scope=""col"">Case</th>
			<th scope=""col"">Case type</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/ae-eng.html"">Aluminum extrusions (AE)</a></td>
			<td>Dumping&nbsp;and subsidy: China</td>
		</tr>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/crs-eng.html"">Cold-rolled steel (CRS)</a></td>
			<td>Dumping and subsidy: China, South Korea, Vietnam</td>
		</tr>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/cswp1-eng.html"">Carbon steel welded pipe (CSWP1)</a></td>
			<td></td>
		</tr>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/hp-eng.html"">Heavy plate (HP)</a></td>
			<td>Dumping&nbsp;and subsidy: China</td>
		</tr>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/octg1-eng.html"">Oil country tubular goods (OCTG1)</a></td>
			<td>Dumping&nbsp;and subsidy: China</td>
		</tr>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/uds-eng.html"">Upholstered domestic seating (UDS)</a></td>
			<td>Dumping&nbsp;and subsidy: China</td>
		</tr>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/wp-eng.html"">Whole potatoes (POT)</a></td>
			<td>Dumping: United States</td>
		</tr>
	</tbody>
</table>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-05-16</time></dd>
</dl>
</div>";
		#endregion

		#region html81 Whole potatoes (POT)
		const string html81WP = @"<table class=""wb-tables table table-striped"" data-wb-tables='{""columnDefs"":[{""visible"":false,""targets"":2},{""orderable"":false,""targets"":1}],""paging"":false}'>
	<colgroup>
		<col class=""col-md-4"">
		<col>
		<col>
	</colgroup>
	<thead>
		<tr>
			<th scope=""col"">Case</th>
			<th scope=""col"">Case type</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/wp-eng.html"">Whole potatoes (POT)</a></td>
			<td>Dumping: United States</td>
		</tr>
	</tbody>
</table>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-05-16</time></dd>
</dl>
</div>";
		#endregion

		#region html9
		const string html9 = @"<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Aluminum extrusions: Measures in force</h1>
</header>

<p>Dumping&nbsp;and subsidizing (China)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>AE</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>

	<div class=""well"">
		<p>""Aluminum extrusions produced via an extrusion process, of alloys having metallic elements falling within the alloy designations published by The Aluminum Association commencing with 1, 2, 3, 5, 6 or 7 (or proprietary or other certifying body equivalents), with the finish being as extruded (mill), mechanical, anodized or painted or otherwise coated, whether or not worked, having a wall thickness greater than 0.5&nbsp;mm., with a maximum weight per meter of 22 kilograms and a profile or cross-section which fits within a circle having a diameter of 254&nbsp;mm., originating in/or exported from the People's Republic of China.""</p>
	</div>

	<h3>Exclusions</h3>

	<ul>
		<li>aluminum extrusions produced from either a 6063 or a 6005 alloy type with a T6 temper designation, in various lengths, with a powder coat finish on both the interior and the exterior surfaces of the extrusion, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard, ""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels"", for use in exterior railing systems;</li>
		<li>aluminum extrusions produced from a 6063 alloy type with a T5 temper designation, having a length of 3.66 m, with a powder coat finish, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard, ""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels"", for use as head rails and bottom rails in fabric window shades and blinds where the fabric has a cross-sectional honeycomb or ""cellular"" construction;</li>
		<li>aluminum extrusions produced from a 6063 alloy type with a T5 temper designation and forming part of the Vario System™ 20, 30, 40, 45 and 60 series line of profiles, or equivalent, having a length of either 4.5 or 5.8 m and a straightness tolerance of +/-1.5&nbsp;mm or less per 6.0 m of length, for use in those parts of mechanical systems and automated machinery, such as gantry systems and conveyors, where precise linear movement is required;</li>
		<li>aluminum extrusions produced from either a 6063 or a 6463 alloy type, having a length of 3 m, with a hand-applied gold and silver leaf finish, for use as picture frame mouldings;</li>
		<li>aluminum extrusions produced from a 6063 alloy type with either a T5 or a T6 temper designation, having a length of between 20 and 33&nbsp;ft. (between 6.10 and 10.06 m), with a powder coat finish, which finish is certified to meet the American Architectural Manufacturers Association AAMA 2603 standard (""Voluntary Specification, Performance Requirements and Test Procedures for Pigmented Organic Coatings on Aluminum Extrusions and Panels""), for use in window frames;</li>
		<li>heat sinks imported under tariff item No. 8473.30.90 and weighing 700 g or less; and</li>
		<li>aluminum extrusions produced by China Square Industrial Ltd. from either a 6063 or a 6463 alloy type with a T5 temper designation, with a profile or cross-section which fits within a circle having a diameter of 100&nbsp;mm, for use by MAAX Bath Inc. in the assembly of its shower enclosures, specifically identified in the Appendix of the Determination and reasons issued by the Canadian International Trade Tribunal on February 10, 2011, in Inquiry No. NQ-2008-003R. The list of these excluded products can be found at the following link: <a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353483/index.do"">https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353483/index.do</a></li>
	</ul>
	<p>For the excluded products listed above that require a finish which is &quot;certified to meet the American Architectural Manufacturers Association AAMA 2603 standard&quot;, the importer must be able to provide evidence that their goods meet that standard. </p>
</section>
<section>
	<h2>Investigation information</h2>
	<p>The dates of the investigative proceedings and findings concerning this case are:</p>
	<table class=""table table-bordered"">
		<thead>
			<tr class=""active"">
				<th scope=""col"">Action</th>
				<th scope=""col"">Date</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-de-eng.html"">Initiation of Investigation</a></td>
				<td>August 18, 2008</td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-pd-eng.html"">Preliminary Determination</a></td>
				<td>November 17, 2008</td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/i-e/ad1379/ad1379-i08-fd-eng.html"">Final Determination</a></td>
				<td>February 16, 2009</td>
			</tr>
			<tr>
				<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353476/index.do"">Canadian International Trade Tribunal's Finding</a></td>
				<td>March 17, 2009</td>
			</tr>
			<tr>
				<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353483/index.do"">Tribunal's Remand Determination</a></td>
				<td>February 10, 2011</td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/ri-re/ad1379/ad1379-ri11-nc-eng.html"">Re-Investigation</a></td>
				<td>February 20, 2012</td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/er-rre/rr2013-003/rr2013-003-e13-de-eng.html"">Expiry Review Determination</a></td>
				<td>October 18, 2013</td>
			</tr>
			<tr>
				<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353850/index.do"">Canadian International Trade Tribunal's Order</a></td>
				<td>March 17, 2014</td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/sp-pp/ae2018/ae2018-sr-eng.html"">Scope Ruling - Statement of Reasons - Nokia Canada Inc.</a></td>
				<td>February 22, 2019</td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/er-rre/ae2019/ae2019-de-eng.html"">Expiry Review Determination</a></td>
				<td>August 16, 2019</td>
			</tr>
			<tr>
				<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/460010/index.do"">Canadian International Trade Tribunal's Order</a></td>
				<td>January 13, 2020</td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/up/ae2021/ae202101-nc-eng.html"">Normal value review&mdash;Fujian Fenan</a></td>
				<td><time class=""nowrap"" datetime=""2021-09-28"">September 28, 2021</time></td>
			</tr>
			<tr>
				<td><a href=""/sima-lmsi/up/ae2021/ae202102-nc-eng.html"">Normal value review&mdash;Test Rite</a></td>
				<td><time class=""nowrap"" datetime=""2022-03-03"">March 3, 2022</time></td>
			</tr>
		</tbody>
	</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>

	<p>The subject goods are usually classified under the following tariff classification numbers:</p>
	
	<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
		<li>7604.10.00.30</li>
		<li>7604.10.00.40</li>
		<li>7604.29.00.11</li>
		<li>7604.29.00.19</li>
		<li>7608.10.00.90</li>
		<li>7608.20.00.00</li>
		<li>7610.90.90.90</li>
	</ul>
	<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">Anti-dumping</span> duties)</h2>

	<h3>Country of origin or export: China</h3>
		
	<p>Information regarding the normal values of subject goods should be obtained from the exporter. The following table identifies the exporters who currently have been issued normal values:</p>
	
	<table class=""table table-bordered"">
		<thead>
			<tr class=""info"">
				<th class=""text-center"" scope=""col"">Exporter</th>
				<th class=""text-center"" scope=""col"">Exporter ID</th>
				<th class=""text-center"" scope=""col"">Cooperative since</th>
				<th class=""text-center"" scope=""col"">Last revised</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td>China Square Industrial Ltd</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2009-03"">2009-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>CSI Solar&nbsp;Co.&nbsp;Ltd.&nbsp;/&nbsp;Canadian Solar Manufacturing (Changshu) Inc.</td>
				<td class=""text-center"">757166400RM0001 / 757200209RM0001</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>C-Link International Trading (Beijing)&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-08"">2015-08</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-08"">2015-08</time></td>
			</tr>
			<tr>
				<td>Guangdong JMA Aluminium Profile Factory (Group)&nbsp;Co.,&nbsp;Ltd.</td>
				<td class=""text-center"">776890402RM0001</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2009-03"">2009-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>Guangdong Luoxiang Aluminium&nbsp;Co.,&nbsp;Ltd.</td>
				<td class=""text-center"">723779906RM0001</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2017-02"">2017-02</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2017-02"">2017-02</time></td>
			</tr>
			<tr>
				<td>Guangdong Suyue Aluminium&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">776946303RM0001</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2018-11"">2018-11</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2018-11"">2018-11</time></td>
			</tr>
			<tr>
				<td>Hangzhou Grand Import&nbsp;&amp;&nbsp;Export Co., Ltd.</td>
				<td class=""text-center"">722662400RM0001</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2022-03"">2022-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2022-03"">2022-03</time></td>
			</tr>
			<tr>
				<td>Metaltek Group&nbsp;Co.,&nbsp;Ltd.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2016-04"">2016-04</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2016-04"">2016-04</time></td>
			</tr>
			<tr>
				<td>Modular Assembly Tech.&nbsp;Co.,&nbsp;Ltd.</td>
				<td class=""text-center"">787513506RM0001</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2017-02"">2017-02</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2017-02"">2017-02</time></td>
			</tr>
			<tr>
				<td>PanAsia Aluminum (China) Limited</td>
				<td class=""text-center"">717814404RM0002</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2009-03"">2009-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>Shanghai Sourcing Ltd./Shangahi CS Manufacturing Co. (China Synergy Group)</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2016-07"">2016-07</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2017-03"">2017-03</time></td>
			</tr>
			<tr>
				<td>Wujiang City Yongheng Aluminum Alloy Industry&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">768494809RM0001</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-07"">2015-07</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-07"">2015-07</time></td>
			</tr>
			<tr>
				<td>Zeus Asian Resource&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2013-07"">2013-07</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2013-07"">2013-07</time></td>
			</tr>
		</tbody>
	</table>
	
	<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own normal values, the anti-dumping duty is equal to 101% of the export price.</p>
</section>
<section>
	<h2>Duty liability<br>
	(Countervailing duties)</h2>

	<h3>Country of origin or export: China</h3>
	<p>Effective on imports of subject goods released by the CBSA on or after 2012-02-20:</p>
	<p>The following table identifies the exporters who currently have a specific amount of subsidy:</p>

	<table class=""table table-bordered"">
		<thead>
			<tr class=""info"">
				<th class=""text-center"" scope=""col"">Exporter</th>
				<th class=""text-center"" scope=""col"">Exporter ID</th>
				<th class=""text-center"" scope=""col"">Amount of Subsidy CNY&nbsp;/&nbsp;KGM</th>
				<th class=""text-center"" scope=""col"">Cooperative since</th>
				<th class=""text-center"" scope=""col"">Last revised</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td>China Square Industrial Ltd</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center"">0.75</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2009-03"">2009-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>CSI Solar&nbsp;Co.&nbsp;Ltd.&nbsp;/&nbsp;Canadian Solar Manufacturing (Changshu) Inc.</td>
				<td class=""text-center"">757166400RM0001 / 757200209RM0001</td>
				<td class=""text-center"">1.84</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>C-Link International Trading (Beijing)&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center"">0.00</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-08"">2015-08</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-08"">2015-08</time></td>
			</tr>
			<tr>
				<td>Guangdong JMA Aluminium Profile Factory (Group)&nbsp;Co.,&nbsp;Ltd.</td>
				<td class=""text-center"">776890402RM0001</td>
				<td class=""text-center"">1.17</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2009-03"">2009-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>Guangdong Suyue Aluminium&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">776946303RM0001</td>
				<td class=""text-center"">0.00</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2018-11"">2018-11</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2018-11"">2018-11</time></td>
			</tr>
			<tr>
				<td>Hangzhou Grand Import&nbsp;&amp;&nbsp;Export Co., Ltd.</td>
				<td class=""text-center"">722662400RM0001</td>
				<td class=""text-center"">0.22</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2022-03"">2022-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2022-03"">2022-03</time></td>
			</tr>
			<tr>
				<td>PanAsia Aluminum (China) Limited</td>
				<td class=""text-center"">717814404RM0002</td>
				<td class=""text-center"">1.75</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2009-03"">2009-03</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2012-02"">2012-02</time></td>
			</tr>
			<tr>
				<td>Wujiang City Yongheng Aluminum Alloy Industry&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">768494809RM0001</td>
				<td class=""text-center"">0.00</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-07"">2015-07</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2015-07"">2015-07</time></td>
			</tr>
			<tr>
				<td>Zeus Asian Resource&nbsp;Co.&nbsp;Ltd.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center"">1.17</td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2013-07"">2013-07</time></td>
				<td class=""text-center""><time class=""nowrap"" datetime=""2013-07"">2013-07</time></td>
			</tr>
		</tbody>
	</table>

	<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own amount of subsidy, the countervailing duty is equal to 15.84 Renminbi per kilogram.</p>
</section>
<section>
	<h2>Disclosure of normal values and amounts of subsidy</h2>
	
	<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a> and from the finding of the CITT. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act to Importers.</i></p>
	
	<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information Required on Customs Documents</h2>

	<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
	
	<p>The import documentation should clearly indicate the following:</p>
	
	<ul>
		<li>Confirmation whether the product is subject to anti-dumping and countervailing duty</li>
		<li>Name and address of producer/manufacturer</li>
		<li>Name and address of vendor (if different from the producer)</li>
		<li>Customer's name and address</li>
		<li>Canadian importer's name and address (if different from the customer)</li>
		<li>Full product description of the goods, including model ID, model description and a physical description of the type/shape of the extrusions, alloy, finish, and whether the goods fall within the scope of subject goods:</li>
		<li>Date of sale, date of shipment</li>
		<li>Quantity (including, the unit of measure)</li>
		<li>Unit selling price, total selling price</li>
		<li>Currency of settlement used (e.g., US$, CDN$, etc.)</li>
		<li>Terms and conditions of sale (e.g., FOB, CIF, etc.)</li>
		<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada from the point of direct shipment (including, the inland and ocean freight, insurance, etc.).</li>
		<li>The amount of any export taxes applicable to the goods.</li>
	</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>

	<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>

	<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Aluminum%20extrusions"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>

	<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

	<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CBSA reference number(s)</h2>

	<ul class=""list-unstyled"">
		<li>Dumping file #: 4214-22</li>
		<li>Dumping case #: AD1379</li>
		<li>Subsidy file #: 4218-26</li>
		<li>Subsidy case #: CV124</li>
		<li>AE 2018 SP</li>
		<li>AE 2019 ER</li>
		<li>AE 2021 UP1</li>
	</ul>
</section>
<section>
	<h2>CITT reference number(s)</h2>

	<ul class=""list-unstyled"">
		<li>NQ-2008-003</li>
		<li>NQ-2008-003R</li>
		<li>RR-2013-003</li>
		<li>RR-2018-008</li>
	</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc&#64;cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/ae-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5"" data-wb-share='{""lnkClass"": ""btn btn-default btn-block""}'></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-08-08</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html10
		const string html10 = @"<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Cold-rolled steel: Measures in force</h1>
</header>

<p>Dumping&nbsp;and subsidizing (China, South&nbsp;Korea, Vietnam)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>CRS</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
		<div class=""well"">
			<p>Cold-reduced flat-rolled sheet products of carbon steel (alloy and non-alloy), in coils or cut lengths, in thicknesses up to 0.142&nbsp;inches (3.61&nbsp;mm) and widths up to 73&nbsp;inches (1854&nbsp;mm) inclusive, originating in or exported from the People’s Republic of China, the Republic of Korea, and the Socialist Republic of Vietnam, and excluding:</p>

			<ol class=""lst-lwr-alph"">
				<li>organic coated (including pre-paint and laminate) and metallic coated steel;</li>
				<li>steel products for use in the manufacture of passenger automobiles, buses, trucks, ambulances or hearses or chassis therefor, or parts thereof, or accessories or parts thereof;</li>
				<li>steel products for use in the manufacture of aeronautic products;</li>
				<li>perforated steel;</li>
				<li>stainless steel;</li>
				<li>silicon electrical steel; and </li>
				<li>tool steel.</li>
			</ol>
		</div>
</section>
<section>
	<h2>Investigations Information</h2>
		<p>The dates of the investigative proceedings and finding concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/crs2018/crs2018-in-eng.html"">Initiation of Investigations</a></td>
					<td>May 25, 2018</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/crs2018/crs2018-pd-eng.html"">Preliminary Determinations</a></td>
					<td>August 23, 2018</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/crs2018/crs2018-fd-eng.html"">Final Determinations</a></td>
					<td>October 31, 2018</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/407833/index.do"">Canadian International Trade Tribunal’s Finding</a></td>
					<td>December 21, 2018</td>
				</tr>
			</tbody> 
		</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
		<p>Prior to January 1, 2022, the subject goods were usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7209.15.00.00</li>
		</ul>

		<p>Beginning January 1, 2022, under the revised customs tariff schedule, subject goods are normally classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7209.16.00.40</li>
			<li>7209.16.00.90</li>
		</ul>

		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">Anti-dumping</span> duties&nbsp;and countervailing Duties)</h2>

	<h3>Country of origin or export: China, South&nbsp;Korea, Vietnam</h3>
	
		<p>Effective on imports of subject goods released by the CBSA on or after December 22, 2018.</p>

		<p>No exporters in China, South&nbsp;Korea or Vietnam received normal values or a specific amount of subsidy following the conclusion of the investigation. For further information please consult the <a href=""/sima-lmsi/i-e/crs2018/crs2018-nf-eng.html"">CBSA Notice of Final Determinations</a>.</p>

		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under SIMA and from the CITT finding. Given that no exporters or producers provided a response to the CBSA’s RFIs, normal values and amount of subsidy will therefore be determined by a ministerial specification under SIMA.</p>

		<p>For importations of subject goods originating in/or exported from China, South&nbsp;Korea or Vietnam, the anti-dumping duty is listed in the table below:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of origin or export</th>
					<th class=""text-center"" scope=""col"">Anti-dumping Duty*</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China - All Exporters</td>
					<td class=""text-right"">91.9%</td>
				</tr>
				<tr>
					<td>South&nbsp;Korea - All Exporters</td>
					<td class=""text-right"">53.0%</td>
				</tr>
				<tr>
					<td>Vietnam - All Exporters</td>
					<td class=""text-right"">99.2%</td>
				</tr>
			</tbody>
		</table>
		<p class=""small"">*As a percentage of export price.</p>

		<p>For importations of subject goods originating in/or exported from China, South&nbsp;Korea or Vietnam, the countervailing duty is listed in the table below:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of origin or export</th>
					<th class=""text-center"" scope=""col"">Amount of Subsidy per Metric Tonne</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China - All Exporters</td>
					<td class=""text-right"">506 Chinese Renminbi</td>
				</tr>
				<tr>
					<td>South&nbsp;Korea - All Exporters</td>
					<td class=""text-right"">86,733 South&nbsp;Korean Won</td>
				</tr>
				<tr>
					<td>Vietnam - All Exporters</td>
					<td class=""text-right"">2,607,988 Vietnamese Dong</td>
				</tr>
			</tbody>
		</table>
		
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information Required on Customs Documents</h2>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>

		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>		
			<li>Confirmation whether the product is subject to anti-dumping and countervailing duties</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description of the goods, including:
				<ul>
					<li>Model ID</li>
					<li>Model description</li>
					<li>Product quality (primes, seconds, etc.)</li>
					<li>Product form (coils, sheet, etc.)</li>
					<li>Steel grade</li>
					<li>Width</li>
					<li>Nominal thickness</li>
					<li>Minimum thickness</li>
					<li>Annealing </li>
					<li>Surface (standard, semi-critical, critical, surface exposed, etc.)</li>
					<li>Finishing (rough matte, regular matte, light matte, etc.)</li>
					<li>Packaging </li>
					<li>Tolerance requirement (width and flatness requirements)</li>
					<li>For automotive use (is product designed specifically for automotive manufacturing?)</li>
				</ul>
			</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
			<li>The amount of any export taxes applicable to the goods</li>
		</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>
		<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Cold-rolled%20steel"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>

		<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CBSA reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>CRS 2018 IN</li>
		</ul>
</section>
<section>
	<h2>CITT reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>NQ-2018-002</li>
			<li>PI-2018-002</li>
		</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc&#64;cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/crs-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5"" data-wb-share='{""lnkClass"": ""btn btn-default btn-block""}'></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2021-12-08</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html11
		const string html11 = @"<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Carbon steel welded pipe: Measures in force</h1>
</header>

<p>Dumping&nbsp;and subsidizing (China)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>CSWP1</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
		<div class=""well"">
			<p>""Carbon steel welded pipe, commonly identified as standard pipe, in the nominal size range of &frac12;&nbsp;inch up to and including 6&nbsp;inches (12.7&nbsp;mm to 168.3&nbsp;mm in outside diameter) inclusive, in various forms and finishes, usually supplied to meet ASTM&nbsp;A53, ASTM&nbsp;A135, ASTM&nbsp;A252, ASTM&nbsp;A589, ASTM&nbsp;A795, ASTM&nbsp;F1083 or Commercial Quality, or AWWA&nbsp;C200-97 or equivalent specifications, including water well casing, piling pipe, sprinkler pipe and fencing pipe, but excluding oil and gas line pipe made to API specifications exclusively, originating in/or exported from the People's Republic of China.""</p>
		</div>

	<h3>Exclusions</h3>
		
		<ul>
			<li>carbon steel welded pipe in nominal pipe sizes of 1&nbsp;inch, meeting the requirements of specification ASTM A53, Grade B, Schedule 10, with a black or galvanized finish, and with plain ends, for use in fire protection applications;</li>
			<li>carbon steel welded pipe in nominal pipe sizes of &frac12; inch to 2&nbsp;inches inclusive, produced using the electric resistance welding process and meeting the requirements of specification ASTM A53, Grade A, for use in the production of carbon steel pipe nipples; and</li>
			<li>carbon steel welded pipe in nominal pipe sizes of &frac12; inch to 6&nbsp;inches inclusive, dual-stencilled to meet the requirements of both specification ASTM A252, Grades 1 to 3, and specification API 5L, with bevelled ends and in random lengths, for use as foundation piles.</li>
		</ul>
</section>
<section>
	<h2>Investigation information</h2>
		<p>The dates of the investigative proceedings and findings concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Initiation of Investigation</td>
					<td>January 23, 2008</td>
				</tr>
				<tr>
					<td>Preliminary Determination</td>
					<td>April 22, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1373/ad1373-i08-fd-eng.html"">Final Determination</a></td>
					<td>July 21, 2008</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353467/index.do"">Canadian International Trade Tribunal's Finding</a></td>
					<td>August 20, 2008</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/ad1373/ad1373-ri10-nc-eng.html"">Re-Investigation</a></td>
					<td>February 14, 2011</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/rr2012-003/rr2012-003-e12-de-eng.html"">Expiry Review Determination</a></td>
					<td>April 4, 2013</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353820/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>August 19, 2013</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/cswp12018/cswp12018-de-eng.html"">Expiry Review Determination</a></td>
					<td>November 2, 2018</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/417109/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>March 28, 2019</td>
				</tr>
			</tbody>
		</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
		<p>Prior to January 1, 2022, the subject goods were usually classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7306.30.00.10</li>
			<li>7306.30.00.20</li>
			<li>7306.30.00.30</li>
		</ul>
		
		<p>Beginning January 1, 2022, under the revised customs tariff schedule, subject goods are normally classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7306.30.00.42</li>
			<li>7306.30.00.43</li>
			<li>7306.30.00.44</li>
			<li>7306.30.00.45</li>
			<li>7306.30.00.46</li>
			<li>7306.30.00.47</li>
			<li>7306.30.00.49</li>
			<li>7306.30.00.52</li>
			<li>7306.30.00.53</li>
			<li>7306.30.00.54</li>
			<li>7306.30.00.55</li>
			<li>7306.30.00.56</li>
			<li>7306.30.00.57</li>
			<li>7306.30.00.59</li>
			<li>7306.30.00.62</li>
			<li>7306.30.00.63</li>
			<li>7306.30.00.64</li>
			<li>7306.30.00.65</li>
			<li>7306.30.00.66</li>
			<li>7306.30.00.67</li>
			<li>7306.30.00.69</li>
		</ul>
		
		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
</section>
<section>
	<h2>Duty liability<br>
	(Anti-dumping&nbsp;and countervailing duties)</h2>

	<h3>Country of origin or export: China</h3>
		
		<p>Effective on imports of subject goods released by the CBSA on or after 2011-02-14:</p>

		<p>No exporters in China received normal values or a specific amount of subsidy in the most recent re-investigation, please consult <a href=""/sima-lmsi/ri-re/ad1373/ad1373-ri10-nc-eng.html"">CBSA Notice of Conclusion of Re-investigation</a>.</p>

		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under SIMA and from the CITT order. Given that no exporters or producers provided a response to the CBSA’s RFI, normal values and amount of subsidy will therefore be determined by a ministerial specification under SIMA.</p>

		<p>For importations of subject goods originating in/or exported from China, the anti-dumping duty is equal to 179% of the export price, as specified by the Minister.</p>

		<p>For importations of subject goods originating in/or exported from China, the countervailing duty is equal to 5,280 Renminbi per metric tonne, as specified by the Minister.</p>

		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information required on customs documents</h2>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li>Harmonized System (HS) classification number</li>
			<li>Confirmation whether the product is subject to anti-dumping and countervailing duties</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description 
				<ul>
					<li>Model ID</li>
					<li>Model description</li>
					<li>Product name and/or number</li>
					<li>Product grade and specification</li>
					<li>Dimension (nominal size) and wall thickness</li>
					<li>Pipe finish</li>
					<li>End finish</li>
				</ul>
			</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure &ndash; e.g. kg, metric tonne)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.) and,</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.).</li>
		</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>
		<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Carbon%20steel%20welded%20pipe"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>

		<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CBSA reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>Dumping file #: 4214-16</li>
			<li>Dumping case #: AD1373</li>
			<li>Subsidy file #: 4218-24</li>
			<li>Subsidy case #: CV123</li>
		</ul>
</section>
<section>
	<h2>CITT reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>NQ-2008-001</li>
			<li>RR-2012-003</li>
		</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc&#64;cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/cswp1-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5"" data-wb-share='{""lnkClass"": ""btn btn-default btn-block""}'></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2021-12-08</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html12
		const string html12 = @"<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Heavy plate: Measures in force</h1>
</header>

<p>Dumping (Chinese&nbsp;Taipei and Germany)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>HP</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
		<div class=""well"">
			<p>Hot-rolled carbon steel plate and high-strength low-alloy steel plate not further manufactured than hot-rolled, heat-treated or not, in cut lengths, in widths greater than 72&nbsp;inches (+/- 1829&nbsp;mm) to 152&nbsp;inches (+/- 3,860&nbsp;mm) inclusive, and thicknesses from 0.375&nbsp;inches (+/- 9.525&nbsp;mm) up to and including 4.5&nbsp;inches (+/- 114.3&nbsp;mm) (with all dimensions being plus or minus allowable tolerances contained in the applicable standards), but excluding:</p>

			<ul>
				<li>plate in coil form, and</li>
				<li>plate having a rolled, raised figure at regular intervals on the surface (also known as floor plate).</li>
			</ul>

			<p>For greater certainty, the subject goods include steel plate which contains alloys greater than required by recognized industry standards, provided the steel does not meet recognized industry standards for an alloy-grade steel plate.</p>
		</div>

	<h3>Exclusions</h3>

		<p>The Canadian International Trade Tribunal excludes the following products from its finding:</p>

		<ol>
			<li>Hot-rolled carbon steel plate manufactured to the following specifications and grades:
				<ul>
					<li>ASME SA-285/SA-285M or ASTM A-285/A-285M</li>
					<li>ASME SA-299/SA-299M or ASTM A-299/A-299M</li>
					<li>ASME SA-515/SA-515M or ASTM A-515/A-515M</li>
					<li>ASME SA-516/SA-516M or ASTM A-516/A-516M (including, but not limited to, SA/A516 Grade 70)</li>
					<li>ASME SA-537/SA-537M or ASTM A-537/A-537M or</li>
					<li>ASME SA-841/SA-841M or ASTM A-841/A-841M</li>
				</ul>
			which is normalized (heat treated) and vacuum degassed (including while molten) with a sulphur content less than or equal to 0.003&nbsp;percent and a phosphorus content less than or equal to 0.017&nbsp;percent, imported exclusively for use in the manufacture of pressure vessels for the oil and gas sector for use in sour service and hydrogen-induced cracking applications</li>
			<li>Hot-rolled carbon steel plate in grade ASME SA-516 Grade 70 or ASTM A-516 Grade 70 normalized (heat treated) with a thickness greater than 3.28&nbsp;inches</li>
			<li>Hot-rolled carbon steel plate produced to the following specifications and grades:
				<ul>
					<li>ASME SA-516/SA-516M or ASTM A-516/A-516M, normalized</li>
					<li>ASME SA-299/SA-299M or ASTM A-299/A-299M, normalized and</li>
					<li>ASME SA-537/SA-537M or ASTM A-537/A-537M, normalized</li>
				</ul>
			in the following dimensions:
				<ul>
					<li>2.5&nbsp;inches thick, greater than or equal to 151&nbsp;inches wide and of any length</li>
					<li>greater than or equal to 3&nbsp;inches thick, greater than or equal to 121&nbsp;inches wide and of any length</li>
					<li>greater than 3.28&nbsp;inches thick of any width and length</li>
				</ul>
			</li>
			<li>Heavy plate imported by Irving Shipbuilding Inc. for use in the Arctic and Offshore Patrols Ships shipbuilding project</li>
		</ol>
</section>
<section>
	<h2>Investigation information</h2>
		<p>The dates of the investigative proceedings and finding concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th>Action</th>
					<th>Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/hp2020/hp2020-in-eng.html"">Initiation of investigation</a></td>
					<td>May 27, 2020</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/hp2020/hp2020-pd-eng.html"">Preliminary decisions</a></td>
					<td>October 9, 2020</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/hp2020/hp2020-nd-eng.html"">Final determination</a></td>
					<td>January 7, 2021</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/492057/index.do"">Canadian International Trade Tribunal's finding</a></td>
					<td>February 5, 2021</td>
				</tr>
			</tbody>
		</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
		<p>Subject goods are normally classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7208.51.00.11</li>
			<li>7208.51.00.12</li>
			<li>7208.51.00.19</li>
		</ul>
		
		<p>Please note that these tariff classification numbers may apply to goods which are not subject to the <cite>Special Import Measures Act</cite> (SIMA) measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under tariff classification numbers that are not listed.</p>

		<p>Refer to the product definition for the authoritative details regarding the subject goods. For more information on the tariff classification numbers, please refer to the <a href=""/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">harmonized commodity description and coding system</a>.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">anti-dumping</span> duties)</h2>

	<h3>Country of origin or export: Chinese&nbsp;Taipei and Germany</h3>
	
	<p>The following table identifies the exporter(s) who currently have been issued normal values. Please refer to the <a href=""/sima-lmsi/mif-mev/id/hp-eng.html"">Normal value model ID table</a> for information relating to model IDs, model descriptions and units of measure. Information regarding specific normal values of subject goods should be obtained from the exporter. Please note that model information is posted only for exporters who have successfully enrolled in an Exporter ID.</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Source</th>
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Exporter ID</th>
					<th class=""text-center"" scope=""col"">Cooperative since</th>
					<th class=""text-center"" scope=""col"">Last revised</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Chinese&nbsp;Taipei</td>
					<td>China Steel Corporation</td>
					<td>783779606RM0001</td>
					<td><time class=""nowrap"" datetime=""2021-01"">2021-01</time></td>
					<td><time class=""nowrap"" datetime=""2021-01"">2021-01</time></td>
				</tr>
				<tr>
					<td>Germany</td>
					<td>AG der Dillinger Hüttenwerke</td>
					<td>779637404RM0001</td>
					<td><time class=""nowrap"" datetime=""2021-01"">2021-01</time></td>
					<td><time class=""nowrap"" datetime=""2021-01"">2021-01</time></td>
				</tr>
			</tbody>
		</table>

		<p>Pursuant to paragraph 41(1)(a) of the <cite>Special Import Measures Act</cite>, the Canada Border Services Agency terminated the dumping investigation in respect of certain heavy plate exported to Canada by:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Source</th>
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Exporter ID</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Turkey</td>
					<td>Ereğli Demir ve Çelik Fabrikaları T.A.Ş. (Erdemir).</td>
					<td>Exporter has not applied</td>
				</tr>
			</tbody>
		</table>

		<p>For importations of subject goods originating in/or exported from Chinese&nbsp;Taipei and Germany for which the exporter has not been issued normal values, the anti-dumping duty is listed in the table below:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Source of origin or export</th>
					<th class=""text-center"" scope=""col"">Margin of dumping rate for all other exporters<sup><span class=""wb-inv"">footnote </span>1</sup></th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Chinese&nbsp;Taipei</td>
					<td>80.6%</td>
				</tr>
				<tr>
					<td>Germany</td>
					<td>68.6%</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td colspan=""2""><sup><span class=""wb-inv"">Footnote </span>1</sup>As a percentage of export price</td>
				</tr>
			</tfoot>
		</table>
</section>
<section>
	<h2>Disclosure of normal values</h2>
		<p>The liability for anti-dumping duty results from the proceeding conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a>. Information regarding the normal value of the subject goods in question and the amount of anti-dumping duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act</i>.</p>
		
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information required on customs documents</h2>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li><b>Confirmation whether the product is subject to SIMA duties</b></li>
			<li>Exporter ID</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Name and location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer’s name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description of the goods, including: 
				<ul>
					<li>Model ID</li>
					<li>Model description</li>
					<li>Product number – Indicate product number assigned to the exporter in the investigation</li>
					<li>Specification – Identify the specification of the product exported to Canada</li>
					<li>Grade – Identify the grade of the product exported to Canada</li>
					<li>Heat treatment - Indicate whether or not the product exported to Canada was heat treated</li>
					<li>Vacuum degassed – Indicate if the product is vacuum degassed (yes/no)</li>
					<li>Product quality – Indicate if the product is prime or secondary product</li>
					<li>Thickness – Indicate the thickness of the product exported to Canada (specified in inches)</li>
					<li>Width – Indicate the width of the product exported to Canada (specified in inches)</li>
					<li>Length – Indicate the length of the product exported to Canada (specified in inches)</li>
				</ul>
			</li>
		</ul>

		<h3>Other relevant characteristics</h3>

		<ul>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g., US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g., FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
			<li>The amount of any export taxes applicable to the goods</li>
		</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>
		<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Heavy%20plate"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>

		<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CITT reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>NQ-2020-001</li>
		</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc&#64;cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/hp-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5"" data-wb-share='{""lnkClass"": ""btn btn-default btn-block""}'></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-06-14</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html13
		const string html13 = @"<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Oil country tubular goods: Measures in force</h1>
</header>

<p>Dumping&nbsp;and subsidizing (China)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>OCTG1</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
		<div class=""well"">
			<p>“Oil country tubular goods including, in particular, casing and tubing, made of carbon or alloy steel, welded or seamless, heat-treated or not heat-treated, regardless of end finish, having an outside diameter from 2 3/8&nbsp;inches to 13 3/8&nbsp;inches (60.3&nbsp;mm to 339.7&nbsp;mm), meeting or supplied to meet API specification 5CT or equivalent standard, in all grades, excluding drill pipe, seamless casing up to 11 3/4&nbsp;inches (298.5&nbsp;mm) in outside diameter, pup joints, welded or seamless, heat-treated or not heat-treated, in lengths of up to 3.66 m (12&nbsp;feet), and coupling stock originating in or exported from China.”</p>
		</div>
</section>
<section>
	<h2>Investigation information</h2>
		<p>The dates of the investigative proceedings and findings concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1385/ad1385-i09-de-eng.html"">Initiation of Investigation</a></td>
					<td>August 24, 2009</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1385/ad1385-i09-pd-eng.html"">Preliminary Determination </a></td>
					<td>November 23, 2009</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/ad1385/ad1385-i09-fd-eng.html"">Final Determination</a></td>
					<td>February 22, 2010</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353474/index.do"">Canadian International Trade Tribunal's Findings</a></td>
					<td>March 23, 2010</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/cv122-125/cv122-ri11-nc-eng.html"">Re-investigation</a></td>
					<td>November 7, 2011</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/rr2014-003/rr2014-003-e14-de-eng.html"">Expiry Review Determination</a></td>
					<td>November 10, 2014</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/354294/index.do"">Canadian International Trade Tribunal's Order </a></td>
					<td>March 2, 2015</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/ad1371-1385-1390-1404/ad1371-1385-1390-1404-ri15-nc-eng.html"">Notice of Conclusion of Re-investigation</a></td>
					<td>December 14, 2015</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/octg1sc2019/octg1sc201901-nc-eng.html"">Normal Value Review – HG Tubulars</a></td>
					<td>March 20, 2019</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/octg1sc2019/octg1sc201902-nc-eng.html"">Normal Value Review – Shandong Molong</a></td>
					<td>May 29, 2019</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/sp-pp/scoctg12019/scoctg12019-sr-eng.html"">Scope ruling&mdash;Statement of Reasons&mdash;Western Alliance Tubulars Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2019-09-06"">September 6, 2019</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/os2019/os2019-nc-eng.html"">Notice of conclusion of re-investigations</a></td>
					<td>May 25, 2020</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/octg12020/octg12020-de-eng.html"">Expiry review determination</a></td>
					<td>July 17, 2020</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/sp-pp/octg12021/octg12021-na-eng.html"">Amended scope ruling&mdash;Western Alliance Tubulars Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2021-11-26"">November 26, 2021</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/octg2022/octg2022-nc-eng.html"">Notice of conclusion of <span class=""nowrap"">re-investigation</span></a></td>
					<td><time class=""nowrap"" datetime=""2022-09-06"">September 6, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/sc2022/sc202201-nc-eng.html"">Notice of conclusion of <span class=""nowrap"">re-investigation</span>&mdash;JingJiang Special Steel Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2022-10-11"">October 11, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/xr-rea/octg12021/octg1202101-nc-eng.html"">Notice of conclusion of expedited review: Golden Ring</a></td>
					<td><time class=""nowrap"" datetime=""2022-12-02"">December 2, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/octg12021/octg1202101-nc-eng.html"">Notice of conclusion of normal value review: Shandong Continental </a></td>
					<td><time class=""nowrap"" datetime=""2022-12-02"">December 2, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/octg12021/octg1202102-nc-eng.html"">Notice of conclusion of normal value review: <abbr>CPTDC</abbr></a></td>
					<td><time class=""nowrap"" datetime=""2022-12-02"">December 2, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/os2022/os2022-nc-eng.html"">Notice of conclusion of <span class=""nowrap"">re-investigation</span></a></td>
					<td><time class=""nowrap"" datetime=""2023-03-17"">March 17, 2023</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/xr-rea/os2022/os2022-nc-eng.html"">Notice of conclusion of expedited review: Linzhou Fengbao Pipe</a></td>
					<td><time class=""nowrap"" datetime=""2023-03-17"">March 17, 2023</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/os2022/os202201-nc-eng.html"">Notice of conclusion of normal value review: Dalipal Pipe Company</a></td>
					<td><time class=""nowrap"" datetime=""2023-04-11"">April 11, 2023</time></td>
				</tr>
			</tbody>
		</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
		<p>Beginning January 1, 2022, under the revised customs tariff schedule, subject goods are normally classified under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>7304.29.00.32</li>
			<li>7304.29.00.33</li>
			<li>7304.29.00.34</li>
			<li>7304.29.00.35</li>
			<li>7304.29.00.36</li>
			<li>7304.29.00.37</li>
			<li>7304.29.00.39</li>
			<li>7304.29.00.42</li>
			<li>7304.29.00.43</li>
			<li>7304.29.00.44</li>
			<li>7304.29.00.45</li>
			<li>7304.29.00.46</li>
			<li>7304.29.00.47</li>
			<li>7304.29.00.49</li>
			<li>7304.29.00.52</li>
			<li>7304.29.00.53</li>
			<li>7304.29.00.54</li>
			<li>7304.29.00.55</li>
			<li>7304.29.00.56</li>
			<li>7304.29.00.57</li>
			<li>7304.29.00.59</li>
			<li>7304.29.00.62</li>
			<li>7304.29.00.63</li>
			<li>7304.29.00.64</li>
			<li>7304.29.00.65</li>
			<li>7304.29.00.66</li>
			<li>7304.29.00.67</li>
			<li>7304.29.00.69</li>
			<li>7304.29.00.72</li>
			<li>7304.29.00.73</li>
			<li>7304.29.00.74</li>
			<li>7304.29.00.75</li>
			<li>7304.29.00.76</li>
			<li>7304.29.00.77</li>
			<li>7304.29.00.79</li>
			<li>7306.29.00.12</li>
			<li>7306.29.00.13</li>
			<li>7306.29.00.14</li>
			<li>7306.29.00.15</li>
			<li>7306.29.00.16</li>
			<li>7306.29.00.17</li>
			<li>7306.29.00.19</li>
			<li>7306.29.00.22</li>
			<li>7306.29.00.23</li>
			<li>7306.29.00.24</li>
			<li>7306.29.00.25</li>
			<li>7306.29.00.26</li>
			<li>7306.29.00.27</li>
			<li>7306.29.00.29</li>
			<li>7306.29.00.32</li>
			<li>7306.29.00.33</li>
			<li>7306.29.00.34</li>
			<li>7306.29.00.35</li>
			<li>7306.29.00.36</li>
			<li>7306.29.00.37</li>
			<li>7306.29.00.39</li>
			<li>7306.29.00.42</li>
			<li>7306.29.00.43</li>
			<li>7306.29.00.44</li>
			<li>7306.29.00.45</li>
			<li>7306.29.00.46</li>
			<li>7306.29.00.47</li>
			<li>7306.29.00.49</li>
			<li>7306.29.00.52</li>
			<li>7306.29.00.53</li>
			<li>7306.29.00.54</li>
			<li>7306.29.00.55</li>
			<li>7306.29.00.56</li>
			<li>7306.29.00.57</li>
			<li>7306.29.00.59</li>
			<li>7306.29.00.62</li>
			<li>7306.29.00.63</li>
			<li>7306.29.00.64</li>
			<li>7306.29.00.65</li>
			<li>7306.29.00.66</li>
			<li>7306.29.00.67</li>
			<li>7306.29.00.69</li>
			<li>7306.29.00.72</li>
			<li>7306.29.00.73</li>
			<li>7306.29.00.74</li>
			<li>7306.29.00.75</li>
			<li>7306.29.00.76</li>
			<li>7306.29.00.77</li>
			<li>7306.29.00.79</li>
		</ul>
		
		<p>Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">Anti-dumping</span> duties)</h2>

	<h3>Country of origin or export: China</h3>
	
		<p>Information regarding the normal values of subject goods should be obtained from the exporter. The following table identifies the exporters who currently have been issued normal values:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Exporter ID</th>
					<th class=""text-center"" scope=""col"">Cooperative since</th>
					<th class=""text-center"" scope=""col"">Last revised</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Huludao City Steel Pipe Industrial&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">789635406RM0001</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Jiangsu Changbao Group
						<ul>
							<li>Jiangsu Changbao Precision Steel Tube&nbsp;Co.,&nbsp;Ltd.</li>
							<li>Jiangsu Changbao Steel Tube&nbsp;Co.,&nbsp;Ltd.</li>
							<li>Jiangsu Changbao Steel Tubulars Corporation</li>
						</ul></td>
					<td class=""text-center"">781312806RM0001</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Jiangsu Chengde Steel Tube Share Company</td>
					<td class=""text-center"">768490708RM0001</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>JingJiang Special Steel Co., Ltd.</td>
					<td class=""text-center"">749538740RM0001</td>
					<td><time class=""nowrap"" datetime=""2022-10"">2022-10</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Shandong Molong Petroleum Machinery&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">749737946RM0001</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Shengli Oilfield Shengji Petroleum Equipment&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">777684101RM0001</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Tianjin Pipe Corporation</td>
					<td class=""text-center"">781449905RM0001</td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Tianjin TianGang Special Petroleum Pipe Manufacture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">749336004RM0001</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Vallourec Tianda (Anhui)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">In progress</td>
					<td><time class=""nowrap"" datetime=""2018-12"">2018-12</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Zibo Freet Thermal Tech&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">786185405RM0001</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Golden Ring Industrial Limited-Liability Company Liaohe Oilfield Panjin</td>
					<td class=""text-center"">715442745RM0001</td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Shandong Continental Petroleum Equipment Co. Ltd.</td>
					<td class=""text-center"">745638544RM0001</td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Dalipal Pipe Company</td>
					<td class=""text-center"">738721349RM0001</td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Petrotex Oil & Gas Equipment Limited</td>
					<td class=""text-center"">In progress</td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Shandong Yonglijinggong Petroleum Equipment Co., Ltd.</td>
					<td class=""text-center"">In progress</td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Shandong Meshine Thermal Tech Co., Ltd.</td>
					<td class=""text-center"">776500811RM0001</td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Bohai Equipment Liaohe Thermal Recovery Machinery</td>
					<td class=""text-center"">In progress</td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Linzhou Fengbao Pipe Industry Co., Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
					<td><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
			</tbody>
		</table>
		<p>Normal values are updated quarterly based on the average quarterly price changes calculated from the monthly <abbr>OCTG</abbr> Price Guide published by <a href=""https://www.argusmedia.com/en/metals/pipe-logix/octg-service"">Argus Pipe Logix</a>. Updates to the normal values take effect on the first day of the second month following each calendar quarter (i.e. February&nbsp;1, May&nbsp;1, August&nbsp;1 and November&nbsp;1).</p>

		<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own normal values, the anti-dumping duty is 166.9% of the export price.</p>
</section>
<section>
	<h2>Duty liability<br>
	(Countervailing duties)</h2>

	<h3>Country of origin or export: China</h3>
		
		<p>The following table identifies the exporters who currently have a specific amount of subsidy:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Exporter ID</th>
					<th class=""text-center"" scope=""col"">Amount of Subsidy CNY&nbsp;/&nbsp;TNE</th>
					<th class=""text-center"" scope=""col"">Cooperative since</th>
					<th class=""text-center"" scope=""col"">Last revised</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Vallourec Tianda (Anhui)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">In progress</td>
					<td>141</td>
					<td><time class=""nowrap"" datetime=""2018-12"">2018-12</time></td>
					<td><time class=""nowrap"" datetime=""2018-12"">2018-12</time></td>
				</tr>
				<tr>
					<td>Hengyang Steel Tube Group Int&rsquo;l Trading Inc.</td>
					<td class=""text-center"">769055203RM0001</td>
					<td>65.14</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Hengyang Valin MPM&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td>65.14</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Hengyang Valin Steel Tube&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">769045808RM0001</td>
					<td>65.14</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Huludao City Steel Pipe Industrial&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">789635406RM0001</td>
					<td>2.20</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Jiangsu Changbao Group – 
						<ul>
							<li>Jiangsu Changbao Precision Steel Tube&nbsp;Co.,&nbsp;Ltd.</li>
							<li>Jiangsu Changbao Steel Tube&nbsp;Co.,&nbsp;Ltd.</li>
							<li>Jiangsu Changbao Steel Tubulars Corporation</li>
						</ul></td>
					<td class=""text-center"">781312806RM0001</td>
					<td>80.43</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2018-06"">2018-06</time></td>
				</tr>
				<tr>
					<td>Jiangsu Chengde Steel Tube Share Company</td>
					<td class=""text-center"">768490708RM0001</td>
					<td>616.52</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>JingJiang Special Steel Co., Ltd.</td>
					<td class=""text-center"">749538740RM0001</td>
					<td>136.68</td>
					<td><time class=""nowrap"" datetime=""2022-10"">2022-10</time></td>
					<td><time class=""nowrap"" datetime=""2022-10"">2022-10</time></td>
				</tr>
				<tr>
					<td>Shandong Molong Petroleum Machinery&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">749737946RM0001</td>
					<td>45.63</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Shengli Oil Field Freet Petroleum Equipment&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td>87.78</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2016-05"">2016-05</time></td>
				</tr>
				<tr>
					<td>Zibo Freet Thermal Tech&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">786185405RM0001</td>
					<td>26.61</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Shengli Oil Field Freet Petroleum Steel Pipe&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">786175000RM0001</td>
					<td>214.21</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Shengli Oil Field Shengji Petroleum Equipment&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">777684101RM0001</td>
					<td>35.95</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Tianjin Pipe Corporation</td>
					<td class=""text-center"">781449905RM0001</td>
					<td>183.75</td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Tianjin TianGang Special Petroleum Pipe Manufacture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">749336004RM0001</td>
					<td>106.78</td>
					<td><time class=""nowrap"" datetime=""2010-03"">2010-03</time></td>
					<td><time class=""nowrap"" datetime=""2015-12"">2015-12</time></td>
				</tr>
				<tr>
					<td>Golden Ring Industrial Limited-Liability Company Liaohe Oilfield Panjin</td>
					<td class=""text-center"">715442745RM0001</td>
					<td>63.08</td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Shandong Continental Petroleum Equipment Co. Ltd.</td>
					<td class=""text-center"">745638544RM0001</td>
					<td>9.97</td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>China Petroleum Technology and Development Corporation</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td>409.87</td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Shandong Meshine Thermal Tech Co., Ltd.</td>
					<td class=""text-center"">776500811RM0001</td>
					<td>9.63</td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Bohai Equipment Liaohe Thermal Recovery Machinery</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td>408.35</td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Dalipal Pipe Company</td>
					<td class=""text-center"">738721349RM0001</td>
					<td>21.31</td>
					<td><time class=""nowrap"" datetime=""2023-04"">2023-04</time></td>
					<td><time class=""nowrap"" datetime=""2023-04"">2023-04</time></td>
				</tr>
			</tbody>
		</table>
		<p>For importations of subject goods originating in/or exported from China for which the exporter has not been issued its own amount of subsidy, the countervailing duty is equal to 4,070 Renminbi per metric tonne.</p>
</section>
<section>
	<h2>Disclosure of Normal Values and Amounts of Subsidy</h2>
		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a> and from the finding of the CITT. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act to Importers.</i></p>
		
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information Required on Customs Documents</h2>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li>Confirmation whether the product is subject to <span class=""nowrap"">anti-dumping</span> duties</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Location of plant/mill of production </li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description
				<ul>
					<li>model ID</li>
					<li>model description</li>
					<li>product type (i.e. welded casing, seamless tubing, vacuum insulated tubing, etc.)</li>
					<li>grade</li>
					<li>outside diameter</li>
					<li>end finish</li>
					<li>gauge (nominal weight in lbs/ft. or Kg/m)</li>
				</ul>
			</li>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure &ndash; e.g. kg, metric tonne)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.)</li>
			<li>The amount of any export taxes applicable to the goods.</li>
		</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>
		<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Oil%20country%20tubular%20goods"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>

		<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CBSA reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>Dumping file #: 4214-26</li>
			<li>Dumping case #: AD1385</li>
			<li>Subsidy file #: 4218-27</li>
			<li>Subsidy case #: CV125</li>
			<li>SC OCTG1 2019 SP</li>
			<li>OS 2019 RI</li>
			<li>OCTG1 2021 SP</li>
			<li>OCTG 2022 RI</li>
			<li>SC 2022 UP1</li>
			<li>OCTG1 2021 XR</li>
			<li>OCTG1 2021 UP1</li>
			<li>OCTG1 2021 UP2</li>
			<li>OS 2022 RI</li>
			<li>OS 2022 XR</li>
		</ul>
</section>
<section>
	<h2>CITT reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>NQ-2009-004</li>
			<li>RR-2014-003</li>
			<li>RR-2019-005</li>
		</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc&#64;cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/octg1-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5"" data-wb-share='{""lnkClass"": ""btn btn-default btn-block""}'></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-06-12</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html14
		const string html14 = @"<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Upholstered domestic seating: Measures in force</h1>
</header>

<p>Dumping and subsidy (China and Vietnam)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>UDS</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
		<div class=""well"">
			<p>Upholstered seating for domestic purposes originating in or exported from the People’s Republic of China and the Socialist Republic of Vietnam, whether motion (including reclining, swivel and other motion features) or stationary, whether upholstered with a covering of leather (either full or partial), fabric (including leather substitutes) or both, including, but not limited to seating such as sofas, chairs, loveseats, sofa beds, day beds, futons, ottomans, stools and home theatre seating (HTS).</p>

			<p>Excluding:</p>

			<ol class=""lst-lwr-alph"">
				<li>Stationary (i.e. non motion) seating upholstered only with fabric (rather than leather), even if the fabric is a leather-substitute (such as leather like or leather-look polyurethane or vinyl)</li>
				<li>dining table chairs or benches (with or without arms) that are manufactured for dining room end-use, which are commonly paired with dining table sets</li>
				<li>upholstered stools with a seating height greater than 24&nbsp;inches (commonly referred to as “bar stools” or “counter stools”), with or without backs, and/or foldable</li>
				<li>seating manufactured for outdoor use (e.g. patio or swing chairs)</li>
				<li>bean bag seating and</li>
				<li>foldable or stackable seating</li>
			</ol>

			<p>For greater certainty, the product definition includes:</p>

			<ol class=""lst-lwr-alph"">
				<li>Upholstered motion seating with reclining, swivel, rocking, zero gravity, gliding, adjustable headrest, massage functions or similar functions</li>
				<li>seating with frames constructed from metal, wood or both</li>
				<li>seating produced as sectional items or parts of sectional items</li>
				<li>seating with or without arms, whether part of sectional items or not and</li>
				<li>foot rests and foot stools (with or without storage)</li>
			</ol>
		</div>

		<p>On September 2, 2021, the Canadian International Trade Tribunal excluded the following products from its finding:</p>

		<ol>
			<li>Specialized reclining massage chairs, not intended to be used for general seating purposes, with padded seat, headrest, back, and footrest, and containing built-in motorized mechanical components that operate by way of computerized controls to provide a full body massage for a single person, including to the head and/or neck, shoulders, back, buttocks, arms, and legs and/or feet</li>
			<li>Medical lift chairs containing electric motion mechanisms and motorized positioning controls, designed to carefully lift, lower and tilt (by raising or lowering the base and back of the seating) the occupant, and otherwise adjust the occupant’s seating position by adjusting one or more of the headrest, footrest, and seat; designed, manufactured, and tested to meet or exceed the requirements of Health Canada’s Medical Devices Regulations (SOR/98-282) applicable thereto and conforming with the following, or equivalent, standards and testing methodologies: EN12182, ANSI/AAMI/ISO10993, ANSI/AAMI/ES60601-1, CAL117, BSEN1021, ISO8191, ANSI/AAMI/ES60601-1-2, ISO14971</li>
			<li>Height-adjustable ergonomic gaming chairs for use with a desk and intended to be used primarily while playing video games, upholstered in leather or a leather-substitute, with armrests, headrests, lumbar support pillows, five-star swivel bases, and wheels or castors</li>
		</ol>
</section>
<section>
	<h2>Investigations information</h2>
		<p>The dates of the investigative proceedings and finding concerning this case are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""/sima-lmsi/i-e/uds2020/uds2020-in-eng.html"">Initiation of investigations</a></td>
					<td><time class=""nowrap"" datetime=""2020-12-21"">December 21, 2020</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/uds2020/uds2020-pd-eng.html"">Preliminary determinations</a></td>
					<td><time class=""nowrap"" datetime=""2021-05-05"">May 5, 2021</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/i-e/uds2020/uds2020-fd-eng.html"">Final determination</a></td>
					<td><time class=""nowrap"" datetime=""2021-08-03"">August 3, 2021</time></td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/512053/index.do"">Canadian International Trade Tribunal Finding</a></td>
					<td><time class=""nowrap"" datetime=""2021-09-02"">September 2, 2021</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202204-nt-eng.html"">Notice of termination of normal value review: Ashley Furniture Industries, LLC</a></td>
					<td><time class=""nowrap"" datetime=""2022-06-10"">June 10, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202202-nc-eng.html"">Conclusion of normal value review: HTL Furniture (China) Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2022-06-17"">June 17, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202205-nt-eng.html"">Notice of termination of normal value review: Ruihao Furniture MFG Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2022-06-27"">June 27, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202206-nt-eng.html"">Notice of termination of normal value review: Henglin</a></td>
					<td><time class=""nowrap"" datetime=""2022-06-27"">June 27, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2021/uds202101-nc-eng.html"">Conclusion of normal value review: Shenzhen</a></td>
					<td><time class=""nowrap"" datetime=""2022-11-14"">November 14, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202201-nc-eng.html"">Conclusion of normal value review: Man Wah Furniture Manufacturing (Huizhou) Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2022-11-30"">November 30, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202203-nc-eng.html"">Conclusion of normal value review: Gu Jia Intelligent Household Jiaxing Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2022-12-05"">December 5, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/xr-rea/uds2021/uds2021-nc-eng.html"">Notice of conclusion of expedited review: Dongguan Advantech Polyurethane Foam Products Co., Ltd., Jiashan Foamtech Furniture Ltd., and Gold Lion Furniture (Shanghai) Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2022-12-05"">December 30, 2022</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202208-nc-eng.html"">Conclusion of normal value review: Dongguan Tianhang Furniture Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2023-02-17"">February 17, 2023</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/xr-rea/uds2022/uds202202-nc-eng.html"">Notice of conclusion of expedited review: Kaiser 2 Furniture Industry (Vietnam) Co., Ltd.</a></td>
					<td><time class=""nowrap"" datetime=""2023-02-28"">February 28, 2023</time></td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/up/uds2022/uds202207-nc-eng.html"">Conclusion of normal value review: Zhejiang Kuka Merlin Furniture Co., Ltd</a></td>
					<td><time class=""nowrap"" datetime=""2023-03-27"">March 27, 2023</time></td>
				</tr>
			</tbody>
		</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
		<p>The subject goods are usually imported under the following tariff classification numbers:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>9401.41.00.00</li>
			<li>9401.49.00.00</li>
			<li>9401.61.10.10</li>
			<li>9401.61.10.90</li>
			<li>9401.71.10.10</li>
			<li>9401.71.10.90</li>
		</ul>
		
		<p>Please note that these tariff classification numbers may apply to goods which are not subject to the <cite>Special Import Measures Act</cite> (SIMA) measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under tariff classification numbers that are not listed.</p>

		<p>Refer to the product definition for the authoritative details regarding the subject goods. For more information on the tariff classification numbers, please refer to the <a href=""/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">harmonized commodity description and coding system</a>.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">Anti-dumping</span> duties)</h2>

	<h3>Country of origin or export: China and Vietnam</h3>
		
		<p>Information regarding the normal values of subject goods should be obtained from the exporter. The following table identifies the exporters who currently have been issued normal values:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Exporter ID</th>
					<th class=""text-center"" scope=""col"">Cooperative since</th>
					<th class=""text-center"" scope=""col"">Last revised</th>
				</tr>
			</thead>
			<tbody>
				<tr class=""active"">
					<td colspan=""4"">China</td>
				</tr>
				<tr>
					<td>Anji Cozy Home&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757988506RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Anji Hengrui Furniture Co.,Ltd.</td>
					<td class=""text-center"">757350103RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Anji Hengyi Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757369509RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Anji UES Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">747979300RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Dongguan Tianhang Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">749778742RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2023-02"">2023-02</time></td>
				</tr>
				<tr>
					<td>Foshan DOB Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">774727606RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Foshan Xingpeichong Huitong Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Gu Jia Intelligent Household Jiaxing&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Haining Fanmei Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">769061201RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>HaiNing Happy Leather Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775697709RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Haining Kendy Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757979505RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Haining Nicelink Home Furnishings&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">738498609RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>(Hangzhou) Huatong Industries Inc.</td>
					<td class=""text-center"">770170306RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Henglin Home Furnishings&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">821321049RM0002</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>HHC Changzhou Corp.</td>
					<td class=""text-center"">770362200RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>HTL Furniture (Huai An)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775591001RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Jason Furniture (Hangzhou)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Jiaxing Motion Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775356207RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Jiaxing Vitra Electrical Technology&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">738497809RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Man Wah Furniture Manufacturing (Huizhou)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">768494205RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-11"">2022-11</time></td>
				</tr>
				<tr>
					<td>Megain Furniture (Dong Guan)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757832506RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Natuzzi (China) Ltd.</td>
					<td class=""text-center"">797369097RM0002</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Ruihao Furniture MFG Co., Ltd</td>
					<td class=""text-center"">757824701RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Shanghai Trayton Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">893640409RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Trayton Furniture (Jiaxing)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775910300RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>UE Furniture Co., Ltd</td>
					<td class=""text-center"">711225110RM0002</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Vanguard Industrial JiaXing&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">770297000RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Violino Furniture (Shenzhen) Ltd.</td>
					<td class=""text-center"">769058405RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-11"">2022-11</time></td>
				</tr>
				<tr>
					<td>Zhejiang Botai Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Zhejiang Chuanyang Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">750294902RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Zhejiang Happy Smart Furnishings&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">770874907RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Zhejiang Kuka Merlin Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2023-03"">2023-03</time></td>
				</tr>
				<tr>
					<td>Dongguan Advantech Polyurethane Foam Products Co., Ltd.</td>
					<td class=""text-center"">725286744RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Jiashan Foamtech Furniture Ltd.</td>
					<td class=""text-center"">766706618RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Gold Lion Furniture (Shanghai) Co. Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr class=""active"">
					<td colspan=""4"">Vietnam</td>
				</tr>
				<tr>
					<td>Delancey Street Furniture Vietnam&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Koda Saigon&nbsp;Co.&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Motomotion Vietnam Limited Company</td>
					<td class=""text-center"">769067000RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Timberland&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">769617507RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>UE Furniture Vietnam&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">746305408RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Vietnam Hang Phong Furniture Company Limited</td>
					<td class=""text-center"">756590204RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Wanek Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">769046400RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Wendelbo SEA JSC</td>
					<td class=""text-center"">758201305RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Kaiser 2 Furniture Industry (Vietnam) Co., Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2023-02"">2023-02</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2023-02"">2023-02</time></td>
				</tr>
				<tr class=""active"">
					<td colspan=""4"">Other</td>
				</tr>
				<tr>
					<td>Ashley Furniture Industries, LLC</td>
					<td class=""text-center"">899015036RM0002</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Restoration Hardware, Inc.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Wendelbo Interiors A/S</td>
					<td class=""text-center"">769430802RM0001</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
			</tbody>
		</table>

		<p>For importations of subject goods for which the exporter has not been issued its own normal values, the rates of anti-dumping duty are equal to:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of origin or export</th>
					<th class=""text-center"" scope=""col"">Margin of dumping rate for all other exporters<sup>1</sup></th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China</td>
					<td class=""text-right"">188.0%</td>
				</tr>
				<tr>
					<td>Vietnam</td>
					<td class=""text-right"">179.5%</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class=""small"" colspan=""2""><sup>1</sup>As a percentage of export price</td>
				</tr>
			</tfoot>
		</table>

		<p>As a result of the Canadian International Trade Tribunal (CITT) finding on September 2, 2021, model-specific normal values for future shipments of subject goods from exporters of certain upholstered domestic seating will be determined pursuant to sections 15 to 23 of the Special Import Measures Act (SIMA) where sufficient information to allow this determination is available to the CBSA. Subject goods for which model-specific normal values have not been established will be determined based on the export price plus an amount equal to the exporter’s weighted average margin of dumping determined at the <a href=""/sima-lmsi/i-e/uds2020/uds2020-nf-eng.html"">final determination</a>. Normal values for any exporter not listed will be determined pursuant to section 29 of SIMA based on the export price as determined under section 24, 25 or 29 of SIMA, plus an amount equal to 188.0% for China or 179.5% for Vietnam of that export price.</p>

		<p>It should be noted that while this methodology will be applied for the purpose of assessing an amount of anti-dumping duty at the time of importation, the normal value and export price of such goods may subsequently be re-determined by a designated officer where a request for redetermination is made, or where the designated officer deems it advisable. Such redeterminations could result in retroactive changes to the amount of anti-dumping duty assessed.</p>
</section>
<section>
	<h2>Duty liability<br>
	(Countervailing duties)</h2>

	<h3>Country of origin or export: China and Vietnam</h3>
		
		<p>The following table identifies the exporters who currently have a specific amount of subsidy:</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Exporter ID</th>
					<th class=""text-center"" scope=""col"">Amount of subsidy per piece</th>
					<th class=""text-center"" scope=""col"">Cooperative since</th>
					<th class=""text-center"" scope=""col"">Last revised</th>
				</tr>
			</thead>
			<tbody>
				<tr class=""active"">
					<td colspan=""5"">China</td>
				</tr>
				<tr>
					<td>Anji Cozy Home&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757988506RM0001</td>
					<td class=""text-center"">15.14 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Anji Hengrui Furniture Co.,Ltd.</td>
					<td class=""text-center"">757350103RM0001</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Anji Hengyi Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757369509RM0001</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Anji UES Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">747979300RM0001</td>
					<td class=""text-center"">15.66 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Dongguan Tianhang Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">749778742RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Foshan DOB Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">774727606RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Foshan Xingpeichong Huitong Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Gu Jia Intelligent Household Jiaxing&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Haining Fanmei Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">769061201RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>HaiNing Happy Leather Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775697709RM0001</td>
					<td class=""text-center"">94.15 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Haining Kendy Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757979505RM0001</td>
					<td class=""text-center"">854.23 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Haining Nicelink Home Furnishings&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">738498609RM0001</td>
					<td class=""text-center"">53.24 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>(Hangzhou) Huatong Industries Inc.</td>
					<td class=""text-center"">770170306RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Henglin Home Furnishings&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">821321049RM0002</td>
					<td class=""text-center"">20.56 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>HHC Changzhou Corp.</td>
					<td class=""text-center"">770362200RM0001</td>
					<td class=""text-center"">47.02 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>HTL Furniture (China)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">817835317RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>HTL Furniture (Huai An)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775591001RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Jason Furniture (Hangzhou)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">21.04 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Jiaxing Motion Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775356207RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Jiaxing Vitra Electrical Technology&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">738497809RM0001</td>
					<td class=""text-center"">17.17 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Man Wah Furniture Manufacturing (Huizhou)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">768494205RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Megain Furniture (Dong Guan)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">757832506RM0001</td>
					<td class=""text-center"">37.36 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Natuzzi (China) Ltd.</td>
					<td class=""text-center"">797369097RM0002</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Ruihao Furniture MFG Co., Ltd</td>
					<td class=""text-center"">757824701RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Shanghai Trayton Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">893640409RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Trayton Furniture (Jiaxing)&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">775910300RM0001</td>
					<td class=""text-center"">181.18 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>UE Furniture Co., Ltd</td>
					<td class=""text-center"">711225110RM0002</td>
					<td class=""text-center"">14.58 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Vanguard Industrial JiaXing&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">770297000RM0001</td>
					<td class=""text-center"">25.03 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Violino Furniture (Shenzhen) Ltd.</td>
					<td class=""text-center"">769058405RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Zhejiang Botai Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">15.70 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Zhejiang Chuanyang Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">750294902RM0001</td>
					<td class=""text-center"">12.19 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Zhejiang Happy Smart Furnishings&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">770874907RM0001</td>
					<td class=""text-center"">82.72 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Zhejiang Kuka Merlin Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">19.36 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Dongguan Advantech Polyurethane Foam Products Co., Ltd.</td>
					<td class=""text-center"">725286744RM0001</td>
					<td class=""text-center"">10.47 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Jiashan Foamtech Furniture Ltd.</td>
					<td class=""text-center"">766706618RM0001</td>
					<td class=""text-center"">2.95 CNY</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr>
					<td>Gold Lion Furniture (Shanghai) Co. Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2022-12"">2022-12</time></td>
				</tr>
				<tr class=""active"">
					<td colspan=""5"">Vietnam</td>
				</tr>
				<tr>
					<td>Delancey Street Furniture Vietnam&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Koda Saigon&nbsp;Co.&nbsp;Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Motomotion Vietnam Limited Company</td>
					<td class=""text-center"">769067000RM0001</td>
					<td class=""text-center"">173,163.43 VND</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Timberland&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">769617507RM0001</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>UE Furniture Vietnam&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">746305408RM0001</td>
					<td class=""text-center"">-<sup>1</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Vietnam Hang Phong Furniture Company Limited</td>
					<td class=""text-center"">756590204RM0001</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Wanek Furniture&nbsp;Co.,&nbsp;Ltd.</td>
					<td class=""text-center"">769046400RM0001</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Wendelbo SEA JSC</td>
					<td class=""text-center"">758201305RM0001</td>
					<td class=""text-center"">-</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Kaiser 2 Furniture Industry (Vietnam) Co., Ltd.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">9,342.24 VND</td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2023-02"">2023-02</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2023-02"">2023-02</time></td>
				</tr>
				<tr class=""active"">
					<td colspan=""5"">Other</td>
				</tr>
				<tr>
					<td>Ashley Furniture Industries, LLC</td>
					<td class=""text-center"">899015036RM0002</td>
					<td class=""text-center"">-<sup>2</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Restoration Hardware, Inc.</td>
					<td class=""text-center"">Exporter has not applied</td>
					<td class=""text-center"">-<sup>2</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
				<tr>
					<td>Wendelbo Interiors A/S</td>
					<td class=""text-center"">769430802RM0001</td>
					<td class=""text-center"">-<sup>2</sup></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
					<td class=""text-center""><time class=""nowrap"" datetime=""2021-08"">2021-08</time></td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class=""small"" colspan=""5"">
						<ol class=""list-unstyled"">
							<li><sup>1</sup>Pursuant to section 2(1) of the Special Import Measures Act (SIMA), an amount of subsidy of less than 1% of the export price of the goods is insignificant for a developed country and of less than 2% of the export price of the goods for a developing country</li>
							<li><sup>2</sup>Please note that these exporters located outside of China and Vietnam are not subject to a specific amount of subsidy but their producers located in China and Vietnam may have received a specific amount of subsidy. Imports from these exporters may be subject to countervailing duties equal to the countervailing duties determined for their producers. Where the producer does not have a specific countervailing duty rate or the producer can not be identified, imports will be subject to the all others countervailing rate for China or Vietnam. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter to avoid unexpected duty liabilities during import</li>
						</ol>
					</td>
				</tr>
			</tfoot>
		</table>

		<p>The CBSA, pursuant to paragraph 41(1)(a) of SIMA, terminated the investigation of subsidizing of certain upholstered domestic furniture originating in or exported from China by Anji Hengrui Furniture&nbsp;Co.,&nbsp;Ltd., Anji Hengyi Furniture&nbsp;Co.,&nbsp;Ltd., Dongguan Tianhang Furniture&nbsp;Co.,&nbsp;Ltd., Foshan DOB Furniture&nbsp;Co.,&nbsp;Ltd., Foshan Xingpeichong Huitong Furniture&nbsp;Co.,&nbsp;Ltd., Gu Jia Intelligent Household Jiaxing&nbsp;Co.,&nbsp;Ltd., Haining Fanmei Furniture&nbsp;Co.,&nbsp;Ltd., (Hangzhou) Huatong Industries Inc., HTL Furniture (China)&nbsp;Co.,&nbsp;Ltd., HTL Furniture (Huai An)&nbsp;Co.,&nbsp;Ltd., Jiaxing Motion Furniture&nbsp;Co.,&nbsp;Ltd., Man Wah Furniture Manufacturing (Huizhou)&nbsp;Co.,&nbsp;Ltd., Natuzzi (China) Ltd., Ruihao Furniture MFG Co., Ltd, Shanghai Trayton Furniture&nbsp;Co.,&nbsp;Ltd., Violino Furniture (Shenzhen) Ltd., and in respect of certain upholstered domestic seating originating in or exported from Vietnam by Delancey Street Furniture Vietnam&nbsp;Co.,&nbsp;Ltd., Koda Saigon&nbsp;Co.&nbsp;Ltd., Timberland&nbsp;Co.,&nbsp;Ltd., UE Vietnam&nbsp;Co.,&nbsp;Ltd., Vietnam Hang Phong Furniture Company Limited, Wanek Furniture&nbsp;Co.,&nbsp;Ltd., and Wendelbo SEA JSC, as there was no subsidizing or the amounts of subsidy were insignificant.</p>

		<p>For importations of subject goods for which the exporter has not been issued a specific rate, the rates of countervailing duty are equal to:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Country of origin or export</th>
					<th class=""text-center"" scope=""col"">Countervailing duty rate for all other exporters<sup>1</sup></th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>China</td>
					<td class=""text-right"">1,390.65 CNY</td>
				</tr>
				<tr>
					<td>Vietnam</td>
					<td class=""text-right"">1,914,726.79 VND</td>
				</tr>
			</tbody>
			<tfoot>
				<tr>
					<td class=""small"" colspan=""2""><sup>1</sup>Amount of subsidy per piece</td>
				</tr>
			</tfoot>
		</table>
</section>
<section>
	<h2>Disclosure of normal values and amounts of subsidy</h2>
		<p>The liability for anti-dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a>. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <i>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the Special Import Measures Act to Importers</i>.</p>
		
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information required on customs documents</h2>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li>Confirmation whether the product is subject to anti-dumping duty and/or countervailing duties</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Name and location of plant/mill of production</li>
			<li>Place from which direct shipment to Canada began</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Country of origin</li>
			<li>Country of export</li>
			<li>Canadian customer’s name and address</li>
			<li>Canadian importer’s name and address (if different from the customer)</li>
			<li>Full product description of the goods, including:
				<ul>
					<li>Model ID</li>
					<li>Model description</li>
					<li>Product type</li>
					<li>Length</li>
					<li>Number of seats</li>
					<li>Cover material</li>
					<li>Padding:
						<ul>
							<li>Maximum foam density of seat cushions</li>
							<li>% memory foam used in seat cushion</li>
							<li>Feathers</li>
						</ul>
					</li>
					<li>Motion mechanisms:
						<ul>
							<li>Number of power recline</li>
							<li>Number of power headrest</li>
							<li>Number of power lumbar</li>
							<li>Number of manual recline</li>
							<li>Number of manual headrest</li>
							<li>Number of rocking, gliding, or swivel mechanisms</li>
							<li>Number of zero gravity mechanisms</li>
							<li>Number and type of additional motion mechanisms</li>
						</ul>
					</li>
					<li>Special features:
						<ul>
							<li>Number of USB outlets</li>
							<li>Number of AC power outlets</li>
							<li>Heating</li>
							<li>Cooling</li>
							<li>Lights (including LEDs)</li>
							<li>Massage&mdash;Mechanical</li>
							<li>Massage&mdash;Air bladder</li>
							<li>Vibration (e.g. for movie and gaming effects)</li>
							<li>Number of speakers</li>
							<li>Number of cup holders</li>
							<li>Storage</li>
							<li>Number and type of additional special features</li>
						</ul>
					</li>
				</ul>
			</li>
		</ul>

		<h3>Other relevant characteristics</h3>
		<ul>
			<li>Date of sale, date of shipment</li>
			<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
			<li>Unit selling price and total selling price to importer in Canada</li>
			<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
			<li>The amount of any export taxes applicable to the goods</li>
		</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA appeals</a>.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>
		<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Upholstered%20domestic%20seating"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>

		<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CBSA reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>UDS 2020 IN</li>
		</ul>
</section>
<section>
	<h2>CITT reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>PI-2020-007</li>
		</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc&#64;cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/uds-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5"" data-wb-share='{""lnkClass"": ""btn btn-default btn-block""}'></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-04-06</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html15
		const string html15 = @"<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Whole potatoes: Measures in force</h1>
</header>

<p>Dumping (United States)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>POT</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
		<div class=""well"">
			<p>""Whole potatoes originating in/or exported from the U.S. for use or consumption in British Columbia.&quot;</p>
		</div>

	<h3>Exclusions</h3>
		
		<ul>
			<li>seed potatoes;</li>
			<li>imports during the period from May 1 to July 31 (inclusive) of each calendar year;</li>
			<li>red potatoes;</li>
			<li>yellow potatoes;</li>
			<li>exotic potato varieties; and</li>
			<li>white and russet potatoes imported in 50-pound cartons in the following count sizes: 40, 50, 60, 70 and 80.</li>
			<li>whole potatoes certified as organic by a recognized certification agency</li>
		</ul>
</section>
<section>
	<h2>Investigation information</h2>
		<p>This case was originally two separate anti-dumping investigations concerning whole potatoes. The subject goods covered in each investigation, as well as the dates of the proceedings and findings, are as follows:</p>
		
		<p>Whole potatoes with netted or russeted skin, excluding seed potatoes, in non-size A, also commonly known as strippers, originating in/or exported from the state of Washington, United States, for use or consumption in British Columbia.</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Initiation of Investigation</td>
					<td>September 30, 1983</td>
				</tr>
				<tr>
					<td>Preliminary Determination</td>
					<td>March 5, 1984</td>
				</tr>
				<tr>
					<td>Tribunal's Finding</td>
					<td>June 4, 1984</td>
				</tr>
				<tr>
					<td>Final Determination</td>
					<td>October 12, 1984</td>
				</tr>
			</tbody>
		</table>

		<p>Whole potatoes, originating in/or exported from the United States, for use or consumption in British Columbia, excluding seed potatoes, and excluding whole potatoes with netted or russeted skin in non-size A, originating in/or exported from the state of Washington.</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>Initiation of Investigation</td>
					<td>October 18, 1985</td>
				</tr>
				<tr>
					<td>Preliminary Determination</td>
					<td>December 20, 1985</td>
				</tr>
				<tr>
					<td>Final Determination</td>
					<td>March 20, 1986</td>
				</tr>
				<tr>
					<td>Canadian International Trade Tribunal's Finding</td>
					<td>April 18, 1986</td>
				</tr>
			</tbody>
		</table>
		
		<p>The dates of the investigative proceedings and findings concerning the joint cases are:</p>
		
		<table class=""table table-bordered"">
			<thead>
				<tr class=""active"">
					<th scope=""col"">Action</th>
					<th scope=""col"">Date</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353773/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>September 14, 1990</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353806/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>September 14, 1995</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353819/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>September 13, 2000</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/ad0689/ad0689nri-eng.html"">Re-Investigation</a></td>
					<td>June 4, 2004</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/rr2004-006/rr2004-006-sor-eng.html"">Expiry Review Determination</a></td>
					<td>April 14, 2005</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353763/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>September 12, 2005</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/ad0689/ad0689-ri09-nc-eng.html"">Re-Investigation</a></td>
					<td>September 25, 2009</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/rr2009-002/rr2009-002-e09-de-eng.html"">Expiry Review Determination</a></td>
					<td>May 14, 2010</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/353759/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>September 10, 2010</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/ri-re/ad0689/ad0689-ri14-nc-eng.html"">Re-Investigation</a></td>
					<td>September 18, 2014</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/rr2014-004/rr2014-004-e14-de-eng.html"">Expiry Review Determination</a></td>
					<td>April 29, 2015</td>
				</tr>
				<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/354345/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>September 9, 2015</td>
				</tr>
				<tr>
					<td><a href=""/sima-lmsi/er-rre/pot2020/pot2020-de-eng.html"">Expiry Review Determination</a></td>
					<td>December 24, 2020</td>
				</tr>
 			<tr>
					<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/514024/index.do"">Canadian International Trade Tribunal's Order</a></td>
					<td>June 2, 2021</td>
				</tr>
			</tbody>
		</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
		<p>The subject goods are usually classified under the following tariff classification number:</p>
		
		<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
			<li>0701.90.00.20</li>
		</ul>
		
		<p>Please note that the classification number may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">Anti-dumping</span> duties)</h2>

	<h3>Country of origin or export: United-States</h3>
		
		<p>Effective on imports of subject goods released by the CBSA on or after 2015-09-09:</p>
		
		<p>No exporters in the United-States received normal values in the most recent re-investigation. <a href=""/sima-lmsi/ri-re/ad0689/ad0689-ri14-nc-eng.html""><b>CBSA Notice of Conclusion of Re-investigation</b></a>.</p>
		
		<p>The liability for anti-dumping duty results from the proceedings conducted under SIMA and from the CITT order. Given that no exporters or producers provided a response to the CBSA’s RFI, normal values will therefore be determined by a ministerial specification under SIMA.</p>

		<p>For importations of subject goods originating in/or exported from the United States, the normal values for all exporters will be based on the total costs and expenses associated with growing and harvesting potatoes, using various United States university cost studies, plus an amount for profit and an estimated amount for packing, administration and selling the goods, as specified by the Minister.</p>

		<p>Effective on imports of subject goods released by the CBSA, information relating to the anti-dumping duty, including normal values, model IDs, model descriptions and units of measure, is listed in the table below.</p>

		<table class=""table table-bordered"">
			<thead>
				<tr class=""info"">
					<th class=""text-center"" scope=""col"">Exporter</th>
					<th class=""text-center"" scope=""col"">Exporter ID</th>
					<th class=""text-center"" scope=""col"">Model ID</th>
					<th class=""text-center"" scope=""col"">Model description</th>
					<th class=""text-center"" scope=""col"">Normal value (USD)</th>
					<th class=""text-center"" scope=""col"">Unit of measure</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td class=""text-center"" rowspan=""8"">All exporters</td>
					<td class=""text-center"" rowspan=""8"">In progress</td>
					<td>5X10 LB FILM/MESH BAGS</td>
					<td>5x10 lb film/mesh bags; 5x10 lb Sac en plastique en filet/en mailles</td>
					<td>$14.63</td>
					<td><abbr>CWT</abbr></td>
				</tr>
				<tr>
					<td>10X5 LB FILM/MESH BAGS</td>
					<td>10x5 lb film/mesh bags; 10x5 lb Sac en plastique en filet/en mailles</td>
					<td>$15.84</td>
					<td><abbr>CWT</abbr></td>
				</tr>
				<tr>
					<td>50 LB CARTON</td>
					<td>50 lb Carton</td>
					<td>$14.69</td>
					<td><abbr>CWT</abbr></td>
				</tr>
				<tr>
					<td>50 LB SACK</td>
					<td>50 lb Sack; 50 lb Sac</td>
					<td>$15.00</td>
					<td><abbr>CWT</abbr></td>
				</tr>
				<tr>
					<td>100 LB SACK</td>
					<td>100 lb Sack; 100 lb Sac</td>
					<td>$12.67</td>
					<td><abbr>CWT</abbr></td>
				</tr>
				<tr>
					<td>10 LB PAPER/POLY BAGS</td>
					<td>10 lb paper/poly bags; 10 lb Sac en plastique/papier</td>
					<td>$14.63</td>
					<td><abbr>CWT</abbr></td>
				</tr>
				<tr>
					<td>15 LB PAPER/POLY BAGS</td>
					<td>15 lb paper/poly bags; 15 lb Sac en plastique/papier</td>
					<td>$14.63</td>
					<td><abbr>CWT</abbr></td>
				</tr>
				<tr>
					<td>20 LB PAPER/POLY BAGS</td>
					<td>20 lb paper/poly bags; 20 lb Sac en plastique/papier</td>
					<td>$14.63</td>
					<td>CWT</td>
				</tr>
			</tbody>
		</table>
		<p>For information on duty assessment, refer to the <a href=""/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information required on customs documents</h2>
		<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
		
		<p>The import documentation should clearly indicate the following:</p>
		
		<ul>
			<li>Indication whether the product is subject to <span class=""nowrap"">anti-dumping</span> duties</li>
			<li>Exporter ID</li>
			<li>Name and address of producer/manufacturer</li>
			<li>Name and address of vendor (if different from the producer)</li>
			<li>Customer's name and address</li>
			<li>Canadian importer's name and address (if different from the customer)</li>
			<li>Full product description including whether the goods fall within the scope of subject goods:
				<p class=""well mrgn-tp-md"">Whole potatoes originating in/or exported from the United States of America for use or consumption in the province of British Columbia, but excluding:</p>
				<ul>
					<li>Seed potatoes;</li>
					<li>Imports during the period from May 1 to July 31, inclusive, of each calendar year;</li>
					<li>Red potatoes;</li>
					<li>Yellow potatoes;</li>
					<li>Exotic potato varieties;</li>
					<li>White and russet potatoes imported in 50-lb. cartons in the following count sizes: 40, 50, 60, 70 and 80; and</li>
					<li>Whole potatoes certified as organic by a recognized certification agency.</li>
				</ul>
			</li>
			<li>Model ID</li>
			<li>Model description</li>
			<li>Date of sale</li>
			<li>Date of shipment</li>
			<li>Product identification name or number (for example, 5/10 lb russet, 10/5 lb russet)</li>
			<li>Quantity (including the unit of measure)</li>
			<li>Unit selling price, total selling price</li>
			<li>Currency of settlement used (e.g., US$, CDN$, etc.)</li>
			<li>Terms and conditions of sale (e.g., FOB, CIF, etc.)</li>
			<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada from the point of direct shipment (including the inland and ocean freight, insurance, etc.).</li>
		</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
		<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""/sima-lmsi/appeals-eng.html"">SIMA Appeals</a> page.</p>
</section>
<section>
	<h2>Email for duty assessment questions</h2>
		<p><a href=""mailto:trade_programs-programmes_commerciaux&#64;cbsa-asfc.gc.ca?subject=Whole%20potatoes"">Trade_Programs-Programmes_commerciaux&#64;cbsa-asfc.gc.ca</a></p>

		<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

		<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CITT reference number(s)</h2>
		<ul class=""list-unstyled"">
			<li>ADT-4-84</li>
			<li>CIT-16-85</li>
			<li>RR-89-010</li>
			<li>RR-94-007</li>
			<li>RR-99-005</li>
			<li>RR-2004-006</li>
			<li>RR-2009-002</li>
			<li>RR-2014-004</li>
			<li>RR-2020-002</li>
		</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc&#64;cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/wp-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5"" data-wb-share='{""lnkClass"": ""btn btn-default btn-block""}'></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-06-14</time></dd>
</dl>
</div>
</main>";
		#endregion

		#region html17 WT
		const string html17 = @"
<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Wind towers: Measures in force</h1>
</header>

<p>Dumping and subsidizing (China)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>WT</abbr></p>
</section>
<section>
	<h2>Product information</h2>

	<h3>Product definition</h3>
		
	<div class=""well"">
		<p>“Certain steel utility wind towers and sections thereof originating in or exported from the People's Republic of China:</p>

		<ol class=""lst-upr-alph"">
			<li>with or without flanges, doors, or internal or external components (e.g., flooring/decking/platforms, ladders, lifts, brackets, electrical busbars, electrical cabling, conduit, cable harness for nacelle generator, interior lighting, tool and storage lockers) attached or adjoined to the wind tower or section, and</li>
			<li>whether or not they are joined with non-subject merchandise, such as nacelles or rotor blades, and whether or not they have internal or external components attached to the subject merchandise</li>
			<li>but excluding
				<ol class=""lst-lwr-rmn"">
					<li>nacelles and rotors (e.g. blades and hubs), regardless of whether they are attached to the wind tower or sections</li>
					<li>Subject to paragraph 1.C.i., flanges, doors and internal or external components which are not attached to the wind towers or sections thereof, unless those components are shipped with the wind towers or sections and are intended to be attached to the wind tower or sections as part of its final assembly or construction</li>
				</ol>
			</li>
		</ol>

		<p>For certainty and clarity,</p>

		<ol class=""lst-upr-alph"">
			<li>The wind towers and sections described at paragraph 1 are designed to, or capable of, supporting the nacelle and rotor blades for a wind turbine with both:
				<ol class=""lst-lwr-rmn"">
					<li>a minimum rated electrical power generation capacity in excess of 100 kilowatts (""kW""), and</li>
					<li>with a minimum height of 50 meters measured from the base of the tower to the bottom of the nacelle (i.e., where the top of the tower and nacelle are joined) when fully assembled</li>
				</ol>
			</li>
			<li>Items described at paragraph 1.A. and attached to the towers or sections thereof are part of the tower or tower sections and within scope unless specifically excluded under paragraph 1.C.</li>
			<li>The goods described at paragraph 1.A. are a non-exhaustive list. The absence of a good from the list does not mean the good is excluded</li>
			<li>The goods described at paragraph 1.A include a kit of fabricated steel components that are designed and intended to be assembled or constructed into a wind tower or section thereof”</li>
		</ol>
	</div>
</section>
<section>
	<h2>Investigations information</h2>
	<p>The dates of the proceedings and finding concerning this case are:</p>
		
	<table class=""table table-bordered"">
		<thead>
			<tr class=""active"">
				<th scope=""col"">Action</th>
				<th scope=""col"">Date</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/wt2023/wt2023-in-eng.html"">Initiation of investigation</a></td>
				<td><time datetime=""2023-04-21"">April 21, 2023</time></td>
			</tr>
			<tr>
				<td>Preliminary determination</td>
				<td><time datetime=""2023-07-20"">July 20, 2023</time></td>
			</tr>
			<tr>
				<td>Final determinations</td>
				<td>October 18, 2023</td>
			</tr>
			<tr>
				<td>Canadian International Trade Tribunal’s Finding</td>
				<td>November 17, 2023</td>
			</tr>
		</tbody>
	</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
	<p>Subject goods are normally classified under the following tariff classification number:</p>
	
	<p class=""mrgn-lft-lg"">7308.20.00.00</p>
	
	<p>However, they can also be imported under the following tariff number, in particular if they are imported with other wind turbine components, such as the nacelle or rotors:</p>
	
	<p class=""mrgn-lft-lg"">8502.31.00.00</p>
	
	<p>Please note that these tariff classification numbers may apply to goods which are not subject to the <cite>Special Import Measures Act</cite> (SIMA) measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under tariff classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>
	
	<p>For more information on the tariff classification numbers, please refer to the Canada Border Services Agency’s (CBSA) <a href=""https://www.cbsa-asfc.gc.ca/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">Harmonized commodity description and coding system</a>.</p>
</section>
<section>
	<h2>Duty liability<br>
	(Provisional <span class=""nowrap"">anti-dumping</span> duties)</h2>

	<h3>Country of origin or export: China</h3>

	<p>Provisional anti dumping duty is payable on subject goods that are released from the CBSA during the period commencing July 20, 2023, and ending on the earlier of the day the dumping investigation is terminated, the day on which the Canadian International Trade Tribunal (CITT) makes an order or finding, or the day an undertaking is accepted.</p>

	<p>For information regarding the rates of provisional anti dumping duty, please consult the CBSA’s <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/wt2023/wt2023-np-eng.html"">Notice of preliminary determinations</a>.</p>

	<p>For importations of subject goods for which the exporter has not been issued a specific rate, the rate of anti dumping duty is equal to 151%, as a percentage of export price.</p>
</section>
<section>
	<h2>Duty liability<br>
	(Provisional countervailing duties)</h2>

	<h3>Country of origin or export: China</h3>

	<p>Provisional countervailing duty is payable on subject goods that are released from the CBSA during the period commencing July 20, 2023, and ending on the earlier of the day the subsidy investigation is terminated, the day on which CITT makes an order or finding, or the day an undertaking is accepted.</p>

	<p>For information regarding the rates of provisional subsidy duty, please consult the CBSA’s <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/wt2023/wt2023-np-eng.html"">Notice of preliminary determinations</a>.</p>

	<p>For importations of subject goods for which the exporter has not been issued a specific rate, the rate of countervailing duty is equal to 42.8%, as a percentage of export price.</p>
</section>
<section>
	<h2>Disclosure of normal values and amounts of subsidy</h2>
	<p>The liability for provisional anti-dumping and countervailing duty results from the proceedings conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a> and the preliminary determinations of the CITT. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti-dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need-to-know basis in accordance with the provisions of <a href=""https://www.cbsa-asfc.gc.ca/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the <cite>Special Import Measures Act</cite> to Importers.</p>

	<p>For information on duty assessment, refer to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information required on customs documents</h2>
	<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
	
	<p>The import documentation should clearly indicate the following:</p>
	
	<ul>
		<li>Confirmation whether the product is subject to provisional duties</li>
		<li>Exporter ID</li>
		<li>Name and address of producer/manufacturer</li>
		<li>Name and location of plant/factory of production</li>
		<li>Place from which direct shipment to Canada began</li>
		<li>Name and address of vendor (if different from the producer)</li>
		<li>Country of origin</li>
		<li>Country of export</li>
		<li>Canadian customer’s name and address</li>
		<li>Canadian importer’s name and address (if different from the customer)</li>
		<li>Full product description of the goods, including:
			<ul>
				<li>Model ID</li>
				<li>Project name</li>
				<li>Product type</li>
				<li>Type of section</li>
				<li>Height of section (or tower)</li>
				<li>Weight of steel plates</li>
				<li>Weight of flanges</li>
				<li>Weight of internals and external components</li>
				<li>Steel plate grade</li>
				<li>Steel plate thickness</li>
				<li>Flanges grade</li>
				<li>Welding treatment</li>
				<li>Degree of metallization</li>
				<li>Paint coating materials</li>
				<li>Number of paint coats internally</li>
				<li>Number of paint coats externally</li>
				<li>Electrical conduit type and material</li>
				<li>Electrical conduit length</li>
				<li>Number of elevators</li>
				<li>Number of platforms and material</li>
				<li>Number of ladders and material</li>
				<li>Number of doors and material</li>
				<li>Other internal or external components</li>
			</ul>
		</li>
		<li>Date of sale, date of shipment</li>
		<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
		<li>Unit selling price and total selling price to importer in Canada</li>
		<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
		<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
		<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
		<li>The amount of any export taxes applicable to the goods</li>
	</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
	<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/subjectivity-decisions-assujettissement-eng.html"">President-level re-determinations</a>.</p>
</section>
<section>
	<h2>Email for subjectivity opinions and duty assessment questions</h2>
	<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca?subject=Wind%20towers"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>

	<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, please review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

	<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CITT reference number(s)</h2>
	<ul class=""list-unstyled"">
		<li>PI-2023-001</li>
	</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""https://www.cbsa-asfc.gc.ca/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc@cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&amp;Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/wt-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5 wb-init wb-share-inited"" data-wb-share=""{&quot;lnkClass&quot;: &quot;btn btn-default btn-block&quot;}"" id=""wb-auto-4""><section id=""shr-pg0"" class=""shr-pg mfp-hide modal-dialog modal-content overlay-def""><header class=""modal-header""><h2 class=""modal-title"">Share this page</h2></header><div class=""modal-body""><ul class=""list-unstyled colcount-xs-2""><li><a href=""https://www.blogger.com/blog_this.pyra?t=&amp;u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;n=Wind%20towers%3A%20Measures%20in%20force"" class=""shr-lnk blogger btn btn-default"" rel=""noreferrer noopener"">Blogger</a></li><li><a href=""https://www.diigo.com/post?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;title=Wind%20towers%3A%20Measures%20in%20force"" class=""shr-lnk diigo btn btn-default"" rel=""noreferrer noopener"">Diigo</a></li><li><a href=""mailto:?to=&amp;subject=Wind%20towers%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html%0A"" class=""shr-lnk email btn btn-default"" rel=""noreferrer noopener"">Email</a></li><li><a href=""https://www.facebook.com/sharer.php?u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;t=Wind%20towers%3A%20Measures%20in%20force"" class=""shr-lnk facebook btn btn-default"" rel=""noreferrer noopener"">Facebook</a></li><li><a href=""https://mail.google.com/mail/?view=cm&amp;fs=1&amp;tf=1&amp;to=&amp;su=Wind%20towers%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html%0A"" class=""shr-lnk gmail btn btn-default"" rel=""noreferrer noopener"">Gmail</a></li><li><a href=""https://www.linkedin.com/shareArticle?mini=true&amp;url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;title=Wind%20towers%3A%20Measures%20in%20force&amp;ro=false&amp;summary=&amp;source="" class=""shr-lnk linkedin btn btn-default"" rel=""noreferrer noopener"">LinkedIn®</a></li><li><a href=""https://www.myspace.com/Modules/PostTo/Pages/?u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;t=Wind%20towers%3A%20Measures%20in%20force"" class=""shr-lnk myspace btn btn-default"" rel=""noreferrer noopener"">MySpace</a></li><li><a href=""https://www.pinterest.com/pin/create/button/?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;media=&amp;description=Wind%20towers%3A%20Measures%20in%20force"" class=""shr-lnk pinterest btn btn-default"" rel=""noreferrer noopener"">Pinterest</a></li><li><a href=""https://reddit.com/submit?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;title=Wind%20towers%3A%20Measures%20in%20force"" class=""shr-lnk reddit btn btn-default"" rel=""noreferrer noopener"">reddit</a></li><li><a href=""https://tinyurl.com/create.php?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html"" class=""shr-lnk tinyurl btn btn-default"" rel=""noreferrer noopener"">TinyURL</a></li><li><a href=""https://www.tumblr.com/share/link?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html&amp;name=Wind%20towers%3A%20Measures%20in%20force&amp;description="" class=""shr-lnk tumblr btn btn-default"" rel=""noreferrer noopener"">tumblr</a></li><li><a href=""https://twitter.com/intent/tweet?text=Wind%20towers%3A%20Measures%20in%20force&amp;url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html"" class=""shr-lnk twitter btn btn-default"" rel=""noreferrer noopener"">Twitter</a></li><li><a href=""https://api.whatsapp.com/send?text=Wind%20towers%3A%20Measures%20in%20force%0A%0Ahttps%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html"" class=""shr-lnk whatsapp btn btn-default"" rel=""noreferrer noopener"">Whatsapp</a></li><li><a href=""https://compose.mail.yahoo.com/?to=&amp;subject=Wind%20towers%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fwt-eng.html%0A"" class=""shr-lnk yahoomail btn btn-default"" rel=""noreferrer noopener"">Yahoo! Mail</a></li></ul><p class=""col-sm-12 shr-dscl"">No endorsement of any products or services is expressed or implied.</p><div class=""clearfix""></div></div></section><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/wt-eng.html#shr-pg0"" aria-controls=""shr-pg0"" class=""shr-opn wb-lbx btn btn-default btn-block wb-lbx-inited wb-init"" id=""wb-auto-5""><span class=""glyphicon glyphicon-share""></span>Share this page</a></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2023-07-20</time></dd>
</dl>
</div>
</main>
";
		#endregion

		#region html18 wt-eng.html
		const string html18 = @"<table class=""wb-tables table table-striped"" data-wb-tables='{""columnDefs"":[{""visible"":false,""targets"":2},{""orderable"":false,""targets"":1}],""paging"":false}'>
	<colgroup>
		<col class=""col-md-4"">
		<col>
		<col>
	</colgroup>
	<thead>
		<tr>
			<th scope=""col"">Case</th>
			<th scope=""col"">Case type</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td class=""sorting_1""><a href=""/sima-lmsi/mif-mev/wt-eng.html"">Wind towers (WT)</a></td>
			<td>Dumping&nbsp;and subsidy: China</td>
		</tr>
	</tbody>
</table>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-05-16</time></dd>
</dl>
</div>";
		#endregion

		#region html19 sg-eng.html
		const string html19 = @"<table class=""wb-tables table table-striped"" data-wb-tables='{""columnDefs"":[{""visible"":false,""targets"":2},{""orderable"":false,""targets"":1}],""paging"":false}'>
	<colgroup>
		<col class=""col-md-4"">
		<col>
		<col>
	</colgroup>
	<thead>
		<tr>
			<th scope=""col"">Case</th>
			<th scope=""col"">Case type</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td class=""sorting_1""><a href=""/sima-lmsi/mif-mev/sg-eng.html"">Steel grating (SG)</a></td>
			<td>Dumping&nbsp;and subsidy: China</td>
		</tr>
	</tbody>
</table>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-06-16</time></dd>
</dl>
</div>";
		#endregion

		#region mat-eng.html
		const string htmlIndexMattresses = @"<table class=""wb-tables table table-striped"" data-wb-tables='{""columnDefs"":[{""visible"":false,""targets"":2},{""orderable"":false,""targets"":1}],""paging"":false}'>
	<colgroup>
		<col class=""col-md-4"">
		<col>
		<col>
	</colgroup>
	<thead>
		<tr>
			<th scope=""col"">Case</th>
			<th scope=""col"">Case type</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><a href=""/sima-lmsi/mif-mev/mat-eng.html"">Mattresses (MAT)</a></td>
			<td>Dumping and subsidy: China</td>
		</tr>
	</tbody>
</table>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified:&#32;</dt>
<dd><time property=""dateModified"">2023-05-16</time></dd>
</dl>
</div>";
		const string htmlMattresses = @"
<main property=""mainContentOfPage"" resource=""#wb-main"" class=""container"" typeof=""WebPageElement"">


<!-- MainContentStart -->

<header>
	<h1 property=""name"" id=""wb-cont"" dir=""ltr"">Mattresses: Measures in force</h1>
</header>

<p>Dumping&nbsp;and subsidizing (China)</p>

<section>
	<h2>Measure in force code (<abbr>MIF</abbr> code)</h2>
	<p><abbr>MAT</abbr></p>
</section>
<section>
	<h2>Product information</h2>
	<h3>Product definition</h3>
	<div class=""well"">
		<p>“Mattresses, mattress toppers, and mattresses for use and incorporation into furniture regardless of size and core type, originating in or exported from the People’s Republic of China, whether imported independently or in a set with a mattress foundation, mattress topper, or both.</p>

		<p>Excluding:</p>

		<ol class=""lst-lwr-alph"">
			<li>pet mattresses</li>
			<li>mattresses which are incorporated into furniture and which are subject to the Canadian International Trade Tribunal’s Finding in NQ-2021-002</li>
			<li>mattress foundations</li>
			<li>tufted futon mattresses which do not include innersprings or foam</li>
			<li>camping mattresses</li>
			<li>stretcher or gurney mattresses</li>
			<li>custom mattresses for boats, RVs, or other vehicles</li>
			<li>airbeds</li>
			<li>water beds and</li>
			<li>mattress toppers less than three inches in thickness”</li>
		</ol>
	</div>
</section>
<section>
	<h2>Investigation information</h2>
	<p>The dates of the proceedings concerning this case are:</p>
	<table class=""table table-bordered"">
		<thead>
			<tr class=""active"">
				<th scope=""col"">Action</th>
				<th scope=""col"">Date</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/mat2022/mat2022-ni-eng.html"">Initiation of investigations</a></td>
				<td><time class=""nowrap"" datetime=""2022-02-24"">February 24, 2022</time></td>
			</tr>
			<tr>
				<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/mat2022/mat2022-np-eng.html"">Preliminary determinations</a></td>
				<td><time class=""nowrap"" datetime=""2022-07-07"">July 7, 2022</time></td>
			</tr>
			<tr>
				<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/i-e/mat2022/mat2022-fd-eng.html"">Final determinations</a></td>
				<td><time class=""nowrap"" datetime=""2022-10-05"">October 5, 2022</time></td>
			</tr>
			<tr>
				<td><a href=""https://decisions.citt-tcce.gc.ca/citt-tcce/a/en/item/521001/index.do"">Canadian International Trade Tribunal’s Finding</a></td>
				<td><time class=""nowrap"" datetime=""2022-11-04"">November 4, 2022</time></td>
			</tr>
			<tr>
				<td><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/up/mat2023/mat202301-ni-eng.html"">Initiation of normal value review: Shenzhen Lantise Tech</a></td>
				<td><time class=""nowrap"" datetime=""2023-08-28"">August 28, 2023</time></td>
			</tr>
		</tbody>
	</table>
</section>
<section>
	<h2>Tariff classification numbers</h2>
	<p>The subject goods are usually classified under the following tariff classification numbers:</p>
	
	<ul class=""list-unstyled colcount-xs-1 colcount-sm-2 colcount-md-4 mrgn-lft-md"">
		<li>9404.21.00.00</li>
		<li>9404.29.00.00</li>
		<li>9404.90.10.50</li>
		<li>9404.90.90.40</li>
	</ul>

	<p>Please note that these tariff classification numbers may apply to goods which are not subject to the <cite>Special Import Measures Act</cite> (SIMA) measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under tariff classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.</p>

	<p>Refer to the product definition for the authoritative details regarding the subject goods.</p>

	<p>For more information on the tariff classification numbers, please refer to the Canada Border Services Agency’s (CBSA) <a href=""https://www.cbsa-asfc.gc.ca/trade-commerce/tariff-tarif/hcdcs-hsdcm/menu-eng.html"">Harmonized Commodity Description and Coding System</a>.</p>
</section>
<section>
	<h2>Duty liability<br>
	(<span class=""nowrap"">Anti-dumping</span> duties)</h2>
	<h3>Country of origin or export: China</h3>	
	<p>The following table identifies the exporters who currently have been issued normal values. Please refer to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/id/mat-eng.html"">Normal value model ID table</a> for information relating to model IDs, model descriptions and units of measure. Information regarding the normal values of subject goods should be obtained from the exporter. Please note that model information is posted only for exporters who have successfully enrolled in an Exporter ID.</p>

	<table class=""table table-bordered"">
		<thead>
			<tr class=""info"">
				<th class=""text-center"" scope=""col"">Exporter</th>
				<th class=""text-center"" scope=""col"">Exporter ID</th>
				<th class=""text-center"" scope=""col"">Cooperative since</th>
				<th class=""text-center"" scope=""col"">Last revised</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td>Dongguan Sinohome Ltd.</td>
				<td class=""text-center"">745281147RM0001</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Foshan EON Technology Industry Co., Ltd.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Foshan Suilong Furniture Co., Ltd.</td>
				<td class=""text-center"">745899344RM0001</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Gold Lion Furniture (Shanghai) Co., Ltd.</td>
				<td class=""text-center"">794336354RM0001</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Guangdong Eonjoy Technology Ltd.</td>
				<td class=""text-center"">783756612RM0001</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Healthcare Co., Ltd.</td>
				<td class=""text-center"">776506214RM0001</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Jinlongheng Furniture Co., Ltd.</td>
				<td class=""text-center"">787301019RM0001</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Sinomax (Zhejiang) Polyurethane Technology Ltd.</td>
				<td class=""text-center"">745346148RM0001</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Zhejiang Glory Home Furnishings Co. Ltd.</td>
				<td class=""text-center"">In progress</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Zinus Xiamen Inc.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Zinus Zhangzhou Inc.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
		</tbody>
	</table>

	<p>Pursuant to paragraph 41(1)(a) of SIMA, the CBSA terminated the dumping investigation in respect of mattresses exported to Canada from:</p>
	
	<table class=""table table-bordered"">
		<thead>
			<tr class=""info"">
				<th class=""text-center"" scope=""col"">Exporter</th>
				<th class=""text-center"" scope=""col"">Exporter ID</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td>Jiaxing Taien Springs Co., Ltd.</td>
				<td class=""text-center"">745089342RM0001</td>
			</tr>
			<tr>
				<td>Xianghe Kaneman Furniture Ltd.</td>
				<td class=""text-center"">745177741RM0001</td>
			</tr>
		</tbody>
	</table>

	<p>For importations of subject goods originating in or exported from China, for which the exporter has not been issued specific normal values, the anti-dumping duty is 146.6% of the export price.</p>
</section>
<section>
	<h2>Duty liability<br>
	(Countervailing duties)</h2>
	<h3>Country of origin or export: China</h3>
	<p>The following table identifies the exporters who currently have a specific amount of subsidy:</p>

	<table class=""table table-bordered"">
		<thead>
			<tr class=""info"">
				<th class=""text-center"" scope=""col"">Exporter</th>
				<th class=""text-center"" scope=""col"">Exporter ID</th>
				<th class=""text-center"" scope=""col"">Amount of subsidy per piece</th>
				<th class=""text-center"" scope=""col"">Cooperative since</th>
				<th class=""text-center"" scope=""col"">Last revised</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td>Foshan EON Technology Industry Co., Ltd.</td>
				<td class=""text-center"">Exporter has not applied</td>
				<td class=""text-center"">3.90&nbsp;CNY</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Foshan Suilong Furniture Co., Ltd.</td>
				<td class=""text-center"">745899344RM0001</td>
				<td class=""text-center"">6.62&nbsp;CNY</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Healthcare Co., Ltd.</td>
				<td class=""text-center"">776506214RM0001</td>
				<td class=""text-center"">4.50&nbsp;CNY</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Jiaxing Taien Springs Co., Ltd.</td>
				<td class=""text-center"">745089342RM0001</td>
				<td class=""text-center"">5.01&nbsp;CNY</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Sinomax (Zhejiang) Polyurethane Technology Ltd.</td>
				<td class=""text-center"">745346148RM0001</td>
				<td class=""text-center"">34.10&nbsp;CNY</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
			<tr>
				<td>Zhejiang Glory Home Furnishings Co. Ltd.</td>
				<td class=""text-center"">In progress</td>
				<td class=""text-center"">5.54&nbsp;CNY</td>
				<td class=""text-center"">2022-11</td>
				<td class=""text-center"">2022-11</td>
			</tr>
		</tbody>
	</table>

	<p>Pursuant to paragraph 41(1)(a) of SIMA, the CBSA terminated the subsidizing investigation in respect of mattresses exported to Canada from:</p>
	
	<table class=""table table-bordered"">
		<thead>
			<tr class=""info"">
				<th class=""text-center"" scope=""col"">Exporter</th>
				<th class=""text-center"" scope=""col"">Exporter ID</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td>Dongguan Sinohome Ltd.</td>
				<td class=""text-center"">745281147RM0001</td>
			</tr>
			<tr>
				<td>Gold Lion Furniture (Shanghai) Co., Ltd.</td>
				<td class=""text-center"">794336354RM0001</td>
			</tr>
			<tr>
				<td>Guangdong Eonjoy Technology Ltd.</td>
				<td class=""text-center"">783756612RM0001</td>
			</tr>
			<tr>
				<td>Jinlongheng Furniture Co., Ltd.</td>
				<td class=""text-center"">787301019RM0001</td>
			</tr>
			<tr>
				<td>Xianghe Kaneman Furniture Ltd.</td>
				<td class=""text-center"">745177741RM0001</td>
			</tr>
			<tr>
				<td>Zinus Xiamen Inc.</td>
				<td class=""text-center"">Exporter has not applied</td>
			</tr>
			<tr>
				<td>Zinus Zhangzhou Inc.</td>
				<td class=""text-center"">Exporter has not applied</td>
			</tr>
		</tbody>
	</table>

	<p>For importations of subject goods originating in or exported from China, for which the exporter has not been issued a specific amount of subsidy, the countervailing duty is equal to 178.61 CNY per piece.</p>
</section>
<section>
	<h2>Disclosure of normal values and amounts of subsidy</h2>
	<p>The liability for anti-dumping and countervailing duties results from the proceeding conducted under <a href=""http://laws-lois.justice.gc.ca/eng/acts/s-15/"">SIMA</a> and from the finding of the CITT. Information regarding the normal value and amount of subsidy of the subject goods in question and the amount of anti dumping and countervailing duty payable should be obtained from the exporter. Related information may be made available to importers on a need to know basis in accordance with the provisions of <a href=""https://www.cbsa-asfc.gc.ca/publications/dm-md/d14/d14-1-2-eng.html"">Memorandum D14-1-2</a>, <cite>Disclosure of Normal Values, Export Prices, and Amounts of Subsidy Established Under the <cite>Special Import Measures Act</cite> to Importers</cite>.</p>

	<p>For information on duty assessment, refer to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/self-auto-eng.html"">Guide for self-assessing SIMA duties</a>.</p>
</section>
<section>
	<h2>Information required on Customs Documents</h2>
	<p>The import documentation should include the information listed below. Failure to provide this information may result in the application of penalties to the importer, pursuant to the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/amps-rsap-eng.html"">Administrative Monetary Penalty System</a> (AMPS).</p>
	
	<p>The import documentation should clearly indicate the following:</p>
	
	<ul>
		<li>Confirmation whether the product is subject to provisional duties</li>
		<li>Name and address of producer/manufacturer</li>
		<li>Name and location of plant/mill of production</li>
		<li>Place from which direct shipment to Canada began</li>
		<li>Name and address of vendor (if different from the producer)</li>
		<li>Country of origin</li>
		<li>Country of export</li>
		<li>Canadian customer’s name and address</li>
		<li>Canadian importer’s name and address (if different from the customer)</li>
		<li>Exporter ID</li>
		<li>MIF code</li>
		<li>Full product description of the goods, including:
			<ul>
				<li><b>Model ID</b></li>
				<li><b>Model description</b></li>
				<li><b>Product type</b></li>
				<li><b>Reversible</b></li>
				<li><b>Mattress dimensions</b>
					<ul>
						<li>Length</li>
						<li>Width</li>
						<li>Total Height</li>
					</ul>
				</li>
				<li>Mattress core
					<ul>
						<li>Type of core</li>
						<li>Core Thickness</li>
						<li>Core Innerspring type</li>
						<li>Core Foam Type</li>
						<li>Core Foam Density</li>
					</ul>
				</li>
				<li>Mattress layers (please provide for each layer)
					<ul>
						<li>Material type</li>
						<li>Foam density</li>
						<li>Thickness</li>
					</ul>
				</li>
				<li>Quilting
					<ul>
						<li>Textile</li>
						<li>Fill type</li>
						<li>Foam density</li>
						<li>Thickness</li>
					</ul>
				</li>
				<li>Ticking/cover
					<ul>
						<li>Textile</li>
						<li>Removable</li>
					</ul>
				</li>
				<li>Packaging</li>
				<li>Additional features</li>
				<li>Brand name and model</li>
			</ul>
		</li>
		<li>Date of sale, date of shipment</li>
		<li>Quantity (state unit of measure, e.g. kilograms, pounds, metric tonnes, etc.)</li>
		<li>Unit selling price and total selling price to importer in Canada</li>
		<li>Currency of settlement used (e.g. US$, CDN$, etc.)</li>
		<li>Terms and conditions of sale (e.g. FOB, CIF, etc.)</li>
		<li>All costs, expenses, and charges incurred by the exporter and vendor in the shipment of the subject goods to Canada (includes inland and ocean freight, insurance, duties, port and handling charges, etc.) and</li>
		<li>The amount of any export taxes applicable to the goods</li>
	</ul>
</section>
<section>
	<h2>Appeal decisions relating to subjectivity</h2>
	<p>Summaries of appeal decisions made by the CBSA respecting whether an imported good is subject to this measure in force can be found on the <a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/subjectivity-decisions-assujettissement-eng.html"">President-level re-determinations</a> page.</p>
</section>
<section>
	<h2>Email for subjectivity opinions and duty assessment questions</h2>
	<p><a href=""mailto:trade_programs-programmes_commerciaux@cbsa-asfc.gc.ca?subject=Mattresses"">Trade_Programs-Programmes_commerciaux@cbsa-asfc.gc.ca</a></p>

	<p><span class=""label label-warning"">Important</span> Prior to submitting a subjectivity opinion request, review the “detailed product information”. Each request must be supported with essential information including, but not limited to, pictures, mill certificates, measurements, origin of the goods, etc. in relation to the specific product in question.</p>

	<p>Failure to provide sufficient information will result in a rejection of the request by the CBSA.</p>
</section>
<section>
	<h2>CITT reference number(s)</h2>
	<ul class=""list-unstyled"">
		<li>NQ-2022-001</li>
	</ul>
</section>

<div class=""row row-no-gutters mrgn-tp-xl"">

<div class=""col-sm-6 col-md-5 col-lg-4"">
<details class=""brdr-0"">
<summary class=""btn btn-default text-center"">Report a problem on this page</summary>
<div class=""well row"">
<div class=""gc-rprt-prblm"">
<div class=""gc-rprt-prblm-frm gc-rprt-prblm-tggl"">
<p>This email is to report problems or inaccuracies on a page. Spam and comments containing offensive language will be reported or deleted. For help with Canada Border Services Agency (CBSA) programs or services, contact <a href=""https://www.cbsa-asfc.gc.ca/contact/bis-sif-eng.html"">border information services</a>.</p>
<a class=""btn btn-primary"" href=""mailto:cbsa.webmaster-webmestre.asfc@cbsa-asfc.gc.ca?Subject=Report%20a%20problem%20on%20a%20page%0D%0A&amp;Body=This%20email%20is%20to%20report%20problems%20or%20inaccuracies%20on%20a%20page.%20Spam%20and%20comments%20containing%20offensive%20language%20will%20be%20reported%20or%20deleted.%20For%20help%20with%20Canada%20Border%20Services%20Agency%20%28CBSA%29%20programs%20or%20services%2C%20contact%20border%20information%20services%20%28https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fcontact%2Fbis-sif-eng.html%29.%0D%0A%0D%0AURL%3A%20http%3A%2F%2Fwww.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/mat-eng.html%0D%0A%0D%0AComments%3A%0D%0A%0D%0A"">Report a problem by email</a>
</div>
</div>
</div>
</details>
</div>
	<div class=""wb-share col-sm-4 col-md-3 col-sm-offset-2 col-md-offset-4 col-lg-offset-5 wb-init wb-share-inited"" data-wb-share=""{&quot;lnkClass&quot;: &quot;btn btn-default btn-block&quot;}"" id=""wb-auto-4""><section id=""shr-pg0"" class=""shr-pg mfp-hide modal-dialog modal-content overlay-def""><header class=""modal-header""><h2 class=""modal-title"">Share this page</h2></header><div class=""modal-body""><ul class=""list-unstyled colcount-xs-2""><li><a href=""https://www.blogger.com/blog_this.pyra?t=&amp;u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;n=Mattresses%3A%20Measures%20in%20force"" class=""shr-lnk blogger btn btn-default"" rel=""noreferrer noopener"">Blogger</a></li><li><a href=""https://www.diigo.com/post?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;title=Mattresses%3A%20Measures%20in%20force"" class=""shr-lnk diigo btn btn-default"" rel=""noreferrer noopener"">Diigo</a></li><li><a href=""mailto:?to=&amp;subject=Mattresses%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html%0A"" class=""shr-lnk email btn btn-default"" rel=""noreferrer noopener"">Email</a></li><li><a href=""https://www.facebook.com/sharer.php?u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;t=Mattresses%3A%20Measures%20in%20force"" class=""shr-lnk facebook btn btn-default"" rel=""noreferrer noopener"">Facebook</a></li><li><a href=""https://mail.google.com/mail/?view=cm&amp;fs=1&amp;tf=1&amp;to=&amp;su=Mattresses%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html%0A"" class=""shr-lnk gmail btn btn-default"" rel=""noreferrer noopener"">Gmail</a></li><li><a href=""https://www.linkedin.com/shareArticle?mini=true&amp;url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;title=Mattresses%3A%20Measures%20in%20force&amp;ro=false&amp;summary=&amp;source="" class=""shr-lnk linkedin btn btn-default"" rel=""noreferrer noopener"">LinkedIn®</a></li><li><a href=""https://www.myspace.com/Modules/PostTo/Pages/?u=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;t=Mattresses%3A%20Measures%20in%20force"" class=""shr-lnk myspace btn btn-default"" rel=""noreferrer noopener"">MySpace</a></li><li><a href=""https://www.pinterest.com/pin/create/button/?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;media=&amp;description=Mattresses%3A%20Measures%20in%20force"" class=""shr-lnk pinterest btn btn-default"" rel=""noreferrer noopener"">Pinterest</a></li><li><a href=""https://reddit.com/submit?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;title=Mattresses%3A%20Measures%20in%20force"" class=""shr-lnk reddit btn btn-default"" rel=""noreferrer noopener"">reddit</a></li><li><a href=""https://tinyurl.com/create.php?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html"" class=""shr-lnk tinyurl btn btn-default"" rel=""noreferrer noopener"">TinyURL</a></li><li><a href=""https://www.tumblr.com/share/link?url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html&amp;name=Mattresses%3A%20Measures%20in%20force&amp;description="" class=""shr-lnk tumblr btn btn-default"" rel=""noreferrer noopener"">tumblr</a></li><li><a href=""https://twitter.com/intent/tweet?text=Mattresses%3A%20Measures%20in%20force&amp;url=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html"" class=""shr-lnk twitter btn btn-default"" rel=""noreferrer noopener"">Twitter</a></li><li><a href=""https://api.whatsapp.com/send?text=Mattresses%3A%20Measures%20in%20force%0A%0Ahttps%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html"" class=""shr-lnk whatsapp btn btn-default"" rel=""noreferrer noopener"">Whatsapp</a></li><li><a href=""https://compose.mail.yahoo.com/?to=&amp;subject=Mattresses%3A%20Measures%20in%20force&amp;body=https%3A%2F%2Fwww.cbsa-asfc.gc.ca%2Fsima-lmsi%2Fmif-mev%2Fmat-eng.html%0A"" class=""shr-lnk yahoomail btn btn-default"" rel=""noreferrer noopener"">Yahoo! Mail</a></li></ul><p class=""col-sm-12 shr-dscl"">No endorsement of any products or services is expressed or implied.</p><div class=""clearfix""></div></div></section><a href=""https://www.cbsa-asfc.gc.ca/sima-lmsi/mif-mev/mat-eng.html#shr-pg0"" aria-controls=""shr-pg0"" class=""shr-opn wb-lbx btn btn-default btn-block wb-lbx-inited wb-init"" id=""wb-auto-5""><span class=""glyphicon glyphicon-share""></span>Share this page</a></div>

</div>
<div class=""pagedetails"">
<dl id=""wb-dtmd"">
<dt>Date modified: </dt>
<dd><time property=""dateModified"">2023-10-17</time></dd>
</dl>
</div>
</main>";
		#endregion
	}
}
