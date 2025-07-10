//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTelPreDriveChecklistEntryValidation
//
//    This class should be used for overriding validation in AutoTelPreDriveChecklistEntryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistEntryValidation : AutoTelPreDriveChecklistEntryValidation
	{
		public TelPreDriveChecklistEntryValidation(AutoTelPreDriveChecklistEntry parent) : base(parent)
		{
		}
	}
}
