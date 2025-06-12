using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using Moq;

namespace CargoWise.eHub.Portal.Tests.Model
{
	public class ModelValidationHelper
	{
		public static CargoWise.eHub.Portal.Controllers.ControllerBase InitilizeStubController(IeHubTransactionsContext context)
		{
			var httpContext = new Mock<HttpContextBase>();
			var routeData = new RouteData();
			var controller = new CargoWise.eHub.Portal.Controllers.ControllerBase();
			controller.Context = context;

			FormCollection formValues = new FormCollection() 
			{
				{ "Test", "test" },
				{ "FirstName", "TestName" } 
			};

			controller.ValueProvider = formValues.ToValueProvider();

			var controllerContext = new Mock<ControllerContext>(httpContext.Object, routeData, controller);
			controller.ControllerContext = controllerContext.Object;
			return controller;
		}
	}
}