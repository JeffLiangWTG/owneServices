namespace Enterprise.Warehouse.Web.WebService
{
	public class PalletIDWebServiceResponse : WebServiceResponse
	{
		public PalletIDWebServiceResponse()
			: base()
		{
			PalletID = "";
			UpdatedCountToBuildFrom = 0;
		}

		public string PalletID { get; set; }
		public int UpdatedCountToBuildFrom { get; set; }
	}
}
