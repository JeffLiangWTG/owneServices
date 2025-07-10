using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineReservedFieldCollection))]
	sealed class JobComInvoiceLineReservedFieldCollectionTest : ReservedFieldCollectionTest<JobComInvoiceLineReservedField>
	{
		protected override CusCodeDataCollection<JobComInvoiceLineReservedField> GetCusCodeDataCollection()
		{
			return new JobComInvoiceLineReservedFieldCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<JobComInvoiceLineReservedField>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
