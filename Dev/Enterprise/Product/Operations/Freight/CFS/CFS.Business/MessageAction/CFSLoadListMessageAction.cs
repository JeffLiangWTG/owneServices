using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListMessageAction : ImportMessageAction
	{
		public CFSLoadListMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new LoadListValueObjectDataAdapter());
		}

		protected override IGlbGroup NotificationGroup
		{
			get { return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.AgencyBillOfLadingImportNotificationGroup.Value)); }
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("30080c51-9e5a-4e6a-ad1c-ac027e3182fc", "System->Registry->Notification->Agency Bill Of Lading Import Notification Group"); }
		}
	}
}
