using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
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
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	[CodeProperty(Schema.YAO_JobNumber)]
	public class CYDAdHocServiceOrder : AutoCYDAdHocServiceOrder,
		IDocAddresses,
		ICYDJobInvoicingSupporter,
		IJobInvoicingPlugIn,
		ICYDAdHocServiceOrder,
		IHaveServices,
		IWorkflowProvider,
		IDocumentSupportable,
		IEDocsProvider,
		IStmNoteParent
	{
		public CYDAdHocServiceOrder(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Facility")]
		[List("Lookups.Warehouses")]
		public override ZGuid YAO_WW_Facility { get => base.YAO_WW_Facility; set => base.YAO_WW_Facility = value; }

		public WhsWarehouse Facility
		{
			get => Factory.Load<WhsWarehouse>(YAO_WW_Facility);
		}

		[ChildEditable]
		public CYDAdHocServiceCollection AdHocServices
		{
			get
			{
				if (adHocServices == null)
				{
					adHocServices = new CYDAdHocServiceCollection(Factory, this);
					RegisterEditableChildObject(adHocServices);
				}
				return adHocServices;
			}
		}

		CYDAdHocServiceCollection adHocServices;

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

		public OrgHeader Org => Client.Organisation;
		#endregion

		#region IJobInvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn
		{
			get
			{
				return invoicingPlugIn ?? (invoicingPlugIn = new CYDJobInvoicingPlugIn<CYDAdHocServiceOrder>(this));
			}
		}
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => new CYDJobInvoicingSupporter<CYDAdHocServiceOrder>(this);

		public bool AllowInvoiceDeletion => true;

		public string JobNumber => YAO_JobNumber.ToString();

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

		#region ICYDJobInvoicingSupporter

		JobInvoicingConsumerType ICYDJobInvoicingSupporter.ConsumerType => JobInvoicingConsumerTypes.CYDAdHocServiceOrder;

		SecurityCheckpoint ICYDJobInvoicingSupporter.AuditSecurity => Env.Security.CYDAdHocServiceOrderAuditBilling;

		SecurityCheckpoint ICYDJobInvoicingSupporter.JobInvoicingSecurity => Env.Security.CYDAdHocServiceOrderJobInvoicing;

		OrgHeader ICYDJobInvoicingSupporter.DefaultClient => null;

		#endregion

		#region IWorkflowProvider

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CYDAdHocServiceOrderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.CYDAdHocServiceOrderWorkflowDescriptorCode;

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

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CYDAdHocServiceOrderDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.CYDAdHocServiceOrder);
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

		#region IStmNoteParent

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var notes = base.NoteTypesCore;
				notes.Add(PredefinedNoteTypes.Instance.BookingNotes);

				return notes;
			}
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("7c9e6679-7425-40de-944b-e07fc1f90ae7", "Ad Hoc Service Order");
			}
		}

		#endregion

		#region IHaveServices

		IHaveServices[] IHaveServices.DependentServiceParents => [];

		public ZString TableCode => CYDAdHocServiceOrderSchema.Constants.Prefix;

		JobServiceDependentCollection services;

		[ChildEditable]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new JobServiceDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
				}
				return services;
			}
		}

		public ZString TransportMode => null;

		public ZString ContainerMode => null;

		public BusinessObject ServiceParent => this;

		public bool NeedsServiceEvents => true;

		public IBranch ServiceBranch => Org?.Branch;

		public void JobServiceDeleted(ZGuid servicePK)
		{
		}

		#endregion

		#region Implementation

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			adHocServices?.DeleteAll();
			base.Delete();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			YAO_WW_Facility = Factory.NewWithValidTestData<WhsWarehouse>().PK;
		}
#endif

		#endregion
	}
}
