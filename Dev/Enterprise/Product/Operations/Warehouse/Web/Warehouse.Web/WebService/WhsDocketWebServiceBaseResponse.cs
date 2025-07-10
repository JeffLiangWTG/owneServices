using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	// Properties tested in base classes
	public abstract class WhsDocketWebServiceBaseResponse : WebServiceResponse
	{
		public CodeDescriptionPairInfo[] HeldCodes { get; set; }
		public bool ShowStockOnHandWarningOnPutaway { get; set; }
		public bool CanDuplicatePreviousLine { get; set; }
		public bool WarehouseHasSingleDockDoorLocation { get; set; }
		public string SingleDockDoorLocation { get; set; }
		public string SingleDockDoorLocation_UserFriendly { get; set; }
		public Guid SingleDockDoorLocationPK { get; set; }
	}
}
