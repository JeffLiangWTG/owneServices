//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUS7501DocPrintingAddInfoValidation
//
//    This class should be used for overriding validation in AutoUS7501DocPrintingAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class US7501DocPrintingAddInfoValidation : AutoUS7501DocPrintingAddInfoValidation
	{
		public US7501DocPrintingAddInfoValidation(AutoUS7501DocPrintingAddInfo parent) : base(parent)
		{
		}
	}
}
