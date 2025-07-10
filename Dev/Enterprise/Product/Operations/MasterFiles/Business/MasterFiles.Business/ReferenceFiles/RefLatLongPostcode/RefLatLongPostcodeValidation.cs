//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefLatLongPostcodeValidation
//
//    This class should be used for overriding validation in AutoRefLatLongPostcodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefLatLongPostcodeValidation : AutoRefLatLongPostcodeValidation
	{
		public RefLatLongPostcodeValidation(AutoRefLatLongPostcode parent) : base(parent)
		{
		}
	}
}
