using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CIN750WithoutUIMessageSenderTest : TestCaseWithFactory
	{
		public void TestIsRegisteredWithObjectFactory()
		{
			var senderRegistrationKey = "CIN750WithoutUIMessageSender";
			var sender = ObjectFactory.Get(senderRegistrationKey);
			AssertNotNull($"Expected {senderRegistrationKey} registered with ObjectFactory", sender);
		}

		[TestDate(2021, 1, 1)]
		public void TestSendMessage_InMessage() => TestSendMessage_Succeed_Core(CIN750NotificationMessageTypes.CIN750InNotification);

		[TestDate(2021, 1, 1)]
		public void TestSendMessage_CorMessage() => TestSendMessage_Succeed_Core(CIN750NotificationMessageTypes.CIN750CorNotification);

		[TestDate(2021, 1, 1)]
		public void TestSendMessage_ConsMessage() => TestSendMessage_Succeed_Core(CIN750NotificationMessageTypes.CIN750ConsNotification);

		void TestSendMessage_Succeed_Core(CIN750NotificationMessageTypes notificationType)
		{
			using (Factory.AddDisposableService())
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				IEnumerable<ZString> msnReferences;
				var documentName = ZString.Empty;
				var expectedMessage = ZString.Empty;
				BusinessObject bizObj;

				switch (notificationType)
				{
					case CIN750NotificationMessageTypes.CIN750InNotification:
						msnReferences = Helper.InNotificationMSNReferences;
						documentName = TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750InFromRCN;
						expectedMessage = Helper.InNotificationExpectedMessage;
						bizObj = Helper.CreateBusinessObjectForInNotification();
						break;
					case CIN750NotificationMessageTypes.CIN750CorNotification:
						msnReferences = Helper.CorNotificationMSNReferences;
						documentName = TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750CorFromRCN;
						expectedMessage = Helper.CorNotificationExpectedMessage;
						bizObj = Helper.CreateBusinessObjectForCorNotification();
						break;
					case CIN750NotificationMessageTypes.CIN750DeconsNotification:
						msnReferences = Helper.DeconsNotificationMSNReferences;
						documentName = TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750DeconsFromDCN;
						expectedMessage = Helper.DeconsNotificationExpectedMessage;
						bizObj = Helper.CreateBusinessObjectForDeconsNotification();
						break;
					case CIN750NotificationMessageTypes.CIN750ConsNotification:
						msnReferences = Helper.ConsNotificationMSNReferences;
						documentName = TransitDocDataConstants.DispatchConsignmentDataStoreNames.CIN750ConsFromDCN;
						expectedMessage = Helper.ConsNotificationExpectedMessage;
						bizObj = Helper.CreateBusinessObjectForConsNotification();
						break;
					default:
						throw new NotImplementedException();
				}

				Factory.Save();

				TestSendMessage_Succeed_Core(bizObj, documentName, msnReferences, expectedMessage);
			}
		}

		void TestSendMessage_Succeed_Core(BusinessObject bizObj, string documentName, IEnumerable<ZString> msnReferences, ZString expectedMessage)
		{
			var notifications = new NotificationsHandler();
			var sender = new CIN750WithoutUIMessageSender();
			var res = sender.SendMessage(bizObj, notifications);

			Assert("message has been sent", res);
			AssertEquals("no errors messages", 0, notifications.Notifications.Count);

			var documentDataStorageQuery = new ZQuery();
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, documentName);
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, bizObj.PK);

			var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

			AssertNotNull("document data has been created", documentDataStorage);

			var logs = (documentDataStorage as IStmALogParent)
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode
					|| l.SL_SE_NKEvent == Events.DataExportCode)
				.ToArray();

			var addOnValues = bizObj.GetAddOnValues();
			AssertEquals(addOnValues.Count, msnReferences.Count());

			var messageId = addOnValues.First().XV_Data.Substring(0, 36);
			var relatedLogValues = addOnValues.Select(a => a.XV_Data);
			foreach (var log in logs.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode))
			{
				AssertCollectionContains("AddOnValue Logs", $"{messageId}|{log.PK}", relatedLogValues);
			}

			Helper.AssertMSNReferenceCore(logs, msnReferences);

			if (!expectedMessage.IsEmpty)
			{
				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
				AssertMultilineASCIIEquals("UXml", string.Format(expectedMessage, message.Message.Interchange.EI_SessionGUID, message.Message.Interchange.EI_InterchangeNum, message.Message.EM_MessageNum, messageId), message.Message.EM_MessageText);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestSendMessage_InMessageHasBeenSent_SendCorMessage()
		{
			using (Factory.AddDisposableService())
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rcn = Helper.CreateBusinessObjectForInNotification();
				var rtu = Helper.CreateReceiveTransportationUnit("RTU2", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "V2");
				var packageState = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P6", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 3, weightUQ: "KG", receiveUnit: rtu);
				packageState.Package.KP_GoodsDescription = "Test Description";

				Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=6|PTP=REF|RFN=EDIDATRC0000001|WGT=11.800");

				Factory.Save();

				TestSendMessage_Succeed_Core(rcn, TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750CorFromRCN, new ZString[] { "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750CorNotification|OTY=-1|PTP=REF|RFN=EDIDATRC0000001|WGT=-3.000" }, ZString.Empty);
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestSendMessage_InMessageHasNotBeenSent_SendInMessage()
		{
			using (Factory.AddDisposableService())
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rcn = Helper.CreateBusinessObjectForInNotification();
				var rtu = Helper.CreateReceiveTransportationUnit("RTU2", rcn.Warehouse.PK, rcn.Warehouse.DefaultInboundDockDoorLocation.PK, vehicleRef: "V2");
				Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P6", TransitWarehouseStatuses.Codes.AdjustedOut, weight: 3, weightUQ: "KG", receiveUnit: rtu);
				Factory.Save();

				TestSendMessage_Succeed_Core(rcn, TransitDocDataConstants.ReceiveConsignmentDataStoreNames.CIN750InFromRCN, new ZString[] { "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800" }, ZString.Empty);
			}
		}

		void TestSendMessage_Failed_Core(BusinessObject bizObj, string expectedErrorMessage, bool expectInsertNote = true)
		{
			var notifications = new NotificationsHandler();
			var sender = new CIN750WithoutUIMessageSender();
			var res = sender.SendMessage(bizObj, notifications);

			AssertEquals(false, res);
			var errorMessages = notifications.Notifications.Select(n => n.Message);
			AssertEquals(1, errorMessages.Count());
			AssertCollectionContains(expectedErrorMessage, errorMessages);

			if (bizObj is IStmNoteParent && expectInsertNote)
			{
				var cinNote = (bizObj as IStmNoteParent).FindOrCreateCIN750StmNote();
				var cinNoteText = cinNote.ST_NoteText;
				AssertContains(expectedErrorMessage, cinNoteText);
			}
		}

		[TestDate(2023, 5, 1)]
		public void TestSendMessage_HasErrorMessage_In()
		{
			using (Factory.AddDisposableService())
			{
				using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var rcn = Helper.CreateBusinessObjectForInNotification();
					foreach (var packageState in rcn.PackageStates)
					{
						packageState.Package.KP_Weight = 0;
					}
					Factory.Save();

					var expectedErrorMessage = "Cannot Send CIN 750 In Notification because the reported Packages Weight (0.000) is equal to the Weight of Received Packages.";
					TestSendMessage_Failed_Core(rcn, expectedErrorMessage);
				}
			}
		}

		[TestDate(2023, 5, 1)]
		public void TestSendMessage_HasErrorMessage_MessageHasBeenSentCompletely()
		{
			using (Factory.AddDisposableService())
			{
				using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var rcn = Helper.CreateBusinessObjectForInNotification();
					Helper.CreateStmALog(rcn, "MSN", "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=5|PTP=REF|RFN=EDIDATRC0000001|WGT=8.800");
					Factory.Save();

					var expectedErrorMessage = "Cannot send CIN 750 Notification because the Packages on the Receive Consignment are reported completely.";
					TestSendMessage_Failed_Core(rcn, expectedErrorMessage, expectInsertNote: false);
				}
			}
		}

		[TestDate(2023, 5, 1)]
		public void TestSendMessage_HasErrorMessage_Decons()
		{
			using (Factory.AddDisposableService())
			{
				using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
					warehouse.WarehouseAddress.Address1 = "WH1Address";
					Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");
					var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);
					var location = Helper.CreateLocation(warehouse);
					var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);

					Helper.CreateAdditionalReference(rcn, "MAB-1", AdditionalReferenceTypes.Codes.MasterBill);
					Helper.CreateCustomsAdditionalReference(rcn, TransitWarehouseCustomsReferenceTypes.Codes.TempStorageDeclaration, "TST1");
					dcn.WDC_HouseBillNumber = "HSB1";

					var packageState = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, weight: 10);
					packageState.Package.KP_GoodsDescription = "DESC";

					Factory.Save();

					TestSendMessage_Failed_Core(dcn, "Cannot Send CIN 750 Deconsolidation Notification without Goods Detail.");
				}
			}
		}

		[TestDate(2024,6,1)]
		public void TestSendMessage_HasErrorMessage_Out()
		{
			using (Factory.AddDisposableService())
			{
				using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
					warehouse.WarehouseAddress.Address1 = "WH1Address";
					var location = Helper.CreateLocation(warehouse);
					Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, "C001");

					var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
					var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
					var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, location);
					var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
					var dcn = Helper.CreateDispatchConsignment("DC0000001", warehouse.PK);

					Helper.CreateStmALog(rcn, EventCodes.MessageSent, "|HBL=-|JOB=RC0000001|MBL=-|MST=CIN750InNotification|OTY=1|PTP=REF|RFN=EDIDATRC0000001|WGT=2");
					var packageState = Helper.CreatePackageState(rcn, 1, PackType.Freight.PKG, "P1", TransitWarehouseStatuses.Codes.Departed, weight: 2, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
					packageState.Package.KP_GoodsDescription = "DESC";

					Factory.Save();

					TestSendMessage_Failed_Core(dcn, "Customs Documents is required.");
				}
			}
		}

		protected CIN750DataObjectMessageSenderTestHelper Helper => helper ?? (helper = new CIN750DataObjectMessageSenderTestHelper(Factory));
		CIN750DataObjectMessageSenderTestHelper helper;
	}
}
