using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaBill : AsycudaBill,
		Integration.Customs.ASYCUDA.USExportManifest.IAsycudaBill,
		IAdditionalBusinessObjectFetchStrategyProvider
	{
		public USExportAsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = $"{USExportAsycudaManifestHeader.US_Sea_Exp} {Header.AMA_JobReference}";
				if (!ABL_BillNumber.IsEmpty)
				{
					result += $" - {ABL_BillIssuer}{ABL_BillNumber}";
				}
				return result;
			}
		}

		AsycudaArrivalLine ArrivalLine
		{
			get
			{
				if (!IsDeleting && !IsDeleted && (arrivalLine == null || arrivalLine.IsDeleted))
				{
					arrivalLine = GetArrivalLine();
					if (Header != null && arrivalLine == null)
					{
						arrivalLine = Header.ArrivalHeader.ArrivalDetails.AddNew();
						arrivalLine.ATL_ABL_AsycudaBill = PK;
					}
					RegisterEditableChildObject(arrivalLine);
				}
				return arrivalLine;
			}
		}
		AsycudaArrivalLine arrivalLine;

		public ZInt ATL_Quantity
		{
			get => GetArrivalLine()?.ATL_Quantity ?? ZInt.Zero;
			set
			{
				if (ATL_Quantity != value && ArrivalLine != null)
				{
					if (value > 0)
					{
						ArrivalLine.ATL_Quantity = value;
					}
					else
					{
						ArrivalLine.ATL_Quantity = ZInt.Zero;
					}
				}
			}
		}

		public ZDecimal ATL_Weight
		{
			get => GetArrivalLine()?.ATL_Weight ?? ZDecimal.Zero;
			set
			{
				if (ATL_Weight != value && ArrivalLine != null)
				{
					if (value > 0)
					{
						ArrivalLine.ATL_Weight = value;
					}
					else
					{
						ArrivalLine.ATL_Weight = ZDecimal.Zero;
					}
				}
			}
		}

		[MaxLength(2)]
		public ZString ATL_WeightUQ
		{
			get => GetArrivalLine()?.ATL_WeightUQ ?? ZString.Empty;
			set
			{
				if (ATL_WeightUQ != value && ArrivalLine != null)
				{
					ArrivalLine.ATL_WeightUQ = value;
				}
			}
		}

		public override ZString ABL_BillIssuer
		{
			get => base.ABL_BillIssuer;
			set
			{
				var oldValue = base.ABL_BillIssuer;
				if (oldValue != value)
				{
					fABL_BillIssuerOldValue = oldValue;
					base.ABL_BillIssuer = value;
					if (!IsValidationSuspended)
					{
						if (IsChildMasterBill && Header is USExportAsycudaManifestHeader header && header.MasterBill.PK.Equals(PK))
						{
							header.Validation.ValidateMasterBOL();
							header.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public ZString ABL_BillIssuerOldValue => fABL_BillIssuerOldValue;
		ZString fABL_BillIssuerOldValue;

		[ResourceStringData("FE7363EC-13E9-4A19-BBED-5086962A8B9A", Caption = "Place of Receipt")]
		public override ZString ABL_LocationInformation
		{
			get => base.ABL_LocationInformation;
			set => base.ABL_LocationInformation = value;
		}

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaBillLookups.BillOfLadingType))]
		[ResourceStringData("6542986F-B70A-4F4A-9E18-504E16D63123", Caption = "Bill of Lading Type")]
		[MaxLength(2)]
		public override ZString ABL_SpecialCargoCode
		{
			get => base.ABL_SpecialCargoCode;
			set => base.ABL_SpecialCargoCode = value;
		}

		[ResourceStringData("7C26AD9E-1F8A-42AA-AA41-D9DF6447D62F", Caption = "AES Exemption Code")]
		[MaxLength(3)]
		public override ZString ABL_UCRNumber
		{
			get => base.ABL_UCRNumber;
			set
			{
				base.ABL_UCRNumber = value;
				if (!base.IsValidationSuspended)
				{
					ValidateITNAndExemptionCodeAndInBondNumber();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaBillLookups.InlandTransportMode))]
		[MaxLength(2)]
		public override ZString ABL_InlandTransportMode
		{
			get => base.ABL_InlandTransportMode;
			set
			{
				if (base.ABL_InlandTransportMode != value)
				{
					base.ABL_InlandTransportMode = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaBillLookups.ManifestUQList))]
		public override ZString ABL_ManifestUQ
		{
			get => base.ABL_ManifestUQ;
			set => base.ABL_ManifestUQ = value;
		}

		public ZBool ABL_CustomsLoadPortIsDropEdit => PortOfLadingDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaBillLookups.CustomsLoadPortList))]
		public override ZString ABL_CustomsLoadPort
		{
			get => base.ABL_CustomsLoadPort;
			set => base.ABL_CustomsLoadPort = value;
		}

		public override ZString ABL_RL_NKPortOfLoading
		{
			get => base.ABL_RL_NKPortOfLoading;
			set
			{
				var hasChanges = base.ABL_RL_NKPortOfLoading != value;
				base.ABL_RL_NKPortOfLoading = value;
				if (!IsCopying && hasChanges)
				{
					PortOfLadingDefaulter.DefaultPort();
				}
			}
		}

		public ZBool ABL_CustomsDischargePortIsDropEdit => PortOfUnladingDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaBillLookups.CustomsPortOfUnladingList))]
		public override ZString ABL_CustomsDischargePort
		{
			get => base.ABL_CustomsDischargePort;
			set => base.ABL_CustomsDischargePort = value;
		}

		public override ZString ABL_RL_NKPortOfDischarge
		{
			get => base.ABL_RL_NKPortOfDischarge;
			set
			{
				var hasChanges = base.ABL_RL_NKPortOfDischarge != value;
				base.ABL_RL_NKPortOfDischarge = value;
				if (!IsCopying && hasChanges)
				{
					PortOfUnladingDefaulter.DefaultPort();
				}
			}
		}

		public ZBool ABL_CustomsOriginPortIsDropEdit => PortOfOriginDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaBillLookups.CustomsOriginPortList))]
		public override ZString ABL_CustomsOriginPort
		{
			get => base.ABL_CustomsOriginPort;
			set => base.ABL_CustomsOriginPort = value;
		}

		public override ZString ABL_RL_NKOrigin
		{
			get => base.ABL_RL_NKOrigin;
			set
			{
				var hasChanges = base.ABL_RL_NKOrigin != value;
				base.ABL_RL_NKOrigin = value;
				if (!IsCopying && hasChanges)
				{
					PortOfOriginDefaulter.DefaultPort();
				}
			}
		}

		public ZBool ABL_CustomsFinalDestinationPortIsDropEdit => PortOfFinalDestinationDefaulter.HasMultipleMappingPorts;

		[List(nameof(Lookups) + "." + nameof(USExportAsycudaBillLookups.CustomsFinalDestinationPortList))]
		public override ZString ABL_CustomsFinalDestinationPort
		{
			get => base.ABL_CustomsFinalDestinationPort;
			set => base.ABL_CustomsFinalDestinationPort = value;
		}

		public override ZString ABL_RL_NKFinalDestination
		{
			get => base.ABL_RL_NKFinalDestination;
			set
			{
				var hasChanges = base.ABL_RL_NKFinalDestination != value;
				base.ABL_RL_NKFinalDestination = value;
				if (!IsCopying && hasChanges)
				{
					PortOfFinalDestinationDefaulter.DefaultPort();
				}
			}
		}

		public ZString AESITNNumbers
		{
			get
			{
				if (fAESITNNumbers.IsEmpty)
				{
					fAESITNNumbers = AESITNNumberCollection.GetCodesAsCommaSeparatedString();
				}
				return fAESITNNumbers;
			}
			set
			{
				if (fAESITNNumbers != value)
				{
					fAESITNNumbers = value;
					AESITNNumberCollection.RefreshCollection(value);
					AESITNNumbersInfo.RefreshBinding();
					RefreshBinding();

					if (!base.IsValidationSuspended)
					{
						ValidateITNAndExemptionCodeAndInBondNumber();
					}
				}
			}
		}
		ZString fAESITNNumbers;

		public ZPropertyInfo AESITNNumbersInfo => GetZPropertyInfo(nameof(AESITNNumbers));

		public ZString InBondNumbers
		{
			get
			{
				if (fInBondNumbers.IsEmpty)
				{
					fInBondNumbers = InBondNumberCollection.GetCodesAsCommaSeparatedString();
				}
				return fInBondNumbers;
			}
			set
			{
				if (fInBondNumbers != value)
				{
					fInBondNumbers = value;
					InBondNumberCollection.RefreshCollection(value);
					InBondNumbersInfo.RefreshBinding();
					RefreshBinding();

					if (!base.IsValidationSuspended)
					{
						ValidateITNAndExemptionCodeAndInBondNumber();
					}
				}
			}
		}
		ZString fInBondNumbers;

		public ZPropertyInfo InBondNumbersInfo => GetZPropertyInfo(nameof(InBondNumbers));

		void ValidateITNAndExemptionCodeAndInBondNumber()
		{
			if (Validation is USExportAsycudaBillValidationForRegularBill validationForRegularBill)
			{
				validationForRegularBill.ValidateAESITNNumbers();
				validationForRegularBill.ValidateInBondNumbers();
				validationForRegularBill.ValidateABL_UCRNumber();
			}
			else if (Validation is USExportAsycudaBillValdiationForMasterChild validationForMasterChild)
			{
				validationForMasterChild.ValidateAESITNNumbers();
				validationForMasterChild.ValidateInBondNumbers();
				validationForMasterChild.ValidateABL_UCRNumber();
			}
		}

		UNLOCO_USPortsDefaulter PortOfLadingDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.D, ABL_RL_NKPortOfLoading, Header.AMA_TransportMode, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes);
				}

				return portOfLadingDefaulter ?? (portOfLadingDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, ABL_CustomsLoadPortInfo, ABL_RL_NKPortOfLoadingInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter portOfLadingDefaulter;

		public BusinessObjectCollection PortOfLadingRefLocoMappings => PortOfLadingDefaulter.MappingPorts;

		UNLOCO_USPortsDefaulter PortOfUnladingDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.K, ABL_RL_NKPortOfDischarge, ZString.Empty, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return portOfUnladingDefaulter ?? (portOfUnladingDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, ABL_CustomsDischargePortInfo, ABL_RL_NKPortOfDischargeInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter portOfUnladingDefaulter;

		public BusinessObjectCollection PortOfUnladingRefLocoMappings => PortOfUnladingDefaulter.MappingPorts;

		UNLOCO_USPortsDefaulter PortOfOriginDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.D, ABL_RL_NKOrigin, Header.AMA_TransportMode, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes);
				}

				return portOfOriginDefaulter ?? (portOfOriginDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, ABL_CustomsOriginPortInfo, ABL_RL_NKOriginInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter portOfOriginDefaulter;

		public BusinessObjectCollection PortOfOriginRefLocoMappings => PortOfOriginDefaulter.MappingPorts;

		UNLOCO_USPortsDefaulter PortOfFinalDestinationDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.K, ABL_RL_NKFinalDestination, ZString.Empty, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return portOfFinalDestinationDefaulter ?? (portOfFinalDestinationDefaulter =
					new UNLOCO_USPortsDefaulter(Factory, ABL_CustomsFinalDestinationPortInfo, ABL_RL_NKFinalDestinationInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter portOfFinalDestinationDefaulter;

		public BusinessObjectCollection PortOfFinalDestinationRefLocoMappings => PortOfFinalDestinationDefaulter.MappingPorts;

		AsycudaArrivalLine GetArrivalLine() => Factory.LoadTop1<AsycudaArrivalLine>(DataHelper.GenerateClusterKeyQuery(ABL_ClusterKey, PK, AsycudaArrivalLineSchema.ATL_ClusterKey, AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, !IsInDatabase));

		public new AsycudaPackCollection<USExportAsycudaPack, USExportAsycudaBill> Packs => (AsycudaPackCollection<USExportAsycudaPack, USExportAsycudaBill>)base.Packs;

		public new USExportAsycudaManifestHeader Header => (USExportAsycudaManifestHeader)base.Header;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection()
		{
			return new AsycudaPackCollection<USExportAsycudaPack, USExportAsycudaBill>(this);
		}

		[ChildEditable(true)]
		public USExportVisitedPortCollection VisitedPorts
		{
			get
			{
				if (visitedPorts == null)
				{
					visitedPorts = new USExportVisitedPortCollection(this);
					visitedPorts.Load();
					RegisterEditableChildObject(visitedPorts);
				}
				return visitedPorts;
			}
		}

		USExportVisitedPortCollection visitedPorts;

		[ChildEditable(true)]
		public CusEntryNumCollection AESITNNumberCollection
		{
			get
			{
				if (fAESITNNumberCollection == null)
				{
					fAESITNNumberCollection = new CusEntryNumCollection(this, CusEntryNumberTypes.UnitedStates.ITN, AESITNHumanReadableName);
					fAESITNNumberCollection.Load();
					RegisterEditableChildObject(fAESITNNumberCollection);
				}
				return fAESITNNumberCollection;
			}
		}
		CusEntryNumCollection fAESITNNumberCollection;

		[ChildEditable(true)]
		public CusEntryNumCollection InBondNumberCollection
		{
			get
			{
				if (fInBondNumberCollection == null)
				{
					fInBondNumberCollection = new CusEntryNumCollection(this, CusEntryNumberTypes.UnitedStates.InBond, InBondHumanReadableName);
					fInBondNumberCollection.Load();
					RegisterEditableChildObject(fInBondNumberCollection);
				}
				return fInBondNumberCollection;
			}
		}
		CusEntryNumCollection fInBondNumberCollection;

		internal static string AESITNHumanReadableName
		{
			get { return Res.GetString("4A21591E-7D2D-410D-9299-8694FA327AC3", "AES ITN"); }
		}

		internal static string InBondHumanReadableName
		{
			get { return Res.GetString("18E41EBA-6C85-48A5-B96C-A89630C59A12", "In-Bond Number"); }
		}

		protected override Type GetPackTypeCore() => typeof(USExportAsycudaPack);

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(USExportCusCodeType.Codes.UVP, typeof(USExportVisitedPort));
				return result;
			}
		}

		public new USExportAsycudaBillLookups Lookups => (USExportAsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new USExportAsycudaBillLookups(this);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				this.DeleteChildren<CusEntryNumber>(CusEntryNumSchema.CE_ParentID);
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_SpecialCargoCode = BillOfLadingTypeList.Codes.RegularBillOfLading;
		}

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild()
		{
			return new USExportAsycudaBillValdiationForMasterChild(this);
		}

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill()
		{
			return new USExportAsycudaBillValidationForRegularBill(this);
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}
	}
}
