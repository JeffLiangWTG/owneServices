using System;
using System.Linq;
using System.Reactive;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Vessel = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Vessel;

namespace Enterprise.Freight.Forwarding.Documents.DE.Testing
{
	sealed class AdvancedLogisticsPortOrderBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var consol = CreateConsol();
			CreateExportTransport(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertNotNull(advancedLogisticsPortOrder);
		}

		public void TestPopulateHeaderInfoExport()
		{
			var consol = CreateConsol();
			CreateExportTransport(consol);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertNotNull(advancedLogisticsPortOrder);
			AssertEquals("ConsolNumber", "C00002000", advancedLogisticsPortOrder.ConsolNumber);
			AssertEquals("BOL_Reference", consol.JK_MasterBillNum, advancedLogisticsPortOrder.BillOfLading);
			AssertEquals("BookingReference", consol.JK_BookingReference, advancedLogisticsPortOrder.CarrierBookingReference);
			AssertEquals("OperationalPort", "DEBRE", advancedLogisticsPortOrder.OperationalPort.Code);
			AssertEquals("Direction", Constants.FreightShipmentDirection.Description.Export, advancedLogisticsPortOrder.Direction);
			AssertEquals("ContainerMode", consol.JK_ConsolMode, advancedLogisticsPortOrder.ContainerMode.Code);
			AssertEquals("FreightForwarderReference", consol.JK_UniqueConsignRef, advancedLogisticsPortOrder.FreightForwarderReference);
			AssertEquals("PortOfLoading", "DEBRE", advancedLogisticsPortOrder.PortOfLoading.Code);
			AssertEquals("PortOfDischarge", "CNYTN", advancedLogisticsPortOrder.PortOfDischarge.Code);
			AssertEquals("PortOfOrigin", "DEBRE", advancedLogisticsPortOrder.PortOfOrigin.Code);
			AssertEquals("PortOfDestination", "CNYTN", advancedLogisticsPortOrder.PortOfDestination.Code);
			AssertEquals("VesselName", "COSCO NEBULA", advancedLogisticsPortOrder.Vessel.Name);
			AssertEquals("VoyageFlightNo", "85475", advancedLogisticsPortOrder.VoyageFlightNo);
			AssertEquals("LloydsImo", "9795622", advancedLogisticsPortOrder.Vessel.LloydsIMO);
			AssertEquals("ETD should be set", new ZDateTime(2021, 01, 24, 8, 45, 00), advancedLogisticsPortOrder.ETD);
			AssertEquals("ETA should be set", new ZDateTime(2021, 01, 26, 12, 15, 00), advancedLogisticsPortOrder.ETA);
			AssertEquals("MarksAndNumbers", "", advancedLogisticsPortOrder.MarksAndNumbers);
			AssertEquals("TransportsModePreCarriage", "ROA", advancedLogisticsPortOrder.TransportModePreCarriageOrOnForwarding.Code);
			AssertEquals("PreCarriageID", "", advancedLogisticsPortOrder.PreCarriageOrOnForwardingID);

			consol.Transports[0].JW_RL_NKLoadPort = "";
			consol.Transports[0].JW_RL_NKDiscPort = "";
			advancedLogisticsPortOrder = builder.Build();
			AssertEquals("PortOfLoading", "FRPAR", advancedLogisticsPortOrder.PortOfLoading.Code);
			AssertEquals("PortOfDischarge", "BEANR", advancedLogisticsPortOrder.PortOfDischarge.Code);

			AddExtraTransport(consol, Constants.TransportPlanningType.PreCarriage, "RAI", "DEAAH", "DEBRV", legOrder: 1);
			AddMainTransport(consol, "DEBRV", "NLRTM", legOrder: 2);
			advancedLogisticsPortOrder = builder.Build();
			AssertEquals("OperationalPort", "DEBRV", advancedLogisticsPortOrder.OperationalPort.Code);
			AssertEquals("PortOfLoading", "DEBRV", advancedLogisticsPortOrder.PortOfLoading.Code);
			AssertEquals("PortOfDischarge", "NLRTM", advancedLogisticsPortOrder.PortOfDischarge.Code);
			AssertEquals("VesselName", "MSC UBERTY", advancedLogisticsPortOrder.Vessel.Name);
			AssertEquals("VoyageFlightNo", "12401", advancedLogisticsPortOrder.VoyageFlightNo);
			AssertEquals("LloydsImo", "489541", advancedLogisticsPortOrder.Vessel.LloydsIMO);
			AssertEquals("ETD should be set", new ZDateTime(2021, 01, 23, 7, 35, 00), advancedLogisticsPortOrder.ETD);
			AssertEquals("ETA should be set", new ZDateTime(2021, 01, 25, 15, 55, 00), advancedLogisticsPortOrder.ETA);
			AssertEquals("TransportsModePreCarriage", "RAI", advancedLogisticsPortOrder.TransportModePreCarriageOrOnForwarding.Code);
			AssertEquals("PreCarriageID", "COUCH124", advancedLogisticsPortOrder.PreCarriageOrOnForwardingID);

			AssertAddressData(consol.SendingForwarderAddress, advancedLogisticsPortOrder.Forwarder);
			AssertAddressData(consol.DepartureCTOAddress, advancedLogisticsPortOrder.CTO);
			AssertPopulateWarehouseCode(consol, consol.DepartureCTOAddress, "CTOWarehouseCode");
		}

		public void TestPopulateHeaderInfoImport()
		{
			var consol = CreateConsol();
			CreateTransport(consol, "BEANR", "NLRTM", legOrder: 1);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertNotNull(advancedLogisticsPortOrder);
			AssertEquals("PortOfOrigin", "FRPAR", advancedLogisticsPortOrder.PortOfOrigin.Code);
			AssertEquals("PortOfDestination", "BEANR", advancedLogisticsPortOrder.PortOfDestination.Code);
			AssertEquals("PortOfLoading", "FRPAR", advancedLogisticsPortOrder.PortOfLoading.Code);
			AssertEquals("PortOfDischarge", "BEANR", advancedLogisticsPortOrder.PortOfDischarge.Code);

			AssertEquals("ConsolNumber", "C00002000", advancedLogisticsPortOrder.ConsolNumber);
			AssertEquals("Direction", Constants.FreightShipmentDirection.Description.Import, advancedLogisticsPortOrder.Direction);
			AssertEquals("OperationalPort", "", advancedLogisticsPortOrder.OperationalPort.Code);

			AddMainTransport(consol, "BEZEE", "DEBRE", legOrder: 2);
			AddExtraTransport(consol, Constants.TransportPlanningType.Other, "RAI", "DEAAH", "DEBRV", legOrder: 3);
			advancedLogisticsPortOrder = builder.Build();
			AssertEquals("OperationalPort", "DEBRE", advancedLogisticsPortOrder.OperationalPort.Code);
			AssertEquals("PortOfOrigin", "BEZEE", advancedLogisticsPortOrder.PortOfOrigin.Code);
			AssertEquals("PortOfDestination", "DEBRE", advancedLogisticsPortOrder.PortOfDestination.Code);
			AssertEquals("PortOfLoading", "BEZEE", advancedLogisticsPortOrder.PortOfLoading.Code);
			AssertEquals("PortOfDischarge", "DEBRE", advancedLogisticsPortOrder.PortOfDischarge.Code);
			AssertEquals("TransportsModeOnForwarding", Core.Constants.TransportModes.Rail, advancedLogisticsPortOrder.TransportModePreCarriageOrOnForwarding.Code);
			AssertEquals("OnForwardingID", "COUCH124", advancedLogisticsPortOrder.PreCarriageOrOnForwardingID);

			AssertAddressData(consol.ReceivingForwarderAddress, advancedLogisticsPortOrder.Forwarder);
			AssertAddressData(consol.ArrivalCTOAddress, advancedLogisticsPortOrder.CTO);
			AssertPopulateWarehouseCode(consol, consol.ArrivalCTOAddress, "CTOWarehouseCode");
		}

		public void TestSISNumberDefault()
		{
			#region Departure

			var consol = CreateConsol("DEBRE", "AUSYD");
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "DEBRE";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_DeparturePortRouteId = "Test Depart Port";

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertEquals("Test Depart Port", advancedLogisticsPortOrder.SisNumber);

			#endregion

			#region Arrival

			consol = CreateConsol("AUSYD", "DEBRE");
			transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "DEBRE";
			transport.JW_ArrivalPortRouteId = "Test Arrival Port";

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();

			AssertEquals("Test Arrival Port", advancedLogisticsPortOrder.SisNumber);

			#endregion
		}

		public void TestValidateHeaderInformation()
		{
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			GenRegCertAccredMaintList certificate = currentStaff.Certificates.AddNew();
			certificate.XZ_Type = Constants.StaffDefaultCertificateIDAndTrainingTypes.DBH;
			certificate.XZ_RefNumber = "DBHLicNumber";

			Factory.Save();

			var exportClientNo = "TestZKV";
			FreightDataRegistry.Instance.ALPOExportClientNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exportClientNo);

			var consol = CreateConsol();
			CreateExportTransport(consol);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			var operationalPortErrorMessage = "Operational port can only be 'DEBRE', 'DEBRV', 'DECUX', 'DEWVN' or 'DEHAM'.";
			var portOfLoadingErrorMessage = "Port of Loading is required.";
			var portOfDischargeErrorMessage = "Port of Discharge is required.";
			var vesselNameErrorMessage = "Vessel Name is required.";
			var transportETDErrorMessage = "ETD is required.";
			var preCarriageErrorMessage = "Pre-Carriage ID is required.";
			var alpoUserIdErrorMessage = "ALPO User Id is required. Specify DBH user code of logged on user (Human Resources > Certificates and ID Numbers tab in Staff and Resources)";

			AssertNoMessageError("Operational Port should not have error message", ((Unloco)advancedLogisticsPortOrder.OperationalPort).CodeInfo, operationalPortErrorMessage);
			AssertNoMessageError("Port Of Loading should not have error message", ((Unloco)advancedLogisticsPortOrder.PortOfLoading).CodeInfo, portOfLoadingErrorMessage);
			AssertNoMessageError("Port Of Discharge should not have error message", ((Unloco)advancedLogisticsPortOrder.PortOfDischarge).CodeInfo, portOfDischargeErrorMessage);
			AssertNoMessageError("Vessel Name should not have error message", ((Vessel)advancedLogisticsPortOrder.Vessel).NameInfo, vesselNameErrorMessage);
			AssertNoMessageError("ETD should not have error message", advancedLogisticsPortOrder.ETDInfo, transportETDErrorMessage);
			AssertNoMessageError("Alpo user id should not have error message", advancedLogisticsPortOrder.ALPOUserIDInfo, alpoUserIdErrorMessage);

			advancedLogisticsPortOrder.OperationalPort.Code = "";
			advancedLogisticsPortOrder.PortOfLoading.Code = "";
			advancedLogisticsPortOrder.PortOfDischarge.Code = "";
			advancedLogisticsPortOrder.Vessel.Name = "";
			advancedLogisticsPortOrder.ETD = ZDateTime.Empty;
			advancedLogisticsPortOrder.ALPOUserID = "";

			advancedLogisticsPortOrder.ValidateAllIncludingChildren();

			AssertHasMessageError("Operational Port should have error message", ((Unloco)advancedLogisticsPortOrder.OperationalPort).CodeInfo, operationalPortErrorMessage);
			AssertHasMessageError("Port Of Loading should have error message", ((Unloco)advancedLogisticsPortOrder.PortOfLoading).CodeInfo, portOfLoadingErrorMessage);
			AssertHasMessageError("Port Of Discharge should have error message", ((Unloco)advancedLogisticsPortOrder.PortOfDischarge).CodeInfo, portOfDischargeErrorMessage);
			AssertHasMessageError("Vessel Name should have error message", ((Vessel)advancedLogisticsPortOrder.Vessel).NameInfo, vesselNameErrorMessage);
			AssertHasMessageError("ETD should have error message", advancedLogisticsPortOrder.ETDInfo, transportETDErrorMessage);
			AssertHasMessageError("Alpo user id should have error message", advancedLogisticsPortOrder.ALPOUserIDInfo, alpoUserIdErrorMessage);

			advancedLogisticsPortOrder.SisNumber = "N001";
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();

			AssertHasMessageError("Operational Port should have error message", ((Unloco)advancedLogisticsPortOrder.OperationalPort).CodeInfo, operationalPortErrorMessage);
			AssertNoMessageError("Port Of Loading should not have error message", ((Unloco)advancedLogisticsPortOrder.PortOfLoading).CodeInfo, portOfLoadingErrorMessage);
			AssertNoMessageError("Port Of Discharge should not have error message", ((Unloco)advancedLogisticsPortOrder.PortOfDischarge).CodeInfo, portOfDischargeErrorMessage);
			AssertHasMessageError("Vessel Name should have error message", ((Vessel)advancedLogisticsPortOrder.Vessel).NameInfo, vesselNameErrorMessage);
			AssertNoMessageError("ETD should not have error message", advancedLogisticsPortOrder.ETDInfo, transportETDErrorMessage);

			consol.Transports[0].JW_RL_NKLoadPort = "";
			consol.Transports[0].JW_RL_NKDiscPort = "";
			var preCarriage = AddExtraTransport(consol, Constants.TransportPlanningType.PreCarriage, "RAI", "DEAAH", "DEBRV", legOrder: 1);
			AddMainTransport(consol, "DEBRV", "NLRTM", legOrder: 2);

			preCarriage.JW_VoyageFlight = ZString.Empty;
			preCarriage.JW_TransportMode = Constants.TransportModes.Sea;
			advancedLogisticsPortOrder = builder.Build();
			AssertHasMessageError("Pre-Carriage ID should have error message", advancedLogisticsPortOrder.PreCarriageOrOnForwardingIDInfo, preCarriageErrorMessage);

			preCarriage.JW_TransportMode = Constants.TransportModes.Air;
			advancedLogisticsPortOrder = builder.Build();
			AssertNoMessageError("Pre-Carriage ID should have error message", advancedLogisticsPortOrder.PreCarriageOrOnForwardingIDInfo, preCarriageErrorMessage);

			preCarriage.JW_TransportMode = Constants.TransportModes.Road;
			advancedLogisticsPortOrder = builder.Build();
			AssertNoMessageError("Pre-Carriage ID should have error message", advancedLogisticsPortOrder.PreCarriageOrOnForwardingIDInfo, preCarriageErrorMessage);

			preCarriage.JW_TransportMode = Constants.TransportModes.Rail;
			advancedLogisticsPortOrder = builder.Build();
			AssertHasMessageError("Pre-Carriage ID should have error message", advancedLogisticsPortOrder.PreCarriageOrOnForwardingIDInfo, preCarriageErrorMessage);

			preCarriage.JW_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			advancedLogisticsPortOrder = builder.Build();
			AssertHasMessageError("Pre-Carriage ID should have error message", advancedLogisticsPortOrder.PreCarriageOrOnForwardingIDInfo, preCarriageErrorMessage);

			preCarriage.JW_Vessel = "MSC POOLSTER";
			advancedLogisticsPortOrder = builder.Build();
			AssertNoMessageError("Pre-Carriage ID should have error message", advancedLogisticsPortOrder.PreCarriageOrOnForwardingIDInfo, preCarriageErrorMessage);
		}

		public void TestPopulatePackingLineDocumentNoLabel()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.OuterPackLines.AddNew();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);

			AssertEquals("Document Number", packLine.DocumentNoLabel);

			packLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AES;
			AssertEquals("MRN", packLine.DocumentNoLabel);

			packLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AE1;
			AssertEquals("LRN", packLine.DocumentNoLabel);

			packLine.EntryType.Code = EntryTypes.Codes.EntryTypes_1000N;
			AssertEquals("Document Number", packLine.DocumentNoLabel);
		}

		public void TestPopulatePackingLineReferenceNumber()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();

			var packlineBO = shipmentBO.OuterPackLines.AddNew();
			packlineBO.JL_ExportRefNumber = "ref number1";

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			AssertEquals("ref number1", packLine.ExportReferenceNumber);

			packlineBO.JL_ExportRefNumber = ZString.Empty;
			shipmentBO.CustomsEntryNumber = "ref number2";
			advancedLogisticsPortOrder = builder.Build();
			packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			AssertEquals("ref number2", packLine.ExportReferenceNumber);
		}

		public void TestPopulatePackingLineEntryType()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_GoodsValue = 1001;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.JS_ActualWeight = 1001;

			var packlineBO = shipmentBO.OuterPackLines.AddNew();
			packlineBO.JL_ExportRefNumber = "ref number1";

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(1);
			AssertEquals(EntryTypes.Codes.EntryTypes_AES, packLine.EntryType.Code);
			AssertEquals(nameof(EntryTypeStatus.DefaultToAES), packLine.EntryTypeStatus);
			AssertEquals(2, ((ICodeDescriptionPairList)((CodeDescription)packLine.EntryType).Codes).Count);

			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipmentBO.CustomsEntryNumber = "CEN001";
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;
			advancedLogisticsPortOrder = builder.Build();
			packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(1);
			AssertEquals(EntryTypes.Codes.EntryTypes_AES, packLine.EntryType.Code);
			AssertEquals(nameof(EntryTypeStatus.ReadOnly_DefaultToAES), packLine.EntryTypeStatus);

			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			shipmentBO.CustomsEntryNumber = "CEN001";
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;
			advancedLogisticsPortOrder = builder.Build();
			packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(1);
			AssertEquals(EntryTypes.Codes.EntryTypes_AE1, packLine.EntryType.Code);
			AssertEquals(nameof(EntryTypeStatus.ReadOnly_DefaultToAE1), packLine.EntryTypeStatus);

			consolBO.JK_RL_NKLoadPort = ZString.Empty;
			consolBO.JK_RL_NKDischargePort = "DEWVN";
			parameters.DataStoreName = "AdvancedLogisticsPortOrder_IMP";
			advancedLogisticsPortOrder = builder.Build();
			packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(1);
			AssertEquals(EntryTypes.Codes.EntryTypes_NA, packLine.EntryType.Code);
			AssertEquals(nameof(EntryTypeStatus.ReadOnly_DefaultToNA), packLine.EntryTypeStatus);
		}

		public void TestPopulatePackingLineCompleteAndShortage_VisibleAndDefaultValues()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipmentBO.CustomsEntryNumber = "CEN001";
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;

			shipmentBO.OuterPackLines.RemoveAndDeleteAll();

			var packlineBO = shipmentBO.OuterPackLines.AddNew();
			packlineBO.JL_ExportRefNumber = "ref number1";
			packlineBO.JL_ItemNo = 0;

			var packlineBO2 = shipmentBO.OuterPackLines.AddNew();
			packlineBO2.JL_ExportRefNumber = "ref number1";
			packlineBO2.JL_ItemNo = 0;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var firstPackLine = advancedLogisticsPortOrder.Shipments.First().PackingLines.First(x => x.PackageNumber == "1");
			var secondPackLine = advancedLogisticsPortOrder.Shipments.First().PackingLines.First(x => x.PackageNumber == "2");

			Assert(!firstPackLine.Complete);
			AssertEquals(firstPackLine.CargoItem, "0");
			Assert(firstPackLine.CargoItemInfo.Notifications.FirstOrDefault().Message == "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			Assert(firstPackLine.IsVisible_CargoItem);
			Assert(firstPackLine.IsVisible_PackageNumber);
			Assert(secondPackLine.Complete);
			Assert(!secondPackLine.Shortage);
			AssertEquals(secondPackLine.CargoItem, "0");
			Assert(secondPackLine.CargoItemInfo.Notifications.FirstOrDefault().Message == "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			Assert(secondPackLine.IsVisible_Complete);
			Assert(secondPackLine.IsVisible_Shortage);
			Assert(secondPackLine.IsVisible_CargoItem);
			Assert(secondPackLine.IsVisible_PackageNumber);

			firstPackLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AE1;
			secondPackLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AE1;
			firstPackLine.CargoItem = "1";
			secondPackLine.CargoItem = "1";
			Assert(!firstPackLine.Complete);
			Assert(!firstPackLine.Shortage);
			Assert(!firstPackLine.CargoItemInfo.Notifications.Any());
			Assert(secondPackLine.Complete);
			Assert(!secondPackLine.Shortage);
			Assert(!firstPackLine.CargoItemInfo.Notifications.Any());
			Assert(secondPackLine.IsVisible_Complete);
			Assert(secondPackLine.IsVisible_Shortage);
			Assert(firstPackLine.IsVisible_Complete);
			Assert(firstPackLine.IsVisible_Shortage);
			Assert(secondPackLine.IsVisible_CargoItem);
			Assert(secondPackLine.IsVisible_PackageNumber);
			Assert(firstPackLine.IsVisible_CargoItem);
			Assert(firstPackLine.IsVisible_PackageNumber);

			firstPackLine.EntryType.Code = EntryTypes.Codes.EntryTypes_7777N;
			secondPackLine.EntryType.Code = EntryTypes.Codes.EntryTypes_7777N;
			Assert(!firstPackLine.Complete);
			Assert(!firstPackLine.Shortage);
			Assert(!firstPackLine.IsVisible_Complete);
			Assert(!firstPackLine.IsVisible_Shortage);
			Assert(!firstPackLine.IsVisible_CargoItem);
			Assert(!firstPackLine.IsVisible_PackageNumber);
			Assert(secondPackLine.Complete);
			Assert(!secondPackLine.Shortage);
			Assert(!secondPackLine.IsVisible_Complete);
			Assert(!secondPackLine.IsVisible_Shortage);
			Assert(!secondPackLine.IsVisible_CargoItem);
			Assert(!secondPackLine.IsVisible_PackageNumber);

			firstPackLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AES;
			secondPackLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AES;
			Assert(!firstPackLine.Complete);
			Assert(!firstPackLine.Shortage);
			Assert(firstPackLine.IsVisible_Complete);
			Assert(firstPackLine.IsVisible_Shortage);
			Assert(firstPackLine.IsVisible_CargoItem);
			Assert(firstPackLine.IsVisible_PackageNumber);
			Assert(secondPackLine.Complete);
			Assert(!secondPackLine.Shortage);
			Assert(secondPackLine.IsVisible_Complete);
			Assert(secondPackLine.IsVisible_Shortage);
			Assert(secondPackLine.IsVisible_CargoItem);
			Assert(secondPackLine.IsVisible_PackageNumber);
		}

		public void TestPopulatePackingLineCompleteAndShortage_EnableDisable_Validations()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipmentBO.CustomsEntryNumber = "CEN001";
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;

			shipmentBO.OuterPackLines.RemoveAndDeleteAll();

			var packlineBO = shipmentBO.OuterPackLines.AddNew();
			packlineBO.JL_ExportRefNumber = "ref number1";
			packlineBO.JL_ItemNo = 0;

			var packlineBO2 = shipmentBO.OuterPackLines.AddNew();
			packlineBO2.JL_ExportRefNumber = "ref number1";
			packlineBO2.JL_ItemNo = 0;

			var packlineBO3 = shipmentBO.OuterPackLines.AddNew();
			packlineBO3.JL_ExportRefNumber = "ref number1";
			packlineBO3.JL_ItemNo = 0;

			var packlineBO4 = shipmentBO.OuterPackLines.AddNew();
			packlineBO4.JL_ExportRefNumber = "ref number1";
			packlineBO4.JL_ItemNo = 0;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var firstPackLine = advancedLogisticsPortOrder.Shipments.First().PackingLines.First(x => x.PackageNumber == "1");
			var secondPackLine = advancedLogisticsPortOrder.Shipments.First().PackingLines.First(x => x.PackageNumber == "2");
			var thirdPackLine = advancedLogisticsPortOrder.Shipments.First().PackingLines.First(x => x.PackageNumber == "3");
			var fourthPackLine = advancedLogisticsPortOrder.Shipments.First().PackingLines.First(x => x.PackageNumber == "4");

			firstPackLine.CargoItem = "1";
			secondPackLine.CargoItem = "1";
			thirdPackLine.CargoItem = "1";
			fourthPackLine.CargoItem = "1";
			Assert(!secondPackLine.Complete);
			Assert(secondPackLine.CargoItem == "1");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "1");
			Assert(fourthPackLine.PackageNumber == "4");
			Assert(!secondPackLine.Shortage);
			Assert(!fourthPackLine.Shortage);
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);
			AssertNoMessageErrors(secondPackLine.CargoItemInfo);
			AssertNoMessageErrors(fourthPackLine.CargoItemInfo);

			fourthPackLine.Complete = false;
			firstPackLine.CargoItem = "0";
			secondPackLine.CargoItem = "0";
			thirdPackLine.CargoItem = "0";
			fourthPackLine.CargoItem = "0";
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.CargoItem == "0");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(!fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "0");
			Assert(fourthPackLine.PackageNumber == "4");
			Assert(!secondPackLine.Shortage);
			Assert(!fourthPackLine.Shortage);
			AssertHasMessageError(secondPackLine.ShortageInfo, $"At least one packline needs to be marked as complete or have a Shortage in case {secondPackLine.documentNoLabel} is not marked as complete.");
			AssertHasMessageError(fourthPackLine.ShortageInfo, $"At least one packline needs to be marked as complete or have a Shortage in case {secondPackLine.documentNoLabel} is not marked as complete.");
			AssertHasMessageError(firstPackLine.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertHasMessageError(secondPackLine.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertHasMessageError(thirdPackLine.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertHasMessageError(fourthPackLine.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			secondPackLine.Shortage = true;
			firstPackLine.CargoItem = "1";
			secondPackLine.CargoItem = "1";
			thirdPackLine.CargoItem = "1";
			fourthPackLine.CargoItem = "1";
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.CargoItem == "1");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(!fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "1");
			Assert(fourthPackLine.PackageNumber == "4");
			Assert(secondPackLine.Shortage);
			Assert(!fourthPackLine.Shortage);
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);
			AssertNoMessageErrors(firstPackLine.CargoItemInfo);
			AssertNoMessageErrors(secondPackLine.CargoItemInfo);
			AssertNoMessageErrors(thirdPackLine.CargoItemInfo);
			AssertNoMessageErrors(fourthPackLine.CargoItemInfo);

			fourthPackLine.Shortage = true;
			secondPackLine.CargoItem = "1";
			fourthPackLine.CargoItem = "1";
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.CargoItem == "1");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(!fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "1");
			Assert(fourthPackLine.PackageNumber == "4");
			Assert(secondPackLine.Shortage);
			Assert(fourthPackLine.Shortage);
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);
			AssertNoMessageErrors(firstPackLine.CargoItemInfo);
			AssertNoMessageErrors(secondPackLine.CargoItemInfo);
			AssertNoMessageErrors(thirdPackLine.CargoItemInfo);
			AssertNoMessageErrors(fourthPackLine.CargoItemInfo);

			secondPackLine.Shortage = false;
			fourthPackLine.Shortage = false;
			secondPackLine.CargoItem = "1";
			fourthPackLine.CargoItem = "1";
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.CargoItem == "1");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(!fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "1");
			Assert(fourthPackLine.PackageNumber == "4");
			Assert(!secondPackLine.Shortage);
			Assert(!fourthPackLine.Shortage);
			AssertHasMessageError(secondPackLine.ShortageInfo, $"At least one packline needs to be marked as complete or have a Shortage in case {secondPackLine.documentNoLabel} is not marked as complete.");
			AssertHasMessageError(fourthPackLine.ShortageInfo, $"At least one packline needs to be marked as complete or have a Shortage in case {secondPackLine.documentNoLabel} is not marked as complete.");
			AssertNoMessageErrors(firstPackLine.CargoItemInfo);
			AssertNoMessageErrors(secondPackLine.CargoItemInfo);
			AssertNoMessageErrors(thirdPackLine.CargoItemInfo);
			AssertNoMessageErrors(fourthPackLine.CargoItemInfo);

			secondPackLine.Shortage = true;
			fourthPackLine.Shortage = true;
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.CargoItem == "1");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(!fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "1");
			Assert(fourthPackLine.PackageNumber == "4");
			Assert(secondPackLine.Shortage);
			Assert(fourthPackLine.Shortage);
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);
			AssertNoMessageErrors(firstPackLine.CargoItemInfo);
			AssertNoMessageErrors(secondPackLine.CargoItemInfo);
			AssertNoMessageErrors(thirdPackLine.CargoItemInfo);
			AssertNoMessageErrors(fourthPackLine.CargoItemInfo);

			secondPackLine.Shortage = false;
			fourthPackLine.Shortage = false;
			firstPackLine.CargoItem = "1";
			secondPackLine.CargoItem = "1";
			thirdPackLine.CargoItem = "2";
			fourthPackLine.CargoItem = "2";
			firstPackLine.PackageNumber = string.Empty;
			secondPackLine.PackageNumber = string.Empty;
			thirdPackLine.PackageNumber = string.Empty;
			fourthPackLine.PackageNumber = string.Empty;
			secondPackLine.Shortage = true;
			fourthPackLine.Shortage = true;
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.CargoItem == "1");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(!fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "2");
			Assert(fourthPackLine.PackageNumber == "4");
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);
			AssertNoMessageErrors(firstPackLine.CargoItemInfo);
			AssertNoMessageErrors(secondPackLine.CargoItemInfo);
			AssertNoMessageErrors(thirdPackLine.CargoItemInfo);
			AssertNoMessageErrors(fourthPackLine.CargoItemInfo);

			secondPackLine.Shortage = false;
			fourthPackLine.Shortage = false;
			firstPackLine.CargoItem = "1";
			secondPackLine.CargoItem = "1";
			thirdPackLine.CargoItem = "1";
			fourthPackLine.CargoItem = "1";
			firstPackLine.PackageNumber = string.Empty;
			secondPackLine.PackageNumber = string.Empty;
			thirdPackLine.PackageNumber = string.Empty;
			fourthPackLine.PackageNumber = string.Empty;
			secondPackLine.Shortage = true;
			fourthPackLine.Shortage = true;
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.CargoItem == "1");
			Assert(secondPackLine.PackageNumber == "2");
			Assert(!fourthPackLine.Complete);
			Assert(fourthPackLine.CargoItem == "1");
			Assert(fourthPackLine.PackageNumber == "4");
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);
			AssertNoMessageErrors(firstPackLine.CargoItemInfo);
			AssertNoMessageErrors(secondPackLine.CargoItemInfo);
			AssertNoMessageErrors(thirdPackLine.CargoItemInfo);
			AssertNoMessageErrors(fourthPackLine.CargoItemInfo);

			fourthPackLine.Shortage = false;
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(secondPackLine.Shortage);
			Assert(!fourthPackLine.Shortage);
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);

			secondPackLine.Shortage = false;
			Assert(!secondPackLine.Complete);
			Assert(!fourthPackLine.Complete);
			Assert(!secondPackLine.Shortage);
			Assert(!fourthPackLine.Shortage);
			AssertHasMessageError(secondPackLine.ShortageInfo, $"At least one packline needs to be marked as complete or have a Shortage in case {secondPackLine.documentNoLabel} is not marked as complete.");
			AssertHasMessageError(fourthPackLine.ShortageInfo, $"At least one packline needs to be marked as complete or have a Shortage in case {secondPackLine.documentNoLabel} is not marked as complete.");

			fourthPackLine.Complete = true;
			Assert(!secondPackLine.Complete);
			Assert(fourthPackLine.Complete);
			Assert(!secondPackLine.Shortage);
			Assert(!fourthPackLine.Shortage);
			AssertNoMessageErrors(secondPackLine.ShortageInfo);
			AssertNoMessageErrors(fourthPackLine.ShortageInfo);
		}

		public void TestSetDefaultValuesForAESCustomsData()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			consolBO.Containers.RemoveAndDeleteAll();

			var shipmentBO1 = consolBO.Shipments.AddNew();
			shipmentBO1.JS_UniqueConsignRef = "S0001";
			shipmentBO1.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO1.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipmentBO1.CustomsEntryNumber = "CEN001";
			shipmentBO1.JS_ActualWeight = 999;
			shipmentBO1.JS_GoodsValue = 999;

			shipmentBO1.OuterPackLines.RemoveAndDeleteAll();

			var packlineBO = shipmentBO1.OuterPackLines.AddNew();
			packlineBO.JL_ExportRefNumber = "ref number1";
			packlineBO.JL_ItemNo = 0;

			var packlineBO2 = shipmentBO1.OuterPackLines.AddNew();
			packlineBO2.JL_ExportRefNumber = "ref number1";
			packlineBO2.JL_ItemNo = 0;

			var shipmentBO2 = consolBO.Shipments.AddNew();
			shipmentBO2.JS_UniqueConsignRef = "S0002";
			shipmentBO2.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO2.CustomsEntryNumberType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			shipmentBO2.CustomsEntryNumber = "CEN001";
			shipmentBO2.JS_ActualWeight = 999;
			shipmentBO2.JS_GoodsValue = 999;

			shipmentBO2.OuterPackLines.RemoveAndDeleteAll();

			var packlineBO3 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO3.JL_ExportRefNumber = "ref number2";
			packlineBO3.JL_ItemNo = 0;

			var packlineBO4 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO4.JL_ExportRefNumber = "ref number2";
			packlineBO4.JL_ItemNo = 0;

			var packlineBO5 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO5.JL_ExportRefNumber = "ref number2";
			packlineBO5.JL_ItemNo = 1;

			var packlineBO6 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO6.JL_ExportRefNumber = "ref number2";
			packlineBO6.JL_ItemNo = 1;

			var packlineBO7 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO7.JL_ExportRefNumber = "ref number2";
			packlineBO7.JL_ItemNo = 2;

			var packlineBO8 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO8.JL_ExportRefNumber = "ref number3";
			packlineBO8.JL_ItemNo = 1;

			var packlineBO9 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO9.JL_ExportRefNumber = "ref number3";
			packlineBO9.JL_ItemNo = 1;

			var packlineBO10 = shipmentBO2.OuterPackLines.AddNew();
			packlineBO10.JL_ExportRefNumber = "ref number4";
			packlineBO10.JL_ItemNo = 1;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine1 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0001").PackingLines.First(x => x.PackageNumber == "1");
			var packLine2 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0001").PackingLines.First(x => x.PackageNumber == "2");
			var packLine3 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "1" && x.CargoItem == "0");
			var packLine4 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "2" && x.CargoItem == "0");
			var packLine5 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "1" && x.CargoItem == "1" && x.ExportReferenceNumber == "ref number2");
			var packLine6 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "2" && x.CargoItem == "1" && x.ExportReferenceNumber == "ref number2");
			var packLine7 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber.IsEmpty && x.CargoItem == "2");
			var packLine8 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "1" && x.CargoItem == "1" && x.ExportReferenceNumber == "ref number3");
			var packLine9 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "2" && x.CargoItem == "1" && x.ExportReferenceNumber == "ref number3");
			var packLine10 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber.IsEmpty && x.CargoItem.IsEmpty);

			void AssertDefaultValuesForAESCustomsData(PackingLine packingLine, ZBool complete, ZBool shortage, ZString cargoItem, ZString packageNumber)
			{
				AssertEquals(packingLine.Complete, complete);
				AssertEquals(packingLine.Shortage, shortage);
				AssertEquals(packingLine.CargoItem, cargoItem);
				AssertEquals(packingLine.PackageNumber, packageNumber);
			}

			AssertDefaultValuesForAESCustomsData(packLine1, false, false, "0", "1");
			AssertDefaultValuesForAESCustomsData(packLine2, true, false, "0", "2");
			AssertDefaultValuesForAESCustomsData(packLine3, false, false, "0", "1");
			AssertDefaultValuesForAESCustomsData(packLine4, true, false, "0", "2");
			AssertDefaultValuesForAESCustomsData(packLine5, false, false, "1", "1");
			AssertDefaultValuesForAESCustomsData(packLine6, true, false, "1", "2");
			AssertDefaultValuesForAESCustomsData(packLine7, true, false, "2", ZString.Empty);
			AssertDefaultValuesForAESCustomsData(packLine8, false, false, "1", "1");
			AssertDefaultValuesForAESCustomsData(packLine9, true, false, "1", "2");
			AssertDefaultValuesForAESCustomsData(packLine10, true, false, ZString.Empty, ZString.Empty);
		}

		public void TestShortageInfoValueChanged()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			consolBO.Containers.RemoveAndDeleteAll();

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_UniqueConsignRef = "S0002";
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			shipmentBO.CustomsEntryNumber = "CEN001";
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;

			shipmentBO.OuterPackLines.RemoveAndDeleteAll();

			var packlineBO1 = shipmentBO.OuterPackLines.AddNew();
			packlineBO1.JL_ExportRefNumber = "ref number2";
			packlineBO1.JL_ItemNo = 0;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine1 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First();

			Assert(packLine1.Complete);
			Assert(!packLine1.Shortage);
			AssertNullOrEmpty(packLine1.CargoItem);
			AssertNullOrEmpty(packLine1.PackageNumber);

			packLine1.Shortage = true;

			Assert(!packLine1.Complete);
			Assert(packLine1.Shortage);
			AssertEquals(packLine1.CargoItem, "0");
			AssertEquals(packLine1.PackageNumber, "1");

			var packlineBO2 = shipmentBO.OuterPackLines.AddNew();
			packlineBO2.JL_ExportRefNumber = "ref number2";
			packlineBO2.JL_ItemNo = 0;

			builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			advancedLogisticsPortOrder = builder.Build();
			packLine1 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "1" && x.CargoItem == "0");
			var packLine2 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "2" && x.CargoItem == "0");

			Assert(packLine2.Complete);
			Assert(!packLine2.Shortage);
			AssertEquals(packLine2.CargoItem, "0");
			AssertEquals(packLine2.PackageNumber, "2");

			packLine2.CargoItem = ZString.Empty;
			packLine2.PackageNumber = ZString.Empty;

			Assert(packLine2.Complete);
			Assert(!packLine2.Shortage);
			AssertNullOrEmpty(packLine2.CargoItem);
			AssertNullOrEmpty(packLine2.PackageNumber);

			packLine2.Shortage = true;

			Assert(!packLine2.Complete);
			Assert(packLine2.Shortage);
			AssertEquals(packLine2.CargoItem, "0");
			AssertEquals(packLine2.PackageNumber, "2");
		}

		public void TestPackingLineValidation_Shortage()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			consolBO.Containers.RemoveAndDeleteAll();

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_UniqueConsignRef = "S0002";
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			shipmentBO.CustomsEntryNumber = "CEN001";
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;

			shipmentBO.OuterPackLines.RemoveAndDeleteAll();

			var packlineBO1 = shipmentBO.OuterPackLines.AddNew();
			packlineBO1.JL_ExportRefNumber = "ref number2";
			packlineBO1.JL_ItemNo = 0;

			var packlineBO2 = shipmentBO.OuterPackLines.AddNew();
			packlineBO2.JL_ExportRefNumber = "ref number2";
			packlineBO2.JL_ItemNo = 0;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine1 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "1" && x.CargoItem == "0");
			var packLine2 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "2" && x.CargoItem == "0");

			AssertNoMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");
			AssertNoMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");

			packLine2.Complete = false;

			AssertHasMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");
			AssertHasMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");

			packLine1.Shortage = true;

			AssertNoMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");
			AssertNoMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");

			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_AES;

			AssertNoMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertHasMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");

			packLine2.Complete = true;

			AssertNoMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertNoMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");

			packLine1.Shortage = false;

			AssertHasMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertNoMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case LRN is not marked as complete.");

			packLine2.EntryType.Code = EntryTypes.Codes.EntryTypes_AES;

			AssertNoMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertNoMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");

			packLine2.Complete = false;

			AssertHasMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertHasMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");

			packLine2.CargoItem = "1";

			AssertHasMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertHasMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");

			packLine2.Complete = true;
			AssertHasMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertNoMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");

			packLine1.CargoItem = "1";
			AssertNoMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");
			AssertNoMessageError(packLine2.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case MRN is not marked as complete.");

			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_1000G;
			AssertHasMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case Document Number is not marked as complete.");

			packLine1.Complete = true;
			AssertNoMessageError(packLine1.ShortageInfo, "At least one packline needs to be marked as complete or have a Shortage in case Document Number is not marked as complete.");
		}

		public void TestPackingLineValidation_CargoItem()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			consolBO.Containers.RemoveAndDeleteAll();

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.JS_UniqueConsignRef = "S0002";
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			shipmentBO.CustomsEntryNumber = "CEN001";
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;

			shipmentBO.OuterPackLines.RemoveAndDeleteAll();

			var packlineBO1 = shipmentBO.OuterPackLines.AddNew();
			packlineBO1.JL_ExportRefNumber = "ref number2";
			packlineBO1.JL_ItemNo = 1;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine1 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First();

			AssertNoMessageError(packLine1.CargoItemInfo, "Cargo Item no. should be numeric or blank.");
			AssertNoMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine1.CargoItem = "0";

			AssertNoMessageError(packLine1.CargoItemInfo, "Cargo Item no. should be numeric or blank.");
			AssertHasMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine1.CargoItem = "1";

			AssertNoMessageError(packLine1.CargoItemInfo, "Cargo Item no. should be numeric or blank.");
			AssertNoMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine1.CargoItem = ZString.Empty;

			AssertNoMessageError(packLine1.CargoItemInfo, "Cargo Item no. should be numeric or blank.");
			AssertNoMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine1.CargoItem = "0x";

			AssertHasMessageError(packLine1.CargoItemInfo, "Cargo Item no. should be numeric or blank.");
			AssertNoMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			var packlineBO2 = shipmentBO.OuterPackLines.AddNew();
			packlineBO2.JL_ExportRefNumber = "ref number2";
			packlineBO2.JL_ItemNo = 1;

			builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			advancedLogisticsPortOrder = builder.Build();
			packLine1 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "1" && x.CargoItem == "1");
			var packLine2 = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == "S0002").PackingLines.First(x => x.PackageNumber == "2" && x.CargoItem == "1");

			AssertNoMessageError(packLine1.CargoItemInfo, "Cargo Item no. should be numeric or blank.");
			AssertNoMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			AssertNoMessageError(packLine2.CargoItemInfo, "Cargo Item no. should be numeric or blank.");
			AssertNoMessageError(packLine2.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine1.CargoItem = ZString.Empty;

			AssertHasMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertNoMessageError(packLine2.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine2.CargoItem = ZString.Empty;

			AssertHasMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertHasMessageError(packLine2.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine2.CargoItem = "0";
			packLine1.CargoItem = "0";

			AssertHasMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertHasMessageError(packLine2.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine2.CargoItem = "1";
			packLine1.CargoItem = "1";

			AssertNoMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertNoMessageError(packLine2.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine1.CargoItem = ZString.Empty;
			packLine2.CargoItem = ZString.Empty;

			AssertHasMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertHasMessageError(packLine2.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");

			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_1000G;

			AssertNoMessageError(packLine1.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
			AssertNoMessageError(packLine2.CargoItemInfo, "Please provide Cargo Item no. directly on the form or in Shipment > Packline > item No.");
		}

		public void TestPopulatePackingLineHarmonisedCode()
		{
			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();

			var packlineBO = shipmentBO.OuterPackLines.AddNew();
			packlineBO.JL_HarmonisedCode = "HC Code";

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			AssertEquals("HC Code", packLine.HarmonizedCode.Code);
		}

		public void TestPopulateParties()
		{
			var consol = CreateConsol();
			CreateTransport(consol, "DEBRE", "CNYTN");
			CreateAddresses(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertAddressData(consol.ShippingLineAddress, advancedLogisticsPortOrder.Carrier);
			AssertPopulateCarrierCode(consol, consol.ShippingLineAddress, "CarrierCode");
		}

		void AssertPopulateWarehouseCode(ForwardingConsol consol, OrgAddress address, string warehouseCodePropertyName)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var warehouseCode = address.Header.CustomsCodes.AddNew();
			warehouseCode.OK_CodeType = OrgCusCode.CodeTypes.EDISiteID;
			warehouseCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			warehouseCode.OK_CustomsRegNo = "WHCODE001";

			var advancedLogisticsPortOrder = builder.Build();

			AssertEquals($"{warehouseCodePropertyName} should be from org", "WHCODE001", ((RegistrationNumber)advancedLogisticsPortOrder[warehouseCodePropertyName]).Value);

			var warehouseCode2 = address.CustomsCodes.AddNew();
			warehouseCode2.OK_CodeType = OrgCusCode.CodeTypes.EDISiteID;
			warehouseCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			warehouseCode2.OK_CustomsRegNo = "WHCODE002";

			advancedLogisticsPortOrder = builder.Build();

			AssertEquals($"{warehouseCodePropertyName} should be from org", "WHCODE002", ((RegistrationNumber)advancedLogisticsPortOrder[warehouseCodePropertyName]).Value);

			var warehouseCode3 = address.Header.CustomsCodes.AddNew();
			warehouseCode3.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.CWC;
			warehouseCode3.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			warehouseCode3.OK_CustomsRegNo = "WHCODE003";

			advancedLogisticsPortOrder = builder.Build();

			AssertEquals($"{warehouseCodePropertyName} should be from org", "WHCODE003", ((RegistrationNumber)advancedLogisticsPortOrder[warehouseCodePropertyName]).Value);

			var warehouseCode4 = address.CustomsCodes.AddNew();
			warehouseCode4.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.CWC;
			warehouseCode4.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			warehouseCode4.OK_CustomsRegNo = "WHCODE004";

			advancedLogisticsPortOrder = builder.Build();

			AssertEquals($"{warehouseCodePropertyName} should be from org", "WHCODE004", ((RegistrationNumber)advancedLogisticsPortOrder[warehouseCodePropertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			advancedLogisticsPortOrder = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)advancedLogisticsPortOrder[warehouseCodePropertyName]).Value);
		}

		void AssertPopulateCarrierCode(ForwardingConsol consol, OrgAddress address, string carrierCodePropertyName)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);

			var carrierCodeBHT = address.Header.CustomsCodes.AddNew();
			carrierCodeBHT.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.BHT;
			carrierCodeBHT.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			carrierCodeBHT.OK_CustomsRegNo = "BHT001";

			var advancedLogisticsPortOrder = builder.Build();
			AssertEquals($"{carrierCodePropertyName} should be from org BHT", "BHT001", ((RegistrationNumber)advancedLogisticsPortOrder[carrierCodePropertyName]).Value);

			var carrierCodeBHT2 = address.CustomsCodes.AddNew();
			carrierCodeBHT2.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.BHT;
			carrierCodeBHT2.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;
			carrierCodeBHT2.OK_CustomsRegNo = "BHT002";

			advancedLogisticsPortOrder = builder.Build();
			AssertEquals($"{carrierCodePropertyName} should be from org BHT", "BHT002", ((RegistrationNumber)advancedLogisticsPortOrder[carrierCodePropertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			advancedLogisticsPortOrder = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)advancedLogisticsPortOrder[carrierCodePropertyName]).Value);
		}

		public void TestPopulateSendingParty()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = "DE";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertAddressData(branchProxy.MainAddress, advancedLogisticsPortOrder.SendingParty);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			advancedLogisticsPortOrder = builder.Build();
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, advancedLogisticsPortOrder.SendingParty);
		}

		public void TestPopulateGoodsAndEquipmentInformation()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				PopulateGoodsAndEquipmentInformation(true);
			}

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				PopulateGoodsAndEquipmentInformation(false);
			}

			void PopulateGoodsAndEquipmentInformation(bool activateIsCombustibleForDGItems)
			{
				var consol = CreateConsol();
				CreateBasicGoodsAndEquipment(consol);

				var parameters = new DummyDocDataObjectParameters
				{
					DataStoreName = "AdvancedLogisticsPortOrder_IMP"
				};

				var builder = new AdvancedLogisticsPortOrderBuilder(consol);
				var advancedLogisticsPortOrder = builder.Build();

				AssertNotNull(advancedLogisticsPortOrder);

				AssertEquals("AdvancedLogisticsPortOrder should contain 1 container", 1, advancedLogisticsPortOrder.Containers.Count);

				var container1 = advancedLogisticsPortOrder.Containers.ElementAt(0);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should contain 1 packinglines", 1, container1.PackingLines.Count);
				AssertEquals("AdvancedLogisticsPortOrder container 1 Equipment Number MSCU1245787", "MSCU1245787", container1.Number);
				AssertEquals("AdvancedLogisticsPortOrder container 1 Type 20GP with ISO Type 22P2", "22P2", container1.Type.ISOCode);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should not be empty", false, container1.IsEmpty);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should have TareWeight 954", 954m, container1.TareWeight.Value);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should have GrossWeight 972", 972m, container1.GrossWeight.Value);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should have NetWeight 18", 18m, container1.NetWeight.Value);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should have Seal 1 = Seal1", "Seal1", container1.Seal);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should have Seal 2 = Seal2", "Seal2", container1.SecondSeal);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should have Seal 3 = Seal3", "Seal3", container1.ThirdSeal);
				AssertEquals("AdvancedLogisticsPortOrder container 1 should have Total Volume 9", 9m, container1.Volume.Value);

				var packline1 = container1.PackingLines.ElementAt(0);
				AssertEquals("Description (good 1)", "ROOF COVERING", packline1.GoodsDescription);
				AssertEquals("Outturn Comment (good 1)", "Outturn Comment", packline1.OutturnComment);
				AssertEquals("Marks And Numbers (good 1)", "MarksAndNosPL1", packline1.MarksAndNumbers);
				AssertEquals("MRN (good 1)", "MRN00001", packline1.ReferenceNumber);
				AssertEquals("Commodity (good 1)", "1234", packline1.Commodity.Code);
				AssertEquals("Packs (good 1)", 8, packline1.Quantity);
				AssertEquals("Package Type (good 1)", "PLT", packline1.PackageType.Code);
				AssertEquals("Weight (good 1)", 18m, packline1.Weight.Value);
				AssertEquals("Weight UM (good 1)", "KG", packline1.Weight.Unit.Code);
				AssertEquals("Volume (good 1)", 9m, packline1.Volume.Value);
				AssertEquals("Volume UM (good 1)", "M3", packline1.Volume.Unit.Code);
				AssertEquals("Number of Dangerous Goods (good 1)", 1, packline1.DangerousGoods.Count);

				var dangerousGood1 = packline1.DangerousGoods.ElementAt(0);
				AssertEquals("IMOClass of Dangerous Goods (good 1 - dg 1)", "1.4G", dangerousGood1.IMOClass);
				AssertEquals("UNDG Code of Dangerous Goods (good 1 - dg 1)", "0503b", dangerousGood1.Code);
				AssertEquals("Sublabel 1 of Dangerous Goods (good 1 - dg 1)", "SL1", dangerousGood1.SubLabel1);
				AssertEquals("PackingGroup of Dangerous Goods (good 1 - dg 1)", "II", dangerousGood1.PackingGroup);

				if (activateIsCombustibleForDGItems)
				{
					AssertNull("FlashPoint of Dangerous Goods is null", dangerousGood1.FlashPoint);
				}
				else
				{
					AssertEquals("FlashPoint of Dangerous Goods (good 1 - dg 1)", 85.0m, dangerousGood1.FlashPoint.Value);
				}

				AssertEquals("Proper Shipping Name of Dangerous Goods (good 1 - dg 1)", "AIR BAG MODULES", dangerousGood1.ProperShippingName);
				AssertEquals("PackedInLimitedQty of Dangerous Goods (good 1 - dg 1)", false, dangerousGood1.PackedInLimitedQuantity);
			}
		}

		public void TestPopulateGoodsAndEquipmentInformation_ShippersConsol()
		{
			var consol = CreateConsol();
			CreateBasicGoodsAndEquipment(consol);

			var firstContainer = consol.Containers.Cast<ForwardingContainer>().FirstOrDefault();
			AssertNotNull("Pre-condition", firstContainer);
			firstContainer.JC_ContainerMode = Constants.ContainerModes.ShippersConsol;

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertNotNull(advancedLogisticsPortOrder);
			AssertEquals("AdvancedLogisticsPortOrder should contain 1 container", 1, advancedLogisticsPortOrder.Containers.Count);
		}

		public void TestPopulateMarkAndNumbers()
		{
			var consol = CreateConsol();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00001816";
			shipment.JS_MarksAndNumbers = "marks & numbers";

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			AssertEquals("Mark and Numbers should be shipment's Mark and Numbers, when consol has one shipment", shipment.JS_MarksAndNumbers, advancedLogisticsPortOrder.MarksAndNumbers);

			var anotherShipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00001817";
			shipment.JS_MarksAndNumbers = "another marks & numbers";

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();
			AssertEquals("Mark and Numbers should be empty, when consol has more than one shipment", ZString.Empty, advancedLogisticsPortOrder.MarksAndNumbers);
		}

		public void TestPopulateGoodsWithoutEquipmentInformation()
		{
			var consol = CreateConsol();
			CreateBasicGoodsWithoutEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertEquals("AdvancedLogisticsPortOrder should contain 0 containers", 0, advancedLogisticsPortOrder.Containers.Count);
			AssertEquals("AdvancedLogisticsPortOrder should contain 2 packinglines", 2, advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.Count);

			var packline1 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAt(0);
			AssertEquals("Description (good 1)", "ROOF COVERING", packline1.GoodsDescription);
			AssertEquals("Outturn Comment (good 1)", "Outturn Comment", packline1.OutturnComment);
			AssertEquals("Marks And Numbers (good 1)", "marks & numbers", packline1.MarksAndNumbers);
			AssertEquals("MRN (good 1)", "MRN00001", packline1.ReferenceNumber);
			AssertEquals("Commodity (good 1)", "1248", packline1.Commodity.Code);
			AssertEquals("Packs (good 1)", 8, packline1.Quantity);
			AssertEquals("Package Type (good 1)", "PLT", packline1.PackageType.Code);
			AssertEquals("Weight (good 1)", 18m, packline1.Weight.Value);
			AssertEquals("Weight UM (good 1)", "KG", packline1.Weight.Unit.Code);
			AssertEquals("Volume (good 1)", 9m, packline1.Volume.Value);
			AssertEquals("Volume UM (good 1)", "M3", packline1.Volume.Unit.Code);
			AssertEquals("Number of Dangerous Goods (good 1)", 1, packline1.DangerousGoods.Count);

			var dangerousGood1 = packline1.DangerousGoods.ElementAt(0);
			AssertEquals("IMOClass of Dangerous Goods (good 1 - dg 1)", "1.4G", dangerousGood1.IMOClass);
			AssertEquals("UNDG Code of Dangerous Goods (good 1 - dg 1)", "0503b", dangerousGood1.Code);
			AssertEquals("Sublabel 1 of Dangerous Goods (good 1 - dg 1)", "SL1", dangerousGood1.SubLabel1);
			AssertEquals("PackingGroup of Dangerous Goods (good 1 - dg 1)", "II", dangerousGood1.PackingGroup);
			AssertEquals("FlashPoint of Dangerous Goods (good 1 - dg 1)", 85.0m, dangerousGood1.FlashPoint.Value);
			AssertEquals("Proper Shipping Name of Dangerous Goods (good 1 - dg 1)", "AIR BAG MODULES", dangerousGood1.ProperShippingName);
			AssertEquals("PackedInLimitedQty of Dangerous Goods (good 1 - dg 1)", false, dangerousGood1.PackedInLimitedQuantity);
		}

		public void TestPopulateDangerousGoodsPacksWeightVolume_ShouldFallbackToPackingLinePacksWeightVolume_WhenThereIsOneDangerousGoodsRecord()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var forwardingPackingLine = AddPackingLine(shipment, true, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packingLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAt(0);
			var dangerousGood = packingLine.DangerousGoods.ElementAt(0);

			AssertEquals("Quantity should fallback to PackingLine quantity when there is one dangerous goods record", packingLine.Quantity, dangerousGood.Quantity);
			AssertEquals("Package Type should fallback to PackingLine package type when there is one dangerous goods record", packingLine.PackageType.Code, dangerousGood.PackageType.Code);
			AssertEquals("Weight should fallback to PackingLine weight when there is one dangerous goods record", packingLine.Weight.Value, dangerousGood.Weight.Value);
			AssertEquals("Weight Unit should fallback to PackingLine weight unit when there is one dangerous goods record", packingLine.Weight.Unit.Code, dangerousGood.Weight.Unit.Code);
			AssertEquals("Volume should fallback to PackingLine volume when there is one dangerous goods record", packingLine.Volume.Value, dangerousGood.Volume.Value);
			AssertEquals("Volume Unit should fallback to PackingLine volume unit when there is one dangerous goods record", packingLine.Volume.Unit.Code, dangerousGood.Volume.Unit.Code);

			AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();
			packingLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAt(0);
			var firstDangerousGoodsRecord = packingLine.DangerousGoods.ElementAt(0);
			AssertEquals("Quantity should not fallback to PackingLine quantity when there is more than one dangerous goods record", 0, firstDangerousGoodsRecord.Quantity);
			AssertEquals("Package Type should not fallback to PackingLine package type when there is more than one dangerous goods record", ZString.Empty, firstDangerousGoodsRecord.PackageType.Code);
			AssertEquals("Weight should not fallback to PackingLine weight when there is more than one dangerous goods record", 0m, firstDangerousGoodsRecord.Weight.Value);
			AssertEquals("Weight Unit default value is KG", ZString.Empty, firstDangerousGoodsRecord.Weight.Unit.Code);
			AssertEquals("Volume should not fallback to PackingLine volume when there is more than one dangerous goods record", 0m, firstDangerousGoodsRecord.Volume.Value);
			AssertEquals("Volume Unit default value is M3", ZString.Empty, firstDangerousGoodsRecord.Volume.Unit.Code);

			var secondDangerousGoodsRecord = packingLine.DangerousGoods.ElementAt(1);
			AssertEquals("Quantity should not fallback to PackingLine quantity when there is more than one dangerous goods record", 0, secondDangerousGoodsRecord.Quantity);
			AssertEquals("Package Type should not fallback to PackingLine package type when there is more than one dangerous goods record", ZString.Empty, secondDangerousGoodsRecord.PackageType.Code);
			AssertEquals("Weight should not fallback to PackingLine weight when there is more than one dangerous goods record", 0m, secondDangerousGoodsRecord.Weight.Value);
			AssertEquals("Weight Unit default value is KG", ZString.Empty, secondDangerousGoodsRecord.Weight.Unit.Code);
			AssertEquals("Volume should not fallback to PackingLine volume when there is more than one dangerous goods record", 0m, secondDangerousGoodsRecord.Volume.Value);
			AssertEquals("Volume Unit default value is M3", ZString.Empty, secondDangerousGoodsRecord.Volume.Unit.Code);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			AddPackingLine(shipment, true, true, "BAG", 12, true, Core.Constants.Weight.Kilograms, 8m, true, Core.Constants.Volume.CubicMetres, 4m);

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();
			packingLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAt(0);
			var dangerousGoodsRecord = packingLine.DangerousGoods.ElementAt(0);
			AssertEquals("Quantity should be dangerous goods quantity when it has value", 12, dangerousGoodsRecord.Quantity);
			AssertEquals("Package Type should be dangerous goods package type when it has value", "BAG", dangerousGoodsRecord.PackageType.Code);
			AssertEquals("Weight should be dangerous goods weight when it has value", 8m, dangerousGoodsRecord.Weight.Value);
			AssertEquals("Weight Unit should be dangerous goods weight unit when it has value", Constants.Weight.Kilograms, dangerousGoodsRecord.Weight.Unit.Code);
			AssertEquals("Volume should be dangerous goods volume when it has value", 4m, dangerousGoodsRecord.Volume.Value);
			AssertEquals("Volume Unit should be dangerous goods volume unit when it has value", Core.Constants.Volume.CubicMetres, dangerousGoodsRecord.Volume.Unit.Code);
		}

		public void TestPopulateDangerousGoodsPacksWeightVolumeForContainerizedConsol_ShouldFallbackToPackingLinePacksWeightVolume_WhenThereIsOneDangerousGoodsRecord()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol);
			var consolContainer = CreateContainer(consol);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var forwardingPackingLine = AddPackingLine(shipment, true, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);
			forwardingPackingLine.JL_JC = consolContainer.PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			var container = advancedLogisticsPortOrder.Containers.ElementAt(0);
			var packingLine = container.PackingLines.ElementAt(0);
			var dangerousGood = packingLine.DangerousGoods.ElementAt(0);

			AssertEquals("Quantity should fallback to PackingLine quantity when there is one dangerous goods record", packingLine.Quantity, dangerousGood.Quantity);
			AssertEquals("Package Type should fallback to PackingLine package type when there is one dangerous goods record", packingLine.PackageType.Code, dangerousGood.PackageType.Code);
			AssertEquals("Weight should fallback to PackingLine weight when there is one dangerous goods record", packingLine.Weight.Value, dangerousGood.Weight.Value);
			AssertEquals("Weight Unit should fallback to PackingLine weight unit when there is one dangerous goods record", packingLine.Weight.Unit.Code, dangerousGood.Weight.Unit.Code);
			AssertEquals("Volume should fallback to PackingLine volume when there is one dangerous goods record", packingLine.Volume.Value, dangerousGood.Volume.Value);
			AssertEquals("Volume Unit should fallback to PackingLine volume unit when there is one dangerous goods record", packingLine.Volume.Unit.Code, dangerousGood.Volume.Unit.Code);

			AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();

			container = advancedLogisticsPortOrder.Containers.ElementAt(0);
			packingLine = container.PackingLines.ElementAt(0);

			var firstDangerousGoodsRecord = packingLine.DangerousGoods.ElementAt(0);
			AssertEquals("Quantity should not fallback to PackingLine quantity when there is more than one dangerous goods record", 0, firstDangerousGoodsRecord.Quantity);
			AssertEquals("Package Type should not fallback to PackingLine package type when there is more than one dangerous goods record", ZString.Empty, firstDangerousGoodsRecord.PackageType.Code);
			AssertEquals("Weight should not fallback to PackingLine weight when there is more than one dangerous goods record", 0m, firstDangerousGoodsRecord.Weight.Value);
			AssertEquals("Weight Unit default value is KG", ZString.Empty, firstDangerousGoodsRecord.Weight.Unit.Code);
			AssertEquals("Volume should not fallback to PackingLine volume when there is more than one dangerous goods record", 0m, firstDangerousGoodsRecord.Volume.Value);
			AssertEquals("Volume Unit default value is M3", ZString.Empty, firstDangerousGoodsRecord.Volume.Unit.Code);

			var secondDangerousGoodsRecord = packingLine.DangerousGoods.ElementAt(1);
			AssertEquals("Quantity should not fallback to PackingLine quantity when there is more than one dangerous goods record", 0, secondDangerousGoodsRecord.Quantity);
			AssertEquals("Package Type should not fallback to PackingLine package type when there is more than one dangerous goods record", ZString.Empty, secondDangerousGoodsRecord.PackageType.Code);
			AssertEquals("Weight should not fallback to PackingLine weight when there is more than one dangerous goods record", 0m, secondDangerousGoodsRecord.Weight.Value);
			AssertEquals("Weight Unit default value is KG", ZString.Empty, secondDangerousGoodsRecord.Weight.Unit.Code);
			AssertEquals("Volume should not fallback to PackingLine volume when there is more than one dangerous goods record", 0m, secondDangerousGoodsRecord.Volume.Value);
			AssertEquals("Volume Unit default value is M3", ZString.Empty, secondDangerousGoodsRecord.Volume.Unit.Code);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			AddPackingLine(shipment, true, true, "BAG", 12, true, Core.Constants.Weight.Kilograms, 8m, true, Core.Constants.Volume.CubicMetres, 4m);

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();

			container = advancedLogisticsPortOrder.Containers.ElementAt(0);
			packingLine = container.PackingLines.ElementAt(0);

			var dangerousGoodsRecord = packingLine.DangerousGoods.ElementAt(0);
			AssertEquals("Quantity should be dangerous goods quantity when it has value", 12, dangerousGoodsRecord.Quantity);
			AssertEquals("Package Type should be dangerous goods package type when it has value", "BAG", dangerousGoodsRecord.PackageType.Code);
			AssertEquals("Weight should be dangerous goods weight when it has value", 8m, dangerousGoodsRecord.Weight.Value);
			AssertEquals("Weight Unit should be dangerous goods weight unit when it has value", Constants.Weight.Kilograms, dangerousGoodsRecord.Weight.Unit.Code);
			AssertEquals("Volume should be dangerous goods volume when it has value", 4m, dangerousGoodsRecord.Volume.Value);
			AssertEquals("Volume Unit should be dangerous goods volume unit when it has value", Core.Constants.Volume.CubicMetres, dangerousGoodsRecord.Volume.Unit.Code);
		}

		public void TestDefaultDangerousGoods_NetExplosiveWeight()
		{
			var consol = CreateConsol();
			CreateBasicGoodsWithoutEquipment(consol);

			AddPackingLine(consol.Shipments.Cast<ForwardingShipment>().First(), true, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);

			var forwardingPackingLine = AddPackingLine(consol.Shipments.Cast<ForwardingShipment>().First(), true, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);
			AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);
			AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var dangerousGoods = advancedLogisticsPortOrder.Shipments.SelectMany(shipment => shipment.PackingLines.SelectMany(packingLine => packingLine.DangerousGoods));

			AssertEquals(5, dangerousGoods.Count());
			Assert(dangerousGoods.All(x => x.NetExplosiveWeight != null && x.NetExplosiveWeight.Unit.Code == Constants.Weight.Kilograms));

			consol.Shipments.RemoveAndDeleteAll();

			CreateBasicGoodsAndEquipment(consol);
			var container = consol.Containers.Cast<ForwardingContainer>().First();

			container.PackLines.Add(AddPackingLine(consol.Shipments.Cast<ForwardingShipment>().First(), true, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m));

			forwardingPackingLine = AddPackingLine(consol.Shipments.Cast<ForwardingShipment>().First(), true, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);
			AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);
			AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);
			AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);

			container.PackLines.Add(forwardingPackingLine);

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();
			dangerousGoods = advancedLogisticsPortOrder.Shipments.SelectMany(shipment => shipment.PackingLines.SelectMany(packingLine => packingLine.DangerousGoods));

			AssertEquals(6, dangerousGoods.Count());
			Assert(dangerousGoods.All(x => x.NetExplosiveWeight != null && x.NetExplosiveWeight.Unit.Code == Constants.Weight.Kilograms));
		}

		public void TestValidateEquipmentInformation()
		{
			const string expectedError1 = "Make sure an ISO type has been set on the container type";
			const string expectedError2 = "MRN Number needs to be provided (Shipment > Packing > Pack Line > Export Ref Number or Shipment > Customs Entry Number)";
			const string expectedError3 = "LRN Number needs to be provided (Shipment > Packing > Pack Line > Export Ref Number or Shipment > Customs Entry Number)";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);

			advancedLogisticsPortOrder.Containers.ElementAt(0).Type.ISOCode = ZString.Empty;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("ISO Container Type should not have error", advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).Type.ISOCodeInfo, expectedError1);

			packLine.ExportReferenceNumber = ZString.Empty;
			packLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AES;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("Packline should have error", packLine.ExportReferenceNumberInfo, expectedError2);
			AssertNoMessageError("Packline should not have error", packLine.ExportReferenceNumberInfo, expectedError3);

			packLine.ExportReferenceNumber = ZString.Empty;
			packLine.EntryType.Code = EntryTypes.Codes.EntryTypes_AE1;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("Packline should have error", packLine.ExportReferenceNumberInfo, expectedError3);
			AssertNoMessageError("Packline should not have error", packLine.ExportReferenceNumberInfo, expectedError2);
		}

		public void TestValidateEORIAndEoriBranchSuffix()
		{
			const string sendingAgentEoriErrorMessage = "In case of AE1, EORI Number of Sending Agent or Consignor needs to be provided. (See Sending Agent > Config > Registration Number \"EOR\")";
			const string sendingAgentEoriBranchSuffixErrorMessage = "In case of AE1, EORI Branch Suffix of Sending Agent or Consignor needs to be provided. (See Sending Agent > Config > Registration Number \"EBS\")";
			const string consignorEoriErrorMessage = "In case of AE1, EORI Number of Sending Agent or Consignor needs to be provided. (See Shipment > Consignor > Config > Registration Number \"EOR\")";
			const string consignorEoriBranchSuffixErrorMessage = "In case of AE1, EORI Branch Suffix of Sending Agent or Consignor needs to be provided. (See Shipment > Consignor > Config > Registration Number \"EBS\")";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var shipmentDO = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0);
			var packLineDO = shipmentDO.PackingLines.ElementAtOrDefault(0);

			packLineDO.EntryType.Code = EntryTypes.Codes.EntryTypes_AE1;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError(advancedLogisticsPortOrder.EoriNumber.ValueInfo, sendingAgentEoriErrorMessage);
			AssertHasMessageError(advancedLogisticsPortOrder.EoriBranchSuffix.ValueInfo, sendingAgentEoriBranchSuffixErrorMessage);
			AssertHasMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriNumber).ValueInfo, consignorEoriErrorMessage);
			AssertHasMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriBranchSuffix).ValueInfo, consignorEoriBranchSuffixErrorMessage);

			advancedLogisticsPortOrder.EoriNumber.Value = "abc";
			shipmentDO.ConsignorEoriBranchSuffix.Value = "abc";
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError(advancedLogisticsPortOrder.EoriNumber.ValueInfo, sendingAgentEoriErrorMessage);
			AssertHasMessageError(advancedLogisticsPortOrder.EoriBranchSuffix.ValueInfo, sendingAgentEoriBranchSuffixErrorMessage);
			AssertHasMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriNumber).ValueInfo, consignorEoriErrorMessage);
			AssertHasMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriBranchSuffix).ValueInfo, consignorEoriBranchSuffixErrorMessage);

			packLineDO.EntryType.Code = EntryTypes.Codes.EntryTypes_1000G;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertNoMessageError(advancedLogisticsPortOrder.EoriNumber.ValueInfo, sendingAgentEoriErrorMessage);
			AssertNoMessageError(advancedLogisticsPortOrder.EoriBranchSuffix.ValueInfo, sendingAgentEoriBranchSuffixErrorMessage);
			AssertNoMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriNumber).ValueInfo, consignorEoriErrorMessage);
			AssertNoMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriBranchSuffix).ValueInfo, consignorEoriBranchSuffixErrorMessage);

			packLineDO.EntryType.Code = EntryTypes.Codes.EntryTypes_AE1;
			advancedLogisticsPortOrder.EoriNumber.Value = "abc";
			advancedLogisticsPortOrder.EoriBranchSuffix.Value = "abc";
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertNoMessageError(advancedLogisticsPortOrder.EoriNumber.ValueInfo, sendingAgentEoriErrorMessage);
			AssertNoMessageError(advancedLogisticsPortOrder.EoriBranchSuffix.ValueInfo, sendingAgentEoriBranchSuffixErrorMessage);
			AssertNoMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriNumber).ValueInfo, consignorEoriErrorMessage);
			AssertNoMessageError(((RegistrationNumber)shipmentDO.ConsignorEoriBranchSuffix).ValueInfo, consignorEoriBranchSuffixErrorMessage);
		}

		public void TestValidateDocumentNumber()
		{
			const string expectedError = "Document Number needs to be provided (Shipment > Packing > Pack Line > Export Ref Number or Shipment > Customs Entry Number)";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			var entryTypesRuleList = new string[]
			{
				EntryTypes.Codes.EntryTypes_2222G,
				EntryTypes.Codes.EntryTypes_3333G,
				EntryTypes.Codes.EntryTypes_3333N,
				EntryTypes.Codes.EntryTypes_9999G,
				EntryTypes.Codes.EntryTypes_9999N,
				EntryTypes.Codes.EntryTypes_4444N,
				EntryTypes.Codes.EntryTypes_5555D,
				EntryTypes.Codes.EntryTypes_7777G,
				EntryTypes.Codes.EntryTypes_7777N
			};

			foreach (var entryType in entryTypesRuleList)
			{
				packLine.EntryType.Code = entryType;

				packLine.ExportReferenceNumber = ZString.Empty;
				advancedLogisticsPortOrder.ValidateAllIncludingChildren();
				AssertHasMessageError("Document should have error", packLine.ExportReferenceNumberInfo, expectedError);
			}
		}

		public void TestValidateOrigin()
		{
			const string expectedError = "Origin required.";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			var shipment = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == packLine.ShipmentID);
			var entryTypesRuleList = new string[]
			{
				EntryTypes.Codes.EntryTypes_1000G,
				EntryTypes.Codes.EntryTypes_1000N,
				EntryTypes.Codes.EntryTypes_2222G,
				EntryTypes.Codes.EntryTypes_5555D,
				EntryTypes.Codes.EntryTypes_7777G,
				EntryTypes.Codes.EntryTypes_7777N
			};

			foreach (var entryType in entryTypesRuleList)
			{
				packLine.EntryType.Code = entryType;

				shipment.Origin.Code = ZString.Empty;
				advancedLogisticsPortOrder.ValidateAllIncludingChildren();
				AssertHasMessageError("Origin should have error", ((Unloco)shipment.Origin).CodeInfo, expectedError);
			}
		}

		public void TestValidateDestination()
		{
			const string expectedError = "Destination required.";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			var shipment = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == packLine.ShipmentID);
			var entryTypesRuleList = new string[]
			{
				EntryTypes.Codes.EntryTypes_1000G,
				EntryTypes.Codes.EntryTypes_1000N,
				EntryTypes.Codes.EntryTypes_1111G,
				EntryTypes.Codes.EntryTypes_1111N,
				EntryTypes.Codes.EntryTypes_2222G,
				EntryTypes.Codes.EntryTypes_3333G,
				EntryTypes.Codes.EntryTypes_3333N,
				EntryTypes.Codes.EntryTypes_9999G,
				EntryTypes.Codes.EntryTypes_9999N,
				EntryTypes.Codes.EntryTypes_4444N,
				EntryTypes.Codes.EntryTypes_5555D,
				EntryTypes.Codes.EntryTypes_7777G,
				EntryTypes.Codes.EntryTypes_7777N
			};

			foreach (var entryType in entryTypesRuleList)
			{
				packLine.EntryType.Code = entryType;

				shipment.Destination.Code = ZString.Empty;
				advancedLogisticsPortOrder.ValidateAllIncludingChildren();
				AssertHasMessageError("Destination should have error", ((Unloco)shipment.Destination).CodeInfo, expectedError);
			}
		}

		public void TestValidateGoodsDescription()
		{
			const string expectedError = "Goods Description required.";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			var entryTypesRuleList = new string[]
			{
				EntryTypes.Codes.EntryTypes_1000G,
				EntryTypes.Codes.EntryTypes_1000N,
				EntryTypes.Codes.EntryTypes_1111G,
				EntryTypes.Codes.EntryTypes_1111N,
				EntryTypes.Codes.EntryTypes_2222G,
				EntryTypes.Codes.EntryTypes_3333G,
				EntryTypes.Codes.EntryTypes_3333N,
				EntryTypes.Codes.EntryTypes_9999G,
				EntryTypes.Codes.EntryTypes_9999N,
				EntryTypes.Codes.EntryTypes_4444N,
				EntryTypes.Codes.EntryTypes_5555D,
				EntryTypes.Codes.EntryTypes_7777G,
				EntryTypes.Codes.EntryTypes_7777N,
				EntryTypes.Codes.EntryTypes_M
			};

			foreach (var entryType in entryTypesRuleList)
			{
				packLine.EntryType.Code = entryType;

				packLine.GoodsDescription = ZString.Empty;
				advancedLogisticsPortOrder.ValidateAllIncludingChildren();
				AssertHasMessageError("Goods Description should have error", packLine.GoodsDescriptionInfo, expectedError);
			}
		}

		public void TestValidateHarmonizedCode()
		{
			const string expectedError = "Harmonized Code required.";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			var entryTypesRuleList = new[]
			{
				EntryTypes.Codes.EntryTypes_1000G,
				EntryTypes.Codes.EntryTypes_1000N,
				EntryTypes.Codes.EntryTypes_4444N
			};

			foreach (var entryType in entryTypesRuleList)
			{
				packLine.EntryType.Code = entryType;

				packLine.HarmonizedCode.Code = ZString.Empty;
				advancedLogisticsPortOrder.ValidateAllIncludingChildren();
				AssertHasMessageError("Harmonized Code should have error", ((HarmonizedCode)packLine.HarmonizedCode).CodeInfo, expectedError);
			}
		}

		public void TestSameDocumentNumberShouldNotHaveDifferentEntryType()
		{
			const string expectedError = "It is not allowed to set different Entry types for the same document number!";

			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.CustomsEntryNumberType = ZString.Empty;
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.JS_ActualWeight = 999;
			shipmentBO.JS_GoodsValue = 999;

			shipmentBO.OuterPackLines.AddNew();
			shipmentBO.OuterPackLines.AddNew();
			shipmentBO.OuterPackLines.AddNew();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();

			var packLine1 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(1);
			var packLine2 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(2);
			var packLine3 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(3);
			Assert(packLine1.EntryType.Code.IsEmpty);
			Assert(packLine2.EntryType.Code.IsEmpty);
			Assert(packLine3.EntryType.Code.IsEmpty);

			packLine1.ExportReferenceNumber = "same ref number";
			packLine2.ExportReferenceNumber = "same ref number";
			packLine3.ExportReferenceNumber = "same ref number";
			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;
			packLine2.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;
			packLine3.EntryType.Code = EntryTypes.Codes.EntryTypes_AES;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError);
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine2.EntryType).CodeInfo, expectedError);
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine3.EntryType).CodeInfo, expectedError);

			packLine2.EntryType.Code = EntryTypes.Codes.EntryTypes_7777G;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("Entry Type should have error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError);
			AssertHasMessageError("Entry Type should have error", ((CodeDescription)packLine2.EntryType).CodeInfo, expectedError);
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine3.EntryType).CodeInfo, expectedError);

			packLine1.ExportReferenceNumber = ZString.Empty;
			packLine2.ExportReferenceNumber = ZString.Empty;
			packLine3.ExportReferenceNumber = ZString.Empty;
			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;
			packLine2.EntryType.Code = EntryTypes.Codes.EntryTypes_7777G;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError);
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine2.EntryType).CodeInfo, expectedError);
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine3.EntryType).CodeInfo, expectedError);
		}

		public void TestValidateCorrectEntryTypeAfterSaved()
		{
			const string expectedError_ShipmentHasMRN = "Entry Type should be AES when an MRN Number has been provided in Shipment > Entry Details";
			const string expectedError_ShipmentHasLRN = "Entry Type should be AE1 when an LRN Number has been provided in Shipment > Entry Details";
			const string expectedError_GoodsValueAbove1000EUR = "Invalid entry Type, as the goods value of this shipment is above € 1000,00";
			const string expectedError_GoodsWeightAbove1000KG = "Invalid entry Type, as the total weight of this shipment is above 1000 kg";

			var consolBO = Factory.New<ForwardingConsol>();
			consolBO.JK_AgentType = Constants.AgentType.Agent;
			consolBO.JK_ConsolMode = Constants.ContainerModes.LCL;
			consolBO.JK_RL_NKLoadPort = "DEWVN";

			var shipmentBO = consolBO.Shipments.AddNew();
			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			shipmentBO.OuterPackLines.AddNew();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			var advancedLogisticsPortOrder = builder.Build();

			var packLine1 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;

			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("EntryType should have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_ShipmentHasMRN);
			AssertNoMessageError("EntryType should not have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_ShipmentHasLRN);

			shipmentBO.CustomsEntryNumberType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			advancedLogisticsPortOrder = builder.Build();

			packLine1 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;

			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("EntryType should have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_ShipmentHasLRN);
			AssertNoMessageError("EntryType should not have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_ShipmentHasMRN);

			shipmentBO.CustomsEntryNumberType = ZString.Empty;
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_GoodsValue = 1001;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.JS_ActualWeight = 999;
			builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			advancedLogisticsPortOrder = builder.Build();

			packLine1 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;

			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("EntryType should have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_GoodsValueAbove1000EUR);
			AssertNoMessageError("EntryType should not have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_GoodsWeightAbove1000KG);

			shipmentBO.CustomsEntryNumberType = ZString.Empty;
			shipmentBO.JS_RX_NKGoodsValueCurr = Constants.CurrencyCodes.EuropeanUnion;
			shipmentBO.JS_GoodsValue = 999;
			shipmentBO.JS_UnitOfWeight = Constants.Weight.Kilograms;
			shipmentBO.JS_ActualWeight = 1001;
			builder = new AdvancedLogisticsPortOrderBuilder(consolBO);
			advancedLogisticsPortOrder = builder.Build();

			packLine1 = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			packLine1.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;

			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertNoMessageError("EntryType should not have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_GoodsValueAbove1000EUR);
			AssertHasMessageError("EntryType should have this error", ((CodeDescription)packLine1.EntryType).CodeInfo, expectedError_GoodsWeightAbove1000KG);
		}

		public void TestValidateEntryType()
		{
			const string expectedEmptyError = "Entry Type required.";
			const string expectedInvalidError = "Invalid Entry Type.";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);

			packLine.EntryType.Code = EntryTypes.Codes.EntryTypes_5555D;

			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine.EntryType).CodeInfo, expectedEmptyError);
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine.EntryType).CodeInfo, expectedInvalidError);

			packLine.EntryType.Code = ZString.Empty;
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertHasMessageError("Entry Type should have error", ((CodeDescription)packLine.EntryType).CodeInfo, expectedEmptyError);
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine.EntryType).CodeInfo, expectedInvalidError);

			packLine.EntryType.Code = "asd";
			advancedLogisticsPortOrder.ValidateAllIncludingChildren();
			AssertNoMessageError("Entry Type should not have error", ((CodeDescription)packLine.EntryType).CodeInfo, expectedEmptyError);
			AssertHasMessageError("Entry Type should have error", ((CodeDescription)packLine.EntryType).CodeInfo, expectedInvalidError);
		}

		public void TestValidateConsignor()
		{
			const string expectedError = "Consignor required.";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			var shipment = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == packLine.ShipmentID);
			var entryTypesRuleList = new string[]
			{
				EntryTypes.Codes.EntryTypes_1000G,
				EntryTypes.Codes.EntryTypes_1000N,
				EntryTypes.Codes.EntryTypes_1111G,
				EntryTypes.Codes.EntryTypes_1111N,
				EntryTypes.Codes.EntryTypes_2222G,
				EntryTypes.Codes.EntryTypes_4444N
			};

			foreach (var entryType in entryTypesRuleList)
			{
				packLine.EntryType.Code = entryType;

				shipment.Consignor.CompanyName = ZString.Empty;
				advancedLogisticsPortOrder.ValidateAllIncludingChildren();
				AssertHasMessageError("Consignor should have error", ((Address)shipment.Consignor).CompanyNameInfo, expectedError);
			}
		}

		public void TestValidateConsignee()
		{
			const string expectedError = "Consignee required.";

			var consol = CreateConsol("DEWVN", "BEANR");
			CreateBasicGoodsAndEquipment(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_EXP"
			};

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packLine = advancedLogisticsPortOrder.Containers.ElementAtOrDefault(0).PackingLines.ElementAtOrDefault(0);
			var shipment = advancedLogisticsPortOrder.Shipments.First(x => x.ShipmentID == packLine.ShipmentID);
			var entryTypesRuleList = new string[]
			{
				EntryTypes.Codes.EntryTypes_5555D,
				EntryTypes.Codes.EntryTypes_7777G,
				EntryTypes.Codes.EntryTypes_7777N
			};

			foreach (var entryType in entryTypesRuleList)
			{
				packLine.EntryType.Code = entryType;

				shipment.Consignee.CompanyName = ZString.Empty;
				advancedLogisticsPortOrder.ValidateAllIncludingChildren();
				AssertHasMessageError("Consignee should have error", ((Address)shipment.Consignee).CompanyNameInfo, expectedError);
			}
		}

		public void TestPopulateALPOReference()
		{
			var consol = CreateConsol();
			var szbNumber = consol.Numbers.AddNew();
			szbNumber.CE_EntryType = GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber;
			szbNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			szbNumber.CE_EntryNum = "12345";

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertEquals("ALPO Reference", szbNumber.CE_EntryNum, advancedLogisticsPortOrder.ALPOReference);
		}

		public void TestPopulateALPOUserID()
		{
			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			GenRegCertAccredMaintList certificate = currentStaff.Certificates.AddNew();
			certificate.XZ_Type = Constants.StaffDefaultCertificateIDAndTrainingTypes.DBH;
			certificate.XZ_RefNumber = "DBHLicNumber";

			Factory.Save();

			var exportClientNo = "TestZKV";
			FreightDataRegistry.Instance.ALPOExportClientNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exportClientNo);
			AssertEquals(exportClientNo, FreightDataRegistry.Instance.ALPOExportClientNo.Value);

			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			AssertEquals("DBHLicNumber", advancedLogisticsPortOrder.ALPOUserID);
		}

		public void TestErrorPlaceHolderValidation()
		{
			const string expectedError1 = "No Packing Lines found! Advanced Logistics Port Order cannot be created!";
			const string expectedError2 = "Container is required";

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = "AdvancedLogisticsPortOrder_IMP"
			};

			var consol = CreateConsol();
			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();

			AssertHasMessageError("Error, no packinglines present", advancedLogisticsPortOrder.ErrorPlaceHolderInfo, expectedError1);
			AssertHasMessageError("Error, no containers present", advancedLogisticsPortOrder.ErrorPlaceHolderInfo, expectedError2);

			CreateBasicGoodsAndEquipment(consol);
			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();

			AssertNoMessageError("No error, packinglines present with containers", advancedLogisticsPortOrder.ErrorPlaceHolderInfo, expectedError1);
			AssertNoMessageError("No error, containers present", advancedLogisticsPortOrder.ErrorPlaceHolderInfo, expectedError2);

			consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.Other;
			CreateBasicGoodsWithoutEquipment(consol);
			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();

			AssertNoMessageError("No error, packinglines present", advancedLogisticsPortOrder.ErrorPlaceHolderInfo, expectedError1);
			AssertNoMessageError("No error, no containers present", advancedLogisticsPortOrder.ErrorPlaceHolderInfo, expectedError2);
		}

		public void TestDangerousGoodsValidation()
		{
			var consol = CreateConsol();
			var shipment = CreateShipment(consol);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var forwardingPackingLine = AddPackingLine(shipment, false, false, ZString.Empty, 0, false, ZString.Empty, 0m, false, ZString.Empty, 0m);
			var undg = AddDangerousGoods(forwardingPackingLine, false, ZString.Empty, 0, true, Constants.Weight.Grams, 100m, false, ZString.Empty, 0m);

			var builder = new AdvancedLogisticsPortOrderBuilder(consol);
			var advancedLogisticsPortOrder = builder.Build();
			var packingLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAt(0);
			var dangerousGood = packingLine.DangerousGoods.ElementAt(0);

			AssertEquals(0m, dangerousGood.NetExplosiveWeight.Value);
			AssertHasMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight needs to be greater than 0 for UNDG class 1 substances.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be negative.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be higher than DG gross weight.");

			dangerousGood.NetExplosiveWeight.Value = -1m;

			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight needs to be greater than 0 for UNDG class 1 substances.");
			AssertHasMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be negative.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be higher than DG gross weight.");

			dangerousGood.NetExplosiveWeight.Value = 1m;

			AssertEquals("100.00 G", dangerousGood.Weight.ToString());
			AssertEquals("1.00 KG", dangerousGood.NetExplosiveWeight.ToString());
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight needs to be greater than 0 for UNDG class 1 substances.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be negative.");
			AssertHasMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be higher than DG gross weight.");

			dangerousGood.NetExplosiveWeight.Value = 0.1m;

			AssertEquals("100.00 G", dangerousGood.Weight.ToString());
			AssertEquals("0.10 KG", dangerousGood.NetExplosiveWeight.ToString());
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight needs to be greater than 0 for UNDG class 1 substances.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be negative.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be higher than DG gross weight.");

			undg.Substance.DG_Class = "0.4G";

			builder = new AdvancedLogisticsPortOrderBuilder(consol);
			advancedLogisticsPortOrder = builder.Build();
			packingLine = advancedLogisticsPortOrder.Shipments.ElementAtOrDefault(0).PackingLines.ElementAt(0);
			dangerousGood = packingLine.DangerousGoods.ElementAt(0);

			AssertEquals(0m, dangerousGood.NetExplosiveWeight.Value);
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight needs to be greater than 0 for UNDG class 1 substances.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be negative.");
			AssertNoMessageError(dangerousGood.NetExplosiveWeight.ValueInfo, "Net Explosive Weight can't be higher than DG gross weight.");
		}

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var advancedLogisticsPortOrder = new AdvancedLogisticsPortOrderBuilder(consol).Build();
			AssertEquals(Constants.ContainerModes.FCL, advancedLogisticsPortOrder.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, advancedLogisticsPortOrder.ContainerMode.Description);
		}

		#endregion
		#region Implement

		protected override void SetUp()
		{
			base.SetUp();
			CreateRefVessels();
		}

		ForwardingConsol CreateConsol(string loadPort = "FRPAR", string dischargePort = "BEANR")
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			CreateAddresses(consol);

			return consol;
		}

		void CreateExportTransport(ForwardingConsol consol, byte legOrder = 3)
		{
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = legOrder;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "DEBRE";
			transport.JW_RL_NKDiscPort = "CNYTN";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2021, 01, 24, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2021, 01, 26, 12, 15, 00);
		}

		void CreateTransport(ForwardingConsol consol, string loadPort, string discPort, byte legOrder = 1)
		{
			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = legOrder;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.Other;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2021, 01, 24, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2021, 01, 26, 12, 15, 00);
		}

		void AddMainTransport(ForwardingConsol consol, string loadPort, string discPort, byte legOrder = 2)
		{
			var mainTransport = consol.Transports.AddNew();
			mainTransport.JW_LegOrder = legOrder;
			mainTransport.JW_TransportMode = Constants.TransportModes.Sea;
			mainTransport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			mainTransport.JW_RL_NKLoadPort = loadPort;
			mainTransport.JW_RL_NKDiscPort = discPort;
			mainTransport.JW_ETD = new ZDateTime(2021, 01, 23, 7, 35, 00);
			mainTransport.JW_ETA = new ZDateTime(2021, 01, 25, 15, 55, 00);
			mainTransport.JW_Vessel = "MSC UBERTY";
			mainTransport.JW_VoyageFlight = "12401";
		}

		Freight.Business.Transport AddExtraTransport(ForwardingConsol consol, ZString transportType, ZString transportMode, string loadPort, string discPort, byte legOrder)
		{
			var preCarriageTransport = consol.Transports.AddNew();
			preCarriageTransport.JW_LegOrder = legOrder;
			preCarriageTransport.JW_TransportMode = transportMode;
			preCarriageTransport.JW_TransportType = transportType;
			preCarriageTransport.JW_RL_NKLoadPort = loadPort;
			preCarriageTransport.JW_RL_NKDiscPort = discPort;
			preCarriageTransport.JW_ETD = new ZDateTime(2021, 01, 21, 7, 35, 00);
			preCarriageTransport.JW_ETA = new ZDateTime(2021, 01, 22, 15, 55, 00);
			switch (transportMode)
			{
				case Constants.TransportModes.Road:
					preCarriageTransport.JW_Vessel = "TRAILER";
					preCarriageTransport.JW_VoyageFlight = "1-TRU-CK1";
					break;
				case Constants.TransportModes.Rail:
					preCarriageTransport.JW_Vessel = "JOURNEY REF";
					preCarriageTransport.JW_VoyageFlight = "COUCH124";
					break;
				case Constants.TransportModes.Sea:
					preCarriageTransport.JW_Vessel = "MSC POOLSTER";
					preCarriageTransport.JW_VoyageFlight = "124";
					break;
			}

			return preCarriageTransport;
		}

		void CreateRefVessels()
		{
			var vesselSea = Factory.NewWithValidTestData<RefVessel>();
			vesselSea.RV_Name = "COSCO NEBULA";
			vesselSea.RV_LloydsNumber = "9795622";
			vesselSea.RV_VesselType = "CV";
			vesselSea.RV_RN_NKCountryOfReg = "HK";
			vesselSea.RV_RadioCallSign = "VRRW8";

			var vesselSea2 = Factory.NewWithValidTestData<RefVessel>();
			vesselSea2.RV_Name = "MSC UBERTY";
			vesselSea2.RV_LloydsNumber = "489541";
			vesselSea2.RV_VesselType = "CV";
			vesselSea2.RV_RN_NKCountryOfReg = "DE";
			vesselSea2.RV_RadioCallSign = "OP5DR";

			var vesselBarge = Factory.NewWithValidTestData<RefVessel>();
			vesselBarge.RV_Name = "MSC POOLSTER";
			vesselBarge.RV_VesselType = "BA";
			vesselBarge.RV_RN_NKCountryOfReg = "BE";
			vesselBarge.RV_RadioCallSign = "OT5325";
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
			carrier.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "Sending Forwarder";
			sendingForwarder.OH_RL_NKClosestPort = "BEANR";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Antwerp";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Receiving Forwarder";
			receivingForwarder.OH_RL_NKClosestPort = "BEANR";
			receivingForwarder.MainAddress.Address1 = "Unit 13";
			receivingForwarder.MainAddress.Address2 = "4 Lost Lane";
			receivingForwarder.MainAddress.City = "Antwerp";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "BE";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

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
		}

		void CreateBasicGoodsAndEquipment(ForwardingConsol consol)
		{
			var container = CreateContainer(consol);

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TS1";
			consignee.OH_FullName = "Test1 Name";

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TS2";
			consignor.OH_FullName = "Test2 Name";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001816";
			shipment1.JS_HouseBill = "S00001816";
			shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment1.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "BEANR";
			shipment1.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment1.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment1.JS_ShippedOnBoard = "SHP";
			shipment1.JS_ShippedOnBoardDate = ZDate.Today;
			shipment1.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment1.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment1.JS_GoodsDescription = "goods description";
			shipment1.JS_MarksAndNumbers = "marks & numbers";
			shipment1.JS_BookingReference = "BKG000001";
			shipment1.JS_NoOriginalBills = 1;
			shipment1.JS_NoCopyBills = 2;
			shipment1.ConsigneePK = consignee.PK;
			shipment1.ConsignorPK = consignor.PK;

			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 8;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 18;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 9;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "ROOF COVERING";
			packline1.JL_OutturnComment = "Outturn Comment";
			packline1.JL_MarksAndNumbers = "MarksAndNosPL1";
			packline1.JL_DetailedDescription = "";
			packline1.JL_ContainerPackingOrder = 3;
			packline1.JL_Calc_ContainerNumber = "MSCU1245787";
			packline1.JL_JC = container.PK;
			packline1.JL_RH_NKCommodityCode = "1248";
			packline1.JL_RefNumber = "MRN00001";
			packline1.JL_ImportRefNumber = "IMP001";
			packline1.JL_ExportRefNumber = "EXP001";
			packline1.JL_HarmonisedCode = "HC Code";

			var commodity = Factory.NewWithValidTestData<RefCommodityCode>();
			commodity.RH_Code = "1248";
			var map = commodity.RefCommodityCodeMaps.AddNew();
			map.LC_RH_NKCommodityCode = "1248";
			map.LC_LocalCode = "1234";
			map.LC_RN_NKCountry = Constants.CountryCodes.Germany;
			map.LC_LocalCodeProvider = DELocalCommodityCodeProviderList.Codes.DE_DBH;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0503", "b", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "0503";
				subs.DG_Variant = "b";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.Substance.DG_ExceptedQuantityCode = "E0";
			undg1.Substance.DG_PG = "II";
			undg1.Substance.DG_PSN = "AIR BAG MODULES";
			undg1.Substance.DG_EMS = "F-I,S-S";
			undg1.Substance.DG_Class = "1.4G";
			undg1.Substance.DG_SubLabel1 = "SL1";

			undg1.DI_IsCombustible = false;
			undg1.DI_DGFlashPoint = 85m;
			undg1.DI_TechnicalName = "Airbag Mercedes C";
			undg1.DI_MPMarinePollutant = "";
			undg1.DI_DGVolume = 0m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 142m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_IsLimitedQuantity = false;
			undg1.DI_PackageCount = 6;
			undg1.DI_F3_NKPackType = "BOX";
			undg1.DI_OC_DGContact = contact.PK;

			container.PackLines.Add(packline1);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001817";
			shipment2.JS_HouseBill = "S00001817";
			shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment2.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "BEANR";
			shipment2.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment2.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment2.JS_ShippedOnBoard = "SHP";
			shipment2.JS_ShippedOnBoardDate = ZDate.Today;
			shipment2.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment2.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment2.JS_GoodsDescription = "goods description 2";
			shipment2.JS_MarksAndNumbers = "marks & numbers 2";
			shipment2.JS_BookingReference = "BKG000001";
			shipment2.JS_NoOriginalBills = 1;
			shipment2.JS_NoCopyBills = 2;

			shipment2.OuterPackLines.RemoveAndDeleteAll();

			var packline2 = shipment2.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 16;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 36;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 18;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "ROOF COVERING EXTRA";
			packline2.JL_OutturnComment = "Outturn Comment";
			packline2.JL_MarksAndNumbers = "MarksAndNosPL2";
			packline2.JL_DetailedDescription = "";
			packline2.JL_ContainerPackingOrder = 3;
			packline2.JL_RH_NKCommodityCode = "1248";
			packline2.JL_RefNumber = "MRN00002";
			packline2.JL_Calc_ContainerNumber = "";
		}

		ForwardingContainer CreateContainer(ForwardingConsol consol)
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = "20GP";
			refContainer.RC_ISOType = "22P2";
			refContainer.RC_TareWeight = 954;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "MSCU1245787";
			container.JC_DeliveryMode = "CY/CY";
			container.JC_IsEmptyContainer = false;
			container.JC_GrossWeightUQ = "KG";
			container.JC_GrossWeight = 1950;
			container.JC_RC = refContainer.PK;
			container.JC_SealNum = "Seal1";
			container.JC_AdditionalSealNum = "Seal2";
			container.JC_Additional2SealNum = "Seal3";
			return container;
		}

		void CreateBasicGoodsWithoutEquipment(ForwardingConsol consol)
		{
			var shipment = CreateShipment(consol);
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 8;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 18;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 9;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Description = "ROOF COVERING";
			packline1.JL_OutturnComment = "Outturn Comment";
			packline1.JL_DetailedDescription = "";
			packline1.JL_ContainerPackingOrder = 3;
			packline1.JL_Calc_ContainerNumber = "MSCU1245787";
			packline1.JL_RH_NKCommodityCode = "1248";
			packline1.JL_RefNumber = "MRN00001";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0503", "b", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "0503";
				subs.DG_Variant = "b";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			undg1.Substance.DG_ExceptedQuantityCode = "E0";
			undg1.Substance.DG_PG = "II";
			undg1.Substance.DG_PSN = "AIR BAG MODULES";
			undg1.Substance.DG_EMS = "F-I,S-S";
			undg1.Substance.DG_Class = "1.4G";
			undg1.Substance.DG_SubLabel1 = "SL1";

			undg1.DI_IsCombustible = true;
			undg1.DI_DGFlashPoint = 85m;
			undg1.DI_TechnicalName = "Airbag Mercedes C";
			undg1.DI_MPMarinePollutant = "";
			undg1.DI_DGVolume = 0m;
			undg1.DI_UnitOfVolume = "M3";
			undg1.DI_DGWeight = 142m;
			undg1.DI_UnitOfWeight = "KG";
			undg1.DI_IsLimitedQuantity = false;
			undg1.DI_PackageCount = 6;
			undg1.DI_F3_NKPackType = "BOX";
			undg1.DI_OC_DGContact = contact.PK;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 16;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 36;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 18;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Description = "ROOF COVERING ADDITIONAL PARTS";
			packline2.JL_OutturnComment = "Outturn Comment 2";
			packline2.JL_DetailedDescription = "";
			packline2.JL_ContainerPackingOrder = 3;
			packline2.JL_Calc_ContainerNumber = "MSCU1245787";
			packline2.JL_RH_NKCommodityCode = "1248";
			packline2.JL_RefNumber = "MRN00002";
		}

		ForwardingShipment CreateShipment(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();

			shipment.JS_UniqueConsignRef = "S00001816";
			shipment.JS_HouseBill = "S00001816";
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "BEANR";
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(2);
			shipment.JS_GoodsDescription = "goods description";
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_NoOriginalBills = 1;
			shipment.JS_NoCopyBills = 2;

			return shipment;
		}

		ForwardingPackLine AddPackingLine(ForwardingShipment shipment, bool addDangerousGoods, bool populateDGPacks, ZString packType, ZInt packCount, bool populateDGWeight, ZString wUnit, ZDecimal weight, bool populateDGVolume, ZString vUnit, ZDecimal volume)
		{
			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.JL_PackageCount = 8;
			packingLine.JL_F3_NKPackType = "PLT";
			packingLine.JL_ActualWeight = 18;
			packingLine.JL_ActualWeightUQ = "KG";
			packingLine.JL_ActualVolume = 9;
			packingLine.JL_ActualVolumeUQ = "M3";
			packingLine.JL_Description = "ROOF COVERING";
			packingLine.JL_OutturnComment = "Outturn Comment";
			packingLine.JL_DetailedDescription = "";
			packingLine.JL_ContainerPackingOrder = 3;
			packingLine.JL_Calc_ContainerNumber = "MSCU1245787";
			packingLine.JL_RH_NKCommodityCode = "1248";
			packingLine.JL_RefNumber = "MRN00001";

			if (addDangerousGoods)
			{
				AddDangerousGoods(packingLine, populateDGPacks, packType, packCount, populateDGWeight, wUnit, weight, populateDGVolume, vUnit, volume);
			}

			return packingLine;
		}

		UNDGDataItem AddDangerousGoods(ForwardingPackLine packingLine, bool populateDGPacks, ZString packType, ZInt packCount, bool populateDGWeight, ZString wUnit, ZDecimal weight, bool populateDGVolume, ZString vUnit, ZDecimal volume)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0503", "b", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "0503";
				subs.DG_Variant = "b";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}

			var undg = packingLine.UNDGs.AddNew();
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			undg.Substance.DG_ExceptedQuantityCode = "E0";
			undg.Substance.DG_PG = "II";
			undg.Substance.DG_PSN = "AIR BAG MODULES";
			undg.Substance.DG_EMS = "F-I,S-S";
			undg.Substance.DG_Class = "1.4G";
			undg.Substance.DG_SubLabel1 = "SL1";

			undg.DI_IsCombustible = true;
			undg.DI_DGFlashPoint = 85m;
			undg.DI_TechnicalName = "Airbag Mercedes C";
			undg.DI_MPMarinePollutant = "";
			undg.DI_IsLimitedQuantity = false;
			undg.DI_OC_DGContact = contact.PK;

			if (populateDGVolume)
			{
				undg.DI_DGVolume = volume;
				undg.DI_UnitOfVolume = vUnit;
			}

			if (populateDGWeight)
			{
				undg.DI_DGWeight = weight;
				undg.DI_UnitOfWeight = wUnit;
			}

			if (populateDGPacks)
			{
				undg.DI_PackageCount = packCount;
				undg.DI_F3_NKPackType = packType;
			}

			return undg;
		}

		#endregion
	}
}
