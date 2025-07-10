//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTariffRelationshipViewLookups
//
//    This class should be used for overriding collections in AutoTariffRelationshipViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;

namespace Enterprise.Customs.Universal
{
	public class TariffRelationshipViewLookups : AutoTariffRelationshipViewLookups
	{
		public TariffRelationshipViewLookups(AutoTariffRelationshipView parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList TariffTypeList => RefCusTariffTypeList.GetCachedList(Factory, Parent.CountryCode);

		protected new TariffRelationshipView Parent => (TariffRelationshipView)base.Parent;
	}
}
