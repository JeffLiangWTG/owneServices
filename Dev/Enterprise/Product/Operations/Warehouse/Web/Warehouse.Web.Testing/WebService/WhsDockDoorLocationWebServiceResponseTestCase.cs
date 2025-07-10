namespace Enterprise.Warehouse.Web.WebService.Testing
{
	public class WhsDockDoorLocationWebServiceResponseTestCase : WebServiceResponseTestCase
	{
		#region TestProperties

		protected override void TestPropertiesCore()
		{
			base.TestPropertiesCore();

			var response = (WhsDockDoorLocationWebServiceResponse)GetNewResponse();
			AssertEquals("DockDoorLocation", true, string.IsNullOrEmpty(response.DockDoorLocation));

			response.DockDoorLocation = "DockDoor-1";
			AssertEquals("DockDoorLocation", "DockDoor-1", response.DockDoorLocation);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetNewResponse()
		{
			return new WhsDockDoorLocationWebServiceResponse();
		}

		#endregion
	}
}
