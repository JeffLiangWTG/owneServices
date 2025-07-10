using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestActiveOrganisationValidation()
		{
			string errorMessage = "This Organization is not active.";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = org.PK;
			shipment.NotifyPartyDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoError(shipment.NotifyPartyDocumentaryAddress.OrganisationPKInfo, errorMessage);

			org.OH_IsActive = false;
			shipment.NotifyPartyDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasError(shipment.NotifyPartyDocumentaryAddress.OrganisationPKInfo, errorMessage);
		}

		#region TestValidateConsigneePK

		public void TestValidateConsigneePK()
		{
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee1.OH_IsConsignee = ZBool.True;

			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee2.OH_IsConsignee = ZBool.True;

			CommonConsol bCNConsol = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			bCNConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			bCNConsol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;

			CommonShipment bCNShipment1 = bCNConsol.Shipments.AddNew();
			bCNShipment1.ConsigneePK = consignee1.PK;
			bCNShipment1.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings("Expect no warnings on consignee field of first CommonShipment added", bCNShipment1.ConsigneePKInfo);

			CommonShipment bCNShipment2 = bCNConsol.Shipments.AddNew();
			bCNShipment2.ConsigneePK = consignee1.PK;
			bCNShipment1.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings("Expect no warnings on consignee field of subsequent CommonShipment which matches first shipment", bCNShipment2.ConsigneePKInfo);

			CommonShipment bCNShipment3 = bCNConsol.Shipments.AddNew();
			bCNShipment3.ConsigneePK = consignee2.PK;
			bCNShipment1.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasWarnings("Expect warning on consignee field of subsequent CommonShipment which does not match first shipment", bCNShipment3.ConsigneePKInfo);

			CommonShipment unrelatedShipment = Factory.NewWithValidTestData<CommonShipment>();
			unrelatedShipment.ConsigneePK = consignee2.PK;
			bCNShipment1.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings("Expect no warnings on consignee field of CommonShipment unrelated to BCN consol", unrelatedShipment.ConsigneePKInfo);

			CommonConsol nonBCNConsol = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			nonBCNConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			nonBCNConsol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			CommonShipment nonBCNShipment1 = nonBCNConsol.Shipments.AddNew();
			nonBCNShipment1.ConsigneePK = consignee1.PK;
			bCNShipment1.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings("Expect no warnings on consignee field of first CommonShipment added", nonBCNShipment1.ConsigneePKInfo);

			CommonShipment nonBCNShipment2 = nonBCNConsol.Shipments.AddNew();
			nonBCNShipment2.ConsigneePK = consignee1.PK;
			bCNShipment1.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarnings("Expect no warnings on consignee field of subsequent CommonShipment which does not match first shipment", bCNShipment2.ConsigneePKInfo);
		}

		#endregion

		#region TestValidateConsignorVsConsignee

		public void TestValidateConsignorVsConsignee()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsignorPK = ZGuid.Empty;

			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasErrors("Empty Consignor is error if Consignee is empty.", shipment.ConsignorPKInfo);

			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasErrors("Dont loose errors when validation called dirrectly.", shipment.ConsignorPKInfo);

			OrgHeader org = Factory.New<OrgHeader>();
			shipment.ConsigneePK = org.PK;
			shipment.ConsignorDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors("Empty Consignor should be valid if Consignee not empty.", shipment.ConsignorPKInfo);
		}

		#endregion

		#region TestValidateConsigneeVsConsignor

		public void TestValidateConsigneeVsConsignor()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsignorPK = ZGuid.Empty;

			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasErrors("Empty Consignee is error if Consignor is empty.", shipment.ConsigneePKInfo);

			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasErrors("Dont loose errors when validation called dirrectly.", shipment.ConsigneePKInfo);

			OrgHeader org = Factory.New<OrgHeader>();
			shipment.ConsignorPK = org.PK;
			shipment.ConsigneeDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors("Empty Consignee should be valid if Consignor not empty.", shipment.ConsigneePKInfo);
		}

		#endregion

		#region TestValidateNotifyParty

		public void TestValidateNotifyParty()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsigneePK = ZGuid.Empty;
			shipment.ConsignorPK = ZGuid.Empty;

			shipment.NotifyPartyDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors("Dont add errors to notify party just because Consignor/Consignee are not set", shipment.NotifyPartyDocumentaryAddress.OrganisationPKInfo);
		}

		#endregion

		#region TestValidateManufacturer

		public void TestValidateManufacturer()
		{
			var shipment = Factory.New<CommonShipment>();

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.ManufacturerDocAddress.Validation.ValidateOrganisationPK();

			AssertNoErrors("Shipment dont call the validation when the shipment type is not 3PT.", shipment.ManufacturerDocAddress.OrganisationPKInfo);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.ThirdPartyOwnershipHouse;
			shipment.ManufacturerDocAddress.Validation.ValidateOrganisationPK();

			AssertHasError("Shipment should call the validation when the shipment type is 3PT.", shipment.ManufacturerDocAddress.OrganisationPKInfo, "This Organization has no address entered.");

			var org = Factory.New<OrgHeader>();

			shipment.ManufacturerDocAddress.OrganisationPK = org.PK;
			shipment.ManufacturerDocAddress.Validation.ValidateOrganisationPK();
			AssertNoErrors(shipment.ManufacturerDocAddress.OrganisationPKInfo);
		}

		#endregion
	}
}
