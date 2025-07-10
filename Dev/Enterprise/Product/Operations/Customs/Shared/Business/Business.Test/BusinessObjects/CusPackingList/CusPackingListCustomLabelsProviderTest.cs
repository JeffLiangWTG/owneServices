using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	class CusPackingListCustomLabelsProviderTest : TestCaseWithFactory
	{
		public void TestCustomLabelsProvider()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			decl.JE_OH_Supplier = org.PK;
			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var customLabelsProvider = new CusPackingListCustomLabelsProvider(packingList);
			var provider = (ICustomLabelsProvider)customLabelsProvider;
			var list = provider.GetCustomFields(org, Factory);

			CombineAssertions(() =>
			{
				AssertEquals(packingList, provider.ConfigOrgProvider);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomAttribute1.Name);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomAttribute2.Name);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomFlag1.Name);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomFlag2.Name);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomDate1.Name);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomDate2.Name);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomDecimal1.Name);
				AssertCustomLabelListContains(list, CusPackingListSchema.CUL_CustomDecimal2.Name);
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
