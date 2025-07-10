//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgRateTariffLevelLookups
//
//    This class should be used for overriding collections in AutoOrgRateTariffLevelLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateTariffLevelLookups : AutoOrgRateTariffLevelLookups
	{
		public OrgRateTariffLevelLookups(AutoOrgRateTariffLevel parent) : base(parent)
		{
		}

		public new OrgRateTariffLevel Parent
		{
			get { return (OrgRateTariffLevel)base.Parent; }
		}

		OrgCompanyData CompanyData
		{
			get { return Parent.Parent; }
		}

		public CodeDescriptionPairList TariffLevels
		{
			get
			{
				return CompanyData != null ? CompanyData.Lookups.CompanyTariffLevels : new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList TariffTypes
		{
			get { return CachedOrgCodeLists.CompanyTariffTypes_List(Factory, Parent.P7_GC); }
		}

		OrgCodeLists CachedOrgCodeLists
		{
			get { return cachedOrgCodeLists ?? (cachedOrgCodeLists = new OrgCodeLists()); }
		}

		OrgCodeLists cachedOrgCodeLists;

		public CodeDescriptionPairList Modes
		{
			get
			{
				CodeDescriptionPairList result = CompanyData != null ? CompanyData.Lookups.GetCompanyTransportModes(Parent.P7_TariffType) : new CodeDescriptionPairList();

				if (!result.ContainsCode(OrgRateTariffLevel.ALL))
				{
					result.Insert(0, new CodeDescriptionPair(OrgRateTariffLevel.ALL, Res.GetString("bebdc920-7dab-4743-8614-7e25ae2fecbb", "All Possible Modes")));
				}

				return result;
			}
		}

		public CodeDescriptionPairList Directions
		{
			get
			{
				CodeDescriptionPairList result = CompanyData != null ? CompanyData.Lookups.GetApplicableDirections(Parent.P7_TariffType) : new CodeDescriptionPairList();

				if (!result.ContainsCode(nameof(OrgRateTariffLevel.Directions.ALL)))
				{
					result.Insert(0, new CodeDescriptionPair(nameof(OrgRateTariffLevel.Directions.ALL), Res.GetString("cd291474-1ec8-4d61-b33e-e1dae0a2dfa5", "All Directions")));
				}

				return result;
			}
		}
	}
}
