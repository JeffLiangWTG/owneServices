using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Packing.Business.Testing
{
	[UseSnapshotProtection]
	public class PackageIDBaseGeneratorTest : PackingTestCaseWithFactory
	{
		#region TestGenerateIDs

		public void TestGenerateIDs()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			// no packages

			PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser);
			AssertEquals("No packages were selected.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			Notify.Clear();

			// package with qty > 1

			var packageWithQtyOf2 = packageJob.Packages.AddNew("PLT");
			packageWithQtyOf2.KP_PackageQty = 2;

			AssertEquals(false, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package ID should not be generated when KP_PackageQty != 1.", true, packageWithQtyOf2.KP_PackageID.IsEmpty);
			AssertEquals("Package IDs can only be created for non-Container Packages that have no Package ID and a Qty of one (1).", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Warning, Notify.LastEvent.Type);
			Notify.Clear();

			// packages that already have IDs

			var packageWithID = packageJob.Packages.AddNew("PLT");
			packageWithID.KP_PackageID = "ID-123";

			AssertEquals(false, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Existing Package ID should not have been overwritten.", "ID-123", packageWithID.KP_PackageID);
			AssertEquals("Package IDs can only be created for non-Container Packages that have no Package ID and a Qty of one (1).", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Warning, Notify.LastEvent.Type);
			Notify.Clear();

			// two packages with empty IDs

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("PLT");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Existing Package ID should not have been overwritten.", "ID-123", packageWithID.KP_PackageID);
			AssertEquals("The PackageJob has no parent (eg WhsDocket), the PackageJob's JobNo should be used.", "P00000001-001", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob has no parent (eg WhsDocket), the PackageJob's JobNo should be used.", "P00000001-002", packageWithNoID_2.KP_PackageID);
			AssertNull("No message should have been shown.", Notify.LastEvent);
			packageWithNoID_1.KP_PackageID = "";
			packageWithNoID_2.KP_PackageID = "";
			Notify.Clear();

			// two packages with empty IDs - parent job is attached but unsaved (has no job ID)

			var dummy = Factory.New<DummyWithPacking>();
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			AssertEquals(false, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("The Dummy does not yet have a Job Number. Try saving the Dummy first.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			AssertEquals(null, dummy.LastSSCCGenerationContext);
			Notify.Clear();

			// two packages with empty IDs - parent job is attached and saved (has a job ID)

			packageJob.KJ_JobID = "";
			dummy.JobNoForPackingParent = "D00000001";
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("The PackageJob has a parent (Dummy), the Dummy's JobNo should be used.", "D00000001-001", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob has a parent (Dummy), the Dummy's JobNo should be used.", "D00000001-002", packageWithNoID_2.KP_PackageID);
			AssertNull("No message should have been shown.", Notify.LastEvent);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
			Notify.Clear();

			// two packages, one with an empty ID - make sure the next ID is unique

			packageWithNoID_2.KP_PackageID = "";
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsOnSave));
			AssertEquals("Number generation should use number fountain.", "D00000001-003", packageWithNoID_2.KP_PackageID);
			AssertEquals(SSCCGenerationContext.GeneratingIDsOnSave, dummy.LastSSCCGenerationContext);
			Notify.Clear();

			// two packages, one with an empty ID - make sure we honour the Package ID Customisation registry setting

			var customisation = PackingRegistry.Instance.PackageIDCustomisation.Value;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "5";
			dummy.JobNoForPackingParent = "D00000002";

			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation))
			{
				packageWithNoID_1.KP_PackageID = "";
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("Number generation should use the Packing Registry suffix length.", "D00000002-00001", packageWithNoID_1.KP_PackageID);
				AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
			}
		}

		public void TestGenerateIDs_GeneratingIDsViaUser()
		{
			AssertGenerateIDs_WithContext(SSCCGenerationContext.GeneratingIDsViaUser);
		}

		public void TestGenerateIDs_ScanPacking()
		{
			AssertGenerateIDs_WithContext(SSCCGenerationContext.ScanPacking);
		}

		public void TestGenerateIDs_AutoClosingPackage()
		{
			AssertGenerateIDs_WithContext(SSCCGenerationContext.AutoClosingPackage);
		}

		void AssertGenerateIDs_WithContext(SSCCGenerationContext context)
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			dummy.JobNoForPackingParent = "D00001000";
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("CTN");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, context));
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "D00001000-001", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "D00001000-002", packageWithNoID_2.KP_PackageID);
			AssertEquals(context, dummy.LastSSCCGenerationContext);
			AssertEquals("Should only Auto-Save if the context is GeneratingIDsViaUser or ScanPacking.",
				context == SSCCGenerationContext.GeneratingIDsViaUser || context == SSCCGenerationContext.ScanPacking, packageWithNoID_1.IsInDatabase);
		}

		public void TestGenerateIDs_WithSSCCPrefix()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			dummy.SSCCPrefix = "2111112";
			dummy.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy.JobNoForPackingParent = "D00001000";
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("CTN");
			var container = packageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000012", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000029", packageWithNoID_2.KP_PackageID);
			AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
			AssertNotNull("A message should have been shown.", Notify.LastEvent);
			AssertEquals("Some message was shown!", Notify.LastEvent.Message);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

			PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
			Notify.Clear();
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000036", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000043", packageWithNoID_2.KP_PackageID);
			AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
			AssertNull("A message should *not* have been shown.", Notify.LastEvent);
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, dummy.LastSSCCGenerationContext);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateIDs_WithSSCCPrefix_NoTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var packageJob = factory.NewWithValidTestData<PkgPackageJob>();
				packageJob.KJ_JobID = "P00000001";

				var dummy = factory.New<DummyWithPacking>();
				dummy.SSCCPrefix = "2111112";
				dummy.ShouldShowMessagesInGetSSCCPrefix = true;
				dummy.JobNoForPackingParent = "D00001000";
				packageJob.KJ_ParentID = dummy.PK;
				packageJob.KJ_ParentTableCode = dummy.TablePrefix;

				var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
				var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("CTN");
				var container = packageJob.Packages.AddNew("CNT");
				container.Container.K0_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000012", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000029", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertNotNull("A message should have been shown.", Notify.LastEvent);
				AssertEquals("Some message was shown!", Notify.LastEvent.Message);
				AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

				PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
				Notify.Clear();
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000036", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000043", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertNull("A message should *not* have been shown.", Notify.LastEvent);
				AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, dummy.LastSSCCGenerationContext);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateIDs_WithSSCCPrefix_NoTransaction_SaveError()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var packageJob = factory.NewWithValidTestData<PkgPackageJob>();
				packageJob.KJ_JobID = "P00000001";

				var dummy = factory.New<DummyWithPacking>();
				dummy.SSCCPrefix = "2111112";
				dummy.ShouldShowMessagesInGetSSCCPrefix = true;
				dummy.JobNoForPackingParent = "D00001000";
				packageJob.KJ_ParentID = dummy.PK;
				packageJob.KJ_ParentTableCode = dummy.TablePrefix;

				var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
				var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("CTN");
				var container = packageJob.Packages.AddNew("CNT");
				container.Container.K0_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				factory.Saving += ThrowException;
				try
				{
					PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser);
				}
				catch (ZCannotSaveException)
				{
				}

				factory.Saving -= ThrowException;

				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000012", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000029", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertNotNull("A message should have been shown.", Notify.LastEvent);
				AssertEquals("Some message was shown!", Notify.LastEvent.Message);
				AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

				PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
				Notify.Clear();
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000036", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000043", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertNull("A message should *not* have been shown.", Notify.LastEvent);
				AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, dummy.LastSSCCGenerationContext);
			}

			static void ThrowException(BusinessObjectFactory f) => throw new ZCannotSaveException("Test!", "Test!");
		}

		public void TestGenerateIDs_WithSSCCPrefix_SaveError()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			dummy.SSCCPrefix = "2111112";
			dummy.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy.JobNoForPackingParent = "D00001000";
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("CTN");

			Factory.Saving += ThrowException;
			PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.AutoClosingPackage);
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000012", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000029", packageWithNoID_2.KP_PackageID);
			AssertEquals(SSCCGenerationContext.AutoClosingPackage, dummy.LastSSCCGenerationContext);

			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException)
			{
			}

			Factory.Saving -= ThrowException;

			// after failing a save, we should not be able to save this factory as there is no safe way to recover
			AssertExceptionThrown(typeof(ZCannotSaveException), "Package ID Generation failed and the Packages cannot be saved. Retry the operation again.", Factory.Save);

			static void ThrowException(BusinessObjectFactory f) => throw new ZCannotSaveException("Test!", "Test!");
		}

		public void TestGenerateIDs_WithSSCCPrefix_SaveError_NoIDsGenerated()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			dummy.SSCCPrefix = "2111112";
			dummy.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy.JobNoForPackingParent = "D00001000";
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			var container = packageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Saving += ThrowException;
			PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.AutoClosingPackage);
			AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
			AssertEquals(SSCCGenerationContext.AutoClosingPackage, dummy.LastSSCCGenerationContext);

			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException)
			{
			}

			Factory.Saving -= ThrowException;

			// save should succeed as no IDs were generated
			AssertNoExceptionThrown(Factory.Save);

			static void ThrowException(BusinessObjectFactory f) => throw new ZCannotSaveException("Test!", "Test!");
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateIDs_NoTransaction()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var packageJob = factory.NewWithValidTestData<PkgPackageJob>();
				packageJob.KJ_JobID = "P00000001";

				var dummy = factory.New<DummyWithPacking>();
				dummy.JobNoForPackingParent = "D00001000";
				packageJob.KJ_ParentID = dummy.PK;
				packageJob.KJ_ParentTableCode = dummy.TablePrefix;

				var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
				var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("CTN");
				var container = packageJob.Packages.AddNew("CNT");
				container.Container.K0_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-001", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-002", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

				PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
				Notify.Clear();
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-003", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-004", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, dummy.LastSSCCGenerationContext);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestGenerateIDs_NoTransaction_SaveError()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var packageJob = factory.NewWithValidTestData<PkgPackageJob>();
				packageJob.KJ_JobID = "P00000001";

				var dummy = factory.New<DummyWithPacking>();
				dummy.JobNoForPackingParent = "D00001000";
				packageJob.KJ_ParentID = dummy.PK;
				packageJob.KJ_ParentTableCode = dummy.TablePrefix;

				var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
				var packageWithNoID_2 = packageWithNoID_1.Packages.AddNew("CTN");
				var container = packageJob.Packages.AddNew("CNT");
				container.Container.K0_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				factory.Saving += ThrowException;
				try
				{
					PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser);
				}
				catch (ZCannotSaveException)
				{
				}

				factory.Saving -= ThrowException;

				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-001", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-002", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

				PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
				Notify.Clear();
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-003", packageWithNoID_1.KP_PackageID);
				AssertEquals("The PackageJob parent Job No should be used.", "D00001000-004", packageWithNoID_2.KP_PackageID);
				AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
				AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, dummy.LastSSCCGenerationContext);
			}

			static void ThrowException(BusinessObjectFactory f) => throw new ZCannotSaveException("Test!", "Test!");
		}

		public void TestGenerateIDs_MultiplePackages_NumberFountainVisitedOnce()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_3 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_4 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_5 = packageJob.Packages.AddNew("PLT");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package has correct package id.", "P00000001-001", packageWithNoID_1.KP_PackageID);
			AssertEquals("Package has correct package id.", "P00000001-002", packageWithNoID_2.KP_PackageID);
			AssertEquals("Package has correct package id.", "P00000001-003", packageWithNoID_3.KP_PackageID);
			AssertEquals("Package has correct package id.", "P00000001-004", packageWithNoID_4.KP_PackageID);
			AssertEquals("Package has correct package id.", "P00000001-005", packageWithNoID_5.KP_PackageID);

			AssertEquals("Number fountain visited only once (GetNexts).", 1, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestGenerateIDs_MultiplePackages_NumberFountainVisitedOnce_WithExistingPackageId()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_3 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_4 = packageJob.Packages.AddNew("PLT");
			var packageWithID = packageJob.Packages.AddNew("PLT");
			packageWithID.KP_PackageID = "P00000001-002";

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(new[] { packageWithNoID_1, packageWithNoID_2, packageWithNoID_3, packageWithNoID_4 }, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package has correct package id.", "P00000001-001", packageWithNoID_1.KP_PackageID);
			AssertEquals("Package has correct package id.", "P00000001-003", packageWithNoID_2.KP_PackageID);
			AssertEquals("Package has correct package id.", "P00000001-004", packageWithNoID_3.KP_PackageID);
			AssertEquals("Package has correct package id.", "P00000001-005", packageWithNoID_4.KP_PackageID);
			AssertEquals("Package has same package id.", "P00000001-002", packageWithID.KP_PackageID);

			AssertEquals("Number fountain visited twice only (2xGetNexts).", 2, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestGenerateIDs_WithSSCCPrefix_MultiplePackages_NumberFountainVisitedOnce()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			dummy.SSCCPrefix = "2111112";
			dummy.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy.JobNoForPackingParent = "D00001000";
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageJob.Packages.AddNew("CTN");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package has correct package id.", "021111120000000012", packageWithNoID_1.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000029", packageWithNoID_2.KP_PackageID);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

			AssertEquals("Number fountain visited once only.", 1, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestGenerateIDs_WithSSCCPrefix_MultiplePackagesWithChildPackage_NumberFountainVisitedOnce()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			dummy.SSCCPrefix = "2111112";
			dummy.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy.JobNoForPackingParent = "D00001000";
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_1b = packageWithNoID_1.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageJob.Packages.AddNew("CTN");
			var packageWithNoID_2b = packageWithNoID_2.Packages.AddNew("CTN");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package has correct package id.", "021111120000000012", packageWithNoID_1.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000029", packageWithNoID_1b.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000036", packageWithNoID_2.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000043", packageWithNoID_2b.KP_PackageID);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

			AssertEquals("Number fountain visited once only.", 1, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestGenerateIDs_WithSSCCPrefix_ChildPackageWithChildPackages_NumberFountainVisitedOnce()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			dummy.SSCCPrefix = "2111112";
			dummy.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy.JobNoForPackingParent = "D00001000";
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			var packageWithNoID_1 = packageJob.Packages.AddNew("PLT");
			var packageWithNoID_1a = packageWithNoID_1.Packages.AddNew("PLT");
			var packageWithNoID_1ab = packageWithNoID_1a.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageJob.Packages.AddNew("CTN");
			var packageWithNoID_2a = packageWithNoID_2.Packages.AddNew("CTN");
			var packageWithNoID_2ab = packageWithNoID_2a.Packages.AddNew("CTN");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package has correct package id.", "021111120000000012", packageWithNoID_1.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000029", packageWithNoID_1a.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000036", packageWithNoID_1ab.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000043", packageWithNoID_2.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000050", packageWithNoID_2a.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000067", packageWithNoID_2ab.KP_PackageID);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

			AssertEquals("Number fountain visited once only.", 1, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestGenerateIDs_WithSSCCPrefix_MultiplePackageJobs_NumberFountainVisitedOncePerJob()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			packageJob1.KJ_JobID = "P00000001";

			var dummy1 = Factory.New<DummyWithPacking>();
			dummy1.SSCCPrefix = "2111112";
			dummy1.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy1.JobNoForPackingParent = "D00001000";
			packageJob1.KJ_ParentID = dummy1.PK;
			packageJob1.KJ_ParentTableCode = dummy1.TablePrefix;

			var packageJob2 = Factory.New<PkgPackageJob>();
			packageJob2.KJ_JobID = "P00000002";

			var dummy2 = Factory.New<DummyWithPacking>();
			dummy2.SSCCPrefix = "3111113";
			dummy2.ShouldShowMessagesInGetSSCCPrefix = true;
			dummy2.JobNoForPackingParent = "D00002000";
			packageJob2.KJ_ParentID = dummy2.PK;
			packageJob2.KJ_ParentTableCode = dummy2.TablePrefix;

			var packageWithNoID_1 = packageJob1.Packages.AddNew("PLT");
			var packageWithNoID_1b = packageWithNoID_1.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageJob2.Packages.AddNew("CTN");
			var packageWithNoID_2b = packageWithNoID_2.Packages.AddNew("CTN");

			var packagesToTest = packageJob2.Packages.Concat(packageJob1.Packages);
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packagesToTest, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package has correct package id.", "021111120000000012", packageWithNoID_1.KP_PackageID);
			AssertEquals("Package has correct package id.", "021111120000000029", packageWithNoID_1b.KP_PackageID);
			AssertEquals("Package has correct package id.", "031111130000000010", packageWithNoID_2.KP_PackageID);
			AssertEquals("Package has correct package id.", "031111130000000027", packageWithNoID_2b.KP_PackageID);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy1.LastSSCCGenerationContext);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy2.LastSSCCGenerationContext);

			AssertEquals("Number fountain visited once per job.", 2, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestGenerateIDs_MultiplePackageJobs_OneWithError()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			packageJob1.KJ_JobID = "P00000001";

			var packageWithNoID_1 = packageJob1.Packages.AddNew("PLT");
			var packageWithNoID_2 = packageJob1.Packages.AddNew("BAG");

			var packageJob2 = Factory.New<PkgPackageJob>();
			packageJob2.KJ_JobID = "P00000005";
			var dummy = Factory.New<DummyWithPacking>();
			packageJob2.KJ_ParentID = dummy.PK;
			packageJob2.KJ_ParentTableCode = dummy.TablePrefix;
			var packageWithDummy = packageJob2.Packages.AddNew("BOX");

			var packagesToTest = packageJob2.Packages.Concat(packageJob1.Packages);
			CombineAssertions(() =>
			{
				AssertEquals(false, PackageIDGenerator.GenerateIDsForAllPackages(packagesToTest, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("No id should be generated for because one the supplied PackageJobs has an error.", string.Empty, packageWithNoID_1.KP_PackageID);
				AssertEquals("No id should be generated for because one the supplied PackageJobs has an error.", string.Empty, packageWithNoID_2.KP_PackageID);
				AssertEquals("No id should be generated for dummy", string.Empty, packageWithDummy.KP_PackageID);
				AssertEquals("The Dummy does not yet have a Job Number. Try saving the Dummy first.", Notify.LastEvent.Message);
				AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
				AssertEquals(null, dummy.LastSSCCGenerationContext);
			});
		}

		public void TestGenerateIDs_DoesNotGenerateForContainers()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			// two packages with empty IDs

			var container = packageJob.Packages.AddNew("CNT");
			container.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var pallet = container.Packages.AddNew("PLT");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Package ID's should *not* be generated for Containers.", true, container.KP_PackageID.IsEmpty);
			AssertEquals("The PackageJob has no parent (eg WhsDocket), the PackageJob's JobNo should be used.", "P00000001-001", pallet.KP_PackageID);
			AssertNull("No message should have been shown.", Notify.LastEvent);
		}

		public void TestGenerateIDs_ExistingPackageIDsFromLegacyGenerationMethod()
		{
			// Old generation method did not use a number fountain for non SSCC package IDs, instead it checked all existing package IDs and incremented them each time
			// This tests that the new number fountain method will not result in duplicated package IDs, and will silently let the number fountain "catch up"
			// if a package job has IDs generated with the old method and the new registry setting has not been overriden. 

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			// no packages

			PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser);
			AssertEquals("No packages were selected.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			Notify.Clear();

			// two packages with empty IDs

			var packageWithIDAlreadySet = packageJob.Packages.AddNew("PLT");
			packageWithIDAlreadySet.KP_PackageID = "P00000001-001";
			Factory.Save();

			var packageWithNoIDSet = packageJob.Packages.AddNew("PLT");
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));

			AssertEquals("PackageID should not have changed.", "P00000001-001", packageWithIDAlreadySet.KP_PackageID);
			AssertEquals("PackageID should not have been reset.", false, packageWithIDAlreadySet.HasChanges);
			AssertEquals("Number fountain should have \"caught up\" to the existing package IDs.", "P00000001-002", packageWithNoIDSet.KP_PackageID);
			AssertEquals("packageWithNoIDSet should have been saved.", false, packageWithNoIDSet.HasChanges);
			AssertNull("No message should have been shown.", Notify.LastEvent);
		}

		#region TestGenerateIDs_WithSaveInTransactionActionInFactory

		public void TestGenerateIDs_WithSaveInTransactionActionInFactory()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			var package = packageJob.Packages.AddNew("PLT", 1);
			Factory.Save();

			Factory.SaveInTransactionActions.Add(new DummyTransactionParticipant(Factory));
			AssertNoExceptionThrown(() => PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
		}

		class DummyTransactionParticipant : SaveInTransactionActionWithFactory
		{
			public DummyTransactionParticipant(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override IChangedTableNames SaveInTransaction()
			{
				return ChangedTableNames.Empty;
			}
		}

		#endregion

		#region TestGenerateIDs_Concurrency

		[TestDate(2014, 12, 16, 12, 30, 35)]
		public void TestGenerateIDs_Concurrency()
		{
			Factory.RefreshEnabled = false;
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;
			var package = packageJob.Packages.AddNew("PLT");
			ConcurrencyInfo.SetConcurrencyPolicy(package, nameof(PkgPackage.KP_KPH_PackageHeader), ConcurrencyPolicy.Strict);
			Factory.Save();

			PkgPackage package_NewFactory = null;
			Factory.Saving += delegate
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var packageJob_NewFactory = newFactory.Load<PkgPackageJob>(packageJob.PK);
				package_NewFactory = newFactory.Load<PkgPackage>(package.PK);
				package_NewFactory.KP_PackageID = "SOMEPACKAGEID";
				newFactory.Save();
			};

			AssertEquals("Should fail save Generation.", false, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));

			AssertEquals("Should pass.", "SOMEPACKAGEID", package_NewFactory.KP_PackageID);
			AssertEquals("Should have new number, but fail save.", "P00000001-001", package.KP_PackageID);
			AssertMultilineASCIIEquals("Should fail with error.", @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
Package P00000001-001 (CargoWise Support @ 16 Dec 2014 12:30:00)
	Package Sequence
	Package Header (Critical change)".Trim(), Notify.LastEvent.Message.Trim());
			Notify.Clear();
		}

		#endregion

		#endregion

		#region Test Number Generation

		public void TestNumberGeneration()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				var customisation1 = CreateCustomisations(clientCodedPrefix: "AA");
				AddCustomisationToRegistry(customisation1);
				AssertGeneratedJobIDFromFountain("AA000001");
				AssertGeneratedJobIDFromFountain("AA000002");
				AssertGeneratedJobIDFromFountain("AA000003");

				var customisation2 = CreateCustomisations(clientCodedPrefix: "BB");
				AddCustomisationToRegistry(customisation2);
				AssertGeneratedJobIDFromFountain("BB000001");
				AssertGeneratedJobIDFromFountain("BB000002");
				AssertGeneratedJobIDFromFountain("BB000003");

				var customisation3 = CreateCustomisations(clientCodedPrefix: "TEST");
				AddCustomisationToRegistry(customisation3);
				AssertEquals("00000001", Env.NumberFountains.GetPackageIDGeneratorFountain("TEST").GetNextFormatted(Factory));
				AssertGeneratedJobIDFromFountain("TEST000002");
			}
		}

		void AssertGeneratedJobIDFromFountain(string expect)
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var dummy = Factory.New<DummyWithPacking>();
			packageJob.KJ_ParentID = dummy.PK;
			var pkg = packageJob.Packages.AddNew("PLT");

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("There is no Parent Job as Table Code is not set.", null, dummy.LastSSCCGenerationContext);
		}

		BillOfLadingNumberCustomisation CreateCustomisations(string clientCodedPrefix)
		{
			var customisation = new BillOfLadingNumberCustomisation();
			foreach (BillOfLadingNumberCustomisationElement element in customisation.Elements)
			{
				switch (element.Key)
				{
					case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
						element.Include = true;
						element.Detail = "6";
						element.Order = 50;
						break;

					case BillOfLadingNumberCustomisationElement.Keys.ClientCoded1:
						element.Include = !string.IsNullOrEmpty(clientCodedPrefix);
						element.Detail = clientCodedPrefix;
						element.Fountain = true;
						element.Order = 1;
						break;

					default:
						element.Include = false;
						break;
				}
			}

			return customisation;
		}

		void AddCustomisationToRegistry(BillOfLadingNumberCustomisation customisation)
		{
			PackingRegistry.Instance.PackageIDCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);
		}

		#endregion

		#region TestClearIDs

		public void TestClearIDs()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			// no packages

			PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
			AssertEquals("No packages were selected.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			Notify.Clear();

			// packages

			var package = packageJob.Packages.AddNew();
			var childPackage = package.Packages.AddNew();
			package.KP_PackageID = "123";
			childPackage.KP_PackageID = "456";

			PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
			AssertEquals(true, package.KP_PackageID.IsEmpty);
			AssertEquals(true, childPackage.KP_PackageID.IsEmpty);
			AssertNull("No message should have been shown.", Notify.LastEvent);
			Notify.Clear();
		}

		public void TestClearIDs_DoesNotClearContainerIDs()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			// containers only

			var container = packageJob.Packages.AddNew("CNT");
			container.KP_PackageID = "Container123";
			PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);

			AssertEquals("Container123", container.KP_PackageID);
			AssertEquals("All Selected Packages are Containers or have Tracking Numbers. Container should be manually cleared and Tracking Numbers cannot be cleared.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			Notify.Clear();

			// containers and other packages

			var pallet = container.Packages.AddNew("PLT");
			pallet.KP_PackageID = "Pallet123";

			PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
			AssertEquals("Container123", container.KP_PackageID);
			AssertEquals(true, pallet.KP_PackageID.IsEmpty);
			AssertNull("No message should have been shown.", Notify.LastEvent);
			Notify.Clear();
		}

		public void TestClearIDs_DoesNotClearTrackingNumbers()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			// is sent to RTUS

			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "123";
			package.IsSentToRTUS = true;
			PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);

			AssertEquals("123", package.KP_PackageID);
			AssertEquals("All Selected Packages are Containers or have Tracking Numbers. Container should be manually cleared and Tracking Numbers cannot be cleared.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			Notify.Clear();

			// one package hasn't been sent to RTUS

			var pallet = packageJob.Packages.AddNew("PLT");
			pallet.KP_PackageID = "Pallet123";

			PackageIDGenerator.ClearIDs(packageJob.Packages, Notify);
			AssertEquals("123", package.KP_PackageID);
			AssertEquals(true, pallet.KP_PackageID.IsEmpty);
			AssertNull("No message should have been shown.", Notify.LastEvent);
			Notify.Clear();
		}

		#endregion

		#region TestNumberGenerationLinksPackageJobToNumberFountain

		public void TestNumberGenerationLinksPackageJobToNumberFountain_JobNumberIncludedInFountain()
		{
			TestNumberGenerationLinksPackageJobToNumberFountainCore(isJobNoIncludedInFountain: true);
		}

		public void TestNumberGenerationLinksPackageJobToNumberFountain_JobNumberNotIncludedInSequence()
		{
			TestNumberGenerationLinksPackageJobToNumberFountainCore(isJobNoIncludedInFountain: false);
		}

		void TestNumberGenerationLinksPackageJobToNumberFountainCore(bool isJobNoIncludedInFountain)
		{
			var packingParent = Helper.CreatePackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);

			packingParent.JobNoForPackingParent = "D01";
			var packableItemParent = Helper.CreatePackableItemParent();
			Helper.CreatePackableItem(packableItemParent, 5m);
			Factory.Save();

			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box);
			package.Pack(packableItemParent, 5m);

			var customisation = PackingRegistry.Instance.PackageIDCustomisation.Value;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Fountain = isJobNoIncludedInFountain;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Fountain = true;

			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation))
			{
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("Number generation should use the Job No and company code.", "D01-EDI001", package.KP_PackageID);
				Factory.Save();
			}

			var numberFountainName = isJobNoIncludedInFountain ? "GeneratorFountain-PKGID-EDID01" : "GeneratorFountain-PKGID-EDI";
			var numberFountainOwner = GetFountainOwner(numberFountainName);
			Assert(numberFountainOwner.HasValue);
			AssertEquals(isJobNoIncludedInFountain ? packageJob.PK : Guid.Empty, numberFountainOwner.Value);
		}

		public void TestNumberGenerationLinksPackageJobToNumberFountain_JobNoNotIncludedInCustomisation()
		{
			var packingParent = Helper.CreatePackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);

			packingParent.JobNoForPackingParent = "D01";
			var packableItemParent = Helper.CreatePackableItemParent();
			Helper.CreatePackableItem(packableItemParent, 5m);
			Factory.Save();

			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box);
			package.Pack(packableItemParent, 5m);

			var customisation = PackingRegistry.Instance.PackageIDCustomisation.Value;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Include = false;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Fountain = false;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Fountain = true;

			var companyCode = Env.CurrentCompany.Code;
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation))
			{
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("Number generation should use the company code and branch code.", $"-{companyCode}001", package.KP_PackageID);
				Factory.Save();
			}

			var numberFountainName = $"GeneratorFountain-PKGID-{companyCode}";
			var numberFountainOwner = GetFountainOwner(numberFountainName);
			Assert(numberFountainOwner.HasValue);
			AssertEquals(Guid.Empty, numberFountainOwner.Value);
		}

		public void TestNumberGenerationLinksPackageJobToNumberFountain_HandlingUnit_JobNoIncludedInCustomisation()
		{
			TestNumberGenerationLinksPackageJobToNumberFountain_HandlingUnitCore(jobNumberIncluded: true);
		}

		public void TestNumberGenerationLinksPackageJobToNumberFountain_HandlingUnit_JobNoNotIncludedInCustomisation()
		{
			TestNumberGenerationLinksPackageJobToNumberFountain_HandlingUnitCore(jobNumberIncluded: false);
		}

		void TestNumberGenerationLinksPackageJobToNumberFountain_HandlingUnitCore(bool jobNumberIncluded)
		{
			var hu1 = Factory.NewWithValidTestData<PkgHandlingUnit>();
			var hu2 = Factory.NewWithValidTestData<PkgHandlingUnit>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(hu1);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(hu2);

			var packableItemParent1 = Helper.CreatePackableItemParent();
			Helper.CreatePackableItem(packableItemParent1, 5m);
			var packableItemParent2 = Helper.CreatePackableItemParent();
			Helper.CreatePackableItem(packableItemParent2, 5m);
			Factory.Save();

			var package1 = Helper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box);
			package1.Pack(packableItemParent1, 5m);
			var package2 = Helper.CreatePackage(packageJob2, 1, Constants.PkgUnit.Box);
			package2.Pack(packableItemParent2, 5m);

			var customisation = PackingRegistry.Instance.PackageIDCustomisation.Value;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Include = jobNumberIncluded;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Fountain = jobNumberIncluded;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Fountain = true;

			var companyCode = Env.CurrentCompany.Code;
			var jobNumber = jobNumberIncluded ? "HU" : "";
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation))
			{
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob1.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("Number generation should use the company code, and HU if jobNumberIncluded.", $"{jobNumber}-{companyCode}001", package1.KP_PackageID);
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob2.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("Number generation should use the company code, and HU if jobNumberIncluded.", $"{jobNumber}-{companyCode}002", package2.KP_PackageID);
				Factory.Save();
			}

			var numberFountainName = $"GeneratorFountain-PKGID-{companyCode}{jobNumber}";
			var numberFountainOwner = GetFountainOwner(numberFountainName);
			Assert(numberFountainOwner.HasValue);
			AssertEquals(Guid.Empty, numberFountainOwner.Value);
		}

		Guid? GetFountainOwner(string name)
		{
			var sql = "SELECT SN_Owner FROM dbo.StmNums WHERE SN_Name = @name";
			var command = ((IDbConnected)Factory).Connection.Command(sql);
			command.AddParameter("@name", SqlDbType.VarChar, name);

			return (Guid?)command.ExecuteScalar();
		}

		#endregion
	}
}
