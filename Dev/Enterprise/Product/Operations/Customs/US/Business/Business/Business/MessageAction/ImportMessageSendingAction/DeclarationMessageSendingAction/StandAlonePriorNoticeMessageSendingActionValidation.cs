using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class StandAlonePriorNoticeMessageSendingActionValidation : USImportMessageSendingActionValidation
	{
		public StandAlonePriorNoticeMessageSendingActionValidation(StandAlonePriorNoticeMessageSendingAction parent)
			: base(parent)
		{
		}

		protected new StandAlonePriorNoticeMessageSendingAction Parent
		{
			get { return (StandAlonePriorNoticeMessageSendingAction)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUS_PNActionCode();
		}

		public void ValidateUS_PNActionCode()
		{
			ValidateCalculatedProperty(Parent.US_PNActionCodeInfo);
		}

		protected override void CheckUS_SendMessage()
		{
			base.CheckUS_SendMessage();

			var priorNoticeHeader = Parent.PriorNoticeHeader;
			if (Parent.US_SendMessage && priorNoticeHeader != null && priorNoticeHeader.ACEStandalonePriorNoticeLines.IsCountEqualTo(0))
			{
				if (priorNoticeHeader.RelatedBillRequired)
				{
					Parent.US_SendMessageInfo.AddMessageError(NoPriorNoticeLinesForMultipleMasterBills);
				}
				else
				{
					Parent.US_SendMessageInfo.AddMessageError(NoPriorNoticeLinesToSend);
				}
			}
		}
		internal const string NoPriorNoticeLinesForMultipleMasterBills = "There are no prior notice lines to be sent against this bill of lading. There may be no bill of lading associated to the invoice or none of the invoice lines on the associated invoice require prior notice.";
		internal const string NoPriorNoticeLinesToSend = "There are no prior notice lines to be sent. Only those FDA lines which are not disclaimed and do not have a Confirmation Number will be sent.";

		protected void CheckUS_PNActionCode()
		{
			var declaration = Parent.Declaration;
			if (declaration != null && (declaration.IsACE || declaration.IsFTZPGAStandAlonePriorNotice))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PNActionCodeInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.US_PNActionCodeInfo, Parent.Lookups.ACEPNActionCodeList);

				if (Parent.US_PNActionCode == ACEPNActionCodeList.Codes.D ||
					Parent.US_PNActionCode == ACEPNActionCodeList.Codes.R)
				{
					Parent.US_PNActionCodeInfo.AddMessageError(CodeForFutureUse);
				}
			}
		}
		internal const string CodeForFutureUse = "Delete and Replace action codes are for future use. It is likely that submitting this to Customs will result in an error response.";
	}
}
