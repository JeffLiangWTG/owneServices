using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
	/// <summary>
	/// Transit Warehouse is a GLOW development. This class only exists to support UXML, Workflow and Documents.
	/// </summary>
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
	[Packing.Business.Testing.PackingParentTestCase.TestExcludePackingParentHasTestCase]
#endif
	[UniversalDataContext(DataContextType.TransitReceiveHeader)]
	[CodeProperty(WhsItemReceiveTransportationUnit.Schema.WRH_ReferenceNumber)]
	[DescriptionProperty(WhsItemReceiveTransportationUnit.Schema.WRH_VehicleReference)]
	public class WhsItemReceiveTransportationUnit : AutoWhsItemReceiveTransportationUnit,
		IWhsItemReceiveTransportationUnit,
		IDocManagerSupport,
		IWorkflowProvider,
		IProcessHandlingInfoProvider,
		IDocAddresses,
		IItemHeader,
		IJobNumber,
		IPackingParent,
		IPackingParentWithOutturn,
		IPackingParentSupportsImportingBookedDimensions,
		IDocumentSupportable,
		IJobCostingPlugIn,
		ITransportationUnitForApportioning,
		IJobInvoicingPlugIn,
		ITransitJobInvoicingPlugIn,
		IRatingSupporter,
		ITransitTransportationUnitForRating,
		IEDocsProvider,
		IPackingParentWithAttachedParent,
		IHaveCusEntryNumReferences,
		IPackingParentSupportsPackageExtensions
	{
		public WhsItemReceiveTransportationUnit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TransportReferenceDetails

		public ZString TransportReference
		{
			get
			{
				var transportAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber);
				var runSheetNumber = transportAdditionalReference != null && transportAdditionalReference.CE_EntryNum != "" ? transportAdditionalReference.CE_EntryNum : ZString.Empty;
				return runSheetNumber.IsEmpty ? WRH_VehicleReference : runSheetNumber;
			}
		}

		public ZString MasterBillNumber
		{
			get
			{
				var masterBillAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.MasterBill);
				return masterBillAdditionalReference != null && masterBillAdditionalReference.CE_EntryNum != ""
					? masterBillAdditionalReference.CE_EntryNum
					: GetASNMasterBillNumber();
			}
		}

		ZString GetASNMasterBillNumber()
		{
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, PK));
			var asns = pivots.Select(p => p.ReceiveASN);
			var asnsWithMasterBillNumbers = asns.OrderByDescending(a => a.WRP_ReferenceNumber).FirstOrDefault(d => d.MasterBillNumber != "");
			return asnsWithMasterBillNumbers?.MasterBillNumber ?? ZString.Empty;
		}

		public ZString CarrierBookingReference
		{
			get
			{
				var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, PK));
				var asns = pivots.Select(p => p.ReceiveASN);
				var asnsWtihFirstCarrierBookingReference = asns.OrderBy(r => r.WRP_ReferenceNumber).FirstOrDefault(d => d.CarrierBookingReference != "");
				return asnsWtihFirstCarrierBookingReference?.CarrierBookingReference ?? ZString.Empty;
			}
		}

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
				}

				return additionalReferenceNumbers;
			}
		}

		ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

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

		#region ContainerNumber

		public ZString ContainerNumber => IsContainerUnitType ? WRH_VehicleReference : ZString.Empty;

		#endregion

		#region VehicleNumber

		public ZString VehicleNumber => IsVehicleUnitType ? WRH_VehicleReference : ZString.Empty;

		#endregion

		#region HasContainerEquipmentDetails

		public bool HasContainerEquipmentDetails => PackageExtension?.Package?.Container != null;

		#endregion

		#region ContainerOrEquipmentDetails

		PkgPackageContainer ContainerOrEquipmentDetails => PackageExtension?.Package?.Container;

		#endregion

		#region ContainerTypeCode

		public ZString ContainerTypeCode => HasContainerEquipmentDetails ? ContainerOrEquipmentDetails?.ContainerType?.RC_Code ?? ZString.Empty : ZString.Empty;

		#endregion

		#region ContainerISOType

		public ZString ContainerISOType => HasContainerEquipmentDetails ? ContainerOrEquipmentDetails?.ContainerType?.RC_ISOType ?? ZString.Empty : ZString.Empty;

		#endregion

		#region HasSealNumber

		public bool HasSealNumber => HasContainerEquipmentDetails && !string.IsNullOrEmpty(ContainerOrEquipmentDetails?.K0_Seal1 ?? "");

		#endregion

		#region IsContainerUnitType

		public bool IsContainerUnitType => WRH_UnitType == TransportUnitTypes.Container || WRH_UnitType == TransportUnitTypes.ULD || (WRH_UnitType == TransportUnitTypes.Vehicle && HasContainerEquipmentDetails);

		#endregion

		#region IsVehicleUnitType

		public bool IsVehicleUnitType => WRH_UnitType == TransportUnitTypes.Vehicle;

		#endregion

		#region Related Entities

		#region Warehouse

		[RelatedBusinessObject("Warehouse")]
		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WRH_WW_Warehouse
		{
			get { return base.WRH_WW_Warehouse; }
			set { base.WRH_WW_Warehouse = value; }
		}

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WRH_WW_Warehouse); }
		}

		#endregion

		#region Location

		[RelatedBusinessObject("Location")]
		public override ZGuid WRH_WL_StagingLocation
		{
			get { return base.WRH_WL_StagingLocation; }
			set { base.WRH_WL_StagingLocation = value; }
		}

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WRH_WL_StagingLocation); }
		}

		#endregion

		#region PackageStates

		public WhsItemPackageStateCollection PackageStates
		{
			get { return packageStates ?? (packageStates = new WhsItemPackageStateCollection(this)); }
		}

		WhsItemPackageStateCollection packageStates;

		public IEnumerable<WhsItemPackageState> BookedPackagesInPendingASNs
		{
			get
			{
				if (bookedPackagesInPendingASNs == null)
				{
					var pendingASNs = ReceiveASNs.Where(asn => asn.WRP_CompleteTime.IsEmpty);
					bookedPackagesInPendingASNs = pendingASNs.SelectMany(asn => asn.PackageStates).Where(wps => wps.WPS_Status == TransitWarehouseStatuses.Codes.Booked);
				}
				return bookedPackagesInPendingASNs;
			}
		}

		IEnumerable<WhsItemPackageState> bookedPackagesInPendingASNs;

		#endregion

		#region ReceiveConsignments

		public IReadOnlyCollection<WhsItemReceiveConsignment> ReceiveConsignments
		{
			get
			{
				if (receiveConsignments == null)
				{
					var rcnQuery = new ZQuery(WhsItemReceiveConsignmentSchema.PK, PackageStates.Select(p => p.WPS_WRC_TransitReceiveConsignment));
					receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(rcnQuery);
				}
				return receiveConsignments;
			}
		}

		IReadOnlyCollection<WhsItemReceiveConsignment> receiveConsignments;

		#endregion

		#region ReceiveASNs

		public WhsItemReceiveASNCollection ReceiveASNs => receiveASNs ??= new WhsItemReceiveASNCollection(Factory, this);

		WhsItemReceiveASNCollection receiveASNs;

		public IEnumerable<WhsItemReceiveASN> ReceiveASNsFromPackageStates => PackageStates.Select(ps => ps.ReceiveASN).Distinct();

		#endregion

		#region Container

		public PkgPackageContainer Container => PackageExtension?.Package?.Container;

		#endregion

		#region ASNWhsItemPackageStateCollection

		public ActiveBusinessObjectCollection<WhsItemPackageState> ReceiveASNsPackageStates
		{
			get
			{
				if (receiveASNsPackageStates == null)
				{
					receiveASNsPackageStates = new ActiveBusinessObjectCollection<WhsItemPackageState>(Factory, new AdhocCollectionRelationship(typeof(WhsItemPackageState)));
					receiveASNsPackageStates.AddRange(ReceiveASNs.SelectMany(a => a.PackageStates).Distinct());
				}

				return receiveASNsPackageStates;
			}
		}

		ActiveBusinessObjectCollection<WhsItemPackageState> receiveASNsPackageStates;

		#endregion

		#endregion

		#region WRH_ReferenceNumber

		[ResourceStringData("51c799b8-56d1-462e-b076-0b32ea05f112", Caption = "Reference Number")]
		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRH_ReferenceNumber
		{
			get { return base.WRH_ReferenceNumber; }
			set { base.WRH_ReferenceNumber = value; }
		}

		#endregion

		#region WRH_GateInTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WRH_GateInTime
		{
			get { return base.WRH_GateInTime; }
			set { base.WRH_GateInTime = value; }
		}

		public ZDateTimeOffset GateInTime => WRH_GateInTime;

		#endregion

		#region WRH_UnloadCompleteTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WRH_UnloadCompleteTime
		{
			get { return base.WRH_UnloadCompleteTime; }
			set { base.WRH_UnloadCompleteTime = value; }
		}

		#endregion

		#region WRH_GateOutTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WRH_GateOutTime
		{
			get { return base.WRH_GateOutTime; }
			set { base.WRH_GateOutTime = value; }
		}

		#endregion

		#region WRH_SystemCreateTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WRH_SystemCreateTimeUtc
		{
			get { return base.WRH_SystemCreateTimeUtc; }
			set { base.WRH_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region WRH_SystemCreateUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRH_SystemCreateUser
		{
			get { return base.WRH_SystemCreateUser; }
			set { base.WRH_SystemCreateUser = value; }
		}

		#endregion

		#region WRH_SystemLastEditTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WRH_SystemLastEditTimeUtc
		{
			get { return base.WRH_SystemLastEditTimeUtc; }
			set { base.WRH_SystemLastEditTimeUtc = value; }
		}

		#endregion

		#region WRH_SystemLastEditUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WRH_SystemLastEditUser
		{
			get { return base.WRH_SystemLastEditUser; }
			set { base.WRH_SystemLastEditUser = value; }
		}

		#endregion

		#region WRH_SignedBySignature

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBlob WRH_SignedBySignature
		{
			get { return base.WRH_SignedBySignature; }
			set { base.WRH_SignedBySignature = value; }
		}

		#endregion

		#region IsGatedOut

		public ZBool IsGatedOut
		{
			get => !WRH_GateOutTime.IsEmpty;
			set
			{
				if (value)
				{
					var dateTimeOffsetNow = TransitWarehouseHelper.GetNowInCurrentWarehouse(Warehouse);
					var gateOutTime = new ZDateTimeOffset(dateTimeOffsetNow.Year, dateTimeOffsetNow.Month, dateTimeOffsetNow.Day, dateTimeOffsetNow.Hour, dateTimeOffsetNow.Minute, 0, dateTimeOffsetNow.Offset);
					if ((!WRH_UnloadCompleteTime.IsEmpty || !WRH_UnloadCompleteNotYetProcessedTime.IsEmpty) && gateOutTime.IsValid)
					{
						if (!WRH_UnloadCompleteTime.IsEmpty)
						{
							WRH_GateOutTime = WRH_UnloadCompleteTime <= gateOutTime ? gateOutTime : WRH_UnloadCompleteTime;
						}
						else
						{
							WRH_GateOutTime = WRH_UnloadCompleteNotYetProcessedTime <= gateOutTime ? gateOutTime : WRH_UnloadCompleteNotYetProcessedTime;
						}

						if (ContainerizedPackageState != null)
						{
							ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
						}

						var whs = Warehouse;

						Logs.AddNew(
							Events.GateOut,
							WRH_ReferenceNumber,
							dateTimeOffsetNow.ToDateTime(),
							new KeyValuePair<string, string>("FAC", CargoWise.EventReference.Constants.Facilities.Code.Depot),
							new KeyValuePair<string, string>("LOC", whs != null && whs.WarehouseAddress != null ? (string)whs.WarehouseAddress.OA_City : string.Empty),
							new KeyValuePair<string, string>("TYP", HasContainerEquipmentDetails ? "ContainerID" : "VehicleReference"),
							new KeyValuePair<string, string>("REF", WRH_VehicleReference),
							new KeyValuePair<string, string>("WHS", whs != null ? (string)whs.WW_WarehouseCode : string.Empty));
					}
				}
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			PackageExtension?.Delete();
			base.Delete();
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region TransportCompany

		public JobDocAddress TransportCompany
		{
			get
			{
				if (transportCompany == null || transportCompany.IsDeleted)
				{
					transportCompany = DocAddresses.FindOrCreateWithRequirement(TransportCompanyDocAddressDocAddressRequirement);
				}
				return transportCompany;
			}
		}
		JobDocAddress transportCompany;

		JobDocAddressRequirement TransportCompanyDocAddressDocAddressRequirement
		{
			get { return transportCompanyDocAddressDocAddressRequirement ?? (transportCompanyDocAddressDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress, ContactType.LocalTransport)); }
		}

		JobDocAddressRequirement transportCompanyDocAddressDocAddressRequirement;

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

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WRH_ReferenceNumber.IsEmpty ?
					Res.GetString("7A5BF35B-1CBA-4774-A60B-C87BE568E858", "Receive Transportation Unit") :
					Res.GetString("FA774C03-BC2F-4C0E-8C79-F196E22F63DC", "Receive Transportation Unit {0}", WRH_ReferenceNumber);
			}
		}

		#endregion

		#region FormarttedReference

		public ZString FormattedReference => !string.IsNullOrEmpty(WRH_VehicleReference) ?
			TransitWarehouseHelper.GetFormattedReferenceString(WRH_VehicleReference, WRH_ReferenceNumber) :
			TransitWarehouseHelper.GetFormattedReferenceString(ContainerTypeCode, WRH_ReferenceNumber);

		#endregion

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
			get { return new[] { DocAddressType.TransportCompanyDocumentaryAddress, DocAddressType.ClientRequestedBillingParty }; }
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.TransitReceiveTransportationUnit);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IJobNumber Memebers

		public string JobNumber => WRH_ReferenceNumber;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.TransitReceiveTransportationUnit; }
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsItemReceiveTransportationUnitProcessTaskCollection(this));
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
			result.Add(ProcessTaskTemplateSchema.P0_WW, WRH_WW_Warehouse, ZGuid.Empty);
			return result;
		}

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new WhsItemReceiveTransportationUnitProcessHandlingInfoProvider(this); }
		}

		#endregion

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot
		{
			get { return false; }
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

		bool IPackingParent.IsScanEventsVisible
		{
			get { return false; }
		}

		ZString IPackingParent.JobDescription
		{
			get { return ""; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return ZString.Empty; }
		}

		ZString IPackingParent.JobNo
		{
			get { return WRH_ReferenceNumber; }
		}

		bool IPackingParent.IsPackingJobReadOnly
		{
			get { return false; }
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

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		ParentJobType IPackingParent.ParentJobType => ParentJobType.None;

		PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.Outer;

		bool IPackingParent.CanReleasePackage(PkgPackage package) => true;

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
		}

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

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

		IOutturnProvider IPackingParentWithOutturn.GetOutturnProvider(PkgPackage package) => null;

		(string ContainerNumber, PkgPackageContainer Container) IPackingParentWithOutturn.GetParentContainer(PkgPackage package)
		{
			return (null, null);
		}

		int IPackingParentWithOutturn.TotalNumberOfPiecesOutturned => PackageStates.Sum(p => p.Package.KP_PackageQty);

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ?? (documentSupporter = new WhsItemReceiveTransportationUnitDocumentSupporter(this));

		DocumentSupporter documentSupporter;

		#endregion

		#region IJobCostingPlugIn Members

		public ZString JK_UniqueConsignRef => WRH_ReferenceNumber;

		public RefUNLOCO LoadPort => null;

		public RefUNLOCO DischargePort => null;

		public JobProfitLossCollection ProfitLossContainer => null;

		public decimal ConsolExchangeRate => 0m;

		public RefCurrency ConsolCurrency => null;

		public bool IsMasterCollect => false;

		public OrgHeader ReceivingAgent => null;

		public OrgHeader ReceivingAgentAPInvoicingParty => null;

		public OrgHeader ReceivingAgentARInvoicingParty => null;

		public OrgHeader SendingAgent => null;

		public OrgHeader SendingAgentAPInvoicingParty => null;

		public OrgHeader SendingAgentARInvoicingParty => null;

		public ZString ContainerMode => ZString.Empty;

		public ZString ConsolType => ZString.Empty;

		public ZString Direction => ZString.Empty;

		public ZString Module => ApportionmentMethodModules.TransitWarehouse;

		public CodeDescriptionPairList PrepaidCollectList => null;

		public IGenericJobCostSupporter CostSupporter => costSupporter ?? (costSupporter = new WhsItemTransportationUnitCostSupporter<WhsItemReceiveTransportationUnit>(this));

		IGenericJobCostSupporter costSupporter;

		public decimal ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK) => 0m;

		public void AddNewToLogs(Event @event, ZString reference) => Logs.AddNew(@event, reference);

		public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob) => ZString.Empty;

		#region TransportMode

		public ZString TransportMode
		{
			get
			{
				return PackageStates.FirstOrDefault(p => !string.IsNullOrEmpty(p.ReceiveConsignment?.WRC_TransportMode ?? ""))?.ReceiveConsignment?.WRC_TransportMode ?? "";
			}
		}

		#endregion

		#endregion

		#region ITransportationUnitForCost Member

		public ZGuid CreditorPK => TransitWarehouseHelper.GetTransportCompanyOrganisation(TransportCompany)?.PK ?? ZGuid.Empty;

		public IEnumerable<ZString> DefaultChargeGroups => new ZString[] { ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit };

		public ZString MasterBillNum => MasterBillNumber;

		public IEnumerable<IJobInvoicingPlugIn> Consignments => ReceiveConsignments.Any()
			? ReceiveConsignments
			: Enumerable.Empty<IJobInvoicingPlugIn>();

		public ZDateTime ETA => ReceiveConsignments.Any()
			? ReceiveConsignments.Where(t => t.WRC_ExpectedArrivalTime.IsValid).Select(t => t.WRC_ExpectedArrivalTime).OrderBy(t => t).First()
			: ZDateTime.Empty;

		public ZDateTime ETD => ReceiveConsignments.Any()
			? ReceiveConsignments.Where(t => t.WRC_ExpectedDispatchTime.IsValid).Select(t => t.WRC_ExpectedDispatchTime).OrderByDescending(t => t).First()
			: ZDateTime.Empty;

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
				return types;
			}
		}

		#endregion

		#region InvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn => invoicingPlugIn ?? (invoicingPlugIn = new TransitJobInvoicingPlugIn<WhsItemReceiveTransportationUnit>(this));
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => InvoicingPlugIn.InvoicingSupporter;

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			if (WRH_ReferenceNumber.IsEmpty)
			{
				WRH_ReferenceNumber = (ZString)Env.NumberFountains.TransitWarehouseReceiveID.GetNextFormatted(Factory);
			}
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
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

		JobInvoicingConsumerType ITransitJobInvoicingPlugIn.ConsumerType => JobInvoicingConsumerTypes.TransitReceiveTransportationUnit;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.AuditSecurity => Env.Security.WhsItemReceiveTransportationUnitAuditBilling;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.JobInvoicingSecurity => Env.Security.WhsItemReceiveTransportationUnitJobInvoicing;

		IJobInvoicingSupporter ITransitJobInvoicingPlugIn.InvoicingSupporter => new TransitJobInvoicingSupporter<WhsItemReceiveTransportationUnit>(this);

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider => new WhsItemTransportationUnitRatingAdaptersProvider<WhsItemReceiveTransportationUnit>(this);

		#endregion

		#region ITransitJobForRating

		WhsWarehouse ITransitJobForRating.Warehouse => Warehouse;

		FreightMode? ITransitJobForRating.FreightMode
		{
			get
			{
				FreightMode? mode = null;
				if (Container != null)
				{
					mode = TransitWarehouseHelper.GetFreightModeByTransportMode(Container.ContainerType.RC_ShippingMode, true);
				}
				else
				{
					var transportModesFromRCNs = ReceiveConsignments.Select(r => r.WRC_TransportMode).Where(t => !t.IsEmpty).Distinct();
					if (transportModesFromRCNs.Any())
					{
						var freightModes = transportModesFromRCNs.Select(t => TransitWarehouseHelper.GetFreightModeByTransportMode(t)).Distinct();
						mode = freightModes.Count() == 1 ? freightModes.First() : FreightMode.UKN;
					}
				}

				return mode;
			}
		}

		ZDateTime ITransitJobForRating.ExpectedArrivalDate => WRH_GateInTime.ToLocalZDateTime();

		ZDateTime ITransitJobForRating.ExpectedDepartureDate => WRH_UnloadCompleteTime.ToLocalZDateTime();

		string ITransitJobForRating.ChargeCodeGroup => ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit;

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
		WPS_WRH_TransitReceiveHeader = @RTUPK
),
OverpackHandlingUnits AS
(
	SELECT WPS_PK, WPS_KP_Package
	FROM
		PackageStates
	WHERE
		WPS_IsHandlingUnit = 1
		AND WPS_WRC_TransitReceiveConsignment IS NOT NULL
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
	WPS_UnloadedTime,
	WPS_LoadedTime
FROM
	PackageStates
	JOIN dbo.PkgPackage ON WPS_KP_Package = KP_PK
	OUTER APPLY (SELECT TOP 1 DI_PK FROM dbo.UNDGDataItem WHERE DI_ParentID = KP_PK) AS DG
WHERE
	WPS_PK NOT IN (SELECT InnerPackageStatePK FROM OverpackHandlingUnitInners)";
					var sqlParams = new ZSqlParameterCollection
					{
						{ "@RTUPK", PK, WhsItemReceiveConsignmentSchema.PK },
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

		JobDocAddress ITransitTransportationUnitForRating.TransportCompanyDocAddress => TransportCompany;

		KeyValuePair<PkgPackageContainer, bool>? ITransitTransportationUnitForRating.ContainerForRating
		{
			get
			{
				if (Container != null)
				{
					var transitPackagesForRating = ((ITransitJobForRating)this).TransitPackagesForRating;
					var allPackagesArePallet = transitPackagesForRating.Any() && !transitPackagesForRating.Any(p => p.PackageType != Core.Constants.PkgUnit.Pallet && p.UnitType != TransportUnitTypes.ULD && p.UnitType != TransportUnitTypes.Container);
					return new KeyValuePair<PkgPackageContainer, bool>(Container, allPackagesArePallet);
				}

				return null;
			}
		}

		#endregion

		#region IPackingParentWithAttachedParent

		ZString IPackingParentWithAttachedParent.GetAttachedJobNumber(PkgPackage package) => TransitWarehouseHelper.GetAttachedJobNumber(package);

		#endregion

		#region IWhsItemReceiveTransportationUnit Members

		IWhsWarehouse IWhsItemReceiveTransportationUnit.Warehouse => Warehouse;
		IConsignment IWhsItemReceiveTransportationUnit.LatestReceiveConsignment => ReceiveConsignments.OrderByDescending(c => c.WRC_SystemCreateTimeUtc).FirstOrDefault();
		ZString IWhsItemReceiveTransportationUnit.UnitType => WRH_UnitType;
		ZString IWhsItemReceiveTransportationUnit.ReferenceNumber
		{
			get => WRH_ReferenceNumber;
			set => WRH_ReferenceNumber = value;
		}
		ZGuid IWhsItemReceiveTransportationUnit.PK => this.PK;

		#endregion

		#region JobHeader

		public JobHeader JobHeader
		{
			get { return new JobHeader.Loader(this).Load(); }
		}

		#endregion

		#region ULDPackageExtension

		public PkgPackageExtension PackageExtension
		{
			get
			{
				return (packageExtension ?? (packageExtension = Factory.LoadTop1<PkgPackageExtension>(new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, PK))));
			}
		}
		PkgPackageExtension packageExtension;

		#endregion

		#region ContainerizedPackageState

		public WhsItemPackageState ContainerizedPackageState
		{
			get
			{
				return PackageExtension == null ? null : containerizedPackageState ?? (containerizedPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, PackageExtension.KPN_KP_Package)));
			}
		}

		WhsItemPackageState containerizedPackageState;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsItemReceiveTransportationUnitFetchStrategy(this);
		}

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			// delete Location View object and replace with a WhsLocation Row 
			// to force the factory to save the Location first
			var location = Location
				?? Factory.NewWithValidTestData<WhsLocation>();

			if (!location.IsInDatabase)
			{
				var row = location.WLV_WR;
				var area = location.WLV_WA_PickingArea;
				var locationType = location.WLV_WLT_LocationType;

				location.Delete();

				var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
				var newLocation = rowFactory.New(ZArchitecture.Schema.WhsLocationSchema.Constants.TableName);

				var locationPK = System.Guid.NewGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.PK] = locationPK;
				newLocation.Table.Rows.Add(newLocation);

				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WR] = row.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WA_PickingArea] = area.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WA_PutawayArea] = area.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WLT_LocationType] = locationType.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_Column] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_Level] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_Tray] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_PutawayPathSequence] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemCreateTimeUtc] = ZDateTime.UtcNow.SqlFormat.ToString();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemLastEditTimeUtc] = ZDateTime.UtcNow.SqlFormat.ToString();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemCreateUser] = 'E';
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemLastEditUser] = 'E';
				WRH_WL_StagingLocation = locationPK;
			}
		}

#endif
		#endregion
	}
}
