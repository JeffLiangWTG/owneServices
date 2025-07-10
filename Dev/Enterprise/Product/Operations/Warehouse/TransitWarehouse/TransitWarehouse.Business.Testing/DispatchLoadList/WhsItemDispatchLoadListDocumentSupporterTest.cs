using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListDocumentSupporter))]
	class WhsItemDispatchLoadListDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var loadList = Factory.New<WhsItemDispatchLoadList>();
			var docSupporter = ((IDocumentSupportable)loadList).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var loadList = Factory.New<WhsItemDispatchLoadList>();
			var docSupporter = ((IDocumentSupportable)loadList).DocumentSupporter;
			AssertEquals(Env.Security.WhsItemDispatchLoadListCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuName.Contains("Labels for All DCNs") || base.ExcludeDocumentCommandTest(documentCommand);
		}

		#region TestDocumentSupporter_SupportedChildBusinessContexts

		public void TestDocumentSupporter_SupportedChildBusinessContexts()
		{
			var dll = Factory.New<WhsItemDispatchLoadList>();
			var docSupporter = ((IDocumentSupportable)dll).DocumentSupporter;
			AssertContainsExactElementsInAnyOrder(new BusinessContext[] { BusinessContext.TransitDspConsignmnt, BusinessContext.TransitDispTranspUnt }, docSupporter.SupportedChildBusinessContexts);
		}

		#endregion

		#region TestGetChildCollection

		public void TestGetChildCollection_TransitDspConsignmnt_OrderByDCNHouseBillNumber()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateWarehouse("TTT", "A", 3, 1);
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dll = helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var dcn1 = helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			dcn1.WDC_HouseBillNumber = "HB1";
			helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn1, dispatchLoadList: dll);

			var dcn2 = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn2.WDC_HouseBillNumber = "HB2";
			helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn2, dispatchLoadList: dll);
			Factory.Save();

			var testMenuItem = Factory.New<StmMenuItem>();
			testMenuItem.SU_MenuName = "test Menu";

			var docSupporter = ((IDocumentSupportable)dll).DocumentSupporter;
			var dcns = docSupporter.GetChildCollection(testMenuItem, BusinessContext.TransitDspConsignmnt, null);
			AssertContainsExactElementsInExactOrder(new[] { dcn1, dcn2 }, dcns);
		}

		public void TestGetChildCollection_TransitDspConsignmnt_NoHouseBillNumber_OrderByDCNConsignmentIDAsFallBack()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateWarehouse("TTT", "A", 3, 1);
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dll = helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var dcn1 = helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn1, dispatchLoadList: dll);

			var dcn2 = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn2, dispatchLoadList: dll);
			Factory.Save();

			var testMenuItem = Factory.New<StmMenuItem>();
			testMenuItem.SU_MenuName = "test Menu";

			var docSupporter = ((IDocumentSupportable)dll).DocumentSupporter;
			var dcns = docSupporter.GetChildCollection(testMenuItem, BusinessContext.TransitDspConsignmnt, null);
			AssertContainsExactElementsInExactOrder(new[] { dcn2, dcn1 }, dcns);
		}

		#endregion

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<WhsItemDispatchLoadList>();
		}

		#endregion
	}
}
