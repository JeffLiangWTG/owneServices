using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class PackageStateColumnIndexerTransitLogHelperTest : TransitLogTableHelperTest<IColumnIndexer, PackageStateIndexerColumn>
	{
		public void TestGetTable_GivenPackageStates()
		{
			var tableHelper = GetTableHelper();

			var rcn = Helper.CreateReceiveConsignment("RCN1", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "RC0000001");

			var package1 = Helper.CreatePackage(rcn.PackageJob, "P1", 1, "PKG");
			var packageState1 = Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);
			var package2 = Helper.CreatePackage(rcn.PackageJob, "P2", 1, "PKG");
			var packageState2 = Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);
			var package3 = Helper.CreatePackage(rcn.PackageJob, "", 3, "PKG");
			var packageState3 = Helper.CreatePackageState(package3, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);
			var package4 = Helper.CreatePackage(rcn.PackageJob, "B1", 1, "BOX");
			var packageState4 = Helper.CreatePackageState(package4, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);
			var package5 = Helper.CreatePackage(rcn.PackageJob, "", 5, "BOX");
			var packageState5 = Helper.CreatePackageState(package5, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);

			Factory.Save();

			var packageStates = new WhsItemPackageState[] { packageState1, packageState2, packageState3, packageState4, packageState5 };
			var indexers = packageStates.Select(p => GetIndexer(p));
			var typeAndQty = new List<(ZString Type, long Qty)>()
			{
				(Type: "PKG", Qty: 10),
				(Type: "BOX", Qty: 10)
			};

			var expected = new ZString(
				@"
UXML Package    Package        RCN
10 BOX          5 BOX          RCN1 (RC0000001)
-               B1             RCN1 (RC0000001)
10 PKG          3 PKG          RCN1 (RC0000001)
-               P1             RCN1 (RC0000001)
-               P2             RCN1 (RC0000001)");
			var result = ((PackageStateColumnIndexerTransitLogHelper)tableHelper).GetTable(null, indexers, typeAndQty, PackageStateIndexerColumn.UxmlPackage, PackageStateIndexerColumn.Package, PackageStateIndexerColumn.RCN);
			AssertEquals(expected, result);
		}

		public override void TestGetColumns()
		{
			var tableHelper = GetTableHelper();

			var rcn = Helper.CreateReceiveConsignment("RCN1", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "RC0000001");
			var dcn = Helper.CreateDispatchConsignment("DCN1", TestWarehouse.PK, jobID: "DC0000001");
			var asn = helper.CreateReceiveASN("ASN0000001", TestWarehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL0000001", TestWarehouse.PK);
			var stagingLocation = Helper.CreateLocation(TestWarehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU0000001", TestWarehouse.PK, stagingLocation.PK, vehicleRef: "VEH1");
			var dtu = Helper.CreateDispatchTransportationUnit("DTU0000001", TestWarehouse.PK, vehicleRef: "VEH2");

			var package1 = Helper.CreatePackage(rcn.PackageJob, "P1", 1, "PKG");
			var packageState1 = Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Departed, receiveConsignment: rcn, dispatchConsignment: dcn, receiveUnit: rtu, dispatchUnit: dtu, receiveASN: asn, dispatchLoadList: loadList);
			var package2 = Helper.CreatePackage(rcn.PackageJob, ZString.Empty, 2, "CTN");
			var packageState2 = Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);
			var package3 = Helper.CreatePackage(rcn.PackageJob, "P3", 1, "PKG");
			var packageState3 = Helper.CreatePackageState(package3, TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();

			var packageStates = new WhsItemPackageState[] { packageState1, packageState2, packageState3 };
			var indexers = packageStates.Select(p => GetIndexer(p));

			var columnID = tableHelper.GetColumn(indexers, PackageStateIndexerColumn.Package);
			var columnRCN = tableHelper.GetColumn(indexers, PackageStateIndexerColumn.RCN);
			var columnDCN = tableHelper.GetColumn(indexers, PackageStateIndexerColumn.DCN);
			var columnStatus = tableHelper.GetColumn(indexers, PackageStateIndexerColumn.Status);

			AssertEquals("Package", columnID.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "P1", "2 CTN", "P3" }, columnID.Values);
			AssertEquals("RCN", columnRCN.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "RCN1 (RC0000001)", "RCN1 (RC0000001)", ZString.Empty }, columnRCN.Values);
			AssertEquals("DCN", columnDCN.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "DCN1 (DC0000001)", ZString.Empty, ZString.Empty }, columnDCN.Values);
			AssertEquals("Status", columnStatus.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "Departed", "Booked", "Arrived" }, columnStatus.Values);
		}

		public override void TestGetTable_Sorted()
		{
			var tableHelper = GetTableHelper();

			var rcn1 = Helper.CreateReceiveConsignment("2", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "2");
			var rcn2 = Helper.CreateReceiveConsignment("1", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "1");
			var rcn3 = Helper.CreateReceiveConsignment("3", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "3");
			var rcn4 = Helper.CreateReceiveConsignment("4", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "4");
			var package1 = Helper.CreatePackage(rcn1.PackageJob, "3", 1, "PKG");
			var packageState1 = Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn1);
			var package2 = Helper.CreatePackage(rcn2.PackageJob, "3", 1, "PKG");
			var packageState2 = Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn2);
			var package3 = Helper.CreatePackage(rcn3.PackageJob, "1", 1, "PKG");
			var packageState3 = Helper.CreatePackageState(package3, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn3);
			var package4 = Helper.CreatePackage(rcn4.PackageJob, "2", 1, "PKG");
			var packageState4 = Helper.CreatePackageState(package4, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn4);

			Factory.Save();

			var packageStates = new WhsItemPackageState[] { packageState1, packageState2, packageState3, packageState4 };
			var indexers = packageStates.Select(p => GetIndexer(p));
			var result = tableHelper.GetTable(null, indexers, PackageStateIndexerColumn.Package, PackageStateIndexerColumn.RCN);

			//		Columns as passed in:
			//		3     (2)
			//		3     (1)
			//		1     (3)
			//		2     (4)

			var expected = new ZString(
				@"
Package        RCN
1              3 (3)
2              4 (4)
3              1 (1)
3              2 (2)");
			AssertEquals(expected, result);
		}

		public void TestGetTable_GivenOversizedColumn_TruncatesWithPadding()
		{
			var tableHelper = GetTableHelper();
			tableHelper.MaximumColumnWidth = 30;

			// Get the largest strings that can fit the fields
			var length15String = ZString.Empty;
			for (int i = 0; i < 15; i++)
			{
				length15String += 'w';
			}

			var rcn = Helper.CreateReceiveConsignment(length15String, serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: length15String);
			var package = Helper.CreatePackage(rcn.PackageJob, "P1", 1, "PKG");
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);

			Factory.Save();

			var packageStates = new WhsItemPackageState[] { packageState };
			var indexers = packageStates.Select(p => GetIndexer(p));
			var result = tableHelper.GetTable(null, indexers, PackageStateIndexerColumn.RCN, PackageStateIndexerColumn.RCN);

			var expected = new ZString(@"
RCN                           RCN
wwwwwwwwwwwwwww (wwwwww...    wwwwwwwwwwwwwww (wwwwww...");
			AssertEquals("Should truncate oversized cells", expected, result);
		}

		protected override (IColumnIndexer BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var rcnId = "RC" + id.ToString().PadLeft(7, '0');
			var rcn = Helper.CreateReceiveConsignment("RCN" + id, serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: rcnId);
			var packageId = "P" + id.ToString().PadLeft(7, '0');
			var package = Helper.CreatePackage(rcn.PackageJob, packageId, 1, "PKG");
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);
			Factory.Save();
			return (GetIndexer(packageState), packageId);
		}

		protected override PackageStateIndexerColumn GetDefaultColumn() => PackageStateIndexerColumn.Package;

		protected override TransitLogTableHelper<IColumnIndexer, PackageStateIndexerColumn> GetTableHelper() => new PackageStateColumnIndexerTransitLogHelper(UniversalFactory);

		protected override void SetUp()
		{
			base.SetUp();
			UniversalFactory = new UniversalObjectFactory();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		protected IColumnIndexer GetIndexer(BusinessObject bo)
		{
			return UniversalFactory.RowFactory.LoadFromPK(bo.TableName, bo.PK) as IColumnIndexer;
		}

		WhsWarehouse TestWarehouse { get; set; }

		protected UniversalObjectFactory UniversalFactory { get; set; }
	}
}
