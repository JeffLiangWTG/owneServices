using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	sealed class WarehouseInBondDataEventContextReaderTest : TestCaseWithFactory
	{
		public void TestEventContextValues()
		{
			var loadPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU!23";
			loadPort.RL_IATA = "AU!";
			var loadPortMap = loadPort.RefLocoMaps.AddNew();
			loadPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			loadPortMap.RY_LocalPortCode = "12!AU";
			loadPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			var discPort = Factory.New<RefUNLOCO>();
			discPort.RL_Code = "US!23";
			discPort.RL_IATA = "US!";
			var discPortMap = discPort.RefLocoMaps.AddNew();
			discPortMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			discPortMap.RY_LocalPortCode = "1!US";
			discPortMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SDF32@#$";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			header.BH_CarrierSCAC = "OTE2";
			header.BH_ImportConveyanceName = "BOB'S VESSEL";
			header.BH_LloydsNumber = "LL32342";
			header.BH_VoyageNumber = "VO323";
			header.BH_ImportLoadPortKCode = "12!AU";
			header.BH_PortUnladingDCode = "1!US";
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.InBondNumber = "INB3242";
			Factory.Save();
			var manager = header.GetUniversalDataContextManager() as IEventDataContextManager;
			var eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MBOLOriginUNLOCO - AU!23
MBOLDestinationUNLOCO - US!23
VesselName - BOB'S VESSEL
LloydsNumber - LL32342
VoyageNumber - VO323
CarrierCode - OTE2
TransportMode - SEA
DeclarationReference - INB0000001".Trim(), eventContextValues);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			manager = header.GetUniversalDataContextManager() as IEventDataContextManager;
			eventContextValues = string.Join(System.Environment.NewLine, manager.EventContextValues.Select(o => o.Key + " - " + o.Value).ToArray());
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
MAWBOriginIATAAirportCode - AU!
MAWBDestinationIATAAirportCode - US!
MBOLOriginUNLOCO - AU!23
MBOLDestinationUNLOCO - US!23
VesselName - BOB'S VESSEL
LloydsNumber - LL32342
VoyageNumber - VO323
CarrierCode - OTE2
TransportMode - AIR
DeclarationReference - INB0000001".Trim(), eventContextValues);
		}
	}
}
