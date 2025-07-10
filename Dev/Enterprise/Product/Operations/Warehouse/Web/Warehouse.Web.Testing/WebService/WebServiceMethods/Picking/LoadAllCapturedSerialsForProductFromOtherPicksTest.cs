using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class LoadAllCapturedSerialsForProductFromOtherPicksTest : WhsSecureServiceTestCase
	{
		public void TestLoadAllCapturedSerialsForProductFromOtherPicks()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 2m);
			var orderLine4 = Helper.CreateWhsOrderLine(order3, data.Part2, 2m);

			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3);
			Helper.Factory.Save();

			orderLine1.ReleaseLines[0].SerialNumber = "S1";
			orderLine1.ReleaseLines[0].Quantity = 1m;
			orderLine1.ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);

			orderLine2.ReleaseLines[0].SerialNumber = "S3";
			orderLine2.ReleaseLines[0].Quantity = 1m;
			orderLine2.ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);

			orderLine3.ReleaseLines[0].SerialNumber = "S5";
			orderLine3.ReleaseLines[0].Quantity = 1m;
			orderLine3.ReleaseLines.AddNew("", "", "", "S6", ZDate.Empty, ZDate.Empty, 1m);
			orderLine4.ReleaseLines.AddNew("", "", "", "S7", ZDate.Empty, ZDate.Empty, 1m);
			orderLine4.ReleaseLines.AddNew("", "", "", "S8", ZDate.Empty, ZDate.Empty, 1m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var results1 = webService1.LoadAllCapturedSerialsForProductFromOtherPicks(data.Org1.PK.ToGuid(), pick1.PK.ToGuid(), data.Part1.PK.ToGuid());
			AssertEquals("Web service call is successful.", ErrorTypes.None, results1.Error);
			Assert("Web service call is successful.", string.IsNullOrEmpty(results1.ErrorMessage));
			AssertContainsExactElementsInAnyOrder(new[] { "S5", "S6" }, results1.ReleaseCapturedSerials);

			var webService2 = GetNewWebService(data.Whs1);
			var results2 = webService2.LoadAllCapturedSerialsForProductFromOtherPicks(data.Org1.PK.ToGuid(), pick1.PK.ToGuid(), data.Part2.PK.ToGuid());
			AssertEquals("Web service call is successful.", ErrorTypes.None, results2.Error);
			Assert("Web service call is successful.", string.IsNullOrEmpty(results2.ErrorMessage));
			AssertContainsExactElementsInAnyOrder(new[] { "S7", "S8" }, results2.ReleaseCapturedSerials);

			var webService3 = GetNewWebService(data.Whs1);
			var results3 = webService3.LoadAllCapturedSerialsForProductFromOtherPicks(data.Org1.PK.ToGuid(), pick2.PK.ToGuid(), data.Part1.PK.ToGuid());
			AssertEquals("Web service call is successful.", ErrorTypes.None, results3.Error);
			Assert("Web service call is successful.", string.IsNullOrEmpty(results3.ErrorMessage));
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2", "S3", "S4" }, results3.ReleaseCapturedSerials);

			var webService4 = GetNewWebService(data.Whs1);
			var results4 = webService4.LoadAllCapturedSerialsForProductFromOtherPicks(data.Org1.PK.ToGuid(), pick2.PK.ToGuid(), data.Part2.PK.ToGuid());
			AssertEquals("Web service call is successful.", ErrorTypes.None, results4.Error);
			Assert("Web service call is successful.", string.IsNullOrEmpty(results4.ErrorMessage));
			AssertEquals("Nothing is returned.", false, results4.ReleaseCapturedSerials.Any());
		}

		public void TestLoadAllCapturedSerialsForProductFromOtherPicks_DoesNotIncludeFromFinalisedPicks()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 2m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			Helper.Factory.Save();

			orderLine1.ReleaseLines[0].SerialNumber = "S1";
			orderLine1.ReleaseLines[0].Quantity = 1m;
			orderLine1.ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);

			orderLine2.ReleaseLines[0].SerialNumber = "S3";
			orderLine2.ReleaseLines[0].Quantity = 1m;
			orderLine2.ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);

			orderLine3.ReleaseLines[0].SerialNumber = "S5";
			orderLine3.ReleaseLines[0].Quantity = 1m;
			orderLine3.ReleaseLines.AddNew("", "", "", "S6", ZDate.Empty, ZDate.Empty, 1m);
			Helper.Factory.Save();

			pick2.FinaliseAllOrders();
			pick2.FinalisePick();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(pick2);
			AssertIsFinalisedPrecondition(order2);

			var webService1 = GetNewWebService(data.Whs1);
			var results1 = webService1.LoadAllCapturedSerialsForProductFromOtherPicks(data.Org1.PK.ToGuid(), pick1.PK.ToGuid(), data.Part1.PK.ToGuid());
			AssertEquals("Web service call is successful.", ErrorTypes.None, results1.Error);
			Assert("Web service call is successful.", string.IsNullOrEmpty(results1.ErrorMessage));
			AssertContainsExactElementsInAnyOrder(new[] { "S5", "S6" }, results1.ReleaseCapturedSerials);

			var webService2 = GetNewWebService(data.Whs1);
			var results2 = webService2.LoadAllCapturedSerialsForProductFromOtherPicks(data.Org1.PK.ToGuid(), pick3.PK.ToGuid(), data.Part1.PK.ToGuid());
			AssertEquals("Web service call is successful.", ErrorTypes.None, results2.Error);
			Assert("Web service call is successful.", string.IsNullOrEmpty(results2.ErrorMessage));
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2" }, results2.ReleaseCapturedSerials);
		}

		public void TestLoadAllCapturedSerialsForProductFromOtherPicks_DifferentClient()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var org2 = Helper.CreateClient("ORG2");

			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.SetClientAttributeType(org2, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(org2, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "O1");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 2m);

			var pick1 = Helper.CreatePickNew(order1);
			Helper.CreatePickNew(order2);
			Helper.Factory.Save();

			orderLine1.ReleaseLines[0].SerialNumber = "S1";
			orderLine1.ReleaseLines[0].Quantity = 1m;
			orderLine1.ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);

			orderLine2.ReleaseLines[0].SerialNumber = "S3";
			orderLine2.ReleaseLines[0].Quantity = 1m;
			orderLine2.ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var results = webService.LoadAllCapturedSerialsForProductFromOtherPicks(data.Org1.PK.ToGuid(), pick1.PK.ToGuid(), data.Part1.PK.ToGuid());
			AssertEquals("Web service call is successful.", ErrorTypes.None, results.Error);
			Assert("Web service call is successful.", string.IsNullOrEmpty(results.ErrorMessage));
			AssertEquals("Nothing is returned.", false, results.ReleaseCapturedSerials.Any());
		}
	}
}
