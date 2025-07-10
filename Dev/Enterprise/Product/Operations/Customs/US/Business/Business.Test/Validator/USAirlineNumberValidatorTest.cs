using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USAirlineNumberValidatorTest : TestCaseWithFactory
	{
		public void TestCheckAirFlightNo()
		{
			AssertEquals(string.Empty, USAirlineNumberValidator.CheckAirFlightNo(ZString.Empty, Factory));
			AssertEquals(string.Empty, USAirlineNumberValidator.CheckAirFlightNo("QF001", Factory));
			AssertEquals(USAirlineNumberValidator.InvalidFlightNoFormat, USAirlineNumberValidator.CheckAirFlightNo("!Y&BA", Factory));
			AssertEquals(USAirlineNumberValidator.InvalidFlightNoFormat, USAirlineNumberValidator.CheckAirFlightNo("A8XXX", Factory));
		}
	}
}
