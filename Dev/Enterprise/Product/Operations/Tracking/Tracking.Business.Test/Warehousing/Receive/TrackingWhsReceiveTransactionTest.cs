using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[SetGlobalsIsWeb]
	[HttpContextEnabledTest]
	sealed class TrackingWhsReceiveTransactionTest : TrackingInvoiceLoaderTest
	{
		protected override ITransactionSupport GetNewBusinessObject()
		{
			TrackingWhsReceive result = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			result.WhsReceive.WD_OH_Client = TestOrg.PK;
			result.WhsReceive.WD_ExternalReference = "S00001000";
			result.WhsReceive.WD_WW_Whs = Factory.NewWithValidTestData<WhsWarehouse>().PK;
			result.SiteUser = TestSiteUser;
			return result;
		}
	}
}
