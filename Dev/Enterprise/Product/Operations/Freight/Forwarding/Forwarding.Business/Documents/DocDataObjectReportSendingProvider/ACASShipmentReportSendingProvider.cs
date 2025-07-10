using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ACASShipmentReportSendingProvider : DocDataObjectReportSendingProvider
	{
		public ACASShipmentReportSendingProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string DataContext => "AirCargoAdvanceScreening";

		protected override ZGuid MenuItemPK => ShipmentSystemFormMenuItems.DocumentMenuACASShipmentReport;

		public override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobShipment;
	}
}
