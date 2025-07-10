using System;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ChargeTypeHelperTest : TestCase
	{
		public void TestGetOtherDA63DutiesCodes()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[] { "12A", "13A", "13B", "13C", "13D", "13E", "15A", "15B", "17A", "2P1", "2P2", "2P3" }, ChargeTypeHelper.GetOtherDA63DutiesCodes());
		}

		public void TestCustomsSchedule1P1And2Codes()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[] { "1P1", "2P1", "2P2", "2P3" }, ChargeTypeHelper.GetCustomsDutiesSchedule1P1And2Codes());
		}

		public void TestIsExciseDutyCode()
		{
			AssertIs(ChargeTypeHelper.IsExciseDutyCode, new ZString[] { "12A" });
		}

		public void TestIsAntiDumpingDutyCode()
		{
			AssertIs(ChargeTypeHelper.IsAntiDumpingDutyCode, new ZString[] { "2P1", "2P2", "2P3" });
		}

		void AssertIs(Func<ZString, bool> funcToTest, ZString[] codes)
		{
			foreach (var code in new ZString[] { "12A", "12B", "13A", "13B", "13C", "13D", "13E", "15A", "15B", "17A", "1P1", "2P1", "2P2", "2P3" })
			{
				AssertEquals(code, codes.FirstOrDefault(x => x == code) == code, funcToTest(code));
			}
		}
	}
}
