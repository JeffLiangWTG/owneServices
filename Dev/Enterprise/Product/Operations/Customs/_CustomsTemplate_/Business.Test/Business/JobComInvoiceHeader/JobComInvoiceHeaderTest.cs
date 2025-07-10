using System;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	class JobComInvoiceHeaderTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<JobComInvoiceHeader>("Update BaseJobComInvoiceHeaderTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseJobComInvoiceHeader)));
		}

		public override void TestLocalCurrencyCodeCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", GlbCompany.CurrentCompany.LocalCurrency.Code, Factory.New<JobComInvoiceHeader>().LocalCurrencyCode);
		}

		protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

		protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);
	}
}

