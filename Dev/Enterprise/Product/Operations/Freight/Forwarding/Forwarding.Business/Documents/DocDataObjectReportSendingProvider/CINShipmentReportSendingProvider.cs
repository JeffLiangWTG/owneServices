using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CINShipmentReportSendingProvider : DocDataObjectReportSendingProvider
	{
		public CINShipmentReportSendingProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string DataContext => "CINExportNotification";
		protected override ZGuid MenuItemPK => ShipmentSystemFormMenuItems.CINExportNotificationPK;
		public override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobShipment;
	}
}
