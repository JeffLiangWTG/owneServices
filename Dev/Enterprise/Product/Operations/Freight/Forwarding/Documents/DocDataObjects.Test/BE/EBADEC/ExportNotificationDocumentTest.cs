using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class ExportNotificationDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testOrgProxy = testObjectCreator.CreateOrgHeader("BECOMP", true, true, "BEANR");
			testOrgProxy.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Belgium;
			var testCompany = testObjectCreator.CreateNewCompany("BE", "BE", orgProxy: testOrgProxy);
			var testBranch = testObjectCreator.CreateBranch("ANR", "AntwerpBranche", testCompany, testOrgProxy);

			testBranch.OrgProxy.MainAddress.CustomsCodes.DeleteAll();
			testBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNxxx", Constants.CountryCodes.Australia);
			testBranch.OrgProxy.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORIxxx", Constants.CountryCodes.Belgium);
			Factory.Save();

			var exportNotificationBEMenuItemQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Notification (BE)");
			var exportNotificationBEMenuItem = Factory.Load<StmMenuItem>(exportNotificationBEMenuItemQuery);

			Assert("There's no menu with name 'Export Notification (BE)'", exportNotificationBEMenuItem.Length >= 1);
			AssertEquals("There's more than one menu with name 'Export Notification (BE)'", 1, exportNotificationBEMenuItem.Length);

			var exportNotificationBEMenuTemplatePivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, exportNotificationBEMenuItem.First().PK);
			var exportNotificationBEMenuTemplatePivot = Factory.Load<VisualizerMenuTemplatePivot>(exportNotificationBEMenuTemplatePivotQuery);

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.OuterPackLines.AddNew();

				AssertContents(consol, exportNotificationBEMenuTemplatePivot.Single(), CreatContentWithoutPackLines());
			}

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("CONT", Constants.TransportModes.Sea, false);

				AssertContents(consol, exportNotificationBEMenuTemplatePivot.Single(), CreatContentCONT());
			}

			using (testBranch.SetAsTemporaryContext())
			{
				var consol = CreateConsol("RORO", Constants.TransportModes.Sea, true);

				AssertContents(consol, exportNotificationBEMenuTemplatePivot.Single(), CreatContentRORO());
			}
		}

		string CreatContentWithoutPackLines() => $@"[2,3] There are no vehicle packlines or no packlines allocated to a container.
[6,42] Created By
";

		string CreatContentCONT() => $@"[2,3] Notification of Export Consignment
[5,4] Delivery Details
[6,4] Sender's ID
[6,16] Transport Mode
[6,24] Vessel Type
[6,32] Ferry Terminal
[6,40] Terminal
[7,4] DUN: DUNxxx
[7,16] Sea Freight
[7,24] BA
[7,32] ☐
[7,40] BEANR
[9,4] Container Number
[9,15] Booking Reference
[10,4] TBNU1111111
[10,15] B20201025
[11,15] Document Number
[11,27] Entry Type
[11,39] Customs Office Code
[12,15] MRN_CONT_111
[12,39] BE1010
[13,15] MRN222
[13,39] BE2020
[17,42] Created By
";

		string CreatContentRORO() => $@"[2,3] Notification of Export Consignment
[5,4] Delivery Details
[6,4] Sender's ID
[6,16] Transport Mode
[6,24] Vessel Type
[6,32] Ferry Terminal
[6,40] Terminal
[7,4] DUN: DUNxxx
[7,16] Sea Freight
[7,24] BA
[7,32] ☐
[7,40] BEANR1111122222
[9,4] VIN
[9,15] Booking Reference
[10,4] VIN11111117777777
[10,15] B20201025
[11,15] Document Number
[11,27] Entry Type
[11,39] Customs Office Code
[12,15] MRN111
[12,39] BE1010
[16,42] Created By
";

		#region Implementation

		ForwardingConsol CreateConsol(ZString type, ZString transportModeToTerminal, ZBool terminalCodeFromPSNFromCTO)
		{
			var consol = Factory.New<ForwardingConsol>();

			if (type == "CONT")
			{
				consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			}
			else
			{
				consol.JK_ConsolMode = Constants.ContainerModes.RollOnRollOff;
			}

			consol.JK_UniqueConsignRef = "C20201209";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "BEANR";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_BookingReference = "B20201025";

			CreatePackingLinesAndContainers(consol, type);
			CreateTransports(consol, transportModeToTerminal);
			CreateConsolAddresses(consol, terminalCodeFromPSNFromCTO);

			return consol;
		}

		void CreatePackingLinesAndContainers(ForwardingConsol consol, ZString type)
		{
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SH0001000";
			shipment1.JS_HouseBill = "H0000056";
			shipment1.JS_RL_NKOrigin = "BEANR";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.JS_F3_NKPackType = "PLT";
			shipment1.JS_MarksAndNumbers = "Marks";
			shipment1.JS_GoodsDescription = "Goods description";
			shipment1.CustomsEntryNumberType = "MRN";
			shipment1.CustomsEntryNumber = "MRN111";
			shipment1.OuterPackLines.RemoveAndDeleteAll();

			var packingLine1 = shipment1.OuterPackLines.AddNew();
			packingLine1.JL_PackageCount = 2;
			packingLine1.JL_F3_NKPackType = "PLT";
			packingLine1.JL_ActualWeight = 200;
			packingLine1.JL_ActualWeightUQ = "KG";
			packingLine1.JL_ActualVolume = 300;
			packingLine1.JL_ActualVolumeUQ = "M3";
			packingLine1.JL_DetailedDescription = "pack1";
			packingLine1.JL_ContainerPackingOrder = 1;

			if (type == "CONT")
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "TBNU1111111";

				packingLine1.SetContainer(consol, container);
				packingLine1.JL_ExportRefNumber = "MRN_CONT_111";

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "SH0002000";
				shipment2.JS_HouseBill = "H0000056";
				shipment2.JS_RL_NKOrigin = "BEANR";
				shipment2.JS_RL_NKDestination = "AUSYD";
				shipment2.JS_F3_NKPackType = "PLT";
				shipment2.JS_MarksAndNumbers = "Marks";
				shipment2.JS_GoodsDescription = "Goods description";
				shipment2.CustomsEntryNumberType = "MRN";
				shipment2.CustomsEntryNumber = "MRN222";
				shipment2.OuterPackLines.RemoveAndDeleteAll();

				var packingLine2 = shipment2.OuterPackLines.AddNew();
				packingLine2.JL_PackageCount = 2;
				packingLine2.JL_F3_NKPackType = "PLT";
				packingLine2.JL_ActualWeight = 200;
				packingLine2.JL_ActualWeightUQ = "KG";
				packingLine2.JL_ActualVolume = 300;
				packingLine2.JL_ActualVolumeUQ = "M3";
				packingLine2.JL_DetailedDescription = "pack2";
				packingLine2.JL_ContainerPackingOrder = 1;

				packingLine2.SetContainer(consol, container);

				var declaration2 = Factory.New<Enterprise.Integration.Customs.EU.IJobDeclaration>();
				declaration2.JE_JS = shipment2.PK;
				declaration2.CustomsOfficeCollection.RemoveAndDeleteAll();
				var officeOfExport2 = declaration2.CustomsOfficeCollection.AddNew();
				officeOfExport2.CY_Code = BelgianPortsConstants.EuOfficeCodesTypes.ActualExitOffice;
				officeOfExport2.CY_Data = "BE2020";

				shipment1.JS_PackingMode = Constants.ContainerModes.FCL;
				shipment2.JS_PackingMode = Constants.ContainerModes.FCL;
			}
			else
			{
				packingLine1.JL_VehicleColor = "RED";
				packingLine1.JL_VehicleMake = "Maserati ";
				packingLine1.JL_VehicleModel = "Quattroporte";
				packingLine1.JL_VehicleNumberOfDoors = 5;
				packingLine1.JL_VehicleYear = 2020;
				packingLine1.JL_RefNumber = "VIN11111117777777";

				shipment1.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			}

			var declaration1 = Factory.New<Enterprise.Integration.Customs.EU.IJobDeclaration>();
			declaration1.JE_JS = shipment1.PK;
			declaration1.CustomsOfficeCollection.RemoveAndDeleteAll();
			var officeOfExport1 = declaration1.CustomsOfficeCollection.AddNew();
			officeOfExport1.CY_Code = BelgianPortsConstants.EuOfficeCodesTypes.OfficeOfExit;
			officeOfExport1.CY_Data = "BE1010";
		}

		void CreateTransports(ForwardingConsol consol, ZString transportModeToTerminal)
		{
			var preTransport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			preTransport.JW_LegOrder = 1;
			preTransport.JW_TransportMode = transportModeToTerminal;
			preTransport.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			preTransport.JW_RL_NKLoadPort = "BEMAL";
			preTransport.JW_RL_NKDiscPort = "BEANR";
			preTransport.JW_ETD = new ZDateTime(2020, 10, 1);
			preTransport.JW_VoyageFlight = "CC789";

			if (transportModeToTerminal == Constants.TransportModes.Sea)
			{
				var preVessel = Factory.New<RefVessel>();
				preVessel.RV_Code = "Stoomboot van Zwarte Piet";

				preVessel.RV_VesselType = Constants.VesselType.Barge;
				preVessel.RV_LloydsNumber = "LYDS456";
				preVessel.RV_RadioCallSign = "Radio456";
				preTransport.JW_Vessel = preVessel.RV_Code;
			}

			var transport = consol.Transports.AddNew();
			transport.JW_LegOrder = 2;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "BEANR";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_Vessel = "MSC Antwerp";
			transport.JW_VoyageFlight = "V111";
			transport.JW_ETD = new ZDateTime(2020, 10, 15);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Stoomboot van Sinterklaas";
			vessel.RV_VesselType = Constants.VesselType.CargoVessel;
			vessel.RV_LloydsNumber = "LYDS123";
			vessel.RV_RadioCallSign = "Radio123";
			transport.JW_Vessel = vessel.RV_Code;
			transport.JW_VoyageFlight = "CC456";
		}

		void CreateConsolAddresses(ForwardingConsol consol, ZBool terminalCodeFromPSNFromCTO)
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "MAERSK";
			sendingForwarder.OH_RL_NKClosestPort = "DKAAL";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Aalborg";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			departureCTO.OH_FullName = "YUMMY";
			departureCTO.OH_RL_NKClosestPort = "BEANR";
			departureCTO.MainAddress.Address1 = "Unit 200";
			departureCTO.MainAddress.Address2 = "55 Why Lane";
			departureCTO.MainAddress.City = "Antwerp";
			departureCTO.MainAddress.Postcode = "2000";
			departureCTO.MainAddress.OA_RN_NKCountryCode = "BE";
			departureCTO.MainAddress.Header.OH_IsFerryWaterTerminal = false;

			if (terminalCodeFromPSNFromCTO)
			{
				departureCTO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PortSystemNumber, "BEANR1111122222", Constants.CountryCodes.Belgium);
			}

			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;
		}

		#endregion
	}
}
