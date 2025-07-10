//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationGroupLookups
//
//    This class should be used for overriding collections in AutoGlbAccreditationGroupLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationGroupLookups : AutoGlbAccreditationGroupLookups
	{
		public GlbAccreditationGroupLookups(AutoGlbAccreditationGroup parent) : base(parent)
		{
		}

		public GlbAccreditationCollection MainAccreditationList
		{
			get
			{
				return Factory.GetCachedValue("GlbAccreditationLookups.GlbAccreditationCollection", () => new GlbAccreditationCollection(Factory, new ZQuery(GlbAccreditationSchema.HAC_IsRefresher, false)));
			}
		}
	}
}
