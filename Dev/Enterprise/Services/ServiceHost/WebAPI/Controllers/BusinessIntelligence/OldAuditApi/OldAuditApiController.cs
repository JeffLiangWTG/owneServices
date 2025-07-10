using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.OldAuditApi
{
	[Authorize]
	[CW1IdentityBasicAuthentication]
	public class OldAuditApiController : BiWebServicesController
	{
		public OldAuditApiController() : base()
		{
		}

		[Route("api/analytics/audit-data-summary")]
		[HttpGet]
		public IHttpActionResult GetSummaryDataQuery()
		{
			var queryNameValuePairs = Request?.GetQueryNameValuePairs();
			string format = GetQueryParameterValue(queryNameValuePairs, (NoResString)"response_format") ?? (NoResString)"";
			string fromDate = GetQueryParameterValue(queryNameValuePairs, (NoResString)"from_time");
			string toDate = GetQueryParameterValue(queryNameValuePairs, (NoResString)"to_time");
			string lsn = GetQueryParameterValue(queryNameValuePairs, (NoResString)"after_lsn");
			int batchSize = int.Parse(GetQueryParameterValue(queryNameValuePairs, (NoResString)"page_size") ?? (NoResString)"0");
			return GetSummaryData(format, fromDate, toDate, batchSize, lsn);
		}

		[Route("api/analytics/audit-data")]
		[HttpGet]
		public IHttpActionResult GetAuditDataQuery()
		{
			var queryNameValuePairs = Request?.GetQueryNameValuePairs();
			string format = GetQueryParameterValue(queryNameValuePairs, (NoResString)"response_format") ?? (NoResString)"";
			string schemaName = GetQueryParameterValue(queryNameValuePairs, (NoResString)"schema");
			string tableName = GetQueryParameterValue(queryNameValuePairs, (NoResString)"table");
			string lsn = GetQueryParameterValue(queryNameValuePairs, (NoResString)"lsn");
			int pageNo = int.Parse(GetQueryParameterValue(queryNameValuePairs, (NoResString)"page") ?? (NoResString)"1");
			int pageSize = int.Parse(GetQueryParameterValue(queryNameValuePairs, (NoResString)"page_size") ?? (NoResString)"0");
			return GetAuditData(schemaName, tableName, pageNo, pageSize, format, lsn);
		}
		static string GetQueryParameterValue(IEnumerable<KeyValuePair<string, string>> queryNameValuePairs, string name)
		{
			return queryNameValuePairs.Where(query => query.Key.Equals(name)).Select(query => query.Value).FirstOrDefault();
		}
#if DEBUG
		public virtual
#else
		public
#endif
			IHttpActionResult GetSummaryData(string format, string fromDateTime, string toDateTime, int batchSize, string lsn)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var businessAreaCheckPoint = Env.Security.AuditServices;
				if (!businessAreaCheckPoint.IsAllowed)
				{
					return BadAPIRequestMessage(businessAreaCheckPoint.ErrorMessageForNotAllowed);
				}
			}

			if (isValidResponseFormatRequested(format))
			{
				DateTime dtFromDate;
				DateTime dtToDate;
				System.Data.DataTable data;

				try
				{
					if (fromDateTime != null && toDateTime != null && lsn == null)
					{
						dtFromDate = APIDateStringToDate(fromDateTime);
						dtToDate = APIDateStringToDate(toDateTime);
#pragma warning disable CS0618 // Type or member is obsolete
						data = ReportService.RetrieveCdcHistorySummaryDetails(dtFromDate, dtToDate, batchSize);
#pragma warning restore CS0618
					}
					else if (fromDateTime == null && toDateTime == null && lsn != null)
					{
#pragma warning disable CS0618 // Type or member is obsolete
						data = ReportService.RetrieveCdcHistorySummaryDetailsAfterLsn(lsn, batchSize);
#pragma warning restore CS0618
					}
					else
					{
						return BadAPIRequestMessage(InvalidAuditSummaryQuery);
					}
					if (format.Equals(BIAPIServiceConstants.JSON))
					{
						return new JsonActionResult(HttpStatusCode.OK, data);
					}
					else
					{
						return new CsvActionResult(HttpStatusCode.OK, data);
					}
				}
				catch (FormatException e)
				{
					return BadAPIRequestMessage(e.Message);
				}
				catch (AuditAPIException e)
				{
					return BadAPIRequestMessage(e.Message);
				}
			}
			return BadResponseFormat;
		}

		public DateTime APIDateStringToDate(string dateString)
		{
			DateTime result;
			if (DateTime.TryParseExact(dateString, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
			{
				return result;
			}
			else
			{
				throw new FormatException(Res.GetString("c23cfee3-da4e-451b-81da-ef9aa2698730", "DateTime string is not in the correct format: yyyyMMddHHmm"));
			}
		}

#if DEBUG
		public virtual
#else
		public
#endif
			IHttpActionResult GetAuditData(string schemaName, string tableName, int pageNo, int pageSize, string format, string lsn)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var businessAreaCheckPoint = Env.Security.AuditServices;
				if (!businessAreaCheckPoint.IsAllowed)
				{
					return BadAPIRequestMessage(businessAreaCheckPoint.ErrorMessageForNotAllowed);
				}
			}

			if (isValidResponseFormatRequested(format))
			{
				try
				{
					if (!ValidateLsnFormat(lsn))
					{
						return BadAPIRequestMessage(Res.GetString("0C59FFC6-0E43-494F-AE2F-D95D0EC29D70", "LSN string is not in the correct format."));
					}

#pragma warning disable CS0618 // Type or member is obsolete
					var data = ReportService.GetCdcChanges(lsn, pageNo, schemaName, tableName, pageSize);
#pragma warning restore CS0618 // Type or member is obsolete

					if (data.Columns.Count == 0)
					{
						if (pageNo < 1)
						{
							return BadAPIRequestMessage(InvalidBatch);
						}
						return BadAPIRequestMessage(TableNotFound);
					}
					if (format.Equals(BIAPIServiceConstants.JSON))
					{
						return new JsonActionResult(HttpStatusCode.OK, data);
					}
					else
					{
						return new CsvActionResult(HttpStatusCode.OK, data);
					}
				}
				catch (AuditAPIException e)
				{
					return BadAPIRequestMessage(e.Message);
				}
			}
			return BadResponseFormat;
		}
	}
}
