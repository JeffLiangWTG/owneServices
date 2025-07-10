using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CottonFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2008, 9, 11)]
		public void TestCottonFeeExempt()
		{
			var tariffRules = new USCTariffRule.Loader(Factory).LoadDuplicates(TariffRuleList.Codes.CottonFeeExemption, "98020040", "98020040", ZDateTime.Today, ZDateTime.Empty);
			if (tariffRules.Length == 0)
			{
				var tariffRule = Factory.New<USCTariffRule>();
				tariffRule.U1_RuleCode = TariffRuleList.Codes.CottonFeeExemption;
				tariffRule.U1_Tariff = "98020040";
				tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			}

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206900040"; // Cotton
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(invoiceLine.CusEntryLine.CottonAmount > 0);

			invoiceLine.US_SupTariff = "9802004020";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, invoiceLine.CusEntryLine.CottonAmount);

			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			Assert(!invoiceLine.US_CottonFeeExemptInfo.HasMessageError(FormalImportAddInfoJobComInvoiceLineValidation.CottonFeeApplicable));
		}

		[TestDate(2008, 9, 11)]
		public void TestCottonFee()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206900040"; // Cotton
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			AssertEquals(2.06m, new CottonFeeCalculator().CalculateFee(invoiceLine).Amount);

			invoiceLine.JI_CustomsSecondQuantity = 500m;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("no Cotton on entry line level as it is less than threshold amount", 0m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateWithRoundedValue()
		{
			var tariff = new USCTariff.Loader(Factory).LoadBestMatch("6203424016", ZDateTime.Today);
			foreach (USCTariffDutyRate dutyRate in tariff.DutyRates)
			{
				if (dutyRate.UD_TaxFeeClassCode == Core.Constants.USCustoms.FeeCodes.Cotton)
				{
					dutyRate.UD_TaxFeeSpecificRate = 0.0108380m;
					break;
				}
			}
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6203424016";

			invoiceLine.JI_CustomsQuantity = 500m;
			invoiceLine.JI_CustomsSecondQuantity = 184.97m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2.01m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CottonFee);
		}

		public void TestBelowMinimumCottonFeeOnInvoiceLineButGreaterThanMinimumOnEntryLines()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206900040"; // Cotton
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsSecondQuantity = 500m;//just enough so that alone, it is less than 2, but if merged and aggregated, it is over 2 
			FeeResult fee = new CottonFeeCalculator().CalculateFee(invoiceLine);
			Assert(fee.Amount > 0m);//1.03
			AssertEquals(true, fee.IsRequired);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6206900040"; // Cotton
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.JI_CustomsSecondQuantity = 500m;//just enough so that alone, it is less than 2, but if merged and aggregated, it is over 2 
			FeeResult fee2 = new CottonFeeCalculator().CalculateFee(invoiceLine2);
			Assert(fee2.Amount > 0m);//1.03
			AssertEquals(true, fee2.IsRequired);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition:One entry generated", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("PreCondition:One entry line generated", 1, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertNotNull("Cotton Fee is there for the merged entry line", declaration.CustomsEntryHeaders[0].MergedLines[0].Fees.GetElementWithThisCode(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		public void TestExemptCotton()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6206900040"; // Cotton
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsSecondQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(invoiceLine.CusEntryLine.CottonAmount > 0m);

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Exempt", 0m, invoiceLine.CusEntryLine.CottonAmount);

			invoiceLine.US_CottonFeeExempt = ZString.Empty;
			invoiceLine.US_CottonCertificateNo = "324789";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("AMS Fee Exempt - organic certificate", 0m, invoiceLine.CusEntryLine.CottonAmount);
		}

		[TestDate(2009, 1, 1)]
		public void TestCottonFeeIsExemptForSecondarySPI_H()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			AssertNotNull("Cotton fee is usually applicable", invoiceLine.ImportTariff.DutyRates.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine.JI_CustomsQuantity = 10000M;
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsSecondQuantity = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(0m, invoiceLine.CusEntryLine.CottonAmount);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("becomes exempt", 0m, invoiceLine.CusEntryLine.CottonAmount);
		}

		public void TestCottonFeeIsExemptForAGOGTextileClaims()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008042";
			invoiceLine.JI_Tariff = "5201002800";
			AssertNotNull("Cotton fee is usually applicable", invoiceLine.ImportTariff.DutyRates.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine.JI_CustomsQuantity = 10000M;
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsSecondQuantity = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNull("No cotton fee as parent indicates it is exempt", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));

			JobComInvoiceLine standAloneLine = declaration.InvoiceLines.AddNew();
			standAloneLine.JI_ParentID = ZGuid.Empty;
			standAloneLine.JI_LinePrice = 15000m;
			standAloneLine.JI_Tariff = "5201002800";
			standAloneLine.JI_CustomsQuantity = 10000M;
			AssertNotNull("Cotton fee is usually applicable", standAloneLine.ImportTariff.DutyRates.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.Cotton));

			standAloneLine.JI_CustomsSecondQuantity = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotNull("Cotton fee is calculated", standAloneLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		[TestDate(2009, 1, 1)]
		public void TestCottonFeeCalculationForEnsemble()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0040";
			invoiceLine.JI_CustomsQuantity = 375.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 415m;//KG

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6104.62.2028";
			invoiceLine2.JI_CustomsQuantity = 375.00000m;
			invoiceLine2.JI_CustomsSecondQuantity = 415m;//KG

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull("Cotton fee should have been calculated", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertNotNull("Cotton fee have been calculated for secondary line of ensemble parent as the second line is ACS.", invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));

			AssertEquals("415KG * (0.00888900 + 0.00869500)", 7.30m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CottonFee);
		}

		[TestDate(2009, 1, 1)]
		public void TestCottonFeeCalculationForXAndVLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9404908020";
			invoiceLine.JI_CustomsQuantity = 375.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 415m;//KG
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			AssertNotNull(invoiceLine.ImportTariff);
			var dutyRate = invoiceLine.ImportTariff.DutyRates[0];
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Cotton;
			dutyRate.UD_TaxFeeComputationCode = "2";
			dutyRate.UD_TaxFeeFlag = "1";

			Assert(invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "9404908020";
			invoiceLine2.JI_CustomsQuantity = 375.00000m;
			invoiceLine2.JI_CustomsSecondQuantity = 415m;//KG
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine3.JI_Tariff = "9404901000";
			invoiceLine3.JI_CustomsQuantity = 375.00000m;
			invoiceLine3.JI_CustomsSecondQuantity = 415m;//KG
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine4.JI_Tariff = "6304920000";
			invoiceLine4.JI_CustomsQuantity = 375.00000m;
			invoiceLine4.JI_CustomsSecondQuantity = 415m;//KG
			invoiceLine4.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var invoiceLine5 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine5.JI_Tariff = "6303910020";
			invoiceLine5.JI_CustomsQuantity = 375.00000m;
			invoiceLine5.JI_CustomsSecondQuantity = 415m;//KG
			invoiceLine5.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNull("Cotton fee should NOT have been calculated for x lines", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertNotEquals("Cotton fee should have been calculated for V lines", 0m, invoiceLine5.CusEntryLine.CottonAmount);
		}

		[TestDate(2009, 1, 1)]
		public void TestCottonFeeCalculationForEnsembleWhenParentDoesNotAttractCotton()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0010";
			invoiceLine.JI_CustomsQuantity = 400.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 725.00m;//KG

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6102.20.0020";
			invoiceLine2.JI_CustomsQuantity = 400.00000m;
			invoiceLine2.JI_CustomsSecondQuantity = 725.00m;//KG

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNull("Cotton fee is not applicable for parent", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertNotNull("Cotton fee is applicable because ensemble parent does not attract cotton fee", invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));

			AssertEquals("Cotton fee", 7.23m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CottonFee);
		}

		[TestDate(2009, 1, 1)]
		public void TestCottonFeeCalculationForEnsembleIsNotDoneWhenTIBEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0040";
			invoiceLine.JI_CustomsQuantity = 375.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 415m;//KG

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6104.62.2028";
			invoiceLine2.JI_CustomsQuantity = 375.00000m;
			invoiceLine2.JI_CustomsSecondQuantity = 415m;//KG

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNull("Cotton fee should NOT have been calculated for TIB entry", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertNull("Cotton fee should NOT have been calculated for secondary line of ensemble parent", invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("No Fee payable for TIB", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CottonFee);
		}

		[TestDate(2009, 1, 1)]
		public void TestCottonFeeCalculationThatWouldBePayableForEnsembleWhenTIBEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0040";
			invoiceLine.JI_CustomsQuantity = 375.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 415m;//KG

			JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6104.62.2028";
			invoiceLine2.JI_CustomsQuantity = 375.00000m;
			invoiceLine2.JI_CustomsSecondQuantity = 415m;//KG

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNull("Cotton fee should NOT have been calculated for TIB entry", invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertNull("Cotton fee should NOT have been calculated for secondary line of ensemble parent", invoiceLine2.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));

			AssertEquals("No Fee payable for TIB", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CottonFee);
			AssertEquals("Fee that would be payable if not a TIB entry - 415KG * 0.00888900", 3.69m, new CottonFeeCalculator(true, false).CalculateFee(invoiceLine).Amount);
		}

		[TestDate(2012, 1, 2)]
		public void TestCottonFeeCalculatedOnParentRateWhenNotExempt()
		{
			var uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "5810929080";
			uscTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff.UE_DateTo = new ZDateTime(2012, 12, 31);
			uscTariff.UE_Unit1 = "KG";
			uscTariff.UE_DutyComputationCode = "7";
			uscTariff.UE_Column1RateAdValorem = 0.074m;
			uscTariff.UE_Column2RateAdValorem = 0.9m;

			var uscTariffDutyRate = uscTariff.DutyRates.AddNew();
			uscTariffDutyRate.UD_DutyElement = "5";
			uscTariffDutyRate.UD_TaxFeeClassCode = "056";
			uscTariffDutyRate.UD_TaxFeeComputationCode = "1";
			uscTariffDutyRate.UD_TaxFeeFlag = "1";
			uscTariffDutyRate.UD_TaxFeeSpecificRate = 0.002749m;

			uscTariff = Factory.New<USCTariff>();
			uscTariff.UE_Tariff = "5209316020";
			uscTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			uscTariff.UE_DateTo = new ZDateTime(2012, 12, 31);
			uscTariff.UE_Unit1 = "M2";
			uscTariff.UE_Unit2 = "KG";
			uscTariff.UE_DutyComputationCode = "7";
			uscTariff.UE_Column1RateAdValorem = 0.084m;
			uscTariff.UE_Column2RateAdValorem = 0.209m;

			uscTariffDutyRate = uscTariff.DutyRates.AddNew();
			uscTariffDutyRate.UD_DutyElement = "5";
			uscTariffDutyRate.UD_ISOCountryCode = "AU";
			uscTariffDutyRate.UD_TaxFeeClassCode = "056";
			uscTariffDutyRate.UD_TaxFeeComputationCode = "2";
			uscTariffDutyRate.UD_TaxFeeFlag = "1";
			uscTariffDutyRate.UD_TaxFeeSpecificRate = 0.013057m;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5810.92.9080";
			invoiceLine.JI_CustomsQuantity = 375.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 500m;//KG

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "5209.31.6020";
			invoiceLine2.JI_CustomsQuantity = 375.00000m;
			invoiceLine2.JI_CustomsSecondQuantity = 500m;//KG

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Cotton fee, when not exempt, should be calculated based on parent tariff and this time, < 2$, DeMinimus rule applies", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CottonFee);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
