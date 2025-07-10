using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class JobDeclarationSynchroniserTest : Customs.Business.Testing.JobDeclarationSynchroniserTest
	{
		public void TestSetHazardousCargoByShipment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HBL1";
			declaration.JE_JS = shipment.PK;
			AssertNotNull(declaration.Shipment);

			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "HAZ1";
			commodityCode.RH_IsHazardous = true;

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_RH_NKCommodityCode = "HAZ1";

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_HazardousCargo);

			shipment.OuterPackLines.RemoveAndDeleteAll();
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(YesNoDefaultList.Codes.No, declaration.US_HazardousCargo);

			var commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "KNZ";
			commodityCode2.RH_IsHazardous = true;
			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_RH_NKCommodityCode = "KNZ";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_HazardousCargo);

			line2.JL_RH_NKCommodityCode = "";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(YesNoDefaultList.Codes.No, declaration.US_HazardousCargo);

			line2.JL_RH_NKCommodityCode = "KNZ";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_HazardousCargo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declaration.US_HazardousCargo);
		}

		protected override string ExpectedJE_TotalNoOfPacksPackTypeForTestSynchroniserBuyersConsolLead
		{
			get { return "PAL"; }
		}

		public void TestShouldNotStopSynchWhenDecMessagesAreAdded_CS00269857()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			declarationUS.ShipmentSynchroniser.SetEnabled(true, false);
			((Integration.Customs.IJobDeclarationWithShipmentSynchonisation)declarationUS).SynchroniseWithShipmentIfNeeded();

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.JE_MasterBill = "80954789";
			declarationUS.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "ZXZZ";
			declarationUS.Factory.Save();

			AssertEquals("one Carrier query message is generated and added to declaration.Messages", 1, declarationUS.Messages.Count);

			Assert("Should remain readonly", declarationUS.JE_OH_SupplierInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestFieldSynchroniserEnumerationNotModified()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			testHelper.Consignee.OH_RL_NKClosestPort = "USLAX";
			testHelper.Consignor.OH_RL_NKClosestPort = "AUSYD";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
		}

		public new void TestSynchronise_FromConsolForAir()
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
			AssertEquals("Flight No - consolSingaporeER.JK_JX_JV_VoyageFlight", "QF023", declaration.JE_VoyageFlightNo);
			AssertEquals("Shipping Line", consolSingaporeER.ShippingLinePK, declaration.JE_OH_ShippingLine);
			AssertEquals("CTO", consolSingaporeER.JK_OA_ArrivalCTOAddress, declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals("UnpackDepot", consolSingaporeER.JK_OA_UnpackDepotAddress, declaration.DepotDocAddress.E2_OA_Address);
			AssertEquals("Master bill num", consolSingaporeER.JK_MasterBillNum, declaration.JE_MasterBill);
		}

		public new void TestVesselVoyageSyncroniseToCorrectTransport()
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
			transport1.JW_VoyageFlight = "AA2";
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
			AssertEquals("Voyage", "AA002", declaration.JE_VoyageFlightNo);
		}

		public new void TestVesselVoyageSyncroniseToCorrectTransportForExport()
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
			transport1.JW_VoyageFlight = "CC12";
			transport1.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			AssertEquals("Vessel", "2222", declaration.JE_VesselName);
			AssertEquals("Voyage", "CC012", declaration.JE_VoyageFlightNo);
		}

		public void TestConsolFIRMSCodeSynchronisation()
		{
			OrgHeader cfsOrganisation = Factory.New<OrgHeader>();
			cfsOrganisation.MainAddress.OA_Address1 = "TEST";
			OrgCusCode cusCode = cfsOrganisation.MainAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode.OK_CustomsRegNo = "A001";

			OrgHeader ctoOrganisation = Factory.New<OrgHeader>();
			ctoOrganisation.MainAddress.OA_Address1 = "TEST1";
			OrgCusCode cusCode1 = ctoOrganisation.MainAddress.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode1.OK_CustomsRegNo = "B002";

			consol.JK_OA_UnpackDepotAddress = cfsOrganisation.MainAddress.PK;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("A001", declarationUS.US_US_NKLocationOfGoods);

			consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			consol.JK_OA_ArrivalCTOAddress = ctoOrganisation.MainAddress.PK;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("B002", declarationUS.US_US_NKLocationOfGoods);
		}

		public void TestShipmentITNumberDateSynchronisation()
		{
			testHelper.Consignee.OH_RL_NKClosestPort = "USLAX";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.JS_HouseBill = "H1";

			string itNo = "123456789";
			ZDateTime itDate = new ZDateTime(2008, 9, 11);

			CusEntryNumber number = shipment.Numbers.AddNew();
			number.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			number.CE_EntryNum = itNo;
			number.CE_IssueDate = itDate;
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			SetTransportMode(Core.Constants.TransportModes.Sea);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(itNo, declarationUS.JE_PrimaryITNumber);
			AssertEquals(itDate, declarationUS.US_ITDate);

			string itNo2 = "987654532";
			ZDateTime itDate2 = ZDateTime.Today;

			CusEntryNumber number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			number2.CE_EntryNum = itNo2;
			number2.CE_IssueDate = itDate2;
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			declarationUS.JE_JS = ZGuid.Empty;
			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration2.JE_JS = shipment.PK;
			declaration2.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(itNo, declaration2.JE_PrimaryITNumber);
			AssertEquals(itDate, declaration2.US_ITDate);

			declaration2.JE_JS = ZGuid.Empty;
			shipment.Numbers.RemoveAndDeleteAll();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			JobDeclaration declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_JS = shipment.PK;
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration3.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			declaration3.LowestBills.Rebuild();
			AssertEquals("IT No for Shipment Empty, but comes from Arrival Consol", itNo2, declaration3.JE_PrimaryITNumber);
			AssertEquals("IT Date for Shipment Empty, but comes from Arrival Consol", itDate2, declaration3.US_ITDate);

			consol.Numbers.RemoveAndDeleteAll();
			declaration3.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("No Developer Error expected after consol numbers have been deleted", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestDirectConsolWithITNumberDateSynchronisation()
		{
			string itNo = "123456789";
			ZDateTime itDate = new ZDateTime(2008, 9, 11);

			CusEntryNumber number = shipment.Numbers.AddNew();
			number.CE_EntryType = UnitedStatesAdditionalReferenceNumberTypes.Codes.IT;
			number.CE_EntryNum = itNo;
			number.CE_IssueDate = itDate;
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "DRT";
			consol.JK_MasterBillNum = "OPLU342980";
			Assert(consol.IsDirect);

			shipment.JS_HouseBill = "";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USCHI";
			shipment.ConsigneePK = importer.PK;
			shipment.ConsignorPK = supplier.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_GB = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew().PK;
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("IT No for Shipment Empty, but comes from Arrival Consol", itNo, declaration.JE_PrimaryITNumber);
			AssertEquals("IT Date for Shipment Empty, but comes from Arrival Consol", itDate, declaration.US_ITDate);
			AssertEquals("one bill only", 1, declaration.Bills.Count);
		}

		public void TestUS_SchDExportSynchronisation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "7856", "7856 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3242", "3242 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			SetupUSPortDetails();
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			declarationUS.JE_TransportMode = declarationUS.TransportModeAirCodeForTesting;
			declarationUS.US_RL_NKPortOfExport = uszzz.Code;
			AssertEquals(false, declarationUS.IsImport);
			AssertEquals("7856", declarationUS.US_SchDExport);

			declarationUS.JE_TransportMode = declarationUS.TransportModeSeaCodeForTesting;
			AssertEquals("3242", declarationUS.US_SchDExport);

			declarationUS.US_RL_NKPortOfExport = auzzz.Code;
			AssertEquals("", declarationUS.US_SchDExport);

			consol.JK_RL_NKLoadPort = auzzz.Code;
			consol.JK_RL_NKDischargePort = uszzz.Code;
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declarationUS.US_SchDExport);
		}

		public void TestUS_UC_NKCountryOfExportSynchronisation()
		{
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.US_UC_NKCountryOfExport = ZString.Empty;
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("AU", declarationUS.US_UC_NKCountryOfExport);
			shipment.JS_RL_NKOrigin = "NZAKL";
			AssertEquals("NZ", declarationUS.US_UC_NKCountryOfExport);
			shipment.JS_RL_NKOrigin = "MMAKY";
			AssertEquals("MM", declarationUS.US_UC_NKCountryOfExport);
			shipment.JS_RL_NKOrigin = "BUMMK";
			AssertEquals("BU", declarationUS.US_UC_NKCountryOfExport);

			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals("", declarationUS.US_UC_NKCountryOfExport);
			shipment.JS_RL_NKOrigin = "NZAKL";
			AssertEquals("", declarationUS.US_UC_NKCountryOfExport);
		}

		public void TestUS_SchDLoadingSynchronisation()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "7856", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3242", "Test Name", startDate, endDate);
			newFactory.Save();

			SetupUSPortDetails();
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			declarationUS.JE_TransportMode = declarationUS.TransportModeAirCodeForTesting;
			declarationUS.JE_RL_NKPortOfLoading = uszzz.Code;
			AssertEquals(false, declarationUS.IsImport);
			AssertEquals("7856", declarationUS.US_SchDLoading);

			declarationUS.JE_TransportMode = declarationUS.TransportModeSeaCodeForTesting;
			AssertEquals("3242", declarationUS.US_SchDLoading);

			consol.JK_RL_NKLoadPort = auzzz.Code;
			AssertEquals("", declarationUS.US_SchDLoading);

			consol.JK_RL_NKLoadPort = uszzz.Code;
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declarationUS.US_SchDLoading);
		}

		public void TestUS_SchDArrivalSynchronisationWithUNLOCOImport()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3242", "Test Name", startDate, endDate);
			newFactory.Save();

			SetupUSPortDetails();

			RefLocoMap map = uszzz.RefLocoMaps.AddNew();
			map.RY_LocalPortCode = "K891";
			map.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			map.RY_IsSystem = true;

			RefLocoMap map2 = uszzz.RefLocoMaps.AddNew();
			map2.RY_LocalPortCode = "ALL8";
			map2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			map2.RY_IsSystem = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = auzzz.Code;
			consol.JK_RL_NKDischargePort = uszzz.Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(consol.JK_RL_NKDischargePort, declaration.JE_RL_NKPortOfArrival);
			AssertEquals("3242", declaration.US_SchDArrival);

			uszzz.RefLocoMaps.DeleteAll();
			RefLocoMap map3 = uszzz.RefLocoMaps.AddNew();
			map3.RY_LocalPortCode = "SEA3LONG";
			map3.RY_IsSystem = true;
			AssertEquals(1, uszzz.RefLocoMaps.Count);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("", declaration.US_SchDArrival);
		}

		public void TestUS_SchDArrivalsShouldEmptyWhenExportDestinationIsNotInUS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60267", "60267 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			SetupUSPortDetails();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			shipment.JS_TotalPackageCount = 50;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Carton;
			shipment.JS_RL_NKOrigin = uszzz.Code;
			shipment.JS_RL_NKDestination = auzzz.Code;

			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, declarationUS.IsExport);
			AssertEquals(Core.Constants.TransportModes.Air, declarationUS.JE_TransportMode);

			AssertEquals("AUSYD", declarationUS.JE_RL_NKPortOfArrival);
			AssertEquals("", declarationUS.US_SchDArrival);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, declarationUS.IsExport);
			AssertEquals(Core.Constants.TransportModes.Sea, declarationUS.JE_TransportMode);
			AssertEquals("AUSYD", declarationUS.JE_RL_NKPortOfArrival);
			AssertEquals("60267", declarationUS.US_SchDArrival);
		}

		public void TestUS_SchDLoadingSynchronisationWithUNLOCO()
		{
			SetupUSPortDetails();

			RefLocoMap map2 = auzzz.RefLocoMaps.AddNew();
			map2.RY_LocalPortCode = "ALL8";
			map2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			map2.RY_IsSystem = true;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = auzzz.Code;
			consol.JK_RL_NKDischargePort = uszzz.Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(consol.JK_JX_JA_RL_NKPortOfLoading, declaration.JE_RL_NKPortOfLoading);
			AssertEquals("", declaration.US_SchDLoading);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			auzzz.RefLocoMaps.DeleteAll();

			RefLocoMap map4 = auzzz.RefLocoMaps.AddNew();
			map4.RY_LocalPortCode = "888LONG";
			map4.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			map4.RY_IsSystem = true;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("", declaration.US_SchDLoading);
		}

		public void TestUS_SchDExportSynchronisationWithUNLOCO()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3242", "3242 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			SetupUSPortDetails();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = uszzz.Code;
			consol.JK_RL_NKDischargePort = auzzz.Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			var exportTransport = consol.Transports.ExportTransport;
			exportTransport.JW_RL_NKLoadPort = uszzz.Code;
			Factory.Save();

			AssertNotNull(exportTransport);

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(uszzz.Code, declaration.US_RL_NKPortOfExport);
			AssertEquals("3242", declaration.US_SchDExport);

			RefLocoMap map = uszzz.RefLocoMaps.AddNew();
			map.RY_LocalPortCode = "K891";
			map.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;

			RefLocoMap map2 = uszzz.RefLocoMaps.AddNew();
			map2.RY_LocalPortCode = "ALL8";
			map2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;

			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("3242", declaration.US_SchDExport);

			uszzz.RefLocoMaps.DeleteAll();

			RefLocoMap map3 = uszzz.RefLocoMaps.AddNew();
			map3.RY_LocalPortCode = "K891";
			map3.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			map3.RY_LocalPortCode = "SEA3LONG";
			exportTransport.JW_RL_NKLoadPort = uszzz.Code;
			AssertEquals(uszzz.Code, declaration.US_RL_NKPortOfExport);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("", declaration.US_SchDExport);
		}

		public void TestUS_SchDArrivalSynchronisation()
		{
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "7856", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3242", "Test Name", startDate, endDate);
			Factory.Save();

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			SetupUSPortDetails();
			consol.JK_RL_NKDischargePort = auzzz.Code;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.JE_TransportMode = declarationUS.TransportModeAirCodeForTesting;
			declarationUS.JE_RL_NKPortOfArrival = uszzz.Code;
			AssertEquals("7856", declarationUS.US_SchDArrival);

			declarationUS.JE_TransportMode = declarationUS.TransportModeSeaCodeForTesting;
			AssertEquals("3242", declarationUS.US_SchDArrival);

			declarationUS.JE_RL_NKPortOfArrival = auzzz.Code;
			AssertEquals("", declarationUS.US_SchDArrival);

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, declarationUS.US_SchDArrivalInfo.ReadOnly);
			AssertEquals("", declarationUS.US_SchDArrival);

			declarationUS.JE_OverrideFreightDefaults = true;
			AssertEquals(false, declarationUS.US_SchDArrivalInfo.ReadOnly);

			declarationUS.JE_OverrideFreightDefaults = false;
			declarationUS.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;
			declarationUS.JE_TransportMode = declarationUS.TransportModeAirCodeForTesting;
			AssertEquals("", declarationUS.US_SchDArrival);

			declarationUS.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
			AssertEquals("", declarationUS.US_SchDArrival);
		}

		public void TestJE_RL_NKPortOfLoadingSynchronisation()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			declarationUS.JE_RL_NKPortOfLoading = ZString.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(false, declarationUS.IsImport);
			AssertEquals(true, declarationUS.JE_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals("USLAX", declarationUS.JE_RL_NKPortOfLoading);

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNull(consol.Transports.ImportTransport);
			AssertEquals("", declarationUS.JE_RL_NKPortOfLoading);

			consol.JK_RL_NKDischargePort = "USSFO";
			consol.JK_RL_NKLoadPort = "AUBNE";
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(true);
			AssertNotNull(consol.Transports.ImportTransport);
			AssertEquals("AUBNE", declarationUS.JE_RL_NKPortOfLoading);
		}

		public void TestUS_RL_NKPortOfExportSynchronisation()
		{
			Transport exportTransport = consol.Transports.ExportTransport;
			Factory.Save();
			AssertNotNull(exportTransport);
			declarationUS.US_RL_NKPortOfExport = ZString.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));

			AssertEquals(true, declarationUS.US_RL_NKPortOfExportInfo.ReadOnly);
			AssertEquals("USLAX", exportTransport.JW_RL_NKLoadPort);
			AssertEquals("USLAX", declarationUS.US_RL_NKPortOfExport);

			exportTransport.JW_RL_NKLoadPort = "USCHI";
			AssertEquals("USCHI", declarationUS.US_RL_NKPortOfExport);

			shipment.Consols.RemoveAndDeleteAll();
			declarationUS.JE_RL_NKPortOfLoading = "USSFO";
			AssertEquals("USSFO", declarationUS.US_RL_NKPortOfExport);
		}

		public override void TestSynchronise_FromShipmentOtherDetails()
		{
			Transport transport = Consol.Transports[0];
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var foreign = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, countryCode)).Code;
			transport.JW_RL_NKLoadPort = foreign;
			transport.JW_RL_NKDiscPort = "USLAX";
			Shipment.JS_RL_NKOrigin = foreign;
			Shipment.JS_RL_NKDestination = "USLAX";

			ZQuery uSConsignorFilter = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
			uSConsignorFilter.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "USLAX"); //we love deb
			var partyInCurrentCountry = Factory.LoadTop1<OrgHeader>(new ZQuery());
			partyInCurrentCountry.OH_RL_NKClosestPort = countryCode + "AAA";
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
			Shipment.JS_RL_NKOrigin = foreign;
			Shipment.JS_RL_NKDestination = "USLAX";

			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertEquals("Goods Description", Shipment.JS_GoodsDescription, declaration.JE_GoodsDescription);
			AssertEquals("Consignee", Shipment.ConsigneePK, declaration.JE_OH_Importer);
			AssertEquals("Consignor", Shipment.ConsignorPK, declaration.JE_OH_Supplier);
			AssertEquals("HouseBill", "H123456", declaration.JE_HouseBill);
			AssertEquals("ActualWeight", Shipment.JS_ActualWeight, declaration.JE_TotalWeight);
			AssertEquals("Unit of Weight", Shipment.JS_UnitOfWeight, declaration.JE_TotalWeightUnit);
			AssertEquals("ActualVolume", Shipment.JS_ActualVolume, declaration.JE_TotalVolume);
			AssertEquals("Unit of Volume", Shipment.JS_UnitOfVolume, declaration.JE_TotalVolumeUnit);
			AssertEquals("Outer packs", Shipment.JS_TotalPackageCount, declaration.JE_TotalNoOfPacks);
			AssertEquals("Outer pack type", ShippingOrPackingingUnitList.Codes.Carton, declaration.JE_TotalNoOfPacksPackType);
			AssertEquals("Origin port", Shipment.JS_RL_NKOrigin, declaration.JE_RL_NKOrigin);
			AssertEquals("Destination port", Shipment.JS_RL_NKDestination, declaration.JE_RL_NKFinalDestination);
		}

		public override void TestSeaTransportModeFormattingForImport()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertContainerModeSynchronisation();
		}

		void AssertContainerModeSynchronisation()
		{
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);

			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.Loose;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.ULD;
			AssertEquals("JE_ContainerMode", Enterprise.Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
		}

		public override void TestSeaTransportModeFormatting()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertContainerModeSynchronisation();
		}

		public override void TestJobDeclarationSynchroniser()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			Shipment.JS_RS_NKServiceLevel = "XYZ";
			AssertEquals("Declaration.JE_RS_NKServiceLevel", "XYZ", declaration.JE_RS_NKServiceLevel);

			Shipment.JS_TransportMode = "";
			Shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Declaration.JE_TransportMode", Enterprise.Core.Constants.TransportModes.Sea, declaration.JE_TransportMode);

			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("Declaration.JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);

			var testConsignor = GetOverseasConsignor();
			Shipment.ConsignorPK = testConsignor.PK;
			AssertEquals("Declaration.JE_OH_Supplier", testConsignor.PK, declaration.JE_OH_Supplier);

			var testConsignee = GetLocalConsignee();
			Shipment.ConsigneePK = testConsignee.PK;
			AssertEquals("Declaration.JE_OH_Importer", testConsignee.PK, declaration.JE_OH_Importer);

			Transport.JW_ATD = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_ExportDate", Consol.JK_JX_JA_A_DEP, declaration.JE_ExportDate);

			Transport.JW_ATA = ZDateTime.Empty;
			Transport.JW_ETA = ZDateTime.Empty;
			Consol.JK_DatePortOfFirstArrival = ZDateTime.Today.AddDays(2);
			AssertEquals(@"Declaration.JE_DateOfArrival should be empty, because we are not interested in DateOfFirstArrival.
					Date Of Arrival should come from Transport Leg", ZDateTime.Empty, declaration.JE_DateOfArrival);

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

			AssertEquals("Declaration.JE_TotalNoOfPacksPackType", ShippingOrPackingingUnitList.Codes.Pallet, declaration.JE_TotalNoOfPacksPackType);

			Shipment.JS_GoodsDescription = TestGoodsDescription;
			AssertEquals("Declaration.JE_GoodsDescription", TestGoodsDescription, declaration.JE_GoodsDescription);

			Shipment.JS_E_ARV = ZDateTime.Today.AddDays(1);
			AssertEquals("Declaration.JE_DateAtFinalDestination", Shipment.JS_E_ARV, declaration.JE_DateAtFinalDestination);

			Shipment.JS_E_DEP = ZDateTime.Today.AddDays(4);
			AssertEquals("Declaration.JE_DateAtOrigin", Shipment.JS_E_DEP, declaration.JE_DateAtOrigin);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "USLAX";

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "USLAX";

			Transport.JW_ATA = ZDateTime.Empty;
			Transport.JW_ETA = ZDateTime.Empty;
			Consol.JK_DatePortOfFirstArrival = ZDateTime.Today.AddDays(2);
			AssertEquals("Declaration.JE_DateOfArrival should not be empty", Shipment.JS_E_ARV, declaration.JE_DateOfArrival);
		}

		public override void TestLoadAndDischargeSyncroniseToCorrectTransport_Export()
		{
			Assert("This test is currently not applicable for US", true);
		}

		public void TestJE_Packs()
		{
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			shipment.JS_TotalPackageCount = 50;
			shipment.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.Carton;

			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, declarationUS.IsExport);
			AssertEquals(10, declarationUS.JE_TotalNoOfPacks);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Pallet, declarationUS.JE_TotalNoOfPacksPackType);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.ConsigneePK = testHelper.Consignor.PK;
			shipment.ConsignorPK = testHelper.Consignee.PK;

			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, declarationUS.IsImport);
			AssertEquals(50, declarationUS.JE_TotalNoOfPacks);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Carton, declarationUS.JE_TotalNoOfPacksPackType);

			shipment.JS_TotalPackageCount = ZInt.Zero;
			shipment.JS_F3_NKTotalCountPackType = "";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, declarationUS.IsImport);
			AssertEquals(10, declarationUS.JE_TotalNoOfPacks);
			AssertEquals(ShippingOrPackingingUnitList.Codes.Pallet, declarationUS.JE_TotalNoOfPacksPackType);
		}

		public void TestUS_DateOfExportSynchronisation()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "AUSYD";

			shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);

			declarationUS = Factory.New<JobDeclaration>();
			declarationUS.JE_JS = shipment.PK;
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Export;

			TestHelper.MakeConsolRelevantToDeclaration(consol, declarationUS);
			Transport exportTransport = consol.Transports.ExportTransport;
			AssertNotNull(exportTransport);
			ZDateTime date1 = new ZDateTime(2007, 11, 26);
			ZDateTime date2 = new ZDateTime(2007, 11, 27);
			ZDateTime date3 = new ZDateTime(2007, 11, 28);
			exportTransport.JW_ETD = date3;
			exportTransport.JW_ATD = date1;

			declarationUS.US_DateOfExport = ZDateTime.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			declarationUS.US_RL_NKPortOfExport = "USSFO";
			declarationUS.JE_RL_NKPortOfLoading = "USLAX";
			declarationUS.JE_ExportDate = date2;

			AssertEquals(true, declarationUS.US_DateOfExportInfo.ReadOnly);
			AssertEquals(date1, declarationUS.US_DateOfExport);

			exportTransport.JW_ATD = ZDateTime.Empty;
			AssertEquals(date3, declarationUS.US_DateOfExport);

			exportTransport.JW_ETD = ZDateTime.Empty;
			AssertEquals(declarationUS.JE_ExportDate, declarationUS.US_DateOfExport);

			exportTransport.JW_ETD = date1;
			AssertEquals(date1, declarationUS.US_DateOfExport);

			exportTransport.JW_ATD = date3;
			AssertEquals(date3, declarationUS.US_DateOfExport);
			Factory.Save();
			shipment.Consols.RemoveAndDeleteAll();
			declarationUS.JE_ExportDate = date1;
			AssertEquals(date1, declarationUS.US_DateOfExport);

			var oldConsol = consol;
			consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			TestHelper.MakeConsolRelevantToDeclaration(consol, declarationUS);
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(true);
			oldConsol.Delete();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETD = new ZDateTime(2010, 6, 2);
			Transport transport2 = consol.Transports[0];
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_ETD = new ZDateTime(2010, 6, 1);
			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "USLAX";
			transport3.JW_ETD = new ZDateTime(2010, 6, 5);
			Transport transport4 = consol.Transports.AddNew();
			transport4.JW_LegOrder = 4;
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_RL_NKLoadPort = "USLAX";
			transport4.JW_RL_NKDiscPort = "USCHI";
			transport4.JW_ETD = new ZDateTime(2010, 6, 6);
			Transport transport5 = shipment.Transports.AddNew();
			transport5.JW_LegOrder = 5;
			transport5.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport5.JW_RL_NKLoadPort = "USCHI";
			transport5.JW_RL_NKDiscPort = "CATOR";
			transport5.JW_ETD = new ZDateTime(2010, 6, 7);
			declarationUS.JE_ExportDate = date1;

			AssertEquals(new ZDateTime(2010, 6, 1), declarationUS.US_DateOfExport);

			transport1.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals(new ZDateTime(2010, 6, 2), declarationUS.US_DateOfExport);

			transport1.JW_LegOrder = 2;
			AssertEquals(new ZDateTime(2010, 6, 1), declarationUS.US_DateOfExport);

			transport1.JW_ATD = new ZDateTime(2010, 5, 1);
			AssertEquals(new ZDateTime(2010, 5, 1), declarationUS.US_DateOfExport);

			transport2.JW_LegOrder = 1;
			AssertEquals(new ZDateTime(2010, 6, 1), declarationUS.US_DateOfExport);

			transport2.JW_RL_NKDiscPort = "AUADL";
			AssertEquals(new ZDateTime(2010, 5, 1), declarationUS.US_DateOfExport);

			transport1.JW_RL_NKLoadPort = "NZAKL";
			AssertEquals(date1, declarationUS.US_DateOfExport);

			transport3.JW_RL_NKLoadPort = "AUBJD";
			AssertEquals(new ZDateTime(2010, 6, 5), declarationUS.US_DateOfExport);
		}

		public void TestJE_ExportDateSynchronisation()
		{
			consol.Transports[0].JW_ETD = ZDateTime.Today;
			declarationUS.JE_ExportDate = ZDateTime.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZDateTime.Today, declarationUS.JE_ExportDate);

			consol.JK_RL_NKLoadPort = "GBLIV";
			consol.JK_RL_NKDischargePort = "USORD";
			consol.Transports.RemoveAndDeleteAll();

			Transport transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "GBLIV";
			transport1.JW_RL_NKDiscPort = "GBLHR";
			transport1.JW_ETD = ZDateTime.Today.AddDays(-3);
			transport1.JW_ETA = ZDateTime.Today.AddDays(-3);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "GBLHR";
			transport2.JW_RL_NKDiscPort = "USJFK";
			transport2.JW_ETD = ZDateTime.Today.AddDays(-2);
			transport2.JW_ETA = ZDateTime.Today.AddDays(-2);

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "USJFK";
			transport3.JW_RL_NKDiscPort = "USORD";
			transport3.JW_ETD = ZDateTime.Today;
			transport3.JW_ETA = ZDateTime.Today;

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.JE_ExportDate = ZDateTime.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZDateTime.Today.AddDays(-2), declarationUS.JE_ExportDate);

			transport1.JW_RL_NKDiscPort = "PR123";
			transport2.JW_RL_NKLoadPort = "PR123";
			declarationUS.JE_ExportDate = ZDateTime.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZDateTime.Today.AddDays(-3), declarationUS.JE_ExportDate);
		}

		public void TestJE_DateOfArrival()
		{
			consol.Transports[0].JW_ATA = ZDateTime.Today;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZDateTime.Today, declarationUS.JE_DateOfArrival);

			consol.JK_RL_NKLoadPort = "GBLIV";
			consol.JK_RL_NKDischargePort = "USORD";
			consol.Transports.RemoveAndDeleteAll();

			Transport transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "GBLIV";
			transport1.JW_RL_NKDiscPort = "GBLHR";
			transport1.JW_ATA = ZDateTime.Today.AddDays(-3);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "GBLHR";
			transport2.JW_RL_NKDiscPort = "USJFK";
			transport2.JW_ATA = ZDateTime.Today.AddDays(-2);

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "USJFK";
			transport3.JW_RL_NKDiscPort = "USORD";
			transport3.JW_ATA = ZDateTime.Today;

			Transport importTransport = consol.Transports.ImportTransport;
			AssertNotNull(importTransport);

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.JE_DateOfArrival = ZDateTime.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZDateTime.Today.AddDays(-2), declarationUS.JE_DateOfArrival);
		}

		public void TestJE_DateOfArrivalSynchronisationForExport()
		{
			consol.Transports.RemoveAndDeleteAll();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "USDEN";
			transport1.JW_ETD = ZDateTime.Today.AddDays(-4);
			transport1.JW_ETA = ZDateTime.Today.AddDays(-3);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USDEN";
			transport2.JW_RL_NKDiscPort = "AUBNE";
			transport2.JW_ETD = ZDateTime.Today.AddDays(-2);
			transport2.JW_ETA = ZDateTime.Today.AddDays(-1);

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "AUBNE";
			transport3.JW_RL_NKDiscPort = "AUSYD";
			transport3.JW_ETD = ZDateTime.Today.AddDays(-1);
			transport3.JW_ETA = ZDateTime.Today;

			declarationUS.JE_DateOfArrival = ZDateTime.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));

			AssertEquals("JE_DateOfArrival", transport2.JW_ETA, declarationUS.JE_DateOfArrival);
		}

		public void TestPackingModeSynchronisationForAirAndRail()
		{
			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declarationUS.JE_ContainerMode);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declarationUS.JE_ContainerMode);

			shipment.DepartureContainers.RemoveAndDeleteAll();
			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declarationUS.JE_ContainerMode);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declarationUS.JE_ContainerMode);

			shipment.JS_RL_NKOrigin = testHelper.NZAKL.RL_Code;
			shipment.JS_RL_NKDestination = testHelper.CATOR.RL_Code;
			consol.JK_RL_NKLoadPort = testHelper.AUSYD.RL_Code;
			consol.JK_RL_NKDischargePort = testHelper.USLAX.RL_Code;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = testHelper.USLAX.RL_Code;
			consol2.JK_RL_NKDischargePort = testHelper.CATOR.RL_Code;
			container = consol2.Containers.AddNew();

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declarationUS.JE_ContainerMode);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declarationUS.JE_ContainerMode);

			container.JC_JK = consol.PK;
			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declarationUS.JE_ContainerMode);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declarationUS.JE_ContainerMode);

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Export;
			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declarationUS.JE_ContainerMode);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.NonContainerised, declarationUS.JE_ContainerMode);

			container.JC_JK = consol2.PK;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declarationUS.JE_ContainerMode);

			consol2.JK_TransportMode = Core.Constants.TransportModes.Rail;
			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(Core.Constants.ContainerModes.Containerised, declarationUS.JE_ContainerMode);
		}

		public void TestJE_OA_ManufacturerAddress()
		{
			var org1 = CreateOrganisation("MANUF1", "MANUFACTURER 1", "MANUFACTURER ADDRESS 1");
			shipment.DocAddresses.AddNew(org1.MainAddress, MasterFiles.Integration.DocAddressType.Manufacturer);

			var org2 = CreateOrganisation("MANUF2", "MANUFACTURER 2", "MANUFACTURER ADDRESS 2");
			declarationUS.JE_OA_ManufacturerAddress = org2.MainAddress.PK;

			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(org1.MainAddress.PK, declarationUS.JE_OA_ManufacturerAddress);

			var org3 = CreateOrganisation("MANUF3", "MANUFACTURER 3", "MANUFACTURER ADDRESS 3");
			var manufacturerAddress = shipment.DocAddresses.FindByDocAddressType(MasterFiles.Integration.DocAddressType.Manufacturer);
			manufacturerAddress.E2_OA_Address = org3.MainAddress.PK;
			AssertEquals(org3.MainAddress.PK, declarationUS.JE_OA_ManufacturerAddress);

			manufacturerAddress.E2_AddressOverride = true;
			manufacturerAddress.E2_Address1 = "CUSTOM MANUFACTURER ADDRESS";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Should NOT synchronise if Override flag is on", org3.MainAddress.PK, declarationUS.JE_OA_ManufacturerAddress);

			manufacturerAddress.Delete();
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Should NOT synchronise if no manunfacturer is present on Shipment", org3.MainAddress.PK, declarationUS.JE_OA_ManufacturerAddress);
		}

		public void TestJE_OH_ForwarderIsSetFromCurrentBranchOrgPK()
		{
			OrgHeader org = GlbBranch.CurrentBranch.OrgProxy;
			org.OH_IsForwarder = ZBool.True;
			declarationUS.JE_OH_Forwarder = ZGuid.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(org.PK, declarationUS.JE_OH_Forwarder);

			declarationUS.JE_OH_Forwarder = ZGuid.NewZGuid();
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertNotEquals(org.PK, declarationUS.JE_OH_Forwarder);

			org.OH_IsForwarder = ZBool.False;
			declarationUS.JE_OH_Forwarder = ZGuid.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(ZGuid.Empty, declarationUS.JE_OH_Forwarder);
		}

		public void TestUS_ExportCoLoadCarrier()
		{
			SetTransportMode(Core.Constants.TransportModes.Sea);
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Export;

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_BookingReference = "BK123";
			consol.JK_MasterBillNum = "MWB123";
			var carrierOrg = Factory.New<OrgHeader>();
			carrierOrg.OH_Code = "CAR";
			carrierOrg.Addresses.AddNewMainAddress();

			consol.JK_OA_ShippingLineAddress_ZAddress.OrgPK = carrierOrg.PK;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("It should be syned from carrier.", carrierOrg.PK, declarationUS.JE_OH_ShippingLine);

			var coLoadOrg = Factory.New<OrgHeader>();
			coLoadOrg.Addresses.AddNewMainAddress();
			consol.JK_OA_CreditorAddress_ZAddress.OrgPK = coLoadOrg.PK;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("It should be syned from co-load.", coLoadOrg.PK, declarationUS.JE_OH_ShippingLine);

			consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("It should be syned from carrier.", carrierOrg.PK, declarationUS.JE_OH_ShippingLine);

			consol.JK_CoLoadMasterBill = "MB-123";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("It should be syned from co-load.", ZGuid.Empty, declarationUS.JE_OH_ShippingLine);
		}

		public void TestTransportReference()
		{
			SetTransportMode(Core.Constants.TransportModes.Air);
			consol.JK_BookingReference = "BK123";
			consol.JK_MasterBillNum = "MWB123";
			shipment.JS_HouseBill = "HWB123";
			declarationUS.US_TransportReference = ZString.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, declarationUS.IsExport);
			AssertEquals(true, declarationUS.US_TransportReferenceInfo.ReadOnly);
			AssertEquals("MWB-123", declarationUS.US_TransportReference);

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("", declarationUS.US_TransportReference);

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("MWB-123", declarationUS.US_TransportReference);

			SetTransportMode(Core.Constants.TransportModes.Sea);
			AssertEquals(true, declarationUS.US_TransportReferenceInfo.ReadOnly);
			AssertEquals("BK123", declarationUS.US_TransportReference);

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_CoLoadBookingReference = "CBR-123";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(true, declarationUS.IsExport);
			AssertEquals("It should be syned from co-load.", "CBR-123", declarationUS.US_TransportReference);
			consol.JK_CoLoadBookingReference = ZString.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("It should be syned from carrier.", "BK123", declarationUS.US_TransportReference);
			consol.JK_CoLoadMasterBill = "MB-123";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("It should be syned from co-load.", ZString.Empty, declarationUS.US_TransportReference);

			SetTransportMode(Core.Constants.TransportModes.Air);
			consol.JK_MasterBillNum = ZString.Empty;
			AssertEquals(true, declarationUS.US_TransportReferenceInfo.ReadOnly);
			AssertEquals("HWB123", declarationUS.US_TransportReference);

			SetTransportMode(Core.Constants.TransportModes.Road);
			AssertEquals(true, declarationUS.US_TransportReferenceInfo.ReadOnly);
			AssertEquals("", declarationUS.US_TransportReference);

			SetTransportMode(Core.Constants.TransportModes.Sea);
			shipment.ConsigneePK = testHelper.Consignor.PK;
			shipment.ConsignorPK = testHelper.Consignee.PK;
			consol.JK_MasterBillNum = "MWB-123";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(false, declarationUS.IsExport);
			AssertEquals("", declarationUS.US_TransportReference);
			AssertEquals(true, declarationUS.US_TransportReferenceInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Air);
			AssertEquals(false, declarationUS.IsExport);
			AssertEquals("", declarationUS.US_TransportReference);
			AssertEquals(true, declarationUS.US_TransportReferenceInfo.ReadOnly);
		}

		public void TestTransportModeSynchronisation()
		{
			SetTransportMode(Core.Constants.TransportModes.Road);
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.JE_TransportMode = ZString.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(TransportTypeList.Codes.Truck, declarationUS.JE_TransportMode);
			AssertEquals(Core.Constants.TransportModes.Road, shipment.JS_TransportMode);
		}

		protected override IDictionary<string, string> GetTransportModeMap() => new Dictionary<string, string>
		{
			{ Core.Constants.TransportModes.Air, TransportTypeList.Codes.Air },
			{ Core.Constants.TransportModes.Courier, TransportTypeList.Codes.Mail },
			{ Core.Constants.TransportModes.Road, TransportTypeList.Codes.Truck },
			{ Core.Constants.TransportModes.Sea, TransportTypeList.Codes.Sea },
		};

		public void TestMessageTypeChangeCallSynchronizationForHouseBill()
		{
			SetTransportMode(Core.Constants.TransportModes.Sea);

			consol.JK_MasterBillNum = "MAST0012";
			CusEntryNumber num1 = consol.Numbers.AddNew();
			num1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			num1.CE_EntryNum = "GTRE6543234";

			shipment.JS_HouseBill = "HWB123";
			CusEntryNumber num2 = shipment.Numbers.AddNew();
			num2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			num2.CE_EntryNum = "APLU5006003";

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Export;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declarationUS);
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declarationUS.JE_HouseBill);
			AssertEquals("", declarationUS.JE_HouseBillIssuerSCAC);
			AssertEquals("MAST0012", declarationUS.JE_MasterBill);
			AssertEquals("", declarationUS.JE_MasterBillIssuerSCAC);

			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declarationUS);
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("5006003", declarationUS.JE_HouseBill);
			AssertEquals("APLU", declarationUS.JE_HouseBillIssuerSCAC);
			AssertEquals("6543234", declarationUS.JE_MasterBill);
			AssertEquals("GTRE", declarationUS.JE_MasterBillIssuerSCAC);
		}

		public void TestTransportReferenceSync()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "USLAX";
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_HouseBill = "HAW1234";

				declarationUS = Factory.New<JobDeclaration>();
				declarationUS.JE_TransportMode = Core.Constants.TransportModes.Air;
				declarationUS.JE_JS = shipment.PK;

				declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
				AssertEquals(shipment.JS_HouseBill, declarationUS.US_TransportReference);

				var consol = shipment.Consols.AddNew();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "USLAX";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "12345678";
				declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
				AssertEquals("123-45678", declarationUS.US_TransportReference);
			}
		}

		#region Bills Synchronization

		public void TestMasterBillIssuerSCACSync()
		{
			consol.JK_MasterBillNum = "MWB123";
			AssertEquals("", declarationUS.JE_MasterBillIssuerSCAC);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			SetTransportMode(Core.Constants.TransportModes.Road);

			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Sea);
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			consol.JK_MasterBillNum = "AA1D5002003";//consol is not in the same factory as declaration
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			consol.JK_MasterBillNum = "8C1N5002003";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "APLU";
			carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.RailContainer;

			consol.JK_MasterBillNum = "AP1I5002003";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Air);
			var carrier = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, "800"));
			if (carrier == null)
			{
				carrier = Factory.New<RefAirline>();
				carrier.RM_EagleAddedAirlinePrefixOrAccountingCode = "800";
			}
			carrier.RM_TwoCharacterCode = "TY";
			carrier.RM_AirlineName1 = "TEST";

			declarationUS.JE_MasterBill = "";
			declarationUS.JE_MasterBillIssuerSCAC = "";
			consol.JK_MasterBillNum = "8005002003";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("TY", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			CusEntryNumber num1 = consol.Numbers.AddNew();
			num1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			num1.CE_EntryNum = "GTRE6543234";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("TY", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("GTRE", declarationUS.JE_MasterBillIssuerSCAC);
			Assert(declarationUS.JE_MasterBillIssuerSCACInfo.ReadOnly);

			declarationUS.JE_MasterBill = "";
			declarationUS.JE_MasterBillIssuerSCAC = "";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Should always be trimmed, regardless of whether the registry setting applies or not", "GTRE", declarationUS.JE_MasterBillIssuerSCAC);

			consol.Numbers.RemoveAndDeleteAll();
			consol.JK_MasterBillNum = "TR1W5002003";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declarationUS.JE_MasterBillIssuerSCAC);
		}

		public void TestMasterBillNumberSync()
		{
			consol.JK_MasterBillNum = "MWB123";
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("MWB123", declarationUS.JE_MasterBill);

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			SetTransportMode(Core.Constants.TransportModes.Road);
			TestHelper.MakeConsolRelevantToDeclaration(consol, declarationUS);
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("MWB123", declarationUS.JE_MasterBill);

			SetTransportMode(Core.Constants.TransportModes.Sea);
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declarationUS);
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("MWB123", declarationUS.JE_MasterBill);

			consol.JK_MasterBillNum = "AA1D5002003";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("AA1D5002003", declarationUS.JE_MasterBill);

			consol.JK_MasterBillNum = "8C1N5002003";
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("8C1N5002003", declarationUS.JE_MasterBill);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			consol.JK_MasterBillNum = "AP1I5002003";
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("AP1I5002003", declarationUS.JE_MasterBill);

			SetTransportMode(Core.Constants.TransportModes.Air);
			consol.JK_MasterBillNum = "0005002003";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("0005002003", declarationUS.JE_MasterBill);

			CusEntryNumber num1 = consol.Numbers.AddNew();
			num1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			num1.CE_EntryNum = "GTRE6543234";
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("0005002003", declarationUS.JE_MasterBill);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("6543234", declarationUS.JE_MasterBill);

			declarationUS.JE_MasterBill = "";
			declarationUS.JE_MasterBillIssuerSCAC = "";
			declarationUS.ShipmentSynchroniser.SetEnabled(false, false);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("Should always be trimmed, regardless of whether the registry setting applies or not", "6543234", declarationUS.JE_MasterBill);

			consol.Numbers.RemoveAndDeleteAll();
			consol.JK_MasterBillNum = "TREW5002003";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("TREW5002003", declarationUS.JE_MasterBill);
		}

		public void TestHouseBillNumberSync()
		{
			shipment.JS_HouseBill = "HWB123";
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declarationUS.JE_HouseBill);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			SetTransportMode(Core.Constants.TransportModes.Road);

			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declarationUS.JE_HouseBill);

			SetTransportMode(Core.Constants.TransportModes.Sea);
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declarationUS.JE_HouseBill);

			shipment.JS_HouseBill = "1005465230";
			AssertEquals("1005465230", declarationUS.JE_HouseBill);

			shipment.JS_HouseBill = "465230";
			AssertEquals("465230", declarationUS.JE_HouseBill);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			shipment.JS_HouseBill = "600890";
			AssertEquals("600890", declarationUS.JE_HouseBill);

			shipment.JS_HouseBill = "56230124";
			AssertEquals("56230124", declarationUS.JE_HouseBill);

			shipment.JS_HouseBill = "APLU";
			AssertEquals("APLU", declarationUS.JE_HouseBill);

			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_HouseBill = "IY465230";
			AssertEquals("IY465230", declarationUS.JE_HouseBill);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("IY465230", declarationUS.JE_HouseBill);
			AssertEquals("", declarationUS.JE_HouseBillIssuerSCAC);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("IY465230", declarationUS.JE_HouseBill);
		}

		#endregion

		public void TestGetFirmsCode()
		{
			// Conditions: CFS only on Shipment, no CFS/CTO on consol
			OrgHeader cfsOrganisation = Factory.New<OrgHeader>();
			cfsOrganisation.MainAddress.OA_Address1 = "TEST";
			OrgCusCode cusCode = cfsOrganisation.MainAddress.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode.OK_CustomsRegNo = "A001";

			shipment.JS_OA_ImportReleaseDepot = cfsOrganisation.MainAddress.PK;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("A001", declarationUS.US_US_NKLocationOfGoods);

			// Conditions: CFS on shipment and consol.
			OrgHeader cfsOrganisation2 = Factory.New<OrgHeader>();
			cfsOrganisation2.MainAddress.OA_Address1 = "TEST2";
			OrgCusCode cusCode2 = cfsOrganisation2.MainAddress.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode2.OK_CustomsRegNo = "B001";
			consol.JK_OA_UnpackDepotAddress = cfsOrganisation2.MainAddress.PK;

			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("A001", declarationUS.US_US_NKLocationOfGoods);

			// Condition: No CFS on shipment. CFS available on consol
			shipment.JS_OA_ImportReleaseDepot = Guid.Empty;
			declarationUS.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("B001", declarationUS.US_US_NKLocationOfGoods);

			// Condition: No consol, only shipment
			shipment.JS_OA_ImportReleaseDepot = Guid.Empty;
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_OA_ImportReleaseDepot = cfsOrganisation.MainAddress.PK;

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_JS = shipment1.PK;
			declaration1.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("A001", declaration1.US_US_NKLocationOfGoods);

			// Conditions: Org contains more than 1 addresses of FRM type, CFS on shipment and consol.
			OrgHeader cfsOrganisation3 = Factory.New<OrgHeader>();
			cfsOrganisation3.MainAddress.OA_Address1 = "TEST3";
			OrgCusCode cusCode3 = cfsOrganisation3.MainAddress.CustomsCodes.AddNew();
			cusCode3.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode3.OK_CustomsRegNo = "C001";
			var orgAddress3 = cfsOrganisation3.Addresses.AddNew();
			OrgCusCode cusCode4 = orgAddress3.CustomsCodes.AddNew();
			cusCode4.OK_CodeType = OrgCusCode.USACodeTypes.FIRMSCode;
			cusCode4.OK_CustomsRegNo = "D001";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_OA_ImportReleaseDepot = orgAddress3.PK;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_JS = shipment3.PK;
			declaration3.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("D001", declaration3.US_US_NKLocationOfGoods);
		}

		protected override bool ShouldIgnoreInfoForDetection(ZPropertyInfo info)
		{
			AddInfoJobDeclaration declarationInfo;
			JobDeclaration declaration;
			switch (info.Name)
			{
				case JobDeclaration.Schema.US_RL_NKPortOfExport:
				case JobDeclaration.Schema.US_SchDExport:
				case JobDeclaration.Schema.US_TransportReference:
					declarationInfo = info.BizObj as AddInfoJobDeclaration;
					declaration = declarationInfo == null ? null : declarationInfo.Declaration;
					return declaration == null || !declaration.IsExport;
				case JobDeclaration.Schema.US_UC_NKCountryOfExport:
				case JobDeclaration.Schema.US_HazardousCargo:
					declarationInfo = info.BizObj as AddInfoJobDeclaration;
					declaration = declarationInfo == null ? null : declarationInfo.Declaration;
					return declaration == null || !declaration.IsImport;
				case JobDeclaration.Schema.JE_OA_ManufacturerAddress:
					return true;	// Manufacturer's address synch is conditional
				default:
					return base.ShouldIgnoreInfoForDetection(info);
			}
		}

		protected override void SetupRegistriesForDetechEnabledForUniversalXml(ZString messageType)
		{
			base.SetupRegistriesForDetechEnabledForUniversalXml(messageType);
			USCustomsDataRegistry.Instance.DoDefaultCountriesOfOriginAndExport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		}

		protected override OrgHeader CreateOrg2ForDetection()
		{
			var org = base.CreateOrg2ForDetection();
			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FIRMSCode, "KD32", Core.Constants.CountryCodes.UnitedStates);
			return org;
		}

		protected override void SetupLocalPort1(RefUNLOCO port)
		{
			base.SetupLocalPort1(port);
			testHelper.AddUSScheduleCode(Schedule.D, port, "3333", USLocoMapSystemUsageList.Codes.Sea);
		}

		protected override void SetupLocalPort2(RefUNLOCO port)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4444", "4444 CUSOF", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();

			base.SetupLocalPort2(port);
			testHelper.AddUSScheduleCode(Schedule.D, port, "4444", USLocoMapSystemUsageList.Codes.Sea);
		}

		protected override void SetupForeignPort2(RefUNLOCO port)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "23232", "23232 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
			base.SetupForeignPort2(port);

			testHelper.AddUSScheduleCode(Schedule.K, port, "23232", USLocoMapSystemUsageList.Codes.All);
		}

		protected override void SetupLocalPort3(RefUNLOCO port)
		{
			base.SetupLocalPort3(port);
			testHelper.AddUSScheduleCode(Schedule.D, port, "5555", USLocoMapSystemUsageList.Codes.Sea);
		}

		void SetTransportMode(ZString transportMode)
		{
			consol.JK_TransportMode = transportMode;
			shipment.JS_TransportMode = transportMode;
			declarationUS.JE_TransportMode = transportMode;
		}

		JobDeclaration declarationUS;
		ForwardingConsol consol;
		CommonContainer container;
		ForwardingShipment shipment;
		PackLine packLine;
		DeclarationTestHelper testHelper;
		RefUNLOCO auzzz;
		RefUNLOCO uszzz;

		void SetupUSPortDetails()
		{
			auzzz = testHelper.CreateUnlocoIfNotExists("AUZZZ", testHelper.Australia);
			var map1 = auzzz.RefLocoMaps.AddNew();
			map1.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			map1.RY_LocalPortCode = "23232";
			map1.RY_IsSystem = true;

			uszzz = testHelper.CreateUnlocoIfNotExists("USZZZ", testHelper.UnitedStates);
			RefLocoMap map2 = uszzz.RefLocoMaps.AddNew();
			map2.RY_LocalPortCode = "3242";
			map2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			map2.RY_IsSystem = true;
			RefLocoMap map3 = uszzz.RefLocoMaps.AddNew();
			map3.RY_LocalPortCode = "7856";
			map3.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			map3.RY_IsSystem = true;
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new DeclarationTestHelper(Factory);
			declarationUS = Factory.New<JobDeclaration>();
			declarationUS.JE_MessageType = JobMessageTypeList.Codes.Import;
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			container = consol.Containers.AddNew();
			shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = testHelper.Consignee.PK;
			shipment.ConsignorPK = testHelper.Consignor.PK;
			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;
			packLine.JL_JC = container.PK;
			declarationUS.JE_JS = shipment.PK;
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = base.GetJobDeclaration();
			declaration.JE_OA_ManufacturerAddress = CreateOrganisation("MANUF", "MANUFACTURER", "MANUFACTURER ADDRESS").MainAddress.PK;
			return declaration;
		}
	}
}
