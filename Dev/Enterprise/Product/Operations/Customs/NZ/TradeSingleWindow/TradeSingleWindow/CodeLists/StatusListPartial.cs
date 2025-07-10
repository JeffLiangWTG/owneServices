
namespace Enterprise.Customs.NZ.TradeSingleWindow
{
	partial class StatusList
	{
		public static bool IsDeliveryOnPayment(string nZCSStatus)
		{
			return nZCSStatus == StatusList.Codes.EntryClearedCashToPayPriorToDelivery
				|| nZCSStatus == StatusList.Codes.EntryClearedCashToPayPriorToDeliveryEntryRoutedToDocumentAudit
				|| nZCSStatus == StatusList.Codes.EntryClearedCashToPayPriorToDeliveryPleaseNoteWarningsCorrectIfNecessary;
		}

		public static bool IsDeliveryOrderHerewithMethodOfPayment(string status)
		{
			return status == StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified
				|| status == StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit
				|| status == StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedPleaseNoteWarningsCorrectIfNecessary;
		}

		public static bool IsDeliveryOrderSentToRecipient(string status)
		{
			return status == StatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecified
				|| status == StatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit
				|| status == StatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecifiedPleaseNoteWarningsCorrectIfNecessary;
		}
	}
}
