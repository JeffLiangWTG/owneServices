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
using Enterprise.Registry.Business.Warehouse;
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
	/// Transit Warehouse is a GLOW development. This class only exists Workflow and Documents.
	/// </summary>
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
	[Packing.Business.Testing.PackingParentTestCase.TestExcludePackingParentHasTestCase]
#endif
	[UniversalDataContext(DataContextType.TransitDispatchHeader)]

	[CodeProperty(WhsItemDispatchTransportationUnit.Schema.WDH_ReferenceNumber)]
	[DescriptionProperty(WhsItemDispatchTransportationUnit.Schema.WDH_VehicleReference)]
	public class WhsItemDispatchTransportationUnit : AutoWhsItemDispatchTransportationUnit,
		IWhsItemDispatchTransportationUnit,
		IDocManagerSupport,
		IWorkflowProvider,
		IDocumentSupportable,
		IPackingParent,
		IPackingParentWithOutturn,
		IProcessHandlingInfoProvider,
		IDocAddresses,
		IItemHeader,
		IJobNumber,
		IJobCostingPlugIn,
		ITransportationUnitForApportioning,
		IJobInvoicingPlugIn,
		ITransitJobInvoicingPlugIn,
		IRatingSupporter,
		ITransitTransportationUnitForRating,
		IEDocsProvider,
		IPackingParentSupportsPackageExtensions,
		IHaveCusEntryNumReferences
	{
		public WhsItemDispatchTransportationUnit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TransportReferenceDetails

		public ZString TransportReference
		{
			get
			{
				var runSheetAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber);
				var runSheetNumber = runSheetAdditionalReference != null && runSheetAdditionalReference.CE_EntryNum != "" ? runSheetAdditionalReference.CE_EntryNum : ZString.Empty;
				return runSheetNumber.IsEmpty ? WDH_VehicleReference : runSheetNumber;
			}
		}

		public ZString MasterBillNumber
		{
			get
			{
				var masterBillAdditionalReference = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(TransportAdditionalReferenceTypes.Codes.MasterBill);
				return masterBillAdditionalReference != null && masterBillAdditionalReference.CE_EntryNum != ""
					? masterBillAdditionalReference.CE_EntryNum
					: GetLoadListMasterBillNumber();
			}
		}

		ZString GetLoadListMasterBillNumber()
		{
			var pivots = Factory.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, PK));
			var loadLists = pivots.Select(p => p.DispatchLoadList);
			var loadListsWithMasterBillNumbers = loadLists.OrderByDescending(l => l.WDL_JobID).FirstOrDefault(d => d.MasterBillNumber != "");
			return loadListsWithMasterBillNumbers?.MasterBillNumber ?? ZString.Empty;
		}

		public ZString CarrierBookingReference
		{
			get
			{
				var pivots = Factory.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, PK));
				var loadLists = pivots.Select(p => p.DispatchLoadList);
				var loadListsWtihFirstCarrierBookingReference = loadLists.OrderBy(r => r.WDL_JobID).FirstOrDefault(d => d.CarrierBookingReference != "");
				return loadListsWtihFirstCarrierBookingReference?.CarrierBookingReference ?? ZString.Empty;
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

		#endregion

		#region WDH_WW_Warehouse

		[ResourceStringData("WhsItemDispatchTransportationUnit|WDH_WW_Warehouse", Caption = "Warehouse")]
		[RelatedBusinessObject("Warehouse")]
		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WDH_WW_Warehouse
		{
			get { return base.WDH_WW_Warehouse; }
			set { base.WDH_WW_Warehouse = value; }
		}

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WDH_WW_Warehouse); }
		}

		#endregion

		#region WDH_ReferenceNumber

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDH_ReferenceNumber
		{
			get { return base.WDH_ReferenceNumber; }
			set { base.WDH_ReferenceNumber = value; }
		}

		#endregion

		#region WDH_GateInTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WDH_GateInTime
		{
			get { return base.WDH_GateInTime; }
			set { base.WDH_GateInTime = value; }
		}

		public ZDateTimeOffset GateInTime => WDH_GateInTime;

		#endregion

		#region WDH_LoadCompleteTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WDH_LoadCompleteTime
		{
			get { return base.WDH_LoadCompleteTime; }
			set { base.WDH_LoadCompleteTime = value; }
		}

		#endregion

		#region WDH_GateOutTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WDH_GateOutTime
		{
			get { return base.WDH_GateOutTime; }
			set { base.WDH_GateOutTime = value; }
		}

		#endregion

		#region WDH_FinalisedTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		[ResourceStringData("WhsItemDispatchTransportationUnit|WDH_FinalisedTime", Caption = "Finalized Time")]
		public override ZDateTimeOffset WDH_FinalisedTime
		{
			get { return base.WDH_FinalisedTime; }
			set
			{
				base.WDH_FinalisedTime = value;
			}
		}

		#endregion

		#region WDH_SystemCreateTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WDH_SystemCreateTimeUtc
		{
			get { return base.WDH_SystemCreateTimeUtc; }
			set { base.WDH_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region WDH_SystemCreateUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDH_SystemCreateUser
		{
			get { return base.WDH_SystemCreateUser; }
			set { base.WDH_SystemCreateUser = value; }
		}

		#endregion

		#region WDH_SystemLastEditTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WDH_SystemLastEditTimeUtc
		{
			get { return base.WDH_SystemLastEditTimeUtc; }
			set { base.WDH_SystemLastEditTimeUtc = value; }
		}

		#endregion

		#region WDH_SystemLastEditUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WDH_SystemLastEditUser
		{
			get { return base.WDH_SystemLastEditUser; }
			set { base.WDH_SystemLastEditUser = value; }
		}

		#endregion

		#region WDH_SignedBySignature

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBlob WDH_SignedBySignature
		{
			get { return base.WDH_SignedBySignature; }
			set { base.WDH_SignedBySignature = value; }
		}

		#endregion

		#region Finalise

		public void Finalise(ZDateTimeOffset finalisedTime, string reason)
		{
			if (!WDH_GateOutTime.IsEmpty && finalisedTime.IsValid && WDH_GateOutTime <= finalisedTime)
			{
				WDH_FinalisedTime = finalisedTime;
				var dateTimeOffsetNow = TransitWarehouseHelper.GetNowInCurrentWarehouse(Warehouse);

				var (allInnerDTUs, allTargetPackageStates) = UpdateAllPackageStatesAndTheirInnersStatus(finalisedTime, TransitWarehouseStatuses.Codes.Finalized);
				UpdateAllDTUsLogs(allInnerDTUs, dateTimeOffsetNow, TransitWarehouseStatuses.Codes.Finalized, reason, (NoResString)"Finalised");
			}
		}

		#endregion

		#region HasContainerEquipmentDetails

		public bool HasContainerEquipmentDetails => PackageExtension?.Package?.Container != null;

		#endregion

		#region IsGatedOut

		public ZBool IsGatedOut
		{
			get => !WDH_GateOutTime.IsEmpty;
			set
			{
				var cntNeedLoadingOntoVehicle = WarehouseDataRegistry.Instance.CNTNeedLoadingOntoVehicle.Value;
				var uldNeedLoadingOntoVehicle = WarehouseDataRegistry.Instance.ULDNeedLoadingOntoVehicle.Value < ZDateTime.Today;
				var canGateOut = WDH_UnitType == TransportUnitTypes.Vehicle
							  || (WDH_UnitType == TransportUnitTypes.Container && (ContainerizedPackageState.WPS_WDH_TransitDispatchHeader.IsEmpty && !cntNeedLoadingOntoVehicle))
							  || (WDH_UnitType == TransportUnitTypes.ULD && (ContainerizedPackageState.WPS_WDH_TransitDispatchHeader.IsEmpty && !uldNeedLoadingOntoVehicle));
				if (value && canGateOut)
				{
					var dateTimeOffsetNow = TransitWarehouseHelper.GetNowInCurrentWarehouse(Warehouse);
					var gateOutTime = new ZDateTimeOffset(dateTimeOffsetNow.Year, dateTimeOffsetNow.Month, dateTimeOffsetNow.Day, dateTimeOffsetNow.Hour, dateTimeOffsetNow.Minute, 0, dateTimeOffsetNow.Offset);
					if (!WDH_LoadCompleteTime.IsEmpty && gateOutTime.IsValid)
					{
						var (allDTUs, allTargetPackageStates) = UpdateAllPackageStatesAndTheirInnersStatus(gateOutTime, TransitWarehouseStatuses.Codes.Departed);
						UpdateAllDTUsLogs(allDTUs, dateTimeOffsetNow, TransitWarehouseStatuses.Codes.Departed);
					}
				}
			}
		}

		#endregion

		#region UpdateAllDTUsAndTheirInnersStatus

		(WhsItemDispatchTransportationUnit[], WhsItemPackageState[]) UpdateAllPackageStatesAndTheirInnersStatus(ZDateTimeOffset time, string status)
		{
			var availableULDAndCNTPKs = PackageStates
				.Where(p => p.WPS_UnitType == PackageStateUnitType.Codes.AirULDContainer || p.WPS_UnitType == PackageStateUnitType.Codes.SeaContainer)
				.SelectMany(p => p.Package.PackageExtensions.Where(e => e.KPN_ParentTableCode == WhsItemDispatchTransportationUnitSchema.Constants.Prefix)
				.Select(e => e.KPN_ParentID)).Distinct();

			var allInnerDTUs = Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery(WhsItemDispatchTransportationUnitSchema.PK, availableULDAndCNTPKs));

			var allDTUs = allInnerDTUs.Concat(new[] { this }).ToArray();

			foreach (var dtu in allDTUs)
			{
				if (status == TransitWarehouseStatuses.Codes.Finalized)
				{
					dtu.WDH_FinalisedTime = time;
					if (dtu.WDH_GateOutTime.IsEmpty)
					{
						dtu.WDH_GateOutTime = time;
					}
				}
				else if (status == TransitWarehouseStatuses.Codes.Departed)
				{
					dtu.WDH_GateOutTime = WDH_LoadCompleteTime <= time ? time : WDH_LoadCompleteTime;
				}
			}

			var packageStateInDTUQuery = new ZQuery(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader, PK);
			packageStateInDTUQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, status);

			var packageStateQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			packageStateQuery.AddToFilter(packageStateInDTUQuery);

			var currentDTUisVehicle = WDH_UnitType == TransportUnitTypes.Vehicle;
			if (currentDTUisVehicle)
			{
				var pkgPackagePKlist = PackageStates
					.Where(p => p.WPS_UnitType == PackageStateUnitType.Codes.AirULDContainer || p.WPS_UnitType == PackageStateUnitType.Codes.SeaContainer)
					.Select(p => p.WPS_KP_Package);

				var pkgPackageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), WhsItemPackageStateSchema.WPS_KP_Package, PkgPackageSchema.PK);
				pkgPackageSubQuery.AddToFilter(PkgPackageSchema.KP_KP_TopHandlingUnitPackage, pkgPackagePKlist);

				packageStateQuery.AddSubQuery(pkgPackageSubQuery, JoinCondition.Or);
			}
			else
			{
				var packageExtensionSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageExtension), WhsItemPackageStateSchema.WPS_KP_Package, PkgPackageExtensionSchema.KPN_KP_Package);
				packageExtensionSubQuery.AddToFilter(PkgPackageExtensionSchema.KPN_ParentID, PK);

				packageStateQuery.AddSubQuery(packageExtensionSubQuery, JoinCondition.Or);
			}

			var allTargetPackageStates = Factory.Load<WhsItemPackageState>(packageStateQuery);

			var uldAndCNTPackageStates = PackageStates.Where(p => p.WPS_UnitType == PackageStateUnitType.Codes.AirULDContainer || p.WPS_UnitType == PackageStateUnitType.Codes.SeaContainer);
			foreach (var targetPackageState in allTargetPackageStates)
			{
				if (!targetPackageState.WPS_WDL_LoadList.IsEmpty)
				{
					if (targetPackageState.WPS_LoadedTime.IsEmpty)
					{
						targetPackageState.WPS_LoadedTime = WDH_LoadStartTime;
					}
					if (targetPackageState.WPS_WDH_TransitDispatchHeader.IsEmpty)
					{
						if (currentDTUisVehicle)
						{
							var uldCNT = uldAndCNTPackageStates.FirstOrDefault(p => p.WPS_KP_Package == targetPackageState.Package.KP_KP_TopHandlingUnitPackage);
							var uldCNTPK = uldCNT?.Package.PackageExtensions.Where(e => e.KPN_ParentTableCode == WhsItemDispatchTransportationUnitSchema.Constants.Prefix).Select(e => e.KPN_ParentID).FirstOrDefault();
							targetPackageState.WPS_WDH_TransitDispatchHeader = uldCNTPK ?? PK;
						}
						else if (targetPackageState.WPS_UnitType != PackageStateUnitType.Codes.AirULDContainer &&
								targetPackageState.WPS_UnitType  != PackageStateUnitType.Codes.SeaContainer)
						{
							targetPackageState.WPS_WDH_TransitDispatchHeader = PK;
						}
					}
				}
				targetPackageState.WPS_Status = status;
			}

			return (allDTUs, allTargetPackageStates);
		}

		void UpdateAllDTUsLogs(WhsItemDispatchTransportationUnit[] allDTUs, ZDateTimeOffset dateTimeOffsetNow, string status, string reason = null, string message = null)
		{
			if (status == TransitWarehouseStatuses.Codes.Finalized)
			{
				allDTUs.ForEach(d => d.Logs.AddNew(
					Events.StatusUpdated,
					d.WDH_ReferenceNumber,
					dateTimeOffsetNow.ToDateTime(),
					new KeyValuePair<string, string>("RES", reason),
					new KeyValuePair<string, string>("TYP", message)));
			}
			else if (status == TransitWarehouseStatuses.Codes.Departed)
			{
				var whs = Warehouse;

				allDTUs.ForEach(d => d.Logs.AddNew(
					Events.GateOut,
					d.WDH_ReferenceNumber,
					dateTimeOffsetNow.ToDateTime(),
					new KeyValuePair<string, string>("FAC", CargoWise.EventReference.Constants.Facilities.Code.Depot),
					new KeyValuePair<string, string>("LOC", whs != null && whs.WarehouseAddress != null ? (string)whs.WarehouseAddress.OA_City : string.Empty),
					new KeyValuePair<string, string>("TYP", d.WDH_UnitType == TransportUnitTypes.ULD ? "ULDID" : d.WDH_UnitType != TransportUnitTypes.Vehicle ? "ContainerID" : "VehicleReference"),
					new KeyValuePair<string, string>("REF", WDH_VehicleReference),
					new KeyValuePair<string, string>("WHS", whs != null ? (string)whs.WW_WarehouseCode : string.Empty)));
			}
		}

		#endregion

		#region PackageStates

		public WhsItemPackageStateCollection PackageStates
		{
			get { return packageStates ?? (packageStates = new WhsItemPackageStateCollection(this)); }
		}

		WhsItemPackageStateCollection packageStates;

		public IEnumerable<WhsItemPackageState> ValidPackageStatesInPendingDLLs
		{
			get
			{
				if (validPackageStatesInPendingDLLs == null)
				{
					var pendingDLLs = DispatchLoadLists.Where(dll => dll.WDL_CompleteTime.IsEmpty && dll.WDL_IsActive);
					validPackageStatesInPendingDLLs = pendingDLLs.SelectMany(dll => dll.PackageStates)
						.Where(wps => wps.WPS_Status != TransitWarehouseStatuses.Codes.Departed &&
										wps.WPS_Status != TransitWarehouseStatuses.Codes.AdjustedOut &&
										wps.WPS_Status != TransitWarehouseStatuses.Codes.Finalized)
						.ToList();
				}
				return validPackageStatesInPendingDLLs;
			}
		}

		IEnumerable<WhsItemPackageState> validPackageStatesInPendingDLLs;

		#endregion

		#region ContainerOrEquipmentDetails

		PkgPackageContainer ContainerOrEquipmentDetails => PackageExtension?.Package?.Container;

		#endregion

		#region ContainerNumber

		public ZString ContainerNumber => IsContainerUnitType ? WDH_VehicleReference : ZString.Empty;

		#endregion

		#region VehicleNumber

		public ZString VehicleNumber => IsVehicleUnitType ? WDH_VehicleReference : ZString.Empty;

		#endregion

		#region ContainerTypeCode

		public ZString ContainerTypeCode => HasContainerEquipmentDetails ? ContainerOrEquipmentDetails?.ContainerType?.RC_Code ?? ZString.Empty : ZString.Empty;

		#endregion

		#region ContainerISOType

		public ZString ContainerISOType => HasContainerEquipmentDetails ? ContainerOrEquipmentDetails?.ContainerType?.RC_ISOType ?? ZString.Empty : ZString.Empty;

		#endregion

		#region IsContainerUnitType

		public bool IsContainerUnitType => WDH_UnitType == TransportUnitTypes.Container || WDH_UnitType == TransportUnitTypes.ULD || (WDH_UnitType == TransportUnitTypes.Vehicle && HasContainerEquipmentDetails);

		#endregion

		#region IsVehicleUnitType

		public bool IsVehicleUnitType => WDH_UnitType == TransportUnitTypes.Vehicle;

		#endregion

		#region DispatchLoadLists

		public WhsItemDispatchLoadListCollection DispatchLoadLists => dispatchLoadLists ??= new WhsItemDispatchLoadListCollection(Factory, this);

		WhsItemDispatchLoadListCollection dispatchLoadLists;

		#endregion

		#region Container

		public PkgPackageContainer Container => PackageExtension?.Package?.Container;

		#endregion

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
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

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return WDH_ReferenceNumber.IsEmpty ?
					Res.GetString("1869F580-5B1C-48B3-827E-843EEDFA7A25", "Dispatch Transportation Unit") :
					Res.GetString("E1B316C1-E1AB-4233-9C4F-AE87F272AAAE", "Dispatch Transportation Unit {0}", WDH_ReferenceNumber);
			}
		}

		#endregion

		#region FormarttedReference

		public ZString FormattedReference => !string.IsNullOrEmpty(WDH_VehicleReference) ?
			TransitWarehouseHelper.GetFormattedReferenceString(WDH_VehicleReference, WDH_ReferenceNumber) :
			TransitWarehouseHelper.GetFormattedReferenceString(ContainerTypeCode, WDH_ReferenceNumber);

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

		ZString IPackingParent.ConnoteNo
		{
			get { return ZString.Empty; }
		}

		ZString IPackingParent.JobNo
		{
			get { return WDH_ReferenceNumber; }
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

		#region PackageExtension

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
				if (containerizedPackageState == null && (WDH_UnitType == TransportUnitTypes.ULD || WDH_UnitType == TransportUnitTypes.Container) && PackageExtension != null)
				{
					containerizedPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, PackageExtension.KPN_KP_Package));
				}

				return containerizedPackageState;
			}
		}
		WhsItemPackageState containerizedPackageState;

		#endregion

		#region IPackingParentWithOutturn

		IContainerView IPackingParentWithOutturn.GetContainerView(PkgPackageContainer container) => new ContainerViewForDispatch(container);

		IOutturnProvider IPackingParentWithOutturn.GetOutturnProvider(PkgPackage package) => null;

		(string ContainerNumber, PkgPackageContainer Container) IPackingParentWithOutturn.GetParentContainer(PkgPackage package)
		{
			return (null, null);
		}
		int IPackingParentWithOutturn.TotalNumberOfPiecesOutturned => PackageStates.Sum(p => p.Package.KP_PackageQty);

		#endregion

		#region IJobNumber Memebers

		public string JobNumber => WDH_ReferenceNumber;

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
				JobDocAddressDependentCollection docAddresses = null;
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
				}

				return docAddresses;
			}
		}

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
					docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.TransitDispatchTransportationUnit);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.TransitDispatchTransportationUnit; }
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsItemDispatchTransportationUnitProcessTaskCollection(this));
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
			result.Add(ProcessTaskTemplateSchema.P0_WW, WDH_WW_Warehouse, ZGuid.Empty);
			return result;
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsItemDispatchTransportationUnitDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new WhsItemDispatchTransportationUnitProcessHandlingInfoProvider(this); }
		}

		#endregion

		#region IJobCostingPlugIn Members

		public ZString JK_UniqueConsignRef => WDH_ReferenceNumber;

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

		public IGenericJobCostSupporter CostSupporter => costSupporter ?? (costSupporter = new WhsItemTransportationUnitCostSupporter<WhsItemDispatchTransportationUnit>(this));

		IGenericJobCostSupporter costSupporter;

		public decimal ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK) => 0m;

		public void AddNewToLogs(Event @event, ZString reference) => Logs.AddNew(@event, reference);

		public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob) => ZString.Empty;

		public ZString TransportMode
		{
			get
			{
				return PackageStates.FirstOrDefault(p => !string.IsNullOrEmpty(p.DispatchConsignment?.WDC_TransportMode ?? ""))?.DispatchConsignment?.WDC_TransportMode ?? "";
			}
		}

		#endregion

		#region ITransportationUnitForCost Member

		public ZGuid CreditorPK => TransitWarehouseHelper.GetTransportCompanyOrganisation(TransportCompany)?.PK ?? ZGuid.Empty;

		public IEnumerable<ZString> DefaultChargeGroups => new ZString[] { ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit };

		public ZString MasterBillNum => MasterBillNumber;

		IEnumerable<WhsItemDispatchConsignment> dispatchConsignments => PackageStates.Where(t => t.DispatchConsignment != null).Select(t => t.DispatchConsignment).Distinct();

		public IEnumerable<IJobInvoicingPlugIn> Consignments => dispatchConsignments.Any()
			? dispatchConsignments
			: Enumerable.Empty<IJobInvoicingPlugIn>();

		public ZDateTime ETA => ZDateTime.Empty;

		public ZDateTime ETD => ZDateTime.Empty;

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(TransitWarehouseNoteHelper.GetNoteTypes());
				types.Add(PredefinedNoteTypes.Instance.UnloadLoadNotes);
				return types;
			}
		}

		#endregion

		#region InvoicingPlugIn

		public IJobInvoicingPlugIn InvoicingPlugIn => invoicingPlugIn ?? (invoicingPlugIn = new TransitJobInvoicingPlugIn<WhsItemDispatchTransportationUnit>(this));
		IJobInvoicingPlugIn invoicingPlugIn;

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => InvoicingPlugIn.InvoicingSupporter;

		#region IJobHeaderParent Members

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			if (WDH_ReferenceNumber.IsEmpty)
			{
				WDH_ReferenceNumber = (ZString)Env.NumberFountains.TransitWarehouseDispatchID.GetNextFormatted(Factory);
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

		JobInvoicingConsumerType ITransitJobInvoicingPlugIn.ConsumerType => JobInvoicingConsumerTypes.TransitDispatchTransportationUnit;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.AuditSecurity => Env.Security.WhsItemDispatchTransportationUnitAuditBilling;

		SecurityCheckpoint ITransitJobInvoicingPlugIn.JobInvoicingSecurity => Env.Security.WhsItemDispatchTransportationUnitJobInvoicing;

		IJobInvoicingSupporter ITransitJobInvoicingPlugIn.InvoicingSupporter => new TransitJobInvoicingSupporter<WhsItemDispatchTransportationUnit>(this);

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider => new WhsItemTransportationUnitRatingAdaptersProvider<WhsItemDispatchTransportationUnit>(this);

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
					var transportModesFromDLLs = DispatchLoadLists.Select(dll => dll.WDL_TransportMode).Where(t => !t.IsEmpty).Distinct();
					if (transportModesFromDLLs.Any())
					{
						var freightModes = transportModesFromDLLs.Select(t => TransitWarehouseHelper.GetFreightModeByTransportMode(t)).Distinct();
						mode = freightModes.Count() == 1 ? freightModes.First() : FreightMode.UKN;
					}
				}

				return mode;
			}
		}

		ZDateTime ITransitJobForRating.ExpectedArrivalDate => WDH_GateInTime.ToLocalZDateTime();

		ZDateTime ITransitJobForRating.ExpectedDepartureDate => WDH_LoadCompleteTime.ToLocalZDateTime();

		string ITransitJobForRating.ChargeCodeGroup => ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit;

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
		WPS_WDH_TransitDispatchHeader = @DTUPK
),
HandlingUnits AS
(
	SELECT WPS_PK, WPS_KP_Package
	FROM
		PackageStates
	WHERE
		WPS_IsHandlingUnit = 1
),
HandlingUnitInners AS
(
	SELECT KP_PK AS InnerPackagePK
	FROM
		HandlingUnits
		JOIN dbo.PkgPackage ON KP_KP_TopHandlingUnitPackage = HandlingUnits.WPS_KP_Package
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
	WPS_KP_Package NOT IN (SELECT InnerPackagePK FROM HandlingUnitInners)";
					var sqlParams = new ZSqlParameterCollection
					{
						{ "@DTUPK", PK, WhsItemReceiveConsignmentSchema.PK },
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

		#region IWhsItemDispatchTransportationUnit Members

		IWhsWarehouse IWhsItemDispatchTransportationUnit.Warehouse => Warehouse;
		IConsignment IWhsItemDispatchTransportationUnit.LatestDispatchConsignment => dispatchConsignments.OrderByDescending(c => c.WDC_SystemCreateTimeUtc).FirstOrDefault();
		ZString IWhsItemDispatchTransportationUnit.UnitType => WDH_UnitType;
		ZString IWhsItemDispatchTransportationUnit.ReferenceNumber
		{
			get => WDH_ReferenceNumber;
			set => WDH_ReferenceNumber = value;
		}
		ZGuid IWhsItemDispatchTransportationUnit.PK => this.PK;
		#endregion

		#region JobHeader

		public JobHeader JobHeader
		{
			get { return new JobHeader.Loader(this).Load(); }
		}

		#endregion

		#region IEdocsProvider

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsItemDispatchTransportationUnitFetchStrategy(this);
		}

		#endregion
	}
}
