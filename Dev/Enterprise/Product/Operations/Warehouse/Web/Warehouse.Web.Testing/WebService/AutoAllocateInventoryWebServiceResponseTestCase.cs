using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class AutoAllocateInventoryWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		protected override void TestPropertiesCore()
		{
			var response = new AutoAllocateInventoryWebServiceResponse();
			AssertEquals("", response.PalletID);
			AssertEquals("", response.Location);
			AssertEquals("", response.LocationUserFriendly);
			AssertEquals(Guid.Empty, response.LocationPK);
			AssertEquals(false, response.IsFixed);
			AssertEquals(false, response.IsVoidLocation);
			AssertEquals(false, response.IsDockDoorLocation);

			response.PalletID = "ABC";
			var newGuid = Guid.NewGuid();
			response.Location = "A-1";
			response.LocationUserFriendly = "A-1";
			response.LocationPK = newGuid;
			response.IsFixed = true;
			response.IsVoidLocation = true;
			response.IsDockDoorLocation = true;

			AssertEquals("ABC", response.PalletID);
			AssertEquals("A-1", response.Location);
			AssertEquals("A-1", response.LocationUserFriendly);
			AssertEquals(newGuid, response.LocationPK);
			AssertEquals(true, response.IsFixed);
			AssertEquals(true, response.IsVoidLocation);
			AssertEquals(true, response.IsDockDoorLocation);
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new AutoAllocateInventoryWebServiceResponse();
		}

		protected new AutoAllocateInventoryWebServiceResponse Response
		{
			get
			{
				return (AutoAllocateInventoryWebServiceResponse)base.Response;
			}
		}
	}
}
