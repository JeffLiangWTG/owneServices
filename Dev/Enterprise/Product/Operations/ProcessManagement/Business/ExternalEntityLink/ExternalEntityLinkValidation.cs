//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExternalEntityLinkValidation
//
//    This class should be used for overriding validation in AutoExternalEntityLinkValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.ProcessManagement.Business
{
	public class ExternalEntityLinkValidation : AutoExternalEntityLinkValidation
	{
		public ExternalEntityLinkValidation(AutoExternalEntityLink parent)
			: base(parent)
		{
		}
	}
}
