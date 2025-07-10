using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusCondition : AutoRefCusCondition, ITariffEffectiveDatesRelatedBusinessObject, ITranslatableZZBusinessObject
	{
		public RefCusCondition(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_StartDate", Caption = "Start Date")]
		public override ZDateTime ZX1_StartDate { get => base.ZX1_StartDate; set => base.ZX1_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_EndDate", Caption = "End Date")]
		public override ZDateTime ZX1_EndDate { get => base.ZX1_EndDate; set => base.ZX1_EndDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_Source", Caption = "Source")]
		public override ZString ZX1_Source
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZX1_Source, RefCusConditionLanguageSchema.ZXJ_Source);
			set => base.ZX1_Source = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_Comment", Caption = "Comment")]
		public override ZString ZX1_Comment
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZX1_Comment, RefCusConditionLanguageSchema.ZXJ_Comment);
			set => base.ZX1_Comment = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_IsImport", Caption = "Is Import")]
		public override ZBool ZX1_IsImport { get => base.ZX1_IsImport; set => base.ZX1_IsImport = value; }

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_IsExport", Caption = "Is Export")]
		public override ZBool ZX1_IsExport { get => base.ZX1_IsExport; set => base.ZX1_IsExport = value; }

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_ConditionValueTrueMeansStop", Caption = "True Means Stop")]
		public override ZBool ZX1_ConditionValueTrueMeansStop { get => base.ZX1_ConditionValueTrueMeansStop; set => base.ZX1_ConditionValueTrueMeansStop = value; }

		[RelatedBusinessObject("CusTariff")]
		public override ZGuid ZX1_ZZ1_Tariff
		{
			get { return base.ZX1_ZZ1_Tariff; }
			set { base.ZX1_ZZ1_Tariff = value; }
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZX1_ZZ1_Tariff);

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping")]
		public override ZString ZX1_ZZZ_NKDataGrouping
		{
			get { return base.ZX1_ZZZ_NKDataGrouping; }
			set { base.ZX1_ZZZ_NKDataGrouping = value; }
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ZX1_Severity", Caption = "Severity")]
		public override ZString ZX1_Severity { get => base.ZX1_Severity; set => base.ZX1_Severity = value; }

		public override bool SupportsNotes => false;

		#region ConditionValues

		[ChildEditable]
		public RefCusConditionValueCollection ConditionValues
		{
			get
			{
				if (conditionValues == null)
				{
					conditionValues = new RefCusConditionValueCollection(this);
					RegisterEditableChildObject(conditionValues);
				}
				return conditionValues;
			}
		}
		RefCusConditionValueCollection conditionValues;

		#endregion

		#region RefCusConditionType

		[RelatedBusinessObject("CusConditionType")]
		public override ZGuid ZX1_ZX2_ConditionType
		{
			get { return base.ZX1_ZX2_ConditionType; }
			set { base.ZX1_ZX2_ConditionType = value; }
		}

		public RefCusConditionType CusConditionType => Factory.Load<RefCusConditionType>(ZX1_ZX2_ConditionType);

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ConditionType", Caption = "Condition Type")]
		public ZString ConditionType => CusConditionType?.ZX2_ConditionType ?? ZString.Empty;

		public ZString ConditionClass => CusConditionType?.ZX2_ConditionClass ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|ConditionTypeDescription", Caption = "Condition Type Description", ShortCaption = "Description")]
		public ZString ConditionTypeDescription => CusConditionType?.ZX2_Description ?? ZString.Empty;

		#endregion

		#region Applicabilities

		[ChildEditable]
		public CusRefApplicabilityViewCollection Applicabilities
		{
			get
			{
				if (applicabilities == null)
				{
					applicabilities = new CusRefApplicabilityViewCollection(this);
					RegisterEditableChildObject(applicabilities);
				}
				return applicabilities;
			}
		}

		CusRefApplicabilityViewCollection applicabilities;

		[ChildEditable]
		public FilteredCusRefApplicabilityViewCollection FilteredApplicabilities
		{
			get
			{
				if (filteredApplicabilities == null)
				{
					filteredApplicabilities = new FilteredCusRefApplicabilityViewCollection(this, CusTariff.IsParentDataGrouping);
					RegisterEditableChildObject(filteredApplicabilities);
				}
				return filteredApplicabilities;
			}
		}
		FilteredCusRefApplicabilityViewCollection filteredApplicabilities;

		#endregion

		#region ConditionChecks

		public ZBool ShouldStop(ConditionChecker.EvaluateConditionValue evaluationDelegate, IUniversalRateCalcData calcData = null) => ShouldStopIfConditionMet(evaluationDelegate, calcData);

		public ZBool IsInformationCondition => ConditionValues.Any(c => c.IsInformationConditionValue);

		ZBool ShouldStopIfConditionMet(ConditionChecker.EvaluateConditionValue evaluationDelegate, IUniversalRateCalcData calcData = null)
		{
			var conditionValuesExcludingInformation = ConditionValues.Where(x => !x.IsInformationConditionValue);
			var conditionMet = !conditionValuesExcludingInformation.Any() || conditionValuesExcludingInformation.GroupBy(x => x.ZX3_LogicalORWithinGroup).All(grouping => !grouping.Any() || grouping.Any(condValue => condValue.IsConditionMet(evaluationDelegate, calcData)));
			return ZX1_ConditionValueTrueMeansStop ? conditionMet : !conditionMet;
		}

		#endregion

		#region RefCusPreference

		public CusRefPreferenceView CusPreference => Factory.Load<CusRefPreferenceView>(ZX1_ZZS_Preference);

		[RelatedBusinessObject("CusPreference")]
		public override ZGuid ZX1_ZZS_Preference
		{
			get => base.ZX1_ZZS_Preference;
			set => base.ZX1_ZZS_Preference = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|PreferenceCode", Caption = "Preference")]
		public ZString PreferenceCode => CusPreference?.ZZS_Preference ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.Universal.RefCusCondition|PreferenceDescription", Caption = "Preference Description", ShortCaption = "Description")]
		public ZString PreferenceDescription => CusPreference?.ZZS_Description ?? ZString.Empty;

		#endregion

		#region ITariffEffectiveDatesRelatedBusinessObject Members

		ZString ITariffDataGroupingRelatedBusinessObject.DataGrouping => ZX1_ZZZ_NKDataGrouping;

		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.StartDate => ZX1_StartDate;

		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.EndDate => ZX1_EndDate;

		#endregion

		#region ITranslatableZZBusinessObject Members

		public ITableSchema LanguageTableSchema => RefCusConditionLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusConditionLanguage);

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : TranslatableZZBusinessObjectFetchStrategy<RefCusCondition>
		{
			public Strategy(RefCusCondition refCusCondition)
				: base(refCusCondition)
			{
			}

			new RefCusCondition BusinessObject => (RefCusCondition)base.BusinessObject;

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(RefCusConditionTypeSchema.PK, BusinessObject.ZX1_ZX2_ConditionType);
				Factory.AddFetchHint(RefCusPreferenceSchema.PK, BusinessObject.ZX1_ZZS_Preference);
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(RefCusConditionValueSchema.ZX3_ZX1_Condition, BusinessObject.PK);
				Factory.AddFetchHint(RefCusApplicabilitySchema.ZZT_ZX1_Conditions, BusinessObject.PK);
			}
		}

		#endregion
	}
}
