using System;
using System.Web.Routing;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Views
{
	[TestClass]
	public class ServiceViewTests
	{
		#region Setup 

		[TestInitialize()]
		public void Startup()
		{
			MvcApplication.RegisterRoutes(RouteTable.Routes);
		}

		[TestCleanup()]
		public void Cleanup()
		{
			RouteTable.Routes.Clear();
		}

		#endregion

		[TestMethod]
		public void ServiceIndexView()
		{
			var serviceListView = new ServiceView();
			serviceListView.Client = new eHubClient() {CC_PK = Guid.NewGuid(), CC_ID = "TestClient"};
		}
	}
}
