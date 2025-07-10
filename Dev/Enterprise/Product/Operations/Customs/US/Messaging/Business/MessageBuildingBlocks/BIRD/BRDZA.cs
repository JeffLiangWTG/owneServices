using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("ZA")]
	[OutputBlock("ZA")]
	public sealed partial class BRDZA : MessageBlock
	{
		public BRDZA()
			: base("ZA")
		{
		}

		[MessageBlockString(35, 3, "C")]
		public ZString ConsigneeName;

		[MessageBlockString(35, 38, "C")]
		public ZString ConsigneeAddress1;
	}
}
