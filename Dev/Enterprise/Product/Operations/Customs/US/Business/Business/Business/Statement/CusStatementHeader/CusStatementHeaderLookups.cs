//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStatementHeaderLookups
//
//    This class should be used for overriding collections in AutoCusStatementHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class CusStatementHeaderLookups : Customs.Business.CusStatementHeaderLookups
	{
		public CusStatementHeaderLookups(CusStatementHeader parent)
			: base(parent)
		{
		}

		public ConsigneeCollection ImportersList
		{
			get
			{
				if (importersList == null)
				{
					importersList = new ConsigneeCollection(Factory);
				}
				return importersList;
			}
		}
		protected ConsigneeCollection importersList;

		public StatementHeaderStatusList StatementHeaderStatusList
		{
			get { return Factory.GetCachedValue<StatementHeaderStatusList>(); }
		}

		public PaymentTypeList PaymentTypeList
		{
			get { return Factory.GetCachedValue<PaymentTypeList>(); }
		}

		public PaymentStatusList PaymentStatusList
		{
			get { return Factory.GetCachedValue<PaymentStatusList>(); }
		}

		public PaymentPartyList PaymentPartyList
		{
			get { return Factory.GetCachedValue<PaymentPartyList>(); }
		}

		public StatementLineFilterByOptionList LineFilterByOptions
		{
			get { return Factory.GetCachedValue<StatementLineFilterByOptionList>(); }
		}
	}
}
