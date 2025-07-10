using System.Collections.Generic;
using System.Linq;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class PackageJobStructureComparerTest : DtbBookingConsignmentUniversalTestCase
	{
		#region TestIsPackageJobSameStructureAsXMLPackageStructure
		public void TestIsPackageJobSameStructureAsXMLPackageStructure()
		{
			// Setup Package Tree Structure as follows:
			// 2x PLT
			// 1x PLT
			//	-> 2x CTN
			//		-> 2x BOX
			//	-> 1x KEG
			var pallet1 = UniversalTestHelper.CreatePackageDataObject(2, "", "plt");
			var pallet2 = UniversalTestHelper.CreatePackageDataObject(1, "", "PLT");
			var cartonDOs = UniversalTestHelper.CreatePackageDataObject(2, "", "CTN");
			cartonDOs.SetPackingLineCollection(() => new List<PackingLine> { UniversalTestHelper.CreatePackageDataObject(2, "", "BOX") });
			pallet2.SetPackingLineCollection(() => new List<PackingLine> { cartonDOs, UniversalTestHelper.CreatePackageDataObject(1, "", "KEG") });
			var emptyPackageJob = Factory.New<PkgPackageJob>();
			AssertEquals("Empty Package Job should not match the tree structure above.", false, PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(Factory, Enumerable.Empty<Container>(), new List<PackingLine> { pallet1, pallet2 }, emptyPackageJob.Row()));
			var packageJobWithIncorrectOuters = Factory.New<PkgPackageJob>();
			packageJobWithIncorrectOuters.Packages.AddNew("PLT");
			var palletWithChildren = packageJobWithIncorrectOuters.Packages.AddNew("PLT");
			var cartonsOnJob1 = palletWithChildren.Packages.AddNew("CTN", 2);
			cartonsOnJob1.Packages.AddNew("BOX", 2);
			var keg = palletWithChildren.Packages.AddNew("KEG");
			AssertEquals("Package Job should not match the tree structure above as the Outers are wrong.", false, PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(Factory, Enumerable.Empty<Container>(), new List<PackingLine> { pallet1, pallet2 }, packageJobWithIncorrectOuters.Row()));
			var packageJobWithIncorrectInners = Factory.New<PkgPackageJob>();
			packageJobWithIncorrectInners.Packages.AddNew("PLT", 2);
			var palletWithIncorrectChildren = packageJobWithIncorrectInners.Packages.AddNew("PLT");
			var cartonsOnJob2 = palletWithIncorrectChildren.Packages.AddNew("CTN", 2);
			cartonsOnJob2.Packages.AddNew("BOX", 2);
			AssertEquals("Package Job should not match the tree structure above as the Inners are wrong.", false, PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(Factory, Enumerable.Empty<Container>(), new List<PackingLine> { pallet1, pallet2 }, packageJobWithIncorrectInners.Row()));
			var correctPackageJob = Factory.New<PkgPackageJob>();
			correctPackageJob.Packages.AddNew("PLT");
			correctPackageJob.Packages.AddNew("PLT");
			var palletWithCorrectChildren = correctPackageJob.Packages.AddNew("PLT");
			var cartonsOnJob3 = palletWithCorrectChildren.Packages.AddNew("CTN", 2);
			cartonsOnJob3.Packages.AddNew("BOX", 2);
			palletWithCorrectChildren.Packages.AddNew("KEG");
			AssertEquals("Package Job should match the tree structure above.", true, PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(Factory, Enumerable.Empty<Container>(), new List<PackingLine> { pallet1, pallet2 }, correctPackageJob.Row()));
		}

		#endregion
		#region TestIsPackageJobSameStructureAsXMLPackageStructure_WithContainers
		public void TestIsPackageJobSameStructureAsXMLPackageStructure_WithContainers()
		{
			// Setup Package Tree Structure as follows:
			// 3x PLT
			// 1x PLT
			//	-> 2x BOX
			// 2x Container
			// 1x Container
			//	-> 2x CTN
			var pallet1 = UniversalTestHelper.CreatePackageDataObject(3, "", "PLT");
			var pallet2 = UniversalTestHelper.CreatePackageDataObject(1, "", "PLT");
			pallet2.SetPackingLineCollection(() => new List<PackingLine> { UniversalTestHelper.CreatePackageDataObject(2, "", "BOX") });
			var container1 = UniversalTestHelper.CreateContainerDataObject("");
			container1.ContainerCount = 2;
			var container2 = UniversalTestHelper.CreateContainerDataObject("", containerLink: 2);
			container2.ContainerCount = 1;
			var cartonDOs = UniversalTestHelper.CreatePackageDataObject(2, "", "CTN");
			cartonDOs.ContainerLink = 2;
			var packageJobWithWrongContainers = Factory.New<PkgPackageJob>();
			packageJobWithWrongContainers.Packages.AddNew("PLT", 3);
			var palletOnJob1 = packageJobWithWrongContainers.Packages.AddNew("PLT");
			palletOnJob1.Packages.AddNew("BOX", 2);
			packageJobWithWrongContainers.Packages.AddNew("CNT");
			var containerOnJob1 = packageJobWithWrongContainers.Packages.AddNew("CNT");
			containerOnJob1.Packages.AddNew("CTN", 2);
			AssertEquals("Package Job should not match the tree structure above as the containers are different.", false, PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(Factory, new[] { container1, container2 }, new List<PackingLine> { pallet1, pallet2, cartonDOs }, packageJobWithWrongContainers.Row()));
			var packageJobWithWrongInners = Factory.New<PkgPackageJob>();
			packageJobWithWrongInners.Packages.AddNew("PLT", 3);
			var palletOnJob2 = packageJobWithWrongInners.Packages.AddNew("PLT");
			palletOnJob2.Packages.AddNew("BOX", 2);
			packageJobWithWrongInners.Packages.AddNew("CNT", 2);
			var containerOnJob2 = packageJobWithWrongInners.Packages.AddNew("CNT");
			containerOnJob2.Packages.AddNew("CTN");
			AssertEquals("Package Job should not match the tree structure above as the inners are different.", false, PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(Factory, new[] { container1, container2 }, new List<PackingLine> { pallet1, pallet2, cartonDOs }, packageJobWithWrongInners.Row()));
			var correctPackageJob = Factory.New<PkgPackageJob>();
			correctPackageJob.Packages.AddNew("PLT", 3);
			var palletOnJob3 = correctPackageJob.Packages.AddNew("PLT");
			palletOnJob3.Packages.AddNew("BOX", 2);
			correctPackageJob.Packages.AddNew("CNT", 2);
			var containerOnJob3 = correctPackageJob.Packages.AddNew("CNT");
			containerOnJob3.Packages.AddNew("CTN", 2);
			AssertEquals("Package Job should match the tree structure above.", true, PackageJobStructureComparer.IsPackageJobSameStructureAsXMLPackageStructure(Factory, new[] { container1, container2 }, new List<PackingLine> { pallet1, pallet2, cartonDOs }, correctPackageJob.Row()));
		}
		#endregion
	}
}
