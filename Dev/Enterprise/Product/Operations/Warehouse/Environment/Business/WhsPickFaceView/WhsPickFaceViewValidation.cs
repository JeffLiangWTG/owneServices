//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsPickFaceViewValidation
//
//    This class should be used for overriding validation in AutoWhsPickFaceViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceViewValidation : AutoWhsPickFaceViewValidation
	{
		public WhsPickFaceViewValidation(AutoWhsPickFaceView parent)
			: base(parent)
		{
		}
	}
}
