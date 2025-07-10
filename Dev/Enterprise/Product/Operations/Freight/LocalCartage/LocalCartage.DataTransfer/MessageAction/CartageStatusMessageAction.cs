using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.LocalCartage.DataTransfer
{
	public class CartageStatusMessageAction : ImportMessageAction
	{
		public CartageStatusMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new CommonCartageStatusValueObjectDataAdapter());
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.LocalCartageNotificationGroup.Value));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("cb0d3d68-0b37-4a8c-aa10-d1a8b23b515e", "System->Registry->Notification->Port Transport Import Notification Group"); }
		}
	}
}
