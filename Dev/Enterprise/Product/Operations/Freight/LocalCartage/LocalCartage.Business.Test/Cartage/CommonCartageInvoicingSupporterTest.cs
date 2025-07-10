using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageInvoicingSupporter))]
	public class CommonCartageInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CommonCartage>();
		}

		public void TestVoyageVesselOrFlightDate()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.Vessel = "OTELLO";
			cartage.VoyageFlight = "009";
			var supporter = new CommonCartageInvoicingSupporter(cartage);
			AssertEquals("009/OTELLO", supporter.VoyageVesselOrFlightDate);
		}

		public void TestIncludeInConsolCosting()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var supporter = new CommonCartageInvoicingSupporter(cartage);
			// CASE 1: Cartage is NOT cancelled.
			AssertEquals(true, supporter.IncludeInConsolCosting(false));
			AssertEquals(true, supporter.IncludeInConsolCosting(true));
			// CASE 2: Cartage is cancelled.
			cartage.IsCancelled = true;
			AssertEquals(false, supporter.IncludeInConsolCosting(false));
			AssertEquals(false, supporter.IncludeInConsolCosting(true));
		}

		public override void TestOuterPackTotal()
		{
			var helper = new LocalCartageTestHelper(Factory);
			var cartage = helper.CreateCartage(Constants.CartageJobType.NEW_AirImport, 2);
			helper.SetupBookedMove(cartage.LooseBookedMoves[0], 10, Constants.PkgUnit.Bag, 11m, Constants.Weight.Kilograms, 0, Constants.Volume.CubicMetres);
			helper.SetupBookedMove(cartage.LooseBookedMoves[1], 23, Constants.PkgUnit.Bag, 24m, Constants.Weight.Kilograms, 0, Constants.Volume.CubicMetres);
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			var supporter = new CommonCartageInvoicingSupporter(cartage);
			AssertEquals(33, supporter.OuterPackTotal);
			helper.SetupBookedMove(cartage.LooseBookedMoves[1], 5, Constants.PkgUnit.Pallet, 14m, Constants.Weight.Kilograms, 0, Constants.Volume.CubicMetres);
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertEquals(15, supporter.OuterPackTotal);
		}
	}
}
