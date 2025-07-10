using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgProfitSharePartyCollection : DependentBusinessObjectCollection<OrgProfitShareParty, OrgProfitShareDetails>
	{
		protected OrgProfitSharePartyCollection(OrgProfitShareDetails profitShareDetails) : base(profitShareDetails)
		{
		}

		#region Get Party

		public OrgProfitShareParty GetParty(string partyType)
		{
			var query = new ZQuery(OrgProfitSharePartySchema.PS_PartyType, partyType);
			var results = Find(query);
			return results.Length > 0 ? (OrgProfitShareParty)results[0] : null;
		}

		#endregion

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = new ZQuery();
			query.AddToFilter(OrgProfitSharePartySchema.PS_PartyType, OrgProfitShareTypes);
			return query;
		}

		protected abstract HashSet<string> OrgProfitShareTypes { get; }
		internal abstract string OrgProfitShareType { get; }

		public const string OrgProfitShareTypeGeneral = "GEN";
		public const string OrgProfitShareTypeGatewayConsolProfitRedistribution = "GPR";
		public const string OrgProfitShareTypeChargeCodeWithType = "CCT";
	}

	/// <summary>
	/// Collection of OrgProfitShareParty(s) to show on General grids.
	/// </summary>
	public class OrgProfitSharePartyGeneralCollection : OrgProfitSharePartyCollection
	{
		public OrgProfitSharePartyGeneralCollection(OrgProfitShareDetails profitShareDetails) : base(profitShareDetails)
		{
		}

		protected override HashSet<string> OrgProfitShareTypes => OrgProfitSharePartyLookups.PartyTypeCodes.GeneralTypes;

		internal override string OrgProfitShareType => OrgProfitShareTypeGeneral;
	}

	/// <summary>
	/// Collection of OrgProfitShareParty(s) to show on G/W Consol Profit Share Redistribution grids.
	/// </summary>
	public class OrgProfitSharePartyRedistributionCollection : OrgProfitSharePartyCollection
	{
		public OrgProfitSharePartyRedistributionCollection(OrgProfitShareDetails profitShareDetails) : base(profitShareDetails)
		{
		}

		protected override HashSet<string> OrgProfitShareTypes => OrgProfitSharePartyLookups.PartyTypeCodes.GatewayConsolProfitRedistributionTypes;

		internal override string OrgProfitShareType => OrgProfitShareTypeGatewayConsolProfitRedistribution;
	}
}