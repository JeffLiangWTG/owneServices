using System;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using Microsoft.OData.Edm;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	class RefDataLoaderTests
	{
		[Test]
		public void GetQuery()
		{
			var loader1 = new RefDataLoader("http://singleslash", false);
			var loader2 = new RefDataLoader("http://singleslash/", false);

			var query = "abc?filter=test etc";

			var expected = new Uri("http://singleslash/abc?filter=test etc");

			Assert.That(loader1.GetQuery(query), Is.EqualTo(expected), "Base url without backslash");
			Assert.That(loader2.GetQuery(query), Is.EqualTo(expected), "Base url with backslash");
		}

		[Test]
		public void LoadData()
		{
			var json = @"{
""@odata.context"":""http://refdbrepoupdate-uat.wtg.zone/Update/odata/$metadata#RefCusTariffUpdate"",
""value"":[{
""ZZ1_PK"":""789b084d-cc70-4a5f-b47d-351bc643bba7"",
""ZZ1_ZZI_TariffType"":""be1148f2-5d0b-4feb-9042-c7ee54b76be3"",
""ZZ1_TariffCode"":""72259210"",
""ZZ1_IAMUnique"":0,
""ZZ1_Description"":""OF A THICKNESS OF LESS THAN 0,45 MM"",
""ZZ1_StartDate"":""2019-01-01T00:00:00Z"",
""ZZ1_EndDate"":""2079-06-06T23:59:00Z"",
""ZZ1_PublishedDate"":""2019-10-18"",
""ZZ1_ZZF_NKTaxOrFeeCode"":""VAT"",
""ZZ1_ZZZ_NKDataGrouping"":""ZA"",
""ZZ1_CompositeKeyOnZZ5"":""15.72.04.25.9.2.10""}]}";
		
			Assert.DoesNotThrow(() =>
			{
				var data = RefDataLoader.DeserializeObject<RefCusTariff>(json);
				Assert.That(data[0].ZZ1_PublishedDate, Is.EqualTo(new Date(2019, 10, 18)));
			}, "No exception should be thrown");
		}
	}
}
