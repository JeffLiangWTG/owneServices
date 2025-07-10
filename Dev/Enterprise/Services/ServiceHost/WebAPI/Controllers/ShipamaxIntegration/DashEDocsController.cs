using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using CargoWise.Types;
using Enterprise.Dash.Business.Services;
using Enterprise.Dash.Integration;
using Enterprise.Dash.Integration.Services;
using Enterprise.DocumentScanning.Web;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/dash/edocs")]
	[GlowTicketAuthentication]
	public class DashEDocsController : ApiController
	{
		readonly IDashEDocsService dashEDocsService;

		public DashEDocsController() : this(new DashEDocsService(new ShipamaxService()))
		{
		}

		public DashEDocsController(IDashEDocsService dashEDocsService)
		{
			this.dashEDocsService = dashEDocsService;
		}

		[HttpGet]
		[Route("details")]
		public IHttpActionResult GetDashEDocsDetails(string docMainId, string docId, string docToken)
		{
			var validationErrors = Validate(docMainId, docId, docToken);

			if (!string.IsNullOrWhiteSpace(validationErrors))
			{
				return BadRequest(validationErrors);
			}

			var docPkGuid = new ZGuid(Guid.Parse(docId));
			var docMainPkGuid = string.IsNullOrWhiteSpace(docMainId) ? ZGuid.Empty : new ZGuid(Guid.Parse(docMainId));

			try
			{
				var edocDetails = dashEDocsService.GetDashEDocsDetails(docMainPkGuid, docPkGuid, docToken);
				return Ok(edocDetails);
			}
			catch (DashException ex)
			{
				return new ResponseMessageResult(
						Request.CreateErrorResponse(
							HttpStatusCode.NotFound,
							new HttpError(ex.Message)));
			}
		}

		string Validate(string docMainId, string docId, string docToken)
		{
			if (string.IsNullOrWhiteSpace(docId))
			{
				return (NoResString)$"Parameter '{nameof(docId)}' must have a value";
			}

			if (!Guid.TryParse(docId, out var _))
			{
				return (NoResString)$"Parameter '{nameof(docId)}' must be a valid GUID: {docId}";
			}

			if (!string.IsNullOrWhiteSpace(docMainId) && !Guid.TryParse(docMainId, out var _))
			{
				return (NoResString)$"Parameter '{nameof(docMainId)}' must be a valid GUID: {docMainId}";
			}

			if (string.IsNullOrWhiteSpace(docToken))
			{
				return (NoResString)$"Parameter '{nameof(docToken)}' must have a value";
			}

			return string.Empty;
		}
	}
}
