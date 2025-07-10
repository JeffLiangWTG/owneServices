//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryLineLookups
//
//    This class should be used for overriding collections in AutoCusEntryLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusEntryLineLookups : AutoCusEntryLineLookups
	{
		public CusEntryLineLookups(AutoCusEntryLine parent)
			: base(parent)
		{
		}

		public virtual EntryLineStatusList EntryLineStatusList
		{
			get { return Factory.GetCachedValue<EntryLineStatusList>(); }
		}

		public virtual CodeDescriptionPairList FlatAmountUQList
		{
			get
			{
				return Factory.GetCachedValue("BaseCusEntryLine.FlatAmountUQList", () =>
					{
						var result = new CodeDescriptionPairList(OLookUpEditType.Weight);
						result.AddRange(new CodeDescriptionPairList(OLookUpEditType.Volume));
						return result;
					});
			}
		}
	}
}
