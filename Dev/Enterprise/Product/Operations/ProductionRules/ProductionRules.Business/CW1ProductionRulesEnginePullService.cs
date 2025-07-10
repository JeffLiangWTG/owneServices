using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.Service;

namespace Enterprise.ProductionRules.Business
{
	public class CW1ProductionRulesEnginePullService : ProductionRulesEngineService
	{
		protected override IProductionRulesLoader GetRulesLoader() => new EmptyRulesLoader();

		protected override IUserDefinedPropertyLoader GetUserDefinedPropertyLoader() => new CW1UserDefinedPropertyLoader();

		protected override IUnitConverter GetUnitConverter() => new UnitConverter();

		protected override DateTime GetCurrentTime() => ZDateTime.Now.ToDateTime();

		protected override int GetRulesEngineSessionCacheMinutes() => SystemDataRegistry.Instance.RulesEngineSessionFactoryCacheMinutes.Value;

		protected override ICustomFieldLoader GetCustomFieldLoader() => new CW1CustomFieldLoader();

		class EmptyRulesLoader : IProductionRulesLoader
		{
			public IEnumerable<IProductionRule> LoadAllRulesIncludingInactive(string contextType, string contextSubType) => throw new InvalidOperationException("This class should only be used for pull rulesets.");
			public IEnumerable<IProductionRule> LoadRules(string contextType, string contextSubType, ProductionRuleSetFilter filters) => throw new InvalidOperationException("This class should only be used for pull rulesets.");
		}
	}
}
