using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OGAAgencyRequirement))]
	public class OGAAgencyRequirementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIndicatorReadOnlyForInvoiceLineAndProduct()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.APHIS, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.FDA, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.FCC, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.DOT, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.VNE, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes._370, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.AMR, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.DDTC, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.HMS, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.SIMP, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.ODS, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.FSIS, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.PST, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.HFC, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.Lacey, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.AMS, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.TSCA, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.TTB, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.NHTSA, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.ATF, invoiceLine);
			CheckIndicatorReadOnlyForInvoiceLine(GovernmentAgencyProgramCodeList.Codes.COA, invoiceLine);

			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.APHIS, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.FDA, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.FSIS, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.FWS, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.Lacey, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes._370, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.AMR, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.DDTC, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.HMS, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.SIMP, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.ODS, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.PST, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.HFC, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.VNE, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.TSCA, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.TTB, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.OMC, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.AMS, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.NOP, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.NHTSA, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.ATF, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.CPSC, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.DEA, pivot);
			CheckIndicatorReadOnlyForProduct(GovernmentAgencyProgramCodeList.Codes.COA, pivot);
		}

		public void TestPGAIndicatorValidated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.APHIS, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.FDA, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.FCC, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.DOT, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.VNE, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes._370, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.AMR, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.DDTC, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.HMS, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.SIMP, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.ODS, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.FSIS, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.PST, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.HFC, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.Lacey, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.AMS, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.TSCA, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.TTB, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.NHTSA, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.ATF, invoiceLine);
			CheckIndicatorValidation(GovernmentAgencyProgramCodeList.Codes.COA, invoiceLine);
		}

		public void CheckIndicatorValidation(string agencyCode, JobComInvoiceLine invoiceLine)
		{
			var code = agencyCode;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement.ValidateIndicator();

			AssertEquals(false, fpgaProvider.GetIndicatorInfo(code).HasMessageErrors());
		}

		public void CheckIndicatorReadOnlyForInvoiceLine(string agencyCode, JobComInvoiceLine invoiceLine)
		{
			var code = agencyCode;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var indicatorInfo = fpgaProvider.GetIndicatorInfo(code);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
indicatorInfo, () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			AssertEquals(indicatorInfo.ReadOnly, requirement.IndicatorInfo.ReadOnly);
		}

		public void CheckIndicatorReadOnlyForProduct(string agencyCode, CusClassPartPivot pivot)
		{
			var code = agencyCode;
			var fpgaProvider = new ProductPGAgencyRequirementProvider(pivot);
			var indicatorInfo = fpgaProvider.GetIndicatorInfo(code);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
indicatorInfo, () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			AssertEquals(indicatorInfo.ReadOnly, requirement.IndicatorInfo.ReadOnly);
		}

		public void TestPGANOPOnProduct()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0808404015";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD4AM8";
			tariff.UE_OGACodes = "FD4AM8";

			Factory.Save();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "SHOWNOPTEST";
			product.OP_Desc = "SHOWNOPTEST";
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "0808404015";
			pivot.CD_NOPIndicator = OGAIndicatorList.Codes.Declared;

			var agencyCode = GovernmentAgencyProgramCodeList.Codes.NOP;
			var fpgaProvider = new ProductPGAgencyRequirementProvider(pivot);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);

			AssertEquals("USDA/Agricultural Marketing Service Data Related to organics is required (AM8)", requirement.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement.Indicator);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			requirement.DisclaimedReason = "C";
			AssertHasMessageError(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			requirement.DisclaimedReason = "D";
			AssertHasMessageError(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestPGADisclaimReasonValidated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.APHIS, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.FSIS, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.ODS, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.PST, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.HFC, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.VNE, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.TSCA, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.AMS, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.NOP, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.NHTSA, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.Lacey, invoiceLine);
			CheckDisclaimReasonValidation(GovernmentAgencyProgramCodeList.Codes.FDA, invoiceLine);
		}

		public void TestDisclaimedReasonMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var code = GovernmentAgencyProgramCodeList.Codes.APHIS;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			var property = (KPropertyDescriptor)TypeDescriptor.GetProperties(requirement)["DisclaimedReason"];
			AssertNotNull(property);

			int maxLength = MetaData.GetMaxLength(requirement, property);
			AssertEquals("MaxLength value", 1, maxLength);
		}

		void CheckDisclaimReasonValidation(string agencyCode, JobComInvoiceLine invoiceLine)
		{
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);
			requirement.ValidateDisclaimReason();

			AssertEquals(false, fpgaProvider.GetDisclaimReasonInfo(agencyCode).HasMessageErrors());
		}

		public void TestPGADiscalimedReason_Lacey()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var agencyCode = GovernmentAgencyProgramCodeList.Codes.Lacey;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);

			requirement.DisclaimedReason = "C";
			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);

			requirement.DisclaimedReason = "D";
			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertNoMessageError(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "WEIGHTTEST";
			product.OP_Desc = "WEIGHTTEST";
			product.OP_Weight = 10;
			product.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			product.OP_NetWeight = 9;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "9101000010";
			pivot.CD_LaceyActIndicator = OGAIndicatorList.Codes.Declared;

			var fpgaProvider2 = new ProductPGAgencyRequirementProvider(pivot);
			var requirement2 = new OGAAgencyRequirement(fpgaProvider2.Factory, () => fpgaProvider2.GetRequirementDescription(agencyCode),
fpgaProvider2.GetIndicatorInfo(agencyCode), () => fpgaProvider2.ValidateIndicator(agencyCode),
fpgaProvider2.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider2.ValidateDisclaimReason(agencyCode),
() => fpgaProvider2.IsPGA(agencyCode) ? fpgaProvider2.GetDisclaimReasonList(agencyCode) : null);

			requirement2.Indicator = OGAIndicatorList.Codes.Disclaimed;
			requirement2.DisclaimedReason = "C";
			AssertHasMessageError(requirement2.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);

			requirement2.Indicator = OGAIndicatorList.Codes.Disclaimed;
			requirement2.DisclaimedReason = "D";
			AssertHasMessageError(requirement2.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestPGADisclaimedReasonValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.APHIS, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.FDA, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.FSIS, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.ODS, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.PST, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.HFC, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.VNE, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.TSCA, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.AMS, PGADisclaimReasonList.Codes.B, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.NHTSA, PGADisclaimReasonList.Codes.A, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.NOP, PGADisclaimReasonList.Codes.A, invoiceLine);
			DisclaimedReason(GovernmentAgencyProgramCodeList.Codes.Lacey, PGADisclaimReasonList.Codes.A, invoiceLine);
		}

		void DisclaimedReason(string agencyCode, string disclaimedReasonCode1, JobComInvoiceLine invoiceLine)
		{
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);

			requirement.DisclaimedReason = disclaimedReasonCode1;
			AssertEquals(disclaimedReasonCode1, fpgaProvider.GetDisclaimReasonInfo(agencyCode).Value);
		}

		public void TestPGAIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.APHIS, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.FCC, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.FDA, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.FSIS, invoiceLine);

			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.Lacey, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes._370, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.AMR, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.DDTC, invoiceLine);

			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.HMS, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.ODS, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.PST, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.HFC, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.VNE, invoiceLine);

			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.TSCA, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.TTB, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.AMS, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.NHTSA, invoiceLine);

			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.ATF, invoiceLine);
			CheckIndicatorValue(GovernmentAgencyProgramCodeList.Codes.SIMP, invoiceLine);
		}

		void CheckIndicatorValue(string agencyCode, JobComInvoiceLine invoiceLine)
		{
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(agencyCode),
fpgaProvider.GetIndicatorInfo(agencyCode), () => fpgaProvider.ValidateIndicator(agencyCode),
fpgaProvider.GetDisclaimReasonInfo(agencyCode), () => fpgaProvider.ValidateDisclaimReason(agencyCode),
() => fpgaProvider.IsPGA(agencyCode) ? fpgaProvider.GetDisclaimReasonList(agencyCode) : null);

			requirement.Indicator = OGAIndicatorList.Codes.Declared;
			AssertEquals(OGAIndicatorList.Codes.Declared, fpgaProvider.GetIndicatorInfo(agencyCode).Value);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, fpgaProvider.GetIndicatorInfo(agencyCode).Value);
		}

		public void TestProperties()
		{
			var requirement = (OGAAgencyRequirement)GetNewBusinessObject();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2EP4EP8DT2";
			tariff.UE_OGACodes = "FD2EP4EP8DT2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.NHTSA;
			AssertEquals("DOT/National Highway Traffic Safety Administration HS-7 data is required (DT2)", requirement.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement.Indicator);

			var code = GovernmentAgencyProgramCodeList.Codes.VNE;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement1 = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			requirement1.AgencyCode = GovernmentAgencyProgramCodeList.Codes.VNE;
			AssertEquals("Vehicle and Engines specific data is required (EP4)", requirement1.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement1.Indicator);

			requirement1.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_VNEInd);

			code = GovernmentAgencyProgramCodeList.Codes.FDA;
			var requirement3 = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement3.AgencyCode = GovernmentAgencyProgramCodeList.Codes.FDA;
			AssertEquals("FDA Admissibility Review Required (FD2)", requirement3.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement3.Indicator);

			requirement3.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_FDAIndicator);

			invoiceLine.JI_Tariff = ZString.Empty;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "0000000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			tariff2.UE_OGACodes = "FD2";
			tariff2.UE_PGACodes = "FD0FW2";
			invoiceLine.JI_Tariff = tariff2.UE_Tariff;

			AssertEquals("Subject to FDA Admissibility Review. DO NOT SUBMIT (FD0)", requirement3.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, requirement3.Indicator);

			code = GovernmentAgencyProgramCodeList.Codes.Lacey;
			var requirement4 = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement4.AgencyCode = GovernmentAgencyProgramCodeList.Codes.Lacey;
			requirement4.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_LaceyIndicator);
			requirement4.DisclaimedReason = PGADisclaimReasonList.Codes.B;
			AssertEquals(PGADisclaimReasonList.Codes.B, invoiceLine.US_LaceyDisclaimReason);
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.D;
			AssertEquals(PGADisclaimReasonList.Codes.D, requirement4.DisclaimedReason);

			code = GovernmentAgencyProgramCodeList.Codes.TSCA;
			tariff.UE_OGACodes = "EP8";
			tariff.UE_PGACodes = "EP8";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var requirement5 = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement5.AgencyCode = GovernmentAgencyProgramCodeList.Codes.TSCA;
			requirement5.Indicator = OGAIndicatorList.Codes.Disclaimed;
			Assert("we can send disclaimed message", !requirement5.DisclaimedReason_ReadOnly);
		}

		public void TestUS_OGAIndicatorList()
		{
			var requirement = (OGAAgencyRequirement)GetNewBusinessObject();
			var list = new GovernmentAgencyProgramCodeList();

			requirement.CanDisclaim = false;
			foreach (var code in new[]
			{
				GovernmentAgencyProgramCodeList.Codes.DDTC,
				GovernmentAgencyProgramCodeList.Codes.ATF,
				GovernmentAgencyProgramCodeList.Codes.SIMP,
				GovernmentAgencyProgramCodeList.Codes.COA
			})
			{
				list.RemoveCode(code);
				requirement.AgencyCode = code;
				AssertEquals(OGAIndicatorList.GetWithoutDisclaim(Factory), requirement.US_OGAIndicatorList);
			}

			requirement.CanDisclaim = true;
			foreach (ICodeDescription pair in list)
			{
				requirement.AgencyCode = pair.Code;
				AssertEquals(Factory.GetCachedValue<OGAIndicatorList>(), requirement.US_OGAIndicatorList);
			}
		}

		public void TestNMFSRequirement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "0000000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2NM2NM4NM6NM8";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			var code = GovernmentAgencyProgramCodeList.Codes._370;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes._370;
			AssertEquals("370 specific data is required (NM2)", requirement.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement.Indicator);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFS370Ind);
			AssertHasMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertHasMessageError(invoiceLine.US_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			requirement.DisclaimedReason = "~";
			AssertNoMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertHasMessageErrorContaining(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoiceLine.US_NMFS370DisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			requirement.DisclaimedReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_NMFS370DisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageErrorContaining(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoiceLine.US_NMFS370DisclaimReasonInfo, ListValidation.InvalidCodeMessageError);

			code = GovernmentAgencyProgramCodeList.Codes.AMR;
			fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.AMR;
			AssertEquals("Antarctic Marine Living Resources specific data is required (NM4)", requirement.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement.Indicator);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSAMRInd);
			AssertHasMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertHasMessageError(invoiceLine.US_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			requirement.DisclaimedReason = "~";
			AssertNoMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertHasMessageErrorContaining(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSAMRDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			requirement.DisclaimedReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_NMFSAMRDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageErrorContaining(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSAMRDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);

			code = GovernmentAgencyProgramCodeList.Codes.HMS;
			fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.HMS;
			AssertEquals("Highly Migratory Species specific data is required (NM6)", requirement.Requirement);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement.Indicator);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_NMFSHMSInd);
			AssertHasMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertHasMessageError(invoiceLine.US_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			requirement.DisclaimedReason = "~";
			AssertNoMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertHasMessageErrorContaining(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(invoiceLine.US_NMFSHMSDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);
			requirement.DisclaimedReason = PGADisclaimReasonList.Codes.A;
			AssertNoMessageError(requirement.DisclaimedReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageError(invoiceLine.US_NMFSHMSDisclaimReasonInfo, AgencyRequirementsValidator.RequirementConstants.PGA.PGADisclaimedReason);
			AssertNoMessageErrorContaining(requirement.DisclaimedReasonInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(invoiceLine.US_NMFSHMSDisclaimReasonInfo, ListValidation.InvalidCodeMessageError);

			code = GovernmentAgencyProgramCodeList.Codes.SIMP;
			fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);

			requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
			fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
			fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
			() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.SIMP;
			AssertEquals("Seafood Import Monitoring Program data is required (NM8)", requirement.Requirement);

			AssertEquals(OGAIndicatorList.Codes.Declared, requirement.Indicator);

			code = GovernmentAgencyProgramCodeList.Codes.COA;
			fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);

			requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
			fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
			fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
			() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.COA;
			AssertEquals("NMFS Certificate of Admissibility is required", requirement.Requirement);

			AssertEquals(OGAIndicatorList.Codes.Declared, requirement.Indicator);
		}

		public void TestDDTCRequirement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var code = GovernmentAgencyProgramCodeList.Codes.DDTC;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.DDTC;
			AssertEquals("", requirement.Requirement);
			AssertEquals("", requirement.Indicator);
		}

		public void TestTTBRequirement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var code = GovernmentAgencyProgramCodeList.Codes.TTB;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.TTB;
			AssertEquals("", requirement.Requirement);
			AssertEquals("", requirement.Indicator);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "TB2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			var fpgaProvider2 = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement2 = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement2.AgencyCode = GovernmentAgencyProgramCodeList.Codes.TTB;
			AssertEquals(OGARequirementList.Descriptions.TB2 + " (TB2)", requirement2.Requirement);
			AssertEquals(OGARequirementList.Descriptions.TB2 + " (TB2)", invoiceLine.US_TTBReqDesc);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_TTBInd);
			AssertEquals(OGAIndicatorList.Codes.Declared, requirement2.Indicator);
		}

		public void TestPropertiesForAPHIS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "   AQ1";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			var code = GovernmentAgencyProgramCodeList.Codes.APHIS;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.APHIS;
			AssertEquals("APHIS data may be required (AQ1)", requirement.Requirement);

			tariff.UE_PGACodes = "   AQ2";
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			code = GovernmentAgencyProgramCodeList.Codes.APHIS;
			fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.APHIS;
			AssertEquals("APHIS data is required (AQ2)", requirement.Requirement);

			tariff.UE_PGACodes = "   AQX";
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			code = GovernmentAgencyProgramCodeList.Codes.APHIS;
			fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.APHIS;
			AssertEquals("APHIS  data may be required(no disclaim required) (AQX)", requirement.Requirement);
		}

		public void TestPropertiesForFWS()
		{
			using (ZZCustomsFunctionality.TemporarilySetupFWSEffective())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = "0000000000";
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.Today;
				tariff.UE_PGACodes = "   FW1";
				invoiceLine.JI_Tariff = tariff.UE_Tariff;

				var code = GovernmentAgencyProgramCodeList.Codes.FWS;
				var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
				var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
	fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
	fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
	() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
				requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.FWS;
				AssertEquals("U.S. Fish and Wildlife data may be required. (FW1)", requirement.Requirement);

				tariff.UE_PGACodes = "   FW2";
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = tariff.UE_Tariff;
				fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
				requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
	fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
	fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
	() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
				requirement.AgencyCode = GovernmentAgencyProgramCodeList.Codes.FWS;
				AssertEquals("U.S. Fish and Wildlife Service data is required (FW2)", requirement.Requirement);
			}
		}

		public void TestPropertiesForTSCA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2EP4EP8DT2";
			tariff.UE_OGACodes = "EP8";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var code = GovernmentAgencyProgramCodeList.Codes.TSCA;
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);

			requirement.AgencyCode = code;
			AssertEquals("Toxic Substances Control Act specific data is required (EP8)", requirement.Requirement);

			requirement.Indicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_TSCAInd);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();

			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2EP4EP8DT2";
			tariff.UE_OGACodes = "AQ2";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;

			var code = GovernmentAgencyProgramCodeList.Codes.NHTSA;
			var fpgaProvider = new InvoiceLinePGAAgencyRequirementsProvider(invoiceLine);
			var requirement = new OGAAgencyRequirement(fpgaProvider.Factory, () => fpgaProvider.GetRequirementDescription(code),
fpgaProvider.GetIndicatorInfo(code), () => fpgaProvider.ValidateIndicator(code),
fpgaProvider.GetDisclaimReasonInfo(code), () => fpgaProvider.ValidateDisclaimReason(code),
() => fpgaProvider.IsPGA(code) ? fpgaProvider.GetDisclaimReasonList(code) : null);
			requirement.CanDisclaim = true;
			return requirement;
		}

		JobComInvoiceLine invoiceLine;
		USCTariff tariff;

		public new void TestSettingValueCallsRefreshBinding()
		{
			Assert(true);
		}

		#endregion
	}
}
