using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusPermitHeader))]
	public class BaseCusPermitHeaderTest : SharedCusPermitHeaderTest<BaseCusPermitHeader>
	{
		public void TestAddTransaction()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			AssertEquals("No Transactions", 0, permitHeader.CusPermitLineTransactions.Count);

			permitHeader.AddTransaction("REF1", "CMT1", "APPID", "PROCEDURE", 100m, 200m);

			AssertEquals("Transaction Added", 1, permitHeader.CusPermitLineTransactions.Count);
			AssertEquals("CPL_Reference", "REF1", permitHeader.CusPermitLineTransactions.First().CPL_Reference);
			AssertEquals("CPL_Comment", "CMT1", permitHeader.CusPermitLineTransactions.First().CPL_Comment);
			AssertEquals("CPL_TranValue", 100m, permitHeader.CusPermitLineTransactions.First().CPL_TranValue);
			AssertEquals("CPL_TranQty", 200m, permitHeader.CusPermitLineTransactions.First().CPL_TranQty);
			AssertEquals("CPL_TransactionStatus", "", permitHeader.CusPermitLineTransactions.First().CPL_TransactionStatus);
			AssertEquals("CPL_ReferenceNumberLine", 0, permitHeader.CusPermitLineTransactions.First().CPL_ReferenceNumberLine);
			AssertEquals("CPL_Procedure", "PROCEDURE", permitHeader.CusPermitLineTransactions.First().CPL_Procedure);

			permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", 100m, 200m, PermitTransactionStatusList.Codes.Pending, 1);
			var trans2 = permitHeader.CusPermitLineTransactions.First(x => x.CPL_Reference == "REF2");
			AssertEquals("CPL_TransactionStatus", "PND", trans2.CPL_TransactionStatus);
			AssertEquals("CPL_ReferenceNumberLine", 1, trans2.CPL_ReferenceNumberLine);
		}

		public void TestAddTransaction_PreventPermitBusting()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			CombineAssertions(() =>
			{
				AssertEquals("No Transactions", 0, permitHeader.GetTransactions().Count());
				permitHeader.AddTransaction("REF1", "CMT1", "APPID", "PROCEDURE", 100m, 200m, PermitTransactionStatusList.Codes.Pending, 1);
				AssertEquals("Transaction Added", 1, permitHeader.GetTransactions().Count());
				var message = string.Empty;
				var availableValue = ZDecimal.Zero;
				AssertNull("Value Busting", permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -101m, -200m, PermitTransactionStatusList.Codes.Pending, 1, notifier: (m, a) =>
				{
					message = m;
					availableValue = a;
				}));
				AssertEquals("Value Busting Message", "The permit KW65QXUA86QBTJDVP3315VMJ2K7FD1AKPK2 available value will be exceeded by 1. The available is 100.", message);
				AssertEquals("The available permit value", 100m, availableValue);
				AssertEquals("Still 1 transaction", 1, permitHeader.GetTransactions().Count());
				var availableQuantity = ZDecimal.Zero;
				AssertNull("Quantity Busting", permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -100m, -201m, PermitTransactionStatusList.Codes.Pending, 1, notifier: (m, a) =>
				{
					message = m;
					availableQuantity = a;
				}));
				AssertEquals("Quantity Busting Message", "The permit KW65QXUA86QBTJDVP3315VMJ2K7FD1AKPK2 available quantity will be exceeded by 1. The available is 200.", message);
				AssertEquals("The available permit quantity", 200m, availableQuantity);
				AssertEquals("Only 1 transaction", 1, permitHeader.GetTransactions().Count());
			});
		}

		public void TestAddTransaction_PreventPermitBustingForValuePermits()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;

			CombineAssertions(() =>
			{
				AssertEquals("No Transactions", 0, permitHeader.GetTransactions().Count());
				permitHeader.AddTransaction("REF1", "CMT1", "APPID", "PROCEDURE", 100m, 0m, PermitTransactionStatusList.Codes.Pending, 1);
				AssertEquals("Transaction Added", 1, permitHeader.GetTransactions().Count());
				var message = string.Empty;
				var availableValue = ZDecimal.Zero;
				AssertNull("Value Busting", permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -101m, -0m, PermitTransactionStatusList.Codes.Pending, 1, notifier: (m, a) =>
				{
					message = m;
					availableValue = a;
				}));
				AssertEquals("Value Busting Message", "The permit KW65QXUA86QBTJDVP3315VMJ2K7FD1AKPK2 available value will be exceeded by 1. The available is 100.", message);
				AssertEquals("The available permit value", 100m, availableValue);
				AssertEquals("Still 1 transaction", 1, permitHeader.GetTransactions().Count());

				permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -100m, -201m, PermitTransactionStatusList.Codes.Pending, 1);
				AssertEquals("2 transactions. Does not bust quantity as permit is of type VAL", 2, permitHeader.GetTransactions().Count());
			});
		}

		public void TestAddTransaction_PreventPermitBustingForQuantityPermits()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			permitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;

			CombineAssertions(() =>
			{
				AssertEquals("No Transactions", 0, permitHeader.GetTransactions().Count());
				permitHeader.AddTransaction("REF1", "CMT1", "APPID", "PROCEDURE", 0m, 200m, PermitTransactionStatusList.Codes.Pending, 1);
				AssertEquals("Transaction Added", 1, permitHeader.GetTransactions().Count());
				var message = string.Empty;
				var availableQuantity = ZDecimal.Zero;
				AssertNull("Quantity Busting", permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -0m, -201m, PermitTransactionStatusList.Codes.Pending, 1, notifier: (m, a) =>
				{
					message = m;
					availableQuantity = a;
				}));
				AssertEquals("Quantity Busting Message", "The permit KW65QXUA86QBTJDVP3315VMJ2K7FD1AKPK2 available quantity will be exceeded by 1. The available is 200.", message);
				AssertEquals("The available permit quantity", 200m, availableQuantity);
				AssertEquals("Still 1 transaction", 1, permitHeader.GetTransactions().Count());

				permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -101m, -200m, PermitTransactionStatusList.Codes.Pending, 1);
				AssertEquals("2 transactions. Does not bust value as permit is of type QTY", 2, permitHeader.GetTransactions().Count());
			});
		}

		public void TestAddTransaction_IfPermitIsAlreadyBusted()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			new PermitTestDataHelper(Factory).CreatePermitLineTransaction(permitHeader, "REF1", "", "APPID", Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -100m, PermitTransactionStatusList.Codes.Confirmed);

			CombineAssertions(() =>
			{
				var transactions = permitHeader.GetTransactions().ToArray();
				AssertEquals("Initial - transactions", 1, transactions.Length);
				AssertEquals("Initial - 1st CPL_TranValue", -50m, transactions[0].CPL_TranValue);
				AssertEquals("Initial - 1st CPL_TranQty", -100m, transactions[0].CPL_TranQty);

				permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", 1m, 2m, PermitTransactionStatusList.Codes.Pending, 1);
				transactions = permitHeader.GetTransactions().ToArray();
				AssertEquals("We can reduce busted value - transactions", 2, transactions.Length);
				AssertEquals("We can reduce busted value - 1st CPL_TranValue", -50m, transactions[0].CPL_TranValue);
				AssertEquals("We can reduce busted value - 1st CPL_TranQty", -100m, transactions[0].CPL_TranQty);
				AssertEquals("We can reduce busted value - 2nd CPL_TranValue", 1m, transactions[1].CPL_TranValue);
				AssertEquals("We can reduce busted value - 2nd CPL_TranQty", 2m, transactions[1].CPL_TranQty);

				permitHeader.AddTransaction("REF2", "CMT1", "APPID", "PROCEDURE", -1m, -2m, PermitTransactionStatusList.Codes.Pending, 1);
				transactions = permitHeader.GetTransactions().ToArray();
				AssertEquals("We can't increase busted value - transactions", 2, transactions.Length);
			});
		}

		public void TestALogIsCreatedWhenHeaderInformationIsChanged()
		{
			var holder = Factory.New<OrgHeader>();
			holder.MainAddress.OA_Address1 = "Address1";
			holder.OH_Code = "HLDR1";
			var permit = Factory.New<BaseCusPermitHeader>();
			permit.CPH_OH_PermitHolder = holder.PK;
			permit.CPH_Number = "PRM1";
			permit.CPH_StartDate = ZDate.Today;
			permit.CPH_Type = "RCC";
			permit.CPH_SubType = "ACO";
			permit.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			Factory.Save();

			var holder2 = Factory.New<OrgHeader>();
			holder2.OH_Code = "HLDR2";
			permit.CPH_OH_PermitHolder = holder2.PK;
			permit.CPH_Number = "PRM2";
			permit.CPH_StartDate = ZDate.Today.AddDays(1);
			permit.CPH_Type = "IMP";
			permit.CPH_SubType = "";
			permit.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
			permit.CPH_UnitOfMeasure = "KG";
			permit.CPH_OA_AppliesTo = holder.MainAddress.PK;

			Factory.Save();
			var logs = permit.Logs.GetAllLogs().OfType<StmALog>().Select(log => log.SL_Reference);
			AssertCollectionContains("Permit Holder: From 'HLDR1' to 'HLDR2'", logs);
			AssertCollectionContains("Number: From 'PRM1' to 'PRM2'", logs);
			AssertCollectionContains("Start Date: From '" + ZDate.Today + "' to '" + ZDate.Today.AddDays(1) + "'", logs);
			AssertCollectionContains("Type: From 'RCC' to 'IMP'", logs);
			AssertCollectionContains("Sub Type: From 'ACO' to ''", logs);
			AssertCollectionContains("Qty Val Indicator: From 'BTH' to 'VAL'", logs);
			AssertCollectionContains("Unit Of Measure: From '' to 'KG'", logs);
			AssertCollectionContains("Applies To: From '' to 'Address1'", logs);
		}

		public void TestCPH_SubType_ReadOnly()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			Assert("CPH_SubType_ReadOnly must be true", PermitHeader.CPH_SubType_ReadOnly);
		}

		public void TestHasNonZeroOrderBalance()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			Assert("no orders", !permitHeader.HasNonZeroOrderBalance());

			var obl1 = permitHeader.CusPermitLineTransactions.AddNew();
			obl1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			obl1.CPL_TranQty = 1m;
			obl1.CPL_TranValue = 1m;
			Assert("no orders", !permitHeader.HasNonZeroOrderBalance());

			var obl2 = permitHeader.CusPermitLineTransactions.AddNew();
			obl2.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			obl2.CPL_TranQty = 6m;
			obl2.CPL_TranValue = 6m;
			Assert("no orders", !permitHeader.HasNonZeroOrderBalance());

			var order1 = permitHeader.CusPermitLineTransactions.AddNew();
			order1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			order1.CPL_TranQty = -5m;
			order1.CPL_TranValue = -5m;
			Assert("q=-5, v=-5", permitHeader.HasNonZeroOrderBalance());

			var order2 = permitHeader.CusPermitLineTransactions.AddNew();
			order2.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			order2.CPL_TranQty = 2m;
			order2.CPL_TranValue = 3m;
			Assert("q=-3, v=-2", permitHeader.HasNonZeroOrderBalance());

			var order3 = permitHeader.CusPermitLineTransactions.AddNew();
			order3.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			order3.CPL_TranQty = 0m;
			order3.CPL_TranValue = 2m;
			Assert("q=-3, v=0", permitHeader.HasNonZeroOrderBalance());

			var order4 = permitHeader.CusPermitLineTransactions.AddNew();
			order4.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			order4.CPL_TranQty = 3m;
			order4.CPL_TranValue = 2m;
			Assert("q=0, v=2", permitHeader.HasNonZeroOrderBalance());

			var order5 = permitHeader.CusPermitLineTransactions.AddNew();
			order5.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			order5.CPL_TranQty = 0m;
			order5.CPL_TranValue = -2m;
			Assert("q=0, v=0", !permitHeader.HasNonZeroOrderBalance());
		}

		public void TestHasPendingTransaction()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			var obl1 = permitHeader.CusPermitLineTransactions.AddNew();
			var obl2 = permitHeader.CusPermitLineTransactions.AddNew();

			obl1.CPL_TransactionStatus = ZString.Empty;
			obl2.CPL_TransactionStatus = ZString.Empty;
			Assert("No pending transaction", !permitHeader.HasPendingTransaction());

			obl2.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			Assert("No pending transaction", permitHeader.HasPendingTransaction());
		}

		public void TestClose()
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			var obl1 = permitHeader.CusPermitLineTransactions.AddNew();
			obl1.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			obl1.CPL_TranQty = 7m;
			obl1.CPL_TranValue = 7m;

			var order1 = permitHeader.CusPermitLineTransactions.AddNew();
			order1.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			order1.CPL_TranQty = -5m;
			order1.CPL_TranValue = -5m;

			AssertEquals(false, permitHeader.CPH_IsClosed);

			permitHeader.Close();

			AssertEquals(true, permitHeader.CPH_IsClosed);
			Assert("has closed log entry", permitHeader.Logs.HasLogWith(
				l => l.SL_SE_NKEvent == "PCL" && l.SL_Reference == "Permit closed"
			));
		}

		public void TestDefaultValues()
		{
			var permitHeader = Factory.New<BaseCusPermitHeader_ForTest>();
			AssertEquals("Defaulting Type if only one code", "TAR", permitHeader.CPH_Type);
			AssertEquals(Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Permit, permitHeader.CPH_ApplicationCode);
		}

		protected override ControllerID ExpectedControllerID => ControllerIDs.Customs.Permits;

		#region Implementation

		protected override BaseCusPermitHeader GetNewPermitHeader(BusinessObjectFactory factory)
			=> factory.NewWithValidTestData<BaseCusPermitHeader>();

		protected override ZString ShortName => "Permit";

		#endregion

		class BaseCusPermitHeader_ForTest : BaseCusPermitHeader
		{
			public BaseCusPermitHeader_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override PermitCountrySpecificInstruction CountrySpecificInstruction => new PermitCountrySpecificInstruction_ForTest(Factory);
		}

		class PermitCountrySpecificInstruction_ForTest : PermitCountrySpecificInstruction
		{
			public PermitCountrySpecificInstruction_ForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public override PermitTypeList GetTypeList()
			{
				var types = new PermitTypeList();
				var codes = types.GetAllCodes();
				foreach (var code in codes)
				{
					types.RemoveCode(code);
				}
				types.AddPair("TAR", "Tariff");
				return types;
			}
		}
	}
}
