using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsPLOfficeCodePhase5DepartureValidation(NctsPLOfficeCode parent) : EU.NCTS.Business.NctsEuOfficeCodePhase5DepartureValidation(parent)
{
	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		CheckRuleRP57();
	}

	void CheckRuleRP57()
	{
		var parent = Parent;
		var movementHeader = parent.MovementHeader;
		if (parent.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture
			&& !parent.OfficeCountryCode.EqualsIgnoringCase(Core.Constants.CountryCodes.Poland))
		{
			parent.CY_DataInfo.AddMessageError(Res.GetString("245BDBF3-6184-45A3-96CD-09300B67317E", "[RP57] Customs office of exit (DEP) must start with 'PL'."));
		}
	}
}
