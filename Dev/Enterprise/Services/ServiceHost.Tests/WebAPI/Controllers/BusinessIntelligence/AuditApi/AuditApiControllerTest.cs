using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Audit;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Test
{
	class AuditApiControllerTest : TransactionedTestCase
	{
		#region Change Summary

		public void TestChangeSummaryValidAllParams()
		{
			AssertValidParameters(new ChangeSummaryParameters() { After_Lsn = "00020C72000003100035", Format = FormatType.JSON });
		}

		public void TestChangeSummaryValidAllParamsWith0x()
		{
			AssertValidParameters(new ChangeSummaryParameters() { After_Lsn = "0x00020C72000003100035", Format = FormatType.JSON });
		}

		public void TestChangeSummaryValidWithDefaultFormat()
		{
			AssertValidParameters(new ChangeSummaryParameters() { After_Lsn = "00020C72000003100035" });
		}

		public void TestChangeSummaryInvalidTooLong()
		{
			AssertInvalidParameters(new ChangeSummaryParameters() { After_Lsn = "000000000000000000000000000" },
				new string[] { "The After_Lsn parameter must be a valid LSN (22 characters long starting with '0x')." });
		}

		public void TestChangeSummaryInvalidTooShort()
		{
			AssertInvalidParameters(new ChangeSummaryParameters() { After_Lsn = "0AB" },
				new string[] { "The After_Lsn parameter must be a valid LSN (22 characters long starting with '0x')." });
		}

		public void TestChangeSummaryInvalidBadChar()
		{
			AssertInvalidParameters(new ChangeSummaryParameters() { After_Lsn = "p0020C72000003100035" },
				new string[] { "The After_Lsn parameter must be a valid LSN (22 characters long starting with '0x')." });
		}

		public void TestChangeSummaryReportUsageSuccess()
		{
			var mockBiReportsService = new Mock<IBiReportsService>();
			Controller._reportsService = mockBiReportsService.Object;

			var mockService = new Mock<IAuditApiService>();
			Controller._auditApiService = mockService.Object;

			var mockResponse = new ChangeSummaryResponse {
				totalItems = 3,
				items = new [] {
					new ChangedTable(),
					new ChangedTable(),
					new ChangedTable()
				}
			};

			mockService.Setup(s => s.GetChangedTablesList(It.IsAny<string>())).Returns(mockResponse);

			Controller.GetChangeSummary(new ChangeSummaryParameters() { After_Lsn = "00020C72000003100035", Format = FormatType.JSON });
			AssertReportUsage(mockBiReportsService, HttpStatusCode.OK, mockResponse.items.Length);
		}

		#endregion

		#region Change Detail

		public void TestChangeDetailValidAllParams()
		{
			AssertValidParameters(new ChangeDetailParameters() { SchemaName = "schema", TableName = "table", After_Lsn = "0x00000000000000000000", Max_Lsn = "0xFFFFFFFFFFFFFFFFFFFF", Format = FormatType.JSON });
		}

		public void TestChangeDetailInvalidTooLong()
		{
			AssertInvalidParameters(new ChangeDetailParameters() { SchemaName = "schema", TableName = "table", After_Lsn = "000000000000000000000000000" },
				new string[] { "The After_Lsn parameter must be a valid LSN (22 characters long starting with '0x')." });
		}

		public void TestChangeDetailMissingSchema()
		{
			AssertInvalidParameters(new ChangeDetailParameters() { After_Lsn = "0x00000000000000000000", TableName = "table" },
				new string[] { "The SchemaName field is required." });
		}

		public void TestChangeDetailMissingTable()
		{
			AssertInvalidParameters(new ChangeDetailParameters() { After_Lsn = "0x00000000000000000000", SchemaName = "schema" },
				new string[] { "The TableName field is required." });
		}

		#endregion

		#region Audit API

		public void TestChecksApiIsEnabled()
		{
			using (SystemDataRegistry.Instance.BiAuditAPI.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var httpActionResult = Controller.GetChangeSummary(new ChangeSummaryParameters());
				GetResponseBody(httpActionResult, out var result, out var returnCode);

				AssertEquals(HttpStatusCode.BadRequest, returnCode);
				AssertEquals(GetExpectedErrorResponse("The Audit Web API has not been enabled on this system."), result);
			}
		}

		public void TestHandlesInvalidModel()
		{
			Controller.ModelState.AddModelError("errorKey", "wrong! :(");

			var httpActionResult = Controller.GetChangeSummary(new ChangeSummaryParameters());
			GetResponseBody(httpActionResult, out var result, out var returnCode);

			AssertEquals(HttpStatusCode.BadRequest, returnCode);
			AssertEquals(GetExpectedErrorResponse("Parameter is invalid: wrong! :("), result);
		}

		public void TestChecksSecurity()
		{
			Env.Security.AuditServices.IsAllowed = false;

			var httpActionResult = Controller.GetChangeSummary(new ChangeSummaryParameters());
			GetResponseBody(httpActionResult, out var result, out var returnCode);

			AssertEquals(HttpStatusCode.BadRequest, returnCode);
			var expectedMessage = "You do not have the appropriate security rights to run this function.\\r\\n\\r\\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\\r\\n\\r\\nManage -> Business Intelligence & Analytics -> API -> Audit Data";
			AssertEquals(GetExpectedErrorResponse(expectedMessage), result);
		}

		public void TestHandlesApiException()
		{
			var mockService = new Mock<IAuditApiService>();
			mockService.Setup(s => s.GetChangedTablesList(It.IsAny<string>())).Throws(new AuditAPIException("my message"));
			Controller._auditApiService = mockService.Object;

			var httpActionResult = Controller.GetChangeSummary(new ChangeSummaryParameters());
			GetResponseBody(httpActionResult, out var result, out var returnCode);

			AssertEquals(HttpStatusCode.BadRequest, returnCode);
			AssertEquals(GetExpectedErrorResponse("my message"), result);
		}

		public void TestAuditApiAuthenticationAttributeIsApplied()
		{
			var controllerType = typeof(AuditApiController);

			AssertCollectionContains(typeof(AuditApiAuthenticationAttribute), controllerType.GetCustomAttributes(inherit: false).Select(o => o.GetType()));
		}

		[ExpectNoExceptions()]
		public void TestFormatterCalledWithFormat()
		{
			{
				var mockFormatter = new Mock<IAuditApiHttpActionResultFactory>();
				Controller._httpResultFactory = mockFormatter.Object;

				var httpActionResult = Controller.GetChangeSummary(new ChangeSummaryParameters());

				mockFormatter.Verify(x => x.GetResult(It.IsAny<ChangeSummaryResponse>(), FormatType.JSON, HttpStatusCode.OK), Times.Once);
			}
		}

		public void TestChecksApiReportUsageNullParameters()
		{
			var mockBiReportsService = new Mock<IBiReportsService>();
			Controller._reportsService = mockBiReportsService.Object;

			Controller.GetChangeSummary(null);
			AssertReportUsage(mockBiReportsService, HttpStatusCode.BadRequest, 0);
		}

		public void TestChecksApiReportUsageUnauthorizedAccess()
		{
			Env.Security.AuditServices.IsAllowed = false;
			var mockBiReportsService = new Mock<IBiReportsService>();
			Controller._reportsService = mockBiReportsService.Object;

			Controller.GetChangeSummary(new ChangeSummaryParameters());
			AssertReportUsage(mockBiReportsService, HttpStatusCode.BadRequest, 0);
		}

		public void TestChecksApiReportUsageApiNotEnabled()
		{
			using (SystemDataRegistry.Instance.BiAuditAPI.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var mockBiReportsService = new Mock<IBiReportsService>();
				Controller._reportsService = mockBiReportsService.Object;

				Controller.GetChangeSummary(new ChangeSummaryParameters());
				AssertReportUsage(mockBiReportsService, HttpStatusCode.BadRequest, 0);
			}
		}

		public void TestChecksApiReportUsageInvalidModel()
		{
			Controller.ModelState.AddModelError("errorKey", "wrong! :(");

			var mockBiReportsService = new Mock<IBiReportsService>();
			Controller._reportsService = mockBiReportsService.Object;

			Controller.GetChangeSummary(new ChangeSummaryParameters());
			AssertReportUsage(mockBiReportsService, HttpStatusCode.BadRequest, 0);
		}

		public void TestChecksApiReportUsageApiException()
		{
			var mockBiReportsService = new Mock<IBiReportsService>();
			Controller._reportsService = mockBiReportsService.Object;

			var mockService = new Mock<IAuditApiService>();
			mockService.Setup(s => s.GetChangedTablesList(It.IsAny<string>())).Throws(new AuditAPIException("my message"));
			Controller._auditApiService = mockService.Object;

			Controller.GetChangeSummary(new ChangeSummaryParameters());
			AssertReportUsage(mockBiReportsService, HttpStatusCode.BadRequest, 0);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			var context = new HttpActionContext()
			{
				ControllerContext = new HttpControllerContext()
				{
					Controller = new DummyController(new GenericIdentity("testUser")),
					Configuration = new HttpConfiguration()
				}
			};

			Controller = new AuditApiController();
			Controller.ControllerContext = context.ControllerContext;
			Controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://test/api/replication/");
			Controller._auditApiService = new Mock<IAuditApiService>().Object;
			Controller._httpResultFactory = new AuditApiHttpActionResultFactory();
			Controller._reportsService = new Mock<IBiReportsService>().Object;

			SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		AuditApiController Controller { get; set; }

		void AssertValidParameters<T>(T obj) => AssertValidation(obj, Array.Empty<string>(), true);

		void AssertInvalidParameters<T>(T obj, IEnumerable<string> errors) => AssertValidation(obj, errors, false);

		void AssertValidation<T>(T obj, IEnumerable<string> errors, bool expected)
		{
			var context = new ValidationContext(obj) { };
			var results = new List<ValidationResult>();
			var succcess = Validator.TryValidateObject(obj, context, results, true);
			CombineAssertions(() =>
			{
				AssertEquals(expected, succcess);
				AssertArrayEqualsByElements(errors.ToArray(), results.Select(e => e.ErrorMessage).ToArray());
			});
		}

		string GetExpectedErrorResponse(string message)
		{
			var expectedApiVersion = new EnterpriseInformationRetriever().VersionNumber;
			return $"{{\"apiVersion\":\"{expectedApiVersion}\",\"error\":{{\"errors\":[{{\"message\":\"{message}\"}}]}}}}";
		}

		void GetResponseBody(IHttpActionResult actionResult, out string body, out HttpStatusCode returnCode)
		{
			var token = new CancellationToken(canceled: false);
			var response = actionResult.ExecuteAsync(token).GetAwaiter().GetResult();

			var deflated = new GZipStream(response.Content.ReadAsStreamAsync().GetAwaiter().GetResult(), CompressionMode.Decompress, leaveOpen: true);
			body = new StreamReader(deflated).ReadToEnd();

			returnCode = response.StatusCode;
		}

		void AssertReportUsage(Mock<IBiReportsService> mockBiReportsService, HttpStatusCode expectedCode, int expectedCount)
		{
			mockBiReportsService.Verify(m => m.ReportUsage("/api/replication/", "", expectedCode.ToString(), Db.AuditDatabaseName, expectedCount.ToString(), It.IsAny<string>()), Times.Once());
			Assert(true);
		}

		#endregion
	}
}
