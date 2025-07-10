//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusTariffRelationshipValidation
//
//    This class should be used for overriding validation in AutoRefCusTariffRelationshipValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusTariffRelationshipValidation : AutoRefCusTariffRelationshipValidation
	{
		public RefCusTariffRelationshipValidation(AutoRefCusTariffRelationship parent)
			: base(parent)
		{
		}
	}
}
