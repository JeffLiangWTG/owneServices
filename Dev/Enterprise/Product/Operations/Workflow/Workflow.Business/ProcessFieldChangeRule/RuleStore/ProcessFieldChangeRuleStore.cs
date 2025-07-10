using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRuleStore : IProcessFieldChangeRuleStore
	{
		public Dictionary<string, List<IReadOnlyProcessFieldChangeRule>> GetProcessFieldChangeRules(BusinessObjectFactory factory, string processType)
		{
			return Instance.GetProcessFieldChangeRules(factory, processType);
		}

		public IEnumerable<string> GetActiveRuleTables(BusinessObjectFactory factory)
		{
			return Instance.GetActiveRuleTables(factory);
		}

		public void ClearCache()
		{
			Instance.ClearCache();
		}

		[ThreadSafe]
		static readonly ProcessFieldChangeRuleStoreImpl Instance = new ProcessFieldChangeRuleStoreImpl();
	}

	class ProcessFieldChangeRuleStoreImpl
	{
		readonly TimeSpan CacheExpiry = TimeSpan.FromSeconds(ObjectFactory.Get<IEntityFrameworkSettings>().UberFactoryTimeoutPeriod);
		readonly MemoryCache ImplMemoryCache = new MemoryCache(nameof(ProcessFieldChangeRuleStoreImpl));

		const string HOOKED_TABLES = "PFRLookups.Memory.HookedTables";

		public void ClearCache()
		{
			List<string> cacheKeys = ImplMemoryCache.Select(kvp => kvp.Key).ToList();
			foreach (string cacheKey in cacheKeys)
			{
				ImplMemoryCache.Remove(cacheKey);
			}
		}

		internal IEnumerable<IReadOnlyProcessFieldChangeRule> GetActiveProcessFieldChangeRulesByType(BusinessObjectFactory factory, string processType)
		{
			return LoadFromDbByType(factory, processType);
		}

		internal Dictionary<string, List<IReadOnlyProcessFieldChangeRule>> GetProcessFieldChangeRules(BusinessObjectFactory factory, string processType)
		{
			var storedLookup = ImplMemoryCache.Get(processType) as Dictionary<string, List<IReadOnlyProcessFieldChangeRule>>;
			if (storedLookup != null)
			{
				return storedLookup;
			}
			var newLookup = new Dictionary<string, List<IReadOnlyProcessFieldChangeRule>>(StringComparer.OrdinalIgnoreCase);
			var rules = GetActiveProcessFieldChangeRulesByType(factory, processType).ToList();

			foreach (var rule in rules)
			{
				foreach (var field in rule.Fields)
				{
					if (!field.IsBlacklisted)
					{
						if (!newLookup.ContainsKey(field.FieldName))
						{
							newLookup[field.FieldName] = new List<IReadOnlyProcessFieldChangeRule>();
						}

						newLookup[field.FieldName].Add(rule);
					}
				}
			}

			ImplMemoryCache.Set(processType, newLookup, DateTimeOffset.Now.Add(CacheExpiry));

			return newLookup;
		}

		internal IEnumerable<string> GetActiveRuleTables(BusinessObjectFactory factory)
		{
			var pfrTables = ImplMemoryCache.Get(HOOKED_TABLES) as IEnumerable<string>;
			if (pfrTables != null)
			{
				return pfrTables;
			}

			var newPfrTables = LoadActiveRuleTablesFromDb(factory);
			if (newPfrTables != null)
			{
				ImplMemoryCache.Set(HOOKED_TABLES, newPfrTables, DateTimeOffset.Now.Add(CacheExpiry));
				return newPfrTables;
			}
			return Enumerable.Empty<string>();
		}

		#region Load From DB

		internal IEnumerable<IReadOnlyProcessFieldChangeRule> LoadFromDbByType(BusinessObjectFactory factory, string processType)
		{
			var query = new ZQuery(ProcessFieldChangeRuleSchema.PFR_ProcessType, processType);
			return LoadActiveFromDb(factory, query);
		}

		internal IEnumerable<IReadOnlyProcessFieldChangeRule> LoadActiveFromDb(BusinessObjectFactory factory, ZQuery query)
		{
			query.AddToFilter(ProcessFieldChangeRuleSchema.PFR_IsActive, true);
			var processFieldChangeRules = factory.Load<ProcessFieldChangeRule>(query);

			if (processFieldChangeRules.Length > 0)
			{
				var activeRuleFieldQuery = new ZQuery(ProcessFieldChangeRuleFieldSchema.PFL_PFR, processFieldChangeRules.Select(x => x.PK));
				var activeRuleFieldsByParent = factory.Load<ProcessFieldChangeRuleField>(activeRuleFieldQuery)
					.GroupBy(x => x.PFL_PFR)
					.ToDictionary(x => x.Key, y => (IEnumerable<ProcessFieldChangeRuleField>)y);
				var result = new List<IReadOnlyProcessFieldChangeRule>(processFieldChangeRules.Length);

				foreach (var changeRule in processFieldChangeRules)
				{
					if (!activeRuleFieldsByParent.TryGetValue(changeRule.PK, out var childFields))
					{
						childFields = Enumerable.Empty<ProcessFieldChangeRuleField>();
					}
					result.Add(new ReadOnlyProcessFieldChangeRule(changeRule, childFields));
				}
				return result;
			}

			return Enumerable.Empty<IReadOnlyProcessFieldChangeRule>();
		}

		internal IEnumerable<string> LoadActiveRuleTablesFromDb(BusinessObjectFactory factory)
		{
			var rules = factory.Load<ProcessFieldChangeRule>(new ZQuery()).Where(rule => rule.PFR_IsActive).ToList();
			var allFields = rules.SelectMany(rule => rule.Fields);
			var allTables = allFields.Select(field => field.PFL_TableCode.ToString()).Distinct().ToList() as IEnumerable<string>;

			return allTables;
		}
		#endregion
	}
}
