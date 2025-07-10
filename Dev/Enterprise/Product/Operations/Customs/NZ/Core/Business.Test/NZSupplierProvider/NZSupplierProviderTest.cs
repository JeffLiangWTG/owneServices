using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using INZSupplierProvider = Enterprise.Integration.Customs.NZ.INZSupplierProvider;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.Application;
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	class NZSupplierProviderTest : TestCaseWithFactory
	{
		[TestDate(2021, 11, 18)]
		public void TestGetSupplierList()
		{
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "NZSupplier01";
			cusCode.OK_OH = orgHeader.PK;

			Factory.Save();

			var collection = ObjectFactory.Get<INZSupplierProvider>().GetSupplierList(Factory, cusCode);

			AssertEquals("Default Filter count", 5, collection.FilterBusinessObjectDefaults.Count);
			AssertEquals("Description is set", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Description:Property"));
			AssertEquals("List Type is set", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("List Type:Property"));
			AssertEquals("Country/Region or Grouping", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Country/Region or Grouping:DefaultProperty"));
			AssertEquals("Effective Date", true, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Effective Date:Property1"));
			AssertEquals("Description is", orgHeader.OH_FullName, (ZString)collection.FilterBusinessObjectDefaults["Description:Property"].Value);
			AssertEquals("List Type is", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZSupplierListType, (ZString)collection.FilterBusinessObjectDefaults["List Type:Property"].Value);
			AssertEquals("List Type IsRemovable", ZBool.False, (ZBool)collection.FilterBusinessObjectDefaults["List Type:Property"].IsRemovable);
			AssertEquals("Country/Region or Grouping is", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, (ZString)collection.FilterBusinessObjectDefaults["Country/Region or Grouping:DefaultProperty"].Value);
			AssertEquals("Country/Region or Grouping IsRemovable", ZBool.False, (ZBool)collection.FilterBusinessObjectDefaults["Country/Region or Grouping:DefaultProperty"].IsRemovable);
			AssertEquals("Country/Region or Grouping is", ZDateTime.Today, (ZDateTime)collection.FilterBusinessObjectDefaults["Effective Date:Property1"].Value);
		}
	}
}
