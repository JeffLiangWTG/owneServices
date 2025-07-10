using CargoWise.Types;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemPackageStateDTO
	{
		public bool IsNew { get; set; }
		public bool WPS_IsHandlingUnit { get; set; }
		public string WPS_UnitType { get; set; }
		public string WPS_Status { get; set; }
		public ZGuid WPS_WW_Warehouse { get; set; }
		public ZGuid WPS_KP_Package { get; set; }
		public ZGuid? KP_KP_ParentPackage { get; set; }
		public ZGuid? WPS_WRC_TransitReceiveConsignment { get; set; }
		public ZGuid? WPS_WDC_TransitDispatchConsignment { get; set; }
		public bool? WPS_IsHighRisk { get; set; }
		public ZString KP_ExternalReference { get; set; }
	}
}
