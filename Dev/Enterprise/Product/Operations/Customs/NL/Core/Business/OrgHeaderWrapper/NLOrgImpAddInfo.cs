using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class NLOrgImpAddInfo : AutoNLOrgImpAddInfo, Integration.Customs.NL.IOrgImpAddInfo
{
	public NLOrgImpAddInfo(BusinessObjectFactory factory) : base(factory)
	{
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			Deserialise();
		}
	}

	public NLOrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
		: base(parentPropertyInfo.BizObj.Factory)
	{
		ParentPropertyInfo = parentPropertyInfo;
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			Deserialise();
		}
	}

	[List(nameof(Lookups) + "." + nameof(NLOrgImpAddInfoLookups.VATDefermentList))]
	public override ZString ZO_VATDeferment
	{
		get => base.ZO_VATDeferment;
		set => base.ZO_VATDeferment = value;
	}

	public static NLOrgImpAddInfo Get(OrgHeader organisation) => (NLOrgImpAddInfo)organisation?.GetCountryData(Core.Constants.CountryCodes.Netherlands).ImpAddInfo;
}
