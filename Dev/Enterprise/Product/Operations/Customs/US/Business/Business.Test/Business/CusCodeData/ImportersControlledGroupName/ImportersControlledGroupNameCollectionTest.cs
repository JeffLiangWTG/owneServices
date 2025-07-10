using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ImportersControlledGroupNameCollection))]
	sealed class ImportersControlledGroupNameCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<ImportersControlledGroupName>
	{
		public void TestAllowNew()
		{
			var collection = GetCusCodeDataCollection();
			AssertEquals(false, collection.AllowNew);
			CountryData.OrgHeader.OH_IsConsignee = true;
			AssertEquals(true, collection.AllowNew);
		}

		protected override Customs.Business.CusCodeDataCollection<ImportersControlledGroupName> GetCusCodeDataCollection() => new ImportersControlledGroupNameCollection(CountryData);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<ImportersControlledGroupName>();

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
