using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MessageRecipientPartyTypeListTest : TestCaseWithFactory
	{
		public void TestDescriptions()
		{
			MessageRecipientPartyType partyType =
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.Broker |
				MessageRecipientPartyType.ImportBroker |
				MessageRecipientPartyType.ExportBroker |
				MessageRecipientPartyType.Client |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.None |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.Print |
				MessageRecipientPartyType.ReceivingAgent |
				MessageRecipientPartyType.SendingAgent |
				MessageRecipientPartyType.ControllingAgent |
				MessageRecipientPartyType.ControllingCustomer |
				MessageRecipientPartyType.TransportCo |
				MessageRecipientPartyType.Carrier |
				MessageRecipientPartyType.NotifyParty |
				MessageRecipientPartyType.DeliveryToParty |
				MessageRecipientPartyType.PickupParty |
				MessageRecipientPartyType.EDICommunication |
				MessageRecipientPartyType.InvoiceDebtor |
				MessageRecipientPartyType.DepartureCFS |
				MessageRecipientPartyType.ArrivalCFS |
				MessageRecipientPartyType.Forwarder |
				MessageRecipientPartyType.ArrivalCarrier |
				MessageRecipientPartyType.DepartureCarrier |
				MessageRecipientPartyType.Principal |
				MessageRecipientPartyType.DepartureCTO |
				MessageRecipientPartyType.ArrivalCTO |
				MessageRecipientPartyType.DepartureContainerYard |
				MessageRecipientPartyType.ArrivalContainerYard |
				MessageRecipientPartyType.WarehouseInwards |
				MessageRecipientPartyType.WarehouseOutwards |
				MessageRecipientPartyType.HVLVAirClearanceAgent |
				MessageRecipientPartyType.SGAccess |
				MessageRecipientPartyType.ShippingManager |
				MessageRecipientPartyType.BookingParty |
				MessageRecipientPartyType.DeConsolidator |
				MessageRecipientPartyType.PickupAgent |
				MessageRecipientPartyType.DeliveryAgent |
				MessageRecipientPartyType.CartageAgent |
				MessageRecipientPartyType.JapanCustomsAFR |
				MessageRecipientPartyType.Warehouse |
				MessageRecipientPartyType.DepartureTransitWarehouse |
				MessageRecipientPartyType.ArrivalTransitWarehouse |
				MessageRecipientPartyType.BondedWarehouseInwards |
				MessageRecipientPartyType.BondedWarehouseOutwards |
				MessageRecipientPartyType.BondedWhsChangeOfOwnership |
				MessageRecipientPartyType.BondedWhsChangeOfRegime |
				MessageRecipientPartyType.AutoDocumentDelivery |
				MessageRecipientPartyType.CreditControlledDocumentApproval |
				MessageRecipientPartyType.CarrierBookingAgent |
				MessageRecipientPartyType.NVOCC |
				MessageRecipientPartyType.ExternalBroker |
				MessageRecipientPartyType.HVLVForwarder |
				MessageRecipientPartyType.CarrierMessagingDebtor |
				MessageRecipientPartyType.TransportJobRegistry |
				MessageRecipientPartyType.GlobalTradeManagement |
				MessageRecipientPartyType.ContainerYard |
				MessageRecipientPartyType.GateManagement |
				MessageRecipientPartyType.PortForTransitManifest;

			var list = new MessageRecipientPartyTypeList(partyType);
			AssertEquals("Consignee", list.FindByPartyType(MessageRecipientPartyType.Consignee).Description);
			AssertEquals("Consignor", list.FindByPartyType(MessageRecipientPartyType.Consignor).Description);
			AssertEquals("Broker", list.FindByPartyType(MessageRecipientPartyType.Broker).Description);
			AssertEquals("Import Broker", list.FindByPartyType(MessageRecipientPartyType.ImportBroker).Description);
			AssertEquals("Export Broker", list.FindByPartyType(MessageRecipientPartyType.ExportBroker).Description);
			AssertEquals("Bill to Party(s)", list.FindByPartyType(MessageRecipientPartyType.BillToParty).Description);
			AssertEquals("Pickup Transport", list.FindByPartyType(MessageRecipientPartyType.PickupCartage).Description);
			AssertEquals("Delivery Transport", list.FindByPartyType(MessageRecipientPartyType.DeliveryCartage).Description);
			AssertEquals("Sending Agent", list.FindByPartyType(MessageRecipientPartyType.SendingAgent).Description);
			AssertEquals("Receiving Agent", list.FindByPartyType(MessageRecipientPartyType.ReceivingAgent).Description);
			AssertEquals("Controlling Agent", list.FindByPartyType(MessageRecipientPartyType.ControllingAgent).Description);
			AssertEquals("Controlling Customer", list.FindByPartyType(MessageRecipientPartyType.ControllingCustomer).Description);
			AssertEquals("Organization Proxy", list.FindByPartyType(MessageRecipientPartyType.OrgProxy).Description);
			AssertEquals("Email", list.FindByPartyType(MessageRecipientPartyType.Email).Description);
			AssertEquals("Transport Co", list.FindByPartyType(MessageRecipientPartyType.TransportCo).Description);
			AssertEquals("Carrier", list.FindByPartyType(MessageRecipientPartyType.Carrier).Description);
			AssertEquals("Notify Party", list.FindByPartyType(MessageRecipientPartyType.NotifyParty).Description);
			AssertEquals("Delivery to Party", list.FindByPartyType(MessageRecipientPartyType.DeliveryToParty).Description);
			AssertEquals("Pickup Party", list.FindByPartyType(MessageRecipientPartyType.PickupParty).Description);
			AssertEquals("EDI Communication", list.FindByPartyType(MessageRecipientPartyType.EDICommunication).Description);
			AssertEquals("Invoice Debtor", list.FindByPartyType(MessageRecipientPartyType.InvoiceDebtor).Description);
			AssertEquals("Departure CFS", list.FindByPartyType(MessageRecipientPartyType.DepartureCFS).Description);
			AssertEquals("Arrival CFS", list.FindByPartyType(MessageRecipientPartyType.ArrivalCFS).Description);
			AssertEquals("Forwarder", list.FindByPartyType(MessageRecipientPartyType.Forwarder).Description);
			AssertEquals("Arrival Carrier", list.FindByPartyType(MessageRecipientPartyType.ArrivalCarrier).Description);
			AssertEquals("Departure Carrier", list.FindByPartyType(MessageRecipientPartyType.DepartureCarrier).Description);
			AssertEquals("Principal", list.FindByPartyType(MessageRecipientPartyType.Principal).Description);
			AssertEquals("Departure CTO", list.FindByPartyType(MessageRecipientPartyType.DepartureCTO).Description);
			AssertEquals("Arrival CTO", list.FindByPartyType(MessageRecipientPartyType.ArrivalCTO).Description);
			AssertEquals("Departure Container Yard", list.FindByPartyType(MessageRecipientPartyType.DepartureContainerYard).Description);
			AssertEquals("Arrival Container Yard", list.FindByPartyType(MessageRecipientPartyType.ArrivalContainerYard).Description);
			AssertEquals("Warehouse Inwards", list.FindByPartyType(MessageRecipientPartyType.WarehouseInwards).Description);
			AssertEquals("Warehouse Outwards", list.FindByPartyType(MessageRecipientPartyType.WarehouseOutwards).Description);
			AssertEquals("Bonded Warehouse Inwards", list.FindByPartyType(MessageRecipientPartyType.BondedWarehouseInwards).Description);
			AssertEquals("Bonded Warehouse Outwards", list.FindByPartyType(MessageRecipientPartyType.BondedWarehouseOutwards).Description);
			AssertEquals("HVLV Air Clearance Agent", list.FindByPartyType(MessageRecipientPartyType.HVLVAirClearanceAgent).Description);
			AssertEquals("SG Access", list.FindByPartyType(MessageRecipientPartyType.SGAccess).Description);
			AssertEquals("Shipping Manager", list.FindByPartyType(MessageRecipientPartyType.ShippingManager).Description);
			AssertEquals("Booking Party", list.FindByPartyType(MessageRecipientPartyType.BookingParty).Description);
			AssertEquals("Air Cargo De-Consolidator", list.FindByPartyType(MessageRecipientPartyType.DeConsolidator).Description);
			AssertEquals("Pickup Agent", list.FindByPartyType(MessageRecipientPartyType.PickupAgent).Description);
			AssertEquals("Delivery Agent", list.FindByPartyType(MessageRecipientPartyType.DeliveryAgent).Description);
			AssertEquals("Cartage Agent", list.FindByPartyType(MessageRecipientPartyType.CartageAgent).Description);
			AssertEquals("Japan Customs Advance Filing Rules", list.FindByPartyType(MessageRecipientPartyType.JapanCustomsAFR).Description);
			AssertEquals("Warehouse", list.FindByPartyType(MessageRecipientPartyType.Warehouse).Description);
			AssertEquals("Departure Transit Warehouse", list.FindByPartyType(MessageRecipientPartyType.DepartureTransitWarehouse).Description);
			AssertEquals("Arrival Transit Warehouse", list.FindByPartyType(MessageRecipientPartyType.ArrivalTransitWarehouse).Description);
			AssertEquals("Bonded Warehouse Change of Ownership", list.FindByPartyType(MessageRecipientPartyType.BondedWhsChangeOfOwnership).Description);
			AssertEquals("Bonded Warehouse Change of Regime", list.FindByPartyType(MessageRecipientPartyType.BondedWhsChangeOfRegime).Description);
			//AssertEquals("Client", List.FindByPartyType(MessageRecipientPartyType.Client).Description);
			AssertEquals("Auto-Deliver using Document Config", list.FindByPartyType(MessageRecipientPartyType.AutoDocumentDelivery).Description);
			AssertEquals("Credit Controlled Document Approval", list.FindByPartyType(MessageRecipientPartyType.CreditControlledDocumentApproval).Description);
			AssertEquals("Carrier Booking Agent", list.FindByPartyType(MessageRecipientPartyType.CarrierBookingAgent).Description);
			AssertEquals("NVOCC", list.FindByPartyType(MessageRecipientPartyType.NVOCC).Description);
			AssertEquals("External Broker", list.FindByPartyType(MessageRecipientPartyType.ExternalBroker).Description);
			AssertEquals("HVLV Forwarder", list.FindByPartyType(MessageRecipientPartyType.HVLVForwarder).Description);
			AssertEquals("Carrier Messaging Debtor", list.FindByPartyType(MessageRecipientPartyType.CarrierMessagingDebtor).Description);
			AssertEquals("PT/TB Registry", list.FindByPartyType(MessageRecipientPartyType.TransportJobRegistry).Description);
			AssertEquals("Global Trade Management", list.FindByPartyType(MessageRecipientPartyType.GlobalTradeManagement).Description);
			AssertEquals("Container Yard", list.FindByPartyType(MessageRecipientPartyType.ContainerYard).Description);
			AssertEquals("Gate Management", list.FindByPartyType(MessageRecipientPartyType.GateManagement).Description);
			AssertEquals("Port For Transit Manifest", list.FindByPartyType(MessageRecipientPartyType.PortForTransitManifest).Description);
		}

		public void TestGetPartyTypes()
		{
			List[0].Value = true;
			List[1].Value = true;
			AssertEquals(EnumValue1 | EnumValue2, List.PartyTypes);

			List[0].Value = false;
			List[1].Value = true;
			AssertEquals(EnumValue2, List.PartyTypes);
		}

		public void TestSetPartyTypes()
		{
			List.PartyTypes = EnumValue1 | EnumValue2;
			AssertEquals(EnumValue1 | EnumValue2, List.PartyTypes);
			AssertEquals(true, List[0].Value);
			AssertEquals(true, List[1].Value);

			List.PartyTypes = EnumValue2;
			AssertEquals(EnumValue2, List.PartyTypes);
			AssertEquals(false, List[0].Value);
			AssertEquals(true, List[1].Value);
		}

		public void TestPartyTypesChanged()
		{
			bool partyTypesChangedFired = false;
			List.PartyTypesChanged += delegate
			{ partyTypesChangedFired = true; };

			List.PartyTypes = EnumValue1 | EnumValue2;
			AssertEquals(true, partyTypesChangedFired);
			partyTypesChangedFired = false;

			List.PartyTypes = EnumValue1 | EnumValue2;
			AssertEquals(false, partyTypesChangedFired);
			partyTypesChangedFired = false;

			List.PartyTypes = EnumValue2;
			AssertEquals(true, partyTypesChangedFired);
			partyTypesChangedFired = false;

			List.PartyTypes = EnumValue1;
			AssertEquals(true, partyTypesChangedFired);
		}

		#region Implementation

		MessageRecipientPartyTypeList List
		{
			get
			{
				if (list == null)
				{
					list = new MessageRecipientPartyTypeList(EnumValue1 | EnumValue2);
				}
				return list;
			}
		}
		MessageRecipientPartyTypeList list;

		static readonly MessageRecipientPartyType EnumValue1 = MessageRecipientPartyType.Consignee;
		static readonly MessageRecipientPartyType EnumValue2 = MessageRecipientPartyType.SendingAgent;

		#endregion
	}
}
