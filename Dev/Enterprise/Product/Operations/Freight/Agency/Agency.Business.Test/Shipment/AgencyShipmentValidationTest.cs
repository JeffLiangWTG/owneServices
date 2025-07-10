using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentValidationTest : BaseAgencyTest
	{
		public void TestValidateJS_NKLoadPort()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_NKLoadPort = ZString.Empty;
			shipment.Validation.ValidateJS_NKLoadPort();
			AssertHasWarnings("Expecting Load Port to be empty and have warnings.", shipment.JS_NKLoadPortInfo);
			shipment.JS_NKLoadPort = "ABCDE";
			shipment.Validation.ValidateJS_NKLoadPort();
			AssertHasErrors("Expecting Load Port to be invalid and have errors.", shipment.JS_NKLoadPortInfo);
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.Validation.ValidateJS_NKLoadPort();
			AssertNoWarnings("Expecting Load Port to be correct, not expecting warnings.", shipment.JS_NKLoadPortInfo);
			shipment.JS_NKDischargePort = "AUSYD";
			shipment.Validation.ValidateJS_NKLoadPort();
			AssertHasErrors("Load and Discharge ports are the same, expecting errors.", shipment.JS_NKLoadPortInfo);
		}

		public void TestValidateJS_NKDischargePort()
		{
			JobSailing sailing = CreateSailing("AUBNE", "NLAMS", ZDateTime.Today);
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = ZString.Empty;
			shipment.Validation.ValidateJS_NKDischargePort();
			AssertHasWarnings("Expecting Discharge Port to be empty and have warnings.", shipment.JS_NKDischargePortInfo);
			shipment.JS_NKDischargePort = "ABCDE";
			shipment.Validation.ValidateJS_NKDischargePort();
			AssertHasErrors("Expecting Discharge Port to be invalid and have errors.", shipment.JS_NKDischargePortInfo);
			shipment.JS_NKDischargePort = "CNCAH";
			shipment.Validation.ValidateJS_NKDischargePort();
			AssertNoWarnings("Expecting Discharge Port to be correct, not expecting warnings.", shipment.JS_NKDischargePortInfo);
			shipment.JS_NKLoadPort = "CNCAH";
			shipment.Validation.ValidateJS_NKDischargePort();
			AssertHasErrors("Load and Discharge ports are the same, expecting errors.", shipment.JS_NKDischargePortInfo);
		}

		public void TestValidateJS_A_BKD()
		{
			JobSailing sailing = CreateSailing("AUSYD", "CNCAH", ZDateTime.Today.AddDays(1));
			Factory.Save();
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_A_BKD = ZDateTime.Invalid;
			shipment.Validation.ValidateJS_A_BKD();
			AssertHasErrors("Expecting Booked Date to be invalid and have errors.", shipment.JS_A_BKDInfo);
			shipment.JS_A_BKD = ZDateTime.Today;
			shipment.Validation.ValidateJS_A_BKD();
			AssertNoErrors("Expecting Booked Date to be correct, not expecting errors.", shipment.JS_A_BKDInfo);
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "CNCAH";
			AssertEquals("Expecting sailing to have been chosen", sailing.PK, shipment.JS_JX);
			AssertNoErrors("Expecting booked date to be correct", shipment.JS_A_BKDInfo);
			AssertNoWarnings("Expecting booked date to be correct", shipment.JS_A_BKDInfo);
			shipment.JS_A_BKD = ZDateTime.Today.AddDays(40);
			AssertHasWarnings("Expecting booked date to have warning", shipment.JS_A_BKDInfo);
			AssertNoErrors("Expecting booked date to have warning", shipment.JS_A_BKDInfo);
		}

		public void TestValidateJS_RL_NKOrigin()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = ZString.Empty;
			shipment.Validation.ValidateJS_RL_NKOrigin();
			AssertHasErrors("Expecting Origin to be empty and have errors.", shipment.JS_RL_NKOriginInfo);
			shipment.JS_RL_NKOrigin = "ABCDE";
			shipment.Validation.ValidateJS_RL_NKOrigin();
			AssertHasErrors("Expecting Origin to be invalid and have errors.", shipment.JS_RL_NKOriginInfo);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.Validation.ValidateJS_RL_NKOrigin();
			AssertNoErrors("Expecting Origin to be correct, not expecting errors.", shipment.JS_RL_NKOriginInfo);
		}

		public void TestValidateJS_RL_NKDestination()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKDestination = ZString.Empty;
			shipment.Validation.ValidateJS_RL_NKDestination();
			AssertHasErrors("Expecting Destination to be empty and have errors.", shipment.JS_RL_NKDestinationInfo);
			shipment.JS_RL_NKDestination = "ABCDE";
			shipment.Validation.ValidateJS_RL_NKDestination();
			AssertHasErrors("Expecting Destination to be invalid and have errors.", shipment.JS_RL_NKDestinationInfo);
			shipment.JS_RL_NKDestination = "CHRRC";
			shipment.Validation.ValidateJS_RL_NKDestination();
			AssertNoErrors("Expecting Destination to be correct, not expecting errors.", shipment.JS_RL_NKDestinationInfo);
			shipment.JS_RL_NKDestination = "CNCAN";
			shipment.Validation.ValidateJS_RL_NKDestination();
			AssertNoErrors("Expecting Destination to be correct, not expecting errors.", shipment.JS_RL_NKDestinationInfo);
		}

		public void TestValidateJS_GoodsDescription()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_GoodsDescription = ZString.Empty;
			shipment.Validation.ValidateJS_GoodsDescription();
			AssertHasErrors("Expecting Goods Description to be empty and have errors.", shipment.JS_GoodsDescriptionInfo);
			shipment.JS_GoodsDescription = "CARGO";
			shipment.Validation.ValidateJS_GoodsDescription();
			AssertNoErrors("Expecting Goods Description to be correct, not expecting errors.", shipment.JS_GoodsDescriptionInfo);
		}

		public void TestValidateJS_JX()
		{
			JobSailing sailing = CreateSailing("AUSYD", "CNCAN", ZDateTime.Today.AddDays(10));
			Factory.Save();
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.Validation.ValidateJS_JX();
			AssertHasWarnings("Expecting Sailing (JS_JX) to be empty and have warnings.", shipment.JS_JXInfo);
			shipment.JS_A_BKD = ZDateTime.Today;
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = ZString.Empty;
			shipment.Validation.ValidateJS_JX();
			AssertHasWarnings("Expecting Sailing (JS_JX) to be empty and have warnings.", shipment.JS_JXInfo);
			shipment.JS_A_BKD = ZDateTime.Today;
			shipment.JS_NKLoadPort = ZString.Empty;
			shipment.JS_NKDischargePort = "CNCAN";
			shipment.Validation.ValidateJS_JX();
			AssertHasWarnings("Expecting Sailing (JS_JX) to be empty and have warnings.", shipment.JS_JXInfo);
			shipment.JS_A_BKD = ZDateTime.Today;
			shipment.JS_NKLoadPort = "AUSYD";
			shipment.JS_NKDischargePort = "CNCAN";
			shipment.Validation.ValidateJS_JX();
			AssertNoWarnings("Expecting Sailing (JS_JX) to be correct, not expecting warnings.", shipment.JS_JXInfo);
		}

		public void TestValidateJS_INCO()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Collect;
			Assert(!shipment.JS_INCOInfo.HasError("Enter a valid Payment Term."));
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.CollectThirdParty;
			Assert(shipment.JS_INCOInfo.HasError("Enter a valid Payment Term."));
			shipment.JS_INCO = Core.Constants.DomesticPaymentTerms.Prepaid;
			Assert(!shipment.JS_INCOInfo.HasError("Enter a valid Payment Term."));
		}

		public void TestValidateCustomsEntryNumber()
		{
			ZString cAN = CANType.CustomsAuthorityNumber.Code;
			ZString cCN = CANType.ContingencyCustomsAuthorityNumber.Code;
			ZString xDD = CMRExportExemptionCodes.EXDD.Code;
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AssertNoWarning("CustomsEntryNumberInfo should has NO warning", shipment.CustomsEntryNumberInfo, "CAN must be 9 characters.");
			AssertNoWarning("CustomsEntryNumberInfo should has NO warning", shipment.CustomsEntryNumberInfo, "Entry Number is not specified");
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", shipment.CustomsEntryNumberInfo);
			shipment.CustomsEntryNumberType = xDD;
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", shipment.CustomsEntryNumberInfo);
			shipment.CustomsEntryNumberType = cAN;
			AssertHasWarning("CustomsEntryNumberInfo should has warning", shipment.CustomsEntryNumberInfo, "Entry Number is not specified");
			shipment.CustomsEntryNumberType = cCN;
			AssertHasWarning("CustomsEntryNumberInfo should has warning", shipment.CustomsEntryNumberInfo, "Entry Number is not specified");
			shipment.CustomsEntryNumber = "777";
			AssertNoWarning("CustomsEntryNumberInfo should has NO warning", shipment.CustomsEntryNumberInfo, "Entry Number is not specified");
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", shipment.CustomsEntryNumberInfo);
			shipment.CustomsEntryNumberType = cAN;
			AssertHasWarning("CustomsEntryNumberInfo should has warning", shipment.CustomsEntryNumberInfo, "CAN must be 9 characters.");
			shipment.CustomsEntryNumber = "AAAAJH4EF";
			AssertNoWarning("CustomsEntryNumberInfo should has NO warning", shipment.CustomsEntryNumberInfo, "CAN must be 9 characters.");
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", shipment.CustomsEntryNumberInfo);
		}

		public void TestValidateContainerCustomsEntryNumberAU()
		{
			const string error = "CAN is available on either Shipment OR Container level";
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
				AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
				AgencyShipmentContainer containerReal1 = shipment.RealContainers.AddNew();
				containerReal1.CustomsEntryNumberType = "CAN";
				containerReal1.CustomsEntryNumber = "111";
				AgencyShipmentContainer containerReal2 = shipment.RealContainers.AddNew();
				AgencyShipmentContainer containerBooked1 = shipment.BookedContainers.AddNew();
				containerBooked1.CustomsEntryNumberType = "CAN";
				containerBooked1.CustomsEntryNumber = "222";
				AgencyShipmentContainer containerBooked2 = shipment.BookedContainers.AddNew();
				AssertNoError("CustomsEntryNumberInfo should has NO error", containerReal1.CustomsEntryNumberInfo, error);
				AssertNoError("CustomsEntryNumberInfo should has NO error", containerReal2.CustomsEntryNumberInfo, error);
				AssertNoError("CustomsEntryNumberInfo should has NO error", containerBooked1.CustomsEntryNumberInfo, error);
				AssertNoError("CustomsEntryNumberInfo should has NO error", containerBooked2.CustomsEntryNumberInfo, error);
				shipment.CustomsEntryNumberType = "CAN";
				shipment.CustomsEntryNumber = "777";
				foreach (AgencyShipmentContainer container in shipment.BookedContainers)
				{
					container.Validation.ValidateAll();
				}

				foreach (AgencyShipmentContainer container in shipment.RealContainers)
				{
					container.Validation.ValidateAll();
				}

				AssertHasError("CustomsEntryNumberInfo should has error", containerReal1.CustomsEntryNumberInfo, error);
				AssertNoError("CustomsEntryNumberInfo should has NO error", containerReal2.CustomsEntryNumberInfo, error);
				AssertHasError("CustomsEntryNumberInfo should has error", containerBooked1.CustomsEntryNumberInfo, error);
				AssertNoError("CustomsEntryNumberInfo should has NO error", containerBooked2.CustomsEntryNumberInfo, error);
				shipment.CustomsEntryNumber = "";
				foreach (AgencyShipmentContainer container in shipment.BookedContainers)
				{
					container.Validation.ValidateAll();
					AssertNoError("CustomsEntryNumberInfo should has NO error", container.CustomsEntryNumberInfo, error);
				}

				foreach (AgencyShipmentContainer container in shipment.RealContainers)
				{
					container.Validation.ValidateAll();
					AssertNoError("CustomsEntryNumberInfo should has NO error", container.CustomsEntryNumberInfo, error);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry;
			}
		}

		public void TestValidateContainerCustomsEntryNumberNotAU()
		{
			ZString currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.PuertoRico;
				AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
				AgencyShipmentContainer containerReal1 = shipment.RealContainers.AddNew();
				containerReal1.CustomsEntryNumber = "111";
				AgencyShipmentContainer containerReal2 = shipment.RealContainers.AddNew();
				AgencyShipmentContainer containerBooked1 = shipment.BookedContainers.AddNew();
				containerBooked1.CustomsEntryNumber = "222";
				AgencyShipmentContainer containerBooked2 = shipment.BookedContainers.AddNew();
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerReal1.CustomsEntryNumberInfo);
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerReal2.CustomsEntryNumberInfo);
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerBooked1.CustomsEntryNumberInfo);
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerBooked2.CustomsEntryNumberInfo);
				shipment.CustomsEntryNumber = "777";
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerReal1.CustomsEntryNumberInfo);
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerReal2.CustomsEntryNumberInfo);
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerBooked1.CustomsEntryNumberInfo);
				AssertNoNotifications("CustomsEntryNumberInfo should has NO error", containerBooked2.CustomsEntryNumberInfo);
				shipment.CustomsEntryNumber = "";
				foreach (AgencyShipmentContainer container in shipment.BookedContainers)
				{
					container.Validation.ValidateAll();
					AssertNoNotifications("CustomsEntryNumberInfo should has NO error", container.CustomsEntryNumberInfo);
				}

				foreach (AgencyShipmentContainer container in shipment.RealContainers)
				{
					container.Validation.ValidateAll();
					AssertNoNotifications("CustomsEntryNumberInfo should has NO error", container.CustomsEntryNumberInfo);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = currentCountry;
			}
		}

		#region Top Level Packs Support
		public void TestTopLevelPacks_ShouldNotCheckMeasuresAgainstPacklinesTotals()
		{
			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				AgencyShipment shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = "XXX";
				shipment.JS_OuterPacks = 10;
				shipment.JS_ActualVolume = 1m;
				shipment.JS_ActualWeight = 10m;
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 1;
				packline.JL_ActualVolume = 2m;
				packline.JL_ActualWeight = 20m;
				shipment.Validation.ValidateJS_OuterPacks();
				shipment.Validation.ValidateJS_ActualVolume();
				shipment.Validation.ValidateJS_ActualWeight();
				AssertHasWarning(shipment.JS_OuterPacksInfo, "Entered number of packs does not equal the total number in the Pack Lines.");
				AssertHasWarning(shipment.JS_ActualVolumeInfo, "Entered volume does not match total volume of the packlines.");
				AssertHasWarning(shipment.JS_ActualWeightInfo, "Entered weight does not match total weight of the packlines.");
				shipment.JS_PackingMode = mode;
				packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 3;
				packline.JL_ActualVolume = 3m;
				packline.JL_ActualWeight = 30m;
				shipment.Validation.ValidateJS_OuterPacks();
				shipment.Validation.ValidateJS_ActualVolume();
				shipment.Validation.ValidateJS_ActualWeight();
				AssertNoWarning(shipment.JS_OuterPacksInfo, "Entered number of packs does not equal the total number in the Pack Lines.");
				AssertNoWarning(shipment.JS_ActualVolumeInfo, "Entered volume does not match total volume of the packlines.");
				AssertNoWarning(shipment.JS_ActualWeightInfo, "Entered weight does not match total weight of the packlines.");
			}
		}

		public void TestTopLevelPacks_ShouldCheckMeasuresAgainstContainerTotals()
		{
			foreach (string mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				AgencyShipment shipment = Factory.New<AgencyShipment>();
				shipment.JS_PackingMode = "XXX";
				shipment.JS_ActualVolume = 1m;
				shipment.JS_ActualWeight = 10m;
				shipment.JS_OuterPacks = 1;
				var container = shipment.BookedContainers.AddNew();
				container.JC_GrossVolume = 2m;
				container.JC_GrossWeight = 20m;
				container.JC_ContainerCount = 2;
				Action<bool> assertShipmentHasMeasuresMismatchWarnings = (warningsExpected) =>
				{
					shipment.Validation.ValidateJS_ActualVolume();
					shipment.Validation.ValidateJS_ActualWeight();
					shipment.Validation.ValidateJS_OuterPacks();
					string volumeWarning = "Entered volume does not match total volume of the vehicles/packs.";
					string weightWarning = "Entered weight does not match total weight of the vehicles/packs.";
					string packsWarning = "Entered number of packs does not match total number in vehicles/packs.";
					if (warningsExpected)
					{
						AssertHasWarning(shipment.JS_ActualVolumeInfo, volumeWarning);
						AssertHasWarning(shipment.JS_ActualWeightInfo, weightWarning);
						AssertHasWarning(shipment.JS_OuterPacksInfo, packsWarning);
					}
					else
					{
						AssertNoWarning(shipment.JS_ActualVolumeInfo, volumeWarning);
						AssertNoWarning(shipment.JS_ActualWeightInfo, weightWarning);
						AssertNoWarning(shipment.JS_OuterPacksInfo, packsWarning);
					}
				};
				assertShipmentHasMeasuresMismatchWarnings(false);
				shipment.JS_PackingMode = mode;
				shipment.BookedContainers.RemoveAll();
				container = shipment.BookedContainers.AddNew();
				container.JC_GrossVolume = 2m;
				container.JC_GrossWeight = 20m;
				container.JC_ContainerCount = 2;
				assertShipmentHasMeasuresMismatchWarnings(true);
				shipment.JS_ActualVolume = 2m;
				shipment.JS_ActualWeight = 20m;
				shipment.JS_OuterPacks = 2;
				assertShipmentHasMeasuresMismatchWarnings(false);
			}
		}

		#endregion
		public void TestCheckDeliveryAgent_ShipmentHasAnEmptyDeliveryAgent_GenerateError()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			shipment.Validation.ValidateJS_OH_DeliveryAgent();
			AssertHasErrors("An error is expected.", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestCheckDeliveryAgent_ShipmentHasAnUnexistingDeliveryAgent_GenerateError()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = ZGuid.NewZGuid();
			AssertHasErrors("An error is expected.", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestCheckDeliveryAgent_ShipmentHasADeliveryAgentWhichIsNotPrincipal_GenerateError()
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_IsShippingProvider = true;
			agent.CompanyData.OB_CRIsShipsAgencyPrincipal = false;
			Factory.Save();
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = agent.PK;
			AssertHasErrors("An error is expected.", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestCheckDeliveryAgent_ShipmentHasADeliveryAgentWhichIsAllowedPrincipal_PrincipalShouldBeEqualDeliveryAgent()
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_IsShippingProvider = true;
			agent.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();
			Env.Security.AgencyPrincipalAccess.IsAllowed = true;
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = agent.PK;
			AssertNoErrors("Should have no errors.", shipment.JS_OH_DeliveryAgentInfo);
			AssertEquals("Principal should be valid", agent.PK, shipment.Principal.PK);
		}

		public void TestCheckDeliveryAgent_ShipmentHasADeliveryAgentWhichIsDeniedPrincipal_GenerateError()
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_IsShippingProvider = true;
			agent.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();
			Env.Security.AgencyPrincipalAccess.IsAllowed = false;
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = agent.PK;
			AssertHasErrors("An error is expected.", shipment.JS_OH_DeliveryAgentInfo);
		}

		public void TestValidateJS_HouseBill_CheckForDuplicates_ShouldNotRaiseWarningWhenShipmentIsForwardingOrCFS()
		{
			using (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creationFactory = new BusinessObjectFactory();
				var shipment1 = creationFactory.NewWithValidTestData<AgencyShipment>();
				shipment1.JS_UniqueConsignRef = "ONE";
				shipment1.JS_HouseBill = "HELLO";
				creationFactory.Save();
				var shipment2 = Factory.NewWithValidTestData<AgencyShipment>();
				shipment2.JS_UniqueConsignRef = "TWO";
				shipment2.JS_HouseBill = "HELLO";
				shipment2.JS_IsForwardRegistered = true;
				var shipmentReloaded = Factory.Load<CommonShipment>(shipment1.PK);
				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertNoWarning("Shipment should not have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nTWO\r\n");
				shipment2.JS_IsCFSRegistered = true;
				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertNoWarning("Shipment should not have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nTWO\r\n");
				shipment2.JS_IsCFSRegistered = false;
				shipment2.JS_IsForwardRegistered = false;
				shipmentReloaded.Validation.ValidateJS_HouseBill();
				AssertHasWarning("Shipment should have warnings on JS_HouseBill, duplicate entry", shipmentReloaded.JS_HouseBillInfo, "This House Bill number is already in use on: \r\nTWO\r\n");
			}
		}

		public void TestValidateJS_ShipmentStatus_AgencyBooking()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();

			booking.JS_ShipmentStatus = ZString.Empty;
			booking.Validation.ValidateJS_ShipmentStatus();

			AssertHasError(booking.JS_ShipmentStatusInfo, "Please enter a Status.");

			booking.JS_ShipmentStatus = "GTW";
			booking.Validation.ValidateJS_ShipmentStatus();

			AssertHasError(booking.JS_ShipmentStatusInfo, "Enter a valid Status.");

			foreach (ICodeDescription status in new AgencyShipmentStatusList(false))
			{
				booking.JS_ShipmentStatus = status.Code;

				Factory.Save();
				booking.Reload();

				booking.Validation.ValidateJS_ShipmentStatus();

				AssertNoError(booking.JS_ShipmentStatusInfo, "Enter a valid Status.");
			}
		}

		#region Implementation

		JobSailing CreateSailing(ZString load, ZString discharge, ZDateTime etd)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = load;
			origin.JA_E_DEP = etd;
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;
			return sailing;
		}
		#endregion
	}
}
