using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.HouseBill
{
	sealed class BillOfLadingFIATAIntegrationTest : BillOfLadingTemplateBaseTest
	{
		public override string TemplateName
		{
			get
			{
				return "BillOfLadingFIATA";
			}
		}

		public override ForwardingShipment GetNewShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_HouseBillOfLadingType = Constants.HouseBillOfLadingTypes.Code.FIATAHBL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_NoOriginalBills = 2;
			shipment.JS_NoCopyBills = 3;

			var departureConsol = shipment.Consols.AddNew();
			departureConsol.JK_UniqueConsignRef = "CONSOL0001";
			departureConsol.JK_TransportMode = "SEA";
			departureConsol.JK_RL_NKLoadPort = "AUSYD";
			departureConsol.JK_RL_NKDischargePort = "NZAKL";
			departureConsol.JK_BookingReference = "BookingRef";
			departureConsol.JK_CoLoadBookingReference = "CoLoadBookingRef";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "MAERSK";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit 13";
			shipper.MainAddress.Address2 = "4 Lost Lane";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2000";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "DUMMY";
			consignee.OH_RL_NKClosestPort = "NZAKL";
			consignee.MainAddress.Address1 = "Unit 1";
			consignee.MainAddress.Address2 = "4 What Lane";
			consignee.MainAddress.City = "Auckland";
			consignee.MainAddress.Postcode = "5022";
			consignee.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			var sendingForwarderContact = sendingForwarder.Contacts.AddNew();
			sendingForwarderContact.OC_Email = "test1@test.com";
			sendingForwarderContact.OC_Phone = "11111";
			sendingForwarderContact.OC_ContactName = "ContactName1";

			departureConsol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "JIMMY";
			receivingForwarder.OH_RL_NKClosestPort = "NZAKL";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Auckland";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NZ";
			receivingForwarder.MainAddress.OA_Phone = "23333";

			var receivingForwarderContact = receivingForwarder.Contacts.AddNew();
			receivingForwarderContact.OC_Email = "test2@test.com";
			receivingForwarderContact.OC_Phone = "22222";
			receivingForwarderContact.OC_ContactName = "ContactName2";

			departureConsol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "FUNNY";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			Factory.Save();
			return shipment;
		}

		const string Content =
@"[2,4] Consignor
[2,32] S00001000
[2,40] Country Code
[3,6] MAERSK
UNIT 13
4 LOST LANE
SYDNEY NSW 2000
AUSTRALIA
[4,40] AU
[10,5] Consigned to order of
[11,6] DUMMY
UNIT 1
4 WHAT LANE
AUCKLAND 5022
NEW ZEALAND
[15,5] Notify Address
[16,6] FUNNY
UNIT 888
8 WHAT LANE
AUCKLAND 5012
NEW ZEALAND
[18,18] Place of Receipt
[19,18] SYDNEY, AUSTRALIA
[20,6] Ocean Vessel
[20,18] Port of loading
[21,6]  /
[21,18] SYDNEY, AUSTRALIA
[22,6] Port of discharge
[22,18] Place of Delivery
[22,27] ORIGINAL
[23,6] AUCKLAND, NEW ZEALAND
[23,18] AUCKLAND, NEW ZEALAND
[24,6] Marks and Numbers
[24,17] Number and Kind of Packages
[24,27] Description of Goods
[24,33] Gross Weight
[24,37] Measurement
[25,22] 0 (s)
[25,33] 0.000 KG
[25,36] 0.000 M3
[32,6] CAN: 
[32,20] Consol Ref: CONSOL0001
[33,6] Total Packages:
[33,16] ZERO (S)
[34,6] SHIPPED ON BOARD 
[34,20] according to the declaration of the consignor
[36,6] Declaration of Interest of the consignor in
timely delivery (Clause 6.2.)
[36,29] Declared value for ad valorem rate according to
the declaration of the consignor (Clauses 7 and 8)
[39,6] The goods and instructions are accepted and dealt with subject to the Standard Conditions printed overleaf.

Taken in charge in apparent good order and condition, unless otherwise noted herein, at the place of receipt for transport and delivery as mentioned above.

One of these Multimodal Transport Bills of Lading must be surrendered duly endorsed in exchange for the goods. In Witness whereof the original Multimodal
Transport Bills of Lading and all of this tenor and date have been signed in the number stated below, one of which being accomplished the other(s) to be void.
[41,6] Freight amount
[41,22] Freight Payable at
[41,30] Place and date of issue
[42,6] FREIGHT COLLECT
[42,22] AUCKLAND, NEW ZEALAND
[42,30] BRISBANE, AUSTRALIA
[43,6] Cargo Insurance through the undersigned
[43,22] No. of Originals
[43,30] Stamp and Signature
[44,9] Not Covered
[44,15] Covered according to attached Policy
[44,22] 3 (THREE)
[44,30] AS CARRIER
[48,6] For delivery of goods please apply to:
[50,6] JIMMY
UNIT 399
50 WHAT LANE
AUCKLAND 5023
NEW ZEALAND
[54,6] Phone: 23333
[55,6] HBoL-FIATA";

		[TestDate(2022, 9, 23)]
		public override void TestDocumentContent()
		{
			var shipment = GetNewShipment();

			var menuItemPK = new ZGuid("e7701494-3e29-41a6-8c24-1457cedb8244");
			var templatePK = new ZGuid("40e652ce-ea13-4897-bbad-a4954921b1f3");

			using (RawDataRegistry.Instance.UseFormBuilderHouseBills.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, menuItemPK, "Original", 0, Content, TemplateName, templatePK);
			}
		}
	}
}
