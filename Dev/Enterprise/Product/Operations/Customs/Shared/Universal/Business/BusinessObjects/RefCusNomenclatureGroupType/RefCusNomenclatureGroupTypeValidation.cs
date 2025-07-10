//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusNomenclatureGroupTypeValidation
//
//    This class should be used for overriding validation in AutoRefCusNomenclatureGroupTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusNomenclatureGroupTypeValidation : AutoRefCusNomenclatureGroupTypeValidation
	{
		public RefCusNomenclatureGroupTypeValidation(AutoRefCusNomenclatureGroupType parent) : base(parent)
		{
		}
	}
}
