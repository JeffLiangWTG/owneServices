using Enterprise.Core;

namespace Enterprise.Packing.Business.Testing
{
	public class PackableItemParentWrapperValidationTest : PackingBusinessObjectValidationTestCase
	{
		#region TestValidateProposedPackQty

		public void TestValidateProposedPackQty()
		{
			Data.CreatePackingData();

			var wrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			AssertNoErrors(wrapper.ProposedPackQtyInfo);

			wrapper.ProposedPackQty = 101;
			AssertHasError(wrapper.ProposedPackQtyInfo, "Cannot Pack 101x TV as there are only 100 available.");

			Data.DummyLine1.TotalQty = 1;
			wrapper.ProposedPackQty = 2;
			AssertHasError(wrapper.ProposedPackQtyInfo, "Cannot Pack 2x TV as there is only 1 available.");
			Data.DummyLine1.TotalQty = 100; // cleanup;

			wrapper.ProposedPackQty = 100;
			AssertNoErrors(wrapper.ProposedPackQtyInfo);

			wrapper.ProposedPackQty = -1;
			AssertHasError(wrapper.ProposedPackQtyInfo, "Quantity to pack cannot be less than 0.");

			// test too many packages warning

			wrapper.PackageTypeToCreateDescription = "Box";
			wrapper.PackageQtyToCreate = 12;
			wrapper.ProposedPackQty = 0;
			AssertNoWarnings("Proposed Pack Qty is empty, no warning should be added.", wrapper.ProposedPackQtyInfo);

			wrapper.PackageTypeToCreateDescription = "";
			wrapper.ProposedPackQty = 10;
			AssertNoWarnings("Package Type is empty, no warning should be added.", wrapper.ProposedPackQtyInfo);
			wrapper.PackageTypeToCreateDescription = "Box"; // cleanup

			wrapper.PackageQtyToCreate = 12;
			wrapper.ProposedPackQty = 10;
			AssertHasWarning(wrapper.ProposedPackQtyInfo, "There are 12 Boxes but only 10 TV(s). 2 Boxes will not contain a TV.");
		}

		#endregion

		#region TestValidateProposedRemoveQty

		public void TestValidateProposedRemoveQty()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var packedItem = package.Pack_ForTesting(Data.DummyLine1, 10m);

			var wrapper = new PackableItemParentWrapper(Factory, packedItem.AsGroup(), Data.Dummy);
			AssertNoErrors(wrapper.ProposedRemoveQtyInfo);

			wrapper.ProposedRemoveQty = 101;
			AssertHasError(wrapper.ProposedRemoveQtyInfo, "There are only 10 items packed.");

			wrapper.ProposedRemoveQty = 10;
			AssertNoErrors(wrapper.ProposedRemoveQtyInfo);

			wrapper.ProposedRemoveQty = -1;
			AssertHasError(wrapper.ProposedRemoveQtyInfo, "Quantity to remove cannot be less than 0.");
		}

		#endregion

		#region TestValidateUnpackedWeight

		public void TestValidateUnpackedWeight()
		{
			Data.CreatePackingData();

			Data.DummyLine1.WeightPerUnit = 0m;
			Data.DummyLine1.WeightUQ = "";

			Data.DummyLine2.WeightPerUnit = 2m;
			Data.DummyLine2.WeightUQ = "";

			Data.DummyLine3.WeightPerUnit = 2m;
			Data.DummyLine3.WeightUQ = Constants.Weight.Kilograms;

			var package = Data.PackageJob.Packages.AddNew();
			package.Pack(Data.DummyLine1, 10m);
			package.Pack(Data.DummyLine2, 10m);
			package.Pack(Data.DummyLine3, 10m);

			var collection = new PackableItemParentWrapperCollection(Factory, new PackableItemParentWrapper[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper, Data.DummyLine3Wrapper });
			var wrapper1 = collection.FindByPackableItemParent(Data.DummyLine1);
			var wrapper2 = collection.FindByPackableItemParent(Data.DummyLine2);
			var wrapper3 = collection.FindByPackableItemParent(Data.DummyLine3);
			AssertNoWarnings("Precondition", wrapper1.UnpackedWeightInfo);
			AssertNoWarnings("Precondition", wrapper2.UnpackedWeightInfo);
			AssertNoWarnings("Precondition", wrapper3.UnpackedWeightInfo);

			// accessing the property should run the validation
			var tmp1 = wrapper1.UnpackedWeight;
			var tmp2 = wrapper2.UnpackedWeight;
			var tmp3 = wrapper3.UnpackedWeight;
			AssertHasWarning(wrapper1.UnpackedWeightInfo, "No Weight defined. When packing this item the Package Weight should be manually entered.");
			AssertHasWarning(wrapper2.UnpackedWeightInfo, "No Weight UQ defined. When packing this item the Package Weight should be manually entered.");
			AssertNoWarnings(wrapper3.UnpackedWeightInfo);
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var packedItem = package.Pack_ForTesting(Data.DummyLine1, 10m);

			var wrapper = new PackableItemParentWrapper(Factory, packedItem.AsGroup(), Data.Dummy);
			using (wrapper.GetValidationSuspender())
			{
				wrapper.ProposedPackQty = -1;
				wrapper.ProposedRemoveQty = -1;
			}
			AssertNoErrors(wrapper.ProposedPackQtyInfo);
			AssertNoErrors(wrapper.ProposedRemoveQtyInfo);

			wrapper.Validation.ValidateAll();
			AssertHasErrors(wrapper.ProposedPackQtyInfo);
			AssertHasErrors(wrapper.ProposedRemoveQtyInfo);
		}

		#endregion
	}
}
