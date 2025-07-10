using System.Collections.Generic;

namespace Enterprise.Freight.Business
{
	public interface IShipmentVsConsolMessageHelper
	{
		bool IsAllowedToAddNewShipment(out string message, CommonConsol parentConsol);
		IShipmentConsolAttachRequest IsAllowedToAttachShipment(CommonConsol parentConsol, CommonShipment shipment);
		IShipmentConsolDetachRequest IsAllowedToDetachShipments(CommonConsol parentConsol, IEnumerable<CommonShipment> shipments);

		bool IsAllowedToAddNewConsol(out string message, CommonShipment parentShipment);
		IShipmentConsolAttachRequest IsAllowedToAttachConsol(CommonShipment parentShipment, CommonConsol consol);
		IShipmentConsolDetachRequest IsAllowedToDetachConsols(CommonShipment parentShipment, IEnumerable<CommonConsol> consols);

		bool IsAllowedToAddNewSubShipment(out string message, CommonShipment master);
		bool IsAllowedToAttachSubShipment(out string message, CommonShipment master, CommonShipment sub = null);

		CommonConsol[] GetConsolsToDetachFromSubShipments(out string message, CommonConsol parentConsol, CommonShipment oldMaster, CommonShipment newMaster, IEnumerable<CommonShipment> subShipments);
		void DetachShipmentsFromConsols(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols);

		string CheckRelatedReceivingAgents(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols);
		string CheckRelatedSendingAgents(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols);

		IReadOnlyCollection<string> CheckDatesWithinRange(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols);

		IReadOnlyCollection<string> CheckShipmentEstimatedDeliveryIsAfterConsolArrival(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols);

		string CheckShipmentAndConsolComplianceRisk(IEnumerable<CommonShipment> shipments, IEnumerable<CommonConsol> consols);

		bool IsGatewayServiceLevelCheckSuspended { get; set; }
	}
}
