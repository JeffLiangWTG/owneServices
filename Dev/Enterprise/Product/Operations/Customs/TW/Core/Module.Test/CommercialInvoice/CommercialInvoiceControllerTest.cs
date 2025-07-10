using System;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceController))]
	public sealed class CommercialInvoiceControllerTest : Customs.Module.Testing.CommercialInvoiceControllerTest
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
