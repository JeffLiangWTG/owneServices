//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTariffRelationshipViewValidation
//
//    This class should be used for overriding validation in AutoTariffRelationshipViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class TariffRelationshipViewValidation : AutoTariffRelationshipViewValidation
	{
		public TariffRelationshipViewValidation(AutoTariffRelationshipView parent) : base(parent)
		{
		}
	}
}
