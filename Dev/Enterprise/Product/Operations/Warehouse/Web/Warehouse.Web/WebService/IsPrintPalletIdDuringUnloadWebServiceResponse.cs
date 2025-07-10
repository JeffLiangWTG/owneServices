namespace Enterprise.Warehouse.Web.WebService
{
	public class IsPrintPalletIdDuringUnloadWebServiceResponse : WebServiceResponse
	{
		public IsPrintPalletIdDuringUnloadWebServiceResponse()
			: base()
		{
			IsPrintPalletIDDuringUnload = false;
		}

		public bool IsPrintPalletIDDuringUnload { get; set; }
	}
}
