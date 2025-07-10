using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class SGTN41DeclarationUserControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestSGTN41DeclarationUserControl()
		{
			var control = new SGTN41DeclarationUserControl();
			control.Dispose();
		}

		public void TestOrganisationModuleIDs()
		{
			Declaration.JE_MessageType = "";
			using (var form = new ZForm(Declaration))
			using (var sGTN41DeclarationUserControl = new SGTN41DeclarationUserControl())
			{
				sGTN41DeclarationUserControl.JobDeclaration = Declaration;
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(sGTN41DeclarationUserControl, ".");
				form.Controls.Add(sGTN41DeclarationUserControl);
				form.Show();
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.ClaimantGuidFindBox.ModuleID);
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.ConsigneeGuidFindBox.ModuleID);
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.OutwardCarrierAgentGuidFindBox.ModuleID);
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.InwardCarrierAgentGuidFindBox.ModuleID);
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.HandlingAgentGuidFindBox.ModuleID);
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.ForwarderGuidFindBox.ModuleID);
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.ExporterGuidFindBox.ModuleID);
				AssertEquals(ModuleIDs.Organisation, sGTN41DeclarationUserControl.EndUserGuidFindBox.ModuleID);
			}
		}

		public void TestCOTabIsHiddenOnCondition()
		{
			Declaration.JE_MessageType = "";
			using (var form = new ZForm(Declaration))
			using (var sGTN41DeclarationUserControl = new SGTN41DeclarationUserControl())
			{
				sGTN41DeclarationUserControl.JobDeclaration = Declaration;
				form.Controls.Add(sGTN41DeclarationUserControl);
				form.Show();
				AssertNull(sGTN41DeclarationUserControl.SGDeclarationTabControl.TabPages[sGTN41DeclarationUserControl.CertificateOfOriginTabPage.Name]);
				sGTN41DeclarationUserControl.Visible = false;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
				sGTN41DeclarationUserControl.Visible = true;
				AssertNotNull(sGTN41DeclarationUserControl.SGDeclarationTabControl.TabPages[sGTN41DeclarationUserControl.CertificateOfOriginTabPage.Name]);
				sGTN41DeclarationUserControl.Visible = false;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
				sGTN41DeclarationUserControl.Visible = true;
				AssertNotNull(sGTN41DeclarationUserControl.SGDeclarationTabControl.TabPages[sGTN41DeclarationUserControl.CertificateOfOriginTabPage.Name]);
				sGTN41DeclarationUserControl.Visible = false;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
				Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKO;
				sGTN41DeclarationUserControl.Visible = true;
				AssertNull(sGTN41DeclarationUserControl.SGDeclarationTabControl.TabPages[sGTN41DeclarationUserControl.CertificateOfOriginTabPage.Name]);
				sGTN41DeclarationUserControl.Visible = false;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
				sGTN41DeclarationUserControl.Visible = true;
				AssertNull(sGTN41DeclarationUserControl.SGDeclarationTabControl.TabPages[sGTN41DeclarationUserControl.CertificateOfOriginTabPage.Name]);
				sGTN41DeclarationUserControl.Visible = false;
				Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
				sGTN41DeclarationUserControl.Visible = true;
				AssertNotNull(sGTN41DeclarationUserControl.SGDeclarationTabControl.TabPages[sGTN41DeclarationUserControl.CertificateOfOriginTabPage.Name]);
			}
		}

		public void TestControlsAreAdjustedForDifferentTradeNetVersions()
		{
			using (var form = new ZForm(Declaration))
			using (var sGTN41DeclarationUserControl = new SGTN41DeclarationUserControl())
			{
				sGTN41DeclarationUserControl.JobDeclaration = Declaration;
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(sGTN41DeclarationUserControl, ".");
				form.Controls.Add(sGTN41DeclarationUserControl);
				form.Show();
				AssertEquals("TotalOtherTaxCalcEdit", true, sGTN41DeclarationUserControl.TotalOtherTaxCalcEdit.Visible);
				AssertEquals("TotalOtherTaxLabel", true, sGTN41DeclarationUserControl.TotalOtherTaxLabel.Visible);
				AssertEquals("StartDateLabel", "Start Date:", sGTN41DeclarationUserControl.StartDateLabel.Text);
			}
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}
