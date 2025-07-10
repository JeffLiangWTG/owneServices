using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	public class SendOCRFromConsolTest : SendOCRBaseTest
	{
		public void TestDeclarantPinRequired()
		{
			var ocrSender = GetNewSender(TSWTransactionTypes.Original);
			Assert(!ocrSender.DeclarantPinRequired);
		}

		public void TestValidationIsNotRunForCancellationMessages()
		{
			var ocrSender = GetNewSender(TSWTransactionTypes.Cancel);
			AssertEquals(0, ocrSender.ErrorCount);
		}

		public void TestApplicationReference()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				consol.JK_UniqueConsignRef = "C000007529";
				var msgRefNum = Common.CusEntryNumber.New(consol, CusEntryNumberTypeList.Codes.OutwardReportNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				msgRefNum.CE_EntryLineReference = "OCR00000001";

				var ocrSender = GetNewSender(TSWTransactionTypes.Original);
				ocrSender.SendMessage();
				AssertEquals("OCR00000001", consol.Messages[0].EM_ApplicationReference);
			}
		}

		public void TestErrors()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				var ocrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertEquals(9, ocrSender.ErrorCount);
				var errorsExpected =
@"Carrier is not valid. Please enter a valid Carrier for this consol.
ORN can be sent only for Air Or Sea.
Voyage or flight no. is empty for this consol.
Master Bill Number (BOL) is empty for this consol.
Port of loading is not valid.
Port of discharge is not valid.
You have not attached any shipments yet.
Estimated departure date is not valid.
Either Delivery Notification Organization, Delivery Notification Port or both Delivery Notification Party and email should be entered to ensure that the CCA receives delivery advices.
";
				AssertEquals(errorsExpected, ocrSender.Errors);

				var shippingLine = Factory.New<OrgHeader>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "87273828";
				shipment.CustomsEntryNumberType = "CUS";
				shipment.CustomsEntryNumber = "2928482";
				consol.JK_TransportMode = JobTransportModeList.Codes.Air;
				consol.JK_MasterBillNum = "APLU13102015";
				var consolTransport = consol.Transports[0];
				consolTransport.JW_OA_CarrierAddress = shippingLine.MainAddress.PK;
				consolTransport.JW_VoyageFlight = "QF115";
				consolTransport.JW_ETD = ZDateTime.Today;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";

				consol.SetSystemDefinedValue(OutwardReportManifestStatus.Schema.DeliveryNotificationPartyName, (ZString)"TEST");
				consol.SetSystemDefinedValue(OutwardReportManifestStatus.Schema.DeliveryNotificationPartyEmail, (ZString)"TEST@MAIL.COM");

				ocrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertEquals(ZString.Empty, ocrSender.Errors);
				AssertEquals(0, ocrSender.ErrorCount);
			}
		}

		public void TestSendMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				consol.JK_UniqueConsignRef = "C000007529";
				var ocrSender = GetNewSender(TSWTransactionTypes.Original);
				ocrSender.SendMessage();

				AssertEquals(OutwardReportStatusList.Descriptions.AwaitingResponse, manifestStatus.E2_MessageStatus);
				AssertEquals(1, consol.Messages.Count);
				var sentMessage = consol.Messages[0];
				AssertEquals(MessageTypeList.Codes.OCR, sentMessage.EM_MessageType);
				AssertEquals("C000007529", sentMessage.EM_ApplicationReference);
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Original, sentMessage.EM_MessageSubType);
			}
		}

		public void TestSendCancellation()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				var ocrSender = GetNewSender(TSWTransactionTypes.Cancel);
				ocrSender.SendMessage();
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Cancellation, consol.Messages[0].EM_MessageSubType);
			}
		}

		public void TestSendReplacement()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "87937571Q"))
			{
				var ocrSender = GetNewSender(TSWTransactionTypes.Replace);
				ocrSender.SendMessage();
				AssertEquals(NZCMessage.MessageTypes.OutwardReport.MessageSubTypes.Replacement, consol.Messages[0].EM_MessageSubType);
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

				using (NZCustomsDataRegistry.Instance.ExportOrnTestMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertTestMode(false);
				}
			}
		}

		public void TestStatusTransactionScopeRollback()
		{
			consol.JK_UniqueConsignRef = "C002003289";
			Factory.Save();

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "40237571L"))
			{
				Factory.Saving += (factory) => throw new ApplicationException("Test");

				var ocrSender = GetNewSender(TSWTransactionTypes.Original);
				AssertExceptionThrown<CargoWise.Common.RethrownByExceptionHandlerException>(() => ocrSender.SendMessage());

				CombineAssertions("Rollback on failure", () =>
				{
					AssertEquals(OutwardReportStatusList.Descriptions.NotSent, manifestStatus.E2_MessageStatus);
					AssertEquals("Messages.Count", 0, consol.Messages.Count);
					AssertEquals("Unsaved Logs", false, consol.Logs.GetAllLogs().Any(x => !x.IsInDatabase));
				});
			}
		}

		void AssertTestMode(bool isTestMode)
		{
			consol = Factory.New<ForwardingConsol>();
			var ocrSender = GetNewSender(TSWTransactionTypes.Original);
			ocrSender.SendMessage();
			AssertEquals(isTestMode, consol.Messages[0].EM_IsTestMessage);
		}

		protected override SendTSW GetNewSender(TSWTransactionTypes transactionType)
		{
			return new SendOCRFromConsol(consol, null, manifestStatus, transactionType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			manifestStatus = new OutwardReportManifestStatus(consol);
		}
		ForwardingConsol consol;
		OutwardReportManifestStatus manifestStatus;
	}

	public class SendOCRFromConsolForTest : SendOCRFromConsol
	{
		public SendOCRFromConsolForTest(ForwardingConsol consol, IAdditionalInformation additionalMessageInformation, OutwardReportManifestStatus manifestStatus, TSWTransactionTypes transactionType)
			: base(consol, additionalMessageInformation, manifestStatus, transactionType)
		{
		}

		public new List<string> errorList => base.errorList;
	}
}
