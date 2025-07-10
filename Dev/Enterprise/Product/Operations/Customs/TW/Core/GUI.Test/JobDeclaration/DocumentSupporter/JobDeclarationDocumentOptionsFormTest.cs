using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(JobDeclarationDocumentOptionsForm))]
	sealed class JobDeclarationDocumentOptionsFormTest : ZFormBasherTest
	{
		public void TestAddressGroupBox()
		{
			var jobDeclarationDocumentAddressConfig = Declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			using (var form = new JobDeclarationDocumentOptionsForm(false, jobDeclarationDocumentAddressConfig))
			{
				form.Show();
				CombineAssertions(() =>
				{
					Assert("ImportAddressGroupBox should be visible", form.FindSingle<ZGroupBox>("ImportAddressGroupBox").Visible);
					Assert("ExportAddressGroupBox should be hidden", !form.FindSingle<ZGroupBox>("ExportAddressGroupBox").Visible);
				});
			}

			using (var form = new JobDeclarationDocumentOptionsForm(true, jobDeclarationDocumentAddressConfig))
			{
				form.Show();
				CombineAssertions(() =>
				{
					Assert("ImportAddressGroupBox should be hidden", !form.FindSingle<ZGroupBox>("ImportAddressGroupBox").Visible);
					Assert("ExportAddressGroupBox should be visible", form.FindSingle<ZGroupBox>("ExportAddressGroupBox").Visible);
				});
			}
		}

		public void TestCellSettingGroupBox()
		{
			var jobDeclarationDocumentAddressConfig = Declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig.SetDefaultJobDeclarationDocumentOptions();
			using (var form = new JobDeclarationDocumentOptionsForm(false, jobDeclarationDocumentAddressConfig))
			{
				form.Show();
				Assert("CellSettingGroupBox should be hidden", !form.FindSingle<ZGroupBox>("CellSettingGroupBox").Visible);
			}

			var documentSupporter = Declaration.DocumentSupporter;
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			menuItemForTesting.SU_BusinessContext = "Customs";
			menuItemForTesting.SU_MenuName = "Import Customs Declaration (Proof)";
			menuItemForTesting.SU_MenuPath = "Declaration Documents";
			menuItemForTesting.SU_ContactType = "CNE";
			documentSupporter.GetDataStateBeforeRun(menuItemForTesting);
			var jobDeclarationDocumentAddressConfig2 = documentSupporter.JobDeclarationDocumentAddressConfig;
			jobDeclarationDocumentAddressConfig2.SetDefaultJobDeclarationDocumentOptions();
			using (var form = new JobDeclarationDocumentOptionsForm(false, jobDeclarationDocumentAddressConfig2))
			{
				form.Show();
				Assert("CellSettingGroupBox should be visible", form.FindSingle<ZGroupBox>("CellSettingGroupBox").Visible);
			}
		}

		public void TestFormHeading()
		{
			using (var form = new JobDeclarationDocumentOptionsForm(Declaration.IsExport, Declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig))
			{
				AssertEquals("Customs Declaration Options", form.FormHeading);
			}
		}

		public void TestShowErrorsDialog()
		{
			using (var form = new JobDeclarationDocumentOptionsForm(Declaration.IsExport, Declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig))
			{
				bool formClosedFlag = false;
				form.FormClosed += (sender, e) => formClosedFlag = true;
				form.Show();
				var deliverButton = form.FindSingle<ZButton>("DeliverButton");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var goodsDescriptionConfigs = JobDeclarationDocumentAddressConfig.GoodsDescriptionConfigs;
				goodsDescriptionConfigs.RemoveAndDeleteAll();
				deliverButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("Error message", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult None", DialogResult.None, form.DialogResult);
					Assert("Form not closed", !formClosedFlag);
				});

				var goodsDescriptionConfig = goodsDescriptionConfigs.AddNew();
				goodsDescriptionConfig.Caption = "Test Caption:";
				goodsDescriptionConfig.Field = ImportDeclarationDocumentFieldList.Codes.Model;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				deliverButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("DialogResult OK", DialogResult.OK, form.DialogResult);
					Assert("Form closed", formClosedFlag);
				});
			}
		}

		public void TestFieldShowInDropDown()
		{
			var jobDeclarationDocumentAddressConfig = Declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;
			using (var form = new JobDeclarationDocumentOptionsForm(false, jobDeclarationDocumentAddressConfig))
			{
				form.Show();
				var fieldColumnStyle = (ZDropEditColumnStyleInfo)form.FindSingle<ZGrid>("GoodsDescriptionGrid").GetColumnStyle("Field");
				AssertEquals(Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, fieldColumnStyle.ShowInDropDown);
			}
		}

		protected override Form GetFormToBashCore() => new JobDeclarationDocumentOptionsForm(Declaration.IsExport, Declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig);

		JobDeclarationDocumentAddressConfig JobDeclarationDocumentAddressConfig => Declaration.DocumentSupporter.JobDeclarationDocumentAddressConfig;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = "IMP";
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
