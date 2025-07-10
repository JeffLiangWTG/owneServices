using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderDocAddressValidationTest : WhsBusinessObjectValidationTestCase
	{
		public void TestValidateConsignee()
		{
			var order = Factory.New<WhsOrder>();
			order.ConsigneeDocAddress.Validation.ValidateOrganisationPK();
			AssertHasError(order.ConsigneeDocAddress.OrganisationPKInfo, "The Consignee has no address entered");
			var consignee = Factory.New<OrgHeader>();
			order.ConsigneeDocAddress.E2_OA_Address = consignee.Addresses.MainAddress.PK;
			order.ConsigneeDocAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(order.ConsigneeDocAddress.OrganisationPKInfo);
		}

		public void TestValidateTransportCo()
		{
			Globals.IsWeb = false;
			var order = Factory.New<WhsOrder>();
			order.TransportCoDocAddress.Validation.ValidateOrganisationPK();
			AssertHasWarning(order.TransportCoDocAddress.OrganisationPKInfo, "The Transport Company has no address entered");
			var transportCo = Factory.New<OrgHeader>();
			order.TransportCoDocAddress.E2_OA_Address = transportCo.Addresses.MainAddress.PK;
			order.TransportCoDocAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings(order.TransportCoDocAddress.OrganisationPKInfo);

			Globals.IsWeb = true;
			order = Factory.New<WhsOrder>();
			order.TransportCoDocAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings(order.TransportCoDocAddress.OrganisationPKInfo);
			transportCo = Factory.New<OrgHeader>();
			order.TransportCoDocAddress.E2_OA_Address = transportCo.Addresses.MainAddress.PK;
			order.TransportCoDocAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings(order.TransportCoDocAddress.OrganisationPKInfo);
		}

		public void TestValidateDistributionCentrePK()
		{
			var distributionCentre = Factory.New<OrgHeader>();
			distributionCentre.OH_IsActive = false;

			var order = Factory.New<WhsOrder>();
			order.DistributionCentreDocAddress.E2_OA_Address = distributionCentre.Addresses.MainAddress.PK;
			order.DistributionCentreDocAddress.Validation.ValidateOrganisationPK();

			AssertHasError(order.DistributionCentreDocAddress.OrganisationPKInfo, "This Organization is not active.");

			distributionCentre.OH_IsActive = true;
			order.DistributionCentreDocAddress.Validation.ValidateOrganisationPK();

			AssertNoErrors(order.DistributionCentreDocAddress.OrganisationPKInfo);
		}
	}
}
