//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefFacilityLocalCodeValidation
//
//    This class should be used for overriding validation in AutoRefFacilityLocalCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefFacilityLocalCodeValidation : AutoRefFacilityLocalCodeValidation
	{
		public RefFacilityLocalCodeValidation(AutoRefFacilityLocalCode parent) : base(parent)
		{
		}
	}
}
