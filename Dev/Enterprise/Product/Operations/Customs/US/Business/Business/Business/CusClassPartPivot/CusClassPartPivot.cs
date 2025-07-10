using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusClassPartPivot :
		AutoCusClassPartPivot,
		Integration.Customs.US.ICusClassPartPivot,
		ICusAddInfoTypeSupporter,
		IHaveAdditionalDataForBorderWise,
		ICusCodeDataTypeSupporter,
		ITariffProvider,
		IAddInfoManager
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new OrgSupplierPart Part
		{
			get { return base.Part; }
		}

		public new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		public new CusClassPartPivotLookups Lookups
		{
			get { return (CusClassPartPivotLookups)base.Lookups; }
		}

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		public new CusClassPartPivotValidation Validation
		{
			get { return (CusClassPartPivotValidation)base.Validation; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				ZString childType = CI_ChildType;
				var part = CI_CI_Parent.IsEmpty ? Part : null;
				OGAAgencyRequirements.RemoveAndDeleteAll();
				ExportPGAAgencyRequirements.RemoveAndDeleteAll();
				CensusWarningOverrides.RemoveAndDeleteAll();

				PGAs.RemoveAndDeleteAll();
				VehicleLines.RemoveAndDeleteAll();
				PSTLines.RemoveAndDeleteAll();
				HFCHeaders.RemoveAndDeleteAll();
				APHISHeaders.RemoveAndDeleteAll();
				ATFLines.RemoveAndDeleteAll();
				AMSLines.RemoveAndDeleteAll();
				ACEFDAs.RemoveAndDeleteAll();
				OMCHeaders.RemoveAndDeleteAll();
				TTBLines.RemoveAndDeleteAll();
				NHTSALines.RemoveAndDeleteAll();
				CPSCLines.RemoveAndDeleteAll();
				DEAHeaders.RemoveAndDeleteAll();
				NMFSLines.RemoveAndDeleteAll();
				FWSLines.RemoveAndDeleteAll();

				base.Delete();
				if (part != null && !childType.IsEmpty)
				{
					foreach (CusClassPartPivot pivot in part.GetUSPivots())
					{
						if (!pivot.IsDeleted && pivot.CI_ChildType == childType)
						{
							pivot.Validation.ValidateAll();
						}
					}
				}
			}
		}

		protected override ZString GetTariffNumbersIncludingComponents()
		{
			if (tariffNumbersIncludingComponents == null)
			{
				tariffNumbersIncludingComponents = new CachedProperty<ZString>(Factory, delegate
				{
					var result = new ZStringBuilder();

					result.AppendIfNotEmpty(FormattedTariffNumber);

					Children.Cast<CusClassPartPivot>().
						OrderBy(x => x.CI_ChildListOrder).
						TakeWhile(x => !x.CI_TariffNum.IsEmpty).
						Take(4).
						ToList().
						ForEach(x => result.Append(x.CI_TariffNum));

					if (Children.Count > 4)
					{
						result.Append("...");
					}

					return result.ToStringWithDelimiterBetweenAppends("/");
				});
			}

			return tariffNumbersIncludingComponents.Value;
		}
		CachedProperty<ZString> tariffNumbersIncludingComponents;

		public USCTariff ImportTariff
		{
			get
			{
				return IsTariffUSCTariff ?
					new USCTariff.Loader(Factory).LoadBestMatch(TariffNumber, EffectiveDate) : null;
			}
		}

		public TariffView ExportTariff
		{
			get
			{
				TariffView tariff = null;
				if (IsExportTariff)
				{
					var tariffType = GetTariffType();
					tariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, tariffType, TariffNumber, EffectiveDate);
				}
				return tariff;
			}
		}

		internal bool IsTariffUSCTariff
		{
			get { return IsImportClassification || Parent != null && Parent.IsImportClassification; }
		}

		public ZBool IsHTE => CI_ChildType == ClassificationTypeList.Codes.HTE;
		public ZBool IsSHB => CI_ChildType == ClassificationTypeList.Codes.SHB;
		public ZBool IsExportTariff => IsHTE || IsSHB;

		public ZDateTime EffectiveDate
		{
			get
			{
				if (effectiveDateCached == null)
				{
					effectiveDateCached = new CachedProperty<ZDateTime>(Factory, () =>
					{
						var result = ZDateTime.Today;
						var dateStart = CI_DateStart;
						if (dateStart.IsValid && (dateStart > result || CI_DateEnd < result))
						{
							result = dateStart;
						}

						return result;
					});
				}
				return effectiveDateCached.Value;
			}
		}
		CachedProperty<ZDateTime> effectiveDateCached;

		public bool IsAMSEGGEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AMSEGG, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
		public bool IsAMSPNTEffective => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.AMSPNT, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);
		protected override bool IsExportClassificationCore => IsExportTariff;

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ClassificationTypes))]
		[ReadOnlyMember(nameof(CI_ChildType_ReadOnly))]
		public override ZString CI_ChildType
		{
			get { return base.CI_ChildType; }
			set
			{
				var oldValue = CI_ChildType;

				var args = HandleOnTariffTypeChangingEvent(value, base.CI_ChildType);

				if (!args.Cancel)
				{
					base.CI_ChildType = value;
					if (!IsCopying && oldValue != CI_ChildType)
					{
						if (CI_ChildType != ClassificationTypeList.Codes.HTI)
						{
							CI_SupplementalTariff = "";
						}

						DefaultOGAIndicatorsWhenChanges();

						UpdateTaxRateFieldsOnTariffChange();

						if (NeedToRemovePGAData)
						{
							RemovePGAOGAData();
						}
						Details.MarkAsNeedingValidation();
					}
				}
			}
		}

		bool CI_ChildType_ReadOnly
		{
			get { return HasChildren; }
		}

		public ZBool HasChildren
		{
			get { return Children.Count > 0; }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		public override ZString CI_TariffNum
		{
			get { return base.CI_TariffNum; }
			set
			{
				var oldValueForLog = CI_FormattedTariffNum;
				var oldValue = CI_TariffNum;
				base.CI_TariffNum = value;
				if (oldValue != CI_TariffNum && !IsCopying)
				{
					DefaultOGAIndicatorsWhenChanges();
					if (fOGARequirementCalculator != null)
					{
						fOGARequirementCalculator.Initialise();
					}
				}
				this.LogIfPropertyValueChange("Tariff", oldValueForLog, CI_FormattedTariffNum);
			}
		}

		protected override bool UseUniversalTariff => false;

		public ZBool IsOnlySteelProductAvailable => CI_TariffNum.StartsWith("72", StringComparison.OrdinalIgnoreCase) || CI_TariffNum.StartsWith("73", StringComparison.OrdinalIgnoreCase);

		public ZBool IsOnlyAluminumProductAvailable => CI_TariffNum.StartsWith("76", StringComparison.OrdinalIgnoreCase);

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		public override ZString CI_FormattedTariffNum
		{
			get { return base.CI_FormattedTariffNum; }
			set { base.CI_FormattedTariffNum = value; }
		}

		public override ZString CI_SupplementalTariff
		{
			get
			{
				return base.CI_SupplementalTariff;
			}
			set
			{
				var oldValue = CI_FormattedSupplementalTariff;
				var hasChanges = value != CI_SupplementalTariff;
				base.CI_SupplementalTariff = value;

				if (hasChanges && !IsCopying)
				{
					if (fOGARequirementCalculator != null)
					{
						fOGARequirementCalculator.Initialise();
					}

					DefaultOGAIndicatorsWhenChanges();
					LogIfPropertyValueChange("Prov/Prog. Tariff", oldValue, CI_FormattedSupplementalTariff);
				}
			}
		}

		public USCTariff ImportSupTariff
		{
			get
			{
				return IsTariffUSCTariff ?
					new USCTariff.Loader(Factory).LoadBestMatch(CI_SupplementalTariff, EffectiveDate) : null;
			}
		}

		bool IsSupplementalTariffReadOnly
		{
			get { return !(Parent == null && IsImportClassification) && !(Parent != null && Parent.IsImportClassification); }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsSupplementalTariffReadOnly))]
		public override ZString CI_FormattedSupplementalTariff
		{
			get { return base.CI_FormattedSupplementalTariff; }
			set { base.CI_FormattedSupplementalTariff = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Classifications))]
		[ReadOnlyMember(nameof(CI_CC_ReadOnly))]
		public override ZGuid CI_CC
		{
			get { return base.CI_CC; }
			set
			{
				bool hasChanges = base.CI_CC != value;
				base.CI_CC = value;
				if (hasChanges && !IsCopying)
				{
					if (fOGARequirementCalculator != null)
					{
						fOGARequirementCalculator.Initialise();
					}

					UpdateTaxRateFieldsOnTariffChange();
					DefaultOGAIndicatorsWhenChanges();
				}
			}
		}

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.USCountryList))]
		public override ZString CD_UC_NKCountryOfOrigin
		{
			get { return base.CD_UC_NKCountryOfOrigin; }
			set
			{
				bool hasChanges = base.CD_UC_NKCountryOfOrigin != value;
				base.CD_UC_NKCountryOfOrigin = value;

				if (hasChanges && !IsCopying)
				{
					if (fOGARequirementCalculator != null)
					{
						fOGARequirementCalculator.Initialise();
					}

					DefaultOGAIndicatorsWhenChanges();
				}
			}
		}

		public override ZString CD_DDTCUSMLCategoryCode
		{
			get { return base.CD_DDTCUSMLCategoryCode; }
			set
			{
				base.CD_DDTCUSMLCategoryCode = value;
				if (CD_JurisdictionNumber_ReadOnly)
				{
					CD_DDTCJurisdictionNumber = ZString.Empty;
				}

				CD_DDTCJurisdictionNumberInfo.RefreshBinding();
			}
		}

		bool CD_JurisdictionNumber_ReadOnly => CD_DDTCUSMLCategoryCode != USMLCategoryCodes.Codes.MiscellaneousArticles;

		[ReadOnlyMember(nameof(CD_JurisdictionNumber_ReadOnly))]
		public override ZString CD_DDTCJurisdictionNumber
		{
			get { return base.CD_DDTCJurisdictionNumber; }
			set { base.CD_DDTCJurisdictionNumber = value; }
		}

		void SetIndicatorsWhenChanges()
		{
			CD_LaceyActIndicator = PGARequirementIndicator.RequireACE_LaceyData ? OGAIndicatorList.Codes.Declared : "";
			CD_LaceyActDisclaimReason = ZString.Empty;
			CD_ACEFDAIndicator = PGARequirementIndicator.RequireACEFDA ? OGAIndicatorList.Codes.Declared : "";
			CD_ACEFDADisclaimReason = ZString.Empty;
			CD_NHTSAIndicator = PGARequirementIndicator.RequireNHTSA ? OGAIndicatorList.Codes.Declared : "";
			CD_NHTSADisclaimReason = ZString.Empty;
			CD_ODSIndicator = PGARequirementIndicator.RequireODS ? OGAIndicatorList.Codes.Declared : "";
			CD_ODSDisclaimReason = ZString.Empty;
			CD_OMCIndicator = ZZCustomsFunctionality.IsOMCEffective && PGARequirementIndicator.RequireOMC ? OGAIndicatorList.Codes.Declared : "";
			CD_OMCDisclaimReason = ZString.Empty;
			CD_TSCAClaimIndicator = PGARequirementIndicator.RequireTSCA ? OGAIndicatorList.Codes.Declared : "";
			CD_TSCADisclaimReason = ZString.Empty;
			CD_PSTIndicator = PGARequirementIndicator.RequirePST ? OGAIndicatorList.Codes.Declared : "";
			CD_PSTDisclaimReason = ZString.Empty;
			CD_PSTDisclaimProgram = ZString.Empty;
			CD_VNEIndicator = PGARequirementIndicator.RequireVNE ? OGAIndicatorList.Codes.Declared : "";
			CD_VNEDisclaimReason = ZString.Empty;
			CD_ATFIndicator = ZString.Empty;
			CD_TTBIndicator = PGARequirementIndicator.RequireTTB ? OGAIndicatorList.Codes.Declared : "";
			CD_TTBDisclaimReason = ZString.Empty;
			CD_NOPIndicator = PGARequirementIndicator.RequireNOP ? OGAIndicatorList.Codes.Declared : "";
			CD_NOPDisclaimReason = ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void DefaultOGAIndicatorsWhenChanges()
		{
			if (IsImportClassification)
			{
				SetIndicatorsWhenChanges();

				var isRequireAMSEGG = IsAMSEGGEffective && PGARequirementIndicator.RequireAMSEGG;
				var isRequireAMSORD = PGARequirementIndicator.RequireAMSORD;
				var isRequireAMSPNT = IsAMSPNTEffective && PGARequirementIndicator.RequireAMSPNT;
				var isRequireAMS = isRequireAMSEGG || isRequireAMSORD || isRequireAMSPNT;
				CD_AMSIndicator = isRequireAMS ? OGAIndicatorList.Codes.Declared : "";
				CD_AMSDisclaimReason = ZString.Empty;
				CD_AMSDisclaimProgram = ZString.Empty;

				CD_DDTCIndicator = ZString.Empty;

				CD_CPSCIndicator = ZZCustomsFunctionality.IsCPSCEffective && PGARequirementIndicator.RequireCPSC ? OGAIndicatorList.Codes.Declared : "";
				CD_CPSCDisclaimReason = ZString.Empty;

				CD_DEAIndicator = ZString.Empty;
				CD_DEADisclaimReason = ZString.Empty;

				CD_APHISIndicator = PGARequirementIndicator.RequireAPHIS ? OGAIndicatorList.Codes.Declared : "";
				CD_APHISDisclaimReason = ZString.Empty;

				CD_NMFS370Indicator = PGARequirementIndicator.RequireNMFS370 ? OGAIndicatorList.Codes.Declared : "";
				CD_NMFS370DisclaimReason = ZString.Empty;
				CD_NMFSCOAIndicator = PGARequirementIndicator.RequireNMFSCOA ? OGAIndicatorList.Codes.Declared : "";
				CD_NMFSAMRIndicator = PGARequirementIndicator.RequireNMFSAMR ? OGAIndicatorList.Codes.Declared : "";
				CD_NMFSAMRDisclaimReason = ZString.Empty;
				CD_NMFSHMSIndicator = PGARequirementIndicator.RequireNMFSHMS ? OGAIndicatorList.Codes.Declared : "";
				CD_NMFSHMSDisclaimReason = ZString.Empty;
				CD_NMFSSIMPIndicator = PGARequirementIndicator.RequireNMFSSIM ? OGAIndicatorList.Codes.Declared : "";

				CD_FWSIndicator = ZZCustomsFunctionality.IsFWSEffective && PGARequirementIndicator.RequireFWS ? OGAIndicatorList.Codes.Declared : "";
				CD_FWSDisclaimReason = ZString.Empty;

				CD_NOPIndicator = PGARequirementIndicator.RequireNOP ? OGAIndicatorList.Codes.Declared : "";
				CD_NOPDisclaimReason = ZString.Empty;

				CD_HFCIndicator = PGARequirementIndicator.RequireHFC ? OGAIndicatorList.Codes.Declared : "";
				CD_HFCDisclaimReason = ZString.Empty;
			}
			else if (IsExportTariff)
			{
				CD_AMSIndicator = ExportPGARequirementIndicator.RequireAMS ? OGAIndicatorList.Codes.Declared : "";
				CD_ATFIndicator = ExportPGARequirementIndicator.RequireATF ? OGAIndicatorList.Codes.Declared : "";
				CD_FWSIndicator = ExportPGARequirementIndicator.RequireFWS ? OGAIndicatorList.Codes.Declared : "";
				CD_PSTIndicator = ExportPGARequirementIndicator.RequireEPA ? OGAIndicatorList.Codes.Declared : "";
				CD_NMFSHMSIndicator = ExportPGARequirementIndicator.RequireNMFS ? OGAIndicatorList.Codes.Declared : "";
				CD_TTBIndicator = ExportPGARequirementIndicator.RequireTTB ? OGAIndicatorList.Codes.Declared : "";
			}
		}

		public override ZString CD_CPSCDisclaimReason
		{
			get { return base.CD_CPSCDisclaimReason; }
			set
			{
				if (base.CD_CPSCDisclaimReason != value)
				{
					CPSCLines.RemoveAndDeleteAll();
					if (value == PGADisclaimReasonList.Codes.A && CD_CPSCIndicator == OGAIndicatorList.Codes.Disclaimed)
					{
						CPSCLines.AddNew();
					}
					base.CD_CPSCDisclaimReason = value;
				}
			}
		}

		public PGARequirementIndicator PGARequirementIndicator
		{
			get { return new PGARequirementIndicator(() => ImportTariff, () => ImportSupTariff, () => EffectiveDate, (x) => true, () => CD_UC_NKCountryOfOrigin); }
		}

		public ExportPGARequirementIndicator ExportPGARequirementIndicator
		{
			get { return new ExportPGARequirementIndicator(Factory, () => ExportTariff, EffectiveDate); }
		}

		void UpdateTaxRateFieldsOnTariffChange()
		{
			USCTariff importTariff = ImportTariff;

			if (importTariff != null)
			{
				CD_TaxApplicability = importTariff.IsTaxRequired ? TaxApplyList.Codes.Yes : string.Empty;
			}
			else
			{
				CD_TaxApplicability = ZString.Empty;
			}

			CD_TaxCode = ZString.Empty;
			CD_TaxRateType = ZString.Empty;
			CD_TaxRateDesc = ZString.Empty;
			CD_TaxRate = ZDecimal.Zero;
			CD_TTBRateDesignationCode = ZString.Empty;
			CD_CBMADefaultTaxRate = ZDecimal.Zero;
		}

		public override ZString CD_TaxApplicability
		{
			get { return base.CD_TaxApplicability; }
			set
			{
				bool hasChanges = base.CD_TaxApplicability != value;
				base.CD_TaxApplicability = value;

				if (hasChanges && !IsCopying)
				{
					CD_TaxCode = ZString.Empty;
					CD_TaxRateDesc = ZString.Empty;
					CD_TaxRate = ZDecimal.Zero;
					CD_TTBRateDesignationCode = ZString.Empty;
					CD_CBMADefaultTaxRate = ZDecimal.Zero;

					CD_TaxCodeInfo.RefreshBinding();
				}
			}
		}

		internal bool IsTaxRateOverridden
		{
			get { return CD_TaxApplicability == TaxApplyList.Codes.Override; }
		}

		[ReadOnlyMember(nameof(TaxCode_ReadOnly))]
		public override ZString CD_TaxCode
		{
			get
			{
				ZString result = base.CD_TaxCode;

				if (result.IsEmpty && TaxApplyList.IsTaxApplicable(CD_TaxApplicability))
				{
					USCTariff importTariff = ImportTariff;
					result = importTariff != null ? importTariff.GetUniqueTaxCode() : ZString.Empty;
				}

				return result;
			}
			set
			{
				ZString oldTaxCode = CD_TaxCode;

				USCTariff importTariff = ImportTariff;
				ZString valueToAssign = importTariff != null && importTariff.GetUniqueTaxCode() == value ? ZString.Empty : value;

				base.CD_TaxCode = valueToAssign;

				if (oldTaxCode != CD_TaxCode && !IsCopying)
				{
					CD_TaxRateType = ZString.Empty;
					CD_TaxRateDesc = ZString.Empty;
					CD_TaxRate = ZDecimal.Zero;

					if (IsCBMAProductClaim)
					{
						DefaultTaxRateDescForCBMAIfPossible();
					}
					else
					{
						CD_TTBRateDesignationCode = ZString.Empty;
						CD_CBMADefaultTaxRate = ZDecimal.Zero;
					}
				}
			}
		}

		bool TaxCode_ReadOnly
		{
			get { return !IsTaxRateOverridden; }
		}

		[ReadOnlyMember(nameof(TaxRateT_ReadOnly))]
		public override ZString CD_TaxRateType
		{
			get { return base.CD_TaxRateType; }
			set
			{
				var oldValue = CD_TaxRateType;
				base.CD_TaxRateType = value;
				if (oldValue != value && !IsCopying)
				{
					CD_TaxRateDesc = ZString.Empty;
				}
			}
		}

		bool TaxRateT_ReadOnly
		{
			get
			{
				bool result = true;

				if (ImportTariff != null && !CD_TaxCode.IsEmpty)
				{
					USCTariffDutyRate dutyRate = ImportTariff.DutyRates.GetRateForTaxFeeClassCode(CD_TaxCode);
					result = dutyRate == null || dutyRate.UD_TaxFeeComputationCode != ComputationCodeList.Codes.SpecificSpecific;
				}

				return result;
			}
		}

		[DecimalPlaces(8)]
		public override ZDecimal CD_TaxRate
		{
			get
			{
				ZDecimal result = base.CD_TaxRate;

				if (result.IsEmpty && !CD_TaxRateDesc.IsEmpty)
				{
					result = AppendixBTaxRateList.GetRate(CD_TaxRateDesc);
				}

				return result;
			}
			set { base.CD_TaxRate = value; }
		}

		[ReadOnlyMember(nameof(TaxRateS_ReadOnly))]
		public override ZString CD_TaxRateDesc
		{
			get
			{
				ZString result = base.CD_TaxRateDesc;

				if (result.IsEmpty && TaxApplyList.IsTaxApplicable(CD_TaxApplicability))
				{
					result = AppendixBTaxRateList.GetNormalTaxRateString(ImportTariff, CD_TaxCode, CD_TaxRateType);
				}

				return result;
			}
			set
			{
				var valueToAssign = AppendixBTaxRateList.GetNormalTaxRateString(ImportTariff, CD_TaxCode, CD_TaxRateType) == value ? ZString.Empty : value;

				if (!settingCD_TaxRateDescInProgress && CD_TaxRateDesc != valueToAssign)
				{
					try
					{
						settingCD_TaxRateDescInProgress = true;
						base.CD_TaxRateDesc = valueToAssign;

						if (IsTaxRateSpecifiedManually)
						{
							if (IsCBMAProductClaim && USClassificationLookups.TaxRateList[CD_TaxRateDesc] is CBMATaxRate taxRate)
							{
								CD_TaxRate = taxRate.Rate;
							}
							else
							{
								CD_TaxRate = ZDecimal.Zero;
							}
						}
						else
						{
							CD_TaxRate = AppendixBTaxRateList.GetRate(CD_TaxRateDesc);
						}

						if (IsTaxRateReduced)
						{
							CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.C;
						}

						CD_TaxRateInfo.RefreshBinding();
					}
					finally
					{
						settingCD_TaxRateDescInProgress = false;
					}
				}
			}
		}

		bool settingCD_TaxRateDescInProgress;

		public bool TaxRateS_ReadOnly
		{
			get { return !IsTaxRateOverridden; }
		}

		[DecimalPlaces(8)]
		[ReadOnlyMember(nameof(CBMADefaultTaxRate_ReadOnly))]
		public override ZDecimal CD_CBMADefaultTaxRate
		{
			get => base.CD_CBMADefaultTaxRate;
			set => base.CD_CBMADefaultTaxRate = value;
		}

		public bool CBMADefaultTaxRate_ReadOnly
		{
			get { return !IsCBMAProductClaim; }
		}

		[ReadOnlyMember(nameof(TTBRateDesignationCode_ReadOnly))]
		public override ZString CD_TTBRateDesignationCode
		{
			get => base.CD_TTBRateDesignationCode;
			set
			{
				base.CD_TTBRateDesignationCode = value;

				if (USClassificationLookups.CBMATaxRateList[CD_TTBRateDesignationCode] is CBMATaxRate taxRate)
				{
					CD_CBMADefaultTaxRate = taxRate.Rate;
				}
				else
				{
					CD_CBMADefaultTaxRate = ZDecimal.Zero;
				}

				CD_TTBRateDesignationCodeInfo.RefreshBinding();
			}
		}

		public bool TTBRateDesignationCode_ReadOnly
		{
			get { return !IsCBMAProductClaim; }
		}

		public ZBool IsTaxRateSpecifiedManually
		{
			get { return CD_TaxRateDesc.EqualsIgnoringCase(AppendixBTaxRateList.Codes.Specify) || IsTaxRateReduced; }
		}

		public ZBool IsTaxRateReduced
		{
			get { return CD_TaxRateDesc.EqualsIgnoringCase(AppendixBTaxRateList.CBMAEligible); }
		}

		public ZBool IsTaxRateSpecifiedInList
		{
			get { return !IsTaxRateSpecifiedManually; }
		}

		public ZBool IsCBMAProductClaim => Factory.GetValue(ref isCBMAProductClaimCached, () => CD_ProductClaim == SecondarySpecProgIndicatorList.Codes.C);
		CachedProperty<ZBool> isCBMAProductClaimCached;

		public override ZGuid CI_CI_Parent
		{
			get { return base.CI_CI_Parent; }
			set { base.CI_CI_Parent = value; }
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		[List(nameof(Part) + "." + nameof(Customs.Business.OrgSupplierPart.Lookups) + "." + nameof(Customs.Business.OrgSupplierPartLookups.OP_WeightUQ_List))]
		public override ZString CD_WeightUQ
		{
			get { return base.CD_WeightUQ; }
			set { base.CD_WeightUQ = value; }
		}

		[MeasureUnit(Schema.CD_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal CD_GrossWeight
		{
			get { return base.CD_GrossWeight; }
			set { base.CD_GrossWeight = value; }
		}

		[MeasureUnit(Schema.CD_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal CD_NetWeight
		{
			get { return base.CD_NetWeight; }
			set { base.CD_NetWeight = value; }
		}

		public override ZString CD_CottonFeeExempt
		{
			get { return base.CD_CottonFeeExempt; }
			set
			{
				base.CD_CottonFeeExempt = value;
				if (!IsCopying && CD_CottonFeeExempt == YesNoDefaultList.Codes.Yes)
				{
					CD_CottonCertificate = ZString.Empty;
				}
			}
		}

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.ProductClaimList))]
		public override ZString CD_ProductClaim
		{
			get { return base.CD_ProductClaim; }
			set
			{
				var oldValue = base.CD_ProductClaim;
				base.CD_ProductClaim = value;
				if (CD_ProductClaim != oldValue && !IsCopying)
				{
					if (IsCBMAProductClaim)
					{
						DefaultTaxRateDescForCBMAIfPossible();
					}
					else
					{
						CD_TTBRateDesignationCode = ZString.Empty;
						CD_CBMADefaultTaxRate = ZDecimal.Zero;
					}
				}
			}
		}

		public override ZGuid CI_OP
		{
			get { return base.CI_OP; }
			set
			{
				var oldValue = CI_OP;
				base.CI_OP = value;
				if (!IsCopying && oldValue != CI_OP)
				{
					PopulateChildrenCI_OP();
				}
			}
		}

		public override ZDecimal CD_AMMVPerUnit
		{
			get => base.CD_AMMVPerUnit;
			set
			{
				var oldValue = CD_AMMVPerUnit;
				base.CD_AMMVPerUnit = value;
				var newValue = CD_AMMVPerUnit;
				if (!IsCopying && oldValue != newValue)
				{
					if (newValue.IsEmpty || CD_AMMVPercentage.IsEmpty)
					{
						CD_AMMVPercentageInfo.RefreshBinding();
					}
					else
					{
						CD_AMMVPercentage = ZDecimal.Zero;
					}
				}
			}
		}
		bool CD_AMMVPerUnit_ReadOnly => !CD_AMMVPercentage.IsEmpty && CD_AMMVPerUnit.IsEmpty;

		[ReadOnlyMember(nameof(CD_AMMVPerUnit_ReadOnly))]
		public override ZString CD_AMMVPerUnitCurrency { get => base.CD_AMMVPerUnitCurrency; set => base.CD_AMMVPerUnitCurrency = value; }

		[ReadOnlyMember(nameof(CD_AMMVPercentage_ReadOnly))]
		public override ZDecimal CD_AMMVPercentage
		{
			get => base.CD_AMMVPercentage;
			set
			{
				var oldValue = CD_AMMVPercentage;
				base.CD_AMMVPercentage = value;
				var newValue = CD_AMMVPercentage;
				if (!IsCopying && oldValue != newValue)
				{
					if (newValue.IsEmpty || CD_AMMVPerUnit.IsEmpty)
					{
						CD_AMMVPerUnitInfo.RefreshBinding();
					}
					else
					{
						CD_AMMVPerUnit = ZDecimal.Zero;
						CD_AMMVPerUnitCurrency = ZString.Empty;
					}
				}
			}
		}

		bool CD_AMMVPercentage_ReadOnly => !CD_AMMVPerUnit.IsEmpty && CD_AMMVPercentage.IsEmpty;

		void DefaultTaxRateDescForCBMAIfPossible()
		{
			var cbmaList = USClassificationLookups.CBMATaxRateList;
			CD_TTBRateDesignationCode = cbmaList.Count == 1 ? cbmaList[0].Code : string.Empty;
		}

		void PopulateChildrenCI_OP()
		{
			var partPK = CI_OP;
			foreach (var child in Factory.Load<CusClassPartPivot>(new ZQuery(CusClassPartPivotSchema.CI_CI_Parent, PK)).Where(x => x.CI_OP != partPK))
			{
				child.CI_OP = partPK;
			}
		}

		public ReconIssues GetReconIssueCalculated()
		{
			return ReconIssueCodeList.GetReconIssuesValue(CD_ReconIssue);
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : PivotFetchStrategy
		{
			public Strategy(CusClassPartPivot pivot)
				: base(pivot)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				CusClassPartPivot pivot = (CusClassPartPivot)BusinessObject;
				Factory.AddFetchHint(OrgSupplierPartSchema.PK, pivot.CI_OP);
				Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(CusUSClassificationSchema.CD_ParentID, pivot.PK);
				Factory.AddFetchHint(CusLineTariffDetailSchema.BZ_ParentID, pivot.PK);
			}

			protected override void FetchForFactorySaveCore()
			{
				base.FetchForFactorySaveCore();
				Factory.AddFetchHint(CusClassPartPivotSchema.CI_CI_Parent, BusinessObject.PK);
			}
		}

		#endregion

		#region Collections

		#region PGA FDA

		[ChildEditable(true)]
		public ACEFDACollection ACEFDAs
		{
			get
			{
				if (aceFDAs == null)
				{
					aceFDAs = new ACEFDACollection(this);
					aceFDAs.Load();
					RegisterEditableChildObject(aceFDAs);
				}
				return aceFDAs;
			}
		}
		ACEFDACollection aceFDAs;

		public bool HasACEFDAs
		{
			get { return ACEFDAs.Count > 0; }
		}

		#endregion

		#region PGA

		[ChildEditable(true)]
		public PGACollection PGAs
		{
			get
			{
				if (fPGAs == null)
				{
					fPGAs = new PGACollection(this);
					fPGAs.Load();
					RegisterEditableChildObject(fPGAs);
				}
				return fPGAs;
			}
		}
		PGACollection fPGAs;

		public bool HasLaceyActData
		{
			get { return PGAs.Count > 0; }
		}

		#endregion

		#region OMC

		[ChildEditable(true)]
		public OMCHeaderCollection OMCHeaders
		{
			get
			{
				if (fOMCHeaders == null)
				{
					fOMCHeaders = new OMCHeaderCollection(this);
					fOMCHeaders.Load();
					RegisterEditableChildObject(fOMCHeaders);
				}
				return fOMCHeaders;
			}
		}
		OMCHeaderCollection fOMCHeaders;

		public bool HasOMCHeaders
		{
			get { return OMCHeaders.Count > 0; }
		}

		#endregion

		#region TTB

		[ChildEditable(true)]
		public TTBLineCollection TTBLines
		{
			get
			{
				if (ttbLines == null)
				{
					ttbLines = new TTBLineCollection(this);
					ttbLines.Load();
					RegisterEditableChildObject(ttbLines);
				}
				return ttbLines;
			}
		}
		TTBLineCollection ttbLines;

		public bool HasTTBData
		{
			get { return TTBLines.Count > 0; }
		}

		#endregion

		#region FWS
		[ChildEditable(true)]
		public FWSHeaderCollection FWSLines
		{
			get
			{
				if (fwsLines == null)
				{
					fwsLines = new FWSHeaderCollection(this);
					fwsLines.Load();
					RegisterEditableChildObject(fwsLines);
				}
				return fwsLines;
			}
		}
		FWSHeaderCollection fwsLines;

		public bool HasFWSLines
		{
			get { return FWSLines.Count > 0; }
		}

		#endregion

		#region CensusWarning

		[ChildEditable(true)]
		public CensusWarningOverrideCollection CensusWarningOverrides
		{
			get
			{
				if (censusWarningOverrides == null)
				{
					censusWarningOverrides = new CensusWarningOverrideCollection(this);
					censusWarningOverrides.Load();
					RegisterEditableChildObject(censusWarningOverrides);
				}
				return censusWarningOverrides;
			}
		}
		CensusWarningOverrideCollection censusWarningOverrides;

		#endregion

		#region NHTSA

		[ChildEditable(true)]
		public NHTSAHeaderCollection NHTSALines
		{
			get
			{
				if (fNHTSALines == null)
				{
					fNHTSALines = new NHTSAHeaderCollection(this);
					fNHTSALines.Load();
					RegisterEditableChildObject(fNHTSALines);
				}
				return fNHTSALines;
			}
		}
		NHTSAHeaderCollection fNHTSALines;

		public bool HasNHTSALines
		{
			get { return NHTSALines.Count > 0; }
		}
		#endregion

		#region CPSC

		[ChildEditable(true)]
		public CPSCHeaderCollection CPSCLines
		{
			get
			{
				if (fCPSCLines == null)
				{
					fCPSCLines = new CPSCHeaderCollection(this);
					fCPSCLines.Load();
					RegisterEditableChildObject(fCPSCLines);
				}
				return fCPSCLines;
			}
		}
		CPSCHeaderCollection fCPSCLines;

		public bool HasCPSCLines
		{
			get { return CPSCLines.Count > 0; }
		}
		#endregion

		#region VNE
		[ChildEditable(true)]
		public VehicleCollection VehicleLines
		{
			get
			{
				if (vehicleLines == null)
				{
					vehicleLines = new VehicleCollection(this);
					vehicleLines.Load();
					RegisterEditableChildObject(vehicleLines);
				}
				return vehicleLines;
			}
		}
		VehicleCollection vehicleLines;

		public ZBool HasVNELines
		{
			get { return VehicleLines.Count > 0; }
		}
		#endregion

		#region PST
		[ChildEditable(true)]
		public PesticideCollection PSTLines
		{
			get
			{
				if (pstLines == null)
				{
					pstLines = new PesticideCollection(this);
					pstLines.Load();
					RegisterEditableChildObject(pstLines);
				}
				return pstLines;
			}
		}
		PesticideCollection pstLines;

		public ZBool HasPSTLines
		{
			get { return PSTLines.Count > 0; }
		}

		#endregion

		#region HFC

		[ChildEditable(true)]
		public USHFCHeaderCollection HFCHeaders
		{
			get
			{
				if (hfcHeaders == null)
				{
					hfcHeaders = new USHFCHeaderCollection(this);
					hfcHeaders.Load();
					RegisterEditableChildObject(hfcHeaders);
				}
				return hfcHeaders;
			}
		}
		USHFCHeaderCollection hfcHeaders;

		public ZBool HasHFCHeaders
		{
			get { return HFCHeaders.Count > 0; }
		}

		#endregion

		#region DEA

		[ChildEditable(true)]
		public DEAHeaderCollection DEAHeaders
		{
			get
			{
				if (fDEAHeaders == null)
				{
					fDEAHeaders = new DEAHeaderCollection(this);
					fDEAHeaders.Load();
					RegisterEditableChildObject(fDEAHeaders);
				}
				return fDEAHeaders;
			}
		}
		DEAHeaderCollection fDEAHeaders;

		public bool HasDEAHeaders
		{
			get { return DEAHeaders.Count > 0; }
		}

		#endregion

		#region APHIS

		[ChildEditable(true)]
		public APHISHeaderCollection APHISHeaders
		{
			get
			{
				if (fAPHISHeaders == null)
				{
					fAPHISHeaders = new APHISHeaderCollection(this);
					fAPHISHeaders.Load();
					RegisterEditableChildObject(fAPHISHeaders);
				}
				return fAPHISHeaders;
			}
		}
		APHISHeaderCollection fAPHISHeaders;

		public bool HasAPHISLines
		{
			get { return APHISHeaders.Count > 0; }
		}

		#endregion

		#endregion

		public bool IsODSIndBeDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(CD_ODSIndicator); }
		}

		public bool IsTSCAIndBeDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(CD_TSCAClaimIndicator); }
		}

		#region AMS Lines
		[ChildEditable(true)]
		public AMSCollection AMSLines
		{
			get
			{
				if (fAMSLines == null)
				{
					fAMSLines = new AMSCollection(this);
					fAMSLines.Load();
					RegisterEditableChildObject(fAMSLines);
				}
				return fAMSLines;
			}
		}
		AMSCollection fAMSLines;

		public bool HasAMSData
		{
			get { return AMSLines.Count > 0; }
		}

		#endregion

		#region US_TSCACertification

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_TSCAIndicatorList))]
		public ZString US_TSCACertification
		{
			get { return CD_TSCAIndicator; }
			set
			{
				CD_TSCAIndicator = value;
				if (!CD_TSCAIndicator.IsEmpty && CD_TSCAClaimIndicator != OGAIndicatorList.Codes.Declared)
				{
					CD_TSCAClaimIndicator = OGAIndicatorList.Codes.Declared;
				}
			}
		}

		public ZPropertyInfo US_TSCACertificationInfo
		{
			get { return CD_TSCAIndicatorInfo; }
		}

		#endregion

		#region US_TSCAODSCertIndividual

		[List(nameof(USClassificationLookups) + "." + nameof(CusUSClassificationLookups.CD_TSCAODSCertIndividualList))]
		public ZString US_TSCAODSCertIndividual
		{
			get { return CD_TSCAODSCertIndividual; }
			set { CD_TSCAODSCertIndividual = value; }
		}

		public ZPropertyInfo US_TSCAODSCertIndividualInfo
		{
			get { return CD_TSCAODSCertIndividualInfo; }
		}

		#endregion

		#region ATF

		[ChildEditable(true)]
		public ATFCollection ATFLines
		{
			get
			{
				if (fATFLines == null)
				{
					fATFLines = new ATFCollection(this);
					fATFLines.Load();
					RegisterEditableChildObject(fATFLines);
				}
				return fATFLines;
			}
		}
		ATFCollection fATFLines;

		public bool HasATFLines
		{
			get { return ATFLines.Count > 0; }
		}

		#endregion

		public event System.ComponentModel.CancelEventHandler OnTariffTypeChanging;

		System.ComponentModel.CancelEventArgs HandleOnTariffTypeChangingEvent(string changedTariffType, string oldValue)
		{
			var args = new System.ComponentModel.CancelEventArgs(false);

			if (oldValue == ClassificationTypeList.Codes.HTI && (changedTariffType == ClassificationTypeList.Codes.HTE || changedTariffType == ClassificationTypeList.Codes.SHB))
			{
				bool hasOGAPGADataToBeDeleted = HasPGAOGAData;

				if (hasOGAPGADataToBeDeleted && OnTariffTypeChanging != null)
				{
					OnTariffTypeChanging(this, args);
					NeedToRemovePGAData = true;
				}
			}

			return args;
		}

		internal bool NeedToRemovePGAData { get; set; }

		internal bool HasPGAOGAData
		{
			get
			{
				return HasACEFDAs
					|| HasAMSData
					|| HasAPHISLines
					|| HasATFLines
					|| HasCPSCLines
					|| HasDEAHeaders
					|| HasLaceyActData
					|| HasNHTSALines
					|| HasOMCHeaders
					|| HasPSTLines
					|| HasHFCHeaders
					|| HasTTBData
					|| HasVNELines;
			}
		}

		void RemovePGAOGAData()
		{
			DeleteAllAndRefreshBinding(PGAs);
			DeleteAllAndRefreshBinding(ACEFDAs);
			DeleteAllAndRefreshBinding(NHTSALines);
			DeleteAllAndRefreshBinding(PSTLines);
			DeleteAllAndRefreshBinding(HFCHeaders);
			DeleteAllAndRefreshBinding(VehicleLines);
			DeleteAllAndRefreshBinding(ATFLines);
			DeleteAllAndRefreshBinding(OMCHeaders);
			DeleteAllAndRefreshBinding(TTBLines);
			DeleteAllAndRefreshBinding(CPSCLines);
			DeleteAllAndRefreshBinding(DEAHeaders);
			DeleteAllAndRefreshBinding(APHISHeaders);
			DeleteAllAndRefreshBinding(AMSLines);
		}

		void DeleteAllAndRefreshBinding(BusinessObjectCollection collection)
		{
			collection.RemoveAndDeleteAll();
			collection.RefreshBinding();
		}

		#region NMFS Lines

		[ChildEditable(true)]
		public NMFSLineCollection NMFSLines
		{
			get
			{
				if (nmfsLines == null)
				{
					nmfsLines = new NMFSLineCollection(this);
					nmfsLines.Load();
					RegisterEditableChildObject(nmfsLines);
				}
				return nmfsLines;
			}
		}
		NMFSLineCollection nmfsLines;

		public IEnumerable<NMFSLine> NMFS370Lines
		{
			get { return NMFSLines.OfType<NMFSLine>().Where(x => x.Is370ProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSCOALines
		{
			get { return NMFSLines.OfType<NMFSLine>().Where(x => x.IsCOAProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSAMRLines
		{
			get { return NMFSLines.OfType<NMFSLine>().Where(x => x.IsAMRProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSHMSLines
		{
			get { return NMFSLines.OfType<NMFSLine>().Where(x => x.IsHMSProgramType); }
		}

		public IEnumerable<NMFSLine> NMFSSIMPLines
		{
			get { return NMFSLines.OfType<NMFSLine>().Where(x => x.IsSIMProgramType); }
		}

		public ZBool HasNMFSLines
		{
			get { return NMFSLines.Count > 0; }
		}

		#endregion

		public bool UseHTSClassification
		{
			get { return !UseSCHBClassification; }
		}

		public bool UseSCHBClassification
		{
			get { return IsSHB; }
		}

		public ZString CalcMiscLicenseTypeLabel
		{
			get
			{
				USCTariff importTariff = new USCTariff.Loader(Factory).LoadBestMatch(TariffNumber, EffectiveDate);
				return importTariff == null ? new ZString("Misc. License No.:") : importTariff.MiscLicenseTypeLabel;
			}
		}

		public USCACCase AntidumpingDutyCase
		{
			get { return Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, CD_ADDCaseNo); }
		}

		public USCACCase CountervailingDutyCase
		{
			get { return Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, CD_CVDCaseNo); }
		}

		internal USCACCaseRate GetUSCACCaseRate(USCACCase uscCase)
		{
			return uscCase?.GetDepositRate(ZDate.Today);
		}

		bool CD_ADDDepositRateDescription_ReadOnly
		{
			get { return !DepositRateIndicatorList.DepositRateIndContainOverride(CD_ADDDepositRateInd); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(CD_ADDDepositRateDescription_ReadOnly))]
		public ZString CD_ADDDepositRateDescription
		{
			get { return DepositRateIndicatorList.GetDepositRateDescription(CD_ADDDepositRateInd, USClassificationLookups.AntidumpingDutyDepositRates, CD_ADDDepositRateOverride, USClassificationLookups.AntidumpingUSCACCaseRate); }
			set
			{
				DepositRateIndicatorList.SetDepositRateDescription(value, CD_ADDDepositRateInd, CD_ADDDepositRateOverrideInfo);
				CD_ADDDepositRateDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CD_ADDDepositRateDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CD_ADDDepositRateDescription); }
		}

		public override ZString CD_ADDDepositRateInd
		{
			get => base.CD_ADDDepositRateInd;
			set
			{
				var oldValue = CD_ADDDepositRateInd;
				base.CD_ADDDepositRateInd = value;
				if (oldValue != CD_ADDDepositRateInd)
				{
					CD_ADDDepositRateOverride = ZDecimal.Zero;
					CD_ADDDepositRateDescriptionInfo.RefreshBinding();
				}
			}
		}

		bool CD_CVDDepositRateDescription_ReadOnly
		{
			get { return !DepositRateIndicatorList.DepositRateIndContainOverride(CD_CVDDepositRateInd); }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(CD_CVDDepositRateDescription_ReadOnly))]
		public ZString CD_CVDDepositRateDescription
		{
			get { return DepositRateIndicatorList.GetDepositRateDescription(CD_CVDDepositRateInd, USClassificationLookups.CountervailingDutyDepositRates, CD_CVDDepositRateOverride, USClassificationLookups.CountervailingUSCACCaseRate); }
			set
			{
				DepositRateIndicatorList.SetDepositRateDescription(value, CD_CVDDepositRateInd, CD_CVDDepositRateOverrideInfo);
				CD_CVDDepositRateDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CD_CVDDepositRateDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CD_CVDDepositRateDescription); }
		}

		public override ZString CD_CVDDepositRateInd
		{
			get => base.CD_CVDDepositRateInd;
			set
			{
				var oldValue = CD_CVDDepositRateInd;
				base.CD_CVDDepositRateInd = value;
				if (oldValue != CD_CVDDepositRateInd)
				{
					CD_CVDDepositRateOverride = ZDecimal.Zero;
					CD_CVDDepositRateDescriptionInfo.RefreshBinding();
				}
			}
		}

		#region Properties for Module Grid Binding

		internal ZString TariffDescription
		{
			get
			{
				(ZString tariffType, ZString tariffCode) = GetTariffData();
				var tariff = Factory.GetTariff(tariffType, tariffCode, EffectiveDate);
				return tariff?.Description ?? ZString.Empty;
			}
		}

		(ZString tariffType, ZString tariffCode) GetTariffData()
		{
			return (GetTariffType(), Classification?.CC_TariffNum ?? CI_TariffNum);
		}

		ZString GetTariffType()
		{
			switch (CI_ChildType)
			{
				case ClassificationTypeList.Codes.HTE:
					return Universal.Constants.TariffTypes.Export;
				case ClassificationTypeList.Codes.SHB:
					return Universal.Constants.TariffTypes.ScheduleB;
				default:
					return Universal.Constants.TariffTypes.HarmonizedSystem;
			}
		}

		#endregion

		[ChildEditable(true)]
		public new CusClassPartPivotCollection Children
		{
			get { return (CusClassPartPivotCollection)base.Children; }
		}

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetCusClassPartPivotCollection()
		{
			if (children == null)
			{
				children = new CusClassPartPivotCollection(this);
				children.Load();
				RegisterEditableChildObject(children);
			}
			return children;
		}
		CusClassPartPivotCollection children;

		protected override bool HasLoadedChildrenCore => children != null;

		public new CusAttributeFilterCollection Attributes1 => (CusAttributeFilterCollection)base.Attributes1;

		public new CusAttributeFilterCollection Attributes2 => (CusAttributeFilterCollection)base.Attributes2;

		public new CusAttributeFilterCollection Attributes3 => (CusAttributeFilterCollection)base.Attributes3;

		protected override Customs.Business.CusAttributeFilterCollection GetNewAttributeFilterCollection(ZString attributeType)
		{
			return new CusAttributeFilterCollection(this, attributeType);
		}

		#region IHaveAdditionalDataForBorderWise Members

		AdditionalDataForBorderWise IHaveAdditionalDataForBorderWise.GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise(UseSCHBClassification ? "E" : "I", ZDate.Today);
		}

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return Lookups.Tariffs.TypeOfElements; }
		}

		#endregion

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return new TariffFormatter();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			CusClassPartPivot result = (CusClassPartPivot)base.CloneInternal(args);
			using (result.GetValidationSuspender())
			{
				foreach (PGA pga in PGAs)
				{
					result.PGAs.Add((PGA)pga.Clone());
				}

				foreach (ACEFDA acefda in ACEFDAs)
				{
					result.ACEFDAs.Add((ACEFDA)acefda.Clone());
				}

				foreach (OMCHeader omc in OMCHeaders)
				{
					result.OMCHeaders.Add((OMCHeader)omc.Clone());
				}

				foreach (TTBLine ttb in TTBLines)
				{
					result.TTBLines.Add((TTBLine)ttb.Clone());
				}

				foreach (NHTSAHeader nhtsa in NHTSALines)
				{
					result.NHTSALines.Add((NHTSAHeader)nhtsa.Clone());
				}

				foreach (CPSCHeader cpsc in CPSCLines)
				{
					result.CPSCLines.Add((CPSCHeader)cpsc.Clone());
				}

				foreach (Vehicle vne in VehicleLines)
				{
					result.VehicleLines.Add((Vehicle)vne.Clone());
				}

				foreach (Pesticide pst in PSTLines)
				{
					result.PSTLines.Add((Pesticide)pst.Clone());
				}

				foreach (USHFCHeader hfc in HFCHeaders)
				{
					result.HFCHeaders.Add((USHFCHeader)hfc.Clone());
				}

				foreach (DEAHeader dea in DEAHeaders)
				{
					result.DEAHeaders.Add((DEAHeader)dea.Clone());
				}

				foreach (APHISHeader aphis in APHISHeaders)
				{
					var newaphis = (APHISHeader)aphis.Clone();
					result.APHISHeaders.Add(newaphis);

					foreach (APHISProduct product in aphis.Products)
					{
						newaphis.Products.Add((APHISProduct)product.Clone());
					}

					foreach (APHISInspection inspection in aphis.Inspections)
					{
						newaphis.Inspections.Add((APHISInspection)inspection.Clone());
					}

					foreach (APHISLicense license in aphis.Licenses)
					{
						newaphis.Licenses.Add((APHISLicense)license.Clone());
					}
				}

				foreach (ATF atf in ATFLines)
				{
					result.ATFLines.Add((ATF)atf.Clone());
				}

				foreach (AMS existLineAMS in AMSLines)
				{
					AMS newAMS = result.AMSLines.AddNew();
					existLineAMS.UpdateAddInfoProperties();
					newAMS.B7_AddInfoData = existLineAMS.B7_AddInfoData;
					newAMS.UpdateAddInfoProperties();

					foreach (AMSLine amsLine in existLineAMS.AMSLines)
					{
						AMSLine newAMSLine = newAMS.AMSLines.AddNew();
						amsLine.Data.UpdateRelatedPropertyInfo();
						newAMSLine.B7_AddInfoData = amsLine.B7_AddInfoData;
						newAMSLine.Data.UpdateRelatedPropertyInfo();
					}
				}

				foreach (FWSHeader fwsLine in FWSLines)
				{
					result.FWSLines.Add((FWSHeader)fwsLine.Clone());
				}

				foreach (NMFSLine nmfsLine in NMFSLines)
				{
					result.NMFSLines.Add((NMFSLine)nmfsLine.Clone());
				}

				foreach (CusClassPartPivot childPivot in Children)
				{
					CusClassPartPivot childPivotCloned = (CusClassPartPivot)childPivot.Clone(args);
					result.Children.Add(childPivotCloned);
				}

				result.Details.CopyPersistentValuesFrom(Details, new BusinessObjectCloneArgs(new[] { CusUSClassification.Schema.CD_ParentID }));
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CI_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
		}

		protected override ZString DefaultChildType => ZString.Empty;

		#region AddInfo Wrapper Properties

		#region CD_ProductExclusion

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoCusClassPartPivotLookups.ProductExclusionList))]
		public virtual ZString CD_ProductExclusion
		{
			get => AddInfo.US_ProductExclusion;
			set
			{
				if (CD_ProductExclusion != value)
				{
					AddInfo.US_ProductExclusion = value;
					CD_ProductExclusionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CD_ProductExclusionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CD_ProductExclusion), x => AddInfo.US_ProductExclusionInfo); }
		}

		#endregion

		#region CD_ExclusionNumber

		public virtual ZString CD_ExclusionNumber
		{
			get => AddInfo.US_ExclusionNumber;
			set
			{
				if (CD_ExclusionNumber != value)
				{
					AddInfo.US_ExclusionNumber = value;
					CD_ExclusionNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CD_ExclusionNumberInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CD_ExclusionNumber), x => AddInfo.US_ExclusionNumberInfo); }
		}

		#endregion

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
			result.Add(CusAddInfoTypeAttribute.Codes.USFDA, typeof(FDA));
			result.Add(CusAddInfoTypeAttribute.Codes.USFCC, typeof(FCC));
			result.Add(CusAddInfoTypeAttribute.Codes.USDOT, typeof(DOT));
			result.Add(CusAddInfoTypeAttribute.Codes.USPGACommon, typeof(PGA));
			result.Add(CusAddInfoTypeAttribute.Codes.USACEFDA, typeof(ACEFDA));
			result.Add(CusAddInfoTypeAttribute.Codes.USATF, typeof(ATF));
			result.Add(CusAddInfoTypeAttribute.Codes.USAMS, typeof(AMS));
			result.Add(CusAddInfoTypeAttribute.Codes.USTTBLine, typeof(TTBLine));
			result.Add(CusAddInfoTypeAttribute.Codes.USNMFSLine, typeof(NMFSLine));
			result.Add(CusAddInfoTypeAttribute.Codes.USPesticide, typeof(Pesticide));
			result.Add(CusAddInfoTypeAttribute.Codes.USPGAVehicle, typeof(Vehicle));
			result.Add(CusAddInfoTypeAttribute.Codes.USNHTSAHeader, typeof(NHTSAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISHeader, typeof(APHISHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USOMCHeader, typeof(OMCHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USFWSHeader, typeof(FWSHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USCPSCHeader, typeof(CPSCHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.USDEAHeader, typeof(DEAHeader));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.CensusWarningOverride, typeof(CensusWarningOverride));
			return result;
		}

		#endregion

		#region PGAAgencyRequirementProvider

		public OGAAgencyRequirementCollection OGAAgencyRequirements
		{
			get
			{
				if (ogaAgencyRequirements == null)
				{
					ogaAgencyRequirements = new OGAAgencyRequirementCollection(new ProductPGAgencyRequirementProvider(this));
					ogaAgencyRequirements.Populate();
				}

				return ogaAgencyRequirements;
			}
		}
		OGAAgencyRequirementCollection ogaAgencyRequirements;

		public ExportPGAAgencyRequirementCollection ExportPGAAgencyRequirements
		{
			get
			{
				if (exportPGAAgencyRequirements == null)
				{
					exportPGAAgencyRequirements = new ExportPGAAgencyRequirementCollection(new ExportPGAProductRequirementsProvider(this));
					exportPGAAgencyRequirements.Populate();
				}
				return exportPGAAgencyRequirements;
			}
		}
		ExportPGAAgencyRequirementCollection exportPGAAgencyRequirements;

		public Func<ZString, ZBool, ZBool> OnExportPGAIndicatorChangedEvent;

		#region CalculatorObjects

		public OGARequirementCalculator OGARequirementCalculator
		{
			get
			{
				if (fOGARequirementCalculator == null)
				{
					fOGARequirementCalculator = new OGARequirementCalculator(Factory, () => ImportTariff, () => ImportSupTariff, () => EffectiveDate, () => CD_UC_NKCountryOfOrigin);
				}
				return fOGARequirementCalculator;
			}
		}
		OGARequirementCalculator fOGARequirementCalculator;

		#endregion

		#region CD_ATFIndicator

		public override ZString CD_ATFIndicator
		{
			get { return base.CD_ATFIndicator; }
			set
			{
				var oldValue = base.CD_ATFIndicator;
				var shouldUpdateIndicator = oldValue != value;

				if (shouldUpdateIndicator && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.ATF, ExportATF.HasExportData);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.CD_ATFIndicator = value;

					if (!IsCopying && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(value))
					{
						ATFLines.RemoveAndDeleteAll();
						fExportATF = null;
					}
				}
			}
		}

		#endregion

		#region DEA

		public override ZString CD_DEAIndicator
		{
			get { return base.CD_DEAIndicator; }
			set
			{
				var oldValue = base.CD_DEAIndicator;
				var shouldUpdateIndicator = oldValue != value;

				if (shouldUpdateIndicator && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.DEA, HasDEAHeaders);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.CD_DEAIndicator = value;

					if (!IsCopying && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(CD_DEAIndicator))
					{
						DEAHeaders.RemoveAndDeleteAll();
					}
				}
			}
		}

		#endregion

		#region CD_TTBIndicator

		public override ZString CD_TTBIndicator
		{
			get { return base.CD_TTBIndicator; }
			set
			{
				var oldValue = base.CD_TTBIndicator;
				var shouldUpdateIndicator = oldValue != value;

				if (shouldUpdateIndicator && IsExportTariff && !OGAIndicatorList.IsToBeDeclaredOrDisclaimed(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.TTB, HasTTBData);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.CD_TTBIndicator = value;

					if (!IsCopying && IsExportTariff && !OGAIndicatorList.IsToBeDeclaredOrDisclaimed(CD_TTBIndicator))
					{
						TTBLines.RemoveAndDeleteAll();
					}
				}
			}
		}

		#endregion

		#region DDTC
		public bool HasImportDDTCData
		{
			get { return OGAIndicatorList.IsToBeDeclared(CD_DDTCIndicator); }
		}

		#endregion

		#region  US_Requirement

		public ZString US_FDARequirement
		{
			get { return OGARequirementCalculator.FDARequirementDesc; }
		}

		public ZString US_DOTRequirement
		{
			get { return OGARequirementCalculator.DOTRequirementDesc; }
		}

		public ZString US_NOPRequirementDesc
		{
			get { return OGARequirementCalculator.NOPRequirementDesc; }
		}

		public ZString US_AMSRequirementDesc
		{
			get { return OGARequirementCalculator.AMSRequirementDesc; }
		}

		public ZString US_FWSRequirementDesc
		{
			get { return OGARequirementCalculator.FWSRequirementDesc; }
		}

		public ZString US_FSISRequirementDesc
		{
			get { return OGARequirementCalculator.FSISRequirementDesc; }
		}

		public ZString US_ODSRequirementDesc
		{
			get { return OGARequirementCalculator.ODSRequirementDesc; }
		}

		public ZString US_TSCARequirementDesc
		{
			get { return OGARequirementCalculator.TSCARequirementDesc; }
		}

		public ZString US_VNERequirementDesc
		{
			get { return OGARequirementCalculator.VNERequirementDesc; }
		}

		public ZString US_PSTRequirementDesc
		{
			get { return OGARequirementCalculator.PSTRequirementDesc; }
		}

		public ZString US_HFCRequirementDesc
		{
			get { return OGARequirementCalculator.HFCRequirementDesc; }
		}

		public ZString US_LaceyRequirementDesc
		{
			get { return OGARequirementCalculator.ACELaceyRequirementDesc; }
		}

		public ZString US_APHISRequirementDesc
		{
			get { return OGARequirementCalculator.APHISRequirementDesc; }
		}

		public ZString US_NMFS370RequirementDesc
		{
			get { return OGARequirementCalculator.NMFS370RequirementDesc; }
		}

		public ZString US_NMFSCOARequirementDesc
		{
			get { return OGARequirementCalculator.NMFSCOARequirementDesc; }
		}

		public ZString US_NMFSAMRRequirementDesc
		{
			get { return OGARequirementCalculator.NMFSAMRRequirementDesc; }
		}

		public ZString US_NMFSHMSRequirementDesc
		{
			get { return OGARequirementCalculator.NMFSHMSRequirementDesc; }
		}

		public ZString US_NMFSSIMPRequirementDesc
		{
			get { return OGARequirementCalculator.NMFSSIMRequirementDesc; }
		}

		public ZString US_NHTSARequirementDesc
		{
			get { return OGARequirementCalculator.NHTSARequirementDesc; }
		}

		public ZString US_TTBRequirementDesc
		{
			get { return OGARequirementCalculator.TTBRequirementDesc; }
		}

		public ZString US_OMCRequirementDesc
		{
			get { return OGARequirementCalculator.OMCRequirementDesc; }
		}

		public ZString US_FDARequirementDesc
		{
			get { return OGARequirementCalculator.FDARequirementDesc; }
		}

		public ZString US_ACEFDARequirementDesc
		{
			get { return OGARequirementCalculator.ACEFDARequirementDesc; }
		}

		internal ZString US_FDARequirementCode
		{
			get { return OGARequirementCalculator.FDARequirementCode; }
		}

		internal ZString US_NOPRequirementCode
		{
			get { return OGARequirementCalculator.NOPRequirementCode; }
		}

		internal ZString US_CPSCRequirementDesc
		{
			get { return OGARequirementCalculator.CPSCRequirementDesc; }
		}

		internal ZString US_DEARequirementDesc
		{
			get { return OGARequirementCalculator.DEARequirementDesc; }
		}

		#endregion

		#region ITariffProvider

		ZString ITariffProvider.Tariff
		{
			get { return Classification != null ? Classification.CC_TariffNum : CI_TariffNum; }
		}

		ZPropertyInfo ITariffProvider.TariffInfo
		{
			get { return Classification != null ? Classification.CC_TariffNumInfo : CI_TariffNumInfo; }
		}

		#endregion

		#region CD_AMSIndicator

		public override ZString CD_AMSIndicator
		{
			get { return base.CD_AMSIndicator; }
			set
			{
				var oldValue = base.CD_AMSIndicator;
				var shouldUpdateIndicator = oldValue != value;

				if (shouldUpdateIndicator && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.AMS, !CD_ExportCertificateNo.IsEmpty);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.CD_AMSIndicator = value;

					if (!IsCopying && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(CD_AMSIndicator))
					{
						CD_ExportCertificateNo = ZString.Empty;
					}

					if (!OGAIndicatorList.IsToBeDisclaimed(CD_AMSIndicator))
					{
						CD_AMSDisclaimProgram = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region CD_AMSDisclaimProgram

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.AMSDisclaimProgramList))]
		[ReadOnlyMember(nameof(CD_AMSDisclaimProgram_ReadOnly))]
		public override ZString CD_AMSDisclaimProgram
		{
			get { return base.CD_AMSDisclaimProgram; }
			set { base.CD_AMSDisclaimProgram = value; }
		}

		bool CD_AMSDisclaimProgram_ReadOnly
		{
			get { return CD_AMSIndicator != OGAIndicatorList.Codes.Disclaimed; }
		}

		#endregion

		#region CD_PSTIndicator

		public override ZString CD_PSTIndicator
		{
			get { return base.CD_PSTIndicator; }
			set
			{
				var oldValue = base.CD_PSTIndicator;
				var shouldUpdateIndicator = oldValue != value;

				if (shouldUpdateIndicator && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						var hasEPAData = !CD_EPAConsentNumber.IsEmpty || !CD_HazWasteTrackingNo.IsEmpty || !CD_EPANetQty.IsEmpty || !CD_EPANetQtyUQ.IsEmpty;
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.EPA, hasEPAData);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.CD_PSTIndicator = value;

					if (!IsCopying && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(CD_PSTIndicator))
					{
						CD_EPAConsentNumber = ZString.Empty;
						CD_HazWasteTrackingNo = ZString.Empty;
						CD_EPANetQty = ZDecimal.Zero;
						CD_EPANetQtyUQ = ZString.Empty;
					}

					if (!OGAIndicatorList.IsToBeDisclaimed(CD_PSTIndicator))
					{
						CD_PSTDisclaimProgram = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region CD_PSTDisclaimProgram

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.PSTDisclaimProgramList))]
		[ReadOnlyMember(nameof(CD_PSTDisclaimProgram_ReadOnly))]
		public override ZString CD_PSTDisclaimProgram
		{
			get { return base.CD_PSTDisclaimProgram; }
			set { base.CD_PSTDisclaimProgram = value; }
		}

		bool CD_PSTDisclaimProgram_ReadOnly
		{
			get { return CD_PSTIndicator != OGAIndicatorList.Codes.Disclaimed; }
		}

		#endregion

		#region CD_NMFSHMSIndicator

		public override ZString CD_NMFSHMSIndicator
		{
			get { return base.CD_NMFSHMSIndicator; }
			set
			{
				var oldValue = base.CD_NMFSHMSIndicator;
				var shouldUpdateIndicator = oldValue != value;

				if (shouldUpdateIndicator && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.NMFS, NMFSLines.Count > 0);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.CD_NMFSHMSIndicator = value;

					if (!IsCopying && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(CD_NMFSHMSIndicator))
					{
						NMFSLines.RemoveAndDeleteAll();
					}
				}
			}
		}

		#endregion

		#region CD_FWSIndicator

		[ReadOnlyMember(nameof(CD_FWSIndicator_ReadOnly))]
		public override ZString CD_FWSIndicator
		{
			get { return base.CD_FWSIndicator; }
			set
			{
				var oldValue = base.CD_FWSIndicator;
				var shouldUpdateIndicator = oldValue != value;

				if (shouldUpdateIndicator && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(value))
				{
					if (OnExportPGAIndicatorChangedEvent != null)
					{
						shouldUpdateIndicator = OnExportPGAIndicatorChangedEvent(GovernmentAgencyProgramCodeList.Codes.FWS, ExportFWS.HasExportData);
					}
				}

				if (shouldUpdateIndicator)
				{
					base.CD_FWSIndicator = value;

					if (!IsCopying && IsExportTariff && !OGAIndicatorList.IsToBeDeclared(CD_FWSIndicator))
					{
						FWSLines.RemoveAndDeleteAll();
						fExportFWS = null;
					}
				}
			}
		}

		bool CD_FWSIndicator_ReadOnly => IsExportTariff && CD_FWSIndicator.IsEmpty && !ZZCustomsFunctionality.IsEnableFWSEffective;

		public ZBool IsFWSDeclared
		{
			get { return OGAIndicatorList.IsToBeDeclared(CD_FWSIndicator); }
		}

		#endregion

		#endregion

		#region Export PGA Properties

		#region ExportATF

		public ATF ExportATF
		{
			get
			{
				if (fExportATF == null || fExportATF.IsNull || fExportATF.IsDeleted)
				{
					if (OGAIndicatorList.IsToBeDeclared(CD_ATFIndicator))
					{
						fExportATF = HasATFLines ? ATFLines.OfType<ATF>().FirstOrDefault() : ATFLines.AddNew();
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

		public FWSHeader ExportFWS
		{
			get
			{
				if (fExportFWS == null || fExportFWS.IsNull || fExportFWS.IsDeleted)
				{
					if (OGAIndicatorList.IsToBeDeclared(CD_FWSIndicator))
					{
						fExportFWS = HasFWSLines ? FWSLines.OfType<FWSHeader>().FirstOrDefault() : FWSLines.AddNew();
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

		#region CD_EPAConsentNumber

		[MaxLength(12)]
		public override ZString CD_EPAConsentNumber
		{
			get { return base.CD_EPAConsentNumber; }
			set { base.CD_EPAConsentNumber = value; }
		}

		#endregion

		#region CD_EPANetQty

		[MaxLength(10)]
		public override ZDecimal CD_EPANetQty
		{
			get { return base.CD_EPANetQty; }
			set { base.CD_EPANetQty = value; }
		}

		#endregion

		#region CD_EPANetQtyUQ

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.EPANetQtyUQList))]
		[MaxLength(2)]
		public override ZString CD_EPANetQtyUQ
		{
			get { return base.CD_EPANetQtyUQ; }
			set { base.CD_EPANetQtyUQ = value; }
		}

		#endregion

		#region CD_ExportCertificateNo

		[MaxLength(13)]
		public override ZString CD_ExportCertificateNo
		{
			get { return base.CD_ExportCertificateNo; }
			set { base.CD_ExportCertificateNo = value; }
		}

		#endregion

		#region CD_HazWasteTrackingNo

		[MaxLength(12)]
		public override ZString CD_HazWasteTrackingNo
		{
			get { return base.CD_HazWasteTrackingNo; }
			set { base.CD_HazWasteTrackingNo = value; }
		}

		#endregion

		#endregion

		#region AddInfo/Validation/Lookups objects

		public AddInfoCusClassPartPivotLookups AddInfoLookups => AddInfo.Lookups;

		public AddInfoCusClassPartPivotValidation AddInfoValidation => AddInfo.Validation;

		public AddInfoCusClassPartPivot AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoCusClassPartPivot(CI_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}

				return fAddInfo;
			}
		}
		AddInfoCusClassPartPivot fAddInfo;

		IAddInfo IAddInfoManager.AddInfo => AddInfo;

		#endregion

		#region Additional Tariffs Properties

		#region Additional Tariff 1

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsSupplementalTariffReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.CusClassPartPivot|SupFormattedAdditionalTariff1", ShortCaption = "Prov Add. Tariff 1", Caption = "Prov/Prog. Additional Tariff 1")]
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
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff1();
				}

				SupFormattedAdditionalTariff1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff1Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff1));

		public ZString CI_SupAdditionalTariff1 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff1)?.BZ_Tariff ?? ZString.Empty;

		#endregion

		#region Additional Tariff 2

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsSupplementalTariffReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.CusClassPartPivot|SupFormattedAdditionalTariff2", ShortCaption = "Prov Add. Tariff 2", Caption = "Prov/Prog. Additional Tariff 2")]
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
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff2();
				}

				SupFormattedAdditionalTariff2Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff2Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff2));

		public ZString CI_SupAdditionalTariff2 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff2)?.BZ_Tariff ?? ZString.Empty;

		#endregion

		#region Additional Tariff 3

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsSupplementalTariffReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.CusClassPartPivot|SupFormattedAdditionalTariff3", ShortCaption = "Prov Add. Tariff 3", Caption = "Prov/Prog. Additional Tariff 3")]
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
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff3();
				}

				SupFormattedAdditionalTariff3Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff3Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff3));

		public ZString CI_SupAdditionalTariff3 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff3)?.BZ_Tariff ?? ZString.Empty;

		#endregion

		#region Additional Tariff 4

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsSupplementalTariffReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.CusClassPartPivot|SupFormattedAdditionalTariff4", ShortCaption = "Prov Add. Tariff 4", Caption = "Prov/Prog. Additional Tariff 4")]
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
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff4();
				}

				SupFormattedAdditionalTariff4Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff4Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff4));

		public ZString CI_SupAdditionalTariff4 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff4)?.BZ_Tariff ?? ZString.Empty;

		#endregion

		#region Additional Tariff 5

		[MaxLength(15)]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsSupplementalTariffReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.CusClassPartPivot|SupFormattedAdditionalTariff5", ShortCaption = "Prov Add. Tariff 5", Caption = "Prov/Prog. Additional Tariff 5")]
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
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateSupFormattedAdditionalTariff5();
				}

				SupFormattedAdditionalTariff5Info.RefreshBinding();
			}
		}

		public ZPropertyInfo SupFormattedAdditionalTariff5Info => GetZPropertyInfo(nameof(SupFormattedAdditionalTariff5));

		public ZString CI_SupAdditionalTariff5 => LoadTariffDetailByType(CusLineTariffTypeList.Codes.AdditionalTariff5)?.BZ_Tariff ?? ZString.Empty;

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
	}
}
