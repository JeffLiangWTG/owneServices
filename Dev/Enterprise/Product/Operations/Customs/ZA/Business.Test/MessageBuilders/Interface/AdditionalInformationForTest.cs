using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class AdditionalInformationForTest : IAdditionalInformation
	{
		public ZString Code { get; set; }

		public ZString Value { get; set; }

		public ZInt? Group { get; set; }
	}
}
