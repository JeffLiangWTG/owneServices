using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentProcessTaskCollection))]
	internal class AgencyShipmentProcessTaskCollectionTest : ProcessTaskCollectionTest<AgencyShipmentProcessTaskCollection>
	{
		#region OriginCountry / DestinationCountry
		public void TestOriginCountry()
		{
			Shipment.JS_RL_NKOrigin = "MYPKG";
			AssertEquals("MY", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals("MY", Collection.DestinationCountry);
		}

		#endregion
		#region IsCondition1or2Met - Common Items
		public void TestIsCondition1or2Met_ForImport()
		{
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.Import, ""));
			Shipment.JS_RL_NKOrigin = "MYPKG";
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals(true, Shipment.IsImport());
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Import));
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.Import, ""));
		}

		public void TestIsCondition1or2Met_ForExport()
		{
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.Export, ""));
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(true, Shipment.IsExport());
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Export));
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.Export, ""));
		}

		public void TestIsCondition1or2Met_ForDomestic()
		{
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Domestic));
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.Domestic, ""));
			Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Shipment.JS_RL_NKDestination = GlbBranch.CurrentBranch.GB_RL_NKHomePort.Substring(0, 2) + "XXX";
			AssertEquals(true, Shipment.IsDomestic());
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Domestic));
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.Domestic, ""));
		}

		#endregion
		#region IsCondition1Met
		public void TestIsCondition1Met_ForOriginDifferentFromFirstLoad()
		{
			Shipment.JS_NKLoadPort = "AUSYD";
			Shipment.JS_NKDischargePort = "MYPKG";
			Shipment.JS_RL_NKOrigin = "AUSYD";
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));
			Shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));
		}

		public void TestIsCondition1Met_ForDestinationDifferentFromFinalDischarge()
		{
			Shipment.JS_NKLoadPort = "AUSYD";
			Shipment.JS_NKDischargePort = "MYPKG";
			Shipment.JS_RL_NKDestination = "MYPKG";
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));
			Shipment.JS_RL_NKDestination = "USLAX";
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));
		}

		public void TestIsCondition2Met_ForConfirmed()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Confirmed));
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.OriginPickup;
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Confirmed));
		}

		public void TestIsCondition2Met_ForBooked()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Booked));
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.OriginPickup;
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.Booked));
		}

		public void TestIsCondition2Met_ForWaitListed()
		{
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			AssertEquals(true, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.WaitListed));
			Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.OriginPickup;
			AssertEquals(false, Collection.IsCondition1Met(AgencyShipmentWorkflowCondition1CodeList.Codes.WaitListed));
		}

		#endregion
		#region IsCondition2Met
		public void TestIsCondition2Met_ForFCL()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.FCL, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.FCL, ""));
		}

		public void TestIsCondition2Met_ForBLK()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.BLK, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.BLK, ""));
		}

		public void TestIsCondition2Met_ForBBK()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.BBK, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.BBK, ""));
		}

		public void TestIsCondition2Met_ForLQD()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.LQD, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.LQD, ""));
		}

		public void TestIsCondition2Met_ForROR()
		{
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals(true, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.ROR, ""));
			Shipment.JS_PackingMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(AgencyShipmentWorkflowCondition2CodeList.Codes.ROR, ""));
		}

		#endregion
		#region Implementation
		protected override AgencyShipmentProcessTaskCollection GetCollectionToTestCore()
		{
			return new AgencyShipmentProcessTaskCollection(Shipment);
		}

		AgencyShipment Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.NewWithValidTestData<AgencyShipment>());
			}
		}

		AgencyShipment shipment;
		#endregion
	}
}
