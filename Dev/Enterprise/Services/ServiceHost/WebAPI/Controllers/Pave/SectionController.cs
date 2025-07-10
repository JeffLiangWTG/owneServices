using System;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class SectionController : BasePaveController<ISectionService>
	{
		public SectionController() : base(() => new SectionService())
		{
		}

		#region Section Data

		[Route("Pave/Section/{sectionPK}")]
		public IHttpActionResult Get(Guid sectionPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = Service.Get(sectionPK);

				return ToPaveResponseJson(result);
			}
		}

		#endregion

		#region TimeRecording

		//TODO: Move to a specific controller
		[Route("Pave/Section/{sectionPK}/TimeRecording")]
		public IHttpActionResult GetTimeRecording(Guid sectionPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					var result = Service.GetTimeRecording(sectionPK);
					return ToPaveResponseJson(result);
				}
				catch (Exception ex)
				{
					if (PaveControllerHelper.ShouldReportError(ex))
					{
						ErrorReporter.ReportOnce($"{nameof(GetTimeRecording)} request URI: {Request.RequestUri}", ex);
					}

					return InternalServerError(ex);
				}
			}
		}

		#endregion

		#region Filter Strips

		[Route("api/Pave/Section/{sectionPK}/Filters")]
		public IHttpActionResult GetFilterStrips(Guid sectionPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				try
				{
					var result = Service.GetFilterStrips(sectionPK);
					return ToPaveResponseJson(result);
				}
				catch (Exception ex)
				{
					if (PaveControllerHelper.ShouldReportError(ex))
					{
						ErrorReporter.ReportOnce($"{nameof(GetFilterStrips)} request URI: {Request.RequestUri}", ex);
					}

					return InternalServerError(ex);
				}
			}
		}

		#endregion
	}
}
