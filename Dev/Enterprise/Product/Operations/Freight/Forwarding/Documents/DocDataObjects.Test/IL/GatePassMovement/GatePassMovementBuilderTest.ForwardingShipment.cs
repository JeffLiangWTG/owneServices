using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed partial class GatePassMovementBuilderTest : TestCaseWithFactory
	{
		public void TestGeneral_ForwardingShipment()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea);

			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertDataObject(dataObject, "Sea", "FCL", "11", "ARR123", "FDN456", "");

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertDataObject(dataObject, "Road", "FCL", "20", "ARR123", "", "");

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertDataObject(dataObject, "Air", "LSE", "1", "", "114-22882053", "HB123");

			shipment.JS_TransportMode = Core.Constants.TransportModes.Other;
			dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertDataObject(dataObject, "Other", "LSE", "", "", "", "");
		}

		public void TestProcessType_ForwardingShipment()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("IL => Import", "1", dataObject.ProcessType);

			shipment.JS_RL_NKDestination = "ITMIL";
			dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("Not IL => Export", "3", dataObject.ProcessType);
		}

		public void TestCargoType_ForwardingShipment()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("FCL", "FCL", dataObject.CargoType.Code);
		}

		public void TestCargoType_InList_ForwardingShipment()
		{
			const string messageError = "Cargo Type should be selected from the list";

			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			var codeInfo = ((CodeDescription)dataObject.CargoType).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoType.Code = "NotInList";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestOriginSite_Mandatory_ForwardingShipment()
		{
			const string messageError = "Origin Site must be provided";
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();

			var codeInfo = ((CodeDescription)dataObject.OriginSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.OriginSite.Code = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestOriginSite_InList_ForwardingShipment()
		{
			const string messageError = "Origin Site should be selected from the list";
			CreateFacilities();

			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			var codeInfo = ((CodeDescription)dataObject.OriginSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.OriginSite.Code = "IL000001";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestDestinationSite_Mandatory_ForwardingShipment()
		{
			const string messageError = "Destination Site must be provided";
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();

			var codeInfo = ((CodeDescription)dataObject.DestinationSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.DestinationSite.Code = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestDestinationSite_InList_ForwardingShipment()
		{
			const string messageError = "Destination Site should be selected from the list";
			CreateFacilities();

			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			var codeInfo = ((CodeDescription)dataObject.DestinationSite).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.DestinationSite.Code = "IL000001";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestTransportMethod_Mandatory_ForwardingShipment()
		{
			const string messageError = "Transport Method must be provided";
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();

			var codeInfo = ((CodeDescription)dataObject.TransportMethod).CodeInfo;
			AssertHasMessageError(codeInfo, messageError);

			dataObject.TransportMethod.Code = "1";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);
		}

		public void TestTransportMethod_InList_ForwardingShipment()
		{
			const string messageError = "Transport Method should be selected from the list";
			CreateILTransportMethod();

			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			var codeInfo = ((CodeDescription)dataObject.TransportMethod).CodeInfo;
			dataObject.TransportMethod.Code = "93";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);

			dataObject.TransportMethod.Code = "NotInList";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierType_Mandatory_ForwardingShipment()
		{
			const string messageError = "Cargo Identifier Type must be provided";
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();

			var codeInfo = ((CodeDescription)dataObject.CargoIdentifierType).CodeInfo;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoIdentifierType.Code = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierType_InList_ForwardingShipment()
		{
			const string messageError = "Cargo Identifier Type should be selected from the list";
			CreateILCargoIdentifierType();

			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			var codeInfo = ((CodeDescription)dataObject.CargoIdentifierType).CodeInfo;
			dataObject.TransportMethod.Code = "11";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoIdentifierType.Code = "NotInList";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierKey1_Mandatory_ForwardingShipment()
		{
			const string messageError = "Cargo Identifier Key 1 must be provided";
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();

			var codeInfo = dataObject.CargoIdentifierKey1Info;
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoIdentifierKey1 = ZString.Empty;
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestCargoIdentifierKey2_Mandatory_ForwardingShipment()
		{
			const string messageError = "Cargo Identifier Key 2 must be provided";
			CombineAssertions("When TransportMode!=Road", () =>
			{
				var seaShipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
				var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(seaShipment)).Build();

				var codeInfo = dataObject.CargoIdentifierKey2Info;
				AssertNoMessageError(codeInfo, messageError);

				dataObject.CargoIdentifierKey2 = ZString.Empty;
				dataObject.ValidateAll();
				AssertHasMessageError(codeInfo, messageError);
			});

			CombineAssertions("When TransportMode==Road", () =>
			{
				var roadShipment = CreateShipment(Core.Constants.TransportModes.Road, addReceivingForwarderVat: false);
				var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(roadShipment)).Build();
				dataObject.ValidateAll();
				AssertNoMessageError(dataObject.CargoIdentifierKey2Info, messageError);
			});
		}

		public void TestBuildCargoIdentifierKey1_WhenAir_ForwardingShipment()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Air);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("When TransportMode==Air, ATD and ETD are empty", "", dataObject.CargoIdentifierKey1);

			shipment = CreateShipment(Core.Constants.TransportModes.Air, addETD: true);
			dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("When TransportMode==Air, ATD is empty, take ETD", "2023", dataObject.CargoIdentifierKey1);

			shipment = CreateShipment(Core.Constants.TransportModes.Air, addETD: true, addATD: true);
			dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("When TransportMode==Air, when ATD is not empty, take ATD", "2024", dataObject.CargoIdentifierKey1);
		}

		public void TestCargoIdentifierKey3IsVisible_ForwardingShipment()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("When TransportMode!=Air", false, dataObject.CargoIdentifierKey3IsVisible);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
			AssertEquals("When TransportMode==Air", true, dataObject.CargoIdentifierKey3IsVisible);
		}

		void AssertDataObject(GatePassMovementDocDataObject dataObject, string whenTransportMode, string expectedCargoType, string expectedCargoIdentifierType, string expectedCargoIdentifierKey1, string expectedCargoIdentifierKey2, string expectedCargoIdentifierKey3)
		{
			CombineAssertions($"When TransportMode={whenTransportMode}", () =>
			{
				AssertEquals("SourceType", "ForwardingShipment", ((IDataSourceProvider)dataObject).SourceType);
				AssertEquals("SourceID", "UNIQ123", ((IDataSourceProvider)dataObject).SourceID);
				AssertEquals("GatePassMovementNumber", "100000007", dataObject.GatePassMovementNumber);
				AssertEquals("ProcessType", "1", dataObject.ProcessType);

				AssertEquals("OriginSite", "ILASH", dataObject.OriginSite.Code);
				AssertEquals("DestinationSite", "ILTLV", dataObject.DestinationSite.Code);
				AssertEquals("CargoType", expectedCargoType, dataObject.CargoType.Code);
				AssertEquals("TransportMethod", "", dataObject.TransportMethod.Code);

				AssertEquals("CargoIdentifierType", expectedCargoIdentifierType, dataObject.CargoIdentifierType.Code);
				AssertEquals("CargoIdentifierKey1", expectedCargoIdentifierKey1, dataObject.CargoIdentifierKey1);
				AssertEquals("CargoIdentifierKey2", expectedCargoIdentifierKey2, dataObject.CargoIdentifierKey2);
				AssertEquals("CargoIdentifierKey3", expectedCargoIdentifierKey3, dataObject.CargoIdentifierKey3);
			});
		}

		ForwardingShipment CreateShipment(string transportMode, bool addReceivingForwarderVat = true, bool addImportBrokerVat = true, bool addImportReleaseDepot = true, bool addShipmentFdn = true, bool addETD = false, bool addATD = false)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UniqueConsignRef = "UNIQ123";
			shipment.JS_RL_NKDestination = "ILASH";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_GMN = "100000007";
			shipment.JS_HouseBill = "HB123";

			if (addShipmentFdn)
			{
				var fdnShipment = shipment.Numbers.AddNew();
				fdnShipment.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
				fdnShipment.CE_EntryNum = "FDN456";
			}

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "ILASH";
			consol.MasterBillAirlinePrefix = "114";
			consol.MasterBillMAWB = "22882053";
			var fdnConsol = consol.CusEntryNums.AddNew();
			fdnConsol.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
			fdnConsol.CE_EntryNum = "FDN123";

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "ILASH";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Ashdod";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "IL";
			if (addReceivingForwarderVat)
			{
				receivingForwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "111222333", "IL");
			}

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "ILASH";
			transport.JW_ArrivalPortRouteId = "ARR123";
			transport.JW_ETD = addETD ? new ZDateTime(2023, 1, 1) : ZDateTime.Empty;
			transport.JW_ATD = addATD ? new ZDateTime(2024, 1, 1) : ZDateTime.Empty;

			var importBroker = Factory.New<OrgHeader>();
			importBroker.OH_FullName = "I'm Import Broker";
			importBroker.OH_RL_NKClosestPort = "ILTLV";
			importBroker.MainAddress.Address1 = "Unit 400";
			importBroker.MainAddress.Address2 = "51 What Lane";
			importBroker.MainAddress.City = "Tel Aviv";
			importBroker.MainAddress.Postcode = "5024";
			importBroker.MainAddress.OA_RN_NKCountryCode = "IL";
			if (addImportBrokerVat)
			{
				importBroker.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "555666777", "IL");
			}

			shipment.JS_OH_ImportBroker = importBroker.PK;

			if (addImportReleaseDepot)
			{
				var importReleaseDepot = Factory.New<OrgHeader>();
				importReleaseDepot.OH_FullName = "I'm Import Release Depo";
				importReleaseDepot.OH_RL_NKClosestPort = "ILTLV";
				importReleaseDepot.MainAddress.Address1 = "Unit 500";
				importReleaseDepot.MainAddress.Address2 = "52 What Lane";
				importReleaseDepot.MainAddress.City = "Jerusalem";
				importReleaseDepot.MainAddress.Postcode = "5025";
				importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "IL";
				importReleaseDepot.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "ILTLV", "IL");

				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;
			}

			return shipment;
		}
	}
}
