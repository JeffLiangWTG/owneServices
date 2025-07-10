using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(IMProductValueDefaultOptionCollection))]
	sealed class IMProductValueDefaultOptionCollectiontTest : NonPersistentBusinessObjectCollectionTestCase<IMProductValueDefaultOptionCollection>
	{
		public void TestAddProductValueDefaultOptions()
		{
			var list = new List<ZString>() { DefaultOptions.Codes.Classification, DefaultOptions.Codes.CountryOfOrigin, DefaultOptions.Codes.Preference };
			var collection = GetCollectionToTest();
			collection.AddProductValueDefaultOptions(list);

			AssertEquals(3, collection.Count);
			AssertEquals(DefaultOptions.Codes.Classification, collection[0].FieldType);
			AssertEquals(DefaultOptions.Codes.CountryOfOrigin, collection[1].FieldType);
			AssertEquals(DefaultOptions.Codes.Preference, collection[2].FieldType);
		}

		public void TestGetDefaultOptionsString()
		{
			CompanyData.ImporterOverride = true;

			var collection = GetCollectionToTest();
			var option1 = collection.AddNew();
			option1.FieldType = DefaultOptions.Codes.Classification;
			var option2 = collection.AddNew();
			option2.FieldType = DefaultOptions.Codes.CountryOfOrigin;
			var option3 = collection.AddNew();
			option3.FieldType = DefaultOptions.Codes.Preference;

			AssertEquals("OVR,CLASS,COO,PREFF", collection.GetDefaultOptionsString());
		}

		protected override IMProductValueDefaultOptionCollection GetCollectionToTest()
		{
			return new IMProductValueDefaultOptionCollection(CompanyData);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IMProductValueDefaultOption(Factory);
		}

		OrgCompanyData CompanyData
		{
			get
			{
				return companyData ?? (companyData = Factory.NewWithValidTestData<OrgCompanyData>());
			}
		}
		OrgCompanyData companyData;
	}
}
