using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	public class WebHyperlinkTrampolineController : ApiController
	{
		string GetLicenceCode(string company = null)
		{
			if (company == null)
			{
				return string.Empty;
			}

			var factory = new BusinessObjectFactory();
			var companyBizO = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, company);
			if (companyBizO != null)
			{
				return companyBizO.LicenceKeyIdentifier;
			}

			return string.Empty;
		}

		[HttpGet]
		[Route("link/{userAction}/{cw1controller}/{pk}")]
		public IHttpActionResult GetTrampolineLandingPage(string userAction, string cw1controller, Guid pk, string company = null, string lang = null, string license = null)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var licenseCode = license ?? GetLicenceCode(company);

				switch (userAction)
				{
					case "ShowEditForm":
						var uri = ShowEditFormUrlHandler.Instance.CreateFromTrampolineData(cw1controller, pk, licenseCode);
						return RedirectCargoWiseOne(uri, lang);
				}

				return StatusCode(HttpStatusCode.NotImplemented);
			}
		}

		[HttpGet]
		[Route("TrampolineLandingPageStyle")]
		public HttpResponseMessage GetTrampolineLandingPageStyle()
		{
			return new HttpResponseMessage()
			{
				Content = new StringContent(ResourceHelpers.GetStringResource("WebAPI.Controllers.System.WebHyperlinkTrampolineLandingPage.css"), Encoding.UTF8, "text/css")
			};
		}

		[HttpGet]
		[Route("edit/{type}/{id}")]
		public IHttpActionResult GetTrampolineLandingPage(string type, string id, string company = null, string lang = null)
		{
			if (ClientHookLoader.Instance.Client != Clients.EDI)
			{
				return StatusCode(HttpStatusCode.NotFound);
			}

			using (Db.DisposableActionForDbConnection())
			{
				var licenceCode = GetLicenceCode(company);

				var queryString = new QueryString
				{
					{ (NoResString)"Command", "ShortCode" }
				};

				if (!string.IsNullOrEmpty(licenceCode))
				{
					queryString.Add("LicenceCode", licenceCode);
				}

				queryString.Add((NoResString)"Id", id);
				queryString.Add((NoResString)"Type", type);

				var uri = $"edient:{queryString}";

				return RedirectCargoWiseOne(uri, lang);
			}
		}

		IHttpActionResult RedirectCargoWiseOne(string cargoWiseOneUri, string language)
			=> new CargoWiseOneRedirectActionResult(cargoWiseOneUri, language, this);

		sealed class CargoWiseOneRedirectActionResult : IHttpActionResult
		{
			public CargoWiseOneRedirectActionResult(string uri, string language, ApiController controller)
			{
				this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
				this.language = language;
				this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
			}

			readonly string uri;
			readonly ApiController controller;
			readonly string language;

			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				using (Db.DisposableActionForDbConnection())
				{
					return Task.FromResult(Execute());
				}
			}

			HttpResponseMessage Execute()
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				response.RequestMessage = controller.Request;

				response.Content = new StringContent(GetSinglePageResponse(), Encoding.UTF8);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

				return response;
			}

			string GetSinglePageResponse()
			{
				var page = GetResponseTemplate();
				page = page.Replace("{{URI}}", uri);

				ConfigureBranding();

				var productName = BrandingFactory.Instance.ProductName;

				page = page.Replace("{{SITE_ROOT}}", controller.RequestContext.VirtualPathRoot);
				page = page.Replace("{{CW1_TITLE}}", productName);
				page = page.Replace("{{WAIT_MESSAGE}}", Format(ResString.GetMultilingualString("B49F1886-6DBB-4566-B0EB-9578B64C3F5E", "Please wait while {0} is launched...", productName)));
				page = page.Replace("{{MANUAL_MESSAGE}}", Format(ResString.GetMultilingualString("9C322DFD-8C00-48E1-A646-30481833B157", "If {0} does not start after a few seconds, click the button below to launch the loader again.", productName)));
				page = page.Replace("{{BUTTON_TEXT}}", Format(ResString.GetMultilingualString("34CB80F5-D22D-4040-A768-32C5FB89C969", "Open")));
				return page;
			}

			string GetResponseTemplate()
			{
				return ResourceHelpers.GetStringResource("WebAPI.Controllers.System.WebHyperlinkTrampolineLandingPage.html");
			}

			string Format(MultilingualString text)
			{
				string translatedText;

				if (string.IsNullOrEmpty(language))
				{
					translatedText = text.ToString(Res.DefaultLanguage);
				}
				else
				{
					translatedText = text.ToString(language);
				}

				// Escape any XML characters
				return new XText(translatedText).ToString(SaveOptions.DisableFormatting);
			}

			static void ConfigureBranding()
			{
				using (Db.DisposableActionForDbConnection())
				{
					if (DataRegistry.Instance.ProductivityWiseModeEnabled)
					{
						BrandingFactory.Configure(BrandingFactory.BrandingType.ProductivityWise);
					}
					else
					{
						BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseNext);
					}
				}
			}
		}
	}
}
