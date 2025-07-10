using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbLinehaulManifestPackageCollectionNonTransactionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestConcurrencySynchronization_OverlappingSaves()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new TransportBookingConsignmentTestHelper(factory);

			// setup
			var consignment = testHelper.CreateBookingConsignment();
			var package1 = testHelper.CreatePackage(consignment, 10m, 10m);
			package1.KP_PackageID = "PACK1";
			var package2 = testHelper.CreatePackage(consignment, 10m, 10m);
			package2.KP_PackageID = "PACK2";
			var package3 = testHelper.CreatePackage(consignment, 10m, 10m);
			package3.KP_PackageID = "PACK3";
			var package4 = testHelper.CreatePackage(consignment, 10m, 10m);
			package4.KP_PackageID = "PACK4";
			var depot = factory.NewWithValidTestData<OrgHeader>();
			depot.MainAddress.OA_Address1 = "Test";
			var depot2 = factory.NewWithValidTestData<OrgHeader>();
			depot2.MainAddress.OA_Address1 = "Test2";
			var savedManifest = factory.New<DtbLinehaulManifest>();
			savedManifest.LHM_StartDateTimeUtc = ZDateTime.Now;
			savedManifest.LHM_OA_OriginDepot = depot.MainAddress.PK;
			savedManifest.LHM_OA_DestinationDepot = depot2.MainAddress.PK;
			factory.Save();

			Thread otherUser = null;

			using (var user1Connection = Db.NewExtraConnectionToMainDb())
			using (var user2Connection = Db.NewExtraConnectionToMainDb())
			{
				var user1Factory = new BusinessObjectFactory(user1Connection) { RefreshEnabled = false };
				var user2Factory = new BusinessObjectFactory(user2Connection) { RefreshEnabled = false };

				// User 1 adds Pack1, Pack2 and saves
				var user1Manifest = user1Factory.Load<DtbLinehaulManifest>(savedManifest.PK);
				user1Manifest.Packages.Add(package1);
				user1Manifest.Packages.Add(package2);
				user1Factory.Save();
				AssertOnlyThesePackagesExist(user1Manifest, package1, package2);

				// User 2 removes pack1, adds pack 3 - does not yet save
				var user2Manifest = user2Factory.Load<DtbLinehaulManifest>(savedManifest.PK);
				user2Manifest.Packages.RemoveFromRelationship(package1);
				user2Manifest.Packages.Add(package3);

				// User 1 adds pack 4, removes pack 2 and saves
				// During user 1's save, user 2 also saves
				user1Manifest.Packages.Add(package4);
				user1Manifest.Packages.RemoveFromRelationship(package2);

				user2Factory.ThreadSentry.RelinquishThreadOwnership(); //Give up thread factory ownership on current thread
				user1Manifest.Packages.OnAfterManifestLockGrantedForTesting += delegate
				{
					otherUser = new Thread(delegate()
					{
						using (Db.DisposableActionForDbConnection())
						{
							user2Factory.ThreadSentry.TakeThreadOwnership(); // Take factory ownership on current thread
							user2Factory.Save();
						}
					});

					otherUser.Start();

					Thread.Sleep(5000); // ensure Other User executes Save and is blocked
				};

				user1Factory.Save();
				if (otherUser != null && otherUser.IsAlive)
				{
					otherUser.Join(5000); // wait a few seconds for the Other User to complete their attempt to save
				}

				AssertOnlyThesePackagesExist(user2Manifest, package3, package4);

				if (ErrorReporter.TotalErrorCount == 1)
				{
					AssertStartsWith("Expected DB connection access by other thread", "Attempted to access an object owned by another thread.", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		[UseSnapshotProtection]
		public void TestConcurrencySynchronization()
		{
			using (var user1Connection = Db.NewExtraConnectionToMainDb())
			using (var user2Connection = Db.NewExtraConnectionToMainDb())
			{
				var user1Factory = new BusinessObjectFactory(user1Connection) { RefreshEnabled = false };
				var user2Factory = new BusinessObjectFactory(user2Connection) { RefreshEnabled = false };

				var testHelper = new TransportBookingConsignmentTestHelper(user1Factory);

				// setup
				var consignment = testHelper.CreateBookingConsignment();
				var package1 = testHelper.CreatePackage(consignment, 10m, 10m);
				package1.KP_PackageID = "PACK1";
				var package2 = testHelper.CreatePackage(consignment, 10m, 10m);
				package2.KP_PackageID = "PACK2";
				var package3 = testHelper.CreatePackage(consignment, 10m, 10m);
				package3.KP_PackageID = "PACK3";
				var package4 = testHelper.CreatePackage(consignment, 10m, 10m);
				package4.KP_PackageID = "PACK4";
				var depot = user1Factory.NewWithValidTestData<OrgHeader>();
				depot.MainAddress.OA_Address1 = "Test";
				var depot2 = user1Factory.NewWithValidTestData<OrgHeader>();
				depot2.MainAddress.OA_Address1 = "Test2";
				var user1Manifest = user1Factory.New<DtbLinehaulManifest>();
				user1Manifest.LHM_StartDateTimeUtc = ZDateTime.Now;
				user1Manifest.LHM_OA_OriginDepot = depot.MainAddress.PK;
				user1Manifest.LHM_OA_DestinationDepot = depot2.MainAddress.PK;
				user1Factory.Save();

				// User 1 adds Pack1, Pack2 and saves
				user1Manifest.Packages.Add(package1);
				user1Manifest.Packages.Add(package2);
				user1Factory.Save();
				AssertOnlyThesePackagesExist(user1Manifest, package1, package2);

				// User 2 removes pack1, adds pack 3 - does not yet save
				var user2Manifest = user2Factory.Load<DtbLinehaulManifest>(user1Manifest.PK);
				user2Manifest.Packages.RemoveFromRelationship(package1);
				user2Manifest.Packages.Add(package3);

				// User 1 adds pack 4, removes pack 2 and saves
				user1Manifest.Packages.Add(package4);
				user1Manifest.Packages.RemoveFromRelationship(package2);
				user1Factory.Save();

				// result should be 1,4
				AssertOnlyThesePackagesExist(user1Manifest, package1, package4);

				// User 2 now saves - should end up as 3,4
				user2Factory.Save();
				AssertOnlyThesePackagesExist(user2Manifest, package3, package4);
			}
		}

		void AssertOnlyThesePackagesExist(DtbLinehaulManifest manifest, params PkgPackage[] expectedPackages)
		{
			AssertEquals(expectedPackages.Length, manifest.Packages.Count);
			var packageIds = expectedPackages.Select(x => x.KP_PackageID).OrderBy(x => x).Distinct();
			AssertEquals(string.Join(",", packageIds), manifest.LHM_PackageString);
			foreach (var package in expectedPackages)
			{
				AssertCollectionContains(package.PK, manifest.Packages.Select(x => x.PK));
			}
		}
	}
}
