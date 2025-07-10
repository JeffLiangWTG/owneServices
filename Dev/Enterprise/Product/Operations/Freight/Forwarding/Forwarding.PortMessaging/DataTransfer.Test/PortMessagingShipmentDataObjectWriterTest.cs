using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer.Testing
{
	sealed class PortMessagingShipmentDataObjectWriterTest : ShipmentDataObjectWriterTest
	{
		public void TestExportEORICode_SendingForwarder()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", Constants.CountryCodes.Germany);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.FillWithValidTestData();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var writer = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, true, PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var shipmentData = writer.GetDataObject(shipment);

			AssertNotNull("Expected to have a shipment data object created", shipmentData);

			var organizationAddress = shipmentData.OrganizationAddressCollection.Find(address => address.AddressType.GetValueOrDefault() == AddressTypes.SendingForwarderAddress);
			AssertNotNull("Expected to export the linked consol's SendingForwarder", organizationAddress);

			AssertEquals("Expected the Eori number to be included with the linked consol", 1, organizationAddress.RegistrationNumberCollection.Count);

			var registrationNumber = organizationAddress.RegistrationNumberCollection[0];
			AssertEquals("Expected the Eori number to be included", "EOR", registrationNumber.Type.Code.GetValueOrDefault());
			AssertEquals("Expected the Eori number to be included", "1234", registrationNumber.Value.GetValueOrDefault());
			AssertEquals("Expected the Eori number to be included", "DE", registrationNumber.CountryOfIssue.Code.GetValueOrDefault());
		}

		public void TestPortMessaging()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			shipmentPortMessaging.JSM_MovementReferenceNumber = "MRN1";
			shipmentPortMessaging.JSM_MovementReferenceNumberComplete = true;
			shipmentPortMessaging.JSM_LocalReferenceNumber = "LRN1";
			shipmentPortMessaging.JSM_LocalReferenceNumberComplete = true;
			shipmentPortMessaging.JSM_ExemptionReason = ExemptionReasonList.Codes.ExemptionReason2;
			shipmentPortMessaging.JSM_ATBNumber = "ATB123";
			shipmentPortMessaging.JSM_Annex30AType = Annex30ATypeList.Codes.AlreadyCompleted;
			shipmentPortMessaging.JSM_Annex30AFailureProcess = true;
			shipmentPortMessaging.JSM_ExportDeclarationReference = "RAHHHHHH";
			shipmentPortMessaging.JSM_ForwardingCustomsOfficeCode = "83031478";
			shipmentPortMessaging.JSM_CustomsReleaseDate = ZDateTime.Today;

			var packLine = shipment.OuterPackLines.AddNew();
			var packLinePortMessaging = PackLinePortMessaging.LoadOrCreate(packLine);
			packLinePortMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			packLinePortMessaging.JLM_MovementReferenceNumber = "MRN2";
			packLinePortMessaging.JLM_MovementReferenceNumberComplete = false;
			packLinePortMessaging.JLM_LocalReferenceNumber = "LRN2";
			packLinePortMessaging.JLM_LocalReferenceNumberComplete = false;
			packLinePortMessaging.JLM_ExemptionReason = ExemptionReasonList.Codes.ExemptionReason3;
			packLinePortMessaging.JLM_ATBNumber = "ATB456";
			packLinePortMessaging.JLM_Annex30AType = Annex30ATypeList.Codes.SeparateEntry;
			packLinePortMessaging.JLM_Annex30AFailureProcess = false;
			packLinePortMessaging.JLM_ExportDeclarationReference = "GRRRRRRR";
			packLinePortMessaging.JLM_CustomsReleaseDate = ZDateTime.BrettsBirthday;

			var dataObjectWriter = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, false, PortMessagingManager.MessageType.PortOrderOutbound, "AAA");
			var shipmentData = dataObjectWriter.GetDataObject(shipment);
			AssertEquals(null, shipmentData.PortMessaging);
			AssertEquals(null, shipmentData.PackingLineCollection[0].PortMessaging);

			dataObjectWriter = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, false, PortMessagingManager.MessageType.PortOrderWithHDS, "AAA");
			shipmentData = dataObjectWriter.GetDataObject(shipment);
			var shipmentPortMessagingDO = shipmentData.PortMessaging;
			var packLinePortMessagingDO = shipmentData.PackingLineCollection[0].PortMessaging;

			CombineAssertions(delegate
			{
				AssertEquals(EntryTypeList.Codes.Message, shipmentPortMessagingDO.TypeOfDeclaration.Code);
				AssertEquals(EntryTypeList.Descriptions.Message, shipmentPortMessagingDO.TypeOfDeclaration.Description);
				AssertEquals("MRN1", shipmentPortMessagingDO.MRN);
				AssertEquals("Y", shipmentPortMessagingDO.MRNComplete);
				AssertEquals("LRN1", shipmentPortMessagingDO.LRN);
				AssertEquals("Y", shipmentPortMessagingDO.LRNComplete);
				AssertEquals(ExemptionReasonList.Codes.ExemptionReason2, shipmentPortMessagingDO.ExemptionReason.Code);
				AssertEquals(ExemptionReasonList.Descriptions.ExemptionReason2, shipmentPortMessagingDO.ExemptionReason.Description);
				AssertEquals("ATB123", shipmentPortMessagingDO.ATB);
				AssertEquals(Annex30ATypeList.Codes.AlreadyCompleted, shipmentPortMessagingDO.Annex30AType.Code);
				AssertEquals(Annex30ATypeList.Descriptions.AlreadyCompleted, shipmentPortMessagingDO.Annex30AType.Description);
				AssertEquals(true, shipmentPortMessagingDO.Annex30AFailureProcess);
				AssertEquals("RAHHHHHH", shipmentPortMessagingDO.ExportDeclarationNumber);
				AssertEquals(null, shipmentPortMessagingDO.ForwardingCustomsOfficeCode);
				AssertEquals(ZDateTime.Today, shipmentPortMessagingDO.CustomsReleaseDate);
			});

			CombineAssertions(delegate
			{
				AssertEquals(EntryTypeList.Codes.Message, packLinePortMessagingDO.TypeOfDeclaration.Code);
				AssertEquals(EntryTypeList.Descriptions.Message, packLinePortMessagingDO.TypeOfDeclaration.Description);
				AssertEquals("MRN2", packLinePortMessagingDO.MRN);
				AssertEquals("N", packLinePortMessagingDO.MRNComplete);
				AssertEquals("LRN2", packLinePortMessagingDO.LRN);
				AssertEquals("N", packLinePortMessagingDO.LRNComplete);
				AssertEquals(ExemptionReasonList.Codes.ExemptionReason3, packLinePortMessagingDO.ExemptionReason.Code);
				AssertEquals(ExemptionReasonList.Descriptions.ExemptionReason3, packLinePortMessagingDO.ExemptionReason.Description);
				AssertEquals("ATB456", packLinePortMessagingDO.ATB);
				AssertEquals(Annex30ATypeList.Codes.SeparateEntry, packLinePortMessagingDO.Annex30AType.Code);
				AssertEquals(Annex30ATypeList.Descriptions.SeparateEntry, packLinePortMessagingDO.Annex30AType.Description);
				AssertEquals(false, packLinePortMessagingDO.Annex30AFailureProcess);
				AssertEquals("GRRRRRRR", packLinePortMessagingDO.ExportDeclarationNumber);
				AssertEquals(ZDateTime.BrettsBirthday, packLinePortMessagingDO.CustomsReleaseDate);
			});

			dataObjectWriter = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, false, PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation, "AAA");
			shipmentData = dataObjectWriter.GetDataObject(shipment);
			shipmentPortMessagingDO = shipmentData.PortMessaging;

			AssertEquals("83031478", shipmentPortMessagingDO.ForwardingCustomsOfficeCode);
		}

		public void TestPortMessaging_Dakosy_GetCurrentConsol()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Constants.TransportModes.Road;
			consol1.JK_RL_NKLoadPort = "DELUM";
			consol1.JK_RL_NKDischargePort = "DEHAM";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = "DEHAM";
			consol2.JK_RL_NKDischargePort = "EGALY";

			var writer = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, true, PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var shipmentData = writer.GetDataObject(shipment);

			AssertEquals("DEHAM", shipmentData.PortOfLoading.Code);
			AssertEquals("EGALY", shipmentData.PortOfDischarge.Code);
		}

		public void TestPortMessaging_Dakosy_ReplacesGBWhenOriginOrDestinationInNorthernIreland()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "GBLAR";
			shipment.JS_RL_NKDestination = "GBBEL";

			shipment.Origin.CountryStates.RW_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			shipment.Origin.CountryStates.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;

			shipment.Destination.CountryStates.RW_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			shipment.Destination.CountryStates.RW_RegionName = RefUNLOCO.Regions.NorthernIreland;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "GBLAR";
			consol.JK_RL_NKDischargePort = "GBBEL";

			var writer = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, true, PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var shipmentData = writer.GetDataObject(shipment);

			AssertEquals("XILAR", shipmentData.PortOfOrigin.Code);
			AssertEquals("XIBEL", shipmentData.PortOfDestination.Code);
		}

		public void TestPortMessagingNoteCollection_PortMessageRemarks_WithShipmentRemarks()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PortMessageRemarks.Description, "Shipment Remarks");

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "EGALY";

			var writer = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, true, PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var shipmentData = writer.GetDataObject(shipment);

			AssertEquals("Shipment Remarks", shipmentData.SubShipmentCollection.FirstOrDefault().PortMessaging.Remarks);
		}

		public void TestPortMessagingNoteCollection_PortMessageRemarks_WithoutShipmentRemarks()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";
			consol.JK_RL_NKDischargePort = "EGALY";

			var writer = new PortMessagingShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, shipment)), false, true, PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var shipmentData = writer.GetDataObject(shipment);

			AssertNull(shipmentData.SubShipmentCollection.FirstOrDefault().PortMessaging.Remarks);
		}
	}
}
