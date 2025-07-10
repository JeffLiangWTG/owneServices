using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class OriginalReconDutyDataTest : TestCaseWithFactory
	{
		public void TestSecondaryLinesWithSup()
		{
			using (reconDec.ReconWrappedJobDeclaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine.US_SupTariff = "9802.00.8068";
				invoiceLine.US_98GoodsValue = 1852m;
				invoiceLine.JI_Tariff = "9102.11.1010";
				invoiceLine.JI_LinePrice = 3406m;
				invoiceLine.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine);

				JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine2.US_SupTariff = "9802.00.8068";
				invoiceLine2.US_98GoodsValue = 1010m;
				invoiceLine2.JI_Tariff = "9102.11.1020";
				invoiceLine2.JI_LinePrice = 1609m;
				invoiceLine2.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine2);

				JobComInvoiceLine invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine3.US_SupTariff = "9802.00.8068";
				invoiceLine3.US_98GoodsValue = 0m;
				invoiceLine3.JI_Tariff = "9102.11.1030";
				invoiceLine3.JI_LinePrice = 1345m;
				invoiceLine3.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine3);

				JobComInvoiceLine invoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine4.US_SupTariff = "9802.00.8068";
				invoiceLine4.US_98GoodsValue = 204m;
				invoiceLine4.JI_Tariff = "9102.11.1040";
				invoiceLine4.JI_LinePrice = 0m;
				invoiceLine4.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine4);

				IEntryLineOrInvoiceLineDutyData orignalDutyData = new ReconOriginalDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, orignalDutyData.SecondaryLines.Count());

				orignalDutyData = new ReconOriginalDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, orignalDutyData.SecondaryLines.Count());

				orignalDutyData = new ReconOriginalDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, orignalDutyData.SecondaryLines.Count());

				orignalDutyData = new ReconOriginalDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, orignalDutyData.SecondaryLines.Count());

				IEntryLineOrInvoiceLineDutyData originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(7, originalSupDutyData.SecondaryLines.Count());

				originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, originalSupDutyData.SecondaryLines.Count());

				originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, originalSupDutyData.SecondaryLines.Count());

				originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, originalSupDutyData.SecondaryLines.Count());

				IEntryLineOrInvoiceLineDutyData reconDutyData = new ReconCurrentDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconDutyData.SecondaryLines.Count());

				reconDutyData = new ReconOriginalDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconDutyData.SecondaryLines.Count());

				reconDutyData = new ReconOriginalDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconDutyData.SecondaryLines.Count());

				reconDutyData = new ReconOriginalDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconDutyData.SecondaryLines.Count());

				IEntryLineOrInvoiceLineDutyData reconSupDutyData = new ReconSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(7, reconSupDutyData.SecondaryLines.Count());

				reconSupDutyData = new ReconOriginalSupDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconSupDutyData.SecondaryLines.Count());

				reconSupDutyData = new ReconOriginalSupDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconSupDutyData.SecondaryLines.Count());

				reconSupDutyData = new ReconOriginalSupDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconSupDutyData.SecondaryLines.Count());
			}
		}

		public void TestSecondaryLinesWithoutSup()
		{
			using (reconDec.ReconWrappedJobDeclaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine.JI_Tariff = "9102.11.1010";
				invoiceLine.JI_LinePrice = 3406m;
				invoiceLine.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine);

				JobComInvoiceLine invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine2.JI_Tariff = "9102.11.1020";
				invoiceLine2.JI_LinePrice = 1609m;
				invoiceLine2.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine2);

				JobComInvoiceLine invoiceLine3 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine3.JI_Tariff = "9102.11.1030";
				invoiceLine3.JI_LinePrice = 1345m;
				invoiceLine3.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine3);

				JobComInvoiceLine invoiceLine4 = invoiceLine.AddSecondaryInvoiceLine();
				invoiceLine4.JI_Tariff = "9102.11.1040";
				invoiceLine4.JI_LinePrice = 0m;
				invoiceLine4.JI_CustomsQuantity = 1000m;
				SetReconOriginalValues(invoiceLine4);

				IEntryLineOrInvoiceLineDutyData orignalDutyData = new ReconOriginalDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(3, orignalDutyData.SecondaryLines.Count());

				orignalDutyData = new ReconOriginalDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, orignalDutyData.SecondaryLines.Count());

				orignalDutyData = new ReconOriginalDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, orignalDutyData.SecondaryLines.Count());

				orignalDutyData = new ReconOriginalDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, orignalDutyData.SecondaryLines.Count());

				IEntryLineOrInvoiceLineDutyData originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, originalSupDutyData.SecondaryLines.Count());

				originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, originalSupDutyData.SecondaryLines.Count());

				originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, originalSupDutyData.SecondaryLines.Count());

				originalSupDutyData = new ReconOriginalSupDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, originalSupDutyData.SecondaryLines.Count());

				IEntryLineOrInvoiceLineDutyData reconDutyData = new ReconCurrentDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(3, reconDutyData.SecondaryLines.Count());

				reconDutyData = new ReconOriginalDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconDutyData.SecondaryLines.Count());

				reconDutyData = new ReconOriginalDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconDutyData.SecondaryLines.Count());

				reconDutyData = new ReconOriginalDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconDutyData.SecondaryLines.Count());

				IEntryLineOrInvoiceLineDutyData reconSupDutyData = new ReconSupDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconSupDutyData.SecondaryLines.Count());

				reconSupDutyData = new ReconOriginalSupDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconSupDutyData.SecondaryLines.Count());

				reconSupDutyData = new ReconOriginalSupDutyData(invoiceLine3, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconSupDutyData.SecondaryLines.Count());

				reconSupDutyData = new ReconOriginalSupDutyData(invoiceLine4, originalEntry.US_R_DateForMPFCalc);
				AssertEquals(0, reconSupDutyData.SecondaryLines.Count());
			}
		}

		public void TestReconWithSupTariff()
		{
			invoiceLine.US_SupTariff = "9802008068";
			invoiceLine.JI_Tariff = "4201006000";
			invoiceLine.JI_CustomsQuantity = 2;
			invoiceLine.US_98GoodsValue = 2300m;
			invoiceLine.JI_LinePrice = 20000m;

			invoiceLine.US_R_OrigSupTariff = "9802008068";
			invoiceLine.US_R_OrigTariff = "4201006000";
			invoiceLine.US_R_OrigFirstQty = 2;
			invoiceLine.US_R_Orig98Value = 2300m;
			invoiceLine.US_R_OrigCV = 20000m;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals("Duty calculated", 560m, invoiceLine.US_Duty);
			AssertEquals("Duty calculated", 560m, invoiceLine.US_R_OrigDuty);
			AssertEquals("Total Duty payable", 560m, originalEntry.ReconDuty);
			AssertEquals("Total Duty payable", 560m, originalEntry.OriginalDuty);
		}

		public void TestGetOrSetDutyFeeCharge()
		{
			IEntryLineOrInvoiceLineDutyData provider = originalData;

			provider.SetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 20m, new FeeCalculationInternalData());
			provider.SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey, 15m, new FeeCalculationInternalData());

			AssertEquals("Amount", 20m, provider.GetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));

			AssertEquals("Amount", 15m, provider.GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey));

			IFeeCalculationDataProvider feeProvider = originalData;
			feeProvider.SetFeeResult(Core.Constants.USCustoms.FeeCodes.Honey, 50m, new FeeCalculationInternalData());
			AssertEquals("Amount", 65m, provider.GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Honey));
		}

		public void TestIsFeeOverriden()
		{
			ReconEntryOriginalCharge charge = invoiceLine.ReconOriginalCharges.AddNew();
			charge.CY_Code = Core.Constants.USCustoms.FeeCodes.Honey;

			charge = invoiceLine.ReconOriginalCharges.AddNew();
			charge.CY_Code = Core.Constants.USCustoms.FeeCodes.Mango;
			charge.CY_IsOverridden = true;

			IFeeCalculationDataProvider provider = originalData;
			AssertEquals(false, provider.IsFeeOverriden(Core.Constants.USCustoms.FeeCodes.Honey));
			AssertEquals(true, provider.IsFeeOverriden(Core.Constants.USCustoms.FeeCodes.Mango));
			AssertEquals(false, provider.IsFeeOverriden(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestParentTariffLine()
		{
			AssertNull(originalData.ParentTariffLine);

			JobComInvoiceLine parentLine = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = parentLine.PK;
			AssertNotNull(originalData.ParentTariffLine);

			parentLine.JI_Tariff = "1";
			parentLine.US_R_OrigTariff = "2";

			AssertEquals("2", originalData.ParentTariffLine.Tariff);
		}

		public void TestSpecialProgramsIndicatorPrimary()
		{
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, "Expired Special Program Indicator");
			universalReferenceTestHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, PrimarySpecProgramIndicatorList.Codes.A, "A", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1), Universal.RefCusCodeListAttributeTypes.Codes.USSPIException, PrimarySpecProgramIndicatorList.Codes.D);
			Factory.Save();

			invoiceLine.US_R_OrigSPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertEquals("US_R_OrigSPI", PrimarySpecProgramIndicatorList.Codes.A, invoiceLine.US_R_OrigSPI);

			originalEntry.US_R_DutyRateDate = ZDateTime.Today.AddDays(-5);
			IEntryLineOrInvoiceLineDutyData provider = originalData;
			AssertEquals("SpecialProgramsIndicatorPrimary", ZString.Empty, provider.SpecialProgramsIndicatorPrimary);
		}

		public void TestSpecialProgramsIndicatorCountry()
		{
			var universalReferenceTestHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, "Expired Special Program Indicator");
			universalReferenceTestHelper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USExpiredSPI, PrimarySpecProgramIndicatorList.Codes.A, "A", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1), Universal.RefCusCodeListAttributeTypes.Codes.USSPIException, PrimarySpecProgramIndicatorList.Codes.D);
			Factory.Save();

			invoiceLine.US_R_OrigSPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertEquals("US_R_OrigSPI", PrimarySpecProgramIndicatorList.Codes.A, invoiceLine.US_R_OrigSPI);

			originalEntry.US_R_DutyRateDate = ZDateTime.Today.AddDays(-5);
			IEntryLineOrInvoiceLineDutyData provider = originalData;
			AssertEquals("SpecialProgramsIndicatorCountry", ZString.Empty, provider.SpecialProgramsIndicatorCountry);
		}

		public void TestIFeeCalculationDataProvider()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0000000000";
			tariff.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff.UE_DateTo = ZDateTime.Today;

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Avocado;

			invoiceLine.US_R_OrigTariff = "0000000000";
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 2);

			IFeeCalculationDataProvider provider = originalData;

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals("IsSetVLine", false, provider.IsSetVLine);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("IsSetVLine", true, provider.IsSetVLine);

			invoiceLine.US_R_OrigCV = 1000.51m;
			AssertEquals("CustomsValue:", 1001m, provider.CustomsValue);

			invoiceLine.US_R_OrigCV = 0.45m;
			AssertEquals("CustomsValue:", 0m, provider.CustomsValue);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals(true, provider.IsSetVLine);

			JobComInvoiceLine secondary = invoiceLine.AddSecondaryInvoiceLine();
			IDutyData secondaryDutyData = new ReconOriginalDutyData(secondary, originalEntry.US_R_DateForMPFCalc);
			AssertEquals("ParentTariffLine of originalDutyData should return Original version of duty data for parent tariff line", "0000000000", secondaryDutyData.ParentTariffLine.Tariff);
		}

		public void TestGetSelectedRateType()
		{
			ReconEntryOriginalCharge charge = invoiceLine.ReconOriginalCharges.AddNew();
			charge.CY_Code = Core.Constants.USCustoms.FeeCodes.Honey;
			charge.CY_SelectedRateType = RateTypeList.Codes.Secondary;

			ReconOriginalDutyData reconOriginalDutyData = new ReconOriginalDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
			AssertEquals(RateTypeList.Codes.Secondary, reconOriginalDutyData.GetSelectedRateType(Core.Constants.USCustoms.FeeCodes.Honey));
		}

		[TestDate(2009, 6, 1)]
		public void TestCottonFeeThresholdAndEntryTotal()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_MsgMode = JobApplicationCodeList.Codes.ACS;
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 9);
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 63650m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6103.43.1570";
			invoiceLine.JI_CustomsQuantity = 2361.00000m;
			invoiceLine.JI_LinePrice = 58574.00m;
			invoiceLine.JI_CustomsSecondQuantity = 6197.0000m;

			invoiceLine.US_R_OrigCV = invoiceLine.JI_LinePrice;
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_R_OrigSecondQty = invoiceLine.JI_CustomsSecondQuantity;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6111.30.5020";
			invoiceLine2.JI_CustomsQuantity = 141m;
			invoiceLine2.JI_LinePrice = 5076.00m;
			invoiceLine2.JI_CustomsSecondQuantity = 321.0000m;

			invoiceLine2.US_R_OrigCV = invoiceLine2.JI_LinePrice;
			invoiceLine2.US_R_OrigTariff = invoiceLine2.JI_Tariff;
			invoiceLine2.US_R_OrigFirstQty = invoiceLine2.JI_CustomsQuantity;
			invoiceLine2.US_R_OrigSecondQty = invoiceLine2.JI_CustomsSecondQuantity;

			declaration.CalculateDutyFeesForChangedEntries();

			AssertNotEquals("invoice line cotton fee exisst", 0m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertNotEquals("invoice line cotton fee exisst", 0m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			AssertEquals("however invoice line 2 cotton fee is exempt", 0m, invoiceLine2.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("however invoice line 2 cotton fee is exempt", 0m, invoiceLine2.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			AssertEquals("Total Cotton Fee for originalEntry", invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton), originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals("Total Cotton Fee for originalEntry", invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton), originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		public void TestCalculateException()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 63650m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "8457.20.0010";
			invoiceLine.US_R_OrigSupTariff = "9802.00.8068";
			invoiceLine.US_R_Orig98Value = 319m;
			invoiceLine.US_R_OrigCV = 1356m;
			invoiceLine.TariffCalculateExceptionMessage = "Test error1";

			var dutyData = new ReconOriginalDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
			AssertEquals("Test error1", ((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException);

			((IEntryLineOrInvoiceLineDutyData)dutyData).CalculateException = "Test error2";
			AssertEquals("Test error2", invoiceLine.TariffCalculateExceptionMessage);
		}

		public void TestSecondaryLines()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 63650m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_R_OrigTariff = "8457.20.0010";
			invoiceLine.US_R_OrigSupTariff = "9802.00.8068";
			invoiceLine.US_R_Orig98Value = 319m;
			invoiceLine.US_R_OrigCV = 1356m;

			var dutyData = new ReconOriginalDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
			Assert("IsSecondaryLine of its own sup line", dutyData.IsSecondaryTariffLine);
			AssertEquals(0, ((IFeeCalculationDataProvider)dutyData).SecondaryLines.Count());

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_R_OrigTariff = "9102.11.1010";
			AssertEquals("PreCondition:SecondaryTariffLines are added", 3, invoiceLine2.SecondaryTariffLines.Count());
			var dutyData2 = new ReconOriginalDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);

			Assert(!dutyData2.IsSecondaryTariffLine);
			var firstChildLine = invoiceLine2.SecondaryTariffLines.ElementAt(0);
			firstChildLine.US_R_OrigSupTariff = "9802.00.8068";
			AssertEquals("include supData for the first child line", 4, ((IFeeCalculationDataProvider)dutyData2).SecondaryLines.Count());
		}

		public void TestCalculateForOriginalReconData()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;
			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 63650m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2204.10.0030";
			AssertNotNull(invoiceLine.ImportTariff);

			invoiceLine.JI_CustomsQuantity = 150.00000m;
			invoiceLine.JI_LinePrice = 5000.00m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_TaxRate = 0.9999m;

			invoiceLine.US_R_OrigTariff = "2204.10.0030";
			AssertNotNull(invoiceLine.OriginalImportTariff);

			invoiceLine.US_R_OrigFirstQty = 150.00000m;
			invoiceLine.US_R_OrigCV = 5000.00m;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_R_OrigTaxRate = 0.8888m;

			declaration.CalculateDutyFeesForAllEntries();

			AssertEquals("Amount for Recon Current", 149.99m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Wines));
			AssertEquals("Amount for Recon Original", 133.32m, invoiceLine.ReconOriginalCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Wines));

			AssertEquals("Amount for Recon Current", 149.99m, originalEntry.ReconWines);
			AssertEquals("Amount for Recon Original", 133.32m, originalEntry.OriginalWines);
		}

		public void TestWhenTaxIsNotApplicableForRecon()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;
			var invoice = entry.Invoice;

			SetUpTariffsForTaxRelatedFields();

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_R_OrigTariff = "00000000";
			invoiceLine.US_R_OrigFirstQty = 200m;

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.No;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals(50m, entry.ReconWines);
			AssertEquals("Not applicable is indicated", 0m, entry.OriginalWines);

			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;
			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals(50m, entry.ReconWines);
			AssertEquals(100m, entry.OriginalWines);
		}

		public void TestWhenTaxIsConditionalForRecon()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;
			var invoice = entry.Invoice;

			SetUpTariffsForTaxRelatedFields();

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000001";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_R_OrigTariff = "00000001";
			invoiceLine.US_R_OrigFirstQty = 100m;

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;

			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Specify;
			invoiceLine.US_R_OrigTaxRateS = AppendixBTaxRateList.Codes.Specify;

			invoiceLine.US_TaxRate = 0.3m;
			invoiceLine.US_R_OrigTaxRate = 0.4m;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals(30m, entry.ReconOtherExcise);
			AssertEquals(40m, entry.OriginalOtherExcise);
		}

		public void TestTwoTobaccoTax()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			entry.US_R_DutyRateDate = ZDateTime.Today;
			entry.US_R_CalcOrigDuty = true;
			var invoice = entry.Invoice;

			var invoiceLine = reconDec.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2403102050";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_R_OrigTariff = "2403102080";
			invoiceLine.US_R_OrigFirstQty = 1000m;

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Yes;

			reconDec.CalculateDutyFeesForAllEntries();

			AssertEquals(241.82m, entry.ReconTobacco);
			AssertEquals(2418.23m, entry.OriginalTobacco);
		}

		public void TestChargeAndFeeRelatedMembers()
		{
			invoiceLine.ReconOriginalCharges.SetAmount("AAA", 1m);
			invoiceLine.US_R_OrigDuty = 2m;

			JobComInvoiceLine invoiceLine2 = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.ReconOriginalCharges.SetAmount("AAA", 4m);
			invoiceLine2.US_R_OrigDuty = 8m;

			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries[0];
			originalEntry.OriginalCharges.RemoveAndDeleteAll();

			originalData.RollUpFees(new ReconOriginalDutyDataLineHeader(originalEntry));
			AssertEquals("Charge & Fees should have been rolled up", 1m, originalEntry.OriginalCharges.GetAmount("AAA"));
			AssertEquals("Charge & Fees should have been rolled up", 2m, originalEntry.OriginalCharges.GetAmount("DTY"));

			ReconOriginalDutyData originalData2 = new ReconOriginalDutyData(invoiceLine2, originalEntry.US_R_DateForMPFCalc);
			originalData2.RollUpFees(new ReconOriginalDutyDataLineHeader(originalEntry));
			AssertEquals("Charge & Fees should have been rolled up", 5m, originalEntry.OriginalCharges.GetAmount("AAA"));
			AssertEquals("Charge & Fees should have been rolled up", 10m, originalEntry.OriginalCharges.GetAmount("DTY"));
		}

		public void TestRollUpFeesForMonthlyFiling()
		{
			originalEntry.US_R_MonthlyFiling = true;
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 50m);

			invoiceLine.ReconOriginalCharges.SetAmount("499", 1m);

			originalData.RollUpFees(new ReconOriginalDutyDataLineHeader(originalEntry));
			AssertEquals("MPF is not rolled up", 50m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestRollUpFeesChangedLinesOnly()
		{
			originalEntry.US_R_ChangedLinesOnly = true;
			originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 17.33m);
			invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 5.32m);

			originalData.RollUpFees(new ReconOriginalDutyDataLineHeader(originalEntry));
			AssertEquals("original MPF", 17.33m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		public void TestCalculateFees()
		{
			JobComInvoiceHeader invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 1);
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_Tariff = "2402106000";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";
			invoiceLine.JI_CustomsQuantity = 2000m;
			invoiceLine.JI_CustomsSecondQuantity = 2360m;

			invoiceLine.US_R_OrigCV = 15000m;
			invoiceLine.US_R_OrigTariff = "2402103030";
			invoiceLine.US_R_OrigSPI = "";
			invoiceLine.US_R_OrigFirstQty = 1000m;
			invoiceLine.US_R_OrigFirstUQ = "K";
			invoiceLine.US_R_OrigSecondQty = 1360m;
			invoiceLine.US_R_OrigSecondUQ = "KG";

			reconDec.CalculateDutyFeesForChangedEntries();

			AssertEquals("Original Tobacco Fee using original tariff and quantities", 1828m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Original Duty Amount using original tariff and quantities", 3275.4m, originalEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));

			AssertEquals("Recon Tobacco Fee using recon tariff and quantities", 3656m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Recon Duty Amount using recon tariff and quantities", 1555.2m, originalEntry.ReconCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.Duty));
		}

		public void TestSecondaryTariffLineRelatedMembers()
		{
			JobComInvoiceLine secondaryLine = invoiceLine.AddSecondaryInvoiceLine();

			ReconOriginalDutyData secondaryLineOriginalData = new ReconOriginalDutyData(secondaryLine, originalEntry.US_R_DateForMPFCalc);

			invoiceLine.US_R_OrigCV = 3000.50m;
			secondaryLine.US_R_OrigCV = 50m;
			AssertEquals("TotalCustomsValueIncludingSecondaryLines", 3051m, originalData.TotalCustomsValueIncludingSecondaryLines);

			AssertEquals("SecondaryTariffLines", 1, new List<IEntryLineOrInvoiceLineDutyData>(originalData.SecondaryLines).Count);
		}

		[TestDate(2020, 05, 29)]
		public void TestCombineLineInterface()
		{
			#region Setup Tariffs

			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
				tariff99038801.UE_DutyComputationCode = "7";
				tariff99038801.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038801.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038804 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038804")).LastOrDefault();
			if (tariff99038804 == null)
			{
				tariff99038804 = Factory.New<USCTariff>();
				tariff99038804.UE_Tariff = "99038804";
				tariff99038804.UE_DutyComputationCode = "7";
				tariff99038804.UE_Column1RateAdValorem = 0.15m;
			}
			tariff99038804.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038804.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff8517620020 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8517620020")).LastOrDefault();
			if (tariff8517620020 == null)
			{
				tariff8517620020 = Factory.New<USCTariff>();
				tariff8517620020.UE_Tariff = "8517620020";
				tariff8517620020.UE_DutyComputationCode = "7";
				tariff8517620020.UE_Column1RateAdValorem = 0.05m;
			}
			tariff8517620020.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff8517620020.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			#endregion

			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_CH_ReconEntry = originalEntry.CH_PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_R_OrigSupTariff = tariff99038801.UE_Tariff;
			invoiceLine.US_SupTariff = tariff99038801.UE_Tariff;

			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLine.PK;
			invoiceLineTwo.US_R_OrigTariff = tariff8517620020.UE_Tariff;
			invoiceLineTwo.JI_Tariff = tariff8517620020.UE_Tariff;
			invoiceLineTwo.US_R_OrigSupTariff = tariff99038804.UE_Tariff;
			invoiceLineTwo.US_SupTariff = tariff99038804.UE_Tariff;
			invoiceLineTwo.US_R_OrigCV = 10000m;
			invoiceLineTwo.JI_LinePrice = 11000m;
			Factory.Save();

			IEntryLineOrInvoiceLineDutyData originalDutyData = new ReconOriginalDutyData(invoiceLine, ZDateTime.Today);
			CombineAssertions("Current duty data on first invoice line", () =>
			{
				AssertEquals("Customs value on duty data is zero.", ZDecimal.Zero, originalDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is empty.", ZString.Empty, originalDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038801", originalDutyData.SupTariffs.Contains(tariff99038801.UE_Tariff));
				AssertNull("Duty data shouldn't have combine parent line.", originalDutyData.CombineParentLine);
				var childLines = originalDutyData.ChildLines.ToArray();
				AssertEquals("As sup tariff is not empty, duty data shouldn't have child lines.", 0, childLines.Length);
				var combineChildLines = originalDutyData.CombineChildLines.ToArray();
				AssertEquals("Duty data should hava one combine child line.", 1, combineChildLines.Length);
				AssertEquals("Tariff on combine child line is 8517620020", tariff8517620020.UE_Tariff, combineChildLines[0].Tariff);
				Assert("Sup tariff on combine child line is 99038804", combineChildLines[0].SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineAllLines = originalDutyData.CombineAllLines.ToArray();
				AssertEquals("Duty data should have four combine lines.", 4, combineAllLines.Length);
				AssertContainsExactElementsInAnyOrder(new[] { tariff99038801.UE_Tariff, tariff99038804.UE_Tariff, tariff8517620020.UE_Tariff }, combineAllLines.Where(x => !x.Tariff.IsEmpty).Select(y => y.Tariff));
			});

			originalDutyData = new ReconOriginalDutyData(invoiceLineTwo, ZDateTime.Today);
			CombineAssertions("Current duty data on second invoice line", () =>
			{
				AssertEquals("Customs value on duty data is 10000.", 10000m, originalDutyData.CustomsValue);
				AssertEquals("Tariff on duty data is 8517620020", tariff8517620020.UE_Tariff, originalDutyData.Tariff);
				Assert("Sup tariff on duty data is 99038804", originalDutyData.SupTariffs.Contains(tariff99038804.UE_Tariff));
				var combineParentLine = originalDutyData.CombineParentLine;
				AssertNotNull("Duty data should have combine parent line.", combineParentLine);
				AssertEquals("Tariff on combine parent line is empty", ZString.Empty, combineParentLine.Tariff);
				Assert("Sup tariff on combine parent line is 99038801", combineParentLine.SupTariffs.Contains(tariff99038801.UE_Tariff));
				var childLines = originalDutyData.ChildLines.ToArray();
				AssertEquals("This is child invoice line, duty data shouldn't have child lines.", 0, childLines.Length);
				var combineChildLines = originalDutyData.CombineChildLines.ToArray();
				AssertEquals("This is child invoice line, duty data shouldn't have combine child lines.", 0, combineChildLines.Length);
				var combineAllLines = originalDutyData.CombineAllLines.ToArray();
				AssertEquals("Duty data should have four combine lines.", 4, combineAllLines.Length);
				AssertContainsExactElementsInAnyOrder(new[] { tariff99038801.UE_Tariff, tariff99038804.UE_Tariff, tariff8517620020.UE_Tariff }, combineAllLines.Where(x => !x.Tariff.IsEmpty).Select(y => y.Tariff));
			});
		}

		public void TestTariffRelatedMembers()
		{
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;

			invoiceLine.JI_Tariff = USCTariff.DOTMayBeApplicable;
			AssertNotEquals("Original tariff & recon tariff should be different", originalData.ImportTariff, invoiceLine.ImportTariff);

			invoiceLine.US_R_OrigSPI = "SG";
			invoiceLine.OriginalImportTariff.UE_SPICode = "SG";//becomes duty-free
			invoiceLine.ImportTariff.UE_SPICode = "";//no SPI claims avaiable

			AssertEquals("Becomes duty free for original duty. Should use the original duty data set", true, originalData.IsDutyFreeSPIClaimed);

			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_CustomsUnitQty = "AA";
			invoiceLine.JI_CustomsSecondQuantity = 2m;
			invoiceLine.JI_CustomsSecondUnitQty = "BB";
			invoiceLine.JI_CustomsThirdQuantity = 3m;
			invoiceLine.JI_CustomsThirdUnitQty = "CC";

			invoiceLine.US_R_OrigFirstUQ = "DD";
			invoiceLine.US_R_OrigFirstQty = 4.525m;
			invoiceLine.US_R_OrigSecondUQ = "EE";
			invoiceLine.US_R_OrigSecondQty = 5.419m;
			invoiceLine.US_R_OrigThirdUQ = "FF";
			invoiceLine.US_R_OrigThirdQty = 6.499m;

			AssertEquals("Quantity1", 5m, originalData.Quantity1);
			AssertEquals("UQ1", "DD", originalData.UQ1);
			AssertEquals("Quantity2", 5m, originalData.Quantity2);
			AssertEquals("UQ2", "EE", originalData.UQ2);
			AssertEquals("Quantity3", 6m, originalData.Quantity3);
			AssertEquals("UQ3", "FF", originalData.UQ3);

			invoiceLine.US_R_OrigFirstUQ = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_R_OrigSecondUQ = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_R_OrigThirdUQ = ABIUnitOfMeasureList.Codes.ProofLiter;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 1.44m;

			invoiceLine.US_R_OrigTariff = tariff.UE_Tariff;
			invoiceLine.US_R_OrigFirstQty = 4.525m;
			invoiceLine.US_R_OrigSecondQty = 5.419m;
			invoiceLine.US_R_OrigThirdQty = 6.499m;

			originalData = new ReconOriginalDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);

			AssertEquals("Quantity1", 4.53m, originalData.Quantity1);

			invoiceLine.US_R_Textile = true;
			AssertEquals("Quantity1 should be rounded to whole number, because is textile", 5m, originalData.Quantity1);
			invoiceLine.US_R_Textile = false;

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			tariff.UE_Column2RateSpecific = 1.44m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CU";

			AssertEquals("Quantity2", 5.42m, originalData.Quantity2);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			dutyRate.UD_TaxFeeAdvalorem = 1.4m;
			dutyRate.UD_TaxFeeSpecificRate = 1.6m;

			AssertEquals("Quantity3", 6.50m, originalData.Quantity3);
		}

		ReconDeclaration reconDec;
		JobComInvoiceLine invoiceLine;
		ReconOriginalDutyData originalData;
		ReconOriginalEntryHeader originalEntry;

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			reconDec = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			originalEntry = reconDec.OriginalEntries[0];
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_DateForMPFCalc = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;

			invoiceLine = reconDec.InvoiceLines[0];
			originalData = new ReconOriginalDutyData(invoiceLine, originalEntry.US_R_DateForMPFCalc);
		}

		void SetReconOriginalValues(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigCV = invoiceLine.JI_CustomsValue >= invoiceLine.US_98GoodsValue ? (ZDecimal)(invoiceLine.JI_CustomsValue - invoiceLine.US_98GoodsValue) : invoiceLine.JI_CustomsValue;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_R_OrigSPI = invoiceLine.US_SPI;
			invoiceLine.US_R_OrigSecondQty = invoiceLine.JI_CustomsSecondQuantity;

			invoiceLine.US_R_OrigSupTariff = invoiceLine.US_SupTariff;
			invoiceLine.US_R_Orig98Value = invoiceLine.US_98GoodsValue;
			invoiceLine.US_R_OrigSupQty1 = invoiceLine.US_SupQty1;
			invoiceLine.US_R_OrigSupQty2 = invoiceLine.US_SupQty2;
			invoiceLine.US_R_OrigSupQty3 = invoiceLine.US_SupQty3;
		}

		void SetUpTariffsForTaxRelatedFields()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			USCTariff tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00000001";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_Unit1 = "L";

			USCTariffDutyRate dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			dutyRate2.UD_TaxFeeFlag = "2";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			USCTariff tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = "00000002";
			tariff3.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff3.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff3.UE_Unit1 = "PFL";

			USCTariffDutyRate dutyRate3 = tariff3.DutyRates.AddNew();
			dutyRate3.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DistilledSpirits;
			dutyRate3.UD_TaxFeeFlag = "2";
			dutyRate3.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			dutyRate3.UD_TaxFeeSpecificRate = 0.7m;
			dutyRate3.UD_TaxFeeAdvalorem = 0.8m;
		}
	}
}
