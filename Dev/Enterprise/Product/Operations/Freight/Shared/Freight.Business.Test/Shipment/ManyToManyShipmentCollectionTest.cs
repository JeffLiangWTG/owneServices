using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ManyToManyShipmentCollectionTest : BaseFreightTest
	{
		#region TestDetachingAShipmentFromAConsolShouldUnpackItFromAnyContainersOnTheConsol

		public void TestDetachingAShipmentFromAConsolShouldUnpackItFromAnyContainersOnTheConsol()
		{
			var shipment = Factory.New<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 1;

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			consol.Shipments.Add(shipment);

			Factory.Save();

			AssertEquals("precondition:", true, packLine.Containers.Contains(container));

			var factory2 = new BusinessObjectFactory();
			var consol2 = (CommonConsol)factory2.Load<Enterprise.Integration.Forwarding.IForwardingConsol>(consol.PK);
			consol2.Shipments.RemoveAll();

			var packLine2 = factory2.Load<PackLine>(packLine.PK);
			AssertEquals("the CommonShipment should no longer be packed in the container", false, packLine2.Containers.Contains(container.PK));
		}

		#endregion

		#region TestDefaultsForNewChild

		public void TestDefaultsForNewChild()
		{
			ZDateTime dateOfArrival = new ZDateTime(2005, 4, 14);
			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_Code = "SENDAGENT";
			sendingAgent.OH_FullName = "Sending Agent";
			sendingAgent.MainAddress.OA_Address1 = "Sending Address";

			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_Code = "RECVAGENT";
			receivingAgent.OH_FullName = "Receiving Agent";
			receivingAgent.MainAddress.OA_Address1 = "Receiving Address";

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Combination;
			consol.JK_MasterBillNum = "11111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "123";
			transport.JW_ETD = new ZDateTime(2005, 3, 20);
			transport.JW_ETA = dateOfArrival;

			Factory.Save();

			AirDefaultDelay.G1_DaysFromDestinationArrivalToClientDelivery = 4;

			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			CommonShipment newShipment1 = consol.Shipments.AddNew();
			AssertEquals("Default Transport Mode from Consol.", consol.JK_TransportMode, newShipment1.JS_TransportMode);
			AssertEquals("Default Container Mode from Consol.", consol.JK_ConsolMode, newShipment1.JS_PackingMode);
			AssertEquals("Do not default Origin from Consol Loading.", ZString.Empty, newShipment1.JS_RL_NKOrigin);
			AssertEquals("Default Destination from Consol Discharge.", transport.JW_RL_NKDiscPort, newShipment1.JS_RL_NKDestination);
			Assert("Direct consol should not default Consignor.", newShipment1.ConsignorPK.IsEmpty);
			Assert("Direct consol should not default Consignee.", newShipment1.ConsigneePK.IsEmpty);
			AssertEquals("CommonShipment ETA should default to ConsolETA + PortDeliveryTime", dateOfArrival.AddDays(3), newShipment1.JS_E_ARV);
			AssertEquals("CommonShipment Estimated delivery should default to ConsolETA + days until ETA + days until delivery", dateOfArrival.AddDays(7), newShipment1.DocsAndCartage.JP_EstimatedDelivery);

			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CommonShipment newShipment2 = consol.Shipments.AddNew();
			AssertEquals("Default Origin from Consol Loading.", transport.JW_RL_NKLoadPort, newShipment2.JS_RL_NKOrigin);
			AssertEquals("Do not default Destination from Consol Discharge.", ZString.Empty, newShipment2.JS_RL_NKDestination);
		}

		public void TestDefaultsForNewChild_PortsFromConsol()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Combination;
			consol.JK_MasterBillNum = "11111";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Factory.Save();

			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			CommonShipment newShipment = (CommonShipment)((IBindingList)consol.Shipments).AddNew();
			AssertEquals("Default Origin NOT set from Consol Loading.", ZString.Empty, newShipment.JS_RL_NKOrigin);
			AssertEquals("Default Destination NOT set from Consol Discharge.", ZString.Empty, newShipment.JS_RL_NKDestination);
		}

		#endregion

		#region TestShipmentETADefaultsToConsolETAPlusDeliveryTimeWhenAttached

		public void TestShipmentETADefaultsToConsolETAPlusDeliveryTimeWhenAttached()
		{
			ZDateTime dateOfArrival = new ZDateTime(2005, 4, 14);

			CommonShipment testShipment = Factory.New<CommonShipment>();
			testShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			testShipment.ConsigneePK = LocalConsignee.PK;
			testShipment.JS_RL_NKOrigin = "HKHKG";
			testShipment.JS_RL_NKDestination = "AUBNE";
			testShipment.JS_E_DEP = dateOfArrival.AddDays(-10);
			testShipment.JS_E_ARV = dateOfArrival.AddDays(-4);

			CommonShipment testShipment2 = Factory.New<CommonShipment>();
			testShipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			testShipment2.ConsigneePK = LocalConsignee.PK;
			testShipment2.JS_RL_NKOrigin = "HKHKG";
			testShipment2.JS_RL_NKDestination = "AUBNE";
			testShipment2.JS_E_DEP = dateOfArrival.AddDays(-10);
			testShipment2.JS_E_ARV = dateOfArrival.AddDays(-4);

			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Core.Constants.ContainerModes.Combination;
			consol1.JK_RL_NKLoadPort = "HKHKG";
			consol1.JK_RL_NKDischargePort = "AUMEL";

			Transport transport1 = consol1.Transports[0];
			transport1.JW_VoyageFlight = "123";
			consol1.JK_MasterBillNum = "11111";
			transport1.JW_Vessel = "vesselname";
			transport1.JW_ETD = dateOfArrival.AddDays(-9);
			transport1.JW_ETA = dateOfArrival;

			Factory.Save();

			consol1.Shipments.Add(testShipment);
			AssertEquals("ShipmentETA should be Consol ETA + PortDeliveryTime", dateOfArrival.AddDays(3), testShipment.JS_E_ARV);

			using (BusinessObjectUniversalCopyFactoryService.EnsureServiceIsSetUp(Factory))
			{
				consol1.Shipments.Add(testShipment2);
				AssertEquals("ShipmentETA should be original Shipment ETA", dateOfArrival.AddDays(-4), testShipment2.JS_E_ARV);
			}

			var consignee1 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee1.OH_IsConsignee = ZBool.True;

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee2.OH_IsConsignee = ZBool.True;

			var consol2 = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;

			var firstShipment = consol2.Shipments.AddNew();
			AssertEquals("Expect consignee of 1st CommonShipment to be empty", ZGuid.Empty, firstShipment.ConsigneePK);
			firstShipment.ConsigneePK = consignee1.PK;

			var secondShipment = consol2.Shipments.AddNew();
			AssertEquals("Expect that a subsequent CommonShipment with empty consignee should default to first shipment's consignee", firstShipment.ConsigneePK, secondShipment.ConsigneePK);

			var thirdShipment = Factory.NewWithValidTestData<CommonShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			thirdShipment.ConsigneePK = consignee2.PK;
			consol2.Shipments.Add(thirdShipment);
			AssertEquals("Expect that a subsequent shipment's consignee will remain unchanged if it wasn't initially empty", consignee2.PK, thirdShipment.ConsigneePK);

			var consol3 = Factory.NewWithValidTestData<CommonConsol>(TestBusinessObjectKind.MinimumRequiredToSave);
			consol3.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol3.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			firstShipment = consol3.Shipments.AddNew();
			AssertEquals("Expect consignee of 1st CommonShipment to be empty", ZGuid.Empty, firstShipment.ConsigneePK);
			firstShipment.ConsigneePK = consignee1.PK;
			secondShipment = consol3.Shipments.AddNew();
			AssertEquals("Expect non-BCN consol not to default CommonShipment consignees", ZGuid.Empty, secondShipment.ConsigneePK);
		}

		#endregion

		#region TestShipmentETDeliveryDefaultsToShipmentETAPlusDeliveryTimeWhenAttached

		public void TestShipmentETDeliveryDefaultsToShipmentETAPlusDeliveryTimeWhenAttached()
		{
			CreateDefaultDelay(Core.Constants.TransportModes.Air, "AUMEL", "AUBNE", 1, 2);

			ZDateTime dateOfArrival = new ZDateTime(2005, 4, 14);

			CommonShipment testShipment = Factory.New<CommonShipment>();
			testShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			testShipment.ConsigneePK = LocalConsignee.PK;
			testShipment.JS_RL_NKOrigin = "HKHKG";
			testShipment.JS_RL_NKDestination = "AUBNE";
			testShipment.JS_E_DEP = dateOfArrival.AddDays(-10);
			testShipment.JS_E_ARV = dateOfArrival.AddDays(-4);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Combination;
			consol.JK_RL_NKLoadPort = "HKHKG";
			consol.JK_RL_NKDischargePort = "AUMEL";

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "123";
			consol.JK_MasterBillNum = "11111";
			transport.JW_Vessel = "vesselname";
			transport.JW_ETD = dateOfArrival.AddDays(-9);
			transport.JW_ETA = dateOfArrival;

			Factory.Save();

			consol.Shipments.Add(testShipment);
			AssertEquals("Shipment.DocsAndCartage Est. Delivery should be CommonShipment ETA + PortDeliveryTime", testShipment.JS_E_ARV.AddDays(2), testShipment.DocsAndCartage.JP_EstimatedDelivery);
		}

		void CreateDefaultDelay(string freightMode, string dischargePort, string destinationPort, ZByte daysDelayFromArrivalToDeliver, ZByte daysFromDestinationArrivalToClientDelivery)
		{
			GlbPortDeliveryTime delay = Factory.New<GlbPortDeliveryTime>();
			delay.G1_FreightMode = freightMode;
			delay.G1_RL_NKDischargePort = dischargePort;
			delay.G1_RL_NKDestinationPort = destinationPort;
			delay.G1_DaysDelayFromArrivalToDeliver = daysDelayFromArrivalToDeliver;
			delay.G1_DaysFromDestinationArrivalToClientDelivery = daysFromDestinationArrivalToClientDelivery;
		}

		#endregion

		#region TestContainerPackPivotsAreDeleted

		public void TestContainerPackPivotsAreDeleted()
		{
			//Create a Consol with a Container
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1234567";
			//Create a Shipmnet with an Outer Packline and attach the Consol
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = 1;
			shipment.JS_ActualWeight = 100;
			shipment.Consols.Add(consol);

			AssertEquals("Container should be allocated to Packline", 1, shipment.OuterPackLines[0].Containers.Count);

			Factory.Save();
			ZQuery filter = new ZQuery(JobContainerPackPivotSchema.J6_JC, container.PK);
			filter.AddToFilter(JobContainerPackPivotSchema.J6_JL, shipment.OuterPackLines[0].PK);
			BusinessObject[] pivots = Factory.Load(typeof(JobContainerPackPivot), filter);
			AssertEquals("ContainerPackPivot not found", 1, pivots.Length);

			consol.Shipments.Remove(shipment);
			Factory.Save();
			AssertEquals("Container should be unallcoated from Packline", 0, shipment.OuterPackLines[0].Containers.Count);
			pivots = Factory.Load(typeof(JobContainerPackPivot), filter);
			AssertEquals("ContainerPackPivot should be deleted", 0, pivots.Length);
		}

		#endregion

		#region TestAdjustShipmentDatesOnAdded

		public void TestAdjustShipmentDatesOnAdded()
		{
			ZDateTime today = ZDateTime.Today;
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_E_DEP = today.AddDays(1);
			shipment.JS_E_ARV = today.AddDays(5);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_ETD = today;
			transport.JW_ETA = today.AddDays(7);

			consol.Shipments.Add(shipment);

			AssertEquals("CommonShipment ETD should be adjusted to match consol", transport.JW_ETD, shipment.JS_E_DEP);
			AssertEquals("CommonShipment ETA should be adjusted to match consol if no default port delivery time.", transport.JW_ETA, shipment.JS_E_ARV);
		}

		public void TestAdjustShipmentDatesOnAddedSuppressed()
		{
			ZDateTime today = ZDateTime.Today;
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.JS_E_DEP = today.AddDays(1);
			shipment.JS_E_ARV = today.AddDays(5);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_ETD = today;
			transport.JW_ETA = today.AddDays(7);

			shipment.IsSuppressedETAETDOnAttachToConsol = true;
			consol.Shipments.Add(shipment);

			AssertEquals(false, shipment.IsSuppressedETAETDOnAttachToConsol);
			AssertNotEquals("CommonShipment ETD should NOT be adjusted to match consol", transport.JW_ETD, shipment.JS_E_DEP);
			AssertNotEquals("CommonShipment ETA should NOT be adjusted to match consol if no default port delivery time.", transport.JW_ETA, shipment.JS_E_ARV);
		}

		#endregion

		#region Test Consol Totals Refresh

		#region Totals Helper Methods

		CommonShipment GetShipmentForTotals(decimal weightInKg, decimal volumeInM3, int outerPacks)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_ActualWeight = weightInKg;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_ActualVolume = volumeInM3;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment.JS_OuterPacks = outerPacks;

			return shipment;
		}

		CommonConsol GetConsolForTotals()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			return consol;
		}

		Dictionary<string, Dictionary<BusinessObject, EventHandler>> GetBizObjectHandlersDictionary(BusinessObject bizObject)
		{
			var bizObjectPropertyInfos = bizObject.GetType().GetProperties();
			var bizObjectZPropertyInfo = (from pi in bizObjectPropertyInfos
										  where pi.PropertyType == typeof(ZPropertyInfo)
										  select pi).FirstOrDefault();
			var bizObjectZPropertyInfoInstance = bizObjectZPropertyInfo.GetValue(bizObject, null);
			var propertyInfoStorage = bizObjectZPropertyInfoInstance.GetType().GetProperty("PropertyInfoStorage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bizObjectZPropertyInfoInstance, null);
			var valueChangedDictionary = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(propertyInfoStorage, null);

			return valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(valueChangedDictionary) as Dictionary<string, Dictionary<BusinessObject, EventHandler>>;
		}

		#endregion

		public void TestParentConsolRefreshTotals()
		{
			List<string> sink = new List<string>();

			EventHandler registerEvent = (s, e) =>
				{
					InfoEventArgs infoArgs = (InfoEventArgs)e;
					sink.Add(infoArgs.Info.Name);
				};

			var consol = GetConsolForTotals();

			consol.JK_TotalShipmentWeightInfo.ValueChanged += registerEvent;
			consol.JK_TotalShipmentWeightUnitInfo.ValueChanged += registerEvent;
			consol.JK_TotalShipmentVolumeInfo.ValueChanged += registerEvent;
			consol.JK_TotalShipmentVolumeUnitInfo.ValueChanged += registerEvent;
			consol.JK_TotalShipmentLoadingMetersInfo.ValueChanged += registerEvent;
			consol.JK_TotalShipmentChargeableInfo.ValueChanged += registerEvent;
			consol.JK_TotalShipmentChargeableUnitInfo.ValueChanged += registerEvent;
			consol.JK_ConsolChargeableInfo.ValueChanged += registerEvent;
			consol.JK_CorrectedConsolVolumeInfo.ValueChanged += registerEvent;
			consol.JK_CorrectedConsolWeightInfo.ValueChanged += registerEvent;
			consol.JK_TotalShipmentQuantityInfo.ValueChanged += registerEvent;
			consol.JK_CorrectedConsolWeightUnitInfo.ValueChanged += registerEvent;
			consol.JK_CorrectedConsolVolumeUnitInfo.ValueChanged += registerEvent;

			var shipment = GetShipmentForTotals(200m, 1m, 5);

			consol.Shipments.Add(shipment);
			AssertContainsExactElementsInAnyOrder(new string[] { "JK_TotalShipmentWeight", "JK_TotalShipmentWeightUnit",
				"JK_TotalShipmentVolume", "JK_TotalShipmentVolumeUnit", "JK_TotalShipmentLoadingMeters", "JK_TotalShipmentChargeable", "JK_TotalShipmentChargeableUnit",
				"JK_ConsolChargeable", "JK_CorrectedConsolVolume", "JK_CorrectedConsolWeight", "JK_TotalShipmentQuantity", "JK_CorrectedConsolWeightUnit", "JK_CorrectedConsolVolumeUnit",
				"JK_TotalShipmentWeight", "JK_TotalShipmentVolume", "JK_TotalShipmentLoadingMeters", "JK_TotalShipmentChargeable", "JK_TotalShipmentQuantity" }, sink);
			sink.Clear();

			shipment.JS_ActualWeight = 400m;
			AssertContainsExactElementsInAnyOrder(new string[] { "JK_TotalShipmentWeight", "JK_TotalShipmentChargeable", "JK_ConsolChargeable", "JK_CorrectedConsolWeight" }, sink);
			sink.Clear();

			shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;
			AssertContainsExactElementsInAnyOrder(new string[] { "JK_TotalShipmentWeight", "JK_TotalShipmentWeightUnit", "JK_TotalShipmentChargeable", "JK_TotalShipmentChargeableUnit", "JK_ConsolChargeable", "JK_CorrectedConsolWeight", "JK_CorrectedConsolWeightUnit" }, sink);
			sink.Clear();

			shipment.JS_ActualVolume = 5m;
			AssertContainsExactElementsInAnyOrder(new string[] { "JK_TotalShipmentVolume", "JK_TotalShipmentChargeable", "JK_ConsolChargeable", "JK_CorrectedConsolVolume" }, sink);
			sink.Clear();

			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicYards;
			AssertContainsExactElementsInAnyOrder(new string[] { "JK_TotalShipmentVolume", "JK_TotalShipmentVolumeUnit", "JK_TotalShipmentChargeable", "JK_TotalShipmentChargeableUnit", "JK_ConsolChargeable", "JK_CorrectedConsolVolume", "JK_CorrectedConsolVolumeUnit" }, sink);
			sink.Clear();

			shipment.JS_LoadingMeters = 10m;
			AssertContainsExactElementsInAnyOrder(new string[] { "JK_TotalShipmentLoadingMeters" }, sink);
			sink.Clear();

			shipment.JS_OuterPacks = 22;
			AssertCollectionContains("JK_TotalShipmentQuantity", sink);
		}

		public void TestParentConsolTotalsEventsAddRemove()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			Dictionary<string, Dictionary<BusinessObject, EventHandler>> handlers = null;

			AssertNoExceptionThrown("Prequisite failed, possibly caused by change by architectural change in CargoWise.EntityFramework e.g. renamed property etc. Have a look at ZPropertyInfo class -> EventHandler ValueChanged and try to determine the change. " +
				"GetBizObjectHandlersDictionary(BusinessObject bizObject) method uses reflection to retrieve shipment event handlers",
				() => handlers = GetBizObjectHandlersDictionary(shipment));

			var consol = Factory.NewWithValidTestData<CommonConsol>();

			consol.Shipments.Add(shipment);

			var afterAttachShipmentHandlers = (from dicEntryKey in handlers.Keys
											   from innerDicEntryKey in handlers[dicEntryKey].Keys
											   where innerDicEntryKey == shipment
											   select new { PropertyName = dicEntryKey, Handler = handlers[dicEntryKey][shipment] }).ToList();

			var attachedShipmentPropertyNames = new List<string>();

			foreach (var entry in afterAttachShipmentHandlers)
			{
				foreach (var method in entry.Handler.GetInvocationList())
				{
					if (method.Target == consol.Shipments)
					{
						attachedShipmentPropertyNames.Add(entry.PropertyName);
					}
				}
			}

			var expectedAttachedShipmentPropertyNames = new List<string>()
			{
				"JS_ActualWeight", "JS_UnitOfWeight", "JS_ActualVolume", "JS_UnitOfVolume",
				"JS_LoadingMeters", "JS_ActualChargeable", "JS_ChargeableUnit", "JS_OuterPacks"
			};

			AssertContainsExactElementsInAnyOrder(String.Format("Some event handlers for the following shipment properties have not been attached in ManyToManyShipmentCollection: {0}",
				String.Join(" ,", expectedAttachedShipmentPropertyNames.ToArray())),
				expectedAttachedShipmentPropertyNames,
				attachedShipmentPropertyNames);

			consol.Shipments.Remove(shipment);

			var afterDetachShipmentHandlers = (from dicEntryKey in handlers.Keys
											   from innerDicEntryKey in handlers[dicEntryKey].Keys
											   where innerDicEntryKey == shipment
											   select new { PropertyName = dicEntryKey, Handler = handlers[dicEntryKey][shipment] }).ToList();

			var unDetachedShipmentPropertyNames = new List<string>();

			foreach (var entry in afterDetachShipmentHandlers)
			{
				foreach (var method in entry.Handler.GetInvocationList())
				{
					if (method.Target == consol.Shipments)
					{
						unDetachedShipmentPropertyNames.Add(entry.PropertyName);
					}
				}
			}

			AssertEquals(String.Format("Event handlers for the following shipment properties have not been removed from ManyToManyShipmentCollection: {0}",
				String.Join(" ,", unDetachedShipmentPropertyNames.ToArray())), 0, unDetachedShipmentPropertyNames.Count);
		}

		#endregion

		#region TestRemoveNewShipmentFromRelationship

		public void TestRemoveNewShipmentFromRelationship()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			var newShipment = Factory.New<CommonShipment>();
			consol.Shipments.Add(newShipment);
			ChildEditableService.SetState(Factory, Integration.ChildEditableServiceStates.Shipment);

			AssertEquals("Precondition: There is one console in the new shipment", 1, newShipment.Consols.Count);
			Assert("Precondition: the new shipment should not be in the database", !newShipment.IsInDatabase);

			consol.Shipments.Remove(newShipment);

			AssertEquals("the new shipment should be removed from the collection ", 0, consol.Shipments.Count);
			AssertEquals("ParentConsol should be removed from the consoles of the new shipment", 0, newShipment.Consols.Count);
		}

		#endregion

		#region Duplicate Loading And Discharge

		public void TestDuplicateLoadingAndDischarge_WhenParentConsolAttachedToBuyersConsolLeadShipment()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_UniqueConsignRef = "C0000001";

			var subShipment = Factory.NewWithValidTestData<CommonShipment>();
			subShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_UniqueConsignRef = "S000001";
			subShipment.Consols.Add(consol1);

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			consol2.JK_UniqueConsignRef = "C0000002";

			var masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_UniqueConsignRef = "S000002";
			masterShipment.Consols.Add(consol2);
			masterShipment.CoLoadShipments.Add(subShipment);

			subShipment.Consols.Add(consol2);

			consol2.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(true, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Loading."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			consol2.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(false, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Loading."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "CNSHA";

			consol2.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(true, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Discharge."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			consol2.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(false, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Discharge."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			consol2.JK_RL_NKLoadPort = "CNNGB";
			consol2.JK_RL_NKDischargePort = "CNSHA";
			consol2.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.FTL;
			consol2.IsDomesticFreight = true;

			consol2.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(false, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Discharge."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "AUMEL";
			consol2.JK_TransportMode = Core.Constants.TransportModes.Rail;
			consol2.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			consol2.IsDomesticFreight = true;

			consol2.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(false, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Discharge."));
		}

		public void TestDuplicateLoadingAndDischarge_WhenParentConsolIsNotAttachedToBuyersConsolLeadShipment()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_UniqueConsignRef = "C0000001";

			var subShipment = Factory.NewWithValidTestData<CommonShipment>();
			subShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_UniqueConsignRef = "S000001";
			subShipment.Consols.Add(consol1);

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			consol2.JK_UniqueConsignRef = "C0000002";

			var masterShipment = Factory.NewWithValidTestData<CommonShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_UniqueConsignRef = "S000002";
			masterShipment.Consols.Add(consol2);
			masterShipment.CoLoadShipments.Add(subShipment);

			subShipment.Consols.Add(consol2);

			consol1.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(true, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Loading."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			consol1.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(false, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Loading."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "CNSHA";

			consol1.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(true, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Discharge."));

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			consol1.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(false, subShipment.RowErrors.Contains("Shipment already on a Consol with same Port of Discharge."));
		}

		public void TestDuplicateLoadingAndDischarge_WhenMultipleTransportLegs()
		{
			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_UniqueConsignRef = "S000001";

			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_UniqueConsignRef = "C0000001";
			shipment1.Consols.Add(consol1);

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "CNSHA";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			consol2.JK_UniqueConsignRef = "C0000002";
			shipment1.Consols.Add(consol2);

			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol1, shipment1, false, false);
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol2, shipment1, false, false);

			var existingLeg1 = (Transport)consol1.Transports.ToList()[0];
			existingLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transport1 = consol1.Transports.AddNew();
			transport1.JW_ParentGUID = shipment1.PK;
			transport1.JW_ParentType = Core.Constants.ShipmentTypes.StandardHouse;
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "CNSHA";

			// AUSYD -> NZAKL, NZAKL -> CNSHA && CNSHA -> NZAKL
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol1, shipment1, false, true);
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol2, shipment1, false, true);

			var existingLeg2 = (Transport)consol2.Transports.ToList()[0];
			existingLeg2.JW_RL_NKLoadPort = "AUSYD";

			var transport2 = consol2.Transports.AddNew();
			transport2.JW_ParentGUID = shipment1.PK;
			transport2.JW_ParentType = Core.Constants.ShipmentTypes.StandardHouse;
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			// AUSYD -> NZAKL, NZAKL -> CNSHA && CNSHA -> AUSYD, AUSYD -> NZAKL
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol1, shipment1, true, false);
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol2, shipment1, false, true);

			consol1.Transports.Remove(transport1);
			existingLeg1.JW_RL_NKDiscPort = "CNSHA";

			// AUSYD -> CNSHA && CNSHA -> AUSYD, AUSYD -> NZAKL
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol1, shipment1, true, false);
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol2, shipment1, true, false);
		}

		public void TestDuplicateLoadingAndDischarge_WhenParentConsolChanges()
		{
			var shipment1 = Factory.NewWithValidTestData<CommonShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment1.JS_UniqueConsignRef = "S000001";

			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_UniqueConsignRef = "C0000001";
			shipment1.Consols.Add(consol1);

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "CNSHA";
			consol2.JK_RL_NKDischargePort = "NZAKL";
			consol2.JK_UniqueConsignRef = "C0000002";
			shipment1.Consols.Add(consol2);

			var existingLeg1 = (Transport)consol1.Transports.ToList()[0];
			existingLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transport1 = consol1.Transports.AddNew();
			transport1.JW_ParentGUID = shipment1.PK;
			transport1.JW_ParentType = Core.Constants.ShipmentTypes.StandardHouse;
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "CNSHA";

			// AUSYD -> NZAKL, NZAKL -> CNSHA && CNSHA -> NZAKL
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol1, shipment1, false, true);
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol2, shipment1, false, true);

			shipment1.Consols.Remove(consol1);
			shipment1.Consols.Add(consol1);

			// Changing order of consol attachment should not affect errors
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol1, shipment1, false, true);
			CheckConsolShipmentsForDuplicateLoadingAndDischargeError(consol2, shipment1, false, true);
		}

		void CheckConsolShipmentsForDuplicateLoadingAndDischargeError(CommonConsol consol, CommonShipment shipment, bool containsLoad, bool containsDischarge)
		{
			consol.Shipments.CheckAllShipmentsForDuplicateLoadingAndDischarge();
			AssertEquals(containsLoad, shipment.RowErrors.Contains("Shipment already on a Consol with same Port of Loading."));
			AssertEquals(containsDischarge, shipment.RowErrors.Contains("Shipment already on a Consol with same Port of Discharge."));
		}

		public void TestDuplicateLoadingAndDischarge_WhenParentConsolIsDetached()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_UniqueConsignRef = "S000001";

			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "CNSHA";
			consol1.JK_UniqueConsignRef = "C0000001";
			shipment.Consols.Add(consol1);

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "CNSHA";
			consol2.JK_UniqueConsignRef = "C0000002";
			shipment.Consols.Add(consol2);

			Factory.Save();

			consol1.RunPreSaveValidation();
			const string duplicateLoadPortMessage = "Shipment already on a Consol with same Port of Loading.";
			const string duplicateDiscPortMessage = "Shipment already on a Consol with same Port of Discharge.";
			AssertHasRowError(shipment, duplicateLoadPortMessage);
			AssertHasRowError(shipment, duplicateDiscPortMessage);

			shipment.Consols.Remove(consol1);
			AssertNoRowError(shipment, duplicateLoadPortMessage);
			AssertNoRowError(shipment, duplicateDiscPortMessage);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			SeaDefaultDelay = Factory.New<GlbPortDeliveryTime>();
			SeaDefaultDelay.G1_FreightMode = Core.Constants.TransportModes.Sea;
			SeaDefaultDelay.G1_RL_NKDischargePort = "AUMEL";
			SeaDefaultDelay.G1_RL_NKDestinationPort = "AUBNE";
			SeaDefaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;

			AirDefaultDelay = Factory.New<GlbPortDeliveryTime>();
			AirDefaultDelay.G1_FreightMode = Core.Constants.TransportModes.Air;
			AirDefaultDelay.G1_RL_NKDischargePort = "USLAX";
			AirDefaultDelay.G1_RL_NKDestinationPort = "USLAX";
			AirDefaultDelay.G1_DaysDelayFromArrivalToDeliver = 3;
		}

		GlbPortDeliveryTime AirDefaultDelay;
		GlbPortDeliveryTime SeaDefaultDelay;

		#endregion
	}
}
