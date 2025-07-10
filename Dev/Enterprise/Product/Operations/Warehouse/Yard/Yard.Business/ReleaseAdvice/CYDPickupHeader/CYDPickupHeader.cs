using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[CodeProperty(Schema.YPH_JobNumber)]
	public class CYDPickupHeader : AutoCYDPickupHeader,
		IDocumentSupportable,
		IDocAddresses,
		IDocManagerSupport,
		IEDocsProvider,
		IJobInvoicingPlugIn,
		IStmNoteParent,
		IWorkflowProvider
	{
		public CYDPickupHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Yard")]
		public override ZGuid YPH_WW_Yard { get => base.YPH_WW_Yard; set => base.YPH_WW_Yard = value; }

		public WhsWarehouse Yard
		{
			get => Factory.Load<WhsWarehouse>(YPH_WW_Yard);
		}

		#endregion

		#region RelatedCollections

		[ChildEditable]
		public CYDPickupCollection PickupCollection
		{
			get
			{
				if (pickupCollection == null)
				{
					pickupCollection = new CYDPickupCollection(this);
					pickupCollection.Load();
					RegisterEditableChildObject(pickupCollection);
				}

				return pickupCollection;
			}
		}
		CYDPickupCollection pickupCollection;

		#endregion RelatedCollections

		#region IJobInvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get
			{
				return invoicingPlugIn ?? (invoicingPlugIn = new CYDJobInvoicingPlugIn<CYDPickupHeader>(this));
			}
		}
		IJobInvoicingPlugIn invoicingPlugIn;

		public IJobInvoicingSupporter InvoicingSupporter { get; set; }

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => YPH_JobNumber.ToString();

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CYDPickupHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.CYDPickupHeaderWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_WW, YPH_WW_Yard, null);
			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

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
					DocAddressType.TransportCompanyDocumentaryAddress,
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

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CYDPickupHeaderDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IStmNoteParent

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var noteTypes = base.NoteTypesCore;
				noteTypes.Add(PredefinedNoteTypes.Instance.PickupInstructionsNote);
				noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);

				return noteTypes;
			}
		}

		#endregion

		#region Implementation

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("b3e76a15-e30b-444b-a7a5-c2091bd029f0", "Bulk runs out");
			}
		}

		#endregion

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			PickupCollection.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(YPH_JobNumberInfo, Env.NumberFountains.CYDPickupHeaderJobNumber);
			}

			base.OnSaving();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			YPH_WW_Yard = warehouse.PK;
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
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.CYDPickupHeader);
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

		#region DropOffLocation

		public JobDocAddress DropOffLocation
		{
			get
			{
				if (dropOffLocation == null || dropOffLocation.IsDeleted)
				{
					dropOffLocation = DocAddresses.FindOrCreateWithRequirement(DropOffLocationRequirement);
				}
				return dropOffLocation;
			}
		}
		JobDocAddress dropOffLocation;

		JobDocAddressRequirement DropOffLocationRequirement
		{
			get { return dropOffLocationRequirement ??= new JobDocAddressRequirement(DocAddressType.DropOffAddress, ContactType.LocalClient); }
		}

		JobDocAddressRequirement dropOffLocationRequirement;

		#endregion

		#region TransportProvider

		public JobDocAddress TransportProvider
		{
			get
			{
				if (transportProvider == null || transportProvider.IsDeleted)
				{
					transportProvider = DocAddresses.FindOrCreateWithRequirement(TransportProviderRequirement);
				}
				return transportProvider;
			}
		}
		JobDocAddress transportProvider;

		JobDocAddressRequirement TransportProviderRequirement
		{
			get { return transportProviderRequirement ??= new JobDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress, ContactType.LocalClient); }
		}

		JobDocAddressRequirement transportProviderRequirement;

		#endregion
	}
}
