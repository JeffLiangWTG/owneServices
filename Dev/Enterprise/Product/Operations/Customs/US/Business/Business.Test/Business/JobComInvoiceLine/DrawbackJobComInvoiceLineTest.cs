using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	partial class JobComInvoiceLineTest
	{
		public void TestCloneAllCollections()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.US_EntryType = ACEDrawbackProvisionsList.Codes._76;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var nafta = invoiceLine.DrawbackNAFTAs.AddNew();
			nafta.US_DRWNAFTACountryImportEntry = "EN001";
			nafta.US_DRWNAFTACountryImportEntryDate = new ZDateTime(2025, 01, 01);
			nafta.US_DRWNAFTACountryTariffNumber = "88591201";
			nafta.US_DRWNAFTACountryTariffNumber2 = "88591202";
			nafta.US_DRWNAFTACountryTariffNumber3 = "88591203";
			nafta.US_DRWNAFTACountryDutyRate = 12m;
			nafta.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 100m;
			nafta.US_DRWNAFTACountryOfExport = "CA";
			nafta.US_DRWNAFTACountryImportDuty = 1m;
			var additionalTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			additionalTariff.US_LineNo = 1;
			additionalTariff.US_FormattedTariff = "1234567890";
			additionalTariff.US_Description = "Test Description";
			additionalTariff.US_Quantity1 = 10m;
			additionalTariff.US_Quantity2 = 20m;
			additionalTariff.US_Quantity3 = 30m;
			additionalTariff.US_AllowableQty1 = 40m;
			additionalTariff.US_AllowableQty2 = 50m;
			additionalTariff.US_AllowableQty3 = 60m;
			additionalTariff.US_ValuePerUnit1 = 1m;
			additionalTariff.US_ValuePerUnit2 = 2m;
			additionalTariff.US_ValuePerUnit3 = 3m;
			additionalTariff.US_SubstitutedValue1 = 4m;
			additionalTariff.US_SubstitutedValue2 = 5m;
			additionalTariff.US_SubstitutedValue3 = 6m;
			additionalTariff.US_UQ1 = "AE";
			additionalTariff.US_UQ2 = "AM";
			additionalTariff.US_UQ3 = "AP";
			Factory.Save();

			var clonedInvoiceLine = invoiceLine.Clone();
			AssertEquals(1, clonedInvoiceLine.DrawbackNAFTAs.Count);
			var clonedNAFTA = clonedInvoiceLine.DrawbackNAFTAs[0];
			AssertEquals("EN001", clonedNAFTA.US_DRWNAFTACountryImportEntry);
			AssertEquals(new ZDateTime(2025, 01, 01), clonedNAFTA.US_DRWNAFTACountryImportEntryDate);
			AssertEquals("88591201", clonedNAFTA.US_DRWNAFTACountryTariffNumber);
			AssertEquals("88591202", clonedNAFTA.US_DRWNAFTACountryTariffNumber2);
			AssertEquals("88591203", clonedNAFTA.US_DRWNAFTACountryTariffNumber3);
			AssertEquals(12m, clonedNAFTA.US_DRWNAFTACountryDutyRate);
			AssertEquals(100m, clonedNAFTA.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty);
			AssertEquals("CA", clonedNAFTA.US_DRWNAFTACountryOfExport);
			AssertEquals(1m, clonedNAFTA.US_DRWNAFTACountryImportDuty);

			AssertEquals(1, clonedInvoiceLine.DrawbackAdditionalImportTariffNumbers.Count);
			var clonedTariff = clonedInvoiceLine.DrawbackAdditionalImportTariffNumbers[0];
			AssertEquals(1, clonedTariff.US_LineNo);
			AssertEquals("1234.56.7890", clonedTariff.US_FormattedTariff);
			AssertEquals("Test Description", clonedTariff.US_Description);
			AssertEquals(10m, clonedTariff.US_Quantity1);
			AssertEquals(20m, clonedTariff.US_Quantity2);
			AssertEquals(30m, clonedTariff.US_Quantity3);
			AssertEquals(40m, clonedTariff.US_AllowableQty1);
			AssertEquals(50m, clonedTariff.US_AllowableQty2);
			AssertEquals(60m, clonedTariff.US_AllowableQty3);
			AssertEquals(1m, clonedTariff.US_ValuePerUnit1);
			AssertEquals(2m, clonedTariff.US_ValuePerUnit2);
			AssertEquals(3m, clonedTariff.US_ValuePerUnit3);
			AssertEquals(4m, clonedTariff.US_SubstitutedValue1);
			AssertEquals(5m, clonedTariff.US_SubstitutedValue2);
			AssertEquals(6m, clonedTariff.US_SubstitutedValue3);
			AssertEquals("AE", clonedTariff.US_UQ1);
			AssertEquals("AM", clonedTariff.US_UQ2);
			AssertEquals("AP", clonedTariff.US_UQ3);
		}

		public void TestImportTrackingNumber()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.US_EntryType = ACEDrawbackProvisionsList.Codes._76;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			invoiceLine.US_DRWImpTrkID = "TTT";
			AssertEquals("TTT", ((IACEDrawbackExportClaim)invoiceLine).ImportTrackingNumber);

			invoiceLine.US_DRWIsForManufacturerSection = true;
			AssertEquals(ZString.Empty, ((IACEDrawbackExportClaim)invoiceLine).ImportTrackingNumber);
		}

		public void TestQtyDecimals()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			invoiceLine.US_DRWImportQuantity = 0.08333m;
			invoiceLine.US_DRWImportQuantity2 = 0.08333m;
			invoiceLine.US_DRWImportQuantity3 = 0.08333m;
			invoiceLine.US_DRWExportQuantity = 0.08333m;
			invoiceLine.US_DRWQuantityUsed = 0.08333m;
			AssertEquals(0.0833m, invoiceLine.DRWImportQuantity);
			AssertEquals(0.0833m, invoiceLine.DRWImportQuantity2);
			AssertEquals(0.0833m, invoiceLine.DRWImportQuantity3);
			AssertEquals(0.0833m, invoiceLine.DRWExportQuantity);
			AssertEquals(0.0833m, invoiceLine.US_DRWQuantityUsed);
		}

		public void TestQuantityDecimalPlaces()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			AssertEquals(4, Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.DRWImportQuantityInfo).DecimalPlaces);
			AssertEquals(4, Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.DRWImportQuantity2Info).DecimalPlaces);
			AssertEquals(4, Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.DRWImportQuantity3Info).DecimalPlaces);
			AssertEquals(4, Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.US_DRWQuantityUsedInfo).DecimalPlaces);
			AssertEquals(4, Enterprise.Customs.Business.ZPropertyInfoExtensions.GetAttribute<DecimalPlacesAttribute>(invoiceLine.DRWExportQuantityInfo).DecimalPlaces);
		}

		public void TestSettingUS_DRWLineDutyWithCalculateUS_DRWCalcDuty()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.JI_Tariff = "10203040";
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.US_DRWImportQuantity = 1;
			invoiceLine.US_DRWImportUQ = "NO";
			invoiceLine.US_DRWExportQuantity = 1;
			invoiceLine.US_DRWExportUQ = "NO";
			invoiceLine.US_DRWDeclaredVFD = 1000m;
			invoiceLine.US_DRWLineDuty = 100m;
			AssertEquals("US_DRWCalcDuty", 99m, invoiceLine.US_DRWCalcDuty);
			AssertEquals("CalculatedDuty", 99m, invoiceLine.CalculatedDuty);
			invoiceLine.LineDuty = 200m;
			AssertEquals("US_DRWCalcDuty", 198m, invoiceLine.US_DRWCalcDuty);
			AssertEquals("CalculatedDuty", 198m, invoiceLine.CalculatedDuty);
		}

		public void TestSetDefaultDrawbackClaimsValues()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			AssertEquals("Should be set to drawback default", 0m, invoiceLine.US_DRWImportQuantity);
			AssertNotContains("Not Serialised to AddInfo", "DRWImportQuantity", invoiceLine.JI_AddInfo);

			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = dec.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			AssertEquals("Should not be set to drawback default", 0m, invoiceLine.US_DRWImportQuantity);
			AssertNotContains("Not serialised to AddInfo", "DRWImportQuantity", invoiceLine.JI_AddInfo);
		}

		public void TestSuspendUS_DRWAdValoremRateSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWAdValoremRate, 1, 2, 3);
		}

		public void TestUS_DRWWeightedRatioSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWWeightedRatio, 1, 2, 3);
		}

		public void TestSuspendUS_DRWMPFWeightedRatioSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWMPFWeightedRatio, 1, 2, 3);
		}

		public void TestSuspendUS_DRWLineDutySetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWLineDuty, 1, 2, 3);
		}

		public void TestSuspendUS_DRWDeclaredTaxSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWDeclaredTax, 1, 2, 3);
		}

		public void TestSuspendUS_DRWDeclaredVFDSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWDeclaredVFD, 1, 2, 3);
		}

		public void TestSuspendUS_DRWDeclaredHMFSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWDeclaredHMF, 1, 2, 3);
		}

		public void TestSuspendUS_DRWDeclaredMPFSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWDeclaredMPF, 1, 2, 3);
		}

		public void TestSuspendUS_DRWImportQuantitySetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWImportQuantity, 1, 2, 3);
		}

		public void TestSuspendUS_DRWImportUQSetter()
		{
			AssertSuspendFieldSetter<ZString>(JobComInvoiceLine.Schema.US_DRWImportUQ, "1", "2", "3");
		}

		public void TestSuspendUS_DRWExportQuantitySetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWExportQuantity, 1, 2, 3);
		}

		public void TestSuspendUS_DRWExportUQSetter()
		{
			AssertSuspendFieldSetter<ZString>(JobComInvoiceLine.Schema.US_DRWExportUQ, "1", "2", "3");
		}

		public void TestSuspendUS_DRWImportQuantity2Setter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWImportQuantity2, 1, 2, 3);
		}

		public void TestSuspendUS_DRWImportUQ2Setter()
		{
			AssertSuspendFieldSetter<ZString>(JobComInvoiceLine.Schema.US_DRWImportUQ2, "1", "2", "3");
		}

		public void TestSuspendUS_DRWImportQuantity3Setter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWImportQuantity3, 1, 2, 3);
		}

		public void TestSuspendUS_DRWImportUQ3Setter()
		{
			AssertSuspendFieldSetter<ZString>(JobComInvoiceLine.Schema.US_DRWImportUQ3, "1", "2", "3");
		}

		public void TestSuspendUS_DRWValuePerUQSetter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWValuePerUQ, 1, 2, 3);
		}

		public void TestSuspendUS_DRWValuePerUQ2Setter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWValuePerUQ2, 1, 2, 3);
		}

		public void TestSuspendUS_DRWValuePerUQ3Setter()
		{
			AssertSuspendFieldSetter<ZDecimal>(JobComInvoiceLine.Schema.US_DRWValuePerUQ3, 1, 2, 3);
		}

		public void TestSuspendUS_DRWLineDutyRateDescSetter()
		{
			AssertSuspendFieldSetter<ZString>(JobComInvoiceLine.Schema.US_DRWLineDutyRateDesc, "1", "2", "3");
		}

		public void TestSuspendUS_DRWCalcDutyWithAdValoremRateSetter()
		{
			AssertSuspendFieldSetter(JobComInvoiceLine.Schema.US_DRWCalcDutyWithAdValoremRate, ZBool.False, ZBool.True, ZBool.True);
		}

		public void TestExporterAndDestroyerProperties()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var exporter = Factory.New<OrgHeader>();
			invoiceLine.ExporterOrDestroyer.OrganisationPK = exporter.PK;
			AssertEquals(exporter.PK, invoiceLine.US_OH_DRWExporterOrDestroyer);
			AssertEquals(exporter.MainAddress.PK, invoiceLine.US_OA_DRWExporterOrDestroyer);
		}

		public void TestNoCalculatedAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0.0m, invoiceLine.AdjClaimHMF);
			AssertEquals(0.0m, invoiceLine.AdjClaimMPF);
			AssertEquals(0.0m, invoiceLine.AdjClaimTax);
			AssertEquals(0.0m, invoiceLine.AdjClaimDuty);
			AssertEquals(0.0m, invoiceLine.ClaimedDuty);
			AssertEquals(0.0m, invoiceLine.ClaimedHMF);
			AssertEquals(0.0m, invoiceLine.ClaimedMPF);
			AssertEquals(0.0m, invoiceLine.ClaimedTax);
			AssertEquals(true, invoiceLine.NoDrawbackAmounts);

			var oneFee = invoiceLine.DrawbackOtherFees.AddNew();
			oneFee.US_FeeType = Core.Constants.USCustoms.FeeCodes.Wines;

			var fee = invoiceLine.DrawbackOtherFees[0];
			AssertEquals(0m, fee.ClaimedAmount);
			AssertEquals(0m, fee.AdjClaimAmount);
			AssertEquals(true, invoiceLine.NoDrawbackAmounts);

			var twoFee = invoiceLine.DrawbackOtherFees.AddNew();
			twoFee.US_FeeType = Core.Constants.USCustoms.FeeCodes.Cotton;
			twoFee.US_FeeAmount = 1m;
			twoFee.US_CalculatedAmount = 0.90m;
			AssertEquals(2, invoiceLine.DrawbackOtherFees.Count);
			AssertEquals(false, invoiceLine.NoDrawbackAmounts);

			invoiceLine.DrawbackOtherFees.RemoveAll();
			AssertEquals(true, invoiceLine.NoDrawbackAmounts);

			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.DRWImportQuantity = 1m;
			invoiceLine.DRWExportQuantity = 1m;
			invoiceLine.DeclaredTax = 200m;
			AssertEquals("Calculated tax amount", 200m, invoiceLine.ClaimedTax);
			AssertEquals(false, invoiceLine.NoDrawbackAmounts);
		}

		public void TestDrawbackNoticeOfIntentProperties()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			drawback.US_DRWTENo = "DecTENo";
			drawback.US_DRWIntendedPortOfExport = "1011";
			drawback.US_UI_NKCarrierSCAC = "MPX";

			AssertEquals("Effetive value from dec", "DecTENo", line.US_DRWTENo);
			AssertEquals("Effetive value from dec", "1011", line.US_DRWIntendedPortOfExport);
			AssertEquals("Effetive value from dec", "MPX", line.US_UI_NKCarrierSCAC);

			line.US_DRWTENo = "LineTENo";
			line.US_DRWIntendedPortOfExport = "1022";
			line.US_UI_NKCarrierSCAC = "XXX";

			AssertEquals("Value from line itself", "LineTENo", line.US_DRWTENo);
			AssertEquals("Value from line itself", "1022", line.US_DRWIntendedPortOfExport);
			AssertEquals("Value from line itself", "XXX", line.US_UI_NKCarrierSCAC);
		}

		public void TestDrawbackEntryNoOrCMDNo()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var line = drawback.InvoiceLines.AddNew();
			line.US_DRWCertOfManufacture = "CM12345";
			AssertEquals("DrawbackEntryNoOrCMDNo", "CM12345", line.DrawbackEntryNoOrCMDNo);
			line.US_ImportEntryNo = "ENTRY1";
			AssertEquals("DrawbackEntryNoOrCMDNo", "ENTRY1", line.DrawbackEntryNoOrCMDNo);
		}

		public void TestDrawbackEntryNoAndLineOrCMDNumber()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			line.US_ImportEntryNo = "ABC01234567";
			line.US_DRWImportEntryLine = 3;
			line.US_DRWCertOfManufacture = "CD11";
			AssertEquals("ABC-0123456-7", line.FormattedDrawbackEntryNoOrCMDNumber);
			AssertEquals("ABC-0123456-7 / 3", line.GetFormattedImportEntryNoWithLineItemNo());
			line.US_ImportEntryNo = ZString.Empty;
			AssertEquals("CD11", line.FormattedDrawbackEntryNoOrCMDNumber);
			AssertEquals("", line.GetFormattedImportEntryNoWithLineItemNo());
		}

		public void TestCalculatedTaxAmount()
		{
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			importDec.US_EntryFilerCode = "ABC";
			var entryHeader = importDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "ENTRY2";
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 5;
			entryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 1000m);
			var decInvoice = importDec.Invoices.AddNew();
			var decInvoiceLine = decInvoice.InvoiceLines.AddNew();
			decInvoiceLine.JI_CL = entryLine.PK;
			decInvoiceLine.JI_CustomsUnitQty = "NO";
			decInvoiceLine.JI_CustomsQuantity = 20m;

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;

			Factory.Save();

			line1.US_DRWExportQuantity = 10m;
			line1.US_DRWExportUQ = "NO";

			line1.US_ImportEntryNo = "ABCENTRY2";
			line1.US_DRWImportEntryLine = 5;

			AssertEquals("Calculated tax amount", 500m, line1.ClaimedTax);
			AssertEquals("Calculated tax amount", 495m, line1._99ClaimedTax);
			line1.US_DRWIsForImportSection = true;
			line1.US_DRWClaimAmountOverriden_New = true;
			line1.DRWImportQuantity = 1m;
			line1.DRWExportQuantity = 1m;
			line1.DeclaredTax = 200m;
			AssertEquals("Calculated tax amount", 200m, line1.ClaimedTax);
			AssertEquals("Overriden claim tax", 198m, line1._99ClaimedTax);

			line1.DRWImportQuantity = 2m;
			line1.US_DRWClaimAmountOverriden_New = false;
			AssertEquals("Calculated tax amount", 500m, line1.ClaimedTax);
			AssertEquals("Calculated claim tax", 495m, line1._99ClaimedTax);
		}

		public void TestSettingUS_ImportEntryNoDoesNotLoadDrawbackTwice()
		{
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";
			importDeclaration.JE_TransportMode = "TRK";

			importDeclaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";
			var importInvoiceLine = importDeclaration.InvoiceLines.AddNew();
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.JI_Tariff = "100231450";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			importDeclaration.ImportEntryNumber = "12345678";
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.US_EntryType = "08";

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvoiceLineTesting>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC12345678-1";
			AssertEquals(1, invoiceLine.SetCount);
			AssertEquals("100231450", invoiceLine.JI_Tariff);

			// no entry details changed
			invoiceLine.Delete();
			invoiceLine = Factory.New<JobComInvoiceLineTesting>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC12345678";
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.JI_Tariff = ZString.Empty;
			_ = invoiceLine.DrawbackImportEntryLine;
			invoiceLine.US_ImportEntryNo = "ABC12345678-1";
			_ = invoiceLine.DrawbackImportEntryLine;
			AssertEquals(ZString.Empty, invoiceLine.JI_Tariff);

			// Test EntryLine No is changed
			invoiceLine.Delete();
			invoiceLine = Factory.New<JobComInvoiceLineTesting>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC12345678";
			invoiceLine.US_DRWImportEntryLine = 2;
			_ = invoiceLine.DrawbackImportEntryLine;
			invoiceLine.US_ImportEntryNo = "ABC12345678-1";
			AssertEquals(1, invoiceLine.SetCount);
			AssertEquals("100231450", invoiceLine.JI_Tariff);

			// Test EntryNo is changed
			invoiceLine.Delete();
			invoiceLine = Factory.New<JobComInvoiceLineTesting>();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC12345671";
			invoiceLine.US_DRWImportEntryLine = 1;
			_ = invoiceLine.DrawbackImportEntryLine;

			invoiceLine.US_ImportEntryNo = "ABC12345678-1";
			_ = invoiceLine.DrawbackImportEntryLine;
			AssertEquals(1, invoiceLine.SetCount);
			AssertEquals("100231450", invoiceLine.JI_Tariff);
		}

		public void TestDrawbackValueAreDefaultedOnOverride()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567891";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.16200000m;
			tariff.UE_ShortDescription = "HELLO WORLD";
			tariff.UE_Unit1 = "NO";
			tariff.UE_Unit2 = "KG";
			tariff.UE_Unit3 = "PK";

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDec.US_EnableENS = true;
			importDec.ImportEntryNumber = "12345678";
			var importInvoice = importDec.Invoices.AddNew();
			importInvoice.JZ_InvoiceNumber = "INV1234";

			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_Tariff = "1234567891";
			importInvoiceLine.JI_InvoiceUQ = "NO";
			importInvoiceLine.JI_InvoiceQuantity = 1000m;
			importInvoiceLine.JI_CustomsQuantity = 1000m;
			importInvoiceLine.JI_CustomsSecondQuantity = 200m;
			importInvoiceLine.JI_CustomsThirdQuantity = 100m;
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.US_PayableMPF = 100m;
			importDec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var importEntryHeader = importDec.ActiveEntryHeaders.EntrySummaryEntry;
			var importEntryLine = importEntryHeader.MergedLines[0];
			importEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 300m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 500m);
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._15;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWOldData = ZBool.True;
			invoiceLine.US_DRWIsForImportSection = ZBool.True;
			invoiceLine.US_ImportEntryNo = "XJ512345678-1";
			invoiceLine.US_DRWExportQuantity = 500m;
			var claims = invoiceLine.Claims;
			CombineAssertions("Entry Default", () =>
			{
				AssertEquals("invoiceLine.US_DRWOldData", ZBool.False, invoiceLine.US_DRWOldData);
				AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2%", claims.DutyClaim.LineDutyRateDesc);
				AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
				AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
				AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
				AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 99.00m, 1m);
				AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, ZDecimal.Zero, ZDecimal.Zero, 1m);
				AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 148.50m, 1m);
				AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, ZDecimal.Zero, ZDecimal.Zero, 1m);
				AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, ZDecimal.Zero, 1m);
			});

			invoiceLine.US_DRWOldData = ZBool.True;
			claims.DutyClaim.LineDutyRateDesc = ZString.Empty;
			ClearOverrideData(claims.DutyClaim);
			ClearOverrideData(claims.IRTaxClaim);
			ClearOverrideData(claims.HMFClaim);
			ClearOverrideData(claims.MPFClaim);
			ClearOverrideData(claims.OtherFeesClaim);
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			CombineAssertions("Override Default", () =>
			{
				AssertEquals("invoiceLine.US_DRWOldData", ZBool.False, invoiceLine.US_DRWOldData);
				AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
				AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
				AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
				AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
				AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 801.9m, 1m);
				AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, ZDecimal.Zero, ZDecimal.Zero, 1m);
				AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 148.50m, 1m);
				AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, ZDecimal.Zero, ZDecimal.Zero, 1m);
				AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, ZDecimal.Zero, 1m);
			});
		}

		[TestDate(2020, 4, 1)]
		public void TestDrawbackImportEntryLineUseView()
		{
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";
			importDeclaration.JE_TransportMode = "TRK";

			importDeclaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";
			var importInvoiceLine = importDeclaration.InvoiceLines.AddNew();
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.JI_Tariff = "100231450";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.US_EntryType = "08";

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = importDeclaration.EntryFilerCode + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;
			Factory.Save();

			using (DataRegistry.Business.USCustomsDataRegistry.Instance.UseViewForDrawbackEntryLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				dec = newFactory.Load<JobDeclaration>(dec.PK);
				invoice = (JobComInvoiceHeader)dec.Invoices.FindByPK(invoice.PK);
				invoiceLine = (JobComInvoiceLine)invoice.JobComInvoiceLines.FindByPK(invoiceLine.PK);
				var drawbackImportEntryLine = invoiceLine.DrawbackImportEntryLine;
				AssertType<USImportEntryLine>(drawbackImportEntryLine);
				AssertDbHits(new Dictionary<string, int>()
				{
					{ CusUnderbondDecSchema.Constants.TableName, 1 },
					{ JobDeclarationSchema.Constants.TableName, 1 },
					{ JobComInvoiceHeaderSchema.Constants.TableName, 2 },
					{ JobComInvoiceLineSchema.Constants.TableName, 1 },
					{ USImportEntryLineSchema.Constants.TableName, 1 }
				}, newFactory);
			}
			using (DataRegistry.Business.USCustomsDataRegistry.Instance.UseViewForDrawbackEntryLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var newFactory = new BusinessObjectFactory();
				dec = newFactory.Load<JobDeclaration>(dec.PK);
				invoice = (JobComInvoiceHeader)dec.Invoices.FindByPK(invoice.PK);
				invoiceLine = (JobComInvoiceLine)invoice.JobComInvoiceLines.FindByPK(invoiceLine.PK);
				var drawbackImportEntryLine = invoiceLine.DrawbackImportEntryLine;
				AssertType<CusEntryLine>(drawbackImportEntryLine);
				AssertDbHits(new Dictionary<string, int>()
				{
					{ CusEntryHeaderSchema.Constants.TableName, 1 },
					{ CusEntryLineSchema.Constants.TableName, 1 },
					{ CusUnderbondDecSchema.Constants.TableName, 1 },
					{ JobDeclarationSchema.Constants.TableName, 2 },
					{ JobComInvoiceHeaderSchema.Constants.TableName, 2 },
					{ JobComInvoiceLineSchema.Constants.TableName, 1 }
				}, newFactory);
			}
		}

		public void TestUS_DRWImportEntryLine()
		{
			SetupData();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			InvoiceLine.US_DRWIsForImportSection = true;
			InvoiceLine.US_ImportEntryNo = "XJ533333333-2";
			AssertEquals("Pre-condition", "1111111111", InvoiceLine.JI_Tariff);

			InvoiceLine.US_DRWImportEntryLine = 1;
			InvoiceLine.JI_Tariff = "2222222222";
			AssertEquals("2222222222", InvoiceLine.JI_Tariff);

			((ISupportDataImporting)InvoiceLine).IsImportingData = false;
			InvoiceLine.US_DRWImportEntryLine = 2;
			AssertEquals("1111111111", InvoiceLine.JI_Tariff);

			InvoiceLine.US_DRWImportEntryLine = 1;
			InvoiceLine.JI_Tariff = "3333333333";
			AssertEquals("3333333333", InvoiceLine.JI_Tariff);

			((ISupportDataImporting)InvoiceLine).IsImportingData = true;
			InvoiceLine.US_DRWImportEntryLine = 2;
			AssertEquals("1111111111", InvoiceLine.JI_Tariff);
		}

		public void TestUS_ImportEntryNo()
		{
			SetupData();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			InvoiceLine.US_DRWIsForImportSection = true;
			((ISupportDataImporting)InvoiceLine).IsImportingData = true;
			InvoiceLine.US_ImportEntryNo = "XJ533333333-2";
			AssertEquals("XJ533333333", InvoiceLine.US_ImportEntryNo);
			AssertEquals(2, InvoiceLine.US_DRWImportEntryLine);
			AssertEquals("1111111111", InvoiceLine.JI_Tariff);
			AssertEquals(0m, InvoiceLine._99ClaimedDuty.Round(2));
			AssertEquals("STUFF", InvoiceLine.JI_Description);
			AssertEquals(new ZDate(2011, 07, 25), InvoiceLine.US_DRWEntryDate);
			AssertEquals("2210", InvoiceLine.US_DRWPort);
			AssertEquals(66.67m, InvoiceLine.DutyPerUnit.Round(2));
			AssertEquals(0.3m, InvoiceLine.DutyRate);
			AssertEquals(1, InvoiceLine.DrawbackAdditionalImportTariffNumbers.Count);
			AssertEquals("9911111111", InvoiceLine.DrawbackAdditionalImportTariffNumbers[0].US_Tariff);
			AssertEquals(0m, InvoiceLine.DrawbackAdditionalImportTariffNumbers[0].US_ValuePerUnit1);

			InvoiceLine.US_ImpDecInvoiceNum = "INV-1-3";
			AssertEquals("INV-1-3", InvoiceLine.US_ImpDecInvoiceNum);
		}

		public void TestDrawbackMessageBuilderInterfaceMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_ImportEntryNo = "123123456";
			invoiceLine.US_DRWPort = "3901";
			invoiceLine.US_DRWEntryDate = new ZDateTime(2008, 1, 1);
			invoiceLine.US_DRWCMCDIndicator = "C";
			invoiceLine.US_DRWCertOfManufacture = "CM1234";
			invoiceLine.JI_Description = "GOODS DESC";
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.DRWImportQuantity = 100;
			invoiceLine.DRWExportQuantity = 100;
			invoiceLine.US_DRWExportUQ = "KG";
			invoiceLine.DeclaredVFD = 123.45m;
			invoiceLine.DeclaredTax = 67.89m;
			invoiceLine.US_ExportTariff = "8483308090";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.LineDuty = invoiceLine.Claims.DutyClaim.DeclaredAmount * 0.5m;

			AssertEquals("Export Tariff Formatted", "8483.30.8090", invoiceLine.US_FormattedExportTariff);
			AssertEquals("Export Tariff", "8483308090", invoiceLine.US_ExportTariff);
			AssertEquals("DrawbackImportEntry", "123123456", invoiceLine.DrawbackImportEntry);
			AssertEquals("DrawbackImportEntryPort", "3901", invoiceLine.DrawbackImportEntryPort);
			AssertEquals("DrawbackImportEntryDate", new ZDate(2008, 1, 1), invoiceLine.DrawbackImportEntryDate);
			AssertEquals("CMCDIndicator", "C", invoiceLine.CMCDIndicator);
			AssertEquals("DrawbackClaimDuty", 61.11m, invoiceLine.DrawbackClaimDuty);
			AssertEquals("DrawbackClaimTax", 67.21m, invoiceLine.DrawbackClaimTax);
			AssertEquals("CertificateOfManufactureNumber", "CM1234", invoiceLine.CertificateOfManufactureNumber);
			AssertEquals("CertificateOfManufacturePort", "3901", invoiceLine.CertificateOfManufacturePort);
			AssertEquals("DrawbackManufactureQuantity", 100m, invoiceLine.DrawbackManufactureQuantity);
			AssertEquals("DrawbackManufactureUnitOfMeasure", "KG", invoiceLine.DrawbackManufactureUnitOfMeasure);
			AssertEquals("DescriptionForBlock41", "GOODS DESC", invoiceLine.DescriptionForBlock41);
		}

		public void TestReadOnlyProperties()
		{
			//ExportSectionData
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForExportSection = true;
			Assert("US_ExportTariff is NOT ReadOnly", !invoiceLine.US_ExportTariffInfo.ReadOnly);
			invoiceLine.US_DRWIsForExportSection = false;
			Assert("US_ExportTariff is ReadOnly", invoiceLine.US_ExportTariffInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			Assert("US_ExportTariff is NOT ReadOnly with non-Drawback", !invoiceLine.US_ExportTariffInfo.ReadOnly);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Assert("US_ExportTariff is ReadOnly again", invoiceLine.US_ExportTariffInfo.ReadOnly);
			Assert("US_DRWExportDate is ReadOnly", invoiceLine.US_DRWExportDateInfo.ReadOnly);
			Assert("US_DRWExportAction is ReadOnly", invoiceLine.US_DRWExportActionInfo.ReadOnly);
			Assert("US_DRWExportID is ReadOnly", invoiceLine.US_DRWExportIDInfo.ReadOnly);
			Assert("US_DRWExportDest is ReadOnly", invoiceLine.US_DRWExportDestInfo.ReadOnly);
			invoiceLine.US_DRWIsForExportSection = true;
			Assert("US_ExportTariff is NOT ReadOnly again", !invoiceLine.US_ExportTariffInfo.ReadOnly);
			Assert("US_DRWExportDate is NOT ReadOnly", !invoiceLine.US_DRWExportDateInfo.ReadOnly);
			Assert("US_DRWExportAction is NOT ReadOnly", !invoiceLine.US_DRWExportActionInfo.ReadOnly);
			Assert("US_DRWExportID is NOT ReadOnly", !invoiceLine.US_DRWExportIDInfo.ReadOnly);
			Assert("US_DRWExportDest is NOT ReadOnly", !invoiceLine.US_DRWExportDestInfo.ReadOnly);

			//Manufacturing section
			invoiceLine.US_DRWIsForManufacturerSection = false;
			Assert(invoiceLine.US_DRWQuantityUsed_ReadOnly);
			Assert(invoiceLine.US_DRWUQUsed_ReadOnly);
			Assert(invoiceLine.US_DRWDateOfManufacture_ReadOnly);
			Assert(invoiceLine.US_DRWDescrManufactured_ReadOnly);
			Assert(invoiceLine.US_DRWDescrUsed_ReadOnly);
			Assert(invoiceLine.US_DRWFactoryLocation_ReadOnly);
			invoiceLine.US_DRWIsForManufacturerSection = true;
			Assert(!invoiceLine.US_DRWQuantityUsed_ReadOnly);
			Assert(!invoiceLine.US_DRWUQUsed_ReadOnly);
			Assert(!invoiceLine.US_DRWDateOfManufacture_ReadOnly);
			Assert(!invoiceLine.US_DRWDescrManufactured_ReadOnly);
			Assert(!invoiceLine.US_DRWDescrUsed_ReadOnly);
			Assert(!invoiceLine.US_DRWFactoryLocation_ReadOnly);
		}

		public void TestMPFWeightedRatio()
		{
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";

			importDeclaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";
			var importInvoiceLine = importDeclaration.InvoiceLines.AddNew();
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.JI_Tariff = "100231450";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importDeclaration.US_PayableMPF = 0m;
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.US_EntryType = "08";

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = importDeclaration.EntryFilerCode + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;
			AssertEquals(0m, invoiceLine.MPFWeightedRatio);
		}

		public void TestWeightedRatio()
		{
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";
			importDeclaration.JE_TransportMode = "TRK";

			importDeclaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";
			var importInvoiceLine = importDeclaration.InvoiceLines.AddNew();
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.JI_Tariff = "100231450";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.US_EntryType = "08";

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = importDeclaration.EntryFilerCode + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;

			AssertEquals(0m, invoiceLine.WeightedRatio);
		}

		[TestDate(2007, 3, 19)]
		public void TestJI_CustomsQuantityAndSupplementaryQty1()
		{
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99060742", "1", 0m, "KG");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;

			invoiceLine.US_SupTariff = "99060742"; // 5.99 cents/litre extra
			invoiceLine.US_SupUQ1 = invoiceLine.JI_CustomsUnitQty;
			AssertEquals(1000m, invoiceLine.US_SupQty1);

			invoiceLine.JI_CustomsQuantity = 2000m;
			AssertEquals(2000m, invoiceLine.US_SupQty1);

			invoiceLine.US_SupQty1 = 2400m;
			AssertEquals(2400m, invoiceLine.US_SupQty1);

			invoiceLine.JI_CustomsQuantity = 2400m;
			AssertEquals(2400m, invoiceLine.US_SupQty1);

			invoiceLine.US_SupTariff = "";
			AssertEquals(0m, invoiceLine.US_SupQty1);
		}

		public void TestUseImportClassificationUseExportClassification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(true, invoiceLine.UseExportClassification);
			AssertEquals(false, invoiceLine.UseImportClassification);

			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals(false, invoiceLine.UseExportClassification);
			AssertEquals(true, invoiceLine.UseImportClassification);
		}

		public void TestSetDefaultFromPreviousLine()
		{
			var exportingCarrier = Factory.New<OrgHeader>();
			exportingCarrier.OH_FullName = "EXPORTER NAME";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC12345678";
			invoiceLine.US_DRWCertOfManufacture = "CM123456";
			invoiceLine.US_DRWEntryDate = new ZDateTime(2008, 3, 2);
			invoiceLine.US_DRWPort = "1234";
			invoiceLine.US_DRWCMCDIndicator = "M";
			invoiceLine.US_DRWImportEntryLine = 5;
			invoiceLine.US_DRWDateRcvFrom = new ZDateTime(2008, 1, 2);
			invoiceLine.US_DRWDateRcvTo = new ZDateTime(2008, 1, 3);
			invoiceLine.US_DRWDateUsedFrom = new ZDateTime(2008, 1, 4);
			invoiceLine.US_DRWDateUsedTo = new ZDateTime(2008, 1, 5);
			invoiceLine.US_DRWExportDate = new ZDateTime(2008, 1, 6);
			invoiceLine.US_DRWExportAction = "D";
			invoiceLine.US_DRWExportID = "EXPINVNO";
			invoiceLine.US_DRWExportDest = "AU";
			invoiceLine.US_UI_NKCarrierSCAC = "MPX";

			JobComInvoiceLine invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("US_ImportEntryNo defaulted from first line", "ABC12345678", invoiceLine2.US_ImportEntryNo);
			AssertEquals("US_DRWCertOfManufacture defaulted from first line", "CM123456", invoiceLine2.US_DRWCertOfManufacture);
			AssertEquals("US_DRWEntryDate defaulted from first line", new ZDateTime(2008, 3, 2), invoiceLine2.US_DRWEntryDate);
			AssertEquals("US_DRWPort defaulted from first line", "1234", invoiceLine2.US_DRWPort);
			AssertEquals("US_DRWCMCDIndicator defaulted from first line", "M", invoiceLine2.US_DRWCMCDIndicator);
			AssertEquals("US_DRWImportEntryLine NOT defaulted from first line", (ZInt)0, invoiceLine2.US_DRWImportEntryLine);
			AssertEquals("US_DRWDateRcvFrom defaulted from first line", new ZDateTime(2008, 1, 2), invoiceLine2.US_DRWDateRcvFrom);
			AssertEquals("US_DRWDateRcvTo defaulted from first line", new ZDateTime(2008, 1, 3), invoiceLine2.US_DRWDateRcvTo);
			AssertEquals("US_DRWDateUsedFrom defaulted from first line", new ZDateTime(2008, 1, 4), invoiceLine2.US_DRWDateUsedFrom);
			AssertEquals("US_DRWDateUsedTo defaulted from first line", new ZDateTime(2008, 1, 5), invoiceLine2.US_DRWDateUsedTo);
			AssertEquals("US_DRWExportDate defaulted from first line", new ZDateTime(2008, 1, 6), invoiceLine2.US_DRWExportDate);
			AssertEquals("US_DRWExportAction defaulted from first line", "D", invoiceLine2.US_DRWExportAction);
			AssertEquals("US_DRWExportID defaulted from first line", "EXPINVNO", invoiceLine2.US_DRWExportID);
			AssertEquals("US_DRWExportDest defaulted from first line", "AU", invoiceLine2.US_DRWExportDest);
			AssertEquals("US_DRWExporterName defaulted from first line", "MPX", invoiceLine2.US_UI_NKCarrierSCAC);

			invoiceLine2.US_DRWIsForImportSection = true;
			invoiceLine2.US_DRWIsForExportSection = false;
			JobComInvoiceLine invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("US_ImportEntryNo defaulted from previous line", "ABC12345678", invoiceLine3.US_ImportEntryNo);
			AssertEquals("US_DRWCertOfManufacture defaulted from previous line", "CM123456", invoiceLine3.US_DRWCertOfManufacture);
			AssertEquals("US_DRWEntryDate defaulted from previous line", new ZDateTime(2008, 3, 2), invoiceLine3.US_DRWEntryDate);
			AssertEquals("US_DRWPort defaulted from previous line", "1234", invoiceLine3.US_DRWPort);
			AssertEquals("US_DRWCMCDIndicator defaulted from previous line", "M", invoiceLine3.US_DRWCMCDIndicator);
			AssertEquals("US_DRWImportEntryLine NOT defaulted from previous line", (ZInt)0, invoiceLine3.US_DRWImportEntryLine);
			AssertEquals("US_DRWDateRcvFrom defaulted from previous line", new ZDateTime(2008, 1, 2), invoiceLine3.US_DRWDateRcvFrom);
			AssertEquals("US_DRWDateRcvTo defaulted from previous line", new ZDateTime(2008, 1, 3), invoiceLine3.US_DRWDateRcvTo);
			AssertEquals("US_DRWDateUsedFrom defaulted from previous line", new ZDateTime(2008, 1, 4), invoiceLine3.US_DRWDateUsedFrom);
			AssertEquals("US_DRWDateUsedTo defaulted from previous line", new ZDateTime(2008, 1, 5), invoiceLine3.US_DRWDateUsedTo);
			AssertEquals("US_DRWExportDate NOT defaulted from previous line", ZDateTime.Empty, invoiceLine3.US_DRWExportDate);
			AssertEquals("US_DRWExportAction NOT defaulted from previous line", "", invoiceLine3.US_DRWExportAction);
			AssertEquals("US_DRWExportID NOT defaulted from first previous", "", invoiceLine3.US_DRWExportID);
			AssertEquals("US_DRWExportDest NOT defaulted from first previous", "", invoiceLine3.US_DRWExportDest);
			AssertEquals("US_DRWExporterName NOT defaulted from first previous", ZString.Empty, invoiceLine3.US_UI_NKCarrierSCAC);

			invoiceLine3.US_DRWIsForImportSection = false;
			invoiceLine3.US_DRWIsForExportSection = false;
			JobComInvoiceLine invoiceLine4 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("US_ImportEntryNo NOT defaulted from previous line", "", invoiceLine4.US_ImportEntryNo);
			AssertEquals("US_DRWCertOfManufacture NOT defaulted from previous line", "", invoiceLine4.US_DRWCertOfManufacture);
			AssertEquals("US_DRWEntryDate NOT defaulted from previous line", ZDateTime.Empty, invoiceLine4.US_DRWEntryDate);
			AssertEquals("US_DRWPort NOT defaulted from previous line", "", invoiceLine4.US_DRWPort);
			AssertEquals("US_DRWCMCDIndicator NOT defaulted from previous line", "", invoiceLine4.US_DRWCMCDIndicator);
			AssertEquals("US_DRWImportEntryLine NOT defaulted from previous line", (ZInt)0, invoiceLine4.US_DRWImportEntryLine);
			AssertEquals("US_DRWDateRcvFrom NOT defaulted from previous line", ZDateTime.Empty, invoiceLine4.US_DRWDateRcvFrom);
			AssertEquals("US_DRWDateRcvTo NOT defaulted from previous line", ZDateTime.Empty, invoiceLine4.US_DRWDateRcvTo);
			AssertEquals("US_DRWDateUsedFrom NOT defaulted from previous line", ZDateTime.Empty, invoiceLine4.US_DRWDateUsedFrom);
			AssertEquals("US_DRWDateUsedTo NOT defaulted from previous line", ZDateTime.Empty, invoiceLine4.US_DRWDateUsedTo);
			AssertEquals("US_DRWExportDate NOT defaulted from previous line", ZDateTime.Empty, invoiceLine4.US_DRWExportDate);
			AssertEquals("US_DRWExportAction NOT defaulted from previous line", "", invoiceLine4.US_DRWExportAction);
			AssertEquals("US_DRWExportID NOT defaulted from first previous", "", invoiceLine4.US_DRWExportID);
			AssertEquals("US_DRWExportDest NOT defaulted from first previous", "", invoiceLine4.US_DRWExportDest);
			AssertEquals("US_DRWExporterName NOT defaulted from first previous", ZString.Empty, invoiceLine4.US_UI_NKCarrierSCAC);
		}

		public void TestProductDefaultsForImportOnlyLine()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = false;
			invoiceLine.JI_PartNo = ImpNoLocalPart;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", ImpNoLocalTariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", "", invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImportOnlyLineWithLocalPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = false;
			invoiceLine.JI_PartNo = ImpWithLocalPart;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", ImpWithLocalPartTariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", "", invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForExportOnlyLine()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = false;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = ExpNoLocalPart;
			invoiceLine.JI_InvoiceUQ = "BOX";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", "", invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", ExpNoLocalTariffNum, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForExportOnlyLineOnLocalPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = false;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = ValidLocalPart;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", "", invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", LocalPartTariffNum, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineNoLocalExpOnlyPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = ExpNoLocalPart;
			invoiceLine.JI_InvoiceUQ = "BOX";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", "", invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", ExpNoLocalTariffNum, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineLocalNotFoundExpOnlypart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = ExpWithInvalidLocalPart;
			invoiceLine.JI_InvoiceUQ = "BOX";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", "", invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", ExpWithInvalidLocalTariffNum, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineNoLocalExpWithImpPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = DuelExportPart;
			invoiceLine.JI_InvoiceUQ = "BOX";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", DuelImportTariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", DuelExportTariffNum, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineLocalNotFoundExpWithImpPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = DuelExportInvalidLocalPart;
			invoiceLine.JI_InvoiceUQ = "BOX";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", DuelImportTariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", DuelExportTariffNum, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineNoLocalImpOnlyPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = ImpNoLocalPart;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", ImpNoLocalTariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", "", invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineLocalNotFoundImpOnlyPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = ImpWithInvalidLocalPart;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", ImpWithInvalidLocalTariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", "", invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineNoLocalImpWithExpPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = DuelImportPart;
			invoiceLine.JI_InvoiceUQ = "BOX";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", DuelImportTariffNum2, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", DuelExportTariffNum2, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineLocalNotFoundImpWithExpPart()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = DuelImportInvalidLocalPart;
			invoiceLine.JI_InvoiceUQ = "UNT";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", DuelImportTariffNum2, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", DuelExportTariffNum2, invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineWithLocalAndLocalPartNotExists()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = InValidLocalPart2;
			invoiceLine.JI_InvoiceUQ = "BOX";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", ImpWithInvalidLocal2TariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", "", invoiceLine.US_ExportTariff);
		}

		public void TestProductDefaultsForImpAndExpLineWithLocalAndLocalPartExists()
		{
			JobDeclaration declaration = GetDeclarationAndParts();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.JI_PartNo = ValidLocalPart;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_InvoiceQuantity = 2m;

			AssertEquals("Import tariff", ImpWithLocalPartTariffNum, invoiceLine.JI_Tariff);
			AssertEquals("Export tariff", LocalPartTariffNum, invoiceLine.US_ExportTariff);
		}

		public void TestImportInvoiceLineUsingImportWizard()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1020304050";
			tariff.UE_Unit1 = Core.Constants.Weight.Kilograms;
			tariff.UE_Unit2 = Core.Constants.Weight.Grams;
			tariff.UE_Unit3 = Core.Constants.Weight.Pounds;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var referencetDec = Factory.New<JobDeclaration>();
			referencetDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			referencetDec.US_EntryFilerCode = "MC2";
			var entryHeader = referencetDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "62963374";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 2;
			entryLine1.CL_AdValoremTariff = "123456";
			var decInvoice = referencetDec.Invoices.AddNew();
			var decInvoiceLine = decInvoice.InvoiceLines.AddNew();
			decInvoiceLine.JI_CL = entryLine1.PK;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = declaration.Invoices.AddNew();
			var collection = declaration.FilteredInvoiceLines;
			var impl = new ImportCollectionInfoImpl(collection);
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_LineNo) { HeaderText = "LNO" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_InvoiceQuantity) { HeaderText = "Inv. Qty" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_Description) { HeaderText = "Goods Description" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_ImportEntryNo) { HeaderText = "Imp Entry No" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWImportEntryLine) { HeaderText = "Imp Entry Line No" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_ImpDecInvoiceNum) { HeaderText = "Imp. Decl. Invoice Number" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.DeclaredVFD) { HeaderText = "Entered Value " });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.DeclaredMPF) { HeaderText = "MPF Claim" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.DeclaredHMF) { HeaderText = "HMF Claim" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWClaimAmountOverriden_New) { HeaderText = "Override" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.LineDuty) { HeaderText = "Line Duty" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.JI_Tariff) { HeaderText = "Tariff" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWIsForImportSection) { HeaderText = "Imp Section" });
			impl.Add(new ImportPropertyInfoImpl<JobComInvoiceLine>(JobComInvoiceLine.Schema.US_DRWIsForExportSection) { HeaderText = "Exp Section" });

			var wizard = ((IImportWizardProvider)collection).GetImportWizard(impl, null, new FileMapperForTest());
			AssertEquals(14, wizard.Mapping.Count);
			for (int i = 0; i < wizard.Mapping.Count; i++)
			{
				wizard.Mapping[i].AddFileColumnIndex(i);
			}
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testDocumentPath = resourceRetriever.SaveResourceToFile("Enterprise.Customs.US.Business.Testing.Business.JobComInvoiceLine.TestFiles.Drawback Import Test.csv");
				wizard.FileName = testDocumentPath;
				wizard.StartingRow = 2;
				AssertNoExceptionThrown(() => wizard.ImportIntoCollection(collection));
				AssertEquals(1, collection.Count);

				var invoiceLine = collection.OfType<JobComInvoiceLine>().FirstOrDefault();
				CombineAssertions(() =>
				{
					AssertEquals("JI_LineNo", 1, (int)invoiceLine.JI_LineNo);
					AssertEquals("JI_InvoiceQuantity", 1m, invoiceLine.JI_InvoiceQuantity);
					AssertEquals("JI_Description", "TS7054M  MOUNTED BRAKE LININGS", invoiceLine.JI_Description);
					AssertEquals("US_ImportEntryNo", "MC262963374", invoiceLine.US_ImportEntryNo);
					AssertEquals("US_ImpDecInvoiceNum", "RN-111711-C", invoiceLine.US_ImpDecInvoiceNum);
					AssertEquals("DeclaredVFD", 100m, invoiceLine.DeclaredVFD);
					AssertEquals("DeclaredMPF", 0.02m, invoiceLine.DeclaredMPF);
					AssertEquals("DeclaredHMF", 0.01m, invoiceLine.DeclaredHMF);
					AssertEquals("US_DRWClaimAmountOverriden_New", ZBool.True, invoiceLine.US_DRWClaimAmountOverriden_New);
					AssertEquals("LineDuty", 5m, invoiceLine.LineDuty);
					AssertEquals("US_DRWIsForImportSection", ZBool.True, invoiceLine.US_DRWIsForImportSection);
					AssertEquals("US_DRWIsForExportSection", ZBool.False, invoiceLine.US_DRWIsForExportSection);
				});
			}
		}

		public void TestUS_DRWCalcDutyWithAdValoremRate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_CustomsQuantity = 23m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			Assert(invoiceLine.US_DRWCalcDutyWithAdValoremRate_ReadOnly);

			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "100231459")) ?? GetImportTariff("100231459", "KG");
			tariff.UE_DutyComputationCode = "";
			tariff.UE_Column1RateAdValorem = 0m;

			invoiceLine.JI_Tariff = "100231459";
			Assert(invoiceLine.US_DRWCalcDutyWithAdValoremRate_ReadOnly);
			Assert(invoiceLine.US_DRWAdValoremRate_ReadOnly);
			invoiceLine.US_DRWCalcDutyWithAdValoremRate = true;
			Assert(invoiceLine.LineDutyInfo.ReadOnly);

			tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "100231450"));
			if (tariff == null)
			{
				tariff = GetImportTariff("100231450", "KG");
			}
			tariff.UE_DutyComputationCode = "7";
			tariff.UE_Column1RateAdValorem = 0.068m;

			invoiceLine.JI_Tariff = "100231450";
			invoiceLine.US_DRWClaimAmountOverriden_New = true; //should default advalorem details on this point

			Assert(!invoiceLine.US_DRWCalcDutyWithAdValoremRate_ReadOnly);
			Assert(!invoiceLine.US_DRWAdValoremRate_ReadOnly);
			Assert(invoiceLine.US_DRWCalcDutyWithAdValoremRate);
			AssertEquals(6.8m, invoiceLine.US_DRWAdValoremRate);
		}

		public void TestIsAutoCalculated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var drawbackOtherFee = invoiceLine.DrawbackOtherFees.AddNew();
			AssertEquals(true, invoiceLine.IsAutoCalculated);

			Assert("Should be readonly", invoiceLine._99ClaimedDutyInfo.ReadOnly);
			Assert("Should be readonly", invoiceLine._99ClaimedTaxInfo.ReadOnly);
			Assert("Should be readonly", invoiceLine._99ClaimedMPFInfo.ReadOnly);
			Assert("Should be readonly", invoiceLine._99ClaimedHMFInfo.ReadOnly);
			Assert("Should be readonly", invoiceLine._99ClaimedOtherFeesInfo.ReadOnly);
			Assert("Should be readonly", drawbackOtherFee._99ClaimedAmountInfo.ReadOnly);

			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			AssertEquals(false, invoiceLine.IsAutoCalculated);

			Assert("Should not be readonly", !invoiceLine._99ClaimedDutyInfo.ReadOnly);
			Assert("Should not be readonly", !invoiceLine._99ClaimedTaxInfo.ReadOnly);
			Assert("Should not be readonly", !invoiceLine._99ClaimedMPFInfo.ReadOnly);
			Assert("Should not be readonly", !invoiceLine._99ClaimedHMFInfo.ReadOnly);
			Assert("Should not be readonly", invoiceLine._99ClaimedOtherFeesInfo.ReadOnly);
			Assert("Should not be readonly", !drawbackOtherFee._99ClaimedAmountInfo.ReadOnly);
		}

		public void TestUS_DRWCalcDutyWithAdValoremRateWithOverriddenEnteredValue()
		{
			var tariff = Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "100231450")) ?? GetImportTariff("100231450", "KG");
			tariff.UE_DutyComputationCode = "7";
			tariff.UE_Column1RateAdValorem = 0.01m;

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";

			importDeclaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";
			var importInvoiceLine = importDeclaration.InvoiceLines.AddNew();
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.JI_Tariff = "100231450";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC" + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.JI_Tariff = "100231450";
			invoiceLine.US_DRWClaimAmountOverriden_New = true; //should default advalorem details on this point
			Assert("PReCondition", invoiceLine.US_DRWCalcDutyWithAdValoremRate);
			AssertEquals("PreCondition", 1m, invoiceLine.US_DRWAdValoremRate);

			invoiceLine.DeclaredVFD = 10000m;
			AssertEquals(100m, invoiceLine.LineDuty);
			invoiceLine.DeclaredVFD = 1000m;
			AssertEquals(10m, invoiceLine.LineDuty);

			invoiceLine.US_DRWCalcDutyWithAdValoremRate = false;
			AssertEquals(0m, invoiceLine.US_DRWAdValoremRate);

			invoiceLine.US_DRWClaimAmountOverriden_New = false;
			AssertEquals("lineduty should display as original import entry line", 100m, invoiceLine.LineDuty);
		}

		[TestDate(2016, 8, 18)]
		public void TestIACEDrawbackImportClaim()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "P1";
			invoiceLine.US_ImpDecInvoiceNum = "INV1";
			invoiceLine.US_DRWImpActInd = "D";
			invoiceLine.US_ImportEntryNo = "ABC12345678";
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.US_DRWCDInd = true;
			invoiceLine.US_DRWImpManufRuleNo = "1234567890";
			invoiceLine.US_DRWClaimBasis = "01";
			invoiceLine.US_DRWDateRcvFrom = ZDateTime.Today;
			invoiceLine.US_DRWDateUsedFrom = ZDateTime.Today.AddDays(5);
			invoiceLine.JI_Tariff = "100231450";
			invoiceLine.JI_Description = "TEST IMPORT DESC";
			invoiceLine.US_DRWImpTrkID = "12345";
			invoiceLine.US_DRWAccMethod = DrawbackAccountingMethodCodeList.Codes._01;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.US_DRWWeightedRatio = 1m;
			invoiceLine.US_DRWMPFWeightedRatio = 1m;
			invoiceLine.DRWImportQuantity = 123m;
			invoiceLine.DRWImportUQ = "KG";
			invoiceLine.DRWExportQuantity = 100m;
			invoiceLine.DRWExportUQ = "KG";
			invoiceLine.DRWAllowableQTY = 150m;
			invoiceLine.LineDuty = 100m;
			invoiceLine.DeclaredVFD = 200m;
			invoiceLine.DeclaredHMF = 500m;
			invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 200m);
			invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DomesticTax, 300m);

			var importAdditionalTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			importAdditionalTariff.US_Quantity1 = 99m;
			importAdditionalTariff.US_AllowableQty1 = 789m;
			importAdditionalTariff.US_Tariff = "1102324578";
			importAdditionalTariff.US_UQ1 = "NO";
			importAdditionalTariff.US_ValuePerUnit1 = 10m;
			importAdditionalTariff.US_SubstitutedValue1 = 10.1m;

			var importAdditionalTariff1 = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			importAdditionalTariff1.US_Tariff = "9901011000";
			importAdditionalTariff1.US_UQ1 = "NO";

			var importAdditionalTariff2 = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			importAdditionalTariff2.US_Tariff = "9801023000";
			importAdditionalTariff2.US_UQ1 = "NO";

			var importClaim = (IACEDrawbackImportClaim)invoiceLine;
			CombineAssertions(() =>
			{
				AssertEquals("D", importClaim.ActionIndicator);
				AssertEquals("ABC", importClaim.EntryFilerCode);
				AssertEquals("12345678", importClaim.EntryNumber);
				AssertEquals("1", importClaim.CBPESLine);
				AssertEquals("X", importClaim.CertificateofDeliveryIndicator);
				AssertEquals("1234567890", importClaim.ManufacturerRulingNumber);
				AssertEquals("01", importClaim.BasisOfClaim);
				AssertEquals(ZDateTime.Today, importClaim.ManufDateReceived);
				AssertEquals(ZDateTime.Today.AddDays(5), importClaim.ManufDateUsed);
				AssertEquals("12345", importClaim.TrackingIdentificationNumber);
				AssertEquals(DrawbackAccountingMethodCodeList.Codes._01, importClaim.DrawbackAccountingMethodCode);
				AssertEquals(4, importClaim.ImportClassifications.Count());
				AssertEquals(4, importClaim.RevenueAmounts.Count());

				var importClassificationList = importClaim.ImportClassifications.ToList();
				var importClassification0 = importClassificationList[0];
				AssertEquals("9801023000", importClassification0.HTSNumber);
				var exportQuantityAndUnit0 = importClassification0.ExportQuantityAndUnit;
				AssertEquals(0m, exportQuantityAndUnit0.Quantity);
				AssertEquals(0m, exportQuantityAndUnit0.AllowableQuantity);
				AssertEquals("KG", exportQuantityAndUnit0.UnitOfMeasure);
				AssertEquals(0m, exportQuantityAndUnit0.GoodsValuePerUnit);
				AssertEquals(0m, exportQuantityAndUnit0.SubstitutedValuePerUnit);

				var importClassification1 = importClassificationList[1];
				AssertEquals("9901011000", importClassification1.HTSNumber);
				var exportQuantityAndUnit1 = importClassification1.ExportQuantityAndUnit;
				AssertEquals(0m, exportQuantityAndUnit1.Quantity);
				AssertEquals(0m, exportQuantityAndUnit1.AllowableQuantity);
				AssertEquals("KG", exportQuantityAndUnit1.UnitOfMeasure);
				AssertEquals(0m, exportQuantityAndUnit1.GoodsValuePerUnit);
				AssertEquals(0m, exportQuantityAndUnit1.SubstitutedValuePerUnit);

				var importClassification2 = importClassificationList[2];
				AssertEquals("100231450", importClassification2.HTSNumber);
				AssertEquals("TEST IMPORT DESC, Invoice No.: INV1, Part No.: P1", importClassification2.DescriptionText);
				var exportQuantityAndUnit2 = importClassification2.ExportQuantityAndUnit;
				AssertEquals(100m, exportQuantityAndUnit2.Quantity);
				AssertEquals("KG", exportQuantityAndUnit2.UnitOfMeasure);
				AssertEquals(150m, exportQuantityAndUnit2.AllowableQuantity);
				AssertEquals(1.626m, exportQuantityAndUnit2.GoodsValuePerUnit);

				var importClassification3 = importClassificationList[3];
				AssertEquals("1102324578", importClassification3.HTSNumber);
				var exportQuantityAndUnit3 = importClassification3.ExportQuantityAndUnit;
				AssertEquals(99m, exportQuantityAndUnit3.Quantity);
				AssertEquals(789m, exportQuantityAndUnit3.AllowableQuantity);
				AssertEquals("KG", exportQuantityAndUnit3.UnitOfMeasure);
				AssertEquals(10m, exportQuantityAndUnit3.GoodsValuePerUnit);
				AssertEquals(10.1m, exportQuantityAndUnit3.SubstitutedValuePerUnit);

				var revenueAmountList = importClaim.RevenueAmounts.ToList();
				var dutyRevenue = revenueAmountList[0];
				AssertEquals(DrawbackOtherFeeTypesList.Codes.DrawbackDuty, dutyRevenue.AccountingClassCode);
				AssertEquals(80.48m, dutyRevenue.ClaimAmount);
				var hmfRevenue = revenueAmountList[1];
				AssertEquals(DrawbackOtherFeeTypesList.Codes.DrawbackHMF, hmfRevenue.AccountingClassCode);
				AssertEquals(402.44m, hmfRevenue.ClaimAmount);
				var oilSpillRevenue = revenueAmountList[2];
				AssertEquals(DrawbackOtherFeeTypesList.Codes.OilSpillTax, oilSpillRevenue.AccountingClassCode);
				AssertEquals(160.97m, oilSpillRevenue.ClaimAmount);
				var domesticRevenue = revenueAmountList[3];
				AssertEquals(DrawbackOtherFeeTypesList.Codes.DomesticTax, domesticRevenue.AccountingClassCode);
				AssertEquals(241.46m, domesticRevenue.ClaimAmount);
			});
		}

		[TestDate(2016, 8, 18)]
		public void TestIACEDrawbackManufactureClaim()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWMafActInd = "D";
			invoiceLine.US_DRWImpManufRuleNo = "1234567890";
			invoiceLine.JI_Tariff = "100231450";
			invoiceLine.US_DRWQuantityUsed = 123m;
			invoiceLine.US_DRWUQUsed = "KG";
			invoiceLine.US_DRWDateOfManufacture = ZDateTime.Today;
			invoiceLine.US_DRWFactoryLocation = "AA BCDEF";
			invoiceLine.US_DRWDescrManufactured = "1234 DDDSA";
			invoiceLine.US_DRWManufRuleNo = "2345678901";
			invoiceLine.US_DRWMafTrkID = "12345";

			var manufClaim = (IACEDrawbackManufactureClaim)invoiceLine;
			CombineAssertions(() =>
			{
				AssertEquals("D", manufClaim.ActionIndicator);
				AssertEquals("1234567890", manufClaim.ImportManufactureRulingNumber);
				AssertEquals("100231450", manufClaim.HTSNumber);
				AssertEquals(123m, manufClaim.Quantity);
				AssertEquals("KG", manufClaim.UnitOfMeasure);
				AssertEquals(ZDateTime.Today, manufClaim.ProductionDate);
				AssertEquals("AA BCDEF", manufClaim.FactoryLocation);
				AssertEquals("1234 DDDSA", manufClaim.DescriptionText);
				AssertEquals("2345678901", manufClaim.ManufactureRulingNumber);
				AssertEquals("12345", manufClaim.ManufacturedTrackingID);
			});
		}

		[TestDate(2016, 8, 18)]
		public void TestIACEDrawbackExportAndTFTEAClaim()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTEXP";
			orgHeader.OH_FullName = "TEST EXPORT ORG";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "P1";
			invoiceLine.US_ImpDecInvoiceNum = "INV1";
			invoiceLine.US_DRWExportAction = "D";
			invoiceLine.US_TariffType = TariffTypeList.Codes.ScheduleB;
			invoiceLine.US_ExportTariff = "100231451";
			invoiceLine.US_DRWExportQuantity = 150m;
			invoiceLine.US_DRWExportUQ = "KG";
			invoiceLine.US_DRWExportDate = ZDateTime.Today;
			invoiceLine.US_DRWExpNoticeInd = true;
			invoiceLine.US_DRWExpWavInd = true;
			invoiceLine.US_OH_DRWExporterOrDestroyer = orgHeader.PK;
			invoiceLine.US_DRWExportDest = Core.Constants.CountryCodes.Singapore;
			invoiceLine.US_DRWExpBOLInd = true;
			invoiceLine.US_DRWExpBOLCarrier = "ABCD";
			invoiceLine.JI_Description = "TEST IMPORT DESC";
			invoiceLine.US_DRWExportID = "AAAABBBBCC";

			var exportClaim = (IACEDrawbackExportClaim)invoiceLine;
			CombineAssertions(() =>
			{
				AssertEquals("D", exportClaim.ExportDestroyIndicator);
				AssertEquals("100231451", exportClaim.HTSNumber);
				AssertEquals(150m, exportClaim.Quantity);
				AssertEquals("KG", exportClaim.UnitOfMeasure);
				AssertEquals(ZDateTime.Today, exportClaim.ExportDate);
				AssertEquals("Y", exportClaim.NoticeOfIntentIndicator);
				AssertEquals("Y", exportClaim.WaiverToDrawbackIndicator);
				AssertEquals("TEST EXPORT ORG", exportClaim.NameOfExporter);
				AssertEquals(Core.Constants.CountryCodes.Singapore, exportClaim.CountryOfUltimateDestination);
				AssertEquals("Y", exportClaim.BOLIndicator);
				AssertEquals("ABCD", exportClaim.BOLCarrierCode);
				AssertEquals("TEST IMPORT DESC, Invoice No.: INV1, Part No.: P1", exportClaim.DescriptionText);
				AssertEquals("AAAABBBBCC", exportClaim.UniqueIdentifierNumber);
			});

			orgHeader.OH_FullName = "THIS IS A VERY LONG COMPANY NAME;THIS IS A LONG COMPANY NAME;";
			invoiceLine.JI_Description = "TEST EXPORT DESC";
			var tfteaClaim = (IACEDrawbackTFTEAClaim)invoiceLine;
			CombineAssertions(() =>
			{
				AssertEquals("D", tfteaClaim.ExportDestroyIndicator);
				AssertEquals("100231451", tfteaClaim.HTSNumber);
				AssertEquals(150m, tfteaClaim.Quantity);
				AssertEquals("KG", tfteaClaim.UnitOfMeasure);
				AssertEquals(ZDateTime.Today, tfteaClaim.ExportDate);
				AssertEquals("Y", tfteaClaim.NoticeOfIntentIndicator);
				AssertEquals("Y", tfteaClaim.WaiverToDrawbackIndicator);
				AssertEquals("THIS IS A VERY LONG COMPANY NA", tfteaClaim.NameOfExporter);
				AssertEquals(Core.Constants.CountryCodes.Singapore, tfteaClaim.CountryOfUltimateDestination);
				AssertEquals("Y", tfteaClaim.BOLIndicator);
				AssertEquals("ABCD", tfteaClaim.BOLCarrierCode);
				AssertEquals("TEST EXPORT DESC, Invoice No.: INV1, Part No.: P1", tfteaClaim.DescriptionText);
				AssertEquals("AAAABBBBCC", tfteaClaim.UniqueIdentifierNumber);
				AssertEquals("X", tfteaClaim.ScheduleBCode);
			});
		}

		public void TestOtherFessImportedForACEDrawback()
		{
			var referenceDeclaration = Factory.New<JobDeclaration>();
			referenceDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			referenceDeclaration.JE_DeclarationReference = "B12345678";
			referenceDeclaration.US_SchDEntry = "2210";
			referenceDeclaration.JE_EntryAuthorisationDate = new ZDate(2011, 08, 01);
			referenceDeclaration.JE_DateOfArrival = new ZDate(2011, 07, 25);
			var referenceInvoice = referenceDeclaration.Invoices.AddNew();
			var referenceInvoiceLine = referenceInvoice.JobComInvoiceLines.AddNew();
			var referenceEntryHeader = referenceDeclaration.CustomsEntryHeaders.AddNew();
			referenceEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var referenceEntryLine = referenceEntryHeader.MergedLines.AddNew();
			referenceEntryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			referenceEntryLine.CL_LineNumber = 2;
			referenceInvoiceLine.JI_CL = referenceEntryLine.PK;
			referenceEntryHeader.EntryNumber = "33333333";
			referenceEntryLine.CL_AdValoremTariff = "1111111111";
			referenceInvoiceLine.JI_InvoiceUQ = "NO";
			referenceInvoiceLine.JI_InvoiceQuantity = 15m;
			referenceInvoiceLine.JI_CustomsUnitQty = "NO";
			referenceInvoiceLine.JI_CustomsQuantity = 12m;

			referenceEntryLine.CL_DutyPercent = 5m;
			referenceEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 250m);
			referenceEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Beef, 100m);
			referenceEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 200m);
			referenceEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseInformal, 300m);
			referenceEntryLine.Fees.AddOrUpdate("111", 50m);
			referenceEntryLine.CL_Description = "STUFF";
			referenceEntryLine.CL_CustomsValue = 250m;

			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceLine = InvoiceLine;
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "XJ533333333-2";

			CombineAssertions(() =>
			{
				AssertEquals("XJ533333333", invoiceLine.US_ImportEntryNo);
				AssertEquals(2, invoiceLine.US_DRWImportEntryLine);
				AssertEquals("1111111111", invoiceLine.JI_Tariff);
				AssertEquals(0m, invoiceLine._99ClaimedDuty);
				AssertEquals("STUFF", invoiceLine.JI_Description);
				AssertEquals(new ZDate(2011, 07, 25), invoiceLine.US_DRWEntryDate);
				AssertEquals("2210", invoiceLine.US_DRWPort);
				AssertEquals(false, invoiceLine.US_DRWClaimAmountOverriden_New);
				AssertEquals(3, invoiceLine.DrawbackOtherFees.Count);
				var drawbackBeefFee = invoiceLine.DrawbackOtherFees[DrawbackOtherFeeTypesList.Codes.BeefFee];
				AssertNotNull(drawbackBeefFee);
				AssertEquals(100m, drawbackBeefFee.DeclaredAmount);
				var drawbackCottonFee = invoiceLine.DrawbackOtherFees[DrawbackOtherFeeTypesList.Codes.CottonFee];
				AssertNotNull(drawbackCottonFee);
				AssertEquals(200m, drawbackCottonFee.DeclaredAmount);
				var drawbackOtherFee = invoiceLine.DrawbackOtherFees[DrawbackOtherFeeTypesList.Codes.OtherFee];
				AssertNotNull(drawbackOtherFee);
				AssertEquals(300m, drawbackOtherFee.DeclaredAmount);
			});
		}

		public void TestUpdateDescriptionFromDrawbackImportEntryLineWhenIsImporting()
		{
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var referenceEntryHeader = importDeclaration.CustomsEntryHeaders.AddNew();
			referenceEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			referenceEntryHeader.EntryNumber = "33333333";

			for (var i = 1; i < 3; i++)
			{
				var entryLine = referenceEntryHeader.MergedLines.AddNew();
				entryLine.CL_Description = i.ToString("000");
				entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
				entryLine.CL_LineNumber = new ZShort(i.ToString());
			}
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_DeclarationReference = "B20180129";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_Description = "AAA";

			invoiceLine.US_ImportEntryNo = "XJ533333333-1";
			AssertEquals("AAA", invoiceLine.JI_Description);

			invoiceLine.JI_Description = string.Empty;
			invoiceLine.US_ImportEntryNo = "XJ533333333-2";
			AssertEquals("002", invoiceLine.JI_Description);
		}

		public void TestIACEDrawbackTrackingNumberLine()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			((IACEDrawbackTrackingNumberLine)invoiceLine).ArrangeTrackingNumbers(0);
			AssertEquals(ZString.Empty, invoiceLine.US_DRWImpTrkID);
			AssertEquals(ZString.Empty, invoiceLine.US_DRWMafTrkID);

			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWIsForManufacturerSection = true;
			((IACEDrawbackTrackingNumberLine)invoiceLine).ArrangeTrackingNumbers(0);
			AssertEquals(ZString.Empty, invoiceLine.US_DRWImpTrkID);
			AssertEquals(ZString.Empty, invoiceLine.US_DRWMafTrkID);

			dec.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			((IACEDrawbackTrackingNumberLine)invoiceLine).ArrangeTrackingNumbers(0);
			AssertEquals("00001", invoiceLine.US_DRWImpTrkID);
			AssertEquals("50001", invoiceLine.US_DRWMafTrkID);

			dec.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			((IACEDrawbackTrackingNumberLine)invoiceLine).ArrangeTrackingNumbers(0);
			AssertEquals(ZString.Empty, invoiceLine.US_DRWImpTrkID);
			AssertEquals(ZString.Empty, invoiceLine.US_DRWMafTrkID);
		}

		public void TestDefaultDescriptionAndQuantityForDrawbackAdditionalTariff()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "100231450";
			tariff1.UE_ShortDescription = "IAN TEST MAIN TARIFF";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff1.UE_DutyComputationCode = "7";
			tariff1.UE_Column1RateAdValorem = 0.01m;
			tariff1.UE_Unit1 = "KG";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "100231451";
			tariff2.UE_ShortDescription = "IAN TEST CHILD TARIFF 1";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_DutyComputationCode = "7";
			tariff2.UE_Column1RateAdValorem = 0.015m;

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";

			var importInvoiceLine = importDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.JI_Tariff = "100231450";
			importInvoiceLine.JI_CustomsUnitQty = "KG";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importInvoiceLine.JI_CustomsSecondUnitQty = "NO";
			importInvoiceLine.JI_CustomsSecondQuantity = 300m;
			importInvoiceLine.JI_CustomsThirdUnitQty = "X";
			importInvoiceLine.JI_CustomsThirdQuantity = 400m;
			importInvoiceLine.US_SupTariff = "100231451";
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC" + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;

			AssertEquals("100231450", invoiceLine.JI_Tariff);
			AssertEquals("KG", invoiceLine.DRWImportUQ);
			AssertEquals(200m, invoiceLine.DRWImportQuantity);
			AssertEquals("NO", invoiceLine.DRWImportUQ2);
			AssertEquals(300m, invoiceLine.DRWImportQuantity2);
			AssertEquals("X", invoiceLine.DRWImportUQ3);
			AssertEquals(400m, invoiceLine.DRWImportQuantity3);

			AssertEquals(1, invoiceLine.DrawbackAdditionalImportTariffNumbers.Count);
			var additionalImportTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers[0];
			AssertEquals("100231451", additionalImportTariff.US_Tariff);
			AssertEquals("IAN TEST CHILD TARIFF 1", additionalImportTariff.US_Description);
			AssertEquals("KG", additionalImportTariff.US_UQ1);
			AssertEquals(0m, additionalImportTariff.US_Quantity1);
			AssertEquals("NO", additionalImportTariff.US_UQ2);
			AssertEquals(0m, additionalImportTariff.US_Quantity2);
			AssertEquals("X", additionalImportTariff.US_UQ3);
			AssertEquals(0m, additionalImportTariff.US_Quantity3);
		}

		public void TestDefaultValuePerUnitForSTNTariff()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "910231450";
			tariff1.UE_ShortDescription = "IAN TEST MAIN TARIFF";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff1.UE_DutyComputationCode = "7";
			tariff1.UE_Column1RateAdValorem = 0.01m;
			tariff1.UE_Unit1 = "KG";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "910231451";
			tariff2.UE_ShortDescription = "IAN TEST CHILD TARIFF 1";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_DutyComputationCode = "7";
			tariff2.UE_Column1RateAdValorem = 0.015m;
			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_Tariff = tariff1.UE_Tariff;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			var ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff.U3_U1 = tariffRule.PK;
			ruleSecondaryTariff.U3_TariffFrom = tariff2.UE_Tariff;
			ruleSecondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;
			Factory.Save();

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";
			var importInvoiceLine1 = importDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			importInvoiceLine1.JI_Tariff = tariff1.UE_Tariff;
			importInvoiceLine1.JI_CustomsQuantity = 100m;
			importInvoiceLine1.JI_LinePrice = 1500m;
			AssertEquals(2, importDeclaration.FilteredInvoiceLines.Count);
			var importInvoiceLine2 = importDeclaration.FilteredInvoiceLines[1];
			AssertEquals(tariff2.UE_Tariff, importInvoiceLine2.JI_Tariff);
			importInvoiceLine2.JI_CustomsQuantity = 100m;
			importInvoiceLine2.JI_LinePrice = 300m;
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC" + importDeclaration.ImportEntryNumber + "-1";
			CombineAssertions(() =>
			{
				AssertEquals("910231450", invoiceLine.JI_Tariff);
				AssertEquals(15m, invoiceLine.DRWGoodsValuePerUQ);
				AssertEquals("KG", invoiceLine.DRWImportUQ);
				AssertEquals(100m, invoiceLine.DRWImportQuantity);

				AssertEquals(1, invoiceLine.DrawbackAdditionalImportTariffNumbers.Count);
				var additionalImportTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers[0];
				AssertEquals("910231451", additionalImportTariff.US_Tariff);
				AssertEquals(3m, additionalImportTariff.US_ValuePerUnit1);
				AssertEquals("KG", additionalImportTariff.US_UQ1);
				AssertEquals(0m, additionalImportTariff.US_Quantity1);

				AssertEquals("Value Per Unit(15+3=18) doesn't match Entered Value/Import Quantity(1800/100=18).", invoiceLine.ValuePerUQNotMatchNotification);
				additionalImportTariff.US_ValuePerUnit1 = 0m;
				AssertEquals("Value Per Unit(15) doesn't match Entered Value/Import Quantity(1800/100=18).", invoiceLine.ValuePerUQNotMatchNotification);
				additionalImportTariff.US_ValuePerUnit1 = 5.13m;
				AssertEquals("Value Per Unit(15+5.13=20.13) doesn't match Entered Value/Import Quantity(1800/100=18).", invoiceLine.ValuePerUQNotMatchNotification);
				invoiceLine.DRWImportQuantity = 0m;
				AssertEquals("Value Per Unit(5.13) doesn't match Entered Value/Import Quantity(0).", invoiceLine.ValuePerUQNotMatchNotification);
				additionalImportTariff.US_ValuePerUnit1 = 0m;
				AssertEquals("Value Per Unit(0) doesn't match Entered Value/Import Quantity(0).", invoiceLine.ValuePerUQNotMatchNotification);
				invoiceLine.DRWImportQuantity = 0.83333m;
				AssertEquals("Value Per Unit(1800.072) doesn't match Entered Value/Import Quantity(1800/0.8333=2160.0864).", invoiceLine.ValuePerUQNotMatchNotification);
				invoiceLine.DeclaredVFD = 1500.01m;
				invoiceLine.DRWImportQuantity = 830m;
				AssertEquals(false, invoiceLine.IsValuePerUQNotMatch);
				AssertEquals("Value Per Unit(1.8072) doesn't match Entered Value/Import Quantity(1500.01/830=1.8072).", invoiceLine.ValuePerUQNotMatchNotification);
				AssertNoWarnings(invoiceLine.US_DRWValuePerUQInfo);
			});
		}

		public void TestIsValuePerUQNotMatch()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.JI_Tariff = "0403105000";

			invoiceLine.DRWImportQuantity = 0.0833m;
			invoiceLine.DRWImportUQ = ABIUnitOfMeasureList.Codes.Dozen;

			invoiceLine.DeclaredVFD = 80.79m;
			AssertEquals("invoiceLine.DRWGoodsValuePerUQ", 969.8679m, invoiceLine.DRWGoodsValuePerUQ);
			AssertEquals("invoiceLine.IsValuePerUQNotMatch", false, invoiceLine.IsValuePerUQNotMatch);
		}

		public void TestDefaultAccountingMethodForACEDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.US_DRWAccMethod);
			invoiceLine.US_DRWIsForImportSection = true;
			AssertEquals(DrawbackAccountingMethodCodeList.Codes._00, invoiceLine.US_DRWAccMethod);

			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			AssertEquals(ZString.Empty, invoiceLine.US_DRWAccMethod);
		}

		public void TestDrawbackEnteredValuePer()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._51;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.DRWImportQuantity = 1000m;
			invoiceLine.DRWImportUQ = "XX";
			invoiceLine.DeclaredVFD = 200m;
			invoiceLine.DRWGoodsValuePerUQ = 7.1234m;

			AssertEquals("7.1234", ((IDrawback7551ImportDocLine)invoiceLine).DrawbackEnteredValuePer);

			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("0.2", ((IDrawback7551ImportDocLine)invoiceLine).DrawbackEnteredValuePer);
		}

		public void TestDefaultUS_DRWQuarterlyHMFOnChangeOfImportDeclarationNumber()
		{
			var referencetDec = Factory.New<JobDeclaration>();
			referencetDec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			referencetDec.US_EntryFilerCode = "ABC";
			var entryHeader = referencetDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "ENTRY001";
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var decInvoice = referencetDec.Invoices.AddNew();
			var decInvoiceLine = decInvoice.InvoiceLines.AddNew();
			decInvoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;
			line1.US_ImportEntryNo = "ABCENTRY001";
			line1.US_DRWImportEntryLine = 1;
			AssertEquals(true, line1.US_DRWQuarterlyHMF);

			referencetDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();
			drawback = NewFactory().New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoice = drawback.Invoices.AddNew();
			line1 = invoice.InvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;
			line1.US_ImportEntryNo = "ABCENTRY001";
			line1.US_DRWImportEntryLine = 1;
			AssertEquals(false, line1.US_DRWQuarterlyHMF);
		}

		public void TestDrawbackCountryOfDestinationSetToOuterSpace()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.US_DRWExportDest = "FZ";
			AssertNoErrors(invoiceLine.US_DRWExportDestInfo);
			AssertEquals(invoiceLine.US_DRWExportDestDescription, "FTZ");

			invoiceLine.US_DRWExportDest = "FN";
			AssertNoErrors(invoiceLine.US_DRWExportDestInfo);
			AssertEquals(invoiceLine.US_DRWExportDestDescription, "Petroleum Products");

			invoiceLine.US_DRWExportDest = "FF";
			AssertNoErrors(invoiceLine.US_DRWExportDestInfo);
			AssertEquals(invoiceLine.US_DRWExportDestDescription, "Outer Space");
		}

		public void TestGetViewDrawbackImportEntryLineData()
		{
			var referencetDec = Factory.New<JobDeclaration>();
			referencetDec.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			referencetDec.US_EntryFilerCode = "ABC";
			var entryHeader = referencetDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "ENTRY001";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "123456";
			entryLine1.US_SupLine = true;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 1;
			entryLine2.US_ChildLineNum = 1;
			entryLine2.CL_AdValoremTariff = "1234567";
			entryLine2.US_CL_ParentLine = entryLine1.PK;
			var entryLine3 = entryHeader.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 1;
			entryLine3.US_ChildLineNum = 2;
			entryLine3.CL_AdValoremTariff = "12345678";
			entryLine3.US_CL_ParentLine = entryLine1.PK;
			var decInvoice = referencetDec.Invoices.AddNew();
			var decInvoiceLine = decInvoice.InvoiceLines.AddNew();
			decInvoiceLine.JI_CL = entryLine1.PK;
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;
			line1.US_ImportEntryNo = "ABCENTRY001";
			line1.US_DRWImportEntryLine = 1;
			AssertEquals("1234567", line1.DrawbackImportEntryLine.CL_AdValoremTariff);

			entryLine1.US_SupLine = false;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			invoice = factory.Load<JobComInvoiceHeader>(invoice.PK);
			invoice.InvoiceLines.RemoveAndDeleteAll();
			line1 = invoice.InvoiceLines.AddNew();
			line1.US_DRWIsForImportSection = true;
			line1.US_ImportEntryNo = "ABCENTRY001";
			line1.US_DRWImportEntryLine = 1;
			AssertEquals("123456", line1.DrawbackImportEntryLine.CL_AdValoremTariff);
		}

		public void TestGoodsValuePerUnitAndExportValue()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "100231450";
			tariff1.UE_ShortDescription = "IAN TEST MAIN TARIFF";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff1.UE_DutyComputationCode = "7";
			tariff1.UE_Column1RateAdValorem = 0.01m;
			tariff1.UE_Unit1 = "KG";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "100231451";
			tariff2.UE_ShortDescription = "IAN TEST CHILD TARIFF 1";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_DutyComputationCode = "7";
			tariff2.UE_Column1RateAdValorem = 0.015m;
			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.EligibleForSecondaryTariffNumbers;
			tariffRule.U1_Tariff = tariff1.UE_Tariff;
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			var ruleSecondaryTariff = Factory.New<USCRuleSecondaryTariff>();
			ruleSecondaryTariff.U3_U1 = tariffRule.PK;
			ruleSecondaryTariff.U3_TariffFrom = tariff2.UE_Tariff;
			ruleSecondaryTariff.U3_DateFrom = ZDateTime.BrettsBirthday;
			Factory.Save();

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_EntryFilerCode = "ABC";
			var importInvoiceLine1 = importDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			importInvoiceLine1.JI_Tariff = tariff1.UE_Tariff;
			importInvoiceLine1.JI_CustomsQuantity = 100m;
			importInvoiceLine1.JI_LinePrice = 1500m;
			importInvoiceLine1.US_SupTariff = "99038801";
			AssertEquals(2, importDeclaration.FilteredInvoiceLines.Count);
			var importInvoiceLine2 = importDeclaration.FilteredInvoiceLines[1];
			AssertEquals(tariff2.UE_Tariff, importInvoiceLine2.JI_Tariff);
			importInvoiceLine2.JI_CustomsQuantity = 100m;
			importInvoiceLine2.JI_LinePrice = 300m;
			importDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC" + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;

			AssertEquals("100231450", invoiceLine.JI_Tariff);
			AssertEquals(15m, invoiceLine.DRWGoodsValuePerUQ);
			AssertEquals(18m, invoiceLine.Claims.DutyClaim.ValuePerUnitIncludingSecondaryLines);
			invoiceLine.DRWExportQuantity = 5;
			AssertEquals(90m, invoiceLine.ExportValue);

			AssertEquals(2, invoiceLine.DrawbackAdditionalImportTariffNumbers.Count);
			var additionalImportTariff1 = invoiceLine.DrawbackAdditionalImportTariffNumbers[0];
			AssertEquals("99038801", additionalImportTariff1.US_Tariff);
			AssertEquals(0m, additionalImportTariff1.US_ValuePerUnit1);
			var additionalImportTariff2 = invoiceLine.DrawbackAdditionalImportTariffNumbers[1];
			AssertEquals("100231451", additionalImportTariff2.US_Tariff);
			AssertEquals(3m, additionalImportTariff2.US_ValuePerUnit1);
		}

		public void TestRevenueAmountsWhenAccPaymentTicked()
		{
			#region Create Import Declaration

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var importInvoice = importDec.Invoices.AddNew();
			importInvoice.JZ_InvoiceNumber = "INV1234";

			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_CustomsUnitQty = "NO";
			importInvoiceLine.JI_CustomsQuantity = 2000m;
			importInvoiceLine.US_PayableMPF = 100m;
			importInvoiceLine.US_CustomsValue = 1000m;
			importInvoiceLine.JI_Description = "TEST DRAWBACK DESCRIPTION";

			var importEntryHeader = importDec.CustomsEntryHeaders.AddNew();
			importEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			importEntryHeader.EntryNumber = "12345678";
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);

			var importEntryLine = importEntryHeader.MergedLines.AddNew();
			importEntryLine.CL_LineNumber = 1;
			importEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 500m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 200m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 500m);
			importEntryLine.CL_CustomsValue = 1000m;
			importInvoiceLine.JI_CL = importEntryLine.PK;
			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._58;
			declaration.US_AcceleratedClaimInd = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "XJ512345678-1";
			invoiceLine.DRWExportQuantity = 10m;

			var importClaim = (IACEDrawbackImportClaim)invoiceLine;
			CombineAssertions("Acc Payment Ticked", () =>
			{
				AssertEquals("RevenueAmounts count", 2, importClaim.RevenueAmounts.Count());
				Assert("RevenueAmounts has 364 fee code", importClaim.RevenueAmounts.FirstOrDefault(x => x.AccountingClassCode == DrawbackOtherFeeTypesList.Codes.DrawbackDuty) != null);
				Assert("RevenueAmounts has 399 fee code", importClaim.RevenueAmounts.FirstOrDefault(x => x.AccountingClassCode == DrawbackOtherFeeTypesList.Codes.DrawbackMPF) != null);
			});

			declaration.US_AcceleratedClaimInd = false;
			importClaim = invoiceLine;
			CombineAssertions("Acc Payment Ticked", () =>
			{
				AssertEquals("RevenueAmounts count", 3, importClaim.RevenueAmounts.Count());
				Assert("RevenueAmounts has 364 fee code", importClaim.RevenueAmounts.FirstOrDefault(x => x.AccountingClassCode == DrawbackOtherFeeTypesList.Codes.DrawbackDuty) != null);
				Assert("RevenueAmounts has 399 fee code", importClaim.RevenueAmounts.FirstOrDefault(x => x.AccountingClassCode == DrawbackOtherFeeTypesList.Codes.DrawbackMPF) != null);
				Assert("RevenueAmounts has 056 fee code", importClaim.RevenueAmounts.FirstOrDefault(x => x.AccountingClassCode == DrawbackOtherFeeTypesList.Codes.CottonFee) != null);
			});
		}

		OrgHeader claimant;
		JobDeclaration GetDeclarationAndParts()
		{
			USCTariff tariff1 = GetImportTariff(ImpNoLocalTariffNum, "KG");
			Universal.TariffView scheduleB1 = GetExportTariff(ExpNoLocalTariffNum, "NO");
			USCTariff tariff2 = GetImportTariff(ImpWithLocalPartTariffNum, "KG");
			Universal.TariffView scheduleB2 = GetExportTariff(LocalPartTariffNum, "NO");
			Universal.TariffView scheduleB3 = GetExportTariff(ExpWithInvalidLocalTariffNum, "NO");
			Universal.TariffView scheduleB4 = GetExportTariff(DuelExportTariffNum, "NO");
			USCTariff tariff3 = GetImportTariff(DuelImportTariffNum, "KG");
			USCTariff tariff4 = GetImportTariff(ImpWithInvalidLocalTariffNum, "NO");
			Universal.TariffView scheduleB5 = GetExportTariff(DuelExportTariffNum2, "KG");
			USCTariff tariff5 = GetImportTariff(DuelImportTariffNum2, "NO");
			USCTariff tariff6 = GetImportTariff(ImpWithInvalidLocal2TariffNum, "NO");

			claimant = Factory.New<OrgHeader>();
			claimant.OH_Code = "CLAIMANT";

			OrgSupplierPart part1 = GetOnePart(ImpNoLocalPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Owner, ImpNoLocalTariffNum, ZString.Empty, ZString.Empty);
			OrgSupplierPart part2 = GetOnePart(ExpNoLocalPart, "BOX", 5m, OrgPartRelation.RelationshipTypes.Supplier, ZString.Empty, ExpNoLocalTariffNum, ZString.Empty);
			OrgSupplierPart part3 = GetOnePart(ImpWithLocalPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Owner, ImpWithLocalPartTariffNum, ZString.Empty, ValidLocalPart);
			OrgSupplierPart part4 = GetOnePart(ValidLocalPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Supplier, ZString.Empty, LocalPartTariffNum, ZString.Empty);
			OrgSupplierPart part5 = GetOnePart(ExpWithInvalidLocalPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Supplier, ZString.Empty, ExpWithInvalidLocalTariffNum, InValidLocalPart);
			OrgSupplierPart part6 = GetOnePart(DuelExportPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Supplier, DuelImportTariffNum, DuelExportTariffNum, ZString.Empty);
			OrgSupplierPart part7 = GetOnePart(DuelExportInvalidLocalPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Supplier, DuelImportTariffNum, DuelExportTariffNum, InValidLocalPart);
			OrgSupplierPart part8 = GetOnePart(ImpWithInvalidLocalPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Owner, ImpWithInvalidLocalTariffNum, ZString.Empty, InValidLocalPart);
			OrgSupplierPart part9 = GetOnePart(DuelImportPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Owner, DuelImportTariffNum2, DuelExportTariffNum2, ZString.Empty);
			OrgSupplierPart part10 = GetOnePart(DuelImportInvalidLocalPart, "UNT", 5m, OrgPartRelation.RelationshipTypes.Owner, DuelImportTariffNum2, DuelExportTariffNum2, InValidLocalPart);
			OrgSupplierPart part11 = GetOnePart(ImpWithInvalidLocalPart2, "UNT", 5m, OrgPartRelation.RelationshipTypes.Owner, ImpWithInvalidLocal2TariffNum, ZString.Empty, InValidLocalPart2);

			Factory.Save();

			JobDeclaration result = Factory.New<JobDeclaration>();
			result.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			result.JE_OH_Importer = claimant.PK;
			JobComInvoiceHeader invoice = result.Invoices.AddNew();
			return result;
		}
		const string InValidLocalPart = "INVALIDLOCAL";
		const string InValidLocalPart2 = "INVALIDLOCAL2";
		const string ImpNoLocalPart = "IMPNOLOCAL";
		const string ImpNoLocalTariffNum = "1111111110";
		const string ExpNoLocalPart = "EXPNOLOCAL";
		const string ExpNoLocalTariffNum = "1111111210";
		const string ImpWithLocalPart = "IMPWITHLOCAL";
		const string ImpWithLocalPartTariffNum = "1111111310";
		const string ValidLocalPart = "VALIDLOCALPART";
		const string LocalPartTariffNum = "1111111410";
		const string ExpWithInvalidLocalPart = "EXPWITHINVALIDLOCAL";
		const string ExpWithInvalidLocalTariffNum = "1111111510";
		const string DuelExportPart = "DUELEXPORT";
		const string DuelExportTariffNum = "1111111610";
		const string DuelImportTariffNum = "1111111710";
		const string DuelExportInvalidLocalPart = "DUELEXPORTINVALIDLOCAL";
		const string ImpWithInvalidLocalPart = "IMPWITHINVALIDLOCAL";
		const string ImpWithInvalidLocalTariffNum = "1111111810";
		const string DuelImportPart = "DUELIMPORT";
		const string DuelExportTariffNum2 = "1111111910";
		const string DuelImportTariffNum2 = "1111112010";
		const string DuelImportInvalidLocalPart = "DUELIMPORTINVALIDLOCAL";
		const string ImpWithInvalidLocalPart2 = "IMPWITHINVALIDLOCAL2";
		const string ImpWithInvalidLocal2TariffNum = "1111112110";

		OrgSupplierPart GetOnePart(ZString partNum, ZString partUnits, ZDecimal weight, string relationship, ZString importTariffItem, ZString exportTariffItem, ZString localPartNum)
		{
			var result = Factory.New<OrgSupplierPart>();
			result.OP_PartNum = partNum;
			result.OP_StockKeepingUnit = partUnits;
			result.OP_Weight = weight;
			result.OP_NetWeight = weight;
			result.OP_WeightUQ = "KG";

			OrgPartUnit units = result.PartUnits.AddNew();
			units.OF_QuantityInParent = 12m;
			units.OF_ParentPackType = "BOX";
			units.OF_PackType = "NO";

			OrgPartRelation relation = result.RelatedOrganisations.AddNew();
			relation.OU_OH = claimant.PK;
			relation.OU_Relationship = relationship;
			relation.OU_LocalPartNumber = localPartNum;
			if (!importTariffItem.IsEmpty)
			{
				CusClassification class1 = Factory.New<CusClassification>();
				class1.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				class1.CC_TariffNum = importTariffItem;
				class1.CC_LookupCode = class1.PK.ToString().Substring(0, 35);
				CusClassPartPivot importPivot = result.PivotsForBinding.AddNew();
				importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				importPivot.CI_CC = class1.PK;
			}
			if (!exportTariffItem.IsEmpty)
			{
				CusClassification class2 = Factory.New<CusClassification>();
				class2.CC_ClassificationType = CusClassification.ClassificationType.EXP;
				class2.CC_TariffNum = exportTariffItem;
				class2.CC_LookupCode = class2.PK.ToString().Substring(0, 35);
				CusClassPartPivot exportPivot = result.PivotsForBinding.AddNew();
				exportPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
				exportPivot.CI_CC = class2.PK;
			}
			return result;
		}

		USCTariff GetImportTariff(ZString tariffItem, ZString units)
		{
			USCTariff result = Factory.New<USCTariff>();
			result.UE_Tariff = tariffItem;
			result.UE_Unit1 = units;
			result.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			result.UE_DateTo = ZDateTime.Today.AddYears(1);
			return result;
		}

		Universal.TariffView GetExportTariff(ZString tariffItem, ZString units)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusTariffTypeSB = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
			Factory.Save();

			var scheduleB = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, cusTariffTypeSB.PK, tariffItem, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

			helper.CreateTariffUOM(scheduleB, "CU1", units);
			return scheduleB;
		}

		void AssertSuspendFieldSetter<T>(string propertyName, T value1, T value2, T value3)
			where T : IZType
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var info = invoiceLine.ZPropertyInfoHash[propertyName];
			info.Value = value1;
			using (invoiceLine.SetterSuspender.SuspendSetting(propertyName))
			{
				info.Value = value2;
				AssertEquals(value1, info.Value);
			}

			info.Value = value3;
			AssertEquals(value3, info.Value);
		}

		void ClearOverrideData(DrawbackClaims.IndividualClaim claim)
		{
			claim.DeclaredAmount = ZDecimal.Zero;
			claim.WeightedRatio = ZDecimal.Zero;
			claim.CalculatedAmount = ZDecimal.Zero;
		}

		void AssertClaim(ZString prefix, DrawbackClaims.IndividualClaim claim, ZDecimal declaredAmount, ZDecimal calculatedAmount, ZDecimal weightedRatio)
		{
			AssertEquals(prefix + " claim.DeclaredAmount", declaredAmount, claim.DeclaredAmount);
			AssertEquals(prefix + " claim.CalculatedAmount", calculatedAmount, claim.CalculatedAmount);
			AssertEquals(prefix + " claim.WeightedRatio", weightedRatio, claim.WeightedRatio);
		}

		void SetupData()
		{
			var referenceDeclaration = Factory.New<JobDeclaration>();
			referenceDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			referenceDeclaration.JE_DeclarationReference = "B12345678";
			referenceDeclaration.US_SchDEntry = "2210";
			referenceDeclaration.JE_EntryAuthorisationDate = new ZDate(2011, 08, 01);
			referenceDeclaration.JE_DateOfArrival = new ZDate(2011, 07, 25);
			var referenceInvoice = referenceDeclaration.Invoices.AddNew();
			var referenceInvoiceLine = referenceInvoice.JobComInvoiceLines.AddNew();
			var referenceEntryHeader = referenceDeclaration.CustomsEntryHeaders.AddNew();
			referenceEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var referenceEntryLine = referenceEntryHeader.MergedLines.AddNew();
			referenceEntryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			referenceEntryLine.CL_LineNumber = 2;
			referenceInvoiceLine.JI_CL = referenceEntryLine.PK;
			referenceEntryHeader.EntryNumber = "33333333";
			referenceEntryLine.CL_AdValoremTariff = "1111111111";
			referenceInvoiceLine.JI_InvoiceUQ = "NO";
			referenceInvoiceLine.JI_InvoiceQuantity = 15m;
			referenceInvoiceLine.JI_CustomsUnitQty = "NO";
			referenceInvoiceLine.JI_CustomsQuantity = 12m;
			referenceInvoiceLine.JI_Tariff = "1111111111";
			referenceEntryLine.CL_DutyPercent = 5m;
			referenceEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 250m);
			referenceEntryLine.CL_Description = "STUFF";
			referenceEntryLine.CL_CustomsValue = 250m;
			Factory.Save();

			var referenceEntryLine2 = referenceEntryHeader.MergedLines.AddNew();
			referenceEntryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			referenceEntryLine2.CL_LineNumber = 2;
			referenceEntryLine2.CL_AdValoremTariff = "9911111111";
			referenceEntryLine2.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 50m);
			referenceEntryLine2.US_SupLine = true;
			referenceInvoiceLine.US_SupTariff = "9911111111";
			referenceEntryLine.ResetTotalsAndCachedValues();
			referenceEntryLine.US_CL_ParentLine = referenceEntryLine2.PK;
			referenceEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 250m);
			referenceEntryLine.CL_CustomsValue = 1000m;
			referenceInvoiceLine.US_CustomsValue = 1000m;
			Factory.Save();
		}
	}

	sealed class JobComInvoiceLineTesting : JobComInvoiceLine
	{
		public JobComInvoiceLineTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString JI_Tariff
		{
			get { return base.JI_Tariff; }
			set
			{
				SetCount++;
				base.JI_Tariff = value;
			}
		}

		public int SetCount;
	}
}
