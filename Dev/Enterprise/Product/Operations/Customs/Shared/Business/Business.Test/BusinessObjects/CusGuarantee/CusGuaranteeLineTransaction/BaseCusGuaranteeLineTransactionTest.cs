using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGuaranteeLineTransaction))]
	sealed class BaseCusGuaranteeLineTransactionTest : SharedCusPermitLineTransactionTest<BaseCusGuaranteeLineTransaction>
	{
		public void TestCPL_TranValue()
		{
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(BaseCusGuaranteeLineTransaction), nameof(BaseCusGuaranteeLineTransaction.CPL_TranValue), true, x => x.DecimalPlaces == 2);
		}

		public void TestValidationType()
		{
			AssertType<CusGuaranteeLineTransactionValidation>(LineTransaction.Validation);
		}

		public void TestLookupsType()
		{
			AssertType<CusGuaranteeLineTransactionLookups>(LineTransaction.Lookups);
		}

		public void TestOpeningBalanceIsAggregated()
		{
			AssertEquals("An opening balance guarantee transaction line should have its IsAggregated property set to true by default", true, LineTransaction.CPL_IsAggregated);
		}

		public void TestAddingAdjTransactionSetsIsAggregatedToFalse()
		{
			LineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertEquals("Adding an Adj transaction first should set IsAggregated property set to false", false, LineTransaction.CPL_IsAggregated);
		}

		public void TestDefaultTransactionStatus()
		{
			var adjustmentTransaction1 = GuaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			AssertEquals("A new manual adjustment transaction line should have its status set to 'confirmed (CON) by default", PermitTransactionStatusList.Codes.Confirmed, adjustmentTransaction1.CPL_TransactionStatus);
		}

		public void TestDefaultTransactionType()
		{
			var guaranteeHeader = GuaranteeHeader;
			AssertEquals(PermitTransactionTypeList.Codes.OBL, LineTransaction.CPL_TransactionType);
			var adjustmentTransaction1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			AssertEquals(PermitTransactionTypeList.Codes.ADJ, adjustmentTransaction1.CPL_TransactionType);
			var adjustmentTransaction2 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			AssertEquals(PermitTransactionTypeList.Codes.ADJ, adjustmentTransaction2.CPL_TransactionType);
		}

		public void TestReadOnlyAfterSave()
		{
			Assert(!LineTransaction.ReadOnly);
			Factory.Save();
			Assert(LineTransaction.ReadOnly);
		}

		public void TestIsAggregatedCannotBeChangedAfterSavingToDatabase()
		{
			var transaction = LineTransaction;
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transaction.CPL_Reference = "REFERENCE";
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			});
			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				transaction.CPL_IsAggregated = false;
			});
		}

		public void TestCannotDeleteTransactionsAfterSavingToDatabase()
		{
			var transaction = LineTransaction;
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transaction.CPL_Reference = "REFERENCE";
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				transaction.Delete();
			});
		}

		#region Implementation

		protected override string ShortName => "Guarantee Transaction";

		protected override BaseCusGuaranteeLineTransaction GetNewLineTransaction(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var lineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			lineTransaction.FillWithValidTestData();
			return lineTransaction;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		BaseCusGuaranteeHeader GuaranteeHeader => LineTransaction.GuaranteeHeader;

		#endregion
	}
}
