using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	abstract class WhsReleaseSecureServiceTestCase : WhsSecureServiceTestCase
	{
		#region Helper for Release Tests

		#region TestReleaseJobForReference

		protected void TestReleaseJobForReference(WhsSecureService webService, string searchedReference, bool releaseAction, PkgPackageJob expectedFoundPackageJob, WhsWarehouse warehouse)
		{
			var expectedPackageJobs = new List<PkgPackageJob> { expectedFoundPackageJob };
			TestReleaseJobForReference(webService, searchedReference, releaseAction, expectedPackageJobs, warehouse);
		}

		protected void TestReleaseJobForReference(WhsSecureService webService, string searchedReference, bool releaseAction, List<PkgPackageJob> expectedPackageJobs, WhsWarehouse warehouse)
		{
			TestReleaseJobForReferenceCore(webService, searchedReference, releaseAction, expectedPackageJobs);
		}

		void TestReleaseJobForReferenceCore(WhsSecureService webService, string searchedReference, bool release, List<PkgPackageJob> expectedPackageJobs)
		{
			CombineAssertions(() =>
			{
				foreach (var expectedPackageJob in expectedPackageJobs)
				{
					if (release == expectedPackageJob.KJ_ReleasedTimeUtc.IsValid)
					{
						expectedPackageJob.KJ_ReleasedTimeUtc = !release ? ZDateTime.UtcNow : ZDateTime.Empty;
						expectedPackageJob.KJ_GS_NKReleasedBy = !release ? Env.CurrentUser.Initials : string.Empty;
						expectedPackageJob.Factory.Save();
					}
					AssertEquals("Precondition", !release, expectedPackageJob.IsReleased);
				}
			});

			PackageChoiceInfo[] actualPackageJobs;

			if (release)
			{
				var response = webService.Release_ReleaseJobForReference(searchedReference);
				actualPackageJobs = response.PackageChoices.ToArray();
				AssertSuccessfulResponse(response, webService);
			}
			else
			{
				var response = webService.Release_UnreleaseJobForReference(searchedReference);
				actualPackageJobs = response.PackageChoices.ToArray();
				AssertSuccessfulResponse(response, webService);
			}

			AssertEquals("Count", expectedPackageJobs.Count, actualPackageJobs.Length);
			CombineAssertions(() =>
			{
				foreach (var expectedPackageJob in expectedPackageJobs)
				{
					var actualPackage = actualPackageJobs.FirstOrDefault(p => p.JobId.Equals(expectedPackageJob.KJ_JobID));
					AssertNotNull($"Should contain {expectedPackageJob.KJ_JobID}", actualPackage);
				}
			});

			if (expectedPackageJobs.Count == 1)
			{
				AssertEquals($"Should be {(release ? "released" : "unreleased")}", release, expectedPackageJobs[0].IsReleased);
				CombineAssertions(() =>
				{
					foreach (var package in expectedPackageJobs[0].Packages)
					{
						AssertEquals($"Should be {(release ? "released via job" : "unreleased")}", release, package.KP_IsReleasedViaJob);
					}
				});
			}
		}

		#endregion

		#region AssertReleaseJobForReference_NotFound

		protected void AssertReleaseJobForReference_NotFound(WhsSecureService webService, string searchedReference, bool release)
		{
			var response = release ? webService.Release_ReleaseJobForReference(searchedReference) : webService.Release_UnreleaseJobForReference(searchedReference);
			AssertBusinessValidationError(webService, string.Format("Reference '{0}' not found!", searchedReference), response);
		}

		#endregion

		#region TestReleasePackageForReference

		protected void TestReleasePackageForReference(WhsSecureService webService, string searchedReference, bool releaseAction, PkgPackage expectedPackage, WhsWarehouse warehouse)
		{
			var expectedPackages = new List<PkgPackage> { expectedPackage };
			TestReleasePackageForReference(webService, searchedReference, releaseAction, expectedPackages, warehouse);
		}

		protected void TestReleasePackageForReference(WhsSecureService webService, string searchedReference, bool releaseAction, List<PkgPackage> expectedPackages, WhsWarehouse warehouse)
		{
			TestReleasePackageForReferenceCore(webService, searchedReference, releaseAction, expectedPackages);
		}

		void TestReleasePackageForReferenceCore(WhsSecureService webService, string searchedReference, bool release, List<PkgPackage> expectedPackages)
		{
			CombineAssertions(() =>
			{
				foreach (var package in expectedPackages)
				{
					if (release == package.IsReleased)
					{
						ReleaseOrUnreleasePackage(!release, package);
						package.Factory.Save();
					}
					AssertEquals("Precondition", !release, package.IsReleased);
				}
			});

			PackageChoiceInfo[] actualPackages;

			if (release)
			{
				var response = webService.Release_ReleasePackageForReference(searchedReference);
				AssertSuccessfulResponse(response, webService);

				actualPackages = response.PackageChoices.ToArray();
			}
			else
			{
				var response = webService.Release_UnreleasePackageForReference(searchedReference);
				AssertSuccessfulResponse(response, webService);
				actualPackages = response.PackageChoices.ToArray();
			}

			AssertEquals("Count", expectedPackages.Count, actualPackages.Length);
			CombineAssertions(() =>
			{
				foreach (var package in expectedPackages)
				{
					var actualPackage = actualPackages.FirstOrDefault(p => p.PK.Equals(package.PK.ToGuid()));
					AssertNotNull($"Should contain {package.KP_PackageID}-{package.PackageJob.KJ_JobID}", actualPackage);
				}
			});

			if (expectedPackages.Count == 1)
			{
				AssertEquals($"Should be {(release ? "released" : "unreleased")}", release, expectedPackages[0].IsReleased);
			}
		}

		#endregion

		#region TestReleasePackageForPk

		protected void TestReleasePackageForPk(WhsSecureService webService, PkgPackage package, bool release)
		{
			if (release == package.IsReleased)
			{
				ReleaseOrUnreleasePackage(!release, package);
			}
			AssertEquals("Precondition", !release, package.IsReleased);

			package.Factory.Save();

			if (release)
			{
				webService.Release_ReleasePackageForPk(package.PK.ToGuid());
			}
			else
			{
				webService.Release_UnreleasePackageForPk(package.PK.ToGuid());
			}

			AssertEquals(release, package.IsReleased);
		}

		#endregion

		static void ReleaseOrUnreleasePackage(bool isRelease, PkgPackage package)
		{
			package.KP_ReleasedTimeUtc = isRelease ? ZDateTime.UtcNow : ZDateTime.Empty;
			package.KP_GS_NKReleasedBy = isRelease ? Env.CurrentUser.Initials : string.Empty;
		}

		#region AssertReleasePackageForPk_NotFound

		protected void AssertReleasePackageForPk_NotFound(WhsSecureService webService, ZGuid packagePk, bool release)
		{
			var response = release ? webService.Release_ReleasePackageForPk(packagePk.ToGuid()) : webService.Release_UnreleasePackageForPk(packagePk.ToGuid());
			AssertBusinessValidationError(webService, "The package does not exist.", response);
		}

		#endregion

		#endregion
	}
}
