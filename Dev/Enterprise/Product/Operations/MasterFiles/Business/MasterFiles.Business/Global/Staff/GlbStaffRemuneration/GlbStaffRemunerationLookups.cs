//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbStaffRemunerationLookups
//
//    This class should be used for overriding collections in AutoGlbStaffRemunerationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffRemunerationLookups : AutoGlbStaffRemunerationLookups
	{
		public GlbStaffRemunerationLookups(AutoGlbStaffRemuneration parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList StaffEmploymentTypes
		{
			get { return SystemDataRegistry.Instance.StaffEmploymentTypes.Value; }
		}
	}
}
