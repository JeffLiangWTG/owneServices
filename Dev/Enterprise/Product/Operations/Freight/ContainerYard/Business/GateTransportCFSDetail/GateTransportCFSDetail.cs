using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.ContainerYard.Business
{
	[CodeProperty(Schema.GTF_JobNumber), DescriptionProperty(Schema.GTF_EquipmentNumber)]
	public class GateTransportCFSDetail : AutoGateTransportCFSDetail, IEDocsProvider, IWorkflowProvider
	{
		public GateTransportCFSDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region GTF_WL_Dock

		public WhsLocation Dock
		{
			get { return Factory.Load<WhsLocation>(GTF_WL_Dock); }
		}

		[RelatedBusinessObject("Dock")]
		public override ZGuid GTF_WL_Dock
		{
			get { return base.GTF_WL_Dock; }
			set { base.GTF_WL_Dock = value; }
		}

		#endregion

		#region GTF_GTD_GateBookingDetail

		[RelatedBusinessObject("GateBookingDetail")]
		public override ZGuid GTF_GTD_GateBookingDetail
		{
			get { return base.GTF_GTD_GateBookingDetail; }
			set { base.GTF_GTD_GateBookingDetail = value; }
		}

		public GateBookingDetail GateBookingDetail => Factory.Load<GateBookingDetail>(GTF_GTD_GateBookingDetail);

		#endregion

		#region GTF_WW_Facility

		[RelatedBusinessObject("Facility")]
		[List("Lookups.Warehouses")]
		public override ZGuid GTF_WW_Facility
		{
			get { return base.GTF_WW_Facility; }
			set { base.GTF_WW_Facility = value; }
		}

		public WhsWarehouse Facility => Factory.Load<WhsWarehouse>(GTF_WW_Facility);

		#endregion

		#region GTF_GTT

		[RelatedBusinessObject("GateTransport")]
		public override ZGuid GTF_GTT
		{
			get { return base.GTF_GTT; }
			set { base.GTF_GTT = value; }
		}

		public GateTransport GateTransport
		{
			get { return Factory.Load<GateTransport>(GTF_GTT); }
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();

			base.Delete();
		}

		#region IEDocsProvider

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.GateTransportCFSDetail));
			}
		}
		DocManagerInfo docManagerInfo;

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return eDocsProviderSupporter ?? (eDocsProviderSupporter = new EDocsProviderSupporter(this));
		}
		EDocsProviderSupporter eDocsProviderSupporter;

		public DocumentSupporter DocumentSupporter => new GateTransportCFSDetailDocumentSupporter(this);

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GateTransportCFSDetailProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		GateTransportCFSDetailProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.GateTransportCFSWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var selectionCriteria = new ColumnValueRanker();
			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_OH_Client, GTF_OH_Owner, ZGuid.Empty);
			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_WW, GTF_WW_Facility, ZGuid.Empty);
			var relatedBranchId = Facility != null && Facility.WW_GB_RelatedCompanyBranch.IsValid
					 ? Facility.WW_GB_RelatedCompanyBranch
					 : ZGuid.Empty;
			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_GB, relatedBranchId, ZGuid.Empty);

			return selectionCriteria;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		#endregion
	}
}
