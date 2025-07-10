#if NETFRAMEWORK
using System.Web.Http;
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
#elif NET
using System.Net.Mime;
using Enterprise.Services.ServiceHost.NetCore;
using Microsoft.AspNetCore.Mvc;
#endif
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NETFRAMEWORK
	[RoutePrefix("api/accounting")]
#elif NET
	[Route("api/accounting")]
#endif
	public class AccountingController : ControllerBase
	{
		[Route("getControllerId")]
		[HttpGet]
#if NET
		[Produces(MediaTypeNames.Application.Json)]
#endif
		public IActionResult GetControllerID(ZGuid parentJobPK, string parentTableCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var job = factory.LoadGenericJob<IGenericJob>(parentJobPK, parentTableCode);
				var controllerID = job.GetConsumerController().Name;
				return Ok(controllerID);
			}
		}
	}
}
