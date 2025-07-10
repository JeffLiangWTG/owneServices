using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	sealed class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestTypedIndexer()
		{
			var collection = new InvoiceLineCompleteCollection(Declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		new JobDeclaration Declaration
		{
			get
			{
				return (JobDeclaration)base.Declaration;
			}
		}

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		public void TestSetDefaultFromPreviousLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			AssertNotEquals(ZGuid.Empty, invLine.JI_CEI);
			AssertEquals(declaration.CusEntryInstruction.PK, invLine.JI_CEI);
			invLine.JI_Group = "XXX";
			var testInvLine = invHeader.InvoiceLines.AddNew() as JobComInvoiceLine;
			AssertEquals("", testInvLine.JI_Group);
		}

		public void TestGeneratorEntryNumber()
		{
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_CustomsOffice = "BB";
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			entryInstruction.CEI_CustomsOffice = "AA";
			entryInstruction.CEI_BoxNumber = "123";
			Declaration.JE_CustomsOffice = "BB";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 16);
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.AllocateEntryNumber();
			AssertNotEquals(ZString.Empty, entryHeader.EntryNumber);
		}

		public void TestNotAutoAllocatePackageToInvoiceLineWhenPackageIsForInvoiceHeader()
		{
			CustomsDataRegistry.Instance.AutoAllocatePackageToInvoiceLines.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "PK";
			var invoice = declaration.Invoices.AddNew();
			invoice.ToggleLinkageWithPackage(package, true);
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertNotNull(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
			AssertEquals(1, invoiceLine.PackagesForInvoiceLinesForBindingOnly.Count);
			Assert("Should not be marked for invoice line as it is marked for invoice header", !invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
		}

		public void TestSetDefaultProcedureForCommonInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CusEntryInstruction.CEI_Style = "L1";
			var newInvoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("99", newInvoiceLine.JI_Procedure);
			declaration.CusEntryInstruction.CEI_Style = "F1";
			newInvoiceLine = declaration.InvoiceLines.AddNew();
			AssertNotEquals("99", newInvoiceLine.JI_Procedure);
		}

		public void TestSetDefaultOrderNumberForCommonInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV 1";
			invoiceHeader.JZ_OrderNumber = "order 123";
			var invoiceLineViewCollection = declaration.FilteredInvoiceLines;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_OrderNumber = "test1234";
			var clonedInvoiceLine = invoiceLineViewCollection.AddNew();
			AssertEquals("order 123", clonedInvoiceLine.JI_OrderNumber);
		}

		public override void TestSetDefaultValuesForTheFirstLineOnwardsWithOrderLineAttachedThatHasVeryLongOrderNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OrderNumber = "test1234";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine's JI_JZ", invoiceHeader.PK, invoiceLine1.JI_JZ);
			invoiceLine1.JI_OrderNumber = "123-45674";

			var organization = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order = Factory.New<Order>();
			order.BuyerPK = organization.PK;
			order.SupplierPK = organization.PK;
			order.JD_OrderNumber = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			AssertEquals("Pre-condition: order.JD_OrderNumber.Length", 26, order.JD_OrderNumber.Length);

			var orderLine = order.OrderLines.AddNew();
			invoiceLine1.JI_JO = orderLine.PK;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine2's JI_JZ copied from the first line", invoiceLine1.JI_JZ, invoiceLine2.JI_JZ);
			AssertEquals("invoiceLine2's JI_OrderNumber copied from the invoice header", "test1234", invoiceLine2.JI_OrderNumber);
		}

		public override void TestSetDefaultValuesForTheFirstLineOnwards()
		{
			var declaration = Declaration;
			var isDeclarationWithEntryInstruction = !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction;

			var invoice1 = GetInvoiceHeaderFromDec(declaration);
			var invoice2 = GetInvoiceHeaderFromDec(declaration);
			invoice1.JZ_OrderNumber = "Test1234";
			CusEntryInstruction testInstruction1 = null;
			CusEntryInstruction testInstruction2 = null;
			if (isDeclarationWithEntryInstruction)
			{
				testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction1.CEI_Style = "11";
				testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInstruction2.CEI_Style = "12";
			}
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine's JI_JZ", invoice1.PK, invoiceLine.JI_JZ);

			invoiceLine.JI_OrderNumber = "123-45674";
			if (isDeclarationWithEntryInstruction)
			{
				invoiceLine.JI_CEI = testInstruction2.PK;
			}

			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			AssertEquals("invoiceLine2's JI_JZ copied from the first line", invoiceLine.JI_JZ, invoiceLine2.JI_JZ);
			AssertEquals("Invoice line2's JI_OrderNumber copied from the invoice header", invoice1.JZ_OrderNumber, invoiceLine2.JI_OrderNumber);
			AssertEquals("Invoice line2's JI_Calc_EntryInstruction copied from the first line", isDeclarationWithEntryInstruction ? invoiceLine.JI_CEI : ZGuid.Empty, invoiceLine2.JI_CEI);

			invoiceLine2.JI_JZ = invoice2.PK;
			if (isDeclarationWithEntryInstruction)
			{
				invoiceLine2.JI_CEI = testInstruction1.PK;
			}
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			AssertEquals("invoiceLine3's JI_JZ copied", invoiceLine2.JI_JZ, invoiceLine3.JI_JZ);
			AssertEquals("Invoice line2's JI_Calc_EntryInstruction copied", isDeclarationWithEntryInstruction ? invoiceLine2.JI_CEI : ZGuid.Empty, invoiceLine3.JI_CEI);
		}
	}
}
