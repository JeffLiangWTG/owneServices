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
	sealed class DeliveryOrderBuilderTest : TestCaseWithFactory
	{
		public void TestGeneral()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea);

			var dataObject = new DeliveryOrderBuilder(shipment).Build();
			AssertDataObject(dataObject, "11", "ARR123", expectEmptyDealNumber: false);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			dataObject = new DeliveryOrderBuilder(shipment).Build();
			AssertDataObject(dataObject, "20", "FDN456", expectEmptyDealNumber: true);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			dataObject = new DeliveryOrderBuilder(shipment).Build();
			AssertDataObject(dataObject, "", "", expectEmptyDealNumber: true);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Other;
			dataObject = new DeliveryOrderBuilder(shipment).Build();
			AssertDataObject(dataObject, "", "", expectEmptyDealNumber: true);
		}

		public void TestForwarderVat_Mandatory()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addReceivingForwarderVat: false);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();

			AssertHasMessageError(dataObject.ForwarderVatInfo, "Forwarder Vat must be provided");

			dataObject.ForwarderVat = "111222333";
			dataObject.ValidateAll();
			AssertNoMessageError(dataObject.ForwarderVatInfo, "Forwarder Vat must be provided");
		}

		public void TestCustomsBrokerVat_Mandatory()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addImportBrokerVat: false);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();

			AssertHasMessageError(dataObject.CustomsBrokerVatInfo, "Customs Broker Vat must be provided");

			dataObject.CustomsBrokerVat = "111222333";
			dataObject.ValidateAll();
			AssertNoMessageError(dataObject.CustomsBrokerVatInfo, "Customs Broker Vat must be provided");
		}

		public void TestDeliverySite_Mandatory()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addImportReleaseDepot: false);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();

			AssertHasMessageError(((CodeDescription)dataObject.DeliverySite).CodeInfo, "Delivery Site must be provided");

			dataObject.DeliverySite.Code = "111222333";
			AssertNoMessageError(((CodeDescription)dataObject.DeliverySite).CodeInfo, "Delivery Site must be provided");
		}

		public void TestDeliverySite_InList()
		{
			const string messageError = "Delivery Site should be selected from the list";
			CreateFacilities();

			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addImportReleaseDepot: false);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();
			var codeInfo = ((CodeDescription)dataObject.DeliverySite).CodeInfo;
			AssertHasMessageError(codeInfo, messageError);

			dataObject.DeliverySite.Code = "IL000001";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);
		}

		public void TestDeliverySite()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea);
			shipment.JS_RL_NKDestination = "ILHFA";
			var dataObject = new DeliveryOrderBuilder(shipment).Build();
			AssertEquals("When ImportReleaseDepot has valid CCP", "1234", dataObject.DeliverySite.Code);

			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			dataObject = new DeliveryOrderBuilder(shipment).Build();
			AssertEquals("When ImportReleaseDepot has not valid CCP, but Destination is not null", "ILHFA", dataObject.DeliverySite.Code);

			shipment.JS_RL_NKDestination = ZString.Empty;
			dataObject = new DeliveryOrderBuilder(shipment).Build();
			AssertNullOrEmpty("When ImportReleaseDepot has not valid CCP, and Destination is null", dataObject.DeliverySite.Code);
		}

		public void TestCargoIdentifierType_Mandatory()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Other);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();

			AssertHasMessageError(((CodeDescription)dataObject.CargoIdentifierType).CodeInfo, "Cargo Identifier Type must be provided");

			dataObject.CargoIdentifierType.Code = "15";
			AssertNoMessageError(((CodeDescription)dataObject.CargoIdentifierType).CodeInfo, "Cargo Identifier Type must be provided");
		}

		public void TestCargoIdentifierType_InList()
		{
			const string messageError = "Cargo Identifier Type should be selected from the list";
			CreateILCargoIdentifierType();

			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addImportReleaseDepot: false);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();
			var codeInfo = ((CodeDescription)dataObject.CargoIdentifierType).CodeInfo;
			dataObject.CargoIdentifierType.Code = "11";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);

			dataObject.CargoIdentifierType.Code = "NotInList";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		public void TestManifestNumber_Mandatory()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Truck);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();

			dataObject.ValidateAll();
			AssertHasMessageError(dataObject.ManifestNumberInfo, "Manifest Number must be provided");

			dataObject.ManifestNumber = "1234";
			dataObject.ValidateAll();
			AssertNoMessageError(dataObject.ManifestNumberInfo, "Manifest Number must be provided");
		}

		public void TestDealNumber_Mandatory()
		{
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea, addShipmentFdn: false);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();

			AssertHasMessageError(dataObject.DealNumberInfo, "Deal Number must be provided");

			dataObject.DealNumber = "1234";
			dataObject.ValidateAll();
			AssertNoMessageError(dataObject.DealNumberInfo, "Deal Number must be provided");
		}

		public void TestReceiverType_Mandatory()
		{
			const string messageError = "Receiver Type must be provided";

			var shipment = CreateShipment(Core.Constants.TransportModes.Other);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();
			var codeInfo = ((CodeDescription)dataObject.ReceiverType).CodeInfo;

			AssertHasMessageError(codeInfo, messageError);

			dataObject.ReceiverType.Code = "3";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);
		}

		public void TestReceiverType_InList()
		{
			const string messageError = "Receiver Type should be selected from the list";

			var shipment = CreateShipment(Core.Constants.TransportModes.Other);
			var dataObject = new DeliveryOrderBuilder(shipment).Build();
			var codeInfo = ((CodeDescription)dataObject.ReceiverType).CodeInfo;

			dataObject.ReceiverType.Code = "3";
			dataObject.ValidateAll();
			AssertNoMessageError(codeInfo, messageError);

			dataObject.ReceiverType.Code = "1";
			dataObject.ValidateAll();
			AssertHasMessageError(codeInfo, messageError);
		}

		void AssertDataObject(DeliveryOrderDocDataObject dataObject, string expectedCargoIdentifierType, string expectedManifestNumber, bool expectEmptyDealNumber)
		{
			AssertEquals("SourceType should equal to the expected value", "ForwardingShipment", ((IDataSourceProvider)dataObject).SourceType);
			AssertEquals("SourceID should equal to the expected value", "UNIQ123", ((IDataSourceProvider)dataObject).SourceID);
			AssertEquals("DeliveryOrderNumber should equal to the expected value", "100000007", dataObject.DeliveryOrderNumber);
			AssertEquals("ForwarderName should equal to the expected value", "I'm Receiving Stuff", dataObject.Forwarder.CompanyName);
			AssertEquals("ForwarderAddress should equal to the expected value", "Unit 399", dataObject.Forwarder.AddressLine1);
			AssertEquals("ForwarderCountry should equal to the expected value", "IL", dataObject.Forwarder.Country.Code);
			AssertEquals("ForwarderVat should equal to the expected value", "111222333", dataObject.ForwarderVat);
			AssertEquals("CustomsBrokerName should equal to the expected value", "I'm Import Broker", dataObject.CustomsBroker.CompanyName);
			AssertEquals("CustomsBrokerAddress should equal to the expected value", "Unit 400", dataObject.CustomsBroker.AddressLine1);
			AssertEquals("CustomsBrokerCountry should equal to the expected value", "IL", dataObject.CustomsBroker.Country.Code);
			AssertEquals("CustomsBrokerVat should equal to the expected value", "555666777", dataObject.CustomsBrokerVat);
			AssertEquals("DeliverySite should equal to the expected value", "1234", dataObject.DeliverySite.Code);
			AssertEquals("CargoIdentifierType should equal to the expected value", expectedCargoIdentifierType, dataObject.CargoIdentifierType.Code);
			AssertEquals("ManifestNumber should equal to the expected value", expectedManifestNumber, dataObject.ManifestNumber);
			AssertEquals("ReceiverType should be empty", "", dataObject.ReceiverType.Code);
			if (expectEmptyDealNumber)
			{
				AssertNullOrEmpty("DealNumber", dataObject.DealNumber);
			}
			else
			{
				AssertEquals("DealNumber should equal to the expected value", "FDN456", dataObject.DealNumber);
			}
		}

		ForwardingShipment CreateShipment(string transportMode, bool addReceivingForwarderVat = true, bool addImportBrokerVat = true, bool addImportReleaseDepot = true, bool addShipmentFdn = true)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UniqueConsignRef = "UNIQ123";
			shipment.JS_DLO = "100000007";

			if (addShipmentFdn)
			{
				var fdnShipment = shipment.Numbers.AddNew();
				fdnShipment.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
				fdnShipment.CE_EntryNum = "FDN456";
			}

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "ILASH";
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
				importReleaseDepot.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1234", "IL");

				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;
			}

			return shipment;
		}

		void CreateFacilities()
		{
			var factory = Factory;
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "IL000001", "IL000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();
		}

		void CreateILCargoIdentifierType()
		{
			var factory = Factory;
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "C1259");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType, "11", "SeaDealImport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();
		}
	}
}
