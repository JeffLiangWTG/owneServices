using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitLineTransactionLookupsTest<TSharedCusPermitLineTransactionLookups, TSharedCusPermitLineTransaction> : BusinessObjectLookupsTestCase
			where TSharedCusPermitLineTransactionLookups : SharedCusPermitLineTransactionLookups
			where TSharedCusPermitLineTransaction : SharedCusPermitLineTransaction
	{
		public void TestPermitTransactionCategories()
		{
			AssertType<PermitTransactionCategoryList>(LineTransaction.Lookups.PermitTransactionCategories);
		}

		public void TestPermitTransactionTypes()
		{
			AssertType<PermitTransactionTypeList>(LineTransaction.Lookups.PermitTransactionTypes);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var permit = Factory.New<BaseCusPermitHeader>();
				permit.CPH_Type = "FTZ";
				var transaction = permit.CusPermitLineTransactions.AddNew();
				Assert("Contains FTZ", transaction.Lookups.PermitTransactionTypes.ContainsCode("FTZ"));
			}
		}

		public void TestPermitTransactionStatuses()
		{
			var list = LineTransaction.Lookups.PermitTransactionStatuses;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "PND, ", list.CodesAsString);
				AssertSame("Cached", list, LineTransaction.Lookups.PermitTransactionStatuses);
			});
		}

		#region Implementation

		protected abstract TSharedCusPermitLineTransaction GetNewLineTransaction(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			LineTransaction = GetNewLineTransaction(Factory);
		}

		protected TSharedCusPermitLineTransaction LineTransaction { get; set; }

		protected SharedCusPermitHeader PermitHeader => LineTransaction.PermitHeader;

		#endregion
	}
}
