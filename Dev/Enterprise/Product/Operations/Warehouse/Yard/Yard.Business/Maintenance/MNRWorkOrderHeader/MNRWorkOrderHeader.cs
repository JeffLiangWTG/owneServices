using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[CodeProperty(Schema.MWO_JobNumber)]
	[UniversalDataContext(DataContextType.MNRWorkOrder)]
	public class MNRWorkOrderHeader : AutoMNRWorkOrderHeader,
		ICYDJobInvoicingSupporter,
		ICYDYardUnitsForRating,
		IDocAddresses,
		IDocManagerSupport, 
		IDocumentSupportable,
		IEDocsProvider,
		IWorkflowProvider,
		IJobInvoicingPlugIn,
		IMNRWorkOrderHeader,
		IRatingSupporter,
		IStmNoteParent,
		IYardWorkOrderForRating,
		ICYDReceiveAdvice
	{
		public MNRWorkOrderHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			DPPAmount = YardUnitState?.UnitLineItem?.YLI_DPPAmount ?? 0;
		}

		#region Properties

		public IReadOnlyList<MNRSurvey> Surveys
		{
			get => Factory.Load<MNRSurvey>(new ZQuery(MNRSurveySchema.MRS_MWO_MNRWorkOrderHeader, this.PK));
		}
		public CYDYardUnitState YardUnitState
		{
			get => Factory.Load<CYDYardUnitState>(MWO_ParentID);
		}

		[RelatedBusinessObject("Yard")]

		public override ZGuid MWO_WW_Facility { get => base.MWO_WW_Facility; set => base.MWO_WW_Facility = value; }

		public WhsWarehouse Yard
		{
			get
			{
				return Factory.Load<WhsWarehouse>(MWO_WW_Facility);
			}
		}

		[ChildEditable]
		public MNRWorkOrderLineCollection WorkOrderLines
		{
			get
			{
				if (workOrderLines == null)
				{
					workOrderLines = new MNRWorkOrderLineCollection(Factory, this);
					RegisterEditableChildObject(workOrderLines);
				}
				return workOrderLines;
			}
		}

		MNRWorkOrderLineCollection workOrderLines;

		[ChildEditable]
		public MNRWorkOrderApprovalPartyCollection WorkOrderApprovalParties
		{
			get
			{
				if (workOrderApprovalParties == null)
				{
					workOrderApprovalParties = new MNRWorkOrderApprovalPartyCollection(Factory, this);
					RegisterEditableChildObject(workOrderApprovalParties);
				}
				return workOrderApprovalParties;
			}
		}

		MNRWorkOrderApprovalPartyCollection workOrderApprovalParties;

		public ZString Status
		{
			get
			{
				if (!MWO_IsActive)
				{
					return Res.GetString("a09b9544-5c0d-4a9d-b8c5-f1fdb528b92d", "Deactivate");
				}

				if (!MWO_IsEstimateCompleted)
				{
					return Res.GetString("b6f1d154-8bec-45ab-a71d-0cb1d2d3a9e1", "Draft");
				}

				if (MWO_WorkOrderApprovedTime == ZDateTimeOffset.Empty)
				{
					return WorkOrderApprovalParties.Any(party => party.MNA_ApprovalResult == "REV")
						? Res.GetString("8b31bbd1-84c3-4f87-aa30-e3c630f3569b", "Revision")
						: Res.GetString("257128f9-52d3-458f-b48b-d6ea4a1c45eb", "New");
				}

				if (WorkOrderApprovalParties.Any(party => party.MNA_ApprovalResult == "PEN") &&
					!WorkOrderApprovalParties.Any(party => party.MNA_ApprovalResult == "REV"))
				{
					return Res.GetString("4b37ce38-42c3-4d6d-a43f-dd1c41c56aff", "Awaiting");
				}

				if (WorkOrderApprovalParties.All(party => party.MNA_ApprovalResult == "APP"))
				{
					if (WorkOrderLines.All(line => line.MWL_TaskEndTime != ZDateTimeOffset.Empty))
					{
						return Res.GetString("285b3152-cc16-488a-a650-c7a674960b52", "Completed");
					}

					if (WorkOrderLines.Any(line => line.MWL_TaskStartTime != ZDateTimeOffset.Empty) &&
						!WorkOrderLines.All(line => line.MWL_TaskEndTime != ZDateTimeOffset.Empty))
					{
						return Res.GetString("0d0ef8e7-2d3d-4838-b177-67abdfe02417", "Progress");
					}

					if (WorkOrderLines.All(line => line.MWL_TaskStartTime == ZDateTimeOffset.Empty))
					{
						return Res.GetString("a6477649-9064-446a-b2ba-06885779cf1d", "Approved");
					}
				}

				return ZString.Empty;
			}
		}

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new MNRWorkOrderHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.MNRWorkOrderHeaderWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress);

			var addressEntry = Factory.Load<JobDocAddress>(query).SingleOrDefault();

			if (addressEntry != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, addressEntry.OrganisationPK, null);
			}

			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

		#endregion

		#region IStmNoteParent

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.SurveyInstruction);
				noteTypes.Add(PredefinedNoteTypes.Instance.ContainerComment);

				return noteTypes;
			}
		}

		#endregion

		#region IDocAddress

		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new[]
				{
					DocAddressType.ShippingLineAddress,
					DocAddressType.ConsigneeAddress,
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.TransportCompanyDocumentaryAddress,
					DocAddressType.Forwarder
				};
			}
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return ""; }
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.MNRWorkOrderHeader);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region Implementation
		public override void Delete()
		{
			WorkOrderLines?.DeleteAll();
			WorkOrderApprovalParties?.DeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			MWO_Type = "STL";
			MWO_WW_Facility = Factory.NewWithValidTestData<WhsWarehouse>().PK;
			MWO_ParentTableCode = "YUS";

			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			MWO_ParentID = yardUnitState.PK;

			var receiveAdvice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			var receiveAdviceLine = Factory.NewWithValidTestData<CYDReceiveAdviceLine>();

			yardUnitState.YUS_YRL_ReceiveLine = receiveAdviceLine.PK;
			receiveAdviceLine.YRL_YRA_ReceiveAdvice = receiveAdvice.PK;
		}
#endif

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("95fff26a-7fc2-46b0-b9cf-f14af4349b1a", "Work order {0}", MWO_JobNumber);
			}
		}

		#endregion

		#endregion

		#region IJobInvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get
			{
				return invoicingPlugIn ?? (invoicingPlugIn = new CYDJobInvoicingPlugIn<MNRWorkOrderHeader>(this));
			}
		}
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => new CYDJobInvoicingSupporter<MNRWorkOrderHeader>(this);

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => MWO_JobNumber.ToString();

		public void OnJobCreated(JobHeader job)
		{
		}

		public void OnJobCreating(JobHeader job)
		{
		}

		public void OnJobDeleted(JobHeader job)
		{
		}

		public void OnJobDeleting(JobHeader job)
		{
		}

		public void SetJobNumberFieldOnSaving()
		{
		}

		#endregion

		#region ICYDJobInvoicingSupporter

		JobInvoicingConsumerType ICYDJobInvoicingSupporter.ConsumerType => JobInvoicingConsumerTypes.MNRWorkOrderHeader;

		SecurityCheckpoint ICYDJobInvoicingSupporter.AuditSecurity => Env.Security.MNRWorkOrderAuditBilling;

		SecurityCheckpoint ICYDJobInvoicingSupporter.JobInvoicingSecurity => Env.Security.MNRWorkOrderJobInvoicing;

		OrgHeader ICYDJobInvoicingSupporter.DefaultClient => Client;

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new MNRWorkOrderHeaderDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IRatingSupporter

		public RatingAdaptersProvider AdaptersProvider => new MNRWorkOrderHeaderRatingAdaptersProvider<MNRWorkOrderHeader>(this);

		#endregion

		#region ICYDYardUnitCollectionForRating

		IEnumerable<CYDYardUnitState> ICYDYardUnitsForRating.YardUnits => Surveys.Select(s => s.YardUnitState).ToArray();

		IReadOnlyList<string> ICYDYardUnitsForRating.ChargeCodeGroupList => new[] { ChargeCodeGroupList.Codes.MNRWorkOrderHeader };

		WhsWarehouse ICYDYardUnitsForRating.Yard => Yard;

		OrgHeader ICYDYardUnitsForRating.Client => Surveys[0]?.YardUnitState.ReceiveAdvice.Client.Organisation;

		public ZDecimal DPPAmount { get; set; }

		public bool HasDPP => YardUnitState?.UnitLineItem?.YLI_DPPAmount != 0;

		public JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.MNRWorkOrderHeader;

		public OrgHeader Client => YardUnitState?.ReceiveAdvice.Client.Organisation;

		public OrgHeader Lessee => YardUnitState.ReceiveAdvice.Lessee.Organisation;

		public OrgHeader Insurer => YardUnitState.Insurer.Organisation;

		public OrgHeader ThirdParty => YardUnitState.ThirdParty.Organisation;

		public KeyValuePair<string, string> MappedCommunityCode => new KeyValuePair<string, string>();

		#endregion
	}
}
