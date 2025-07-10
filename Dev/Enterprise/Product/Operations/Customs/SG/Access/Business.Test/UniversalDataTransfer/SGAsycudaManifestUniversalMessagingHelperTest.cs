using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer
{
	sealed class SGAsycudaManifestUniversalMessagingHelperTest : ASYCUDA.Business.UniversalDataTransfer.Testing.AsycudaManifestUniversalMessagingHelperTest
	{
		public void TestSendSGAccessMessageViaEHub()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
			var source = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			source.AMA_RL_NKPortOfLoading = "SGSIN";
			source.AMA_RL_NKPortOfDischarge = "DEFRA";
			source.AMA_JobReference = "I have changes now";
			source.AMA_E_ARV = ZDateTime.Now;
			source.AMA_TransportMode = Core.Constants.TransportModes.Air;
			source.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			source.AMA_ManifestType = "MGE";
			source.RegistrationStatus = "IP";
			var bill = source.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var bill2 = source.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			packedItem2.API_MessageStatus = "ERR";
			GetNewAsycudaManifestUniversalMessagingHelper().SendViaEHub(source, "MGE", MessageSubTypeCodes.Codes.Original, new List<IMessageParent> { packedItem });
			var queuedMessage = source.Messages[0];
			AssertEquals("UDM", queuedMessage.EM_ApplicationCode);
			AssertEquals("XUS", queuedMessage.EM_MessageType);
			AssertEquals("XUS", queuedMessage.EM_MessageSubType);
			AssertEquals("TRX", queuedMessage.EM_ReceiveTransmit);
			AssertEquals("SNT", queuedMessage.EM_Status);
			AssertEquals("HQU", queuedMessage.Interchange.EI_Status);
			AssertEquals("SGCustoms", queuedMessage.Interchange.EI_To);
			AssertNotEquals("", queuedMessage.Interchange.eHubID);
			CombineAssertions(() =>
			{
				AssertContains("ActionPurpose", "<ActionPurpose>\r\n        <Code>AED</Code>", queuedMessage.EM_MessageText);
				AssertContains("ActionPurpose", "<Description>SG Export ACCESS Manifest</Description>", queuedMessage.EM_MessageText);
				AssertContains("EventType", "<EventType>\r\n        <Code>MRS</Code>", queuedMessage.EM_MessageText);
				AssertContains("EventReference", "<EventReference>|MSB=ORG|MST=MGE</EventReference>", queuedMessage.EM_MessageText);
				AssertEquals("SNT", packedItem.API_MessageStatus);
				AssertEquals("SNT", bill.ABL_MessageStatus);
				AssertEquals("ERR", source.AMA_MessageStatus);
				AssertEquals("", source.RegistrationStatus);
			});
		}

		public void TestSendSGAccessMessageViaEHub_MessageStatusNotUpdatedWhenFactorySavingFailed() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
			var source = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			source.AMA_RL_NKPortOfLoading = "SGSIN";
			source.AMA_RL_NKPortOfDischarge = "DEFRA";
			source.AMA_JobReference = "I have changes now";
			source.AMA_E_ARV = ZDateTime.Now;
			source.AMA_TransportMode = Core.Constants.TransportModes.Air;
			source.AMA_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			source.AMA_ManifestType = "MGE";
			source.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			source.RegistrationStatus = "IP";
			var bill = source.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.API_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			var bill2 = source.Bills.AddNew();
			var pack2 = bill2.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			packedItem2.API_MessageStatus = "ERR";
			var messagingHelper = new AsycudaManifestUniversalMessagingHelperForTestingMessageStatusIssue(new TestLogger(), null);
			messagingHelper.SendViaEHub(source, "MGE", MessageSubTypeCodes.Codes.Original, new List<IMessageParent> { packedItem });
			AssertNoExceptionThrown(() => source.Factory.Save());
			AssertEquals("No message was created and sent", 0, source.Messages.Count);
			AssertEquals("API_MessageStatus doesn't get updated when message not sent", MessageStatusCodeList.Codes.NotSent, packedItem.API_MessageStatus);
			AssertEquals("AMA_MessageStatus doesn't get updated when message not sent", MessageStatusCodeList.Codes.Awaiting, source.AMA_MessageStatus);
			AssertEquals("RegistrationStatus doesn't get updated when message not sent", "IP", source.RegistrationStatus);
		});

		public void TestSendIMAccessMessageViaEHub()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore);
			ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SGSIN";
			header.AMA_RL_NKPortOfDischarge = "DEFRA";
			header.AMA_JobReference = "I have changes now";
			header.AMA_E_ARV = ZDateTime.Now;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = SG.Access.Business.Constants.ManifestType.Import;
			var bill1 = header.Bills.AddNew();
			bill1.CycleDate = new ZDateTime(2018, 6, 6);
			bill1.CycleNumber = "6";
			var pack = bill1.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var bill2 = header.Bills.AddNew();
			bill2.CycleDate = new ZDateTime(2018, 7, 7);
			bill2.CycleNumber = "7";
			var pack2 = bill2.Packs.AddNew();
			var packedItem2 = pack2.PackedItem;
			packedItem2.API_MessageStatus = "ERR";
			var chooser = new MessageChooser(header, new List<AsycudaBill> { bill1, bill2 }, true);
			chooser.CycleDate = new ZDateTime(2018, 6, 9);
			chooser.CycleNumber = "9";
			new SGAsycudaManifestUniversalMessagingHelper(new TestLogger(), chooser).SendViaEHub(header, "MGI", MessageSubTypeCodes.Codes.Original, new List<IMessageParent> { packedItem });
			var queuedMessage = header.Messages[0];
			AssertContains("ActionPurpose", @"      <AddInfo>
        <Key>CycleDate</Key>
        <Value>2018-06-09T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>CycleNumber</Key>
        <Value>9</Value>
      </AddInfo>", queuedMessage.EM_MessageText);
		}

		public void TestGetSGAccessActionPurpose()
		{
			var description = "SG Export ACCESS Manifest";
			AssertActionPurpose("MGE", "ORG", "AED", description);
			AssertActionPurpose("MGE", "CHG", "AEU", description);
			AssertActionPurpose("MGE", "CNL", "AEC", description);
			AssertActionPurpose("MGE", "CNM", "AEX", description);
			description = "SG Import ACCESS Manifest";
			AssertActionPurpose("MGI", "ORG", "PCM", description);
			AssertActionPurpose("MGI", "CNL", "PCU", description);
			AssertActionPurpose("MGI", "CNM", "PCX", description);
		}

		protected override ASYCUDA.Business.AsycudaManifestHeader SetUpLargeManifestForDontConvertXmlToHtmlTest() => SetUpManifestForEhubTest(Factory, "SGSIN", SGManifestTypes.Codes.MGI);

		protected override AsycudaManifestUniversalMessagingHelper GetNewAsycudaManifestUniversalMessagingHelper() => new SGAsycudaManifestUniversalMessagingHelper(new TestLogger(), null);

		void AssertActionPurpose(string messageType, string messageSubType, string expectedCode, string expectedDescription)
		{
			var actionPurpose = SGAsycudaManifestUniversalMessagingHelper.GetSGAccessActionPurpose(messageType, messageSubType);
			AssertEquals("Code", expectedCode, actionPurpose.Code);
			AssertEquals("Description", expectedDescription, actionPurpose.Description);
		}

		sealed class TestLogger : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}

		sealed class AsycudaManifestUniversalMessagingHelperForTestingMessageStatusIssue : SGAsycudaManifestUniversalMessagingHelper
		{
			public AsycudaManifestUniversalMessagingHelperForTestingMessageStatusIssue(INotifications notifications, ICycleDetailSupporter cycleDetailSupporter)
				: base(notifications, cycleDetailSupporter)
			{
			}

			protected override void SaveChanges(BusinessObjectFactory factory)
			{
				throw new Exception();
			}

			protected override string CalculateMessageStatus(IMessageParent messageParent) => MessageStatusCodeList.Codes.Sent;

			protected override ZString GetRecipientID(ASYCUDA.Business.AsycudaManifestHeader header) => "RECIPIENT";
		}
	}
}
