using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.MedlemsLand;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.TradeGroups.Tests
{
	sealed class TradeGroupsTests
	{
		[Test]
		public void TestCompareUniversalXml()
		{
			var countryXmlData = XmlHelper.ReadDeserializedManifestResourceContent<LandgruppeListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.TradeGroups.Testfiles.Input.landgruppe.xml");
			var memberXmlData = XmlHelper.ReadDeserializedManifestResourceContent<MedlemLandListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.TradeGroups.Testfiles.Input.medlemsland.xml");

			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "04/22/2022 23:02:06", "01/01/2020 12:12:12", outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.TradeGroups.Testfiles.Output.RefCusTradeGroup_NO.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.Empty);
				Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
			});
		}

		[Test]
		public void TestEmptyTradeGroupCode()
		{
			AssertTradeGroupError(string.Empty, "Avgift fra alle land", "1990-01-01", "2009-01-01");
		}

		[Test]
		public void TestEmptyTradeGroupDescription()
		{
			AssertTradeGroupError("ALLE", string.Empty, "1990-01-01", "2009-01-01");
		}

		[Test]
		public void TestTradeGroupStartDateError()
		{
			AssertTradeGroupError("ALLE", "Avgift fra alle land", "1990-23-01", "2009-01-01");
		}

		[Test]
		public void TestTradeGroupStartBeforeEndError()
		{
			AssertTradeGroupError("ALLE", "Avgift fra alle land", "2022-12-31", "2022-01-01-");
		}

		[Test]
		public void TestTradeGroupEndDateError()
		{
			AssertTradeGroupError("ALLE", "Avgift fra alle land", "1990-01-01", "2009-28-02");
		}

		[Test]
		public void TestTradeMemberEmptyCountry()
		{
			AssertTradeGroupMemberError(string.Empty, "1990-01-01", "2009-01-01");
		}

		[Test]
		public void TestTradeMemberStartDateError()
		{
			AssertTradeGroupMemberError("MH", "1990-23-01", "2009-01-01");
		}

		[Test]
		public void TestTradeMemberEndDateError()
		{
			AssertTradeGroupMemberError("MH", "1990-01-01", "2009-28-02");
		}

		[Test]
		public void TestTradeMemberStartBeforeEndError()
		{
			AssertTradeGroupMemberError("MH", "2022-12-31", "2022-12-30");
		}

		[Test]
		public void TestTradeGroupMemberModifiedDateError()
		{
			var countryXmlData = SetupTradeGroup("ALD1", "Avgift fra alle land unntatt Europa", "1990-01-01", "2009-01-01");
			var memberXmlData = SetupTradeGroupMember("MH", "1990-01-01", "2009-01-01");
			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "04/22/2022 23:02:06", "13/13/2021 35:12:12", outputTempFileForTest);
			Assert.That(errors, Does.StartWith("Failed to parse lastUpdated DateTime 13/13/2021 35:12:12 from Tradegroup Members file"));
		}

		[Test]
		public void TestTradeGroupModifiedDateError()
		{
			var countryXmlData = SetupTradeGroup("ALD1", "Avgift fra alle land unntatt Europa", "1990-01-01", "2009-01-01");
			var memberXmlData = SetupTradeGroupMember("MH", "1990-01-01", "2009-01-01");
			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "31/32/2022 27:02:06", "01/01/2020 12:12:12", outputTempFileForTest);
			Assert.That(errors, Does.StartWith("Failed to parse lastUpdated DateTime 31/32/2022 27:02:06 from Tradegroups file"));
		}

		[Test]
		public void TestModifiedDateMemberGroupMoreRecent()
		{
			const string expectedPublicationTime = @"<UniversalReferenceData>
  <DataSource>NO TradeGroups</DataSource>
  <PublicationTime>2022-01-09T00:00:00</PublicationTime>";

			var countryXmlData = SetupTradeGroup("ALD1", "Avgift fra alle land unntatt Europa", "1990-01-01", "2009-01-01");
			var memberXmlData = SetupTradeGroupMember("MH", "1990-01-01", "2009-01-01");
			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "01/09/2022 00:00:00", "01/06/2022 00:00:00", outputTempFileForTest);
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);
			Assert.Multiple(() =>
			{
				Assert.That(actualUniversalXml, Does.StartWith(expectedPublicationTime));
				Assert.That(errors, Is.Empty);
			});
		}

		[Test]
		public void TestModifiedDateTradeGroupMoreRecent()
		{
			const string expectedPublicationTime = @"<UniversalReferenceData>
  <DataSource>NO TradeGroups</DataSource>
  <PublicationTime>2022-01-07T12:00:00</PublicationTime>";

			var countryXmlData = SetupTradeGroup("ALD1", "Avgift fra alle land unntatt Europa", "1990-01-01", "2009-01-01");
			var memberXmlData = SetupTradeGroupMember("MH", "1990-01-01", "2009-01-01");
			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "01/07/2022 12:00:00", "01/06/2022 00:00:00", outputTempFileForTest);
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);
			Assert.Multiple(() =>
			{
				Assert.That(actualUniversalXml, Does.StartWith(expectedPublicationTime));
				Assert.That(errors, Is.Empty);
			});
		}

		[Test]
		public void TestTALLTradeGroupMemberCountries()
		{
			var countryXmlData = SetupTradeGroup("TALL", "Toll fra alle land (ordinær toll)", "2000-01-01", "2079-06-06");
			var memberXmlData = SetupTradeGroupMember("MH", "1990-01-01", "2009-01-01");
			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "01/07/2022 12:00:00", "01/06/2022 00:00:00", outputTempFileForTest);
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.Multiple(() =>
			{
				var xmlDoc = XDocument.Parse(actualUniversalXml);

				var tradeGroupCountry = xmlDoc.Descendants("RefCusTradeGroupCountry").FirstOrDefault();
				Assert.NotNull(tradeGroupCountry);

				var zzbEndDate = tradeGroupCountry.Element("ZZB_EndDate")?.Value;
				var zzbTradeGroupCountryCode = tradeGroupCountry.Element("ZZB_RN_NKTradeGroupCountryCode")?.Value;
				var zzbStartDate = tradeGroupCountry.Element("ZZB_StartDate")?.Value;

				Assert.That(zzbEndDate, Is.EqualTo("2079-06-06T23:59:00"));
				Assert.That(zzbTradeGroupCountryCode, Is.EqualTo("MH"));
				Assert.That(zzbStartDate, Is.EqualTo("2000-01-01T00:00:00"));
			});
		}

		[SetUp]
		public void Setup()
		{
			outputTempFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		[TearDown]
		public void TearDown()
		{
			outputTempFileForTest.DeleteTestOutput();
		}
		string outputTempFileForTest;

		void AssertTradeGroupError(string tradeGroup, string description, string startDate, string endDate)
		{
			var expectedErrorString = $@"Unable to parse TradeGroup due to empty code, empty description or invalid Dates.
DETAILS:
Trade Group: {tradeGroup}
Description: {description}
Start Date: {startDate}
End Date: {endDate}
";
			var countryXmlData = SetupTradeGroup(tradeGroup, description, startDate, endDate);
			var memberXmlData = XmlHelper.ReadDeserializedManifestResourceContent<MedlemLandListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.TradeGroups.Testfiles.Input.medlemsland.xml");

			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "04/22/2022 23:02:06", "01/01/2020 12:12:12", outputTempFileForTest);
			Assert.That(errors, Is.EqualTo(expectedErrorString));
		}

		void AssertTradeGroupMemberError(string memberCountry, string startDate, string endDate)
		{
			var expectedErrorString = $@"Unable to parse TradeGroup Member due to empty code, empty description or invalid Dates.
DETAILS:
Trade Group: ALD1
Member: {memberCountry}
Start Date: {startDate}
End Date: {endDate}
";
			var countryXmlData = SetupTradeGroup("ALD1", "Avgift fra alle land unntatt Europa", "1990-01-01", "2009-01-01");
			var memberXmlData = SetupTradeGroupMember(memberCountry, startDate, endDate);

			var errors = TradeGroupsParser.ConvertToXMLFile(countryXmlData, memberXmlData, "04/22/2022 23:02:06", "01/01/2020 12:12:12", outputTempFileForTest);
			Assert.That(errors, Is.EqualTo(expectedErrorString));
		}

		static LandgruppeListe SetupTradeGroup(string tradeGroup, string description, string startDate, string endDate)
		{
			return new LandgruppeListe
			{
				CountryGroup = new Services.TradeGroups.LandGruppe.Landgruppe[]
				{
					new Services.TradeGroups.LandGruppe.Landgruppe()
					{
						CountryGroupCode = tradeGroup,
						CountryGroupName = description,
						PreferenceCode= "N",
						DateStart = startDate,
						DateEnd = endDate
					}
				}
			};
		}

		static MedlemLandListe SetupTradeGroupMember(string memberCountry, string startDate, string endDate)
		{
			return new MedlemLandListe
			{
				Member = new Medlemsland[]
				{
					new Medlemsland()
					{
						CountryCode = memberCountry,
						CountryName = "Marshalløyene",
						CountryGroups = new Services.TradeGroups.MedlemsLand.Landgruppe[]
						{
							new Services.TradeGroups.MedlemsLand.Landgruppe()
							{
								CountryGroupCode = "ALD1",
								DateStart = startDate,
								DateEnd = endDate
							}
						}
					}
				}
			};
		}
	}
}
