//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTariffUOMViewLookups
//
//    This class should be used for overriding collections in AutoTariffUOMViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class TariffUOMViewLookups : AutoTariffUOMViewLookups
	{
		public TariffUOMViewLookups(AutoTariffUOMView parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TypeList => Parent.ZZ8_IsSystem ? new CodeDescriptionPairList() : Factory.GetCachedValue<UOMTypeList>();

		public CodeDescriptionPairList UOMList => Parent.ZZ8_IsSystem ? new CodeDescriptionPairList() : RefCusCodeListTypes.GetCachedList(Factory, Parent.CusTariff.ZZ1_ZZZ_NKDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);

		protected new TariffUOMView Parent => (TariffUOMView)base.Parent;
	}
}
