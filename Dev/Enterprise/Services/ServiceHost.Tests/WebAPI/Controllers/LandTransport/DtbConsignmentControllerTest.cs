using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	class DtbConsignmentControllerTest : TestCaseWithFactory
	{
		public void TestClassHasGlowAttribute()
		{
			var attribute = typeof(DtbConsignmentController).GetCustomAttributes(typeof(GlowTicketAuthenticationAttribute)).FirstOrDefault();
			AssertNotNull(attribute);
		}

		#region GetDefaultValuesForConsignment

		public void TestGetDefaultValuesForConsignment()
		{
			var pickupOrg = Factory.New<OrgHeader>();
			pickupOrg.OH_Code = "PICORG";
			pickupOrg.OH_IsConsignor = true;
			pickupOrg.MiscServ.OM_EXDefaultIncoTerm = "IN1";
			pickupOrg.MiscServ.OM_RS_NKEXDefaultServiceLevel = "SL1";

			var deliveryOrg = Factory.New<OrgHeader>();
			deliveryOrg.OH_Code = "DLVORG";
			deliveryOrg.OH_IsConsignee = true;
			deliveryOrg.MiscServ.OM_IMDefaultINCOTerm = "IN2";
			deliveryOrg.MiscServ.OM_RS_NKIMDefaultServiceLevel = "SL2";

			Factory.Save();

			var actionResult = controller.GetDefaultValuesForConsignment(pickupOrg.MainAddress.PK.ToGuid(), deliveryOrg.MainAddress.PK.ToGuid(), Core.Constants.ContainerModes.FTL);
			var response = actionResult.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
			var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			Assert(response.IsSuccessStatusCode);
			AssertEquals("{\"incoTerm\":\"IN2\",\"serviceLevel\":\"SL2\"}", content);
		}

		public void TestGetDefaultValuesForConsignment_Route()
		{
			var attribute = typeof(DtbConsignmentController).GetMethod("GetDefaultValuesForConsignment").GetCustomAttributes(typeof(RouteAttribute)).FirstOrDefault();
			AssertEquals("api/LandTransport/GetDefaultValuesForConsignment/{pickupAddressPK}/{deliveryAddressPK}/{jobType}", ((RouteAttribute)attribute).Template);
		}

		public void TestGetDefaultValuesForConsignment_ShouldHaveHttpGetAttribute()
		{
			var attribute = typeof(DtbConsignmentController).GetMethod("GetDefaultValuesForConsignment").GetCustomAttributes(typeof(HttpGetAttribute)).FirstOrDefault();
			AssertNotNull(attribute);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			controller = new DtbConsignmentController();
			controller.ControllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage())
			{
				Controller = controller
			};
		}

		DtbConsignmentController controller;

		#endregion
	}
}
