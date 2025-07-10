using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrder))]
	public class WhsOrderIPackingParentWithPackableItemsTest : PackingParentWithPackableItemsTestCase<WhsOrder>
	{
		protected override WhsOrder GetNewParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			helper.CreatePickNew(order);
			AssertEquals("Precondition - Order should be Picking.", true, order.IsAttachedToPickButNotFinalised);

			return order;
		}

		#region TestRestrictEdits

		#region TestRestrictEdits_TotePackagePropertiesAreReadOnly

		public void TestRestrictEdits_TotePackagesAreReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();

			AssertEquals("Precondition: Package is NOT a tote.", false, package.GetIsTote());
			AssertEquals("Precondition: Package is NOT ReadOnly.", false, package.ReadOnly);

			package.SetIsTote(true);
			AssertEquals("Package is a tote.", true, package.GetIsTote());
			AssertEquals("Package is ReadOnly.", true, package.ReadOnly);
		}

		#endregion

		#region TestRestrictEdits_TotePackageChildrenReadOnly

		public void TestRestrictEdits_TotePackageChildrenReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package = packageJob.Packages.AddNew();
			var childPackageOne = package.Packages.AddNew();
			var secondOrderChildPackageOne = childPackageOne.Packages.AddNew();
			var childPackageTwo = package.Packages.AddNew();
			var allPackages = new List<PkgPackage>
			{
				package, childPackageOne, secondOrderChildPackageOne, childPackageTwo
			};

			AssertEquals("Precondition: Package is NOT a tote.", false, package.GetIsTote());
			CombineAssertions(() =>
			{
				foreach (var child in allPackages)
				{
					AssertEquals(
						string.Format(Culture.Invariant, "Precondition: Non-Tote package child is NOT readonly - {0}",
							child.Description), false, child.ReadOnly);
				}
			});

			package.SetIsTote(true);
			AssertEquals("Package is a tote.", true, package.GetIsTote());
			CombineAssertions(() =>
			{
				foreach (var child in allPackages)
				{
					AssertEquals(
						string.Format(Culture.Invariant, "Tote package child is readonly - {0}", child.Description),
						true, child.ReadOnly);
				}
			});
		}

		#endregion

		#region TestRestrictEdits_CannotDeleteTote

		public void TestRestrictEdits_CannotDeleteTote_WithPackedItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 3m);

			AssertNotNull("Precondition: packageJob.ParentJob is NOT null.", packageJob.ParentJob);
			AssertEquals("Precondition: Package is NOT a tote.", false, package.GetIsTote());
			AssertEquals("Precondition: Non-tote packages with packed items are able to be deleted.", true,
				package.CanDelete);

			package.SetIsTote(true);
			AssertEquals("Package is a tote.", true, package.GetIsTote());
			AssertEquals("Tote packages with packed items cannot be deleted", false, package.CanDelete);
		}

		public void TestRestrictEdits_CanDeleteEmptyTote()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();

			AssertNotNull("Precondition: packageJob.ParentJob is NOT null.", packageJob.ParentJob);
			AssertEquals("Precondition: Package is NOT a tote.", false, package.GetIsTote());
			AssertEquals("Precondition: Non-tote packages are able to be deleted.", true, package.CanDelete);

			package.SetIsTote(true);
			AssertEquals("Package is a tote.", true, package.GetIsTote());
			AssertEquals("Empty Tote packages can be deleted", true, package.CanDelete);
		}

		#endregion

		#region TestRestrictEdits_CanUnpackFullyPickedTote

		public void TestRestrictEdits_CanUnpackFullyPickedTote()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();
			var innerPackage = package.Packages.AddNew();

			AssertNotNull("Precondition: packageJob.ParentJob is NOT null.", packageJob.ParentJob);
			AssertEquals("Precondition: Package is NOT a tote.", false, package.GetIsTote());
			AssertEquals("Precondition: Non-tote packages are able to be unpacked.", true,
				package.IsAvailableForUnpacking(out ZString errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);

			package.SetIsTote(true);
			AssertEquals("Package is a tote.", true, package.GetIsTote());
			AssertEquals("Tote packages ARE able to be unpacked if they are fully picked.", true,
				package.IsAvailableForUnpacking(out errorMessage));
			AssertEquals("Tote packages ARE able to be packed if they are fully picked.", true,
				package.IsAvailableForPacking(out errorMessage));
		}

		#endregion

		#endregion

		#region TestGetSSCCPrefix

		public void TestGetSSCCPrefix_DisplaysNoMessageWhenNeitherClientNorWarehouseOrgHasGS1Prefix()
		{
			IPackingParent order = GetNewParent();

			foreach (SSCCGenerationContext context in Enum.GetValues(typeof(SSCCGenerationContext)))
			{
				AssertEquals("", order.GetSSCCPrefix(Notify, context));
				AssertNull(Notify.LastQueryUserEventArgs);
			}
		}

		public void TestGetSSCCPrefix_DisplaysNoMessageWhenClientHasNoGS1PrefixButWarehouseGS1PrefixHasDifferentPremisesAddress()
		{
			var order = SetupOrderWithWarehouseAddress(PremisesAddress.Different);

			foreach (SSCCGenerationContext context in Enum.GetValues(typeof(SSCCGenerationContext)))
			{
				AssertEquals("", order.GetSSCCPrefix(Notify, context));
				AssertNull(Notify.LastQueryUserEventArgs);
			}
		}

		public void TestGetSSCCPrefix_DisplaysNoMessageWhenClientHasGS1Prefix()
		{
			var order = SetupOrderWithClientOrgHeader();

			foreach (SSCCGenerationContext context in Enum.GetValues(typeof(SSCCGenerationContext)))
			{
				AssertEquals("2222222", order.GetSSCCPrefix(Notify, context));
				AssertNull(Notify.LastQueryUserEventArgs);
			}
		}

		public void TestGetSSCCPrefix_DisplaysMessageWhenOnlyWarehouseOrgHasGS1Prefix_GeneratingIDsViaUser()
		{
			const string whsGS1CompanyPrefix = "1111111";
			var order = SetupOrderWithWarehouseAddress();
			var notificationBuffer = new TestNotificationBuffer(true);

			AssertEquals(whsGS1CompanyPrefix, order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
			AssertNull("To check if barcode is SSCC, should not ask user about the Warehouse GS1 company prefix.",
				notificationBuffer.LastQueryUserEventArgs);

			AssertEquals(whsGS1CompanyPrefix, order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Generate SSCC Number", ((QueryUserMsgBoxEventArgs)notificationBuffer.LastQueryUserEventArgs).Caption);
			AssertEquals(
				"Client Donald Trump does not have a GS1 company prefix, would you like to use the warehouse GS1 company prefix?",
				((QueryUserMsgBoxEventArgs)notificationBuffer.LastQueryUserEventArgs).Message);

			// ensure we cache the user selection and do not ask again

			notificationBuffer.LastQueryUserEventArgs = null;
			AssertNull("Precondition", notificationBuffer.LastQueryUserEventArgs);

			notificationBuffer.DefaultResponse =
				false; // needs to be a different selection to prove we are caching the previous selection
			AssertEquals("SSCC from last user selection should be used.", whsGS1CompanyPrefix,
				order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertNull("SSCC Message should not be displayed a second time.", notificationBuffer.LastQueryUserEventArgs);
		}

		public void TestGetSSCCPrefix_FallbackToWarehousePrefix_ScanPacking()
		{
			var order = SetupOrderWithWarehouseAddress();
			var notifications = new TestNotificationBuffer(true);

			AssertEquals("", order.GetSSCCPrefix(notifications, SSCCGenerationContext.ScanPacking));
			AssertNull("Scan Packing should not ask the user.", notifications.LastQueryUserEventArgs);

			((WhsOrder)order).Warehouse.WW_UseGS1PrefixFallback = true;
			AssertEquals("Test that Scan Packing Context caches the result.", "",
				order.GetSSCCPrefix(notifications, SSCCGenerationContext.ScanPacking));
			AssertNull("Scan Packing should not ask the user.", notifications.LastQueryUserEventArgs);
		}

		public void TestGetSSCCPrefix_FallbackToWarehousePrefix_ScanPacking_WhenResponseIsCached()
		{
			const string whsGS1CompanyPrefix = "1111111";
			var order = SetupOrderWithWarehouseAddress();
			var notificationBuffer = new TestNotificationBuffer(true);

			AssertEquals(whsGS1CompanyPrefix, order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.GeneratingIDsViaUser));

			((WhsOrder)order).Warehouse.WarehouseAddress.Header.CustomsCodes.RemoveAndDeleteAll();
			AssertEquals("Test that Scan Packing Context uses the cached result.", whsGS1CompanyPrefix,
				order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.ScanPacking));
		}

		public void TestGetSSCCPrefix_FallbackToWarehousePrefix_GeneratingIDsOnSave()
		{
			const string whsGS1CompanyPrefix = "1111111";
			var order = SetupOrderWithWarehouseAddress();
			var notificationBuffer = new TestNotificationBuffer(true);

			AssertEquals("", order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.GeneratingIDsOnSave));
			AssertNull("Generating IDs on Save should not ask the user.", notificationBuffer.LastQueryUserEventArgs);

			((WhsOrder)order).Warehouse.WW_UseGS1PrefixFallback = true;
			AssertEquals("Test that Generating IDs On Save Context does not cache the result.", whsGS1CompanyPrefix,
				order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.GeneratingIDsOnSave));
			AssertNull("Generating IDs on Save should not ask the user.", notificationBuffer.LastQueryUserEventArgs);
		}

		public void TestGetSSCCPrefix_FallbackToWarehousePrefix_AutoClosingPackage()
		{
			// This context is strange in that the only places that use it are the Order & Pick "Auto Pack" Operational Actions
			// and it immediately saves the Order right after generating the IDs (and cannot prompt the user a question),
			// this context and the Packing method should probably be deleted as this context will have identical behaviour to GeneratingIDsOnSave.
			const string whsGS1CompanyPrefix = "1111111";
			var order = SetupOrderWithWarehouseAddress();
			var notificationBuffer = new TestNotificationBuffer(true);

			AssertEquals("", order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.AutoClosingPackage));
			AssertNull("Generating IDs on Save should not ask the user.", notificationBuffer.LastQueryUserEventArgs);

			((WhsOrder)order).Warehouse.WW_UseGS1PrefixFallback = true;
			AssertEquals("Test that Closing Package Context does not cache the result.", whsGS1CompanyPrefix,
				order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.AutoClosingPackage));
			AssertNull("Generating IDs on Save should not ask the user.", notificationBuffer.LastQueryUserEventArgs);
		}

		public void TestGetSSCCPrefix_FallbackToWarehousePrefix_CheckIfBarcodeIsSSCC()
		{
			const string whsGS1CompanyPrefix = "1111111";
			var order = SetupOrderWithWarehouseAddress();
			var notificationBuffer = new TestNotificationBuffer(true);

			AssertEquals(whsGS1CompanyPrefix, order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
			AssertNull("To check if barcode is SSCC, should not ask user about the Warehouse GS1 company prefix.",
				notificationBuffer.LastQueryUserEventArgs);

			((WhsOrder)order).Client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "2222222");
			AssertEquals("CheckIfBarcodeIsSSCC Context should not cache result.", "2222222",
				order.GetSSCCPrefix(notificationBuffer, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
		}

		public void TestGetSSCCPrefix_FallbackToWarehousePrefix_InvalidContext()
		{
			var order = SetupOrderWithWarehouseAddress();
			AssertExceptionThrown<ArgumentException>(() => order.GetSSCCPrefix(Notify, (SSCCGenerationContext)(-1)));
		}

		public void TestGetSSCCPrefix_DisplaysNoMessageWhenGS1PrefixFlagIsOn()
		{
			var order = SetupOrderWithWarehouseAddress();
			((WhsOrder)order).Warehouse.WW_UseGS1PrefixFallback = true;

			foreach (SSCCGenerationContext context in Enum.GetValues(typeof(SSCCGenerationContext)))
			{
				AssertEquals("order.GetSSCCPrefix", "1111111", order.GetSSCCPrefix(Notify, context));
				AssertNull(Notify.LastQueryUserEventArgs);
			}
		}

		public void TestGetSSCCPrefix_DisplaysNoMessageWhenGS1PrefixFlagIsOnAndPremisesAddressIsEmpty()
		{
			var order = SetupOrderWithWarehouseAddress(PremisesAddress.None);
			((WhsOrder)order).Warehouse.WW_UseGS1PrefixFallback = true;

			foreach (SSCCGenerationContext context in Enum.GetValues(typeof(SSCCGenerationContext)))
			{
				AssertEquals("1111111", order.GetSSCCPrefix(Notify, context));
				AssertNull(Notify.LastQueryUserEventArgs);
			}
		}

		#endregion

		#region TestParentJobType

		public void TestParentJobType()
		{
			AssertEquals(ParentJobType.WarehouseOrder, ((IPackingParent)Factory.New<WhsOrder>()).ParentJobType);
		}

		#endregion

		#region Implementation

		IPackingParent SetupOrderWithClientOrgHeader()
		{
			var client = Factory.New<OrgHeader>();
			client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "2222222");

			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = client.PK;

			return order;
		}

		enum PremisesAddress
		{
			None,
			Same,
			Different
		}

		IPackingParent SetupOrderWithWarehouseAddress(PremisesAddress premisesAddressType = PremisesAddress.Same)
		{
			var warehouse = Factory.New<WhsWarehouse>();
			var order = Factory.New<WhsOrder>();
			var orgHeader = Factory.New<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1111111");

			switch (premisesAddressType)
			{
				case PremisesAddress.Same:
					cusCode.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
					break;

				case PremisesAddress.Different:
					cusCode.OK_OA_PremisesAddress = ZGuid.NewZGuid();
					break;

				case PremisesAddress.None:
					break;
			}

			warehouse.WW_OA_WarehouseAddress = orgHeader.MainAddress.PK;
			order.WD_WW_Whs = warehouse.PK;

			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Donald Trump";
			order.WD_OH_Client = client.PK;

			return order;
		}

		TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer(false)); }
		}

		TestNotificationBuffer notify;

		#endregion
	}
}
