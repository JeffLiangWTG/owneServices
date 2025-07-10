using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;
using ICartageHelper = Enterprise.Freight.LocalCartage.Integration.ICartageHelper;

namespace Enterprise.Warehouse.Transit.Business
{
	/// <summary>
	/// Transit Warehouse is a GLOW development. This class only exists to support Universal XML, Workflow and Documents.
	/// </summary>
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
	[Packing.Business.Testing.PackingParentTestCase.TestExcludePackingParentHasTestCase]
#endif
	[UniversalDataContext(DataContextType.TransitDispatch)]
	[CodeProperty(WhsItemDispatchConsignment.Schema.WDC_JobID)]
	[DescriptionProperty(WhsItemDispatchConsignment.Schema.WDC_ConsignmentID)]
	[VisualizableDocumentsSupportable("DispatchConsignmentVisualizableDocumentSupporter")]
	public class WhsItemDispatchConsignment : AutoWhsItemDispatchConsignment,
		IConsignment,
		IDocAddresses,
		IJobNumber,
		IHaveEventsFromServices,
		IHavePortReferences,
		IPackingParent,
		IPackingParentSupportsImportingBookedDimensions,
		IPackingParentWithOutturn,
		IDocManagerSupport,
		IWorkflowProvider,
		IDocumentSupportable,
		IRatingSupporter,
		IJobInvoicingPlugIn,
		IEDocsProvider,
		ITransitConsignmentForRating,
		ITransitDispatchConsignment,
		IUniversalXMLNoteParent,
		IStmNoteParent,
		IAdditionalReferenceNumberTypeProvider,
		ITransitJobInvoicingPlugIn,
		ITransitJobForConsolCosting,
		IPackingParentWithAttachedParent,
		IHaveCusEntryNumReferences,
		IDtbBookingParent,
		IWhsItemDispatchConsignment,
		ICancellable,
		IWhsItemConsignmentOrderReferenceProvider
	{
		public WhsItemDispatchConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Objects

		#region PackageStates

		public WhsItemPackageStateCollection PackageStates
		{
			get { return packageStates ?? (packageStates = new WhsItemPackageStateCollection(this)); }
		}

		WhsItemPackageStateCollection packageStates;

		#endregion

		#region BookingPartyDocAddress

		public JobDocAddress BookingPartyDocAddress
		{
			get
			{
				if (bookingPartyDocAddress == null || bookingPartyDocAddress.IsDeleted)
				{
					bookingPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(BookingPartyDocAddressRequirement);
				}
				return bookingPartyDocAddress;
			}
		}
		JobDocAddress bookingPartyDocAddress;

		JobDocAddressRequirement BookingPartyDocAddressRequirement
		{
			get { return bookingPartyDocAddressRequirement ?? (bookingPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress, ContactType.LocalClient)); }
		}

		JobDocAddressRequirement bookingPartyDocAddressRequirement;

		#endregion

		#region CTODocAddress

		public JobDocAddress CTODocAddress
		{
			get
			{
				if (ctoDocAddress == null || ctoDocAddress.IsDeleted)
				{
					ctoDocAddress = DocAddresses.FindOrCreateWithRequirement(CTODocAddressRequirement);
				}
				return ctoDocAddress;
			}
		}
		JobDocAddress ctoDocAddress;

		JobDocAddressRequirement CTODocAddressRequirement
		{
			get
			{
				return ctoDocAddressRequirement ?? (ctoDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.DepartureCTOAddress, ContactType.CTO));
			}
		}
		JobDocAddressRequirement ctoDocAddressRequirement;

		#endregion

		#region ConsignorDocAddress

		public JobDocAddress ConsignorDocAddress
		{
			get
			{
				if (consignorDocAddress == null || consignorDocAddress.IsDeleted)
				{
					consignorDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsignorDocAddressRequirement);
				}
				return consignorDocAddress;
			}
		}
		JobDocAddress consignorDocAddress;

		JobDocAddressRequirement ConsignorDocAddressRequirement
		{
			get
			{
				return consignorDocAddressRequirement ?? (consignorDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.LocalCartageExporter, ContactType.Consignor));
			}
		}
		JobDocAddressRequirement consignorDocAddressRequirement;

		#endregion

		#region ConsigneeDocAddress

		public JobDocAddress ConsigneeDocAddress
		{
			get
			{
				if (consigneeDocAddress == null || consigneeDocAddress.IsDeleted)
				{
					consigneeDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeDocAddressRequirement);
				}
				return consigneeDocAddress;
			}
		}
		JobDocAddress consigneeDocAddress;

		JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get
			{
				return consigneeDocAddressRequirement ??
				(consigneeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress, ContactType.Consignee));
			}
		}
		JobDocAddressRequirement consigneeDocAddressRequirement;

		#endregion

		#region ClientRequestedBillToPartyDocAddress

		public JobDocAddress ClientRequestedBillToPartyDocAddress
		{
			get
			{
				if (clientRequestedBillToPartyDocAddress == null || clientRequestedBillToPartyDocAddress.IsDeleted)
				{
					clientRequestedBillToPartyDocAddress = DocAddresses.FindOrCreateWithRequirement(ClientRequestedBillToPartyDocAddressRequirement);
				}
				return clientRequestedBillToPartyDocAddress;
			}
		}
		JobDocAddress clientRequestedBillToPartyDocAddress;

		JobDocAddressRequirement ClientRequestedBillToPartyDocAddressRequirement
		{
			get
			{
				return clientRequestedBillToPartyDocAddressRequirement ??
				(clientRequestedBillToPartyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ClientRequestedBillingParty, ContactType.LocalClient));
			}
		}
		JobDocAddressRequirement clientRequestedBillToPartyDocAddressRequirement;

		public ZString BillingPartyCompanyName
		{
			get
			{
				return TransitWarehouseHelper.BillingPartyCompanyName(JobHeader, ClientRequestedBillToPartyDocAddress);
			}
		}

		#endregion

		#region DeliveryDocAddress

		public JobDocAddress DeliveryDocAddress
		{
			get
			{
				if (deliveryDocAddress == null || deliveryDocAddress.IsDeleted)
				{
					deliveryDocAddress = DocAddresses.FindOrCreateWithRequirement(DeliveryDocAddressRequirement);
				}
				return deliveryDocAddress;
			}
		}
		JobDocAddress deliveryDocAddress;

		JobDocAddressRequirement DeliveryDocAddressRequirement => deliveryDocAddressRequirement ?? (deliveryDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneePickupDeliveryAddress));
		JobDocAddressRequirement deliveryDocAddressRequirement;

		#endregion

		#region TransportCompanyDocAddress

		public JobDocAddress TransportCompanyDocAddress
		{
			get
			{
				if (transportCompanyDocAddress == null || transportCompanyDocAddress.IsDeleted)
				{
					transportCompanyDocAddress = DocAddresses.FindOrCreateWithRequirement(TransportCompanyDocAddressRequirement);
				}
				return transportCompanyDocAddress;
			}
		}
		JobDocAddress transportCompanyDocAddress;

		JobDocAddressRequirement TransportCompanyDocAddressRequirement => transportCompanyDocAddressRequirement ?? (transportCompanyDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress, ContactType.LocalTransport));
		JobDocAddressRequirement transportCompanyDocAddressRequirement;

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(TransitWarehouseNoteHelper.GetNoteTypes());
				types.Add(PredefinedNoteTypes.Instance.UnloadLoadNotes);
				types.Add(PredefinedNoteTypes.Instance.CIN750MessageNotes);
				types.Add(PredefinedNoteTypes.Instance.CRESAMessageNotes);

				return types;
			}
		}

		#endregion

		#region ConsignmentOrderReferences

		public WhsItemConsignmentOrderReferenceCollection OrderReferences => orderReferences ??= new WhsItemConsignmentOrderReferenceCollection(this);

		WhsItemConsignmentOrderReferenceCollection orderReferences;

		public IActiveBusinessObjectCollection<IWhsItemConsignmentOrderReference> WhsItemConsignmentOrderReferences => OrderReferences;

		#endregion

		#endregion

		#region Properties

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WDC_SystemCreateTimeUtc
		{
			get { return base.WDC_SystemCreateTimeUtc; }
			set { base.WDC_SystemCreateTimeUtc = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDC_SystemCreateUser
		{
			get { return base.WDC_SystemCreateUser; }
			set { base.WDC_SystemCreateUser = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WDC_SystemLastEditTimeUtc
		{
			get { return base.WDC_SystemLastEditTimeUtc; }
			set { base.WDC_SystemLastEditTimeUtc = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDC_SystemLastEditUser
		{
			get { return base.WDC_SystemLastEditUser; }
			set { base.WDC_SystemLastEditUser = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDC_JobID
		{
			get { return base.WDC_JobID; }
			set { base.WDC_JobID = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WDC_ParentID
		{
			get { return base.WDC_ParentID; }
			set { base.WDC_ParentID = value; }
		}

		[List("Lookups.Directions")]
		public override ZString WDC_Direction
		{
			get { return base.WDC_Direction; }
			set { base.WDC_Direction = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDC_ParentTableCode
		{
			get { return base.WDC_ParentTableCode; }
			set { base.WDC_ParentTableCode = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBool WDC_IsAuthorizedForDispatch
		{
			get { return base.WDC_IsAuthorizedForDispatch; }
			set { base.WDC_IsAuthorizedForDispatch = value; }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDC_ConsignmentID
		{
			get { return base.WDC_ConsignmentID; }
			set { base.WDC_ConsignmentID = value; }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WDC_JobID.IsEmpty ?
					Res.GetString("C5F42206-B7BF-46E5-95A6-22D8EE6E7EC7", "Dispatch Consignment") :
					Res.GetString("C7AF6068-7A1D-44FB-9D5C-BF9C6CD2E689", "Dispatch Consignment {0}", WDC_JobID);
			}
		}

		[RelatedBusinessObject("Warehouse")]
		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WDC_WW_Warehouse { get => base.WDC_WW_Warehouse; set => base.WDC_WW_Warehouse = value; }

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WDC_WW_Warehouse); }
		}

		public ZBool IsAnyRelatedRCNBlind => PackageStates.Any(p => !p.WPS_WRC_TransitReceiveConsignment.IsEmpty && p.ReceiveConsignment.IsBlind);

		#endregion

		#region AdditionalReferenceNumbers

		[ChildEditable]
		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollectionProvider>();
					additionalReferenceNumbers = provider.GetCollection(this);

					var additionalReferenceNumbersBusinessObjectCollection = additionalReferenceNumbers as BusinessObjectCollection;
					if (additionalReferenceNumbersBusinessObjectCollection != null)
					{
						additionalReferenceNumbersBusinessObjectCollection.Load();
					}

					RegisterEditableChildObject(additionalReferenceNumbers);
				}

				return additionalReferenceNumbers;
			}
		}

		ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		public ZString AdditionalReferenceNumbersSingleLine
		{
			get
			{
				return TransitWarehouseHelper.AdditionalReferenceNumbersSingleLine(AdditionalReferenceNumbers);
			}
		}

		#endregion

		#region PortReferences

		public IPortReferenceCollection PortReferences
		{
			get
			{
				if (portReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<IPortReferenceCollectionProvider>();
					portReferenceNumbers = provider.GetCollection(this);
				}

				return portReferenceNumbers;
			}
		}

		IPortReferenceCollection portReferenceNumbers;

		#endregion

		#region CusEntryNumReferences

		public ICusEntryNumReferenceCollection CusEntryNumReferences
		{
			get
			{
				if (cusEntryNumReferences == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumReferenceCollectionProvider>();
					cusEntryNumReferences = provider.GetCollection(this);
				}

				return cusEntryNumReferences;
			}
		}

		ICusEntryNumReferenceCollection cusEntryNumReferences;

		#endregion

		#region ShipmentNumber

		public ZString ShipmentNumber
		{
			get
			{
				var shipmentAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber);
				return shipmentAdditionalReference != null && shipmentAdditionalReference.CE_EntryNum != ""
					? shipmentAdditionalReference.CE_EntryNum
					: ZString.Empty;
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			OrderReferences.DeleteAll();
			base.Delete();
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region FormarttedReference

		public ZString FormattedReference => TransitWarehouseHelper.GetFormattedReferenceString(WDC_ConsignmentID, WDC_JobID);

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		// interfaces

		#region IDocAddresses Members

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

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return ""; }
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new[] { DocAddressType.LocalCartageExporter, DocAddressType.ConsigneeDocumentaryAddress, DocAddressType.BookingPartyDocumentaryAddress, DocAddressType.ClientRequestedBillingParty, DocAddressType.ConsigneePickupDeliveryAddress, DocAddressType.TransportCompanyDocumentaryAddress, DocAddressType.DepartureCTOAddress }; }
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return WDC_JobID; }
		}

		#endregion

		#region JobHeader

		public JobHeader JobHeader
		{
			get { return new JobHeader.Loader(this).Load(); }
		}

		#endregion

		#region IConsignment Memebers

		IWhsWarehouse IConsignment.Warehouse => Warehouse;
		IJobDocAddress IConsignment.BookingPartyDocAddress => BookingPartyDocAddress;

		public ZString MasterBillNumber
		{
			get
			{
				string masterBillNumber;
				var masterBillAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.MasterBill);
				if (masterBillAdditionalReference != null && masterBillAdditionalReference.CE_EntryNum != "")
				{
					masterBillNumber = masterBillAdditionalReference.CE_EntryNum;
				}
				else
				{
					masterBillNumber = PackageStates
						.Select(p => p.DispatchLoadList)
						.FirstOrDefault(dll => dll != null && !dll.MasterBillNumber.IsEmpty)?.MasterBillNumber;
				}

				return masterBillNumber;
			}
		}

		public ZString HouseBillNumber
		{
			get
			{
				return WDC_HouseBillNumber;
			}
		}

		ZString IConsignment.JobID => WDC_JobID;
		ZString IConsignment.TransportMode => WDC_TransportMode;
		ZString IConsignment.Direction => WDC_Direction;

		#endregion

		#region IHaveServices

		ZString IHaveServices.TableCode => WhsItemDispatchConsignmentSchema.Constants.Prefix;

		[ChildEditable(true)]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new WhsJobServiceDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
				}
				return services;
			}
		}
		WhsJobServiceDependentCollection services;

		ZString IHaveServices.TransportMode => "";

		ZString IHaveServices.ContainerMode => "";

		IHaveServices[] IHaveServices.DependentServiceParents => Array.Empty<IHaveServices>();

		BusinessObject IHaveServices.ServiceParent => this;

		bool IHaveServices.NeedsServiceEvents => true;

		IEnumerable<KeyValuePair<string, string>> IHaveEventsFromServices.ReferenceParameters
		{
			get
			{
				if (referenceParametersForServiceEvents == null)
				{
					referenceParametersForServiceEvents = new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, CargoWise.EventReference.Constants.Facilities.Code.Depot),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, Warehouse.WarehouseAddress?.City ?? string.Empty),
					};
				}

				return referenceParametersForServiceEvents;
			}
		}
		IEnumerable<KeyValuePair<string, string>> referenceParametersForServiceEvents;

		void IHaveServices.JobServiceDeleted(ZGuid servicePK)
		{
			if (Services.Any(s => s.PK == servicePK))
			{
				var unloadTaskQuery = new ZQuery(WhsItemUnloadTaskSchema.WUT_ES_ActiveJobService, servicePK);

				var unloadTasksWithCurrentConsignment = Factory.Load<WhsItemUnloadTask>(unloadTaskQuery);
				unloadTasksWithCurrentConsignment.ForEach(unloadTask =>
				{
					unloadTask.SetValue(WhsItemUnloadTaskSchema.WUT_ES_ActiveJobService, null);
				});
			}
		}

		IBranch IHaveServices.ServiceBranch => Warehouse?.RelatedCompanyBranch;

		#endregion

		#region InvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn => invoicingPlugIn ?? (invoicingPlugIn = new TransitJobInvoicingPlugIn<WhsItemDispatchConsignment>(this));
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => InvoicingPlugIn.InvoicingSupporter;

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			if (WDC_JobID.IsEmpty)
			{
				WDC_JobID = Env.NumberFountains.TransitWarehouseDispatchConsignmentID.GetNextFormatted(Factory);
			}
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			CartageHelper.AttachCartageJobsToParentJob(job, PK);

			ConsignmentJobHelper.AttachLTConsignmentJobsToParentJob(job, PK);

			TransitWarehouseConsignmentJobHelper.AttachConsignmentJobToParentJob(job, WDC_ParentID, WDC_ParentTableCode);
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		#endregion

		#endregion

		#region ITransitJobInvoicingPlugIn

		JobInvoicingConsumerType ITransitJobInvoicingPlugIn.ConsumerType => JobInvoicingConsumerTypes.TransitDispatch;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.AuditSecurity => Env.Security.WhsItemDispatchConsignmentAuditBilling;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.JobInvoicingSecurity => Env.Security.WhsItemDispatchConsignmentJobInvoicing;

		IJobInvoicingSupporter ITransitJobInvoicingPlugIn.InvoicingSupporter => new WhsItemConsignmentInvoicingSupporter<WhsItemDispatchConsignment>(this);

		#endregion

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot
		{
			get { return false; }
		}

		#endregion

		#region PackageJob

		public PkgPackageJob PackageJob
		{
			get { return packageJob ?? (packageJob = GetPackageJob()); }
		}

		PkgPackageJob GetPackageJob()
		{
			var packageJobToReturn = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(this);
			return packageJobToReturn;
		}

		PkgPackageJob packageJob;

		#endregion

		#region IPackingParent

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			return new PackageActionStrategy(package);
		}

		ControllerID IPackingParent.ControllerID
		{
			get { return null; }
		}

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
		{
			return ZString.Empty;
		}

		DocumentOptions IPackingParent.DocumentOptions
		{
			get { return DocumentOptions.ShowBasicLabelOnly; }
		}

		bool IPackingParent.IsAutoPrintAllowed
		{
			get { return false; }
		}

		bool IPackingParent.IsParentJobFinalised
		{
			get { return false; }
		}

		bool IPackingParent.IsPackingJobReadOnly
		{
			get { return false; }
		}

		bool IPackingParent.IsScanEventsVisible
		{
			get { return false; }
		}

		ZString IPackingParent.JobDescription
		{
			get { return ""; }
		}

		ZString IPackingParent.JobNo
		{
			get { return WDC_JobID; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return ZString.Empty; }
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
		}

		void IPackingParent.OnPackageJobReleased()
		{
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob packageJob)
		{
		}

		void IPackingParent.OnPackageDelete(PkgPackage package)
		{
			var packageState = PackageStates.SingleOrDefault(p => p.WPS_KP_Package == package.PK);
			packageState?.Delete();
		}

		ZString IPackingParent.CarrierServiceLevelCode(PkgPackage package)
		{
			return "";
		}

		OrgHeader IPackingParent.CarrierBookingAgent => null;

		OrgHeader IPackingParent.GetCarrier(PkgPackage package)
		{
			return null;
		}

		ZString IPackingParent.TransportReference { get => ZString.Empty; set { } }

		bool IPackingParent.IsLoosePackageIDsSupported
		{
			get { return false; }
		}

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
		}

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		ParentJobType IPackingParent.ParentJobType => ParentJobType.None;

		PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.OuterWithLooseID;

		bool IPackingParent.CanReleasePackage(PkgPackage package) => true;

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
		}

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new WhsItemDispatchConsignmentDocManagerInfo(this, Constants.DocManagerCodes.TransitDispatchConsignment);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsItemDispatchConsignmentDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new WhsItemConsignmentRatingAdaptersProvider<WhsItemDispatchConsignment>(this); }
		}

		#endregion

		#region ITransitConsignmentForRating Members

		string ITransitConsignmentForRating.ServiceLevel => WDC_RS_NKServiceLevel;

		FreightMode? ITransitJobForRating.FreightMode
		{
			get
			{
				if (!WDC_TransportMode.IsEmpty)
				{
					return TransitWarehouseHelper.GetFreightModeByTransportMode(WDC_TransportMode);
				}
				else
				{
					return null;
				}
			}
		}

		string ITransitJobForRating.ChargeCodeGroup => ChargeCodeGroupList.Codes.TRWDispatch;

		ZDateTime ITransitJobForRating.ExpectedArrivalDate =>
			PackageStates.Any() && PackageStates.Any(p => p.WPS_WRC_TransitReceiveConsignment != ZGuid.Empty && p.ReceiveConsignment.WRC_ExpectedArrivalTime != ZDateTime.Empty) ? PackageStates.Where(p => p.WPS_WRC_TransitReceiveConsignment != ZGuid.Empty).Select(p => p.ReceiveConsignment.WRC_ExpectedArrivalTime).Distinct().OrderBy(o => o).FirstOrDefault()
			: ZDateTime.Now;

		ZDateTime ITransitJobForRating.ExpectedDepartureDate =>
			PackageStates.Any() && PackageStates.Any(p => p.WPS_WRC_TransitReceiveConsignment != ZGuid.Empty && p.ReceiveConsignment.WRC_ExpectedDispatchTime != ZDateTime.Empty) ? PackageStates.Where(p => p.WPS_WRC_TransitReceiveConsignment != ZGuid.Empty).Select(p => p.ReceiveConsignment.WRC_ExpectedDispatchTime).Distinct().OrderByDescending(o => o).FirstOrDefault()
			: ZDateTime.Now;

		IEnumerable<TransitPackageForRatingInfo> ITransitJobForRating.TransitPackagesForRating
		{
			get
			{
				if (transitPackagesForRating == null)
				{
					transitPackagesForRating = PackageStates.Where(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Departed)
						.Select(p => new TransitPackageForRatingInfo(p.PK, p.WPS_UnitType, p.Package.KP_PackageQty, p.Package.KP_F3_NKPackType, p.Package.KP_RH_NKCommodityCode,
							p.Package.KP_Weight, p.Package.KP_WeightUQ, p.Package.KP_Volume, p.Package.KP_VolumeUQ,
							p.HasDangerousGoods, p.WPS_UnloadedTime, ZDateTimeOffset.Empty, p.WPS_LoadedTime));
				}

				return transitPackagesForRating;
			}
		}

		IEnumerable<TransitPackageForRatingInfo> transitPackagesForRating;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.TransitDispatchConsignment; }
		}

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsItemDispatchConsignmentProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, ConsigneeDocAddress.OrganisationPK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_WW, WDC_WW_Warehouse, ZGuid.Empty);
			return result;
		}

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IPackingParentWithOutturn

		IContainerView IPackingParentWithOutturn.GetContainerView(PkgPackageContainer container) => new ContainerViewForDispatch(container);

		IOutturnProvider IPackingParentWithOutturn.GetOutturnProvider(PkgPackage package) => new OutturnProviderForDCN(package);

		(string ContainerNumber, PkgPackageContainer Container) IPackingParentWithOutturn.GetParentContainer(PkgPackage package)
		{
			var packageState = PackageStates.SingleOrDefault(p => p.WPS_KP_Package == package.PK);

			if (packageState?.DispatchTransportationUnit != null && !string.IsNullOrEmpty(packageState?.DispatchTransportationUnit.ContainerNumber))
			{
				var container = packageState.DispatchTransportationUnit.Container;
				return (packageState.DispatchTransportationUnit.WDH_VehicleReference, container);
			}

			return (null, null);
		}

		int IPackingParentWithOutturn.TotalNumberOfPiecesOutturned => PackageStates.Sum(p => p.Package.KP_PackageQty);

		#endregion

		#region IPackingParentWithAttachedParent

		ZString IPackingParentWithAttachedParent.GetAttachedJobNumber(PkgPackage package) => TransitWarehouseHelper.GetAttachedJobNumber(package);

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.WhsTransitDispatchConsignment; }
		}

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobDescription
		{
			get { return HumanReadableNameCore; }
		}

		ZString IRelatedJob.JobNumber
		{
			get => WDC_JobID;
		}

		ZString IRelatedJob.JobStatus
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IDtbBookingParent Members

		ZString IDtbBookingParent.JobTypeDescription
		{
			get { return Res.GetString("b6424ad8-4a74-489c-8b91-b937ddfc9208", "Dispatch Consignment"); }
		}

		ZString IDtbBookingParent.JobType
		{
			get { return ((IWorkflowProvider)this).WorkflowType; }
		}

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections()
		{
			return new DtbBookingDirection[] { DtbBookingDirection.DLV };
		}

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob
		{
			get { return this; }
		}

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
			foreach (IDtbBooking booking in bookings)
			{
				var universalJobLinkCreatorForDCN = ObjectFactory.Get<IUniversalJobLinkCreator>("IUniversalJobLinkCreator", Factory, this, DataContextType.TransportBooking);
				universalJobLinkCreatorForDCN.CreateUniversalJobLink(booking as BusinessObject);
				var universalJobLinkCreatorForTB = ObjectFactory.Get<IUniversalJobLinkCreator>("IUniversalJobLinkCreator", Factory, booking, DataContextType.TransitDispatch);
				universalJobLinkCreatorForTB.CreateUniversalJobLink(this);
			}
			Factory.Save();
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule
		{
			get { return false; }
		}

		bool IDtbBookingParent.RequiresMultiContainerBooking => true;

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK
		{
			get
			{
				if (IsParentAnIDtbBookingParent())
				{
					return WDC_ParentID;
				}
				else
				{
					return PK;
				}
			}
		}

		public string BookingParentTablePrefix
		{
			get
			{
				if (IsParentAnIDtbBookingParent())
				{
					return WDC_ParentTableCode;
				}
				else
				{
					return TablePrefix;
				}
			}
		}

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		bool IsParentAnIDtbBookingParent()
		{
			return ValidWDCParentTableCodesThatAreIDtbBookingParents.Contains(WDC_ParentTableCode);
		}

		List<string> ValidWDCParentTableCodesThatAreIDtbBookingParents
		{
			get
			{
				return new List<string>() { JobShipmentSchema.Constants.Prefix };
			}
		}

		#endregion

		#region BusinessObjectsWithRelatedEvents

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));

				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var documentData = documentDataLoader.Load(this);
				if (documentData != null)
				{
					result.AddRange(documentData.Cast<BusinessObject>());
				}

				return result.ToArray();
			}
		}

		#endregion

		#region IAdditionalReferenceNumberTypeProvider

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			var isAdditionalReferenceNumber = category == CusEntryNumber.Categories.AdditionalReferenceNumber;

			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (isAdditionalReferenceNumber)
			{
				result = WarehouseDataRegistry.Instance.AdditionalReferenceType.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).GetCodeDescriptionPairList();
			}

			return result;
		}

		#endregion

		#region ITransitJobForConsolCosting

		ZString ITransitJobForConsolCosting.HouseBillNumber => HouseBillNumber.IsEmpty ? WDC_ConsignmentID : HouseBillNumber;

		RefUNLOCO ITransitJobForConsolCosting.Destination => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, WDC_RL_NKDestination);

		IEnumerable<WhsItemPackageState> ITransitJobForConsolCosting.PackageStatesForConsolCosting => packageStatesForConsolCosting ?? (packageStatesForConsolCosting = PackageStates.Where(t =>
			t.WPS_Status == TransitWarehouseStatuses.Codes.FreightLoaded ||
			t.WPS_Status == TransitWarehouseStatuses.Codes.Departed ||
			t.WPS_Status == TransitWarehouseStatuses.Codes.Finalized));
		IEnumerable<WhsItemPackageState> packageStatesForConsolCosting;

		#endregion

		#region JobHelper
		ICartageHelper CartageHelper
		{
			get { return cartageHelper ?? (cartageHelper = ObjectFactory.Get<ICartageHelper>()); }
		}
		ICartageHelper cartageHelper;

		IConsignmentJobHelper ConsignmentJobHelper
		{
			get { return consignmentJobHelper ?? (consignmentJobHelper = ObjectFactory.Get<IConsignmentJobHelper>()); }
		}
		IConsignmentJobHelper consignmentJobHelper;
		#endregion

		#region ICancellable

		public bool IsCancelled { get; set; }

		public bool IsCancelledHasChanged => true;

		public string CanCancel() => null;

		public string CanReactivate() => null;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsItemDispatchConsignmentFetchStrategy(this);
		}

		#endregion

		#region GetOuterPackages

		/// <summary>
		/// This property is used to get standalone packages or overpacks and first level inner packed in handling unit which assigned to dispatch consignment
		/// And the result is used by document supporter
		/// </summary>
		public IEnumerable<PkgPackage> OuterPackages => PackageStates.Any() ? OuterPackageStates.Select(p => p.Package) : new List<PkgPackage>();

		public IEnumerable<WhsItemPackageState> OuterPackageStates
		{
			get
			{
				if (PackageStates.Any())
				{
					return PackageStates.Where(p => p.HandlingUnit == null
					|| p.HandlingUnit.WPS_UnitType == PackageStateUnitType.Codes.HandlingUnit
					|| p.HandlingUnit.WPS_UnitType == PackageStateUnitType.Codes.AirULDContainer
					|| p.HandlingUnit.WPS_UnitType == PackageStateUnitType.Codes.SeaContainer);
				}
				else
				{
					return new List<WhsItemPackageState>();
				}
			}
		}

		#endregion

		#region IsSplit

		public ZBool IsSplit
		{
			get
			{
				if (!WDC_ParentID.IsEmpty)
				{
					var dcnQuery = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ParentID, WDC_ParentID);
					dcnQuery.AddToFilter(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, Warehouse.PK));
					return Factory.Load<IWhsItemDispatchConsignment>(dcnQuery).Length > 1;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion
	}
}
