using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbLinehaulManifestPackageCollection))]
	sealed class DtbLinehaulManifestPackageCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbLinehaulManifestPackageCollection>
	{
		public void TestEmptyPackageIDNotAdded()
		{
			var consignment = Helper.CreateBookingConsignment();
			var package1 = Helper.CreatePackage(consignment, 10m, 10m);
			package1.KP_PackageID = "";
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			manifest.LHM_PackageString = "";
			var collection = new DtbLinehaulManifestPackageCollection(manifest);
			AssertEquals(0, collection.Count);
		}

		public void TestBlankPackageIDNotAdded()
		{
			var consignment = Helper.CreateBookingConsignment();
			var package1 = Helper.CreatePackage(consignment, 10m, 10m);
			package1.KP_PackageID = "";
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			manifest.LHM_PackageString = "PACK1,,PACK2";
			var collection = new DtbLinehaulManifestPackageCollection(manifest);
			AssertEquals(0, collection.Count);
		}

		public void TestRemoveFromRelationshipSetsHasChanges()
		{
			var consignment = Helper.CreateBookingConsignment();
			var package1 = Helper.CreatePackage(consignment, 10m, 10m);
			package1.KP_PackageID = "PACK1";
			var package2 = Helper.CreatePackage(consignment, 10m, 10m);
			package2.KP_PackageID = "PACK2";
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			manifest.LHM_PackageString = "PACK1,PACK2";
			var collection = new DtbLinehaulManifestPackageCollection(manifest);
			AssertEquals(2, collection.Count);
			manifest.HasChanges = false;

			collection.RemoveFromRelationship(collection[0]);
			AssertEquals(1, collection.Count);
			AssertEquals(true, manifest.HasChanges);
		}

		public void TestLoad()
		{
			var consignment = Helper.CreateBookingConsignment();
			var package1 = Helper.CreatePackage(consignment, 10m, 10m);
			package1.KP_PackageID = "PACK1";
			var package2 = Helper.CreatePackage(consignment, 10m, 10m);
			package2.KP_PackageID = "PACK2";
			var package3 = Helper.CreatePackage(consignment, 10m, 10m);
			package3.KP_PackageID = "PACK4";
			var booking = Helper.CreateBooking("ZUBZAYRYAL");
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			manifest.LHM_PackageString = "PACK1, PACK4,PACK5 ";
			var collection = new DtbLinehaulManifestPackageCollection(manifest);

			AssertEquals(2, collection.Count);
			AssertCollectionContains(package1, collection);
			AssertCollectionContains(package3, collection);
		}

		public void TestSaveItems()
		{
			var consignment = Helper.CreateBookingConsignment();
			var package1 = Helper.CreatePackage(consignment, 10m, 10m);
			package1.KP_PackageID = "PACK2";
			var package2 = Helper.CreatePackage(consignment, 10m, 10m);
			package2.KP_PackageID = "PACK1";
			var package3 = Helper.CreatePackage(consignment, 10m, 10m);
			package3.KP_PackageID = "PACK3";

			var manifest = Factory.NewWithValidTestData<DtbLinehaulManifest>();
			var collection = new DtbLinehaulManifestPackageCollection(manifest);
			collection.Add(package1);
			collection.Add(package2);
			Factory.Save();

			AssertEquals("PACK1,PACK2", manifest.LHM_PackageString);

			collection.Add(package3);
			Factory.Save();
			AssertEquals("PACK1,PACK2,PACK3", manifest.LHM_PackageString);

			var package4 = Helper.CreatePackage(consignment, 10m, 10m);
			package4.KP_PackageID = "PACK3";
			collection.Add(package4);
			Factory.Save();
			AssertEquals("PACK1,PACK2,PACK3", manifest.LHM_PackageString);
		}

		#region Implementation

		protected override DtbLinehaulManifestPackageCollection GetCollectionToTest()
		{
			var manifest = Factory.New<DtbLinehaulManifest>();
			return new DtbLinehaulManifestPackageCollection(manifest);
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
