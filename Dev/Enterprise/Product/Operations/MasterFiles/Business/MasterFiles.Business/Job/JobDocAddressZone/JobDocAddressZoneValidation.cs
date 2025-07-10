//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobDocAddressZoneValidation
//
//    This class should be used for overriding validation in AutoJobDocAddressZoneValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class JobDocAddressZoneValidation : AutoJobDocAddressZoneValidation
	{
		public JobDocAddressZoneValidation(AutoJobDocAddressZone parent) : base(parent)
		{
		}
	}
}
