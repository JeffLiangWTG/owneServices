using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	#region WhsOrderFlatFileDataImporter

	public class WhsOrderFlatFileDataImporter : WhsDocketFlatFileDataImporter<WhsOrder>
	{
		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new WhsOrderDataConverter(notifications, FactoryProvider.Current);
		}

		protected override WhsDocketValueObjectDataAdapter<WhsOrder> GetDataAdapter()
		{
			return new WhsOrderValueObjectDataAdapter();
		}

		protected override WhsDocketCollection GetNewDocketCollection()
		{
			return new WhsOrderCollection(FactoryProvider.Current, new AdhocCollectionRelationship(typeof(WhsOrder)));
		}
	}

	#endregion

	#region WhsOrderFlatFileDataImporterWithNotification

	/// <summary>
	/// Data importer with wrapper to handle email messaging on a per file basis
	/// </summary>
	public class WhsOrderFlatFileDataImporterWithNotification : WhsOrderFlatFileDataImporter
	{
		#region Constructors

		public WhsOrderFlatFileDataImporterWithNotification(INotifications notifications, GuidRegistryItem notificationGroup, string fileName)
		{
			NotificationGroup = notificationGroup;
			FileName = fileName;

			var notificationBuffer = notifications as NotificationBuffer;
			Notifications = notificationBuffer ?? new NotificationBuffer(notifications);
		}

		readonly GuidRegistryItem NotificationGroup;
		readonly NotificationBuffer Notifications;
		readonly string FileName;

		#endregion

		#region Import

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			var returnStatus = base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			SendEmailToNotificationGroup();

			return returnStatus;
		}

		#endregion

		#region SendEmailToNotificationGroup

		void SendEmailToNotificationGroup()
		{
			var isImportSuccessful = !Notifications.HasErrors;
			if (!isImportSuccessful || IsEmailNotificationSentOnSuccess)
			{
				try
				{
					var email = BuildEmailDef(isImportSuccessful);
					Env.OutgoingMailManager.CreateAndSave(email, NotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(NotificationGroup));
				}
				catch (EmailSendFailedException) { }
			}
		}

		EmailDef BuildEmailDef(bool isImportSuccessful)
		{
			var eMail = new EmailDef();

			var importStatusAsString = isImportSuccessful ? Res.GetString("e8c998d0-628a-49f3-9a3a-a4b525c0a0c7", "Succeeded") : Res.GetString("35903c9f-d7ea-47e9-a579-616db7b404e3", "Failed");

			eMail.Subject = Res.GetString("2ff73035-37d0-42f9-acc6-9d078d352a48", "Warehouse Order Import {0}", importStatusAsString);
			eMail.Body = Notifications.AsString;

			if (File.Exists(FileName))
			{
				eMail.Subject += " - " + Path.GetFileName(FileName);
				if (!isImportSuccessful)
				{
					eMail.Attachments.Add(new AttachmentDef(FileName));
				}
			}

			return eMail;
		}

		protected bool IsEmailNotificationSentOnSuccess => !SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.Value;

		#endregion
	}

	#endregion
}
