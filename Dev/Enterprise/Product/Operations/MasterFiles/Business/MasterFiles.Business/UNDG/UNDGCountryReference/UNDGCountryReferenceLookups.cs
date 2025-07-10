//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGCountryReferenceLookups
//
//    This class should be used for overriding collections in AutoUNDGCountryReferenceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGCountryReferenceLookups : AutoUNDGCountryReferenceLookups
	{
		public UNDGCountryReferenceLookups(AutoUNDGCountryReference parent) : base(parent)
		{
		}

		public static CodeDescriptionPairList Types
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.Add(new CodeDescriptionPair(Core.Constants.UNDGCountryReference.Type.PSA, Core.Constants.UNDGCountryReference.TypeDescription.PSA));
				result.Add(new CodeDescriptionPair(Core.Constants.UNDGCountryReference.Type.ICPE, Core.Constants.UNDGCountryReference.TypeDescription.ICPE));
				return result;
			}
		}
	}
}
