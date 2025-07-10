using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.Business.CusAuthorisationHeader;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CusAuthorizationUsagePhase5Validation : EU.NCTS.Business.CusAuthorizationUsagePhase5Validation
{
	new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

	public CusAuthorizationUsagePhase5Validation(CusAuthorizationUsage parent) : base(parent)
	{
	}

	protected override void CheckAGC_CodeIsInvalid(ZPropertyInfo agcCodeInfo) => ListValidation.MessageErrorIfInvalidCode(agcCodeInfo, ResString.GetMultilingualString("fdca9982-11bc-482e-9e14-a57f234308f8", "Entered Authorization code is not in the list."));

	protected override void CheckAGC_Location()
	{
		base.CheckAGC_Location();
		var parent = Parent;

		if (parent is CusAuthorizationUsage authorizationUsage
			&& authorizationUsage.Header is NctsHeader header
			&& header.IsPhase5Departure)
		{
			var location = parent.AGC_Location;
			if (location.IsEmpty)
			{
				parent.AGC_LocationInfo.AddMessageError(Res.GetString("51473a0f-2041-48c4-b663-a995b7c65d1d", "You have not selected Location."));
			}
			else
			{
				var filter = new CusAuthorisationHeaderQueryBuilder(header.CountryCode);
				var authorisationNumber = parent.AGC_Number;
				if (!authorisationNumber.IsEmpty)
				{
					filter.AddAuthorisationNumberFilter(authorisationNumber);
				}
				var code = parent.AGC_Code;
				if (!code.IsEmpty)
				{
					filter.AddTypeFilter(code);
				}
				var owner = parent.AGC_OH_Owner;
				if (!owner.IsEmpty)
				{
					filter.AddPermitHoldersFilter(owner);
				}
				filter.AddTransactionDateFilter(ZDateTime.Today);
				var query = filter.AddRuleFilter(filter.Build(), CusAuthorisationRuleTypeList.Codes.Location, location);
				if (!parent.Factory.Exists(typeof(CusAuthorisationHeader), query))
				{
					parent.AGC_LocationInfo.AddMessageError(Res.GetString("005f4738-d118-4b5d-9e2b-7521b80dbe4c", "The Location you have selected is not in the list."));
				}
			}
		}
	}
}
