using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http.Results;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Services.ServiceHost.Tests
{
	class ReportRunningControllerTest : BaseReportControllerTest<ReportRunningController>
	{
		public void TestGetReportSummary()
		{
			AssertEquals("pre-condition", false, Env.CurrentUser.IsWebUser);
			var expectedResult = new List<ReportSummaryData>
			{
				new ReportSummaryData
				{
					Id = Guid.NewGuid(), IsClientSpecific = false, IsPublished = true, IsSystemDefined = true, ReportDescription = "Report 1", ReportName = "Report 1"
				},
				new ReportSummaryData
				{
					Id = Guid.NewGuid(), IsClientSpecific = false, IsPublished = true, IsSystemDefined = true, ReportDescription = "Report 2", ReportName = "Report 2"
				}
			};
			var businessContext = "RepWareHouse";

			mockService.Setup(x => x.GetReportSummaryCollection(businessContext)).Returns(expectedResult).Verifiable();

			var actionResult = controller.GetReportSummaries(businessContext);
			AssertJsonResult(expectedResult, actionResult);

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("pre-condition", true, Env.CurrentUser.IsWebUser);

				expectedResult = new List<ReportSummaryData>
				{
					new ReportSummaryData
					{
						Id = Guid.NewGuid(), IsClientSpecific = false, IsPublished = true, IsSystemDefined = true, ReportDescription = "Report 1", ReportName = "Report 1"
					},
					new ReportSummaryData
					{
						Id = Guid.NewGuid(), IsClientSpecific = false, IsPublished = true, IsSystemDefined = true, ReportDescription = "Report 2", ReportName = "Report 2"
					}
				};
				businessContext = "RepWareHouse";
				var webReportModes = new List<string> { businessContext };

				mockService.Setup(x => x.GetReportSummaryCollectionForContact(webReportModes)).Returns(expectedResult).Verifiable();

				actionResult = controller.GetReportSummaries(businessContext);
				AssertJsonResult(expectedResult, actionResult);
			}
		}

		public void TestGetReportData()
		{
			var id = Guid.NewGuid();
			var expectedResult = new ReportData { Id = id, SortOrderCollection = new[] { "Sort1", "Sort2" } };
			expectedResult.FilterData.AccountingNumberRangeFilterCollection.Add(new AccountingNumberRangeFilter { DisplayName = "WhatEver" });

			mockService.Setup(x => x.GetReportData(id)).Returns(expectedResult).Verifiable();

			var actionResult = controller.GetReportData(id);
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetReportBytes()
		{
			var expectedData = new byte[] { 1, 2, 3 };
			var reportBinaryData = new ReportBinaryData { Data = expectedData, FileType = FileType.XLS, Name = "Test" };

			var selectedReportData = new SelectedValueReportData { Id = Guid.NewGuid() };
			mockService.Setup(x => x.GetReportBytes(selectedReportData)).Returns(reportBinaryData).Verifiable();

			var responseMessage = controller.GetReportBytes(selectedReportData);
			var content = ((ResponseMessageResult)responseMessage).Response.Content;
			var actualData = content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
			AssertEquals(actualData, expectedData);

			AssertEquals("application/octet-stream", content.Headers.ContentType.MediaType);
			AssertEquals("attachment; filename=Test.XLS", content.Headers.ContentDisposition.ToString());
		}

		public void TestGetReportBytes_NotFoundResponse()
		{
			var response = controller.GetReportBytes(new SelectedValueReportData { Id = Guid.NewGuid() });
			AssertEquals(HttpStatusCode.NotFound, ((ResponseMessageResult)response).Response.StatusCode);
		}

		public void TestGetReportBytes_BadRequestResponse()
		{
			var expectedValidationError = new ReportRunningError { ErrorType = ReportServiceErrorType.ValidationError };
			expectedValidationError.Errors.Add("First Filter Validation Error");
			expectedValidationError.Errors.Add("Second Filter Validation Error");

			var selectedReportData = new SelectedValueReportData { Id = Guid.NewGuid() };
			mockService.Setup(x => x.RunningError).Returns(expectedValidationError);

			var attribute = new ReportServiceErrorHandlerAttribute();
			var executedContext = new System.Web.Http.Filters.HttpActionExecutedContext(controller.ActionContext, null);

			controller.GetReportBytes(selectedReportData);
			attribute.OnActionExecuted(executedContext);
			AssertJsonResult(expectedValidationError, executedContext.Response);

			var expectedRunningError = new ReportRunningError { ErrorType = ReportServiceErrorType.RunningError };
			expectedRunningError.Errors.Add("Only One Running Error");

			mockService.Setup(x => x.RunningError).Returns(expectedRunningError);
			controller.GetReportBytes(selectedReportData);
			attribute.OnActionExecuted(executedContext);
			AssertJsonResult(expectedRunningError, executedContext.Response);
		}

		public void TestGetLookupDependencyValue()
		{
			var id = Guid.NewGuid();
			var expectedResult = "GlbStaff";
			mockService.Setup(x => x.GetDependencyValueForLookupFilter(id, "FilterName", "STF")).Returns(expectedResult).Verifiable();
			var actionResult = controller.GetLookupDependencyValue(id, "FilterName", "STF");
			AssertJsonResult(expectedResult, actionResult);
		}

		public void TestGetCodeListDependencyValue()
		{
			var id = Guid.NewGuid();
			var expectedResult = new List<CodeDescription> { new CodeDescription { Code = "STF", Description = "Staff" }, new CodeDescription { Code = "ORG", Description = "Organisation" } };
			mockService.Setup(x => x.GetDependencyValueForCodeListMultipleChoiceFilter(id, "FilterName", "SET")).Returns(expectedResult).Verifiable();
			var actionResult = controller.GetCodeListDependencyValue(id, "FilterName", "SET");
			AssertJsonResult(expectedResult, actionResult);
		}
	}
}
