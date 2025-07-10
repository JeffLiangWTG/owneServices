//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUNDGSubstanceADRLookups
//
//    This class should be used for overriding collections in AutoUNDGSubstanceADRLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstanceADRLookups : AutoUNDGSubstanceADRLookups
	{
		public UNDGSubstanceADRLookups(AutoUNDGSubstanceADR parent) : base(parent)
		{
		}

		#region ADRClassList

		public CodeDescriptionPairList ADRClassList
		{
			get { return UNDGDataItemLookups.GetDGClassList(Factory); }
		}

		#endregion

		#region Packing Group

		public CodeDescriptionPairList PackingGroupList => UNDGSubstanceLookups.GetPackingGroupList(Factory);

		#endregion

		#region Excepted Quantities

		public CodeDescriptionPairList ExceptedQuantityList => UNDGSubstanceLookups.GetExceptedQuantityList(Factory);

		#endregion

		#region Languages

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

		#endregion

		public UNDGCountryReferenceCollection UNDGCountryReferences => UNDGSubstanceLookups.GetUNDGCountryReferences(Factory);
	}
}
