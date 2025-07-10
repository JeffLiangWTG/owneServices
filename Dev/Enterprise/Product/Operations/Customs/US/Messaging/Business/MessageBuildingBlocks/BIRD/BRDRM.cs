using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("RM")]
	[OutputBlock("RM")]
	public sealed partial class BRDRM : MessageBlock
	{
		public BRDRM()
			: base("RM")
		{
		}

		/// <summary>
		/// LOC = Location of Goods (FIRMS) Code
		/// </summary>
		[MessageBlockString(78, 3, "C")]
		public ZString Remark;
	}
}
