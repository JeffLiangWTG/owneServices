#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mime;
using System.Text;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Enterprise.Services.ServiceHost.NetCore
{
	[GlowTicketAuthentication]
	[Route("api/Dummy")]
	public class DummyController : ControllerBase
	{
		[HttpGet]
		public IActionResult Get()
		{
			return Ok((NoResString)"DummyController - Get");
		}

		[Route("GetFullTestName/{firstName}/{lastName}")]
		[HttpGet]
		public IActionResult GetWithRouteParams(string firstName, [FromRoute] string lastName)
		{
			return Ok($"FirstName : {firstName} LastName : {lastName}");
		}

		[Route("GetFullTestName/{firstName}")]
		[HttpGet]
		public IActionResult GetWithQueryParams(string firstName, [FromQuery] string lastName)
		{
			return Ok($"FirstName : {firstName} LastName : {lastName}");
		}

		[Route("BadRequest")]
		[HttpGet]
		public IActionResult BadRequestSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Bad Request Response.",
				RequestID = Guid.NewGuid()
			};
			return BadRequest(response);
		}

		[Route("BadRequestWithStringResponse")]
		[HttpGet]
		public IActionResult BadRequestWithStringResponse()
		{
			var response = (NoResString)"Bad Request";
			return this.BadRequest(message: response);
		}

		[Route("BadRequestWithModelState")]
		[HttpPost]
		public IActionResult BadRequestWithStringResponse([FromBody] DummyModelState state)
		{
			if (!ModelState.IsValid)
			{
				return this.BadRequest(ModelState, nameof(state));
			}
			return this.BadRequest(message: (NoResString)"The model state should have been false");
		}

		[Route("ForbiddenNoContent")]
		[HttpGet]

		public IActionResult ForbiddenNoContent()
		{
			return StatusCode((int)HttpStatusCode.Forbidden);
		}

		[Route("Forbidden")]
		[HttpGet]

		public IActionResult ForbiddenSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Forbidden Response.",
				RequestID = Guid.NewGuid()
			};
			return this.Forbidden(response);
		}

		[Route("Conflict")]
		[HttpGet]
		public IActionResult ConflictSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Conflict Response.",
				RequestID = Guid.NewGuid()
			};
			return Conflict(response);
		}

		[Route("UnprocessableEntity")]
		[HttpGet]
		public IActionResult UnprocessableEntitySample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Unprocessable Entity Response.",
				RequestID = Guid.NewGuid()
			};
			return UnprocessableEntity(response);
		}

		[Route("InternalServerError")]
		[HttpGet]
		public IActionResult InternalServerErrorSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Internal Server Error Response.",
				RequestID = Guid.NewGuid()
			};
			return this.InternalServerError(response);
		}

		[Route("InternalServerErrorNegotiateContentResult")]
		[HttpGet]
		public IActionResult InternalServerErrorNegotiateContentResult()
		{
			return this.InternalServerError(Res.GetString("3E8008E8-AE3C-4ED8-9D01-B906E49F51A9", "Unexpected error happened: {0}", new Exception("This is InternalServerErrorNegotiateContentResult")));
		}

		[Route("InternalServerErrorResponseMessage")]
		[HttpGet]
		public IActionResult InternalServerErrorResponseMessage()
		{
			var message = $@"<center><p><strong><span class=""g-header1"">";
			return this.ReturnContentAsText(message, HttpStatusCode.InternalServerError, Encoding.UTF8, "text/html");
		}

		[Route("InternalServerErrorNoContent")]
		[HttpGet]
		public IActionResult InternalServerErrorNoContent()
		{
			return StatusCode((int)HttpStatusCode.InternalServerError);
		}

		[Route("InternalServerErrorWithException")]
		[HttpGet]
		public IActionResult InternalServerErrorWithExceptionSample()
		{
			Exception exception = new Exception("Sample Internal Server exception");
			return this.InternalServerError(exception);
		}

		[Route("plain/BadRequest")]
		[HttpGet]
		public IActionResult PlainTextBadRequestSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Bad Request Response.",
				RequestID = Guid.NewGuid()
			};
			return this.ReturnContentAsText(response, HttpStatusCode.BadRequest, Encoding.UTF8, MediaTypeNames.Text.Plain);
		}

		[Route("plain/ForbiddenObject")]
		[HttpGet]
		public IActionResult PlainTextForbiddenObjectSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Forbidden Response.",
				RequestID = Guid.NewGuid()
			};
			return this.ReturnContentAsText(response, HttpStatusCode.Forbidden, Encoding.UTF8, MediaTypeNames.Text.Plain);
		}

		[Route("plain/Forbidden")]
		[HttpGet]
		public IActionResult PlainTextForbiddenSample()
		{
			string response = (NoResString)"Forbidden String Sample.";
			return this.ReturnContentAsText(response, HttpStatusCode.Forbidden, Encoding.UTF8, MediaTypeNames.Text.Plain);
		}

		[Route("plain/Conflict")]
		[HttpGet]
		public IActionResult PlainTextConflictSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Conflict Response.",
				RequestID = Guid.NewGuid()
			};
			return this.ReturnContentAsText(response, HttpStatusCode.Conflict, Encoding.UTF8, MediaTypeNames.Text.Plain);
		}

		[Route("plain/UnprocessableEntity")]
		[HttpGet]
		public IActionResult PlainTextUnprocessableEntitySample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Unprocessable Entity Response.",
				RequestID = Guid.NewGuid()
			};
			return this.ReturnContentAsText(response, (HttpStatusCode)422, Encoding.UTF8, MediaTypeNames.Text.Plain);
		}

		[Route("plain/InternalServerError")]
		[HttpGet]
		public IActionResult PlainTextInternalServerErrorSample()
		{
			var response = new DummyPoco
			{
				Response = (NoResString)"Sample Internal Server Error Response.",
				RequestID = Guid.NewGuid()
			};
			return this.ReturnContentAsText(response, HttpStatusCode.InternalServerError, Encoding.UTF8, MediaTypeNames.Text.Plain);
		}
	}

	public class DummyPoco
	{
		public string Response { get; set; }
		public Guid RequestID { get; set; } = Guid.NewGuid();
	}

	public class DummyModelState
	{
		[Required]
		public string RequestID { get; set; }

		[Range(1, 100)]
		public int Age { get; set; }

		[Required]
		public DummyModelChild DummyModelChild { get; set; } = new DummyModelChild();

		[Required]
		public List<DummyModelChild> SubClassCollection { get; set; }
	}

	public class DummyModelChild
	{
		[Required]
		public string ChildName { get; set; }
	}
}
