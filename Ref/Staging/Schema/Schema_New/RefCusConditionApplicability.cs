using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public class RefCusConditionApplicability
	{
		public RefCusConditionApplicability()
		{
			S07_PK = Guid.NewGuid();
			RefCusConditionApplicabilityValues = new HashSet<RefCusConditionApplicabilityValue>();
			RefCusConditionApplicabilityLanguages = new HashSet<RefCusConditionApplicabilityLanguage>();
			RefCusExcludedTradeGroupNews = new HashSet<RefCusExcludedTradeGroupNew>();
		}

		public RefCusConditionApplicability(RefCusCondition condition, RefCusApplicability app)
			: this()
		{
			S07_ZX2_NKConditionType = condition.ZX1_ZX2_NKConditionType;
			S07_ZZ1_Tariff = condition.ZX1_ZZ1_Tariff;
			S07_ZZ5_Nomenclature = condition.ZX1_ZZ5_Nomenclature;
			S07_StartDate = app.ZZT_StartDate > condition.ZX1_StartDate ? app.ZZT_StartDate : condition.ZX1_StartDate;
			S07_EndDate = app.ZZT_EndDate < condition.ZX1_EndDate ? app.ZZT_EndDate : condition.ZX1_EndDate;
			S07_Source = condition.ZX1_Source;
			S07_Comment = condition.ZX1_Comment;
			S07_IsImport = condition.ZX1_IsImport;
			S07_IsExport = condition.ZX1_IsExport;
			S07_ConditionValueTrueMeansStop = condition.ZX1_ConditionValueTrueMeansStop;
			S07_ZZS_NKPreference = condition.ZX1_ZZS_NKPreference;
			S07_ZZZ_NKDataGrouping = condition.ZX1_ZZZ_NKDataGrouping;
			S07_LogicalANDWithinGroup = condition.ZX1_LogicalANDWithinGroup;
			S07_ZY7_NKConditionCode = condition.ZX1_ZY7_NKConditionCode;
			S07_AdditionalComment = condition.ZX1_AdditionalComment;
			S07_ZX2_ZZZ_NKDataGrouping = condition.ZX1_ZX2_ZZZ_NKDataGrouping;
			S07_ZZS_ZZZ_NKDataGrouping = condition.ZX1_ZZS_ZZZ_NKDataGrouping;
			S07_Severity = condition.ZX1_Severity;

			OriginalRefCusApplicabilityPK = app.ZZT_PK;
			S07_ZZA_NKTradeGroup = app.ZZT_ZZA_NKTradeGroup;
			S07_AdditionalCode = app.ZZT_AdditionalCode;
			S07_OrderNumber = app.ZZT_OrderNumber;
			S07_ZZA_NKSecondTradeGroup = app.ZZT_ZZA_NKSecondTradeGroup;
			S07_ZZA_ZZZ_NKDataGrouping = app.ZZT_ZZA_ZZZ_NKDataGrouping;
			S07_ZZA_ZZZ_NKSecondDataGrouping = app.ZZT_ZZA_ZZZ_NKSecondDataGrouping;

			foreach (var conditionValue in condition.RefCusConditionValues)
			{
				var condAppVal = new RefCusConditionApplicabilityValue(conditionValue);
				condAppVal.S08_S07_ConditionApplicability = S07_PK;
				RefCusConditionApplicabilityValues.Add(condAppVal);
			}
			foreach (var conditionLanguage in condition.RefCusConditionLanguages)
			{
				var condAppLang = new RefCusConditionApplicabilityLanguage(conditionLanguage);
				condAppLang.S09_S07_ConditionApplicability = S07_PK;
				RefCusConditionApplicabilityLanguages.Add(condAppLang);
			}

			foreach (var ex in app.RefCusExcludedTradeGroups)
			{
				var exNew = new RefCusExcludedTradeGroupNew(ex);
				exNew.S03_S07_ConditionApplicability = S07_PK;
				RefCusExcludedTradeGroupNews.Add(exNew);
			}
		}

		public Guid S07_PK { get; set; }
		public string S07_ZX2_NKConditionType { get; set; }
		public Guid? S07_ZZ1_Tariff { get; set; }
		public Guid? S07_ZZ5_Nomenclature { get; set; }
		public DateTime S07_StartDate { get; set; }
		public DateTime S07_EndDate { get; set; }
		public string S07_Source { get; set; }
		public string S07_Comment { get; set; }
		public bool S07_IsImport { get; set; }
		public bool S07_IsExport { get; set; }
		public bool S07_ConditionValueTrueMeansStop { get; set; }
		public string S07_ZZS_NKPreference { get; set; }
		public string S07_ZZZ_NKDataGrouping { get; set; }
		public byte S07_LogicalANDWithinGroup { get; set; }
		public string S07_ZY7_NKConditionCode { get; set; }
		public string S07_AdditionalComment { get; set; }
		public string S07_ZZA_NKTradeGroup { get; set; }
		public string S07_AdditionalCode { get; set; }
		public string S07_OrderNumber { get; set; }
		public string S07_ZZA_NKSecondTradeGroup { get; set; }
		public string S07_ZX2_ZZZ_NKDataGrouping { get; set; }
		public string S07_ZZS_ZZZ_NKDataGrouping { get; set; }
		public string S07_ZZA_ZZZ_NKDataGrouping { get; set; }
		public string S07_ZZA_ZZZ_NKSecondDataGrouping { get; set; }
		public string S07_Severity { get; set; }

		public ICollection<RefCusConditionApplicabilityValue> RefCusConditionApplicabilityValues { get; set; }
		public ICollection<RefCusConditionApplicabilityLanguage> RefCusConditionApplicabilityLanguages { get; set; }
		public ICollection<RefCusExcludedTradeGroupNew> RefCusExcludedTradeGroupNews { get; set; }

		public Guid OriginalRefCusApplicabilityPK { get; private set; }
	}
}
