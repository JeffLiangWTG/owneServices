using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccTransactionLinesTest : BusinessObjectValidationTestCase
	{
		public void TestAL_GC_HeaderAndLinesBranchCompanyAreInconsistent()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, GlbBranch.CurrentBranch.PK));
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CAU";
			var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherBranch.GB_GC = company.PK;
			AssertNotEquals("Pre-Requisite.", branch.Company.PK, anotherBranch.Company.PK);

			var (line, _) = SetupLineAndChargeForTest(lineAmount: 50m);

			line.TransactionHeader.AH_GB = branch.PK;
			line.AL_GB = anotherBranch.PK;
			AssertHasError(line.AL_GCInfo, "The company of header: EDI is inconsistent with the company of line: CAU.");

			line.AL_GC = ZGuid.Empty;
			AssertNoError(line.AL_GCInfo, "The company of header: EDI is inconsistent with the company of line: CAU.");

			line.AL_GB = anotherBranch.PK;
			AssertHasError(line.AL_GCInfo, "The company of header: EDI is inconsistent with the company of line: CAU.");

			line.AL_GC = ZGuid.Invalid;
			AssertNoError(line.AL_GCInfo, "The company of header: EDI is inconsistent with the company of line: CAU.");
		}

		public void TestAL_AG_SetAlternateGLAccountDissections()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.General;
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var dissection1 = glHeader.AlternateGLAccountDissections.AddNew();
			dissection1.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			dissection1.ADC_SeparateNumbering = true;
			var dissection2 = glHeader.AlternateGLAccountDissections.AddNew();
			dissection2.ADC_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR;
			dissection2.ADC_SeparateNumbering = false;

			var dissectionAttributes = transactionLine.AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>();
			AssertEquals(0, dissectionAttributes.Count());
			transactionLine.AL_AG = glHeader.PK;
			AssertEquals(2, dissectionAttributes.Count());
			Assert(dissectionAttributes.Any(x => x.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG));
			Assert(dissectionAttributes.Any(x => x.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR));

			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			transactionLine.AccTransactionLineDissectionAttributes.RemoveAndDeleteAll();
			AssertEquals(0, dissectionAttributes.Count());
			transactionLine.AL_AG = glHeader.PK;
			AssertEquals(0, dissectionAttributes.Count());
		}

		#region Test for Tax Id And Tax Message Mapping Validation

		public void TestTaxIDAndTaxMessageMappingValidation_WhenSetTaxRate()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TaxRate01";
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Code = "TaxRate02";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			var bizObj = Factory.NewWithValidTestData<AccTransactionLines>();
			bizObj.AL_LineType = TransactionLineTypes.Cost;
			bizObj.AL_AT = taxRate1.PK;

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
				(TransactionLineTypes.Cost, taxRate1, taxMsg1),
				(TransactionLineTypes.Cost, taxRate2, taxMsg2));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			bizObj.AL_A9_VATClass = taxMsg1.PK;
			bizObj.AL_AT = taxRate1.PK;
			AssertNoErrors(bizObj.AL_A9_VATClassInfo);

			bizObj.AL_AT = taxRate2.PK;
			AssertHasErrors("Error Mapping", bizObj.AL_A9_VATClassInfo);
		}

		public void TestAL_JH_DefaultGLAccounts()
		{
			var glAccount1 = Factory.NewWithValidTestData<AccGLHeader>();
			var glAccount2 = Factory.NewWithValidTestData<AccGLHeader>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGAA1";

			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000011";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AG_CostAccount = glAccount1.PK;

			var gLPostingOverrides = chargeCode.GLPostingOverrides;
			gLPostingOverrides.DeleteAll();
			var glPostingOverride = gLPostingOverrides.AddNew();
			glPostingOverride.Y1_ConsolidationAccountingCategoryClass = "ALL";
			glPostingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			glPostingOverride.Y1_JobType = "ALL";
			glPostingOverride.Y1_TransportMode = "ALL";
			glPostingOverride.Y1_Direction = "ALL";
			glPostingOverride.Y1_ConsolContainerMode = "ALL";
			glPostingOverride.Y1_MasterPaymentType = "ALL";
			glPostingOverride.Y1_HousePaymentType = "ALL";
			glPostingOverride.Y1_AG_CST = glAccount2.PK;
			glPostingOverride.Y1_AG_ACR = glAccount2.PK;
			glPostingOverride.Y1_AG_WIP = glAccount2.PK;
			glPostingOverride.Y1_AG_REV = glAccount2.PK;

			Factory.Save();

			var invoiceLine = Factory.New(typeof(AccTransactionLines)) as AccTransactionLines;
			invoiceLine.AL_LineType = TransactionLineTypes.Cost;
			invoiceLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			invoiceLine.AL_OH = org.PK;
			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_AG = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, invoiceLine.AL_AG);

			invoiceLine.AL_JH = job.PK;
			AssertEquals(glAccount2.PK, invoiceLine.AL_AG);
		}

		public void TestTransactionLineAmountChangeWhenItsLinkedToPostedCharge_NotCollectedWhenNoChangeInLineAmount()
		{
			var criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var (line, _) = SetupLineAndChargeForTest(lineAmount: 50m);

			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			line.AL_LineAmount = 50m;
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			line.AL_LineAmount = 45m;
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains($@"AL_LineAmount has been changed from 50 to 45 after Job charge creation.
JobCharge LocalAmount: 50

StackTrace:",
criticalValidationError);
		}

		public void TestTransactionLineAmountChangeWhenItsLinkedToPostedCharge_NotCollectedWhenThereIsNoJobCharge()
		{
			var criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var (line, _) = SetupLineAndChargeForTest(createJobCharge: false, lineAmount: 50m);

			line.AL_LineAmount = 45m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var charge = CreateJobCharge(line, null, 50m);
			charge.JR_JH = line.AL_JH;
			charge.JR_AL_ARLine = line.PK;

			line.AL_LineAmount = 60m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains($@"AL_LineAmount has been changed from 45 to 60 after Job charge creation.
JobCharge LocalAmount: 50

StackTrace:",
criticalValidationError);
		}

		public void TestTransactionLineAmountChangeWhenItsLinkedToPostedCharge_NotCollectedWhenIsRevenuePostedFalse()
		{
			var criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var (line, _) = SetupLineAndChargeForTest(lineAmount: 50m);

			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_LineAmount = 45m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_LineAmount = 60m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains($@"AL_LineAmount has been changed from 45 to 60 after Job charge creation.
JobCharge LocalAmount: 50

StackTrace:",
criticalValidationError);
		}

		public void TestTransactionLineOSAmountChangeWhenItsLinkedToPostedCharge_NotCollectedWhenNoChangeInLineAmount()
		{
			var criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var (line, _) = SetupLineAndChargeForTest(lineAmount: 50m);

			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			line.AL_OSAmount = 50m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			line.AL_OSAmount = 60m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains($@"AL_OSAmount has been changed from 50 to 60 after Job charge creation.
JobCharge OSAmount: 50

StackTrace:",
criticalValidationError);
		}

		public void TestTransactionLineOSAmountChangeWhenItsLinkedToPostedCharge_NotCollectedWhenThereIsNoJobCharge()
		{
			var criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var (line, _) = SetupLineAndChargeForTest(createJobCharge: false, lineAmount: 50m);

			line.AL_OSAmount = 45m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var charge = CreateJobCharge(line, null, 50m);
			charge.JR_JH = line.AL_JH;
			charge.JR_AL_ARLine = line.PK;

			line.AL_OSAmount = 60m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains($@"AL_OSAmount has been changed from 45 to 60 after Job charge creation.
JobCharge OSAmount: 50

StackTrace:",
criticalValidationError);
		}

		public void TestTransactionLineOSAmountChangeWhenItsLinkedToPostedCharge_NotCollectedWhenIsRevenuePostedFalse()
		{
			var criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var (line, _) = SetupLineAndChargeForTest(lineAmount: 50m);

			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_OSAmount = 45m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_OSAmount = 60m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains($@"AL_OSAmount has been changed from 45 to 60 after Job charge creation.
JobCharge OSAmount: 50

StackTrace:",
criticalValidationError);
		}

		public void TestTransactionLineOSAmountChangeWhenItsLinkedToPostedCharge_NotCollectedWhenLineAndChargeCurrenciesDifferent()
		{
			var criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			var (line, charge) = SetupLineAndChargeForTest(lineAmount: 50m);

			charge.JR_RX_NKSellCurrency = "AUD";
			line.AL_RX_NKTransactionCurrency = "USD";

			line.AL_OSAmount = 45m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains("TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge: There is no data collected for this PK", criticalValidationError);

			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_OSAmount = 60m; // Act
			criticalValidationError = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionLineOSAmountChangeWhenItsLinkedToPostedCharge);
			AssertContains($@"AL_OSAmount has been changed from 45 to 60 after Job charge creation.
JobCharge OSAmount: 50

StackTrace:",
criticalValidationError);
		}

		public void TestDisableWorkflowSettingPropertiesAfterOnSavingAttribute()
		{
			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			var componentType = line.GetType();
			var ignoreBoAttributes = componentType.GetCustomAttributes(typeof(DisableWorkflowSettingPropertiesAfterOnSavingAttribute), true);
			AssertEquals(1, ignoreBoAttributes.Length);
			AssertNotNull("DisableWorkflowSettingPropertiesAfterOnSaving Attribute", ignoreBoAttributes[0] as DisableWorkflowSettingPropertiesAfterOnSavingAttribute);
		}

		(AccTransactionLines, JobCharge) SetupLineAndChargeForTest(bool createJobCharge = true, decimal lineAmount = 100M)
		{
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = header.PK;
			line.AL_JH = job.PK;
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			line.AL_OSAmount = line.AL_LineAmount = lineAmount;
			line.AL_RX_NKTransactionCurrency = "AUD";

			JobCharge charge = null;
			if (createJobCharge)
			{
				charge = CreateJobCharge(line, job, lineAmount);
			}

			return (line, charge);
		}

		JobCharge CreateJobCharge(AccTransactionLines line, JobHeader job, decimal sellAmount)
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job != null ? job.PK : ZGuid.Empty;
			charge.JR_AL_ARLine = line.PK;
			charge.JR_RX_NKSellCurrency = "AUD";

			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = sellAmount;

			return charge;
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		#endregion
	}
}
