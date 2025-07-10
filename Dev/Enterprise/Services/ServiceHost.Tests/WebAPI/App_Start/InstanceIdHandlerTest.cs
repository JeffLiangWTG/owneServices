using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class InstanceIdHandlerTest : TestCaseWithFactory
	{
		public void TestInstanceIdHandler_CreateResponseWithNoInstanceIdHeader_WhenOk()
		{
			using (var handler = new InstanceIdHandler())
			using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo.fr"))
			{
				handler.InnerHandler = new TestHandler(HttpStatusCode.OK);
				using (var invoker = new HttpMessageInvoker(handler))
				{
					var result = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result;
					result.Headers.TryGetValues("wtg-instid", out var instanceId);
					AssertNull(instanceId);
				}
			}
		}

		public void TestInstanceIdHandler_ShouldNotCacheInstanceIdException()
		{
			var errorReporterMock = new Mock<IErrorReporter>();

			using (var handler = new InstanceIdHandler())
			using (var invoker = new HttpMessageInvoker(handler))
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo.fr"))
			{
				handler.InnerHandler = new TestHandler(HttpStatusCode.Unauthorized);
				{
					HttpResponseMessage response = null;
					using (Db.Connection.BeginTransactionWithManager())
					{
						Db.Connection.ExecuteNonQuery("sp_rename 'GetInstanceId', 'GetInstanceId_Temp'");

						AssertNoExceptionThrown(() => response = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result);
						AssertEquals(response.Headers.Count(), 0);

						errorReporterMock.Verify(reporter => reporter.Report(null, "Unable to retrieve database Instance Id", It.IsAny<SqlException>()), Times.Once);

						AssertNoExceptionThrown(() => response = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result);
						AssertEquals(response.Headers.Count(), 0);

						errorReporterMock.Verify(reporter => reporter.Report(null, "Unable to retrieve database Instance Id", It.IsAny<SqlException>()), Times.Exactly(2));

						Db.Connection.ExecuteNonQuery("sp_rename 'GetInstanceId_Temp', 'GetInstanceId'");
					}

					AssertNoExceptionThrown(() => response = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result);
					AssertEquals(response.Headers.Count(), 1);
					Assert(response.Headers.Contains("wtg-instid"));
					errorReporterMock.Verify(reporter => reporter.Report(null, "Unable to retrieve database Instance Id", It.IsAny<SqlException>()), Times.Exactly(2));
				}
			}
		}

		public void TestInstanceIdHandler_CreateResponseWithInstanceIdHeader_WhenUnauthorized()
		{
			string instanceId;
			using (var command = Db.Connection.Command("GetInstanceId"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddOutputParameter("@InstanceId", SqlDbType.VarChar, 10, 0, 0, null);
				command.ExecuteNonQuery();

				instanceId = (string)command.GetParameterValue("@InstanceId");
			}

			using (var handler = new InstanceIdHandler())
			using (var invoker = new HttpMessageInvoker(handler))
			using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo.fr"))
			{
				handler.InnerHandler = new TestHandler(HttpStatusCode.Unauthorized);
				{
					var result = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result;
					result.Headers.TryGetValues("wtg-instid", out var instanceIdHeader);
					AssertEquals(instanceId, instanceIdHeader.FirstOrDefault());
				}
			}
		}

		public void TestInstanceIdHandler_GetInstanceIdShouldBeCached()
		{
			using (Db.Connection.TrackExecutedCommands())
			using (var handler = new InstanceIdHandler())
			{
				handler.InnerHandler = new TestHandler(HttpStatusCode.Unauthorized);
				using (var invoker = new HttpMessageInvoker(handler))
				using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo.fr"))
				{
					var result = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result;
					var result1 = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result;

					result.Headers.TryGetValues("wtg-instid", out var instanceIdHeader);
					result1.Headers.TryGetValues("wtg-instid", out var instanceIdHeader1);
					AssertEquals(instanceIdHeader.FirstOrDefault(), instanceIdHeader1.FirstOrDefault());
				}

				AssertEquals(Db.Connection.ExecutedCommands.Count(), 1);
			}
		}

		[ExpectNoExceptions]
		public void TestInstanceIdHandler_UsesDbSafelyFromAnotherThread()
		{
			string instanceId;
			using (var command = Db.Connection.Command("GetInstanceId"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddOutputParameter("@InstanceId", SqlDbType.VarChar, 10, 0, 0, null);
				command.ExecuteNonQuery();

				instanceId = (string)command.GetParameterValue("@InstanceId");
			}

			using (var handler = new InstanceIdHandler())
			using (var invoker = new HttpMessageInvoker(handler))
			using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo.fr"))
			{
				handler.InnerHandler = new TestHandler(HttpStatusCode.Unauthorized);

				RunSafelyFromAnotherThread(() =>
				{
					var result = invoker.SendAsync(httpRequestMessage, new CancellationToken()).Result;

					result.Headers.TryGetValues("wtg-instid", out var instanceIdHeader);
					AssertEquals(instanceId, instanceIdHeader.FirstOrDefault());
				});
			}
		}

		void RunSafelyFromAnotherThread(Action actionToRun)
		{
			ExceptionDispatchInfo exceptionInfo = null;

			var thread = new Thread(() =>
			{
				try
				{
					actionToRun();
				}
				catch (Exception ex)
				{
					exceptionInfo = ExceptionDispatchInfo.Capture(ex);
				}
			});

			thread.Start();
			thread.Join(1000);

			exceptionInfo?.Throw();
		}

		class TestHandler : DelegatingHandler
		{
			public TestHandler(HttpStatusCode httpStatusCode)
			{
				this.httpStatusCode = httpStatusCode;
			}

			readonly HttpStatusCode httpStatusCode;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(new HttpResponseMessage(httpStatusCode));
			}
		}
	}
}
