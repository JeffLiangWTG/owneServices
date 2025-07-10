using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoiceMessageAction : ImportMessageAction
	{
		public InvoiceMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new InvoiceXmlDataImporter(FactoryProvider, StandAloneInvoiceValueObjectDataAdapter.New());
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.CommercialInvoiceImportNotificationGroup.Value));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("990dc503-431b-4264-a3ed-119e189cff9c", "System->Registry->Notification->Commercial Invoice Import Notification Group"); }
		}
	}
}
