using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.LicenseManager;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public interface IInvoiceLineProvider
	{
		JobComInvoiceLine InvoiceLine { get; }
	}

	public enum USLinePriceCalculationFieldSettingType
	{
		AIILineInvQty,
		AIILineItemAmount,
		AIILineUnitPrice,
		AIILine98InvCurrPerUnit,
		AIILineUnitBasis,
		AIILinePercActvIngr,
		_98ValueInvCurr,
		_98InvCurrPerUnit
	}

	[SystemDefinedValues]
	[UniversalCopyAddInfo]
	[UniversalCopyIgnoreElement("CusAddInfos")]
	[CodeProperty("Description")]
	[GlowDataDefinition("IUSJobComInvoiceLine")]
	public partial class JobComInvoiceLine : AutoJobComInvoiceLine
		, Integration.Customs.US.IJobComInvoiceLine
		, IHazardousMaterial
		, IHaveAdditionalDataForBorderWise
		, IUltimateDistributee
		, IDutyData
		, IFeeCalculationDataProvider
		, IChargeApportionee
		, IReconOriginalChargeParent
		, ICustomsBrokerDetails
		, ICusAddInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, IInvoiceLineProvider
		, ITSCAData
		, IAESAMS
		, IAESEPA
		, ICusDispositionParent
		, IInvoiceLineDutyDataProvider
		, IADDCVDLiability
		, IInvoiceLine
		, IPGADataChangeTrackerSupporter
		, IImportWrappedPropertySupporter
		, IEffectiveValueManagerSupporter
		, IDocAddresses
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			previousPivotPK = IsInDatabase ? US_CI_PreviousPivot : ZGuid.Empty;
			GetConvertedStockUnit = x => GetConvertedUnit(x);
		}

		#region Schema

		public new partial class Schema : AutoJobComInvoiceLine.Schema
		{
			public const string JI_Calc_EntryNumber = "JI_Calc_EntryNumber";
			public const string JI_Calc_AllocatedQty = "JI_Calc_AllocatedQty";
			public const string JI_Calc_AllocatedQtyBalance = "JI_Calc_AllocatedQtyBalance";
			public const string JI_Calc_BondedWhsQuantity = "JI_Calc_BondedWhsQuantity";
			public const string JI_Calc_XTN = "JI_Calc_XTN";
			public const string JI_OA_FDAShipperAddress = "JI_OA_FDAShipperAddress";
			public const string US_DOTRequirementDesc = "US_DOTRequirementDesc";
			public const string US_FCCRequirementDesc = "US_FCCRequirementDesc";
			public const string US_RX_ADDDepositValueCurrency = "US_RX_ADDDepositValueCurrency";
			public const string US_RX_CVDDepositValueCurrency = "US_RX_CVDDepositValueCurrency";
			public const string EntryNumberAndMergeLineNumber = "EntryNumberAndMergeLineNumber";
			public const string ManufacturerNameAndID = "ManufacturerNameAndID";
			public const string OriginalTariffFormatted = "OriginalTariffFormatted";
			public const string OriginalSupTariffFormatted = "OriginalSupTariffFormatted";
			public const string SupTariffFormatted = "SupTariffFormatted";
			public const string US_CH_ReconEntry = "US_CH_ReconEntry";
			public const string IsCottonFeeExempt = "IsCottonFeeExempt";
			public const string CalcMiscLicenseTypeLabel = "CalcMiscLicenseTypeLabel";
			public const string US_R_OrigOtherFeeCode = "US_R_OrigOtherFeeCode";
			public const string US_R_OrigOtherFeeAmount = "US_R_OrigOtherFeeAmount";
			public const string US_R_OrigTaxAmount = "US_R_OrigTaxAmount";
			public const string US_R_OrigHMFAmount = "US_R_OrigHMFAmount";
			public const string US_R_OrigMPFAmount = "US_R_OrigMPFAmount";
			public const string US_R_ReconMPFAmount = "US_R_ReconMPFAmount";
			public const string US_R_ReconHMFAmount = "US_R_ReconHMFAmount";
			public const string US_R_ReconOtherFeeCode = "US_R_ReconOtherFeeCode";
			public const string US_R_ReconOtherFeeAmount = "US_R_ReconOtherFeeAmount";
			public const string US_R_OverrideOriginMPF = "US_R_OverrideOriginMPF";
			public const string US_R_OverrideReconMPF = "US_R_OverrideReconMPF";
			public const string US_R_OverrideOriginHMF = "US_R_OverrideOriginHMF";
			public const string US_R_OverrideReconHMF = "US_R_OverrideReconHMF";
			public const string US_R_OverrideOrigOtherFeeAmount = "US_R_OverrideOrigOtherFeeAmount";
			public const string US_R_OverrideReconOtherFeeAmount = "US_R_OverrideReconOtherFeeAmount";
			public const string US_FSISReqDesc = "US_FSISReqDesc";
			public const string US_ODSReqDesc = "US_ODSReqDesc";
			public const string US_VNEReqDesc = "US_VNEReqDesc";
			public const string BondedWhsQuantityForGUI = "BondedWhsQuantityForGUI";
			public const string US_PSTReqDesc = "US_PSTReqDesc";
			public const string US_LaceyRequirementDesc = "US_LaceyRequirementDesc";
			public const string US_NHTSAReqDesc = "US_NHTSAReqDesc";
			public const string FTZCurrentTariffFormatted = "FTZCurrentTariffFormatted";
			public const string ManufacturerMID = "ManufacturerMID";

			public const string ForeignExporterOrgPK = "ForeignExporterOrgPK";
			public const string US_DDTCTrackingStatusDesc = "US_DDTCTrackingStatusDesc";
			public const string US_ODSTrackingStatusDesc = "US_ODSTrackingStatusDesc";
			public const string US_TSCATrackingStatusDesc = "US_TSCATrackingStatusDesc";
			public const string US_ADDDepositRateDescription = "US_ADDDepositRateDescription";
			public const string US_CVDDepositRateDescription = "US_CVDDepositRateDescription";

			public const string US_FirstPermitLicenseType = "US_FirstPermitLicenseType";
			public const string US_FirstPermitLicenseNumber = "US_FirstPermitLicenseNumber";
			public const string US_SupAdditionalTariff1Duty = "US_SupAdditionalTariff1Duty";
			public const string US_OverrideSupAdditionalTariff1Duty = "US_OverrideSupAdditionalTariff1Duty";
			public const string US_SupAdditionalTariff2Duty = "US_SupAdditionalTariff2Duty";
			public const string US_OverrideSupAdditionalTariff2Duty = "US_OverrideSupAdditionalTariff2Duty";
			public const string US_SupAdditionalTariff3Duty = "US_SupAdditionalTariff3Duty";
			public const string US_OverrideSupAdditionalTariff3Duty = "US_OverrideSupAdditionalTariff3Duty";
			public const string US_SupAdditionalTariff4Duty = "US_SupAdditionalTariff4Duty";
			public const string US_OverrideSupAdditionalTariff4Duty = "US_OverrideSupAdditionalTariff4Duty";
			public const string US_SupAdditionalTariff5Duty = "US_SupAdditionalTariff5Duty";
			public const string US_OverrideSupAdditionalTariff5Duty = "US_OverrideSupAdditionalTariff5Duty";
			public const string US_SupAdditionalTariff1GoodsValue = "US_SupAdditionalTariff1GoodsValue";
			public const string US_SupAdditionalTariff2GoodsValue = "US_SupAdditionalTariff2GoodsValue";
			public const string US_SupAdditionalTariff3GoodsValue = "US_SupAdditionalTariff3GoodsValue";
			public const string US_SupAdditionalTariff4GoodsValue = "US_SupAdditionalTariff4GoodsValue";
			public const string US_SupAdditionalTariff5GoodsValue = "US_SupAdditionalTariff5GoodsValue";
		}

		#endregion

		#region AddInfo Effective properties

		#region US_DDTCTrackingStatus
		[ReadOnly(true)]
		public override ZString US_DDTCTrackingStatus
		{
			get { return base.US_DDTCTrackingStatus; }
			set { base.US_DDTCTrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_DDTCTrackingStatusDesc", Caption = "DDTC Status")]
		public ZString US_DDTCTrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_DDTCTrackingStatus); }
		}

		public ZPropertyInfo US_DDTCTrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCTrackingStatusDesc); }
		}
		#endregion

		#region US_ODSTrackingStatus
		[ReadOnly(true)]
		public override ZString US_ODSTrackingStatus
		{
			get { return base.US_ODSTrackingStatus; }
			set { base.US_ODSTrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_ODSTrackingStatusDesc", Caption = "ODS Status")]
		public ZString US_ODSTrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_ODSTrackingStatus); }
		}

		public ZPropertyInfo US_ODSTrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ODSTrackingStatusDesc); }
		}
		#endregion

		#region US_TSCATrackingStatus
		[ReadOnly(true)]
		public override ZString US_TSCATrackingStatus
		{
			get { return base.US_TSCATrackingStatus; }
			set { base.US_TSCATrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_TSCATrackingStatusDesc", Caption = "TSCA Status")]
		public ZString US_TSCATrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TSCATrackingStatus); }
		}

		public ZPropertyInfo US_TSCATrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TSCATrackingStatusDesc); }
		}
		#endregion

		#region US_ZoneStatus

		public override ZString US_ZoneStatus
		{
			get { return GetEffectiveValueToReturn(base.US_ZoneStatus, Schema.US_ZoneStatus, JobComInvoiceHeader.Schema.US_ZoneStatus); }
			set
			{
				var hasChanged = US_ZoneStatus != value;
				if (hasChanged)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_ZoneStatus = GetEffectiveValueToSet(value, Schema.US_ZoneStatus, JobComInvoiceHeader.Schema.US_ZoneStatus);
				if (US_ZoneStatus != ZoneStatusList.Codes.PrivilegedForeign)
				{
					US_PrivilegedStatusDate = ZDateTime.Empty;
					US_FTZCurrentTariff = ZString.Empty;
				}

				if (hasChanged && !IsCopying)
				{
					RefreshInvoiceLinesWithSpecificColumnsChanged();
					RecalculateSupTariffsWhenCriteriaChanges();
				}
			}
		}

		public bool US_ZoneStatus_ReadOnly
		{
			get { return IsSecondaryTariffLine; }
		}

		#endregion

		#region US_PrivilegedStatusDate

		public bool US_PrivilegedStatusDate_ReadOnly
		{
			get { return IsSecondaryTariffLine; }
		}

		public override ZDateTime US_PrivilegedStatusDate
		{
			get { return GetEffectiveValueToReturn(base.US_PrivilegedStatusDate, Schema.US_PrivilegedStatusDate, JobComInvoiceHeader.Schema.US_PrivilegedStatusDate); }
			set
			{
				var hasChanged = US_PrivilegedStatusDate != value;
				base.US_PrivilegedStatusDate = GetEffectiveValueToSet(value, Schema.US_PrivilegedStatusDate, JobComInvoiceHeader.Schema.US_PrivilegedStatusDate);
				if (hasChanged)
				{
					RecalculateSupTariffsWhenCriteriaChanges();
				}
			}
		}

		#endregion

		internal void RecalculateSupTariffsWhenCriteriaChanges()
		{
			ResetSupTariffs();
			SetDefaultSupTariffs();
		}

		public override ZString US_AESOriginIndicator
		{
			get
			{
				if (US_ExportCode == ExportInformationCodeList.Codes.HH)
				{
					return base.US_AESOriginIndicator;
				}
				else
				{
					return GetEffectiveValueToReturn(base.US_AESOriginIndicator, Schema.US_AESOriginIndicator, JobComInvoiceHeader.Schema.US_AESOriginIndicator);
				}
			}
			set
			{
				if (US_ExportCode == ExportInformationCodeList.Codes.HH)
				{
					base.US_AESOriginIndicator = value;
				}
				else
				{
					base.US_AESOriginIndicator = GetEffectiveValueToSet(value, Schema.US_AESOriginIndicator, JobComInvoiceHeader.Schema.US_AESOriginIndicator);
				}
			}
		}

		public override ZDateTime US_DateOfExport
		{
			get { return GetEffectiveValueToReturn(base.US_DateOfExport, "", JobComInvoiceHeader.Schema.US_DateOfExport); }
			set { base.US_DateOfExport = GetEffectiveValueToSet(value, "", JobComInvoiceHeader.Schema.US_DateOfExport); }
		}

		#region US_ECCN

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.US_ECCNList))]
		public override ZString US_ECCN
		{
			get { return GetEffectiveValueToReturn(base.US_ECCN, Schema.US_ECCN, JobComInvoiceHeader.Schema.US_ECCN); }
			set { base.US_ECCN = GetEffectiveValueToSet(value, Schema.US_ECCN, JobComInvoiceHeader.Schema.US_ECCN); }
		}

		public ZBool ECCNCodeFindBoxVisible => HasAvailableECCNNumbers;

		public ZBool ECCNTextBoxVisible => !HasAvailableECCNNumbers;

		public ZBool HasAvailableECCNNumbers => !US_LicenseType.IsEmpty && LicenseValidationHelper.HasAvailableECCNNumber(Factory, US_LicenseType);

		#endregion

		#region US_ExportCode
		public override ZString US_ExportCode
		{
			get { return GetEffectiveValueToReturn(base.US_ExportCode, Schema.US_ExportCode, JobComInvoiceHeader.Schema.US_ExportCode); }
			set { base.US_ExportCode = GetEffectiveValueToSet(value, Schema.US_ExportCode, JobComInvoiceHeader.Schema.US_ExportCode); }
		}
		#endregion

		#region US_LicenseNo
		[ReadOnlyMember(nameof(US_LicenseNo_ReadOnly))]
		public override ZString US_LicenseNo
		{
			get
			{
				var result = base.US_LicenseNo;
				if (!LicenseAndLicenseExemptionTypeManager.IsRequiredSpaceCode(US_LicenseType, Factory, ExportDateForLicenseType))
				{
					result = GetEffectiveValueToReturn(result, Schema.US_LicenseNo, JobComInvoiceHeader.Schema.US_LicenseNo);
				}
				return result;
			}
			set
			{
				if (LicenseAndLicenseExemptionTypeManager.IsRequiredSpaceCode(US_LicenseType, Factory, ExportDateForLicenseType))
				{
					base.US_LicenseNo = value;
				}
				else
				{
					base.US_LicenseNo = GetEffectiveValueToSet(value, Schema.US_LicenseNo, JobComInvoiceHeader.Schema.US_LicenseNo);
				}
			}
		}

		bool US_LicenseNo_ReadOnly
		{
			get
			{
				var result = false;
				if (IsExport)
				{
					var exportDate = ExportDateForLicenseType;
					if (exportDate.IsValid)
					{
						result = UniversalReferenceDataHelper.GetLicenseNoReadOnly_LicenseNoIsNotEmpty(Factory, US_LicenseType, exportDate, US_LicenseNo);
					}
				}
				return result;
			}
		}
		#endregion

		#region US_LicenseType
		public override ZString US_LicenseType
		{
			get
			{
				return IsFTZAdmission ? base.US_LicenseType : GetEffectiveValueToReturn(base.US_LicenseType, Schema.US_LicenseType, JobComInvoiceHeader.Schema.US_LicenseType);
			}
			set
			{
				if (IsFTZAdmission)
				{
					base.US_LicenseType = value;
				}
				else
				{
					var oldValue = this.US_LicenseType;
					var newValue = GetEffectiveValueToSet(value, Schema.US_LicenseType, JobComInvoiceHeader.Schema.US_LicenseType);
					var hasChanges = oldValue != newValue;
					base.US_LicenseType = newValue;
					if (hasChanges && !IsCopying)
					{
						var exportDate = ExportDateForLicenseType;
						if (exportDate.IsValid)
						{
							var licenseNo = UniversalReferenceDataHelper.GetLicenseNo(Factory, value, exportDate);
							if (!licenseNo.IsEmpty)
							{
								US_LicenseNo = licenseNo.SubstringSafe(0, AutoUSAddInfo.Schema.US_LicenseNoMaxLength);
							}
							else if (!US_LicenseNo.IsEmpty && LicenseAndLicenseExemptionTypeManager.IsRequiredSpaceCode(value, Factory, exportDate))
							{
								US_LicenseNo = ZString.Empty;
							}
						}
						if (InvoiceHeader != null)
						{
							new RegistrationNumberDefaulter().DefaultDDTCRegistrationNumber(US_DDTCRegistrationNoInfo, US_LicenseType, InvoiceHeader.US_USPPI);
						}
					}
				}
			}
		}
		#endregion

		public ZString CalcMiscLicenseTypeLabel
		{
			get
			{
				var result = ZString.Empty;
				if (IsFTZAdmission)
				{
					result = "License No.";
				}
				else
				{
					var importSupTariff = ImportSupTariff;
					var tariff = importSupTariff != null && !importSupTariff.UE_PermitLicenseIndicator.IsEmpty ? importSupTariff : ImportTariff;
					result = tariff == null ? DefaultMiscLicenceLabelValue : (string)tariff.MiscLicenseTypeLabel.TrimEnd(':');
				}
				return result;
			}
		}

		public const string DefaultMiscLicenceLabelValue = "Misc. License No.";

		public ZPropertyInfo CalcMiscLicenseTypeLabelInfo
		{
			get { return GetZPropertyInfo(Schema.CalcMiscLicenseTypeLabel); }
		}

		#region Entry Types

		public bool IsQuota
		{
			get { return Declaration?.IsQuota ?? false; }
		}

		public bool IsInformal
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsImport && EntryTypeList.IsInformal(declaration.US_EntryType);
			}
		}

		public bool IsTIBEntryType
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsImport && declaration.US_EntryType == EntryTypeList.Codes.TemporaryImportationBond;
			}
		}

		public bool IsDomesticCargo
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.US_EntryType == EntryTypeList.Codes.Warehouse && declaration.US_DomesticCargo;
			}
		}

		#endregion

		public bool IsACSForRecon
		{
			get
			{
				var entry = InvoiceHeader?.ReconOriginalEntry;
				if (entry != null)
				{
					return entry.US_R_MsgMode == JobApplicationCodeList.Codes.ACS;
				}

				return false;
			}
		}

		public bool IsValidForAII
		{
			get { return !US_IsExcludedFromAII; }
		}

		public bool IsNonLineGroupingOrOnlyOneAIILine
		{
			get { return !IsLineGroupingEnabled || TotalNoOfSequences == 1; }
		}

		public bool IsLineGroupingEnabled
		{
			get
			{
				JobComInvoiceHeader invoice = InvoiceHeader;
				return invoice != null && invoice.IsLineGroupingEnabled;
			}
		}

		public bool IsConsumptionForNAFTARecon
		{
			get { return IsImport && Declaration != null && Declaration.US_NAFTAReconIndicator && EntryTypeList.IsConsumptionForNAFTARecon(ImportEntryType); }
		}

		public override ZDateTime US_DateOfExportFromCountryOfOrigin
		{
			get { return GetEffectiveValueToReturn(base.US_DateOfExportFromCountryOfOrigin, Schema.US_DateOfExportFromCountryOfOrigin, JobComInvoiceHeader.Schema.US_DateOfExportFromCountryOfOrigin); }
			set { base.US_DateOfExportFromCountryOfOrigin = GetEffectiveValueToSet(value, Schema.US_DateOfExportFromCountryOfOrigin, JobComInvoiceHeader.Schema.US_DateOfExportFromCountryOfOrigin); }
		}

		public override ZString US_UC_NKCountryOfOrigin
		{
			get
			{
				return !base.US_UC_NKCountryOfOrigin.IsEmpty ?
					base.US_UC_NKCountryOfOrigin :
					IsSecondaryTariffLine ? ParentTariffLine.US_UC_NKCountryOfOrigin : InvoiceHeader?.US_UC_NKCountryOfOrigin ?? string.Empty;
			}
			set
			{
				var oldValue = US_UC_NKCountryOfOrigin;
				if (oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
					ResetSupTariffs();
				}

				base.US_UC_NKCountryOfOrigin = InvoiceHeader == null || InvoiceHeader.US_UC_NKCountryOfOrigin != value ? value : ZString.Empty;

				if (oldValue != US_UC_NKCountryOfOrigin)
				{
					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
					}

					if (fOGARequirementCalculator != null)
					{
						fOGARequirementCalculator.Initialise();
					}

					RefreshInvoiceLinesWithSpecificColumnsChanged();
					RecalculateSupTariffsWhenCriteriaChanges();
					DefaultUS_ZoneStatusForFTZ();
					SetDefaultValueOrCleanSanctionsIfNeeded();
				}
			}
		}

		public override ZString US_UC_NKCountryOfExport
		{
			get
			{
				return !base.US_UC_NKCountryOfExport.IsEmpty ?
					base.US_UC_NKCountryOfExport :
					IsSecondaryTariffLine ? ParentTariffLine.US_UC_NKCountryOfExport : InvoiceHeader?.US_UC_NKCountryOfExport ?? string.Empty;
			}
			set
			{
				if (!IsCopying && US_UC_NKCountryOfExport != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_UC_NKCountryOfExport = InvoiceHeader == null || InvoiceHeader.US_UC_NKCountryOfExport != value ? value : ZString.Empty;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_DisclaimSanctions", Caption = "Disclaim Sanctions")]
		public override ZBool US_DisclaimSanctions
		{
			get { return base.US_DisclaimSanctions; }
			set
			{
				var oldValue = base.US_DisclaimSanctions;
				base.US_DisclaimSanctions = value;
				if (!IsCopying && oldValue != base.US_DisclaimSanctions)
				{
					if (TariffMatchesFishingCondition)
					{
						if (base.US_DisclaimSanctions)
						{
							FishingInformations.RemoveAndDeleteAll();
						}
						else
						{
							FishingInformations.Cast<FishingInformation>().ForEach(x => x.AddInfoValidation.ValidateFishingInformation());
						}
						FishingInformations.RefreshBinding();
					}
					else if (TariffMatchesMiningCondition)
					{
						if (base.US_DisclaimSanctions)
						{
							MiningInformations.RemoveAndDeleteAll();
						}
						else
						{
							MiningInformations.Cast<MiningInformation>().ForEach(x => x.Validation.ValidateCY_Data());
						}
						MiningInformations.RefreshBinding();
					}
				}
			}
		}

		public override ZString US_SchDLoading
		{
			get
			{
				return IsSecondaryTariffLine
					? ParentTariffLine.US_SchDLoading
					: base.US_SchDLoading.IsEmpty && Declaration != null ? Declaration.US_SchDLoading : base.US_SchDLoading;
			}
			set
			{
				base.US_SchDLoading = Declaration == null || Declaration.US_SchDLoading != value ? value : ZString.Empty;

				if (Declaration != null)
				{
					Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public bool US_SchDLoading_ReadOnly
		{
			get { return IsSecondaryTariffLine; }
		}

		#region SPI

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.SPIList))]
		public override ZString US_SPI
		{
			get
			{
				ZString result = base.US_SPI;

				//in-lieu tariff overrides whatever duty rate is for secondary.
				if (result.IsEmpty)
				{
					IDutyData parentLine = ((IDutyData)this).ParentTariffLine;

					if (parentLine == null || parentLine.ImportTariff == null || !parentLine.ImportTariff.Applies(TariffRuleList.Codes.InLieuTariffs, EffectiveDateForDutyRate))
					{
						result = GetEffectiveValueToReturn(result, JobComInvoiceLine.Schema.US_SPI, "");
					}
				}

				return result;
			}
			set
			{
				ZString valueAssigned = value;
				ZString valueToSet = GetEffectiveValueToSet(valueAssigned, JobComInvoiceLine.Schema.US_SPI, "");

				//if users have set a value which is the same as its parent line SPI, then what users have assigned should be written to AddInfo
				//if InLieu tariff is used because no fallback is allowed for InLieu Tariffs.

				IDutyData parentLine = ((IDutyData)this).ParentTariffLine;

				if (valueToSet.IsEmpty && !valueAssigned.IsEmpty &&
					parentLine != null &&
					(parentLine.SpecialProgramsIndicatorPrimary == valueAssigned || parentLine.SpecialProgramsIndicatorCountry == valueAssigned) &&
					parentLine.ImportTariff != null &&
					parentLine.ImportTariff.Applies(TariffRuleList.Codes.InLieuTariffs, EffectiveDateForDutyRate))
				{
					valueToSet = valueAssigned;
				}

				base.US_SPI = valueToSet;

				if (!IsCopying && IsParentLine)
				{
					ClearSPIInChildLine();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.ProductClaimList))]
		public override ZString US_SecondarySPI
		{
			get { return base.US_SecondarySPI; }
			set
			{
				var originalvalue = US_SecondarySPI;
				base.US_SecondarySPI = value;

				if (!IsCopying && originalvalue != US_SecondarySPI)
				{
					if (IsSetXLine && !Declaration.IsRecon)
					{
						JI_LinePrice = ZDecimal.Zero;
					}

					DefalutCBMAReleatedFields();

					SetHeaderComponentParentCalculator.UpdateWhenSetIndicatorChanges();
					AIILines.MarkAsNeedingValidation();
					InvoiceHeader?.InvalidateJZ_Calc_LinesEnteredCache();
				}
			}
		}

		void DefalutCBMAReleatedFields()
		{
			if (IsCBMAProductClaim)
			{
				DefaultCBMARelatedFieldsFromImporter();
				DefaultTaxRateSForCBMAIfPossible();
			}
		}

		void DefaultTaxRateSForCBMAIfPossible()
		{
			if (IsCBMA23Effective)
			{
				var cbmaList = AddInfoLookups.CBMATaxRateList;
				US_TTBRateDesignationCode = cbmaList.Count == 1 ? cbmaList[0].Code : string.Empty;
			}
			else
			{
				if (!settingUS_TaxRateSInProgress)
				{
					var list = AddInfoLookups.TaxRateList;
					US_TaxRateS = list.Count == 1 ? list[0].Code : string.Empty;
				}
			}
		}

		void ClearSPIInChildLine()
		{
			foreach (var childLine in ChildLines)
			{
				if (this.IsCombinedLine() || childLine.US_SPI == US_SPI)
				{
					childLine.US_SPI = ZString.Empty;
					childLine.US_SPIInfo.RefreshBinding();
				}
			}
		}

		#endregion

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.EPANetQtyUQList))]
		public override ZString US_EPANetQtyUQ
		{
			get { return base.US_EPANetQtyUQ; }
			set { base.US_EPANetQtyUQ = value; }
		}

		public override ZString US_DestinationState
		{
			get { return GetEffectiveValueToReturn(base.US_DestinationState, Schema.US_DestinationState, JobComInvoiceHeader.Schema.US_DestinationState); }
			set { base.US_DestinationState = GetEffectiveValueToSet(value, Schema.US_DestinationState, JobComInvoiceHeader.Schema.US_DestinationState); }
		}

		public override ZString US_TransactionsRelated
		{
			get
			{
				ZString result;
				if (IsSecondaryTariffLine)
				{
					result = ParentTariffLine.US_TransactionsRelated;
				}
				else
				{
					result = GetEffectiveValueToReturn(base.US_TransactionsRelated, Schema.US_TransactionsRelated, JobComInvoiceHeader.Schema.US_TransactionsRelated);
				}
				return result;
			}
			set
			{
				if (!Declaration?.IsReconMessageType ?? true)
				{
					base.US_TransactionsRelated = GetEffectiveValueToSet(value, Schema.US_TransactionsRelated, JobComInvoiceHeader.Schema.US_TransactionsRelated);
				}
			}
		}

		[ReadOnlyMember(nameof(ManufacturerOrgPK_ReadOnly))]
		public override ZGuid JI_OA_ManufacturerAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JI_OA_ManufacturerAddress,
														Schema.JI_OA_ManufacturerAddress,
														JobComInvoiceHeader.Schema.JZ_OA_ManufacturerAddress,
														Schema.ManufacturerOrgPK,
														disableSettingManufacturerDefaults,
														(addressOrgPK) => SetJI_OA_ManufacturerAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK));
			}

			set
			{
				var oldValue = base.JI_OA_ManufacturerAddress;
				var valueChanged = oldValue != value;
				if (!IsCopying && valueChanged)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}

				// need to compare with actual value before setting and always validate.
				var newValue = GetEffectiveValueToSet(value, Schema.JI_OA_ManufacturerAddress, JobComInvoiceHeader.Schema.JZ_OA_ManufacturerAddress);
				if (oldValue != newValue)
				{
					base.JI_OA_ManufacturerAddress = newValue;
				}
				else
				{
					Validation.ValidateJI_OA_ManufacturerAddress();
				}

				InvoiceHeader?.MarkAsNeedingValidation();

				if (!IsCopying)
				{
					RefreshFDAManufacturerAddress();

					if (JI_OA_ManufacturerAddress.IsValid)
					{
						DefaultCountryOfOriginFromOrganisationDetails(ManufacturerAddress);
					}

					if (valueChanged)
					{
						MarkReconIndicatorsDirtyIfNecessary();
					}
				}
			}
		}

		protected void DefaultCountryOfOriginFromOrganisationDetails(OrgAddress address)
		{
			if (!IsCopying && !IsSecondaryTariffLine && DataRegistry.Business.USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty))
			{
				new CountryOfOriginDefaulter().Default(US_UC_NKCountryOfOriginInfo, address);
			}
		}

		void DefaultCBMARelatedFieldsFromImporterWhenCBMA23IsNotEffective()
		{
			if (US_ControlledGroupName.IsEmpty && AddInfoLookups.ControlledGroupNames.Count == 1)
			{
				US_ControlledGroupName = AddInfoLookups.ControlledGroupNames[0].Code;
			}

			var identifiersCount = AddInfoLookups.ForeignProducerIdentifiers.Count;
			if (US_FPI.IsEmpty && identifiersCount > 0)
			{
				if (identifiersCount == 1)
				{
					US_FPI = AddInfoLookups.ForeignProducerIdentifiers[0].Code;
				}
				else if (identifiersCount > 1)
				{
					var foreignProducerIdentifierPrefix = ZString.Empty;
					switch (US_TaxCode)
					{
						case Core.Constants.USCustoms.FeeCodes.DistilledSpirits:
							foreignProducerIdentifierPrefix = "S";
							break;
						case Core.Constants.USCustoms.FeeCodes.Wines:
							foreignProducerIdentifierPrefix = "W";
							break;
						case Core.Constants.USCustoms.FeeCodes.OtherExcise:
							foreignProducerIdentifierPrefix = "B";
							break;
					}

					if (!foreignProducerIdentifierPrefix.IsEmpty)
					{
						if (Declaration?.ImporterWrapper is OrgHeaderWrapper importerWrapper)
						{
							var matchedForeignProducerIdentifiers = new List<ZString>();
							foreach (AllocationQuantityPerFPI allocationQty in importerWrapper.AllocationQuantityPerFPIs)
							{
								var foreignProducerIdentifier = allocationQty.US_ForeignProducerIdentifier.Left(AutoUSAddInfo.Schema.US_FPIMaxLength);
								if (foreignProducerIdentifier.Left(1) == foreignProducerIdentifierPrefix && allocationQty.US_OA_ManufacturerAddress == JI_OA_ManufacturerAddress)
								{
									matchedForeignProducerIdentifiers.Add(foreignProducerIdentifier);
								}

								if (matchedForeignProducerIdentifiers.Count > 2)
								{
									break;
								}
							}

							if (matchedForeignProducerIdentifiers.Count == 1)
							{
								US_FPI = matchedForeignProducerIdentifiers[0];
							}
						}
					}
				}
			}
		}

		void DefaultCBMARelatedFieldsFromImporterWhenCBMA23IsEffective()
		{
			var identifiers = AddInfoLookups.ForeignProducerIdentifiers;
			if (US_FPI.IsEmpty && identifiers.Count == 1)
			{
				US_FPI = identifiers[0].Code;
			}
		}

		void DefaultCBMARelatedFieldsFromImporter()
		{
			if (IsCBMA23Effective)
			{
				DefaultCBMARelatedFieldsFromImporterWhenCBMA23IsEffective();
			}
			else
			{
				DefaultCBMARelatedFieldsFromImporterWhenCBMA23IsNotEffective();
			}
		}

		public OrgHeader Manufacturer
		{
			get
			{
				return ManufacturerAddress?.Header;
			}
		}

		void MarkReconIndicatorsDirtyIfNecessary()
		{
			if (Declaration != null && IsImport && !IsFTZAdmission)
			{
				Declaration.MarkReconIndicatorsDirty();
			}
		}

		public void RefreshFDAManufacturerAddress()
		{
			foreach (FDA fda in FDAs)
			{
				fda.RefreshUS_FDAManufacturerAddress_ZAddress();
				fda.AddInfoValidation.ValidateUS_FDAManufacturerAddress();
			}

			foreach (ACEFDA ace_fda in ACE_FDALines)
			{
				ace_fda.RefreshUS_ManufacturerAddress_ZAddress();
				ace_fda.AddInfoValidation.ValidateUS_ManufacturerAddress();
			}
		}

		public IOrganisationDetails ManufacturerDetails => OrganisationDetails.New(JI_OA_ManufacturerAddressInfo, OrgMatchedCustomsRegNoType.MID) ?? InvoiceHeader?.ManufacturerDetails;

		public ZString ManufacturerFallBackToSupplierNumber
		{
			get
			{
				var result = ZString.Empty;
				IOrganisationDetails organisationDetails = ManufacturerDetails;
				if (organisationDetails == null)
				{
					var invoiceHeader = this.InvoiceHeader;
					organisationDetails = invoiceHeader == null ? null : OrganisationDetails.New(invoiceHeader.JZ_OA_SupplierAddressInfo, OrgMatchedCustomsRegNoType.MID);
				}

				if (organisationDetails != null)
				{
					result = organisationDetails.MatchedCustomsRegoNumber;
				}

				return result.ToUpper();
			}
		}

		public bool HasFDAData
		{
			get { return FDAs.Count > 0; }
		}

		bool ShouldDefaultFDAToBeDeclared
		{
			get
			{
				return (PGARequirementIndicator.RequireFDA && !IsACECargoCertificationMode || PGARequirementIndicator.RequireACEFDA && IsACEFDARelevant && !IsDomesticCargo);
			}
		}

		public bool HasFDAReportingRequirement
		{
			get { return PGARequirementIndicator.HasFDARequirement || PGARequirementIndicator.HasACEFDARequirement; }
		}

		internal bool HasPriorNoticeRequirements
		{
			get { return JI_FDARequirementCode == OGARequirementList.Codes.FD3 || JI_FDARequirementCode == OGARequirementList.Codes.FD4; }
		}

		internal bool HasFDAAdmissibilityReviewRequirement
		{
			get
			{
				return JI_FDARequirementCode == OGARequirementList.Codes.FD4;
			}
		}

		public bool RequiresPriorNoticeReporting()
		{
			return RequiresPriorNoticeReporting(ImportTariffForPGA) || RequiresPriorNoticeReporting(ImportSupTariff);
		}

		bool RequiresPriorNoticeReporting(USCTariff importTariff)
		{
			if (importTariff != null)
			{
				if (importTariff.FDAPriorNoticeAndAdmissibilityReviewRequired)
				{
					return true;
				}

				if (IsFDADeclared)
				{
					if (Declaration.CanHavePGAFDA)
					{
						if (ACE_FDALines.Cast<ACEFDA>().Any(x => x.IsPriorNotice))
						{
							return true;
						}
					}
					else
					{
						if (importTariff.FDAPriorNoticeAndAdmissibilityReviewMayBeRequired && FDAs.Cast<FDA>().Any(x => !x.US_PND))
						{
							return true;
						}

						if (FDAs.Cast<FDA>().Any(x => x.US_FDAForcePN))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public bool HasFCCData
		{
			get { return FCCs.Count > 0; }
		}

		public bool HasDOTData
		{
			get { return DOTs.Count > 0; }
		}

		public override ZString US_TariffType
		{
			get { return GetEffectiveValueToReturn(base.US_TariffType, "", JobComInvoiceHeader.Schema.US_TariffType); }
			set
			{
				bool hasChanges = US_TariffType != value;
				base.US_TariffType = GetEffectiveValueToSet(value, "", JobComInvoiceHeader.Schema.US_TariffType);
				if (IsExport && hasChanges && !IsCopying)
				{
					PartSyncManager?.Refresh();
					RefreshTariffDetails();
					DefaultExportOGAIndicatorsWhenTariffChanges();
				}
			}
		}

		public void RefreshTariffDetails()
		{
			if (IsExport)
			{
				var tariff = TariffExpirationDateWinin30Days;
				if (tariff != null && (tariff.ZZ1_ZZ8_UQ1 != JI_CustomsUnitQty || tariff.ZZ1_ZZ8_UQ2 != JI_CustomsSecondUnitQty || tariff.ZZ1_ZZ8_UQ3 != JI_CustomsThirdUnitQty))
				{
					UpdateTariffWhileRetainingQuantities(JI_TariffInfo, JI_CustomsQuantityInfo, JI_CustomsSecondQuantityInfo, JI_CustomsThirdQuantityInfo);
				}
			}
			else
			{
				var tariff = (ITariff)ImportTariffForPGA;//Should not use today's date to select a tariff. It should use the job's duty date.

				if (tariff != null)
				{
					if (tariff.Unit1 != JI_CustomsUnitQty || tariff.Unit2 != JI_CustomsSecondUnitQty || tariff.Unit3 != JI_CustomsThirdUnitQty)
					{
						UpdateTariffWhileRetainingQuantities(JI_TariffInfo, JI_CustomsQuantityInfo, JI_CustomsSecondQuantityInfo, JI_CustomsThirdQuantityInfo);
					}
				}
			}

			if (IsImport)
			{
				var supTariff = (ITariff)ImportSupTariff;
				if (supTariff != null)
				{
					if (supTariff.Unit1 != US_SupUQ1 || supTariff.Unit2 != US_SupUQ2 || supTariff.Unit3 != US_SupUQ3)
					{
						UpdateTariffWhileRetainingQuantities(US_SupTariffInfo, US_SupQty1Info, US_SupQty2Info, US_SupQty3Info);
					}
				}

				var supAdditionalTariff1 = (ITariff)ImportSupAdditionalTariff1;
				if (supAdditionalTariff1 != null)
				{
					if (supAdditionalTariff1.Unit1 != US_SupAdditionalTariff1UQ)
					{
						UpdateTariffWhileRetainingQuantities(SupFormattedAdditionalTariff1Info, US_SupAdditionalTariff1UQInfo, null, null);
					}
				}

				var supAdditionalTariff2 = (ITariff)ImportSupAdditionalTariff2;
				if (supAdditionalTariff2 != null)
				{
					if (supAdditionalTariff2.Unit1 != US_SupAdditionalTariff2UQ)
					{
						UpdateTariffWhileRetainingQuantities(SupFormattedAdditionalTariff2Info, US_SupAdditionalTariff2UQInfo, null, null);
					}
				}

				var supAdditionalTariff3 = (ITariff)ImportSupAdditionalTariff3;
				if (supAdditionalTariff3 != null)
				{
					if (supAdditionalTariff3.Unit1 != US_SupAdditionalTariff3UQ)
					{
						UpdateTariffWhileRetainingQuantities(SupFormattedAdditionalTariff3Info, US_SupAdditionalTariff3UQInfo, null, null);
					}
				}

				var supAdditionalTariff4 = (ITariff)ImportSupAdditionalTariff4;
				if (supAdditionalTariff4 != null)
				{
					if (supAdditionalTariff4.Unit1 != US_SupAdditionalTariff4UQ)
					{
						UpdateTariffWhileRetainingQuantities(SupFormattedAdditionalTariff4Info, US_SupAdditionalTariff4UQInfo, null, null);
					}
				}

				var supAdditionalTariff5 = (ITariff)ImportSupAdditionalTariff5;
				if (supAdditionalTariff5 != null)
				{
					if (supAdditionalTariff5.Unit1 != US_SupAdditionalTariff5UQ)
					{
						UpdateTariffWhileRetainingQuantities(SupFormattedAdditionalTariff5Info, US_SupAdditionalTariff5UQInfo, null, null);
					}
				}
			}

			var declaration = this.Declaration;
			if (declaration != null && declaration.IsRecon)
			{
				ITariff orgImpTariff = OriginalImportTariff;

				if (orgImpTariff != null)
				{
					if (orgImpTariff.Unit1 != US_R_OrigFirstUQ || orgImpTariff.Unit2 != US_R_OrigSecondUQ || orgImpTariff.Unit3 != US_R_OrigThirdUQ)
					{
						UpdateTariffWhileRetainingQuantities(US_R_OrigTariffInfo, US_R_OrigFirstQtyInfo, US_R_OrigSecondQtyInfo, US_R_OrigThirdQtyInfo);
					}
				}

				ITariff impSupTariff = ImportSupTariff;

				if (impSupTariff != null)
				{
					if (impSupTariff.Unit1 != US_SupUQ1 || impSupTariff.Unit2 != US_SupUQ2 || impSupTariff.Unit3 != US_SupUQ3)
					{
						UpdateTariffWhileRetainingQuantities(US_SupTariffInfo, US_SupQty1Info, US_SupQty2Info, US_SupQty3Info);
					}
				}

				ITariff orgImpSupTariff = OriginalImportSupTariff;

				if (orgImpSupTariff != null)
				{
					if (orgImpSupTariff.Unit1 != US_R_OrigSupUQ1 || orgImpSupTariff.Unit2 != US_R_OrigSupUQ2 || orgImpSupTariff.Unit3 != US_R_OrigSupUQ3)
					{
						UpdateTariffWhileRetainingQuantities(US_R_OrigSupTariffInfo, US_R_OrigSupQty1Info, US_R_OrigSupQty2Info, US_R_OrigSupQty3Info);
					}
				}
			}
		}

		void UpdateTariffWhileRetainingQuantities(ZPropertyInfo tariffFieldInfo, ZPropertyInfo qty1Info, ZPropertyInfo qty2Info, ZPropertyInfo qty3Info)
		{
			var originalTariffValue = tariffFieldInfo.Value;
			var originalCustomsQty = qty1Info.Value;
			var originalSecondQty = qty2Info?.Value ?? ZDecimal.Zero;
			var originalThirdQty = qty3Info?.Value ?? ZDecimal.Zero;

			tariffFieldInfo.Value = ZString.Empty;
			tariffFieldInfo.Value = originalTariffValue;

			if (!qty1Info.ReadOnly)
			{
				qty1Info.Value = originalCustomsQty;
			}

			if (qty2Info != null && !qty2Info.ReadOnly)
			{
				qty2Info.Value = originalSecondQty;
			}

			if (qty3Info != null && !qty3Info.ReadOnly)
			{
				qty3Info.Value = originalThirdQty;
			}
		}

		public ZString JI_Calc_EntryNumber
		{
			get
			{
				CusEntryLine cusEntryLine = this.CusEntryLine;
				return (cusEntryLine == null) ? ZString.Empty : cusEntryLine.CL_Calc_EntryNumber;
			}
		}

		public override ZString JI_Calc_MergedLineNumber
		{
			get { return CusEntryLine == null ? notMerged : CusEntryLine.CL_LineNumberFormatted; }
		}

		public ZPropertyInfo JI_Calc_EntryNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_EntryNumber); }
		}

		public ZString JI_Calc_XTN
		{
			get
			{
				CusEntryLine cusEntryLine = this.CusEntryLine;
				return (cusEntryLine == null) ? ZString.Empty : cusEntryLine.CL_Calc_XTN;
			}
		}

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInParentTariffLine, string fieldNameInJobComInvoiceHeader, string fieldNameInJobDeclaration = "") where T : IZType
		{
			return EffectiveValueManager.GetEffectiveValueToReturn(baseValue, fieldNameInParentTariffLine, () =>
			{
				var result = default(T);
				if (!string.IsNullOrEmpty(fieldNameInParentTariffLine) && IsSecondaryTariffLine)
				{
					result = (T)ParentTariffLine[fieldNameInParentTariffLine];
				}
				else if (!string.IsNullOrEmpty(fieldNameInJobComInvoiceHeader))
				{
					JobComInvoiceHeader invoice = InvoiceHeader;
					if (invoice != null)
					{
						result = (T)invoice[fieldNameInJobComInvoiceHeader];
					}
				}
				else if (!string.IsNullOrEmpty(fieldNameInJobDeclaration))
				{
					var declaration = Declaration;
					if (declaration != null)
					{
						result = (T)declaration[fieldNameInJobDeclaration];
					}
				}
				return result;
			});
		}

		ZGuid GetEffectiveAddressValueToReturn(ZGuid baseValue, string localAddressFieldName, string parentAddressFieldName, string addressOrgPKField, ZBool disableSettingDefaults, Action<ZGuid> seAddressOrgPKWithoutSettingDefaults, string addressOrgPKFieldInInvoice = "")
		{
			return EffectiveValueManager.GetEffectiveValueToReturn(baseValue, parentAddressFieldName, () =>
			{
				var result = ZGuid.Empty;
				if (IsChildLine)
				{
					result = (ZGuid)ParentTariffLine[localAddressFieldName];
				}
				else
				{
					var invoice = InvoiceHeader;
					if (invoice != null)
					{
						var addressOrgPK = (ZGuid)this[addressOrgPKField];
						var addressOrgPKInInvoice = ZGuid.Empty;
						if (!addressOrgPKFieldInInvoice.IsNullOrEmpty())
						{
							addressOrgPKInInvoice = (ZGuid)invoice[addressOrgPKFieldInInvoice];
						}
						else
						{
							addressOrgPKInInvoice = (ZGuid)invoice[addressOrgPKField];
						}
						if (addressOrgPK != ZGuid.Invalid && addressOrgPKInInvoice != ZGuid.Invalid)
						{
							result = (ZGuid)invoice[parentAddressFieldName];
							if (!disableSettingDefaults && addressOrgPK != addressOrgPKInInvoice)
							{
								seAddressOrgPKWithoutSettingDefaults(addressOrgPKInInvoice);
							}
						}
					}
				}
				return result;
			});
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInParentTariffLine, string fieldNameInJobComInvoiceHeader, string fieldNameInJobDeclaration = "") where T : IZType
		{
			T result = valuePassed;

			if (!string.IsNullOrEmpty(fieldNameInParentTariffLine) && IsSecondaryTariffLine)
			{
				if (result.Equals(ParentTariffLine[fieldNameInParentTariffLine]))
				{
					result = (T)valuePassed.Default;
				}
			}
			else if (!string.IsNullOrEmpty(fieldNameInJobComInvoiceHeader))
			{
				JobComInvoiceHeader invoice = InvoiceHeader;

				if (invoice != null && invoice[fieldNameInJobComInvoiceHeader].Equals(valuePassed))
				{
					result = (T)valuePassed.Default;
				}
			}
			else if (!string.IsNullOrEmpty(fieldNameInJobDeclaration))
			{
				var declaration = Declaration;
				if (declaration != null && declaration[fieldNameInJobDeclaration].Equals(valuePassed))
				{
					result = (T)valuePassed.Default;
				}
			}

			return result;
		}

		#endregion

		#region Overriden Properties

		[ReadOnlyMember(nameof(ManufacturerOrgPK_ReadOnly))]
		public override ZGuid ManufacturerOrgPK { get => base.ManufacturerOrgPK; set => base.ManufacturerOrgPK = value; }

		public bool ManufacturerOrgPK_ReadOnly => (!Manufacturer?.IsInDatabase) ?? false;
		public bool IsAMSEGGEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AMSEGG, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
		public bool IsAMSPNTEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AMSPNT, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
		public bool IsAMSNOPEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AMSNOP, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
		public bool IsCBMA23Effective => EffectiveDateForDutyRate >= new ZDateTime(2023, 01, 01);

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_OGAIndicatorList))]
		public override ZString US_TSCAInd
		{
			get { return base.US_TSCAInd; }
			set
			{
				var oldValue = US_TSCAInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, new IPGADataCorrection[] { TSCADataCorrection });
				}
				base.US_TSCAInd = value;
				if (!IsCopying && oldValue != US_TSCAInd)
				{
					ClearDisclaimReason(value, US_TSCADisclaimReasonInfo);
					SetDefaultValueOrClearTSCAODSDataIfNeeded();
					if (OGAIndicatorList.IsToBeDeclared(US_TSCAInd))
					{
						Validation.ValidateJI_Tariff();
					}
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_TSCACertification
		{
			get { return base.US_TSCACertification; }
			set
			{
				var oldValue = US_TSCACertification;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_TSCACertification = value;
			}
		}

		[ReadOnlyMember(nameof(US_TSCADisclaimReason_ReadOnly))]
		public override ZString US_TSCADisclaimReason
		{
			get { return base.US_TSCADisclaimReason; }
			set
			{
				var oldValue = US_TSCADisclaimReason;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_TSCADisclaimReason = value;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_TSCAODSCertIndividualList))]
		public override ZString US_TSCAODSCertIndividual
		{
			get { return base.US_TSCAODSCertIndividual; }
			set
			{
				var hasChanged = US_TSCAODSCertIndividual != value;
				base.US_TSCAODSCertIndividual = value;
				if (hasChanged && !IsCopying)
				{
					RefreshInvoiceLinesWithPGAIndicators();

					var invoice = InvoiceHeader;
					if (invoice != null)
					{
						if (value == PartyTypeList.Codes.Importer)
						{
							if (IORWrapper is IPGAContactDetails importerWrapper)
							{
								US_FDAContactName = importerWrapper.Name;
								US_FDAContactPhoneNo = importerWrapper.PhoneNumber.SubstringSafe(0, AutoUSAddInfo.Schema.US_FDAContactPhoneNoMaxLength).GetUSFormattPhoneNumber();
								US_FDAContactEmail = importerWrapper.EmailAddress;
							}
						}
						else if (value == PartyTypeList.Codes.CustomsBroker)
						{
							US_FDAContactName = ((ICustomsBrokerDetails)this).ContactName;
							US_FDAContactPhoneNo = ((ICustomsBrokerDetails)this).ContactPhone.GetUSFormattPhoneNumber();
							US_FDAContactEmail = ((ICustomsBrokerDetails)this).ContactEmail;
						}
					}
				}
			}
		}

		public override ZString US_FDAContactName
		{
			get { return base.US_FDAContactName; }
			set
			{
				var oldValue = US_FDAContactName;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactName = value;
			}
		}

		public override ZString US_FDAContactPhoneNo
		{
			get { return base.US_FDAContactPhoneNo; }
			set
			{
				var oldValue = US_FDAContactPhoneNo;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactPhoneNo = value;
			}
		}

		public override ZString US_FDAContactEmail
		{
			get { return base.US_FDAContactEmail; }
			set
			{
				var oldValue = US_FDAContactEmail;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDAContactEmail = value;
			}
		}

		void SetDefaultValueOrClearTSCAODSDataIfNeeded()
		{
			if (IsTSCAIndBeDeclared || IsODSIndBeDeclared)
			{
				if (US_TSCAODSCertIndividual.IsEmpty)
				{
					US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
				}
			}
			else if (US_TSCATrackingStatus.IsEmpty && US_ODSTrackingStatus.IsEmpty)
			{
				US_TSCAODSCertIndividual = ZString.Empty;
				US_TSCACertification = ZString.Empty;
			}
		}

		public bool IsTSCAIndBeDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_TSCAInd); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_OGAIndicatorList))]
		public override ZString US_AMSInd
		{
			get { return base.US_AMSInd; }
			set
			{
				var oldValue = US_AMSInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, AMSLines.Cast<IPGADataCorrection>());
				}

				var shouldUpdate = true;
				if (IsExport && oldValue != value && !OGAIndicatorList.IsToBeDeclared(value) && OnExportPGAIndicatorChangedEvent != null)
				{
					shouldUpdate = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.AMS, !US_ExportCertificateNo.IsEmpty);
				}

				if (shouldUpdate)
				{
					base.US_AMSInd = value;
					if (!IsCopying && oldValue != value)
					{
						if (value.IsEmpty)
						{
							US_AMSDisclaimProgram = ZString.Empty;
							US_AMSDisclaimReason = ZString.Empty;
						}
						else if (OGAIndicatorList.IsToBeDisclaimed(value))
						{
							var tariffs = new USCTariff[] { ImportTariffForPGA, ImportSupTariff };
							if (tariffs.Any(x => USAMSAddInfoValidation.IsTariffEligbleForAMS(x, TariffRuleList.Codes.HTSExemptFromAMSEG1ProgramRequirement, EffectiveDateForDutyRate)))
							{
								US_AMSDisclaimReason = PGADisclaimReasonList.Codes.A;
								US_AMSDisclaimProgram = AMSProgramList.Codes.EG1;
							}
							else
							{
								US_AMSDisclaimReason = PGADisclaimReasonList.Codes.B;
								if (AddInfoLookups.AMSDisclaimProgramList.Count == 1)
								{
									US_AMSDisclaimProgram = AddInfoLookups.AMSDisclaimProgramList[0].Code;
								}
								else
								{
									US_AMSDisclaimProgram = ZString.Empty;
								}
							}
						}
						else
						{
							ClearDisclaimReason(value, US_AMSDisclaimReasonInfo);
						}

						Declaration?.UpdatePGADataReplacementUpdateRequired();
						RefreshInvoiceLinesWithPGAIndicators();
						if (IsExport && !OGAIndicatorList.IsToBeDeclared(US_AMSInd))
						{
							US_ExportCertificateNo = ZString.Empty;
						}

						DefaultFDADateIfRequired();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(US_AMSDisclaimProgram_ReadOnly))]
		public override ZString US_AMSDisclaimProgram
		{
			get { return base.US_AMSDisclaimProgram; }
			set
			{
				var hasChanged = US_AMSDisclaimProgram != value;
				if (!IsCopying && hasChanged)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_AMSDisclaimProgram = value;
				if (hasChanged)
				{
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		[ReadOnlyMember(nameof(US_AMSDisclaimReason_ReadOnly))]
		public override ZString US_AMSDisclaimReason
		{
			get { return base.US_AMSDisclaimReason; }
			set
			{
				if (!IsCopying && US_AMSDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_AMSDisclaimReason = value;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_OGAIndicatorList))]
		public override ZString US_NOPInd
		{
			get { return base.US_NOPInd; }
			set
			{
				var oldValue = US_NOPInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, AMSLines.Cast<IPGADataCorrection>());
				}

				base.US_NOPInd = value;
				if (!IsCopying && oldValue != value)
				{
					if (value.IsEmpty)
					{
						US_NOPDisclaimReason = ZString.Empty;
					}
					else if (OGAIndicatorList.IsToBeDisclaimed(value))
					{
						US_NOPDisclaimReason = PGADisclaimReasonList.Codes.A;
					}
					else
					{
						ClearDisclaimReason(value, US_NOPDisclaimReasonInfo);
					}

					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
					DefaultFDADateIfRequired();
				}
			}
		}

		[ReadOnlyMember(nameof(US_NOPDisclaimReason_ReadOnly))]
		public override ZString US_NOPDisclaimReason
		{
			get { return base.US_NOPDisclaimReason; }
			set
			{
				if (!IsCopying && US_NOPDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_NOPDisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_PSTDisclaimProgram_ReadOnly))]
		public override ZString US_PSTDisclaimProgram
		{
			get { return base.US_PSTDisclaimProgram; }
			set
			{
				var hasChanged = US_PSTDisclaimProgram != value;
				if (!IsCopying && hasChanged)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_PSTDisclaimProgram = value;
				if (hasChanged)
				{
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		[ReadOnlyMember(nameof(US_PSTDisclaimReason_ReadOnly))]
		public override ZString US_PSTDisclaimReason
		{
			get { return base.US_PSTDisclaimReason; }
			set
			{
				if (!IsCopying && US_PSTDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_PSTDisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_HFCDisclaimReason_ReadOnly))]
		public override ZString US_HFCDisclaimReason
		{
			get { return base.US_HFCDisclaimReason; }
			set
			{
				if (!IsCopying && US_HFCDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_HFCDisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_VNEDisclaimReason_ReadOnly))]
		public override ZString US_VNEDisclaimReason
		{
			get { return base.US_VNEDisclaimReason; }
			set
			{
				if (!IsCopying && US_VNEDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_VNEDisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_FSISDisclaimReason_ReadOnly))]
		public override ZString US_FSISDisclaimReason
		{
			get { return base.US_FSISDisclaimReason; }
			set
			{
				if (!IsCopying && US_FSISDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FSISDisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_NMFS370DisclaimReason_ReadOnly))]
		public override ZString US_NMFS370DisclaimReason
		{
			get { return base.US_NMFS370DisclaimReason; }
			set
			{
				if (!IsCopying && US_NMFS370DisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_NMFS370DisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_NMFSAMRDisclaimReason_ReadOnly))]
		public override ZString US_NMFSAMRDisclaimReason
		{
			get { return base.US_NMFSAMRDisclaimReason; }
			set
			{
				if (!IsCopying && US_NMFSAMRDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_NMFSAMRDisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_NMFSHMSDisclaimReason_ReadOnly))]
		public override ZString US_NMFSHMSDisclaimReason
		{
			get { return base.US_NMFSHMSDisclaimReason; }
			set
			{
				if (!IsCopying && US_NMFSHMSDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_NMFSHMSDisclaimReason = value;
			}
		}

		[ReadOnlyMember(nameof(US_LaceyDisclaimReason_ReadOnly))]
		public override ZString US_LaceyDisclaimReason
		{
			get { return base.US_LaceyDisclaimReason; }
			set
			{
				if (!IsCopying && US_LaceyDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_LaceyDisclaimReason = value;
			}
		}

		public override bool IsContainerLinkMandatory
		{
			get { return false; }
		}

		public bool IsSetting98InvCurrPerUnit
		{
			get { return IsFieldSettingInProgress(USLinePriceCalculationFieldSettingType._98InvCurrPerUnit); }
		}

		public override ZDecimal US_98InvCurrPerUnit
		{
			get { return base.US_98InvCurrPerUnit; }
			set
			{
				if (IsSetting98InvCurrPerUnit)
				{
					ErrorReporter.ReportOnce("JobComInvoiceLine.US_98InvCurrPerUnit_Set", // Column names are in a string, which is okay
							"Called US_98InvCurrPerUnit from within itself" + System.Environment.NewLine +
							"Original Value = " + US_98InvCurrPerUnit.ToString(6) + " with PK = " + PK + System.Environment.NewLine +
							"New Value = " + value.ToString(6));
				}
				else
				{
					using (GetNewLinePriceCalculationFieldSettingSupporter(USLinePriceCalculationFieldSettingType._98InvCurrPerUnit))
					{
						ZDecimal oldValue = US_98InvCurrPerUnit;
						base.US_98InvCurrPerUnit = value;
						if (!IsCopying && oldValue != US_98InvCurrPerUnit)
						{
							UpdateUS_98ValueInvCurrIfChanged();
						}
					}
				}
			}
		}

		protected override ZString EffectiveCountryOfOriginCore
		{
			//US_UC_NKCountryOfOrigin getter takes care of fallback to invoice header
			get
			{
				return IsImport ? US_UC_NKCountryOfOrigin : base.EffectiveCountryOfOriginCore;
			}
		}

		public override ZPropertyInfo CountryOfOriginFieldInfo
		{
			get { return US_UC_NKCountryOfOriginInfo; }
		}

		protected override ZString CompleteEntryNumberCore
		{
			get
			{
				var result = base.CompleteEntryNumberCore;

				if (IsImport)
				{
					result = CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(Declaration.US_EntryFilerCode, result);
				}

				return result;
			}
		}

		public bool IsSetting98ValueInvCurr
		{
			get { return IsFieldSettingInProgress(USLinePriceCalculationFieldSettingType._98ValueInvCurr); }
		}

		public override ZDecimal US_98ValueInvCurr
		{
			get { return base.US_98ValueInvCurr; }
			set
			{
				if (IsSetting98ValueInvCurr)
				{
					ErrorReporter.ReportOnce("JobComInvoiceLine.US_98ValueInvCurr_Set", // Column names are in a string, which is okay
							"Called US_98ValueInvCurr from within itself" + System.Environment.NewLine +
							"Original Value = " + US_98ValueInvCurr.ToString(2) + " with PK = " + PK + System.Environment.NewLine +
							"New Value = " + value.ToString(2));
				}
				else
				{
					using (GetNewLinePriceCalculationFieldSettingSupporter(USLinePriceCalculationFieldSettingType._98ValueInvCurr))
					{
						ZDecimal oldValue = US_98ValueInvCurr;
						base.US_98ValueInvCurr = value;
						if (!IsCopying && oldValue != US_98ValueInvCurr)
						{
							if (InvoiceHeader != null && !InvoiceHeader.IsExport)
							{
								InvoiceHeader.ApportionLineWeight(this);
							}

							if (Declaration != null)
							{
								Declaration.MarkApportionmentDirty();
							}

							UpdateUS_98InvCurrPerUnitIfChanged();
							InvoiceHeader?.InvalidateJZ_Calc_LinesEnteredCache();
						}
					}
				}
			}
		}

		public override ZDecimal US_98GoodsValue
		{
			get { return base.US_98GoodsValue; }
			set
			{
				ZDecimal oldValue = US_98GoodsValue;
				base.US_98GoodsValue = value;
				if (!IsCopying && oldValue != US_98GoodsValue)
				{
					if (InvoiceHeader != null && !InvoiceHeader.IsExport)
					{
						InvoiceHeader.ApportionLineWeight(this);
					}

					if (Declaration != null)
					{
						Declaration.MarkApportionmentDirty();
					}
				}
			}
		}

		public override ZString US_FirstSale
		{
			get { return GetEffectiveValueToReturn(base.US_FirstSale, "", JobComInvoiceHeader.Schema.US_FirstSale); }
			set { base.US_FirstSale = GetEffectiveValueToSet(value, "", JobComInvoiceHeader.Schema.US_FirstSale); }
		}

		protected override bool US_98GoodsValue_ReadOnlyCore
		{
			get { return !IsUSReturnedGoodsTransaction && !IsUSOrigin; }
		}

		public bool IsUSReturnedGoodsTransaction
		{
			get
			{
				return JI_Tariff.StartsWith("9801") || CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(US_SupTariff) || CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(US_SupAdditionalTariff1);
			}
		}

		public override ZString JI_Description
		{
			get { return base.JI_Description; }
			set
			{
				ZString oldValue = JI_Description;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.JI_Description = value;
			}
		}

		public override ZString JI_ExtraInfoForClassification
		{
			get { return base.JI_ExtraInfoForClassification; }
			set
			{
				ZString oldValue = JI_ExtraInfoForClassification;
				base.JI_ExtraInfoForClassification = value;
			}
		}

		public override bool IsExtendedCommercialDescriptionEnabled
		{
			get { return true; }
		}

		public override ZDecimal US_AMMVPerUnit
		{
			get { return base.US_AMMVPerUnit; }
			set
			{
				var oldValue = US_AMMVPerUnit;
				base.US_AMMVPerUnit = value;
				if (!IsCopying && oldValue != US_AMMVPerUnit && IsImport)
				{
					if (US_AMMVPerUnit.IsEmpty || US_AMMVPercentage.IsEmpty)
					{
						CalculateAMMVCharge();
					}
					else
					{
						US_AMMVPercentage = ZDecimal.Zero;
					}
				}
			}
		}
		bool US_AMMVPerUnit_ReadOnly => !US_AMMVPercentage.IsEmpty && US_AMMVPerUnit.IsEmpty;

		public override ZDecimal US_AMMVPercentage
		{
			get { return base.US_AMMVPercentage; }
			set
			{
				var oldValue = base.US_AMMVPercentage;
				base.US_AMMVPercentage = value;
				if (!IsCopying && oldValue != US_AMMVPercentage && IsImport)
				{
					if (US_AMMVPerUnit.IsEmpty || US_AMMVPercentage.IsEmpty)
					{
						CalculateAMMVCharge();
					}
					else
					{
						US_AMMVPerUnit = ZDecimal.Zero;
					}
				}
			}
		}
		bool US_AMMVPercentage_ReadOnly => !US_AMMVPerUnit.IsEmpty && US_AMMVPercentage.IsEmpty;

		#region DDTC

		public override ZString US_DDTCInd
		{
			get { return base.US_DDTCInd; }
			set
			{
				var oldValue = US_DDTCInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, new IPGADataCorrection[] { DDTCDataCorrection });
				}
				base.US_DDTCInd = value;
				if (!IsCopying && oldValue != US_DDTCInd)
				{
					ClearACEDDTCDataIfNeeded();
					RefreshInvoiceLinesWithPGAIndicators();
					if (IsDDTCIndBeDeclared)
					{
						DefaultDDTCArrivalDate();
						new RegistrationNumberDefaulter().DefaultDDTCRegistrationNumber(US_DDTCRegistrationNoInfo, Importer);
					}
				}
			}
		}

		public ZString DDTCStatus
		{
			get { return DDTCDataCorrection.GetStatus(); }
		}

		public ZString DDTCStatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(DDTCStatus); }
		}

		public ZDateTime DDTCStatusDate
		{
			get { return DDTCDataCorrection.GetStatusDate(); }
		}

		public bool IsDDTCIndBeDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_DDTCInd); }
		}

		public override ZString US_DDTCITARExemptionNo
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCITARExemptionNo, Schema.US_DDTCITARExemptionNo, JobComInvoiceHeader.Schema.US_DDTCITARExemptionNo); }
			set { base.US_DDTCITARExemptionNo = GetEffectiveValueToSet(value, Schema.US_DDTCITARExemptionNo, JobComInvoiceHeader.Schema.US_DDTCITARExemptionNo); }
		}

		public override ZString US_DDTCMilitaryEquipmentIndicator
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCMilitaryEquipmentIndicator, Schema.US_DDTCMilitaryEquipmentIndicator, JobComInvoiceHeader.Schema.US_DDTCMilitaryEquipmentIndicator); }
			set { base.US_DDTCMilitaryEquipmentIndicator = GetEffectiveValueToSet(value, Schema.US_DDTCMilitaryEquipmentIndicator, JobComInvoiceHeader.Schema.US_DDTCMilitaryEquipmentIndicator); }
		}

		public override ZString US_DDTCPartyCertificationIndicator
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCPartyCertificationIndicator, Schema.US_DDTCPartyCertificationIndicator, JobComInvoiceHeader.Schema.US_DDTCPartyCertificationIndicator); }
			set { base.US_DDTCPartyCertificationIndicator = GetEffectiveValueToSet(value, Schema.US_DDTCPartyCertificationIndicator, JobComInvoiceHeader.Schema.US_DDTCPartyCertificationIndicator); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_DDTCRegistrationNo", Caption = "DDTC Registration No.", ShortCaption = "DDTC Reg. No.")]
		public override ZString US_DDTCRegistrationNo
		{
			get { return IsACE ? base.US_DDTCRegistrationNo : GetEffectiveValueToReturn(base.US_DDTCRegistrationNo, Schema.US_DDTCRegistrationNo, JobComInvoiceHeader.Schema.US_DDTCRegistrationNo); }
			set
			{
				var oldValue = US_DDTCRegistrationNo;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_DDTCRegistrationNo = IsACE ? value : GetEffectiveValueToSet(value, Schema.US_DDTCRegistrationNo, JobComInvoiceHeader.Schema.US_DDTCRegistrationNo);
			}
		}

		public override ZString US_DDTCUSMLCategoryCode
		{
			get { return GetEffectiveValueToReturn(base.US_DDTCUSMLCategoryCode, Schema.US_DDTCUSMLCategoryCode, JobComInvoiceHeader.Schema.US_DDTCUSMLCategoryCode); }
			set
			{
				base.US_DDTCUSMLCategoryCode = GetEffectiveValueToSet(value, Schema.US_DDTCUSMLCategoryCode, JobComInvoiceHeader.Schema.US_DDTCUSMLCategoryCode);
				if (US_JurisdictionNumber_ReadOnly)
				{
					US_JurisdictionNumber = ZString.Empty;
				}
			}
		}

		bool US_JurisdictionNumber_ReadOnly => US_DDTCUSMLCategoryCode != USMLCategoryCodes.Codes.MiscellaneousArticles;

		[ReadOnlyMember(nameof(US_JurisdictionNumber_ReadOnly))]
		public override ZString US_JurisdictionNumber
		{
			get
			{
				if (US_JurisdictionNumber_ReadOnly)
				{
					return base.US_JurisdictionNumber;
				}
				return GetEffectiveValueToReturn(base.US_JurisdictionNumber, Schema.US_JurisdictionNumber, JobComInvoiceHeader.Schema.US_JurisdictionNumber);
			}
			set { base.US_JurisdictionNumber = GetEffectiveValueToSet(value, Schema.US_JurisdictionNumber, JobComInvoiceHeader.Schema.US_JurisdictionNumber); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_DDTCLicenseNo", Caption = "DDTC License Number", ShortCaption = "DDTC Lic. No.")]
		public override ZString US_DDTCLicenseNo
		{
			get { return base.US_DDTCLicenseNo; }
			set
			{
				var oldValue = US_DDTCLicenseNo;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_DDTCLicenseNo = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_DDTCLicenseType", Caption = "DDTC License Type", ShortCaption = "DDTC Lic. Type")]
		public override ZString US_DDTCLicenseType
		{
			get { return base.US_DDTCLicenseType; }
			set
			{
				var oldValue = US_DDTCLicenseType;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_DDTCLicenseType = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_DDTCExemptionCode", Caption = "DDTC Exemption Number", ShortCaption = "DDTC Exempt. No.")]
		public override ZString US_DDTCExemptionCode
		{
			get { return base.US_DDTCExemptionCode; }
			set
			{
				var oldValue = US_DDTCExemptionCode;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_DDTCExemptionCode = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_DDTCArrivalDate", Caption = "DDTC Anticipated Arrival Date", MediumCaption = "Anticipated Arrival Date", ShortCaption = "Arrival Date")]
		public override ZDateTime US_DDTCArrivalDate
		{
			get { return base.US_DDTCArrivalDate; }
			set
			{
				var oldValue = US_DDTCArrivalDate;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_DDTCArrivalDate = value;
			}
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_AllocationQuantity", Caption = "Allocation Quantity")]
		public override ZDecimal US_AllocationQuantity
		{
			get => base.US_AllocationQuantity;
			set => base.US_AllocationQuantity = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_ControlledGroupName", Caption = "Controlled Group Name")]
		public override ZString US_ControlledGroupName
		{
			get => base.US_ControlledGroupName;
			set => base.US_ControlledGroupName = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_FPI", ShortCaption = "FPI", Caption = "Foreign Producer Identifier")]
		public override ZString US_FPI
		{
			get => base.US_FPI;
			set
			{
				var foreignProducerIdentifier = US_FPI;
				base.US_FPI = value;
				if (IsCBMAProductClaimAndIsNotCBMA23Effective && foreignProducerIdentifier != US_FPI && !IsCopying)
				{
					if (US_AllocationQuantity.IsEmpty && Declaration?.ImporterWrapper is OrgHeaderWrapper importerWrapper)
					{
						foreach (AllocationQuantityPerFPI allocationQty in importerWrapper.AllocationQuantityPerFPIs)
						{
							if (allocationQty.US_ForeignProducerIdentifier.EqualsIgnoringCase(US_FPI) && allocationQty.US_OA_ManufacturerAddress == JI_OA_ManufacturerAddress)
							{
								US_AllocationQuantity = allocationQty.US_AllocationQuantity;
								break;
							}
						}
					}
				}
			}
		}

		public ZBool FPIDropEditIsVisiable => IsCBMAProductClaimAndIsNotCBMA23Effective || (IsCBMAProductClaimAndIsCBMA23Effective && EntryTypeList.IsExWarehouseTypeOrFTZ(ImportEntryType));
		public ZBool FPITextBoxIsVisiable => IsCBMAProductClaimAndIsCBMA23Effective && !EntryTypeList.IsExWarehouseTypeOrFTZ(ImportEntryType);

		public ZString FPIFieldType
		{
			get
			{
				var result = nameof(FieldType.Text);
				if (!FPITextBoxIsVisiable)
				{
					result = nameof(FieldType.TextDropEdit);
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_FlavorContentCreditInd", Caption = "Flavor Content Credit Indicator")]
		public override ZBool US_FlavorContentCreditInd
		{
			get => base.US_FlavorContentCreditInd;
			set => base.US_FlavorContentCreditInd = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_TTBRateDesignationCode", ShortCaption = "CBMA Rate Desig.", Caption = "CBMA Rate Designation Code")]
		[List(nameof(AddInfoLookups) + "." + nameof(CBMATaxRateList))]
		public override ZString US_TTBRateDesignationCode
		{
			get => base.US_TTBRateDesignationCode;
			set
			{
				base.US_TTBRateDesignationCode = value;

				if (AddInfoLookups.CBMATaxRateList[US_TTBRateDesignationCode] is CBMATaxRate taxRate)
				{
					US_CBMADefaultTaxRate = taxRate.Rate;
				}
				else
				{
					US_CBMADefaultTaxRate = ZDecimal.Zero;
				}

				US_TTBRateDesignationCodeInfo.RefreshBinding();
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_CBMADefaultTaxRate", Caption = "CBMA Rate")]
		[DecimalPlaces(8)]
		public override ZDecimal US_CBMADefaultTaxRate
		{
			get => base.US_CBMADefaultTaxRate;
			set => base.US_CBMADefaultTaxRate = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_Prim_NA", ShortCaption = "Prim. Ctry/Rgn. N/A", Caption = "Primary Country/Region N/A")]
		public override ZBool US_Prim_NA
		{
			get => base.US_Prim_NA;
			set => base.US_Prim_NA = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_RN_NKPrimCtry", ShortCaption = "Prim. Ctry/Rgn.", Caption = "Primary Country/Region")]
		public override ZString US_RN_NKPrimCtry
		{
			get => base.US_RN_NKPrimCtry;
			set => base.US_RN_NKPrimCtry = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_Sec_NA", ShortCaption = "Sec. Ctry/Rgn. N/A", Caption = "Secondary Country/Region N/A")]
		public override ZBool US_Sec_NA
		{
			get => base.US_Sec_NA;
			set => base.US_Sec_NA = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_RN_NKSecCtry", ShortCaption = "Sec. Ctry/Rgn.", Caption = "Secondary Country/Region")]
		public override ZString US_RN_NKSecCtry
		{
			get => base.US_RN_NKSecCtry;
			set => base.US_RN_NKSecCtry = value;
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_RN_NKCastCtry", ShortCaption = "Cast Ctry/Rgn.", Caption = "Country/Region of Cast")]
		public override ZString US_RN_NKCastCtry
		{
			get => base.US_RN_NKCastCtry;
			set => base.US_RN_NKCastCtry = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.CountryList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_RN_NKCertOrigin", ShortCaption = "Cert. Orig.", Caption = "Certificate Of Origin")]
		public override ZString US_RN_NKCertOrigin
		{
			get => base.US_RN_NKCertOrigin;
			set => base.US_RN_NKCertOrigin = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.CountryList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_RN_NKMeltCtry", ShortCaption = "Melt. Ctry/Rgn.", Caption = "Melted/Poured Country/Region", FullDescription = "If the code 'ZZ' is entered in Melted/Poured Country/Region field, then 'OTH' will be sent in the Steel Melt and Pour Country Detail section of the entry summary message.")]
		public override ZString US_RN_NKMeltCtry
		{
			get => base.US_RN_NKMeltCtry;
			set => base.US_RN_NKMeltCtry = value;
		}

		public override ZString JI_PartNo
		{
			get
			{
				if (jI_PartNoCached == null)
				{
					jI_PartNoCached = new CachedProperty<ZString>(Factory, ImportHelper.GetPartNo);
				}
				return jI_PartNoCached.Value;
			}
			set
			{
				if (JI_PartNo_CanBeSetByCustomer)
				{
					bool hasChanges = base.JI_PartNo != value;

					if (hasChanges && !IsCopying)
					{
						DeleteProductLines();
						DeleteChildLines(JI_Tariff.IsEmpty);
						previousPivotPK = ZGuid.Invalid;
						base.JI_PartNo = value;

						var declaration = Declaration;
						if (declaration != null)
						{
							if (declaration.IsElectronicInvoice)
							{
								US_ArticleNoA = JI_Calc_SupplierPartNo;

								if (US_ArticleNoA.IsEmpty)
								{
									US_ArticleNoA = JI_Calc_OwnerPartNo;
								}
								else if (US_ArticleNoB.IsEmpty)
								{
									US_ArticleNoB = JI_Calc_OwnerPartNo;
								}
							}

							if (declaration.IsFormalImport)
							{
								declaration.MarkReconIndicatorsDirty();
							}
						}

						US_FlavorContentCreditInd = Pivot?.CD_FlavorContentCreditIndicator ?? false;
					}
				}
			}
		}
		CachedProperty<ZString> jI_PartNoCached;

		protected override bool JI_PartNo_CanBeSetByCustomerCore
		{
			get { return JI_ParentID.IsEmpty && ProductParentTariffLine == null; }
		}

		public override ZString JI_PartAttrib1
		{
			get
			{
				var result = ZString.Empty;
				var parentLine = ProductParentTariffLine ?? ParentTariffLine;
				if (parentLine != null)
				{
					result = parentLine.JI_PartAttrib1;
				}
				else
				{
					result = base.JI_PartAttrib1;
				}

				return result;
			}
			set
			{
				base.JI_PartAttrib1 = value;
			}
		}

		public override ZString JI_PartAttrib2
		{
			get
			{
				var result = ZString.Empty;
				var parentLine = ProductParentTariffLine ?? ParentTariffLine;
				if (parentLine != null)
				{
					result = parentLine.JI_PartAttrib2;
				}
				else
				{
					result = base.JI_PartAttrib2;
				}

				return result;
			}
			set
			{
				base.JI_PartAttrib2 = value;
			}
		}

		public override ZString JI_PartAttrib3
		{
			get
			{
				var result = ZString.Empty;
				var parentLine = ProductParentTariffLine ?? ParentTariffLine;
				if (parentLine != null)
				{
					result = parentLine.JI_PartAttrib3;
				}
				else
				{
					result = base.JI_PartAttrib3;
				}

				return result;
			}
			set
			{
				base.JI_PartAttrib3 = value;
			}
		}

		void DeleteChildLines(bool deleteChildLines)
		{
			if (deleteChildLines)
			{
				ChildLines.DeleteAll();
				RefreshChildLines();
			}
		}

		void DeleteProductLines()
		{
			ProductRelatedLines.DeleteAll();
			RefreshProductRelatedLines();
		}

		public override void UpdatePartSyncManagerAndRefresh(bool enabledStatus)
		{
			if (JI_PartNo_CanBeSetByCustomer)
			{
				base.UpdatePartSyncManagerAndRefresh(enabledStatus);
				if (!enabledStatus)
				{
					if (!JI_PartNo.IsEmpty)
					{
						DeleteProductLines();
						DeleteChildLines(true);
					}
					previousPivotPK = ZGuid.Invalid;
				}
			}
		}

		protected override void ASNRefereshDataCountrySpecific(IEnumerable<ZString> refreshOptions, BaseCusClassPartPivot pivot)
		{
			base.ASNRefereshDataCountrySpecific(refreshOptions, pivot);
			var usPivot = (CusClassPartPivot)pivot;
			if (refreshOptions.Contains(DefaultOptions.Codes.CountryOfOrigin))
			{
				US_UC_NKCountryOfOrigin = usPivot.CD_UC_NKCountryOfOrigin;
			}
			if (refreshOptions.Contains(DefaultOptions.Codes.Preference))
			{
				US_SPI = usPivot.CD_SPI;
			}
		}

		protected override bool CanCopyFromProduct => JI_PartNo_CanBeSetByCustomer && (IsDrawbackDeclaration || WasPivotChanged);

		public override void UpdateDetailsFromProductOnPartChangeCore()
		{
			DeleteProductLines();
			var declaration = Declaration;
			if (declaration != null && declaration.IsDrawback)
			{
				base.UpdateDetailsFromProductOnPartChangeCore();
				UpdateDrawbackDetailsOnPartChange();
			}
			else
			{
				base.UpdateDetailsFromProductOnPartChangeCore();
				var isImport = IsImport;
				if (isImport)
				{
					UpdateDetailsOnPartChangeForImport();
				}
				else if (IsExport)
				{
					UpdateAESDetailsOnPartChange();
				}

				if (isImport && declaration != null && !declaration.IsFTZAdmission)
				{
					declaration.MarkReconIndicatorsDirty();
				}
			}
		}

		void AddDetailsFromProducts(CusClassPartPivot pivot)
		{
			base.US_ADD_NA = pivot.CD_ADDApplicable;
			base.US_ADDDepositRateIndicator = pivot.CD_ADDDepositRateInd;
			base.US_ADDDepositRateOverride = pivot.CD_ADDDepositRateOverride;
			base.US_ADDCaseNo = pivot.CD_ADDCaseNo;
			base.US_ADCVDStat = pivot.CD_ADCVDStat;
			base.US_CVD_NA = pivot.CD_CVDApplicable;
			base.US_CVDDepositRateIndicator = pivot.CD_CVDDepositRateInd;
			base.US_CVDDepositRateOverride = pivot.CD_CVDDepositRateOverride;
			base.US_CVDCaseNo = pivot.CD_CVDCaseNo;
			base.US_TaxApply = pivot.CD_TaxApplicability;
			base.US_SecondarySPI = pivot.CD_ProductClaim;
			base.US_SPI = pivot.CD_SPI;
			base.US_CBTPACertificateNo = pivot.CD_CBTPACertificate;
			base.US_CottonCertificateNo = pivot.CD_CottonCertificate;
			base.US_CottonFeeExempt = pivot.CD_CottonFeeExempt;
			base.US_MiscPermitNo = pivot.CD_MiscLicenceNo.Left(base.US_MiscPermitNoInfo.MaxLength);
			base.US_PIRPRulingNo = pivot.CD_RulingNumber;
			base.US_PIRPRulingType = pivot.CD_RulingType;
			base.US_WoolLicenceNo = pivot.CD_WoolLicenceNo;
			base.US_IsBondedADD = pivot.CD_ADDBonded;
			base.US_IsBondedCVD = pivot.CD_CVDBonded;
			base.US_IsNAFTANet = pivot.CD_NAFTANetCost;
			base.US_CAExportCertificate = pivot.CD_SugarCertificate;
			base.US_AgricultureLicNo = pivot.CD_AgricultureLicenceNo;
			if (IsFTZAdmission || IsConsumptionFTZ)
			{
				base.US_ZoneStatus = pivot.CD_ZoneStatus;
			}
			base.US_TSCAIndicator = pivot.CD_TSCAIndicator;
			base.JI_OA_ManufacturerAddress = pivot.CD_OA_Manufacturer;
			base.JI_OA_ExporterAddress = pivot.CD_OA_Exporter;
			base.US_ProductExclusion = pivot.CD_ProductExclusion;
			base.US_ExclusionNumber = pivot.CD_ExclusionNumber;

			if (!pivot.CD_UC_NKCountryOfExport.IsEmpty)
			{
				base.US_UC_NKCountryOfExport = pivot.CD_UC_NKCountryOfExport;
			}
			base.US_TaxCode = pivot.CD_TaxCode;
			base.US_TaxRateT = pivot.CD_TaxRateType;

			if (IsCBMAProductClaimAndIsNotCBMA23Effective)
			{
				base.US_TaxRateS = pivot.CD_TTBRateDesignationCode;
				base.US_TaxRate = pivot.CD_CBMADefaultTaxRate;
			}
			else
			{
				base.US_TaxRateS = pivot.CD_TaxRateDesc;
				base.US_TaxRate = pivot.CD_TaxRate;
				if (IsCBMAProductClaimAndIsCBMA23Effective)
				{
					base.US_TTBRateDesignationCode = pivot.CD_TTBRateDesignationCode;
					base.US_CBMADefaultTaxRate = pivot.CD_CBMADefaultTaxRate;
				}
			}

			US_9802PerUnit = pivot.CD_9802USDValuePerUnit;
			US_98InvCurrPerUnit = pivot.CD_9802ValuePerUnit;
			base.US_WeightNET = pivot.CD_NetWeight;
			base.US_AESOriginIndicator = pivot.CD_OriginIndicator;
			base.US_ECCN = pivot.CD_ECCN;
			base.US_ExportCode = pivot.CD_ExportCode;
			base.US_LicenseType = pivot.CD_LicenceType;
			base.US_DDTCITARExemptionNo = pivot.CD_ITARExemptionNo;
			base.US_DDTCMilitaryEquipmentIndicator = pivot.CD_MilitaryEquipInd;
			base.US_DDTCRegistrationNo = pivot.CD_DDTCRegoNo;
			base.US_DDTCUSMLCategoryCode = pivot.CD_DDTCUSMLCategoryCode;
			base.US_DDTCUnit = pivot.CD_DDTCUnit;
			base.US_ADDDecID = pivot.CD_ADDDecID;
			base.US_GrossWeight = pivot.CD_GrossWeight;
			base.US_DDTCPartyCertificationIndicator = pivot.CD_PartyCertInd;
			base.US_LicenseNo = pivot.CD_MiscLicenceNo;
			US_AMMVPerUnit = pivot.CD_AMMVPerUnit;
			US_AMMVPercentage = pivot.CD_AMMVPercentage;
			US_Prim_NA = pivot.CD_PrimaryCountryNA;
			US_RN_NKPrimCtry = pivot.CD_RN_NKPrimaryCountry;
			US_Sec_NA = pivot.CD_SecondaryCountryNA;
			US_RN_NKSecCtry = pivot.CD_RN_NKSecondaryCountry;
			US_RN_NKCastCtry = pivot.CD_RN_NKCastCountry;
			US_RN_NKCertOrigin = pivot.CD_RN_NKCertificateOrigin;
			US_RN_NKMeltCtry = pivot.CD_RN_NKMeltCountry;

			if (IsCBMAProductClaim)
			{
				DefaultCBMARelatedFieldsFromImporter();
			}

			SupFormattedAdditionalTariff1 = pivot.SupFormattedAdditionalTariff1.Left(15);
			SupFormattedAdditionalTariff2 = pivot.SupFormattedAdditionalTariff2.Left(15);
			SupFormattedAdditionalTariff3 = pivot.SupFormattedAdditionalTariff3.Left(15);
			SupFormattedAdditionalTariff4 = pivot.SupFormattedAdditionalTariff4.Left(15);
			SupFormattedAdditionalTariff5 = pivot.SupFormattedAdditionalTariff5.Left(15);
		}

		void SetOriginCountryFromProducts(CusClassPartPivot pivot)
		{
			if (!pivot.CD_UC_NKCountryOfOrigin.IsEmpty)
			{
				base.US_UC_NKCountryOfOrigin = pivot.CD_UC_NKCountryOfOrigin;
			}
		}

		ZString SetDefaultCottonCertificateNo(CusClassPartPivot pivot)
		{
			var result = ZString.Empty;
			if (US_CottonCertificateNo.IsEmpty && pivot.CD_CottonCertificateApply)
			{
				var part = pivot.Part;
				var cottonDocNumber = GetCottonDocNumberByType(part);
				var declaration = Declaration;
				if (cottonDocNumber.IsEmpty && declaration != null)
				{
					cottonDocNumber = GetCottonDocNumberByType(declaration.Importer);
				}

				if (!cottonDocNumber.IsEmpty)
				{
					if (!IsACE)
					{
						US_CottonCertificateNo = cottonDocNumber;
					}
					else if (!LicenceAndPermits.Cast<LicenceAndPermit>().Any(x => x.CY_Code == LicencePermitTypeList.Codes._22 && x.CY_Data == cottonDocNumber))
					{
						result = cottonDocNumber;
					}
				}
			}
			return result;
		}

		ZString GetCottonDocNumberByType(IHaveRequiredDocuments documentsProvider)
		{
			var result = ZString.Empty;
			if (documentsProvider != null)
			{
				var cottonDocument = documentsProvider.RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault(x => x.EQ_DocType == Core.Constants.RefDocTypes.Cotton && x.EQ_ValidToDate >= EffectiveDateForDutyRate);
				if (cottonDocument != null)
				{
					result = cottonDocument.EQ_DocNumber.Left(base.US_CottonCertificateNoInfo.MaxLength);
				}
			}
			return result;
		}

		protected override ZString SecondTariff => SupTariffFormatted;

		protected override ZString SecondTariffDutyRate => GetMatchingSupEntryLine(US_SupTariff)?.US_DutyRateDesc ?? ZString.Empty;

		protected override ZString SecondTariffCustomsQty => US_SupQty1.IsDefault ? ZString.Empty : $"{US_SupQty1.ToString("0.00")} {US_SupUQ1}";

		protected override ZString ThirdTariff => SupFormattedAdditionalTariff1;

		protected override ZString ThirdTariffDutyRate => GetMatchingSupEntryLine(US_SupAdditionalTariff1)?.US_DutyRateDesc ?? ZString.Empty;

		protected override ZString ThirdTariffCustomsQty => US_SupAdditionalTariff1Qty.IsDefault ? ZString.Empty : $"{US_SupAdditionalTariff1Qty.ToString("0.00")} {US_SupAdditionalTariff1UQ}";

		protected override ZString FourthTariff => SupFormattedAdditionalTariff2;

		protected override ZString FourthTariffDutyRate => GetMatchingSupEntryLine(US_SupAdditionalTariff2)?.US_DutyRateDesc ?? ZString.Empty;

		protected override ZString FourthTariffCustomsQty => US_SupAdditionalTariff2Qty.IsDefault ? ZString.Empty : $"{US_SupAdditionalTariff2Qty.ToString("0.00")} {US_SupAdditionalTariff2UQ}";

		protected override ZString FifthTariff => SupFormattedAdditionalTariff3;

		protected override ZString FifthTariffDutyRate => GetMatchingSupEntryLine(US_SupAdditionalTariff3)?.US_DutyRateDesc ?? ZString.Empty;

		protected override ZString FifthTariffCustomsQty => US_SupAdditionalTariff3Qty.IsDefault ? ZString.Empty : $"{US_SupAdditionalTariff3Qty.ToString("0.00")} {US_SupAdditionalTariff3UQ}";

		protected override ZString SixthTariff => SupFormattedAdditionalTariff4;

		protected override ZString SixthTariffDutyRate => GetMatchingSupEntryLine(US_SupAdditionalTariff4)?.US_DutyRateDesc ?? ZString.Empty;

		protected override ZString SixthTariffCustomsQty => US_SupAdditionalTariff4Qty.IsDefault ? ZString.Empty : $"{US_SupAdditionalTariff4Qty.ToString("0.00")} {US_SupAdditionalTariff4UQ}";

		protected override ZString SeventhTariff => SupFormattedAdditionalTariff5;

		protected override ZString SeventhTariffDutyRate => GetMatchingSupEntryLine(US_SupAdditionalTariff5)?.US_DutyRateDesc ?? ZString.Empty;

		protected override ZString SeventhTariffCustomsQty => US_SupAdditionalTariff5Qty.IsDefault ? ZString.Empty : $"{US_SupAdditionalTariff5Qty.ToString("0.00")} {US_SupAdditionalTariff5UQ}";

		internal void AddExportAdditionalDetailsToProducts(CusClassPartPivot pivot)
		{
			// TODO: invoiceLine.US_PerUnitCost & invoiceLine.US_RX_NKPerUnitCostCurr does not exist. Maybe they are obsolete ? If so,
			// remove them from pivot
			pivot.CD_ITARExemptionNo = base.US_DDTCITARExemptionNo;
			pivot.CD_DDTCRegoNo = base.US_DDTCRegistrationNo;
			pivot.CD_DDTCJurisdictionNumber = base.US_JurisdictionNumber;
			pivot.CD_DDTCUSMLCategoryCode = base.US_DDTCUSMLCategoryCode;
			pivot.CD_DDTCUnit = base.US_DDTCUnit;
			pivot.CD_ECCN = base.US_ECCN;
			pivot.CD_ExportCode = base.US_ExportCode;
			pivot.CD_MiscLicenceNo = base.US_LicenseNo.SubstringSafe(0, CusUSClassificationSchema.CD_MiscLicenceNo.MaxLength);
			pivot.CD_LicenceType = base.US_LicenseType;

			pivot.CD_OriginIndicator = base.US_AESOriginIndicator;

			pivot.CD_MilitaryEquipInd = base.US_DDTCMilitaryEquipmentIndicator;

			pivot.CD_PartyCertInd = base.US_DDTCPartyCertificationIndicator;

			pivot.CD_AMSIndicator = base.US_AMSInd;
			pivot.CD_AMSDisclaimReason = base.US_AMSDisclaimReason;
			pivot.CD_AMSDisclaimProgram = base.US_AMSDisclaimProgram;

			pivot.CD_PSTIndicator = base.US_PSTIndicator;
			pivot.CD_PSTDisclaimReason = base.US_PSTDisclaimReason;
			pivot.CD_PSTDisclaimProgram = base.US_PSTDisclaimProgram;

			pivot.CD_NMFSHMSIndicator = base.US_NMFSHMSInd;
			pivot.CD_NMFSHMSDisclaimReason = base.US_NMFSHMSDisclaimReason;

			pivot.CD_ATFIndicator = base.US_ATFInd;

			pivot.CD_FWSIndicator = base.US_FWSInd;
			pivot.CD_FWSDisclaimReason = base.US_FWSDisclaimReason;

			pivot.CD_DEAIndicator = base.US_DEAInd;
			pivot.CD_DEADisclaimReason = base.US_AMSDisclaimReason;

			pivot.CD_TTBIndicator = base.US_TTBInd;
			pivot.CD_TTBDisclaimReason = base.US_TTBDisclaimReason;
		}

		internal void AddImportAdditionalDetailsToProducts(CusClassPartPivot pivot)
		{
			// TODO: iinvoiceLine.US_PerUnitCost, invoiceLine.US_RX_NKPerUnitCostCurr, invoiceLine.US_PercentageActiveIngredient,
			//invoiceLine.US_RX_NK98InvCurrPerUnitCurr do not exist. Maybe they are obsolete ? If so, remove them from pivot
			pivot.CD_9802USDValuePerUnit = base.US_9802PerUnit;
			pivot.CD_9802ValuePerUnit = base.US_98InvCurrPerUnit;
			pivot.CD_ADDApplicable = base.US_ADD_NA;
			pivot.CD_ADDDepositRateInd = base.US_ADDDepositRateIndicator;
			pivot.CD_ADDDepositRateOverride = base.US_ADDDepositRateOverride;
			pivot.CD_ADDCaseNo = base.US_ADDCaseNo;
			pivot.CD_AMMVPerUnit = base.US_AMMVPerUnit;
			pivot.CD_AMMVPercentage = base.US_AMMVPercentage;
			pivot.CD_AgricultureLicenceNo = base.US_AgricultureLicNo;
			pivot.CD_SugarCertificate = base.US_CAExportCertificate;
			pivot.CD_CBTPACertificate = base.US_CBTPACertificateNo;
			pivot.CD_CVDApplicable = base.US_CVD_NA;
			pivot.CD_CVDDepositRateInd = base.US_CVDDepositRateIndicator;
			pivot.CD_CVDDepositRateOverride = base.US_CVDDepositRateOverride;
			pivot.CD_CVDCaseNo = base.US_CVDCaseNo;
			pivot.CD_CottonCertificate = base.US_CottonCertificateNo;
			pivot.CD_CottonFeeExempt = base.US_CottonFeeExempt;

			pivot.CD_NAFTANetCost = base.US_IsNAFTANet;
			pivot.CD_ADDBonded = base.US_IsBondedADD;
			pivot.CD_CVDBonded = base.US_IsBondedCVD;
			pivot.CD_MiscLicenceNo = base.US_MiscPermitNo;
			pivot.CD_OA_Manufacturer = base.JI_OA_ManufacturerAddress;
			pivot.CD_OA_Exporter = base.JI_OA_ExporterAddress;
			pivot.CD_RulingNumber = base.US_PIRPRulingNo;
			pivot.CD_RulingType = base.US_PIRPRulingType;
			pivot.CD_SPI = base.US_SPI;
			pivot.CD_ProductClaim = base.US_SecondarySPI;
			pivot.CD_TSCAIndicator = base.US_TSCAIndicator;
			pivot.CD_TaxApplicability = base.US_TaxApply;
			pivot.CD_TaxCode = base.US_TaxCode;
			pivot.CD_TaxRateType = base.US_TaxRateT;

			if (IsCBMAProductClaimAndIsNotCBMA23Effective)
			{
				pivot.CD_TTBRateDesignationCode = base.US_TaxRateS;
				pivot.CD_CBMADefaultTaxRate = base.US_TaxRate;
			}
			else
			{
				pivot.CD_TaxRateDesc = base.US_TaxRateS;
				pivot.CD_TaxRate = base.US_TaxRate;
				if (IsCBMAProductClaimAndIsCBMA23Effective)
				{
					pivot.CD_TTBRateDesignationCode = base.US_TTBRateDesignationCode;
					pivot.CD_CBMADefaultTaxRate = base.US_CBMADefaultTaxRate;
				}
			}

			pivot.CD_UC_NKCountryOfExport = base.US_UC_NKCountryOfExport;
			pivot.CD_UC_NKCountryOfOrigin = base.US_UC_NKCountryOfOrigin;
			pivot.CD_WoolLicenceNo = base.US_WoolLicenceNo;
			pivot.CD_ZoneStatus = base.US_ZoneStatus;

			pivot.CD_LaceyActIndicator = base.US_LaceyIndicator;
			pivot.CD_LaceyActDisclaimReason = base.US_LaceyDisclaimReason;

			pivot.CD_ACEFDAIndicator = base.US_FDAIndicator;
			pivot.CD_ACEFDADisclaimReason = base.US_FDADisclaimReason;

			pivot.CD_NHTSAIndicator = base.US_NHTSAIndicator;
			pivot.CD_NHTSADisclaimReason = base.US_NHTDisclaimReason;

			pivot.CD_ATFIndicator = base.US_ATFInd;

			pivot.CD_ODSIndicator = base.US_ODSInd;
			pivot.CD_ODSDisclaimReason = base.US_ODSDisclaimReason;

			pivot.CD_TSCAClaimIndicator = base.US_TSCAInd;
			pivot.CD_TSCADisclaimReason = base.US_TSCADisclaimReason;

			pivot.CD_PSTIndicator = base.US_PSTIndicator;
			pivot.CD_PSTDisclaimReason = base.US_PSTDisclaimReason;
			pivot.CD_PSTDisclaimProgram = base.US_PSTDisclaimProgram;

			pivot.CD_HFCIndicator = base.US_HFCInd;
			pivot.CD_HFCDisclaimReason = base.US_HFCDisclaimReason;

			pivot.CD_OMCIndicator = base.US_OMCInd;
			pivot.CD_OMCDisclaimReason = base.US_OMCDisclaimReason;

			pivot.CD_VNEIndicator = base.US_VNEInd;
			pivot.CD_VNEDisclaimReason = base.US_VNEDisclaimReason;

			pivot.CD_AMSIndicator = base.US_AMSInd;
			pivot.CD_AMSDisclaimReason = base.US_AMSDisclaimReason;
			pivot.CD_AMSDisclaimProgram = base.US_AMSDisclaimProgram;

			pivot.CD_NOPIndicator = base.US_NOPInd;
			pivot.CD_NOPDisclaimReason = base.US_NOPDisclaimReason;

			pivot.CD_TTBIndicator = base.US_TTBInd;
			pivot.CD_TTBDisclaimReason = base.US_TTBDisclaimReason;

			pivot.CD_CPSCIndicator = base.US_CPSCInd;
			pivot.CD_CPSCDisclaimReason = base.US_CPSCDisclaimReason;

			pivot.CD_DEAIndicator = base.US_DEAInd;
			pivot.CD_DEADisclaimReason = base.US_DEADisclaimReason;

			pivot.CD_APHISIndicator = base.US_APHISInd;
			pivot.CD_APHISDisclaimReason = base.US_APHISDisclaimReason;

			pivot.CD_DDTCIndicator = base.US_DDTCInd;

			pivot.CD_NMFS370Indicator = base.US_NMFS370Ind;
			pivot.CD_NMFS370DisclaimReason = base.US_NMFS370DisclaimReason;

			pivot.CD_NMFSAMRIndicator = base.US_NMFSAMRInd;
			pivot.CD_NMFSAMRDisclaimReason = base.US_NMFSAMRDisclaimReason;

			pivot.CD_NMFSHMSIndicator = base.US_NMFSHMSInd;
			pivot.CD_NMFSHMSDisclaimReason = base.US_NMFSHMSDisclaimReason;

			pivot.CD_NMFSSIMPIndicator = base.US_NMFSSIMPInd;

			pivot.CD_FWSIndicator = base.US_FWSInd;
			pivot.CD_FWSDisclaimReason = base.US_FWSDisclaimReason;

			pivot.CD_ProductExclusion = base.US_ProductExclusion;
			pivot.CD_ExclusionNumber = base.US_ExclusionNumber;

			pivot.CD_PrimaryCountryNA = base.US_Prim_NA;
			pivot.CD_RN_NKPrimaryCountry = base.US_RN_NKPrimCtry;
			pivot.CD_SecondaryCountryNA = base.US_Sec_NA;
			pivot.CD_RN_NKSecondaryCountry = base.US_RN_NKSecCtry;
			pivot.CD_RN_NKCastCountry = base.US_RN_NKCastCtry;
			pivot.CD_RN_NKCertificateOrigin = base.US_RN_NKCertOrigin;
			pivot.CD_RN_NKMeltCountry = base.US_RN_NKMeltCtry;
			pivot.CD_NMFSCOAIndicator = base.US_NMFSCOAInd;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void UpdateDetailsOnPartChangeForImport()
		{
			CusClassPartPivot bestMatch = Pivot ?? Factory.GetNull<CusClassPartPivot>();
			SetOriginCountryFromProducts(bestMatch);
			US_SupTariff = bestMatch.CI_SupplementalTariff.Left(AddInfo.Schema.US_SupTariffMaxLength);
			if (HasEmptySupTariff)
			{
				SetDefaultSupTariffs();
			}
			AddDetailsFromProducts(bestMatch);
			var cottonCertificateNoToAddToLicenceAndPermits = SetDefaultCottonCertificateNo(bestMatch);
			ZDecimal unitPrice = ZDecimal.Zero;
			ZDecimal invCurrPerUnit = ZDecimal.Zero;
			if (JI_RX_NKLinePriceCurr == bestMatch.CD_RX_NKPerUnitCostCurr)
			{
				unitPrice = bestMatch.CD_PerUnitCost;
			}
			if (JI_RX_NKLinePriceCurr == bestMatch.CD_RX_NK9802ValuePerUnitCurr && !US_98GoodsValue_ReadOnly)
			{
				invCurrPerUnit = bestMatch.CD_9802ValuePerUnit;
			}
			var declaration = Declaration;
			USLinkedToDeclarationData linkedToDeclarationData = null;
			if (declaration != null)
			{
				linkedToDeclarationData = (USLinkedToDeclarationData)declaration.GetNewLinkedToDeclarationData();
			}

			US_98InvCurrPerUnit = invCurrPerUnit;

			if (US_98GoodsValue_ReadOnly)
			{
				US_9802PerUnit = ZDecimal.Zero;
			}

			if (unitPrice > 0m)
			{
				UnitPrice = unitPrice;
			}

			UpdateOGA_PGADetails(bestMatch, Part, linkedToDeclarationData, cottonCertificateNoToAddToLicenceAndPermits);

			if (!IsFTZAdmission)
			{
				MapValuesBetweenACEAndACS(true);
			}

			if (!bestMatch.IsNull && declaration != null && bestMatch.Children.Count > 0 && !declaration.IsDefaultSecondaryTariffLinesSuspended)
			{
				var pivotChildren = bestMatch.Children.Cast<CusClassPartPivot>().OrderBy(x => x.CI_ChildListOrder).ToArray();
				(Dictionary<CusClassPartPivot, JobComInvoiceLine> matchedChildLines, List<Tuple<ZGuid, ZString>> tariffToCustomFieldsList) = GetMatchedChildLinesDeleteIfUnableToMatchAndStoreCustomsFields(pivotChildren.Where(x => x.CI_ChildType == ClassificationChildTypeList.Codes.COMPONENT).ToArray());
				var invoiceHeader = InvoiceHeader;
				using (Declaration.SuspendDefaultingSecondaryTariffLines())
				using (invoiceHeader.GetLineNumberRenumberingSuspender())
				{
					var shouldReNumber = !invoiceHeader.IsLineNumberRenumberingForDataImportSuspended;
					var remainingInvoiceLines = new List<JobComInvoiceLine>();
					ZShort lineNo = 0;
					if (shouldReNumber)
					{
						var existingInvoiceLines = new List<JobComInvoiceLine>(Factory.Load<JobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JZ, invoiceHeader.PK)));
						if (!existingInvoiceLines.Contains(this))
						{
							existingInvoiceLines.Add(this);
						}
						remainingInvoiceLines.AddRange(existingInvoiceLines.OrderBy(x => x.JI_LineNo).Select(x =>
						{
							x.InitialisePartSyncManager(); // in case the OnLoaded hasn't been called in CreateBusinessObjectsFromRows yet.
							return x;
						}));
						existingInvoiceLines = null;
						foreach (var existingInvoiceLine in remainingInvoiceLines.ToArray())
						{
							lineNo++;
							if (existingInvoiceLine.JI_LineNo != lineNo)
							{
								existingInvoiceLine.JI_LineNo = lineNo;
							}
							remainingInvoiceLines.Remove(existingInvoiceLine);
							if (existingInvoiceLine == this)
							{
								break;
							}
						}
					}

					var relatedPart = Part;
					var shouldResetLineValuesForCombinedLines = false;
					var shouldMatch = matchedChildLines.Count > 0;
					var noMatchLines = new List<JobComInvoiceLine>();
					foreach (var pivot in pivotChildren)
					{
						var tariffNum = pivot.CI_TariffNum;
						var shouldRestoreCustomsFields = false;
						JobComInvoiceLine newChild = null;
						if (pivot.CI_ChildType == ClassificationChildTypeList.Codes.COMPONENT)
						{
							if (shouldMatch)
							{
								newChild = matchedChildLines[pivot];
							}

							if (newChild == null)
							{
								newChild = AddSecondaryInvoiceLine();
								shouldRestoreCustomsFields = true;
							}
						}
						else if (pivot.CI_ChildType == ClassificationChildTypeList.Codes.Related)
						{
							newChild = AddProductRelatedInvoiceLine();
						}

						if (newChild != null)
						{
							newChild.JI_LineNo = ++lineNo;
							newChild.JI_Tariff = tariffNum;
							newChild.SetOriginCountryFromProducts(pivot);
							newChild.US_SupTariff = pivot.CI_SupplementalTariff.Left(AddInfo.Schema.US_SupTariffMaxLength);
							newChild.AddDetailsFromProducts(pivot);
							cottonCertificateNoToAddToLicenceAndPermits = newChild.SetDefaultCottonCertificateNo(pivot);

							newChild.JI_InvoiceUQ = JI_InvoiceUQ;
							newChild.US_TaxApply = US_TaxApply;
							newChild.JI_Description = pivot.CI_Description.IsEmpty ? JI_Description : pivot.CI_Description;
							unitPrice = ZDecimal.Zero;
							invCurrPerUnit = ZDecimal.Zero;
							if (newChild.JI_RX_NKLinePriceCurr == pivot.CD_RX_NKPerUnitCostCurr)
							{
								unitPrice = pivot.CD_PerUnitCost;
							}
							if (newChild.JI_RX_NKLinePriceCurr == pivot.CD_RX_NK9802ValuePerUnitCurr)
							{
								invCurrPerUnit = pivot.CD_9802ValuePerUnit;
							}
							var childAIILine = newChild.FirstAIILine;
							if (childAIILine != null && newChild.IsNonLineGroupingOrOnlyOneAIILine)
							{
								childAIILine.US_UnitBasis = 1;
								childAIILine.US_PercActvIngr = ZDecimal.Zero;
								childAIILine.US_UnitPrice = unitPrice;
								childAIILine.US_98InvCurrPerUnit = invCurrPerUnit;
							}
							else
							{
								newChild.UnitPrice = unitPrice;
							}
							newChild.JI_WeightUQ = pivot.CD_WeightUQ;
							newChild.JI_NetWeightUQ = pivot.CD_WeightUQ;

							newChild.UpdateOGA_PGADetails(pivot, relatedPart, linkedToDeclarationData, cottonCertificateNoToAddToLicenceAndPermits);
							newChild.US_98InvCurrPerUnit = invCurrPerUnit;
							if (!IsFTZAdmission)
							{
								newChild.MapValuesBetweenACEAndACS();
							}
							if (shouldRestoreCustomsFields)
							{
								var tariffToCustomFields = tariffToCustomFieldsList.FirstOrDefault(l => l.Item2 == tariffNum);
								if (tariffToCustomFields != null)
								{
									CustomFieldsConversionHelper.MoveCustomFields(Factory, JobComInvoiceLineSchema.Constants.Prefix, tariffToCustomFields.Item1, JobComInvoiceLineSchema.Constants.Prefix, newChild.PK);
									tariffToCustomFieldsList.Remove(tariffToCustomFields);
								}
								else
								{
									noMatchLines.Add(newChild);
								}
							}
							shouldResetLineValuesForCombinedLines |= !tariffNum.IsEmpty;
						}
					}

					if (shouldReNumber)
					{
						remainingInvoiceLines.ForEach(x => x.JI_LineNo = ++lineNo);
					}

					RestoreCustomFieldsForNonMatched(tariffToCustomFieldsList, noMatchLines);

					if (shouldResetLineValuesForCombinedLines)
					{
						ResetInvoiceLineValuesForCombinedLines();
					}
				}
			}
		}

		void RestoreCustomFieldsForNonMatched(List<Tuple<ZGuid, ZString>> tariffToCustomFieldsList, IEnumerable<JobComInvoiceLine> noMatchLines)
		{
			if (tariffToCustomFieldsList.Count > 0)
			{
				foreach (var noMatchLine in noMatchLines)
				{
					var tariffToCustomFields = tariffToCustomFieldsList.FirstOrDefault();
					if (tariffToCustomFields != null)
					{
						CustomFieldsConversionHelper.MoveCustomFields(Factory, JobComInvoiceLineSchema.Constants.Prefix, tariffToCustomFields.Item1, JobComInvoiceLineSchema.Constants.Prefix, noMatchLine.PK);
						if (tariffToCustomFieldsList.Remove(tariffToCustomFields) && tariffToCustomFieldsList.Count == 0)
						{
							break;
						}
					}
				}
			}
		}

		(Dictionary<CusClassPartPivot, JobComInvoiceLine> matchedChildLines, List<Tuple<ZGuid, ZString>> tariffToCustomFieldsList) GetMatchedChildLinesDeleteIfUnableToMatchAndStoreCustomsFields(CusClassPartPivot[] components)
		{
			var matchedChildLines = new Dictionary<CusClassPartPivot, JobComInvoiceLine>();
			var tariffToCustomFieldsList = new List<Tuple<ZGuid, ZString>>();
			var existingChildLines = ChildLines.ToList();
			var shouldDeleteExisting = existingChildLines.Count != components.Length;
			if (!shouldDeleteExisting)
			{
				for (var i = 0; i < components.Length; i++)
				{
					var component = components[i];
					var childLine = existingChildLines[i];
					if (component.CI_TariffNum == childLine.JI_Tariff)
					{
						matchedChildLines.Add(component, childLine);
					}
					else
					{
						shouldDeleteExisting = true;
						break;
					}
				}
			}

			if (shouldDeleteExisting)
			{
				foreach (var child in existingChildLines)
				{
					var tariffToCustomFields = new Tuple<ZGuid, ZString>(ZGuid.NewZGuid(), child.JI_Tariff);
					if (CustomFieldsConversionHelper.MoveCustomFields(child.Factory, JobComInvoiceLineSchema.Constants.Prefix, child.PK, JobComInvoiceLineSchema.Constants.Prefix, tariffToCustomFields.Item1))
					{
						tariffToCustomFieldsList.Add(tariffToCustomFields);
					}

					child.Delete();
				}
				RefreshChildLines();
				matchedChildLines.Clear();
			}

			return (matchedChildLines, tariffToCustomFieldsList);
		}

		void ResetInvoiceLineValuesForCombinedLines()
		{
			if (!(((ISupportDataImporting)this).IsImportingData) && this.IsCombinedLine() && !IsSetXLine && !IsSetVLine)
			{
				var setterSuspender = this.SetterSuspender;
				var regularTariffLine = this.GetCombinedLines().FirstOrDefault(x => x.IsNormalTariffLine());
				if (regularTariffLine != null && regularTariffLine.ParentTariffLine is JobComInvoiceLine parentTariffLine)
				{
					ResumeSettingAndSetValue(SetterSuspender, JobComInvoiceLine.Schema.JI_LinePrice, regularTariffLine, parentTariffLine);
					ResumeSettingAndSetValue(SetterSuspender, JobComInvoiceLine.Schema.JI_InvoiceQuantity, regularTariffLine, parentTariffLine);
					ResumeSettingAndSetValue(SetterSuspender, JobComInvoiceLine.Schema.JI_Weight, regularTariffLine, parentTariffLine);
					ResumeSettingAndSetValue(SetterSuspender, JobComInvoiceLine.Schema.JI_NetWeight, regularTariffLine, parentTariffLine);
					ResumeSettingAndSetValue(SetterSuspender, JobComInvoiceLine.Schema.JI_Volume, regularTariffLine, parentTariffLine);
					ResumeSettingAndSetValue(SetterSuspender, JobComInvoiceLine.Schema.US_ProductExclusion, regularTariffLine, parentTariffLine);
					ResumeSettingAndSetValue(SetterSuspender, JobComInvoiceLine.Schema.US_ExclusionNumber, regularTariffLine, parentTariffLine);
				}
			}
		}

		void ResumeSettingAndSetValue(SetterSuspender setterSuspender, string propertyName, JobComInvoiceLine regularTariffLine, JobComInvoiceLine parentTariffLine)
		{
			using (setterSuspender.ResumeSetting(propertyName))
			{
				var valueFromParentTariffLine = (IZType)parentTariffLine[propertyName];
				var valueFromRegularTariffLine = (IZType)regularTariffLine[propertyName];
				if (!valueFromParentTariffLine.IsEmpty && valueFromRegularTariffLine.IsEmpty)
				{
					parentTariffLine[propertyName] = valueFromParentTariffLine.Default;
					regularTariffLine[propertyName] = valueFromParentTariffLine;
				}
			}
		}

		IEnumerable<string> GetPropertyNames(System.Collections.IEnumerable infos)
		{
			foreach (ZPropertyInfo info in infos)
			{
				yield return info.Name;
			}
		}

		public new CusClassPartPivot Pivot
		{
			get { return (CusClassPartPivot)base.Pivot; }
		}

		protected override BaseCusClassPartPivot GetPivotCore()
		{
			BaseCusClassPartPivot result = null;
			OrgSupplierPart part = Part;
			bool justUpdatedByDataRefresh = false;
			if (part != null)
			{
				justUpdatedByDataRefresh = part.JustUpdatedByDataRefresh;
				if (IsDrawbackDeclaration)
				{
					if (IsForExportSectionOfDrawback)
					{
						result = part.GetUSPivots().GetExportMatch(Core.Constants.CountryCodes.UnitedStates, true, ZGuid.Empty, ZGuid.Empty, EffectiveDateForDutyRate);
					}
					else if (IsForImportSectionOfDrawback)
					{
						result = part.GetUSPivots().GetImportMatch(Core.Constants.CountryCodes.UnitedStates, ZGuid.Empty, ZGuid.Empty, EffectiveDateForDutyRate);
					}
				}
				else if (IsImport)
				{
					result = part.GetUSPivots().GetImportMatch(Core.Constants.CountryCodes.UnitedStates, InvoiceHeader.JZ_OH_Buyer, InvoiceHeader.JZ_OH_Supplier, EffectiveDateForDutyRate, GetPartAttribs());
				}
				else if (IsExport)
				{
					result = part.GetUSPivots().GetExportMatch(Core.Constants.CountryCodes.UnitedStates, UseScheduleB, InvoiceHeader.JZ_OH_Buyer, InvoiceHeader.JZ_OH_Supplier, EffectiveDateForDutyRate);
				}
			}
			ZGuid newPivotPK = result == null ? ZGuid.Empty : result.PK;
			wasPivotChanged = justUpdatedByDataRefresh || previousPivotPK != newPivotPK;
			if (!isSuspendPivotChange)
			{
				previousPivotPK = newPivotPK;
			}

			return result;
		}

		internal IDisposable SuspendPreviousPivotPKUpdate()
		{
			isSuspendPivotChange = true;
			return new DisposableAction(() => isSuspendPivotChange = false);
		}

		bool WasPivotChanged
		{
			get
			{
				var pivot = Pivot;
				return wasPivotChanged;
			}
		}
		bool wasPivotChanged;

		ZGuid previousPivotPK;

		protected override bool IsProductAuditedOrInvalidCore()
		{
			isSuspendPivotChange = true;
			var result = base.IsProductAuditedOrInvalidCore();
			isSuspendPivotChange = false;
			return result;
		}
		bool isSuspendPivotChange;

		void UpdateOGAPGADetailsFromProduct(CusClassPartPivot pivot, OrgSupplierPart part, USLinkedToDeclarationData linkedToDeclarationData)
		{
			if (pivot != null && !pivot.IsNull && part != null && !part.JustUpdatedByDataRefresh)
			{
				CalculateShouldCopyPGAFromProduct();
				CopyACEFDADetailsFromProduct(pivot);

				if (!IsFTZAdmission)
				{
					CopyLaceyActDetailsFromProduct(pivot, linkedToDeclarationData);
					CopyCensusWarningOverrideFromProduct(pivot);
					CopyNHTSADetailsFromProduct(pivot);
					CopyATFdetailsFromProduct(pivot);
					CopyODSTSCAdetailsFromProduct(pivot);
					CopyPSTdetailsFromProduct(pivot);
					CopyHFCdetailsFromProduct(pivot);
					CopyOMCDetailsFromProduct(pivot);
					CopyVNEdetailsFromProduct(pivot);
					CopyTTBDetailsFromProduct(pivot);
					CopyAMSDetailsFromProduct(pivot);
					CopyNOPDetailsFromProduct(pivot);
					CopyCPSCDetailsFromProduct(pivot);
					CopyDEADetailsFromProduct(pivot);
					CopyAPHISDetailsFromProduct(pivot);
					CopyDDTCDetailsFromProduct(pivot);
					CopyNMFS370DeatilsFromProduct(pivot);
					CopyNMFSCOADeatilsFromProduct(pivot);
					CopyNMFSSIMPDeatilsFromProduct(pivot);
					CopyNMFSAMRDeatilsFromProduct(pivot);
					CopyNMFSHMSDeatilsFromProduct(pivot);
					CopyFWSDetailsFromProduct(pivot);
				}
			}
		}

		void CalculateShouldCopyPGAFromProduct()
		{
			var declaration = Declaration;
			if (declaration != null && declaration.IsACECargoCertificationMode)
			{
				var isEntrySummary = declaration.US_EnableENS;
				var isCargoRelease = declaration.US_EnableCRL;
				var entryType = declaration.US_EntryType;
				var isExpeditedRelease = declaration.US_PGAExpeditedRelease;
				var isCertified = declaration.US_CertifyCargoRelease;
				var isWeeklyEstimateFiling = declaration.IsConsumptionFTZ && declaration.IsWeeklyEstimateFilingDate;

				ShouldNotDefaultFDAForProductXMLImport = true;
				ShouldNotDefaultACEFDAForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.FDA);
				ShouldNotDefaultPGAForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.Lacey);
				ShouldNotDefaultNHTSAForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.NHTSA);
				ShouldNotDefaultATFForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.ATF);
				ShouldNotDefaultODSTSCAForProductXMLImport |= !(GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.ODS) || GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.TSCA));
				ShouldNotDefaultPSTForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.PST);
				ShouldNotDefaultHFCForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.HFC);
				ShouldNotDefaultOMCForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.OMC);
				ShouldNotDefaultVNEForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.VNE);
				ShouldNotDefaultTTBForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.TTB);
				ShouldNotDefaultAMSForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.AMS);
				ShouldNotDefaultNOPForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.NOP);
				ShouldNotDefaultCPSCForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.CPSC);
				ShouldNotDefaultDEAForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.DEA);
				ShouldNotDefaultAPHISForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.APHIS);

				ShouldNotDefaultFWSForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.FWS);
				ShouldNotDefaultNMFS370ForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes._370);
				ShouldNotDefaultNMFSCOAForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.COA);
				ShouldNotDefaultNMFSAMRForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.AMR);
				ShouldNotDefaultNMFSHMSForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.HMS);
				ShouldNotDefaultNMFSSIMPForProductXMLImport |= !GovernmentAgencyProgramCodeList.IsPGAAllowed(isEntrySummary, isCargoRelease, isCertified, isExpeditedRelease, isWeeklyEstimateFiling, entryType, GovernmentAgencyProgramCodeList.Codes.SIMP);
			}
		}

		void CopyCensusWarningOverrideFromProduct(CusClassPartPivot pivot)
		{
			foreach (var cwo in pivot.CensusWarningOverrides)
			{
				CensusWarningOverrides.Add((CensusWarningOverride)cwo.Clone());
			}
		}

		void CopyLaceyActDetailsFromProduct(CusClassPartPivot pivot, USLinkedToDeclarationData linkedToDeclarationData)
		{
			if (!ShouldNotDefaultPGAForProductXMLImport)
			{
				US_LaceyIndicator = pivot.CD_LaceyActIndicator;
				US_LaceyDisclaimReason = pivot.CD_LaceyActDisclaimReason;

				foreach (PGA pga in pivot.PGAs)
				{
					var clonedPGA = (PGA)pga.Clone();

					LaceyActLines.Add(clonedPGA);
					if (linkedToDeclarationData != null)
					{
						if (!linkedToDeclarationData.IsACECargoCertificationMode)
						{
							clonedPGA.Licenses.RemoveAndDeleteAll();
							clonedPGA.US_UnknownBreakdown = false;
							clonedPGA.US_TrackingStatus = ZString.Empty;

							foreach (ConstituentElement elem in clonedPGA.PG04ConstituentElements)
							{
								elem.US_PGAPercentOfConstituentElement = elem.US_PGAPercentOfConstituentElement.Round(3);
								elem.US_PGAQuantityOfConstituentElement = elem.US_PGAQuantityOfConstituentElement.Round(2);
								elem.UpdateAddInfoProperties();
							}
						}

						clonedPGA.TransformData(linkedToDeclarationData.IsACECargoCertificationMode ? JobApplicationCodeList.Codes.ACE : JobApplicationCodeList.Codes.ACS);
						clonedPGA.US_CertifyingIndividual = ZString.Empty;
					}
					clonedPGA.SetPGARelatedContainers();
					clonedPGA.US_CertifyingIndividual = pga.US_CertifyingIndividual;
				}
			}
		}

		void CopyCPSCDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultCPSCForProductXMLImport)
			{
				US_CPSCInd = pivot.CD_CPSCIndicator;
				US_CPSCDisclaimReason = pivot.CD_CPSCDisclaimReason;

				if (CPSCHeaders.Count > 0)
				{
					CPSCHeaders.RemoveAndDeleteAll();
				}

				foreach (var cpsc in pivot.CPSCLines.Cast<CPSCHeader>())
				{
					var clonedCPSC = cpsc.Clone() as CPSCHeader;
					clonedCPSC.Lots.RemoveAndDeleteAll();
					clonedCPSC.RuleAndLabs.RemoveAndDeleteAll();
					CPSCHeaders.Add(clonedCPSC);

					foreach (CPSCRule rule in cpsc.RuleAndLabs)
					{
						var newRule = clonedCPSC.RuleAndLabs.AddNew();
						rule.UpdateAddInfoProperties();
						newRule.B7_AddInfoData = rule.B7_AddInfoData;
						newRule.UpdateAddInfoProperties();

						foreach (CPSCReport report in rule.ReportAndLabs)
						{
							var newReport = newRule.ReportAndLabs.AddNew();
							report.UpdateAddInfoProperties();
							newReport.B7_AddInfoData = report.B7_AddInfoData;
							newReport.UpdateAddInfoProperties();
						}
					}
				}
			}
		}

		void CopyNHTSADetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNHTSAForProductXMLImport)
			{
				US_NHTSAIndicator = pivot.CD_NHTSAIndicator;
				US_NHTDisclaimReason = pivot.CD_NHTSADisclaimReason;

				foreach (NHTSAHeader nhtsa in pivot.NHTSALines)
				{
					var clonedNHTSA = nhtsa.Clone() as NHTSAHeader;
					clonedNHTSA.US_CertifyingIndividual = ZString.Empty;
					clonedNHTSA.US_TrackingStatus = ZString.Empty;
					NHTSALines.Add(clonedNHTSA);
					clonedNHTSA.US_CertifyingIndividual = nhtsa.US_CertifyingIndividual;
				}
			}
		}

		void CopyATFdetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultATFForProductXMLImport)
			{
				US_ATFInd = pivot.CD_ATFIndicator;

				foreach (ATF atf in pivot.ATFLines)
				{
					var clonedATF = atf.Clone() as ATF;
					clonedATF.US_TrackingStatus = ZString.Empty;
					ATFLines.Add(clonedATF);
				}
			}
		}

		void CopyODSTSCAdetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultODSTSCAForProductXMLImport)
			{
				US_ODSInd = pivot.CD_ODSIndicator;
				US_ODSDisclaimReason = pivot.CD_ODSDisclaimReason;
				US_TSCAInd = pivot.CD_TSCAClaimIndicator;
				US_TSCADisclaimReason = pivot.CD_TSCADisclaimReason;
				US_TSCACertification = pivot.CD_TSCAIndicator;
				US_TSCAODSCertIndividual = pivot.CD_TSCAODSCertIndividual;
			}
		}

		void CopyOMCDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultOMCForProductXMLImport)
			{
				US_OMCInd = pivot.CD_OMCIndicator;
				US_OMCDisclaimReason = pivot.CD_OMCDisclaimReason;

				foreach (OMCHeader omc in pivot.OMCHeaders)
				{
					var clonedOMC = omc.Clone() as OMCHeader;
					OMCHeaders.Add(clonedOMC);
				}
			}
		}

		void CopyPSTdetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultPSTForProductXMLImport)
			{
				US_PSTIndicator = pivot.CD_PSTIndicator;
				US_PSTDisclaimReason = pivot.CD_PSTDisclaimReason;
				US_PSTDisclaimProgram = pivot.CD_PSTDisclaimProgram;

				foreach (Pesticide pst in pivot.PSTLines)
				{
					var clonedPST = pst.Clone() as Pesticide;
					clonedPST.US_TrackingStatus = ZString.Empty;
					PSTLines.Add(clonedPST);
				}
			}
		}

		void CopyHFCdetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultHFCForProductXMLImport)
			{
				US_HFCInd = pivot.CD_HFCIndicator;
				US_HFCDisclaimReason = pivot.CD_HFCDisclaimReason;

				foreach (USHFCHeader hfc in pivot.HFCHeaders)
				{
					var clonedHFC = hfc.Clone() as USHFCHeader;
					USHFCHeaders.Add(clonedHFC);
				}
			}
		}

		void CopyDDTCDetailsFromProduct(CusClassPartPivot pivot)
		{
			US_DDTCInd = pivot.CD_DDTCIndicator;
			US_DDTCExemptionCode = pivot.CD_ITARExemptionNo.Left(8);
			US_DDTCLicenseNo = pivot.CD_DDTCLicenceNo;
			US_DDTCLicenseType = pivot.CD_DDTCLicenceType;
			US_DDTCRegistrationNo = pivot.CD_DDTCRegoNo;
		}

		void CopyVNEdetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultVNEForProductXMLImport)
			{
				US_VNEInd = pivot.CD_VNEIndicator;
				US_VNEDisclaimReason = pivot.CD_VNEDisclaimReason;

				foreach (Vehicle vne in pivot.VehicleLines)
				{
					var clonedVNE = vne.Clone() as Vehicle;
					clonedVNE.US_CertifyingIndividual = ZString.Empty;
					clonedVNE.US_TrackingStatus = ZString.Empty;
					VehicleLines.Add(clonedVNE);
					clonedVNE.US_CertifyingIndividual = vne.US_CertifyingIndividual;
				}
			}
		}

		void CopyDEADetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultDEAForProductXMLImport)
			{
				US_DEAInd = pivot.CD_DEAIndicator;
				US_DEADisclaimReason = pivot.CD_DEADisclaimReason;

				foreach (var dea in pivot.DEAHeaders)
				{
					var clonedDEA = dea.Clone() as DEAHeader;
					clonedDEA.US_TrackingStatus = ZString.Empty;
					DEAHeaders.Add(clonedDEA);
				}
			}
		}

		void CopyAPHISDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultAPHISForProductXMLImport)
			{
				US_APHISInd = pivot.CD_APHISIndicator;
				US_APHISDisclaimReason = pivot.CD_APHISDisclaimReason;

				foreach (APHISHeader aphis in pivot.APHISHeaders)
				{
					var clonedAPHIS = aphis.Clone() as APHISHeader;
					clonedAPHIS.US_TrackingStatus = ZString.Empty;
					APHISHeaders.Add(clonedAPHIS);

					foreach (APHISProduct product in aphis.Products)
					{
						clonedAPHIS.Products.Add(product.Clone());
					}

					foreach (APHISInspection inspection in aphis.Inspections)
					{
						clonedAPHIS.Inspections.Add(inspection.Clone());
					}

					foreach (APHISLicense license in aphis.Licenses)
					{
						clonedAPHIS.Licenses.Add(license.Clone());
					}
				}
			}
		}

		void UpdateOGA_PGADetails(CusClassPartPivot pivot, OrgSupplierPart part, USLinkedToDeclarationData linkedToDeclarationData, ZString cottonCertificateNoToAddToLicenceAndPermits)
		{
			CleanUpOGA_PGADetails(pivot);
			if (!cottonCertificateNoToAddToLicenceAndPermits.IsEmpty)
			{
				LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._22, cottonCertificateNoToAddToLicenceAndPermits);
			}
			UpdateOGAPGADetailsFromProduct(pivot, part, linkedToDeclarationData);
		}

		void CleanUpOGA_PGADetails(CusClassPartPivot pivot)
		{
			if (pivot == null || pivot.IsNull || !PartWasJustUpdatedByDataRefresh(pivot))
			{
				if (!ShouldNotDefaultFDAForProductXMLImport)
				{
					FDAs.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultACEFDAForProductXMLImport)
				{
					ACE_FDALines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultDOTForProductXMLImport)
				{
					DOTs.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultNHTSAForProductXMLImport)
				{
					NHTSALines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultPGAForProductXMLImport)
				{
					LaceyActLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultATFForProductXMLImport)
				{
					ATFLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultPSTForProductXMLImport)
				{
					PSTLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultHFCForProductXMLImport)
				{
					USHFCHeaders.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultOMCForProductXMLImport)
				{
					OMCHeaders.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultVNEForProductXMLImport)
				{
					VehicleLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultTTBForProductXMLImport)
				{
					TTBLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultAMSForProductXMLImport)
				{
					var amsLines = AMSLines.Cast<AMS>().Where(x => AMSProgramList.IsAMSProgramButNotNOP(Factory, x.US_Program));
					var list = new List<AMS>();
					foreach (var ams in amsLines)
					{
						list.Add(ams);
					}
					list.ForEach(x => AMSLines.RemoveAndDelete(x));
				}

				if (!ShouldNotDefaultNOPForProductXMLImport)
				{
					var nopLines = AMSLines.Cast<AMS>().Where(x => x.IsNOPProgram);
					var list = new List<AMS>();
					foreach (var nop in nopLines)
					{
						list.Add(nop);
					}
					list.ForEach(x => AMSLines.RemoveAndDelete(x));
				}

				if (!ShouldNotDefaultCPSCForProductXMLImport)
				{
					CPSCHeaders.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultDEAForProductXMLImport)
				{
					DEAHeaders.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultAPHISForProductXMLImport)
				{
					APHISHeaders.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultNMFSAMRForProductXMLImport)
				{
					NMFSAMRLines.DeleteAll();
				}

				if (!ShouldNotDefaultNMFS370ForProductXMLImport)
				{
					NMFS370Lines.DeleteAll();
				}

				if (!ShouldNotDefaultNMFSCOAForProductXMLImport)
				{
					NMFSCOALines.DeleteAll();
				}

				if (!ShouldNotDefaultFWSForProductXMLImport)
				{
					FWSHeaders.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultNMFSHMSForProductXMLImport)
				{
					NMFSHMSLines.DeleteAll();
				}

				if (!ShouldNotDefaultNMFSSIMPForProductXMLImport)
				{
					NMFSSIMPLines.DeleteAll();
				}

				LicenceAndPermits.RemoveAndDeleteAll();
				CensusWarningOverrides.RemoveAndDeleteAll();
				RemoveDDTCFields();
			}
		}

		bool PartWasJustUpdatedByDataRefresh(CusClassPartPivot pivot)
		{
			var part = pivot != null && !pivot.IsNull ? pivot.Part : null;
			return part != null && part.JustUpdatedByDataRefresh;
		}

		void CopyACEFDADetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultACEFDAForProductXMLImport)
			{
				var indicatorValue = pivot.CD_ACEFDAIndicator;
				US_FDAIndicator = indicatorValue;
				US_FDADisclaimReason = OGAIndicatorList.IsToBeDisclaimed(indicatorValue) && !IsFTZAdmission ? pivot.CD_ACEFDADisclaimReason : ZString.Empty;
				if (OGAIndicatorList.IsToBeDeclared(indicatorValue))
				{
					foreach (ACEFDA fda in pivot.ACEFDAs)
					{
						var clonedFDA = (ACEFDA)fda.Clone();
						clonedFDA.US_TrackingStatus = ZString.Empty;
						if (fda.EnableDocAddressForFDA)
						{
							fda.DocAddresses.CloneElementsTo(clonedFDA.DocAddresses);
						}
						ACE_FDALines.Add(clonedFDA);
					}
					ACE_FDALines.OfType<ACEFDA>().ForEach(x => x.SetFDADefaultValueWhenCopyingFromProduct());
				}
			}
		}

		void CopyNMFS370DeatilsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNMFS370ForProductXMLImport)
			{
				US_NMFS370Ind = pivot.CD_NMFS370Indicator;
				US_NMFS370DisclaimReason = pivot.CD_NMFS370DisclaimReason;

				foreach (NMFSLine nmfs370 in pivot.NMFS370Lines)
				{
					var cloned370 = (NMFSLine)nmfs370.Clone();
					cloned370.US_TrackingStatus = ZString.Empty;
					NMFSLines.Add(cloned370);
				}
			}
		}

		void CopyNMFSCOADeatilsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNMFSCOAForProductXMLImport)
			{
				US_NMFSCOAInd = pivot.CD_NMFSCOAIndicator;

				foreach (NMFSLine nmfsCOA in pivot.NMFSCOALines)
				{
					var clonedCOA = (NMFSLine)nmfsCOA.Clone();
					clonedCOA.US_TrackingStatus = ZString.Empty;
					NMFSLines.Add(clonedCOA);
				}
			}
		}

		void CopyNMFSAMRDeatilsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNMFSAMRForProductXMLImport)
			{
				US_NMFSAMRInd = pivot.CD_NMFSAMRIndicator;
				US_NMFSAMRDisclaimReason = pivot.CD_NMFSAMRDisclaimReason;

				foreach (NMFSLine nmfsAMR in pivot.NMFSAMRLines)
				{
					var clonedAMR = (NMFSLine)nmfsAMR.Clone();
					clonedAMR.US_TrackingStatus = ZString.Empty;
					NMFSLines.Add(clonedAMR);
				}
			}
		}

		void CopyNMFSHMSDeatilsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNMFSHMSForProductXMLImport)
			{
				US_NMFSHMSInd = pivot.CD_NMFSHMSIndicator;
				US_NMFSHMSDisclaimReason = pivot.CD_NMFSHMSDisclaimReason;

				foreach (NMFSLine nmfsHMS in pivot.NMFSHMSLines)
				{
					var clonedHMS = (NMFSLine)nmfsHMS.Clone();
					clonedHMS.US_TrackingStatus = ZString.Empty;
					NMFSLines.Add(clonedHMS);
				}
			}
		}

		void CopyNMFSSIMPDeatilsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNMFSSIMPForProductXMLImport)
			{
				US_NMFSSIMPInd = pivot.CD_NMFSSIMPIndicator;

				foreach (NMFSLine nmfsSIMP in pivot.NMFSSIMPLines)
				{
					var clonedSIMP = (NMFSLine)nmfsSIMP.Clone();
					clonedSIMP.US_TrackingStatus = ZString.Empty;
					NMFSLines.Add(clonedSIMP);
				}
			}
		}

		void CopyFWSDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultFWSForProductXMLImport)
			{
				US_FWSInd = pivot.CD_FWSIndicator;
				US_FWSDisclaimReason = pivot.CD_FWSDisclaimReason;

				foreach (FWSHeader fws in pivot.FWSLines)
				{
					var clonedFWS = (FWSHeader)fws.Clone();
					clonedFWS.US_TrackingStatus = ZString.Empty;
					FWSHeaders.Add(clonedFWS);
				}
			}
		}

		void CopyTTBDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultTTBForProductXMLImport)
			{
				US_TTBInd = pivot.CD_TTBIndicator;
				US_TTBDisclaimReason = pivot.CD_TTBDisclaimReason;

				foreach (TTBLine ttb in pivot.TTBLines)
				{
					var clonedTTB = (TTBLine)ttb.Clone();
					clonedTTB.US_TrackingStatus = ZString.Empty;
					clonedTTB.DefaultPermitNumberFromIOR(this);
					TTBLines.Add(clonedTTB);
				}
			}
		}

		void CopyAMSDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultAMSForProductXMLImport)
			{
				US_AMSInd = pivot.CD_AMSIndicator;
				US_AMSDisclaimReason = pivot.CD_AMSDisclaimReason;
				US_AMSDisclaimProgram = pivot.CD_AMSDisclaimProgram;

				foreach (AMS ams in pivot.AMSLines.Cast<AMS>().Where(x => AMSProgramList.IsAMSProgramButNotNOP(Factory, x.US_Program)))
				{
					var clonedAMS = (AMS)ams.Clone();
					clonedAMS.US_TrackingStatus = ZString.Empty;
					clonedAMS.AMSLines.RemoveAndDeleteAll();
					AMSLines.Add(clonedAMS);

					foreach (AMSLine amsLine in ams.AMSLines)
					{
						var newAMSLine = clonedAMS.AMSLines.AddNew();
						amsLine.Data.UpdateRelatedPropertyInfo();
						newAMSLine.B7_AddInfoData = amsLine.B7_AddInfoData;
						newAMSLine.Data.UpdateRelatedPropertyInfo();
					}
				}
			}
		}

		void CopyNOPDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNOPForProductXMLImport)
			{
				US_NOPInd = pivot.CD_NOPIndicator;
				US_NOPDisclaimReason = pivot.CD_NOPDisclaimReason;

				foreach (AMS nop in pivot.AMSLines.Cast<AMS>().Where(x => x.IsNOPProgram))
				{
					var clonedNOP = (AMS)nop.Clone();
					clonedNOP.US_TrackingStatus = ZString.Empty;
					clonedNOP.AMSLines.RemoveAndDeleteAll();
					clonedNOP.LotCodes.RemoveAndDeleteAll();
					AMSLines.Add(clonedNOP);

					foreach (AMSLine nopLine in nop.AMSLines)
					{
						var newNOPLine = clonedNOP.AMSLines.AddNew();
						nopLine.Data.UpdateRelatedPropertyInfo();
						newNOPLine.B7_AddInfoData = nopLine.B7_AddInfoData;
						newNOPLine.Data.UpdateRelatedPropertyInfo();
					}

					foreach (var lotCode in nop.LotCodes)
					{
						var newLotCode = clonedNOP.LotCodes.AddNew();
						newLotCode.CY_Code = lotCode.CY_Code;
						newLotCode.CY_Data = lotCode.CY_Data;
					}
				}
			}
		}

		public bool ShouldNotDefaultFDAForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultACEFDAForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultAMSForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultNOPForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultDOTForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultNHTSAForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultPGAForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultATFForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultODSTSCAForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultOMCForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultPSTForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultHFCForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultVNEForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultTTBForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultCPSCForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultDEAForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultAPHISForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultFWSForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultNMFS370ForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultNMFSCOAForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultNMFSAMRForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultNMFSHMSForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultNMFSSIMPForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultExportAMSForProductXMLImport
		{
			get;
			set;
		}

		public bool ShouldNotDefaultExportEPAForProductXMLImport
		{
			get;
			set;
		}

		public override ZDecimal LinePriceForBalanceCalc
		{
			get { return base.LinePriceForBalanceCalc + (IsImport ? US_98ValueInvCurr : ZDecimal.Zero); }
		}

		public override ZDecimal JI_LinePrice
		{
			get { return base.JI_LinePrice; }
			set
			{
				ZDecimal oldValue = JI_LinePrice;
				var parentTariffLine = ParentTariffLine;
				var oldTotalLinePriceIncludingChildLines = parentTariffLine != null ? parentTariffLine.TotalLinePriceIncludingChildLines : TotalLinePriceIncludingChildLines;
				base.JI_LinePrice = value;
				if (!IsCopying && oldValue != JI_LinePrice)
				{
					if (InvoiceHeader is JobComInvoiceHeader invoiceHeader && !invoiceHeader.IsExport)
					{
						invoiceHeader.ApportionLineWeight(this);
					}

					var declaration = this.Declaration;

					if (declaration != null && declaration.IsImport)
					{
						if (FDAs.Count == 1 && oldValue == FDAs[0].US_InvCurrFDAValue)
						{
							FDAs[0].US_InvCurrFDAValue = JI_LinePrice;
						}

						if (ACE_FDALines.Count == 1 && oldValue == ACE_FDALines[0].US_InvCurrValue)
						{
							ACE_FDALines[0].US_InvCurrValue = JI_LinePrice;
						}

						if (LaceyActLines.Count == 1 && oldValue == LaceyActLines[0].US_InvCurrPGAValue)
						{
							LaceyActLines[0].US_InvCurrPGAValue = JI_LinePrice;
						}

						if (IsParentLine || parentTariffLine != null)
						{
							if (OnlyFWSHeaderInSet is FWSHeader fwsHeader && oldTotalLinePriceIncludingChildLines == fwsHeader.US_InvCurrPGAValue)
							{
								fwsHeader.US_InvCurrPGAValue = parentTariffLine != null ? parentTariffLine.TotalLinePriceIncludingChildLines : TotalLinePriceIncludingChildLines;
							}
						}
						else if (FWSHeaders.Count == 1 && FWSHeaders[0] is FWSHeader fwsHeader && oldValue == fwsHeader.US_InvCurrPGAValue)
						{
							fwsHeader.US_InvCurrPGAValue = JI_LinePrice;
						}
					}
					if (IsImport)
					{
						CalculateAMMVCharge();
					}
				}
			}
		}

		public ZDecimal TotalLinePriceIncludingChildLines
		{
			get
			{
				if (totalLinePriceIncludingChildLinesCached == null)
				{
					totalLinePriceIncludingChildLinesCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var result = JI_LinePrice;
						if (IsParentLine)
						{
							result += ChildLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_LinePrice);
						}

						return result;
					});
				}
				return totalLinePriceIncludingChildLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalLinePriceIncludingChildLinesCached;

		public ZDecimal TotalCustomsValueIncludingChildLines
		{
			get
			{
				if (totalCustomsValueIncludingChildLinesCached == null)
				{
					totalCustomsValueIncludingChildLinesCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var isMerged = CusEntryLine != null;
						var result = isMerged ? US_CustomsValue : JI_CustomsValue.Round(0);
						if (IsParentLine)
						{
							result += ChildLines.Cast<JobComInvoiceLine>().Sum(x => isMerged ? x.US_CustomsValue : x.JI_CustomsValue.Round(0));
						}

						return result;
					});
				}
				return totalCustomsValueIncludingChildLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalCustomsValueIncludingChildLinesCached;

		public ZDecimal TotalFWSValueIncludingChildLines
		{
			get
			{
				if (totalFWSValueIncludingChildLinesCached == null)
				{
					totalFWSValueIncludingChildLinesCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var result = FWSHeaders.TotalInvCurrPGAValue;
						if (IsParentLine)
						{
							result += ChildLines.Cast<JobComInvoiceLine>().Sum(x => x.FWSHeaders.TotalInvCurrPGAValue);
						}

						return result;
					});
				}
				return totalFWSValueIncludingChildLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalFWSValueIncludingChildLinesCached;

		public ZDecimal TotalFWSUSDValueIncludingChildLines
		{
			get
			{
				if (totalFWSUSDValueIncludingChildLinesCached == null)
				{
					totalFWSUSDValueIncludingChildLinesCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var result = FWSHeaders.TotalUSDValue;
						if (IsParentLine)
						{
							result += ChildLines.Cast<JobComInvoiceLine>().Sum(x => x.FWSHeaders.TotalUSDValue);
						}

						return result;
					});
				}
				return totalFWSUSDValueIncludingChildLinesCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalFWSUSDValueIncludingChildLinesCached;

		FWSHeader OnlyFWSHeaderInSet
		{
			get
			{
				if (onlyFWSHeaderInSetCached == null)
				{
					onlyFWSHeaderInSetCached = new CachedProperty<FWSHeader>(Factory, () =>
					{
						if (FWSHeaders.Count > 1)
						{
							return null;
						}
						else
						{
							if (ParentTariffLine is JobComInvoiceLine parentTariffLine)
							{
								return parentTariffLine.OnlyFWSHeaderInSet;
							}

							var result = (FWSHeader)FWSHeaders.FirstOrDefault();
							foreach (var childLine in ChildLines)
							{
								var childLineFWSHeadersCount = childLine.FWSHeaders.Count;
								if (childLineFWSHeadersCount == 0)
								{
									continue;
								}
								else if (childLineFWSHeadersCount > 1 || result != null)
								{
									return null;
								}
								result = childLine.FWSHeaders[0];
							}
							return result;
						}
					});
				}
				return onlyFWSHeaderInSetCached.Value;
			}
		}
		CachedProperty<FWSHeader> onlyFWSHeaderInSetCached;

		public override ZShort JI_LineNo
		{
			get { return base.JI_LineNo; }
			set
			{
				base.JI_LineNo = value;
				UpdateReconOriginLineNo();
			}
		}

		bool IsUSOrigin
		{
			get { return IsImport && US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override void CalculateLinePriceFromInvoiceQuantityChanges()
		{
			if (!IsSettingLinePrice)
			{
				if (JI_LinePrice.IsEmpty)
				{
					if (UnitPrice > 0 && JI_InvoiceQuantity > 0)
					{
						JI_LinePrice = JI_InvoiceQuantity * UnitPrice;
					}
				}
				else
				{
					UpdateUnitPriceIfChangedFromLinePriceChanges();
				}
			}

			if (IsImport)
			{
				if (US_9802PerUnit > 0)
				{
					US_98GoodsValue = US_9802PerUnit * JI_InvoiceQuantity;
				}

				if (!IsSetting98ValueInvCurr && US_98InvCurrPerUnit > 0)
				{
					US_98ValueInvCurr = US_98InvCurrPerUnit * JI_InvoiceQuantity;
				}
			}
			else
			{
				ApportionedCharges.OfType<InvoiceLineApportionCharge>().Where(ch => ch.IsAMMV()).DeleteAll();
			}
		}

		public override ZDecimal JI_CustomsValue
		{
			get
			{
				if (jI_CustomsValueCached == null)
				{
					jI_CustomsValueCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal value = ZDecimal.Zero;

						if (IsSetXLine && !IsRecon)
						{
							foreach (JobComInvoiceLine childLine in ChildLines)
							{
								if (childLine.IsVParentLine)
								{
									foreach (var vChild in childLine.ChildLines)
									{
										value += vChild.JI_CustomsValue;
									}
								}
								else if (childLine.IsSetVLine)
								{
									value += childLine.JI_CustomsValue;
								}
							}
						}
						else
						{
							value = JI_Calc_FOB_InLocalCurrency + TotalOriginalGoodsValueInUSD;
						}

						return value;
					});
				}
				return jI_CustomsValueCached.Value;
			}
		}
		CachedProperty<ZDecimal> jI_CustomsValueCached;

		public ImportInvoiceLineHelper ImportHelper => importHelper ?? (importHelper = new ImportInvoiceLineHelper(this));
		ImportInvoiceLineHelper importHelper;
		public ZDecimal TotalOriginalGoodsValueInUSD
		{
			get
			{
				if (totalOriginalGoodsValueInUSDCached == null)
				{
					totalOriginalGoodsValueInUSDCached = new CachedProperty<ZDecimal>(Factory, ImportHelper.GetTotalOriginalGoodsValueInUSD);
				}
				return totalOriginalGoodsValueInUSDCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalOriginalGoodsValueInUSDCached;

		public override ZDecimal JI_InvoiceQuantity
		{
			get { return base.JI_InvoiceQuantity; }
			set
			{
				ZDecimal oldValue = JI_InvoiceQuantity;
				base.JI_InvoiceQuantity = value;
				if (!IsCopying && oldValue != JI_InvoiceQuantity)
				{
					using (GetNewLinePriceCalculationFieldSettingSupporter(Customs.Business.LinePriceCalculationFieldSettingType.Quantity))
					{
						foreach (JobComInvoiceLine invoiceLine in ProductRelatedLines)
						{
							invoiceLine.JI_InvoiceQuantity = JI_InvoiceQuantity;
						}

						if (!JI_PartNo_CanBeSetByCustomer)
						{
							if (!US_GrossWeight.IsEmpty)
							{
								JI_Weight = US_GrossWeight * JI_InvoiceQuantity;
							}

							if (!US_WeightNET.IsEmpty)
							{
								JI_NetWeight = US_WeightNET * JI_InvoiceQuantity;
							}
						}
					}

					UpdateBondedWhsRelatedData();
					UpdateFDALineRelateData();
					UpdateACEFDALineRelateData();
					if (IsImport)
					{
						CalculateAMMVCharge();
					}
				}
			}
		}

		void UpdateFDALineRelateData()
		{
			if (FDAs.Count == 1 && JI_InvoiceQuantity != 0 && Part != null)
			{
				FDAs[0].SetFDAQuantityDefaultsByInvoice();
			}
		}

		void UpdateACEFDALineRelateData()
		{
			if (ACE_FDALines.Count == 1 && JI_InvoiceQuantity != 0 && Part != null)
			{
				ACE_FDALines[0].SetFDAQuantityDefaultsByInvoice();
			}
		}

		void UpdateBondedWhsRelatedData(bool shouldDefaultBondedWhsQuantity = true)
		{
			if (!updateBondedWhsRelatedDataInProgress)
			{
				try
				{
					updateBondedWhsRelatedDataInProgress = true;
					if (shouldDefaultBondedWhsQuantity && JI_PartNo_CanBeSetByCustomer && !JI_InvoiceUQ.IsEmpty && !ABIUnitOfMeasureList.IsPackageType(JI_InvoiceUQ))
					{
						JI_BondedWhsQuantity = JI_InvoiceQuantity;
					}
					var declaration = Declaration;
					if (declaration != null && declaration.PackableInvoiceLines.Count == 0)
					{
						declaration.WHSPacks.RemoveAndDeleteAll();
						declaration.WHSPackLines.RemoveAndDeleteAll();
					}
				}
				finally
				{
					updateBondedWhsRelatedDataInProgress = false;
				}
			}
		}
		bool updateBondedWhsRelatedDataInProgress;

		public override ZString JI_InvoiceUQ
		{
			get { return base.JI_InvoiceUQ; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					var oldValue = JI_InvoiceUQ;
					base.JI_InvoiceUQ = value;

					if (!IsCopying && oldValue != JI_InvoiceUQ)
					{
						UpdateBondedWhsRelatedData();
						foreach (JobComInvoiceLine invoiceLine in ProductRelatedLines)
						{
							invoiceLine.JI_InvoiceUQ = JI_InvoiceUQ;
						}
						UpdateFDALineRelateData();
						UpdateACEFDALineRelateData();
						FDAs.Cast<FDA>().ForEach(x => x.SetFDADefaultValueForBaseQty());
						ACE_FDALines.Cast<ACEFDA>().ForEach(x => x.SetFDADefaultValueForBaseQty());
					}
				}
			}
		}

		public override ZGuid US_JI_ParentProduct
		{
			get { return base.US_JI_ParentProduct; }
			set
			{
				ZGuid oldValue = US_JI_ParentProduct;
				var oldProductParentTariffLine = ProductParentTariffLine;
				base.US_JI_ParentProduct = value;
				if (!IsCopying && oldValue != US_JI_ParentProduct)
				{
					if (ProductParentTariffLine != null)
					{
						ProductParentTariffLine.RefreshProductRelatedLines();
						JI_BondedWhsQuantity = ZDecimal.Zero;
						US_WHSEntryLineNo = ZShort.Zero;
					}
					if (oldProductParentTariffLine != null)
					{
						oldProductParentTariffLine.RefreshProductRelatedLines();
					}
					JI_BondedWhsQuantity = ZDecimal.Zero;
					US_WHSEntryLineNo = ZShort.Zero;
				}
			}
		}

		public override ZGuid JI_JZ
		{
			get { return base.JI_JZ; }
			set
			{
				JobComInvoiceHeader oldHeader = InvoiceHeader;
				bool hasChanged = JI_JZ != value;
				var productLines = ProductRelatedLines.ToList();

				if (!IsCopying && hasChanged)
				{
					ResetReconOriginLineNo();
				}
				base.JI_JZ = value;
				if (JI_JZ.IsValid && InvoiceHeader != null)
				{
					if (base.JI_OA_ManufacturerAddress.IsEmpty)
					{
						DefaultCountryOfOriginFromOrganisationDetails(ManufacturerAddress);
					}
				}

				if (!IsCopying && hasChanged)
				{
					foreach (var childLine in ChildLines)
					{
						if (childLine.JI_ParentID == PK)
						{
							childLine.JI_JZ = JI_JZ;
						}
					}

					foreach (var productLine in productLines)
					{
						if (productLine.US_JI_ParentProduct == PK)
						{
							productLine.JI_JZ = JI_JZ;
						}
					}

					var declaration = Declaration;

					var invoiceHeader = InvoiceHeader;
					if (invoiceHeader != null)
					{
						if (declaration == null || !declaration.IsACECargoCertificationMode)
						{
							AIILines.UpdateLineGroupingDetails();
							LineGroupingRanges.MarkAsNeedingValidation();
						}
						invoiceHeader.MarkAsNeedingValidation();
					}
					MarkJobDeclarationAsNeedingValidation();

					if (oldHeader != null && (invoiceHeader == null || oldHeader.JZ_JE != invoiceHeader.JZ_JE))
					{
						DeleteWHSPackLines();
					}

					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		void MarkJobDeclarationAsNeedingValidation()
		{
			var declaration = Declaration;
			if (declaration != null && !declaration.IsMarkingAsNeedingValidationSuspended)
			{
				declaration.MarkAsNeedingValidation();
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Tariffs))]
		public override ZString JI_FormattedTariff
		{
			get { return base.JI_FormattedTariff; }
			set { base.JI_FormattedTariff = value; }
		}

		public override ZString US_SupTariff
		{
			get { return base.US_SupTariff; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoUSAddInfo.Schema.US_SupTariff))
				{
					ZString valueToSet = TariffFormatter.Format(value);
					bool hasChanges = valueToSet != US_SupTariff;
					if (!IsCopying && hasChanges)
					{
						PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
					}

					CalculateAndCachePGARequirementValues();
					base.US_SupTariff = valueToSet;

					if (hasChanges && !IsCopying)
					{
						if (fOGARequirementCalculator != null)
						{
							fOGARequirementCalculator.Initialise();
						}

						USCTariff tariff = ImportSupTariff;

						US_SupUQ1 = tariff != null ? tariff.UE_Unit1 : ZString.Empty;
						US_SupUQ2 = tariff != null ? tariff.UE_Unit2 : ZString.Empty;
						US_SupUQ3 = tariff != null ? tariff.UE_Unit3 : ZString.Empty;

						SupTariffMarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_SupTariff);

						DefaultOGAIndicatorsWhenTariffChanges(false);
						DefaultTextileCategoryNumberFromTariff();

						DefaultFDADateIfRequired();

						SetSupTariffInChildLine(US_SupTariff, Schema.US_SupTariff);

						CalcMiscLicenseTypeLabelInfo.RefreshBinding();
						AIILines.MarkAsNeedingValidation();
						RefreshInvoiceLinesWithSpecificColumnsChanged();

						if (Declaration != null)
						{
							Declaration.RefreshHas9802Tariff();
						}

						if (!CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(US_SupTariff) && !CustomsValueDeciderForInvoiceLine.IsTariffValidFor98GoodsValue(US_SupAdditionalTariff1))
						{
							US_98GoodsValue = ZDecimal.Zero;
							US_98ValueInvCurr = ZDecimal.Zero;
							ClearAIILineDetail(AIILine.Schema.US_98InvCurrPerUnit);
						}
						DefaultUS_LicenseTypeForFTZ();
					}
				}
			}
		}

		void CalculateAndCachePGARequirementValues()
		{
			shouldDefaultFDAToBeDeclaredCached = ShouldDefaultFDAToBeDeclared;
			requireFSISCached = PGARequirementIndicator.RequireFSIS;
			requireDOTCached = PGARequirementIndicator.RequireDOT;
			requireODSCached = PGARequirementIndicator.RequireODS;
			requireVNECached = PGARequirementIndicator.RequireVNE;
			requirePSTCached = PGARequirementIndicator.RequirePST;
			requireHFCCached = PGARequirementIndicator.RequireHFC;
			requireNMFS370Cached = PGARequirementIndicator.RequireNMFS370;
			requireNMFSCOACached = PGARequirementIndicator.RequireNMFSCOA;
			requireNMFSAMRCached = PGARequirementIndicator.RequireNMFSAMR;
			requireNMFSHMSCached = PGARequirementIndicator.RequireNMFSHMS;
			requireNMFSSIMCached = PGARequirementIndicator.RequireNMFSSIM;
			requireAPHISCached = PGARequirementIndicator.RequireAPHIS;
			requireFWSCached = PGARequirementIndicator.RequireFWS;
			requireOMCCached = PGARequirementIndicator.RequireOMC;
			requireTSCACached = PGARequirementIndicator.RequireTSCA;
			requireNHTSACached = PGARequirementIndicator.RequireNHTSA;
			requireAMSEGGCached = PGARequirementIndicator.RequireAMSEGG;
			requireAMSORDCached = PGARequirementIndicator.RequireAMSORD;
			requireAMSPNTCached = PGARequirementIndicator.RequireAMSPNT;
			requireNOPCached = PGARequirementIndicator.RequireNOP;
			requireTTBCached = PGARequirementIndicator.RequireTTB;
			requireCPSCCached = PGARequirementIndicator.RequireCPSC;
			requireACE_LaceyDataCached = PGARequirementIndicator.RequireACE_LaceyData;
		}
		bool shouldDefaultFDAToBeDeclaredCached;
		bool requireFSISCached;
		bool requireDOTCached;
		bool requireODSCached;
		bool requireVNECached;
		bool requirePSTCached;
		bool requireHFCCached;
		bool requireNMFS370Cached;
		bool requireNMFSCOACached;
		bool requireNMFSAMRCached;
		bool requireNMFSHMSCached;
		bool requireNMFSSIMCached;
		bool requireAPHISCached;
		bool requireFWSCached;
		bool requireOMCCached;
		bool requireTSCACached;
		bool requireNHTSACached;
		bool requireAMSEGGCached;
		bool requireAMSORDCached;
		bool requireAMSPNTCached;
		bool requireNOPCached;
		bool requireTTBCached;
		bool requireCPSCCached;
		bool requireACE_LaceyDataCached;

		bool ShouldCopySupTariffFromParentTariffLine(ZString supTariff)
		{
			var parentLine = ParentTariffLine ?? this;
			return !this.IsCombinedLine() && !parentLine.IsDerivedSetsPrentLine() && !(CalculateDutyForSetsHelper.IsSTNTariff(ImportTariff, EffectiveDateForDutyRate) && CalculateDutyForSetsHelper.Is9903Tariff(supTariff)) && (Declaration == null || !Declaration.IsTemporaryImportationBond || !USCTariff.IsTIB(supTariff));
		}

		public bool IsDerivedSetsPrentLine()
		{
			return Chapter98Helper.Is99Tariff(US_SupTariff) && !JI_Tariff.IsEmpty && ImportTariff != null && ImportTariff.UE_DutyComputationCode == ComputationCodeList.Codes.Derived && ChildLines.Any();
		}

		void SetSupTariffInChildLine(ZString supTariff, string fieldName)
		{
			if (ShouldCopySupTariffFromParentTariffLine(supTariff))
			{
				ChildLines
					.Where(childLine => childLine.IsSecondaryTariffLine)
					.ForEach(childLine => childLine[fieldName] = supTariff);
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupTariffsList))]
		[MaxLength(15)]
		public ZString SupTariffFormatted
		{
			get { return TariffFormatter.DisplayFormat(US_SupTariff); }
			set
			{
				ZString oldValue = SupTariffFormatted;
				ZString formattedValue = value.ExcludeChars(" .");
				ZString newValue = formattedValue.Length > 10 ? formattedValue.SubstringSafe(0, 10) : formattedValue;
				US_SupTariff = newValue;
				SupTariffFormattedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SupTariffFormattedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.SupTariffFormatted, x => GetAddInfo().US_SupTariffInfo); }
		}

		public ZString SupTariffFormattedFieldType
		{
			get
			{
				ZString result = nameof(FieldType.TextCodeFindBox);
				if (IsACENormalInvoiceLine && ApplicableSupTariffs.Length > 0)
				{
					result = nameof(FieldType.TextDropEdit);
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupQty1", Caption = "Prov/Prog. Qty 1")]
		public override ZDecimal US_SupQty1
		{
			get { return GetEffectiveCustomsQuantity(base.US_SupQty1, US_SupUQ1, JI_CustomsUnitQty, JI_CustomsQuantity, ImportSupTariff); }
			set
			{
				ZDecimal valueToSet = value;
				ZDecimal effectiveValue = GetEffectiveCustomsQuantity(ZDecimal.Zero, US_SupUQ1, JI_CustomsUnitQty, JI_CustomsQuantity, ImportSupTariff);

				if (valueToSet == effectiveValue)
				{
					valueToSet = ZDecimal.Zero;
				}

				base.US_SupQty1 = valueToSet;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupUQ1", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		public override ZString US_SupUQ1
		{
			get { return base.US_SupUQ1; }
			set
			{
				bool hasChanged = US_SupUQ1 != value;
				base.US_SupUQ1 = value;

				if (!IsCopying && hasChanged)
				{
					US_SupQty1 = ZDecimal.Zero;
				}
			}
		}

		public bool US_SupQty1_ReadOnly
		{
			get { return !IsQuantityRequired(US_SupUQ1); }
		}

		public bool US_SupUQ1_ReadOnly
		{
			get { return true; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupQty2", Caption = "Prov/Prog. Qty 2")]
		public override ZDecimal US_SupQty2
		{
			get { return GetEffectiveCustomsQuantity(base.US_SupQty2, US_SupUQ2, JI_CustomsSecondUnitQty, JI_CustomsSecondQuantity, ImportSupTariff); }
			set
			{
				ZDecimal valueToSet = value;
				ZDecimal effectiveValue = GetEffectiveCustomsQuantity(ZDecimal.Zero, US_SupUQ2, JI_CustomsSecondUnitQty, JI_CustomsSecondQuantity, ImportSupTariff);

				if (valueToSet == effectiveValue)
				{
					valueToSet = ZDecimal.Zero;
				}

				base.US_SupQty2 = valueToSet;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupUQ2", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		public override ZString US_SupUQ2
		{
			get { return base.US_SupUQ2; }
			set
			{
				bool hasChanged = US_SupUQ2 != value;
				base.US_SupUQ2 = value;

				if (!IsCopying && hasChanged)
				{
					US_SupQty2 = ZDecimal.Zero;
				}
			}
		}
		public bool US_SupQty2_ReadOnly
		{
			get { return !IsQuantityRequired(US_SupUQ2); }
		}

		public bool US_SupUQ2_ReadOnly
		{
			get { return true; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupQty3", Caption = "Prov/Prog. Qty 3")]
		public override ZDecimal US_SupQty3
		{
			get { return GetEffectiveCustomsQuantity(base.US_SupQty3, US_SupUQ3, JI_CustomsThirdUnitQty, JI_CustomsThirdQuantity, ImportSupTariff); }
			set
			{
				ZDecimal valueToSet = value;
				ZDecimal effectiveValue = GetEffectiveCustomsQuantity(ZDecimal.Zero, US_SupUQ3, JI_CustomsThirdUnitQty, JI_CustomsThirdQuantity, ImportSupTariff);

				if (valueToSet == effectiveValue)
				{
					valueToSet = ZDecimal.Zero;
				}

				base.US_SupQty3 = valueToSet;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupUQ3", Caption = "UQ")]
		public override ZString US_SupUQ3
		{
			get { return base.US_SupUQ3; }
			set
			{
				bool hasChanged = US_SupUQ3 != value;
				base.US_SupUQ3 = value;

				if (!IsCopying && hasChanged)
				{
					US_SupQty3 = ZDecimal.Zero;
				}
			}
		}

		public bool US_SupQty3_ReadOnly
		{
			get { return !IsQuantityRequired(US_SupUQ3); }
		}

		public bool US_SupUQ3_ReadOnly
		{
			get { return true; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupGoodsValue", ShortCaption = "Prov/Prog. Value", Caption = "Prov/Prog. Goods Value", FullDescription = "If entered, the duty for Prov/Prog. Tariff will be calculated based on this value.")]
		public override ZDecimal US_SupGoodsValue
		{
			get => base.US_SupGoodsValue;
			set => base.US_SupGoodsValue = value;
		}

		internal TariffViewAsCodeDescription[] ApplicableSupTariffs => Factory.GetValue(ref applicableSupTariffsCached, () =>
		{
			var supTariffViewsList = new List<TariffViewAsCodeDescription>();

			if (IsImport && IsAluminumSmeltEffective && TariffMatchesSmeltCondition)
			{
				var isCombinedLine = this.IsCombinedLine();
				var entryTypeConditionValueForSmelt = ImportEntryType == EntryTypeList.Codes.ConsumptionFTZ && US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign ? "06" : "~06";

				foreach (var countryCode in new[] { EffectiveCountryOfOrigin, US_RN_NKPrimCtry, US_RN_NKSecCtry, US_RN_NKCastCtry }.Distinct())
				{
					if (!countryCode.IsEmpty)
					{
						var rateSelectionCriteria = new RateSelectionCriteria(this, Universal.Constants.RateTypes.Duty, UniversalReferenceConstants.RateCodes.Codes.Duty, countryCode);
						var tariffViews = USRefTariffDataLoader.GetSupTariffs(Factory, JI_Tariff, rateSelectionCriteria, new[] { TariffRuleList.Codes.AdditionalTariffs, TariffRuleList.Codes.RussianTariffs }, countryCode, IsSteelOriginFromEUN);
						foreach (var tariffView in tariffViews)
						{
							if (isCombinedLine)
							{
								if (!supTariffViewsList.Contains(tariffView))
								{
									supTariffViewsList.Add(tariffView);
								}
							}
							else
							{
								if (USRefTariffDataLoader.IsTariffMatchCondition(Factory, tariffView.Tariff, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT, UniversalReferenceConstants.TariffConditionValueTypes.Codes.Entry, entryTypeConditionValueForSmelt, countryCode, EffectiveDateForDutyRate))
								{
									return new[] { tariffView };
								}
							}
						}
					}
				}
			}

			if (supTariffViewsList.Count == 0)
			{
				supTariffViewsList.AddRange(USRefTariffDataLoader.GetSupTariffs(Factory, JI_Tariff, AllApplicableRatesSelectionCriteria, new[] { TariffRuleList.Codes.AdditionalTariffs, TariffRuleList.Codes.RussianTariffs }, EffectiveCountryOfOrigin, IsSteelOriginFromEUN));
			}

			if (supTariffViewsList.Count > 0)
			{
				var notApplicableConditions = new List<(ZString conditionClass, ZString conditionCode)>
				{
					(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT),
					(Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT)
				};

				if (supTariffViewsList.Any(tariffView =>
					notApplicableConditions.Any(c =>
						USRefTariffDataLoader.IsTariffMatchCondition(
							Factory,
							tariffView.Tariff,
							c.conditionClass,
							c.conditionCode,
							UniversalReferenceConstants.TariffConditionValueTypes.Codes.DESIG,
							TariffConditionValue.Values.Optional,
							EffectiveCountryOfOrigin,
							EffectiveDateForDutyRate))))
				{
					supTariffViewsList.Add(new TariffViewAsCodeDescription(TariffViewAsCodeDescription.NotApplicableCode, TariffViewAsCodeDescription.NotApplicableDescriptionForSection232, true));
				}
			}

			return supTariffViewsList.ToArray();
		});
		CachedProperty<TariffViewAsCodeDescription[]> applicableSupTariffsCached;

		internal bool SupTariffHasAttributeA99 => USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, US_SupTariff, EffectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs) != null;

		[MaxLength(15)]
		[ReadOnlyMember(nameof(isNotForImportSecionOfDrawbackDocument))]
		public override ZString JI_Tariff
		{
			get { return base.JI_Tariff; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_Tariff))
				{
					bool hasChanges = base.JI_Tariff != value;
					bool shouldApportionLineWeight = hasChanges && !IsCopying && InvoiceHeader != null;
					bool orginNeedToApportionNetWeight = shouldApportionLineWeight && NeedToApportionNetWeight;

					if (hasChanges)
					{
						CalculateAndCachePGARequirementValues();
						ClearCalculateException();

						if (!IsCopying)
						{
							RefreshInvoiceLinesWithSpecificColumnsChanged();
							PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
							ResetSupTariffs();
						}
					}

					base.JI_Tariff = value;

					if (hasChanges && !IsCopying)
					{
						if (IsDrawbackDeclaration)
						{
							var shouldReCalculateDrawbackData = ShouldReCalculateDrawbackData;
							using (shouldReCalculateDrawbackData ? SuspendReCalculatedDrawbackData() : null)
							{
								var data = IsAdvaloremTariffForDrawbackClaimOverriden;
								if (data.value)
								{
									RefreshUS_DRWAdValoremRateAndDutyRateDesc(data.value, data.importTariff);
								}
								else
								{
									if (US_DRWCalcDutyWithAdValoremRate)
									{
										US_DRWCalcDutyWithAdValoremRate = false;
									}

									if (!US_DRWAdValoremRate.IsEmpty)
									{
										US_DRWAdValoremRate = 0m;
									}
								}
							}

							if (shouldReCalculateDrawbackData)
							{
								Claims.DutyClaim.DefaultDutyRateDesc();
								Claims.DutyClaim.Default_99ClaimedDutyAndCalculatedAmount();
							}
						}
						else
						{
							var shouldDefault = Pivot == null || Pivot.CI_TariffNum != value;
							if (fOGARequirementCalculator != null)
							{
								fOGARequirementCalculator.Initialise();
							}
							USCTariff importTariff = null;
							if (IsExport)
							{
								DefaultExportOGAIndicatorsWhenTariffChanges();
								SetUS_IsUsedVehicle();
							}
							else
							{
								importTariff = ImportTariff;
								//Export no longer uses USCTariff table if TariffType is HTS
								TariffMarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(JI_Tariff);
								if (importTariff != null)
								{
									if (!importTariff.IsSpecificSpecificDutyRate)
									{
										US_SelectedRateType = ZString.Empty;
									}

									DefaultTextileCategoryNumberFromTariff();
									DefaultOGAIndicatorsWhenTariffChanges(shouldDefault);
									DefaultFDADateIfRequired();
									DefaultUS_CottonFeeExempt(shouldDefault);

									Factory.GetCachedValue<SecondaryTariffCreator>().AddSecondaryTariffsFromTariffRule(this);
								}
							}

							ClearADD_CVD_NAIndicators(shouldDefault);
							DefaultTaxRelatedFields(importTariff, JobComInvoiceLine.Schema.US_TaxApply, JobComInvoiceLine.Schema.US_TaxCode, JobComInvoiceLine.Schema.US_TaxRateT, JobComInvoiceLine.Schema.US_TaxRateS, JobComInvoiceLine.Schema.US_TaxRate, JobComInvoiceLine.Schema.US_TaxQty);

							DefaultMandatoryFees();
							CalcMiscLicenseTypeLabelInfo.RefreshBinding();
							AIILines.MarkAsNeedingValidation();
							SetDefaultValueOrCleanSanctionsIfNeeded();
						}
						SetDefaultSupTariffs();
						DefaultUS_ZoneStatusForFTZ();
						DefaultUS_LicenseTypeForFTZ();
					}

					if (shouldAddAdditionalImportTariffNumbers && IsDrawbackDeclaration)
					{
						AddAdditionalImportTariffNumbers();
					}

					if (shouldApportionLineWeight && orginNeedToApportionNetWeight != NeedToApportionNetWeight)
					{
						InvoiceHeader.ApportionLineWeight(this);
					}
				}
			}
		}

		void SetDefaultValueOrCleanSanctionsIfNeeded()
		{
			if (!TariffMatchesFishingCondition)
			{
				FishingInformations.RemoveAndDeleteAll();
			}

			if (!TariffMatchesMiningCondition)
			{
				MiningInformations.RemoveAndDeleteAll();
			}

			if (ZZCustomsFunctionality.IsSanctionsEffective && (Declaration?.US_DisclaimSanctions ?? false) && (TariffMatchesFishingCondition || TariffMatchesMiningCondition))
			{
				US_DisclaimSanctions = true;
			}
		}

		public void CopyNMFSDataToSanctions()
		{
			if (!US_DisclaimSanctions && TariffMatchesFishingCondition)
			{
				FishingInformations.RemoveAndDeleteAll();

				var vesselName = Declaration?.JE_VesselName ?? ZString.Empty;

				var defaultMethodOfHarvest = NMFSSIMPLines.FirstOrDefault(x => !x.US_SourceType.IsEmpty)?.US_SourceType ?? ZString.Empty;
				if (defaultMethodOfHarvest.IsEmpty)
				{
					defaultMethodOfHarvest = NMFSCOALines.FirstOrDefault(x => !x.US_SourceType.IsEmpty)?.US_SourceType ?? ZString.Empty;
				}

				foreach (NMFSLine nmfs in NMFSLines)
				{
					if (nmfs.HarvestingDetails.Count > 0)
					{
						foreach (NMFSHarvestingDetail harvestingDetail in nmfs.HarvestingDetails)
						{
							var countryOfHarvest = harvestingDetail?.US_HarvestedCountry ?? ZString.Empty;
							var vesselFlag = harvestingDetail?.US_VesselCountry ?? ZString.Empty;

							if (harvestingDetail.HarvestingVessles.Count > 0)
							{
								foreach (NMFSVessels vessel in harvestingDetail.HarvestingVessles)
								{
									var refVessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, vessel.US_HarvestedVessel);
									CreateNewFishingInformation(defaultMethodOfHarvest, vesselName, vesselFlag.IsEmpty ? vessel.US_HarvestedCountry : vesselFlag, countryOfHarvest, refVessel?.RV_LloydsNumber ?? ZString.Empty);
								}
							}
							else
							{
								CreateNewFishingInformation(defaultMethodOfHarvest, vesselName, vesselFlag, countryOfHarvest, ZString.Empty);
							}
						}
					}
					else
					{
						CreateNewFishingInformation(defaultMethodOfHarvest, vesselName, ZString.Empty, ZString.Empty, ZString.Empty);
					}
				}
			}
		}

		void CreateNewFishingInformation(ZString methodOfHarvest, ZString vesselName, ZString vesselFlag, ZString countryOfHarvest, ZString vesselIMO)
		{
			if (!methodOfHarvest.IsEmpty || !vesselName.IsEmpty || !vesselFlag.IsEmpty || !countryOfHarvest.IsEmpty || !vesselIMO.IsEmpty)
			{
				var fishing = FishingInformations.AddNew();
				fishing.US_MethodOfHarvest = methodOfHarvest;
				fishing.US_VesselName = vesselName;
				fishing.US_HarvestedCountry = countryOfHarvest;
				if (!vesselFlag.IsEmpty)
				{
					fishing.US_VesselCountry = vesselFlag;
				}
				if (!vesselIMO.IsEmpty)
				{
					fishing.US_VesselIMO = vesselIMO;
				}
			}
		}

		void DefaultUS_CottonFeeExempt(bool shouldDefaultCottonFeeExempt)
		{
			if (shouldDefaultCottonFeeExempt)
			{
				US_CottonFeeExempt = ZString.Empty;
			}
		}

		internal bool TariffRequiresVehicleReporting => UseHTSForExport && (ExportTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Values.Mandatory) ?? false)
					|| (ScheduleBTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.EV1, UniversalReferenceConstants.TariffAttributeTypes.Values.Mandatory) ?? false);

		internal void SetUS_IsUsedVehicle()
		{
			US_IsUsedVehicle = TariffRequiresVehicleReporting;
		}

		public ZString TariffCalculateExceptionMessage { get; set; }

		internal void SetDefaultSupTariffs()
		{
			if (IsACENormalInvoiceLine)
			{
				var applicableSupTariffs = ApplicableSupTariffs;
				if (applicableSupTariffs.Length > 0)
				{
					var effectiveDateForDutyRate = EffectiveDateForDutyRate;
					if (IsTariffWithGAEType)
					{
						foreach (var tariffView in applicableSupTariffs)
						{
							var tariff = tariffView.Tariff;
							if (USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, tariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE) != null
								|| USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, tariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232) == null)
							{
								US_SupTariff = tariff;
								return;
							}
						}
					}
					else if (applicableSupTariffs.Length == 1)
					{
						var firstTariff = applicableSupTariffs[0];
						if (firstTariff.IsMandatory)
						{
							var firstApplicableSupTariff = firstTariff.Tariff;
							if ((US_ProductExclusion.IsEmpty || !firstApplicableSupTariff.StartsWith("990380", StringComparison.OrdinalIgnoreCase))
								&& (USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, firstApplicableSupTariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs) == null
								|| USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, firstApplicableSupTariff, effectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula) == null))
							{
								US_SupTariff = firstApplicableSupTariff.Left(US_SupTariffInfo.MaxLength);
							}
						}
					}
					else if (!US_DateOfExport.IsEmpty)
					{
						var supTariff99038809 = applicableSupTariffs.FirstOrDefault(x => x.Tariff.EqualsIgnoringCase("99038809"));
						if (supTariff99038809 != null)
						{
							if (US_DateOfExport < DefaultExportDateFor99038809)
							{
								US_SupTariff = supTariff99038809.Tariff;
							}
							else if (applicableSupTariffs.Length == 2)
							{
								US_SupTariff = applicableSupTariffs.FirstOrDefault(x => !x.Tariff.EqualsIgnoringCase("99038809"))?.Tariff ?? ZString.Empty;
							}
						}
					}
				}
			}
		}

		internal readonly DateTime DefaultExportDateFor99038809 = new DateTime(2019, 05, 10);

		public bool IsTariffWithGAEType => Factory.GetValue(ref isTariffWithGAETypeCached, () => TariffHasGAEType(JI_Tariff));
		CachedProperty<bool> isTariffWithGAETypeCached;

		public bool IsSupTariffWithGAEType => Factory.GetValue(ref isSupTariffWithGAETypeCached, () => TariffHasGAEType(US_SupTariff));
		CachedProperty<bool> isSupTariffWithGAETypeCached;

		bool TariffHasGAEType(ZString tariff)
		{
			return !tariff.IsEmpty && USRefTariffDataLoader.TariffViewHasRuleWithAttribute(Factory, tariff, EffectiveDateForDutyRate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE) != null;
		}

		internal void ResetSupTariffs()
		{
			if (IsACENormalInvoiceLine)
			{
				var applicableSupTariffs = ApplicableSupTariffs;
				if (applicableSupTariffs.Length > 0 && !applicableSupTariffs.Any(x => x.Tariff == US_SupTariff))
				{
					US_SupTariff = ZString.Empty;
				}
			}
			ResetApplicableSupTariffList();
		}

		internal void ValidateSupTariffs()
		{
			if (IsACENormalInvoiceLine)
			{
				var applicableSupTariffs = ApplicableSupTariffs;
				if (applicableSupTariffs.Length == 0 && !US_SupTariff.IsEmpty)
				{
					AddInfoValidation.ValidateUS_SupTariff();
				}
			}
		}

		internal void ClearCalculateException()
		{
			TariffCalculateExceptionMessage = ZString.Empty;
		}

		void DefaultUS_ZoneStatusForFTZ()
		{
			if (IsFTZAdmission)
			{
				var applicableSupTariffs = ApplicableSupTariffs;
				US_ZoneStatus = applicableSupTariffs.Length > 0 ? (ZString)ZoneStatusList.Codes.PrivilegedForeign : ZString.Empty;
			}
		}

		internal void DefaultUS_LicenseTypeForFTZ()
		{
			if (IsFTZAdmission)
			{
				US_LicenseType = LicenseTypeForFTZ;
			}
		}

		internal ZString LicenseTypeForFTZ
		{
			get
			{
				if (licenseTypeForFTZ == null)
				{
					licenseTypeForFTZ = new CachedProperty<ZString>(Factory, () =>
					{
						var result = "";
						var tariff = ImportSupTariff is USCTariff importSupTariff && !importSupTariff.UE_PermitLicenseIndicator.IsEmpty ? importSupTariff : ImportTariff;
						var permitLicenseIndicator = tariff?.UE_PermitLicenseIndicator ?? ZString.Empty;
						if (!permitLicenseIndicator.IsEmpty)
						{
							var effectiveDateForDutyRate = EffectiveDateForDutyRate;
							var refCusCodeList = Factory.GetCachedValue("FTZLicenseTypeRefCusCodeList|" + effectiveDateForDutyRate.ToString(), () =>
							{
								return ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FTZLicenseType, effectiveDateForDutyRate);
							});
							var refCusCode = refCusCodeList.FirstOrDefault
								(
									x => x.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().FirstOrDefault
									(
										y => y.ZZE_ZXE_NKName.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ImportPermitLicenceType)
									).ZZE_Value.EqualsIgnoringCase(permitLicenseIndicator)
								);
							result = refCusCode?.ZZD_Code ?? ZString.Empty;
						}
						return result;
					});
				}
				return licenseTypeForFTZ.Value;
			}
		}
		CachedProperty<ZString> licenseTypeForFTZ;

		public bool IsACENormalInvoiceLine
		{
			get => Declaration != null && Declaration.IsACE && !IsDeleted && !IsChildLine && (!IsParentLine || CalculateDutyForSetsHelper.IsSTNTariff(ImportTariff, EffectiveDateForDutyRate));
		}

		void ResetApplicableSupTariffList()
		{
			applicableSupTariffList = null;
			applicableSupTariffsCached = null;
		}

		public CodeDescriptionPairList ApplicableSupTariffList
		{
			get
			{
				if (applicableSupTariffList == null)
				{
					applicableSupTariffList = new CodeDescriptionPairList();
					applicableSupTariffList.AddRange(ApplicableSupTariffs);
				}
				return applicableSupTariffList;
			}
		}
		CodeDescriptionPairList applicableSupTariffList;

		(bool value, USCTariff importTariff) IsAdvaloremTariffForDrawbackClaimOverriden
		{
			get
			{
				var isOverride = US_DRWClaimAmountOverriden_New;
				var importTariff = isOverride ? ImportTariff : null;
				return (isOverride && importTariff != null && importTariff.UE_DutyComputationCode == ComputationCodeList.Codes.AdValorem, importTariff);
			}
		}

		void DefaultTaxRelatedFields(USCTariff importTariff, string taxApplyFieldName, string taxCodeFieldName, string taxRateTypeFieldName, string taxRateSFieldName, string taxRateFieldName, string taxRateQuantityFieldName)
		{
			var defaultTaxApplyValue = ZString.Empty;

			if (importTariff != null)
			{
				if (importTariff.IsTaxRequired)
				{
					if (Declaration?.IsClearedInPR ?? false)
					{
						defaultTaxApplyValue = TaxApplyList.Codes.No;
					}
					else if (importTariff.IsTaxComputationUnknown)
					{
						defaultTaxApplyValue = TaxApplyList.Codes.Override;
					}
					else
					{
						defaultTaxApplyValue = TaxApplyList.Codes.Yes;
					}
				}
				else if (TaxApplicableForBulkLiquor)
				{
					defaultTaxApplyValue = TaxApplyList.Codes.No;
				}
			}

			this[taxApplyFieldName] = defaultTaxApplyValue;
			this[taxCodeFieldName] = ZString.Empty;
			this[taxRateTypeFieldName] = ZString.Empty;
			this[taxRateSFieldName] = ZString.Empty;
			this[taxRateFieldName] = ZDecimal.Zero;
			this[taxRateQuantityFieldName] = ZDecimal.Zero;
		}

		protected override bool UseUniversalTariffCore => false;

		public override bool ShouldWipeNKTaxType => false;

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.Tariffs))]
		[MaxLength(15)]
		public ZString FTZCurrentTariffFormatted
		{
			get { return TariffFormatter.DisplayFormat(US_FTZCurrentTariff); }
			set
			{
				ZString oldValue = FTZCurrentTariffFormatted;
				ZString formattedValue = value.ExcludeChars(" .");
				ZString newValue = formattedValue.Length > 10 ? formattedValue.SubstringSafe(0, 10) : formattedValue;
				US_FTZCurrentTariff = newValue;
				FTZCurrentTariffFormattedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo FTZCurrentTariffFormattedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FTZCurrentTariffFormatted, x => GetAddInfo().US_FTZCurrentTariffInfo); }
		}

		public override ZString US_FTZCurrentTariff
		{
			get { return base.US_FTZCurrentTariff; }
			set
			{
				var valueToSet = TariffFormatter.Format(value);
				var hasChanges = US_FTZCurrentTariff != valueToSet;
				base.US_FTZCurrentTariff = valueToSet;

				if (hasChanges && !IsCopying && !IsDrawbackDeclaration)
				{
					if (fOGARequirementCalculator != null)
					{
						fOGARequirementCalculator.Initialise();
					}

					DefaultOGAIndicatorsWhenTariffChanges(false);
					DefaultFDADateIfRequired();
				}
			}
		}

		public ZDate FTZCurrentDutyDate
		{
			get
			{
				var declaration = Declaration;
				return declaration.IsConsumptionFTZ ? new DutyFeeDateCalculator().GetDutyFeeDate(declaration) : ZDate.Empty;
			}
		}

		bool TaxApplicableForBulkLiquor
		{
			get
			{
				return Declaration != null && Declaration.IsBulkLiquorTaxDeferred &&
					ImportTariff != null && ImportTariff.IsTaxApplicable;
			}
		}

		void ClearADD_CVD_NAIndicators(bool shouldDefaultADD_CVD_NAIndicators)
		{
			if (shouldDefaultADD_CVD_NAIndicators)
			{
				US_CVD_NA = false;
				US_ADD_NA = false;
			}
		}

		void DefaultExportOGAIndicatorsWhenTariffChanges()
		{
			US_AMSInd = ExportPGARequirementIndicator.RequireAMS ? OGAIndicatorList.Codes.Declared : "";
			US_ATFInd = ExportPGARequirementIndicator.RequireATF ? OGAIndicatorList.Codes.Declared : "";
			US_FWSInd = ExportPGARequirementIndicator.RequireFWS ? OGAIndicatorList.Codes.Declared : "";
			US_PSTIndicator = ExportPGARequirementIndicator.RequireEPA ? OGAIndicatorList.Codes.Declared : "";
			US_NMFSHMSInd = ExportPGARequirementIndicator.RequireNMFS ? OGAIndicatorList.Codes.Declared : "";
			US_TTBInd = ExportPGARequirementIndicator.RequireTTB ? OGAIndicatorList.Codes.Declared : "";
		}

		public ExportPGARequirementIndicator ExportPGARequirementIndicator
		{
			get { return new ExportPGARequirementIndicator(Factory, () => UseScheduleB ? ScheduleBTariff : ExportTariff, EffectiveDateForDutyRate); }
		}

		void DefaultOGAIndicatorsWhenTariffChanges(bool hasRegularTariffNumberChanged)
		{
			//If the requirement is 'MayBe', then system should let users select 'D'(Declared). Otherwise
			var isShouldDefaultFDAToBeDeclared = ShouldDefaultFDAToBeDeclared;
			if (shouldDefaultFDAToBeDeclaredCached != isShouldDefaultFDAToBeDeclared)
			{
				US_FDAIndicator = isShouldDefaultFDAToBeDeclared ? OGAIndicatorList.Codes.Declared : "";
				US_FDADisclaimReason = ZString.Empty;
			}

			var isRequireFSIS = PGARequirementIndicator.RequireFSIS;
			if (requireFSISCached != isRequireFSIS)
			{
				var isFSISEffective = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.PGAFSIS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

				US_FSISInd = isFSISEffective && isRequireFSIS ? OGAIndicatorList.Codes.Declared : "";
				if (!isFSISEffective && JI_FDARequirementCode == OGARequirementList.Codes.FS4)
				{
					US_FSISInd = ZString.Empty;
				}

				US_FSISDisclaimReason = ZString.Empty;
			}

			UpdateExclusiveOGAPGAIndicators(OGAPGAIndicatorUpdateContext.TariffChange, IsACECargoCertificationMode, hasRegularTariffNumberChanged);
		}

		public PGARequirementIndicator PGARequirementIndicator
		{
			get { return new PGARequirementIndicator(() => ImportTariffForPGA, () => ImportSupTariff, () => EffectiveDateForDutyRate, (x) => IsPGAIndicatorAllowedToBeDefaulted(x), () => US_UC_NKCountryOfOrigin); }
		}

		enum OGAPGAIndicatorUpdateContext { TariffChange, CertificationModeChange, MessagingStart }

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void UpdateExclusiveOGAPGAIndicators(OGAPGAIndicatorUpdateContext updateContext, bool isACECargoCertificationMode, bool hasRegularTariffNumberChanged)
		{
			var requireDOT = PGARequirementIndicator.RequireDOT;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_DOTIndicatorInfo, null, requireDOTCached != requireDOT, updateContext, requireDOT, !isACECargoCertificationMode);

			var requireODS = PGARequirementIndicator.RequireODS;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_ODSIndInfo, US_ODSDisclaimReasonInfo, requireODSCached != requireODS, updateContext, requireODS, isACECargoCertificationMode);

			var requireVNE = PGARequirementIndicator.RequireVNE;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_VNEIndInfo, US_VNEDisclaimReasonInfo, requireVNECached != requireVNE, updateContext, requireVNE, isACECargoCertificationMode);

			var requirePST = PGARequirementIndicator.RequirePST;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_PSTIndicatorInfo, US_PSTDisclaimReasonInfo, requirePSTCached != requirePST, updateContext, requirePST, isACECargoCertificationMode);

			var requireHFC = PGARequirementIndicator.RequireHFC;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_HFCIndInfo, US_HFCDisclaimReasonInfo, requireHFCCached != requireHFC, updateContext, requireHFC, isACECargoCertificationMode);

			UpdateOGAPGAIndicatorAndDisclaimReason(US_ATFIndInfo, null, hasRegularTariffNumberChanged, updateContext, false, isACECargoCertificationMode);

			var requireNMFS370 = PGARequirementIndicator.RequireNMFS370;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_NMFS370IndInfo, US_NMFS370DisclaimReasonInfo, requireNMFS370Cached != requireNMFS370, updateContext, requireNMFS370, isACECargoCertificationMode);

			var requireNMFSCOA = PGARequirementIndicator.RequireNMFSCOA;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_NMFSCOAIndInfo, null, requireNMFSCOACached != requireNMFSCOA, updateContext, requireNMFSCOA, isACECargoCertificationMode);

			var requireNMFSAMR = PGARequirementIndicator.RequireNMFSAMR;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_NMFSAMRIndInfo, US_NMFSAMRDisclaimReasonInfo, requireNMFSAMRCached != requireNMFSAMR, updateContext, requireNMFSAMR, isACECargoCertificationMode);

			var requireNMFSHMS = PGARequirementIndicator.RequireNMFSHMS;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_NMFSHMSIndInfo, US_NMFSHMSDisclaimReasonInfo, requireNMFSHMSCached != requireNMFSHMS, updateContext, requireNMFSHMS, isACECargoCertificationMode);

			var requireNMFSSIM = PGARequirementIndicator.RequireNMFSSIM;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_NMFSSIMPIndInfo, null, requireNMFSSIMCached != requireNMFSSIM, updateContext, requireNMFSSIM, isACECargoCertificationMode);

			var requireAPHIS = PGARequirementIndicator.RequireAPHIS;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_APHISIndInfo, US_APHISDisclaimReasonInfo, requireAPHISCached != requireAPHIS, updateContext, requireAPHIS, isACECargoCertificationMode);

			var requireFWS = PGARequirementIndicator.RequireFWS;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_FWSIndInfo, US_FWSDisclaimReasonInfo, requireFWSCached != requireFWS, updateContext, ZZCustomsFunctionality.IsFWSEffective && requireFWS, isACECargoCertificationMode);

			var requireOMC = PGARequirementIndicator.RequireOMC;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_OMCIndInfo, US_OMCDisclaimReasonInfo, requireOMCCached != requireOMC, updateContext, ZZCustomsFunctionality.IsOMCEffective && requireOMC, isACECargoCertificationMode);

			var requireTSCA = PGARequirementIndicator.RequireTSCA;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_TSCAIndInfo, US_TSCADisclaimReasonInfo, requireTSCACached != requireTSCA, updateContext, requireTSCA, isACECargoCertificationMode);

			var requireNHTSA = PGARequirementIndicator.RequireNHTSA;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_NHTSAIndicatorInfo, US_NHTDisclaimReasonInfo, requireNHTSACached != requireNHTSA, updateContext, requireNHTSA, isACECargoCertificationMode);

			var requireAMSEGG = PGARequirementIndicator.RequireAMSEGG;
			var requireAMSORD = PGARequirementIndicator.RequireAMSORD;
			var requireAMSPNT = PGARequirementIndicator.RequireAMSPNT;
			var hasAMSRequirementChanged = requireAMSEGGCached != requireAMSEGG || requireAMSORDCached != requireAMSORD || requireAMSPNTCached != requireAMSPNT;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_AMSIndInfo, US_AMSDisclaimReasonInfo, hasAMSRequirementChanged, updateContext, IsAMSEGGEffective && requireAMSEGG || requireAMSORD || IsAMSPNTEffective && requireAMSPNT, isACECargoCertificationMode);

			var requireNOP = PGARequirementIndicator.RequireNOP;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_NOPIndInfo, US_NOPDisclaimReasonInfo, requireNOPCached != requireNOP, updateContext, IsAMSNOPEffective && requireNOP, isACECargoCertificationMode);

			var requireTTB = PGARequirementIndicator.RequireTTB;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_TTBIndInfo, US_TTBDisclaimReasonInfo, requireTTBCached != requireTTB, updateContext, requireTTB, isACECargoCertificationMode);

			var requireCPSC = PGARequirementIndicator.RequireCPSC;
			UpdateOGAPGAIndicatorAndDisclaimReason(US_CPSCIndInfo, US_CPSCDisclaimReasonInfo, requireCPSCCached != requireCPSC, updateContext, ZZCustomsFunctionality.IsCPSCEffective && requireCPSC, isACECargoCertificationMode);

			UpdateOGAPGAIndicatorAndDisclaimReason(US_DEAIndInfo, US_DEADisclaimReasonInfo, hasRegularTariffNumberChanged, updateContext, false, isACECargoCertificationMode);

			if (updateContext == OGAPGAIndicatorUpdateContext.TariffChange)
			{
				if (hasRegularTariffNumberChanged)
				{
					US_LaceyIndicator = PGARequirementIndicator.RequireACE_LaceyData && isACECargoCertificationMode ? OGAIndicatorList.Codes.Declared : "";
					US_LaceyDisclaimReason = ZString.Empty;
				}
			}
			else
			{
				US_LaceyIndicator = !isACECargoCertificationMode ? ZString.Empty : US_LaceyIndicator;
				US_LaceyDisclaimReason = !isACECargoCertificationMode ? ZString.Empty : US_LaceyDisclaimReason;
			}
		}

		void UpdateOGAPGAIndicatorAndDisclaimReason(ZPropertyInfo indicatorInfo, ZPropertyInfo disclaimReasonInfo, bool hasPGARequirementChanged, OGAPGAIndicatorUpdateContext updateContext, bool isIndicatorRequired, bool isRelevant)
		{
			if (hasPGARequirementChanged)
			{
				if (indicatorInfo != null)
				{
					indicatorInfo.Value = GetDefaultIndicator(updateContext, isIndicatorRequired, isRelevant, (ZString)indicatorInfo.Value);
				}

				if (disclaimReasonInfo != null)
				{
					disclaimReasonInfo.Value = GetDefaultIndicator(updateContext, false, isRelevant, (ZString)disclaimReasonInfo.Value);
				}
			}
		}

		static ZString GetDefaultIndicator(OGAPGAIndicatorUpdateContext updateContext, bool isRequired, bool isRelevant, string currentValue)
		{
			if (updateContext == OGAPGAIndicatorUpdateContext.TariffChange)
			{
				return isRequired && isRelevant ? OGAIndicatorList.Codes.Declared : "";
			}
			else if (updateContext == OGAPGAIndicatorUpdateContext.MessagingStart)
			{
				return isRelevant ? currentValue : "";
			}
			else
			{
				return isRelevant ? (isRequired && string.IsNullOrEmpty(currentValue) ? OGAIndicatorList.Codes.Declared : currentValue) : "";
			}
		}

		internal void UpdateOGAPGADetailsOnMessaging(USLinkedToDeclarationData linkedToDeclarationData)
		{
			UpdateOGAPGADetailsCore(linkedToDeclarationData, OGAPGAIndicatorUpdateContext.MessagingStart);
		}

		internal void UpdateOGAPGADetailsWhenCertificationModeChanges(USLinkedToDeclarationData linkedToDeclarationData)
		{
			UpdateOGAPGADetailsCore(linkedToDeclarationData, OGAPGAIndicatorUpdateContext.CertificationModeChange);
		}

		void UpdateOGAPGADetailsCore(USLinkedToDeclarationData linkedToDeclarationData, OGAPGAIndicatorUpdateContext updateContext)
		{
			UpdateExclusiveOGAPGAIndicators(updateContext, linkedToDeclarationData.IsACECargoCertificationMode, false);
			if (linkedToDeclarationData.IsACECargoCertificationMode)
			{
				//FCC is both OGA and PGA
				DeleteAllAndRefreshBinding(DOTs);
			}
			else
			{
				//FSIS data could be entered on ACS certification mode or ACS jobs.
				US_FDADisclaimReason = ZString.Empty;
				DeleteAllAndRefreshBinding(VehicleLines);
				DeleteAllAndRefreshBinding(AMSLines);
				DeleteAllAndRefreshBinding(ATFLines);
				if (!linkedToDeclarationData.ACECargoReleaseType.IsEmpty)
				{
					DeleteAllAndRefreshBinding(LaceyActLines);
				}
				DeleteAllAndRefreshBinding(PSTLines);
				DeleteAllAndRefreshBinding(USHFCHeaders);
				DeleteAllAndRefreshBinding(NHTSALines);
				DeleteAllAndRefreshBinding(APHISHeaders);
				DeleteAllAndRefreshBinding(FWSHeaders);
				DeleteAllAndRefreshBinding(NMFSLines);
				DeleteAllAndRefreshBinding(OMCHeaders);
				DeleteAllAndRefreshBinding(TTBLines);
				DeleteAllAndRefreshBinding(CPSCHeaders);
				DeleteAllAndRefreshBinding(DEAHeaders);
				RemoveDDTCFields();
				DeleteAllAndRefreshBinding(FWSHeaders);
				DeleteAllAndRefreshBinding(NMFSLines);
			}

			if (linkedToDeclarationData.CanHavePGAFDA)
			{
				DeleteAllAndRefreshBinding(FDAs);
			}
			else
			{
				DeleteAllAndRefreshBinding(ACE_FDALines);
			}
			foreach (PGA pga in LaceyActLines)
			{
				if (!linkedToDeclarationData.IsACECargoCertificationMode)
				{
					pga.Licenses.RemoveAndDeleteAll();
					pga.US_UnknownBreakdown = false;
				}

				pga.TransformData(linkedToDeclarationData.IsACECargoCertificationMode ? JobApplicationCodeList.Codes.ACE : JobApplicationCodeList.Codes.ACS);
			}
		}
		void RemoveDDTCFields()
		{
			US_DDTCExemptionCode = ZString.Empty;
			US_DDTCLicenseNo = ZString.Empty;
			US_DDTCLicenseType = ZString.Empty;
			US_DDTCRegistrationNo = ZString.Empty;
		}

		void DeleteAllAndRefreshBinding(BusinessObjectCollection collection)
		{
			collection.RemoveAndDeleteAll();
			collection.RefreshBinding();
		}

		public override ZString US_APHISInd
		{
			get { return base.US_APHISInd; }
			set
			{
				var oldValue = US_APHISInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, APHISHeaders.Cast<IPGADataCorrection>());
				}
				base.US_APHISInd = value;
				if (!IsCopying && oldValue != US_APHISInd)
				{
					if (!US_APHISDisclaimReason.IsEmpty && US_APHISDisclaimReason_ReadOnly)
					{
						US_APHISDisclaimReason = ZString.Empty;
					}
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		[ReadOnlyMember(nameof(US_APHISDisclaimReason_ReadOnly))]
		public override ZString US_APHISDisclaimReason
		{
			get { return base.US_APHISDisclaimReason; }
			set
			{
				if (!IsCopying && US_APHISDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_APHISDisclaimReason = value;
			}
		}

		bool US_APHISDisclaimReason_ReadOnly
		{
			get { return US_APHISInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		public override ZString US_TTBInd
		{
			get { return base.US_TTBInd; }
			set
			{
				var oldValue = US_TTBInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, TTBLines.Cast<IPGADataCorrection>());
				}

				var shouldUpdateIndicator = true;
				if (IsExport && oldValue != value && !OGAIndicatorList.IsToBeDeclaredOrDisclaimed(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.TTB, HasTTBLines);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.US_TTBInd = value;
					if (!IsCopying && oldValue != US_TTBInd)
					{
						if (!US_TTBDisclaimReason.IsEmpty && US_TTBDisclaimReason_ReadOnly)
						{
							US_TTBDisclaimReason = ZString.Empty;
						}
						Declaration?.UpdatePGADataReplacementUpdateRequired();
						RefreshInvoiceLinesWithPGAIndicators();
						if (IsExport && !OGAIndicatorList.IsToBeDeclaredOrDisclaimed(US_TTBInd))
						{
							TTBLines.RemoveAndDeleteAll();
						}
					}
				}
			}
		}

		[ReadOnlyMember(nameof(US_TTBDisclaimReason_ReadOnly))]
		public override ZString US_TTBDisclaimReason
		{
			get { return base.US_TTBDisclaimReason; }
			set
			{
				if (!IsCopying && US_TTBDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_TTBDisclaimReason = value;
			}
		}

		bool US_TTBDisclaimReason_ReadOnly
		{
			get { return US_TTBInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		[ReadOnlyMember(nameof(US_FWSInd_ReadOnly))]
		public override ZString US_FWSInd
		{
			get { return base.US_FWSInd; }
			set
			{
				var oldValue = US_FWSInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, FWSHeaders.Cast<IPGADataCorrection>());
				}

				var shouldUpdateIndicator = true;
				if (IsExport && oldValue != value && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.FWS, ExportFWS.HasExportData);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.US_FWSInd = value;
					if (!IsCopying && oldValue != US_FWSInd)
					{
						if (!US_FWSDisclaimReason.IsEmpty && US_FWSDisclaimReason_ReadOnly)
						{
							US_FWSDisclaimReason = ZString.Empty;
						}
						Declaration?.UpdatePGADataReplacementUpdateRequired();
						RefreshInvoiceLinesWithPGAIndicators();
						if (IsExport && !OGAIndicatorList.IsToBeDeclared(US_FWSInd))
						{
							FWSHeaders.RemoveAndDeleteAll();
							fExportFWS = null;
						}
					}
				}
			}
		}

		bool US_FWSInd_ReadOnly => IsExport && US_FWSInd.IsEmpty && !ZZCustomsFunctionality.IsEnableFWSEffective;

		[ReadOnlyMember(nameof(US_FWSDisclaimReason_ReadOnly))]
		public override ZString US_FWSDisclaimReason
		{
			get { return base.US_FWSDisclaimReason; }
			set
			{
				if (!IsCopying && US_FWSDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FWSDisclaimReason = value;
			}
		}

		bool US_FWSDisclaimReason_ReadOnly
		{
			get { return US_FWSInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_OGAIndicatorWithoutDisclaimerList))]
		public override ZString US_ATFInd
		{
			get { return base.US_ATFInd; }
			set
			{
				var oldValue = US_ATFInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, ATFLines.Cast<IPGADataCorrection>());
				}

				var shouldUpdateIndicator = true;
				if (IsExport && oldValue != value && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.ATF, ExportATF.HasExportData);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.US_ATFInd = value;
					if (!IsCopying && oldValue != US_ATFInd)
					{
						Declaration?.UpdatePGADataReplacementUpdateRequired();
						RefreshInvoiceLinesWithPGAIndicators();
						if (IsExport)
						{
							if (!IsATFDeclared)
							{
								ATFLines.RemoveAndDeleteAll();
								fExportATF = null;
							}
						}
						this.DefaultFDADateIfRequired();
					}
				}
			}
		}

		public Func<ZString, ZBool, ZBool> OnExportPGAIndicatorChangedEvent;

		public override ZString US_ODSInd
		{
			get { return base.US_ODSInd; }
			set
			{
				var oldValue = US_ODSInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, new IPGADataCorrection[] { ODSDataCorrection });
				}
				base.US_ODSInd = value;
				if (!IsCopying && oldValue != US_ODSInd)
				{
					ClearDisclaimReason(value, US_ODSDisclaimReasonInfo);
					SetDefaultValueOrClearTSCAODSDataIfNeeded();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		[ReadOnlyMember(nameof(US_ODSDisclaimReason_ReadOnly))]
		public override ZString US_ODSDisclaimReason
		{
			get { return base.US_ODSDisclaimReason; }
			set
			{
				var oldValue = US_ODSDisclaimReason;
				if (!IsCopying && oldValue != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_ODSDisclaimReason = value;
			}
		}

		public bool IsODSIndBeDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_ODSInd); }
		}

		public override ZString US_VNEInd
		{
			get { return base.US_VNEInd; }
			set
			{
				var oldValue = US_VNEInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, VehicleLines.Cast<IPGADataCorrection>());
				}
				base.US_VNEInd = value;
				if (!IsCopying && oldValue != US_VNEInd)
				{
					ClearDisclaimReason(value, US_VNEDisclaimReasonInfo);
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public CodeDescriptionPairList WithoutDisclaimList => OGAIndicatorList.GetWithoutDisclaim(Factory);

		void ClearDisclaimReason(ZString value, ZPropertyInfo info)
		{
			if (value != OGAIndicatorList.Codes.Disclaimed && info.Value != null && !info.Value.IsEmpty)
			{
				info.Value = ZString.Empty;
			}
		}

		public override ZString US_FSISInd
		{
			get { return base.US_FSISInd; }
			set
			{
				var oldValue = US_FSISInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, FSISLines.Cast<IPGADataCorrection>());
				}
				base.US_FSISInd = value;
				if (!IsCopying && oldValue != value)
				{
					ClearDisclaimReason(value, US_FSISDisclaimReasonInfo);
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_PSTIndicator
		{
			get { return base.US_PSTIndicator; }
			set
			{
				var oldValue = US_PSTIndicator;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, PSTLines.Cast<IPGADataCorrection>());
				}

				var shouldUpdateIndicator = true;
				if (IsExport && oldValue != value && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						var hasEPAData = !US_EPAConsentNumber.IsEmpty || !US_HazWasteTrackingNo.IsEmpty || !US_EPANetQty.IsEmpty || !US_EPANetQtyUQ.IsEmpty;
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.EPA, hasEPAData);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.US_PSTIndicator = value;
					if (!IsCopying && oldValue != US_PSTIndicator)
					{
						ClearDisclaimReason(value, US_PSTDisclaimReasonInfo);
						Declaration?.UpdatePGADataReplacementUpdateRequired();
						RefreshInvoiceLinesWithPGAIndicators();
						if (IsExport && !OGAIndicatorList.IsToBeDeclared(US_PSTIndicator))
						{
							US_EPAConsentNumber = ZString.Empty;
							US_HazWasteTrackingNo = ZString.Empty;
							US_EPANetQty = ZDecimal.Zero;
							US_EPANetQtyUQ = ZString.Empty;
						}
					}

					if (!OGAIndicatorList.IsToBeDisclaimed(US_PSTIndicator))
					{
						base.US_PSTDisclaimProgram = ZString.Empty;
					}
				}
			}
		}

		public override ZString US_HFCInd
		{
			get { return base.US_HFCInd; }
			set
			{
				var oldValue = US_HFCInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, USHFCHeaders.Cast<IPGADataCorrection>());
				}
				base.US_HFCInd = value;
				if (!IsCopying && oldValue != US_HFCInd)
				{
					ClearDisclaimReason(value, US_HFCDisclaimReasonInfo);
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_NHTSAIndicator
		{
			get { return base.US_NHTSAIndicator; }
			set
			{
				var oldValue = US_NHTSAIndicator;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, NHTSALines.Cast<IPGADataCorrection>());
				}
				base.US_NHTSAIndicator = value;
				if (!IsCopying && oldValue != US_NHTSAIndicator)
				{
					ClearDisclaimReason(value, US_NHTDisclaimReasonInfo);
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_OMCInd
		{
			get { return base.US_OMCInd; }
			set
			{
				var oldValue = US_OMCInd;
				var shouldUpdate = oldValue != value && !IsCopying;

				if (shouldUpdate)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, OMCHeaders.Cast<IPGADataCorrection>());
				}
				base.US_OMCInd = value;

				if (shouldUpdate)
				{
					if (US_OMCInd != OGAIndicatorList.Codes.Disclaimed && !US_OMCDisclaimReason.IsEmpty)
					{
						US_OMCDisclaimReason = ZString.Empty;
					}

					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}

				if (OGAIndicatorList.IsToBeDisclaimed(US_OMCInd))
				{
					US_OMCDisclaimReason = PGADisclaimReasonList.Codes.A;
				}
			}
		}

		[ReadOnlyMember(nameof(US_OMCDisclaimReason_ReadOnly))]
		public override ZString US_OMCDisclaimReason
		{
			get { return base.US_OMCDisclaimReason; }
			set
			{
				if (!IsCopying && US_OMCDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_OMCDisclaimReason = value;
			}
		}

		public override ZString US_NMFS370Ind
		{
			get { return base.US_NMFS370Ind; }
			set
			{
				var oldValue = US_NMFS370Ind;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, NMFS370Lines.Cast<IPGADataCorrection>());
				}
				base.US_NMFS370Ind = value;
				if (!IsCopying && oldValue != US_NMFS370Ind)
				{
					if (US_NMFS370Ind != OGAIndicatorList.Codes.Disclaimed && !US_NMFS370DisclaimReason.IsEmpty)
					{
						US_NMFS370DisclaimReason = ZString.Empty;
					}
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_NMFSCOAInd
		{
			get { return base.US_NMFSCOAInd; }
			set
			{
				var oldValue = US_NMFSCOAInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, NMFSCOALines.Cast<IPGADataCorrection>());
				}
				base.US_NMFSCOAInd = value;
				if (!IsCopying && oldValue != US_NMFS370Ind)
				{
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_NMFSSIMPInd
		{
			get { return base.US_NMFSSIMPInd; }
			set
			{
				var oldValue = US_NMFSSIMPInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, NMFSSIMPLines.Cast<IPGADataCorrection>());
				}
				base.US_NMFSSIMPInd = value;
				if (!IsCopying && oldValue != US_NMFSSIMPInd)
				{
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_NMFSAMRInd
		{
			get { return base.US_NMFSAMRInd; }
			set
			{
				var oldValue = US_NMFSAMRInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, NMFSAMRLines.Cast<IPGADataCorrection>());
				}
				base.US_NMFSAMRInd = value;
				if (!IsCopying && oldValue != US_NMFSAMRInd)
				{
					if (US_NMFSAMRInd != OGAIndicatorList.Codes.Disclaimed && !US_NMFSAMRDisclaimReason.IsEmpty)
					{
						US_NMFSAMRDisclaimReason = ZString.Empty;
					}
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public override ZString US_NMFSHMSInd
		{
			get { return base.US_NMFSHMSInd; }
			set
			{
				var oldValue = US_NMFSHMSInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, NMFSHMSLines.Cast<IPGADataCorrection>());
				}

				var shouldUpdateIndicator = true;
				if (IsExport && oldValue != value && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.NMFS, NMFSLines.Count > 0);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.US_NMFSHMSInd = value;
					if (!IsCopying && oldValue != US_NMFSHMSInd)
					{
						if (US_NMFSHMSInd != OGAIndicatorList.Codes.Disclaimed && !US_NMFSHMSDisclaimReason.IsEmpty)
						{
							US_NMFSHMSDisclaimReason = ZString.Empty;
						}
						Declaration?.UpdatePGADataReplacementUpdateRequired();
						RefreshInvoiceLinesWithPGAIndicators();
						if (IsExport && !OGAIndicatorList.IsToBeDeclared(US_NMFSHMSInd))
						{
							NMFSLines.RemoveAndDeleteAll();
						}
					}
				}
			}
		}

		public override ZString US_CPSCInd
		{
			get { return base.US_CPSCInd; }
			set
			{
				var oldValue = US_CPSCInd;
				var shouldUpdate = oldValue != value && !IsCopying;

				if (shouldUpdate)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, CPSCHeaders.Cast<IPGADataCorrection>());
				}
				base.US_CPSCInd = value;

				if (shouldUpdate)
				{
					if (US_CPSCInd != OGAIndicatorList.Codes.Disclaimed && !US_CPSCDisclaimReason.IsEmpty)
					{
						US_CPSCDisclaimReason = ZString.Empty;
					}
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		[ReadOnlyMember(nameof(US_CPSCDisclaimReason_ReadOnly))]
		public override ZString US_CPSCDisclaimReason
		{
			get { return base.US_CPSCDisclaimReason; }
			set
			{
				if (!IsCopying && US_CPSCDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
					if ((US_CPSCDisclaimReason == PGADisclaimReasonList.Codes.A && value != PGADisclaimReasonList.Codes.A) || value == PGADisclaimReasonList.Codes.A)
					{
						CPSCHeaders.DeleteAll();
						if (value == PGADisclaimReasonList.Codes.A)
						{
							CPSCHeaders.AddNew();
						}
					}
				}
				base.US_CPSCDisclaimReason = value;
			}
		}

		public override ZString US_DEAInd
		{
			get { return base.US_DEAInd; }
			set
			{
				var oldValue = US_DEAInd;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, DEAHeaders.Cast<IPGADataCorrection>());
				}

				var shouldUpdate = true;
				if (IsExport && oldValue != value && !OGAIndicatorList.IsToBeDeclared(value) && OnExportPGAIndicatorChangedEvent != null)
				{
					shouldUpdate = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.DEA, HasDEAHeaders);
				}

				if (shouldUpdate)
				{
					base.US_DEAInd = value;

					if (!IsCopying && oldValue != value)
					{
						if (US_DEAInd != OGAIndicatorList.Codes.Disclaimed)
						{
							if (!US_DEADisclaimReason.IsEmpty)
							{
								US_DEADisclaimReason = ZString.Empty;
							}
						}
						else
						{
							if (US_DEADisclaimReason.IsEmpty)
							{
								US_DEADisclaimReason = PGADisclaimReasonList.Codes.A;
							}
						}
						Declaration?.UpdatePGADataReplacementUpdateRequired();
						RefreshInvoiceLinesWithPGAIndicators();
						if (IsExport && !OGAIndicatorList.IsToBeDeclared(US_DEAInd))
						{
							DEAHeaders.RemoveAndDeleteAll();
						}

						DefaultFDADateIfRequired();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(US_DEADisclaimReason_ReadOnly))]
		public override ZString US_DEADisclaimReason
		{
			get { return base.US_DEADisclaimReason; }
			set
			{
				if (!IsCopying && US_DEADisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_DEADisclaimReason = value;
			}
		}

		public override ZString US_LaceyIndicator
		{
			get { return base.US_LaceyIndicator; }
			set
			{
				var oldValue = US_LaceyIndicator;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, LaceyActLines.Cast<IPGADataCorrection>());
				}
				base.US_LaceyIndicator = value;
				if (!IsCopying && oldValue != US_LaceyIndicator)
				{
					ClearDisclaimReason(value, US_LaceyDisclaimReasonInfo);
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		List<ZString> MandatoryFeesForTariff
		{
			get
			{
				var result = new List<ZString>();
				if (ImportTariff != null)
				{
					result.AddRange(ImportTariff.GetRequiredFeeCodes());
				}
				return result;
			}
		}

		void DefaultMandatoryFees()
		{
			List<ZString> mandatoryFees = MandatoryFeesForTariff;

			foreach (FeeCusCodeData fee in FeeCusCodes.ToArray())
			{
				if (CusFeeCodeConstants.IsRelatedToTariffNumber(fee.CY_Code) && !mandatoryFees.Contains(fee.CY_Code) && !fee.CY_IsOverridden)
				{
					fee.Delete();
				}
			}

			foreach (ZString mandatoryFee in mandatoryFees)
			{
				if (FeeCusCodes.GetFirstElementHaving(mandatoryFee) == null)
				{
					FeeCusCodes.AddNew(mandatoryFee);
				}
			}
		}

		public override ZString JI_CustomsUnitQty
		{
			get { return base.JI_CustomsUnitQty; }
			set
			{
				ZString oldValue = JI_CustomsUnitQty;
				base.JI_CustomsUnitQty = value;
				if (!IsCopying && oldValue != JI_CustomsUnitQty)
				{
					if (GetJI_CustomsQuantityReadOnly())
					{
						ClearAIILineDetail(AIILine.Schema.US_CustomsQty);
					}
				}
			}
		}

		protected override bool ClearCustomsQuantityWhenUQSet
		{
			get
			{
				return !IsDrawbackDeclaration;
			}
		}

		internal ZBool CanConvertCustomsQtyToKG
		{
			get { return Core.Constants.Weight.ContainsCode(JI_CustomsUnitQty); }
		}

		internal ZDecimal CustomsQuantityInKG
		{
			get { return new ZWeight(JI_CustomsQuantity, JI_CustomsUnitQty).InKilogramsSafe; }
		}

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return !IsDrawbackDeclaration;
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		public override ZString JI_CustomsSecondUnitQty
		{
			get { return base.JI_CustomsSecondUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_CustomsSecondUnitQty)))
				{
					ZString oldValue = JI_CustomsSecondUnitQty;
					if (!IsCopying && oldValue != value)
					{
						if (ClearCustomsQuantityWhenUQSet)
						{
							if (!JI_CustomsSecondUnitQty.IsEmpty)
							{
								JI_CustomsSecondQuantity = ZDecimal.Zero;
							}
						}
					}

					base.JI_CustomsSecondUnitQty = value;
					if (!IsCopying && oldValue != JI_CustomsSecondUnitQty)
					{
						if (!ShouldConverCustomsQuantity2)
						{
							JI_CustomsSecondQuantity = ZDecimal.Zero;
							ClearAIILineDetail(AIILine.Schema.US_SecondQty);
						}
					}
				}
			}
		}

		public override bool ShouldConverCustomsQuantity2
		{
			get { return IsQuantityRequired(JI_CustomsSecondUnitQty); }
		}

		public bool JI_CustomsSecondUnitQty_ReadOnly => true;

		[ReadOnlyMember(nameof(JI_CustomsSecondQuantity_ReadOnly))]
		public override ZDecimal JI_CustomsSecondQuantity
		{
			get { return base.JI_CustomsSecondQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_CustomsSecondQuantity)))
				{
					ZDecimal oldValue = JI_CustomsSecondQuantity;
					base.JI_CustomsSecondQuantity = value;
					if (!IsCopying && oldValue != JI_CustomsSecondQuantity)
					{
						ClearCustomsQuantityIfSameAsEffective(Schema.US_SupQty2, JI_CustomsSecondQuantity, US_SupUQ2, JI_CustomsSecondUnitQty, ImportSupTariff);

						if (IsNonLineGroupingOrOnlyOneAIILine)
						{
							AIILine aiiLine = FirstAIILine;
							if (aiiLine != null)
							{
								aiiLine.US_SecondQty = JI_CustomsSecondQuantity;
								aiiLine.US_SecondQtyInfo.RefreshBinding(oldValue);
							}
						}
					}
				}
			}
		}

		public bool JI_CustomsSecondQuantity_ReadOnly
		{
			get
			{
				return !IsQuantityRequired(JI_CustomsSecondUnitQty);
			}
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.InvoiceUQList))]
		public override ZString JI_CustomsThirdUnitQty
		{
			get { return base.JI_CustomsThirdUnitQty; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_CustomsThirdUnitQty)))
				{
					ZString oldValue = JI_CustomsThirdUnitQty;
					if (!IsCopying && oldValue != value)
					{
						if (ClearCustomsQuantityWhenUQSet)
						{
							if (!JI_CustomsThirdUnitQty.IsEmpty)
							{
								JI_CustomsThirdQuantity = ZDecimal.Zero;
							}
						}
					}

					base.JI_CustomsThirdUnitQty = value;
					if (!IsCopying && oldValue != JI_CustomsThirdUnitQty)
					{
						if (!IsQuantityRequired(JI_CustomsThirdUnitQty))
						{
							JI_CustomsThirdQuantity = ZDecimal.Zero;
							ClearAIILineDetail(AIILine.Schema.US_ThirdQty);
						}
					}
				}
			}
		}

		[ReadOnlyMember(nameof(JI_CustomsThirdQuantity_ReadOnly))]
		public override ZDecimal JI_CustomsThirdQuantity
		{
			get { return base.JI_CustomsThirdQuantity; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_CustomsThirdQuantity)))
				{
					ZDecimal oldValue = JI_CustomsThirdQuantity;
					base.JI_CustomsThirdQuantity = value;
					if (!IsCopying && oldValue != JI_CustomsThirdQuantity)
					{
						ClearCustomsQuantityIfSameAsEffective(Schema.US_SupQty3, JI_CustomsThirdQuantity, US_SupUQ3, JI_CustomsThirdUnitQty, ImportSupTariff);

						if (IsNonLineGroupingOrOnlyOneAIILine)
						{
							AIILine aiiLine = FirstAIILine;
							if (aiiLine != null)
							{
								aiiLine.US_ThirdQty = JI_CustomsThirdQuantity;
								aiiLine.US_ThirdQtyInfo.RefreshBinding(oldValue);
							}
						}
					}
				}
			}
		}

		public bool JI_CustomsThirdQuantity_ReadOnly
		{
			get { return !IsQuantityRequired(JI_CustomsThirdUnitQty); }
		}

		[ReadOnlyMember(nameof(US_SelectedRateType_ReadOnly))]
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_SelectedRateTypeList))]
		public override ZString US_SelectedRateType
		{
			get { return base.US_SelectedRateType; }
			set { base.US_SelectedRateType = value; }
		}

		public bool US_SelectedRateType_ReadOnly
		{
			get { return !(ImportTariff != null && ImportTariff.IsSpecificSpecificDutyRate); }
		}

		public override ZString US_TaxApply
		{
			get { return base.US_TaxApply; }
			set
			{
				bool hasChanges = base.US_TaxApply != value;
				base.US_TaxApply = value;

				if (hasChanges && !IsCopying)
				{
					US_TaxCode = ZString.Empty;
					if (TaxRateT_ReadOnly)
					{
						US_TaxRateT = ZString.Empty;
					}
					US_TaxRateS = ZString.Empty;
					US_TaxRate = ZDecimal.Zero;
					US_TaxQty = ZDecimal.Zero;

					DefalutCBMAReleatedFields();
				}
			}
		}

		public ZBool IsTaxApplicable => TaxApplyList.IsTaxApplicable(US_TaxApply);

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(TaxQty_ReadOnly))]
		public override ZDecimal US_TaxQty
		{
			get { return base.US_TaxQty; }
			set { base.US_TaxQty = value; }
		}

		internal bool TaxQty_ReadOnly => IsRecon && !IsTaxQtyRequired;
		internal bool IsTaxRateOverridden => US_TaxApply == TaxApplyList.Codes.Override;
		internal bool IsTaxRateApplicable => US_TaxApply == TaxApplyList.Codes.Yes;

		[ReadOnlyMember(nameof(TaxCode_ReadOnly))]
		[BusinessObjectTestExclude]
		public override ZString US_TaxCode
		{
			get => base.US_TaxCode;
			set
			{
				var oldTaxCode = US_TaxCode;
				var valueSafe = string.IsNullOrEmpty(value) ? GetDefaultTaxCode(ImportTariff) : value;

				if (oldTaxCode != valueSafe)
				{
					base.US_TaxCode = valueSafe;

					if (!IsCopying)
					{
						if (IsCBMAProductClaim)
						{
							DefalutCBMAReleatedFields();
							US_TaxRateT = ZString.Empty;
						}
						else
						{
							US_TaxRateS = ZString.Empty;
							US_TaxRateT = ZString.Empty;
							US_TaxRate = ZDecimal.Zero;
						}

						DeleteElementInFeeCusCodes(oldTaxCode, FeeCusCodes);

						if (!valueSafe.IsEmpty && IsTaxRateApplicable && FeeCusCodes.GetFirstElementHaving(valueSafe) == null)
						{
							FeeCusCodes.AddNew(valueSafe);
						}
					}
				}
			}
		}

		void DeleteElementInFeeCusCodes(ZString taxCode, IFees feeCusCodes)
		{
			if (!taxCode.IsEmpty)
			{
				IFee feeData = feeCusCodes.GetFeeFor(taxCode);

				if (feeData != null && !feeData.IsOverridden)
				{
					feeData.Delete();
				}
			}
		}

		public ZString GetDefaultTaxCode(USCTariff tariff)
		{
			var key = string.Format(CultureInfo.InvariantCulture, "USTaxCode_{0}_{1}", tariff?.PK ?? ZGuid.Empty, IsTaxApplicable);
			return Factory.GetCachedValue(key, () => IsTaxApplicable && tariff != null ? tariff.GetUniqueTaxCode() : ZString.Empty);
		}

		bool TaxCode_ReadOnly
		{
			get { return !IsTaxApplicable; }
		}

		[ReadOnlyMember(nameof(TaxRateT_ReadOnly))]
		public override ZString US_TaxRateT
		{
			get { return base.US_TaxRateT; }
			set
			{
				bool hasChanges = base.US_TaxRateT != value;

				if (hasChanges)
				{
					base.US_TaxRateT = value;

					if (!IsCopying)
					{
						UpdateTaxRateTypeInFeeCusCodes(US_TaxCode, US_TaxRateT, FeeCusCodes);
						US_TaxRateS = ZString.Empty;
					}
				}
			}
		}

		void UpdateTaxRateTypeInFeeCusCodes(ZString taxCode, ZString taxRateType, IFees feeCusCodes)
		{
			if (!taxCode.IsEmpty)
			{
				IFee feeData = feeCusCodes.GetFeeFor(taxCode);

				if (feeData == null && !taxRateType.IsEmpty)
				{
					feeData = feeCusCodes.AddNew();
					feeData.Code = taxCode;
				}

				if (feeData != null)
				{
					feeData.SelectedRateType = taxRateType;
				}
			}
		}

		public bool TaxRateT_ReadOnly
		{
			get { return IsTaxRateTypeReadOnly(ImportTariff, US_TaxCode, US_TaxApply); }
		}

		bool IsTaxRateTypeReadOnly(USCTariff importTariff, ZString taxCode, ZString taxApply)
		{
			return Factory.GetCachedValue((importTariff?.PK ?? ZGuid.Empty).ToString() + taxCode + taxApply, () =>
			{
				bool result = true;

				if (importTariff != null && !taxCode.IsEmpty)
				{
					USCTariffDutyRate dutyRate = importTariff.DutyRates.GetRateForTaxFeeClassCode(taxCode);
					result = dutyRate == null || !dutyRate.IsSpecificSpecificTaxFee || taxApply == TaxApplyList.Codes.Override;
				}

				return result;
			});
		}

		[DecimalPlaces(8)]
		[ReadOnlyMember(nameof(TaxRate_ReadOnly))]
		public override ZDecimal US_TaxRate
		{
			get
			{
				ZDecimal result = base.US_TaxRate;

				if (result.IsEmpty && !US_TaxRateS.IsEmpty)
				{
					result = AppendixBTaxRateList.GetRate(US_TaxRateS);
				}

				return result;
			}
			set { base.US_TaxRate = value; }
		}

		internal bool TaxRate_ReadOnly => IsRecon && !IsTaxRateSpecifiedManually;

		[ReadOnlyMember(nameof(TaxRateS_ReadOnly))]
		[BusinessObjectTestExclude]
		public override ZString US_TaxRateS
		{
			get => base.US_TaxRateS;
			set
			{
				var oldValue = base.US_TaxRateS;
				var valueSafe = string.IsNullOrEmpty(value) ? GetDefaultTaxRateS() : value;

				if (!settingUS_TaxRateSInProgress && oldValue != valueSafe)
				{
					try
					{
						settingUS_TaxRateSInProgress = true;
						base.US_TaxRateS = valueSafe;

						if (!IsCopying)
						{
							if (IsTaxRateSpecifiedManually)
							{
								if (IsCBMAProductClaim && AddInfoLookups.TaxRateList[US_TaxRateS] is CBMATaxRate taxRate)
								{
									US_TaxRate = taxRate.Rate;
								}
								else
								{
									US_TaxRate = ZDecimal.Zero;
								}
							}
							else
							{
								US_TaxRate = AppendixBTaxRateList.GetRate(US_TaxRateS);
								if (IsCBMAProductClaim)
								{
									US_TTBRateDesignationCode = ZString.Empty;
								}
							}

							if (IsTaxRateReduced && (Declaration?.IsACE ?? false))
							{
								US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
							}

							if (!IsTaxQtyRequired)
							{
								US_TaxQty = ZDecimal.Zero;
							}
						}
					}
					finally
					{
						settingUS_TaxRateSInProgress = false;
					}
				}
			}
		}
		bool settingUS_TaxRateSInProgress;

		public ZString GetDefaultTaxRateS()
		{
			var key = string.Format(CultureInfo.InvariantCulture, "USTaxRateS_{0}_{1}_{2}_{3}", ImportTariff?.PK ?? ZGuid.Empty, US_TaxCode, US_TaxRateT, IsTaxApplicable);
			return Factory.GetCachedValue(key, () => IsTaxApplicable ? AppendixBTaxRateList.GetNormalTaxRateString(ImportTariff, US_TaxCode, US_TaxRateT) : string.Empty);
		}

		public bool TaxRateS_ReadOnly
		{
			get { return !IsTaxApplicable; }
		}

		public ZBool IsTaxRateSpecifiedManually
		{
			get { return US_TaxRateS.EqualsIgnoringCase(AppendixBTaxRateList.Codes.Specify) || IsTaxRateReduced || IsCBMAProductClaimAndIsNotCBMA23Effective; }
		}

		public ZBool IsTaxRateReduced
		{
			get { return US_TaxRateS.EqualsIgnoringCase(AppendixBTaxRateList.CBMAEligible); }
		}

		public ZBool IsTaxRateSpecifiedInList
		{
			get { return !IsTaxRateSpecifiedManually; }
		}

		public ZBool IsTaxQtyRequired
		{
			get { return AppendixBTaxRateList.DoesUQMatchNoneOfCustomsUQs(US_TaxRateS, JI_CustomsUnitQty, JI_CustomsSecondUnitQty); }
		}

		public bool TariffMarkedForReferenceFileRequest
		{
			get { return tariffMarkedForReferenceFileRequest; }
			set
			{
				tariffMarkedForReferenceFileRequest = value;
				if (tariffMarkedForReferenceFileRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestTariffs();
					}
				}
			}
		}
		bool tariffMarkedForReferenceFileRequest;

		public bool SupTariffMarkedForReferenceFileRequest
		{
			get { return supTariffMarkedForReferenceFileRequest; }
			set
			{
				supTariffMarkedForReferenceFileRequest = value;
				if (supTariffMarkedForReferenceFileRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestTariffs();
					}
				}
			}
		}
		bool supTariffMarkedForReferenceFileRequest;

		public bool SupAdditionalTariff1MarkedForReferenceFileRequest
		{
			get { return supAdditionalTariff1MarkedForReferenceFileRequest; }
			set
			{
				supAdditionalTariff1MarkedForReferenceFileRequest = value;
				if (supAdditionalTariff1MarkedForReferenceFileRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestTariffs();
					}
				}
			}
		}
		bool supAdditionalTariff1MarkedForReferenceFileRequest;

		public bool SupAdditionalTariff2MarkedForReferenceFileRequest
		{
			get { return supAdditionalTariff2MarkedForReferenceFileRequest; }
			set
			{
				supAdditionalTariff2MarkedForReferenceFileRequest = value;
				if (supAdditionalTariff2MarkedForReferenceFileRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestTariffs();
					}
				}
			}
		}
		bool supAdditionalTariff2MarkedForReferenceFileRequest;

		public bool SupAdditionalTariff3MarkedForReferenceFileRequest
		{
			get { return supAdditionalTariff3MarkedForReferenceFileRequest; }
			set
			{
				supAdditionalTariff3MarkedForReferenceFileRequest = value;
				if (supAdditionalTariff3MarkedForReferenceFileRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestTariffs();
					}
				}
			}
		}
		bool supAdditionalTariff3MarkedForReferenceFileRequest;

		public bool SupAdditionalTariff4MarkedForReferenceFileRequest
		{
			get { return supAdditionalTariff4MarkedForReferenceFileRequest; }
			set
			{
				supAdditionalTariff4MarkedForReferenceFileRequest = value;
				if (supAdditionalTariff4MarkedForReferenceFileRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestTariffs();
					}
				}
			}
		}
		bool supAdditionalTariff4MarkedForReferenceFileRequest;

		public bool SupAdditionalTariff5MarkedForReferenceFileRequest
		{
			get { return supAdditionalTariff5MarkedForReferenceFileRequest; }
			set
			{
				supAdditionalTariff5MarkedForReferenceFileRequest = value;
				if (supAdditionalTariff5MarkedForReferenceFileRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestTariffs();
					}
				}
			}
		}
		bool supAdditionalTariff5MarkedForReferenceFileRequest;

		void DefaultDDTCArrivalDate()
		{
			US_DDTCArrivalDate = Declaration.US_FDAADTA;
		}

		void DefaultDDTCRegistrationNumber()
		{
			US_DDTCRegistrationNo = Supplier?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.DDTCRegistrationNumber) ?? ZString.Empty;
		}

		public void DefaultFDADateIfRequired()
		{
			if (!((ISupportDataImporting)this).IsImportingData && Declaration is JobDeclaration declaration && !IsImportViaBIRD)
			{
				declaration.DefaultFDADateOnDeclarationLevel();
			}
		}

		void DefaultTextileCategoryNumberFromTariff()
		{
			var importTariff = ImportSupTariff != null && !ImportSupTariff.UE_TextileCategoryNumber.IsEmpty ? ImportSupTariff : ImportTariff;

			if (importTariff != null)
			{
				if (ImportSupTariff != null && ImportSupTariff.IsRepairOrAssembly(EffectiveDateForDutyRate))
				{
					// Visa category # should not be sent with 9802 tariff therefore do not default value & clear out any value they may have defaulted from primary tariff
					US_TextileCategoryNo = ZString.Empty;
				}
				else if (!IsSetXLine)
				{
					US_TextileCategoryNo = importTariff.UE_TextileCategoryNumber;
				}
			}

			IDutyData parentDutyData = HasSupplementary ? ((IDutyData)this).ParentTariffLine : IsParentLine ? this : ParentTariffLine;
			if (IsSecondaryTariffLine || IsParentLine || HasSupplementary)
			{
				if (parentDutyData != null &&
					parentDutyData.ImportTariff != null &&
					parentDutyData.ImportTariff.Applies(TariffRuleList.Codes.HaitiTariffHope, EffectiveDateForDutyRate)
					)
				{
					JobComInvoiceLine parentLine = ParentTariffLine ?? this;
					var singleSecondaryLine = parentLine.SecondaryTariffLines.IsCountEqualTo(1) ? parentLine.SecondaryTariffLines.ElementAt(0) : (HasSupplementary ? this : null);

					if (singleSecondaryLine != null)
					{
						if (singleSecondaryLine.US_TextileCategoryNo.IsEmpty)
						{
							parentLine.US_TextileCategoryNo = "900";
						}
						else
						{
							parentLine.US_TextileCategoryNo = "";
						}
					}
				}
			}

			if (!US_TextileCategoryNo.IsEmpty && (ImportTariff != null && ImportTariff.NoCategoryNumberToBeEntered(EffectiveDateForDutyRate) ||
												parentDutyData != null && parentDutyData.ImportTariff != null && parentDutyData.ImportTariff.NoCategoryNumberToBeEntered(EffectiveDateForDutyRate)))
			{
				US_TextileCategoryNo = ZString.Empty;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceLineFetchStrategy(this);
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() => new TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(invoiceLine =>
		{
			Universal.ITariff tariff;
			if (invoiceLine.IsExport)
			{
				tariff = invoiceLine.TariffExpirationDateWinin30Days;
			}
			else
			{
				tariff = invoiceLine.ImportTariff;
			}
			return tariff;
		});

		public override ZString CustomsUQ
		{
			get
			{
				var result = ZString.Empty;
				if (IsExport)
				{
					var tariff = TariffExpirationDateWinin30Days;
					result = tariff != null ? tariff.ZZ1_ZZ8_UQ1 : result;
				}
				else
				{
					var tariff = (ITariff)ImportTariff;
					result = tariff != null ? tariff.Unit1 : result;
				}
				return result;
			}
		}

		public override ZDecimal JI_Calc_DutyAmount
		{
			get { return US_Duty + US_SupDuty + US_SupAdditionalTariff1Duty + US_SupAdditionalTariff2Duty + US_SupAdditionalTariff3Duty + US_SupAdditionalTariff4Duty + US_SupAdditionalTariff5Duty; }
		}

		public bool US_Duty_ReadOnly
		{
			get { return !US_OverrideDuty; }
		}

		public bool US_R_OrigDuty_ReadOnly
		{
			get { return !US_R_OrigOverrideDuty; }
		}

		public bool US_R_OrigSupDuty_ReadOnly
		{
			get { return !US_R_OrigOverrideSupDuty; }
		}

		public bool US_SupDuty_ReadOnly
		{
			get { return !US_OverrideSupDuty; }
		}

		public override ZBool US_OverrideDuty
		{
			get { return base.US_OverrideDuty; }
			set
			{
				ZBool oldValue = US_OverrideDuty;

				base.US_OverrideDuty = value;

				if (oldValue != US_OverrideDuty)
				{
					US_Duty = ZDecimal.Zero;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_WHSEntryLineNo", Caption = "WHS Ent./FTZ Adm Line Number", ShortCaption = "WHS/FTZ Line No.")]
		[ReadOnlyMember(nameof(WHSEntryDetailReadOnly))]
		public override ZShort US_WHSEntryLineNo
		{
			get { return base.US_WHSEntryLineNo; }
			set { base.US_WHSEntryLineNo = value; }
		}

		bool WHSEntryDetailReadOnly
		{
			get { return !JI_PartNo_CanBeSetByCustomer; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_WHSEntryNumber", Caption = "WHS Ent./FTZ Adm Number", ShortCaption = "WHS Ent./FTZ Adm No.")]
		[ReadOnlyMember(nameof(WHSEntryDetailReadOnly))]
		public override ZString US_WHSEntryNumber
		{
			get { return base.US_WHSEntryNumber; }
			set { base.US_WHSEntryNumber = value; }
		}

		public ZString BondedWhsQuantityForGUIDataFieldType
		{
			get { return WHSPackLines.Count > 0 ? nameof(FieldType.Text) : nameof(FieldType.Decimal); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|BondedWhsQuantityForGUI", Caption = "Warehouse Package Qty", ShortCaption = "WHS Pkg. Qty")]
		[ReadOnlyMember(nameof(JI_BondedWhsQuantity_ReadOnly))]
		[BusinessObjectTestExclude]
		public ZString BondedWhsQuantityForGUI
		{
			get { return WHSPackLines.Count > 0 ? SeeWHSPacksMessage : JI_BondedWhsQuantity.ToString(5); }
			set
			{
				JI_BondedWhsQuantity = ZDecimal.ParseSafe(value, ZDecimal.Zero);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBondedWhsQuantityForGUI();
				}
				BondedWhsQuantityForGUIInfo.RefreshBinding();
			}
		}
		const string SeeWHSPacksMessage = "See WHS Packs";

		public ZPropertyInfo BondedWhsQuantityForGUIInfo
		{
			get { return GetZPropertyInfo(Schema.BondedWhsQuantityForGUI); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|JI_BondedWhsQuantity", Caption = "Warehouse Package Qty", ShortCaption = "WHS Pkg. Qty")]
		[ReadOnlyMember(nameof(JI_BondedWhsQuantity_ReadOnly))]
		public override ZDecimal JI_BondedWhsQuantity
		{
			get { return base.JI_BondedWhsQuantity; }
			set
			{
				var oldValue = JI_BondedWhsQuantity;
				base.JI_BondedWhsQuantity = value;
				if (!IsCopying && oldValue != JI_BondedWhsQuantity)
				{
					DeleteWHSPackLines();
					UpdateBondedWhsRelatedData(false);
				}
			}
		}

		bool JI_BondedWhsQuantity_ReadOnly
		{
			get { return !JI_PartNo_CanBeSetByCustomer || WHSPackLines.Count > 0; }
		}

		public override ZGuid JI_OP
		{
			get { return base.JI_OP; }
			set
			{
				var oldValue = JI_OP;
				base.JI_OP = value;
				if (!IsCopying && oldValue != JI_OP)
				{
					DeleteWHSPackLines();
				}
			}
		}

		protected override void InitialiseUNDGs()
		{
			base.InitialiseUNDGs();
			UNDGs.CountChanged += new EventHandler(UNDGs_CountChanged);
		}

		void UNDGs_CountChanged(object sender, EventArgs e)
		{
			foreach (UNDGDataItem dgItem in UNDGs)
			{
				dgItem.DI_DGInfo.ValueChanged -= new EventHandler(DI_DGInfo_ValueChanged);
			}

			if (UNDGs.Count == 1)
			{
				UNDGs[0].DI_DGInfo.ValueChanged += new EventHandler(DI_DGInfo_ValueChanged);
			}
		}

		void DI_DGInfo_ValueChanged(object sender, EventArgs e)
		{
			if (UNDGs.Count > 0 && UNDGs[0].UNDGSubstance != null)
			{
				JI_HazMatCodeQualifier = HazMatQualifierList.Codes.UnitedNations;
				JI_HazMatCode = UNDGs[0].UNDGSubstance.DG_Code;
				US_HazMatDesc = UNDGs[0].UNDGSubstance.DG_PSN.Left(AddInfo.Schema.US_HazMatDescMaxLength);
			}
		}

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return !(IsDrawbackDeclaration || IsQuantityRequired(JI_CustomsUnitQty));
		}

		public override bool NeedsCustomsQuantity
		{
			get
			{
				bool result = false;

				if (IsExport)//Used for validations to add a message error in base. Import does not need the validation
				{
					var customsUQ = JI_CustomsUnitQty;
					result = !customsUQ.IsEmpty &&
						customsUQ != AESUnitOfMeasureList.Codes.NoUnitRequired &&
						!IsLimitedReportingExportCode;
				}

				return result;
			}
		}

		public bool NeedsSecondCustomsQuantity
		{
			get
			{
				bool result = false;

				if (IsExport)//Used for validations to add a message error in base. Import does not need the validation
				{
					var customsSecondUQ = JI_CustomsSecondUnitQty;
					result = !customsSecondUQ.IsEmpty &&
						customsSecondUQ != AESUnitOfMeasureList.Codes.NoUnitRequired &&
						!IsLimitedReportingExportCode;
				}

				return result;
			}
		}

		public bool NeedsThirdCustomsQuantity
		{
			get { return IsQuantityRequired(JI_CustomsThirdUnitQty); }
		}

		public override ZDate EffectiveDateForDutyRate
		{
			get
			{
				if (effectiveDateForDutyRateCached == null)
				{
					effectiveDateForDutyRateCached = new CachedProperty<ZDate>(Factory, delegate
					{
						ZDate result = ZDate.Empty;
						var invoiceHeader = InvoiceHeader;
						var declaration = invoiceHeader == null ? null : invoiceHeader.JobDeclaration;

						if (IsExport)
						{
							result = US_DateOfExport.Date;
							if (!result.IsValid)
							{
								result = ZDate.Today;
							}
						}
						else
						{
							if (declaration != null && declaration.IsRecon)
							{
								if (invoiceHeader != null && invoiceHeader.ReconOriginalEntry != null && invoiceHeader.ReconOriginalEntry.US_R_DutyRateDate.IsValid)
								{
									result = invoiceHeader.ReconOriginalEntry.US_R_DutyRateDate.Date;
								}
							}
							else if (declaration != null && declaration.IsDrawback && this.US_DRWEntryDate.IsValid)
							{
								result = this.US_DRWEntryDate.Date;
							}
							else
							{
								result = ImportHelper.GetEffectiveDateForDutyRate();
							}
						}

						return result;
					});
				}
				return effectiveDateForDutyRateCached.Value;
			}
		}
		CachedProperty<ZDate> effectiveDateForDutyRateCached;

		public ZDate DateForADD_CVD
		{
			get
			{
				if (dateForADD_CVDCached == null)
				{
					dateForADD_CVDCached = new CachedProperty<ZDate>(Factory, delegate
					{
						return new DutyFeeDateCalculator().GetDutyFeeDateForADD_CVD(Declaration);
					});
				}
				return dateForADD_CVDCached.Value;
			}
		}
		CachedProperty<ZDate> dateForADD_CVDCached;

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.SetIndicatorList))]
		public override ZString US_SetInd
		{
			get { return base.US_SetInd; }
			set
			{
				bool hasChanges = base.US_SetInd != value;
				if (hasChanges)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_SetInd = value;

				if (hasChanges && !IsCopying)
				{
					SetHeaderComponentParentCalculator.UpdateWhenSetIndicatorChanges();
					if (US_SetInd == SecondarySpecProgIndicatorList.Codes.X)
					{
						US_TextileCategoryNo = ZString.Empty;
					}
					InvoiceHeader?.InvalidateJZ_Calc_LinesEnteredCache();
				}
			}
		}

		[ReadOnlyMember(nameof(JI_ParentID_ReadOnly))]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override ZGuid JI_ParentID
		{
			get { return base.JI_ParentID; }
			set
			{
				bool isDiff = base.JI_ParentID != value;

				JobComInvoiceLine oldParent = ParentTariffLine;
				if (isDiff && !IsCopying)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
					if (oldParent != null)
					{
						oldParent.US_IsParent = oldParent.ChildLines.Any(x => x.PK != this.PK);//at least one child not it self, it self will no longer child soon
						oldParent.childLines = null;
					}

					JobComInvoiceLine newParentLine = Factory.Load<JobComInvoiceLine>(value);
					if (newParentLine != null && newParentLine != this)
					{
						newParentLine.US_IsParent = true;
						newParentLine.childLines = null;// validation triggered at the line (base.JI_ParentID = value;) needs a refreshed ChildLines
					}
				}

				//***Should not access newParentLine.ChildLines or oldParent.ChildLines before base.JI_ParentID = value is executed***
				base.JI_ParentID = value;
				var newValue = JI_ParentID;
				var parentTariffLine = newValue.IsValid ? this.ParentTariffLine : null;
				if (parentTariffLine != null)
				{
					if (isDiff && !IsCopying && parentTariffLine.IsImportViaBIRD)
					{
						IsImportViaBIRD = true;
					}

					foreach (JobComInvoiceLine oldChildLine in ChildLines)//if this line is now a childline - then previous child lines can no longer be children
					{
						oldChildLine.JI_ParentID = newValue;
					}
					JI_BondedWhsQuantity = ZDecimal.Zero;
					US_WHSEntryLineNo = ZShort.Zero;
				}

				if (isDiff && !IsCopying)
				{
					UpdateBondedWhsRelatedData();
					DeleteWHSPackLines();

					SetHeaderComponentParentCalculator.UpdateWhenJI_ParentChanges();
					ClearIrrelevantValuesIfChildLine();
					if (InvoiceHeader != null)
					{
						InvoiceHeader.MarkAsNeedingValidation();
						LineGroupingRanges.MarkAsNeedingValidation();
					}
					MarkJobDeclarationAsNeedingValidation();

					Factory.GetCachedValue<SecondaryTariffCreator>().AddSecondaryTariffsFromTariffRule(this);

					DefaultTextileCategoryNumberFromTariff();
					AIILines.UpdateLineGroupingDetails();

					if (oldParent != null && !oldParent.IsValidationSuspended)
					{
						oldParent.Validation.ValidateJI_Tariff();
					}

					if (parentTariffLine != null && IsSecondaryTariffLine)
					{
						if (HasEmptySupTariff && ShouldCopySupTariffFromParentTariffLine(parentTariffLine.US_SupTariff))
						{
							US_SupTariff = parentTariffLine.US_SupTariff;
						}

						if (Declaration != null && Declaration.IsRecon &&
							US_R_OrigSupTariff.IsEmpty &&
							ShouldCopySupTariffFromParentTariffLine(parentTariffLine.US_R_OrigSupTariff))
						{
							US_R_OrigSupTariff = parentTariffLine.US_R_OrigSupTariff;
						}
					}

					if (this.IsCombinedLine() && IsSecondaryTariffLine)
					{
						this.US_SPI = ZString.Empty;
						this.US_SPIInfo.RefreshBinding();
					}
				}
			}
		}

		bool JI_ParentID_ReadOnly
		{
			get { return !JI_PartNo.IsEmpty; }
		}

		internal void ClearIrrelevantValuesIfChildLine()
		{
			if (IsChildLine)
			{
				JI_OA_ConsigneeAddress = ZGuid.Empty;

				if (IsSecondaryTariffLine)
				{
					US_UC_NKCountryOfExport = ZString.Empty;
					US_UC_NKCountryOfOrigin = ZString.Empty;
				}
			}
		}

		public ZString ImportEntryType
		{
			get { return Declaration?.US_EntryType ?? ZString.Empty; }
		}

		public IDisposable GetSetHeaderComponentParentCalculatorSuspender()
		{
			return SetHeaderComponentParentCalculator.GetCalculatorSuspender();
		}

		SetHeaderComponentParentCalculator SetHeaderComponentParentCalculator
		{
			get { return fSetHeaderComponentParentCalculator ?? (fSetHeaderComponentParentCalculator = new SetHeaderComponentParentCalculator(this)); }
		}
		SetHeaderComponentParentCalculator fSetHeaderComponentParentCalculator;

		protected override bool IsValidForLineTotalCalculation
		{
			get { return !IsSetXLine; }
		}

		public override Money JI_OverseasFreight
		{
			get
			{
				if (jI_OverseasFreightCached == null)
				{
					jI_OverseasFreightCached = new CachedProperty<Money>(Factory, delegate
					{
						return new OverseasFreightAndInsuranceCalculator().GetCharge(this, new string[] { Core.Constants.Customs.CustomsCharges.Codes.OverseasFreight });
					});
				}
				return jI_OverseasFreightCached.Value;
			}
		}
		CachedProperty<Money> jI_OverseasFreightCached;

		public override Money JI_OverseasInsurance
		{
			get
			{
				if (jI_OverseasInsuranceCached == null)
				{
					jI_OverseasInsuranceCached = new CachedProperty<Money>(Factory, delegate
					{
						return new OverseasFreightAndInsuranceCalculator().GetCharge(this, new string[] { Core.Constants.Customs.CustomsCharges.Codes.OverseasInsurance });
					});
				}
				return jI_OverseasInsuranceCached.Value;
			}
		}
		CachedProperty<Money> jI_OverseasInsuranceCached;

		public override Guid RegistryBranchPK
		{
			get
			{
				if (registryBranchPKCached == null)
				{
					registryBranchPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryBranchPK();
					});
				}
				return registryBranchPKCached.Value;
			}
		}
		CachedProperty<Guid> registryBranchPKCached;

		Guid GetRegistryBranchPK()
		{
			return base.RegistryBranchPK;
		}

		public override Guid RegistryCompanyPK
		{
			get
			{
				if (registryCompanyPKCached == null)
				{
					registryCompanyPKCached = new CachedProperty<Guid>(Factory, delegate
					{
						return GetRegistryCompanyPK();
					});
				}
				return registryCompanyPKCached.Value;
			}
		}
		CachedProperty<Guid> registryCompanyPKCached;

		Guid GetRegistryCompanyPK()
		{
			return base.RegistryCompanyPK;
		}

		protected override OrgHeader ImporterCore
		{
			get { return IsImport && InvoiceHeader != null ? InvoiceHeader.Importer : base.ImporterCore; }
		}

		public override bool NeedToApportionNetWeight
		{
			get
			{
				var entryType = Declaration == null ? ZString.Empty : Declaration.US_EntryType;
				return !IsImport
					   || NeedToApportionNetWeightCore(ImportTariff, entryType)
					   || SecondaryTariffLines.Select(l => l.ImportTariff).Any(t => NeedToApportionNetWeightCore(t, entryType));
			}
		}

		bool NeedToApportionNetWeightCore(USCTariff tariff, ZString entryType)
		{
			return tariff != null
				&& (tariff.UE_DutyComputationCode == ComputationCodeList.Codes.AdValorem || tariff.UE_DutyComputationCode == ComputationCodeList.Codes.Free)
				&& !(tariff.UE_QuotaIndicator && EntryTypeList.IsQuotaVisa(entryType));
		}

		public override OrgAddress ConsigneeAddressForDocument => ConsigneeAddress;

		#region Additional Tariff Related Properties

		#region Additional Tariff 1

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupTariffsList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|SupFormattedAdditionalTariff1", ShortCaption = "Prov Add. Tariff 1", Caption = "Prov/Prog. Additional Tariff 1")]
		[BusinessObjectTestExclude]
		public ZString SupFormattedAdditionalTariff1
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.AdditionalTariffFormatted ?? ZString.Empty;
			set
			{
				if (value != SupFormattedAdditionalTariff1)
				{
					CheckMaximumLength(SupFormattedAdditionalTariff1Info, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.AdditionalTariffFormatted, value, CusLineTariffTypeList.Codes.AdditionalTariff1);
					SupAdditionalTariff1MarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_SupAdditionalTariff1);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff1();
				}

				SupFormattedAdditionalTariff1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff1Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff1));

		[ReadOnlyMember(nameof(US_SupAdditionalTariff1Duty_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff1Duty", ShortCaption = "Prov Add. Duty 1", Caption = "Prov/Prog. Additional Duty 1")]
		public ZDecimal US_SupAdditionalTariff1Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.US_SupDuty ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff1Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_SupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff1);
				}

				US_SupAdditionalTariff1DutyInfo.RefreshBinding();
			}
		}

		public bool US_SupAdditionalTariff1Duty_ReadOnly
		{
			get => !US_OverrideSupAdditionalTariff1Duty;
		}

		public ZPropertyInfo US_SupAdditionalTariff1DutyInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff1Duty);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_OverrideSupAdditionalTariff1Duty", Caption = "Override Prov Add. Duty 1")]
		public ZBool US_OverrideSupAdditionalTariff1Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.US_OverrideSupDuty ?? false;
			set
			{
				if (value != US_OverrideSupAdditionalTariff1Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_OverrideSupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff1);
				}

				US_OverrideSupAdditionalTariff1DutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_OverrideSupAdditionalTariff1DutyInfo => GetZPropertyInfo(Schema.US_OverrideSupAdditionalTariff1Duty);

		public ZString US_SupAdditionalTariff1 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.BZ_Tariff ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff1Qty", ShortCaption = "Prov Add. Qty 1", Caption = "Prov/Prog Add. Qty 1")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff1Qty_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff1Qty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.BZ_Qty1 ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff1Qty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Qty1, value, CusLineTariffTypeList.Codes.AdditionalTariff1);
				}

				US_SupAdditionalTariff1QtyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff1QtyInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff1Qty));

		public bool US_SupAdditionalTariff1Qty_ReadOnly => !IsQuantityRequired(US_SupAdditionalTariff1UQ);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff1UQ", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString US_SupAdditionalTariff1UQ
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.BZ_UQ1 ?? ZString.Empty;
			set
			{
				if (value != US_SupAdditionalTariff1UQ)
				{
					CheckMaximumLength(US_SupAdditionalTariff1UQInfo, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_UQ1, value, CusLineTariffTypeList.Codes.AdditionalTariff1);
				}

				US_SupAdditionalTariff1UQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff1UQInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff1UQ));

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff1GoodsValue", ShortCaption = "Prov Add. Value 1", Caption = "Prov/Prog. Add. Goods Value 1", FullDescription = "If entered, the duty for Prov/Prog. Additional Tariff 1 will be calculated based on this value.")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff1GoodsValue_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff1GoodsValue
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.BZ_Value ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff1GoodsValue)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Value, value, CusLineTariffTypeList.Codes.AdditionalTariff1);
				}

				US_SupAdditionalTariff1GoodsValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff1GoodsValueInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff1GoodsValue);

		public bool US_SupAdditionalTariff1GoodsValue_ReadOnly => US_SupAdditionalTariff1.IsEmpty;

		#endregion

		#region Additional Tariff 2

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupTariffsList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|SupFormattedAdditionalTariff2", ShortCaption = "Prov Add. Tariff 2", Caption = "Prov/Prog. Additional Tariff 2")]
		[BusinessObjectTestExclude]
		public ZString SupFormattedAdditionalTariff2
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.AdditionalTariffFormatted ?? ZString.Empty;
			set
			{
				if (value != SupFormattedAdditionalTariff2)
				{
					CheckMaximumLength(SupFormattedAdditionalTariff2Info, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.AdditionalTariffFormatted, value, CusLineTariffTypeList.Codes.AdditionalTariff2);
					SupAdditionalTariff2MarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_SupAdditionalTariff2);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff2();
				}

				SupFormattedAdditionalTariff2Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff2Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff2));

		[ReadOnlyMember(nameof(US_SupAdditionalTariff2Duty_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff2Duty", ShortCaption = "Prov Add. Duty 2", Caption = "Prov/Prog. Additional Duty 2")]
		public ZDecimal US_SupAdditionalTariff2Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.US_SupDuty ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff2Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_SupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff2);
				}

				US_SupAdditionalTariff2DutyInfo.RefreshBinding();
			}
		}

		public bool US_SupAdditionalTariff2Duty_ReadOnly
		{
			get => !US_OverrideSupAdditionalTariff2Duty;
		}

		public ZPropertyInfo US_SupAdditionalTariff2DutyInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff2Duty);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_OverrideSupAdditionalTariff2Duty", Caption = "Override Prov Add. Duty 2")]
		public ZBool US_OverrideSupAdditionalTariff2Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.US_OverrideSupDuty ?? false;
			set
			{
				if (value != US_OverrideSupAdditionalTariff2Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_OverrideSupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff2);
				}

				US_OverrideSupAdditionalTariff2DutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_OverrideSupAdditionalTariff2DutyInfo => GetZPropertyInfo(Schema.US_OverrideSupAdditionalTariff2Duty);

		public ZString US_SupAdditionalTariff2 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.BZ_Tariff ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff2Qty", ShortCaption = "Prov Add. Qty 2", Caption = "Prov/Prog Add. Qty 2")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff2Qty_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff2Qty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.BZ_Qty1 ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff2Qty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Qty1, value, CusLineTariffTypeList.Codes.AdditionalTariff2);
				}

				US_SupAdditionalTariff2QtyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff2QtyInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff2Qty));

		public bool US_SupAdditionalTariff2Qty_ReadOnly => !IsQuantityRequired(US_SupAdditionalTariff2UQ);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff2UQ", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString US_SupAdditionalTariff2UQ
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.BZ_UQ1 ?? ZString.Empty;
			set
			{
				if (value != US_SupAdditionalTariff2UQ)
				{
					CheckMaximumLength(US_SupAdditionalTariff2UQInfo, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_UQ1, value, CusLineTariffTypeList.Codes.AdditionalTariff2);
				}

				US_SupAdditionalTariff2UQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff2UQInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff2UQ));

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff2GoodsValue", ShortCaption = "Prov Add. Value 2", Caption = "Prov/Prog. Add. Goods Value 2", FullDescription = "If entered, the duty for Prov/Prog. Additional Tariff 2 will be calculated based on this value.")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff2GoodsValue_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff2GoodsValue
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.BZ_Value ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff2GoodsValue)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Value, value, CusLineTariffTypeList.Codes.AdditionalTariff2);
				}

				US_SupAdditionalTariff2GoodsValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff2GoodsValueInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff2GoodsValue);

		public bool US_SupAdditionalTariff2GoodsValue_ReadOnly => US_SupAdditionalTariff2.IsEmpty;

		#endregion

		#region Additional Tariff 3

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupTariffsList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|SupFormattedAdditionalTariff3", ShortCaption = "Prov Add. Tariff 3", Caption = "Prov/Prog. Additional Tariff 3")]
		[BusinessObjectTestExclude]
		public ZString SupFormattedAdditionalTariff3
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.AdditionalTariffFormatted ?? ZString.Empty;
			set
			{
				if (value != SupFormattedAdditionalTariff3)
				{
					CheckMaximumLength(SupFormattedAdditionalTariff3Info, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.AdditionalTariffFormatted, value, CusLineTariffTypeList.Codes.AdditionalTariff3);
					SupAdditionalTariff3MarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_SupAdditionalTariff3);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff3();
				}

				SupFormattedAdditionalTariff3Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff3Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff3));

		[ReadOnlyMember(nameof(US_SupAdditionalTariff3Duty_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff3Duty", ShortCaption = "Prov Add. Duty 3", Caption = "Prov/Prog. Additional Duty 3")]
		public ZDecimal US_SupAdditionalTariff3Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.US_SupDuty ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff3Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_SupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff3);
				}

				US_SupAdditionalTariff3DutyInfo.RefreshBinding();
			}
		}

		public bool US_SupAdditionalTariff3Duty_ReadOnly
		{
			get => !US_OverrideSupAdditionalTariff3Duty;
		}

		public ZPropertyInfo US_SupAdditionalTariff3DutyInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff3Duty);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_OverrideSupAdditionalTariff3Duty", Caption = "Override Prov Add. Duty 3")]
		public ZBool US_OverrideSupAdditionalTariff3Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.US_OverrideSupDuty ?? false;
			set
			{
				if (value != US_OverrideSupAdditionalTariff3Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_OverrideSupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff3);
				}

				US_OverrideSupAdditionalTariff3DutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_OverrideSupAdditionalTariff3DutyInfo => GetZPropertyInfo(Schema.US_OverrideSupAdditionalTariff3Duty);

		public ZString US_SupAdditionalTariff3 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.BZ_Tariff ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff3Qty", ShortCaption = "Prov Add. Qty 3", Caption = "Prov/Prog Add. Qty 3")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff3Qty_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff3Qty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.BZ_Qty1 ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff3Qty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Qty1, value, CusLineTariffTypeList.Codes.AdditionalTariff3);
				}

				US_SupAdditionalTariff3QtyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff3QtyInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff3Qty));

		public bool US_SupAdditionalTariff3Qty_ReadOnly => !IsQuantityRequired(US_SupAdditionalTariff3UQ);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff3UQ", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString US_SupAdditionalTariff3UQ
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.BZ_UQ1 ?? ZString.Empty;
			set
			{
				if (value != US_SupAdditionalTariff3UQ)
				{
					CheckMaximumLength(US_SupAdditionalTariff3UQInfo, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_UQ1, value, CusLineTariffTypeList.Codes.AdditionalTariff3);
				}

				US_SupAdditionalTariff3UQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff3UQInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff3UQ));

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff3GoodsValue", ShortCaption = "Prov Add. Value 3", Caption = "Prov/Prog. Add. Goods Value 3", FullDescription = "If entered, the duty for Prov/Prog. Additional Tariff 3 will be calculated based on this value.")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff3GoodsValue_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff3GoodsValue
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.BZ_Value ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff3GoodsValue)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Value, value, CusLineTariffTypeList.Codes.AdditionalTariff3);
				}

				US_SupAdditionalTariff3GoodsValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff3GoodsValueInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff3GoodsValue);

		public bool US_SupAdditionalTariff3GoodsValue_ReadOnly => US_SupAdditionalTariff3.IsEmpty;

		#endregion

		#region Additional Tariff 4

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupTariffsList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|SupFormattedAdditionalTariff4", ShortCaption = "Prov Add. Tariff 4", Caption = "Prov/Prog. Additional Tariff 4")]
		[BusinessObjectTestExclude]
		public ZString SupFormattedAdditionalTariff4
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.AdditionalTariffFormatted ?? ZString.Empty;
			set
			{
				if (value != SupFormattedAdditionalTariff4)
				{
					CheckMaximumLength(SupFormattedAdditionalTariff4Info, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.AdditionalTariffFormatted, value, CusLineTariffTypeList.Codes.AdditionalTariff4);
					SupAdditionalTariff4MarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_SupAdditionalTariff4);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff4();
				}

				SupFormattedAdditionalTariff4Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff4Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff4));

		[ReadOnlyMember(nameof(US_SupAdditionalTariff4Duty_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff4Duty", ShortCaption = "Prov Add. Duty 4", Caption = "Prov/Prog. Additional Duty 4")]
		public ZDecimal US_SupAdditionalTariff4Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.US_SupDuty ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff4Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_SupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff4);
				}

				US_SupAdditionalTariff4DutyInfo.RefreshBinding();
			}
		}

		public bool US_SupAdditionalTariff4Duty_ReadOnly
		{
			get => !US_OverrideSupAdditionalTariff4Duty;
		}

		public ZPropertyInfo US_SupAdditionalTariff4DutyInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff4Duty);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_OverrideSupAdditionalTariff4Duty", Caption = "Override Prov Add. Duty 4")]
		public ZBool US_OverrideSupAdditionalTariff4Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.US_OverrideSupDuty ?? false;
			set
			{
				if (value != US_OverrideSupAdditionalTariff4Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_OverrideSupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff4);
				}

				US_OverrideSupAdditionalTariff4DutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_OverrideSupAdditionalTariff4DutyInfo => GetZPropertyInfo(Schema.US_OverrideSupAdditionalTariff4Duty);

		public ZString US_SupAdditionalTariff4 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.BZ_Tariff ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff4Qty", ShortCaption = "Prov Add. Qty 4", Caption = "Prov/Prog Add. Qty 4")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff4Qty_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff4Qty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.BZ_Qty1 ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff4Qty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Qty1, value, CusLineTariffTypeList.Codes.AdditionalTariff4);
				}

				US_SupAdditionalTariff4QtyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff4QtyInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff4Qty));

		public bool US_SupAdditionalTariff4Qty_ReadOnly => !IsQuantityRequired(US_SupAdditionalTariff4UQ);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff4UQ", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString US_SupAdditionalTariff4UQ
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.BZ_UQ1 ?? ZString.Empty;
			set
			{
				if (value != US_SupAdditionalTariff4UQ)
				{
					CheckMaximumLength(US_SupAdditionalTariff4UQInfo, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_UQ1, value, CusLineTariffTypeList.Codes.AdditionalTariff4);
				}

				US_SupAdditionalTariff4UQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff4UQInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff4UQ));

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff4GoodsValue", ShortCaption = "Prov Add. Value 4", Caption = "Prov/Prog. Add. Goods Value 4", FullDescription = "If entered, the duty for Prov/Prog. Additional Tariff 4 will be calculated based on this value.")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff4GoodsValue_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff4GoodsValue
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.BZ_Value ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff4GoodsValue)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Value, value, CusLineTariffTypeList.Codes.AdditionalTariff4);
				}

				US_SupAdditionalTariff4GoodsValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff4GoodsValueInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff4GoodsValue);

		public bool US_SupAdditionalTariff4GoodsValue_ReadOnly => US_SupAdditionalTariff4.IsEmpty;

		#endregion

		#region Additional Tariff 5

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.SupTariffsList))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|SupFormattedAdditionalTariff5", ShortCaption = "Prov Add. Tariff 5", Caption = "Prov/Prog. Additional Tariff 5")]
		[BusinessObjectTestExclude]
		public ZString SupFormattedAdditionalTariff5
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.AdditionalTariffFormatted ?? ZString.Empty;
			set
			{
				if (value != SupFormattedAdditionalTariff5)
				{
					CheckMaximumLength(SupFormattedAdditionalTariff5Info, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.AdditionalTariffFormatted, value, CusLineTariffTypeList.Codes.AdditionalTariff5);
					SupAdditionalTariff5MarkedForReferenceFileRequest = TariffValidator.CorrectTariffLength(US_SupAdditionalTariff5);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff5();
				}

				SupFormattedAdditionalTariff5Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff5Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff5));

		[ReadOnlyMember(nameof(US_SupAdditionalTariff5Duty_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff5Duty", ShortCaption = "Prov Add. Duty 5", Caption = "Prov/Prog. Additional Duty 5")]
		public ZDecimal US_SupAdditionalTariff5Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.US_SupDuty ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff5Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_SupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff5);
				}

				US_SupAdditionalTariff5DutyInfo.RefreshBinding();
			}
		}

		public bool US_SupAdditionalTariff5Duty_ReadOnly
		{
			get => !US_OverrideSupAdditionalTariff5Duty;
		}

		public ZPropertyInfo US_SupAdditionalTariff5DutyInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff5Duty);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_OverrideSupAdditionalTariff5Duty", Caption = "Override Prov Add. Duty 5")]
		public ZBool US_OverrideSupAdditionalTariff5Duty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.US_OverrideSupDuty ?? false;
			set
			{
				if (value != US_OverrideSupAdditionalTariff5Duty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.US_OverrideSupDuty, value, CusLineTariffTypeList.Codes.AdditionalTariff5);
				}

				US_OverrideSupAdditionalTariff5DutyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_OverrideSupAdditionalTariff5DutyInfo => GetZPropertyInfo(Schema.US_OverrideSupAdditionalTariff5Duty);

		public ZString US_SupAdditionalTariff5 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.BZ_Tariff ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff5Qty", ShortCaption = "Prov Add. Qty 5", Caption = "Prov/Prog Add. Qty 5")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff5Qty_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff5Qty
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.BZ_Qty1 ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff5Qty)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Qty1, value, CusLineTariffTypeList.Codes.AdditionalTariff5);
				}

				US_SupAdditionalTariff5QtyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff5QtyInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff5Qty));

		public bool US_SupAdditionalTariff5Qty_ReadOnly => !IsQuantityRequired(US_SupAdditionalTariff5UQ);

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff5UQ", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CustomsUQList))]
		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString US_SupAdditionalTariff5UQ
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.BZ_UQ1 ?? ZString.Empty;
			set
			{
				if (value != US_SupAdditionalTariff5UQ)
				{
					CheckMaximumLength(US_SupAdditionalTariff5UQInfo, value);
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_UQ1, value, CusLineTariffTypeList.Codes.AdditionalTariff5);
				}

				US_SupAdditionalTariff5UQInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff5UQInfo => GetZPropertyInfo(nameof(US_SupAdditionalTariff5UQ));

		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|US_SupAdditionalTariff5GoodsValue", ShortCaption = "Prov Add. Value 5", Caption = "Prov/Prog. Add. Goods Value 5", FullDescription = "If entered, the duty for Prov/Prog. Additional Tariff 5 will be calculated based on this value.")]
		[ReadOnlyMember(nameof(US_SupAdditionalTariff5GoodsValue_ReadOnly))]
		public ZDecimal US_SupAdditionalTariff5GoodsValue
		{
			get => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.BZ_Value ?? ZDecimal.Zero;
			set
			{
				if (value != US_SupAdditionalTariff5GoodsValue)
				{
					UpdateOrAddTariffDetail(CusLineTariffDetail.Schema.BZ_Value, value, CusLineTariffTypeList.Codes.AdditionalTariff5);
				}

				US_SupAdditionalTariff5GoodsValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_SupAdditionalTariff5GoodsValueInfo => GetZPropertyInfo(Schema.US_SupAdditionalTariff5GoodsValue);

		public bool US_SupAdditionalTariff5GoodsValue_ReadOnly => US_SupAdditionalTariff5.IsEmpty;

		#endregion

		CusLineTariffDetail LoadTariffDetailByType(ZString tariffType)
		{
			return CusLineTariffDetails.Cast<CusLineTariffDetail>().FirstOrDefault(detail => detail.BZ_Type == tariffType && !detail.IsDeleted);
		}

		void UpdateOrAddTariffDetail(ZString fieldToUpdate, IZType valueToSet, ZString tariffType)
		{
			var tariffDetail = LoadTariffDetailByType(tariffType);
			if (tariffDetail == null)
			{
				tariffDetail = CusLineTariffDetails.AddNew();
				tariffDetail.BZ_Type = tariffType;
			}

			tariffDetail[fieldToUpdate] = valueToSet;
		}

		protected override bool SupportsAdditionalTariffs => true;

		public new ICusLineTariffDetailCollection<CusLineTariffDetail> CusLineTariffDetails => (CusLineTariffDetailCollection<CusLineTariffDetail>)base.CusLineTariffDetails;

		protected override ICusLineTariffDetailCollection<Customs.Business.CusLineTariffDetail> GetCusLineTariffDetails() => new CusLineTariffDetailCollection(this);

		#endregion

		#endregion

		#region Overriden Methods

		public override void OnLoaded()
		{
			using (EnableDebugLog())
			{
				base.OnLoaded();
				if (!IsDeleted)
				{
					DDTCDataCorrection.RegisterTrackerIfNeeded();
					TSCADataCorrection.RegisterTrackerIfNeeded();
					ODSDataCorrection.RegisterTrackerIfNeeded();
				}
			}
		}

		internal void RefreshInvoiceLinesWithPGAIndicators()
		{
			Declaration?.PGAFlags.RefreshInvoiceLinesWithPGAIndicators();

			if (InvoiceHeader != null)
			{
				InvoiceHeader.RefreshInvoiceLinesWithPGAIndicators();
			}
		}

		void RefreshInvoiceLinesWithSpecificColumnsChanged()
		{
			InvoiceHeader?.RefreshInvoiceLinesWithSpecificColumnsChanged();
			Declaration?.PGAFlags.RefreshInvoiceLinesWithSpecificColumnsChanged();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (IsChildLine)
			{
				LineGroupingRanges.RemoveAndDeleteAll();
			}

			if (!IsValidForAII)
			{
				AIILines.RemoveAndDeleteAll();
			}
			US_CI_PreviousPivot = previousPivotPK.IsValid ? previousPivotPK : ZGuid.Empty;

			if (IsCBMAProductClaimAndIsCBMA23Effective)
			{
				US_CBMADefaultTaxAmount = US_CBMADefaultTaxRate * ((IDutyData)this).Quantity1;
			}
			else
			{
				US_CBMADefaultTaxAmount = ZDecimal.Zero;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				OGAAgencyRequirements.RemoveAndDeleteAll();
				ExportPGAAgencyRequirements.RemoveAndDeleteAll();

				var parentTariffLine = ParentTariffLine;
				NMFSLines.RemoveAndDeleteAll();
				APHISHeaders.RemoveAndDeleteAll();
				FWSHeaders.RemoveAndDeleteAll();
				NHTSALines.RemoveAndDeleteAll();
				ATFLines.RemoveAndDeleteAll();
				PSTLines.RemoveAndDeleteAll();
				USHFCHeaders.RemoveAndDeleteAll();
				ACE_FDALines.RemoveAndDeleteAll();
				VehicleLines.RemoveAndDeleteAll();
				AMSLines.RemoveAndDeleteAll();
				FSISLines.RemoveAndDeleteAll();
				FDAs.RemoveAndDeleteAll();
				FCCs.RemoveAndDeleteAll();
				DOTs.RemoveAndDeleteAll();
				TTBLines.RemoveAndDeleteAll();
				OMCHeaders.RemoveAndDeleteAll();
				LaceyActLines.RemoveAndDeleteAll();
				FeeCusCodes.RemoveAndDeleteAll();
				AIILines.RemoveAndDeleteAll();
				LineGroupingRanges.RemoveAndDeleteAll();
				CensusWarningOverrides.RemoveAndDeleteAll();
				CPSCHeaders.RemoveAndDeleteAll();
				DEAHeaders.RemoveAndDeleteAll();
				ReconOriginalCharges.RemoveAndDeleteAll();
				DrawbackAdditionalImportTariffNumbers.RemoveAndDeleteAll();
				DrawbackAdditionalExportTariffNumbers.RemoveAndDeleteAll();
				FishingInformations.RemoveAndDeleteAll();
				MiningInformations.RemoveAndDeleteAll();

				var declaration = Declaration;
				if (!JI_PartNo.IsEmpty && declaration != null)
				{
					declaration.MarkReconIndicatorsDirty();
				}
				using (declaration != null ? declaration.SuspendWeightApportionment() : DisposableAction.NoAction)
				{
					using (GetValidationSuspender())
					{
						foreach (var invoiceLine in ChildLines)
						{
							invoiceLine.Delete();
						}
					}

					foreach (var invoiceLine in ProductRelatedLines)
					{
						invoiceLine.Delete();
					}
				}

				RefreshInvoiceLinesWithPGAIndicators();
				RefreshInvoiceLinesWithSpecificColumnsChanged();

				DeleteWHSPackLines();

				base.Delete();

				if (parentTariffLine != null && !parentTariffLine.IsDeleted)
				{
					parentTariffLine.US_IsParent = parentTariffLine.ChildLines.IsCountMoreThan(1) || (parentTariffLine.ChildLines.IsCountEqualTo(1) && !parentTariffLine.ChildLines.Contains(this));
					parentTariffLine.RefreshChildLines();

					if (!parentTariffLine.IsValidationSuspended)
					{
						parentTariffLine.Validation.ValidateJI_Tariff();
					}

					foreach (var childLine in parentTariffLine.SecondaryTariffLines)
					{
						if (!childLine.IsValidationSuspended)
						{
							childLine.Validation.ValidateJI_ParentID();
						}
					}
				}

				var productParentTariffLine = ProductParentTariffLine;
				if (productParentTariffLine != null && !IsDeleted && !productParentTariffLine.IsDeleted)
				{
					productParentTariffLine.RefreshProductRelatedLines();
				}

				if (declaration != null)
				{
					Declaration.RefreshHas9802Tariff();
				}
			}
		}

		void DeleteWHSPackLines()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				declaration.WHSPackLines.OfType<WHSPackLine>().Where(x => x.US_JI_InvoiceLine == PK).DeleteAll();
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			bool oldValue = PartSyncManager.Enabled;
			JobComInvoiceLine result = null;
			try
			{
				PartSyncManager.Enabled = false; // stop PartSyncManager from changing the JI_CustomsQuantity
				result = (JobComInvoiceLine)base.CloneInternal(args);
				result.ResetValuesAfterClone();

				foreach (FDA fda in FDAs)
				{
					var fdaCloned = (FDA)fda.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(FDA), false));
					fdaCloned.US_FDAConfirmDate = ZDateTime.Empty;
					fdaCloned.US_FDAValue = ZDecimal.Zero;
					fdaCloned.US_InvCurrFDAValue = ZDecimal.Zero;

					result.FDAs.Add(fdaCloned);
				}

				foreach (ACEFDA ace_fda in ACE_FDALines)
				{
					var ace_fdaCloned = (ACEFDA)ace_fda.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(ACEFDA), false));
					ace_fdaCloned.US_TrackingStatus = ZString.Empty;
					result.ACE_FDALines.Add(ace_fdaCloned);
				}

				foreach (Vehicle vne in VehicleLines)
				{
					var vneCloned = (Vehicle)vne.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(Vehicle), false));
					vneCloned.US_TrackingStatus = ZString.Empty;
					result.VehicleLines.Add(vneCloned);
				}

				foreach (AMS ams in AMSLines)
				{
					var amsCloned = (AMS)ams.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(AMS), false));
					amsCloned.US_TrackingStatus = ZString.Empty;
					result.AMSLines.Add(amsCloned);
				}

				foreach (FCC fcc in FCCs)
				{
					result.FCCs.Add((FCC)fcc.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(FCC), false)));
				}

				foreach (DOT dot in DOTs)
				{
					result.DOTs.Add((DOT)dot.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(DOT), false)));
				}

				foreach (PGA pga in LaceyActLines)
				{
					var pgaCloned = (PGA)pga.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(PGA), false));
					pgaCloned.US_TrackingStatus = ZString.Empty;
					result.LaceyActLines.Add(pgaCloned);
				}

				foreach (USInvoiceLineFSISLine fsis in FSISLines)
				{
					var fsisCloned = (USInvoiceLineFSISLine)fsis.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(USInvoiceLineFSISLine), false));
					fsisCloned.US_TrackingStatus = ZString.Empty;
					result.FSISLines.Add(fsisCloned);
				}

				foreach (Pesticide pesticide in PSTLines)
				{
					var pstCloned = (Pesticide)pesticide.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(Pesticide), false));
					pstCloned.US_TrackingStatus = ZString.Empty;
					result.PSTLines.Add(pstCloned);
				}

				foreach (USHFCHeader hfcHeader in USHFCHeaders)
				{
					var hfcCloned = (USHFCHeader)hfcHeader.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(USHFCHeader), false));
					hfcCloned.US_TrackingStatus = ZString.Empty;
					result.USHFCHeaders.Add(hfcCloned);
				}

				foreach (NMFSLine nmfs in NMFSLines)
				{
					var nmfsCloned = (NMFSLine)nmfs.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NMFSLine), false));
					nmfsCloned.US_TrackingStatus = ZString.Empty;
					result.NMFSLines.Add(nmfsCloned);
				}

				foreach (TTBLine ttb in TTBLines)
				{
					var ttbCloned = (TTBLine)ttb.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(TTBLine), false));
					ttbCloned.US_TrackingStatus = ZString.Empty;
					result.TTBLines.Add(ttbCloned);
				}

				foreach (APHISHeader aphis in APHISHeaders)
				{
					var aphisCloned = (APHISHeader)aphis.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(APHISHeader), false));
					aphisCloned.US_TrackingStatus = ZString.Empty;
					result.APHISHeaders.Add(aphisCloned);
				}

				foreach (OMCHeader omc in OMCHeaders)
				{
					var omcCloned = (OMCHeader)omc.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(OMCHeader), false));
					omcCloned.US_TrackingStatus = ZString.Empty;
					result.OMCHeaders.Add(omcCloned);
				}

				foreach (FWSHeader fws in FWSHeaders)
				{
					var fwsCloned = (FWSHeader)fws.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(FWSHeader), false));
					fwsCloned.US_TrackingStatus = ZString.Empty;
					result.FWSHeaders.Add(fwsCloned);
				}

				foreach (NHTSAHeader nhtsaLine in NHTSALines)
				{
					var nhtsaCloned = (NHTSAHeader)nhtsaLine.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NHTSAHeader), false));
					nhtsaCloned.US_TrackingStatus = ZString.Empty;
					result.NHTSALines.Add(nhtsaCloned);
				}

				foreach (ATF atfLine in ATFLines)
				{
					var atfCloned = (ATF)atfLine.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(ATF), false));
					atfCloned.US_TrackingStatus = ZString.Empty;
					result.ATFLines.Add(atfCloned);
				}

				foreach (CPSCHeader cpsc in CPSCHeaders)
				{
					var cpscCloned = (CPSCHeader)cpsc.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(CPSCHeader), false));
					cpscCloned.US_TrackingStatus = ZString.Empty;
					result.CPSCHeaders.Add(cpscCloned);
				}

				foreach (DEAHeader dea in DEAHeaders)
				{
					var deaCloned = (DEAHeader)dea.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(DEAHeader), false));
					deaCloned.US_TrackingStatus = ZString.Empty;
					result.DEAHeaders.Add(deaCloned);
				}

				if (IsDrawback)
				{
					CopyDrawbackDetails(result);
				}
			}
			finally
			{
				PartSyncManager.Enabled = true;
			}
			return result;
		}

		protected override void ResetValuesAfterCloneCore()
		{
			US_98GoodsValue = ZDecimal.Zero;
			US_98ValueInvCurr = ZDecimal.Zero;
			US_ADDDepositValue = ZDecimal.Zero;
			US_ADDuty = ZDecimal.Zero;
			US_CVDDepositValue = ZDecimal.Zero;
			US_CVDuty = ZDecimal.Zero;
			US_CustomsValue = ZDecimal.Zero;
			US_Duty = ZDecimal.Zero;
			US_SupDuty = ZDecimal.Zero;
			US_HasMPF = false;
			US_OverrideDuty = false;
			US_OverrideSupDuty = false;
			US_PayableMPF = ZDecimal.Zero;
			US_DateOfExport = ZDateTime.Empty;
			US_DDTCArrivalDate = ZDateTime.Empty;
			US_DDTCTrackingStatus = ZString.Empty;
			US_ODSTrackingStatus = ZString.Empty;
			US_TSCATrackingStatus = ZString.Empty;
			ResetDutyAndFeeAnalysisData();
			this.ClearTrackingID();
		}

		public void ResetDutyAndFeeAnalysisData()
		{
			US_FTADuty = ZDecimal.Zero;
			US_FTAPayableMPF = ZDecimal.Zero;
			US_NonFTADuty = ZDecimal.Zero;
			US_NonFTAPayableMPF = ZDecimal.Zero;
		}

		#endregion

		#region AddInfo Overriden Properties

		public override ZString US_ADDCaseNo
		{
			get { return base.US_ADDCaseNo; }
			set
			{
				ZString oldValue = US_ADDCaseNo;
				base.US_ADDCaseNo = value;
				if (oldValue != US_ADDCaseNo)
				{
					US_ADDQty = ZDecimal.Zero;
					US_ADDDepositRateIndicator = Lookups.AntidumpingDutyDepositRates.Count > 0 ? Lookups.AntidumpingDutyDepositRates[0].Code : string.Empty;
					US_ADDDecID = ZString.Empty;

					if (IsFTZADDCVD)
					{
						US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
					}

					RefreshInvoiceLinesWithSpecificColumnsChanged();
				}
			}
		}

		public override ZString US_CVDCaseNo
		{
			get { return base.US_CVDCaseNo; }
			set
			{
				ZString oldValue = US_CVDCaseNo;
				base.US_CVDCaseNo = value;
				if (oldValue != US_CVDCaseNo)
				{
					US_CVDQty = ZDecimal.Zero;//for ACE, it get copied from
					US_CVDDepositRateIndicator = Lookups.CountervailingDutyDepositRates.Count > 0 ? Lookups.CountervailingDutyDepositRates[0].Code : string.Empty;

					if (IsFTZADDCVD)
					{
						US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
					}

					RefreshInvoiceLinesWithSpecificColumnsChanged();
				}
			}
		}

		public override ZString US_ADDDepositRateIndicator
		{
			get { return base.US_ADDDepositRateIndicator; }
			set
			{
				bool hasChanges = base.US_ADDDepositRateIndicator != value;
				base.US_ADDDepositRateIndicator = value;

				if (hasChanges && !IsCopying)
				{
					if (IsADDManual)
					{
						US_ADDDepositValue = ZDecimal.Zero;
					}
					else
					{
						US_ADDuty = ZDecimal.Zero;
						if (US_ADDDepositRateIndicator.Contains(DepositRateIndicatorList.Codes.Specific))
						{
							US_ADDDepositValue = ZDecimal.Zero;
							CopyQuantityFromCustomsQuantity(US_ADDUQ, Schema.US_ADDQty);
						}
						else
						{
							US_ADDQty = ZDecimal.Zero;
						}
					}

					US_ADDDepositRateOverride = ZDecimal.Zero;
				}
			}
		}

		public override ZString US_CVDDepositRateIndicator
		{
			get { return base.US_CVDDepositRateIndicator; }
			set
			{
				bool hasChanges = base.US_CVDDepositRateIndicator != value;
				base.US_CVDDepositRateIndicator = value;

				if (hasChanges && !IsCopying)
				{
					if (IsCVDManual)
					{
						US_CVDDepositValue = ZDecimal.Zero;
					}
					else
					{
						US_CVDuty = ZDecimal.Zero;
						if (US_CVDDepositRateIndicator.Contains(DepositRateIndicatorList.Codes.Specific))
						{
							US_CVDDepositValue = ZDecimal.Zero;
							CopyQuantityFromCustomsQuantity(US_CVDUQ, Schema.US_CVDQty);
						}
						else
						{
							US_CVDQty = ZDecimal.Zero;
						}
					}

					US_CVDDepositRateOverride = ZDecimal.Zero;
				}
			}
		}

		void CopyQuantityFromCustomsQuantity(ZString adcvdUQ, string adcvdQtyFieldName)
		{
			if (!adcvdUQ.IsEmpty)
			{
				if (adcvdUQ == JI_CustomsUnitQty)
				{
					this[adcvdQtyFieldName] = JI_CustomsQuantity;
				}
				else if (adcvdUQ == JI_CustomsSecondUnitQty)
				{
					this[adcvdQtyFieldName] = JI_CustomsSecondQuantity;
				}
			}
		}

		[ReadOnlyMember(nameof(IsADDManual))]
		public override ZDecimal US_ADDDepositValue
		{
			get { return base.US_ADDDepositValue; }
			set { base.US_ADDDepositValue = value; }
		}

		[ReadOnlyMember(nameof(IsCVDManual))]
		public override ZDecimal US_CVDDepositValue
		{
			get { return base.US_CVDDepositValue; }
			set { base.US_CVDDepositValue = value; }
		}

		public ZBool IsADDManual
		{
			get { return !IsACE && US_ADDDepositRateIndicator.Contains(DepositRateIndicatorList.Codes.Specific); }
		}

		public ZBool IsCVDManual
		{
			get { return !IsACE && US_CVDDepositRateIndicator.Contains(DepositRateIndicatorList.Codes.Specific); }
		}

		#region US_VehicleID

		protected bool US_VehicleID_ReadOnly
		{
			get { return GetUS_VehicleIDReadOnly(); }
		}

		protected virtual bool GetUS_VehicleIDReadOnly()
		{
			return IsExport && !US_IsUsedVehicle;
		}

		#endregion

		#region US_VehicleIDType

		protected bool US_VehicleIDType_ReadOnly
		{
			get { return GetUS_VehicleIDTypeReadOnly(); }
		}

		protected virtual bool GetUS_VehicleIDTypeReadOnly()
		{
			return IsExport && !US_IsUsedVehicle;
		}
		#endregion

		#region US_VehicleTitleNo

		protected bool US_VehicleTitleNo_ReadOnly
		{
			get { return GetUS_VehicleTitleNoReadOnly(); }
		}

		protected virtual bool GetUS_VehicleTitleNoReadOnly()
		{
			return IsExport && !US_IsUsedVehicle;
		}
		#endregion

		#region US_VehicleTitleState

		protected bool US_VehicleTitleState_ReadOnly
		{
			get { return GetUS_VehicleTitleStateReadOnly(); }
		}

		protected virtual bool GetUS_VehicleTitleStateReadOnly()
		{
			return IsExport && !US_IsUsedVehicle;
		}
		#endregion

		public override ZGuid JI_OA_ConsigneeAddress
		{
			get { return GetEffectiveAddressValueToReturn(base.JI_OA_ConsigneeAddress, JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress, JobComInvoiceLine.Schema.ConsigneeAddressOrgPK, disableSettingConsigneeDefaults, (addressOrgPK) => SetJI_OA_ConsigneeAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK)); }
			set
			{
				if (!IsCopying && JI_OA_ConsigneeAddress != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.JI_OA_ConsigneeAddress = GetEffectiveValueToSet(value, JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);

				if (JI_OA_ShipToPartyAddress.IsEmpty)
				{
					JI_OA_ShipToPartyAddress = value;
				}
			}
		}

		public bool JI_OA_ConsigneeAddress_ReadOnly
		{
			get { return ParentTariffLine != null; }
		}

		[ReadOnlyMember(nameof(JI_OA_SoldToPartyAddress_ReadOnly))]
		public override ZGuid JI_OA_SoldToPartyAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JI_OA_SoldToPartyAddress,
					JobComInvoiceLine.Schema.JI_OA_SoldToPartyAddress,
					JobComInvoiceHeader.Schema.JZ_OA_SoldToPartyAddress,
					JobComInvoiceLine.Schema.SoldToPartyOrgPK,
					disableSettingSoldToPartyDefaults,
					(addressOrgPK) => SetJI_OA_SoldToParty_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK));
			}
			set
			{
				var oldValue = base.JI_OA_SoldToPartyAddress;
				var newValue = GetEffectiveValueToSet(value, JobComInvoiceLine.Schema.JI_OA_SoldToPartyAddress, JobComInvoiceHeader.Schema.JZ_OA_SoldToPartyAddress);
				if (oldValue != newValue)
				{
					base.JI_OA_SoldToPartyAddress = newValue;
				}
				else
				{
					Validation.ValidateJI_OA_SoldToPartyAddress();
				}
			}
		}

		bool JI_OA_SoldToPartyAddress_ReadOnly
		{
			get { return ParentTariffLine != null; }
		}

		public ZString SoldToPartyEIN
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(SoldToParty, OrgMatchedCustomsRegNoType.EIN); }
		}

		public override ZGuid JI_OA_Seller
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JI_OA_Seller,
														Schema.JI_OA_Seller,
														JobComInvoiceHeader.Schema.JZ_OA_SellerAddress,
														Schema.SellerOrgPK,
														disableSettingSellerDefaults,
														(addressOrgPK) => SetJI_OA_SellerAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK));
			}

			set
			{
				var oldValue = base.JI_OA_Seller;
				var newValue = GetEffectiveValueToSet(value, Schema.JI_OA_Seller, JobComInvoiceHeader.Schema.JZ_OA_SellerAddress);
				if (!IsCopying && oldValue != value)
				{
					base.JI_OA_Seller = newValue;
				}
				else
				{
					Validation.ValidateJI_OA_Seller();
				}

				InvoiceHeader?.MarkAsNeedingValidation();
			}
		}

		public override ZGuid JI_OA_ShipToPartyAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(base.JI_OA_ShipToPartyAddress,
														Schema.JI_OA_ShipToPartyAddress,
														JobComInvoiceHeader.Schema.JZ_OA_ShipToPartyAddress,
														Schema.ShipToPartyOrgPK,
														disableSettingShipToPartyDefaults,
														(addressOrgPK) => SetJI_OA_ShipToPartyAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK));
			}
			set
			{
				var oldValue = base.JI_OA_ShipToPartyAddress;
				var newValue = GetEffectiveValueToSet(value, Schema.JI_OA_ShipToPartyAddress, JobComInvoiceHeader.Schema.JZ_OA_ShipToPartyAddress);
				if (oldValue != newValue)
				{
					base.JI_OA_ShipToPartyAddress = newValue;
				}
				else
				{
					Validation.ValidateJI_OA_ShipToPartyAddress();
				}

				InvoiceHeader?.MarkAsNeedingValidation();
				this.ACE_FDALines.OfType<ACEFDA>().ForEach(fda => fda.RefreshDeliverToPartyZAddress());
			}
		}

		public bool JI_OA_Seller_ReadOnly
		{
			get { return ParentTariffLine != null; }
		}

		[ReadOnlyMember(nameof(JI_OA_ExporterAddress_ReadOnly))]
		public override ZGuid JI_OA_ExporterAddress
		{
			get
			{
				return GetEffectiveAddressValueToReturn(
			  base.JI_OA_ExporterAddress,
			  Schema.JI_OA_ExporterAddress,
			  JobComInvoiceHeader.Schema.JZ_OA_ExporterAddress,
			  Schema.ForeignExporterOrgPK,
			  disableSettingForeignExporterDetails,
			  (addressOrgPK) => SetJI_OA_ExporterAddress_ZAddressOrgPKWithoutSettingDefaults(addressOrgPK),
			   JobComInvoiceHeader.Schema.ExporterOrgPK);
			}
			set
			{
				if (!IsCopying && JI_OA_ExporterAddress != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.JI_OA_ExporterAddress = GetEffectiveValueToSet(value, JobComInvoiceLine.Schema.JI_OA_ExporterAddress, JobComInvoiceHeader.Schema.JZ_OA_ExporterAddress);
			}
		}

		bool JI_OA_ExporterAddress_ReadOnly
		{
			get { return ParentTariffLine != null; }
		}

		public ZString ForeignExporterMID
		{
			get { return OrgHeaderWrapper.GetCustomsCodeFromAddress(ExporterAddress, OrgCusCode.USACodeTypes.ManufacturerID); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_OGAIndicatorList))]
		public override ZString US_FDAIndicator
		{
			get { return base.US_FDAIndicator; }
			set
			{
				var wasFDADeclared = IsFDADeclared;
				var oldValue = US_FDAIndicator;
				if (!IsCopying && oldValue != value)
				{
					value = PGADataChangeTrackerSupporter.LoadAllPGARelatedDataAndMarkStatusToBeDeletedIfNeeded(oldValue, value, ACE_FDALines.Cast<IPGADataCorrection>());
				}

				base.US_FDAIndicator = value;

				if (!IsCopying && oldValue != US_FDAIndicator)
				{
					var declaration = Declaration;
					if (declaration != null && declaration.IsPersistent && !declaration.CanHavePGAFDA)
					{
						declaration.UpdateFDAMsgStatus(wasFDADeclared == IsFDADeclared ? 0 : (wasFDADeclared ? -1 : 1));
					}

					if (US_FDADisclaimReason_ReadOnly)
					{
						if (!US_FDADisclaimReason.IsEmpty)
						{
							US_FDADisclaimReason = ZString.Empty;
						}
					}
					else
					{
						if (US_FDADisclaimReason.IsEmpty)
						{
							US_FDADisclaimReason = PGADisclaimReasonList.Codes.A;
						}
					}
					Declaration?.UpdatePGADataReplacementUpdateRequired();
					RefreshInvoiceLinesWithPGAIndicators();
					DefaultFDADateIfRequired();
				}
			}
		}

		[ReadOnlyMember(nameof(US_FDADisclaimReason_ReadOnly))]
		public override ZString US_FDADisclaimReason
		{
			get { return base.US_FDADisclaimReason; }
			set
			{
				if (!IsCopying && US_FDADisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_FDADisclaimReason = value;
			}
		}

		bool US_FDADisclaimReason_ReadOnly
		{
			get { return US_FDAIndicator != OGAIndicatorList.Codes.Disclaimed; }
		}

		[ReadOnlyMember(nameof(US_PayableMPF_ReadOnly))]
		public override ZDecimal US_PayableMPF
		{
			get { return base.US_PayableMPF; }
			set { base.US_PayableMPF = value; }
		}

		bool US_PayableMPF_ReadOnly
		{
			get { return true; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_OGAIndicatorList))]
		public override ZString US_FCCIndicator
		{
			get { return base.US_FCCIndicator; }
			set { base.US_FCCIndicator = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_OGAIndicatorList))]
		public override ZString US_DOTIndicator
		{
			get { return base.US_DOTIndicator; }
			set
			{
				bool hasChanges = base.US_DOTIndicator != value;
				base.US_DOTIndicator = value;

				if (hasChanges && !IsCopying)
				{
					RefreshInvoiceLinesWithPGAIndicators();
				}
			}
		}

		public bool US_R_OrigFirstQty_ReadOnly
		{
			get { return !IsQuantityRequired(US_R_OrigFirstUQ); }
		}

		public bool US_R_OrigFirstUQ_ReadOnly
		{
			get { return true; }
		}

		public bool US_R_OrigSecondQty_ReadOnly
		{
			get { return !IsQuantityRequired(US_R_OrigSecondUQ); }
		}

		public bool US_R_OrigSecondUQ_ReadOnly
		{
			get { return true; }
		}

		public bool US_R_OrigThirdQty_ReadOnly
		{
			get { return !IsQuantityRequired(US_R_OrigThirdUQ); }
		}

		public bool US_R_OrigThirdUQ_ReadOnly
		{
			get { return true; }
		}

		internal ZDecimal DairyQty
		{
			get
			{
				return
						JI_CustomsUnitQty == ABIUnitOfMeasureList.Codes.ContentKilogram ? JI_CustomsQuantity :
						JI_CustomsSecondUnitQty == ABIUnitOfMeasureList.Codes.ContentKilogram ? JI_CustomsSecondQuantity :
						JI_CustomsThirdUnitQty == ABIUnitOfMeasureList.Codes.ContentKilogram ? JI_CustomsThirdQuantity : ZDecimal.Zero;
			}
		}

		bool IsCKGReportingUnit
		{
			get
			{
				return JI_CustomsUnitQty == ABIUnitOfMeasureList.Codes.ContentKilogram ||
					JI_CustomsSecondUnitQty == ABIUnitOfMeasureList.Codes.ContentKilogram ||
					JI_CustomsThirdUnitQty == ABIUnitOfMeasureList.Codes.ContentKilogram;
			}
		}

		#endregion

		#region New Properties

		#region FDA Shipper Address

		public JobDocAddress FDAShipperDocumentaryAddress
		{
			get
			{
				if (fFDAShipperDocumentaryAddress == null || fFDAShipperDocumentaryAddress.IsDeleted)
				{
					fFDAShipperDocumentaryAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.FDAShipperAddress);
					fFDAShipperDocumentaryAddress.OnRelationshipFieldsChanged += new EventHandler(FDAShipperDocumentaryAddress_OnRelationshipFieldsChanged);
				}
				return fFDAShipperDocumentaryAddress;
			}
		}
		JobDocAddress fFDAShipperDocumentaryAddress;

		void FDAShipperDocumentaryAddress_OnRelationshipFieldsChanged(object sender, EventArgs e)
		{
			MarkAsNeedingValidation();
		}

		[RelatedBusinessObject(nameof(FDAShipperAddress))]
		[List(nameof(JI_OA_FDAShipperAddress_ZAddress) + "+" + nameof(ZAddress.OrgAddress_List))]
		public ZGuid JI_OA_FDAShipperAddress
		{
			get
			{
				var result = FDAShipperDocumentaryAddress.E2_OA_Address;
				if (result.IsEmpty)
				{
					var fdaShipperAddressOrgPK = JI_OA_FDAShipperAddress_ZAddress.OrgPK;
					if (fdaShipperAddressOrgPK != ZGuid.Invalid)
					{
						if (IsChildLine)
						{
							var parentTariffLine = ParentTariffLine;
							result = parentTariffLine.JI_OA_FDAShipperAddress;

							var parentFDAShipperAddressOrgPK = parentTariffLine.JI_OA_FDAShipperAddress_ZAddress.OrgPK;
							if (fdaShipperAddressOrgPK != parentFDAShipperAddressOrgPK)
							{
								JI_OA_FDAShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(parentFDAShipperAddressOrgPK);
							}
						}
						else if (InvoiceHeader is JobComInvoiceHeader invoiceHeader)
						{
							var invoiceFDAShipperAddress = invoiceHeader.JZ_OA_FDAShipperAddress;
							if (!invoiceFDAShipperAddress.IsEmpty)
							{
								result = invoiceFDAShipperAddress;

								var invoiceFDAShipperAddressOrgPK = invoiceHeader.JZ_OA_FDAShipperAddress_ZAddress.OrgPK;
								if (fdaShipperAddressOrgPK != invoiceFDAShipperAddressOrgPK)
								{
									JI_OA_FDAShipperAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(invoiceFDAShipperAddressOrgPK);
								}
							}
						}
					}
				}
				return result;
			}
			set
			{
				var oldValue = JI_OA_FDAShipperAddress;
				FDAShipperDocumentaryAddress.E2_OA_Address = GetEffectiveValueToSet(value, Schema.JI_OA_FDAShipperAddress, JobComInvoiceHeader.Schema.JZ_OA_FDAShipperAddress);

				if (!IsCopying && JI_OA_FDAShipperAddress.IsValid)
				{
					DefaultCountryOfOriginFromOrganisationDetails(FDAShipperDocumentaryAddress.Address);
				}

				Validation.ValidateJI_OA_FDAShipperAddress();
				JI_OA_FDAShipperAddressInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JI_OA_FDAShipperAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JI_OA_FDAShipperAddress); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress JI_OA_FDAShipperAddress_ZAddress
		{
			get
			{
				if (fJI_OA_FDAShipperAddress_ZAddress == null)
				{
					fJI_OA_FDAShipperAddress_ZAddress = new ZAddress(JI_OA_FDAShipperAddressInfo);
					fJI_OA_FDAShipperAddress_ZAddress.IsOrgVisible = true;
					fJI_OA_FDAShipperAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
				}
				return fJI_OA_FDAShipperAddress_ZAddress;
			}
		}
		ZAddress fJI_OA_FDAShipperAddress_ZAddress;

		public OrgAddress FDAShipperAddress
		{
			get { return Factory.Load<OrgAddress>(JI_OA_FDAShipperAddress); }
		}

		#endregion

		internal ZDateTime ExportDateForLicenseType => !US_DateOfExport.IsValid || US_DateOfExport.IsEmpty ? Declaration.GetEffectiveDateForECR() : US_DateOfExport;

		public ZString ManufacturerCompanyName => Factory.GetValue(ref manufacturerCompanyNameCached, () => (ManufacturerAddress as IAddressDetails)?.CompanyName ?? ZString.Empty);
		CachedProperty<ZString> manufacturerCompanyNameCached;

		public ZString ManufacturerMID
		{
			get => AddressParser.GetMIDCodeFromAddressPK(manufacturerAddressPK, Factory);
			set
			{
				manufacturerAddressPK = ZGuid.Empty;
				if (!value.IsEmpty)
				{
					manufacturerAddressPK = AddressParser.GetAddressPKFromMatchingMIDCode(value, Factory);
					if (!manufacturerAddressPK.IsValid)
					{
						manufacturerAddressPK = new MIDOrganisation().CreateMIDOrganizationIfNecessary(value, Factory)?.PK ?? ZGuid.Empty;
					}
				}
				if (!manufacturerAddressPK.IsEmpty && manufacturerAddressPK.IsValid)
				{
					JI_OA_ManufacturerAddress = manufacturerAddressPK;
				}
			}
		}
		ZGuid manufacturerAddressPK;

		#region WHS Pack

		public bool IsWHSPackable
		{
			get { return JI_PartNo_CanBeSetByCustomer && JI_BondedWhsQuantity.IsEmpty && !JI_InvoiceQuantity.IsEmpty && SupplierPart != null; }
		}

		public ZDecimal JI_Calc_BondedWhsQuantity
		{
			get
			{
				if (jI_Calc_BondedWhsQuantityCached == null)
				{
					jI_Calc_BondedWhsQuantityCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						return JI_BondedWhsQuantity.IsEmpty ? (ZDecimal)WHSPackLines.Sum(x => x.B7_Calc_NoOfPackages) : JI_BondedWhsQuantity;
					});
				}
				return jI_Calc_BondedWhsQuantityCached.Value;
			}
		}
		CachedProperty<ZDecimal> jI_Calc_BondedWhsQuantityCached;

		#region JI_Calc_AllocatedQty
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|JI_Calc_AllocatedQty", Caption = "Allocated on Pack Lines Qty", ShortCaption = "Allocated Qty")]
		public ZDecimal JI_Calc_AllocatedQty
		{
			get
			{
				if (jI_Calc_AllocatedQtyCached == null)
				{
					jI_Calc_AllocatedQtyCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						return WHSPackLines.Sum(x => x.US_PackedQty);
					});
				}
				return jI_Calc_AllocatedQtyCached.Value;
			}
		}
		CachedProperty<ZDecimal> jI_Calc_AllocatedQtyCached;

		public ZPropertyInfo JI_Calc_AllocatedQtyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_AllocatedQty); }
		}
		#endregion

		#region JI_Calc_AllocatedQtyBalance
		[ResourceStringData("Enterprise.Customs.US.Business.JobComInvoiceLine|JI_Calc_AllocatedQtyBalance", Caption = "Balance")]
		public ZDecimal JI_Calc_AllocatedQtyBalance
		{
			get { return JI_InvoiceQuantity - JI_Calc_AllocatedQty; }
		}

		public ZPropertyInfo JI_Calc_AllocatedQtyBalanceInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_AllocatedQtyBalance); }
		}
		#endregion

		public IReadOnlyList<WHSPackLine> WHSPackLines
		{
			get
			{
				if (whsPackLines == null)
				{
					var declaration = Declaration;
					if (declaration != null)
					{
						whsPackLines = declaration.WHSPackLines.OfType<WHSPackLine>().Where(x => x.US_JI_InvoiceLine == PK).ToArray();
					}
				}
				return whsPackLines;
			}
		}
		WHSPackLine[] whsPackLines;

		public void RefreshWHSPackLines()
		{
			whsPackLines = null;
			BondedWhsQuantityForGUIInfo.RefreshBinding();
		}
		#endregion

		public OrgHeaderWrapper IORWrapper
		{
			get
			{
				if (cachedIORWrapper == null)
				{
					cachedIORWrapper = new CachedProperty<OrgHeaderWrapper>(Factory, () =>
					{
						OrgHeaderWrapper result = null;
						if (Declaration is JobDeclaration declaration && declaration.IsPersistent)
						{
							result = declaration.IORWrapper;
						}
						else if (IsImport && InvoiceHeader.Importer is OrgHeader ior)
						{
							result = OrgHeaderWrapper.New(ior);
						}
						return result;
					});
				}

				return cachedIORWrapper.Value;
			}
		}
		CachedProperty<OrgHeaderWrapper> cachedIORWrapper;

		public ZBool PrivilegedStatusDateVisible
		{
			get { return (US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign); }
		}

		public ZBool ADDQtyVisible
		{
			get { return US_ADDDepositRateIndicator.Contains(DepositRateIndicatorList.Codes.Specific); }
		}

		public ZBool CVDQtyVisible
		{
			get { return US_CVDDepositRateIndicator.Contains(DepositRateIndicatorList.Codes.Specific); }
		}

		public ZBool ADDValueVisible
		{
			get { return !ADDQtyVisible; }
		}

		public ZBool CVDValueVisible
		{
			get { return !CVDQtyVisible; }
		}

		#region US_ADDDepositRateDescription
		bool US_ADDDepositRateDescription_ReadOnly
		{
			get { return !DepositRateIndicatorList.DepositRateIndContainOverride(US_ADDDepositRateIndicator); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(US_ADDDepositRateDescription_ReadOnly))]
		public ZString US_ADDDepositRateDescription
		{
			get { return DepositRateIndicatorList.GetDepositRateDescription(US_ADDDepositRateIndicator, Lookups.AntidumpingDutyDepositRates, US_ADDDepositRateOverride, Lookups.AntidumpingUSCACCaseRate); }
			set
			{
				DepositRateIndicatorList.SetDepositRateDescription(value, US_ADDDepositRateIndicator, US_ADDDepositRateOverrideInfo);
				US_ADDDepositRateDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_ADDDepositRateDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.US_ADDDepositRateDescription); }
		}
		#endregion

		#region US_CVDDepositRateDescription
		bool US_CVDDepositRateDescription_ReadOnly
		{
			get { return !DepositRateIndicatorList.DepositRateIndContainOverride(US_CVDDepositRateIndicator); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(US_CVDDepositRateDescription_ReadOnly))]
		public ZString US_CVDDepositRateDescription
		{
			get { return DepositRateIndicatorList.GetDepositRateDescription(US_CVDDepositRateIndicator, Lookups.CountervailingDutyDepositRates, US_CVDDepositRateOverride, Lookups.CountervailingUSCACCaseRate); }
			set
			{
				DepositRateIndicatorList.SetDepositRateDescription(value, US_CVDDepositRateIndicator, US_CVDDepositRateOverrideInfo);
				US_CVDDepositRateDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_CVDDepositRateDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.US_CVDDepositRateDescription); }
		}
		#endregion

		public ZDecimal ADDDepositValueInLocalCurrency
		{
			get
			{
				RefCurrency currency = Factory.Load<RefCurrency>(US_RX_ADDDepositValueCurrency);
				ZDecimal result = 0m;
				if (currency != null)
				{
					result = CurrencyConverter.ConvertExact(new Money(US_ADDDepositValue, currency), Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates)).Amount;
				}
				return result;
			}
		}

		public ZGuid US_RX_ADDDepositValueCurrency
		{
			get { return JI_RX_InvoiceCurrencyReadonly; }
		}

		public ZPropertyInfo US_RX_ADDDepositValueCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.US_RX_ADDDepositValueCurrency); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_UnitOfMeasureList))]
		public ZString US_ADDUQ
		{
			get
			{
				if (uS_ADDUQCached == null)
				{
					uS_ADDUQCached = new CachedProperty<ZString>(Factory, delegate
					{
						var acCaseRate = AntidumpingDutyCase?.CaseRates.GetDepositRate(DateForADD_CVD);
						return acCaseRate != null ? acCaseRate.U6_Unit : ZString.Empty;
					});
				}
				return uS_ADDUQCached.Value;
			}
		}
		CachedProperty<ZString> uS_ADDUQCached;

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceHeaderLookups.US_UnitOfMeasureList))]
		public ZString US_CVDUQ
		{
			get
			{
				if (uS_CVDUQCached == null)
				{
					uS_CVDUQCached = new CachedProperty<ZString>(Factory, delegate
					{
						var acCaseRate = CountervailingDutyCase?.CaseRates.GetDepositRate(DateForADD_CVD);
						return acCaseRate != null ? acCaseRate.U6_Unit : ZString.Empty;
					});
				}
				return uS_CVDUQCached.Value;
			}
		}
		CachedProperty<ZString> uS_CVDUQCached;

		public ZDecimal US_ADDDepositRate
		{
			get
			{
				if (uS_ADDDepositRateCached == null)
				{
					uS_ADDDepositRateCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						if (DepositRateIndicatorList.DepositRateIndContainOverride(US_ADDDepositRateIndicator))
						{
							return US_ADDDepositRateOverride;
						}
						var acCaseRate = AntidumpingDutyCase?.GetDepositRate(DateForADD_CVD);
						return DepositRateIndicatorList.GetDepositRate(acCaseRate, US_ADDDepositRateIndicator);
					});
				}
				return uS_ADDDepositRateCached.Value;
			}
		}
		CachedProperty<ZDecimal> uS_ADDDepositRateCached;

		public ZDecimal ValueForADD
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (!US_ADDCaseNo.IsEmpty)
				{
					result = GetValueForADDForThisLineOnly();
				}

				return result;
			}
		}

		ZDecimal GetValueForADDForThisLineOnly()
		{
			ZDecimal result = ADDDepositValueInLocalCurrency;

			if (result.IsEmpty)
			{
				result = JI_CustomsValue;
			}

			return result;
		}

		public ZDecimal ValueForCVD
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (!US_CVDCaseNo.IsEmpty)
				{
					result = GetValueForCVDForThisLineOnly();
				}

				return result;
			}
		}

		ZDecimal GetValueForCVDForThisLineOnly()
		{
			ZDecimal result = CVDDepositValueInLocalCurrency;

			if (result.IsEmpty)
			{
				result = JI_CustomsValue;
			}

			return result;
		}

		public ZDecimal US_CVDDepositRate
		{
			get
			{
				if (uS_CVDDepositRateCached == null)
				{
					uS_CVDDepositRateCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						if (DepositRateIndicatorList.DepositRateIndContainOverride(US_CVDDepositRateIndicator))
						{
							return US_CVDDepositRateOverride;
						}
						var acCaseRate = CountervailingDutyCase?.GetDepositRate(DateForADD_CVD);
						return DepositRateIndicatorList.GetDepositRate(acCaseRate, US_CVDDepositRateIndicator);
					});
				}
				return uS_CVDDepositRateCached.Value;
			}
		}
		CachedProperty<ZDecimal> uS_CVDDepositRateCached;

		public ZDecimal CVDDepositValueInLocalCurrency
		{
			get
			{
				RefCurrency currency = Factory.Load<RefCurrency>(US_RX_CVDDepositValueCurrency);
				ZDecimal result = 0m;
				if (currency != null)
				{
					result = CurrencyConverter.ConvertExact(new Money(US_CVDDepositValue, currency), Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates)).Amount;
				}
				return result;
			}
		}

		public ZGuid US_RX_CVDDepositValueCurrency
		{
			get { return JI_RX_InvoiceCurrencyReadonly; }
		}

		public ZPropertyInfo US_RX_CVDDepositValueCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.US_RX_CVDDepositValueCurrency); }
		}

		internal ZDecimal TotalCustomsValueIncludingSecondaryLines
		{
			get
			{
				var result = US_CustomsValueTotal;

				foreach (JobComInvoiceLine childLine in SecondaryTariffLines)
				{
					result += childLine.US_CustomsValueTotal;
				}

				return result;
			}
		}

		ZDecimal US_CustomsValueTotal
		{
			get
			{
				var result = ZDecimal.Zero;
				if (IsSetXLine && !IsRecon)
				{
					foreach (JobComInvoiceLine childLine in ChildLines)
					{
						if (childLine.IsSetVLine)
						{
							result += childLine.US_CustomsValueTotal;
						}
					}
				}
				else
				{
					result = US_CustomsValue + TotalOriginalGoodsValueInUSD;
				}
				return result;
			}
		}

		public bool IsFTZAdmission
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsFTZAdmission;
			}
		}

		public bool IsConsumptionFTZ
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsConsumptionFTZ;
			}
		}

		public bool IsDutyFreeSPIClaimed
		{
			get
			{
				return (ImportTariff == null || ImportTariff.BecomesDutyFreeDueTo(US_SPI)) &&
					(ImportSupTariff == null || ImportSupTariff.BecomesDutyFreeDueTo(US_SPI));
			}
		}

		public bool IsRecon
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsRecon;
			}
		}

		public bool IsQuantityRequired(ZString uq)
		{
			return !uq.IsEmpty && uq != (IsExport ? AESUnitOfMeasureList.Codes.NoUnitRequired : ABIUnitOfMeasureList.Codes.NoUnitRequired);
		}

		public bool IsDutyFree
		{
			get { return JI_Calc_DutyAmount == 0m; }
		}

		public bool IsTaxOrFeePayable
		{
			get { return FeeCusCodes.GetTotal() > 0m; }
		}

		public ZString US_VisaNo_Effective
		{
			get { return GetEffectiveValueFromParentOrSingleChild<ZString>(USAddInfoSchema.US_VisaNo); }
		}

		public ZString US_TextileCategoryNo_Effective
		{
			get { return GetEffectiveValueFromParentOrSingleChild<ZString>(USAddInfoSchema.US_TextileCategoryNo); }
		}

		/// <summary>
		/// A category number or visa number can be entered at parent or its single child line
		/// </summary>
		T GetEffectiveValueFromParentOrSingleChild<T>(SchemaColumn columnName) where T : IZType
		{
			T result = (T)this[columnName];

			if (result.IsEmpty)
			{
				if (JI_ParentID.IsValid)
				{
					JobComInvoiceLine parentLine = ParentTariffLine;

					if (parentLine != null)
					{
						result = (T)parentLine[columnName];
					}
				}
				else
				{
					var childLines = SecondaryTariffLines;
					if (childLines.IsCountEqualTo(1))
					{
						result = (T)childLines.ElementAt(0)[columnName];
					}
				}
			}
			return result;
		}

		public USCCountry CountryOfOrigin_US
		{
			get { return Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, US_UC_NKCountryOfOrigin); }
		}

		public USCCountry CountryOfExport_US
		{
			get { return Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, US_UC_NKCountryOfExport); }
		}

		public ZString Description
		{
			get { return InvoiceHeader == null ? JI_LineNo.ToString() : InvoiceHeader.JZ_InvoiceNumber + " - " + JI_LineNo.ToString(); }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		public bool IsParentLine
		{
			get { return US_IsParent; }
		}

		/// <summary>
		/// Secondary tariff lines or set component lines
		/// </summary>
		public IEnumerable<JobComInvoiceLine> ChildLines
		{
			get
			{
				if (childLines == null)
				{
					if (US_IsParent)
					{
						ZQuery query = new ZQuery(JobComInvoiceLineSchema.JI_ParentID, PK);
						query.AddToFilter(JoinCondition.And, JobComInvoiceLineSchema.JI_JZ, SQLComparisonOperator.Equal, JI_JZ);
						query.AddToFilter(JoinCondition.And, JobComInvoiceLineSchema.PK, SQLComparisonOperator.NotEqual, PK);
						query.OrderBy = JobComInvoiceLineSchema.JI_LineNo.Name;
						query.FetchOnlyFromLocalCache = !IsInDatabase;

						if (IsInDatabase)
						{
							var clusterKeyQuery = new ZQuery(JobComInvoiceLineSchema.JI_ClusterKey, JI_ClusterKey);
							clusterKeyQuery.AddToFilter(query);
							query = clusterKeyQuery;
						}

						childLines = Factory.Load<JobComInvoiceLine>(query);
					}
					else
					{
						childLines = Array.Empty<JobComInvoiceLine>();
					}
				}

				return childLines.Where(line => !line.IsDeleted);
			}
		}
		JobComInvoiceLine[] childLines;

		public void RefreshChildLines()
		{
			childLines = null;
		}

		public JobComInvoiceLine ProductParentTariffLine
		{
			get { return Factory.Load<JobComInvoiceLine>(US_JI_ParentProduct); }
		}

		public IEnumerable<JobComInvoiceLine> ProductRelatedLines
		{
			get
			{
				if (productRelatedLines == null)
				{
					productRelatedLines = new List<JobComInvoiceLine>();

					if (InvoiceHeader != null)
					{
						foreach (var invoiceLine in InvoiceHeader.InvoiceLines.OfType<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo))
						{
							if (invoiceLine.US_JI_ParentProduct == PK)
							{
								productRelatedLines.Add(invoiceLine);
							}
						}
					}
				}

				return productRelatedLines.Where(line => !line.IsDeleted);
			}
		}
		List<JobComInvoiceLine> productRelatedLines;

		public void RefreshProductRelatedLines()
		{
			productRelatedLines = null;
		}

		public bool HasSecondaryTariffLines
		{
			get { return !SecondaryTariffLines.IsNullOrEmpty(); }
		}

		public IEnumerable<JobComInvoiceLine> SecondaryTariffLines
		{
			get
			{
				return IsSetXLine ? Enumerable.Empty<JobComInvoiceLine>() : ChildLines;
			}
		}

		/// <summary>
		/// Is this invoice line a normal (stand-alone) or parent or set X line?
		/// </summary>
		public bool IsNormalOrParentOrSetXLine
		{
			get { return JI_ParentID.IsEmpty && !IsSetVLine; }
		}

		public USCACCase AntidumpingDutyCase
		{
			get { return Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, US_ADDCaseNo); }
		}

		public USCACCase CountervailingDutyCase
		{
			get { return Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, US_CVDCaseNo); }
		}

		public ZString JI_FDARequirementDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.FDA)
					? IsACEFDARelevant
						? OGARequirementCalculator.ACEFDARequirementDesc
						: OGARequirementCalculator.FDARequirementDesc
					: ZString.Empty;
			}
		}

		internal ZString JI_FDARequirementCode
		{
			get { return IsACEFDARelevant ? OGARequirementCalculator.ACEFDARequirementCode : OGARequirementCalculator.FDARequirementCode; }
		}

		public ZPropertyInfo JI_FDARequirementDescInfo
		{
			get { return GetZPropertyInfo(nameof(JI_FDARequirementDesc)); }
		}

		public ZString US_DOTRequirementDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.DOT)
					? OGARequirementCalculator.DOTRequirementDesc
					: ZString.Empty;
			}
		}

		public ZPropertyInfo US_DOTRequirementDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_DOTRequirementDesc); }
		}

		public ZString US_FCCRequirementDesc
		{
			get { return ZString.Empty; }
		}

		internal OGARequirementCalculator OGARequirementCalculator
		{
			get { return fOGARequirementCalculator ?? (fOGARequirementCalculator = new OGARequirementCalculator(Factory, () => ImportTariffForPGA, () => ImportSupTariff, () => EffectiveDateForDutyRate, () => US_UC_NKCountryOfOrigin)); }
		}
		OGARequirementCalculator fOGARequirementCalculator;

		#region FDA Value Running Totals

		public ZDecimal FDAValueInvCurrRunningTotal
		{
			get
			{
				if (fFDAValueInvCurrRunningTotalCached == null)
				{
					fFDAValueInvCurrRunningTotalCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var result = ZDecimal.Zero;
						foreach (FDA fda in FDAs)
						{
							if (!fda.IsDeleted && !fda.IsDeleting)
							{
								result += fda.US_InvCurrFDAValue;
							}
						}
						return result;
					});
				}
				return fFDAValueInvCurrRunningTotalCached.Value;
			}
		}
		CachedProperty<ZDecimal> fFDAValueInvCurrRunningTotalCached;

		internal ZDecimal ACE_FDAValueInvCurrRunningTotal
		{
			get
			{
				if (aceFDAValueInvCurrRunningTotalCached == null)
				{
					aceFDAValueInvCurrRunningTotalCached = new CachedProperty<ZDecimal>(Factory, () =>
					{
						var result = ZDecimal.Zero;
						foreach (ACEFDA fda in ACE_FDALines)
						{
							if (!fda.IsDeletedOrBeingDeleted())
							{
								result += fda.US_InvCurrValue;
							}
						}
						return result;
					});
				}
				return aceFDAValueInvCurrRunningTotalCached.Value;
			}
		}
		CachedProperty<ZDecimal> aceFDAValueInvCurrRunningTotalCached;

		public ZString FDAValueUSDRunningTotalString
		{
			get { return IsOGAValueUpToDate ? FDAValueUSDRunningTotal.ToString(0) : "..."; }
		}

		public ZPropertyInfo FDAValueUSDRunningTotalStringInfo
		{
			get { return GetZPropertyInfo(nameof(FDAValueUSDRunningTotalString)); }
		}

		internal ZDecimal FDAValueUSDRunningTotal
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (FDA fda in FDAs)
				{
					if (!fda.IsDeleted && !fda.IsDeleting)
					{
						result += fda.US_FDAValue;
					}
				}
				return result;
			}
		}

		internal bool IsOGAValueUpToDate
		{
			get
			{
				bool result = false;

				var declaration = this.Declaration;

				if (declaration != null)
				{
					if (declaration.IsOGAValueGoingToBeCalculated)
					{
						result = CusEntryLine != null && !declaration.MergeManager.RequiresMerge;
					}
					else
					{
						result = !declaration.ApportionmentDirty;
					}
				}

				return result;
			}
		}

		#endregion

		public bool UseScheduleB
		{
			get
			{
				bool result = false;
				if (Declaration != null)
				{
					if (Declaration.IsExport || Declaration.IsDrawback)
					{
						result = US_TariffType == TariffTypeList.Codes.ScheduleB;
					}
				}

				return result;
			}
		}

		public bool UseHTSForExport
		{
			get
			{
				return IsExport && US_TariffType == TariffTypeList.Codes.HTS;
			}
		}

		/// <summary>
		/// This indicates if fees will be exempt because it is an organic product. US_CottonCertificateNo happens to be used to store either a cotton certificate number or organic certificate number for ACS.
		/// </summary>
		public bool IsAMSFeeExempt
		{
			get
			{
				return IsACE && LicenceAndPermits.ContainsCode(LicencePermitTypeList.Codes._22) ||
					!IsACE && !US_CottonCertificateNo.IsEmpty && US_CottonCertificateNo != CottonFeeCalculator.ExemptCottonFeeCertificate;
			}
		}

		public bool IsRaspberryFeeExempt
		{
			get { return IsACE && LicenceAndPermits.ContainsCode(LicencePermitTypeList.Codes._23) || IsAMSFeeExempt; }
		}

		public bool HasCottonCertificate
		{
			get { return IsACE && LicenceAndPermits.ContainsCode(LicencePermitTypeList.Codes._12) || !IsACE && !US_CottonCertificateNo.IsEmpty; }
		}

		internal List<ZString> CombinedCottonFeeExemptIndicators
		{
			get
			{
				List<ZString> result = new List<ZString>();

				if (!US_CottonFeeExempt.IsEmpty)
				{
					result.Add(US_CottonFeeExempt);
				}

				foreach (JobComInvoiceLine secondaryLine in SecondaryTariffLines)
				{
					if (!secondaryLine.US_CottonFeeExempt.IsEmpty && !result.Contains(secondaryLine.US_CottonFeeExempt))
					{
						result.Add(secondaryLine.US_CottonFeeExempt);

						if (result.Count > 1)
						{
							break;
						}
					}
				}

				return result;
			}
		}

		public bool IsCottonFeeExemptIndicated
		{
			get
			{
				List<ZString> result = CombinedCottonFeeExemptIndicators;
				return result.Count == 1 && result[0] == YesNoDefaultList.Codes.Yes;
			}
		}

		public bool IsLimitedReportingExportCode
		{
			get { return !US_ExportCode.IsEmpty && Lookups.LimitedReportingExportCodeList.ContainsCode(US_ExportCode); }
		}

		public bool IsCountryOfOriginCanada
		{
			get
			{
				ZString origin = US_UC_NKCountryOfOrigin;
				return CanadaProvinceTerritoryCodes.IsCanadianProvince(origin) || CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(origin) || origin == Core.Constants.CountryCodes.Canada;
			}
		}

		public USCTariff ImportTariff
		{
			get
			{
				return IsExport ? null : Factory.GetCachedValue(JI_Tariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + EffectiveDateForDutyRate.ToString(),
					delegate
					{
						var result = new USCTariff.Loader(Factory).LoadBestMatch(JI_Tariff, EffectiveDateForDutyRate);
						return result;
					});
			}
		}

		public USCTariff ImportSupTariff => IsExport ? null : ImportHelper.ImportSupTariff;

		public USCTariff ImportSupAdditionalTariff1 => IsExport ? null : ImportHelper.ImportSupAdditionalTariff1;

		public USCTariff ImportSupAdditionalTariff2 => IsExport ? null : ImportHelper.ImportSupAdditionalTariff2;

		public USCTariff ImportSupAdditionalTariff3 => IsExport ? null : ImportHelper.ImportSupAdditionalTariff3;

		public USCTariff ImportSupAdditionalTariff4 => IsExport ? null : ImportHelper.ImportSupAdditionalTariff4;

		public USCTariff ImportSupAdditionalTariff5 => IsExport ? null : ImportHelper.ImportSupAdditionalTariff5;

		public TariffView UniversalImportTariff
			=> IsExport ? null : new TariffView.Loader(Factory).LoadMostRecentCachedTariff(CustomsCountryCode, Universal.Constants.TariffTypes.HarmonizedSystem, JI_Tariff, ZDate.Today);

		public TariffView ExportTariff
		 => IsExport ? new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.Export, JI_Tariff, EffectiveDateForDutyRate) : null;

		public TariffView ScheduleBTariff
		 => IsExport & UseScheduleB ? new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB, JI_Tariff, EffectiveDateForDutyRate) : null;

		public TariffView TariffExpirationDateWinin30Days
		{
			get
			{
				var tariffType = UseScheduleB ? Universal.Constants.TariffTypes.ScheduleB : Universal.Constants.TariffTypes.Export;
				var result = UseScheduleB ? ScheduleBTariff : ExportTariff;
				if (IsExport && result == null)
				{
					var tariff = new TariffView.Loader(Factory).LoadLatestCachedTariff(Core.Constants.CountryCodes.UnitedStates, tariffType, JI_Tariff);
					var validDate = EffectiveDateForDutyRate;
					if (tariff != null && tariff.ZZ1_EndDate is ZDateTime tariffEndDate && tariffEndDate.IsValid && validDate.IsValid && (validDate - tariffEndDate).TotalDays <= TariffExpireDaysThresHold)
					{
						result = tariff;
					}
				}
				return result;
			}
		}

		public const int TariffExpireDaysThresHold = 30;

		public TariffView SupTariff => IsExport ? null : new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem, US_SupTariff, EffectiveDateForDutyRate);

		USCTariff FTZCurrentTariff
		{
			get
			{
				return IsExport ? null : Factory.GetCachedValue(US_FTZCurrentTariff.PadRight(USCTariff.Schema.UE_TariffMaxLength) + FTZCurrentDutyDate.ToString(),
					delegate
					{
						return new USCTariff.Loader(Factory).LoadBestMatch(US_FTZCurrentTariff, FTZCurrentDutyDate);
					});
			}
		}

		public ZBool FTZCurrentTariffVisible
		{
			get { return IsACECargoCertificationMode && US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign; }
		}

		public USCTariff ImportTariffForPGA
		{
			get { return HasFTZCurrentTariff ? FTZCurrentTariff : ImportTariff; }
		}

		public ZBool IsSoftwoodLumberSection804FarmBillRequirement
		{
			get
			{
				var declaration = Declaration;
				return declaration != null &&
					IsSoftwoodLumberAgreementTariff
					&& EntryTypeList.IsSoftwoodLumberSection803FarmBillRequired(declaration.US_EntryType);
			}
		}

		public ZBool IsSoftwoodLumberAgreementTariff
		{
			get
			{
				var importTariff = ImportTariff;
				var result = importTariff != null && importTariff.RequiresCanadianLumberPermit;
				if (!result)
				{
					var importSupTariff = ImportSupTariff;
					result = importSupTariff != null && importSupTariff.RequiresCanadianLumberPermit;
				}
				return result;
			}
		}

		public LicenceAndPermit KRExportSteelCertificateNumber
		{
			get { return LicenceAndPermits.OfType<LicenceAndPermit>().FirstOrDefault(x => x.CY_Code == LicencePermitTypeList.Codes.KR && !x.CY_Data.IsEmpty); }
		}

		public ZString CommercialDescription
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder(JI_Description);
				result.AppendIfNotEmpty(JI_ExtraInfoForClassification);
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public bool IsEntrySummaryValidationMode
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null && invoiceHeader.IsEntrySummaryValidationMode;
			}
		}

		public bool IsACEFDARelevant
		{
			get { return Declaration != null && Declaration.IsACEFDARelevant; }
		}

		public bool IsNHTSARelevant
		{
			get { return Declaration != null && Declaration.IsNHTSARelevant; }
		}

		public bool IsACEEntrySummaryValidationMode
		{
			get
			{
				var result = false;
				var declaration = Declaration;
				if (declaration != null && declaration.IsPersistent)
				{
					result = declaration.IsACE && declaration.IsEntrySummaryValidationMode;
				}
				return result;
			}
		}

		public bool IsCargoReleaseValidationMode
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null && invoiceHeader.IsCargoReleaseValidationMode;
			}
		}

		public bool IsACECargoReleaseValidationMode
		{
			get
			{
				var result = false;
				var declaration = Declaration;
				if (declaration != null && declaration.IsPersistent)
				{
					result = declaration.IsACECargoReleaseValidationMode;
				}
				return result;
			}
		}

		public bool IsStandAlonePriorNoticeMode
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null && invoiceHeader.IsStandAlonePriorNoticeMode;
			}
		}

		public bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get { return IsEntrySummaryValidationMode || IsCargoReleaseValidationMode; }
		}

		public ZBool IsFDADeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_FDAIndicator); }
		}

		public ZBool IsAMSDisclaimed
		{
			get { return OGAIndicatorList.IsToBeDisclaimed(US_AMSInd); }
		}

		public ZBool IsNOPDisclaimed
		{
			get { return OGAIndicatorList.IsToBeDisclaimed(US_NOPInd); }
		}

		public ZBool IsFCCDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_FCCIndicator); }
		}

		public ZBool IsDOTDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_DOTIndicator); }
		}

		public RefCountry CountryOfOriginRefCountry
		{
			get { return RefCountry.LoadFromCountryCode(Factory, US_UC_NKCountryOfOrigin); }
		}

		public ZBool IsOriginFromEUN => CountryOfOriginRefCountry?.IsPartOfEuropeanUnion ?? false;

		public ZBool IsSteelOriginFromEUN => IsOnlySteelProductAvailable && IsOriginFromEUN;

		public ZBool IsSteelOriginFromEUNForAnySupTariff => ApplicableSupTariffs != null && ApplicableSupTariffs.Any(x => USRefTariffDataLoader.IsTradeGroupEUNForSupTariff(Factory, CustomsCountryCode, x.Tariff, EffectiveDateForDutyRate));

		public ZBool IsSteelProductWithApplicableSupTariff => IsOnlySteelProductAvailable && ApplicableSupTariffs.Length > 0;

		public bool IsSteelOriginFromSpecificCountriesOtherThanEU => IsOnlySteelProductAvailable && (US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Japan || US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.UnitedKingdom
				|| US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Argentina || US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.Brazil || US_UC_NKCountryOfOrigin == Core.Constants.CountryCodes.KoreaSouth);

		public RefCountry CountryOfExportEffectiveRefCountry
		{
			get { return RefCountry.LoadFromCountryCode(Factory, US_UC_NKCountryOfExport); }
		}

		#region US_ManifestUQ

		[ReadOnlyMember(nameof(US_ManifestUQ_ReadOnly))]
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.ManifestUQList))]
		public override ZString US_ManifestUQ
		{
			get
			{
				var result = ZString.Empty;
				var declaration = Declaration;
				if (declaration != null)
				{
					if (declaration.IsConsumptionFTZ)
					{
						result = base.US_ManifestUQ;
						if (result.IsEmpty)
						{
							result = declaration.JE_TotalNoOfPacksPackType;
						}
					}
					else
					{
						result = declaration.PrimaryMasterBill != null ? declaration.PrimaryMasterBill.UniqueManifestUQ : ZString.Empty;
					}
				}

				return result;
			}
			set
			{
				base.US_ManifestUQ = value;
			}
		}

		bool US_ManifestUQ_ReadOnly
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && !declaration.IsConsumptionFTZ;
			}
		}

		#endregion

		#region Properties For Binding

		public ZString EntryNumberAndMergeLineNumber
		{
			get { return JI_Calc_EntryNumber + "/" + JI_Calc_MergedLineNumber.PadLeft(CusEntryLine.EntryLineNumberLength, '0'); }
		}

		public ZPropertyInfo EntryNumberAndMergeLineNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EntryNumberAndMergeLineNumber); }
		}

		public override ZPropertyInfo ExposedCusEntryLineErrorPropertyInfo
		{
			get { return EntryNumberAndMergeLineNumberInfo; }
		}

		public CusEntryLine GetMatchingSupEntryLine(ZString supplementaryTariff)
		{
			CusEntryLine result = null;

			if (!supplementaryTariff.IsEmpty && supplementaryTariff != TariffViewAsCodeDescription.NotApplicableCode && CusEntryLine is CusEntryLine entryLine)
			{
				result = entryLine.ParentLine;

				var line = entryLine.ParentLine ?? entryLine;
				if (line.ChildLines.Count > 0)
				{
					if (line.CL_AdValoremTariff == supplementaryTariff)
					{
						result = line;
					}
					else
					{
						result = line.ChildLines.FirstOrDefault(x => x.CL_AdValoremTariff == supplementaryTariff);
					}
				}
			}

			return result;
		}

		public ZString DutyFormula
		{
			get
			{
				ZString result = MergeAdvice;

				if (CusEntryLine is CusEntryLine line && Declaration is JobDeclaration declaration && !declaration.MergeManager.RequiresMergeBeforeSave)
				{
					result = AppendixFCalculator.CombineTwoRateStrings(line.US_DutyRateDesc, ZString.Empty);

					void AppendSupDutyRateString(ZString supplementaryTariff)
					{
						var supEntryLine = GetMatchingSupEntryLine(supplementaryTariff);
						if (supEntryLine != null)
						{
							result = AppendixFCalculator.CombineTwoRateStrings(result, supEntryLine.US_DutyRateDesc);
						}
					}

					AppendSupDutyRateString(US_SupTariff);
					AppendSupDutyRateString(US_SupAdditionalTariff5);
					AppendSupDutyRateString(US_SupAdditionalTariff4);
					AppendSupDutyRateString(US_SupAdditionalTariff3);
					AppendSupDutyRateString(US_SupAdditionalTariff2);
					AppendSupDutyRateString(US_SupAdditionalTariff1);
				}

				return result;
			}
		}

		public ZString DutyFormulaDescription
		{
			get
			{
				ZString result = MergeAdvice;
				if (CusEntryLine != null && Declaration != null && !Declaration.MergeManager.RequiresMergeBeforeSave)
				{
					result = ZString.Empty;
					if (!IsSecondaryTariffLine && !IsParentLine && HasEmptySupTariff)
					{
						result = ImportTariff != null ? Factory.GetCachedValue<ComputationCodeList>().GetDescriptionFromCode(ImportTariff.UE_DutyComputationCode) : string.Empty;
					}
					else
					{
						result = CompositeDutyCalculation;
					}
				}

				return result;
			}
		}

		internal const string CompositeDutyCalculation = "Composite Duty Calculation. No Formula Description available";

		internal const string MergeAdvice = "Please merge. Click Brokerage > Generate Entries(Merge) or save.";

		#region ManufacturerNameAndID
		public ZString ManufacturerNameAndID
		{
			get
			{
				if (manufacturerNameAndIDCached == null)
				{
					manufacturerNameAndIDCached = new CachedProperty<ZString>(Factory, delegate
					{
						return ManufacturerAddress != null ? ManufacturerAddress.EffectiveCompanyNameTruncated + " (" + ManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates) + ")" : "";
					});
				}
				return manufacturerNameAndIDCached.Value;
			}
		}
		CachedProperty<ZString> manufacturerNameAndIDCached;

		public ZPropertyInfo ManufacturerNameAndIDInfo
		{
			get { return GetZPropertyInfo(Schema.ManufacturerNameAndID); }
		}
		#endregion

		#region FreightInLocalCurrency

		public ZDecimal FreightInLocalCurrency
		{
			get
			{
				ZDecimal result = 0;
				if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null)
				{
					result = ConvertToLocalAmountExact(GetEffectiveMoney(new Money(JI_Calc_FreightInInvoiceCurr, InvoiceHeader.Invoice_Currency))).Amount;
				}
				return result;
			}
		}

		public ZPropertyInfo FreightInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(FreightInLocalCurrency)); }
		}

		#endregion

		#region InsuranceInLocalCurrency

		public ZDecimal InsuranceInLocalCurrency
		{
			get
			{
				ZDecimal result = 0;
				if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null)
				{
					result = ConvertToLocalAmountExact(GetEffectiveMoney(new Money(JI_Calc_InsuranceInInvoiceCurr, InvoiceHeader.Invoice_Currency))).Amount;
				}
				return result;
			}
		}

		#endregion

		#region FOBValueInLocalCurrency

		public ZDecimal FOBValueInLocalCurrency
		{
			get
			{
				ZDecimal result = 0;
				if (InvoiceHeader != null && InvoiceHeader.Invoice_Currency != null)
				{
					result = ConvertToLocalAmountExact(GetEffectiveMoney(new Money(JI_Calc_FOB, InvoiceHeader.Invoice_Currency))).Amount;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Cotton Fee Exempt / US_CottonCertificateNo

		public override ZString US_CottonFeeExempt
		{
			get
			{
				ZString result = base.US_CottonFeeExempt;

				if (result.IsEmpty)
				{
					result = GetEffectiveCottonFeeExemptFromParentTariffLine();
				}

				return result;
			}
			set
			{
				ZString parentValue = GetEffectiveCottonFeeExemptFromParentTariffLine();

				base.US_CottonFeeExempt = value == parentValue ? ZString.Empty : value;

				if (US_CottonFeeExempt == YesNoDefaultList.Codes.Yes)
				{
					US_CottonCertificateNo = ZString.Empty;
				}

				foreach (JobComInvoiceLine secondaryLine in SecondaryTariffLines)
				{
					if (secondaryLine.US_CottonFeeExempt == US_CottonFeeExempt)
					{
						secondaryLine.US_CottonFeeExempt = ZString.Empty;
					}
				}
			}
		}

		ZString GetEffectiveCottonFeeExemptFromParentTariffLine()
		{
			ZString result = ZString.Empty;

			if (IsSecondaryTariffLine && ImportTariff != null && ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton))
			{
				result = ParentTariffLine.US_CottonFeeExempt;
			}

			return result;
		}

		public bool US_CottonCertificateNo_ReadOnly
		{
			get { return IsCottonFeeExemptIndicated; }
		}

		#endregion

		public int TotalNoOfSequences
		{
			get
			{
				if (totalNoOfSequencesCached == null)
				{
					totalNoOfSequencesCached = new CachedProperty<int>(Factory, delegate
					{
						var invoiceLine = ParentTariffLine ?? this;

						var ranges = invoiceLine.LineGroupingRanges;

						return ranges.TotalNoOfSequences * (InvoiceHeader.US_GenAIIForSup && !HasEmptySupTariff ? 2 : 1);
					});
				}
				return totalNoOfSequencesCached.Value;
			}
		}
		CachedProperty<int> totalNoOfSequencesCached;

		public bool DoesAIIRequireRegeneration
		{
			get { return TotalNoOfSequences != AIILines.Count; }
		}

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "CY_ParentID,CY_ParentTableCode", DisableCopyMethodLink = true)]
		public InvoiceLineGroupingRange FirstGroupingRange
		{
			get
			{
				if (firstGroupingRangeCached == null)
				{
					firstGroupingRangeCached = new CachedProperty<InvoiceLineGroupingRange>(Factory, delegate
					{
						InvoiceLineGroupingRange firstGroupingRange = null;
						if (IsValidMessageTypeForAIILine)
						{
							JobComInvoiceLine invoiceLine = ParentTariffLine;
							if (invoiceLine == null)
							{
								firstGroupingRange = LineGroupingRanges.GetRangeWithLowestSequenceNo();
								if (firstGroupingRange == null && !IsLineGroupingEnabled)
								{
									firstGroupingRange = LineGroupingRanges.AddNew();
									firstGroupingRange.US_StartSequenceNo = JI_LineNo;
								}
							}
							else
							{
								firstGroupingRange = invoiceLine.FirstGroupingRange;
							}
						}
						return firstGroupingRange;
					});
				}
				return firstGroupingRangeCached.Value;
			}
		}
		CachedProperty<InvoiceLineGroupingRange> firstGroupingRangeCached;

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "B7_ParentID,B7_ParentTableCode", DisableCopyMethodLink = true)]
		public AIILine FirstAIILine
		{
			get
			{
				if (firstAIILineCached == null)
				{
					firstAIILineCached = new CachedProperty<AIILine>(Factory, delegate
					{
						if (!IsValidMessageTypeForAIILine)
						{
							firstAIILine = null;
						}
						else
						{
							bool needsReload = firstAIILine == null || firstAIILine.IsDeleted || firstAIILine.B7_ParentID != PK;
							if (!needsReload)
							{
								var range = FirstGroupingRange;
								needsReload = range != null && firstAIILine.US_CY_LineGroupRef != range.PK;
							}
							if (needsReload)
							{
								ClearFirstAIILineReference();
								if (!IsValidForAII)
								{
									firstAIILine = Factory.GetNull<AIILine>();
								}
								else
								{
									firstAIILine = GetInvoiceLineLowestAIILineNo();
									if (firstAIILine == null || firstAIILine.IsDeleted)
									{
										firstAIILine = CurrentNonCommittedAIILine;
									}
									if ((firstAIILine == null || firstAIILine.IsDeleted) && !IsLineGroupingEnabled)
									{
										try
										{
											firstAIILineAccessCount++;
											firstAIILine = AIILines.AddNew(FirstGroupingRange);
										}
										finally
										{
											firstAIILineAccessCount--;
										}
									}

									if (!IsLineGroupingEnabled)
									{
										RegisterEditableChildObject(firstAIILine);
									}
								}
							}
						}
						return firstAIILine;
					});
				}
				return firstAIILineCached.Value;
			}
		}
		AIILine firstAIILine;
		CachedProperty<AIILine> firstAIILineCached;
		internal AIILine CurrentNonCommittedAIILine;
		int firstAIILineAccessCount;

		void ResetReconOriginLineNo()
		{
			if (!US_R_OrigEntryLineNo.IsEmpty && (Declaration?.IsACERecon ?? false))
			{
				US_R_OrigEntryLineNo = ZString.Empty;
			}
		}

		void UpdateReconOriginLineNo()
		{
			if (!IsCopying && !JI_LineNo.IsEmpty && US_R_OrigEntryLineNo.IsEmpty && JI_LineNo < 1000 && (Declaration?.IsACERecon ?? false))
			{
				var newOrigEntryLineNo = JI_LineNo.ToString();
				if (!InvoiceHeader.InvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.US_R_OrigEntryLineNo == newOrigEntryLineNo))
				{
					US_R_OrigEntryLineNo = newOrigEntryLineNo;
				}
			}
		}

		bool IsValidMessageTypeForAIILine
		{
			get { return Declaration != null && Declaration.IsFormalImport && !Declaration.IsACECargoCertificationMode; }
		}

		public ZString US_FSISReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.FSIS)
					? OGARequirementCalculator.FSISRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_ODSReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.ODS)
					? OGARequirementCalculator.ODSRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_TSCAReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.TSCA)
					? OGARequirementCalculator.TSCARequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_VNEReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.VNE)
					? OGARequirementCalculator.VNERequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_PSTReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.PST)
					? OGARequirementCalculator.PSTRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_HFCReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.HFC)
					? OGARequirementCalculator.HFCRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_LaceyRequirementDesc
		{
			get
			{
				return !IsInformal && IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.Lacey) && !EntryTypeList.IsInformal(ImportEntryType)
					? IsACECargoCertificationMode
						? OGARequirementCalculator.ACELaceyRequirementDesc
						: OGARequirementCalculator.LaceyRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_APHISReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.APHIS)
					? OGARequirementCalculator.APHISRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_FWSReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.FWS)
					? OGARequirementCalculator.FWSRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_NMFS370ReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes._370)
					? OGARequirementCalculator.NMFS370RequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_NMFSCOAReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.COA)
					? OGARequirementCalculator.NMFSCOARequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_NMFSAMRReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.AMR)
					? OGARequirementCalculator.NMFSAMRRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_NMFSHMSReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.HMS)
					? OGARequirementCalculator.NMFSHMSRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_NMFSSIMReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.SIMP)
					? OGARequirementCalculator.NMFSSIMRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_OMCReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.OMC)
					? OGARequirementCalculator.OMCRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_NHTSAReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.NHTSA)
					? OGARequirementCalculator.NHTSARequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_AMSReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.AMS)
					? OGARequirementCalculator.AMSRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_NOPReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.NOP)
					? OGARequirementCalculator.NOPRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_CPSCReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.CPSC)
					? OGARequirementCalculator.CPSCRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_TTBReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.TTB)
					? OGARequirementCalculator.TTBRequirementDesc
					: ZString.Empty;
			}
		}

		public ZString US_DEAReqDesc
		{
			get
			{
				return IsPGAIndicatorAllowedToBeDefaulted(GovernmentAgencyProgramCodeList.Codes.DEA)
					? OGARequirementCalculator.DEARequirementDesc
					: ZString.Empty;
			}
		}

		public ZString FSISForm95401Bills
		{
			get
			{
				var result = ZString.Empty;
				var bills = Declaration.Bills;
				var packages = Declaration.Packages;
				if (bills.Count > 0 && packages.Count > 0)
				{
					var noContainersForInvoiceLineRelatedPackage = true;
					var printAllBills = ZString.Empty;
					var containersForInvoiceLine = ContainersForInvoiceLinesForBindingOnly.OfType<NonPersistentCusContainer>().Where(x => x.IsForInvoiceLine);
					foreach (Bill bill in bills)
					{
						var numberToPrint = bill.NumberForFSISForm95401Printing;
						if (!numberToPrint.IsEmpty)
						{
							var relatedContainers = packages.OfType<Package>().Where(x => x.CW_HouseBill == bill.CU_BillUniqueCode);
							if (containersForInvoiceLine.Any(container => relatedContainers.Any(package => container.ContainerNumber == package.CW_ContainerNoOrEquipmentNo)))
							{
								noContainersForInvoiceLineRelatedPackage = false;
								result = AppendBillNumberForFSISForm95401(result, bill);
							}
							else if (noContainersForInvoiceLineRelatedPackage)
							{
								printAllBills = AppendBillNumberForFSISForm95401(printAllBills, bill);
							}
						}
					}
					if (noContainersForInvoiceLineRelatedPackage)
					{
						result = printAllBills;
					}
				}
				return result.TrimEnd();
			}
		}

		ZString AppendBillNumberForFSISForm95401(ZString existingBillNumbersString, Bill bill)
		{
			var parentBill = bill.ParentBill;
			if (parentBill != null)
			{
				var parentParentBill = parentBill.ParentBill;
				if (parentParentBill != null && !parentParentBill.NumberForFSISForm95401Printing.IsEmpty && !existingBillNumbersString.Contains(parentParentBill.NumberForFSISForm95401Printing))
				{
					existingBillNumbersString += parentParentBill.NumberForFSISForm95401Printing + " ";
				}

				if (!parentBill.NumberForFSISForm95401Printing.IsEmpty && !existingBillNumbersString.Contains(parentBill.NumberForFSISForm95401Printing))
				{
					existingBillNumbersString += parentBill.NumberForFSISForm95401Printing + " ";
				}
			}

			if (!bill.NumberForFSISForm95401Printing.IsEmpty && !existingBillNumbersString.Contains(bill.NumberForFSISForm95401Printing))
			{
				existingBillNumbersString += bill.NumberForFSISForm95401Printing + " ";
			}

			return existingBillNumbersString;
		}

		ZBool HasFTZCurrentTariff
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsConsumptionFTZ && FTZCurrentTariff != null;
			}
		}

		internal ZBool IsImportViaBIRD;

		public ZBool IsDCSRequired
		{
			get
			{
				return US_LicenseType != USAESLicenseCode.Codes.C43 && US_LicenseType != USAESLicenseCode.Codes.C45 && US_ECCN != "EAR99";
			}
		}

		public ZBool PrintECCN
		{
			get
			{
				Regex reg9x515 = new Regex(@"^9[a-zA-Z0-9]515");
				Regex reg600 = new Regex(@"^[a-zA-Z0-9]{2}6[a-zA-Z0-9]{2}");
				return reg9x515.IsMatch(US_ECCN) || reg600.IsMatch(US_ECCN);
			}
		}

		public ZBool IsITARCompliance
		{
			get { return US_LicenseType.StartsWith("S", StringComparison.OrdinalIgnoreCase); }
		}

		public ZBool IsOnlySteelProductAvailable
		{
			get { return JI_Tariff.StartsWith("72", StringComparison.OrdinalIgnoreCase) || JI_Tariff.StartsWith("73", StringComparison.OrdinalIgnoreCase); }
		}

		public ZBool IsOnlyAluminumProductAvailable
		{
			get { return JI_Tariff.StartsWith("76", StringComparison.OrdinalIgnoreCase); }
		}

		public ZBool IsEmbroideryChildTariffLine => Factory.GetValue(ref isEmbroideryChildTariffLineCached, () => ParentTariffLine is JobComInvoiceLine parentTariffLine && USRefTariffDataLoader.IsEmbroideryTariff(Factory, parentTariffLine.JI_Tariff, EffectiveDateForDutyRate));
		CachedProperty<ZBool> isEmbroideryChildTariffLineCached;

		public ZBool IsCBMAProductClaim => Factory.GetValue(ref isCBMAProductClaimCached, () => (Declaration?.IsACE ?? false) && US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.C);
		CachedProperty<ZBool> isCBMAProductClaimCached;

		public ZBool IsCBMAProductClaimAndIsCBMA23Effective => Factory.GetValue(ref isCBMAProductClaimAndIsCBMA23Effective, () => IsCBMAProductClaim && IsCBMA23Effective);
		CachedProperty<ZBool> isCBMAProductClaimAndIsCBMA23Effective;

		public ZBool IsCBMAProductClaimAndIsNotCBMA23Effective => Factory.GetValue(ref isCBMAProductClaimAndIsNotCBMA23Effective, () => IsCBMAProductClaim && !IsCBMA23Effective);
		CachedProperty<ZBool> isCBMAProductClaimAndIsNotCBMA23Effective;

		public ZDecimal TTBConfirmationRate => Factory.GetValue(ref tTBConfirmationRateCached, () =>
		{
			var result = ZDecimal.Zero;
			var taxRateS = IsCBMA23Effective ? US_TTBRateDesignationCode : US_TaxRateS;
			var taxRateList = IsCBMA23Effective ? AddInfoLookups.CBMATaxRateList : AddInfoLookups.TaxRateList;
			if (!taxRateS.IsEmpty && taxRateList[taxRateS] is CBMATaxRate taxRate)
			{
				result = taxRate.TTBConfirmationRate;
			}
			return result;
		});
		CachedProperty<ZDecimal> tTBConfirmationRateCached;

		public ZBool IsAluminumSmeltAndCastCountryClaimed => Factory.GetValue(ref isAluminumSmeltAndCastCountryClaimed, () => US_Prim_NA || !US_RN_NKPrimCtry.IsEmpty || US_Sec_NA || !US_RN_NKSecCtry.IsEmpty || !US_RN_NKCastCtry.IsEmpty);
		CachedProperty<ZBool> isAluminumSmeltAndCastCountryClaimed;

		public ZBool IsAluminumSmeltEffective => ZZCustomsFunctionality.IsAluminumSmeltEffective(EffectiveDateForDutyRate);

		public ZBool TariffMatchesSmeltCondition => USRefTariffDataLoader.IsTariffMatchCondition(Factory, JI_Tariff, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, UniversalReferenceConstants.TariffConditionTypes.Codes.SMELT, ZString.Empty, ZString.Empty, ZString.Empty, EffectiveDateForDutyRate);
		public ZBool TariffMatchesMeltCondition => USRefTariffDataLoader.IsTariffMatchCondition(Factory, JI_Tariff, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT, ZString.Empty, ZString.Empty, ZString.Empty, EffectiveDateForDutyRate);

		public ZBool SupTariffMatchesAutoCondition => USRefTariffDataLoader.IsTariffMatchCondition(Factory, US_SupTariff, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.AUTO, TariffConditionValueTypes.Codes.Entry, EntryTypeList.Codes.TemporaryImportationBond, ZString.Empty, EffectiveDateForDutyRate);

		public ZBool TariffMatchesFishingCondition => USRefTariffDataLoader.IsTariffMatchCondition(Factory, JI_Tariff, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Fishing, ZString.Empty, ZString.Empty, EffectiveCountryOfOrigin, EffectiveDateForDutyRate);

		public ZBool TariffMatchesMiningCondition => USRefTariffDataLoader.IsTariffMatchCondition(Factory, JI_Tariff, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Mining, ZString.Empty, ZString.Empty, EffectiveCountryOfOrigin, EffectiveDateForDutyRate);

		public RefCusCondition TariffMELTCondition => USRefTariffDataLoader.GetApplicableCondition(Factory, US_SupTariff, UniversalReferenceConstants.TariffConditionTypes.Codes.MELT, EffectiveDateForDutyRate);

		#region ForeignExporterOrgPK

		public ZGuid ForeignExporterOrgPK
		{
			get { return JI_OA_ExporterAddress_ZAddress.OrgPK; }
			set { JI_OA_ExporterAddress_ZAddress.OrgPK = value; }
		}

		#endregion

		#region ExportATF

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "B7_ParentID,B7_ParentTableCode", DisableCopyMethodLink = true)]
		public ATF ExportATF
		{
			get
			{
				if (fExportATF == null || fExportATF.IsNull)
				{
					if (OGAIndicatorList.IsToBeDeclared(US_ATFInd))
					{
						if (HasATFDetails)
						{
							fExportATF = ATFLines.OfType<ATF>().FirstOrDefault();
						}
						else
						{
							fExportATF = ATFLines.AddNew();
							fExportATF.CopyQuantityFrom(this);
						}
					}
					else
					{
						fExportATF = Factory.GetNull<ATF>();
					}
				}

				return fExportATF;
			}
		}
		ATF fExportATF;

		#endregion

		#region ExportFWS

		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "B7_ParentID,B7_ParentTableCode", DisableCopyMethodLink = true)]
		public FWSHeader ExportFWS
		{
			get
			{
				if (fExportFWS == null || fExportFWS.IsNull || fExportFWS.IsDeleted)
				{
					if (OGAIndicatorList.IsToBeDeclared(US_FWSInd))
					{
						fExportFWS = HasFWSHeaders ? FWSHeaders.OfType<FWSHeader>().FirstOrDefault() : FWSHeaders.AddNew();
					}
					else
					{
						fExportFWS = Factory.GetNull<FWSHeader>();
					}
				}

				return fExportFWS;
			}
		}
		FWSHeader fExportFWS;

		#endregion

		#region First Permit/License

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.LicencePermitTypes))]
		[MaxLength(2)]
		public ZString US_FirstPermitLicenseType
		{
			get => GetEffectiveValueFromFirstLicencesAndPermits(LicenceAndPermit.Schema.CY_Code);
			set => SetEffectiveValueToFirstLicensesAndPermits(value, US_FirstPermitLicenseTypeInfo, LicenceAndPermit.Schema.CY_Code);
		}

		public ZPropertyInfo US_FirstPermitLicenseTypeInfo => GetZPropertyInfo(Schema.US_FirstPermitLicenseType);

		[MaxLength(10)]
		public ZString US_FirstPermitLicenseNumber
		{
			get => GetEffectiveValueFromFirstLicencesAndPermits(LicenceAndPermit.Schema.CY_Data);
			set => SetEffectiveValueToFirstLicensesAndPermits(value, US_FirstPermitLicenseNumberInfo, LicenceAndPermit.Schema.CY_Data);
		}

		public ZPropertyInfo US_FirstPermitLicenseNumberInfo => GetZPropertyInfo(Schema.US_FirstPermitLicenseNumber);

		ZString GetEffectiveValueFromFirstLicencesAndPermits(string schemaName)
		{
			var result = ZString.Empty;
			var firstLicenseAndPermit = LicenceAndPermits.FirstOrDefault();
			if (firstLicenseAndPermit != null)
			{
				result = (ZString)firstLicenseAndPermit[schemaName];
			}

			return result;
		}

		void SetEffectiveValueToFirstLicensesAndPermits(ZString valueToSet, ZPropertyInfo propertyInfo, string schemaName)
		{
			CheckMaximumLength(propertyInfo, valueToSet);

			var firstLicenseAndPermit = LicenceAndPermits.FirstOrDefault();
			if (firstLicenseAndPermit == null && !valueToSet.IsEmpty)
			{
				firstLicenseAndPermit = LicenceAndPermits.AddNew();
			}

			if (firstLicenseAndPermit != null)
			{
				firstLicenseAndPermit[schemaName] = valueToSet;
			}

			propertyInfo.RefreshBinding();
		}

		#endregion

		#endregion

		#region New Methods

		internal void MapValuesBetweenACEAndACS(bool licenseFromProduct = false)
		{
			if (IsACE)
			{
				MapValuesToACE(licenseFromProduct);
			}
			else
			{
				MapValuesToACS();
			}
		}

		void MapValuesToACE(bool licenseFromProduct)
		{
			if (!US_ADDCaseNo.IsEmpty && US_ADDDepositRateIndicator != DepositRateIndicatorList.Codes.Specific && US_ADDDepositRateIndicator != DepositRateIndicatorList.Codes.OverrideAdValorem && US_ADDDepositRateIndicator != DepositRateIndicatorList.Codes.OverrideSpecific)
			{
				US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			}

			if (!US_CVDCaseNo.IsEmpty && US_CVDDepositRateIndicator != DepositRateIndicatorList.Codes.Specific && US_CVDDepositRateIndicator != DepositRateIndicatorList.Codes.OverrideAdValorem && US_CVDDepositRateIndicator != DepositRateIndicatorList.Codes.OverrideSpecific)
			{
				US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			}

			if (US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.X || US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.V)
			{
				US_SetInd = US_SecondarySPI;
				US_SecondarySPI = "";
			}

			if (!US_AgricultureLicNo.IsEmpty)
			{
				LicenceAndPermits.SetStringValueHavingCodeOrDeleteIfValueEmpty(LicencePermitTypeList.Codes._14, US_AgricultureLicNo, true);
				US_AgricultureLicNo = "";
			}

			if (!US_WoolLicenceNo.IsEmpty)
			{
				LicenceAndPermits.SetStringValueHavingCodeOrDeleteIfValueEmpty(LicencePermitTypeList.Codes._17, US_WoolLicenceNo, true);
				US_WoolLicenceNo = "";
			}

			if (!US_CottonCertificateNo.IsEmpty)
			{
				if (licenseFromProduct)
				{
					LicenceAndPermits.SetStringValueHavingCodeOrDeleteIfValueEmpty(LicencePermitTypeList.Codes._12, US_CottonCertificateNo, true);
				}
				else
				{
					LicenceAndPermits.AddNew(UndefinedLicenceType, US_CottonCertificateNo);
				}
				US_CottonCertificateNo = "";
			}

			if (!US_CAExportCertificate.IsEmpty)
			{
				LicenceAndPermits.SetStringValueHavingCodeOrDeleteIfValueEmpty(LicencePermitTypeList.Codes._16, US_CAExportCertificate, true);
				US_CAExportCertificate = "";
			}
			if (!US_CBTPACertificateNo.IsEmpty)
			{
				LicenceAndPermits.SetStringValueHavingCodeOrDeleteIfValueEmpty(LicencePermitTypeList.Codes._18, US_CBTPACertificateNo, true);
				US_CBTPACertificateNo = "";
			}
			if (!US_MiscPermitNo.IsEmpty)
			{
				LicenceAndPermits.SetStringValueHavingCodeOrDeleteIfValueEmpty(ImportTariff != null ? ImportTariff.UE_PermitLicenseIndicator : ZString.Empty, US_MiscPermitNo, true);
				US_MiscPermitNo = "";
			}
		}

		const string UndefinedLicenceType = "??";

		void MapValuesToACS()
		{
			if (!US_SetInd.IsEmpty)
			{
				US_SecondarySPI = US_SetInd;
				US_SetInd = "";
			}

			foreach (LicenceAndPermit permit in LicenceAndPermits)
			{
				switch (permit.CY_Code)
				{
					case LicencePermitTypeList.Codes._14:
						US_AgricultureLicNo = permit.CY_Data.SubstringSafe(0, USAddInfoSchema.US_AgricultureLicNo.MaxLength);
						break;
					case LicencePermitTypeList.Codes._17:
						US_WoolLicenceNo = permit.CY_Data.SubstringSafe(0, USAddInfoSchema.US_WoolLicenceNo.MaxLength);
						break;
					case LicencePermitTypeList.Codes._12:
					case LicencePermitTypeList.Codes._22:
					case LicencePermitTypeList.Codes._23:
						if (LicenceAndPermits.Find(x => LicencePermitTypeList.DeclareInCottonOrganicExemptionFieldInACS(x.CY_Code)).Count() > 1)
						{
							US_CottonCertificateNo = ZString.Empty;
						}
						else
						{
							US_CottonCertificateNo = permit.CY_Data.SubstringSafe(0, USAddInfoSchema.US_CottonCertificateNo.MaxLength);
						}
						break;
					case LicencePermitTypeList.Codes._16:
						US_CAExportCertificate = permit.CY_Data.SubstringSafe(0, USAddInfoSchema.US_CAExportCertificate.MaxLength);
						break;
					case LicencePermitTypeList.Codes._18:
						US_CBTPACertificateNo = permit.CY_Data.SubstringSafe(0, USAddInfoSchema.US_CBTPACertificateNo.MaxLength);
						break;
				}
			}

			if (LicenceAndPermits.Count > 0)
			{
				US_MiscPermitNo = LicenceAndPermits.GetStringDataHaving(ImportTariff != null ? ImportTariff.UE_PermitLicenseIndicator : ZString.Empty).SubstringSafe(0, USAddInfoSchema.US_MiscPermitNo.MaxLength);
			}

			LicenceAndPermits.RemoveAndDeleteAll();
		}

		public CusEntryLine GetEntryLineFor(ZString messageType, bool supLine)
		{
			return GetEntryLineFor(messageType, supLine, (x) => !x.US_SupAdditionalLine && !x.US_SupAdditionalLine2 && !x.US_SupAdditionalLine3 && !x.US_SupAdditionalLine4 && !x.US_SupAdditionalLine5);
		}

		public CusEntryLine GetEntryLineFor(ZString messageType, bool supLine, Func<CusEntryLine, bool> checkSupAdditionalLine)
		{
			CusEntryLine result = null;
			var entryLine = this.CusEntryLine;
			if (entryLine != null && entryLine.Header.CH_MessageType == messageType && supLine == entryLine.US_SupLine && (checkSupAdditionalLine == null || checkSupAdditionalLine(entryLine)))
			{
				result = entryLine;
			}
			else
			{
				foreach (CusEntryLine additionalEntryLine in AdditionalEntryLineLinks.GetEntryLineFor(messageType))
				{
					if (additionalEntryLine.US_SupLine == supLine && (checkSupAdditionalLine == null || checkSupAdditionalLine(additionalEntryLine)))
					{
						result = additionalEntryLine;
						break;
					}
				}
			}

			return result;
		}

		internal JobComInvoiceLine AddSecondaryInvoiceLine()
		{
			using (EnableDebugLog())
			{
				JobComInvoiceLine result = InvoiceHeader.JobComInvoiceLines.AddNew();
				result.JI_ParentID = PK;
				return result;
			}
		}

		internal JobComInvoiceLine AddProductRelatedInvoiceLine()
		{
			JobComInvoiceLine result = InvoiceHeader.JobComInvoiceLines.AddNew();
			result.US_JI_ParentProduct = PK;
			return result;
		}

		internal void DetachPivotsForFDAOrPGA()
		{
			FDAs.OfType<FDA>().Where(x => !x.IsDeleted).ToList().ForEach(x =>
			{
				x.BillsForFDALine.RemoveAndDeleteAll();
				x.ContainersForFDALine.RemoveAndDeleteAll();
			});
			LaceyActLines.OfType<PGA>().Where(x => !x.IsDeleted).ToList().ForEach(x => x.ContainersForPGALine.RemoveAndDeleteAll());
		}

		public ZDecimal GetSupCustomsValue(bool isSupLine, bool isSupAdditionalLine1, bool isSupAdditionalLine2, bool isSupAdditionalLine3, bool isSupAdditionalLine4, bool isSupAdditionalLine5)
		{
			var result = ZDecimal.Zero;
			if (InvoiceHeader is JobComInvoiceHeader header)
			{
				var supGoodsValueFieldName = GetSupGoodsValueFieldName(isSupLine, isSupAdditionalLine1, isSupAdditionalLine2, isSupAdditionalLine3, isSupAdditionalLine4, isSupAdditionalLine5);
				if (!supGoodsValueFieldName.IsEmpty)
				{
					var supGoodsValueInMoney = new Money((ZDecimal)this[supGoodsValueFieldName], header.Invoice_Currency);
					result = ConvertToLocalAmountExact(supGoodsValueInMoney).Amount.Round(0);
				}
			}

			return result;
		}

		ZString GetSupGoodsValueFieldName(bool isSupLine, bool isSupAdditionalLine1, bool isSupAdditionalLine2, bool isSupAdditionalLine3, bool isSupAdditionalLine4, bool isSupAdditionalLine5)
		{
			if (isSupAdditionalLine5)
			{
				return JobComInvoiceLine.Schema.US_SupAdditionalTariff5GoodsValue;
			}
			else if (isSupAdditionalLine4)
			{
				return JobComInvoiceLine.Schema.US_SupAdditionalTariff4GoodsValue;
			}
			else if (isSupAdditionalLine3)
			{
				return JobComInvoiceLine.Schema.US_SupAdditionalTariff3GoodsValue;
			}
			else if (isSupAdditionalLine2)
			{
				return JobComInvoiceLine.Schema.US_SupAdditionalTariff2GoodsValue;
			}
			else if (isSupAdditionalLine1)
			{
				return JobComInvoiceLine.Schema.US_SupAdditionalTariff1GoodsValue;
			}
			else if (isSupLine)
			{
				return JobComInvoiceLine.Schema.US_SupGoodsValue;
			}

			return ZString.Empty;
		}

		#endregion

		#region PGA Data Correction

		protected override bool IsACEDDTCDataDeclarationReadonly
		{
			get { return base.IsACEDDTCDataDeclarationReadonly || DDTCDataCorrection.IsPGALineReadOnly(); }
		}

		internal DDTCDataCorrection DDTCDataCorrection
		{
			get
			{
				if (ddtcDataCorrection == null)
				{
					ddtcDataCorrection = new DDTCDataCorrection(this);
				}
				return ddtcDataCorrection;
			}
		}
		DDTCDataCorrection ddtcDataCorrection;

		protected override bool IsACETSCADataDeclarationReadonly
		{
			get { return base.IsACETSCADataDeclarationReadonly || TSCADataCorrection.IsPGALineReadOnly() || ODSDataCorrection.IsPGALineReadOnly(); }
		}

		internal TSCADataCorrection TSCADataCorrection
		{
			get
			{
				if (tscaDataCorrection == null)
				{
					tscaDataCorrection = new TSCADataCorrection(this);
				}
				return tscaDataCorrection;
			}
		}
		TSCADataCorrection tscaDataCorrection;

		public ZString TSCAStatus
		{
			get { return TSCADataCorrection.GetStatus((short)this.US_TSCALineNumber); }
		}

		public ZString TSCAStatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(TSCAStatus); }
		}

		public ZDateTime TSCAStatusDate
		{
			get { return TSCADataCorrection.GetStatusDate((short)this.US_TSCALineNumber); }
		}

		internal ODSDataCorrection ODSDataCorrection
		{
			get
			{
				if (odsDataCorrection == null)
				{
					odsDataCorrection = new ODSDataCorrection(this);
				}
				return odsDataCorrection;
			}
		}
		ODSDataCorrection odsDataCorrection;

		public ZString ODSStatus
		{
			get { return ODSDataCorrection.GetStatus((short)this.US_ODSLineNumber); }
		}

		public ZString ODSStatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(ODSStatus); }
		}

		public ZDateTime ODSStatusDate
		{
			get { return ODSDataCorrection.GetStatusDate((short)this.US_ODSLineNumber); }
		}

		internal bool HasPGAIndicators
		{
			get
			{
				return !US_DDTCInd.IsEmpty || !US_TSCAInd.IsEmpty || !US_ODSInd.IsEmpty || !US_APHISInd.IsEmpty || !US_FDAIndicator.IsEmpty || !US_ATFInd.IsEmpty || !US_AMSInd.IsEmpty || !US_NOPInd.IsEmpty ||
					!US_PSTIndicator.IsEmpty || !US_VNEInd.IsEmpty || !US_FSISInd.IsEmpty || !US_FWSInd.IsEmpty || !US_NHTSAIndicator.IsEmpty || !US_NMFS370Ind.IsEmpty || !US_NMFSAMRInd.IsEmpty || !US_NMFSCOAInd.IsEmpty ||
					!US_NMFSHMSInd.IsEmpty || !US_NMFSSIMPInd.IsEmpty || !US_LaceyIndicator.IsEmpty || !US_TTBInd.IsEmpty || !US_CPSCInd.IsEmpty || !US_DEAInd.IsEmpty || !US_OMCInd.IsEmpty || !US_HFCInd.IsEmpty;
			}
		}

		internal IReadOnlyList<IPGADataCorrection> PGADataCorrections => Factory.GetValue(ref cachedPGADataCorrections,
			() =>
			{
				var result = new List<IPGADataCorrection>(APHISHeaders.Count
					+ ACE_FDALines.Count
					+ ATFLines.Count
					+ AMSLines.Count
					+ PSTLines.Count
					+ VehicleLines.Count
					+ FSISLines.Count
					+ FWSHeaders.Count
					+ NHTSALines.Count
					+ NMFSLines.Count
					+ OMCHeaders.Count
					+ LaceyActLines.Count
					+ TTBLines.Count
					+ DEAHeaders.Count
					+ USHFCHeaders.Count
					+ 3
					+ CPSCHeaders.Count);
				result.AddRange(APHISHeaders.Cast<IPGADataCorrection>());
				result.AddRange(ACE_FDALines.Cast<IPGADataCorrection>());
				result.AddRange(ATFLines.Cast<IPGADataCorrection>());
				result.AddRange(AMSLines.Cast<IPGADataCorrection>());
				result.AddRange(PSTLines.Cast<IPGADataCorrection>());
				result.AddRange(VehicleLines.Cast<IPGADataCorrection>());
				result.AddRange(FSISLines.Cast<IPGADataCorrection>());
				result.AddRange(FWSHeaders.Cast<IPGADataCorrection>());
				result.AddRange(NHTSALines.Cast<IPGADataCorrection>());
				result.AddRange(NMFSLines.Where(x => SupportNMFSPGADataCorrection(x.US_ProgramType)).Cast<IPGADataCorrection>());
				result.AddRange(OMCHeaders.Cast<IPGADataCorrection>());
				result.AddRange(LaceyActLines.Cast<IPGADataCorrection>());
				result.AddRange(TTBLines.Cast<IPGADataCorrection>());
				result.AddRange(DEAHeaders.Cast<IPGADataCorrection>());
				result.AddRange(USHFCHeaders.Cast<IPGADataCorrection>());
				result.Add(DDTCDataCorrection);
				result.Add(TSCADataCorrection);
				result.Add(ODSDataCorrection);
				result.AddRange(CPSCHeaders.Cast<IPGADataCorrection>());
				return result;
			});
		CachedProperty<IReadOnlyList<IPGADataCorrection>> cachedPGADataCorrections;

		bool SupportNMFSPGADataCorrection(ZString programType)
		{
			switch (programType.ToUpperInvariant())
			{
				case NMFSProgramCodeList.Codes.AMR:
				case NMFSProgramCodeList.Codes.COA:
				case NMFSProgramCodeList.Codes.HMS:
				case NMFSProgramCodeList.Codes.SIM:
				case NMFSProgramCodeList.Codes._370:
					return true;
			}
			return false;
		}

		#endregion

		#region Collections

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public AIILineCollection AIILines
		{
			get
			{
				if (aiiLines == null)
				{
					aiiLines = new AIILineCollection(this);
					aiiLines.Load();
					RegisterEditableChildObject(aiiLines);
				}
				return aiiLines;
			}
		}
		AIILineCollection aiiLines;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public InvoiceLineGroupingRangeCollection LineGroupingRanges
		{
			get
			{
				if (lineGroupingRanges == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						lineGroupingRanges = Factory.GetCachedValue("FakeyLineGroupingRanges", delegate
						{ return new InvoiceLineGroupingRangeCollection(nullInvoiceLine); });
					}
					else
					{
						lineGroupingRanges = new InvoiceLineGroupingRangeCollection(this);
						lineGroupingRanges.Load();
						RegisterEditableChildObject(lineGroupingRanges);
					}
				}
				return lineGroupingRanges;
			}
		}
		InvoiceLineGroupingRangeCollection lineGroupingRanges;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public FeeCusCodeDataCollection FeeCusCodes
		{
			get
			{
				if (feeCusCodes == null)
				{
					feeCusCodes = new FeeCusCodeDataCollection(this);
					feeCusCodes.Load();
					RegisterEditableChildObject(feeCusCodes);
					feeCusCodes.Sort(CusCodeDataSchema.CY_Code.Name, System.ComponentModel.ListSortDirection.Ascending);
				}

				return feeCusCodes;
			}
		}
		FeeCusCodeDataCollection feeCusCodes;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public ReconEntryOriginalChargeCollection ReconOriginalCharges
		{
			get
			{
				if (fReconOriginalCharges == null)
				{
					fReconOriginalCharges = new ReconEntryOriginalChargeCollection(this);
					fReconOriginalCharges.Load();
					RegisterEditableChildObject(fReconOriginalCharges);
					fReconOriginalCharges.Sort(CusCodeDataSchema.CY_Code.Name, System.ComponentModel.ListSortDirection.Ascending);
				}

				return fReconOriginalCharges;
			}
		}
		ReconEntryOriginalChargeCollection fReconOriginalCharges;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public ReconRefundedChargeCollection ReconRefundedFees
		{
			get
			{
				if (reconRefundedCharges == null)
				{
					reconRefundedCharges = new ReconRefundedChargeCollection(this);
					reconRefundedCharges.Load();
					RegisterEditableChildObject(reconRefundedCharges);
					reconRefundedCharges.Sort(CusCodeDataSchema.CY_Code.Name, System.ComponentModel.ListSortDirection.Ascending);
				}

				return reconRefundedCharges;
			}
		}
		ReconRefundedChargeCollection reconRefundedCharges;

		public InvoiceHeaderFilteredActiveCollection FilteredInvoice_List
		{
			get
			{
				if (Declaration != null)
				{
					return Declaration.FilteredInvoices;
				}
				return null;
			}
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public LicenceAndPermitCollection LicenceAndPermits
		{
			get
			{
				if (licenceAndPermits == null)
				{
					licenceAndPermits = new LicenceAndPermitCollection(this);
					licenceAndPermits.Load();
					RegisterEditableChildObject(licenceAndPermits);
				}
				return licenceAndPermits;
			}
		}
		LicenceAndPermitCollection licenceAndPermits;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public CensusWarningOverrideCollection CensusWarningOverrides
		{
			get
			{
				if (censusWarningOverrides == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						censusWarningOverrides = Factory.GetCachedValue("FakeyCensusWarningOverrides", delegate
						{ return new CensusWarningOverrideCollection(nullInvoiceLine); });
					}
					else
					{
						censusWarningOverrides = new CensusWarningOverrideCollection(this);
						censusWarningOverrides.Load();
						RegisterEditableChildObject(censusWarningOverrides);
					}
				}
				return censusWarningOverrides;
			}
		}
		CensusWarningOverrideCollection censusWarningOverrides;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusAddInfoSchema.Constants.TableName, CusAddInfoSchema.Constants.B7_ParentID, CusAddInfoSchema.Constants.B7_ParentTableCode)]
		public FishingInformationCollection FishingInformations
		{
			get
			{
				if (fFishingInformations == null)
				{
					fFishingInformations = new FishingInformationCollection(this);
					fFishingInformations.Load();
					RegisterEditableChildObject(fFishingInformations);
				}
				return fFishingInformations;
			}
		}
		FishingInformationCollection fFishingInformations;

		public ZBool HasFishingInformations
		{
			get { return FishingInformations.Count > 0; }
		}

		[ChildEditable(true)]
		public MiningInformationCollection MiningInformations
		{
			get
			{
				if (fMiningInformations == null)
				{
					fMiningInformations = new MiningInformationCollection(this);
					fMiningInformations.Load();
					RegisterEditableChildObject(fMiningInformations);
				}
				return fMiningInformations;
			}
		}
		MiningInformationCollection fMiningInformations;

		public ZBool HasMiningInformations
		{
			get { return MiningInformations.Count > 0; }
		}

		#endregion

		#region Automated warehousing integration - TODO

		protected override bool IsBondedWarehousingDisabledCore
		{
			get { return Declaration?.IsBondedWarehousingDisabled ?? false; }
		}

		protected override bool IsGoingIntoBondedWarehouseCore
		{
			get
			{
				var result = JI_PartNo_CanBeSetByCustomer;
				if (result)
				{
					var declaration = Declaration;
					result = declaration != null && declaration.IsInwardBondedWarehousingEnabled;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected override IEnumerable<string> GetSupportedFields() => base.GetSupportedFields().Union(new[]
		{
			AutoUSAddInfo.Schema.US_SupTariff,
			AutoUSAddInfo.Schema.US_DRWAdValoremRate,
			AutoUSAddInfo.Schema.US_DRWWeightedRatio,
			AutoUSAddInfo.Schema.US_DRWMPFWeightedRatio,
			AutoUSAddInfo.Schema.US_DRWLineDuty,
			AutoUSAddInfo.Schema.US_DRWDeclaredTax,
			AutoUSAddInfo.Schema.US_DRWDeclaredVFD,
			AutoUSAddInfo.Schema.US_DRWDeclaredHMF,
			AutoUSAddInfo.Schema.US_DRWDeclaredMPF,
			AutoUSAddInfo.Schema.US_DRWImportQuantity,
			AutoUSAddInfo.Schema.US_DRWImportUQ,
			AutoUSAddInfo.Schema.US_DRWExportQuantity,
			AutoUSAddInfo.Schema.US_DRWExportUQ,
			AutoUSAddInfo.Schema.US_DRWImportQuantity2,
			AutoUSAddInfo.Schema.US_DRWImportUQ2,
			AutoUSAddInfo.Schema.US_DRWImportQuantity3,
			AutoUSAddInfo.Schema.US_DRWImportUQ3,
			AutoUSAddInfo.Schema.US_DRWValuePerUQ,
			AutoUSAddInfo.Schema.US_DRWValuePerUQ2,
			AutoUSAddInfo.Schema.US_DRWValuePerUQ3,
			AutoUSAddInfo.Schema.US_DRWLineDutyRateDesc,
			AutoUSAddInfo.Schema.US_DRWCalcDutyWithAdValoremRate
		});

		protected override bool SupportsBondedWarehousingCore
		{
			get
			{
				return Declaration?.SupportsBondedWarehousing ?? false;
			}
		}

		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);
		protected override ZString CustomsCountryCodeCore
		{
			get { return Core.Constants.CountryCodes.UnitedStates; }
		}

		protected override ZString GetPartPivotTypeCore()
		{
			var isExport = IsExport;
			var pivotType = ClassificationTypeList.Codes.HTI;
			var cusClass = Classification;
			if (cusClass != null)
			{
				if (cusClass.CC_ClassificationType == CusClassification.ClassificationType.IMP)
				{
					if (isExport)
					{
						pivotType = ClassificationTypeList.Codes.HTE;
					}
				}
				else if (cusClass.CC_ClassificationType == CusClassification.ClassificationType.EXP)
				{
					pivotType = ClassificationTypeList.Codes.SHB;
				}
			}
			else if (isExport)
			{
				pivotType = UseScheduleB ? ClassificationTypeList.Codes.SHB : ClassificationTypeList.Codes.HTE;
			}
			return pivotType;
		}

		void ContainersPivot_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!ContainersPivot.IsLoading)
			{
				if (!havedAddedFetchHint)
				{
					havedAddedFetchHint = true;
					foreach (var pga in LaceyActLines.OfType<PGA>())
					{
						var query = new ZQuery(GenPivotSchema.XX_Relation1ID, pga.PK);
						query.AddToFilter(GenPivotSchema.XX_RelationType, PGARelatedContainersGenPivot.RelationType);
						Factory.AddFetchHint(GenPivotSchema.Instance, query);
					}

					foreach (var fda in FDAs.OfType<FDA>())
					{
						var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
						query.AddToFilter(GenPivotSchema.XX_RelationType, FDARelatedContainersGenPivot.RelationType);
						Factory.AddFetchHint(GenPivotSchema.Instance, query);
					}

					foreach (var fda in ACE_FDALines.OfType<ACEFDA>())
					{
						var query = new ZQuery(GenPivotSchema.XX_Relation1ID, fda.PK);
						query.AddToFilter(GenPivotSchema.XX_RelationType, FDARelatedContainersGenPivot.RelationType);
						Factory.AddFetchHint(GenPivotSchema.Instance, query);
					}
				}
				foreach (var pga in LaceyActLines.OfType<PGA>())
				{
					pga.SetPGARelatedContainers();
				}

				foreach (var fda in FDAs.OfType<FDA>())
				{
					fda.SetFDARelatedContainers();
				}
			}
		}
		bool havedAddedFetchHint;

		void ClearACEDDTCDataIfNeeded()
		{
			if (IsACE && US_DDTCInd != OGAIndicatorList.Codes.Declared && US_DDTCTrackingStatus.IsEmpty)
			{
				US_DDTCLicenseType = ZString.Empty;
				US_DDTCLicenseNo = ZString.Empty;
				US_DDTCExemptionCode = ZString.Empty;
				US_DDTCArrivalDate = ZDateTime.Empty;
				US_DDTCRegistrationNo = ZString.Empty;
			}
		}

		AIILine GetInvoiceLineLowestAIILineNo()
		{
			AIILine result = null;

			ZQuery query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USAIILine);
			query.AddToFilter(CusAddInfoSchema.B7_ParentID, PK);

			query.FetchOnlyFromLocalCache = !IsInDatabase;
			var lines = Factory.Load<AIILine>(query);
			foreach (AIILine aiiLine in lines)
			{
				if (result == null || result.US_LineNo > aiiLine.US_LineNo)
				{
					result = aiiLine;
				}
			}
			return result;
		}

		ILinePriceCalculationFieldSettingSupporter LinePriceCalculationFieldSettingSupporter
		{
			get { return this; }
		}

		protected override void UpdateUnitPriceIfChangedFromLinePriceChanges()
		{
			if (!IsSettingUnitPrice)
			{
				ZDecimal newUnitPrice = CalculateUnitPrice();
				if (newUnitPrice > ZDecimal.Zero && UnitPrice != newUnitPrice)
				{
					UnitPrice = newUnitPrice;
				}
			}
		}

		void UpdateUS_98ValueInvCurrIfChanged()
		{
			if (!IsSetting98ValueInvCurr)
			{
				ZDecimal new98ValueInvCurr = Calculate98ValueInvCurrFromUnitPrice();
				if (US_98ValueInvCurr != new98ValueInvCurr)
				{
					US_98ValueInvCurr = new98ValueInvCurr;
				}
			}
		}

		ZDecimal Calculate98ValueInvCurrFromUnitPrice()
		{
			return decimal.Round(JI_InvoiceQuantity * US_98InvCurrPerUnit, 2);
		}

		void UpdateUS_98InvCurrPerUnitIfChanged()
		{
			if (!IsSetting98InvCurrPerUnit)
			{
				ZDecimal invCurrPerUnit = CalculateUS_98InvCurrPerUnit();
				if (US_98InvCurrPerUnit != invCurrPerUnit)
				{
					US_98InvCurrPerUnit = invCurrPerUnit;
				}
			}
		}

		ZDecimal CalculateUS_98InvCurrPerUnit()
		{
			return US_98ValueInvCurr == ZDecimal.Zero || JI_InvoiceQuantity == ZDecimal.Zero ? ZDecimal.Zero : new ZDecimal(US_98ValueInvCurr / JI_InvoiceQuantity).Round(6);
		}

		void CalculateAMMVCharge()
		{
			ApportionedCharges.OfType<InvoiceLineApportionCharge>().Where(ch => ch.IsAMMV()).DeleteAll();
			if ((!US_AMMVPerUnit.IsEmpty && !JI_InvoiceQuantity.IsEmpty) || (!US_AMMVPercentage.IsEmpty && !JI_LinePrice.IsEmpty))
			{
				var ammvCharge = ApportionedCharges.AddNewAMMVCharge();
				if (!US_AMMVPerUnit.IsEmpty && !JI_InvoiceQuantity.IsEmpty)
				{
					ammvCharge.J7_Amount = US_AMMVPerUnit * JI_InvoiceQuantity;
					var ammvPerUnitCurrency = Pivot?.CD_AMMVPerUnitCurrency;
					ammvCharge.J7_RX_NKCurrency = string.IsNullOrEmpty(ammvPerUnitCurrency) ? (ZString)Core.Constants.CurrencyCodes.UnitedStates : (ZString)ammvPerUnitCurrency;
				}
				else
				{
					ammvCharge.J7_Amount = US_AMMVPercentage * JI_LinePrice / 100;
					ammvCharge.J7_RX_NKCurrency = JI_RX_NKLinePriceCurr;
					ammvCharge.J7_Percentage = US_AMMVPercentage;
				}
			}
		}

		bool IsImporterSetupToNotConvertSKU()
		{
			var importer = OrgHeaderWrapper.New(Importer_Effective);
			return importer != null && importer.ZO_DoNotConvertSKU;
		}

		ZString GetConvertedUnit(ZString unit)
		{
			if (IsImport && IsImporterSetupToNotConvertSKU())
			{
				return unit;
			}
			var invoiceUQList = Lookups.InvoiceUQList;
			CusRefPacks refPack = null;
			CusRefPacksCollection oneToOneConversionList = GetOneToOneConversionList();
			if (oneToOneConversionList.Count > 0)
			{
				OrgHeader supplier = Supplier_Effective;
				ZQuery packsQuery = new ZQuery(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.UnitedStates);
				packsQuery.AddToFilter(RefPacksSchema.RP_CommercialPack, unit);
				packsQuery.AddToFilter(RefPacksSchema.RP_ConversionFactor, 1);
				packsQuery.OrderBy = RefPacksSchema.RP_CustomsPack.Name;
				CusRefPacks firstPossibleRefPackMatch = null;
				if (supplier != null)
				{
					ZQuery supplierPacksQuery = packsQuery.DeepClone();
					supplierPacksQuery.AddToFilter(RefPacksSchema.RP_OH_Supplier, supplier.PK);
					foreach (CusRefPacks pack in oneToOneConversionList.Find(supplierPacksQuery))
					{
						if (firstPossibleRefPackMatch == null)
						{
							firstPossibleRefPackMatch = pack;
						}
						if (invoiceUQList.ContainsCode(pack.RP_CustomsPack))
						{
							refPack = pack;
							break;
						}
					}
				}

				if (refPack == null)
				{
					packsQuery.AddToFilter(RefPacksSchema.RP_OH_Supplier, null);
					foreach (CusRefPacks pack in oneToOneConversionList.Find(packsQuery))
					{
						if (firstPossibleRefPackMatch == null)
						{
							firstPossibleRefPackMatch = pack;
						}
						if (invoiceUQList.ContainsCode(pack.RP_CustomsPack))
						{
							refPack = pack;
							break;
						}
					}
				}

				if (refPack == null && firstPossibleRefPackMatch != null && !invoiceUQList.ContainsCode(unit))
				{
					refPack = firstPossibleRefPackMatch;
				}
			}

			return (refPack != null) ? refPack.RP_CustomsPack : unit;
		}

		CusRefPacksCollection GetOneToOneConversionList()
		{
			return Factory.GetCachedValue("USOneToOneCusRefPacksConversionList", delegate
			{
				var oneToOnefilteredPacks = CusRefPacksHelper.LoadFilteredRefPacks(Factory, Core.Constants.CountryCodes.UnitedStates, RPTypeList.Codes.CommercialInvoice, ZString.Empty, true, 1);

				var oneToOneOrderdfilteredPacks = oneToOnefilteredPacks.OrderBy(pack => pack.RP_CustomsPack);

				var collection = new CusRefPacksCollection(Factory);
				collection.AddRange(oneToOneOrderdfilteredPacks);

				return collection;
			});
		}

		void UpdateAESDetailsOnPartChange()
		{
			CusClassPartPivot bestMatch = Pivot ?? Factory.GetNull<CusClassPartPivot>();
			US_AESOriginIndicator = bestMatch.CD_OriginIndicator;
			US_ECCN = bestMatch.CD_ECCN;
			US_LicenseType = bestMatch.CD_LicenceType;
			US_LicenseNo = bestMatch.CD_LicenceNo;
			US_DDTCITARExemptionNo = bestMatch.CD_ITARExemptionNo;
			US_DDTCMilitaryEquipmentIndicator = bestMatch.CD_MilitaryEquipInd;
			US_DDTCPartyCertificationIndicator = bestMatch.CD_PartyCertInd;
			US_DDTCRegistrationNo = bestMatch.CD_DDTCRegoNo;
			US_JurisdictionNumber = bestMatch.CD_DDTCJurisdictionNumber;
			US_DDTCUSMLCategoryCode = bestMatch.CD_DDTCUSMLCategoryCode;
			US_DDTCUnit = bestMatch.CD_DDTCUnit;
			US_ExportCode = bestMatch.CD_ExportCode;
			if (JI_RX_NKLinePriceCurr == bestMatch.CD_RX_NKPerUnitCostCurr || bestMatch.IsNull)
			{
				if (bestMatch.CD_PerUnitCost > 0m)
				{
					UnitPrice = bestMatch.CD_PerUnitCost;
				}
			}

			CleanUpExportPGADetails(bestMatch);
			UpdateExportPGADetailsFromProduct(bestMatch, Part);
		}

		void UpdateExportPGADetailsFromProduct(CusClassPartPivot pivot, OrgSupplierPart part)
		{
			if (pivot != null && !pivot.IsNull && part != null && !part.JustUpdatedByDataRefresh)
			{
				CopyExportAMSDetailsFromProduct(pivot);
				CopyExportEPADetailsFromProduct(pivot);
				CopyExportATFdetailsFromProduct(pivot);
				CopyExportNMFSDetailsFromProduct(pivot);
				CopyExportFWSDetailsFromProduct(pivot);
				CopyExportDEADetailsFromProduct(pivot);
				CopyExportTTBDetailsFromProduct(pivot);
			}
		}

		void CleanUpExportPGADetails(CusClassPartPivot pivot)
		{
			if (pivot == null || pivot.IsNull || !PartWasJustUpdatedByDataRefresh(pivot))
			{
				if (!ShouldNotDefaultExportAMSForProductXMLImport)
				{
					US_ExportCertificateNo = ZString.Empty;
				}

				if (!ShouldNotDefaultExportEPAForProductXMLImport)
				{
					US_EPAConsentNumber = ZString.Empty;
					US_HazWasteTrackingNo = ZString.Empty;
					US_EPANetQty = ZDecimal.Zero;
					US_EPANetQtyUQ = ZString.Empty;
				}

				if (!ShouldNotDefaultNMFSHMSForProductXMLImport)
				{
					NMFSLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultTTBForProductXMLImport)
				{
					TTBLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultDEAForProductXMLImport)
				{
					DEAHeaders.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultATFForProductXMLImport)
				{
					ATFLines.RemoveAndDeleteAll();
				}

				if (!ShouldNotDefaultFWSForProductXMLImport)
				{
					FWSHeaders.RemoveAndDeleteAll();
				}

				CleanUpExportPGAIndicators();
			}

			ExportPGAAgencyRequirements.RefreshBindingIncludingChildren();
		}

		void CleanUpExportPGAIndicators()
		{
			US_ATFInd = ZString.Empty;
			US_FWSInd = ZString.Empty;
			US_AMSInd = ZString.Empty;
			US_PSTIndicator = ZString.Empty;
			US_NMFSHMSInd = ZString.Empty;
			US_DEAInd = ZString.Empty;
			US_TTBInd = ZString.Empty;
		}

		void CopyExportAMSDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultExportAMSForProductXMLImport)
			{
				US_AMSInd = pivot.CD_AMSIndicator;
				US_ExportCertificateNo = pivot.CD_ExportCertificateNo;
			}
		}

		void CopyExportEPADetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultExportEPAForProductXMLImport)
			{
				US_PSTIndicator = pivot.CD_PSTIndicator;
				US_EPAConsentNumber = pivot.CD_EPAConsentNumber;
				US_HazWasteTrackingNo = pivot.CD_HazWasteTrackingNo;
			}
		}

		void CopyExportATFdetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultATFForProductXMLImport)
			{
				US_ATFInd = pivot.CD_ATFIndicator;

				if (pivot.ExportATF != null && ExportATF != null)
				{
					pivot.ExportATF.UpdateAddInfoProperties();
					ExportATF.B7_AddInfoData = pivot.ExportATF.B7_AddInfoData;
					ExportATF.US_Quantity = ZDecimal.Zero;
					ExportATF.UpdateAddInfoProperties();
					ExportATF.CopyQuantityFrom(this);
				}
			}
		}

		void CopyExportNMFSDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultNMFSHMSForProductXMLImport)
			{
				US_NMFSHMSInd = pivot.CD_NMFSHMSIndicator;

				foreach (var nmfs in pivot.NMFSLines)
				{
					var clonedNMFS = (NMFSLine)nmfs.Clone();
					clonedNMFS.US_Quantity = ZDecimal.Zero;
					clonedNMFS.US_UnitOfMeasure = ZString.Empty;
					clonedNMFS.US_DISDocumentID = ZString.Empty;
					NMFSLines.Add(clonedNMFS);
				}
			}
		}

		void CopyExportFWSDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultFWSForProductXMLImport)
			{
				US_FWSInd = pivot.CD_FWSIndicator;

				if (pivot.ExportFWS != null && ExportFWS != null)
				{
					pivot.ExportFWS.UpdateAddInfoProperties();
					ExportFWS.B7_AddInfoData = pivot.ExportFWS.B7_AddInfoData;
					ExportFWS.UpdateAddInfoProperties();
				}
			}
		}

		void CopyExportDEADetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultDEAForProductXMLImport)
			{
				US_DEAInd = pivot.CD_DEAIndicator;

				foreach (var dea in pivot.DEAHeaders)
				{
					var clonedDEAHeader = (DEAHeader)dea.Clone();
					if (clonedDEAHeader.Constituents.Count > 0)
					{
						clonedDEAHeader.US_Weight = ZDecimal.Zero;
						clonedDEAHeader.US_UnitOfMeasure = ZString.Empty;
					}
					DEAHeaders.Add(clonedDEAHeader);
				}
			}
		}

		void CopyExportTTBDetailsFromProduct(CusClassPartPivot pivot)
		{
			if (!ShouldNotDefaultTTBForProductXMLImport)
			{
				US_TTBInd = pivot.CD_TTBIndicator;

				foreach (TTBLine ttb in pivot.TTBLines)
				{
					TTBLines.Add((TTBLine)ttb.Clone());
				}
			}
		}

		protected override BaseCustomsQuantityConverter GetCustomsQuantityConverter()
		{
			return new USCustomsQuantityConverter(this, JI_CustomsQuantityInfo, JI_CustomsUnitQtyInfo);
		}

		internal ZDecimal CustomsConversionFactor
		{
			get { return CustomsQuantityConverter.CustomsConversionFactor; }
		}

		void ClearFirstAIILineReference()
		{
			if (firstAIILine != null)
			{
				UnRegisterEditableChildObject(firstAIILine);
				firstAIILine = null;
			}
		}

		void ClearAIILineDetail(string fieldName)
		{
			ClearAIILineDetail(fieldName, false);
		}

		void ClearAIILineDetail(string fieldName, bool force)
		{
			if (force || IsLineGroupingEnabled)
			{
				foreach (AIILine aiiLine in AIILines)
				{
					ZPropertyInfo info = aiiLine.ZPropertyInfoHash[fieldName];
					if (info != null)
					{
						info.Value = info.DefaultValue;
					}
				}
			}
		}

		protected override bool ShouldReApportionWeightOnLinePriceChange
		{
			get { return InvoiceHeader != null && InvoiceHeader.IsExport; }
		}

		protected override ZDecimal LinePriceForWeightApportionCalculationCore
		{
			get
			{
				CurrencyConverter converter = CurrencyConverter;
				ZDecimal result = ZDecimal.Zero;

				if (Declaration != null)
				{
					Money linePriceMoney = converter == null ? Money.Empty : converter.ConvertExact(JI_LinePriceMoney, Declaration.Country.LocalCurrency, false);
					result = linePriceMoney.Amount;

					if (!Declaration.IsExport)
					{
						result += TotalOriginalGoodsValueInUSD;
					}
				}

				return result;
			}
		}

		internal USCACCaseRate GetUSCACCaseRate(USCACCase uscCase)
		{
			return uscCase?.GetDepositRate(DateForADD_CVD);
		}

		AdditionalDataForBorderWise IHaveAdditionalDataForBorderWise.GetAdditionalDataForBorderWise(string bindingProperty)
		{
			JobDeclaration declaration = this.Declaration;
			if (declaration != null && declaration.IsDrawback)
			{
				return new AdditionalDataForBorderWise(bindingProperty.Contains(JobComInvoiceLine.Schema.US_FormattedExportTariff) ? "E" : "I", US_DRWEntryDate.IsValid ? US_DRWEntryDate.Date : ZDate.Today);
			}
			else
			{
				return new AdditionalDataForBorderWise(!UseScheduleB ? "I" : "E", EffectiveDateForDutyRate);
			}
		}

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			// This is used for *both* import and export tariff find boxes, which have
			// different tariff business object types. There is no common base, so
			// BusinessObject will have to surfice.
			get { return typeof(BusinessObject); }
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			var invoiceHeader = InvoiceHeader;
			if (invoiceHeader == null || invoiceHeader.IsExport || !(invoiceHeader.JobDeclaration is JobDeclaration declaration))
			{
				return new ExportJobComInvoiceLineValidation(this);
			}
			else if (declaration.IsDrawback)
			{
				if (declaration.IsACEDrawback)
				{
					return new ACEDrawbackJobComInvoiceLineValidation(this);
				}
				else
				{
					return new ACSDrawbackJobComInvoiceLineValidation(this);
				}
			}
			else if (declaration.IsRecon)
			{
				return new ReconJobComInvoiceLineValidation(this);
			}
			else if (declaration.IsFTZAdmission)
			{
				return new FTZJobComInvoiceLineValidation(this);
			}
			else if (declaration.IsACE)
			{
				return new ACEImportJobComInvoiceLineValidation(this);
			}
			else
			{
				return new FormalImportJobComInvoiceLineValidation(this);
			}
		}

		public new CusEntryLine.Loader CusEntryLineLoader
		{
			get
			{
				if (cusEntryLineLoader == null)
				{
					cusEntryLineLoader = new CusEntryLine.Loader(Factory);
				}

				return cusEntryLineLoader;
			}
		}
		CusEntryLine.Loader cusEntryLineLoader;

		#endregion

		#region Data Import

		public override void PrepareDataImport()
		{
			base.PrepareDataImport();
			JI_ParentID = ZGuid.Empty;
			US_SecondarySPI = ZString.Empty;
		}

		#endregion

		#region IHazardousMaterial Members

		ZBool IHazardousMaterial.IsHazRelevant
		{
			get { return UNDGs.Count > 0 && UNDGs[0].UNDGSubstance != null; }
		}

		ZBool IHazardousMaterial.IsFlashPointTempRelevant
		{
			get { return UNDGs.Count > 0 && UNDGs[0].UNDGSubstance != null && !UNDGs[0].UNDGSubstance.DG_FlashPoint.IsEmpty; }
		}

		ZString IHazardousMaterial.HazMatCode
		{
			get { return JI_HazMatCode; }
		}

		ZString IHazardousMaterial.HazMatClass
		{
			get { return UNDGs.Count > 0 && UNDGs[0].UNDGSubstance != null ? UNDGs[0].UNDGSubstance.DG_UNNO : ZString.Empty; }
		}

		ZString IHazardousMaterial.HazMatQualifier
		{
			get { return JI_HazMatCodeQualifier; }
		}

		ZString IHazardousMaterial.HazMatDesc
		{
			get { return US_HazMatDesc; }
		}

		ZString IHazardousMaterial.ContactName
		{
			get { return UNDGs.Count > 0 && UNDGs[0].DGContact != null ? UNDGs[0].DGContact.OC_ContactName.Left(24) : ZString.Empty; }
		}

		ZDecimal IHazardousMaterial.FlashPointTemp
		{
			get { return UNDGs.Count > 0 ? UNDGs[0].DI_DGFlashPoint : ZDecimal.Zero; }
		}

		ZString IHazardousMaterial.HazMatClassificationDesc
		{
			get { return US_HazMatClassDesc; }
		}

		#endregion

		#region FDA

		public bool CopyLastFDADetailsToNewLine
		{
			get { return (Declaration != null && Declaration.CopyLastFDADetailsToNewLine) && !IsDataImportInProgress; }
		}

		public bool CopyLastPGADetailsToNewLine
		{
			get { return (Declaration != null && Declaration.CopyLastPGADetailsToNewLine) && !IsDataImportInProgress; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public FDACollection FDAs
		{
			get
			{
				if (fdas == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						fdas = Factory.GetCachedValue("FakeyFDAs", delegate
						{ return new FDACollection(nullInvoiceLine); });
					}
					else
					{
						fdas = new FDACollection(this);
						fdas.Load();
						RegisterEditableChildObject(fdas);
					}
				}

				return fdas;
			}
		}
		FDACollection fdas;

		#endregion

		#region DOTS

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DOTCollection DOTs
		{
			get
			{
				if (dots == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						dots = Factory.GetCachedValue("FakeyDOTs", delegate
						{ return new DOTCollection(nullInvoiceLine); });
					}
					else
					{
						dots = new DOTCollection(this);
						dots.Load();
						RegisterEditableChildObject(dots);
					}
				}
				return dots;
			}
		}
		DOTCollection dots;

		#endregion

		#region IBOMExpander Members

		public override ZBool IsBOMLineExpanded
		{
			get { return US_BOMLineExpanded; }
			set
			{
				US_BOMLineExpanded = value;
			}
		}

		public override ZGuid BOMParentLinePK
		{
			get { return US_BOMParentLine; }
			set { US_BOMParentLine = value; }
		}

		#endregion

		#region FCC

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public FCCCollection FCCs
		{
			get
			{
				if (fccs == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						fccs = Factory.GetCachedValue("FakeyFCCs", delegate
						{ return new FCCCollection(nullInvoiceLine); });
					}
					else
					{
						fccs = new FCCCollection(this);
						fccs.Load();
						RegisterEditableChildObject(fccs);
					}
				}
				return fccs;
			}
		}
		FCCCollection fccs;

		#endregion

		#region IUltimateDistributee Members

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee IUltimateDistributee.LineDutyTaxEntryFeeItems
		{
			get
			{
				DutyTaxEntryFee result = new DutyTaxEntryFee();

				CusEntryLine entryLine = CusEntryLine;

				if (entryLine != null)
				{
					MessageBuilders.ICusEntryLine line = entryLine;
					result[CustomsDisbursementChargeCode.TotalDuty] = JI_Calc_DutyAmount;
					result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] = GetAmountApportionedFromCusEntryLine(line.AntidumpingDuty + line.CountervailingDuty).Amount;
					result[CustomsDisbursementChargeCode.Excise] = GetAmountApportionedFromCusEntryLine(line.ExciseTax).Amount;

					List<ICustomsFee> customsFees = new List<ICustomsFee>(((IUltimateDistributee)this).Fees);
					foreach (ICustomsFee fee in customsFees)
					{
						if (fee.FeeCode == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
						{
							result[CustomsDisbursementChargeCode.SpecialTax1] = fee.AmountInLocalCurrency;
						}
						else if (fee.FeeCode == Core.Constants.USCustoms.FeeCodes.HMF)
						{
							result[CustomsDisbursementChargeCode.SpecialTax2] = fee.AmountInLocalCurrency;
						}
						else
						{
							result[CustomsDisbursementChargeCode.EntryFees] += fee.AmountInLocalCurrency;
						}
					}
				}

				return result;
			}
		}

		public ZDecimal HMFAmount
		{
			get { return FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.HMF); }
		}

		public ZBool IsMPFOverridden
		{
			get
			{
				var mpfCharge = FeeCusCodes.GetCharge(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
				return mpfCharge != null && mpfCharge.CY_IsOverridden;
			}
		}

		IEnumerable<ICustomsFee> IUltimateDistributee.Fees
		{
			get
			{
				foreach (FeeCusCodeData fee in FeeCusCodes)
				{
					if (!CusFeeCodeConstants.IsExciseTax(fee.CY_Code))
					{
						ZDecimal amount = 0m;

						if (fee.CY_Code == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing)
						{
							amount = US_PayableMPF.Round(2);
						}
						else
						{
							if (IsFeePayableForLC(fee.CY_Code))
							{
								amount = fee.CY_FeeAmount.Round(2);
							}
						}

						if (amount > 0)
						{
							yield return new CustomsFee(fee.CY_Code, amount);
						}
					}
				}

				foreach (var feeCode in CusFeeCodeConstants.HeaderLevelFeeCodes)
				{
					ZDecimal amount = 0m;

					CusEntryLine entryLine = CusEntryLine;
					if (entryLine != null && entryLine.Header != null)
					{
						amount = entryLine.GetAmountApportionedFromCusEntryHeader(entryLine.Header.Charges.GetAmount(feeCode));
					}

					if (amount > 0)
					{
						yield return new CustomsFee(feeCode, amount.Round(2));
					}
				}
			}
		}

		bool IsFeePayableForLC(string feeType)
		{
			bool result = false;

			CusEntryLine entryLine = CusEntryLine;

			if (entryLine != null)
			{
				switch (feeType)
				{
					case Core.Constants.USCustoms.FeeCodes.HMF:
						result = entryLine.Header != null && entryLine.Header.HMFAmountForEntry > 0;
						break;

					case Core.Constants.USCustoms.FeeCodes.Cotton:
						result = entryLine.CottonAmount > 0;
						break;

					default:
						result = FeeCusCodes.GetFeeOrChargeAmount(feeType) > 0;
						break;
				}
			}

			return result;
		}

		class CustomsFee : ICustomsFee
		{
			public CustomsFee(ZString code, ZDecimal amount)
			{
				this.code = code;
				this.amount = amount;
			}
			readonly ZString code;
			readonly ZDecimal amount;

			#region ICustomsFee Members

			public ZDecimal AmountInLocalCurrency
			{
				get { return amount; }
			}

			public ZString FeeCode
			{
				get { return code; }
			}

			#endregion
		}

		ZString IUltimateDistributee.CountryOfOriginCode
		{
			get { return US_UC_NKCountryOfOrigin; }
		}

		#endregion

		#region IDutyData Members

		ZString IDutyData.Tariff
		{
			get { return JI_Tariff; }
		}

		ZDate IDutyData.DateForDutyCalculation
		{
			get { return EffectiveDateForDutyRate; }
		}

		ZDecimal IDutyData.Quantity1
		{
			get { return RoundedQuantity.GetRoundedQuantity1(this, JI_CustomsQuantity); }
		}

		ZString IDutyData.UQ1
		{
			get { return JI_CustomsUnitQty; }
		}

		ZDecimal IDutyData.Quantity2
		{
			get { return RoundedQuantity.GetRoundedQuantity2(this, JI_CustomsSecondQuantity); }
		}

		ZString IDutyData.UQ2
		{
			get { return JI_CustomsSecondUnitQty; }
		}

		ZDecimal IDutyData.Quantity3
		{
			get { return RoundedQuantity.GetRoundedQuantity3(this, JI_CustomsThirdQuantity); }
		}

		ZString IDutyData.UQ3
		{
			get { return JI_CustomsThirdUnitQty; }
		}

		ZDecimal IDutyData.CustomsValue
		{
			get { return CustomsValueDeciderForInvoiceLine.GetCustomsValue(this, false, true).Round(0); }
		}

		ZDecimal IDutyData.SupCustomsValue
		{
			get { return ZDecimal.Zero; }
		}

		ZString IDutyData.SpecialProgramsIndicatorPrimary
		{
			get
			{
				ZString result = ZString.Empty;

				if (Factory.GetCachedValue<PrimarySpecProgramIndicatorList>().ContainsCode(US_SPI))
				{
					result = US_SPI;

					result = this.GetEffectiveSPIForDutyCalculation(result);
				}

				return result;
			}
		}

		public ZString SpecialProgramsIndicatorCountry
		{
			get
			{
				ZString result = ZString.Empty;

				if (AddInfoLookups.US_SpecialProgramList.ContainsCode(US_SPI))
				{
					result = US_SPI;

					result = this.GetEffectiveSPIForDutyCalculation(result);
				}

				return result;
			}
		}

		ZString IDutyData.CountryOfOrigin
		{
			get { return US_UC_NKCountryOfOrigin; }
		}

		ZString IDutyData.SpecialProgramsIndicatorSecondary
		{
			get { return US_SecondarySPI; }
		}

		ZString IDutyData.SelectedRateType
		{
			get { return US_SelectedRateType; }
		}

		ZString IDutyData.EntryType
		{
			get { return Declaration != null ? Declaration.US_EntryType : ZString.Empty; }
		}

		bool IDutyData.IsClearedInPR
		{
			get { return Declaration != null && Declaration.IsClearedInPR; }
		}

		bool IDutyData.IsAMSFeeExempt
		{
			get { return IsAMSFeeExempt; }
		}

		bool IDutyData.IsCottonFeeExemptIndicated
		{
			get { return IsCottonFeeExemptIndicated; }
		}

		IDutyData IDutyData.ParentTariffLine
		{
			get { return HasEmptySupTariff ? ParentTariffLine : SupplementaryParentTariffIDutyData; }
		}

		internal SupplementaryParentTariffIDutyData SupplementaryParentTariffIDutyData
		{
			get { return supplementaryParentTariffIDutyData ?? (supplementaryParentTariffIDutyData = new SupplementaryParentTariffIDutyData(this)); }
		}
		SupplementaryParentTariffIDutyData supplementaryParentTariffIDutyData;

		ZDecimal IDutyData.ValueForADD
		{
			get { return ValueForADD.Round(0); }
		}

		ZDecimal IDutyData.ADDDepositRate
		{
			get { return US_ADDDepositRate; }
		}

		ZDecimal IDutyData.ValueForCVD
		{
			get { return ValueForCVD.Round(0); }
		}

		ZDecimal IDutyData.CVDDepositRate
		{
			get { return US_CVDDepositRate; }
		}

		ZDecimal IDutyData.ADDQuantity
		{
			get { return US_ADDQty; }
		}

		ZString IDutyData.ADDCaseRateTypeQualifier
		{
			get { return US_ADDDepositRateIndicator; }
		}

		ZDecimal IDutyData.CVDQuantity
		{
			get { return US_CVDQty; }
		}

		ZString IDutyData.CVDCaseRateTypeQualifier
		{
			get { return US_CVDDepositRateIndicator; }
		}

		ZDecimal? IDutyData.ADDutyManual
		{
			get
			{
				ZDecimal? result = null;

				if (IsADDManual && HasEmptySupTariff)
				{
					result = US_ADDuty;
				}

				return result;
			}
		}

		ZDecimal? IDutyData.CVDutyManual
		{
			get
			{
				ZDecimal? result = null;

				if (IsCVDManual && HasEmptySupTariff)
				{
					result = US_CVDuty;
				}

				return result;
			}
		}

		bool IDutyData.HasTextileCategoryNo
		{
			get { return !US_TextileCategoryNo_Effective.IsEmpty; }
		}

		bool IDutyData.IsSecondaryTariffLine
		{
			get { return !HasEmptySupTariff || IsSecondaryTariffLine; }
		}

		#region Chapter98

		bool IDutyData.IsCombineSecondaryTariffLine
		{
			get { return IsCombineSecondaryTariffLine; }
		}

		public bool IsCombineSecondaryTariffLine => Factory.GetValue(ref isCombineSecondaryTariffLineCached,
			() => (!US_SupAdditionalTariff1.IsEmpty && (!ChildLines.Any() || IsSetXLine || IsSetVLine)) || (Chapter98Helper.IsCombineSecondaryTariffLine(this) && !ParentTariffLine.HasSecondaryTariffsFromTariffRule));
		CachedProperty<bool> isCombineSecondaryTariffLineCached;

		IEnumerable<IDutyData> IDutyData.CombineChildLines => Factory.GetValue(ref combineChildLinesCached, () =>
		{
			var result = new List<IDutyData>();
			if (IsCombineParentTariffLine)
			{
				result.AddRange(ChildLines);
			}
			else if (!US_SupAdditionalTariff1.IsEmpty && (!ChildLines.Any() || IsSetXLine || IsSetVLine))
			{
				result.Add(this);
			}

			return result.ToArray();
		});
		CachedProperty<IEnumerable<IDutyData>> combineChildLinesCached;

		IEnumerable<IDutyData> IDutyData.CombineAllLines => Factory.GetValue(ref combineAllLinesCached, () =>
		{
			var result = new List<IDutyData>();
			var parentLine = this.ParentTariffLine ?? this;
			var combinedChildLiness = ((IDutyData)parentLine).CombineChildLines.ToList();
			foreach (var line in combinedChildLiness)
			{
				result.Add(line);
			}

			if (this.IsParentLine && this.IsSetVLine && this.IsCombineParentTariffLine)
			{
				var combinedVChildLiness = ((IDutyData)this).CombineChildLines.ToList();
				foreach (var line in combinedVChildLiness)
				{
					result.Add(line);
				}
			}

			if (result.Count > 0 && !result.Contains(parentLine))
			{
				result.Insert(0, parentLine);
			}

			return result;
		});
		CachedProperty<IEnumerable<IDutyData>> combineAllLinesCached;

		IDutyData IDutyData.CombineParentLine
		{
			get => !US_SupAdditionalTariff1.IsEmpty && (!ChildLines.Any() || IsSetXLine || IsSetVLine) ? this : this.ParentTariffLine;
		}

		IReadOnlyList<ZString> IDutyData.SupTariffs => Factory.GetValue(ref supTariffsCached, () =>
		{
			var result = new List<ZString>();

			void AddSupTariffToListIfNotEmpty(ZString tariffNumber)
			{
				if (!tariffNumber.IsEmpty && tariffNumber != TariffViewAsCodeDescription.NotApplicableCode)
				{
					result.Add(tariffNumber);
				}
			}

			AddSupTariffToListIfNotEmpty(US_SupAdditionalTariff1);
			AddSupTariffToListIfNotEmpty(US_SupAdditionalTariff2);
			AddSupTariffToListIfNotEmpty(US_SupAdditionalTariff3);
			AddSupTariffToListIfNotEmpty(US_SupAdditionalTariff4);
			AddSupTariffToListIfNotEmpty(US_SupAdditionalTariff5);
			AddSupTariffToListIfNotEmpty(US_SupTariff);

			return result.ToArray();
		});
		CachedProperty<ZString[]> supTariffsCached;

		public bool Is98SecondaryOrParentTariffLine
		{
			get { return IsCombineParentTariffLine && Chapter98Helper.Is98Tariff(US_SupTariff) || Chapter98Helper.Is98SecondaryTariffLine(this); }
		}

		public bool IsCombineParentTariffLine
		{
			get { return ChildLines.Any(x => x.IsCombineSecondaryTariffLine); }
		}

		bool HasSecondaryTariffsFromTariffRule
		{
			get
			{
				var result = ZBool.False;
				if (JI_ParentID.IsEmpty && ImportTariff != null && Pivot == null)
				{
					var tariffRule = ImportTariff.GetTariffRuleIfApplies(TariffRuleList.Codes.EligibleForSecondaryTariffNumbers, EffectiveDateForDutyRate);
					result = tariffRule != null && tariffRule.SecondaryTariffs.Count == 1;
				}
				return result;
			}
		}

		/// <summary>
		/// Is a secondary tariff line as opposed to a set component line?
		/// </summary>
		public bool IsSecondaryTariffLine
		{
			get { return !IsSetVLine && IsChildLine; }
		}

		ZBool IDutyData.IsSetXLine => IsSetXLine;

		#endregion

		#endregion

		#region IFeeCalculationDataProvider Members

		bool IFeeCalculationDataProvider.IsACS
		{
			get => Declaration != null && !Declaration.IsACE && Declaration.IsImport;
		}

		public bool IsDomesticMerchandise
		{
			get { return ImportEntryType == EntryTypeList.Codes.ConsumptionFTZ && US_ZoneStatus == ZoneStatusList.Codes.Domestic; }
		}

		public bool IsFeeOverriden(string feeCode)
		{
			FeeCusCodeData result = FeeCusCodes.GetFirstElementHaving(feeCode);

			return result != null && result.CY_IsOverridden;
		}

		void IFeeCalculationDataProvider.SetFeeResult(string feeCode, decimal amount, FeeCalculationInternalData feeCalculationInternalData)
		{
			ZDecimal existingAmount = FeeCusCodes.GetFeeOrChargeAmount(feeCode);
			FeeCusCodes.UpdateOrAddCharge(feeCode, existingAmount + amount, MandatoryFeesForTariff);
		}

		bool IFeeCalculationDataProvider.IsFeeOverriden(string feeCode)
		{
			FeeCusCodeData fee = FeeCusCodes.GetFirstElementHaving(feeCode);
			return fee != null && fee.CY_IsOverridden;
		}

		ZString IFeeCalculationDataProvider.GetSelectedRateType(string feeCode)
		{
			FeeCusCodeData feeData = FeeCusCodes.GetFirstElementHaving(feeCode);

			return feeData != null ? feeData.CY_SelectedRateType : ZString.Empty;
		}

		public ZDateTime DateForMPFCalculation
		{
			get { return Declaration?.DateForMPFCalculation ?? ZDateTime.Today; }
		}

		ZDecimal? IFeeCalculationDataProvider.OverriddenTaxRate
		{
			get { return GetOverriddenTaxRate(US_TaxApply, US_TaxRate, US_TaxCode, ImportTariff); }
		}

		public ZString OverriddenTaxRateUQ
		{
			get { return GetOverriddenTaxRateUQ(US_TaxRateS); }
		}

		internal ZDecimal? GetOverriddenTaxRate(ZString taxApply, ZDecimal taxRate, ZString taxCode, USCTariff importTariff)
		{
			ZDecimal? result = null;

			if (taxApply == TaxApplyList.Codes.Override)
			{
				result = taxRate;
			}
			else if (importTariff != null && importTariff.IsTaxApplicable)
			{
				if (taxApply == TaxApplyList.Codes.No || taxApply.IsEmpty)
				{
					result = ZDecimal.Zero;
				}
				else if (taxApply == TaxApplyList.Codes.Yes)
				{
					if (importTariff.UE_Tariff == "2403102050" || importTariff.UE_Tariff == "2403102080")
					{
						var dutyRate = importTariff.DutyRates.GetRateForTaxFeeClassCode(taxCode);
						result = dutyRate != null ? dutyRate.UD_TaxFeeSpecificRate : ZDecimal.Zero;
					}
					else if (importTariff.IsTaxComputationUnknown)
					{
						result = taxRate;
					}
				}
			}

			return result;
		}

		internal ZString GetOverriddenTaxRateUQ(ZString rateDesc)
		{
			return AppendixBTaxRateList.GetUQ(rateDesc);
		}

		ZString IFeeCalculationDataProvider.TaxCode
		{
			get { return US_TaxCode; }
		}

		ZString IFeeCalculationDataProvider.TaxRateType
		{
			get { return US_TaxRateT; }
		}

		ZString IFeeCalculationDataProvider.TaxComputationCode
		{
			get { return TaxComputationCode; }
		}

		ZDecimal IFeeCalculationDataProvider.TaxRateQuantity
		{
			get { return US_TaxQty; }
		}

		ZDecimal IFeeCalculationDataProvider.DairyQty
		{
			get { return DairyQty; }
		}

		ZString IFeeCalculationDataProvider.VisaNumber
		{
			get
			{
				return US_VisaNo;
			}
		}

		public ZString TaxComputationCode => Factory.GetValue(ref taxComputationCodeCached, () =>
		{
			var taxRateS = (IsCBMAProductClaimAndIsNotCBMA23Effective && IsTaxRateOverridden) ? (ZString)AppendixBTaxRateList.Codes.Specify : US_TaxRateS;
			return AppendixBTaxRateList.GetComputationCode(taxRateS, JI_Tariff, JI_CustomsUnitQty, JI_CustomsSecondUnitQty);
		});
		CachedProperty<ZString> taxComputationCodeCached;

		public ZString OrigTaxComputationCode
		{
			get { return AppendixBTaxRateList.GetComputationCode(US_R_OrigTaxRateS, US_R_OrigTariff, US_R_OrigFirstUQ, US_R_OrigSecondUQ); }
		}

		IEnumerable<IDutyData> IFeeCalculationDataProvider.SecondaryLines
		{
			get
			{
				if (HasEmptySupTariff && !IsSecondaryTariffLine)
				{
					foreach (JobComInvoiceLine secondaryLine in SecondaryTariffLines)
					{
						if (!secondaryLine.HasEmptySupTariff)
						{
							yield return new SupplementaryParentTariffIDutyData(secondaryLine);
						}

						yield return secondaryLine;
					}
				}
			}
		}

		#endregion

		#region IChargeApportionee Members

		protected override ZDecimal GetBaseValueToApportionOnCore(CurrencyConverter currencyConverter, string distributeBy)
		{
			ZDecimal result = 0m;

			if (distributeBy == Customs.Common.ChargeDistributeByList.Codes.Value && Declaration != null && !Declaration.IsExport)
			{
				var priceMoney = Money.Empty;
				var localCurrency = Declaration.Country.LocalCurrency;
				if (this.IsCombinedLine() && !IsSetXLine && !IsSetVLine)
				{
					foreach (JobComInvoiceLine line in this.GetCombinedLines())
					{
						priceMoney = currencyConverter.Add(priceMoney, line.JI_LinePriceMoney);
						priceMoney = currencyConverter.Add(priceMoney, new Money(line.TotalOriginalGoodsValueInUSD, localCurrency));
					}
				}
				else
				{
					priceMoney = currencyConverter.Add(priceMoney, JI_LinePriceMoney);
					priceMoney = currencyConverter.Add(priceMoney, new Money(TotalOriginalGoodsValueInUSD, localCurrency));
				}
				result = currencyConverter.ConvertExact(priceMoney, localCurrency).Amount;
			}
			else
			{
				result = base.GetBaseValueToApportionOnCore(currencyConverter, distributeBy);
			}

			return result;
		}

		protected override bool IsValidToApportionToCore
		{
			get { return !IsSetXLine && (!this.IsCombinedLine() || this.IsNormalTariffLine()); }
		}

		#endregion

		#region Government Agencies Data - EPA, FSIS, Lacey Act, NMFS, DEA

		public bool IsACECargoCertificationMode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsACECargoCertificationMode;
			}
		}

		public bool IsACSCargoCertificationMode
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsACSCargoCertificationMode;
			}
		}

		public bool DoesMatchCertificationMode(string agencyCode)
		{
			return !IsACSCargoCertificationMode && IsPGA(agencyCode)
				|| IsACSCargoCertificationMode && !IsPGA(agencyCode) && (agencyCode != GovernmentAgencyProgramCodeList.Codes.Lacey || !IsACE)//Lacey cannot be sent in ACE declaration unless it is certified in ACE.
				|| agencyCode == GovernmentAgencyProgramCodeList.Codes.FSIS//FSIS is both OGA and PGA
				|| agencyCode == GovernmentAgencyProgramCodeList.Codes.FDA && Declaration.IsACEStandalonePNWithoutENSAndCRL //standalone PN for ACE can be submitted without ENS or CRL
				;
		}

		public bool IsPGA(string agencyCode)
		{
			switch (agencyCode)
			{
				case GovernmentAgencyProgramCodeList.Codes.DOT:
				case GovernmentAgencyProgramCodeList.Codes.FCC:
					return false;

				case GovernmentAgencyProgramCodeList.Codes.Lacey:
					return !IsACSCargoCertificationMode;

				case GovernmentAgencyProgramCodeList.Codes.FDA:
					return Declaration.CanHavePGAFDA;

				default:
					return true;
			}
		}

		public ZBool IsPGAIndicatorAllowedToBeDefaulted(string agencyCode)
		{
			var result = true;
			var declaration = Declaration;

			if (declaration != null && declaration.IsACECargoCertificationMode)
			{
				result = GovernmentAgencyProgramCodeList.IsPGAAllowed(declaration.US_EnableENS, declaration.US_EnableCRL, declaration.US_CertifyCargoRelease, declaration.US_PGAExpeditedRelease, declaration.IsConsumptionFTZ && declaration.IsWeeklyEstimateFilingDate, declaration.US_EntryType, agencyCode);
			}

			return result;
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public VehicleCollection VehicleLines
		{
			get
			{
				if (vehicleLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						vehicleLines = Factory.GetCachedValue("FakeyVNEs", delegate
						{ return new VehicleCollection(nullInvoiceLine); });
					}
					else
					{
						vehicleLines = new VehicleCollection(this);
						vehicleLines.Load();
						RegisterEditableChildObject(vehicleLines);
					}
				}
				return vehicleLines;
			}
		}
		VehicleCollection vehicleLines;

		public ZBool HasVNEDetails
		{
			get { return VehicleLines.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public AMSCollection AMSLines
		{
			get
			{
				if (amsLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						amsLines = Factory.GetCachedValue("FakeyAMSs", delegate
						{ return new AMSCollection(nullInvoiceLine); });
					}
					else
					{
						amsLines = new AMSCollection(this);
						amsLines.Load();
						RegisterEditableChildObject(amsLines);
					}
				}
				return amsLines;
			}
		}
		AMSCollection amsLines;

		public ZBool HasAMSDetails
		{
			get { return AMSLines.Count > 0; }
		}

		public ZBool HasAMSMO4
		{
			get { return AMSLines.OfType<AMS>().Any(x => x.US_Program == AMSProgramList.Codes.MO4); }
		}

		public ZBool HasAMSNOP
		{
			get { return AMSLines.OfType<AMS>().Any(x => x.IsNOPProgram); }
		}

		public ZBool HasAMSOR1
		{
			get { return AMSLines.OfType<AMS>().Any(x => x.US_Program == AMSProgramList.Codes.OR1); }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public ATFCollection ATFLines
		{
			get
			{
				if (atfLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						atfLines = Factory.GetCachedValue("FakeyATFs", delegate
						{ return new ATFCollection(nullInvoiceLine); });
					}
					else
					{
						atfLines = new ATFCollection(this);
						atfLines.Load();
						RegisterEditableChildObject(atfLines);
					}
				}
				return atfLines;
			}
		}
		ATFCollection atfLines;

		public ZBool HasATFDetails
		{
			get { return ATFLines.Count > 0; }
		}

		public ZBool HasFSISDetails
		{
			get { return FSISLines.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public PGACollection LaceyActLines
		{
			get
			{
				if (laceyActLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						laceyActLines = Factory.GetCachedValue("FakeyLACEYs", delegate
						{ return new PGACollection(nullInvoiceLine); });
					}
					else
					{
						laceyActLines = new PGACollection(this);
						laceyActLines.Load();
						RegisterEditableChildObject(laceyActLines);
					}
				}
				return laceyActLines;
			}
		}
		PGACollection laceyActLines;

		public bool HasLaceyActData
		{
			get { return LaceyActLines.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public USInvoiceLineFSISLineCollection FSISLines
		{
			get
			{
				if (fFSISLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						fFSISLines = Factory.GetCachedValue("FakeyFSISs", delegate
						{ return new USInvoiceLineFSISLineCollection(nullInvoiceLine); });
					}
					else
					{
						fFSISLines = new USInvoiceLineFSISLineCollection(this);
						fFSISLines.Load();
						RegisterEditableChildObject(fFSISLines);
					}
				}
				return fFSISLines;
			}
		}
		USInvoiceLineFSISLineCollection fFSISLines;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public OMCHeaderCollection OMCHeaders
		{
			get
			{
				if (fOMCHeaders == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						fOMCHeaders = Factory.GetCachedValue("FakeyOMCs", delegate
						{ return new OMCHeaderCollection(nullInvoiceLine); });
					}
					else
					{
						fOMCHeaders = new OMCHeaderCollection(this);
						fOMCHeaders.Load();
						RegisterEditableChildObject(fOMCHeaders);
					}
				}
				return fOMCHeaders;
			}
		}
		OMCHeaderCollection fOMCHeaders;

		public bool HasOMCHeaders
		{
			get { return OMCHeaders.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public PesticideCollection PSTLines
		{
			get
			{
				if (pstLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						pstLines = Factory.GetCachedValue("FakeyPSTs", delegate
						{ return new PesticideCollection(nullInvoiceLine); });
					}
					else
					{
						pstLines = new PesticideCollection(this);
						pstLines.Load();
						RegisterEditableChildObject(pstLines);
					}
				}
				return pstLines;
			}
		}
		PesticideCollection pstLines;

		public ZBool HasPSTLines
		{
			get { return PSTLines.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public NHTSAHeaderCollection NHTSALines
		{
			get
			{
				if (fNHTSALines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						fNHTSALines = Factory.GetCachedValue("FakeyNHTSAs", delegate
						{ return new NHTSAHeaderCollection(nullInvoiceLine); });
					}
					else
					{
						fNHTSALines = new NHTSAHeaderCollection(this);
						fNHTSALines.Load();
						RegisterEditableChildObject(fNHTSALines);
					}
				}
				return fNHTSALines;
			}
		}
		NHTSAHeaderCollection fNHTSALines;

		public ZBool HasNHTSADetails
		{
			get { return NHTSALines.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public CPSCHeaderCollection CPSCHeaders
		{
			get
			{
				if (cpscHeaders == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						cpscHeaders = Factory.GetCachedValue("FakeyCPSCs", delegate
						{ return new CPSCHeaderCollection(nullInvoiceLine); });
					}
					else
					{
						cpscHeaders = new CPSCHeaderCollection(this);
						cpscHeaders.Load();
						RegisterEditableChildObject(cpscHeaders);
					}
				}
				return cpscHeaders;
			}
		}
		CPSCHeaderCollection cpscHeaders;

		public ZBool HasCPSCHeaders
		{
			get { return CPSCHeaders.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public USHFCHeaderCollection USHFCHeaders
		{
			get
			{
				if (ushfcHeaders == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						ushfcHeaders = Factory.GetCachedValue("FakeyUSHFCs", delegate
						{ return new USHFCHeaderCollection(nullInvoiceLine); });
					}
					else
					{
						ushfcHeaders = new USHFCHeaderCollection(this);
						ushfcHeaders.Load();
						RegisterEditableChildObject(ushfcHeaders);
					}
				}
				return ushfcHeaders;
			}
		}
		USHFCHeaderCollection ushfcHeaders;

		public ZBool HasUSHFCHeaders
		{
			get { return USHFCHeaders.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public APHISHeaderCollection APHISHeaders
		{
			get
			{
				if (aphisHeaders == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						aphisHeaders = Factory.GetCachedValue("FakeyAPHISs", delegate
						{ return new APHISHeaderCollection(nullInvoiceLine); });
					}
					else
					{
						aphisHeaders = new APHISHeaderCollection(this);
						aphisHeaders.Load();
						RegisterEditableChildObject(aphisHeaders);
					}
				}
				return aphisHeaders;
			}
		}
		APHISHeaderCollection aphisHeaders;

		public ZBool HasAPHISHeaders
		{
			get { return APHISHeaders.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public FWSHeaderCollection FWSHeaders
		{
			get
			{
				if (fwsHeaders == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						fwsHeaders = Factory.GetCachedValue("FakeyFWSs", delegate
						{ return new FWSHeaderCollection(nullInvoiceLine); });
					}
					else
					{
						fwsHeaders = new FWSHeaderCollection(this);
						fwsHeaders.Load();
						RegisterEditableChildObject(fwsHeaders);
					}
				}
				return fwsHeaders;
			}
		}
		FWSHeaderCollection fwsHeaders;

		public ZBool HasFWSHeaders
		{
			get { return FWSHeaders.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public NMFSLineCollection NMFSLines
		{
			get
			{
				if (nmfsLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						nmfsLines = Factory.GetCachedValue("FakeyNMFSs", delegate
						{ return new NMFSLineCollection(nullInvoiceLine); });
					}
					else
					{
						nmfsLines = new NMFSLineCollection(this);
						nmfsLines.Load();
						RegisterEditableChildObject(nmfsLines);
					}
				}
				return nmfsLines;
			}
		}
		NMFSLineCollection nmfsLines;

		public IEnumerable<NMFSLine> NMFS370Lines
		{
			get { return NMFSLines.Cast<NMFSLine>().Where(x => x.Is370ProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSCOALines
		{
			get { return NMFSLines.Cast<NMFSLine>().Where(x => x.IsCOAProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSSIMPLines
		{
			get { return NMFSLines.Cast<NMFSLine>().Where(x => x.IsSIMProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSAMRLines
		{
			get { return NMFSLines.Cast<NMFSLine>().Where(x => x.IsAMRProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSHMSLines
		{
			get { return NMFSLines.Cast<NMFSLine>().Where(x => x.IsHMSProgramType); }
		}

		public ZBool HasNMFSLines
		{
			get { return NMFSLines.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public TTBLineCollection TTBLines
		{
			get
			{
				if (ttbLines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						ttbLines = Factory.GetCachedValue("FakeyTTBs", delegate
						{ return new TTBLineCollection(nullInvoiceLine); });
					}
					else
					{
						ttbLines = new TTBLineCollection(this);
						ttbLines.Load();
						RegisterEditableChildObject(ttbLines);
					}
				}
				return ttbLines;
			}
		}
		TTBLineCollection ttbLines;

		public ZBool HasTTBLines
		{
			get { return TTBLines.Count > 0; }
		}

		public OGAAgencyRequirementCollection OGAAgencyRequirements
		{
			get
			{
				if (ogaAgencyRequirements == null)
				{
					ogaAgencyRequirements = new OGAAgencyRequirementCollection(new InvoiceLinePGAAgencyRequirementsProvider(this));
					ogaAgencyRequirements.Populate();
				}
				return ogaAgencyRequirements;
			}
		}
		OGAAgencyRequirementCollection ogaAgencyRequirements;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public ACEFDACollection ACE_FDALines
		{
			get
			{
				if (aceFDALines == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						aceFDALines = Factory.GetCachedValue("FakeyACEFDAs", delegate
						{ return new ACEFDACollection(nullInvoiceLine); });
					}
					else
					{
						aceFDALines = new ACEFDACollection(this);
						aceFDALines.Load();
						RegisterEditableChildObject(aceFDALines);
					}
				}
				return aceFDALines;
			}
		}
		internal ACEFDACollection aceFDALines;

		public ZBool HasACE_FDALines
		{
			get { return ACE_FDALines.Count > 0; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DEAHeaderCollection DEAHeaders
		{
			get
			{
				if (deaHeaders == null)
				{
					if (Declaration != null && Declaration.IsRecon)
					{
						var nullInvoiceLine = Factory.GetNull<JobComInvoiceLine>();
						deaHeaders = Factory.GetCachedValue("FakeyDEAs", delegate
						{ return new DEAHeaderCollection(nullInvoiceLine); });
					}
					else
					{
						deaHeaders = new DEAHeaderCollection(this);
						deaHeaders.Load();
						RegisterEditableChildObject(deaHeaders);
					}
				}
				return deaHeaders;
			}
		}
		DEAHeaderCollection deaHeaders;

		public ZBool HasDEAHeaders
		{
			get { return DEAHeaders.Count > 0; }
		}

		#endregion

		#region New readonly
		bool US_VNEDisclaimReason_ReadOnly
		{
			get { return US_VNEInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_NHTDisclaimReason_ReadOnly
		{
			get { return US_NHTSAIndicator != OGAIndicatorList.Codes.Disclaimed; }
		}

		[ReadOnlyMember(nameof(US_NHTDisclaimReason_ReadOnly))]
		public override ZString US_NHTDisclaimReason
		{
			get { return base.US_NHTDisclaimReason; }
			set
			{
				if (!IsCopying && US_NHTDisclaimReason != value)
				{
					PGADataChangeTrackerSupporter.LoadAllPGARelatedDataIfNeeded();
				}
				base.US_NHTDisclaimReason = value;
			}
		}

		bool US_NMFS370DisclaimReason_ReadOnly
		{
			get { return US_NMFS370Ind != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_NMFSAMRDisclaimReason_ReadOnly
		{
			get { return US_NMFSAMRInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_NMFSHMSDisclaimReason_ReadOnly
		{
			get { return US_NMFSHMSInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_ODSDisclaimReason_ReadOnly
		{
			get { return US_ODSInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_OMCDisclaimReason_ReadOnly
		{
			get { return US_OMCInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_FSISDisclaimReason_ReadOnly
		{
			get { return US_FSISInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_PSTDisclaimProgram_ReadOnly
		{
			get { return US_PSTIndicator != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_PSTDisclaimReason_ReadOnly
		{
			get { return US_PSTIndicator != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_LaceyDisclaimReason_ReadOnly
		{
			get { return US_LaceyIndicator != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_TSCADisclaimReason_ReadOnly
		{
			get { return US_TSCAInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_AMSDisclaimProgram_ReadOnly
		{
			get { return US_AMSInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_AMSDisclaimReason_ReadOnly
		{
			get { return US_AMSInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_NOPDisclaimReason_ReadOnly
		{
			get { return US_NOPInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_CPSCDisclaimReason_ReadOnly
		{
			get { return US_CPSCInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_DEADisclaimReason_ReadOnly
		{
			get { return US_DEAInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		bool US_HFCDisclaimReason_ReadOnly
		{
			get { return US_HFCInd != OGAIndicatorList.Codes.Disclaimed; }
		}

		#endregion

		#region Export PGA Data

		public ExportPGAAgencyRequirementCollection ExportPGAAgencyRequirements
		{
			get
			{
				if (exportPGAAgencyRequirements == null)
				{
					exportPGAAgencyRequirements = new ExportPGAAgencyRequirementCollection(new ExportPGAInvoiceLineRequirementsProvider(this));
					exportPGAAgencyRequirements.Populate();
				}
				return exportPGAAgencyRequirements;
			}
		}
		ExportPGAAgencyRequirementCollection exportPGAAgencyRequirements;

		internal ZBool IsExportNMFSDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_NMFSHMSInd); }
		}

		internal ZBool IsAMSDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_AMSInd); }
		}

		internal ZBool IsNOPDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_NOPInd); }
		}

		internal ZBool IsExportEPADeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_PSTIndicator); }
		}

		internal ZBool IsATFDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_ATFInd); }
		}

		internal ZBool IsFWSDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(US_FWSInd); }
		}

		#endregion

		#region FTZ

		internal ZBool IsFTZBTAInvolved
		{
			get { return RequiresPriorNoticeReporting() && US_F_PNDisclaimer != YesNoDefaultList.Codes.Yes; }
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			Enterprise.MasterFiles.Business.Testing.DGSubstanceTestHelper.CreateIfDoesntExist("123", "a", "IMO", additionalInitialisation: (subs) => subs.DG_FlashPoint = "below 0");

			JI_HazMatCode = "A";
			JI_HazMatCodeQualifier = "U";
			US_HazMatDesc = "Desc";
			US_HazMatClassDesc = "Description";
		}
#endif
		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USAIILine, typeof(AIILine));
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISHeader, typeof(APHISHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USFWSHeader, typeof(FWSHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USFDA, typeof(FDA));
			result.Add(CusAddInfoTypeAttribute.Codes.USACEFDA, typeof(ACEFDA));
			result.Add(CusAddInfoTypeAttribute.Codes.USFCC, typeof(FCC));
			result.Add(CusAddInfoTypeAttribute.Codes.USDOT, typeof(DOT));
			result.Add(CusAddInfoTypeAttribute.Codes.USPGACommon, typeof(PGA));
			result.Add(CusAddInfoTypeAttribute.Codes.USDrawbackNAFTA, typeof(DrawbackNAFTA));
			result.Add(CusAddInfoTypeAttribute.Codes.USFSISCertificate, typeof(USInvoiceLineFSISLine));
			result.Add(CusAddInfoTypeAttribute.Codes.USPGAVehicle, typeof(Vehicle));
			result.Add(CusAddInfoTypeAttribute.Codes.USNMFSLine, typeof(NMFSLine));
			result.Add(CusAddInfoTypeAttribute.Codes.USATF, typeof(ATF));
			result.Add(CusAddInfoTypeAttribute.Codes.USOMCHeader, typeof(OMCHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USAMS, typeof(AMS));
			result.Add(CusAddInfoTypeAttribute.Codes.USTTBLine, typeof(TTBLine));
			result.Add(CusAddInfoTypeAttribute.Codes.USNHTSAHeader, typeof(NHTSAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USPesticide, typeof(Pesticide));
			result.Add(CusAddInfoTypeAttribute.Codes.USCPSCHeader, typeof(CPSCHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USDEAHeader, typeof(DEAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USDrawbackOtherFee, typeof(DrawbackOtherFee));
			result.Add(CusAddInfoTypeAttribute.Codes.USDrawbackAdditionalImportTariffNumber, typeof(DrawbackAdditionalImportTariffNumber));
			result.Add(CusAddInfoTypeAttribute.Codes.USHFCHeader, typeof(USHFCHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USNMFSHarvestingDetail, typeof(FishingInformation));
			return result;
		}

		#endregion

		public EffectiveValueManager EffectiveValueManager => effectiveValueManager ?? (effectiveValueManager = new EffectiveValueManager());
		EffectiveValueManager effectiveValueManager;

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.InvoiceLineNumberRange, typeof(InvoiceLineGroupingRange));
			result.Add(CusCodeDataTypeList.Codes.Fee, typeof(FeeCusCodeData));
			result.Add(CusCodeDataTypeList.Codes.ReconEntryOriginalCharge, typeof(ReconEntryOriginalCharge));
			result.Add(CusCodeDataTypeList.Codes.ReconRefundedCharge, typeof(ReconRefundedCharge));
			result.Add(CusCodeDataTypeList.Codes.LicenceAndPermit, typeof(LicenceAndPermit));
			result.Add(CusCodeDataTypeList.Codes.CensusWarningOverride, typeof(CensusWarningOverride));
			result.Add(CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber, typeof(DrawbackAdditionalExportTariffNumber));
			result.Add(CusCodeDataTypeList.Codes.IORBusinessRules, typeof(RestrictedCode));
			result.Add(CusCodeDataTypeList.Codes.MiningInformationForSanctions, typeof(MiningInformation));
			return result;
		}

		#endregion

		#region IAddressDetailsWithContact Members

		IAddressDetails ICustomsBrokerDetails.Address
		{
			get
			{
				var declaration = Declaration;
				return declaration == null ? null : declaration.BranchIAddressDetails;
			}
		}

		ZString ICustomsBrokerDetails.ContactName
		{
			get
			{
				var invoice = InvoiceHeader;
				return invoice == null ? ZString.Empty : invoice.US_FDAContactName;
			}
		}

		ZString ICustomsBrokerDetails.ContactPhone
		{
			get
			{
				var invoice = InvoiceHeader;
				return invoice == null ? ZString.Empty : invoice.US_FDAContactPhoneNo;
			}
		}

		ZString ICustomsBrokerDetails.ContactEmail
		{
			get
			{
				var invoice = InvoiceHeader;
				return invoice == null ? ZString.Empty : invoice.US_FDAContactEmail;
			}
		}

		#endregion

		#region IInvoiceLineProvider Members
		JobComInvoiceLine IInvoiceLineProvider.InvoiceLine { get { return this; } }
		#endregion

		#region ITSCAData Members
		ZString ITSCAData.ContactName
		{
			get { return US_FDAContactName; }
		}

		ZString ITSCAData.ContactPhone
		{
			get { return US_FDAContactPhoneNo; }
		}

		ZString ITSCAData.ContactEmail
		{
			get { return US_FDAContactEmail; }
		}

		ZString ITSCAData.TSCACertificationCode
		{
			get { return US_TSCACertification.IsEmpty ? string.Empty : US_TSCACertification == TSCAIndicatorList.Codes.TSCAPositive ? "EP4" : "EP5"; }
		}

		ZDate ITSCAData.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_TSCASignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_TSCASignDate = value;
				}
			}
		}

		ZString ITSCAData.DeclarationCertificate
		{
			get
			{
				var tscaData = this as ITSCAData;
				return tscaData != null && tscaData.CertifySignatureDate.IsValid ? "Y" : "";
			}
		}

		ZInt ITSCAData.TSCALineNumber
		{
			get { return US_TSCALineNumber; }
			set { US_TSCALineNumber = value; }
		}

		ZInt ITSCAData.ODSLineNumber
		{
			get { return US_ODSLineNumber; }
			set { US_ODSLineNumber = value; }
		}

		#endregion

		#region IAESEPA Members

		ZString IAESEPA.EPAConsentNumber
		{
			get { return US_EPAConsentNumber; }
		}

		ZString IAESEPA.HazWasteManifestTrackingNumber
		{
			get { return US_HazWasteTrackingNo; }
		}

		ZDecimal IAESEPA.EPANetQuantity
		{
			get { return US_EPANetQty; }
		}

		ZString IAESEPA.EPANetQuantityUQ
		{
			get { return US_EPANetQtyUQ; }
		}

		#endregion

		#region IAESAMS Members

		ZString IAESAMS.ExportCertificateNo
		{
			get { return US_ExportCertificateNo; }
		}

		#endregion

		#region IPGALineStatus Members

		public CusDispositionCollection PGALineCusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return JobComInvoiceLineSchema.Constants.Prefix; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}

		#endregion

		#region IInvoiceLineDutyDataProvider Members

		IFees IInvoiceLineDutyDataProvider.FeeCusCodes => FeeCusCodes;
		IEnumerable<IEntryLineDutyDataProvider> IInvoiceLineDutyDataProvider.AllEntryLines
		{
			get
			{
				var cusEntryLine = CusEntryLine;
				yield return cusEntryLine;
				var messageType = cusEntryLine.Header.CH_MessageType;
				foreach (CusEntryLine entryLine in AdditionalEntryLineLinks.GetEntryLineFor(messageType))
				{
					yield return entryLine;
				}
			}
		}
		ZShort? IInvoiceLineDutyDataProvider.InvoiceDisplaySequence => InvoiceHeader?.JZ_InvoiceDisplaySequence;
		ZBool IInvoiceLineDutyDataProvider.IsQuotaProductExclusion => !US_ProductExclusion.IsEmpty && !US_ExclusionNumber.IsEmpty && EntryTypeList.IsQuotaProductExclusionType(Declaration.US_EntryType);
		ZBool IInvoiceLineDutyDataProvider.HasSupTariffOnly => HasSupplementary && JI_Tariff.IsEmpty;

		#endregion

		#region IInvoiceLine Members
		ZString IInvoiceLine.BaseJI_PartNo => base.JI_PartNo;
		IInvoiceLine IInvoiceLine.ProductParentTariffLine => ProductParentTariffLine;
		IEnumerable<IInvoiceLine> IInvoiceLine.ChildLines => ChildLines;
		IEnumerable<IInvoiceLine> IInvoiceLine.ChildVLines => ChildVLines;
		IInvoiceHeader IInvoiceLine.InvoiceHeader => InvoiceHeader;

		ZDate IInvoiceLine.FTZAdmissionEffectiveDateForDutyRate
		{
			get
			{
				if (fTZAdmissionEffectiveDateForDutyRateCached == null)
				{
					fTZAdmissionEffectiveDateForDutyRateCached = new CachedProperty<ZDate>(Factory, () =>
					{
						var date = Declaration is JobDeclaration declaration && declaration.IsFTZAdmission ? declaration.JE_DateOfArrival : ZDateTime.Invalid;
						return date.IsValid ? date.Date : ZDate.Invalid;
					});
				}
				return fTZAdmissionEffectiveDateForDutyRateCached.Value;
			}
		}
		CachedProperty<ZDate> fTZAdmissionEffectiveDateForDutyRateCached;

		ZDate IInvoiceLine.ImportEffectiveDateForDutyRate
		{
			get
			{
				if (importEffectiveDateForDutyRateCached == null)
				{
					importEffectiveDateForDutyRateCached = new CachedProperty<ZDate>(Factory, () =>
					{
						var declaration = Declaration;
						return declaration == null ? ZDate.Today : new DutyFeeDateCalculator().GetDutyFeeDate(declaration);
					});
				}
				return importEffectiveDateForDutyRateCached.Value;
			}
		}
		CachedProperty<ZDate> importEffectiveDateForDutyRateCached;

		RefCurrency IInvoiceLine.Invoice_Currency => InvoiceHeader?.Invoice_Currency;
		bool IInvoiceLine.HasDeclaration => Declaration != null;
		IInvoiceLine IInvoiceLine.ParentTariffLine => ParentTariffLine;
		#endregion

		#region RateSelectionCriteria

		protected override IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new RateSelectionCriteria(this, Universal.Constants.RateTypes.Duty, UniversalReferenceConstants.RateCodes.Codes.Duty, EffectiveCountryOfOrigin);

		public class RateSelectionCriteria : RateSelectionCriteria<JobComInvoiceLine>
		{
			public RateSelectionCriteria(JobComInvoiceLine invoiceLine, ZString rateType, ZString rateCode, ZString tradeGroupCountry)
				: base(invoiceLine, rateType, rateCode)
			{
				TradeGroupCountry = GetTradeGroupCountry(tradeGroupCountry);
			}

			ZString GetTradeGroupCountry(ZString tradeGroupCountry)
			{
				if (CanadaProvinceTerritoryCodes.IsCanadianProvince(tradeGroupCountry) || CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(tradeGroupCountry))
				{
					return (ZString)Core.Constants.CountryCodes.Canada;
				}
				return tradeGroupCountry;
			}

			protected override ZDateTime GetEffectiveDate(JobComInvoiceLine invoiceLine) => invoiceLine.EffectiveDateForDutyRate;
		}

		#endregion

		#region IPGADataChangeTrackerSupporter Members
		IPGADataChangeTrackerSupporter PGADataChangeTrackerSupporter => this;
		PGADataChangeTracker IPGADataChangeTrackerSupporter.Tracker => Declaration?.PGATrackerHelper;
		#endregion

		#region IImportWrappedPropertySupporter Members
		string IImportWrappedPropertySupporter.GetWrappedProperty(string propertyName)
		{
			string result = null;
			if (propertyName.StartsWith("JobUSComInvoiceLine."))
			{
				result = propertyName.Substring(20);
			}
			return result;
		}
		#endregion

		#region IDocAddress members

		[ChildEditable(true)]
		internal JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddress == null)
				{
					fDocAddress = new JobDocAddressDependentCollection(this);
					fDocAddress.Load();
					RegisterEditableChildObject(fDocAddress);
				}
				return fDocAddress;
			}
		}
		JobDocAddressDependentCollection fDocAddress;

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Enterprise.Environment.Env.Security.None;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return !(docAddress == fLocationOfDestruction ||
				docAddress == fLocationOfMerchandise ||
				docAddress == fExporterOrDestroyer);
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			return new JobDocAddressRequirement(addressType);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.DrawbackExporterOrDestroyer,
					DocAddressType.DrawbackLocationOfDestruction,
					DocAddressType.DrawbackLocationOfMerchandise,
					DocAddressType.FDAShipperAddress
				};
			}
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			if (IsACEDrawback)
			{
				return new ACEDrawbackJobComInvoiceLineJobDocAddressValidation(addressToValidate, this);
			}

			return null;
		}

		#endregion

		public bool IsFTZADDCVD => IsFTZAdmission && !(US_ADDCaseNo.IsEmpty && US_CVDCaseNo.IsEmpty);

		public bool IsLVS => false;

		public bool IsFishNotFromRussia => US_UC_NKCountryOfOrigin != Core.Constants.CountryCodes.Russia && (UniversalImportTariff?.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.NonRUCertificationRequired) ?? false);
	}
}
