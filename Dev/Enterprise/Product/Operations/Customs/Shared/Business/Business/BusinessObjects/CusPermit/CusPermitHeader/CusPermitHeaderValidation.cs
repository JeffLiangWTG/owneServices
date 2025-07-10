//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusPermitHeaderValidation
//
//    This class should be used for overriding validation in AutoCusPermitHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusPermitHeaderValidation : AutoCusPermitHeaderValidation
	{
		public CusPermitHeaderValidation(AutoCusPermitHeader parent)
			: base(parent)
		{
		}

		public new CommonCusPermitHeader Parent => (CommonCusPermitHeader)base.Parent;
	}
}
