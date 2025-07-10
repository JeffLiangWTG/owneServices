using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.CN.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using ShippingLineMessagingRequirement = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.ShippingLineMessagingRequirement;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class ShippingInstructionBuilderTest : CarrierMessageDataBuilderTest
	{
		#region TestPopulateFromNewConsolDoesNotThrowException

		public void TestPopulateFromNewConsolDoesNotThrowException()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertNoExceptionThrown(() => new ShippingInstructionBuilder(consol).Build());
		}

		#endregion

		#region TestPopulateFromForwardingConsol

		[TestDate(2018, 1, 1)]
		public void TestPopulateFromForwardingConsol()
		{
			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = CreateConsol();
				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

				CombineAssertions(() =>
				{
					AssertEquals("SourceType", "ForwardingConsol", shippingInstruction.SourceType);
					AssertEquals("SourceID", "C00001001", shippingInstruction.SourceID);
					AssertEquals("DocumentName", "ShippingInstruction", shippingInstruction.DocumentName);

					AssertEquals("ContainerMode", "FCL", shippingInstruction.ContainerMode.Code);
					AssertEquals("BookingReference", "驴100", shippingInstruction.BookingReference);
					AssertEquals("NumberOfOriginals", 1, shippingInstruction.NumberOfOriginals);
					AssertEquals("NumberOfCopies", 3, shippingInstruction.NumberOfCopies);
					AssertEquals("DateOfIssue", new ZDateTime(2018, 10, 1), shippingInstruction.DateOfIssue);
					AssertEquals("IsDoorPickup", true, shippingInstruction.IsDoorPickup);
					AssertEquals("IsDoorDelivery", false, shippingInstruction.IsDoorDelivery);
					AssertEquals("ReleaseType", "BOL", shippingInstruction.ReleaseType.Code);

					AssertEquals("Origin.Code", "CNSHA", shippingInstruction.Origin.Code);
					AssertEquals("Origin.Description", "Shanghai Hongqiao International Apt", shippingInstruction.Origin.Name);

					AssertEquals("Destination.Code", "AUSYD", shippingInstruction.Destination.Code);
					AssertEquals("Destination.Description", "Sydney", shippingInstruction.Destination.Name);

					AssertEquals("PlaceOfReceipt.Code", "CNSHA", shippingInstruction.PlaceOfReceipt.Code);
					AssertEquals("PlaceOfReceipt.Description", "Shanghai Hongqiao International Apt", shippingInstruction.PlaceOfReceipt.Name);

					AssertEquals("PlaceOfDelivery.Code", "AUSYD", shippingInstruction.PlaceOfDelivery.Code);
					AssertEquals("PlaceOfDelivery.Description", "Sydney", shippingInstruction.PlaceOfDelivery.Name);

					AssertEquals("PlaceOfIssue.Code", "DKAAL", shippingInstruction.PlaceOfIssue.Code);
					AssertEquals("PlaceOfIssue.Description", "Aalborg", shippingInstruction.PlaceOfIssue.Name);

					AssertTransports(shippingInstruction);
					AssertAddresses(shippingInstruction);
				});
			}
		}

		void AssertTransports(CarrierMessageData shippingInstruction)
		{
			AssertContainsExactElementsInAnyOrder("Transports",
				new[]
				{
					"CNSHA -> SGSIN",
					"SGSIN -> NZAKL",
					"NZAKL -> AUSYD"
				},
				shippingInstruction.Transports.Select(t => $"{t.PortOfLoading.Code} -> {t.PortOfDischarge.Code}"));
		}

		void AssertAddresses(CarrierMessageData shippingInstruction)
		{
			var addresses = new[]
			{
				$"{nameof(shippingInstruction.Shipper)}\r\n{shippingInstruction.Shipper.ToAssertString()}",
				$"{nameof(shippingInstruction.Carrier)}\r\n{shippingInstruction.Carrier.ToAssertString()}",
				$"{nameof(shippingInstruction.Creditor)}\r\n{shippingInstruction.Creditor.ToAssertString()}",
				$"{nameof(shippingInstruction.Consignee)}\r\n{shippingInstruction.Consignee.ToAssertString()}",
				$"{nameof(shippingInstruction.NotifyParty)}\r\n{shippingInstruction.NotifyParty.ToAssertString()}",
				$"{nameof(shippingInstruction.NotifyParty2)}\r\n{shippingInstruction.NotifyParty2.ToAssertString()}",
				$"{nameof(shippingInstruction.NotifyParty3)}\r\n{shippingInstruction.NotifyParty3.ToAssertString()}",
				$"{nameof(shippingInstruction.Forwarder)}\r\n{shippingInstruction.Forwarder.ToAssertString()}",
				$"{nameof(shippingInstruction.Buyer)}\r\n{shippingInstruction.Buyer.ToAssertString()}",
				$"{nameof(shippingInstruction.FreightPayer)}\r\n{shippingInstruction.FreightPayer.ToAssertString()}",
				$"{nameof(shippingInstruction.PickupFrom)}\r\n{shippingInstruction.PickupFrom.ToAssertString()}",
				$"{nameof(shippingInstruction.DeliverTo)}\r\n{shippingInstruction.DeliverTo.ToAssertString()}",
				$"{nameof(shippingInstruction.Recipient)}\r\n{shippingInstruction.Recipient.ToAssertString()}"
			};

			AssertContainsExactElementsInAnyOrder("Addresses",
				new[]
				{
@"Shipper
I'm Sending Stuff
Unit 200
55 Why Lane
Sender Name
name@sender.com
1111111
2222222",

@"Carrier
MAERSK
Unit 13
4 Lost Lane",

@"Consignee
I'm Receiving Stuff
Unit 399
50 What Lane
Receiver Name
name@receiver.com
3333333
4444444",

@"Forwarder
I'm Sending Stuff
Unit 200
55 Why Lane
Sender Name
name@sender.com
1111111
2222222",

"Buyer",

@"Creditor
I'm The Money
Cashed up
1 Moolah St",

"FreightPayer",

@"NotifyParty
Stay in touch
Unit 205
128 Why Lane
stayintouch@test.com",

@"NotifyParty2
I'm Notifying About Stuff
Unit 2
60 What Lane",

@"NotifyParty3
Notifying 3
Unit 2
60 What Kine",

@"Recipient
MAERSK
Unit 13
4 Lost Lane",

@"PickupFrom
CONSPA
Unit 15
5 Lost Lane
aaa@test.com
12344
222",

@"DeliverTo
BLOOP
199 Crab Road
Crabby"
				},
				addresses.Select(address => address.TrimEnd()));
		}

		#endregion

		#region Populate Shipment Buyer

		public void TestPopulateShipmentBuyer()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_PackageGrouping = PackageGrouping.Codes.GroupByPackLine;

			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.CompanyName = "Test1 Name";

			var org2 = Factory.New<OrgHeader>();
			org2.MainAddress.CompanyName = "Test2 Name";

			var coLoadMasterShipment = Factory.New<ForwardingShipment>();
			coLoadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			coLoadMasterShipment.BuyerDocAddress.E2_OA_Address = org2.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.BuyerDocAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment.JS_JS_ColoadMasterShipment = coLoadMasterShipment.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO = carrierMessageData.Shipments.Cast<Shipment>().FirstOrDefault();
				AssertNotNull(shipmentDO.Buyer);
				AssertEquals(org2.MainAddress.CompanyName, shipmentDO.Buyer.CompanyName);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO = carrierMessageData.Shipments.Cast<Shipment>().FirstOrDefault();
				AssertNull(shipmentDO.Buyer);
			}
		}

		public void TestPopulateShipmentBuyer_PackageGrouping()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.CompanyName = "Test1 Name";
			shipment.BuyerDocAddress.E2_OA_Address = org1.MainAddress.PK;

			AssertShipmentBuyer(PackageGrouping.Codes.DoNotGroup);
			AssertShipmentBuyer(PackageGrouping.Codes.GroupByPackLine);
			AssertShipmentBuyer(PackageGrouping.Codes.GroupByShipment);

			void AssertShipmentBuyer(ZString packageGrouping)
			{
				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					consol.JK_PackageGrouping = packageGrouping;

					var builder = CreateDocDataObjectBuilder(consol);
					var carrierMessageData = builder.Build();

					var shipmentDO = carrierMessageData.Shipments.Cast<Shipment>().FirstOrDefault();
					AssertNotNull(shipmentDO.Buyer);
					AssertEquals(org1.MainAddress.CompanyName, shipmentDO.Buyer.CompanyName);
				}
			}
		}

		#endregion

		#region Populate Shipment Supplier

		public void TestPopulateShipmentSupplier()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_PackageGrouping = PackageGrouping.Codes.GroupByPackLine;

			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.CompanyName = "Test1 Name";

			var org2 = Factory.New<OrgHeader>();
			org2.MainAddress.CompanyName = "Test2 Name";

			var coLoadMasterShipment = Factory.New<ForwardingShipment>();
			coLoadMasterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			coLoadMasterShipment.SupplierDocAddress.E2_OA_Address = org2.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.SupplierDocAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment.JS_JS_ColoadMasterShipment = coLoadMasterShipment.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO = carrierMessageData.Shipments.Cast<Shipment>().FirstOrDefault();
				AssertNotNull(shipmentDO.Supplier);
				AssertEquals(org2.MainAddress.CompanyName, shipmentDO.Supplier.CompanyName);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO = carrierMessageData.Shipments.Cast<Shipment>().FirstOrDefault();
				AssertNull(shipmentDO.Supplier);
			}
		}

		public void TestPopulateShipmentSupplier_PackageGrouping()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			var org1 = Factory.New<OrgHeader>();
			org1.MainAddress.CompanyName = "Test1 Name";
			shipment.SupplierDocAddress.E2_OA_Address = org1.MainAddress.PK;

			AssertShipmentSupplier(PackageGrouping.Codes.DoNotGroup);
			AssertShipmentSupplier(PackageGrouping.Codes.GroupByPackLine);
			AssertShipmentSupplier(PackageGrouping.Codes.GroupByShipment);

			void AssertShipmentSupplier(ZString packageGrouping)
			{
				using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					consol.JK_PackageGrouping = packageGrouping;

					var builder = CreateDocDataObjectBuilder(consol);
					var carrierMessageData = builder.Build();

					var shipmentDO = carrierMessageData.Shipments.Cast<Shipment>().FirstOrDefault();
					AssertNotNull(shipmentDO.Supplier);
					AssertEquals(org1.MainAddress.CompanyName, shipmentDO.Supplier.CompanyName);
				}
			}
		}

		#endregion

		#region EORINumber

		public void TestPopulateICS2DeclarantEORINumber()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";
			var eori = org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DE12345a", Core.Constants.CountryCodes.Germany);

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NO!2#";

			var existingRelationShip = Factory.New<OrgRelatedParty>();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_Parent = org1.PK;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			Factory.Save();

			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);
			selfFiler.E2_OA_Address = org1.MainAddress.PK;

			AssertNotNull(selfFiler);
			Assert(consol.IsICS2);

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert(wrapper.IsShowICS2);
			AssertEquals("DE12345A", wrapper.ICS2DeclarantEORINumber);

			eori.OK_CustomsRegNo = "12345b";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert(wrapper.IsShowICS2);
			AssertEquals("DE12345B", wrapper.ICS2DeclarantEORINumber);

			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			eori.OK_CustomsRegNo = "xI12345b";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert(wrapper.IsShowICS2);
			AssertEquals("XI12345B", wrapper.ICS2DeclarantEORINumber);
		}

		public void TestGetTaxNumber_EORINumber()
		{
			PopulateRefData();
			var consol = CreateConsol("NLRTM", "BEANR");

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "NLRTM";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NL";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			receivingForwarder.OH_RL_NKClosestPort = "GBEDI";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "GB";
			consol.JK_RL_NKDischargePort = "GBEDI";
			consol.Transports[2].JW_RL_NKDiscPort = "GBEDI";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			consol.ReceivingForwarder.CustomsCodes.RemoveAll();
			var consingeeEOR = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consingeeEOR.OK_RN_NKCodeCountry = "GB";
			consingeeEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			consingeeEOR.OK_CustomsRegNo = "12345a";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GB12345A", wrapper.ConsigneeTaxInfo1.Number);

			consingeeEOR.OK_CustomsRegNo = "GB12345b";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GB12345B", wrapper.ConsigneeTaxInfo1.Number);

			consingeeEOR.OK_CustomsRegNo = "Xi12345b";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("XI12345B", wrapper.ConsigneeTaxInfo1.Number);
		}

		#endregion

		#region Validation

		#region GroupITNNumber

		public void TestValidateGroupITNNumber()
		{
			const string errorMessageEmptyGroupITNNumberAndGroupPOFNumber = "SHIPMENT1: ITN (Internal Transaction Number) is required for AES (Automated Export System) reporting for exports from the United States or its overseas territories. Enter the ITN in the Shipment > Entry Details field.";
			const string warningMessageInvalidGroupITNNumber = "Please enter valid ITN number(s), the number must start with the letter \"X\", \r\nfollowed by the year, month and day of acceptance in the AES, and six randomly assigned digits.";
			const string errorMessageForExceedLength = "Number with over 35 characters will not be accepted by the carrier.";

			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");

				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));
				var shipment1GroupedPackingLine2 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));

				AssertHasMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
				AssertHasMessageError(shipment1GroupedPackingLine2.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
			}

			var shipment1CusEntryNumber = shipment1.CusEntryNumbers.AddNew();
			shipment1CusEntryNumber.CE_EntryType = Customs.Common.US.CusEntryNumberTypeList.Codes.ITN;
			shipment1CusEntryNumber.CE_EntryNum = "X20230316311995";

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");

				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));
				var shipment1GroupedPackingLine2 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));

				AssertNoWarning(shipment1GroupedPackingLine1.GroupITNNumberInfo, warningMessageInvalidGroupITNNumber);
				AssertNoWarning(shipment1GroupedPackingLine2.GroupITNNumberInfo, warningMessageInvalidGroupITNNumber);

				AssertNoMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
				AssertNoMessageError(shipment1GroupedPackingLine2.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);

				AssertNoMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageForExceedLength);
				AssertNoMessageError(shipment1GroupedPackingLine2.GroupITNNumberInfo, errorMessageForExceedLength);

				shipment1GroupedPackingLine1.GroupITNNumber = "InvalidNumber";
				AssertNoMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageForExceedLength);
				AssertHasWarning(shipment1GroupedPackingLine1.GroupITNNumberInfo, warningMessageInvalidGroupITNNumber);

				shipment1GroupedPackingLine1.GroupITNNumber = "X20221212139314";
				AssertNoMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
				AssertNoWarning(shipment1GroupedPackingLine1.GroupITNNumberInfo, warningMessageInvalidGroupITNNumber);
				AssertNoMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageForExceedLength);

				shipment1GroupedPackingLine1.GroupITNNumber = "ExceedTheLength11111111111111111111111111111111111";
				AssertNoMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
				AssertNoWarning(shipment1GroupedPackingLine1.GroupITNNumberInfo, warningMessageInvalidGroupITNNumber);
				AssertHasMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageForExceedLength);
			}
		}

		public void TestValidateGroupITNNumber_ForSubShipmentsInAssemblyShipment()
		{
			const string errorMessageEmptyGroupITNNumberAndGroupPOFNumber = "SHIPMENT1: ITN (Internal Transaction Number) is required for AES (Automated Export System) reporting for exports from the United States or its overseas territories. Enter the ITN in the Shipment > Entry Details field.";

			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var assemblyMaster = consol.Shipments.AddNew();
			assemblyMaster.JS_UniqueConsignRef = "ASMSHIPMENT";
			assemblyMaster.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var shipment1 = assemblyMaster.CoLoadShipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>()
					.FirstOrDefault(x => x.ShipmentID == "ASMSHIPMENT")?.Shipments.Cast<Shipment>()
					.FirstOrDefault(x => x.ShipmentID == "SHIPMENT1");
				AssertNotNull(shipmentDO1);

				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));
				var shipment1GroupedPackingLine2 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));

				AssertHasMessageError(shipment1GroupedPackingLine1.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
				AssertHasMessageError(shipment1GroupedPackingLine2.GroupITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>()
					.FirstOrDefault(x => x.ShipmentID == "ASMSHIPMENT")?.Shipments.Cast<Shipment>()
					.FirstOrDefault(x => x.ShipmentID == "SHIPMENT1");
				AssertNotNull(shipmentDO1);

				AssertHasMessageError(shipmentDO1.ITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
				AssertHasMessageError(shipmentDO1.ITNNumberInfo, errorMessageEmptyGroupITNNumberAndGroupPOFNumber);
			}
		}

		public void TestValidateGroupITNNumberWarning()
		{
			const string warningMessage = "When both the ITN and Export Statement are entered, only the ITN information will be transmitted to the carrier via electronic messaging.";

			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "USCHI";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment1.DocsAndCartage.JP_ExportStatement = "LOW";

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var shipment1CusEntryNumber = shipment1.CusEntryNumbers.AddNew();
			shipment1CusEntryNumber.CE_EntryType = Customs.Common.US.CusEntryNumberTypeList.Codes.ITN;
			shipment1CusEntryNumber.CE_EntryNum = "X20230316311995";

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));

				AssertEquals("X20230316311995", shipment1GroupedPackingLine1.GroupITNNumber);
				AssertEquals("NOEEI §30.37(a)", shipment1GroupedPackingLine1.GroupPOFNumber);

				AssertHasWarning(shipment1GroupedPackingLine1.GroupITNNumberInfo, warningMessage);
			}
		}

		#endregion

		#region GroupCTKNumber

		public void TestValidateGroupCTKNumber()
		{
			const string warningMessage = @"The CTK - Cargo Tracking Note number is required for cargo destined to Ghana.
Please enter CTK in either Consol > Details > Numbers > Reference Numbers,
Or in Shipment (SHIPMENT1) > Additional Details > Reference Numbers.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "GHACC";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKDestination = "GHACC";
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));

				AssertHasWarning(shipment1GroupedPackingLine1.GroupCTKNumberInfo, warningMessage);

				var ctkNumber = shipment1.Numbers.AddNew();
				ctkNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
				ctkNumber.CE_EntryNum = "0001";
				ctkNumber.CE_RN_NKCountryCode = "GH";

				builder = CreateDocDataObjectBuilder(consol);
				carrierMessageData = builder.Build();
				shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));

				AssertNoWarning(shipment1GroupedPackingLine1.GroupCTKNumberInfo, warningMessage);
			}
		}

		#endregion

		#region Addresses

		#region Consignee

		public void TestUltimateConsignee()
		{
			var mandatoryMessage = "Ultimate consignee is required for Master Bill for destination of Australia.";
			var verifyMessage = "Ultimate consignee might be required for Master Bill for destination of Australia - please check with the Carrier or local authorities at destination.";

			var consol = CreateConsol();
			var shipment1 = consol.Shipments[0];
			var shipment2 = consol.Shipments.AddNew();

			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_FullName = "CONSPA";
			consignee1.OH_RL_NKClosestPort = "AUSYD";
			consignee1.MainAddress.Address1 = "Unit 15";
			consignee1.MainAddress.Address2 = "5 Lost Lane";
			consignee1.MainAddress.City = "Sydney";
			consignee1.MainAddress.Postcode = "2000";
			consignee1.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = consignee1.MainAddress.PK;

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_FullName = "CONSPA2";
			consignee2.OH_RL_NKClosestPort = "AUSYD";
			consignee2.MainAddress.Address1 = "Unit 35";
			consignee2.MainAddress.Address2 = "25 Lost Lane";
			consignee2.MainAddress.City = "Sydney";
			consignee2.MainAddress.Postcode = "2000";
			consignee2.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = consignee2.MainAddress.PK;

			var ultimateConsigneeRule = consol.DischargePort.Country.Rules.AddNew();
			ultimateConsigneeRule.R7_RN_NKDestination = consol.DischargePort.RL_RN_NKCountryCode;
			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.NotRequired;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Ultimate Consignee not required - use Receiving Forwarder", consol.ReceivingForwarder.OH_FullName, shippingInstruction.Consignee.CompanyName);
			AssertNoMessageError(shippingInstruction.Consignee.CompanyNameInfo, mandatoryMessage);
			AssertNoWarning(shippingInstruction.Consignee.CompanyNameInfo, verifyMessage);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.Mandatory;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Ultimate Consignee cannot be calculated - use Receiving Forwarder", consol.ReceivingForwarder.OH_FullName, shippingInstruction.Consignee.CompanyName);
			AssertHasMessageError(shippingInstruction.Consignee.CompanyNameInfo, mandatoryMessage);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Ultimate Consignee cannot be calculated - use Receiving Forwarder", consol.ReceivingForwarder.OH_FullName, shippingInstruction.Consignee.CompanyName);
			AssertHasWarning(shippingInstruction.Consignee.CompanyNameInfo, verifyMessage);

			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = consignee1.MainAddress.PK;
			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.NotRequired;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Ultimate Consignee not required - use Receiving Forwarder", consol.ReceivingForwarder.OH_FullName, shippingInstruction.Consignee.CompanyName);
			AssertNoMessageError(shippingInstruction.Consignee.CompanyNameInfo, mandatoryMessage);
			AssertNoWarning(shippingInstruction.Consignee.CompanyNameInfo, verifyMessage);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Ultimate Consignee from shipment as all the same", shipment1.ConsigneeDocumentaryAddress.CompanyName, shippingInstruction.Consignee.CompanyName);
			AssertNoWarning(shippingInstruction.Consignee.CompanyNameInfo, verifyMessage);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.Mandatory;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Ultimate Consignee from shipment as all the same", shipment1.ConsigneeDocumentaryAddress.CompanyName, shippingInstruction.Consignee.CompanyName);
			AssertNoMessageError(shippingInstruction.Consignee.CompanyNameInfo, mandatoryMessage);

			shippingInstruction.Consignee.CompanyName = "TO ORDER";
			AssertHasMessageError(shippingInstruction.Consignee.CompanyNameInfo, mandatoryMessage);

			shippingInstruction.Consignee.CompanyName = "SOMEONE COMPLETELY DIFFERENT";
			AssertHasWarning(shippingInstruction.Consignee.CompanyNameInfo, mandatoryMessage);
		}

		public void TestConsigneeCountryValidation()
		{
			var consol = CreateConsol();
			consol.Transports[2].JW_RL_NKDiscPort = "BRSAO";

			var consigneeAddressPK = consol.ReceivingForwarderAddress.PK;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Precondition", "Australia", shippingInstruction.Consignee.Country.Name);
			AssertHasMessageError(shippingInstruction.Consignee.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			shippingInstruction.Consignee.Country.Name = "Brazil";
			AssertNoMessageError(shippingInstruction.Consignee.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			shippingInstruction.Consignee.Country.Name = "Guatamala";
			AssertHasMessageError(shippingInstruction.Consignee.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			shippingInstruction.NotifyParty.Country.Name = "Brazil";
			AssertNoMessageError(shippingInstruction.Consignee.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			consol.Shipments[0].JS_RL_NKDestination = "CNSHA";
			consol.Transports[2].JW_RL_NKDiscPort = "CNSHA";
			consol.JK_OA_ReceivingForwarderAddress = consigneeAddressPK;

			var chinaTaxNumber = consol.ReceivingForwarder.CustomsCodes.AddNew();
			chinaTaxNumber.OK_RN_NKCodeCountry = "CN";
			chinaTaxNumber.OK_CodeType = "NGB";
			chinaTaxNumber.OK_CustomsRegNo = "12345";

			var australiaTaxNumber = consol.ReceivingForwarder.CustomsCodes.AddNew();
			australiaTaxNumber.OK_RN_NKCodeCountry = "AU";
			australiaTaxNumber.OK_CodeType = "ABN";
			australiaTaxNumber.OK_CustomsRegNo = "54321";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(shippingInstruction.Consignee.Country.NameInfo, "Consignee's country is different from Destination country.");

			shippingInstruction.Consignee.Country.Name = "CHINA";
			AssertNoWarning(shippingInstruction.Consignee.Country.NameInfo, "Consignee's country is different from Destination country.");
		}

		public void TestConsigneeAndNotifyPartyContactValidation()
		{
			const string expectedError = "Please enter at least one communication method (phone or email) for the Consignee or Notify Party.";

			var consol = CreateConsol();
			consol.Transports[0].JW_RL_NKLoadPort = "CNSHA";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			wrapper.Consignee.Phone = "";
			wrapper.Consignee.Email = "";
			wrapper.NotifyParty.Phone = "";
			wrapper.NotifyParty.Email = "email@test.com";

			AssertNoMessageError(wrapper.Consignee.ContactInfo, expectedError);
			AssertNoMessageError(wrapper.NotifyParty.ContactInfo, expectedError);

			wrapper.NotifyParty.Email = "";

			AssertHasMessageError(wrapper.Consignee.ContactInfo, expectedError);
			AssertHasMessageError(wrapper.NotifyParty.ContactInfo, expectedError);

			wrapper.Consignee.Phone = "012393939";

			AssertNoMessageError(wrapper.Consignee.ContactInfo, expectedError);
			AssertNoMessageError(wrapper.NotifyParty.ContactInfo, expectedError);
		}

		public void TestConsigneePostcodeValidation()
		{
			var consol = CreateConsol();

			consol.Transports[2].JW_RL_NKLoadPort = "AUBNE";
			consol.Transports[2].JW_RL_NKDiscPort = "USNYC";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("Precondition", !wrapper.Consignee.Postcode.IsEmpty);
			AssertNoMessageError(wrapper.Consignee.PostcodeInfo, "Consignee's postcode is required for US imports.");

			wrapper.Consignee.Postcode = "";
			AssertHasMessageError(wrapper.Consignee.PostcodeInfo, "Consignee's postcode is required for US imports.");

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoMessageError(wrapper.Consignee.PostcodeInfo, "Consignee's postcode is required for US imports.");

			consol.Transports[2].JW_RL_NKDiscPort = "CAVAN";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(wrapper.Consignee.PostcodeInfo, "Consignee's postcode is required for Canadian imports.");

			wrapper.Consignee.Postcode = "";
			AssertHasMessageError(wrapper.Consignee.PostcodeInfo, "Consignee's postcode is required for Canadian imports.");

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoMessageError(wrapper.Consignee.PostcodeInfo, "Consignee's postcode is required for Canadian imports.");
		}

		public void TestConsigneeValidation_ToOrder()
		{
			var consol = CreateConsol();
			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var consignee = wrapper.Consignee;

			AssertAddressToOrder("Consignee", consignee, false);
		}

		[TestDate(2021, 10, 1)]
		public void TestConsigneeValidation_AddressValidation_WhenConsolRoutingLegIsInIndia()
		{
			var messageErrorForRequireFullAddressForIndia = "Full address is required to comply with SCMTR Manifest reporting for India.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = CreateConsol();
				consol.Transports[2].JW_RL_NKLoadPort = "CNSHA";
				consol.Transports[2].JW_RL_NKDiscPort = "IN5PA";

				var wrapper = new ShippingInstructionBuilder(consol).Build();
				var consignee = wrapper.Consignee;

				consignee.Country.Code = string.Empty;
				consignee.Postcode = string.Empty;
				consignee.City = string.Empty;

				AssertHasMessageError(consignee.Country.NameInfo, messageErrorForRequireFullAddressForIndia);
				AssertHasMessageError(consignee.PostcodeInfo, messageErrorForRequireFullAddressForIndia);
				AssertHasMessageError(consignee.CityInfo, messageErrorForRequireFullAddressForIndia);

				consignee.Country.Code = "IN";
				consignee.Postcode = "10062";
				consignee.City = "Sydney";

				AssertNoMessageError(consignee.Country.NameInfo, messageErrorForRequireFullAddressForIndia);
				AssertNoMessageError(consignee.PostcodeInfo, messageErrorForRequireFullAddressForIndia);
				AssertNoMessageError(consignee.CityInfo, messageErrorForRequireFullAddressForIndia);
			}
		}

		[TestDate(2021, 10, 1)]
		public void TestConsingeeValidation_IndiaImports_RequiredPANNumber()
		{
			var indiaConsingeeTaxNumberMessage = "Consignee's IEC or PAN is required for imports to India.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				PopulateRefData();
				var consol = CreateConsol();
				consol.JK_RL_NKDischargePort = "IN5PA";
				consol.Transports[2].JW_RL_NKDiscPort = "IN5PA";

				var receivingForwarder = Factory.New<OrgHeader>();
				receivingForwarder.OH_FullName = "I'm Receiving Stuff";
				receivingForwarder.OH_RL_NKClosestPort = "IN5PA";
				receivingForwarder.MainAddress.Address1 = "Unit 399";
				receivingForwarder.MainAddress.Address2 = "50 What Lane";
				receivingForwarder.MainAddress.Postcode = "5023";
				receivingForwarder.MainAddress.OA_RN_NKCountryCode = "IN";

				consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

				var wrapper = new ShippingInstructionBuilder(consol).Build();
				var consignee = wrapper.Consignee;

				var panTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN);
				AssertHasMessageError("Require PAN for an India consignee", panTaxInfo.NumberInfo, indiaConsingeeTaxNumberMessage);

				consignee.Country.Code = "CN";
				AssertNoMessageError("Do not require PAN for a China consignee", panTaxInfo.NumberInfo, indiaConsingeeTaxNumberMessage);

				var consingeePAN = consol.ReceivingForwarder.CustomsCodes.AddNew();
				consingeePAN.OK_RN_NKCodeCountry = "IN";
				consingeePAN.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
				consingeePAN.OK_CustomsRegNo = "98989";

				var consingeeIEC = consol.ReceivingForwarder.CustomsCodes.AddNew();
				consingeeIEC.OK_RN_NKCodeCountry = "IN";
				consingeeIEC.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
				consingeeIEC.OK_CustomsRegNo = "80058";

				wrapper = new ShippingInstructionBuilder(consol).Build();

				AssertNoMessageError("With IEC", wrapper.ConsigneeTaxInfo1.NumberInfo, indiaConsingeeTaxNumberMessage);
				AssertNoMessageError("With PAN", wrapper.ConsigneeTaxInfo2.NumberInfo, indiaConsingeeTaxNumberMessage);

				wrapper.ConsigneeTaxInfo1.Number = string.Empty;
				AssertNoMessageError("With PAN", wrapper.ConsigneeTaxInfo2.NumberInfo, indiaConsingeeTaxNumberMessage);

				wrapper.ConsigneeTaxInfo2.Number = string.Empty;
				AssertHasMessageError("Neither PAN nor IEC were entered", wrapper.ConsigneeTaxInfo2.NumberInfo, indiaConsingeeTaxNumberMessage);
			}
		}

		[TestDate(2021, 10, 1)]
		public void TestConsingeeValidation_IndiaImports_RequiredPANNumber_NotifyPartSameAsConsignee()
		{
			var indiaSameAsConsingeeTaxNumberMessage = "Consignee's PAN is required for imports to India when the Notify Party is 'Same as Consignee'.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				PopulateRefData();
				var consol = CreateConsol();
				consol.JK_RL_NKDischargePort = "IN5PA";
				consol.Transports[2].JW_RL_NKDiscPort = "IN5PA";

				var receivingForwarder = Factory.New<OrgHeader>();
				receivingForwarder.OH_FullName = "I'm Receiving Stuff";
				receivingForwarder.OH_RL_NKClosestPort = "IN5PA";
				receivingForwarder.MainAddress.Address1 = "Unit 399";
				receivingForwarder.MainAddress.Address2 = "50 What Lane";
				receivingForwarder.MainAddress.Postcode = "5023";
				receivingForwarder.MainAddress.OA_RN_NKCountryCode = "IN";

				consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

				var consingeePAN = consol.ReceivingForwarder.CustomsCodes.AddNew();
				consingeePAN.OK_RN_NKCodeCountry = "IN";
				consingeePAN.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
				consingeePAN.OK_CustomsRegNo = "98989";

				var wrapper = new ShippingInstructionBuilder(consol).Build();
				var notifyParty = wrapper.NotifyParty;
				notifyParty.CompanyName = "SAME AS CONSIGNEE";

				AssertEquals("PAN", "98989", wrapper.ConsigneeTaxInfo1.Number);
				AssertNoMessageError("Require PAN for an India consignee", wrapper.ConsigneeTaxInfo1.NumberInfo, indiaSameAsConsingeeTaxNumberMessage);

				wrapper.ConsigneeTaxInfo1.Number = string.Empty;
				AssertHasMessageError("Require PAN for an India consignee", wrapper.ConsigneeTaxInfo1.NumberInfo, indiaSameAsConsingeeTaxNumberMessage);

				wrapper.ConsigneeTaxInfo1.Number = "A123";
				AssertNoMessageError("PAN entered", wrapper.ConsigneeTaxInfo1.NumberInfo, indiaSameAsConsingeeTaxNumberMessage);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestRequiredConsigneeOrNotifyPartyContactEmail_IndiaImports()
		{
			var indiaConsigneeContactEmailMessage = "Consignee's Email is required for India imports.";
			var indiaNotifyPartyContactEmailMessage = "Notify Party's Email is required for India imports.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = CreateConsol();
				consol.JK_RL_NKDischargePort = "IN5PA";
				consol.Transports[2].JW_RL_NKDiscPort = "IN5PA";

				var wrapper = new ShippingInstructionBuilder(consol).Build();
				var consignee = wrapper.Consignee;
				var notifyParty = wrapper.NotifyParty;

				notifyParty.Email = string.Empty;
				consignee.Email = string.Empty;

				AssertHasMessageError("Require contact email from consignee or notify party", consignee.EmailInfo, indiaConsigneeContactEmailMessage);
				AssertHasMessageError("Require contact email from consignee or notify party", notifyParty.EmailInfo, indiaNotifyPartyContactEmailMessage);

				notifyParty.Email = "tom@gmail.com";
				AssertNoMessageError("Do not require notify party contact email when consignee's email had been entered", consignee.EmailInfo, indiaConsigneeContactEmailMessage);
				AssertNoMessageError("Notify party's email entered", notifyParty.EmailInfo, indiaNotifyPartyContactEmailMessage);

				notifyParty.Email = string.Empty;
				consignee.Email = "tom@gmail.com";

				AssertNoMessageError("Notify party's email entered", consignee.EmailInfo, indiaConsigneeContactEmailMessage);
				AssertNoMessageError("Do not require notify party contact email when consignee's email had been entered", notifyParty.EmailInfo, indiaNotifyPartyContactEmailMessage);

				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.Transports[2].JW_RL_NKDiscPort = "CNSHA";

				wrapper = new ShippingInstructionBuilder(consol).Build();
				consignee = wrapper.Consignee;
				notifyParty = wrapper.NotifyParty;

				notifyParty.Email = string.Empty;
				consignee.Email = string.Empty;

				AssertNoMessageError("Email validation only for India imports", consignee.EmailInfo, indiaConsigneeContactEmailMessage);
				AssertNoMessageError("Email validation only for India imports", notifyParty.EmailInfo, indiaNotifyPartyContactEmailMessage);
			}
		}

		public void TestConsingeeValidation_GBImport_RequiredEoriNumber()
		{
			var gbConsingeeTaxNumberMessage = "Consignee's EORI is required for Great Britain.";

			PopulateRefData();
			var consol = CreateConsol("NLRTM", "BEANR");

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "NLRTM";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "NL";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			AssertNull("There should be no EORI for a none GB import", eorTaxInfo);

			receivingForwarder.OH_RL_NKClosestPort = "GBEDI";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "GB";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNull("There should be no EORI for a none GB import", wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));

			consol.JK_RL_NKDischargePort = "GBEDI";
			consol.Transports[2].JW_RL_NKDiscPort = "GBEDI";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError("Required EORI number for a GB import", wrapper.ConsigneeTaxInfo1.NumberInfo, gbConsingeeTaxNumberMessage);

			var consingeeEOR = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consingeeEOR.OK_RN_NKCodeCountry = "GB";
			consingeeEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			consingeeEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError("Required EORI number for a GB import", wrapper.ConsigneeTaxInfo1.NumberInfo, gbConsingeeTaxNumberMessage);

			consol.ReceivingForwarder.CustomsCodes.RemoveAll();
			consingeeEOR = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consingeeEOR.OK_RN_NKCodeCountry = "GB";
			consingeeEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			consingeeEOR.OK_CustomsRegNo = "12345";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GB added to EORI number", "GB12345", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoMessageError("No error EORI number filled in with no prefix", wrapper.ConsigneeTaxInfo1.NumberInfo, gbConsingeeTaxNumberMessage);

			consingeeEOR.OK_CustomsRegNo = "XI54321";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("No GB added to EORI number because number starts with XI", "XI54321", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoMessageError("No error EORI number filled in with XI prefix", wrapper.ConsigneeTaxInfo1.NumberInfo, gbConsingeeTaxNumberMessage);
		}

		public void TestConsingeeValidation_GBImport_RequiredEoriNumber_CountryOfConsingeeIsNotGB()
		{
			var gbConsingeeTaxNumberMessage = "Consignee's EORI is required for Great Britain.";

			CreateRefDocOrgCusCode(Constants.CountryCodes.UnitedKingdom, Constants.CountryCodes.UnitedKingdom, "EOR", 1);

			Factory.Save();

			var consol = CreateConsol("AU2CO", "GB2AB");

			var consingee = Factory.New<OrgHeader>();
			consingee.OH_FullName = "I'm Receiving Stuff";
			consingee.OH_RL_NKClosestPort = "EGAAC";
			consingee.MainAddress.Address1 = "Unit 399";
			consingee.MainAddress.Address2 = "50 What Lane";
			consingee.MainAddress.Postcode = "5023";
			consingee.MainAddress.OA_RN_NKCountryCode = "EG";
			consol.JK_OA_ReceivingForwarderAddress = consingee.MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbConsingeeTaxNumberMessage);

			var consingeeEOR = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consingeeEOR.OK_RN_NKCodeCountry = "GB";
			consingeeEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			consingeeEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbConsingeeTaxNumberMessage);

			consingeeEOR.OK_RN_NKCodeCountry = "EG";
			consingeeEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbConsingeeTaxNumberMessage);

			consingeeEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			Assert(eorTaxInfo.Number.EndsWith("123"));
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbConsingeeTaxNumberMessage);

			consingee.OH_RL_NKClosestPort = "GBEDI";
			consingee.MainAddress.OA_RN_NKCountryCode = "GB";
			consol.JK_OA_ReceivingForwarderAddress = consingee.MainAddress.PK;

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertHasMessageError(eorTaxInfo.NumberInfo, gbConsingeeTaxNumberMessage);
		}

		public void TestPopulateConsingeeEoriNumber_CountryOfPODIsGB()
		{
			CreateRefDocOrgCusCode(Constants.CountryCodes.UnitedKingdom, Constants.CountryCodes.UnitedKingdom, "EOR", 1);

			Factory.Save();

			var consol = CreateConsol("AU2CO", "GB2AB");

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "EGAAC";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "EG";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			var consingeeEOR = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consingeeEOR.OK_RN_NKCodeCountry = "GB";
			consingeeEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			consingeeEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			consingeeEOR.OK_RN_NKCodeCountry = "EG";
			consingeeEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			consingeeEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ConsigneeTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertEquals("EG123", eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);
		}

		public void TestConsingeeValidation_EgyptImports_RequiredTaxNumber()
		{
			var egConsigneeTaxNumberMessage = @"Consignee’s Egyptian Importer VAT Number is mandatory for imports to Egypt,
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=EG, Type=VAT.";
			var egNotifyPartyTaxNumberMessage = @"Notify Party’s Egyptian Importer VAT Number is mandatory for imports to Egypt when Consignee is 'TO ORDER',
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=EG, Type=VAT.";

			var egyptGcrBO = Factory.New<RefDocOrgCusCode>();
			egyptGcrBO.DOC_DocumentType = "ESI";
			egyptGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Egypt;
			egyptGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptGcrBO.DOC_Notes = "Tax ID";
			egyptGcrBO.DOC_CodeType = "VAT";
			egyptGcrBO.DOC_Priority = 1;
			egyptGcrBO.DOC_Description = "VAT Number";

			Factory.Save();

			var consol = CreateConsol("AUSYD", "EGALY");
			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Is To Egypt", true, wrapper.IsToEgypt);
			AssertEquals("Tax Info Code Value is empty", string.Empty, wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("Tax Info Code", "VAT", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("Tax Info Country Code", "EG", wrapper.ConsigneeTaxInfo1.Country.Code);
			AssertEquals("Tax Info Regulation Country Code", "EG", wrapper.ConsigneeTaxInfo1.RegulatingCountry.Code);
			AssertHasMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, egConsigneeTaxNumberMessage);

			wrapper.ConsigneeTaxInfo1.Number = "ABC";
			wrapper.ValidateAllIncludingChildren();
			AssertEquals("Tax Info Code Value is filled in", "ABC", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("VAT Number", wrapper.ConsigneeTaxInfo1.DisplayedLabel);
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, egConsigneeTaxNumberMessage);

			wrapper.Consignee.CompanyName = "TO ORDER";
			wrapper.ConsigneeTaxInfo1.Number = string.Empty;
			wrapper.ValidateAllIncludingChildren();
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, egConsigneeTaxNumberMessage);
			AssertHasMessageError(wrapper.NotifyPartyTaxInfo1.NumberInfo, egNotifyPartyTaxNumberMessage);

			wrapper.NotifyPartyTaxInfo1.Number = "ABC";
			wrapper.ValidateAllIncludingChildren();
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, egConsigneeTaxNumberMessage);
			AssertEquals("Tax Info Code Value is filled in", "ABC", wrapper.NotifyPartyTaxInfo1.Number);
			AssertEquals("VAT Number", wrapper.NotifyPartyTaxInfo1.DisplayedLabel);
			AssertNoMessageError(wrapper.NotifyPartyTaxInfo1.NumberInfo, egNotifyPartyTaxNumberMessage);
		}

		public void TestConsingeeValidation_KenyaImports_RequiredTaxNumber()
		{
			var keConsigneeTaxNumberMessage = @"Consignee PIN (Personal Identification Number) is required to comply with Manifest reporting for Kenya,
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=KE, Type=PIN";
			var keNotifyPartyTaxNumberMessage = @"Notify Party PIN (Personal Identification Number) is required to comply with Manifest reporting for Kenya,
Please maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=KE, Type=PIN";

			var kenyaGcrBO = Factory.New<RefDocOrgCusCode>();
			kenyaGcrBO.DOC_DocumentType = "ESI";
			kenyaGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			kenyaGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Kenya;
			kenyaGcrBO.DOC_Notes = "Personal Identification";
			kenyaGcrBO.DOC_CodeType = "PIN";
			kenyaGcrBO.DOC_ShortLabel = "PersID";
			kenyaGcrBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "KEMBA");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("Is To Kenya", true, wrapper.IsToKenya);
			AssertEquals("Tax Info Code", "PIN", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("Tax Info Country Code", "KE", wrapper.ConsigneeTaxInfo1.Country.Code);
			AssertEquals("Tax Info Regulation Country Code", "KE", wrapper.ConsigneeTaxInfo1.RegulatingCountry.Code);

			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, keConsigneeTaxNumberMessage);
			AssertEquals("Tax Info Code Value is empty", string.Empty, wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarnings(wrapper.NotifyPartyTaxInfo1.NumberInfo);

			consol.ReceivingForwarderAddress.Header.CustomsCodes.AddNew(OrgCusCode.KenyaCodeTypes.PIN, "PIN123", Constants.CountryCodes.Kenya);
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarnings(wrapper.ConsigneeTaxInfo1.NumberInfo);
			AssertEquals("Tax Info Code Value is filled in", "PIN123", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarnings(wrapper.NotifyPartyTaxInfo1.NumberInfo);

			consol.ReceivingForwarderAddress.Header.OH_FullName = "TO ORDER";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Consignee1 Tax info should be empty", string.Empty, wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("Consignee1 Tax info should be empty", string.Empty, wrapper.ConsigneeTaxInfo1.Number);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, keNotifyPartyTaxNumberMessage);

			consol.ReceivingForwarderAddress.Header.CustomsCodes.RemoveAndDeleteAll();
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Consignee1 Tax info should be empty", string.Empty, wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("Consignee1 Tax info should be empty", string.Empty, wrapper.ConsigneeTaxInfo1.Number);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, keNotifyPartyTaxNumberMessage);

			consol.NotifyPartyDocumentaryAddress.Organisation.CustomsCodes.AddNew(OrgCusCode.KenyaCodeTypes.PIN, "PIN123", Constants.CountryCodes.Kenya);
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Consignee1 Tax info should be empty", string.Empty, wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("Consignee1 Tax info should be empty", string.Empty, wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, keNotifyPartyTaxNumberMessage);
			AssertEquals("Tax Info Code Value is filled in", "PIN123", wrapper.NotifyPartyTaxInfo1.Number);

			consol.ReceivingForwarderAddress.Header.OH_FullName = "KENYA COMPANY";
			consol.NotifyPartyDocumentaryAddress.Organisation.OH_FullName = "SAME AS CONSIGNEE";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, keConsigneeTaxNumberMessage);
			AssertEquals("Tax Info Code Value is empty", string.Empty, wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("NotifyParty1 Tax info should be empty", string.Empty, wrapper.NotifyPartyTaxInfo1.Code);
			AssertEquals("NotifyParty1 Tax info should be empty", string.Empty, wrapper.NotifyPartyTaxInfo1.Number);

			consol.NotifyPartyDocumentaryAddress.Organisation.CustomsCodes.RemoveAndDeleteAll();
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, keConsigneeTaxNumberMessage);
			AssertEquals("Tax Info Code Value is empty", string.Empty, wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("NotifyParty1 Tax info should be empty", string.Empty, wrapper.NotifyPartyTaxInfo1.Code);
			AssertEquals("NotifyParty1 Tax info should be empty", string.Empty, wrapper.NotifyPartyTaxInfo1.Number);

			consol.ReceivingForwarderAddress.Header.CustomsCodes.AddNew(OrgCusCode.KenyaCodeTypes.PIN, "PIN123", Constants.CountryCodes.Kenya);
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarnings(wrapper.ConsigneeTaxInfo1.NumberInfo);
			AssertEquals("Tax Info Code Value is filled in", "PIN123", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("NotifyParty1 Tax info should be empty", string.Empty, wrapper.NotifyPartyTaxInfo1.Code);
			AssertEquals("NotifyParty1 Tax info should be empty", string.Empty, wrapper.NotifyPartyTaxInfo1.Number);
		}

		public void TestConsingeeValidation_TaxNumberLength()
		{
			const string taxNumberLengthWarningMessage = @"Company tax ID longer than 26 characters may result in rejection from carriers.
Please maintain proper ID in the Organization > Config > Registration Numbers/Codes tab.";

			var refDocOrgCusCodeGCRBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeGCRBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeGCRBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeGCRBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeGCRBO.DOC_Notes = "Personal Identification";
			refDocOrgCusCodeGCRBO.DOC_CodeType = "GCR";
			refDocOrgCusCodeGCRBO.DOC_ShortLabel = "PersID";
			refDocOrgCusCodeGCRBO.DOC_Priority = 1;

			var refDocOrgCusCodeCOMBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeCOMBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeCOMBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCOMBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCOMBO.DOC_Notes = "Registration ID";
			refDocOrgCusCodeCOMBO.DOC_CodeType = "COM";
			refDocOrgCusCodeCOMBO.DOC_ShortLabel = "TEST_SL_EG";
			refDocOrgCusCodeCOMBO.DOC_Priority = 1;

			var refDocOrgCusCodeCJNBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeCJNBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeCJNBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCJNBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCJNBO.DOC_Notes = "TEST";
			refDocOrgCusCodeCJNBO.DOC_CodeType = "CJN";
			refDocOrgCusCodeCJNBO.DOC_ShortLabel = "TEST_SL_1";
			refDocOrgCusCodeCJNBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "KEMBA");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNoWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);

			var gcrCustomsCode = consol.ReceivingForwarderAddress.Header.CustomsCodes.AddNew("GCR", "1234567890", Constants.CountryCodes.Kenya);
			var comCustomsCode = consol.ReceivingForwarderAddress.Header.CustomsCodes.AddNew("COM", "1234567890", Constants.CountryCodes.Kenya);
			var cjnCustomsCode = consol.ReceivingForwarderAddress.Header.CustomsCodes.AddNew("CJN", "1234567890", Constants.CountryCodes.Kenya);

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNoWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);

			gcrCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";
			comCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";
			cjnCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertHasWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertHasWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertHasWarning(wrapper.ConsigneeTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);
		}

		[ExpectNoExceptions]
		public void TestConsigneeTaxInfo_WhenDirectConsolForBangladesh_ShouldUseBIN()
		{
			CreateRefDocOrgCusCode(CountryCodes.Bangladesh, CountryCodes.Bangladesh, "AIN", 1, ZString.Empty, "AIN", "ESI");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BDDAC";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bangladesh Consignee";
			consignee.OH_RL_NKClosestPort = "BDDAC";
			consignee.MainAddress.Address1 = "Unit 155";
			consignee.MainAddress.Address2 = "55 Lost Lane";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BD";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			consol.ReceivingForwarderWithContact.OrgPK = consignee.PK;

			Factory.Save();

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions("Should not find any tax info from ref data.", () =>
			{
				AssertNull("Consignee", shippingInstruction.ConsigneeTaxInfo1);
				AssertNull("NotifyParty", shippingInstruction.NotifyPartyTaxInfo1);
			});

			CreateRefDocOrgCusCode(CountryCodes.Bangladesh, CountryCodes.Bangladesh, "VAT", 1, ZString.Empty, "BIN", "ESI");

			Factory.Save();

			const string expectedErrorConsignee = "The Consignee VAT (BIN – Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			const string expectedErrorNotifyParty = "The Notify Party VAT (BIN – Business Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(shippingInstruction.ConsigneeTaxInfo1.NumberInfo, expectedErrorConsignee);
			AssertHasMessageError(shippingInstruction.NotifyPartyTaxInfo1.NumberInfo, expectedErrorNotifyParty);

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			Factory.Save();

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertNoMessageError(shippingInstruction.ConsigneeTaxInfo1.NumberInfo, expectedErrorConsignee);
			AssertNoMessageError(shippingInstruction.NotifyPartyTaxInfo1.NumberInfo, expectedErrorNotifyParty);
			AssertEquals("123BIN", shippingInstruction.ConsigneeTaxInfo1.Number);
			AssertEquals("VAT", shippingInstruction.ConsigneeTaxInfo1.Code);
			AssertEquals("BD", shippingInstruction.ConsigneeTaxInfo1.Country.Code);
			AssertEquals("BD", shippingInstruction.ConsigneeTaxInfo1.RegulatingCountry.Code);
			AssertEquals("123BIN", shippingInstruction.NotifyPartyTaxInfo1.Number);
			AssertEquals("VAT", shippingInstruction.NotifyPartyTaxInfo1.Code);
			AssertEquals("BD", shippingInstruction.NotifyPartyTaxInfo1.Country.Code);
			AssertEquals("BD", shippingInstruction.NotifyPartyTaxInfo1.RegulatingCountry.Code);
		}

		public void TestConsigneeTaxInfo_WhenNonDirectConsol_ShouldUseAIN()
		{
			CreateRefDocOrgCusCode(CountryCodes.Bangladesh, CountryCodes.Bangladesh, "VAT", 1, ZString.Empty, "BIN", "ESI");
			CreateRefDocOrgCusCode(CountryCodes.Bangladesh, CountryCodes.Bangladesh, "AIN", 1, ZString.Empty, "AIN", "ESI");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BDDAC";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_IsNeutralMaster = ZBool.False;
			consol.JK_MasterBillNum = "17610000001";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BDDAC";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Bangladesh Consignee";
			consignee.OH_RL_NKClosestPort = "BDDAC";
			consignee.MainAddress.Address1 = "Unit 155";
			consignee.MainAddress.Address2 = "55 Lost Lane";
			consignee.MainAddress.City = "Bangladesh City";
			consignee.MainAddress.Postcode = "2215";
			consignee.MainAddress.OA_RN_NKCountryCode = "BD";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.NotifyPartyContactPK = consignee.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			consol.ReceivingForwarderWithContact.OrgPK = consignee.PK;
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			Factory.Save();

			const string expectedErrorConsignee = "The Sending Agent AIN (Agent Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";
			const string expectedErrorNotifyParty = "The Notify Party AIN (Agent Identification Number) is required for imports to Bangladesh to comply with customs import processing per Customs Circular NBR/IT/AWIP/ADMINP(1)/12/499.";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(shippingInstruction.ConsigneeTaxInfo1.NumberInfo, expectedErrorConsignee);
			AssertHasMessageError(shippingInstruction.NotifyPartyTaxInfo1.NumberInfo, expectedErrorNotifyParty);

			var ainCode = consignee.CustomsCodes.AddNew();
			ainCode.OK_CodeType = OrgCusCode.BangladeshCodeTypes.AIN;
			ainCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			ainCode.OK_CustomsRegNo = "123AIN";
			var vatCode = consignee.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Bangladesh;
			vatCode.OK_CustomsRegNo = "123BIN";

			Factory.Save();

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(shippingInstruction.ConsigneeTaxInfo1.NumberInfo, expectedErrorConsignee);
			AssertNoMessageError(shippingInstruction.NotifyPartyTaxInfo1.NumberInfo, expectedErrorNotifyParty);
			AssertEquals("123AIN", shippingInstruction.ConsigneeTaxInfo1.Number);
			AssertEquals("AIN", shippingInstruction.ConsigneeTaxInfo1.Code);
			AssertEquals("BD", shippingInstruction.ConsigneeTaxInfo1.Country.Code);
			AssertEquals("BD", shippingInstruction.ConsigneeTaxInfo1.RegulatingCountry.Code);
			AssertEquals("123AIN", shippingInstruction.NotifyPartyTaxInfo1.Number);
			AssertEquals("AIN", shippingInstruction.NotifyPartyTaxInfo1.Code);
			AssertEquals("BD", shippingInstruction.NotifyPartyTaxInfo1.Country.Code);
			AssertEquals("BD", shippingInstruction.NotifyPartyTaxInfo1.RegulatingCountry.Code);
		}
		#endregion

		#region Notify Party

		public void TestNotifyPartyCountryValidation()
		{
			var consol = CreateConsol();
			consol.Transports[2].JW_RL_NKDiscPort = "BRSAO";

			var consigneeAddressPK = consol.ReceivingForwarderAddress.PK;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Precondition", "Australia", shippingInstruction.NotifyParty.Country.Name);
			AssertHasMessageError(shippingInstruction.NotifyParty.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			shippingInstruction.NotifyParty.Country.Name = "Brazil";
			AssertNoMessageError(shippingInstruction.NotifyParty.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			shippingInstruction.NotifyParty.Country.Name = "Guatamala";
			AssertHasMessageError(shippingInstruction.NotifyParty.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			shippingInstruction.Consignee.Country.Name = "Brazil";
			AssertNoMessageError(shippingInstruction.NotifyParty.Country.NameInfo, "Either Consignee's or Notify Party's country code must be Brazil for Brazil imports.");

			consol.Shipments[0].JS_RL_NKDestination = "CNSHA";
			consol.Transports[2].JW_RL_NKDiscPort = "CNSHA";
			consol.JK_OA_ReceivingForwarderAddress = consigneeAddressPK;

			var chinaTaxNumber = consol.NotifyParty.CustomsCodes.AddNew();
			chinaTaxNumber.OK_RN_NKCodeCountry = "CN";
			chinaTaxNumber.OK_CodeType = "NGB";
			chinaTaxNumber.OK_CustomsRegNo = "12345";

			var australiaTaxNumber = consol.NotifyParty.CustomsCodes.AddNew();
			australiaTaxNumber.OK_RN_NKCodeCountry = "AU";
			australiaTaxNumber.OK_CodeType = "ABN";
			australiaTaxNumber.OK_CustomsRegNo = "54321";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(shippingInstruction.NotifyParty.Country.NameInfo, "Notify Party's country is different from Destination country.");

			shippingInstruction.NotifyParty.Country.Name = "CHINA";
			AssertNoWarning(shippingInstruction.Consignee.Country.NameInfo, "Notify Party's country is different from Destination country.");
		}

		public void TestNotifyPartyPostcodeValidation()
		{
			var consol = CreateConsol();

			consol.Transports[2].JW_RL_NKLoadPort = "AUBNE";
			consol.Transports[2].JW_RL_NKDiscPort = "USNYC";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("Precondition", !wrapper.NotifyParty.Postcode.IsEmpty);
			AssertNoMessageError(wrapper.NotifyParty.PostcodeInfo, "Notify Party's postcode is required for US imports.");

			wrapper.NotifyParty.Postcode = "";
			AssertHasMessageError(wrapper.NotifyParty.PostcodeInfo, "Notify Party's postcode is required for US imports.");
		}

		public void TestNotifyPartyValidation_RequiredEoriNumber()
		{
			var gbNotifyPartyTaxNumberMessage = "Notify Party's EORI is required for Great Britain.";

			PopulateRefData();
			var consol = CreateConsol("NLRTM", "BEANR");

			var notifyParty = consol.NotifyPartyDocumentaryAddress.Organisation;
			notifyParty.OH_FullName = "NotifyParty";
			notifyParty.OH_RL_NKClosestPort = "NLRTM";
			notifyParty.MainAddress.Address1 = "Unit 399";
			notifyParty.MainAddress.Address2 = "50 What Lane";
			notifyParty.MainAddress.Postcode = "5023";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NL";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			AssertNull("There should be no EORI", eorTaxInfo);

			notifyParty.OH_RL_NKClosestPort = "GBEDI";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "GB";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNull("There should be no EORI", wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));

			consol.JK_RL_NKDischargePort = "GBEDI";
			consol.Transports[2].JW_RL_NKDiscPort = "GBEDI";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError("Required EORI number", wrapper.NotifyPartyTaxInfo1.NumberInfo, gbNotifyPartyTaxNumberMessage);

			var notifyPartyEOR = notifyParty.CustomsCodes.AddNew();
			notifyPartyEOR.OK_RN_NKCodeCountry = "GB";
			notifyPartyEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			notifyPartyEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError("Required EORI number", wrapper.NotifyPartyTaxInfo1.NumberInfo, gbNotifyPartyTaxNumberMessage);

			notifyParty.CustomsCodes.RemoveAll();
			notifyPartyEOR = notifyParty.CustomsCodes.AddNew();
			notifyPartyEOR.OK_RN_NKCodeCountry = "GB";
			notifyPartyEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			notifyPartyEOR.OK_CustomsRegNo = "12345";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GB added to EORI number", "GB12345", wrapper.NotifyPartyTaxInfo1.Number);
			AssertNoMessageError("No error EORI number filled in with no prefix", wrapper.NotifyPartyTaxInfo1.NumberInfo, gbNotifyPartyTaxNumberMessage);
		}

		public void TestNotifyPartyValidation_RequiredEoriNumber_CountryOfConsingeeIsNotGB()
		{
			var gbNotifyPartyTaxNumberMessage = "Notify Party's EORI is required for Great Britain.";

			CreateRefDocOrgCusCode(Constants.CountryCodes.UnitedKingdom, Constants.CountryCodes.UnitedKingdom, "EOR", 1);

			Factory.Save();

			var consol = CreateConsol("AU2CO", "GB2AB");

			var notifyParty = consol.NotifyPartyDocumentaryAddress.Organisation;
			notifyParty.OH_FullName = "NotifyParty";
			notifyParty.OH_RL_NKClosestPort = "EGAAC";
			notifyParty.MainAddress.Address1 = "Unit 399";
			notifyParty.MainAddress.Address2 = "50 What Lane";
			notifyParty.MainAddress.Postcode = "5023";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "EG";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbNotifyPartyTaxNumberMessage);

			var notifyPartyEOR = notifyParty.CustomsCodes.AddNew();
			notifyPartyEOR.OK_RN_NKCodeCountry = "GB";
			notifyPartyEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			notifyPartyEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbNotifyPartyTaxNumberMessage);

			notifyPartyEOR.OK_RN_NKCodeCountry = "EG";
			notifyPartyEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbNotifyPartyTaxNumberMessage);

			notifyPartyEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			Assert(eorTaxInfo.Number.EndsWith("123"));
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbNotifyPartyTaxNumberMessage);

			notifyParty.OH_RL_NKClosestPort = "GBEDI";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "GB";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertHasMessageError(eorTaxInfo.NumberInfo, gbNotifyPartyTaxNumberMessage);
		}

		public void TestPopulateNotifyPartyEoriNumber_CountryOfPODIsGB()
		{
			CreateRefDocOrgCusCode(Constants.CountryCodes.UnitedKingdom, Constants.CountryCodes.UnitedKingdom, "EOR", 1);

			Factory.Save();

			var consol = CreateConsol("AU2CO", "GB2AB");

			var notifyParty = consol.NotifyPartyDocumentaryAddress.Organisation;
			notifyParty.OH_FullName = "NotifyParty";
			notifyParty.OH_RL_NKClosestPort = "EGAAC";
			notifyParty.MainAddress.Address1 = "Unit 399";
			notifyParty.MainAddress.Address2 = "50 What Lane";
			notifyParty.MainAddress.Postcode = "5023";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "EG";
			consol.JK_OA_ReceivingForwarderAddress = notifyParty.MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			var notifyPartyEOR = notifyParty.CustomsCodes.AddNew();
			notifyPartyEOR.OK_RN_NKCodeCountry = "GB";
			notifyPartyEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			notifyPartyEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			notifyPartyEOR.OK_RN_NKCodeCountry = "EG";
			notifyPartyEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			notifyPartyEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertEquals("EG123", eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);
		}

		[TestDate(2022, 1, 1)]
		public void TestNotifyPartyValidation_IndiaImports()
		{
			var messageErrorForRequireFullAddressForIndia = "Full address is required to comply with SCMTR Manifest reporting for India.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = CreateConsol();
				consol.JK_RL_NKDischargePort = "IN5PA";
				consol.Transports[2].JW_RL_NKDiscPort = "IN5PA";

				var wrapper = new ShippingInstructionBuilder(consol).Build();
				var notifyParty = wrapper.NotifyParty;

				notifyParty.Country.Code = string.Empty;
				notifyParty.CompanyName = string.Empty;
				notifyParty.AddressLine1 = string.Empty;
				notifyParty.AddressLine2 = string.Empty;

				wrapper.Consignee.CompanyName = "TO ORDER";
				wrapper.Consignee.Country.Code = "IN";

				AssertHasMessageError("Notify Party full address is requires when consignee is TO ORDER", notifyParty.Country.NameInfo, messageErrorForRequireFullAddressForIndia);
				AssertHasMessageError("Notify Party full address is requires when consignee is TO ORDER", notifyParty.AddressLine1Info, messageErrorForRequireFullAddressForIndia);
				AssertHasMessageError("Notify Party full address is requires when consignee is TO ORDER", notifyParty.CompanyNameInfo, messageErrorForRequireFullAddressForIndia);

				wrapper.Consignee.CompanyName = "COM 1";
				wrapper.Consignee.Country.Code = "CN";

				AssertHasMessageError("Notify Party full address is requires when consignee is not in India", notifyParty.Country.NameInfo, messageErrorForRequireFullAddressForIndia);
				AssertHasMessageError("Notify Party full address is requires when consignee is not in India", notifyParty.AddressLine1Info, messageErrorForRequireFullAddressForIndia);
				AssertHasMessageError("Notify Party full address is requires when consignee is not in India", notifyParty.CompanyNameInfo, messageErrorForRequireFullAddressForIndia);

				wrapper.Consignee.Country.Code = "IN";
				AssertNoMessageError("No mandatory message error if consingee is not TO ORDER and consignee is in India", notifyParty.Country.NameInfo, messageErrorForRequireFullAddressForIndia);
				AssertNoMessageError("No mandatory message error if consingee is not TO ORDER and consignee is in India", notifyParty.AddressLine1Info, messageErrorForRequireFullAddressForIndia);
				AssertNoMessageError("No mandatory message error if consingee is not TO ORDER and consignee is in India", notifyParty.CompanyNameInfo, messageErrorForRequireFullAddressForIndia);

				wrapper.Consignee.Country.Code = "CN";
				notifyParty.Country.Code = "IN";
				notifyParty.CompanyName = "10062";
				notifyParty.AddressLine1 = "Sydney";

				AssertNoMessageError("Entered address", notifyParty.Country.NameInfo, messageErrorForRequireFullAddressForIndia);
				AssertNoMessageError("Entered address", notifyParty.AddressLine1Info, messageErrorForRequireFullAddressForIndia);
				AssertNoMessageError("Entered address", notifyParty.CompanyNameInfo, messageErrorForRequireFullAddressForIndia);

				notifyParty.AddressLine1 = string.Empty;
				notifyParty.AddressLine2 = "Sydney";
				AssertNoMessageError("Entered address", notifyParty.AddressLine1Info, messageErrorForRequireFullAddressForIndia);
			}
		}

		[TestDate(2022, 1, 1)]
		public void TestNotifyPartyValidation_RequirePAN_WhenConsolRoutingLegIsInIndia()
		{
			var messageErrorForRequirePANCusCodeForIndia = "Notify Party PAN is required to comply with SCMTR Manifest reporting for India.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				PopulateRefData();
				var consol = CreateConsol();
				consol.JK_RL_NKDischargePort = "IN5PA";
				consol.Transports[2].JW_RL_NKDiscPort = "IN5PA";

				var notifyParty = Factory.New<OrgHeader>();
				notifyParty.OH_FullName = "I'm Receiving Stuff";
				notifyParty.OH_RL_NKClosestPort = "IN5PA";
				notifyParty.MainAddress.Address1 = "Unit 399";
				notifyParty.MainAddress.Address2 = "50 What Lane";
				notifyParty.MainAddress.Postcode = "5023";
				notifyParty.MainAddress.OA_RN_NKCountryCode = "IN";

				consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

				var wrapper = new ShippingInstructionBuilder(consol).Build();

				var panTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN);
				AssertHasMessageError("Require PAN for an India Notify Party", panTaxInfo.NumberInfo, messageErrorForRequirePANCusCodeForIndia);

				wrapper.NotifyParty.Country.Code = "CN";
				AssertNoMessageError("Do not require PAN for a China Notify Party", wrapper.NotifyParty.EmailInfo, messageErrorForRequirePANCusCodeForIndia);

				var notifyPartyPAN = consol.NotifyParty.CustomsCodes.AddNew();
				notifyPartyPAN.OK_RN_NKCodeCountry = "IN";
				notifyPartyPAN.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
				notifyPartyPAN.OK_CustomsRegNo = "98989";

				wrapper = new ShippingInstructionBuilder(consol).Build();
				panTaxInfo = wrapper.NotifyPartyTaxInfo.FirstOrDefault(t => t.Code == IndiaOrgCusCodeInfo.OrgCusCodes.PAN);

				AssertEquals("PAN", "98989", panTaxInfo.Number);
				AssertNoMessageError("With PAN", panTaxInfo.NumberInfo, messageErrorForRequirePANCusCodeForIndia);
			}
		}

		public void TestNotifyPartyAddressIsAlwaysPopulatedByNonDirectConsolNotifyPartyAddressWithAnyUltimateConsigneeRules()
		{
			var consol = CreateConsol();
			var shipment = consol.Shipments[0];

			var shipmentNotifyParty = Factory.New<OrgHeader>();
			shipmentNotifyParty.OH_FullName = "CONSPA";
			shipmentNotifyParty.OH_RL_NKClosestPort = "AUSYD";
			shipmentNotifyParty.MainAddress.Address1 = "Unit 15";
			shipmentNotifyParty.MainAddress.Address2 = "5 Lost Lane";
			shipmentNotifyParty.MainAddress.City = "Sydney";
			shipmentNotifyParty.MainAddress.Postcode = "2000";
			shipmentNotifyParty.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = shipmentNotifyParty.MainAddress.PK;

			var consolNotifyPartyAddress = consol.NotifyPartyDocumentaryAddress.AddressAsASingleLine;
			var shipmentNotifyPartyAddress = shipment.NotifyPartyDocumentaryAddress.AddressAsASingleLine;

			Assert("Pre-condition: Consol is not direct", !consol.IsDirect);

			AssertNotNullOrEmpty("Pre-condition: consol notify party address is not empty.", consolNotifyPartyAddress);
			AssertNotNullOrEmpty("Pre-condition: shipment notify party address is not empty.", shipmentNotifyPartyAddress);
			AssertNotEquals("Pre-condition: consol and shipment has different notify party address.", consolNotifyPartyAddress, shipmentNotifyPartyAddress);

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(consol.NotifyParty, shippingInstruction.NotifyParty);

			var ultimateConsigneeRule = consol.DischargePort.Country.Rules.AddNew();
			ultimateConsigneeRule.R7_RN_NKDestination = consol.DischargePort.RL_RN_NKCountryCode;
			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.NotRequired;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(consol.NotifyParty, shippingInstruction.NotifyParty);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.Mandatory;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(consol.NotifyParty, shippingInstruction.NotifyParty);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(consol.NotifyParty, shippingInstruction.NotifyParty);
		}

		public void TestNotifyPartyAddressIsAlwaysPopulatedByDirectShipmentNotifyPartyAddressWithAnyUltimateConsigneeRules()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = "DRT";
			var shipment = consol.Shipments[0];

			var shipmentNotifyParty = Factory.New<OrgHeader>();
			shipmentNotifyParty.OH_FullName = "CONSPA";
			shipmentNotifyParty.OH_RL_NKClosestPort = "AUSYD";
			shipmentNotifyParty.MainAddress.Address1 = "Unit 15";
			shipmentNotifyParty.MainAddress.Address2 = "5 Lost Lane";
			shipmentNotifyParty.MainAddress.City = "Sydney";
			shipmentNotifyParty.MainAddress.Postcode = "2000";
			shipmentNotifyParty.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = shipmentNotifyParty.MainAddress.PK;

			var consolNotifyPartyAddress = consol.NotifyPartyDocumentaryAddress.AddressAsASingleLine;
			var shipmentNotifyPartyAddress = shipment.NotifyPartyDocumentaryAddress.AddressAsASingleLine;

			Assert("Pre-condition: Consol is direct", consol.IsDirect);

			AssertNotNullOrEmpty("Pre-condition: consol notify party address is not empty.", consolNotifyPartyAddress);
			AssertNotNullOrEmpty("Pre-condition: shipment notify party address is not empty.", shipmentNotifyPartyAddress);
			AssertNotEquals("Pre-condition: consol and shipment has different notify party address.", consolNotifyPartyAddress, shipmentNotifyPartyAddress);

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(shipment.NotifyParty, shippingInstruction.NotifyParty);

			var ultimateConsigneeRule = consol.DischargePort.Country.Rules.AddNew();
			ultimateConsigneeRule.R7_RN_NKDestination = consol.DischargePort.RL_RN_NKCountryCode;
			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.NotRequired;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(shipment.NotifyParty, shippingInstruction.NotifyParty);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.Mandatory;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(shipment.NotifyParty, shippingInstruction.NotifyParty);

			ultimateConsigneeRule.R7_UltimateConsigneeRule = Constants.UltimateConsigneeRuleTypes.Codes.VerifyWithCarrierOrLocalAuthorities;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertAddressData(shipment.NotifyParty, shippingInstruction.NotifyParty);
		}

		#endregion

		#region Shipper

		public void TestShipperCountryValidation()
		{
			var consol = CreateConsol();

			var chinaTaxNumber = consol.SendingForwarder.CustomsCodes.AddNew();
			chinaTaxNumber.OK_RN_NKCodeCountry = "CN";
			chinaTaxNumber.OK_CodeType = "GCR";
			chinaTaxNumber.OK_CustomsRegNo = "12345";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(shippingInstruction.Shipper.Country.NameInfo, "Shipper's country is different from Origin country.");

			shippingInstruction.Shipper.Country.Name = "AUSTRALIA";
			AssertHasWarning(shippingInstruction.Shipper.Country.NameInfo, "Shipper's country is different from Origin country.");
		}

		[TestDate(2022, 1, 1)]
		public void TestShipperValidation_IndiaExports_RequiredPANNumber()
		{
			var indiaShipperTaxNumberMessage = "Shipper's IEC or PAN is required for exports from India.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				PopulateRefData();
				var consol = CreateConsol();
				consol.JK_RL_NKLoadPort = "IN5PA";
				consol.Transports[0].JW_RL_NKLoadPort = "IN5PA";

				var sendingForwarder = Factory.New<OrgHeader>();
				sendingForwarder.OH_FullName = "I'm Receiving Stuff";
				sendingForwarder.OH_RL_NKClosestPort = "IN5PA";
				sendingForwarder.MainAddress.Address1 = "Unit 399";
				sendingForwarder.MainAddress.Address2 = "50 What Lane";
				sendingForwarder.MainAddress.Postcode = "5023";
				sendingForwarder.MainAddress.OA_RN_NKCountryCode = "IN";

				consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

				var consingeePAN = consol.SendingForwarder.CustomsCodes.AddNew();
				consingeePAN.OK_RN_NKCodeCountry = "IN";
				consingeePAN.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.PAN;
				consingeePAN.OK_CustomsRegNo = "98989";

				var consingeeIEC = consol.SendingForwarder.CustomsCodes.AddNew();
				consingeeIEC.OK_RN_NKCodeCountry = "IN";
				consingeeIEC.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;
				consingeeIEC.OK_CustomsRegNo = "80058";

				var wrapper = new ShippingInstructionBuilder(consol).Build();

				AssertNoMessageError("With IEC", wrapper.ShipperTaxInfo1.NumberInfo, indiaShipperTaxNumberMessage);
				AssertNoMessageError("With PAN", wrapper.ShipperTaxInfo2.NumberInfo, indiaShipperTaxNumberMessage);

				wrapper.ShipperTaxInfo1.Number = string.Empty;
				AssertNoMessageError("With PAN", wrapper.ShipperTaxInfo2.NumberInfo, indiaShipperTaxNumberMessage);

				wrapper.ShipperTaxInfo2.Number = string.Empty;
				AssertHasMessageError("Neither PAN nor IEC were entered", wrapper.ShipperTaxInfo2.NumberInfo, indiaShipperTaxNumberMessage);
			}
		}

		public void TestShipperValidation_TaxNumberLength()
		{
			const string taxNumberLengthWarningMessage = @"Company tax ID longer than 26 characters may result in rejection from carriers.
Please maintain proper ID in the Organization > Config > Registration Numbers/Codes tab.";

			var refDocOrgCusCodeGCRBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeGCRBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeGCRBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			refDocOrgCusCodeGCRBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Australia;
			refDocOrgCusCodeGCRBO.DOC_Notes = "Personal Identification";
			refDocOrgCusCodeGCRBO.DOC_CodeType = "GCR";
			refDocOrgCusCodeGCRBO.DOC_ShortLabel = "PersID";
			refDocOrgCusCodeGCRBO.DOC_Priority = 1;

			var refDocOrgCusCodeCOMBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeCOMBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeCOMBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			refDocOrgCusCodeCOMBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Australia;
			refDocOrgCusCodeCOMBO.DOC_Notes = "Registration ID";
			refDocOrgCusCodeCOMBO.DOC_CodeType = "COM";
			refDocOrgCusCodeCOMBO.DOC_ShortLabel = "TEST_SL_EG";
			refDocOrgCusCodeCOMBO.DOC_Priority = 1;

			var refDocOrgCusCodeCJNBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeCJNBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeCJNBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			refDocOrgCusCodeCJNBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Australia;
			refDocOrgCusCodeCJNBO.DOC_Notes = "TEST";
			refDocOrgCusCodeCJNBO.DOC_CodeType = "CJN";
			refDocOrgCusCodeCJNBO.DOC_ShortLabel = "TEST_SL_1";
			refDocOrgCusCodeCJNBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "KEMBA");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNoWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);

			var gcrCustomsCode = consol.SendingForwarder.CustomsCodes.AddNew("GCR", "1234567890", Constants.CountryCodes.Australia);
			var comCustomsCode = consol.SendingForwarder.CustomsCodes.AddNew("COM", "1234567890", Constants.CountryCodes.Australia);
			var cjnCustomsCode = consol.SendingForwarder.CustomsCodes.AddNew("CJN", "1234567890", Constants.CountryCodes.Australia);

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNoWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);

			gcrCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";
			comCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";
			cjnCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertHasWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertHasWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertHasWarning(wrapper.ShipperTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);
		}

		public void TestShipperValidation_GBExport_RequiredEoriNumber()
		{
			var gbShipperTaxNumberMessage = "Shipper's EORI is required for Great Britain.";

			PopulateRefData();
			var consol = CreateConsol("NLRTM", "BEANR");

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Receiving Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "NLRTM";
			sendingForwarder.MainAddress.Address1 = "Unit 399";
			sendingForwarder.MainAddress.Address2 = "50 What Lane";
			sendingForwarder.MainAddress.Postcode = "5023";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "NL";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			AssertNull("There should be no EORI for a none GB export", eorTaxInfo);

			sendingForwarder.OH_RL_NKClosestPort = "GBEDI";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "GB";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNull("There should be no EORI for a none GB export", wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));

			consol.JK_RL_NKLoadPort = "GBEDI";
			consol.Transports[0].JW_RL_NKLoadPort = "GBEDI";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError("Required EORI number for a GB", wrapper.ShipperTaxInfo1.NumberInfo, gbShipperTaxNumberMessage);

			var shipperEOR = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperEOR.OK_RN_NKCodeCountry = "GB";
			shipperEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			shipperEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError("Required EORI number for a GB export", wrapper.ShipperTaxInfo1.NumberInfo, gbShipperTaxNumberMessage);

			consol.SendingForwarder.CustomsCodes.RemoveAll();
			shipperEOR = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperEOR.OK_RN_NKCodeCountry = "GB";
			shipperEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			shipperEOR.OK_CustomsRegNo = "12345";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GB added to EORI number", "GB12345", wrapper.ShipperTaxInfo1.Number);
			AssertNoMessageError("No error EORI number filled in", wrapper.ShipperTaxInfo1.NumberInfo, gbShipperTaxNumberMessage);

			shipperEOR.OK_CustomsRegNo = "XI54321";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("No GB added to EORI number because number starts with XI", "XI54321", wrapper.ShipperTaxInfo1.Number);
			AssertNoMessageError("No error EORI number filled in", wrapper.ShipperTaxInfo1.NumberInfo, gbShipperTaxNumberMessage);
		}

		public void TestShipperValidation_GBExport_RequiredEoriNumber_CountryOfShipperIsNotGB()
		{
			var gbShipperTaxNumberMessage = "Shipper's EORI is required for Great Britain.";

			CreateRefDocOrgCusCode(Constants.CountryCodes.UnitedKingdom, Constants.CountryCodes.UnitedKingdom, "EOR", 1);

			Factory.Save();

			var consol = CreateConsol("GB2AB", "AU2CO");

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Shipper";
			shipper.OH_RL_NKClosestPort = "EGAAC";
			shipper.MainAddress.Address1 = "Unit 399";
			shipper.MainAddress.Address2 = "50 What Lane";
			shipper.MainAddress.Postcode = "5023";
			shipper.MainAddress.OA_RN_NKCountryCode = "EG";
			consol.JK_OA_SendingForwarderAddress = shipper.MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbShipperTaxNumberMessage);

			var shipperEOR = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperEOR.OK_RN_NKCodeCountry = "GB";
			shipperEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			shipperEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbShipperTaxNumberMessage);

			shipperEOR.OK_RN_NKCodeCountry = "EG";
			shipperEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbShipperTaxNumberMessage);

			shipperEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			Assert(eorTaxInfo.Number.EndsWith("123"));
			AssertNoMessageError(eorTaxInfo.NumberInfo, gbShipperTaxNumberMessage);

			shipper.OH_RL_NKClosestPort = "GBEDI";
			shipper.MainAddress.OA_RN_NKCountryCode = "GB";
			consol.JK_OA_SendingForwarderAddress = shipper.MainAddress.PK;

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertHasMessageError(eorTaxInfo.NumberInfo, gbShipperTaxNumberMessage);
		}

		public void TestPopulateShipperEoriNumber_CountryOfPOLIsGB()
		{
			CreateRefDocOrgCusCode(Constants.CountryCodes.UnitedKingdom, Constants.CountryCodes.UnitedKingdom, "EOR", 1);

			Factory.Save();

			var consol = CreateConsol("GB2AB", "AU2CO");

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "Shipper";
			shipper.OH_RL_NKClosestPort = "EGAAC";
			shipper.MainAddress.Address1 = "Unit 399";
			shipper.MainAddress.Address2 = "50 What Lane";
			shipper.MainAddress.Postcode = "5023";
			shipper.MainAddress.OA_RN_NKCountryCode = "EG";
			consol.JK_OA_SendingForwarderAddress = shipper.MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			var eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			var shipperEOR = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperEOR.OK_RN_NKCodeCountry = "GB";
			shipperEOR.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			shipperEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			shipperEOR.OK_RN_NKCodeCountry = "EG";
			shipperEOR.OK_CustomsRegNo = "";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertNullOrEmpty(eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);

			shipperEOR.OK_CustomsRegNo = "123";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			eorTaxInfo = wrapper.ShipperTaxInfo.FirstOrDefault(t => t.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			AssertEquals("EG123", eorTaxInfo.Number);
			AssertEquals("EG", eorTaxInfo.Country.Code);
		}

		public void TestShipperValidation_EgyptImports_RequiredTaxNumber()
		{
			var egyptGcrBO = Factory.New<RefDocOrgCusCode>();
			egyptGcrBO.DOC_DocumentType = "ESI";
			egyptGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			egyptGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptGcrBO.DOC_Notes = "Registration ID";
			egyptGcrBO.DOC_CodeType = "COM";
			egyptGcrBO.DOC_ShortLabel = "TEST_SL_EG";
			egyptGcrBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "EGALY");
			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(true, wrapper.IsToEgypt);
			AssertEquals(string.Empty, wrapper.ShipperTaxInfo1.Number);
			AssertEquals("COM", wrapper.ShipperTaxInfo1.Code);
			AssertEquals("AU", wrapper.ShipperTaxInfo1.Country.Code);
			AssertEquals("EG", wrapper.ShipperTaxInfo1.RegulatingCountry.Code);

			AssertHasMessageError(wrapper.ShipperTaxInfo1.NumberInfo, "Exporter registration number of the Shipper is mandatory for cargo destined to Egypt.");

			wrapper.ShipperTaxInfo1.Number = "ABC";
			AssertNoMessageError(wrapper.ShipperTaxInfo1.NumberInfo, "Exporter registration number of the Shipper is mandatory for cargo destined to Egypt.");
		}

		#endregion

		#region Recipient

		public void TestRecipientType()
		{
			var consol = CreateConsol();
			var creditorAddressPK = consol.JK_OA_CreditorAddress;
			AssertEquals("Precondition", "AGT", consol.JK_AgentType);

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Carrier", shippingInstruction.RecipientType);
			AssertEquals("MAERSK", shippingInstruction.Recipient.CompanyName);

			consol.JK_AgentType = "CLD";
			consol.JK_OA_CreditorAddress = creditorAddressPK;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("Co-Load With", shippingInstruction.RecipientType);
			AssertEquals("I'm The Money", shippingInstruction.Recipient.CompanyName);
		}

		public void TestRecipientNonContainerisedMessagingValidation()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = "BBK";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(shippingInstruction.Recipient.CompanyNameInfo, "Please make sure this Carrier supports non-containerised messaging.  Alternatively, you can use the \"Deliver Document\" button to send this form as PDF.");

			consol.JK_ConsolMode = "FCL";
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(shippingInstruction.Recipient.CompanyNameInfo, "Please make sure this Carrier supports non-containerised messaging.  Alternatively, you can use the \"Deliver Document\" button to send this form as PDF.");
		}

		public void TestRecipientSCACCodeValidation()
		{
			var consol = CreateConsol();

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(shippingInstruction.Recipient.CompanyNameInfo, "Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC (or Type C1C).");

			var cw1Scac = consol.ShippingLine.CustomsCodes.AddNew();
			cw1Scac.OK_CustomsRegNo = "12345";
			cw1Scac.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(shippingInstruction.Recipient.CompanyNameInfo, "Carrier SCAC code must not exceed 4 characters.");

			var scac = consol.ShippingLine.CustomsCodes.AddNew();
			scac.OK_RN_NKCodeCountry = "US";
			scac.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			scac.OK_CustomsRegNo = "漢";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(shippingInstruction.Recipient.CompanyNameInfo, "Most messaging providers do not support non ASCII characters. Please edit Carrier SCAC value under Organisation > Config > Country US, Type CCC (or Type C1C).");

			scac.OK_CustomsRegNo = "1234";
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertNoMessageError(shippingInstruction.Recipient.CompanyNameInfo, "Carrier SCAC is missing from Carrier organization record under Organisation > Config > Country US, Type CCC (or Type C1C).");
			AssertNoMessageError(shippingInstruction.Recipient.CompanyNameInfo, "Carrier SCAC code must not exceed 4 characters.");
			AssertNoMessageError(shippingInstruction.Recipient.CompanyNameInfo, "Most messaging providers do not support non ASCII characters. Please edit Carrier SCAC value under Organisation > Config > Country US, Type CCC (or Type C1C).");
		}

		#endregion

		#region PickupFrom

		public void TestPickupFromAddressValidation()
		{
			const string expectedError = "Pickup from name and address are mandatory when 'Door Pickup' is selected.";

			var consol = CreateConsol();
			consol.Containers[0].JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CY;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.PickupFrom.AddressLine1 = "";

			AssertHasMessageError(shippingInstruction.PickupFrom.CompanyNameInfo, expectedError);

			shippingInstruction.PickupFrom.AddressLine1 = "Line 1";
			AssertNoMessageError(shippingInstruction.PickupFrom.CompanyNameInfo, expectedError);

			consol.Containers[0].JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.PickupFrom.AddressLine1 = "";

			AssertNoMessageError(shippingInstruction.PickupFrom.CompanyNameInfo, expectedError);
		}

		#endregion

		#region DeliverTo

		public void TestDeliverToAddressValidation()
		{
			const string expectedError = "Deliver to name and address are mandatory when 'Door Delivery' is selected.";

			var consol = CreateConsol();
			consol.Containers[0].JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CFS;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.DeliverTo.AddressLine1 = "";

			AssertHasMessageError(shippingInstruction.DeliverTo.CompanyNameInfo, expectedError);

			shippingInstruction.DeliverTo.AddressLine1 = "Line 1";
			AssertNoMessageError(shippingInstruction.DeliverTo.CompanyNameInfo, expectedError);

			consol.Containers[0].JC_DeliveryMode = Constants.DeliveryModes.Codes.CY_CY;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.DeliverTo.AddressLine1 = "";

			AssertNoMessageError(shippingInstruction.DeliverTo.CompanyNameInfo, expectedError);
		}

		#endregion

		#endregion

		#region Tax Numbers

		public void TestBrazilTaxNumbers_Import_RefDataTable()
		{
			PopulateRefData();

			const string expectedError = "CNPJ or CPF number is mandatory for Brazil imports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=BR, Type=CJN or CPF.";

			var consol = CreateConsol("CNSHA", "BRSAO");

			consol.ReceivingForwarderAddress.OA_RN_NKCountryCode = "BR";
			consol.NotifyPartyDocumentaryAddress.Address.OA_RN_NKCountryCode = "BR";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(wrapper.ConsigneeTaxInfo1.Code, "CJN");
			AssertEquals(wrapper.NotifyPartyTaxInfo1.Code, "CJN");
			AssertHasMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertHasMessageError(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			wrapper.Consignee.CompanyName = "My BR Company";
			AssertHasMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertNoMessageError(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			wrapper.Consignee.Country.Name = "Venezuela";
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertHasMessageError(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			var consigneeCPF = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeCPF.OK_RN_NKCodeCountry = "BR";
			consigneeCPF.OK_CodeType = "CPF";
			consigneeCPF.OK_CustomsRegNo = "CPF_23798";

			var notifyPartyCPF = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyCPF.OK_RN_NKCodeCountry = "BR";
			notifyPartyCPF.OK_CodeType = "CPF";
			notifyPartyCPF.OK_CustomsRegNo = "CPF_98989";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertEquals("CPF_23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoMessageError(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);
			AssertEquals("CPF_98989", wrapper.NotifyPartyTaxInfo1.Number);

			var consigneeCJN = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeCJN.OK_RN_NKCodeCountry = "BR";
			consigneeCJN.OK_CodeType = "CJN";
			consigneeCJN.OK_CustomsRegNo = "CJN_23798";

			var notifyPartyCJN = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyCJN.OK_RN_NKCodeCountry = "BR";
			notifyPartyCJN.OK_CodeType = "CJN";
			notifyPartyCJN.OK_CustomsRegNo = "CJN_98989";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertEquals("CJN_23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoMessageError(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);
			AssertEquals("CJN_98989", wrapper.NotifyPartyTaxInfo1.Number);
		}

		public void TestBrazilTaxNumbers_Export_RefDataTable()
		{
			PopulateRefData();

			const string expectedError = "CNPJ or CPF number is mandatory for Brazil exports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=BR, Type=CJN or CPF.";
			var consol = CreateConsol("BRSAO", "AUBNE");

			consol.SendingForwarder.MainAddress.OA_RN_NKCountryCode = "BR";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(wrapper.ShipperTaxInfo1.Code, "CJN");
			AssertEquals(wrapper.ShipperTaxInfo1.Number, "");
			AssertHasMessageError(wrapper.ShipperTaxInfo1.NumberInfo, expectedError);

			var shipperCPF = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperCPF.OK_RN_NKCodeCountry = "BR";
			shipperCPF.OK_CodeType = "CPF";
			shipperCPF.OK_CustomsRegNo = "CPF_23798";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(wrapper.ShipperTaxInfo1.NumberInfo, expectedError);
			AssertEquals("CPF_23798", wrapper.ShipperTaxInfo1.Number);

			var shipperCJN = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperCJN.OK_RN_NKCodeCountry = "BR";
			shipperCJN.OK_CodeType = "CJN";
			shipperCJN.OK_CustomsRegNo = "CJN_23798";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(wrapper.ShipperTaxInfo1.NumberInfo, expectedError);
			AssertEquals("CJN_23798", wrapper.ShipperTaxInfo1.Number);
		}

		public void TestPanamaTaxNumbers_Import_RefDataTable()
		{
			PopulateRefData();

			var refUNLOCO = Factory.New<RefUNLOCO>();
			refUNLOCO.RL_Code = "PACCT";
			refUNLOCO.RL_IATA = "PA";
			Factory.Save();

			var consigneeTaxNumberMessage = "Consignee RUC is required for Panama imports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=PA, Type=RUC.";

			var notifyPartyTaxNumberMessage = "Notify Party RUC is required for Panama imports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=PA, Type=RUC.";

			var consol = CreateConsol("AUSYD", "PACCT");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("RUC", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("RUC", wrapper.NotifyPartyTaxInfo1.Code);

			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.Country.Code = CountryCodes.Belgium;
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			var consigneeRUC = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeRUC.OK_RN_NKCodeCountry = CountryCodes.Panama;
			consigneeRUC.OK_CodeType = PanamaOrgCusCodeInfo.OrgCusCodes.RUC;
			consigneeRUC.OK_CustomsRegNo = "1234";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("1234", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);

			var notifyPartyRUC = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyRUC.OK_RN_NKCodeCountry = CountryCodes.Panama;
			notifyPartyRUC.OK_CodeType = PanamaOrgCusCodeInfo.OrgCusCodes.RUC;
			notifyPartyRUC.OK_CustomsRegNo = "5678";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("5678", wrapper.NotifyPartyTaxInfo1.Number);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);
		}

		public void TestPanamaTaxNumbers_Export_RefDataTable()
		{
			PopulateRefData();

			var refUNLOCO = Factory.New<RefUNLOCO>();
			refUNLOCO.RL_Code = "PACCT";
			refUNLOCO.RL_IATA = "PA";
			Factory.Save();

			var shipperTaxNumberMessage = "Shipper RUC is required for Panama exports.\r\nPlease maintain it in the Organization > Config > Registration Numbers/Codes, Country of Issue=PA, Type=RUC.";

			var consol = CreateConsol("PACCT", "AUSYD");
			consol.SendingForwarder.MainAddress.OA_RN_NKCountryCode = "PA";

			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("RUC", wrapper.ShipperTaxInfo1.Code);
			AssertHasWarning(wrapper.ShipperTaxInfo1.NumberInfo, shipperTaxNumberMessage);

			var shipperCJN = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperCJN.OK_RN_NKCodeCountry = "PA";
			shipperCJN.OK_CodeType = "RUC";
			shipperCJN.OK_CustomsRegNo = "1234";

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("1234", wrapper.ShipperTaxInfo1.Number);
			AssertNoWarning(wrapper.ShipperTaxInfo1.NumberInfo, shipperTaxNumberMessage);
		}

		public void TestGuatemalaTaxNumbers_Import_RefDataTable()
		{
			PopulateRefData();

			var consigneeTaxNumberMessage = "Consignee NIT is required for Guatemala Imports.";
			var notifyPartyTaxNumberMessage = "Notify NIT is required for Guatemala Imports.";

			var consol = CreateConsol("AUSYD", "GTAAZ");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("NIT", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("NIT", wrapper.NotifyPartyTaxInfo1.Code);
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "My GT Company";
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.Country.Code = CountryCodes.Belgium;
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.CompanyName = "My Something Company";

			var consigneeNIT = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeNIT.OK_RN_NKCodeCountry = CountryCodes.Guatemala;
			consigneeNIT.OK_CodeType = OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria;
			consigneeNIT.OK_CustomsRegNo = "23798";

			var notifyPartyNIT = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyNIT.OK_RN_NKCodeCountry = CountryCodes.Guatemala;
			notifyPartyNIT.OK_CodeType = OrgCusCode.GuatemalaCodeTypes.NumeroDeIdentificacionTributaria;
			notifyPartyNIT.OK_CustomsRegNo = "98989";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertEquals("98989", wrapper.NotifyPartyTaxInfo1.Number);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);
		}

		public void TestMoroccoTaxNumbers_Import_RefDataTable()
		{
			PopulateRefData();

			var consigneeTaxNumberMessage = "Consignee ICE is required for Imports to Morocco.";
			var notifyPartyTaxNumberMessage = "Notify ICE is required for Imports to Morocco.";

			var consol = CreateConsol("AUSYD", "MACAS");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("ICE", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("ICE", wrapper.NotifyPartyTaxInfo1.Code);
			AssertNull(wrapper.ShipperTaxInfo1);

			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "My GT Company";
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.Country.Code = CountryCodes.Belgium;
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.CompanyName = "My Something Company";

			var consigneeICE = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeICE.OK_RN_NKCodeCountry = CountryCodes.Morocco;
			consigneeICE.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			consigneeICE.OK_CustomsRegNo = "23798";

			var notifyPartyICE = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyICE.OK_RN_NKCodeCountry = CountryCodes.Morocco;
			notifyPartyICE.OK_CodeType = OrgCusCode.MoroccoCodeTypes.ICE;
			notifyPartyICE.OK_CustomsRegNo = "98989";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertEquals("98989", wrapper.NotifyPartyTaxInfo1.Number);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);
		}

		public void TestMoroccoTaxNumbers_Export_RefDataTable()
		{
			PopulateRefData();

			var consol = CreateConsol("MACAS", "AUSYD");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNull(wrapper.ConsigneeTaxInfo1);
			AssertNull(wrapper.NotifyPartyTaxInfo1);
			AssertNull(wrapper.ShipperTaxInfo1);
		}

		public void TestEgyptTaxNumbers_RefDataTable()
		{
			PopulateRefData();
			var egyptAustraliaGcrBO = Factory.New<RefDocOrgCusCode>();
			egyptAustraliaGcrBO.DOC_DocumentType = "ESI";
			egyptAustraliaGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			egyptAustraliaGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptAustraliaGcrBO.DOC_Notes = "Tax ID";
			egyptAustraliaGcrBO.DOC_CodeType = "GCR";
			egyptAustraliaGcrBO.DOC_ShortLabel = "TEST_SL_AU";
			egyptAustraliaGcrBO.DOC_Priority = 1;

			var egyptBrazilGcrBO = Factory.New<RefDocOrgCusCode>();
			egyptBrazilGcrBO.DOC_DocumentType = "ESI";
			egyptBrazilGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			egyptBrazilGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptBrazilGcrBO.DOC_Notes = "Registration ID";
			egyptBrazilGcrBO.DOC_CodeType = "ACI";
			egyptBrazilGcrBO.DOC_ShortLabel = "TEST_SL_EG";
			egyptBrazilGcrBO.DOC_Priority = 1;

			var egyptGcrBO = Factory.New<RefDocOrgCusCode>();
			egyptGcrBO.DOC_DocumentType = "ESI";
			egyptGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Egypt;
			egyptGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptGcrBO.DOC_Notes = "Registration ID";
			egyptGcrBO.DOC_CodeType = "ACI";
			egyptGcrBO.DOC_ShortLabel = "TEST_SL_EG";
			egyptGcrBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "EGALY");

			consol.ReceivingForwarderAddress.OA_RN_NKCountryCode = "EG";
			consol.NotifyPartyDocumentaryAddress.Address.OA_RN_NKCountryCode = "EG";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "EGALY";

			consol.JK_OA_SendingForwarderAddress = CreateOrgHeaderForCountry("AU").MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = CreateOrgHeaderForCountry("EG").MainAddress.PK;
			consol.NotifyPartyDocumentaryAddress.OrganisationPK = CreateOrgHeaderForCountry("EG").PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(true, wrapper.IsToEgypt);

			AssertEquals("AU", wrapper.PlaceOfReceipt.Country.Code);
			AssertEquals("EG", wrapper.PlaceOfDelivery.Country.Code);

			AssertEquals("AU", wrapper.Shipper.Country.Code);
			AssertEquals("EG", wrapper.NotifyParty.Country.Code);
			AssertEquals("EG", wrapper.Consignee.Country.Code);

			AssertEquals(1, wrapper.ShipperTaxInfo.Count);
			AssertEquals("Exporter Register", wrapper.ShipperTaxInfo1.DisplayedLabel);
			AssertEquals("Commercial Register", wrapper.NotifyPartyTaxInfo1.DisplayedLabel);
			AssertEquals("Commercial Register", wrapper.ConsigneeTaxInfo1.DisplayedLabel);

			AssertEquals(string.Empty, wrapper.ShipperTaxInfo1.Number);
			AssertEquals(string.Empty, wrapper.NotifyPartyTaxInfo1.Number);
			AssertEquals(string.Empty, wrapper.ConsigneeTaxInfo1.Number);

			var consigneeACI = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeACI.OK_RN_NKCodeCountry = "EG";
			consigneeACI.OK_CodeType = "ACI";
			consigneeACI.OK_CustomsRegNo = "23798";

			var notifyPartyACI = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyACI.OK_RN_NKCodeCountry = "EG";
			notifyPartyACI.OK_CodeType = "ACI";
			notifyPartyACI.OK_CustomsRegNo = "98989";

			var shipperGCR = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperGCR.OK_RN_NKCodeCountry = "AU";
			shipperGCR.OK_CodeType = "GCR";
			shipperGCR.OK_CustomsRegNo = "12345";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("ACI", wrapper.NotifyPartyTaxInfo1.Code);
			AssertEquals("98989", wrapper.NotifyPartyTaxInfo1.Number);
			AssertEquals("ACI", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("GCR", wrapper.ShipperTaxInfo1.Code);
			AssertEquals("AU-02-12345", wrapper.ShipperTaxInfo1.Number);
			AssertEquals("EG", wrapper.ShipperTaxInfo1.RegulatingCountry.Code);

			egyptGcrBO.DOC_CodeType = "VAT";
			Factory.Save();
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(true, wrapper.IsToEgypt);
			AssertEquals("VAT Number", wrapper.NotifyPartyTaxInfo1.DisplayedLabel);
			AssertEquals("VAT Number", wrapper.ConsigneeTaxInfo1.DisplayedLabel);

			egyptGcrBO.DOC_CodeType = "COM";
			Factory.Save();
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(true, wrapper.IsToEgypt);
			AssertEquals("Commerical Register", wrapper.NotifyPartyTaxInfo1.DisplayedLabel);
			AssertEquals("Commerical Register", wrapper.ConsigneeTaxInfo1.DisplayedLabel);
		}

		public void TestConsigneeVATNumberValidationForEgypt()
		{
			var errorMessage = "Consignee’s Egyptian Importer VAT Number should contain 9 digits.";

			var egyptBO = Factory.New<RefDocOrgCusCode>();
			egyptBO.DOC_DocumentType = "ESI";
			egyptBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Egypt;
			egyptBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptBO.DOC_Notes = "Tax ID";
			egyptBO.DOC_CodeType = "VAT";
			egyptBO.DOC_Description = "VAT Number";
			egyptBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "EGALY");

			var consigneeVAT = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeVAT.OK_RN_NKCodeCountry = "EG";
			consigneeVAT.OK_CodeType = "VAT";
			consigneeVAT.OK_CustomsRegNo = "123456789";

			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals(true, wrapper.IsToEgypt);
			AssertEquals("VAT", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("123456789", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("VAT Number", wrapper.ConsigneeTaxInfo1.DisplayedLabel);
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, errorMessage);

			consigneeVAT.OK_CustomsRegNo = "123";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, errorMessage);

			consigneeVAT.OK_CustomsRegNo = "1234567890";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, errorMessage);

			consigneeVAT.OK_CustomsRegNo = "889/1A97421";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("889197421", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, errorMessage);

			consigneeVAT.OK_CustomsRegNo = "ABC+*/-";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("", wrapper.ConsigneeTaxInfo1.Number);
			AssertHasMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, errorMessage);

			wrapper.ConsigneeTaxInfo1.Number = "889/1A97421";
			AssertHasMessageError(wrapper.ConsigneeTaxInfo1.NumberInfo, errorMessage);
		}

		public void TestExportRegistrationNumberValidationForEgypt()
		{
			string warningMessage = "To comply with requirements from certain carriers, Shipper’s Exporter Registration Number \r\nshould be a maximum of 17 alphanumerics including special characters.";
			string errorMessage = "Shipper’s Exporter Registration Number should consist of: \r\n- Shipper’s country code \r\n- Type of registration (01-Company Registry, 02-VAT) \r\n- Exporter registration number";
			var egyptAustraliaGcrBO = Factory.New<RefDocOrgCusCode>();
			egyptAustraliaGcrBO.DOC_DocumentType = "ESI";
			egyptAustraliaGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			egyptAustraliaGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Egypt;
			egyptAustraliaGcrBO.DOC_Notes = "Tax ID";
			egyptAustraliaGcrBO.DOC_CodeType = "GCR";
			egyptAustraliaGcrBO.DOC_ShortLabel = "TEST_SL_AU";
			egyptAustraliaGcrBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "EGALY");

			var shipperGCR = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperGCR.OK_RN_NKCodeCountry = "AU";
			shipperGCR.OK_CodeType = "GCR";
			shipperGCR.OK_CustomsRegNo = "12345";

			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals(true, wrapper.IsToEgypt);
			AssertEquals("Exporter Register", wrapper.ShipperTaxInfo1.DisplayedLabel);
			AssertEquals("GCR", wrapper.ShipperTaxInfo1.Code);
			AssertEquals("AU-02-12345", wrapper.ShipperTaxInfo1.Number);
			AssertEquals("EG", wrapper.ShipperTaxInfo1.RegulatingCountry.Code);
			AssertNoMessageError(wrapper.ShipperTaxInfo1.NumberInfo, errorMessage);

			wrapper.ShipperTaxInfo1.Number = "AU-02-12345-123456789";
			AssertHasWarning(wrapper.ShipperTaxInfo1.NumberInfo, warningMessage);

			wrapper.ShipperTaxInfo1.Number = "00-02-12345";
			AssertHasMessageError(wrapper.ShipperTaxInfo1.NumberInfo, errorMessage);

			wrapper.ShipperTaxInfo1.Number = "AU/02/12345";
			AssertHasMessageError(wrapper.ShipperTaxInfo1.NumberInfo, errorMessage);

			wrapper.ShipperTaxInfo1.Number = "AU-09-12345";
			AssertHasMessageError(wrapper.ShipperTaxInfo1.NumberInfo, errorMessage);

			wrapper.ShipperTaxInfo1.Number = "EG-01-12345";
			AssertHasMessageError(wrapper.ShipperTaxInfo1.NumberInfo, errorMessage);

			wrapper.ShipperTaxInfo1.Number = "AU-02-789";
			AssertHasMessageError(wrapper.ShipperTaxInfo1.NumberInfo, errorMessage);
		}

		public void TestVietnamTaxNumbers_RefDataTable()
		{
			const string expectedError = "Company ID, VAT (Government VAT Code) is required to comply with Vietnam Customs notice No. 6889/TCHQ-GSQL";
			PopulateRefData();

			var consol = CreateConsol("AUSYD", "VNHAN");
			consol.JK_OA_ReceivingForwarderAddress = CreateOrgHeaderForCountry("VN").MainAddress.PK;
			consol.NotifyPartyDocumentaryAddress.OrganisationPK = CreateOrgHeaderForCountry("VN").PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			var notifyPartyVAT = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyVAT.OK_RN_NKCodeCountry = "VN";
			notifyPartyVAT.OK_CodeType = "VAT";
			notifyPartyVAT.OK_CustomsRegNo = "99988";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);

			consol.NotifyParty.CustomsCodes.RemoveAll();

			var consigneeVAT = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeVAT.OK_RN_NKCodeCountry = "VN";
			consigneeVAT.OK_CodeType = "VAT";
			consigneeVAT.OK_CustomsRegNo = "38373";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedError);
		}

		public void TestChinaTaxNumbers_RefDataTable()
		{
			PopulateRefData();

			const string expectedWarning_AU = "Company ID, ABN (Australian Business Number (GST Registration Code)) or GCR (Australian Corporation Number (Government Corporation Code))\r\nis required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.";
			const string expectedWarning_CN = "Company ID, GCR (Government Corporation Code) or USC (Unified Social Credit Identifier)\r\nis required to comply with CCAM (China Customs Advanced Manifest) reporting in line with Customs Circular No.56.";

			var consol = CreateConsol();
			consol.Transports[0].JW_RL_NKLoadPort = "AUBNE";
			consol.Transports[2].JW_RL_NKDiscPort = "CNNGB";
			consol.SendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN"; // shipper address

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedWarning_AU);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedWarning_AU);
			AssertHasWarning(wrapper.ShipperTaxInfo1.NumberInfo, expectedWarning_CN);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedWarning_AU);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedWarning_AU);

			wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedWarning_AU);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedWarning_AU);

			var consigneeTaxNumber = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeTaxNumber.OK_RN_NKCodeCountry = "AU";
			consigneeTaxNumber.OK_CodeType = "ABN";
			consigneeTaxNumber.OK_CustomsRegNo = "12345";

			var notifyPartyTaxNumber = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyTaxNumber.OK_RN_NKCodeCountry = "AU";
			notifyPartyTaxNumber.OK_CodeType = "GCR";
			notifyPartyTaxNumber.OK_CustomsRegNo = "27474";

			var shipperTaxNumber = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperTaxNumber.OK_RN_NKCodeCountry = "CN";
			shipperTaxNumber.OK_CodeType = "USC";
			shipperTaxNumber.OK_CustomsRegNo = "343434";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedWarning_AU);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedWarning_AU);
			AssertNoWarning(wrapper.ShipperTaxInfo1.NumberInfo, expectedWarning_CN);
		}

		[TestDate(2021, 10, 1)]
		public void TestIndiaTaxNumbers_RefDataTable()
		{
			const string expectedWarning = "For India, the Consignee or Notify Party requires the IEC and GST numbers together with a contact email address. Ensure this information is included if applicable.";

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1)))
			{
				PopulateRefData();

				var consol = CreateConsol();
				consol.Transports[2].JW_RL_NKDiscPort = "INBOM";
				consol.SendingForwarder.MainAddress.OA_RN_NKCountryCode = "IN";
				consol.ReceivingForwarder.MainAddress.OA_RN_NKCountryCode = "IN";
				consol.NotifyParty.MainAddress.OA_RN_NKCountryCode = "IN";

				var wrapper = new ShippingInstructionBuilder(consol).Build();
				AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedWarning);
				AssertHasWarning(wrapper.Consignee.EmailInfo, expectedWarning);
				AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedWarning);
				AssertHasWarning(wrapper.NotifyParty.EmailInfo, expectedWarning);

				wrapper.Consignee.CompanyName = "TO ORDER";
				AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedWarning);
				AssertHasWarning(wrapper.Consignee.EmailInfo, expectedWarning);
				AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedWarning);
				AssertHasWarning(wrapper.NotifyParty.EmailInfo, expectedWarning);

				wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";
				AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedWarning);
				AssertHasWarning(wrapper.Consignee.EmailInfo, expectedWarning);
				AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, expectedWarning);
				AssertHasWarning(wrapper.NotifyParty.EmailInfo, expectedWarning);
			}
		}

		public void TestIndonesiaTaxNumbers_Import_RefDataTable()
		{
			const string expectedError = "Consignee PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017";

			PopulateRefData();

			var consol = CreateConsol("AUSYD", "IDAJN");

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);

			var consigneeVAT = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeVAT.OK_RN_NKCodeCountry = "ID";
			consigneeVAT.OK_CodeType = "PPN";
			consigneeVAT.OK_CustomsRegNo = "38373";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning("Finds regulating: ID, country of issue: ID", wrapper.ConsigneeTaxInfo1.NumberInfo, expectedError);
		}

		public void TestIndonesiaTaxNumbers_ExportNotToCN_RefDataTable()
		{
			PopulateRefData();

			const string expectedError = "Consignor PPN (NPWP tax identification number) or PAS (Passport) is required to comply with Manifest reporting in line with Regulation No.158/PMK.04/2017";
			Action<CarrierMessageData> assertPerCondition = (data) =>
			{
				Assert(!data.PlaceOfReceipt.IsInCountry(Constants.CountryCodes.China));
				Assert(!data.PlaceOfDelivery.IsInCountry(Constants.CountryCodes.China));
			};

			var consol = CreateConsol();
			consol.JK_RL_NKLoadPort = "IDAJN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = CreateOrgHeaderForCountry("ID").MainAddress.PK;
			consol.Shipments[0].JS_RL_NKOrigin = "IDAJN";
			consol.Transports[0].JW_RL_NKLoadPort = "IDAJN";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert(!wrapper.Shipper.IsEmpty());
			assertPerCondition(wrapper);
			AssertEquals("", wrapper.ShipperTaxInfo1.Number);
			AssertHasWarning(wrapper.ShipperTaxInfo1.NumberInfo, expectedError);

			var shipperPpn = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperPpn.OK_RN_NKCodeCountry = "ID";
			shipperPpn.OK_CodeType = "PPN";
			shipperPpn.OK_CustomsRegNo = "123456";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			assertPerCondition(wrapper);
			AssertEquals("123456", wrapper.ShipperTaxInfo1.Number);
			AssertNoWarning(wrapper.ShipperTaxInfo1.NumberInfo, expectedError);
		}

		public void TestTaxNumberNotInRefTable()
		{
			const string expectedError = "9999 is used for company, but you can modify it to 8888 for an individual";
			const string expectedInvalidLabelError = "Must be 9999 for company, or 8888 for an individual";

			var consol = CreateConsol("CNSHA", "BRSAO");

			consol.ReceivingForwarderAddress.OA_RN_NKCountryCode = "BR";
			consol.NotifyPartyDocumentaryAddress.Address.OA_RN_NKCountryCode = "BR";
			consol.SendingForwarder.MainAddress.OA_RN_NKCountryCode = "BR";

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(1, wrapper.ShipperTaxInfo.Count);
			AssertEquals(1, wrapper.ConsigneeTaxInfo.Count);
			AssertEquals(1, wrapper.NotifyPartyTaxInfo.Count);
			AssertHasWarning(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedError);
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.ShortLabelInfo, expectedError);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.ShortLabelInfo, expectedError);
			AssertNoMessageError(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);
			AssertNoMessageError(wrapper.NotifyPartyTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);

			wrapper.ShipperTaxInfo1.ShortLabel = "8888";
			AssertHasWarning(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedError);
			AssertNoMessageError(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);

			wrapper.ShipperTaxInfo1.ShortLabel = "BLAH";
			AssertNoWarning(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedError);
			AssertHasMessageError(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);

			PopulateRefData();
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedError);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.ShortLabelInfo, expectedError);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.ShortLabelInfo, expectedError);
			AssertNoMessageError(wrapper.ShipperTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);
			AssertNoMessageError(wrapper.ConsigneeTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);
			AssertNoMessageError(wrapper.NotifyPartyTaxInfo1.ShortLabelInfo, expectedInvalidLabelError);
		}

		public void TestNotifyPartyValidation_TaxNumberLength()
		{
			const string taxNumberLengthWarningMessage = @"Company tax ID longer than 26 characters may result in rejection from carriers.
Please maintain proper ID in the Organization > Config > Registration Numbers/Codes tab.";

			var refDocOrgCusCodeGCRBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeGCRBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeGCRBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeGCRBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeGCRBO.DOC_Notes = "Personal Identification";
			refDocOrgCusCodeGCRBO.DOC_CodeType = "GCR";
			refDocOrgCusCodeGCRBO.DOC_ShortLabel = "PersID";
			refDocOrgCusCodeGCRBO.DOC_Priority = 1;

			var refDocOrgCusCodeCOMBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeCOMBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeCOMBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCOMBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCOMBO.DOC_Notes = "Registration ID";
			refDocOrgCusCodeCOMBO.DOC_CodeType = "COM";
			refDocOrgCusCodeCOMBO.DOC_ShortLabel = "TEST_SL_EG";
			refDocOrgCusCodeCOMBO.DOC_Priority = 1;

			var refDocOrgCusCodeCJNBO = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeCJNBO.DOC_DocumentType = "ESI";
			refDocOrgCusCodeCJNBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCJNBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Kenya;
			refDocOrgCusCodeCJNBO.DOC_Notes = "TEST";
			refDocOrgCusCodeCJNBO.DOC_CodeType = "CJN";
			refDocOrgCusCodeCJNBO.DOC_ShortLabel = "TEST_SL_1";
			refDocOrgCusCodeCJNBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "KEMBA");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNoWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);

			var gcrCustomsCode = consol.NotifyPartyDocumentaryAddress.Address.Header.CustomsCodes.AddNew("GCR", "1234567890", Constants.CountryCodes.Kenya);
			var comCustomsCode = consol.NotifyPartyDocumentaryAddress.Address.Header.CustomsCodes.AddNew("COM", "1234567890", Constants.CountryCodes.Kenya);
			var cjnCustomsCode = consol.NotifyPartyDocumentaryAddress.Address.Header.CustomsCodes.AddNew("CJN", "1234567890", Constants.CountryCodes.Kenya);

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNoWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);

			gcrCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";
			comCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";
			cjnCustomsCode.OK_CustomsRegNo = "123456789012345678901234567890";

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertHasWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "GCR").NumberInfo, taxNumberLengthWarningMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "COM").NumberInfo, taxNumberLengthWarningMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo.First(x => x.Code == "CJN").NumberInfo, taxNumberLengthWarningMessage);
		}

		public void TestJordanTaxNumbers_Import_RefDataTable()
		{
			PopulateRefData();

			var consigneeTaxNumberMessage = "Consignee GST is required for Jordan Imports.";
			var notifyPartyTaxNumberMessage = "Notify GST is required for Jordan Imports.";

			var consol = CreateConsol("AUSYD", "JO8AJ");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals(OrgCusCode.CodeTypes.GSTCode, wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals(OrgCusCode.CodeTypes.GSTCode, wrapper.NotifyPartyTaxInfo1.Code);
			AssertNull(wrapper.ShipperTaxInfo1);

			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "My JO Company";
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.Country.Code = CountryCodes.Belgium;
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.Country.Code = CountryCodes.Belgium;
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.Country.Code = CountryCodes.Jordan;
			wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			var consigneeGST = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeGST.OK_RN_NKCodeCountry = CountryCodes.Jordan;
			consigneeGST.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			consigneeGST.OK_CustomsRegNo = "23798";

			var notifyPartyGST = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyGST.OK_RN_NKCodeCountry = CountryCodes.Jordan;
			notifyPartyGST.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
			notifyPartyGST.OK_CustomsRegNo = "98989";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertEquals("98989", wrapper.NotifyPartyTaxInfo1.Number);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);
		}

		public void TestJordanTaxNumbers_Export_RefDataTable()
		{
			PopulateRefData();

			var consol = CreateConsol("JO8AJ", "AUSYD");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNull(wrapper.ConsigneeTaxInfo1);
			AssertNull(wrapper.NotifyPartyTaxInfo1);
			AssertNull(wrapper.ShipperTaxInfo1);
		}

		public void TestSaudiArabiaTaxNumbers_Import_RefDataTable()
		{
			PopulateRefData();

			var consigneeTaxNumberMessage = "Consignee VAT is required for Imports to Saudi Arabia.";
			var notifyPartyTaxNumberMessage = "Notify VAT is required for Imports to Saudi Arabia.";

			var consol = CreateConsol("AUSYD", "SAABT");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals(OrgCusCode.CodeTypes.VATCode, wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals(OrgCusCode.CodeTypes.VATCode, wrapper.NotifyPartyTaxInfo1.Code);
			AssertNull(wrapper.ShipperTaxInfo1);

			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.CompanyName = "My SA Company";
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.Consignee.Country.Code = CountryCodes.Belgium;
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.Country.Code = CountryCodes.Belgium;
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			wrapper.NotifyParty.Country.Code = CountryCodes.SaudiArabia;
			wrapper.NotifyParty.CompanyName = "SAME AS CONSIGNEE";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);

			var consigneeVAT = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeVAT.OK_RN_NKCodeCountry = CountryCodes.SaudiArabia;
			consigneeVAT.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			consigneeVAT.OK_CustomsRegNo = "23798";

			var notifyPartyVAT = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyVAT.OK_RN_NKCodeCountry = CountryCodes.SaudiArabia;
			notifyPartyVAT.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			notifyPartyVAT.OK_CustomsRegNo = "98989";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, consigneeTaxNumberMessage);
			AssertEquals("98989", wrapper.NotifyPartyTaxInfo1.Number);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, notifyPartyTaxNumberMessage);
		}

		public void TestSaudiArabiaTaxNumbers_Export_RefDataTable()
		{
			PopulateRefData();

			var consol = CreateConsol("SAABT", "AUSYD");
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertNull(wrapper.ConsigneeTaxInfo1);
			AssertNull(wrapper.NotifyPartyTaxInfo1);
			AssertNull(wrapper.ShipperTaxInfo1);
		}

		#endregion

		#region TaxInfo

		void PopulateRefData()
		{
			var brazilBOCJN = Factory.New<RefDocOrgCusCode>();
			brazilBOCJN.DOC_DocumentType = "ESI";
			brazilBOCJN.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			brazilBOCJN.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Brazil;
			brazilBOCJN.DOC_Notes = "TEST";
			brazilBOCJN.DOC_CodeType = "CJN";
			brazilBOCJN.DOC_ShortLabel = "TEST_SL_1";
			brazilBOCJN.DOC_Priority = 2;

			var brazilBOCPF = Factory.New<RefDocOrgCusCode>();
			brazilBOCPF.DOC_DocumentType = "ESI";
			brazilBOCPF.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			brazilBOCPF.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Brazil;
			brazilBOCPF.DOC_Notes = "TEST";
			brazilBOCPF.DOC_CodeType = "CPF";
			brazilBOCPF.DOC_ShortLabel = "TEST_SL_1";
			brazilBOCPF.DOC_Priority = 2;

			var refDocOrgCusCodeBO2 = Factory.New<RefDocOrgCusCode>();
			refDocOrgCusCodeBO2.DOC_DocumentType = "ESI";
			refDocOrgCusCodeBO2.DOC_RN_NKCodeCountry = Constants.CountryCodes.Brazil;
			refDocOrgCusCodeBO2.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.UnitedKingdom;
			refDocOrgCusCodeBO2.DOC_Notes = "TEST";
			refDocOrgCusCodeBO2.DOC_CodeType = "ABC";
			refDocOrgCusCodeBO2.DOC_ShortLabel = "TEST_SL_2";
			refDocOrgCusCodeBO2.DOC_Priority = 3;

			var vietnamVatBO = Factory.New<RefDocOrgCusCode>();
			vietnamVatBO.DOC_DocumentType = "ESI";
			vietnamVatBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.VietNam;
			vietnamVatBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.VietNam;
			vietnamVatBO.DOC_Notes = "TEST";
			vietnamVatBO.DOC_CodeType = "VAT";
			vietnamVatBO.DOC_ShortLabel = "TEST_SL_3";
			vietnamVatBO.DOC_Priority = 1;

			var chinaAbnBo = Factory.New<RefDocOrgCusCode>();
			chinaAbnBo.DOC_DocumentType = "ESI";
			chinaAbnBo.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			chinaAbnBo.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			chinaAbnBo.DOC_Notes = "TEST";
			chinaAbnBo.DOC_CodeType = "ABN";
			chinaAbnBo.DOC_ShortLabel = "TEST_SL_10";
			chinaAbnBo.DOC_Priority = 1;

			var chinaGcrBO = Factory.New<RefDocOrgCusCode>();
			chinaGcrBO.DOC_DocumentType = "ESI";
			chinaGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			chinaGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			chinaGcrBO.DOC_Notes = "TEST";
			chinaGcrBO.DOC_CodeType = "GCR";
			chinaGcrBO.DOC_ShortLabel = "TEST_SL_11";
			chinaGcrBO.DOC_Priority = 1;

			var chinaChinaUscBO = Factory.New<RefDocOrgCusCode>();
			chinaChinaUscBO.DOC_DocumentType = "ESI";
			chinaChinaUscBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.China;
			chinaChinaUscBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			chinaChinaUscBO.DOC_Notes = "TEST";
			chinaChinaUscBO.DOC_CodeType = "USC";
			chinaChinaUscBO.DOC_ShortLabel = "TEST_SL_99";
			chinaChinaUscBO.DOC_Priority = 1;

			var chinaChinaGcrBO = Factory.New<RefDocOrgCusCode>();
			chinaChinaGcrBO.DOC_DocumentType = "ESI";
			chinaChinaGcrBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.China;
			chinaChinaGcrBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			chinaChinaGcrBO.DOC_Notes = "TEST";
			chinaChinaGcrBO.DOC_CodeType = "GCR";
			chinaChinaGcrBO.DOC_ShortLabel = "TEST_SL_12";
			chinaChinaGcrBO.DOC_Priority = 1;

			var indiaIecBO = Factory.New<RefDocOrgCusCode>();
			indiaIecBO.DOC_DocumentType = "ESI";
			indiaIecBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			indiaIecBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.India;
			indiaIecBO.DOC_Notes = "TEST";
			indiaIecBO.DOC_CodeType = "IEC";
			indiaIecBO.DOC_ShortLabel = "USC";
			indiaIecBO.DOC_Description = "Unified Social Credit Identifier";
			indiaIecBO.DOC_Priority = 1;

			var indiaGstBO = Factory.New<RefDocOrgCusCode>();
			indiaGstBO.DOC_DocumentType = "ESI";
			indiaGstBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			indiaGstBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.India;
			indiaGstBO.DOC_Notes = "TEST";
			indiaGstBO.DOC_CodeType = "GST";
			indiaGstBO.DOC_ShortLabel = "GST";
			indiaGstBO.DOC_Description = "Goods and Services Tax";
			indiaGstBO.DOC_Priority = 1;

			var indiaPanBO = Factory.New<RefDocOrgCusCode>();
			indiaPanBO.DOC_DocumentType = "ESI";
			indiaPanBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.India;
			indiaPanBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.India;
			indiaPanBO.DOC_Notes = "TEST";
			indiaPanBO.DOC_CodeType = "PAN";
			indiaPanBO.DOC_ShortLabel = "PAN";
			indiaPanBO.DOC_Description = "PAN";
			indiaPanBO.DOC_Priority = 1;

			var indonesiaPpnBO = Factory.New<RefDocOrgCusCode>();
			indonesiaPpnBO.DOC_DocumentType = "ESI";
			indonesiaPpnBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Indonesia;
			indonesiaPpnBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Indonesia;
			indonesiaPpnBO.DOC_Notes = "TEST";
			indonesiaPpnBO.DOC_CodeType = "PPN";
			indonesiaPpnBO.DOC_ShortLabel = "TEST_SL_15";
			indonesiaPpnBO.DOC_Priority = 1;

			var indonesiaChinaGcr = Factory.New<RefDocOrgCusCode>();
			indonesiaChinaGcr.DOC_DocumentType = "ESI";
			indonesiaChinaGcr.DOC_RN_NKCodeCountry = Constants.CountryCodes.Indonesia;
			indonesiaChinaGcr.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			indonesiaChinaGcr.DOC_Notes = "TEST";
			indonesiaChinaGcr.DOC_CodeType = "GCR";
			indonesiaChinaGcr.DOC_ShortLabel = "TEST_SL_16";
			indonesiaChinaGcr.DOC_Priority = 1;

			var nzChinaGcr = Factory.New<RefDocOrgCusCode>();
			nzChinaGcr.DOC_DocumentType = "ESI";
			nzChinaGcr.DOC_RN_NKCodeCountry = Constants.CountryCodes.NewZealand;
			nzChinaGcr.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.China;
			nzChinaGcr.DOC_Notes = "TEST";
			nzChinaGcr.DOC_CodeType = "GCR";
			nzChinaGcr.DOC_ShortLabel = "TEST_SL_17";
			nzChinaGcr.DOC_Priority = 1;

			var gbEorBO = Factory.New<RefDocOrgCusCode>();
			gbEorBO.DOC_DocumentType = "ESI";
			gbEorBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			gbEorBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.UnitedKingdom;
			gbEorBO.DOC_Notes = "TEST";
			gbEorBO.DOC_CodeType = "EOR";
			gbEorBO.DOC_ShortLabel = "EORI";
			gbEorBO.DOC_Priority = 2;

			var gbExtraBO = Factory.New<RefDocOrgCusCode>();
			gbExtraBO.DOC_DocumentType = "ESI";
			gbExtraBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.UnitedKingdom;
			gbExtraBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.UnitedKingdom;
			gbExtraBO.DOC_Notes = "TEST";
			gbExtraBO.DOC_CodeType = "GCR";
			gbExtraBO.DOC_ShortLabel = "GCR";
			gbExtraBO.DOC_Priority = 1;

			var gtNitBO = Factory.New<RefDocOrgCusCode>();
			gtNitBO.DOC_DocumentType = "ESI";
			gtNitBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Guatemala;
			gtNitBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Guatemala;
			gtNitBO.DOC_Notes = "TEST";
			gtNitBO.DOC_CodeType = "NIT";
			gtNitBO.DOC_ShortLabel = "NIT";
			gtNitBO.DOC_Priority = 1;

			var paRUCBO = Factory.New<RefDocOrgCusCode>();
			paRUCBO.DOC_DocumentType = "ESI";
			paRUCBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Panama;
			paRUCBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Panama;
			paRUCBO.DOC_Notes = "TEST";
			paRUCBO.DOC_CodeType = "RUC";
			paRUCBO.DOC_ShortLabel = "RUC";
			paRUCBO.DOC_Priority = 1;

			var moICEBO = Factory.New<RefDocOrgCusCode>();
			moICEBO.DOC_DocumentType = "ESI";
			moICEBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Morocco;
			moICEBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Morocco;
			moICEBO.DOC_Notes = "TEST";
			moICEBO.DOC_CodeType = "ICE";
			moICEBO.DOC_ShortLabel = "ICE";
			moICEBO.DOC_Priority = 1;
			moICEBO.DOC_Direction = "IMP";

			var joICEBO = Factory.New<RefDocOrgCusCode>();
			joICEBO.DOC_DocumentType = "ESI";
			joICEBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Jordan;
			joICEBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Jordan;
			joICEBO.DOC_Notes = "TEST";
			joICEBO.DOC_CodeType = OrgCusCode.CodeTypes.GSTCode;
			joICEBO.DOC_ShortLabel = "GST";
			joICEBO.DOC_Priority = 1;
			joICEBO.DOC_Direction = "IMP";

			var saVATBO = Factory.New<RefDocOrgCusCode>();
			saVATBO.DOC_DocumentType = "ESI";
			saVATBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.SaudiArabia;
			saVATBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.SaudiArabia;
			saVATBO.DOC_Notes = "TEST";
			saVATBO.DOC_CodeType = "VAT";
			saVATBO.DOC_ShortLabel = "VAT";
			saVATBO.DOC_Priority = 1;
			saVATBO.DOC_Direction = "IMP";

			Factory.Save();
		}

		public void TestConsingeeValidation_IsraelImports_RequiredTaxNumber()
		{
			var israelConsigneeTaxNumberMessage = @"Consignee VAT Number is required for imports to Israel to comply with Manifest reporting.";
			var israelNotifyPartyTaxNumberMessage = @"Notify Party VAT Number is required when Consignee is ‘TO ORDER’ or its address is not in Israel for imports to Israel to comply with Manifest reporting.";

			var israelBO = Factory.New<RefDocOrgCusCode>();
			israelBO.DOC_DocumentType = "ESI";
			israelBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.Israel;
			israelBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Israel;
			israelBO.DOC_Notes = "TEST";
			israelBO.DOC_CodeType = "VAT";
			israelBO.DOC_ShortLabel = "VAT";
			israelBO.DOC_Priority = 1;

			Factory.Save();

			var consol = CreateConsol("AUSYD", "IL8UH");

			consol.ReceivingForwarderAddress.OA_RN_NKCountryCode = "IL";
			consol.NotifyPartyDocumentaryAddress.Address.OA_RN_NKCountryCode = "IL";

			var consigneeIL = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeIL.OK_RN_NKCodeCountry = "IL";
			consigneeIL.OK_CodeType = "VAT";
			consigneeIL.OK_CustomsRegNo = "11111";

			var notifyPartyIL = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyIL.OK_RN_NKCodeCountry = "IL";
			notifyPartyIL.OK_CodeType = "VAT";
			notifyPartyIL.OK_CustomsRegNo = "22222";

			var wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("Tax Info Country Code", "IL", wrapper.ConsigneeTaxInfo1.Country.Code);
			AssertEquals("Tax Info Regulation Country Code", "IL", wrapper.ConsigneeTaxInfo1.RegulatingCountry.Code);
			AssertEquals("Tax Info Country Code", "IL", wrapper.NotifyPartyTaxInfo1.Country.Code);
			AssertEquals("Tax Info Regulation Country Code", "IL", wrapper.NotifyPartyTaxInfo1.RegulatingCountry.Code);
			AssertEquals("11111", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("22222", wrapper.NotifyPartyTaxInfo1.Number);

			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, israelConsigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, israelNotifyPartyTaxNumberMessage);
			wrapper.NotifyPartyTaxInfo1.Number = string.Empty;
			wrapper.Consignee.CompanyName = "TO ORDER";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, israelConsigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, israelNotifyPartyTaxNumberMessage);

			consol = CreateConsol("AUSYD", "IL8UH");
			wrapper = new ShippingInstructionBuilder(consol).Build();

			wrapper.ConsigneeTaxInfo1.Number = string.Empty;
			wrapper.NotifyPartyTaxInfo1.Number = string.Empty;
			AssertHasWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, israelConsigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, israelNotifyPartyTaxNumberMessage);

			wrapper.ConsigneeTaxInfo1.Number = "11111";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, israelConsigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, israelNotifyPartyTaxNumberMessage);

			wrapper.ConsigneeTaxInfo1.Country.Code = "CN";
			wrapper.NotifyPartyTaxInfo1.Country.Code = "IL";
			wrapper.NotifyPartyTaxInfo1.Number = string.Empty;
			AssertEquals("Tax Info Country Code", "CN", wrapper.ConsigneeTaxInfo1.Country.Code);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, israelConsigneeTaxNumberMessage);
			AssertHasWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, israelNotifyPartyTaxNumberMessage);

			wrapper.NotifyPartyTaxInfo1.Number = "22222";
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, israelConsigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, israelNotifyPartyTaxNumberMessage);

			consol = CreateConsol("AUSYD", "IL8UH");
			consigneeIL = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeIL.OK_RN_NKCodeCountry = "IL";
			consigneeIL.OK_CodeType = "VAT";
			consigneeIL.OK_CustomsRegNo = "11111";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(string.Empty, wrapper.NotifyPartyTaxInfo1.Number);
			AssertNoWarning(wrapper.ConsigneeTaxInfo1.NumberInfo, israelConsigneeTaxNumberMessage);
			AssertNoWarning(wrapper.NotifyPartyTaxInfo1.NumberInfo, israelNotifyPartyTaxNumberMessage);
		}

		public void TestTaxInfoPopulatesCorrectly()
		{
			PopulateRefData();
			var consol = CreateConsol("BRSAO", "GBLON");

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("BR", wrapper.PlaceOfReceipt.Country.Code);
			AssertEquals("GB", wrapper.PlaceOfDelivery.Country.Code);

			AssertEquals("BR", wrapper.Shipper.Country.Code);
			AssertEquals("GB", wrapper.NotifyParty.Country.Code);
			AssertEquals("GB", wrapper.Consignee.Country.Code);

			AssertEquals(3, wrapper.ShipperTaxInfo.Count);
			AssertEquals("TEST_SL_1", wrapper.ShipperTaxInfo1.ShortLabel);
			AssertEquals("GCR", wrapper.NotifyPartyTaxInfo1.ShortLabel);
			AssertEquals("GCR", wrapper.ConsigneeTaxInfo1.ShortLabel);
			AssertEquals("", wrapper.NotifyPartyTaxInfo1.Number);
			AssertEquals("", wrapper.ConsigneeTaxInfo1.Number);

			var consigneeGCR = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeGCR.OK_RN_NKCodeCountry = "GB";
			consigneeGCR.OK_CodeType = "GCR";
			consigneeGCR.OK_CustomsRegNo = "23798";

			var notifyPartyGCR = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyGCR.OK_RN_NKCodeCountry = "GB";
			notifyPartyGCR.OK_CodeType = "GCR";
			notifyPartyGCR.OK_CustomsRegNo = "98989";

			var shipperCJN = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperCJN.OK_RN_NKCodeCountry = "BR";
			shipperCJN.OK_CodeType = "CJN";
			shipperCJN.OK_CustomsRegNo = "12345";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GCR", wrapper.NotifyPartyTaxInfo1.Code);
			AssertEquals("98989", wrapper.NotifyPartyTaxInfo1.Number);
			AssertEquals("GCR", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("CJN", wrapper.ShipperTaxInfo1.Code);
			AssertEquals("12345", wrapper.ShipperTaxInfo1.Number);
		}

		public void TestTaxInfoPopulatesWithOtherCountry()
		{
			PopulateRefData();

			var indonesiaPpnBO = Factory.New<RefDocOrgCusCode>();
			indonesiaPpnBO.DOC_DocumentType = "ESI";
			indonesiaPpnBO.DOC_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
			indonesiaPpnBO.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.UnitedStates;
			indonesiaPpnBO.DOC_Notes = "TEST";
			indonesiaPpnBO.DOC_CodeType = "PPN";
			indonesiaPpnBO.DOC_ShortLabel = "TEST_SL_US";
			indonesiaPpnBO.DOC_Priority = 1;
			Factory.Save();

			var consol = CreateConsol("IDBAH", "USLAX");

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("ID", wrapper.PlaceOfReceipt.Country.Code);
			AssertEquals("US", wrapper.PlaceOfDelivery.Country.Code);

			AssertEquals("ID", wrapper.Shipper.Country.Code);
			AssertEquals("US", wrapper.NotifyParty.Country.Code);
			AssertEquals("US", wrapper.Consignee.Country.Code);

			AssertEquals(1, wrapper.ShipperTaxInfo.Count);
			AssertEquals("TEST_SL_US", wrapper.NotifyPartyTaxInfo1.ShortLabel);
			AssertEquals("TEST_SL_US", wrapper.ConsigneeTaxInfo1.ShortLabel);
			AssertEquals("TEST_SL_15", wrapper.ShipperTaxInfo1.ShortLabel);
			AssertEquals("", wrapper.NotifyPartyTaxInfo1.Number);
			AssertEquals("", wrapper.ConsigneeTaxInfo1.Number);
		}

		public void TestTaxInfo_ShipperFallback()
		{
			PopulateRefData();
			var consol = CreateConsol();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "CNNJI";
			consol.JK_OA_SendingForwarderAddress = CreateOrgHeaderForCountry("NZ").MainAddress.PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(1, wrapper.ShipperTaxInfo.Count);
			AssertEquals("GCR", wrapper.ShipperTaxInfo1.Code);
			AssertEquals("empty number from fallback (DiscPort in china)", "", wrapper.ShipperTaxInfo1.Number);

			var shipperCJN = consol.SendingForwarder.CustomsCodes.AddNew();
			shipperCJN.OK_RN_NKCodeCountry = "NZ";
			shipperCJN.OK_CodeType = "GCR";
			shipperCJN.OK_CustomsRegNo = "12345";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GCR", wrapper.ShipperTaxInfo1.Code);
			AssertEquals("populated number from fallback (DiscPort in china)", "12345", wrapper.ShipperTaxInfo1.Number);
		}

		public void TestTaxInfo_ConsigneeAndNotifyPartyFallback()
		{
			PopulateRefData();
			var consol = CreateConsol();
			consol.JK_RL_NKLoadPort = "CNNJI";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_ReceivingForwarderAddress = CreateOrgHeaderForCountry("NZ").MainAddress.PK;
			consol.NotifyPartyDocumentaryAddress.OrganisationPK = CreateOrgHeaderForCountry("NZ").PK;

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(1, wrapper.NotifyPartyTaxInfo.Count);
			AssertEquals("GCR", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("GCR", wrapper.NotifyPartyTaxInfo1.Code);
			AssertEquals("empty number from fallback (LoadPort in china)", "", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("empty number from fallback (LoadPort in china)", "", wrapper.NotifyPartyTaxInfo1.Number);

			var consigneeCJN = consol.ReceivingForwarder.CustomsCodes.AddNew();
			consigneeCJN.OK_RN_NKCodeCountry = "NZ";
			consigneeCJN.OK_CodeType = "GCR";
			consigneeCJN.OK_CustomsRegNo = "23798";

			var notifyPartyCJN = consol.NotifyParty.CustomsCodes.AddNew();
			notifyPartyCJN.OK_RN_NKCodeCountry = "NZ";
			notifyPartyCJN.OK_CodeType = "GCR";
			notifyPartyCJN.OK_CustomsRegNo = "98989";

			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("GCR", wrapper.ConsigneeTaxInfo1.Code);
			AssertEquals("GCR", wrapper.NotifyPartyTaxInfo1.Code);
			AssertEquals("populated number from fallback (PortOfDischarge)", "23798", wrapper.ConsigneeTaxInfo1.Number);
			AssertEquals("populated number from fallback (PortOfDischarge)", "98989", wrapper.NotifyPartyTaxInfo1.Number);
		}

		public OrgHeader CreateOrgHeaderForCountry(ZString countryCode)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "I'm Sending Stuff";
			orgHeader.OH_RL_NKClosestPort = "CNNJI";
			orgHeader.MainAddress.Address1 = "Unit 200";
			orgHeader.MainAddress.Address2 = "55 Why Lane";
			orgHeader.MainAddress.City = "Conficious Ave";
			orgHeader.MainAddress.Postcode = "10000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = countryCode;

			var newContact = Factory.New<OrgContact>();
			newContact.OC_OH = orgHeader.PK;
			newContact.OC_ContactName = "Sender Name";
			newContact.OC_Email = "name@sender.com";
			newContact.OC_Phone = "1111111";
			newContact.OC_Fax = "2222222";

			//consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			return orgHeader;
		}

		#endregion

		public void TestValidateCTKNumber()
		{
			var warningMessage = @"The CTK - Cargo Tracking Note number is required for cargo destined to Ghana.
Please enter CTK in either Consol > Details > Numbers > Reference Numbers,
Or in Shipment (S000001) > Additional Details > Reference Numbers.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "GHACC";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S000001";
			shipment.JS_RL_NKDestination = "GHACC";

			var builder = CreateDocDataObjectBuilder(consol);
			var carrierMessageData = builder.Build();
			var shipmentDO = carrierMessageData.Shipments.First();

			AssertHasWarning(shipmentDO.CTKNumberInfo, warningMessage);

			var ctkNumber = shipment.Numbers.AddNew();
			ctkNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
			ctkNumber.CE_EntryNum = "0001";
			ctkNumber.CE_RN_NKCountryCode = "GH";

			builder = CreateDocDataObjectBuilder(consol);
			carrierMessageData = builder.Build();
			shipmentDO = carrierMessageData.Shipments.First();

			AssertNoWarning(shipmentDO.CTKNumberInfo, warningMessage);
		}

		public void TestValidateITNNumber()
		{
			var warningMessageForIllegalFormat = "Please enter valid ITN number(s), the number must start with the letter \"X\", \r\nfollowed by the year, month and day of acceptance in the AES, and six randomly assigned digits.";
			var errorMessageForExceedLength = "Number with over 35 characters will not be accepted by the carrier.";

			var consol = CreateConsol();
			var shipment = consol.Shipments[0];
			shipment.JS_UniqueConsignRef = "S000001";
			consol.Transports[0].JW_RL_NKLoadPort = "USCHI";

			var itnNumber = shipment.CusEntryNumbers.AddNew();
			itnNumber.CE_EntryType = "ITN";
			itnNumber.CE_EntryNum = "X202310204444";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			var shipmentDO = shippingInstruction.Shipments.First();
			AssertNoMessageError(shipmentDO.ITNNumberInfo, errorMessageForExceedLength);
			AssertHasWarning(shipmentDO.ITNNumberInfo, warningMessageForIllegalFormat);

			shipmentDO.ITNNumber = "A20231020666666";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoMessageError(shipmentDO.ITNNumberInfo, errorMessageForExceedLength);
			AssertHasWarning(shipmentDO.ITNNumberInfo, warningMessageForIllegalFormat);

			shipmentDO.ITNNumber = "X20231020A55555";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoMessageError(shipmentDO.ITNNumberInfo, errorMessageForExceedLength);
			AssertHasWarning(shipmentDO.ITNNumberInfo, warningMessageForIllegalFormat);

			shipmentDO.ITNNumber = "X20231032666666";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoMessageError(shipmentDO.ITNNumberInfo, errorMessageForExceedLength);
			AssertHasWarning(shipmentDO.ITNNumberInfo, warningMessageForIllegalFormat);

			shipmentDO.ITNNumber = "X20231020666666,X20231021666666,A20231022666666";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoMessageError(shipmentDO.ITNNumberInfo, errorMessageForExceedLength);
			AssertHasWarning(shipmentDO.ITNNumberInfo, warningMessageForIllegalFormat);

			shipmentDO.ITNNumber = "X20231020666666,X20231021666666,X202310226666666666666666666666666666666666";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoWarning(shipmentDO.ITNNumberInfo, warningMessageForIllegalFormat);
			AssertHasMessageError(shipmentDO.ITNNumberInfo, errorMessageForExceedLength);
		}

		public void TestIncludeHbl()
		{
			var consol = CreateConsol();
			var transport = consol.Transports.LastTransportWithTransportMode(Constants.TransportModes.Sea);
			transport.JW_RL_NKDiscPort = "USLAX";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.IncludeHbl = true;
			shippingInstruction.USCanadaManifestSelfFilerID = "";
			AssertHasWarning(shippingInstruction.IncludeHblInfo, "House Bills will be included with electronic transmission only. Not every carrier/NVOCC can process House Bill details even if they are submitted electronically. Please consult carrier directly for more information.");
			AssertNoMessageError(shippingInstruction.IncludeHblInfo, "House Bills will be included with electronic transmission only. Not every carrier/NVOCC can process House Bill details even if they are submitted electronically. Please consult carrier directly for more information.");

			shippingInstruction.USCanadaManifestSelfFilerID = "AA";
			AssertHasMessageError(shippingInstruction.IncludeHblInfo, "House Bills can only be included if the carrier is to file US/Canada Manifest.");
		}

		#endregion

		#region Booking Reference

		public void TestBookingReference()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = "AGT";
			consol.JK_BookingReference = "BookRef";
			consol.JK_CoLoadBookingReference = "CLDBookRef";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("BKG Ref for AGT consol", "BookRef", shippingInstruction.BookingReference);
			AssertNoMessageError(shippingInstruction.BookingReferenceInfo, "At least one Carrier Booking Reference number is required.");

			shippingInstruction.BookingReference = "";
			AssertHasMessageError(shippingInstruction.BookingReferenceInfo, "At least one Carrier Booking Reference number is required.");

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadBookingReference = "CLDBookRef";
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("BKG Ref for CLD consol", "CLDBookRef", shippingInstruction.CoLoadBookingReference);
			AssertNoMessageError(shippingInstruction.CoLoadBookingReferenceInfo, "At least one Co-Loader Booking Reference number is required.");

			shippingInstruction.CoLoadBookingReference = "";
			AssertHasMessageError(shippingInstruction.CoLoadBookingReferenceInfo, "At least one Co-Loader Booking Reference number is required.");

			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("BKG Ref for GCL consol", "BookRef", shippingInstruction.BookingReference);
			AssertNoMessageError(shippingInstruction.BookingReferenceInfo, "At least one Gateway Co-Loader Booking Reference number is required.");

			shippingInstruction.BookingReference = "";
			AssertHasMessageError(shippingInstruction.BookingReferenceInfo, "At least one Gateway Co-Loader Booking Reference number is required.");
		}

		#endregion

		#region Release Type

		public void TestReleaseType()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = "CLD";
			consol.JK_ReleaseType = "CAD";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BOL", shippingInstruction.ReleaseType.Code);
				AssertEquals("BOL Original", shippingInstruction.ReleaseType.Description);
			});

			consol.JK_AgentType = "DRT";
			consol.DirectShipment.JS_ReleaseType = "SWB";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("SWB", shippingInstruction.ReleaseType.Code);
				AssertEquals("Sea Waybill", shippingInstruction.ReleaseType.Description);
			});

			consol.DirectShipment.JS_ReleaseType = "CAD";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("BOL", shippingInstruction.ReleaseType.Code);
				AssertEquals("BOL Original", shippingInstruction.ReleaseType.Description);
			});

			consol.JK_AgentType = "AGT";
			consol.JK_ReleaseType = "EBL";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals("SWB", shippingInstruction.ReleaseType.Code);
				AssertEquals("Sea Waybill", shippingInstruction.ReleaseType.Description);
			});

			consol.JK_ReleaseType = "NON";

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "TO ORDER";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("BOL", shippingInstruction.ReleaseType.Code);

			consol.JK_AgentType = "DRT";
			consol.DirectShipment.JS_ReleaseType = "SWB";
			consol.DirectShipment.ConsigneeDocumentaryAddress.E2_OA_Address = receivingForwarder.MainAddress.PK;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals("BOL", shippingInstruction.ReleaseType.Code);
		}

		#endregion

		#region Number Of Originals and Copies

		public void TestPopulateNumberOfOriginalsAndCopies()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = "CLD";
			consol.JK_ReleaseType = "CAD";
			consol.JK_NoOriginalBills = 0;
			consol.JK_NoCopyBills = 1;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals(0, shippingInstruction.NumberOfOriginals);
				AssertEquals(1, shippingInstruction.NumberOfCopies);
			});

			consol.JK_ReleaseType = "NON";
			consol.ReceivingForwarderAddress.Header.OH_FullName = "TO ORDER";
			consol.JK_NoOriginalBills = 3;
			consol.JK_NoCopyBills = 2;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals(3, shippingInstruction.NumberOfOriginals);
				AssertEquals(2, shippingInstruction.NumberOfCopies);
			});

			consol.JK_AgentType = "DRT";
			consol.DirectShipment.JS_ReleaseType = "SWB";
			consol.DirectShipment.JS_NoOriginalBills = 2;
			consol.DirectShipment.JS_NoCopyBills = 3;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals(3, shippingInstruction.NumberOfOriginals);
				AssertEquals(3, shippingInstruction.NumberOfCopies);
			});

			consol.DirectShipment.JS_ReleaseType = "CAD";
			consol.DirectShipment.JS_NoOriginalBills = 2;
			consol.DirectShipment.JS_NoCopyBills = 3;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals(2, shippingInstruction.NumberOfOriginals);
				AssertEquals(3, shippingInstruction.NumberOfCopies);
			});

			consol.JK_AgentType = "AGT";
			consol.JK_ReleaseType = "EBL";
			consol.JK_NoOriginalBills = 0;
			consol.JK_NoCopyBills = 1;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			CombineAssertions(() =>
			{
				AssertEquals(1, shippingInstruction.NumberOfOriginals);
				AssertEquals(1, shippingInstruction.NumberOfCopies);
			});
		}

		#endregion

		#region Place and Date of Issue

		public void TestPlaceAndDateOfIssue()
		{
			const string expectedDateOfIssueError = "Date of Issue is required if Place of Issue is entered.";
			const string expectedPlaceOfIssueError = "Place of Issue is required if Date of Issue is entered.";

			var consol = CreateConsol();
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("DKAAL", shippingInstruction.PlaceOfIssue.Code);
			AssertEquals(new ZDateTime(2018, 10, 1), shippingInstruction.DateOfIssue);

			AssertNoMessageError(shippingInstruction.PlaceOfIssue.CodeInfo, expectedPlaceOfIssueError);
			AssertNoMessageError(shippingInstruction.DateOfIssueInfo, expectedDateOfIssueError);

			shippingInstruction.PlaceOfIssue.Code = "";
			AssertHasMessageError(shippingInstruction.PlaceOfIssue.CodeInfo, expectedPlaceOfIssueError);
			AssertNoMessageError(shippingInstruction.DateOfIssueInfo, expectedDateOfIssueError);

			shippingInstruction.PlaceOfIssue.Code = "AUSYD";
			shippingInstruction.DateOfIssue = ZDateTime.Invalid;
			AssertNoMessageError(shippingInstruction.PlaceOfIssue.CodeInfo, expectedPlaceOfIssueError);
			AssertHasMessageError(shippingInstruction.DateOfIssueInfo, expectedDateOfIssueError);

			shippingInstruction.PlaceOfIssue.Code = "DKAAL";
			shippingInstruction.DateOfIssue = ZDateTime.Empty;
			AssertNoMessageError(shippingInstruction.PlaceOfIssue.CodeInfo, expectedPlaceOfIssueError);
			AssertHasMessageError(shippingInstruction.DateOfIssueInfo, expectedDateOfIssueError);

			shippingInstruction.PlaceOfIssue.Code = "";
			shippingInstruction.DateOfIssue = ZDateTime.Empty;
			AssertNoMessageError(shippingInstruction.PlaceOfIssue.CodeInfo, expectedPlaceOfIssueError);
			AssertNoMessageError(shippingInstruction.DateOfIssueInfo, expectedDateOfIssueError);
		}

		#endregion

		#region Ports

		public void TestPorts()
		{
			var consol = CreateConsol();
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.PortOfLoading.Code, shippingInstruction.PortOfLoading.CodeInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(shippingInstruction.PortOfLoading.CodeInfo, "Port of Loading is required.");
			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.PortOfDischarge.Code, shippingInstruction.PortOfDischarge.CodeInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(shippingInstruction.PortOfDischarge.CodeInfo, "Port of Discharge is required.");
			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.Origin.Code, shippingInstruction.Origin.CodeInfo);
			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.Destination.Code, shippingInstruction.Destination.CodeInfo);
			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.PlaceOfReceipt.Code, shippingInstruction.PlaceOfReceipt.CodeInfo);
			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.PlaceOfDelivery.Code, shippingInstruction.PlaceOfDelivery.CodeInfo);
			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.PlaceOfIssue.Code, shippingInstruction.PlaceOfIssue.CodeInfo);
		}

		#endregion

		#region Container Validation

		public void TestContainerTypeValidation()
		{
			var consol = CreateConsol();
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.ContainerMode.Code,
				shippingInstruction.ContainerMode.CodeInfo);

			var unsupportedContainerTypes = new List<string> { Constants.ContainerModes.BreakBulk, Constants.ContainerModes.Liquid, Constants.ContainerModes.Bulk, Constants.ContainerModes.RollOnRollOff };

			foreach (var unsupportedContainerType in unsupportedContainerTypes)
			{
				shippingInstruction.ContainerMode.Code = unsupportedContainerType;
				AssertHasWarning(shippingInstruction.ContainerMode.CodeInfo, "Carrier may not support this type of consol.");
			}

			var supportedContainerTypes = new List<string> { Constants.ContainerModes.FCL, Constants.ContainerModes.LCL, Constants.ContainerModes.BuyersConsol, Constants.ContainerModes.Other };

			foreach (var supportedContainerType in supportedContainerTypes)
			{
				shippingInstruction.ContainerMode.Code = supportedContainerType;
				AssertNoWarning(shippingInstruction.ContainerMode.CodeInfo, "Carrier may not support this type of consol.");
			}
		}

		public void TestContainerVGMValidation()
		{
			var warning = "VGM details on this form are for information, print, fax or email only. VGM information to participating carriers should be transmitted separately as per Verified Gross Container Weight form or according to local authority requirements.";
			var consol = CreateConsol();
			var container = consol.Containers[0];

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertHasWarning(shippingInstruction.Containers.First().VerifiedMethod.DescriptionInfo, warning);

			container.JC_GrossWeightVerificationType = string.Empty;
			consol.JK_ConsolMode = "BBK";

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(shippingInstruction.Containers.First().VerifiedMethod.DescriptionInfo, warning);
		}

		public void TestContainerNumberNoPacksValidation()
		{
			var noPacksMessageError = "There are no packs in this container.";
			var consol = CreateConsol();

			var container = (ForwardingContainer)consol.Containers.First();
			var shipment = (ForwardingShipment)consol.Shipments.First();
			shipment.OuterPackLines.RemoveAndDeleteAll();

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var containerDO = carrierMessageData.Containers.Cast<Container>().First();
			AssertHasMessageError(containerDO.NumberInfo, noPacksMessageError);

			void RefreshContainer()
			{
				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				containerDO = carrierMessageData.Containers.Cast<Container>().First();
			}

			container.JC_IsEmptyContainer = true;
			RefreshContainer();
			AssertHasMessageError("The container has been marked as empty", containerDO.NumberInfo, noPacksMessageError);

			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			RefreshContainer();
			AssertNoMessageError("None Containerized", containerDO.NumberInfo, noPacksMessageError);

			container.JC_IsEmptyContainer = false;
			var packline = shipment.OuterPackLines.AddNew();
			container.PackLines.Add(packline);

			RefreshContainer();
			AssertNoMessageError("There are packs in the container", containerDO.NumberInfo, noPacksMessageError);
		}

		public void TestContainerNumberMandatoryValidation()
		{
			var mandatoryMessageError = "Please enter a Container Number.";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var container = consol.Containers.AddNew();

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var containerDO = carrierMessageData.Containers.Cast<Container>().First();
			AssertHasMessageError(containerDO.NumberInfo, mandatoryMessageError);

			void RefreshContainer()
			{
				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				containerDO = carrierMessageData.Containers.Cast<Container>().First();
			}

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			RefreshContainer();
			AssertHasMessageError(containerDO.NumberInfo, mandatoryMessageError);

			container.JC_ContainerNum = "EREQ8615011";
			RefreshContainer();
			AssertNoMessageError(containerDO.NumberInfo, mandatoryMessageError);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsNVO = true;
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_AgentType = Constants.AgentType.Agent;
			container.JC_ContainerNum = ZString.Empty;

			RefreshContainer();
			AssertNoMessageError(containerDO.NumberInfo, mandatoryMessageError);

			container.JC_ContainerNum = "EREQ8615011";
			RefreshContainer();
			AssertNoMessageError(containerDO.NumberInfo, mandatoryMessageError);

			shippingLine.RSL_IsNVO = false;
			container.JC_ContainerNum = ZString.Empty;
			RefreshContainer();
			AssertHasMessageError(containerDO.NumberInfo, mandatoryMessageError);

			container.JC_ContainerNum = "EREQ8615011";
			RefreshContainer();
			AssertNoMessageError(containerDO.NumberInfo, mandatoryMessageError);
		}

		#endregion

		#region Shipments Validation

		public void TestShipmentsValidation()
		{
			var messageError = "S000001: ITN (Internal Transaction Number) is required for AES (Automated Export System) reporting for exports from the United States or its overseas territories. Enter the ITN in the Shipment > Entry Details field.";

			var singaporeCode = "SGSIN";
			var puertoRicoCode = "PRARR";

			var consol = CreateConsol();
			var shipment = consol.Shipments[0];
			shipment.JS_UniqueConsignRef = "S000001";
			consol.Transports[0].JW_RL_NKLoadPort = "USCHI";

			consol.Transports[0].JW_RL_NKDiscPort = singaporeCode;
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(
				"Message error should display when no ITN number for exports from US.",
				shippingInstruction.Shipments.First().ITNNumberInfo,
				messageError
			);

			consol.Transports[0].JW_RL_NKDiscPort = puertoRicoCode;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(
				"Message error should display when no ITN number for exports from US to US Overseas Territory.",
				shippingInstruction.Shipments.First().ITNNumberInfo,
				messageError
			);

			var itnNumber = shipment.CusEntryNumbers.AddNew();
			itnNumber.CE_EntryType = "ITN";
			itnNumber.CE_EntryNum = "C001";

			consol.Transports[0].JW_RL_NKDiscPort = singaporeCode;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(
				"Message error should not display when ITN number provided for exports from US.",
				shippingInstruction.Shipments.First().ITNNumberInfo,
				messageError
			);

			consol.Transports[0].JW_RL_NKDiscPort = puertoRicoCode;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertNoMessageError(
				"Message error should not display when ITN number provided for exports from US to US Overseas Territory.",
				shippingInstruction.Shipments.First().ITNNumberInfo,
				messageError
			);
		}

		public void TestShipmentsValidation_USOverseasTerritories()
		{
			var messageError = "ITN (Internal Transaction Number) is required for AES (Automated Export System) reporting for exports from the United States or its overseas territories. Enter the ITN in the Shipment > Entry Details field.";

			var usOverseasTerritories = new[] { "PR", "VI", "GU", "AS", "MP" };
			var singaporeCode = "SGSIN";
			var usCode = "USMIA";

			foreach (var countryCode in usOverseasTerritories)
			{
				var usOverseasTerritoryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)).RL_Code;
				var otherUSOverseasTerritory = usOverseasTerritories.First(territoryCode => territoryCode != countryCode);
				var otherUSOverseasTerritoryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherUSOverseasTerritory)).RL_Code;

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var consol = CreateConsol();
					var shipment = consol.Shipments[0];
					consol.Transports[0].JW_RL_NKLoadPort = usOverseasTerritoryCode;
					consol.Transports[0].JW_RL_NKDiscPort = singaporeCode;

					var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
					AssertHasMessageErrorContaining(
						"Message error should display when no ITN number for exports from US Overseas Territories.",
						shippingInstruction.Shipments.First().ITNNumberInfo,
						messageError
					);

					consol.Transports[0].JW_RL_NKDiscPort = otherUSOverseasTerritoryCode;
					shippingInstruction = new ShippingInstructionBuilder(consol).Build();
					AssertHasMessageErrorContaining(
						"Message error should display when no ITN number for exports from one US Overseas Territory to another.",
						shippingInstruction.Shipments.First().ITNNumberInfo,
						messageError
					);

					consol.Transports[0].JW_RL_NKDiscPort = usCode;
					shippingInstruction = new ShippingInstructionBuilder(consol).Build();
					AssertHasMessageErrorContaining(
						"Message error should display when no ITN number for export from US Overseas Territory to US.",
						shippingInstruction.Shipments.First().ITNNumberInfo,
						messageError
					);

					var itnNumber = shipment.CusEntryNumbers.AddNew();
					itnNumber.CE_EntryType = "ITN";
					itnNumber.CE_EntryNum = "C001";

					consol.Transports[0].JW_RL_NKDiscPort = singaporeCode;
					shippingInstruction = new ShippingInstructionBuilder(consol).Build();
					AssertNoMessageErrorContaining(
						"Message error should not display when ITN number provided for exports from US Overseas Territories.",
						shippingInstruction.Shipments.First().ITNNumberInfo,
						messageError
					);

					consol.Transports[0].JW_RL_NKDiscPort = otherUSOverseasTerritoryCode;
					shippingInstruction = new ShippingInstructionBuilder(consol).Build();
					AssertNoMessageErrorContaining(
						"Message error should not display when ITN number provided for exports from one US Overseas Territory to another.",
						shippingInstruction.Shipments.First().ITNNumberInfo,
						messageError
					);

					consol.Transports[0].JW_RL_NKDiscPort = usOverseasTerritoryCode;
					shippingInstruction = new ShippingInstructionBuilder(consol).Build();
					AssertNoMessageErrorContaining(
						"Message error should not display when ITN number provided for exports from US Overseas Territory to US.",
						shippingInstruction.Shipments.First().ITNNumberInfo,
						messageError
					);
				}
			}

			foreach (var countryCode in new[] { "BR", "DE" })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var consol = CreateConsol();
					var shipment = consol.Shipments[0];
					consol.Transports[0].JW_RL_NKLoadPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)).RL_Code;

					var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
					AssertNoMessageErrorContaining("No message error for non-US Overseas Territories", shippingInstruction.Shipments.First().ITNNumberInfo, messageError);
				}
			}
		}

		public void TestIsUSOrUSTerritoryExportPopulatesCorrectly()
		{
			var usCode1 = "USLAX";
			var usCode2 = "USMIA";

			var nonUSCode1 = "SGSIN";
			var nonUSCode2 = "AUSYD";

			var usOverseasTerritoryCode1a = "PRARR";
			var usOverseasTerritoryCode1b = "PRBAR";
			var usOverseasTerritoryCode2 = "VIAGL";

			var consol = CreateConsol();
			var firstTransport = (Freight.Business.Transport)consol.Transports.First();
			var lastTransport = (Freight.Business.Transport)consol.Transports.Last();

			var shippingInstructionBuilder = new ShippingInstructionBuilder(consol);

			firstTransport.JW_RL_NKLoadPort = usCode1;
			lastTransport.JW_RL_NKDiscPort = nonUSCode1;
			var carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be true for exports from US.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				true
			);

			lastTransport.JW_RL_NKDiscPort = usOverseasTerritoryCode1a;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be true for exports from US to US Overseas Territories.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				true
			);

			lastTransport.JW_RL_NKDiscPort = usCode2;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be false for domestic consols.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				false
			);

			firstTransport.JW_RL_NKLoadPort = usOverseasTerritoryCode1a;
			lastTransport.JW_RL_NKDiscPort = nonUSCode1;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be true for exports from US Overseas Territories.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				true
			);

			lastTransport.JW_RL_NKDiscPort = usCode1;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be true for exports from US Overseas Territories to US.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				true
			);

			lastTransport.JW_RL_NKDiscPort = usOverseasTerritoryCode2;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be true for exports from one US Overseas Territory to another.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				true
			);

			lastTransport.JW_RL_NKDiscPort = usOverseasTerritoryCode1b;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be false for domestic consols.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				false
			);

			firstTransport.JW_RL_NKLoadPort = nonUSCode1;
			lastTransport.JW_RL_NKDiscPort = nonUSCode2;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be false when load port is not US or US Overseas Territory.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				false
			);

			lastTransport.JW_RL_NKDiscPort = usCode1;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be false for US imports that don't come from US Overseas Territory.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				false
			);

			lastTransport.JW_RL_NKDiscPort = usOverseasTerritoryCode1a;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(
				"IsUSOrUSTerritoryExport should be false for US Overseas Territory imports that don't come from US.",
				carrierMessageData.IsUSOrUSTerritoryExport,
				false
			);
		}

		public void TestIsUSOrCAImportPopulatesCorrectly()
		{
			var consol = CreateConsol();
			var shippingInstructionBuilder = new ShippingInstructionBuilder(consol);

			consol.JK_RL_NKLoadPort = "BEANR";
			consol.JK_RL_NKDischargePort = "USLAX";
			var carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(true, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);

			consol.JK_RL_NKDischargePort = "PRARR";
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(true, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);

			consol.JK_RL_NKDischargePort = "GUGUM";
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(true, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);

			consol.JK_RL_NKDischargePort = "MPROP";
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(true, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);

			consol.JK_RL_NKDischargePort = "VIAGL";
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(true, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);

			consol.JK_RL_NKDischargePort = "ASOFU";
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(true, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);

			consol.JK_RL_NKDischargePort = "CAVAN";
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(true, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);

			consol.JK_RL_NKDischargePort = "NLRTM";
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals(false, carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported);
		}

		public void TestRequireOneShipmentValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(carrierMessageData.ErrorPlaceHolderInfo, "You need at least one shipment");

			consol.Shipments.AddNew();

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.ErrorPlaceHolderInfo, "You need at least one shipment");
		}

		#endregion

		#region PackingLines Validation

		public void TestPackingLinesImportHarmonizedCodeValidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "USLAX";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			AssertHasMessageError("Import Harmonized is required for US", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to US.");

			void RefreshImportHarmonizedCode()
			{
				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			}

			consol.JK_RL_NKDischargePort = "VNTTH";
			RefreshImportHarmonizedCode();

			AssertHasMessageError("Import Harmonized is required for VN", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to VN.");

			consol.JK_RL_NKDischargePort = "IDBAH";
			RefreshImportHarmonizedCode();

			AssertHasMessageError("Import Harmonized is required for ID", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to ID.");

			consol.JK_RL_NKDischargePort = "BRABR";
			RefreshImportHarmonizedCode();

			AssertHasMessageError("Import Harmonized is required for BR", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to BR.");

			consol.JK_RL_NKDischargePort = "BRABR";
			packline.JL_HarmonisedCode = "9009";
			RefreshImportHarmonizedCode();

			AssertNoMessageError("Harmonized code is not empty", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to BR.");

			packline.JL_HarmonisedCode = string.Empty;
			var harmonisedCodes = packline.HarmonisedCodes.AddNew();
			harmonisedCodes.JLH_RN_NKCountry = "BR";
			harmonisedCodes.JLH_Code = "1234";

			RefreshImportHarmonizedCode();

			AssertNoMessageError("BR harmonized code is entered", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to BR.");
		}

		public void TestPackingLinesExportHarmonizedCodeValidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;
			AssertHasMessageError("Export Harmonized is required for US", exportHarmonizedCode.CodeInfo, "Harmonized System Code is required for exports to US.");

			void RefreshExportHarmonizedCode()
			{
				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;
			}

			consol.JK_RL_NKLoadPort = "BRABR";
			RefreshExportHarmonizedCode();

			AssertHasMessageError("Export Harmonized is required for BR", exportHarmonizedCode.CodeInfo, "Harmonized System Code is required for exports to BR.");

			consol.JK_RL_NKLoadPort = "BRHAM";
			packline.JL_HarmonisedCode = "9009";
			RefreshExportHarmonizedCode();

			AssertNoMessageError("Harmonized code is not empty", exportHarmonizedCode.CodeInfo, "Harmonized System Code is required for exports to BR.");

			packline.JL_HarmonisedCode = string.Empty;
			var harmonisedCodes = packline.HarmonisedCodes.AddNew();
			harmonisedCodes.JLH_RN_NKCountry = "BR";
			harmonisedCodes.JLH_Code = "1234";

			RefreshExportHarmonizedCode();

			AssertNoMessageError("BR harmonized code is entered", exportHarmonizedCode.CodeInfo, "Harmonized System Code is required for exports to BR.");
		}

		[TestDate(2021, 10, 1)]
		public void TestPackingLinesHarmonizedCodeValidations_WhenConsolRoutingLegIsInIndia()
		{
			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "IN5PA";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 2;
				packline.JL_F3_NKPackType = "PLT";

				var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				var harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
				AssertHasMessageError("Harmonized Code is required for IN exports", harmonizedCode.CodeInfo, "A six digit HS Code is required to comply with SCMTR Manifest reporting for India.");

				void RefreshExportHarmonizedCode()
				{
					carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
					harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
				}

				consol.JK_RL_NKLoadPort = "BRABR";
				consol.JK_RL_NKDischargePort = "IN5PA";
				RefreshExportHarmonizedCode();

				AssertHasMessageError("Harmonized Code is required for IN imports", harmonizedCode.CodeInfo, "A six digit HS Code is required to comply with SCMTR Manifest reporting for India.");

				consol.JK_RL_NKLoadPort = "BRHAM";
				consol.JK_RL_NKDischargePort = "CNSHA";
				var mainTransport = consol.Transports.OfType<Freight.Business.Transport>().Single();
				mainTransport.JW_TransportMode = Constants.TransportModes.Sea;
				mainTransport.JW_RL_NKLoadPort = "BRABR";
				mainTransport.JW_RL_NKDiscPort = "IN5PA";

				var transitingIndiaTransport = consol.Transports.AddNew();
				transitingIndiaTransport.JW_TransportMode = Constants.TransportModes.Sea;
				transitingIndiaTransport.JW_RL_NKLoadPort = "IN5PA";
				transitingIndiaTransport.JW_RL_NKDiscPort = "CNSHA";
				transitingIndiaTransport.JW_LegOrder = 2;

				RefreshExportHarmonizedCode();

				AssertHasMessageError("Harmonized Code is required for IN transiting", harmonizedCode.CodeInfo, "A six digit HS Code is required to comply with SCMTR Manifest reporting for India.");

				harmonizedCode.Code = "12AA";
				AssertHasMessageError("Harmonized Code should be 6 digits", harmonizedCode.CodeInfo, "A six digit HS Code is required to comply with SCMTR Manifest reporting for India.");

				harmonizedCode.Code = "123456";
				AssertNoMessageError(harmonizedCode.CodeInfo, "A six digit HS Code is required to comply with SCMTR Manifest reporting for India.");
			}
		}

		public void TestPackingLinesImportHarmonizedCodeValidations_WhenConsolRoutingLegIsInEurope()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "GBDVR";
			consol.JK_RL_NKDischargePort = "KRPUS";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "ITMIL";
			shipment.JS_RL_NKDestination = "KRKUM";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";

			var preTransport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			preTransport.JW_TransportMode = Constants.TransportModes.Sea;
			preTransport.JW_RL_NKLoadPort = "ITAOI";
			preTransport.JW_RL_NKDiscPort = "GRPIR";

			var mainTransport = consol.Transports.AddNew();
			mainTransport.JW_TransportMode = Constants.TransportModes.Sea;
			mainTransport.JW_RL_NKLoadPort = "GRPIR";
			mainTransport.JW_RL_NKDiscPort = "KRPUS";
			mainTransport.JW_LegOrder = 2;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			var importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			AssertHasMessageError("Import Harmonized Code is required for europe imports", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to the EU for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");

			void RefreshImportHarmonizedCode()
			{
				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			}

			consol.JK_RL_NKLoadPort = "ITAOI";
			RefreshImportHarmonizedCode();

			AssertNoMessageError("Import Harmonized Code is not required for europe exports", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to the EU for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");

			consol.JK_RL_NKLoadPort = "GBDVR";
			consol.JK_RL_NKDischargePort = "DEHAM";
			shipment.JS_RL_NKDestination = "DEHAM";
			preTransport.JW_RL_NKDiscPort = "MYPKG";
			mainTransport.JW_RL_NKLoadPort = "MYPKG";

			RefreshImportHarmonizedCode();

			AssertHasMessageError("Import Harmonized Code is required", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to the EU for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");

			consol.JK_RL_NKLoadPort = "ITAOI";
			RefreshImportHarmonizedCode();

			AssertNoMessageError("Import Harmonized Code is not required for europe exports", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to the EU for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");
		}

		public void TestPackingLineValidation_HSCodesMustBe6DigitForICS2()
		{
			var warningMessage = "HS code(s) reported for ICS2 must be 6-digit only.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "GBDVR";
			consol.JK_RL_NKDischargePort = "KRPUS";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "ITMIL";
			shipment.JS_RL_NKDestination = "KRKUM";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_HarmonisedCode = "12345";

			var hc1 = packline.HarmonisedCodes.AddNew();
			hc1.JLH_RN_NKCountry = "GB";
			hc1.JLH_Code = "1234";

			var hc2 = packline.HarmonisedCodes.AddNew();
			hc2.JLH_RN_NKCountry = "AU";
			hc2.JLH_Code = "123433";

			var transport1 = consol.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_LegOrder = 1;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			var harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
			var importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			var exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
			AssertNoWarning(importHarmonizedCode.CodeInfo, warningMessage);
			AssertNoWarning(exportHarmonizedCode.CodeInfo, warningMessage);

			packline.JL_HarmonisedCode = "1234567890";
			hc1.JLH_Code = "1234567890";
			hc2.JLH_Code = "1234567890";

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
			importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "BEANR";
			transport2.JW_LegOrder = 2;

			packline.JL_HarmonisedCode = "12345";
			hc1.JLH_Code = "12345";

			var hc3 = packline.HarmonisedCodes.AddNew();
			hc3.JLH_RN_NKCountry = "BE";
			hc3.JLH_Code = "12345";

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
			importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

			AssertHasWarning(harmonizedCode.CodeInfo, warningMessage);
			AssertHasWarning(importHarmonizedCode.CodeInfo, warningMessage);
			AssertHasWarning(exportHarmonizedCode.CodeInfo, warningMessage);

			packline.JL_HarmonisedCode = "1234567890";
			hc1.JLH_Code = "1234567890";
			hc3.JLH_Code = "1234567890";

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
			importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

			AssertHasWarning(harmonizedCode.CodeInfo, warningMessage);
			AssertHasWarning(importHarmonizedCode.CodeInfo, warningMessage);
			AssertHasWarning(exportHarmonizedCode.CodeInfo, warningMessage);

			packline.JL_HarmonisedCode = "123456";
			hc1.JLH_Code = "123456";
			hc2.JLH_Code = "123456";

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
			importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
		}

		public void TestPackingLineValidation_HSCodesMustBe6DigitForICS2_PackageGrouping()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var warningMessage = "HS code(s) reported for ICS2 must be 6-digit only.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "GBDVR";
				consol.JK_RL_NKDischargePort = "KRPUS";
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				consol.Containers.AddNew();

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
				shipment.JS_RL_NKOrigin = "ITMIL";
				shipment.JS_RL_NKDestination = "KRKUM";

				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 2;
				packline.JL_F3_NKPackType = "PLT";
				packline.JL_HarmonisedCode = "12345";

				var hc1 = packline.HarmonisedCodes.AddNew();
				hc1.JLH_RN_NKCountry = "GB";
				hc1.JLH_Code = "1234";

				var hc2 = packline.HarmonisedCodes.AddNew();
				hc2.JLH_RN_NKCountry = "AU";
				hc2.JLH_Code = "123433";

				var packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_PackageCount = 2;
				packline2.JL_F3_NKPackType = "PLT";
				packline2.JL_HarmonisedCode = "22345";

				var hc21 = packline.HarmonisedCodes.AddNew();
				hc21.JLH_RN_NKCountry = "GB";
				hc21.JLH_Code = "2234";

				var hc22 = packline.HarmonisedCodes.AddNew();
				hc22.JLH_RN_NKCountry = "AU";
				hc22.JLH_Code = "223433";

				var transport1 = consol.Transports.AddNew();
				transport1.JW_TransportMode = Constants.TransportModes.Sea;
				transport1.JW_RL_NKLoadPort = "USLAX";
				transport1.JW_RL_NKDiscPort = "AUSYD";
				transport1.JW_LegOrder = 1;

				var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				var harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
				var importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
				var exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

				AssertEquals(2, harmonizedCode.Code.Split(", ").Length);
				AssertEquals(2, importHarmonizedCode.Code.Split(", ").Length);
				AssertEquals(2, exportHarmonizedCode.Code.Split(", ").Length);

				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
				AssertNoWarning(importHarmonizedCode.CodeInfo, warningMessage);
				AssertNoWarning(exportHarmonizedCode.CodeInfo, warningMessage);

				packline.JL_HarmonisedCode = "1234567890";
				hc1.JLH_Code = "1234567890";
				hc2.JLH_Code = "1234567890";

				packline2.JL_HarmonisedCode = "2234567890";
				hc21.JLH_Code = "2234567890";
				hc22.JLH_Code = "2234567890";

				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
				importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
				exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = Constants.TransportModes.Sea;
				transport2.JW_RL_NKLoadPort = "AUSYD";
				transport2.JW_RL_NKDiscPort = "BEANR";
				transport2.JW_LegOrder = 2;

				packline.JL_HarmonisedCode = "12345";
				hc1.JLH_Code = "12345";

				packline2.JL_HarmonisedCode = "22345";
				hc21.JLH_Code = "22345";

				var hc3 = packline.HarmonisedCodes.AddNew();
				hc3.JLH_RN_NKCountry = "BE";
				hc3.JLH_Code = "12345";

				var hc23 = packline.HarmonisedCodes.AddNew();
				hc23.JLH_RN_NKCountry = "BE";
				hc23.JLH_Code = "22345";

				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
				importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
				exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

				AssertHasWarning(harmonizedCode.CodeInfo, warningMessage);
				AssertHasWarning(importHarmonizedCode.CodeInfo, warningMessage);
				AssertHasWarning(exportHarmonizedCode.CodeInfo, warningMessage);

				packline.JL_HarmonisedCode = "1234567890";
				hc1.JLH_Code = "1234567890";
				hc3.JLH_Code = "1234567890";

				packline2.JL_HarmonisedCode = "223456";
				hc21.JLH_Code = "223456";
				hc23.JLH_Code = "223456";

				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
				importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
				exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

				AssertHasWarning(harmonizedCode.CodeInfo, warningMessage);
				AssertHasWarning(importHarmonizedCode.CodeInfo, warningMessage);
				AssertHasWarning(exportHarmonizedCode.CodeInfo, warningMessage);

				packline.JL_HarmonisedCode = "123456";
				hc1.JLH_Code = "123456";
				hc3.JLH_Code = "123456";

				carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
				harmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().HarmonizedCode;
				importHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ImportHarmonizedCode;
				exportHarmonizedCode = (HarmonizedCode)carrierMessageData.Shipments.First().PackingLines.First().ExportHarmonizedCode;

				AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);
				AssertNoWarning(importHarmonizedCode.CodeInfo, warningMessage);
				AssertNoWarning(exportHarmonizedCode.CodeInfo, warningMessage);
			}
		}

		public void TestPackingLineValidation_MandatoryMarksAndNumbers()
		{
			var warningMessage = "Marks & Numbers are mandatory when carrier is filing the House Bill. Please enter the ICS2 Declarant EORI field if Carrier Filing is not intended.";

			var consol = CreateConsol("AUSYD", "USLAX");
			var shipment = consol.Shipments[0];
			shipment.JS_MarksAndNumbers = ZString.Empty;
			if (shipment.CoLoadMasterShipment != null)
			{
				shipment.CoLoadMasterShipment.JS_MarksAndNumbers = ZString.Empty;
			}
			var packLine = shipment.OuterPackLines[0];
			packLine.JL_MarksAndNumbers = ZString.Empty;
			var wrapper = new ShippingInstructionBuilder(consol).Build();

			Assert(!wrapper.IsShowICS2);
			AssertEquals(string.Empty, wrapper.ICS2DeclarantEORINumber);
			AssertNoWarning(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().MarksAndNumbersInfo, warningMessage);

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nOrThErN IRelAnd";
			}

			consol = CreateConsol("AUSYD", "GBBEL");
			shipment = consol.Shipments[0];
			shipment.JS_MarksAndNumbers = ZString.Empty;
			if (shipment.CoLoadMasterShipment != null)
			{
				shipment.CoLoadMasterShipment.JS_MarksAndNumbers = ZString.Empty;
			}
			packLine = shipment.OuterPackLines[0];
			packLine.JL_MarksAndNumbers = ZString.Empty;
			wrapper = new ShippingInstructionBuilder(consol).Build();

			Assert(wrapper.IsShowICS2);
			AssertEquals(string.Empty, wrapper.ICS2DeclarantEORINumber);
			AssertHasMessageError(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().MarksAndNumbersInfo, warningMessage);

			wrapper.ICS2DeclarantEORINumber = "111111";
			AssertNoMessageError(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().MarksAndNumbersInfo, warningMessage);

			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new ShippingInstructionBuilder(consol).Build();

			Assert(wrapper.IsShowICS2);
			AssertEquals(string.Empty, wrapper.ICS2DeclarantEORINumber);
			AssertNoMessageError(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().MarksAndNumbersInfo, warningMessage);
		}

		public void TestPackingLineValidation_MarksAndNumbersExceedingCharacterLimit()
		{
			var warningMessage = "Marks & Numbers over 512 characters will be truncated during transmission to ICS2 system.";

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nOrThErN IRelAnd";
			}
			var consol = CreateConsol("AUSYD", "GBBEL");
			var packLine = consol.Shipments[0].OuterPackLines[0];
			packLine.JL_MarksAndNumbers = new string('1', 513);

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().MarksAndNumbersInfo, warningMessage);

			wrapper.ICS2DeclarantEORINumber = "111111";
			AssertNoWarning(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().MarksAndNumbersInfo, warningMessage);

			packLine.JL_MarksAndNumbers = new string('1', 512);
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().MarksAndNumbersInfo, warningMessage);
		}

		public void TestPackingLineValidation_GoodsDescriptionExceedingCharacterLimit()
		{
			var warningMessage = "Goods Description over 512 characters will be truncated during transmission to ICS2 system.";

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nOrThErN IRelAnd";
			}
			var consol = CreateConsol("AUSYD", "GBBEL");
			var packLine = consol.Shipments[0].OuterPackLines[0];
			packLine.JL_DetailedDescription = new string('1', 513);

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertHasWarning(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().GoodsDescriptionInfo, warningMessage);

			wrapper.ICS2DeclarantEORINumber = "111111";
			AssertNoWarning(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().GoodsDescriptionInfo, warningMessage);

			packLine.JL_DetailedDescription = new string('1', 512);
			wrapper = new ShippingInstructionBuilder(consol).Build();
			AssertNoWarning(wrapper.Shipments.FirstOrDefault().PackingLines.FirstOrDefault().GoodsDescriptionInfo, warningMessage);
		}

		#endregion

		#region VesselNameValidation

		public void TestVesselNameValidation()
		{
			var consol = CreateConsol();
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.Transports.Main.Vessel.Name, shippingInstruction.Transports.Main.Vessel.NameInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(shippingInstruction.Transports.Main.Vessel.NameInfo, "Vessel name is required.");
		}

		#endregion

		#region VoyageFlightNumberValidation

		public void TestVoyageFlightNumberValidation()
		{
			var consol = CreateConsol();
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertionHelper.AssertAsciiCharactersValidation(shippingInstruction.Transports.Main.VoyageFlightNumber, shippingInstruction.Transports.Main.VoyageFlightNumberInfo);
			AssertionHelper.AssertMessageErrorIfEmpty(shippingInstruction.Transports.Main.VoyageFlightNumberInfo, "Voyage is required.");
		}

		#endregion

		#region Main Transport Validation

		public void TestMainTransportValidation()
		{
			var consol = CreateConsol();
			consol.Transports[0].JW_TransportType = Constants.TransportPlanningType.Other;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertHasMessageError(shippingInstruction.ErrorPlaceHolderInfo, "Main Sea leg is required.");
		}

		#endregion

		#region Containers

		public void TestContainersValidation()
		{
			var requireContainerAndPackingLinesMessageError = "Container details are required for Shipping Instruction and Amendment messages.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;
			var data = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(data.ErrorPlaceHolderInfo, requireContainerAndPackingLinesMessageError);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.ErrorPlaceHolderInfo, requireContainerAndPackingLinesMessageError);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.Containers.AddNew();
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.ErrorPlaceHolderInfo, requireContainerAndPackingLinesMessageError);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			data = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(data.ErrorPlaceHolderInfo, requireContainerAndPackingLinesMessageError);
		}

		public void TestContainerSealValidation()
		{
			var errorMessage = "Sealed By is mandatory when Seal Number exists.";

			var consol = Factory.New<ForwardingConsol>();
			var containerSrc = consol.Containers.AddNew();

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.Cast<Container>().First();

			void RefreshExportCode()
			{
				data = CreateDocDataObjectBuilder(consol).Build();
				container = data.Containers.Cast<Container>().First();
			}

			AssertNoMessageError(((CodeDescription)container.SealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_SealNum = "100";
			RefreshExportCode();

			AssertHasMessageError(((CodeDescription)container.SealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_SealNum = "100";
			containerSrc.JC_SealParty = Constants.ContainerSealParties.Codes.CarrierShippingLine;
			RefreshExportCode();

			AssertNoMessageError(((CodeDescription)container.SealPartyType).CodeInfo, errorMessage);
		}

		public void TestContainer2ndSealValidation()
		{
			var errorMessage = "Sealed By is mandatory when Seal Number exists.";

			var consol = Factory.New<ForwardingConsol>();
			var containerSrc = consol.Containers.AddNew();

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.Cast<Container>().First();

			void RefreshExportCode()
			{
				data = CreateDocDataObjectBuilder(consol).Build();
				container = data.Containers.Cast<Container>().First();
			}

			AssertNoMessageError(((CodeDescription)container.SecondSealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_AdditionalSealNum = "100";
			RefreshExportCode();

			AssertHasMessageError(((CodeDescription)container.SecondSealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_AdditionalSealNum = "100";
			containerSrc.JC_AdditionalSealParty = Constants.ContainerSealParties.Codes.CarrierShippingLine;
			RefreshExportCode();

			AssertNoMessageError(((CodeDescription)container.SecondSealPartyType).CodeInfo, errorMessage);
		}

		public void TestContainer3rdSealValidation()
		{
			var errorMessage = "Sealed By is mandatory when Seal Number exists.";

			var consol = Factory.New<ForwardingConsol>();
			var containerSrc = consol.Containers.AddNew();

			var data = CreateDocDataObjectBuilder(consol).Build();
			var container = data.Containers.Cast<Container>().First();

			void RefreshExportCode()
			{
				data = CreateDocDataObjectBuilder(consol).Build();
				container = data.Containers.Cast<Container>().First();
			}

			AssertNoMessageError(((CodeDescription)container.ThirdSealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_Additional2SealNum = "100";
			RefreshExportCode();

			AssertHasMessageError(((CodeDescription)container.ThirdSealPartyType).CodeInfo, errorMessage);

			containerSrc.JC_Additional2SealNum = "100";
			containerSrc.JC_Additional2SealParty = Constants.ContainerSealParties.Codes.CarrierShippingLine;
			RefreshExportCode();

			AssertNoMessageError(((CodeDescription)container.ThirdSealPartyType).CodeInfo, errorMessage);
		}

		#endregion

		#region ChargesValidation

		public void TestIsFreightPrepaidAndIsFreightCollectAndIsFreightAsAgreedValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.IsFreightPrepaid = false;
			carrierMessageData.IsFreightCollect = false;
			carrierMessageData.IsFreightAsAgreed = false;

			var mustChoseOneMessageError = "A payment type must be selected for 'Freight Charges'.";

			AssertHasMessageError(carrierMessageData.IsFreightPrepaidInfo, mustChoseOneMessageError);
			AssertHasMessageError(carrierMessageData.IsFreightCollectInfo, mustChoseOneMessageError);
			AssertHasMessageError(carrierMessageData.IsFreightAsAgreedInfo, mustChoseOneMessageError);

			carrierMessageData.IsFreightPrepaid = true;
			AssertEquals(false, carrierMessageData.IsFreightCollect);
			AssertEquals(false, carrierMessageData.IsFreightAsAgreed);
			AssertNoMessageError(carrierMessageData.IsFreightPrepaidInfo, mustChoseOneMessageError);
			AssertNoMessageError(carrierMessageData.IsFreightCollectInfo, mustChoseOneMessageError);
			AssertNoMessageError(carrierMessageData.IsFreightAsAgreedInfo, mustChoseOneMessageError);

			carrierMessageData.IsFreightCollect = true;
			AssertEquals(false, carrierMessageData.IsFreightPrepaid);
			AssertEquals(false, carrierMessageData.IsFreightAsAgreed);
			AssertNoMessageError(carrierMessageData.IsFreightPrepaidInfo, mustChoseOneMessageError);
			AssertNoMessageError(carrierMessageData.IsFreightCollectInfo, mustChoseOneMessageError);
			AssertNoMessageError(carrierMessageData.IsFreightAsAgreedInfo, mustChoseOneMessageError);

			carrierMessageData.IsFreightAsAgreed = true;
			AssertEquals(true, carrierMessageData.IsFreightCollect);
			AssertEquals(false, carrierMessageData.IsFreightPrepaid);
			AssertNoMessageError(carrierMessageData.IsFreightPrepaidInfo, mustChoseOneMessageError);
			AssertNoMessageError(carrierMessageData.IsFreightCollectInfo, mustChoseOneMessageError);
			AssertNoMessageError(carrierMessageData.IsFreightAsAgreedInfo, mustChoseOneMessageError);
		}

		#endregion

		#region NumberOfOriginals And ReleaseType Validation

		public void TestNumberOfOriginalsAndReleaseTypeValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReleaseType = "BRR";
			consol.JK_NoOriginalBills = 0;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(carrierMessageData.NumberOfOriginalsInfo, "This Release Type requires at least one Original Bill");

			consol.JK_NoOriginalBills = 1;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.NumberOfOriginalsInfo, "This Release Type requires at least one Original Bill");

			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.NonNegotiable;
			consol.JK_NoOriginalBills = 0;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.NumberOfOriginalsInfo, "This Release Type requires at least one Original Bill");
		}

		#endregion

		#region ReleaseType changes when Consignee CompanyName changes

		public void TestReleaseTypeChangesWhenConsigneeCompanyNameChanges()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReleaseType = "NON";
			consol.JK_NoCopyBills = 3;
			consol.JK_NoOriginalBills = 2;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals(ShippingInstructionReleaseTypes.Codes.SeaWaybill, carrierMessageData.ReleaseType.Code);
			AssertEquals(carrierMessageData.NumberOfCopies, 3);
			AssertEquals(carrierMessageData.NumberOfOriginals, 3);

			carrierMessageData.Consignee.CompanyName = "TO ORDER";
			AssertEquals(ShippingInstructionReleaseTypes.Codes.BOLOriginal, carrierMessageData.ReleaseType.Code);
			AssertEquals(carrierMessageData.NumberOfCopies, 3);
			AssertEquals(carrierMessageData.NumberOfOriginals, 2);
		}

		#endregion

		#region BillOfLadingNumber

		public void TestPopulateBillOfLadingNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_MasterBillNum = "MBL00023";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "B00588";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(string.Empty, carrierMessageData.BillOfLadingNumber);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("B00588", carrierMessageData.BillOfLadingNumber);

			consol.Shipments.AddNew();
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(string.Empty, carrierMessageData.BillOfLadingNumber);
		}

		#endregion

		#region CountrySpecificFields

		public void TestPopulateCountrySpecificFields()
		{
			var sendingAgent = CreateOrgHeader("SOSA", "SO SendingAgent");
			sendingAgent.CustomsCodes.AddNew("CCC", "1234", "US");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNCAN";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("1234", carrierMessageData.USCanadaManifestSelfFilerID);
			AssertEquals(false, carrierMessageData.IsTransitThroughBrazil);
			AssertEquals("Not Applicable", carrierMessageData.BRWoodenPackageProcessType.Code);
			AssertAsciiCharactersValidation("USCanadaManifestSelfFilerID", carrierMessageData.USCanadaManifestSelfFilerIDInfo);

			consol.JK_RL_NKLoadPort = "BRABC";
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals(true, carrierMessageData.IsTransitThroughBrazil);
			AssertEquals(string.Empty, carrierMessageData.USCanadaManifestSelfFilerID);
		}

		public void TestUSCanadaManifestSelfFilerIDDefault_CA() => TestUSCanadaManifestSelfFilerIDDefault_Helper("CA2KS", "CA");
		public void TestUSCanadaManifestSelfFilerIDDefault_PR() => TestUSCanadaManifestSelfFilerIDDefault_Helper("PRADJ", "PR");
		public void TestUSCanadaManifestSelfFilerIDDefault_GU() => TestUSCanadaManifestSelfFilerIDDefault_Helper("GUAGA", "GU");
		public void TestUSCanadaManifestSelfFilerIDDefault_MP() => TestUSCanadaManifestSelfFilerIDDefault_Helper("MPROP", "MP");
		public void TestUSCanadaManifestSelfFilerIDDefault_VI() => TestUSCanadaManifestSelfFilerIDDefault_Helper("VIAGL", "VI");
		public void TestUSCanadaManifestSelfFilerIDDefault_AS() => TestUSCanadaManifestSelfFilerIDDefault_Helper("ASAPI", "AS");

		public void TestUSCanadaManifestSelfFilerIDDefault_US_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("USLAX");
		public void TestUSCanadaManifestSelfFilerIDDefault_CA_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("CA2KS");
		public void TestUSCanadaManifestSelfFilerIDDefault_PR_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("PRADJ");
		public void TestUSCanadaManifestSelfFilerIDDefault_GU_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("GUAGA");
		public void TestUSCanadaManifestSelfFilerIDDefault_MP_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("MPROP");
		public void TestUSCanadaManifestSelfFilerIDDefault_VI_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("VIAGL");
		public void TestUSCanadaManifestSelfFilerIDDefault_AS_WithFallback() => TestUSCanadaManifestSelfFilerIDDefault_Helper("ASAPI");

		public void TestUSCanadaManifestSelfFilerIDDefault_Helper(string dischargePort, string dischargeCountry = "")
		{
			var sendingAgent = CreateOrgHeader("SOSA", "SO SendingAgent");
			sendingAgent.CustomsCodes.AddNew("CCC", "1234", "US");
			if (!string.IsNullOrEmpty(dischargeCountry))
			{
				sendingAgent.CustomsCodes.AddNew("CCC", "5555", dischargeCountry);
			}

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNCAN";
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			if (!string.IsNullOrEmpty(dischargeCountry))
			{
				AssertEquals("5555", shippingInstruction.USCanadaManifestSelfFilerID);
			}
			else
			{
				AssertEquals("1234", shippingInstruction.USCanadaManifestSelfFilerID);
			}
		}

		public void TestBRWoodenPackageProcessType_Validation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNCAN";
			consol.JK_RL_NKDischargePort = "USLAX";

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("Not Applicable", carrierMessageData.BRWoodenPackageProcessType.Code);
			AssertAsciiCharactersValidation("USCanadaManifestSelfFilerID", carrierMessageData.USCanadaManifestSelfFilerIDInfo);

			carrierMessageData.BRWoodenPackageProcessType.Code = string.Empty;
			AssertHasMessageError(carrierMessageData.BRWoodenPackageProcessType.CodeInfo, "Wooden Package is required.");

			carrierMessageData.BRWoodenPackageProcessType.Code = "Processed";
			AssertNoMessageError(carrierMessageData.BRWoodenPackageProcessType.CodeInfo, "Wooden Package is required.");
		}

		OrgHeader CreateOrgHeader(ZString orgCode, ZString orgName)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = orgName;
			orgHeader.OH_RL_NKClosestPort = "DKAAL";
			orgHeader.MainAddress.Address1 = "Unit 13";
			orgHeader.MainAddress.Address2 = "4 Lost Lane";
			orgHeader.MainAddress.City = "Aalborg";
			orgHeader.MainAddress.Postcode = "2000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "DK";

			return orgHeader;
		}

		#endregion

		#region Goods Value

		public void TestPopulateGoodsValue()
		{
			var consol = CreateConsol();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			var consolCost1 = CreateConsolCost(consol.PK, chargeCode.PK, Constants.CurrencyCodes.UnitedStates);
			consolCost1[JobConsolCostSchema.E6_OH_Creditor] = consol.Creditor.PK;

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsValue = 40m;
			shipment1.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.UnitedStates;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_GoodsValue = 60m;
			shipment2.JS_RX_NKGoodsValueCurr = ZString.Empty;

			var shipment3_WrongCurrency = consol.Shipments.AddNew();
			shipment3_WrongCurrency.JS_GoodsValue = 10.40m;
			shipment3_WrongCurrency.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.Canada;

			Factory.Save();

			var data = CreateDocDataObjectBuilder(consol).Build();

			CombineAssertions(() =>
			{
				AssertEquals("BOL Currency is USD", Constants.CurrencyCodes.UnitedStates, data.GoodsValue.Currency.Code);
				AssertEquals("Goods Value: should only include currency that matches BOL Currency or is blank ", 100m, data.GoodsValue.Amount);
			});
		}

		public void TestPopulateGoodsValueCurrency()
		{
			var consol = CreateConsol();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			var consolCost1 = CreateConsolCost(consol.PK, chargeCode.PK, Constants.CurrencyCodes.UnitedStates);
			consolCost1[JobConsolCostSchema.E6_OH_Creditor] = consol.Creditor.PK;

			var consolCost2 = CreateConsolCost(consol.PK, chargeCode.PK, Constants.CurrencyCodes.UnitedStates);
			consolCost2[JobConsolCostSchema.E6_OH_Creditor] = consol.Creditor.PK;

			var consolCost3 = CreateConsolCost(consol.PK, chargeCode.PK, Constants.CurrencyCodes.UnitedStates);
			consolCost3[JobConsolCostSchema.E6_OH_Creditor] = consol.ShippingLine.PK;

			var consolCost4 = CreateConsolCost(consol.PK, chargeCode.PK, Constants.CurrencyCodes.China);
			consolCost4[JobConsolCostSchema.E6_OH_Creditor] = consol.ShippingLine.PK;

			var shipment1 = consol.Shipments[0];
			shipment1.JS_GoodsValue = 15.2m;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_GoodsValue = 16.6m;

			Factory.Save();

			var data = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals("Match from creditor and all FRT charges have the same currency", Constants.CurrencyCodes.UnitedStates, data.GoodsValue.Currency.Code);

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			data = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(
				"Fallback as company's currency when match from ShippingLine but all FRT charges have NOT the same currency",
				Constants.CurrencyCodes.Australia,
				data.GoodsValue.Currency.Code
			);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			data = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(
				"Fallback as company's currency when Creditor and ShippingLine are empty",
				Constants.CurrencyCodes.Australia,
				data.GoodsValue.Currency.Code
			);
		}

		[TestDate(2021, 10, 1)]
		public void TestGoodsValueValidation()
		{
			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = CreateConsol();
				consol.Transports[1].JW_RL_NKDiscPort = "IN5PA";
				consol.Transports[2].JW_RL_NKLoadPort = "IN5PA";
				consol.Transports[2].JW_RL_NKDiscPort = "CNSHA";

				var data = CreateDocDataObjectBuilder(consol).Build();
				data.GoodsValue.Currency.Code = string.Empty;

				AssertEquals(0m, data.GoodsValue.Amount);
				AssertNoMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertNoMessageErrors(((CodeDescription)data.GoodsValue.Currency).CodeInfo);

				data.GoodsValue.Amount = 19.3m;

				AssertNoMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertHasMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");

				data.GoodsValue.Currency.Code = "CNY";
				AssertNoMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");
			}

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = CreateConsol("AUSYD", "IN5PA");

				var data = CreateDocDataObjectBuilder(consol).Build();
				data.GoodsValue.Currency.Code = string.Empty;

				AssertEquals(0m, data.GoodsValue.Amount);
				AssertHasMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertNoMessageErrors(((CodeDescription)data.GoodsValue.Currency).CodeInfo);

				data.GoodsValue.Amount = 19.3m;

				AssertNoMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertHasMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");

				data.GoodsValue.Currency.Code = "CNY";
				AssertNoMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");
			}

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = CreateConsol("IN5PA", "AUSYD");

				var data = CreateDocDataObjectBuilder(consol).Build();
				data.GoodsValue.Currency.Code = string.Empty;

				AssertEquals(0m, data.GoodsValue.Amount);
				AssertNoMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertNoMessageErrors(((CodeDescription)data.GoodsValue.Currency).CodeInfo);

				data.GoodsValue.Amount = 19.3m;

				AssertNoMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertHasMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");

				data.GoodsValue.Currency.Code = "CNY";
				AssertNoMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");
			}

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			{
				var consol = CreateConsol();
				consol.Transports[2].JW_RL_NKLoadPort = "IN5PA";
				consol.Transports[2].JW_RL_NKDiscPort = "CNSHA";

				var data = CreateDocDataObjectBuilder(consol).Build();
				data.GoodsValue.Currency.Code = string.Empty;

				AssertEquals(0m, data.GoodsValue.Amount);
				AssertNoMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertNoMessageErrors(((CodeDescription)data.GoodsValue.Currency).CodeInfo);

				data.GoodsValue.Amount = 19.3m;

				AssertNoMessageError(data.GoodsValue.AmountInfo, "Goods Value is required to comply with SCMTR Manifest reporting for India.");
				AssertHasMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");

				data.GoodsValue.Currency.Code = "CNY";
				AssertNoMessageError(((CodeDescription)data.GoodsValue.Currency).CodeInfo, "BOL Currency is required to comply with SCMTR Manifest reporting for India.");
			}
		}

		[TestDate(2021, 10, 1)]
		public void TestSuppressGoodsValue_WhenHasDischargePortInIndia()
		{
			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			using (FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = CreateConsol("AUSYD", "IN5PA");

				var data = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals(false, data.SuppressGoodsValue);
			}

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			using (FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol();
				consol.Transports[1].JW_RL_NKDiscPort = "IN5PA";

				var data = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals(false, data.SuppressGoodsValue);
			}
		}

		[TestDate(2021, 10, 1)]
		public void TestSuppressGoodsValue_WhenhasNoDischargePortInIndia()
		{
			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			using (FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = CreateConsol("IN5PA", "AUSYD");

				var data = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals(!FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.Value, data.SuppressGoodsValue);
			}

			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			using (FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol();
				consol.Transports[1].JW_RL_NKLoadPort = "IN5PA";

				var data = CreateDocDataObjectBuilder(consol).Build();

				AssertEquals(!FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.Value, data.SuppressGoodsValue);
			}
		}

		[TestDate(2021, 10, 1)]
		public void TestSuppressGoodsValue_ShouldBeTrue_WhenConsolHasNotTransportLegInIndiaAndEnableGoodsValueRegistryIsFalse()
		{
			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			using (FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = CreateConsol();
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUSYD";

				consol.Transports[0].JW_RL_NKLoadPort = "CNSHA";
				consol.Transports[0].JW_RL_NKDiscPort = "SGSIN";

				consol.Transports[1].JW_RL_NKLoadPort = "SGSIN";
				consol.Transports[1].JW_RL_NKDiscPort = "NZAKL";

				consol.Transports[2].JW_RL_NKLoadPort = "NZAKL";
				consol.Transports[2].JW_RL_NKDiscPort = "AUSYD";

				var data = CreateDocDataObjectBuilder(consol).Build();
				Assert(data.SuppressGoodsValue);
			}
		}

		[TestDate(2021, 10, 1)]
		public void TestSuppressGoodsValue_ShouldBeFalse_WhenConsolHasNotTransportLegInIndiaAndEnableGoodsValueRegistryIsTrue()
		{
			using (FreightDataRegistry.Instance.SCMTREnableDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2021, 10, 1)))
			using (FreightDataRegistry.Instance.EnableGoodsValueForShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = CreateConsol();
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUSYD";

				consol.Transports[0].JW_RL_NKLoadPort = "CNSHA";
				consol.Transports[0].JW_RL_NKDiscPort = "SGSIN";

				consol.Transports[1].JW_RL_NKLoadPort = "SGSIN";
				consol.Transports[1].JW_RL_NKDiscPort = "NZAKL";

				consol.Transports[2].JW_RL_NKLoadPort = "NZAKL";
				consol.Transports[2].JW_RL_NKDiscPort = "AUSYD";

				var data = CreateDocDataObjectBuilder(consol).Build();
				Assert(!data.SuppressGoodsValue);
			}
		}

		BusinessObject CreateConsolCost(ZGuid parentPK, ZGuid chargeCodePK, ZString currencyCode)
		{
			var result = (BusinessObject)Factory.New<IJobConsolCost>();
			result[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCodePK;
			result[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			result[JobConsolCostSchema.E6_RX_NKCurrency] = currencyCode;

			result.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				result[JobConsolCostSchema.E6_ParentID] = parentPK;
				result[JobConsolCostSchema.E6_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			}
			finally
			{
				result.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			return result;
		}

		#endregion

		#region Send Contact/Company Id in SI Bill Clauses

		public void TestSendContactCompanyIdInSIBillClauses()
		{
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.Australia, OrgCusCode.ChinaCodeTypes.USC, Constants.TaxRelatedDocumentType.ShippingInstruction);
			TaxNumberTestHelper.AddNewRefDocOrgCusCode(Constants.CountryCodes.China, Constants.CountryCodes.China, OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Constants.TaxRelatedDocumentType.ShippingInstruction);

			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shipment = consol.Shipments[0];

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "AUSYD";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";
			consignee.MainAddress.OA_Phone = "+65 2300 89822";
			consignee.MainAddress.OA_Email = "contact@consignee.com";
			var consigneeContact = consignee.Contacts.AddNew();
			consigneeContact.OC_ContactName = "PETER WILLIAMS";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.ContactPK = consigneeContact.PK;
			var consigneeTaxCode = consignee.CustomsCodes.AddNew();
			consigneeTaxCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			consigneeTaxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			consigneeTaxCode.OK_CustomsRegNo = "923456789012345670";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "CNSHA";
			consignor.OH_RL_NKClosestPort = "CNSHA";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";
			consignor.MainAddress.OA_Fax = "+61 2 8300 8900";
			consignor.MainAddress.OA_Phone = "+61 2 8300 8900";
			consignor.MainAddress.OA_Email = "contact@shipper.com";
			var consignorContact = consignor.Contacts.AddNew();
			consignorContact.OC_ContactName = "ALAN JOHNS";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.ContactPK = consignorContact.PK;
			var consignorTaxCode = consignor.CustomsCodes.AddNew();
			consignorTaxCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			consignorTaxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			consignorTaxCode.OK_CustomsRegNo = "11223491505";

			var notifyPart = Factory.New<OrgHeader>();
			notifyPart.OH_FullName = "AUBNE";
			notifyPart.OH_RL_NKClosestPort = "AUBNE";
			notifyPart.MainAddress.OA_RN_NKCountryCode = "AU";
			notifyPart.MainAddress.OA_Fax = "+862160001100";
			notifyPart.MainAddress.OA_Phone = "+862160001000";
			notifyPart.MainAddress.OA_Email = "sales.cnsha@youragent.com";
			var notifyPartContact = notifyPart.Contacts.AddNew();
			notifyPartContact.OC_ContactName = "IMPORT DIVISION";
			var notifyPartTaxCode = notifyPart.CustomsCodes.AddNew();
			notifyPartTaxCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.USC;
			notifyPartTaxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			notifyPartTaxCode.OK_CustomsRegNo = "923456789012345670";
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyPart.MainAddress.PK;
			consol.NotifyPartyDocumentaryAddress.ContactPK = notifyPartContact.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = consol.NotifyPartyDocumentaryAddress.Address.PK;
			shipment.NotifyPartyDocumentaryAddress.ContactPK = consol.NotifyPartyDocumentaryAddress.ContactPK;

			Factory.Save();

			using (FreightConfigurationRegistry.Instance.SendContactCompanyIdInSIBillClauses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = CreateDocDataObjectBuilder(consol).Build();
				AssertNullOrEmpty(data.OtherBillClauses);
			}

			using (FreightConfigurationRegistry.Instance.SendContactCompanyIdInSIBillClauses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = CreateDocDataObjectBuilder(consol).Build();
				var expected = new ZString(@"Shipper- CONTACT:ALAN JOHNS, TEL:+61 2 8300 8900, FAX:+61 2 8300 8900, EMAIL:contact@shipper.com, TAXID:ABN(l)+11223491505
Consignee- CONTACT:PETER WILLIAMS, TEL:+65 2300 89822, EMAIL:contact@consignee.com, TAXID:USC(l)+923456789012345670
Notify Party- CONTACT:IMPORT DIVISION, TEL:+862160001000, FAX:+862160001100, EMAIL:sales.cnsha@youragent.com, TAXID:USC(l)+923456789012345670");
				AssertEquals(expected, data.OtherBillClauses);
			}
		}

		#endregion

		#region TestPopulateForwarderAddress_Registry

		public void TestPopulateForwarderAddress_Registry()
		{
			var consol = Factory.New<ForwardingConsol>();
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			sendingForwarder.MiscServ.OM_FWAsAgentName = "ASD Lines";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			AssertEquals("Precondition", false, consol.IsDirect);

			var carrierMessageData = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(false, carrierMessageData.Forwarder.IsEmpty());

			using (FreightConfigurationRegistry.Instance.SIDefaultForwarderForNonDirectConsols.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				carrierMessageData = new ShippingInstructionBuilder(consol).Build();
				AssertEquals(true, carrierMessageData.Forwarder.IsEmpty());

				consol.JK_AgentType = Constants.AgentType.Direct;
				AssertEquals(true, consol.IsDirect);

				carrierMessageData = new ShippingInstructionBuilder(consol).Build();
				AssertEquals(false, carrierMessageData.Forwarder.IsEmpty());
			}
		}

		#endregion

		#region TestAddHSCodeValidationForIsrael

		public void TestAddHSCodeValidationForIsrael()
		{
			var errorMessage = "Harmonized System Code is required for imports to Israel to comply with Manifest reporting.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "IL8UH";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "IL8UH";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IL8UH";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = string.Empty;
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			var harmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().HarmonizedCode;
			AssertHasMessageError(harmonizedCode.CodeInfo, errorMessage);

			packLine.JL_HarmonisedCode = "11111";
			shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			harmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().HarmonizedCode;
			AssertNoError(harmonizedCode.CodeInfo, errorMessage);
		}

		#endregion

		#region TestAddHSCodeValidationForMalaysia

		public void TestAddHSCodeValidationForMalaysia()
		{
			var errorMessage = "HS Code is required for Sea exports from or imports to Malaysia.";
			var warningMessage = "It is recommended to fill in Harmonized Code to assist with faster booking and reconciliation processes.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "MYABU";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "MYABU";
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = string.Empty;
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			var harmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().HarmonizedCode;

			AssertEquals(1, harmonizedCode.CodeInfo.Notifications.GetMessageErrors().Count());
			AssertHasMessageErrorContaining("Import Harmonized is required for Malaysia", harmonizedCode.CodeInfo, errorMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			void RefreshImportHarmonizedCode()
			{
				shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
				harmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().HarmonizedCode;
			}

			packLine.JL_HarmonisedCode = "11111";
			RefreshImportHarmonizedCode();
			AssertNoMessageErrors("Import Harmonized is required for Malaysia", harmonizedCode.CodeInfo);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = string.Empty;
			var harmonizedCodes = packLine.HarmonisedCodes.AddNew();
			harmonizedCodes.JLH_RN_NKCountry = "MY";
			harmonizedCodes.JLH_Code = "55555";
			RefreshImportHarmonizedCode();
			AssertNoMessageErrors("Import Harmonized is required for Malaysia", harmonizedCode.CodeInfo);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = string.Empty;
			packLine.HarmonisedCodes.DeleteAll();
			RefreshImportHarmonizedCode();
			AssertHasMessageErrorContaining("Harmonized is required for Malaysia", harmonizedCode.CodeInfo, errorMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "MYABU";
			RefreshImportHarmonizedCode();
			AssertHasMessageErrorContaining("Harmonized is required for Malaysia", harmonizedCode.CodeInfo, errorMessage);
			AssertNoWarning(harmonizedCode.CodeInfo, warningMessage);

			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "AUSYD";
			RefreshImportHarmonizedCode();
			AssertNoMessageErrors("Harmonized is required for Malaysia", harmonizedCode.CodeInfo);
			AssertHasWarning(harmonizedCode.CodeInfo, warningMessage);

			packLine.JL_HarmonisedCode = string.Empty;
			packLine.HarmonisedCodes.DeleteAll();
			RefreshImportHarmonizedCode();
			AssertNoMessageErrors("Harmonized is required for Malaysia", harmonizedCode.CodeInfo);
			AssertHasWarning(harmonizedCode.CodeInfo, warningMessage);
		}

		#endregion

		#region AcidNumber

		public void TestAcidNumberValidationForEgypt()
		{
			var errorMessage = "ACID Number is mandatory for cargo with destination Egypt. Enter the ACI in the Additional References on the Consol.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "EGALY";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var num = consol.Numbers.AddNew();
			num.CE_RN_NKCountryCode = "EG";
			num.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			num.CE_EntryNum = "1234567890123456789";
			Factory.Save();

			var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals(true, shippingInstruction.IsToEgypt);
			AssertEquals("1234567890123456789", shippingInstruction.AcidNumber);
			AssertNoMessageError(shippingInstruction.AcidNumberInfo, errorMessage);

			shippingInstruction.AcidNumber = "";
			AssertHasMessageError(shippingInstruction.AcidNumberInfo, errorMessage);
		}

		public void TestPopulateAcidNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "EGALY";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "CNSHA";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var num1 = consol.Numbers.AddNew();
			num1.CE_RN_NKCountryCode = "EG";
			num1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			num1.CE_EntryNum = "1234567890123456789";

			var num2 = consol.Numbers.AddNew();
			num2.CE_RN_NKCountryCode = "EG";
			num2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			num2.CE_EntryNum = "2234567890123456789";

			var num3 = shipment.Numbers.AddNew();
			num3.CE_RN_NKCountryCode = "EG";
			num3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AdvanceCargoInformationReference;
			num3.CE_EntryNum = "3234567890123456789";

			Factory.Save();

			var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals(true, shippingInstruction.IsToEgypt);
			AssertEquals("Populate ACID number from the consol", @"1234567890123456789
2234567890123456789", shippingInstruction.AcidNumber);
		}

		#endregion

		#region AddHSCodeValidation

		public void TestAddHSCodeValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "DEHAM";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "HKHKG";
			transport.JW_RL_NKDiscPort = "TRALI";
			transport.JW_Vessel = "ANRO ASIA";
			transport.JW_VoyageFlight = "324443";
			transport.JW_ETD = new ZDateTime(2019, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "TRALI";
			transport2.JW_RL_NKDiscPort = "DEHAM";
			transport2.JW_Vessel = "ANRO ASIA";
			transport2.JW_VoyageFlight = "324443";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = string.Empty;
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			var shipment1 = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = string.Empty;
			packLine1.JL_PackageCount = 3;
			packLine1.JL_F3_NKPackType = "PLT";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = string.Empty;
			packLine2.JL_PackageCount = 4;
			packLine2.JL_F3_NKPackType = "PLT";

			var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
			var importHarmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().ImportHarmonizedCode;

			AssertHasMessageErrorContaining("Import Harmonized is required for EU", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to the EU for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");

			void RefreshImportHarmonizedCode()
			{
				shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
				importHarmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().ImportHarmonizedCode;
			}

			packLine.JL_HarmonisedCode = string.Empty;
			consol.JK_RL_NKDischargePort = "TRALI";
			transport2.JW_RL_NKDiscPort = "TRALI";
			RefreshImportHarmonizedCode();

			AssertEquals(1, importHarmonizedCode.CodeInfo.Notifications.Count());
			AssertHasMessageErrorContaining("Import Harmonized is required for TR", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to Turkey for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");

			packLine.JL_HarmonisedCode = string.Empty;
			var harmonizedCodes = packLine.HarmonisedCodes.AddNew();
			harmonizedCodes.JLH_RN_NKCountry = "TR";
			harmonizedCodes.JLH_Code = "123";
			consol.JK_RL_NKDischargePort = "TRALI";
			RefreshImportHarmonizedCode();

			AssertEquals(1, importHarmonizedCode.CodeInfo.Notifications.Count());
			AssertHasWarningContaining("TR harmonized code must be at least 4 digits", importHarmonizedCode.CodeInfo, "At least four but recommended six digit Harmonized System Code is required for imports to Turkey for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");

			packLine.JL_HarmonisedCode = string.Empty;
			harmonizedCodes.JLH_RN_NKCountry = "TR";
			harmonizedCodes.JLH_Code = "123abc";
			consol.JK_RL_NKDischargePort = "TRALI";
			RefreshImportHarmonizedCode();

			AssertEquals(1, importHarmonizedCode.CodeInfo.Notifications.Count());
			AssertHasWarningContaining("TR harmonized code must be at least 4 digits", importHarmonizedCode.CodeInfo, "At least four but recommended six digit Harmonized System Code is required for imports to Turkey for the carrier to complete mandatory ENS (Entry Summary Declaration) filing.");
		}

		public void TestAddHSCodeValidationForPH()
		{
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "PHBPH";

				var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
				transport.JW_LegOrder = 1;
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "SGSIN";
				transport.JW_Vessel = "ANRO ASIA";
				transport.JW_VoyageFlight = "324443";
				transport.JW_ETD = new ZDateTime(2019, 12, 1);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_LegOrder = 2;
				transport2.JW_TransportMode = Constants.TransportModes.Sea;
				transport2.JW_TransportType = Constants.TransportPlanningType.Other;
				transport2.JW_RL_NKLoadPort = "SGSIN";
				transport2.JW_RL_NKDiscPort = "PHBPH";
				transport2.JW_Vessel = "ANRO ASIA";
				transport2.JW_VoyageFlight = "324443";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_HarmonisedCode = string.Empty;
				packLine.JL_PackageCount = 2;
				packLine.JL_F3_NKPackType = "PLT";

				var shipment1 = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packLine1 = shipment1.OuterPackLines.AddNew();
				packLine1.JL_HarmonisedCode = string.Empty;
				packLine1.JL_PackageCount = 3;
				packLine1.JL_F3_NKPackType = "PLT";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packLine2 = shipment2.OuterPackLines.AddNew();
				packLine2.JL_HarmonisedCode = string.Empty;
				packLine2.JL_PackageCount = 4;
				packLine2.JL_F3_NKPackType = "PLT";

				var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
				var importHarmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().ImportHarmonizedCode;

				AssertEquals(1, importHarmonizedCode.CodeInfo.Notifications.Count());
				AssertHasMessageErrorContaining("Import Harmonized is required for PH", importHarmonizedCode.CodeInfo, "Harmonized System Code is required for imports to Philippines in line with Customs Order No. 48-2019.");

				void RefreshImportHarmonizedCode()
				{
					shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
					importHarmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().ImportHarmonizedCode;
				}

				packLine.JL_HarmonisedCode = string.Empty;
				var harmonizedCodes1 = packLine.HarmonisedCodes.AddNew();
				harmonizedCodes1.JLH_RN_NKCountry = "PH";
				harmonizedCodes1.JLH_Code = "12345";
				consol.JK_RL_NKDischargePort = "PHBPH";
				RefreshImportHarmonizedCode();

				AssertHasMessageErrorContaining("PH harmonized code must be at least 6 digits", importHarmonizedCode.CodeInfo, "For imports to Philippines, a six digit HS Code is required, in line with Customs Order No. 48-2019.");

				packLine.JL_HarmonisedCode = string.Empty;
				harmonizedCodes1.JLH_RN_NKCountry = "PH";
				harmonizedCodes1.JLH_Code = "12345abc";
				consol.JK_RL_NKDischargePort = "PHBPH";
				RefreshImportHarmonizedCode();

				AssertHasMessageErrorContaining("PH harmonized code must be at least 6 digits", importHarmonizedCode.CodeInfo, "For imports to Philippines, a six digit HS Code is required, in line with Customs Order No. 48-2019.");
			}

			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "SGSIN";

				var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
				transport.JW_LegOrder = 1;
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
				transport.JW_RL_NKLoadPort = "HKHKG";
				transport.JW_RL_NKDiscPort = "PHBPH";
				transport.JW_Vessel = "ANRO ASIA";
				transport.JW_VoyageFlight = "324443";
				transport.JW_ETD = new ZDateTime(2019, 12, 1);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_LegOrder = 2;
				transport2.JW_TransportMode = Constants.TransportModes.Sea;
				transport2.JW_TransportType = Constants.TransportPlanningType.Other;
				transport2.JW_RL_NKLoadPort = "PHBPH";
				transport2.JW_RL_NKDiscPort = "SGSIN";
				transport2.JW_Vessel = "ANRO ASIA";
				transport2.JW_VoyageFlight = "324443";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_HarmonisedCode = string.Empty;
				packLine.JL_PackageCount = 2;
				packLine.JL_F3_NKPackType = "PLT";

				var shipment1 = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packLine1 = shipment1.OuterPackLines.AddNew();
				packLine1.JL_HarmonisedCode = string.Empty;
				packLine1.JL_PackageCount = 3;
				packLine1.JL_F3_NKPackType = "PLT";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

				var packLine2 = shipment2.OuterPackLines.AddNew();
				packLine2.JL_HarmonisedCode = string.Empty;
				packLine2.JL_PackageCount = 4;
				packLine2.JL_F3_NKPackType = "PLT";

				var shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
				var importHarmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().ImportHarmonizedCode;

				AssertNoNotifications("Import Harmonized is required for PH", importHarmonizedCode.CodeInfo);

				void RefreshImportHarmonizedCode()
				{
					shippingInstruction = CreateDocDataObjectBuilder(consol).Build();
					importHarmonizedCode = (HarmonizedCode)shippingInstruction.Shipments.First().PackingLines.First().ImportHarmonizedCode;
				}

				packLine.JL_HarmonisedCode = "123456";
				var harmonizedCodes1 = packLine.HarmonisedCodes.AddNew();
				harmonizedCodes1.JLH_RN_NKCountry = "PH";
				harmonizedCodes1.JLH_Code = "123456";
				consol.JK_RL_NKDischargePort = "PHBPH";
				RefreshImportHarmonizedCode();

				AssertNoNotifications("PH harmonized code must be at least 6 digits", importHarmonizedCode.CodeInfo);
			}
		}

		#endregion

		#region FreightForwarderReference

		public void TestFreightForwarderReference()
		{
			var consol = CreateConsol();

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "C00000001";
			Factory.Save();

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("FreightForwarderReference", "C00000001", shippingInstruction.FreightForwarderReference);
		}

		#endregion

		#region HIRReference

		public void TestHIRReference()
		{
			var consol = CreateConsol();

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = "HIR123";
			Factory.Save();

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("HIR Reference", "HIR123", shippingInstruction.HIRReference.Value);
		}

		#endregion

		#region TestGoodsHandlingAndSpecialInstructions

		public void TestPopulateSpecialInstructionsNoNotesNoShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "驴100";

			consol.Notes.RemoveAndDeleteAll();

			var shippingInstruction = new ShippingInstructionBuilder(consol);
			var build = shippingInstruction.Build();

			AssertEquals(false, consol.IsDirect);
			AssertEquals("", build.SpecialInstructions);

			consol.JK_AgentType = Constants.AgentType.Direct;
			build = shippingInstruction.Build();

			AssertEquals(true, consol.IsDirect);
			AssertEquals("", build.SpecialInstructions);
		}

		public override void TestPopulateSpecialInstructions()
		{
			var consol = CreateConsol();

			var sendingAgentNote = consol.SendingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "SendingAgent Special instructions");
			sendingAgentNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);

			var shippingInstruction = new ShippingInstructionBuilder(consol);
			var build = shippingInstruction.Build();
			AssertEquals(false, consol.IsDirect);
			Assert(!consol.Notes.VisibleNotes.Contains(sendingAgentNote));
			AssertEquals("", build.SpecialInstructions);

			sendingAgentNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			build = shippingInstruction.Build();
			Assert(consol.Notes.VisibleNotes.Contains(sendingAgentNote));
			AssertEquals("SendingAgent Special instructions", build.SpecialInstructions);

			consol.ShippingLine.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Carrier Special instructions");
			build = shippingInstruction.Build();
			AssertEquals("Carrier Special instructions", build.SpecialInstructions);

			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Special instructions");
			build = shippingInstruction.Build();
			AssertEquals("Special instructions", build.SpecialInstructions);
		}

		public override void TestPopulateSpecialInstructionsForDRTConsol()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.Direct;
			var shipment = consol.Shipments[0];

			var shipper = CreateAddress();
			var shipperNote = shipper.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Shipper Special instructions");
			shipperNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var shippingInstruction = new ShippingInstructionBuilder(consol);
			var build = shippingInstruction.Build();
			AssertEquals(true, consol.IsDirect);
			Assert(!shipment.Notes.VisibleNotes.Contains(shipperNote));
			AssertEquals("", build.SpecialInstructions);

			shipperNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			build = shippingInstruction.Build();
			Assert(shipment.Notes.VisibleNotes.Contains(shipperNote));
			AssertEquals("Shipper Special instructions", build.SpecialInstructions);

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Shipment Special instructions");
			build = shippingInstruction.Build();
			AssertEquals("Shipment Special instructions", build.SpecialInstructions);

			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Special instructions");
			build = shippingInstruction.Build();
			AssertEquals("Special instructions", build.SpecialInstructions);
		}

		public void TestGoodsHandlingInstructions()
		{
			var consol = CreateConsol();

			var sendingAgentNote = consol.SendingForwarder.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "SendingAgent Handling instructions");
			sendingAgentNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);

			var shippingInstruction = new ShippingInstructionBuilder(consol);
			var build = shippingInstruction.Build();
			AssertEquals(false, consol.IsDirect);
			Assert(!consol.Notes.VisibleNotes.Contains(sendingAgentNote));
			AssertEquals("", build.GoodsHandlingInstructions);

			sendingAgentNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			build = shippingInstruction.Build();
			Assert(consol.Notes.VisibleNotes.Contains(sendingAgentNote));
			AssertEquals("SendingAgent Handling instructions", build.GoodsHandlingInstructions);

			consol.ShippingLine.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Carrier Handling instructions");
			build = shippingInstruction.Build();
			AssertEquals("Carrier Handling instructions", build.GoodsHandlingInstructions);

			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instructions");
			build = shippingInstruction.Build();
			AssertEquals("Handling instructions", build.GoodsHandlingInstructions);
		}

		public void TestGoodsHandlingInstructionsForDRTConsol()
		{
			var consol = CreateConsol();
			consol.JK_AgentType = Constants.AgentType.Direct;
			var shipment = consol.Shipments[0];

			var shipper = CreateAddress();
			var shipperNote = shipper.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Shipper Handling instructions");
			shipperNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.I);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var shippingInstruction = new ShippingInstructionBuilder(consol);
			var build = shippingInstruction.Build();
			AssertEquals(true, consol.IsDirect);
			Assert(!shipment.Notes.VisibleNotes.Contains(shipperNote));
			AssertEquals("", build.GoodsHandlingInstructions);

			shipperNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			build = shippingInstruction.Build();
			Assert(shipment.Notes.VisibleNotes.Contains(shipperNote));
			AssertEquals("Shipper Handling instructions", build.GoodsHandlingInstructions);

			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Shipment Handling instructions");
			build = shippingInstruction.Build();
			AssertEquals("Shipment Handling instructions", build.GoodsHandlingInstructions);

			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling instructions");
			build = shippingInstruction.Build();
			AssertEquals("Handling instructions", build.GoodsHandlingInstructions);
		}

		#endregion

		#region TestCarrierMessagingRequirementsValidation

		public void TestValidateBOLDocumentationProvider_IsDirectConsol()
		{
			var errorMessage = "This carrier supports electronic bill of lading and the shipper is requesting an electronic bill of lading.\r\nPlease choose your preferred eBL Provider. Select <Not Listed> when the preferred provider is not listed.";
			var warningMessage = "This carrier supports electronic bill of lading and the shipper is requesting an electronic bill of lading.\r\nPlease choose your preferred eBL Provider. Select <Not Listed> when the preferred provider is not listed, no eBL will be issued by the carrier.";
			var eBLProviderNotValidErrorMessage = "eBL Provider is not valid.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_AgentType = Constants.AgentType.Direct;

			var shippingline = Factory.New<OrgHeader>();
			shippingline.OH_FullName = "Carrier";
			shippingline.OH_RL_NKClosestPort = "AUSYD";
			shippingline.MainAddress.Address1 = "UNIT05";
			shippingline.MainAddress.Address2 = "Haha Street";
			shippingline.MainAddress.City = "AUCKLAND";
			shippingline.MainAddress.Postcode = "1050";
			shippingline.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingline.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_ShippingLineAddress = shippingline.MainAddress.PK;

			var shippinglineRefShippingLine = Factory.New<RefShippingLine>();
			shippingline.OH_RSL_ShippingLine = shippinglineRefShippingLine.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "UNIT05";
			consignor.MainAddress.Address2 = "Haha Street";
			consignor.MainAddress.City = "AUCKLAND";
			consignor.MainAddress.Postcode = "1050";
			consignor.MainAddress.OA_RN_NKCountryCode = "NZ";
			consignor.MainAddress.OA_Email = "Flah@Floogle.com";
			consignor.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			consignor.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = true;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var billOfLadingProviderMessagingRequirement = shippinglineRefShippingLine.ShippingLineMessagingRequirements.AddNew();
			billOfLadingProviderMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.BillOfLadingProvider;
			billOfLadingProviderMessagingRequirement.RSR_IsShippingInstruction = true;

			var eblProvider = shippinglineRefShippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider.RSE_IsAvailable = true;
			eblProvider.RSE_Name = EBLProviderConstants.Codes.Bolero;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.EBLProvider.Code = string.Empty;
			AssertEquals(string.Empty, shippingInstruction.EBLProvider.Code);
			AssertHasMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.NotListed;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertHasWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.Bolero;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			consignor.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = false;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.EBLProvider.Code = string.Empty;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.NotListed;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.Bolero;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = "EBLProvider";
			AssertHasMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, eBLProviderNotValidErrorMessage);
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);
		}

		public void TestValidateBOLDocumentationProvider_IsNonDirectConsol()
		{
			var errorMessage = "This carrier supports electronic bill of lading and the shipper is requesting an electronic bill of lading.\r\nPlease choose your preferred eBL Provider. Select <Not Listed> when the preferred provider is not listed.";
			var warningMessage = "This carrier supports electronic bill of lading and the shipper is requesting an electronic bill of lading.\r\nPlease choose your preferred eBL Provider. Select <Not Listed> when the preferred provider is not listed, no eBL will be issued by the carrier.";
			var eBLProviderNotValidErrorMessage = "eBL Provider is not valid.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "TO ORDER";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 399";
			sendingForwarder.MainAddress.Address2 = "50 What Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "5023";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = true;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var shippingline = Factory.New<OrgHeader>();
			shippingline.OH_FullName = "Carrier";
			shippingline.OH_RL_NKClosestPort = "AUSYD";
			shippingline.MainAddress.Address1 = "UNIT05";
			shippingline.MainAddress.Address2 = "Haha Street";
			shippingline.MainAddress.City = "AUCKLAND";
			shippingline.MainAddress.Postcode = "1050";
			shippingline.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingline.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_ShippingLineAddress = shippingline.MainAddress.PK;

			var shippinglineRefShippingLine = Factory.New<RefShippingLine>();
			shippingline.OH_RSL_ShippingLine = shippinglineRefShippingLine.PK;

			var billOfLadingProviderMessagingRequirement = shippinglineRefShippingLine.ShippingLineMessagingRequirements.AddNew();
			billOfLadingProviderMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.BillOfLadingProvider;
			billOfLadingProviderMessagingRequirement.RSR_IsShippingInstruction = true;

			var eblProvider = shippinglineRefShippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider.RSE_IsAvailable = true;
			eblProvider.RSE_Name = EBLProviderConstants.Codes.Bolero;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.EBLProvider.Code = string.Empty;
			AssertEquals(string.Empty, shippingInstruction.EBLProvider.Code);
			AssertHasMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.NotListed;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertHasWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.Bolero;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = false;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			shippingInstruction.EBLProvider.Code = string.Empty;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.NotListed;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = EBLProviderConstants.Codes.Bolero;
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);

			shippingInstruction.EBLProvider.Code = "NotInList";
			AssertHasMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, eBLProviderNotValidErrorMessage);
			AssertNoMessageError(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, errorMessage);
			AssertNoWarning(((CodeDescription)shippingInstruction.EBLProvider).CodeInfo, warningMessage);
		}

		public void TestPopulateElectronicBillOfLadingProviderMandatory()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_AgentType = Constants.AgentType.Direct;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "TO ORDER";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 399";
			sendingForwarder.MainAddress.Address2 = "50 What Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "5023";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = true;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var shippingline = Factory.New<OrgHeader>();
			shippingline.OH_FullName = "Carrier";
			shippingline.OH_RL_NKClosestPort = "AUSYD";
			shippingline.MainAddress.Address1 = "UNIT05";
			shippingline.MainAddress.Address2 = "Haha Street";
			shippingline.MainAddress.City = "AUCKLAND";
			shippingline.MainAddress.Postcode = "1050";
			shippingline.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingline.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_ShippingLineAddress = shippingline.MainAddress.PK;

			var shippingLineRefShippingLine = Factory.New<RefShippingLine>();
			shippingline.OH_RSL_ShippingLine = shippingLineRefShippingLine.PK;

			var billOfLadingProviderMessagingRequirement = shippingLineRefShippingLine.ShippingLineMessagingRequirements.AddNew();
			billOfLadingProviderMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.BillOfLadingProvider;
			billOfLadingProviderMessagingRequirement.RSR_IsShippingInstruction = true;

			var eblProvider = shippingLineRefShippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider.RSE_IsAvailable = false;
			eblProvider.RSE_Name = EBLProviderConstants.Codes.Bolero;

			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			Assert(!shippingInstruction.ElectronicBillOfLadingProviderMandatory);

			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			eblProvider.RSE_IsAvailable = true;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			Assert(shippingInstruction.ElectronicBillOfLadingProviderMandatory);
		}

		public void TestPopulateBOLDocumentationProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "TO ORDER";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 399";
			sendingForwarder.MainAddress.Address2 = "50 What Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "5023";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var shippingline = Factory.New<OrgHeader>();
			shippingline.OH_FullName = "Carrier";
			shippingline.OH_RL_NKClosestPort = "AUSYD";
			shippingline.MainAddress.Address1 = "UNIT05";
			shippingline.MainAddress.Address2 = "Haha Street";
			shippingline.MainAddress.City = "AUCKLAND";
			shippingline.MainAddress.Postcode = "1050";
			shippingline.MainAddress.OA_RN_NKCountryCode = "NZ";
			shippingline.MainAddress.OA_Email = "Flah@Floogle.com";
			consol.JK_OA_ShippingLineAddress = shippingline.MainAddress.PK;

			var shippinglineRefShippingLine = Factory.New<RefShippingLine>();
			shippingline.OH_RSL_ShippingLine = shippinglineRefShippingLine.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "Carrier";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.MainAddress.Address1 = "UNIT05";
			consignor.MainAddress.Address2 = "Haha Street";
			consignor.MainAddress.City = "AUCKLAND";
			consignor.MainAddress.Postcode = "1050";
			consignor.MainAddress.OA_RN_NKCountryCode = "NZ";
			consignor.MainAddress.OA_Email = "Flah@Floogle.com";
			consignor.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			consignor.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var billOfLadingProviderMessagingRequirement = shippinglineRefShippingLine.ShippingLineMessagingRequirements.AddNew();
			billOfLadingProviderMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.BillOfLadingProvider;
			billOfLadingProviderMessagingRequirement.RSR_IsShippingInstruction = true;

			var eblProvider = shippinglineRefShippingLine.ShippingLineEBLProviders.AddNew();
			eblProvider.RSE_IsAvailable = true;
			eblProvider.RSE_Name = EBLProviderConstants.Codes.Bolero;
			eblProvider.RSE_IsDefault = true;

			consol.JK_AgentType = Constants.AgentType.Direct;
			consignor.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = false;
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(EBLProviderConstants.Codes.NotListed, shippingInstruction.EBLProvider.Code);

			consignor.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(EBLProviderConstants.Codes.Bolero, shippingInstruction.EBLProvider.Code);

			consol.JK_AgentType = Constants.AgentType.Agent;
			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = false;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(EBLProviderConstants.Codes.NotListed, shippingInstruction.EBLProvider.Code);

			sendingForwarder.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = true;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			AssertEquals(EBLProviderConstants.Codes.Bolero, shippingInstruction.EBLProvider.Code);
		}

		public void TestCarrierMessagingRequirementsValidation_IEL()
		{
			var errorMessage = "This carrier only supports integration via email to local office.\r\nContact name and email address are required to send Shipping Instruction.\r\nPlease maintain contact name and email address in carrier Organization > Contact > Email and Receiving Documents > Group SHP.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "MYABU";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			var contact = carrier.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_ContactName = "TEST NAME";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			shippingLine.RSL_ShippingInstructionAvailable = true;
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNullOrEmpty(carrierMessageData.Recipient.Contact);

			shippingLine.RSL_ShippingInstructionAvailable = false;
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertEquals("TEST NAME", carrierMessageData.Recipient.Contact);

			var shippingLineMessagingRequirement = shippingLine.ShippingLineMessagingRequirements.AddNew();
			shippingLineMessagingRequirement.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.IntegrationViaEmailToCarrierLocalOffice;
			shippingLineMessagingRequirement.RSR_IsShippingInstruction = true;

			shippingLine.RSL_ShippingInstructionAvailable = true;

			AssertEquals("test@test.com", carrierMessageData.Recipient.Email);
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			contact.Documents.RemoveAll();
			shippingLine.RSL_ShippingInstructionAvailable = false;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertHasMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);

			document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "SHP";
			shippingLine.RSL_ShippingInstructionAvailable = true;

			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			AssertNoMessageError(carrierMessageData.Recipient.ContactInfo, errorMessage);
		}

		#endregion

		#region TestBrazil
		public void TestIsBrazilExport()
		{
			var brCode1 = "BRRIO";
			var brCode2 = "BRSAO";

			var nonBRCode1 = "SGSIN";
			var nonBRCode2 = "AUSYD";

			var consol = CreateConsol();
			var firstTransport = (Freight.Business.Transport)consol.Transports.First();
			var lastTransport = (Freight.Business.Transport)consol.Transports.Last();

			var shippingInstructionBuilder = new ShippingInstructionBuilder(consol);

			firstTransport.JW_RL_NKLoadPort = brCode1;
			lastTransport.JW_RL_NKDiscPort = nonBRCode1;
			var carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsBrazilExport should be true for exports from BR.", carrierMessageData.IsBrazilExport, true);

			lastTransport.JW_RL_NKDiscPort = brCode2;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsBrazilExport should be false for domestic consols.", carrierMessageData.IsBrazilExport, false);

			firstTransport.JW_RL_NKLoadPort = nonBRCode1;
			lastTransport.JW_RL_NKDiscPort = nonBRCode2;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsBrazilExport should be false when load port is not BR.", carrierMessageData.IsBrazilExport, false);

			lastTransport.JW_RL_NKDiscPort = brCode1;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsBrazilExport should be false for BR imports.", carrierMessageData.IsBrazilExport, false);
		}

		public void TestPopulateRUCNumber()
		{
			var consol = CreateConsol();
			var shipment = consol.Shipments[0];

			var shipmentRUC1 = shipment.Numbers.AddNew();
			shipmentRUC1.CE_EntryType = "RUC";
			shipmentRUC1.CE_EntryNum = "Shipment RUC 111";
			shipmentRUC1.CE_RN_NKCountryCode = "BR";
			var shipmentRUC2 = shipment.Numbers.AddNew();
			shipmentRUC2.CE_EntryType = "RUC";
			shipmentRUC2.CE_EntryNum = "Shipment RUC 222";
			shipmentRUC2.CE_RN_NKCountryCode = "BR";
			var shipmentRUC3 = shipment.Numbers.AddNew();
			shipmentRUC3.CE_EntryType = "RUC";
			shipmentRUC3.CE_EntryNum = "Shipment RUC 222";
			shipmentRUC3.CE_RN_NKCountryCode = "BR";

			Factory.Save();

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("RUCNUmber from Shipment > Additional Details > Reference Numbers", "Shipment RUC 111,Shipment RUC 222", shippingInstruction.RUCNumber);

			var rucNumber1 = consol.Numbers.AddNew();
			rucNumber1.CE_EntryType = "RUC";
			rucNumber1.CE_EntryNum = "Consol RUC 111";
			rucNumber1.CE_RN_NKCountryCode = "BR";
			var rucNumber2 = consol.Numbers.AddNew();
			rucNumber2.CE_EntryType = "RUC";
			rucNumber2.CE_EntryNum = "Consol RUC 222";
			rucNumber2.CE_RN_NKCountryCode = "BR";
			var rucNumber3 = consol.Numbers.AddNew();
			rucNumber3.CE_EntryType = "RUC";
			rucNumber3.CE_EntryNum = "Consol RUC 222";
			rucNumber3.CE_RN_NKCountryCode = "BR";

			Factory.Save();

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("RUCNUmber from consol > Details > Numbers", "Consol RUC 111,Consol RUC 222", shippingInstruction.RUCNumber);
		}
		#endregion

		public void TestShippingInstructionPopulatePorts()
		{
			var frtChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			frtChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			frtChargeCode.AC_Code = "MYFREIGHT";
			frtChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			frtChargeCode.AC_Desc = "Description";
			frtChargeCode.AC_ChargeGroup = "FRT";

			var nonFrtChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			nonFrtChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			nonFrtChargeCode.AC_Code = "NOTFREIGHT";
			nonFrtChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			nonFrtChargeCode.AC_Desc = "BLAH";
			nonFrtChargeCode.AC_ChargeGroup = "ORG";

			Env.Registry.FreightChargeCode = frtChargeCode.PK.ToGuid();

			Factory.Save();

			var consol = CreateConsol();
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			var consolCost = (BusinessObject)Factory.New<IJobConsolCost>();
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode] = frtChargeCode.PK;
			consolCost[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;

			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("OperationalPort", "CNSHA", shippingInstruction.OperationalPort.Code);
			AssertEquals("FreighPayableAt PPD", "CNNJI", shippingInstruction.FreightPayableAt.Code);

			AssertNoMessageError(shippingInstruction.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(shippingInstruction.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("OperationalPort", "CNSHA", shippingInstruction.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "AUSYD", shippingInstruction.FreightPayableAt.Code);

			AssertNoMessageError(shippingInstruction.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(shippingInstruction.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consolCost.Delete();
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("OperationalPort", "CNSHA", shippingInstruction.OperationalPort.Code);
			AssertEquals("FreighPayableAt PPD", "CNSHA", shippingInstruction.FreightPayableAt.Code);

			AssertNoMessageError(shippingInstruction.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(shippingInstruction.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("OperationalPort", "CNSHA", shippingInstruction.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "AUSYD", shippingInstruction.FreightPayableAt.Code);

			AssertNoMessageError(shippingInstruction.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertNoMessageError(shippingInstruction.FreightPayableAt.CodeInfo, "Freight Payable At is required.");

			consol.JK_RL_NKLoadPort = "ABCDE";
			consol.JK_RL_NKDischargePort = "ABCDE";
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("OperationalPort", "ABCDE", shippingInstruction.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", "ABCDE", shippingInstruction.FreightPayableAt.Code);

			AssertHasMessageError(shippingInstruction.OperationalPort.CodeInfo, "You have not entered a valid un loco.");
			AssertHasMessageError(shippingInstruction.FreightPayableAt.CodeInfo, "You have not entered a valid un loco.");

			consol.JK_RL_NKLoadPort = string.Empty;
			consol.JK_RL_NKDischargePort = string.Empty;
			shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("OperationalPort", string.Empty, shippingInstruction.OperationalPort.Code);
			AssertEquals("FreighPayableAt CCX", string.Empty, shippingInstruction.FreightPayableAt.Code);

			AssertHasMessageError(shippingInstruction.OperationalPort.CodeInfo, "Operational Port is required.");
			AssertHasMessageError(shippingInstruction.FreightPayableAt.CodeInfo, "Freight Payable At is required.");
		}

		#region TestSealNumberValidationErrorMessage
		public void TestSealNumberValidationErrorMessage()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var container1 = Factory.New<ForwardingContainer>();
			consol.Containers.Add(container1);

			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_SealNum = "0123456789123456";
			consol.Containers.Add(container2);

			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "TestOrg";
			var address = Factory.New<OrgAddress>();
			address.OA_OH = organization.PK;
			consol.JK_OA_ShippingLineAddress = address.PK;

			var shippingLine = Factory.New<RefShippingLine>();
			shippingLine.RSL_OceanCarrierMessagingAvailable = true;
			organization.OH_RSL_ShippingLine = shippingLine.PK;

			var sel = Factory.New<RefShippingLineMessagingRequirement>();
			sel.RSR_IsShippingInstruction = true;
			sel.RSR_RST_NKType = ShippingLineMessagingRequirement.Types.SealNumberMandatory;
			sel.RSR_RSL_ShippingLine = shippingLine.PK;

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();

			Container[] containers = shippingInstruction.Containers.ToArray();
			CombineAssertions(() =>
			{
				AssertHasMessageError("Seal number is mandatory if SEL ticked in Shipping Line.", containers[0].SealInfo, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForExists);
				AssertHasMessageError("Seal number cannot exceed 15 characters.", containers[1].SealInfo, ShippingLineMessagingRequirement.ValidationMessages.SealNumberMandatoryForLength);
			});
		}

		#endregion

		public void TestPackageGrouping_Not_DNG_DUENumber()
		{
			CreateRefCountryRules(Constants.CountryCodes.Brazil, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "BRSAO";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment1.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment2.DetailedGoodsDescriptionNoteText = "SHIPMENT2 DetailedGoodsDescriptionNoteText";
			shipment2.JS_GoodsDescription = "SHIPMENT2 JS_GoodsDescription";
			shipment2.JS_MarksAndNumbers = "SHIPMENT2 JS_MarksAndNumbers";
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();
			shipment2.CusEntryNumbers.RemoveAndDeleteAll();

			var shipment1CusEntryNumber = shipment1.CusEntryNumbers.AddNew();
			shipment1CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Brazil.DUE;
			shipment1CusEntryNumber.CE_EntryNum = "TEST1";

			var shipment1CusEntryNumber2 = shipment1.CusEntryNumbers.AddNew();
			shipment1CusEntryNumber2.CE_EntryType = CusEntryNumberTypes.Brazil.DUE;
			shipment1CusEntryNumber2.CE_EntryNum = "TEST2";

			var shipment2CusEntryNumber = shipment2.CusEntryNumbers.AddNew();
			shipment2CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Brazil.DUE;
			shipment2CusEntryNumber.CE_EntryNum = "TEST1";

			var shipment2CusEntryNumber2 = shipment2.CusEntryNumbers.AddNew();
			shipment2CusEntryNumber2.CE_EntryType = CusEntryNumberTypes.Brazil.DUE;
			shipment2CusEntryNumber2.CE_EntryNum = "TEST3";

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;

			var s1OuterPackLine2 = PopulatePackLine(shipment2.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				var shipment1GroupedPackingLine = shipmentDO1.PackingLines.First();
				var shipment2GroupedPackingLine = shipmentDO2.PackingLines.First();

				AssertEquals("TEST1, TEST2", shipment1GroupedPackingLine.GroupDUENumber);
				AssertEquals("TEST1, TEST3", shipment2GroupedPackingLine.GroupDUENumber);
			}
		}

		public void TestPackageGrouping_Not_DNG_POFNumber()
		{
			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.Australia, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_GoodsDescription = "SHIPMENT1 JS_GoodsDescription";
			shipment1.JS_MarksAndNumbers = "SHIPMENT1 JS_MarksAndNumbers";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment1.JS_RL_NKOrigin = "USCHI";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.DocsAndCartage.JP_ExportStatement = "LOW";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment2.DetailedGoodsDescriptionNoteText = "SHIPMENT2 DetailedGoodsDescriptionNoteText";
			shipment2.JS_GoodsDescription = "SHIPMENT2 JS_GoodsDescription";
			shipment2.JS_MarksAndNumbers = "SHIPMENT2 JS_MarksAndNumbers";
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;

			var s1OuterPackLine2 = PopulatePackLine(shipment2.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, null, null, true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				var shipment1GroupedPackingLine = shipmentDO1.PackingLines.First();
				var shipment2GroupedPackingLine = shipmentDO2.PackingLines.First();

				AssertEquals("NOEEI §30.37(a)", shipment1GroupedPackingLine.GroupPOFNumber);
				AssertEquals(string.Empty, shipment2GroupedPackingLine.GroupPOFNumber);
			}
		}

		public void TestPackageGrouping_Not_DNG_ITNNumber()
		{
			CreateRefCountryRules(Constants.CountryCodes.UnitedStates, Constants.CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment1.DocsAndCartage.JP_ExportStatement = "LOW";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPMENT2";
			shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipment2.JS_UnitOfVolume = Constants.Volume.Litre;
			shipment2.DetailedGoodsDescriptionNoteText = "SHIPMENT2 DetailedGoodsDescriptionNoteText";
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;

			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment2.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();
			shipment2.CusEntryNumbers.RemoveAndDeleteAll();

			var shipment1CusEntryNumber = shipment1.CusEntryNumbers.AddNew();
			shipment1CusEntryNumber.CE_EntryType = Customs.Common.US.CusEntryNumberTypeList.Codes.ITN;
			shipment1CusEntryNumber.CE_EntryNum = "X20230316311995";

			var shipment2CusEntryNumber = shipment2.CusEntryNumbers.AddNew();
			shipment2CusEntryNumber.CE_EntryType = Customs.Common.US.CusEntryNumberTypeList.Codes.ITN;
			shipment2CusEntryNumber.CE_EntryNum = "X20230316311900";

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;

			var shipment2OuterPackLine = shipment2.OuterPackLines.AddNew();
			PopulatePackLine(shipment2OuterPackLine, 100, Constants.PkgUnit.Keg, 100, Constants.Weight.Kilograms, 100, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Centigrade).JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipmentDO2 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT2");

				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));
				var shipment1GroupedPackingLine2 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));
				var shipment2GroupedPackingLine1 = shipmentDO2.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));

				AssertEquals("X20230316311995", shipment1GroupedPackingLine1.GroupITNNumber);
				AssertEquals("X20230316311995", shipment1GroupedPackingLine2.GroupITNNumber);

				AssertEquals("X20230316311900", shipment2GroupedPackingLine1.GroupITNNumber);
			}
		}

		public void TestPackageGrouping_Not_DNG_CTKNumber()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "GHACC";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_RL_NKDestination = "GHACC";

			var consolCTKNumber = consol.Numbers.AddNew();
			consolCTKNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
			consolCTKNumber.CE_EntryNum = "0001";
			consolCTKNumber.CE_RN_NKCountryCode = "GH";

			var shipmentCTKNumber = shipment1.Numbers.AddNew();
			shipmentCTKNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CargoTrackingNote;
			shipmentCTKNumber.CE_EntryNum = "0002";
			shipmentCTKNumber.CE_RN_NKCountryCode = "GH";

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");
				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));

				AssertEquals("0001", shipment1GroupedPackingLine1.GroupCTKNumber);
			}
		}

		public void TestFreightAsAgreedSelectedWhenIsPayableElsewhereSelected()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();
			carrierMessageData.IsFreightAsAgreed = false;

			OptionalCharge optionalChargeBasicFreight = (OptionalCharge)carrierMessageData.OptionalChargeBasicFreight;
			optionalChargeBasicFreight.IsPayableElsewhere = true;
			AssertEquals("When Payable Elsewhere selected, Freight as Agreed selected by default also.", true, carrierMessageData.IsFreightAsAgreed);
		}

		#region ExportStatementFields

		public void TestPopulateExportStatementFieldsForUSOverseasTerritories()
		{
			var countryCode = CountryCodes.PuertoRico;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
				var countrySetting = countrySettingCollection[countryCode];

				var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
				SetupStatementSetting(statementSetting);
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_ConsolMode = ContainerModes.FCL;

				var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
				transport.JW_LegOrder = 1;
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
				transport.JW_RL_NKLoadPort = "PRSJU";
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_Vessel = "Dragon";
				transport.JW_VoyageFlight = "111";
				transport.JW_ETD = new ZDateTime(2023, 12, 31);

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment.JS_UniqueConsignRef = "SHP005000";

				SetupConsolAndShipmentData(consol, shipment, countryCode);

				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				var firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);

				AssertEquals(ZString.Empty, firstShipmentDO.ExportStatement);
				AssertEquals(ZString.Empty, firstShipmentDO.ExportStatementField1Type);
				AssertEquals(ZString.Empty, firstShipmentDO.ExportStatementField1Code);
				AssertEquals(ZString.Empty, firstShipmentDO.ExportStatementField2Type);
				AssertEquals(ZString.Empty, firstShipmentDO.ExportStatementField2Code);

				shipment.JS_RL_NKOrigin = "PRSJU";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;

				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);

				AssertEquals(statementSetting.Statement, firstShipmentDO.ExportStatement);
				AssertEquals(SEDStatementFieldType.Codes.AgentEIN, firstShipmentDO.ExportStatementField1Type);
				AssertEquals("99-1234567890", firstShipmentDO.ExportStatementField1Code);
				AssertEquals(SEDStatementFieldType.Codes.FilerID, firstShipmentDO.ExportStatementField2Type);
				AssertEquals("12345678", firstShipmentDO.ExportStatementField2Code);

				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.SplitShipments, ZString.Empty);
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.XTN, ZString.Empty);
				AssertPopulateExportStatementField1(ZString.Empty, ZString.Empty);

				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.DateOfExport, "12-31-2023");
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.ITN, "ITN123");
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.ShipperEIN, "99-12345555");
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.ShipperEINAndFilerID, "99-12345555, 12346666");

				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.SRN, "BookRef123");
				shipment.JS_BookingReference = ZString.Empty;
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.SRN, "SHP005000");

				void AssertPopulateExportStatementField1(ZString field1Type, ZString expectedField1Code)
				{
					statementSetting.Field1 = field1Type;
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);
					var shippingInstruction2 = new ShippingInstructionBuilder(consol).Build();
					var firstShipmentDO2 = shippingInstruction2.Shipments.FirstOrDefault();

					AssertEquals(field1Type, firstShipmentDO2.ExportStatementField1Type);
					AssertEquals(expectedField1Code, firstShipmentDO2.ExportStatementField1Code);
				}
			}
		}

		public void TestPopulateExportStatementFieldsForUSOverseasTerritories_ForPackageGrouping()
		{
			var countryCode = CountryCodes.PuertoRico;
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
				var countrySetting = countrySettingCollection[countryCode];

				var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
				SetupStatementSetting(statementSetting);
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_ConsolMode = ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "PRSJU";
				consol.JK_RL_NKDischargePort = "AUSYD";
				var container = consol.Containers.AddNew();

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = TransportModes.Sea;
				shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment1.JS_UniqueConsignRef = "SHP005000";
				var packLine1 = shipment1.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;

				SetupConsolAndShipmentData(consol, shipment1, countryCode);

				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				var firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);

				var firstPackLine = firstShipmentDO.PackingLines.First();
				AssertEquals(ZString.Empty, firstPackLine.GroupPOFNumber);
				AssertEquals(ZString.Empty, firstPackLine.GroupExportStatementField1Type);
				AssertEquals(ZString.Empty, firstPackLine.GroupExportStatementField1Code);
				AssertEquals(ZString.Empty, firstPackLine.GroupExportStatementField2Type);
				AssertEquals(ZString.Empty, firstPackLine.GroupExportStatementField2Code);

				shipment1.JS_RL_NKOrigin = "PRSJU";
				shipment1.JS_RL_NKDestination = "AUSYD";
				shipment1.DocsAndCartage.JP_ExportStatement = statementSetting.Code;

				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);

				firstPackLine = firstShipmentDO.PackingLines.First();
				AssertEquals(statementSetting.Statement, firstPackLine.GroupPOFNumber);
				AssertEquals(SEDStatementFieldType.Codes.AgentEIN, firstPackLine.GroupExportStatementField1Type);
				AssertEquals("99-1234567890", firstPackLine.GroupExportStatementField1Code);
				AssertEquals(SEDStatementFieldType.Codes.FilerID, firstPackLine.GroupExportStatementField2Type);
				AssertEquals("12345678", firstPackLine.GroupExportStatementField2Code);
			}
		}

		public void TestValidateExportStatementFields()
		{
			var countryCode = CountryCodes.PuertoRico;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
				var countrySetting = countrySettingCollection[countryCode];
				countrySetting.CountryCode = GlbBranch.CurrentBranch.Country.Code;

				var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
				SetupStatementSetting(statementSetting);
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_ConsolMode = ContainerModes.FCL;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment.JS_UniqueConsignRef = "SHP005000";

				shipment.JS_RL_NKOrigin = "PRSJU";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;

				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				var firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);

				var warningMessage1 = $"SHP005000 Export Statement {statementSetting.Code} requires the Agent's EIN. Please enter a value or change the Export Statement requirements in Registry > Freight > Shipment > Export Statements.";
				var warningMessage2 = $"SHP005000 Export Statement {statementSetting.Code} requires the Filer ID. Please enter a value or change the Export Statement requirements in Registry > Freight > Shipment > Export Statements.";
				var errorMessage = "This field exceeds the maximum 35 character code limit.";

				AssertHasWarning(firstShipmentDO.ExportStatementField1CodeInfo, warningMessage1);
				AssertHasWarning(firstShipmentDO.ExportStatementField2CodeInfo, warningMessage2);
				AssertNoMessageError(firstShipmentDO.ExportStatementField1CodeInfo, errorMessage);

				SetupConsolAndShipmentData(consol, shipment, countryCode);
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);

				AssertNoWarning(firstShipmentDO.ExportStatementField1CodeInfo, warningMessage1);
				AssertNoWarning(firstShipmentDO.ExportStatementField2CodeInfo, warningMessage2);
				AssertNoMessageError(firstShipmentDO.ExportStatementField1CodeInfo, errorMessage);

				var orgCusCode = consol.SendingForwarder.GetRegistrationNumberObject(countryCode, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
				orgCusCode.SecuredCustomsRegNo = "99-123456789012345678901234567890123";
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);
				AssertHasMessageError(firstShipmentDO.ExportStatementField1CodeInfo, errorMessage);

				orgCusCode.SecuredCustomsRegNo = "99-12345678901234567890123456789012";
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);
				AssertNoMessageError(firstShipmentDO.ExportStatementField1CodeInfo, errorMessage);
			}
		}

		public void TestValidateExportStatementFields_ForShippersEINAndFilerID()
		{
			var countryCode = CountryCodes.PuertoRico;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
				var countrySetting = countrySettingCollection[countryCode];
				countrySetting.CountryCode = GlbBranch.CurrentBranch.Country.Code;

				var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
				SetupStatementSetting(statementSetting);
				statementSetting.Field1 = SEDStatementFieldType.Codes.ShipperEINAndFilerID;
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_ConsolMode = ContainerModes.FCL;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment.JS_UniqueConsignRef = "SHP005000";

				shipment.JS_RL_NKOrigin = "PRSJU";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;

				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				var firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);

				var warningMessage1 = $"SHP005000 Export Statement {statementSetting.Code} requires the Shipper's EIN and Filer ID. Please enter a value or change the Export Statement requirements in Registry > Freight > Shipment > Export Statements.";
				AssertHasWarning(firstShipmentDO.ExportStatementField1CodeInfo, warningMessage1);

				SetupConsolAndShipmentData(consol, shipment, countryCode);
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstShipmentDO = shippingInstruction.Shipments.FirstOrDefault();
				AssertNotNull(firstShipmentDO);
				AssertNoWarning(firstShipmentDO.ExportStatementField1CodeInfo, warningMessage1);
			}
		}

		public void TestValidateExportStatementFields_ForPackageGrouping()
		{
			var countryCode = CountryCodes.PuertoRico;
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
				var countrySetting = countrySettingCollection[countryCode];
				countrySetting.CountryCode = countryCode;

				var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
				SetupStatementSetting(statementSetting);
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_ConsolMode = ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "PRSJU";
				consol.JK_RL_NKDischargePort = "AUSYD";
				var container = consol.Containers.AddNew();

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = TransportModes.Sea;
				shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment1.JS_UniqueConsignRef = "SHP005000";
				var packLine1 = shipment1.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;

				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				shipment1.JS_RL_NKOrigin = "PRSJU";
				shipment1.JS_RL_NKDestination = "AUSYD";
				shipment1.DocsAndCartage.JP_ExportStatement = statementSetting.Code;

				var warningMessage1 = $"SHP005000 Export Statement {statementSetting.Code} requires the Agent's EIN. Please enter a value or change the Export Statement requirements in Registry > Freight > Shipment > Export Statements.";
				var warningMessage2 = $"SHP005000 Export Statement {statementSetting.Code} requires the Filer ID. Please enter a value or change the Export Statement requirements in Registry > Freight > Shipment > Export Statements.";
				var errorMessage = "This field exceeds the maximum 35 character code limit.";

				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				var firstPackLine = shippingInstruction.Shipments.FirstOrDefault()?.PackingLines.FirstOrDefault();
				AssertNotNull(firstPackLine);
				AssertHasWarning(firstPackLine.GroupExportStatementField1CodeInfo, warningMessage1);
				AssertHasWarning(firstPackLine.GroupExportStatementField2CodeInfo, warningMessage2);
				AssertNoMessageError(firstPackLine.GroupExportStatementField1CodeInfo, errorMessage);

				SetupConsolAndShipmentData(consol, shipment1, countryCode);
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstPackLine = shippingInstruction.Shipments.FirstOrDefault()?.PackingLines.FirstOrDefault();
				AssertNotNull(firstPackLine);
				AssertNoWarning(firstPackLine.GroupExportStatementField1CodeInfo, warningMessage1);
				AssertNoWarning(firstPackLine.GroupExportStatementField2CodeInfo, warningMessage2);
				AssertNoMessageError(firstPackLine.GroupExportStatementField1CodeInfo, errorMessage);

				var orgCusCode = consol.SendingForwarder.GetRegistrationNumberObject(countryCode, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
				orgCusCode.SecuredCustomsRegNo = "99-123456789012345678901234567890123";
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstPackLine = shippingInstruction.Shipments.FirstOrDefault()?.PackingLines.FirstOrDefault();
				AssertNotNull(firstPackLine);
				AssertHasMessageError(firstPackLine.GroupExportStatementField1CodeInfo, errorMessage);

				orgCusCode.SecuredCustomsRegNo = "99-12345678901234567890123456789012";
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstPackLine = shippingInstruction.Shipments.FirstOrDefault()?.PackingLines.FirstOrDefault();
				AssertNotNull(firstPackLine);
				AssertNoMessageError(firstPackLine.GroupExportStatementField1CodeInfo, errorMessage);
			}
		}

		public void TestValidateExportStatementFields_ForPackageGrouping_And_ShippersEINAndFilerID()
		{
			var countryCode = CountryCodes.PuertoRico;
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
				var countrySetting = countrySettingCollection[countryCode];
				countrySetting.CountryCode = countryCode;

				var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
				SetupStatementSetting(statementSetting);
				statementSetting.Field1 = SEDStatementFieldType.Codes.ShipperEINAndFilerID;
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_ConsolMode = ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "PRSJU";
				consol.JK_RL_NKDischargePort = "AUSYD";
				var container = consol.Containers.AddNew();

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = TransportModes.Sea;
				shipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment1.JS_UniqueConsignRef = "SHP005000";
				var packLine1 = shipment1.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;

				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				shipment1.JS_RL_NKOrigin = "PRSJU";
				shipment1.JS_RL_NKDestination = "AUSYD";
				shipment1.DocsAndCartage.JP_ExportStatement = statementSetting.Code;

				var warningMessage1 = $"SHP005000 Export Statement {statementSetting.Code} requires the Shipper's EIN and Filer ID. Please enter a value or change the Export Statement requirements in Registry > Freight > Shipment > Export Statements.";

				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				var firstPackLine = shippingInstruction.Shipments.FirstOrDefault()?.PackingLines.FirstOrDefault();
				AssertNotNull(firstPackLine);
				AssertHasWarning(firstPackLine.GroupExportStatementField1CodeInfo, warningMessage1);

				SetupConsolAndShipmentData(consol, shipment1, countryCode);
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;

				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				firstPackLine = shippingInstruction.Shipments.FirstOrDefault()?.PackingLines.FirstOrDefault();
				AssertNotNull(firstPackLine);
				AssertNoWarning(firstPackLine.GroupExportStatementField1CodeInfo, warningMessage1);
			}
		}

		void SetupStatementSetting(ExportStatementSetting statementSetting)
		{
			statementSetting.Visibility = "UDF";
			statementSetting.Field1 = SEDStatementFieldType.Codes.AgentEIN;
			statementSetting.Field2 = SEDStatementFieldType.Codes.FilerID;
			statementSetting.UseOnDirectMasterBillOfLading = true;
			statementSetting.UseOnConsolidationMasterBillOfLading = true;
		}

		void SetupConsolAndShipmentData(ForwardingConsol consol, ForwardingShipment shipment, ZString countryCode)
		{
			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_IsForwarder = true;
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.OH_Code = "SENDAGENT";

			var sendingAgentEinCode = sendingAgent.CustomsCodes.AddNew();
			sendingAgentEinCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			sendingAgentEinCode.SecuredCustomsRegNo = "99-1234567890";
			sendingAgentEinCode.OK_RN_NKCodeCountry = countryCode;

			var sendingAgentEnfCode = sendingAgent.CustomsCodes.AddNew();
			sendingAgentEnfCode.OK_CodeType = OrgCusCode.USACodeTypes.EntryFilerCode;
			sendingAgentEnfCode.SecuredCustomsRegNo = "12345678";
			sendingAgentEnfCode.OK_RN_NKCodeCountry = countryCode;

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			shipment.JS_E_DEP = new ZDateTime(2023, 12, 31);
			shipment.JS_BookingReference = "BookRef123";

			shipment.CustomsEntryNumberType = Customs.Common.US.CusEntryNumberTypeList.Codes.ITN;
			shipment.CustomsEntryNumber = "ITN123";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_IsConsignor = true;
			sendingAgent.OH_FullName = "Consignor";
			sendingAgent.OH_Code = "CONSIGNOR";
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			var consignorEinCode = consignor.CustomsCodes.AddNew();
			consignorEinCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			consignorEinCode.SecuredCustomsRegNo = "99-12345555";
			consignorEinCode.OK_RN_NKCodeCountry = countryCode;

			var consignorEnfCode = consignor.CustomsCodes.AddNew();
			consignorEnfCode.OK_CodeType = OrgCusCode.USACodeTypes.EntryFilerCode;
			consignorEnfCode.SecuredCustomsRegNo = "12346666";
			consignorEnfCode.OK_RN_NKCodeCountry = countryCode;
		}

		public void TestPopulateExportStatementFields_DateOfExport()
		{
			var countryCode = CountryCodes.PuertoRico;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var countrySettingCollection = FreightDataRegistry.Instance.ExportStatementSettings.Value;
				var countrySetting = countrySettingCollection[countryCode];
				var statementSetting = countrySetting.Statements.OfType<ExportStatementSetting>().First();
				SetupStatementSetting(statementSetting);
				FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_ConsolMode = ContainerModes.FCL;

				var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
				transport.JW_LegOrder = 1;
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
				transport.JW_RL_NKLoadPort = "AUCBR";
				transport.JW_RL_NKDiscPort = "USATL";
				transport.JW_Vessel = "Dragon";
				transport.JW_VoyageFlight = "111";
				transport.JW_ETD = new ZDateTime(2018, 12, 1);

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
				shipment.JS_UniqueConsignRef = "SHP005000";
				shipment.JS_RL_NKOrigin = "PRSJU";
				shipment.JS_RL_NKDestination = "USATL";
				shipment.DocsAndCartage.JP_ExportStatement = statementSetting.Code;
				SetupConsolAndShipmentData(consol, shipment, countryCode);

				var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.DateOfExport, string.Empty);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_LegOrder = 2;
				transport2.JW_TransportMode = Constants.TransportModes.Sea;
				transport2.JW_TransportType = Constants.TransportPlanningType.Other;
				transport2.JW_RL_NKLoadPort = "USATL";
				transport2.JW_RL_NKDiscPort = "NZAKL";
				transport2.JW_Vessel = "Steven";
				transport2.JW_VoyageFlight = "222";
				transport2.JW_ETD = new ZDateTime(2018, 12, 5);
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.DateOfExport, "12-05-2018");

				transport2.JW_RL_NKDiscPort = "USLAX";
				var transport3 = consol.Transports.AddNew();
				transport3.JW_LegOrder = 3;
				transport3.JW_TransportMode = Constants.TransportModes.Sea;
				transport3.JW_TransportType = Constants.TransportPlanningType.Other;
				transport3.JW_RL_NKLoadPort = "USLAX";
				transport3.JW_RL_NKDiscPort = "GUAGA";
				transport3.JW_Vessel = "Miranda";
				transport3.JW_VoyageFlight = "333";
				transport3.JW_ETD = new ZDateTime(2018, 12, 10);
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.DateOfExport, "12-10-2018");

				transport.JW_RL_NKLoadPort = "PRAGU";
				shippingInstruction = new ShippingInstructionBuilder(consol).Build();
				AssertPopulateExportStatementField1(SEDStatementFieldType.Codes.DateOfExport, "12-01-2018");

				void AssertPopulateExportStatementField1(ZString field1Type, ZString expectedField1Code)
				{
					statementSetting.Field1 = field1Type;
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, countrySettingCollection);
					var shippingInstruction2 = new ShippingInstructionBuilder(consol).Build();
					var firstShipmentDO2 = shippingInstruction2.Shipments.FirstOrDefault();
					AssertEquals(field1Type, firstShipmentDO2.ExportStatementField1Type);
					AssertEquals(expectedField1Code, firstShipmentDO2.ExportStatementField1Code);
				}
			}
		}

		#endregion

		#region ICS2

		public void TestValidICS2DeclarantEORI()
		{
			var warningMessage1 = "Please verify ICS2 filing method before transmitting this Shipping Instruction to the carrier.\r\n - 'Carrier Filing' requires this field to be blank and necessary HBL data required by ICS2 will be shared with the carrier.\r\n - ‘Self Filing’ requires user to include the EORI of next filing party in this field and no HBL information will be shared with the carrier, but self-filing a partial ENS with ICS2 will be required.";
			var warningMessage2 = "The EORI number consists of:\r\n - A country code of the issuing Member State (2 letters); followed by\r\n - An identifier that is unique in the Member State (up to 15 alphanumeric characters).";
			var eorIsMissingAgainstSelfFilerOrgWarning = "EORI number is missing in your Self-Filer organization. Please verify in Consol > Addresses > Self-Filer > Config > Registration Numbers/Codes.";

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "NO!2#";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsForwarder = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "NO!2#";
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NO!2#";

			var existingRelationShip = Factory.New<OrgRelatedParty>();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_Parent = org1.PK;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_Location = unloco.RL_Code;
			existingRelationShip.PR_FreightTransportMode = TransportModes.Sea;

			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			Factory.Save();

			var selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);
			AssertNotNull(selfFiler);
			Assert(consol.IsICS2);

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert(wrapper.IsShowICS2);
			AssertEquals(string.Empty, wrapper.ICS2DeclarantEORINumber);
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals(string.Empty, wrapper.ICS2DeclarantEORINumber);
			Assert(wrapper.IsShowICS2);
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);

			wrapper.ICS2DeclarantEORINumber = "NO123456789012345";
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);

			wrapper.ICS2DeclarantEORINumber = "NO1234567890123456";
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);

			wrapper.ICS2DeclarantEORINumber = "1234567890123456";
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + eorIsMissingAgainstSelfFilerOrgWarning);

			var eori = org2.CustomsCodes.AddNew();
			eori.OK_RN_NKCodeCountry = "NO";
			eori.OK_CodeType = "EOR";
			eori.OK_CustomsRegNo = "12345";

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals("NO12345", wrapper.ICS2DeclarantEORINumber);
			Assert(wrapper.IsShowICS2);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);

			wrapper.ICS2DeclarantEORINumber = "NO123456789012345";
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);

			wrapper.ICS2DeclarantEORINumber = "NO1234567890123456";
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);

			wrapper.ICS2DeclarantEORINumber = "1234567890123456";
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);

			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			Factory.Save();

			selfFiler = consol.DocAddresses.FindByDocAddressType(DocAddressType.SelfFiler);
			AssertNull(selfFiler);

			wrapper = new ShippingInstructionBuilder(consol).Build();

			AssertEquals(string.Empty, wrapper.ICS2DeclarantEORINumber);
			Assert(wrapper.IsShowICS2);
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);

			wrapper.ICS2DeclarantEORINumber = "NO123456789012345";
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);

			wrapper.ICS2DeclarantEORINumber = "NO1234567890123456";
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);

			wrapper.ICS2DeclarantEORINumber = "1234567890123456";
			AssertHasWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1 + System.Environment.NewLine + warningMessage2);
			AssertNoWarning(wrapper.ICS2DeclarantEORINumberInfo, warningMessage1);
		}

		public void TestIsShowICS2()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("AUSYD -> SGSIN - SEA", !wrapper.IsShowICS2);

			transport.JW_RL_NKLoadPort = "DE222";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("DE222 -> SGSIN - SEA", !wrapper.IsShowICS2);

			transport.JW_RL_NKDiscPort = "DEBRM";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("DE222 -> DEBRM - SEA", !wrapper.IsShowICS2);

			transport.JW_RL_NKLoadPort = "ESBCN";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("ESBCN -> DEBRM - SEA", !wrapper.IsShowICS2);

			transport.JW_TransportMode = Constants.TransportModes.Air;
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("ESBCN -> DEBRM - Air", !wrapper.IsShowICS2);

			transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("ESBCN -> DEBRM - IWT", !wrapper.IsShowICS2);

			transport.JW_RL_NKDiscPort = "ESALJ";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("ESBCN -> ESALJ - IWT", !wrapper.IsShowICS2);

			transport.JW_RL_NKLoadPort = "AUSYD";
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("AUSYD -> ESALJ - IWT", wrapper.IsShowICS2);

			transport.JW_TransportMode = Constants.TransportModes.Sea;
			wrapper = new ShippingInstructionBuilder(consol).Build();
			Assert("AUSYD -> ESALJ - Sea", wrapper.IsShowICS2);

			var ports = new string[] { "BGVAR", "HRSPU", "CYLMS", "DKCPH", "EETLL", "FIHEL", "FRMRS", "GBBEL", "NOOSL", "SKBTS", "CHBSL" };

			foreach (var loadPort in ports)
			{
				foreach (var dischargePort in ports)
				{
					if (loadPort == dischargePort)
					{
						continue;
					}

					transport.JW_TransportMode = Constants.TransportModes.Sea;
					transport.JW_RL_NKLoadPort = loadPort;
					transport.JW_RL_NKDiscPort = dischargePort;

					wrapper = new ShippingInstructionBuilder(consol).Build();
					Assert($"{loadPort} -> {dischargePort} - Sea", !wrapper.IsShowICS2);

					transport.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
					transport.JW_RL_NKLoadPort = loadPort;
					transport.JW_RL_NKDiscPort = dischargePort;
					wrapper = new ShippingInstructionBuilder(consol).Build();
					Assert($"{loadPort} -> {dischargePort} - IWT", !wrapper.IsShowICS2);
				}
			}

			foreach (var port in ports)
			{
				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = port;
				wrapper = new ShippingInstructionBuilder(consol).Build();
				Assert($"AUSYD -> {port} - SEA", wrapper.IsShowICS2);

				transport.JW_TransportMode = Constants.TransportModes.Sea;
				transport.JW_RL_NKLoadPort = port;
				transport.JW_RL_NKDiscPort = "AUSYD";
				wrapper = new ShippingInstructionBuilder(consol).Build();
				Assert($"{port} -> AUSYD - SEA", !wrapper.IsShowICS2);
			}
		}

		#endregion

		#region TestFilterEoriTaxInfoByCountryAndDirection

		public void TestFilterEoriTaxInfoByCountryAndDirection()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NOMOB";
			consol.JK_RL_NKDischargePort = "FRABC";
			consol.JK_BookingReference = "驴100";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "NOXXX";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKLoadPort.Left(2);
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			var sendingForwarderContact = Factory.New<OrgContact>();
			sendingForwarderContact.OC_OH = sendingForwarder.PK;
			sendingForwarderContact.OC_ContactName = "Sender Name";
			sendingForwarderContact.OC_Email = "name@sender.com";
			sendingForwarderContact.OC_Phone = "1111111";
			sendingForwarderContact.OC_Fax = "2222222";
			consol.JK_OC_SendingForwarderContact = sendingForwarderContact.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "FRXXX";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Pairs";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_OH = receivingForwarder.PK;
			receivingForwarderContact.OC_ContactName = "Receiver Name";
			receivingForwarderContact.OC_Email = "name@receiver.com";
			receivingForwarderContact.OC_Phone = "3333333";
			receivingForwarderContact.OC_Fax = "4444444";
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Stay in touch";
			notifyParty.OH_RL_NKClosestPort = "AUSYD";
			notifyParty.MainAddress.Address1 = "Unit 205";
			notifyParty.MainAddress.Address2 = "128 Why Lane";
			notifyParty.MainAddress.City = "Sydney";
			notifyParty.MainAddress.Postcode = "2000";
			notifyParty.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			notifyParty.MainAddress.OA_Email = "stayintouch@test.com";
			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = "NO";
			cusCode.OK_CustomsRegNo = "1001";
			sendingForwarder.CustomsCodes.Add(cusCode);
			var code = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code.DOC_DocumentType = "ESI";
			code.DOC_Direction = "IMP";
			code.DOC_RN_NKCodeCountry = "NO";
			code.DOC_RN_NKRegulatingCountry = "NO";
			code.DOC_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			var cusCode2 = Factory.New<OrgCusCode>();
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Turn;
			cusCode2.OK_RN_NKCodeCountry = "NO";
			cusCode2.OK_CustomsRegNo = "1002";
			sendingForwarder.CustomsCodes.Add(cusCode2);
			var code2 = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code2.DOC_DocumentType = "ESI";
			code2.DOC_Direction = DocDataConstants.DocOrgCusCodeDirection.Export;
			code2.DOC_RN_NKCodeCountry = "NO";
			code2.DOC_RN_NKRegulatingCountry = "NO";
			code2.DOC_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Turn;
			Factory.Save();
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(2, carrierMessageData.ShipperTaxInfo.Count);
			AssertEquals("1002", carrierMessageData.ShipperTaxInfo1.Number);
			AssertEquals("NO1001", carrierMessageData.ShipperTaxInfo2.Number);
			AssertEquals(0, carrierMessageData.ConsigneeTaxInfo.Count);
			AssertEquals(0, carrierMessageData.NotifyPartyTaxInfo.Count);

			code.DOC_Direction = DocDataConstants.DocOrgCusCodeDirection.Export;
			Factory.Save();
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(2, carrierMessageData.ShipperTaxInfo.Count);
			AssertEquals("NO1001", carrierMessageData.ShipperTaxInfo1.Number);
			AssertEquals("1002", carrierMessageData.ShipperTaxInfo2.Number);
			AssertEquals(0, carrierMessageData.ConsigneeTaxInfo.Count);
			AssertEquals(0, carrierMessageData.NotifyPartyTaxInfo.Count);

			cusCode.OK_RN_NKCodeCountry = "FR";
			code.DOC_RN_NKCodeCountry = "FR";
			code.DOC_RN_NKRegulatingCountry = "FR";
			code.DOC_Direction = DocDataConstants.DocOrgCusCodeDirection.Import;
			sendingForwarder.CustomsCodes.Remove(cusCode);
			receivingForwarder.CustomsCodes.Add(cusCode);
			Factory.Save();
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(1, carrierMessageData.ShipperTaxInfo.Count);
			AssertEquals("1002", carrierMessageData.ShipperTaxInfo1.Number);
			AssertEquals(1, carrierMessageData.ConsigneeTaxInfo.Count);
			AssertEquals("FR1001", carrierMessageData.ConsigneeTaxInfo1.Number);
			AssertEquals(1, carrierMessageData.NotifyPartyTaxInfo.Count);
			AssertEquals(string.Empty, carrierMessageData.NotifyPartyTaxInfo1.Number);

			receivingForwarder.CustomsCodes.Remove(cusCode);
			notifyParty.CustomsCodes.Add(cusCode);
			Factory.Save();
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(1, carrierMessageData.ShipperTaxInfo.Count);
			AssertEquals("1002", carrierMessageData.ShipperTaxInfo1.Number);
			AssertEquals(1, carrierMessageData.ConsigneeTaxInfo.Count);
			AssertEquals(string.Empty, carrierMessageData.ConsigneeTaxInfo1.Number);
			AssertEquals(1, carrierMessageData.NotifyPartyTaxInfo.Count);
			AssertEquals("FR1001", carrierMessageData.NotifyPartyTaxInfo1.Number);

			notifyParty.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Switzerland;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			code.DOC_RN_NKCodeCountry = Constants.CountryCodes.Switzerland;
			code.DOC_RN_NKRegulatingCountry = Constants.CountryCodes.Switzerland;
			Factory.Save();
			carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(1, carrierMessageData.NotifyPartyTaxInfo.Count);
			AssertEquals("CH1001", carrierMessageData.NotifyPartyTaxInfo1.Number);
		}

		#endregion

		#region TestCUSCodesValidation

		public void TestCUSCodesValidation()
		{
			var cusCode = Factory.NewWithValidTestData<Customs.Universal.ZZRefCusCodeListCombined>();

			cusCode.ZZD_Code = "useful";
			cusCode.ZZD_CodeType = DocDataConstants.RefCusCodeListType.Code_ECICS;
			cusCode.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			Factory.Save();

			var errorMessage = "Invalid code has been detected. Please use the searching tile to lookup the correct CUS code.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "DE222";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");

				var packingLine = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));

				AssertNoMessageError(packingLine.CUSCode1Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode2Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode3Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode4Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode5Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode6Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode7Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode8Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode9Info, errorMessage);

				packingLine.CUSCode1 = "useless";

				AssertHasMessageError(packingLine.CUSCode1Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode2Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode3Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode4Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode5Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode6Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode7Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode8Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode9Info, errorMessage);

				packingLine.CUSCode2 = "useless";
				packingLine.CUSCode3 = "useless";
				packingLine.CUSCode4 = "useless";
				packingLine.CUSCode5 = "useless";
				packingLine.CUSCode6 = "useless";
				packingLine.CUSCode7 = "useless";
				packingLine.CUSCode8 = "useless";
				packingLine.CUSCode9 = "useless";

				AssertHasMessageError(packingLine.CUSCode1Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode2Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode3Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode4Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode5Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode6Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode7Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode8Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode9Info, errorMessage);

				packingLine.CUSCode1 = "useful";

				AssertNoMessageError(packingLine.CUSCode1Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode2Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode3Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode4Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode5Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode6Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode7Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode8Info, errorMessage);
				AssertHasMessageError(packingLine.CUSCode9Info, errorMessage);

				packingLine.CUSCode2 = "useful";
				packingLine.CUSCode3 = "useful";
				packingLine.CUSCode4 = "useful";
				packingLine.CUSCode5 = "useful";
				packingLine.CUSCode6 = "useful";
				packingLine.CUSCode7 = "useful";
				packingLine.CUSCode8 = "useful";
				packingLine.CUSCode9 = "useful";

				AssertNoMessageError(packingLine.CUSCode1Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode2Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode3Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode4Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode5Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode6Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode7Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode8Info, errorMessage);
				AssertNoMessageError(packingLine.CUSCode9Info, errorMessage);
			}
		}

		public void TestCUSCodesValidation_PackageGrouping()
		{
			var cusCode = Factory.NewWithValidTestData<Customs.Universal.ZZRefCusCodeListCombined>();

			cusCode.ZZD_Code = "useful";
			cusCode.ZZD_CodeType = DocDataConstants.RefCusCodeListType.Code_ECICS;
			cusCode.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			Factory.Save();

			var errorMessage = "Invalid code has been detected. Please use the searching tile to lookup the correct CUS code.";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "DE222";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_UnitOfVolume = Constants.Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Bag;
			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Constants.Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Constants.Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;
			var s1OuterPackLine3 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 30, Constants.PkgUnit.Pail, 30, Constants.Weight.Grams, 30, Constants.Volume.MegaLitre, "Group1 MarksAndNumbers", "Group1 DetailedDescription");
			s1OuterPackLine3.JL_JC = container2.PK;

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");

				var shipment1GroupedPackingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));

				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode1Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode2Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode3Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode4Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode5Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode6Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode7Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode8Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode9Info, errorMessage);

				shipment1GroupedPackingLine1.CUSCode1 = "useless";

				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode1Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode2Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode3Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode4Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode5Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode6Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode7Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode8Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode9Info, errorMessage);

				shipment1GroupedPackingLine1.CUSCode2 = "useless";
				shipment1GroupedPackingLine1.CUSCode3 = "useless";
				shipment1GroupedPackingLine1.CUSCode4 = "useless";
				shipment1GroupedPackingLine1.CUSCode5 = "useless";
				shipment1GroupedPackingLine1.CUSCode6 = "useless";
				shipment1GroupedPackingLine1.CUSCode7 = "useless";
				shipment1GroupedPackingLine1.CUSCode8 = "useless";
				shipment1GroupedPackingLine1.CUSCode9 = "useless";

				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode1Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode2Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode3Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode4Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode5Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode6Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode7Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode8Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode9Info, errorMessage);

				shipment1GroupedPackingLine1.CUSCode1 = "useful";

				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode1Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode2Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode3Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode4Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode5Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode6Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode7Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode8Info, errorMessage);
				AssertHasMessageError(shipment1GroupedPackingLine1.CUSCode9Info, errorMessage);

				shipment1GroupedPackingLine1.CUSCode2 = "useful";
				shipment1GroupedPackingLine1.CUSCode3 = "useful";
				shipment1GroupedPackingLine1.CUSCode4 = "useful";
				shipment1GroupedPackingLine1.CUSCode5 = "useful";
				shipment1GroupedPackingLine1.CUSCode6 = "useful";
				shipment1GroupedPackingLine1.CUSCode7 = "useful";
				shipment1GroupedPackingLine1.CUSCode8 = "useful";
				shipment1GroupedPackingLine1.CUSCode9 = "useful";

				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode1Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode2Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode3Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode4Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode5Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode6Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode7Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode8Info, errorMessage);
				AssertNoMessageError(shipment1GroupedPackingLine1.CUSCode9Info, errorMessage);
			}
		}

		public void TestSyncOtherPackingLineHBLPaymentTypeInShipmentWhenChanges()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_FullName = "Fudge burners";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.MainAddress.Address1 = "line 1";
				org.MainAddress.Address2 = "line 2";
				org.MainAddress.City = "Sydney";
				org.MainAddress.Postcode = "2000";

				var contact = Factory.New<OrgContact>();
				contact.OC_OH = org.PK;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_ConsolMode = Constants.ContainerModes.LCL;
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S001";
				shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "S002";
				shipment2.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_PackageCount = 1;
				packLine.JL_F3_NKPackType = "PLT";

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 2;
				packLine2.JL_F3_NKPackType = "PLT";

				var packLine3 = shipment2.OuterPackLines.AddNew();
				packLine3.JL_PackageCount = 3;
				packLine3.JL_F3_NKPackType = "PLT";

				var job = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
				job.JH_OC_LocalBillingContact = contact.PK;

				var orgHeader = (OrgHeader)job.LocalZAddressWithContact.OrgHeader;
				orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "CBC";

				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				AssertEquals(2, carrierMessageData.Shipments.First(s => s.ShipmentID == "S001").PackingLines.Where(packingLine => packingLine.HBLPaymentType == "A").Count());
				AssertEquals(1, carrierMessageData.Shipments.First(s => s.ShipmentID == "S002").PackingLines.Where(packingLine => packingLine.HBLPaymentType == "D").Count());

				carrierMessageData.Shipments.First(s => s.ShipmentID == "S001").PackingLines.FirstOrDefault().HBLPaymentType = "B";
				AssertEquals(2, carrierMessageData.Shipments.First(s => s.ShipmentID == "S001").PackingLines.Where(packingLine => packingLine.HBLPaymentType == "B").Count());
				AssertEquals(1, carrierMessageData.Shipments.First(s => s.ShipmentID == "S002").PackingLines.Where(packingLine => packingLine.HBLPaymentType == "D").Count());
			}
		}

		#endregion

		#region TestGetEoriTaxNumberWithoutCountryValidation

		public void TestGetEoriTaxNumberWithoutCountryValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001001";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NOMOB";
			consol.JK_RL_NKDischargePort = "FRABC";
			consol.JK_BookingReference = "驴100";
			consol.JK_NoOriginalBills = 1;
			consol.JK_NoCopyBills = 3;
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 1);

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "FRXXX";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Pairs";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = consol.JK_RL_NKDischargePort.Left(2);
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var receivingForwarderContact = Factory.New<OrgContact>();
			receivingForwarderContact.OC_OH = receivingForwarder.PK;
			receivingForwarderContact.OC_ContactName = "Receiver Name";
			receivingForwarderContact.OC_Email = "name@receiver.com";
			receivingForwarderContact.OC_Phone = "3333333";
			receivingForwarderContact.OC_Fax = "4444444";
			consol.JK_OC_ReceivingForwarderContact = receivingForwarderContact.PK;

			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_RN_NKCodeCountry = "DE";
			cusCode.OK_CustomsRegNo = "1001";
			receivingForwarder.CustomsCodes.Add(cusCode);

			var code = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			code.DOC_DocumentType = "ESI";
			code.DOC_Direction = DocDataConstants.DocOrgCusCodeDirection.Both;
			code.DOC_RN_NKCodeCountry = "FR";
			code.DOC_RN_NKRegulatingCountry = "FR";
			code.DOC_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			Factory.Save();
			var carrierMessageData = CreateDocDataObjectBuilder(consol).Build();

			AssertEquals(1, carrierMessageData.ConsigneeTaxInfo.Count);
			AssertEquals("DE1001", carrierMessageData.ConsigneeTaxInfo1.Number);
			AssertEquals("DE", carrierMessageData.ConsigneeTaxInfo1.RegulatingCountry.Code);
		}

		#endregion

		#region Implementation

		protected override CarrierMessageDataBuilder CreateDocDataObjectBuilder(ForwardingConsol consol)
		{
			return new ShippingInstructionBuilder(consol);
		}

		protected override string GetCarrierMessageDataBuilderName()
		{
			return DataContext.ShippingInstruction;
		}

		OrgHeader CreateAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Consignor";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = "Unit52";
			orgHeader.MainAddress.Address2 = "Dorcus yamadai";
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2017";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

			return orgHeader;
		}

		RefDocOrgCusCode CreateRefDocOrgCusCode(string countryCode, string regulatingCountryCode, string codeType, byte priority, string notes = "notes", string shortLabel = "label", string documentType = "ESI")
		{
			var refDocOrgCusCodeBo = Factory.New<RefDocOrgCusCode>();

			refDocOrgCusCodeBo.DOC_RN_NKCodeCountry = countryCode;
			refDocOrgCusCodeBo.DOC_RN_NKRegulatingCountry = regulatingCountryCode;
			refDocOrgCusCodeBo.DOC_CodeType = codeType;
			refDocOrgCusCodeBo.DOC_Priority = priority;
			refDocOrgCusCodeBo.DOC_Notes = notes;
			refDocOrgCusCodeBo.DOC_ShortLabel = shortLabel;
			refDocOrgCusCodeBo.DOC_DocumentType = documentType;

			return refDocOrgCusCodeBo;
		}

		#endregion

		#region TestCanadaExport
		public void TestIsCanadaExport()
		{
			var brCode1 = "CATOR";
			var brCode2 = "CAVAN";

			var nonBRCode1 = "SGSIN";
			var nonBRCode2 = "AUSYD";

			var consol = CreateConsol();
			var firstTransport = (Freight.Business.Transport)consol.Transports.First();
			var lastTransport = (Freight.Business.Transport)consol.Transports.Last();

			var shippingInstructionBuilder = new ShippingInstructionBuilder(consol);

			firstTransport.JW_RL_NKLoadPort = brCode1;
			lastTransport.JW_RL_NKDiscPort = nonBRCode1;
			var carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsCanadaExport should be true for exports from CA.", carrierMessageData.IsCanadaExport, true);

			lastTransport.JW_RL_NKDiscPort = brCode2;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsCanadaExport should be false for domestic consols.", carrierMessageData.IsCanadaExport, false);

			firstTransport.JW_RL_NKLoadPort = nonBRCode1;
			lastTransport.JW_RL_NKDiscPort = nonBRCode2;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsCanadaExport should be false when load port is not CA.", carrierMessageData.IsCanadaExport, false);

			lastTransport.JW_RL_NKDiscPort = brCode1;
			carrierMessageData = shippingInstructionBuilder.Build();
			AssertEquals("IsCanadaExport should be false for CA imports.", carrierMessageData.IsCanadaExport, false);
		}

		public void TestValidateCTNNumber()
		{
			var warningMessage = "Please enter valid CERS Proof of Report number(s). The number must start with Exporter’s Authorization ID (e.g., \"AA1234\"), \r\nfollowed by the Submission Date in the format YYYYMMDD, and Sequential Number from 1 to 99999999999.";
			var errorMessage = "Number with over 35 characters will not be accepted by the carrier.";

			var consol = CreateConsol();
			var shipment = consol.Shipments[0];
			shipment.JS_UniqueConsignRef = "S000001";
			consol.Transports[0].JW_RL_NKLoadPort = "CATOR";

			var ctnNumber = shipment.CusEntryNumbers.AddNew();
			ctnNumber.CE_EntryType = "CTN";
			ctnNumber.CE_EntryNum = "AA1234202401011";

			var shippingInstruction = new ShippingInstructionBuilder(consol).Build();
			var shipmentDO = shippingInstruction.Shipments.First();

			AssertNoMessageError(shipmentDO.CTNNumberInfo, errorMessage);
			AssertNoWarning(shipmentDO.CTNNumberInfo, warningMessage);

			shipmentDO.CTNNumber = "1112342024010100000000001";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoMessageError(shipmentDO.CTNNumberInfo, errorMessage);
			AssertHasWarning(shipmentDO.CTNNumberInfo, warningMessage);

			shipmentDO.CTNNumber = "AB12342024010000000000001";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoMessageError(shipmentDO.CTNNumberInfo, errorMessage);
			AssertHasWarning(shipmentDO.CTNNumberInfo, warningMessage);

			shipmentDO.CTNNumber = "CD1234202401010";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoMessageError(shipmentDO.CTNNumberInfo, errorMessage);
			AssertHasWarning(shipmentDO.CTNNumberInfo, warningMessage);

			shipmentDO.CTNNumber = "AA123420240101000000000020000000000001";
			shippingInstruction.ValidateAllIncludingChildren();
			AssertNoWarning(shipmentDO.CTNNumberInfo, warningMessage);
			AssertHasMessageError(shipmentDO.CTNNumberInfo, errorMessage);
		}

		public void TestValidateGroupCTNNumber()
		{
			var warningMessage = "Please enter valid CERS Proof of Report number(s). The number must start with Exporter’s Authorization ID (e.g., \"AA1234\"), \r\nfollowed by the Submission Date in the format YYYYMMDD, and Sequential Number from 1 to 99999999999.";
			var errorMessage = "Number with over 35 characters will not be accepted by the carrier.";

			CreateRefCountryRules(CountryCodes.Canada, CountryCodes.China, ZString.Empty, ZGuid.Empty, true);
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_PackageGrouping = PackageGrouping.Codes.GroupByPackLine;
			consol.JK_RL_NKLoadPort = "CATOR";
			consol.JK_RL_NKDischargePort = "CNSHG";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPMENT1";
			shipment1.JS_UnitOfWeight = Weight.Grams;
			shipment1.JS_UnitOfVolume = Volume.MegaLitre;
			shipment1.DetailedGoodsDescriptionNoteText = "SHIPMENT1 DetailedGoodsDescriptionNoteText";
			shipment1.JS_F3_NKPackType = PkgUnit.Box;
			shipment1.JS_F3_NKTotalCountPackType = PkgUnit.Bag;
			shipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1.Numbers.RemoveAndDeleteAll();

			var s1OuterPackLine1 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 10, Constants.PkgUnit.Pail, 10, Constants.Weight.Kilograms, 10, Constants.Volume.CubicMetres, "Group1 MarksAndNumbers", "Group1 DetailedDescription", true, -2, 3, Temperature.Fahrenheit);
			s1OuterPackLine1.JL_JC = container1.PK;
			var s1OuterPackLine2 = PopulatePackLine(shipment1.OuterPackLines.AddNew(), 20, Constants.PkgUnit.Keg, 20, Constants.Weight.Grams, 20, Constants.Volume.MegaLitre, "Group2 MarksAndNumbers", "Group2 DetailedDescription", true, 2, 5, Temperature.Fahrenheit);
			s1OuterPackLine2.JL_JC = container1.PK;

			var shipment1Number = shipment1.CusEntryNumbers.AddNew();
			shipment1Number.CE_EntryType = "CTN";
			shipment1Number.CE_EntryNum = "AA1234202401011";

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var builder = CreateDocDataObjectBuilder(consol);
				var carrierMessageData = builder.Build();

				var shipmentDO1 = carrierMessageData.Shipments.Cast<Shipment>().First(x => x.ShipmentID == "SHIPMENT1");

				var packingLine1 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group1"));
				var packingLine2 = shipmentDO1.PackingLines.First(x => x.GoodsDescription.StartsWith("Group2"));

				AssertNoWarning(packingLine1.GroupCTNNumberInfo, warningMessage);
				AssertNoWarning(packingLine2.GroupCTNNumberInfo, warningMessage);

				AssertNoMessageError(packingLine1.GroupCTNNumberInfo, errorMessage);
				AssertNoMessageError(packingLine2.GroupCTNNumberInfo, errorMessage);

				packingLine1.GroupCTNNumber = "0012342024010100000000001";
				AssertNoMessageError(packingLine1.GroupCTNNumberInfo, errorMessage);
				AssertHasWarning(packingLine1.GroupCTNNumberInfo, warningMessage);

				packingLine1.GroupCTNNumber = "AA12342024010000000000001";
				AssertNoMessageError(packingLine1.GroupCTNNumberInfo, errorMessage);
				AssertHasWarning(packingLine1.GroupCTNNumberInfo, warningMessage);

				packingLine1.GroupCTNNumber = "AA1234202401010";
				AssertNoMessageError(packingLine1.GroupCTNNumberInfo, errorMessage);
				AssertHasWarning(packingLine1.GroupCTNNumberInfo, warningMessage);

				packingLine1.GroupCTNNumber = "AA123420240101000000000020000000000001";
				AssertNoWarning(packingLine1.GroupCTNNumberInfo, warningMessage);
				AssertHasMessageError(packingLine1.GroupCTNNumberInfo, errorMessage);
			}
		}
		#endregion
	}
}
