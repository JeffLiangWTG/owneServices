using System;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.GUI.CommercialInvoice;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	sealed class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
	{
		protected override Customs.Business.BaseJobComInvoiceHeader GetNewInvoiceHeader()
		{
			return Factory.New<JobComInvoiceHeader>();
		}

		protected override Type ExpectedFormType
		{
			get
			{
				return typeof(CommercialInvoiceForm);
			}
		}
	}
}
