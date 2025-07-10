using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonWorkSheetRatingAdapterTest : TestCaseWithFactory
	{
		public void TestPrecondition()
		{
			var workSheet = Factory.New<CommonWorkSheet>();
			var adapter = new CommonWorkSheetRatingAdapter(workSheet);
			AssertEquals("Freight Mode", FreightMode.UKN, adapter.FreightMode);
			AssertNotNull(adapter.JobDatesProvider);
		}

		public void TestStatusInformation()
		{
			var container = Factory.New<CommonContainer>();
			// CASE 1: Precondition
			var workSheet1 = Factory.New<CommonWorkSheet>();
			var adapter1 = new CommonWorkSheetRatingAdapter(workSheet1);
			AssertNotNull(adapter1);
			AssertEquals("Precondition: Status Information (Can Execute)", true, adapter1.StatusInformation.CanExecute);
			AssertEquals("Precondition: Status Information (Message)", ZString.Empty, adapter1.StatusInformation.Message);
			// CASE 2: All legs use container
			var leg21 = Helper.CreateCartageLeg(true);
			var leg22 = Helper.CreateCartageLeg(true);
			var workSheet2 = Factory.New<CommonWorkSheet>();
			workSheet2.CartageLegs.AddRange(new[] { leg21, leg22 });
			var adapter2 = new CommonWorkSheetRatingAdapter(workSheet2);
			AssertNotNull(adapter2);
			AssertEquals("CASE 2 (Can Execute)", true, adapter2.StatusInformation.CanExecute);
			AssertEquals("CASE 2 (Message)", ZString.Empty, adapter2.StatusInformation.Message);
			// CASE 3: All legs use no container (loose)
			var leg31 = Helper.CreateCartageLeg(false);
			var leg32 = Helper.CreateCartageLeg(false);
			var workSheet3 = Factory.New<CommonWorkSheet>();
			workSheet3.CartageLegs.AddRange(new[] { leg31, leg32 });
			var adapter3 = new CommonWorkSheetRatingAdapter(workSheet3);
			AssertNotNull(adapter3);
			AssertEquals("CASE 3 (Can Execute)", true, adapter3.StatusInformation.CanExecute);
			AssertEquals("CASE 3 (Message)", ZString.Empty, adapter3.StatusInformation.Message);
			// CASE 4: All legs use container and no container (mixed)
			var leg41 = Helper.CreateCartageLeg(true);
			var leg42 = Helper.CreateCartageLeg(false);
			var workSheet4 = Factory.New<CommonWorkSheet>();
			workSheet4.CartageLegs.AddRange(new[] { leg41, leg42 });
			var adapter4 = new CommonWorkSheetRatingAdapter(workSheet4);
			AssertNotNull(adapter4);
			AssertEquals("CASE 4 (Can Execute)", false, adapter4.StatusInformation.CanExecute);
			AssertEquals("CASE 4 (Message)", "Costing cannot be run on a run sheet that contains Containerized and Loose Transport Legs. Please create separate run sheets.", adapter4.StatusInformation.Message);
		}

		public void TestAdapterTypeAndID()
		{
			var workSheet1 = Factory.New<CommonWorkSheet>();
			var adapter = new CommonWorkSheetRatingAdapter(workSheet1);
			AssertEquals(AdapterType.RunSheet, adapter.AdapterType);
			AssertEquals(workSheet1.EY_RunSheetNumber, adapter.OperationalJobCode);
		}

		public CommonWorkSheetRatingAdapterTest()
		{
			Helper = new TestCommonWorkSheetHelper(Factory);
		}

		readonly TestCommonWorkSheetHelper Helper;
	}
}
