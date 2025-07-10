using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	[TestedType(typeof(CusAddInfoCollection<AddInfoWithTypeCode, BaseJobComInvoiceLine>))]
	sealed class CusAddInfoCollectionWithMasterTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			return new CusAddInfoCollection<AddInfoWithTypeCode, BaseJobComInvoiceLine>(invoiceLine);
		}
	}
}
