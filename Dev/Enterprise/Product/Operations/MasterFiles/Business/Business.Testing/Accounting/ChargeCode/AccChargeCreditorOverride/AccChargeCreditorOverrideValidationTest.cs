using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeCreditorOverrideValidationTest : BusinessObjectValidationTestCase
	{
		#region ACC_OH_Creditor

		public void TestValidateACC_OH_Creditor_GlobalCreditorOverride_CreditorIsPayable()
		{
			var (company2, _) = CreateCompanyAndBranch("GC2", "Company2", "BR2", "Branch2");
			var globalChargeCode = CreateChargeCode("AAA", Guid.Empty, Core.Constants.ChargeType.Margin, "Test Description");
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			Factory.Save();

			foreach (var localChargeCode in globalChargeCode.ChildChargeCodes)
			{
				var chargeCodeCompany = Factory.Load<GlbCompany>(localChargeCode.AC_GC);
				var companyData = creditor.GetCompanyDataForGlbCompany(localChargeCode.Company);
				companyData.OB_IsCreditor = true;
			}
			Factory.Save();

			var globalCreditorOverride = CreateAccChargeCreditorOverride(globalChargeCode, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, AccChargeCreditorOverrideLookups.TransportModeAdditionalCodes.All, creditor: creditor.PK);
			AssertNoExceptionThrown
			(
				"GIVEN creditor in all companies are payable WHEN saving GlobalChargeCode > CreditorOverride THEN should not give error",
				Factory.Save
			);
		}

		public void TestValidateACC_OH_Creditor_GlobalCreditorOverride_CreditorIsNotPayable()
		{
			var (company2, _) = CreateCompanyAndBranch("GC2", "Company2", "BR2", "Branch2");
			var globalChargeCode = CreateChargeCode("AAA", Guid.Empty, Core.Constants.ChargeType.Margin, "Test Description");
			var company1Creditor = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			CombineAssertions("Precondition: Creditor is not payable", () =>
			{
				AssertEquals("Company1", false, company1Creditor.OH_IsCreditor);
				var company2CompanyData = company1Creditor.GetCompanyDataForGlbCompany(company2);
				AssertEquals("Company2", false, company2CompanyData.OB_IsCreditor);
			});

			var globalCreditorOverride = CreateAccChargeCreditorOverride(globalChargeCode, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, AccChargeCreditorOverrideLookups.TransportModeAdditionalCodes.All, creditor: company1Creditor.PK);
			globalCreditorOverride.Validation.ValidateAll();
			CombineAssertions("GIVEN Creditor is not payable WHEN saving GlobalChargeCode > CreditorOverride THEN it should not error BUT it should error on related LocalChargeCodes > CreditorOverrides", () =>
			{
				AssertNoErrors
				(
					"GlobalCreditorOverride",
					globalCreditorOverride.ACC_OH_CreditorInfo
				);

				// LocalCreditorOverrides
				AssertExceptionThrown
				(
					typeof(AccChargeCode.ValidationOnLocalChargeCodeException),
					@"It is not possible to save the Global Charge Code, because the changes made to the Charge Code for company 'Eagle Datamation International Pte Ltd' would cause these errors:

Error - ACC_OH_Creditor: Organization should be Payable.",
					() => Factory.Save()
				);
			});
		}

		(GlbCompany, GlbBranch) CreateCompanyAndBranch(string companyCode, string companyName, string branchCode, string branchName)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.CompanyName = companyName;
			company.GC_Code = companyCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = branchCode;
			branch.GB_BranchName = branchName;

			return (company, branch);
		}

		AccChargeCode CreateChargeCode(string code, Guid company, string chargeType, string description)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_GC = company;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_Desc = description;

			return chargeCode;
		}

		public void TestValidateACC_OH_Creditor_GlobalCreditorOverride_ShouldValidateLocalCreditorOverrideAccordingToChargeCodeCompany()
		{
			var company1 = Env.CurrentCompanyPK;
			var (_, branch2) = CreateCompanyAndBranch("GC2", "Company2", "BR2", "Branch2");
			var company1GlobalChargeCode = CreateChargeCode("AAA", Guid.Empty, Core.Constants.ChargeType.Margin, "Charge AAA Description");
			var company1Creditor = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var company1LocalChargeCode = company1GlobalChargeCode.ChildChargeCodes.Single(chargeCode => chargeCode.AC_GC == company1);
			var company1LocalCreditorOverride = CreateAccChargeCreditorOverride(company1LocalChargeCode, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, AccChargeCreditorOverrideLookups.TransportModeAdditionalCodes.All, creditor: company1Creditor.PK);
			AssertExceptionThrown
			(
				typeof(AccChargeCode.ValidationOnLocalChargeCodeException),
				@"It is not possible to save the Global Charge Code, because the changes made to the Charge Code for company 'Eagle Datamation International' would cause these errors:

Error - ACC_OH_Creditor: Organization should be Payable.",
				() => Factory.Save()
			);

			company1Creditor.OH_IsCreditor = true;
			AssertNoExceptionThrown
			(
				"GIVEN Creditor is payable in Company1 WHEN saving LocalChargeCode THEN should not have error",
				Factory.Save
			);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch2.PK.ToGuid(), Guid.Empty))
			{
				var company2Factory = new BusinessObjectFactory { NameForDebugging = "Company2 Factory" };
				var company2Creditor = company2Factory.Load<OrgHeader>(company1Creditor.PK);
				var company2GlobalChargeCode = company2Factory.Load<AccChargeCode>(company1GlobalChargeCode.PK);
				AssertEquals("Precondition: Company2: Creditor is not payable", false, company2Creditor.OH_IsCreditor);

				company2GlobalChargeCode.AC_Desc = company2GlobalChargeCode.AC_Desc + " (updated)";
				AssertNoExceptionThrown
				(
					"GIVEN Creditor is not payable in Company2 WHEN saving GlobalChargeCode THEN should not have error",
					company2Factory.Save
				);
			}
		}

		#endregion

		public void TestValidateACC_PaymentTerm_UniqueConstraint()
		{
			CreditorOverride.Validation.ValidateAll();
			AssertNoErrors("Precondition: no error", CreditorOverride);

			var creditorOverride2 = CreateAccChargeCreditorOverride(
				creditorOverride.ChargeCode,
				creditorOverride.ACC_JobType,
				creditorOverride.ACC_Direction,
				creditorOverride.ACC_TransportMode,
				creditorOverride.ACC_PaymentTerm,
				creditorOverride.ACC_GE_Department,
				creditorOverride.ACC_DefaultingRule);
			CreditorOverride.Validation.ValidateACC_PaymentTerm();
			AssertHasError("Duplicate AccChargeCreditorOverride causes error.", CreditorOverride.ACC_PaymentTermInfo, "At least one more record already sets a behavior for the same Job parameters.");

			creditorOverride2.ACC_PaymentTerm = Core.Constants.PaymentType.Prepaid;
			CreditorOverride.Validation.ValidateACC_PaymentTerm();
			AssertNoErrors("AccChargeCreditorOverrides with different PaymentTerm should not have error", CreditorOverride.ACC_PaymentTermInfo);
		}

		public void TestValidateACC_PaymentTerm()
		{
			CreditorOverride.ACC_PaymentTerm = "???";
			CreditorOverride.Validation.ValidateACC_PaymentTerm();
			AssertHasError("Invalid PaymentTerm", CreditorOverride.ACC_PaymentTermInfo, "Enter a valid Payment Term.");

			CreditorOverride.ACC_PaymentTerm = ZString.Empty;
			CreditorOverride.Validation.ValidateACC_PaymentTerm();
			AssertNoErrors("Blank PaymentTerm", CreditorOverride.ACC_PaymentTermInfo);

			var creditorOverrideLookups = new AccChargeCreditorOverrideLookups(CreditorOverride);
			var validPaymentTerms = creditorOverrideLookups.PaymentTermList.GetAllCodes();
			foreach (var validPaymentTerm in validPaymentTerms)
			{
				CreditorOverride.ACC_PaymentTerm = validPaymentTerm;
				CreditorOverride.Validation.ValidateACC_PaymentTerm();
				AssertNoErrors($"Valid PaymentTerm '{validPaymentTerm}'", CreditorOverride.ACC_PaymentTermInfo);
			}
		}

		public void TestValidateACC_CreditorRole()
		{
			CreditorOverride.ACC_OH_Creditor = ZGuid.Empty;
			CreditorOverride.ACC_CreditorRole = "???";
			CreditorOverride.Validation.ValidateACC_CreditorRole();
			AssertHasError("Invalid CreditRole", CreditorOverride.ACC_CreditorRoleInfo, "Enter a valid Creditor Role.");

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			CreditorOverride.ACC_OH_Creditor = creditor.PK;
			CreditorOverride.ACC_CreditorRole = ZString.Empty;
			CreditorOverride.Validation.ValidateACC_CreditorRole();
			AssertNoErrors("Blank CreditorRole", CreditorOverride.ACC_CreditorRoleInfo);

			var creditorOverrideLookups = new AccChargeCreditorOverrideLookups(CreditorOverride);
			var validCreditorRoles = creditorOverrideLookups.CreditorRoleList.GetAllCodes();
			foreach (var validCreditorRole in validCreditorRoles)
			{
				CreditorOverride.ACC_OH_Creditor = ZGuid.Empty;
				CreditorOverride.ACC_CreditorRole = validCreditorRole;
				CreditorOverride.Validation.ValidateACC_CreditorRole();
				AssertNoErrors($"Valid CreditorRole '{validCreditorRole}'", CreditorOverride.ACC_CreditorRoleInfo);
			}
		}

		public void TestValidate_CreditorAndCreditorRoleCannotBeSetTogether()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			Factory.Save();

			var creditorOverrideLookups = new AccChargeCreditorOverrideLookups(CreditorOverride);
			var validCreditorRole = creditorOverrideLookups.CreditorRoleList.GetAllCodes()[0];

			CreditorOverride.ACC_OH_Creditor = ZGuid.Empty;
			CreditorOverride.ACC_CreditorRole = ZString.Empty;
			CreditorOverride.Validation.ValidateACC_OH_Creditor();
			CreditorOverride.Validation.ValidateACC_CreditorRole();
			CombineAssertions("Creditor and CreditorRole are empty", () =>
			{
				AssertHasError("Creditor", CreditorOverride.ACC_OH_CreditorInfo, "Please configure at least one of the Specific Creditor or Creditor Role.");
				AssertHasError("CreditorRole", CreditorOverride.ACC_CreditorRoleInfo, "Please configure at least one of the Specific Creditor or Creditor Role.");
			});

			CreditorOverride.ACC_OH_Creditor = creditor.PK;
			CreditorOverride.Validation.ValidateACC_OH_Creditor();
			CreditorOverride.Validation.ValidateACC_CreditorRole();
			CombineAssertions("Creditor is set and CreditorRole is empty", () =>
			{
				AssertNoErrors("Creditor", CreditorOverride.ACC_OH_CreditorInfo);
				AssertNoErrors("CreditorRole", CreditorOverride.ACC_CreditorRoleInfo);
			});

			CreditorOverride.ACC_OH_Creditor = ZGuid.Empty;
			CreditorOverride.ACC_CreditorRole = validCreditorRole;
			CreditorOverride.Validation.ValidateACC_OH_Creditor();
			CreditorOverride.Validation.ValidateACC_CreditorRole();
			CombineAssertions("Creditor is empty and CreditorRole is set", () =>
			{
				AssertNoErrors("Creditor", CreditorOverride.ACC_OH_CreditorInfo);
				AssertNoErrors("CreditorRole", CreditorOverride.ACC_CreditorRoleInfo);
			});

			CreditorOverride.ACC_OH_Creditor = creditor.PK;
			CreditorOverride.ACC_CreditorRole = validCreditorRole;
			CreditorOverride.Validation.ValidateACC_OH_Creditor();
			CreditorOverride.Validation.ValidateACC_CreditorRole();
			CombineAssertions("Creditor and CreditorRole are set", () =>
			{
				AssertHasError("Creditor", CreditorOverride.ACC_OH_CreditorInfo, "Specific Creditor and Creditor Role cannot be configured at the same time.");
				AssertHasError("CreditorRole", CreditorOverride.ACC_CreditorRoleInfo, "Specific Creditor and Creditor Role cannot be configured at the same time.");
			});
		}

		#region Implementation

		AccChargeCreditorOverride CreateAccChargeCreditorOverride
		(
			string jobType = "ALL",
			string direction = "",
			string transportMode = "",
			string paymentTerm = "",
			ZGuid department = default(ZGuid),
			string defaultingRule = "SCA",
			ZGuid creditor = default(ZGuid),
			string creditorRole = ""
		) => CreateAccChargeCreditorOverride(null, jobType, direction, transportMode, paymentTerm, department, defaultingRule, creditor, creditorRole);

		AccChargeCreditorOverride CreateAccChargeCreditorOverride
		(
			AccChargeCode chargeCode,
			string jobType = "ALL",
			string direction = "",
			string transportMode = "",
			string paymentTerm = "ALL",
			ZGuid department = default(ZGuid),
			string defaultingRule = "SCA",
			ZGuid creditor = default(ZGuid),
			string creditorRole = ""
		)
		{
			var accChargeCreditorOverride = chargeCode == null
				? Factory.NewWithValidTestData<AccChargeCreditorOverride>()
				: chargeCode.CreditorOverrides.AddNew();
			accChargeCreditorOverride.ACC_JobType = jobType;
			accChargeCreditorOverride.ACC_DefaultingRule = defaultingRule;
			accChargeCreditorOverride.ACC_Direction = direction;
			accChargeCreditorOverride.ACC_GE_Department = department;
			accChargeCreditorOverride.ACC_PaymentTerm = paymentTerm;
			accChargeCreditorOverride.ACC_TransportMode = transportMode;
			accChargeCreditorOverride.ACC_OH_Creditor = creditor;
			accChargeCreditorOverride.ACC_CreditorRole = creditorRole;
			return accChargeCreditorOverride;
		}

		AccChargeCreditorOverride CreditorOverride =>
			creditorOverride ?? (creditorOverride = CreateAccChargeCreditorOverride
			(
				JobInvoicingConsumerTypes.ShipmentCode,
				Core.Constants.FreightShipmentDirection.Code.Import,
				AccChargeCreditorOverrideLookups.TransportModeAdditionalCodes.All,
				Core.Constants.PaymentType.Collect,
				ZGuid.Empty,
				JobInvoicingConsumerType.ChargeCreditorDefaultingRulesBase.SpecificCreditorAlways.Code,
				creditor: ZGuid.Empty,
				creditorRole: DocAddressTypes.Codes.OverseasAgent
			));
		AccChargeCreditorOverride creditorOverride;

		protected override void SetUp()
		{
			base.SetUp();
			creditorOverride = null;
		}

		#endregion
	}
}
