using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[CodeProperty(Schema.YTU_TransportationUnitID)]
	[UniversalDataContext(DataContextType.CYDTransportationUnit)]
	public class CYDTransportationUnit : AutoCYDTransportationUnit,
		IWorkflowProvider,
		IDocumentSupportable,
		IDocAddresses,
		IDocManagerSupport,
		IEDocsProvider,
		IStmNoteParent,
		IJobInvoicingPlugIn,
		ICYDJobInvoicingSupporter,
		IProcessHandlingInfoProvider,
		ICYDTransportationUnit,
		IRatingSupporter
	{
		public CYDTransportationUnit(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("WaitingBayLocation")]
		public override ZGuid YTU_WL_WaitingBayLocation { get => base.YTU_WL_WaitingBayLocation; set => base.YTU_WL_WaitingBayLocation = value; }

		[RelatedBusinessObject("Yard")]
		public override ZGuid YTU_WW_Yard { get => base.YTU_WW_Yard; set => base.YTU_WW_Yard = value; }

		public WhsWarehouse Yard
		{
			get => Factory.Load<WhsWarehouse>(YTU_WW_Yard);
		}

		public WhsLocation WaitingBayLocation
		{
			get => Factory.Load<WhsLocation>(YTU_WL_WaitingBayLocation);
		}

		public JobDocAddress TransportCompanyDocAddress
		{
			get
			{
				if (transportCompanyDocAddress == null)
				{
					transportCompanyDocAddress = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress));
				}
				return transportCompanyDocAddress;
			}
		}

		JobDocAddress transportCompanyDocAddress;

		#endregion

		#region RelatedCollections

		[ChildEditable]
		public CYDTransportationUnitDeliveryCollection Deliveries
		{
			get
			{
				if (deliveries == null)
				{
					deliveries = new CYDTransportationUnitDeliveryCollection(this);
					RegisterEditableChildObject(deliveries);
				}

				return deliveries;
			}
		}
		CYDTransportationUnitDeliveryCollection deliveries;

		[ChildEditable]
		public CYDTransportationUnitPickupCollection Pickups
		{
			get
			{
				if (pickups == null)
				{
					pickups = new CYDTransportationUnitPickupCollection(this);
					RegisterEditableChildObject(pickups);
				}

				return pickups;
			}
		}
		CYDTransportationUnitPickupCollection pickups;

		[ChildEditable]
		public CYDTransportationUnitReceiveYardUnitCollection ReceiveYardUnits
		{
			get
			{
				if (receiveYardUnits == null)
				{
					receiveYardUnits = new CYDTransportationUnitReceiveYardUnitCollection(this);
					RegisterEditableChildObject(receiveYardUnits);
				}

				return receiveYardUnits;
			}
		}
		CYDTransportationUnitReceiveYardUnitCollection receiveYardUnits;

		[ChildEditable]
		public CYDTransportationUnitReleaseYardUnitCollection ReleaseYardUnits
		{
			get
			{
				if (releaseYardUnits == null)
				{
					releaseYardUnits = new CYDTransportationUnitReleaseYardUnitCollection(this);
					RegisterEditableChildObject(releaseYardUnits);
				}

				return releaseYardUnits;
			}
		}
		CYDTransportationUnitReleaseYardUnitCollection releaseYardUnits;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return YTU_TransportationUnitID.IsEmpty ?
					Res.GetString("d825d6e3-d3f3-4d7f-a36f-b4c3f8c16e4b", "Transportation Unit") :
					Res.GetString("a480c600-186e-4f41-ade5-583cd5e42d86", "Transportation Unit {0}", YTU_TransportationUnitID);
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CYDTransportationUnitProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.CYDTransportationUnitWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

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

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get
			{
				return documentSupporter ?? (documentSupporter = new CYDTransportationUnitDocumentSupporter(this));
			}
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IDocAddresses Members

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

		public IReadOnlyList<DocAddressType> SupportedAddressTypes
		{
			get { return new[] { DocAddressType.TransportCompanyDocumentaryAddress }; }
		}

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		public SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			var jobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress);
			jobDocAddressRequirement.IsMandatory = true;
			jobDocAddressRequirement.CanOverride = false;
			return jobDocAddressRequirement;
		}

		public void DocAddressChanged(JobDocAddress docAddress)
		{
		}

		public void OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public bool CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		public OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.CYDTransportationUnit));
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsProvider

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

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

		#region IJobInvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn => invoicingPlugIn ??= new CYDJobInvoicingPlugIn<CYDTransportationUnit>(this);
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => new CYDJobInvoicingSupporter<CYDTransportationUnit>(this);

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => YTU_TransportationUnitID.ToString();

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

		JobInvoicingConsumerType ICYDJobInvoicingSupporter.ConsumerType => JobInvoicingConsumerTypes.CYDTransportationUnit;

		SecurityCheckpoint ICYDJobInvoicingSupporter.AuditSecurity => Env.Security.CYDTransportationUnitAuditBilling;

		SecurityCheckpoint ICYDJobInvoicingSupporter.JobInvoicingSecurity => Env.Security.CYDTransportationUnitJobInvoicing;

		OrgHeader ICYDJobInvoicingSupporter.DefaultClient => TransportCompanyDocAddress?.Organisation;

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public ProcessHandlingInfo ProcessHandlingInfo => new CYDTransportationUnitProcessHandlingInfoProvider(this);

		#endregion

		#region IRatingSupporter

		public RatingAdaptersProvider AdaptersProvider => new CYDTransportationUnitRatingAdapterProvider<CYDTransportationUnit>(this);

		#endregion

		#region ICYDTransportationUnit

		IActiveBusinessObjectCollection<ICYDDelivery> ICYDTransportationUnit.GetDeliveries => Deliveries;

		IActiveBusinessObjectCollection<ICYDPickup> ICYDTransportationUnit.GetPickups => Pickups;

		#endregion

		#region Implementation

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(YTU_TransportationUnitIDInfo, Env.NumberFountains.CYDTransportationUnitID);
			}

			base.OnSaving();
		}

		#region SupportsClone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new List<string>(base.GetPropertiesToExcludeFromCloning()) {
				CYDTransportationUnitSchema.Constants.YTU_TransportationUnitID,
				CYDTransportationUnitSchema.Constants.YTU_GateInTime,
				CYDTransportationUnitSchema.Constants.YTU_GateOutTime,
				CYDTransportationUnitSchema.Constants.YTU_WL_WaitingBayLocation
			};
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		#endregion
	}
}
