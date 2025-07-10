using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Service;

namespace Enterprise.ProductionRules.Business
{
	public class CW1UserDefinedPropertyLoader : UserDefinedPropertyLoader
	{
		public override IReadOnlyDictionary<string, Type> GetUserDefinedPropertiesFromDatabase(string factTypeKey)
		{
			using (var connection = Db.DisposableActionForDbConnection())
			{
				var factTypeSubQuery = new ZDBOnlySubQuery(typeof(ProductionRulesFactTypeView), GenCustomColumnDefinitionSchema.XC_ParentID);
				factTypeSubQuery.AddToFilter(ProductionRulesFactTypeViewSchema.PFV_FactTypeKey, factTypeKey);

				var query = new ZDBOnlyQuery(typeof(GenCustomColumnDefinition));
				query.AddSubQuery(factTypeSubQuery, JoinCondition.And);
				query.AddToFilter(GenCustomColumnDefinitionSchema.XC_ParentTableCode, ProductionRulesFactTypeViewSchema.Constants.Prefix);

				var factory = new BusinessObjectFactory { NameForDebugging = nameof(CW1UserDefinedPropertyLoader) };
				return factory.Load<GenCustomColumnDefinition>(query)
						.ToDictionary(GetName, col => GetTypeFromTypeCode(col.XC_Type), StringComparer.OrdinalIgnoreCase);
			}
		}

		static string GetName(GenCustomColumnDefinition customColumn) => customColumn.XC_Name.ToString();
	}
}
