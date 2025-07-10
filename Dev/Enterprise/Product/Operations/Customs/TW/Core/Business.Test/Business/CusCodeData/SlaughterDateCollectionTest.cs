using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(SlaughterDateCollection))]
	sealed class SlaughterDateCollectionTest : CusCodeDataCollectionTest<SlaughterDate>
	{
		protected override CusCodeDataCollection<SlaughterDate> GetCusCodeDataCollection()
		{
			return new SlaughterDateCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<SlaughterDate>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = Factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew());
		JobComInvoiceLine invoiceLine;
	}
}
