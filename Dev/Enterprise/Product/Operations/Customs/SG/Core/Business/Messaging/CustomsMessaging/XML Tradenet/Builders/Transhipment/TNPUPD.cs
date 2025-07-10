namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class TNPUPD : TNPDEC
	{
		public TNPUPD(ITNPUPD cusDec)
			: base(cusDec)
		{
		}

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			var inNonPaymentUpdate = inboundMessage.TranshipmentMovementUpdate = new TranshipmentMovementUpdate();
			inNonPaymentUpdate.Declaration = BuildDeclaration();
			inNonPaymentUpdate.Update = BuildUpdate();
		}

		protected override bool SupportsUpdateAmendmentSection => true;

		#region ICusMessage

		public override string MessageType => CommonAccessReferenceCodeList.Codes.TNPUPD;
		public override string MessageSubType => CUSDECEDIMessage.Amendment;

		#endregion
	}
}
