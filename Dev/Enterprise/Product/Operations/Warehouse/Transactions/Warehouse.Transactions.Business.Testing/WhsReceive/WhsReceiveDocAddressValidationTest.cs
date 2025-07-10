using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveDocAddressValidationTest : WhsBusinessObjectValidationTestCase
	{
		public void TestValidateTransportCo()
		{
			var receive = Factory.New<WhsReceive>();
			receive.TransportCoDocAddress.Validation.ValidateOrganisationPK();
			AssertHasWarning(receive.TransportCoDocAddress.OrganisationPKInfo, "The Transport Company has no address entered");

			var transportCo = Factory.New<OrgHeader>();
			receive.TransportCoPK = transportCo.PK;
			receive.TransportCoDocAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings(receive.TransportCoDocAddress.OrganisationPKInfo);
		}
	}
}
