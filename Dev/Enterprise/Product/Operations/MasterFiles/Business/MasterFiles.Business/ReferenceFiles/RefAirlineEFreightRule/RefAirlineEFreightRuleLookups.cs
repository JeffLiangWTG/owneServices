using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineEFreightRuleLookups : AutoRefAirlineEFreightRuleLookups
	{
		public RefAirlineEFreightRuleLookups(AutoRefAirlineEFreightRule parent)
			: base(parent)
		{
		}

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region EFreightStatus_List

		public CodeDescriptionPairList EFreightStatus_List
		{
			get
			{
				return Factory.GetCachedValue("EFreightStatus_List",
					() => new CodeDescriptionPairList(OLookUpEditType.EFreightStatus));
			}
		}

		#endregion
	}
}
