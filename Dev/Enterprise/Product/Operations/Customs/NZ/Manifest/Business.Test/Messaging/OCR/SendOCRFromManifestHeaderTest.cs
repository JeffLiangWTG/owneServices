using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class SendOCRFromManifestHeaderTest : NZ.Business.TradeSingleWindow.Testing.SendOCRBaseTest
	{
		public void TestSendCancellation()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				var ocrSender = GetNewSender(TSWTransactionTypes.Cancel);
				ocrSender.SendMessage();
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation, header.Messages[0].EM_MessageSubType);
			}
		}

		public void TestSendReplacement()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				var ocrSender = GetNewSender(TSWTransactionTypes.Replace);
				ocrSender.SendMessage();
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Replacement, header.Messages[0].EM_MessageSubType);
			}
		}

		public void TestMessageStatusSet()
		{
			header.Bills.AddNew();
			header.Bills.AddNew();
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				var ocrSender = GetNewSender(TSWTransactionTypes.Original);
				ocrSender.SendMessage();
				CombineAssertions(() =>
				{
					AssertEquals("AMA_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, header.AMA_MessageStatus);
					AssertEquals("First ABL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, header.Bills[0].ABL_MessageStatus);
					AssertEquals("Second ABL_MessageStatus", ASYCUDA.Business.MessageStatusCodeList.Codes.Sent, header.Bills[1].ABL_MessageStatus);
				});
			}
		}

		public void TestTSWOCRMessageInTestMode()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				using (NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertTestMode(true);
				}
			}
		}

		public void TestTSWOCRMessageNotInTestMode()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				using (NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertTestMode(false);
				}
			}
		}

		public void TestSendOCR()
		{
			using (NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				header.AMA_JobReference = "test123";
				header.Validation.ValidateAll();
				var sender = new SendOCRFromManifestHeaderForTest(header, null, TSWTransactionTypes.Original);
				AssertEquals(ZString.Empty, sender.DeclarantPinEncrypted);
				Assert(!sender.DeclarantPinRequired);
				AssertEquals("test123", sender.ApplicationReference);
				AssertEquals(MessageTypeList.Codes.OCR, sender.MessageType);
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Original, sender.MessageSubType);
				Assert(sender.GetIsMessageInTestMode);
				AssertEquals(0, header.Messages.Count);
				var message = Factory.New<TSWMessage>();
				sender.AddMessageToMessages(message);
				AssertEquals(1, header.Messages.Count);
				AssertContains("Manifest Type: At least one bill (shipment) is required.", sender.GetBOValidationMessageErrors());
			}
		}

		public void TestStatusTransactionScopeRollback()
		{
			header.Bills.AddNew();
			header.Bills.AddNew();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var ocrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => ocrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals("AMA_MessageStatus", "", header.AMA_MessageStatus);
					AssertEquals("First ABL_MessageStatus", "", header.Bills[0].ABL_MessageStatus);
					AssertEquals("Second ABL_MessageStatus", "", header.Bills[1].ABL_MessageStatus);
					AssertEquals("Messages.Count", 0, header.Messages.Count);
				});
			}
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType) => new SendOCRFromManifestHeader(header, null, transactionType);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader header;

		void AssertTestMode(bool isTestMode)
		{
			var ocrSender = GetNewSender(TSWTransactionTypes.Original);
			ocrSender.SendMessage();
			AssertEquals(isTestMode, header.Messages[0].EM_IsTestMessage);
		}

		sealed class SendOCRFromManifestHeaderForTest : SendOCRFromManifestHeader
		{
			public SendOCRFromManifestHeaderForTest(AsycudaManifestHeader manifestHeader, IAdditionalInformation additionalMessageInformation, TSWTransactionTypes transactionType) : base(manifestHeader, additionalMessageInformation, transactionType)
			{
			}

			internal new ZString MessageType => base.MessageType;
			internal new ZString MessageSubType => base.MessageSubType;
			internal new ZBool GetIsMessageInTestMode => base.GetIsMessageInTestMode;
			internal new void AddMessageToMessages(TSWMessage message) => base.AddMessageToMessages(message);
		}
	}
}
