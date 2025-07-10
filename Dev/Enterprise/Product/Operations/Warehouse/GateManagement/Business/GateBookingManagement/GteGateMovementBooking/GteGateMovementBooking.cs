using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using EventCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using GateManagementConstants = Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[UniversalDataContext(DataContextType.GateMovementBooking)]
	public class GteGateMovementBooking : AutoGteGateMovementBooking, IGteGateMovementBooking, IWorkflowProvider
	{
		public GteGateMovementBooking(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				Logs.AddNew(AutoEvents.BookingPending, new KeyValuePair<string, string>[]
				{
					new (EventCodes.EquipmentReferenceNumber, GBM_UnitNumber),
					new (EventCodes.Facility, GBM_FacilityTableCode),
					new (EventCodes.JobNumber, Booking.GBK_SourceReferenceNumber),
					new (EventCodes.ReferenceNumber, Booking.GBK_ReferenceNumber),
				});
			}

			if (GBM_CancelledReasonInfo.HasChanges && !GBM_CancelledReasonInfo.Value.IsEmpty && GBM_CancelledReasonInfo.OriginalValue.IsEmpty)
			{
				GBM_GS_NKCancelledBy = GBM_GS_NKCancelledBy == string.Empty ? GlbStaff.CurrentUser.GS_Code : GBM_GS_NKCancelledBy;
				GBM_CancelledSource = GBM_CancelledSource == string.Empty ? GateManagementConstants.DataSources.VehicleBookingSystem : GBM_CancelledSource;

				if (GBM_CancelledTime.IsEmpty)
				{
					var latestBKLlog = Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingCancelledCode).OrderByDescending(x => x.SL_EventTime).FirstOrDefault();
					GBM_CancelledTime = latestBKLlog?.SL_EventTimeOffset ?? ZDateTimeOffset.Now;
				}

				Booking.PropagateBookingCancellation(this);
			}

			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(GBM_MovementBookingNumberInfo, Env.NumberFountains.GteMovementBookingNumber);
			}

			base.OnSaving();
		}

		#region Properties

		public GteBooking Booking => Factory.Load<GteBooking>(GBM_GBK_Booking);

		[RelatedBusinessObject("Booking")]
		public override ZGuid GBM_GBK_Booking { get => base.GBM_GBK_Booking; set => base.GBM_GBK_Booking = value; }

		#endregion

		#region Related Collections
		[ChildEditable]
		public GteGateMovementCollection GateMovements
		{
			get
			{
				if (gateMovements == null)
				{
					gateMovements = new GteGateMovementCollection(this);
					RegisterEditableChildObject(gateMovements);
				}

				return gateMovements;
			}
		}
		GteGateMovementCollection gateMovements;
		#endregion

		#region IWorkflowProvider Members

		[ChildEditable(true)]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}
				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GteGateMovementBookingProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.GteGateMovementBookingWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			if (Booking != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_WW, Booking.GBK_WW_Facility, ZGuid.Empty);
			}
			if (Booking?.Facility != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, Booking.Facility.WW_WarehouseType, ZString.Empty);
			}
			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			GBM_SourceReferenceNumber = string.Empty;
			GBM_Source = string.Empty;
		}

#endif

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			GateMovements.DeleteAll();
			base.Delete();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("1a7a7101-0605-4d51-a64a-b6351ccd0b54", "Gate Movement Booking");
	}
}
