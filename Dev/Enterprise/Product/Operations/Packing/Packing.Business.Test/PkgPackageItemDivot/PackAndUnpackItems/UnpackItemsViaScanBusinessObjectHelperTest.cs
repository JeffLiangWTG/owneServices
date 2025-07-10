using System.Linq;
using System.Reflection;

namespace Enterprise.Packing.Business.Testing
{
	public class UnpackItemsViaScanBusinessObjectHelperTest : PackingTestCaseWithFactory
	{
		public void TestSelectPackableItemParentValidatesPackageQty()
		{
			Data.CreatePackingData();
			Data.DummyLine1.BarcodeTUNPackQty = 5m;
			Data.DummyLine2.BarcodeTUNPackQty = 5m;

			var tunMatch = new BarcodeMatch(true, "BOX", 5m);

			var dAbs = new[]
			{
				// pack 5 x TUN for Dummy1
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m),

				// pack 3 x TUN for Dummy2
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine2, 5m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine2, 5m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine2, 5m)
			}
			.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, tunMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, dAbs, isUnpackingQty: true);

			// reflect out the bizO helper rather than create a new one, otherwise we would have 2 instances with different current wrappers and validation cannot be tested
			var property = bizO.GetType().GetProperty("ScanHelper", BindingFlags.Instance | BindingFlags.NonPublic);
			var helper = (UnpackItemsViaScanBusinessObjectHelper)property.GetValue(bizO, null);
			AssertEquals("Precondition - We are particular about ensuring we get exactly what we want to test.", typeof(UnpackItemsViaScanBusinessObjectHelper), helper.GetType());
			AssertEquals("Precondition - Wrappers should be merged and made distinct by attributes.", 2, bizO.PackableItemParentsForBinding.Count);

			// set package qty to create to 4 and ensure no errors (no selection)
			bizO.PackageQtyToCreate = 4;
			AssertNoErrors(bizO.PackageQtyToCreateInfo);

			// select item2 and ensure error (as there are only 3 available)
			helper.SelectPackableItemParent(bizO.PackableItemParentsForBinding.FindByPackableItemParent(Data.DummyLine2));
			AssertHasErrors(bizO.PackageQtyToCreateInfo);

			// select item1 and ensure no error (as there are 5 available)
			helper.SelectPackableItemParent(bizO.PackableItemParentsForBinding.FindByPackableItemParent(Data.DummyLine1));
			AssertNoErrors(bizO.PackageQtyToCreateInfo);
		}
	}
}
