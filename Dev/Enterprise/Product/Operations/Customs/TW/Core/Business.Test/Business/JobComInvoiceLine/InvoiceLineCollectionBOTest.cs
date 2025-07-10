using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection))]
	sealed class InvoiceLineCollectionBOTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
	{
		public void TestCopyLastLineDetailsToNewLinesIfEnabled()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";
			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine1 = invoiceLineViewCollection.AddNew();
			invoiceLine1.CertificateOfOriginNumber = "X8";
			invoiceLine1.CertificateOfOriginNumberItemNumber = 1;
			invoiceLine1.JI_BondedGoodsCode = "X1";
			invoiceLine1.JI_InvoiceQuantity = 1;
			invoiceLine1.JI_LinePrice = 20;
			invoiceLine1.JI_Procedure = "38";
			invoiceLine1.JI_UseOneTenthCV = false;
			invoiceLine1.JI_RAPCurr = "JPY";
			invoiceLine1.JI_Calc_RAPRORUnitPrice = 100m;
			invoiceLine1.JI_RAPPrice = 100m;

			try
			{
				((IBusinessObjectInternals)invoiceLine1).IsCopying = true;
				invoiceLine1.JI_EnteredUnitPrice = 30m;
			}
			finally
			{
				((IBusinessObjectInternals)invoiceLine1).IsCopying = false;
			}

			AssertEquals(20m, invoiceLine1.JI_LinePrice);
			AssertEquals(30m, invoiceLine1.JI_EnteredUnitPrice);

			var permit1 = invoiceLine1.PermitCusSupportingCollection.AddNew();
			permit1.CSI_ReferenceNumber = "T00101";
			permit1.CSI_LineNo = 1;
			var permit2 = invoiceLine1.PermitCusSupportingCollection.AddNew();
			permit2.CSI_ReferenceNumber = "T00102";
			permit2.CSI_LineNo = 2;
			var permitExemptionCode1 = invoiceLine1.ExemptionOfControllingAgenciesCusSupportings.AddNew();
			permitExemptionCode1.CSI_ReferenceNumber = "T00201";

			var clonedInvoiceLineForImportTesting = invoiceLineViewCollection.AddNew();
			AssertEquals("X8", clonedInvoiceLineForImportTesting.CertificateOfOriginNumber);
			AssertEquals(ZShort.Zero, clonedInvoiceLineForImportTesting.CertificateOfOriginNumberItemNumber);
			AssertEquals("X1", clonedInvoiceLineForImportTesting.JI_BondedGoodsCode);
			AssertEquals(20m, clonedInvoiceLineForImportTesting.JI_LinePrice);
			AssertEquals(30m, clonedInvoiceLineForImportTesting.JI_EnteredUnitPrice);
			AssertEquals("PermitCusSupportingCollection.Count", 2, clonedInvoiceLineForImportTesting.PermitCusSupportingCollection.Count);
			CombineAssertions("PermitCusSupportingCollection", () =>
			{
				AssertEquals("[0].CSI_ReferenceNumber", "T00101", clonedInvoiceLineForImportTesting.PermitCusSupportingCollection[0].CSI_ReferenceNumber);
				AssertEquals("[0].CSI_LineNo", 1, clonedInvoiceLineForImportTesting.PermitCusSupportingCollection[0].CSI_LineNo);
				AssertEquals("[1].CSI_ReferenceNumber", "T00102", clonedInvoiceLineForImportTesting.PermitCusSupportingCollection[1].CSI_ReferenceNumber);
				AssertEquals("[1].CSI_LineNo", 2, clonedInvoiceLineForImportTesting.PermitCusSupportingCollection[1].CSI_LineNo);
			});
			AssertEquals("ExemptionOfControllingAgenciesCusSupportings.Count", 1, clonedInvoiceLineForImportTesting.ExemptionOfControllingAgenciesCusSupportings.Count);
			AssertEquals("ExemptionOfControllingAgenciesCusSupportings.CSI_ReferenceNumber", "T00201", clonedInvoiceLineForImportTesting.ExemptionOfControllingAgenciesCusSupportings[0].CSI_ReferenceNumber);
			AssertEquals("JPY", clonedInvoiceLineForImportTesting.JI_RAPCurr);
			AssertEquals(100m, clonedInvoiceLineForImportTesting.JI_Calc_RAPRORUnitPrice);
			AssertEquals(100m, clonedInvoiceLineForImportTesting.JI_RAPPrice);
			AssertEquals(false, clonedInvoiceLineForImportTesting.JI_UseOneTenthCV);

			invoiceLineViewCollection.RemoveAndDelete(clonedInvoiceLineForImportTesting);
			invoiceLine1.JI_UseOneTenthCV = true;
			clonedInvoiceLineForImportTesting = invoiceLineViewCollection.AddNew();
			AssertEquals(true, clonedInvoiceLineForImportTesting.JI_UseOneTenthCV);

			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = false;
			invoiceLineViewCollection.RemoveAndDelete(clonedInvoiceLineForImportTesting);
			clonedInvoiceLineForImportTesting = invoiceLineViewCollection.AddNew();
			AssertEquals(0m, clonedInvoiceLineForImportTesting.JI_LinePrice);
			AssertEquals(0m, clonedInvoiceLineForImportTesting.JI_EnteredUnitPrice);
			AssertEquals(false, clonedInvoiceLineForImportTesting.JI_UseOneTenthCV);

			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLineViewCollection.RemoveAndDelete(clonedInvoiceLineForImportTesting);
			var clonedInvoiceLineForExportTesting = invoiceLineViewCollection.AddNew();
			AssertEquals("X8", clonedInvoiceLineForExportTesting.CertificateOfOriginNumber);
			AssertEquals(ZShort.Zero, clonedInvoiceLineForExportTesting.CertificateOfOriginNumberItemNumber);
			AssertEquals("X1", clonedInvoiceLineForExportTesting.JI_BondedGoodsCode);
		}

		public void TestCopyLastLineDetailsToNewLinesForPaymentMethods()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "INV 1";
				var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
				invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
				var invoiceLine1 = invoiceLineViewCollection.AddNew();
				invoiceLine1.JI_DtyPymntMthd = "DEF";
				invoiceLine1.JI_TpfPymntMthd = "DEF";
				invoiceLine1.JI_VatPymntMthd = "DEF";
				Factory.Save();
				invoiceLine1.JI_DtyPymntMthd = "CAS";
				invoiceLine1.JI_TpfPymntMthd = "CAS";
				invoiceLine1.JI_VatPymntMthd = "CAS";
				var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
				AssertEquals("TW_DtyPymntMthd should be copied.", "CAS", clonedInvoiceLine.JI_DtyPymntMthd);
				AssertEquals("TW_TpfPymntMthd should be copied.", "CAS", clonedInvoiceLine.JI_TpfPymntMthd);
				AssertEquals("TW_VatPymntMthd should be copied.", "CAS", clonedInvoiceLine.JI_VatPymntMthd);
			});

			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "INV 1";
				var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
				invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
				var invoiceLine1 = invoiceLineViewCollection.AddNew();
				invoiceLine1.JI_DtyPymntMthd = "DEF";
				invoiceLine1.JI_TpfPymntMthd = "DEF";
				invoiceLine1.JI_VatPymntMthd = "DEF";
				var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
				AssertEquals("TW_DtyPymntMthd should be copied.", "DEF", clonedInvoiceLine.JI_DtyPymntMthd);
				AssertEquals("TW_TpfPymntMthd should be copied.", "DEF", clonedInvoiceLine.JI_TpfPymntMthd);
				AssertEquals("TW_VatPymntMthd should be copied.", "DEF", clonedInvoiceLine.JI_VatPymntMthd);
			});

			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "INV 1";
				var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
				invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
				var invoiceLine1 = invoiceLineViewCollection.AddNew();
				invoiceLine1.JI_DtyPymntMthd = "DEF";
				invoiceLine1.JI_TpfPymntMthd = "DEF";
				invoiceLine1.JI_VatPymntMthd = "DEF";
				Factory.Save();
				invoiceLine1.JI_DtyPymntMthd = "CAS";
				invoiceLine1.JI_TpfPymntMthd = "CAS";
				invoiceLine1.JI_VatPymntMthd = "CAS";
				var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
				AssertEquals("TW_DtyPymntMthd should be copied.", "CAS", clonedInvoiceLine.JI_DtyPymntMthd);
				AssertEquals("TW_TpfPymntMthd should be copied.", "CAS", clonedInvoiceLine.JI_TpfPymntMthd);
				AssertEquals("TW_VatPymntMthd should be copied.", "CAS", clonedInvoiceLine.JI_VatPymntMthd);
			});

			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "INV 1";
				var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
				invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
				var invoiceLine1 = invoiceLineViewCollection.AddNew();
				invoiceLine1.JI_DtyPymntMthd = "DEF";
				invoiceLine1.JI_TpfPymntMthd = "DEF";
				invoiceLine1.JI_VatPymntMthd = "DEF";
				var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
				AssertEquals("TW_DtyPymntMthd should be copied.", "DEF", clonedInvoiceLine.JI_DtyPymntMthd);
				AssertEquals("TW_TpfPymntMthd should be copied.", "DEF", clonedInvoiceLine.JI_TpfPymntMthd);
				AssertEquals("TW_VatPymntMthd should be copied.", "DEF", clonedInvoiceLine.JI_VatPymntMthd);
			});
		}

		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = new InvoiceLineViewCollection(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestCopyInvoiceLineTaxIfNeeded()
		{
			var universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = universalReferenceTestDataHelper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var tariffTypeCT = universalReferenceTestDataHelper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "CT");
			Factory.Save();
			var tariff = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "22090000005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalReferenceTestDataHelper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariff);
			var childTariff = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeCT.PK, "OTHERBEVERAGE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalReferenceTestDataHelper.CreateTariffRelationship(childTariff.PK, tariffTypeHSN.PK, "22090000005");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";
			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine1 = invoiceLineViewCollection.AddNew();
			invoiceLine1.JI_Tariff = "22090000005";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			AssertEquals(1, invoiceLine1.Taxes.Count);
			AssertEquals("CT", invoiceLine1.Taxes[0].JLT_Type);
			AssertEquals("OTHERBEVERAGE", invoiceLine1.Taxes[0].JLT_Tariff);
			var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			AssertEquals(1, clonedInvoiceLine.Taxes.Count);
			AssertEquals("CT", clonedInvoiceLine.Taxes[0].JLT_Type);
			AssertEquals("OTHERBEVERAGE", clonedInvoiceLine.Taxes[0].JLT_Tariff);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			AssertEquals(0, clonedInvoiceLine.Taxes.Count);
		}

		public void TestCopyLastLineDetailsToNewLinesForJI_Group()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";
			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine1 = invoiceLineViewCollection.AddNew();
			invoiceLine1.JI_Group = "AA";
			var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			AssertEquals("", clonedInvoiceLine.JI_Group);
		}

		public void TestCopyPreviousBondedEntryNumberInfo()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";
			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine = invoiceLineViewCollection.AddNew();
			invoiceLine.PreviousBondedEntryNumber = "Entry1234";
			AssertEquals("Entry1234", invoiceLine.PreviousBondedEntryNumber);
			invoiceLine.PreviousBondedEntryNumber = "Entry1235";
			var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			AssertEquals("Entry1235", clonedInvoiceLine.PreviousBondedEntryNumber);
		}

		public void TestCopyLastLineDetailsToNewLinesForTrademarkStorageDocsGuid()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";
			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			invoiceLineViewCollection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine1 = invoiceLineViewCollection.AddNew();
			var trademark = ZGuid.NewZGuid();
			invoiceLine1.TrademarkStorageDocsGuid = trademark;
			var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			AssertEquals(trademark, clonedInvoiceLine.TrademarkStorageDocsGuid);
		}

		public void TestRebuildControllingMessageHeaderLinkInvoiceLinesOnAdded()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader1 = entryInstruction.ControllingMessageHeaders.AddNew();
			var messageHeader2 = entryInstruction.ControllingMessageHeaders.AddNew();
			var testCollection1 = messageHeader1.ControllingMessageHeaderLinkInvoiceLines;
			var testCollection2 = messageHeader2.ControllingMessageHeaderLinkInvoiceLines;
			Assert(messageHeader1.IsControllingMessageHeaderLinkInvoiceLinesLoaded);
			Assert(messageHeader2.IsControllingMessageHeaderLinkInvoiceLinesLoaded);
			AssertEquals(2, testCollection1.Count);
			AssertEquals(2, testCollection2.Count);

			var line3 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			AssertEquals(3, testCollection1.Count);
			AssertEquals(3, testCollection2.Count);
		}

		public void TestRebuildInvoiceQuantityAndUnitQtyResultOnAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines = declaration.InvoiceLines;
			var invoiceLine1 = invoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";

			var invoiceLine2 = (JobComInvoiceLine)((IBindingList)invoiceLines).AddNew();
			invoiceLines.Remove(invoiceLine2);
			invoiceLine2.JI_InvoiceQuantity = 100;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLines.Add(invoiceLine2);

			CombineAssertions(() =>
			{
				AssertEquals(200m, invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceQuantity);
				AssertEquals("PCE", invoiceLine2.InvoiceQuantityAndUnitQtyResultCollection[0].InvoiceUQ);
			});
		}

		protected override InvoiceLineViewCollection GetCollectionToTest()
		{
			return new InvoiceLineViewCollection(JobDeclaration);
		}

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	}
}
