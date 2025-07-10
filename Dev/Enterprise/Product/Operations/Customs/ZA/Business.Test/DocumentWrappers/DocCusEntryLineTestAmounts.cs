using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	class DocCusEntryLineTestAmounts : TestDataClassForZAJobDeclaration
	{
		public void TestCustomsValue()
		{
			mockCusEntryLine.Setup(m => m.CL_CustomsValue).Returns(new ZDecimal(1200.10m));
			AssertEquals("Customs value for Line 2", 1200.10m, entryLineWrapper.CustomsValue);
		}

		public void TestVAT()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.Fees.AddOrUpdate("VAT", 1200.10m);
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invLine = invoice.InvoiceLines.AddNew();

			invLine.JI_CL = entryLine.PK;
			invLine.JI_ZZF_NKTaxType = "VAT";
			entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);
			AssertEquals("VAT for Line ", 1200.10m, entryLineWrapper.GSTVATAmount);
		}

		public void TestDuty()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 1200.10m);
			entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);
			AssertEquals("VAT for Line ", 1200.10m, entryLineWrapper.DutyAmountRounded);
		}

		public void TestCurrencyConverterNotBeNull()
		{
			InternalDocEntryLine internalEntryLineWrapper = InternalDocEntryLine.New(Declaration.CustomsEntryHeaders[0].MergedLines[0], Factory);
			AssertNotNull(internalEntryLineWrapper.CurrencyConverter);
		}

		public void TestHasVTEAddInfo()
		{
			ICusEntryLine entryLine = Declaration.CustomsEntryHeaders[0].MergedLines[0];
			AdditionalInformation vTE = Declaration.CustomsEntryHeaders[0].MergedLines[0].AdditionalInformationCodes.AddNew("VTE");
			InternalDocEntryLine internalEntryLineWrapper = InternalDocEntryLine.New(entryLine, Factory);
			AssertEquals("TRUE", internalEntryLineWrapper.HasVTEAddInfo);

			Declaration.CustomsEntryHeaders[0].MergedLines[0].AdditionalInformationCodes.Remove(vTE);
			internalEntryLineWrapper = InternalDocEntryLine.New(entryLine, Factory);
			AssertEquals("FALSE", internalEntryLineWrapper.HasVTEAddInfo);
		}

		/// <summary> Incident#I00020827 </summary>
		public void TestMarkupPercentInSupplierImporterLinkIsWrapped()
		{
			OrgHeader supplier = OrgHeader.New(Factory);
			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_RL_NKClosestPort = "ZAAAM";
			OrgSupplierBuyerLink link = importer.SupplierLinks.AddNew(supplier);
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.SouthAfrica;
			link.OL_ValuationBasisMarkupPercent = 10m;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = testInstruction.PK;
			invoiceLine.JI_Tariff = "123";

			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			DocCusEntryLine docEntryLine = DocCusEntryLine.New(declaration.CustomsEntryHeaders[0].MergedLines[0], Factory);

			AssertEquals(0.1m, docEntryLine.MarkupPercent);
		}

		protected DocCusEntryLine entryLineWrapper;
		protected Mock<CusEntryLine> mockCusEntryLine;

		protected override void SetUp()
		{
			base.SetUp();
			mockCusEntryLine = Factory.NewMoq<CusEntryLine>();
			entryLineWrapper = DocCusEntryLine.New(mockCusEntryLine.Object, Factory);
		}

		protected class InternalDocEntryLine : DocCusEntryLine
		{
			InternalDocEntryLine(ICusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
				: base(cusEntryLine, factoryToWrap)
			{
			}

			public new CurrencyConverter CurrencyConverter
			{
				get { return base.CurrencyConverter; }
			}

			public new static InternalDocEntryLine New(ICusEntryLine entryLine, BusinessObjectFactory factoryToWrap)
			{
				return new InternalDocEntryLine(entryLine, factoryToWrap);
			}
		}
	}
}
