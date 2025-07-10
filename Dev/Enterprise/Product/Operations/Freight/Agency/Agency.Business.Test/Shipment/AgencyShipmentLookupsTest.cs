using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var shipment = Factory.New<AgencyShipment>();
			AssertEquals("Booked is a valid option", true, shipment.Lookups.JS_ShipmentStatus_List.ContainsCode(ShipmentStatusList.Codes.Booked));
			AssertEquals("WaitListed is a valid option", true, shipment.Lookups.JS_ShipmentStatus_List.ContainsCode(ShipmentStatusList.Codes.WaitListed));
			AssertEquals("Confirmed is NOT a valid option", false, shipment.Lookups.JS_ShipmentStatus_List.ContainsCode(ShipmentStatusList.Codes.Confirmed));
		}

		public void TestStatusListNotConfirmed()
		{
			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("EBK, EBC, BKD, BKX, BKJ, WEB, WTL", new AgencyShipmentStatusList(false).CodesAsString);

				var parameters = new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.New, ShipmentStatusList.Codes.ElectronicBooking),
					new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus)
				};

				var shipment1 = Factory.NewWithValidTestData<AgencyBooking>();
				shipment1.Logs.AddNew(Events.StatusUpdated, parameters);

				Assert(!shipment1.GetIsConfirmed());
				Assert(!shipment1.IsReceivedElectronicBooking());
				AssertEquals("BKD, WTL, WEB", shipment1.Lookups.JS_ShipmentStatus_List.CodesAsString);

				var shipment2 = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();

				Assert(!shipment2.GetIsConfirmed());
				Assert(!shipment2.IsReceivedElectronicBooking());
				AssertEquals("BKD, WTL, WEB", shipment2.Lookups.JS_ShipmentStatus_List.CodesAsString);

				shipment2.IsCancelled = true;
				AssertEquals("BKD, WTL, WEB, BKJ", shipment2.Lookups.JS_ShipmentStatus_List.CodesAsString);

				shipment2.IsCancelled = false;
				shipment2.Logs.AddNew(Events.StatusUpdated, parameters);

				Assert(!shipment2.IsReceivedElectronicBooking());
				AssertEquals("BKD, WTL, WEB", shipment2.Lookups.JS_ShipmentStatus_List.CodesAsString);

				Factory.Save();
				shipment2.Reload();

				Assert(shipment2.IsReceivedElectronicBooking());
				AssertEquals("EBK, BKD, BKJ", shipment2.Lookups.JS_ShipmentStatus_List.CodesAsString);

				var shipment3 = Factory.NewWithValidTestData<AgencyBooking>();
				shipment3.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;

				Factory.Save();
				shipment3.Reload();

				AssertEquals("BKD, WTL, WEB", shipment3.Lookups.JS_ShipmentStatus_List.CodesAsString);

				shipment3.IsCancelled = true;
				AssertEquals("BKD, WTL, WEB, BKJ", shipment3.Lookups.JS_ShipmentStatus_List.CodesAsString);

				var shipment4 = Factory.NewWithValidTestData<AgencyBooking>();
				shipment4.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;

				Factory.Save();
				shipment4.Reload();

				shipment4.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;

				Factory.Save();
				shipment4.Reload();

				AssertEquals(ShipmentStatusList.Codes.WebBooking, shipment4.GetShipmentStatusBeforeLastestEBookingCancellationRequest());

				AssertEquals("BKD, WTL, WEB, EBC", shipment4.Lookups.JS_ShipmentStatus_List.CodesAsString);

				shipment4.JS_ShipmentStatus = ShipmentStatusList.Codes.BookingCancelled;
				AssertEquals("BKD, WTL, WEB, BKX", shipment4.Lookups.JS_ShipmentStatus_List.CodesAsString);

				shipment4.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
				AssertEquals("BKD, WTL, WEB", shipment4.Lookups.JS_ShipmentStatus_List.CodesAsString);

				shipment4.JS_ShipmentStatus = string.Empty;
				AssertEquals("BKD, WTL, WEB", shipment4.Lookups.JS_ShipmentStatus_List.CodesAsString);

				var shipment5 = Factory.NewWithValidTestData<AgencyBooking>();
				shipment5.JS_ShipmentStatus = ShipmentStatusList.Codes.EBookingCancellationRequest;

				AssertEquals("BKD, WTL, WEB", shipment5.Lookups.JS_ShipmentStatus_List.CodesAsString);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.NewWithValidTestData<AgencyBooking>();

				Assert(!shipment.GetIsConfirmed());
				AssertEquals("EBK, BKD, WEB, WTL", shipment.Lookups.JS_ShipmentStatus_List.CodesAsString);
			}
		}

		public void TestStatusListIsConfirmed()
		{
			var parameters = new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.New, ShipmentStatusList.Codes.ElectronicShippingInstruction),
				new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Type, Core.Constants.EventReferenceMessageTypes.ShipmentStatus)
			};

			var shipment = Factory.NewWithValidTestData<AgencyShipment>();		
			shipment.Confirm();		
			Factory.Save();

			shipment.Logs.AddNew(Events.StatusUpdated, parameters);
			Factory.Save();

			Assert(shipment.GetIsConfirmed());
			Assert(shipment.IsReceivedElectronicShippingInstruction());

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("ESI, CNF, WFI", shipment.Lookups.JS_ShipmentStatus_List.CodesAsString);
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("ESI, CNF, SIJ", shipment.Lookups.JS_ShipmentStatus_List.CodesAsString);
			}
		}

		public void TestPaymentTermsList()
		{
			var shipment = Factory.New<AgencyShipment>();
			Assert(shipment.Lookups.JS_INCO_List.ContainsCode(Core.Constants.DomesticPaymentTerms.Collect));
			Assert(shipment.Lookups.JS_INCO_List.ContainsCode(Core.Constants.DomesticPaymentTerms.Prepaid));
			Assert(!shipment.Lookups.JS_INCO_List.ContainsCode(Core.Constants.DomesticPaymentTerms.CollectThirdParty));
		}

		public void TestJS_INCOList()
		{
			var shipment = Factory.New<AgencyShipment>();
			var property = TypeDescriptor.GetProperties(shipment)[JobShipmentSchema.Constants.JS_INCO];
			var list = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(shipment, property);
			const string expected = "CLT - Collect\r\n" + "PPD - Prepaid";
			AssertMultilineASCIIEquals("", expected, list.ElementsAsString);
		}

		public void TestConsignorContacts_List()
		{
			var shipment = Factory.New<AgencyShipment>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org2.Contacts.AddNew();
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			var org1Contacts = (OrgContactDependentCollection)shipment.Lookups.ConsignorContacts_List;
			org1Contacts.Load();
			AssertEquals("Expecting correct contact in consignor 1", contact1.PK, org1.Contacts[0].PK);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			var org2Contacts = (OrgContactDependentCollection)shipment.Lookups.ConsignorContacts_List;
			org2Contacts.Load();
			AssertEquals("Expecting correct contact in consignor 2", contact2.PK, org2.Contacts[0].PK);
			// Expect no exception
			shipment.ConsignorPK = ZGuid.Invalid;
			var orgContactsBodgy = shipment.Lookups.ConsignorContacts_List;
			AssertEquals("No consignor, no contacts", 0, orgContactsBodgy.Count);
		}

		public void TestConsigneeContacts_List()
		{
			var shipment = Factory.New<AgencyShipment>();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org2.Contacts.AddNew();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			var org1Contacts = (OrgContactDependentCollection)shipment.Lookups.ConsigneeContacts_List;
			org1Contacts.Load();
			AssertEquals("Expecting correct contact in Consignee 1", contact1.PK, org1.Contacts[0].PK);
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			var org2Contacts = (OrgContactDependentCollection)shipment.Lookups.ConsigneeContacts_List;
			org2Contacts.Load();
			AssertEquals("Expecting correct contact in Consignee 2", contact2.PK, org2.Contacts[0].PK);
			// Expect no exception
			shipment.ConsigneePK = ZGuid.Invalid;
			var orgContactsBodgy = shipment.Lookups.ConsigneeContacts_List;
			AssertEquals("No Consignee, no contacts", 0, orgContactsBodgy.Count);
		}
	}
}
