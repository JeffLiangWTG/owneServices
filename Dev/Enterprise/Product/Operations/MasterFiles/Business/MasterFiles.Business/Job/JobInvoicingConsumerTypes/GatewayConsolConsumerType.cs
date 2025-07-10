
namespace Enterprise.MasterFiles.Business
{
	using Enterprise.ZArchitecture.Core;

	public class GatewayConsolConsumerType : ConsolConsumerType
	{
		public GatewayConsolConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ResourceString MenuName(IJobInvoicingPlugIn host)
		{
			return ResString.GetMultilingualString("2189f823-4649-48c4-b90b-cc666926f3a2", "Gateway Billing");
		}

		public override string DisplayName(IJobInvoicingPlugIn host)
		{
			return Res.GetString("2189f823-4649-48c4-b90b-cc666926f3a2", "Gateway Billing");
		}
	}
}
