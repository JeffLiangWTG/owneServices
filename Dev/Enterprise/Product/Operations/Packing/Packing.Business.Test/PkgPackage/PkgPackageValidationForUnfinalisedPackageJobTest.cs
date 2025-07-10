using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageValidationForUnfinalisedPackageJobTest : PackingBusinessObjectValidationTestCase
	{
		#region TestValidateKP_PackageQty

		public void TestValidateKP_PackageQty()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			package.KP_PackageQty = 1;
			AssertNoErrors(package.KP_PackageQtyInfo);

			package.KP_PackageQty = 0;
			AssertHasError(package.KP_PackageQtyInfo, "Package quantity cannot be less than 1");

			package.KP_PackageQty = -1;
			AssertHasError(package.KP_PackageQtyInfo, "Package quantity cannot be less than 1");
		}

		#endregion

		#region TestPackageQtyDivisibility

		public void TestPackageQtyDivisibility()
		{
			Data.CreatePackingData();
			var packageLevel1 = Data.PackageJob.Packages.AddNew("PLT", 3);

			var packageLevel2 = packageLevel1.Packages.AddNew("BOX", 16);
			AssertHasError(packageLevel1.KP_PackageQtyInfo, "The sum of All inner package quantities must be divisible by the outer package quantity.");

			packageLevel2.KP_PackageQty = 15;
			AssertNoErrors(packageLevel1.KP_PackageQtyInfo);

			packageLevel1.KP_PackageQty = 2;
			AssertHasError(packageLevel1.KP_PackageQtyInfo, "The sum of All inner package quantities must be divisible by the outer package quantity.");
			packageLevel1.KP_PackageQty = 3;

			var packageLevel3 = packageLevel2.Packages.AddNew("BTL", 90); // 6 per box

			packageLevel2.KP_PackageQty = 11;
			AssertHasError(packageLevel1.KP_PackageQtyInfo, "The sum of All inner package quantities must be divisible by the outer package quantity.");
			AssertHasError(packageLevel2.KP_PackageQtyInfo, "The sum of All inner package quantities must be divisible by the outer package quantity.");
		}

		public void TestPackageQtyDivisibility_WithMultipleInners()
		{
			Data.CreatePackingData();

			var outerPallet = Data.PackageJob.Packages.AddNew("PLT", 10);
			var innerBoxes = outerPallet.Packages.AddNew("BOX", 8);
			AssertHasError(outerPallet.KP_PackageQtyInfo, "The sum of All inner package quantities must be divisible by the outer package quantity.");

			var innerCartons = outerPallet.Packages.AddNew("CTN", 2);
			AssertNoErrors(outerPallet.KP_PackageQtyInfo);

			var innerKegs = outerPallet.Packages.AddNew("KEG", 20);
			AssertNoErrors(outerPallet.KP_PackageQtyInfo);

			outerPallet.KP_PackageQty = 5;
			AssertNoErrors(outerPallet.KP_PackageQtyInfo);

			outerPallet.KP_PackageQty = 7;
			AssertHasError(outerPallet.KP_PackageQtyInfo, "The sum of All inner package quantities must be divisible by the outer package quantity.");
		}

		public void TestPackageQtyDivisibility_WhenParentPackageQtyIs0()
		{
			Data.CreatePackingData();
			var packageLevel1 = Data.PackageJob.Packages.AddNew("PLT", 0);
			var packageLevel2 = packageLevel1.Packages.AddNew("BOX", 2);
			AssertNoErrors(packageLevel2.KP_PackageQtyInfo);
		}

		#endregion

		#region TestValidateKP_F3_NKPackType

		public void TestValidateKP_F3_NKPackType()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var package = Data.PackageJob.Packages.AddNew();
			package.KP_F3_NKPackType = "";
			AssertHasErrors(package.KP_F3_NKPackTypeInfo);

			package.KP_F3_NKPackType = "PLT";
			AssertNoErrors(package.KP_F3_NKPackTypeInfo);

			package.KP_F3_NKPackType = "xXx";
			AssertHasErrors(package.KP_F3_NKPackTypeInfo);
		}

		public void TestValidateKP_F3_NKPackType_ContainerCannotBeChild()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var container = Data.PackageJob.Packages.AddNew("CNT");
			AssertNoErrors(container.KP_F3_NKPackTypeInfo);

			pallet.Packages.Add(container);
			container.Validation.ValidateKP_F3_NKPackType();
			AssertHasError(container.KP_F3_NKPackTypeInfo, "Containers cannot be packed into other Packages.");
		}

		#endregion

		#region TestValidateKP_Weight

		public void TestValidateKP_Weight()
		{
			var package = Factory.New<PkgPackage>();
			AssertNonNegativeValidation(package.KP_WeightInfo, "Package Weight");
		}

		#endregion

		#region TestValidateKP_Volume

		public void TestValidateKP_Volume()
		{
			var package = Factory.New<PkgPackage>();
			AssertNonNegativeValidation(package.KP_VolumeInfo, "Package Volume");
		}

		#endregion

		#region TestValidateKP_Length

		public void TestValidateKP_Length()
		{
			var package = Factory.New<PkgPackage>();
			AssertNonNegativeValidation(package.KP_LengthInfo, "Package Length");
		}

		#endregion

		#region TestValidateKP_Width

		public void TestValidateKP_Width()
		{
			var package = Factory.New<PkgPackage>();
			AssertNonNegativeValidation(package.KP_WidthInfo, "Package Width");
		}

		#endregion

		#region TestValidateKP_Height

		public void TestValidateKP_Height()
		{
			var package = Factory.New<PkgPackage>();
			AssertNonNegativeValidation(package.KP_HeightInfo, "Package Height");
		}

		#endregion

		#region TestValidateKP_WeightUQ

		public void TestValidateKP_WeightUQ()
		{
			var package = Factory.New<PkgPackage>();
			AssertUnitValidation(package.KP_WeightInfo, 10m, package.KP_WeightUQInfo, "KG", "XX");
		}

		#endregion

		#region TestValidateKP_VolumeUQ

		public void TestValidateKP_VolumeUQ()
		{
			var package = Factory.New<PkgPackage>();
			AssertUnitValidation(package.KP_VolumeInfo, 10m, package.KP_VolumeUQInfo, "M3", "XX");
		}

		#endregion

		#region TestValidateKP_DimensionUQ

		public void TestValidateKP_DimensionUQ()
		{
			var package = Factory.New<PkgPackage>();

			AssertUnitValidation(package.KP_LengthInfo, 10m, package.KP_DimensionUQInfo, "M", "XX");
			package.KP_Length = 0m; // clean up

			AssertUnitValidation(package.KP_WidthInfo, 10m, package.KP_DimensionUQInfo, "M", "XX");
			package.KP_Width = 0m; // clean up

			AssertUnitValidation(package.KP_HeightInfo, 10m, package.KP_DimensionUQInfo, "M", "XX");
		}

		#endregion

		#region TestValidateKP_PackageID

		public void TestValidateKP_PackageID()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();

			// ensure package ID is only allowed when qty = 1

			package.KP_PackageID = "abc";
			AssertNoErrors(package.KP_PackageIDInfo);

			package.KP_PackageQty = 2;
			AssertHasError(package.KP_PackageIDInfo, "A Package ID is for a single Package. Either change the Package Qty to 1 or remove the Package ID.");

			package.KP_PackageQty = 1;
			AssertNoErrors(package.KP_PackageIDInfo);

			// ensure package ID is unique in hierarchy

			var childPackage = package.Packages.AddNew();
			childPackage.KP_PackageID = "aBc"; // intentionally uses different case
			AssertHasError(childPackage.KP_PackageIDInfo, "Package ID 'aBc' is assigned to another package (IDs must be unique per Job).");

			childPackage.KP_PackageID = "aBc2";
			AssertNoErrors(childPackage.KP_PackageIDInfo);
		}

		public void TestValidateKP_PackageID_WhenContainer_PackageIdLength()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew(PkgUnit.Container);

			package.KP_PackageID = "123456789012";
			AssertNoErrors(package.KP_PackageIDInfo);

			package.KP_PackageID = "1234567890123";
			AssertHasError(package.KP_PackageIDInfo, "The Container Package ID cannot be more than 12 characters.");

			// ensure container-specific validation does not run on non-containers

			package.KP_F3_NKPackType = PkgUnit.Pallet;
			AssertNoErrors(package.KP_PackageIDInfo);
		}

		public void TestValidateKP_PackageID_WhenContainer_PackageId()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParentUsingPackageHandlingUnitDivots);
			var parentJob = Factory.New<DummyPackingParentUsingPackageHandlingUnitDivots>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var package = packageJob.Packages.AddNew();
			package.KP_F3_NKPackType = PkgUnit.Container;
			parentJob.NotificationTypeForInvalidContainerNumber = NotificationTypes.Warning;

			package.KP_PackageID = "FAKE4100011";
			AssertNoNotifications("Should have no notifications as the container number is valid", package.KP_PackageIDInfo);

			package.KP_PackageID = "FAKE4100010";
			AssertHasWarning("Should provide warning about check digit", package.KP_PackageIDInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");
			AssertNoMessageErrors("Should not have message error as NotificationTypeForInvalidContainerNumber is Warning", package.KP_PackageIDInfo);

			package.KP_PackageID = "F@KE4100010";
			AssertHasWarning("Should provide warning about format", package.KP_PackageIDInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertNoMessageErrors("Should not have message error as NotificationTypeForInvalidContainerNumber is Warning", package.KP_PackageIDInfo);

			parentJob.NotificationTypeForInvalidContainerNumber = NotificationTypes.MessageError;

			package.KP_PackageID = "FAKE4100010";
			AssertHasMessageError("Should provide message error about check digit", package.KP_PackageIDInfo, "Container number does not have a valid check (last) digit. The check digit should be 1.");
			AssertNoWarnings("Should not have warning as NotificationTypeForInvalidContainerNumber is Message Error", package.KP_PackageIDInfo);

			package.KP_PackageID = "F@KE4100010";
			AssertHasMessageError("Should provide message error about format", package.KP_PackageIDInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertNoWarnings("Should not have warning as NotificationTypeForInvalidContainerNumber is Message Error", package.KP_PackageIDInfo);

			package.KP_PackageID = string.Empty;
			AssertNoNotifications("Should have no notifications as container number is empty", package.KP_PackageIDInfo);

			package.KP_PackageID = null;
			AssertNoNotifications("Should have no notifications as container number is null", package.KP_PackageIDInfo);

			package.KP_PackageID = "FAKE4100010";
			package.KP_F3_NKPackType = PkgUnit.Pallet;
			AssertNoNotifications("Should have no notifications as package is not a container", package.KP_PackageIDInfo);

			package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = PkgUnit.Container;
			package.KP_PackageID = "FAKE4100010";
			AssertNoNotifications("Should have no notifications as it lacks a package job", package.KP_PackageIDInfo);

			packageJob = Factory.New<PkgPackageJob>();
			package = packageJob.Packages.AddNew();
			AssertNoExceptionThrown("Should not throw exception due to it lacking a package parent", () =>
			{
				package.KP_F3_NKPackType = PkgUnit.Container;
			});
			package.KP_PackageID = "FAKE4100010";
			AssertNoNotifications("Should have no notifications as it lacks a package parent", package.KP_PackageIDInfo);

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
		}

		#endregion

		#region TestValidateKP_PreviousPackageID

		public void TestValidateKP_PreviousPackageID()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertNoErrors("Precondition", package.KP_PreviousPackageIDInfo);

			package.KP_PreviousPackageID = "123";
			AssertHasError(package.KP_PreviousPackageIDInfo, "Previous Package ID should not be entered if there is no new Package ID.");

			package.KP_PackageID = "456";
			package.Validation.ValidateKP_PreviousPackageID();
			AssertNoErrors(package.KP_PreviousPackageIDInfo);

			package.KP_PreviousPackageID = "456";
			AssertHasError(package.KP_PreviousPackageIDInfo, "Previous Package ID cannot be the same as the current Package ID.");
		}

		#endregion

		#region TestValidateKP_IsReleased

		public void TestValidateKP_IsReleased()
		{
			Data.CreatePackingData();

			var pallet = Data.PackageJob.Packages.AddNew();
			pallet.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertNoErrors(pallet.KP_ReleasedTimeUtcInfo);

			var box = pallet.Packages.AddNew();
			box.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertHasError(box.KP_ReleasedTimeUtcInfo, "An Inner Package cannot be Released. Either Move this package up to the top level, or Cancel it's Release.");
		}

		#endregion

		#region TestValidateKP_IsReleasedViaJob

		public void TestValidateKP_IsReleasedViaJob()
		{
			Data.CreatePackingData();

			var pallet = Data.PackageJob.Packages.AddNew();
			pallet.KP_IsReleasedViaJob = true;
			AssertNoErrors(pallet.KP_IsReleasedViaJobInfo);

			var box = pallet.Packages.AddNew();
			box.KP_IsReleasedViaJob = true;
			AssertHasError(box.KP_IsReleasedViaJobInfo, "An Inner Package cannot be Released. Either Move this package up to the top level or Delete it.");
		}

		#endregion

		#region TestValidateKP_RH_NKCommodityCode

		public void TestValidateKP_RH_NKCommodityCode()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertNoErrors(package.KP_RH_NKCommodityCodeInfo);

			package.KP_RH_NKCommodityCode = "X";
			AssertHasErrors(package.KP_RH_NKCommodityCodeInfo);

			var commodity = Factory.LoadTop1<RefCommodityCode>(new ZQuery());
			package.KP_RH_NKCommodityCode = commodity.RH_Code;
			AssertNoErrors(package.KP_RH_NKCommodityCodeInfo);
		}

		#endregion

		#region TestValidateKP_TareWeight

		public void TestValidateKP_TareWeight()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "CNT";
			AssertNonNegativeValidation(package.KP_TareWeightInfo, "Tare Weight");
		}

		#endregion

		#region TestValidateKP_DunnageWeight

		public void TestValidateKP_DunnageWeight()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "CNT";
			AssertNonNegativeValidation(package.KP_DunnageWeightInfo, "Dunnage Weight");
		}

		#endregion

		// temperature

		#region TestValidateKP_RequiredTemperatureMinimumAndMax

		public void TestValidateKP_RequiredTemperatureMinimumAndMax()
		{
			var package = Factory.New<PkgPackage>();

			package.KP_RequiredTemperatureMinimum = 0;
			package.KP_RequiredTemperatureMaximum = 0;
			AssertNoErrors(package.KP_RequiredTemperatureMinimumInfo);
			AssertNoErrors(package.KP_RequiredTemperatureMaximumInfo);

			package.KP_RequiredTemperatureMinimum = 5;
			package.KP_RequiredTemperatureMaximum = 0;
			AssertHasError(package.KP_RequiredTemperatureMinimumInfo, "The Minimum Temperature cannot be greater than the Maximum Temperature.");
			AssertHasError(package.KP_RequiredTemperatureMaximumInfo, "The Maximum Temperature cannot be less than the Minimum Temperature.");

			package.KP_RequiredTemperatureMinimum = 5;
			package.KP_RequiredTemperatureMaximum = 10;
			AssertNoErrors(package.KP_RequiredTemperatureMinimumInfo);
			AssertNoErrors(package.KP_RequiredTemperatureMaximumInfo);

			package.KP_RequiredTemperatureMinimum = -1;
			package.KP_RequiredTemperatureMaximum = -1;
			AssertNoErrors(package.KP_RequiredTemperatureMinimumInfo);
			AssertNoErrors(package.KP_RequiredTemperatureMaximumInfo);
		}

		#endregion

		#region TestValidateKP_RequiredTemperatureUnit

		public void TestValidateKP_RequiredTemperatureUnit()
		{
			var package = Factory.New<PkgPackage>();
			AssertListValidation(package.KP_RequiredTemperatureUnitInfo, Core.Constants.Temperature.Centigrade, "X");

			package.KP_RequiredTemperatureUnit = "";
			package.Validation.ValidateKP_RequiredTemperatureUnit();
			AssertNoErrors(package.KP_RequiredTemperatureUnitInfo);

			package.KP_RequiresTemperatureControl = true;
			package.Validation.ValidateKP_RequiredTemperatureUnit();
			AssertHasErrors("If the Package is temperature controlled, the temperature unit should be required.", package.KP_RequiredTemperatureUnitInfo);
		}

		#endregion

		// calculated

		#region TestValidateGoodsWeight

		public void TestValidateGoodsWeight()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_WeightUQ = "KG";
			package.KP_TareWeight = 2m;
			package.KP_Weight = 1m;

			AssertEquals("Precondition", -1m, package.GoodsWeight);
			AssertEquals("Precondition", "KG", package.KP_WeightUQ);

			package.Validation.ValidateGoodsWeight();
			AssertHasError(package.GoodsWeightInfo, "Goods Weight, calculated by Package Weight(1KG) - Package Tare Weight(2KG), cannot be less than 0.");
		}

		#endregion
	}
}
