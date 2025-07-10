using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class TNPUPDCancel : TNPDEC
	{
		public TNPUPDCancel(ITNPUPD cusDec)
			: base(cusDec)
		{
		}

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			var transhipmentUpdate = inboundMessage.TranshipmentMovementUpdate = new TranshipmentMovementUpdate();
			transhipmentUpdate.Update = BuildUpdate();
			transhipmentUpdate.Cancellation = BuildCancellation();
		}

		#region ICusMessage

		public override string MessageType => CommonAccessReferenceCodeList.Codes.TNPUPD;
		public override string MessageSubType => CUSDECEDIMessage.Cancellation;
		protected override ZString UpdateIndicatorCode => SGConstants.UpdateIndicators.CNL;

		#endregion
	}
}
