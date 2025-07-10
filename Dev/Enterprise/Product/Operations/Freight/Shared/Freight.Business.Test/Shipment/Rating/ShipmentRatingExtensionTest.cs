using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentRatingExtensionTest : BaseFreightTest
	{
		#region DocsAndCartageServiceInfos

		public void TestDocsAndCartageServiceInfos()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var services = shipment.GetJobServices();

			AssertJobServicesCollection(services, false);

			shipment.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.Now;
			shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Now;
			shipment.DocsAndCartage.JP_DeliveryTruckWaitTime = ZDateTime.Now;
			shipment.DocsAndCartage.JP_DeliveryLabourTime = ZDateTime.Now;
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = new ZByte(4);

			services = shipment.GetJobServices();
			AssertJobServicesCollection(services, true);
		}

		static void AssertJobServicesCollection(JobServicesCollection services, bool isEnabled)
		{
			AssertNotNull(services);
			Assert(services.Any());

			var serviceInfos = services.FindServices(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.CartageDemurrageTotal).ToList();
			var serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(isEnabled, serviceInfo.IsEnabled);
			AssertEquals("HR", serviceInfo.Unit);
			AssertEquals(false, serviceInfo.IsCostForSpotRate);

			serviceInfos = services.FindServices(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Labor).ToList();
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(isEnabled, serviceInfo.IsEnabled);
			AssertEquals("HR", serviceInfo.Unit);
			AssertEquals(false, serviceInfo.IsCostForSpotRate);

			serviceInfos = services.FindServices(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.CartageDemurrageTotal).ToList();
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(isEnabled, serviceInfo.IsEnabled);
			AssertEquals("HR", serviceInfo.Unit);
			AssertEquals(false, serviceInfo.IsCostForSpotRate);

			serviceInfos = services.FindServices(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Labor).ToList();
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(isEnabled, serviceInfo.IsEnabled);
			AssertEquals("HR", serviceInfo.Unit);
			AssertEquals(false, serviceInfo.IsCostForSpotRate);

			serviceInfos = services.FindServices(ChargeCodeGroupList.Codes.Destination, ChargeCodeSubGroupList.Storage).ToList();
			serviceInfo = serviceInfos.FirstOrDefault();
			AssertNotNull(serviceInfo);
			AssertEquals(isEnabled, serviceInfo.IsEnabled);
			AssertEquals("HR", serviceInfo.Unit);
			AssertEquals(false, serviceInfo.IsCostForSpotRate);
		}

		#endregion

		#region Shipment Penalty

		public void TestShipmentPenaltyServiceInfos_ServiceShouldBeDisabledAndShouldNotThrowExceptionWhenDurationIsEmptyOrInvalid()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = ContainerModes.FCL;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;

			var container = consol.Containers.AddNew();
			var penalty = shipment.PickupPenalties.AddNew();
			penalty.CPY_JC_Container = container.PK;
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
			penalty.CPY_Duration = ZDateTime.Empty;

			CombineAssertions(() =>
			{
				Assert("Pre-condition: CPY_Duration is empty", penalty.CPY_Duration.IsEmpty);
				AssertNoExceptionThrown("Should not throw any exception when CPY_Duration is empty", () => { AssertService(); });

				penalty.CPY_Duration = DateTime.MinValue;
				Assert("Pre-condition: CPY_Duration is invalid", !penalty.CPY_Duration.IsValid);
				AssertNoExceptionThrown("Should not throw any exception when CPY_Duration is invalid", () => { AssertService(); });
			});

			void AssertService()
			{
				AssertEquals("Pre-condition: DurationAsDays is zero", (ZByte)0, penalty.DurationAsDays);

				var services = shipment.GetJobServices();
				var serviceInfos = services.FindServices(ChargeCodeGroupList.Codes.Origin, ChargeCodeSubGroupList.Storage);
				var serviceInfo = serviceInfos.FirstOrDefault();

				AssertEquals("Pre-condition: Service duration should be zero.", 0.0, serviceInfo.ServiceDuration.TotalSeconds);
				AssertEquals("Service should be disabled.", false, serviceInfo.IsEnabled);
				AssertEquals(true, serviceInfo.UseTotalCostAndIgnoreRate);
			}
		}

		public void TestShipmentPenaltyServiceInfos_ForFCLSeaShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;

			AssertShipmentPenaltyServiceInfos(shipment, false, false, null);

			var container = consol.Containers.AddNew();
			AssertShipmentPenaltyServiceInfos(shipment, true, true, container);

			consol.JK_TransportMode = TransportModes.Air;
			AssertShipmentPenaltyServiceInfos(shipment, false, false, container);

			consol.JK_TransportMode = TransportModes.Sea;
			AssertShipmentPenaltyServiceInfos(shipment, true, true, container);

			shipment.JS_PackingMode = ContainerModes.LCL;
			AssertShipmentPenaltyServiceInfos(shipment, false, false, container);
		}

		public void TestShipmentPenaltyServiceInfos_ForBCNLeadShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_PackingMode = ContainerModes.BuyersConsol;
			shipment.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;

			AssertShipmentPenaltyServiceInfos(shipment, false, false, null);

			var container = consol.Containers.AddNew();
			AssertShipmentPenaltyServiceInfos(shipment, false, true, container);

			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			AssertShipmentPenaltyServiceInfos(shipment, false, false, container);

			shipment.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;
			AssertShipmentPenaltyServiceInfos(shipment, false, true, container);

			shipment.JS_PackingMode = ContainerModes.LCL;
			shipment.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;
			AssertShipmentPenaltyServiceInfos(shipment, false, false, container);
		}

		public void AssertShipmentPenaltyServiceInfos(CommonShipment shipment, bool isShipmentPickupPenaltyApplicable, bool isShipmentDeliveryPenaltyApplicable, CommonContainer container, bool isEnableShipmentPenalty = true)
		{
			AssertPenalty(shipment.PickupPenalties, ChargeCodeGroupList.Codes.Origin, isShipmentPickupPenaltyApplicable);
			AssertPenalty(shipment.DeliveryPenalties, ChargeCodeGroupList.Codes.Destination, isShipmentDeliveryPenaltyApplicable);

			void AssertPenalty(ShipmentContainerPenaltyCollection penaltyCollection, ZString chargeCodeGroup, bool isShipmentPenaltyApplicable)
			{
				penaltyCollection.DeleteAll();

				var assertServiceInfo = new AssertServiceInfo(chargeCodeGroup, isShipmentPenaltyApplicable);
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.Storage, 2, 20, ContainerPenaltyCreditorType.Codes.Transport, container);
				assertServiceInfo.IsCTOStorageServiceEnabled = false; // STO + TRS = Invalid combination
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				var containerPenalty = AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.Storage, 2, 20, ContainerPenaltyCreditorType.Codes.Carrier, container);
				assertServiceInfo.IsCarrierStorageServiceEnabled = isShipmentPenaltyApplicable; // STO + CAR = STC
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				containerPenalty.DurationAsDays = 0;
				assertServiceInfo.IsCarrierStorageServiceEnabled = false;
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				containerPenalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Hours;
				containerPenalty.CPY_Duration = TimeSpan.FromHours(1);
				assertServiceInfo.IsCarrierStorageServiceEnabled = isShipmentPenaltyApplicable;
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				containerPenalty.CPY_Duration = TimeSpan.FromHours(0);
				assertServiceInfo.IsCarrierStorageServiceEnabled = false;
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.Storage, 2, 20, ContainerPenaltyCreditorType.Codes.CTO, container);
				assertServiceInfo.IsCTOStorageServiceEnabled = isShipmentPenaltyApplicable; // STO + CTO = STG
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.Detention, 2, 20, ContainerPenaltyCreditorType.Codes.CTO, container);
				assertServiceInfo.IsDetentionServiceEnabled = false; // DET + CTO = Invalid combination
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.Detention, 2, 20, ContainerPenaltyCreditorType.Codes.Transport, container);
				assertServiceInfo.IsDetentionServiceEnabled = false; // DET + TRS = Invalid combination
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.Detention, 2, 20, ContainerPenaltyCreditorType.Codes.Carrier, container);
				assertServiceInfo.IsDetentionServiceEnabled = isShipmentPenaltyApplicable; // DET + CAR = DTN
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.TruckWaitTime, 2, 20, ContainerPenaltyCreditorType.Codes.CTO, container);
				assertServiceInfo.IsTruckWaitTimeServiceEnabled = false; // TWT + CTO = Invalid combination
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.TruckWaitTime, 2, 20, ContainerPenaltyCreditorType.Codes.Carrier, container);
				assertServiceInfo.IsTruckWaitTimeServiceEnabled = false; // TWT + CAR = Invalid combination
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);

				AttachPenaltyToShipment(penaltyCollection, ContainerPenaltyPenaltyType.Codes.TruckWaitTime, 2, 20, ContainerPenaltyCreditorType.Codes.Transport, container);
				assertServiceInfo.IsTruckWaitTimeServiceEnabled = isShipmentPenaltyApplicable; // TWT + TRS = DME
				AssertShipmentPenaltyServiceInfos(shipment, assertServiceInfo);
			}
		}

		static void AssertShipmentPenaltyServiceInfos(CommonShipment shipment, AssertServiceInfo assertServiceInfo)
		{
			var services = shipment.GetJobServices();
			AssertNotNull(services);

			CombineAssertions(() =>
			{
				AssertService(ChargeCodeSubGroupList.CarrierStorage, assertServiceInfo.IsCarrierStorageServiceEnabled, true);
				AssertService(ChargeCodeSubGroupList.Storage, assertServiceInfo.IsCTOStorageServiceEnabled, true);
				AssertService(ChargeCodeSubGroupList.ContainerDetention, assertServiceInfo.IsDetentionServiceEnabled, true);
				AssertService(ChargeCodeSubGroupList.Labor, assertServiceInfo.IsLabourServiceEnabled, false);
				AssertService(ChargeCodeSubGroupList.CartageDemurrageTotal, assertServiceInfo.IsTruckWaitTimeServiceEnabled, true);
			});

			void AssertService(ZString chargeCodeSubGroup, bool isEnabled, bool useTotalCostAndIgnoreRate)
			{
				var staticServicesThatShouldBeExcluded = new List<string>(new[] { "Pickup Demurrage", "Delivery Demurrage" });
				var serviceInfo =
					services
					.FindServices(assertServiceInfo.ChargeCodeGroup, chargeCodeSubGroup)
					.FirstOrDefault(s => !staticServicesThatShouldBeExcluded.Contains(s.ServiceDescription));

				var serviceName = $"{assertServiceInfo.ChargeCodeGroup}|{chargeCodeSubGroup}";

				if (isEnabled)
				{
					AssertNotNull($"{serviceName} service", serviceInfo);
					Assert($"{serviceName} service should be enabled.", serviceInfo.IsEnabled);
					AssertEquals(useTotalCostAndIgnoreRate, serviceInfo.UseTotalCostAndIgnoreRate);
				}
				else if (assertServiceInfo.IsShipmentPenaltyApplicable)
				{
					AssertNotNull($"{serviceName} service", serviceInfo);
					Assert($"{serviceName} service should be disabled.", !serviceInfo.IsEnabled);
					AssertEquals(useTotalCostAndIgnoreRate, serviceInfo.UseTotalCostAndIgnoreRate);
				}
				else if (serviceInfo != null)
				{
					Assert($"{serviceName} service should be disabled (not applicable).", !serviceInfo.IsEnabled);
				}
			}
		}

		ShipmentContainerPenalty AttachPenaltyToShipment(ShipmentContainerPenaltyCollection penaltyCollection, ZString penaltyType, ZByte duration, ZDecimal perUnitCost, ZString creditorType, CommonContainer container)
		{
			var penalty = penaltyCollection.AddNew();

			if (container != default)
			{
				penalty.CPY_JC_Container = container.PK;
			}

			penalty.CPY_PenaltyType = penaltyType;
			penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
			penalty.CPY_CreditorType = creditorType;
			penalty.FreeTimeAsDays = 0;
			penalty.DurationAsDays = duration;
			penalty.CPY_PerUnitCost = perUnitCost;
			return penalty;
		}

		class AssertServiceInfo
		{
			public AssertServiceInfo(ZString chargeCodeGroup, bool isShipmentPenaltyApplicable)
			{
				ChargeCodeGroup = chargeCodeGroup;
				IsShipmentPenaltyApplicable = isShipmentPenaltyApplicable;
			}
			public ZString ChargeCodeGroup { get; }
			public bool IsShipmentPenaltyApplicable { get; set; }
			public bool IsCarrierStorageServiceEnabled { get; set; }
			public bool IsCTOStorageServiceEnabled { get; set; }
			public bool IsDetentionServiceEnabled { get; set; }
			public bool IsLabourServiceEnabled { get; set; }
			public bool IsTruckWaitTimeServiceEnabled { get; set; }
		}

		#endregion
	}
}
