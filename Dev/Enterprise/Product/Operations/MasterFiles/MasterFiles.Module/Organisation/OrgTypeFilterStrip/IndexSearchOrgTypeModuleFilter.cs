using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class IndexSearchOrgTypeModuleFilter : OrgTypeModuleFilter, IIndexSearchModuleFilter
	{
		public IndexSearchOrgTypeModuleFilter(ZString description) : base(description)
		{
		}

		public IGlowQuery GetGlowIndexQuery()
		{
			var checkboxQuery = new List<IGlowQuery>();
			if (Property0)
			{
				checkboxQuery.Add(new EqualQuery(new Term("CWDefaultHiddenOrgCompanyPKWithIsDebtor", GlbCompany.CurrentCompany.PK.ToString()), useQuotes: false));
			}

			if (Property1)
			{
				checkboxQuery.Add(new EqualQuery(new Term("CWDefaultHiddenOrgCompanyPKWithIsCreditor", GlbCompany.CurrentCompany.PK.ToString()), useQuotes: false));
			}

			var orgTypeDic = new Dictionary<string, bool>()
			{
				{ "IsConsignee", Property2 },
				{ "IsConsignor", Property3 },
				{ "IsCarrier", Property4 },
				{ "IsForwarder", Property5 },
				{ "IsTransportClient", Property6 },
				{ "IsWarehouseClient", Property7 },
				{ "IsBroker", Property8 },
				{ "IsServices", Property9 },
				{ "IsCompetitor", Property10 },
				{ "IsSales", Property11 },
				{ "IsControllingAgent", Property12 },
				{ "IsControllingCustomer", Property13 },
			};

			foreach (var item in orgTypeDic)
			{
				if (item.Value)
				{
					checkboxQuery.Add(new EqualQuery(new Term($"OrganizationType", item.Key), useQuotes: true, allExact: true));
				}
			}

			if (AndJoinCondition)
			{
				return new BooleanQuery(BooleanOperator.And, checkboxQuery.ToArray());
			}
			else if (OrJoinCondition)
			{
				return new BooleanQuery(BooleanOperator.Or, checkboxQuery.ToArray());
			}
			return null;
		}
	}
}
