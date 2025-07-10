//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccAccountFeeLookups
//
//    This class should be used for overriding collections in AutoAccAccountFeeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAccountFeeLookups : AutoAccAccountFeeLookups
	{
		public AccAccountFeeLookups(AutoAccAccountFee parent)
			: base(parent)
		{
		}

		public override AccGLHeaderCollection GLAccounts
		{
			get
			{
				AccGLHeaderCollection gLRevenueAccountCollection = new AccGLHeaderCollection(Factory, GetAccountCollectionFilter());
				return gLRevenueAccountCollection;
			}
		}

		ZQuery GetAccountCollectionFilter()
		{
			var accountTypes = new string[] { Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.AccountType.BalanceSheetAccount };
			var query = new ZQuery(AccGLHeaderSchema.AG_AccountType, accountTypes);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, SQLComparisonOperator.NotEqual, true);
			return query;
		}

		public CodeDescriptionPairList AccountFeeCalculationRuleList
		{
			get
			{
				if (accountFeeRule_list == null)
				{
					accountFeeRule_list = new CodeDescriptionPairList();
					accountFeeRule_list.AddPair(AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee, ResString.GetMultilingualString("561f3c71-0899-4a4d-81a9-5557abb6fdf1", "Do Not Charge Account Fee"));
					accountFeeRule_list.AddPair(AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, ResString.GetMultilingualString("23c89048-56b8-4e86-b686-fa24460d171c", "Bill When Transactions Have Been Posted"));
					accountFeeRule_list.AddPair(AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists, ResString.GetMultilingualString("2f550ce4-1b91-483e-8998-39de4b4af3c6", "Bill when an Outstanding Balance Exists"));
					accountFeeRule_list.AddPair(AccAccountFee.AccountFeeCalculationRuleType.WhenEitherTransactionPostedOrOutstandingBalanceExists, ResString.GetMultilingualString("d3e0ed0a-ca97-4ffd-af64-41fd397324f8", "Bill When Transactions Posted OR When Outstanding Balance Exists"));
				}
				return accountFeeRule_list;
			}
		}
		CodeDescriptionPairList accountFeeRule_list;
	}
}
