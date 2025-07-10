using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingHouseCollection))]
	sealed class PackingHouseCollectionTest : CusCodeDataCollectionTest<PackingHouse>
	{
		protected override CusCodeDataCollection<PackingHouse> GetCusCodeDataCollection()
		{
			return new PackingHouseCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<PackingHouse>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew());
		JobComInvoiceLine invoiceLine;
	}
}
