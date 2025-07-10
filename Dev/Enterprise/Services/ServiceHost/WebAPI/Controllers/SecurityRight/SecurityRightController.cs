#if NETFRAMEWORK
using System.Web.Http;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
#endif
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost.NetCore
{
	[GlowTicketAuthentication]
	public class SecurityRightController : ControllerBase
	{
		[HttpGet]
		[Route("api/getrefdoctypesecurities")]
		public IActionResult GetRefDocTypeSecurities()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var securityRights = new CodeDescriptionPairList();

				var documents = new DocumentWebSecurityRights(factory).OrderBy(x => x.Code);
				foreach (var item in documents)
				{
					securityRights.AddPair(item.Code, item.MultilingualDescription);
				}

				return Ok(securityRights);
			}
		}

		[HttpGet]
		[Route("api/getreportsecurities")]
		public IActionResult GetReportSecurities()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory();
				var securityRights = new CodeDescriptionPairList();

				var reports = ReportsWebSecurityRights.New(factory).OrderBy(x => x.MultilingualDescription);
				foreach (var item in reports)
				{
					securityRights.AddPair(item.Code, item.MultilingualDescription);
				}

				return Ok(securityRights);
			}
		}
	}
}
