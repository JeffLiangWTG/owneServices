using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class DrawbackClaimsTest : TestCaseWithFactory
	{
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
			importDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var importEntryHeader = importDec.ActiveEntryHeaders.EntrySummaryEntry;
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			var importEntryLine = importEntryHeader.MergedLines[0];
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
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
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "XJ512345678-1";
			invoiceLine.US_DRWExportQuantity = 500m;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			var claims = invoiceLine.Claims;
			claims.DefaultOverrideData();
			CombineAssertions("Default", () =>
			{
				AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
				AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
				AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
				AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
				AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 801.90m, 1m);
				AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, 400m, 198m, 1m);
				AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 148.50m, 1m);
				AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, ZDecimal.Zero, ZDecimal.Zero, 1m);
				AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, ZDecimal.Zero, 1m);
			});
			invoiceLine.US_DRWExportQuantity = 100m;
			CombineAssertions("Export Quantity Update", () =>
			{
				AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
				AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
				AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
				AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
				AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 160.38m, 1m);
				AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, 400m, 39.60m, 1m);
				AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 29.70m, 1m);
				AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, ZDecimal.Zero, ZDecimal.Zero, 1m);
				AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, ZDecimal.Zero, 1m);
			});
			CombineAssertions("Setting empty and redefault", () =>
			{
				claims.GoodsValuePerUnit = ZDecimal.Zero;
				AssertEquals("claims.GoodsValuePerUnit", ZDecimal.Zero, claims.GoodsValuePerUnit);
				claims.GoodsValuePerUnit2 = ZDecimal.Zero;
				AssertEquals("claims.GoodsValuePerUnit2", ZDecimal.Zero, claims.GoodsValuePerUnit2);
				claims.GoodsValuePerUnit3 = ZDecimal.Zero;
				AssertEquals("claims.GoodsValuePerUnit3", ZDecimal.Zero, claims.GoodsValuePerUnit3);

				AssertClaimEmpty(nameof(claims.DutyClaim), claims.DutyClaim);
				ICalculationExhibitsSupporter supporter = claims.DutyClaim;
				AssertEquals("TotalLineValue", 10000m, supporter.TotalLineValue);
				AssertEquals("claims.DutyClaim.LineDuty", ZDecimal.Zero, claims.DutyClaim.LineDuty);
				AssertClaimEmpty(nameof(claims.HMFClaim), claims.HMFClaim);
				AssertClaimEmpty(nameof(claims.IRTaxClaim), claims.IRTaxClaim);
				AssertClaimEmpty(nameof(claims.MPFClaim), claims.MPFClaim);
				AssertClaimEmpty(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim);

				claims.DefaultOverrideData();
				AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
				AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
				AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
				AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
				AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 160.38m, 1m);
				AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, 400m, 39.60m, 1m);
				AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 29.7m, 1m);
				AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, ZDecimal.Zero, ZDecimal.Zero, 1m);
				AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, ZDecimal.Zero, 1m);
			});

			Factory.Save();
			var newFactory = new BusinessObjectFactory();

			var invoiceLineInDiffFactoryMock = newFactory.LoadMoq<JobComInvoiceLine>(invoiceLine.PK);
			var invoiceLineInDiffFactoryMockProtected = invoiceLineInDiffFactoryMock.Protected();
			invoiceLineInDiffFactoryMockProtected.Setup<Customs.Business.IBaseDrawbackEntryLine>("DrawbackImportEntryLineCore").Throws<InvalidOperationException>();

			var invoiceLineInDiffFactory = invoiceLineInDiffFactoryMock.Object;
			invoiceLineInDiffFactory.Validation.ValidateAll();
			AssertNoExceptionThrown(() => _ = invoiceLineInDiffFactory.Claims.MPFClaim.LineAmount);
		}

		void AssertClaimEmpty(ZString prefix, DrawbackClaims.IndividualClaim claim)
		{
			claim.DeclaredAmount = ZDecimal.Zero;
			AssertEquals(prefix + " claim.DeclaredAmount", ZDecimal.Zero, claim.DeclaredAmount);
			claim.CalculatedAmount = ZDecimal.Zero;
			AssertEquals(prefix + " claim.CalculatedAmount", ZDecimal.Zero, claim.CalculatedAmount);
			claim.WeightedRatio = ZDecimal.Zero;
			AssertEquals(prefix + " claim.WeightedRatio", ZDecimal.Zero, claim.WeightedRatio);
		}

		void AssertClaim(ZString prefix, DrawbackClaims.IndividualClaim claim, ZDecimal declaredAmount, ZDecimal calculatedAmount, ZDecimal weightedRatio)
		{
			AssertEquals(prefix + " claim.DeclaredAmount", declaredAmount, claim.DeclaredAmount);
			AssertEquals(prefix + " claim.CalculatedAmount", calculatedAmount, claim.CalculatedAmount);
			AssertEquals(prefix + " claim.WeightedRatio", weightedRatio, claim.WeightedRatio);
		}

		public void TestDutyDescFromTariff()
		{
			SetupImportEntry();
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			invoiceLine.JI_Tariff = "0403105000";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "ZZ";
			invoiceLine.JI_InvoiceQuantity = 500m;
			invoiceLine.JI_InvoiceUQ = "XX";
			invoiceLine.US_DRWCertOfManufacture = "CD1111";
			invoiceLine.JI_Description = "DESCRIP";
			invoiceLine.US_DRWClaimAmountOverriden_New = false;
			invoiceLine.Claims.DutyClaim.DefaultDutyRateDesc();
			var dutyRateDesc = invoiceLine.Claims.DutyClaim.LineDutyRateDesc;
			Assert(!invoiceLine.US_DRWCalcDutyWithAdValoremRate);
			AssertEquals("$1.035/ZZ + 17%", dutyRateDesc);
			AssertEquals(200m, invoiceLine.Claims.DutyClaim.LineDuty);
			AssertEquals(0.2m, invoiceLine.Claims.DutyClaim.DutyRate);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1234567891";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.16200000m;

			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.JI_Tariff = "1234567891";
			Assert(invoiceLine.US_DRWCalcDutyWithAdValoremRate);
			AssertEquals("16.2000%", invoiceLine.Claims.DutyClaim.LineDutyRateDesc);
			AssertEquals(0.162m, invoiceLine.Claims.DutyClaim.DutyRate);
		}

		public void TestLineDutyRateDesc()
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
			importDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC" + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;

			AssertEquals("1.5%+1%", invoiceLine.LineDutyRateDesc);
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			AssertEquals(2.5m, invoiceLine.US_DRWAdValoremRate);
			AssertEquals("2.500%", invoiceLine.LineDutyRateDesc);

			var additionalTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			additionalTariff.US_Tariff = "100231450";
			AssertEquals(3.5m, invoiceLine.US_DRWAdValoremRate);
			AssertEquals("1%+1.5%+1%", invoiceLine.LineDutyRateDesc);
		}

		public void TestLineDutyRateDescForFree()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "100231452";
			tariff.UE_ShortDescription = "TEST TARIFF 1";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_DutyComputationCode = "7";
			tariff.UE_Column1RateAdValorem = 0m;
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "100231453";
			tariff1.UE_ShortDescription = "TEST TARIFF 2";
			tariff1.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff1.UE_DutyComputationCode = "7";
			tariff1.UE_Column1RateAdValorem = 0m;
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "100231454";
			tariff2.UE_ShortDescription = "TEST TARIFF 3";
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
			importInvoiceLine.JI_Tariff = "100231452";
			importInvoiceLine.JI_CustomsUnitQty = "KG";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importInvoiceLine.US_SupTariff = "100231453";
			importDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Drawback;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "ABC" + importDeclaration.ImportEntryNumber;
			invoiceLine.US_DRWImportEntryLine = 1;

			AssertEquals(DutyResult.DutyFreeString, invoiceLine.LineDutyRateDesc);
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			AssertEquals(0m, invoiceLine.US_DRWAdValoremRate);
			AssertEquals(DutyResult.DutyFreeString, invoiceLine.LineDutyRateDesc);

			var importDeclaration1 = Factory.New<JobDeclaration>();
			importDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration1.US_EnableENS = true;
			importDeclaration1.US_EntryFilerCode = "ABC";

			var importInvoiceLine1 = importDeclaration1.Invoices.AddNew().InvoiceLines.AddNew();
			importInvoiceLine1.JI_LinePrice = 10000m;
			importInvoiceLine1.JI_Tariff = "100231452";
			importInvoiceLine1.JI_CustomsUnitQty = "KG";
			importInvoiceLine1.JI_CustomsQuantity = 200m;
			importInvoiceLine1.US_SupTariff = "100231454";
			importDeclaration1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.US_DRWIsForImportSection = true;
			invoiceLine1.US_ImportEntryNo = "ABC" + importDeclaration1.ImportEntryNumber;
			invoiceLine1.US_DRWImportEntryLine = 1;
			AssertEquals("1.5%", invoiceLine1.LineDutyRateDesc);
			invoiceLine1.US_DRWClaimAmountOverriden_New = true;
			AssertEquals(1.5m, invoiceLine1.US_DRWAdValoremRate);
			AssertEquals("1.500%", invoiceLine1.LineDutyRateDesc);
		}

		public void TestICalculationExhibitsSupporterMembers()
		{
			var importEntryHeader = SetupImportEntry();
			var importEntryLine = importEntryHeader.MergedLines[0];

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWExportUQ = "XX";
			invoiceLine.US_DRWExportQuantity = 500m;
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			invoiceLine.US_DRWCertOfManufacture = "CD1111";
			invoiceLine.JI_Description = "DESCRIP";

			IDutyCalculationExhibitsSupporter supporter = invoiceLine.Claims.DutyClaim;
			AssertEquals("ImportEntryOrCMDNo", "XJ5-1234567-8", supporter.ImportEntryOrCMDNo);
			AssertEquals("Invoice No", "INV1234", supporter.InvoiceNo);
			AssertEquals("Part No", "Part1", supporter.PartNo);
			AssertEquals("Import Quan", 1000m, supporter.ImportQuantity);
			AssertEquals("Export Quan", 500m, supporter.ExportQuantity);
			AssertEquals("Description", "DESCRIP", supporter.Description);
			AssertEquals("Per unit", 1m, supporter.PerUnit);
			AssertEquals("Amount Paid", 100m, supporter.AmountPaid);
			AssertEquals("Amount Claimed", 99m, supporter.AmountClaimed);
			AssertEquals("Export Value", 500m, supporter.ExportValue);
			AssertEquals("Duty Rate", .2m, supporter.DutyRate);

			invoiceLine.US_ImportEntryNo = ZString.Empty;
			AssertEquals("ImportEntryOrCMDNo", "CD1111", supporter.ImportEntryOrCMDNo);
			AssertEquals("Invoice No", ZString.Empty, supporter.InvoiceNo);

			invoiceLine.JI_PartNo = "SALAD";
			AssertEquals(@"If there is no import declaration attached to a drawback line, and there is a part specified on the drawback line 
then get the part number from the drawback line part", "SALAD", supporter.PartNo);

			importEntryLine.RandomLine.JI_PartNo = ZString.Empty;
			Factory.Save();

			drawback = new BusinessObjectFactory().New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			invoice = drawback.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWExportUQ = "XX";
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			invoiceLine.JI_PartNo = "SALAD";
			AssertEquals(ZString.Empty, invoiceLine.DrawbackImportEntryLine.RandomLine.JI_PartNo);
			supporter = invoiceLine.Claims.DutyClaim;
			AssertEquals(@"If linked declaration does not have a part, and there is a part specified on the drawback line 
then get the part number from the drawback line part.", "SALAD", supporter.PartNo);
		}

		CusEntryHeader SetupImportEntry(bool addSecondLine = false)
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var importInvoice = importDec.Invoices.AddNew();
			importInvoice.JZ_InvoiceNumber = "INV1234";

			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_InvoiceUQ = "XX";
			importInvoiceLine.JI_InvoiceQuantity = 1000m;
			importInvoiceLine.JI_CustomsUnitQty = "NO";
			importInvoiceLine.JI_CustomsQuantity = 2000m;
			importInvoiceLine.US_PayableMPF = 100m;
			importInvoiceLine.JI_PartNo = "Part1";
			importInvoiceLine.US_CustomsValue = 1000m;

			var importEntryHeader = importDec.CustomsEntryHeaders.AddNew();
			importEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			importEntryHeader.EntryNumber = "12345678";

			var importEntryLine = importEntryHeader.MergedLines.AddNew();
			importEntryLine.CL_LineNumber = 2;
			importEntryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			importEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 300m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 500m);
			importEntryLine.CL_CustomsValue = 1000m;

			importInvoiceLine.JI_CL = importEntryLine.PK;

			if (addSecondLine)
			{
				importInvoiceLine.US_PayableMPF = 33.35m;
				importEntryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.USCustoms.FeeCodes.HMF).CF_ChargeAmount = 133m;

				importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
				importInvoiceLine.JI_InvoiceUQ = "XX";
				importInvoiceLine.JI_InvoiceQuantity = 1000m;
				importInvoiceLine.JI_CustomsUnitQty = "NO";
				importInvoiceLine.JI_CustomsQuantity = 2000m;
				importInvoiceLine.US_PayableMPF = 66.65;
				importInvoiceLine.JI_PartNo = "Part1";

				importEntryLine = importEntryHeader.MergedLines.AddNew();
				importEntryLine.CL_LineNumber = 3;
				importEntryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
				importEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 267m);
				importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 500m);
				importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 300m);
				importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 500m);
				importEntryLine.CL_CustomsValue = 2000m;

				importInvoiceLine.JI_CL = importEntryLine.PK;
			}

			importDec.CalculateTotalEnteredValue();
			Factory.Save();
			return importEntryHeader;
		}

		public void TestCaculatedDrawbackClaims()
		{
			var importEntryHeader = SetupImportEntry();
			var importEntryLine = importEntryHeader.MergedLines[0];
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWExportUQ = "NO";
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";

			// Assert matching unit
			AssertEquals("Import Quantity", 1000m, invoiceLine.DRWImportQuantity);
			AssertEquals("Import UQ", "XX", invoiceLine.DRWImportUQ);
			AssertEquals("Export Quantity", 0m, invoiceLine.DRWExportQuantity);
			AssertEquals("Export UQ", "XX", invoiceLine.DRWExportUQ);

			invoiceLine.US_ImportEntryNo = "";
			invoiceLine.US_DRWExportUQ = "XX";
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			AssertEquals("Import Quantity", 1000m, invoiceLine.DRWImportQuantity);
			AssertEquals("Import UQ", "XX", invoiceLine.DRWImportUQ);
			AssertEquals("Export Quantity", 0m, invoiceLine.DRWExportQuantity);
			AssertEquals("Export UQ", "XX", invoiceLine.DRWExportUQ);

			invoiceLine.US_DRWExportQuantity = 500m;
			// Claimed Duty
			AssertEquals("Declared Duty", 1000m, invoiceLine.DeclaredVFD);
			AssertEquals("Duty per Unit", 1m, invoiceLine.DutyPerUnit);
			AssertEquals("Export Value", 500m, invoiceLine.ExportValue);
			AssertEquals("Duty Rate", 0.2m, invoiceLine.DutyRate);
			AssertEquals("Claimed Duty", 100m, invoiceLine.ClaimedDuty);
			AssertEquals("99% Claimed Duty", 99m, invoiceLine._99ClaimedDuty);

			// Claimed Tax
			AssertEquals("Declared Tax", 300m, invoiceLine.DeclaredTax);
			AssertEquals("Tax Per Unit", 0.3m, invoiceLine.TaxPerUnit);
			AssertEquals("Claimed Tax", 150m, invoiceLine.ClaimedTax);
			AssertEquals("99% Claimed Tax", 148.5m, invoiceLine._99ClaimedTax);

			// Claimed MPF
			AssertEquals("Declared MPF", 100m, invoiceLine.DeclaredMPF);
			AssertEquals("MPF Per Unit", 0.099m, invoiceLine.MPFPerUnit);
			AssertEquals("Claimed MPF", 49.5m, invoiceLine.ClaimedMPF);
			AssertEquals("MPF Weighted Ratio", 1m, invoiceLine.MPFWeightedRatio);
			AssertEquals("Line MPF should be Total MPF * Weighted Ratio", 100m, invoiceLine.LineMPF);

			// Claimed HMF
			AssertEquals("Declared HMF", 400m, invoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0.396m, invoiceLine.HMFPerUnit);
			AssertEquals("Claimed HMF", 198m, invoiceLine.ClaimedHMF);
			AssertEquals("Weighted Ratio", 1m, invoiceLine.WeightedRatio);
			AssertEquals("Line HMF - Total HMF * Weighted Ratio", 400m, invoiceLine.LineHMF);

			// Claimed Other Fees
			AssertEquals("Declared Other Fees", 500m, invoiceLine.DeclaredOtherFees);
			AssertEquals("Other Fees Per Unit", 0.5m, invoiceLine.OtherFeesPerUnit);
			AssertEquals("Claimed Other Fees", 250m, invoiceLine.ClaimedOtherFees);
			AssertEquals("99% Claimed Other Fees", 247.5m, invoiceLine._99ClaimedOtherFees);

			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			AssertHMFMPFNotClaimable(invoiceLine);

			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.SubstitutionManufacturerDrawback;
			AssertHMFMPFNotClaimable(invoiceLine);
			drawback.US_DRWSection = "1313 (D)";
			AssertEquals("100% Claimed Tax", 150m, invoiceLine._99ClaimedTax);
			drawback.US_DRWSection = "1313(d)";
			AssertEquals("100% Claimed Tax", 150m, invoiceLine._99ClaimedTax);

			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._07;
			AssertEquals("100% Claimed Tax", 150m, invoiceLine._99ClaimedTax);

			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			AssertEquals("99% Claimed Tax", 148.5m, invoiceLine._99ClaimedTax);

			invoiceLine.US_ImportEntryNo = "12345678";
			ICalculationExhibitsSupporter supporter = invoiceLine.Claims.HMFClaim;
			AssertEquals("Description", ZString.Empty, supporter.Description);

			invoiceLine.JI_PartNo = "SALAD";
			AssertEquals("Description", "Part No.: SALAD", supporter.Description);

			invoiceLine.JI_Description = "TEST";
			AssertEquals("Description", "TEST, Part No.: SALAD", supporter.Description);
		}

		void AssertHMFMPFNotClaimable(JobComInvoiceLine invoiceLine)
		{
			AssertEquals("Declared HMF", 0m, invoiceLine.DeclaredHMF);
			AssertEquals("Declared MPF", 0m, invoiceLine.DeclaredMPF);
		}

		public void TestOverridenDrawbackClaims()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;

			invoiceLine.DRWImportQuantity = 1000m;
			invoiceLine.DRWImportUQ = "XX";
			invoiceLine.DRWExportQuantity = 500m;
			invoiceLine.DRWExportUQ = "PK";

			invoiceLine.DeclaredVFD = 200m;
			invoiceLine.DeclaredTax = 300m;
			invoiceLine.DeclaredMPF = 100m;
			invoiceLine.DeclaredHMF = 400m;
			invoiceLine.WeightedRatio = 0.233145;
			invoiceLine.DeclaredOtherFees = 500m;

			invoiceLine.LineDuty = invoiceLine.Claims.DutyClaim.DeclaredAmount * 0.2m;

			AssertEquals("Import Quantity", 1000m, invoiceLine.DRWImportQuantity);
			AssertEquals("Import UQ", "PK", invoiceLine.DRWImportUQ);
			AssertEquals("Export Quantity", 500m, invoiceLine.DRWExportQuantity);
			AssertEquals("Export UQ", "PK", invoiceLine.DRWExportUQ);

			// Claimed Duty
			AssertEquals("Declared Duty", 200m, invoiceLine.DeclaredVFD);
			AssertEquals("Duty per Unit", 0.2m, invoiceLine.DutyPerUnit);
			AssertEquals("Export Value", 100m, invoiceLine.ExportValue);
			AssertEquals("Duty Rate", 0.2m, invoiceLine.DutyRate);
			AssertEquals("Claimed Duty", 20m, invoiceLine.ClaimedDuty);
			AssertEquals("99% Claimed Duty", 19.8m, invoiceLine._99ClaimedDuty);

			// Claimed Tax
			AssertEquals("Declared Tax", 300m, invoiceLine.DeclaredTax);
			AssertEquals("Tax Per Unit", 0.3m, invoiceLine.TaxPerUnit);
			AssertEquals("Claimed Tax", 150m, invoiceLine.ClaimedTax);
			AssertEquals("99% Claimed Tax", 148.5m, invoiceLine._99ClaimedTax);

			// Claimed MPF
			AssertEquals("Declared MPF", 100m, invoiceLine.DeclaredMPF);
			AssertEquals("MPF Per Unit", 0.0230769m, invoiceLine.MPFPerUnit);
			AssertEquals("Claimed MPF", 11.53845m, invoiceLine.ClaimedMPF);
			AssertEquals("Line MPF", 23.31m, invoiceLine.LineMPF);

			// Claimed HMF
			AssertEquals("Declared HMF", 400m, invoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0.0923274m, invoiceLine.HMFPerUnit);
			AssertEquals("Claimed HMF", 46.1637m, invoiceLine.ClaimedHMF);
			AssertEquals("Line HMF", 93.26m, invoiceLine.LineHMF);

			// Claimed Other Fees
			AssertEquals("Declared Other Fees", 500m, invoiceLine.DeclaredOtherFees);
			AssertEquals("Other Fees Per Unit", 0.5m, invoiceLine.OtherFeesPerUnit);
			AssertEquals("Claimed Other Fees", 250m, invoiceLine.ClaimedOtherFees);
			AssertEquals("99% Claimed Other Fees", 247.5m, invoiceLine._99ClaimedOtherFees);
		}

		[TestDate(2018, 2, 20)]
		public void TestOverridenCaculatedDrawbackClaims()
		{
			var importEntryHeader = SetupImportEntry(addSecondLine: true);
			var importEntryLine = importEntryHeader.MergedLines[0];
			var randomLine = importEntryLine.RandomLine;
			randomLine.JI_LinePrice = 1000m;
			importEntryLine.US_HasMPF = true;
			var fee = randomLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			fee.CY_FeeAmount = 50m;
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 50m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWExportQuantity = 1000m;
			invoiceLine.US_DRWExportUQ = "NO";
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			AssertEquals("Export UQ", "XX", invoiceLine.DRWExportUQ);

			// Claimed MPF
			AssertEquals("Declared MPF", 100m, invoiceLine.DeclaredMPF);
			AssertEquals(@"MPR Weighted Ratio is 1, because now it is calculated like Line Customs Value/Total Value of Lines with MPF. 
Line customs value is 1000m, only one entry line has MPF and line value for this line with MPF is 1000m", 1m, invoiceLine.MPFWeightedRatio.Round(6));

			AssertEquals("MPF Per Unit", 0.099m, invoiceLine.MPFPerUnit.Round(8));
			AssertEquals("Claimed MPF", 99.000m, invoiceLine.ClaimedMPF);
			AssertEquals("Line MPF should be Total MPF * MPF Weighted Ratio", 100m, invoiceLine.LineMPF);

			// Claimed HMF
			AssertEquals("Declared HMF", 400m, invoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0.1319967m, invoiceLine.HMFPerUnit.Round(8));
			AssertEquals("Claimed HMF", 131.9967000m, invoiceLine.ClaimedHMF);
			AssertEquals("Line HMF - Total HMF * Weighted Ratio", 133.33m, invoiceLine.LineHMF);

			// now tick override, but calcs should not change
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			AssertEquals("Declared MPF", 100m, invoiceLine.DeclaredMPF);
			AssertEquals("MPF Weighted Ratio", 1m, invoiceLine.MPFWeightedRatio);
			AssertEquals("MPF Per Unit", 0.099m, invoiceLine.MPFPerUnit.Round(8));
			AssertEquals("Claimed MPF", 99.000m, invoiceLine.ClaimedMPF);
			AssertEquals("Weighted Ratio", 0.333333m, invoiceLine.WeightedRatio.Round(6));
			AssertEquals("Line MPF should be Total MPF * MPF Weighted Ratio", 100m, invoiceLine.LineMPF);
			AssertEquals("Declared HMF", 400m, invoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0.1319967m, invoiceLine.HMFPerUnit.Round(8));
			AssertEquals("Claimed HMF", 131.9967000m, invoiceLine.ClaimedHMF);
			AssertEquals("Line HMF - Total HMF * Weighted Ratio", 133.33m, invoiceLine.LineHMF);

			// now simulate existing entry before upgrade where ratio will be zero
			invoiceLine.WeightedRatio = 0m;
			invoiceLine.MPFWeightedRatio = 0m;
			CombineAssertions(() =>
			{
				AssertEquals("Declared MPF", 33.35m, invoiceLine.DeclaredMPF);
				AssertEquals("MPF Per Unit", 0.0330165m, invoiceLine.MPFPerUnit.Round(8));
				AssertEquals("Claimed MPF", 33.0165000m, invoiceLine.ClaimedMPF);
				AssertEquals("MPF Weighted Ratio", 0m, invoiceLine.MPFWeightedRatio.Round(6));
				AssertEquals("Weighted Ratio", 0m, invoiceLine.WeightedRatio.Round(6));
				AssertEquals("Line MPF should be same as declared", 33.35m, invoiceLine.LineMPF);
				AssertEquals("Declared HMF", 133m, invoiceLine.DeclaredHMF);
				AssertEquals("HMF Per Unit", 0.13167m, invoiceLine.HMFPerUnit.Round(8));
				AssertEquals("Claimed HMF", 131.67000m, invoiceLine.ClaimedHMF);
				AssertEquals("Line HMF should be same as declared", 133m, invoiceLine.LineHMF);
			});

			invoiceLine.US_DRWIsForManufacturerSection = true;
			invoiceLine.US_DRWQuantityUsed = 300m;
			invoiceLine.US_ImportEntryNo = "";
			invoiceLine.US_DRWUQUsed = "NO";
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			invoiceLine.DRWImportQuantity = 1000m;
			invoiceLine.WeightedRatio = 1m;
			CombineAssertions(() =>
			{
				AssertEquals("Declared MPF", 100m, invoiceLine.DeclaredMPF);
				AssertEquals("MPF Per Unit", 0.099m, invoiceLine.MPFPerUnit.Round(8));
				AssertEquals("Claimed MPF", 29.700m, invoiceLine.ClaimedMPF);
				AssertEquals("Line MPF", 100m, invoiceLine.LineMPF);
				AssertEquals("Declared HMF", 400m, invoiceLine.DeclaredHMF);
				AssertEquals("HMF Per Unit", 0.396m, invoiceLine.HMFPerUnit.Round(8));
				AssertEquals("Claimed HMF", 118.800m, invoiceLine.ClaimedHMF);
			});
		}

		public void TestOverridenMPFCalculation()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;

			invoiceLine.DRWImportQuantity = 1000m;
			invoiceLine.DRWImportUQ = "XX";
			invoiceLine.DRWExportQuantity = 500m;

			invoiceLine.DeclaredMPF = 25m;
			invoiceLine.WeightedRatio = 0.233145m;

			AssertEquals("Declared MPF", 25m, invoiceLine.DeclaredMPF);
			AssertEquals("Line MPF", 5.83m, invoiceLine.LineMPF);
			AssertEquals("MPF Per Unit", 0.0057717m, invoiceLine.MPFPerUnit);
			AssertEquals("Claimed MPF", 2.88585m, invoiceLine.ClaimedMPF);

			invoiceLine.MPFWeightedRatio = 0.111112m;
			AssertEquals("Declared MPF", 25m, invoiceLine.DeclaredMPF);
			AssertEquals("Line MPF", 2.78m, invoiceLine.LineMPF);
			AssertEquals("MPF Per Unit", 0.0027522m, invoiceLine.MPFPerUnit);
			AssertEquals("Claimed MPF", 1.3761m, invoiceLine.ClaimedMPF);
		}

		[TestDate(2018, 2, 20)]
		public void TestCaculatedDrawbackClaimsForManufacturing()
		{
			var importEntryHeader = SetupImportEntry();
			var importEntryLine = importEntryHeader.MergedLines[0];
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;

			var drawbackInvoice = drawback.Invoices.AddNew();
			var drawbackInvoiceLine = drawbackInvoice.InvoiceLines.AddNew();
			drawbackInvoiceLine.US_DRWIsForImportSection = true;

			drawbackInvoiceLine.WeightedRatio = 1m;
			drawbackInvoiceLine.US_DRWIsForManufacturerSection = true;
			drawbackInvoiceLine.US_DRWQuantityUsed = 250m;
			drawbackInvoiceLine.US_DRWUQUsed = "NO";
			drawbackInvoiceLine.US_ImportEntryNo = "XJ512345678-2";

			// Assert manufacturing unit matched original customs units

			AssertEquals("Import Quantity", 2000m, drawbackInvoiceLine.DRWImportQuantity);
			AssertEquals("Import UQ", "XX", drawbackInvoiceLine.DRWImportUQ);
			AssertEquals("Export Quantity", 0m, drawbackInvoiceLine.DRWExportQuantity);
			AssertEquals("Export UQ", "XX", drawbackInvoiceLine.DRWExportUQ);
			AssertEquals("Export Quantity", 250m, drawbackInvoiceLine.US_DRWQuantityUsed);
			AssertEquals("Export UQ", "NO", drawbackInvoiceLine.US_DRWUQUsed);

			AssertEquals("Declared MPF", 100m, drawbackInvoiceLine.DeclaredMPF);
			AssertEquals("MPF Per Unit", 0.0495m, drawbackInvoiceLine.MPFPerUnit.Round(8));
			AssertEquals("Claimed MPF", 12.375m, drawbackInvoiceLine.ClaimedMPF);
			AssertEquals("Line MPF", 100m, drawbackInvoiceLine.LineMPF);
			AssertEquals("Declared HMF", 400m, drawbackInvoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0.198m, drawbackInvoiceLine.HMFPerUnit.Round(8));
			AssertEquals("Claimed HMF", 49.5m, drawbackInvoiceLine.ClaimedHMF);

			drawbackInvoiceLine.US_ImportEntryNo = "";
			drawbackInvoiceLine.DRWExportUQ = "YY"; // nothing should change in calculation, because calculation based on used qty and uq
			drawbackInvoiceLine.US_ImportEntryNo = "XJ512345678-2";
			drawbackInvoiceLine.US_DRWExportQuantity = 1000m;
			AssertEquals("Import Quantity", 2000m, drawbackInvoiceLine.DRWImportQuantity);
			AssertEquals("Import UQ", "XX", drawbackInvoiceLine.DRWImportUQ);
			AssertEquals("Export Quantity", 1000m, drawbackInvoiceLine.DRWExportQuantity);
			AssertEquals("Export UQ", "XX", drawbackInvoiceLine.DRWExportUQ);
			AssertEquals("Export Quantity", 250m, drawbackInvoiceLine.US_DRWQuantityUsed);
			AssertEquals("Export UQ", "NO", drawbackInvoiceLine.US_DRWUQUsed);

			AssertEquals("Declared MPF", 100m, drawbackInvoiceLine.DeclaredMPF);
			AssertEquals("MPF Per Unit", 0.0495m, drawbackInvoiceLine.MPFPerUnit.Round(8));
			AssertEquals("Claimed MPF", 12.375m, drawbackInvoiceLine.ClaimedMPF);
			AssertEquals("Line MPF", 100m, drawbackInvoiceLine.LineMPF);
			AssertEquals("Declared HMF", 400m, drawbackInvoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0.198m, drawbackInvoiceLine.HMFPerUnit.Round(8));
			AssertEquals("Claimed HMF", 49.5m, drawbackInvoiceLine.ClaimedHMF);

			// Assert manufacturing unit matched original invoice units

			drawbackInvoiceLine.US_ImportEntryNo = "";
			drawbackInvoiceLine.US_DRWQuantityUsed = 100m;
			drawbackInvoiceLine.US_DRWUQUsed = "XX";
			drawbackInvoiceLine.US_ImportEntryNo = "XJ512345678-2";

			AssertEquals("Declared MPF", 100m, drawbackInvoiceLine.DeclaredMPF);
			AssertEquals("MPF Per Unit", 0.099m, drawbackInvoiceLine.MPFPerUnit.Round(8));
			AssertEquals("Claimed MPF", 9.9m, drawbackInvoiceLine.ClaimedMPF);
			AssertEquals("Line MPF", 100m, drawbackInvoiceLine.LineMPF);
			AssertEquals("Declared HMF", 400m, drawbackInvoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0.396m, drawbackInvoiceLine.HMFPerUnit.Round(8));
			AssertEquals("Claimed HMF", 39.6m, drawbackInvoiceLine.ClaimedHMF);

			// Assert manufacturing units do not match original customs units and original invoice units - do not calculate

			drawbackInvoiceLine.US_ImportEntryNo = "";
			drawbackInvoiceLine.US_DRWQuantityUsed = 120m;
			drawbackInvoiceLine.US_DRWUQUsed = "PKG";
			drawbackInvoiceLine.US_ImportEntryNo = "XJ512345678-2";

			AssertEquals("Declared MPF", 100m, drawbackInvoiceLine.DeclaredMPF);
			AssertEquals("MPF Per Unit", 0m, drawbackInvoiceLine.MPFPerUnit.Round(8));
			AssertEquals("Claimed MPF", 0m, drawbackInvoiceLine.ClaimedMPF);
			AssertEquals("Line MPF", 100m, drawbackInvoiceLine.LineMPF);
			AssertEquals("Declared HMF", 400m, drawbackInvoiceLine.DeclaredHMF);
			AssertEquals("HMF Per Unit", 0m, drawbackInvoiceLine.HMFPerUnit.Round(8));
			AssertEquals("Claimed HMF", 0m, drawbackInvoiceLine.ClaimedHMF);
		}

		public void TestOverrideACEClaims()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._51;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;

			invoiceLine.DRWImportQuantity = 1000m;
			invoiceLine.DRWImportUQ = "XX";
			invoiceLine.DRWExportQuantity = 500m;

			invoiceLine.DRWAllowableQTY = 1000m;
			invoiceLine.DRWGoodsValuePerUQ = 100m;

			var oilTaxFee = invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.OilSpillTax, 888);
			var domesticTaxFee = invoiceLine.DrawbackOtherFees.AddNewOrUpdate(DrawbackOtherFeeTypesList.Codes.DomesticTax, 777m);

			AssertEquals("Allowable Quantity", 1000m, invoiceLine.DRWAllowableQTY);
			AssertEquals("Goods value per unit", 100m, invoiceLine.DRWGoodsValuePerUQ);
			AssertEquals("Declared Oil Tax Amount", 888m, oilTaxFee.DeclaredAmount);
			AssertEquals("Declared Domestic Tax Amount", 777m, domesticTaxFee.DeclaredAmount);
		}

		public void TestUseOriginalDataWhenOverrideAndDRWOldDataIsTrue()
		{
			using (USCustomsDataRegistry.Instance.UseViewForDrawbackEntryLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
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
				importDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				importInvoiceLine.US_PayableMPF = 100m;
				var importEntryHeader = importDec.ActiveEntryHeaders.EntrySummaryEntry;
				importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
				importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
				var importEntryLine = importEntryHeader.MergedLines[0];
				importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
				importEntryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 200m);
				importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
				importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Wines, 300m);
				importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 500m);
				Factory.Save();

				var drawback = Factory.New<JobDeclaration>();
				drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
				drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._08;
				var invoice = drawback.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.US_DRWIsForImportSection = true;
				RestData(invoiceLine, "XJ512345679-5", 500m);
				var claims = invoiceLine.Claims;
				CombineAssertions("Default unmatched entry", () =>
				{
					AssertEquals("claims.DutyClaim.LineDutyRateDesc", "Free", claims.DutyClaim.LineDutyRateDesc);
					AssertEquals("invoiceLine.US_DRWValuePerUQ", ZDecimal.Zero, invoiceLine.US_DRWValuePerUQ);
					AssertEquals("invoiceLine.US_DRWValuePerUQ2", ZDecimal.Zero, invoiceLine.US_DRWValuePerUQ2);
					AssertEquals("invoiceLine.US_DRWValuePerUQ3", ZDecimal.Zero, invoiceLine.US_DRWValuePerUQ3);
					AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);
					AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);
					AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);
					AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);
					AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);
				});

				RestData(invoiceLine, "XJ512345678-1", 500m);
				claims = invoiceLine.Claims;
				CombineAssertions("Default", () =>
				{
					AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
					AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
					AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
					AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
					AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 801.90m, 1m);
					AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, 400m, 198m, 1m);
					AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 148.50m, 1m);
					AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, 100m, 49.5m, 1m);
					AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, 247.50m, 1m);
				});
				invoiceLine.US_DRWWeightedRatio = 0m;
				invoiceLine.US_DRWMPFWeightedRatio = 0m;
				CombineAssertions("Zero Weight Ratio", () =>
				{
					AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
					AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
					AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
					AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
					AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 801.90m, 0m);
					AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, 400m, 198m, 0m);
					AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 148.50m, 0m);
					AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, 100m, 49.5m, 0m);
					AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, 247.50m, 0m);
				});
				invoiceLine.US_DRWWeightedRatio = 0.5m;
				invoiceLine.US_DRWMPFWeightedRatio = 0.6m;
				CombineAssertions("Weight Ratio", () =>
				{
					AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
					AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
					AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
					AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
					AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 10000m, 801.90m, 0.5m);
					AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, 400m, 99m, 0.5m);
					AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 300m, 148.50m, 0.5m);
					AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, 100m, 29.7m, 0.6m);
					AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 500m, 247.50m, 0.5m);
				});
				invoiceLine.US_DRWDeclaredVFD = 1m;
				invoiceLine.US_DRWDeclaredTax = 1m;
				invoiceLine.US_DRWDeclaredHMF = 1m;
				invoiceLine.US_DRWDeclaredMPF = 1m;
				invoiceLine.US_DRWDeclaredOtherFees = 1m;
				CombineAssertions("Declared", () =>
				{
					AssertEquals("claims.DutyClaim.LineDutyRateDesc", "16.2000%", claims.DutyClaim.LineDutyRateDesc);
					AssertEquals("invoiceLine.US_DRWValuePerUQ", 10m, invoiceLine.US_DRWValuePerUQ);
					AssertEquals("invoiceLine.US_DRWValuePerUQ2", 50m, invoiceLine.US_DRWValuePerUQ2);
					AssertEquals("invoiceLine.US_DRWValuePerUQ3", 100m, invoiceLine.US_DRWValuePerUQ3);
					AssertClaim(nameof(claims.DutyClaim), claims.DutyClaim, 1m, 0.08m, 0.5m);
					AssertClaim(nameof(claims.HMFClaim), claims.HMFClaim, 1m, 0.25, 0.5m);
					AssertClaim(nameof(claims.IRTaxClaim), claims.IRTaxClaim, 1m, 0.49m, 0.5m);
					AssertClaim(nameof(claims.MPFClaim), claims.MPFClaim, 1m, 0.3m, 0.6m);
					AssertClaim(nameof(claims.OtherFeesClaim), claims.OtherFeesClaim, 1m, 0.49m, 0.5m);
				});
			}
		}

		void RestData(JobComInvoiceLine invoiceLine, ZString importEntryNo, ZDecimal exportQuantity)
		{
			invoiceLine.US_ImportEntryNo = importEntryNo;
			invoiceLine.US_DRWExportQuantity = exportQuantity;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.US_DRWOldData = true;
			invoiceLine.US_DRWDutyRate_New = -1m;
			invoiceLine.US_DRWWeightedRatio = -1m;
			invoiceLine.US_DRWLineDuty = -1m;
			invoiceLine.US_DRWDeclaredVFD = -1m;
			invoiceLine.US_DRWDeclaredTax = -1m;
			invoiceLine.US_DRWDeclaredHMF = -1m;
			invoiceLine.US_DRWDeclaredMPF = -1m;
			invoiceLine.US_DRWDeclaredOtherFees = -1m;
			invoiceLine.US_DRWCalcDuty = ZDecimal.Zero;
			invoiceLine.US_DRWCalcHMF = ZDecimal.Zero;
			invoiceLine.US_DRWCalcMPF = ZDecimal.Zero;
			invoiceLine.US_DRWCalcTax = ZDecimal.Zero;
			invoiceLine.USI_DRW99ClaimedDuty = ZDecimal.Zero;
			invoiceLine.USI_DRW99ClaimedTax = ZDecimal.Zero;
			invoiceLine.USI_DRW99ClaimedHMF = ZDecimal.Zero;
			invoiceLine.USI_DRW99ClaimedMPF = ZDecimal.Zero;
		}

		public void TestACEDrawbackOtherFeeClaims()
		{
			SetupImportEntry();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._51;

			var drawbackInvoice = drawback.Invoices.AddNew();
			var drawbackInvoiceLine = drawbackInvoice.InvoiceLines.AddNew();
			drawbackInvoiceLine.US_DRWIsForImportSection = true;
			drawbackInvoiceLine.US_DRWClaimAmountOverriden_New = false;
			drawbackInvoiceLine.US_ImportEntryNo = "XJ512345678-2";
			AssertEquals(1, drawbackInvoiceLine.DrawbackOtherFees.Count);

			var cottonFee = drawbackInvoiceLine.DrawbackOtherFees[DrawbackOtherFeeTypesList.Codes.CottonFee];
			AssertEquals(500m, cottonFee.DeclaredAmount);
			AssertEquals(0m, cottonFee.ClaimedAmount);
			AssertEquals(0m, cottonFee._99ClaimedAmount);
			AssertEquals(0m, cottonFee.CalculatedAmount);

			drawbackInvoiceLine.DRWExportQuantity = 500m;
			drawbackInvoiceLine.DRWExportUQ = "XX";
			AssertEquals(500m, cottonFee.DeclaredAmount);
			AssertEquals(250m, cottonFee.ClaimedAmount);
			AssertEquals(247.5m, cottonFee._99ClaimedAmount);
			AssertEquals(247.5m, cottonFee.CalculatedAmount);

			drawbackInvoiceLine.US_DRWClaimAmountOverriden_New = true;
			cottonFee.DeclaredAmount = 1000m;
			AssertEquals(1000m, cottonFee.DeclaredAmount);
			AssertEquals(500m, cottonFee.ClaimedAmount);
			AssertEquals(495m, cottonFee._99ClaimedAmount);
			AssertEquals(495m, cottonFee.CalculatedAmount);
		}

		public void TestHMFMPFDrawbackClaimsRatiosForProvisionalTariff()
		{
			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			importDeclaration.US_EnableENS = true;
			importDeclaration.US_PayableMPF = 300m;
			importDeclaration.US_EntryFilerCode = "XJ5";

			importDeclaration.Invoices.AddNew().JZ_RX_NKInvoice_Currency = "USD";
			var importInvoiceLine = importDeclaration.InvoiceLines.AddNew();
			importInvoiceLine.JI_LinePrice = 10000m;
			importInvoiceLine.US_SupTariff = "100231450";
			importInvoiceLine.JI_CustomsQuantity = 200m;
			importInvoiceLine.US_PayableMPF = 300m;

			var importEntryHeader = importDeclaration.CustomsEntryHeaders.AddNew();
			importEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			importEntryHeader.EntryNumber = "12345678";

			var importEntryLine = importEntryHeader.MergedLines.AddNew();
			importEntryLine.CL_LineNumber = 2;
			importEntryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			importInvoiceLine.JI_CL = importEntryLine.PK;

			var childEntryLine = importEntryHeader.MergedLines.AddNew();
			childEntryLine.CL_LineNumber = 2;
			childEntryLine.US_CL_ParentLine = importEntryLine.PK;

			childEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			childEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 300m);
			childEntryLine.CL_CustomsValue = 1000m;
			childEntryLine.US_HasMPF = true;
			var amount = childEntryLine.PayableMPFAmount;
			importDeclaration.CalculateTotalEnteredValue();
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._08;

			var drawbackInvoice = drawback.Invoices.AddNew();
			var drawbackInvoiceLine = drawbackInvoice.InvoiceLines.AddNew();
			drawbackInvoiceLine.US_DRWIsForImportSection = true;
			drawbackInvoiceLine.US_ImportEntryNo = importDeclaration.EntryFilerCode + importDeclaration.ImportEntryNumber;
			drawbackInvoiceLine.US_DRWImportEntryLine = 2;

			AssertEquals("The MPF Ratio should include in its calculation the CustomsValue of ChildLines", 1m, drawbackInvoiceLine.MPFWeightedRatio.Round(6));
			AssertEquals("The HMF Ratio should include in its calculation the CustomsValue of ChildLines", 1m, drawbackInvoiceLine.WeightedRatio.Round(6));
		}

		public void TestGoodsValuePerUnits()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._08;

			var invoiceLine = drawback.InvoiceLines.AddNew();
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.DRWImportUQ = "NO";
			invoiceLine.DRWImportUQ2 = "NO";
			invoiceLine.DRWImportUQ3 = "NO";
			invoiceLine.DRWImportQuantity = 588m;
			invoiceLine.DRWImportQuantity2 = 588m;
			invoiceLine.DRWImportQuantity3 = 588m;
			invoiceLine.DeclaredVFD = 1294m;

			AssertEquals(2.2006m, invoiceLine.DRWGoodsValuePerUQ);
			AssertEquals(2.2006m, invoiceLine.DRWGoodsValuePerUQ2);
			AssertEquals(2.2006m, invoiceLine.DRWGoodsValuePerUQ3);
		}

		public void TestDeclaredAmountForOtherFee()
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
			CombineAssertions("Acc Payment Ticked", () =>
			{
				AssertEquals("invoiceLine.Claims.OtherFeesClaim.DeclaredAmount", ZDecimal.Zero, invoiceLine.Claims.OtherFeesClaim.DeclaredAmount);
				AssertEquals("invoiceLine.DrawbackOtherFees.Count", 1, invoiceLine.DrawbackOtherFees.Count);
				var cottonFee = invoiceLine.DrawbackOtherFees[0];
				AssertEquals("cottonFee.US_FeeType", "056", cottonFee.US_FeeType);
				AssertEquals("cottonFee.DeclaredAmount", ZDecimal.Zero, cottonFee.DeclaredAmount);
			});

			declaration.US_AcceleratedClaimInd = false;
			CombineAssertions("Acc Payment Unticked", () =>
			{
				AssertEquals("invoiceLine.Claims.OtherFeesClaim.DeclaredAmount", 500m, invoiceLine.Claims.OtherFeesClaim.DeclaredAmount);
				AssertEquals("invoiceLine.DrawbackOtherFees.Count", 1, invoiceLine.DrawbackOtherFees.Count);
				var cottonFee = invoiceLine.DrawbackOtherFees[0];
				AssertEquals("cottonFee.US_FeeType", "056", cottonFee.US_FeeType);
				AssertEquals("cottonFee.DeclaredAmount", 500m, cottonFee.DeclaredAmount);
			});
		}

		public void TestCalculatedAmountRoundingIssue_CS00950300()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._58;
			declaration.US_AcceleratedClaimInd = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Exported;
			invoiceLine.US_ImportEntryNo = "XJ512345678-1";
			invoiceLine.US_DRWImportEntryLine = 1;
			invoiceLine.JI_Tariff = "9608100000";

			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.US_DRWExportAction = ACEDrawbackActionCodeList.Codes.Exported;
			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.US_ExportTariff = "9608100000";

			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.DRWImportQuantity = 35387m;
			invoiceLine.DRWImportUQ = ABIUnitOfMeasureList.Codes.Pieces;
			invoiceLine.DRWExportQuantity = 35387m;
			invoiceLine.DRWExportUQ = ABIUnitOfMeasureList.Codes.Pieces;
			invoiceLine.DeclaredVFD = 7063m;
			invoiceLine.LineDuty = 664.61m;

			AssertEquals("invoiceLine.ExportValue", 7062.89133m, invoiceLine.ExportValue);
			AssertEquals("invoiceLine.ClaimedDuty", 664.59977m, invoiceLine.ClaimedDuty);
			AssertEquals("invoiceLine.CalculatedDuty", 657.95m, invoiceLine.CalculatedDuty);
		}

		public void TestCalculatedAmountForAllFeesBasedOnSubstitutedValuePerUnitIfPresent()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var importDec = Factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var importInvoice = importDec.Invoices.AddNew();
			importInvoice.JZ_InvoiceNumber = "INV1234";

			var importInvoiceLine = importInvoice.JobComInvoiceLines.AddNew();
			importInvoiceLine.JI_CustomsUnitQty = "NO";
			importInvoiceLine.JI_CustomsQuantity = 20000m;
			importInvoiceLine.US_CustomsValue = 40000m;
			importInvoiceLine.JI_Description = "TEST DRAWBACK DESCRIPTION";
			importInvoiceLine.US_PayableMPF = 123.25;

			var importEntryHeader = importDec.CustomsEntryHeaders.AddNew();
			importEntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			importEntryHeader.EntryNumber = "12345678";

			var importEntryLine = importEntryHeader.MergedLines.AddNew();
			importEntryLine.CL_LineNumber = 1;
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 1000m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Tobacco, 100m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.HMF, 44.48m);
			importEntryLine.CL_CustomsValue = 40000m;
			importInvoiceLine.JI_CL = importEntryLine.PK;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._73;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_ImportEntryNo = "XJ512345678-1";
			invoiceLine.US_DRWExportQuantity = 20000m;
			invoiceLine.US_DRWExportUQ = "NO";
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.US_DRWCalcDutyWithAdValoremRate = true;
			invoiceLine.US_DRWAdValoremRate = 6.5m;

			var claims = invoiceLine.Claims;

			AssertEquals("Calculated should equal DeclaredVFD * 6.5% * (Substituted Value Per Unit / Goods Value Per Unit)", 2574m, invoiceLine.CalculatedDuty, 0.01m);

			AssertEquals("CalculatedTax should be (DeclaredTax * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 99m, invoiceLine.CalculatedTax, 0.01m);
			AssertEquals("TaxPerUnit should equal OriginalTaxPerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.005m, invoiceLine.TaxPerUnit, 0.0001m);

			AssertEquals("CalculatedMPF should be (DeclaredMPF * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 122.02m, invoiceLine.CalculatedMPF, 0.01m);
			AssertEquals("MPFPerUnit should equal OriginalMPFPerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.0061m, invoiceLine.MPFPerUnit, 0.0001m);

			AssertEquals("CalculatedHMF should be (DeclaredHMF * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 44.04m, invoiceLine.CalculatedHMF, 0.01m);
			AssertEquals("HMFPerUnit should equal OriginalHMFPerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.0022m, invoiceLine.HMFPerUnit, 0.0001m);

			AssertEquals("OtherFee CalculatedAmount should be (DeclaredAmount * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 990m, claims.OtherFeesClaim._99ClaimedDuty, 0.01m);
			AssertEquals("OtherFeePerUnit should equal OriginalOtherFeePerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.05m, claims.OtherFeesClaim.PerUnit, 0.0001m);

			invoiceLine.US_DRWSubstituted = 1m;

			AssertEquals("Calculated should equal DeclaredVFD * 6.5% * (Substituted Value Per Unit / Goods Value Per Unit)", 1287m, invoiceLine.CalculatedDuty, 0.01m);

			AssertEquals("CalculatedTax should be (DeclaredTax * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 49.5m, invoiceLine.CalculatedTax, 0.01m);
			AssertEquals("TaxPerUnit should equal OriginalTaxPerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.0025m, invoiceLine.TaxPerUnit, 0.0001m);

			AssertEquals("CalculatedMPF should be (DeclaredMPF * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 61.01m, invoiceLine.CalculatedMPF, 0.01m);
			AssertEquals("MPFPerUnit should equal OriginalMPFPerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.00305m, invoiceLine.MPFPerUnit, 0.0001m);

			AssertEquals("CalculatedHMF should be (DeclaredHMF * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 22.02m, invoiceLine.CalculatedHMF, 0.01m);
			AssertEquals("HMFPerUnit should equal OriginalHMFPerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.0011m, invoiceLine.HMFPerUnit, 0.0001m);

			AssertEquals("OtherFee CalculatedAmount should be (DeclaredAmount * (Substituted Value Per Unit / Goods Value Per Unit)) * 99%", 495m, claims.OtherFeesClaim._99ClaimedDuty, 0.01m);
			AssertEquals("OtherFeePerUnit should equal OriginalOtherFeePerUnit * (Substituted Value Per Unit / Goods Value Per Unit)", 0.025m, claims.OtherFeesClaim.PerUnit, 0.0001m);
		}

		public void Test99ClaimedDuty()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.US_DRWSubstituted = 1m;
			invoiceLine.US_DRWQuantityUsed = 1m;
			invoiceLine.US_DRWDeclaredVFD = 10m;
			invoiceLine.US_DRWExportQuantity = 100m;
			invoiceLine.US_DRWImportQuantity = 1000m;
			var nafta1 = invoiceLine.DrawbackNAFTAs.AddNew();
			nafta1.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 100m;
			var nafta2 = invoiceLine.DrawbackNAFTAs.AddNew();
			nafta2.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 25m;
			invoiceLine.US_DRWLineDuty = 500m;
			var dutyClaim = invoiceLine.Claims.DutyClaim;
			AssertEquals(24.75m, invoiceLine._99ClaimedDuty);
			AssertEquals(49.5m, invoiceLine.CalculatedDuty);
			nafta2.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 0m;
			dutyClaim = invoiceLine.Claims.DutyClaim;
			AssertEquals(49.5m, invoiceLine._99ClaimedDuty);
			AssertEquals(49.5m, invoiceLine.CalculatedDuty);
			invoiceLine.US_DRWLineDuty = 1500m;
			dutyClaim = invoiceLine.Claims.DutyClaim;
			AssertEquals(99m, invoiceLine._99ClaimedDuty);
			AssertEquals(148.5m, invoiceLine.CalculatedDuty);

			invoiceLine.US_DRWCalcDutyWithAdValoremRate = true;
			invoiceLine.US_DRWAdValoremRate = 4200m;
			AssertEquals(41.58m, invoiceLine._99ClaimedDuty);
			nafta2.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 38m;
			AssertEquals(37.62m, invoiceLine._99ClaimedDuty);
			nafta2.US_DRWEquivalentUSDollarAmountOfNAFTACountryDuty = 40m;
			AssertEquals(39.6m, invoiceLine._99ClaimedDuty);
			invoiceLine.DrawbackNAFTAs.RemoveAndDelete(nafta2);
			AssertEquals(41.58m, invoiceLine._99ClaimedDuty);
		}

		public void TestOverriden99ClaimedDuty()
		{
			var importEntryHeader = SetupImportEntry();
			var importEntryLine = importEntryHeader.MergedLines[0];
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryLine.Fees.AddOrUpdate(Core.Constants.USCustoms.FeeCodes.Cotton, 200m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			importEntryHeader.Charges.AddNew(Core.Constants.USCustoms.FeeCodes.HMF, 400m);
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			drawback.US_EntryType = EntryTypeList.Codes.NAFTADutyDeferral;

			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWExportUQ = "NO";
			invoiceLine.US_ImportEntryNo = "XJ512345678-2";
			invoiceLine.US_DRWExportQuantity = 500m;

			var cottonFee = invoiceLine.DrawbackOtherFees.Cast<DrawbackOtherFee>().First(x => x.US_FeeType == Core.Constants.USCustoms.FeeCodes.Cotton);

			AssertEquals(99m, invoiceLine._99ClaimedDuty);
			AssertEquals(148.5m, invoiceLine._99ClaimedTax);
			AssertEquals(198m, invoiceLine._99ClaimedHMF);
			AssertEquals(49.5m, invoiceLine._99ClaimedMPF);
			AssertEquals(99m, cottonFee.US_99ClaimAmount);
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var invoiceLine1 = factory1.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(99m, invoiceLine1.USI_DRW99ClaimedDuty);
			AssertEquals(148.5m, invoiceLine1.USI_DRW99ClaimedTax);
			AssertEquals(198m, invoiceLine1.USI_DRW99ClaimedHMF);
			AssertEquals(49.5m, invoiceLine1.USI_DRW99ClaimedMPF);
			var cottonFee1 = factory1.Load<DrawbackOtherFee>(cottonFee.PK);
			AssertEquals(99m, cottonFee1.US_99ClaimAmount);

			invoiceLine._99ClaimedDuty = 100m;
			invoiceLine._99ClaimedTax = 200m;
			invoiceLine._99ClaimedHMF = 300m;
			invoiceLine._99ClaimedMPF = 400m;
			cottonFee.US_99ClaimAmount = 500m;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var invoiceLine2 = factory2.Load<JobComInvoiceLine>(invoiceLine.PK);
			AssertEquals(100m, invoiceLine2.USI_DRW99ClaimedDuty);
			AssertEquals(200m, invoiceLine2.USI_DRW99ClaimedTax);
			AssertEquals(300m, invoiceLine2.USI_DRW99ClaimedHMF);
			AssertEquals(400m, invoiceLine2.USI_DRW99ClaimedMPF);
			var cottonFee2 = factory2.Load<DrawbackOtherFee>(cottonFee.PK);
			AssertEquals(500m, cottonFee2.US_99ClaimAmount);
		}

		public void TestValidate99ClaimedAmountWhenCalculatedAmnoutIsChanged()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine._99ClaimedDuty = 100m;
			AssertHasMessageError(invoiceLine._99ClaimedDutyInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine.CalculatedDuty = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedDutyInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);

			invoiceLine._99ClaimedHMF = 100m;
			AssertHasMessageError(invoiceLine._99ClaimedHMFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine.CalculatedHMF = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedHMFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);

			invoiceLine._99ClaimedMPF = 100m;
			AssertHasMessageError(invoiceLine._99ClaimedMPFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine.CalculatedMPF = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedMPFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);

			invoiceLine._99ClaimedTax = 100m;
			AssertHasMessageError(invoiceLine._99ClaimedTaxInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine.CalculatedTax = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedTaxInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);

			var aceOtherFee = invoiceLine.DrawbackOtherFees.AddNew();
			aceOtherFee._99ClaimedAmount = 100m;
			AssertHasMessageError(aceOtherFee._99ClaimedAmountInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			aceOtherFee.CalculatedAmount = 100m;
			AssertNoMessageError(aceOtherFee.CalculatedAmountInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
		}

		[TestDate(2021, 10, 01)]
		public void Test99ClaimedAmountsWhenOverrideIsTicked()
		{
			var tariff1 = USCTariffTest.CreateTariff(Factory, "4202929100");
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff1.UE_Column1RateAdValorem = 0.176m;
			var tariff2 = USCTariffTest.CreateTariff(Factory, "99038803");
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff2.UE_Column1RateAdValorem = 0.25m;
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._58;
			var invoice = drawback.Invoices.AddNew();
			var invoiceLine = drawback.InvoiceLines.AddNew();
			invoiceLine.US_DRWIsForImportSection = true;
			invoiceLine.US_DRWImpActInd = ACEDrawbackActionCodeList.Codes.Exported;
			invoiceLine.US_ImportEntryNo = "10103606158";
			invoiceLine.JI_Tariff = "4202929100";
			invoiceLine.US_DRWAccMethod = DrawbackAccountingMethodCodeList.Codes._02;
			invoiceLine.US_DRWIsForExportSection = true;
			invoiceLine.US_DRWExportAction = ACEDrawbackActionCodeList.Codes.Exported;
			invoiceLine.US_DRWExportDate = new ZDateTime(2019, 06, 05);
			invoiceLine.US_DRWExportDest = Core.Constants.CountryCodes.Japan;
			invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;
			invoiceLine.US_ExportTariff = "4202929100";
			var additionalTariff = invoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew();
			additionalTariff.US_Tariff = "99038803";
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.WeightedRatio = 0m;
			invoiceLine.MPFWeightedRatio = 0.029083m;
			invoiceLine.DRWImportQuantity = 34505m;
			invoiceLine.DRWImportUQ = ABIUnitOfMeasureList.Codes.Pieces;
			invoiceLine.DRWExportQuantity = 34505m;
			invoiceLine.DRWExportUQ = ABIUnitOfMeasureList.Codes.Pieces;
			invoiceLine.DRWGoodsValuePerUQ = 0.0337m;
			invoiceLine.DeclaredVFD = 1165.23m;
			invoiceLine.LineDuty = 496.39m;
			invoiceLine.DeclaredMPF = 138.21;
			CombineAssertions("Calculated and 99 claimed amount before", () =>
			{
				AssertEquals("invoiceLine._99ClaimedDuty", 491.42m, invoiceLine._99ClaimedDuty);
				AssertEquals("invoiceLine.CalculatedDuty", 491.42m, invoiceLine.CalculatedDuty);
				AssertEquals("invoiceLine._99ClaimedMPF", 3.98m, invoiceLine._99ClaimedMPF);
				AssertEquals("invoiceLine.CalculatedMPF", 3.98m, invoiceLine.CalculatedMPF);
			});

			invoiceLine.AddInfoChild.Delete();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedInvoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			CombineAssertions("Calculated and 99 claimed amount after", () =>
			{
				AssertEquals("invoiceLine._99ClaimedDuty", 491.42m, loadedInvoiceLine._99ClaimedDuty);
				AssertEquals("invoiceLine.CalculatedDuty", 491.42m, loadedInvoiceLine.CalculatedDuty);
				AssertEquals("invoiceLine._99ClaimedMPF", 3.98m, loadedInvoiceLine._99ClaimedMPF);
				AssertEquals("invoiceLine.CalculatedMPF", 3.98m, loadedInvoiceLine.CalculatedMPF);
			});
		}

		public void TestLineDutyAndClaimedDutyNotZeroWhenOverride()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var tariff = USCTariffTest.CreateTariff(Factory, "4202929100");
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_Column1RateAdValorem = 0.176m;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "12345678";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.HongKong;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.HongKong;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1760m, invoiceLine.US_Duty);
			Factory.Save();

			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			drawback.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			drawback.Invoices.AddNew();
			var drawbackLine = drawback.InvoiceLines.AddNew();
			drawbackLine.US_DRWIsForImportSection = true;
			drawbackLine.US_ImportEntryNo = "XJ512345678";
			drawbackLine.US_DRWImportEntryLine = 1;
			drawbackLine.US_DRWClaimAmountOverriden_New = true;
			drawbackLine.US_DRWCalcDutyWithAdValoremRate = false;
			drawbackLine.DRWImportQuantity = 1000m;
			drawbackLine.DRWImportUQ = Core.Constants.Weight.Kilograms;
			drawbackLine.DRWExportQuantity = 100m;
			drawbackLine.DRWExportUQ = Core.Constants.Weight.Kilograms;
			drawbackLine.LineDuty = -1;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDrawback = newFactory.Load<JobDeclaration>(drawback.PK);
			var loadedLine = loadedDrawback.InvoiceLines[0];
			AssertEquals("loadedLine.LineDuty", 1760m, loadedLine.LineDuty);
			AssertEquals("loadedLine._99ClaimedDuty", 174.24m, loadedLine._99ClaimedDuty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
