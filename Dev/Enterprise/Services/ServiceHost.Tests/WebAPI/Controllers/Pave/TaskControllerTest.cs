using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Common;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Task;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Enterprise.Services.ServiceHost.Tests
{
	class TaskControllerTest : PaveControllerTestCase<ITaskService, TaskController>
	{
		#region SetUp

		protected Mock<ITaskService> ServiceMock;
		protected TaskController Controller;
		readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
		};

		protected override void SetUp()
		{
			base.SetUp();
			serializerSettings.Converters.Add(new StringEnumConverter());
			ServiceMock = new Mock<ITaskService>();
			var mockHttpRequestMessage = new Mock<HttpRequestMessage> { CallBase = true };
			var mocHttpConfiguration = new Mock<HttpConfiguration> { CallBase = true };
			var identity = GlowTicketTestHelper.CreateStaffIdentity((GlbStaff)Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			Controller = (TaskController)Activator.CreateInstance(typeof(TaskController));
			Controller.SetServiceForTest(ServiceMock.Object);
			Controller.ControllerContext = new HttpControllerContext(mocHttpConfiguration.Object, new HttpRouteData(new HttpRoute()), mockHttpRequestMessage.Object)
			{
				Controller = Controller
			};
			Controller.Request.RequestUri = new Uri("http://URI");
			Controller.User = new GenericPrincipal(identity, null);
		}

		#endregion

		public override void TestOnCreateController_ShouldUseDbConnectionCorrectly()
		{
			var lasErrorReported = string.Empty;
			TaskController controller = null;

			Task.Run(() =>
			{
				ErrorReporter.Clear();
				lasErrorReported = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
				controller = new TaskController();

				controller.Dispose();
			})
			.ConfigureAwait(false)
			.GetAwaiter()
			.GetResult();

			AssertNullOrEmpty(lasErrorReported);
		}

		#region Assignment

		public void TestDelete_AssignmentCapability()
		{
			var taskPK = Guid.NewGuid();

			ServiceMock.Setup(m => m.TryDeleteAssignmentCapability(taskPK, out It.Ref<PaveError>.IsAny)).Returns(true);

			var statusCode = GetStatusCode(Controller.DeleteAssignmentCapability(taskPK));
			var result = GetResult(Controller.DeleteAssignmentCapability(taskPK));

			ServiceMock.Verify();

			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals(SerializeToPaveResponseJson<object>(), result);
		}

		public void TestDelete_AssignmentStaff()
		{
			var taskPK = Guid.NewGuid();

			ServiceMock.Setup(m => m.TryDeleteAssignmentStaff(taskPK, out It.Ref<PaveError>.IsAny)).Returns(true);

			var statusCode = GetStatusCode(Controller.DeleteAssignmentStaff(taskPK));
			var result = GetResult(Controller.DeleteAssignmentStaff(taskPK));

			ServiceMock.Verify();

			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals(SerializeToPaveResponseJson<object>(), result);
		}

		public void TestDelete_Assignment()
		{
			var taskPK = Guid.NewGuid();

			ServiceMock.Setup(m => m.TryDeleteAssignment(taskPK, out It.Ref<PaveError>.IsAny)).Returns(true);

			var statusCode = GetStatusCode(Controller.DeleteAssignment(taskPK));
			var result = GetResult(Controller.DeleteAssignment(taskPK));

			ServiceMock.Verify();

			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals(SerializeToPaveResponseJson<object>(), result);
		}

		#endregion

		#region Change Channel

		public void TestPost_ChangeTaskChannel()
		{
			var dto = CreateDummyChannelTaskDTO("to", TaskChangeChannelMethod.SelectedTask);

			ServiceMock.Setup(m => m.TryChangeChannel(dto, false)).Returns(new TryChangeTaskChannelResult());

			var statusCode = GetStatusCode(Controller.PostChangeTaskChannel(dto));
			var result = GetResult(Controller.PostChangeTaskChannel(dto));

			ServiceMock.Verify();

			AssertEquals(HttpStatusCode.OK, statusCode);
		}

		#endregion

		#region Notes

		public void Test_GetNotes()
		{
			var dto = new TaskNotesResponse
			{
				Content = "The war is over!",
				Hash = Guid.NewGuid().ToString(),
			};

			var taskPK = new Guid();
			ServiceMock.Setup(m => m.GetNotes(taskPK)).Returns(dto);

			var statusCode = GetStatusCode(Controller.GetNotes(taskPK));
			var result = GetResult(Controller.GetNotes(taskPK));

			ServiceMock.Verify();

			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals(SerializeToPaveResponseJson(dto), result);
		}

		public void Test_PostChangeNotes()
		{
			var dto = new TryUpdateNotesResult(new TaskNotesResponse
			{
				Content = "The war is never over!",
				Hash = Guid.NewGuid().ToString(),
			});

			var request = new UpdateTaskNotesRequest
			{
				NewNotes = "BLA",
				PreviousHash = "hash"
			};

			var taskPK = new Guid();
			ServiceMock.Setup(m => m.TryUpdateNotes(taskPK, request)).Returns(dto);

			var statusCode = GetStatusCode(Controller.PostChangeNotes(taskPK, request));
			var result = GetResult(Controller.PostChangeNotes(taskPK, request));

			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals(SerializeToPaveResponseJson(dto.TaskNotesResponse), result);

			ServiceMock.Reset();
			ServiceMock.Setup(m => m.TryUpdateNotes(taskPK, request)).Returns(new TryUpdateNotesResult(new[] { "On no!" }));

			statusCode = GetStatusCode(Controller.PostChangeNotes(taskPK, request));
			result = GetResult(Controller.PostChangeNotes(taskPK, request));
			AssertEquals(HttpStatusCode.OK, statusCode);
			AssertEquals(SerializeToPaveResponseJson<TryUpdateNotesResult>(error: new PaveError(PaveErrorCode.ValidationError, new[] { "On no!" })), result);

			request.PreviousHash = null;
			statusCode = GetStatusCode(Controller.PostChangeNotes(taskPK, request));
			AssertEquals(HttpStatusCode.BadRequest, statusCode);
		}

		#endregion

		#region Helpers

		protected new string SerializeToPaveResponseJson<T>(T obj = null, PaveError error = null)
			where T : class
		{
			var paveResponse = new PaveResponse<T>(obj, error);
			return JsonConvert.SerializeObject(paveResponse, serializerSettings);
		}

		ChangeTaskChannelRequestDTO CreateDummyChannelTaskDTO(string destinationChannelEntityCode = null, TaskChangeChannelMethod method = TaskChangeChannelMethod.SelectedTask)
		{
			return new ChangeTaskChannelRequestDTO
			{
				TaskPK = Guid.NewGuid(),
				DestinationChannelEntityPK = destinationChannelEntityCode,
				Method = method
			};
		}

		#endregion
	}
}
