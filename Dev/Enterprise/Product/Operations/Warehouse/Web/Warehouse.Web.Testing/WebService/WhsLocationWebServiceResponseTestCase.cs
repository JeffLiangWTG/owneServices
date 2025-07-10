using System;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsLocationWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			var response = new WhsLocationWebServiceResponse();
			AssertEquals("", response.Location);
			AssertEquals("", response.LocationUserFriendly);
			AssertEquals("", response.LocationFormattedCheckDigit);
			AssertEquals(Guid.Empty, response.LocationPK);
			AssertEquals(false, response.IsFixed);
			AssertEquals(false, response.IsVoidLocation);
			AssertEquals(false, response.IsDockDoorLocation);
			AssertEquals(false, response.IsPackingStation);
			AssertEquals(false, response.IsPackingConsolidation);

			var newGuid = Guid.NewGuid();
			response.Location = "A-1";
			response.LocationUserFriendly = "A-1";
			response.LocationFormattedCheckDigit = "11";
			response.LocationPK = newGuid;
			response.IsFixed = true;
			response.IsVoidLocation = true;
			response.IsDockDoorLocation = true;
			response.IsPackingStation = true;
			response.IsPackingConsolidation = true;
			AssertEquals("A-1", response.Location);
			AssertEquals("A-1", response.LocationUserFriendly);
			AssertEquals("11", response.LocationFormattedCheckDigit);
			AssertEquals(newGuid, response.LocationPK);
			AssertEquals(true, response.IsFixed);
			AssertEquals(true, response.IsVoidLocation);
			AssertEquals(true, response.IsDockDoorLocation);
			AssertEquals(true, response.IsPackingStation);
			AssertEquals(true, response.IsPackingConsolidation);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsLocationWebServiceResponse();
		}

		protected new WhsLocationWebServiceResponse Response
		{
			get
			{
				return (WhsLocationWebServiceResponse)base.Response;
			}
		}

		#endregion
	}
}
