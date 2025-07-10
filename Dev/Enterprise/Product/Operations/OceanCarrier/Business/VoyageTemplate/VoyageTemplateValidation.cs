//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoVoyageTemplateValidation
//
//    This class should be used for overriding validation in AutoVoyageTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.OceanCarrier.Business
{
	public sealed class VoyageTemplateValidation : AutoVoyageTemplateValidation
	{
		public VoyageTemplateValidation(AutoVoyageTemplate parent) : base(parent)
		{
		}
	}
}
