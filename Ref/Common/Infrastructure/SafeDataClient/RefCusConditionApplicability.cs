using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	[NonPersistentObject]
	public class RefCusConditionApplicability : INonPersistentBusinessObjectFlatten
	{
		public RefCusConditionApplicability(ISafeRepository repo)
		{
			S07_PK = Guid.NewGuid();
			RefCusConditionApplicabilityValues = new HashSet<RefCusConditionApplicabilityValue>();
			RefCusConditionApplicabilityLanguages = new HashSet<RefCusConditionApplicabilityLanguage>();
			RefCusExcludedTradeGroupNews = new HashSet<RefCusExcludedTradeGroupNew>();
			repo.TrackNonPersistentFlattenObject(this);
		}

		public RefCusConditionApplicability(RefCusCondition condition, RefCusApplicability app, ISafeRepository repo)
			: this(repo)
		{
			S07_ZX2_ConditionType = condition.ZX1_ZX2_ConditionType;
			S07_ZZ1_Tariff = condition.ZX1_ZZ1_Tariff;
			S07_ZZ5_Nomenclature = condition.ZX1_ZZ5_Nomenclature;
			S07_StartDate = app.ZZT_StartDate > condition.ZX1_StartDate ? app.ZZT_StartDate : condition.ZX1_StartDate;
			S07_EndDate = app.ZZT_EndDate < condition.ZX1_EndDate ? app.ZZT_EndDate : condition.ZX1_EndDate;
			S07_Source = condition.ZX1_Source;
			S07_Comment = condition.ZX1_Comment;
			S07_IsImport = condition.ZX1_IsImport;
			S07_IsExport = condition.ZX1_IsExport;
			S07_ConditionValueTrueMeansStop = condition.ZX1_ConditionValueTrueMeansStop;
			S07_ZZS_Preference = condition.ZX1_ZZS_Preference;
			S07_ZZZ_NKDataGrouping = condition.ZX1_ZZZ_NKDataGrouping;
			S07_LogicalANDWithinGroup = condition.ZX1_LogicalANDWithinGroup;
			S07_ZY7_NKConditionCode = condition.ZX1_ZY7_NKConditionCode;
			S07_AdditionalComment = condition.ZX1_AdditionalComment;
			S07_Severity = condition.ZX1_Severity;

			S07_ZZA_TradeGroup = app.ZZT_ZZA_TradeGroup;
			S07_AdditionalCode = app.ZZT_AdditionalCode;
			S07_OrderNumber = app.ZZT_OrderNumber;
			S07_ZZA_SecondTradeGroup = app.ZZT_ZZA_SecondTradeGroup;
			S07_ZY2_AdditionalCode = app.ZZT_ZY2_AdditionalCode;
			S07_ZZH_TariffRelationship = app.ZZT_ZZH_TariffRelationship;
			RefCusCondition = condition;
			RefCusApplicability = app;
			app.RefCusConditionApplicabilities.Add(this);
			condition.RefCusConditionApplicabilities.Add(this);
			foreach (var conditionValue in condition.RefCusConditionValues)
			{
				var condAppVal = new RefCusConditionApplicabilityValue(conditionValue, this);
				condAppVal.S08_S07_ConditionApplicability = S07_PK;
				RefCusConditionApplicabilityValues.Add(condAppVal);
			}
			foreach (var conditionLanguage in condition.RefCusConditionLanguages)
			{
				var condAppLang = new RefCusConditionApplicabilityLanguage(conditionLanguage, this);
				condAppLang.S09_S07_ConditionApplicability = S07_PK;
				RefCusConditionApplicabilityLanguages.Add(condAppLang);
			}

			foreach (var ex in app.RefCusExcludedTradeGroups)
			{
				var exNew = new RefCusExcludedTradeGroupNew(ex, this);
				exNew.S03_S07_ConditionApplicability = S07_PK;
				RefCusExcludedTradeGroupNews.Add(exNew);
			}
		}

		public IEnumerable<object> LinkedObjects
		{
			get
			{
				yield return RefCusApplicability;
				yield return RefCusCondition;
				foreach (var condAppVal in RefCusConditionApplicabilityValues)
				{
					yield return condAppVal.RefCusConditionValue;
				}
				foreach (var condAppLang in RefCusConditionApplicabilityLanguages)
				{
					yield return condAppLang.RefCusConditionLanguage;
				}
				foreach (var exNew in RefCusExcludedTradeGroupNews)
				{
					yield return exNew.RefCusExcludedTradeGroup;
				}
			}
		}

		public void Link(INonPersistentBusinessObjectParent parent, INonPersistentBusinessObjectComparison comp, out IEnumerable<object> unLinkedObjs)
		{
			if (!isCreatedCalled)
			{
				throw new InvalidOperationException("Created method should be called before Link method");
			}
			var cond = (RefCusCondition)parent;
			var results = new List<object>();
			var unlinkedCond = UnlinkRefCusCondition();
			RefCusApplicability unlinkedApp = null;
			results.Add(unlinkedCond);
			var matchedApp = cond.RefCusApplicabilities.FirstOrDefault(x => comp.IsIdentical(this, x));
			if (matchedApp == null)
			{
				matchedApp = RefCusApplicability;
				cond.RefCusApplicabilities.Add(matchedApp);
				matchedApp.ZZT_ZX1_Conditions = cond.ZX1_PK;
				unlinkedCond.RefCusApplicabilities.Remove(matchedApp);
			}
			else
			{
				unlinkedApp = UnLinkRefCusApplicability();
				results.Add(unlinkedApp);
			}
			Link(matchedApp, cond);
			foreach (var condAppVal in RefCusConditionApplicabilityValues)
			{
				var machedAppVal = cond.RefCusConditionValues.FirstOrDefault(x => comp.IsIdentical(condAppVal, x));
				if (machedAppVal == null)
				{
					machedAppVal = condAppVal.RefCusConditionValue;
					machedAppVal.ZX3_ZX1_Condition = cond.ZX1_PK;
					cond.RefCusConditionValues.Add(machedAppVal);
					unlinkedCond.RefCusConditionValues.Remove(machedAppVal);
				}
				else
				{
					results.AddRange(condAppVal.Unlink());
				}
				condAppVal.Link(machedAppVal);
			}
			foreach (var condAppLang in RefCusConditionApplicabilityLanguages)
			{
				var machedAppLang = cond.RefCusConditionLanguages.FirstOrDefault(x => comp.IsIdentical(condAppLang, x));
				if (machedAppLang == null)
				{
					machedAppLang = condAppLang.RefCusConditionLanguage;
					machedAppLang.ZXJ_ZX1_Condition = cond.ZX1_PK;
					cond.RefCusConditionLanguages.Add(machedAppLang);
					unlinkedCond.RefCusConditionLanguages.Remove(machedAppLang);
				}
				else
				{
					results.AddRange(condAppLang.Unlink());
				}
				condAppLang.Link(machedAppLang);
			}
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				var matchedEx = matchedApp.RefCusExcludedTradeGroups.FirstOrDefault(x => comp.IsIdentical(exNew, x));
				if (matchedEx == null)
				{
					matchedEx = exNew.RefCusExcludedTradeGroup;
					matchedEx.ZZC_ZZT_Applicability = matchedApp.ZZT_PK;
					matchedApp.RefCusExcludedTradeGroups.Add(matchedEx);
					if (unlinkedApp != null)
					{
						unlinkedApp.RefCusExcludedTradeGroups.Remove(matchedEx);
					}
				}
				else
				{
					results.AddRange(exNew.Unlink());
				}
				exNew.Link(matchedEx);
			}
			unLinkedObjs = results;
		}

		void Link(RefCusApplicability app, RefCusCondition cond)
		{
			RefCusApplicability = app;
			RefCusCondition = cond;
			if (!app.RefCusConditionApplicabilities.Contains(this))
			{
				app.RefCusConditionApplicabilities.Add(this);
			}
			if (!cond.RefCusConditionApplicabilities.Contains(this))
			{
				cond.RefCusConditionApplicabilities.Add(this);
			}
		}

		public IEnumerable<object> Create()
		{
			isCreatedCalled = true;
			var result = new List<object>();
			if (RefCusApplicability == null)
			{
				var cond = new RefCusCondition() { ZX1_PK = Guid.NewGuid() };
				cond.ZX1_ZZ1_Tariff = S07_ZZ1_Tariff;
				result.Add(cond);
				var app = new RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
				cond.RefCusApplicabilities.Add(app);
				app.ZZT_ZX1_Conditions = cond.ZX1_PK;
				Link(app, cond);
				result.Add(app);
				UpdateCore(true);
			}
			foreach (var condAppVal in RefCusConditionApplicabilityValues)
			{
				if (condAppVal.RefCusConditionValue == null)
				{
					var condVal = RefCusCondition.RefCusConditionValues.FirstOrDefault(x => nonPersistentObjecComparison.IsIdentical(condAppVal, x));
					if (condVal == null)
					{
						condVal = new RefCusConditionValue { ZX3_PK = Guid.NewGuid() };
						condVal.ZX3_ZX1_Condition = RefCusCondition.ZX1_PK;
						RefCusCondition.RefCusConditionValues.Add(condVal);
						result.Add(condVal);
					}
					condAppVal.Link(condVal);
					condAppVal.Update();
				}
			}
			foreach (var condAppLang in RefCusConditionApplicabilityLanguages)
			{
				if (condAppLang.RefCusConditionLanguage == null)
				{
					var condLang = RefCusCondition.RefCusConditionLanguages.FirstOrDefault(x => nonPersistentObjecComparison.IsIdentical(condAppLang, x));
					if (condLang == null)
					{
						condLang = new RefCusConditionLanguage { ZXJ_PK = Guid.NewGuid() };
						condLang.ZXJ_ZX1_Condition = RefCusCondition.ZX1_PK;
						RefCusCondition.RefCusConditionLanguages.Add(condLang);
						result.Add(condLang);
					}
					condAppLang.Link(condLang);
					condAppLang.Update();
				}
			}
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				if (exNew.RefCusExcludedTradeGroup == null)
				{
					var ex = RefCusApplicability.RefCusExcludedTradeGroups.FirstOrDefault(x => nonPersistentObjecComparison.IsIdentical(exNew, x));
					if (ex == null)
					{
						ex = new RefCusExcludedTradeGroup() { ZZC_PK = Guid.NewGuid() };
						ex.ZZC_ZZT_Applicability = RefCusApplicability.ZZT_PK;
						RefCusApplicability.RefCusExcludedTradeGroups.Add(ex);
						result.Add(ex);
					}
					exNew.Link(ex);
					exNew.Update();
				}
			}
			return result;
		}
		bool isCreatedCalled;

		public IEnumerable<object> Unlink()
		{
			var result = new List<object>();
			foreach (var condAppVal in RefCusConditionApplicabilityValues)
			{
				result.AddRange(condAppVal.Unlink());
			}
			foreach (var condAppLang in RefCusConditionApplicabilityLanguages)
			{
				result.AddRange(condAppLang.Unlink());
			}
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				result.AddRange(exNew.Unlink());
			}
			result.Add(UnLinkRefCusApplicability());
			result.Add(UnlinkRefCusCondition());
			return result;
		}

		RefCusCondition UnlinkRefCusCondition()
		{
			var result = RefCusCondition;
			result.RefCusConditionApplicabilities.Remove(this);
			RefCusCondition = null;
			return result;
		}

		RefCusApplicability UnLinkRefCusApplicability()
		{
			var result = RefCusApplicability;
			result.RefCusConditionApplicabilities.Remove(this);
			RefCusApplicability = null;
			return result;
		}

		void UpdateCore(bool updateParent)
		{
			var app = RefCusApplicability;
			var cond = RefCusCondition;

			if (updateParent)
			{
				cond.ZX1_StartDate = NonPersistentBusinessObjectComparison.MinStartDate;
				cond.ZX1_EndDate = NonPersistentBusinessObjectComparison.MaxEndDate;
				cond.ZX1_ZX2_ConditionType = S07_ZX2_ConditionType;
				cond.ZX1_ZZ1_Tariff = S07_ZZ1_Tariff;
				cond.ZX1_ZZ5_Nomenclature = S07_ZZ5_Nomenclature;
				cond.ZX1_Source = S07_Source;
				cond.ZX1_Comment = S07_Comment;
				cond.ZX1_IsImport = S07_IsImport;
				cond.ZX1_IsExport = S07_IsExport;
				cond.ZX1_ConditionValueTrueMeansStop = S07_ConditionValueTrueMeansStop;
				cond.ZX1_ZZS_Preference = S07_ZZS_Preference;
				cond.ZX1_ZZZ_NKDataGrouping = S07_ZZZ_NKDataGrouping;
				cond.ZX1_LogicalANDWithinGroup = S07_LogicalANDWithinGroup;
				cond.ZX1_ZY7_NKConditionCode = S07_ZY7_NKConditionCode;
				cond.ZX1_AdditionalComment = S07_AdditionalComment;
				cond.ZX1_Severity = S07_Severity;
			}
			app.ZZT_ZZA_TradeGroup = S07_ZZA_TradeGroup;
			app.ZZT_AdditionalCode = S07_AdditionalCode;
			app.ZZT_OrderNumber = S07_OrderNumber;
			app.ZZT_ZZA_SecondTradeGroup = S07_ZZA_SecondTradeGroup;
			app.ZZT_StartDate = S07_StartDate;
			app.ZZT_EndDate = S07_EndDate;
			app.ZZT_ZY2_AdditionalCode = S07_ZY2_AdditionalCode;
		}

		public void Update(bool updateParent)
		{
			UpdateCore(updateParent);
			foreach (var condAppVal in RefCusConditionApplicabilityValues)
			{
				condAppVal.Update();
			}
			foreach (var condAppLang in RefCusConditionApplicabilityLanguages)
			{
				condAppLang.Update();
			}
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				exNew.Update();
			}
		}

		INonPersistentBusinessObjectComparison nonPersistentObjecComparison = new NonPersistentBusinessObjectComparison();
		public Guid? TariffPK => S07_ZZ1_Tariff;
		public Guid? TariffNationalCodePK => null;
		public Guid? NomenclatureGroupPK => S07_ZZ5_Nomenclature;

		public INonPersistentBusinessObjectParent Parent => RefCusCondition;
		public ICollection<INonPersistentBusinessObjectFlatten> TopLevelNonPersistentObjects => new HashSet<INonPersistentBusinessObjectFlatten> { this };

		public Guid S07_PK { get; set; }
		public Guid? S07_ZX2_ConditionType { get; set; }
		public Guid? S07_ZZ1_Tariff { get; set; }
		public Guid? S07_ZZ5_Nomenclature { get; set; }
		public DateTimeOffset S07_StartDate { get; set; }
		public DateTimeOffset S07_EndDate { get; set; }
		public string S07_Source { get; set; }
		public string S07_Comment { get; set; }
		public bool S07_IsImport { get; set; }
		public bool S07_IsExport { get; set; }
		public bool S07_ConditionValueTrueMeansStop { get; set; }
		public Guid? S07_ZZS_Preference { get; set; }
		public string S07_ZZZ_NKDataGrouping { get; set; }
		public byte S07_LogicalANDWithinGroup { get; set; }
		public string S07_ZY7_NKConditionCode { get; set; }
		public string S07_AdditionalComment { get; set; }
		public Guid? S07_ZZA_TradeGroup { get; set; }
		public string S07_AdditionalCode { get; set; }
		public string S07_OrderNumber { get; set; }
		public Guid? S07_ZZA_SecondTradeGroup { get; set; }
		public Guid? S07_ZY2_AdditionalCode { get; set; }
		public string S07_Severity { get; set; }
		public Guid? S07_ZZH_TariffRelationship { get; set; }
		public virtual RefCusCondition RefCusCondition { get; set; }
		public RefCusApplicability RefCusApplicability { get; set; }
		public ICollection<RefCusConditionApplicabilityValue> RefCusConditionApplicabilityValues { get; set; }
		public ICollection<RefCusConditionApplicabilityLanguage> RefCusConditionApplicabilityLanguages { get; set; }
		public ICollection<RefCusExcludedTradeGroupNew> RefCusExcludedTradeGroupNews { get; set; }
	}
}
