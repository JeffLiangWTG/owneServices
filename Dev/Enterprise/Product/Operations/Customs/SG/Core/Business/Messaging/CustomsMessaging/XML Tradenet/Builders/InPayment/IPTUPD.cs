namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class IPTUPD : IPTDEC
	{
		public IPTUPD(IIPTUPD cusDec)
			: base(cusDec)
		{
		}

		public override string MessageType => CommonAccessReferenceCodeList.Codes.IPTUPD;
		public override string MessageSubType => CUSDECEDIMessage.Amendment;

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			var inPaymentUpdate = inboundMessage.InPaymentUpdate = new InPaymentUpdate();
			inPaymentUpdate.Declaration = BuildDeclaration();
			inPaymentUpdate.Update = BuildUpdate();
		}

		protected override bool SupportsUpdateAmendmentSection => true;
	}
}
