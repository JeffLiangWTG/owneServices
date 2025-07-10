using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class JobDeclarationSynchroniserTest : SynchroniserTestCase
	{
		public virtual void TestShouldSynchroniseContainerModeWithPackingMode()
		{
			decSynchroniser = new JobDeclarationSynchroniserForTest(declaration);
			Assert(decSynchroniser.ShouldSynchroniseContainerModeWithPackingMode);
		}

		public virtual void TestSynchroniserBuyersConsolLead()
		{
			ZString localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.RN_Code)).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRANY";
			consol.JK_RL_NKDischargePort = localPort;
			consol.JK_MasterBillNum = "M1234567";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "LB";
			shipment.JS_OuterPacks = 100;
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_ActualVolume = 5;

			var shipment1 = shipment.CoLoadShipments.AddNew();
			shipment1.JS_OuterPacks = 200;
			shipment1.JS_ActualWeight = 2000;
			shipment1.JS_UnitOfWeight = "LB";
			shipment1.JS_F3_NKPackType = "PLT";
			shipment1.JS_ActualVolume = 1.75;

			var shipment2 = shipment.CoLoadShipments.AddNew();
			shipment2.JS_OuterPacks = 300;
			shipment2.JS_ActualWeight = 3000;
			shipment2.JS_UnitOfWeight = "LB";
			shipment2.JS_F3_NKPackType = "PLT";
			shipment2.JS_ActualVolume = 3.8;
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise();
			AssertEquals((decimal)(1000 + 2000 + 3000), declaration.JE_TotalWeight);
			AssertEquals("LB", declaration.JE_TotalWeightUnit);
			AssertEquals(100 + 200 + 300, declaration.JE_TotalNoOfPacks);
			AssertEquals(ExpectedJE_TotalNoOfPacksPackTypeForTestSynchroniserBuyersConsolLead, declaration.JE_TotalNoOfPacksPackType);

			// Changes are propagated
			shipment.JS_ActualWeight = 1001;
			shipment2.JS_OuterPacks = 303;
			AssertEquals((decimal)(1001 + 2000 + 3000), declaration.JE_TotalWeight);
			AssertEquals(100 + 200 + 303, declaration.JE_TotalNoOfPacks);
			AssertEquals("JE_TotalVolume", (decimal)(5 + 1.75 + 3.8), declaration.JE_TotalVolume);

			//mixed mass units are converted to KG
			shipment.JS_UnitOfWeight = "G";
			AssertEquals(2268.963m, declaration.JE_TotalWeight);  // Equals (1001 / 1000) + (2000 / 2.205) + (3000 / 2.205)
			AssertEquals("KG", declaration.JE_TotalWeightUnit);

			//mixed pack units are ignored 
			shipment2.JS_F3_NKPackType = "PAI";
			AssertEquals(0, declaration.JE_TotalNoOfPacks);
			AssertEquals("", declaration.JE_TotalNoOfPacksPackType);

			//mixed volume units should be converted to M3
			shipment2.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			AssertEquals("JE_TotalVolume", 6.858m, declaration.JE_TotalVolume);
			AssertEquals("M3", declaration.JE_TotalVolumeUnit);
		}

		protected virtual string ExpectedJE_TotalNoOfPacksPackTypeForTestSynchroniserBuyersConsolLead
		{
			get { return "PLT"; }
		}

		public void TestJobDocAddressSynchroniserDoesNotTouchDeletedObjectsProperties()
		{
			CombineAssertions(delegate
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var declaration = GetJobDeclaration();
				declaration.JE_JS = shipment.PK;
				var sourceJDA = shipment.GetArrivalCTODocAddress;
				var destJDA = declaration.DepotDocAddress;
				var jobDocAddressSyncher = new JobDocAddressSynchroniser(destJDA, sourceJDA);
				AssertNoExceptionThrown("Should see no exception when synching", delegate
				{ jobDocAddressSyncher.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force)); });
				destJDA.Delete();
				AssertNoExceptionThrown("Should see no exception when synching to deleted destination JDA", delegate
				{ jobDocAddressSyncher.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force)); });
				sourceJDA.Delete();
				AssertNoExceptionThrown("Should see no exception when synching from deleted source JDA", delegate
				{ jobDocAddressSyncher.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force)); });
			});
		}

		public void TestShipmentsDropModeIsSynchronisedVerbatim_Import()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var localCountry = declaration.CountryCode;
			var sydney = localCountry + "SYD";
			var heathrow = localCountry == "GB" ? "XXLHR" : "GBLHR";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = sydney;
			consignor.OH_RL_NKClosestPort = heathrow;
			Shipment.Consols.RemoveAll();
			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPK = consignor.PK;
			Shipment.JS_RL_NKOrigin = heathrow;
			Shipment.JS_RL_NKDestination = sydney;
			Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "ABC";
			Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "DEF";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("ABC", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
		}

		public void TestShipmentsDropModeIsSynchronisedVerbatim_Export()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var localCountry = declaration.CountryCode;
			var sydney = localCountry + "SYD";
			var heathrow = localCountry == "GB" ? "XXLHR" : "GBLHR";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = heathrow;
			consignor.OH_RL_NKClosestPort = sydney;
			Shipment.Consols.RemoveAll();
			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPK = consignor.PK;
			Shipment.JS_RL_NKOrigin = sydney;
			Shipment.JS_RL_NKDestination = heathrow;
			Shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "ABC";
			Shipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded = "DEF";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("DEF", declaration.JE_FCLDeliveryOrPickupEquipmentNeeded);
		}

		public void TestScreeningStatusSyncInitiated()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals(ScreeningStatusesList.Codes.Matched, declaration.JE_ScreeningStatus);
		}

		public virtual void TestLoadAndDischargeSyncroniseToCorrectTransport_Import()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			ZString localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode)).Code;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "KRANY";
			consol.JK_RL_NKDischargePort = localPort;

			Transport firstInternationalLegTransport = consol.Transports[0];
			firstInternationalLegTransport.JW_RL_NKLoadPort = "KRANY";
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			Transport secondInternationalLegTransport = consol.Transports.AddNew();
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = localPort;
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(7);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(8);
			secondInternationalLegTransport.JW_LegOrder = 2;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			declaration.JE_RL_NKPortOfLoading = "";
			declaration.JE_RL_NKPortOfArrival = "";

			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(true);

			AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", "JPHIU", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", localPort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("JE_ExportDate is that of the first international leg's departure", ZDateTime.BrettsBirthday.AddDays(7), declaration.JE_ExportDate);
			AssertEquals("JE_DateOfArrival is that of the last international leg's arrival", ZDateTime.BrettsBirthday.AddDays(8), declaration.JE_DateOfArrival);
		}

		public void TestCarrierExport()
		{
			var declaration = GetJobDeclaration();
			var country = declaration.CountryCode;
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, country)).Code;
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, country)).Code;
			var foreignPort2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { country, foreignPort.Left(2) })).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = localPort;
			consol.JK_RL_NKDischargePort = foreignPort2;
			var mainShippingLine = Factory.New<OrgHeader>();
			var mainShippingLineAddress = mainShippingLine.Addresses.AddNewMainAddress();
			var legOneShippingLine = Factory.New<OrgHeader>();
			var legTwoShippingLine = Factory.New<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = mainShippingLineAddress.PK;
			consol.Transports.RemoveAndDeleteAll();
			var legOne = consol.Transports.AddNew();
			var legTwo = consol.Transports.AddNew();
			legOne.CarrierPK = legOneShippingLine.PK;
			legOne.JW_RL_NKLoadPort = localPort;
			legOne.JW_RL_NKDiscPort = foreignPort;
			legOne.JW_TransportType = "PRE";
			legTwo.CarrierPK = legTwoShippingLine.PK;
			legTwo.JW_RL_NKLoadPort = foreignPort;
			legTwo.JW_RL_NKDiscPort = foreignPort2;
			legTwo.JW_TransportType = "FL1";
			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = "EXP";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			AssertEquals("Carrier for declaration is the carrier of the interesting leg", legOneShippingLine.PK, declaration.JE_OH_ShippingLine);
			legOne.CarrierPK = ZGuid.Empty;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			AssertEquals("Carrier for declaration is the carrier of consol because the interesting leg's carrier is not defined", mainShippingLine.PK, declaration.JE_OH_ShippingLine);
		}

		public void TestCarrierImport()
		{
			var declaration = GetJobDeclaration();
			var country = declaration.CountryCode;
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, country)).Code;
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, country)).Code;
			var foreignPort2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { country, foreignPort.Left(2) })).Code;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = foreignPort2;
			consol.JK_RL_NKDischargePort = localPort;
			var mainShippingLine = Factory.New<OrgHeader>();
			var mainShippingLineAddress = mainShippingLine.Addresses.AddNewMainAddress();
			var legOneShippingLine = Factory.New<OrgHeader>();
			var legTwoShippingLine = Factory.New<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = mainShippingLineAddress.PK;
			consol.Transports.RemoveAndDeleteAll();
			var legOne = consol.Transports.AddNew();
			var legTwo = consol.Transports.AddNew();
			legOne.CarrierPK = legOneShippingLine.PK;
			legOne.JW_RL_NKLoadPort = foreignPort2;
			legOne.JW_RL_NKDiscPort = foreignPort;
			legOne.JW_TransportType = "PRE";
			legTwo.CarrierPK = legTwoShippingLine.PK;
			legTwo.JW_RL_NKLoadPort = foreignPort;
			legTwo.JW_RL_NKDiscPort = localPort;
			legTwo.JW_TransportType = "FL1";
			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = "IMP";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			AssertEquals("Carrier for declaration is the carrier of the interesting leg", legTwoShippingLine.PK, declaration.JE_OH_ShippingLine);
			legTwo.CarrierPK = ZGuid.Empty;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			AssertEquals("Carrier for declaration is the carrier of consol because the interesting leg's carrier is not defined", mainShippingLine.PK, declaration.JE_OH_ShippingLine);
		}

		public void TestVesselVoyageSyncroniseToCorrectTransport()
		{
			var code = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, code)).Code;
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, code)).Code;
			var foreignPort1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { code, foreignPort.Left(2) })).Code;

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = foreignPort;
			consol.JK_RL_NKDischargePort = localPort;

			var transport0 = consol.Transports[0];
			transport0.JW_RL_NKLoadPort = foreignPort;
			transport0.JW_RL_NKDiscPort = foreignPort1;
			transport0.JW_Vessel = "1111";
			transport0.JW_VoyageFlight = "1";
			transport0.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = foreignPort1;
			transport1.JW_RL_NKDiscPort = localPort;
			transport1.JW_Vessel = "2222";
			transport1.JW_VoyageFlight = "2";
			transport1.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = localPort;
			transport2.JW_RL_NKDiscPort = localPort;
			transport2.JW_Vessel = "3333";
			transport2.JW_VoyageFlight = "3";
			transport2.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			shipment = consol.Shipments.AddNew();
			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			AssertEquals("Vessel", "2222", declaration.JE_VesselName);
			AssertEquals("Voyage", "2", declaration.JE_VoyageFlightNo);
		}

		public void TestVesselVoyageSyncroniseToCorrectTransportForExport()
		{
			var declaration = GetJobDeclaration();
			var countryCode = declaration.CountryCode;
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)).Code;

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, localPort);
			query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode);
			var localPort1 = Factory.LoadTop1<RefUNLOCO>(query).Code;

			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, countryCode)).Code;

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = localPort;
			consol.JK_RL_NKDischargePort = foreignPort;

			var transport0 = consol.Transports[0];
			transport0.JW_RL_NKLoadPort = localPort;
			transport0.JW_RL_NKDiscPort = localPort1;
			transport0.JW_Vessel = "1111";
			transport0.JW_VoyageFlight = "1";
			transport0.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = localPort1;
			transport1.JW_RL_NKDiscPort = foreignPort;
			transport1.JW_Vessel = "2222";
			transport1.JW_VoyageFlight = "2";
			transport1.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			AssertEquals("Vessel", "2222", declaration.JE_VesselName);
			AssertEquals("Voyage", "2", declaration.JE_VoyageFlightNo);
		}

		public void TestOriginDestinationDefaultedOnlyFromShipment()
		{
			var declaration = GetJobDeclaration();
			var countryCode = declaration.CountryCode;
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode)).Code;

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, localPort);
			query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode);
			var localPort1 = Factory.LoadTop1<RefUNLOCO>(query).Code;

			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, countryCode)).Code;

			query = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, foreignPort);
			query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, countryCode);
			var foreignPort1 = Factory.LoadTop1<RefUNLOCO>(query).Code;

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = localPort;
			consol.JK_RL_NKDischargePort = foreignPort;

			var transport0 = consol.Transports[0];
			transport0.JW_RL_NKLoadPort = localPort;
			transport0.JW_RL_NKDiscPort = foreignPort;
			transport0.JW_Vessel = "1111";
			transport0.JW_VoyageFlight = "1";
			transport0.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = localPort1;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = foreignPort1;

			shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			shipment.JS_RL_NKOrigin = localPort;
			shipment.JS_RL_NKDestination = foreignPort;

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Origin should be defaulted from Shipment.Origin", localPort, declaration.JE_RL_NKOrigin);
			AssertEquals("Destination should be defaulted from Shipment.Destination", foreignPort, declaration.JE_RL_NKFinalDestination);
		}

		public virtual void TestLoadAndDischargeSyncroniseToCorrectTransport_Export()
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			ZString localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode)).Code;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = localPort;
			consol.JK_RL_NKDischargePort = "KRANY";

			Transport firstInternationalLegTransport = consol.Transports[0];
			firstInternationalLegTransport.JW_RL_NKLoadPort = localPort;
			firstInternationalLegTransport.JW_RL_NKDiscPort = "JPHIU";
			firstInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday;
			firstInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(1);
			firstInternationalLegTransport.JW_LegOrder = 1;

			Transport secondInternationalLegTransport = consol.Transports.AddNew();
			secondInternationalLegTransport.JW_RL_NKLoadPort = "JPHIU";
			secondInternationalLegTransport.JW_RL_NKDiscPort = "KRANY";
			secondInternationalLegTransport.JW_ETD = ZDateTime.BrettsBirthday.AddDays(7);
			secondInternationalLegTransport.JW_ETA = ZDateTime.BrettsBirthday.AddDays(8);
			secondInternationalLegTransport.JW_LegOrder = 2;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			AssertEquals("Precondition: Declaration.JE_RL_NKPortOfLoading", "", declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Precondition: Declaration.JE_RL_NKPortOfArrival", "", declaration.JE_RL_NKPortOfArrival);

			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(true);

			AssertEquals("Declaration.JE_RL_NKPortOfLoading is first international leg's load", localPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Declaration.JE_RL_NKPortOfArrival is last international leg's discharge", "JPHIU", declaration.JE_RL_NKPortOfArrival);
			AssertEquals("JE_ExportDate is that of the first international leg's departure", ZDateTime.BrettsBirthday, declaration.JE_ExportDate);
			AssertEquals("JE_DateOfArrival is that of the last international leg's arrival", ZDateTime.BrettsBirthday.AddDays(1), declaration.JE_DateOfArrival);
		}

		//TODO: Move to CusContainerCollectionSynchroniserTest
		public void TestSynchroniseContainerCount()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";
			ForwardingContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";
			Factory.Save();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1";

			shipment.JS_F3_NKPackType = "123";
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 12;
			packLine1.JL_F3_NKPackType = "AA";
			packLine1.JL_JC = container1.PK;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 14;
			packLine2.JL_F3_NKPackType = "BB";
			packLine2.JL_JC = container2.PK;

			BaseJobDeclaration declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			AssertEquals("Pre Condition", 2, consol.JK_Calc_ContainerCount);
			AssertEquals("Pre Condition", 2, shipment.JS_Calc_ContainerCount);
			AssertEquals("Pre Condition", 0, declaration.JE_ContainerCount.ToZInt());
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Should have synchronised", 2, declaration.JE_ContainerCount.ToZInt());
		}

		public void TestDoesNotSynchroniseWhenOverrideIsSet()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1";
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_F3_NKPackType = "AA";

			declaration.JE_OverrideFreightDefaults = true;

			AssertEquals("Declaration should not have been synchronised", 0, declaration.Bills.Count);
		}

		public virtual void TestSynchronise_FromShipmentOtherDetails()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;

			ZQuery uSConsignorFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			uSConsignorFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "USLAX");
			var partyInCurrentCountry = Factory.LoadTop1<OrgHeader>(new ZQuery());
			partyInCurrentCountry.OH_RL_NKClosestPort = declaration.CountryCode + "AAA";
			partyInCurrentCountry.OH_IsConsignee = true;

			Shipment.ConsigneePK = partyInCurrentCountry.PK;
			Shipment.ConsignorPK = Factory.LoadTop1<OrgHeader>(uSConsignorFilter).PK;
			Shipment.JS_GoodsDescription = "TEST DESCRIPTION OF THE GOODS";
			Shipment.JS_TransportMode = "AIR";
			Shipment.JS_E_ARV = new ZDateTime(2003, 12, 31);
			Shipment.JS_E_DEP = new ZDateTime(2003, 12, 30);
			Shipment.JS_ShippedOnBoardDate = new ZDateTime(2003, 12, 29);
			Shipment.JS_HouseBill = "H123456";
			Shipment.JS_ActualWeight = 10;
			Shipment.JS_UnitOfWeight = "KG";
			Shipment.JS_ActualVolume = 3;
			Shipment.JS_UnitOfVolume = "M3";
			Shipment.JS_OuterPacks = 2;
			Shipment.JS_F3_NKPackType = "PLT";
			Shipment.JS_TotalPackageCount = 3;//inner pack
			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;

			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Goods Description", Shipment.JS_GoodsDescription, declaration.JE_GoodsDescription);
			AssertEquals("Consignee", Shipment.ConsigneePK, declaration.JE_OH_Importer);
			AssertEquals("Consignor", Shipment.ConsignorPK, declaration.JE_OH_Supplier);
			AssertEquals("HouseBill", Shipment.JS_HouseBill, declaration.JE_HouseBill);
			AssertEquals("ActualWeight", Shipment.JS_ActualWeight, declaration.JE_TotalWeight);
			AssertEquals("Unit of Weight", Shipment.JS_UnitOfWeight, declaration.JE_TotalWeightUnit);
			AssertEquals("ActualVolume", Shipment.JS_ActualVolume, declaration.JE_TotalVolume);
			AssertEquals("Unit of Volume", Shipment.JS_UnitOfVolume, declaration.JE_TotalVolumeUnit);
			AssertEquals("Outer packs", Shipment.JS_OuterPacks, declaration.JE_TotalNoOfPacks);
			AssertEquals("Outer pack type", Shipment.JS_F3_NKPackType, declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("Origin port", Shipment.JS_RL_NKOrigin, declaration.JE_RL_NKOrigin);
			AssertEquals("Destination port", Shipment.JS_RL_NKDestination, declaration.JE_RL_NKFinalDestination);
		}

		public void TestSynchroniseHouseBills()
		{
			ForwardingShipment masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Master";

			ForwardingShipment sub1 = masterShipment.CoLoadShipments.AddNew();
			sub1.JS_HouseBill = "Sub1";

			ForwardingShipment sub2 = masterShipment.CoLoadShipments.AddNew();
			sub2.JS_HouseBill = "Sub2";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = masterShipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("PreCondition:Two sub shipments", 2, masterShipment.CoLoadShipments.Count);
			if (declaration.IsPackingInformationRelevant)
			{
				AssertEquals("2 house bills", 2, declaration.Bills.Count);
				AssertNotNull("house bill no synchronised", declaration.Bills.FindByBillNumberAndType("Sub1", BillTypeList.Codes.HouseBill));
				AssertNotNull("house bill no synchronised", declaration.Bills.FindByBillNumberAndType("Sub2", BillTypeList.Codes.HouseBill));
			}
			else
			{
				AssertEquals("1 house bill", 1, declaration.Bills.Count);
				AssertNotNull("house bill no synchronised", declaration.Bills.FindByBillNumberAndType("Sub1", BillTypeList.Codes.HouseBill));
			}
		}

		public void TestSynchroniseHouseBillsWithHLSShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			shipment.JS_HouseBill = "S342089";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("HLS house bill number should be copied", 1, declaration.Bills.Count);
			AssertNotNull("house bill synchronised", declaration.Bills.FindByBillNumberAndType(shipment.JS_HouseBill, BillTypeList.Codes.HouseBill));
		}

		public void TestSynchroniseHouseBills_Master()
		{
			ForwardingShipment masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Master";

			ForwardingShipment sub1 = masterShipment.CoLoadShipments.AddNew();
			sub1.JS_HouseBill = "Sub1";

			ForwardingShipment sub2 = masterShipment.CoLoadShipments.AddNew();
			sub2.JS_HouseBill = "Sub2";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = masterShipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("PreCondition:Two sub shipments", 2, masterShipment.CoLoadShipments.Count);
			if (declaration.IsPackingInformationRelevant)
			{
				AssertEquals("2 house bills", 2, declaration.Bills.Count);
				AssertNull("house bill no NOT synchronised for master", declaration.Bills.FindByBillNumberAndType(masterShipment.JS_HouseBill, BillTypeList.Codes.HouseBill));
				AssertNotNull("house bill no synchronised", declaration.Bills.FindByBillNumberAndType(sub1.JS_HouseBill, BillTypeList.Codes.HouseBill));
				AssertNotNull("house bill no synchronised", declaration.Bills.FindByBillNumberAndType(sub2.JS_HouseBill, BillTypeList.Codes.HouseBill));
			}
			else
			{
				AssertEquals("1 house bill", 1, declaration.Bills.Count);
				AssertNull("house bill no NOT synchronised for master", declaration.Bills.FindByBillNumberAndType(masterShipment.JS_HouseBill, BillTypeList.Codes.HouseBill));
				AssertNotNull("house bill no synchronised", declaration.Bills.FindByBillNumberAndType(sub1.JS_HouseBill, BillTypeList.Codes.HouseBill));
			}
		}

		public void TestSynchroniseHouseBillsWithOneBillOnlyOnBuyersConsolThenAddASecondBill()
		{
			var primaryShipment = Factory.New<ForwardingShipment>();
			primaryShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			primaryShipment.JS_HouseBill = "PRIMARY";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = primaryShipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			var secondaryShipment = Factory.New<ForwardingShipment>();
			secondaryShipment.JS_HouseBill = "SECONDARY";
			primaryShipment.CoLoadShipments.Add(secondaryShipment);

			AssertEquals("Precondition:", secondaryShipment.JS_JS_ColoadMasterShipment, primaryShipment.PK);

			((BusinessObjectCollection)declaration.Bills).Sort(CusDecHouseBillSchema.CU_BillNum.Name);
			var bills = new ZStringBuilder();
			foreach (Bill bill in declaration.Bills)
			{
				bills.Append(bill.CU_BillTypeAndNum);
			}

			if (declaration.IsPackingInformationRelevant)
			{
				AssertEquals("Declaration.Bills needs both Primary and Secondary for Buyer's consol", "HB:PRIMARY, HB:SECONDARY", bills.ToStringWithDelimiterBetweenAppends(", "));
			}
			else
			{
				AssertEquals("Declaration.Bills needs only Primary for Buyer's consol", "HB:PRIMARY", bills.ToStringWithDelimiterBetweenAppends(", "));
			}
		}

		public void TestOngoingSynchronisationOfHouseBills()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			masterShipment.JS_HouseBill = "Consolidation";

			ForwardingShipment sub1 = consol.Shipments.AddNew();
			sub1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			sub1.JS_HouseBill = "Sub1";

			ForwardingShipment sub2 = consol.Shipments.AddNew();
			sub2.JS_JS_ColoadMasterShipment = masterShipment.PK;
			sub2.JS_HouseBill = "Sub2";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = masterShipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			if (declaration.IsPackingInformationRelevant)
			{
				AssertEquals("2 house bills - 1 for each sub", 2, declaration.Bills.Count);
			}
			else
			{
				AssertEquals("only 1 house bill", 1, declaration.Bills.Count);
			}

			sub2.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			AssertEquals("PreCondition:1 sub-shipment for the master", 1, masterShipment.CoLoadShipments.Count);
			AssertEquals("1 house bill", 1, declaration.Bills.Count);
			AssertNotNull("Sub 1 remains", declaration.Bills.FindByBillNumberAndType(sub1.JS_HouseBill, BillTypeList.Codes.HouseBill));
			AssertNull("Sub 2 removed", declaration.Bills.FindByBillNumberAndType(sub2.JS_HouseBill, BillTypeList.Codes.HouseBill));
		}

		public void TestRefreshWhenConsolChanges()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = "SEA";
			consol1.JK_RL_NKLoadPort = "KRPUS";
			consol1.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol1.JK_MasterBillNum = "M1";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = "SEA";
			consol2.JK_RL_NKLoadPort = "KRPUS";
			consol2.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol2.JK_MasterBillNum = "M2";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment = factory2.New<ForwardingShipment>();
			shipment.JS_HouseBill = "H1";
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol1.PK));
			factory2.Save();

			var declaration = factory2.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("MasterBill", "M1", declaration.JE_MasterBill);

			shipment.Consols.Remove(factory2.Load<ForwardingConsol>(consol1.PK));
			shipment.Consols.Add(factory2.Load<ForwardingConsol>(consol2.PK));
			//this change should be synchronised
			AssertEquals("MasterBill", "M2", declaration.JE_MasterBill);
		}

		//TODO: Move to CusContainerCollectionSynchroniserTest
		public void TestSynchronise_ContainersForMultipleConsols()
		{
			const string containerNumber = "CRXU1234568";
			const string additionalContainerNumber = "CRXU1234569";

			ForwardingConsol domesticConsol = Factory.New<ForwardingConsol>();
			domesticConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			domesticConsol.JK_RL_NKLoadPort = "DEFRA";
			domesticConsol.JK_RL_NKDischargePort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, "DE"))).Code;
			domesticConsol.Shipments.Add(Shipment);

			CommonContainer container1 = domesticConsol.Containers.AddNew();
			container1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR").PK;
			container1.JC_ContainerNum = containerNumber;

			CommonContainer container2 = domesticConsol.Containers.AddNew();
			container2.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR").PK;
			container2.JC_ContainerNum = additionalContainerNumber;

			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			Shipment.JS_RL_NKOrigin = "DEFRA";

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_RL_NKLoadPort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			CommonContainer container = Consol.Containers.AddNew();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(Consol, container);
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR").PK;

			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Container Count", 1, declaration.CusContainers.Count);
			AssertEquals("Container #", containerNumber, declaration.CusContainers[0].CO_ContainerNumber);

			packLine.SetContainer(domesticConsol, container1);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Container Count", 1, declaration.CusContainers.Count);
			AssertEquals("Container #", containerNumber, declaration.CusContainers[0].CO_ContainerNumber);

			packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(domesticConsol, container2);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Container Count", 1, declaration.CusContainers.Count);
			AssertEquals("Container #", containerNumber, declaration.CusContainers[0].CO_ContainerNumber);
		}

		//TODO: Move to CusContainerCollectionSynchroniserTest
		public void TestSynchronise_Containers()
		{
			const string ContainerNumber = "CRXU1234568";
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			Shipment.JS_RL_NKOrigin = "DEFRA";
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			CommonContainer container = Consol.Containers.AddNew();
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.SetContainer(Consol, container);

			container.JC_ContainerNum = ContainerNumber;

			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20NOR").PK;

			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Container Count", 1, declaration.CusContainers.Count);
			AssertEquals("Container #", ContainerNumber, declaration.CusContainers[0].CO_ContainerNumber);
		}

		//TODO: Move to CusContainerCollectionSynchroniserTest
		public void TestSynchronise_ContainersDoesNotCreateExtraContainers()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			Shipment.JS_RL_NKOrigin = "DEFRA";
			Shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_JS = Shipment.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;

			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			BaseCusContainer container = (BaseCusContainer)((System.ComponentModel.IBindingList)declaration.CusContainers).AddNew();
			container.Validation.ValidateCO_Seal();

			AssertEquals("Container Count", 1, declaration.CusContainers.Count);
		}

		public void TestSynchroniseExportCtoAndDepot()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			shipment.JS_RL_NKDestination = "KRANY";

			ForwardingConsol exportConsol = shipment.Consols.AddNew();
			exportConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport exportTransport = exportConsol.Transports[0];

			exportConsol.JK_RL_NKLoadPort = shipment.JS_RL_NKOrigin;
			exportConsol.JK_RL_NKDischargePort = "SGSIN";
			exportTransport.JW_ETA = new ZDateTime(2004, 01, 20);
			exportConsol.JK_MasterBillNum = "08166666666";
			exportTransport.JW_VoyageFlight = "QF45";
			exportConsol.JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;
			exportConsol.JK_OA_DepartureCTOAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			exportConsol.JK_OA_PackDepotAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("CTO", exportConsol.JK_OA_DepartureCTOAddress, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals("UnpackDepot", exportConsol.JK_OA_PackDepotAddress, declaration.DepotDocAddress.E2_OA_Address);

			declaration.ContainerTerminalOperatorDocAddress.HasChanges = false;
			declaration.DepotDocAddress.HasChanges = false;

			AssertEquals("CTO.IsSavedByFactory", true, declaration.ContainerTerminalOperatorDocAddress.IsSavedByFactory);
			AssertEquals("UnpackDepot.IsSavedByFactory", true, declaration.DepotDocAddress.IsSavedByFactory);
		}

		public void TestSynchronise_FromConsolForAir()
		{
			//Import
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "KRANY";
			shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;

			ForwardingConsol consolUSSingapore = shipment.Consols.AddNew();
			consolUSSingapore.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transportUSSingapore = consolUSSingapore.Transports[0];

			ForwardingConsol consolSingaporeER = shipment.Consols.AddNew();
			consolSingaporeER.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transportSingaporeER = consolSingaporeER.Transports[0];

			consolUSSingapore.JK_RL_NKLoadPort = "KRASA";
			consolUSSingapore.JK_RL_NKDischargePort = "SGSIN";
			transportUSSingapore.JW_ETA = new ZDateTime(2004, 01, 20);
			consolUSSingapore.JK_MasterBillNum = "08166666666";
			transportUSSingapore.JW_VoyageFlight = "QF45";
			consolUSSingapore.JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgHeader>(new ZQuery()).MainAddress.PK;
			consolUSSingapore.JK_OA_ArrivalCTOAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			consolUSSingapore.JK_OA_UnpackDepotAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			consolUSSingapore.JK_RL_NKPortOfFirstArrival = "SGSIN";

			consolSingaporeER.JK_RL_NKLoadPort = "SGSIN";
			consolSingaporeER.JK_RL_NKDischargePort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			transportSingaporeER.JW_ETA = new ZDateTime(2004, 01, 27);
			consolSingaporeER.JK_MasterBillNum = "08155555555";
			transportSingaporeER.JW_VoyageFlight = "QF23";
			consolSingaporeER.JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consolUSSingapore.ShippingLinePK)).MainAddress.PK;
			consolSingaporeER.JK_OA_ArrivalCTOAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consolUSSingapore.JK_OA_ArrivalCTOAddress)).PK;
			consolSingaporeER.JK_OA_UnpackDepotAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consolUSSingapore.JK_OA_UnpackDepotAddress)).PK;
			consolSingaporeER.JK_RL_NKPortOfFirstArrival = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode)).Code;

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Port of Loading", consolSingaporeER.JK_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port of Discharge", consolSingaporeER.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Port of first arrival", consolSingaporeER.JK_RL_NKPortOfFirstArrival, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Flight No", consolSingaporeER.JK_JX_JV_VoyageFlight, declaration.JE_VoyageFlightNo);
			AssertEquals("Shipping Line", consolSingaporeER.ShippingLinePK, declaration.JE_OH_ShippingLine);
			AssertEquals("CTO", consolSingaporeER.JK_OA_ArrivalCTOAddress, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals("UnpackDepot", consolSingaporeER.JK_OA_UnpackDepotAddress, declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals("Master bill num", consolSingaporeER.JK_MasterBillNum, declaration.JE_MasterBill);
		}

		public void TestSynchroniseWhenShipmentHas2Consol()
		{
			RefUNLOCO localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			RefUNLOCO overseasPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, localPort.RL_RN_NKCountryCode));
			RefUNLOCO overseasPort2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { localPort.RL_RN_NKCountryCode, overseasPort.RL_RN_NKCountryCode }));

			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKLoadPort = overseasPort.Code;
			consol1.JK_RL_NKDischargePort = localPort.Code;
			consol1.JK_MasterBillNum = "08622955132";

			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = overseasPort.Code;
			shipment1.JS_RL_NKDestination = localPort.Code;

			ForwardingShipment shipment2 = consol1.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = overseasPort.Code;
			shipment2.JS_RL_NKDestination = localPort.Code;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment2.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Port of Loading", overseasPort.Code, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port of Discharge", localPort.Code, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("Master bill", "08622955132", declaration.JE_MasterBill);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Cuba))
			{
				consol1.JK_RL_NKLoadPort = overseasPort.Code;
				consol1.JK_RL_NKDischargePort = "CUBAN";
				consol1.JK_MasterBillNum = "08622955132";

				ForwardingConsol consol2 = shipment2.Consols.AddNew();
				consol2.JK_RL_NKLoadPort = localPort.Code;
				consol2.JK_RL_NKDischargePort = overseasPort2.Code;
				consol2.JK_RL_NKPortOfFirstArrival = localPort.Code;
				consol2.JK_MasterBillNum = "08166518654";

				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

				AssertEquals("Port of Loading", consol1.JK_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
				AssertEquals("Port of Discharge", consol1.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);
				AssertEquals("Master bill", consol1.JK_MasterBillNum, declaration.JE_MasterBill);
			}
		}

		public void TestSynchroniseTranshipment()
		{
			var dec = BaseJobDeclaration.New(Factory);
			var port1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, dec.CountryCode));
			var port2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { dec.CountryCode, port1.RL_RN_NKCountryCode }));
			var port3 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { dec.CountryCode, port1.RL_RN_NKCountryCode, port2.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = port1.RL_Code;
			shipment.JS_RL_NKDestination = port3.RL_Code;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "OBL1";
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol1.JK_RL_NKLoadPort = port2.RL_Code;
			consol1.JK_RL_NKDischargePort = localPort.RL_Code;

			var consol2 = shipment.Consols.AddNew();
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			consol2.JK_MasterBillNum = "OBL2";
			consol2.JK_RL_NKLoadPort = localPort.RL_Code;
			consol2.JK_RL_NKDischargePort = port3.RL_Code;

			dec.JE_JS = shipment.PK;

			Factory.Save(); // To stop the JobContainer being deleted when not attached to a consol

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("OBL2", dec.JE_MasterBill);

			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("OBL1", dec.JE_MasterBill);
		}

		public void TestSynchronise_PicksCorrectExportConsol()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			ForwardingConsol consolSG_US = shipment.Consols.AddNew();
			consolSG_US.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transportSG_US = consolSG_US.Transports[0];

			ForwardingConsol consolER_SG = shipment.Consols.AddNew();
			consolER_SG.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transportER_SG = consolER_SG.Transports[0];

			shipment.JS_RL_NKOrigin = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			shipment.JS_RL_NKDestination = "KRANY";

			consolER_SG.JK_RL_NKLoadPort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			consolER_SG.JK_RL_NKDischargePort = "SGSIN";
			transportER_SG.JW_ETA = new ZDateTime(2004, 1, 20);

			consolSG_US.JK_RL_NKLoadPort = "SGSIN";
			consolSG_US.JK_RL_NKDischargePort = "KRASA";
			transportER_SG.JW_ETA = new ZDateTime(2004, 1, 27);
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Port of Loading", consolER_SG.JK_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port of Discharge", consolER_SG.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);
		}

		public void TestSynchronise_PicksCorrectImportConsol()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consolKRSingapore = shipment.Consols.AddNew();
			consolKRSingapore.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transportKRSingapore = consolKRSingapore.Transports[0];

			ForwardingConsol consolSingaporeER = shipment.Consols.AddNew();
			consolSingaporeER.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transportSingaporeER = consolSingaporeER.Transports[0];

			shipment.JS_RL_NKOrigin = "KRANY";
			shipment.JS_RL_NKDestination = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;

			consolKRSingapore.JK_RL_NKLoadPort = "KRANY";
			consolKRSingapore.JK_RL_NKDischargePort = "SGSIN";
			transportKRSingapore.JW_ETA = new ZDateTime(2004, 01, 20);

			consolSingaporeER.JK_RL_NKLoadPort = "SGSIN";
			consolSingaporeER.JK_RL_NKDischargePort = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, declaration.CountryCode))).Code;
			transportSingaporeER.JW_ETA = new ZDateTime(2004, 01, 27);

			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Port of Loading", consolSingaporeER.JK_RL_NKLoadPort, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("Port of Discharge", consolSingaporeER.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);
		}

		public void TestSynchronise_OrderNumbersToOwnerRef()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ForwardingShipment shipment = CreateImportShipment();
			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "KRANY";
			transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			shipment.JS_RL_NKOrigin = "KRASA";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			declaration.JE_JS = shipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("pre-condition", "", declaration.JE_OwnerRef);

			Order inv1Order = shipment.AttachedOrders.AddNew();
			inv1Order.JD_OrderNumber = "ORD NUM1";

			OrderItem orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "ORDINV12345";

			AssertEquals("Order Number", "ORD NUM1, ORDINV12345", declaration.JE_OwnerRef);
			AssertEquals(true, declaration.JE_OwnerRefInfo.ReadOnly);

			decSynchroniser.SetEnabled(false, false);
			AssertEquals(false, declaration.JE_OwnerRefInfo.ReadOnly);
		}

		public void TestSynchronise_OrderNumbersToOwnerRef_DontSynch()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment = CreateImportShipment();
			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "KRANY";
			transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			shipment.JS_RL_NKOrigin = "KRASA";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			declaration.JE_JS = shipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("pre-condition", "", declaration.JE_OwnerRef);

			Order inv1Order = shipment.AttachedOrders.AddNew();
			inv1Order.JD_OrderNumber = "ORD NUM1";

			OrderItem orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "ORDINV12345";

			AssertEquals("", declaration.JE_OwnerRef);
			AssertEquals(false, declaration.JE_OwnerRefInfo.ReadOnly);
		}

		public virtual void TestJobDeclarationSynchroniser()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			Shipment.JS_RS_NKServiceLevel = "XYZ";
			AssertEquals("Declaration.JE_RS_NKServiceLevel", "XYZ", declaration.JE_RS_NKServiceLevel);

			Shipment.JS_TransportMode = "";
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Declaration.JE_TransportMode", Enterprise.Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);

			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("Declaration.JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);

			OrgHeader testConsignor = GetOverseasConsignor();
			Shipment.ConsignorPK = testConsignor.PK;
			AssertEquals("Declaration.JE_OH_Supplier", testConsignor.PK, declaration.JE_OH_Supplier);

			OrgHeader testConsignee = GetLocalConsignee();
			Shipment.ConsigneePK = testConsignee.PK;
			AssertEquals("Declaration.JE_OH_Importer", testConsignee.PK, declaration.JE_OH_Importer);

			Transport.JW_ATA = ZDateTime.Today.AddDays(3);
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_A_ARV, declaration.JE_DateOfArrival);

			Transport.JW_ATD = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_ExportDate", Consol.JK_JX_JA_A_DEP, declaration.JE_ExportDate);

			Transport.JW_Vessel = "12345";
			AssertEquals("Declaration.JE_VesselName", Consol.JK_JX_JV_NKVessel, declaration.JE_VesselName);

			Shipment.JS_HouseBill = TestHouseBillNumber;
			AssertEquals("Declaration.JE_HouseBill", TestHouseBillNumber, declaration.JE_HouseBill);

			Shipment.JS_ActualWeight = 3210.0m;
			AssertEquals("Declaration.JE_TotalWeight", 3210.0m, declaration.JE_TotalWeight);

			Shipment.JS_UnitOfWeight = Enterprise.Core.Constants.Weight.Pounds;
			AssertEquals("Declaration.JE_TotalWeightUnit", Enterprise.Core.Constants.Weight.Pounds, declaration.JE_TotalWeightUnit);

			Shipment.JS_ActualVolume = 13.2m;
			AssertEquals("Declaration.JE_TotalVolume", 13.2m, declaration.JE_TotalVolume);

			Shipment.JS_UnitOfVolume = Enterprise.Core.Constants.Volume.CubicFeet;
			AssertEquals("Declaration.JE_TotalVolumeUnit", Enterprise.Core.Constants.Volume.CubicFeet, declaration.JE_TotalVolumeUnit);

			Shipment.JS_OuterPacks = 12;
			AssertEquals("Declaration.JE_TotalNoOfPacks", 12, declaration.JE_TotalNoOfPacks);

			Shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Box;
			Shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Pallet;

			AssertEquals("Declaration.JE_TotalNoOfPacksPackType", Enterprise.Core.Constants.PkgUnit.Pallet, declaration.JE_TotalNoOfPacksPackType);

			Shipment.JS_GoodsDescription = TestGoodsDescription;
			AssertEquals("Declaration.JE_GoodsDescription", TestGoodsDescription, declaration.JE_GoodsDescription);

			Shipment.JS_E_ARV = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_DateOfFirstArrival", Shipment.JS_E_ARV, declaration.JE_DateAtFinalDestination);

			Shipment.JS_E_DEP = ZDateTime.Today.AddDays(4);
			AssertEquals("Declaration.JE_DateAtOrigin", Shipment.JS_E_DEP, declaration.JE_DateAtOrigin);
		}

		public void TestJobDeclarationSynchroniser_TransportMode()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "GBANY";
			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				var map = GetTransportModeMap();
				foreach (var entry in map)
				{
					shipment.JS_TransportMode = entry.Key;
					AssertEquals($"Shipment.JS_TransportMode = '{entry.Key}' => Declaration.JE_TransportMode = '{entry.Value}'", entry.Value, declaration.JE_TransportMode);
				}

				shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
				AssertEquals($"Import: Shipment.JS_TransportMode = 'FAS' => Declaration.JE_TransportMode = 'SEA'", map[Core.Constants.TransportModes.Sea], declaration.JE_TransportMode);

				shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
				AssertEquals($"Import: Shipment.JS_TransportMode = 'FSA' => Declaration.JE_TransportMode = 'AIR'", map[Core.Constants.TransportModes.Air], declaration.JE_TransportMode);

				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
				AssertEquals($"Export: Shipment.JS_TransportMode = 'FAS' => Declaration.JE_TransportMode = 'AIR'", map[Core.Constants.TransportModes.Air], declaration.JE_TransportMode);

				shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
				AssertEquals($"Export: Shipment.JS_TransportMode = 'FSA' => Declaration.JE_TransportMode = 'SEA'", map[Core.Constants.TransportModes.Sea], declaration.JE_TransportMode);
			});
		}

		protected virtual IDictionary<string, string> GetTransportModeMap() => new Dictionary<string, string>
		{
			{ Core.Constants.TransportModes.Air, TransportTypeList.Codes.Air },
			{ Core.Constants.TransportModes.Courier, TransportTypeList.Codes.Mail },
			{ Core.Constants.TransportModes.Road, TransportTypeList.Codes.Road },
			{ Core.Constants.TransportModes.Sea, TransportTypeList.Codes.Sea },
		};

		//TODO: Move to CusContainerCollectionSynchroniserTest
		public void TestContainersSynchronisation()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Containerised;
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = TestContainerNumber;
			container1.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container1.JC_SealNum = TestSealNumber;
			container1.JC_AdditionalSealNum = TestSealNumber2;
			container1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, TestContainerTypeNK).PK;

			PackLine outerPackLine = Shipment.OuterPackLines.AddNew();
			outerPackLine.JL_JC = container1.PK;
			outerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;

			AssertEquals("CusContainer Count", 1, declaration.CusContainers.Count);
			AssertEquals("CusContainer ContainerNum", TestContainerNumber, declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("CusContainer Container Mode", Enterprise.Core.Constants.ContainerModes.FCL, declaration.CusContainers[0].CO_FCL_LCL_AIR);
			AssertEquals("CusContainer Seal", TestSealNumber, declaration.CusContainers[0].CO_Seal);
			AssertEquals("CusContainer Second Seal", TestSealNumber2, declaration.CusContainers[0].CO_SecondSeal);
			AssertEquals("CusContainer RC", container1.JC_RC, declaration.CusContainers[0].CO_RC);

			outerPackLine.JL_ActualWeight = 13.6m;
			container1.JC_ContainerNum = TestContainerNumber2;
			container1.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container1.JC_SealNum = TestSealNumber2;
			container1.JC_AdditionalSealNum = TestSealNumber;
			container1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, TestContainerType2NK).PK;

			AssertEquals("CusContainer ContainerNum", TestContainerNumber2, declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("CusContainer Container Mode", Enterprise.Core.Constants.ContainerModes.FCL, declaration.CusContainers[0].CO_FCL_LCL_AIR);
			AssertEquals("CusContainer Seal", TestSealNumber2, declaration.CusContainers[0].CO_Seal);
			AssertEquals("CusContainer Second Seal", TestSealNumber, declaration.CusContainers[0].CO_SecondSeal);
			AssertEquals("CusContainer RC", container1.JC_RC, declaration.CusContainers[0].CO_RC);

			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = TestContainerNumber;
			container2.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container2.JC_SealNum = TestSealNumber;
			container2.JC_AdditionalSealNum = TestSealNumber2;
			container2.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, TestContainerTypeNK).PK;

			Factory.Save(); // To stop the JobContainer being deleted when not attached to a consol

			outerPackLine.JL_JC = container2.PK;

			AssertEquals("CusContainer Count", 1, declaration.CusContainers.Count);
			AssertEquals("CusContainer ContainerNum", TestContainerNumber, declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("CusContainer Container Mode", Enterprise.Core.Constants.ContainerModes.FCL, declaration.CusContainers[0].CO_FCL_LCL_AIR);
			AssertEquals("CusContainer Seal", TestSealNumber, declaration.CusContainers[0].CO_Seal);
			AssertEquals("CusContainer Seal", TestSealNumber2, declaration.CusContainers[0].CO_SecondSeal);
			AssertEquals("CusContainer RC", container2.JC_RC, declaration.CusContainers[0].CO_RC);

			PackLine outerPackLine2 = Shipment.OuterPackLines.AddNew();
			outerPackLine2.JL_JC = container2.PK;

			AssertEquals("CusContainer Count", 1, declaration.CusContainers.Count);
			AssertEquals("CusContainer ContainerNum", TestContainerNumber, declaration.CusContainers[0].CO_ContainerNumber);
			AssertEquals("CusContainer Container Mode", Enterprise.Core.Constants.ContainerModes.FCL, declaration.CusContainers[0].CO_FCL_LCL_AIR);
			AssertEquals("CusContainer Seal", TestSealNumber, declaration.CusContainers[0].CO_Seal);
			AssertEquals("CusContainer Seal", TestSealNumber2, declaration.CusContainers[0].CO_SecondSeal);
			AssertEquals("CusContainer RC", container2.JC_RC, declaration.CusContainers[0].CO_RC);

			outerPackLine2.JL_ActualWeight = 23.2m;

			outerPackLine.JL_JC = container1.PK;
			container1.JC_ContainerNum = TestContainerNumber;
			container1.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container1.JC_SealNum = TestSealNumber;
			container2.JC_AdditionalSealNum = TestSealNumber2;
			container1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, TestContainerTypeNK).PK;

			AssertEquals("CusContainer Count", 2, declaration.CusContainers.Count);
			AssertEquals("CusContainer ContainerNum", TestContainerNumber, declaration.CusContainers[1].CO_ContainerNumber);
			AssertEquals("CusContainer Container Mode", Enterprise.Core.Constants.ContainerModes.FCL, declaration.CusContainers[1].CO_FCL_LCL_AIR);
			AssertEquals("CusContainer Seal", TestSealNumber, declaration.CusContainers[1].CO_Seal);
			AssertEquals("CusContainer Seal", TestSealNumber2, declaration.CusContainers[0].CO_SecondSeal);
			AssertEquals("CusContainer RC", container1.JC_RC, declaration.CusContainers[1].CO_RC);
		}

		//TODO: Move to CusContainerCollectionSynchroniserTest
		public void TestContainerDeletedSynchronisation()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = Shipment.PK;
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = TestContainerNumber;
			container1.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container1.JC_SealNum = TestSealNumber;
			container1.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, TestContainerTypeNK).PK;

			PackLine outerPackLine = Shipment.OuterPackLines.AddNew();
			outerPackLine.JL_JC = container1.PK;
			outerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;

			AssertEquals("CusContainer Count", 1, declaration.CusContainers.Count);
			AssertEquals("CusContainer ContainerNum", TestContainerNumber, declaration.CusContainers[0].CO_ContainerNumber);
			outerPackLine.JL_JC = ZGuid.Empty;
			AssertEquals("CusContainer Count", 0, declaration.CusContainers.Count);
		}

		public void TestDeclarationDeletedSynchronisation()
		{
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = Shipment.PK;
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			var container = Consol.Containers.AddNew();
			container.JC_ContainerNum = TestContainerNumber;
			container.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container.JC_SealNum = TestSealNumber;
			container.JC_RC = Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, TestContainerTypeNK).PK;

			var outerPackLine = Shipment.OuterPackLines.AddNew();
			outerPackLine.JL_JC = container.PK;
			outerPackLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Basket;

			AssertEquals("OuterPackLines count", 1, shipment.OuterPackLines.Count);
			shipment.OuterPackLines.Remove(outerPackLine);
			declaration.Delete();

			Assert("Declaration is deleted", declaration.IsDeleted);
			AssertEquals("OuterPackLines count", 0, shipment.OuterPackLines.Count);
		}

		public virtual void TestSeaTransportModeFormattingForImport()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);

			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Loose;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.TransportModes.Air, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.ULD;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.TransportModes.Air, declaration.JE_ContainerMode);
		}

		public virtual void TestSeaTransportModeFormatting()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.Bulk, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.Liquid, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);

			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Loose;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.TransportModes.Air, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.ULD;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.TransportModes.Air, declaration.JE_ContainerMode);
		}

		[ExpectNoExceptions]
		public void TestConsolSynchroniserWithExceptionCatched()
		{
			declaration.JE_JS = Shipment.PK;
			Transport.JW_ETA = ZDateTime.Today.AddDays(3);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_E_ARV, declaration.JE_DateOfArrival);

			declaration.JE_DateOfArrivalInfo.ValueChanged += new EventHandler((o, e) =>
			{
				decSynchroniser.ConsolFieldSynchronisers.Add(new FieldSynchroniser(declaration.JE_SystemCreateUserInfo, () => { return (ZString)"TST"; }, () => { return Array.Empty<ZPropertyInfo>(); }));
			});

			Transport.JW_ATA = ZDateTime.Today.AddDays(10);
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			declaration.JE_SystemCreateUser = ZString.Empty;
			AssertEquals("Declaration.JE_DateOfArrival", ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals("Declaration.JE_SystemCreateUser", ZString.Empty, declaration.JE_SystemCreateUser);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_A_ARV, declaration.JE_DateOfArrival);
			AssertEquals("Declaration.JE_SystemCreateUser", "TST", declaration.JE_SystemCreateUser);
		}

		public void TestConsolChangingSynchroniser()
		{
			declaration.JE_JS = Shipment.PK;
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			Transport.JW_ETA = ZDateTime.Today.AddDays(3);
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_E_ARV, declaration.JE_DateOfArrival);

			Transport.JW_ATA = ZDateTime.Today.AddDays(3);
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_A_ARV, declaration.JE_DateOfArrival);

			Transport.JW_ATA = ZDateTime.Empty;
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_E_ARV, declaration.JE_DateOfArrival);

			Transport.JW_ATA = ZDateTime.Today.AddDays(4);
			AssertEquals("Declaration.JE_DateOfArrival", Consol.JK_JX_JB_A_ARV, declaration.JE_DateOfArrival);

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			Shipment.Consols.Remove(Consol);
			var newConsol = Factory.New<ForwardingConsol>();
			TestHelper.MakeConsolRelevantToDeclaration(newConsol, declaration);
			Shipment.Consols.Add(newConsol);

			var newTransport = newConsol.Transports[0];
			newTransport.JW_ETA = ZDateTime.Today.AddDays(2);
			AssertEquals("Declaration.JE_DateOfArrival", newTransport.JW_ETA, declaration.JE_DateOfArrival);
		}

		public void TestRoadShipmentWithSeveralTransportLegs()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CNRU901600152785";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "KRSEL";
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var leg1 = consol.Transports[0];
			leg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			leg1.JW_RL_NKLoadPort = "KRSEL";
			leg1.JW_RL_NKDiscPort = "JPHAL";
			leg1.JW_LegOrder = (ZByte)1;

			var leg2 = consol.Transports.AddNew();
			leg2.JW_TransportMode = Core.Constants.TransportModes.Rail;
			leg2.JW_RL_NKLoadPort = "JPHAL";
			leg2.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			leg2.JW_LegOrder = (ZByte)2;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = ZString.Empty;
			shipment.JS_RL_NKOrigin = "KRSEL";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);

			AssertEquals("should not create an empty bill with no house bill number", 1, declaration.Bills.Count);
			AssertEquals(ZString.Empty, declaration.JE_HouseBill);
			AssertEquals(Core.Constants.TransportModes.Rail, declaration.JE_TransportMode);
		}

		class JobDeclarationSynchroniserTestUseSupplierAndImporterAddress : JobDeclarationSynchroniserTest
		{
			public void TestSynchWithOverridenOrgs_Import()
			{
				PrepareOrgs();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKDischargePort = localPort;

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKDestination = localPort;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = addressForeign.PK;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = addressLocal.PK;
				shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
				var declaration = GetJobDeclaration();
				declaration.JE_MessageType = "IMP";
				declaration.JE_JS = shipment.PK;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals("Port is editable when source address overridden", false, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
				AssertEquals("JE_OH_Importer is blank when overridden", ZGuid.Empty, declaration.JE_OH_Importer);
				AssertEquals("JE_OH_Importer is editable when overridden", false, declaration.JE_OH_ImporterInfo.ReadOnly);
				AssertEquals("JE_OA_ImporterAddress is blank when overridden", ZGuid.Empty, declaration.JE_OA_ImporterAddress);
				AssertEquals("JE_OA_ImporterAddress is editable when overridden", false, declaration.JE_OA_ImporterAddressInfo.ReadOnly);

				shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = false;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals("Port is locked, without explicit re-synch, when override is changed", true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
				AssertEquals("JE_OH_Importer is NOT blank when not overridden", orgLocal.PK, declaration.JE_OH_Importer);
				AssertEquals("JE_OA_ImporterAddress is NOT blank when not overridden", orgLocal.MainAddress.PK, declaration.JE_OA_ImporterAddress);
				AssertEquals("JE_OA_ImporterAddress is locked when not overridden", true, declaration.JE_OA_ImporterAddressInfo.ReadOnly);

				shipment.Consols.Add(consol);
				shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals(localPort, declaration.JE_RL_NKPortOfArrival);
				AssertEquals("Port is locked now that we are able to calculate a port", true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
			}

			public void TestSynchWithOverridenOrgs_Export()
			{
				PrepareOrgs();
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = localPort;

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = localPort;
				shipment.ConsigneeDocumentaryAddress.E2_OA_Address = addressForeign.PK;
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = addressLocal.PK;
				shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
				var declaration = GetJobDeclaration();
				declaration.JE_MessageType = "EXP";
				declaration.JE_JS = shipment.PK;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals("Port is editable when source address overridden", false, declaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
				AssertEquals("JE_OH_Supplier is blank when overridden", ZGuid.Empty, declaration.JE_OH_Supplier);
				AssertEquals("JE_OH_Supplier is editable when overridden", false, declaration.JE_OH_SupplierInfo.ReadOnly);
				AssertEquals("JE_OA_SupplierAddress is blank when overridden", ZGuid.Empty, declaration.JE_OA_SupplierAddress);
				AssertEquals("JE_OA_SupplierAddress is editable when overridden", false, declaration.JE_OA_SupplierAddressInfo.ReadOnly);

				shipment.ConsignorDocumentaryAddress.E2_AddressOverride = false;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals("Port is locked, without explicit re-synch, when override is changed", true, declaration.JE_RL_NKPortOfLoadingInfo.ReadOnly);
				AssertEquals("JE_OH_Supplier is NOT blank when not overridden", orgLocal.PK, declaration.JE_OH_Supplier);
				AssertEquals("JE_OA_SupplierAddress is NOT blank when not overridden", addressLocal.PK, declaration.JE_OA_SupplierAddress);
				AssertEquals("JE_OA_SupplierAddress is locked when not overridden", true, declaration.JE_OA_SupplierAddressInfo.ReadOnly);

				shipment.Consols.Add(consol);
				shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				AssertEquals(localPort, declaration.JE_RL_NKPortOfLoading);
				AssertEquals("Port is locked now that we are able to calculate a port", true, declaration.JE_RL_NKPortOfArrivalInfo.ReadOnly);
			}

			protected override BaseJobDeclaration GetJobDeclaration()
			{
				return Factory.New<BaseJobDeclarationWithUseSupplierAndImporterAddressForTest>();
			}

			class BaseJobDeclarationWithUseSupplierAndImporterAddressForTest : BaseJobDeclaration
			{
				public BaseJobDeclarationWithUseSupplierAndImporterAddressForTest(BusinessObjectFactory factory, System.Data.DataRow row)
					: base(factory, row)
				{ }

				public override bool UseImporterAddress => true;

				public override bool UseSupplierAddress => true;
			}

			void PrepareOrgs()
			{
				localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
				var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;

				orgLocal = Factory.New<OrgHeader>();
				addressLocal = orgLocal.MainAddress;
				addressLocal.OA_Address1 = "Daniel";
				addressLocal.OA_RL_NKRelatedPortCode = localPort;

				var orgForeign = Factory.New<OrgHeader>();
				addressForeign = orgForeign.MainAddress;
				addressForeign.OA_Address1 = "Daniel";
				addressForeign.OA_RL_NKRelatedPortCode = foreignPort;
			}

			ZString localPort;
			OrgHeader orgLocal;
			OrgAddress addressLocal;
			OrgAddress addressForeign;
		}

		class JobDeclarationSynchroniserTestWithSettablePacking : JobDeclarationSynchroniserTest
		{
			public void TestSynchronisePackingHonoursDeclarationsIsPackingRelevantSetting_Relevant()
			{
				((BaseJobDeclarationWithSettableIsPackingInformationRelevantForTest)declaration).packingRelevantTest = true;
				Shipment.OuterPackLines.AddNew().JL_ActualWeight = 69m;
				Shipment.JS_HouseBill = "Hawb123";
				decSynchroniser.Synchronise(true);
				AssertEquals("Stuff from packing tab is synched because declaration's IsPackingInformationRelevantCore is true", 1, declaration.Packages.Count);
			}

			public void TestSynchronisePackingHonoursDeclarationsIsPackingRelevantSetting_Irrelevant()
			{
				((BaseJobDeclarationWithSettableIsPackingInformationRelevantForTest)declaration).packingRelevantTest = false;
				Shipment.OuterPackLines.AddNew().JL_ActualWeight = 69m;
				Shipment.JS_HouseBill = "Hawb123";
				decSynchroniser.Synchronise(true);
				AssertEquals("Stuff from packing tab not synched because declaration's IsPackingInformationRelevantCore is false", 0, declaration.Packages.Count);
			}

			protected override BaseJobDeclaration GetJobDeclaration()
			{
				var result = Factory.New<BaseJobDeclarationWithSettableIsPackingInformationRelevantForTest>();
				result.packingRelevantTest = true;
				return result;
			}

			class BaseJobDeclarationWithSettableIsPackingInformationRelevantForTest : BaseJobDeclaration
			{
				public BaseJobDeclarationWithSettableIsPackingInformationRelevantForTest(BusinessObjectFactory factory, System.Data.DataRow row)
					: base(factory, row)
				{ }

				public bool packingRelevantTest;
				protected override bool IsPackingInformationRelevantCore { get { return packingRelevantTest; } }
			}
		}

		public void TestIncoTerm()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_INCO = "FOB";
			AssertEquals("Declaration.JE_ShipmentIncoTerm", Shipment.JS_INCO, declaration.JE_ShipmentIncoTerm);
			Shipment.JS_INCO = "CIF";
			AssertEquals("Declaration.JE_ShipmentIncoTerm", Shipment.JS_INCO, declaration.JE_ShipmentIncoTerm);
		}

		#region Synchronising JE_OwnerRef

		public void TestOwnerRefNotPopulatedIfNoOrderNumbers()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			ForwardingShipment shipment = CreateImportShipment();
			declaration.JE_JS = shipment.PK;
			JobDeclarationSynchroniser sync = new JobDeclarationSynchroniser(declaration);
			sync.SetEnabled(true, false);

			OrderItem item = declaration.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "ownerref";
			AssertEquals("ownerref initially", "ownerref", declaration.JE_OwnerRef);
			item.Delete();
			AssertEquals("ownerref should be the same since there are no order numbers at all", "ownerref", declaration.JE_OwnerRef);
		}

		public void TestOwnerRefNotPopulatedIfHasCustomsMessages()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment = CreateImportShipment();
			declaration.JE_JS = shipment.PK;
			JobDeclarationSynchroniser sync = new JobDeclarationSynchroniser(declaration);
			sync.SetEnabled(true, false);

			OrderItem item = declaration.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "ownerref";
			AssertEquals("ownerref initially", "ownerref", declaration.JE_OwnerRef);
			item.JT_OrderReference = "ownerref2";
			AssertEquals("ownerref2 now", "ownerref2", declaration.JE_OwnerRef);
		}

		public void TestSynchroniseJE_OwnerRef()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			ForwardingShipment shipment = CreateImportShipment();
			declaration.JE_JS = shipment.PK;
			JobDeclarationSynchroniser sync = new JobDeclarationSynchroniser(declaration);
			sync.SetEnabled(true, false);

			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "123";
			declaration.AttachedOrders.Add(order1);
			AssertEquals("123", declaration.JE_OwnerRef);

			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "456";
			declaration.AttachedOrders.Add(order2);
			AssertEquals("123, 456", declaration.JE_OwnerRef);

			declaration.DocsAndCartage.JP_OrderItemsAsString = "789";
			AssertEquals("123, 456, 789", declaration.JE_OwnerRef);
		}

		public void TestDontSyncJE_OwnerRefIfItIsAutoAssigned()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertDontSyncJE_OwnerRefIfItIsAutoAssigned(false);
			AssertDontSyncJE_OwnerRefIfItIsAutoAssigned(true);
		}

		public void TestDontSynchroniseOwnerRefWhenNotEnabled()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment = CreateImportShipment();
			declaration.JE_JS = shipment.PK;
			JobDeclarationSynchroniser sync = new JobDeclarationSynchroniser(declaration);
			sync.SetEnabled(true, false);

			OrderItem item = declaration.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "ownerref";
			AssertEquals("ownerref initially", "ownerref", declaration.JE_OwnerRef);
			item.JT_OrderReference = "ownerref2";
			AssertEquals("ownerref2 now", "ownerref2", declaration.JE_OwnerRef);

			sync.SetEnabled(false, false);
			item.JT_OrderReference = "ownerref3";
			AssertEquals("Still ownerref2 because it is not enabled", "ownerref2", declaration.JE_OwnerRef);
			OrderItem item2 = declaration.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "asdasdsa";
			AssertEquals("Still ownerref2 because it is not enabled", "ownerref2", declaration.JE_OwnerRef);
		}

		public void TestPopulateOwnerRefFromOrderNumbers()
		{
			ForwardingShipment shipment = CreateImportShipment();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			OrderItem testOrderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			testOrderItem.JT_OrderReference = "ORDERNO";

			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Owner Ref should not default", "", declaration.JE_OwnerRef);

			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.ShipmentSynchroniser.SetEnabled(true, false);

			AssertEquals("Owner Ref should now default", "ORDERNO", declaration.JE_OwnerRef);
		}

		public void TestPopulateOwnerRefFromOrderNumbersWhenNoConsignee()
		{
			ForwardingShipment shipment = CreateImportShipment();
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var testConsignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			testConsignee.OH_RL_NKClosestPort = declaration.CountryCode + "AAA";
			testConsignee.OH_IsConsignee = true;
			shipment.ConsigneePK = testConsignee.PK;

			OrderItem orderItem1 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "ORDER1";
			OrderItem orderItem2 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "ORDER2";

			declaration.JE_JS = shipment.PK;
			decSynchroniser = new JobDeclarationSynchroniser(declaration);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Owner Ref should not default", "", declaration.JE_OwnerRef);

			testConsignee.MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = "YES";
			decSynchroniser.SetEnabled(false, false);
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Owner Ref should now default", "ORDER1, ORDER2", declaration.JE_OwnerRef);
		}

		#endregion

		public void TestPopulateOwnerRefFromMultipleOrderNumbers()
		{
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// Export shipment
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = PortNotInCountry(GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2));

			OrderItem orderItem1 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "ExportOrderRef1";
			OrderItem orderItem2 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "ExportOrderRef2";

			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			AssertEquals(declaration.JE_OwnerRef, string.Join(", ", orderItem1.JT_OrderReference, orderItem2.JT_OrderReference));
			Assert(declaration.JE_OwnerRefInfo.ReadOnly);

			// Import
			shipment = CreateImportShipment();
			declaration.JE_JS = shipment.PK;

			orderItem1 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "ImportOrderRef1";
			orderItem2 = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "ImportOrderRef2";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			AssertEquals(declaration.JE_OwnerRef, string.Join(", ", orderItem1.JT_OrderReference, orderItem2.JT_OrderReference));
			Assert(declaration.JE_OwnerRefInfo.ReadOnly);
		}

		public void TestDetectEnabledForUniversalXml_Export()
		{
			AssertDetectEnabledForUniversalXml(JobMessageTypeList.Codes.Export, LocalPort1.RL_Code, ForeignPort1.RL_Code, LocalPort2.RL_Code, ForeignPort2.RL_Code, ForeignPort3.RL_Code, false);
		}

		public void TestDetectEnabledForUniversalXml_Import()
		{
			AssertDetectEnabledForUniversalXml(JobMessageTypeList.Codes.Import, ForeignPort1.RL_Code, LocalPort1.RL_Code, ForeignPort2.RL_Code, LocalPort2.RL_Code, LocalPort3.RL_Code, false);
		}

		protected virtual void AssertDetectEnabledForUniversalXml(ZString messageType, ZString origin, ZString destination, ZString loadPort, ZString dischargePort, ZString firstArrival, ZBool overrideFreightDefaults)
		{
			SetupRegistriesForDetechEnabledForUniversalXml(messageType);
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.JE_MessageType = messageType;
			declaration.JE_OverrideFreightDefaults = false;
			SetupForDetection(shipment, origin, destination);
			SetupForDetection(consol, loadPort, dischargePort, firstArrival);
			SetupPackingDetail(shipment, consol);
			declaration.ShipmentSynchroniser.Synchronise(true);
			Factory.Save();
			declaration.ShipmentSynchroniser.DetectEnabled = true;
			AssertEquals("declaration.ShipmentSynchroniser.SyncChangesDetected", false, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("No changes should be detected", false, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.SetEnabled(false, true);
			declaration.JE_JS = ZGuid.Empty;
			declaration.Delete();
			Factory.Save();
			declaration = GetJobDeclaration();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			declaration.JE_MessageType = messageType;
			ClearDataForSynchronisationDetection(declaration);
			((IBusinessObjectInternals)declaration).Row[BaseJobDeclaration.Schema.JE_OverrideFreightDefaults] = false;
			Factory.Save();
			declaration.ShipmentSynchroniser.DetectEnabled = true;
			AssertEquals("declaration.ShipmentSynchroniser.SyncChangesDetected", false, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Changes should be detected", true, declaration.ShipmentSynchroniser.SyncChangesDetected);
			declaration.ShipmentSynchroniser.SetEnabled(false, false);
			using (SynchroniserDetectionHelper.SetupEnableDetectionForTesting())
			{
				declaration.ShipmentSynchroniser.Synchronise(true);
				var unchangedData = SynchroniserDetectionHelper.GetInfosNotChanged(ShouldIgnoreInfoForDetection);
				if (!unchangedData.IsEmpty)
				{
					Fail("Test data is required for the following synchronise fields:\r\n" + unchangedData);
				}
				var missingData = SynchroniserDetectionHelper.GetSynchroniserWithData();
				if (!missingData.IsEmpty)
				{
					Fail("Test data is required for the following synchronisers:\r\n" + missingData);
				}
			}
			declaration.ShipmentSynchroniser.SetEnabled(true, false);
			Factory.Save();
			var message = GetQueuedUniversalShipmentMessage(shipment);
			message.EM_MessageText = message.EM_MessageText.
						Replace(consol.JK_UniqueConsignRef, "").
						Replace(consol.JK_UniqueConsignRef, "").
						Replace(shipment.JS_UniqueConsignRef, "").
						Replace(declaration.JE_DeclarationReference, "").
						Replace("DataSource", "DataTarget").
						Replace("1130", "1140");
			Factory.Save();
			var logger = new ServiceTaskLogForTesting();
			ProcessUniversalMessage(message, logger);
			var newShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_HouseBill, shipment.JS_HouseBill.Replace("1130", "1140")));
			var newDeclaration = (BaseJobDeclaration)newShipment.DeclarationForDocuments;
			AssertEquals("newDeclaration.JE_OverrideFreightDefaults", overrideFreightDefaults, newDeclaration.JE_OverrideFreightDefaults);
		}

		protected void ProcessUniversalMessage(IEDIMessage message, ISimpleLogger logger)
		{
			new UniversalMessageProcessingManager(logger).Process(message);
		}

		protected IEDIMessage GetQueuedUniversalShipmentMessage(ForwardingShipment shipment)
		{
			return GetQueuedUniversalShipmentMessageCore(shipment);
		}

		protected IEDIMessage GetQueuedUniversalShipmentMessage(BaseJobDeclaration declaration)
		{
			return GetQueuedUniversalShipmentMessageCore(declaration);
		}

		IEDIMessage GetQueuedUniversalShipmentMessageCore(BusinessObject bizObj)
		{
			var sourceBOManager = bizObj.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var actionInfo = new ActionInfo(RecipientRoleType.ORP, bizObj);
			var writer = sourceBOManager.GetShipmentDataObjectWriter(new DataWritingManager(actionInfo));
			var dataObject = (UniversalDataBuss.DataObjects.Universal.Shipment)writer.GetDataObject(bizObj);
			ZString messageText;
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter().WriteXML(dataObject, stream);
				using (var reader = new StreamReader(stream))
				{
					messageText = reader.ReadToEnd();
				}
			}
			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_MessageText = messageText;
			Factory.Save();
			return message;
		}

		protected virtual void SetupPackingDetail(ForwardingShipment shipment, ForwardingConsol consol)
		{
			ForwardingPackLine packingLine;
			if (shipment.OuterPackLines.Count == 0)
			{
				packingLine = shipment.OuterPackLines.AddNew();
				packingLine.JL_PackageCount = shipment.JS_OuterPacks;
				packingLine.JL_F3_NKPackType = shipment.JS_F3_NKPackType;
			}
			else
			{
				packingLine = shipment.OuterPackLines[0];
			}
			packingLine.SetContainer(consol, consol.Containers[0]);
		}

		protected virtual void ClearDataForSynchronisationDetection(BaseJobDeclaration declaration)
		{
		}

		protected virtual void SetupRegistriesForDetechEnabledForUniversalXml(ZString messageType)
		{
			eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CustomsDataRegistry.Instance.PopulateOwnersRef.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected virtual bool ShouldIgnoreInfoForDetection(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case BaseJobDeclaration.Schema.JE_RL_NKPortOfFirstArrival:
				case BaseJobDeclaration.Schema.JE_DateOfFirstArrival:
					var dec = (BaseJobDeclaration)info.BizObj;
					return !dec.IsFirstArrivalDateAndPortUsed;
				case BaseJobDeclaration.Schema.JE_ScreeningStatus: // It's set to UNK
				case BaseJobDeclaration.Schema.JE_RS_NKServiceLevel: // The getter use Shipment
				case BaseCusContainer.Schema.CO_ContainerNumber: // CO_ContainerNumber is set on creation by synchroniser
				case BaseCusContainer.Schema.CO_RC: // CO_RC is set on creation by synchroniser
				case Bill.Schema.CU_NoOfPacks:  // Only synched for buyer's consol, this test is not a BCN, so we should not care, pfff
				case Bill.Schema.CU_PackType: // Only synched for buyer's consol, this test is not a BCN, so we should not care, pfff
					return true;
			}
			return false;
		}

		#region Implementation

		protected virtual void SetupForDetection(ForwardingShipment shipment, ZString origin, ZString destination)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.ConsignorPK = Consignor.PK;
			shipment.ConsigneePK = Consignee.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_HouseBill = "hb1511131130a"; // need to be in lowercase
			shipment.JS_RS_NKServiceLevel = "XyZ";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(-2);
			shipment.JS_E_ARV = ZDateTime.Today.AddDays(+2);
			shipment.JS_ActualVolume = 1.5m;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicYards;
			shipment.JS_ActualWeight = 1.654m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.JS_GoodsDescription = "GooDS AS GOOD";
			shipment.JS_GoodsValue = 15000m;
			shipment.GoodsValueCurrencyPK = GlbCompany.CurrentCompany.LocalCurrency.PK;
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Box;
			shipment.JS_HouseBillIssueDate = ZDateTime.Today;

			var orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "OrdER1";

			var itNumber = shipment.Numbers.AddNew();
			itNumber.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			itNumber.CE_EntryNum = "It31130";
			itNumber.CE_IssueDate = ZDateTime.Today;
			itNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
		}

		protected virtual void SetupForDetection(ForwardingConsol consol, ZString loadPort, ZString dischargePort, ZString firstArrival)
		{
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "mb1511131130a"; // need to be in lowercase
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_RL_NKPortOfFirstArrival = firstArrival;
			consol.JK_DatePortOfFirstArrival = ZDateTime.Today;
			consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = Org1.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = Org2.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = Org3.MainAddress.PK;
			consol.JK_OA_PackDepotAddress = Consignee.MainAddress.PK;
			consol.JK_BookingReference = "Bk1130";
			consol.JK_MasterBillIssueDate = ZDateTime.Today.AddDays(-1);

			var transport0 = consol.Transports[0];
			transport0.JW_Vessel = "VESsel NAME";
			transport0.JW_VoyageFlight = "v1130a"; // need to be in lowercase

			var container = consol.Containers.AddNew();
			SetupForDetection(container);
		}

		protected virtual void SetupForDetection(ForwardingContainer container)
		{
			container.JC_ContainerNum = "ConT1130";
			container.JC_RC = ContainerType1.PK;
			container.JC_SealNum = "Sl1130";
			container.JC_AdditionalSealNum = "sL21130";
		}

		RefContainer ContainerType1
		{
			get { return containerType1 ?? (containerType1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20NOR")); }
		}
		RefContainer containerType1;

		OrgHeader Org1
		{
			get
			{
				if (org1 == null)
				{
					org1 = CreateOrg1ForDetection();
				}
				return org1;
			}
		}
		OrgHeader org1;

		protected virtual OrgHeader CreateOrg1ForDetection()
		{
			return CreateOrganisation("ORG1@#123", "ORG1 NAME", "ORG1 ADDRESS 1");
		}

		OrgHeader Org2
		{
			get
			{
				if (org2 == null)
				{
					org2 = CreateOrg2ForDetection();
				}
				return org2;
			}
		}
		OrgHeader org2;

		protected virtual OrgHeader CreateOrg2ForDetection()
		{
			return CreateOrganisation("ORG2@#223", "ORG2 NAME", "ORG2 ADDRESS 1");
		}

		OrgHeader Org3
		{
			get
			{
				if (org3 == null)
				{
					org3 = CreateOrg3ForDetection();
				}
				return org3;
			}
		}
		OrgHeader org3;

		protected virtual OrgHeader CreateOrg3ForDetection()
		{
			return CreateOrganisation("ORG3@#323", "ORG3 NAME", "ORG3 ADDRESS 1");
		}

		OrgHeader Consignor
		{
			get
			{
				if (consignor == null)
				{
					consignor = CreateConsignorForDetection();
				}
				return consignor;
			}
		}
		OrgHeader consignor;

		protected virtual OrgHeader CreateConsignorForDetection()
		{
			var org = CreateOrganisation("CGN!@#123", "CONSIGNOR NAME", "CONSIGNOR ADDRESS 1");
			org.OH_IsConsignor = true;
			return org;
		}

		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = CreateConsigneeForDetection();
				}
				return consignee;
			}
		}
		OrgHeader consignee;

		protected virtual OrgHeader CreateConsigneeForDetection()
		{
			var org = CreateOrganisation("CNE!@#123", "CONSIGNEE NAME", "CONSIGNEE ADDRESS 1");
			org.OH_IsConsignee = true;
			return org;
		}

		OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					carrier = CreateCarrierForDetection();
				}
				return carrier;
			}
		}
		OrgHeader carrier;

		protected virtual OrgHeader CreateCarrierForDetection()
		{
			var org = CreateOrganisation("CAR!@#123", "CARRIER NAME", "CARRIER ADDRESS 1");
			org.OH_IsShippingLine = true;
			return org;
		}

		protected virtual OrgHeader CreateOrganisation(ZString code, ZString fullName, ZString address1)
		{
			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code) ?? Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = fullName;
			org.MainAddress.OA_Address1 = address1;
			return org;
		}

		RefUNLOCO ForeignPort1
		{
			get
			{
				if (foreignPort1 == null)
				{
					foreignPort1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
					SetupForeignPort1(foreignPort1);
				}
				return foreignPort1;
			}
		}
		RefUNLOCO foreignPort1;

		protected virtual void SetupForeignPort1(RefUNLOCO port)
		{
		}

		RefUNLOCO ForeignPort2
		{
			get
			{
				if (foreignPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, ForeignPort1.PK);
					foreignPort2 = Factory.LoadTop1<RefUNLOCO>(query);
					SetupForeignPort2(foreignPort2);
				}
				return foreignPort2;
			}
		}
		RefUNLOCO foreignPort2;

		protected virtual void SetupForeignPort2(RefUNLOCO port)
		{
		}

		RefUNLOCO ForeignPort3
		{
			get
			{
				if (foreignPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { ForeignPort1.PK, ForeignPort2.PK });
					foreignPort3 = Factory.LoadTop1<RefUNLOCO>(query);
					SetupForeignPort3(foreignPort3);
				}
				return foreignPort3;
			}
		}
		RefUNLOCO foreignPort3;

		protected virtual void SetupForeignPort3(RefUNLOCO port)
		{
		}

		RefUNLOCO LocalPort1
		{
			get
			{
				if (localPort1 == null)
				{
					localPort1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
					SetupLocalPort1(localPort1);
				}
				return localPort1;
			}
		}
		RefUNLOCO localPort1;

		protected virtual void SetupLocalPort1(RefUNLOCO port)
		{
		}

		RefUNLOCO LocalPort2
		{
			get
			{
				if (localPort2 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, LocalPort1.PK);
					localPort2 = Factory.LoadTop1<RefUNLOCO>(query);
					SetupLocalPort2(localPort2);
				}
				return localPort2;
			}
		}
		RefUNLOCO localPort2;

		protected virtual void SetupLocalPort2(RefUNLOCO port)
		{
		}

		RefUNLOCO LocalPort3
		{
			get
			{
				if (localPort3 == null)
				{
					var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, new[] { LocalPort1.PK, LocalPort2.PK });
					localPort3 = Factory.LoadTop1<RefUNLOCO>(query);
					SetupLocalPort3(localPort3);
				}
				return localPort3;
			}
		}
		RefUNLOCO localPort3;

		protected virtual void SetupLocalPort3(RefUNLOCO port)
		{
		}

		protected BaseJobDeclaration declaration;

		protected ForwardingConsol Consol
		{
			get { return consol ?? (consol = CreateConsol()); }
		}
		ForwardingConsol consol;

		protected Transport Transport
		{
			get { return transport ?? (transport = CreateTransport(Consol)); }
		}
		Transport transport;

		protected ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = CreateShipment(Consol)); }
		}
		ForwardingShipment shipment;

		protected JobDeclarationSynchroniser decSynchroniser;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetJobDeclaration();
			declaration.JE_JS = Shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(Consol, declaration);
			decSynchroniser = declaration.ShipmentSynchroniser;
		}

		protected virtual ForwardingShipment CreateShipment(ForwardingConsol consol)
		{
			var result = Consol.Shipments.AddNew();
			result.JS_RL_NKDestination = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			return result;
		}

		protected virtual ForwardingConsol CreateConsol()
		{
			var result = CreateFCLConsol();
			result.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AAA";
			return result;
		}

		protected virtual Transport CreateTransport(ForwardingConsol consol)
		{
			return consol.Transports[0];
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		void AssertDontSyncJE_OwnerRefIfItIsAutoAssigned(bool autoAssigned)
		{
			BaseJobDeclaration declaration = GetJobDeclaration();
			if (autoAssigned)
			{
				declaration.JE_OH_Importer = Factory.New(typeof(OrgHeader)).PK;
				declaration.Importer.MiscServ.OM_IMAutoImpJobRefered = true;
			}

			ForwardingShipment shipment = CreateImportShipment();
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();
			JobDeclarationSynchroniser sync = new JobDeclarationSynchroniser(declaration);
			sync.SetEnabled(true, false);

			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "123";
			declaration.AttachedOrders.Add(order1);
			AssertEquals(autoAssigned ? "" : "123", declaration.JE_OwnerRef);
		}

		protected ForwardingShipment CreateImportShipment()
		{
			var result = Factory.New<ForwardingShipment>();
			result.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			result.JS_RL_NKOrigin = PortNotInCountry(GlbBranch.CurrentBranch.GB_RL_NKHomePort.SubstringSafe(0, 2));
			return result;
		}

		ZString PortNotInCountry(ZString countryToExclude)
		{
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, countryToExclude);
			var result = Factory.LoadTop1<RefUNLOCO>(filter);
			return result.RL_Code;
		}

		#endregion
	}
}
