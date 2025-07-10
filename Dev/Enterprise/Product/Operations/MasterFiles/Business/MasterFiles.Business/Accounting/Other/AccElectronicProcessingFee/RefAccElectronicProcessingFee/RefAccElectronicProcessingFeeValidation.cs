//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefAccElectronicProcessingFeeValidation
//
//    This class should be used for overriding validation in AutoRefAccElectronicProcessingFeeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefAccElectronicProcessingFeeValidation : AutoRefAccElectronicProcessingFeeValidation
	{
		public RefAccElectronicProcessingFeeValidation(AutoRefAccElectronicProcessingFee parent) : base(parent)
		{
		}
	}
}
