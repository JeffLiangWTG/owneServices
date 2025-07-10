//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoSGCusClassPartPivotAddInfoValidation
//
//    This class should be used for overriding validation in AutoSGCusClassPartPivotAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGCusClassPartPivotAddInfoValidation : AutoSGCusClassPartPivotAddInfoValidation
	{
		public SGCusClassPartPivotAddInfoValidation(AutoSGCusClassPartPivotAddInfo parent)
			: base(parent)
		{
		}
	}
}
