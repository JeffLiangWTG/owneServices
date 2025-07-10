using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	internal class CARMExciseTaxTest : CARMBusinessObjectTest<CARMExciseTax>
	{
		protected override void AssertDeserializedEntry(CARMExciseTax entryProperties, int caseID)
		{
			switch (caseID)
			{
				case 1:
					Assert.NotNull(entryProperties);
					Assert.AreEqual("0409000010", entryProperties.TariffNumber);
					Assert.AreEqual("C00", entryProperties.ExciseTaxCode);
					Assert.AreEqual(Constants.DefaultValues.MinDateTime, entryProperties.ExciseTaxCodeValidStartDate);
					Assert.AreEqual(Constants.DefaultValues.MaxDateTime, entryProperties.ExciseTaxCodeValidEndDate);
					break;
				case 2:
					Assert.AreEqual(DateTime.Parse("1998-01-01T00:00:00", CultureInfo.InvariantCulture), entryProperties.ExciseTaxCodeValidStartDate);
					Assert.AreEqual(DateTime.Parse("2024-05-13T23:59:00", CultureInfo.InvariantCulture), entryProperties.ExciseTaxCodeValidEndDate);
					Assert.AreEqual("0409000000", entryProperties.TariffNumber);
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
                <d:TariffNumber>0409.00.00.10</d:TariffNumber>
                <d:ExciseTaxCode>C00</d:ExciseTaxCode>
                <d:AsOfDate>2024-09-10T00:00:00</d:AsOfDate>
                <d:Language>EN</d:Language>
                <d:ExciseTaxCodeValidStartDate>1000-05-13T00:00:00</d:ExciseTaxCodeValidStartDate>
                <d:ExciseTaxCodeValidEndDate>9999-10-20T00:00:00</d:ExciseTaxCodeValidEndDate>
                <d:InactiveIndicator>false</d:InactiveIndicator>
                <d:UpdateOn>2024-04-09T00:00:00</d:UpdateOn>
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
                <d:TariffNumber>0409.00.00</d:TariffNumber>
                <d:ExciseTaxCodeValidStartDate>1998-01-01T00:00:00</d:ExciseTaxCodeValidStartDate>
                <d:ExciseTaxCodeValidEndDate>2024-05-13T00:00:00</d:ExciseTaxCodeValidEndDate>
            </m:properties>
        </content>
    </entry>
</feed>
");
			}
		}
	}
}
