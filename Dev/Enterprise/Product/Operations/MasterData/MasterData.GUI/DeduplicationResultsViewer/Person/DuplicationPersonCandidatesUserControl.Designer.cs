using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	partial class DuplicationPersonCandidatesUserControl
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
			PhoneColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			TitleColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			RelatedToColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			IsDissolvedColumn = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			DuplicationCandidatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DuplicationCandidatesGrid)).BeginInit();
			this.DuplicationCandidatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.GUI.DeduplicationPersonResultDetail);

			this.BindingSource.SetBindingMember(this.DuplicationCandidatesGrid, "DuplicationCandidates");
			//The line(s) below are a compile-time check for a binding member.Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterData.Business.DuplicationCandidateCollection<DuplicationPersonCandidate>)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).ConfidenceDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).ConfidenceScore)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Active)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).IgnoredByStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).IgnoredForEveryoneStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).Title)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).RelatedTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterData.Business.DuplicationPersonCandidate)(((System.Collections.IList)(((Enterprise.MasterData.GUI.DeduplicationPersonResultDetail)(null)).DuplicationCandidates)).SyncRoot)).IsDissolved)));
			ConfidenceColumn.ColumnName = "ConfidenceDescription";
			ConfidenceColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ConfidenceColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("0c096df9-3b36-4fce-a0b5-42c4e1a67ba7", "Confidence");
			ConfidenceScoreColumn.ColumnName = "ConfidenceScore";
			ConfidenceScoreColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			ConfidenceScoreColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("3e8423c0-6e95-4a86-ae07-f55034bb5ace", "Score");
			TypeColumn.ColumnName = "Type";
			TypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			TypeColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("109a7967-c9ea-4db3-bca5-1135a9c845f0", "Active Associations");
			NameColumn.ColumnName = "Name";
			NameColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			NameColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("e4305471-e873-423c-9dee-548febbd05f3", "Name");
			ActiveColumn.ColumnName = "Active";
			ActiveColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			ActiveColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("e7f25d65-a821-48d3-aaf3-73fcb0174da3", "Active");
			StatusColumn.ColumnName = "Status";
			StatusColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			StatusColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("6dbb5b3b-ba25-44d9-bf00-482798c01c3e", "Status");
			IgnoredByStaffColumn.ColumnName = "IgnoredByStaff";
			IgnoredByStaffColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			IgnoredByStaffColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("439c8e80-4046-4ff5-96ea-571ce3f31bbe", "Ignored By");
			IgnoredForEveryoneStaffColumn.ColumnName = "IgnoredForEveryoneStaff";
			IgnoredForEveryoneStaffColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			IgnoredForEveryoneStaffColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("1158db85-005c-4f97-814c-8e568f2d44a8", "Ignored for Everyone");
			PhoneColumn.ColumnName = "Phone";
			PhoneColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			PhoneColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("dc5a6522-d4d6-44f9-bc14-aa7966d006dd", "Phone");
			TitleColumn.ColumnName = "Title";
			TitleColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			TitleColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("d905d4df-cd7d-4d7c-8db7-e338bbbeaad9", "Title");
			RelatedToColumn.ColumnName = "RelatedTo";
			RelatedToColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			RelatedToColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("fcd63090-ce83-474e-8832-a7c63011182a", "Related To");
			IsDissolvedColumn.ColumnName = "IsDissolved";
			IsDissolvedColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			IsDissolvedColumn.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("29d7e6da-f161-40cb-bfae-9f851b7bc3a2", "Dissolve into Master");
			this.DuplicationCandidatesGrid.AllowNavigation = false;
			this.DuplicationCandidatesGrid.CaptionVisible = false;
			this.DuplicationCandidatesGrid.DisableImportDataMenuItem = true;
			this.DuplicationCandidatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DuplicationCandidatesGrid.GridId = "788d10e1-bed8-43c0-a346-982198134232";
			this.DuplicationCandidatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DuplicationCandidatesGrid.LayoutKey = "DuplicationPersonCandidatesGrid";
			this.DuplicationCandidatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DuplicationCandidatesGrid.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 60, true);
			this.DuplicationCandidatesGrid.Name = "DuplicationPersonCandidatesGrid";
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
			this.Name = "DuplicationPersonCandidatesUserControl";
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
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo PhoneColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo TitleColumn;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo RelatedToColumn;
		private Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo IsDissolvedColumn;

		#endregion
	}
}
