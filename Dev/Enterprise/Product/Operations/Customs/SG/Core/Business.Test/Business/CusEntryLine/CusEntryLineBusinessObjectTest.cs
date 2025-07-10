using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineBusinessObjectTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestTypeDecider()
		{
			Assert(Factory.New<Customs.Business.CusEntryLine>() is CusEntryLine);
		}

		#region Duty/Excise Tests

		public void TestSGTariff()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01011000";
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			var entryLine = (CusEntryLine)Declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals(null, entryLine.Tariff);
		}

		public void TestDutiableWGTVOLUNIT_()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 12.43m;

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.SG_TotalDutiableWGTVOLQTY = 15.25m;

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			AssertEquals(27.68m, ((CusEntryLine)Declaration.ActiveEntryHeaders[0].MergedLines[0]).DutiableWGTVOLUNIT);
		}

		public void TestDutiableWGTVOLUNITUQ_()
		{
			InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = "LPA";
			AssertEquals("LPA", EntryLine.DutiableWGTVOLUNITUQ);
		}

		public void TestDutyUnitRate_()
		{
			InvoiceLine.SG_DutyUnitRate = 12.5m;
			AssertEquals(12.5m, EntryLine.DutyUnitRate);
		}

		public void TestDutyPercentageRate_()
		{
			InvoiceLine.SG_DutyPercentageRate = 12.5m;
			AssertEquals(12.5m, EntryLine.DutyPercentageRate);
		}

		public void TestExciseUnitRate_()
		{
			InvoiceLine.SG_ExciseUnitRate = 12.5m;
			AssertEquals(12.5m, EntryLine.ExciseUnitRate);
		}

		public void TestDutyRateUnit()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			var entryLine = (CusEntryLine)Declaration.ActiveEntryHeaders[0].MergedLines[0];
			entryLine.RandomLine.JI_Tariff = "87113020";
			invoiceLine.SG_TariffCommodityType = CommodityTypeList.Codes.Vehicle;
			AssertEquals(string.Empty, entryLine.DutyRateUnit);

			entryLine.RandomLine.JI_Tariff = "03011010";
			invoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.DAL;
			entryLine = (CusEntryLine)declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals(UnitOfQuantityCodeList.Codes.DAL, entryLine.DutyRateUnit);
		}

		[TestDate(2014, 05, 01)]
		public void TestDutyRateUnit2()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			var entryLine = (CusEntryLine)Declaration.ActiveEntryHeaders[0].MergedLines[0];
			entryLine.RandomLine.JI_Tariff = "22042921";
			invoiceLine.SG_TariffCommodityType = CommodityTypeList.Codes.Alcohol;
			AssertEquals(SGConstants.LPA, entryLine.DutyRateUnit);

			entryLine.RandomLine.JI_Tariff = "03011010";
			invoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.DAL;
			entryLine = (CusEntryLine)declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals(UnitOfQuantityCodeList.Codes.DAL, entryLine.DutyRateUnit);
		}

		[TestDate(2012, 01, 01)]
		public void TestMVDutyRateUnitWhenUOMIsEmpty()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "87112051";
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			var entryLine = (CusEntryLine)Declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("Pre-condition: 2012 HS Tariff should have empty UoM", true, entryLine.RandomLine.UniversalTariff.ZZ1_ZZ8_UQ1.IsEmpty);
			invoiceLine.SG_TariffCommodityType = CommodityTypeList.Codes.Vehicle;
			AssertEquals("PER", entryLine.DutyRateUnit);
		}

		public void TestExcisePercentageRate_()
		{
			InvoiceLine.SG_ExcisePercentageRate = 12.5m;
			AssertEquals(12.5m, EntryLine.ExcisePercentageRate);
		}

		public void TestTobaccoMultiplier()
		{
			InvoiceLine.SG_TobaccoMultiplier = 3;
			AssertEquals(3, EntryLine.TobaccoMultiplier);
		}

		public void TestPercAlcohol()
		{
			InvoiceLine.SG_PercAlcohol = 33.33m;
			AssertEquals(33.33m, EntryLine.PercAlcohol);
		}

		public void TestLSP_()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.SG_LastSellingPrice = 12.43m;

			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			AssertEquals(12.43m, ((CusEntryLine)Declaration.ActiveEntryHeaders[0].MergedLines[0]).LSP);
		}

		public void TestPreferenceRateApplies()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var cusEntryLine = (CusEntryLine)declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals(false, cusEntryLine.PreferenceRateApplies);

			invoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRF;
			AssertEquals(true, cusEntryLine.PreferenceRateApplies);
			invoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			AssertEquals(true, cusEntryLine.PreferenceRateApplies);
			invoiceLine.JI_PrimaryPreference = "X";
			AssertEquals(false, cusEntryLine.PreferenceRateApplies);
			invoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.STD;
			AssertEquals(false, cusEntryLine.PreferenceRateApplies);
		}

		public void TestOtherTaxPercentageRate()
		{
			InvoiceLine.SG_OtherTaxPercentageRate = 8.5m;
			AssertEquals(8.5m, EntryLine.OtherTaxPercentageRate);
		}

		public void TestOtherTaxUnitRate()
		{
			InvoiceLine.SG_OtherTaxUnitRate = 0.84m;
			AssertEquals(0.84m, EntryLine.OtherTaxUnitRate);
		}

		public void TestGSTRate_()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "Goods and Services Tax");
			Factory.Save();

			AssertEquals(0.07m, EntryLine.GSTRate);
			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();

			AssertEquals(0.07m, EntryLine.GSTRate);
		}

		public void TestExciseAmount_()
		{
			EntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 12.34m);
			AssertEquals(12.34m, EntryLine.ExciseAmount);
		}

		#endregion

		public void TestIsDG()
		{
			InvoiceLine.JI_HazMatCodeQualifier = "";
			AssertEquals(false, EntryLine.IsDG);

			InvoiceLine.JI_HazMatCodeQualifier = DGIndicatorCodeList.Codes.N;
			AssertEquals(false, EntryLine.IsDG);

			InvoiceLine.JI_HazMatCodeQualifier = DGIndicatorCodeList.Codes.Y;
			AssertEquals(true, EntryLine.IsDG);
		}

		public void TestMaximumLinesForSG()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
			var invoiceHeader1 = Declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV-1";
			var invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "22030010";
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);
			AssertEquals("JI_LineNo", (ZShort)1, invoiceLine.JI_LineNo);

			for (int i = 1; i < 30; i++)
			{
				invoiceLine = invoiceHeader1.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "22030010";
			}

			AssertEquals("JI_LineNo", (ZShort)30, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(false, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertNoMessageError(invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);

			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			var entryHeader = Declaration.ActiveEntryHeaders[0];
			AssertEquals("Merged lines on merging", 30, entryHeader.MergedLines.Count);

			var invoiceHeader2 = Declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV-2";
			for (int i = 1; i < 21; i++)
			{
				invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "24011010";
			}

			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "24011010";
			AssertEquals("JI_LineNo", (ZShort)21, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasMessageErrors());
			AssertHasMessageError("For all declarations, maximum entry lines is 50 regardless of multiple invoices", invoiceLine.JI_LineNoInfo, SGConstants.MaxLinesValidation.MaxLineLimitForDeclaration);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			entryHeader = declaration.ActiveEntryHeaders[0];
			AssertEquals("Merged lines on merging", 51, entryHeader.MergedLines.Count);

			var cusEntryLine = entryHeader.MergedLines[50];
			cusEntryLine.Validation.ValidateCL_LineNumber();
			cusEntryLine.Header.Validation.ValidateAll();
			var expectedError = string.Format(CultureInfo.CurrentCulture, SGConstants.MaxLinesValidation.MaxEntryLimitForDeclaration + "\r\nThe merged line count is currently {0} entry lines.", declaration.MergedLinesCount);
			AssertHasMessageError("Merged lines also now exceed the maximum number of lines allowed", cusEntryLine.Header.EntryNumberInfo, expectedError);

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			invoiceLine = invoiceHeader2.JobComInvoiceLines.AddNew();
			AssertEquals((ZShort)22, invoiceLine.JI_LineNo);
			invoiceLine.Validation.ValidateJI_LineNo();
			AssertEquals(true, invoiceLine.JI_LineNoInfo.HasMessageErrors());

			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			AssertEquals("Merged lines on merging", 3, entryHeader.MergedLines.Count);
			cusEntryLine = entryHeader.MergedLines[0];
			cusEntryLine.Validation.ValidateCL_LineNumber();
			cusEntryLine.Header.Validation.ValidateAll();
			AssertNoMessageErrors(cusEntryLine.Header.EntryNumberInfo);
			AssertNoMessageError("Merged lines are now within the maximum number allowed", cusEntryLine.Header.EntryNumberInfo, expectedError);

			cusEntryLine = entryHeader.MergedLines[1];
			cusEntryLine.Validation.ValidateCL_LineNumber();
			cusEntryLine.Header.Validation.ValidateAll();
			AssertNoMessageErrors(cusEntryLine.Header.EntryNumberInfo);
			AssertNoMessageError("Merged lines are  within the maximum number allowed", cusEntryLine.Header.EntryNumberInfo, expectedError);

			cusEntryLine = entryHeader.MergedLines[2];
			cusEntryLine.Validation.ValidateCL_LineNumber();
			cusEntryLine.Header.Validation.ValidateAll();
			AssertNoMessageErrors(cusEntryLine.Header.EntryNumberInfo);
			AssertNoMessageError("Merged lines are within the maximum number allowed", cusEntryLine.Header.EntryNumberInfo, expectedError);
		}

		#region ICusInvoice Tests

		[TestDate(2007, 1, 1)]
		public void TestInvoiceDate()
		{
			InvoiceHeader.JZ_InvoiceDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, CusInvoice.InvoiceDate);
		}

		public void TestInvoicePK()
		{
			AssertEquals(InvoiceHeader.PK, CusInvoice.InvoicePK);
		}

		public void TestInvoiceTotalAmount()
		{
			InvoiceHeader.JZ_InvoiceAmount = 20m;
			AssertEquals(20m, CusInvoice.InvoiceTotalAmount);
		}

		public void TestInvoiceCurrExchangeRate()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			AssertEquals(1.79m, CusInvoice.InvoiceCurrExchangeRate);
		}

		public void TestInvoiceCurrency()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusInvoice.InvoiceCurrency);
		}

		public void TestIncoTerm()
		{
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CNI;
			AssertEquals(UnitPriceTermTypeCodeList.Codes.CNI, CusInvoice.IncoTerm);
		}

		public void TestInvoiceNumber()
		{
			InvoiceHeader.JZ_InvoiceNumber = UnitPriceTermTypeCodeList.Codes.CNI;
			AssertEquals(UnitPriceTermTypeCodeList.Codes.CNI, CusInvoice.InvoiceNumber);
		}

		public void TestSupplier()
		{
			InvoiceHeader.JZ_OH_Supplier = Factory.New<OrgHeader>().PK;
			InvoiceHeader.Supplier.OH_FullName = "SUPPLIER";
			AssertEquals("SUPPLIER", CusInvoice.Supplier.Name);
		}

		public void TestFreightCharge()
		{
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 15m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(15m, CusInvoice.FreightCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusInvoice.FreightCharge.CurrencyCode);
			AssertEquals(1.79m, CusInvoice.FreightCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.FreightCharge.Percentage);

			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 35m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(50m, CusInvoice.FreightCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusInvoice.FreightCharge.CurrencyCode);
			AssertEquals(1.79m, CusInvoice.FreightCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.FreightCharge.Percentage);

			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals(99.87m, CusInvoice.FreightCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, CusInvoice.FreightCharge.CurrencyCode);
			AssertEquals(1m, CusInvoice.FreightCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.FreightCharge.Percentage);

			InvoiceHeader.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 100m, Core.Constants.CurrencyCodes.Singapore);
			AssertEquals(199.87m, CusInvoice.FreightCharge.Amount);
		}

		public void TestInsuranceCharge()
		{
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 15m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(15m, CusInvoice.InsuranceCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusInvoice.InsuranceCharge.CurrencyCode);
			AssertEquals(1.79m, CusInvoice.InsuranceCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.InsuranceCharge.Percentage);

			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 35m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(50m, CusInvoice.InsuranceCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusInvoice.InsuranceCharge.CurrencyCode);
			AssertEquals(1.79m, CusInvoice.InsuranceCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.InsuranceCharge.Percentage);

			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals(99.87m, CusInvoice.InsuranceCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, CusInvoice.InsuranceCharge.CurrencyCode);
			AssertEquals(1m, CusInvoice.InsuranceCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.InsuranceCharge.Percentage);

			InvoiceHeader.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 100m, Core.Constants.CurrencyCodes.Singapore);
			AssertEquals(199.87m, CusInvoice.InsuranceCharge.Amount);
		}

		public void TestOtherCharge()
		{
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 15m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(15m, CusInvoice.OtherCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusInvoice.OtherCharge.CurrencyCode);
			AssertEquals(1.79m, CusInvoice.OtherCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.OtherCharge.Percentage);

			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 35m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(50m, CusInvoice.OtherCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusInvoice.OtherCharge.CurrencyCode);
			AssertEquals(1.79m, CusInvoice.OtherCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.OtherCharge.Percentage);

			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals(99.87m, CusInvoice.OtherCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, CusInvoice.OtherCharge.CurrencyCode);
			AssertEquals(1m, CusInvoice.OtherCharge.ExchangeRate);
			AssertEquals(0m, CusInvoice.OtherCharge.Percentage);

			InvoiceHeader.GroupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 100m, Core.Constants.CurrencyCodes.Singapore);
			AssertEquals(199.87m, CusInvoice.OtherCharge.Amount);
		}

		#endregion

		#region ICusItem Tests

		public void TestIsMotorVehicle()
		{
			InvoiceLine.JI_Tariff = "87031010";
			AssertEquals(true, CusItem.IsMotorVehicle);

			InvoiceLine.JI_Tariff = "";
			AssertEquals(false, CusItem.IsMotorVehicle);
		}

		public void TestIsLiquor()
		{
			InvoiceLine.JI_Tariff = "22030011";
			AssertEquals(true, CusItem.IsLiquor);

			InvoiceLine.JI_Tariff = "";
			AssertEquals(false, CusItem.IsLiquor);
		}

		public void TestIsTobacco()
		{
			InvoiceLine.JI_Tariff = "24022090";
			AssertEquals(true, CusItem.IsTobacco);

			InvoiceLine.JI_Tariff = "";
			AssertEquals(false, CusItem.IsTobacco);
		}

		public void TestIsStrategic()
		{
			InvoiceLine.SG_IsStrategic = true;
			AssertEquals(true, CusItem.IsStrategic);

			InvoiceLine.SG_IsStrategic = false;
			AssertEquals(false, CusItem.IsStrategic);
		}

		public void TestCategoryCode()
		{
			InvoiceLine.SG_CategoryCode = "CatCode";
			AssertEquals("CatCode", CusItem.CategoryCode);
		}

		public void TestEndUseCode1()
		{
			InvoiceLine.SG_EndUseCode1 = "EU1";
			AssertEquals("EU1", CusItem.EndUseCode1);
		}

		public void TestEndUseCode2()
		{
			InvoiceLine.SG_EndUseCode2 = "EU2";
			AssertEquals("EU2", CusItem.EndUseCode2);
		}

		public void TestEndUseCode3()
		{
			InvoiceLine.SG_EndUseCode3 = "EU3";
			AssertEquals("EU3", CusItem.EndUseCode3);
		}

		public void TestDutyAmount()
		{
			EntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, 221.23m);
			AssertEquals(221.23m, CusItem.DutyAmount);
		}

		public void TestDutyPercentageRate()
		{
			InvoiceLine.SG_DutyPercentageRate = 4m;
			AssertEquals(4m, CusItem.DutyPercentageRate);
		}

		public void TestDutyUnitRate()
		{
			InvoiceLine.SG_DutyUnitRate = 4m;
			AssertEquals(4m, CusItem.DutyUnitRate);
		}

		public void TestCustomsValue()
		{
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.CIF;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceLine.JI_LinePrice = 1000m;
			AssertEquals(1000m, CusItem.CustomsValue);
		}

		public void TestExciseAmount()
		{
			EntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 121.23m);
			AssertEquals(121.23m, CusItem.ExciseAmount);
		}

		public void TestOtherTaxAmount()
		{
			EntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.OtherTax, 35.50m);
			AssertEquals(35.50m, EntryLine.OtherTaxAmount);
		}

		public void TestExcisePercentageRate()
		{
			InvoiceLine.SG_ExcisePercentageRate = 4m;
			AssertEquals(4m, CusItem.ExcisePercentageRate);
		}

		public void TestExciseUnitRate()
		{
			InvoiceLine.SG_ExciseUnitRate = 4m;
			AssertEquals(4m, CusItem.ExciseUnitRate);
		}

		public void TestGSTPayable()
		{
			EntryLine.Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 21.23m);
			AssertEquals(21.23m, CusItem.GSTPayable);
		}

		public new void TestGSTRate()
		{
			InvoiceHeader.SG_GSTRate = 8;
			AssertEquals(8, CusItem.GSTRate);
		}

		public void TestHSQuantity()
		{
			InvoiceLine.JI_CustomsQuantity = 20m;
			AssertEquals(20m, CusItem.HSQuantity);
		}

		public void TestLSPValue()
		{
			InvoiceLine.SG_LastSellingPrice = 20m;
			AssertEquals(20m, CusItem.LSPValue);
		}

		public void TestPercentageOfAlcohol()
		{
			InvoiceLine.SG_PercAlcohol = 20m;
			AssertEquals(20m, CusItem.PercentageOfAlcohol);
		}

		public void TestTotalDutiableQuantity()
		{
			InvoiceLine.SG_TotalDutiableWGTVOLQTY = 20m;
			AssertEquals(20m, CusItem.TotalDutiableQuantity);
		}

		public void TestUnitDutiableQuantity()
		{
			InvoiceLine.SG_UnitDutiableWGTVOLQTY = 20m;
			AssertEquals(20m, CusItem.UnitDutiableQuantity);
		}

		public void TestUnitPrice()
		{
			InvoiceLine.JI_LinePrice = 1000m;
			InvoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals(100m, CusItem.UnitPrice);
		}

		public void TestPackInmostQuantity()
		{
			InvoiceLine.SG_InmostPackQuantity = 15;
			AssertEquals(15, CusItem.PackInmostQuantity);
		}

		public void TestPackInQuantity()
		{
			InvoiceLine.SG_InPackQuantity = 1;
			AssertEquals(1, CusItem.PackInQuantity);
		}

		public void TestPackInnerQuantity()
		{
			InvoiceLine.SG_InnerPackQuantity = 5;
			AssertEquals(5, CusItem.PackInnerQuantity);
		}

		public void TestPackOuterQuantity()
		{
			InvoiceLine.SG_OuterPackQuantity = 9;
			AssertEquals(9, CusItem.PackOuterQuantity);
		}

		public void TestBrandName()
		{
			InvoiceLine.JI_BrandName = "BrandName";
			AssertEquals("BrandName", CusItem.BrandName);
		}

		public void TestCountryOfOriginCode()
		{
			InvoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, CusItem.CountryOfOriginCode);
		}

		public void TestCurrentLotNumber()
		{
			InvoiceLine.SG_LotNo = "LotNumber";
			AssertEquals("LotNumber", CusItem.CurrentLotNumber);
		}

		public void TestDGIndicator()
		{
			InvoiceLine.JI_HazMatCodeQualifier = "";
			AssertEquals(DGIndicatorCodeList.Codes.N, CusItem.DGIndicator);

			InvoiceLine.JI_HazMatCodeQualifier = DGIndicatorCodeList.Codes.N;
			AssertEquals(DGIndicatorCodeList.Codes.N, CusItem.DGIndicator);

			InvoiceLine.JI_HazMatCodeQualifier = DGIndicatorCodeList.Codes.Y;
			AssertEquals(DGIndicatorCodeList.Codes.Y, CusItem.DGIndicator);
		}

		public void TestE_SDNPIndicator()
		{
			InvoiceLine.SG_ESNDPIndicator = "XY";
			AssertEquals("XY", CusItem.E_SDNPIndicator);
		}

		public void TestEndUseDescription()
		{
			InvoiceLine.SG_EndUseDescription = "EndUseDescription";
			AssertEquals("EndUseDescription", CusItem.EndUseDescription);
		}

		public void TestGoodsDescription()
		{
			InvoiceLine.JI_Description = "Description";
			AssertEquals("Description", CusItem.GoodsDescription);
		}

		public void TestHSCode()
		{
			EntryLine.CL_AdValoremTariff = "00000000";
			AssertEquals("00000000", CusItem.HSCode);
		}

		public void TestHSQuantityUnitType()
		{
			InvoiceLine.JI_CustomsUnitQty = UnitOfQuantityCodeList.Codes.NMB;
			AssertEquals("NMB", CusItem.HSQuantityUnitType);
		}

		public void TestModelDescription()
		{
			InvoiceLine.JI_Model = "Model";
			AssertEquals("Model", CusItem.ModelDescription);
		}

		public void TestInvoiceUQ()
		{
			InvoiceLine.JI_InvoiceUQ = UnitOfQuantityCodeList.Codes.NMB;
			AssertEquals("NMB", CusItem.InvoiceUQ);
		}

		public void TestMarksAndNumbers()
		{
			InvoiceLine.MarksAndNumbers = "MarksAndNumbers";
			AssertEquals("MarksAndNumbers", CusItem.MarksAndNumbers);
		}

		public void TestItemDutyRefund()
		{
			InvoiceLine.SG_RefundForItemCustomsDutyAmount = 10m;
			AssertEquals(10m, CusItem.ItemDutyRefund);
		}

		public void TestItemExciseRefund()
		{
			InvoiceLine.SG_RefundForItemExciseAmount = 11m;
			AssertEquals(11m, CusItem.ItemExciseRefund);
		}

		public void TestItemGSTRefund()
		{
			InvoiceLine.SG_RefundForItemGSTAmount = 10m;
			AssertEquals(10m, CusItem.ItemGSTRefund);
		}

		public void TestInwardHAWB()
		{
			InvoiceLine.SG_InwardHAWB = "InwardHAWB";
			AssertEquals("InwardHAWB", CusItem.InwardHAWB);
		}

		public void TestInwardMAWB()
		{
			InvoiceLine.SG_InwardMAWB = "InwardMAWB";
			AssertEquals("InwardMAWB", CusItem.InwardMAWB);
		}

		public void TestOutwardHAWB()
		{
			InvoiceLine.SG_OutwardHAWB = "OutwardHAWB";
			AssertEquals("OutwardHAWB", CusItem.OutwardHAWB);
		}

		public void TestOutwardMAWB()
		{
			InvoiceLine.JI_OutwardMAWB = "OutwardMAWB";
			AssertEquals("OutwardMAWB", CusItem.OutwardMAWB);
		}

		public void TestPackInmostUnitType()
		{
			InvoiceLine.SG_InmostPackQuantityUnit = "NM1";
			AssertEquals("NM1", CusItem.PackInmostUnitType);
		}

		public void TestPackInUnitType()
		{
			InvoiceLine.SG_InPackQuantityUnit = "NM2";
			AssertEquals("NM2", CusItem.PackInUnitType);
		}

		public void TestPackInnerUnitType()
		{
			InvoiceLine.SG_InnerPackQuantityUnit = "NM3";
			AssertEquals("NM3", CusItem.PackInnerUnitType);
		}

		public void TestPackOuterUnitType()
		{
			InvoiceLine.SG_OuterPackQuantityUnit = "NM4";
			AssertEquals("NM4", CusItem.PackOuterUnitType);
		}

		public void TestPreferenceIndicator()
		{
			InvoiceLine.JI_PrimaryPreference = PreferentialIndicatorCodeList.Codes.PRI;
			AssertEquals(PreferentialIndicatorCodeList.Codes.PRI, CusItem.PreferenceIndicator);
		}

		public void TestPreviousLotNumber()
		{
			InvoiceLine.SG_PreviousLotNo = "PreviousLotNo";
			AssertEquals("PreviousLotNo", CusItem.PreviousLotNumber);
		}

		public void TestSerialNumber()
		{
			EntryLine.CL_LineNumber = 10;
			AssertEquals("10", CusItem.SerialNumber);
		}

		public void TestTotalDutiableQuantityUnitType()
		{
			InvoiceLine.SG_TotalDutiableWGTVOLQTYUnit = UnitOfQuantityCodeList.Codes.NMB;
			AssertEquals("NMB", CusItem.TotalDutiableQuantityUnitType);
		}

		public void TestUnitDutiableQuantityUnitType()
		{
			InvoiceLine.SG_UnitDutiableWGTVOLQTYUnit = "NM1";
			AssertEquals("NM1", CusItem.UnitDutiableQuantityUnitType);
		}

		public void TestProductCodes()
		{
			CusLineTariffDetail tariffDetail = InvoiceLine.ProductCodes.AddNew();
			IEnumerable<ICusProductCode> productCodes = CusItem.ProductCodes;

			using (IEnumerator<ICusProductCode> enumerator = productCodes.GetEnumerator())
			{
				enumerator.MoveNext();
				AssertEquals(tariffDetail, enumerator.Current);
			}
		}

		public void TestOptionalItemCharge()
		{
			InvoiceLine.Charges.AddNew(InvoiceLineCharge.ChargeTypes.OptionalItemCharges, 15m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(15m, CusItem.OptionalItemCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusItem.OptionalItemCharge.CurrencyCode);
			AssertEquals(1.79m, CusItem.OptionalItemCharge.ExchangeRate);
			AssertEquals(0m, CusItem.OptionalItemCharge.Percentage);

			InvoiceLine.Charges.AddNew(InvoiceLineCharge.ChargeTypes.OptionalItemCharges, 35m, Core.Constants.CurrencyCodes.SouthAfrica);
			AssertEquals(50m, CusItem.OptionalItemCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.SouthAfrica, CusItem.OptionalItemCharge.CurrencyCode);
			AssertEquals(1.79m, CusItem.OptionalItemCharge.ExchangeRate);
			AssertEquals(0m, CusItem.OptionalItemCharge.Percentage);

			InvoiceLine.Charges.AddNew(InvoiceLineCharge.ChargeTypes.OptionalItemCharges, 100m, Core.Constants.CurrencyCodes.UnitedStates);
			AssertEquals(99.87m, CusItem.OptionalItemCharge.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, CusItem.OptionalItemCharge.CurrencyCode);
			AssertEquals(1m, CusItem.OptionalItemCharge.ExchangeRate);
			AssertEquals(0m, CusItem.OptionalItemCharge.Percentage);

			InvoiceLine.Charges.AddNew(InvoiceLineCharge.ChargeTypes.OptionalItemCharges, 100m, Core.Constants.CurrencyCodes.Singapore);
			AssertEquals(199.87m, CusItem.OptionalItemCharge.Amount);
		}

		#endregion

		#region ICusCertItem

		[TestDate(2007, 1, 1)]
		public void TestDateOfManufacturingCost()
		{
			InvoiceLine.SG_ManufacturingCostStatementDate = ZDateTime.Today;
			AssertEquals(new ZDateTime(2007, 1, 1), CusCertItem.DateOfManufacturingCost);
		}

		public void TestItemValue()
		{
			InvoiceLine.SG_CertItemValue = 10m;
			AssertEquals(10m, CusCertItem.ItemValue);
		}

		public void TestItemQuantity()
		{
			InvoiceLine.SG_CertItemQuantity = 11m;
			AssertEquals(11m, CusCertItem.ItemQuantity);
		}

		public void TestPercentageContent()
		{
			InvoiceLine.SG_PercContent = 9;
			AssertEquals(9, CusCertItem.PercentageContent);
		}

		public void TestTextileQuotaQty()
		{
			InvoiceLine.SG_TextileQuotaQuantity = 8;
			AssertEquals(8m, CusCertItem.TextileQuotaQty);
		}

		public void TestItemDescription()
		{
			InvoiceLine.CertItemDescription = "ItemDescription";
			AssertEquals("ItemDescription", CusCertItem.ItemDescription);
		}

		public void TestItemQuantityUnitType()
		{
			InvoiceLine.SG_CertItemQuantityUnit = "PCE";
			AssertEquals("PCE", CusCertItem.ItemQuantityUnitType);
		}

		public void TestOriginCriterion1()
		{
			InvoiceLine.SG_CertOriginCriterion1 = "OriginCriterion1";
			AssertEquals("OriginCriterion1", CusCertItem.OriginCriterion1);
		}

		public void TestOriginCriterion2()
		{
			InvoiceLine.SG_CertOriginCriterion2 = "OriginCriterion2";
			AssertEquals("OriginCriterion2", CusCertItem.OriginCriterion2);
		}

		public void TestOriginCriterion3()
		{
			InvoiceLine.SG_CertOriginCriterion3 = "OriginCriterion3";
			AssertEquals("OriginCriterion3", CusCertItem.OriginCriterion3);
		}

		public void TestCertHSCode()
		{
			InvoiceLine.SG_CertHSCode = "123456";
			AssertEquals("123456", CusCertItem.CertHSCode);
		}

		public void TestTextileCategoryCode()
		{
			InvoiceLine.SG_TextileCatCode = "TXT";
			AssertEquals("TXT", CusCertItem.TextileCategoryCode);
		}

		public void TestTextileQuotaUnitCode()
		{
			InvoiceLine.SG_TextileQuotaQuantityUnit = "UNT";
			AssertEquals("UNT", CusCertItem.TextileQuotaUnitCode);
		}

		#endregion

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		#region Implementation
		#region Interface Objects

		ICusItem CusItem
		{
			get { return EntryLine; }
		}

		ICusInvoice CusInvoice
		{
			get { return EntryLine; }
		}

		ICusCertItem CusCertItem
		{
			get { return EntryLine; }
		}

		#endregion

		#region Declaration

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MergeBy = "TRF";
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion

		#region EntryHeader

		CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		#endregion

		#region EntryLine

		CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					fEntryLine = EntryHeader.MergedLines.AddNew();
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;

		#endregion

		#region InvoiceHeader

		JobComInvoiceHeader InvoiceHeader
		{
			get { return (JobComInvoiceHeader)InvoiceLine.InvoiceHeader; }
		}

		#endregion

		#region InvoiceLine

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine.JI_CL = EntryLine.PK;
				}

				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#endregion

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);

			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "22042921");
			helper.CreateTariffUOM(tariff1, Constants.UnitOfMeasureTypes.StatisticalUOMType, "LTR");
			helper.CreateTariffUOM(tariff1, Constants.UnitOfMeasureTypes.AdditionalUOMType, SGConstants.LPA);
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff1);

			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "03011010");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, UnitOfQuantityCodeList.Codes.DAL, tariff2);

			helper.LoadOrCreateNewTariff(tariffType, "87112051");

			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "87031010");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Vehicle, tariff3);

			var tariff4 = helper.LoadOrCreateNewTariff(tariffType, "22030011");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff4);

			var tariff5 = helper.LoadOrCreateNewTariff(tariffType, "24022090");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff5);

			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;

		protected override bool RatesAreReciprocal
		{
			get { return true; }
		}

		protected override ZString ExpectedFallbackEntrylineDescription => "LINE";

		#endregion

		#endregion
	}
}
