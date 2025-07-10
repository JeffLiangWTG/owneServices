using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PackingRegistry))]
	public class PackingRegistryTest : RegistryItemSetTestCaseWithFactory<PackingRegistry>
	{
		#region TestAnonymousPackageCreationEnabled

		public void TestAnonymousPackageCreationEnabled()
		{
			TestRegistryItem(ItemSet.AnonymousPackageCreationEnabled,
				"AnonymousPackageCreationEnabled",
				PackingRegistry.Categories.Packing,
				"Package Scanning Auto-Creation",
				"If enabled, 'anonymous' packages will automatically be created when scanning barcodes that do not match a package in the system. They can then be allocated to jobs when a matching package ID/barcode is entered.",
				RegistryStorageFlags.System,
				false);
		}

		#endregion

		#region TestAnonymousPackagePurgeTime

		public void TestAnonymousPackagePurgeTime()
		{
			TestRegistryItem(ItemSet.AnonymousPackagePurgeTime,
				"AnonymousPackagePurgeTime",
				PackingRegistry.Categories.Packing,
				"Anonymous Package Purge Time",
				"The time (in days) after which Anonymous Packages will be deleted if they have remained unattached to a Job.",
				RegistryStorageFlags.System,
				14);
		}

		#endregion

		#region TestWeightUnit

		public void TestWeightUnit()
		{
			TestRegistryItem(ItemSet.WeightUnit, "WeightUnit", PackingRegistry.Categories.Packing, "Weight Unit", "The default Weight Unit for new Packages in Packing.",
				RegistryStorageFlags.All, new CodeDescriptionPairList(OLookUpEditType.Weight), Constants.Weight.Kilograms);
		}

		#endregion

		#region TestVolumeUnit

		public void TestVolumeUnit()
		{
			TestRegistryItem(ItemSet.VolumeUnit, "VolumeUnit", PackingRegistry.Categories.Packing, "Volume Unit", "The default Volume Unit for new Packages in Packing.",
				RegistryStorageFlags.All, new CodeDescriptionPairList(OLookUpEditType.Volume), Constants.Volume.CubicMetres);
		}

		#endregion

		#region TestDimensionUnit

		public void TestDimensionUnit()
		{
			TestRegistryItem(ItemSet.DimensionUnit, "DimensionUnit", PackingRegistry.Categories.Packing, "Dimension Unit", "The default Dimension Unit for new Packages in Packing.",
				RegistryStorageFlags.All, new CodeDescriptionPairList(OLookUpEditType.Length), Constants.Length.Metres);
		}

		#endregion

		#region TestOuterPackageUnit

		public void TestOuterPackageUnit()
		{
			TestRegistryItem(ItemSet.OuterPackageUnit, "PackageUnit", PackingRegistry.Categories.Packing, "Outer Package Unit", "The default Package Unit for new Outer Packages in Packing.",
				RegistryStorageFlags.All, new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair(), Constants.PkgUnit.Pallet);
		}

		#endregion

		#region TestInnerPackageUnit

		public void TestInnerPackageUnit()
		{
			var packTypes = new RefPackTypeCollection(new BusinessObjectFactory()).GetAsCodeDescriptionPair();
			packTypes.RemoveCode(Constants.PkgUnit.Container);

			TestRegistryItem(ItemSet.InnerPackageUnit, "InnerPackageUnit", PackingRegistry.Categories.Packing, "Inner Package Unit", "The default Package Unit for new Inner Packages in Packing.",
				RegistryStorageFlags.All, packTypes, Constants.PkgUnit.Carton);
		}

		#endregion

		#region TestPackageIDCustomisation

		public void TestPackageIDCustomisation()
		{
			AssertEquals(RegistryStorageFlags.All, ItemSet.PackageIDCustomisation.Storage);
			var customisation = new BillOfLadingNumberCustomisation();

			customisation.RemoveFountainPrefix = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.PackageIDCustomisation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customisation);

			var value = ItemSet.PackageIDCustomisation.Value;

			AssertEquals("Value.RemovePrefix", true, value.RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Categories", NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.PackageID, value.Categories);
		}

		public void TestPackageIDCustomisation_Default()
		{
			// Default as per the format before customisable fountain was implemented
			// [JOBNUMBER]-[SEQUENCENUMBER] i.e.  W00000001-001
			var defaultCustomisation = ItemSet.PackageIDCustomisation.DefaultValue;
			AssertEquals(true, defaultCustomisation.AllowNonAlphanumericCharacters);
			AssertEquals(false, defaultCustomisation.EnableMacroInsertion);
			AssertEquals("JobNo should be used with fountain.", true, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Fountain);
			AssertEquals("JobNo.", true, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Include);
			AssertEquals("JobNo should be first.", (ZByte)1, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Order);

			AssertEquals("ClientCoded1.", "-", defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Detail);
			AssertEquals("ClientCoded1.", true, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Include);
			AssertEquals("ClientCoded1 (dash) should be in the middle.", (ZByte)2, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Order);

			AssertEquals("SequenceNumber length should be 3.", "3", defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("SequenceNumber.", true, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Include);
			AssertEquals("SequenceNumber should be last.", (ZByte)50, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Order);
		}

		#endregion

		#region TestPackageIDUniquePeriodInMonths

		public void TestPackageIDUniquePeriodInMonths()
		{
			var item = ItemSet.PackageIDUniquePeriodInMonths;

			AssertEquals(RegistryOptions.IsOnlyForSupport, item.Options); // this should be changed to 'Default' once all development items done, open to users.
			AssertEquals(6, item.DefaultValue);
			AssertEquals((double)1, ((IntRegistryDataType)item.DataType).LowerBound);
			AssertEquals((double)12, ((IntRegistryDataType)item.DataType).UpperBound);

			AssertEquals(item.DefaultValue, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			AssertEquals(10, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#endregion

		#region TestProductLabelDateFormat

		public void TestProductLabelDateFormat()
		{
			TestRegistryItem(ItemSet.ProductLabelDateFormat, "ProductLabelDateFormat", PackingRegistry.Categories.Packing_PackingLabels, "Product Label Date Format",
				"The Date Format of Dates on Product Labels. Default: dd.MM.yyyy",
				RegistryStorageFlags.All, RegistryOptions.Default, TextEditorType.TextBox, "dd.MM.yyyy");
		}

		#endregion

		#region TestDefaultDocumentToPrintOnClosePackage

		public void TestDefaultDocumentToPrintOnClosePackage()
		{
			TestRegistryItem(ItemSet.DefaultDocumentToPrintOnClosePackage,
				"DefaultDocumentToPrintOnClosePackage",
				PackingRegistry.Categories.Packing_PackingLabels,
				"Default Label to Print on Close Package when Scan Packing",
				"Default Label to Print on Close Package when Scan Packing",
				RegistryStorageFlags.Branch,
				RegistryFindBoxCollection.PackingDocument,
				PackingRegistry.DefaultTargetLabelDocument);
		}

		#endregion

		#region TestDefaultTargetLabelDocument

		public void TestDefaultTargetLabelDocument()
		{
			AssertEquals(new Guid("89758b9c-7fc7-4f5f-a095-f5d3ec318cde"), PackingRegistry.DefaultTargetLabelDocument);

			var menuItem = Factory.Load<StmMenuItem>(PackingRegistry.DefaultTargetLabelDocument);
			AssertEquals(nameof(BusinessContext.Package), menuItem.SU_BusinessContext);
			AssertEquals(true, menuItem.SU_IsSystemDefined);
			AssertEquals("Product/Delivery (Selected Only)", menuItem.SU_MenuName);
			AssertEquals("CNE", menuItem.SU_ContactType);
		}

		#endregion

		#region TestAutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses

		public void TestAutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses()
		{
			TestRegistryItem(ItemSet.AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses,
				"AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses",
				PackingRegistry.Categories.Packing_PackingLabels,
				"Auto-Print Label On Package Close For Overridden Consignee Addresses",
				"Enabling this setting allows label auto-print on package close for overridden consignee addresses.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				false);
		}

		#endregion

		#region TestDamagedReasons

		public void TestDamagedReasons()
		{
			TestRegistryItemWithDefaultCode(ItemSet.DamagedReasons, "DamagedReasons", PackingRegistry.Categories.Packing,
				"Damaged Reasons", "The available reasons that may be provided for a damaged package.", RegistryStorageFlags.System, 3, 0);
		}

		#endregion

		#region ConditionallyVisibleRegistryItems

		protected override System.Collections.Generic.IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "PackageIDUniquePeriodInMonths";
			}
		}

		#endregion

		#region TestLastPackingFountainsDeleteTimeUtc

		public void TestLastPackingFountainsDeleteTimeUtc()
		{
			TestGenericRegistryItem(
				ItemSet.LastPackingFountainsDeleteTimeUtc,
				"LastPackingFountainsDeleteTimeUtc",
				PackingRegistry.Categories.Packing,
				"Last Packing Fountains Delete Run",
				"Time (in UTC) of the last time packing fountains of finalized packing jobs were deleted.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				new DateTime(1900, 1, 1));
		}

		#endregion
	}
}
