using System;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataTransfer.Testing
{
	sealed class USInvoicesGeneratorFromXSDTest : InvoicesGeneratorFromXSDTest
	{
		public override void TestImportInvoicesDetails()
		{
			Assert(true);
		}

		protected override Type ExpectedInvoiceAdapterType => typeof(USInvoiceDataAdapter);

		USInvoicesGeneratorFromXSD generator;
		protected override InvoicesGeneratorFromXSD Generator => generator ?? (generator = new USInvoicesGeneratorFromXSD(Factory.New<JobDeclaration>()));
	}
}
