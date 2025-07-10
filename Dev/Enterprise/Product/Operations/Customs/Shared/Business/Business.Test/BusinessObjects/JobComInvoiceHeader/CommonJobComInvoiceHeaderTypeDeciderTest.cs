using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CommonJobComInvoiceHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadInvoiceHeader()
		{
			BaseJobComInvoiceHeader invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			Type decidedType = new CommonJobComInvoiceHeaderTypeDecider().GetTypeForLoad(((INeedRow)invoiceHeader).Row, Factory);
			Assert(typeof(BaseJobComInvoiceHeader).IsAssignableFrom(decidedType));
		}

		public void TestGetTypeForLoadInvoiceGroupHeader()
		{
			BaseJobComInvoiceGroupHeader invoiceGroupHeader = Factory.New<BaseJobComInvoiceGroupHeader>();
			Type decidedType = new CommonJobComInvoiceHeaderTypeDecider().GetTypeForLoad(((INeedRow)invoiceGroupHeader).Row, Factory);
			Assert(typeof(BaseJobComInvoiceGroupHeader).IsAssignableFrom(decidedType));
		}
	}
}
