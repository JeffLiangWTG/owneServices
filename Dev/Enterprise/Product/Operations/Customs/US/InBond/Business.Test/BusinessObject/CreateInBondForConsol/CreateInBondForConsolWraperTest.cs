using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CreateInBondForConsolWraper))]
	sealed class CreateInBondForConsolWraperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConsolNumber()
		{
			AssertEquals(wrapper.ConsolNumber, "C00000001");
		}

		public void TestShipmentsWithoutInBond()
		{
			AssertEquals(wrapper.ShipmentsWithoutInBond.Count, 1);
		}

		public void TestNewMovementHeaders()
		{
			AssertEquals(wrapper.NewMovementHeaders.Count, 1);
		}

		public void TestInitialize()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ABCD";
			carrier.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.AirNonContainer;
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "081";
			airline.RM_AirlineName1 = "TESTING AIRLINE";
			airline.RM_TwoCharacterCode = "TA";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TSTCONSIGNEE";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = "US";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "ABC", Core.Constants.CountryCodes.UnitedStates);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_UnpackDepotAddress = org2.MainAddress.PK;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.ConsignorPK = org1.PK;
			shipment1.JS_ActualWeight = 12m;
			shipment1.JS_GoodsValue = 23m;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment1.PK;
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = "BAG";
			var shipment2 = consol.Shipments.AddNew();
			shipment2.ConsignorPK = org1.PK;
			shipment2.JS_ActualWeight = 13m;
			shipment2.JS_GoodsValue = 24m;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "ABCD532342332";
			consol.JK_RL_NKLoadPort = "ABD12";
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_RL_NKDiscPort = "US123";
			transport.JW_VoyageFlight = "TA123";
			transport.JW_Vessel = "ABC";
			var firstLeg = consol.Transports[0];
			firstLeg.JW_ETD = ZDateTime.Today.AddDays(-1);
			transport.JW_ETA = ZDateTime.Today;

			var testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
			testUSLoco.RL_Code = "ABD12";
			testUSLoco.RL_PortName = "TEST Port - ABD12";
			testUSLoco.RL_IsSystem = true;
			testUSLoco.RL_HasAirport = true;
			testUSLoco.RL_HasSeaport = true;
			testUSLoco.RL_RN_NKCountryCode = "US";
			var testUSLoco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			testUSLoco1.RL_Code = "US123";
			testUSLoco1.RL_PortName = "TEST Port - ABD12";
			testUSLoco1.RL_IsSystem = true;
			testUSLoco1.RL_HasAirport = true;
			testUSLoco1.RL_HasSeaport = true;
			testUSLoco1.RL_RN_NKCountryCode = "US";
			var locoMap = Factory.NewWithValidTestData<RefLocoMap>();
			locoMap.RY_LocalPortCode = "0001";
			locoMap.RY_RL_NKLocoPort = "ABD12";
			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_IsSystem = false;
			var locoMap1 = Factory.NewWithValidTestData<RefLocoMap>();
			locoMap1.RY_LocalPortCode = "0002";
			locoMap1.RY_RL_NKLocoPort = "US123";
			locoMap1.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			locoMap1.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap1.RY_IsSystem = false;

			var inBond = Factory.New<CusInBondHeader>();
			var wrapper = new CreateInBondForConsolWraper(consol);
			var collection1 = wrapper.ShipmentsWithoutInBond;
			var header = wrapper.NewMovementHeaders[0];
			header.AllocatedShipmentsForMovement.AddRange(collection1);
			wrapper.Initialize(inBond);

			AssertEquals(shipment1.ConsigneeDocumentaryAddress.E2_OA_Address, inBond.BH_OA_Importer);
			AssertEquals(org1.PK, inBond.BH_OH_Supplier);
			AssertEquals(US.Messaging.Business.TransportModeCodes.Codes.AirNonContainer, inBond.BH_ImportTransportMode);
			AssertEquals(airline.RM_TwoCharacterCode, inBond.BH_CarrierSCAC);
			AssertEquals(transport.JW_Vessel, inBond.BH_ImportConveyanceName);
			AssertEquals("0001", inBond.BH_ImportLoadPortKCode);
			AssertEquals(firstLeg.JW_ETD, inBond.BH_SailingDate);
			AssertEquals(transport.JW_ETA, inBond.BH_ETA);
			AssertEquals("0002", inBond.BH_PortUnladingDCode);
			AssertEquals("ABC", inBond.BH_FIRMS);

			var cbp7512Line1 = inBond.MovementHeaders[0].MovementDetails[0].CBP7512Lines[0];
			AssertEquals("12 BAG", cbp7512Line1.BI_Description);
			AssertEquals("N/M", cbp7512Line1.BI_MarksAndNumbers);
			AssertEquals(12m, cbp7512Line1.BI_Weight);
			AssertEquals(47m, cbp7512Line1.BI_MonetaryValue);

			var cbp7512Line2 = inBond.MovementHeaders[0].MovementDetails[1].CBP7512Lines[0];
			AssertEquals("0", cbp7512Line2.BI_Description);
			AssertEquals("N/M", cbp7512Line2.BI_MarksAndNumbers);
			AssertEquals(13m, cbp7512Line2.BI_Weight);
			AssertEquals(47m, cbp7512Line2.BI_MonetaryValue);

			inBond = Factory.New<CusInBondHeader>();
			shipment1.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.Canada;
			wrapper = new CreateInBondForConsolWraper(consol);
			collection1 = wrapper.ShipmentsWithoutInBond;
			header = wrapper.NewMovementHeaders[0];
			header.AllocatedShipmentsForMovement.AddRange(collection1);
			wrapper.Initialize(inBond);

			cbp7512Line1 = inBond.MovementHeaders[0].MovementDetails[0].CBP7512Lines[0];
			AssertEquals(0m, cbp7512Line1.BI_MonetaryValue);
			cbp7512Line2 = inBond.MovementHeaders[0].MovementDetails[1].CBP7512Lines[0];
			AssertEquals(0m, cbp7512Line2.BI_MonetaryValue);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return wrapper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00000001";
			consol.Shipments.AddNew();
			wrapper = new CreateInBondForConsolWraper(consol);
		}
		CreateInBondForConsolWraper wrapper;
		ForwardingConsol consol;
	}
}
