using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DutyCalculationHelperTest : TestCaseWithFactory
	{
		public void TestCusTariffTypeOrderComparer()
		{
			var originalList = new TariffView[] { tariffView2, tariffView3, tariffView1, tariffView4 };
			var rateTypes = new ZString[] { "RT1", "RT2", null };
			var expectedResult = new TariffView[] { tariffView1, tariffView2, tariffView3, tariffView4 };
			var orderedList = originalList.ToList().OrderBy(x => x, new DutyCalculationHelper.CusTariffTypeOrderComparer(rateTypes.ToList())).ToArray();
			AssertArrayEqualsByElements(expectedResult, orderedList);
		}

		public void TestCusRateCodeOrderComparer()
		{
			var originalList = new CusRefRateCodeView[] { rateCode3, rateCode1, rateCode2, rateCode4 };
			var rateTypes = new ZString[] { "RT1", "RT2", null };
			var expectedResult = new CusRefRateCodeView[] { rateCode1, rateCode2, rateCode3, rateCode4 };
			var orderedList = originalList.ToList().OrderBy(x => x, new DutyCalculationHelper.CusRateTypeOrderComparer(rateTypes.ToList())).ToArray();
			AssertArrayEqualsByElements(expectedResult, orderedList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			tariffType1 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "TT1");
			tariffType2 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "TT2");
			tariffType3 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "TT3");
			tariffType4 = UniversalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "TT4");
			rateType1 = UniversalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "RT1");
			rateType2 = UniversalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "RT1");
			rateType3 = UniversalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "RT2");
			rateType4 = UniversalReferenceDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "RT2");
			rateCode1 = UniversalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "RT1", rateType1.PK);
			rateCode2 = UniversalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "RT2", rateType2.PK);
			rateCode3 = UniversalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "RT1", rateType3.PK);
			rateCode4 = UniversalReferenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "RT2", rateType4.PK);
			Factory.Save();
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			tariffView1 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1.PK, "1010102030", startDate, endDate);
			tariffView2 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2.PK, "1010102031", startDate, endDate);
			tariffView3 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3.PK, "1010102032", startDate, endDate);
			tariffView4 = UniversalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4.PK, "1010102033", startDate, endDate);
			UniversalReferenceDataHelper.CreateRate(tariffView1, rateCode1.PK, startDate, endDate);
			UniversalReferenceDataHelper.CreateRate(tariffView2, rateCode2.PK, startDate, endDate);
			UniversalReferenceDataHelper.CreateRate(tariffView3, rateCode3.PK, startDate, endDate);
			UniversalReferenceDataHelper.CreateRate(tariffView4, rateCode4.PK, startDate, endDate);
			Factory.Save();
		}

		ZAUniversalReferenceTestDataHelper UniversalReferenceDataHelper
		{
			get
			{
				if (universalReferenceDataHelper == null)
				{
					universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
				}

				return universalReferenceDataHelper;
			}
		}

		ZAUniversalReferenceTestDataHelper universalReferenceDataHelper;
		TariffView tariffView1;
		TariffView tariffView2;
		TariffView tariffView3;
		TariffView tariffView4;
		RefCusRateType rateType1;
		RefCusRateType rateType2;
		RefCusRateType rateType3;
		RefCusRateType rateType4;
		CusRefRateCodeView rateCode1;
		CusRefRateCodeView rateCode2;
		CusRefRateCodeView rateCode3;
		CusRefRateCodeView rateCode4;
		RefCusTariffType tariffType1;
		RefCusTariffType tariffType2;
		RefCusTariffType tariffType3;
		RefCusTariffType tariffType4;
	}
}
