using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Testing;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageHeaderIDBaseGeneratorTest : PackingTestCaseWithFactory
	{
		#region TestGenerateIDs

		public void TestGenerateIDs()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;

			// no packages

			PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsViaUser);
			AssertEquals("No packages were selected.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			Notify.Clear();

			// packages that already have IDs

			var packageWithID = GetNewPackageHeaderForIDGeneration(packageJob);
			packageWithID.KP_PackageID = "ID-123";

			AssertEquals(false, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("Existing Package ID should not have been overwritten.", "ID-123", packageWithID.KP_PackageID);
			AssertEquals("Package IDs can only be created for non-Container Packages that have no Package ID and a Qty of one (1).", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Warning, Notify.LastEvent.Type);
			Notify.Clear();

			// two packages with empty IDs

			var packageWithNoID_1 = GetNewPackageHeaderForIDGeneration(packageJob);
			var packageWithNoID_2 = GetNewPackageHeaderForIDGeneration(packageJob);

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
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

			AssertEquals(false, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("The Dummy does not yet have a Job Number. Try saving the Dummy first.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			AssertEquals(null, dummy.LastSSCCGenerationContext);
			Notify.Clear();

			// two packages with empty IDs - parent job is attached and saved (has a job ID)

			packageJob.KJ_JobID = "";
			dummy.JobNoForPackingParent = "D00000001";
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("The PackageJob has a parent (Dummy), the Dummy's JobNo should be used.", "D00000001-001", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob has a parent (Dummy), the Dummy's JobNo should be used.", "D00000001-002", packageWithNoID_2.KP_PackageID);
			AssertNull("No message should have been shown.", Notify.LastEvent);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
			Notify.Clear();

			// two packages, one with an empty ID - make sure the next ID is unique

			packageWithNoID_2.KP_PackageID = "";
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsOnSave));
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
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("Number generation should use the Packing Registry suffix length.", "D00000002-00001", packageWithNoID_1.KP_PackageID);
				AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);
			}
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

			var packageWithNoID_1 = GetNewPackageHeaderForIDGeneration(packageJob);
			var packageWithNoID_2 = GetNewPackageHeaderForIDGeneration(packageJob);

			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000012", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000029", packageWithNoID_2.KP_PackageID);
			AssertNotNull("A message should have been shown.", Notify.LastEvent);
			AssertEquals("Some message was shown!", Notify.LastEvent.Message);
			AssertEquals(SSCCGenerationContext.GeneratingIDsViaUser, dummy.LastSSCCGenerationContext);

			PackageIDGenerator.ClearIDs(packageJob.LoosePackageIDs, Notify);
			Notify.Clear();
			AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.LoosePackageIDs, Notify, SSCCGenerationContext.CheckIfBarcodeIsSSCC));
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000036", packageWithNoID_1.KP_PackageID);
			AssertEquals("The PackageJob parent SSCC Prefix should be used.", "021111120000000043", packageWithNoID_2.KP_PackageID);
			AssertNull("A message should *not* have been shown.", Notify.LastEvent);
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestClearIDs

		public void TestClearIDs()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			// no packages

			PackageIDGenerator.ClearIDs(packageJob.LoosePackageIDs, Notify);
			AssertEquals("No packages were selected.", Notify.LastEvent.Message);
			AssertEquals(NotificationType.Error, Notify.LastEvent.Type);
			Notify.Clear();

			// packages

			var package = GetNewPackageHeaderForIDGeneration(packageJob);
			package.KP_PackageID = "123";

			PackageIDGenerator.ClearIDs(packageJob.LoosePackageIDs, Notify);
			AssertEquals(true, package.KP_PackageID.IsEmpty);
			AssertNull("No message should have been shown.", Notify.LastEvent);
			Notify.Clear();
		}

		#endregion

		#region Implementation

		ISupportPackageIDGeneration GetNewPackageHeaderForIDGeneration(PkgPackageJob packageJob)
		{
			var pkgHeader = packageJob.LoosePackageIDs.AddNew();
			pkgHeader.CurrentPackageJob = packageJob;
			return pkgHeader;
		}

		#endregion
	}
}
