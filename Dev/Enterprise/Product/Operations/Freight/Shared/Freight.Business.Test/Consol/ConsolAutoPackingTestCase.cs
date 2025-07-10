using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public class ConsolAutoPackingTestCase : BaseFreightTest
	{
		public void TestPack()
		{
			consol.AutomaticallyUpdatePackLineContainers = true;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			PackLine pack = shipment.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("1 pack line", 1, shipment.OuterPackLines.Count);
			if (container.PackLines.Count == 0)
			{
				AssertEquals("1 unpacked, 0 packed", 1, consol.UnAllocatedPackLines.Count);
				AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
				AssertEquals("no changes", false, consol.HasChanges);

				container.AddPackLine(pack);

				AssertEquals("changed by pack", true, consol.HasChanges);
			}

			AssertEquals("0 unpacked, 1 packed", 0, consol.UnAllocatedPackLines.Count);
			AssertEquals("0 unpacked, 1 packed", 1, container.PackLines.Count);
		}

		public void TestPack_DoNotPackIntoEmptyContainer()
		{
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = Helper.SydLaxSailing.PK;

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var container = consol.Containers.AddNew();
			container.JC_RC = refContainer.PK;

			var emptyContainer = consol.Containers.AddNew();
			emptyContainer.JC_RC = refContainer.PK;
			emptyContainer.JC_IsEmptyContainer = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;

			var pack1 = Factory.New<PackLine>();
			pack1.JL_ActualVolume = 15;
			pack1.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(pack1);

			var pack2 = Factory.New<PackLine>();
			pack2.JL_ActualVolume = 15;
			pack2.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(pack2);

			var pack3 = Factory.New<PackLine>();
			pack3.JL_ActualVolume = 15;
			pack3.JL_FreightMode = FreightConstants.OuterPackType;
			shipment.OuterPackLines.Add(pack3);

			AssertEquals("Packed into non-empty container", container.PK, pack1.Containers[0].PK);
			AssertEquals("Packed into non-empty container", container.PK, pack2.Containers[0].PK);
			AssertEquals("Can't pack into empty container, so packed into full container", container.PK, pack3.Containers[0].PK);
		}

		public void TestUnpack()
		{
			consol.AutomaticallyUpdatePackLineContainers = true;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			CommonContainer container = consol.Containers.AddNew();
			container.JC_JK = consol.PK;
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			PackLine pack = shipment.OuterPackLines.AddNew();
			Factory.Save();

			if (container.PackLines.Count == 0)
			{
				AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
				AssertEquals("1 unpacked, 0 packed", 1, consol.UnAllocatedPackLines.Count);
				AssertEquals("no changes", false, consol.HasChanges);

				container.AddPackLine(pack);

				AssertEquals("changed by pack", true, container.PackLines.HasChanges);
			}

			AssertEquals("0 unpacked, 1 packed", 0, consol.UnAllocatedPackLines.Count);
			AssertEquals("0 unpacked, 1 packed", 1, container.PackLines.Count);

			Factory.Save();
			AssertEquals("just saved", false, consol.HasChanges);
			container.RemovePackLine(pack);

			AssertEquals("1 unpacked, 0 packed", 1, consol.UnAllocatedPackLines.Count);
			AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
			AssertEquals("changed by unpack", true, container.PackLines.HasChanges);
		}

		public void TestAllocatingPacklinesGetRemovedFromUnAllocatedPackLineView()
		{
			consol.AutomaticallyUpdatePackLineContainers = true;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			CommonContainer container = consol.Containers.AddNew();
			container.JC_RC = RC_40GP_PK;
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			PackLine pack = shipment.OuterPackLines.AddNew();
			pack.JL_PackageCount = 5;
			Factory.Save();

			if (container.PackLines.Count == 0)
			{
				AssertEquals("1 unpacked, 0 packed", 1, consol.UnAllocatedPackLines.Count);
				AssertEquals("1 unpacked, 0 packed", 0, container.PackLines.Count);
				AssertEquals("no changes", false, consol.HasChanges);
			}
			else
			{
				container.RemovePackLine(pack);
			}

			consol.AllocateShipment(shipment);
			AssertEquals("0 unpacked, 1 packed", 0, consol.UnAllocatedPackLines.Count);
			AssertEquals("0 unpacked, 1 packed", 1, container.PackLines.Count);
		}

		public void TestAllocatePackLinesDoesNotAllocateLinesWithZeroPackCount()
		{
			consol.AutomaticallyUpdatePackLineContainers = true;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = Helper.SydLaxSailing.PK;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_PackageCount = 5;
			var pack2 = shipment.OuterPackLines.AddNew();
			pack2.JL_PackageCount = 3;
			var pack3 = shipment.OuterPackLines.AddNew();
			pack3.JL_PackageCount = 0;
			var container = consol.Containers.AddNew();
			container.JC_RC = RC_40GP_PK;

			consol.AllocateShipment(shipment);
			Assert("0 pack qty line is not allocated", consol.UnAllocatedPackLines.Contains(pack3));
			Assert("Other lines are allocated", container.PackLines.Contains(pack1));
			Assert("Other lines are allocated", container.PackLines.Contains(pack2));
		}

		public void TestAutomaticallyUpdatePackLineContainers()
		{
			FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var consol = Factory.New<CommonConsol>();
			AssertEquals(false, consol.AutomaticallyUpdatePackLineContainers);
			consol.AutomaticallyUpdatePackLineContainers = true;
			AssertEquals(true, consol.AutomaticallyUpdatePackLineContainers);

			FreightConfigurationRegistry.Instance.AutoPackFreightContainers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol1 = Factory.New<CommonConsol>();
			AssertEquals(true, consol1.AutomaticallyUpdatePackLineContainers);
			consol1.AutomaticallyUpdatePackLineContainers = false;
			AssertEquals(false, consol1.AutomaticallyUpdatePackLineContainers);

			var consol2 = Factory.New<CommonConsol>();
			AssertEquals(true, consol2.AutomaticallyUpdatePackLineContainers);
			using (consol2.DisableAutoUpdatePackLineContainers())
			{
				AssertEquals(false, consol2.AutomaticallyUpdatePackLineContainers);
			}
			AssertEquals(true, consol2.AutomaticallyUpdatePackLineContainers);
		}

		public void TestRelatedAndUnAllocatedPackLinesChangeWhenRelationShipmentsReduced()
		{
			var masterShipment = Factory.New<CommonShipment>();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var coloadShipmentA = masterShipment.CoLoadShipments.AddNew();
			var coloadShipmentB = masterShipment.CoLoadShipments.AddNew();

			coloadShipmentA.OuterPackLines.AddNew();
			coloadShipmentB.OuterPackLines.AddNew();

			var consol = Factory.New<CommonConsol>();

			consol.Containers.RemoveAll();
			consol.Shipments.Add(masterShipment);

			Factory.Save();

			AssertEquals("Consol should has three shipments!", 3, consol.Shipments.Count);
			AssertEquals("There are two related packlines in consol.", 2, consol.RelatedPackLines.Count);
			AssertEquals("Because consol doesn't have container, so there are two UnAllocated packlines in consol.", 2, consol.UnAllocatedPackLines.Count);

			var newFactory = NewFactory();

			coloadShipmentA = newFactory.Load<CommonShipment>(coloadShipmentA.PK);
			coloadShipmentB = newFactory.Load<CommonShipment>(coloadShipmentB.PK);

			coloadShipmentA.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			coloadShipmentB.JS_JS_ColoadMasterShipment = ZGuid.Empty;

			coloadShipmentA.Consols.RemoveAll();
			coloadShipmentB.Consols.RemoveAll();

			newFactory.Save();

			AssertEquals("Consol should only have the master shipment!", 1, consol.Shipments.Count);
			AssertEquals("There are not related packline in consol.", 0, consol.RelatedPackLines.Count);
			AssertEquals("Because the relation shipment remove all coload shipments, so there are not UnAllocated packline in consol.", 0, consol.UnAllocatedPackLines.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new SailingsForTestClasses(Factory);
			consol = CreateConsol();
		}

		protected virtual CommonConsol CreateConsol()
		{
			return Factory.New<CommonConsol>();
		}

		#endregion

		CommonConsol consol;
		SailingsForTestClasses Helper;
	}
}
