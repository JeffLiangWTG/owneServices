using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.DDPDisbursementCalculation;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DDPDisbursementLineDutyDataTest : DDPDisbursementDutyDataBaseTest
	{
		public void TestGetOrSetDutyFeeCharge()
		{
			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount("AAA", 10m, new FeeCalculationInternalData());

			AssertEquals(10m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount("AAA"));

			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 20m, new FeeCalculationInternalData());

			AssertEquals(20m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
		}

		[TestDate(2008, 1, 1)]
		public void TestSetFeeResultForCottonFee()
		{
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine.JI_CustomsSecondQuantity = 500m;

			JobComInvoiceLine line2 = invoiceLine.Declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = USCTariff.CottonFeeApplicable;
			line2.JI_CustomsSecondQuantity = 50m;

			invoiceLine.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Both lines are merged into the same entry line", invoiceLine.JI_CL, line2.JI_CL);

			//The total cotton fee is less than threshold
			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 1.03m, new FeeCalculationInternalData());
			AssertEquals(0m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			line2.JI_CustomsSecondQuantity = 500m;//The total is 2.06
			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 1.03m, new FeeCalculationInternalData());
			AssertEquals(1.03m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine.JI_CL, line2.JI_CL);

			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 1.03m, new FeeCalculationInternalData());
			AssertEquals(0m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		[TestDate(2017, 1, 1)]
		public void TestSetFeeResultForCottonFeeCancelThreshold()
		{
			invoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			invoiceLine.Declaration.US_EntryFilerCode = "XJ5";
			invoiceLine.JI_Tariff = USCTariff.CottonFeeApplicable;
			invoiceLine.JI_CustomsSecondQuantity = 500m;

			JobComInvoiceLine line2 = invoiceLine.Declaration.InvoiceLines.AddNew();
			line2.JI_Tariff = USCTariff.CottonFeeApplicable;
			line2.JI_CustomsSecondQuantity = 50m;

			invoiceLine.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Both lines are merged into the same entry line", invoiceLine.JI_CL, line2.JI_CL);

			//The total cotton fee is less than threshold
			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 1.03m, new FeeCalculationInternalData());
			AssertEquals(1.03m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			line2.JI_CustomsSecondQuantity = 500m;//The total is 2.06
			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 1.03m, new FeeCalculationInternalData());
			AssertEquals(1.03m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));

			invoiceLine.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(invoiceLine.JI_CL, line2.JI_CL);

			((IEntryLineOrInvoiceLineDutyData)dutyData).SetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 1.03m, new FeeCalculationInternalData());
			AssertEquals(1.03m, ((IEntryLineOrInvoiceLineDutyData)dutyData).GetDutyFeeChargeAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
		}

		public void TestSecondaryLines()
		{
			invoiceLine.AddSecondaryInvoiceLine();
			bool hasSecondaryLines = false;
			foreach (IEntryLineOrInvoiceLineDutyData secondaryDutyData in ((IEntryLineOrInvoiceLineDutyData)dutyData).SecondaryLines)
			{
				hasSecondaryLines = true;
				AssertEquals(typeof(DDPDisbursementLineDutyData), secondaryDutyData.GetType());
			}

			Assert(hasSecondaryLines);
		}

		public void TestSecondaryLines2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8457.20.0010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			chargesAndFees = new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>();
			customsValues = new Dictionary<JobComInvoiceLine, CustomsValues>();
			var dutyData = new DDPDisbursementLineDutyData(invoiceLine, chargesAndFees, customsValues, true);
			Assert("IsSecondaryLine of its own sup line", dutyData.IsSecondaryTariffLine);
			AssertEquals(0, ((IFeeCalculationDataProvider)dutyData).SecondaryLines.Count());

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9102.11.1010";
			AssertEquals("PreCondition:SecondaryTariffLines are added", 3, invoiceLine2.SecondaryTariffLines.Count());
			var dutyData2 = new DDPDisbursementLineDutyData(invoiceLine2, chargesAndFees, customsValues, true);

			Assert(!dutyData2.IsSecondaryTariffLine);
			var firstChildLine = invoiceLine2.SecondaryTariffLines.ElementAt(0);
			firstChildLine.US_SupTariff = "9802.00.8068";
			AssertEquals("include supData for the first child line", 4, ((IFeeCalculationDataProvider)dutyData2).SecondaryLines.Count());
		}

		public void TestIDutyDataMembers()
		{
			invoiceLine.US_SelectedRateType = RateTypeList.Codes.Primary;
			AssertEquals("SelectedRateType", RateTypeList.Codes.Primary, iDutyData.SelectedRateType);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.F;
			AssertEquals("SpecialProgramsIndicatorSecondary", SecondarySpecProgIndicatorList.Codes.F, iDutyData.SpecialProgramsIndicatorSecondary);

			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.E;
			AssertEquals("SpecialProgramsIndicatorPrimary", PrimarySpecProgramIndicatorList.Codes.E, iDutyData.SpecialProgramsIndicatorPrimary);
			AssertEquals("SpecialProgramsIndicatorCountry", ZString.Empty, iDutyData.SpecialProgramsIndicatorCountry);

			invoiceLine.US_SPI = SpecialProgramList.Codes.AU;
			AssertEquals("SpecialProgramsIndicatorCountry", SpecialProgramList.Codes.AU, iDutyData.SpecialProgramsIndicatorCountry);
			AssertEquals("SpecialProgramsIndicatorPrimary", ZString.Empty, iDutyData.SpecialProgramsIndicatorPrimary);

			invoiceLine.US_UC_NKCountryOfOrigin = "TT";
			AssertEquals("CountryOfOrigin", "TT", iDutyData.CountryOfOrigin);

			invoiceLine.JI_CustomsQuantity = 2m;
			AssertEquals("Quantity1", 2m, iDutyData.Quantity1);

			invoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals("UQ1", "KG", iDutyData.UQ1);

			invoiceLine.JI_CustomsSecondQuantity = 4m;
			AssertEquals("Quantity2", 4m, iDutyData.Quantity2);

			invoiceLine.JI_CustomsSecondUnitQty = "LT";
			AssertEquals("UQ2", "LT", iDutyData.UQ2);

			invoiceLine.JI_CustomsThirdQuantity = 8m;
			AssertEquals("Quantity3", 8m, iDutyData.Quantity3);

			invoiceLine.JI_CustomsThirdUnitQty = "NO";
			AssertEquals("UQ3", "NO", iDutyData.UQ3);

			invoiceLine.JI_Tariff = new TariffFormatter().DisplayFormat(USCTariff.CottonFeeApplicable);
			AssertEquals("Tariff", USCTariff.CottonFeeApplicable, iDutyData.Tariff);

			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			AssertEquals(false, dutyData.IsAMSFeeExempt);
			AssertEquals(true, dutyData.IsCottonFeeExemptIndicated);

			invoiceLine.US_CottonCertificateNo = "ORGANICXX";
			AssertEquals(true, dutyData.IsAMSFeeExempt);

			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals(true, dutyData.IsSetVLine);
		}

		public void TestIFeeCalculationDataProvidersMembers()
		{
			FeeCusCodeData overriden = invoiceLine.FeeCusCodes.AddNew("AAA", "");
			overriden.CY_IsOverridden = true;

			AssertEquals("IsOverriden", true, dutyData.IsFeeOverriden("AAA"));
			AssertEquals(false, dutyData.IsFeeOverriden("BBB"));

			overriden.CY_SelectedRateType = "P";
			AssertEquals("P", dutyData.GetSelectedRateType("AAA"));
			AssertEquals("", dutyData.GetSelectedRateType(""));

			CustomsValues values = new CustomsValues();
			values.CustomsValue = 100m;
			values.SupCustomsValue = 5m;
			customsValues.Add(invoiceLine, values);
			AssertEquals(100m, dutyData.CustomsValueForMPFCalculation);

			var supDutyData = new DDPDisbursementSupDutyData(invoiceLine, chargesAndFees, customsValues, true);
			AssertEquals(5m, supDutyData.CustomsValueForMPFCalculation);

			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_1;
			AssertEquals("L", dutyData.OverriddenTaxRateUQ);
			AssertEquals("", supDutyData.OverriddenTaxRateUQ);

			invoiceLine.US_TaxQty = 3248.38m;
			AssertEquals(3248.38m, dutyData.TaxRateQuantity);
			AssertEquals(0m, supDutyData.TaxRateQuantity);
		}

		[TestDate(2009, 6, 1)]
		public void TestDDPForDomesticMerchandise()
		{
			invoiceLine.InvoiceHeader.JZ_IncoTerm = "DDP";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Disbursement charge for DDP", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals(25.00m, invoiceLine.ApportionedCharges[0].J7_Amount);

			invoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("No Disbursement charges for DDP defaulted as this is a domestic merchandise", 0m, invoiceLine.ApportionedCharges[0].J7_Amount);
		}

		public void TestParentLine()
		{
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038803", "7", 0.25m, "");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineOne = invoice.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "8206000000";
			invoiceLineOne.US_SupTariff = "99038803";

			chargesAndFees = new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>();
			customsValues = new Dictionary<JobComInvoiceLine, CustomsValues>();
			var lineDutyData = (IEntryLineOrInvoiceLineDutyData)new DDPDisbursementLineDutyData(invoiceLineOne, chargesAndFees, customsValues, true);
			AssertEquals("8206000000", lineDutyData.Tariff);
			AssertEquals(1, lineDutyData.SupTariffs.Count);
			Assert(lineDutyData.SupTariffs.Contains("99038803"));

			var parentLineData = lineDutyData.ParentLine;
			AssertNotNull(parentLineData);
			AssertEquals("99038803", parentLineData.Tariff);
			AssertEquals(1, parentLineData.SupTariffs.Count);
			Assert(parentLineData.SupTariffs.Contains("99038803"));

			invoiceLineOne.US_SupTariff = ZString.Empty;
			var invoiceLineTwo = invoice.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_Tariff = "8205595510";
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			lineDutyData = new DDPDisbursementLineDutyData(invoiceLineTwo, chargesAndFees, customsValues, true);
			AssertEquals("8205595510", lineDutyData.Tariff);
			AssertEquals(0, lineDutyData.SupTariffs.Count);

			parentLineData = lineDutyData.ParentLine;
			AssertNotNull(parentLineData);
			AssertEquals("8206000000", parentLineData.Tariff);
			AssertEquals(0, parentLineData.SupTariffs.Count);
		}

		internal override DDPDisbursementDutyDataBase GetDDPDisbursementDutyDataForTest(JobComInvoiceLine invoiceLine) => new DDPDisbursementLineDutyData(invoiceLine, null, null, true);

		protected override void SetUp()
		{
			base.SetUp();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			chargesAndFees = new Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>>();
			customsValues = new Dictionary<JobComInvoiceLine, CustomsValues>();

			dutyData = new DDPDisbursementLineDutyData(invoiceLine, chargesAndFees, customsValues, true);

			iDutyData = dutyData;
		}

		JobComInvoiceLine invoiceLine;
		DDPDisbursementLineDutyData dutyData;
		IDutyData iDutyData;

		Dictionary<JobComInvoiceLine, Dictionary<string, DDPCalculationResultData>> chargesAndFees;
		Dictionary<JobComInvoiceLine, CustomsValues> customsValues;
	}
}
