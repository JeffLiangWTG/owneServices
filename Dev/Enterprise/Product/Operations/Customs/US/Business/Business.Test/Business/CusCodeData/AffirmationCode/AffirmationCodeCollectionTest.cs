using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AffirmationCodeCollection))]
	sealed class AffirmationCodeCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<AffirmationCode>
	{
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<AffirmationCode> GetCusCodeDataCollection() => InvoiceLine.FDAs[0].AffirmationCodes;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<AffirmationCode>();
			result.CY_ParentID = InvoiceLine.FDAs[0].PK;
			result.CY_ParentTableCode = "B7";
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
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.FDAs.AddNew();
				}
				return invoiceLine;
			}
		}
	}
}
