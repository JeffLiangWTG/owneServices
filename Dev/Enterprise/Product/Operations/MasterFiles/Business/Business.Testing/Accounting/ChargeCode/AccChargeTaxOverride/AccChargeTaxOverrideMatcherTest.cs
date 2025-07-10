using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeTaxOverrideMatcherTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetTaxRateNullCheck()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE2";

			AccChargeTaxOverride @override = CreateTaxOverride(code, TaxRate8, "COS", "IMP", "CIF", "TRN", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			@override.AO_VATExemptOnExportCharges = true;
			Factory.Save();

			//this calls GetRate() such that its local variable "Organization" is null
			code.GetChargeTaxOverride(GetParameters("CIF", CostSell.Cost, JobInvoicingConsumerTypes.LocalCartage.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));

			//this calls GetRate() such that its local variable "RatesToExclude" is null
			code.GetChargeTaxOverride(GetParameters("CIF", CostSell.Cost, JobInvoicingConsumerTypes.LocalCartage.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
		}

		public void TestGetTaxOverrideUsesAO_A9_DefaultVATClassIfExists()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";
			AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate1, "COS", "IMP", "EXW", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			Factory.Save();

			var result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate1.PK, result.AO_AT);
			AssertEquals(ZGuid.Empty, override1.AO_A9_DefaultVATClass);
			AssertEquals("OverrideInvTaxMsg should come from tax rate if it's not defined", TaxRate1.AT_A9_DefaultVatClass, result.AO_A9_DefaultVATClass);

			AccInvMsg anotherMsg = Factory.NewWithValidTestData<AccInvMsg>();
			override1.AO_A9_DefaultVATClass = anotherMsg.PK;
			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("OverrideInvTaxMsg should come from another msg", anotherMsg.PK, result.AO_A9_DefaultVATClass);
		}

		public void TestGetTaxOverridePermutation()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";
			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_Code = "CCODE2";

			var override1 = CreateTaxOverride(code, TaxRate1, "COS", "IMP", "EXW", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override2 = CreateTaxOverride(code, TaxRate2, "REV", "IMP", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override3 = CreateTaxOverride(code, TaxRate3, "ALL", "IMP", "EXW", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);

			var override4 = CreateTaxOverride(code, TaxRate4, "COS", "EXP", "FOB", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override5 = CreateTaxOverride(code, TaxRate5, "COS", "ALL", "FOB", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override6 = CreateTaxOverride(code, TaxRate6, "COS", "ALL", "FOB", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override7 = CreateTaxOverride(code, TaxRate7, "COS", "IMP", "ALL", "BRK", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);

			var override8 = CreateTaxOverride(code2, TaxRate8, "COS", "IMP", "CIF", "TRN", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);

			var override9 = CreateTaxOverride(code, TaxRate9, "COS", "IMP", "ALL", "BRK", "ALL", "ALL", "ALL", "AIR", "ALL", false, false);
			var override10 = CreateTaxOverride(code, TaxRate10, "COS", "EXP", "ALL", "BRK", "ALL", "ALL", "ALL", "SEA", "ALL", false, false);
			var override11 = CreateTaxOverride(code, TaxRate11, "REV", "IMP", "ALL", "BRK", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override12 = CreateTaxOverride(code, TaxRate12, "REV", "DOM", "ALL", "BRK", "ALL", "ALL", "ALL", "FIX", "ALL", false, false);
			var override13 = CreateTaxOverride(code, TaxRate13, "REV", "DOM", "ALL", "BRK", "ALL", "ALL", "ALL", "IWT", "ALL", false, false);
			var override14 = CreateTaxOverride(code, TaxRate14, "REV", "DOM", "ALL", "BRK", "ALL", "ALL", "ALL", "OWN", "ALL", false, false);
			var override15 = CreateTaxOverride(code, TaxRate15, "REV", "DOM", "ALL", "BRK", "ALL", "ALL", "ALL", "MAI", "ALL", false, false);

			Factory.Save();

			var result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate1.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Match found for generic cost/sell", TaxRate2.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("FOB", CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Match found for generic INCOTERM", TaxRate2.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("No match", null, result);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Cost, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Match found", TaxRate7.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("FOB", CostSell.Cost, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Match found", TaxRate7.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("FOB", CostSell.Cost, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Export, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Match found", TaxRate4.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("CIF", CostSell.Cost, JobInvoicingConsumerTypes.LocalCartage.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertNull("No Match found", result);

			result = code2.GetChargeTaxOverride(GetParameters("CIF", CostSell.Cost, JobInvoicingConsumerTypes.LocalCartage.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Match found", TaxRate8.PK, result.AO_AT);

			result = code2.GetChargeTaxOverride(GetParameters("CIF", CostSell.Cost, null, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertNull("No Match found", result);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Cost, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Import, "AIR", null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate9.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Revenue, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Import, "SEA", null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate11.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("CIF", CostSell.Cost, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Export, "SEA", null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate10.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Revenue, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Domestic, "FIX", null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate12.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Revenue, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Domestic, "IWT", null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate13.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Revenue, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Domestic, "OWN", null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate14.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("EXW", CostSell.Revenue, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Domestic, "MAI", null, null, null, null, null, null, null));
			AssertEquals("Exact match found", TaxRate15.PK, result.AO_AT);

			result = code.GetChargeTaxOverride(GetParameters("CIF", CostSell.Revenue, JobInvoicingConsumerTypes.Brokerage.Code, Directions.Export, "AIR", null, null, null, null, null, null, null));
			AssertNull("No Match found", result);
		}

		public void TestGetChargeTaxOverrideReturnsOnlyRulesWithCreateTaxRecordTrue()
		{
			AccChargeCode code1 = Factory.NewWithValidTestData<AccChargeCode>();
			code1.AC_Code = "CCODE1";
			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_Code = "CCODE2";

			AccChargeTaxOverride override1 = CreateTaxOverride(code1, TaxRate1, ALL, ALL, ALL, ALL, ALL, ALL, ALL, ALL, ALL, false, false, createTaxRecord: true);
			AccChargeTaxOverride override2 = CreateTaxOverride(code1, TaxRate2, "REV", ALL, ALL, ALL, ALL, ALL, ALL, ALL, ALL, false, false, createTaxRecord: false);
			AccChargeTaxOverride override3 = CreateTaxOverride(code2, TaxRate3, "REV", "EXP", ALL, ALL, ALL, ALL, ALL, ALL, ALL, false, false, createTaxRecord: true);

			Factory.Save();

			var result = code1.GetChargeTaxOverride(GetParameters(ALL, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertNull("Matching record has false for create Tax Record", result);

			result = code1.GetChargeTaxOverride(GetParameters(ALL, CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Has matching ALL type record with create Tax Record true", TaxRate1.PK, result.AO_AT);

			result = code2.GetChargeTaxOverride(GetParameters(ALL, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, null, null, null, null, null, null, null));
			AssertEquals("Has matching REV type record with create Tax Record true", TaxRate3.PK, result.AO_AT);
		}

		public void TestGetTaxOverridePermutationDependOnLocationCriteria()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";

			RefUNLOCO aUXXX = CreateUNLOCOForTest("AU000", Constants.CountryCodes.Australia);
			RefUNLOCO nZXXX = CreateUNLOCOForTest("NZ000", Constants.CountryCodes.NewZealand);

			RefUNLOCO gBXXX = CreateUNLOCOForTest("GB000", Constants.CountryCodes.UnitedKingdom);
			RefUNLOCO gBYYY = CreateUNLOCOForTest("GB999", Constants.CountryCodes.UnitedKingdom);
			RefUNLOCO fRXXX = CreateUNLOCOForTest("FR000", Constants.CountryCodes.France);
			RefUNLOCO dEXXX = CreateUNLOCOForTest("DE000", Constants.CountryCodes.Germany);

			RefUNLOCO cAXXX = CreateUNLOCOForTest("CA000", Constants.CountryCodes.Canada);
			RefZoneHeader newTaxZone = Factory.New<RefZoneHeader>();
			newTaxZone.FZ_Code = "TAXZ";
			newTaxZone.FZ_Description = "Tax Zone";
			newTaxZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;
			RefZonePivot portZonePivot = Factory.New<RefZonePivot>();
			portZonePivot.F2_FZ = newTaxZone.PK;
			portZonePivot.F2_ParentID = cAXXX.PK;
			portZonePivot.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			OrgHeader orgGB = CreateNewOrgForTest(gBXXX); // Located within the UK
			OrgHeader orgFR = CreateNewOrgForTest(fRXXX); // Located in Eurpean Union 
			OrgHeader orgAU = CreateNewOrgForTest(aUXXX); // Outside European Union

			AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate1, ALL, ALL, ALL, ALL, "GB", "GB", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override2 = CreateTaxOverride(code, TaxRate2, ALL, ALL, ALL, ALL, "GB", "EUN", "GB", "ALL", "ALL", false, false);
			AccChargeTaxOverride override3 = CreateTaxOverride(code, TaxRate3, ALL, ALL, ALL, ALL, "GB", "EUN", "EUN", "ALL", "ALL", false, false);
			AccChargeTaxOverride override4 = CreateTaxOverride(code, TaxRate4, ALL, ALL, ALL, ALL, "GB", "EUN", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override5 = CreateTaxOverride(code, TaxRate5, ALL, ALL, ALL, ALL, "EUN", "EUN", "GB", "ALL", "ALL", false, false);
			AccChargeTaxOverride override6 = CreateTaxOverride(code, TaxRate6, ALL, ALL, ALL, ALL, "EUN", "EUN", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override7 = CreateTaxOverride(code, TaxRate7, ALL, ALL, ALL, ALL, "GB", "NZ", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override8 = CreateTaxOverride(code, TaxRate8, ALL, ALL, ALL, ALL, "ALL", "GB", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override9 = CreateTaxOverride(code, TaxRate9, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override10 = CreateTaxOverride(code, TaxRate10, ALL, ALL, ALL, ALL, "TAXZ", "GB", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override11 = CreateTaxOverride(code, TaxRate11, ALL, "IMP", ALL, ALL, "GB", "AU", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override12 = CreateTaxOverride(code, TaxRate12, ALL, "EXP", ALL, ALL, "AU", "NZ", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override13 = CreateTaxOverride(code, TaxRate13, ALL, "DOM", ALL, ALL, "AU", "AU", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override14 = CreateTaxOverride(code, TaxRate14, ALL, "OTH", ALL, ALL, "GB", "FR", "ALL", "ALL", "ALL", false, false);

			Factory.Save();

			AssertTaxOverride(code, TaxRate1, gBXXX, gBYYY, orgAU, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate2, gBXXX, dEXXX, orgGB, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate3, gBXXX, dEXXX, orgFR, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate4, gBXXX, dEXXX, orgAU, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate5, fRXXX, dEXXX, orgGB, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate6, fRXXX, dEXXX, orgAU, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate7, gBXXX, nZXXX, orgAU, Directions.Import, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate8, aUXXX, gBXXX, orgAU, Directions.Export, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate9, null, null, orgAU, Directions.Export, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate10, cAXXX, gBXXX, orgAU, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate11, gBXXX, aUXXX, orgAU, Directions.Import, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate12, aUXXX, nZXXX, orgAU, Directions.Export, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate13, aUXXX, aUXXX, orgAU, Directions.Domestic, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate14, gBXXX, fRXXX, orgAU, Directions.CrossTrade, AccChargeTaxOverride.ALL);
		}

		public void TestGetTaxOverrideForOrganisationCategory()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";

			OrgHeader orgBUS = Factory.NewWithValidTestData<OrgHeader>();
			orgBUS.OH_Category = "BUS";
			OrgHeader orgGOV = Factory.NewWithValidTestData<OrgHeader>();
			orgGOV.OH_Category = "GOV";
			OrgHeader orgNAT = Factory.NewWithValidTestData<OrgHeader>();
			orgNAT.OH_Category = "NAT";
			OrgHeader orgNGO = Factory.NewWithValidTestData<OrgHeader>();
			orgNGO.OH_Category = "NGO";

			AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate1, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override2 = CreateTaxOverride(code, TaxRate2, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "BUS", false, false);
			AccChargeTaxOverride override3 = CreateTaxOverride(code, TaxRate3, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "GOV", false, false);
			AccChargeTaxOverride override4 = CreateTaxOverride(code, TaxRate4, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "NAT", false, false);
			AccChargeTaxOverride override5 = CreateTaxOverride(code, TaxRate5, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "NGO", false, false);
			Factory.Save();

			AssertTaxOverride(code, TaxRate2, null, null, orgBUS, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate3, null, null, orgGOV, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate4, null, null, orgNAT, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate5, null, null, orgNGO, Directions.CrossTrade, AccChargeTaxOverride.ALL);
		}

		public void TestGetTaxOverrideForSplitVATPaymentOrg()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARVATSplitPaymentApplicable = false;
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARVATSplitPaymentApplicable = true;

			AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate1, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override2 = CreateTaxOverride(code, TaxRate2, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, true);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, AccChargeTaxOverride.ALL);
				AssertTaxOverride(code, TaxRate2, null, null, org2, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate2, null, null, org1, Directions.CrossTrade, AccChargeTaxOverride.ALL);
				AssertTaxOverride(code, TaxRate2, null, null, org2, Directions.CrossTrade, AccChargeTaxOverride.ALL);
			}
		}

		public void TestGetTaxOverrideForSplitVATPaymentOrg_OneALLTaxOverride_AO_SplitPaymentVATOrganisationIsFalse()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARVATSplitPaymentApplicable = false;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARVATSplitPaymentApplicable = true;

			org1.OH_Category = OrgConstants.Category.Business;
			org2.OH_Category = OrgConstants.Category.Business;

			var override1 = CreateTaxOverride(code, TaxRate1, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, false);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			org1.OH_Category = OrgConstants.Category.Government;
			org2.OH_Category = OrgConstants.Category.Government;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}
		}

		public void TestGetTaxOverrideForSplitVATPaymentOrg_TaxOverrideWithMatchingOrgCategory_AO_SplitPaymentVATOrganisationIsFalse()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARVATSplitPaymentApplicable = false;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARVATSplitPaymentApplicable = true;

			org1.OH_Category = OrgConstants.Category.Business;
			org2.OH_Category = OrgConstants.Category.Business;

			var override1 = CreateTaxOverride(code, TaxRate1, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override2 = CreateTaxOverride(code, TaxRate2, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "BUS", false, false);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate2, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate2, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate2, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate2, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			org1.OH_Category = OrgConstants.Category.Government;
			org2.OH_Category = OrgConstants.Category.Government;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}
		}

		public void TestGetTaxOverrideForSplitVATPaymentOrg_TaxOverrideWithMatchingOrgCategory_AO_SplitPaymentVATOrganisationIsTrue()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARVATSplitPaymentApplicable = false;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARVATSplitPaymentApplicable = true;

			org1.OH_Category = OrgConstants.Category.Business;
			org2.OH_Category = OrgConstants.Category.Business;

			var override1 = CreateTaxOverride(code, TaxRate1, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override2 = CreateTaxOverride(code, TaxRate2, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "BUS", false, true);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate2, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate2, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate2, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			org1.OH_Category = OrgConstants.Category.Government;
			org2.OH_Category = OrgConstants.Category.Government;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}
		}

		public void TestGetTaxOverrideForSplitVATPaymentOrg_TaxOverrideWithMatchingOrgCategory_AO_SplitPaymentVATOrganisationIsTrueAndFalse()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_ARVATSplitPaymentApplicable = false;
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_ARVATSplitPaymentApplicable = true;

			org1.OH_Category = OrgConstants.Category.Business;
			org2.OH_Category = OrgConstants.Category.Business;

			var override1 = CreateTaxOverride(code, TaxRate1, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			var override2 = CreateTaxOverride(code, TaxRate2, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "BUS", false, false);
			var override3 = CreateTaxOverride(code, TaxRate3, ALL, ALL, ALL, ALL, "ALL", "ALL", "ALL", "ALL", "BUS", false, true);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate2, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate3, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate3, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate3, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			org1.OH_Category = OrgConstants.Category.Government;
			org2.OH_Category = OrgConstants.Category.Government;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				AssertTaxOverride(code, TaxRate1, null, null, org1, Directions.CrossTrade, Constants.TransportModes.All);
				AssertTaxOverride(code, TaxRate1, null, null, org2, Directions.CrossTrade, Constants.TransportModes.All);
			}
		}

		public void TestGetTaxOverrideForOtherIntoEUN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
				code.AC_Code = "CCODE1";

				RefUNLOCO aUXXX = CreateUNLOCOForTest("AU000", Constants.CountryCodes.Australia);
				RefUNLOCO nZXXX = CreateUNLOCOForTest("NZ000", Constants.CountryCodes.NewZealand);

				RefUNLOCO gBXXX = CreateUNLOCOForTest("GB000", Constants.CountryCodes.UnitedKingdom);
				RefUNLOCO fRXXX = CreateUNLOCOForTest("FR000", Constants.CountryCodes.France);

				OrgHeader orgGB = CreateNewOrgForTest(gBXXX); // Located within the UK

				AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate1, ALL, "OTH", ALL, ALL, "AU", "EUN", "ALL", "ALL", "ALL", false, false);
				AccChargeTaxOverride override2 = CreateTaxOverride(code, TaxRate2, ALL, "OTH", ALL, ALL, "EUN", "AU", "ALL", "ALL", "ALL", false, false);

				Factory.Save();

				AssertTaxOverride(code, TaxRate1, aUXXX, gBXXX, orgGB, Directions.Import, AccChargeTaxOverride.ALL);
				AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, AccChargeTaxOverride.ALL);
			}
		}

		public void TestGetTaxOverrideForEUN()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Sweden;
			AssertEquals("Precondition: Current Company Country", Constants.CountryCodes.Sweden, "SE");

			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE1";
			code.AC_AT_GSTRate = TaxRate3.PK;

			RefUNLOCO uSXXX = CreateUNLOCOForTest("US000", Constants.CountryCodes.UnitedStates);
			uSXXX.Country.RN_EconomicGrouping = "NAF";
			AssertEquals("Precondition: US Country should have an economic grouping", "NAF", uSXXX.Country.RN_EconomicGrouping);

			RefUNLOCO aUXXX = CreateUNLOCOForTest("AU000", Constants.CountryCodes.Australia);
			aUXXX.Country.RN_EconomicGrouping = "";
			AssertEquals("Precondition: AU Country should NOT have an economic grouping", "", aUXXX.Country.RN_EconomicGrouping);

			RefUNLOCO sEXXX = CreateUNLOCOForTest("SE000", Constants.CountryCodes.Sweden);
			OrgHeader orgSE = CreateNewOrgForTest(sEXXX);

			AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate1, ALL, "IMP", ALL, ALL, "NEU", "SE", "ALL", "ALL", "ALL", false, false);
			Factory.Save();

			AssertTaxOverride(code, TaxRate1, aUXXX, sEXXX, orgSE, Directions.Import, AccChargeTaxOverride.ALL);
			AssertTaxOverride(code, TaxRate1, uSXXX, sEXXX, orgSE, Directions.Import, AccChargeTaxOverride.ALL);
		}

		public void TestGetTaxOverrideForCustomsStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
				code.AC_Code = "CCODE1";

				RefUNLOCO aUXXX = CreateUNLOCOForTest("AU000", Constants.CountryCodes.Australia);
				RefUNLOCO nZXXX = CreateUNLOCOForTest("NZ000", Constants.CountryCodes.NewZealand);

				RefUNLOCO gBXXX = CreateUNLOCOForTest("GB000", Constants.CountryCodes.UnitedKingdom);
				RefUNLOCO fRXXX = CreateUNLOCOForTest("FR000", Constants.CountryCodes.France);

				OrgHeader orgGB = CreateNewOrgForTest(gBXXX); // Located within the UK

				AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate1, ALL, "OTH", ALL, ALL, "AU", "EUN", "ALL", "ALL", "ALL", false, false);
				AccChargeTaxOverride override2 = CreateTaxOverride(code, TaxRate2, ALL, "OTH", ALL, ALL, "EUN", "AU", "ALL", "ALL", "ALL", false, false);
				AccChargeTaxOverride override3 = CreateTaxOverride(code, TaxRate3, ALL, "DOM", ALL, ALL, "AU", "EUN", "ALL", "ALL", "ALL", false, false);
				override1.AO_CustomsStatus = "T1";
				override2.AO_CustomsStatus = "T2";

				Factory.Save();

				CombineAssertions("Test Entry Details(customsStatus)", () =>
				{
					AssertTaxOverride(code, TaxRate1, aUXXX, gBXXX, orgGB, Directions.Import, ZString.Empty, "T1", "", null);
					AssertTaxOverride(code, null, aUXXX, gBXXX, orgGB, Directions.Import, ZString.Empty, "T2", "", null);
					AssertTaxOverride(code, null, aUXXX, gBXXX, orgGB, Directions.Import, ZString.Empty, "", "", null);

					AssertTaxOverride(code, null, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "T1", "", null);
					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "T2", "", null);
					AssertTaxOverride(code, null, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "", "", null);

					AssertTaxOverride(code, TaxRate3, aUXXX, gBXXX, orgGB, Directions.Domestic, ZString.Empty, "", "", null);
				});

				CombineAssertions("When CT Status is not empty or null , it will be used to match Entry Type instead of Entry Details(customsStatus)", () =>
				{
					AssertTaxOverride(code, null, aUXXX, gBXXX, orgGB, Directions.Import, ZString.Empty, "T1", "T2", null);
					AssertTaxOverride(code, TaxRate1, aUXXX, gBXXX, orgGB, Directions.Import, ZString.Empty, "T2", "T1", null);
					AssertTaxOverride(code, TaxRate1, aUXXX, gBXXX, orgGB, Directions.Import, ZString.Empty, "", "T1", null);

					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "T1", "T2", null);
					AssertTaxOverride(code, null, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "T2", "T1", null);
					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "", "T2", null);
				});

				AccChargeTaxOverride override4 = CreateTaxOverride(code, TaxRate2, ALL, "OTH", ALL, ALL, "EUN", "AU", "ALL", "ALL", "ALL", false, false);
				Factory.Save();

				CombineAssertions("Test Matching when Entry Type is not restricted", () =>
				{
					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "T1", "", null);
					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "T2", "", null);
					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "", "", null);

					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "", "T1", null);
					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "", "T2", null);
					AssertTaxOverride(code, TaxRate2, gBXXX, aUXXX, orgGB, Directions.Export, ZString.Empty, "", "TT", null);
				});
			}
		}

		public void TestGetTaxRateWithTransportMode()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "CCODE2";

			RefUNLOCO aUXXX = CreateUNLOCOForTest("AU000", Constants.CountryCodes.Australia);
			RefUNLOCO gBXXX = CreateUNLOCOForTest("GB000", Constants.CountryCodes.UnitedKingdom);
			OrgHeader orgGB = CreateNewOrgForTest(gBXXX); // Located within the UK

			AccChargeTaxOverride override1 = CreateTaxOverride(code, TaxRate8, "REV", "IMP", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			AccChargeTaxOverride override2 = CreateTaxOverride(code, TaxRate9, "REV", "IMP", "ALL", "SHP", "ALL", "ALL", "ALL", "SEA", "ALL", false, false);
			AccChargeTaxOverride override3 = CreateTaxOverride(code, TaxRate10, "REV", "DOM", "ALL", "SHP", "ALL", "ALL", "ALL", "AIR", "ALL", false, false);
			AccChargeTaxOverride override4 = CreateTaxOverride(code, TaxRate7, "REV", "EXP", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			override1.AO_VATExemptOnExportCharges = override2.AO_VATExemptOnExportCharges = override3.AO_VATExemptOnExportCharges = override4.AO_VATExemptOnExportCharges = false;
			Factory.Save();

			AssertTaxOverride(code, TaxRate9, aUXXX, gBXXX, orgGB, Directions.Import, "SEA", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(code, TaxRate8, aUXXX, gBXXX, orgGB, Directions.Import, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(code, TaxRate10, aUXXX, gBXXX, orgGB, Directions.Domestic, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(code, TaxRate7, aUXXX, gBXXX, orgGB, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
		}

		public void TestGetTaxRate_Branch()
		{
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "CCode1";

			var origin = CreateUNLOCOForTest("TEST1", Constants.CountryCodes.Australia);
			var destination = CreateUNLOCOForTest("TEST2", Constants.CountryCodes.UnitedKingdom);
			var orgHeader = CreateNewOrgForTest(destination);

			var testBranch = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)).First();

			var override1 = CreateTaxOverride(chargeCode, TaxRate9, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			override1.AO_GB = ZGuid.Empty;
			var override2 = CreateTaxOverride(chargeCode, TaxRate10, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			override2.AO_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			AssertTaxOverride(chargeCode, TaxRate9, origin, destination, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, testBranch);
			AssertTaxOverride(chargeCode, TaxRate10, origin, destination, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, GlbBranch.CurrentBranch);
		}

		public void TestGetTaxRate_HomeCountryOrZone_Branch()
		{
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "CCode1";

			var auUnlocoA = CreateUNLOCOWithStateForTest("TEST1", Constants.CountryCodes.Australia, "AAA");
			var auUnlocoB = CreateUNLOCOWithStateForTest("TEST2", Constants.CountryCodes.Australia, "BBB");
			var gbUnloco = CreateUNLOCOWithStateForTest("TEST3", Constants.CountryCodes.UnitedKingdom, "CCC");

			var auOrgHeaderA = Factory.New<OrgHeader>();
			auOrgHeaderA.OH_Code = "AU111";
			var address1 = auOrgHeaderA.Addresses[0];
			address1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			address1.OA_RL_NKRelatedPortCode = auUnlocoA.RL_Code;
			address1.OA_RN_NKCountryCode = "AU";
			address1.OA_State = auUnlocoA.CountryStates.RW_Code;

			var auOrgHeaderB = Factory.New<OrgHeader>();
			auOrgHeaderB.OH_Code = "AU222";
			var address2 = auOrgHeaderB.Addresses[0];
			address2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			address2.OA_RL_NKRelatedPortCode = auUnlocoB.RL_Code;
			address2.OA_RN_NKCountryCode = "AU";
			address2.OA_State = auUnlocoB.CountryStates.RW_Code;

			var gbOrgHeader = Factory.New<OrgHeader>();
			gbOrgHeader.OH_Code = "GB333";
			var address3 = gbOrgHeader.Addresses[0];
			address3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			address3.OA_RL_NKRelatedPortCode = gbUnloco.RL_Code;
			address3.OA_RN_NKCountryCode = "GB";
			address3.OA_State = gbUnloco.CountryStates.RW_Code;

			var auBranch = CreateNewBranchForTest(GlbCompany.CurrentCompany, "AAA", true);

			var override1 = CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "BST");
			var override2 = CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "BSX");
			var override3 = CreateTaxOverride(chargeCode, TaxRate3, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "ALL");
			Factory.Save();

			AssertTaxOverride(chargeCode, TaxRate1, auUnlocoA, auUnlocoB, auOrgHeaderA, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate2, auUnlocoB, auUnlocoB, auOrgHeaderB, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate3, gbUnloco, auUnlocoB, gbOrgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
		}

		public void TestGetTaxRate_HomeCountryOrZone_TaxZones()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "CCode1";

			var gbOrgHeader = Factory.New<OrgHeader>();
			gbOrgHeader.OH_Code = "GB333";
			var gbUnloco = CreateUNLOCOWithStateForTest("TSTGB", Constants.CountryCodes.UnitedKingdom, "CCC");
			var address3 = gbOrgHeader.Addresses[0];
			address3.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			address3.OA_RL_NKRelatedPortCode = gbUnloco.RL_Code;
			address3.OA_RN_NKCountryCode = "GB";
			address3.OA_State = gbUnloco.CountryStates.RW_Code;

			var newTaxZone = Factory.New<RefZoneHeader>();
			newTaxZone.FZ_Code = "TAXZ";
			newTaxZone.FZ_Description = "Tax Zone";
			newTaxZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;
			newTaxZone.UNLOCOs.Add(gbUnloco);

			var override1 = CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "TAXZ");
			var override2 = CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "ALL");
			Factory.Save();

			//Use Organisation.MainAddress
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertTaxOverride(chargeCode, TaxRate1, null, null, gbOrgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, null);
			}

			//Use Organisation.UNLOCO
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertTaxOverride(chargeCode, TaxRate1, null, null, gbOrgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, null);
			}
		}

		public void TestGetTaxRate_OriginDestination_Branch()
		{
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "CCode1";

			var auUnlocoA = CreateUNLOCOWithStateForTest("TEST1", Constants.CountryCodes.Australia, "AAA");
			var auUnlocoB = CreateUNLOCOWithStateForTest("TEST2", Constants.CountryCodes.Australia, "BBB");
			var gbUnloco = CreateUNLOCOWithStateForTest("TEST3", Constants.CountryCodes.UnitedKingdom, "CCC");
			var orgHeader = CreateNewOrgForTest(gbUnloco);

			var auBranch = CreateNewBranchForTest(GlbCompany.CurrentCompany, "AAA", true);

			var override1 = CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "SHP", "BST", "ALL", "ALL", "ALL", "ALL", false, false);
			var override2 = CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "SHP", "BSX", "ALL", "ALL", "ALL", "ALL", false, false);
			var override3 = CreateTaxOverride(chargeCode, TaxRate3, "ALL", "ALL", "ALL", "SHP", "ALL", "BST", "ALL", "ALL", "ALL", false, false);
			var override4 = CreateTaxOverride(chargeCode, TaxRate4, "ALL", "ALL", "ALL", "SHP", "ALL", "BSX", "ALL", "ALL", "ALL", false, false);
			var override5 = CreateTaxOverride(chargeCode, TaxRate5, "ALL", "ALL", "ALL", "SHP", "BST", "BST", "ALL", "ALL", "ALL", false, false);
			var override6 = CreateTaxOverride(chargeCode, TaxRate6, "ALL", "ALL", "ALL", "SHP", "BST", "BSX", "ALL", "ALL", "ALL", false, false);
			var override7 = CreateTaxOverride(chargeCode, TaxRate7, "ALL", "ALL", "ALL", "SHP", "BSX", "BST", "ALL", "ALL", "ALL", false, false);
			var override8 = CreateTaxOverride(chargeCode, TaxRate8, "ALL", "ALL", "ALL", "SHP", "BSX", "BSX", "ALL", "ALL", "ALL", false, false);
			Factory.Save();

			AssertTaxOverride(chargeCode, TaxRate1, auUnlocoA, gbUnloco, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate2, auUnlocoB, gbUnloco, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate3, gbUnloco, auUnlocoA, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate4, gbUnloco, auUnlocoB, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate5, auUnlocoA, auUnlocoA, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate6, auUnlocoA, auUnlocoB, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate7, auUnlocoB, auUnlocoA, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, TaxRate8, auUnlocoB, auUnlocoB, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
			AssertTaxOverride(chargeCode, null, gbUnloco, gbUnloco, orgHeader, Directions.Import, "SEA", ZString.Empty, ZString.Empty, auBranch);
		}

		public void TestTaxOverrideForDifferentTaxZone_OrgAddress()
		{
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "CCode1";

			var auUnlocoA = CreateUNLOCOForTest("TEST1", Constants.CountryCodes.Australia);
			var auUnlocoB = CreateUNLOCOForTest("TEST2", Constants.CountryCodes.Australia);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_RL_NKClosestPort = auUnlocoA.Code;
			org1.Addresses.AddNewMainAddress();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_RL_NKClosestPort = auUnlocoB.Code;
			org2.Addresses.AddNewMainAddress();

			var taxZoneA = Factory.New<RefZoneHeader>();
			taxZoneA.FZ_Code = "TAXA";
			taxZoneA.FZ_Description = "Tax Zone A";
			taxZoneA.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var taxZoneB = Factory.New<RefZoneHeader>();
			taxZoneB.FZ_Code = "TAXB";
			taxZoneB.FZ_Description = "Tax Zone B";
			taxZoneB.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var portZonePivotA = Factory.New<RefZonePivot>();
			portZonePivotA.F2_FZ = taxZoneA.PK;
			portZonePivotA.F2_ParentID = auUnlocoA.PK;
			portZonePivotA.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var portZonePivotB = Factory.New<RefZonePivot>();
			portZonePivotB.F2_FZ = taxZoneB.PK;
			portZonePivotB.F2_ParentID = auUnlocoB.PK;
			portZonePivotB.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var override1 = CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, taxZoneA.FZ_Code);
			var override2 = CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, taxZoneB.FZ_Code);

			Factory.Save();

			AssertTaxOverride(chargeCode, TaxRate1, null, null, org1, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(chargeCode, TaxRate2, null, null, org2, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
		}

		public void TestTaxOverrideForDifferentTaxZone_UNLOCO()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "CCode1";

			var auUnlocoA = CreateUNLOCOForTest("TEST1", Constants.CountryCodes.Australia);
			var auUnlocoB = CreateUNLOCOForTest("TEST2", Constants.CountryCodes.Australia);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_RL_NKClosestPort = auUnlocoA.Code;
			org1.Addresses.AddNewMainAddress();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_RL_NKClosestPort = auUnlocoB.Code;
			org2.Addresses.AddNewMainAddress();

			var taxZoneA = Factory.New<RefZoneHeader>();
			taxZoneA.FZ_Code = "TAXA";
			taxZoneA.FZ_Description = "Tax Zone A";
			taxZoneA.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var taxZoneB = Factory.New<RefZoneHeader>();
			taxZoneB.FZ_Code = "TAXB";
			taxZoneB.FZ_Description = "Tax Zone B";
			taxZoneB.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var portZonePivotA = Factory.New<RefZonePivot>();
			portZonePivotA.F2_FZ = taxZoneA.PK;
			portZonePivotA.F2_ParentID = auUnlocoA.PK;
			portZonePivotA.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var portZonePivotB = Factory.New<RefZonePivot>();
			portZonePivotB.F2_FZ = taxZoneB.PK;
			portZonePivotB.F2_ParentID = auUnlocoB.PK;
			portZonePivotB.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var override1 = CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, taxZoneA.FZ_Code);
			var override2 = CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, taxZoneB.FZ_Code);

			Factory.Save();

			AssertTaxOverride(chargeCode, TaxRate1, null, null, org1, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(chargeCode, TaxRate2, null, null, org2, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
		}

		public void TestGetTaxRate_HomeCountryState()
		{
			var bstUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INDEL");
			var bsxUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INHRI");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var branch = CreateNewBranchForTest(GlbCompany.CurrentCompany, "DL", true);
				var auUnlocoA = CreateUNLOCOWithStateForTest("TEST1", Constants.CountryCodes.Australia, "AAA");
				var auUnlocoB = CreateUNLOCOWithStateForTest("TEST2", Constants.CountryCodes.Australia, "BBB");

				var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
				testOrg1.Addresses[0].OA_RL_NKRelatedPortCode = bstUNLOCO.Code;
				testOrg1.Addresses[0].OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
				testOrg1.Addresses[0].State = "DL";

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				chargeCode.AC_Code = "CCode1";

				var override2 = CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "BST");
				var override3 = CreateTaxOverride(chargeCode, TaxRate3, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "BSX");
				var override4 = CreateTaxOverride(chargeCode, TaxRate4, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "ALX");
				var override5 = CreateTaxOverride(chargeCode, TaxRate5, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, null, "ALL");
				Factory.Save();

				//Fixed place of Supply location type = State | Fixed place of Supply location state is same as Branch state
				AssertTaxOverride(chargeCode, TaxRate2, auUnlocoA, auUnlocoB, testOrg1, Directions.Import, "SEA", ZString.Empty, ZString.Empty, branch, bstUNLOCO.CountryStates);
				//Fixed place of Supply location type = State | Fixed place of Supply location state is NOT same as Branch state
				AssertTaxOverride(chargeCode, TaxRate3, auUnlocoA, auUnlocoB, testOrg1, Directions.Import, "SEA", ZString.Empty, ZString.Empty, branch, bsxUNLOCO.CountryStates);
				//Fixed place of Supply location type = Rule | Fixed place of Supply location rule is ALX
				AssertTaxOverride(chargeCode, TaxRate4, auUnlocoA, auUnlocoB, testOrg1, Directions.Import, "SEA", ZString.Empty, ZString.Empty, branch, new LocationRule(GlbCompany.CurrentCompany, "ALX"));
				//Fixed place of Supply location type = UNLOCO | Fixed place of Supply location state is same as current Branch
				AssertTaxOverride(chargeCode, TaxRate2, auUnlocoA, auUnlocoB, testOrg1, Directions.Import, "SEA", ZString.Empty, ZString.Empty, branch, bstUNLOCO);
			}
		}

		public void TestGetTaxRate_TransactionContext()
		{
			using (AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				chargeCode.AC_Code = "CCode1";

				CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, transactionContext: "INT");
				CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, transactionContext: "STD");

				Factory.Save();

				AssertTaxOverride(chargeCode, null, null, null, null, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, transactionContext: "INT");// transactionContext = INT Defaulting Rule will default to ART so TaxID should be null
				AssertTaxOverride(chargeCode, TaxRate2, null, null, null, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, transactionContext: "STD");
			}
		}

		public void TestGetTaxRate_SupplyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				chargeCode.AC_Code = "CCode1";

				CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, supplyType: AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC);
				CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, supplyType: AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX);
				CreateTaxOverride(chargeCode, TaxRate3, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);

				Factory.Save();

				AssertTaxOverride(chargeCode, TaxRate1, null, null, null, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, supplyType: AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC);
				AssertTaxOverride(chargeCode, TaxRate2, null, null, null, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, supplyType: AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOX);
				AssertTaxOverride(chargeCode, TaxRate3, null, null, null, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, supplyType: AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INX);
			}
		}

		public void TestGetTaxOverride_TaxRegistration()
		{
			var loader = new RefUNLOCO.Loader(Factory);

			var australiaOrganisation = CreateNewOrgForTest(loader.Load("AUSYD"));

			var aseanOrganisation = CreateNewOrgForTest(loader.Load("SGSIN"));

			var indiaOrganisation = CreateNewOrgForTest(loader.Load("INBOM"));

			var noTaxCodeOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			noTaxCodeOrganisation.OH_Code = "NOTAXORG";
			noTaxCodeOrganisation.OH_RL_NKClosestPort = "INHYD";

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			CreateDefaultTaxOverride(chargeCode, TaxRate1).AO_TaxRegCntryOrGroup = ALL;
			CreateDefaultTaxOverride(chargeCode, TaxRate2).AO_TaxRegCntryOrGroup = Constants.CountryCodes.Australia;
			CreateDefaultTaxOverride(chargeCode, TaxRate3).AO_TaxRegCntryOrGroup = EconomicGroupList.Codes.ASEAN;
			CreateDefaultTaxOverride(chargeCode, TaxRate4).AO_TaxRegCntryOrGroup = Constants.CountryCodes.India;
			CreateDefaultTaxOverride(chargeCode, TaxRate5).AO_TaxRegCntryOrGroup = OrgCusCode.CodeTypes.SpecialEconomicZone;

			Factory.Save();

			AssertTaxOverride(chargeCode, TaxRate1, null, null, null, Directions.Unknown, ALL, "", "", null);
			AssertTaxOverride(chargeCode, TaxRate2, null, null, australiaOrganisation, Directions.Unknown, ALL, "", "", null);
			AssertTaxOverride(chargeCode, TaxRate3, null, null, aseanOrganisation, Directions.Unknown, ALL, "", "", null);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Singapore))
			{
				//Login in country should not change the results
				AssertTaxOverride(chargeCode, TaxRate2, null, null, australiaOrganisation, Directions.Unknown, ALL, "", "", null);
				AssertTaxOverride(chargeCode, TaxRate3, null, null, aseanOrganisation, Directions.Unknown, ALL, "", "", null);
			}

			AssertTaxOverride(chargeCode, TaxRate4, null, null, indiaOrganisation, Directions.Unknown, ALL, "", "", null);
			AssertTaxOverride(chargeCode, TaxRate1, null, null, noTaxCodeOrganisation, Directions.Unknown, ALL, "", "", null);
		}

		public void TestGetTaxOverride_TaxRegistration_SpecialEconomicZoneIsCompanyCountrySpecific()
		{
			var refUnloco = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			var sezOrganisation = CreateNewOrgForTest(refUnloco);
			sezOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SpecialEconomicZone, "111", Constants.CountryCodes.Australia);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			CreateDefaultTaxOverride(chargeCode, TaxRate1).AO_TaxRegCntryOrGroup = ALL;
			CreateDefaultTaxOverride(chargeCode, TaxRate2).AO_TaxRegCntryOrGroup = Constants.CountryCodes.Australia;
			CreateDefaultTaxOverride(chargeCode, TaxRate3).AO_TaxRegCntryOrGroup = OrgCusCode.CodeTypes.SpecialEconomicZone;

			Factory.Save();

			AssertTaxOverride(chargeCode, TaxRate3, null, null, sezOrganisation, Directions.Unknown, ALL, "", "", null);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				AssertTaxOverride(chargeCode, TaxRate2, null, null, sezOrganisation, Directions.Unknown, ALL, "", "", null);
			}
		}

		public void TestGetTaxOverride_TaxRegistration_SpecialEconomicZoneOverridesToExemptOrganisations()
		{
			var taxExemptIndiaOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			taxExemptIndiaOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SpecialEconomicZone, "111", Constants.CountryCodes.India);
			taxExemptIndiaOrganisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode,
				IndiaComplianceInfo.UnreportedRegistrationNumber, Constants.CountryCodes.India);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			CreateDefaultTaxOverride(chargeCode, TaxRate1).AO_TaxRegCntryOrGroup = ALL;
			CreateDefaultTaxOverride(chargeCode, TaxRate2).AO_TaxRegCntryOrGroup = Constants.CountryCodes.India;
			CreateDefaultTaxOverride(chargeCode, TaxRate3).AO_TaxRegCntryOrGroup = OrgCusCode.CodeTypes.SpecialEconomicZone;

			Factory.Save();

			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockOrgCusCodePredicateProvider = new Mock<IOrgCusCodePredicateProvider>();
			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				mockICountryComplianceFactory.Setup(x => x.GetIOrgCusCodePredicateProvider(It.IsAny<ZString>()))
					.Returns(mockOrgCusCodePredicateProvider.Object);

				mockOrgCusCodePredicateProvider.Setup(x => x.IncludeForOrgHeaderTaxRegsistration(It.IsAny<OrgCusCode>())).Returns(true);
				AssertTaxOverride(chargeCode, TaxRate3, null, null, taxExemptIndiaOrganisation, Directions.Unknown, ALL, "", "", null);

				mockOrgCusCodePredicateProvider.Setup(x => x.IncludeForOrgHeaderTaxRegsistration(It.IsAny<OrgCusCode>())).Returns(false);

				// Because it's such a specific edge case, we're going with the simpliest solution which is both
				// SEZ code and the org is not to be IncludeForOrgHeaderTaxRegsistration, then SEZ wins because
				// realistically why else would you add the SEZ?
				AssertTaxOverride(chargeCode, TaxRate3, null, null, taxExemptIndiaOrganisation, Directions.Unknown, ALL, "", "", null);
			}
		}

		public void TestGetTaxRate_DebtorRole()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_Code = "CCode1";

			CreateTaxOverride(chargeCode, TaxRate1, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, debtorRole: DebtorRoleList.Codes.Agent);
			CreateTaxOverride(chargeCode, TaxRate2, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, debtorRole: DebtorRoleList.Codes.NotConsignorOrConsignee);
			CreateTaxOverride(chargeCode, TaxRate3, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false);
			CreateTaxOverride(chargeCode, TaxRate4, "ALL", "ALL", "ALL", "SHP", "ALL", "ALL", "ALL", "ALL", "ALL", false, false, debtorRole: DebtorRoleList.Codes.NotOwnGoods);

			Factory.Save();

			OrgHeader debtorForwarder = Factory.NewWithValidTestData<OrgHeader>();
			debtorForwarder.OH_IsForwarder = true;
			OrgHeader debtorNonForwarder = Factory.NewWithValidTestData<OrgHeader>();
			debtorNonForwarder.OH_IsForwarder = false;
			OrgHeader debtorForwarderNeverOwner = Factory.NewWithValidTestData<OrgHeader>();
			debtorForwarderNeverOwner.OH_IsForwarder = true;
			debtorForwarderNeverOwner.CompanyData.OB_ARGoodsOwnership = OrgCompanyDataLookups.GoodsOwnership.NeverOwner;
			OrgHeader debtorNonForwarderNeverOwner = Factory.NewWithValidTestData<OrgHeader>();
			debtorNonForwarderNeverOwner.OH_IsForwarder = false;
			debtorNonForwarderNeverOwner.CompanyData.OB_ARGoodsOwnership = OrgCompanyDataLookups.GoodsOwnership.NeverOwner;
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();

			AssertTaxOverride(chargeCode, TaxRate1, null, null, debtorForwarder, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(chargeCode, TaxRate1, null, null, debtorForwarder, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: debtorForwarder, consignee: consignee);
			AssertTaxOverride(chargeCode, TaxRate1, null, null, debtorForwarder, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: consignor, consignee: debtorForwarder);

			AssertTaxOverride(chargeCode, TaxRate2, null, null, debtorNonForwarder, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(chargeCode, TaxRate3, null, null, debtorNonForwarder, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: debtorNonForwarder, consignee: consignee);
			AssertTaxOverride(chargeCode, TaxRate3, null, null, debtorNonForwarder, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: consignor, consignee: debtorNonForwarder);

			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: consignor);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignee: consignee);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: debtorForwarderNeverOwner);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignee: debtorForwarderNeverOwner);

			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorNonForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorNonForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: consignor);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorNonForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignee: consignee);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorNonForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignor: debtorNonForwarderNeverOwner);
			AssertTaxOverride(chargeCode, TaxRate4, null, null, debtorNonForwarderNeverOwner, Directions.Export, "AIR", ZString.Empty, ZString.Empty, null, consignee: debtorNonForwarderNeverOwner);
		}

		#region Implementation

		void AssertTaxOverride(AccChargeCode code, AccTaxRate expectedTaxRate, ILocation origin, ILocation destination, OrgHeader organisation, Directions direction, ZString transportMode, GlbBranch branch = null, AccInvMsg expectedInvTaxMsg = null)
		{
			AssertTaxOverride(code, expectedTaxRate, origin, destination, organisation, direction, transportMode, null, null, branch, null, expectedInvTaxMsg);
		}

		void AssertTaxOverride(AccChargeCode code, AccTaxRate expectedTaxRate, ILocation origin, ILocation destination, OrgHeader organisation, Directions direction, ZString transportMode, ZString customsStatus, ZString communityTransitStatus, GlbBranch branch,
			ILocation fixedPlaceofSupply = null, AccInvMsg expectedInvTaxMsg = null, string transactionContext = "", string supplyType = "", OrgHeader consignor = null, OrgHeader consignee = null)
		{
			var actualTaxRate = code.GetChargeTaxOverride(GetParameters(ALL, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, direction, transportMode, organisation, origin, destination, branch, customsStatus, communityTransitStatus, fixedPlaceofSupply, transactionContext, supplyType, consignor, consignee));
			if (expectedTaxRate == null)
			{
				AssertNull(actualTaxRate?.TaxRate);
			}
			else
			{
				AssertNotNull($"Expected Tax Rate '{expectedTaxRate.AT_Code}', but actual Tax Rate was null", actualTaxRate);
				AssertEquals("Tax Rate Override", expectedTaxRate.AT_Code, actualTaxRate.TaxRate.AT_Code);
				if (expectedInvTaxMsg != null)
				{
					AssertEquals("Tax Rate Override", expectedInvTaxMsg.PK, actualTaxRate.AO_A9_DefaultVATClass);
				}
			}
		}

		AccTaxRate TaxRate1;
		AccTaxRate TaxRate2;
		AccTaxRate TaxRate3;
		AccTaxRate TaxRate4;
		AccTaxRate TaxRate5;
		AccTaxRate TaxRate6;
		AccTaxRate TaxRate7;
		AccTaxRate TaxRate8;
		AccTaxRate TaxRate9;
		AccTaxRate TaxRate10;
		AccTaxRate TaxRate11;
		AccTaxRate TaxRate12;
		AccTaxRate TaxRate13;
		AccTaxRate TaxRate14;
		AccTaxRate TaxRate15;

		const string ALL = "ALL";

		protected override void SetUp()
		{
			base.SetUp();
			TaxRate1 = CreateTaxRateForTest("TAX1");
			TaxRate2 = CreateTaxRateForTest("TAX2");
			TaxRate3 = CreateTaxRateForTest("TAX3");
			TaxRate4 = CreateTaxRateForTest("TAX4");
			TaxRate5 = CreateTaxRateForTest("TAX5");
			TaxRate6 = CreateTaxRateForTest("TAX6");
			TaxRate7 = CreateTaxRateForTest("TAX7");
			TaxRate8 = CreateTaxRateForTest("TAX8");
			TaxRate9 = CreateTaxRateForTest("TAX9");
			TaxRate10 = CreateTaxRateForTest("TAX10");
			TaxRate11 = CreateTaxRateForTest("TAX11");
			TaxRate12 = CreateTaxRateForTest("TAX12");
			TaxRate13 = CreateTaxRateForTest("TAX13");
			TaxRate14 = CreateTaxRateForTest("TAX14");
			TaxRate15 = CreateTaxRateForTest("TAX15");
		}

		AccTaxRate CreateTaxRateForTest(ZString code)
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return taxRate;
		}

		AccChargeTaxOverride CreateDefaultTaxOverride(AccChargeCode chargeCode, AccTaxRate taxRate)
		{
			return CreateTaxOverride(chargeCode, taxRate, ALL, ALL, ALL, ALL, ALL, ALL, ALL, ALL, ALL, false, false);
		}

		AccChargeTaxOverride CreateTaxOverride(AccChargeCode chargeCode, AccTaxRate taxRate, ZString costSell, ZString direction, ZString incoTerm, ZString jobType,
			ZString origin, ZString destination, ZString taxRegCountry, ZString transportMode, ZString orgCategory, ZBool exporterExemption, ZBool splitVATPaymentOrg, AccInvMsg invTaxMsg = null, string homeCountryOrZone = "",
			string transactionContext = "", string supplyType = "", string debtorRole = "", bool createTaxRecord = true)
		{
			AccChargeTaxOverride @override = chargeCode.TaxOverrides.AddNew();
			@override.AO_AT = taxRate.PK;
			@override.AO_CostSellAll = costSell;
			@override.AO_Direction = direction;
			@override.AO_IncoTerm = incoTerm;
			@override.AO_JobType = jobType;
			@override.AO_TaxRegCntryOrGroup = taxRegCountry;
			@override.AO_Origin = origin;
			@override.AO_Destination = destination;
			@override.AO_TransportMode = transportMode;
			@override.AO_OrganisationCategory = orgCategory;
			@override.AO_HomeCountryOrZone = homeCountryOrZone;
			@override.AO_VATExemptOnExportCharges = exporterExemption;
			@override.AO_SplitPaymentVATOrganisation = splitVATPaymentOrg;
			@override.AO_SupplyType = supplyType;
			@override.AO_DebtorRole = debtorRole;
			@override.AO_CreateTaxRecord = createTaxRecord;

			if (invTaxMsg != null)
			{
				@override.AO_A9_DefaultVATClass = invTaxMsg.PK;
			}

			if (!string.IsNullOrEmpty(transactionContext))
			{
				@override.AO_TransactionContext = transactionContext;
			}

			if (!string.IsNullOrEmpty(supplyType))
			{
				@override.AO_SupplyType = supplyType;
			}

			return @override;
		}

		RefUNLOCO CreateUNLOCOForTest(ZString uNLOCOCode, ZString countryCode)
		{
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = uNLOCOCode;

			RefCountry country = RefCountry.LoadFromCountryCode(Factory, countryCode);
			uNLOCO.RL_RN_NKCountryCode = country.Code;

			return uNLOCO;
		}

		RefUNLOCO CreateUNLOCOWithStateForTest(ZString unlocoCode, ZString countryCode, ZString stateCode)
		{
			var unloco = CreateUNLOCOForTest(unlocoCode, countryCode);
			var state = Factory.New<RefCountryStates>();
			state.RW_Code = stateCode;
			state.RW_RN_NKCountryCode = countryCode;
			unloco.RL_RW = state.PK;

			return unloco;
		}

		GlbBranch CreateNewBranchForTest(GlbCompany company, string stateCode, bool createBranchOrgProxy)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.Addresses.RemoveAll();
			var address = orgProxy.Addresses.AddNew();
			address.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			address.State = stateCode;
			if (createBranchOrgProxy)
			{
				branch.GB_OH_OrgProxy = orgProxy.PK;
			}
			else
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			return branch;
		}

		OrgHeader CreateNewOrgForTest(RefUNLOCO port)
		{
			OrgHeader newOrg = Factory.New<OrgHeader>();
			newOrg.OH_Code = port.RL_RN_NKCountryCode + " ORG";
			newOrg.OH_RL_NKClosestPort = port.Code;

			OrgCusCode taxCode = newOrg.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = port.RL_RN_NKCountryCode;
			taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(port.RL_RN_NKCountryCode);
			taxCode.OK_CustomsRegNo = "123456789";

			return newOrg;
		}

		AccChargeTaxOverrideMatcher.TaxCalculationParameters GetParameters(ZString incoTerm, CostSell costOrSell, ZString jobType, Directions direction, ZString transportMode, OrgHeader organisation,
			ILocation origin, ILocation destination, GlbBranch branch, ZString customsStatus, ZString communityTransitStatus, ILocation fixedPlaceOfSupply, string transactionContext = "", string supplyType = "",
			OrgHeader consignor = null, OrgHeader consignee = null)
		{
			return new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = incoTerm,
				CostOrSell = costOrSell,
				JobType = jobType,
				Direction = direction,
				TransportMode = transportMode,
				Organisation = organisation,
				Origin = origin,
				Destination = destination,
				CustomsStatus = customsStatus,
				CommunityTransitStatus = communityTransitStatus,
				FixedPlaceOfSupply = fixedPlaceOfSupply,
				Branch = branch,
				TransactionContext = transactionContext,
				SupplyType = supplyType,
				Consignor = consignor,
				Consignee = consignee
			};
		}

		#endregion
	}
}
