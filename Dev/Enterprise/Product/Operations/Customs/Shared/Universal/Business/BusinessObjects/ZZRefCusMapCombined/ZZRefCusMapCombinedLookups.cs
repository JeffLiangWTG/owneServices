//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusMapCombinedLookups
//
//    This class should be used for overriding collections in AutoZZRefCusMapCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusMapCombinedLookups : AutoZZRefCusMapCombinedLookups
	{
		public ZZRefCusMapCombinedLookups(AutoZZRefCusMapCombined parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList MapTypesList
		{
			get { return RefCusMapTypeList.GetListOfEditableTypes(Factory); }
		}

		public IBusinessObjectCollection CountryOrGroupingList
		{
			get { return new RefCountryCollection(Factory); } // TODO: change this to a collection which contains both countries and group
		}
	}
}
