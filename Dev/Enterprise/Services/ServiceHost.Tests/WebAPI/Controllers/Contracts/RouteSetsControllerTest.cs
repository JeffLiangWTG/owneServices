using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.Contracts;
using static Enterprise.Services.ServiceHost.WebAPI.Controllers.Contracts.RouteSetsController;

namespace Enterprise.Services.ServiceHost.Tests
{
	class RouteSetsControllerTest : TestCaseWithFactory
	{
		public void TestReturns403_WhenNoConsolPKs()
		{
			const string noPKsMessage = "Must specify at least one consolidation PK";

			AssertGetRouteSetsForConsolsReturns403(null, noPKsMessage);
			AssertGetRouteSetsForConsolsReturns403(new List<string>(), noPKsMessage);
		}

		public void TestReturns403_WhenConsolPKsAreNotGuids()
		{
			const string invalidGuidMessage = "One or more of the provided consolidation PKs are not valid guids";

			AssertGetRouteSetsForConsolsReturns403(new List<string>() { null }, invalidGuidMessage);
			AssertGetRouteSetsForConsolsReturns403(new List<string>() { "123" }, invalidGuidMessage);
		}

		public void TestReturnsRouteSetsForValidConsolPKs()
		{
			var consolWithRouteSets = Factory.NewWithValidTestData<ForwardingConsol>();

			var routeSet1Leg1 = consolWithRouteSets.Transports[0];
			routeSet1Leg1.JW_RL_NKLoadPort = "AUSYD";
			routeSet1Leg1.JW_RL_NKDiscPort = "AUMEL";

			var routeSet1Leg2 = consolWithRouteSets.Transports.AddNew();
			routeSet1Leg2.JW_RL_NKLoadPort = "AUMEL";
			routeSet1Leg2.JW_RL_NKDiscPort = "AUPER";

			var routeSet2Leg1 = consolWithRouteSets.Transports.AddNew();
			routeSet2Leg1.JW_RL_NKLoadPort = "NZAKL";
			routeSet2Leg1.JW_RL_NKDiscPort = "NZWLG";

			Factory.Save();

			AssertEquals(1, routeSet1Leg1.RouteSetNumber);
			AssertEquals(1, routeSet1Leg2.RouteSetNumber);
			AssertEquals(2, routeSet2Leg1.RouteSetNumber);

			var consolWithoutRouteSets = Factory.NewWithValidTestData<ForwardingConsol>();

			var noRouteSetLeg1 = consolWithoutRouteSets.Transports[0];
			noRouteSetLeg1.JW_RL_NKLoadPort = "AUSYD";
			noRouteSetLeg1.JW_RL_NKDiscPort = "AUMEL";

			var noRouteSetLeg2 = consolWithoutRouteSets.Transports.AddNew();
			noRouteSetLeg2.JW_RL_NKLoadPort = "AUMEL";
			noRouteSetLeg2.JW_RL_NKDiscPort = "";

			Factory.Save();

			AssertEquals(0, noRouteSetLeg1.RouteSetNumber);
			AssertEquals(0, noRouteSetLeg2.RouteSetNumber);

			var routeSetsResult = controller.GetRouteSetsForConsols(new List<string>()
			{
				consolWithRouteSets.PK.ToString(),
				consolWithoutRouteSets.PK.ToString()
			}) as JsonResult<Dictionary<ZGuid, IEnumerable<IEnumerable<ZGuid>>>>;

			AssertNotNull(routeSetsResult);
			Assert("Result should have one entry per consol", routeSetsResult.Content.Count == 2);

			var expectedConsolWithRouteSetsResult = new[]
			{
				new[] { routeSet1Leg1.PK, routeSet1Leg2.PK },
				new[] { routeSet2Leg1.PK },
			};
			var consolWithRouteSetsResult = routeSetsResult.Content[consolWithRouteSets.PK];

			for (var i = 0; i < expectedConsolWithRouteSetsResult.Length; i++)
			{
				var expectedRouteSet = expectedConsolWithRouteSetsResult[i];
				var resultRouteSet = consolWithRouteSetsResult.ElementAt(i);
				AssertContainsExactElementsInExactOrder(expectedRouteSet, resultRouteSet);
			}

			var expectedConsolWithoutRouteSetsResult = new[]
			{
				new[] { noRouteSetLeg1.PK },
				new[] { noRouteSetLeg2.PK }
			};
			var consolWithoutRouteSetsResult = routeSetsResult.Content[consolWithoutRouteSets.PK];

			for (var i = 0; i < expectedConsolWithoutRouteSetsResult.Length; i++)
			{
				var expectedRouteSet = expectedConsolWithoutRouteSetsResult[i];
				var resultRouteSet = consolWithoutRouteSetsResult.ElementAt(i);
				AssertContainsExactElementsInExactOrder(expectedRouteSet, resultRouteSet);
			}
		}

		void AssertGetRouteSetsForConsolsReturns403(List<string> consolPKs, string expectedMessage)
		{
			var result = controller.GetRouteSetsForConsols(consolPKs) as BadRequestErrorMessageResult;
			AssertNotNull(result);
			AssertEquals(expectedMessage, result.Message);
		}

		public void TestReturnsRouteSetsNumberForValidTransportPKs()
		{
			var consolWithRouteSets = Factory.NewWithValidTestData<ForwardingConsol>();

			var routeSet1Leg1 = consolWithRouteSets.Transports[0];
			routeSet1Leg1.JW_RL_NKLoadPort = "AUSYD";
			routeSet1Leg1.JW_RL_NKDiscPort = "AUMEL";

			var routeSet1Leg2 = consolWithRouteSets.Transports.AddNew();
			routeSet1Leg2.JW_RL_NKLoadPort = "AUMEL";
			routeSet1Leg2.JW_RL_NKDiscPort = "AUPER";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var shipmentLeg1 = shipment.Transports.AddNew();
			shipmentLeg1.JW_RL_NKLoadPort = "AUSYD";
			shipmentLeg1.JW_RL_NKDiscPort = "AUMEL";

			Factory.Save();

			var routeSetsNumberResult = controller.GetRouteSetNumberForConsolTransportLeg(routeSet1Leg1.PK.ToString()) as NegotiatedContentResult<RouteSetNumberJsonResponse>;
			AssertNotNull(routeSetsNumberResult);
			var routeSetNumber = routeSetsNumberResult.Content.routeSetNumber;
			AssertEquals(1, routeSetNumber);

			var routeSetsShipmentResult = controller.GetRouteSetNumberForConsolTransportLeg(shipmentLeg1.PK.ToString()) as BadRequestErrorMessageResult;
			AssertEquals($"Consol Transport leg is not found.", routeSetsShipmentResult.Message);

			var routeSetsNumberInvalidResult = controller.GetRouteSetNumberForConsolTransportLeg("1234") as BadRequestErrorMessageResult;
			AssertEquals($"The provided transport PK is not valid guid.", routeSetsNumberInvalidResult.Message);

			routeSetsNumberInvalidResult = controller.GetRouteSetNumberForConsolTransportLeg(null) as BadRequestErrorMessageResult;
			AssertEquals($"Must specify transport PK.", routeSetsNumberInvalidResult.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			controller = new RouteSetsController();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
		}

		RouteSetsController controller;
	}
}
