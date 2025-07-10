using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class PackageColumnIndexerHelperTest : ColumnIndexerHelperTest<PkgPackage>
	{
		public void TestGetPackageId()
		{
			AssertGetValue(
				packageId => CreateTestPackageFromID(packageId),
				p => p.KP_PackageID,
				PackageColumnIndexerHelper.GetPackageId,
				(input: "P1", expected: "P1"));
		}

		public void TestGetHeader()
		{
			AssertGetRelatedIndexer(
				() => CreateTestPackageFromID(),
				p => p.GetPackageHeader(),
				PackageColumnIndexerHelper.GetHeader);
		}

		PkgPackage CreateTestPackageFromID(string id = "P1")
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			return Helper.CreatePackage(rcn.PackageJob, id, 1, "PKG");
		}
	}
}
