using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Customs.SG.V4.Business.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class IPTUPDRefundTest : TestCaseWithFactory
	{
		public void TestIPTUPDRefundMessage()
		{
			DataProvider.AdditionalMessageInfo.RefundCode = "RF08";
			AssertEquals(CommonAccessReferenceCodeList.Codes.IPTUPD, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Refund, Message.MessageSubType);
			AssertNull(TradenetDeclaration.InboundMessage);
			var testIPTUPDRefundBuilder = new IPTUPDRefund(DataProvider);
			testIPTUPDRefundBuilder.Build(TradenetDeclaration);
			var inboundMessage = TradenetDeclaration.InboundMessage;
			AssertType<InPaymentUpdate>(inboundMessage.InPaymentUpdate);
			AssertType<Update>(inboundMessage.InPaymentUpdate.Update);
			AssertType<UpdateRefund>(inboundMessage.InPaymentUpdate.Update.Refund);
			AssertType<RefundOnly>(inboundMessage.InPaymentUpdate.RefundOnly);
			AssertNull(inboundMessage.InPayment);
			AssertNull(inboundMessage.InPaymentUpdate.Cancellation);
			AssertNull(inboundMessage.InPaymentUpdate.Declaration);
		}

		public void TestRefund_RefundHeader()
		{
			// Update properties
			DataProvider.AdditionalMessageInfo.UpdateIndicator = "PRS";
			DataProvider.JobNumber = "CD2020";
			DataProvider.NumberOfRequestsForUpdate = 1;
			DataProvider.PermitNoToUpdateOrCancel = "001";
			DataProvider.ReplacementPermitNumber = "002";
			DataProvider.AdditionalMessageInfo.RefundCode = "RF08";
			DataProvider.AdditionalMessageInfo.ReasonForRefund = "Test Reason For Refund";
			// Refund properties
			DataProvider.JobNumber = "CD2020";
			DataProvider.DeclarantId = "SFZ";
			DataProvider.AdditionalRecipients = new ZString[] { "A00", "A11" };
			DataProvider.AdditionalMessageInfo.CancellationCode = "TST";
			DataProvider.Declarant = new AgentInfoTestClass { Name = "Test SG Broker", Phone = "64 85721111", Code = "V13t001" };
			DataProvider.AdditionalMessageInfo.SupportingDocuments = new AttachmentsTestClass[2] { new AttachmentsTestClass { FileName = "ATTDOC_0001", DocType = SupportingDocumentTypeCodeList.Codes.DocType001 }, new AttachmentsTestClass { FileName = "ATTDOC_0002", DocType = SupportingDocumentTypeCodeList.Codes.DocType002 } };
			Message.Build(TradenetDeclaration);
			CombineAssertions("RefundOnly", () =>
			{
				var refundOnly = TradenetDeclaration.InboundMessage.InPaymentUpdate.RefundOnly;
				var header = refundOnly.RefundHeader;
				Assert("DeclarationIndicator", header.DeclarationIndicator);
				Assert("DeclarationIndicatorSpecified", header.DeclarationIndicatorSpecified);
				// TODO: Are both of these correct results?
				AssertEquals("MessageReference", "WTGCD2020", header.MessageReference);
				AssertEquals("DeclarantID", SGXmlEDIMessage.SendersReferencePlaceHolderXml, header.DeclarantID);
				AssertEquals("CommonAccessReference", CommonAccessReferenceCodeList.Codes.IPTUPD, header.CommonAccessReference);
				AssertArrayEqualsByElements("AdditionalRecipientID", new[] { "A00", "A11" }, header.AdditionalRecipientID);
				var uniqueReferenceNumber = header.UniqueReferenceNumber;
				AssertEquals("UniqueReferenceNumber ID", SGXmlEDIMessage.MessageNumberPlaceHolderXml, uniqueReferenceNumber.ID);
				AssertEquals("UniqueReferenceNumber Date", SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml, uniqueReferenceNumber.Date);
				AssertEquals("UniqueReferenceNumber SequenceNumeric", SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml, uniqueReferenceNumber.SequenceNumeric);
				var declarantParty = refundOnly.DeclarantParty;
				AssertEquals("64 85721111", declarantParty.Telephone);
				AssertEquals("V13T001", declarantParty.PersonInformation.CodeValue);
				AssertEquals("TEST SG BROKER", declarantParty.PersonInformation.Name);
				var references = refundOnly.SupportingDocumentReference;
				var idAndNames = references.Select(c => $"{c.DocumentID}:{c.Filename}").ToArray();
				AssertArrayEqualsByElements("SupportingDocumentReference", new[] { "001:ATTDOC_0001", "002:ATTDOC_0002" }, idAndNames);
			}

			);
			CombineAssertions("Update", () =>
			{
				var update = TradenetDeclaration.InboundMessage.InPaymentUpdate.Update;
				AssertEquals("UpdateIndicatorCode", "PRS", update.UpdateIndicatorCode);
				AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
				AssertEquals("UpdatePermitNumber", "001", update.UpdatePermitNumber);
				AssertEquals("ReplacementPermitNumber", "002", update.ReplacementPermitNumber);
				AssertEquals("Refund.ReasonCode", "RF08", update.Refund.ReasonCode);
				AssertEquals("Refund.Reason", "TEST REASON FOR REFUND", update.Refund.Reason[0]);
				AssertNull("Amendment", update.Amendment);
			}

			);
		}

		public void TestRefund_FRF()
		{
			DataProvider.AdditionalMessageInfo.UpdateIndicator = "FRF";
			BuildRefundItemData();
			Message.Build(TradenetDeclaration);
			AssertNull("RefundItems", TradenetDeclaration.InboundMessage.InPaymentUpdate.RefundOnly.RefundItem);
			CombineAssertions("RefundSummary", () =>
			{
				var refundSummary = TradenetDeclaration.InboundMessage.InPaymentUpdate.RefundOnly.RefundSummary;
				AssertEquals("NumberOfItems", 0, refundSummary.NumberOfItems);
				var totalTariffRefund = refundSummary.TotalTariffRefund;
				AssertEquals("TotalCustomsDutyRefundAmount", 110m, totalTariffRefund.TotalCustomsDutyRefundAmount);
				AssertEquals("TotalCustomsDutyRefundAmountSpecified", true, totalTariffRefund.TotalCustomsDutyRefundAmountSpecified);
				AssertEquals("TotalExciseDutyRefundAmount", 120m, totalTariffRefund.TotalExciseDutyRefundAmount);
				AssertEquals("TotalExciseDutyRefundAmountSpecified", true, totalTariffRefund.TotalExciseDutyRefundAmountSpecified);
				AssertEquals("TotalGoodsAndServicesTaxRefundAmount", 130m, totalTariffRefund.TotalGoodsAndServicesTaxRefundAmount);
				AssertEquals("TotalGoodsAndServicesTaxRefundAmountSpecified", true, totalTariffRefund.TotalGoodsAndServicesTaxRefundAmountSpecified);
			}

			);
		}

		public void TestRefund_PRG()
		{
			DataProvider.AdditionalMessageInfo.UpdateIndicator = "PRG";
			BuildRefundItemData();
			Message.Build(TradenetDeclaration);
			AssertNull("RefundItems", TradenetDeclaration.InboundMessage.InPaymentUpdate.RefundOnly.RefundItem);
			CombineAssertions("RefundSummary", () =>
			{
				var refundSummary = TradenetDeclaration.InboundMessage.InPaymentUpdate.RefundOnly.RefundSummary;
				AssertEquals("NumberOfItems", 0, refundSummary.NumberOfItems);
				var totalTariffRefund = refundSummary.TotalTariffRefund;
				AssertEquals("TotalCustomsDutyRefundAmount", 0m, totalTariffRefund.TotalCustomsDutyRefundAmount);
				AssertEquals("TotalCustomsDutyRefundAmountSpecified", false, totalTariffRefund.TotalCustomsDutyRefundAmountSpecified);
				AssertEquals("TotalExciseDutyRefundAmount", 0m, totalTariffRefund.TotalExciseDutyRefundAmount);
				AssertEquals("TotalExciseDutyRefundAmountSpecified", false, totalTariffRefund.TotalExciseDutyRefundAmountSpecified);
				AssertEquals("TotalGoodsAndServicesTaxRefundAmount", 130m, totalTariffRefund.TotalGoodsAndServicesTaxRefundAmount);
				AssertEquals("TotalGoodsAndServicesTaxRefundAmountSpecified", true, totalTariffRefund.TotalGoodsAndServicesTaxRefundAmountSpecified);
			}

			);
		}

		public void TestRefund_PRS()
		{
			DataProvider.AdditionalMessageInfo.UpdateIndicator = "PRS";
			BuildRefundItemData();
			Message.Build(TradenetDeclaration);
			CombineAssertions("RefundItems", () =>
			{
				var refundItems = TradenetDeclaration.InboundMessage.InPaymentUpdate.RefundOnly.RefundItem;
				var item1 = refundItems[0];
				AssertEquals("Item 1 ItemSequenceNumeric", 1, item1.ItemSequenceNumeric);
				AssertEquals("Item 1 ItemSequenceNumericSpecified", true, item1.ItemSequenceNumericSpecified);
				AssertEquals("Item 1 ItemHarmonizedSystemCode", "84733092", item1.ItemHarmonizedSystemCode);
				var item1TariffRefund = item1.TariffRefund;
				AssertEquals("Item 1 CustomsDutyRefundAmount", 10m, item1TariffRefund.CustomsDutyRefundAmount);
				AssertEquals("Item 1 CustomsDutyRefundAmountSpecified", true, item1TariffRefund.CustomsDutyRefundAmountSpecified);
				AssertEquals("Item 1 ExciseDutyRefundAmount", 20m, item1TariffRefund.ExciseDutyRefundAmount);
				AssertEquals("Item 1 ExciseDutyRefundAmountSpecified", true, item1TariffRefund.ExciseDutyRefundAmountSpecified);
				AssertEquals("Item 1 GoodsAndServicesTaxRefundAmount", 30m, item1TariffRefund.GoodsAndServicesTaxRefundAmount);
				AssertEquals("Item 1 GoodsAndServicesTaxRefundAmountSpecified", true, item1TariffRefund.GoodsAndServicesTaxRefundAmountSpecified);
				var item2 = refundItems[1];
				AssertEquals("Item 2 ItemSequenceNumeric", 2, item2.ItemSequenceNumeric);
				AssertEquals("Item 2 ItemSequenceNumericSpecified", true, item2.ItemSequenceNumericSpecified);
				AssertEquals("Item 2 ItemHarmonizedSystemCode", "84733093", item2.ItemHarmonizedSystemCode);
				var item2TariffRefund = item2.TariffRefund;
				AssertEquals("Item 2 CustomsDutyRefundAmount", 0m, item2TariffRefund.CustomsDutyRefundAmount);
				AssertEquals("Item 2 CustomsDutyRefundAmountSpecified", false, item2TariffRefund.CustomsDutyRefundAmountSpecified);
				AssertEquals("Item 2 ExciseDutyRefundAmount", 0m, item2TariffRefund.ExciseDutyRefundAmount);
				AssertEquals("Item 2 ExciseDutyRefundAmountSpecified", false, item2TariffRefund.ExciseDutyRefundAmountSpecified);
				AssertEquals("Item 2 GoodsAndServicesTaxRefundAmount", 40m, item2TariffRefund.GoodsAndServicesTaxRefundAmount);
				AssertEquals("Item 2 GoodsAndServicesTaxRefundAmountSpecified", true, item2TariffRefund.GoodsAndServicesTaxRefundAmountSpecified);
			}

			);
			CombineAssertions("RefundSummary", () =>
			{
				var refundSummary = TradenetDeclaration.InboundMessage.InPaymentUpdate.RefundOnly.RefundSummary;
				AssertEquals("NumberOfItems", 2, refundSummary.NumberOfItems);
				var totalTariffRefund = refundSummary.TotalTariffRefund;
				AssertEquals("TotalCustomsDutyRefundAmount", 10m, totalTariffRefund.TotalCustomsDutyRefundAmount);
				AssertEquals("TotalCustomsDutyRefundAmountSpecified", true, totalTariffRefund.TotalCustomsDutyRefundAmountSpecified);
				AssertEquals("TotalExciseDutyRefundAmount", 20m, totalTariffRefund.TotalExciseDutyRefundAmount);
				AssertEquals("TotalExciseDutyRefundAmountSpecified", true, totalTariffRefund.TotalExciseDutyRefundAmountSpecified);
				AssertEquals("TotalGoodsAndServicesTaxRefundAmount", 70m, totalTariffRefund.TotalGoodsAndServicesTaxRefundAmount);
				AssertEquals("TotalGoodsAndServicesTaxRefundAmountSpecified", true, totalTariffRefund.TotalGoodsAndServicesTaxRefundAmountSpecified);
			}

			);
		}

		void BuildRefundItemData()
		{
			DataProvider.AdditionalMessageInfo.DutyRefundAmount = 110m;
			DataProvider.AdditionalMessageInfo.ExciseRefundAmount = 120m;
			DataProvider.AdditionalMessageInfo.GSTRefundAmount = 130m;
			DataProvider.Items = new ItemsTestClass[] { new ItemsTestClass()
			{ HSCode = "84733091", ItemDutyRefund = 0m, ItemExciseRefund = 0, ItemGSTRefund = 0 }, new ItemsTestClass()
			{ HSCode = "84733092", ItemDutyRefund = 10m, ItemExciseRefund = 20m, ItemGSTRefund = 30m }, new ItemsTestClass()
			{ HSCode = "84733093", ItemDutyRefund = 0, ItemExciseRefund = 0, ItemGSTRefund = 40m } };
		}

		IPTUPDRefund Message => message ?? (message = new IPTUPDRefund(DataProvider));
		IPTUPDRefund message;
		IPTUPDTestClass DataProvider => dataProvider ?? (dataProvider = new IPTUPDTestClass());
		IPTUPDTestClass dataProvider;
		TradenetDeclaration TradenetDeclaration => tradenetDeclaration ?? (tradenetDeclaration = new TradenetDeclaration());
		TradenetDeclaration tradenetDeclaration;
	}
}
