using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.Testing
{
	class StatusListTest : TestCaseWithFactory
	{
		public void TestIsDeliveryOnPayment()
		{
			Assert(StatusList.IsDeliveryOnPayment(StatusList.Codes.EntryClearedCashToPayPriorToDelivery));
			Assert(!StatusList.IsDeliveryOnPayment("AAA"));
			Assert(!StatusList.IsDeliveryOnPayment(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified));
			Assert(StatusList.IsDeliveryOnPayment(StatusList.Codes.EntryClearedCashToPayPriorToDeliveryEntryRoutedToDocumentAudit));
			Assert(StatusList.IsDeliveryOnPayment(StatusList.Codes.EntryClearedCashToPayPriorToDeliveryPleaseNoteWarningsCorrectIfNecessary));
		}

		public void TestIsDeliveryOrderHerewithMethodOfPayment()
		{
			Assert(StatusList.IsDeliveryOrderHerewithMethodOfPayment(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified));
			Assert(!StatusList.IsDeliveryOrderHerewithMethodOfPayment("AAA"));
			Assert(!StatusList.IsDeliveryOrderHerewithMethodOfPayment(StatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecified));
			Assert(StatusList.IsDeliveryOrderHerewithMethodOfPayment(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedPleaseNoteWarningsCorrectIfNecessary));
			Assert(StatusList.IsDeliveryOrderHerewithMethodOfPayment(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit));
		}

		public void TestIsDeliveryOrderSentToRecipient()
		{
			Assert(StatusList.IsDeliveryOrderSentToRecipient(StatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecified));
			Assert(!StatusList.IsDeliveryOrderSentToRecipient("AAA"));
			Assert(!StatusList.IsDeliveryOrderSentToRecipient(StatusList.Codes.DeliveryOrderHerewithMethodOfPaymentAsSpecified));
			Assert(StatusList.IsDeliveryOrderSentToRecipient(StatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecifiedEntryRoutedToDocumentAudit));
			Assert(StatusList.IsDeliveryOrderSentToRecipient(StatusList.Codes.DeliveryOrderSentToRecipientMethodOfPaymentAsSpecifiedPleaseNoteWarningsCorrectIfNecessary));
		}
	}
}
