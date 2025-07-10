using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(WHSPackLine))]
	public class WHSPackLineTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<WHSPackLine>
	{
		public void TestDefaults()
		{
			var line = Factory.New<WHSPackLine>();
			AssertEquals("B7_ParentTableCode", JobDeclarationSchema.Constants.Prefix, line.B7_ParentTableCode);
		}

		public void TestCalcProperties()
		{
			var whsPack = Declaration.WHSPacks.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var line = Declaration.WHSPackLines.AddNew(whsPack);
			line.US_JI_InvoiceLine = invoiceLine.PK;
			AssertEquals("B7_Calc_NoOfPackages", 0, line.B7_Calc_NoOfPackages);
			AssertEquals("B7_Calc_QtyInSinglePackage", 0m, line.B7_Calc_QtyInSinglePackage);

			whsPack.US_PackageQty = 10;
			AssertEquals("B7_Calc_NoOfPackages", 10, line.B7_Calc_NoOfPackages);
			AssertEquals("B7_Calc_QtyInSinglePackage", 0m, line.B7_Calc_QtyInSinglePackage);

			line.US_PackedQty = 150m;
			AssertEquals("B7_Calc_NoOfPackages", 10, line.B7_Calc_NoOfPackages);
			AssertEquals("B7_Calc_QtyInSinglePackage", 15m, line.B7_Calc_QtyInSinglePackage);
		}

		public void TestSettingInvoiceLineSetQtyIfNotEntered()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 10m;
			var line1 = Declaration.WHSPackLines.AddNew();
			line1.US_JI_InvoiceLine = invoiceLine1.PK;
			AssertEquals(ZDecimal.Zero, line1.US_PackedQty);
			line1.US_JI_InvoiceLine = invoiceLine2.PK;
			AssertEquals(10m, line1.US_PackedQty);
			line1.US_JI_InvoiceLine = invoiceLine1.PK;
			AssertEquals(10m, line1.US_PackedQty);
			line1.US_PackedQty = 6m;
			var line2 = Declaration.WHSPackLines.AddNew();
			AssertEquals(ZDecimal.Zero, line2.US_PackedQty);
			line2.US_JI_InvoiceLine = invoiceLine1.PK;
			AssertEquals(ZDecimal.Zero, line2.US_PackedQty);
			line1.US_JI_InvoiceLine = invoiceLine2.PK;
			line2.US_JI_InvoiceLine = invoiceLine2.PK;
			AssertEquals(4m, line2.US_PackedQty);
		}

		public void TestClone()
		{
			var line = Declaration.WHSPackLines.AddNew(WHSPack);
			line.US_PackedQty = 10m;
			var clonedLine = (WHSPackLine)line.Clone();
			AssertEquals(10m, clonedLine.US_PackedQty);
		}

		public void TestHumanReadableNameCore()
		{
			AssertEquals("WHS Pack Line Details", WHSPackLine.HumanReadableName);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var whsPack = declaration.WHSPacks.AddNew();
			whsPack.US_PackageQty = 1;
			return declaration.WHSPackLines.AddNew(whsPack);
		}

		WHSPackLine WHSPackLine
		{
			get { return whsPackLine ?? (whsPackLine = Declaration.WHSPackLines.AddNew(WHSPack)); }
		}
		WHSPackLine whsPackLine;

		WHSPack WHSPack
		{
			get { return whsPack ?? (whsPack = Declaration.WHSPacks.AddNew()); }
		}
		WHSPack whsPack;

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		#endregion
	}
}
