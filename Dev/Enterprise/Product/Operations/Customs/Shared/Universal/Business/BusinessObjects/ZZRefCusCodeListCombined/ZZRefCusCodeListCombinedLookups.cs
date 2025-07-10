//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusCodeListCombinedLookups
//
//    This class should be used for overriding collections in AutoZZRefCusCodeListCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusCodeListCombinedLookups : AutoZZRefCusCodeListCombinedLookups
	{
		public ZZRefCusCodeListCombinedLookups(AutoZZRefCusCodeListCombined parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList CodeTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (!Parent.ZZD_CountryOrGrouping.IsEmpty && !Parent.ZZD_CountryOrGroupingInfo.HasErrors())
				{
					result = RefCusCodeTypeList.GetListByCountry(Factory, Parent.ZZD_CountryOrGrouping, !Parent.ZZD_IsSystem);
				}
				return result;
			}
		}

		public IBusinessObjectCollection CountryOrGroupingList => new RefDataGroupingCollection(Factory);

		protected new ZZRefCusCodeListCombined Parent => (ZZRefCusCodeListCombined)base.Parent;
	}
}
