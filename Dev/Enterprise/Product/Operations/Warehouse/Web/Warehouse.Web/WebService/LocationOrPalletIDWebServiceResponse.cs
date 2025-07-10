using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class LocationOrPalletIDWebServiceResponse : WhsLocationWebServiceResponse
	{
		#region Constructors

		public LocationOrPalletIDWebServiceResponse()
			: base()
		{
		}

		#endregion

		#region Properties

		public string PalletID { get; set; }

		public WhsInventoryLineInfoCollection InventoriesOnThePallet { get; set; }

		public bool WarnUserStockOnHandInTheLocationExist { get; set; }

		#endregion
	}
}
