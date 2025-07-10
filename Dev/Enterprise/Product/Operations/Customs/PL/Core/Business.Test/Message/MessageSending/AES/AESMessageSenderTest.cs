using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESMessageSenderTest : AbstractMessageSenderTest
{
	public void TestIsCorrectionAmendment()
	{
		CombineAssertions(() =>
		{
			AssertSendMessageSubType(ExportMessageSendingObjectActionList.Codes.CC511, AESMessageCodes.Descriptions.CC511);
			AssertSendMessageSubType(ExportMessageSendingObjectActionList.Codes.CC513, AESMessageCodes.Descriptions.CC513);
			AssertSendMessageSubType(ExportMessageSendingObjectActionList.Codes.CC514, AESMessageCodes.Descriptions.CC514);
			AssertSendMessageSubType(ExportMessageSendingObjectActionList.Codes.CC515, AESMessageCodes.Descriptions.CC515);
			AssertSendMessageSubType(ExportMessageSendingObjectActionList.Codes.CC566, AESMessageCodes.Descriptions.CC566);
		});

		void AssertSendMessageSubType(ZString actionType, ZString expectedSubType)
		{
			sendingObject.Action = actionType;
			var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
			var message = messageSender.Send();

			AssertEquals($"EM_MessageSubType for {actionType}:", expectedSubType, message.EM_MessageSubType);
		}
	}

	public void TestMessageWithEvidences_ShouldReturnExpectedCodes()
	{
		var expectedCodes = new[] { ExportMessageSendingObjectActionList.Codes.CC583 };

		var actualCodes = AESMessageSender.MessageWithEvidences;
		AssertSequencesEqual(expectedCodes, actualCodes);
	}

	public void TestMessageText_CC513()
	{
		PrepareTestData();
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC513;
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();
		factory.Save();

		CombineAssertions(() =>
		{
			AssertNotEquals("EM_MessageData should contain CC513C xml Message", 0, message.EM_MessageData.Length);

			var expectedRootElement = "xsdaes:CC513C";
			var expectedNodes = new ZString[] { "ExportOperation", "Declarant", "Representative", "GoodsShipment" };
			AssertXmlMessage(message.EM_MessageText, expectedRootElement, expectedNodes);
		});
	}

	public void TestMessageText_CC514()
	{
		PrepareTestData();
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();
		factory.Save();

		CombineAssertions(() =>
		{
			AssertNotEquals("EM_MessageData should contain CC514C xml Message", 0, message.EM_MessageData.Length);

			var expectedRootElement = "xsdaes:CC514C";
			var expectedNodes = new ZString[] { "Declarant", "Representative" };
			AssertXmlMessage(message.EM_MessageText, expectedRootElement, expectedNodes);
		});
	}

	public void TestMessageText_CC515()
	{
		PrepareTestData();
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC515;
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();
		factory.Save();

		CombineAssertions(() =>
		{
			AssertNotEquals("EM_MessageData should contain CC515C xml Message", 0, message.EM_MessageData.Length);

			var expectedRootElement = "xsdaes:CC515C";
			var expectedNodes = new ZString[] { "ExportOperation", "Declarant", "Representative", "GoodsShipment" };
			AssertXmlMessage(message.EM_MessageText, expectedRootElement, expectedNodes);
		});
	}

	public void TestMessageText_CC566()
	{
		PrepareTestData();
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC566;
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();
		factory.Save();

		CombineAssertions(() =>
		{
			AssertNotEquals("EM_MessageData should contain CC566C xml Message", 0, message.EM_MessageData.Length);

			var expectedRootElement = "xsdaes:CC566C";
			var expectedNodes = new ZString[] { "ExportOperation", "PdWResponse" };
			AssertXmlMessage(message.EM_MessageText, expectedRootElement, expectedNodes);
		});
	}

	public void TestMessageText_CC583()
	{
		PrepareTestData();
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC583;
		sendingObject.EntryNumber = "12345678901234567890";
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();
		factory.Save();
		CombineAssertions(() => {
			AssertNotEquals("EM_MessageData should contain CC583C xml Message", 0, message.EM_MessageData.Length);
			var expectedRootElement = "xsdaes:CC583C";
			var expectedNodes = new ZString[] { "ExportOperation", "Declarant", "Representative" };
			AssertXmlMessage(message.EM_MessageText, expectedRootElement, expectedNodes);
		});
	}

	public void TestCreateOrUpdateNoteForAmendmentInvalidationReason_MultipleSendsForDifferentEntryHeaders()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction1 = declaration.CustomsEntryInstructions.AddNew();
		instruction1.CEI_Procedure = "1";
		var instruction2 = declaration.CustomsEntryInstructions.AddNew();
		instruction2.CEI_Procedure = "2";
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = instruction1.PK;
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = instruction2.PK;

		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		Factory.Save();

		var sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
		var sendingObject1 = (BaseMessageSendingObject)sendingObjectParent.SendingObjectsCollection.First();
		var sendingObject2 = (BaseMessageSendingObject)sendingObjectParent.SendingObjectsCollection.Last();

		sendingObject1.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject1.AmendmentInvalidationReason = "ASD";
		sendingObject2.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject2.AmendmentInvalidationReason = "QWE";
		var messageSender1 = new AESMessageSender(factory, sendingObject1, sendingObjectParent);
		var messageSender2 = new AESMessageSender(factory, sendingObject2, sendingObjectParent);
		messageSender1.Send();
		messageSender2.Send();
		factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("Header1 should have the same CH_CustomsMessageRemarks as sendingObject1 AmendmentInvalidationReason", "ASD", sendingObject1.Header.CH_CustomsMessageRemarks);
			AssertEquals("Header2 should have the same CH_CustomsMessageRemarks as sendingObject2 AmendmentInvalidationReason", "QWE", sendingObject2.Header.CH_CustomsMessageRemarks);
		});
	}

	public void TestCreateOrUpdateNoteForAmendmentInvalidationReason_UpdateNoteForTheSameEntryHeader()
	{
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject.AmendmentInvalidationReason = "ABC";
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		messageSender.Send();
		sendingObject.AmendmentInvalidationReason = "ZXC";
		messageSender.Send();
		sendingObject.AmendmentInvalidationReason = ZString.Empty;
		messageSender.Send();
		factory.Save();

		AssertEquals("ZXC", sendingObject.Header.CH_CustomsMessageRemarks);
	}

	public void TestCreateOrUpdateNoteForAmendmentInvalidationReason_MultipleSendsForTheSameEntryHeader()
	{
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject.AmendmentInvalidationReason = "ABC";
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		messageSender.Send();
		messageSender.Send();
		factory.Save();

		AssertEquals("ABC", sendingObject.Header.CH_CustomsMessageRemarks);
	}

	public void TestCreateOrUpdateNoteForAmendmentInvalidationReason()
	{
		var actionList = new[]
		{
			ExportMessageSendingObjectActionList.Codes.CC514
			, ExportMessageSendingObjectActionList.Codes.CC513
			, ExportMessageSendingObjectActionList.Codes.CC515
			, string.Empty
		};
		foreach (var action in actionList)
		{
			sendingObject.Action = action;
			sendingObject.AmendmentInvalidationReason = $"Text for {action}";
			var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
			messageSender.Send();
		}

		factory.Save();
		AssertEquals("Only CC514 should make amendment note", $"Text for {ExportMessageSendingObjectActionList.Codes.CC514}", sendingObject.Header.CH_CustomsMessageRemarks);
	}

	public void TestCreateOrUpdateNoteForAmendmentInvalidationReason_Empty()
	{
		sendingObject.Action = ExportMessageSendingObjectActionList.Codes.CC514;
		sendingObject.AmendmentInvalidationReason = "QWE";
		var messageSender = new AESMessageSender(factory, sendingObject, sendingObjectParent);
		messageSender.Send();
		sendingObject.AmendmentInvalidationReason = ZString.Empty;
		messageSender.Send();

		factory.Save();

		AssertEquals("On empty AmendmentInvalidationReason update CH_CustomsMessageRemarks for should be ignored", "QWE", sendingObject.Header.CH_CustomsMessageRemarks);
	}

	void PrepareTestData()
	{
		var organisation = Factory.New<OrgHeader>();
		organisation.OH_Code = "Org1";

		var organisationAddress = Factory.NewWithValidTestData<OrgAddress>();
		var customsCodeEori = organisationAddress.CustomsCodes.AddNew();
		customsCodeEori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		customsCodeEori.OK_CustomsRegNo = "123456";

		declaration.ExporterDocAddress.OrganisationPK = organisation.PK;
		declaration.SupplierDocumentaryAddress.E2_OA_Address = organisationAddress.PK;
		declaration.ImporterDocumentaryAddress.E2_OA_Address = organisationAddress.PK;
		declaration.ExporterDocAddress.E2_OA_Address = organisationAddress.PK;
		declaration.JE_OA_DeclarantAddress = organisationAddress.PK;
		declaration.JE_OA_Representative = organisationAddress.PK;
		declaration.JE_DeclarantType = PLRepresentationTypeList.Codes._4Direct;
		Factory.Save();
	}
}
