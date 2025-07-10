using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsDocketMessageAction : ImportMessageAction
	{
		public WhsDocketMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new WhsDocketValueObjectDataUniversalAdapter());
		}

		protected override IGlbGroup NotificationGroup
		{
			get { return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.WarehouseImportNotificationGroup.Value)); }
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("40E5AED1-2FE7-41B4-AE88-9D160E521DE2", "System->Registry->Notification->Warehouse Import Notification Group"); }
		}
	}
}
