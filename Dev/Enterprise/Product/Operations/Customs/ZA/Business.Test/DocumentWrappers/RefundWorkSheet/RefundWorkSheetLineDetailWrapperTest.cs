using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Testing;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class RefundWorkSheetLineDetailWrapperTest : DA63LineDetailWrapperTest
	{
		public void TestProductCodeOrDA63Description()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var testTariffType = testHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var testTariff = testHelper.CreateTariff("ZA", testTariffType.PK, "522021", new ZDateTime(2016, 1, 1), new ZDateTime(2017, 1, 1), "522021DESC", 1, "VAT");
			testHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "1", testTariff);
			Factory.Save();
			JobDeclaration declaration = entryLine.Declaration;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1.JI_Tariff = "522021";
			invoiceLine1.JI_Description = "LINE";
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("Description", "LINE", tester.ProductCodeOrDA63Description);
			invoiceLine1.JI_PartNo = "TestPart 1234";
			tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("PartNo", "TestPart 1234", tester.ProductCodeOrDA63Description);
		}

		public void TestPreviousEntryLineNumber()
		{
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals(0, tester.PreviousEntryLineNumber);
			invoiceLine1.JI_PreviousEntryLineNumber = 3;
			tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals(3, tester.PreviousEntryLineNumber);
		}

		public void TestTotalAmountClaimed()
		{
			invoiceLine1.JI_ImportCustomsValue = 1.02;
			invoiceLine1.JI_ImportDutyPaid = 1.03;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12A", 4.04m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 1.04m);
			invoiceLine1.JI_ImportVATPaid = 1.05;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 1.06m);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 1.07m);
			invoiceLine1.JI_ImportCustomsQty = 1.04m;
			invoiceLine1.JI_ImportCustomsQtyUQ = string.Empty;
			invoiceLine1.JI_ImportCustomsQty2 = 2.04m;
			invoiceLine1.JI_ImportCustomsQty2UQ = string.Empty;
			invoiceLine1.JI_ImportCustomsQty3 = 3.04m;
			invoiceLine1.JI_ImportCustomsQty3UQ = string.Empty;
			invoiceLine2.JI_ImportCustomsValue = 2.02;
			invoiceLine2.JI_ImportDutyPaid = 2.03;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(DA63AdditionalDuty.S1P2BDuty, 2.04m);
			invoiceLine2.JI_ImportVATPaid = 2.05;
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PPA, 2.06m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate(LineLevelProvisionalPayments.Codes.PEN, 2.07m);
			invoiceLine2.JI_ImportCustomsQty = 4.04m;
			invoiceLine2.JI_ImportCustomsQtyUQ = "KG";
			invoiceLine2.JI_ImportCustomsQty2 = 5.04m;
			invoiceLine2.JI_ImportCustomsQty2UQ = "KG";
			invoiceLine2.JI_ImportCustomsQty3 = 6.04m;
			invoiceLine2.JI_ImportCustomsQty3UQ = "KG";
			CombineAssertions(() =>
			{
				var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
				AssertEquals(25.80m, tester.TotalAmountClaimed);
			});
		}

		public void TestOtherDA63Duties()
		{
			JobDeclaration testOrgDeclaration = entryLine.Declaration;
			testOrgDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testOrgDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = testOrgDeclaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testEntryLine = entryLine;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("12A", 2);
			testEntryLine.Fees.AddOrUpdate("12A", 2);
			testEntryLine.Fees.AddOrUpdate("13A", 2);
			testEntryLine.Fees.AddOrUpdate("2P1", 2);
			testEntryLine.Fees.AddOrUpdate("2P2", 2);
			testEntryLine.Fees.AddOrUpdate("2P3", 2);
			testEntryLine.Fees.AddOrUpdate("VAT", 50m);
			testEntryLine.Fees.AddOrUpdate("VXX", 40m);
			testEntryLine.Fees.AddOrUpdate("XXX", 99.99m);
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals(10m, tester.OtherDA63Duties);
		}

		public void TestRefundWorkSheetRebatesUsed()
		{
			JobDeclaration declaration = entryLine.Declaration;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "6#";
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_Procedure = "11$";
			invoiceLine1.JI_Tariff = "991001";
			invoiceLine1.CusLineTariffDetails.RemoveAll();
			var tariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = "991012";
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("CusLineTariffDetails.Count", 1, tester.EntryLine.RandomLine.CusLineTariffDetails.Count);
			AssertEquals("Refund WorkSheetRebates Used", false, tester.RefundWorkSheetRebatesUsed);
			invoiceLine2.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine2.JI_CEI = entryInstruction1.PK;
			invoiceLine2.JI_Procedure = "11$";
			invoiceLine2.JI_Tariff = "85284910";
			invoiceLine2.CusLineTariffDetails.RemoveAll();
			var tariffDetail2 = invoiceLine2.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Type = "5P2";
			tariffDetail2.BZ_Tariff = "52203";
			tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("CusLineTariffDetails.Count", 1, tester.EntryLine.RandomLine.CusLineTariffDetails.Count);
			AssertEquals("Refund WorkSheetRebates Used", true, tester.RefundWorkSheetRebatesUsed);
			invoiceLine1.CusLineTariffDetails.RemoveAll();
			invoiceLine2.CusLineTariffDetails.RemoveAll();
			tariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = "991012";
			tariffDetail2 = invoiceLine2.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Type = "5P2";
			tariffDetail2.BZ_Tariff = "52203";
			tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			var countResult = tester.EntryLine.InvoiceLines.OfType<JobComInvoiceLine>().Select(x => x.CusLineTariffDetails.Count).Sum();
			AssertEquals("CusLineTariffDetails.Count", 2, countResult);
			AssertEquals("Refund WorkSheetRebates Used", true, tester.RefundWorkSheetRebatesUsed);
		}

		public void TestDivideOrMultiply()
		{
			JobDeclaration declaration = entryLine.Declaration;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "6#";
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_Procedure = "11$";
			invoiceLine1.JI_Tariff = "991001";
			invoiceLine1.CusLineTariffDetails.RemoveAll();
			var tariffDetail = invoiceLine1.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = "991012";
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("DivideOrMultiply can only be x", tester.DivideOrMultiply, "x");
		}

		public void TestConversionFactorAsString()
		{
			JobDeclaration declaration = entryLine.Declaration;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "6#";
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1.InvoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceLine1.JI_LinePrice = 333m;
			invoiceLine1.JI_ConversionFactor = 0.95m;
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("0.95", tester.ConversionFactorAsString);
		}

		public void TestFobValue()
		{
			JobDeclaration declaration = entryLine.Declaration;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "6#";
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1.InvoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceLine1.JI_LinePrice = 333m;
			invoiceLine1.JI_ImportCustomsValue = 123.45m;

			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			invoiceLine1.JI_ConversionFactor = 0m;
			AssertEquals("JI_ConversionFactor = 0", 0m, tester.FobValue);
			invoiceLine1.JI_ConversionFactor = 1m;
			AssertEquals("JI_ConversionFactor = 1", 123.45m, tester.FobValue);
			invoiceLine1.JI_ConversionFactor = 0.95m;
			AssertEquals("JI_ConversionFactor = 0.95", 129.95m, tester.FobValue);
		}

		public void TestUnitPricePerSuppliersInvoice()
		{
			JobDeclaration declaration = entryLine.Declaration;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "6#";
			invoiceLine1.InvoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceLine1.InvoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceLine1.JI_LinePrice = 333m;
			invoiceLine1.JI_ImportCustomsValue = 123.45m;
			invoiceLine1.JI_ConversionFactor = 1m;

			invoiceLine1.JI_ImportCustomsQty = 1m;
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("JI_ImportCustomsQty = 1", 123.45m, tester.UnitPricePerSuppliersInvoice);
			invoiceLine1.JI_ImportCustomsQty = 0m;
			tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("JI_ImportCustomsQty = 0", 0m, tester.UnitPricePerSuppliersInvoice);
			invoiceLine1.JI_ImportCustomsQty = 13m;
			tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("JI_ImportCustomsQty = 13", 9.50m, tester.UnitPricePerSuppliersInvoice);
		}

		public void TestDutiesForRefundSheet()
		{
			JobDeclaration testOrgDeclaration = entryLine.Declaration;
			testOrgDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testOrgDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrgInstruction = testOrgDeclaration.CustomsEntryInstructions.AddNew();
			testOrgInstruction.CEI_Style = "YY";
			var testEntryLine = entryLine;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("13D", 2);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("12A", 2);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("14D", 2);
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("13F", 2);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("13A", 5);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("2P2", 2);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("VAT", 50m);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("VXX", 40m);
			var tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("Should be 1 line", 1, refundWorkSheetDocumentWrapper.RefundWorkSheetLines.Count);
			AssertEquals("First tax type should be 12A", "12A", tester.FirstOtherDutyTypeForRefundSheet);
			AssertEquals("First tax amount should be 2.00", "2.00", tester.FirstOtherDutyAmountForRefundSheet);
			AssertEquals("Second tax type should be 13A", "13A", tester.SecondOtherDutyTypeForRefundSheet);
			AssertEquals("Second tax amount should be 5.00", "5.00", tester.SecondOtherDutyAmountForRefundSheet);
			var entryLine2 = entryLine.Header.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "99991";
			entryLine2.CL_CustomsValue = 2000m;
			entryLine2.CL_LineNumber = 3;
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine3);
			invoiceLine1.DA63AdditionalDuties.RemoveAndDeleteAll();
			invoiceLine2.DA63AdditionalDuties.RemoveAndDeleteAll();
			invoiceLine1.DA63AdditionalDuties.AddOrUpdate("13D", 2);
			invoiceLine2.DA63AdditionalDuties.AddOrUpdate("15A", 5);
			invoiceLine3.DA63AdditionalDuties.AddOrUpdate("13D", 7);
			refundWorkSheetDocumentWrapper = new RefundWorkSheetDocumentWrapper(entryLine.Header);
			tester = new RefundWorkSheetLineDetailWrapper(entryLine, refundWorkSheetDocumentWrapper);
			AssertEquals("Should be 2 lines", 2, refundWorkSheetDocumentWrapper.RefundWorkSheetLines.Count);
			AssertEquals("First tax type should be 13D", "13D", tester.FirstOtherDutyTypeForRefundSheet);
			AssertEquals("First tax amount should be 2.00", "2.00", tester.FirstOtherDutyAmountForRefundSheet);
			AssertEquals("Second tax type should be 15A", "15A", tester.SecondOtherDutyTypeForRefundSheet);
			AssertEquals("Second tax amount should be 5.00", "5.00", tester.SecondOtherDutyAmountForRefundSheet);
			refundWorkSheetDocumentWrapper = new RefundWorkSheetDocumentWrapper(entryLine.Header);
			tester = new RefundWorkSheetLineDetailWrapper(entryLine2, refundWorkSheetDocumentWrapper);
			AssertEquals("Should be 2 lines", 2, refundWorkSheetDocumentWrapper.RefundWorkSheetLines.Count);
			AssertEquals("First tax type should be 13D", "13D", tester.FirstOtherDutyTypeForRefundSheet);
			AssertEquals("First tax amount should be 7.00", "7.00", tester.FirstOtherDutyAmountForRefundSheet);
			AssertEquals("Second tax type should be empty", "", tester.SecondOtherDutyTypeForRefundSheet);
			AssertEquals("Second tax amount should be empty", "", tester.SecondOtherDutyAmountForRefundSheet);
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			refundWorkSheetDocumentWrapper = new RefundWorkSheetDocumentWrapper(entryHeader);
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine1);
			entryLine.InvoiceLines.Add(invoiceLine2);
		}

		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		RefundWorkSheetDocumentWrapper refundWorkSheetDocumentWrapper;
	}
}
