using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgProfitShareDetailsCollection : BusinessObjectCollection<OrgProfitShareDetails>
	{
		public OrgProfitShareDetailsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}

	public class OrgProfitShareDetailsDependentCollection : DependentBusinessObjectCollection<OrgProfitShareDetails, OrgAgentRelationship>
	{
		public OrgProfitShareDetailsDependentCollection(OrgAgentRelationship agentRelationship)
			: base(agentRelationship)
		{
		}

		public OrgAgentRelationship AgentRelationship
		{
			get { return Master; }
		}

		#region Retrieve Profit Share Agreement

		public OrgProfitShareDetails GetProfitShareAgreement(
			ZDateTime jobDate, ZString freightMode, ZString containerMode,
			ZString sendingPortOrCountryCode, ZString receivingPortOrCountryCode,
			OrganisationsWithTypes orgOverrides, OrgHeader controllingParty,
			string jobType = null, string gatewayAgentType = null, bool freightModeGroupagePriority = false)
		{
			var profitSharesWithSkippedClientOverrideRanking = GetProfitShares(jobDate, freightMode, containerMode,
				sendingPortOrCountryCode, receivingPortOrCountryCode, orgOverrides, controllingParty, jobType,
				gatewayAgentType, freightModeGroupagePriority: freightModeGroupagePriority);

			if (profitSharesWithSkippedClientOverrideRanking.Length == 0)
			{
				return null;
			}

			return GetProfitShares(jobDate, freightMode, containerMode,
				sendingPortOrCountryCode, receivingPortOrCountryCode, orgOverrides, controllingParty, jobType, gatewayAgentType,
				profitSharesWithSkippedClientOverrideRanking, freightModeGroupagePriority: freightModeGroupagePriority)
				.FirstOrDefault();
		}

		OrgProfitShareDetails[] GetProfitShares(ZDateTime jobDate, ZString freightMode, ZString containerMode,
			ZString sendingLocationCode, ZString receivingLocationCode, OrganisationsWithTypes orgOverrides,
			OrgHeader controllingParty, string jobType = null, string gatewayAgentType = null,
			OrgProfitShareDetails[] profitSharesWithSkippedClientOverrideRanking = null,
			bool freightModeGroupagePriority = false)
		{
			var matcher = new ProfitShareMatcher(Factory);
			var ranker = matcher.GetRanker(freightMode, containerMode,
				sendingLocationCode, receivingLocationCode, orgOverrides, controllingParty,
				jobType, gatewayAgentType, profitSharesWithSkippedClientOverrideRanking, freightModeGroupagePriority);

			if (profitSharesWithSkippedClientOverrideRanking != null)
			{
				return new[] { ranker.GetBestMatch<OrgProfitShareDetails>(Factory, new ZQuery(OrgProfitShareDetailsSchema.PK, profitSharesWithSkippedClientOverrideRanking.Select(x => x.PK).ToArray())) };
			}
			else
			{
				var results = ranker.GetBestMatches<OrgProfitShareDetails>(Factory, GetProfitShareDetailsFilter(matcher, jobDate, orgOverrides), useInMemoryFiltering: false, "", 0);
				var agentRelationshipResults = results.Where(x => x.O4_O3_OrgProfitShareHeader == AgentRelationship.PK).ToArray();
				return agentRelationshipResults.Any() ? agentRelationshipResults : results;
			}
		}

		ZQuery GetProfitShareDetailsFilter(ProfitShareMatcher profitShareMatcher, ZDateTime jobDate, OrganisationsWithTypes orgOverrides)
		{
			var query = new ZQuery();
			query.AddToFilter(OrgProfitShareDetailsSchema.O4_O3_OrgProfitShareHeader, GetAgentRelationships().Select(x => x.PK));
			profitShareMatcher.ApplyProfitShareDetailsFilter(query, jobDate, orgOverrides);

			return query;
		}

		IEnumerable<OrgAgentRelationship> GetAgentRelationships()
		{
			var query = new ZQuery();

			if (AgentRelationship.O3_OH_SendingAgent.IsValid)
			{
				query.AddToFilter(OrgAgentRelationshipSchema.O3_OH_SendingAgent, AgentRelationship.O3_OH_SendingAgent);
				query.AddToFilter(OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, null);
			}

			if (AgentRelationship.O3_OH_ReceivingAgent.IsValid)
			{
				var receivingQuery = new ZQuery(OrgAgentRelationshipSchema.O3_OH_ReceivingAgent, AgentRelationship.O3_OH_ReceivingAgent);
				receivingQuery.AddToFilter(OrgAgentRelationshipSchema.O3_OH_SendingAgent, null);
				query.AddToFilter(receivingQuery, JoinCondition.Or);
			}

			if (query.IsEmpty)
			{
				return new[] { AgentRelationship };
			}

			return Factory.Load<OrgAgentRelationship>(query).Append(AgentRelationship);
		}

		#endregion

		#region FK Column

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return OrgProfitShareDetailsSchema.O4_O3_OrgProfitShareHeader; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			OrgProfitShareDetails details = (OrgProfitShareDetails)child;

			if (AgentRelationship.ReceivingAgent != null)
			{
				details.O4_ReceivingPortOrCountry = AgentRelationship.ReceivingAgent.OH_RL_NKClosestPort;
			}

			if (AgentRelationship.SendingAgent != null)
			{
				details.O4_SendingPortOrCountry = AgentRelationship.SendingAgent.OH_RL_NKClosestPort;
			}
		}

		#endregion
	}

	#region Generic Profit Shares

	public class OrgProfitShareDetailsGenericCollection : OrgProfitShareDetailsDependentCollection
	{
		public OrgProfitShareDetailsGenericCollection(OrgAgentRelationship agentRelationship) : base(agentRelationship)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(JoinCondition.And, OrgProfitShareDetailsSchema.O4_OH_OrgOverride, SQLComparisonOperator.Equal, null);
			return filter;
		}
	}

	#endregion

	#region Client Specific Profit Shares

	public class OrgProfitShareDetailsClientSpecificCollection : OrgProfitShareDetailsDependentCollection
	{
		public OrgProfitShareDetailsClientSpecificCollection(OrgAgentRelationship agentRelationship) : base(agentRelationship)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(JoinCondition.And, OrgProfitShareDetailsSchema.O4_OH_OrgOverride, SQLComparisonOperator.NotEqual, null);
			return filter;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			bizOAdded.SetContext(BusinessContext.ClientSpecificProfitShareDetails);
		}
	}

	#endregion
}
