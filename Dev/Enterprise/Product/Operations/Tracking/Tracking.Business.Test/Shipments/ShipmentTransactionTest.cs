namespace Enterprise.Tracking.Business.Testing
{
	sealed class ShipmentTransactionTest : TrackingInvoiceLoaderTest
	{
		protected override ITransactionSupport GetNewBusinessObject()
		{
			TrackingShipment result = Factory.New<TrackingShipment>();
			result.SiteUser = TestSiteUser;
			result.JS_UniqueConsignRef = "S00001000";
			result.ConsigneePK = TestOrg.PK;
			return result;
		}
	}
}
