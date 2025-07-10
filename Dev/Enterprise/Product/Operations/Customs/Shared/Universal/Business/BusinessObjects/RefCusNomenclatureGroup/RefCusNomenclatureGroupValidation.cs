//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusNomenclatureGroupValidation
//
//    This class should be used for overriding validation in AutoRefCusNomenclatureGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusNomenclatureGroupValidation : AutoRefCusNomenclatureGroupValidation
	{
		public RefCusNomenclatureGroupValidation(AutoRefCusNomenclatureGroup parent) : base(parent)
		{
		}
	}
}
