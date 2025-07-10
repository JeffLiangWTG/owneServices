using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusPackageCustomLabelsProviderTest : TestCaseWithFactory
	{
		public void TestCustomLabelsProvider()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decl.JE_OH_Supplier = org.PK;
			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var customLabelsProvider = new CusPackageCustomLabelsProvider(packingList);
			var provider = (ICustomLabelsProvider)customLabelsProvider;
			var list = provider.GetCustomFields(org, Factory);

			CombineAssertions(() =>
			{
				AssertEquals(packingList, provider.ConfigOrgProvider);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomAttribute1);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomAttribute2);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomFlag1);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomFlag1);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomDate1);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomDate2);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomDecimal1);
				AssertCustomLabelListContains(list, CusPackage.Schema.CustomDecimal2);
			});
		}

		void AssertCustomLabelListContains(CustomLabelInfoList list, string propertyName)
		{
			foreach (CustomLabelInfo info in list)
			{
				if (info.PropertyName == propertyName)
				{
					return;
				}
			}
			Fail("Could not find property: " + propertyName + " in CustomLabelInfoList");
		}
	}
}
