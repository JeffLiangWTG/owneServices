//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenCustomAddOnRuleAckValidation
//
//    This class should be used for overriding validation in AutoGenCustomAddOnRuleAckValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class GenCustomAddOnRuleAckValidation : AutoGenCustomAddOnRuleAckValidation
	{
		public GenCustomAddOnRuleAckValidation(AutoGenCustomAddOnRuleAck parent) : base(parent)
		{
		}
	}
}
