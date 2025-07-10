using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CusAuthorizationUsage : EU.NCTS.Business.CusAuthorizationUsage, Integration.Customs.PL.INctsCusAuthorizationUsage
{
	public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("ccac21ef-4798-405e-9240-a96b32b3e4dd", Caption = "Authorization Location", MediumCaption = "Auth. Location", ShortCaption = "Location")]
	[List(nameof(Lookups) + "." + nameof(CusAuthorizationUsageLookups.AuthorisationRuleList))]
	public override ZString AGC_Location
	{
		get => base.AGC_Location;
		set => base.AGC_Location = value;
	}

	public NctsHeader NctsHeader => (NctsHeader)base.Header;

	protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);

	public new CusAuthorizationUsageLookups Lookups => (CusAuthorizationUsageLookups)base.Lookups;

	protected override EU.NCTS.Business.CusAuthorizationUsagePhase5Validation GetNewPhase5Validation() => new CusAuthorizationUsagePhase5Validation(this);

	protected override void SetAGC_Location(CusAuthorisationHeader authorisationHeader)
	{
		if (authorisationHeader != null && AGC_Location.IsEmpty)
		{
			var list = authorisationHeader.CusAuthorisationRules.Where(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location).ToArray();
			if (list.Length >= 1)
			{
				AGC_Location = list[0].CPR_ValueFrom.Left(17);
			}
		}
	}

	protected override void DefaultAGC_Location()
	{
		if (AGC_Location.IsEmpty)
		{
			var number = AGC_Number;
			if (!number.IsEmpty)
			{
				var numberList = Lookups.NumberList.Where(x => x.CPH_Number == number).ToArray();
				if (numberList.Length == 1)
				{
					SetAGC_Location(numberList[0]);
				}
			}
		}
	}
}
