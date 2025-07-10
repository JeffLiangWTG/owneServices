//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDrawbackNAFTAAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSDrawbackNAFTAAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USDrawbackNAFTAAddInfoLookups : AutoUSDrawbackNAFTAAddInfoLookups
	{
		public USDrawbackNAFTAAddInfoLookups(AutoUSDrawbackNAFTAAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList US_NAFTACountryCodeList
		{
			get
			{
				return Factory.GetCachedValue("US_NAFTACountryCodeList", // 'US_NAFTACountryCodeList' is not a database field
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.CountryCodes.Canada, "Canada");
						result.AddPair(Core.Constants.CountryCodes.Mexico, "Mexico");
						return result;
					}
				);
			}
		}
	}
}
