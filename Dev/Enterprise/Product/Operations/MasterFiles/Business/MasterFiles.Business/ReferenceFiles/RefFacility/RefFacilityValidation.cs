//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefFacilityValidation
//
//    This class should be used for overriding validation in AutoRefFacilityValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefFacilityValidation : AutoRefFacilityValidation
	{
		public RefFacilityValidation(AutoRefFacility parent) : base(parent)
		{
		}
	}
}
