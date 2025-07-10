namespace Enterprise.Tracking.Web.ServerServices
{
	public class WarehouseReceiveLineUpdateParameters : WarehouseDocketLineUpdateParameters
	{
		public string ExpectedQuantityControlID { get; set; }
		public string ExpiryDateControlID { get; set; }
	}
}
