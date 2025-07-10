//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceJTTLookups
//
//    This class should be used for overriding collections in AutoUNDGSubstanceJTTLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceJTTLookups : AutoUNDGSubstanceJTTLookups
	{
		public UNDGSubstanceJTTLookups(AutoUNDGSubstanceJTT parent) : base(parent)
		{
		}

		public CodeDescriptionPairList JTTClassList
		{
			get { return UNDGDataItemLookups.GetDGClassList(Factory); }
		}

		public CodeDescriptionPairList PackingGroupList => UNDGSubstanceLookups.GetPackingGroupList(Factory);

		public CodeDescriptionPairList ExceptedQuantityList => UNDGSubstanceLookups.GetExceptedQuantityList(Factory);

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

		public UNDGCountryReferenceCollection UNDGCountryReferences => UNDGSubstanceLookups.GetUNDGCountryReferences(Factory);
	}
}
