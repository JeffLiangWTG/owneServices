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
	public class SIMATextExtractorFixture
	{
		[Test]
		public void ExtractCountryOfOriginOrExport()
		{
			var helper = new Mock<IHttpClientHelper>();
			helper.Setup(x => x.GetMatchedEntityCodesAsync("XXX", It.IsAny<string[]>()))
				.Returns<string, string[]>((url, inputs) =>
				{
					if (inputs[0] == "China")
					{
						return Task.FromResult(new[] { "CN" });
					}
					else if (inputs[0] == "Republic of Korea")
					{
						return Task.FromResult(new[] { "KR" });
					}
					else if (inputs[0] == "European Union")
					{
						return Task.FromResult(new[] { "EU" });
					}
					return Task.FromResult(new string[0]);
				}
			);
			var extractor = new SIMATextExtractor(helper.Object, "XXX");
			Assert.AreEqual(new[] { "CN" }, extractor.ExtractCountryOfOriginOrExport(new[] { "China" }));
			Assert.AreEqual(new[] { "KR" }, extractor.ExtractCountryOfOriginOrExport(new[] { "Republic of Korea" }));
			Assert.AreEqual(new[] { "31" }, extractor.ExtractCountryOfOriginOrExport(new[] { "European Union" }));
		}

		[Test]
		public void ExtractClassificationNumbers()
		{
			var extractor = new SIMATextExtractor(new Mock<IHttpClientHelper>().Object, "http://");
			Assert.AreEqual(new[] { "7306.19.00.90", "7304.19.00.10", "7304.19.00.20" }, extractor.ExtractClassificationNumbers(@"
Prior to January 1, 2017, the subject goods were usually classified under the following tariff classification numbers:

7306.19.00.90
Beginning January 1, 2017, under the revised customs tariff schedule, subject goods are normally classified under the following tariff classification numbers:

7304.19.00.10
7304.19.00.20
7304.19.00.10
7304.19.00.20
Please note that these tariff classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.
"));

			Assert.AreEqual(new[] { "7324.10.00.10", "7324.10.00.90" }, extractor.ExtractClassificationNumbers(@"
The subject goods are usually classified under the following tariff classification numbers:

7324.10.00.10
7324.10.00.90
Please note that these classification numbers may apply to goods which are not subject to SIMA measures, may change because of amendments to the Departmental Consolidation of the Customs Tariff, or the subject goods may be imported under HS classification numbers that are not listed. Refer to the product definition for the authoritative details regarding the subject goods.
"));
		}

		[Test]
		public void ExtractDutyType()
		{
			var extractor = new SIMATextExtractor(new Mock<IHttpClientHelper>().Object, "http://");
			Assert.AreEqual(new[] { "ADD" }, extractor.ExtractDutyTypes(@"
the anti-dumping duty is 103.1% of the export price").ToArray());
			Assert.AreEqual(new[] { "CVD" }, extractor.ExtractDutyTypes(@"
the countervailing duty is equal to 264.94 Renminbi per unit.").ToArray());
		}

		[Test]
		public void ExtractEffectiveDate()
		{
			var extractor = new SIMATextExtractor(new Mock<IHttpClientHelper>().Object, "http://");
			Assert.AreEqual("2014-04-01", extractor.ExtractEffectiveDate(@"
Effective on imports of subject goods released by the CBSA on or after 2014-04-01"));
			Assert.AreEqual("January 5, 2018", extractor.ExtractEffectiveDate(@"
Effective on imports of subject goods released by the CBSA on or after January 5, 2018."));
			Assert.AreEqual("August 16, 2018", extractor.ExtractEffectiveDate(@"
Provisional countervailing duty is payable on subject goods that are released from the CBSA during the period commencing August 16, 2018, and ending on the earlier of the day the subsidy investigation is terminated, the day on which CITT makes an order or finding, or the day an undertaking is accepted"));
		}

		[Test]
		public void ExtractDeterminationDate()
		{
			var extractor = new SIMATextExtractor(new Mock<IHttpClientHelper>().Object, "http://");
			Assert.AreEqual("April 24, 2012", extractor.ExtractDeterminationDate(@"
The dates of the investigative proceedings and findings concerning this case are:

Action	Date
Initiation of Investigation
October 27, 2011
Preliminary Determination
January 25, 2012
Final Determination
April 24, 2012
Canadian International Trade Tribunal's Finding
May 24, 2012
Conclusion of Re-Investigation
April 1, 2014
Conclusion of Re-Investigation
July 7, 2016"));
			Assert.AreEqual("August 2, 2012", extractor.ExtractDeterminationDate(@"
The dates of the investigative proceedings and findings concerning this case are:

Action	Date
Initiation of Investigation
May 4, 2012
Preliminary Determination
August 2, 2012
Canadian International Trade Tribunal's Finding
November 30, 2012
Expiry Review Determination
January 25, 2018
Canadian International Trade Tribunal's Order
July 4, 2018"));

			Assert.AreEqual("August 3, 2012", extractor.ExtractDeterminationDate(@"
The dates of the investigative proceedings and findings concerning this case are:

Action	Date
Initiation of Investigation
May 4, 2012
Preliminary decisions
August 3, 2012
Canadian International Trade Tribunal's Finding
November 30, 2012
Expiry Review Determination
January 25, 2018
Canadian International Trade Tribunal's Order
July 4, 2018"));
		}

		[Test]
		public void ExtractDutyRateUsingWebService()
		{
			var httpClientHelperMock = new Mock<IHttpClientHelper>();
			httpClientHelperMock.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "turkish lira" }))
				.Returns(Task.FromResult(new string[] { "TRY" }));
			httpClientHelperMock.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "brazilian real" }))
				.Returns(Task.FromResult(new string[] { "BRL" }));
			httpClientHelperMock.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "kilogram" }))
				.Returns(Task.FromResult(new string[] { "KGM" }));
			httpClientHelperMock.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "CNY" }))
				.Returns(Task.FromResult(new string[] { "CNY" }));

			var extractor = new SIMATextExtractor(httpClientHelperMock.Object, "http://");
			Assert.AreEqual(Tuple.Create("0.09 * [KGM]", "TRY"), extractor.ExtractDutyRate(@"
All Others Countervailing Duty Rate for Future Shipments
Turkey
0.09 Turkish Lira per  kilogram.")); // Please don't remove any space in this string, they're for special symbol removal test.
			Assert.AreEqual(Tuple.Create("76360.47 * [KGM]", "BRL"), extractor.ExtractDutyRate(@"
Countervailing Duty Rate for Future Shipments
(Amount of Subsidy per Kilogram)
Vietnam
76,360.47 Brazilian  Real")); // Please don't remove any space in this string, they're for special symbol removal test.
			Assert.AreEqual(Tuple.Create("1390.65 * [NMB]", "CNY"), extractor.ExtractDutyRate(@"
			Country of origin or export Countervailing duty rate for all other exporters1
			1,390.65
			CNY"));
		}

		[Test]
		public void ExtractDutyRateIgnoreMatchesWithUnitCodeLengthGreaterThan15()
		{
			var httpClientHelperMock = new Mock<IHttpClientHelper>();
			httpClientHelperMock.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "tl" }))
				.Returns(Task.FromResult(new string[] { "TRY" }));
			httpClientHelperMock.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), new string[] { "kg" }))
				.Returns(Task.FromResult(new string[] { "KGM" }));

			var extractor = new SIMATextExtractor(httpClientHelperMock.Object, "http://");
			Assert.AreEqual(Tuple.Create("0.09 * [KGM]", "TRY"), extractor.ExtractDutyRate(@"As Durum Gıda Sanayi ve Ticaret A.Ş. (Durum Gida) benefitted from prohibited export subsidies, countervailing duty of 0.01 TL/ KG must offset any anti-dumping duties payable for importations from Durum Gida.

For importations of subject goods originating in/or exported from Turkey for which the exporter has not been issued its own amount of subsidy, the countervailing duty is equal to 0.09 TL/KG."));
		}

		[Test]
		public void TestExtractDutyRateFailure()
		{
			var httpClientHelperMock = new Mock<IHttpClientHelper>();
			var extractor = new SIMATextExtractor(httpClientHelperMock.Object, "http://");
			Assert.AreEqual(Tuple.Create("UNDEFINED", ""), extractor.ExtractDutyRate(@"For importations of subject goods originating in/or exported from the United States, the normal values for all exporters will be based on the total
costs and expenses associated with growing and harvesting potatoes, using various United States university cost studies, plus an amount for profit and an estimated amount for packing, administration and selling the goods, as specified by the Minister."));
		}

		[Test]
		public void ExtractDumpingCase()
		{
			var extractor = new SIMATextExtractor(new Mock<IHttpClientHelper>().Object, "http://");
			Assert.AreEqual("AD1373", extractor.ExtractDumpingCase(@"/sima-lmsi/i-e/ad1373/ad1373-i08-fd-eng.html"));
			Assert.AreEqual("GB2016", extractor.ExtractDumpingCase(@"/sima-lmsi/i-e/gb2016/gb2016-np-eng.html"));
			Assert.AreEqual("OCTG42021", extractor.ExtractDumpingCase(@"/sima-lmsi/i-e/octg42021/octg42021-np-eng.html"));
		}
	}
}
