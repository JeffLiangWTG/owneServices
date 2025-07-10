using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer.Testing
{
	sealed class HVLVACEAsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestStringValueCharacterCaseIsUpper()
		{
			var characterCasingProperty = typeof(HVLVACEAsycudaBillDataObjectReader).GetProperty("StringValueCharacterCase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty);
			var readerHelper = new ACEAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
			var header = Factory.BOFactory.New<AsycudaManifestHeader>();
			var reader = new HVLVACEAsycudaBillDataObjectReader(new Shipment(), logger, Factory, header, readerHelper, false);
			var characterCasingValue = characterCasingProperty.GetValue(reader);
			AssertEquals(CharacterCase.Upper, characterCasingValue);
		}

		public void TestImportingAsycudaManifestData_USAIRAMS()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var airLocalPort1 = help.GetAirLocalPort1("US");
			var airLocalPort2 = help.GetAirLocalPort2(airLocalPort1.PK);

			var portOfLoading = new UNLOCO() { Code = airLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO() { Code = airLocalPort2.RL_Code };
			var portOfFirstArrival = new UNLOCO() { Code = airLocalPort1.RL_Code };

			var arrivalTime = ZDateTime.Today.AddDays(1);
			var departureTime = ZDateTime.Today.AddDays(3);

			var carrierCode = "OTD1";
			var billIssuerCode = "BIC1";

			var headerDataObject = help.SetupManifestHeader("MAN00001", portOfLoading, portOfDischarge, arrivalTime, departureTime, "", "IAM");
			headerDataObject.MessagingApplicationCode = new CodeDescriptionPair { Code = ApplicationCodeTypeList.Codes.Consolidator };

			var headerEntryHeader = help.SetupCountryHeaderEntryHeader("US", 1);
			var headerEntryInstruction = help.SetupCountryHeaderEntryInstruction(1, portOfFirstArrival, carrierCode, "US", "IAM", "NT1");

			var bill = help.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var billCountryEntryHeader = help.SetupCountryBillEntryHeader("US", 1, "CLR", "Sender Reference1");

			var billCountryEntryInstruction = help.SetupCountryBillEntryInstruction(1, "Goods Location1", "Location Information1", "IMP", "US", billIssuerCode, 200.01, 300.01, "Q", "A", "TEST", ZDateTime.Empty, ZString.Empty);
			var container = help.SetupContainer("CONT00001", "ABC", 200m, "CC1", "STO", "F", "SPN", "SPT", 10);

			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.AddSafe(headerEntryHeader);

			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.AddSafe(headerEntryInstruction);

			bill.SetEntryHeaderCollection(() => new List<EntryHeader>());
			bill.EntryHeaderCollection.AddSafe(billCountryEntryHeader);

			bill.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			bill.EntryInstructionCollection.AddSafe(billCountryEntryInstruction);

			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.Add(bill);
			headerDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			headerDataObject.ContainerCollection.Add(container);

			var deConsolidatorOrganizationAddress = GetNewAddressData_INTHEMSYD(DocAddressType.CustomsContainerYardAddress);
			var deConsolidator = new OrganisationDataObjectReader(deConsolidatorOrganizationAddress, Logger, Factory).GetMatchedOrNewForTesting();
			deConsolidator.OA_Address1 = "1 ATLAS ROAD";
			deConsolidator.OA_PostCode = "1619";
			deConsolidator.OA_Address2 = "JOHANNESBURG INTERNATIONAL AIRPORT";
			deConsolidator.OA_City = "KEMPTON PARK";
			deConsolidator.CompanyName = "ZA DECONSOLIDATOR";

			var dischargeTerminalOrganizationAddress = GetNewAddressData_INTHEMSYD(DocAddressType.CustomsContainerTerminalOperatorAddress);
			var dischargeTerminal = new OrganisationDataObjectReader(dischargeTerminalOrganizationAddress, Logger, Factory).GetMatchedOrNewForTesting();
			dischargeTerminal.OA_Address1 = "1 ATLAS ROAD";
			dischargeTerminal.OA_PostCode = "1619";
			dischargeTerminal.OA_City = "KEMPTON PARK";
			dischargeTerminal.CompanyName = "ZA TERMINAL";
			headerDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			headerDataObject.OrganizationAddressCollection.AddSafe(deConsolidatorOrganizationAddress);
			headerDataObject.OrganizationAddressCollection.AddSafe(dischargeTerminalOrganizationAddress);
			Factory.SaveForTesting();
			var reader = new AsycudaManifestHeaderDataObjectReader(headerDataObject, Logger, Factory);
			var readerHeaderBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();
			var factory = new BusinessObjectFactory();
			var headerBO = factory.Load<AsycudaManifestHeader>(readerHeaderBO.PK);

			AssertNotNull(headerBO);

			#region Check Contents of header Business Object
			//header
			AssertEquals("headerBO.AMA_ApplicationCode", ApplicationCodeTypeList.Codes.Consolidator, headerBO.AMA_ApplicationCode);
			AssertEquals("headerBO.AMA_MasterBill", "MAN00001", headerBO.AMA_MasterBill);
			AssertEquals("headerBO.AMA_TransportMode", TransportTypeList.Codes.Air, headerBO.AMA_TransportMode);
			AssertEquals("headerBO.AMA_Voyage", "QTX370", headerBO.AMA_Voyage);
			AssertEquals("headerBO.AMA_RL_NKPortOfLoading", airLocalPort1.RL_Code, headerBO.AMA_RL_NKPortOfLoading);
			AssertEquals("headerBO.AMA_RL_NKPortOfDischarge", airLocalPort2.RL_Code, headerBO.AMA_RL_NKPortOfDischarge);
			AssertEquals("headerBO.AMA_VesselName", "TestVessel", headerBO.AMA_VesselName);
			AssertEquals("headerBO.AMA_RN_NKConveyanceNationality", "XX", headerBO.AMA_RN_NKConveyanceNationality);
			AssertEquals("headerBO.AMA_MasterInformation", "Captain Kirk", headerBO.AMA_MasterInformation);
			AssertEquals("headerBO.AMA_E_DEP", departureTime, headerBO.AMA_E_DEP);
			AssertEquals("headerBO.AMA_E_ARV", arrivalTime, headerBO.AMA_E_ARV);
			AssertEquals("headerBO..AMA_MasterBillIssueDate", ZDateTime.Today.AddDays(4), headerBO.AMA_MasterBillIssueDate);
			AssertEquals("headerBO.AMA_OverrideFreightDefaults", false, headerBO.AMA_OverrideFreightDefaults);
			AssertEquals("headerBO.AMA_ManifestType", "IAM", headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", ZDateTime.Today, headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", carrierCode, headerBO.AMA_CarrierCode);
			AssertEquals("headerBO.AMA_Trailer1RegNo", "TRAILER001", headerBO.AMA_Trailer1RegNo);
			AssertEquals("headerBO.AMA_Trailer2RegNo", "TRAILER002", headerBO.AMA_Trailer2RegNo);
			AssertEquals("headerBO.AMA_RN_NKTrailer1RegCountry", "TR", headerBO.AMA_RN_NKTrailer1RegCountry);
			AssertEquals("headerBO.AMA_RN_NKTrailer2RegCountry", "ZA", headerBO.AMA_RN_NKTrailer2RegCountry);
			AssertEquals("headerBO.AMA_AgentType", Core.Constants.AgentType.Agent, headerBO.AMA_AgentType);
			AssertEquals("headerBO.AMA_RadioCallSign", "9064384", headerBO.AMA_RadioCallSign);
			AssertEquals("headerBO.AMA_ContainerMode", Core.Constants.ContainerModes.Other, headerBO.AMA_ContainerMode);
			AssertEquals("headerBO.AMA_IsBuyersConsolidation", ZBool.True, headerBO.AMA_IsBuyersConsolidation);
			AssertEquals("DE-Consolidator Address", deConsolidator.PK, headerBO.AMA_OA_DeconsolidateAddress);
			AssertEquals("Discharge Terminal Address", dischargeTerminal.PK, headerBO.AMA_OA_DischargeTerminalAddress);

			//bill
			AssertEquals("headerBO has 1 bill", 1, headerBO.Bills.Count);
			var billBO = headerBO.Bills[0];
			AssertEquals("billBO.ABL_RL_NKOrigin", airLocalPort1.RL_Code, billBO.ABL_RL_NKOrigin);
			AssertEquals("billBO.ABL_RL_NKFinalDestination", airLocalPort2.RL_Code, billBO.ABL_RL_NKFinalDestination);
			AssertEquals("billBO.ABL_GrossWeight", 300m, billBO.ABL_GrossWeight);
			AssertEquals("billBO.ABL_GoodsDescription", "Goods Desc", billBO.ABL_GoodsDescription);
			AssertEquals("billBO.ABL_Volume", 3m, billBO.ABL_Volume);
			AssertEquals("billBO.ABL_CarrierReference", "Carrier Reference", billBO.ABL_CarrierReference);
			AssertEquals("billBO.ABL_BolType", "STD", billBO.ABL_BolType);
			AssertEquals("billBO.ABL_PrepaidCollect", "PRE", billBO.ABL_PrepaidCollect);
			AssertEquals("billBO.ABL_BillStatus", "", billBO.ABL_BillStatus);
			AssertEquals("billBO.ABL_SenderReference", "", billBO.ABL_SenderReference);
			AssertEquals("billBO.ABL_LocationInformation", "Location Information1", billBO.ABL_LocationInformation);
			AssertEquals("billBO.ABL_GoodsLocation", "GOODS LOCATION1", billBO.ABL_GoodsLocation);
			AssertEquals("billBO.ABL_ShipmentType", "IMP", billBO.ABL_ShipmentType);
			AssertEquals("billBO.ABL_BillIssuer", billIssuerCode, billBO.ABL_BillIssuer);

			//container
			AssertEquals("headerBO has 1 container", 1, headerBO.Containers.Count);
			var containerBO = headerBO.Containers[0];
			AssertEquals("containerBO.ACN_ContainerNumber", "CONT00001", containerBO.ACN_ContainerNumber);
			AssertEquals("containerBO.ACN_Seal1", "ABC", containerBO.ACN_Seal1);
			AssertEquals("containerBO.ACN_GoodsWeight", 200m, containerBO.ACN_GoodsWeight);
			AssertEquals("containerBO.ACN_CommodityCode", "CC1", containerBO.ACN_CommodityCode);
			AssertEquals("containerBO.ACN_StowageLocation", "STO", containerBO.ACN_StowageLocation);
			AssertEquals("containerBO.ACN_EmptyFullIndicator", "F", containerBO.ACN_EmptyFullIndicator);
			AssertEquals("containerBO.ACN_SealingPartyName", "SPN", containerBO.ACN_SealingPartyName);
			AssertEquals("containerBO.ACN_SealingPartyType", "SPT", containerBO.ACN_SealingPartyType);
			AssertEquals("containerBO.ACN_NumberOfPackages", 10, containerBO.ACN_NumberOfPackages);
			#endregion
		}
	}
}
