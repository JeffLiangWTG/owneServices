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
	[CodeProperty(Schema.YRA_JobNumber)]
	[UniversalDataContext(DataContextType.CYDReceiveAdvice)]
	public class CYDReceiveAdvice : AutoCYDReceiveAdvice,
		IDocAddresses,
		IDocumentSupportable,
		IEDocsProvider,
		IJobInvoicingPlugIn,
		IRatingSupporter,
		IStmNoteParent,
		IWorkflowProvider,
		ICYDJobInvoicingSupporter,
		ICYDYardUnitsForRating,
		ICYDReceiveAdvice
	{
		public CYDReceiveAdvice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Yard")]
		public override ZGuid YRA_WW_Yard { get => base.YRA_WW_Yard; set => base.YRA_WW_Yard = value; }

		public WhsWarehouse Yard
		{
			get => Factory.Load<WhsWarehouse>(YRA_WW_Yard);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return YRA_JobNumber.IsEmpty ?
					Res.GetString("166e04e4-2eab-4677-835d-3f9205312e36", "Pre-Arrival Instruction") :
					Res.GetString("6953998c-0a4f-4407-8a98-38cb16752e39", "Pre-Arrival Instruction {0}", YRA_JobNumber);
			}
		}

		#endregion

		#region RelatedCollections

		public IEnumerable<CYDYardUnitState> UnloadedYardUnits
		{
			get
			{
				if (unloadedYardUnits == null)
				{
					var query = new ZDBOnlyQuery(typeof(CYDYardUnitState));
					var subquery = new ZDBOnlySubQuery(typeof(CYDReceiveAdviceLine), CYDYardUnitStateSchema.YUS_YRL_ReceiveLine);
					subquery.AddToFilter(CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice, PK);
					query.AddSubQuery(subquery, JoinCondition.And);
					query.AddToFilter(CYDYardUnitStateSchema.YUS_UnloadTime, SQLComparisonOperator.NotEqual, null);
					unloadedYardUnits = Factory.Load<CYDYardUnitState>(query);
				}
				return unloadedYardUnits;
			}
		}
		IEnumerable<CYDYardUnitState> unloadedYardUnits;

		#endregion RelatedCollections

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
					DocAddressType.Forwarder,
					DocAddressType.ControllingCustomer,
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

		#region IJobInvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get
			{
				return invoicingPlugIn ?? (invoicingPlugIn = new CYDJobInvoicingPlugIn<CYDReceiveAdvice>(this));
			}
		}
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => new CYDJobInvoicingSupporter<CYDReceiveAdvice>(this);

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => YRA_JobNumber.ToString();

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

		JobInvoicingConsumerType ICYDJobInvoicingSupporter.ConsumerType => JobInvoicingConsumerTypes.CYDReceiveAdvice;

		SecurityCheckpoint ICYDJobInvoicingSupporter.AuditSecurity => Env.Security.CYDReceiveAdviceAuditBilling;

		SecurityCheckpoint ICYDJobInvoicingSupporter.JobInvoicingSecurity => Env.Security.CYDReceiveAdviceJobInvoicing;

		OrgHeader ICYDJobInvoicingSupporter.DefaultClient => Client?.Organisation;

		#endregion

		#region IRatingSupporter

		public RatingAdaptersProvider AdaptersProvider => new CYDYardRatingAdaptersProvider<CYDReceiveAdvice>(this);

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CYDReceiveAdviceDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IStmNoteParent

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
				noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);

				return noteTypes;
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CYDReceiveAdviceProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.CYDReceiveAdviceWorkflowDescriptorCode;

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

		#region Implementation

		public override void Delete()
		{
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
			YRA_FromDate = ZDate.Today;
			YRA_ToDate = ZDate.Today.AddDays(7);
			YRA_Mode = "FCL";
			YRA_WW_Yard = Factory.NewWithValidTestData<WhsWarehouse>().PK;
		}
#endif

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.CYDReceiveAdvice);
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

		#region Client

		public JobDocAddress Client
		{
			get
			{
				if (client == null || client.IsDeleted)
				{
					client = DocAddresses.FindOrCreateWithRequirement(ClientAddressRequirement);
				}
				return client;
			}
		}
		JobDocAddress client;

		JobDocAddressRequirement ClientAddressRequirement
		{
			get { return clientAddressRequirement ??= new JobDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress, ContactType.LocalClient); }
		}

		JobDocAddressRequirement clientAddressRequirement;

		#endregion

		#region Lessee

		public JobDocAddress Lessee
		{
			get
			{
				if (lessee == null || lessee.IsDeleted)
				{
					lessee = DocAddresses.FindOrCreateWithRequirement(LesseeAddressRequirement);
				}
				return lessee;
			}
		}

		JobDocAddress lessee;

		JobDocAddressRequirement LesseeAddressRequirement
		{
			get { return lesseeAddressRequirement ??= new JobDocAddressRequirement(DocAddressType.ControllingCustomer, ContactType.LocalClient); }
		}

		JobDocAddressRequirement lesseeAddressRequirement;

		#endregion

		#region ICYDYardUnitCollectionForRating

		IEnumerable<CYDYardUnitState> ICYDYardUnitsForRating.YardUnits => UnloadedYardUnits;

		IReadOnlyList<string> ICYDYardUnitsForRating.ChargeCodeGroupList => new[] { ChargeCodeGroupList.Codes.YardGateIn };

		OrgHeader ICYDYardUnitsForRating.Client => Client.Organisation;

		#endregion

		#region ICYDReceiveAdvice

		public KeyValuePair<string, string> MappedCommunityCode
		{
			get
			{
				if (Client?.Address != null)
				{
					return new KeyValuePair<string, string>(Client.Address.Header.OH_Code, Client.Address.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ContainerChainCommunityCode, string.Empty));
				}
				return new KeyValuePair<string, string>(string.Empty, string.Empty);
			}
		}

		#endregion
	}
}
