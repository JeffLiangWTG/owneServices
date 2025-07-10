using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EmptyInvoiceHeaderValidationTest : TestCaseWithFactory
	{
		public void TestRunReconRelatedValidationsOnly()
		{
			ReconInvoice.Validation.ValidateAll();
			AssertEquals("No message errors expected. There is a validation that is not applicable to Recon", false, ReconInvoice.HasMessageErrors);
			AssertEquals("No errors expected. There is a validation that is not applicable to Recon", false, ReconInvoice.HasErrors);
			AssertEquals("No warnings expected. There is a validation that is not applicable to Recon", false, ReconInvoice.HasWarnings);
		}

		public void TestRunDrawbackRelatedValidationOnly()
		{
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoice = drawback.Invoices.AddNew();
			invoice.JZ_IncoTerm = ZString.Empty;
			invoice.JZ_WeightUQ = "XX";
			invoice.Validation.ValidateAll();
			AssertEquals("No message errors expected. There is a validation that is not applicable to Recon", false, invoice.HasMessageErrors);
			AssertEquals("No errors expected. There is a validation that is not applicable to Recon", false, invoice.HasErrors);
			AssertEquals("No warnings expected. There is a validation that is not applicable to Recon", false, invoice.HasWarnings);
		}

		JobComInvoiceHeader reconInvoice;
		JobComInvoiceHeader ReconInvoice
		{
			get
			{
				if (reconInvoice == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var recon = new ReconDeclaration(declaration);
					reconInvoice = recon.Invoices.AddNew();
				}

				return reconInvoice;
			}
		}
	}
}
