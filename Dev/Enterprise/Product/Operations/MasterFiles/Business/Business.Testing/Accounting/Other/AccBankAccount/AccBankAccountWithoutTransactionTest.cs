using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccBankAccountWithoutTransactionTest : TestCaseWithFactory
	{
		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				AccBankAccountSchema.PK,
				AccBankAccountSchema.AB_AccountNum
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, AccBankAccountSchema.PK.TableSchema.SqlSchemaName, AccBankAccountSchema.PK.TableName, columns);
		}

		public void TestHasChequeNumberBeenUsedOnAPaymentApprovalInDatabase()
		{
			var dataCreationFactory = new BusinessObjectFactory();

			var bankAccount = dataCreationFactory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountNum = GetRandomString(10);
			bankAccount.AB_BSB = GetRandomString(6);
			bankAccount.AB_Code = GetRandomString(3);
			bankAccount.AB_IsDefaultReceiptBankAccount = new ZBool(Core.Constants.BooleanTrueString);
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;

			var paymentApproval1 = dataCreationFactory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval1.AV_ChequeOrReference = "1";
			paymentApproval1.AV_PaymentType = "CHQ";
			paymentApproval1.AV_Status = "APP";
			paymentApproval1.AV_AB = bankAccount.PK;

			var paymentApproval2 = dataCreationFactory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval2.AV_ChequeOrReference = "2";
			paymentApproval2.AV_PaymentType = "CHQ";
			paymentApproval2.AV_Status = "APP";
			paymentApproval2.AV_AB = bankAccount.PK;

			dataCreationFactory.Save();

			var newfactory = new BusinessObjectFactory();
			var bankAccountInCurrentFactory = newfactory.Load<AccBankAccount>(bankAccount.PK);
			AssertEquals(true, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			var sql = string.Format("UPDATE dbo.AccPaymentApproval SET AV_ChequeOrReference = '3', AV_SystemLastEditTimeUtc = GETUTCDATE(), AV_SystemLastEditUser = 'TST' WHERE AV_PK = '{0}'", paymentApproval1.PK);
			Db.Connection.ExecuteNonQuery(sql);
			AssertEquals(false, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			sql = string.Format("UPDATE dbo.AccPaymentApproval SET AV_ChequeOrReference = '1', AV_SystemLastEditTimeUtc = GETUTCDATE(), AV_SystemLastEditUser = 'TST' WHERE AV_PK = '{0}'", paymentApproval2.PK);
			Db.Connection.ExecuteNonQuery(sql);
			AssertEquals(true, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			var paymentApproval2InCurrentFactory = newfactory.Load<AccPaymentApproval>(paymentApproval2.PK);
			paymentApproval2InCurrentFactory.AV_ChequeOrReference = "3";
			AssertEquals(false, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			var paymentApproval1InCurrentFactory = newfactory.Load<AccPaymentApproval>(paymentApproval1.PK);
			paymentApproval1InCurrentFactory.AV_ChequeOrReference = "1";
			AssertEquals(true, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));
		}

		public void TestHasChequeNumberBeenUsedOnAPaymentApprovalNotInDatabase()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountNum = GetRandomString(10);
			bankAccount.AB_BSB = GetRandomString(6);
			bankAccount.AB_Code = GetRandomString(3);
			bankAccount.AB_IsDefaultReceiptBankAccount = new ZBool(Core.Constants.BooleanTrueString);
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var paymentApproval1 = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval1.AV_ChequeOrReference = "1";
			paymentApproval1.AV_PaymentType = "CHQ";
			paymentApproval1.AV_Status = "APP";
			paymentApproval1.AV_AB = bankAccount.PK;

			var paymentApproval2 = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval2.AV_ChequeOrReference = "2";
			paymentApproval2.AV_PaymentType = "CHQ";
			paymentApproval2.AV_Status = "APP";
			paymentApproval2.AV_AB = bankAccount.PK;

			AssertEquals(true, bankAccount.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			paymentApproval1.AV_ChequeOrReference = "3";

			AssertEquals(false, bankAccount.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));
		}

		public void TestHasChequeNumberBeenUsedOnAPaymentApprovalWhenDeleted()
		{
			var dataCreationFactory = new BusinessObjectFactory();

			var bankAccount = dataCreationFactory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_AccountNum = GetRandomString(10);
			bankAccount.AB_BSB = GetRandomString(6);
			bankAccount.AB_Code = GetRandomString(3);
			bankAccount.AB_IsDefaultReceiptBankAccount = new ZBool(Core.Constants.BooleanTrueString);
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_GC = GlbCompany.CurrentCompany.PK;

			var paymentApproval1 = dataCreationFactory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval1.AV_ChequeOrReference = "1";
			paymentApproval1.AV_PaymentType = "CHQ";
			paymentApproval1.AV_Status = "APP";
			paymentApproval1.AV_AB = bankAccount.PK;

			dataCreationFactory.Save();

			var newfactory = new BusinessObjectFactory();
			var bankAccountInCurrentFactory = newfactory.Load<AccBankAccount>(bankAccount.PK);
			AssertEquals(true, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			var sql = string.Format("DELETE dbo.AccPaymentApproval WHERE AV_PK = '{0}'", paymentApproval1.PK);
			Db.Connection.ExecuteNonQuery(sql);
			AssertEquals(false, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			var paymentApproval2 = newfactory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval2.AV_ChequeOrReference = "1";
			paymentApproval2.AV_PaymentType = "CHQ";
			paymentApproval2.AV_Status = "APP";
			paymentApproval2.AV_AB = bankAccount.PK;
			AssertEquals(true, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));

			paymentApproval1.AV_Amount = 100m;
			AssertEquals(true, bankAccountInCurrentFactory.HasChequeNumberBeenUsedOnAPaymentApproval("1", null));
		}

		public static string GetRandomString(int stringLength)
		{
			string result = "";
			for (int i = 0; i < stringLength; i++)
			{
				int index = Generator.Next(65, 90);
				result += (char)index;
			}
			return result;
		}

		public static Random Generator
		{
			get { return generator ?? (generator = new Random()); }
		}
		[ThreadStatic]
		static Random generator;
	}
}
