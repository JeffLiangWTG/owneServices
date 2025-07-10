using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Customs.SG.V4.Business.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class IPTUPDCancelTest : TestCaseWithFactory
	{
		public void TestIPTUPDCancelMessage()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.IPTUPD, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Cancellation, Message.MessageSubType);
			AssertNull(TradenetDeclaration.InboundMessage);
			var testIPTUPDCancelBuilder = new IPTUPDCancel(DataProvider);
			testIPTUPDCancelBuilder.Build(TradenetDeclaration);
			AssertType<InPaymentUpdate>(TradenetDeclaration.InboundMessage.InPaymentUpdate);
			AssertType<Update>(TradenetDeclaration.InboundMessage.InPaymentUpdate.Update);
			AssertType<Cancellation>(TradenetDeclaration.InboundMessage.InPaymentUpdate.Cancellation);
			AssertNull(TradenetDeclaration.InboundMessage.InPayment);
			AssertNull(TradenetDeclaration.InboundMessage.InPaymentUpdate.Declaration);
		}

		public void TestCancellation()
		{
			// Update properties
			DataProvider.AdditionalMessageInfo.UpdateIndicator = "TEST";
			DataProvider.AdditionalMessageInfo.ExtendingTemporaryImportPeriod = true;
			DataProvider.JobNumber = "CD2020";
			DataProvider.PermitNoToUpdateOrCancel = "001";
			DataProvider.NumberOfRequestsForUpdate = 1;
			DataProvider.ReplacementPermitNumber = "002";
			DataProvider.AdditionalMessageInfo.ReasonForAmending = "Test Reason For Amending";
			DataProvider.AdditionalMessageInfo.ReasonForExtendingTemporaryImportPeriod = "Test Reason For Extending Temporary Import Period";
			// Cancellation properties
			DataProvider.JobNumber = "CD2020";
			DataProvider.DeclarantId = "SFZ";
			DataProvider.AdditionalRecipients = new ZString[] { "A00", "A11" };
			DataProvider.AdditionalMessageInfo.CancellationCode = "TST";
			DataProvider.Declarant = new AgentInfoTestClass { Name = "Test SG Broker", Phone = "64 85721111", Code = "V13t001" };
			DataProvider.AdditionalMessageInfo.SupportingDocuments = new AttachmentsTestClass[2] { new AttachmentsTestClass { FileName = "ATTDOC_0001", DocType = SupportingDocumentTypeCodeList.Codes.DocType001 }, new AttachmentsTestClass { FileName = "ATTDOC_0002", DocType = SupportingDocumentTypeCodeList.Codes.DocType002 } };
			Message.Build(TradenetDeclaration);
			CombineAssertions("Cancellation", () =>
			{
				var cancellation = TradenetDeclaration.InboundMessage.InPaymentUpdate.Cancellation;
				var header = cancellation.CancellationHeader;
				Assert("DeclarationIndicator", header.DeclarationIndicator);
				Assert("DeclarationIndicatorSpecified", header.DeclarationIndicatorSpecified);
				AssertEquals("MessageReference", "WTGCD2020", header.MessageReference);
				AssertEquals("DeclarantID", SGXmlEDIMessage.SendersReferencePlaceHolderXml, header.DeclarantID);
				AssertEquals("CommonAccessReference", CommonAccessReferenceCodeList.Codes.IPTUPD, header.CommonAccessReference);
				AssertEquals("CancellationReasonCode", "TST", header.CancellationReasonCode);
				AssertArrayEqualsByElements("AdditionalRecipientID", new[] { "A00", "A11" }, header.AdditionalRecipientID);
				var uniqueReferenceNumber = header.UniqueReferenceNumber;
				AssertEquals("UniqueReferenceNumber ID", SGXmlEDIMessage.MessageNumberPlaceHolderXml, uniqueReferenceNumber.ID);
				AssertEquals("UniqueReferenceNumber Date", SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml, uniqueReferenceNumber.Date);
				AssertEquals("UniqueReferenceNumber SequenceNumeric", SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml, uniqueReferenceNumber.SequenceNumeric);
				var declarantParty = cancellation.DeclarantParty;
				AssertEquals("64 85721111", declarantParty.Telephone);
				AssertEquals("V13T001", declarantParty.PersonInformation.CodeValue);
				AssertEquals("TEST SG BROKER", declarantParty.PersonInformation.Name);
				var references = cancellation.SupportingDocumentReference;
				var idAndNames = references.Select(c => $"{c.DocumentID}:{c.Filename}").ToArray();
				AssertArrayEqualsByElements("SupportingDocumentReference", new[] { "001:ATTDOC_0001", "002:ATTDOC_0002" }, idAndNames);
			}

			);
			CombineAssertions("Update", () =>
			{
				var update = TradenetDeclaration.InboundMessage.InPaymentUpdate.Update;
				AssertEquals("UpdateRequestNumber", 1, update.UpdateRequestNumber);
				AssertEquals("UpdateIndicatorCode", "CNL", update.UpdateIndicatorCode);
				AssertEquals("UpdatePermitNumber", "001", update.UpdatePermitNumber);
				AssertEquals("ReplacementPermitNumber", "002", update.ReplacementPermitNumber);
				AssertNull("Amendment", update.Amendment);
			}

			);
		}

		IPTUPDCancel Message => message ?? (message = new IPTUPDCancel(DataProvider));
		IPTUPDCancel message;
		IPTUPDTestClass DataProvider => dataProvider ?? (dataProvider = new IPTUPDTestClass());
		IPTUPDTestClass dataProvider;
		TradenetDeclaration TradenetDeclaration => tradenetDeclaration ?? (tradenetDeclaration = new TradenetDeclaration());
		TradenetDeclaration tradenetDeclaration;
	}
}
