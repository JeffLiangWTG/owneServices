using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Business;
using Moq;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Services.ServiceHost.Tests
{
	class NudgeServiceTaskHandlerTest : TestCaseWithFactory
	{
		public void TestNudgeEHIServiceTask()
		{
			var nudger = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(nudger.Object))
			{
				var serviceTaskCode = "EHI";
				nudger.Setup(m => m.NudgeServiceTask(serviceTaskCode, It.IsAny<TimeSpan>()));
				var handler = new NudgeServiceTaskHandler();

				var context = MockHttpContext(serviceTaskCode, "");
				handler.ProcessRequest(context);
				AssertEquals(400, context.Response.StatusCode);
				AssertEquals("Invalid parameter(s)", context.Response.StatusDescription);

				context = MockHttpContext(serviceTaskCode, "a_key_which_is_not_right");
				handler.ProcessRequest(context);
				AssertEquals(500, context.Response.StatusCode);
				AssertEquals("Incorrect key", context.Response.StatusDescription);
				context = MockHttpContext(serviceTaskCode, Constants.AuthKey);
				handler.ProcessRequest(context);
				AssertEquals(200, context.Response.StatusCode);
				AssertEquals("", context.Response.StatusDescription);
				Thread.Sleep(500);
			}
		}

		public void TestNudgeServiceTask_UsesDbSafelyFromAnotherThread()
		{
			var serviceTasksProvider = new ServiceTaskScheduleCollectionProvider();
			var collection = serviceTasksProvider.Load(Factory, ServiceTaskScheduleCollection.RemoteStatus.WithoutStatus);
			var task = collection.Tasks.AddNew();
			task.S5_ScheduleType = "T1A";

			Factory.Save();

			string serviceTaskCode = task.S5_ScheduleType;
			var handler = new NudgeServiceTaskHandler();

			var statusCode = 0;
			var statusDescription = "INIT_VALUE";

			var threadstart = new ThreadStart(delegate
			{
				var context = MockHttpContext(serviceTaskCode, Constants.AuthKey);
				handler.ProcessRequest(context);
				Thread.Sleep(500);
				statusCode = context.Response.StatusCode;
				statusDescription = context.Response.StatusDescription;
			});

			var thread = new Thread(threadstart);
			thread.Start();
			thread.Join(1000);

			AssertEquals(200, statusCode);
			AssertEquals("", statusDescription);

			ErrorReporter.Clear();
		}

		public void TestProcessNudgeRequestDoesNotThrowExceptions()
		{
			// Arrange
			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			using (new DisposableAction(ErrorReporter.Clear))
			{
				serviceTaskNudgerMock
					.Setup(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()))
					.Throws<Exception>();

				// Act
				// Assert
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown(() => NudgeServiceTaskHandler.ProcessNudgeRequest(string.Empty));
					AssertNoExceptionThrown(() => serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.AtLeastOnce));
				});
			}
		}

		public void TestProcessNudgeRequestReportsException()
		{
			CombineAssertions(() =>
			{
				Test(new Exception());
				Test(new InvalidOperationException());
				Test(new Win32Exception());
			});

			void Test(Exception exception)
			{
				// Arrange
				var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
				using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
				using (new DisposableAction(ErrorReporter.Clear))
				{
					serviceTaskNudgerMock
						.Setup(nudger => nudger.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>()))
						.Throws(exception);

					// Act
					NudgeServiceTaskHandler.ProcessNudgeRequest(string.Empty);

					// Assert
					AssertEquals(exception, ErrorReporter.LastExceptionReported);
				}
			}
		}

		public void TestFireServiceTaskNudgerInADifferentThread()
		{
			ServiceTaskNudgerForTest.RunningInDifferentThread = false;
			ServiceTaskNudgerForTest.CallerThreadId = Thread.CurrentThread.ManagedThreadId;

			using (ObjectFactory.Substitute<IServiceTaskNudger>(new ServiceTaskNudgerForTest()))
			{
				var nudgeServiceTaskHandler = new NudgeServiceTaskHandler();
				var httpContext = MockHttpContext("EHI", Constants.AuthKey);
				nudgeServiceTaskHandler.ProcessRequest(httpContext);

				Thread.Sleep(500);

				Assert(ServiceTaskNudgerForTest.RunningInDifferentThread);
			}
		}

		class ServiceTaskNudgerForTest : IServiceTaskNudger
		{
			public static int CallerThreadId;
			public static bool RunningInDifferentThread;

			public void NudgeServiceTask(string serviceTaskCode, TimeSpan? delay = null)
			{
				RunningInDifferentThread = (CallerThreadId != Thread.CurrentThread.ManagedThreadId);
			}
		}

		HttpContext MockHttpContext(string serviceTaskCode, string key)
		{
			var request = new HttpRequest("", "http://testurl", string.Format("code={0}&key={1}", serviceTaskCode, key));
			var context = new HttpContext(request, response);
			return context;
		}

		readonly HttpResponse response = new HttpResponse(new StringWriter());
	}
}
