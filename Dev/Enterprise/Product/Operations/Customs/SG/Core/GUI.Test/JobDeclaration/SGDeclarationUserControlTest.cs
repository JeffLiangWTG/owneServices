using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class SGDeclarationUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestSGDeclarationUserControl()
		{
			var control = new SGDeclarationUserControl();
			control.Dispose();
		}

		public void TestOrganisationModuleIDs()
		{
			Declaration.JE_MessageType = "";
			using (ZForm form = new ZForm(Declaration))
			{
				using (SGDeclarationUserControl sgDeclarationUserControl = new SGDeclarationUserControl())
				{
					sgDeclarationUserControl.JobDeclaration = Declaration;
					((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(sgDeclarationUserControl, ".");
					form.Controls.Add(sgDeclarationUserControl);
					form.Show();
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.ClaimantGuidFindBox.ModuleID);
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.ConsigneeGuidFindBox.ModuleID);
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.OutwardCarrierAgentGuidFindBox.ModuleID);
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.InwardCarrierAgentGuidFindBox.ModuleID);
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.HandlingAgentGuidFindBox.ModuleID);
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.ForwarderGuidFindBox.ModuleID);
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.ExporterGuidFindBox.ModuleID);
					AssertEquals(ModuleIDs.Organisation, sgDeclarationUserControl.EndUserGuidFindBox.ModuleID);
				}
			}
		}

		public void TestCOTabIsHiddenOnCondition()
		{
			Declaration.JE_MessageType = "";
			using (ZForm form = new ZForm(Declaration))
			{
				using (SGDeclarationUserControl sgDeclarationUserControl = new SGDeclarationUserControl())
				{
					sgDeclarationUserControl.JobDeclaration = Declaration;
					form.Controls.Add(sgDeclarationUserControl);
					form.Show();
					AssertNull(sgDeclarationUserControl.SGDeclarationTabControl.TabPages[sgDeclarationUserControl.CertificateOfOriginTabPage.Name]);
					sgDeclarationUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
					sgDeclarationUserControl.Visible = true;
					AssertNotNull(sgDeclarationUserControl.SGDeclarationTabControl.TabPages[sgDeclarationUserControl.CertificateOfOriginTabPage.Name]);
					sgDeclarationUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
					sgDeclarationUserControl.Visible = true;
					AssertNotNull(sgDeclarationUserControl.SGDeclarationTabControl.TabPages[sgDeclarationUserControl.CertificateOfOriginTabPage.Name]);
					sgDeclarationUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
					Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKO;
					sgDeclarationUserControl.Visible = true;
					AssertNull(sgDeclarationUserControl.SGDeclarationTabControl.TabPages[sgDeclarationUserControl.CertificateOfOriginTabPage.Name]);
					sgDeclarationUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
					sgDeclarationUserControl.Visible = true;
					AssertNull(sgDeclarationUserControl.SGDeclarationTabControl.TabPages[sgDeclarationUserControl.CertificateOfOriginTabPage.Name]);
					sgDeclarationUserControl.Visible = false;
					Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
					sgDeclarationUserControl.Visible = true;
					AssertNotNull(sgDeclarationUserControl.SGDeclarationTabControl.TabPages[sgDeclarationUserControl.CertificateOfOriginTabPage.Name]);
				}
			}
		}

		public void TestControlsAreAdjustedForDifferentTradeNetVersions()
		{
			using (var form = new ZForm(Declaration))
			using (var sgDeclarationUserControl = new SGDeclarationUserControl())
			{
				sgDeclarationUserControl.JobDeclaration = Declaration;
				Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(sgDeclarationUserControl, ".");
				form.Controls.Add(sgDeclarationUserControl);
				form.Show();
				AssertEquals("TotalOtherTaxCalcEdit", false, sgDeclarationUserControl.TotalOtherTaxCalcEdit.Visible);
				AssertEquals("TotalOtherTaxLabel", false, sgDeclarationUserControl.TotalOtherTaxLabel.Visible);
				AssertEquals("StartDateLabel", "Removal Start Date:", sgDeclarationUserControl.StartDateLabel.Text);
			}
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}
