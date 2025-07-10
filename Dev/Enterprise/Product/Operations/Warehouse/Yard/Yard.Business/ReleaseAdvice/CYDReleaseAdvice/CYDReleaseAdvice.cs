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
using Enterprise.Warehouse.Yard.Busines;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[CodeProperty(Schema.YRE_JobNumber)]
	[UniversalDataContext(DataContextType.CYDReleaseAdvice)]
	public class CYDReleaseAdvice : AutoCYDReleaseAdvice,
		IDocAddresses,
		IDocumentSupportable,
		IEDocsProvider,
		IWorkflowProvider,
		IJobInvoicingPlugIn,
		IRatingSupporter,
		IStmNoteParent,
		ICYDJobInvoicingSupporter,
		ICYDYardUnitsForRating,
		ICYDReleaseAdvice
	{
		public CYDReleaseAdvice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Yard")]
		public override ZGuid YRE_WW_Yard { get => base.YRE_WW_Yard; set => base.YRE_WW_Yard = value; }

		public WhsWarehouse Yard
		{
			get => Factory.Load<WhsWarehouse>(YRE_WW_Yard);
		}

		public bool IsActive
		{
			get
			{
				var today = Yard.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow).Date;
				return today >= YRE_FromDate && today <= YRE_ToDate;
			}
		}

		#endregion

		#region RelatedCollections

		[ChildEditable]
		public CYDReleaseAdviceLineCollection ReleaseAdviceLineCollection
		{
			get
			{
				if (releaseAdviceLineCollection == null)
				{
					releaseAdviceLineCollection = new CYDReleaseAdviceLineCollection(this);
					releaseAdviceLineCollection.Load();
					RegisterEditableChildObject(releaseAdviceLineCollection);
				}

				return releaseAdviceLineCollection;
			}
		}
		CYDReleaseAdviceLineCollection releaseAdviceLineCollection;

		#endregion RelatedCollections

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return YRE_JobNumber.IsEmpty ?
					Res.GetString("d825d6e3-d3f3-4d7f-a36f-b4c3f8c16e4a", "Release Order") :
					Res.GetString("a480c600-186e-4f41-ade5-583cd5e42d85", "Release Order {0}", YRE_JobNumber);
			}
		}

		#endregion

		#region RelatedCollections

		public IEnumerable<CYDYardUnitState> LoadedYardUnits
		{
			get
			{
				if (loadedYardUnits == null)
				{
					var query = new ZDBOnlyQuery(typeof(CYDYardUnitState));
					var subquery = new ZDBOnlySubQuery(typeof(CYDReleaseAdviceLine), CYDYardUnitStateSchema.YUS_YEL_ReleaseLine);
					subquery.AddToFilter(CYDReleaseAdviceLineSchema.YEL_YRE_ReleaseAdvice, PK);
					query.AddSubQuery(subquery, JoinCondition.And);
					query.AddToFilter(CYDYardUnitStateSchema.YUS_LoadTime, SQLComparisonOperator.NotEqual, null);
					loadedYardUnits = Factory.Load<CYDYardUnitState>(query);
				}
				return loadedYardUnits;
			}
		}
		IEnumerable<CYDYardUnitState> loadedYardUnits;

		#endregion RelatedCollections

		#region IJobInvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get
			{
				return invoicingPlugIn ?? (invoicingPlugIn = new CYDJobInvoicingPlugIn<CYDReleaseAdvice>(this));
			}
		}
		IJobInvoicingPlugIn invoicingPlugIn;

		public IJobInvoicingSupporter InvoicingSupporter => new CYDJobInvoicingSupporter<CYDReleaseAdvice>(this);

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => YRE_JobNumber.ToString();

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

		#region IRatingSupporter

		public RatingAdaptersProvider AdaptersProvider => new CYDYardRatingAdaptersProvider<CYDReleaseAdvice>(this);

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
					DocAddressType.BookingPartyDocumentaryAddress,
					DocAddressType.ControllingCustomer,
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
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.CYDReleaseAdvice);
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

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CYDReleaseAdviceDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CYDReleaseAdviceProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.CYDReleaseAdviceWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress);

			var addressEntry = Factory.Load<JobDocAddress>(query).SingleOrDefault();

			if (addressEntry != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, addressEntry.OrganisationPK, ZGuid.Empty);
			}
			else
			{
				result.Add(ProcessTaskTemplateSchema.P0_OH_Client, ZGuid.Empty);
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
				noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
				noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);

				return noteTypes;
			}
		}

		#endregion

		#region Implementation

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			ReleaseAdviceLineCollection.RemoveAndDeleteAll();
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
			YRE_FromDate = ZDate.Today;
			YRE_ToDate = ZDate.Today.AddDays(7);
			YRE_Mode = "FCL";
			YRE_WW_Yard = Factory.NewWithValidTestData<WhsWarehouse>().PK;
		}
#endif

		#endregion

		#region ICYDJobInvoicingSupporter

		JobInvoicingConsumerType ICYDJobInvoicingSupporter.ConsumerType => JobInvoicingConsumerTypes.CYDReleaseAdvice;

		SecurityCheckpoint ICYDJobInvoicingSupporter.AuditSecurity => Env.Security.CYDReleaseAdviceAuditBilling;

		SecurityCheckpoint ICYDJobInvoicingSupporter.JobInvoicingSecurity => Env.Security.CYDReleaseAdviceJobInvoicing;

		OrgHeader ICYDJobInvoicingSupporter.DefaultClient => Client?.Organisation;

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

		IEnumerable<CYDYardUnitState> ICYDYardUnitsForRating.YardUnits => LoadedYardUnits;

		IReadOnlyList<string> ICYDYardUnitsForRating.ChargeCodeGroupList => new[] { ChargeCodeGroupList.Codes.YardGateOut };

		OrgHeader ICYDYardUnitsForRating.Client => Client.Organisation;

		#endregion

		#region ICYDReleaseAdvice

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
