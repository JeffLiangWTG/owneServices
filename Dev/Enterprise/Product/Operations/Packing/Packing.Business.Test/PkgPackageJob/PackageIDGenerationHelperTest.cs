using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageIDGenerationHelperTest : PackingTestCaseWithFactory
	{
		#region TestOnSave_GeneratePackageIDs

		#region TestOnFactorySave_ShouldGenerateIDOnSaving_DeterminesPackageIDGeneration

		public void TestOnFactorySave_ShouldGenerateIDOnSaving_DeterminesPackageIDGeneration()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			packageJob.KJ_JobID = "P00000001";

			ISupportPackageIDGeneration package1 = packageJob.Packages.AddNew();
			ISupportPackageIDGeneration package2 = packageJob.Packages.AddNew();
			AssertEquals("Before save should have two Packages", 2, packageJob.Packages.Count);
			AssertEquals("Before save should one have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("Before save should one have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Before save package1 ShouldGenerateIDOnSaving should be false", false, package1.ShouldGenerateIDOnSaving);
			AssertEquals("Before save package2 ShouldGenerateIDOnSaving should be false", false, package2.ShouldGenerateIDOnSaving);

			var id1 = "unset";
			var id2 = "unset";
			package1.AfterIDGenerated += (s, e) => id1 = package1.KP_PackageID;
			package2.AfterIDGenerated += (s, e) => id2 = package2.KP_PackageID;

			Factory.Save();
			AssertEquals("After save should still have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should still have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Should not have set ID1", "unset", id1);
			AssertEquals("Should not have set ID2", "unset", id2);

			package1.ShouldGenerateIDOnSaving = true;
			Factory.Save();
			AssertEquals("After save should have package1 with PackageID", false, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Should have set ID1", package1.KP_PackageID, id1);
			AssertEquals("Should not have set ID2", "unset", id2);
		}

		public void TestOnFactorySave_ShouldGenerateIDOnSaving_UsesCorrectSSCCGenerationContext()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";

			var dummy = Factory.New<DummyWithPacking>();
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;
			dummy.JobNoForPackingParent = "D00000001";

			ISupportPackageIDGeneration package1 = packageJob.Packages.AddNew();
			ISupportPackageIDGeneration package2 = packageJob.Packages.AddNew();
			var pkgHeader = packageJob.LoosePackageIDs.AddNew();
			pkgHeader.CurrentPackageJob = packageJob;
			ISupportPackageIDGeneration supportPackageIDGeneration = pkgHeader;
			AssertEquals("Before save should have two Packages", 2, packageJob.Packages.Count);
			AssertEquals("Before save should have one Loose ID", 1, packageJob.LoosePackageIDs.Count);
			AssertEquals("Before save should have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("Before save should have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Before save should have loose ID with empty PackageID", true, supportPackageIDGeneration.KP_PackageID.IsEmpty);
			AssertEquals("Before save package1 ShouldGenerateIDOnSaving should be false", false, package1.ShouldGenerateIDOnSaving);
			AssertEquals("Before save package2 ShouldGenerateIDOnSaving should be false", false, package2.ShouldGenerateIDOnSaving);
			AssertEquals("Before save loose ID ShouldGenerateIDOnSaving should be false", false, supportPackageIDGeneration.ShouldGenerateIDOnSaving);
			AssertEquals(null, dummy.LastSSCCGenerationContext);

			var id1 = "unset";
			var id2 = "unset";
			var id3 = "unset";
			package1.AfterIDGenerated += (s, e) => id1 = package1.KP_PackageID;
			package2.AfterIDGenerated += (s, e) => id2 = package2.KP_PackageID;
			supportPackageIDGeneration.AfterIDGenerated += (s, e) => id3 = supportPackageIDGeneration.KP_PackageID;
			supportPackageIDGeneration.ShouldGenerateIDOnSaving = true;

			Factory.Save();
			AssertEquals("After save should still have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should still have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("After save should have loose ID with PackageID", false, supportPackageIDGeneration.KP_PackageID.IsEmpty);
			AssertEquals("Should not have set ID1", "unset", id1);
			AssertEquals("Should not have set ID2", "unset", id2);
			AssertEquals("Should have set ID3", supportPackageIDGeneration.KP_PackageID, id3);
			AssertEquals(SSCCGenerationContext.GeneratingIDsOnSave, dummy.LastSSCCGenerationContext);

			var loosePackageID = id3;
			package1.ShouldGenerateIDOnSaving = true;
			Factory.Save();
			AssertEquals("After save should have package1 with PackageID", false, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("After save should have loose ID with PackageID", false, supportPackageIDGeneration.KP_PackageID.IsEmpty);
			AssertEquals("Should have set ID1", package1.KP_PackageID, id1);
			AssertEquals("Should not have set ID2", "unset", id2);
			AssertEquals("Should not have changed ID3", loosePackageID, supportPackageIDGeneration.KP_PackageID);
			AssertEquals(SSCCGenerationContext.GeneratingIDsOnSave, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestOnFactorySave_ShouldGenerateIDOnSaving_PackageIDGenerationIgnoresDuplicates

		public void TestOnFactorySave_ShouldGenerateIDOnSaving_PackageIDGenerationIgnoresDuplicates()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			packageJob.KJ_JobID = "P00000001";

			ISupportPackageIDGeneration package = packageJob.Packages.AddNew();
			AssertEquals("Before save it should have one Package", 1, packageJob.Packages.Count);
			AssertEquals("Before save it should one have package with empty PackageID", true, package.KP_PackageID.IsEmpty);
			AssertEquals("Before save package ShouldGenerateIDOnSaving should be false", false, package.ShouldGenerateIDOnSaving);

			Factory.Save();
			AssertEquals("After save should still have package with empty PackageID", true, package.KP_PackageID.IsEmpty);

			package.ShouldGenerateIDOnSaving = true;
			Factory.Save();
			AssertEquals("After save should still have package with PackageID", false, package.KP_PackageID.IsEmpty);

			var packageID = package.KP_PackageID;
			Factory.Save();
			AssertEquals("After save should still have package with PackageID", false, package.KP_PackageID.IsEmpty);
			AssertEquals("After another save should still have package with same PackageID", packageID, package.KP_PackageID);
		}

		#endregion

		#region TestOnFactorySave_ShouldGenerateIDOnSaving_NoPackageIDsGeneratedWhenOneHasError

		public void TestOnFactorySave_ShouldGenerateIDOnSaving_NoPackageIDsGeneratedWhenOneHasError()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "";

			var dummy = Factory.New<DummyWithPacking>();
			packageJob.KJ_ParentID = dummy.PK;
			packageJob.KJ_ParentTableCode = dummy.TablePrefix;

			ISupportPackageIDGeneration package1 = packageJob.Packages.AddNew();
			ISupportPackageIDGeneration package2 = packageJob.Packages.AddNew();
			AssertEquals("Before save should have two Packages", 2, packageJob.Packages.Count);
			AssertEquals("Before save should one have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("Before save should one have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);

			package1.ShouldGenerateIDOnSaving = true;
			AssertEquals("Before save package1 ShouldGenerateIDOnSaving should be true", true, package1.ShouldGenerateIDOnSaving);
			AssertEquals("Before save package2 ShouldGenerateIDOnSaving should be false", false, package2.ShouldGenerateIDOnSaving);

			AssertExceptionThrown<ZCannotSaveException>(
				"ID Generation error should throw cannot save exception",
				@"Package IDs could not be successfully generated. Errors below:
The Dummy does not yet have a Job Number. Try saving the Dummy first.",
				() => Factory.Save());
			AssertEquals("After save should still have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should still have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Error prevents attempt to get SSCC Prefix.", null, dummy.LastSSCCGenerationContext);

			packageJob.KJ_JobID = "";
			dummy.JobNoForPackingParent = "D00000001";
			Factory.Save();
			AssertEquals("After save should still have package1 with PackageID", false, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should still have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Context should be Generating IDs on Save.", SSCCGenerationContext.GeneratingIDsOnSave, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestOnFactorySave_ShouldGenerateIDOnSaving_NoPackageIDsGeneratedWhenSavingFailureOccurs

		public void TestOnFactorySave_ShouldGenerateIDOnSaving_NoPackageIDsGeneratedWhenSavingFailureOccurs()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			packageJob.KJ_JobID = "P00000001";

			ISupportPackageIDGeneration package1 = packageJob.Packages.AddNew();
			ISupportPackageIDGeneration package2 = packageJob.Packages.AddNew();
			package1.ShouldGenerateIDOnSaving = true;
			AssertEquals("Before save it should have two Packages", 2, packageJob.Packages.Count);
			AssertEquals("Before save it should have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("Before save it should have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Before save package1 ShouldGenerateIDOnSaving should be true", true, package1.ShouldGenerateIDOnSaving);
			AssertEquals("Before save package2 ShouldGenerateIDOnSaving should be false", false, package2.ShouldGenerateIDOnSaving);

			void savingEventHandler(BusinessObjectFactory factory)
			{
				throw new ZCannotSaveException("Cannot save now", "Testing exception");
			}
			Factory.Saving += savingEventHandler;
			AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());
			AssertEquals("After save should still have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should still have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);

			Factory.Saving -= savingEventHandler;
			Factory.Save();
			AssertEquals("After save should have package1 with PackageID", false, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
		}

		#endregion

		#region TestOnFactorySave_ShouldGenerateIDOnSaving_ShouldClearOnSaveFailure

		public void TestOnFactorySave_ShouldGenerateIDOnSaving_ShouldClearOnSaveFailure()
		{
			var preventGenerationService = new PreventGenerationService();
			Factory.ServiceContainer.AddAfterOnSavingService(preventGenerationService);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			packageJob.KJ_JobID = "P00000001";

			ISupportPackageIDGeneration package1 = packageJob.Packages.AddNew();
			ISupportPackageIDGeneration package2 = packageJob.Packages.AddNew();
			package1.ShouldGenerateIDOnSaving = true;
			package2.ShouldGenerateIDOnSaving = true;
			AssertEquals("Before save it should have two Packages", 2, packageJob.Packages.Count);
			AssertEquals("Before save it should have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("Before save it should have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Before save package1 ShouldGenerateIDOnSaving should be true", true, package1.ShouldGenerateIDOnSaving);
			AssertEquals("Before save package2 ShouldGenerateIDOnSaving should be false", true, package2.ShouldGenerateIDOnSaving);

			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException ex)
			{
				AssertEquals("Exception caught should be expected one.", "Cannot save for test", ex.Message);
			}
			AssertEquals("After save should still have package1 with empty PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should still have package2 with empty PackageID", true, package2.KP_PackageID.IsEmpty);
			Factory.ServiceContainer.RemoveAfterOnSavingService<PreventGenerationService>();

			package1.ShouldGenerateIDOnSaving = false;
			Factory.Save();
			AssertEquals("Package1 ShouldGenerateIDOnSaving should be false", false, package1.ShouldGenerateIDOnSaving);
			AssertEquals("After save should have package1 with No PackageID", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("After save should have package2 with PackageID", false, package2.KP_PackageID.IsEmpty);
		}

		#endregion

		#region TestOnFactorySave_ShouldGenerateIDOnSaving_PackageHeader

		public void TestOnFactorySave_ShouldGenerateIDOnSaving_PackageHeader()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			packageJob.KJ_JobID = "P00000001";
			var pkgHeader = packageJob.LoosePackageIDs.AddNew();
			pkgHeader.CurrentPackageJob = packageJob;

			ISupportPackageIDGeneration iPackageHeader = pkgHeader;
			AssertEquals("Before save should have pkgHeader with empty PackageID", true, pkgHeader.KPH_PackageID.IsEmpty);
			AssertEquals("Before save should have pkgHeader with empty PackageID through interface", true, iPackageHeader.KP_PackageID.IsEmpty);
			AssertEquals("Before save iPackageHeader ShouldGenerateIDOnSaving should be false", false, iPackageHeader.ShouldGenerateIDOnSaving);

			var id = "unset";
			iPackageHeader.AfterIDGenerated += (s, e) => id = iPackageHeader.KP_PackageID;

			iPackageHeader.ShouldGenerateIDOnSaving = true;
			Factory.Save();
			AssertEquals("After save should have PackageHeader with PackageID", false, pkgHeader.KPH_PackageID.IsEmpty);
			AssertEquals("After save should have PackageHeader with PackageID through interface", false, iPackageHeader.KP_PackageID.IsEmpty);
			AssertEquals("After save should have PackageHeader with Create Time.", false, pkgHeader.KPH_SystemCreateTimeUtc.IsEmpty);
			AssertEquals("After save should have PackageHeader with Create User.", false, pkgHeader.KPH_SystemCreateUser.IsEmpty);
			AssertEquals("After save should have PackageHeader with Last Edit Time.", false, pkgHeader.KPH_SystemLastEditTimeUtc.IsEmpty);
			AssertEquals("After save should have PackageHeader with Last Edit User.", false, pkgHeader.KPH_SystemLastEditUser.IsEmpty);
			AssertEquals("Should have set ID", iPackageHeader.KP_PackageID, id);
		}

		#endregion

		#region TestShouldGenerateIDOnSaving_ShouldClearOnSaveFailure_PackageHeader

		[ExpectNoExceptions]
		public void TestShouldGenerateIDOnSaving_ShouldClearOnSaveFailure_PackageHeader()
		{
			var preventGenerationService = new PreventGenerationService();
			Factory.ServiceContainer.AddAfterOnSavingService(preventGenerationService);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_JobID = "P00000001";
			var pkgHeader1 = packageJob.LoosePackageIDs.AddNew();
			pkgHeader1.CurrentPackageJob = packageJob;

			ISupportPackageIDGeneration iPackageHeader1 = pkgHeader1;
			iPackageHeader1.ShouldGenerateIDOnSaving = true;

			AssertEquals("Before save should have pkgHeader1 with empty PackageID through interface", true, iPackageHeader1.KP_PackageID.IsEmpty);
			AssertEquals("Before save iPackageHeader1 ShouldGenerateIDOnSaving should be true", true, iPackageHeader1.ShouldGenerateIDOnSaving);

			try
			{
				Factory.Save();
			}
			catch (ZCannotSaveException ex)
			{
				AssertEquals("Exception caught should be expected one.", "Cannot save for test", ex.Message);
			}
			AssertEquals("After save should still have iPackageHeader1 with empty PackageID", true, iPackageHeader1.KP_PackageID.IsEmpty);
			Factory.ServiceContainer.RemoveAfterOnSavingService<PreventGenerationService>();

			iPackageHeader1.ShouldGenerateIDOnSaving = false;
			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The INSERT statement conflicted with the CHECK constraint \"Constraint_KPH_PackageId\"", true), "No Package ID should be generated and empty ID constraint should throw.");
		}

		#endregion

		#endregion

		#region Implementation

		internal class PreventGenerationService : IAfterOnSavingBOProcessingService
		{
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				throw new ZCannotSaveException("Cannot save for test", "Cannot save");
			}
		}

		#endregion
	}
}
