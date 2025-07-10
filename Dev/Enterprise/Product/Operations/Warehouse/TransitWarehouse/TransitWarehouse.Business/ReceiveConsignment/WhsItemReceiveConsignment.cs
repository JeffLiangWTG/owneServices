using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
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
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	/// <summary>
	/// Transit Warehouse is a GLOW development. This class only exists to support Universal XML, eDocs and Documents.
	/// </summary>
	[UniversalDataContext(DataContextType.TransitReceive)]
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
	[Packing.Business.Testing.PackingParentTestCase.TestExcludePackingParentHasTestCase]
#endif
	[CodeProperty(WhsItemReceiveConsignment.Schema.WRC_JobID)]
	[DescriptionProperty(WhsItemReceiveConsignment.Schema.WRC_ConsignmentID)]
	[VisualizableDocumentsSupportable("ReceiveConsignmentVisualizableDocumentSupporter")]
	public class WhsItemReceiveConsignment : AutoWhsItemReceiveConsignment,
		IConsignment,
		IDocAddresses,
		IDocumentSupportable,
		IDocManagerSupport,
		IEDocsProvider,
		IJobNumber,
		IJobInvoicingPlugIn,
		IHaveEventsFromServices,
		IHavePortReferences,
		IPackingParentWithTransportCompanyAndBookingParty,
		IPackingParentSupportsImportingUnassignedPackageIdsAsPackages,
		IPackingParentSupportsImportingBookedDimensions,
		IRatingSupporter,
		ITransportParentCommon,
		ITransitConsignmentForRating,
		ITransitReceiveConsignment,
		IWorkflowProvider,
		IUniversalXMLNoteParent,
		IStmNoteParent,
		IPackingParentWithOutturn,
		IRoutingSupport,
		ITransportParent,
		ITransportChangeNotifier,
		IAdditionalReferenceNumberTypeProvider,
		ITransitJobInvoicingPlugIn,
		ITransitJobForConsolCosting,
		IPackingParentWithAttachedParent,
		IHaveCusEntryNumReferences,
		IWhsItemReceiveConsignment,
		IWhsItemConsignmentOrderReferenceProvider
	{
		public WhsItemReceiveConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region WRC_WW_IntendedWarehouse

		// This attribute is not needed but tests will fail without it.
		[RelatedBusinessObject("Warehouse")]
		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WRC_WW_IntendedWarehouse
		{
			get { return base.WRC_WW_IntendedWarehouse; }
			set { base.WRC_WW_IntendedWarehouse = value; }
		}

		#endregion

		#region WRC_SystemCreateTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WRC_SystemCreateTimeUtc
		{
			get { return base.WRC_SystemCreateTimeUtc; }
			set { base.WRC_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region WRC_SystemCreateUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRC_SystemCreateUser
		{
			get { return base.WRC_SystemCreateUser; }
			set { base.WRC_SystemCreateUser = value; }
		}

		#endregion

		#region WRC_SystemLastEditTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WRC_SystemLastEditTimeUtc
		{
			get { return base.WRC_SystemLastEditTimeUtc; }
			set { base.WRC_SystemLastEditTimeUtc = value; }
		}

		#endregion

		#region WRC_SystemLastEditUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRC_SystemLastEditUser
		{
			get { return base.WRC_SystemLastEditUser; }
			set { base.WRC_SystemLastEditUser = value; }
		}

		#endregion

		#region WRC_ParentID

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WRC_ParentID
		{
			get { return base.WRC_ParentID; }
			set { base.WRC_ParentID = value; }
		}

		#endregion

		#region WRC_ParentTableCode

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRC_ParentTableCode
		{
			get { return base.WRC_ParentTableCode; }
			set { base.WRC_ParentTableCode = value; }
		}

		#endregion

		#region WRC_CompleteTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WRC_CompleteTime
		{
			get { return base.WRC_CompleteTime; }
			set
			{
				if (WRC_CompleteTime.IsEmpty)
				{
					base.WRC_CompleteTime = value;

					if (!value.IsEmpty)
					{
						Logs.AddNew(
							Events.ItemDocumentJobFinalised,
							WRC_ConsignmentID,
							value.ToDateTime(),
							new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, (NoResString)"RCN Completed"),
							new KeyValuePair<string, string>(EventReferenceParameters.Codes.Facility, Facilities.Code.Depot),
							new KeyValuePair<string, string>(EventReferenceParameters.Codes.Location, Warehouse.WarehouseAddress != null ? (string)Warehouse.WarehouseAddress.OA_City : string.Empty),
							new KeyValuePair<string, string>(EventReferenceParameters.Codes.Warehouse, Warehouse.WW_WarehouseCode));
					}
				}
			}
		}

		#endregion

		#region WRC_JobID

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRC_JobID
		{
			get { return base.WRC_JobID; }
			set { base.WRC_JobID = value; }
		}

		#endregion

		#region WRC_Direction

		[List("Lookups.Directions")]
		public override ZString WRC_Direction
		{
			get { return base.WRC_Direction; }
			set { base.WRC_Direction = value; }
		}

		#endregion

		#region WRC_CustomsStatus

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRC_CustomsStatus
		{
			get { return base.WRC_CustomsStatus; }
			set { base.WRC_CustomsStatus = value; }
		}

		#endregion

		#region WRC_CustomsStatusReason

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRC_CustomsStatusReason
		{
			get { return base.WRC_CustomsStatusReason; }
			set { base.WRC_CustomsStatusReason = value; }
		}

		#endregion

		#region IsBlind

		public ZBool IsBlind => WRC_ParentID == ZGuid.Empty;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WRC_JobID.IsEmpty ?
					Res.GetString("F2DC69A1-B59A-4FD8-B41B-E6E5B279D859", "Receive Consignment") :
					Res.GetString("E4D20DB3-B27B-42E4-84BC-F4A33B35B106", "Receive Consignment {0}", WRC_JobID);
			}
		}

		#endregion

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

		#region Related Objects

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.Load<WhsWarehouse>(WRC_WW_IntendedWarehouse);
				}
				return warehouse;
			}
		}
		WhsWarehouse warehouse;

		#endregion

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
				return ctoDocAddressRequirement ?? (ctoDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ArrivalCTOAddress, ContactType.CTO));
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

		#region ConsignorPickupDeliveryAddress

		public JobDocAddress ConsignorPickupDeliveryAddress
		{
			get
			{
				if (consignorPickupDeliveryAddress == null || consignorPickupDeliveryAddress.IsDeleted)
				{
					consignorPickupDeliveryAddress = DocAddresses.FindOrCreateWithRequirement(ConsignorPickupDeliveryAddressRequirement);
				}
				return consignorPickupDeliveryAddress;
			}
		}
		JobDocAddress consignorPickupDeliveryAddress;

		JobDocAddressRequirement ConsignorPickupDeliveryAddressRequirement
		{
			get
			{
				return consignorPickupDeliveryAddressRequirement ?? (consignorPickupDeliveryAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress, ContactType.Consignor));
			}
		}
		JobDocAddressRequirement consignorPickupDeliveryAddressRequirement;

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

		#region ConsignmentOrderReferences

		public WhsItemConsignmentOrderReferenceCollection OrderReferences => orderReferences ??= new WhsItemConsignmentOrderReferenceCollection(this);

		WhsItemConsignmentOrderReferenceCollection orderReferences;

		public IActiveBusinessObjectCollection<IWhsItemConsignmentOrderReference> WhsItemConsignmentOrderReferences => OrderReferences;

		#endregion

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

		#region CustomsReferenceNumbers

		public ICustomsReferenceCollection CustomsReferenceNumbers
		{
			get
			{
				if (customsReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICustomsReferenceCollectionProvider>();
					customsReferenceNumbers = provider.GetCollection(this);
				}

				return customsReferenceNumbers;
			}
		}

		ICustomsReferenceCollection customsReferenceNumbers;

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

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region FormarttedReference

		public ZString FormattedReference => TransitWarehouseHelper.GetFormattedReferenceString(WRC_ConsignmentID, WRC_JobID);

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
			get { return new[] { DocAddressType.LocalCartageExporter, DocAddressType.ConsignorPickupDeliveryAddress, DocAddressType.ConsigneeDocumentaryAddress, DocAddressType.BookingPartyDocumentaryAddress, DocAddressType.ClientRequestedBillingParty, DocAddressType.ArrivalCTOAddress }; }
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return WRC_JobID; }
		}

		#endregion

		#region IHaveServices

		ZString IHaveServices.TableCode => WhsItemReceiveConsignmentSchema.Constants.Prefix;

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

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot
		{
			get { return true; }
		}

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
			get { return HumanReadableName; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return ZString.Empty; }
		}

		ZString IPackingParent.JobNo
		{
			get { return WRC_ConsignmentID; }
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
		}

		void IPackingParent.OnPackageJobReleased()
		{
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob pkgJob)
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

		OrgAddress IPackingParentWithTransportCompanyAndBookingParty.GetTransportCompany(string packageId, string partyType)
		{
			var packageState = PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == packageId);
			OrgAddress result = null;

			if (partyType == MessageRecipientPartyTypeList.Codes.DeliveryCartage)
			{
				result = packageState?.ReceiveTransportationUnit?.TransportCompany.Address;
			}
			else if (partyType == MessageRecipientPartyTypeList.Codes.PickupCartage)
			{
				result = packageState?.DispatchTransportationUnit?.TransportCompany.Address;
			}

			return result;
		}

		OrgAddress IPackingParentWithTransportCompanyAndBookingParty.GetBookingParty(string packageId, string partyType)
		{
			OrgAddress result = null;
			var packageState = PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == packageId);
			if (partyType == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				result = packageState?.ReceiveConsignment?.BookingPartyDocAddress.Address;
			}
			return result;
		}

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
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.TransitReceiveConsignment);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsItemReceiveConsignmentDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.TransitReceiveConsignment; }
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsItemReceiveConsignmentProcessTaskCollection(this));
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
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, BookingPartyDocAddress.OrganisationPK, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_WW, WRC_WW_IntendedWarehouse, ZGuid.Empty);
			return result;
		}

		#endregion

		#region ITransportParentCommon Members

		public ZString TypeCode => Constants.TransportParentTypes.TransitReceiveConsignment;

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new WhsItemReceiveConsignmentRatingAdaptersProvider(this); }
		}

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;

				types.Add(TransitWarehouseNoteHelper.GetNoteTypes());
				types.Add(PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes);
				types.Add(PredefinedNoteTypes.Instance.UnloadLoadNotes);
				types.Add(PredefinedNoteTypes.Instance.CIN750MessageNotes);
				types.Add(PredefinedNoteTypes.Instance.CRESAMessageNotes);

				return types;
			}
		}

		#endregion

		#region InvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn => invoicingPlugIn ?? (invoicingPlugIn = new TransitJobInvoicingPlugIn<WhsItemReceiveConsignment>(this));
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => InvoicingPlugIn.InvoicingSupporter;

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			if (WRC_JobID.IsEmpty)
			{
				WRC_JobID = (ZString)Env.NumberFountains.TransitWarehouseReceiveConsignmentID.GetNextFormatted(Factory);
			}
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
			TransitWarehouseConsignmentJobHelper.AttachConsignmentJobToParentJob(job, WRC_ParentID, WRC_ParentTableCode);
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

		JobInvoicingConsumerType ITransitJobInvoicingPlugIn.ConsumerType => JobInvoicingConsumerTypes.TransitReceive;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.AuditSecurity => Env.Security.WhsItemReceiveConsignmentAuditBilling;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.JobInvoicingSecurity => Env.Security.WhsItemReceiveConsignmentJobInvoicing;

		IJobInvoicingSupporter ITransitJobInvoicingPlugIn.InvoicingSupporter => new WhsItemConsignmentInvoicingSupporter<WhsItemReceiveConsignment>(this);

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
				var masterBillAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.MasterBill);
				var masterBillNumber = masterBillAdditionalReference != null && masterBillAdditionalReference.CE_EntryNum != "" ? masterBillAdditionalReference.CE_EntryNum : ZString.Empty;
				if (masterBillNumber.IsEmpty)
				{
					masterBillNumber = PackageStates.Select(p => p.ReceiveASN).FirstOrDefault(asn => asn != null && !asn.MasterBillNumber.IsEmpty)?.MasterBillNumber ?? ZString.Empty;
				}

				return masterBillNumber;
			}
		}

		public ZString HouseBillNumber
		{
			get
			{
				return WRC_HouseBillNumber;
			}
		}

		ZString IConsignment.JobID => WRC_JobID;
		ZString IConsignment.TransportMode => WRC_TransportMode;
		ZString IConsignment.Direction => WRC_Direction;

		#endregion

		#region ITransitConsignmentForRating Members

		string ITransitConsignmentForRating.ServiceLevel => WRC_RS_NKServiceLevel;

		FreightMode? ITransitJobForRating.FreightMode
		{
			get
			{
				if (!WRC_TransportMode.IsEmpty)
				{
					return TransitWarehouseHelper.GetFreightModeByTransportMode(WRC_TransportMode);
				}
				else
				{
					return null;
				}
			}
		}

		string ITransitJobForRating.ChargeCodeGroup => ChargeCodeGroupList.Codes.TRWReceive;

		ZDateTime ITransitJobForRating.ExpectedArrivalDate => WRC_ExpectedArrivalTime;

		ZDateTime ITransitJobForRating.ExpectedDepartureDate => WRC_ExpectedDispatchTime;

		IEnumerable<TransitPackageForRatingInfo> ITransitJobForRating.TransitPackagesForRating
		{
			get
			{
				if (transitPackagesForRating == null)
				{
					var sql = $@"
;WITH PackageStates AS
(
	SELECT *
	FROM 
		dbo.WhsItemPackageState
	WHERE
		WPS_WRC_TransitReceiveConsignment = @ReceiveConsignmentPK
		AND WPS_WRH_TransitReceiveHeader IS NOT NULL
		AND WPS_Status <> 'ADJ'
),
OverpackHandlingUnits AS
(
	SELECT WPS_PK, WPS_KP_Package
	FROM
		PackageStates
	WHERE
		WPS_IsHandlingUnit = 1
),
OverpackHandlingUnitInners AS
(
	SELECT ChildPackage.WPS_PK AS InnerPackageStatePK
	FROM
		OverpackHandlingUnits
		JOIN dbo.PkgPackageHandlingUnitDivot ON KPD_KP_HandlingUnit = OverpackHandlingUnits.WPS_KP_Package
		JOIN dbo.WhsItemPackageState AS ChildPackage ON ChildPackage.WPS_KP_Package = KPD_KP_Package
	WHERE
		KPD_UnpackedTime IS NULL
)

SELECT 
	WPS_PK,
	WPS_UnitType,
	WPS_KP_Package,
	KP_PackageQty,
	KP_F3_NKPackType,
	KP_Weight,
	KP_WeightUQ,
	KP_Volume,
	KP_VolumeUQ,
	KP_RH_NKCommodityCode,
	CAST((CASE WHEN DG.DI_PK IS NOT NULL THEN 1 ELSE 0 END) AS BIT) AS HasDG,
	WPS_LoadedTime,
	WRH_UnloadCompleteTime
FROM
	PackageStates
	JOIN dbo.PkgPackage ON WPS_KP_Package = KP_PK
	JOIN dbo.WhsItemReceiveTransportationUnit ON WPS_WRH_TransitReceiveHeader = WRH_PK
	OUTER APPLY (SELECT TOP 1 DI_PK FROM dbo.UNDGDataItem WHERE DI_ParentID = KP_PK) AS DG
WHERE
	WPS_PK NOT IN (SELECT InnerPackageStatePK FROM OverpackHandlingUnitInners)";
					var sqlParams = new ZSqlParameterCollection
					{
						{ "@ReceiveConsignmentPK", PK, WhsItemReceiveConsignmentSchema.PK },
					};

					var query = new ZQuery();
					query.AddFilterAndZSQLParameterCollection(sql, sqlParams);

					var transitPackages = new List<TransitPackageForRatingInfo>();
					using (var reader = Db.Connection.Command(query.LiteralTextSqlFormatted).ExecuteReader())     // Accessing data using a SQL View
					{
						while (reader.Read())
						{
							transitPackages.Add(TransitPackageForRatingInfo.PopulatePackageForRatingInfo(reader));
						}
					}

					transitPackagesForRating = transitPackages;
				}

				return transitPackagesForRating;
			}
		}
		IEnumerable<TransitPackageForRatingInfo> transitPackagesForRating;

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
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

		#region IPackingParentWithOutturn

		IContainerView IPackingParentWithOutturn.GetContainerView(PkgPackageContainer container) => new ContainerViewForReceive(container);

		IOutturnProvider IPackingParentWithOutturn.GetOutturnProvider(PkgPackage package) => new OutturnProviderForRCN(package);

		(string ContainerNumber, PkgPackageContainer Container) IPackingParentWithOutturn.GetParentContainer(PkgPackage package)
		{
			var packageState = PackageStates.SingleOrDefault(p => p.WPS_KP_Package == package.PK);

			if (packageState?.ReceiveTransportationUnit != null && !string.IsNullOrEmpty(packageState.ReceiveTransportationUnit.ContainerNumber))
			{
				var container = packageState.ReceiveTransportationUnit.Container;
				return (packageState.ReceiveTransportationUnit.WRH_VehicleReference, container);
			}

			return (null, null);
		}

		int IPackingParentWithOutturn.TotalNumberOfPiecesOutturned => PackageStates.Sum(p => p.Package.KP_PackageQty);

		#endregion

		#region TransportCollection

		public TransportCollection Transports
		{
			get
			{
				if (transports == null || transports.Any(t => t.IsDeleted))
				{
					transports = new TransportCollection(this);
					transports.Load();
				}
				return transports;
			}
		}
		TransportCollection transports;

		#endregion

		#region IRoutingSupport Members

		RoutingCollection IRoutingSupport.TransportsIncludingRelated => transportsIncludingRelated ?? (transportsIncludingRelated = new RoutingCollection(this));
		RoutingCollection transportsIncludingRelated;

		TransportCollection IRoutingSupport.Transports => Transports;

		ZString IRoutingSupport.TransportMode => WRC_TransportMode;

		string IRoutingSupport.AdditionalETAUpdateMsg => string.Empty;

		string IRoutingSupport.AdditionalETDUpdateMsg => string.Empty;

		#endregion

		#region ITransportParent Members

		public Directions JobDirection => Directions.Unknown;

		TransportSupporter ITransportParent.TransportSupporter => new WhsItemReceiveConsignmentTransportSupporter(this);

		TransportCollection ITransportParent.Transports => Transports;

		#endregion

		#region ITransportChangeNotifier Members

		public void NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue)
		{
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out var handler))
			{
				handler?.Invoke(transport, previousValue);
			}
		}

		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> TransportChangeNotifierDictionary
		{
			get { return transportChangeNotifierDictionary ?? (transportChangeNotifierDictionary = new Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler>()); }
		}

		Dictionary<TransportChangeNotifyType, TransportChangeNotifyHandler> transportChangeNotifierDictionary;

		public void AddTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out var handler))
			{
				handler -= notifier;
				handler += notifier;
			}
			else
			{
				TransportChangeNotifierDictionary.Add(notifyType, notifier);
			}
		}

		public void RemoveTransportChangeNotifier(TransportChangeNotifyType notifyType, TransportChangeNotifyHandler notifier)
		{
			if (TransportChangeNotifierDictionary.TryGetValue(notifyType, out var handler))
			{
				handler -= notifier;
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

		#region BusinessObjectsWithRelatedEventsCore

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
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

		#region ITransitJobForConsolCosting

		ZString ITransitJobForConsolCosting.HouseBillNumber => HouseBillNumber.IsEmpty ? WRC_ConsignmentID : HouseBillNumber;

		RefUNLOCO ITransitJobForConsolCosting.Destination => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, WRC_RL_NKDestination);

		IEnumerable<WhsItemPackageState> ITransitJobForConsolCosting.PackageStatesForConsolCosting => packageStatesForConsolCosting ?? (packageStatesForConsolCosting = PackageStates.Where(t => t.WPS_Status != TransitWarehouseStatuses.Codes.Booked));
		IEnumerable<WhsItemPackageState> packageStatesForConsolCosting;

		#endregion

		#region PreviousLoadPort

		public RefUNLOCO PreviousLoadPort
		{
			get
			{
				var lastTransport = Transports.Cast<Transport>().FirstOrDefault(t => t.JW_RL_NKDiscPort == Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort);
				if (lastTransport == null)
				{
					var extraPorts = Warehouse.RelatedCompanyBranch.ExtraPorts.Cast<GlbBranchExtraPorts>().Select(p => p.GY_RL_NKAdditionalBranchRelatedPort);
					lastTransport = Transports.Cast<Transport>().FirstOrDefault(t => extraPorts.Contains(t.JW_RL_NKDiscPort));
				}
				return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, lastTransport?.JW_RL_NKLoadPort ?? ZString.Empty);
			}
		}

		#endregion

		#region IPackingParentWithAttachedParent

		ZString IPackingParentWithAttachedParent.GetAttachedJobNumber(PkgPackage package) => TransitWarehouseHelper.GetAttachedJobNumber(package);

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsItemReceiveConsignmentFetchStrategy(this);
		}

		#endregion

		#region GetOuterPackages

		/// <summary>
		/// This property is used to get standalone packages or overpacks and first level inner packed in handling unit which assigned to dispatch consignment
		/// And the result is used by document supporter
		/// </summary>
		public IEnumerable<WhsItemPackageState> OuterPackages
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

		#region Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var whs = (BusinessObject)Factory.NewWithValidTestData<WhsWarehouse>();
			WRC_WW_IntendedWarehouse = whs.PK;

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}
