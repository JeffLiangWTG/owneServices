using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

class NLFiscalReferencesUserControlTests : TestCaseWithFactory
{
	public void TestEntryInstructionVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var form = new ZForm(declaration))
		{
			using (var control = new NLFiscalReferencesUserControl())
			{
				control.JobDeclaration = declaration;

				form.Controls.Add(control);
				form.Show();

				AssertEquals("Fiscal References", control.FindSingle<ZGroupBox>(x => x.Name == "FiscalReferencesGroupBox").Text);

				var ctrl = control.FindSingle<ZGuidDropEdit>(x => x.Name == "FiscalReferencesCusEntryInstructionDropEdit");
				var colStyle = control.FindSingle<ZGrid>(x => x.Name == "FiscalReferencesGrid").GetColumnStyle(FiscalReference.Schema.EntryInstructionID);

				control.ShowHideEntryInstruction(true);
				AssertEquals(true, ctrl.Visible);
				AssertEquals(false, colStyle.IsUnavailable);

				control.ShowHideEntryInstruction(false);
				AssertEquals(false, ctrl.Visible);
				AssertEquals(true, colStyle.IsUnavailable);
			}
		}
	}
}
