using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	abstract class TransitDocDataObjectMessageSenderTest : TestCaseWithFactory
	{
		public virtual void TestSendMessageTwiceWithSamePackLineProperties()
		{
			using (Factory.AddDisposableService())
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bizObj = SetBusinessObject();
				Factory.Save();
				var notifications = new NotificationsHandler();

				var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

				Assert("message has been sent", res);
				AssertEquals("no errors messages", 0, notifications.Notifications.Count);

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, DocumentName);
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, bizObj.PK);
				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

				var documentDataStorageLogs = (documentDataStorage as IStmALogParent).Logs.GetAllLogs().Cast<StmALog>();
				var bizObjLogs = (bizObj as IStmALogParent).Logs.GetAllLogs().Cast<StmALog>();

				Helper.AssertMSNReferenceCore(documentDataStorageLogs, MSNReferences);
				Helper.AssertMSNReferenceCore(bizObjLogs, MSNReferences);

				UpdateBusinessObjectForReSending(bizObj);
				Factory.Save();

				res = MessageSender.SendMessage(bizObj, MenuItem, notifications);
				Assert("message has been sent again", res);
				AssertEquals("no errors messages", 0, notifications.Notifications.Count);

				documentDataStorageLogs = (documentDataStorage as IStmALogParent).Logs.GetAllLogs().Cast<StmALog>();
				bizObjLogs = (bizObj as IStmALogParent).Logs.GetAllLogs().Cast<StmALog>();

				Helper.AssertMSNReferenceCore(documentDataStorageLogs, MSNReferences, 2);
				Helper.AssertMSNReferenceCore(bizObjLogs, MSNReferences, 2);
			}
		}

		[TestDate(2021, 1, 1)]
		public virtual void TestSendMessage()
		{
			using (Factory.AddDisposableService())
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bizObj = SetBusinessObject();
				Factory.Save();
				var notifications = new NotificationsHandler();

				var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

				Assert("message has been sent", res);
				AssertEquals("no errors messages", 0, notifications.Notifications.Count);

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, DocumentName);
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
				AssertEquals(addOnValues.Count, MSNReferences.Count());

				var messageId = addOnValues.First().XV_Data.Substring(0, 36);
				var relatedLogValues = addOnValues.Select(a => a.XV_Data);
				foreach (var log in logs.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode))
				{
					AssertCollectionContains("AddOnValue Logs", $"{messageId}|{log.PK}", relatedLogValues);
				}

				Helper.AssertMSNReferenceCore(logs, MSNReferences);

				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
				AssertMultilineASCIIEquals("UXml", string.Format(ExpectedMessage, message.Message.Interchange.EI_SessionGUID, message.Message.Interchange.EI_InterchangeNum, message.Message.EM_MessageNum, messageId), message.Message.EM_MessageText);
			}
		}

		#region Implementation

		protected abstract IDocDataObjectMessageSender MessageSender { get; }

		protected abstract BusinessObject SetBusinessObject();

		protected abstract void UpdateBusinessObjectForReSending(BusinessObject bizObj);

		protected abstract ZGuid MenuItemPK { get; }

		protected IStmMenuItem MenuItem => menuItem ?? (menuItem = Factory.Load<VisualizerMenuItem>(MenuItemPK));
		IStmMenuItem menuItem;

		protected abstract ZString DocumentName { get; }

		protected abstract IEnumerable<ZString> MSNReferences { get; }

		protected abstract ZString ExpectedMessage { get; }

		protected void TestValidationCore(Func<BusinessObject> setUp, string expectedMessage)
		{
			using (Factory.AddDisposableService())
			using (WarehouseDataRegistry.Instance.EnableSendingCIN750Message.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bizObj = setUp();
				Factory.Save();
				var notifications = new NotificationsHandler();

				var result = MessageSender.SendMessage(bizObj, MenuItem, notifications);

				Assert("Can not send message", !result);
				Assert("Should have error messages", notifications.Notifications.Any());
				AssertCollectionContains("Error message", expectedMessage, notifications.Notifications.Select(n => n.Message));
			}
		}

		protected CIN750DataObjectMessageSenderTestHelper Helper => helper ?? (helper = new CIN750DataObjectMessageSenderTestHelper(Factory));
		CIN750DataObjectMessageSenderTestHelper helper;

		#endregion
	}
}
