using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TransactionCreationRestrictionList : CodeDescriptionPairList
	{
		public TransactionCreationRestrictionList()
			: base()
		{
			Add(None);
			Add(Invoice);
			Add(All);
			Add(OutstandingBalance);
		}

		public static CodeDescriptionPair None { get { return new CodeDescriptionPair(Constants.TransactionCreationRestriction.None, ResString.GetMultilingualString("MasterFiles|TransactionCreationRestriction|None", "No Restriction")); } }
		public static CodeDescriptionPair Invoice { get { return new CodeDescriptionPair(Constants.TransactionCreationRestriction.Invoice, ResString.GetMultilingualString("MasterFiles|TransactionCreationRestriction|Invoice", "Restricted from creating New Invoice, Credit Note and Adjustment Note")); } }
		public static CodeDescriptionPair All { get { return new CodeDescriptionPair(Constants.TransactionCreationRestriction.All, ResString.GetMultilingualString("MasterFiles|TransactionCreationRestriction|All", "Restricted from creating New Transactions (All Transaction Types)")); } }
		public static CodeDescriptionPair OutstandingBalance { get { return new CodeDescriptionPair(Constants.TransactionCreationRestriction.OutstandingBalance, ResString.GetMultilingualString("MasterFiles|TransactionCreationRestriction|OutstandingBalance", "Restricted from creating Transactions that would cause the outstanding balance in local currency to increase in value")); } }
	}
}