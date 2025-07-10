using CargoWise.Types;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Test
{
	public class DangerousGoodsManifestJobDocAddressValidationTest : BaseAgencyTest
	{
		public void TestConsignee()
		{
			var org = Factory.New<OrgHeader>();
			Shipment.ConsigneePK = org.PK;

			Shipment.ConsigneeDocumentaryAddress.E2_CompanyName = ZString.Empty;
			Shipment.ConsigneeDocumentaryAddress.E2_Phone = ZString.Empty;
			Shipment.ConsigneeDocumentaryAddress.E2_Contact = ZString.Empty;
			Shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();

			AssertHasMessageError(Shipment.ConsigneeDocumentaryAddress.E2_OA_AddressInfo, "Consignee Company Name is required");
			AssertHasMessageError(Shipment.ConsigneeDocumentaryAddress.E2_ContactInfo, "Consignee Contact Name is required");
			AssertHasMessageError(Shipment.ConsigneeDocumentaryAddress.E2_ContactInfo, "Consignee Phone Number is required");
		}

		public void TestConsignor()
		{
			var org = Factory.New<OrgHeader>();
			Shipment.ConsignorPK = org.PK;

			Shipment.ConsignorDocumentaryAddress.E2_CompanyName = ZString.Empty;
			Shipment.ConsignorDocumentaryAddress.E2_Phone = ZString.Empty;
			Shipment.ConsignorDocumentaryAddress.E2_Contact = ZString.Empty;
			Shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();

			AssertHasMessageError(Shipment.ConsignorDocumentaryAddress.E2_OA_AddressInfo, "Consignor Company Name is required");
			AssertHasMessageError(Shipment.ConsignorDocumentaryAddress.E2_ContactInfo, "Consignor Contact Name is required");
			AssertHasMessageError(Shipment.ConsignorDocumentaryAddress.E2_ContactInfo, "Consignor Phone Number is required");
		}

		#region Implementation

		BillOfLading Shipment
		{
			get { return shipment ?? (shipment = Factory.New<BillOfLading>()); }
		}
		BillOfLading shipment;

		protected override void SetUp()
		{
			base.SetUp();
			DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(Factory);
		}

		#endregion
	}
}
