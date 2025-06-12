using System;
using System.Web.Mvc;
using System.Web.Routing;
using CargoWise.eHub.Portal.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcContrib.TestHelper;
using MvcContrib.UI.Grid;

namespace CargoWise.eHub.Portal.Tests.Routing
{
	[TestClass]
	public class RoutingTests
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
		public void RouteSiteHasDefaultConrtollerAndAction()
		{
			"~/".ShouldMapTo<HomeController>(controller => controller.Index());
		}

		#region Client Controller

		[TestMethod]
		public void RouteClientHasDefaultAction()
		{
			"~/Client".ShouldMapTo<ClientController>(controller => controller.Index(String.Empty, null, null, null, null, null, (GridSortOptions)null, null));
		}

		[TestMethod]
		public void RouteClientHasIndexAction()
		{

			"~/Client/Index".WithMethod(HttpVerbs.Post).ShouldMapTo<ClientController>(controller => controller.Index(String.Empty, null, null, null, null, null, (GridSortOptions)null, null));
		}

		[TestMethod]
		public void RouteClientHasFindAction()
		{
			"~/Client/Find/Sitro".ShouldMapTo<ClientController>(controller => controller.Find("Sitro"));
		}

		[TestMethod]
		public void RouteClientHasDetailsModalAction()
		{
			"~/Client/DetailsModal/94b73f58-6045-4d74-930d-59e410681ec0".ShouldMapTo<ClientController>(controller => controller.DetailsModal(new Guid("94b73f58-6045-4d74-930d-59e410681ec0")));
		}

		[TestMethod]
		public void RouteClientHasDetailsAction()
		{
			"~/Client/Details/94b73f58-6045-4d74-930d-59e410681ec0".ShouldMapTo<ClientController>(controller => controller.Details(new Guid("94b73f58-6045-4d74-930d-59e410681ec0")));
		}

		[TestMethod]
		public void RouteClientHasCreateModalAction()
		{
			"~/Client/CreateModal/test".ShouldMapTo<ClientController>(controller => controller.CreateModal("test"));
		}

		[TestMethod]
		public void RouteClientHasCreateAction()
		{
			"~/Client/Create/test".ShouldMapTo<ClientController>(controller => controller.Create("test"));
		}

		[TestMethod]
		public void RouteClientHasCreateModalActionPost()
		{
			"~/Client/CreateModal".WithMethod(HttpVerbs.Post).ShouldMapTo<ClientController>(controller => controller.CreateModal((FormCollection)null));
		}

		[TestMethod]
		public void RouteClientHasCreateActionPost()
		{
			"~/Client/Create".WithMethod(HttpVerbs.Post).ShouldMapTo<ClientController>(controller => controller.Create((FormCollection)null));
		}

		[TestMethod]
		public void RouteClientHasEditAction()
		{
			"~/Client/Edit/94b73f58-6045-4d74-930d-59e410681ec0".ShouldMapTo<ClientController>(controller => controller.Edit(new Guid("94b73f58-6045-4d74-930d-59e410681ec0")));
		}

		[TestMethod]
		public void RouteClientHasEditActionPost()
		{
			"~/Client/Edit/94b73f58-6045-4d74-930d-59e410681ec0".WithMethod(HttpVerbs.Post).ShouldMapTo<ClientController>(controller => controller.Edit(new Guid("94b73f58-6045-4d74-930d-59e410681ec0"), null));
		}

		[TestMethod]
		public void RouteClientDeleteAction()
		{
			"~/Client/Delete/00000000-0000-0000-0000-000000000000".ShouldMapTo<ClientController>(controller => controller.Delete(Guid.Empty));
		}

		[TestMethod]
		public void RouteClientDeleteActionPost()
		{
			"~/Client/Delete/00000000-0000-0000-0000-000000000000".WithMethod(HttpVerbs.Post).ShouldMapTo<ClientController>(controller => controller.Delete(Guid.Empty, null));
		}

		
		#endregion

		#region Home Controller

		[TestMethod]
		public void RouteHomeHasDefaultAction()
		{
			"~/Home".ShouldMapTo<HomeController>(controller => controller.Index());
		}

		[TestMethod]
		public void RouteHomeAbout()
		{
			"~/Home/About".ShouldMapTo<HomeController>(controller => controller.About());
		}

		#endregion

		#region MessageType Controller

		[TestMethod]
		public void RouteMessageTypeHasDefaultAction()
		{
			"~/MessageType".ShouldMapTo<MessageTypeController>(controller => controller.Index(null, null, (GridSortOptions)null, null));
		}

		[TestMethod]
		public void RouteMessageTypeHasIndexAction()
		{
			"~/MessageType/Index".ShouldMapTo<MessageTypeController>(controller => controller.Index(null, null, (GridSortOptions)null, null));
		}


		[TestMethod]
		public void RouteMessageTypeFindAction()
		{
			"~/MessageType/Find/Sitro".ShouldMapTo<MessageTypeController>(controller => controller.Find("Sitro"));
		}

		[TestMethod]
		public void RouteMessageTypeDetailsModalAction()
		{
			"~/MessageType/DetailsModal/00000000-0000-0000-0000-000000000000".ShouldMapTo<MessageTypeController>(controller => controller.DetailsModal(Guid.Empty));
		}

		[TestMethod]
		public void RouteMessageTypeDetailsAction()
		{
			"~/MessageType/Details/00000000-0000-0000-0000-000000000000".ShouldMapTo<MessageTypeController>(controller => controller.Details(Guid.Empty));
		}

		[TestMethod]
		public void RouteMessageTypeEdiModaltAction()
		{
			"~/MessageType/EditModal/00000000-0000-0000-0000-000000000000".ShouldMapTo<MessageTypeController>(controller => controller.EditModal(Guid.Empty));
		}

		[TestMethod]
		public void RouteMessageTypeEditAction()
		{
			"~/MessageType/Edit/00000000-0000-0000-0000-000000000000".ShouldMapTo<MessageTypeController>(controller => controller.Edit(Guid.Empty));
		}

		[TestMethod]
		public void RouteMessageTypeHasEditModalActionPost()
		{
			"~/MessageType/EditModal/94b73f58-6045-4d74-930d-59e410681ec0".WithMethod(HttpVerbs.Post).ShouldMapTo<MessageTypeController>(controller => controller.EditModal(new Guid("94b73f58-6045-4d74-930d-59e410681ec0"), null));
		}

		[TestMethod]
		public void RouteMessageTypeHasEditActionPost()
		{
			"~/MessageType/Edit/94b73f58-6045-4d74-930d-59e410681ec0".WithMethod(HttpVerbs.Post).ShouldMapTo<MessageTypeController>(controller => controller.Edit(new Guid("94b73f58-6045-4d74-930d-59e410681ec0"), null));
		}

		#endregion

		#region TransformationFilter Controller

		[TestMethod]
		public void RouteTransformationFilterClientsAction()
		{
			"~/TransformationFilter/Clients/Sender/Sitro".ShouldMapTo<TransformationFilterController>(controller => controller.Clients("Sender", "Sitro"));
		}

		[TestMethod]
		public void RouteTransformationFilterOpositClientsAction()
		{
			"~/TransformationFilter/OpositClients/Sender/00000000-0000-0000-0000-000000000000".ShouldMapTo<TransformationFilterController>(controller => controller.OpositClients("Sender", Guid.Empty));
		}

		#endregion

		#region TransformationType Controller

		[TestMethod]
		public void RouteTransformationTypeFindAction()
		{
			"~/TransformationType/Find/94b73f58-6045-4d74-930d-59e410681ec0".ShouldMapTo<TransformationTypeController>(controller => controller.Find(new Guid("94b73f58-6045-4d74-930d-59e410681ec0")));
		}

		[TestMethod]
		public void RouteTransformationTypeAddAction()
		{
			"~/TransformationType/Add/5".ShouldMapTo<TransformationTypeController>(controller => controller.Add(5));
		}

		#endregion

		#region TransformationSet Controller

		[TestMethod]
		public void RouteTransformationSetHasDefaultAction()
		{
			"~/TransformationSet".ShouldMapTo<TransformationSetController>(controller => controller.Index(null, null, null));
		}

		[TestMethod]
		public void RouteTransformationSetIndexAction()
		{
			"~/TransformationSet/Index".ShouldMapTo<TransformationSetController>(controller => controller.Index(null, null, null));
		}

		[TestMethod]
		public void RouteTransformationSetIndexActionWithParameters()
		{
			"~/TransformationSet/Index/00000000-0000-0000-0000-000000000000/00000000-0000-0000-0000-000000000000".ShouldMapTo<TransformationSetController>(controller => controller.Index(Guid.Empty, Guid.Empty, null));
		}

		[TestMethod]
		public void RouteTransformationSetMessageTypeIndexActionWithParameters()
		{
			"~/TransformationSet/Index/00000000-0000-0000-0000-000000000000/00000000-0000-0000-0000-000000000000/00000000-0000-0000-0000-000000000000".ShouldMapTo<TransformationSetController>(controller => controller.Index(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		[TestMethod]
		public void RouteTransformationSetDetailsAction()
		{
			"~/TransformationSet/Details/00000000-0000-0000-0000-000000000000".ShouldMapTo<TransformationSetController>(controller => controller.Details(Guid.Empty));
		}

		[TestMethod]
		public void RouteTransformationSetCreateAction()
		{
			"~/TransformationSet/Create".ShouldMapTo<TransformationSetController>(controller => controller.Create());
		}

		[TestMethod]
		public void RouteTransformationSetCreateActionPost()
		{
			"~/TransformationSet/Create".WithMethod(HttpVerbs.Post).ShouldMapTo<TransformationSetController>(controller => controller.Create(null));
		}

		[TestMethod]
		public void RouteTransformationSetEditAction()
		{
			"~/TransformationSet/Edit/00000000-0000-0000-0000-000000000000".ShouldMapTo<TransformationSetController>(controller => controller.Edit(Guid.Empty));
		}

		[TestMethod]
		public void RouteTransformationSetEditActionPost()
		{
			"~/TransformationSet/Edit/00000000-0000-0000-0000-000000000000".WithMethod(HttpVerbs.Post).ShouldMapTo<TransformationSetController>(controller => controller.Edit(Guid.Empty, null));
		}

		[TestMethod]
		public void RouteTransformationSetDeleteAction()
		{
			"~/TransformationSet/Delete/00000000-0000-0000-0000-000000000000".ShouldMapTo<TransformationSetController>(controller => controller.Delete(Guid.Empty));
		}

		[TestMethod]
		public void RouteTransformationSetDeleteActionPost()
		{
			"~/TransformationSet/Delete/00000000-0000-0000-0000-000000000000".WithMethod(HttpVerbs.Post).ShouldMapTo<TransformationSetController>(controller => controller.Delete(Guid.Empty, null));
		}

		#endregion

		#region CodeMap Controller

		[TestMethod]
		public void RoutCodeMapHasDefaultAction()
		{
			"~/CodeMapping".ShouldMapTo<CodeMappingController>(controller => controller.Index());
		}

		[TestMethod]
		public void RoutCodeMapIndexAction()
		{
			"~/CodeMapping/Index".ShouldMapTo<CodeMappingController>(controller => controller.Index());
		}

		#endregion

		#region Client PIMA Controller

		[TestMethod]
		public void RouteClientPIMAIndexAction()
		{
			"~/ClientPIMA/Index/00000000-0000-0000-0000-000000000000/prod".ShouldMapTo<ClientPIMAController>(controller => controller.Index(Guid.Empty, "prod"));
		}

		[TestMethod]
		public void RouteClientPIMAEditAction()
		{
			"~/ClientPIMA/Edit/00000000-0000-0000-0000-000000000000/prod".ShouldMapTo<ClientPIMAController>(controller => controller.Edit(Guid.Empty, "prod"));
		}

		[TestMethod]
		public void RouteClientPIMAEditActionPost()
		{
			"~/ClientPIMA/Update/00000000-0000-0000-0000-000000000000".WithMethod(HttpVerbs.Post).ShouldMapTo<ClientPIMAController>(controller => controller.Update(Guid.Empty, null));
		}

		#endregion

		#region Message Routing Controller

		[TestMethod]
		public void RouteMessageRoutingIndexAction()
		{
			"~/MessageRouting/Index/00000000-0000-0000-0000-000000000000".ShouldMapTo<MessageRoutingController>(controller => controller.Index(Guid.Empty));
		}

		[TestMethod]
		public void RouteMessageRoutingEditClientProviderAction()
		{
			"~/MessageRouting/EditClientProvider/00000000-0000-0000-0000-000000000000".ShouldMapTo<MessageRoutingController>(controller => controller.EditClientProvider(Guid.Empty));
		}

		[TestMethod]
		public void RouteMessageRoutingEditClientProviderActionPost()
		{
			"~/MessageRouting/EditClientProvider/00000000-0000-0000-0000-000000000000".WithMethod(HttpVerbs.Post).ShouldMapTo<MessageRoutingController>(controller => controller.EditClientProvider(Guid.Empty, null));
		}

		#endregion

		#region Service Controller
		[TestMethod]
		public void RouteServiceDefaultAction()
		{
			"~/Service".ShouldMapTo<ServiceController>(controller => controller.Index(null));
		}

		[TestMethod]
		public void RouteServiceIndexActionClientId()
		{
			"~/Service/Index/00000000-0000-0000-0000-000000000000".ShouldMapTo<ServiceController>(controller => controller.Index(Guid.Empty));
		}

		[TestMethod]
		public void RouteServiceDetailsAction()
		{
			"~/Service/Details/00000000-0000-0000-0000-000000000000".ShouldMapTo<ServiceController>(controller => controller.Details(Guid.Empty));
		}

		[TestMethod]
		public void RouteServiceDeleteAction()
		{
			"~/Service/Delete/00000000-0000-0000-0000-000000000000".ShouldMapTo<ServiceController>(controller => controller.Delete(Guid.Empty));
		}

		[TestMethod]
		public void RouteServiceDeleteActionPost()
		{
			"~/Service/Delete/00000000-0000-0000-0000-000000000000".WithMethod(HttpVerbs.Post).ShouldMapTo<ServiceController>(controller => controller.DeletePost(Guid.Empty));
		}

		#endregion
	}

}
