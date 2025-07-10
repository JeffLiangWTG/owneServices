namespace Enterprise.Tracking.Web.ServerServices
{
	public class WarehouseOrderLineUpdateParameters : WarehouseDocketLineUpdateParameters
	{
		public string ShortfallControlID { get; set; }
	}
}
