using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffType.Loader))]
	public class RefCusTariffTypeLoaderTest : LoaderTestCase
	{
		public void TestLoad()
		{
			var rateType1 = RefCusTariffType.Loader.Load(Factory, Core.Constants.CountryCodes.Chad, "123");
			var rateType2 = RefCusTariffType.Loader.Load(Factory, Core.Constants.CountryCodes.Chad, "123");
			AssertNotNull(rateType1);
			AssertEquals(rateType1, rateType2);
			var rateType3 = RefCusTariffType.Loader.Load(Factory, Core.Constants.CountryCodes.Chad, "234");
			AssertNotEquals(rateType1, rateType3);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusTariffType.Loader(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Chad, "123");
			Factory.Save();
		}
	}
}
