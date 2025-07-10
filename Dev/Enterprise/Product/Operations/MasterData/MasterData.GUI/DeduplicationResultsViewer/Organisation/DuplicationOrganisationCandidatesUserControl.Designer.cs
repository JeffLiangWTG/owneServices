using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	partial class DuplicationOrganisationCandidatesUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ConfidenceColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			ConfidenceScoreColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			TypeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			NameColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			ActiveColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			StatusColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			IgnoredByStaffColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			IgnoredForEveryoneStaffColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			CodeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			UNLOCOColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			DebtorCompanyColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			CreditorCompanyColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			AssociatedConsolsCountColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			AssociatedDeclarationsCountColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			AssociatedShipmentsCountColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			EnterpriseIdColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			EnterpriseCodeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			CompanyCodeColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			ProductIdColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			DuplicationCandidatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicationCandidatesGrid)).BeginInit();
			this.DuplicationCandidatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail);

			this.BindingSource.SetBindingMember(this.DuplicationCandidatesGrid, "DuplicationCandidates");
			//The line(s) below are a compile-time check for a binding member.Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterData.Business.DuplicationCandidateCollection<DuplicationOrganisationCandidate>)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).ConfidenceDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).ConfidenceScore)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Active)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).IgnoredByStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).IgnoredForEveryoneStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).UNLOCO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).DebtorCompany)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).CreditorCompany)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).AssociatedConsolsCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).AssociatedDeclarationsCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).AssociatedShipmentsCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).EnterpriseId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).EnterpriseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).CompanyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationOrganisationCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationOrganisationResultDetail)(null)).DuplicationCandidates)).SyncRoot)).ProductId)));
			ConfidenceColumn.ColumnName = "ConfidenceDescription";
			ConfidenceColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ConfidenceColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a21ea408-6d81-4d3a-94d0-7ab3fbf5ddfb", "Confidence");
			ConfidenceScoreColumn.ColumnName = "ConfidenceScore";
			ConfidenceScoreColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			ConfidenceScoreColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("e07841b0-7a32-4b28-97c8-cf041b47fe9f", "Score");
			TypeColumn.ColumnName = "Type";
			TypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			TypeColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("79cd87b4-7fcf-4a99-9c6a-8e64d95f81cc", "Type");
			NameColumn.ColumnName = "Name";
			NameColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			NameColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a91451e5-439f-4152-b8c5-1fdd4b6bed44", "Name");
			ActiveColumn.ColumnName = "Active";
			ActiveColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ActiveColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("725baa89-4d3a-438d-ad00-99ae9b81d227", "Active");
			StatusColumn.ColumnName = "Status";
			StatusColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			StatusColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("75211c26-a6e9-46df-ba6a-bf9e2b006462", "Status");
			IgnoredByStaffColumn.ColumnName = "IgnoredByStaff";
			IgnoredByStaffColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			IgnoredByStaffColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("351377d2-1e58-49ac-9829-8890568b6904", "Ignored By");
			IgnoredForEveryoneStaffColumn.ColumnName = "IgnoredForEveryoneStaff";
			IgnoredForEveryoneStaffColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			IgnoredForEveryoneStaffColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("fca08531-3983-4bf1-af5b-98be247aaea7", "Ignored for Everyone");
			CodeColumn.ColumnName = "Code";
			CodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CodeColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("af9a4ad7-c415-4862-a183-0a3783d34692", "Code");
			UNLOCOColumn.ColumnName = "UNLOCO";
			UNLOCOColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			UNLOCOColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("2dd5e82c-ba9a-40d8-a78c-e298ced1e9d5", "UNLOCO");
			DebtorCompanyColumn.ColumnName = "DebtorCompany";
			DebtorCompanyColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			DebtorCompanyColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("315d56d8-08a4-4060-a61e-32ac57f31a3a", "Debtor Company");
			CreditorCompanyColumn.ColumnName = "CreditorCompany";
			CreditorCompanyColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CreditorCompanyColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("cf99d0d2-ab34-4511-8490-1d7b76b45bbb", "Creditor Company");
			AssociatedConsolsCountColumn.ColumnName = "AssociatedConsolsCount";
			AssociatedConsolsCountColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			AssociatedConsolsCountColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("6458ce1f-4730-4194-91ec-f386adbf7ce1", "Consols");
			AssociatedDeclarationsCountColumn.ColumnName = "AssociatedDeclarationsCount";
			AssociatedDeclarationsCountColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			AssociatedDeclarationsCountColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a21d2aac-bec7-458e-9cd2-bc2fed378c5d", "Declarations");
			AssociatedShipmentsCountColumn.ColumnName = "AssociatedShipmentsCount";
			AssociatedShipmentsCountColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			AssociatedShipmentsCountColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("2c3b4d8c-7078-4f46-ad86-f1d16e57469a", "Shipments");
			EnterpriseIdColumn.ColumnName = "EnterpriseId";
			EnterpriseIdColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			EnterpriseIdColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("8f306a1d-1b09-45e0-9103-ad7a192445e4", "Enterprise ID");
			EnterpriseCodeColumn.ColumnName = "EnterpriseCode";
			EnterpriseCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			EnterpriseCodeColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("f2d1c687-849c-4268-b084-cd779a918f7f", "Enterprise Code");
			CompanyCodeColumn.ColumnName = "CompanyCode";
			CompanyCodeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			CompanyCodeColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("854eb3ae-01ea-4fa0-a355-4888eb9e31b6", "Company Code");
			ProductIdColumn.ColumnName = "ProductId";
			ProductIdColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ProductIdColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("8c586239-07db-4884-8774-9cbd62271773", "Product");
			this.DuplicationCandidatesGrid.AllowNavigation = false;
			this.DuplicationCandidatesGrid.CaptionVisible = false;
			this.DuplicationCandidatesGrid.DisableImportDataMenuItem = true;
			this.DuplicationCandidatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DuplicationCandidatesGrid.GridId = "60d35dba-56a0-42c7-ae56-8591c6221a07";
			this.DuplicationCandidatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DuplicationCandidatesGrid.LayoutKey = "DuplicationOrganisationCandidatesGrid";
			this.DuplicationCandidatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DuplicationCandidatesGrid.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 60, true);
			this.DuplicationCandidatesGrid.Name = "DuplicationOrganisationCandidatesGrid";
			this.DuplicationCandidatesGrid.RemoveAction = RemoveAction.NoRemovePossible;
			this.DuplicationCandidatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 200, true);
			this.DuplicationCandidatesGrid.ShowMassUpdateMenuItem = false;
			this.DuplicationCandidatesGrid.TabIndex = 0;
			// 
			// DuplicationCandidatesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DuplicationCandidatesGrid);
			this.Name = "DuplicationOrganisationCandidatesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicationCandidatesGrid)).EndInit();
			this.DuplicationCandidatesGrid.ResumeLayout(false);
			this.DuplicationCandidatesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.ZGrid DuplicationCandidatesGrid;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ConfidenceColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ConfidenceScoreColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo TypeColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo NameColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ActiveColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo StatusColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo IgnoredByStaffColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo IgnoredForEveryoneStaffColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo CodeColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo UNLOCOColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo DebtorCompanyColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo CreditorCompanyColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo AssociatedConsolsCountColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo AssociatedDeclarationsCountColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo AssociatedShipmentsCountColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo EnterpriseIdColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo EnterpriseCodeColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo CompanyCodeColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ProductIdColumn;

		#endregion
	}
}
