//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTelPreDriveChecklistHeaderValidation
//
//    This class should be used for overriding validation in AutoTelPreDriveChecklistHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistHeaderValidation : AutoTelPreDriveChecklistHeaderValidation
	{
		public TelPreDriveChecklistHeaderValidation(AutoTelPreDriveChecklistHeader parent) : base(parent)
		{
		}
	}
}
