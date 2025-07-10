using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class DutyFeeInformationForTest : IDutyFeeInformation
	{
		public ZString Code { get; set; }

		public ZDecimal Value { get; set; }
	}
}
