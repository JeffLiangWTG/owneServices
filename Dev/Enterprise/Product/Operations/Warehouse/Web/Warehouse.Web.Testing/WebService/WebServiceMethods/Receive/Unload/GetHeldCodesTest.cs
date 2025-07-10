using System.Linq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetHeldCodesTest : WhsSecureServiceTestCase
	{
		#region TestGetHeldCodes

		public void TestGetHeldCodes_Defaults()
		{
			var webService = GetNewWebService();
			var response = webService.GetHeldCodes();

			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.WhsInventoryHeldCodes);
			AssertEquals(5, response.WhsInventoryHeldCodes.Count);
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => string.IsNullOrEmpty(c.Code) && string.IsNullOrEmpty(c.Description) && string.IsNullOrEmpty(c.ClientCode)));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "HEL" && c.Description == "Held" && string.IsNullOrEmpty(c.ClientCode)));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "DAM" && c.Description == "Damaged" && string.IsNullOrEmpty(c.ClientCode)));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "LCC" && c.Description == "Lost in Cycle Count" && string.IsNullOrEmpty(c.ClientCode)));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "SHORT" && c.Description == "Short Picked" && string.IsNullOrEmpty(c.ClientCode)));

			var firstCode = response.WhsInventoryHeldCodes[0];
			AssertEquals("First Hold Code should be blank code.", "", firstCode.Code);
		}

		public void TestGetHeldCodes_UserDefined()
		{
			var heldCodeABC = Helper.CreateInventoryHeldCode("ABC", "ABC DESC");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.GetHeldCodes();

			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.WhsInventoryHeldCodes);
			AssertEquals(6, response.WhsInventoryHeldCodes.Count);
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => string.IsNullOrEmpty(c.Code) && string.IsNullOrEmpty(c.Description) && string.IsNullOrEmpty(c.ClientCode)));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "ABC" && c.Description == "ABC DESC" && string.IsNullOrEmpty(c.ClientCode)));
		}

		public void TestGetHeldCodes_SystemWideAndClientSpecific()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("AAA", "AAA for system");
			var heldCode2 = Helper.CreateInventoryHeldCode("BBB", "BBB for client 1", client1.PK);
			var heldCode3 = Helper.CreateInventoryHeldCode("CCC", "CCC for client 1", client1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("BBB2", "BBB for client 2", client2.PK);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.GetHeldCodes();

			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.WhsInventoryHeldCodes);
			AssertEquals(9, response.WhsInventoryHeldCodes.Count);
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "AAA" && c.Description == "AAA for system" && string.IsNullOrEmpty(c.ClientCode)));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "BBB" && c.Description == "BBB for client 1" && c.ClientCode == "C1"));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "BBB2" && c.Description == "BBB for client 2" && c.ClientCode == "C2"));
			AssertNotNull(response.WhsInventoryHeldCodes.SingleOrDefault(c => c.Code == "CCC" && c.Description == "CCC for client 1" && c.ClientCode == "C1"));
		}

		#endregion
	}
}
