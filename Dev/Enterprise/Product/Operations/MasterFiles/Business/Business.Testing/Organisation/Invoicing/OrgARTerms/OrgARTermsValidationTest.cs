using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgARTermsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPY_AgreedPaymentMethod()
		{
			AssertEquals(true, organisation.OH_IsDebtor);

			arTerms.PY_AgreedPaymentMethod = "CRQ";
			AssertNoErrors("Should be no errors", arTerms.PY_AgreedPaymentMethodInfo);

			arTerms.PY_AgreedPaymentMethod = "RRR";
			AssertHasErrors("Should be errors", arTerms.PY_AgreedPaymentMethodInfo);

			arTerms.PY_AgreedPaymentMethod = "";
			AssertNoErrors("Should be no errors", arTerms.PY_AgreedPaymentMethodInfo);

			organisation.OH_IsDebtor = false;
			arTerms.PY_InvoiceTerm = "RRR";
			AssertNoErrors("Should be no errors", arTerms.PY_AgreedPaymentMethodInfo);
		}

		public void TestCheckPY_AgreedPaymentMethod_UsesPreferredPaymentMethod()
		{
			var expectedWarningMessage = "Please consider using Credit Card as payment method.";
			var mockPreferredPaymentMethod = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IPreferredPaymentMethod>();
			mockPreferredPaymentMethod.Setup(x => x.GetPreferredPaymentMethodWarning(It.IsAny<string>(), It.IsAny<string>()))
				.Returns((string orgCategory, string paymentMethod) =>
				{
					return (paymentMethod != OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard) ? expectedWarningMessage : null;
				});

			var term = companyData.ARTerms.AddNew();

			term.PY_AgreedPaymentMethod = "";
			AssertNoWarnings("No warning expected when no payment method is set", term.PY_AgreedPaymentMethodInfo);

			term.PY_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			AssertHasWarning(term.PY_AgreedPaymentMethodInfo, expectedWarningMessage);

			term.PY_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard;
			AssertNoWarnings("No warning expected when preferred payment method is selected", term.PY_AgreedPaymentMethodInfo);
		}

		public void TestCheckPY_InvoiceDays()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_IsDebtor = true;
			var orgARTermCollection = new OrgARTermsCollection(companyData);
			Factory.Save();
			var term = companyData.ARTerms.AddNew();
			term.PY_InvoiceTerm = InvoiceTermsList.MonthsFromInvoiceCycleDate.Code;
			term.PY_InvoiceDays = 2;
			AssertNoErrors("2 months invoice term is valid", term.PY_InvoiceDaysInfo);

			term.PY_InvoiceDays = 4;
			AssertNoErrors("4 months invoice term is valid but will have warning", term.PY_InvoiceDaysInfo);
			AssertHasWarningContaining(term.PY_InvoiceDaysInfo, "For MIC invoice term, the value entered refers to number of term months. 4 months will result in invoice due date of more than 120 days from now.");

			term.PY_InvoiceDays = 60;
			AssertHasErrorContaining(term.PY_InvoiceDaysInfo, "For MIC invoice term, the value entered refers to number of term months. 60 months will result in invoice due date of more than 5 years from now and thus is not valid.");

			term.PY_InvoiceDays = 65;
			AssertHasErrorContaining(term.PY_InvoiceDaysInfo, "For MIC invoice term, the value entered refers to number of term months. 65 months will result in invoice due date of more than 5 years from now and thus is not valid.");
		}

		public void TestCheckPY_InvoiceDays_ForDLP()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_IsDebtor = true;

			Factory.Save();

			var term = companyData.ARTerms.AddNew();
			term.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;

			term.PY_InvoiceDays = 99;
			AssertNoErrors("99 Days is a valid value when the invoice term is DLP", term.PY_InvoiceDaysInfo);

			term.PY_InvoiceDays = 100;
			AssertHasErrorContaining(term.PY_InvoiceDaysInfo, "Days must be between 0 and 99.");
		}

		public void TestCheckPY_InvoiceTerm()
		{
			AssertEquals(true, organisation.OH_IsDebtor);

			arTerms.PY_InvoiceTerm = "RRR";
			arTerms.RunPreSaveValidation();
			AssertHasErrors("Company is debtor, invalid code, has errors", arTerms.PY_InvoiceTermInfo);

			arTerms.PY_InvoiceTerm = "";
			AssertHasErrors("Company is debtor, no code, has errors", arTerms.PY_InvoiceTermInfo);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertHasError(arTerms.PY_InvoiceTermInfo, "Invoice Cycle data must be entered.");
			arTerms.ARTermsCycles.AddNew();
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms.PY_InvoiceTermInfo);
			arTerms.ARTermsCycles.DeleteAll();

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.TermDaysAndDebtorPaymentCycle;
			AssertHasError(arTerms.PY_InvoiceTermInfo, "Payment Cycle data must be entered.");
			arTerms.ARPaymentCycles.AddNew();
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms.PY_InvoiceTermInfo);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			AssertNoErrors(arTerms.PY_InvoiceTermInfo);
			arTerms.ARTermsInstallments.AddNew();
			arTerms.RunPreSaveValidation();
			AssertHasError(arTerms.PY_InvoiceTermInfo, "Multiple Installments grid can't have one row only.");
			arTerms.ARTermsInstallments.AddNew();
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms.PY_InvoiceTermInfo);

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromMonthEnd;
			AssertNoErrors("Company is debtor, valid code, no errors", arTerms.PY_InvoiceTermInfo);

			companyData.OB_AROnCreditHold = false;
			companyData.OB_ARCreditApproved = false;
			arTerms.PY_InvoiceTerm = "INV";
			AssertNoErrors("Changed invoice terms to 'INV', no errors", arTerms.PY_InvoiceTermInfo);
			arTerms.PY_InvoiceTerm = "COD";
			AssertNoErrors("Changed invoice terms to 'COD', no errors", arTerms.PY_InvoiceTermInfo);

			companyData.OB_AROnCreditHold = true;
			companyData.OB_ARCreditApproved = true;
			arTerms.PY_InvoiceTerm = "INV";
			AssertNoErrors("Changed invoice terms to 'INV', no errors", arTerms.PY_InvoiceTermInfo);
			arTerms.PY_InvoiceTerm = "COD";
			AssertNoErrors("Changed invoice terms to 'COD', no errors", arTerms.PY_InvoiceTermInfo);

			companyData.OB_ARCreditApproved = true;
			companyData.OB_AROnCreditHold = false;

			arTerms.PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			AssertHasError(arTerms.PY_InvoiceTermInfo, "'DEF' term can't be set if AR Settlement Group is empty.");
			AssertEquals(false, arTerms.Validation.CanDefaultARTermBeSet);

			organisation.ARSettlementGroupPK = organisation.PK;
			arTerms.MarkAsNeedingValidation();
			arTerms.RunPreSaveValidation();
			AssertHasError(arTerms.PY_InvoiceTermInfo, "'DEF' term can't be set if AR Settlement Group is set to itself.");
			AssertEquals(false, arTerms.Validation.CanDefaultARTermBeSet);

			organisation.ARSettlementGroupPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			organisation.ARSettlementGroup.ARSettlementGroupPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			organisation.ARSettlementGroup.CompanyData.OB_IsDebtor = true;
			arTerms.MarkAsNeedingValidation();
			arTerms.RunPreSaveValidation();
			AssertNoErrors(arTerms.PY_InvoiceTermInfo);
			AssertEquals(true, arTerms.Validation.CanDefaultARTermBeSet);

			arTerms.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.All.Code;
			organisation.ARSettlementGroup.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			arTerms.MarkAsNeedingValidation();
			arTerms.RunPreSaveValidation();
			AssertHasError(arTerms.PY_InvoiceTermInfo, "'DEF' term can't be set if settlement organization also has 'DEF' term.");
			AssertEquals(false, arTerms.Validation.CanDefaultARTermBeSet);

			arTerms.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			arTerms.MarkAsNeedingValidation();
			arTerms.RunPreSaveValidation();
			AssertHasError(arTerms.PY_InvoiceTermInfo, "'DEF' term can't be set if settlement organization also has 'DEF' term.");
			AssertEquals(false, arTerms.Validation.CanDefaultARTermBeSet);

			organisation.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm();
			arTerms.MarkAsNeedingValidation();
			arTerms.RunPreSaveValidation();
			AssertHasError(arTerms.PY_InvoiceTermInfo, "'DEF' term can't be set if settlement organization also has 'DEF' term.");
			AssertEquals(false, arTerms.Validation.CanDefaultARTermBeSet);

			organisation.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			arTerms.MarkAsNeedingValidation();
			arTerms.RunPreSaveValidation();
			AssertHasError(arTerms.PY_InvoiceTermInfo, "'DEF' term can't be set if settlement organization also has 'DEF' term.");
			AssertEquals(false, arTerms.Validation.CanDefaultARTermBeSet);

			OrgHeader anotherChildOrg = Factory.NewWithValidTestData<OrgHeader>();
			anotherChildOrg.ARSettlementGroupPK = organisation.ARSettlementGroupPK;

			Factory.Save();

			//organisation.ARSettlementGroup.CompanyData.OB_IsDebtor = true;
			organisation.ARSettlementGroup.CompanyData.MarkAsNeedingValidation();
			organisation.ARSettlementGroup.CompanyData.RunPreSaveValidation();
			AssertHasError(organisation.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTermInfo, "'DEF' term can't be set if this organization is settlement group for another organization with 'DEF' term.");

			arTerms.PY_InvoiceTerm = Constants.InvoiceTerms.FromInvoiceDate;
			organisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			Factory.Save();
			organisation.ARSettlementGroup.CompanyData.MarkAsNeedingValidation();
			organisation.ARSettlementGroup.CompanyData.RunPreSaveValidation();
			AssertHasError(organisation.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTermInfo, "'DEF' term can't be set if this organization is settlement group for another organization with 'DEF' term.");

			organisation.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.FromPeriodEnd;
			anotherChildOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgARTermsLookups.DefaultInvoiceTerm.Code;
			Factory.Save();
			organisation.ARSettlementGroup.CompanyData.MarkAsNeedingValidation();
			organisation.ARSettlementGroup.CompanyData.RunPreSaveValidation();
			AssertHasError(organisation.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTermInfo, "'DEF' term can't be set if this organization is settlement group for another organization with 'DEF' term.");

			anotherChildOrg.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = Constants.InvoiceTerms.PaymentInAdvance;
			Factory.Save();
			organisation.ARSettlementGroup.CompanyData.MarkAsNeedingValidation();
			organisation.ARSettlementGroup.CompanyData.RunPreSaveValidation();
			AssertNoErrors(organisation.ARSettlementGroup.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTermInfo);

			organisation.OH_IsDebtor = false;
			arTerms.PY_InvoiceTerm = "RRR";
			AssertNoErrors("Company is not debtor, no errors", arTerms.PY_InvoiceTermInfo);
		}

		public void TestCheckPY_InvoiceClass()
		{
			AssertEquals(true, organisation.OH_IsDebtor);

			arTerms.PY_InvoiceClass = "RRR";
			arTerms.RunPreSaveValidation();
			AssertHasError("Company is debtor, invalid code, has errors", arTerms.PY_InvoiceClassInfo, "Enter a valid Invoice Type.");

			var defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");

			arTerms.PY_InvoiceClass = "DSB";
			arTerms.Validation.ValidatePY_InvoiceClass();
			AssertNoErrors("Company is debtor, valid code, no errors", arTerms.PY_InvoiceClassInfo);

			arTerms.PY_InvoiceClass = "";
			AssertHasErrors("Company is debtor, no code, has errors", arTerms.PY_InvoiceClassInfo);

			arTerms.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			AssertNoErrors("Company is debtor, valid code, no errors", arTerms.PY_InvoiceClassInfo);

			organisation.OH_IsDebtor = false;
			AssertNoErrors("Company is not debtor, no errors", arTerms.PY_InvoiceClassInfo);

			arTerms.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			arTerms.PY_InvoiceClass = "ALL";
			AssertNoErrors("Company is debtor, valid code, no errors", arTerms.PY_InvoiceClassInfo);
			arTerms.PY_InvoiceClass = OrgARTermsLookups.InvoiceTypes.DSB.Code;
			AssertNoErrors("Company is debtor, valid code, no errors", arTerms.PY_InvoiceClassInfo);
			arTerms.PY_InvoiceClass = JobInvoicingConsumerTypes.AgencyBillOfLading.InvoiceTypeList[0].Code;
			AssertNoErrors("Company is debtor, Invalid code, has errors", arTerms.PY_InvoiceClassInfo);
			arTerms.PY_InvoiceClass = JobInvoicingConsumerTypes.Shipment.InvoiceTypeList[0].Code;
			AssertNoErrors("Company is debtor, valid code, no errors", arTerms.PY_InvoiceClassInfo);
		}

		public void TestCheckPY_JobType()
		{
			var defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");

			arTerms.PY_JobType = "RRR";
			AssertHasError("invalid code, has errors", arTerms.PY_JobTypeInfo, "Enter a valid selection.");

			arTerms.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertNoErrors("valid code, no errors", arTerms.PY_JobTypeInfo);
		}

		public void TestCheckPY_WhenInvoiceTermIsDLP_AndWhenJobTypeIsSHP_ThenExpectNoError()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_IsDebtor = true;
			var term = companyData.ARTerms.AddNew();

			Factory.Save();

			term.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			term.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertNoErrors("Expected no error", term.PY_InvoiceTermInfo);
		}

		public void TestCheckPY_WhenInvoiceTermIsDLP_AndWhenJobTypeIsNonSHPorBRKorTRN_ThenExpectError()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_IsDebtor = true;
			var term = companyData.ARTerms.AddNew();

			Factory.Save();

			term.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			term.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertNoErrors("Expected no error with DLP Invoice Term and Shipment Job Type", term.PY_InvoiceTermInfo);

			term.PY_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			term.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertNoErrors("Expected no error with DLP Invoice Term and Brokerage Job Type", term.PY_InvoiceTermInfo);

			term.PY_JobType = JobInvoicingConsumerTypes.LocalCartageCode;
			term.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertNoErrors("Expected no error with DLP Invoice Term and Port Transport Job Type", term.PY_InvoiceTermInfo);

			term.PY_JobType = "All";
			term.PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;
			AssertHasError("Expected an error", term.PY_InvoiceTermInfo, "DLP invoice term is only available for Shipment, Brokerage and Port Transport Job Type.");
		}

		public void TestCheckPY_Direction()
		{
			var defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");

			arTerms.PY_Direction = "ZZZ";
			AssertHasError("invalid code, has errors", arTerms.PY_DirectionInfo, "Enter a valid selection.");

			arTerms.PY_Direction = "EXP";
			AssertNoErrors("valid code, no errors", arTerms.PY_DirectionInfo);
		}

		public void TestCheckPY_TransportMode()
		{
			var defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");

			arTerms.PY_TransportMode = "ZZZ";
			AssertHasError("invalid code, has errors", arTerms.PY_TransportModeInfo, "Enter a valid selection.");

			arTerms.PY_TransportMode = "AIR";
			AssertNoErrors("valid code, no errors", arTerms.PY_TransportModeInfo);
		}

		public void TestPY_GB()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");

			arTerms.PY_GB_Branch = ZGuid.NewZGuid();
			AssertHasError("invalid code, has errors", arTerms.PY_GB_BranchInfo, "Enter a valid selection.");

			arTerms.PY_GB_Branch = branch.PK;
			AssertNoErrors("valid code, no errors", arTerms.PY_GB_BranchInfo);
		}

		public void TestPY_GE()
		{
			var dept = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var defaultTerms = companyData.ARTerms.AddNew();
			SetupTermsInfo(defaultTerms, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");

			arTerms.PY_GE_Department = ZGuid.NewZGuid();
			AssertHasError("invalid code, has errors", arTerms.PY_GE_DepartmentInfo, "Enter a valid selection.");

			arTerms.PY_GE_Department = dept.PK;
			AssertNoErrors("valid code, no errors", arTerms.PY_GE_DepartmentInfo);
		}

		public void TestDuplicateRow()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			var dept2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			var terms1 = companyData.ARTerms.AddNew();
			var terms2 = companyData.ARTerms.AddNew();
			var terms3 = companyData.ARTerms.AddNew();

			SetupTermsInfo(terms3, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");
			SetupTermsInfo(terms1, JobInvoicingConsumerTypes.ShipmentCode, branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			var errorMsg = string.Format(@"Term settings for following already exists -
Job Type: SHP, Direction: EXP, Transport Mode: SEA, Branch: {0}, Dept.: {1}, Invoice type: DSB", branch1.GB_Code, dept1.GE_Code);

			SetupTermsInfo(terms2, "BRK", branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			AssertNoRowError(terms2, errorMsg);

			terms2.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertHasRowError(terms2, errorMsg);
			terms2.PY_JobType = "BRK";
			AssertNoRowError(terms2, errorMsg);
			terms2.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			terms2.PY_Direction = "EXP";
			terms2.PY_TransportMode = "SEA";

			terms2.PY_GB_Branch = branch2.PK;
			AssertNoRowError(terms2, errorMsg);
			terms2.PY_GB_Branch = branch1.PK;
			AssertHasRowError(terms2, errorMsg);

			terms2.PY_GE_Department = dept2.PK;
			AssertNoRowError(terms2, errorMsg);
			terms2.PY_GE_Department = dept1.PK;
			AssertHasRowError(terms2, errorMsg);

			terms2.PY_Direction = "IMP";
			AssertNoRowError(terms2, errorMsg);
			terms2.PY_Direction = "EXP";
			AssertHasRowError(terms2, errorMsg);

			terms2.PY_TransportMode = "AIR";
			AssertNoRowError(terms2, errorMsg);
			terms2.PY_TransportMode = "SEA";
			AssertHasRowError(terms2, errorMsg);

			terms2.PY_InvoiceClass = "ALL";
			AssertNoRowError(terms2, errorMsg);
			terms2.PY_InvoiceClass = "DSB";
			AssertHasRowError(terms2, errorMsg);
		}

		[ExpectNoExceptions("System.NullReferenceException shouldn't occur")]
		public void TestDuplicateRow_WithEmptyDepartmentOrBranch()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			var dept2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			var terms1 = companyData.ARTerms.AddNew();
			var terms2 = companyData.ARTerms.AddNew();
			var terms3 = companyData.ARTerms.AddNew();

			SetupTermsInfo(terms3, "ALL", ZGuid.Empty, ZGuid.Empty, "", "", "ALL");
			terms3.PY_GB_Branch = ZGuid.Invalid;
			terms3.PY_GE_Department = ZGuid.Invalid;
			SetupTermsInfo(terms1, JobInvoicingConsumerTypes.ShipmentCode, branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			var errorMsg = string.Format(@"Term settings for following already exists -
Job Type: SHP, Direction: EXP, Transport Mode: SEA, Branch: {0}, Dept.: {1}, Invoice type: DSB", branch1.GB_Code, dept1.GE_Code);

			SetupTermsInfo(terms2, "BRK", branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			AssertNoRowError(terms2, errorMsg);

			terms2.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertHasRowError(terms2, errorMsg);
			terms2.PY_JobType = "BRK";
			AssertNoRowError(terms2, errorMsg);
		}

		public void TestDefaultRow()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			var dept2 = Factory.NewWithValidTestData<GlbDepartment>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsDebtor = true;

			var companyData = org1.CompanyData;
			companyData.ARTerms.DeleteAll();

			Factory.Save();

			var terms1 = companyData.ARTerms.AddNew();
			var terms2 = companyData.ARTerms.AddNew();

			SetupTermsInfo(terms2, "ALL", ZGuid.Empty, ZGuid.Empty, ZString.Empty, ZString.Empty, "ALL");
			SetupTermsInfo(terms1, JobInvoicingConsumerTypes.ShipmentCode, branch1.PK, dept1.PK, "EXP", "SEA", "DSB");
			var errorMsg = "Term settings for Job Type: ALL, Invoice type: ALL is mandatory.";

			terms2.PY_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertHasRowError(terms2, errorMsg);
			terms2.PY_JobType = "ALL";
			AssertNoRowError(terms2, errorMsg);

			terms2.PY_GB_Branch = branch1.PK;
			AssertHasRowError(terms2, errorMsg);
			terms2.PY_GB_Branch = ZGuid.Empty;
			AssertNoRowError(terms2, errorMsg);

			terms2.PY_GE_Department = dept1.PK;
			AssertHasRowError(terms2, errorMsg);
			terms2.PY_GE_Department = ZGuid.Empty;
			AssertNoRowError(terms2, errorMsg);

			terms2.PY_InvoiceClass = "DSB";
			AssertHasRowError(terms2, errorMsg);
			terms2.PY_InvoiceClass = "ALL";
			AssertNoRowError(terms2, errorMsg);
		}

		void SetupTermsInfo(OrgARTerms term, ZString jobType, ZGuid branchPK, ZGuid deptPK, ZString direction, ZString transportMode, ZString invoiceType)
		{
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = jobType;
				term.PY_GB_Branch = branchPK;
				term.PY_GE_Department = deptPK;
				term.PY_Direction = direction;
				term.PY_TransportMode = transportMode;
				term.PY_InvoiceClass = invoiceType;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsDebtor = true;
			companyData = organisation.CompanyData;
			arTerms = companyData.ARTerms[0];
		}

		OrgHeader organisation;
		OrgCompanyData companyData;
		OrgARTerms arTerms;
	}
}
