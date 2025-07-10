using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CCTShipmentReportSendingProvider : DocDataObjectReportSendingProvider
	{
		public CCTShipmentReportSendingProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string DataContext => "CargoControlAndTransit";

		protected override ZGuid MenuItemPK => ShipmentSystemFormMenuItems.DocumentMenuCCTShipmentReport;

		public override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobShipment;
	}
}
