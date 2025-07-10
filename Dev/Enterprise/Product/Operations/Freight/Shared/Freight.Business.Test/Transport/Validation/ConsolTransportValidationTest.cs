using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolTransportValidationTest : BaseFreightTest
	{
		#region Cut Off Dates

		public void TestLCL_CutOffDateNotMandatoryWhenTransportUnlinked()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			var transport = consol.Transports[0];
			transport.JW_IsLinked = false;

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), false);
			transport.Validation.ValidateAll();
			AssertNoErrors("Errors should not be raised as registry is off.", transport.JW_DepotCutOffInfo);

			FreightConfigurationRegistry.Instance.ConsolCutOffDate.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), true);
			transport.Validation.ValidateAll();
			AssertEquals("Transport is not linked", false, transport.JW_IsLinked);
			AssertNoErrors("Errors should not be raised as transport is not linked", transport.JW_DepotCutOffInfo);
		}

		#endregion

		#region TestStorageLegsExemptFromPortConstraints

		public void TestStorageLegsExemptFromPortConstraints()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport2 = consol.Transports[0];
			transport2.JW_IsLinked = false;
			transport2.JW_LegOrder = 2;

			Transport transport1 = consol.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Constants.TransportModes.Storage;
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKLoadPort = HomePort;

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Storage;
			transport3.JW_RL_NKLoadPort = OverseasPort;
			transport3.JW_RL_NKDiscPort = OverseasPort;

			consol.Transports.RunPreSaveValidation();
			AssertEquals(2, transport1.Notifications.GetWarnings().Count());
			AssertEquals(2, transport2.Notifications.GetWarnings().Count());
			AssertEquals(2, transport3.Notifications.GetWarnings().Count());
		}

		#endregion

		#region TestValidateLoadAgainstConsolDischarge

		public void TestValidateLoadAgainstConsolDischarge()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports.AddNew();

			consol.JK_RL_NKDischargePort = HomePort;
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_OA_DepartureLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "A routing leg cannot load at the consol's final discharge.");

			transport.JW_RL_NKDiscPort = HomePort;
			transport.JW_OA_ArrivalLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertNoNotifications(transport.JW_RL_NKLoadPortInfo);

			transport.JW_OA_DepartureLocation = ZGuid.Empty;
			AssertHasError(transport.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_RL_NKLoadPort = AlternateHomePort;
			AssertNoErrors(transport.JW_RL_NKLoadPortInfo);
		}

		#endregion

		#region TestValidateDischargeAgainstConsolLoad

		public void TestValidateDischargeAgainstConsolLoad()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports.AddNew();

			consol.JK_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = HomePort;
			transport.JW_OA_ArrivalLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "A routing leg cannot discharge at the consol's first load.");

			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_OA_DepartureLocation = Factory.New<OrgHeader>().MainAddress.PK;
			AssertNoNotifications(transport.JW_RL_NKDiscPortInfo);

			transport.JW_OA_ArrivalLocation = ZGuid.Empty;
			AssertHasError(transport.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport.JW_RL_NKDiscPort = AlternateHomePort;
			AssertNoErrors(transport.JW_RL_NKDiscPortInfo);
		}

		#endregion

		#region TestValidateLoadNotRepeated

		public void TestValidateLoadNotRepeated()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			transport1.JW_RL_NKLoadPort = HomePort;
			AssertNoErrors(transport1.JW_RL_NKLoadPortInfo);

			transport2.JW_RL_NKLoadPort = transport1.JW_RL_NKLoadPort;
			transport1.Validation.ValidateJW_RL_NKLoadPort();
			AssertHasErrors(transport1.JW_RL_NKLoadPortInfo);
		}

		#endregion

		#region TestValidateDischargeNotRepeated

		public void TestValidateDischargeNotRepeated()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			transport1.JW_RL_NKDiscPort = HomePort;
			AssertNoErrors(transport1.JW_RL_NKDiscPortInfo);

			transport2.JW_RL_NKDiscPort = transport1.JW_RL_NKDiscPort;
			transport1.Validation.ValidateJW_RL_NKDiscPort();
			AssertHasErrors(transport1.JW_RL_NKDiscPortInfo);
		}

		#endregion

		#region TestJW_LegOrderIsUnique

		const string LegOrderError = "This value must be unique on the consol.";
		public void TestJW_LegOrderIsUnique()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			AssertNoError(transport1.JW_LegOrderInfo, LegOrderError);
			AssertNoError(transport2.JW_LegOrderInfo, LegOrderError);

			transport2.JW_LegOrder = 1;
			transport1.Validation.ValidateJW_LegOrder();

			AssertHasError(transport1.JW_LegOrderInfo, LegOrderError);
			AssertHasError(transport2.JW_LegOrderInfo, LegOrderError);
		}

		#endregion

		#region TestValidateEstimatedDates

		public void TestValidateEstimatedDates()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			Transport.JW_RL_NKLoadPort = "";
			Transport.JW_RL_NKDiscPort = "";
			Transport.JW_ETD = ZDateTime.Empty;
			Transport.JW_ETA = ZDateTime.Empty;
			Transport.Validation.ValidateAll();

			AssertHasErrors("Error expected on ETD. Not export/import consol & empty ETD.", Transport.JW_ETDInfo);
			AssertNoErrors("No error expected on ETA. Not import consol & empty ETA.", Transport.JW_ETAInfo);

			Transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			Transport.Validation.ValidateAll();
			AssertNoErrors("No error expected on ETD. Import consol & empty ETD.", Transport.JW_ETDInfo);
			AssertHasErrors("Error expected on ETA. Import consol & empty ETA", Transport.JW_ETAInfo);

			Transport.JW_RL_NKDiscPort = "";
			Transport.JW_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			Transport.Validation.ValidateAll();
			AssertHasErrors("Error expected on ETD. Export consol & empty ETD", Transport.JW_ETDInfo);
			AssertNoErrors("No error expected on ETA. Import consol & empty ETA", Transport.JW_ETAInfo);

			ZDateTime currentLocalDate = new ZDateTime(Env.Time.CurrentLocalDate);
			Transport.JW_ETD = currentLocalDate;
			Transport.JW_ETA = currentLocalDate;
			AssertNoErrors("No error expected on ETD. ETD = ETA.", Transport.JW_ETDInfo);
			AssertNoErrors("No error expected on ETA. ETA = ETD.", Transport.JW_ETAInfo);

			Transport.JW_ETD = currentLocalDate.AddDays(1);
			AssertNoErrors("No error expected on ETD. ETD < ETA.AddDays(1).", Transport.JW_ETDInfo);
			AssertNoErrors("No error expected on ETA. ETA > ETD.AddDays(-1).", Transport.JW_ETAInfo);

			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_RL_NKDiscPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			Transport.Validation.ValidateAll();
			AssertHasErrors("Error expected on ETD. ETD > ETA.", Transport.JW_ETDInfo);
			AssertHasErrors("Error expected on ETA. ETA < ETD.", Transport.JW_ETAInfo);

			Transport.JW_RL_NKLoadPort = "USLAX";
			Transport.JW_RL_NKDiscPort = "AUSYD";
			AssertHasError(Transport.JW_ETDInfo, "ETD UTC cannot be after ETA UTC.");
			AssertHasError(Transport.JW_ETAInfo, "ETA UTC cannot be before ETD UTC.");
		}

		#endregion

		#region TestValidateActualDates

		public void TestValidateActualDates()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Air;
			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.JW_RL_NKDiscPort = "GBLON";
			ZDateTime now = new ZDateTime(Env.Time.CurrentUtcDate);

			AssertNoErrors("Pre-condition: No error expected on ATD.", Transport.JW_ATDInfo);
			AssertNoErrors("Pre-condition: No error expected on ATA.", Transport.JW_ATAInfo);

			Transport.JW_ATD = now.AddDays(-3);
			Transport.JW_ATA = now.AddDays(-3);
			AssertNoErrors("No error expected on ATD. ATD and ATA are the same.", Transport.JW_ATDInfo);
			AssertNoErrors("No error expected on ATA. ATD and ATA are the same.", Transport.JW_ATAInfo);

			Transport.JW_ATD = now.AddDays(-4);
			AssertNoErrors("No error expected on ATD. ATD is before the ATA.", Transport.JW_ATDInfo);
			AssertNoErrors("No error expected on ATA. ATD is before the ATA.", Transport.JW_ATAInfo);

			Transport.JW_ATD = now.AddDays(-2);
			Transport.JW_ATA = now.AddDays(-5);
			AssertHasErrors("Error expected on ATD. ATD is after the ATA.", Transport.JW_ATDInfo);
			AssertHasErrors("Error expected on ATA. ATA is after the ATD.", Transport.JW_ATAInfo);

			Transport.JW_ATD = now.AddDays(2);
			Transport.JW_ATA = now.AddDays(5);
			AssertHasErrors("Error expected on ATD. ATD is in the future.", Transport.JW_ATDInfo);
			AssertHasErrors("Error expected on ATA. ATA is in the future", Transport.JW_ATAInfo);
		}

		#endregion

		#region TestValidateLoadingAndDischarge

		public void TestValidateLoadingAndDischarge()
		{
			Transport.JW_Vessel = TestVessel1.RV_FK;
			Transport.JW_VoyageFlight = "32";
			Transport.JW_RL_NKLoadPort = "";
			Transport.JW_RL_NKDiscPort = "";

			Transport.JW_RL_NKLoadPort = HomePort;
			Transport.JW_RL_NKDiscPort = HomePort;
			Transport.Validation.ValidateJW_RL_NKLoadPort();
			Transport.IsDomestic = false;
			AssertHasErrors("Load and discharge should not be the same", Transport.JW_RL_NKLoadPortInfo);
			AssertHasErrors("Load and discharge should not be the same", Transport.JW_RL_NKDiscPortInfo);

			Transport.JW_RL_NKDiscPort = OverseasPort;
			Transport.Validation.ValidateJW_RL_NKLoadPort();
			AssertNoErrors("Discharge not equal to Loading. Should be no errors.", Transport.JW_RL_NKLoadPortInfo);
			AssertNoErrors("Discharge not equal to Loading. Should be no errors.", Transport.JW_RL_NKDiscPortInfo);
		}

		#endregion

		#region TestValidateJW_Vessel

		public void TestValidateJW_Vessel()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_IsLinked = true;

			Transport.Validation.ValidateJW_Vessel();
			AssertHasErrors("Empty Vessel - Error expected", Transport.JW_VesselInfo);

			Transport.JW_IsLinked = false;
			Transport.Validation.ValidateJW_Vessel();
			AssertNoErrors("Empty Vessel but not linked - No error expected", Transport.JW_VesselInfo);

			Transport.JW_Vessel = "AAAAA";
			AssertNoErrors("Unlinked transports should not perform list validation - No Error expected", Transport.JW_VesselInfo);

			Transport.JW_IsLinked = true;
			Transport.Validation.ValidateJW_Vessel();
			AssertHasErrors("Invalid Vessel - Error expected", Transport.JW_VesselInfo);

			Transport.JW_Vessel = TestVessel1.RV_FK;
			AssertNoNotifications("Valid Vessel - No error expected", Transport.JW_VesselInfo);

			Transport.JW_TransportMode = Constants.TransportModes.Rail;
			Transport.JW_Vessel = "1";
			Transport.JW_Vessel = "";
			AssertHasErrors("Empty Journey - Error expected", Transport.JW_VesselInfo);

			Transport.JW_Vessel = "TRUCK";
			AssertNoNotifications("No list validation for Rail Vessel.", Transport.JW_VesselInfo);

			Transport.JW_Vessel = "Any Name";
			AssertNoNotifications("Valid Journey - No error expected", Transport.JW_VesselInfo);

			Transport.JW_TransportMode = Constants.TransportModes.Air;
			Transport.JW_Vessel = "";
			AssertNoNotifications("Empty Vessel - No error expected", Transport.JW_VesselInfo);

			Transport.JW_TransportMode = Constants.TransportModes.Road;
			Transport.Validation.ValidateJW_Vessel();
			AssertNoNotifications("Empty Vessel - No error expected", Transport.JW_VesselInfo);
		}

		#endregion

		#region TestValidateJW_RL_NKLoadPort

		public void TestValidateJW_RL_NKLoadPort()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_RL_NKDiscPort = OverseasPort;

			Transport.JW_RL_NKLoadPort = "";
			Transport.Validation.ValidateJW_RL_NKLoadPort();
			AssertHasError(Transport.JW_RL_NKLoadPortInfo, "Please enter a Load Port.");

			Transport.JW_RL_NKLoadPort = HomePort;
			AssertNoNotifications(Transport.JW_RL_NKLoadPortInfo);

			Transport.JW_RL_NKLoadPort = "ZZZZZ";
			AssertHasError(Transport.JW_RL_NKLoadPortInfo, "Enter a valid Load Port.");

			Transport.JW_IsLinked = false;
			Transport.Validation.ValidateJW_RL_NKLoadPort();
			AssertHasError(Transport.JW_RL_NKLoadPortInfo, "Enter a valid Load Port.");

			Transport.JW_RL_NKLoadPort = HomePort;
			AssertNoNotifications(Transport.JW_RL_NKLoadPortInfo);

			Transport.JW_RL_NKLoadPort = "";
			AssertNoNotifications(Transport.JW_RL_NKLoadPortInfo);

			Transport.JW_RL_NKLoadPort = OverseasPort;
			Transport.IsDomestic = false;
			AssertHasError(Transport.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");
		}

		#endregion

		#region TestValidateJW_RL_NKDiscPort

		public void TestValidateJW_RL_NKDiscPort()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_RL_NKLoadPort = OverseasPort;

			Transport.JW_RL_NKDiscPort = "";
			Transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertHasError(Transport.JW_RL_NKDiscPortInfo, "Please enter a Discharge Port.");

			Transport.JW_RL_NKDiscPort = HomePort;
			AssertNoNotifications(Transport.JW_RL_NKDiscPortInfo);

			Transport.JW_RL_NKDiscPort = "ZZZZZ";
			AssertHasError(Transport.JW_RL_NKDiscPortInfo, "Enter a valid Discharge Port.");

			Transport.JW_IsLinked = false;
			Transport.Validation.ValidateJW_RL_NKDiscPort();
			AssertHasError(Transport.JW_RL_NKDiscPortInfo, "Enter a valid Discharge Port.");

			Transport.JW_RL_NKDiscPort = HomePort;
			AssertNoNotifications(Transport.JW_RL_NKDiscPortInfo);

			Transport.JW_RL_NKDiscPort = "";
			AssertNoNotifications(Transport.JW_RL_NKDiscPortInfo);

			Transport.JW_RL_NKDiscPort = OverseasPort;
			Transport.IsDomestic = false;
			AssertHasError(Transport.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");
		}

		#endregion

		#region TestValidateJW_ETD_TransportOrder

		public void TestValidateJW_ETD_TransportOrder()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();
			AssertETDOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertETDOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertETDOrderValidated(transport2, transport1);
		}

		void AssertETDOrderValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = now;
			AssertNoErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = now.AddDays(1);
			secondTransport.Validation.ValidateJW_ETD();
			AssertHasErrors(firstTransport.JW_ETAInfo);
			AssertHasErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = now.AddDays(-1);
			secondTransport.Validation.ValidateJW_ETD();
			AssertNoErrors(secondTransport.JW_ETDInfo);
			AssertNoErrors(firstTransport.JW_ETAInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;
		}

		void AssertETDOrderNotValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			firstTransport.JW_ETA = now.AddDays(1);
			secondTransport.JW_ETD = now;
			AssertNoErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;

			secondTransport.JW_ETA = now.AddDays(1);
			firstTransport.JW_ETD = now;
			AssertNoErrors(firstTransport.JW_ETDInfo);

			secondTransport.JW_ETA = ZDateTime.Empty;
			firstTransport.JW_ETD = ZDateTime.Empty;
		}

		#endregion

		#region TestValidateJW_ETA_TransportOrder

		public void TestValidateJW_ETA_TransportOrder()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();
			AssertETAOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertETAOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertETAOrderValidated(transport2, transport1);
		}

		void AssertETAOrderValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			secondTransport.JW_ETD = ZDateTime.Empty;
			firstTransport.JW_ETA = now;
			AssertNoErrors(firstTransport.JW_ETAInfo);

			secondTransport.JW_ETD = now.AddDays(-1);
			firstTransport.Validation.ValidateJW_ETA();
			AssertHasErrors(firstTransport.JW_ETAInfo);

			secondTransport.JW_ETD = now.AddDays(1);
			firstTransport.Validation.ValidateJW_ETA();
			AssertNoErrors(firstTransport.JW_ETAInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;
		}

		void AssertETAOrderNotValidated(Transport firstTransport, Transport secondTransport)
		{
			ZDateTime now = ZDateTime.Now;

			firstTransport.JW_ETA = now.AddDays(1);
			secondTransport.JW_ETD = now;
			AssertNoErrors(secondTransport.JW_ETDInfo);

			firstTransport.JW_ETA = ZDateTime.Empty;
			secondTransport.JW_ETD = ZDateTime.Empty;

			secondTransport.JW_ETA = now.AddDays(1);
			firstTransport.JW_ETD = now;
			AssertNoErrors(firstTransport.JW_ETDInfo);

			secondTransport.JW_ETA = ZDateTime.Empty;
			firstTransport.JW_ETD = ZDateTime.Empty;
		}

		#endregion

		#region TestValidateJW_ATD

		public void TestValidateJW_ATD()
		{
			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			AssertATDOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertATDOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertATDOrderValidated(transport2, transport1);
		}

		void AssertATDOrderValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = now;
			AssertNoErrors(transport2.JW_ATDInfo);

			transport1.JW_ATA = now.AddDays(1);
			transport2.Validation.ValidateJW_ATD();
			AssertHasError(transport1.JW_ATAInfo, legOutOfOrderMessage);
			AssertHasError(transport2.JW_ATDInfo, legOutOfOrderMessage);

			transport1.JW_ATA = now.AddDays(-1);
			transport2.Validation.ValidateJW_ATD();
			AssertNoErrors(transport2.JW_ATDInfo);
			AssertNoErrors(transport1.JW_ATAInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;
		}

		void AssertATDOrderNotValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.JW_ATA = now.AddDays(1);
			transport2.JW_ATD = now;
			AssertNoErrors(transport2.JW_ATDInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;

			transport2.JW_ATA = now.AddDays(1);
			transport1.JW_ATD = now;
			AssertNoErrors(transport1.JW_ATDInfo);

			transport2.JW_ATA = ZDateTime.Empty;
			transport1.JW_ATD = ZDateTime.Empty;
		}

		#endregion

		#region TestValidateJW_ATA

		public void TestValidateJW_ATA_TransportOrder()
		{
			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			AssertATAOrderValidated(transport1, transport2);

			transport2.JW_LegOrder = 1;
			AssertATAOrderNotValidated(transport1, transport2);

			transport1.JW_LegOrder = 2;
			AssertATAOrderValidated(transport2, transport1);
		}

		void AssertATAOrderValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport2.JW_ATD = ZDateTime.Empty;
			transport1.JW_ATA = now;
			AssertNoErrors(transport1.JW_ATAInfo);

			transport2.JW_ATD = now.AddDays(-1);
			transport1.Validation.ValidateJW_ATA();
			AssertHasError(transport1.JW_ATAInfo, legOutOfOrderMessage);

			transport2.JW_ATD = now.AddDays(1);
			transport1.Validation.ValidateJW_ATA();
			AssertNoErrors(transport1.JW_ATAInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;
		}

		void AssertATAOrderNotValidated(Transport transport1, Transport transport2)
		{
			var now = ZDateTime.Now;

			transport1.JW_ATA = now.AddDays(1);
			transport2.JW_ATD = now;
			AssertNoErrors(transport2.JW_ATDInfo);

			transport1.JW_ATA = ZDateTime.Empty;
			transport2.JW_ATD = ZDateTime.Empty;

			transport2.JW_ATA = now.AddDays(1);
			transport1.JW_ATD = now;
			AssertNoErrors(transport1.JW_ATDInfo);

			transport2.JW_ATA = ZDateTime.Empty;
			transport1.JW_ATD = ZDateTime.Empty;
		}

		#endregion

		#region TestValidateJW_IsCargoOnly

		void SetupForTestValidateJW_IsCargoOnly(CommonConsol consol)
		{
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packline = shipment.OuterPackLines.AddNew();
			UNDGDataItem undg = packline.UNDGs.AddNew();
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "99999";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_LQ2OrPaxMaxAmtType = UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
			undg.DI_DG = substance.PK;
		}

		public void TestValidateJW_IsCargoOnlyWhenTransportModeIsAIR()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports.AddNew();
			transport.JW_TransportMode = "AIR";

			transport.JW_IsCargoOnly = false;
			AssertNoErrors(transport.JW_IsCargoOnlyInfo);

			SetupForTestValidateJW_IsCargoOnly(consol);
			transport.RunPreSaveValidation();
			AssertHasError(transport.JW_IsCargoOnlyInfo, "This Consol has Shipments attached with Dangerous Goods substances that are forbidden on a passenger flight.");

			transport.JW_IsCargoOnly = true;
			AssertNoErrors(transport.JW_IsCargoOnlyInfo);
		}

		public void TestValidateJW_IsCargoOnlyNeverThrowErrorWhenTransportModeIsNotAIR()
		{
			string[] transportModes = { Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Rail };

			foreach (var mode in transportModes)
			{
				CommonConsol consol = Factory.New<CommonConsol>();
				Transport transport = consol.Transports.AddNew();
				transport.JW_TransportMode = mode;

				transport.JW_IsCargoOnly = false;
				AssertNoErrors(transport.JW_IsCargoOnlyInfo);

				SetupForTestValidateJW_IsCargoOnly(consol);
				transport.RunPreSaveValidation();
				AssertNoErrors(transport.JW_IsCargoOnlyInfo);

				transport.JW_IsCargoOnly = true;
				AssertNoErrors(transport.JW_IsCargoOnlyInfo);
			}
		}

		#endregion

		#region Transport

		Transport Transport
		{
			get
			{
				if (fTransport == null)
				{
					fTransport = Factory.New<CommonConsol>().Transports[0];
				}

				return fTransport;
			}
		}
		Transport fTransport;

		#endregion

		#region TestOneDayToleranceForAirValidation

		public void TestOneDayToleranceForAirValidation_ETD()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_LegOrder = 1;
			transport2.JW_LegOrder = 2;
			transport1.JW_RL_NKDiscPort = "USHNL";
			transport2.JW_RL_NKLoadPort = "AUSYD";

			var etd = ZDateTime.Now;
			transport1.JW_ETD = etd;
			transport1.JW_ETA = etd.AddDays(4);
			transport2.JW_ETD = etd.AddDays(4).AddHours(-6);
			transport2.JW_ETA = etd.AddDays(8);
			AssertNoErrors("ETD is less than a day before the previous transport's ETA", transport2.JW_ETDInfo);

			transport2.JW_ETD = etd.AddDays(2);
			AssertHasError("ETD is more than a day before the previous transport's ETA", transport2.JW_ETDInfo, legOutOfOrderMessage);
		}

		public void TestOneDayToleranceForAirValidation_ETA()
		{
			var consol = Factory.New<CommonConsol>();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			transport1.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport1.JW_LegOrder = 1;
			transport2.JW_LegOrder = 2;
			transport1.JW_RL_NKDiscPort = "USHNL";
			transport2.JW_RL_NKLoadPort = "AUSYD";

			transport2.JW_ETD = ZDateTime.Now.AddDays(10).AddHours(-6);
			transport2.JW_ETA = ZDateTime.Now.AddDays(20);
			transport1.JW_ETD = ZDateTime.Now;
			transport1.JW_ETA = ZDateTime.Now.AddDays(10);
			AssertNoErrors("ETA is less than a day after the next transport's ETD", transport1.JW_ETAInfo);

			transport1.JW_ETA = ZDateTime.Now.AddDays(12);
			AssertHasError("ETA is more than a day after the next transport's ETD", transport1.JW_ETAInfo, legOutOfOrderMessage);
		}

		[TestDate(2014, 07, 31)]
		public void TestOneDayToleranceForLegsBeforeAirLeg_ETD()
		{
			var today = ZDateTime.Now;
			var consol = Factory.New<CommonConsol>();
			var roadLeg = consol.Transports[0];
			roadLeg.JW_TransportMode = Constants.TransportModes.Road;
			roadLeg.JW_RL_NKLoadPort = "JPNRT";
			roadLeg.JW_RL_NKDiscPort = "JPHND";
			roadLeg.JW_ETD = today.AddHours(17);
			roadLeg.JW_ETA = today.AddHours(19);

			var flight1 = consol.Transports.AddNew();
			flight1.JW_TransportMode = Constants.TransportModes.Air;
			flight1.JW_RL_NKLoadPort = "JPHND";
			flight1.JW_RL_NKDiscPort = "USHNL";
			flight1.JW_ETD = today.AddHours(25).AddMinutes(55);
			flight1.JW_ETA = today.AddHours(12).AddMinutes(50);

			var flight2 = consol.Transports.AddNew();
			flight2.JW_TransportMode = Constants.TransportModes.Air;
			flight2.JW_RL_NKLoadPort = "USHNL";
			flight2.JW_RL_NKDiscPort = "USJFK";
			flight2.JW_ETD = today.AddHours(15).AddMinutes(20);
			flight2.JW_ETA = today.AddDays(1).AddHours(7);

			var errorMessage = "Expected no error on the ETD even though it's before the ETA on both preceding legs."
				+ "As flights can cross time zones any air leg or non-sea leg is given a 24 hour buffer for this validation";

			AssertNoError(errorMessage, flight2.JW_ETDInfo, legOutOfOrderMessage);

			var seaLeg = consol.Transports.AddNew();
			seaLeg.JW_TransportMode = Constants.TransportModes.Sea;

			Assert("Pre-con", seaLeg.JW_LegOrder > flight2.JW_LegOrder);
			AssertNoError("Expected no error as sea leg occurs after flight 2", flight2.JW_ETDInfo, legOutOfOrderMessage);

			var intitalSeaLegOrder = seaLeg.JW_LegOrder;
			seaLeg.JW_LegOrder = flight2.JW_LegOrder;
			flight2.JW_LegOrder = intitalSeaLegOrder;

			flight2.Validation.ValidateJW_ETD();

			AssertHasError("Expected error as now there is a sea leg between the routes", flight2.JW_ETDInfo, legOutOfOrderMessage);

			seaLeg.JW_TransportMode = Constants.TransportModes.Road;
			flight2.Validation.ValidateJW_ETD();

			AssertNoError(flight2.JW_ETDInfo, legOutOfOrderMessage);
		}

		[TestDate(2014, 07, 31)]
		public void TestOneDayToleranceForLegsAfterAirLeg_ETA()
		{
			var today = ZDateTime.Now;
			var consol = Factory.New<CommonConsol>();
			var roadLeg1 = consol.Transports[0];
			roadLeg1.JW_TransportMode = Constants.TransportModes.Road;
			roadLeg1.JW_RL_NKLoadPort = "AUBTB";
			roadLeg1.JW_RL_NKDiscPort = "AUSYD";
			roadLeg1.JW_ETD = today.AddHours(10).AddMinutes(20);
			roadLeg1.JW_ETA = today.AddHours(11);

			var flight = consol.Transports.AddNew();
			flight.JW_TransportMode = Constants.TransportModes.Air;
			flight.JW_RL_NKLoadPort = "AUSYD";
			flight.JW_RL_NKDiscPort = "USLAX";
			flight.JW_ETD = today.AddHours(11).AddMinutes(30);
			flight.JW_ETA = today.AddHours(9);      //roughly the local time after a 15.5 hour flight

			var roadLeg2 = consol.Transports.AddNew();
			roadLeg2.JW_TransportMode = Constants.TransportModes.Road;
			roadLeg2.JW_RL_NKLoadPort = "USLAX";
			roadLeg2.JW_RL_NKDiscPort = "USSFO";
			roadLeg2.JW_ETD = today.AddHours(9).AddMinutes(20);
			roadLeg2.JW_ETA = today.AddDays(1);

			var errorMessage = "Expected no error on the ETA even though it's after the ETD on both following legs. "
				+ "As flights can cross time zones any air leg or non-sea leg is given a 24 hour buffer for this validation";

			AssertNoError(errorMessage, roadLeg1.JW_ETAInfo, legOutOfOrderMessage);

			var seaLeg = consol.Transports.AddNew();
			seaLeg.JW_TransportMode = Constants.TransportModes.Sea;

			Assert("Pre-con", seaLeg.JW_LegOrder > roadLeg1.JW_LegOrder);
			AssertNoError("Expected no error as sea leg does not occur between the conflicting dates", roadLeg1.JW_ETDInfo, legOutOfOrderMessage);

			var intitalSeaLegOrder = seaLeg.JW_LegOrder;
			seaLeg.JW_LegOrder = roadLeg1.JW_LegOrder;
			roadLeg1.JW_LegOrder = intitalSeaLegOrder;

			roadLeg1.Validation.ValidateJW_ETD();

			AssertHasError("Expected error as now there is a sea leg between the routes", roadLeg1.JW_ETDInfo, legOutOfOrderMessage);

			seaLeg.JW_TransportMode = Constants.TransportModes.Road;
			roadLeg1.Validation.ValidateJW_ETD();

			AssertNoError(roadLeg1.JW_ETAInfo, legOutOfOrderMessage);
		}

		[TestDate(2014, 07, 31)]
		public void TestOneDayToleranceDoesNotApplyDirectlyAfterAir()
		{
			var today = ZDateTime.Now;
			var consol = Factory.New<CommonConsol>();

			var roadTransport = consol.Transports[0];
			roadTransport.JW_TransportMode = Constants.TransportModes.Road;
			roadTransport.JW_RL_NKLoadPort = "JPHND";
			roadTransport.JW_RL_NKDiscPort = "USLAX";
			roadTransport.JW_ETD = today.AddHours(10);
			roadTransport.JW_ETA = today.AddHours(14);

			var flight = consol.Transports.AddNew();
			flight.JW_TransportMode = Constants.TransportModes.Air;
			flight.JW_RL_NKLoadPort = "USLAX";
			flight.JW_RL_NKDiscPort = "USSFO";
			flight.JW_ETD = today.AddHours(12);
			flight.JW_ETA = today.AddHours(18);

			AssertHasError("Flight should not leave before air has arrived", flight.JW_ETDInfo, legOutOfOrderMessage);
		}

		const string legOutOfOrderMessage = "The date order does not reflect the leg order.";

		#endregion
	}
}
