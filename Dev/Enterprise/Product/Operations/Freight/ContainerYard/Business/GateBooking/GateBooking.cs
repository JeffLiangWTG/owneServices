using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateBooking : AutoGateBooking, IWorkflowProvider, IJobNumber, IUniversalXMLNoteParent
	{
		public GateBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region GateBookingDetails

		[ChildEditable]
		public GateBookingDetailCollection GateBookingDetails
		{
			get
			{
				if (gateBookingDetails == null)
				{
					gateBookingDetails = new GateBookingDetailCollection(this);
					RegisterEditableChildObject(gateBookingDetails);
				}

				return gateBookingDetails;
			}
		}

		GateBookingDetailCollection gateBookingDetails;

		#endregion

		#region Save/Delete

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(GTB_BookingNumberInfo, Env.NumberFountains.GateBookingNumber);
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			base.Delete();

			WorkflowItems.RemoveAndDeleteAll();
			GateBookingDetails.DeleteAll();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GateBookingProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		GateBookingProcessTaskCollection workflowItems;

		ZGuid IWorkflowProviderCore.PK => PK;

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.FacilityGateWorkflowDescriptorCode;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var selectionCriteria = new ColumnValueRanker();
			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_GB, GTB_GB_Branch, ZGuid.Empty);

			var facilityPK = GateBookingDetails
				.FirstOrDefault(detail => !detail.GTD_WW_Facility.IsEmpty)?
				.GTD_WW_Facility ?? ZGuid.Empty;

			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_WW, facilityPK, ZGuid.Empty);

			return selectionCriteria;
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get
			{
				return GTB_BookingNumber;
			}
		}

		#endregion
	}
}
