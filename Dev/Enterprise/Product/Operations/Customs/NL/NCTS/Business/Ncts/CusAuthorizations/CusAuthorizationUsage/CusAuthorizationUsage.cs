using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CusAuthorizationUsage : EU.NCTS.Business.CusAuthorizationUsage
{
	public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override ZString AGC_Number
	{
		get => base.AGC_Number;
		set
		{
			base.AGC_Number = value;
			SetDefaultValuesForGoodsLocation();
		}
	}

	void SetDefaultValuesForGoodsLocation()
	{
		if (Parent is NctsDepartureMovementHeader parent)
		{
			parent.GoodsLocation.SetDefaultsFromAuthorizationIfNeeded(CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(Factory, AGC_Number, AGC_Code, Header.CountryCode).FirstOrDefault());
			parent.GoodsLocationDescriptionInfo.RefreshBinding();
		}
	}
}
