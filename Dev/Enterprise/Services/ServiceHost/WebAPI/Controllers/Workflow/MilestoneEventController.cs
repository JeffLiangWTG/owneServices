using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	[RoutePrefix("api/milestones")]
	public class MilestoneEventController : ApiController
	{
		public MilestoneEventController() : this(new GlowContactSecurityService())
		{
		}

		public MilestoneEventController(IGlowContactSecurityService glowContactSecurityService)
		{
			this.glowContactSecurityService = glowContactSecurityService ?? throw new ArgumentNullException(nameof(glowContactSecurityService));
		}

		[Route("getEditableMilestones/{prefix}/{entityPK}")]
		[HttpGet]
		public IHttpActionResult GetEditableMilestones([FromUri] string prefix, [FromUri] Guid entityPK)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Update Milestones Web Service" };
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contact = identity?.GetContact(factory);

				if (contact == null)
				{
					return StatusCode(HttpStatusCode.Forbidden);
				}

				var entity = factory.Load(prefix, entityPK);
				if (entity == null)
				{
					return NotFound();
				}

				var updateableEventCodes = GetUpdateableMilestoneEventCodes(entity, contact);

				var workFlowShipment = (IWorkflowProvider)entity;
				var currentMilestones = workFlowShipment.WorkflowItems.MilestonesIncludingRelated;
				var editableMilestones = new List<IEditableMilestone>();
				foreach (ProcessTask milestone in currentMilestones)
				{
					if (updateableEventCodes.Contains(milestone.P9_SE_NKMilestoneEvent))
					{
						var editableMilestone = new EditableMilestone();
						editableMilestone.PK = milestone.PK.ToGuid();
						editableMilestone.ScheduledDate = milestone.P9_ScheduledDateOffset.IsValid
								? milestone.P9_ScheduledDateOffset.ToDateTimeOffset()
								: null;
						editableMilestone.ActualDate = milestone.P9_ActualDateForBinding.IsValid
								? milestone.P9_ActualDateOffset.ToDateTimeOffset()
								: null;
						editableMilestone.EventCode = milestone.P9_SE_NKMilestoneEvent;
						editableMilestones.Add(editableMilestone);
					}
				}
				return Json(editableMilestones);
			}
		}

		[Route("updateMilestoneDates/{prefix}/{entityPK}")]
		[HttpPost]
		public IHttpActionResult UpdateMilestoneDates(string prefix, Guid entityPK, [FromBody] EditableMilestone[] milestones)
		{
			if (milestones == null)
			{
				return BadRequest();
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Update Milestones Web Service" };
				var identity = User?.Identity as IGlowAuthenticationTicketIdentity;
				var contact = identity?.GetContact(factory);

				if (contact == null || !HasPermission(identity, milestones))
				{
					return StatusCode(HttpStatusCode.Forbidden);
				}

				var entity = GetEntity(factory, entityPK, contact);
				var updateableMilestoneEventCodes = entity != null ? GetUpdateableMilestoneEventCodes(entity, contact) : [];
				if (entity == null || milestones.Any(m => !updateableMilestoneEventCodes.Contains(m.EventCode)))
				{
					return NotFound();
				}

				var workflowMilestones = entity.WorkflowItems.MilestonesIncludingRelated.OfType<ProcessTask>();
				var milestonesMap = new Dictionary<Guid, ProcessTask>();
				foreach (var milestone in milestones)
				{
					var currentMilestone = workflowMilestones.FirstOrDefault(m => m.PK == milestone.PK);

					if (currentMilestone == null)
					{
						return NotFound();
					}

					if (CheckConcurrency(currentMilestone, milestone))
					{
						return StatusCode(HttpStatusCode.PreconditionFailed);
					}

					milestonesMap[milestone.PK] = currentMilestone;
				}

				UpdateMilestones(milestones, milestonesMap);

				return SaveChanges(factory);
			}
		}

		bool HasPermission(IGlowAuthenticationTicketIdentity identity, EditableMilestone[] milestones)
		{
			var hasEstimatedDateUpdaterRole = glowContactSecurityService.HasGroupRole(identity, (NoResString)"webestimatedmilestonedateupdater");
			var hasActualDateUpdaterRole = glowContactSecurityService.HasGroupRole(identity, (NoResString)"webactualmilestonedateupdater");

			return milestones.All(m =>
				(m.ScheduledDate == m.NewScheduledDate || hasEstimatedDateUpdaterRole) &&
				(m.ActualDate == m.NewActualDate || hasActualDateUpdaterRole));
		}

		ForwardingShipment GetEntity(BusinessObjectFactory factory, Guid entityPK, OrgContact contact)
		{
			var query = ObjectFactory.Get<IContactDataRestrictionProvider>().GetFilter(WebModuleIDs.TrackingShipments, contact.PK, factory);
			query.AddToFilter(JobShipmentSchema.PK, entityPK);
			return factory.LoadTop1<ForwardingShipment>(query);
		}

		void UpdateMilestones(EditableMilestone[] milestones, Dictionary<Guid, ProcessTask> milestonesMap)
		{
			foreach (var milestone in milestones)
			{
				var currentMilestone = milestonesMap[milestone.PK];

				if (milestone.ScheduledDate != milestone.NewScheduledDate)
				{
					currentMilestone.P9_ScheduledDateForBinding = milestone.NewScheduledDate != null ? (ZDateTimeOffset)milestone.NewScheduledDate : ZDateTimeOffset.Empty;
				}

				if (milestone.ActualDate != milestone.NewActualDate)
				{
					currentMilestone.P9_ActualDateForBinding = milestone.NewActualDate != null ? (ZDateTimeOffset)milestone.NewActualDate : ZDateTimeOffset.Empty;
				}

				currentMilestone.ClearRowNotifications();
				currentMilestone.RunPreSaveValidation();
			}
		}

		IHttpActionResult SaveChanges(BusinessObjectFactory factory)
		{
			try
			{
				factory.Save();
				return StatusCode(HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		List<string> GetUpdateableMilestoneEventCodes(BusinessObject entity, OrgContact contact)
		{
			var webParties = WebPartyProvider.GetWebParties(entity);
			return UpdateableMilestoneEventsHelper.GetUpdateableMilestoneEvents(WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.Value, webParties, contact.OC_OH);
		}

		bool CheckConcurrency(ProcessTask currentMilestone, IEditableMilestone milestone)
		{
			return (currentMilestone.PK.ToGuid() != milestone.PK)
				|| (currentMilestone.P9_ScheduledDateOffset.IsValid
								? currentMilestone.P9_ScheduledDateOffset.ToDateTimeOffset()
								: null) != milestone.ScheduledDate
				|| (currentMilestone.P9_ActualDateOffset.IsValid
								? currentMilestone.P9_ActualDateOffset.ToDateTimeOffset()
								: null) != milestone.ActualDate
				|| (currentMilestone.P9_SE_NKMilestoneEvent.ToString() != milestone.EventCode);
		}

		readonly IGlowContactSecurityService glowContactSecurityService;
	}
}
