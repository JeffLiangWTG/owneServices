using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	sealed class ImportEntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestDutyAndTaxDetailsUserControlType()
		{
			using (var form = new Form())
			using (var entryLineAdditionalDataUserControl = new ImportEntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(entryLineAdditionalDataUserControl);
				form.Show();
				AssertEquals("Using Correct EntryLineTaxAndFeeUserControl", typeof(EntryLineTaxAndFeeUserControl), entryLineAdditionalDataUserControl.DutyAndTaxDetails.UserControlType);
			}
		}

		public void TestTariffQuestionControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new ImportEntryLineAdditionalDataUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var tabPage = control.FindSingle<ZTabPage>("TariffQuestionsTabPage");
				AssertEquals(true, tabPage.TabVisible);
				var grid = control.FindSingle<ZGrid>("TariffQuestionsGrid");
				var columnStyle = grid.GetColumnStyle("ON_QuestionType");
				AssertEquals("Type", columnStyle.CaptionResourceString.Caption);
				columnStyle = grid.GetColumnStyle("ON_CPDecNum");
				AssertEquals("Code", columnStyle.CaptionResourceString.Caption);
				columnStyle = grid.GetColumnStyle("ON_AnswerCode");
				AssertEquals("Answer", columnStyle.CaptionResourceString.Caption);
				columnStyle = grid.GetColumnStyle("Description");
				AssertEquals("Description", columnStyle.CaptionResourceString.Caption);
			}
		}
	}
}

