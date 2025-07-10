using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public static class ChargeTypeHelper
	{
		public static ZString[] GetOtherDA63DutiesCodes() => new ZString[] { "12A", "13A", "13B", "13C", "13D", "13E", "15A", "15B", "17A", "2P1", "2P2", "2P3" };
		public static ZString[] GetCustomsDutiesSchedule1P1And2Codes() => new ZString[] { "1P1", "2P1", "2P2", "2P3" };

		public static bool IsExciseDutyCode(ZString code) => code == "12A";
		public static bool IsAntiDumpingDutyCode(ZString code) => code == "2P1" || code == "2P2" || code == "2P3";
	}
}
