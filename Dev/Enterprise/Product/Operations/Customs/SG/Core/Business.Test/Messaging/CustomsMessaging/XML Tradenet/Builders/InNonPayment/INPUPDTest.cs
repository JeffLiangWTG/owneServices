using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class INPUPDTest : TestCaseWithFactory
	{
		public void TestIPTUPDMessage()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.INPUPD, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Amendment, Message.MessageSubType);
			AssertNull(TradenetDeclaration.InboundMessage);
			var testINPUPDBuilder = new INPUPD(DataProvider);
			testINPUPDBuilder.Build(TradenetDeclaration);
			var inboundMessage = TradenetDeclaration.InboundMessage;
			AssertType<InNonPaymentUpdate>(inboundMessage.InNonPaymentUpdate);
			AssertType<Update>(inboundMessage.InNonPaymentUpdate.Update);
			AssertType<InNonPayment>(inboundMessage.InNonPaymentUpdate.Declaration);
			AssertNull(inboundMessage.InNonPayment);
			AssertNull(inboundMessage.InNonPaymentUpdate.Cancellation);
		}

		public void TestUpdate()
		{
			DataProvider.AdditionalMessageInfo.UpdateIndicator = "TEST";
			DataProvider.AdditionalMessageInfo.ExtendingTemporaryImportPeriod = true;
			DataProvider.JobNumber = "CD2020";
			DataProvider.PermitNoToUpdateOrCancel = "001";
			DataProvider.NumberOfRequestsForUpdate = 1;
			DataProvider.ReplacementPermitNumber = "002";
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "Test Reason For Amending";
			DataProvider.AdditionalMessageInfo.ReasonForExtendingTemporaryImportPeriod = "Test Reason For Extending Temporary Import Period";
			Message.Build(TradenetDeclaration);
			CombineAssertions(() =>
			{
				var inboundMessage = TradenetDeclaration.InboundMessage;
				var update = inboundMessage.InNonPaymentUpdate.Update;
				AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
				AssertEquals("UpdateIndicatorCode", "AME", update.UpdateIndicatorCode);
				AssertEquals("UpdatePermitNumber", "001", update.UpdatePermitNumber);
				AssertEquals("ReplacementPermitNumber", "002", update.ReplacementPermitNumber);
				AssertEquals("PermitValidityExtensionIndicator", true, update.Amendment.PermitValidityExtensionIndicator);
				AssertArrayEqualsByElements("AmendmentReason", new[] { "TEST REASON FOR AMENDING" }, update.Amendment.AmendmentReason);
				AssertArrayEqualsByElements("ExtensionReason", new[] { "TEST REASON FOR EXTENDING TEMPORARY IMPORT PERIOD" }, update.Amendment.ExtensionReason);
				AssertEquals("WTGCD2020", inboundMessage.InNonPaymentUpdate.Declaration.Header.MessageReference);
			}

			);
		}

		INPUPD Message => message ?? (message = new INPUPD(DataProvider));
		INPUPD message;
		IINPUPDTestClass DataProvider => dataProvider ?? (dataProvider = new IINPUPDTestClass());
		IINPUPDTestClass dataProvider;
		TradenetDeclaration TradenetDeclaration => tradenetDeclaration ?? (tradenetDeclaration = new TradenetDeclaration());
		TradenetDeclaration tradenetDeclaration;
	}
}
