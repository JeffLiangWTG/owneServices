using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class IPTUPDCancel : IPTDEC
	{
		public IPTUPDCancel(IIPTUPD cusDec)
			: base(cusDec)
		{
		}

		public override string MessageType => CommonAccessReferenceCodeList.Codes.IPTUPD;
		public override string MessageSubType => CUSDECEDIMessage.Cancellation;
		protected override ZString UpdateIndicatorCode => Enterprise.Customs.SG.V4.Business.SGConstants.UpdateIndicators.CNL;

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			var inPaymentUpdate = inboundMessage.InPaymentUpdate = new InPaymentUpdate();
			inPaymentUpdate.Update = BuildUpdate();
			inPaymentUpdate.Cancellation = BuildCancellation();
		}
	}
}
