using System.Data;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderPackagePivotCollection))]
	sealed class InvoiceHeaderPackagePivotCollectionTest : SupporterPackagePivotCollectionTest
	{
		protected override ICusLinkPackageSupporter GetSupporter()
		{
			var declaration = Factory.New<DeclarationSupportsInvoiceHeaderPackage>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			declaration.JE_MasterBill = "X";

			return invoice;
		}

		protected override ICusPackagePivot GetNewPivotOnSameDeclaration(BaseJobDeclaration declaration, BasePackage package)
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			return invoiceHeader.PackagesPivot.AddPivotFor(package);
		}

		class DeclarationSupportsInvoiceHeaderPackage : BaseJobDeclaration
		{
			public DeclarationSupportsInvoiceHeaderPackage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool SupportsChzPivotBetweenInvoiceHeaderAndPackingCore => true;
		}
	}
}
