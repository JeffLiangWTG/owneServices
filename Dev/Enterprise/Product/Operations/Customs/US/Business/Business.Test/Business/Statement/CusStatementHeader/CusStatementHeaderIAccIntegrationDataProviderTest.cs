using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusStatementHeaderIAccIntegrationDataProviderTest : TestCaseWithFactory
	{
		[TestDate(2021, 05, 11)]
		public void TestEndToEndTest_AccountingIntegrationWhenStatementFinilized()
		{
			#region Setup base data for auto-rating

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			DeclarationTestHelper.SetEntryFilerCode("SV9");
			DeclarationTestHelper.SetProcessingDistrictPortCode("1101");

			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			ZGuid disbursementChargePK = Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_IsCreditor = true;
			Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, creditor.PK.ToGuid());

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "900044";
			account.BankAccount = bankAccount.PK;
			DataRegistry.Business.USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			new AccountingPeriodTestHelper().SetupPeriods();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupNotification);

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_FullName = "Test Supplier";
			supplier.CompanyData.OB_IsDebtor = true;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567890";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateAdValorem = 0.025m;

			#endregion

			#region Create declaration, invoice, invoice line and merge

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.JE_MasterBill = "1105202101";
			declaration.ImportEntryNumber = "73011726";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1105202101";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = supplier.MainAddress.PK;
			Factory.Save();
			CombineAssertions("Duty and Fees when line price is $10,000", () =>
			{
				AssertEquals("Duty amount should be 250", 250m, invoiceLine.US_Duty);
				AssertEquals("MPF amount should be 34.64", 34.64m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals("HMF amount should be 12.5", 12.5m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF).CY_FeeAmount);
			});

			#endregion

			#region  AutoRate DSB and Manual Post Revenue charges

			var accIntegrationDataProvider = (IAccIntegrationDataProvider)new Customs.Business.JobDeclarationIAccIntegrationDataProvider(ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.ARPostDSB, new[] { declaration.FormalEntry.PK }, declaration.PK, true, Factory);
			var chargePosterCreator = new CustomsDisbursementChargePosterCreator();
			var poster = chargePosterCreator.GetNewChargePoster(accIntegrationDataProvider.Action, accIntegrationDataProvider.DisbursementChargeCodes);
			var postingResult = poster.RaiseInvoices(accIntegrationDataProvider);
			Factory.Save();
			CombineAssertions("AutoRate DSB and Manual Post Revenue charges", () =>
			{
				var loadChargesQuery = new ZQuery(JobChargeSchema.JR_JH, job.PK);
				loadChargesQuery.OrderBy = JobChargeSchema.Constants.JR_DisplaySequence;
				var charges = Factory.Load<JobCharge>(loadChargesQuery);
				AssertEquals("Should be only 1 charge", 1, charges.Length);
				var disbursementCharge = charges[0];
				AssertEquals("Invoice number should be 73011726", "73011726", disbursementCharge.JR_APInvoiceNum);
				AssertEquals("Charge code should be CUSDSB", "CUSDSB", disbursementCharge.ChargeCode.AC_Code);
				AssertEquals("Cost Amount should be 297.14", 297.14m, disbursementCharge.JR_OSCostAmt);
				AssertEquals("Sell Amount should be 297.14", 297.14m, disbursementCharge.JR_OSSellAmt);
				AssertEquals("AR should be posted", true, disbursementCharge.IsRevenuePosted);
				AssertEquals("AP should NOT be posted", false, disbursementCharge.IsCostPosted);

				var transactionHeaders = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK));
				AssertEquals("Should be only 1 transaction header", 1, transactionHeaders.Length);
				AssertEquals("AH_Ledger should be AR", "AR", transactionHeaders[0].AH_Ledger);
				AssertEquals("AH_TransactionType should be INV", "INV", transactionHeaders[0].AH_TransactionType);
				AssertEquals("AH_InvoiceAmount should be 297.14", 297.14m, transactionHeaders[0].AH_InvoiceAmount);
			});

			#endregion

			#region Adjust line price on invoice line and merge

			invoiceLine.JI_LinePrice = 9000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CombineAssertions("Duty and Fees when line price is $9,000", () =>
			{
				AssertEquals("Duty amount should be 225", 225m, invoiceLine.US_Duty);
				AssertEquals("MPF amount should be 31.18", 31.18m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).CY_FeeAmount);
				AssertEquals("HMF amount should be 11.25", 11.25m, invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.HMF).CY_FeeAmount);
			});

			#endregion

			#region Insert Preliminary statement message and process

			var preliminaryMsgText =
"B001101SV9PFP11201880010510212                                                  " +
"Q11101SV9  73011726  58-1234567890510210000002250000000000000 B00209747      01 " +
"Q21101SV9  73011726 0000000000000000000000           2Y      17100000000        " +
"QA014990000000311850100000001125                                                " +
"Q31120188001  051021SV9              0000002250000000000000000000000001101      " +
"Q4000000000000000000000000000026743000000000000000100000                        " +
"QE014990000000311850100000001125                                                " +
"Y  1101SV9PF00006";

			var preliminaryMessage = Factory.New<MQEDIMessage>();
			preliminaryMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			preliminaryMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			preliminaryMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			preliminaryMessage.EM_Status = EDIMessage.Status.Queued;
			preliminaryMessage.EM_MessageText = preliminaryMsgText;
			preliminaryMessage.EM_GB = declaration.JE_GB;
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			var statements = Factory.Load<CusStatementHeader>(new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, "1120188001"));
			AssertEquals("1 statement created from message", 1, statements.Length);
			var statement = statements[0];
			statement.Messages.Reload(true);
			CombineAssertions("Preliminary statement message received", () =>
			{
				AssertEquals("Preliminary message attached to statement", 1, statement.Messages.Count);
				AssertEquals("B2_Status should be PRE", StatementHeaderStatusList.Codes.Preliminary, statement.B2_Status);
				AssertEquals("B2_PaymentType should be 2", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, statement.B2_PaymentType);
				AssertEquals("B2_PaymentParty should be BRK", PaymentPartyList.Codes.Broker, statement.B2_PaymentParty);
				AssertEquals("1 entry attached to statement", 1, statement.StatementLines.Count);

				var loadChargesQuery = new ZQuery(JobChargeSchema.JR_JH, job.PK);
				loadChargesQuery.OrderBy = JobChargeSchema.Constants.JR_DisplaySequence;
				var charges = Factory.Load<JobCharge>(loadChargesQuery);
				AssertEquals("A second DSB charge should be created, because AR invoice posted on first DSB charge", 2, charges.Length);
				var disbursementCharge = charges[0];
				AssertEquals("Invoice number for first DSB charge should be 73011726", "73011726", disbursementCharge.JR_APInvoiceNum);
				AssertEquals("Charge code for first DSB charge should be CUSDSB", "CUSDSB", disbursementCharge.ChargeCode.AC_Code);
				AssertEquals("Cost Amount for first DSB charge should be 297.14", 297.14m, disbursementCharge.JR_OSCostAmt);
				AssertEquals("Sell Amount for first DSB charge should be 297.14", 297.14m, disbursementCharge.JR_OSSellAmt);
				AssertEquals("AR for first DSB charge should be posted", true, disbursementCharge.IsRevenuePosted);
				AssertEquals("AP for first DSB charge should NOT be posted", false, disbursementCharge.IsCostPosted);
				disbursementCharge = charges[1];
				AssertEquals("Invoice number for second DSB charge should be 73011726/CUSDSB/1", "73011726/CUSDSB/1", disbursementCharge.JR_APInvoiceNum);
				AssertEquals("Charge code for second DSB charge should be CUSDSB", "CUSDSB", disbursementCharge.ChargeCode.AC_Code);
				AssertEquals("Cost Amount for second DSB charge should be -29.71", -29.71m, disbursementCharge.JR_OSCostAmt);
				AssertEquals("Sell Amount for second DSB charge should be -29.71", -29.71m, disbursementCharge.JR_OSSellAmt);
				AssertEquals("AR for second DSB charge should NOT be posted", false, disbursementCharge.IsRevenuePosted);
				AssertEquals("AP for second DSB charge should NOT be posted", false, disbursementCharge.IsCostPosted);

				var transactionHeaders = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK));
				AssertEquals("Should be only 1 transaction header", 1, transactionHeaders.Length);
				AssertEquals("AH_Ledger should be AR", "AR", transactionHeaders[0].AH_Ledger);
				AssertEquals("AH_TransactionType should be INV", "INV", transactionHeaders[0].AH_TransactionType);
				AssertEquals("AH_InvoiceAmount should be 297.14", 297.14m, transactionHeaders[0].AH_InvoiceAmount);

				var paymentTransactions = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, "1120188001"));
				AssertEquals("NO payment transaction should be created", 0, paymentTransactions.Length);
			});

			#endregion

			#region Send ACH Payment Authorization message

			var manager = new AutomatedClearinghouseMessageManager();
			var paymentAction = new StatementPaymentAction(statement);
			paymentAction.ACHPaymentType = ACHPaymentTypeList.Codes.ACHDebit;
			paymentAction.PayerUnitNo = "900044";
			manager.SendAuthorisation(paymentAction);
			Factory.Save();

			var paymentOutgoingMessage = statement.Messages.OfType<MQEDIMessage>().FirstOrDefault(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation);
			CombineAssertions("Send ACH Payment Authorization message", () =>
			{
				AssertNotNull("Payment Authorization message created", paymentOutgoingMessage);
				AssertEquals("B2_AccountNo should be 900044", "900044", statement.B2_AccountNo);
				AssertEquals("B2_PaymentParty should be BRK", PaymentPartyList.Codes.Broker, statement.B2_PaymentParty);
			});

			#endregion

			#region Insert ACH Payment Authorization Response message and process

			var paymentResponseMsgText =
$"B  1101SV9PZ                                               {paymentOutgoingMessage.EM_MessageNum.PadRight(16)}     " +
"E0 STMTNO 000001 REF ID: 1120188001  111111 03 0000026743                       " +
"E1A D01   PAYMENT ACCEPTED                        002  1120188001   051021      " +
"Y  1101SV9PZ";

			var paymentResponseMessage = Factory.New<MQEDIMessage>();
			paymentResponseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			paymentResponseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse;
			paymentResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			paymentResponseMessage.EM_Status = EDIMessage.Status.Queued;
			paymentResponseMessage.EM_MessageText = paymentResponseMsgText;
			paymentResponseMessage.EM_GB = declaration.JE_GB;
			paymentResponseMessage.EM_MessageNum = paymentOutgoingMessage.EM_MessageNum;
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			statement.Reload();
			statement.Messages.Reload(true);
			CombineAssertions("Process ACH Payment Authorization Response message", () =>
			{
				AssertEquals("ACH Payment Authorization Response message attached to statement", 3, statement.Messages.Count);
				AssertEquals("B2_PaymentStatus should be PAA", PaymentStatusList.Codes.PaymentAuthorizationAccepted, statement.B2_PaymentStatus);
				AssertEquals("B2_PaymentParty should be BRK", PaymentPartyList.Codes.Broker, statement.B2_PaymentParty);
				AssertEquals("B2_PaymentAuthorizationDate should be 10/May/21", new ZDateTime(2021, 05, 10), statement.B2_PaymentAuthorizationDate);

				var loadChargesQuery = new ZQuery(JobChargeSchema.JR_JH, job.PK);
				loadChargesQuery.OrderBy = JobChargeSchema.Constants.JR_DisplaySequence;
				var charges = Factory.Load<JobCharge>(loadChargesQuery);
				AssertEquals("Should still be 2 charges only, no additional charges created", 2, charges.Length);
				var disbursementCharge = charges[0];
				AssertEquals("Invoice number for first DSB charge should be 73011726", "73011726", disbursementCharge.JR_APInvoiceNum);
				AssertEquals("Charge code for first DSB charge should be CUSDSB", "CUSDSB", disbursementCharge.ChargeCode.AC_Code);
				AssertEquals("Cost Amount for first DSB charge should be 297.14", 297.14m, disbursementCharge.JR_OSCostAmt);
				AssertEquals("Sell Amount for first DSB charge should be 297.14", 297.14m, disbursementCharge.JR_OSSellAmt);
				AssertEquals("AR for first DSB charge should be posted", true, disbursementCharge.IsRevenuePosted);
				AssertEquals("AP for first DSB charge should NOT be posted", false, disbursementCharge.IsCostPosted);
				disbursementCharge = charges[1];
				AssertEquals("Invoice number for second DSB charge should be 73011726/CUSDSB/1", "73011726/CUSDSB/1", disbursementCharge.JR_APInvoiceNum);
				AssertEquals("Charge code for second DSB charge should be CUSDSB", "CUSDSB", disbursementCharge.ChargeCode.AC_Code);
				AssertEquals("Cost Amount for second DSB charge should be -29.71", -29.71m, disbursementCharge.JR_OSCostAmt);
				AssertEquals("Sell Amount for second DSB charge should be -29.71", -29.71m, disbursementCharge.JR_OSSellAmt);
				AssertEquals("AR for second DSB charge should NOT be posted", false, disbursementCharge.IsRevenuePosted);
				AssertEquals("AP for second DSB charge should NOT be posted", false, disbursementCharge.IsCostPosted);

				var transactionHeaders = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK));
				AssertEquals("Should be only 1 transaction header", 1, transactionHeaders.Length);
				AssertEquals("AH_Ledger should be AR", "AR", transactionHeaders[0].AH_Ledger);
				AssertEquals("AH_TransactionType should be INV", "INV", transactionHeaders[0].AH_TransactionType);
				AssertEquals("AH_InvoiceAmount should be 297.14", 297.14m, transactionHeaders[0].AH_InvoiceAmount);

				var paymentTransactions = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, "1120188001"));
				AssertEquals("NO payment transaction should be created", 0, paymentTransactions.Length);
			});

			#endregion

			#region Insert Final statement message and process

			var finalStatementMsgText =
"B001101SV9PFF11201880010510212                                                  " +
"Q11101SV9  73011726  58-1234567890510210000002250000000000000 B00209747      01 " +
"Q21101SV9  73011726 0000000000000000000000           2Y      17100000000        " +
"QA014990000000311850100000001125                                                " +
"Q31120188001  051021SV9              0000002250000000000000000000000001101      " +
"Q4000000000000000000000000000026743000000000000000100000                        " +
"QE014990000000311850100000001125                                                " +
"Y  1101SV9PF00006";

			var finalizedMessage = Factory.New<MQEDIMessage>();
			finalizedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			finalizedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.DailyStatement;
			finalizedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			finalizedMessage.EM_Status = EDIMessage.Status.Queued;
			finalizedMessage.EM_MessageText = finalStatementMsgText;
			finalizedMessage.EM_GB = declaration.JE_GB;
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			statement.Reload();
			statement.Messages.Reload(true);

			CombineAssertions("Process Final Statement Response message", () =>
			{
				AssertEquals("Final statement message attached to statement", 4, statement.Messages.Count);
				AssertEquals("B2_PaymentParty should be BRK", PaymentPartyList.Codes.Broker, statement.B2_PaymentParty);
				AssertEquals("B2_Status should be FIN", StatementHeaderStatusList.Codes.Final, statement.B2_Status);

				var loadChargesQuery = new ZQuery(JobChargeSchema.JR_JH, job.PK);
				loadChargesQuery.OrderBy = JobChargeSchema.Constants.JR_DisplaySequence;
				var charges = Factory.Load<JobCharge>(loadChargesQuery);
				AssertEquals("Should still be 2 charges only, no additional charges created", 2, charges.Length);
				var disbursementCharge = charges[0];
				AssertEquals("Invoice number for first DSB charge should be 73011726", "73011726", disbursementCharge.JR_APInvoiceNum);
				AssertEquals("Charge code for first DSB charge should be CUSDSB", "CUSDSB", disbursementCharge.ChargeCode.AC_Code);
				AssertEquals("Cost Amount for first DSB charge should be 297.14", 297.14m, disbursementCharge.JR_OSCostAmt);
				AssertEquals("Sell Amount for first DSB charge should be 297.14", 297.14m, disbursementCharge.JR_OSSellAmt);
				AssertEquals("AR for first DSB charge should be posted", true, disbursementCharge.IsRevenuePosted);
				AssertEquals("AP for first DSB charge should be be posted", true, disbursementCharge.IsCostPosted);
				disbursementCharge = charges[1];
				AssertEquals("Invoice number for second DSB charge should be 73011726/CUSDSB/1", "73011726/CUSDSB/1", disbursementCharge.JR_APInvoiceNum);
				AssertEquals("Charge code for second DSB charge should be CUSDSB", "CUSDSB", disbursementCharge.ChargeCode.AC_Code);
				AssertEquals("Cost Amount for second DSB charge should be -29.71", -29.71m, disbursementCharge.JR_OSCostAmt);
				AssertEquals("Sell Amount for second DSB charge should be -29.71", -29.71m, disbursementCharge.JR_OSSellAmt);
				AssertEquals("AR for second DSB charge should NOT be posted", false, disbursementCharge.IsRevenuePosted);
				AssertEquals("AP for second DSB charge should be posted", true, disbursementCharge.IsCostPosted);

				var transactionHeaders = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, job.PK));
				AssertEquals("Should be 3 transaction headers now", 3, transactionHeaders.Length);
				var arTransactions = transactionHeaders.Where(x => x.AH_Ledger == "AR");
				AssertEquals("Should be only 1 AR transaction", 1, arTransactions.Count());
				var arTransaction = arTransactions.FirstOrDefault();
				AssertEquals("AH_Ledger for AR transaction should be AR", "AR", arTransaction.AH_Ledger);
				AssertEquals("AH_TransactionType for AR transaction should be INV", "INV", arTransaction.AH_TransactionType);
				AssertEquals("AH_InvoiceAmount for AR transaction should be 297.14", 297.14m, arTransaction.AH_InvoiceAmount);
				var apTransactions = transactionHeaders.Where(x => x.AH_Ledger == "AP");
				AssertEquals("Should be only 2 AP transaction, one for AP Invoice and another for AP Credit", 2, apTransactions.Count());
				var apInvoiceTransaction = apTransactions.FirstOrDefault(x => x.AH_TransactionType == "INV");
				AssertEquals("AH_InvoiceAmount for AP Invoice should be -297.14", -297.14m, apInvoiceTransaction.AH_InvoiceAmount);
				var apCreditTransaction = apTransactions.FirstOrDefault(x => x.AH_TransactionType == "CRD");
				AssertEquals("AH_InvoiceAmount for AP Credit should be 29.71", 29.71m, apCreditTransaction.AH_InvoiceAmount);

				var paymentTransactions = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, "1120188001"));
				AssertEquals("One payment transaction should be created", 1, paymentTransactions.Length);
				var paymentTransaction = paymentTransactions[0];
				AssertEquals("AH_Ledger for payment should be AP", "AP", paymentTransaction.AH_Ledger);
				AssertEquals("AH_TransactionType for payment should be PAY", "PAY", paymentTransaction.AH_TransactionType);
				AssertEquals("AH_InvoiceAmount for payment should be 267.43", 267.43m, paymentTransaction.AH_InvoiceAmount);
			});

			#endregion
		}

		public void TestSendEmail()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "3914318259";
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_StatementAmount = 0m;

			Factory.Save();
			var dataProvider = (ICustomsPaymentDataProvider)new MessageProcessingStatementAccInvoiceIntegrationDataProvider(statement);
			Assert(!dataProvider.SendEmail);
			statement.B2_StatementAmount = 100m;

			dataProvider = new MessageProcessingStatementAccInvoiceIntegrationDataProvider(statement);
			Assert(dataProvider.SendEmail);
		}

		public void TestIncludeOnlyStatementLinesWithSomethingToPay()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 120m;
			statement.B2_StatementNumber = "4290234";

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "2";
			line.B3_EntryFilerCode = "XJ5";
			line.B3_Status = StatementLineStatusList.Codes.Active;
			line.B3_CustomsFeesTotal = 0m;

			var line2 = statement.StatementLines.AddNew();
			line2.B3_EntryNum = "2";
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_Status = StatementLineStatusList.Codes.Active;
			line2.B3_CustomsFeesTotal = 120m;
			Factory.Save();

			var dataProvider = new MessageProcessingStatementAccInvoiceIntegrationDataProvider(statement);
			AssertEquals(1, dataProvider.InvDataProviders.Length);
			AssertEquals(line2.PK, ((BusinessObject)dataProvider.InvDataProviders[0]).PK);

			var dataProvider2 = new UserInvokedStatementAccInvoiceIntegrationDataProvider(statement);
			AssertEquals(1, dataProvider2.InvDataProviders.Length);
			AssertEquals(line2.PK, ((BusinessObject)dataProvider2.InvDataProviders[0]).PK);
		}

		public void TestPostingActionNoneIfNothingToPay()
		{
			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementAmount = 0m;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			statement.B2_StatementNumber = "4290234";

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "2";
			line.B3_EntryFilerCode = "XJ5";
			line.B3_Status = StatementLineStatusList.Codes.Active;
			line.B3_CustomsFeesTotal = 0m;

			Factory.Save();
			var dataProvider = new MessageProcessingStatementAccInvoiceIntegrationDataProvider(statement);
			AssertEquals(ChargePosterBehaviours.None, dataProvider.Action);
			Assert(!dataProvider.ShouldMakePayment);

			var dataProvider2 = new UserInvokedStatementAccInvoiceIntegrationDataProvider(statement);
			AssertEquals(ChargePosterBehaviours.None, dataProvider2.Action);
			Assert(!dataProvider2.ShouldMakePayment);
		}

		public void TestEmailGroupPK()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~US";
			company.GC_RN_NKCountryCode = "US";

			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "US1";

			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "US2";

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "TST";
			var staff1 = group1.Staff.AddNew();
			staff1.GS_Code = "~AB";
			staff1.GS_EmailAddress = "abc@test.com";
			staff1.GS_LoginName = "AB";

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "TS2";
			var staff2 = group1.Staff.AddNew();
			staff2.GS_Code = "~AC";
			staff2.GS_EmailAddress = "abd@test.com";
			staff2.GS_LoginName = "AC";

			Factory.Save();

			using (Enterprise.Environment.DisposableEnvironment.ForBranch(branch1.PK.ToGuid()))
			{
				USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, "D1");
				USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, "D2");

				var groupNotification1 = new AutoBillingGroupNotification();
				groupNotification1.SendGroupPK = group1.PK;
				CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, groupNotification1);
				var groupNotification2 = new AutoBillingGroupNotification();
				groupNotification2.SendGroupPK = group2.PK;
				CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, groupNotification2);

				var dailyStatement = Factory.New<CusStatementHeader>();
				dailyStatement.B2_StatementNumber = "12345";
				dailyStatement.B2_GC = company.PK;
				dailyStatement.B2_BranchDesignation = "D1";
				AssertEquals(group1.PK, new UserInvokedStatementAccInvoiceIntegrationDataProvider(dailyStatement).EmailGroupPK);

				var statementLine = dailyStatement.StatementLines.AddNew();
				statementLine.B3_EntryFilerCode = "CJ5";
				statementLine.B3_EntryNum = "12345678";

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.US_EnableENS = true;
				declaration.US_EntryFilerCode = "CJ5";
				declaration.ImportEntryNumber = "12345678";
				declaration.JE_GB = branch2.PK;

				declaration.Invoices.AddNew();
				declaration.InvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				Factory.Save();

				AssertEquals(statementLine.Declaration.PK, declaration.PK);

				dailyStatement.B2_BranchDesignation = "";
				AssertEquals(branch2, dailyStatement.GetRelatedBranch());
				AssertEquals(group2.PK, new UserInvokedStatementAccInvoiceIntegrationDataProvider(dailyStatement).EmailGroupPK);
			}
		}

		public void TestOptionGeneratePerDailyStatementForMonthlyStatement()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.MonthlyStatementPaymentPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MonthlyStatementPaymentPostingOptionList.Codes._1);

			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_IsMonthlyStatement = true;
			monthlyStatement.B2_Status = StatementHeaderStatusList.Codes.Final;
			monthlyStatement.B2_StatementAmount = 3300.00m;

			//daily statement 1
			var dailyStatement1 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement1.B2_Status = StatementHeaderStatusList.Codes.Final;
			dailyStatement1.B2_StatementAmount = 300.00m;
			dailyStatement1.B2_StatementNumber = "000000001";

			//line 1
			var statementLine1ForDailyStatement1 = dailyStatement1.StatementLines.AddNew();
			statementLine1ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine1ForDailyStatement1.B3_CustomsFeesTotal = 150m;

			var statementLine1Charge1 = statementLine1ForDailyStatement1.Charges.AddNew();
			statementLine1Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			statementLine1Charge1.B4_ChargeAmount = 1.00m;

			var statementLine1Charge2 = statementLine1ForDailyStatement1.Charges.AddNew();
			statementLine1Charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			statementLine1Charge2.B4_ChargeAmount = 2.00m;

			//line 2
			var statementLine2ForDailyStatement1 = dailyStatement1.StatementLines.AddNew();
			statementLine2ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine2ForDailyStatement1.B3_CustomsFeesTotal = 150m;

			var statementLine2Charge1 = statementLine2ForDailyStatement1.Charges.AddNew();
			statementLine1Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Cotton;
			statementLine1Charge1.B4_ChargeAmount = 12.00m;

			var statementLine2Charge2 = statementLine2ForDailyStatement1.Charges.AddNew();
			statementLine1Charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.DairyFee;
			statementLine1Charge2.B4_ChargeAmount = 21.00m;

			//daily statement 2
			var dailyStatement2 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement2.B2_Status = StatementHeaderStatusList.Codes.Final;
			dailyStatement2.B2_StatementAmount = 3000.00m;

			//line 1
			var statementLine1ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine1ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine1ForDailyStatement2.B3_CustomsFeesTotal = 1000m;

			var statementLine1ForDailyStatement2_Charge1 = statementLine1ForDailyStatement2.Charges.AddNew();
			statementLine1ForDailyStatement2_Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			statementLine1ForDailyStatement2_Charge1.B4_ChargeAmount = 25.00m;

			//line 2
			var statementLine2ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine2ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine2ForDailyStatement2.B3_CustomsFeesTotal = 1000m;

			var statementLine2ForDailyStatement2_Charge1 = statementLine2ForDailyStatement2.Charges.AddNew();
			statementLine1ForDailyStatement2_Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			statementLine1ForDailyStatement2_Charge1.B4_ChargeAmount = 16.00m;

			//line 3
			var statementLine3ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine3ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine3ForDailyStatement2.B3_CustomsFeesTotal = 1000m;

			var statementLine3ForDailyStatement2_Charge1 = statementLine3ForDailyStatement2.Charges.AddNew();
			statementLine1ForDailyStatement2_Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			statementLine1ForDailyStatement2_Charge1.B4_ChargeAmount = 12.00m;

			Factory.Save();

			var dataProvider = new MessageProcessingStatementAccInvoiceIntegrationDataProvider(monthlyStatement);
			AssertEquals(5, dataProvider.InvDataProviders.Length);
			AssertEquals(2, dataProvider.APPaymentGroups.Count());

			var dailyStatementAsDataProvider = dataProvider.APPaymentGroups.FirstOrDefault(x => x.APPaymentNumber == "000000001");
			AssertNotNull(dailyStatementAsDataProvider);
			AssertEquals(2, dailyStatementAsDataProvider.InvoiceDataProviders.Count());
			AssertEquals(statementLine1ForDailyStatement1.PK, ((BusinessObject)dailyStatementAsDataProvider.InvoiceDataProviders.First()).PK);

			//second daily statement should not be included now because statement amount is 0.
			dailyStatement2.B2_StatementAmount = 0m;
			Factory.Save();

			dataProvider = new MessageProcessingStatementAccInvoiceIntegrationDataProvider(monthlyStatement);
			AssertEquals(5, dataProvider.InvDataProviders.Length);
			AssertEquals(1, dataProvider.APPaymentGroups.Count());

			dailyStatementAsDataProvider = dataProvider.APPaymentGroups.FirstOrDefault(x => x.APPaymentNumber == "000000001");
			AssertNotNull(dailyStatementAsDataProvider);
			AssertEquals(2, dailyStatementAsDataProvider.InvoiceDataProviders.Count());
			AssertEquals(statementLine1ForDailyStatement1.PK, ((BusinessObject)dailyStatementAsDataProvider.InvoiceDataProviders.First()).PK);

			statementLine1ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Deleted;
			statementLine2ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Deleted;
			Factory.Save();
			AssertEquals(0, dataProvider.APPaymentGroups.Count());
		}

		public void TestGenerateOnePaymentPerMonthlyStatement()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.MonthlyStatementPaymentPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, MonthlyStatementPaymentPostingOptionList.Codes._2);

			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1112P2222";
			monthlyStatement.B2_IsMonthlyStatement = true;
			monthlyStatement.B2_Status = StatementHeaderStatusList.Codes.Final;
			monthlyStatement.B2_StatementAmount = 3300.00m;

			//daily statement 1
			var dailyStatement1 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement1.B2_Status = StatementHeaderStatusList.Codes.Final;
			dailyStatement1.B2_StatementAmount = 300.00m;
			dailyStatement1.B2_StatementNumber = "000000001";

			//line 1
			var statementLine1ForDailyStatement1 = dailyStatement1.StatementLines.AddNew();
			statementLine1ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine1ForDailyStatement1.B3_CustomsFeesTotal = 150m;

			var statementLine1Charge1 = statementLine1ForDailyStatement1.Charges.AddNew();
			statementLine1Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Duty;
			statementLine1Charge1.B4_ChargeAmount = 1.00m;

			var statementLine1Charge2 = statementLine1ForDailyStatement1.Charges.AddNew();
			statementLine1Charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.AntidumpingDuty;
			statementLine1Charge2.B4_ChargeAmount = 2.00m;

			//line 2
			var statementLine2ForDailyStatement1 = dailyStatement1.StatementLines.AddNew();
			statementLine2ForDailyStatement1.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine2ForDailyStatement1.B3_CustomsFeesTotal = 150m;

			var statementLine2Charge1 = statementLine2ForDailyStatement1.Charges.AddNew();
			statementLine1Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Cotton;
			statementLine1Charge1.B4_ChargeAmount = 12.00m;

			var statementLine2Charge2 = statementLine2ForDailyStatement1.Charges.AddNew();
			statementLine1Charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.DairyFee;
			statementLine1Charge2.B4_ChargeAmount = 21.00m;

			//daily statement 2
			var dailyStatement2 = monthlyStatement.DailyStatements.AddNew();
			dailyStatement2.B2_Status = StatementHeaderStatusList.Codes.Final;
			dailyStatement2.B2_StatementAmount = 3000.00m;

			//line 1
			var statementLine1ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine1ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine1ForDailyStatement2.B3_CustomsFeesTotal = 1000m;

			var statementLine1ForDailyStatement2_Charge1 = statementLine1ForDailyStatement2.Charges.AddNew();
			statementLine1ForDailyStatement2_Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			statementLine1ForDailyStatement2_Charge1.B4_ChargeAmount = 25.00m;

			//line 2
			var statementLine2ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine2ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine2ForDailyStatement2.B3_CustomsFeesTotal = 1000m;

			var statementLine2ForDailyStatement2_Charge1 = statementLine2ForDailyStatement2.Charges.AddNew();
			statementLine1ForDailyStatement2_Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			statementLine1ForDailyStatement2_Charge1.B4_ChargeAmount = 16.00m;

			//line 3
			var statementLine3ForDailyStatement2 = dailyStatement2.StatementLines.AddNew();
			statementLine3ForDailyStatement2.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine3ForDailyStatement2.B3_CustomsFeesTotal = 1000m;

			var statementLine3ForDailyStatement2_Charge1 = statementLine3ForDailyStatement2.Charges.AddNew();
			statementLine1ForDailyStatement2_Charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			statementLine1ForDailyStatement2_Charge1.B4_ChargeAmount = 12.00m;

			Factory.Save();

			var dataProvider = new MessageProcessingStatementAccInvoiceIntegrationDataProvider(monthlyStatement);
			AssertEquals(5, dataProvider.InvDataProviders.Length);
			AssertEquals(1, dataProvider.APPaymentGroups.Count());

			var paymentGroup = dataProvider.APPaymentGroups.First();
			AssertEquals(5, paymentGroup.InvoiceDataProviders.Count());
			AssertEquals("1112P2222", paymentGroup.APPaymentNumber);
		}

		public void TestPostNegativeAmountWhenFinilized()
		{
			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupNotification);

			var testHelper = new Customs.Business.Testing.InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_FullName = "Test Supplier";
			supplier.CompanyData.OB_IsDebtor = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "12345678";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = supplier.MainAddress.PK;
			Factory.Save();

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.B2_StatementNumber = "4290234";
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			statement.B2_StatementAmount = 100m;
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "12345678";
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_CustomsFeesTotal = 100m;

			var charge = statementLine.Charges.AddNew();
			charge.B4_ChargeType = "DTY";
			charge.B4_ChargeAmount = 100m;

			Factory.Save();
			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("AP Total Amount", 100m, statement.APTotalAmount);
			AssertEquals("AP Posted Amount", 0m, statement.APPostedAmount);
			AssertEquals("AP Unposted Amount", 100m, statement.APUnPostedAmount);

			statement.PostAPInvoices = true;
			new StatementAccIntegration(null).Integrate(new UserInvokedStatementAccInvoiceIntegrationDataProvider(statement));
			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("AP Total Amount", 100m, statement.APTotalAmount);
			AssertEquals("AP Posted Amount", 100m, statement.APPostedAmount);
			AssertEquals("AP Unposted Amount", 0m, statement.APUnPostedAmount);

			charge.B4_ChargeAmount = 90m;
			statementLine.B3_CustomsFeesTotal = 90m;
			statement.B2_StatementAmount = 90m;
			new StatementAccIntegration(null).Integrate(new UserInvokedStatementAccInvoiceIntegrationDataProvider(statement));
			Factory.Save();
			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("AP Total Amount", 90m, statement.APTotalAmount);
			AssertEquals("AP Posted Amount", 100m, statement.APPostedAmount);
			AssertEquals("AP Unposted Amount", -10m, statement.APUnPostedAmount);

			var statementRow = ((IBusinessObjectInternals)statement).Row;
			statementRow[CusStatementHeader.Schema.B2_Status] = StatementHeaderStatusList.Codes.Final;
			new StatementAccIntegration(null).Integrate(new UserInvokedStatementAccInvoiceIntegrationDataProvider(statement));
			Factory.Save();
			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("AP Total Amount", 90m, statement.APTotalAmount);
			AssertEquals("AP Posted Amount", 90m, statement.APPostedAmount);
			AssertEquals("AP Unposted Amount", 0m, statement.APUnPostedAmount);

			statementRow = ((IBusinessObjectInternals)statement).Row;
			statementRow[CusStatementHeader.Schema.B2_Status] = StatementHeaderStatusList.Codes.Preliminary;
			charge.B4_ChargeAmount = 80m;
			statementLine.B3_CustomsFeesTotal = 80m;
			statement.B2_StatementAmount = 80m;
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			Factory.Save();
			statement.RefreshAP_ARInvoiceQueryResult();
			AssertEquals("AP Total Amount", 80m, statement.APTotalAmount);
			AssertEquals("AP Posted Amount", 80m, statement.APPostedAmount);
			AssertEquals("AP Unposted Amount", 0m, statement.APUnPostedAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
