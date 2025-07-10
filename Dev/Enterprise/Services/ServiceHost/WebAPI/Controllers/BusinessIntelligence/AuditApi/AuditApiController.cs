using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Audit
{
	[Authorize]
	[AuditApiAuthentication]
	public class AuditApiController : ApiController
	{
		[ThreadSafe]
		protected IBiReportsService ReportsService => _reportsService ??= new BiReportsService();
		internal IBiReportsService _reportsService;

		IAuditApiHttpActionResultFactory HttpResultFactory => _httpResultFactory ??= new AuditApiHttpActionResultFactory();
		internal IAuditApiHttpActionResultFactory _httpResultFactory;

		IAuditApiService AuditApiService => _auditApiService ??= new AuditApiService();
		internal IAuditApiService _auditApiService;

		/// Get the tables that have had changes since afterLsn
		[Route("api/replication/change-summary")]
		[HttpGet]
		public IHttpActionResult GetChangeSummary([FromUri] ChangeSummaryParameters parameters)
		{
			return RunWithChecks(() =>
			{
				var data = AuditApiService.GetChangedTablesList(parameters.After_Lsn);
				ReportUsage(HttpStatusCode.OK, data.items?.Length ?? 0);
				return HttpResultFactory.GetResult(data, parameters.Format);
			}, parameters);
		}

		/// Get the tables that have had changes since afterLsn
		[Route("api/replication/change-detail")]
		[HttpGet]
		public IHttpActionResult GetChangeDetail([FromUri] ChangeDetailParameters parameters)
		{
			return RunWithChecks(() =>
			{
				var maxLsn = !string.IsNullOrEmpty(parameters.Max_Lsn) ? parameters.Max_Lsn : "0xFFFFFFFFFFFFFFFFFFFF";
				var afterSeqVal = !string.IsNullOrEmpty(parameters.After_Seqval) ? parameters.After_Seqval : "0xFFFFFFFFFFFFFFFFFFFF";
				var afterCommandId = parameters.After_Command_Id ?? int.MaxValue;
				var afterOperation = parameters.After_Operation ?? 4;
				var pageSize = parameters.Page_Size ?? 1000;

				var data = AuditApiService.GetChangeDetail(parameters.SchemaName, parameters.TableName, parameters.After_Lsn, afterSeqVal, afterCommandId, afterOperation, maxLsn, pageSize);
				ReportUsage(HttpStatusCode.OK, data.items.Length);
				return HttpResultFactory.GetResult(data, parameters.Format);
			}, parameters);
		}

		IHttpActionResult RunWithChecks(Func<IHttpActionResult> func, AuditApiParameters parameters)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (parameters is null)
				{
					ReportUsage(HttpStatusCode.BadRequest, 0);
					return HttpResultFactory.GetError($"Missing parameters. Please check documentation for correct usage.", FormatType.JSON);
				}

				// Check that the caller is allowed to access this api
				var businessAreaCheckPoint = Env.Security.AuditServices;
				if (!businessAreaCheckPoint.IsAllowed)
				{
					ReportUsage(HttpStatusCode.BadRequest, 0);
					return HttpResultFactory.GetError(businessAreaCheckPoint.ErrorMessageForNotAllowed, parameters.Format);
				}

				// Check that the api is enabled
				var apiEnabled = SystemDataRegistry.Instance.BiAuditAPI.Value;
				if (!apiEnabled)
				{
					ReportUsage(HttpStatusCode.BadRequest, 0);
					return HttpResultFactory.GetError($"The Audit Web API has not been enabled on this system.", parameters.Format);
				}

				// Check that the paremeters provided were parsed correctly
				if (!ModelState.IsValid)
				{
					ReportUsage(HttpStatusCode.BadRequest, 0);
					var errors = ModelState.Values.SelectMany(m => m.Errors.Select(e => e.ErrorMessage));
					return HttpResultFactory.GetErrors(errors.Select(err => $"Parameter is invalid: {err}"), parameters.Format);
				}

				try
				{
					return func();
				}
				catch (AuditAPIException ex)
				{
					ReportUsage(HttpStatusCode.BadRequest, 0);
					return HttpResultFactory.GetError(ex.Message, parameters.Format);
				}
			}
		}

		void ReportUsage(HttpStatusCode statusCode, int rowCount)
		{
			var endpoint = Request?.RequestUri?.AbsolutePath ?? "";
			var parameters = string.Join(", ", Request?.GetQueryNameValuePairs().Select(kvp => $"{kvp.Key}: {kvp.Value}"));
			var statusString = statusCode.ToString();
			var databaseName = Db.AuditDatabaseName;
			var count = rowCount.ToString();
			var timestamp = ZDateTime.UtcNow.ToBestReadableDateTimeString();

			ReportsService.ReportUsage(endpoint, parameters, statusString, databaseName, count, timestamp);
		}
	}
}
