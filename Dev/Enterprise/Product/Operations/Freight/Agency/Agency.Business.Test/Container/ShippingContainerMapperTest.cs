using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ShippingContainerMapperTest : TestCaseWithFactory
	{
		public void TestExcessBooking()
		{
			RefContainer gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer bc1 = AddBooked(shipment, gp20, 3);
			AgencyShipmentContainer rc1 = AddReal(shipment, gp20, "TEST4100029");
			AgencyShipmentContainer rc2 = AddReal(shipment, gp20, "TEST4100034");
			IDictionary<ZGuid, ZGuid[]> map = ShippingContainerMapper.GetBookedRealContainerMap(shipment);
			AssertEquals("map.Count", 1, map.Count);
			AssertContainsExactElementsInAnyOrder("map[bc1.PK]", new ZGuid[] { rc1.PK, rc2.PK }, map[bc1.PK]);
		}

		public void TestExcessReal()
		{
			RefContainer gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer bc1 = AddBooked(shipment, gp20, 2);
			AgencyShipmentContainer rc1 = AddReal(shipment, gp20, "TEST4100013");
			AgencyShipmentContainer rc2 = AddReal(shipment, gp20, "TEST4100029");
			AgencyShipmentContainer rc3 = AddReal(shipment, gp20, "TEST4100034");
			IDictionary<ZGuid, ZGuid[]> map = ShippingContainerMapper.GetBookedRealContainerMap(shipment);
			AssertEquals("map.Count", 2, map.Count);
			ZGuid[] real1 = map[bc1.PK];
			AssertEquals("map[bc1.PK].Length", 2, real1.Length);
			AssertNotEquals("map[bc1.PK][0] != map[bc1.PK][1]", real1[0], real1[1]);
			AssertCollectionContains("map[bc1.PK][0]", real1[0], new ZGuid[] { rc1.PK, rc2.PK, rc3.PK });
			AssertCollectionContains("map[bc1.PK][1]", real1[1], new ZGuid[] { rc1.PK, rc2.PK, rc3.PK });
			ZGuid[] real2 = map[ZGuid.Empty];
			AssertEquals("map[ZGuid.Empty].Length", 1, real2.Length);
			AssertNotEquals("map[ZGuid.Empty][0] != map[bc1.PK][0]", real2[0], real1[0]);
			AssertNotEquals("map[ZGuid.Empty][0] != map[bc1.PK][1]", real2[0], real1[1]);
			AssertCollectionContains("map[ZGuid.Empty]", real2[0], new ZGuid[] { rc1.PK, rc2.PK, rc3.PK });
		}

		public void TestMapping()
		{
			RefContainer gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainer gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer bc1 = AddBooked(shipment, gp20, "TEST4100013");
			AgencyShipmentContainer bc2 = AddBooked(shipment, gp20, "TEST4100029");
			AgencyShipmentContainer bc3 = AddBooked(shipment, gp20, 3);
			AgencyShipmentContainer bc4 = AddBooked(shipment, gp40, 1);
			AgencyShipmentContainer rc1 = AddReal(shipment, gp20, "TEST4100029");
			AgencyShipmentContainer rc2 = AddReal(shipment, gp20, "TEST4100034");
			AgencyShipmentContainer rc3 = AddReal(shipment, gp20, "TEST4100040");
			AgencyShipmentContainer rc4 = AddReal(shipment, gp40, "TEST4100056");
			IDictionary<ZGuid, ZGuid[]> map = ShippingContainerMapper.GetBookedRealContainerMap(shipment);
			AssertEquals("map.Count", 4, map.Count);
			AssertContainsExactElementsInAnyOrder("map[bc1.PK]", System.Array.Empty<ZGuid>(), map[bc1.PK]);
			AssertContainsExactElementsInAnyOrder("map[bc2.PK]", new ZGuid[] { rc1.PK }, map[bc2.PK]);
			AssertContainsExactElementsInAnyOrder("map[bc3.PK]", new ZGuid[] { rc2.PK, rc3.PK }, map[bc3.PK]);
			AssertContainsExactElementsInAnyOrder("map[bc4.PK]", new ZGuid[] { rc4.PK }, map[bc4.PK]);
		}

		#region Implementation
		static AgencyShipmentContainer AddReal(AgencyShipment shipment, RefContainer type, string containerNum)
		{
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_RC = type.PK;
			container.JC_ContainerNum = containerNum;
			container.JC_ContainerCount = 1;
			return container;
		}

		static AgencyShipmentContainer AddBooked(AgencyShipment shipment, RefContainer type, string containerNum)
		{
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_RC = type.PK;
			container.JC_ContainerNum = containerNum;
			container.JC_ContainerCount = 1;
			return container;
		}

		static AgencyShipmentContainer AddBooked(AgencyShipment shipment, RefContainer type, short count)
		{
			AgencyShipmentContainer container = shipment.BookedContainers.AddNew();
			container.JC_RC = type.PK;
			container.JC_ContainerNum = "";
			container.JC_ContainerCount = count;
			return container;
		}
		#endregion
	}
}
