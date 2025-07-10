using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	internal class CARMTariffTest : CARMBusinessObjectTest<CARMTariff>
	{
		[Test]
		public void TestParseToRefCusTariff()
		{
			var tariff = new CARMTariff
			{
				TariffNumber = "8701.10.10.00",
				TariffNumberValidStartDate = Constants.DefaultValues.MinDateTime,
				TariffNumberValidEndDate = Constants.DefaultValues.MaxDateTime,
				Description = "Live",
				UnitOfMeasureCode = "KGM"
			};
			var refTariff = tariff.ParseToRefCusTariff();
			Assert.AreEqual("8701101000", refTariff.ZZ1_TariffCode);
			Assert.AreEqual(Constants.DefaultValues.MinDateTime, refTariff.ZZ1_StartDate);
			Assert.AreEqual(Constants.DefaultValues.MaxDateTime, refTariff.ZZ1_EndDate);
			Assert.AreEqual("Live", refTariff.ZZ1_Description);
			Assert.AreEqual("KGM", refTariff.RefCusTariffUOMs[0].ZZ8_UOM);
			Assert.AreEqual(Constants.ConveyanceRequiredTariffAttrName, refTariff.RefCusTariffAttributes[0].ZZ3_Name);
			Assert.AreEqual("Y", refTariff.RefCusTariffAttributes[0].ZZ3_Value);

			tariff.TariffNumber = string.Empty;
			Assert.Null(tariff.ParseToRefCusTariff());
		}

		protected override void AssertDeserializedEntry(CARMTariff entryProperties, int caseID)
		{
			switch (caseID)
			{
				case 1:
					Assert.NotNull(entryProperties);
					Assert.AreEqual(Constants.DefaultValues.MinDateTime, entryProperties.TariffNumberValidStartDate);
					Assert.AreEqual(Constants.DefaultValues.MaxDateTime, entryProperties.TariffNumberValidEndDate);
					Assert.AreEqual("0101210000", entryProperties.TariffNumber);
					Assert.AreEqual("Live horses, asses, mules and hinnies. - Horses: - Pure-bred breeding animals", entryProperties.Description);
					Assert.AreEqual("NMB", entryProperties.UnitOfMeasureCode);
					break;
				case 2:
					Assert.AreEqual("0101210000", entryProperties.TariffNumber);
					Assert.AreEqual(DateTime.Parse("1998-01-01T00:00:00", CultureInfo.InvariantCulture), entryProperties.TariffNumberValidStartDate);
					Assert.AreEqual(DateTime.Parse("2024-12-31T23:59:00", CultureInfo.InvariantCulture), entryProperties.TariffNumberValidEndDate);
					break;
			}
		}

		protected override IEnumerable<(int caseID, string xml)> TestCasesForDeserializeXML
		{
			get
			{
				yield return (1, @"
<feed xmlns=""http://www.w3.org/2005/Atom"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xml:base=""https://ccapi-ipacc.cbsa-asfc.cloud-nuage.canada.ca/v1/tariff-srv/"">
    <entry>
        <content type=""application/xml"">
            <m:properties xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
                <d:TariffNumber>0101.21.00.00</d:TariffNumber>
                <d:AsOfDate>2024-09-11T00:00:00</d:AsOfDate>
                <d:Language>EN</d:Language>
                <d:SpecialClassificationIndicator>09</d:SpecialClassificationIndicator>
                <d:TariffNumberValidStartDate>1000-01-01T00:00:00</d:TariffNumberValidStartDate>
                <d:TariffNumberValidEndDate>9999-12-31T00:00:00</d:TariffNumberValidEndDate>
                <d:TariffNumberChangeReason>T_INITIAL</d:TariffNumberChangeReason>
                <d:TariffItemNumber>0101.21.00</d:TariffItemNumber>
                <d:TariffItemEffectiveDate>2012-01-01T00:00:00</d:TariffItemEffectiveDate>
                <d:TariffItemExpiryDate>9999-12-31T00:00:00</d:TariffItemExpiryDate>
                <d:TariffItemChangeReasonCode>T_INITIAL</d:TariffItemChangeReasonCode>
                <d:AreaCode1></d:AreaCode1>
                <d:AreaCode2></d:AreaCode2>
                <d:AreaCode3></d:AreaCode3>
                <d:UnitOfMeasureCode>NMB</d:UnitOfMeasureCode>
                <d:ExchangeDateFlag>false</d:ExchangeDateFlag>
                <d:PermitRequiredIndicator>false</d:PermitRequiredIndicator>
                <d:QuotaIndicator>false</d:QuotaIndicator>
                <d:GSTIndicator>true</d:GSTIndicator>
                <d:InactiveIndicator>false</d:InactiveIndicator>
                <d:Description>Live horses, asses, mules and hinnies. - Horses: - Pure-bred breeding animals</d:Description>
                <d:UpdateOn>2021-05-25T00:00:00</d:UpdateOn>
            </m:properties>
        </content>
    </entry>
</feed>
");
				yield return (2, @"
<feed xmlns=""http://www.w3.org/2005/Atom"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xml:base=""https://ccapi-ipacc.cbsa-asfc.cloud-nuage.canada.ca/v1/tariff-srv/"">
    <entry>
        <content type=""application/xml"">
            <m:properties xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"">
                <d:TariffNumber>0101.21.00.00</d:TariffNumber>
                <d:TariffNumberValidStartDate>1998-01-01T00:00:00</d:TariffNumberValidStartDate>
                <d:TariffNumberValidEndDate>2024-12-31T00:00:00</d:TariffNumberValidEndDate>
            </m:properties>
        </content>
    </entry>
</feed>
");
			}
		}
	}
}
