using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.Builders.Testing
{
	sealed class PackLineBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateGoodsDescription_MarksAndNumbers()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;

			var shipmentASM = consol.Shipments.AddNew();
			shipmentASM.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipmentASM.DetailedGoodsDescriptionNoteText = "ship asm detailed desc";
			shipmentASM.JS_GoodsDescription = "ship asm desc";
			shipmentASM.JS_MarksAndNumbers = "ship asm marks";

			var shipmentSTD = shipmentASM.CoLoadShipments.AddNew();
			shipmentSTD.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipmentSTD.DetailedGoodsDescriptionNoteText = "ship std detailed desc";
			shipmentSTD.JS_GoodsDescription = "ship std desc";
			shipmentSTD.JS_MarksAndNumbers = "ship std marks";

			var packline = shipmentSTD.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_DetailedDescription = "pack detailed desc";
			packline.JL_Description = "pack desc";
			packline.JL_MarksAndNumbers = "pack marks";

			var packLineBuilder = new PackingLineBuilder();

			var buildPackingLine = packLineBuilder.Build(packline);
			AssertEquals("pack detailed desc", buildPackingLine.GoodsDescription, "pack detailed desc");
			AssertEquals("pack marks", buildPackingLine.MarksAndNumbers, "pack marks");

			packline.JL_DetailedDescription = string.Empty;
			packline.JL_MarksAndNumbers = string.Empty;
			buildPackingLine = packLineBuilder.Build(packline);
			AssertEquals("pack desc", buildPackingLine.GoodsDescription, "pack desc");
			AssertEquals("ship std marks", buildPackingLine.MarksAndNumbers, "ship std marks");

			packline.JL_Description = string.Empty;
			shipmentSTD.JS_MarksAndNumbers = string.Empty;
			buildPackingLine = packLineBuilder.Build(packline);
			AssertEquals("ship std detailed desc", buildPackingLine.GoodsDescription, "ship std detailed desc");
			AssertEquals("ship asm marks", buildPackingLine.MarksAndNumbers, "ship asm marks");

			shipmentSTD.DetailedGoodsDescriptionNoteText = string.Empty;
			buildPackingLine = packLineBuilder.Build(packline);
			AssertEquals("ship std desc", buildPackingLine.GoodsDescription, "ship std desc");

			shipmentSTD.JS_GoodsDescription = string.Empty;
			buildPackingLine = packLineBuilder.Build(packline);
			AssertEquals("ship asm detailed desc", buildPackingLine.GoodsDescription, "ship asm detailed desc");

			shipmentASM.DetailedGoodsDescriptionNoteText = string.Empty;
			buildPackingLine = packLineBuilder.Build(packline);
			AssertEquals("ship asm desc", buildPackingLine.GoodsDescription, "ship asm desc");

			shipmentASM.JS_GoodsDescription = string.Empty;
			shipmentASM.JS_MarksAndNumbers = string.Empty;
			buildPackingLine = packLineBuilder.Build(packline);
			AssertEquals("GoodsDescription empty", buildPackingLine.GoodsDescription, string.Empty);
			AssertEquals("MarksAndNumbers empty", buildPackingLine.MarksAndNumbers, string.Empty);
		}

		public void TestPopulateRORODetails()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP000001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			shipment.JS_BookingReference = "BR00001";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_VehicleTransmission = Core.Constants.VehicleTransmissionType.Automatic;
			packLine.JL_VehicleColor = "White";
			packLine.JL_VehicleMake = "Volga";
			packLine.JL_VehicleModel = "GAZ-24";
			packLine.JL_VehicleNumberOfDoors = 4;
			packLine.JL_VehicleYear = 1953;
			packLine.JL_RefNumber = "1234567890";
			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = 1.1m;
			packLine.JL_RequiredTemperatureMaximum = 100.1m;
			packLine.JL_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;

			var packingLine = new PackingLineBuilder().Build(packLine);

			AssertEquals("VehicleTransmission.Code", Core.Constants.VehicleTransmissionType.Automatic, packingLine.VehicleTransmission.Code);
			AssertEquals("VehicleTransmission.Description", "Automatic", packingLine.VehicleTransmission.Description);
			AssertEquals("VehicleColor", "White", packingLine.VehicleColor);
			AssertEquals("VehicleMake", "Volga", packingLine.VehicleMake);
			AssertEquals("VehicleModel", "GAZ-24", packingLine.VehicleModel);
			AssertEquals("VehicleNumberOfDoors", 4, packingLine.VehicleNumberOfDoors);
			AssertEquals("VehicleYear", 1953, packingLine.VehicleYear);
			AssertEquals("VIN", "1234567890", packingLine.VIN);
			AssertEquals("ShipmentID", "SHP000001", packingLine.ShipmentID);
			Assert("RequiresTemperatureControl", packingLine.RequiresTemperatureControl);
			AssertEquals("TemperatureMinimum.Value", 1.1m, packingLine.TemperatureMinimum.Value);
			AssertEquals("TemperatureMinimum.Unit.Code", "C", packingLine.TemperatureMinimum.Unit.Code);
			AssertEquals("TemperatureMaximum.Value", 100.1m, packingLine.TemperatureMaximum.Value);
			AssertEquals("TemperatureMaximum.Unit.Code", "C", packingLine.TemperatureMaximum.Unit.Code);
			AssertEquals("ShippersRef", "BR00001", packingLine.ShippersRef);
		}

		public void TestPopulatePackingLineID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP000001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			shipment.JS_BookingReference = "BR00001";

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_VehicleTransmission = Core.Constants.VehicleTransmissionType.Automatic;
			packLine.JL_VehicleColor = "White";
			packLine.JL_VehicleMake = "Volga";
			packLine.JL_VehicleModel = "GAZ-24";
			packLine.JL_VehicleNumberOfDoors = 4;
			packLine.JL_VehicleYear = 1953;
			packLine.JL_RefNumber = "1234567890";
			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = 1.1m;
			packLine.JL_RequiredTemperatureMaximum = 100.1m;
			packLine.JL_PackLineId = "PackLineIdo.O";
			packLine.JL_RequiredTemperatureUnit = Core.Constants.Temperature.Centigrade;

			var packingLineBuilder = new PackingLineBuilder();
			AssertEquals("PackLineIdo.O", packingLineBuilder.Build(packLine).PackingLineID);

			packLine.JL_PackLineId = ZString.Empty;
			AssertNullOrEmpty(packingLineBuilder.Build(packLine).PackingLineID);
		}

		public void TestPopulateWeightAndMeasures()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeight = 15;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_ActualVolume = 2000;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicDecimetres;
			packLine.JL_Length = 1;
			packLine.JL_Width = 2;
			packLine.JL_Height = 3;
			packLine.JL_UnitOfDimension = Core.Constants.Length.Feet;

			var packLineDO = new PackingLineBuilder().Build(packLine);

			AssertEquals("15.00 KG", packLineDO.Weight.ToString());
			AssertEquals("2.00 M3", packLineDO.Volume.ToString());
			AssertEquals("0.91 M", packLineDO.Height.ToString());
			AssertEquals("0.30 M", packLineDO.Length.ToString());
			AssertEquals("0.61 M", packLineDO.Width.ToString());
		}

		public void TestSortingTransports()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C00002000";
			consol1.JK_AgentType = Constants.AgentType.Agent;
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_RL_NKLoadPort = "DEHAM";
			consol1.JK_RL_NKDischargePort = "CNNBO";

			CreateTransport(consol1, 1, Constants.TransportModes.Rail, Constants.TransportPlanningType.PreCarriage, "DEHAM", "BEANR", new ZDateTime(2021, 01, 21, 7, 35, 00), new ZDateTime(2021, 01, 21, 16, 15, 00));
			CreateTransport(consol1, 2, Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel, "BEANR", "NLAMS", new ZDateTime(2021, 01, 23, 10, 40, 00), new ZDateTime(2021, 01, 23, 17, 10, 00));
			CreateTransport(consol1, 3, Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel, "NLAMS", "CNNBO", new ZDateTime(2021, 02, 01, 09, 30, 00), new ZDateTime(2021, 02, 10, 12, 25, 00));

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C00002001";
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol2.JK_RL_NKLoadPort = "FRPAR";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			CreateTransport(consol2, 1, Constants.TransportModes.Truck, Constants.TransportPlanningType.PreCarriage, "FRPAR", "BEANR", new ZDateTime(2021, 01, 18, 10, 00, 00), new ZDateTime(2021, 01, 18, 15, 20, 00));
			CreateTransport(consol2, 2, Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel, "BEANR", "NLAMS", new ZDateTime(2021, 01, 24, 10, 40, 00), new ZDateTime(2021, 01, 24, 17, 10, 00));
			CreateTransport(consol2, 3, Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel, "NLAMS", "SGSIN", new ZDateTime(2021, 02, 01, 09, 30, 00), new ZDateTime(2021, 02, 13, 8, 50, 00));

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00003000";
			shipment.JS_HouseBill = "S00003000";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "BEANR";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_RL_NKLoadPort = "BEANR";
			shipment.JS_RL_NKDischargePort = "SGSIN";
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(10);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 80;
			packline.JL_F3_NKPackType = "PLT";
			packline.JL_ActualWeight = 18000;
			packline.JL_ActualWeightUQ = "KG";
			packline.JL_ActualVolume = 40;
			packline.JL_ActualVolumeUQ = "M3";
			packline.JL_Description = "ROOF COVERING";
			packline.JL_MarksAndNumbers = "MarksAndNos";

			CreateHarmonizedCode(packline, "000000", ZString.Empty);
			CreateHarmonizedCode(packline, "111111", Constants.CountryCodes.Germany);
			CreateHarmonizedCode(packline, "222222", Constants.CountryCodes.Belgium);
			CreateHarmonizedCode(packline, "333333", Constants.CountryCodes.China);
			CreateHarmonizedCode(packline, "444444", Constants.CountryCodes.Singapore);
			CreateHarmonizedCode(packline, "555555", Constants.CountryCodes.Netherlands);
			CreateHarmonizedCode(packline, "666666", Constants.CountryCodes.France);

			consol1.Shipments.Add(shipment);
			consol2.Shipments.Add(shipment);

			var packingLine = new PackingLineBuilder().Build(packline);

			AssertEquals("ExportHarmonizedCode.Code", "666666", packingLine.ExportHarmonizedCode.Code);
			AssertEquals("ExportHarmonizedCode.Country.Code", Constants.CountryCodes.France, packingLine.ExportHarmonizedCode.Country.Code);
			AssertEquals("ImportHarmonizedCode.Code", "444444", packingLine.ImportHarmonizedCode.Code);
			AssertEquals("ImportHarmonizedCode.Country.Code", Constants.CountryCodes.Singapore, packingLine.ImportHarmonizedCode.Country.Code);
		}

		public void TestPopulatePackingLineIdentifier()
		{
			var expectedIdentifier = "PackingLineID";
			var packingLineBO = Factory.NewWithValidTestData<PackLine>();

			var packingLine1 = new PackingLineBuilder().Build(packingLineBO, packingLineIdentifier: expectedIdentifier);
			AssertEquals("Should use the passed in identifier", expectedIdentifier, packingLine1.Identifier);

			var packingLine2 = new PackingLineBuilder().Build(packingLineBO);
			AssertEquals("Should use the packLine PK if no identifier passed", packingLineBO.PK, packingLine2.Identifier);
		}

		public void TestPopulateHouseBillPaymentType()
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

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			var job = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
			job.JH_OC_LocalBillingContact = contact.PK;

			var orgHeader = (OrgHeader)job.LocalZAddressWithContact.OrgHeader;
			var packLineBuilder = new PackingLineBuilder();

			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "CBC";
			var buildPackingLine = packLineBuilder.Build(packLine);
			AssertEquals(buildPackingLine.HBLPaymentType, "A");

			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "CCD";
			buildPackingLine = packLineBuilder.Build(packLine);
			AssertEquals(buildPackingLine.HBLPaymentType, "B");

			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "CHK";
			buildPackingLine = packLineBuilder.Build(packLine);
			AssertEquals(buildPackingLine.HBLPaymentType, "C");

			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "TRF";
			buildPackingLine = packLineBuilder.Build(packLine);
			AssertEquals(buildPackingLine.HBLPaymentType, "H");

			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "DBC";
			buildPackingLine = packLineBuilder.Build(packLine);
			AssertEquals("Expected payment type to default to 'Other' when no matching payment method is found.", buildPackingLine.HBLPaymentType, "D");

			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "";
			buildPackingLine = packLineBuilder.Build(packLine);
			AssertEquals("Expected payment type to default to 'Other' when no matching payment method is found.", buildPackingLine.HBLPaymentType, "D");

			var paymentMethod = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value;
			paymentMethod.Add("AAA", (NoResString)"business cheque");
			paymentMethod.Add("BBB", (NoResString)"E-Payment");
			paymentMethod.Add("CCC", (NoResString)"Electronic funds transfer");

			using (OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, paymentMethod))
			{
				orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "AAA";
				buildPackingLine = packLineBuilder.Build(packLine);
				AssertEquals(buildPackingLine.HBLPaymentType, "C");

				orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "BBB";
				buildPackingLine = packLineBuilder.Build(packLine);
				AssertEquals(buildPackingLine.HBLPaymentType, "H");

				orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "BBB";
				buildPackingLine = packLineBuilder.Build(packLine);
				AssertEquals(buildPackingLine.HBLPaymentType, "H");
			}
		}

		public void TestPopulateHouseBillPaymentType_CoLoadMasterShipment()
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

			var masterShipment = consol.Shipments.AddNew();
			masterShipment.FillWithValidTestData();

			var job = new JobHeader.Loader(Factory, masterShipment).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = org.MainAddress.PK;
			job.JH_OC_LocalBillingContact = contact.PK;

			var orgHeader = (OrgHeader)job.LocalZAddressWithContact.OrgHeader;
			orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = "CBC";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 2;
			packLine.JL_F3_NKPackType = "PLT";

			var packLineBuilder = new PackingLineBuilder();
			var buildPackingLine = packLineBuilder.Build(packLine);
			AssertEquals(buildPackingLine.HBLPaymentType, "A");
		}

		void CreateHarmonizedCode(ForwardingPackLine packline, ZString code, ZString country)
		{
			var harmonizedCode = packline.HarmonisedCodes.AddNew();
			harmonizedCode.JLH_Code = code;
			harmonizedCode.JLH_RN_NKCountry = country;
		}

		void CreateTransport(ForwardingConsol consol, ZByte legorder, ZString mode, ZString type, ZString loadPort, ZString discPort, ZDateTime etd, ZDateTime eta)
		{
			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = legorder;
			transport.JW_TransportMode = mode;
			transport.JW_TransportType = type;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_Vessel = "COSCO X";
			transport.JW_VoyageFlight = "12345";
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
		}
	}
}
