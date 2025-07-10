//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoVoyageTemplatePortCallValidation
//
//    This class should be used for overriding validation in AutoVoyageTemplatePortCallValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class VoyageTemplatePortCallValidation : AutoVoyageTemplatePortCallValidation
	{
		public VoyageTemplatePortCallValidation(AutoVoyageTemplatePortCall parent) : base(parent)
		{
		}
	}
}
