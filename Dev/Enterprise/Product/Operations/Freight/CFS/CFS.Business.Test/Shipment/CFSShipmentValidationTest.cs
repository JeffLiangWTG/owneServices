using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentValidationTest : BaseFreightTest
	{
		#region TestCheckNoOriginalAndCopyBillsCount

		public void TestCheckNoOriginalAndCopyBillsCount()
		{
			var shipment = (CFSShipment)GetShipment();
			shipment.JS_NoOriginalBills = 0;
			shipment.JS_NoCopyBills = 0;

			Assert("CFS Shipment form does not have NoOriginalBills, should remove the count validation.", !Shipment.JS_NoOriginalBillsInfo.HasErrors());
			Assert("CFS Shipment form does not have NoCopyBills, should remove the count validation.", !Shipment.JS_NoCopyBillsInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_RL_NKDestination

		public void TestValidateJS_RL_NKDestination()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			shipment.JS_RL_NKDestination = ZString.Empty;
			shipment.JS_RL_NKOrigin = ZString.Empty;
			Assert(shipment.JS_RL_NKDestinationInfo.HasErrors());

			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RL_NKOrigin = "HKHKG";
			Assert(!shipment.JS_RL_NKDestinationInfo.HasErrors());

			shipment.JS_RL_NKDestination = "OOOOO";
			Assert(shipment.JS_RL_NKDestinationInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_RL_NKOrigin

		public void TestValidateJS_RL_NKOrigin()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			shipment.JS_RL_NKDestination = ZString.Empty;
			shipment.JS_RL_NKOrigin = ZString.Empty;
			Assert(shipment.JS_RL_NKOriginInfo.HasErrors());

			shipment.JS_RL_NKOrigin = "AUSYD";
			Assert(!shipment.JS_RL_NKOriginInfo.HasErrors());

			shipment.JS_RL_NKOrigin = "OOOOO";
			Assert(shipment.JS_RL_NKOriginInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_A_RCV

		public void TestValidateJS_A_RCV()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			AssertEquals("Consols.Count", 0, shipment.Consols.Count);
			AssertEquals("JS_A_RCVInfo.HasNotifications", false, shipment.JS_A_RCVInfo.HasNotifications());

			CFSLoadListConsol importLoadList = Factory.New<CFSLoadListConsol>();
			importLoadList.JK_TransportMode = Constants.TransportModes.Sea;

			Transport importTransport = importLoadList.Transports[0];
			importTransport.JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;

			importLoadList.JK_RL_NKLoadPort = importTransport.JW_RL_NKLoadPort;
			importLoadList.JK_RL_NKDischargePort = importTransport.JW_RL_NKDiscPort;

			CFSLoadListConsol exportLoadList = Factory.New<CFSLoadListConsol>();
			exportLoadList.JK_TransportMode = Constants.TransportModes.Sea;

			Transport exportTransport = exportLoadList.Transports[0];
			exportTransport.JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(false).PK;

			exportLoadList.JK_RL_NKLoadPort = exportTransport.JW_RL_NKLoadPort;
			exportLoadList.JK_RL_NKDischargePort = exportTransport.JW_RL_NKDiscPort;

			shipment.Consols.Add(importLoadList);
			shipment.Consols.Add(exportLoadList);
			AssertEquals("Consols.Count", 2, shipment.Consols.Count);
			AssertEquals("JS_A_RCVInfo.HasNotifications", false, shipment.JS_A_RCVInfo.HasNotifications());

			shipment.JS_A_RCV = importLoadList.Schedule.JX_JA_E_DEP.AddDays(1);
			AssertEquals("JS_A_RCVInfo.HasNotifications", false, shipment.JS_A_RCVInfo.HasNotifications());

			shipment.JS_A_RCV = exportLoadList.Schedule.JX_JA_E_DEP.AddDays(1);
			AssertEquals("JS_A_RCVInfo.HasWarnings()", true, shipment.JS_A_RCVInfo.HasWarnings());
		}

		#endregion

		#region ValidateJS_TranshipToOtherCFS

		public void TestValidateJS_TranshipToOtherCFS()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			CFSLoadListConsol importConsol = shipment.Consols.AddNew();
			importConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			importConsol.Transports[0].JW_JX = ImportSailing1.PK;

			CFSLoadListConsol exportConsol = shipment.Consols.AddNew();
			exportConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			exportConsol.Transports[0].JW_JX = ExportSailing1.PK;

			shipment.JS_TranshipToOtherCFS = false;
			shipment.Validation.ValidateJS_TranshipToOtherCFS();
			AssertNoErrors("JS_TranshipToOtherCFS should not have any errors:", shipment.JS_TranshipToOtherCFSInfo);

			shipment.JS_TranshipToOtherCFS = true;
			shipment.Validation.ValidateJS_TranshipToOtherCFS();
			AssertEquals("JS_TranshipToOtherCFS should have an error", true, shipment.JS_TranshipToOtherCFSInfo.HasErrors());
		}

		#endregion

		#region IsValidationRequiredForSwappedOriginAndDestination

		public void TestIsValidationRequiredForSwappedOriginAndDestination()
		{
			CFSShipment shipment = (CFSShipment)GetShipment();
			CFSLoadListConsol loadList = shipment.Consols.AddNew();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = ExportSailing1.PK;
			shipment.JS_RL_NKOrigin = loadList.Schedule.JX_JB_RL_NKPortOfDischarge;
			shipment.JS_RL_NKDestination = loadList.Schedule.JX_JA_RL_NKPortOfLoading;
			AssertEquals("JS_RL_NKOriginInfo.HasWarnings()", false, shipment.JS_RL_NKOriginInfo.HasWarnings());
			AssertEquals("JS_RL_NKDestinationInfo.HasWarnings()", false, shipment.JS_RL_NKDestinationInfo.HasWarnings());
		}

		#endregion

		#region TestValidateJS_PackingMode

		public void TestValidateJS_PackingMode()
		{
			Shipment.RunPreSaveValidation();
			AssertEquals("should not be any error on a new shipment with nothing done to it", false, Shipment.JS_PackingModeInfo.HasNotifications());
		}

		#endregion

		#region TestHasNotifications

		public void TestHasNotifications()
		{
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			Shipment.JS_OH_HandledOnBehalfOfForwarder = Factory.New<OrgHeader>().PK;
			Shipment.ConsigneePK = consignee.PK;
			Shipment.ConsignorPK = consignor.PK;
			Shipment.JS_RL_NKDestination = "SGSIN";
			Shipment.JS_RL_NKOrigin = "AUBNE";
			Shipment.JS_GoodsDescription = "foobah";
			Shipment.JS_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;

			Shipment.RunPreSaveValidation();
			AssertEquals("should not be any errors", false, Shipment.HasErrors);

			Shipment.JS_ActualVolume = 1;
			Shipment.JS_ActualWeight = 2;
			Shipment.RunPreSaveValidation();
			AssertEquals("should not be any notifications", false, Shipment.HasNotifications());
		}

		#endregion

		#region JS_OH_HandledOnBehalfOfForwarder

		public void TestValidateJS_OH_HandledOnBehalfOfForwarder()
		{
			Shipment.JS_OH_HandledOnBehalfOfForwarder = ZGuid.Empty;
			Shipment.Validation.ValidateJS_OH_HandledOnBehalfOfForwarder();
			Assert(Shipment.JS_OH_HandledOnBehalfOfForwarderInfo.HasWarnings());

			Shipment.JS_OH_HandledOnBehalfOfForwarder = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			Shipment.JS_OH_HandledOnBehalfOfForwarder = ZGuid.Empty;
			Shipment.Validation.ValidateJS_OH_HandledOnBehalfOfForwarder();
			AssertHasError(Shipment.JS_OH_HandledOnBehalfOfForwarderInfo, "Please enter the Client.");

			Shipment.JS_OH_HandledOnBehalfOfForwarder = ZGuid.NewZGuid();
			Shipment.Validation.ValidateJS_OH_HandledOnBehalfOfForwarder();
			Assert(!Shipment.JS_OH_HandledOnBehalfOfForwarderInfo.HasWarnings());
			AssertNoError(Shipment.JS_OH_HandledOnBehalfOfForwarderInfo, "Please enter the Client.");
		}

		#endregion

		#region IsValidationRequiredForEqualImportAndExportBrokers

		public void TestIsValidationRequiredForEqualImportAndExportBrokers()
		{
			Shipment.JS_OH_ImportBroker = ZGuid.NewZGuid();
			Shipment.JS_OH_ExportBroker = Shipment.JS_OH_ImportBroker;
			Shipment.Validation.ValidateJS_OH_ImportBroker();
			Shipment.Validation.ValidateJS_OH_ExportBroker();
			AssertEquals("JS_OH_ImportBrokerInfo.HasErrors()", false, Shipment.JS_OH_ImportBrokerInfo.HasErrors());
			AssertEquals("JS_OH_ExportBrokerInfo.HasErrors()", false, Shipment.JS_OH_ExportBrokerInfo.HasErrors());
		}

		#endregion

		#region TestValidateJS_ActualVolume

		public void TestValidateJS_ActualVolume()
		{
			AssertEquals("JS_Actual Volume should have no errors", false, Shipment.JS_ActualVolumeInfo.HasErrors());
			PackLine pack = Shipment.OuterPackLines.AddNew();
			Shipment.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.FCL;
			Shipment.JS_OuterPacks = 2;
			pack.JL_Height = new ZDecimal(1.023);
			pack.JL_Width = new ZDecimal(1.432);
			pack.JL_Length = new ZDecimal(1.241);
			pack.JL_UnitOfDimension = Constants.Length.Metres;
			pack.JL_PackageCount = 2;
			pack.JL_ActualVolume = pack.CalculatedVolume;
			Shipment.JS_ActualVolume = Enterprise.ZArchitecture.Core.Utilities.Round(pack.JL_Height * pack.JL_Width * pack.JL_Length * pack.JL_PackageCount, JobPackLinesSchema.JL_ActualVolume.Scale);
			Shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			Shipment.Validation.ValidateJS_ActualVolume();

			AssertEquals("JS_Actual Volume should have no errors", false, Shipment.JS_ActualVolumeInfo.HasWarnings());
			AssertEquals("JS_UnitOfVolume should have no errors", false, Shipment.JS_UnitOfVolumeInfo.HasWarnings());
		}

		#endregion

		public void TestOriginAndDestinationValidation()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKOrigin = "";
			shipment.Validation.ValidateJS_RL_NKOrigin();
			AssertEquals("Origin should not be in error if Destination is Set", false, shipment.JS_RL_NKOriginInfo.HasErrors());
		}

		public void TestWarningShowsWhenOuterPacksChangedOnPackedShipment()
		{
			CFSPackLine pack = Shipment.OuterPackLines.AddNew();
			CFSContainer container = pack.Containers.AddNew();
			Shipment.JS_OuterPacks = 5;
			Factory.Save();

			Shipment.JS_OuterPacks = 10;
			Shipment.Validation.ValidateJS_OuterPacks();
			AssertHasWarnings("Field should show a warning about packed shipment quantities changing", Shipment.JS_OuterPacksInfo);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSShipment loadedShipment = newFactory.Load<CFSShipment>(Shipment.PK);
			Shipment.Validation.ValidateJS_OuterPacks();
			AssertNoWarnings("Warning should only appear for a single instance of the shipment", loadedShipment.JS_OuterPacksInfo);
		}

		public void TestWarningShowsWhenOuterPacksPackTypeChangedOnPackedShipment()
		{
			CFSPackLine pack = Shipment.OuterPackLines.AddNew();
			CFSContainer container = pack.Containers.AddNew();
			Shipment.JS_F3_NKPackType = Constants.PkgUnit.Bundle;
			Factory.Save();

			Shipment.JS_F3_NKPackType = Constants.PkgUnit.Crate;
			Shipment.Validation.ValidateJS_F3_NKPackType();
			AssertHasWarnings("Field should show a warning about packed shipment quantities changing", Shipment.JS_F3_NKPackTypeInfo);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSShipment loadedShipment = newFactory.Load<CFSShipment>(Shipment.PK);
			Shipment.Validation.ValidateJS_F3_NKPackType();
			AssertNoWarnings("Warning should only appear for a single instance of the shipment", loadedShipment.JS_F3_NKPackTypeInfo);
		}

		public void TestWarningShowsWhenWeightChangedOnPackedShipment()
		{
			CFSPackLine pack = Shipment.OuterPackLines.AddNew();
			CFSContainer container = pack.Containers.AddNew();
			Shipment.JS_ActualWeight = 20;
			Factory.Save();

			Shipment.JS_ActualWeight = 15;
			Shipment.Validation.ValidateJS_ActualWeight();
			AssertHasWarnings("Field should show a warning about packed shipment quantities changing", Shipment.JS_ActualWeightInfo);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSShipment loadedShipment = newFactory.Load<CFSShipment>(Shipment.PK);
			Shipment.Validation.ValidateJS_ActualWeight();
			AssertNoWarnings("Warning should only appear for a single instance of the shipment", loadedShipment.JS_ActualWeightInfo);
		}

		public void TestWarningShowsWhenWeightUnitChangedOnPackedShipment()
		{
			CFSPackLine pack = Shipment.OuterPackLines.AddNew();
			CFSContainer container = pack.Containers.AddNew();
			Shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;
			Factory.Save();

			Shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;
			Shipment.Validation.ValidateJS_UnitOfWeight();
			AssertHasWarnings("Field should show a warning about packed shipment quantities changing", Shipment.JS_UnitOfWeightInfo);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSShipment loadedShipment = newFactory.Load<CFSShipment>(Shipment.PK);
			Shipment.Validation.ValidateJS_UnitOfWeight();
			AssertNoWarnings("Warning should only appear for a single instance of the shipment", loadedShipment.JS_UnitOfWeightInfo);
		}

		public void TestWarningShowsWhenVolumeChangedOnPackedShipment()
		{
			CFSPackLine pack = Shipment.OuterPackLines.AddNew();
			CFSContainer container = pack.Containers.AddNew();
			Shipment.JS_ActualVolume = 15;
			Factory.Save();

			Shipment.JS_ActualVolume = 12;
			Shipment.Validation.ValidateJS_ActualVolume();
			AssertHasWarnings("Field should show a warning about packed shipment quantities changing", Shipment.JS_ActualVolumeInfo);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSShipment loadedShipment = newFactory.Load<CFSShipment>(Shipment.PK);
			Shipment.Validation.ValidateJS_ActualVolume();
			AssertNoWarnings("Warning should only appear for a single instance of the shipment", loadedShipment.JS_ActualVolumeInfo);
		}

		public void TestWarningShowsWhenVolumeUnitChangedOnPackedShipment()
		{
			CFSPackLine pack = Shipment.OuterPackLines.AddNew();
			CFSContainer container = pack.Containers.AddNew();
			Shipment.JS_UnitOfVolume = Constants.Volume.CubicYards;
			Factory.Save();

			Shipment.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			Shipment.Validation.ValidateJS_UnitOfVolume();
			AssertHasWarnings("Field should show a warning about packed shipment quantities changing", Shipment.JS_UnitOfVolumeInfo);

			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSShipment loadedShipment = newFactory.Load<CFSShipment>(Shipment.PK);
			Shipment.Validation.ValidateJS_UnitOfVolume();
			AssertNoWarnings("Warning should only appear for a single instance of the shipment", loadedShipment.JS_UnitOfVolumeInfo);
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates_ShouldNotRaiseWarningWhenShipmentIsForwardingOrNotCFS()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creationFactory = new BusinessObjectFactory();
				var shipment1 = creationFactory.NewWithValidTestData<CFSShipment>();
				shipment1.JS_UniqueConsignRef = "ONE";
				shipment1.JS_HouseBill = "HELLO";
				creationFactory.Save();

				var shipment2 = Factory.NewWithValidTestData<CFSShipment>();

				shipment2.JS_UniqueConsignRef = "TWO";
				shipment2.JS_HouseBill = "HELLO";
				shipment2.JS_IsForwardRegistered = true;
				shipment2.JS_IsCFSRegistered = false;

				var shipmentReloaded = Factory.Load<CFSShipment>(shipment1.PK);

				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertNoWarning("Shipment should not have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");

				shipment2.JS_IsCFSRegistered = true;
				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertHasWarning("Shipment should not have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo,
					"This House Bill number is already in use on: \r\nTWO\r\n");
			}
		}

		#region ShipmentType

		public void TestValidateJS_ShipmentType()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();

			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.ShippersConsolLead;
			AssertHasErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			AssertNoErrors(shipment.JS_ShipmentTypeInfo);

			shipment.JS_ShipmentType = "CRP";
			AssertHasErrors(shipment.JS_ShipmentTypeInfo);
		}

		#endregion

		#region Implementation

		CFSShipment Shipment;

		protected override void SetUp()
		{
			OriginalBranchOrg = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			new ConstantsAndReusables(Factory).EnsureCurrentCompanyMatchesCurrentBranch();
			Shipment = Factory.New<CFSShipment>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = OriginalBranchOrg;
		}
		ZGuid OriginalBranchOrg;

		protected virtual CommonShipment GetShipment()
		{
			return Factory.New<CFSShipment>();
		}

		#endregion
	}
}
