using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(StatementPaymentAction))]
	sealed class StatementPaymentActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatementNumber()
		{
			StatementHeader.B2_StatementNumber = "5513136098";
			var statementPaymentAction = new StatementPaymentAction(StatementHeader);
			AssertEquals("5513136098", statementPaymentAction.StatementNumber);
		}

		public void TestStatementHeader()
		{
			StatementPaymentAction statementPaymentAction = new StatementPaymentAction(StatementHeader);
			AssertEquals(StatementHeader, statementPaymentAction.StatementHeader);
		}

		public void TestRunPreSaveValidationForTotalAmountPayable()
		{
			CreateTestDateForTotalAmountPayable();
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			var mockLine1 = Factory.NewMoq<CusStatementLine>();
			mockLine1.Object.B3_EntryNum = "1";
			mockLine1.Object.B3_EntryFilerCode = "XJ5";
			mockLine1.Object.B3_B2 = statement.PK;
			var query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.ARPostedAmount = 1000m;
			query.Result.ARUnPostedAmount = 500m;
			mockLine1.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);
			mockLine1.Object.B3_CustomsFeesTotal = 2000m;

			var mockLine2 = Factory.NewMoq<CusStatementLine>();
			mockLine2.Object.B3_EntryNum = "2";
			mockLine2.Object.B3_EntryFilerCode = "XJ6";
			mockLine2.Object.B3_B2 = statement.PK;
			var query2 = new TestQuery();
			query2.Result = new AP_ARInvoiceQueryResult();
			query2.Result.ARPostedAmount = 20m;
			query2.Result.ARUnPostedAmount = 50m;
			mockLine2.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query2);
			mockLine2.Object.B3_CustomsFeesTotal = 70m;
			var statementPaymentAction = new StatementPaymentAction(statement);
			statementPaymentAction.PayerUnitNo = "123456";

			AssertEquals("ARTotalAmount", 1500m, mockLine1.Object.ARTotalAmount);
			AssertEquals("B3_CustomsFeesTotal", 2000m, mockLine1.Object.B3_CustomsFeesTotal);
			statementPaymentAction.RunPreSaveValidation();
			AssertHasMessageErrorContaining(statementPaymentAction.TotalAmountPayableInfo, "The AR amounts of some entries do not match Customs Fee Total of this statement.");

			AssertEquals("ARTotalAmount", 70m, mockLine2.Object.ARTotalAmount);
			AssertEquals("B3_CustomsFeesTotal", 70m, mockLine2.Object.B3_CustomsFeesTotal);
			AssertHasWarningContaining(statementPaymentAction.TotalAmountPayableInfo, "There are unposted AR amounts for some entries.");
		}

		public void TestRunPreSaveValidationForLine()
		{
			CreateTestDateForTotalAmountPayable();
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.ADM;
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			var mockLine1 = Factory.NewMoq<CusStatementLine>();
			mockLine1.Object.B3_EntryNum = "1";
			mockLine1.Object.B3_EntryFilerCode = "XJ5";
			mockLine1.Object.B3_B2 = statement.PK;
			var query = new TestQuery();
			query.Result = new AP_ARInvoiceQueryResult();
			query.Result.ARPostedAmount = 500m;
			query.Result.ARUnPostedAmount = 410m;
			mockLine1.Protected().Setup<IAccountingAP_ARInvoiceQuery>("GetAccountingAP_ARInvoiceQuery").Returns(query);
			mockLine1.Object.B3_CustomsFeesTotal = 2000m;

			var linkJobDeclaration = mockLine1.Object.Declaration;
			AssertNotNull(linkJobDeclaration);
			var statementPaymentAction = new StatementPaymentAction(statement);
			statementPaymentAction.RunPreSaveValidation();

			AssertHasMessageErrorContaining(mockLine1.Object.ReleaseStatusInfo, "This entry has not been released yet.");
			AssertHasWarningContaining(mockLine1.Object.B3_CustomsFeesTotalInfo, "There is an unposted AR amount for this entry.");

			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			statementPaymentAction.RunPreSaveValidation();
			AssertNoMessageErrorContaining(mockLine1.Object.ReleaseStatusInfo, "This entry has not been released yet.");
		}

		public void TestDefaultAccountNoOnConstructor()
		{
			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var creditor = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var creditorWrapper = OrgHeaderWrapper.New(creditor);
			creditorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			creditorWrapper.ZO_AccountNo = "BF4534";

			Factory.Save();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "BF4534";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			account.BankAccount = bankAccount.PK;
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var action = new StatementPaymentAction(StatementHeader);
			AssertEquals("AccountNo is defaulted from registry as payment type is broker", "BF4534", action.PayerUnitNo);
			AssertEquals(ACHPaymentTypeList.Codes.ACHDebit, action.ACHPaymentType);

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			var addInfo = (OrgImpAddInfo)importer.CountryData.ImpAddInfo;
			addInfo.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;
			addInfo.ZO_AccountNo = "";
			Factory.Save();
			StatementHeader.B2_OH_Importer = importer.PK;
			StatementHeader.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			StatementHeader.B2_AccountNo = "";

			action = new StatementPaymentAction(StatementHeader);
			AssertEquals(ACHPaymentTypeList.Codes.ACHCredit, action.ACHPaymentType);
			AssertEquals("AccountNo is defaulted from importer", "", action.PayerUnitNo);
		}

		public void TestDefaultAccountNoOnConstructorWhenNoDefaultACHDebitExists()
		{
			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "BF4534";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			account.BankAccount = bankAccount.PK;
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var action = new StatementPaymentAction(StatementHeader);
			AssertEquals("AccountNo is defaulted from registry as payment type is broker", "BF4534", action.PayerUnitNo);
			AssertEquals(ACHPaymentTypeList.Codes.ACHDebit, action.ACHPaymentType);
		}

		public void TestValidateACHPaymentType()
		{
			var action = new StatementPaymentAction(StatementHeader);

			action.ACHPaymentType = "";
			AssertHasMessageErrors(action.ACHPaymentTypeInfo);

			action.ACHPaymentType = "~";
			AssertHasMessageErrors(action.ACHPaymentTypeInfo);

			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHDebit;
			AssertNoMessageErrors(action.ACHPaymentTypeInfo);

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var creditorWrapper = OrgHeaderWrapper.New(creditor);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var importerWrapper = OrgHeaderWrapper.New(importer);
			Factory.Save();
			Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			StatementHeader.B2_OH_Importer = importer.PK;
			StatementHeader.B2_PaymentParty = PaymentPartyList.Codes.Importer;

			action = new StatementPaymentAction(StatementHeader);
			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHCredit;
			AssertHasMessageError(action.ACHPaymentTypeInfo, StatementPaymentAction.ACHPaymentTypeShouldBeDebitWhenPartyIsNotConfigured);

			creditorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;
			creditorWrapper.ZO_AccountNo = "A12346";
			importerWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHDebit;
			importerWrapper.ZO_AccountNo = "B12346";
			Factory.Save();

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "A12346";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			account.BankAccount = bankAccount.PK;
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHCredit;
			AssertNoMessageError(action.ACHPaymentTypeInfo, StatementPaymentAction.ACHPaymentTypeShouldBeDebitWhenPartyIsNotConfigured);

			action.PayerUnitNo = "A12346";
			AssertEquals("PreCondition", PaymentPartyList.Codes.Broker, action.PaymentParty);

			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHDebit;
			AssertHasMessageError(action.ACHPaymentTypeInfo, string.Format(StatementPaymentAction.PaymentTypeDifferentToPaymentPartyOrgConfiguration, ACHPaymentTypeList.Codes.ACHCredit));

			action.PayerUnitNo = "B1234";
			AssertEquals("PreCondition", PaymentPartyList.Codes.Importer, action.PaymentParty);
			AssertNoMessageError(action.ACHPaymentTypeInfo, string.Format(StatementPaymentAction.PaymentTypeDifferentToPaymentPartyOrgConfiguration, ACHPaymentTypeList.Codes.ACHCredit));
		}

		public void TestValidatePaymentParty()
		{
			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "A12346";
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			account.BankAccount = bankAccount.PK;
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			var action = new StatementPaymentAction(StatementHeader);
			action.PayerUnitNo = "2222";

			AssertEquals(PaymentPartyList.Codes.Importer, action.PaymentParty);
			AssertHasWarning(action.PaymentPartyInfo, string.Format(StatementPaymentAction.PaymentPartyCalculatedAsImporter, PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, "2222"));

			action.PayerUnitNo = "A12346";
			AssertEquals(PaymentPartyList.Codes.Broker, action.PaymentParty);
			AssertNoWarnings(action.PaymentPartyInfo);
		}

		public void TestIPaymentAuthorisation()
		{
			StatementHeader.B2_StatementNumber = "1234";
			StatementHeader.B2_EntryFilerCode = "XJ5";
			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			StatementHeader.B2_BranchDesignation = "01";
			StatementHeader.B2_ProcessPort = "1101";

			var line1 = StatementHeader.StatementLines.AddNew();
			line1.B3_CustomsFeesTotal = 1500m;

			var line2 = StatementHeader.StatementLines.AddNew();
			line2.B3_CustomsFeesTotal = 2000m;

			var action = new StatementPaymentAction(StatementHeader);
			action.PayerUnitNo = "V123";
			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHDebit;

			IPaymentAuthorisation paymentAuthorization = action;

			AssertEquals("AccountNo", "V123", paymentAuthorization.PayersUnitNumber);
			AssertEquals("StatementNumber", "1234", paymentAuthorization.StatementBillNumber);
			AssertEquals("StatementFiler", "XJ5", paymentAuthorization.StatementFiler);

			AssertEquals("01", paymentAuthorization.ClientBranchDesignation);
			AssertEquals("1101", paymentAuthorization.ProcessingPortCode);
			Assert(paymentAuthorization.NegationCode.IsEmpty);
			AssertEquals(ZDate.Empty, paymentAuthorization.NegationDate);

			AssertExceptionThrown(typeof(InvalidOperationException), () => paymentAuthorization.PayersUnitNumber = "");
			AssertExceptionThrown(typeof(InvalidOperationException), () => paymentAuthorization.StatementBillNumber = "");
			AssertExceptionThrown(typeof(InvalidOperationException), () => paymentAuthorization.StatementFiler = "");
			AssertExceptionThrown(typeof(InvalidOperationException), () => paymentAuthorization.PaymentType = "01");

			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHCredit;
			AssertEquals("PayerUnitNumber should be blank if ACH credit", "", paymentAuthorization.PayersUnitNumber);

			action = StatementPaymentAction.NewForNegation(StatementHeader);
			paymentAuthorization = action;

			AssertEquals(YesNoDefaultList.Codes.Yes, paymentAuthorization.NegationCode);
			AssertEquals(ZDate.Today, paymentAuthorization.NegationDate);
		}

		public void TestProcessingPortForRLF()
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			StatementHeader.Messages.Add(message);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.DailyStatement;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText =
@"B011704AZ2QRP17090983990408092            01  2904AZ2  1                        " +
"Q11704AZ2 414629930000000000000000000000 00000000000000000000004146299   01   01" +
"Q21704AZ2 41462993204080936-4087754000000000000000000000000Y  00000000000P      " +
"QA1704AZ2 41462993000000000000000000009331000000000000555400000000000000000000  " +
"Q31704AZ217090983990408090000000000000000000000000000000000000000000000000000000" +
"Q41704AZ200000000000000000000000000000000000000000000000000148850000100000      " +
"QE1704AZ2000000000000000000000000000000009331000000000000005554000000000000     " +
"QF1704AZ2000000000000000000000000000000000000000000000000000000000000000        " +
"QG1704AZ2000000000000000000000000000000000000                                   " +
"Y  1704AZ2QR00008";

			StatementHeader.B2_StatementNumber = "1234";
			StatementHeader.B2_EntryFilerCode = "XJ5";
			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			StatementHeader.B2_BranchDesignation = "01";
			StatementHeader.B2_ProcessPort = "1704";

			CusStatementLine line1 = StatementHeader.StatementLines.AddNew();
			line1.B3_CustomsFeesTotal = 1500m;

			StatementPaymentAction action = new StatementPaymentAction(StatementHeader);
			action.PayerUnitNo = "V123";

			IPaymentAuthorisation paymentAuthorization = action;
			AssertEquals("2904", paymentAuthorization.ProcessingPortCode);

			StatementHeader.B2_PreparerDistrictPort = "2905";
			AssertEquals("2905", paymentAuthorization.ProcessingPortCode);
		}

		public void TestACHPaymentTypeList()
		{
			StatementPaymentAction action = new StatementPaymentAction(StatementHeader);
			Assert(!action.ACHTypeList.ContainsCode(ACHPaymentTypeList.Codes.ImporterCheck));
		}

		public void TestValidateAccountNo()
		{
			StatementHeader.B2_StatementNumber = "1234";
			StatementHeader.B2_EntryFilerCode = "XJ5";
			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;

			CusStatementLine line1 = StatementHeader.StatementLines.AddNew();
			line1.B3_CustomsFeesTotal = 1500m;

			CusStatementLine line2 = StatementHeader.StatementLines.AddNew();
			line2.B3_CustomsFeesTotal = 2000m;

			StatementPaymentAction action = new StatementPaymentAction(StatementHeader);
			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHDebit;
			action.PayerUnitNo = "";
			AssertHasMessageError(action.PayerUnitNoInfo, ValidationConstants.Statement.PayerUnitNoIsMandatory);

			action.PayerUnitNo = "78453";
			AssertNoMessageError(action.PayerUnitNoInfo, ValidationConstants.Statement.PayerUnitNoIsMandatory);
			AssertHasMessageError(action.PayerUnitNoInfo, ValidationConstants.Statement.PayerUnitNoLength);

			action.PayerUnitNo = "123456";
			AssertNoMessageError(action.PayerUnitNoInfo, ValidationConstants.Statement.PayerUnitNoLength);

			action.ACHPaymentType = ACHPaymentTypeList.Codes.ACHCredit;
			action.PayerUnitNo = "";
			AssertNoMessageError(action.PayerUnitNoInfo, ValidationConstants.Statement.PayerUnitNoIsMandatory);

			action.PayerUnitNo = "123";
			AssertHasMessageError(action.PayerUnitNoInfo, ValidationConstants.Statement.PayerUnitNoIsNotRequired);

			action.PayerUnitNo = "";
			AssertNoMessageError(action.PayerUnitNoInfo, ValidationConstants.Statement.PayerUnitNoIsNotRequired);
		}

		public void TestValidatePayByACHCredit()
		{
			var orgHeader = Factory.New<OrgHeader>();
			OrgHeaderWrapper.New(orgHeader).ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;

			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			StatementHeader.B2_OH_Importer = orgHeader.PK;
			StatementHeader.B2_PaymentParty = PaymentPartyList.Codes.Importer;

			var action = new StatementPaymentAction(StatementHeader);
			action.RunPreSaveValidation();
			AssertHasMessageError(action.ACHPaymentTypeInfo, ValidationConstants.Statement.PayByACHCredit);

			StatementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			StatementHeader.B2_OH_Importer = Guid.Empty;
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgHeader.PK.ToGuid());

			action = new StatementPaymentAction(StatementHeader);
			action.RunPreSaveValidation();
			AssertHasMessageError(action.ACHPaymentTypeInfo, ValidationConstants.Statement.PayByACHCredit);

			StatementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			StatementHeader.B2_OH_Importer = orgHeader.PK;
			StatementHeader.B2_PaymentParty = PaymentPartyList.Codes.Importer;

			action = new StatementPaymentAction(StatementHeader);
			action.RunPreSaveValidation();
			AssertNoMessageError(action.ACHPaymentTypeInfo, ValidationConstants.Statement.PayByACHCredit);

			StatementHeader.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			StatementHeader.B2_OH_Importer = Guid.Empty;
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgHeader.PK.ToGuid());

			action = new StatementPaymentAction(StatementHeader);
			action.RunPreSaveValidation();
			AssertNoMessageError(action.ACHPaymentTypeInfo, ValidationConstants.Statement.PayByACHCredit);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = new StatementPaymentAction(StatementHeader);
			result.ClearAllNotifications();
			return result;
		}

		CusStatementHeader statementHeader;
		CusStatementHeader StatementHeader => statementHeader ?? (statementHeader = Factory.New<CusStatementHeader>());

		JobDeclaration declaration;
		void CreateTestDateForTotalAmountPayable()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_DeclarationReference = "T00001";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "1";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ6";
			declaration2.JE_DeclarationReference = "T00002";

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.EntryNumber = "2";
			Factory.Save();
		}
	}
}
