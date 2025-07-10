using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Bi.Registration.Common;
using CargoWise.Data;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence
{
	public abstract class BiWebServicesController : ApiController
	{
		public BiWebServicesController()
		{
			using (Db.DisposableActionForDbConnection())
			{
				ReportService = new BiReportsService();
			}
		}

		[ThreadSafe]
		protected virtual IBiReportsService ReportService { get; }

		[ThreadSafe]
		public ImpersonatedWebRequestHandler ImpersonatedRequestHandler { get; protected set; }

		protected bool isPowerBiRegistrySet =>
				!new BiReportUser().IsNull
				&& !string.IsNullOrEmpty(SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value);

		protected bool SupportLegacyReportServer => SystemDataRegistry.Instance.SupportLegacyReportServer.Value;

		public IPowerBiReportLinkBuilderFactory LinkBuilderFactory { get; } = new GlowPowerBiReportLinkBuilder.Factory();

		public IHttpActionResult BadAPIRequestMessage(string message)
		{
			return BadRequest(Res.GetString("c08ff658-ce96-43a4-9db8-eadb7a2aa1b1", "Incorrect API Request: {0}", message));
		}

		public IHttpActionResult BadRequestMessage()
		{
			if (new BiReportUser().IsNull)
			{
				return BadRequestUserMustSetReportCredentialsIntheRegistry();
			}
			else if (string.IsNullOrEmpty(SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value))
			{
				return BadRequestUserMustSetPowerBiWebPortalInTheRegistry();
			}
			else
			{
				return BadRequest(Res.GetString("F2545ADE-2295-4742-9CC8-45E7A7CA7CD0", "A bad request occurred. The web service was unable to process your request."));
			}
		}

		IHttpActionResult BadRequestUserMustSetPowerBiWebPortalInTheRegistry()
		{
			return BadRequest(Res.GetString("DA729281-C751-44B1-AAD0-0B67E75507E1", "Please enter a valid value for the \"Power BI Web Portal URL\" registry item in the \"System > BI\" path."));
		}

		IHttpActionResult BadRequestUserMustSetReportCredentialsIntheRegistry()
		{
			return BadRequest(Res.GetString("0A54B6CB-2D7C-438F-B4D1-15F814B52E9E", "Please provide a valid value for the \"BI Report Credential\" registry item in the \"System > BI\" path to access Power BI Server."));
		}

		public bool isValidResponseFormatRequested(string responseFormat)
		{
			return (responseFormat.Equals(BIAPIServiceConstants.JSON) || responseFormat.Equals(BIAPIServiceConstants.CSV));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Used to determine the prefix of a string")]
		public bool ValidateLsnFormat(string lsn)
		{
			if (string.IsNullOrEmpty(lsn))
			{
				return true;
			}

			if (!lsn.StartsWith("0x"))
			{
				return false;
			}

			lsn = lsn.Replace("0x", "");
			if (lsn.Length > 20)
			{
				return false;
			}

			return Regex.IsMatch(lsn, "^[0-9A-Fa-f]+$");
		}

		[ThreadSafe]
		public IHttpActionResult BadResponseFormat
		{
			get
			{
				return BadAPIRequestMessage(Res.GetString("03a3c499-3446-48c5-8d45-a8c50ac7eb9d", "You must specify the desired response format to be JSON or CSV"));
			}
		}

		[ThreadSafe]
		public string TableNotFound
		{
			get
			{
				return Res.GetString("9e8ca994-a85d-49ca-82d6-8e2343281be2", "Table not found");
			}
		}

		[ThreadSafe]
		public string InvalidBatch
		{
			get
			{
				return Res.GetString("acb191ca-9c56-41cd-b5e6-75d8949510bc", "Batch number must start from zero");
			}
		}

		[ThreadSafe]
		public string InvalidAuditSummaryQuery
		{
			get
			{
				return Res.GetString("dea37ecd-7828-4474-ac23-3a0092138cfe", "Either from_time and to_time is required or after_LSN is required");
			}
		}

		[ThreadSafe]
		public string CompanyCode
		{
			get
			{
				return companyCode ??
					(companyCode = ((EnvProxy.Instance.CurrentUser as MasterFiles.Business.GlbStaff)?.HomeBranch?.Company.GC_Code).ToString());
			}
		}
		string companyCode;

		[ThreadSafe]
		public string CountryCode
		{
			get
			{
				return countryCode ??
					(countryCode = ((EnvProxy.Instance.CurrentUser as MasterFiles.Business.GlbStaff)?.HomeBranch?.Company.Country).ToString());
			}
		}
		string countryCode;

		protected async Task<HttpResponseMessage> CannotConnectToReportServerMessage(string message)
		{
			var unAthorisedMessage = new HttpResponseMessage();
			var reportServerErrorMessage = Res.GetString("27c16053-6bfc-4f39-9583-25e67bcee69d", "The Report server can't be reached.");
			unAthorisedMessage.Content = new StringContent(string.Format("{0}{1} {2}{3}", "<html><body>", reportServerErrorMessage, message, (NoResString)"</body></html>"));  // html content
			unAthorisedMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			using (var byteStream = await unAthorisedMessage.Content.ReadAsStreamAsync())
			{ }
			return unAthorisedMessage;
		}
	}
}
