using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;

namespace CargoWise.eHub.Core.PipelineComponents.Tools
{
	public class RoutingRuleSqlFactResolver : IFactResolver
	{
		public RoutingRuleSqlFactResolver(eHubTransactionsContext context)
		{
			this.context = context;
		}

		eHubTransactionsContext context;

		public void Resolve(Fact[] facts)
		{
			foreach (var fact in facts.Where(f => f.Type == "SQL"))
			{
				var paramNames = Regex.Matches(fact.Query, @"@\w+");
				List<object> sqlParams = new List<object>();
				foreach (Match match in paramNames)
				{
					var paramFact = facts.FirstOrDefault(f => ('@' + f.Name) == match.Value);
					sqlParams.Add(new SqlParameter(match.Value, paramFact.Value ?? String.Empty));
				}
				var result = context.SqlQuery<string>(fact.Query, sqlParams.ToArray());

				fact.Value = result.FirstOrDefault() ?? String.Empty;
			}
		}
	}
}
