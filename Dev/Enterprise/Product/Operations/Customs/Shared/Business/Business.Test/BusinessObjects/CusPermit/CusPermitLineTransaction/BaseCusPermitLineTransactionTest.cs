using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusPermitLineTransaction))]
	public class BaseCusPermitLineTransactionTest : SharedCusPermitLineTransactionTest<BaseCusPermitLineTransaction>
	{
		public void TestValidationType()
		{
			AssertType<BaseCusPermitLineTransactionValidation>(LineTransaction.Validation);
		}

		public void TestLookupsType()
		{
			AssertType<BaseCusPermitLineTransactionLookups>(LineTransaction.Lookups);
		}

		protected override string ShortName => "Permit Transaction";

		protected override BaseCusPermitLineTransaction GetNewLineTransaction(BusinessObjectFactory factory)
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var lineTransaction = permitHeader.CusPermitLineTransactions.AddNew();
			lineTransaction.FillWithValidTestData();
			return lineTransaction;
		}

		#region Test for Loader

		[TestedType(typeof(BaseCusPermitLineTransaction.Loader))]
		class LoaderTest : LoaderTestCase
		{
			public void TestFindPendingLines()
			{
				var header1 = Factory.New<BaseCusPermitHeader>();
				var header1Line1Ref1 = header1.CusPermitLineTransactions.AddNew();
				header1Line1Ref1.CPL_Reference = "REF1";
				header1Line1Ref1.CPL_TransactionStatus = ZString.Empty;
				header1Line1Ref1.CPL_AppId = "APPID1";
				var header1Line2Ref1 = header1.CusPermitLineTransactions.AddNew();
				header1Line2Ref1.CPL_Reference = "REF1";
				header1Line2Ref1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				header1Line2Ref1.CPL_AppId = "APPID1";
				var header1Line3Ref2 = header1.CusPermitLineTransactions.AddNew();
				header1Line3Ref2.CPL_Reference = "REF2";
				header1Line3Ref2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				header1Line3Ref2.CPL_AppId = "APPID1";
				var header1Line4Ref1 = header1.CusPermitLineTransactions.AddNew();
				header1Line4Ref1.CPL_Reference = "REF1";
				header1Line4Ref1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				header1Line4Ref1.CPL_AppId = "APPID1";
				var header1Line5Ref2 = header1.CusPermitLineTransactions.AddNew();
				header1Line5Ref2.CPL_Reference = "REF2";
				header1Line5Ref2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				header1Line5Ref2.CPL_AppId = "APPID2";

				var header2 = Factory.New<BaseCusPermitHeader>();
				var header2Line1Ref2 = header2.CusPermitLineTransactions.AddNew();
				header2Line1Ref2.CPL_Reference = "REF2";
				header2Line1Ref2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				header2Line1Ref2.CPL_AppId = "APPID1";
				var header2Line2Ref1 = header2.CusPermitLineTransactions.AddNew();
				header2Line2Ref1.CPL_Reference = "REF1";
				header2Line2Ref1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				header2Line2Ref1.CPL_AppId = "APPID1";
				var header2Line3Ref2 = header2.CusPermitLineTransactions.AddNew();
				header2Line3Ref2.CPL_Reference = "REF2";
				header2Line3Ref2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
				header2Line3Ref2.CPL_AppId = "APPID2";

				var loader = new BaseCusPermitLineTransaction.Loader(Factory);

				AssertEquals(0, loader.FindPendingLines("APPID3", "REF2").Length);
				AssertEquals(0, loader.FindPendingLines("APPID1", "E124").Length);
				AssertEquals(0, loader.FindPendingLines("APPID1", "").Length);
				var lines = loader.FindPendingLines("APPID1", "REF2");
				AssertContainsExactElementsInAnyOrder(new[] { header1Line3Ref2, header2Line1Ref2 }, lines);
				lines = loader.FindPendingLines("APPID1", "REF1");
				AssertContainsExactElementsInAnyOrder(new[] { header1Line2Ref1, header1Line4Ref1, header2Line2Ref1 }, lines);
			}

			protected override BusinessObject.Loader GetNewLoaderToTest() => new BaseCusPermitLineTransaction.Loader(Factory);
		}

		#endregion
	}
}
