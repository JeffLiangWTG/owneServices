using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BasePackagePivotsCollection<InvoiceHeaderPackagePivot>))]
	public class BasePackagePivotsCollectionTest : BusinessObjectCollectionTestCase
	{
		public virtual void TestCalculateDefaultNumberOfPacks()
		{
			var standAlonePackage = Factory.New<BasePackage>();
			standAlonePackage.CW_PackQty = 100;
			var standAloneInvoice = Factory.New<BaseJobComInvoiceLine>();
			var pivot = standAloneInvoice.PackagesPivot.AddPivotFor(standAlonePackage);
			AssertNull("Prerequisite", standAlonePackage.Declaration);
			AssertEquals("New line package pivot quantity should not use CalculateDefaultNumberOfPacks() when package has no declaration.", 0, pivot.NumberOfPacks);

			var mock = Factory.NewMoq<BaseJobDeclaration>();
			mock.Protected().Setup<bool>("SupportsChcPivotBetweenInvoiceLineAndPackingCore").Returns(true);
			var declaration = mock.Object;

			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			var package = bill.PackingGroups[0].Packages.AddNew();
			package.CW_PackQty = 10;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var pivotCollection1 = invoiceLine1.PackagesPivot;
			var pivot1 = pivotCollection1.AddPivotFor(package);
			AssertEquals("New line package pivot quantity should be defaulted to residual packages quantity.", 10, pivot1.NumberOfPacks);

			pivot1.NumberOfPacks = 2;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var pivotCollection2 = invoiceLine2.PackagesPivot;
			var pivot2 = pivotCollection2.AddPivotFor(package);
			AssertEquals("New line package pivot quantity should be defaulted to remaining packs quantity when not all packages have already be consumed by previous lines.", 8, pivot2.NumberOfPacks);

			pivot2.NumberOfPacks = 5;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var pivotCollection3 = invoiceLine3.PackagesPivot;
			var pivot3 = pivotCollection3.AddPivotFor(package);
			AssertEquals("New line package pivot quantity should be defaulted to 0 when all packages have already been consumed by previous lines.", 3, pivot3.NumberOfPacks);

			pivot3.NumberOfPacks = 10;
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			var pivotCollection4 = invoiceLine4.PackagesPivot;
			var pivot4 = pivotCollection4.AddPivotFor(package);
			AssertEquals("New line package pivot quantity should be defaulted to 0 when consumed packages quantity exceeds package availability.", 0, pivot4.NumberOfPacks);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			declaration.JE_MasterBill = "X";

			var bill = declaration.PrimaryMasterBill;
			var package = bill.PackingGroups[0].Packages.AddNew();

			return new BasePackagePivotsCollection<InvoiceHeaderPackagePivot>(package);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<InvoiceHeaderPackagePivot>();
		}
	}
}
