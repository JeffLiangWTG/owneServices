using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDEntrySummaryQuery)]
	[InputBlock("J1")]
	public abstract partial class BRDJ1 : MessageBlock // Need to add interface for BIRD System
	{
		public BRDJ1()
			: base("J1")
		{
		}

		[MessageBlockString(3, 3, "M")]
		public ZString Filer;

		[MessageBlockString(9, 6, "M", Justification = Justification.Right)]
		public ZString EntryNumber;

		[MessageBlockString(1, 15, "O")]
		public ZString CollectionBillCode;
	}
}