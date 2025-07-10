using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DocumentScanning.Integration;
using Enterprise.DocumentScanning.Web;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("api/shipamaxdocs")]
	[GlowTicketAuthentication]
	public class EDocsShipamaxController : ApiController
	{
		public EDocsShipamaxController()
			: this(new ShipamaxService())
		{
		}

		public EDocsShipamaxController(IShipamaxService service)
		{
			this.service = service;
		}

		readonly IShipamaxService service;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		const string DocTokenMissing = "The eDoc authorization token is missing.";

		#region Web API endpoints

		[Route("{docPK}/parsed-results")]
		[HttpPut]
		public IHttpActionResult UpdateParseResult(Guid docPK, [FromBody]ShipamaxParseResult parseResult)
		{
			if (!ModelState.IsValid)
			{
				var response = Request.CreateResponse((HttpStatusCode)422, (NoResString)"The request is not in the expected format.");

				return ResponseMessage(response);
			}

			if (!TryGetDocTokenFromHeader(out var docToken))
			{
				return Content(HttpStatusCode.Forbidden, DocTokenMissing);
			}
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					service.SaveParseResult(docPK, docToken, parseResult);
				}
				catch (ShipamaxServiceException ex)
				{
					return GetErrorMessage(ex);
				}
			}

			return Ok((NoResString)"Parse result has been saved successfully.");
		}

		[Route("{docPK}/validate")]
		[HttpGet]
		public IHttpActionResult ValidateDocument(Guid docPK)
		{
			if (!ModelState.IsValid)
			{
				var response = Request.CreateResponse((HttpStatusCode)422, (NoResString)"The request is not in the expected format.");

				return ResponseMessage(response);
			}

			if (!TryGetDocTokenFromHeader(out var docToken))
			{
				return Content(HttpStatusCode.Forbidden, DocTokenMissing);
			}

			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					var docChanges = service.CheckEDocsChanges(docPK, docToken).ToArray();
					return docChanges.Length == 0 ? Ok() : ResponseMessage(Request.CreateResponse((HttpStatusCode)422, JsonConvert.SerializeObject(docChanges)));
				}
				catch (ShipamaxServiceException ex)
				{
					return GetErrorMessage(ex);
				}
			}
		}

		#endregion

		#region Implementation

		bool TryGetDocTokenFromHeader(out string token)
		{
			token = string.Empty;

			if (!Request.Headers.TryGetValues("Doc-Token", out var values))
			{
				return false;
			}

			token = values.FirstOrDefault();

			return !string.IsNullOrEmpty(token);
		}

		internal IHttpActionResult GetErrorMessage(ShipamaxServiceException ex)
		{
			HttpStatusCode httpStatusCode;

			switch (ex.ErrorType)
			{
				case ShipamaxServiceErrorType.NotFoundError:
					httpStatusCode = HttpStatusCode.NotFound;
					break;
				case ShipamaxServiceErrorType.ValidationError:
					httpStatusCode = HttpStatusCode.Forbidden;
					break;
				case ShipamaxServiceErrorType.StatusError:
				case ShipamaxServiceErrorType.ConfigurationError:
					httpStatusCode = HttpStatusCode.BadRequest;
					break;
				default:
					httpStatusCode = HttpStatusCode.InternalServerError;
					ErrorReporter.ReportOnce("EDocsShipamaxController_ExceptionHandler", $"{nameof(EDocsController)} request URI: {Request.RequestUri}", ex);
					break;
			}

			var responseMessage = Request.CreateResponse(httpStatusCode, ex.Error);

			return ResponseMessage(responseMessage);
		}

		#endregion
	}
}
