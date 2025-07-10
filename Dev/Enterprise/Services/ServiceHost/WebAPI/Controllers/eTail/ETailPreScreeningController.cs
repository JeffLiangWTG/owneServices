using System;
using System.Net;
using System.Text;
#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
using Route = System.Web.Http.RouteAttribute;
#endif
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
#if NET
using Enterprise.Services.ServiceHost.NetCore;
#endif
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NET
	[Route("api/eTail")]
#elif NETFRAMEWORK
	[RoutePrefix("api/eTail")]
#endif
	public class ETailPreScreeningController : ControllerBase
	{
		const string DefaultMediaType = "text/html";

		public ETailPreScreeningController()
		{
		}

		[Route("pre-screening/{entityTableCode}/{entityPK:Guid}")]
		[HttpPost]
		public IActionResult GetPreScreenResultList(string entityTableCode, Guid entityPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var provider = GetPreScreeningProvider();
				var response = entityTableCode == HVLVConsignmentSchema.Constants.Prefix ?
					provider.PreScreenHVLVConsignment(entityPK) :
					provider.PreScreenHVLVConsignmentCollection(entityTableCode, entityPK);
				if (response.Finished)
				{
					return this.ReturnContentAsText(response, HttpStatusCode.OK, Encoding.UTF8, DefaultMediaType);
				}
				else
				{
					return this.ReturnContentAsText(response, HttpStatusCode.BadRequest, Encoding.UTF8, DefaultMediaType);
				}
			}
		}

		IETailPreScreeningService GetPreScreeningProvider()
		{
			var provider = ObjectFactory.Get<IETailPreScreeningService>("IETailPreScreeningService", new BusinessObjectFactory());
			return provider;
		}
	}
}
