using System.Collections.Generic;
using Enterprise.Freight.Business;
using Enterprise.Security;

namespace Enterprise.Freight.GUI
{
	public interface IShipmentVsConsolGUIMessageHelper
	{
		void OnShipmentMasterChanged(IShipmentVsConsolMessageHelper messageHelper, CommonShipment shipment, CommonConsol parentConsol, MasterChangedEventArgs e);
		bool IsAllowedToDetachShipments(IShipmentVsConsolMessageHelper messageHelper, CommonConsol parentConsol, IEnumerable<CommonShipment> shipments);
		bool IsAllowedToDetachConsols(IShipmentVsConsolMessageHelper messageHelper, CommonShipment parentShipment, IEnumerable<CommonConsol> consols);

		bool RequestPermissionByImpersonation(string message, SecurityCheckpoint checkpoint);
	}
}
