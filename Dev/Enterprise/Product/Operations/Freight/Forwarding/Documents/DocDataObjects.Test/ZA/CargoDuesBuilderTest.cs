using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing.ZA
{
	sealed class CargoDuesBuilderTest : TestCaseWithFactory
	{
		#region TestPopulateFromNewConsolDoesNotThrowException

		public void TestPopulateFromNewConsolDoesNotThrowException()
		{
			var consol = Factory.New<ForwardingConsol>();
			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = "test",
			};
			AssertNoExceptionThrown(() => new CargoDuesBuilder(consol, parameters).Build());
		}

		#endregion

		#region Tests for each Document Type

		public void TestBuildExportFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Export", true, false, "Export");
		}

		public void TestBuildExportQuotationFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Export (Quotation)", true, true, "Export");
		}

		public void TestBuildImportFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Import", false, false, "Import");
		}

		public void TestBuildImportQuotationFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Import (Quotation)", false, true, "Import");
		}

		public void TestBuildLoadCoastwiseFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Load Coastwise", true, false, "Load Coastwise");
		}

		public void TestBuildLoadCoastwiseQuotationFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Load Coastwise (Quotation)", true, true, "Load Coastwise");
		}

		public void TestBuildDischargeCoastwiseFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Discharge Coastwise", false, false, "Discharge Coastwise");
		}

		public void TestBuildDischargeCoastwiseQuotationFromForwardingConsol()
		{
			TestBuildCargoDuesCore("Cargo Dues - Discharge Coastwise (Quotation)", false, true, "Discharge Coastwise");
		}

		public void TestGoodsInfoCollectionWhenFCL()
		{
			TestGoodsInfoCollectionCore("FCL", true);
		}

		public void TestGoodsInfoCollectionWhenLCL()
		{
			TestGoodsInfoCollectionCore("LCL", true);
		}

		public void TestGoodsInfoCollectionWhenGRP()
		{
			TestGoodsInfoCollectionCore("GRP", true);
		}

		public void TestGoodsInfoCollectionWhenBCN()
		{
			TestGoodsInfoCollectionCore("BCN", true);
		}

		public void TestGoodsInfoCollectionWhenBLK()
		{
			TestGoodsInfoCollectionCore("BLK", false, "Dry Bulk");
		}

		public void TestGoodsInfoCollectionWhenLQD()
		{
			TestGoodsInfoCollectionCore("LQD", false, "Liquid Bulk");
		}

		public void TestGoodsInfoCollectionWhenBBK()
		{
			TestGoodsInfoCollectionCore("BBK", false, "Break Bulk");
		}

		public void TestGoodsInfoCollectionWhenROR()
		{
			TestGoodsInfoCollectionCore("ROR", false, "RO-RO");
		}

		#endregion

		#region Population Tests

		public void TestPopulateShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var shipment1 = consol.Shipments.AddNew();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var packline11 = shipment1.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 2;
			packline11.JL_F3_NKPackType = "PLT";

			var packline12 = shipment1.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 2;
			packline12.JL_F3_NKPackType = "PLT";

			shipment1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_ActualWeight = 13.12m;
			shipment1.JS_UnitOfWeight = "KG";
			shipment1.JS_OuterPacks = 4;
			shipment1.JS_F3_NKPackType = "PLT";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline21 = shipment2.OuterPackLines.AddNew();
			packline21.JL_PackageCount = 2;
			packline21.JL_F3_NKPackType = "PLT";

			shipment2.JS_ActualWeight = 5.36m;
			shipment2.JS_UnitOfWeight = "KG";
			shipment2.JS_OuterPacks = 2;
			shipment2.JS_F3_NKPackType = "PLT";

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = "Cargo Dues - Export",
				LogProvider = CreateLogProviderWithMockLogs("Cargo Dues - Export")
			};

			var cargoDues = new CargoDuesBuilder(consol, parameters).Build();

			var shipmentDOs = cargoDues.ShipmentPackingInfos.Cast<ShipmentPackingInfo>().ToArray();
			AssertEquals(2, shipmentDOs.Length);

			var shipmentDO1 = shipmentDOs[0];
			var shipmentDO2 = shipmentDOs[1];

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("shipmentDOs", new[] { shipment1.PK, shipment2.PK }, shipmentDOs.Select(x => (ZGuid)x.Identifier));
				AssertContainsExactElementsInAnyOrder("shipmentDO1.GoodsInfoCollection", new[] { packline11.PK, packline12.PK }, shipmentDO1.GoodsInfoCollection.Select(x => (ZGuid)x.Identifier));
				AssertContainsExactElementsInAnyOrder("shipmentDO2.GoodsInfoCollection", new[] { packline21.PK }, shipmentDO2.GoodsInfoCollection.Select(x => (ZGuid)x.Identifier));

				AssertEquals("shipmentDO1.ShipmentType.Code", "STD", shipmentDO1.ShipmentType.Code);
				AssertEquals("shipmentDO1.TotalWeight.Value", 13.12m, shipmentDO1.TotalWeight.Value);
				AssertEquals("shipmentDO1.TotalWeight.Unit.Code", "KG", shipmentDO1.TotalWeight.Unit.Code);
				AssertEquals("shipmentDO1.OuterPacks", 4, shipmentDO1.OuterPacks);
				AssertEquals("shipmentDO1.PackType.Code", "PLT", shipmentDO1.PackType.Code);

				AssertEquals("shipmentDO2.ShipmentType.Code", "STD", shipmentDO2.ShipmentType.Code);
				AssertEquals("shipmentDO2.TotalWeight.Value", 5.36m, shipmentDO2.TotalWeight.Value);
				AssertEquals("shipmentDO2.TotalWeight.Unit.Code", "KG", shipmentDO2.TotalWeight.Unit.Code);
				AssertEquals("shipmentDO2.OuterPacks", 2, shipmentDO2.OuterPacks);
				AssertEquals("shipmentDO2.PackType.Code", "PLT", shipmentDO2.PackType.Code);
			});

			AssertionHelper.AssertAddressData(shipment1.ConsigneeDocumentaryAddress, shipmentDO1.Consignee);
			AssertionHelper.AssertAddressData(shipment1.ConsignorDocumentaryAddress, shipmentDO1.Consignor);
		}

		public void TestPopulateColoadShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var shipmentASM = consol.Shipments.AddNew();
			shipmentASM.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			var shipmentSTD1 = shipmentASM.CoLoadShipments.AddNew();
			shipmentSTD1.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline11 = shipmentSTD1.OuterPackLines.AddNew();
			packline11.JL_PackageCount = 2;
			packline11.JL_F3_NKPackType = "PLT";

			var packline12 = shipmentSTD1.OuterPackLines.AddNew();
			packline12.JL_PackageCount = 2;
			packline12.JL_F3_NKPackType = "PLT";

			var shipmentSTD2 = shipmentASM.CoLoadShipments.AddNew();
			shipmentSTD2.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline21 = shipmentSTD2.OuterPackLines.AddNew();
			packline21.JL_PackageCount = 2;
			packline21.JL_F3_NKPackType = "PLT";

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = "Cargo Dues - Export",
				LogProvider = CreateLogProviderWithMockLogs("Cargo Dues - Export")
			};

			var cargoDues = new CargoDuesBuilder(consol, parameters).Build();

			var toplevelShipmentDOs = cargoDues.ShipmentPackingInfos.ToArray();
			AssertEquals(1, toplevelShipmentDOs.Length);

			var subShipments = toplevelShipmentDOs.First().ShipmentPackingInfos.ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { packline11.PK, packline12.PK, packline21.PK }, toplevelShipmentDOs.First().AllGoodsInfoIncludeCoLoadCollection.Select(x => (ZGuid)x.Identifier));
			AssertContainsExactElementsInAnyOrder(new[] { shipmentSTD1.PK, shipmentSTD2.PK }, subShipments.Select(x => (ZGuid)x.Identifier));
			AssertContainsExactElementsInAnyOrder(new[] { packline11.PK, packline12.PK }, subShipments[0].GoodsInfoCollection.Select(x => (ZGuid)x.Identifier));
			AssertContainsExactElementsInAnyOrder(new[] { packline21.PK }, subShipments[1].GoodsInfoCollection.Select(x => (ZGuid)x.Identifier));
		}

		public void TestConsigneeAndConsignorAddressesCorrectlyPopulatedIfDirect()
		{
			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = "Cargo Dues - Export",
				LogProvider = CreateLogProviderWithMockLogs("Cargo Dues - Export")
			};
			var consol = CreateConsolWithMockData(Constants.ContainerModes.Bulk);
			consol.JK_AgentType = "DRT";

			var cargoDues = new CargoDuesBuilder(consol, parameters).Build();
			AssertEquals("Consignee", "Consignee", cargoDues.Consignee.CompanyName);
			AssertEquals("Consignor", "Consignor", cargoDues.Shipper.CompanyName);
		}

		void AssertDocumentPropertiesCorrectlyPopulated(CargoDues cargoDues, bool isExportDocument, bool isQuotationDocument, string direction)
		{
			AssertEquals("IsQuotationDocument", isQuotationDocument, cargoDues.IsQuotationDocument);
			AssertEquals("IsExportDocument", isExportDocument, cargoDues.IsExportDocument);
			AssertEquals("Direction", direction, cargoDues.Direction);
			AssertEquals("IsBulk", true, cargoDues.IsBulk);
			AssertEquals("IsBreakBulk", false, cargoDues.IsBreakBulk);
			AssertEquals("IsContainerised", false, cargoDues.IsContainerised);
			AssertEquals("IsDeepSea", true, cargoDues.IsDeepSea);
			AssertEquals("IsTransship", false, cargoDues.IsTranship);
			AssertEquals("IsCoastwise", false, cargoDues.IsCoastwise);
			AssertEquals("CancellingOrder", false, cargoDues.CancellingOrder);
		}

		void AssertTNPAInformationCorrectlyPopulated(CargoDues cargoDues, bool isExportDocument)
		{
			AssertEquals("TNPA Order Number", "QWERTY123", cargoDues.TNPAOrderNumber);
			AssertEquals("TNPA Quotation Number", "", cargoDues.TNPAQuotationNumber);

			if (isExportDocument)
			{
				AssertEquals("TNPA Arrival Number for Exports", "AOEUI1234", cargoDues.Transports.Main.DepartureReference);
				if (cargoDues.IsCoastwise)
				{
					AssertEquals("TNPA Account Number", "Coastwise001", cargoDues.TNPAAccountNumber);
				}
				else
				{
					AssertEquals("TNPA Account Number", "Export001", cargoDues.TNPAAccountNumber);
				}
			}
			else
			{
				AssertEquals("TNPA Arrival Number for Imports", "QWERTY123", cargoDues.Transports.Main.ArrivalReference);
				if (cargoDues.IsCoastwise)
				{
					AssertEquals("TNPA Account Number", "Coastwise001", cargoDues.TNPAAccountNumber);
				}
				else
				{
					AssertEquals("TNPA Account Number", "Import001", cargoDues.TNPAAccountNumber);
				}
			}

			AssertEquals("IMO Number", "Lllloyd", cargoDues.Transports.Main.Vessel.LloydsIMO);
			AssertEquals("Radio Callsign", "Radio123", cargoDues.Transports.Main.Vessel.RadioCallSign);
			AssertEquals("Carrier Code", "SUDU", cargoDues.CarrierCode);
			AssertEquals("ETD", new ZDateTime(2020, 02, 02), cargoDues.Etd);
			AssertEquals("ETA", new ZDateTime(2020, 02, 20), cargoDues.Eta);
		}

		void AssertAddressCorrectlyPopulated(CargoDues cargoDues, ForwardingConsol consol, bool isExportDocument)
		{
			if (isExportDocument)
			{
				AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, cargoDues.Agent);
			}
			else
			{
				AssertionHelper.AssertAddressData(consol.ReceivingForwarderAddress, cargoDues.Agent);
			}

			AssertionHelper.AssertAddressData(consol.SendingForwarderAddress, cargoDues.Shipper);
			AssertionHelper.AssertAddressData(consol.ReceivingForwarderAddress, cargoDues.Consignee);
			AssertionHelper.AssertAddressData(consol.ArrivalCTOAddress, cargoDues.ArrivalCTO);
			AssertionHelper.AssertAddressData(consol.DepartureCTOAddress, cargoDues.DepartureCTO);
			AssertionHelper.AssertAddressData(consol.ShippingLineAddress, cargoDues.ShippingLine);
			AssertionHelper.AssertCurrentUserAddressData(cargoDues.CurrentUser);
		}

		void AssertCountryVesselAndPortInformationCorrectlyPopulated(CargoDues cargoDues, bool isExportDocument)
		{
			AssertEquals("Origin Country", "China", cargoDues.PlaceOfReceipt.Country.Name);
			AssertEquals("Destination Country", "United Kingdom", cargoDues.PlaceOfDelivery.Country.Name);
			AssertEquals("Vessel Name", "Test Vessel Name 2", cargoDues.Transports.Main.Vessel.Name);
			AssertEquals("Voyage Number", "CC456", cargoDues.Transports.Main.VoyageFlightNumber);

			if (isExportDocument)
			{
				AssertEquals("On-Carrier Vessel Name", "Test Vessel Name 3", cargoDues.Transports.OnForwarding.Vessel.Name);
				AssertEquals("On-Carrier Voyage Number", "CC789", cargoDues.Transports.OnForwarding.VoyageFlightNumber);
				AssertEquals("Service Port Name", "Mala Mala", cargoDues.ServicePort.Name);
				AssertEquals("Service Port Code", "ZAAAM", cargoDues.ServicePort.Code);
			}
			else
			{
				AssertEquals("PreCarriage Vessel Name", "Test Vessel Name 1", cargoDues.Transports.PreCarriage.Vessel.Name);
				AssertEquals("PreCarriage Voyage Number", "CC123", cargoDues.Transports.PreCarriage.VoyageFlightNumber);
				AssertEquals("Service Port", "Cape Town", cargoDues.ServicePort.Name);
				AssertEquals("Service Port Code", "ZACPT", cargoDues.ServicePort.Code);
			}

			AssertEquals("Port of Discharge Name", "Felixstowe", cargoDues.PortOfDischarge.Name);
			AssertEquals("Port of Discharge Code", "GBFXT", cargoDues.PortOfDischarge.Code);
			AssertEquals("Port of Loading Name", "Shanghai Hongqiao International Apt", cargoDues.PortOfLoading.Name);
			AssertEquals("Port of Loading Code", "CNSHA", cargoDues.PortOfLoading.Code);
		}

		void AssertAdditionalInformationCorrectlyPopulated(CargoDues cargoDues)
		{
			AssertEquals("Client Reference", "C00001000", cargoDues.ClientRef);
			AssertEquals("Way Bill Number", "MasterB123", cargoDues.WayBillNumber);
			AssertEquals("Container Operator", "SUDU", cargoDues.ContainerOperator);
			AssertEquals("Terminal", "UDUS", cargoDues.Terminal);
		}

		void AssertCargoDuesCorrectlyPopulated(CargoDues cargoDues)
		{
			AssertEquals("Cargo Dues", cargoDues.CargoDuesSectionTitle);
		}

		void AssertContainersInformation(CargoDues cargoDues)
		{
			var containersAsAssertString = cargoDues.Containers.Select(container => ContainerToAssertString(container));

			AssertMultilineASCIIEquals("Containers (IsContainerised)",
@"CONTAINER1|10|10GP|Empty|10 KG
CONTAINER2|90|20GP|Full|90 KG",
				string.Join("\r\n", containersAsAssertString));
			AssertEquals("TotalNumberOfPacks", 100, cargoDues.TotalNumberOfPacks);
		}

		void AssertGoodsInfoCollectionPopulatedFromShipmentInformation(CargoDues cargoDues, string expectedPackageDescription)
		{
			var shipments = cargoDues.ShipmentPackingInfos.ToArray();
			var goodsInfoAsAssertString1 = shipments[0].GoodsInfoCollection.Select(goodsInfo => GoodsInfoToAssertString(goodsInfo));
			var goodsInfoAsAssertString2 = shipments[1].GoodsInfoCollection.Select(goodsInfo => GoodsInfoToAssertString(goodsInfo));

			AssertMultilineASCIIEquals("GoodsInfoCollection (IsNotContainerised) from shipment1",
$@"packLineMarksAndNos1|10|PT1|{expectedPackageDescription} - shipment goods 1|10.00 KG",
				string.Join("\r\n", goodsInfoAsAssertString1));

			AssertMultilineASCIIEquals("GoodsInfoCollection (IsNotContainerised) from shipment2",
$@"packLineMarksAndNos2|20|PT2|{expectedPackageDescription} - packlineDes2|20.00 KG
shipmentMark|30|PT3|{expectedPackageDescription} - shipmentDescription2|30.00 KG
packLineMarksAndNos4|40|PT4|{expectedPackageDescription} - packlineDetailedDes4|40.00 KG",
				string.Join("\r\n", goodsInfoAsAssertString2));

			AssertEquals("TotalNumberOfPacks", 100, cargoDues.TotalNumberOfPacks);
		}

		public void TestCargoDuesInformation()
		{
			var taxRate = AccTaxRate.Helper.FindTaxRate(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
			taxRate.SetRateNumerator_ForTestOnly(15);
			taxRate.Factory.Save();

			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesExport);
			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesExportQuotation);
			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesImport);
			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesImportQuotation);
			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesLoadCoastwise);
			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesLoadCoastwiseQuotation);
			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesDischargeCoastwise);
			RunCargoDuesInformationTestingCore(DocumentNames.CargoDuesDischargeCoastwiseQuotation);
		}

		void RunCargoDuesInformationTestingCore(string documentName)
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_AgentType = Constants.AgentType.Agent;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;

			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = documentName,
				LogProvider = CreateLogProviderWithMockLogs(documentName)
			};
			var cargoDues = new CargoDuesBuilder(consol1, parameters).Build();

			CombineAssertions(() =>
			{
				AssertCargoDueInformation(cargoDues.DuesCollectionElement1, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement2, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement3, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement4, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement5, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement6, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement7, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement8, ZString.Empty, ZString.Empty);

				AssertEquals("SubTotal", ZDecimal.Zero, cargoDues.SubTotal);
				AssertEquals("VAT = 15% SubTotal", ZDecimal.Zero, cargoDues.VAT);
				AssertEquals("TotalR = SubTotal + VAT", ZDecimal.Zero, cargoDues.TotalR);
			});

			AddDataLinkedEvent(consol1, "39996", documentName);
			cargoDues = new CargoDuesBuilder(consol1, parameters).Build();

			CombineAssertions(() =>
			{
				AssertCargoDueInformation(cargoDues.DuesCollectionElement1, "Desc1", "1111");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement2, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement3, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement4, ZString.Empty, "abc");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement5, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement6, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement7, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement8, ZString.Empty, ZString.Empty);

				AssertEquals("SubTotal", 39996m, cargoDues.SubTotal);
				AssertEquals("VAT = 15% SubTotal", 5999.40m, cargoDues.VAT);
				AssertEquals("TotalR = SubTotal + VAT", 45995.40m, cargoDues.TotalR);
			});

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_AgentType = Constants.AgentType.Agent;
			consol2.JK_ConsolMode = Constants.ContainerModes.FCL;
			AddDataLinkedEvent(consol2, "xxx", documentName);
			cargoDues = new CargoDuesBuilder(consol2, parameters).Build();

			CombineAssertions(() =>
			{
				AssertCargoDueInformation(cargoDues.DuesCollectionElement1, "Desc1", "1111");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement2, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement3, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement4, ZString.Empty, "abc");
				AssertCargoDueInformation(cargoDues.DuesCollectionElement5, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement6, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement7, ZString.Empty, ZString.Empty);
				AssertCargoDueInformation(cargoDues.DuesCollectionElement8, ZString.Empty, ZString.Empty);

				AssertEquals("SubTotal", ZDecimal.Zero, cargoDues.SubTotal);
				AssertEquals("VAT = 15% SubTotal", ZDecimal.Zero, cargoDues.VAT);
				AssertEquals("TotalR = SubTotal + VAT", ZDecimal.Zero, cargoDues.TotalR);
			});
		}

		void AssertCargoDueInformation(DuesCollectionRow cargoDue, ZString description, ZString amount)
		{
			AssertEquals("Description", description, cargoDue.Description);
			AssertEquals("Factor", ZString.Empty, cargoDue.Factor);
			AssertEquals("Rate", ZString.Empty, cargoDue.Rate);
			AssertEquals("Amount", amount, cargoDue.Amount);
		}

		#endregion

		#region Validation Tests

		void TestValidations(CargoDues cargoDues)
		{
			CombineAssertions(() =>
			{
				AssertCompanyNameValidations(cargoDues);
				AssertTNPADataValidations(cargoDues);
				AssertCountryVesselAndPortValidations(cargoDues);
				AssertAdditionalInformationValidations(cargoDues);
				AssertCargoDuesValidations(cargoDues);
			});
		}

		void AssertCompanyNameValidations(CargoDues cargoDues)
		{
			var shipperMessageError = "Shipper name is mandatory for non-containerized consol when place of receipt is in ZA.";
			var consigneeMessageError = "Consignee name is mandatory for non-containerized consol when place of delivery is in ZA.";

			AssertNoMessageError(cargoDues.Shipper.CompanyNameInfo, shipperMessageError);
			AssertNoMessageError(cargoDues.Consignee.CompanyNameInfo, consigneeMessageError);

			cargoDues.Shipper.CompanyName = "";
			cargoDues.Consignee.CompanyName = "";
			cargoDues.IsContainerised = false;
			cargoDues.PortOfLoading.Code = "Not starting with ZA";
			cargoDues.PortOfDischarge.Code = "Not starting with ZA";
			cargoDues.ValidateAllIncludingChildren();

			AssertNoMessageError(cargoDues.Shipper.CompanyNameInfo, shipperMessageError);
			AssertNoMessageError(cargoDues.Consignee.CompanyNameInfo, consigneeMessageError);

			cargoDues.IsContainerised = true;
			cargoDues.PortOfLoading.Code = "ZAAAM";
			cargoDues.PortOfDischarge.Code = "ZAAAM";
			cargoDues.ValidateAllIncludingChildren();

			AssertNoMessageError(cargoDues.Shipper.CompanyNameInfo, shipperMessageError);
			AssertNoMessageError(cargoDues.Consignee.CompanyNameInfo, consigneeMessageError);

			cargoDues.IsContainerised = false;
			cargoDues.ValidateAllIncludingChildren();

			AssertHasMessageError(cargoDues.Shipper.CompanyNameInfo, shipperMessageError);
			AssertHasMessageError(cargoDues.Consignee.CompanyNameInfo, consigneeMessageError);
		}

		void AssertTNPADataValidations(CargoDues cargoDues)
		{
			AssertNoMessageError(cargoDues.TNPAAccountNumberInfo, "TNPA Account Number is required.");
			AssertNoMessageError(cargoDues.TNPAArrivalNumberInfo, "TNPA Arrival Number is required.");
			AssertNoMessageError(cargoDues.IMONumberInfo, "IMO Number is required.");
			AssertNoMessageError(cargoDues.CarrierCodeInfo, "Carrier Code is required.");
			AssertNoMessageError(cargoDues.EtaInfo, "ETA/ETD is required.");
			AssertNoMessageError(cargoDues.EtdInfo, "ETA/ETD is required.");

			cargoDues.TNPAAccountNumber = ZString.Empty;
			cargoDues.TNPAArrivalNumber = ZString.Empty;
			cargoDues.IMONumber = ZString.Empty;
			cargoDues.CarrierCode = ZString.Empty;
			cargoDues.Eta = ZDateTime.Empty;
			cargoDues.Etd = ZDateTime.Empty;

			AssertHasMessageError(cargoDues.TNPAAccountNumberInfo, "TNPA Account Number is required.");
			AssertHasMessageError(cargoDues.TNPAArrivalNumberInfo, "TNPA Arrival Number is required.");
			AssertHasMessageError(cargoDues.IMONumberInfo, "IMO Number is required.");
			AssertHasMessageError(cargoDues.CarrierCodeInfo, "Carrier Code is required.");
			AssertHasMessageError(cargoDues.EtaInfo, "ETA/ETD is required.");
			AssertHasMessageError(cargoDues.EtdInfo, "ETA/ETD is required.");
		}

		void AssertCountryVesselAndPortValidations(CargoDues cargoDues)
		{
			AssertNoMessageError(cargoDues.PlaceOfReceipt.Country.NameInfo, "Country/Region of Origin is required.");
			AssertNoMessageError(cargoDues.PlaceOfDelivery.Country.NameInfo, "Country/Region of Destination is required.");
			AssertNoMessageError(cargoDues.Transports.Main.VoyageFlightNumberInfo, "Voyage number is required.");
			AssertNoMessageError(cargoDues.Transports.Main.Vessel.NameInfo, "Vessel Name is required.");
			AssertNoMessageError(cargoDues.PortOfLoading.NameInfo, "Port of Loading is required.");
			AssertNoMessageError(cargoDues.PortOfDischarge.NameInfo, "Port of Discharge is required.");
			AssertNoMessageError(cargoDues.ServicePort.NameInfo, "Service Port is required.");

			cargoDues.PlaceOfReceipt.Country.Name = string.Empty;
			cargoDues.PlaceOfDelivery.Country.Name = string.Empty;
			cargoDues.Transports.Main.VoyageFlightNumber = string.Empty;
			cargoDues.Transports.Main.Vessel.Name = string.Empty;
			cargoDues.PortOfDischarge.Name = string.Empty;
			cargoDues.PortOfLoading.Name = string.Empty;
			cargoDues.ServicePort.Name = string.Empty;
			cargoDues.PortOfDischarge.Code = string.Empty;
			cargoDues.PortOfLoading.Code = string.Empty;
			cargoDues.ServicePort.Code = string.Empty;

			AssertHasMessageError(cargoDues.PlaceOfReceipt.Country.NameInfo, "Country/Region of Origin is required.");
			AssertHasMessageError(cargoDues.PlaceOfDelivery.Country.NameInfo, "Country/Region of Destination is required.");
			AssertHasMessageError(cargoDues.Transports.Main.VoyageFlightNumberInfo, "Voyage number is required.");
			AssertHasMessageError(cargoDues.Transports.Main.Vessel.NameInfo, "Vessel Name is required.");
			AssertHasMessageError(cargoDues.PortOfLoading.NameInfo, "Port of Loading is required.");
			AssertHasMessageError(cargoDues.PortOfDischarge.NameInfo, "Port of Discharge is required.");
			AssertHasMessageError(cargoDues.ServicePort.NameInfo, "Service Port is required.");
		}

		void AssertAdditionalInformationValidations(CargoDues cargoDues)
		{
			AssertNoMessageError(cargoDues.ClientRefInfo, "Client Ref. number is required.");
			AssertNoMessageError(cargoDues.WayBillNumberInfo, "Bill of Lading/Mates Receipt is required.");
			AssertNoMessageError(cargoDues.ContainerOperatorInfo, "Container Operator/Vessels Agent is required.");
			AssertNoMessageError(cargoDues.TerminalInfo, "TNPA CTO Code is mandatory for Cargo Dues. Please configure CTO Code from Organization /> Config /> Registration Numbers/Codes as TNP code for country/region ZA.");

			cargoDues.IsQuotationDocument = true;
			cargoDues.WayBillNumber = "";
			cargoDues.Terminal = "";
			cargoDues.ValidateAllIncludingChildren();

			AssertNoMessageError(cargoDues.WayBillNumberInfo, "Bill of Lading/Mates Receipt is required.");
			AssertNoMessageError(cargoDues.TerminalInfo, "TNPA CTO Code is mandatory for Cargo Dues. Please configure CTO Code from Organization /> Config /> Registration Numbers/Codes as TNP code for country/region ZA.");

			cargoDues.IsQuotationDocument = false;
			cargoDues.ClientRef = "";
			cargoDues.ContainerOperator = "";
			cargoDues.WayBillNumber = "";
			cargoDues.Terminal = "";
			cargoDues.ValidateAllIncludingChildren();

			AssertHasMessageError(cargoDues.ClientRefInfo, "Client Ref. number is required.");
			AssertHasMessageError(cargoDues.WayBillNumberInfo, "Bill of Lading/Mates Receipt is required.");
			AssertHasMessageError(cargoDues.ContainerOperatorInfo, "Container Operator/Vessels Agent is required.");
			AssertHasMessageError(cargoDues.TerminalInfo, "TNPA CTO Code is mandatory for Cargo Dues. Please configure CTO Code from Organization /> Config /> Registration Numbers/Codes as TNP code for country/region ZA.");
		}

		void AssertCargoDuesValidations(CargoDues cargoDues)
		{
			AssertHasWarning(cargoDues.CargoDuesSectionTitleInfo, "This section is for information only and is not included in Cargo Dues EDI message.");
		}

		void AssertContainersValidations(CargoDues cargoDues)
		{
			var containerNumberMessage = "Container Number is required.";
			var containerTypeMessage = "Container Type is required.";

			foreach (var container in cargoDues.Containers)
			{
				AssertNoMessageError(container.NumberInfo, containerNumberMessage);
				AssertNoMessageError(container.Type.CodeInfo, containerTypeMessage);

				container.Number = ZString.Empty;
				container.Type.Code = ZString.Empty;

				AssertHasMessageError(container.NumberInfo, containerNumberMessage);
				AssertHasMessageError(container.Type.CodeInfo, containerTypeMessage);
			}

			var placeholderMessage = "Container Number is required.";
			AssertNoMessageError(cargoDues.CargoDuesWarningPlaceHolderInfo, placeholderMessage);

			cargoDues.Containers = new List<Container>();
			cargoDues.ValidateAllIncludingChildren();
			AssertHasMessageError(cargoDues.CargoDuesWarningPlaceHolderInfo, placeholderMessage);
		}

		void AssertGoodsInfoCollectionValidationsIfNotContainerised(CargoDues cargoDues)
		{
			var placeholderMessage = "There are no Shipments registered on this consol.";
			AssertNoMessageError(cargoDues.CargoDuesWarningPlaceHolderInfo, placeholderMessage);

			cargoDues.ShipmentPackingInfos = new List<ShipmentPackingInfo>();
			cargoDues.ValidateAllIncludingChildren();
			AssertHasMessageError(cargoDues.CargoDuesWarningPlaceHolderInfo, placeholderMessage);
		}

		#endregion

		#region TestIsContainerised_ForShippersConsol

		public void TestIsContainerised_ForShippersConsol()
		{
			var documentTitle = "Cargo Dues - Export";
			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = documentTitle,
				LogProvider = CreateLogProviderWithMockLogs(documentTitle)
			};
			var consol = CreateConsolWithMockData(Constants.ContainerModes.ShippersConsol);
			var cargoDues = new CargoDuesBuilder(consol, parameters).Build();

			Assert("IsContainerized", cargoDues.IsContainerised);
		}

		#endregion

		#region Implementation

		void TestBuildCargoDuesCore(string documentTitle, bool isExportDocument, bool isQuotationDocument, string direction)
		{
			var registryCodes = new TNPAAccountNumberCollection();
			var zacpt = registryCodes.AddNew();
			zacpt.Port = "ZACPT";
			zacpt.ImportNumber = "Import001";
			zacpt.ExportNumber = "Export001";
			zacpt.CoastwiseNumber = "Coastwise001";
			var zaaam = registryCodes.AddNew();
			zaaam.Port = "ZAAAM";
			zaaam.ImportNumber = "Import001";
			zaaam.ExportNumber = "Export001";
			zaaam.CoastwiseNumber = "Coastwise001";

			using (PortMessagingRegistry.Instance.TNPAAccountNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				var parameters = new DummyDocDataObjectParameters()
				{
					DocumentTitle = documentTitle,
					LogProvider = CreateLogProviderWithMockLogs(documentTitle)
				};
				var consol = CreateConsolWithMockData(Constants.ContainerModes.Bulk);
				var cargoDues = new CargoDuesBuilder(consol, parameters).Build();

				AssertDocumentPropertiesCorrectlyPopulated(cargoDues, isExportDocument, isQuotationDocument, direction);
				AssertAddressCorrectlyPopulated(cargoDues, consol, isExportDocument);
				AssertTNPAInformationCorrectlyPopulated(cargoDues, isExportDocument);
				AssertCountryVesselAndPortInformationCorrectlyPopulated(cargoDues, isExportDocument);
				AssertAdditionalInformationCorrectlyPopulated(cargoDues);
				AssertCargoDuesCorrectlyPopulated(cargoDues);

				TestValidations(cargoDues);
			}
		}

		void TestGoodsInfoCollectionCore(ZString consolMode, bool isContainerised, string expectedPackageDescription = "")
		{
			var documentTitle = "Cargo Dues - Export";
			var parameters = new DummyDocDataObjectParameters()
			{
				DocumentTitle = documentTitle,
				LogProvider = CreateLogProviderWithMockLogs(documentTitle)
			};
			var consol = CreateConsolWithMockData(consolMode);
			var cargoDues = new CargoDuesBuilder(consol, parameters).Build();

			AssertEquals("IsContainerized", isContainerised, cargoDues.IsContainerised);
			if (isContainerised)
			{
				AssertContainersInformation(cargoDues);
				AssertContainersValidations(cargoDues);
			}
			else
			{
				AssertGoodsInfoCollectionPopulatedFromShipmentInformation(cargoDues, expectedPackageDescription);
				AssertGoodsInfoCollectionValidationsIfNotContainerised(cargoDues);
			}
		}

		ZString GoodsInfoToAssertString(GoodsInfo goodsInfo)
		{
			return $"{goodsInfo.MarksAndNos}|{goodsInfo.NumberOfPacks}|{goodsInfo.PackType}|{goodsInfo.GoodsDescription}|{goodsInfo.GrossMass}";
		}

		ZString ContainerToAssertString(Container container)
		{
			var containerDes = container.IsEmpty ? "Empty" : "Full";
			return $@"{container.Number}|{container.PackCount}|{container.Type.Code}|{containerDes}|{container.GrossWeight.Value} {container.GrossWeight.Unit.Code}";
		}

		ForwardingConsol CreateConsolWithMockData(ZString consolMode)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_MasterBillNum = "MasterB123";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "GBFXT";

			var sendingForwarderAddress = Factory.New<OrgHeader>();
			sendingForwarderAddress.OH_FullName = "Sending Forwarder Address";
			sendingForwarderAddress.MainAddress.Address1 = "Unit 001";
			sendingForwarderAddress.MainAddress.Address2 = "Who";
			sendingForwarderAddress.MainAddress.City = "LDN";
			sendingForwarderAddress.MainAddress.Postcode = "2019";
			sendingForwarderAddress.MainAddress.OA_RN_NKCountryCode = "GB";
			consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.MainAddress.PK;

			var receivingForwarderAddress = Factory.New<OrgHeader>();
			receivingForwarderAddress.OH_FullName = "Receiving Forwarder Address";
			receivingForwarderAddress.MainAddress.Address1 = "Unit 002";
			receivingForwarderAddress.MainAddress.Address2 = "Am";
			receivingForwarderAddress.MainAddress.City = "RDJ";
			receivingForwarderAddress.MainAddress.Postcode = "2017";
			receivingForwarderAddress.MainAddress.OA_RN_NKCountryCode = "BR";
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderAddress.MainAddress.PK;

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Handling the Stuff to be send";
			departureCTOAddress.OH_RL_NKClosestPort = "BEANR";
			departureCTOAddress.MainAddress.Address1 = "Unit 200";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "Antwerp";
			departureCTOAddress.MainAddress.Postcode = "2000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var arrivalCTOAddress = Factory.New<OrgHeader>();
			arrivalCTOAddress.OH_FullName = "I'm Handling the Stuff to be received";
			arrivalCTOAddress.OH_RL_NKClosestPort = "BEANR";
			arrivalCTOAddress.MainAddress.Address1 = "Unit 200";
			arrivalCTOAddress.MainAddress.Address2 = "55 Why Lane";
			arrivalCTOAddress.MainAddress.City = "Antwerp";
			arrivalCTOAddress.MainAddress.Postcode = "2000";
			arrivalCTOAddress.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.MainAddress.PK;

			var transport1 = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "CNSHA";
			transport1.JW_RL_NKDiscPort = "ZAAAM";
			transport1.JW_Vessel = "Test Vessel Name 1";
			transport1.JW_VoyageFlight = "CC123";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "ZAAAM";
			transport2.JW_RL_NKDiscPort = "ZACPT";
			transport2.JW_ETD = new ZDateTime(2020, 02, 02);
			transport2.JW_ETA = new ZDateTime(2020, 02, 20);
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Test Vessel Name 2";
			vessel.RV_LloydsNumber = "Lllloyd";
			vessel.RV_RadioCallSign = "Radio123";
			transport2.JW_Vessel = vessel.RV_Code;
			transport2.JW_VoyageFlight = "CC456";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "Random Vesel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel2.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_DepartReference = "AOEUI1234";
			sailing.Destination.JB_ArrivalReference = "QWERTY123";
			transport2.JW_JX = sailing.PK;

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport3.JW_RL_NKLoadPort = "ZACPT";
			transport3.JW_RL_NKDiscPort = "GBFXT";
			transport3.JW_Vessel = "Test Vessel Name 3";
			transport3.JW_VoyageFlight = "CC789";

			var shippingLineAddress = Factory.New<OrgHeader>();
			shippingLineAddress.OH_FullName = "Carrier";
			shippingLineAddress.MainAddress.Address1 = "Unit 000";
			shippingLineAddress.MainAddress.Address2 = "Hypocrea astronidii";
			shippingLineAddress.MainAddress.City = "Mel";
			shippingLineAddress.MainAddress.Postcode = "2019";
			shippingLineAddress.MainAddress.OA_RN_NKCountryCode = "ZA";
			shippingLineAddress.CustomsCodes.AddNew("CCC", "SUDU", "ZA");
			consol.JK_OA_ShippingLineAddress = shippingLineAddress.MainAddress.PK;

			var terminalAddress = Factory.New<OrgHeader>();
			terminalAddress.OH_FullName = "Carrier";
			terminalAddress.MainAddress.Address1 = "Unit 000";
			terminalAddress.MainAddress.Address2 = "Hypocrea astronidii";
			terminalAddress.MainAddress.City = "Mel";
			terminalAddress.MainAddress.Postcode = "2019";
			terminalAddress.MainAddress.OA_RN_NKCountryCode = "ZA";
			terminalAddress.CustomsCodes.AddNew("TNP", "UDUS", "ZA");
			transport2.JW_OA_DepartureLocation = terminalAddress.MainAddress.PK;
			transport2.JW_OA_ArrivalLocation = terminalAddress.MainAddress.PK;

			var consigneeAddress = Factory.New<OrgHeader>();
			consigneeAddress.OH_FullName = "Consignee";
			consigneeAddress.MainAddress.Address1 = "2222 Uptown Blvd";
			consigneeAddress.MainAddress.City = "Albuquerque";
			consigneeAddress.MainAddress.Postcode = "87110";
			consigneeAddress.MainAddress.OA_RN_NKCountryCode = "US";

			var consignorAddress = Factory.New<OrgHeader>();
			consignorAddress.OH_FullName = "Consignor";
			consignorAddress.MainAddress.Address1 = "test";
			consignorAddress.MainAddress.City = "test";
			consignorAddress.MainAddress.Postcode = "1000";
			consignorAddress.MainAddress.OA_RN_NKCountryCode = "AU";

			var shipment1 = consol.TopLevelShipments.AddNew();
			shipment1.JS_GoodsDescription = "shipment goods 1";
			shipment1.ConsigneePK = consigneeAddress.PK;
			shipment1.ConsignorPK = consignorAddress.PK;

			var shipment2 = consol.TopLevelShipments.AddNew();
			shipment2.Notes.AddNew(false, "Detailed Goods Description", "shipmentDescription2");
			shipment2.Notes.AddNew(false, "Marks & Numbers", "shipmentMark");
			shipment2.JS_GoodsDescription = "shipmentDescription3";

			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();
			refContainer1.RC_Code = "10GP";
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "Container1";
			container1.JC_RC = refContainer1.PK;
			container1.JC_IsEmptyContainer = true;

			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refContainer2.RC_Code = "20GP";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "Container2";
			container2.JC_RC = refContainer2.PK;
			container2.JC_IsEmptyContainer = false;

			var packLine1 = shipment1.OuterPackLines.OfType<ForwardingPackLine>().Single();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(packLine1);
			packLine1.JL_PackageCount = 10;
			packLine1.JL_ActualWeight = 10;
			packLine1.JL_ActualWeightUQ = "KG";
			packLine1.JL_MarksAndNumbers = "packLineMarksAndNos1";
			packLine1.JL_F3_NKPackType = "PT1";

			var packLine2 = shipment2.OuterPackLines.OfType<ForwardingPackLine>().Single();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			container2.PackLines.Add(packLine2);
			packLine2.JL_PackageCount = 20;
			packLine2.JL_ActualWeight = 20;
			packLine2.JL_ActualWeightUQ = "KG";
			packLine2.JL_MarksAndNumbers = "packLineMarksAndNos2";
			packLine2.JL_F3_NKPackType = "PT2";
			packLine2.JL_Description = "packlineDes2";

			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.JL_JS = shipment2.PK;
			packLine3.JL_FreightMode = FreightConstants.OuterPackType;
			container2.PackLines.Add(packLine3);
			packLine3.JL_PackageCount = 30;
			packLine3.JL_ActualWeight = 30;
			packLine3.JL_ActualWeightUQ = "KG";
			packLine3.JL_F3_NKPackType = "PT3";
			shipment2.JS_GoodsDescription = "";

			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_FreightMode = FreightConstants.OuterPackType;
			packLine4.JL_PackageCount = 40;
			packLine4.JL_ActualWeight = 40;
			packLine4.JL_ActualWeightUQ = "KG";
			packLine4.JL_MarksAndNumbers = "packLineMarksAndNos4";
			packLine4.JL_F3_NKPackType = "PT4";
			packLine4.JL_DetailedDescription = "packlineDetailedDes4";

			consol.JK_ConsolMode = consolMode;

			return consol;
		}

		IStmALogProvider CreateLogProviderWithMockLogs(string documentTitle)
		{
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>("DEP", "TNPA"),
				new KeyValuePair<string, string>("MST", documentTitle),
				new KeyValuePair<string, string>("RFN", "QWERTY123"));
			return logProvider;
		}

		void CreateLog(IStmALogProvider logProvider, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			logProvider.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), "", parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		void AddDataLinkedEvent(ForwardingConsol consol, string totalCargoDuesAmount, string documentName)
		{
			var cargoDuesDocumentEventXML = GetCargoDuesDocumentEventXML(consol.JK_UniqueConsignRef, totalCargoDuesAmount, documentName);
			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					documentName)
			};

			consol.Logs.CreateOrRecreateEventLog(
				Events.MessageAccepted,
				EstimateActual.Actual,
				ZDateTimeOffset.Now.AddDays(-1),
				ZString.Empty,
				eventParameters);

			var dataLinkedLog = consol.Logs.Find(log => log.SL_SE_NKEvent == Events.MessageAccepted.Code).First();

			var message = Factory.New<IXmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.Content = XElement.Parse(cargoDuesDocumentEventXML);

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = dataLinkedLog.PK;
			pivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
			pivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
		}

		string GetCargoDuesDocumentEventXML(string consolKey, string totalCargoDuesAmount, string documentName) => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DocumentaryOverride>
				<DocumentName>Cargo Dues - Import</DocumentName>
			</DocumentaryOverride>
			<DataTargetCollection>
				<DataTarget>
					<Key>{consolKey}</Key>
					<Type>ForwardingConsol</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2020-12-16T16:39:00</EventTime>
		<EventType>MAA</EventType>
		<EventParameters>
			<Department>TNPA</Department>
			<MessageType>{documentName}</MessageType>
			<ReferenceNumber>3707720274</ReferenceNumber>
		</EventParameters>
		<EventReference>Order Confirmed</EventReference>
		<ContextCollection>
			<Context>
				<Type>TNPA Order Number</Type>
				<Value>3707720274</Value>
			</Context>
			<Context>
				<Type>Total Cargo Dues Amount</Type>
				<Value>{totalCargoDuesAmount}</Value>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>1</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Description</Type>
						<Value>Desc1</Value>
					</SubContext>
					<SubContext>
						<Type>Amount</Type>
						<Value>1111</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>2</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Description</Type>
						<Value></Value>
					</SubContext>
					<SubContext>
						<Type>Amount</Type>
						<Value></Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>3</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Description</Type>
					</SubContext>
					<SubContext>
						<Type>Amount</Type>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>4</Value>
				<SubContextCollection>
					<SubContext>
						<Type>Amount</Type>
						<Value>abc</Value>
					</SubContext>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>5</Value>
				<SubContextCollection>
				</SubContextCollection>
			</Context>
			<Context>
				<Type>ChargeLine</Type>
				<Value>6</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion
	}
}
