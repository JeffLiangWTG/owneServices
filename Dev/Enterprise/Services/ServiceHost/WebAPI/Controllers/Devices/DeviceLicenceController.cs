using System;
using System.Linq;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class DeviceLicenceController : ApiController
	{
		[Route("api/devices/enterprisecodelist")]
		[HttpGet]
		public IHttpActionResult GetEnterpriseCodeList()
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(
					CreateService()
					.GetEnterpriseCodeList()
					.Select(x => new CodeDescriptionPair { Code = x.Code, Description = x.Description }));
			}
		}

		[Route("api/devices/servercodelist")]
		[HttpGet]
		public IHttpActionResult GetServerCodeList([FromUri]string enterpriseCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(
					CreateService()
					.GetServerCodeList(enterpriseCode)
					.Select(x => new CodeDescriptionPair { Code = x.Code, Description = x.Description }));
			}
		}

		[Route("api/devices/customer")]
		[HttpGet]
		public IHttpActionResult GetCustomer([FromUri]string enterpriseCode, [FromUri]string serverCode)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(
					CreateService()
					.GetCustomer(enterpriseCode, serverCode));
			}
		}

		IDeviceLicenceService CreateService()
		{
			var type = DeviceLicenceService.MyTypeDecider.GetTypeForCreate();
			return (IDeviceLicenceService)Activator.CreateInstance(type);
		}
	}
}
