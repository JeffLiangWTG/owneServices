using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Bi.Registration.Common;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	[GlowTicketAuthentication]
	public class BiController : BiWebServicesController
	{
		public BiController() : base()
		{
		}

		[Route("api/analytics/reports/{*tabularmodel}")]
		[HttpGet]
		public IHttpActionResult GetPowerBiReportsList(string tabularModel)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (isPowerBiRegistrySet)
				{
					return Json(ReportService.GetPowerBiReportsList(LinkBuilderFactory, BiReportCategories.GetBiReportCategory(tabularModel)));
				}
				else
				{
					return BadRequestMessage();
				}
			}
		}

		[Route("api/analytics/baseReportPath/{reportType}")]
		[HttpGet]
		public IHttpActionResult GetPowerBiBaseReportPath(string reportType)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (isPowerBiRegistrySet)
				{
					return Json(ReportService.GetPowerBiBaseReportPath(reportType));
				}
				else
				{
					return BadRequestMessage();
				}
			}
		}

		[Route("api/analytics/datasets/ShipmentProfile/{format}/{shipmentNo}")]
		[HttpGet]
		public IHttpActionResult GetShipmentProfileReportData(string format, string shipmentNo)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var businessAreaCheckPoint = Env.Security.AnalysisServices;
				if (!businessAreaCheckPoint.IsAllowed)
				{
					return BadAPIRequestMessage(businessAreaCheckPoint.ErrorMessageForNotAllowed);
				}

				if (isValidResponseFormatRequested(format))
				{
					var data = GetShipmentProfieReportData(shipmentNo);
					if (data.Columns.Count == 0)
					{
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
				return BadResponseFormat;
			}
		}

		[Route("api/analytics/datasets/TrialBalance/{format}/{code}")]
		[HttpGet]
		public IHttpActionResult GetTrialBalanceReportData(string format, string code)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var businessAreaCheckPoint = Env.Security.AnalysisServices;
				if (!businessAreaCheckPoint.IsAllowed)
				{
					return BadAPIRequestMessage(businessAreaCheckPoint.ErrorMessageForNotAllowed);
				}

				if (isValidResponseFormatRequested(format))
				{
					var data = GetTrialBalanceReportData(code);
					if (data.Columns.Count == 0)
					{
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
				return BadResponseFormat;
			}
		}

		[ThreadSafe]
		public HttpClientHandler httpClientHandler { get; private set; }

		[Route("api/analytics/loadReport/{*path}")]
		[HttpPost]
		public async Task<HttpResponseMessage> GetPowerBiReportData(string path)
		{
			HttpResponseMessage serverResponse = null;
			path += ReportService.GetQueryString(Request.RequestUri.PathAndQuery);

			using (httpClientHandler = new ImpersonatedWebRequestHandler(new BiReportUser()))
			using (var proxyRequest = ReportService.CloneRequest(Request, new Uri(ReportService.PowerBiServerName + path)))
			using (var client = new HttpClient(httpClientHandler))
			{
				try
				{
					httpClientHandler.UseDefaultCredentials = false;
					serverResponse = await client.SendAsync(proxyRequest).ConfigureAwait(false);
				}
				catch (HttpRequestException ex)
				{
					return await CannotConnectToReportServerMessage(ex.Message);
				}
				return serverResponse;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "A query string param")]
		const string SpoofFilterParam = "filter";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "A query string param")]
		const string SafeFilterParam = "$filter";
		public string LastPowerBiReportPath;

		[Route("api/analytics/loadReport/{*path}")]
		[HttpGet]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "default report type, A URL string, A query string param")]
		public async Task<HttpResponseMessage> GetPowerBiReport(string path
#if DEBUG
			, bool spoofingTest = false
#endif
			)
		{
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var currentUser = Env.CurrentUserContext.User as MasterFiles.Business.GlbStaff;
				var code = currentUser.HomeBranch.Company.GC_Code;
				var country = currentUser.HomeBranch.Company.Country.Code;
				var branch = currentUser.HomeBranch.GB_Code;
				HttpResponseMessage serverResponse = null;

				using (httpClientHandler = new ImpersonatedWebRequestHandler(new BiReportUser()))
				using (var client = new HttpClient(httpClientHandler))
				{
					if (SupportLegacyReportServer && ReportDeloyer.IsLegacyPowerBiServer)
					{
						httpClientHandler.UseDefaultCredentials = false;
					}
					else
					{
						httpClientHandler.UseDefaultCredentials = true;
					}

					LastPowerBiReportPath = EditPowerBiReportPathString(path);
#if DEBUG
					if (spoofingTest)
					{
						return null;
					}
#endif

					var reportName = path.Split('/').Last();
					var reportServerName = path.Split('/').First();
					var reportItem = ReportService.GetPowerBiItemByName(reportName);
					var reportType = "powerbi";
					var fileName = LastPowerBiReportPath.Split('/').Last();
					bool hasValidCatalogItem = false;

					//if (reportItem != null && !ReportService.IsUserAuthorisedToViewReport(LastPowerBiReportPath, reportItem, out var errorMessageForNotAllowed))
					//{
					//	return await BadRequest(errorMessageForNotAllowed).ExecuteAsync(new CancellationToken(false));
					//}

					if (reportItem != null && !ReportDeloyer.IsLegacyPowerBiServer)
					{
						var ard = new AnalyticsReportDeployer(null);
						var catalogItems = ard.ListChildCatalogItems(ard.ClientFolderPath, true);
						var catalogItem = catalogItems.Where(x => x.Name == reportItem.Name).First();
						hasValidCatalogItem = true;
						reportType = reportItem.ResourceType;

						if (reportType == "report")
						{
							const string ssrsViewerString = "ReportServer/Pages/ReportViewer.aspx?";
							const string rsEmbedString = "?rs:Embed=true";
							string reportString = reportServerName + "/report";
							LastPowerBiReportPath = LastPowerBiReportPath.Replace(reportString, ssrsViewerString).Replace(rsEmbedString, String.Empty);
							httpClientHandler.UseDefaultCredentials = false;
							fileName = ssrsViewerString;
						}
						else // Power BI report item
						{
							reportItem.Id = catalogItem.Id;
							const string idString = "?id=";
							fileName = idString + catalogItem.Id;
						}
					}

					const string powerbiString = "powerbi";
					using (var proxyRequest = hasValidCatalogItem && reportType == powerbiString ? ReportService.CloneRequest(Request, new Uri(ReportService.PowerBiServerName + reportType + fileName)) : ReportService.CloneRequest(Request, new Uri(ReportService.PowerBiServerName + LastPowerBiReportPath)))
					{
						try
						{
							serverResponse = await client.SendAsync(proxyRequest).ConfigureAwait(false);
						}
						catch (HttpRequestException ex)
						{
							var errorResponse = await CannotConnectToReportServerMessage(ex.Message);
							errorResponse.RequestMessage = proxyRequest;
							return errorResponse;
						}
					}

					if (serverResponse.StatusCode == HttpStatusCode.Unauthorized)
					{
						return await ReportService.ShowUnauthorisedUserMessage();
					}

					return await ReportService.ModifyProxyResponse(reportItem, fileName, serverResponse, country, code, branch);
				}
			}
		}

		[Route("api/analytics/loadTrialBalanceReportFilteredData/{code}")]
		[HttpGet]
		public IHttpActionResult GetTrialBalanceReportFilteredData(string code)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var businessAreaCheckPoint = Env.Security.AnalysisServices;
				if (!businessAreaCheckPoint.IsAllowed)
				{
					return BadAPIRequestMessage(businessAreaCheckPoint.ErrorMessageForNotAllowed);
				}

				var data = GetTrialBalanceReportFilteredDataCore(code);
				if (data.Columns.Count == 0)
				{
					return BadAPIRequestMessage(TableNotFound);
				}
				return new JsonActionResult(HttpStatusCode.OK, data);
			}
		}

		string EditPowerBiReportPathString(string path)
		{
			path += ReportService.GetQueryString(Request.RequestUri.PathAndQuery);

			if (!path.ToLower().Contains(SafeFilterParam) && path.ToLower().Contains(SpoofFilterParam))
			{
				path = RemoveQueryStringParamByKey(path);
			}

			return path;
		}

		protected virtual DataTable GetShipmentProfieReportData(string shipmentNo)
		{
			var currentUser = EnvProxy.Instance.CurrentUser as MasterFiles.Business.GlbStaff;
			var code = currentUser.HomeBranch.Company.GC_Code;
			var country = currentUser.HomeBranch.Company.Country.Code;
			return ReportService.GetShipmentProfileReportData<AnalyticsModelHelper>(shipmentNo, code, country);
		}

		protected virtual DataTable GetTrialBalanceReportFilteredDataCore(string code)
		{
			return ReportService.GetTrialBalanceReportFilteredData<AnalyticsModelHelper>(code);
		}

		protected virtual DataTable GetTrialBalanceReportData(string code)
		{
			return ReportService.GetTrialBalanceReportData<AnalyticsModelHelper>(code);
		}

		[Route("api/analytics/getCompanyLogo")]
		[HttpGet]
		public IHttpActionResult GetCompanyLogo()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var base64Image = ReportService.GetCompanyLogoFromRegistry();
				return Json(base64Image);
			}
		}

		[Route("api/analytics/ReportConfigurations")]
		[HttpGet]
		public IHttpActionResult GetReportConfigurations()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var reportConfig = ReportService.GetReportConfigurations(CompanyCode);
				return Json(reportConfig);
			}
		}

		[Route("api/analytics/ReportConfigurations")]
		[HttpPut]
		public IHttpActionResult AddReportConfigurations([FromBody] ConfigurationBody body)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (body == null || string.IsNullOrWhiteSpace(body.LastModifiedDateUTC) || string.IsNullOrWhiteSpace(body.ReportConfigurationID) || string.IsNullOrWhiteSpace(body.ConfigurationName) || string.IsNullOrWhiteSpace(body.ColumnConfiguration))
				{
					return BadRequest(Res.GetString("ccb57545-57e8-4a54-a3d9-5d4bed74c10c", "All arguments must be provided."));
				}
				try
				{
					ReportService.AddReportConfigurations(body.ReportConfigurationID, CompanyCode, body.ConfigurationName, body.ColumnConfiguration, body.IsDefault, body.LastModifiedDateUTC);
				}
				catch (PowerBiException ex)
				{
					return BadRequest(ex.Message);
				}
				return Ok();
			}
		}

		[Route("api/analytics/ReportConfigurations")]
		[HttpPost]
		public IHttpActionResult UpdateReportConfigurations([FromBody] ConfigurationBody body)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (body == null || body.NewModifiedDateUTC == null || body.LastModifiedDateUTC == null || string.IsNullOrWhiteSpace(body.ReportConfigurationID) || string.IsNullOrWhiteSpace(body.ConfigurationName) || string.IsNullOrWhiteSpace(body.ColumnConfiguration))
				{
					return BadRequest(Res.GetString("ccb57545-57e8-4a54-a3d9-5d4bed74c10c", "All arguments must be provided."));
				}
				try
				{
					ReportService.UpdateReportConfigurations(body.ReportConfigurationID, CompanyCode, body.ConfigurationName, body.ColumnConfiguration, body.IsDefault, body.LastModifiedDateUTC, body.NewModifiedDateUTC);
				}
				catch (PowerBiException ex)
				{
					return BadRequest(ex.Message);
				}
				return Ok();
			}
		}

		[Route("api/analytics/ReportConfigurations/{*configurationId}")]
		[HttpDelete]
		public IHttpActionResult DeleteReportConfigurations(string configurationId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (string.IsNullOrWhiteSpace(configurationId))
				{
					return BadRequest(Res.GetString("ccb57545-57e8-4a54-a3d9-5d4bed74c10c", "All arguments must be provided."));
				}
				try
				{
					ReportService.DeleteReportConfigurations(configurationId);
				}
				catch (PowerBiException ex)
				{
					return BadRequest(ex.Message);
				}
				return Ok();
			}
		}

		[Route("api/analytics/userContextInformation")]
		[HttpGet]
		public IHttpActionResult GetUserContextInformation()
		{
			var userContextInformation = JsonConvert.DeserializeObject(ReportService.GetUserContextInformationFromEnvProxy());
			return Json(userContextInformation);
		}

		[Route("api/analytics/lastModifiedDate/{*businessArea}")]
		[HttpGet]
		public IHttpActionResult GetLastModifiedDate(string businessArea)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					return Json(ReportService.GetLastModifiedDateFromDB(GetTabularModelName(businessArea)));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return BadRequest(ExceptionBuilder(ex));
				}
			}
		}

		string ExceptionBuilder(Exception ex)
		{
			return $"Type: {ex.GetType().ToString()} \n Message: {ex.Message} \n StackTrace: {ex.StackTrace}"; // Error Message
		}

		protected virtual string GetTabularModelName(string businessArea)
		{
			return $"{Db.DatabaseName}_{businessArea}Model"; // Tabular model name
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "A query string param")]
		string RemoveQueryStringParamByKey(string path)
		{
			const string FIRST_PARAM_TOKEN = "?";
			const string FILTER = "filter";
			var splitToken = new char[] { '&' };
			var isFirstParam = true;

			if (!path.Contains(FIRST_PARAM_TOKEN))
			{
				return path;
			}

			var url = path.Substring(0, path.IndexOf(FIRST_PARAM_TOKEN, StringComparison.OrdinalIgnoreCase));
			var query = path.Substring(path.IndexOf(FIRST_PARAM_TOKEN, StringComparison.OrdinalIgnoreCase) + 1, path.Length - url.Length - 1);

			var queryParams = query.Split(splitToken);
			StringBuilder sb = new StringBuilder(url);
			foreach (var item in queryParams)
			{
				if (item.StartsWith(FILTER, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				if (isFirstParam)
				{
					sb.Append(FIRST_PARAM_TOKEN);
					isFirstParam = false;
				}
				else
				{
					sb.Append(splitToken);
				}
				sb.Append(item);
			}
			return sb.ToString();
		}

		public AnalyticsReportDeployer ReportDeloyer
		{
			get
			{
				if (reportDeloyer == null)
				{
					reportDeloyer = new AnalyticsReportDeployer(null);
				}

				return reportDeloyer;
			}
		}
		AnalyticsReportDeployer reportDeloyer { get; set; }
	}
}
