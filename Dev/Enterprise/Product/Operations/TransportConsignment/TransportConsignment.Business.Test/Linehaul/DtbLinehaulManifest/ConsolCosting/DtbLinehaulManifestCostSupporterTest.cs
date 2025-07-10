using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbLinehaulManifestCostSupporterTest : DtbBookingConsignmentTestCaseWithFactory
	{
		public void TestCreditor()
		{
			var depot = Factory.New<OrgHeader>();
			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.TransportCompanyPK = depot.PK;
			var costSupporter = (IGenericJobCostSupporter)new DtbLinehaulManifestCostSupporter(manifest);
			AssertEquals(depot.PK, costSupporter.GetCreditorPK(ChargeCodeGroupList.Codes.Transport, ZGuid.Empty));

			manifest.TransportCompanyPK = ZGuid.Empty;
			costSupporter = new DtbLinehaulManifestCostSupporter(manifest);
			AssertEquals(ZGuid.Empty, costSupporter.GetCreditorPK(ChargeCodeGroupList.Codes.Transport, ZGuid.Empty));
		}

		public void TestSendingForwarder()
		{
			var depot = Factory.New<OrgHeader>();
			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.LHM_OA_OriginDepot = depot.MainAddress.PK;
			var costSupporter = (IGenericJobCostSupporter)new DtbLinehaulManifestCostSupporter(manifest);
			AssertEquals(depot, costSupporter.SendingForwarder);
		}

		public void TestReceivingForwarder()
		{
			var depot = Factory.New<OrgHeader>();
			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.LHM_OA_DestinationDepot = depot.MainAddress.PK;
			var costSupporter = (IGenericJobCostSupporter)new DtbLinehaulManifestCostSupporter(manifest);
			AssertEquals(depot, costSupporter.ReceivingForwarder);
		}

		public void TestConsignments()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplate();
			var package1_1 = Helper.CreatePackage(consignment1, 10m, 10m);
			var package1_2 = Helper.CreatePackage(consignment1, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment1.PickupInstruction, package1_1, 1);
			Helper.CreateInstructionPkgDivot(consignment1.PickupInstruction, package1_2, 1);

			var consignment2 = Helper.CreateBookingConsignmentWithTemplate();
			var package2_1 = Helper.CreatePackage(consignment2, 10m, 10m);
			Helper.CreateInstructionPkgDivot(consignment2.PickupInstruction, package2_1, 1);

			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.Packages.Add(package1_1);
			manifest.Packages.Add(package1_2);
			manifest.Packages.Add(package2_1);

			var costSupporter = (IGenericJobCostSupporter)new DtbLinehaulManifestCostSupporter(manifest);
			AssertCollectionContains(consignment1, costSupporter.ShipmentsList);
			AssertCollectionContains(consignment2, costSupporter.ShipmentsList);
		}
	}
}
