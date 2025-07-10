using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class JobHeaderParentDeletionHelperTest : TestCaseWithFactory
	{
		public void TestGetJobNoDeletionErrorMessage()
		{
			AssertEquals(@"The ObjectName cannot be deleted.
Child Invoicing Job(s) have been saved against this Invoicing Job Header.", JobHeaderParentDeletionHelper.GetJobNoDeletionErrorMessage(JobHeaderSchema.Constants.TableName, "The ObjectName"));

			AssertEquals(@"The ObjectName cannot be deleted.
Hot Cheque(s) have been saved against this Invoicing Job Header.", JobHeaderParentDeletionHelper.GetJobNoDeletionErrorMessage(AccHotChequeSchema.Constants.TableName, "The ObjectName"));

			AssertEquals(@"The ObjectName cannot be deleted.
Accounting Transaction(s) have been saved against this Invoicing Job Header.", JobHeaderParentDeletionHelper.GetJobNoDeletionErrorMessage(AccTransactionHeaderSchema.Constants.TableName, "The ObjectName"));

			AssertEquals(@"The ObjectName cannot be deleted.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header.", JobHeaderParentDeletionHelper.GetJobNoDeletionErrorMessage(AccTransactionLinesSchema.Constants.TableName, "The ObjectName"));

			AssertEquals(@"The ObjectName cannot be deleted.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header.", JobHeaderParentDeletionHelper.GetJobNoDeletionErrorMessage(JobChargeSchema.Constants.TableName, "The ObjectName"));
		}

		public void TestCheckIfCanDeleteJobHeaderParent_JobDeactivationConfigurationIsDEF()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var job = new JobHeader.Loader(shipment).TryCreate();

			var portTransport = (IJobHeaderParent)Factory.New<ICommonCartage>();
			var portTransportJob = new JobHeader.Loader(portTransport).TryCreate();
			portTransportJob.JH_JH_ParentJob = job.PK;
			Factory.Save();

			AssertResult("Job set as JH_JH_ParentJob of another job", $"Child Invoicing Job(s) have been saved against this Invoicing Job Header (S00001000) in the company EDI.", shipment.PK);

			TestCaseHelper.ClearTable(AccHotChequeSchema.Constants.TableName);
			var chequeBook = CreateChequeBook();
			InsertAccHotCheque(chequeBook.PK, job.PK);

			AssertResult("Hot Check with Job", "Hot Cheque(s) have been saved against this Invoicing Job Header (S00001000) in the company EDI.", shipment.PK);

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transactionHeader.AH_JH = job.PK;
			Factory.Save();

			AssertResult("TransactionHeader with job", "Accounting Transaction(s) have been saved against this Invoicing Job Header (S00001000) in the company EDI.", shipment.PK);

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			transactionLine.AL_LineAmount = 44;
			transactionLine.AL_JH = job.PK;

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AL_ARLine = transactionLine.PK;
			jobCharge.JR_OSSellAmt = 44m;
			Factory.Save();

			AssertResult("Job charge with job", "Job Invoicing Charge(s) have been saved against this Invoicing Job Header (S00001000) in the company EDI.", shipment.PK);
		}

		public void TestCheckIfCanDeleteJobHeaderParent_JobDeactivationConfigurationIsNPL()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var job = new JobHeader.Loader(shipment).TryCreate();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_Code = "EFEE";
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Desc = "Global DSB Charge Code";
			globalChargeCode.AC_ChargeType = "DSB";
			Factory.Save();

			var localChargeCode = globalChargeCode.ChildChargeCodes.SingleOrDefault(c => c.AC_GC == GlbCompany.CurrentCompany.PK);

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			var jobDeactivationRegistryItem = (CodePairRegistryItem)ObjectFactory.Get<IAccounting>().Registry.JobDeactivationConfiguration;
			jobDeactivationRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.JobDeactivationConfigurations.NoActiveWIPACR.Code);
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.JobCosting;
			transactionHeader.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			transactionHeader.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transactionHeader.AH_JH = job.PK;
			Factory.Save();

			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var transactionLine_WithHeader = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine_WithHeader.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine_WithHeader.AL_LineType = TransactionLineTypes.WIP;
			transactionLine_WithHeader.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			transactionLine_WithHeader.AL_ReverseDate = ZDateTime.Today;
			transactionLine_WithHeader.AL_RX_NKTransactionCurrency = job.Company.GC_RX_NKLocalCurrency;
			transactionLine_WithHeader.AL_JH = job.PK;
			transactionLine_WithHeader.AL_AH = transactionHeader.PK;
			transactionLine_WithHeader.AL_GC = job.JH_GC;
			transactionLine_WithHeader.AL_GB = job.JH_GB;
			transactionLine_WithHeader.AL_GE = job.JH_GE;

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AC = localChargeCode.PK;
			jobCharge.JR_RX_NKSellCurrency = job.Company.GC_RX_NKLocalCurrency;
			jobCharge.JR_JH = transactionLine_WithHeader.AL_JH;
			jobCharge.JR_GB = transactionLine_WithHeader.AL_GB;
			jobCharge.JR_GE = transactionLine_WithHeader.AL_GE;
			Factory.Save();

			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var transactionLine_WithoutHeader = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine_WithoutHeader.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine_WithoutHeader.AL_LineType = TransactionLineTypes.WIP;
			transactionLine_WithoutHeader.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			transactionLine_WithoutHeader.AL_ReverseDate = ZDateTime.Today;
			transactionLine_WithoutHeader.AL_RX_NKTransactionCurrency = job.Company.GC_RX_NKLocalCurrency;
			transactionLine_WithoutHeader.AL_JH = job.PK;
			transactionLine_WithoutHeader.AL_GC = job.JH_GC;
			transactionLine_WithoutHeader.AL_GB = job.JH_GB;
			transactionLine_WithoutHeader.AL_GE = job.JH_GE;
			transactionLine_WithoutHeader.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			Factory.Save();

			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			TestCaseHelper.ClearTable(AccHotChequeSchema.Constants.TableName);
			var chequeBook = CreateChequeBook();
			InsertAccHotCheque(chequeBook.PK, job.PK);

			AssertResult("Hot Check with Job", "Hot Cheque(s) have been saved against this Invoicing Job Header (S00001000) in the company EDI.", shipment.PK);

			var shipment_CanNotBeDeactivated = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var job_CannotBeDeactivated = new JobHeader.Loader(shipment_CanNotBeDeactivated).TryCreate();
			var portTransport = (IJobHeaderParent)Factory.New<ICommonCartage>();
			var portTransportJob = new JobHeader.Loader(portTransport).TryCreate();
			portTransportJob.JH_JH_ParentJob = job_CannotBeDeactivated.PK;
			Factory.Save();

			AssertResult("Job set as JH_JH_ParentJob of another job", $"Child Invoicing Job(s) have been saved against this Invoicing Job Header (S00001001) in the company EDI.", shipment_CanNotBeDeactivated.PK);
		}

		public void TestBlockingReason_WhenCheckIfCanDeleteJobHeaderParent_JobDeactivationConfigurationIsNPL_HasPostedTransactions_TransactionTypeIsNotJRJ()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var job = new JobHeader.Loader(shipment).TryCreate();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_Code = "EFEE";
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Desc = "Global DSB Charge Code";
			globalChargeCode.AC_ChargeType = "DSB";
			Factory.Save();

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			var accountingRegistry = ObjectFactory.Get<IAccounting>().Registry;
			var jobDeactivationRegistryItem = (CodePairRegistryItem)accountingRegistry.JobDeactivationConfiguration;
			jobDeactivationRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.JobDeactivationConfigurations.NoActiveWIPACR.Code);
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transactionHeader.AH_JH = job.PK;
			Factory.Save();

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is false", false, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by transactionHeader.", "Invoice, Credit Note and/or Job Revenue Journal, CFX Journal have been posted against this Invoicing Job (S00001000) in the company EDI.", shipment.PK);

			accountingRegistry.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is true", true, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by transactionHeader.", "Invoice, Credit Note and/or Job Revenue Journal, CFX Journal have been posted against this Invoicing Job (S00001000) in the company EDI.", shipment.PK);
		}

		public void TestBlockingReason_WhenCheckIfCanDeleteJobHeaderParent_JobDeactivationConfigurationIsNPL_HasPostedTransactions_TransactionTypeIsJRJ()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var job = new JobHeader.Loader(shipment).TryCreate();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_Code = "EFEE";
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Desc = "Global DSB Charge Code";
			globalChargeCode.AC_ChargeType = "DSB";
			Factory.Save();

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			var accountingRegistry = ObjectFactory.Get<IAccounting>().Registry;
			var jobDeactivationRegistryItem = (CodePairRegistryItem)accountingRegistry.JobDeactivationConfiguration;
			jobDeactivationRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.JobDeactivationConfigurations.NoActiveWIPACR.Code);
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.JobCosting;
			transactionHeader.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			transactionHeader.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transactionHeader.AH_JH = job.PK;
			Factory.Save();

			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			transactionLine.AL_LineAmount = 44;
			transactionLine.AL_JH = job.PK;

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AL_ARLine = transactionLine.PK;
			jobCharge.JR_OSSellAmt = 44m;
			Factory.Save();

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is false", false, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by transactionLine.", "Invoice, Credit Note and/or Job Revenue Journal, CFX Journal have been posted against this Invoicing Job (S00001000) in the company EDI.", shipment.PK);

			accountingRegistry.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is true", true, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by transactionHeader.", "Invoice, Credit Note and/or Job Revenue Journal, CFX Journal have been posted against this Invoicing Job (S00001000) in the company EDI.", shipment.PK);
		}

		public void TestBlockingReason_WhenCheckIfCanDeleteJobHeaderParent_JobDeactivationConfigurationIsNPL_HasNotReversedWIPACR()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var job = new JobHeader.Loader(shipment).TryCreate();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_Code = "EFEE";
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Desc = "Global DSB Charge Code";
			globalChargeCode.AC_ChargeType = "DSB";
			Factory.Save();

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			var accountingRegistry = ObjectFactory.Get<IAccounting>().Registry;
			var jobDeactivationRegistryItem = (CodePairRegistryItem)accountingRegistry.JobDeactivationConfiguration;
			jobDeactivationRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.JobDeactivationConfigurations.NoActiveWIPACR.Code);
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			var transactionLine_WithoutHeader = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine_WithoutHeader.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine_WithoutHeader.AL_LineType = TransactionLineTypes.WIP;
			transactionLine_WithoutHeader.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			transactionLine_WithoutHeader.AL_ReverseDate = ZDateTime.Empty;
			transactionLine_WithoutHeader.AL_RX_NKTransactionCurrency = job.Company.GC_RX_NKLocalCurrency;
			transactionLine_WithoutHeader.AL_JH = job.PK;
			transactionLine_WithoutHeader.AL_GC = job.JH_GC;
			transactionLine_WithoutHeader.AL_GB = job.JH_GB;
			transactionLine_WithoutHeader.AL_GE = job.JH_GE;
			transactionLine_WithoutHeader.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			Factory.Save();

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is false", false, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by transactionLine.", "Please reverse all WIPs and ACRs before deactivating the job (S00001000) in the company EDI.", shipment.PK);

			accountingRegistry.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is true", true, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by transactionHeader.", @"Please delete all charges excluding system generated JRJ posted against the Electronic Processing Charge (EFEE) from this Invoicing Job (S00001000) in the company EDI.
Please reverse the WIP related to the Electronic Processing Charge (EFEE) in the company EDI.", shipment.PK);
		}

		public void TestBlockingReason_WhenCheckIfCanDeleteJobHeaderParent_JobDeactivationConfigurationIsNPL_HasSavedNonZeroJobCharges()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var job = new JobHeader.Loader(shipment).TryCreate();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_Code = "EFEE";
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Desc = "Global DSB Charge Code";
			globalChargeCode.AC_ChargeType = "DSB";
			Factory.Save();

			var localChargeCode = globalChargeCode.ChildChargeCodes.SingleOrDefault(c => c.AC_GC == GlbCompany.CurrentCompany.PK);

			var accountingRegistryProvider = ObjectFactory.Get<IAccountingRegistryProvider>();
			var accountingRegistry = ObjectFactory.Get<IAccounting>().Registry;
			var jobDeactivationRegistryItem = (CodePairRegistryItem)accountingRegistry.JobDeactivationConfiguration;
			jobDeactivationRegistryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.JobDeactivationConfigurations.NoActiveWIPACR.Code);
			accountingRegistryProvider.ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.JobCosting;
			transactionHeader.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			transactionHeader.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			transactionHeader.AH_JH = job.PK;
			Factory.Save();

			AssertResult("job without blocking parent", string.Empty, shipment.PK);

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_AC = localChargeCode.PK;
			transactionLine.AL_LineAmount = 44;
			transactionLine.AL_JH = job.PK;

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_AL_ARLine = transactionLine.PK;
			jobCharge.JR_OSSellAmt = 44m;
			Factory.Save();

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is false", false, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by jobCharge.", "Please delete all charges from this Invoicing Job (S00001000) in the company EDI.", shipment.PK);

			accountingRegistry.EnableElectronicProcessingChargeFunctionality.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			AssertEquals("Precondition: EnableElectronicProcessingChargeFunctionality is true", true, accountingRegistry.EnableElectronicProcessingChargeFunctionality.Value);
			AssertResult("Blocking by jobCharge.", @"Please delete all charges excluding system generated JRJ posted against the Electronic Processing Charge (EFEE) from this Invoicing Job (S00001000) in the company EDI.
Please reverse the WIP related to the Electronic Processing Charge (EFEE) in the company EDI.", shipment.PK);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestCheckIfCanCancel()
		{
			var parentPK = ZGuid.NewZGuid();
			var foreignCompany = Factory.New<GlbCompany>();
			foreignCompany.GC_Code = "BLA";
			Factory.Save();

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;   // To prevent auto creating WIP/ACR on saving
			job.JH_ParentID = parentPK;
			job.JH_JobNum = "LocalJobNum";

			var foreignJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			foreignJob.JH_ParentTableCode = "XX";
			foreignJob.JH_ParentID = parentPK;
			foreignJob.JH_GC = foreignCompany.PK;
			foreignJob.JH_JobNum = "ForeignJobNum";
			Factory.Save();
			AssertResult("No job headers with associated transactions  - Can Cancel", string.Empty, parentPK);

			//Charges
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			Factory.Save();
			bool allAmountsZero = charge.JR_OSCostAmt == 0m && charge.JR_LocalCostAmt == 0m && charge.JR_EstimatedCost == 0m && charge.JR_AgentDeclaredCostAmt == 0m &&
				charge.JR_OSSellAmt == 0m && charge.JR_LocalSellAmt == 0m && charge.JR_EstimatedRevenue == 0m && charge.JR_AgentDeclaredSellAmt == 0m;
			Assert("Should be Empty JR_OSCostAmt, JR_LocalCostAmt, JR_EstimatedCost, JR_AgentDeclaredCostAmt, JR_OSSellAmt, JR_LocalSellAmt, JR_EstimatedRevenue, JR_AgentDeclaredSellAmt amounts on the JobCharge", allAmountsZero);
			AssertResult("No job headers with non empty charges - Can Cancel", string.Empty, parentPK);

			charge.JR_OSCostAmt = 5m;
			charge.JR_OSCostExRate = 1m;
			Factory.Save();
			AssertResult("Job with non-zero Charge - cannot cancel", "Job Invoicing Charge(s) have been saved against this Invoicing Job Header (LocalJobNum) in the company EDI.", parentPK);

			charge.Delete();
			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = foreignJob.PK;
			Factory.Save();
			AssertResult("Foreign job with 0 amount charge - can cancel", string.Empty, parentPK);

			//TransactionLines
			charge.Delete();
			var line1 = Factory.NewWithValidTestData<AccTransactionLines>();
			line1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			line1.AL_JH = job.PK;
			Factory.Save();
			AssertResult("Job with Transaction line - cannot cancel", "Accounting Transaction Line(s) have been saved against this Invoicing Job Header (LocalJobNum) in the company EDI.", parentPK);

			var newJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			line1.AL_JH = newJob.PK;
			var line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			line2.AL_JH = foreignJob.PK;
			line2.AL_GC = foreignJob.JH_GC;
			Factory.Save();
			AssertResult("Foreign Job with Transaction line - cannot cancel", "Accounting Transaction Line(s) have been saved against this Invoicing Job Header (ForeignJobNum) in the company BLA.", parentPK);

			//TransactionHeaders
			line2.AL_JH = ZGuid.Empty;
			var header1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header1.AH_JH = job.PK;
			Factory.Save();
			AssertResult("Job with Transaction Header - cannot cancel", "Accounting Transaction(s) have been saved against this Invoicing Job Header (LocalJobNum) in the company EDI.", parentPK);

			header1.AH_JH = ZGuid.Empty;
			var header2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header2.AH_JH = foreignJob.PK;
			header2.AH_GC = foreignJob.JH_GC;
			Factory.Save();
			AssertResult("Job with Foregin Transaction Header - cannot cancel", "Accounting Transaction(s) have been saved against this Invoicing Job Header (ForeignJobNum) in the company BLA.", parentPK);

			//AccHotCheques
			TestCaseHelper.ClearTable(AccHotChequeSchema.Constants.TableName);
			header2.AH_JH = ZGuid.Empty;
			var chequeBook = CreateChequeBook();
			InsertAccHotCheque(chequeBook.PK, job.PK);
			AssertResult("Job with AccHotCheque - cannot cancel", "Hot Cheque(s) have been saved against this Invoicing Job Header (LocalJobNum) in the company EDI.", parentPK);

			TestCaseHelper.ClearTable(AccHotChequeSchema.Constants.TableName);
			InsertAccHotCheque(chequeBook.PK, foreignJob.PK);
			AssertResult("Job with Foreign AccHotCheque - cannot cancel", "Hot Cheque(s) have been saved against this Invoicing Job Header (ForeignJobNum) in the company BLA.", parentPK);

			TestCaseHelper.ClearTable(AccHotChequeSchema.Constants.TableName);
			AssertResult("No job headers with associated transactions  - Can Cancel", string.Empty, parentPK);

			//Job Headers
			var portTransport = (IJobHeaderParent)Factory.New<ICommonCartage>();
			var portTransportJobInCurrentCompany = new JobHeader.Loader(portTransport).TryCreate();
			portTransportJobInCurrentCompany.JH_JH_ParentJob = job.PK;
			Factory.Save();
			AssertResult("Current company job set as JH_JH_ParentJob of another job - cannot cancel", $"Child Invoicing Job(s) have been saved against this Invoicing Job Header (LocalJobNum) in the company EDI.", parentPK);

			portTransportJobInCurrentCompany.MarkAsInactive();
			var portTransportJobInNonCurrentCompany = new JobHeader.Loader(portTransport).TryCreate();
			portTransportJobInNonCurrentCompany.JH_GC = foreignCompany.PK;
			portTransportJobInNonCurrentCompany.JH_JH_ParentJob = foreignJob.PK;
			Factory.Save();
			AssertResult("Non-current company job set as JH_JH_ParentJob of another job - cannot cancel", $"Child Invoicing Job(s) have been saved against this Invoicing Job Header (ForeignJobNum) in the company BLA.", parentPK);

			portTransportJobInNonCurrentCompany.MarkAsInactive();
			Factory.Save();
			AssertResult("Current company job and Non-current company job not set as JH_JH_ParentJob - can cancel", string.Empty, parentPK);
		}

		void AssertResult(string assertMessage, string expectedMessage, ZGuid parentPK)
		{
			const string businessObjectName = "This record";
			var expectedCancelMessage = string.Empty;
			var expectedDeleteMessage = string.Empty;

			if (!string.IsNullOrEmpty(expectedMessage))
			{
				expectedCancelMessage = $@"{businessObjectName} cannot be deactivated.
{expectedMessage}";
				expectedDeleteMessage = $@"{businessObjectName} cannot be deleted.
{expectedMessage}";
			}

			AssertEquals(assertMessage, expectedCancelMessage, JobHeaderParentDeletionHelper.CheckIfCanCancelJobHeaderParent(parentPK, businessObjectName));
			AssertEquals(assertMessage, expectedDeleteMessage, JobHeaderParentDeletionHelper.CheckIfCanDeleteJobHeaderParent(parentPK, businessObjectName));
		}

		AccChequeBook CreateChequeBook()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountNum = "999.888.88";
			glHeader.AG_AccountType = "BSH";

			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "TSTBNK";
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_AG = glHeader.PK;

			var chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 12;
			chequeBook.AK_CurrentNo = 5;
			Factory.Save();

			return chequeBook;
		}

		void InsertAccHotCheque(ZGuid chequeBookPK, ZGuid jobPK)
		{
			string sql = $"INSERT INTO dbo.AccHotCheque (AQ_PK, AQ_AK, AQ_ActualOrMaxIndicator, AQ_JH, AQ_SystemCreateTimeUtc, AQ_SystemCreateUser, AQ_SystemLastEditTimeUtc, AQ_SystemLastEditUser) values (NEWID(), '{chequeBookPK}', '{ZArchitecture.Core.ActualOrMaxIndicator.Actual}', '{jobPK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
