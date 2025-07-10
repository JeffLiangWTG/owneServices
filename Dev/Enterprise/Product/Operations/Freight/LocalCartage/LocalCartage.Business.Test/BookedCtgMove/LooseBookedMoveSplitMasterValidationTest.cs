using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.LocalCartage.Business.BookedCtgMove.Testing
{
	internal class LooseBookedMoveSplitMasterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidation()
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
			master.AddRemaining();
			LooseBookedMoveSplit split3 = master.Splits[2];
			AssertEquals("Split3: Should packs divide evenly", 0, split3.Packs);
			AssertEquals("Split3: Should weight divide evenly", 0m, split3.Weight);
			AssertEquals("Split3: Should volume divide evenly", 0m, split3.Volume);
			split3.Packs = 5;
			split3.Weight = 5;
			split3.Volume = 5;
			Assert(master.HasErrors());
			Assert(master.RemainingPacksInfo.HasErrors());
			Assert(master.RemainingWeightInfo.HasErrors());
			Assert(master.RemainingVolumeInfo.HasErrors());
			Assert(!master.RemainingPacksInfo.HasWarnings());
			Assert(!master.RemainingWeightInfo.HasWarnings());
			Assert(!master.RemainingVolumeInfo.HasWarnings());
			master.AllowDiscrepancy = true;
			Assert(!master.HasErrors());
			Assert(!master.RemainingPacksInfo.HasErrors());
			Assert(!master.RemainingWeightInfo.HasErrors());
			Assert(!master.RemainingVolumeInfo.HasErrors());
			Assert(master.RemainingPacksInfo.HasWarnings());
			Assert(master.RemainingWeightInfo.HasWarnings());
			Assert(master.RemainingVolumeInfo.HasWarnings());
		}
	}
}
