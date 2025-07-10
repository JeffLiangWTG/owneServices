using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobDeclarationSynchroniserTest : Customs.Business.Testing.JobDeclarationSynchroniserTest
	{
		public void TestSynchroniseMasterBillIssueAtAndIssueDate_SeaImport()
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillIssueDate = new ZDateTime(2016, 11, 8);
			consol.JK_RL_NKMasterBillIssuePlace = "ZAJNB";
			shipment.ConsignorPK = orgAU.PK;
			shipment.ConsigneePK = orgZA.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertSyncrhonisedDeclarationData(declaration, false, false, true, "Sea Import - expect issued date and place synced");
			Factory.Save();
			var declarationReloaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertSyncrhonisedDeclarationData(declarationReloaded, false, false, true, "Sea Import - expect issued date and place synced after save and reload");
			declarationReloaded.JE_OverrideFreightDefaults = true;
			AssertSyncrhonisedDeclarationData(declarationReloaded, false, true, true, "Sea Import - expect issued date and place not cleared after override");
		}

		public void TestSynchroniseMasterBillIssueAtAndIssueDate_SeaExport()
		{
			consol.JK_RL_NKLoadPort = portZAAAM;
			consol.JK_RL_NKDischargePort = portAUSYD;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillIssueDate = new ZDateTime(2016, 11, 8);
			consol.JK_RL_NKMasterBillIssuePlace = "ZAJNB";
			shipment.ConsignorPK = orgZA.PK;
			shipment.ConsigneePK = orgAU.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertSyncrhonisedDeclarationData(declaration, false, false, false, "Sea Export - expect issued date and place synced");
			Factory.Save();
			var declarationReloaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertSyncrhonisedDeclarationData(declarationReloaded, false, false, false, "Sea Export - expect issued date and place synced after save and reload");
			declarationReloaded.JE_OverrideFreightDefaults = true;
			AssertSyncrhonisedDeclarationData(declarationReloaded, false, true, false, "Sea Export - expect issued date and place not cleared after override");
		}

		public void TestSynchroniseMasterBillIssueAtAndIssueDate()
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillIssueDate = new ZDateTime(2016, 11, 8);
			consol.JK_RL_NKMasterBillIssuePlace = "ZAJNB";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("JE_MasterBillIssuedDate", new ZDateTime(2016, 11, 8), declaration.JE_MasterBillIssuedDate);
			AssertEquals("JE_RL_NKMasterBillIssuedAt", "ZAJNB", declaration.JE_RL_NKMasterBillIssuedAt);
			AssertEquals(false, declaration.JE_MasterBillIssuedDateInfo.ReadOnly);
			AssertEquals(false, declaration.JE_RL_NKMasterBillIssuedAtInfo.ReadOnly);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillIssueDate = new ZDateTime(2016, 11, 9);
			consol.JK_RL_NKMasterBillIssuePlace = "ZADBN";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("JE_MasterBillIssuedDate", new ZDateTime(2016, 11, 8), declaration.JE_MasterBillIssuedDate);
			AssertEquals("JE_RL_NKMasterBillIssuedAt", "ZAJNB", declaration.JE_RL_NKMasterBillIssuedAt);
			AssertEquals(false, declaration.JE_MasterBillIssuedDateInfo.ReadOnly);
			AssertEquals(false, declaration.JE_RL_NKMasterBillIssuedAtInfo.ReadOnly);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_MasterBillIssueDate = new ZDateTime(2016, 11, 8);
			consol.JK_RL_NKMasterBillIssuePlace = "ZAJNB";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("JE_MasterBillIssuedDate", new ZDateTime(2016, 11, 8), declaration.JE_MasterBillIssuedDate);
			AssertEquals("JE_RL_NKMasterBillIssuedAt", "ZAJNB", declaration.JE_RL_NKMasterBillIssuedAt);
			AssertEquals(false, declaration.JE_MasterBillIssuedDateInfo.ReadOnly);
			AssertEquals(false, declaration.JE_RL_NKMasterBillIssuedAtInfo.ReadOnly);
		}

		public void TestSynchroniseHouseBillIssueDate()
		{
			shipment.JS_HouseBill = "";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2016, 11, 8);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("HouseBillIssuedDate", ZDateTime.Empty, declaration.HouseBillIssuedDate);
			shipment.JS_HouseBill = "HB111";
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("HouseBillIssuedDate", new ZDateTime(2016, 11, 8), declaration.HouseBillIssuedDate);
			shipment.JS_HouseBillIssueDate = new ZDateTime(2016, 11, 9);
			declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("HouseBillIssuedDate", new ZDateTime(2016, 11, 9), declaration.HouseBillIssuedDate);
			shipment.JS_HouseBill = "";
			AssertEquals("HouseBillIssuedDate", ZDateTime.Empty, declaration.HouseBillIssuedDate);
		}

		public override void TestJobDeclarationSynchroniser()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_RS_NKServiceLevel = "XYZ";
			AssertEquals("Declaration.JE_RS_NKServiceLevel", "XYZ", base.declaration.JE_RS_NKServiceLevel);
			Shipment.JS_TransportMode = "";
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("base.declaration.JE_TransportMode", Core.Constants.TransportModes.Sea, base.declaration.JE_TransportMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("base.declaration.JE_ContainerMode", Core.Constants.ContainerModes.Containerised, base.declaration.JE_ContainerMode);
			var testConsignor = GetOverseasConsignor();
			Shipment.ConsignorPK = testConsignor.PK;
			AssertEquals("base.declaration.JE_OH_Supplier", testConsignor.PK, base.declaration.JE_OH_Supplier);
			var testConsignee = GetLocalConsignee();
			Shipment.ConsigneePK = testConsignee.PK;
			AssertEquals("base.declaration.JE_OH_Importer", testConsignee.PK, base.declaration.JE_OH_Importer);
			Transport.JW_ATA = ZDateTime.Today.AddDays(3);
			AssertEquals("base.declaration.JE_DateOfArrival", Consol.JK_JX_JB_A_ARV, base.declaration.JE_DateOfArrival);
			Transport.JW_ATD = ZDateTime.Today.AddDays(1);
			AssertEquals("base.declaration.JE_ExportDate", Consol.JK_JX_JA_A_DEP, base.declaration.JE_ExportDate);
			Transport.JW_Vessel = "12345";
			AssertEquals("base.declaration.JE_VesselName", Consol.JK_JX_JV_NKVessel, base.declaration.JE_VesselName);
			Shipment.JS_HouseBill = TestHouseBillNumber;
			AssertEquals("base.declaration.JE_HouseBill", TestHouseBillNumber, base.declaration.JE_HouseBill);
			Shipment.JS_ActualWeight = 3210.0m;
			AssertEquals("base.declaration.JE_TotalWeight", 3210.0m, base.declaration.JE_TotalWeight);
			Shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertEquals("base.declaration.JE_TotalWeightUnit", Core.Constants.Weight.Pounds, base.declaration.JE_TotalWeightUnit);
			Shipment.JS_ActualVolume = 13.2m;
			AssertEquals("base.declaration.JE_TotalVolume", 13.2m, base.declaration.JE_TotalVolume);
			Shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicFeet;
			AssertEquals("base.declaration.JE_TotalVolumeUnit", Core.Constants.Volume.CubicFeet, base.declaration.JE_TotalVolumeUnit);
			Shipment.JS_OuterPacks = 12;
			AssertEquals("base.declaration.JE_TotalNoOfPacks", 12, base.declaration.JE_TotalNoOfPacks);
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Box;
			Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			AssertEquals("base.declaration.JE_TotalNoOfPacksPackType", Core.Constants.PkgUnit.Pallet, base.declaration.JE_TotalNoOfPacksPackType);
			Shipment.JS_GoodsDescription = TestGoodsDescription;
			AssertEquals("base.declaration.JE_GoodsDescription", TestGoodsDescription, base.declaration.JE_GoodsDescription);
			Shipment.JS_E_ARV = ZDateTime.Today.AddDays(1);
			AssertEquals("base.declaration.JE_DateOfFirstArrival", Shipment.JS_E_ARV, base.declaration.JE_DateAtFinalDestination);
			Shipment.JS_E_DEP = ZDateTime.Today.AddDays(4);
			AssertEquals("base.declaration.JE_DateAtOrigin", Shipment.JS_E_DEP, base.declaration.JE_DateAtOrigin);
		}

		public override void TestSeaTransportModeFormatting()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			base.declaration.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Bulk, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Liquid, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.BreakBulk, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.BreakBulk, base.declaration.JE_ContainerMode);
		}

		public override void TestSeaTransportModeFormattingForImport()
		{
			decSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));
			Shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			base.declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Bulk, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Liquid, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.BreakBulk, base.declaration.JE_ContainerMode);
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.BreakBulk, base.declaration.JE_ContainerMode);
		}

		new JobDeclaration declaration;
		ForwardingConsol consol;
		ForwardingShipment shipment;

		protected override void AssertDetectEnabledForUniversalXml(ZString messageType, ZString origin, ZString destination, ZString loadPort, ZString dischargePort, ZString firstArrival, ZBool overrideFreightDefaults)
		{
			Factory.RefreshEnabled = false;
			// This test is broken, it only previously passed as RefreshEnabled = false
			// The reason is because UniversalShipmentMessageProcessor.ImportShipment.usedIncomingShipmentData = false when all the other implementations of this test class evaluate to true
			// Since usedIncomingShipmentData = false the uxml data import is not saved and the code that sets JE_OverrideFreightDefaults = false is never executed.
			// The last assertion asserts JE_OverrideFreightDefaults = false, however it's actually true
			base.AssertDetectEnabledForUniversalXml(messageType, origin, destination, loadPort, dischargePort, firstArrival, overrideFreightDefaults);
		}

		protected override void SetupForDetection(ForwardingShipment shipment, ZString origin, ZString destination)
		{
			base.SetupForDetection(shipment, origin, destination);
			shipment.JS_HouseBill = "HB111";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2016, 11, 8);
		}

		protected override void SetupForDetection(ForwardingConsol consol, ZString loadPort, ZString dischargePort, ZString firstArrival)
		{
			base.SetupForDetection(consol, loadPort, dischargePort, firstArrival);
			consol.JK_MasterBillIssueDate = new ZDateTime(2016, 11, 8);
			consol.JK_RL_NKMasterBillIssuePlace = "ZAJNB";
		}

		protected override bool ShouldIgnoreInfoForDetection(ZPropertyInfo info)
		{
			var declaration = info.BizObj as JobDeclaration;
			switch (info.Name)
			{
				case JobDeclaration.Schema.JE_MasterBillIssuedDate:
					return declaration == null || !(declaration.IsAir || declaration.IsSea);
				case JobDeclaration.Schema.JE_RL_NKMasterBillIssuedAt:
					return declaration == null || !declaration.IsSea;
				case JobDeclaration.Schema.HouseBillIssuedDate:
					return true;
				default:
					return base.ShouldIgnoreInfoForDetection(info);
			}
		}

		protected override void ClearDataForSynchronisationDetection(BaseJobDeclaration declaration)
		{
			base.ClearDataForSynchronisationDetection(declaration);
			var zaDeclaration = (JobDeclaration)declaration;
			zaDeclaration.JE_MasterBillIssuedDate = ZDateTime.Empty;
			zaDeclaration.JE_RL_NKMasterBillIssuedAt = ZString.Empty;
			zaDeclaration.HouseBillIssuedDate = ZDateTime.Empty;
		}

		protected override void SetUp()
		{
			base.SetUp();
			portZAAAM = CreateUnlocoIfNotExists("ZAAAM", Core.Constants.CountryCodes.SouthAfrica).Code;
			portAUSYD = CreateUnlocoIfNotExists("AUSYD", Core.Constants.CountryCodes.Australia).Code;
			orgZA = OrgHeader.New(Factory);
			orgZA.FillWithValidTestData();
			orgZA.OH_RL_NKClosestPort = "ZAAAM";
			orgAU = OrgHeader.New(Factory);
			orgAU.FillWithValidTestData();
			orgAU.OH_RL_NKClosestPort = "AUSYD";
			declaration = Factory.New<JobDeclaration>();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = portAUSYD;
			consol.JK_RL_NKDischargePort = portZAAAM;
			shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = orgZA.PK;
			shipment.ConsignorPK = orgAU.PK;
			declaration.JE_JS = shipment.PK;
		}

		string portZAAAM;
		string portAUSYD;
		OrgHeader orgZA;
		OrgHeader orgAU;

		void AssertSyncrhonisedDeclarationData(JobDeclaration dec, bool readonlyValue, bool overrideFreightDefaults, bool isImportJob, string assertionDescription)
		{
			CombineAssertions(assertionDescription, () =>
			{
				AssertEquals("IsPluggedIntoShipment", true, dec.IsPluggedIntoShipment);
				AssertEquals("IsImport", isImportJob, dec.IsImport);
				AssertEquals("JE_OverrideFreightDefaults", overrideFreightDefaults, dec.JE_OverrideFreightDefaults);
				AssertEquals("JE_MasterBillIssuedDate", new ZDateTime(2016, 11, 8), dec.JE_MasterBillIssuedDate);
				AssertEquals("JE_RL_NKMasterBillIssuedAt", "ZAJNB", dec.JE_RL_NKMasterBillIssuedAt);
				AssertEquals(readonlyValue, dec.JE_MasterBillIssuedDateInfo.ReadOnly);
				AssertEquals(readonlyValue, dec.JE_RL_NKMasterBillIssuedAtInfo.ReadOnly);
			});
		}

		RefUNLOCO CreateUnlocoIfNotExists(ZString code, ZString countryCode)
		{
			RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (unloco == null)
			{
				unloco = Factory.New<RefUNLOCO>();
				unloco.RL_Code = code;
				unloco.RL_RN_NKCountryCode = countryCode;
			}

			return unloco;
		}
	}
}
