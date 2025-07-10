using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	internal class CARMGSTCodeTest : CARMBusinessObjectTest<CARMGSTCode>
	{
		[Test]
		public void TestParseToRefCusTariff()
		{
			var gstCode = new CARMGSTCode
			{
				GSTCode = "01",
				GSTCodeValidStartDate = Constants.DefaultValues.MinDateTime,
				GSTCodeValidEndDate = Constants.DefaultValues.MaxDateTime,
				ExciseTaxRateCheckIndicator = "true",
				GSTCheckGroup = "1",
				GSTRateType = "V",
				Rate = "0.05",
				Description = "Live",
			};
			var refCusCode = gstCode.ParseToRefCusCodeList();
			Assert.AreEqual("001", refCusCode.ZZD_Code);
			Assert.AreEqual(Constants.DefaultValues.MinDateTime, refCusCode.ZZD_StartDate);
			Assert.AreEqual(Constants.DefaultValues.MaxDateTime, refCusCode.ZZD_EndDate);
			Assert.AreEqual("Live", refCusCode.ZZD_Description);
			Assert.AreEqual(4, refCusCode.RefCusCodeListAttributes.Length);

			AssertAttribute(refCusCode, 0, "CheckIndicator", "Y");
			AssertAttribute(refCusCode, 1, "CheckGroup", "1");
			AssertAttribute(refCusCode, 2, "RateType", "V");
			AssertAttribute(refCusCode, 3, "Rate", "0.05");
		}

		void AssertAttribute(RefCusCodeList refCusCode, int index, string name, string value)
		{
			Assert.AreEqual(name, refCusCode.RefCusCodeListAttributes[index].ZZE_ZXE_NKName);
			Assert.AreEqual(value, refCusCode.RefCusCodeListAttributes[index].ZZE_Value);
		}

		[Test]
		public void TestPopulateByCACTaxRate()
		{
			var gstCode = new CARMGSTCode();
			var cacTaxRate = new CACTaxRate
			{
				TaxRefNumber = "001",
				EffectiveDate = Constants.DefaultValues.MinDateTime,
				ExpiryDate = Constants.DefaultValues.MaxDateTime,
				CheckInd = "Y",
				CheckGroup = "1",
				RateType = "V",
				Rate = "5.00000",
				Title = "NORMAL RATE"
			};
			gstCode.PopulateByCACTaxRate(cacTaxRate);
			Assert.AreEqual("001", gstCode.GSTCode);
			Assert.AreEqual(Constants.DefaultValues.MinDateTime, gstCode.GSTCodeValidStartDate);
			Assert.AreEqual(Constants.DefaultValues.MaxDateTime, gstCode.GSTCodeValidEndDate);
			Assert.AreEqual("Y", gstCode.ExciseTaxRateCheckIndicator);
			Assert.AreEqual("1", gstCode.GSTCheckGroup);
			Assert.AreEqual("V", gstCode.GSTRateType);
			Assert.AreEqual("NORMAL RATE", gstCode.Description);
		}

		protected override void AssertDeserializedEntry(CARMGSTCode entryProperties, int caseID)
		{
			switch (caseID)
			{
				case 1:
					Assert.NotNull(entryProperties);
					Assert.AreEqual("001", entryProperties.GSTCode);
					Assert.AreEqual(Constants.DefaultValues.MinDateTime, entryProperties.GSTCodeValidStartDate);
					Assert.AreEqual(Constants.DefaultValues.MaxDateTime, entryProperties.GSTCodeValidEndDate);
					Assert.AreEqual("Y", entryProperties.ExciseTaxRateCheckIndicator);
					Assert.AreEqual("1", entryProperties.GSTCheckGroup);
					Assert.AreEqual("V", entryProperties.GSTRateType);
					Assert.AreEqual("NORMAL RATE", entryProperties.Description);
					break;
				case 2:
					Assert.AreEqual("001", entryProperties.GSTCode);
					Assert.AreEqual(DateTime.Parse("1998-01-01T00:00:00", CultureInfo.InvariantCulture), entryProperties.GSTCodeValidStartDate);
					Assert.AreEqual(DateTime.Parse("2024-12-31T23:59:00", CultureInfo.InvariantCulture), entryProperties.GSTCodeValidEndDate);
					break;
			}
		}

		protected override IEnumerable<(int caseID, string xml)> TestCasesForDeserializeXML
		{
			get
			{
				yield return (1, @"
<feed xmlns=""http://www.w3.org/2005/Atom"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xml:base=""https://IPORTAL-IPORTAIL.CBSA-ASFC.CA:443/sap/opu/odata/sap/TARIFF/"">
    <id>https://IPORTAL-IPORTAIL.CBSA-ASFC.CA:443/sap/opu/odata/sap/TARIFF/gstCodes</id>
    <title type=""text"">gstCodes</title>
    <updated>2024-12-18T10:15:19Z</updated>
    <author>
        <name/>
    </author>
    <link href=""gstCodes"" rel=""self"" title=""gstCodes""/>
    <entry>
        <id>https://IPORTAL-IPORTAIL.CBSA-ASFC.CA:443/sap/opu/odata/sap/TARIFF/gstCodes(GSTCode='01',AsOfDate=datetime'2024-12-18T00%3A00%3A00')</id>
        <title type=""text"">gstCodes(GSTCode='01',AsOfDate=datetime'2024-12-18T00%3A00%3A00')</title>
        <updated>2024-12-18T10:15:19Z</updated>
        <category term=""ZAPI_TARIFF_QUERY_SRV.gstCodesType"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme""/>
        <link href=""gstCodes(GSTCode='01',AsOfDate=datetime'2024-12-18T00%3A00%3A00')"" rel=""self"" title=""gstCodesType""/>
        <content type=""application/xml"">
            <m:properties xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
                <d:GSTCode>01</d:GSTCode>
                <d:AsOfDate>2024-12-18T00:00:00</d:AsOfDate>
                <d:Language>EN</d:Language>
                <d:GSTCodeValidStartDate>1000-01-01T00:00:00</d:GSTCodeValidStartDate>
                <d:GSTCodeValidEndDate>9999-12-31T00:00:00</d:GSTCodeValidEndDate>
                <d:ExciseTaxRateCheckIndicator>true</d:ExciseTaxRateCheckIndicator>
                <d:GSTCheckGroup>1</d:GSTCheckGroup>
                <d:GSTRateType>V</d:GSTRateType>
                <d:InactiveIndicator>false</d:InactiveIndicator>
                <d:Description>NORMAL RATE</d:Description>
                <d:UpdateOn>2021-05-25T00:00:00</d:UpdateOn>
            </m:properties>
        </content>
    </entry>
</feed>
");
				yield return (2, @"
<feed xmlns=""http://www.w3.org/2005/Atom"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xml:base=""https://IPORTAL-IPORTAIL.CBSA-ASFC.CA:443/sap/opu/odata/sap/TARIFF/"">
    <id>https://IPORTAL-IPORTAIL.CBSA-ASFC.CA:443/sap/opu/odata/sap/TARIFF/gstCodes</id>
    <title type=""text"">gstCodes</title>
    <updated>2024-12-18T10:15:19Z</updated>
    <author>
        <name/>
    </author>
    <link href=""gstCodes"" rel=""self"" title=""gstCodes""/>
    <entry>
        <id>https://IPORTAL-IPORTAIL.CBSA-ASFC.CA:443/sap/opu/odata/sap/TARIFF/gstCodes(GSTCode='01',AsOfDate=datetime'2024-12-18T00%3A00%3A00')</id>
        <title type=""text"">gstCodes(GSTCode='01',AsOfDate=datetime'2024-12-18T00%3A00%3A00')</title>
        <updated>2024-12-18T10:15:19Z</updated>
        <category term=""ZAPI_TARIFF_QUERY_SRV.gstCodesType"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme""/>
        <link href=""gstCodes(GSTCode='01',AsOfDate=datetime'2024-12-18T00%3A00%3A00')"" rel=""self"" title=""gstCodesType""/>
        <content type=""application/xml"">
            <m:properties xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
                <d:GSTCode>01</d:GSTCode>
                <d:GSTCodeValidStartDate>1998-01-01T00:00:00</d:GSTCodeValidStartDate>
                <d:GSTCodeValidEndDate>2024-12-31T00:00:00</d:GSTCodeValidEndDate>
            </m:properties>
        </content>
    </entry>
</feed>
");
			}
		}
	}
}
