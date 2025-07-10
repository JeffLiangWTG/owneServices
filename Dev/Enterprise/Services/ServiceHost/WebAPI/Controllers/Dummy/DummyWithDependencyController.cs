#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
#elif NET
using System.Net.Mime;
#endif
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Enterprise.Services.ServiceHost.NetCore
{
	[GlowTicketAuthentication]
	[Route($"api/{nameof(DummyWithDependencyController)}")]
	public class DummyWithDependencyController : ControllerBase
	{
		readonly IDummyDependancy1 dependancy1;
		readonly IDummyDependancy2 dependancy2;

#if NETFRAMEWORK // TODO: fix test usage (Moq?)
		public DummyWithDependencyController() : this(new DummyDependancy1(), new DummyDependancy2())
		{
		}
#endif

		public DummyWithDependencyController(IDummyDependancy1 dependancy1, IDummyDependancy2 dependancy2)
		{
			this.dependancy1 = dependancy1;
			this.dependancy2 = dependancy2;
		}

		[HttpGet]
		[Route($"{nameof(GetDummyString)}")]
#if NET
		[Produces(MediaTypeNames.Application.Json)]
#endif
		public IActionResult GetDummyString()
		{
			return Ok($"{dependancy1.GetDummyString()} | {dependancy2.GetDummyString()}");
		}

		[HttpGet]
		[Route($"{nameof(GetDummyNumber)}")]
#if NET
		[Produces(MediaTypeNames.Application.Json)]
#endif
		public IActionResult GetDummyNumber()
		{
			return Ok(dependancy1.GetDummyNumber() + dependancy2.GetDummyNumber());
		}
	}
}
