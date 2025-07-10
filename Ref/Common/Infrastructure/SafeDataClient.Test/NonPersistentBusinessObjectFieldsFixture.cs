using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	class NonPersistentBusinessObjectFieldsFixture
	{
		[Test]
		public void RefCusRateApplicability_Rate_App()
		{
			var persist_RelatedEntityProperties = new[] { "ZX1_Conditions", "ZZ2_Rate" };
			var rateApp_Props = typeof(RefCusRateApplicability).GetProperties().Where(x => x.Name.StartsWith("S01")).Select(x => x.Name.Substring(4));
			var rateAndApp_Props = typeof(RefCusRate).GetProperties().Where(x => x.Name.StartsWith("ZZ2")).Select(x => x.Name.Substring(4))
				.Union(typeof(RefCusApplicability).GetProperties().Where(x => x.Name.StartsWith("ZZT")).Select(x => x.Name.Substring(4)))
				.Except(IgnoreProperties).Distinct();
			rateApp_Props = rateApp_Props.Union(persist_RelatedEntityProperties);
			CollectionAssert.AreEquivalent(rateAndApp_Props, rateApp_Props);
		}

		[Test]
		public void RefCusRateApplicabilityUOM_RateUOM()
		{
			var nonPersist_RelatedEntityProperties = new[] { "S01_RateApplicability" };
			var persist_RelatedEntityProperties = new[] { "ZZ2_Rate" };
			var rateAppUOM_Props = typeof(RefCusRateApplicabilityUOM).GetProperties().Where(x => x.Name.StartsWith("S02")).Select(x => x.Name.Substring(4));
			var rateUOM_Props = typeof(RefCusRateUOM).GetProperties().Where(x => x.Name.StartsWith("ZXG")).Select(x => x.Name.Substring(4)).Except(IgnoreProperties);

			rateAppUOM_Props = rateAppUOM_Props.Union(persist_RelatedEntityProperties);
			rateUOM_Props = rateUOM_Props.Union(nonPersist_RelatedEntityProperties);
			CollectionAssert.AreEquivalent(rateUOM_Props, rateAppUOM_Props);
		}

		[Test]
		public void RefCusExcludedTradeGroupNew_ExcludedTradeGroup()
		{
			var nonPersist_RelatedEntityProperties = new string[] { "S01_RateApplicability", "S07_ConditionApplicability" };
			var persist_RelatedEntityProperties = new string[] { "ZZT_Applicability" };
			var exNew_Props = typeof(RefCusExcludedTradeGroupNew).GetProperties().Where(x => x.Name.StartsWith("S03")).Select(x => x.Name.Substring(4));
			var ex_Props = typeof(RefCusExcludedTradeGroup).GetProperties().Where(x => x.Name.StartsWith("ZZC")).Select(x => x.Name.Substring(4)).Except(IgnoreProperties);

			exNew_Props = exNew_Props.Union(persist_RelatedEntityProperties);
			ex_Props = ex_Props.Union(nonPersist_RelatedEntityProperties);
			CollectionAssert.AreEquivalent(ex_Props, exNew_Props);
		}

		[Test]
		public void RefCusConditionApplicability_Cond_App()
		{
			var persist_RelatedEntityProperties = new[] { "ZX1_Conditions", "ZZ2_Rate" };
			var condApp_Props = typeof(RefCusConditionApplicability).GetProperties().Where(x => x.Name.StartsWith("S07")).Select(x => x.Name.Substring(4));
			var condAndApp_Props = typeof(RefCusCondition).GetProperties().Where(x => x.Name.StartsWith("ZX1")).Select(x => x.Name.Substring(4))
				.Union(typeof(RefCusApplicability).GetProperties().Where(x => x.Name.StartsWith("ZZT")).Select(x => x.Name.Substring(4)))
				.Except(IgnoreProperties).Distinct();
			condApp_Props = condApp_Props.Union(persist_RelatedEntityProperties);
			CollectionAssert.AreEquivalent(condAndApp_Props, condApp_Props);
		}

		[Test]
		public void RefCusConditionApplicabilityValue_ConditionValue()
		{
			var nonPersist_RelatedEntityProperties = new[] { "S07_ConditionApplicability" };
			var persist_RelatedEntityProperties = new[] { "ZX1_Condition" };
			var condAppVal_Props_Props = typeof(RefCusConditionApplicabilityValue).GetProperties().Where(x => x.Name.StartsWith("S08")).Select(x => x.Name.Substring(4));
			var val_Props = typeof(RefCusConditionValue).GetProperties().Where(x => x.Name.StartsWith("ZX3")).Select(x => x.Name.Substring(4)).Except(IgnoreProperties);

			val_Props = val_Props.Union(nonPersist_RelatedEntityProperties);
			condAppVal_Props_Props = condAppVal_Props_Props.Union(persist_RelatedEntityProperties);
			CollectionAssert.AreEquivalent(val_Props, condAppVal_Props_Props);
		}

		[Test]
		public void RefCusConditionApplicabilityLanguage_ConditionLanguage()
		{
			var nonPersist_RelatedEntityProperties = new[] { "S07_ConditionApplicability" };
			var persist_RelatedEntityProperties = new[] { "ZX1_Condition" };
			var condAppLang_Props = typeof(RefCusConditionApplicabilityLanguage).GetProperties().Where(x => x.Name.StartsWith("S09")).Select(x => x.Name.Substring(4));
			var lang_Props = typeof(RefCusConditionLanguage).GetProperties().Where(x => x.Name.StartsWith("ZXJ")).Select(x => x.Name.Substring(4)).Except(IgnoreProperties);

			lang_Props = lang_Props.Union(nonPersist_RelatedEntityProperties);
			condAppLang_Props = condAppLang_Props.Union(persist_RelatedEntityProperties);
			CollectionAssert.AreEquivalent(lang_Props, condAppLang_Props);
		}

		private readonly static string[] IgnoreProperties = new[]
		{
			"DataSetPK", "DataSetCode", "SysStartTime", "SysEndTime"
		};
	}
}
