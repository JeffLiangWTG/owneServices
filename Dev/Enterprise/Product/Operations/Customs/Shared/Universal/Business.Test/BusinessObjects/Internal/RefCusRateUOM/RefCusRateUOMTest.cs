using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateUOM))]
	internal class RefCusRateUOMTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.New<RefCusRateUOM>();
			result.ZXG_ZZ2_Rate = cusRate.PK;
			result.ZXG_UOM = "ASVX";
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<RefCusRateUOM>();
			result.ZXG_ZZ2_Rate = cusRate.PK;
			result.ZXG_UOM = "ASVX";
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "CUS");
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			Factory.Save();
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RATE0", rateType.PK);
			Factory.Save();
			cusRate = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "DUMMYFORMULA", dataGrouping: Core.Constants.CountryCodes.Eritrea);
		}

		RateView cusRate;
	}
}
