using System;
using System.Web.Http;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using WTG.Foundation.FrameworkExtensions;

namespace Enterprise.Services.ServiceHost
{
	#region SuppressResourceStringsCheckRegion

	[GlowTicketAuthentication]
	[ReportServiceErrorHandler]
	public class ReportConfigurationController : ReportDataBaseController
	{
		[Route("api/report/configurations/{reportId}")]
		[HttpGet]
		public IHttpActionResult GetConfigurations(Guid reportId)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				return Json(Service.GetConfigurations(reportId));
			}
		}

		[Route("api/report/configurations/save")]
		[HttpPost]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult SaveConfiguration(SelectedValueConfigurationData configurationData)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				Service.SaveConfiguration(configurationData);

				return Ok();
			}
		}

		[Route("api/report/configurations/delete/{reportId}/{name}")]
		[HttpDelete]
		[StaffOnlyAuthorizationFilter]
		public IHttpActionResult DeleteConfiguration(Guid reportId, string name)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				Service.DeleteConfiguration(reportId, name);
			}

			return Ok();
		}

		[Route("api/report/configurations/getxmlformat")]
		[HttpGet]
		public IHttpActionResult GetXmlFormat([FromUri] string name)
		{
			if (name.IsNullOrEmpty())
			{
				return Ok(ZString.Empty);
			}
			return Ok(((ZString)name).KeepAlphanumericCharactersXMLFormatting());
		}
	}

	#endregion
}
