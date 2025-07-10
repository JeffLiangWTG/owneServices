using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitDocumentSupporter))]
	public class WhsItemDispatchTransportationUnitDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<WhsItemDispatchTransportationUnit>();
		}

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			var docSupporter = ((IDocumentSupportable)dtu).DocumentSupporter;
			AssertEquals(Env.Security.WhsItemDispatchTransportationUnitCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var dispatchUnit = GetDocumentSupportableBusinessObject();
			var docSupporter = dispatchUnit.DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		#endregion

		#region TestGetContactOrganisation

		public void TestGetContactOrganisation()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateWarehouse("TTT", "A", 3, 1);
			Factory.Save();

			var warehouseOrgHeader = warehouse.WarehouseAddress.Header;
			var transportCompany = Factory.New<OrgHeader>();
			var unit = helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			helper.CreateJobDocAddressFromAddress(unit, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			AssertNull(new WhsItemDispatchTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", null, DocumentDirection.ANY));
			AssertEquals(warehouseOrgHeader, new WhsItemDispatchTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", ContactType.TransitWarehouse, DocumentDirection.ANY).OrgHeader);

			AssertEquals(transportCompany, new WhsItemDispatchTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader);

			unit.TransportCompany.E2_AddressOverride = true;
			AssertNull(new WhsItemDispatchTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY));
		}

		#endregion

		#region TestDocumentSupporter_SupportedChildBusinessContexts

		public void TestDocumentSupporter_SupportedChildBusinessContexts()
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			var docSupporter = ((IDocumentSupportable)dtu).DocumentSupporter;
			AssertContainsExactElementsInAnyOrder(new BusinessContext[] { BusinessContext.TransitDspConsignmnt, BusinessContext.TransitDspLoadList }, docSupporter.SupportedChildBusinessContexts);
		}

		#endregion

		#region TestGetChildCollection

		public void TestGetChildCollection_TransitDspConsignmnt_OrderByDCNHouseBillNumber()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateWarehouse("TTT", "A", 3, 1);

			var dtu = helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnit("WRHID", warehouse.PK, warehouse.Rows[0].Locations[0].PK);
			var dll = helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var dcn1 = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn1.WDC_HouseBillNumber = "HB1";

			var dcn2 = helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			dcn2.WDC_HouseBillNumber = "HB2";

			var dcn3 = helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			dcn3.WDC_HouseBillNumber = "HB3";

			helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn1, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn2, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn2, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn3, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn3, dispatchLoadList: dll, dispatchUnit: dtu);
			Factory.Save();

			var testMenuItem = Factory.New<StmMenuItem>();
			testMenuItem.SU_MenuName = "test Menu";

			var docSupporter = ((IDocumentSupportable)dtu).DocumentSupporter;
			var dcns = docSupporter.GetChildCollection(testMenuItem, BusinessContext.TransitDspConsignmnt, null);
			AssertContainsExactElementsInExactOrder(new[] { dcn1, dcn2, dcn3 }, dcns);
		}

		public void TestGetChildCollection_TransitDspConsignmnt_NoHouseBillNumber_OrderByDCNConsignmentIDAsFallBack()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateWarehouse("TTT", "A", 3, 1);

			var dtu = helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnit("WRHID", warehouse.PK, warehouse.Rows[0].Locations[0].PK);
			var dll = helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var dcn1 = helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var dcn2 = helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var dcn3 = helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn1, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn2, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn2, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn3, dispatchLoadList: dll, dispatchUnit: dtu);
			helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn3, dispatchLoadList: dll, dispatchUnit: dtu);
			Factory.Save();

			var testMenuItem = Factory.New<StmMenuItem>();
			testMenuItem.SU_MenuName = "test Menu";

			var docSupporter = ((IDocumentSupportable)dtu).DocumentSupporter;
			var dcns = docSupporter.GetChildCollection(testMenuItem, BusinessContext.TransitDspConsignmnt, null);
			AssertContainsExactElementsInExactOrder(new[] { dcn3, dcn2, dcn1 }, dcns);
		}
		
		public void TestGetChildCollection_TransitDspLoadList()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateWarehouse("TTT", "A", 3, 1);

			var dtu = helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll1 = helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu.PK);
			helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);
			Factory.Save();

			var testMenuItem = Factory.New<StmMenuItem>();
			testMenuItem.SU_MenuName = "test Menu";

			var docSupporter = ((IDocumentSupportable)dtu).DocumentSupporter;
			var dlls = docSupporter.GetChildCollection(testMenuItem, BusinessContext.TransitDspLoadList, null);
			AssertContainsExactElementsInAnyOrder(new[] { dll2, dll1 }, dlls);
		}

		#endregion
	}
}
