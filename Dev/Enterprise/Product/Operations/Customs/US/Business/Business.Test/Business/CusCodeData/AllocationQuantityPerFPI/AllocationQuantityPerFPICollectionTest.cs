using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AllocationQuantityPerFPICollection))]
	sealed class AllocationQuantityPerFPICollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<AllocationQuantityPerFPI>
	{
		public void TestAllowNew()
		{
			var collection = GetCusCodeDataCollection();
			AssertEquals(false, collection.AllowNew);
			CountryData.OrgHeader.OH_IsConsignee = true;
			AssertEquals(true, collection.AllowNew);
		}

		protected override Customs.Business.CusCodeDataCollection<AllocationQuantityPerFPI> GetCusCodeDataCollection() => new AllocationQuantityPerFPICollection(CountryData);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<AllocationQuantityPerFPI>();

		OrgCountryData countryData;
		OrgCountryData CountryData
		{
			get
			{
				if (countryData == null)
				{
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					orgHeader.OH_IsConsignee = false;
					var wrapper = OrgHeaderWrapper.New(orgHeader);
					countryData = wrapper.CountryData;
				}

				return countryData;
			}
		}
	}
}
