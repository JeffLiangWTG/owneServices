using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("ZR")]
	[OutputBlock("ZR")]
	public sealed partial class BRDZR : MessageBlock
	{
		public BRDZR()
			: base("ZR")
		{
		}

		[MessageBlockString(78, 3, "C")]
		public ZString Remarks;
	}
}