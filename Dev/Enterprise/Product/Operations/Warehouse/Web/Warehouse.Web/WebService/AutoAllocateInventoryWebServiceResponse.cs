namespace Enterprise.Warehouse.Web.WebService
{
	public class AutoAllocateInventoryWebServiceResponse : WhsLocationWebServiceResponse
	{
		public AutoAllocateInventoryWebServiceResponse()
			: base()
		{
			PalletID = "";
		}

		public string PalletID { get; set; }
	}
}
