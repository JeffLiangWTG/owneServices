namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingCFSShipmentTransactionTest : TrackingInvoiceLoaderTest
	{
		protected override ITransactionSupport GetNewBusinessObject()
		{
			var result = Factory.New<TrackingCFSShipment>();
			result.SiteUser = TestSiteUser;
			result.JS_UniqueConsignRef = "S00001000";
			result.ConsigneePK = TestOrg.PK;
			return result;
		}
	}
}
