using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Options.DocumentPrintingForms
{
	sealed class InstructionSelectorTest : TestCaseWithFactory
	{
		public void TestCanOnlySelectOneAtATime()
		{
			var instructions = new[]
			{
				Factory.New<DtbBookingInstruction>(),
				Factory.New<DtbBookingInstruction>()
			};

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(OnSelectionFormShow);

			var selector = new InstructionSelector();
			var selectedInstructions = selector.SelectInstruction(instructions);

			void OnSelectionFormShow(object formOrDialog)
			{
				if (formOrDialog is DocumentInstructionsForm form
					&& form.DataSource is DocumentInstructions documentInstructions)
				{
					var instruction1 = documentInstructions.InstructionsToSelectFrom[0];
					var instruction2 = documentInstructions.InstructionsToSelectFrom[1];

					instruction1.KN_Calc_PrintDocumentForInstruction = true;

					Assert("First Instruction should be selected", instruction1.KN_Calc_PrintDocumentForInstruction);
					Assert("Second Instruction should not be selected", !instruction2.KN_Calc_PrintDocumentForInstruction);

					instruction2.KN_Calc_PrintDocumentForInstruction = true;

					Assert("First Instruction should've been unselected", !instruction1.KN_Calc_PrintDocumentForInstruction);
					Assert("Second Instruction should be selected", instruction2.KN_Calc_PrintDocumentForInstruction);
				}
				else
				{
					Fail("invalid form was shown");
				}
			}
		}

		public void TestIsAccessibleViaObjectFactory()
		{
			var selector = ObjectFactory.Get<IInstructionSelector>();
			Assert("InstructionSelector can be accessed via ObjectFactory", selector is InstructionSelector);
		}
	}
}
