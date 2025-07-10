using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Customs.SG.V4.Business.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class IPTUPDTest : TestCaseWithFactory
	{
		public void TestIPTUPDMessage()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.IPTUPD, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Amendment, Message.MessageSubType);
			AssertNull(TradenetDeclaration.InboundMessage);
			var testIPTUPDBuilder = new IPTUPD(DataProvider);
			testIPTUPDBuilder.Build(TradenetDeclaration);
			var inboundMessage = TradenetDeclaration.InboundMessage;
			AssertType<InPaymentUpdate>(inboundMessage.InPaymentUpdate);
			AssertType<Update>(inboundMessage.InPaymentUpdate.Update);
			AssertType<InPayment>(inboundMessage.InPaymentUpdate.Declaration);
			AssertNull(inboundMessage.InPayment);
			AssertNull(inboundMessage.InPaymentUpdate.Cancellation);
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
				var update = inboundMessage.InPaymentUpdate.Update;
				AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
				AssertEquals("UpdateIndicatorCode", "AME", update.UpdateIndicatorCode);
				AssertEquals("UpdatePermitNumber", "001", update.UpdatePermitNumber);
				AssertEquals("ReplacementPermitNumber", "002", update.ReplacementPermitNumber);
				AssertEquals("PermitValidityExtensionIndicator", true, update.Amendment.PermitValidityExtensionIndicator);
				AssertArrayEqualsByElements("AmendmentReason", new[] { "TEST REASON FOR AMENDING" }, update.Amendment.AmendmentReason);
				AssertNull("ExtensionReason", update.Amendment.ExtensionReason);
				AssertEquals("WTGCD2020", inboundMessage.InPaymentUpdate.Declaration.Header.MessageReference);
			}

			);
		}

		public void TestUnitPriceExchangeRateForSGDIsSentInMessage()
		{
			var items = new ItemsTestClass[] { new ItemsTestClass() };
			items[0].CustomsValue = 17500m;
			items[0].UnitPrice = 1500m;
			items[0].LSPValue = 720m;
			items[0].InvoiceCurrency = "SGD";
			items[0].IsMotorVehicle = false;
			DataProvider.Items = items;
			Message.Build(TradenetDeclaration);
			CombineAssertions(() =>
			{
				var transactionValue = TradenetDeclaration.InboundMessage.InPaymentUpdate.Declaration.Item[0].TransactionValue;
				AssertEquals(17500m, transactionValue.ItemCIFFOBValue);
				Assert(transactionValue.ItemCIFFOBValueSpecified);
				AssertEquals(720m, transactionValue.LastSellingPriceValue);
				Assert(transactionValue.LastSellingPriceValueSpecified);
				AssertNull("transactionValue.UnitPriceValue", transactionValue.UnitPriceValue);
			});

			items[0].IsMotorVehicle = true;
			Message.Build(TradenetDeclaration);
			CombineAssertions(() =>
			{
				var transactionUnitPriceValue = TradenetDeclaration.InboundMessage.InPaymentUpdate.Declaration.Item[0].TransactionValue.UnitPriceValue;
				AssertEquals(1500m, transactionUnitPriceValue.Amount.Value);
				AssertEquals("SGD", transactionUnitPriceValue.Amount.currencyID);
				AssertEquals("ExchangeRateSpecified", true, transactionUnitPriceValue.ExchangeRateSpecified);
				AssertEquals("ExchangeRate", 1.00m, transactionUnitPriceValue.ExchangeRate);
			});
		}

		IPTUPD Message => message ?? (message = new IPTUPD(DataProvider));
		IPTUPD message;
		IPTUPDTestClass DataProvider => dataProvider ?? (dataProvider = new IPTUPDTestClass());
		IPTUPDTestClass dataProvider;
		TradenetDeclaration TradenetDeclaration => tradenetDeclaration ?? (tradenetDeclaration = new TradenetDeclaration());
		TradenetDeclaration tradenetDeclaration;
	}
}
