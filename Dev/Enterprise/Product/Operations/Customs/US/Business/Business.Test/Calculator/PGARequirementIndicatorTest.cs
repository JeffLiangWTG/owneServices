using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.US.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	[CargoWise.Data.Testing.UseSnapshotProtection]
	sealed class PGARequirementIndicatorTest : TestCaseWithFactory
	{
		public void TestTariffSupplementTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;
			tariff.UE_PGACodes = "FD2EP4AL2AQX";

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "@#$34";
			classification.CC_TariffNum = "1010101010";

			pivot.CI_CC = classification.PK;

			ZString tariffNumber = "1010101010";
			ZString supTariffNumber = "1010101010";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			pivot.CI_OP = product.PK;

			pivot.CI_TariffNum = tariffNumber;
			pivot.CI_SupplementalTariff = supTariffNumber;

			var pivottariff = pivot.PGARequirementIndicator;

			AssertEquals(false, pivottariff.RequireFDA);
			AssertEquals(false, pivottariff.RequireDOT);
			AssertEquals(false, pivottariff.RequireACE_LaceyData);

			AssertEquals(false, pivottariff.RequireFSIS);
			AssertEquals(false, pivottariff.RequireODS);
			AssertEquals(true, pivottariff.RequireVNE);
			AssertEquals(false, pivottariff.RequireDOT);
			AssertEquals(false, pivottariff.RequirePST);
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSCOA);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.HasTTBRequirement);
			AssertEquals(true, pivottariff.MayRequireAPHISNoDisclaimRequired);

			tariff.UE_PGACodes = "   EP2EP4FS4EP6FD2FD4AQ2";
			AssertEquals(true, pivottariff.RequireFSIS);
			AssertEquals(true, pivottariff.RequireODS);
			AssertEquals(true, pivottariff.RequireVNE);
			AssertEquals(false, pivottariff.RequireDOT);
			AssertEquals(true, pivottariff.RequirePST);
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);
			AssertEquals(false, pivottariff.HasTTBRequirement);
			AssertEquals(false, pivottariff.RequireACE_LaceyData);
			AssertEquals(true, pivottariff.HasAPHISRequirement);
			AssertEquals(false, pivottariff.MayRequireAPHISNoDisclaimRequired);

			tariff.UE_PGACodes = "   NM1";
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);

			tariff.UE_PGACodes = "   NM2";
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(true, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);

			tariff.UE_PGACodes = "   NM3";
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);

			tariff.UE_PGACodes = "   NM4";
			AssertEquals(true, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);

			tariff.UE_PGACodes = "   NM5";
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);

			tariff.UE_PGACodes = "   NM6";
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(true, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(false, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);

			tariff.UE_PGACodes = "   NM8";
			AssertEquals(false, pivottariff.RequireNMFSAMR);
			AssertEquals(false, pivottariff.RequireNMFSHMS);
			AssertEquals(false, pivottariff.RequireNMFS370);
			AssertEquals(true, pivottariff.RequireNMFSSIM);
			AssertEquals(false, pivottariff.RequireNMFSCOA);

			tariff.UE_PGACodes = "   EP2EP4FS4EP6FD2FD1";
			AssertEquals(false, pivottariff.RequireTTB);

			tariff.UE_PGACodes = "   TB2";
			AssertEquals(true, pivottariff.RequireTTB);
			tariff.UE_PGACodes = "   AL1";
			AssertEquals(false, pivottariff.RequireACE_LaceyData);

			AssertEquals(false, pivottariff.RequireHFC);
			tariff.UE_PGACodes = "   EH2";
			AssertEquals(true, pivottariff.RequireHFC);
		}

		public void TestRequireNMFSCOA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, RefCusConditionTypes.ConditionClass.Control, TariffConditionTypes.Codes.PGA);
			var conditionValueType = helper.CreateOrGetExistingRefCusConditionValueType(Core.Constants.CountryCodes.UnitedStates, TariffConditionValueTypes.Codes.PGA);
			Factory.Save();
			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "1010101010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, zzTariff.PK, "test", true, false, ZDateTime.BrettsBirthday, ZDateTime.Today);
			helper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition.PK, GovernmentAgencyProgramCodeList.Codes.COA);
			Factory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today;

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "@#$34";
			classification.CC_TariffNum = "1010101010";

			pivot.CI_CC = classification.PK;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART434";
			pivot.CI_OP = product.PK;

			pivot.CI_TariffNum = "1010101010";
			pivot.CI_SupplementalTariff = "1010101010";

			var pivottariff = pivot.PGARequirementIndicator;
			AssertEquals(true, pivottariff.RequireNMFSCOA);
		}

		public void TestPGARequirementOnInvoiceLine()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "AL2OM2DT2FD2EP2EP4EP6EP8TB2AM2AM7AQ2CP2FW2NM2NM8FS4";

			invoiceLine.JI_Tariff = "1010101010";
			var requirementIndicator = invoiceLine.PGARequirementIndicator;
			AssertEquals(false, requirementIndicator.RequireACE_LaceyData);
			AssertEquals(true, requirementIndicator.RequireOMC);
			AssertEquals(false, requirementIndicator.RequireACEFDA);
			AssertEquals(false, requirementIndicator.RequireNHTSA);
			AssertEquals(false, requirementIndicator.RequireODS);
			AssertEquals(false, requirementIndicator.RequireTSCA);
			AssertEquals(false, requirementIndicator.RequirePST);
			AssertEquals(false, requirementIndicator.RequireVNE);
			AssertEquals(true, requirementIndicator.RequireTTB);
			AssertEquals(false, requirementIndicator.RequireAMS);
			AssertEquals(false, requirementIndicator.RequireNOP);
			AssertEquals(false, requirementIndicator.RequireAPHIS);
			AssertEquals(false, requirementIndicator.RequireCPSC);
			AssertEquals(false, requirementIndicator.RequireFWS);
			AssertEquals(true, requirementIndicator.RequireNMFS370);
			AssertEquals(false, requirementIndicator.RequireFSIS);
			AssertEquals(true, requirementIndicator.RequireNMFSSIM);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals(false, requirementIndicator.RequireACE_LaceyData);
			AssertEquals(false, requirementIndicator.RequireOMC);
			AssertEquals(false, requirementIndicator.RequireACEFDA);
			AssertEquals(false, requirementIndicator.RequireNHTSA);
			AssertEquals(false, requirementIndicator.RequireODS);
			AssertEquals(false, requirementIndicator.RequireTSCA);
			AssertEquals(false, requirementIndicator.RequirePST);
			AssertEquals(false, requirementIndicator.RequireVNE);
			AssertEquals(false, requirementIndicator.RequireTTB);
			AssertEquals(false, requirementIndicator.RequireAMS);
			AssertEquals(false, requirementIndicator.RequireNOP);
			AssertEquals(false, requirementIndicator.RequireAPHIS);
			AssertEquals(false, requirementIndicator.RequireCPSC);
			AssertEquals(false, requirementIndicator.RequireFWS);
			AssertEquals(false, requirementIndicator.RequireNMFS370);
			AssertEquals(false, requirementIndicator.RequireFSIS);
			AssertEquals(false, requirementIndicator.RequireNMFSSIM);

			tariff.UE_PGACodes = "AL1OM2DT2FD2EP2EP4EP6EP8TB2AM2AQ2CP2FW2NM2FS4";
			AssertEquals(false, requirementIndicator.RequireACE_LaceyData);
		}

		public void TestMergeDoesNotChangeIndicator()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1020304010";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "   AQ2";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1020304010";
			invoiceLine.JI_LinePrice = 1m;
			AssertEquals("invoiceLine.DoesRequireAPHIS", true, invoiceLine.PGARequirementIndicator.RequireAPHIS);
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("invoiceLine.US_APHISInd should not be changed", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_APHISInd);
		}

		protected override void SetUp()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
