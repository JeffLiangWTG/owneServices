using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.BookedCtgMove.Testing
{
	[TestedType(typeof(LooseBookedMoveSplitMaster))]
	internal class LooseBookedMoveSplitMasterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSplitter()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedPackCount = 20;
			move.EW_BookedWeight = 40m;
			move.EW_BookedVolume = 160m;
			move.EW_BookedHeight = 2m;
			move.EW_BookedWidth = 2m;
			move.EW_BookedLength = 2m;
			move.EW_E2PickupAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTOSYD", "Wharf", "2000", "Sydney", "AUSYD", false).PK;
			move.EW_E2WaitPointAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNESYD", "Consignee", "2000", "Sydney", "AUSYD", true).PK;
			move.EW_E2DeliveryAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageYard, "CYDSYD", "ContYard", "2000", "Sydney", "AUSYD", false).PK;
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			AssertEquals("Should default 2 splits", 2, master.Splits.Count);
			LooseBookedMoveSplit split1 = master.Splits[0];
			LooseBookedMoveSplit split2 = master.Splits[1];
			AssertEquals("Split1: Should packs divide evenly", 10, split1.Packs);
			AssertEquals("Split1: Should weight divide evenly", 20m, split1.Weight);
			AssertEquals("Split1: Should volume divide evenly", 80m, split1.Volume);
			AssertEquals("Split2: Should packs divide evenly", 10, split2.Packs);
			AssertEquals("Split2: Should weight divide evenly", 20m, split2.Weight);
			AssertEquals("Split2: Should volume divide evenly", 80m, split2.Volume);
			split2.Packs = 5;
			AssertEquals("Split1: Packs should have been affected", 15, split1.Packs);
			AssertEquals("Split1: Weight should have been affected", 30m, split1.Weight);
			AssertEquals("Split1: Volume should have been affected", 120m, split1.Volume);
			AssertEquals("Split2: Packs should have been set", 5, split2.Packs);
			AssertEquals("Split2: Weight should have been affected", 10m, split2.Weight);
			AssertEquals("Split2: Volume should have been affected", 40m, split2.Volume);
			LooseBookedMoveSplit split3 = master.Splits.AddNew();
			AssertEquals("Split3: Packs should be 0", 0, split3.Packs);
			AssertEquals("Split3: Weight should be 0", 0m, split3.Weight);
			AssertEquals("Split3: Volume should be 0", 0m, split3.Volume);
			split3.Packs = 5;
			AssertEquals("Split1: Packs should NOT have been affected", 15, split1.Packs);
			AssertEquals("Split1: Weight should NOT have been affected", 30m, split1.Weight);
			AssertEquals("Split1: Volume should NOT have been affected", 120m, split1.Volume);
			AssertEquals("Split2: Packs should NOT have been affected", 5, split2.Packs);
			AssertEquals("Split2: Weight should have been affected", 10m, split2.Weight);
			AssertEquals("Split2: Volume should have been affected", 40m, split2.Volume);
			AssertEquals("Split3: Packs was set", 5, split3.Packs);
			AssertEquals("Split3: Weight should be 5/20 of total weight", 10m, split3.Weight);
			AssertEquals("Split3: Volume should be 5/20 of total volume", 40m, split3.Volume);
			master.DivideEqually();
			AssertEquals("Split1: Packs split evenly", 6, split1.Packs);
			AssertEquals("Split1: Weight split evenly", 12m, split1.Weight);
			AssertEquals("Split1: Volume split evenly", 48m, split1.Volume);
			AssertEquals("Split2: Packs split evenly", 7, split2.Packs);
			AssertEquals("Split2: Weight split evenly", 14m, split2.Weight);
			AssertEquals("Split2: Volume split evenly", 56m, split2.Volume);
			AssertEquals("Split3: Packs split evenly", 7, split3.Packs);
			AssertEquals("Split3: Weight split evenly", 14m, split3.Weight);
			AssertEquals("Split3: Volume split evenly", 56m, split3.Volume);
			Assert(!master.HasErrors);
			UNDGDataItem dgItem = move.UNDGs.AddNew();
			dgItem.DI_DGFlashPoint = 1m;
			dgItem.DI_DG = Substance.PK;
			dgItem.DI_OC_DGContact = Factory.New<OrgContact>().PK;
			move.EW_RequestedPickupTimeStart = ZDateTime.Now.AddDays(1);
			move.EW_RequestedPickupTimeEnd = ZDateTime.Now.AddDays(2);
			move.EW_RequestedDeliveryTimeStart = ZDateTime.Now.AddDays(3);
			move.EW_RequestedDeliveryTimeEnd = ZDateTime.Now.AddDays(4);
			move.EW_DropMode = BindToLists.GetCachedLists(Factory).DropModes(false)[0].Code;
			master.Done();
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves[1];
			CommonBookedCtgMove move3 = cartage.LooseBookedMoves[2];
			AssertEquals("Split1 Packs", 6, move.EW_BookedPackCount);
			AssertEquals("Split1 Weight", 12m, move.EW_BookedWeight);
			AssertEquals("Split1 Volume", 48m, move.EW_BookedVolume);
			AssertEquals("Height should NOT be reset on the Master move", 2m, move.EW_BookedHeight);
			AssertEquals("Width should NOT be reset on the Master move", 2m, move.EW_BookedWidth);
			AssertEquals("Length should NOT be reset on the Master move", 2m, move.EW_BookedLength);
			AssertEquals("Split2 Packs", 7, move2.EW_BookedPackCount);
			AssertEquals("Split2 Weight", 14m, move2.EW_BookedWeight);
			AssertEquals("Split2 Volume", 56m, move2.EW_BookedVolume);
			AssertEquals("Height should stay as 0", 0m, move2.EW_BookedHeight);
			AssertEquals("Width should stay as 0", 0m, move2.EW_BookedWidth);
			AssertEquals("Length should stay as 0", 0m, move2.EW_BookedLength);
			AssertEquals("FlashPoint copied from master move", move.UNDGs[0].DI_DGFlashPoint, move2.UNDGs[0].DI_DGFlashPoint);
			AssertEquals("DG copied from master move", move.UNDGs[0].DI_DG, move2.UNDGs[0].DI_DG);
			AssertEquals("Contact copied from master move", move.UNDGs[0].DI_OC_DGContact, move2.UNDGs[0].DI_OC_DGContact);
			AssertEquals("PStart copied from master move", move.EW_RequestedPickupTimeStart, move2.EW_RequestedPickupTimeStart);
			AssertEquals("PEnd copied from master move", move.EW_RequestedPickupTimeEnd, move2.EW_RequestedPickupTimeEnd);
			AssertEquals("DStart copied from master move", move.EW_RequestedDeliveryTimeStart, move2.EW_RequestedDeliveryTimeStart);
			AssertEquals("DEnd copied from master move", move.EW_RequestedDeliveryTimeEnd, move2.EW_RequestedDeliveryTimeEnd);
			AssertEquals("DropMode copied from master move", move.EW_DropMode, move2.EW_DropMode);
			AssertEquals("Pickup copied from master move", move.EW_E2PickupAddressID, move2.EW_E2PickupAddressID);
			AssertEquals("Wait copied from master move", move.EW_E2WaitPointAddressID, move2.EW_E2WaitPointAddressID);
			AssertEquals("Delivery copied from master move", move.EW_E2DeliveryAddressID, move2.EW_E2DeliveryAddressID);
			AssertEquals("Split3 Packs", 7, move3.EW_BookedPackCount);
			AssertEquals("Split3 Weight", 14m, move3.EW_BookedWeight);
			AssertEquals("Split3 Volume", 56m, move3.EW_BookedVolume);
			AssertEquals("Height should stay as 0", 0m, move3.EW_BookedHeight);
			AssertEquals("Width should stay as 0", 0m, move3.EW_BookedWidth);
			AssertEquals("Length should stay as 0", 0m, move3.EW_BookedLength);
			AssertEquals("FlashPoint copied from master move", move.UNDGs[0].DI_DGFlashPoint, move3.UNDGs[0].DI_DGFlashPoint);
			AssertEquals("DG copied from master move", move.UNDGs[0].DI_DG, move3.UNDGs[0].DI_DG);
			AssertEquals("Contact copied from master move", move.UNDGs[0].DI_OC_DGContact, move3.UNDGs[0].DI_OC_DGContact);
			AssertEquals("PStart copied from master move", move.EW_RequestedPickupTimeStart, move3.EW_RequestedPickupTimeStart);
			AssertEquals("PEnd copied from master move", move.EW_RequestedPickupTimeEnd, move3.EW_RequestedPickupTimeEnd);
			AssertEquals("DStart copied from master move", move.EW_RequestedDeliveryTimeStart, move3.EW_RequestedDeliveryTimeStart);
			AssertEquals("DEnd copied from master move", move.EW_RequestedDeliveryTimeEnd, move3.EW_RequestedDeliveryTimeEnd);
			AssertEquals("DropMode copied from master move", move.EW_DropMode, move3.EW_DropMode);
			AssertEquals("Pickup copied from master move", move.EW_E2PickupAddressID, move3.EW_E2PickupAddressID);
			AssertEquals("Wait copied from master move", move.EW_E2WaitPointAddressID, move3.EW_E2WaitPointAddressID);
			AssertEquals("Delivery copied from master move", move.EW_E2DeliveryAddressID, move3.EW_E2DeliveryAddressID);
		}

		public void TestDeletingASplit()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedPackCount = 20;
			move.EW_BookedWeight = 40m;
			move.EW_BookedVolume = 8m;
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			AssertEquals("Should default 2 splits", 2, master.Splits.Count);
			LooseBookedMoveSplit split1 = master.Splits[0];
			LooseBookedMoveSplit split2 = master.Splits[1];
			AssertEquals("Split1: Should packs divide evenly", 10, split1.Packs);
			AssertEquals("Split1: Should weight divide evenly", 20m, split1.Weight);
			AssertEquals("Split1: Should volume divide evenly", 4m, split1.Volume);
			AssertEquals("Split2: Should packs divide evenly", 10, split2.Packs);
			AssertEquals("Split2: Should weight divide evenly", 20m, split2.Weight);
			AssertEquals("Split2: Should volume divide evenly", 4m, split2.Volume);
			split2.Delete();
			AssertEquals("Split1: Should packs divide evenly", 20, split1.Packs);
			AssertEquals("Split1: Should weight divide evenly", 40m, split1.Weight);
			AssertEquals("Split1: Should volume divide evenly", 8m, split1.Volume);
		}

		public void TestGroup()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedPackCount = 2;
			move.EW_BookedWeight = 5m;
			move.EW_BookedVolume = 2m;
			move.EW_BookedLength = 1;
			move.EW_BookedWidth = 1;
			move.EW_BookedHeight = 1;
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			move2.EW_BookedPackCount = 3;
			move2.EW_BookedWeight = 5m;
			move2.EW_BookedVolume = 24m;
			move2.EW_BookedLength = 2;
			move2.EW_BookedWidth = 2;
			move2.EW_BookedHeight = 2;
			LooseBookedMoveSplitMaster.Group(new CommonBookedCtgMove[] { move, move2 });
			Assert("Move2 deleted", move2.IsDeleted);
			AssertEquals("Master Move packs", 5, move.EW_BookedPackCount);
			AssertEquals("Master Move Weight", 10m, move.EW_BookedWeight);
			AssertEquals("Master Move Volume", 26m, move.EW_BookedVolume);
			AssertEquals("Master Move Length - Currently just takes the first", 1m, move.EW_BookedLength);
			AssertEquals("Master Move Width - Currently just takes the first", 1m, move.EW_BookedWidth);
			AssertEquals("Master Move Height - Currently just takes the first", 1m, move.EW_BookedHeight);
		}

		public void TestCopyLegs()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedPackCount = 20;
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();
			leg1.JU_PlannedPickupTime = ZDateTime.Now.AddDays(1);
			leg1.JU_EstimatedDeliveryTime = ZDateTime.Now.AddDays(2);
			leg1.JU_E2PickupAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTOSYD", "Wharf", "2000", "Sydney", "AUSYD", false).PK;
			leg1.JU_E2WaitPointAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNESYD", "Consignee", "2000", "Sydney", "AUSYD", true).PK;
			leg1.JU_E2DeliveryAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageYard, "CYDSYD", "ContYard", "2000", "Sydney", "AUSYD", false).PK;
			leg1.QuickRQTruck = Factory.New<RefEquipment>().PK;
			leg1.QuickOHTransportCompany = Factory.New<OrgHeader>().PK;
			leg1.QuickGSDriver = "AAA";
			leg2.JU_PlannedPickupTime = ZDateTime.Now.AddDays(3);
			leg2.JU_EstimatedDeliveryTime = ZDateTime.Now.AddDays(4);
			leg2.JU_E2PickupAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTOSYD2", "Wharf2", "2000", "Sydney", "AUSYD", false).PK;
			leg2.JU_E2WaitPointAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNESYD2", "Consignee2", "2000", "Sydney", "AUSYD", true).PK;
			leg2.JU_E2DeliveryAddressID = new LocalCartageTestHelper(Factory).CreateJobDocAddress(cartage, DocAddressType.LocalCartageYard, "CYDSYD2", "ContYard2", "2000", "Sydney", "AUSYD", false).PK;
			leg2.JU_EY_RunSheet = Factory.New<CommonWorkSheet>().PK;
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			AssertEquals("Should default 2 splits", 2, master.Splits.Count);
			LooseBookedMoveSplit split1 = master.Splits[0];
			LooseBookedMoveSplit split2 = master.Splits[1];
			AssertEquals("Split1: Should packs divide evenly", 10, split1.Packs);
			master.Done();
			CommonBookedCtgMove newMove = cartage.LooseBookedMoves[1];
			AssertEquals("newMove should have 2 Cartage Legs", 2, newMove.CartageLegs.Count);
			newMove.CartageLegs.ApplySort(JobContainerLegsSchema.Constants.JU_DisplayOrder, System.ComponentModel.ListSortDirection.Ascending);
			CommonCartageLeg newLeg1 = newMove.CartageLegs[0];
			CommonCartageLeg newLeg2 = newMove.CartageLegs[1];
			AssertEquals("newLeg1: JU_PlannedPickupTime", leg1.JU_PlannedPickupTime, newLeg1.JU_PlannedPickupTime);
			AssertEquals("newLeg1: JU_EstimatedDeliveryTime", leg1.JU_EstimatedDeliveryTime, newLeg1.JU_EstimatedDeliveryTime);
			AssertEquals("newLeg1: JU_E2PickupAddressID", leg1.JU_E2PickupAddressID, newLeg1.JU_E2PickupAddressID);
			AssertEquals("newLeg1: JU_E2WaitPointAddressID", leg1.JU_E2WaitPointAddressID, newLeg1.JU_E2WaitPointAddressID);
			AssertEquals("newLeg1: JU_E2DeliveryAddressID", leg1.JU_E2DeliveryAddressID, newLeg1.JU_E2DeliveryAddressID);
			AssertEquals("newLeg1: QuickRQTruck", leg1.QuickRQTruck, newLeg1.QuickRQTruck);
			AssertEquals("newLeg1: QuickOHTransportCompany", leg1.QuickOHTransportCompany, newLeg1.QuickOHTransportCompany);
			AssertEquals("newLeg1: QuickGSDriver", leg1.QuickGSDriver, newLeg1.QuickGSDriver);
			AssertEquals("newLeg2: JU_PlannedPickupTime", leg2.JU_PlannedPickupTime, newLeg2.JU_PlannedPickupTime);
			AssertEquals("newLeg2: JU_EstimatedDeliveryTime", leg2.JU_EstimatedDeliveryTime, newLeg2.JU_EstimatedDeliveryTime);
			AssertEquals("newLeg2: JU_E2PickupAddressID", leg2.JU_E2PickupAddressID, newLeg2.JU_E2PickupAddressID);
			AssertEquals("newLeg2: JU_E2WaitPointAddressID", leg2.JU_E2WaitPointAddressID, newLeg2.JU_E2WaitPointAddressID);
			AssertEquals("newLeg2: JU_E2DeliveryAddressID", leg2.JU_E2DeliveryAddressID, newLeg2.JU_E2DeliveryAddressID);
			AssertEquals("newLeg2: JU_E2DeliveryAddressID", leg2.JU_EY_RunSheet, newLeg2.JU_EY_RunSheet);
		}

		public void TestRemaining()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedPackCount = 20;
			move.EW_BookedWeight = 40m;
			move.EW_BookedVolume = 8m;
			LooseBookedMoveSplitMaster master = new LooseBookedMoveSplitMaster(move);
			AssertEquals("Should default 2 splits", 2, master.Splits.Count);
			LooseBookedMoveSplit split1 = master.Splits[0];
			LooseBookedMoveSplit split2 = master.Splits[1];
			AssertEquals("Split1: Should packs divide evenly", 10, split1.Packs);
			AssertEquals("Split1: Should weight divide evenly", 20m, split1.Weight);
			AssertEquals("Split1: Should volume divide evenly", 4m, split1.Volume);
			AssertEquals("Split2: Should packs divide evenly", 10, split2.Packs);
			AssertEquals("Split2: Should weight divide evenly", 20m, split2.Weight);
			AssertEquals("Split2: Should volume divide evenly", 4m, split2.Volume);
			AssertEquals(0, master.RemainingPacks);
			AssertEquals(0m, master.RemainingWeight);
			AssertEquals(0m, master.RemainingVolume);
			master.AddRemaining();
			LooseBookedMoveSplit split3 = master.Splits[2];
			AssertEquals("Split3: Should packs divide evenly", 0, split3.Packs);
			AssertEquals("Split3: Should weight divide evenly", 0m, split3.Weight);
			AssertEquals("Split3: Should volume divide evenly", 0m, split3.Volume);
			AssertEquals(0, master.RemainingPacks);
			AssertEquals(0m, master.RemainingWeight);
			AssertEquals(0m, master.RemainingVolume);
			split2.Packs = 1;
			split2.Weight = 1m;
			split2.Volume = 1m;
			AssertEquals(9, master.RemainingPacks);
			AssertEquals(19m, master.RemainingWeight);
			AssertEquals(3m, master.RemainingVolume);
		}

		protected UNDGSubstance Substance
		{
			get
			{
				if (substance == null)
				{
					substance = Factory.New<UNDGSubstance>();
					substance.DG_Class = "4.2";
					substance.DG_UNNO = "SUBS";
					substance.DG_Variant = "b";
					substance.DG_Variation = "METAL ARYLS, WATER-REACTIVE, N.O.S.";
					substance.DG_PSN = "METAL ARYLS, WATER-REACTIVE, N.O.S.";
					Factory.Save();
				}

				return substance;
			}
		}

		UNDGSubstance substance;

		protected override BusinessObject GetNewBusinessObject()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move = cartage.LooseBookedMoves.AddNew();
			return new LooseBookedMoveSplitMaster(move);
		}
	}
}
