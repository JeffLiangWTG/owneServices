using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public abstract partial class AutoCusClassPartPivot : Customs.Business.BaseCusClassPartPivot
	{
		protected AutoCusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new partial class Schema : Customs.Business.BaseCusClassPartPivot.Schema
		{
			public const string CD_UC_NKCountryOfOrigin = CusUSClassificationSchema.Constants.CD_UC_NKCountryOfOrigin;
			public const string CD_UC_NKCountryOfExport = CusUSClassificationSchema.Constants.CD_UC_NKCountryOfExport;
			public const string CD_SPI = CusUSClassificationSchema.Constants.CD_SPI;
			public const string CD_ProductClaim = CusUSClassificationSchema.Constants.CD_ProductClaim;
			public const string CD_TaxApplicability = CusUSClassificationSchema.Constants.CD_TaxApplicability;
			public const string CD_TaxCode = CusUSClassificationSchema.Constants.CD_TaxCode;
			public const string CD_TaxRateType = CusUSClassificationSchema.Constants.CD_TaxRateType;
			public const string CD_TaxRateDesc = CusUSClassificationSchema.Constants.CD_TaxRateDesc;
			public const string CD_TaxRate = CusUSClassificationSchema.Constants.CD_TaxRate;
			public const string CD_ReconIssue = CusUSClassificationSchema.Constants.CD_ReconIssue;
			public const string CD_NAFTARecon = CusUSClassificationSchema.Constants.CD_NAFTARecon;
			public const string CD_CottonFeeExempt = CusUSClassificationSchema.Constants.CD_CottonFeeExempt;
			public const string CD_RulingType = CusUSClassificationSchema.Constants.CD_RulingType;
			public const string CD_RulingNumber = CusUSClassificationSchema.Constants.CD_RulingNumber;
			public const string CD_ADDApplicable = CusUSClassificationSchema.Constants.CD_ADDApplicable;
			public const string CD_ADDCaseNo = CusUSClassificationSchema.Constants.CD_ADDCaseNo;
			public const string CD_ADDDepositRateInd = CusUSClassificationSchema.Constants.CD_ADDDepositRateInd;
			public const string CD_ADDBonded = CusUSClassificationSchema.Constants.CD_ADDBonded;
			public const string CD_ADDDecID = CusUSClassificationSchema.Constants.CD_ADDDecID;
			public const string CD_CBMADefaultTaxRate = CusUSClassificationSchema.Constants.CD_CBMADefaultTaxRate;
			public const string CD_CVDApplicable = CusUSClassificationSchema.Constants.CD_CVDApplicable;
			public const string CD_CVDCaseNo = CusUSClassificationSchema.Constants.CD_CVDCaseNo;
			public const string CD_CVDDepositRateInd = CusUSClassificationSchema.Constants.CD_CVDDepositRateInd;
			public const string CD_CVDBonded = CusUSClassificationSchema.Constants.CD_CVDBonded;
			public const string CD_CVDDecID = CusUSClassificationSchema.Constants.CD_CVDDecID;
			public const string CD_ADCVDStat = CusUSClassificationSchema.Constants.CD_ADCVDStat;
			public const string CD_ZoneStatus = CusUSClassificationSchema.Constants.CD_ZoneStatus;
			public const string CD_OA_Manufacturer = CusUSClassificationSchema.Constants.CD_OA_Manufacturer;
			public const string CD_OA_Exporter = CusUSClassificationSchema.Constants.CD_OA_Exporter;
			public const string CD_ActiveIngredientPercentage = CusUSClassificationSchema.Constants.CD_ActiveIngredientPercentage;
			public const string CD_NAFTANetCost = CusUSClassificationSchema.Constants.CD_NAFTANetCost;
			public const string CD_TSCAIndicator = CusUSClassificationSchema.Constants.CD_TSCAIndicator;
			public const string CD_CBTPACertificate = CusUSClassificationSchema.Constants.CD_CBTPACertificate;
			public const string CD_WoolLicenceNo = CusUSClassificationSchema.Constants.CD_WoolLicenceNo;
			public const string CD_SugarCertificate = CusUSClassificationSchema.Constants.CD_SugarCertificate;
			public const string CD_AgricultureLicenceNo = CusUSClassificationSchema.Constants.CD_AgricultureLicenceNo;
			public const string CD_CottonCertificate = CusUSClassificationSchema.Constants.CD_CottonCertificate;
			public const string CD_MiscLicenceNo = CusUSClassificationSchema.Constants.CD_MiscLicenceNo;
			public const string CD_PerUnitCost = CusUSClassificationSchema.Constants.CD_PerUnitCost;
			public const string CD_RX_NKPerUnitCostCurr = CusUSClassificationSchema.Constants.CD_RX_NKPerUnitCostCurr;
			public const string CD_AMMVPerUnit = CusUSClassificationSchema.Constants.CD_AMMVPerUnit;
			public const string CD_AMMVPerUnitCurrency = CusUSClassificationSchema.Constants.CD_AMMVPerUnitCurrency;
			public const string CD_AMMVPercentage = CusUSClassificationSchema.Constants.CD_AMMVPercentage;
			public const string CD_9802USDValuePerUnit = CusUSClassificationSchema.Constants.CD_9802USDValuePerUnit;
			public const string CD_9802ValuePerUnit = CusUSClassificationSchema.Constants.CD_9802ValuePerUnit;
			public const string CD_RX_NK9802ValuePerUnitCurr = CusUSClassificationSchema.Constants.CD_RX_NK9802ValuePerUnitCurr;
			public const string CD_ExportCode = CusUSClassificationSchema.Constants.CD_ExportCode;
			public const string CD_OriginIndicator = CusUSClassificationSchema.Constants.CD_OriginIndicator;
			public const string CD_ECCN = CusUSClassificationSchema.Constants.CD_ECCN;
			public const string CD_ITARExemptionNo = CusUSClassificationSchema.Constants.CD_ITARExemptionNo;
			public const string CD_MilitaryEquipInd = CusUSClassificationSchema.Constants.CD_MilitaryEquipInd;
			public const string CD_DDTCUSMLCategoryCode = CusUSClassificationSchema.Constants.CD_DDTCUSMLCategoryCode;
			public const string CD_PartyCertInd = CusUSClassificationSchema.Constants.CD_PartyCertInd;
			public const string CD_DDTCUnit = CusUSClassificationSchema.Constants.CD_DDTCUnit;
			public const string CD_LicenceType = CusUSClassificationSchema.Constants.CD_LicenceType;
			public const string CD_DDTCRegoNo = CusUSClassificationSchema.Constants.CD_DDTCRegoNo;
			public const string CD_DDTCLicenceNo = CusUSClassificationSchema.Constants.CD_DDTCLicenceNo;
			public const string CD_DDTCJurisdictionNumber = CusUSClassificationSchema.Constants.CD_DDTCJurisdictionNumber;
			public const string CD_DDTCLicenceType = CusUSClassificationSchema.Constants.CD_DDTCLicenceType;
			public const string CD_GrossWeight = CusUSClassificationSchema.Constants.CD_GrossWeight;
			public const string CD_NetWeight = CusUSClassificationSchema.Constants.CD_NetWeight;
			public const string CD_WeightUQ = CusUSClassificationSchema.Constants.CD_WeightUQ;
			public const string CD_LicenceNo = "CD_LicenceNo";
			public const string CD_CottonCertificateApply = CusUSClassificationSchema.Constants.CD_CottonCertificateApply;
			public const string CD_TSCAODSCertIndividual = CusUSClassificationSchema.Constants.CD_TSCAODSCertIndividual;
			public const string CD_TSCAClaimIndicator = CusUSClassificationSchema.Constants.CD_TSCAClaimIndicator;
			public const string CD_TSCADisclaimReason = CusUSClassificationSchema.Constants.CD_TSCADisclaimReason;
			public const string CD_TTBRateDesignationCode = CusUSClassificationSchema.Constants.CD_TTBRateDesignationCode;
			public const string CD_ODSIndicator = CusUSClassificationSchema.Constants.CD_ODSIndicator;
			public const string CD_ODSDisclaimReason = CusUSClassificationSchema.Constants.CD_ODSDisclaimReason;
			public const string CD_PSTIndicator = CusUSClassificationSchema.Constants.CD_PSTIndicator;
			public const string CD_PSTDisclaimReason = CusUSClassificationSchema.Constants.CD_PSTDisclaimReason;
			public const string CD_PSTDisclaimProgram = CusUSClassificationSchema.Constants.CD_PSTDisclaimProgram;
			public const string CD_HFCIndicator = CusUSClassificationSchema.Constants.CD_HFCIndicator;
			public const string CD_HFCDisclaimReason = CusUSClassificationSchema.Constants.CD_HFCDisclaimReason;
			public const string CD_VNEIndicator = CusUSClassificationSchema.Constants.CD_VNEIndicator;
			public const string CD_VNEDisclaimReason = CusUSClassificationSchema.Constants.CD_VNEDisclaimReason;
			public const string CD_OMCIndicator = CusUSClassificationSchema.Constants.CD_OMCIndicator;
			public const string CD_OMCDisclaimReason = CusUSClassificationSchema.Constants.CD_OMCDisclaimReason;
			public const string CD_NOPIndicator = CusUSClassificationSchema.Constants.CD_NOPIndicator;
			public const string CD_NOPDisclaimReason = CusUSClassificationSchema.Constants.CD_NOPDisclaimReason;
			public const string CD_FlavorContentCreditIndicator = CusUSClassificationSchema.Constants.CD_FlavorContentCreditIndicator;
			public const string CD_ADDDepositRateOverride = CusUSClassificationSchema.Constants.CD_ADDDepositRateOverride;
			public const string CD_CVDDepositRateOverride = CusUSClassificationSchema.Constants.CD_CVDDepositRateOverride;
			public const string CD_ADDDepositRateDescription = "CD_ADDDepositRateDescription";
			public const string CD_CVDDepositRateDescription = "CD_CVDDepositRateDescription";
			public const string CD_APHISIndicator = CusUSClassificationSchema.Constants.CD_APHISIndicator;
			public const string CD_APHISDisclaimReason = CusUSClassificationSchema.Constants.CD_APHISDisclaimReason;
			public const string CD_ACEFDAIndicator = CusUSClassificationSchema.Constants.CD_ACEFDAIndicator;
			public const string CD_ACEFDADisclaimReason = CusUSClassificationSchema.Constants.CD_ACEFDADisclaimReason;
			public const string CD_FSISIndicator = CusUSClassificationSchema.Constants.CD_FSISIndicator;
			public const string CD_FSISDisclaimReason = CusUSClassificationSchema.Constants.CD_FSISDisclaimReason;
			public const string CD_FWSIndicator = CusUSClassificationSchema.Constants.CD_FWSIndicator;
			public const string CD_FWSDisclaimReason = CusUSClassificationSchema.Constants.CD_FWSDisclaimReason;
			public const string CD_LaceyActIndicator = CusUSClassificationSchema.Constants.CD_LaceyActIndicator;
			public const string CD_LaceyActDisclaimReason = CusUSClassificationSchema.Constants.CD_LaceyActDisclaimReason;
			public const string CD_NMFS370Indicator = CusUSClassificationSchema.Constants.CD_NMFS370Indicator;
			public const string CD_NMFS370DisclaimReason = CusUSClassificationSchema.Constants.CD_NMFS370DisclaimReason;
			public const string CD_NMFSAMRIndicator = CusUSClassificationSchema.Constants.CD_NMFSAMRIndicator;
			public const string CD_NMFSAMRDisclaimReason = CusUSClassificationSchema.Constants.CD_NMFSAMRDisclaimReason;
			public const string CD_DDTCIndicator = CusUSClassificationSchema.Constants.CD_DDTCIndicator;
			public const string CD_NMFSCOAIndicator = CusUSClassificationSchema.Constants.CD_NMFSCOAIndicator;
			public const string CD_NMFSHMSIndicator = CusUSClassificationSchema.Constants.CD_NMFSHMSIndicator;
			public const string CD_NMFSHMSDisclaimReason = CusUSClassificationSchema.Constants.CD_NMFSHMSDisclaimReason;
			public const string CD_NMFSSIMPIndicator = CusUSClassificationSchema.Constants.CD_NMFSSIMPIndicator;
			public const string CD_TTBIndicator = CusUSClassificationSchema.Constants.CD_TTBIndicator;
			public const string CD_TTBDisclaimReason = CusUSClassificationSchema.Constants.CD_TTBDisclaimReason;
			public const string CD_AMSIndicator = CusUSClassificationSchema.Constants.CD_AMSIndicator;
			public const string CD_AMSDisclaimReason = CusUSClassificationSchema.Constants.CD_AMSDisclaimReason;
			public const string CD_AMSDisclaimProgram = CusUSClassificationSchema.Constants.CD_AMSDisclaimProgram;
			public const string CD_NHTSAIndicator = CusUSClassificationSchema.Constants.CD_NHTSAIndicator;
			public const string CD_NHTSADisclaimReason = CusUSClassificationSchema.Constants.CD_NHTSADisclaimReason;
			public const string CD_ATFIndicator = CusUSClassificationSchema.Constants.CD_ATFIndicator;
			public const string CD_CPSCIndicator = CusUSClassificationSchema.Constants.CD_CPSCIndicator;
			public const string CD_CPSCDisclaimReason = CusUSClassificationSchema.Constants.CD_CPSCDisclaimReason;
			public const string CD_DEAIndicator = CusUSClassificationSchema.Constants.CD_DEAIndicator;
			public const string CD_DEADisclaimReason = CusUSClassificationSchema.Constants.CD_DEADisclaimReason;
			public const string CD_EPAConsentNumber = CusUSClassificationSchema.Constants.CD_EPAConsentNumber;
			public const string CD_EPANetQty = CusUSClassificationSchema.Constants.CD_EPANetQty;
			public const string CD_EPANetQtyUQ = CusUSClassificationSchema.Constants.CD_EPANetQtyUQ;
			public const string CD_ExportCertificateNo = CusUSClassificationSchema.Constants.CD_ExportCertificateNo;
			public const string CD_HazWasteTrackingNo = CusUSClassificationSchema.Constants.CD_HazWasteTrackingNo;
			public const string CD_PrimaryCountryNA = CusUSClassificationSchema.Constants.CD_PrimaryCountryNA;
			public const string CD_RN_NKPrimaryCountry = CusUSClassificationSchema.Constants.CD_RN_NKPrimaryCountry;
			public const string CD_SecondaryCountryNA = CusUSClassificationSchema.Constants.CD_SecondaryCountryNA;
			public const string CD_RN_NKSecondaryCountry = CusUSClassificationSchema.Constants.CD_RN_NKSecondaryCountry;
			public const string CD_RN_NKCastCountry = CusUSClassificationSchema.Constants.CD_RN_NKCastCountry;
			public const string CD_RN_NKCertificateOrigin = CusUSClassificationSchema.Constants.CD_RN_NKCertificateOrigin;
			public const string CD_RN_NKMeltCountry = CusUSClassificationSchema.Constants.CD_RN_NKMeltCountry;
		}
		#endregion

		#region CusUSClassification wrapper Properties

		#region CD_ADDApplicable

		public virtual ZBool CD_ADDApplicable
		{
			get { return Details.CD_ADDApplicable; }
			set { Details.CD_ADDApplicable = value; }
		}

		public virtual ZPropertyInfo CD_ADDApplicableInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ADDApplicable, x => Details.CD_ADDApplicableInfo); }
		}

		#endregion

		#region CD_ADDDepositRateInd

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.AntidumpingDutyDepositRates))]
		public virtual ZString CD_ADDDepositRateInd
		{
			get { return Details.CD_ADDDepositRateInd; }
			set { Details.CD_ADDDepositRateInd = value; }
		}

		public virtual ZPropertyInfo CD_ADDDepositRateIndInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ADDDepositRateInd, x => Details.CD_ADDDepositRateIndInfo); }
		}

		#endregion

		#region CD_ADDDepositRateOverride

		public virtual ZDecimal CD_ADDDepositRateOverride
		{
			get { return Details.CD_ADDDepositRateOverride; }
			set { Details.CD_ADDDepositRateOverride = value; }
		}

		public virtual ZPropertyInfo CD_ADDDepositRateOverrideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ADDDepositRateOverride, x => Details.CD_ADDDepositRateOverrideInfo); }
		}

		#endregion

		#region CD_ADDCaseNo

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ADDCaseNumberList))]
		public virtual ZString CD_ADDCaseNo
		{
			get { return Details.CD_ADDCaseNo; }
			set { Details.CD_ADDCaseNo = value; }
		}

		public virtual ZPropertyInfo CD_ADDCaseNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ADDCaseNo, x => Details.CD_ADDCaseNoInfo); }
		}

		#endregion

		#region CD_ADCVDStat

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_ADDCVDNonReimbursementList))]
		public virtual ZString CD_ADCVDStat
		{
			get { return Details.CD_ADCVDStat; }
			set { Details.CD_ADCVDStat = value; }
		}

		public virtual ZPropertyInfo CD_ADCVDStatInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ADCVDStat, x => Details.CD_ADCVDStatInfo); }
		}

		#endregion

		#region CD_ADDDecID

		public virtual ZString CD_ADDDecID
		{
			get { return Details.CD_ADDDecID; }
			set { Details.CD_ADDDecID = value; }
		}

		public virtual ZPropertyInfo CD_ADDDecIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ADDDecID, x => Details.CD_ADDDecIDInfo); }
		}

		#endregion

		#region CD_CVDApplicable

		public virtual ZBool CD_CVDApplicable
		{
			get { return Details.CD_CVDApplicable; }
			set { Details.CD_CVDApplicable = value; }
		}

		public virtual ZPropertyInfo CD_CVDApplicableInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CVDApplicable, x => Details.CD_CVDApplicableInfo); }
		}

		#endregion

		#region CD_CVDDepositRateInd

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CountervailingDutyDepositRates))]
		public virtual ZString CD_CVDDepositRateInd
		{
			get { return Details.CD_CVDDepositRateInd; }
			set { Details.CD_CVDDepositRateInd = value; }
		}

		public virtual ZPropertyInfo CD_CVDDepositRateIndInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CVDDepositRateInd, x => Details.CD_CVDDepositRateIndInfo); }
		}

		#endregion

		#region CD_CVDDepositRateOverride

		public virtual ZDecimal CD_CVDDepositRateOverride
		{
			get { return Details.CD_CVDDepositRateOverride; }
			set { Details.CD_CVDDepositRateOverride = value; }
		}

		public virtual ZPropertyInfo CD_CVDDepositRateOverrideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CVDDepositRateOverride, x => Details.CD_CVDDepositRateOverrideInfo); }
		}

		#endregion

		#region CD_CVDCaseNo

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.CVDCaseNumberList))]
		public virtual ZString CD_CVDCaseNo
		{
			get { return Details.CD_CVDCaseNo; }
			set { Details.CD_CVDCaseNo = value; }
		}

		public virtual ZPropertyInfo CD_CVDCaseNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CVDCaseNo, x => Details.CD_CVDCaseNoInfo); }
		}

		#endregion

		#region CD_CVDDecID

		public virtual ZString CD_CVDDecID
		{
			get { return Details.CD_CVDDecID; }
			set { Details.CD_CVDDecID = value; }
		}

		public virtual ZPropertyInfo CD_CVDDecIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CVDDecID, x => Details.CD_CVDDecIDInfo); }
		}

		#endregion

		#region CD_ProductClaim

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.ProductClaimList))]
		public virtual ZString CD_ProductClaim
		{
			get { return Details.CD_ProductClaim; }
			set { Details.CD_ProductClaim = value; }
		}

		public virtual ZPropertyInfo CD_ProductClaimInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ProductClaim, x => Details.CD_ProductClaimInfo); }
		}

		#endregion

		#region CD_SPI

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.SPIList))]
		public virtual ZString CD_SPI
		{
			get { return Details.CD_SPI; }
			set { Details.CD_SPI = value; }
		}

		public virtual ZPropertyInfo CD_SPIInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_SPI, x => Details.CD_SPIInfo); }
		}

		#endregion

		#region CD_CBTPACertificate

		public virtual ZString CD_CBTPACertificate
		{
			get { return Details.CD_CBTPACertificate; }
			set { Details.CD_CBTPACertificate = value; }
		}

		public virtual ZPropertyInfo CD_CBTPACertificateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CBTPACertificate, x => Details.CD_CBTPACertificateInfo); }
		}

		#endregion

		#region CD_CottonCertificate

		[ReadOnlyMember(nameof(CD_CottonCertificate_ReadOnly))]
		public virtual ZString CD_CottonCertificate
		{
			get { return Details.CD_CottonCertificate; }
			set { Details.CD_CottonCertificate = value; }
		}

		public virtual ZPropertyInfo CD_CottonCertificateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CottonCertificate, x => Details.CD_CottonCertificateInfo); }
		}

		#endregion

		#region CD_CottonFeeExempt

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.US_YesNoList))]
		public virtual ZString CD_CottonFeeExempt
		{
			get { return Details.CD_CottonFeeExempt; }
			set { Details.CD_CottonFeeExempt = value; }
		}

		public virtual ZPropertyInfo CD_CottonFeeExemptInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CottonFeeExempt, x => Details.CD_CottonFeeExemptInfo); }
		}

		#endregion

		#region CD_CottonCertApplies

		public virtual bool CD_CottonCertificateApply
		{
			get { return Details.CD_CottonCertificateApply; }
			set { Details.CD_CottonCertificateApply = value; }
		}

		public virtual ZPropertyInfo CD_CottonCertAppliesInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CottonCertificateApply, x => Details.CD_CottonCertificateApplyInfo); }
		}
		#endregion

		#region CD_MiscLicenceNo

		public virtual ZString CD_MiscLicenceNo
		{
			get { return Details.CD_MiscLicenceNo; }
			set { Details.CD_MiscLicenceNo = value; }
		}

		public virtual ZPropertyInfo CD_MiscLicenceNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_MiscLicenceNo, x => Details.CD_MiscLicenceNoInfo); }
		}

		#endregion

		#region CD_RulingNumber

		public virtual ZString CD_RulingNumber
		{
			get { return Details.CD_RulingNumber; }
			set { Details.CD_RulingNumber = value; }
		}

		public virtual ZPropertyInfo CD_RulingNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RulingNumber, x => Details.CD_RulingNumberInfo); }
		}

		#endregion

		#region CD_RulingType

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_RulingTypeList))]
		public virtual ZString CD_RulingType
		{
			get { return Details.CD_RulingType; }
			set { Details.CD_RulingType = value; }
		}

		public virtual ZPropertyInfo CD_RulingTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RulingType, x => Details.CD_RulingTypeInfo); }
		}

		#endregion

		#region CD_WoolLicenceNo

		public virtual ZString CD_WoolLicenceNo
		{
			get { return Details.CD_WoolLicenceNo; }
			set { Details.CD_WoolLicenceNo = value; }
		}

		public virtual ZPropertyInfo CD_WoolLicenceNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_WoolLicenceNo, x => Details.CD_WoolLicenceNoInfo); }
		}

		#endregion

		#region CD_ADDBonded

		public virtual ZBool CD_ADDBonded
		{
			get { return Details.CD_ADDBonded; }
			set { Details.CD_ADDBonded = value; }
		}

		public virtual ZPropertyInfo CD_ADDBondedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ADDBonded, x => Details.CD_ADDBondedInfo); }
		}

		#endregion

		#region CD_CVDBonded

		public virtual ZBool CD_CVDBonded
		{
			get { return Details.CD_CVDBonded; }
			set { Details.CD_CVDBonded = value; }
		}

		public virtual ZPropertyInfo CD_CVDBondedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CVDBonded, x => Details.CD_CVDBondedInfo); }
		}

		#endregion

		#region CD_NAFTANetCost

		public virtual ZBool CD_NAFTANetCost
		{
			get { return Details.CD_NAFTANetCost; }
			set { Details.CD_NAFTANetCost = value; }
		}

		public virtual ZPropertyInfo CD_NAFTANetCostInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NAFTANetCost, x => Details.CD_NAFTANetCostInfo); }
		}

		#endregion

		#region CD_SugarCertificate

		public virtual ZString CD_SugarCertificate
		{
			get { return Details.CD_SugarCertificate; }
			set { Details.CD_SugarCertificate = value; }
		}

		public virtual ZPropertyInfo CD_SugarCertificateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_SugarCertificate, x => Details.CD_SugarCertificateInfo); }
		}

		#endregion

		#region CD_AgricultureLicenceNo

		public virtual ZString CD_AgricultureLicenceNo
		{
			get { return Details.CD_AgricultureLicenceNo; }
			set { Details.CD_AgricultureLicenceNo = value; }
		}

		public virtual ZPropertyInfo CD_AgricultureLicenceNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_AgricultureLicenceNo, x => Details.CD_AgricultureLicenceNoInfo); }
		}

		#endregion

		#region CD_ZoneStatus

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ZoneStatuses))]
		public virtual ZString CD_ZoneStatus
		{
			get { return Details.CD_ZoneStatus; }
			set { Details.CD_ZoneStatus = value; }
		}

		public virtual ZPropertyInfo CD_ZoneStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ZoneStatus, x => Details.CD_ZoneStatusInfo); }
		}

		#endregion

		#region CD_NAFTARecon

		public virtual ZBool CD_NAFTARecon
		{
			get { return Details.CD_NAFTARecon; }
			set { Details.CD_NAFTARecon = value; }
		}

		public virtual ZPropertyInfo CD_NAFTAReconInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NAFTARecon, x => Details.CD_NAFTAReconInfo); }
		}

		#endregion

		#region CD_TSCAIndicator

		public virtual ZString CD_TSCAIndicator
		{
			get { return Details.CD_TSCAIndicator; }
			set { Details.CD_TSCAIndicator = value; }
		}

		public virtual ZPropertyInfo CD_TSCAIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TSCAIndicator, x => Details.CD_TSCAIndicatorInfo); }
		}

		#endregion

		#region CD_TSCAODSCertIndividual
		public virtual ZString CD_TSCAODSCertIndividual
		{
			get { return Details.CD_TSCAODSCertIndividual; }
			set { Details.CD_TSCAODSCertIndividual = value; }
		}

		public virtual ZPropertyInfo CD_TSCAODSCertIndividualInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TSCAODSCertIndividual, x => Details.CD_TSCAODSCertIndividualInfo); }
		}
		#endregion

		#region CD_ActiveIngredientPercentage

		public virtual ZDecimal CD_ActiveIngredientPercentage
		{
			get { return Details.CD_ActiveIngredientPercentage; }
			set { Details.CD_ActiveIngredientPercentage = value; }
		}

		public virtual ZPropertyInfo CD_ActiveIngredientPercentageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ActiveIngredientPercentage, x => Details.CD_ActiveIngredientPercentageInfo); }
		}

		#endregion

		#region CD_OA_Manufacturer

		[RelatedBusinessObject("ManufacturerAddress")]
		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.Manufacturers))]
		public virtual ZGuid CD_OA_Manufacturer
		{
			get { return Details.CD_OA_Manufacturer; }
			set { Details.CD_OA_Manufacturer = value; }
		}

		public virtual ZPropertyInfo CD_OA_ManufacturerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_OA_Manufacturer, x => Details.CD_OA_ManufacturerInfo); }
		}

		public virtual OrgAddress ManufacturerAddress
		{
			get { return Factory.Load<OrgAddress>(CD_OA_Manufacturer); }
		}

		#endregion

		#region CD_OA_Exporter

		[RelatedBusinessObject("ExporterAddress")]
		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.Exporters))]
		public virtual ZGuid CD_OA_Exporter
		{
			get { return Details.CD_OA_Exporter; }
			set { Details.CD_OA_Exporter = value; }
		}

		public virtual ZPropertyInfo CD_OA_ExporterInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_OA_Exporter, x => Details.CD_OA_ExporterInfo); }
		}

		public virtual OrgAddress ExporterAddress
		{
			get { return Factory.Load<OrgAddress>(CD_OA_Exporter); }
		}

		#endregion

		#region CD_ReconIssue

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.OtherReconIssueList))]
		public virtual ZString CD_ReconIssue
		{
			get { return Details.CD_ReconIssue; }
			set { Details.CD_ReconIssue = value; }
		}

		public virtual ZPropertyInfo CD_ReconIssueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ReconIssue, x => Details.CD_ReconIssueInfo); }
		}

		#endregion

		#region CD_UC_NKCountryOfExport

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.USCountryList))]
		public virtual ZString CD_UC_NKCountryOfExport
		{
			get { return Details.CD_UC_NKCountryOfExport; }
			set { Details.CD_UC_NKCountryOfExport = value; }
		}

		public virtual ZPropertyInfo CD_UC_NKCountryOfExportInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_UC_NKCountryOfExport, x => Details.CD_UC_NKCountryOfExportInfo); }
		}

		#endregion

		#region CD_UC_NKCountryOfOrigin

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.USCountryList))]
		public virtual ZString CD_UC_NKCountryOfOrigin
		{
			get { return Details.CD_UC_NKCountryOfOrigin; }
			set { Details.CD_UC_NKCountryOfOrigin = value; }
		}

		public virtual ZPropertyInfo CD_UC_NKCountryOfOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_UC_NKCountryOfOrigin, x => Details.CD_UC_NKCountryOfOriginInfo); }
		}

		#endregion

		#region CD_FlavorContentCreditIndicator

		public virtual ZBool CD_FlavorContentCreditIndicator
		{
			get
			{
				return Details.CD_FlavorContentCreditIndicator;
			}
			set
			{
				Details.CD_FlavorContentCreditIndicator = value;
			}
		}

		#endregion

		#region CD_TaxApplicability

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.TaxApplyList))]
		public virtual ZString CD_TaxApplicability
		{
			get { return Details.CD_TaxApplicability; }
			set { Details.CD_TaxApplicability = value; }
		}

		public virtual ZPropertyInfo CD_TaxApplicabilityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TaxApplicability, x => Details.CD_TaxApplicabilityInfo); }
		}

		#endregion

		#region CD_TaxCode

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.TaxCodeList))]
		public virtual ZString CD_TaxCode
		{
			get { return Details.CD_TaxCode; }
			set { Details.CD_TaxCode = value; }
		}

		public virtual ZPropertyInfo CD_TaxCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TaxCode, x => Details.CD_TaxCodeInfo); }
		}

		#endregion

		#region CD_TaxRate

		public virtual ZDecimal CD_TaxRate
		{
			get { return Details.CD_TaxRate; }
			set { Details.CD_TaxRate = value; }
		}

		public virtual ZPropertyInfo CD_TaxRateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TaxRate, x => Details.CD_TaxRateInfo); }
		}

		#endregion

		#region CD_CBMADefaultTaxRate

		public virtual ZDecimal CD_CBMADefaultTaxRate
		{
			get { return Details.CD_CBMADefaultTaxRate; }
			set { Details.CD_CBMADefaultTaxRate = value; }
		}

		public virtual ZPropertyInfo CD_CBMADefaultTaxRateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CBMADefaultTaxRate, x => Details.CD_CBMADefaultTaxRateInfo); }
		}

		#endregion

		#region CD_TaxRateDesc

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.TaxRateList))]
		public virtual ZString CD_TaxRateDesc
		{
			get { return Details.CD_TaxRateDesc; }
			set { Details.CD_TaxRateDesc = value; }
		}

		public virtual ZPropertyInfo CD_TaxRateDescInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TaxRateDesc, x => Details.CD_TaxRateDescInfo); }
		}

		#endregion

		#region CD_TTBRateDesignationCode

		[List(nameof(USClassificationLookups) + "." + nameof(CBMATaxRateList))]
		public virtual ZString CD_TTBRateDesignationCode
		{
			get { return Details.CD_TTBRateDesignationCode; }
			set { Details.CD_TTBRateDesignationCode = value; }
		}

		public virtual ZPropertyInfo CD_TTBRateDesignationCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TTBRateDesignationCode, x => Details.CD_TTBRateDesignationCodeInfo); }
		}

		#endregion

		#region CD_TaxRateType

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_SelectedRateTypeList))]
		public virtual ZString CD_TaxRateType
		{
			get { return Details.CD_TaxRateType; }
			set { Details.CD_TaxRateType = value; }
		}

		public virtual ZPropertyInfo CD_TaxRateTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TaxRateType, x => Details.CD_TaxRateTypeInfo); }
		}

		#endregion

		#region CD_AMMVPerUnit

		[DecimalPlaces(4)]
		public virtual ZDecimal CD_AMMVPerUnit
		{
			get { return Details.CD_AMMVPerUnit; }
			set { Details.CD_AMMVPerUnit = value; }
		}

		public virtual ZPropertyInfo CD_AMMVPerUnitInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_AMMVPerUnit, x => Details.CD_AMMVPerUnitInfo); }
		}

		#endregion

		#region CD_AMMVPerUnitCurrency

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.Currencies))]
		public virtual ZString CD_AMMVPerUnitCurrency
		{
			get { return Details.CD_AMMVPerUnitCurrency; }
			set { Details.CD_AMMVPerUnitCurrency = value; }
		}

		public virtual ZPropertyInfo CD_AMMVPerUnitCurrencyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_AMMVPerUnitCurrency, x => Details.CD_AMMVPerUnitCurrencyInfo); }
		}

		#endregion

		#region CD_AMMVPercentage
		public virtual ZDecimal CD_AMMVPercentage
		{
			get { return Details.CD_AMMVPercentage; }
			set { Details.CD_AMMVPercentage = value; }
		}

		public virtual ZPropertyInfo CD_AMMVPercentageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_AMMVPercentage, x => Details.CD_AMMVPercentageInfo); }
		}

		#endregion

		#region CD_9802USDValuePerUnit

		[DecimalPlaces(4)]
		public virtual ZDecimal CD_9802USDValuePerUnit
		{
			get { return Details.CD_9802USDValuePerUnit; }
			set { Details.CD_9802USDValuePerUnit = value; }
		}

		public virtual ZPropertyInfo CD_9802USDValuePerUnitInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_9802USDValuePerUnit, x => Details.CD_9802USDValuePerUnitInfo); }
		}

		#endregion

		#region CD_9802ValuePerUnit

		[DecimalPlaces(4)]
		public virtual ZDecimal CD_9802ValuePerUnit
		{
			get { return Details.CD_9802ValuePerUnit; }
			set { Details.CD_9802ValuePerUnit = value; }
		}

		public virtual ZPropertyInfo CD_9802ValuePerUnitInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_9802ValuePerUnit, x => Details.CD_9802ValuePerUnitInfo); }
		}

		#endregion

		#region CD_RX_NK9802ValuePerUnitCurr

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.Currencies))]
		public virtual ZString CD_RX_NK9802ValuePerUnitCurr
		{
			get { return Details.CD_RX_NK9802ValuePerUnitCurr; }
			set { Details.CD_RX_NK9802ValuePerUnitCurr = value; }
		}

		public virtual ZPropertyInfo CD_RX_NK9802ValuePerUnitCurrInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RX_NK9802ValuePerUnitCurr, x => Details.CD_RX_NK9802ValuePerUnitCurrInfo); }
		}

		#endregion

		#region CD_PerUnitCost

		[DecimalPlaces(4)]
		public virtual ZDecimal CD_PerUnitCost
		{
			get { return Details.CD_PerUnitCost; }
			set { Details.CD_PerUnitCost = value; }
		}

		public virtual ZPropertyInfo CD_PerUnitCostInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_PerUnitCost, x => Details.CD_PerUnitCostInfo); }
		}

		#endregion

		#region CD_RX_NKPerUnitCostCurr

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.Currencies))]
		public virtual ZString CD_RX_NKPerUnitCostCurr
		{
			get { return Details.CD_RX_NKPerUnitCostCurr; }
			set { Details.CD_RX_NKPerUnitCostCurr = value; }
		}

		public virtual ZPropertyInfo CD_RX_NKPerUnitCostCurrInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RX_NKPerUnitCostCurr, x => Details.CD_RX_NKPerUnitCostCurrInfo); }
		}

		#endregion

		#region CD_GrossWeight

		public virtual ZDecimal CD_GrossWeight
		{
			get { return Details.CD_GrossWeight; }
			set { Details.CD_GrossWeight = value; }
		}

		public virtual ZPropertyInfo CD_GrossWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_GrossWeight, x => Details.CD_GrossWeightInfo); }
		}

		#endregion

		#region CD_WeightUQ

		public virtual ZString CD_WeightUQ
		{
			get { return Details.CD_WeightUQ; }
			set { Details.CD_WeightUQ = value; }
		}

		public virtual ZPropertyInfo CD_WeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_WeightUQ, x => Details.CD_WeightUQInfo); }
		}

		#endregion

		#region CD_NetWeight

		public virtual ZDecimal CD_NetWeight
		{
			get { return Details.CD_NetWeight; }
			set { Details.CD_NetWeight = value; }
		}

		public virtual ZPropertyInfo CD_NetWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NetWeight, x => Details.CD_NetWeightInfo); }
		}

		#endregion

		#region CD_OriginIndicator

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_OriginIndicatorList))]
		public virtual ZString CD_OriginIndicator
		{
			get { return Details.CD_OriginIndicator; }
			set { Details.CD_OriginIndicator = value; }
		}

		public virtual ZPropertyInfo CD_OriginIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_OriginIndicator, x => Details.CD_OriginIndicatorInfo); }
		}

		#endregion

		#region CD_ECCN

		public virtual ZString CD_ECCN
		{
			get { return Details.CD_ECCN; }
			set { Details.CD_ECCN = value; }
		}

		public virtual ZPropertyInfo CD_ECCNInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ECCN, x => Details.CD_ECCNInfo); }
		}

		#endregion

		#region CD_ExportCode

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_ExportCode_List))]
		public virtual ZString CD_ExportCode
		{
			get { return Details.CD_ExportCode; }
			set { Details.CD_ExportCode = value; }
		}

		public virtual ZPropertyInfo CD_ExportCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ExportCode, x => Details.CD_ExportCodeInfo); }
		}

		#endregion

		#region CD_LicenceType

		public virtual ZString CD_LicenceNo
		{
			get { return Details.CD_MiscLicenceNo; }
			set { Details.CD_MiscLicenceNo = value; }
		}

		public virtual ZPropertyInfo CD_LicenceNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_LicenceNo, x => Details.CD_MiscLicenceNoInfo); }
		}

		#endregion

		#region CD_LicenceType

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.USAESLicenseCodes))]
		public virtual ZString CD_LicenceType
		{
			get { return Details.CD_LicenceType; }
			set { Details.CD_LicenceType = value; }
		}

		public virtual ZPropertyInfo CD_LicenceTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_LicenceType, x => Details.CD_LicenceTypeInfo); }
		}

		#endregion

		#region CD_ITARExemptionNo

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_ITARExemptionNoCodes))]
		public virtual ZString CD_ITARExemptionNo
		{
			get { return Details.CD_ITARExemptionNo; }
			set { Details.CD_ITARExemptionNo = value; }
		}

		public virtual ZPropertyInfo CD_ITARExemptionNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ITARExemptionNo, x => Details.CD_ITARExemptionNoInfo); }
		}

		#endregion

		#region CD_MilitaryEquipInd

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.US_YesNoList))]
		public virtual ZString CD_MilitaryEquipInd
		{
			get { return Details.CD_MilitaryEquipInd; }
			set { Details.CD_MilitaryEquipInd = value; }
		}

		public virtual ZPropertyInfo CD_MilitaryEquipIndInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_MilitaryEquipInd, x => Details.CD_MilitaryEquipIndInfo); }
		}

		#endregion

		#region CD_PartyCertInd

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.US_YesNoList))]
		public virtual ZString CD_PartyCertInd
		{
			get { return Details.CD_PartyCertInd; }
			set { Details.CD_PartyCertInd = value; }
		}

		public virtual ZPropertyInfo CD_PartyCertIndInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_PartyCertInd, x => Details.CD_PartyCertIndInfo); }
		}

		#endregion

		#region CD_DDTCLicenceNo

		public virtual ZString CD_DDTCLicenceNo
		{
			get { return Details.CD_DDTCLicenceNo; }
			set { Details.CD_DDTCLicenceNo = value; }
		}

		public virtual ZPropertyInfo CD_DDTCLicenceNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DDTCLicenceNo, x => Details.CD_DDTCLicenceNoInfo); }
		}

		#endregion

		#region CD_DDTCJurisdictionNumber
		public virtual ZString CD_DDTCJurisdictionNumber
		{
			get { return Details.CD_DDTCJurisdictionNumber; }
			set { Details.CD_DDTCJurisdictionNumber = value; }
		}

		public virtual ZPropertyInfo CD_DDTCJurisdictionNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DDTCJurisdictionNumber, x => Details.CD_DDTCJurisdictionNumberInfo); }
		}

		#endregion

		#region CD_DDTCLicenceType

		public virtual ZString CD_DDTCLicenceType
		{
			get { return Details.CD_DDTCLicenceType; }
			set { Details.CD_DDTCLicenceType = value; }
		}

		public virtual ZPropertyInfo CD_DDTCLicenceTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DDTCLicenceType, x => Details.CD_DDTCLicenceTypeInfo); }
		}

		#endregion

		#region CD_DDTCRegoNo

		public virtual ZString CD_DDTCRegoNo
		{
			get { return Details.CD_DDTCRegoNo; }
			set { Details.CD_DDTCRegoNo = value; }
		}

		public virtual ZPropertyInfo CD_DDTCRegoNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DDTCRegoNo, x => Details.CD_DDTCRegoNoInfo); }
		}

		#endregion

		#region CD_DDTCUSMLCategoryCode

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_DDTCUSMLCategoryCodes))]
		public virtual ZString CD_DDTCUSMLCategoryCode
		{
			get { return Details.CD_DDTCUSMLCategoryCode; }
			set { Details.CD_DDTCUSMLCategoryCode = value; }
		}

		public virtual ZPropertyInfo CD_DDTCUSMLCategoryCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DDTCUSMLCategoryCode, x => Details.CD_DDTCUSMLCategoryCodeInfo); }
		}

		#endregion

		#region CD_DDTCUnit

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_DDTCUnitOfMeasureList))]
		public virtual ZString CD_DDTCUnit
		{
			get { return Details.CD_DDTCUnit; }
			set { Details.CD_DDTCUnit = value; }
		}

		public virtual ZPropertyInfo CD_DDTCUnitInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DDTCUnit, x => Details.CD_DDTCUnitInfo); }
		}

		#endregion

		#region CD_APHISIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_APHISIndicator
		{
			get { return Details.CD_APHISIndicator; }
			set { Details.CD_APHISIndicator = value; }
		}

		public virtual ZPropertyInfo CD_APHISIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_APHISIndicator, x => Details.CD_APHISIndicatorInfo); }
		}

		#endregion

		#region CD_APHISDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_APHISDisclaimReason
		{
			get { return Details.CD_APHISDisclaimReason; }
			set { Details.CD_APHISDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_APHISDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_APHISDisclaimReason, x => Details.CD_APHISDisclaimReasonInfo); }
		}

		#endregion

		#region CD_ACEFDAIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_ACEFDAIndicator
		{
			get { return Details.CD_ACEFDAIndicator; }
			set { Details.CD_ACEFDAIndicator = value; }
		}

		public virtual ZPropertyInfo CD_ACEFDAIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ACEFDAIndicator, x => Details.CD_ACEFDAIndicatorInfo); }
		}

		#endregion

		#region CD_ACEFDADisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_ACEFDADisclaimReason
		{
			get { return Details.CD_ACEFDADisclaimReason; }
			set { Details.CD_ACEFDADisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_ACEFDADisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ACEFDADisclaimReason, x => Details.CD_ACEFDADisclaimReasonInfo); }
		}

		#endregion

		#region CD_FSISIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_FSISIndicator
		{
			get { return Details.CD_FSISIndicator; }
			set { Details.CD_FSISIndicator = value; }
		}

		public virtual ZPropertyInfo CD_FSISIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_FSISIndicator, x => Details.CD_FSISIndicatorInfo); }
		}

		#endregion

		#region CD_FSISDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_FSISDisclaimReason
		{
			get { return Details.CD_FSISDisclaimReason; }
			set { Details.CD_FSISDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_FSISDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_FSISDisclaimReason, x => Details.CD_FSISDisclaimReasonInfo); }
		}

		#endregion

		#region CD_FWSIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_FWSIndicator
		{
			get { return Details.CD_FWSIndicator; }
			set { Details.CD_FWSIndicator = value; }
		}

		public virtual ZPropertyInfo CD_FWSIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_FWSIndicator, x => Details.CD_FWSIndicatorInfo); }
		}

		#endregion

		#region CD_FWSDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_FWSDisclaimReason
		{
			get { return Details.CD_FWSDisclaimReason; }
			set { Details.CD_FWSDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_FWSDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_FWSDisclaimReason, x => Details.CD_FWSDisclaimReasonInfo); }
		}

		#endregion

		#region CD_LaceyActIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_LaceyActIndicator
		{
			get { return Details.CD_LaceyActIndicator; }
			set { Details.CD_LaceyActIndicator = value; }
		}

		public virtual ZPropertyInfo CD_LaceyActIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_LaceyActIndicator, x => Details.CD_LaceyActIndicatorInfo); }
		}

		#endregion

		#region CD_LaceyActDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_LaceyActDisclaimReason
		{
			get { return Details.CD_LaceyActDisclaimReason; }
			set { Details.CD_LaceyActDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_LaceyActDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_LaceyActDisclaimReason, x => Details.CD_LaceyActDisclaimReasonInfo); }
		}

		#endregion

		#region CD_NMFS370Indicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_NMFS370Indicator
		{
			get { return Details.CD_NMFS370Indicator; }
			set { Details.CD_NMFS370Indicator = value; }
		}

		public virtual ZPropertyInfo CD_NMFS370IndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFS370Indicator, x => Details.CD_NMFS370IndicatorInfo); }
		}

		#endregion

		#region CD_NMFS370DisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_NMFS370DisclaimReason
		{
			get { return Details.CD_NMFS370DisclaimReason; }
			set { Details.CD_NMFS370DisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_NMFS370DisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFS370DisclaimReason, x => Details.CD_NMFS370DisclaimReasonInfo); }
		}

		#endregion

		#region CD_NMFSAMRIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_NMFSAMRIndicator
		{
			get { return Details.CD_NMFSAMRIndicator; }
			set { Details.CD_NMFSAMRIndicator = value; }
		}

		public virtual ZPropertyInfo CD_NMFSAMRIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFSAMRIndicator, x => Details.CD_NMFSAMRIndicatorInfo); }
		}

		#endregion

		#region CD_NMFSAMRDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_NMFSAMRDisclaimReason
		{
			get { return Details.CD_NMFSAMRDisclaimReason; }
			set { Details.CD_NMFSAMRDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_NMFSAMRDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFSAMRDisclaimReason, x => Details.CD_NMFSAMRDisclaimReasonInfo); }
		}

		#endregion

		#region CD_DDTCIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorWithoutDisclaimerList))]
		public virtual ZString CD_DDTCIndicator
		{
			get { return Details.CD_DDTCIndicator; }
			set { Details.CD_DDTCIndicator = value; }
		}

		public virtual ZPropertyInfo CD_DDTCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DDTCIndicator, x => Details.CD_DDTCIndicatorInfo); }
		}

		#endregion

		#region CD_NMFSCOAIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorWithoutDisclaimerList))]
		public virtual ZString CD_NMFSCOAIndicator
		{
			get { return Details.CD_NMFSCOAIndicator; }
			set { Details.CD_NMFSCOAIndicator = value; }
		}

		public virtual ZPropertyInfo CD_NMFSCOAIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFSCOAIndicator, x => Details.CD_NMFSCOAIndicatorInfo); }
		}

		#endregion

		#region CD_NMFSHMSIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_NMFSHMSIndicator
		{
			get { return Details.CD_NMFSHMSIndicator; }
			set { Details.CD_NMFSHMSIndicator = value; }
		}

		public virtual ZPropertyInfo CD_NMFSHMSIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFSHMSIndicator, x => Details.CD_NMFSHMSIndicatorInfo); }
		}

		#endregion

		#region CD_NMFSHMSDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_NMFSHMSDisclaimReason
		{
			get { return Details.CD_NMFSHMSDisclaimReason; }
			set { Details.CD_NMFSHMSDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_NMFSHMSDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFSHMSDisclaimReason, x => Details.CD_NMFSHMSDisclaimReasonInfo); }
		}

		#endregion

		#region CD_NMFSSIMPIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorWithoutDisclaimerList))]
		public virtual ZString CD_NMFSSIMPIndicator
		{
			get { return Details.CD_NMFSSIMPIndicator; }
			set { Details.CD_NMFSSIMPIndicator = value; }
		}

		public virtual ZPropertyInfo CD_NMFSSIMPIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NMFSSIMPIndicator, x => Details.CD_NMFSSIMPIndicatorInfo); }
		}

		#endregion

		#region CD_ODSIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_ODSIndicator
		{
			get { return Details.CD_ODSIndicator; }
			set { Details.CD_ODSIndicator = value; }
		}

		public virtual ZPropertyInfo CD_ODSIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ODSIndicator, x => Details.CD_ODSIndicatorInfo); }
		}

		#endregion

		#region CD_ODSDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_ODSDisclaimReason
		{
			get { return Details.CD_ODSDisclaimReason; }
			set { Details.CD_ODSDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_ODSDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ODSDisclaimReason, x => Details.CD_ODSDisclaimReasonInfo); }
		}

		#endregion

		#region CD_PSTIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_PSTIndicator
		{
			get { return Details.CD_PSTIndicator; }
			set { Details.CD_PSTIndicator = value; }
		}

		public virtual ZPropertyInfo CD_PSTIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_PSTIndicator, x => Details.CD_PSTIndicatorInfo); }
		}

		#endregion

		#region CD_PSTDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_PSTDisclaimReason
		{
			get { return Details.CD_PSTDisclaimReason; }
			set { Details.CD_PSTDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_PSTDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_PSTDisclaimReason, x => Details.CD_PSTDisclaimReasonInfo); }
		}

		#endregion

		#region CD_PSTDisclaimProgram

		public virtual ZString CD_PSTDisclaimProgram
		{
			get { return Details.CD_PSTDisclaimProgram; }
			set { Details.CD_PSTDisclaimProgram = value; }
		}

		public virtual ZPropertyInfo CD_PSTDisclaimProgramInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_PSTDisclaimProgram, x => Details.CD_PSTDisclaimProgramInfo); }
		}

		#endregion

		#region CD_HFCIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_HFCIndicator
		{
			get { return Details.CD_HFCIndicator; }
			set { Details.CD_HFCIndicator = value; }
		}

		public virtual ZPropertyInfo CD_HFCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_HFCIndicator, x => Details.CD_HFCIndicatorInfo); }
		}

		#endregion

		#region CD_HFCDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_HFCDisclaimReason
		{
			get { return Details.CD_HFCDisclaimReason; }
			set { Details.CD_HFCDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_HFCDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_HFCDisclaimReason, x => Details.CD_HFCDisclaimReasonInfo); }
		}

		#endregion

		#region CD_VNEIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_VNEIndicator
		{
			get { return Details.CD_VNEIndicator; }
			set { Details.CD_VNEIndicator = value; }
		}

		public virtual ZPropertyInfo CD_VNEIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_VNEIndicator, x => Details.CD_VNEIndicatorInfo); }
		}

		#endregion

		#region CD_VNEDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_VNEDisclaimReason
		{
			get { return Details.CD_VNEDisclaimReason; }
			set { Details.CD_VNEDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_VNEDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_VNEDisclaimReason, x => Details.CD_VNEDisclaimReasonInfo); }
		}

		#endregion

		#region CD_TSCAClaimIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_TSCAClaimIndicator
		{
			get { return Details.CD_TSCAClaimIndicator; }
			set { Details.CD_TSCAClaimIndicator = value; }
		}

		public virtual ZPropertyInfo CD_TSCAClaimIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TSCAClaimIndicator, x => Details.CD_TSCAClaimIndicatorInfo); }
		}

		#endregion

		#region CD_TSCADisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_TSCADisclaimReason
		{
			get { return Details.CD_TSCADisclaimReason; }
			set { Details.CD_TSCADisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_TSCADisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TSCADisclaimReason, x => Details.CD_TSCADisclaimReasonInfo); }
		}

		#endregion

		#region CD_TTBIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_TTBIndicator
		{
			get { return Details.CD_TTBIndicator; }
			set { Details.CD_TTBIndicator = value; }
		}

		public virtual ZPropertyInfo CD_TTBIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TTBIndicator, x => Details.CD_TTBIndicatorInfo); }
		}

		#endregion

		#region CD_TTBDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_TTBDisclaimReason
		{
			get { return Details.CD_TTBDisclaimReason; }
			set { Details.CD_TTBDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_TTBDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_TTBDisclaimReason, x => Details.CD_TTBDisclaimReasonInfo); }
		}

		#endregion

		#region CD_OMCIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_OMCIndicator
		{
			get { return Details.CD_OMCIndicator; }
			set { Details.CD_OMCIndicator = value; }
		}

		public virtual ZPropertyInfo CD_OMCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_OMCIndicator, x => Details.CD_OMCIndicatorInfo); }
		}

		#endregion

		#region CD_OMCDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_OMCDisclaimReason
		{
			get { return Details.CD_OMCDisclaimReason; }
			set { Details.CD_OMCDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_OMCDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_OMCDisclaimReason, x => Details.CD_OMCDisclaimReasonInfo); }
		}

		#endregion

		#region CD_AMSIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_AMSIndicator
		{
			get { return Details.CD_AMSIndicator; }
			set { Details.CD_AMSIndicator = value; }
		}

		public virtual ZPropertyInfo CD_AMSIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_AMSIndicator, x => Details.CD_AMSIndicatorInfo); }
		}

		#endregion

		#region CD_AMSDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_AMSDisclaimReason
		{
			get { return Details.CD_AMSDisclaimReason; }
			set { Details.CD_AMSDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_AMSDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_AMSDisclaimReason, x => Details.CD_AMSDisclaimReasonInfo); }
		}

		#endregion

		#region CD_AMSDisclaimProgram

		public virtual ZString CD_AMSDisclaimProgram
		{
			get { return Details.CD_AMSDisclaimProgram; }
			set { Details.CD_AMSDisclaimProgram = value; }
		}

		public virtual ZPropertyInfo CD_AMSDisclaimProgramInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_AMSDisclaimProgram, x => Details.CD_AMSDisclaimProgramInfo); }
		}

		#endregion

		#region CD_NOPIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_NOPIndicator
		{
			get { return Details.CD_NOPIndicator; }
			set { Details.CD_NOPIndicator = value; }
		}

		public virtual ZPropertyInfo CD_NOPIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NOPIndicator, x => Details.CD_NOPIndicatorInfo); }
		}

		#endregion

		#region CD_NOPDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_NOPDisclaimReason
		{
			get { return Details.CD_NOPDisclaimReason; }
			set { Details.CD_NOPDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_NOPDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NOPDisclaimReason, x => Details.CD_NOPDisclaimReasonInfo); }
		}

		#endregion

		#region CD_NHTSAIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_NHTSAIndicator
		{
			get { return Details.CD_NHTSAIndicator; }
			set { Details.CD_NHTSAIndicator = value; }
		}

		public virtual ZPropertyInfo CD_NHTSAIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NHTSAIndicator, x => Details.CD_NHTSAIndicatorInfo); }
		}

		#endregion

		#region CD_NHTSADisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_NHTSADisclaimReason
		{
			get { return Details.CD_NHTSADisclaimReason; }
			set { Details.CD_NHTSADisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_NHTSADisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_NHTSADisclaimReason, x => Details.CD_NHTSADisclaimReasonInfo); }
		}

		#endregion

		#region CD_ATFIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorWithoutDisclaimerList))]
		public virtual ZString CD_ATFIndicator
		{
			get { return Details.CD_ATFIndicator; }
			set { Details.CD_ATFIndicator = value; }
		}

		public virtual ZPropertyInfo CD_ATFIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ATFIndicator, x => Details.CD_ATFIndicatorInfo); }
		}

		#endregion

		#region CD_CPSCIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_CPSCIndicator
		{
			get { return Details.CD_CPSCIndicator; }
			set { Details.CD_CPSCIndicator = value; }
		}

		public virtual ZPropertyInfo CD_CPSCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CPSCIndicator, x => Details.CD_CPSCIndicatorInfo); }
		}

		#endregion

		#region CD_CPSCDisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_CPSCDisclaimReason
		{
			get { return Details.CD_CPSCDisclaimReason; }
			set { Details.CD_CPSCDisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_CPSCDisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_CPSCDisclaimReason, x => Details.CD_CPSCDisclaimReasonInfo); }
		}

		#endregion

		#region CD_DEAIndicator

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.US_OGAIndicatorList))]
		public virtual ZString CD_DEAIndicator
		{
			get { return Details.CD_DEAIndicator; }
			set { Details.CD_DEAIndicator = value; }
		}

		public virtual ZPropertyInfo CD_DEAIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DEAIndicator, x => Details.CD_DEAIndicatorInfo); }
		}

		#endregion

		#region CD_DEADisclaimReason

		[List(nameof(USClassificationLookups) + nameof(CusUSClassificationLookups.PGADisclaimReasonList))]
		public virtual ZString CD_DEADisclaimReason
		{
			get { return Details.CD_DEADisclaimReason; }
			set { Details.CD_DEADisclaimReason = value; }
		}

		public virtual ZPropertyInfo CD_DEADisclaimReasonInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_DEADisclaimReason, x => Details.CD_DEADisclaimReasonInfo); }
		}

		#endregion

		#region CD_EPAConsentNumber

		public virtual ZString CD_EPAConsentNumber
		{
			get { return Details.CD_EPAConsentNumber; }
			set { Details.CD_EPAConsentNumber = value; }
		}

		public virtual ZPropertyInfo CD_EPAConsentNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_EPAConsentNumber, x => Details.CD_EPAConsentNumberInfo); }
		}

		#endregion

		#region CD_EPANetQty

		public virtual ZDecimal CD_EPANetQty
		{
			get { return Details.CD_EPANetQty; }
			set { Details.CD_EPANetQty = value; }
		}

		public virtual ZPropertyInfo CD_EPANetQtyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_EPANetQty, x => Details.CD_EPANetQtyInfo); }
		}

		#endregion

		#region CD_EPANetQtyUQ

		public virtual ZString CD_EPANetQtyUQ
		{
			get { return Details.CD_EPANetQtyUQ; }
			set { Details.CD_EPANetQtyUQ = value; }
		}

		public virtual ZPropertyInfo CD_EPANetQtyUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_EPANetQtyUQ, x => Details.CD_EPANetQtyUQInfo); }
		}

		#endregion

		#region CD_ExportCertificateNo

		public virtual ZString CD_ExportCertificateNo
		{
			get { return Details.CD_ExportCertificateNo; }
			set { Details.CD_ExportCertificateNo = value; }
		}

		public virtual ZPropertyInfo CD_ExportCertificateNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_ExportCertificateNo, x => Details.CD_ExportCertificateNoInfo); }
		}

		#endregion

		#region CD_HazWasteTrackingNo

		public virtual ZString CD_HazWasteTrackingNo
		{
			get { return Details.CD_HazWasteTrackingNo; }
			set { Details.CD_HazWasteTrackingNo = value; }
		}

		public virtual ZPropertyInfo CD_HazWasteTrackingNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_HazWasteTrackingNo, x => Details.CD_HazWasteTrackingNoInfo); }
		}

		#endregion

		#region CD_PrimaryCountryNA

		public virtual ZBool CD_PrimaryCountryNA
		{
			get { return Details.CD_PrimaryCountryNA; }
			set { Details.CD_PrimaryCountryNA = value; }
		}

		public virtual ZPropertyInfo CD_PrimaryCountryNAInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_PrimaryCountryNA, x => Details.CD_PrimaryCountryNAInfo); }
		}

		#endregion

		#region CD_RN_NKPrimaryCountry

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.PrimaryCountries))]
		public virtual ZString CD_RN_NKPrimaryCountry
		{
			get { return Details.CD_RN_NKPrimaryCountry; }
			set { Details.CD_RN_NKPrimaryCountry = value; }
		}

		public virtual ZPropertyInfo CD_RN_NKPrimaryCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RN_NKPrimaryCountry, x => Details.CD_RN_NKPrimaryCountryInfo); }
		}

		#endregion

		#region CD_SecondaryCountryNA

		public virtual ZBool CD_SecondaryCountryNA
		{
			get { return Details.CD_SecondaryCountryNA; }
			set { Details.CD_SecondaryCountryNA = value; }
		}

		public virtual ZPropertyInfo CD_SecondaryCountryNAInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_SecondaryCountryNA, x => Details.CD_SecondaryCountryNAInfo); }
		}

		#endregion

		#region CD_RN_NKSecondaryCountry

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.SecondaryCountries))]
		public virtual ZString CD_RN_NKSecondaryCountry
		{
			get { return Details.CD_RN_NKSecondaryCountry; }
			set { Details.CD_RN_NKSecondaryCountry = value; }
		}

		public virtual ZPropertyInfo CD_RN_NKSecondaryCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RN_NKSecondaryCountry, x => Details.CD_RN_NKSecondaryCountryInfo); }
		}

		#endregion

		#region CD_RN_NKCastCountry

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CastCountries))]
		public virtual ZString CD_RN_NKCastCountry
		{
			get { return Details.CD_RN_NKCastCountry; }
			set { Details.CD_RN_NKCastCountry = value; }
		}

		public virtual ZPropertyInfo CD_RN_NKCastCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RN_NKCastCountry, x => Details.CD_RN_NKCastCountryInfo); }
		}

		#endregion

		#region CD_RN_NKCertificateOrigin

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CertificateOrigins))]
		[ResourceStringData("Enterprise.Customs.US.Business.AutoCusClassPartPivot|CD_RN_NKCertificateOrigin", ShortCaption = "Cert. Orig.", Caption = "Certificate Of Origin")]
		public virtual ZString CD_RN_NKCertificateOrigin
		{
			get { return Details.CD_RN_NKCertificateOrigin; }
			set { Details.CD_RN_NKCertificateOrigin = value; }
		}

		public virtual ZPropertyInfo CD_RN_NKCertificateOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RN_NKCertificateOrigin, x => Details.CD_RN_NKCertificateOriginInfo); }
		}

		#endregion

		#region CD_RN_NKMeltCountry

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.MeltCountries))]
		[ResourceStringData("Enterprise.Customs.US.Business.AutoCusClassPartPivot|CD_RN_NKMeltCountry", ShortCaption = "Melt. Ctry/Rgn.", Caption = "Melted/Poured Country/Region")]
		public virtual ZString CD_RN_NKMeltCountry
		{
			get { return Details.CD_RN_NKMeltCountry; }
			set { Details.CD_RN_NKMeltCountry = value; }
		}

		public virtual ZPropertyInfo CD_RN_NKMeltCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CD_RN_NKMeltCountry, x => Details.CD_RN_NKMeltCountryInfo); }
		}

		#endregion

		#endregion

		#region ClassificationDetails

		public CusUSClassificationLookups USClassificationLookups
		{
			get { return Details.Lookups; }
		}

		[ChildEditable(true)]
		[BusinessObjectTestExclude]
		CusUSClassificationCollection CusUSClassifications
		{
			get
			{
				if (cusUSClassifications == null)
				{
					cusUSClassifications = new CusUSClassificationCollection(this);
					cusUSClassifications.Load();
					RegisterEditableChildObject(cusUSClassifications);
				}
				return cusUSClassifications;
			}
		}
		CusUSClassificationCollection cusUSClassifications;

		public CusUSClassification Details
		{
			get
			{
				if (ReferenceEquals(fDetails, null) || fDetails.IsDeleted)
				{
					if (IsDeleted)
					{
						fDetails = Factory.GetNull<CusUSClassification>();
					}
					else
					{
						fDetails = CusUSClassifications.Cast<CusUSClassification>().OrderBy(x => x.PK).FirstOrDefault() ?? CusUSClassifications.AddNew();
						fDetails.HasChangesChanged -= ClearAuditOnChanged;
						fDetails.HasChangesChanged += ClearAuditOnChanged;
					}
				}
				return fDetails;
			}
		}
		CusUSClassification fDetails;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CusUSClassifications.RemoveAndDeleteAll();
			}
			base.Delete();
		}
		#endregion
	}
}
