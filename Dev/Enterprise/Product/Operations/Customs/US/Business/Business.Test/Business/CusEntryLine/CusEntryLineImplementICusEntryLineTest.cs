using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusEntryLineImplementICusEntryLineTest : DeclarationTestHelper
	{
		public void TestSecondaryTariffLinesForChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entryHeader);

			var entryLine = entryHeader.EntryLines.FirstOrDefault(x => x.CL_AdValoremTariff == testHelper.Test99038501Tariff.UE_Tariff);
			AssertEquals(entryLine.ChildSecondaryEntryLines.Count, 3);

			var ientryLine = entryLine as ICusEntryLine;
			AssertNotNull(ientryLine);
			var secondaryTariffLines = ientryLine.SecondaryTariffLines.ToList();
			AssertEquals("Remove the empty line", secondaryTariffLines.Count, 2);
			AssertEquals(secondaryTariffLines[0].Tariff, testHelper.TestCTariff.UE_Tariff);
			AssertEquals(secondaryTariffLines[1].Tariff, testHelper.Test9802005060Tariff.UE_Tariff);
		}

		public void TestCountryOfOrigin()
		{
			InvoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertEquals("InvoiceLine.US_UC_NKCountryOfOrigin", "AU", InvoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("CountryOfOrigin", "", iEntryLine.CountryOfOrigin);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("CountryOfOrigin", "AU", iEntryLine.CountryOfOrigin);
		}

		public void TestADDSpecificDepositValue()
		{
			InvoiceLine.US_ADDCaseNo = "A";
			InvoiceLine.US_ADDDepositValue = 1.6m;
			AssertEquals("ADDSpecificDepositValue", ZDecimal.Zero, iEntryLine.ADDSpecificDepositValue);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("ADDSpecificDepositValue", 2m, iEntryLine.ADDSpecificDepositValue);

			InvoiceLine.US_ADDDepositValue = 0.49m;
			AssertEquals("ADDSpecificDepositValue", 1m, iEntryLine.ADDSpecificDepositValue);
		}

		public void TestCVDSpecificDepositValue()
		{
			InvoiceLine.US_CVDCaseNo = "C";
			InvoiceLine.US_CVDDepositValue = 1.2m;
			AssertEquals("CVDSpecificDepositValue", ZDecimal.Zero, iEntryLine.CVDSpecificDepositValue);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("CVDSpecificDepositValue", 1m, iEntryLine.CVDSpecificDepositValue);

			InvoiceLine.US_CVDDepositValue = 0.49m;
			AssertEquals("CVDSpecificDepositValue", 1m, iEntryLine.CVDSpecificDepositValue);
		}

		public void TestQuantityIsRoundedToZeroDecimalsForTextiles()
		{
			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();

			InvoiceLine.JI_Tariff = "5210516060";   // Textile code
			InvoiceLine.JI_CustomsQuantity = 1.234m;
			InvoiceLine.JI_CustomsSecondQuantity = 4.561m;
			InvoiceLine.JI_CustomsThirdQuantity = 7.889m;
			InvoiceLine.CusEntryLine.CL_AdValoremTariff = "5210516060";

			AssertEquals("Quantity1", 1m, iEntryLine.Quantity1);
			AssertEquals("Quantity2", 5m, iEntryLine.Quantity2);
			AssertEquals("Quantity3", 8m, iEntryLine.Quantity3);
		}

		public void TestZoneStatus()
		{
			InvoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			AssertEquals("ZoneStatus", "", iEntryLine.ZoneStatus);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("ZoneStatus", ZoneStatusList.Codes.PrivilegedForeign, iEntryLine.ZoneStatus);
		}

		public void TestPrivilegedStatusFilingDate()
		{
			InvoiceLine.US_PrivilegedStatusDate = new ZDate(2006, 12, 3);
			AssertEquals("PrivilegedStatusFilingDate", ZDate.Empty, iEntryLine.PrivilegedStatusFilingDate);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("PrivilegedStatusFilingDate", new ZDate(2006, 12, 3), iEntryLine.PrivilegedStatusFilingDate);
		}

		public void TestNAFTANetCostIndicator()
		{
			InvoiceLine.US_IsNAFTANet = true;
			AssertEquals("NAFTANetCostIndicator", ZBool.False, iEntryLine.NAFTANetCostIndicator);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("NAFTANetCostIndicator", ZBool.True, iEntryLine.NAFTANetCostIndicator);
		}

		public void TestFTZLineItemQuantity()
		{
			InvoiceLine.US_ManifestQty = 150;
			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			var invoiceLine2 = EntryLine.InvoiceLines.AddNew();
			invoiceLine2.US_ManifestQty = 250;
			AssertEquals("FTZLineItemQuantity", 400, iEntryLine.FTZLineItemQuantity);
		}

		public void TestImportTariff()
		{
			InvoiceLine.JI_Tariff = "1902.19.40 00";

			InvoiceLine.Declaration.US_EntryFilerCode = "XJ5";
			InvoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("ImportTariff", "1902194000", InvoiceLine.CusEntryLine.ImportTariff.UE_Tariff);
		}

		public void TestPreImportationReviewProgramRulingsNumber()
		{
			InvoiceLine.US_PIRPRulingNo = "PP234";
			AssertEquals("PreImportationReviewProgramRulingsNumber", ZString.Empty, iEntryLine.PreImportationReviewProgramRulingsNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("PreImportationReviewProgramRulingsNumber", "PP234", iEntryLine.PreImportationReviewProgramRulingsNumber);
		}

		public void TestCommercialDescriptions()
		{
			InvoiceLine.JI_Description = "Hello World";
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Goodbye World";
			AssertCollectionNotContains("InvoiceLine.JI_Description", "Hello World", iEntryLine.CommercialDescriptions);
			AssertCollectionNotContains("invoiceLine2.JI_Description", "Goodbye World", iEntryLine.CommercialDescriptions);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			invoiceLine2.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertCollectionContains("InvoiceLine.JI_Description", "Hello World", iEntryLine.CommercialDescriptions);
			AssertCollectionContains("invoiceLine2.JI_Description", "Goodbye World", iEntryLine.CommercialDescriptions);

			InvoiceLine.JI_ExtraInfoForClassification = "What A Lovely Day";
			invoiceLine2.JI_ExtraInfoForClassification = "What A Sad Day";
			AssertCollectionContains("InvoiceLine.JI_Description", "Hello World" + System.Environment.NewLine + "What A Lovely Day", iEntryLine.CommercialDescriptions);
			AssertCollectionContains("invoiceLine2.JI_Description", "Goodbye World" + System.Environment.NewLine + "What A Sad Day", iEntryLine.CommercialDescriptions);

			Declaration.US_EnableINB = true;
			InvoiceLine.JI_Description = "Test In-Bond Description";
			AssertEquals("InvoiceLine.JI_Description for In-Bond", "Test In-Bond Description", iEntryLine.Description);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			InvoiceLine.JI_Description = "SE line description";
			AssertEquals("InvoiceLine.JI_Description for Simplified Entry", "SE line description", iEntryLine.Description);
		}

		public void TestSpecialProgramsIndicatorPrimary()
		{
			InvoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.A;
			AssertEquals("US_SPI_Effective", PrimarySpecProgramIndicatorList.Codes.A, InvoiceLine.US_SPI);
			AssertEquals("SpecialProgramsIndicatorPrimary", "", iEntryLine.SpecialProgramsIndicatorPrimary);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("SpecialProgramsIndicatorPrimary", PrimarySpecProgramIndicatorList.Codes.A, iEntryLine.SpecialProgramsIndicatorPrimary);
		}

		public void TestQuantity1()
		{
			InvoiceLine.JI_CustomsQuantity = 1.2m;
			AssertEquals("Quantity1", ZDecimal.Zero, iEntryLine.Quantity1);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("Quantity1", 1m, iEntryLine.Quantity1);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 0.5m;

			EntryLine.CL_AdValoremTariff = tariff.UE_Tariff;
			AssertEquals("Quantity1 should not include decimals, because DutyComputationCode is 1 and UE_Column1RateSpecific less than 1",
					1m, iEntryLine.Quantity1);

			tariff.UE_Column1RateSpecific = 1.5m;

			AssertEquals("Quantity1 should include decimals, because DutyComputationCode is 1 and UE_Column1RateSpecific more than 1",
					1.2m, iEntryLine.Quantity1);

			InvoiceLine.JI_CustomsQuantity = 1.278m;
			AssertEquals("Quantity1 should include decimals", 1.28m, iEntryLine.Quantity1);

			InvoiceLine.JI_CustomsUnitQty = ABIUnitOfMeasureList.Codes.Dozen;
			InvoiceLine.JI_CustomsQuantity = 0.001m;
			AssertEquals("Quantity1", 1m, iEntryLine.Quantity1);
		}

		public void TestUnitOfMeasure1()
		{
			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("UnitOfMeasure1", "", iEntryLine.UnitOfMeasure1);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("UnitOfMeasure1", "NO", iEntryLine.UnitOfMeasure1);
		}

		public void TestQuantity2()
		{
			InvoiceLine.JI_CustomsSecondQuantity = 1.2m;
			AssertEquals("Quantity2", ZDecimal.Zero, iEntryLine.Quantity2);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("Quantity2", 1m, iEntryLine.Quantity2);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column2RateSpecific = 0.5m;

			EntryLine.RandomLine.JI_Tariff = tariff.UE_Tariff;
			EntryLine.RandomLine.JI_CustomsSecondQuantity = 1.2m;
			EntryLine.RandomLine.US_UC_NKCountryOfOrigin = "CU";
			EntryLine.CL_AdValoremTariff = tariff.UE_Tariff;

			AssertEquals("Quantity2 should not include decimals, because DutyComputationCode is 2 and UE_Column2RateSpecific less than 1",
						1m, iEntryLine.Quantity2);

			tariff.UE_Column2RateSpecific = 1.5m;

			AssertEquals("Quantity2 should include decimals, because DutyComputationCode is 2 and UE_Column2RateSpecific more than 1",
					1.2m, iEntryLine.Quantity2);
		}

		public void TestUnitOfMeasure2()
		{
			InvoiceLine.JI_CustomsSecondUnitQty = "NO";
			AssertEquals("UnitOfMeasure2", "", iEntryLine.UnitOfMeasure2);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("UnitOfMeasure2", "NO", iEntryLine.UnitOfMeasure2);
		}

		public void TestQuantity3()
		{
			InvoiceLine.JI_CustomsThirdQuantity = 1.2m;
			AssertEquals("Quantity3", ZDecimal.Zero, iEntryLine.Quantity3);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("Quantity3", 1m, iEntryLine.Quantity3);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000002";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_Unit1 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificCompound;
			dutyRate.UD_TaxFeeAdvalorem = 0.4m;
			dutyRate.UD_TaxFeeSpecificRate = 0.6m;

			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			EntryLine.CL_AdValoremTariff = tariff.UE_Tariff;

			AssertEquals("Quantity3 should not include decimals, because DutyComputationCode is E and UD_TaxFeeSpecificRate less than 1",
						1m, iEntryLine.Quantity3);

			dutyRate.UD_TaxFeeSpecificRate = 1.6m;
			AssertEquals("Quantity3 should include decimals, because DutyComputationCode is E and UD_TaxFeeSpecificRate more than 1",
						1m, iEntryLine.Quantity3);
		}

		public void TestUnitOfMeasure3()
		{
			InvoiceLine.JI_CustomsThirdUnitQty = "NO";
			AssertEquals("UnitOfMeasure3", "", iEntryLine.UnitOfMeasure3);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("UnitOfMeasure3", "NO", iEntryLine.UnitOfMeasure3);
		}

		public void TestCountryOfExport()
		{
			InvoiceHeader.US_UC_NKCountryOfExport = Australia.Code;
			AssertEquals("InvoiceLine.US_UC_NKCountryOfExport", Australia.Code, InvoiceLine.US_UC_NKCountryOfExport);
			AssertEquals("CountryOfExport", ZString.Empty, iEntryLine.CountryOfExport);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("CountryOfExport", Australia.Code, iEntryLine.CountryOfExport);
		}

		public void TestDateOfExportationAndPortOfLading()
		{
			Declaration.US_SchDLoading = "60267";
			InvoiceHeader.US_DateOfExport = new ZDate(2006, 4, 23);
			AssertEquals("InvoiceLine.US_DateOfExport", new ZDate(2006, 4, 23), InvoiceLine.US_DateOfExport);
			AssertEquals("InvoiceLine.US_SchDLoading", "60267", InvoiceLine.US_SchDLoading);
			AssertEquals("DateOfExportation", ZDate.Empty, iEntryLine.DateOfExportation);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("DateOfExportation", new ZDate(2006, 4, 23), iEntryLine.DateOfExportation);
			AssertEquals("PortOfLading", "60267", iEntryLine.PortOfLading);

			InvoiceLine.US_SchDLoading = "01520";
			EntryLine.RefreshInvoiceLines();
			AssertEquals("PortOfLading", "01520", iEntryLine.PortOfLading);
		}

		public void TestRelatedPartyIndicator()
		{
			InvoiceLine.InvoiceHeader.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("RelatedPartyIndicator", false, iEntryLine.RelatedPartyIndicator);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("RelatedPartyIndicator", true, iEntryLine.RelatedPartyIndicator);

			InvoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			AssertEquals("RelatedPartyIndicator overriden at line level", false, iEntryLine.RelatedPartyIndicator);
		}

		public void TestSpecialProgramsIndicatorCountry()
		{
			InvoiceLine.US_SPI = SpecialProgramList.Codes.IL;
			AssertEquals("US_SPI", SpecialProgramList.Codes.IL, InvoiceLine.US_SPI);
			AssertEquals("SpecialProgramsIndicatorCountry", "", iEntryLine.SpecialProgramsIndicatorCountry);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("SpecialProgramsIndicatorCountry", SpecialProgramList.Codes.IL, iEntryLine.SpecialProgramsIndicatorCountry);
		}

		public void TestSpecialProgramsIndicatorSecondary()
		{
			InvoiceLine.US_SecondarySPI = PrimarySpecProgramIndicatorList.Codes.R;
			AssertEquals("US_SecondarySPI_Effective", PrimarySpecProgramIndicatorList.Codes.R, InvoiceLine.US_SecondarySPI);
			AssertEquals("SpecialProgramsIndicatorSecondary", "", iEntryLine.SpecialProgramsIndicatorSecondary);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("SpecialProgramsIndicatorSecondary", PrimarySpecProgramIndicatorList.Codes.R, iEntryLine.SpecialProgramsIndicatorSecondary);
		}

		public void TestDateOfExportationFromCountryOfOrigin()
		{
			InvoiceHeader.US_DateOfExportFromCountryOfOrigin = new ZDate(2006, 3, 4);
			AssertEquals("InvoiceLine.US_DateOfExportFromCountryOfOrigin_Effective", new ZDate(2006, 3, 4), InvoiceLine.US_DateOfExportFromCountryOfOrigin);
			AssertEquals("DateOfExportationFromCountryOfOrigin", ZDate.Empty, iEntryLine.DateOfExportationFromCountryOfOrigin);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("DateOfExportationFromCountryOfOrigin", new ZDate(2006, 3, 4), iEntryLine.DateOfExportationFromCountryOfOrigin);
		}

		public void TestVisaNumber()
		{
			InvoiceLine.US_VisaNo = "HSV2342";
			AssertEquals("VisaNumber", "", iEntryLine.VisaNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("VisaNumber", "HSV2342", iEntryLine.VisaNumber);
		}

		public void TestTextileCategoryNumber()
		{
			InvoiceLine.US_TextileCategoryNo = "234";
			AssertEquals("TextileCategoryNumber", "", iEntryLine.TextileCategoryNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("TextileCategoryNumber", "234", iEntryLine.TextileCategoryNumber);
		}

		public void TestVisaQuantity()
		{
			InvoiceLine.US_VisaQty = 1.2m;
			AssertEquals("VisaQuantity", ZDecimal.Zero, iEntryLine.VisaQuantity);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("VisaQuantity", 1.2m, iEntryLine.VisaQuantity);
		}

		public void TestVisaUnitOfMeasure()
		{
			InvoiceLine.US_VisaUQ = "NO";
			AssertEquals("VisaUnitOfMeasure", ZString.Empty, iEntryLine.VisaUnitOfMeasure);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("VisaUnitOfMeasure", "NO", iEntryLine.VisaUnitOfMeasure);
		}

		public void TestAgricultureLicenseNumber()
		{
			InvoiceLine.US_AgricultureLicNo = "AG234";
			AssertEquals("AgricultureLicenseNumber", ZString.Empty, iEntryLine.AgricultureLicenseNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("AgricultureLicenseNumber", "AG234", iEntryLine.AgricultureLicenseNumber);
		}

		public void TestCottonCertificateNumberOrganicExemptionCertificateNumber()
		{
			InvoiceLine.US_CottonCertificateNo = "SD32";
			AssertEquals("CottonCertificateNumberOrganicExemptionCertificateNumber", ZString.Empty, iEntryLine.CottonCertificateNumberOrganicExemptionCertificateNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("CottonCertificateNumberOrganicExemptionCertificateNumber", "SD32", iEntryLine.CottonCertificateNumberOrganicExemptionCertificateNumber);
		}

		public void TestChinaHongKongSWPMIndicator()
		{
			InvoiceLine.US_SWPMIndicator = SWPMList.Codes._1;
			AssertEquals("ChinaHongKongSWPMIndicator", ZString.Empty, iEntryLine.ChinaHongKongSWPMIndicator);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			InvoiceLine.US_UC_NKCountryOfOrigin = "AU";
			AssertEquals("ChinaHongKongSWPMIndicator", ZString.Empty, iEntryLine.ChinaHongKongSWPMIndicator);
			InvoiceLine.US_UC_NKCountryOfOrigin = "CN";
			AssertEquals("ChinaHongKongSWPMIndicator", SWPMList.Codes._1, iEntryLine.ChinaHongKongSWPMIndicator);
		}

		public void TestCanadianExportCertificateSugar()
		{
			InvoiceLine.US_CAExportCertificate = "GS234";
			AssertEquals("CanadianExportCertificateSugar", ZString.Empty, iEntryLine.CanadianExportCertificateSugar);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("CanadianExportCertificateSugar", "GS234", iEntryLine.CanadianExportCertificateSugar);
		}

		public void TestWoolLicense()
		{
			InvoiceLine.US_WoolLicenceNo = "LDG43";
			AssertEquals("WoolLicense", ZString.Empty, iEntryLine.WoolLicense);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("WoolLicense", "LDG43", iEntryLine.WoolLicense);
		}

		public void TestCBTPACertificationNumber()
		{
			InvoiceLine.US_CBTPACertificateNo = "KG4345";
			AssertEquals("CBTPACertificationNumber", ZString.Empty, iEntryLine.CBTPACertificationNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("CBTPACertificationNumber", "KG4345", iEntryLine.CBTPACertificationNumber);
		}

		public void TestMiscellaneousPermitLicenseNumber()
		{
			InvoiceLine.US_MiscPermitNo = "OD4345";
			AssertEquals("MiscellaneousPermitLicenseNumber", ZString.Empty, iEntryLine.MiscellaneousPermitLicenseNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("MiscellaneousPermitLicenseNumber", "OD4345", iEntryLine.MiscellaneousPermitLicenseNumber);
		}

		public void TestIsSoftwoodLumberLine()
		{
			Factory.InvalidateCachedProperties();
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			InvoiceLine.JI_Tariff = "4407100115";
			AssertEquals("IsSoftwoodLumberLine", false, iEntryLine.IsSoftwoodLumberLine);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("IsSoftwoodLumberLine", true, iEntryLine.IsSoftwoodLumberLine);
		}

		public void TestIsSoftwoodLumberImporterDeclaration()
		{
			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine2 = EntryHeader.MergedLines.AddNew();
			AssertEquals("IsSoftwoodLumberImporterDeclaration", false, iEntryLine.IsSoftwoodLumberImporterDeclaration);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			invoiceLine2.JI_CL = EntryLine.PK;
			invoiceLine3.JI_CL = entryLine2.PK;
			EntryLine.RefreshInvoiceLines();
			entryLine2.RefreshInvoiceLines();
			AssertEquals("IsSoftwoodLumberImporterDeclaration", false, iEntryLine.IsSoftwoodLumberImporterDeclaration);

			InvoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			AssertEquals("IsSoftwoodLumberImporterDeclaration", true, iEntryLine.IsSoftwoodLumberImporterDeclaration);

			InvoiceLine.US_LumberImporterDeclaration = ZString.Empty;
			invoiceLine2.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			AssertEquals("IsSoftwoodLumberImporterDeclaration", true, iEntryLine.IsSoftwoodLumberImporterDeclaration);

			invoiceLine2.US_LumberImporterDeclaration = ZString.Empty;
			invoiceLine3.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			AssertEquals("IsSoftwoodLumberImporterDeclaration", true, ((ICusEntryLine)entryLine2).IsSoftwoodLumberImporterDeclaration);
		}

		public void TestSoftwoodLumberExportPrice()
		{
			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine2 = EntryHeader.MergedLines.AddNew();
			AssertEquals("SoftwoodLumberExportPrice", ZDecimal.Zero, iEntryLine.SoftwoodLumberExportPrice);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			invoiceLine2.JI_CL = EntryLine.PK;
			invoiceLine3.JI_CL = entryLine2.PK;
			EntryLine.RefreshInvoiceLines();
			entryLine2.RefreshInvoiceLines();
			AssertEquals("SoftwoodLumberExportPrice", ZDecimal.Zero, iEntryLine.SoftwoodLumberExportPrice);

			InvoiceLine.US_LumberExportPrice = 10m;
			AssertEquals("SoftwoodLumberExportPrice", 10m, iEntryLine.SoftwoodLumberExportPrice);

			invoiceLine2.US_LumberExportPrice = 22m;
			AssertEquals("SoftwoodLumberExportPrice", 32m, iEntryLine.SoftwoodLumberExportPrice);

			invoiceLine3.US_LumberExportPrice = 33m;
			AssertEquals("SoftwoodLumberExportPrice", 33m, ((ICusEntryLine)entryLine2).SoftwoodLumberExportPrice);
		}

		public void TestSoftwoodLumberExportPriceWithSupTariff()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.US_SupTariff = "9802004040";
			invoiceLine.JI_Tariff = "4407100115";
			invoiceLine.US_LumberExportPrice = 20m;
			invoiceLine.US_LumberExportCharges = 30m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(0m, ((ICusEntryLine)invoiceLine.CusEntryLine).SoftwoodLumberExportPrice);
			AssertEquals(0m, ((ICusEntryLine)invoiceLine.CusEntryLine).SoftwoodLumberExportCharges);

			AssertEquals(20m, ((ICusEntryLine)invoiceLine.CusEntryLine.ParentLine).SoftwoodLumberExportPrice);
			AssertEquals(30m, ((ICusEntryLine)invoiceLine.CusEntryLine.ParentLine).SoftwoodLumberExportCharges);

			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.26";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0m, ((ICusEntryLine)invoiceLine.CusEntryLine).SoftwoodLumberExportPrice);
			AssertEquals(0m, ((ICusEntryLine)invoiceLine.CusEntryLine).SoftwoodLumberExportCharges);

			AssertEquals(20m, ((ICusEntryLine)invoiceLine.CusEntryLine.ParentLine).SoftwoodLumberExportPrice);
			AssertEquals(30m, ((ICusEntryLine)invoiceLine.CusEntryLine.ParentLine).SoftwoodLumberExportCharges);
		}

		public void TestSoftwoodLumberExportCharges()
		{
			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryLine entryLine2 = EntryHeader.MergedLines.AddNew();
			AssertEquals("SoftwoodLumberExportCharges", ZDecimal.Zero, iEntryLine.SoftwoodLumberExportCharges);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			invoiceLine2.JI_CL = EntryLine.PK;
			invoiceLine3.JI_CL = entryLine2.PK;
			EntryLine.RefreshInvoiceLines();
			entryLine2.RefreshInvoiceLines();
			AssertEquals("SoftwoodLumberExportCharges", ZDecimal.Zero, iEntryLine.SoftwoodLumberExportCharges);

			InvoiceLine.US_LumberExportCharges = 10m;
			AssertEquals("SoftwoodLumberExportCharges", 10m, iEntryLine.SoftwoodLumberExportCharges);

			invoiceLine2.US_LumberExportCharges = 22m;
			AssertEquals("SoftwoodLumberExportCharges", 32m, iEntryLine.SoftwoodLumberExportCharges);

			invoiceLine3.US_LumberExportCharges = 33m;
			AssertEquals("SoftwoodLumberExportCharges", 33m, ((ICusEntryLine)entryLine2).SoftwoodLumberExportCharges);
		}

		public void TestCountervailingCaseNumber()
		{
			InvoiceLine.US_CVDCaseNo = "GS4345";
			AssertEquals("CountervailingCaseNumber", "", iEntryLine.CountervailingCaseNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("CountervailingCaseNumber", "GS4345", iEntryLine.CountervailingCaseNumber);
		}

		public void TestAntidumpingCaseNumber()
		{
			InvoiceLine.US_ADDCaseNo = "LK789";
			AssertEquals("AntidumpingCaseNumber", ZString.Empty, iEntryLine.AntidumpingCaseNumber);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("AntidumpingCaseNumber", "LK789", iEntryLine.AntidumpingCaseNumber);
		}

		public void TestManufacturerSupplierCode()
		{
			OrgCusCode cusCode = Consignor.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "CBP123", Core.Constants.CountryCodes.UnitedStates);
			InvoiceHeader.JZ_OH_Supplier = Consignor.PK;
			AssertEquals("InvoiceLine.Supplier_Effective", Consignor.PK, InvoiceLine.Supplier_Effective.PK);
			AssertEquals("ManufacturerSupplierCode", "", iEntryLine.ManufacturerSupplierCode);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("ManufacturerSupplierCode", "CBP123", iEntryLine.ManufacturerSupplierCode);
		}

		public void TestExciseTax()
		{
			AssertEquals("ExciseTax", ZDecimal.Zero, iEntryLine.ExciseTax);
			EntryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.DistilledSpirits).CF_ChargeAmount = 1.2m;
			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			AssertEquals("ExciseTax", 1.2m, iEntryLine.ExciseTax);
		}

		public void TestCVDDepositRate()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "C427819000";

			var rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDate.Today;
			rate.U6_AdValoremRate = .1215m;

			InvoiceLine.US_CVDCaseNo = "C427819000";
			AssertEquals("CVDDepositRate", ZDecimal.Zero, iEntryLine.CVDDepositRate);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("CVDDepositRate", .1215m, iEntryLine.CVDDepositRate);
		}

		public void TestADDDepositRate()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A427818000";

			var rate = uscCase.CaseRates.AddNew();
			rate.U6_EffectiveDate = ZDate.Today;
			rate.U6_AdValoremRate = .1995m;

			InvoiceLine.US_ADDCaseNo = "A427818000";
			AssertEquals("ADDDepositRate", ZDecimal.Zero, iEntryLine.ADDDepositRate);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("ADDDepositRate", .1995m, iEntryLine.ADDDepositRate);
		}

		public void TestICusEntryLineGrossWeightInKG()
		{
			InvoiceLine.JI_Weight = 1.2m;
			InvoiceLine.JI_WeightUQ = "KG";

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();

			ICusEntryLine ientryLine = EntryLine;
			AssertEquals(1m, ientryLine.GrossWeightInKilograms);

			InvoiceLine.JI_Weight = 1.6m;
			AssertEquals(2m, ientryLine.GrossWeightInKilograms);
		}

		public void TestGrossWeightIsRolledUpFromSecondaryNonVLines()
		{
			InvoiceLine.Declaration.US_EntryFilerCode = "XJ5";
			InvoiceLine.JI_Weight = 1.4m;
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			JobComInvoiceLine invoiceLineSecondary = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLineSecondary.JI_ParentID = InvoiceLine.PK;

			invoiceLineSecondary.JI_Weight = 3m;
			invoiceLineSecondary.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			InvoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine ientryLine = InvoiceLine.CusEntryLine;
			AssertEquals(4m, ientryLine.GrossWeightInKilograms);

			InvoiceLine.JI_Weight = 1.5m;
			AssertEquals(5m, ientryLine.GrossWeightInKilograms);

			InvoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals(2m, ientryLine.GrossWeightInKilograms);
		}

		public void TestBondedCountervailingDuty()
		{
			InvoiceLine.US_CVDCaseNo = "A";
			InvoiceLine.US_IsBondedCVD = ZBool.True;
			AssertEquals("BondedCountervailingDuty", ZBool.False, iEntryLine.BondedCountervailingDuty);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("BondedCountervailingDuty", ZBool.True, iEntryLine.BondedCountervailingDuty);
		}

		public void TestBondedAntidumpingDuty()
		{
			InvoiceLine.US_ADDCaseNo = "A";
			InvoiceLine.US_IsBondedADD = ZBool.True;
			AssertEquals("BondedAntidumpingDuty", ZBool.False, iEntryLine.BondedAntidumpingDuty);

			EntryHeader.CH_JE = Declaration.PK;
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("BondedAntidumpingDuty", ZBool.True, iEntryLine.BondedAntidumpingDuty);
		}

		public void TestRandomLine()
		{
			JobComInvoiceLine invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "123";
			invoiceLine2.JI_CL = EntryLine.PK;
			JobComInvoiceLine invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "456";
			invoiceLine3.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();

			EntryLine.CL_AdValoremTariff = "123";
			EntryLine.Header.CH_JE = Declaration.PK;
			AssertEquals("123", EntryLine.RandomLine.JI_Tariff);
			EntryLine.CL_AdValoremTariff = "456";
			EntryLine.ResetTotalsAndCachedValues();
			AssertEquals("456", EntryLine.RandomLine.JI_Tariff);
		}

		public void TestFees()
		{
			EntryLine.Fees.RemoveAndDeleteAll();
			int cottonFeeCount = 0;
			int watermelonFeeCount = 0;
			int mushroomFeeCount = 0;
			EntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 23m);
			EntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Watermelon, 32m);
			EntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Mushroom, 85m);

			foreach (IFee fee in iEntryLine.Fees)
			{
				if (fee.Code == Core.Constants.USCustoms.FeeCodes.Cotton)
				{
					AssertEquals("Amount", 23m, fee.Amount);
					cottonFeeCount++;
				}
				else if (fee.Code == Core.Constants.USCustoms.FeeCodes.Watermelon)
				{
					AssertEquals("Amount", 32m, fee.Amount);
					watermelonFeeCount++;
				}
				else if (fee.Code == Core.Constants.USCustoms.FeeCodes.Mushroom)
				{
					AssertEquals("Amount", 85m, fee.Amount);
					mushroomFeeCount++;
				}
			}

			AssertEquals("CottonFeeCount", 1, cottonFeeCount);
			AssertEquals("WatermelonFeeCount", 1, watermelonFeeCount);
			AssertEquals("MushroomFeeCount", 1, mushroomFeeCount);

			var iACECusEntryLine = EntryLine as IACECusEntryLine;
			AssertEquals(ZString.Empty, iACECusEntryLine.FeeExemptionCode);

			EntryLine.RandomLine.US_CottonFeeExempt = YesNoDefaultList.Codes.Yes;
			EntryLine.RandomLine.US_CottonCertificateNo = "AMS0019";
			AssertEquals("1", iACECusEntryLine.FeeExemptionCode);
		}

		public void TestTariffFormattedTariffReturnsHighestDutyTariff()
		{
			EntryLine.CL_AdValoremTariff = "8471704065";
			AssertEquals("8471.70.4065", EntryLine.FormattedTariff);
		}

		public void TestAdditionalDeclarationTypeCode()
		{
			InvoiceLine.JI_CL = EntryLine.PK;
			EntryLine.RefreshInvoiceLines();
			EntryLine.RandomLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			var iACECusEntryLine = EntryLine as IACECusEntryLine;
			var additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(0, additionalDeclarationDetails.Count);

			EntryLine.RandomLine.US_ExclusionNumber = "STL123456";
			additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(1, additionalDeclarationDetails.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._02, additionalDeclarationDetails[0].Key);
			AssertEquals("STL123456", additionalDeclarationDetails[0].Value);

			InvoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes.KR, "1234567890");
			additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(2, additionalDeclarationDetails.Count);

			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._04, additionalDeclarationDetails[1].Key);
			AssertEquals("1234567890", additionalDeclarationDetails[1].Value);

			InvoiceLine.US_Prim_NA = true;
			InvoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.Russia;
			InvoiceLine.US_Sec_NA = true;
			InvoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.China;
			InvoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Canada;

			additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(3, additionalDeclarationDetails.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._07, additionalDeclarationDetails[2].Key);
			AssertEquals("N/ARUN/ACNCA", additionalDeclarationDetails[2].Value);

			EntryLine.RandomLine.US_ADD_Cert = true;
			additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(4, additionalDeclarationDetails.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._06, additionalDeclarationDetails[2].Key);

			InvoiceLine.US_RN_NKMeltCtry = Core.Constants.CountryCodes.Canada;
			additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(5, additionalDeclarationDetails.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._08, additionalDeclarationDetails[4].Key);
			AssertEquals(Core.Constants.CountryCodes.Canada, additionalDeclarationDetails[4].Value);

			InvoiceLine.US_RN_NKMeltCtry = "ZZ";
			additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(5, additionalDeclarationDetails.Count);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._08, additionalDeclarationDetails[4].Key);
			AssertEquals("  OTH", additionalDeclarationDetails[4].Value);
		}

		public void TestAdditionalDeclarationDetails_05()
		{
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "THE MANUFACTURER";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("IsCBMA23Effective", false, invoiceLine.IsCBMA23Effective);

			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.US_ControlledGroupName = "CGN23";
			invoiceLine.US_FPI = "BACMEBR150520";
			invoiceLine.US_AllocationQuantity = 100m;
			invoiceLine.US_FlavorContentCreditInd = ZBool.True;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine.US_TaxRateS = invoiceLine.AddInfoLookups.TaxRateList[0].Code;
			invoiceLine.US_TTBRateDesignationCode = "W01010";
			invoiceLine.US_CBMADefaultTaxRate = 0.01849200m;
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine2.US_ControlledGroupName = "CGN23";
			invoiceLine2.US_AllocationQuantity = 200m;
			invoiceLine2.US_FPI = "BACMEBR150520";
			invoiceLine2.US_FlavorContentCreditInd = ZBool.True;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine2.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine2.US_TaxRateS = invoiceLine2.AddInfoLookups.TaxRateList[0].Code;
			invoiceLine2.US_TTBRateDesignationCode = "W01010";
			invoiceLine2.US_CBMADefaultTaxRate = 0.01849200m;
			entryLine.RefreshInvoiceLines();
			IACECusEntryLine aceEntryLine = entryLine;

			var cbmaData = aceEntryLine.AdditionalDeclarationDetails.Single(x => x.Key == AdditionalDeclarationTypeCodeList.Codes._05);
			AssertEquals("CGN23     BACMEBR150520     THE MANUFACTURER     000001000000YB0101000160000", cbmaData.Value);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			AssertEquals("IsCBMA23Effective", true, invoiceLine.IsCBMA23Effective);
			cbmaData = aceEntryLine.AdditionalDeclarationDetails.Single(x => x.Key == AdditionalDeclarationTypeCodeList.Codes._05);
			AssertEquals("XXXXXXXXXXBACMEBR150520     THE MANUFACTURER     000000000000YW0101000000000", cbmaData.Value);
		}

		public void TestCBMAForCombinedLines()
		{
			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038801.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff99038802 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038802")).LastOrDefault();
			if (tariff99038802 == null)
			{
				tariff99038802 = Factory.New<USCTariff>();
				tariff99038802.UE_Tariff = "99038802";
			}
			tariff99038802.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038802.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff7001001000 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "7001001000")).LastOrDefault();
			if (tariff7001001000 == null)
			{
				tariff7001001000 = Factory.New<USCTariff>();
				tariff7001001000.UE_Tariff = "7001001000";
				tariff7001001000.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
				tariff7001001000.UE_Column1RateAdValorem = 0.25m;
			}
			tariff7001001000.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff7001001000.UE_DateTo = new ZDateTime(2099, 1, 1);

			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer1.OH_FullName = "THE MANUFACTURER";
			manufacturer1.OH_Code = "TMAN1234";

			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer2.OH_FullName = "THE MANUFACTURER 2";
			manufacturer2.OH_Code = "TMAN5678";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2022, 01, 10);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.China;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine01 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("IsCBMA23Effective", false, invoiceLine01.IsCBMA23Effective);
			invoiceLine01.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine01.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;
			invoiceLine01.US_ControlledGroupName = "CGN23";
			invoiceLine01.US_FPI = "BACMEBR150520";
			invoiceLine01.US_AllocationQuantity = 100m;
			invoiceLine01.US_FlavorContentCreditInd = ZBool.True;
			invoiceLine01.US_IsParent = true;
			invoiceLine01.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine01.US_TaxCode = Core.Constants.USCustoms.FeeCodes.OtherExcise;
			invoiceLine01.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine01.US_TaxRateS = invoiceLine01.AddInfoLookups.TaxRateList[0].Code;
			invoiceLine01.US_TTBRateDesignationCode = "W01010";
			invoiceLine01.US_CBMADefaultTaxRate = 0.01849200m;

			var invoiceLine02 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine02.US_SupTariff = tariff99038802.UE_Tariff;
			invoiceLine02.JI_Tariff = tariff7001001000.UE_Tariff;
			invoiceLine02.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;
			invoiceLine02.US_ControlledGroupName = "CGN24";
			invoiceLine02.US_FPI = "WCHATEA3310021";
			invoiceLine02.US_AllocationQuantity = 200m;
			invoiceLine02.US_FlavorContentCreditInd = ZBool.False;
			invoiceLine02.JI_ParentID = invoiceLine01.PK;
			invoiceLine02.JI_ParentLine = invoiceLine01.JI_LineNo;
			invoiceLine02.JI_LinePrice = 20000m;
			invoiceLine02.US_TaxApply = TaxApplyList.Codes.Override;
			invoiceLine02.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine02.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.C;
			invoiceLine02.US_TaxRateS = invoiceLine02.AddInfoLookups.TaxRateList[0].Code;
			invoiceLine02.US_TTBRateDesignationCode = "W01010";
			invoiceLine02.US_CBMADefaultTaxRate = 0.01849200m;

			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var iACECusEntryLine = declaration.CustomsEntryHeaders[0].EntryLines.FirstOrDefault(x => x.US_SupLine) as IACECusEntryLine;
			var additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.Where(x => x.Key == AdditionalDeclarationTypeCodeList.Codes._05).ToList();
			AssertEquals(1, additionalDeclarationDetails.Count);
			AssertEquals("CGN23     BACMEBR150520     THE MANUFACTURER     000001000000YB0101000160000", additionalDeclarationDetails[0].Value);

			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2023, 01, 10);
			AssertEquals("IsCBMA23Effective", true, invoiceLine01.IsCBMA23Effective);
			iACECusEntryLine = declaration.CustomsEntryHeaders[0].EntryLines.FirstOrDefault(x => x.US_SupLine);
			additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.Where(x => x.Key == AdditionalDeclarationTypeCodeList.Codes._05).ToList();
			AssertEquals(1, additionalDeclarationDetails.Count);
			AssertEquals("XXXXXXXXXXBACMEBR150520     THE MANUFACTURER     000000000000YW0101000000000", additionalDeclarationDetails[0].Value);
		}

		public void TestExclusionNumbersForCombinedLines()
		{
			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038801.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff99038802 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038802")).LastOrDefault();
			if (tariff99038802 == null)
			{
				tariff99038802 = Factory.New<USCTariff>();
				tariff99038802.UE_Tariff = "99038802";
			}
			tariff99038802.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038802.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff7001001000 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "7001001000")).LastOrDefault();
			if (tariff7001001000 == null)
			{
				tariff7001001000 = Factory.New<USCTariff>();
				tariff7001001000.UE_Tariff = "7001001000";
				tariff7001001000.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
				tariff7001001000.UE_Column1RateAdValorem = 0.25m;
			}
			tariff7001001000.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff7001001000.UE_DateTo = new ZDateTime(2099, 1, 1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.China;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine01 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine01.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine01.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine01.US_ExclusionNumber = "STL000001";
			invoiceLine01.US_IsParent = true;

			var invoiceLine02 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine02.US_SupTariff = tariff99038802.UE_Tariff;
			invoiceLine02.JI_Tariff = tariff7001001000.UE_Tariff;
			invoiceLine02.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			invoiceLine02.US_ExclusionNumber = "ALU000001";
			invoiceLine02.JI_ParentID = invoiceLine01.PK;
			invoiceLine02.JI_ParentLine = invoiceLine01.JI_LineNo;
			invoiceLine02.JI_LinePrice = 20000m;

			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var iACECusEntryLine = declaration.CustomsEntryHeaders[0].EntryLines.FirstOrDefault(x => x.US_SupLine) as IACECusEntryLine;
			var additionalDeclarationDetails = iACECusEntryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(2, additionalDeclarationDetails.Count);
			AssertEquals("STL000001", additionalDeclarationDetails[0].Value);
			AssertEquals("ALU000001", additionalDeclarationDetails[1].Value);
		}

		public void TestAdditionalDeclarationDetailsForInvocieLineWithMultipleSupTariffs()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "7601.10.3000";
			invoiceLine.SupTariffFormatted = "9903.85.01";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.88.21";
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.US_UC_NKCountryOfOrigin =  Core.Constants.CountryCodes.China;
			invoiceLine.US_Prim_NA = true;
			invoiceLine.US_Sec_NA = true;
			invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.China;

			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._28, "23333");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.FormalEntry;
			var entryLine = (IACECusEntryLine)entry.MergedLines.Find(x => x.CL_AdValoremTariff == "99038802").First();
			var additionalDeclarationDetails = entryLine.AdditionalDeclarationDetails.ToList();
			AssertEquals(1, additionalDeclarationDetails.Count);
			AssertEquals("N/A  N/A  CN", additionalDeclarationDetails[0].Value);
		}

		public void TestISimplifiedEntryLineMembers()
		{
			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_Code = "UC" + new Random().Next(1000000).ToString();
			InvoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer For Line 1";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			InvoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Seller";
			seller.OH_Code = "SE" + new Random().Next(1000000).ToString();
			InvoiceLine.JI_OA_Seller = seller.MainAddress.PK;

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "Sold To Party";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			InvoiceLine.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var shipToParty = Factory.New<OrgHeader>();
			shipToParty.OH_FullName = "SHIP TO PARTY";
			shipToParty.OH_Code = "SIP" + new Random().Next(1000000).ToString();
			InvoiceLine.JI_OA_ShipToPartyAddress = shipToParty.MainAddress.PK;

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "Exporter";
			exporter.OH_Code = "EXP" + new Random().Next(1000000).ToString();
			InvoiceHeader.JZ_OA_ExporterAddress = exporter.MainAddress.PK;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Shipper";
			shipper.OH_Code = "SHP" + new Random().Next(1000000).ToString();
			InvoiceHeader.JZ_OA_ShipperAddress = shipper.MainAddress.PK;

			var distributor = Factory.New<OrgHeader>();
			distributor.OH_FullName = "Distributor";
			distributor.OH_Code = "DIS" + new Random().Next(1000000).ToString();
			InvoiceHeader.JZ_OA_DistributorAddress = distributor.MainAddress.PK;

			var packager = Factory.New<OrgHeader>();
			packager.OH_FullName = "Packager";
			packager.OH_Code = "DIS" + new Random().Next(1000000).ToString();
			InvoiceHeader.JZ_OA_PackagerAddress = packager.MainAddress.PK;

			Declaration.ActiveEntryHeaders.Add(EntryHeader);
			InvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(EntryLine);
			InvoiceLine.US_ManifestQty = 12345;
			InvoiceLine.US_FTZCurrentTariff = "2402108850";
			var iSimplifiedEntryLine = (ISimplifiedEntryLine)EntryLine;
			AssertEquals("FTZ Line Item Quantity", 12345, iSimplifiedEntryLine.FTZLineItemQuantity);
			AssertEquals("FTZ Current Tariff", "2402108850", iSimplifiedEntryLine.FTZCurrentTariff);
			AssertEquals("Entities", 9, iSimplifiedEntryLine.Entities.Count());
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}

		ICusEntryLine iEntryLine => EntryLine;

		CusEntryLine entryLine;
		CusEntryLine EntryLine => entryLine ?? (entryLine = EntryHeader.MergedLines.AddNew());

		CusEntryHeader entryHeader;
		CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = Factory.New<CusEntryHeader>());

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
				}
				return declaration;
			}
		}

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (invoiceHeader == null)
				{
					invoiceHeader = Declaration.Invoices.AddNew();
					invoiceHeader.JZ_RX_NKInvoice_Currency = USD.RX_Code;
				}

				return invoiceHeader;
			}
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
	}
}
