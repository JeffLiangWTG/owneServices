//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGAttributeZZLookups
//
//    This class should be used for overriding collections in AutoUNDGAttributeZZLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGAttributeZZLookups : AutoUNDGAttributeZZLookups
	{
		public UNDGAttributeZZLookups(AutoUNDGAttributeZZ parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Languages
		{
			get
			{
				return Factory.GetCachedValue("UNDGAttribute_Languages", () =>
				{
					return new CodeDescriptionPairList(OLookUpEditType.Language);
				});
			}
		}
	}
}
