using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class TradeGroupReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var expectedOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FRTradeGroup.xml");
			File.Delete(expectedOutputFile);

			var generator = new TradeGroupReferenceDataFileGeneratorForTest();
			var error = new Errors();
			generator.GenerateFiles(new DateTime(2022, 03, 01), ref error);

			Assert.IsTrue(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains("<Schema>"));
			Assert.IsTrue(outputFileContent.Contains("<DataSource>FR - Trade Groups</DataSource>"));
			Assert.IsTrue(outputFileContent.Contains("<UpdateType>Full</UpdateType>"));
			Assert.IsTrue(outputFileContent.Contains(@"
  <Schema>
    <EntityType Name=""RefCusTradeGroup"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZA_TradeGroup"" />
        <PropertyRef Name=""ZZA_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusTradeGroupCountry"" Type=""RefCusTradeGroupCountry"" />
      <Property Name=""ZZA_Description"" Type=""nvarchar"" />
      <Property Name=""ZZA_EndDate"" Type=""smalldatetime"" ConstantValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZA_StartDate"" Type=""smalldatetime"" />
      <Property Name=""ZZA_TradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
    <EntityType Name=""RefCusTradeGroupCountry"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZB_RN_NKTradeGroupCountryCode"" />
      </Key>
      <Property Name=""ZZB_Description"" Type=""nvarchar"" MaxLength=""200"" />
      <Property Name=""ZZB_EndDate"" Type=""date"" ConstantValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZB_RN_NKTradeGroupCountryCode"" Type=""char"" MaxLength=""2"" />
      <Property Name=""ZZB_StartDate"" Type=""date"" />
    </EntityType>
  </Schema>"));
			Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>UE + pays tiers + DOM</ZZA_Description>
    <ZZA_StartDate>2005-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>FR01</ZZA_TradeGroup>
    <RefCusTradeGroupCountry>
      <ZZB_Description>Andorre</ZZB_Description>
      <ZZB_RN_NKTradeGroupCountryCode>AD</ZZB_RN_NKTradeGroupCountryCode>
      <ZZB_StartDate>1991-07-01T00:00:00</ZZB_StartDate>
    </RefCusTradeGroupCountry>"));
			Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Zimbabwe</ZZA_Description>
    <ZZA_StartDate>1984-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>ZW</ZZA_TradeGroup>
    <RefCusTradeGroupCountry>
      <ZZB_Description>Zimbabwe</ZZB_Description>
      <ZZB_RN_NKTradeGroupCountryCode>ZW</ZZB_RN_NKTradeGroupCountryCode>
      <ZZB_StartDate>1984-01-01T00:00:00</ZZB_StartDate>
    </RefCusTradeGroupCountry>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Overseas Departments</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>DPDOM</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Continental France</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>CONTI</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Corsica</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>CORSE</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Guadeloupe</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>GUADE</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>French Guiana</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>GUYAN</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Martinique</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>MARTI</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Mayotte</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>MAYOT</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		Assert.IsTrue(outputFileContent.Contains(@"
  <RefCusTradeGroup>
    <ZZA_Description>Reunion</ZZA_Description>
    <ZZA_StartDate>1900-01-01T00:00:00</ZZA_StartDate>
    <ZZA_TradeGroup>REUNI</ZZA_TradeGroup>
  </RefCusTradeGroup>"));
		}
	}

	class TradeGroupReferenceDataFileGeneratorForTest : TradeGroupReferenceDataFileGenerator
	{
		protected override List<RefCusTradeGroup> GetDataCollection(DateTime publicationDate, ref Errors error)
		{
			var downloaderMock = new Mock<RITADataDownloader>();
			downloaderMock.Setup(x => x.GetFRTradeGroupsWebPage()).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FRTadeGroupWebPage.html")));
			downloaderMock.Setup(x => x.GetFRTradeGroupCountriesWebPage(It.IsAny<string>())).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FR1005TadeGroupCountriesWebPage.html")));
			downloaderMock.Setup(x => x.GetFRCountriesWebPage()).Returns(File.ReadAllText(Path.Combine(ApplicationConfig.Instance.DownloadDirectory, "FRCountriesWebPage.html")));

			var provider = new RITADataProvider(downloaderMock.Object);
			var tradeGroups = provider.GetFRTradeGroups()
									.Concat(provider.GetFRCountriesAsTradeGroups()).ToList()
									.Concat(RITADataProvider.GetApplicationTerritories())
									.ToList();
			if (!tradeGroups.Any())
			{
				Console.WriteLine("No trade group data could be downloaded from Customs. End of process");
				error = Errors.DownloadErr;
			}
			return tradeGroups;
		}
	}
}
