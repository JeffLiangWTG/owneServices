using System;
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
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NET
	[Route("api/eTail")]
#elif NETFRAMEWORK
	[RoutePrefix("api/eTail")]
#endif
	public class ETailConvertToStandAloneDeclarationController : ControllerBase
	{
		public ETailConvertToStandAloneDeclarationController()
		{
		}

		[Route("convertToStandAloneDeclaration/{entityPK}")]
		[HttpPost]
		public IActionResult ConvertToStandAloneDeclaration(Guid entityPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory { NameForDebugging = nameof(ETailConvertToStandAloneDeclarationController) };
				var service = GetStandAloneDeclarationService();
				var response =	service.ConvertToStandAloneDeclaration(entityPK, factory);
				if (response.ConversionSucceeded)
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null, true);
					return Ok(response.ConvertedStandAloneDeclaration.ToString());
				}
				else
				{
					return BadRequest(response.ConversionFailureReason);
				}
			}
		}

		IConvertToStandAloneDeclarationService GetStandAloneDeclarationService() => ObjectFactory.Get<IConvertToStandAloneDeclarationService>("IConvertToStandAloneDeclarationService");
	}
}
