using System;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class PutawayMultiplePalletsWebServiceResponseTest : WebServiceResponseTestCase
	{
		public void TestPalletInfos()
		{
			var response = new PutawayMultiplePalletsWebServiceResponse();
			AssertNull(response.PalletInfos);

			var testPK = Guid.NewGuid();
			var palletInfo = new PutawayPalletInfo("PLT", "D01", "D-01", "11", "Client", testPK);
			response.PalletInfos = new[] { palletInfo };
			AssertEquals("Count is correct", 1, response.PalletInfos.Length);
			AssertEquals(palletInfo, response.PalletInfos[0]);
			AssertEquals("PalletID is correct", "PLT", response.PalletInfos[0].PalletID);
			AssertEquals("Location is correct", "D01", response.PalletInfos[0].Location);
			AssertEquals("Location_UserFriendly is correct", "D-01", response.PalletInfos[0].Location_UserFriendly);
			AssertEquals("ClientCode is correct", "Client", response.PalletInfos[0].ClientCode);
			AssertEquals("DocketPK is correct", testPK, response.PalletInfos[0].DocketPK);
		}

		public void TestShowStockOnHandWarningOnPutaway()
		{
			var response = new PutawayMultiplePalletsWebServiceResponse();
			AssertEquals("ShowStockOnHandWarningOnPutaway defaults to false", false, response.ShowStockOnHandWarningOnPutaway);

			response.ShowStockOnHandWarningOnPutaway = true;
			AssertEquals("ShowStockOnHandWarningOnPutaway now true", true, response.ShowStockOnHandWarningOnPutaway);
		}

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new PutawayMultiplePalletsWebServiceResponse();
		}

		#endregion
	}
}
