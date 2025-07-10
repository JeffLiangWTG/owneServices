using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(SharedCusPermitLineTransaction))]
	public abstract class SharedCusPermitLineTransactionTest<TSharedCusPermitLineTransaction> : EnterpriseBusinessObjectTestCase
		where TSharedCusPermitLineTransaction : SharedCusPermitLineTransaction
	{
		public void TestHumanReadableName()
		{
			LineTransaction.CPL_Reference = "12345  ";
			AssertEquals($"{ShortName} 12345", LineTransaction.HumanReadableName);
		}

		public void TestTransactionTypeDescription()
		{
			LineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.ADJ;
			AssertEquals(LineTransaction.Lookups.PermitTransactionTypes.GetDescriptionFromCode(PermitTransactionTypeList.Codes.ADJ), LineTransaction.TransactionTypeDescription);
		}

		public void TestPermitHeader()
		{
			AssertEquals(LineTransaction.CPL_CPH_PermitHeader, LineTransaction.PermitHeader.PK);
		}

		public void TestCPL_TransactionType_ReadOnly()
		{
			Assert(LineTransaction.CPL_TransactionType_ReadOnly);
		}

		public void TestCPL_TransactionDate_ReadOnly()
		{
			Assert(LineTransaction.CPL_TransactionDate_ReadOnly);
		}

		public void TestReadOnly()
		{
			Assert(!LineTransaction.ReadOnly);
			Factory.Save();
			Assert(LineTransaction.ReadOnly);
		}

		[TestDate(2016, 11, 14)]
		public void TestSetDefaultValues()
		{
			LineTransaction = GetNewLineTransaction(Factory);
			var lineTransaction = LineTransaction.PermitHeader.AddTransaction(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			AssertEquals(new ZDateTime(2016, 11, 14), lineTransaction.CPL_TransactionDate);
		}

		public void TestTransactionStatusDescription()
		{
			CombineAssertions(() =>
			{
				LineTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
				AssertEquals("Valid", PermitTransactionStatusList.Descriptions.Confirmed, LineTransaction.TransactionStatusDescription);

				LineTransaction.CPL_TransactionStatus = "";
				AssertEquals("Empty", "Empty", LineTransaction.TransactionStatusDescription);

				LineTransaction.CPL_TransactionStatus = "~";
				AssertEquals("Invalid", ZString.Empty, LineTransaction.TransactionStatusDescription);
			});
		}

		public void TestLogs()
		{
			var lineTransaction = (AutoCusPermitLineTransaction)LineTransaction.PermitHeader.AddTransaction("Ref1", ZString.Empty, ZString.Empty, ZString.Empty, 1m, ZDecimal.Zero);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNull("No ADD event", lineTransaction.Logs.MostRecentLogByEventTime(AutoEvents.AddedARecordToTheSystem));

				lineTransaction.CPL_Reference = "Ref2";
				Factory.Save();
				AssertNotNull("Has EDT event", lineTransaction.Logs.MostRecentLogByEventTime(AutoEvents.EditedARecord));
			});
		}

		protected abstract string ShortName { get; }

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => GetNewLineTransaction(Factory);

		protected abstract TSharedCusPermitLineTransaction GetNewLineTransaction(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			LineTransaction = GetNewLineTransaction(Factory);
		}

		protected TSharedCusPermitLineTransaction LineTransaction { get; set; }

		#endregion
	}
}
