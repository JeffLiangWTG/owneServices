//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceRIDLookups
//
//    This class should be used for overriding collections in AutoUNDGSubstanceRIDLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceRIDLookups : AutoUNDGSubstanceRIDLookups
	{
		public UNDGSubstanceRIDLookups(AutoUNDGSubstanceRID parent) : base(parent)
		{
		}

		public CodeDescriptionPairList RIDClassList
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
