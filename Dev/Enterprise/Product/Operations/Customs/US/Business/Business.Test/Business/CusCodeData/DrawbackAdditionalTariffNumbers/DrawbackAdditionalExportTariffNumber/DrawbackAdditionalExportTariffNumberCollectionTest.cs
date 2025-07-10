using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackAdditionalExportTariffNumberCollection))]
	sealed class DrawbackAdditionalExportTariffNumberCollectionTest : CusCodeDataCollectionTest<DrawbackAdditionalExportTariffNumber>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<DrawbackAdditionalExportTariffNumber> GetCusCodeDataCollection() => InvoiceLine.DrawbackAdditionalExportTariffNumbers;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			DrawbackAdditionalExportTariffNumber result = Factory.New<DrawbackAdditionalExportTariffNumber>();
			result.CY_Type = CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber;
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			return result;
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}
	}
}
