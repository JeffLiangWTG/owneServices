using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class SupportingInformationControlTest : TestCaseWithFactory
	{
		public void TestGetSupportingDocumentsUserControlType()
		{
			using (var control = new SupportingInformationControlForTest())
			{
				AssertEquals(typeof(SupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlTypeExposed);
			}
		}

		public void TestGetAdditionalInfosUserControlTypeExposed()
		{
			using (var control = new SupportingInformationControlForTest())
			{
				AssertEquals(typeof(PlugIn.AdditionalInfosUserControl), control.GetAdditionalInfosUserControlTypeExposed);
			}
		}

		public void TestGetAdditionalInfosColumnWidths()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var tab = form.FindSingle<ZTabControl>("SupportingInformationTabControl");
				var addTabPage = tab.FindSingle<ZTabPage>("AdditionalInfoTabPage");

				tab.SelectedTab = addTabPage;

				var grid = form.FindSingle<ZGrid>("AdditionalInfosGrid");
				var description = grid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == "CSI_Description") as ZTextBoxColumnStyleInfo;
				AssertEquals("CSI_Description", 520, description.Width);
			}
		}

		public void TestTariffQuestionControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new SupportingInformationControl())
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

		public void TestTabPagesOrder()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.MiscOptionsTabPage;
				var tab = form.FindSingle<ZTabControl>("SupportingInformationTabControl");
				var tabPages = tab.TabPages;
				AssertArrayEqualsByElements("The order of Supporting Information Tab Control is equal to this order", new[] { "TariffQuestionsTabPage", "SupportingDocumentTabPage", "AdditionalInfoTabPage", "PreviousDocumentTabPage" }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
			}
		}
	}

	class SupportingInformationControlForTest : SupportingInformationControl
	{
		public Type GetSupportingDocumentsUserControlTypeExposed => base.GetSupportingDocumentsUserControlType();

		public Type GetAdditionalInfosUserControlTypeExposed => base.GetAdditionalInfosUserControlType();
	}
}
