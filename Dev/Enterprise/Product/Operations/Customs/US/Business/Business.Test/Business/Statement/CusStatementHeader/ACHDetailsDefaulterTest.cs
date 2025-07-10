using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACHDetailsDefaulterTest : TestCaseWithFactory
	{
		public void TestGetPayerUnitNoToDefault()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_AccountNo = "506010";
			AssertEquals("506010", statement.GetPayerUnitNoToDefault());

			statement.B2_AccountNo = ZString.Empty;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statement.B2_BranchDesignation = "14";

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "800102";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "14";
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);
			AssertEquals("800102", statement.GetPayerUnitNoToDefault());

			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(importer).ZO_AccountNo = "222256";
			Factory.Save();
			statement.B2_OH_Importer = importer.PK;
			AssertEquals("222256", statement.GetPayerUnitNoToDefault());

			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statement.B2_PaymentParty = ZString.Empty;
			statement.B2_OH_Importer = ZGuid.Empty;
			AssertEquals(ZString.Empty, statement.GetPayerUnitNoToDefault());

			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			AssertEquals("800102", statement.GetPayerUnitNoToDefault());
		}

		public void TestGetACHPayMethodToDefault()
		{
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(creditor).ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;

			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(importer).ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			Factory.Save();

			Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals(ACHPaymentTypeList.Codes.ACHCredit, statement.GetACHPayMethodToDefault());

			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			AssertEquals(ACHPaymentTypeList.Codes.ACHDebit, statement.GetACHPayMethodToDefault());

			statement.B2_OH_Importer = importer.PK;
			AssertEquals(ACHPaymentTypeList.Codes.ACHDebit, statement.GetACHPayMethodToDefault());
		}

		public void TestGetACHPayMethodWithPaymentParty()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var creditorWrapper = OrgHeaderWrapper.New(creditor);
			creditorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;
			creditorWrapper.ZO_AccountNo = "A1234";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerWrapper = OrgHeaderWrapper.New(importer);
			importerWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			importerWrapper.ZO_AccountNo = "B1234";
			Factory.Save();

			Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_OH_Importer = importer.PK;

			AssertEquals(ACHPaymentTypeList.Codes.ACHDebit, statement.GetACHPayMethodForPaymentParty(PaymentPartyList.Codes.Importer));
			AssertEquals(ACHPaymentTypeList.Codes.ACHCredit, statement.GetACHPayMethodForPaymentParty(PaymentPartyList.Codes.Broker));
		}
	}
}
