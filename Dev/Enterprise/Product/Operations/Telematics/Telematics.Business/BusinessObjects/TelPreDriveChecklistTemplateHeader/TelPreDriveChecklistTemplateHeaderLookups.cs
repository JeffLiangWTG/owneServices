//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTelPreDriveChecklistTemplateHeaderLookups
//
//    This class should be used for overriding collections in AutoTelPreDriveChecklistTemplateHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistTemplateHeaderLookups : AutoTelPreDriveChecklistTemplateHeaderLookups
	{
		public TelPreDriveChecklistTemplateHeaderLookups(AutoTelPreDriveChecklistTemplateHeader parent) : base(parent)
		{
		}
	}
}
