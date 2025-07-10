using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	public class HttpDpsWebHelperTest : TestCaseWithFactory
	{
		public void TestHandleWebException_LogServerErrorDescription()
		{
			BadHttpStatusCodes.ForEach(TestHandleWebException_LogServerErrorDescription_Core);
		}

		public void TestInternalServerError_LogAsError()
		{
			AssertWebExceptionMessage("DPR", ZDateTime.UtcNow.AddHours(-2.1));
			AssertWebExceptionMessage("DPM", ZDateTime.UtcNow.AddDays(-1));

			void AssertWebExceptionMessage(string serviceTaskcode, ZDateTime dateTimeUtc)
			{
				var logger = new LoggerForTest();
				var webException = CreateInternalServerError_MockHttpWebExceptionAndResponse();

				InsertOrUpdateServiceTaskLastSuccessfulRuntime(dateTimeUtc, serviceTaskcode, 1);
				HttpDpsWebHelper.HandleWebException(webException, logger, new DpsServiceTaskHelper(serviceTaskcode));

				CombineAssertions(() =>
				{
					AssertEquals("Should report Error: ErrorReporter.LastKeyReported", "Web error occurred in DPS Service Task.", ErrorReporter.LastKeyReported);
					AssertEquals("Should report Error: ErrorReporter.LastMessageReported", "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", ErrorReporter.LastMessageReported);
				});
				ErrorReporter.Clear();
			}
		}

		public void TestInternalServerError_LogAsWarning()
		{
			AssertWebExceptionMessage("DPR", ZDateTime.UtcNow.AddHours(-2.1), 0);
			AssertWebExceptionMessage("DPM", ZDateTime.UtcNow.AddHours(-0.5), 1);

			void AssertWebExceptionMessage(string serviceTaskcode, ZDateTime dateTimeUtc, int errorCount)
			{
				var logger = new LoggerForTest();
				var webException = CreateInternalServerError_MockHttpWebExceptionAndResponse();

				InsertOrUpdateServiceTaskLastSuccessfulRuntime(dateTimeUtc, serviceTaskcode, errorCount);
				HttpDpsWebHelper.HandleWebException(webException, logger, new DpsServiceTaskHelper(serviceTaskcode));

				CombineAssertions(() =>
				{
					AssertEquals("Should not report Error: ErrorReporter.LastKeyReported", string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals("Should not report Error: ErrorReporter.LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);
				});
				ErrorReporter.Clear();
			}
		}

		public void TestPostIdentifyDesyncedEntities_WhenValidDataIsPassed_ShouldPopulateRequestHeadersAndReturnValidResponse()
		{
			// Arrange
			string localhost = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";
			var expectedResponse = new[] { Guid.NewGuid() };
			var requestPayload = new List<DpsEntitySyncValidationRequest>();
			byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestPayload));

			var certificateInfo = AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo();
			using var tokenProvider = ObjectFactory.Substitute(nameof(IAuthorizationTokenProvider), new AuthorizationTokenProviderForTest());
			using var mdmSupportCertificateForDPS = OrganisationsDataRegistry.Instance.MDMSupportCertificateForDPS
			.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateInfo);

			var httpService = GetHttpResponseService(new Uri(localhost), JsonConvert.SerializeObject(expectedResponse), 200);

			IEnumerable<Guid> actualResult;

			// Act
			try
			{
				httpService.Start();
				using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DpsWebServiceUrlTestHelper.SetRegistryUrl(localhost)))
				{
					actualResult = HttpDpsWebHelper.PostIdentifyDesyncedEntities(requestData);
				}
			}
			finally
			{
				if (httpService.IsStarted)
				{
					httpService.Stop();
				}
			}

			// Assert
			AssertAuthorizationHeader(httpService.Headers["Authorization"]);
			AssertLicenceCodeHeader(httpService.Headers["LicenceCode"]);
			AssertEquals(expectedResponse[0], actualResult.First());
		}

		void AssertAuthorizationHeader(string headerValue)
		{
			var parts = headerValue.Split(' ');
			AssertEquals("Bearer", parts[0]);
			AssertNotNullOrEmpty(parts[1]);
		}

		void AssertLicenceCodeHeader(string actualLicenceCode)
		{
			var expectedLicenceCode = HMACSHA256Helper.GetComputedLicenceCode(
			GlbBranch.GetOneActiveBranchPerCompany().First().Company.GetLicenceKeyIdentifier("-"));
			AssertEquals(expectedLicenceCode, actualLicenceCode);
		}

		HttpServiceForTest GetHttpResponseService(Uri serviceUrl, string response, int status = 200)
		{
			return new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "POST" },
				Processor = (_, __) => Tuple.Create(status, response),
				Uri = serviceUrl,
				ContentType = "application/json"
			};
		}

		WebException CreateInternalServerError_MockHttpWebExceptionAndResponse()
		{
			var sqlException = SqlExceptionBuilder.CreateSqlException(
					SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));

			var mockHttpWebResponse = new Mock<HttpWebResponse>();
			mockHttpWebResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.InternalServerError);
			mockHttpWebResponse.Setup(x => x.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(sqlException.Message)));

			return new WebException("The remote server returned an error: (500) Internal Server Error.", null, WebExceptionStatus.ProtocolError, mockHttpWebResponse.Object);
		}

		void InsertOrUpdateServiceTaskLastSuccessfulRuntime(ZDateTime lastSuccessfulRuntime, string serviceTaskCode, int failedToRunCount)
		{
			var runningStatus = new ServiceTaskRunningStatus
			{
				FailedToRunCount = failedToRunCount,
				LastSuccessfulRuntime = lastSuccessfulRuntime.ToDateTime(),
			};

			var binaryValue = ZBlob.FromUTF8(JsonConvert.SerializeObject(runningStatus));

			_ = TestConnection.ExecuteNonQuery($@"
IF ((SELECT COUNT(*) FROM dbo.StmData WHERE SD_Name = 'ServiceTask{serviceTaskCode}_LastSuccessfulRuntime') = 0)
BEGIN
	INSERT INTO dbo.StmData (SD_PK, SD_Name)
	VALUES (NEWID(), 'ServiceTask{serviceTaskCode}_LastSuccessfulRuntime')
END

UPDATE dbo.StmData SET SD_BinaryValue = @BinaryValue
WHERE SD_Name = 'ServiceTask{serviceTaskCode}_LastSuccessfulRuntime'", parameters => parameters.AddParameter("@BinaryValue", System.Data.SqlDbType.VarBinary, binaryValue.XmlSerializedValue));
		}

		void TestHandleWebException_LogServerErrorDescription_Core(HttpStatusCode httpStatusCode)
		{
			const string exceptionFromServer = @"System.Data.SqlClient.SqlException (0x80131904): Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.
 ---> System.ComponentModel.Win32Exception (258): The wait operation timed out.
   at System.Data.SqlClient.SqlCommand.<>c.<ExecuteDbDataReaderAsync>b__126_0(Task`1 result)
   at System.Threading.Tasks.ContinuationResultTaskFromResultTask`2.InnerInvoke()
   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
--- End of stack trace from previous location where exception was thrown ---
   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot, Thread threadPoolThread)
--- End of stack trace from previous location where exception was thrown ---
   at Dapper.SqlMapper.QueryAsync[T](IDbConnection cnn, Type effectiveType, CommandDefinition command) in /_/Dapper/SqlMapper.Async.cs:line 418
   at Enterprise.DeniedPartyScreening.Service.Business.PendingRescreenAdviceProvider.GetPendingRescreenAdvices(String licence, PendingRescreenAdviceRequest request) in C:\BS\git\wtg\MDM\WebServices\DeniedPartyScreening\DPSv4\src\Enterprise.DeniedPartyScreening.Service.Business\PendingRescreenAdviceProvider.cs:line 62
   at Enterprise.DeniedPartyScreening.Service.Business.PendingRescreenAdviceProvider.GetPendingRescreenAdvices(String licence, PendingRescreenAdviceRequest request) in C:\BS\git\wtg\MDM\WebServices\DeniedPartyScreening\DPSv4\src\Enterprise.DeniedPartyScreening.Service.Business\PendingRescreenAdviceProvider.cs:line 72
   at Enterprise.DeniedPartyScreening.Service.Business.DeniedPartyScreeningServiceProvider.GetPendingRescreenAdvicesCore(PendingRescreenAdviceRequest request, String licenceCode) in C:\BS\git\wtg\MDM\WebServices\DeniedPartyScreening\DPSv4\src\Enterprise.DeniedPartyScreening.Service.Business\DeniedPartyScreeningServiceProvider.cs:line 86
   at Enterprise.DeniedPartyScreening.Service.Business.DeniedPartyScreeningServiceProvider.GetPendingRescreenAdvices(String licenceCode) in C:\BS\git\wtg\MDM\WebServices\DeniedPartyScreening\DPSv4\src\Enterprise.DeniedPartyScreening.Service.Business\DeniedPartyScreeningServiceProvider.cs:line 67
   at Enterprise.DeniedPartyScreening.Service.Rest.Api.Controller.DpsServiceController.GetPendingRescreenAdvices(String licenseCode) in C:\BS\git\wtg\MDM\WebServices\DeniedPartyScreening\DPSv4\src\Enterprise.DeniedPartyScreening.Service.Api\Controllers\DpsServiceController.cs:line 64
   at lambda_method(Closure , Object )
   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Awaited|12_0(ControllerActionInvoker invoker, ValueTask`1 actionResultValueTask)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeNextResourceFilter>g__Awaited|24_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Rethrow(ResourceExecutedContextSealed context)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|19_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Awaited|17_0(ResourceInvoker invoker, Task task, IDisposable scope)
   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|6_0(Endpoint endpoint, Task requestTask, ILogger logger)
   at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
   at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
   at NSwag.AspNetCore.Middlewares.OpenApiDocumentMiddleware.Invoke(HttpContext context)
   at NSwag.AspNetCore.Middlewares.OpenApiDocumentMiddleware.Invoke(HttpContext context)
   at Enterprise.DeniedPartyScreening.Service.Api.ExceptionMiddleware.InvokeAsync(HttpContext httpContext) in C:\BS\git\wtg\MDM\WebServices\DeniedPartyScreening\DPSv4\src\Enterprise.DeniedPartyScreening.Service.Api\Middlewares\ExceptionMiddleware.cs:line 34
ClientConnectionId:aa06bd9e-f823-4006-a75e-ff65b55fe1e6
Error Number:-2,State:0,Class:11";

			var expectedMessage = $"Web error occurred in DPS Service Task.\r\n{exceptionFromServer}\r\n";

			var mockHttpWebResponse = new Mock<HttpWebResponse>();
			mockHttpWebResponse.Setup(x => x.StatusCode).Returns(httpStatusCode);
			mockHttpWebResponse.Setup(x => x.GetResponseStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(exceptionFromServer)));

			var webException = new WebException("Fake exception for test", new Exception("fake inner exception"), WebExceptionStatus.ProtocolError, mockHttpWebResponse.Object);
			var logger = new LoggerForTest();

			HttpDpsWebHelper.HandleWebException(webException, logger);

			AssertEquals(1, logger.LogEntries.Count());
			AssertEquals(expectedMessage, logger.LogEntries.First());

			if (ShouldReportOnceBadHttpStatusCodes.Contains(httpStatusCode))
			{
				AssertEquals("Web error occurred in DPS Service Task.", ErrorReporter.LastKeyReported);
				AssertEquals(exceptionFromServer, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			else
			{
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		static readonly HttpStatusCode[] BadHttpStatusCodes =
		{
			HttpStatusCode.MovedPermanently,
			HttpStatusCode.BadRequest,
			HttpStatusCode.Unauthorized,
			HttpStatusCode.InternalServerError,
			HttpStatusCode.GatewayTimeout
		};

		static readonly HttpStatusCode[] ShouldReportOnceBadHttpStatusCodes =
		{
			HttpStatusCode.InternalServerError
		};
	}
}
