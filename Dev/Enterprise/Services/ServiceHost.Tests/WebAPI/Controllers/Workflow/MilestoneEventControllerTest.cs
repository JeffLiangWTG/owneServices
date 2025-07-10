using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class MilestoneEventControllerTest : TestCaseWithFactory
	{
		#region GetEditableMilestones

		public void TestGetEditableMilestones_StaffLoggedIn()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, staff);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT";
			shipment.ConsigneePK = new Guid();
			Factory.Save();

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertResultContains(HttpStatusCode.Forbidden);
		}
		public void TestGetEditableMilestones_NonExistentRecord()
		{
			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, Guid.NewGuid());
			actionResult.AssertResultContains(HttpStatusCode.NotFound);
		}

		public void TestGetEditableMilestones_EmptyShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertJsonResultEquals(Array.Empty<object>());
		}

		public void TestGetEditableMilestones_ShipmentWithConsignee()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = contact.OC_OH;
			Factory.Save();

			var expectedEventCodes = getExpectedUpdateableMilestoneCodes(WebPartyType.Consignee);
			var expectedMilestones = new List<IEditableMilestone>();
			foreach (string eventCode in expectedEventCodes)
			{
				SetupWorkFlowShipmentWithMilestone(shipment, new ZDateTimeOffset(2000, 1, 1), new ZDateTimeOffset(2000, 1, 1), eventCode);
				expectedMilestones.Add(CreateEditableMilestone(milestone.PK.ToGuid(),
					milestone.P9_ScheduledDateOffset.ToDateTimeOffset(), milestone.P9_ActualDateOffset.ToDateTimeOffset(), milestone.P9_SE_NKMilestoneEvent));
			}

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertResultContains(HttpStatusCode.OK);
			actionResult.AssertJsonResultEquals(expectedMilestones);
		}

		public void TestGetEditableMilestones_ShipmentWithJob()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = contact.OC_OH;
			Factory.Save();

			var expectedEventCodes = getExpectedUpdateableMilestoneCodes(WebPartyType.LocalClient);
			var expectedMilestones = new List<IEditableMilestone>();
			foreach (string eventCode in expectedEventCodes)
			{
				SetupWorkFlowShipmentWithMilestone(shipment, new ZDateTimeOffset(2000, 1, 1), new ZDateTimeOffset(2000, 1, 1), eventCode);
				expectedMilestones.Add(CreateEditableMilestone(milestone.PK.ToGuid(),
					milestone.P9_ScheduledDateOffset.ToDateTimeOffset(), milestone.P9_ActualDateOffset.ToDateTimeOffset(), milestone.P9_SE_NKMilestoneEvent));
			}

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertResultContains(HttpStatusCode.OK);
			actionResult.AssertJsonResultEquals(expectedMilestones);
		}

		public void TestGetEditableMilestones_ShipmentWithConsol()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			contact.OC_OH = org.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CONSOL";
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Consols.Add(consol);
			Factory.Save();

			var expectedEventCodes = getExpectedUpdateableMilestoneCodes(WebPartyType.SendingAgent);
			var expectedMilestones = new List<IEditableMilestone>();
			foreach (string eventCode in expectedEventCodes)
			{
				SetupWorkFlowShipmentWithMilestone(shipment, new ZDateTimeOffset(2000, 1, 1), new ZDateTimeOffset(2000, 1, 1), eventCode);
				expectedMilestones.Add(CreateEditableMilestone(milestone.PK.ToGuid(),
					milestone.P9_ScheduledDateOffset.ToDateTimeOffset(), milestone.P9_ActualDateOffset.ToDateTimeOffset(), milestone.P9_SE_NKMilestoneEvent));
			}

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertResultContains(HttpStatusCode.OK);
			AssertResultContains(actionResult, expectedMilestones);
		}

		public void TestGetEditableMilestones_ShipmentWithDeliveryAgent()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OH_DeliveryAgent = contact.OC_OH;
			Factory.Save();

			var expectedEventCodes = getExpectedUpdateableMilestoneCodes(WebPartyType.DeliveryAgent);
			var expectedMilestones = new List<IEditableMilestone>();
			foreach (string eventCode in expectedEventCodes)
			{
				SetupWorkFlowShipmentWithMilestone(shipment, new ZDateTimeOffset(2000, 1, 1), new ZDateTimeOffset(2000, 1, 1), eventCode);
				expectedMilestones.Add(CreateEditableMilestone(milestone.PK.ToGuid(),
					milestone.P9_ScheduledDateOffset.ToDateTimeOffset(), milestone.P9_ActualDateOffset.ToDateTimeOffset(), milestone.P9_SE_NKMilestoneEvent));
			}

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertResultContains(HttpStatusCode.OK);
			actionResult.AssertJsonResultEquals(expectedMilestones);
		}

		public void TestGetEditableMilestones_ShipmentWithCombinedWebParties()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorPK = contact.OC_OH;
			shipment.JS_OH_ExportBroker = contact.OC_OH;
			Factory.Save();

			var expectedEventCodes = getExpectedUpdateableMilestoneCodes(WebPartyType.Shipper);
			expectedEventCodes.AddRange(getExpectedUpdateableMilestoneCodes(WebPartyType.ExportBroker));
			var expectedMilestones = new List<IEditableMilestone>();
			foreach (string eventCode in expectedEventCodes)
			{
				SetupWorkFlowShipmentWithMilestone(shipment, new ZDateTimeOffset(2000, 1, 1), new ZDateTimeOffset(2000, 1, 1), eventCode);
				expectedMilestones.Add(CreateEditableMilestone(milestone.PK.ToGuid(),
					milestone.P9_ScheduledDateOffset.ToDateTimeOffset(), milestone.P9_ActualDateOffset.ToDateTimeOffset(), milestone.P9_SE_NKMilestoneEvent));
			}

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertResultContains(HttpStatusCode.OK);
			actionResult.AssertJsonResultEquals(expectedMilestones);
		}

		public void TestGetEditableMilestones_ShipmentWithRelatedMilestones()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorPK = contact.OC_OH;
			shipment.JS_OH_ExportBroker = contact.OC_OH;
			var consol = shipment.Consols.AddNew();
			Factory.Save();

			var expectedEventCodes = getExpectedUpdateableMilestoneCodes(WebPartyType.Shipper);
			expectedEventCodes.AddRange(getExpectedUpdateableMilestoneCodes(WebPartyType.ExportBroker));
			var expectedMilestones = new List<IEditableMilestone>();
			foreach (var eventCode in expectedEventCodes)
			{
				SetupWorkFlowShipmentWithMilestone(consol, new ZDateTimeOffset(2000, 1, 1), new ZDateTimeOffset(2000, 1, 1), eventCode);
				expectedMilestones.Add(CreateEditableMilestone(milestone.PK.ToGuid(),
					milestone.P9_ScheduledDateOffset.ToDateTimeOffset(), milestone.P9_ActualDateOffset.ToDateTimeOffset(), milestone.P9_SE_NKMilestoneEvent));
			}

			var actionResult = controller.GetEditableMilestones(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid());
			actionResult.AssertResultContains(HttpStatusCode.OK);
			AssertResultContains(actionResult, expectedMilestones);
		}

		#endregion

		#region UpdateMilestoneDates

		public void TestUpdateMilestoneDates_InvalidClientMilestone()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), null);
				actionResult.AssertResultContains(HttpStatusCode.BadRequest);
			}
		}

		public void TestUpdateMilestoneDates_NonVisibleShipment()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			var nonVisibleShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, nonVisibleShipment.PK.ToGuid(), milestones.ToArray());
			actionResult.AssertResultContains(HttpStatusCode.NotFound);
		}

		public void TestUpdateMilestoneDates_NonEditableMilestone()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			var defaultOtherShipmentMilestoneEventUpdatesCollection = new ShipmentMilestoneEventUpdatesCollection();
			defaultOtherShipmentMilestoneEventUpdatesCollection.AddNew("OTH", WebPartyType.Consignee);
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultOtherShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), milestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.NotFound);
			}
		}

		public void TestUpdateMilestoneDates_NonExistentMilestone()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			var otherMilestones = new List<EditableMilestone>
			{
				CreateEditableMilestone(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, "JOP", null, null),
				CreateEditableMilestone(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(2), DateTimeOffset.UtcNow.AddDays(3), "PCA", null, null),
				CreateEditableMilestone(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-2), null, "ABC", null, null),
				CreateEditableMilestone(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddDays(5), "ECC", null, null)
			};
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), otherMilestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.NotFound);
			}
		}

		public void TestUpdateMilestoneDates_CheckConcurrency()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			var workflowMilestone = Factory.Load<ProcessTask>(milestones[1].PK);
			workflowMilestone.P9_ScheduledDateForBinding = new ZDateTimeOffset(2025, 7, 6, 0, 0, 0, TimeSpan.FromHours(-8));
			Factory.Save();
			milestones[1].NewActualDate = DateTimeOffset.UtcNow.AddDays(+3);
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), milestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.PreconditionFailed);
			}
		}

		public void TestUpdateMilestoneDates_MissingSecurityRoleForEstimatedDate()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(identity, (NoResString)"webestimatedmilestonedateupdater")).Returns(false);
			milestones[1].NewScheduledDate = DateTimeOffset.UtcNow.AddDays(+3);
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), milestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.Forbidden);
			}
		}

		public void TestUpdateMilestoneDates_MissingSecurityRoleForActualDate()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(identity, (NoResString)"webactualmilestonedateupdater")).Returns(false);
			milestones[1].NewActualDate = DateTimeOffset.UtcNow.AddDays(+3);
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), milestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.Forbidden);
			}
		}

		public void TestUpdateMilestoneDates_SaveSingleDate()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			milestones[1].NewActualDate = DateTimeOffset.UtcNow.AddDays(+3);
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), milestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.OK);
				var workflowMilestone = Factory.Load<ProcessTask>(milestones[1].PK);
				AssertEquals(milestones[1].NewActualDate, workflowMilestone.P9_ActualDateOffset.ToDateTimeOffset());
			}
		}

		public void TestUpdateMilestoneDates_SaveMultipleDatesOnSameMilestone()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			milestones[1].NewScheduledDate = DateTimeOffset.UtcNow.AddDays(-3);
			milestones[1].NewActualDate = DateTimeOffset.UtcNow.AddDays(+3);
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), milestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.OK);
				var workflowMilestone = Factory.Load<ProcessTask>(milestones[1].PK);
				AssertEquals(milestones[1].NewActualDate, workflowMilestone.P9_ActualDateOffset.ToDateTimeOffset());
			}
		}

		public void TestUpdateMilestoneDates_SaveMutipleDatesOnMutipleMilestones()
		{
			SetupShipmentWithShipperOrgAndMilestones();
			milestones[0].NewScheduledDate = DateTimeOffset.UtcNow.AddDays(-1);
			milestones[0].NewActualDate = DateTimeOffset.UtcNow.AddDays(-1);
			milestones[1].NewScheduledDate = DateTimeOffset.UtcNow.AddDays(+3);
			milestones[1].NewActualDate = DateTimeOffset.UtcNow.AddDays(+3);
			milestones[2].NewScheduledDate = new DateTimeOffset(2025, 02, 01, 02, 03, 04, TimeSpan.FromHours(-6));
			milestones[2].NewActualDate = new DateTimeOffset(2025, 02, 01, 02, 03, 04, TimeSpan.FromHours(-5));
			milestones[3].NewScheduledDate = DateTimeOffset.UtcNow.AddDays(+3);
			milestones[3].NewActualDate = null;
			milestones[4].NewScheduledDate = null;
			milestones[4].NewActualDate = null;
			using (WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultShipmentMilestoneEventUpdatesCollection))
			{
				var actionResult = controller.UpdateMilestoneDates(JobShipmentSchema.Constants.Prefix, shipment.PK.ToGuid(), milestones.ToArray());
				actionResult.AssertResultContains(HttpStatusCode.OK);
				var workflowMilestone = Factory.Load<ProcessTask>(milestones[1].PK);
				AssertEquals(milestones[1].NewActualDate, workflowMilestone.P9_ActualDateOffset.ToDateTimeOffset());
			}
		}

		#endregion

		#region Helpers

		List<string> getExpectedUpdateableMilestoneCodes(string webPartyType)
		{
			MilestoneEventUpdatesCollection eventUpdatesSettings = WebDataRegistry.Instance.ShipmentMilestoneEventUpdates.Value;

			var expectedResult = new List<string>();

			foreach (MilestoneEventUpdates item in eventUpdatesSettings)
			{
				if (item.GetValue(webPartyType))
				{
					expectedResult.Add(item.EventType);
				}
			}

			return expectedResult;
		}

		static void AssertResultContains(IHttpActionResult actionResult, List<IEditableMilestone> expectedItems)
		{
			foreach (var expected in expectedItems)
			{
				actionResult.AssertJsonResultContains(expected);
			}
		}

		#endregion

		#region Setup

		IGlowAuthenticationTicketIdentity SetupMockIdentity(Guid contactPK)
		{
			var mockIdentity = new Mock<IGlowAuthenticationTicketIdentity>();
			mockIdentity.SetupGet(x => x.IsAuthenticated).Returns(true);
			mockIdentity.SetupGet(x => x.ProviderType).Returns(OrgContactSchema.Constants.Prefix);
			mockIdentity.SetupGet(x => x.ProviderKey).Returns(contactPK);
			controller.User = new GenericPrincipal(mockIdentity.Object, null);

			return mockIdentity.Object;
		}

		void SetupShipmentWithShipperOrgAndMilestones()
		{
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorPK = contact.OC_OH;
			Factory.Save();
			var days = 0;
			var workflowMilestoneWithEmptyDates = SetupWorkFlowShipmentWithMilestone(shipment, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, $"EV{days}");
			milestones = [CreateEditableMilestone(workflowMilestoneWithEmptyDates.PK.ToGuid(), null, null, $"EV{days}", null, null)];
			for (days = 1; days < 5; days++)
			{
				var workflowMilestone = SetupWorkFlowShipmentWithMilestone(
					shipment,
					new ZDateTimeOffset(2025, 6, 2, 0, 0, 0, TimeSpan.FromHours(-8)).AddDays(-days),
					new ZDateTimeOffset(2025, 6, 2, 0, 0, 0, TimeSpan.FromHours(-5)).AddDays(days),
					$"EV{days}"
				);
				milestones.Add(CreateEditableMilestone(
					workflowMilestone.PK.ToGuid(),
					workflowMilestone.P9_ScheduledDateForBinding.ToDateTimeOffset(),
					workflowMilestone.P9_ActualDateForBinding.ToDateTimeOffset(),
					workflowMilestone.P9_SE_NKMilestoneEvent,
					null,
					null
				));
			}
			SetupShipmentMilestoneEventUpdatesCollection();
		}

		ProcessTask SetupWorkFlowShipmentWithMilestone(BusinessObject shipment, ZDateTimeOffset scheduledDate, ZDateTimeOffset actualDate, string eventCode)
		{
			workflowShipment = (IWorkflowProvider)shipment;
			milestone = workflowShipment.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = eventCode;
			milestone.P9_ScheduledDateForBinding = scheduledDate;
			milestone.P9_ActualDateForBinding = actualDate;
			Factory.Save();
			return milestone;
		}

		void SetupShipmentMilestoneEventUpdatesCollection()
		{
			defaultShipmentMilestoneEventUpdatesCollection = new ShipmentMilestoneEventUpdatesCollection();
			foreach (var milestone in milestones)
			{
				defaultShipmentMilestoneEventUpdatesCollection.AddNew(milestone.EventCode, WebPartyType.Shipper);
			}
		}
		EditableMilestone CreateEditableMilestone(Guid pk, DateTimeOffset? scheduledDate, DateTimeOffset? actualDate, string eventCode, DateTimeOffset? newScheduledDate = null, DateTimeOffset? newActualDate = null)
		{
			var milestone = new EditableMilestone();
			milestone.PK = pk;
			milestone.ScheduledDate = scheduledDate;
			milestone.ActualDate = actualDate;
			milestone.EventCode = eventCode;
			milestone.NewScheduledDate = newScheduledDate;
			milestone.NewActualDate = newActualDate;
			return milestone;
		}

		protected override void SetUp()
		{
			base.SetUp();

			contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			mockGlowContactSecurityService = new Mock<IGlowContactSecurityService>();
			controller = new MilestoneEventController(mockGlowContactSecurityService.Object);
			var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
			controller.ControllerContext = controllerContext;
			identity = SetupMockIdentity(contact.PK.ToGuid());
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(identity, (NoResString)"webestimatedmilestonedateupdater")).Returns(true);
			mockGlowContactSecurityService.Setup(x => x.HasGroupRole(identity, (NoResString)"webactualmilestonedateupdater")).Returns(true);
		}

		#endregion

		MilestoneEventController controller;
		Mock<IGlowContactSecurityService> mockGlowContactSecurityService;
		OrgContact contact;
		IGlowAuthenticationTicketIdentity identity;
		ForwardingShipment shipment;
		IWorkflowProvider workflowShipment;
		ProcessTask milestone;
		List<EditableMilestone> milestones;
		ShipmentMilestoneEventUpdatesCollection defaultShipmentMilestoneEventUpdatesCollection;
	}
}
