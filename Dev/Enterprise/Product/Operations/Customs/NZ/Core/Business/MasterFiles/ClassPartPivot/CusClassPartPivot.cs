using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	public class CusClassPartPivot : BaseCusClassPartPivot,
		Integration.Customs.NZ.ICusClassPartPivot,
		IAddInfoManager,
		IHaveNZAddInfo,
		IHaveAdditionalDataForBorderWise,
		ITariffValidationData
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : BaseCusClassPartPivot.Schema
		{
			public const string CI_ConcessionCode = "CI_ConcessionCode";
			public const string CI_PartsOfClassification = "CI_PartsOfClassification";
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ConcessionList))]
		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		[ResourceStringData("NZCusClassPartPivot|B4A92DCC-61D7-46FF-B64B-7969ABE87328", Caption = "Concession")]
		public ZString CI_ConcessionCode
		{
			get { return AddInfo.ZN_ConcessionCode; }
			set { AddInfo.ZN_ConcessionCode = value; }
		}

		public ZPropertyInfo CI_ConcessionCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CI_ConcessionCode, x => AddInfo.ZN_ConcessionCodeInfo); }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		[ResourceStringData("NZCusClassPartPivot|83AEBDFE-2ACF-4A2D-818F-8E738E1D89EC", ShortCaption = "Parts Of Class.", Caption = "Parts Of Classification")]
		public ZString CI_PartsOfClassification
		{
			get { return Factory.GetCachedValue(AddInfo.ZN_PartsOfClassification, () => CurrentTariffFormatter.Format(AddInfo.ZN_PartsOfClassification)); }
			set { AddInfo.ZN_PartsOfClassification = ((NZTariffFormatter)CurrentTariffFormatter).FormatDotted(value); }
		}

		public ZPropertyInfo CI_PartsOfClassificationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CI_PartsOfClassification, x => AddInfo.ZN_PartsOfClassificationInfo); }
		}

		#endregion

		NZAddInfo fAddInfo;
		public NZAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new NZAddInfo(this, CI_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}

		#region CI_RN_NKCountryOfOrigin: Currently proxied from Addinfo, should move to real column

		[MaxLength(2)]
		public new ZString CI_RN_NKCountryOfOrigin
		{
			get { return AddInfo.ZN_RN_NKCountryOfOrigin; }
			set
			{
				bool hasChanges = AddInfo.ZN_RN_NKCountryOfOrigin != value;
				AddInfo.ZN_RN_NKCountryOfOrigin = value;
				if (hasChanges && Part != null)
				{
					Part.MarkAsNeedingValidation();
				}
				CI_RN_NKCountryOfOriginInfo.RefreshBinding();
			}
		}

		public new ZPropertyInfo CI_RN_NKCountryOfOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CI_RN_NKCountryOfOrigin, x => AddInfo.ZN_RN_NKCountryOfOriginInfo); }
		}

		#endregion

		#region Last Audit ReadOnly

		[ReadOnlyMember(nameof(CI_LastAuditedDateReadOnly))]
		public override ZDateTime CI_LastAuditedDate { get => base.CI_LastAuditedDate; set => base.CI_LastAuditedDate = value; }
		public bool CI_LastAuditedDateReadOnly => true;

		[ReadOnlyMember(nameof(CI_LastAuditedUserReadOnly))]
		public override ZString CI_LastAuditedUser { get => base.CI_LastAuditedUser; set => base.CI_LastAuditedUser = value; }
		public bool CI_LastAuditedUserReadOnly => true;

		#endregion

		#region overrides

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[ResourceStringData("NZCusClassPartPivot|721E8D99-11CF-42DB-BEB5-2E83A79ABB04", Caption = "Tariff", FullDescription = "Tariff Code")]
		public override ZString CI_TariffNum
		{
			get => Factory.GetCachedValue(base.CI_TariffNum, () => CurrentTariffFormatter.Format(base.CI_TariffNum));
			set => base.CI_TariffNum = value;
		}

		protected override ZString FormatTariffForSaving(ZString unformattedTariff) => ((NZTariffFormatter)CurrentTariffFormatter).FormatDotted(unformattedTariff);

		public override ZGuid CI_CC
		{
			get
			{
				return base.CI_CC;
			}
			set
			{
				var hasChanged = base.CI_CC != value;
				base.CI_CC = value;
				if (hasChanged)
				{
					UpdateCI_CCDependentValues();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CI_PartsOfClassification = ZString.Empty;
			CI_ConcessionCode = ZString.Empty;
			CI_TariffNum = ZString.Empty;
		}

		void UpdateCI_CCDependentValues()
		{
			if (!IsCopying && CI_CC.IsValid)
			{
				CI_PartsOfClassification = ZString.Empty;
				CI_ConcessionCode = ZString.Empty;
				CI_TariffNum = ZString.Empty;
			}
		}

		protected override TariffFormatter GetTariffFormatter()
		{
			return new NZTariffFormatter();
		}

		protected override ZString GetDutyRateForCurrentCountry()
		{
			string dutyRate = (string)Classification?.DutyRateForCurrentCountry;
			if (dutyRate == null)
			{
				var dutyRateParams = new DutyRateParameters(CI_TariffNum, CI_ConcessionCode, CI_PartsOfClassification, Factory);
				dutyRate = new DutyCalculator(dutyRateParams).DutyRateOnly;
			}
			return dutyRate ?? ZString.Empty;
		}

		protected override bool UseUniversalTariff => UniversalTariffHelper.UseRefDatabaseData;

		protected override ZString UniversalTariffType => Constants.TariffTypes.HarmonizedSystem;

		#endregion

		#region Validation

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		public new CusClassPartPivotValidation Validation
		{
			get { return (CusClassPartPivotValidation)GetNewValidation(); }
		}

		#endregion

		#region IAddInfoManager Members

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IHaveNZAddInfo Members

		NZAddInfo IHaveNZAddInfo.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IHaveAdditionalDataForBorderWise Members

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return Lookups.Tariffs.TypeOfElements; }
		}

		public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise(IsImport ? "I" : "E", ZDate.Today);
		}

		public ZBool IsImport => CI_ChildType == ClassificationTypeList.Codes.HTI;

		#endregion

		#region ITariffValidationData

		ZString ITariffValidationData.TariffCode => CI_TariffNum;

		ZPropertyInfo ITariffValidationData.TariffCodeInfo => CI_TariffNumInfo;

		ITariff ITariffValidationData.TariffBO => UniversalTariffHelper.UseRefDatabaseData ? UniversalTariff : NZCClassification.GetClassForCompleteCode(Factory, CI_TariffNum);

		AllowableTariffCodeTypes ITariffValidationData.AllowableTariffCodeTypes => new AllowableTariffCodeTypes(true, true, false);

		int ITariffValidationData.PermitCodeCount => PermitCodes.Count;

		bool ITariffValidationData.EmptyTariffIsFullError => true;

		bool ITariffValidationData.EmptyTariffIsAllowed => true;

		void ITariffValidationData.ValidateTariffCode()
		{
			Validation.ValidateCI_TariffNum();
		}

		ZString ITariffValidationData.PartsOfTariffCode => CI_PartsOfClassification;

		ZPropertyInfo ITariffValidationData.PartsOfTariffCodeInfo => CI_PartsOfClassificationInfo;

		ITariff ITariffValidationData.PartsOfTariffBO => AddInfo.PartsOfClassification;

		void ITariffValidationData.ValidatePartsOfTariffCode()
		{
			AddInfo.Validation.ValidateZN_PartsOfClassification();
		}

		ZDateTime ITariffValidationData.DateForDutyRate
		{
			get
			{
				if (fDateForDutyRate.IsEmpty)
				{
					fDateForDutyRate = ZDateTime.Today;
				}
				return fDateForDutyRate;
			}
		}
		ZDateTime fDateForDutyRate;

		#endregion

		#region Lookups

		public new CusClassPartPivotLookups Lookups
		{
			get { return new CusClassPartPivotLookups(this); }
		}

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		#endregion

		#region Collections From AddInfo

		public PermitCodeCollection PermitCodes
		{
			get { return AddInfo?.PermitCodes ?? new PermitCodeCollection(Factory, null); }
		}

		public ProhibitedCodeCollection ProhibitedCodes
		{
			get { return AddInfo?.ProhibitedCodes ?? new ProhibitedCodeCollection(Factory, null); }
		}

		public OtherInfoCollection OtherInfos
		{
			get { return AddInfo?.OtherInfos ?? new LineOtherInfoCollection(Factory, null); }
		}

		Integration.Customs.NZ.ICodeDataPairCollection Integration.Customs.NZ.ICusClassPartPivot.PermitCodes => PermitCodes;

		Integration.Customs.NZ.ICodeDataPairCollection Integration.Customs.NZ.ICusClassPartPivot.ProhibitedCodes => ProhibitedCodes;

		Integration.Customs.NZ.ICodeDataPairCollection Integration.Customs.NZ.ICusClassPartPivot.OtherInfos => OtherInfos;

		#endregion
	}
}
