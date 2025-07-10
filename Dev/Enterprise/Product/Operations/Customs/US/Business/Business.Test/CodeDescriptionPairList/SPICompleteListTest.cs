using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SPICompleteListTest : TestCaseWithFactory
	{
		public void TestGetCachedList()
		{
			var list = SPICompleteList.GetCachedList(Factory);
			AssertEquals(39, list.Count);

			var newList = Factory.GetCachedValue("WithNotApplicable", () => new SPICompleteList());
			AssertSame(list, newList);
		}

		public void TestGetCachedListWithoutNotApplicable()
		{
			var list = SPICompleteList.GetCachedListWithoutNotApplicable(Factory);
			AssertEquals(38, list.Count);
			Assert(!list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));

			var newList = Factory.GetCachedValue("NoNotApplicable", () => new SPICompleteList());
			AssertSame(list, newList);
		}

		public void TestDifferentConstructor()
		{
			var list = new SPICompleteList();
			AssertEquals(39, list.Count);

			list = new SPICompleteList(addNotApplicable: false);
			AssertEquals(38, list.Count);

			list = new SPICompleteList(addNotApplicable: true);
			AssertEquals(39, list.Count);
		}

		public void TestCanBeClaimedForMPFExemptionForDutyFreeTariffs()
		{
			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.CA, null, ZDateTime.BrettsBirthday));
			AssertEquals(false, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.KSharp, null, ZDateTime.BrettsBirthday));

			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.MX, null, ZDateTime.BrettsBirthday));
			AssertEquals(false, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.CSharp, null, ZDateTime.BrettsBirthday));

			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.OM, null, ZDateTime.BrettsBirthday));
			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.SG, null, ZDateTime.BrettsBirthday));
			AssertEquals(false, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.BSharp, null, ZDateTime.BrettsBirthday));

			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.AU, null, ZDateTime.BrettsBirthday));
			AssertEquals(false, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.LSharp, null, ZDateTime.BrettsBirthday));

			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.BH, null, ZDateTime.BrettsBirthday));
			AssertEquals(false, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.MA, null, ZDateTime.BrettsBirthday));

			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.PPlus, null, ZDateTime.BrettsBirthday));
			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(PrimarySpecProgramIndicatorList.Codes.P, null, ZDateTime.BrettsBirthday));
			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(PrimarySpecProgramIndicatorList.Codes.Y, null, ZDateTime.BrettsBirthday));
			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.PA, null, ZDateTime.BrettsBirthday));

			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.S, null, ZDateTime.BrettsBirthday));
			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.SPlus, null, ZDateTime.BrettsBirthday));

			var country = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "KR");
			country.UC_SPICode = "K";
			country.UC_SPIBeginDate = new ZDateTime(2012, 3, 15);
			country.UC_SPIEndDate = new ZDateTime(2099, 12, 31);
			AssertEquals(false, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.KR, country, ZDateTime.BrettsBirthday));
			AssertEquals(true, SPICompleteList.CanBeClaimedForMPFExemptionForDutyFreeTariffs(SpecialProgramList.Codes.KR, country, new ZDateTime(2012, 3, 15)));
		}

		public void TestPeruCanBeClaimedForMPFExemption_CS00112164()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0803002020";
			invoiceLine.US_UC_NKCountryOfOrigin = "PE";
			invoiceLine.JI_LinePrice = 10000m;

			AssertEquals(2, invoiceLine.AddInfoLookups.SPIList.Count);
			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode("PE"));
			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode("N/A"));

			invoiceLine.US_SPI = "PE";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MPFAmountForEntry);
		}

		[TestDate(2009, 1, 1)]
		public void TestSPIListForWatchAssembly()
		{
			var declaration = CreateWatchAssemblyDeclaration();

			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];
			var invoiceLine3 = declaration.InvoiceLines[2];
			var invoiceLine4 = declaration.InvoiceLines[3];

			AssertSPIList(invoiceLine1);
			AssertSPIList(invoiceLine2);
			AssertSPIList(invoiceLine3);
			AssertSPIList(invoiceLine4);
		}

		[TestDate(2009, 1, 1)]
		public void TestNAIsNotAnOptionWhenSPIBecomesMandatory()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "99100466";
			invoiceLine.JI_Tariff = "3902205000";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";

			AssertEquals(1, invoiceLine.AddInfoLookups.SPIList.Count);
			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode("SG"));
			Assert(!invoiceLine.AddInfoLookups.SPIList.ContainsCode("N/A"));
		}

		[TestDate(2009, 1, 1)]
		public void TestNAIsAvailableForTariffEvenWhenItHasInvalidDutyRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9015900030";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";

			AssertEquals(3, invoiceLine.AddInfoLookups.SPIList.Count);
			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode("SG"));
			Assert("SPI is not mandatory for 9015900030", invoiceLine.AddInfoLookups.SPIList.ContainsCode("N/A"));
		}

		public void TestWhenParentTariffDoesNotAllowSPI()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.NoSPIRequired;
			tariffRule.U1_Tariff = "00000000";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			AssertEquals("SG should not appear in the list: Parent, only N/A", 1, invoiceLine.AddInfoLookups.SPIList.Count);

			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "7017103000";
			AssertEquals("SG should not appear in the list:Secondary Line, only N/A", 1, secondaryLine.AddInfoLookups.SPIList.Count);
		}

		public void TestWhen9899TariffDoesNotAllowSPI()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.NoSPIRequired;
			tariffRule.U1_Tariff = "98206139";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98206139";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000000";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98206139";
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_UC_NKCountryOfOrigin = "HT";
			AssertEquals("W should not appear in the list, only N/A", 1, invoiceLine.AddInfoLookups.SPIList.Count);
			AssertEquals("N/A", invoiceLine.AddInfoLookups.SPIList[0].Code);
		}

		[TestDate(2009, 1, 1)]
		public void TestForFreelyAssociatedStateDutyFreeTreatmentExceptions()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();

			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.FASDutyFreeTreatmentException;
			tariffRule.U1_Tariff = USCTariff.CottonFeeApplicable.Substring(0, 4);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine.US_UC_NKCountryOfOrigin = "FM";
			AssertEquals("PreCondition", true, invoiceLine.ImportTariff.Applies(TariffRuleList.Codes.FASDutyFreeTreatmentException, invoiceLine.EffectiveDateForDutyRate));

			AssertEquals("PreCondition:Z is a valid SPI", true, invoiceLine.CountryOfOrigin_US.IsValidForSPI("Z", invoiceLine.EffectiveDateForDutyRate));

			AssertEquals("However the tariff falls into a rule, 'FDE' (Freely Associated States Duty Free Treatment Exception)", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode("Z"));

			invoiceLine.JI_Tariff = "0101110010";
			AssertEquals("PreCondition", false, invoiceLine.ImportTariff.Applies(TariffRuleList.Codes.FASDutyFreeTreatmentException, invoiceLine.EffectiveDateForDutyRate));
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode("Z"));
		}

		public void TestGetSPIListRegardlessCountryOfOriginWhenTariffCAFTA()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_Tariff = "00000080";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_DateTo = ZDateTime.MaxSmallDateTime;
			tariffRule.U1_RuleCode = "";
			USCTariffRule tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_Tariff = "00000070";
			tariffRule2.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule2.U1_DateTo = ZDateTime.MaxSmallDateTime;
			tariffRule2.U1_RuleCode = "";

			USCTariff tariffNoP = Factory.New<USCTariff>();
			tariffNoP.UE_Tariff = "00000070";
			tariffNoP.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffNoP.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariffNoP.UE_SPICode = "D AUBHCACLILJ+JOMAMXSG";

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "000000080";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Column1RateAdValorem = 0.08500000m;
			tariff.UE_Column1RateSpecific = 0.42300000m;
			tariff.UE_Column1RateOther = 0.0m;
			tariff.UE_SPICode = "P AUBHCACLILP+JOMAMXSG";

			USCTariff tariffWithRate = Factory.New<USCTariff>();
			tariffWithRate.UE_Tariff = "000000090";
			tariffWithRate.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariffWithRate.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariffWithRate.UE_Column1RateAdValorem = 0.08500000m;
			tariffWithRate.UE_Column1RateSpecific = 0.42300000m;
			tariffWithRate.UE_Column1RateOther = 0.0m;
			tariffWithRate.UE_SPICode = "P AUBHCACLILP+JOMAMXSG";

			USCTariffDutyRate tariffDutyRate1 = Factory.New<USCTariffDutyRate>();
			tariffDutyRate1.UD_UE = tariffWithRate.PK;
			tariffDutyRate1.UD_ISOCountryCode = "P+";
			tariffDutyRate1.UD_SpecificSpecialRate = 9999.999999m;
			tariffDutyRate1.UD_AdValoremSpecialRate = 0.0m;
			tariffDutyRate1.UD_OtherSpecialRate = 0.0m;

			Factory.Save();

			USCCountry au = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "AU");
			USCCountry cr = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "CR");
			cr.UC_MiscellaneousSPIIndicator = "P";

			var listNoP = SPICompleteList.GetRelevantListFor(new SPILine(tariffNoP, au, ZDateTime.Today, Factory, cr));
			AssertEquals("List should have 4 elements including N/A, P, P+ and AU", 4, listNoP.Count);

			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, au, ZDateTime.Today, Factory, cr));
			AssertEquals("List should have 4 elements because Country doesn't have SPIIndicator", 4, list.Count);
			AssertEquals("Should contain AU", true, list.ContainsCode(SpecialProgramList.Codes.AU));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("Should contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));

			var countryHasP = SPICompleteList.GetRelevantListFor(new SPILine(tariffWithRate, au, ZDateTime.Today, Factory, cr));
			AssertEquals("List should have 3 elements because Country has SPIIndicator P.", 3, countryHasP.Count);
			AssertEquals("Should Not contain P+", false, countryHasP.ContainsCode(SpecialProgramList.Codes.PPlus));
		}

		public void TestGetListWhenNoSPIIsAllowed()
		{
			USCTariffRule tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_Tariff = "00000000";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_DateTo = ZDateTime.MaxSmallDateTime;
			tariffRule.U1_RuleCode = TariffRuleList.Codes.NoSPIRequired;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			AssertEquals("NO SPI is Allowed", true, tariff.Applies(TariffRuleList.Codes.NoSPIRequired, ZDateTime.Today));

			USCCountry hn = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "hn");
			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, hn, ZDateTime.Today, Factory));
			AssertEquals("List should have no elements", 1, list.Count);
		}

		public void TestGetRelevantSPIListWhenTariffIsNull()
		{
			var au = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "AU");
			var spiLine = new SPILine(null, au, ZDateTime.Today, Factory, au);
			var list = SPICompleteList.GetRelevantListFor(spiLine);
			Assert("N/A is in the list", list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			Assert("AU should be in the list as it might be valid for MPF exemption", list.ContainsCode("AU"));

			spiLine = new SPILine(null, null, ZDateTime.Today, Factory);
			list = SPICompleteList.GetRelevantListFor(spiLine);
			AssertEquals("Do not include P, P+ as there is no Country of Export in CAFTA", 37, list.Count);
		}

		public void TestGetRelevantSPIListWhenCountryOfOriginIsNull()
		{
			tariff.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXP+SG";

			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, null, ZDateTime.Today, Factory));
			AssertEquals("List should have 19 elements including N/A", 19, list.Count);

			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.AU));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.BH));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.CA));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.CL));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.IL));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.JPlus));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.JO));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.MA));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.MX));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(SpecialProgramList.Codes.SG));

			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.N));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.Y));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.W));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.Z));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("SPIs excluded from db check should be in the list", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.R));
			AssertEquals("List should contain spi from tariff", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
		}

		public void TestGetRelevantSPIListForPassedCountryOfExport()
		{
			tariff.UE_SPICode = "D R P AUBHCACLILJ+JOMAMXP+SG";

			USCCountry au = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "AU");
			au.UC_MiscellaneousSPIIndicator = "P";

			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, au, ZDateTime.Today, Factory, au));
			AssertEquals("List should have 4 elements including N/A", 4, list.Count);

			AssertEquals("Should contain AU", true, list.ContainsCode(SpecialProgramList.Codes.AU));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("Should contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));

			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, au, ZDateTime.Today, Factory));
			AssertEquals("List should have 4 elements including N/A", 4, list.Count);
			AssertEquals("Should contain AU", true, list.ContainsCode(SpecialProgramList.Codes.AU));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("Should contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
		}

		public void TestGetRelevantSPIListForGSPCountry()
		{
			tariff.UE_SPICode = "A D R AUBHCACLILJ+JOMAMXSG";

			var cv = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "CV");
			//setting up preconditions
			cv.UC_GSPIndicator = true;
			//least developed country
			cv.UC_RateColumnIndicator = "3";
			cv.UC_RateColumnBeginDate = ZDateTime.Empty;
			cv.UC_RateColumnEndDate = ZDateTime.Empty;
			cv.UC_SpecialTradeProgramsBeginDate = ZDateTime.Today.AddYears(-1);
			cv.UC_SpecialTradeProgramsEndDate = ZDateTime.Today.AddYears(1);

			var list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, cv, ZDateTime.Today, Factory));
			AssertEquals("List should have 3 elements including N/A", 3, list.Count);

			AssertEquals("Should contain A", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("Should contain D", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));

			//GSP Excluded Countries
			tariff.UE_SPICode = "A*D R AUBHCACLILJ+JOMAMXSG";
			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, cv, ZDateTime.Today, Factory));

			AssertEquals("List should have 3 elements including N/A", 3, list.Count);
			AssertEquals("Should contain A", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("Should contain D", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));

			tariff.UE_GSPExcludedCountries = "CV";
			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, cv, ZDateTime.Today, Factory));
			AssertEquals("List should have 2 elements including N/A", 2, list.Count);

			AssertEquals("Should contain D", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));

			//Least Developed Country
			tariff.UE_SPICode = "A+D R AUBHCACLILJ+JOMAMXSG";
			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, cv, ZDateTime.Today, Factory));
			AssertEquals("List should have 3 elements including N/A", 3, list.Count);
			AssertEquals("Should contain A", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("Should contain D", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));

			var kr = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "KR");
			kr.UC_GSPIndicator = true;
			kr.UC_GSPEndDate = ZDateTime.Empty;

			tariff.UE_SPICode = "A+D R AUBHCACLILJ+JOMAMXSG";
			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, kr, ZDateTime.Today, Factory));
			AssertEquals("List should have no elements including N/A", 1, list.Count);

			tariff.UE_SPICode = "A D R AUBHCACLILJ+JOMAMXSG";
			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, kr, ZDateTime.Today, Factory));
			AssertEquals("List should have 2 elements including N/A", 2, list.Count);
			AssertEquals("Should contain A", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
		}

		public void TestGetRelevantListForCanada()
		{
			tariff.UE_SPICode = "A D R P AUBHCACLILJ+JOMAMXP+SG";

			USCCountry xo = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "XO");

			USCCountry cr = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "CR");
			cr.UC_SPICode = "P";
			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, xo, ZDateTime.Today, Factory, cr));
			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, xo, ZDateTime.Today, Factory, cr));
			AssertEquals("List should have 6 elements including N/A", 6, list.Count);
			AssertEquals("Should contain CA", true, list.ContainsCode(SpecialProgramList.Codes.CA));
			AssertEquals("Should contain S", true, list.ContainsCode(SpecialProgramList.Codes.S));
			AssertEquals("SHould contain S+", true, list.ContainsCode(SpecialProgramList.Codes.SPlus));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("Should contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));

			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, xo, ZDateTime.Today, Factory));
			AssertEquals("List should have 6 elements including N/A", 6, list.Count);
			AssertEquals("Should contain CA", true, list.ContainsCode(SpecialProgramList.Codes.CA));
			AssertEquals("Should contain S", true, list.ContainsCode(SpecialProgramList.Codes.S));
			AssertEquals("SHould contain S+", true, list.ContainsCode(SpecialProgramList.Codes.SPlus));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("Should contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));

			tariff.UE_SPICode = "A D R P AUBHB P+";
			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, xo, ZDateTime.Today, Factory, cr));
			AssertEquals("List should have 8 elements including N/A", 8, list.Count);
			AssertEquals("Should contain B#", true, list.ContainsCode(SpecialProgramList.Codes.BSharp));
			AssertEquals("Should contain B", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.B));
			AssertEquals("Should contain CA", true, list.ContainsCode(SpecialProgramList.Codes.CA));
			AssertEquals("Should contain S", true, list.ContainsCode(SpecialProgramList.Codes.S));
			AssertEquals("SHould contain S+", true, list.ContainsCode(SpecialProgramList.Codes.SPlus));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("Should contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
		}

		public void TestGetRelevantListForKorea()
		{
			tariff.UE_SPICode = "";

			var country = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "KR");
			var list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, country, new ZDateTime(2012, 3, 14), Factory));
			AssertEquals("KR is not effective yet", 1, list.Count);

			country.UC_SPICode = "K";
			country.UC_SPIBeginDate = new ZDateTime(2012, 3, 15);
			country.UC_SPIEndDate = new ZDateTime(2099, 12, 31);

			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, country, new ZDateTime(2012, 3, 15), Factory));
			AssertEquals("List should have 2 elements including N/A", 2, list.Count);
			AssertEquals("SHould contain KR", true, list.ContainsCode(SpecialProgramList.Codes.KR));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));

			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, country, new ZDateTime(2012, 3, 14), Factory));
			AssertEquals("KR is not effective yet", 1, list.Count);
		}

		public void TestGetRelevantListForColombia()
		{
			tariff.UE_SPICode = "";

			var country = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "CO");
			var list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, country, new ZDateTime(2012, 3, 14), Factory));

			AssertEquals("List should have 2 elements including N/A", 2, list.Count);
			AssertEquals("SHould contain KR", true, list.ContainsCode(SpecialProgramList.Codes.CO));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
		}

		public void TestGetRelevantListForPanama()
		{
			tariff.UE_SPICode = "";

			var country = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "PA");
			var list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, country, new ZDateTime(2012, 3, 14), Factory));

			AssertEquals("SHould contain PA as PA is MPF exempt", true, list.ContainsCode(SpecialProgramList.Codes.PA));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
		}

		public void TestGetRelevantListForPAndPPlus()
		{
			tariff.UE_SPICode = "P AUBHCLJOMAMXP+SG";

			USCCountry hn = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "HN");
			hn.UC_MiscellaneousSPIIndicator = "P";

			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, hn, ZDateTime.Today, Factory, hn));
			AssertEquals("List should have 3 elements including N/A", 3, list.Count);
			AssertEquals("SHould contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
			AssertEquals("SHould contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));

			list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, hn, ZDateTime.Today, Factory));
			AssertEquals("List should have 3 elements including N/A", 3, list.Count);
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("SHould contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
			AssertEquals("SHould contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
		}

		public void TestGetRelevantListForEAsterisk()
		{
			tariff.UE_SPICode = "A E*J P AUBHCACLILJOMAMXP+SG";

			USCCountry vc = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "VC");
			vc.UC_SPICode = "E";

			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, vc, ZDateTime.Today, Factory));
			AssertEquals("List should have 6 elements including N/A", 6, list.Count);
			AssertEquals("SHould contain A", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("SHould contain E", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.E));
			AssertEquals("SHould contain W", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.W));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("SHould contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("SHould contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
		}

		public void TestGetRelevantListForJAsterisk()
		{
			tariff.UE_SPICode = "A E*J*P AUBHCACLILJOMAMXP+SG";

			USCCountry co = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, "EC");
			co.UC_SPICode = "J";
			co.UC_SPIEndDate = ZDateTime.Empty;

			CodeDescriptionPairList list = SPICompleteList.GetRelevantListFor(new SPILine(tariff, co, ZDateTime.Today, Factory));
			AssertEquals("List should have 5 elements including N/A", 5, list.Count);
			AssertEquals("SHould contain A", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("SHould contain J", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.J));
			AssertEquals("Should contain N/A", true, list.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
			AssertEquals("SHould contain P", true, list.ContainsCode(PrimarySpecProgramIndicatorList.Codes.P));
			AssertEquals("SHould contain P+", true, list.ContainsCode(SpecialProgramList.Codes.PPlus));
		}

		public void Test98Or99WithFixedSPI()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98220515";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 9999.99999999m;
			tariff.UE_Column2RateAdValorem = 9999.99999999m;
			tariff.UE_AdditionalTariffNumberIndicator = true;

			tariff.UE_SPICode = "P+";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1701111000";
			invoiceLine.US_UC_NKCountryOfOrigin = "CR";
			invoiceLine.US_UC_NKCountryOfExport = "CR";
			invoiceLine.US_SupTariff = "98220515";

			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode("P+"));
		}

		[TestDate(2009, 6, 1)]
		public void TestSPIFor99117704()
		{
			USCTariff tariff = new USCTariff.Loader(Factory).LoadBestMatch("99117704", ZDateTime.Today);

			IRateWrapper rates = DutyRateWrapper.GetWrapper(ZDateTime.Today, tariff, "CL", "CL", "CL");

			//PreCondition
			AssertNotNull("tariff exists for 99117704", tariff);
			Assert("Tariff has a duty rate of 'Do not declare duty'", rates.IsInvalidDutyRate());

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9911.77.04";
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.JI_Tariff = "0811.10.0050";

			AssertEquals("It should contain 'CL'", true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(SpecialProgramList.Codes.CL));
		}

		[TestDate(2018, 10, 10)]
		public void TestSPIForSection232AndSection201()
		{
			#region Setup Tariff

			var tariff99038001 = new USCTariff.Loader(Factory).LoadBestMatch("99038001", ZDateTime.Today);
			if (tariff99038001 == null)
			{
				tariff99038001 = Factory.New<USCTariff>();
				tariff99038001.UE_Tariff = "99038001";
				tariff99038001.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff99038001.UE_DateTo = ZDateTime.Today.AddYears(1);
			}

			var tariff99038501 = new USCTariff.Loader(Factory).LoadBestMatch("99038501", ZDateTime.Today);
			if (tariff99038501 == null)
			{
				tariff99038501 = Factory.New<USCTariff>();
				tariff99038501.UE_Tariff = "99038501";
				tariff99038501.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff99038501.UE_DateTo = ZDateTime.Today.AddYears(1);
			}
			tariff99038501.UE_SPICode = "E P AUBHCACLCOILJOKRMAMXOMPAPESG";

			var tariff99034501 = new USCTariff.Loader(Factory).LoadBestMatch("99034501", ZDateTime.Today);
			if (tariff99034501 == null)
			{
				tariff99034501 = Factory.New<USCTariff>();
				tariff99034501.UE_Tariff = "99034501";
				tariff99034501.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff99034501.UE_DateTo = ZDateTime.Today.AddYears(1);
			}

			var tariff7606123090 = new USCTariff.Loader(Factory).LoadBestMatch("7606123090", ZDateTime.Today);
			if (tariff7606123090 == null)
			{
				tariff7606123090 = Factory.New<USCTariff>();
				tariff7606123090.UE_Tariff = "7606123090";
				tariff7606123090.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff7606123090.UE_DateTo = ZDateTime.Today.AddYears(1);
			}
			tariff7606123090.UE_SPICode = "A*D E J P AUBHCACLCOILJOKRMAMXOMPAPESG";

			var tariff2403196060 = new USCTariff.Loader(Factory).LoadBestMatch("2403196060", ZDateTime.Today);
			if (tariff2403196060 == null)
			{
				tariff2403196060 = Factory.New<USCTariff>();
				tariff2403196060.UE_Tariff = "2403196060";
				tariff2403196060.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff2403196060.UE_DateTo = ZDateTime.Today.AddYears(1);
			}
			tariff2403196060.UE_SPICode = "D E J P A+BHCLCOKRMAOMPAPESG";

			var gmCountry = Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_ISOCountryCode, "GM"));
			if (gmCountry == null)
			{
				gmCountry = Factory.New<USCCountry>();
				gmCountry.UC_ISOCountryCode = "GM";
			}
			gmCountry.UC_GSPIndicator = true;
			gmCountry.UC_GSPBeginDate = ZDateTime.BrettsBirthday;
			gmCountry.UC_GSPEndDate = ZDateTime.Today.AddYears(1);
			gmCountry.UC_SpecialTradeProgramsIndicator = "D";
			gmCountry.UC_SpecialTradeProgramsBeginDate = ZDateTime.BrettsBirthday;
			gmCountry.UC_SpecialTradeProgramsEndDate = ZDateTime.Today.AddYears(1);

			#endregion

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Gambia;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Gambia;
			invoiceLine.JI_Tariff = "7606123090";
			invoiceLine.US_SupTariff = "";
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			invoiceLine.US_SupTariff = "99038001";
			AssertEquals("For section 232, SPI code 'A' shouldn't be claimed when tariff starts with 990380", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("For section 232, SPI code 'D' shouldn't be claimed when tariff starts with 990380", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));

			invoiceLine.US_SupTariff = "";
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			invoiceLine.US_SupTariff = "99038501";
			AssertEquals("For section 232, SPI code 'A' shouldn't be claimed when tariff starts with 990385", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals("For section 232, SPI code 'D' shouldn't be claimed when tariff starts with 990385", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));

			invoiceLine.JI_Tariff = "2403196060";
			invoiceLine.US_SupTariff = "";
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
			invoiceLine.US_SupTariff = "99034501";
			AssertEquals("For section 201, SPI code 'A' shouldn't be claimed when tariff starts with 990345", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));

			invoiceLine.US_SupTariff = "";
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
			AssertEquals(true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.D));
		}

		[TestDate(2016, 09, 26)]
		public void TestSPIWithTariffApplyInLieuTariffs()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1701991011";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_SPICode = "A*E*J P BHCACLILJOMAMXOMSG";

			var supTariff = Factory.New<USCTariff>();
			supTariff.UE_Tariff = "99031524";
			supTariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			supTariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariffRuleQuery = new ZQuery(USCTariffRuleSchema.U1_RuleCode, TariffRuleList.Codes.InLieuTariffs);
			tariffRuleQuery.AddToFilter(USCTariffRuleSchema.U1_Tariff, "9903");
			var supTariffRule = Factory.LoadTop1<USCTariffRule>(tariffRuleQuery);
			if (supTariffRule == null)
			{
				supTariffRule = Factory.New<USCTariffRule>();
				supTariffRule.U1_RuleCode = TariffRuleList.Codes.InLieuTariffs;
				supTariffRule.U1_Tariff = "9903";
				supTariffRule.U1_DateFrom = new ZDateTime(2000, 1, 1);
			}

			var exceptionRuleQuery = new ZQuery(USCTariffRuleExceptionSchema.U2_Tariff, "990317");
			exceptionRuleQuery.AddToFilter(USCTariffRuleExceptionSchema.U2_TariffTo, "990318");
			var exceptionRule = Factory.LoadTop1<USCTariffRuleException>(exceptionRuleQuery);
			if (exceptionRule == null)
			{
				exceptionRule = Factory.New<USCTariffRuleException>();
				exceptionRule.U2_U1 = supTariffRule.PK;
				exceptionRule.U2_Tariff = "990317";
				exceptionRule.U2_TariffTo = "990318";
				exceptionRule.U2_DateFrom = new ZDateTime(2000, 1, 1);
				exceptionRule.U2_DateTo = new ZDateTime(2099, 12, 31);
			}

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1701991011";
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Jordan;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Jordan;
			invoiceLine.US_SupTariff = "99031524";
			AssertEquals("SPI list doesn't contains 'A'", false, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));

			supTariff.UE_Tariff = "99031724";
			Factory.Save();
			invoiceLine.US_SupTariff = "99031724";
			AssertEquals("SPI list contains 'A'", true, invoiceLine.AddInfoLookups.SPIList.ContainsCode(PrimarySpecProgramIndicatorList.Codes.A));
		}

		public void TestSPIForSupTariffHasUSMCA_REPAIRAttribute()
		{
			var tariffCode6216000500 = "6216000500";
			var tariffCode9802004040 = "9802004040";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariffView6216000500 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, tariffCode6216000500, new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06));
			var tariffView9802004040 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, tariffCode9802004040, new ZDateTime(2020, 12, 29), new ZDateTime(2079, 06, 06));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, UniversalReferenceConstants.TariffAttributeTypes.Values.USMCA_REPAIR, tariffView9802004040);

			var tariff6216000500 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCode6216000500)).OrderBy(x => x.UE_DateFrom).LastOrDefault();
			if (tariff6216000500 == null)
			{
				tariff6216000500 = Factory.New<USCTariff>();
				tariff6216000500.UE_Tariff = tariffCode6216000500;
			}

			var tariff9802004040 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCode9802004040)).OrderBy(x => x.UE_DateFrom).LastOrDefault();
			if (tariff9802004040 == null)
			{
				tariff9802004040 = Factory.New<USCTariff>();
				tariff9802004040.UE_Tariff = tariffCode9802004040;
			}
			tariff9802004040.UE_SPICode = "B C P S S+AUBHCLCOILJOKRMAOMPAPESG";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = tariffCode9802004040;
			invoiceLine.JI_Tariff = tariffCode6216000500;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertCollectionContains(SpecialProgramList.Codes.S, invoiceLine.AddInfoLookups.SPIList.GetAllCodes());
			AssertCollectionContains(SpecialProgramList.Codes.SPlus, invoiceLine.AddInfoLookups.SPIList.GetAllCodes());
			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			AssertNoMessageErrors(invoiceLine.US_SPIInfo);
			invoiceLine.US_SPI = SpecialProgramList.Codes.SPlus;
			AssertNoMessageErrors(invoiceLine.US_SPIInfo);

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Russia;
			var rateIndicatorForRU = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, Core.Constants.CountryCodes.Russia)?.GetRateIndicator(ZDateTime.Now.Date) ?? ZString.Empty;
			var shouldContainSAndSPlus = rateIndicatorForRU != "2";
			AssertEquals(shouldContainSAndSPlus, invoiceLine.AddInfoLookups.SPIList.GetAllCodes().Contains(SpecialProgramList.Codes.S));
			AssertEquals(shouldContainSAndSPlus, invoiceLine.AddInfoLookups.SPIList.GetAllCodes().Contains(SpecialProgramList.Codes.SPlus));

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Turkey;
			AssertCollectionContains(SpecialProgramList.Codes.S, invoiceLine.AddInfoLookups.SPIList.GetAllCodes());
			AssertCollectionContains(SpecialProgramList.Codes.SPlus, invoiceLine.AddInfoLookups.SPIList.GetAllCodes());
			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			AssertNoMessageErrors(invoiceLine.US_SPIInfo);
			invoiceLine.US_SPI = SpecialProgramList.Codes.SPlus;
			AssertNoMessageErrors(invoiceLine.US_SPIInfo);
		}

		USCTariff tariff;

		protected override void SetUp()
		{
			base.SetUp();
			tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
		}

		void AssertSPIList(JobComInvoiceLine invoiceLine)
		{
			AssertEquals(2, invoiceLine.AddInfoLookups.SPIList.Count);
			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode("AU"));
			Assert(invoiceLine.AddInfoLookups.SPIList.ContainsCode(SPICompleteList.MoreCodes.NotApplicable));
		}

		JobDeclaration CreateWatchAssemblyDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "REPAIRED WATCH";
				invoiceHeader.JZ_InvoiceAmount = 9426m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				invoiceHeader.US_UC_NKCountryOfOrigin = "AU";
				invoiceHeader.US_UC_NKCountryOfExport = "HK";
				invoiceHeader.JZ_IncoTerm = "FOB";

				var invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;
				invoiceLine1.US_UC_NKCountryOfOrigin = "AU";
				invoiceLine1.US_UC_NKCountryOfExport = "HK";

				var childLine1 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine1.US_SupTariff = "9802008068";
				childLine1.US_98GoodsValue = 1010m;
				childLine1.JI_Tariff = "9102111020";
				childLine1.JI_InvoiceQuantity = 1000m;
				childLine1.JI_InvoiceUQ = "NO";
				childLine1.JI_CustomsQuantity = 1000m;
				childLine1.JI_CustomsUnitQty = "NO";
				childLine1.JI_LinePrice = 1609m;

				var childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802008068";
				childLine2.JI_Tariff = "9102111030";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1345m;

				var childLine3 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine3.US_SupTariff = "9802008068";
				childLine3.US_98GoodsValue = 204m;
				childLine3.JI_Tariff = "9102111040";
				childLine3.JI_InvoiceQuantity = 1000m;
				childLine3.JI_InvoiceUQ = "NO";
				childLine3.JI_CustomsQuantity = 1000m;
				childLine3.JI_CustomsUnitQty = "NO";
				childLine3.JI_LinePrice = 0m;
			}
			return declaration;
		}
		sealed class SPILine : ISPILine
		{
			public SPILine(USCTariff tariff, USCCountry country, ZDateTime effectiveDate, BusinessObjectFactory factory, USCCountry countryOfExport = null)
				: this(tariff, country, effectiveDate, factory, null, new List<SPILine>(), countryOfExport)
			{
			}

			public SPILine(USCTariff tariff, USCCountry country, ZDateTime effectiveDate, BusinessObjectFactory factory, SPILine parentLine, List<SPILine> childLines, USCCountry countryOfExport)
			{
				this.parentLine = parentLine;
				this.childLines = childLines;
				this.tariff = tariff;
				this.country = country;
				this.factory = factory;
				this.effectiveDate = effectiveDate;
				this.countryOfExport = countryOfExport;
			}

			readonly SPILine parentLine;
			readonly List<SPILine> childLines;
			readonly ZDateTime effectiveDate;
			readonly USCTariff tariff;
			readonly USCCountry country;
			readonly USCCountry countryOfExport;
			readonly BusinessObjectFactory factory;

			#region ISPILine Members

			public ZDateTime EffectiveDate
			{
				get { return effectiveDate; }
			}

			public USCTariff ImportTariff
			{
				get { return tariff; }
			}

			public ZString ImportTariffCode
			{
				get { return tariff != null ? tariff.UE_Tariff : ZString.Empty; }
			}

			public USCCountry CountryOfOrigin
			{
				get { return country; }
			}

			public USCCountry CountryOfExport => countryOfExport;

			public ZString CountryOfOriginCode
			{
				get { return country != null ? country.UC_Code : ZString.Empty; }
			}
			public ZString CountryOfExportCode
			{
				get { return countryOfExport != null ? countryOfExport.UC_Code : ZString.Empty; }
			}

			public BusinessObjectFactory Factory
			{
				get { return factory; }
			}

			public ISPILine ParentTariffLine
			{
				get { return parentLine; }
			}

			public IEnumerable<ISPILine> SecondaryTariffLines
			{
				get { return new TypedEnumerable<ISPILine>(childLines); }
			}

			public bool HasSecondaryChildrenLines
			{
				get { return childLines.Count > 0; }
			}

			#endregion
		}
	}
}
