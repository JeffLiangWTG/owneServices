using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.US.USAMS;
using ImportAction = Enterprise.Customs.Business.ImportAction;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	[UniversalDataContext(DataContextType.USAMS)]
	public class CusInBondHeader : Customs.Business.CusInBondHeader,
		Integration.Customs.US.USAMS.ICusInBondHeader,
		ICusInBondHeaderWithConsolSynchonisation,
		Messaging.Business.IMessageActionHeader,
		IDispositionCodeDateParent,
		IDispositionCodeColumnsForFastSearchProvider,
		IValidationModesSupporter,
		ISailingParentFindBox,
		IManifestHeaderForSynchroniser,
		ICusAddInfoTypeSupporter,
		IDocumentSupportable,
		IDocManagerSupport,
		IWorkflowProvider,
		IWorkflowTriggerEventSource,
		IJobNumber,
		ICustomsManifestMessageSupporter,
		IValidateForCustomsMessagingSupporter
	{
		public CusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			EnsureMoveHeaderExists();
		}

		void EnsureMoveHeaderExists()
		{
			var moveHeader = MovementHeader;
		}

		public new class Schema : AutoCusInBondHeader.Schema
		{
			public const string ImporterOrgPK = "ImporterOrgPK";
			public const string BH_IsPaperlessMIBParticipant = "BH_IsPaperlessMIBParticipant";
			public const string BH_IsOutboundCargo = "BH_IsOutboundCargo";
			public const string BH_NoOfAMSBills = "BH_NoOfAMSBills";
			public const string BH_NoOfSailingBills = "BH_NoOfSailingBills";
			public const string BH_NoOfAMSContainers = "BH_NoOfAMSContainers";
			public const string BH_NoOfSailingContainers = "BH_NoOfSailingContainers";
			public const string BH_RL_NKPortOfLoading = "RL_NKPortOfLoading";
			public const string BH_LatestDispositionCode = "BH_LatestDispositionCode";
			public const string BH_LatestDispositionCodeDescription = "BH_LatestDispositionCodeDescription";
		}

		#region New Properties

		public static TriLockMutex CreateMutex(ZGuid boPK)
		{
			return new TriLockMutex(MutexIDs.AMSJobBeingCreatedForConsol, boPK.ToString());
		}

		public override bool ShouldSynchronise
		{
			get { return !BH_OverrideFreightDefaults && Consol != null && MovementHeader.Messages.Count == 0; }
		}

		public ZInt VoyageNumberMaxLength
		{
			get { return 5; }
		}

		public ZBool IsNVOCCHeader
		{
			get { return BH_TransitDirection == DirectionTypeList.Codes.NVOCC; }
		}

		public override ZGuid BH_GB
		{
			get { return base.BH_GB; }
			set
			{
				var oldValue = BH_GB;
				var hasChanged = oldValue != value;
				base.BH_GB = value;
				if (hasChanged)
				{
					BH_CarrierSCAC = SCACInCarrier;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|BH_LatestDispositionCode", Caption = "Latest Disposition", MediumCaption = "Latest Disp.", ShortCaption = "Disposition")]
		public ZString BH_LatestDispositionCode => DistinctLatestDispositionCodesAndDescriptions.DispositionCodes;

		public ZPropertyInfo BH_LatestDispositionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BH_LatestDispositionCode); }
		}

		[ResourceStringData("Enterprise.Customs.US.AMS.Business|BH_LatestDispositionCodeDescription", Caption = "Latest Disposition Description", MediumCaption = "Latest Disp. Desc.", ShortCaption = "Disposition Desc.")]
		public ZString BH_LatestDispositionCodeDescription => DistinctLatestDispositionCodesAndDescriptions.DispositionDescriptions;

		public ZPropertyInfo BH_LatestDispositionCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.BH_LatestDispositionCodeDescription); }
		}

		(ZString DispositionCodes, ZString DispositionDescriptions) DistinctLatestDispositionCodesAndDescriptions
		{
			get
			{
				if (cachedDistinctLatestDispositionCodesAndDescriptions == null)
				{
					cachedDistinctLatestDispositionCodesAndDescriptions = new CachedProperty<(ZString DispositionCodes, ZString DispositionDescriptions)>(Factory, () =>
					{
						var dispositionCodesList = new List<ZString>();
						var dispositionDescriptionsList = new List<ZString>();
						foreach (var bill in this.Bills)
						{
							if (!bill.B0_LatestDispositionCode.IsEmpty)
							{
								dispositionCodesList.Add(bill.B0_LatestDispositionCode);
								dispositionDescriptionsList.Add(bill.B0_LatestDispositionCodeDescription);
							}
						}
						var dispositionCodes = ZString.Join("| ", dispositionCodesList.Distinct().ToArray());
						var dispositionDescriptions = ZString.Join("| ", dispositionDescriptionsList.Distinct().ToArray());
						return (dispositionCodes, dispositionDescriptions);
					});
				}
				return cachedDistinctLatestDispositionCodesAndDescriptions.Value;
			}
		}
		CachedProperty<(ZString DispositionCodes, ZString DispositionDescriptions)> cachedDistinctLatestDispositionCodesAndDescriptions;

		public ZString SCACInCarrier
		{
			get
			{
				var result = ZString.Empty;

				var branchGB = Factory.Load<GlbBranch>(BH_GB);
				var orgProxy = branchGB != null ? Factory.Load<OrgHeader>(branchGB.GB_OH_OrgProxy) : null;
				if (orgProxy != null)
				{
					result = orgProxy.SCACCode.Left(CusInBondHeader.Schema.BH_CarrierSCACMaxLength);
				}

				if (result == ZString.Empty)
				{
					orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					result = orgProxy != null ? orgProxy.SCACCode.Left(CusInBondHeader.Schema.BH_CarrierSCACMaxLength) : ZString.Empty;
				}

				return result;
			}
		}

		internal bool ShouldSynchroniseWithSailing
		{
			get { return !BH_OverrideFreightDefaults && Sailing != null && MovementHeader.Messages.Count == 0; }
		}

		internal bool ShouldSynchroniseWithConsolOrSailing
		{
			get { return ShouldSynchronise || ShouldSynchroniseWithSailing; }
		}

		#region ImporterOrgPK

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.Importers))]
		public ZGuid ImporterOrgPK
		{
			get { return BH_OA_Importer_ZAddress.OrgPK; }
			set { BH_OA_Importer_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ImporterOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ImporterOrgPK, x => BH_OA_Importer_ZAddress.OrgPKInfo); }
		}

		public OrgHeader ImporterOrg
		{
			get { return (OrgHeader)BH_OA_Importer_ZAddress.OrgHeader; }
		}
		#endregion

		#region BH_IsPaperlessMIBParticipant
		public ZBool BH_IsPaperlessMIBParticipant
		{
			get { return BH_HeaderType == ModeTypeList.Codes.PaperlessMIBParticipant; }
			set
			{
				var oldValue = BH_IsPaperlessMIBParticipant;
				BH_HeaderType = value ? ModeTypeList.Codes.PaperlessMIBParticipant : "";
				BH_IsPaperlessMIBParticipantInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BH_IsPaperlessMIBParticipantInfo
		{
			get { return GetZPropertyInfo(Schema.BH_IsPaperlessMIBParticipant); }
		}

		#endregion

		#region BH_IsOutboundCargo
		public ZBool BH_IsOutboundCargo
		{
			get { return BH_ExportFlag == ExportTypeList.Codes.OutboundCargo; }
			set
			{
				var oldValue = BH_IsOutboundCargo;
				BH_ExportFlag = value ? ExportTypeList.Codes.OutboundCargo : "";
				BH_IsOutboundCargoInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BH_IsOutboundCargoInfo
		{
			get { return GetZPropertyInfo(Schema.BH_IsOutboundCargo); }
		}

		#endregion

		#region BH_OverrideFreightDefaults

		public event CancelEventHandler OnOverrideFreightDefaultsUnchecking;
		public event EventHandler OnOverrideFreightDefaultsChanging;

		public sealed override ZBool BH_OverrideFreightDefaults
		{
			get { return base.BH_OverrideFreightDefaults; }
			set
			{
				SetOverrideDefaults(value);
				if (!BH_OverrideFreightDefaults)
				{
					UpdateVesselDetails();
				}
			}
		}

		#endregion

		#region BH_NoOfAMSBills

		public ZInt BH_NoOfAMSBills
		{
			get { return Bills.Count; }
		}

		public ZPropertyInfo BH_NoOfAMSBillsInfo
		{
			get { return GetZPropertyInfo(Schema.BH_NoOfAMSBills); }
		}

		#endregion

		#region BH_NoOfSailingBills

		public ZInt BH_NoOfSailingBills
		{
			get { return BillsOfLading.Count(); }
		}

		public ZPropertyInfo BH_NoOfSailingBillsInfo
		{
			get { return GetZPropertyInfo(Schema.BH_NoOfSailingBills); }
		}

		#endregion

		#region BH_NoOfAMSContainers

		public ZInt BH_NoOfAMSContainers
		{
			get { return Bills.Sum(x => x.B0_NoOfAMSContainers); }
		}

		public ZPropertyInfo BH_NoOfAMSContainersInfo
		{
			get { return GetZPropertyInfo(Schema.BH_NoOfAMSContainers); }
		}

		#endregion

		#region BH_NoOfSailingContainers

		public ZInt BH_NoOfSailingContainers
		{
			get
			{
				if (!fBH_NoOfSailingContainers.HasValue)
				{
					fBH_NoOfSailingContainers = BillsOfLading.Sum(x => x.RealContainers.Count);
				}
				return fBH_NoOfSailingContainers.Value;
			}
		}
		ZInt? fBH_NoOfSailingContainers;

		public ZPropertyInfo BH_NoOfSailingContainersInfo
		{
			get { return GetZPropertyInfo(Schema.BH_NoOfSailingContainers); }
		}

		#endregion

		public bool IsRail
		{
			get { return BH_ImportTransportMode == TransportTypeList.Codes.Rail; }
		}

		public bool HasHVLVParent => (ParentBusinessObject is CommonShipment parentShipment) && parentShipment.IsHighVolumeLowValue;

		public ZDateTime NVOCCActualArrivalDate
		{
			get { return PortArrivalDetails.Count == 1 ? PortArrivalDetails[0].ActualArrivalDate : ZDateTime.Empty; }
		}

		public ZString ActualArrivalDate
		{
			get
			{
				var uniqueArriveDates = PortArrivalDetails.OfType<PortArrivalDetail>().Select(x => x.ActualArrivalDate).Distinct().Take(2).ToArray();
				return uniqueArriveDates.Length > 1 ? "MULTIPLE" : uniqueArriveDates.Length == 1 ? uniqueArriveDates.FirstOrDefault().ToLongTimeString() : string.Empty;
			}
		}

		#region BH_RL_NKPortOfLoading

		public ZString BH_RL_NKPortOfLoading
		{
			get { return (Sailing?.JX_JA_RL_NKPortOfLoading).GetValueOrDefault(); }
		}

		#endregion

		#endregion

		#region Related Objects

		internal IEnumerable<BillOfLading> BillsOfLading
		{
			get
			{
				if (fBillsOfLading == null && Sailing != null)
				{
					fBillsOfLading = new List<BillOfLading>(new BillsOfLadingAtPortStrategy(Sailing.Voyage).GetBillsOnVesselAt(Sailing.JX_JB_E_ARV, false));
				}
				return fBillsOfLading ?? Enumerable.Empty<BillOfLading>();
			}
		}
		List<BillOfLading> fBillsOfLading;

		#region MessageAttacheesRelatedRecords

		/// <summary>
		/// This is the collection Messages tab is bound to. MoveHeader, PTTMoveHeader are shown
		/// </summary>
		[ChildEditable(true)]
		public MessageActionRelatedRecordWrapperCollection MessageAttacheesRelatedRecords
		{
			get
			{
				if (messageAttacheesRelatedRecords == null)
				{
					messageAttacheesRelatedRecords = new MessageActionRelatedRecordWrapperCollection(this, new GetMessageAttacheesToBuild(GetMessageAttacheesToShowInMessagesTab));
					RegisterEditableChildObject(messageAttacheesRelatedRecords);
				}
				return messageAttacheesRelatedRecords;
			}
		}
		MessageActionRelatedRecordWrapperCollection messageAttacheesRelatedRecords;

		IEnumerable<IMessageAttacheeInHeader> GetMessageAttacheesToShowInMessagesTab()
		{
			if (MovementHeader.Messages.Count > 0)
			{
				yield return MovementHeader;
			}

			foreach (var pttMoveHeader in PTTMovements.OfType<CusInBondMoveHeader>())
			{
				if (pttMoveHeader.Messages.Count > 0)
				{
					yield return pttMoveHeader;
				}
			}

			foreach (var inBondMoveHeader in InBondMovementHeaders.OfType<CusInBondMoveHeader>())
			{
				if (inBondMoveHeader.Messages.Count > 0)
				{
					yield return inBondMoveHeader;
				}
			}
		}

		IReadOnlyList<IMessageAttacheeInHeader> Messaging.Business.IMessageActionHeader.MessageAttachees => MessageAttachees;

		internal IMessageAttacheeInHeader[] MessageAttachees
		{
			get
			{
				var result = new List<IMessageAttacheeInHeader>
				{
					MovementHeader
				};
				result.AddRange(PTTMovements.OfType<CusInBondMoveHeader>());
				result.AddRange(InBondMovementHeaders.OfType<CusInBondMoveHeader>());
				return result.ToArray();
			}
		}

		void Messaging.Business.IMessageActionHeader.RebuildMessageAttacheesRelatedRecords(IMessageAttacheeInHeader messageAttachee)
		{
			MessageAttacheesRelatedRecords.RebuildIfNecessary(messageAttachee);
		}

		#endregion

		public CusInBondBill OceanBill
		{
			get
			{
				if (oceanBill == null || oceanBill.IsDeleted || oceanBill.B0_BH != PK)
				{
					if (oceanBill != null)
					{
						UnRegisterEditableChildObject(oceanBill);
					}
					oceanBill = null;
					if (IsNVOCCHeader)
					{
						oceanBill = Factory.LoadTop1<CusInBondBill>(OceanBillQuery);
						if (oceanBill == null)
						{
							oceanBill = Factory.New<CusInBondBill>();
							oceanBill.B0_ShipmentType = CusInBondBill.OceanBillType;
							oceanBill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.MasterBill;
							oceanBill.B0_BH = PK;
						}
						RegisterEditableChildObject(oceanBill);
					}
				}
				return oceanBill;
			}
		}
		CusInBondBill oceanBill;

		public void RefreshCachedValue() => hasEntryType62Or63 = null;

		public bool HasInbondMoveHeaderEntryType62Or63 => CachedValueHelper.GetValue(ref hasEntryType62Or63, () => InBondMovementHeaders.OfType<CusInBondMoveHeader>().Any(x => x.IsTransportandExportEntryType || x.IsImmediateExportEntryType));
		protected CachedValue<bool> hasEntryType62Or63;

		public CusInBondMoveHeader OceanBillPTTMovement
		{
			get
			{
				if (oceanBillPTTMovement == null || oceanBillPTTMovement.IsDeleted || oceanBillPTTMovement.BM_BH != PK)
				{
					if (oceanBillPTTMovement != null)
					{
						UnRegisterEditableChildObject(oceanBillPTTMovement);
					}
					oceanBillPTTMovement = null;
					if (IsNVOCCHeader && OceanBill != null)
					{
						oceanBillPTTMovement = PTTMovements.OfType<CusInBondMoveHeader>().OrderBy(x => x.BM_SystemCreateTimeUtc).FirstOrDefault();
						if (oceanBillPTTMovement == null)
						{
							oceanBillPTTMovement = PTTMovements.AddNew();
							RegisterEditableChildObject(oceanBillPTTMovement);
						}
					}
				}
				return oceanBillPTTMovement;
			}
		}
		CusInBondMoveHeader oceanBillPTTMovement;

		ZQuery OceanBillQuery
		{
			get
			{
				var result = new ZQuery(CusInBondBillSchema.B0_BH, PK);
				result.AddToFilter(CusInBondBillSchema.B0_ShipmentType, CusInBondBill.OceanBillType);
				result.FetchOnlyFromLocalCache = !IsInDatabase;
				return result;
			}
		}

		public new CusInBondMoveHeader MovementHeader
		{
			get { return (CusInBondMoveHeader)base.MovementHeader; }
		}

		[ChildEditable]
		public CusInBondMoveHeaderCollection InBondMovementHeaders
		{
			get
			{
				if (inBondMovementHeaders == null)
				{
					inBondMovementHeaders = new CusInBondMoveHeaderCollection(this, () => IsNVOCCHeader ? SubApplicationCodeList.Codes.SubsequentInBond : SubApplicationCodeList.Codes.MasterInBond, new ZQuery(CusInBondMoveHeaderSchema.BM_SubApplicationCode, SubApplicationCodeList.Codes.SubsequentInBond));
					RegisterEditableChildObject(inBondMovementHeaders);
				}
				return inBondMovementHeaders;
			}
		}
		CusInBondMoveHeaderCollection inBondMovementHeaders;

		[ChildEditable]
		public CusInBondMoveHeaderCollection PTTMovements
		{
			get
			{
				if (pttMovements == null)
				{
					pttMovements = new CusInBondMoveHeaderCollection(this, SubApplicationCodeList.Codes.PermitToTransfer);
					RegisterEditableChildObject(pttMovements);
				}
				return pttMovements;
			}
		}
		CusInBondMoveHeaderCollection pttMovements;

		[ReadOnly(true)]
		public DispositionDataCollection DispositionCodes
		{
			get
			{
				if (fDispositionCodes == null)
				{
					fDispositionCodes = new DispositionDataCollection(this);
					fDispositionCodes.Load();
				}
				return fDispositionCodes;
			}
		}
		DispositionDataCollection fDispositionCodes;

		[ReadOnly(true)]
		public PortArrivalDetailCollection PortArrivalDetails
		{
			get
			{
				if (fPortArrivalDetails == null)
				{
					fPortArrivalDetails = new PortArrivalDetailCollection(this);
					fPortArrivalDetails.InitialisePortArrival();
				}
				return fPortArrivalDetails;
			}
		}
		PortArrivalDetailCollection fPortArrivalDetails;

		[ChildEditable]
		public new CusInBondBillCollection Bills
		{
			get { return (CusInBondBillCollection)base.Bills; }
		}

		protected override ICusInBondBillCollection GetNewBillsCollection()
		{
			var result = new CusInBondBillCollection(this);
			result.CountChanged += OnNoOfBillsChanged;
			return result;
		}

		public void InitializeNewBill(CusInBondBill newBill)
		{
			if (!BH_CarrierSCAC.IsEmpty)
			{
				newBill.SecondaryNotifyParties.AddNewIfNotExist(BH_CarrierSCAC);
			}

			var oceanBill = OceanBill;
			if (oceanBill != null && !oceanBill.B0_IssuerCode.IsEmpty)
			{
				newBill.SecondaryNotifyParties.AddNewIfNotExist(oceanBill.B0_IssuerCode);
			}
		}

		void OnNoOfBillsChanged(object sender, EventArgs e)
		{
			BH_NoOfAMSBillsInfo.RefreshBinding();
			BH_NoOfAMSContainersInfo.RefreshBinding();
		}

		public new CusInBondHeaderLookups Lookups
		{
			get { return (CusInBondHeaderLookups)base.Lookups; }
		}

		public new CusInBondHeaderValidation Validation
		{
			get { return (CusInBondHeaderValidation)base.Validation; }
		}

		#endregion

		#region CBP 1302 form properties

		public ZString PortOfDischarge
		{
			get
			{
				ZString result = "";
				if (!HasMultiDischargePorts)
				{
					var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BH_PortUnladingDCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
					if (port != null)
					{
						result = port.ZZD_Code + " " + port.ZZD_Description;
					}
				}
				else
				{
					result = "MULTI";
				}
				return result;
			}
		}

		public bool HasMultiDischargePorts
		{
			get { return Bills.Any(x => x.HasDifferentDischargePort); }
		}

		public ZString PortOfLoading
		{
			get
			{
				ZString result = "";
				if (Sailing != null)
				{
					var unloco = Sailing.PortOfLoading;
					var schdk = USScheduleResolver.GetScheduleCode(Schedule.K, unloco.RL_Code, USLocoMapSystemUsageList.Codes.Sea, Factory);
					var portName = unloco.RL_PortName;
					result = schdk + " " + portName;
				}

				return result;
			}
		}

		public ZString ConveyanceEvents
		{
			get
			{
				return DispositionCodes.GetDispositionsFor1302Document(System.Environment.NewLine);
			}
		}

		public CBP1302DocumentLineCollection DocumentLines
		{
			get { return documentLines ?? (documentLines = new CBP1302DocumentLineCollection(this)); }
		}
		CBP1302DocumentLineCollection documentLines;

		#endregion

		#region Override Properties

		public override ZString BH_ParentTableCode
		{
			get { return base.BH_ParentTableCode; }
			set
			{
				var oldValue = BH_ParentTableCode;
				base.BH_ParentTableCode = value;
				if (oldValue != BH_ParentTableCode)
				{
					foreach (var bill in Bills)
					{
						bill.MarkAsNeedingValidation();
						foreach (var container in bill.MovementDetail.Containers)
						{
							container.MarkAsNeedingValidation();
							container.Vehicles.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZGuid BH_ParentID
		{
			get => base.BH_ParentID;
			set
			{
				var oldValue = BH_ParentID;
				base.BH_ParentID = value;
				if (oldValue != BH_ParentID)
				{
					Bills.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.FIRMSList))]
		public override ZString BH_FIRMS
		{
			get { return base.BH_FIRMS; }
			set { base.BH_FIRMS = value; }
		}

		public override ZString BH_TransitDirection
		{
			get { return base.BH_TransitDirection; }
			set
			{
				var oldValue = BH_TransitDirection;
				base.BH_TransitDirection = value;
				if (!IsCopying && oldValue != BH_TransitDirection)
				{
					Bills.MarkAsNeedingValidation();
					if (IsNVOCCHeader)
					{
						EnsureOceanBillDetailExists();
					}
					else if (oldValue == DirectionTypeList.Codes.NVOCC)
					{
						DeleteAllOceanBillDetails();
					}
				}
			}
		}

		public override ZString BH_RL_NKPortUnlading
		{
			get { return base.BH_RL_NKPortUnlading; }
			set
			{
				var oldValue = BH_RL_NKPortUnlading;
				base.BH_RL_NKPortUnlading = value;
				if (!IsCopying)
				{
					var newValue = BH_RL_NKPortUnlading;
					if (!IsNVOCCHeader && oldValue != newValue)
					{
						ClearBillValuesIfSame(newValue, CusInBondBill.Schema.B0_RL_NKInBondPortOfDest);
						Bills.MarkAsNeedingValidation();
					}
					if (oldValue != BH_RL_NKPortUnlading)
					{
						var scheduleCodeFromPortUnlading = USScheduleResolver.GetScheduleCode(Schedule.D, BH_RL_NKPortUnlading, USLocoMapSystemUsageList.Codes.Sea, Factory).Left(BH_PortUnladingDCodeInfo.MaxLength);
						if (!scheduleCodeFromPortUnlading.IsEmpty)
						{
							BH_PortUnladingDCode = scheduleCodeFromPortUnlading;
						}
					}
				}
			}
		}

		public override ZPropertyInfo BH_PortUnladingDCodeInfo
		{
			get { return GetZPropertyInfo(Schema.BH_PortUnladingDCode, Res.GetString("USAMSCusInBondHeader|BH_PortUnladingDCode", "Port of Unlading Schedule D")); }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ScheduleDList))]
		public override ZString BH_PortUnladingDCode
		{
			get
			{
				return base.BH_PortUnladingDCode;
			}
			set
			{
				var oldValue = BH_PortUnladingDCode;
				base.BH_PortUnladingDCode = value;
				if (!IsCopying)
				{
					var newValue = BH_PortUnladingDCode;
					if (!IsNVOCCHeader && oldValue != newValue)
					{
						ClearBillValuesIfSame(newValue, CusInBondBill.Schema.B0_InBondPortOfDestDCode);
						Bills.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZDateTime BH_ETA
		{
			get
			{
				return base.BH_ETA;
			}
			set
			{
				var oldValue = BH_ETA;
				base.BH_ETA = value;
				if (!IsCopying)
				{
					var newValue = BH_ETA;
					if (!IsNVOCCHeader && oldValue != newValue)
					{
						ClearBillValuesIfSame(newValue, CusInBondBill.Schema.B0_DateOfDischarge);
						Bills.MarkAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.SCACList))]
		public override ZString BH_CarrierSCAC
		{
			get { return base.BH_CarrierSCAC; }
			set { base.BH_CarrierSCAC = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ConveyanceTransportTypeList))]
		public override ZString BH_ImportTransportMode
		{
			get { return base.BH_ImportTransportMode; }
			set
			{
				var oldValue = BH_ImportTransportMode;
				base.BH_ImportTransportMode = value;
				if (!IsCopying && oldValue != BH_ImportTransportMode)
				{
					Bills.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.RefVessels))]
		[RelatedBusinessObject("ImportConveyanceName")]
		public override ZString BH_ImportConveyanceName
		{
			get { return base.BH_ImportConveyanceName; }
			set
			{
				var oldValue = BH_ImportConveyanceName;
				base.BH_ImportConveyanceName = RemoveSlashes(value);
				if (!IsCopying && oldValue != BH_ImportConveyanceName)
				{
					UpdateVesselDetails();
				}
			}
		}

		public RefVessel ImportConveyanceName
		{
			get { return Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, BH_ImportConveyanceName); }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.Countries))]
		[RelatedBusinessObject("ImportConveyanceCountry")]
		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public override ZString BH_ImportConveyanceCountry
		{
			get { return base.BH_ImportConveyanceCountry; }
			set { base.BH_ImportConveyanceCountry = value; }
		}

		public RefCountry ImportConveyanceCountry
		{
			get { return Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, BH_ImportConveyanceCountry); }
		}

		public override ZString BH_VoyageNumber
		{
			get { return base.BH_VoyageNumber; }
			set { base.BH_VoyageNumber = RemoveSlashes(value); }
		}

		[ReadOnlyMember(nameof(ShouldSynchroniseWithConsolOrSailing))]
		public override ZString BH_LloydsNumber
		{
			get { return base.BH_LloydsNumber; }
			set { base.BH_LloydsNumber = value; }
		}

		protected override BusinessObjectSynchroniser GetNewSynchroniserCore()
		{
			if (fSynchroniser == null)
			{
				var consol = Consol;
				var sailing = Sailing;
				IBusinessObjectState source = null;
				if (consol != null)
				{
					fSynchroniser = new CusInBondHeaderSynchroniser(this);
					source = consol;
#pragma warning disable
					source.UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
					source.UpdatedByDataRefreshIncludingChildren += OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
				}
				else if (sailing != null)
				{
					fSynchroniser = new CusInBondHeaderSailingSynchroniser(this);
					source = sailing;
				}
				else
				{
					throw new NotSupportedException("You can't synchronise with when you don't have a consol/sailing.");
				}
			}
			return fSynchroniser;
		}
		BusinessObjectSynchroniser fSynchroniser;

		internal void ClearBillValuesIfSame(IZType headerValue, string billFieldName)
		{
			foreach (var bill in Bills)
			{
				var billValue = (IZType)bill[billFieldName];

				if (billValue.Equals(headerValue))
				{
					using (bill.SuspendEffectiveValue(billFieldName, headerValue))
					{
						bill[billFieldName] = billValue.Default;
					}

					var infoToRefresh = bill.ZPropertyInfoHash[billFieldName];
					if (infoToRefresh != null)
					{
						infoToRefresh.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region Validation Modes

		public ValidationModes ValidationModes
		{
			get
			{
				if (!fValidationModes.HasValue)
				{
					fValidationModes = ValidationModes.InventoryRecord;
				}
				return fValidationModes.Value;
			}
			set
			{
				var hasChanges = ValidationModes != value;
				fValidationModes = value;
				if (!IsCopying && hasChanges)
				{
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						MarkAsNeedingValidationIncludingChildren();
					}
					SynchroniseBillValidationModes(value);
				}
			}
		}
		ValidationModes? fValidationModes;

		public bool IsInventoryRecordValidationMode
		{
			get
			{
				return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InventoryRecord) || IsInventoryRecordAmendmentValidationMode;
			}
		}

		public bool IsInventoryRecordAmendmentValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InventoryRecordAmendment); }
		}

		public bool IsPermitToTransferValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.PermitToTransfer); }
		}

		public bool IsSubsequentInBondValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.SubsequentInBond); }
		}

		public bool IsInBondArrivalValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondArrival); }
		}

		public bool IsInBondExportationValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondExportation); }
		}

		public bool IsInBondTOLValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondTOL); }
		}

		public bool IsInBondDiversionValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.InBondDiversion); }
		}

		#endregion

		#region OnSaving and Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateJobReferenceIfNeeded();
		}

		public void PopulateJobReferenceIfNeeded()
		{
			PopulateNumberPropertyIfRequired(BH_JobReferenceInfo, x => GetNewJobReference(x));
		}

		ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			var consol = Consol;
			if (consol != null)
			{
				consol.PopulateJK_UniqueConsignRefIfNeeded();
				result = consol.JK_UniqueConsignRef;
			}
			else
			{
				result = Env.NumberFountains.USAMSJobReference.GetNextFormatted(factory);
			}

			return result;
		}

		#endregion

		#region Override Methods

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				DeleteAllOceanBillDetails();
			}
			base.Delete();
		}

		#endregion

		#region Implementation

		void DeleteAllOceanBillDetails()
		{
			PTTMovements.DeleteAll();
			foreach (var oceanBill in Factory.Load<CusInBondBill>(OceanBillQuery))
			{
				oceanBill.Delete();
			}
		}

		void EnsureOceanBillDetailExists()
		{
			if (!((ISupportDataImporting)this).IsImportingData)
			{
				var oceanBill = OceanBill;
				var oceanBillPTTMovement = OceanBillPTTMovement;
			}
		}

		protected override Type BillTypeCore
		{
			get { return typeof(CusInBondBill); }
		}

		protected override Type MovementHeaderTypeCore
		{
			get { return typeof(CusInBondMoveHeader); }
		}

		protected override Customs.Business.CusInBondMoveHeaderCollection GetMovementHeaders()
		{
			return new CusInBondMoveHeaderCollection(this, SubApplicationCodeList.Codes.AMS);
		}

		void SynchroniseBillValidationModes(ValidationModes modes)
		{
			foreach (var bill in Bills)
			{
				if (bill.ValidationModes == modes)
				{
					bill.ValidationModes = ValidationModes.UseParentValidateMode;
				}
			}
		}

		void OnConsolWasUpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			if (fSynchroniser != null)
			{
				fSynchroniser.Synchronise();
			}
		}

		void SetOverrideDefaults(ZBool value)
		{
			var oldValue = BH_OverrideFreightDefaults;
			if (oldValue != value)
			{
				IBusinessObjectState consol = Consol;
				var source = consol ?? Sailing;
				if (!value && source != null)
				{
					var args = new CancelEventArgs(false);
					if (OnOverrideFreightDefaultsUnchecking != null)
					{
						OnOverrideFreightDefaultsUnchecking(this, args);
					}

					if (!args.Cancel)
					{
						base.BH_OverrideFreightDefaults = value;
						BH_OverrideFreightDefaultsInfo.RefreshBinding(oldValue);
						Synchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
					}
					else
					{
						BH_OverrideFreightDefaultsInfo.RefreshBinding();
					}
				}
				else
				{
					base.BH_OverrideFreightDefaults = value;
					BH_OverrideFreightDefaultsInfo.RefreshBinding(oldValue);
					RemoveSynchroniser();
				}
				if (OnOverrideFreightDefaultsChanging != null)
				{
					OnOverrideFreightDefaultsChanging(this, new EventArgs());
				}

				RefreshBindingIncludingChildren();
			}
		}

		void RemoveSynchroniser()
		{
			if (fSynchroniser != null)
			{
				var consol = Consol;
				if (consol != null)
				{
#pragma warning disable
					((IBusinessObjectState)consol).UpdatedByDataRefreshIncludingChildren -= OnConsolWasUpdatedByDataRefreshIncludingChildren;
#pragma warning restore
				}
				fSynchroniser.SetEnabled(false, fSynchroniser.DetectEnabled);
				fSynchroniser.Dispose();
				fSynchroniser = null;
			}
		}

		ZString RemoveSlashes(ZString value)
		{
			return value.Replace("\\", "").Replace("/", "");
		}

		void UpdateVesselDetails()
		{
			if (!((ISupportDataImporting)this).IsImportingData)
			{
				var vessel = ImportConveyanceName;
				if (vessel != null)
				{
					BH_LloydsNumber = vessel.RV_LloydsNumber.ToUpper();
					BH_ImportConveyanceCountry = vessel.RV_RN_NKCountryOfReg.ToUpper();
				}
				else if (BH_ImportConveyanceName.IsEmpty)
				{
					BH_VoyageNumber = ZString.Empty;
					BH_LloydsNumber = ZString.Empty;
					BH_ImportConveyanceCountry = ZString.Empty;
				}
			}
		}

		protected override Customs.Business.CusInBondHeaderValidation GetNewValidation()
		{
			return new CusInBondHeaderValidation(this);
		}

		protected override Customs.Business.CusInBondHeaderLookups GetNewLookups()
		{
			return new CusInBondHeaderLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
			BH_CarrierSCAC = SCACInCarrier;
			BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Bills);
				return result.ToArray<BusinessObject>();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (Consol == null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#endregion

		#region ICusInBondHeaderWithConsolSynchonisation Members

		void ICusInBondHeaderWithConsolSynchonisation.SynchroniseWithConsolIfNeeded()
		{
			SynchroniseIfNeeded();
		}

		public void SynchroniseIfNeeded()
		{
			if (ShouldSynchroniseWithConsolOrSailing)
			{
				using (GetValidationSuspender())
				{
					Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
					Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
					MovementHeader.Messages.CountChanged -= StopSynchronisationOnMessages_CountChanged;
					MovementHeader.Messages.CountChanged += StopSynchronisationOnMessages_CountChanged;
				}
			}
		}

		protected void StopSynchronisationOnMessages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			MovementHeader.Messages.CountChanged -= new CollectionCountChangedEventHandler(StopSynchronisationOnMessages_CountChanged);
			if (Consol != null || Sailing != null)
			{
				Synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Stop));
			}
		}

		#endregion

		#region IDispositionCodeDateParent Members

		public CodeDescriptionPairList DispositionCodeDescriptionList
		{
			get { return Factory.GetCachedValue<ConveyanceEventCodeList>(); }
		}

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			return "";
		}

		#endregion

		#region IDispositionCodeColumnsForFastSearchProvider Members

		SchemaColumn[] IDispositionCodeColumnsForFastSearchProvider.GetColumnsForFastSearch()
		{
			return new SchemaColumn[] { USDispositionDataAddInfoSchema.US_Code };
		}

		#endregion

		#region Sailing

		internal bool HasSailingLinkage
		{
			get { return Sailing != null; }
		}

		public void ImportBillsOfLadingLinkedToTheSameSailing(BillImportActionCollection importActions)
		{
			var sourceBills = new List<BillOfLading>(BillsOfLading);
			using (((ISingleElementListInternal)this).SuspendListChanged())
			{
				foreach (BillImportAction importAction in importActions)
				{
					if (importAction.Action == ImportAction.Replace && !importAction.IsSelected)
					{
						sourceBills.Remove(((ISailingSynchronisationTarget<BillOfLading>)importAction.Bill).Source);
					}
					else if (importAction.Action == ImportAction.Delete && importAction.IsSelected)
					{
						this.Bills.Delete(importAction.Bill);
					}
				}
			}

			this.Bills.Synchronise(sourceBills, false);
			RefreshSailingStatistics();
		}

		public bool CanChangeSailing
		{
			get { return MovementHeader.Messages.Count == 0; }
		}

		public void ChangeSailing(ZGuid sailingPK)
		{
			if (Sailings.Contains(BH_ParentID))
			{
				Sailings.Remove(BH_ParentID);
				RemoveSynchroniser();
			}
			BH_ParentID = sailingPK;
			if (!sailingPK.IsEmpty)
			{
				BH_ParentTableCode = JobSailingSchema.Constants.Prefix;
				var sailing = Factory.Load<JobSailing>(sailingPK);
				if (sailing != null)
				{
					Sailings.Add(sailing);
					SynchroniseIfNeeded();
				}
			}
			else
			{
				BH_ParentTableCode = ZString.Empty;
			}
			UpdateATDAfterSailingIsChanged();
			RefreshSailingStatistics();
			fBillsOfLading = null;
		}

		void UpdateATDAfterSailingIsChanged()
		{
			BH_SailingDate = Sailing != null ? Sailing.JX_JA_E_DEP : ZDateTime.Empty;
		}

		public void UpdateATDAfterSendingMessage(ActionCode actionCode, MessageSendingObjectCollection sendingObjectCollection)
		{
			if (actionCode == ActionCode.VesselDeparture && sendingObjectCollection.Count == 1)
			{
				BH_SailingDate = sendingObjectCollection[0].MB_Date;
			}
		}

		public void RefreshSailingStatistics()
		{
			foreach (var bill in Bills)
			{
				bill.RefreshSailingStatistics();
			}
			fBH_NoOfSailingContainers = null;
			BH_NoOfSailingBillsInfo.RefreshBinding();
			BH_NoOfSailingContainersInfo.RefreshBinding();
		}

		public JobSailing Sailing
		{
			get
			{
				JobSailing result = null;
				if (Sailings.Count == 1)
				{
					result = Sailings[0];
				}
				else if (BH_ParentTableCode == JobSailingSchema.Constants.Prefix)
				{
					result = Factory.Load<JobSailing>(BH_ParentID);
				}
				return result;
			}
		}

		public JobSailingCollection Sailings
		{
			get
			{
				if (fSailings == null)
				{
					fSailings = new JobSailingCollection(Factory);
					if (BH_ParentTableCode == JobSailingSchema.Constants.Prefix && !BH_ParentID.IsEmpty)
					{
						fSailings.AddFromDatabase(BH_ParentID);
					}
				}
				return fSailings;
			}
		}
		public JobSailingCollection fSailings;

		internal ZDecimal ConvertToUSD(ZDecimal amount, ZString currencyCode)
		{
			if (currencyCode.IsEmpty)
			{
				return amount;
			}

			var currency = GetCurrency(currencyCode);
			var usd = GetCurrency(Core.Constants.CurrencyCodes.UnitedStates);
			return currency != null ? CurrencyConverter.ConvertExact(new Money(amount, currency), usd).Amount.Round(0) : amount;
		}

		RefCurrency GetCurrency(ZString code)
		{
			RefCurrency result = null;
			if (currencyList == null)
			{
				currencyList = new Dictionary<ZString, RefCurrency>();
			}
			if (!currencyList.TryGetValue(code, out result) || result == null)
			{
				result = RefCurrency.LoadFromCurrencyCode(Factory, code);
				if (result != null)
				{
					currencyList.Add(code, result);
				}
			}
			return result;
		}
		Dictionary<ZString, RefCurrency> currencyList;

		CurrencyConverter CurrencyConverter
		{
			get { return fCurrencyConverter ?? (fCurrencyConverter = CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, 0)); }
		}
		CurrencyConverter fCurrencyConverter;

		#endregion

		#region ISailingParentFindBox Members

		ZString ISailingParentFindBox.Destination
		{
			get { return ZString.Empty; }
		}

		ZString ISailingParentFindBox.DischargePort
		{
			get { return BH_RL_NKPortUnlading; }
		}

		ZString ISailingParentFindBox.LoadPort
		{
			get { return ZString.Empty; }
		}

		ZString ISailingParentFindBox.Origin
		{
			get { return ZString.Empty; }
		}

		ZGuid ISailingParentFindBox.SailingPK
		{
			get { return BH_ParentTableCode == JobSailingSchema.Constants.Prefix ? BH_ParentID : ZGuid.Empty; }
			set { ChangeSailing(value); }
		}

		ZString ISailingParentFindBox.TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		#endregion

		#region IManifestHeaderForSynchroniser - allows CusInBoundHeader to be the target of synchronisation from ForwardingConsol.

		IManifestBillForSynchroniser IManifestHeaderForSynchroniser.AddNewBill()
		{
			return Bills.AddNew();
		}

		IEnumerable<IManifestBillForSynchroniser> IManifestHeaderForSynchroniser.Bills
		{
			get { return Bills; }
		}

		bool IManifestHeaderForSynchroniser.OverrideFreightDefaults
		{
			get { return BH_OverrideFreightDefaults; }
		}

		Integration.Forwarding.IForwardingConsol IManifestHeaderForSynchroniser.Consol => Consol;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new CusInBondHeaderDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new CusInBondHeaderDocManagerInfo(this)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IWorkflowProvider

		protected override bool SupportsWorkflowCore
		{
			get { return true; }
		}

		protected override ProcessTaskCollection GetNewCusInBondHeaderProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>(this);
		}

		#endregion

		#region IWorkflowProviderCore Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.USAMSWorkflowDescriptorCode; }
		}

		[ChildEditable]
		public ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader> WorkflowItems
		{
			get { return (ProcessTaskCollection<CusInBondHeaderProcessTask, CusInBondHeader>)((IWorkflowProvider)this).WorkflowItems; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GB, BH_GB, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, BH_RL_NKPortOfLoading, BH_RL_NKPortOfLoading.Substring(0, 2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, BH_RL_NKPortUnlading, BH_RL_NKPortUnlading.Substring(0, 2), ZString.Empty);

			return result;
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var parent = Consol as IWorkflowProviderCore;
				if (parent != null)
				{
					list.Add(parent);
				}
				return list;
			}
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return BH_JobReference; }
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USDisposition, ObjectFactory.GetType<Integration.Customs.US.IDispositionData>());
			return result;
		}

		#endregion

		#region ICustomsManifestMessageSupporter Members

		IProcessor ICustomsManifestMessageSupporter.CreateManifestMessageProcessor()
		{
			return new AutoSendUSAMSMessageProcessor(this);
		}

		IProcessor IBaseAutoSendingMessageSupporter.CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode)
		{
			return new CustomsStmProcessQueueCreatorProcessor(this, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, triggerActionCode);
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter Members

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction) => this;

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging => true;

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (humanReadableNameCoreCached == null)
				{
					humanReadableNameCoreCached = new CachedProperty<ZString>(Factory, delegate
					{
						return "AMS " + BH_JobReference;
					});
				}
				return humanReadableNameCoreCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableNameCoreCached;

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = string.Format(CultureInfo.CurrentCulture, "AMS - {0}", BH_JobReference);
				var numberofTransaction = string.Empty;
				switch (BH_TransitDirection)
				{
					case DirectionTypeList.Codes.NVOCC:
						if (Bills.DistinctBy(x => x.B0_MasterBillNumber).Count() == 1 && !Bills[0].B0_MasterBillNumber.IsEmpty)
						{
							numberofTransaction = " - " + Bills[0].B0_MasterBillNumber;
						}
						break;
					case DirectionTypeList.Codes.MVOCC:
						if (!BH_ImportConveyanceName.IsEmpty || !BH_VoyageNumber.IsEmpty)
						{
							numberofTransaction += " - " + BH_ImportConveyanceName;
							if (!numberofTransaction.Equals(" - ") && !BH_VoyageNumber.IsEmpty)
							{
								numberofTransaction += " & ";
							}
							numberofTransaction += BH_VoyageNumber;
						}
						break;
				}

				return Res.GetString("FE702A3D-F226-4F15-AC6D-903495FD4A12", "{0}{1}", result, numberofTransaction);
			}
		}
	}
}
