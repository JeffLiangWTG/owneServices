using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class LocationOrPalletIDWebServiceResponseTestCase : WhsLocationWebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();

			var response = new LocationOrPalletIDWebServiceResponse();
			AssertNull("PalletID", response.PalletID);
			AssertNull("InventoriesOnThePallet", response.InventoriesOnThePallet);
			AssertEquals("HasStockOnHandInTheLocationOutsideCurrentReceipt", false, response.WarnUserStockOnHandInTheLocationExist);

			var inventoriesCollection = new WhsInventoryLineInfoCollection();
			response.PalletID = "PLT-1";
			response.WarnUserStockOnHandInTheLocationExist = true;
			response.InventoriesOnThePallet = inventoriesCollection;
			AssertEquals("PalletID", "PLT-1", response.PalletID);
			AssertEquals("InventoriesOnThePallet", inventoriesCollection, response.InventoriesOnThePallet);
			AssertEquals("HasStockOnHandInTheLocationOutsideCurrentReceipt", true, response.WarnUserStockOnHandInTheLocationExist);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new LocationOrPalletIDWebServiceResponse();
		}

		protected new LocationOrPalletIDWebServiceResponse Response
		{
			get
			{
				return (LocationOrPalletIDWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
