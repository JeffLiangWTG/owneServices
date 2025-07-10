using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class TNPUPDCancelTest : TestCaseWithFactory
	{
		public void TestCancellation()
		{
			DataProvider.DeclarantId = "SFZ";
			DataProvider.AdditionalRecipients = new ZString[] { "A00t001", "A11t002" };
			DataProvider.AdditionalMessageInfo.CancellationCode = "TST";
			var testDeclarant = new AgentInfoTestClass { Name = "Test SG Broker", Phone = "64 85721111", Code = "V13t001" };
			DataProvider.Declarant = testDeclarant;
			var attachments = new AttachmentsTestClass[2];
			attachments[0] = new AttachmentsTestClass { FileName = "ATTDOC_0001", DocType = SupportingDocumentTypeCodeList.Codes.DocType001 };
			attachments[1] = new AttachmentsTestClass { FileName = "ATTDOC_0002", DocType = SupportingDocumentTypeCodeList.Codes.DocType002 };
			DataProvider.AdditionalMessageInfo.SupportingDocuments = attachments;
			Message.Build(TradenetDeclaration);
			var cancellation = TradenetDeclaration.InboundMessage.TranshipmentMovementUpdate.Cancellation;
			var header = cancellation.CancellationHeader;
			Assert("DeclarationIndicator", header.DeclarationIndicator);
			Assert("DeclarationIndicatorSpecified", header.DeclarationIndicatorSpecified);
			AssertEquals("DeclarantID", "# SENDERS REFERENCE PLACE HOLDER #", header.DeclarantID);
			AssertEquals("CommonAccessReference", CommonAccessReferenceCodeList.Codes.TNPUPD, header.CommonAccessReference);
			AssertEquals("CancellationReasonCode", "TST", header.CancellationReasonCode);
			var uniqueReferenceNumber = header.UniqueReferenceNumber;
			AssertEquals("UniqueReferenceNumber ID", SGXmlEDIMessage.MessageNumberPlaceHolderXml, uniqueReferenceNumber.ID);
			AssertEquals("UniqueReferenceNumber Date", SGXmlEDIMessage.MessageDateTimeCreatePlaceHolderXml, uniqueReferenceNumber.Date);
			AssertEquals("UniqueReferenceNumber SequenceNumeric", SGXmlEDIMessage.UniqueBatchNumberPlaceHolderXml, uniqueReferenceNumber.SequenceNumeric);
			AssertArrayEqualsByElements("AdditionalRecipientID - these should be UPPERCASE", new[] { "A00T001", "A11T002" }, header.AdditionalRecipientID);
			var declarantParty = cancellation.DeclarantParty;
			AssertEquals("64 85721111", declarantParty.Telephone);
			AssertEquals("V13T001", declarantParty.PersonInformation.CodeValue);
			AssertEquals("TEST SG BROKER", declarantParty.PersonInformation.Name);
			var references = cancellation.SupportingDocumentReference;
			var idAndNames = references.Select(c => $"{c.DocumentID}:{c.Filename}").ToArray();
			AssertArrayEqualsByElements("SupportingDocumentReference", new[] { "001:ATTDOC_0001", "002:ATTDOC_0002" }, idAndNames);
		}

		public void TestMessageTypeAndSubType()
		{
			AssertEquals(CommonAccessReferenceCodeList.Codes.TNPUPD, Message.MessageType);
			AssertEquals(CUSDECEDIMessage.Cancellation, Message.MessageSubType);
		}

		TNPUPDCancel Message => message ?? (message = new TNPUPDCancel(DataProvider));
		TNPUPDCancel message;
		ITNPUPDTestClass DataProvider => dataProvider ?? (dataProvider = new ITNPUPDTestClass());
		ITNPUPDTestClass dataProvider;
		TradenetDeclaration TradenetDeclaration => tradenetDeclaration ?? (tradenetDeclaration = new TradenetDeclaration());
		TradenetDeclaration tradenetDeclaration;
	}
}
