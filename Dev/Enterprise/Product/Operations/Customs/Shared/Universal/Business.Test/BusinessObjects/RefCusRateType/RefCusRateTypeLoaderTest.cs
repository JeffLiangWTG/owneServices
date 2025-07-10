using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateType.Loader))]
	public class RefCusRateTypeLoaderTest : LoaderTestCase
	{
		public void TestGetRateTypeByRateCode()
		{
			var rateType = RefCusRateType.Loader.GetRateTypeByRateCode(Factory, Core.Constants.CountryCodes.SouthAfrica, "RC1");
			AssertEquals("Duty", rateType.ZZR_Description);
			AssertEquals("DTY", rateType.ZZR_RateType);
			rateType = RefCusRateType.Loader.GetRateTypeByRateCode(Factory, Core.Constants.CountryCodes.China, "RC1");
			AssertNull("No rateType data for CN", rateType);
			rateType = RefCusRateType.Loader.GetRateTypeByRateCode(Factory, Core.Constants.CountryCodes.SouthAfrica, "RC2");
			AssertNull("No rateType data for RC2 ratecode", rateType);
		}

		public void TestLoad()
		{
			var rateType1 = RefCusRateType.Loader.Load(Factory, Core.Constants.CountryCodes.SouthAfrica, RateTypes.AntiDumping);
			var rateType2 = RefCusRateType.Loader.Load(Factory, Core.Constants.CountryCodes.SouthAfrica, RateTypes.AntiDumping);
			AssertEquals(rateType1, rateType2);
			var rateType3 = RefCusRateType.Loader.Load(Factory, Core.Constants.CountryCodes.SouthAfrica, RateTypes.Duty);
			AssertNotEquals(rateType1, rateType3);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new RefCusRateType.Loader(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			Factory.Save();
			helper.LoadOrCreateNewCusRateCode(Factory, "RC1", rateType.PK);
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", Constants.RateTypes.Duty);
			Factory.Save();
		}
	}
}
