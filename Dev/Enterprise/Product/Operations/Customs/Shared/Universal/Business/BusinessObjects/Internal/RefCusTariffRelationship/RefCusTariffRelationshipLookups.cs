//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffRelationshipLookups
//
//    This class should be used for overriding collections in AutoRefCusTariffRelationshipLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffRelationshipLookups : AutoRefCusTariffRelationshipLookups
	{
		public RefCusTariffRelationshipLookups(AutoRefCusTariffRelationship parent)
			: base(parent)
		{
		}

		protected new RefCusTariffRelationship Parent
		{
			get { return (RefCusTariffRelationship)base.Parent; }
		}
	}
}
