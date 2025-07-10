using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	public class ISFMessageAction : ImportMessageAction
	{
		public ISFMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
			OnlySaveDataWhenNoRecordsHaveErrors = true;
		}

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new ImporterSecurityFilingXmlDataImporter(FactoryProvider);
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(
					ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.GetFallBackValueAtAllLevels(
					GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("5AB972A1-F1AF-4782-A255-97C6E9CD63C3", "System->Registry->Notification->ISF XML Import Notification Group"); }
		}

		protected override bool ImportData(IDataImporterControllingSave dataImporter, TextReader reader, INotifications notifications, Enterprise.Messaging.Business.EDIMessage message, out ITransactionParticipant[] forSave)
		{
			forSave = Array.Empty<ITransactionParticipant>();

			var importer = dataImporter as ImporterSecurityFilingXmlDataImporter;
			return importer != null && importer.ImportData(reader, ZString.Empty, notifications, message);
		}
	}
}
