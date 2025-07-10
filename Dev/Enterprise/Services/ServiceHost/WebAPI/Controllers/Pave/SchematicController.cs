using System;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.BufferManagement.Service;
using Enterprise.BufferManagement.Service.Shared;
using SchematicControllerConstants = CargoWise.PAVE.Common.Interfaces.PAVEServicesConstants.SchematicController;

namespace Enterprise.Services.ServiceHost
{
	[RoutePrefix("API/" + SchematicControllerConstants.Path)]
	[GlowTicketAuthentication]
	public class SchematicController : BasePaveController<ISchematicService>
	{
		SchematicServiceLogger Logger { get; }

		public SchematicController() : base(() => new SchematicService())
		{
			Logger = new SchematicServiceLogger(nameof(ServiceHost), nameof(SchematicController));
		}

		[Route(SchematicControllerConstants.ProcessTransferRulesAction)]
		public IHttpActionResult PostProcessTransferRules([FromBody] Guid[] transferablePKs)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				Service.ProcessTransferRules(transferablePKs, Logger);

				return ToPaveResponseJson(Logger.Logs);
			}
		}
	}
}
