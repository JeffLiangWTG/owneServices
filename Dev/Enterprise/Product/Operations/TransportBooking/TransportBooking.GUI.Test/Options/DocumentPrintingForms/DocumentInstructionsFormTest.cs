using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.GUI.Options.DocumentPrintingForms
{
	[TestedType(typeof(DocumentInstructionsForm))]
	sealed class DocumentInstructionsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var documentInstructions = new DocumentInstructions(new InstructionToSelectFromForPrintingCollection(Factory));
			return new DocumentInstructionsForm(documentInstructions);
		}

		public void TestGridColumns()
		{
			var documentInstructions = new DocumentInstructions(new InstructionToSelectFromForPrintingCollection(Factory));
			using (var form = new DocumentInstructionsForm(documentInstructions))
			{
				form.Show();
				var grid = form.Controls.Find("instructionsGrid", true)[0] as ZGrid;
				var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				var instructionTypeColumn = columns.First(c => c.ColumnName == "Instruction+KN_InstructionType");
				var orgTypeColumn = columns.First(c => c.ColumnName == "Instruction+OrganisationType");
				var orgCodeColumn = columns.First(c => c.ColumnName == "Instruction+Address+OrganisationNameOrPK");
				var orgAddressColumn = columns.First(c => c.ColumnName == "Instruction+Address+E2_OA_Address");
				var printCheckboxColumn = columns.First(c => c.ColumnName == "KN_Calc_PrintDocumentForInstruction");
				AssertContainsExactElementsInAnyOrder("All columns listed", [instructionTypeColumn, orgTypeColumn, orgCodeColumn, orgAddressColumn, printCheckboxColumn], columns);
			}
		}

		public void TestReadOnlyColumns()
		{
			var documentInstructions = new DocumentInstructions(new InstructionToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentInstructionsForm(documentInstructions))
			{
				form.Show();

				var grid = form.Controls.Find("instructionsGrid", true)[0] as ZGrid;
				var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				var instructionTypeColumn = columns.First(c => c.ColumnName == "Instruction+KN_InstructionType");
				var orgTypeColumn = columns.First(c => c.ColumnName == "Instruction+OrganisationType");
				var orgCodeColumn = columns.First(c => c.ColumnName == "Instruction+Address+OrganisationNameOrPK");
				var orgAddressColumn = columns.First(c => c.ColumnName == "Instruction+Address+E2_OA_Address");
				var printCheckboxColumn = columns.First(c => c.ColumnName == "KN_Calc_PrintDocumentForInstruction");

				Assert("Instruction Type should be read only.", instructionTypeColumn.IsReadOnly);
				Assert("Org type should be read only.", orgTypeColumn.IsReadOnly);
				Assert("Org Code should be read only.", orgCodeColumn.IsReadOnly);
				Assert("Org Address should be read only.", orgAddressColumn.IsReadOnly);
				Assert("Print check box should not be read only.", !printCheckboxColumn.IsReadOnly);
			}
		}

		public void TestDialogResult()
		{
			var instruction = Factory.NewWithValidTestData<DtbBookingInstruction>();
			var instructionToSelect = new InstructionToSelectFromForPrinting(instruction);
			var instructionsToSelectFrom = new InstructionToSelectFromForPrintingCollection(Factory);
			instructionsToSelectFrom.Add(instructionToSelect);

			var docInstructionsBizObject = new DocumentInstructions(instructionsToSelectFrom);

			using (var form = new DocumentInstructionsForm(docInstructionsBizObject))
			{
				form.Show();

				var printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				UnitTestUserNotification.Instance.ClearMessages();

				printButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertContains("Please select an instruction.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			instructionToSelect.KN_Calc_PrintDocumentForInstruction = true;
			using (var form = new DocumentInstructionsForm(docInstructionsBizObject))
			{
				form.Show();

				var printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				printButton.PerformClick();
				AssertEquals(form.DialogResult, DialogResult.Yes);
			}

			using (var form = (DocumentInstructionsForm)GetFormToBash())
			{
				form.Show();

				var cancelPrintButton = form.Controls.Find("CancelPrintButton", true)[0] as ZButton;
				cancelPrintButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.No);
			}
		}
	}
}
