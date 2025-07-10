//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCarrierAttributeCombinedLookups
//
//    This class should be used for overriding collections in AutoZZRefCarrierAttributeCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCarrierAttributeCombinedLookups : AutoZZRefCarrierAttributeCombinedLookups
	{
		public ZZRefCarrierAttributeCombinedLookups(AutoZZRefCarrierAttributeCombined parent)
			: base(parent)
		{ }

		protected new ZZRefCarrierAttributeCombined Parent => (ZZRefCarrierAttributeCombined)base.Parent;

		public ICodeDescriptionPairList NameList
		{
			get
			{
				CodeDescriptionPairList result;
				var country = Parent.Carrier?.ZZ4_CountryOrGrouping ?? ZString.Empty;
				if (country.IsEmpty)
				{
					result = new CodeDescriptionPairList();
				}
				else
				{
					result = Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "RefCarrierCodeAttributeName_{0}", country), () =>
					{
						var nameList = new CodeDescriptionPairList();
						foreach (var attibute in RefCarrierHelper.GetAttributes(Factory, country))
						{
							nameList.AddPairIfNotExist(attibute.ZZG_Name, attibute.ZZG_Name);
						}
						return nameList;
					});
				}

				return result;
			}
		}
	}
}
