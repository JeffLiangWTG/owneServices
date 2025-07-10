//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTelPreDriveChecklistTemplateEntryValidation
//
//    This class should be used for overriding validation in AutoTelPreDriveChecklistTemplateEntryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistTemplateEntryValidation : AutoTelPreDriveChecklistTemplateEntryValidation
	{
		public TelPreDriveChecklistTemplateEntryValidation(AutoTelPreDriveChecklistTemplateEntry parent) : base(parent)
		{
		}
	}
}
