using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffAdditionalCodeCategory))]
	public class RefCusTariffAdditionalCodeCategoryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping1 = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var dataGrouping2 = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Denmark);
			Factory.Save();
			var category = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "T1T");
			AssertSame(dataGrouping1, category.DataGrouping);
			category.ZY3_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Denmark;
			AssertSame(dataGrouping2, category.DataGrouping);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			return helper.CreateNewOrGetExistingTariffAdditionalCodeCategory(Core.Constants.CountryCodes.Eritrea, "C1T");
		}
	}
}
