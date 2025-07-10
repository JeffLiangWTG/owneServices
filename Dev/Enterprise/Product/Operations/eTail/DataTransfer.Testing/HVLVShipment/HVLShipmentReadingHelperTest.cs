using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLShipmentReadingHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateShipmentDestinationFromPortOfDestination()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();
				Factory.SaveForTesting();

				var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };
				shipmentDataObject.PortOfDestination = new UNLOCO() { Code = "US237", Name = "Great Mills" };

				var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ManifestedVolume = 0,
					Height = 3m,
					Length = 4m,
					Width = 5m,
					OrderReference = "HVI001"
				};

				subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
				shipmentDataObject.SubShipmentCollection.Add(subShipment);

				var newFactory = new UniversalObjectFactory();
				var loadedConsol = newFactory.BOFactory.Load<ForwardingConsol>(consolBO.PK);
				var reader = new ShipmentDataObjectReader(shipmentDataObject, new DummyLogger(), newFactory, ChildShipmentsParent.ToChildShipmentsParent(loadedConsol));
				reader.ReadIntoBusinessObject();
				newFactory.SaveForTesting();

				var importedShipments = Factory.Load<ForwardingShipment>(new ZQuery());
				AssertEquals("1 item has been imported", 1, importedShipments.Length);
				AssertEquals("Destination information should be filled from PortOfDestination", "US237", importedShipments[0].JS_RL_NKDestination);
			}
		}

		public void TestReadConsignmentsFromSubShipments_ShouldCreateConsignmentHeader()
		{
			var consolBO = Factory.NewWithValidTestData<ForwardingConsol>();

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.OH_Code = "BILLTOPARTY";
			var billToPartyContact = billToParty.Contacts.AddNew();
			billToPartyContact.OC_ContactName = "LARRY";

			var shipmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };

			shipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress { AddressType = AddressTypes.SendersLocalClient, OrganizationCode = "BILLTOPARTY", Contact = "LARRY" }
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			shipmentDataObject.SubShipmentCollection.Add(subShipment);

			var reader = new ShipmentDataObjectReader(shipmentDataObject, new DummyLogger(), Factory, ChildShipmentsParent.ToChildShipmentsParent(consolBO));
			reader.ReadIntoBusinessObject();

			var shipment = Factory.Load<ForwardingShipment>(new ZQuery()).Single();

			var consignmentHeaders = Factory.Load<HVLVConsignmentHeader>(new ZQuery());
			AssertEquals("Should create 1 consignment header", 1, consignmentHeaders.Length);
			AssertEquals("Consignment header should link to shipment", shipment.PK, consignmentHeaders[0].HCH_JS_Shipment);

			var consignment = Factory.Load<HVLVConsignment>(new ZQuery()).Single();
			AssertEquals("Consignment should link to consignment header", consignmentHeaders[0].PK, consignment.HVC_HCH_Header);
		}
	}
}
