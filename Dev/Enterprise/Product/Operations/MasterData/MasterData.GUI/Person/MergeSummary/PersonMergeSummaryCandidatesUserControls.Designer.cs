using Enterprise.ZArchitecture.GUI;
using System.Windows.Forms;

namespace Enterprise.MasterData.GUI
{
	partial class PersonMergeSummaryCandidatesUserControls
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleForPersonMergeInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleForPersonMergeInfo();
			this.CandidatesBoundGrid = new PersonMergeZGrid();
			this.CandidatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CandidatesBoundGrid)).BeginInit();
			this.CandidatesBoundGrid.SuspendLayout();
			this.CandidatesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Business.PersonMergeParticipants);
			// 
			// CandidatesBoundGrid
			// 
			this.CandidatesBoundGrid.AllowNavigation = false;
			this.CandidatesBoundGrid.AllowSorting = true;
			this.CandidatesBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CandidatesBoundGrid, "DissolvedCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).DissolvedCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.PersonMergeBusinessObject)(((System.Collections.IList)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).DissolvedCollection)).SyncRoot)).FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.PersonMergeBusinessObject)(((System.Collections.IList)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).DissolvedCollection)).SyncRoot)).ActiveAssociations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.PersonMergeBusinessObject)(((System.Collections.IList)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).DissolvedCollection)).SyncRoot)).MergeStatus)));
			this.CandidatesBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("678397F6-F333-4F74-8BAB-563CA91AA42C", "Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FullName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("726F7F1C-48F9-4B58-BFD7-02D87104CB88", "Active Associations");
			zTextBoxColumnStyleInfo2.ColumnName = "ActiveAssociations";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("30C9492D-4B3E-485F-92B8-EE0B5B96465F", "Merge Status");
			zTextBoxColumnStyleInfo3.ColumnName = "MergeStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.CandidatesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CandidatesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CandidatesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CandidatesBoundGrid.GridId = "40F06B3E-6E93-423E-8A01-515F4947348E";
			this.CandidatesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CandidatesBoundGrid.LayoutKey = "CandidatesBoundGrid";
			this.CandidatesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.CandidatesBoundGrid.Name = "CandidatesBoundGrid";
			this.CandidatesBoundGrid.ReadOnly = true;
			this.CandidatesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 176, true);
			this.CandidatesBoundGrid.TabIndex = 1;
			this.CandidatesBoundGrid.MouseDown += new MouseEventHandler(Grid_MouseDown);
			this.CandidatesBoundGrid.KeyDown += new KeyEventHandler(Grid_KeyDown);
			this.CandidatesBoundGrid.IsWholeRowSelectedOnClick = true;
			// 
			// CandidatesGroupBox
			// 
			this.CandidatesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CandidatesGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("98db5fa5-17ba-42dd-980c-9cd137bea283", "Potential Duplicates");
			this.CandidatesGroupBox.Controls.Add(this.CandidatesBoundGrid);
			this.CandidatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.CandidatesGroupBox.Name = "CandidatesGroupBox";
			this.CandidatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 208, true);
			this.CandidatesGroupBox.TabIndex = 3;
			this.CandidatesGroupBox.TabStop = false;
			// 
			// PersonMergeSummaryCandidatesUserControls
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CandidatesGroupBox);
			this.Name = "PersonMergeSummaryCandidatesUserControls";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 224, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CandidatesBoundGrid)).EndInit();
			this.CandidatesBoundGrid.ResumeLayout(false);
			this.CandidatesBoundGrid.PerformLayout();
			this.CandidatesGroupBox.ResumeLayout(false);
			this.CandidatesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal PersonMergeZGrid CandidatesBoundGrid;
		protected ZGroupBox CandidatesGroupBox;
	}
}
