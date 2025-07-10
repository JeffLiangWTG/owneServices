using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseOrderLineUpdateWSMethodTest : WarehouseDocketLineUpdateWSMethodTest<WarehouseOrderLineUpdateWSMethod>
	{
		protected override void AddExpectedTokensForQuantityChange(WebServiceResponse expectedResponse)
		{
			base.AddExpectedTokensForQuantityChange(expectedResponse);

			var updateValueToken = new UpdateValueResponseToken("Shortfall", "2");
			updateValueToken.Conditions.Add(new ResponseConditionEqualToken("Shortfall", 1m));
			expectedResponse.Add(updateValueToken);
		}

		protected override void AddExpectedTokensForPacksChange(WebServiceResponse expectedResponse)
		{
			base.AddExpectedTokensForPacksChange(expectedResponse);

			var updateValueToken = new UpdateValueResponseToken("Shortfall", "2");
			updateValueToken.Conditions.Add(new ResponseConditionEqualToken("Shortfall", 1m));
			expectedResponse.Add(updateValueToken);
		}

		protected override WarehouseDocketLineUpdateParameters GetLineUpdateParameters()
		{
			var parameters = (WarehouseOrderLineUpdateParameters)base.GetLineUpdateParameters();
			parameters.ShortfallControlID = "Shortfall";

			return parameters;
		}

		protected override WarehouseDocketLineUpdateParameters GetNewLineUpdateParameters() => new WarehouseOrderLineUpdateParameters();

		protected override BusinessObject GetTrackingDocketForTesting() => TrackingWhsOrder.GetTrackingWhsOrder(WarehouseHelper.CreateWhsOrder(TestHelper.TestOrg, Warehouse));

		protected override WhsDocketLine CreateNewDocketLines(BusinessObject trackingDocket, out string lineRef)
		{
			var receive = (TrackingWhsOrder)trackingDocket;
			receive.Lines.AddNew();
			var line = receive.Lines.AddNew();
			receive.Lines.AddNew();
			lineRef = ((BusinessObject)line).PK.ToString();

			return line.WhsOrderLine;
		}

		protected override string GetExpectedMethodSpecificServiceScriptFileName() => "WarehouseOrderLineUpdateWSMethod.js";

		protected override string GetExpectedMethodName() => "WarehouseOrderLineUpdate";

		protected override WarehouseOrderLineUpdateWSMethod GetNewWebServiceMethod() => new WarehouseOrderLineUpdateWSMethod();
	}
}
