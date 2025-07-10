using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbLinehaulManifestConsolRatingAdaptersProviderTest : DtbBookingConsignmentTestCaseWithFactory
	{
		public void TestIJobInvoicingHostWithAdditionalJobs_AdditionalJobs()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplate();
			var package1_1 = Helper.CreatePackage(consignment1, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment1.PickupInstruction, package1_1, 1);
			var package1_2 = Helper.CreatePackage(consignment1, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment1.PickupInstruction, package1_2, 1);

			var consignment2 = Helper.CreateBookingConsignmentWithTemplate();
			var package2_1 = Helper.CreatePackage(consignment2, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment2.PickupInstruction, package2_1, 1);

			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.Packages.Add(package1_1);
			manifest.Packages.Add(package1_2);
			manifest.Packages.Add(package2_1);
			var provider = ((IRatingSupporter)manifest).AdaptersProvider;
			var jobs = provider.GetAdditionalJobs();
			AssertCollectionContains(consignment1, jobs);
			AssertCollectionContains(consignment2, jobs);
		}
	}
}
