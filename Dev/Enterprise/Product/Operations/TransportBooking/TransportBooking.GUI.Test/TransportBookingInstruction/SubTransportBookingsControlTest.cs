using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class SubTransportBookingsControlTest : DtbBookingTestCaseWithFactory
	{
		public void TestBookingsModuleButtonGridColumns()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = 1;
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_IsMaster = true;
			booking.KM_MasterBookingVersion = 1;
			Factory.Save();

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				var gridColumns = form.InstructionViewsControl.SubTransportBookingsControl.BookingsModuleButtonGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var gridColumnNames = gridColumns.Select(l => l.ColumnName);

				var expectedGridColumnNames = new[]
				{
					"KM_JobID",
					"ConsolidationSingleJob+Parent+JobNumber",
					"ConsolidationMultiJob+KB_JobID",
					"KM_KT_NKBookingTemplate",
					"BookingTemplate+KT_DescriptionMultilingual",
					"Address+OrganisationNameOrPK",
					"Address+E2_OA_Address",
					"KM_TransportReference",
					"StatusDescription",
					"KM_Direction",
					"KM_IsActive",
					"WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent",
					"WorkflowItems+Milestones+LastMilestone+DescriptionWithReference",
					"WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding",
					"WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent",
					"WorkflowItems+Milestones+NextMilestone+DescriptionWithReference",
					"WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding",
					"ConsolidationSingleJob+KB_GoodsDescription",
					"WayBillNumber",
					"KM_RS_NKServiceLevel",
					"KM_PL_NKCarrierServiceLevel",
					"KM_RatingFreightMode",
					"KM_IsHazardous",
					"KM_RequiresRefrigeration",
					"BillingPartyOrLocalClientPK_ZAddress+OrgPK",
					"BillingPartyOrLocalClientPK",
					"CarrierAccount+OAN_AccountNumber",
					"TotalPackages",
					"TotalPackagesType",
					"TotalWeightExcludingDuplicatePackages+Amount",
					"TotalWeightExcludingDuplicatePackages+Unit",
					"TotalVolumeExcludingDuplicatePackages+Amount",
					"TotalVolumeExcludingDuplicatePackages+Unit",
					"KM_Calc_ActualVolumeWeight",
					"KM_Calc_ActualVolumeWeightUnit",
					"KM_Calc_RoundedChargeable",
					"ChargeableUnits",
					"KM_OverrideChargeable",
					"CarrierBookingAgentDocAddress+OrganisationNameOrPK",
					"KM_GB_Branch",
					"MasterBooking+KM_JobID",
					"Address+E2_CompanyName",
					"CarrierBookingAgentDocAddress+E2_CompanyName",
					"KM_TransportMode",
					"BillingPartyAddress+CompanyName",
					"KM_Direction",
					"KM_SystemCreateTimeUtc",
					"KM_SystemLastEditTimeUtc",
					"KM_SystemCreateUser",
					"KM_SystemLastEditUser",
					"CarrierBookingAgentDocAddress+E2_OA_Address",
				};

				AssertContainsExactElementsInAnyOrder(expectedGridColumnNames, gridColumnNames);
			}
		}

		public void TestAttachButton()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_IsMaster = true;
			consolidation.KB_MasterBookingVersion = 1;
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_IsMaster = true;
			booking.KM_MasterBookingVersion = 1;
			Factory.Save();

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				AssertNoExceptionThrown("Should not throw exception upon clicking Attach button", () =>
				{
					form.InstructionViewsControl.SubTransportBookingsControl.AttachBookingButton.PerformClick();
					Application.DoEvents();
				});
			}
		}

		public void TestDetachButton()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			var masterBooking = Helper.CreateBooking(masterConsolidation);
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = 1;
			masterConsolidation.KB_MasterBookingVersion = 1;

			var subConsolidation = Helper.CreateConsolidation();
			var subBooking = Helper.CreateBooking(subConsolidation);
			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			subBooking.KM_MasterBookingVersion = masterBooking.KM_MasterBookingVersion;
			subBooking.ConsolidationSingleJob.KB_MasterBookingVersion = masterBooking.ConsolidationSingleJob.KB_MasterBookingVersion;
			Factory.Save();

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			using (var form = new TransportBookingForm(masterBooking))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertNoExceptionThrown("Should not throw exception when detaching sub booking", () =>
				{
					form.InstructionViewsControl.SubTransportBookingsControl.DetachBookingButton.PerformClick();
					Application.DoEvents();
				});
			}

			AssertEquals("Should have detached sub booking", 0, masterBooking.SubBookings.Count);
		}
	}
}
