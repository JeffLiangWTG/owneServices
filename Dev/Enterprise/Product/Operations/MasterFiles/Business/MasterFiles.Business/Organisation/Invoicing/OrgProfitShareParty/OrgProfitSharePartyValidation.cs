using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using PartyTypeCodes = Enterprise.MasterFiles.Business.OrgProfitSharePartyLookups.PartyTypeCodes;

namespace Enterprise.MasterFiles.Business
{
	public class OrgProfitSharePartyValidation : AutoOrgProfitSharePartyValidation
	{
		public OrgProfitSharePartyValidation(AutoOrgProfitShareParty parent) : base(parent)
		{
		}

		#region PS_PartyType

		protected override void CheckPS_PartyType()
		{
			base.CheckPS_PartyType();
			MandatoryValidation.CheckEntered(Parent.PS_PartyTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PS_PartyTypeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PS_PartyTypeInfo, Parent.Factory.Load<OrgProfitShareParty>(new ZQuery(OrgProfitSharePartySchema.PS_O4, Parent.PS_O4)));
			ValidateHeadOfficePartyType();
		}

		void ValidateHeadOfficePartyType()
		{
			if (Parent.PS_PartyType == PartyTypeCodes.HeadOfficeFranchisor &&
					Parent.ProfitShareDetails != null && Parent.ProfitShareDetails.OrgProfitShareHeader != null &&
					Parent.ProfitShareDetails.OrgProfitShareHeader.GroupNetworkOrFranchise == null)
			{
				Parent.PS_PartyTypeInfo.AddError(Res.GetString("c956670b-a283-4f88-88ea-1bec7412979b", "You can use '{0}' type only if 'Head Office' is set", Parent.PS_PartyType));
			}
		}

		#endregion

		#region PS_PartyRateBasis

		protected override void CheckPS_PartyRateBasis()
		{
			base.CheckPS_PartyRateBasis();
			if (!Parent.PS_PartyRate.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.PS_PartyRateBasisInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.PS_PartyRateBasisInfo);

			if (!Parent.PS_PartyRate.IsEmpty && Parent.PS_PartyRateBasis == OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue)
			{
				Parent.PS_PartyRateBasisInfo.AddError(Res.GetString("efa766a4-4017-489c-bac1-fba0b3a5b368", "You can only specify 'Percentage of Gross Revenue' if you specify a percentage based profit share, and not a per unit profit share."));
			}
		}

		#endregion

		#region PS_PartyRate

		protected override void CheckPS_PartyRate()
		{
			base.CheckPS_PartyRate();
			ValidatePS_PartyRateBasis();
		}

		#endregion

		#region PS_PartyProfitSharePercent

		protected override void CheckPS_PartyProfitSharePercent()
		{
			base.CheckPS_PartyProfitSharePercent();

			if (Parent.ProfitShareDetails.OrgProfitShareHeader?.AgentAgreement ?? false)
			{
				var generalPartyDetails = Parent.ProfitShareDetails.PartyDetails
					.OfType<OrgProfitShareParty>()
					.Where(pd => PartyTypeCodes.GeneralTypes.Contains(pd.PS_PartyType))
					.ToList();
				CheckTotalPercentageAndAddError(GetTotalPercentage(generalPartyDetails));
				ValidateAllPartiesInTheSameGrid(generalPartyDetails);
			}
			else
			{
				// Not Standard Agent Agreement: Single Agency Profile for now, we don't care about the total in the general tab.
				CompareValidation.CheckNumberGreaterThanZero(Parent.PS_PartyProfitSharePercentInfo);
				CompareValidation.CheckLessThanOrEqualTo(Parent.PS_PartyProfitSharePercentInfo, 100m);
				// G/W Consol Profit Redistribution tab however still requires to have a sum of total 100%.
				if (PartyTypeCodes.GatewayConsolProfitRedistributionTypes.Contains(Parent.PS_PartyType))
				{
					var partyDetails = Parent.ProfitShareDetails.PartyDetailsForGatewayProfitShareRedistribution
						.OfType<OrgProfitShareParty>()
						.Where(party => PartyTypeCodes.GatewayConsolProfitRedistributionTypes.Contains(party.PS_PartyType))
						.ToList();
					CheckTotalPercentageAndAddError(partyDetails.Sum(p => p.PS_PartyProfitSharePercent));
					ValidateAllPartiesInTheSameGrid(partyDetails);
				}
			}
		}

		void CheckTotalPercentageAndAddError(ZDecimal totalPercentage)
		{
			if (totalPercentage != 100m && totalPercentage != 0m && !Parent.ReadOnly)
			{
				Parent.PS_PartyProfitSharePercentInfo.AddError(Res.GetString("826ebd74-5594-4847-86d3-791da8ecd4d4", "The profit share percentages entered do not add to 100."));
			}
		}

		void ValidateAllPartiesInTheSameGrid(List<OrgProfitShareParty> parties)
		{
			foreach (var party in parties)
			{
				// Property info's HasValidationBeenRun can prevent infinite recursive
				party.Validation.ValidatePS_PartyProfitSharePercent();
			}
		}

		static ZDecimal GetTotalPercentage(List<OrgProfitShareParty> generalPartyDetails)
		{
			ZDecimal result = 0m;
			foreach (var party in generalPartyDetails)
			{
				if ((party.PS_PartyType == PartyTypeCodes.ControllingAgent &&
					party.ProfitShareDetails.ControllingAgentIsDifferentToSendingAndReceiving) ||
					party.PS_PartyType != PartyTypeCodes.ControllingAgent)
				{
					result += party.PS_PartyProfitSharePercent;
				}
			}

			return result;
		}

		#endregion
	}
}
