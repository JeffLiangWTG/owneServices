using System.Web.Routing;
using MvcRouteTester;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WebService.Tests
{
	[TestFixture]
	public class RouteConfigTests
	{
		private RouteCollection routes;

		[SetUp]
		public void SetUp_RegisterRoutes()
		{
			routes = new RouteCollection();
			RouteConfig.RegisterRoutes(routes);
		}

		[Test]
		public void TestRouting()
		{
			RouteAssert.HasRoute(routes, "/authentication/CheckSystemIDExistence?systemid=abc", new { controller = "authentication", action = "CheckSystemIDExistence", SystemId="abc" });
			RouteAssert.HasRoute(routes, "/authentication/ValidateSystemIDAndPassword?systemid=123&password=abc", new { controller = "authentication", action = "ValidateSystemIDAndPassword", SystemId = "123", Password = "abc" });
			RouteAssert.HasRoute(routes, "/authentication/CheckCodeExistence?enterprisecode=123&servercode=abc", new { controller = "authentication", action = "CheckCodeExistence", EnterpriseCode = "123", ServerCode = "abc"});
			RouteAssert.HasRoute(routes, "/authentication/ValidateCodeAndPassword?enterprisecode=123&servercode=abc&password=111", new { controller = "authentication", action = "ValidateCodeAndPassword", EnterpriseCode = "123", ServerCode = "abc", Password = "111"});
		}
	}
}
