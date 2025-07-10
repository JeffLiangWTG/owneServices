using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.DocumentEngine;
using Enterprise.Environment;

namespace Enterprise.Services.ServiceHost
{
	#region SuppressResourceStringsCheckRegion

	[GlowTicketAuthentication]
	[ReportServiceErrorHandler]
	public class ReportRunningController : ReportDataBaseController
	{
		[Route("api/report/running/summaries/{*businessContext}")]
		[HttpGet]
		public IHttpActionResult GetReportSummaries(string businessContext)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (Env.CurrentUser.IsWebUser)
				{
					return Json(Service.GetReportSummaryCollectionForContact(string.IsNullOrEmpty(businessContext) ? new List<string>() : businessContext.Split(',').Select(s => s.Trim()).ToList()));
				}
				else
				{
					return Json(Service.GetReportSummaryCollection(businessContext));
				}
			}
		}

		[Route("api/report/running/data/{id}")]
		[HttpGet]
		public IHttpActionResult GetReportData(Guid id)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetReportData(id));
			}
		}

		[Route("api/report/running/bytes")]
		[HttpPost]
		public IHttpActionResult GetReportBytes(SelectedValueReportData reportData)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var reportBinaryData = Service.GetReportBytes(reportData);
				if (reportBinaryData?.Data != null)
				{
					var response = new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StreamContent(new MemoryStream(reportBinaryData.Data))
					};

					var dispositionType = "attachment";
					var mimeType = "application/octet-stream";
					response.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(dispositionType)
					{ FileName = FormattableString.Invariant($"{reportBinaryData.Name}.{reportBinaryData.FileType}") };

					return ResponseMessage(response);
				}

				return ResponseMessage(new HttpResponseMessage(HttpStatusCode.NotFound));
			}
		}

		[Route("api/report/running/getlookupdependencyvalue")]
		[HttpGet]
		public IHttpActionResult GetLookupDependencyValue(Guid id, string filterName, string selectedValue)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetDependencyValueForLookupFilter(id, filterName, selectedValue));
			}
		}

		[Route("api/report/running/getcodelistdependencyvalue")]
		[HttpGet]
		public IHttpActionResult GetCodeListDependencyValue(Guid id, string filterName, string selectedValue)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetDependencyValueForCodeListMultipleChoiceFilter(id, filterName, selectedValue));
			}
		}
	}

	#endregion
}
