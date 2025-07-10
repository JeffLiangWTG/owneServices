using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetVisualizableDocumentSupporter))]
	class DtbConsignmentRunSheetVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomizeFormCheckpoint()
		{
			AssertEquals(nameof(supporter.CustomizeFormCheckpoint), Env.Security.DtbConsignmentRunSheetCustomiseForms, supporter.CustomizeFormCheckpoint);
		}

		public void TestGetDocDataObject()
		{
			var docDataObject = supporter.GetDocDataObject(runSheet, DataContext.CMRConsignmentNote, null);
			AssertNotNull(nameof(docDataObject.Right), docDataObject.Right);
			AssertType<CMRConsignmentNoteDocDataObjectCollection>($"{nameof(docDataObject.Right)} type", docDataObject.Right);
		}

		public void TestGetAdditionalData_CMRConsignment()
		{
			//run sheet does not contain any instructions
			AssertEquals("Precondition", 0, runSheet.RunSheetInstructions.Count);

			var menuItem = CreateMenuItem(DataContext.CMRConsignmentNote);
			var data = supporter.GetAdditionalData(runSheet, menuItem);

			Assert(data.IsLeft);
			AssertEquals(
				"When run sheet doesn't have any consignments, supporter should show error message.",
				"Run Sheet does not contain any Consignments.",
				data.Left);

			var transportConsignmentHelper = new TransportConsignmentTestHelper(Factory);

			var consignment1 = transportConsignmentHelper.CreateConsignment();
			var address1 = transportConsignmentHelper.CreateConsignmentAddress(consignment1, ConsignmentAddressTypes.Codes.PickUp);
			var action1 = address1.Actions.AddNew();
			AssertNull(action1.RunSheetInstruction);
			var runSheetInstruction1 = runSheet.RunSheetInstructions.AddNew();
			action1.LTA_K1_RunSheetInstruction = runSheetInstruction1.PK;
			AssertEquals(runSheetInstruction1, action1.RunSheetInstruction);

			data = supporter.GetAdditionalData(runSheet, menuItem);

			Assert(data.IsRight);
		}

		protected override void SetUp()
		{
			base.SetUp();
			runSheet = Factory.NewWithValidTestData<DtbConsignmentRunSheet>();
			supporter = new DtbConsignmentRunSheetVisualizableDocumentSupporter(runSheet);
		}

		StmMenuItem CreateMenuItem(string context, string menuName = "")
		{
			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = context;
			var menuItem = Factory.New<DocumentCommand>();
			menuItem.SU_MenuName = menuName;
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			return menuItem;
		}

		DtbConsignmentRunSheet runSheet;
		DtbConsignmentRunSheetVisualizableDocumentSupporter supporter;
	}
}
