using System;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class NationalRateCodeUsageUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void CheckOutputFile()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			string[] tariffCodeList = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\", "TariffList.txt")).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			var expectedOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FRNationalRateCodeUsage.xml");
			File.Delete(expectedOutputFile);

			var generatorMoq = new Mock<NationalRateCodeUsageUniversalReferenceDataFileGenerator>();
			generatorMoq.CallBase = true;
			generatorMoq.Protected().Setup<RefCusCodeList[]>("GetExistingRateCodeUsages", ItExpr.IsAny<string>()).Returns(Array.Empty<RefCusCodeList>());
			var generator = generatorMoq.Object;
			var outputFileCount = generator.GenerateURDFiles(tariffCodeList, Common.UniversalXmlWriter.UpdateType.Full, new DateTime(2022, 04, 13));

			Assert.AreEqual(1, outputFileCount);
			Assert.That(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.That(outputFileContent.Contains("<DataSource>FR - National Rate Code Usage</DataSource>"));
			Assert.That(outputFileContent.Contains(@"<Schema>
    <EntityType Name=""RefCusCodeList"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZD_Code"" />
        <PropertyRef Name=""ZZD_ZZK_NKCodeType"" />
        <PropertyRef Name=""ZZD_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZD_Code"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZD_Description"" Type=""nvarchar"" MaxLength=""2000"" />
      <Property Name=""ZZD_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZD_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZD_ZZK_NKCodeType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZD_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
  </Schema>"));
		Assert.That(outputFileContent.Contains(@"<RefCusCodeList>
    <ZZD_Code>0101210000</ZZD_Code>
    <ZZD_Description>0101210000</ZZD_Description>
    <ZZD_StartDate>1998-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0101291000</ZZD_Code>
    <ZZD_Description>0101291000</ZZD_Description>
    <ZZD_StartDate>1998-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0101299000</ZZD_Code>
    <ZZD_Description>0101299000</ZZD_Description>
    <ZZD_StartDate>1998-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0101300000</ZZD_Code>
    <ZZD_Description>0101300000</ZZD_Description>
    <ZZD_StartDate>1998-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0101900000</ZZD_Code>
    <ZZD_Description>0101900000</ZZD_Description>
    <ZZD_StartDate>1998-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0102211000</ZZD_Code>
    <ZZD_Description>0102211000</ZZD_Description>
    <ZZD_StartDate>2006-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0102213000</ZZD_Code>
    <ZZD_Description>0102213000</ZZD_Description>
    <ZZD_StartDate>2006-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0102219000</ZZD_Code>
    <ZZD_Description>0102219000</ZZD_Description>
    <ZZD_StartDate>2006-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>0106900090</ZZD_Code>
    <ZZD_Description>0106900090</ZZD_Description>
    <ZZD_StartDate>1998-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>2203000100</ZZD_Code>
    <ZZD_Description>2203000100</ZZD_Description>
    <ZZD_StartDate>1998-01-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>
  <RefCusCodeList>
    <ZZD_Code>2208409900</ZZD_Code>
    <ZZD_Description>2208409900</ZZD_Description>
    <ZZD_StartDate>1991-07-01T00:00:00</ZZD_StartDate>
    <ZZD_ZZK_NKCodeType>U395</ZZD_ZZK_NKCodeType>
  </RefCusCodeList>"));
		}
	}
}
