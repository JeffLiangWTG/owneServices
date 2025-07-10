using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class PackageStateColumnIndexerHelperTest : ColumnIndexerHelperTest<WhsItemPackageState>
	{
		public void TestGetStatus()
		{
			AssertGetValue(
				status =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
					var package = Helper.CreatePackage(rcn.PackageJob, "P1", 1, "PKG");
					var packageState = Helper.CreatePackageState(package, status, receiveConsignment: rcn);
					return packageState;
				},
				packageState => packageState.Package.KP_F3_NKPackType,
				(factory, indexer) => PackageStateColumnIndexerHelper.GetStatus(indexer),
				(input: "BKD", expected: new ZString("Booked")));
		}

		public void TestGetRCN()
		{
			AssertGetRelatedIndexer(
				() => CreatePackageStateWithJobs(),
				p => p.ReceiveConsignment,
				PackageStateColumnIndexerHelper.GetRCN);
		}

		public void TestGetDCN()
		{
			AssertGetRelatedIndexer(
				() => CreatePackageStateWithJobs(),
				p => p.DispatchConsignment,
				PackageStateColumnIndexerHelper.GetDCN);
		}

		public void TestGetLoadList()
		{
			AssertGetRelatedIndexer(
				() => CreatePackageStateWithJobs(),
				p => p.DispatchLoadList,
				PackageStateColumnIndexerHelper.GetLoadList);
		}

		public void TestGetPackage()
		{
			AssertGetRelatedIndexer(
				() => CreatePackageStateWithJobs(),
				p => p.Package,
				PackageStateColumnIndexerHelper.GetPackage);
		}

		#region TestGetPackageReference

		public void TestGetPackageReference_WithPackageID()
		{
			TestGetPackageReference("P1", 1, "PKG", "P1");
		}

		public void TestGetPackageReference_WithoutPackageID()
		{
			TestGetPackageReference("", 1, "PKG", "1 PKG");
		}

		public void TestGetPackageReference_WithoutPackageID_QtyAndTypeDefined()
		{
			TestGetPackageReference("", 5, "BOX", "5 BOX");
		}

		void TestGetPackageReference(string packageId, int qty, string type, string expected)
		{
			AssertGetValue(
				data =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
					var package = Helper.CreatePackage(rcn.PackageJob, data.PackageID, data.Qty, data.Type);
					var packageState = Helper.CreatePackageState(package, "BKD", receiveConsignment: rcn);
					return packageState;
				},
				packageState => packageState.Package.KP_F3_NKPackType,
				PackageStateColumnIndexerHelper.GetPackageReference,
				(input: (PackageID: packageId, Qty: qty, Type: type), expected: expected));
		}

		#endregion

		public void TestGetPackageType()
		{
			AssertGetValue(
				type =>
				{
					var warehouse = Helper.CreateTRWWarehouse("WH1");
					var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
					var package = Helper.CreatePackage(rcn.PackageJob, "P1", 1, type);
					var packageState = Helper.CreatePackageState(package, "BKD", receiveConsignment: rcn);
					return packageState;
				},
				packageState => packageState.Package.KP_F3_NKPackType,
				PackageStateColumnIndexerHelper.GetPackageType,
				(input: "PKG", expected: "PKG"));
		}

		WhsItemPackageState CreatePackageStateWithJobs()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var ddl = Helper.CreateDispatchLoadList("DDL1", warehouse.PK);
			var package = Helper.CreatePackage(rcn.PackageJob, "P1", 1, "PKG");
			return Helper.CreatePackageState(package, "BKD", receiveConsignment: rcn, dispatchConsignment: dcn, dispatchLoadList: ddl);
		}
	}
}
