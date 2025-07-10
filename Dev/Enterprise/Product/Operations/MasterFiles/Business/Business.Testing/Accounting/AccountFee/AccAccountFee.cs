using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccAccountFee))]
	sealed class AccAccountFeeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = base.GetNewBusinessObject();
			(bizo as AccAccountFee).AAF_FeeAmount = 200m;
			return bizo;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizo = base.GetNewBusinessObjectForDeleteTest(factory);
			(bizo as AccAccountFee).AAF_FeeAmount = 200m;
			(bizo as AccAccountFee).AAF_Rule = "TCB";
			return bizo;
		}

		public void TestAccountFeeCalculationRuleList()
		{
			var lookup = new AccAccountFeeLookups(Factory.New<AccAccountFee>());
			AssertEquals("Count", 4, lookup.AccountFeeCalculationRuleList.Count);
			AssertEquals("NON", true, lookup.AccountFeeCalculationRuleList.ContainsCode(AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee));
			AssertEquals("TCR", true, lookup.AccountFeeCalculationRuleList.ContainsCode(AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted));
			AssertEquals("OSB", true, lookup.AccountFeeCalculationRuleList.ContainsCode(AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists));
			AssertEquals("TCB", true, lookup.AccountFeeCalculationRuleList.ContainsCode(AccAccountFee.AccountFeeCalculationRuleType.WhenEitherTransactionPostedOrOutstandingBalanceExists));

			AssertEquals("NON Description", "Do Not Charge Account Fee", lookup.AccountFeeCalculationRuleList.GetDescriptionFromCode(AccAccountFee.AccountFeeCalculationRuleType.DoNotChargeAccountFee));
			AssertEquals("TCR Description", "Bill When Transactions Have Been Posted", lookup.AccountFeeCalculationRuleList.GetDescriptionFromCode(AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted));
			AssertEquals("OSB Description", "Bill when an Outstanding Balance Exists", lookup.AccountFeeCalculationRuleList.GetDescriptionFromCode(AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists));
			AssertEquals("TCB Description", "Bill When Transactions Posted OR When Outstanding Balance Exists", lookup.AccountFeeCalculationRuleList.GetDescriptionFromCode(AccAccountFee.AccountFeeCalculationRuleType.WhenEitherTransactionPostedOrOutstandingBalanceExists));
		}

		public void TestNoStmALogs()
		{
			var fee = Factory.NewWithValidTestData<AccAccountFee>();
			fee.AAF_FeeAmount = 1m;
			fee.AAF_Rule = AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted;
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, fee.PK);

				AssertEquals("No StmALog should be generated after creating an AccAccountFee.", 0, Factory.Load<StmALog>(query).Length);

				fee.AAF_FeeAmount = 100m;
				Factory.Save();

				AssertEquals("No StmALog should be generated after editing an AccAccountFee.", 0, Factory.Load<StmALog>(query).Length);

				fee.Delete();
				Factory.Save();

				AssertEquals("No StmALog should be generated after deleting an AccAccountFee.", 0, Factory.Load<StmALog>(query).Length);
			});
		}
	}
}

