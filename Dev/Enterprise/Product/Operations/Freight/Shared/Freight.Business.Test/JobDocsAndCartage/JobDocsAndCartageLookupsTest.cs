using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public class JobDocsAndCartageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJP_ExportStatementList()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			MockJobDocsAndCartageParent parent = new MockJobDocsAndCartageParent(Factory);
			JobDocsAndCartage cartage = JobDocsAndCartage.New(parent);
			TestJobDocsAndCartageLookups lookups = new TestJobDocsAndCartageLookups(cartage);

			AssertEquals(1, lookups.JP_ExportStatementList.Count);
			AssertEquals("DEF", lookups.JP_ExportStatementList[0].Code);
			AssertEquals("Exporter Statements are defined in the System Registry located at Registry --> Freight --> Shipments --> Export Statements.", lookups.JP_ExportStatementList[0].Description);

			parent.SetShipment(shipment);
			CodeDescriptionPairList pairListToCheck = lookups.JP_ExportStatementList;
			CodeDescriptionPairList pairList = FreightDataRegistry.Instance.ExportStatementSettings.Value.GetExportStatementDescriptionPairListForCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertNotEquals(0, lookups.JP_ExportStatementList.Count);
			AssertEquals(pairList.Count, pairListToCheck.Count);
			foreach (CodeDescriptionPair pair in pairList)
			{
				AssertEquals(pair.Description, pairListToCheck.GetDescriptionFromCode(pair.Code));
			}
		}

		public void TestEquipmentLists_WithBuyersConsol()
		{
			MockJobDocsAndCartageParent parent = new MockJobDocsAndCartageParent(Factory);
			JobDocsAndCartage cartage = JobDocsAndCartage.New(parent);
			TestJobDocsAndCartageLookups lookups = new TestJobDocsAndCartageLookups(cartage);

			parent.ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			parent.TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Pickup should use LCLAIREquipmentNeededList for SEA", lookups.LCLAIREquipmentNeededList, lookups.PickupEquipmentNeededList);
			AssertEquals("Delivery should use FCLEquipmentNeededList for SEA", lookups.FCLEquipmentNeededList, lookups.DeliveryEquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("Pickup should use LCLAIREquipmentNeededList for RAI", lookups.LCLAIREquipmentNeededList, lookups.PickupEquipmentNeededList);
			AssertEquals("Delivery should use FCLEquipmentNeededList for RAI", lookups.FCLEquipmentNeededList, lookups.DeliveryEquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Pickup should use LCLAIREquipmentNeededList for ROA", lookups.LCLAIREquipmentNeededList, lookups.PickupEquipmentNeededList);
			AssertEquals("Delivery should use FCLEquipmentNeededList for ROA", lookups.FCLEquipmentNeededList, lookups.DeliveryEquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals("Pickup should use LCLAIREquipmentNeededList for Air/Sea", lookups.LCLAIREquipmentNeededList, lookups.PickupEquipmentNeededList);
			AssertEquals("Delivery should use FCLEquipmentNeededList for Air/Sea", lookups.FCLEquipmentNeededList, lookups.DeliveryEquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Pickup should use LCLAIREquipmentNeededList for AIR", lookups.LCLAIREquipmentNeededList, lookups.PickupEquipmentNeededList);
			AssertEquals("Delivery should use EquipmentNeededList for AIR", lookups.EquipmentNeededList, lookups.DeliveryEquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertEquals("Pickup should use LCLAIREquipmentNeededList for Sea/Air", lookups.LCLAIREquipmentNeededList, lookups.PickupEquipmentNeededList);
			AssertEquals("Delivery should use EquipmentNeededList for Sea/Air", lookups.EquipmentNeededList, lookups.DeliveryEquipmentNeededList);
		}

		public void TestFCLEquipmentNeededList()
		{
			MockJobDocsAndCartageParent parent = new MockJobDocsAndCartageParent(Factory);
			JobDocsAndCartage cartage = JobDocsAndCartage.New(parent);
			TestJobDocsAndCartageLookups lookups = new TestJobDocsAndCartageLookups(cartage);
			AssertNotNull(lookups.FCLEquipmentNeededList);
		}

		public void TestExportEquipmentNeededList()
		{
			var parent = new MockJobDocsAndCartageParent(Factory);
			var cartage = JobDocsAndCartage.New(parent);
			var lookups = new TestJobDocsAndCartageLookups(cartage);

			parent.IsImport = false;
			parent.IsExport = true;
			parent.TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.Sea;
			parent.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.FCLEquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.FCLEquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.LTL;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
		}

		public void TestImportEquipmentNeededList()
		{
			var parent = new MockJobDocsAndCartageParent(Factory);
			var cartage = JobDocsAndCartage.New(parent);
			var lookups = new TestJobDocsAndCartageLookups(cartage);

			parent.IsImport = true;
			parent.IsExport = false;
			parent.TransportMode = Core.Constants.ContainerModes.AIR;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.TransportMode = Core.Constants.TransportModes.Sea;
			parent.ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.FCLEquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.FCLEquipmentNeededList);

			MockIContainer container1 = new MockIContainer();
			parent.Containers.Add(container1);
			container1.ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			MockIContainer container2 = new MockIContainer();
			parent.Containers.Add(container2);
			container2.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.FCLEquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.FCLEquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);

			parent.ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals(lookups.DeliveryEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
			AssertEquals(lookups.PickupEquipmentNeededList, lookups.LCLAIREquipmentNeededList);
		}

		[ExpectNoExceptions]
		public void TestPickupEquipmentNeededListWhenCartageParentIsNull()
		{
			var cartageParent = new MockJobDocsAndCartageParent(Factory);
			cartageParent.ContainerMode = Core.Constants.ContainerModes.FCL;
			cartageParent.TransportMode = Core.Constants.TransportModes.Sea;

			var cartage = JobDocsAndCartage.New(cartageParent);
			var lookups = new TestJobDocsAndCartageLookups(cartage);

			cartageParent.Delete();

			AssertNotNull(lookups.PickupEquipmentNeededList);
			AssertContainsExactElementsInAnyOrder(lookups.FCLAndLCLAIREquipmentNeededList, lookups.PickupEquipmentNeededList);
		}

		[ExpectNoExceptions]
		public void TestDeliveryEquipmentNeededListWhenCartageParentIsNull()
		{
			var cartageParent = new MockJobDocsAndCartageParent(Factory);
			cartageParent.ContainerMode = Core.Constants.ContainerModes.FCL;
			cartageParent.TransportMode = Core.Constants.TransportModes.Sea;

			var cartage = JobDocsAndCartage.New(cartageParent);
			var lookups = new TestJobDocsAndCartageLookups(cartage);

			cartageParent.Delete();

			AssertNotNull(lookups.DeliveryEquipmentNeededList);
			AssertContainsExactElementsInAnyOrder(lookups.FCLAndLCLAIREquipmentNeededList, lookups.DeliveryEquipmentNeededList);
		}

		protected class TestJobDocsAndCartageLookups : JobDocsAndCartageLookups
		{
			public TestJobDocsAndCartageLookups(JobDocsAndCartage parent)
				: base(parent)
			{
			}

			public new FCLEquipmentNeededList FCLEquipmentNeededList
			{
				get { return base.FCLEquipmentNeededList; }
			}

			public new LCLAIREquipmentNeededList LCLAIREquipmentNeededList
			{
				get { return base.LCLAIREquipmentNeededList; }
			}

			public new CodeDescriptionPairList EquipmentNeededList
			{
				get { return base.EquipmentNeededList; }
			}

			public new CodeDescriptionPairList FCLAndLCLAIREquipmentNeededList => base.FCLAndLCLAIREquipmentNeededList;
		}
	}
}
