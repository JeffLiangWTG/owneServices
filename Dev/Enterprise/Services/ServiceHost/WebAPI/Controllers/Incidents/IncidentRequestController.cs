using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class IncidentRequestController : ApiController
	{
		public IncidentRequestController()
		{
		}

		[Route("api/incidents/productlist")]
		[HttpGet]
		[HttpPost]
		public IHttpActionResult GetProductList([FromUri]string language = "")
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contactPK = identity?.GetContactPK() ?? Guid.Empty;

				return Json(
					CreateService()
					.GetProductList(GetLanguageCode(language), contactPK)
					.Select(i => new CodeDescriptionPair { Code = i.Item1, Description = i.Item2 }));
			}
		}

		[Route("api/incidents/modulelist")]
		[HttpGet]
		[HttpPost]
		public IHttpActionResult GetModuleList([FromUri]string product, [FromUri]string criticality, [FromUri]string language = "", [FromUri] string status = "")
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contactPK = identity?.GetContactPK() ?? Guid.Empty;

				return Json(
					CreateService()
					.GetModuleList(product, criticality, GetLanguageCode(language), status, contactPK)
					.Select(i => new CodeDescriptionPair { Code = i.Item1, Description = i.Item2 }));
			}
		}

		[Route("api/incidents/fullproductlist")]
		[HttpGet]
		[HttpPost]
		public IHttpActionResult GetFullProductList()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(
					CreateService()
					.GetFullProductList()
					.Select(i => new CodeDescriptionPair { Code = i.Item1, Description = i.Item2 }));
			}
		}

		[Route("api/incidents/fullmodulelistbyproduct")]
		[HttpGet]
		[HttpPost]
		public IHttpActionResult GetFullModuleListByProduct()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(
					CreateService()
					.GetFullModuleListByProduct());
			}
		}

		[Route("api/incidents/servicetypelist")]
		[HttpGet]
		[HttpPost]
		public IHttpActionResult GetServiceTypeList([FromUri]string product, [FromUri]string criticality, [FromUri]string module, [FromUri]string sourceModuleId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(
					CreateService()
					.GetServiceTypeList(product, criticality, module, sourceModuleId)
					.Select(i => new CodeDescriptionPair { Code = i.Item1, Description = i.Item2 }));
			}
		}

		[Route("api/incidents/notification/unsubscribe")]
		[HttpGet]
		[HttpPost]
		public IHttpActionResult NotificationWhenUnsubscribe([FromUri] Guid jobPK, [FromUri] string userType, [FromUri] string email, [FromBody] ICollection<Guid> userPKs)
		{
			CreateService().NotificationWhenUnsubscribe(jobPK, userType, userPKs, email);
			return Ok();
		}

		[Route("api/incidents/documenturllist")]
		[HttpPost]
		public IHttpActionResult GetDocumentUrls([FromBody] ICollection<string> documentIds)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contactPK = identity?.GetContactPK() ?? Guid.Empty;

				return Json(
					CreateService()
					.GetDocumentUrls(contactPK, documentIds));
			}
		}

		IIncidentRequestService CreateService()
		{
			var type = IncidentRequestService.MyTypeDecider.GetTypeForCreate();
			return (IIncidentRequestService)Activator.CreateInstance(type);
		}

		string GetLanguageCode(string languageCodeFromUri)
		{
			if (!string.IsNullOrEmpty(languageCodeFromUri))
			{
				return languageCodeFromUri;
			}
			else if (Request != null && Request.Headers.TryGetValues(EnterpriseLanguageCodeHeaderKey, out var headerValues)
			   && !string.IsNullOrEmpty(headerValues.First()))
			{
				return headerValues.First();
			}

			return DefaultLanguageCode;
		}

		const string DefaultLanguageCode = "EN";
		const string EnterpriseLanguageCodeHeaderKey = "Enterprise-Language-Code";
	}
}
