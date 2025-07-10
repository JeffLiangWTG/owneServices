namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class INPUPD : INPDEC
	{
		public INPUPD(IINPUPD cusDec)
			: base(cusDec)
		{
		}

		public override string MessageType => CommonAccessReferenceCodeList.Codes.INPUPD;
		public override string MessageSubType => CUSDECEDIMessage.Amendment;

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			var inNonPaymentUpdate = inboundMessage.InNonPaymentUpdate = new InNonPaymentUpdate();
			inNonPaymentUpdate.Declaration = BuildDeclaration();
			inNonPaymentUpdate.Update = BuildUpdate();
		}

		protected override bool SupportsUpdateAmendmentSection => true;

		protected override bool SupportsUpdateAmendmentExtensionReasonSection => true;
	}
}
