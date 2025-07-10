using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.Business.TransitLogColumnIDs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class PackageStateTransitLogHelperTest : TransitLogTableHelperTest<WhsItemPackageState, PackageStateColumn>
	{
		public void TestGetTable_GivenPackageStates()
		{
			var tableHelper = GetTableHelper();

			var rcn = Helper.CreateReceiveConsignment("RCN1", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "RC0000001");
			var stagingLocation = Helper.CreateLocation(TestWarehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU0000001", TestWarehouse.PK, stagingLocation.PK, vehicleRef: "CONT1");

			var package1 = Helper.CreatePackage(rcn.PackageJob, "P1", 1, "PKG");
			var packageState1 = Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Arrived, receiveConsignment: rcn, receiveUnit: rtu);
			var package2 = Helper.CreatePackage(rcn.PackageJob, "P2", 1, "PKG");
			var packageState2 = Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);

			Factory.Save();

			var packageStates = new WhsItemPackageState[] { packageState1, packageState2 };

			var expected = new ZString(
				@"
Package        RCN                 RTU
P1             RCN1 (RC0000001)    CONT1 (RTU0000001)
P2             RCN1 (RC0000001)    -");
			var result = tableHelper.GetTable(null, packageStates, TransitLogColumnIDs.PackageStateColumn.Package, TransitLogColumnIDs.PackageStateColumn.RCN, TransitLogColumnIDs.PackageStateColumn.RTU);
			AssertEquals(expected, result);
		}

		public void TestGetTable_DBHits()
		{
			var tableHelper = GetTableHelper();

			var rcn = Helper.CreateReceiveConsignment("RCN1", serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: "RC0000001");

			List<WhsItemPackageState> packageStates = new List<WhsItemPackageState>();
			for (var i = 0; i < tableHelper.MaximumRows + 1; i++)
			{
				var id = i < 9 ? $"P0{i + 1}" : $"P{i + 1}";
				var package = Helper.CreatePackage(rcn.PackageJob, id, 1, "PKG");
				packageStates.Add(Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn));
			}

			Factory.Save();

			var newFactory = new UniversalObjectFactory();

			var packageStatesInNewFactory = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.PK, packageStates.Select(p => p.PK)));
			tableHelper.GetTable(null, packageStatesInNewFactory, PackageStateColumn.Package, PackageStateColumn.RCN);

			var expected = new Dictionary<string, int>
			{
				{ WhsItemPackageStateSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsItemReceiveConsignmentSchema.Constants.TableName, 1 },
			};
			TestCaseWithFactory.AssertDbHits(expected, newFactory.BOFactory);
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
			var packageState1 = Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Departed, receiveConsignment: rcn, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: loadList, receiveUnit: rtu, receiveASN: asn);
			var package2 = Helper.CreatePackage(rcn.PackageJob, ZString.Empty, 2, "CTN");
			var packageState2 = Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn);
			var package3 = Helper.CreatePackage(rtu.PackageJob, "P3", 1, "PKG");
			var packageState3 = Helper.CreatePackageState(package3, TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Factory.Save();

			var columnID = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.Package);
			var columnRCN = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.RCN);
			var columnDCN = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.DCN);
			var columnASN = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.ASN);
			var columnLoadList = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.LoadList);
			var columnRTU = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.RTU);
			var columnDTU = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.DTU);
			var columnStatus = tableHelper.GetColumn(new WhsItemPackageState[] { packageState1, packageState2, packageState3 }, TransitLogColumnIDs.PackageStateColumn.Status);

			AssertEquals("Package", columnID.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "P1", "2 CTN", "P3" }, columnID.Values);
			AssertEquals("RCN", columnRCN.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "RCN1 (RC0000001)", "RCN1 (RC0000001)", ZString.Empty }, columnRCN.Values);
			AssertEquals("DCN", columnDCN.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "DCN1 (DC0000001)", ZString.Empty, ZString.Empty }, columnDCN.Values);
			AssertEquals("Load List", columnLoadList.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "(DLL0000001)", ZString.Empty, ZString.Empty }, columnLoadList.Values);
			AssertEquals("RTU", columnRTU.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "VEH1 (RTU0000001)", ZString.Empty, "VEH1 (RTU0000001)" }, columnRTU.Values);
			AssertEquals("DTU", columnDTU.Header);
			AssertContainsExactElementsInExactOrder(new ZString[] { "VEH2 (DTU0000001)", ZString.Empty, ZString.Empty }, columnDTU.Values);
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
			var result = tableHelper.GetTable(null, packageStates, TransitLogColumnIDs.PackageStateColumn.Package, TransitLogColumnIDs.PackageStateColumn.RCN);

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
			var result = tableHelper.GetTable(null, packageStates, PackageStateColumn.RCN, PackageStateColumn.RCN);

			var expected = new ZString(@"
RCN                           RCN
wwwwwwwwwwwwwww (wwwwww...    wwwwwwwwwwwwwww (wwwwww...");
			AssertEquals("Should truncate oversized cells", expected, result);
		}

		protected override (WhsItemPackageState BusinessObject, string expectedDisplayId) CreateTestBO(int id)
		{
			var rcnId = "RC" + id.ToString().PadLeft(7, '0');
			var rcn = Helper.CreateReceiveConsignment("RCN" + id, serviceLevel: ZString.Empty, TestWarehouse.PK, jobID: rcnId);
			var packageId = "P" + id.ToString().PadLeft(7, '0');
			var package = Helper.CreatePackage(rcn.PackageJob, packageId, 1, "PKG");
			return (Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn), packageId);
		}

		protected override PackageStateColumn GetDefaultColumn() => PackageStateColumn.Package;

		protected override TransitLogTableHelper<WhsItemPackageState, PackageStateColumn> GetTableHelper() => new PackageStateTransitLogHelper();

		protected override void SetUp()
		{
			base.SetUp();
			TestWarehouse = Helper.CreateTRWWarehouse("WH1");
		}

		WhsWarehouse TestWarehouse { get; set; }
	}
}
