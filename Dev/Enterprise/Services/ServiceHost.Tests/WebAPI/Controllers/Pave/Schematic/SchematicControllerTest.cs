using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Common;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Enterprise.Services.ServiceHost.Tests
{
	class SchematicControllerTest : PaveControllerTestCase<ISchematicService, SchematicController>
	{
		protected Mock<ISchematicService> ServiceMock;
		protected SchematicController Controller;
		readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
		};

		protected override void SetUp()
		{
			base.SetUp();
			serializerSettings.Converters.Add(new StringEnumConverter());
			ServiceMock = new Mock<ISchematicService>();
			var mockHttpRequestMessage = new Mock<HttpRequestMessage> { CallBase = true };
			var mocHttpConfiguration = new Mock<HttpConfiguration> { CallBase = true };
			var identity = GlowTicketTestHelper.CreateStaffIdentity((GlbStaff)Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			Controller = (SchematicController)Activator.CreateInstance(typeof(SchematicController));
			Controller.SetServiceForTest(ServiceMock.Object);
			Controller.ControllerContext = new HttpControllerContext(mocHttpConfiguration.Object, new HttpRouteData(new HttpRoute()), mockHttpRequestMessage.Object)
			{
				Controller = Controller
			};
			Controller.Request.RequestUri = new Uri("http://URI");
			Controller.User = new GenericPrincipal(identity, null);
		}

		public override void TestOnCreateController_ShouldUseDbConnectionCorrectly()
		{
			var lasErrorReported = string.Empty;
			SchematicController controller = null;

			Task.Run(() =>
			{
				ErrorReporter.Clear();
				lasErrorReported = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
				controller = new SchematicController();

				controller.Dispose();
			})
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult();

			AssertNullOrEmpty(lasErrorReported);
		}

		public void TestPostProcessTransferRules()
		{
			var dummyDTO = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			ServiceMock.Setup(m => m.ProcessTransferRules(dummyDTO, It.IsAny<ILogger>()));

			var statusCode = GetStatusCode(Controller.PostProcessTransferRules(dummyDTO));
			var result = GetResult(Controller.PostProcessTransferRules(dummyDTO));

			ServiceMock.Verify();

			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals(SerializeToPaveResponseJson(Enumerable.Empty<string>()), result);
		}
	}
}
