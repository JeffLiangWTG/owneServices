//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusLineTariffDetailLookups
//
//    This class should be used for overriding collections in AutoCusLineTariffDetailLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusLineTariffDetailLookups : AutoCusLineTariffDetailLookups
	{
		public CusLineTariffDetailLookups(AutoCusLineTariffDetail parent)
			: base(parent)
		{
		}

		public new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

		public virtual ICodeDescriptionPairList TariffTypeList => new CodeDescriptionPairList();

		public virtual ICollection QuantityUnitList => new CodeDescriptionPairList();
	}
}
