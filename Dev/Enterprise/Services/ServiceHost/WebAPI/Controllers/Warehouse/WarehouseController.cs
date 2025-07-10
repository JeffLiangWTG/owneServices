using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Services.ServiceHost.WebAPI.Authentication;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class WarehouseController : ApiController
	{
		[Route("api/CycleCount/MarkInventoriesLost")]
		[HttpPost]
		public IHttpActionResult MarkInventoriesLost([FromBody] Guid[] cycleCountPKs)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				if (cycleCountPKs.Length > 0)
				{
					var cycleCountService = ObjectFactory.Get<IWhsCycleCountLocationService>();
					cycleCountService.MarkInventoriesLostInCycleCount(cycleCountPKs.Select(pk => new ZGuid(pk)).ToArray());
				}
			}

			return Ok();
		}

		[Route("api/PackingConsolidation/CreateDockDoorTransfer/{handlingUnitPackagePK}")]
		[HttpPost]
		public IHttpActionResult CreateDockDoorTransfer(Guid handlingUnitPackagePK)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var packingConsolidationService = ObjectFactory.Get<IWhsPackingConsolidationService>();
				errorMessage = packingConsolidationService.CreateDockDoorTransfer(new ZGuid(handlingUnitPackagePK));
			}

			return Ok(errorMessage);
		}

		[Route("api/PackingConsolidation/PutawayStockInDockDoor/{handlingUnitPackagePK}/{dockDoorLocationPK}/{warehousePK}")]
		[HttpPost]
		public IHttpActionResult PutawayStockInDockDoor(Guid handlingUnitPackagePK, Guid dockDoorLocationPK, Guid warehousePK)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var packingConsolidationService = ObjectFactory.Get<IWhsPackingConsolidationService>();
				errorMessage = packingConsolidationService.PutawayStockInDockDoor(handlingUnitPackagePK, dockDoorLocationPK, warehousePK);
			}

			return Ok(errorMessage);
		}

		[Route("api/PackingConsolidation/GenerateHandlingUnit/{handlingUnitPK}/{warehousePK}/{loadPK}")]
		[HttpPost]
		public IHttpActionResult GenerateHandlingUnit(Guid handlingUnitPK, Guid warehousePK, Guid? loadPK)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var packingConsolidationService = ObjectFactory.Get<IWhsPackingConsolidationService>();
				errorMessage = packingConsolidationService.GenerateHandlingUnit(handlingUnitPK, warehousePK, loadPK);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/CreateAdHocServiceJob")]
		[HttpPost]
		public IHttpActionResult CreateAdHocServiceJob([FromBody] CreateAdHocServiceJobArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var glowServicesModuleService = ObjectFactory.Get<IWhsGlowServicesModuleService>();
				errorMessage = glowServicesModuleService.CreateAdHocServiceJob(args.JobPK, args.ClientPK, args.WarehousePK, new ZDate(args.BillingDate), args.CustomerReference);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/CreateWhsJobService")]
		[HttpPost]
		public IHttpActionResult CreateWhsJobService([FromBody] WhsJobServiceArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var glowServicesModuleService = ObjectFactory.Get<IWhsGlowServicesModuleService>();
				errorMessage = glowServicesModuleService.CreateWhsJobService(args.ServicePK, args.ServiceType, args.ServiceCount, args.JobPK, args.JobType, args.BookedDateTimeOffset, args.Contractor, args.Duration > 0 ? TimeSpan.FromSeconds(args.Duration) : ZDateTime.Invalid, args.LocationPK, args.SubLocation, args.Reference, args.Note);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/CompleteWhsJobService")]
		[HttpPost]
		public IHttpActionResult CompleteWhsJobService([FromBody] WhsJobServiceArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var glowServicesModuleService = ObjectFactory.Get<IWhsGlowServicesModuleService>();
				errorMessage = glowServicesModuleService.CompleteWhsJobService(args.ServicePK, args.ServiceType, args.ServiceCount, args.BookedDateTimeOffset, args.Contractor, args.Duration > 0 ? TimeSpan.FromSeconds(args.Duration) : ZDateTime.Invalid, args.LocationPK, args.SubLocation, args.Reference, args.Note, args.FinaliseServiceJob);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/CompleteAllWhsJobServices")]
		[HttpPost]
		public IHttpActionResult CompleteAllWhsJobServices([FromBody] CompleteAllWhsJobServicesArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var glowServicesModuleService = ObjectFactory.Get<IWhsGlowServicesModuleService>();
				errorMessage = glowServicesModuleService.CompleteAllWhsJobServices(args.JobPK, args.JobType, args.FinaliseServiceJob);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/ChangeTaskPlanningStatus")]
		[HttpPost]
		public IHttpActionResult ChangeTaskPlanningStatus([FromBody] ChangeTaskPlanningStatusArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var service = ObjectFactory.Get<IWhsTaskManagementService>();
				errorMessage = service.ChangeTaskPlanningStatus(new ZGuid(args.JobPK), args.JobType, args.ChangeStatusToReady);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/BatchChangeTaskPlanningStatus")]
		[HttpPost]
		public IHttpActionResult BatchChangeTaskPlanningStatus([FromBody] BatchChangeTaskPlanningStatusArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var service = ObjectFactory.Get<IWhsTaskManagementService>();
				errorMessage = service.BatchChangeTaskPlanningStatus(args.JobPKs.Select(pk => new ZGuid(pk)).ToArray(), args.JobType, args.ChangeStatusToReady);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/AddPackageToHandlingUnit/{packagePK}/{handlingUnitPackagePK}")]
		[HttpPost]
		public IHttpActionResult AddPackageToHandlingUnit(Guid packagePK, Guid handlingUnitPackagePK)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var assignDockDoorService = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>();
				errorMessage = assignDockDoorService.AddPackageToHandlingUnit(new ZGuid(packagePK), new ZGuid(handlingUnitPackagePK));
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/RemovePackageFromHandlingUnit/{packagePK}")]
		[HttpPost]
		public IHttpActionResult RemovePackageFromHandlingUnit(Guid packagePK)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var assignDockDoorService = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>();
				errorMessage = assignDockDoorService.RemovePackageFromHandlingUnit(new ZGuid(packagePK));
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/GetIsDockDoorOverrideAllowedForHandlingUnit/{packagePK}")]
		[HttpPost]
		public IHttpActionResult GetIsDockDoorOverrideAllowedForHandlingUnit(Guid packagePK)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			string errorMessage = null;
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var packingConsolidationService = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>();
				errorMessage = packingConsolidationService.GetIsDockDoorOverrideAllowedForHandlingUnit(packagePK);
			}

			return Ok(errorMessage);
		}

		[Route("api/WarehouseServices/GetNextTask")]
		[HttpPost]
		public IHttpActionResult GetNextTask([FromBody] GetNextTaskArgs args)
		{
			if (!GlowPrincipalHelper.IsStaff(User))
			{
				return StatusCode(HttpStatusCode.Forbidden);
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var service = ObjectFactory.Get<IWhsTaskManagementService>();
				var result = service.GetNextTask(GetNewBusinessObjectFactory(), args.TaskReference, Env.CurrentUserPK, args.WarehousePK, args.FormFlowType, args.LastFormFlowType, args.TasksToIgnore);
				return Json(result);
			}
		}

		static BusinessObjectFactory GetNewBusinessObjectFactory() => new BusinessObjectFactory() { NameForDebugging = nameof(WarehouseController) };
	}
}
