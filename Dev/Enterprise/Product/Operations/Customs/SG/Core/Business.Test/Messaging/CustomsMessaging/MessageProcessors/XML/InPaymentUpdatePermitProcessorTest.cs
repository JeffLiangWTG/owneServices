using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class InPaymentUpdatePermitProcessorTest : BasePermitSectionProcessorTest<InPaymentUpdatePermit>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcess()
		{
			var applicationReference = "199702247W202005200800";
			var messageText = LoadXmlFile("IPTUPT.xml");
			var status = Core.SGConstants.DeclarationStatus.RefundPermitReceived;

			var tradenetResponse = AssertProcessValidMessage(string.Empty, status, applicationReference, messageText);

			var entryHeader = GetEntryHeader(MessageProcessor);
			var email = GetResponseEmail(MessageProcessor);

			CombineAssertions(() =>
			{
				var tradeNetOutSection = tradenetResponse.OutboundMessage.InPaymentUpdatePermit;
				var permit = tradeNetOutSection.Permit;
				var authorisationDate = new ZDateTime(2011, 02, 14, 10, 50, 00);

				AssertPermitInfos(entryHeader, permit, authorisationDate);
				AssertEmailContent(email, permit, entryHeader.Declaration, applicationReference, status, entryHeader.Messages.LastIncomingMessage.EM_MessageInterpretation);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = string.Empty;

			var outgoingMessage = Factory.New<SGXmlEDIMessage>();
			entry.Messages.Add(outgoingMessage);

			var incomingMessage = Factory.New<SGXmlEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = LoadXmlFile("IPTUPT.xml");

			Factory.Save();

			outgoingMessage.EM_ApplicationReference = "199702247W202005200800";
			Factory.Save();

			var update = incomingMessage.TradenetResponse.OutboundMessage.InPaymentUpdatePermit.Update;

			void AssertUpdateEntryStatus(string updateIndicatorCode, bool expectedShouldAttachPermitPrint, string expectedEntryStatus)
			{
				update.UpdateIndicatorCode = updateIndicatorCode;

				var messageProcessor = GetMessageProcessor(incomingMessage.TradenetResponse);
				messageProcessor.ProcessMessage(incomingMessage);

				AssertEquals($"Should update the entry status to {expectedEntryStatus} when the UpdateIndicatorCode is '{updateIndicatorCode}'.", expectedEntryStatus, entry.CH_Status);

				var shouldAttachPermitPrint = (bool)GetPropertyValueCore(messageProcessor, "ShouldAttachPermitPrint");
				AssertEquals("Should only be true when the NeedToUpdateRefund is false.", expectedShouldAttachPermitPrint, shouldAttachPermitPrint);
			}

			CombineAssertions(() =>
			{
				AssertUpdateEntryStatus(string.Empty, true, Core.SGConstants.DeclarationStatus.AmendmentPermitReceived);
				AssertUpdateEntryStatus(SGConstants.UpdateIndicators.AME, true, Core.SGConstants.DeclarationStatus.AmendmentPermitReceived);
				AssertUpdateEntryStatus(SGConstants.UpdateIndicators.CNL, true, Core.SGConstants.DeclarationStatus.AmendmentPermitReceived);

				AssertUpdateEntryStatus(SGConstants.UpdateIndicators.AMR, false, Core.SGConstants.DeclarationStatus.RefundPermitReceived);
				AssertUpdateEntryStatus(SGConstants.UpdateIndicators.PRG, false, Core.SGConstants.DeclarationStatus.RefundPermitReceived);
				AssertUpdateEntryStatus(SGConstants.UpdateIndicators.FRF, false, Core.SGConstants.DeclarationStatus.RefundPermitReceived);
				AssertUpdateEntryStatus(SGConstants.UpdateIndicators.PRS, false, Core.SGConstants.DeclarationStatus.RefundPermitReceived);
			});
		}

		public void TestUpdateEntryStatusForRefund()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = string.Empty;

			var outgoingMessage = Factory.New<SGXmlEDIMessage>();
			entry.Messages.Add(outgoingMessage);

			var incomingMessage = Factory.New<SGXmlEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = @"<TradenetResponse xmlns:inp=""urn:crimsonlogic:tn:schema:xsd:InNonPayment"" xmlns:app=""urn:crimsonlogic:tn:schema:xsd:ApprovalMessage"" dateTime=""202106221423"" xmlns:ipt=""urn:crimsonlogic:tn:schema:xsd:InPayment"" xmlns:rej=""urn:crimsonlogic:tn:schema:xsd:RejectionMessage"" xmlns:fee=""urn:crimsonlogic:tn:schema:xsd:FeeMessage"" xmlns=""urn:crimsonlogic:tn:schema:xsd:TradenetResponse"" xmlns:err=""urn:crimsonlogic:tn:schema:xsd:ErrorMessage"" xmlns:cac=""urn:crimsonlogic:tn:schema:xsd:CommonAggregateComponents-2"" xmlns:com=""urn:crimsonlogic:tn:schema:xsd:CustomsCommonCode"" xmlns:cbc=""urn:crimsonlogic:tn:schema:xsd:CommonBasicComponents-2"" xmlns:tnp=""urn:crimsonlogic:tn:schema:xsd:TranshipmentMovement"" xmlns:coo=""urn:crimsonlogic:tn:schema:xsd:CertificateOfOrigin"" xmlns:out=""urn:crimsonlogic:tn:schema:xsd:OutwardDeclaration"" xmlns:cux=""urn:crimsonlogic:tn:schema:xsd:CustomsExchangeRate"" instanceIdentifier=""91124855"">
	<cbc:MessageVersion>041</cbc:MessageVersion>
	<cbc:SenderID>DCS4.DCS4001</cbc:SenderID>
	<cbc:RecipientID>UPS1.UPS1023</cbc:RecipientID>
	<cbc:TotalNumberOfResponse>1</cbc:TotalNumberOfResponse>
	<OutboundMessage>
		<ipt:InPaymentUpdatePermit>
			<cac:Update>
				<cbc:UpdateIndicatorCode>FRF</cbc:UpdateIndicatorCode>
				<cbc:UpdateRequestNumber>1</cbc:UpdateRequestNumber>
				<cbc:ReplacementPermitNumber>ME1C121011B</cbc:ReplacementPermitNumber>
				<cac:Refund>
					<cbc:RefundReferenceNumber>RFM2021068184</cbc:RefundReferenceNumber>
					<cbc:ReasonCode>RF05</cbc:ReasonCode>
				</cac:Refund>
			</cac:Update>
			<cac:RefundOnly>
				<cac:RefundHeader>
					<cbc:MessageReference>WTGB04340433</cbc:MessageReference>
					<cac:UniqueReferenceNumber>
						<cbc:ID>198801949D</cbc:ID>
						<cbc:Date>20210330</cbc:Date>
						<cbc:SequenceNumeric>3002</cbc:SequenceNumeric>
					</cac:UniqueReferenceNumber>
					<cbc:CommonAccessReference>IPTUPT</cbc:CommonAccessReference>
					<cbc:DeclarationIndicator>true</cbc:DeclarationIndicator>
				</cac:RefundHeader>
				<cac:DeclarantParty>
					<cac:PersonInformation>
						<cbc:CodeValue>S9476874H</cbc:CodeValue>
						<cbc:Name>GAGANDEEP KAUR BHULLAR</cbc:Name>
					</cac:PersonInformation>
					<cbc:Telephone>+6563028063</cbc:Telephone>
				</cac:DeclarantParty>
				<cac:SupportingDocumentReference>
					<cbc:DocumentID>999</cbc:DocumentID>
					<cbc:Filename>MSC - 1A719RTKHQL.PDF</cbc:Filename>
				</cac:SupportingDocumentReference>
				<cac:RefundSummary>
					<cac:TotalTariffRefund>
						<cbc:TotalGoodsAndServicesTaxRefundAmount>69.99</cbc:TotalGoodsAndServicesTaxRefundAmount>
					</cac:TotalTariffRefund>
				</cac:RefundSummary>
			</cac:RefundOnly>
			<cac:Permit>
				<cbc:PermitNumber>IG0D328420A</cbc:PermitNumber>
				<cbc:PermitApprovalDatetime>20210622142331SST</cbc:PermitApprovalDatetime>
				<cac:RefundApprovalCondition>
					<cbc:ConditionCode>A00</cbc:ConditionCode>
					<cbc:ConditionDescription>APPROVED BY SINGAPORE CUSTOMS.</cbc:ConditionDescription>
				</cac:RefundApprovalCondition>
				<cac:RefundApprovalCondition>
					<cbc:ConditionCode>C02</cbc:ConditionCode>
					<cbc:ConditionDescription>REFUND APPROVED BY SINGAPORE CUSTOMS.</cbc:ConditionDescription>
				</cac:RefundApprovalCondition>
			</cac:Permit>
		</ipt:InPaymentUpdatePermit>
	</OutboundMessage>
</TradenetResponse>";

			Factory.Save();

			outgoingMessage.EM_ApplicationReference = "198801949D202103303002";
			Factory.Save();

			var update = incomingMessage.TradenetResponse.OutboundMessage.InPaymentUpdatePermit.Update;
			update.UpdateIndicatorCode = SGConstants.UpdateIndicators.FRF;
			var messageProcessor = GetMessageProcessor(incomingMessage.TradenetResponse);
			messageProcessor.ProcessMessage(incomingMessage);
			AssertEquals("Should update the entry status to ROK.", Core.SGConstants.DeclarationStatus.RefundPermitReceived, entry.CH_Status);
		}

		protected override MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse) => new InPaymentUpdatePermitProcessor(new LoggingInformation(), tradenetResponse.OutboundMessage.InPaymentUpdatePermit);
	}
}
