//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefVesselZZValidation
//
//    This class should be used for overriding validation in AutoRefVesselZZValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefVesselZZValidation : AutoRefVesselZZValidation
	{
		public RefVesselZZValidation(AutoRefVesselZZ parent) : base(parent)
		{
		}
	}
}
