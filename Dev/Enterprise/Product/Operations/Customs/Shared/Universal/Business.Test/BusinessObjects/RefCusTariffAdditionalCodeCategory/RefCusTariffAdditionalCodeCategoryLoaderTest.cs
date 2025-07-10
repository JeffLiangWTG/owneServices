using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffAdditionalCodeCategory.Loader))]
	public class RefCusTariffAdditionalCodeCategoryLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var category1 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "T1T");
			var category2 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "T2T");
			Factory.Save();
			AssertSame(category1, RefCusTariffAdditionalCodeCategory.Loader.Load(Factory, Core.Constants.CountryCodes.Eritrea, "T1T"));
			AssertSame(category2, RefCusTariffAdditionalCodeCategory.Loader.Load(Factory, Core.Constants.CountryCodes.Eritrea, "T2T"));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new RefCusTariffAdditionalCodeCategory.Loader(Factory);
	}
}
