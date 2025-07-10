//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusNomenclatureGroupNoteValidation
//
//    This class should be used for overriding validation in AutoRefCusNomenclatureGroupNoteValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusNomenclatureGroupNoteValidation : AutoRefCusNomenclatureGroupNoteValidation
	{
		public RefCusNomenclatureGroupNoteValidation(AutoRefCusNomenclatureGroupNote parent) : base(parent)
		{
		}
	}
}
