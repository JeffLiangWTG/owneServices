using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using System.Windows.Forms;

namespace Enterprise.MasterData.GUI
{
	partial class PersonMergeSummaryRetainedUserControls
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
			this.RetainedBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RetainedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RetainedBoundGrid)).BeginInit();
			this.RetainedBoundGrid.SuspendLayout();
			this.RetainedGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Business.PersonMergeParticipants);
			// 
			// RetainedBoundGrid
			// 
			this.RetainedBoundGrid.AllowNavigation = false;
			this.RetainedBoundGrid.AllowSorting = false;
			this.RetainedBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RetainedBoundGrid, "RetainedCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).RetainedCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.PersonMergeBusinessObject)(((System.Collections.IList)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).RetainedCollection)).SyncRoot)).FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.PersonMergeBusinessObject)(((System.Collections.IList)(((Enterprise.MasterData.Business.PersonMergeParticipants)(null)).RetainedCollection)).SyncRoot)).ActiveAssociations)));
			this.RetainedBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("018AB529-C290-4928-8FB1-0F1F2C798D1C", "Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FullName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("2E00E1A6-6C35-4C17-8A66-42A59E407B28", "Active Associations");
			zTextBoxColumnStyleInfo2.ColumnName = "ActiveAssociations";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			this.RetainedBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RetainedBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RetainedBoundGrid.GridId = "AD2D44C4-50AF-4B05-AA80-392E77647E63";
			this.RetainedBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RetainedBoundGrid.LayoutKey = "RetainedBoundGrid";
			this.RetainedBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.RetainedBoundGrid.Name = "RetainedBoundGrid";
			this.RetainedBoundGrid.ReadOnly = true;
			this.RetainedBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 48, true);
			this.RetainedBoundGrid.TabIndex = 1;
			this.RetainedBoundGrid.MouseDown += new MouseEventHandler(Grid_MouseDown);
			this.RetainedBoundGrid.KeyDown += new KeyEventHandler(Grid_KeyDown);
			this.RetainedBoundGrid.IsWholeRowSelectedOnClick = true;
			// 
			// RetainedGroupBox
			// 
			this.RetainedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RetainedGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("eec34d51-9eb3-463e-b2ee-9e4fd01bb0e2", "Retained Person");
			this.RetainedGroupBox.Controls.Add(this.RetainedBoundGrid);
			this.RetainedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.RetainedGroupBox.Name = "RetainedGroupBox";
			this.RetainedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 80, true);
			this.RetainedGroupBox.TabIndex = 3;
			this.RetainedGroupBox.TabStop = false;
			// 
			// PersonMergeSummaryRetainedUserControls
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RetainedGroupBox);
			this.Name = "PersonMergeSummaryRetainedUserControls";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 96, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RetainedBoundGrid)).EndInit();
			this.RetainedBoundGrid.ResumeLayout(false);
			this.RetainedBoundGrid.PerformLayout();
			this.RetainedGroupBox.ResumeLayout(false);
			this.RetainedGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZGrid RetainedBoundGrid;
		protected ZGroupBox RetainedGroupBox;
	}
}
