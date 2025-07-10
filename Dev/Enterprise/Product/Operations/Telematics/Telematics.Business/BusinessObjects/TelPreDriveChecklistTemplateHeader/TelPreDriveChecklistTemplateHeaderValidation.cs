//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTelPreDriveChecklistTemplateHeaderValidation
//
//    This class should be used for overriding validation in AutoTelPreDriveChecklistTemplateHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistTemplateHeaderValidation : AutoTelPreDriveChecklistTemplateHeaderValidation
	{
		public TelPreDriveChecklistTemplateHeaderValidation(AutoTelPreDriveChecklistTemplateHeader parent) : base(parent)
		{
		}
	}
}
