//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryHeaderLookups
//
//    This class should be used for overriding collections in AutoCusEntryHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusEntryHeaderLookups : AutoCusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(AutoCusEntryHeader parent)
			: base(parent)
		{
		}

		public WarehouseTransactionStatusList WarehouseTransactionStatusList
		{
			get { return Factory.GetCachedValue<WarehouseTransactionStatusList>(); }
		}

		public virtual CodeDescriptionPairList MessageStatusList
		{
			get
			{
				var dec = Declaration;
				return dec == null ? new CodeDescriptionPairList() : dec.Lookups.MessageStatusList;
			}
		}

		public virtual CodeDescriptionPairList CH_MessageTypeList
		{
			get { return Factory.GetCachedValue<JobMessageTypeList>(); }
		}

		public virtual CodeDescriptionPairList CH_EntryStatusList
		{
			get
			{
				var dec = Declaration;
				return dec == null ? new CodeDescriptionPairList() : dec.Lookups.EntryStatusList;
			}
		}

		protected BaseJobDeclaration Declaration
		{
			get { return Parent.Declaration; }
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}
	}
}
