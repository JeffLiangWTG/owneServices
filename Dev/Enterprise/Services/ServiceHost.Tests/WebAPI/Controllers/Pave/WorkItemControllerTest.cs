using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.Data.Testing;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WorkItemControllerTest : PaveControllerTestCase<IWorkItemService, WorkItemController>
	{
		public override void TestOnCreateController_ShouldUseDbConnectionCorrectly()
		{
			var lasErrorReported = string.Empty;
			WorkItemController controller = null;

			Task.Run(() =>
			{
				ErrorReporter.Clear();
				lasErrorReported = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
				controller = new WorkItemController();

				controller.Dispose();
			})
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult();

			AssertNullOrEmpty(lasErrorReported);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetWorkItemTypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			using (var controller = new WorkItemController())
			{
				PrepareController(controller);

				var actionResult = controller.GetWorkItemTypes();
				var statusCode = GetStatusCode(actionResult);
				var result = GetResult(actionResult);

				AssertEquals(HttpStatusCode.OK, statusCode);
				AssertEquals(@"{""success"":true,""error"":null,""data"":[{""code"":""1AA"",""description"":""1AA depth 1""},{""code"":""1BB"",""description"":""1BB depth 1""},{""code"":""1ZZ"",""description"":""1ZZ depth 1""}]}", result);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetWorkItemAreas()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			using (var controller = new WorkItemController())
			{
				PrepareController(controller);

				var actionResult = controller.GetWorkItemAreas("1AA");
				var statusCode = GetStatusCode(actionResult);
				var result = GetResult(actionResult);

				AssertEquals(HttpStatusCode.OK, statusCode);
				AssertEquals(@"{""success"":true,""error"":null,""data"":[{""code"":""2AA"",""description"":""2AA depth 2""},{""code"":""2AB"",""description"":""2AB depth 2""},{""code"":""2CC"",""description"":""2CC depth 2""},{""code"":""2DD"",""description"":""2DD depth 2""},{""code"":""2ZZ"",""description"":""2ZZ depth 2""}]}", result);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetActivityTypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			using (var controller = new WorkItemController())
			{
				PrepareController(controller);

				var actionResult = controller.GetActivityTypes("1AA", "2AA");
				var statusCode = GetStatusCode(actionResult);
				var result = GetResult(actionResult);

				AssertEquals(HttpStatusCode.OK, statusCode);
				AssertEquals(@"{""success"":true,""error"":null,""data"":[{""code"":""3AA"",""description"":""3AA depth 3""},{""code"":""3S1"",""description"":""3S1 depth 3""},{""code"":""3S2"",""description"":""3S2 depth 3""},{""code"":""A11"",""description"":""A11 depth 3""}]}", result);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetActivitySubTypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			using (var controller = new WorkItemController())
			{
				PrepareController(controller);

				var actionResult = controller.GetActivitySubtypes("1AA", "2AA", "3AA");
				var statusCode = GetStatusCode(actionResult);
				var result = GetResult(actionResult);

				AssertEquals(HttpStatusCode.OK, statusCode);
				AssertEquals(@"{""success"":true,""error"":null,""data"":[{""code"":""4AA"",""description"":""4AA depth 4""},{""code"":""4AX"",""description"":""4AX depth 4""},{""code"":""4XX"",""description"":""4XX depth 4""},{""code"":""ANY"",""description"":""ANY depth 4""}]}", result);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGetPriorities()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree5();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			using (var controller = new WorkItemController())
			{
				PrepareController(controller);

				var actionResult = controller.GetPriorities("1ZZ", "2ZZ", "3ZZ", "4ZZ");
				var statusCode = GetStatusCode(actionResult);
				var result = GetResult(actionResult);

				AssertEquals(HttpStatusCode.OK, statusCode);
				AssertEquals(@"{""success"":true,""error"":null,""data"":[{""code"":""5ZZ"",""description"":""5ZZ depth 5""},{""code"":""LOW"",""description"":""LOW depth 5""},{""code"":""MED"",""description"":""MED depth 5""}]}", result);
			}
		}

		void PrepareController(WorkItemController controller)
		{
			var mockHttpRequestMessage = new Mock<HttpRequestMessage> { CallBase = true };
			var mocHttpConfiguration = new Mock<HttpConfiguration> { CallBase = true };
			controller.ControllerContext = new HttpControllerContext(mocHttpConfiguration.Object, new HttpRouteData(new HttpRoute()), mockHttpRequestMessage.Object)
			{
				Controller = controller
			};
			controller.Request.RequestUri = new Uri("http://URI");
			var identity = GlowTicketTestHelper.CreateStaffIdentity((GlbStaff)Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			controller.User = new GenericPrincipal(identity, null);
		}
	}
}
