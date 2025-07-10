using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FeeCalculationDataProviderExtensionsTest : TestCaseWithFactory
	{
		[TestDate(2013, 01, 01)]
		public void TestTaxCalculationWhenRateUQDoesNotMatchCustomsUQs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3303003000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Other_4;
			invoiceLine.US_TaxQty = 10m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var rate = invoiceLine.GetOverriddenTaxRate(invoiceLine.US_TaxApply, invoiceLine.US_TaxRate, invoiceLine.US_TaxCode, invoiceLine.ImportTariff);
			AssertEquals(3.5663227m, rate);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_4;
			invoiceLine.US_TaxQty = 0m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			rate = invoiceLine.GetOverriddenTaxRate(invoiceLine.US_TaxApply, invoiceLine.US_TaxRate, invoiceLine.US_TaxCode, invoiceLine.ImportTariff);
			AssertEquals(0.832142m, rate);
		}

		[TestDate(2008, 9, 11)]
		public void TestIsCottonFeeExempt()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104220040";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._12, "007894812");
			Assert(invoiceLine.IsCottonFeeExempt(false, false));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoiceLine.US_CottonCertificateNo = ZString.Empty;
			Assert(!invoiceLine.IsCottonFeeExempt(false, false));

			invoiceLine.US_CottonCertificateNo = "007894812";
			Assert(invoiceLine.IsCottonFeeExempt(false, false));
		}

		[TestDate(2014, 10, 16)]
		public void TestIsRaspberryFeeExempt()
		{
			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0811202025";
			importTariff.UE_Unit1 = "KG";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.Today;

			var dutyRate = importTariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Raspberry;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.022m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0811202025";
			Assert(!invoiceLine.IsRaspberryFeeExempt());

			invoiceLine.US_CottonCertificateNo = "007894812";
			Assert(invoiceLine.IsRaspberryFeeExempt());

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Assert(!invoiceLine.IsRaspberryFeeExempt());

			var permit = invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._23, "007894812");
			Assert(invoiceLine.IsRaspberryFeeExempt());

			permit.CY_Code = LicencePermitTypeList.Codes._22;
			Assert(invoiceLine.IsRaspberryFeeExempt());
		}

		public void TestShouldCottonFeeExemptBeIndicated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			Assert(invoiceLine.ShouldCottonFeeExemptBeIndicated());

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			Assert("X lines should not be asked to indicate", !invoiceLine.ShouldCottonFeeExemptBeIndicated());

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			Assert("V lines should be asked to indicate", invoiceLine.ShouldCottonFeeExemptBeIndicated());

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			Assert("informal entry type", !invoiceLine.ShouldCottonFeeExemptBeIndicated());

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.US_SecondarySPI = ZString.Empty;

			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6211330035";
			Assert(invoiceLine.ShouldCottonFeeExemptBeIndicated());
			Assert("for secondary lines, no need to indicate as parent has to indicate", !secondaryLine.ShouldCottonFeeExemptBeIndicated());
		}

		public void TestShouldCottonFeeExemptBeIndicated_IsACS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;

			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6211330035";

			Assert(((IFeeCalculationDataProvider)invoiceLine).IsACS);
			Assert(((IFeeCalculationDataProvider)secondaryLine).IsACS);

			Assert(invoiceLine.ShouldCottonFeeExemptBeIndicated());
			Assert(!secondaryLine.ShouldCottonFeeExemptBeIndicated());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			Assert(!((IFeeCalculationDataProvider)invoiceLine).IsACS);
			Assert(!((IFeeCalculationDataProvider)secondaryLine).IsACS);

			Assert(invoiceLine.ShouldCottonFeeExemptBeIndicated());
			Assert(secondaryLine.ShouldCottonFeeExemptBeIndicated());
		}

		public void TestAmountPerUnit()
		{
			ZDecimal amountPerUnit = 1m;
			ZString uQ = "NO";
			AssertEquals("$1.00/NO", IFeeCalculationDataProviderExtensionMethods.AmountPerUnit(amountPerUnit, uQ));

			amountPerUnit = 0.0175362m;
			AssertEquals("1.75362c/NO", IFeeCalculationDataProviderExtensionMethods.AmountPerUnit(amountPerUnit, uQ));

			amountPerUnit = 2.5m;
			uQ = "L";
			AssertEquals("$2.50/L", IFeeCalculationDataProviderExtensionMethods.AmountPerUnit(amountPerUnit, uQ));
		}

		public void TestCottonFeeExemptWhenSupTariffIsInvolved()
		{
			var tariff = new USCTariff.Loader(Factory).LoadBestMatch("6203422005", ZDateTime.Today);
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = "056";
			dutyRate.UD_TaxFeeComputationCode = "2";
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeSpecificRate = 0.009985m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_SupTariff = "9999.00.60";
			invoiceLine.JI_Tariff = "6203.42.2005";

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.JI_CustomsQuantity = 19m;
			invoiceLine.JI_CustomsSecondQuantity = 395m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Cotton fee", ZDecimal.Zero, entry.CottonFee);
		}

		public void TestWhenFeeIsNotRequiredAnyMore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine.JI_CustomsSecondQuantity = 20000m;
			AssertNotNull("PreCondition:Tariff is there", invoiceLine.ImportTariff);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine.JI_Tariff = USCTariff.DOTIsApplicable;//some machine
			AssertNotNull("PreCondition:Tariff is there", invoiceLine.ImportTariff);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNull(invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
