using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class INPUPDCancel : INPDEC
	{
		public INPUPDCancel(IINPUPD cusDec)
			: base(cusDec)
		{
		}

		public override string MessageType => CommonAccessReferenceCodeList.Codes.INPUPD;
		public override string MessageSubType => CUSDECEDIMessage.Cancellation;
		protected override ZString UpdateIndicatorCode => Enterprise.Customs.SG.V4.Business.SGConstants.UpdateIndicators.CNL;

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			var inNonPaymentUpdate = inboundMessage.InNonPaymentUpdate = new InNonPaymentUpdate();
			inNonPaymentUpdate.Update = BuildUpdate();
			inNonPaymentUpdate.Cancellation = BuildCancellation();
		}
	}
}
