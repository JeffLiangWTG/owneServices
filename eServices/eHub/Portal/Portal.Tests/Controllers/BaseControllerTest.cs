using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Tests.Fakes;
using CargoWise.eHub.Portal.Tests.Model;
using Moq;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	public class BaseControllerTest<T> where T : Portal.Controllers.ControllerBase, new()
	{
		protected TestRequest request;
		protected T controller;
		protected IeHubTransactionsContext context;
		protected IeHubTransactionsContext readonlyContext;

		public BaseControllerTest()
		{
			request = new TestRequest();
			var fakeIdentity = new GenericIdentity("CORP\\Test.Name");
			var principal = new GenericPrincipal(fakeIdentity, null);

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(x => x.Request).Returns(request);
			httpContext.SetupGet(x => x.User).Returns(principal);

			var routeData = new RouteData();
			controller = new T();
			context = TestContextWithData.Create();
			readonlyContext = TestContextWithData.Create();
			controller.Context = context;
			controller.ReadOnlyContext = readonlyContext;
			controller.ControllerContext = new ControllerContext(httpContext.Object, routeData, controller);
			FormCollection fakeForm = new FormCollection();
			fakeForm.Add("FakeName", "FakeValue");
			controller.ValueProvider = fakeForm.ToValueProvider();
		}
	}
}
