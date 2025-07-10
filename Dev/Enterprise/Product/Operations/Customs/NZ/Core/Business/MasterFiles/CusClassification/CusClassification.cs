using System;
using System.Collections;
using System.Data;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MasterFiles
{
	/// <summary>
	/// NZ version of CusClassification
	/// </summary>
	public class CusClassification : AutoNZCusClassification
		, IHaveAdditionalDataForBorderWise
		, ITariffValidationData
		, Integration.Customs.NZ.ICusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Validation
		protected override Customs.Business.CusClassificationValidation GetNewValidation()
		{
			return new CusClassificationValidation(this);
		}

		public new CusClassificationValidation Validation
		{
			get { return (CusClassificationValidation)base.Validation; }
		}
		#endregion

		#region GetAdditionalDataForBorderWise

		public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty)
		{
			GlbDepartment dept = GlbDepartment.CurrentDepartment;
			ZString impExp = "I";
			if (dept != null && dept.GE_Direction == "Export")
			{
				impExp = "E";
			}
			return new AdditionalDataForBorderWise(impExp, DateForDutyRate);
		}

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return ClassificationList.TypeOfElements; }
		}

		#endregion

		public new class Schema : AutoNZCusClassification.Schema
		{
			public const string CC_AggregatedPermitCodes = "CC_AggregatedPermitCodes";
			public const string CC_AggregatedProhibitedCodes = "CC_AggregatedProhibitedCodes";
			public const string CC_AggregatedOtherInfoCodes = "CC_AggregatedOtherInfoCodes";
		}

		#region AddInfo Property Accessors

		[List(nameof(ConcessionList))]
		[ResourceStringData("NZCusClassification|96D7CEE4-8F4F-42B3-A599-A93C5008EB43", Caption = "Concession")]
		public override ZString CC_ConcessionCode
		{
			get { return base.CC_ConcessionCode; }
			set { base.CC_ConcessionCode = value; }
		}

		[BusinessObjectTestExclude]
		public override ZString CC_TariffNum
		{
			get => Factory.GetCachedValue(base.CC_TariffNum, () => TariffFormatter.Format(base.CC_TariffNum));
			set => base.CC_TariffNum = value;
		}

		protected override ZString FormatTariffForSaving(ZString unformattedTariff) => TariffFormatter.FormatDotted(unformattedTariff);

		[BusinessObjectTestExclude]
		[ResourceStringData("NZCusClassification|0A81DA52-2824-4902-95F9-50867C3070CC", Caption = "Parts Of Classification")]
		public override ZString CC_PartsOfClassification
		{
			get { return Factory.GetCachedValue(base.CC_PartsOfClassification, () => TariffFormatter.Format(base.CC_PartsOfClassification)); }
			set { base.CC_PartsOfClassification = TariffFormatter.FormatDotted(value); }
		}

		public ITariff PartsOfClassification
		{
			get
			{
				var dateForDutyRate = ZDateTime.Today;
				var partsOfClassification = UniversalTariffHelper.UseRefDatabaseData ? CC_PartsOfClassification.Replace(".", ZString.Empty) : CC_PartsOfClassification;
				return UniversalTariffHelper.GetTariff(Factory, partsOfClassification, dateForDutyRate);
			}
		}

		#endregion

		public ITariff Tariff
		{
			get { return UniversalTariffHelper.UseRefDatabaseData ? UniversalTariffHelper.GetTariff(Factory, CC_TariffNum, ZDateTime.Today) : NZCClassification.GetClassForCompleteCode(Factory, CC_TariffNum); }
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return TariffFormatter;
		}

		protected NZTariffFormatter TariffFormatter
		{
			get { return fTariffFormatter ?? (fTariffFormatter = new NZTariffFormatter()); }
		}
		NZTariffFormatter fTariffFormatter;

		protected override void OnTariffSet(ZString oldTariff)
		{
			base.OnTariffSet(oldTariff);

			var newDescription = UniversalTariffHelper.GetDescription(Factory, CC_TariffNum).Left(CC_DescriptionInfo.MaxLength);
			if (!CC_Description.IsEmpty && (!newDescription.IsEmpty || CC_TariffNum.IsEmpty))
			{
				var oldTariffDescription = UniversalTariffHelper.GetDescription(Factory, oldTariff);
				if (CC_Description == oldTariffDescription.Left(CC_DescriptionInfo.MaxLength))
				{
					CC_Description = ZString.Empty;
				}
			}

			if (CC_Description.IsEmpty)
			{
				CC_Description = newDescription;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			using (SuspendSettingHasChanges())
			{
				CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				CC_ClassificationType = ClassificationType.Both;
			}
		}

		#region New Properties

		[SuppressWeaklyTypedCollectionMessage]
		public IList ConcessionList
		{
			get
			{
				return UniversalTariffHelper.GetConcessionList(Factory, CC_TariffNum, DateForDutyRate);
			}
		}

		public NonDependentNZCClassificationCollection ClassificationList
		{
			get { return new NonDependentNZCClassificationCollection(Factory); }
		}

		[ChildEditable]
		public LineOtherInfoCollection OtherInfos
		{
			get
			{
				if (fOtherInfos == null)
				{
					fOtherInfos = new LineOtherInfoCollection(Factory, CC_OtherInfosInfo);
					fOtherInfos.LoadFromString(CC_OtherInfos);
					RegisterEditableChildObject(fOtherInfos);
				}
				return fOtherInfos;
			}
		}
		LineOtherInfoCollection fOtherInfos;

		[ChildEditable]
		public PermitCodeCollection PermitCodes
		{
			get
			{
				if (fPermitCodes == null)
				{
					fPermitCodes = new PermitCodeCollection(Factory, CC_PermitCodesInfo);
					fPermitCodes.LoadFromString(CC_PermitCodes);
					RegisterEditableChildObject(fPermitCodes);
				}
				return fPermitCodes;
			}
		}
		PermitCodeCollection fPermitCodes;

		[ChildEditable]
		public ProhibitedCodeCollection ProhibitedCodes
		{
			get
			{
				if (fProhibitedCodes == null)
				{
					fProhibitedCodes = new ProhibitedCodeCollection(Factory, CC_ProhibitedCodesInfo);
					fProhibitedCodes.LoadFromString(CC_ProhibitedCodes);
					RegisterEditableChildObject(fProhibitedCodes);
				}
				return fProhibitedCodes;
			}
		}
		ProhibitedCodeCollection fProhibitedCodes;

		Integration.Customs.NZ.ICodeDataPairCollection Integration.Customs.NZ.ICusClassification.PermitCodes => PermitCodes;

		Integration.Customs.NZ.ICodeDataPairCollection Integration.Customs.NZ.ICusClassification.ProhibitedCodes => ProhibitedCodes;

		Integration.Customs.NZ.ICodeDataPairCollection Integration.Customs.NZ.ICusClassification.OtherInfos => OtherInfos;

		#region ReadOnly Aggregated Code List Properties for module grid
		public ZString CC_AggregatedPermitCodes
		{
			get { return PermitCodes.AggregatedCodes; }
		}

		public ZPropertyInfo CC_AggregatedPermitCodesInfo
		{
			get { return GetZPropertyInfo(Schema.CC_AggregatedPermitCodes); }
		}

		public ZString CC_AggregatedProhibitedCodes
		{
			get { return ProhibitedCodes.AggregatedCodes; }
		}

		public ZPropertyInfo CC_AggregatedProhibitedCodesInfo
		{
			get { return GetZPropertyInfo(Schema.CC_AggregatedProhibitedCodes); }
		}

		public ZString CC_AggregatedOtherInfoCodes
		{
			get { return OtherInfos.AggregatedCodes; }
		}

		public ZPropertyInfo CC_AggregatedOtherInfoCodesInfo
		{
			get { return GetZPropertyInfo(Schema.CC_AggregatedOtherInfoCodes); }
		}
		#endregion

		#endregion

		#region DateForDutyRate
		public ZDateTime DateForDutyRate
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

		#region ITariffValidationData Members
		ZString ITariffValidationData.TariffCode
		{
			get { return CC_TariffNum; }
		}

		ZPropertyInfo ITariffValidationData.TariffCodeInfo
		{
			get { return CC_TariffNumInfo; }
		}

		ITariff ITariffValidationData.TariffBO
		{
			get { return Tariff; }
		}

		ZString ITariffValidationData.PartsOfTariffCode
		{
			get { return CC_PartsOfClassification; }
		}

		ZPropertyInfo ITariffValidationData.PartsOfTariffCodeInfo
		{
			get { return CC_PartsOfClassificationInfo; }
		}

		ITariff ITariffValidationData.PartsOfTariffBO
		{
			get { return PartsOfClassification; }
		}

		AllowableTariffCodeTypes ITariffValidationData.AllowableTariffCodeTypes
		{
			get { return new AllowableTariffCodeTypes(true, true, true); }
		}

		int ITariffValidationData.PermitCodeCount
		{
			get { return PermitCodes.Count; }
		}

		bool ITariffValidationData.EmptyTariffIsFullError
		{
			get { return true; }
		}

		bool ITariffValidationData.EmptyTariffIsAllowed
		{
			get { return false; }
		}

		void ITariffValidationData.ValidateTariffCode()
		{
			Validation.ValidateCC_TariffNum();
		}

		void ITariffValidationData.ValidatePartsOfTariffCode()
		{
			Validation.ValidateCC_PartsOfClassification();
		}
		#endregion

		protected override ZString GetDutyRateForCurrentCountry()
		{
			DutyCalculator calculator = new DutyCalculator(new DutyRateParameters(CC_TariffNum, CC_ConcessionCode, CC_PartsOfClassification, Factory));
			return calculator.DutyRateOnly;
		}
	}
}
