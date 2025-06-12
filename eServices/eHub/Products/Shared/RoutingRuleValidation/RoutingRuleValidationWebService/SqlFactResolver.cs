using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	public class SqlFactResolver : IFactResolver
	{
		readonly eHubTransactionsContext Context;

		public SqlFactResolver(eHubTransactionsContext context)
		{
			Context = context;
		}

		public void Resolve(Fact[] facts)
		{
			foreach (var fact in facts.Where(f => f.Type == "SQL"))
			{
				var paramNames = Regex.Matches(fact.Query, @"@\w+");
				var sqlParams = new List<object>();

				foreach (Match match in paramNames)
				{
					var paramFact = facts.FirstOrDefault(f => ('@' + f.Name) == match.Value);
					sqlParams.Add(new SqlParameter(match.Value, paramFact?.Value ?? String.Empty));
				}

				var result = Context.SqlQuery<string>(fact.Query, sqlParams.ToArray());

				fact.Value = result.FirstOrDefault() ?? String.Empty;
			}
		}
	}
}
