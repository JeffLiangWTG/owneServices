using System;
using Enterprise.Customs.NO.Business;
using Enterprise.Customs.NO.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	sealed class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader() => Factory.New<JobComInvoiceHeader>();

		protected override Type ExpectedFormType => typeof(CommercialInvoiceForm);
	}
}
