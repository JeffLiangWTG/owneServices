using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	internal class OrderNumberOwnersRefSpecificFieldSubGroup : BlueprintModuleFilterSubGroup
	{
		internal OrderNumberOwnersRefSpecificFieldSubGroup()
			: base(ModuleFilterSubGroup.Default, false)
		{
		}

		public override QueryBlueprintPart GetSubQuery(QueryBlueprintPart query)
		{
			var result = new ZQuery();

			foreach (var moduleFilterSubGroup in query.Children)
			{
				var orCategories = new List<QueryBlueprintPart>(moduleFilterSubGroup.Children);
				var noneOrCategories = orCategories.Where(x => !x.Query.IsEmpty).ToList();
				orCategories.RemoveAll(x => noneOrCategories.Contains(x));
				if (noneOrCategories.Any())
				{
					var noneResult = new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And)
					{
						Children = noneOrCategories
					};
					result.AddToFilter(GetSubQueryForOrCategory(noneResult));
				}
				foreach (var orCategory in orCategories)
				{
					result.AddToFilter(GetSubQueryForOrCategory(orCategory));
				}
			}

			return new QueryBlueprintPart(result, FilterOrCategory.None, JoinCondition.And);
		}

		ZQuery GetSubQueryForOrCategory(QueryBlueprintPart query)
		{
			// Negative and Positive SQL Operators cannot be mixed into the same sub-query, because negative requires "JE_PK NOT IN..."
			var negativeSqlOperatorParts = query.Children.Where(x =>
				x.Query.Params.Any(param => param.ComparisonOperator.IsNegativeSQLOperator())).ToList();
			var positiveSqlOperatorParts = query.Children.Where(x => !negativeSqlOperatorParts.Contains(x)).ToList();
			var result = new ZQuery();

			if (positiveSqlOperatorParts.Any())
			{
				result.AddToFilter(GetSubQuery(
					new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And)
					{
						Children = positiveSqlOperatorParts
					}.Construct(), false), positiveSqlOperatorParts.First().Join);
			}

			if (negativeSqlOperatorParts.Any())
			{
				var join = negativeSqlOperatorParts.First().Join;
				var negatedJoinCondition = join == JoinCondition.And
					? JoinCondition.Or
					: JoinCondition.And;

				foreach (var negativeSqlOperatorPart in negativeSqlOperatorParts)
				{
					var param = negativeSqlOperatorPart.Query.Params.First();
					negativeSqlOperatorPart.Query = new ZQuery(param.SchemaColumn, param.ComparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), param.Value, param.ComparisonOptions);
					negativeSqlOperatorPart.Join = negatedJoinCondition;
				}
				result.AddToFilter(GetSubQuery(
					new QueryBlueprintPart(new ZQuery(), FilterOrCategory.None, JoinCondition.And)
					{
						Children = negativeSqlOperatorParts
					}.Construct(), true), join);
			}

			return result;
		}

		ZQuery GetSubQuery(ZQuery filter, bool isNot)
		{
			var queryString = string.Format("JE_PK {1}IN (SELECT JE_PK FROM dbo.JobDeclarationOrderNumber {0})", filter.GetAsWhereClause(false), (isNot ? "NOT " : ""));

			var query = new ZQuery();
			query.AddFilterAndZSQLParameterCollection(queryString, new ZSqlParameterCollection(filter.Params), true);

			return query;
		}
	}
}
