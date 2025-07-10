using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class BondedFactoryLookups : JobDocAddressLookups
	{
		public BondedFactoryLookups(AutoJobDocAddress parent)
			: base(parent)
		{
		}

		public BondedFactoryOrgHeaderCollection Organisations
			=> Factory.GetCachedValue("TW|BondedFactory|Organisations", () => new BondedFactoryOrgHeaderCollection(Factory));

		public CodeDescriptionPairList CustomsCodesList => Factory.GetCachedValue("TW|BondedFactory|CustomsCodesList", () => new OrgCodeLists().CustomsCodes_List(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Taiwan)));
	}
}
