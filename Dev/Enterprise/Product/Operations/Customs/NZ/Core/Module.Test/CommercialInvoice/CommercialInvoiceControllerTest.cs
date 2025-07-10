using System;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.CommercialInvoice;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
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
