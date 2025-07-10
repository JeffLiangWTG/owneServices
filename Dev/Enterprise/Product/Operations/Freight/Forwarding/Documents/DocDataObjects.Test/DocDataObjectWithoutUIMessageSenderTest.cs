using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.Testing
{
	abstract class DocDataObjectWithoutUIMessageSenderTest : TestCaseWithFactory
	{
		[TestDate(2021, 1, 1)]
		public void TestSendMessage()
		{
			var bizObj = CreateBusinessObject();
			var notificationsHandler = new NotificationsHandler();

			using (Factory.AddDisposableService())
			{
				var res = MessageSender.SendMessage(bizObj, notificationsHandler);

				var notifications = notificationsHandler
					.Notifications
					.Select(n => n.Message);

				AssertMultilineASCIIEquals("no errors messages",
					string.Empty,
					string.Join("\r\n", notifications));

				Assert("message has been sent", res);

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, DataStoreName);
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

				var msn = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", MSNReference, msn.SL_Reference);

				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);

				AssertMultilineASCIIEquals("UXml", string.Format(ExpectedMessage, message.Message.Interchange.EI_SessionGUID, message.Message.Interchange.EI_InterchangeNum, message.Message.EM_MessageNum), message.Message.EM_MessageText);
			}
		}

		public void TestIsRegisteredWithObjectFactory()
		{
			const string registrationKey = "DocDataObjectWithoutUIMessageSendersProvider";

			var registration = ObjectFactory.Get<Hashtable>(registrationKey);
			AssertNotNull($"Expected {registrationKey} registered with ObjectFactory", registration);

			var senderRegistration = registration[SenderObjectFactoryRegistrationKey] as ObjectHandle;
			AssertNotNull($"Expected sender to be registered with ObjectFactory under {registrationKey} with the following key {SenderObjectFactoryRegistrationKey}", senderRegistration);

			var sender = senderRegistration.GetObject() as IDocDataObjectWithoutUIMessageSender;
			AssertNotNull("Sender has been registered in to use with ObjectFactory", sender);
			AssertEquals("Correct sender has been registered", sender.GetType(), MessageSender.GetType());
		}

		public void TestDataStoreNameMatchesExistingPivot()
		{
			if (!DocumentHasMenuItem)
			{
				Assert(true);
				return;
			}

			var query = new ZQuery(StmMenuTemplatePivotSchema.SI_DataStoreName, DataStoreName);
			var pivots = Factory.Load<StmMenuTemplatePivot>(query);

			Assert("At least 1 pivot exists with the DataStoreName", pivots.Length > 0);
		}

		#region Implementation

		protected abstract IDocDataObjectWithoutUIMessageSender MessageSender { get; }

		protected abstract BusinessObject CreateBusinessObject();

		protected abstract ZString DataStoreName { get; }

		public abstract bool DocumentHasMenuItem { get; }

		protected abstract ZString MSNReference { get; }

		protected abstract ZString ExpectedMessage { get; }

		protected abstract string SenderObjectFactoryRegistrationKey { get; }

		#endregion
	}
}
