using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class VoyageRecyclingPeriodListTest : TestCase
	{
		public void TestGetAmountFromCode()
		{
			AssertEquals(new ZShort(-1), VoyageRecyclingPeriodList.GetAmountFromCode(VoyageRecyclingPeriodList.Codes.Default));
			AssertEquals(new ZShort(0), VoyageRecyclingPeriodList.GetAmountFromCode(VoyageRecyclingPeriodList.Codes.None));
			AssertEquals(new ZShort(6), VoyageRecyclingPeriodList.GetAmountFromCode(VoyageRecyclingPeriodList.Codes.SixMonths));
			AssertEquals(new ZShort(48), VoyageRecyclingPeriodList.GetAmountFromCode(VoyageRecyclingPeriodList.Codes.FortyEightMonths));
			AssertEquals(new ZShort(-2), VoyageRecyclingPeriodList.GetAmountFromCode("XXX"));
		}

		public void TestGetCodeFromAmount()
		{
			AssertEquals(VoyageRecyclingPeriodList.Codes.Default, VoyageRecyclingPeriodList.GetCodeFromAmount(-1));
			AssertEquals(VoyageRecyclingPeriodList.Codes.None, VoyageRecyclingPeriodList.GetCodeFromAmount(0));
			AssertEquals(VoyageRecyclingPeriodList.Codes.SixMonths, VoyageRecyclingPeriodList.GetCodeFromAmount(6));
			AssertEquals(VoyageRecyclingPeriodList.Codes.FortyEightMonths, VoyageRecyclingPeriodList.GetCodeFromAmount(48));
			AssertEquals(ZString.Empty, VoyageRecyclingPeriodList.GetCodeFromAmount(-2));
			AssertEquals(ZString.Empty, VoyageRecyclingPeriodList.GetCodeFromAmount(17));
		}
	}
}
