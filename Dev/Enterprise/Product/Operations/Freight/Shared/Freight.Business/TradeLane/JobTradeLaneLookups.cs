//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobTradeLaneLookups
//
//    This class should be used for overriding collections in AutoJobTradeLaneLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class JobTradeLaneLookups : AutoJobTradeLaneLookups
	{
		public JobTradeLaneLookups(AutoJobTradeLane parent)
			: base(parent)
		{
		}

		#region Locations

		public LocationCollection Locations
		{
			get { return new LocationCollection(Factory); }
		}

		#endregion

		#region DirectionTypes

		public CodeDescriptionPairList DirectionTypes
		{
			get { return new DirectionTypeList(); }
		}

		#endregion

		#region RelatedOrgs

		public override OrgHeaderCollection RelatedOrgs
		{
			get { return new ShipsAgencyPrincipalCollection(Factory); }
		}

		#endregion

	}
}
