using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class FeeMessageProcessor : BaseResponseSectionProcessor<FeeMessage>
	{
		public const string MessageType = "FEE";
		public const string MessageName = "Fee Advice message";

		public FeeMessageProcessor(LoggingInformation logger, FeeMessage feeMessage)
			: base(logger, feeMessage, MessageType, MessageName)
		{
			Argument.NotNull(feeMessage, nameof(feeMessage));
		}

		protected override string URN => GetReferenceNumber(Section.UniqueReferenceNumber);

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.FEEMSG;

		protected override string DoProcessingReturningStatusCore(EDIMessage incomingMessage)
		{
			Logger.Log("Processing Fee Message");
			var result = EDIMessage.Status.Failed;
			this.incomingMessage = incomingMessage;

			Logger.Log("URN Number : " + URN);
			if (ProcessMessage())
			{
				result = EDIMessage.Status.Received;
				CompileFeeAdviceEmail();
				SendAcknowledgementReport(entry, responseEmail);
			}

			return result;
		}

		protected override void SetEntryStatus()
		{
			//status of entry is not affected by this message
		}

		#region email generation

		void CompileFeeAdviceEmail()
		{
			var permitNumber = Section.PermitNumber;
			var licenceNumber = Section.Licence?.FirstOrDefault()?.ReferenceID ?? ZString.Empty;

			var feeDetail = Section.FeeDetail;
			ZDecimal feeAmount = feeDetail?.FeeAmount?.Value ?? ZDecimal.Zero;
			var settlementDate = feeDetail != null ? FormattedDateTime(feeDetail.FeeDate) : ZString.Empty;
			var remittanceInfo = feeDetail?.FeeRemarks?.FreeText ?? ZString.Empty;

			CreateResponseEmail("Licence fee payment advice for ", "Debadv.htm", string.Empty, entry.Declaration.JE_DeclarationReference, URN, permitNumber, licenceNumber, feeAmount.ToString(2), settlementDate, remittanceInfo);
		}

		ZString FormattedDateTime(ZString messageDate)
		{
			return messageDate.SubstringSafe(6, 2) + "-" + GetMonthAbbrev(messageDate.SubstringSafe(4, 2)) + "-" + messageDate.SubstringSafe(0, 4);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ZString GetMonthAbbrev(string month)
		{
			switch (month)
			{
				case "01":
					return "JAN";
				case "02":
					return "FEB";
				case "03":
					return "MAR";
				case "04":
					return "APR";
				case "05":
					return "MAY";
				case "06":
					return "JUN";
				case "07":
					return "JUL";
				case "08":
					return "AUG";
				case "09":
					return "SEP";
				case "10":
					return "OCT";
				case "11":
					return "NOV";
				case "12":
					return "DEC";
				default:
					return "";
			}
		}
		#endregion

	}
}
