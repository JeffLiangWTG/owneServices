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
using Microsoft.AspNetCore.Mvc;
#if NET
using Enterprise.Services.ServiceHost.NetCore;
#endif
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
#if NET
	[Route("api/eTail")]
#elif NETFRAMEWORK
	[RoutePrefix("api/eTail")]
#endif
	public class ETailPrintLabelController : ControllerBase
	{
		public ETailPrintLabelController()
		{
		}

		[Route("labelPrinting/{printerPK:Guid}/{itemPK}")]
		[HttpPost]
		public IActionResult PrintRoutingLabel(Guid printerPK, Guid itemPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var provider = ObjectFactory.Get<IHVLVRoutingLabelProvider>("IHVLVRoutingLabelProvider", itemPK);

				if (!TryPrintLabel(printerPK, provider.RoutingLabel, AttachmentType.PDF, out var printError))
				{
					return this.InternalServerError(printError);
				}
				return Ok();
			}
		}

		bool TryPrintLabel(Guid printerPK, byte[] data, string fileType, out string error)
		{
			error = string.Empty;
			var result = true;

			using (Db.DisposableActionForDbConnection())
			{
				var printFactory = new BusinessObjectFactory() { NameForDebugging = "Ecommerce Routing Label Printing Factory" };
				var printer = ObjectFactory.New<ILabelPrintingService>(printFactory);

				if (!printer.TryPrintLabel(data, fileType, printerPK, out var printingErrorMessage))
				{
					error = Res.GetString("758be42c-3dc9-4186-851a-0bbf637ca3ff", "Failed to print label: {0}", printingErrorMessage);
					result = false;
				}
			}

			return result;
		}
	}
}
