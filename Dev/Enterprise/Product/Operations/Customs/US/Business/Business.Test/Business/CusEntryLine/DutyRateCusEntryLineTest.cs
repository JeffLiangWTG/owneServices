using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DutyRateCusEntryLineTest : TestCaseWithFactory
	{
		[TestDate(2008, 1, 1)]
		public void TestDutyRateWith99()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "2106.90.9700";
			invoiceLine.US_SupTariff = "9904.17.56";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_LinePrice = 260m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals("DutyRate for SupTariffLine", DutyResult.DutyFreeString, supplementaryTariff.CL_DutyPercentAsString);
			AssertEquals("DutyRate", "28.8c/KG + 8.5%", classificationTariff.CL_DutyPercentAsString);
		}

		[TestDate(2008, 1, 1)]
		public void TestDutyRateWith99_SugarCompCodeJ()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "1701.11.5000";
			invoiceLine.US_SupTariff = "9904.17.06";
			invoiceLine.JI_CustomsQuantity = 925.90m;
			invoiceLine.JI_LinePrice = 925.90m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals("DutyRate for SupTariffLine", DutyResult.DutyFreeString, supplementaryTariff.CL_DutyPercentAsString);
			AssertEquals("DutyRate", "33.87c/KG", classificationTariff.CL_DutyPercentAsString);
		}

		public void TestRepairLine()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "8410.11.0000";
			invoiceLine.US_SupTariff = "9802.00.4040";
			invoiceLine.JI_CustomsQuantity = 2m;
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_98GoodsValue = 3700.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine classificationTariff = invoiceLine.CusEntryLine;
			CusEntryLine supplementaryTariff = invoiceLine.CusEntryLine.ParentLine;
			AssertEquals("DutyRate for SupTariffLine", DutyResult.DutyFreeString, supplementaryTariff.CL_DutyPercentAsString);
			AssertEquals("DutyRate", "3.8%", classificationTariff.CL_DutyPercentAsString);
		}

		public void TestDerivedDutyCalculation()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "8211.10.0000";

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "8205.20.3000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			CusEntryLine entryLine2 = invoiceLine2.CusEntryLine;

			AssertEquals("DutyRate", "6.2%", entryLine1.CL_DutyPercentAsString);
			AssertEquals("DutyRate", DutyResult.DutyFreeString, entryLine2.CL_DutyPercentAsString);
		}

		public void TestTobaccoTaxRate()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "2402.10.3030";
			invoiceLine.JI_CustomsQuantity = 85m;
			invoiceLine.JI_CustomsSecondQuantity = 85m;
			invoiceLine.JI_LinePrice = 3166.67m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;

			AssertEquals("DutyRate", "$1.89/KG + 4.7%", entryLine1.CL_DutyPercentAsString);
			AssertEquals("Tax rate", "$1.828/K", invoiceLine.US_TaxRateS);

			var linePrint = new ACSEntryHeaderENS7501Line(entryLine1, false, false);
			AssertEquals("Tax rate", "$1.828/K", linePrint.LineFeePercentAsString);
		}

		public void TestTaxRateOverridden()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "2208.70.0030";
			invoiceLine.JI_CustomsQuantity = 3921m;
			invoiceLine.JI_LinePrice = 14000.00m;

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 2.2084779m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;

			AssertEquals("DutyRate: Comp Code 1, but no specific rate exists", DutyResult.DutyFreeString, entryLine1.CL_DutyPercentAsString);

			var linePrint = new ACSEntryHeaderENS7501Line(entryLine1, false, false);
			AssertEquals("Tax rate", "$2.2084779/PFL", linePrint.LineFeePercentAsString);
		}

		public void TestManyFees()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "0409.00.0044";
			invoiceLine.JI_CustomsQuantity = 19294m;
			invoiceLine.JI_LinePrice = 56540.24m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1007.00.0020";
			AssertNotNull(invoiceLine2.ImportTariff);

			if (!invoiceLine2.ImportTariff.GetRequiredFeeCodes().Any())
			{
				var dutyRate = invoiceLine2.ImportTariff.DutyRates.AddNew();
				dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Sorghum;
				dutyRate.UD_TaxFeeFlag = "1";
				dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.AdValorem;
				dutyRate.UD_TaxFeeAdvalorem = 0.006m;
			}

			invoiceLine2.JI_CustomsQuantity = 300m;
			invoiceLine2.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			CusEntryLine entryLine2 = invoiceLine2.CusEntryLine;

			AssertEquals("DutyRate EntryLine1", "1.9c/KG", entryLine1.CL_DutyPercentAsString);
			AssertEquals("DutyRate EntryLine2", "0.22c/KG", entryLine2.CL_DutyPercentAsString);

			var linePrint = new ACSEntryHeaderENS7501Line(entryLine1, false, false);
			AssertEquals("Fee rate", "2.2c/KG", linePrint.LineFeePercentAsString);

			linePrint = new ACSEntryHeaderENS7501Line(entryLine2, false, false);
			AssertEquals("Fee rate for Sorghum fee", "0.6%", linePrint.LineFeePercentAsString);
		}

		public void TestBeefFee()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "0201.10.0510";
			invoiceLine.JI_CustomsQuantity = 3000m;
			invoiceLine.JI_LinePrice = 37000.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;

			AssertEquals("DutyRate", "4.4c/KG", entryLine1.CL_DutyPercentAsString);

			var linePrint = new ACSEntryHeaderENS7501Line(entryLine1, false, false);
			AssertEquals("Fee rate", "1.459542c/KG", linePrint.LineFeePercentAsString);

			// changes to Duty Free
			invoiceLine.JI_Tariff = "3301.29.5125";
			invoiceLine.JI_CustomsQuantity = 100m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine1 = invoiceLine.CusEntryLine;
			AssertEquals("DutyRate should have been cleared", DutyResult.DutyFreeString, entryLine1.US_DutyRateDesc);
		}

		public void TestTaxDeferredWithSelectedRateType()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "2204.21.2000";
			invoiceLine.JI_CustomsQuantity = 28.35000m;
			invoiceLine.JI_LinePrice = 223.50m;

			invoiceLine.US_TaxRateT = RateTypeList.Codes.Secondary;

			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;

			AssertEquals("DutyRate", "19.8c/L", entryLine1.CL_DutyPercentAsString);
			AssertEquals("Tax rate", "87.1761c/L", invoiceLine.US_TaxRateS);

			var linePrint = new ACSEntryHeaderENS7501Line(entryLine1, false, false);
			AssertEquals("Tax rate when deferred", "DEF 87.1761c/L", linePrint.LineFeePercentAsString);
		}

		public void TestUseLocalCacheForParentLine()
		{
			var declaration = GetDeclaration();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			AssertNull(entryLine.ParentLine);

			var parentEntryLine = entry.AllEntryLines.AddNew();
			entryLine.US_CL_ParentLine = parentEntryLine.PK;
			AssertNotNull(entryLine.ParentLine);
			AssertEquals(parentEntryLine.PK, entryLine.ParentLine.PK);

			entry.AllEntryLines.RemoveAndDelete(parentEntryLine);
			AssertNull(entryLine.ParentLine);

			parentEntryLine = entry.AllEntryLines.AddNew();
			entryLine.US_CL_ParentLine = parentEntryLine.PK;
			AssertNotNull(entryLine.ParentLine);
			AssertEquals(parentEntryLine.PK, entryLine.ParentLine.PK);
		}

		public void Test9813TIBEntry()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "8711.30.0090";
			invoiceLine.US_SupTariff = "9813.00.35";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_LinePrice = 1500.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			CusEntryLine supEntryLine = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals("DutyRate", DutyResult.DutyFreeString, entryLine1.CL_DutyPercentAsString);
			AssertEquals("DutyRate", DutyResult.DutyFreeString, supEntryLine.CL_DutyPercentAsString);

			var linePrint = new ACSEntryHeaderENS7501Line(entryLine1, false, false);
			AssertEquals("Duty Rate when printed", DutyResult.DutyFreeString, linePrint.DutyPercentAsString);

			linePrint = new ACSEntryHeaderENS7501Line(supEntryLine, false, false);
			AssertEquals("Duty Rate when printed", DutyResult.DutyFreeString, linePrint.DutyPercentAsString);
		}

		public void TestWatchAssembly()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "9102.11.1010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.US_98GoodsValue = 1852.00m;

			Assert("PreCondition: Secondary Lines are added automatically", invoiceLine.IsParentLine);

			invoiceLine.ChildLines.ElementAt(0).JI_LinePrice = 1609.00m;
			invoiceLine.ChildLines.ElementAt(0).US_98GoodsValue = 1010.00m;
			invoiceLine.ChildLines.ElementAt(1).JI_LinePrice = 1345.00m;
			invoiceLine.ChildLines.ElementAt(2).US_98GoodsValue = 204.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			CusEntryLine supEntryLine1 = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals("", entryLine1.US_DutyRateDesc);
			AssertEquals(DutyResult.DutyFreeString, supEntryLine1.US_DutyRateDesc);

			var linePrint = new ACSEntryHeaderENS7501Line(supEntryLine1, false, false);
			AssertEquals(DutyResult.DutyFreeString, linePrint.DutyPercentAsString);
			AssertEquals("44c/NO", linePrint.SecondaryLine1DutyPercentAsString);
			AssertEquals(DutyResult.DutyFreeString, linePrint.SecondaryLine2DutyPercentAsString);
			AssertEquals("7.09%", linePrint.SecondaryLine3DutyPercentAsString);
			AssertEquals(DutyResult.DutyFreeString, linePrint.SecondaryLine4DutyPercentAsString);
			AssertEquals("7.09%", linePrint.SecondaryLine5DutyPercentAsString);
			AssertEquals(DutyResult.DutyFreeString, linePrint.SecondaryLine6DutyPercentAsString);
			AssertEquals("7.09%", linePrint.SecondaryLine7DutyPercentAsString);
		}

		public void TestWatchRepair()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "9102.11.1010";
			invoiceLine.US_SupTariff = "9802.00.4040";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.US_98GoodsValue = 1852.00m;

			Assert("PreCondition: Secondary Lines are added automatically", invoiceLine.IsParentLine);

			invoiceLine.ChildLines.ElementAt(0).JI_LinePrice = 1609.00m;
			invoiceLine.ChildLines.ElementAt(0).US_98GoodsValue = 1010.00m;
			invoiceLine.ChildLines.ElementAt(1).JI_LinePrice = 1345.00m;
			invoiceLine.ChildLines.ElementAt(2).US_98GoodsValue = 204.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			CusEntryLine supEntryLine1 = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(DutyResult.DutyFreeString, supEntryLine1.US_DutyRateDesc);
			AssertEquals("8.447%", entryLine1.US_DutyRateDesc);

			var linePrint = new ACSEntryHeaderENS7501Line(supEntryLine1, false, false);
			AssertEquals(DutyResult.DutyFreeString, linePrint.DutyPercentAsString);
			AssertEquals("8.447%", linePrint.SecondaryLine1DutyPercentAsString);
			AssertEquals(DutyResult.DutyFreeString, linePrint.SecondaryLine2DutyPercentAsString);
			AssertEquals("8.447%", linePrint.SecondaryLine3DutyPercentAsString);
			AssertEquals(DutyResult.DutyFreeString, linePrint.SecondaryLine4DutyPercentAsString);
			AssertEquals("8.447%", linePrint.SecondaryLine5DutyPercentAsString);
			AssertEquals(DutyResult.DutyFreeString, linePrint.SecondaryLine6DutyPercentAsString);
			AssertEquals("8.447%", linePrint.SecondaryLine7DutyPercentAsString);
		}

		public void TestICPSCDataMembers()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9801009000";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "CP2";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.USCPSC, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				var declaration = GetDeclaration();
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				var invoiceLine = declaration.InvoiceLines[0];
				invoiceLine.JI_Tariff = "0000009000";
				invoiceLine.US_SupTariff = "9801009000";
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
				var header = invoiceLine.CPSCHeaders.AddNew();
				header.US_ProcessingCode = CPSCProcessingCodeList.Codes.REF;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var entryLine = invoiceLine.CusEntryLine;
				var supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);

				IGovernmentAgencies pga = entryLine;
				IGovernmentAgencies supPGA = supEntryLine;
				AssertEquals("CPSCIndicator", "", pga.CPSCIndicator);
				AssertEquals("CPSCIndicator", OGAIndicatorList.Codes.Declared, supPGA.CPSCIndicator);

				var refHeaders = pga.CPSCHeaders;
				AssertEquals("CPSCHeaders", 0, refHeaders.Count());
				refHeaders = supPGA.CPSCHeaders;
				AssertEquals("CPSCHeaders", 1, refHeaders.Count());

				invoiceLine.US_SupTariff = "10";
				invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				entryLine = invoiceLine.CusEntryLine;
				supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);
				pga = entryLine;
				supPGA = supEntryLine;
				AssertEquals("CPSCIndicator", OGAIndicatorList.Codes.Declared, pga.CPSCIndicator);
				AssertEquals("CPSCIndicator", ZString.Empty, supPGA.CPSCIndicator);
				refHeaders = pga.CPSCHeaders;
				AssertEquals("CPSCHeaders", 1, refHeaders.Count());
				refHeaders = supPGA.CPSCHeaders;
				AssertEquals("CPSCHeaders", 0, refHeaders.Count());
			}
		}

		public void TestIAPHISDataMembers()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9801009000";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "AQ2";

			var declaration = GetDeclaration();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = "0000009000";
			invoiceLine.US_SupTariff = "9801009000";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			var header = invoiceLine.APHISHeaders.AddNew();
			header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			var supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);

			IGovernmentAgencies pga = entryLine;
			IGovernmentAgencies supPGA = supEntryLine;
			AssertEquals("APHISIndicator", "", pga.APHISIndicator);
			AssertEquals("APHISIndicator", OGAIndicatorList.Codes.Declared, supPGA.APHISIndicator);

			var avsHeaders = pga.APHISHeaders;
			AssertEquals("APHISHeaders", 0, avsHeaders.Count());
			avsHeaders = supPGA.APHISHeaders;
			AssertEquals("APHISHeaders", 1, avsHeaders.Count());

			invoiceLine.US_SupTariff = "10";
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
			supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);
			pga = entryLine;
			supPGA = supEntryLine;
			AssertEquals("APHISIndicator", OGAIndicatorList.Codes.Declared, pga.APHISIndicator);
			AssertEquals("APHISIndicator", ZString.Empty, supPGA.APHISIndicator);
			avsHeaders = pga.APHISHeaders;
			AssertEquals("APHISHeaders", 1, avsHeaders.Count());
			avsHeaders = supPGA.APHISHeaders;
			AssertEquals("APHISHeaders", 0, avsHeaders.Count());
		}

		public void TestIDDTCDataMembers()
		{
			var declaration = GetDeclaration();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = "9301903020";
			invoiceLine.US_DDTCInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_DDTCLicenseType = DDTCLicenseTypeCodes.Codes.S61;
			invoiceLine.US_DDTCLicenseNo = "S6123122";
			invoiceLine.US_DDTCRegistrationNo = "REG324";
			invoiceLine.US_DDTCExemptionCode = ACEDDTCExemptionCodes.Codes._12318A2;
			invoiceLine.US_DDTCArrivalDate = new ZDateTime(2015, 3, 28);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;

			IGovernmentAgencies oga = entryLine;
			var ddtcData = oga.DDTCData;
			AssertEquals("LicenseType", DDTCLicenseTypeCodes.Codes.S61, ddtcData.LicenseType);
			AssertEquals("LicenseNumber", "S6123122", ddtcData.LicenseNumber);
			AssertEquals("RegistrationNumber", "REG324", ddtcData.RegistrationNumber);
			AssertEquals("RegistrationNumber", ACEDDTCExemptionCodes.Codes._12318A2, ddtcData.ExemptionCode);
			AssertEquals("AnticipatedArrivalDate", new ZDateTime(2015, 3, 28), ddtcData.AnticipatedArrivalDate);

			invoiceLine.US_DDTCInd = ZString.Empty;
			AssertNotNull("DDTC Data", oga.DDTCData);
		}

		public void TestLicenseNumberlength()
		{
			var declaration = GetDeclaration();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = "9301903020";
			invoiceLine.US_LicenseNo = "12345678901234";
			invoiceLine.US_DDTCArrivalDate = new ZDateTime(2015, 3, 28);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;

			AssertEquals("LicenseNumber for AES message ExportLicenseNumberCFRCitationAuthorizationSymbolKCPACM max length is 14", "12345678901234", entryLine.LicenseNumber);
		}

		public void TestIFWSDataMembers()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9801009000";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "FW2";
			using (ZZCustomsFunctionality.TemporarilySetupFWSEffective())
			{
				var declaration = GetDeclaration();
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EntryFilerCode = "XJ5";
				var invoiceLine = declaration.InvoiceLines[0];
				invoiceLine.JI_Tariff = "0000009000";
				invoiceLine.US_SupTariff = "9801009000";
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				var header = invoiceLine.FWSHeaders.AddNew();
				header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var entryLine = invoiceLine.CusEntryLine;
				var supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);

				IGovernmentAgencies pga = entryLine;
				IGovernmentAgencies supPGA = supEntryLine;
				AssertEquals("FWSIndicator", "", pga.FWSIndicator);
				AssertEquals("FWSIndicator", OGAIndicatorList.Codes.Declared, supPGA.FWSIndicator);

				var fwsHeaders = pga.FWSHeaders;
				AssertEquals("FWSHeaders", 0, fwsHeaders.Count());
				fwsHeaders = supPGA.FWSHeaders;
				AssertEquals("FWSHeaders", 1, fwsHeaders.Count());

				invoiceLine.US_SupTariff = "10";
				invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				entryLine = invoiceLine.CusEntryLine;
				supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);
				pga = entryLine;
				supPGA = supEntryLine;
				AssertEquals("FWSIndicator", OGAIndicatorList.Codes.Declared, pga.FWSIndicator);
				AssertEquals("FWSIndicator", ZString.Empty, supPGA.FWSIndicator);
				fwsHeaders = pga.FWSHeaders;
				AssertEquals("FWSHeaders", 1, fwsHeaders.Count());
				fwsHeaders = supPGA.FWSHeaders;
				AssertEquals("FWSHeaders", 0, fwsHeaders.Count());
			}
		}

		public void TestIDEADataMembers()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9801009000";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_PGACodes = "DE1";
			var declaration = GetDeclaration();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_Tariff = "0000009000";
			invoiceLine.US_SupTariff = "9801009000";
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var header = invoiceLine.DEAHeaders.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = invoiceLine.CusEntryLine;
			var supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);

			IGovernmentAgencies pga = entryLine;
			IGovernmentAgencies supPGA = supEntryLine;
			AssertEquals("DEAIndicator", "", pga.DEAIndicator);
			AssertEquals("DEAIndicator", OGAIndicatorList.Codes.Declared, supPGA.DEAIndicator);

			var refHeaders = pga.DEAHeaders;
			AssertEquals("DEAHeaders", 0, refHeaders.Count());
			refHeaders = supPGA.DEAHeaders;
			AssertEquals("DEAHeaders", 1, refHeaders.Count());

			invoiceLine.US_SupTariff = "10";
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryLine = invoiceLine.CusEntryLine;
			supEntryLine = invoiceLine.GetEntryLineFor(entryLine.Header.CH_MessageType, true);
			pga = entryLine;
			supPGA = supEntryLine;
			AssertEquals("DEAIndicator", OGAIndicatorList.Codes.Declared, pga.DEAIndicator);
			AssertEquals("DEAIndicator", ZString.Empty, supPGA.DEAIndicator);
			refHeaders = pga.DEAHeaders;
			AssertEquals("DEAHeaders", 1, refHeaders.Count());
			refHeaders = supPGA.DEAHeaders;
			AssertEquals("DEAHeaders", 0, refHeaders.Count());
		}

		public void TestWatchRepairWithSPI()
		{
			var declaration = GetDeclaration();
			var invoiceLine = declaration.InvoiceLines[0];

			invoiceLine.JI_Tariff = "9102.11.1010";
			invoiceLine.US_SupTariff = "9802.00.4040";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 3406.00m;
			invoiceLine.US_98GoodsValue = 1852.00m;
			invoiceLine.US_SPI = "CA";

			Assert("PreCondition: Secondary Lines are added automatically", invoiceLine.IsParentLine);

			invoiceLine.ChildLines.ElementAt(0).JI_LinePrice = 1609.00m;
			invoiceLine.ChildLines.ElementAt(0).US_98GoodsValue = 1010.00m;
			AssertEquals("PreCondition", "CA", invoiceLine.ChildLines.ElementAt(0).US_SPI);

			invoiceLine.ChildLines.ElementAt(1).JI_LinePrice = 1345.00m;
			AssertEquals("PreCondition", "CA", invoiceLine.ChildLines.ElementAt(1).US_SPI);

			invoiceLine.ChildLines.ElementAt(2).US_98GoodsValue = 204.00m;
			AssertEquals("PreCondition", "CA", invoiceLine.ChildLines.ElementAt(2).US_SPI);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryLine entryLine1 = invoiceLine.CusEntryLine;
			CusEntryLine supEntryLine1 = invoiceLine.CusEntryLine.ParentLine;

			AssertEquals(DutyResult.DutyFreeString, supEntryLine1.US_DutyRateDesc);
			AssertEquals(DutyResult.DutyFreeString, entryLine1.US_DutyRateDesc);
		}

		public void TestTSCAContactDetails()
		{
			var declaration = GetDeclaration();
			var invoice = declaration.Invoices[0];
			invoice.US_FDAContactName = "BROKER NAME";
			invoice.US_FDAContactPhoneNo = "2345678";
			invoice.US_FDAContactEmail = "broker@abc.com";

			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = ((IGovernmentAgencies)invoiceLine.CusEntryLine).EPA_TSCAData;
			AssertEquals("BROKER NAME", entryLine.ContactName);
			AssertEquals("2345678", entryLine.ContactPhone);
			AssertEquals("broker@abc.com", entryLine.ContactEmail);

			invoiceLine.US_FDAContactName = "TEST NAME";
			invoiceLine.US_FDAContactPhoneNo = "11223344";
			invoiceLine.US_FDAContactEmail = "test@abc.com";
			AssertEquals("TEST NAME", entryLine.ContactName);
			AssertEquals("11223344", entryLine.ContactPhone);
			AssertEquals("test@abc.com", entryLine.ContactEmail);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			return declaration;
		}
	}
}
