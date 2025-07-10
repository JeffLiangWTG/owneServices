using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingDateCollection))]
	sealed class PackingDateCollectionTest : CusCodeDataCollectionTest<PackingDate>
	{
		protected override CusCodeDataCollection<PackingDate> GetCusCodeDataCollection()
		{
			return new PackingDateCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<PackingDate>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew());
		JobComInvoiceLine invoiceLine;
	}
}
