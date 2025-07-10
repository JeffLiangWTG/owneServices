using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceFilterBusinessObject))]
	sealed class CommercialInvoiceFilterBusinessObjectTest : Customs.Module.Testing.CommercialInvoiceFilterBusinessObjectTest
	{
		public void TestLookups()
		{
			var filterBizObj = new CommercialInvoiceFilterBusinessObject();
			AssertType<CommercialInvoiceFilterLookups>(filterBizObj.Lookups);
		}
	}
}
