using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public class CustomReferenceExtensionsTest : TestCaseWithFactory
	{
		public void TestGetCustomsReferenceNumbersRefAndCode_NotIncludingPackageLevel_Fallback()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsNumber(rcn, "CEN1", "T1");
			Helper.CreateCustomsReleaseNumber(rcn, "CRN1", "T2");

			var package1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package1.Package, "PCRN");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: false);

			AssertEquals("RefType", "T2", result.RefType);
			AssertEquals("RefCode", "CRN1", result.RefCode);
		}

		public void TestGetCustomsReferenceNumbersRefAndCode_NotIncludingPackageLevel()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsReleaseNumber(rcn, "CRN1", "T1");
			Helper.CreateCustomsReleaseNumber(rcn, "CRN2", "T2");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: false);

			AssertEquals("RefType", "T1, T2", result.RefType);
			AssertEquals("RefCode", "CRN1, CRN2", result.RefCode);
		}

		public void TestGetCustomsReferenceNumbersRefAndCode_NotIncludingPackageLevel_OnlyCEN()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsNumber(rcn, "CEN1", "T1");
			Helper.CreateCustomsNumber(rcn, "CEN2", "T2");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: false);

			AssertEquals("RefType", "T1, T2", result.RefType);
			AssertEquals("RefCode", "CEN1, CEN2", result.RefCode);
		}

		public void TestGetCustomsReferenceNumbersRefAndCode_IncludingPackageLevel_WithoutPackageSourceType()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsReleaseNumber(rcn, "CRN1", "T1");
			Helper.CreateCustomsReleaseNumber(rcn, "CRN2", "T2");

			var package1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package1.Package, "CRN1");
			var package2 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package2.Package, "PCRN5");
			var package3 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P3", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package3.Package, "PCRN3");
			var package4 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P4", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package4.Package, "PCRN5");
			var package5 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P5", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package5.Package, "PCRN2");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: true);

			AssertEquals("RefType", "T1, T2, T1, T1, T1", result.RefType);
			AssertEquals("RefCode", "CRN1, CRN2, PCRN2, PCRN3, PCRN5", result.RefCode);
		}

		public void TestGetCustomsReferenceNumbersRefAndCode_IncludingPackageLevel_WithoutPackageSourceType_CopyRCNSourceType()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsReleaseNumber(rcn, "CRN1", "T2L");

			var package1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package1.Package, "CRN1");
			var package2 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package2.Package, "PCRN5");
			var package3 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P3", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package3.Package, "PCRN3");
			var package4 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P4", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package4.Package, "PCRN5");
			var package5 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P5", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package5.Package, "PCRN2");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: true);

			AssertEquals("RefType", "T2L, T2L, T2L, T2L", result.RefType);
			AssertEquals("RefCode", "CRN1, PCRN2, PCRN3, PCRN5", result.RefCode);
		}

		public void TestGetCustomsReferenceNumbersRefAndCode_IncludingPackageLevel_RemoveDuplicatedItems()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsReleaseNumber(rcn, "CRN1", "T1");
			Helper.CreateCustomsReleaseNumber(rcn, "CRN2", "T2");

			var package1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package1.Package, "CRN1", "T1");
			var package2 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package2.Package, "CRN2", "T1");
			var package3 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P3", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package3.Package, "CRN3", "T1");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: true);

			AssertEquals("RefType", "T1, T2, T1, T1", result.RefType);
			AssertEquals("RefCode", "CRN1, CRN2, CRN2, CRN3", result.RefCode);
		}

		public void TestGetCustomsReferenceNumbersRefAndCode_IncludingPackageLevel_SomePackagesHaveSourceType()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsReleaseNumber(rcn, "CRN1", "T1");
			Helper.CreateCustomsReleaseNumber(rcn, "CRN2", "T2");

			var package1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package1.Package, "PCRN1", "T1");
			var package2 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package2.Package, "PCRN2");
			var package3 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P3", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package3.Package, "PCRN3", "T2");
			var package4 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P4", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package4.Package, "PCRN4");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: true);

			AssertEquals("RefType", "T1, T2, T1, T1, T1, T2", result.RefType);
			AssertEquals("RefCode", "CRN1, CRN2, PCRN1, PCRN2, PCRN4, PCRN3", result.RefCode);
		}

		public void TestGetCustomsReferenceNumbersRefAndCode_IncludingPackageLevel_MixedFallback()
		{
			var warehouse = CreateWhsWarehouse("WH1", "WH1Address", "C001");

			var rcn = Helper.CreateReceiveConsignment("RC0000001", warehouse.PK);
			Helper.CreateCustomsNumber(rcn, "CEN1", "T1");
			Helper.CreateCustomsNumber(rcn, "CEN2", "T1");
			Helper.CreateCustomsReleaseNumber(rcn, "CRN1", "T1");

			var package1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsNumber(package1.Package, "PCEN1");
			var package2 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P2", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsNumber(package2.Package, "PCEN2-1");
			Helper.CreateCustomsNumber(package2.Package, "PCEN2-2");
			var package3 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P3", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsNumber(package3.Package, "CRN1", "T1");
			var package4 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P4", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsNumber(package4.Package, "PCEN4", "T1");
			var package5 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P5", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsReleaseNumber(package5.Package, "PCRN5", "T1");
			var package6 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P6", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateCustomsNumber(package6.Package, "PCEN6", "T2");
			Helper.CreateCustomsReleaseNumber(package6.Package, "PCRN6", "T2");

			var result = rcn.GetCustomsReferenceNumbersRefAndCode(includingPackageLevel: true);

			AssertEquals("RefType", "T1, T1, T1, T1, T1, T1, T2", result.RefType);
			AssertEquals("RefCode", "CRN1, PCEN1, PCEN2-1, PCEN2-2, PCEN4, PCRN5, PCRN6", result.RefCode);
		}

		WhsWarehouse CreateWhsWarehouse(string warehouseName, string address, string cinCode)
		{
			var warehouse = Helper.CreateWarehouse(warehouseName);
			warehouse.WarehouseAddress.Address1 = address;
			Helper.AddOrgCode(warehouse.WarehouseAddress, OrgCusCode.FranceCodeTypes.CIN, cinCode);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);

			var inLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			warehouse.WW_DefaultInboundDockDoor = inLocation.PK;

			var outLocation = row.Locations.First(l => l.ToLocationString() == "Dock-1-2");
			warehouse.WW_DefaultOutboundDockDoor = outLocation.PK;
			return warehouse;
		}

		#region Implement

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
