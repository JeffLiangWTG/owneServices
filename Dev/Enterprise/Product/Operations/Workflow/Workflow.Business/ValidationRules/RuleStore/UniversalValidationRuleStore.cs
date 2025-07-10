using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Workflow.Business
{
	public class UniversalValidationRuleStore : IUniversalValidationRuleStore
	{
		public IEnumerable<IReadOnlyUniversalValidationRuleSet> GetActiveRulesByCode(BusinessObjectFactory factory, string dataContextType, IEnumerable<string> codes)
		{
			return Instance.GetActiveRulesByCode(factory, dataContextType, codes);
		}

		public IEnumerable<IReadOnlyUniversalValidationRuleSet> GetActiveRulesByDataContext(BusinessObjectFactory factory, string dataContextType)
		{
			return Instance.GetActiveRulesByDataContext(factory, dataContextType);
		}

		static UniversalValidationRuleStoreImpl Instance
		{
			get
			{
#if DEBUG
				if (!OverridableInstance.IsOverriden)
				{
					OverridableInstance.Value = new UniversalValidationRuleStoreImpl();
				}
#endif
				return OverridableInstance.Value;
			}
		}

		[ThreadSafe]
		static readonly Overridable<UniversalValidationRuleStoreImpl> OverridableInstance = new Overridable<UniversalValidationRuleStoreImpl>(new UniversalValidationRuleStoreImpl());
	}

	class UniversalValidationRuleStoreImpl
	{
		readonly TimeSpan CacheExpiry = TimeSpan.FromSeconds(eAdaptorRegistry.Instance.ValidationRuleMacroCacheInSeconds.Value);
		readonly MemoryCache MemoryCache = new MemoryCache(nameof(UniversalValidationRuleStoreImpl));

		internal IEnumerable<IReadOnlyUniversalValidationRuleSet> GetActiveRulesByCode(BusinessObjectFactory factory, string dataContextType, IEnumerable<string> codes)
		{
			if (!string.IsNullOrEmpty(dataContextType) && codes.Any())
			{
				var rulesByCode = GetActiveRulesDictionaryByCode(factory, dataContextType);

				foreach (var code in codes)
				{
					if (rulesByCode.TryGetValue(code, out var rule))
					{
						yield return rule;
					}
				}
			}
		}

		internal IEnumerable<IReadOnlyUniversalValidationRuleSet> GetActiveRulesByDataContext(BusinessObjectFactory factory, string dataContextType)
		{
			return GetActiveRulesDictionaryByCode(factory, dataContextType).Values;
		}

		Dictionary<string, IReadOnlyUniversalValidationRuleSet> GetActiveRulesDictionaryByCode(BusinessObjectFactory factory, string dataContextType)
		{
			var rules = MemoryCache.Get(dataContextType) as Dictionary<string, IReadOnlyUniversalValidationRuleSet>;
			if (rules != null)
			{
				return rules;
			}

			var ruleSets = LoadFromDbByDataContext(factory, dataContextType).ToDictionary(obj => obj.Code.ToString());
			MemoryCache.Set(dataContextType, ruleSets, DateTimeOffset.Now.Add(CacheExpiry));

			return ruleSets;
		}

		#region Load From DB

		IEnumerable<IReadOnlyUniversalValidationRuleSet> LoadFromDbByDataContext(BusinessObjectFactory factory, string dataContextType)
		{
			var query = new ZQuery(UniversalValidationRuleSetSchema.VRS_DataContext, dataContextType);
			return LoadActiveFromDb(factory, query);
		}

		IEnumerable<IReadOnlyUniversalValidationRuleSet> LoadActiveFromDb(BusinessObjectFactory factory, ZQuery query)
		{
			query.AddToFilter(UniversalValidationRuleSetSchema.VRS_IsActive, true);
			var bizRuleSets = factory.Load<UniversalValidationRuleSet>(query);

			if (bizRuleSets.Length > 0)
			{
				var activeRuleQuery = new ZQuery(UniversalValidationRuleSchema.VR_VRS_Parent, bizRuleSets.Select(x => x.PK))
					.AddToFilter(UniversalValidationRuleSchema.VR_IsActive, ZBool.True);
				var activeRulesByParent = factory.Load<UniversalValidationRule>(activeRuleQuery)
					.GroupBy(x => x.VR_VRS_Parent)
					.ToDictionary(x => x.Key, y => (IEnumerable<UniversalValidationRule>)y);
				var result = new List<IReadOnlyUniversalValidationRuleSet>(bizRuleSets.Length);
				foreach (var ruleSet in bizRuleSets)
				{
					if (!activeRulesByParent.TryGetValue(ruleSet.PK, out var childRules))
					{
						childRules = Enumerable.Empty<UniversalValidationRule>();
					}
					result.Add(new ReadOnlyUniversalValidationRuleSet(ruleSet, childRules));
				}
				return result;
			}

			return Enumerable.Empty<IReadOnlyUniversalValidationRuleSet>();
		}

		#endregion
	}
}
