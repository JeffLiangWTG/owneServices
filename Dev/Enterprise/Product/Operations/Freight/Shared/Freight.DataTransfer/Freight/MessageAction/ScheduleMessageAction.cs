using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.DataTransfer
{
	class ScheduleMessageAction : ImportMessageAction
	{
		public ScheduleMessageAction(BusinessObjectFactoryProvider factoryProvider) : base(factoryProvider)
		{
			OnlySaveDataWhenNoRecordsHaveErrors = true;
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(
					NotificationDataRegistry.Instance.SchedulesImportNotificationGroup.GetFallBackValueAtAllLevels(
					GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("c2366e36-f599-437d-a82f-e2edde190712", "System->Registry->Notification->Schedules Import Notification Group"); }
		}

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new ScheduleValueObjectDataAdapter());
		}
	}
}
