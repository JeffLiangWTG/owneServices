using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.Service;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.ProductionRules.Business
{
	public class CW1ProductionRulesLoader : IProductionRulesLoader
	{
		public CW1ProductionRulesLoader(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		BusinessObjectFactory Factory { get; }

		public IEnumerable<IProductionRule> LoadRules(string contextType, string contextSubType, ProductionRuleSetFilter filters)
		{
			#region SuppressResourceStringsCheckRegion

			// NOTE: If extending this, please be mindful of index utilisation
			var sqlFilter = FormattableString.Invariant($@"
				PRL_PRS_RuleSet = (
					SELECT TOP 1
						PRS_PK
					FROM
						dbo.ProductionRuleSet
					WHERE
						PRS_Context = @ContextType
						AND PRS_ContextSubType = @ContextSubType
						AND PRS_IsLive = 1 
						{(filters.WarehousePK.HasValue ? "AND (PRS_WW_Warehouse IS NULL OR PRS_WW_Warehouse = @WarehousePK)" : "AND PRS_WW_Warehouse IS NULL")}
						{(filters.CompanyPK.HasValue ? "AND (PRS_GC_Company IS NULL OR PRS_GC_Company = @CompanyPK)" : "AND PRS_GC_Company IS NULL")}
					{(filters.WarehousePK.HasValue ? "ORDER BY PRS_WW_Warehouse DESC" : "")}
					{(filters.CompanyPK.HasValue ? "ORDER BY PRS_GC_Company DESC" : "")})");

			var query = new ZDBOnlyQuery(typeof(ProductionRule));
			query.IncludeBlob(ProductionRuleSchema.PRL_RuleDefinition);

			var sqlFilterParameters = new ZSqlParameterCollection();
			sqlFilterParameters.Add("@ContextType", contextType, ProductionRuleSetSchema.PRS_Context);
			sqlFilterParameters.Add("@ContextSubType", contextSubType, ProductionRuleSetSchema.PRS_ContextSubType);
			sqlFilterParameters.Add("@WarehousePK", filters.WarehousePK, ProductionRuleSetSchema.PRS_WW_Warehouse);
			sqlFilterParameters.Add("@CompanyPK", filters.CompanyPK, ProductionRuleSetSchema.PRS_GC_Company);

			query.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);

			return LoadRulesCore(contextType, contextSubType, query);

			#endregion
		}

		public IEnumerable<IProductionRule> LoadAllRulesIncludingInactive(string contextType, string contextSubType)
		{
			var ruleSetSubQuery = new ZDBOnlySubQuery(typeof(ProductionRuleSet), ProductionRuleSchema.PRL_PRS_RuleSet);
			ruleSetSubQuery.AddToFilter(ProductionRuleSetSchema.PRS_Context, contextType);
			ruleSetSubQuery.AddToFilter(ProductionRuleSetSchema.PRS_ContextSubType, contextSubType);

			var query = new ZDBOnlyQuery(typeof(ProductionRule));
			query.AddSubQuery(ruleSetSubQuery, JoinCondition.And);
			query.IncludeBlob(ProductionRuleSchema.PRL_RuleDefinition);

			return LoadRulesCore(contextType, contextSubType, query);
		}

		IEnumerable<IProductionRule> LoadRulesCore(string contextType, string contextSubType, ZQuery query)
		{
			return
				Factory.Load<ProductionRule>(query).
				Select(r => new ProductionRuleWrapper(contextType, contextSubType, r.PRL_Name, r.PRL_Description, r.PRL_Priority, r.PRL_RuleDefinition)).
				ToArray();
		}
	}
}
