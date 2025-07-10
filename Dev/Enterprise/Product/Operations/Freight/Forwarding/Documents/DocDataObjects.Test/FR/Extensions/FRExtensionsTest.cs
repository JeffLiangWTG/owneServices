using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	public class FRExtensionsTest : TestCaseWithFactory
	{
		public void TestSONNumber()
		{
			var context = new CommonContext(Factory.NewWithValidTestData<ForwardingShipment>().Factory.GetCachedReadOnlyFactory());

			var address = CreateFrenchAddress();
			address.Header.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "HeaderTest", CountryCodes.France);

			AssertEquals("HeaderTest", address.GetSONNumber(context).Value);

			address.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.SON, "AddressTest", "FR");

			AssertEquals("AddressTest", address.GetSONNumber(context).Value);
		}

		public void TestCi5Number()
		{
			var context = new CommonContext(Factory.NewWithValidTestData<ForwardingShipment>().Factory.GetCachedReadOnlyFactory());

			var address = CreateFrenchAddress();
			address.Header.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "HeaderTest", CountryCodes.France);

			AssertEquals("HeaderTest", address.GetCi5Number(context).Value);

			address.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.CI5, "AddressTest", CountryCodes.France);

			AssertEquals("AddressTest", address.GetCi5Number(context).Value);
		}

		public void TestSiretNumber()
		{
			var context = new CommonContext(Factory.NewWithValidTestData<ForwardingShipment>().Factory.GetCachedReadOnlyFactory());

			var address = CreateFrenchAddress();
			address.Header.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "HeaderTest", "FR");

			AssertEquals("HeaderTest", address.GetSiretNumber(context).Value);

			address.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "AddressTest", "FR");

			AssertEquals("AddressTest", address.GetSiretNumber(context).Value);
		}

		public void TestGetPCS()
		{
			RefUNLOCO portFR = Factory.NewWithValidTestData<RefUNLOCO>();
			portFR.RL_Code = "FRPAR";
			RefLocoMap mapFR = portFR.RefLocoMaps.AddNew();
			mapFR.RY_RN = CountryGuids.France;
			mapFR.RY_SystemUsage = LocoMapSystemUsageList.Codes.PCS;
			mapFR.RY_LocalPortCode = FrenchPortsConstants.PCS.MGI;
			AssertEquals(FrenchPortsConstants.PCS.MGI, FRExtensions.GetPCS(portFR));

			RefUNLOCO portBE = Factory.NewWithValidTestData<RefUNLOCO>();
			portBE.RL_Code = "BEANR";
			RefLocoMap mapBE = portBE.RefLocoMaps.AddNew();
			mapBE.RY_RN = CountryGuids.Belgium;
			mapBE.RY_SystemUsage = LocoMapSystemUsageList.Codes.PCS;
			mapBE.RY_LocalPortCode = FrenchPortsConstants.PCS.MGI;
			AssertEquals(string.Empty, FRExtensions.GetPCS(portBE));
		}

		public void TestGetShipmentNumbersWithSpecifiedRCNSent()
		{
			var shipment = CreateShipment("SH0001001");
			var consol = CreateConsol("C00001001", isImport: true);
			shipment.Consols.Add(consol);

			var i = 0;
			foreach (ForwardingPackLine packline in shipment.OuterPackLines)
			{
				i++;
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"ERC_{i}";
			}

			var shipmentThatAlreadySendRefs = CreateShipment("SH0001002");
			var consolThatAlreadySendRefs = CreateConsol("C00001002", isImport: true);
			shipmentThatAlreadySendRefs.Consols.Add(consolThatAlreadySendRefs);

			i = 0;
			foreach (ForwardingPackLine packline in shipmentThatAlreadySendRefs.OuterPackLines)
			{
				i++;
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"ERC_{i}";
			}

			List<ZString> rcns = new List<ZString>();
			rcns.Add("ERC_1");

			AssertEquals("0 shipments found", 0, FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns).Count);
			shipmentThatAlreadySendRefs.OuterPackLines[0].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs.Factory.Save();
			AssertEquals("1 shipments found", 1, FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns).Count);
			AssertEquals("ERS_1 found", "ERC_1", string.Join(", ", FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns)["SH0001002"]));

			var shipmentThatAlreadySendRefs2 = CreateShipment("SH0001003");
			var consolThatAlreadySendRefs2 = CreateConsol("C00001003", isImport: true);
			shipmentThatAlreadySendRefs2.Consols.Add(consolThatAlreadySendRefs2);

			i = 0;
			foreach (ForwardingPackLine packline in shipmentThatAlreadySendRefs2.OuterPackLines)
			{
				i++;
				var ercNumber = packline.AdditionalReferenceNumbers.AddNew();
				ercNumber.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
				ercNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				ercNumber.CE_EntryNum = $"ERC_{i}";
			}
			shipmentThatAlreadySendRefs2.OuterPackLines[1].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs2.OuterPackLines[2].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs2.Factory.Save();

			AssertEquals("1 shipments found", 1, FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns).Count);
			AssertEquals("ERS_1 found", "ERC_1", string.Join(", ", FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns)["SH0001002"]));
			rcns.Add("ERC_2");
			rcns.Add("ERC_3");
			AssertEquals("2 shipments found", 2, FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns).Count);
			AssertEquals("ERS_2 found", "ERC_1", string.Join(", ", FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns)["SH0001002"]));
			AssertEquals("ERS_3 found", "ERC_2, ERC_3", string.Join(", ", FRExtensions.GetShipmentNumbersWithSpecifiedRCNSent(shipment, rcns)["SH0001003"]));
		}

		#region Implementation

		OrgAddress CreateFrenchAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "FRCAL";

			var address = org.MainAddress;
			address.OA_RN_NKCountryCode = "FR";

			return address;
		}

		ForwardingConsol CreateConsol(string consolNumber = "C00001000", bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			if (isImport)
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "FRPRA";
			}
			else
			{
				consol.JK_RL_NKLoadPort = "FRPRA";
				consol.JK_RL_NKDischargePort = "AUSYD";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL";

			if (isAddressAvailableToCreate)
			{
				CreateAddresses(consol);
			}

			return consol;
		}

		ForwardingShipment CreateShipment(string shipmentNumber = "SH0001000")
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_UnitOfVolume = "D3";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1000;
			packline1.JL_Width = 1000;
			packline1.JL_Height = 300;
			packline1.JL_UnitOfDimension = "CM";
			packline1.JL_HarmonisedCode = "WHISKY";
			packline1.JL_RefNumber = "AMR-57";
			packline1.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";
			packline1.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packline1.JL_LastKnownTransitWarehouseStatus = "RCV";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 15;
			packline2.JL_Width = 10;
			packline2.JL_Height = 3;
			packline2.JL_UnitOfDimension = "M";
			packline2.JL_HarmonisedCode = "WHISKY";
			packline2.JL_RefNumber = "AMR-43";
			packline2.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline2.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline2.JL_LastKnownTransitWarehouseStatus = "RCV";

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 6;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 600;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 450;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Length = 15;
			packline3.JL_Width = 10;
			packline3.JL_Height = 3;
			packline3.JL_UnitOfDimension = "M";
			packline3.JL_HarmonisedCode = "WHISKY";
			packline3.JL_RefNumber = "AMR-43";
			packline3.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline3.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline3.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline3.JL_LastKnownTransitWarehouseStatus = "RCV";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			Factory.Save();

			return shipment;
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "YUMMY";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 200";
			receivingForwarder.MainAddress.Address2 = "55 Why Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}

		#endregion
	}
}
