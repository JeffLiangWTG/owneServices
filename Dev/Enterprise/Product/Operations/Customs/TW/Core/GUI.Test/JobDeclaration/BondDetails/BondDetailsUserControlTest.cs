using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class BondDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsWhenExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var bondedDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "BondedDetailsTabPage");
					jobDeclarationUserControl.RightTabControl.SelectedTab = bondedDetailsTabPage;
					var bondDetailsUserControl = bondedDetailsTabPage.FindSingle<BondDetailsUserControl>(x => x.Name == "TWBondedDetailsUserControl");
					var topRightGroupBox = bondDetailsUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "TopRightGroupBox");
					AssertEquals("Materials", topRightGroupBox.Text);
					var relatedBondedPartiesGroupBox = bondDetailsUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "RelatedBondedPartiesGroupBox");
					AssertEquals("Related Bonded Parties", relatedBondedPartiesGroupBox.Text);
					var reasonForDutyDropEdit = bondDetailsUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "CEI_ReasonForDutyDropEdit");
					Assert("Should not be visible", !reasonForDutyDropEdit.Visible);
					var billOfMaterialsCheckBox = bondDetailsUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "CEI_BillOfMaterialsCheckBox");
					Assert("Should be visible", billOfMaterialsCheckBox.Visible);
					var dutyRefundCheckBox = bondDetailsUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "CEI_DutyRefundCheckBox");
					Assert("Should be visible", dutyRefundCheckBox.Visible);
					var billOfMaterialsPageNoCalcEdit = bondDetailsUserControl.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "CEI_BillOfMaterialsPageNoCalcEdit");
					Assert("Should not be visible", !billOfMaterialsPageNoCalcEdit.Visible);
					declaration.CusEntryInstruction.CEI_BillOfMaterials = true;
					Assert("Should be visible", billOfMaterialsPageNoCalcEdit.Visible);
				}
			}
		}

		public void TestControlsWhenImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.DeclarationTabPage;
				var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl.DeclarationUserControlForTesting;
				if (jobDeclarationUserControl != null)
				{
					var bondedDetailsTabPage = jobDeclarationUserControl.FindSingle<ZTabPage>(x => x.Name == "BondedDetailsTabPage");
					jobDeclarationUserControl.RightTabControl.SelectedTab = bondedDetailsTabPage;
					var bondDetailsUserControl = bondedDetailsTabPage.FindSingle<BondDetailsUserControl>(x => x.Name == "TWBondedDetailsUserControl");
					var topRightGroupBox = bondDetailsUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "TopRightGroupBox");
					AssertEquals("Domestic Sales", topRightGroupBox.Text);
					var relatedBondedPartiesGroupBox = bondDetailsUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "RelatedBondedPartiesGroupBox");
					AssertEquals("Previous Bonded Parties", relatedBondedPartiesGroupBox.Text);
					var reasonForDutyDropEdit = bondDetailsUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "CEI_ReasonForDutyDropEdit");
					Assert("Should be visible", reasonForDutyDropEdit.Visible);
					var billOfMaterialsCheckBox = bondDetailsUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "CEI_BillOfMaterialsCheckBox");
					Assert("Should not be visible", !billOfMaterialsCheckBox.Visible);
					var dutyRefundCheckBox = bondDetailsUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "CEI_DutyRefundCheckBox");
					Assert("Should not be visible", !dutyRefundCheckBox.Visible);
					var billOfMaterialsPageNoCalcEdit = bondDetailsUserControl.FindSingleOrDefault<ZCalcEdit>(c => c.Name == "CEI_BillOfMaterialsPageNoCalcEdit");
					Assert("Should not be visible", !billOfMaterialsPageNoCalcEdit.Visible);
					declaration.CusEntryInstruction.CEI_BillOfMaterials = true;
					Assert("Should not be visible", !billOfMaterialsPageNoCalcEdit.Visible);
				}
			}
		}
	}
}
