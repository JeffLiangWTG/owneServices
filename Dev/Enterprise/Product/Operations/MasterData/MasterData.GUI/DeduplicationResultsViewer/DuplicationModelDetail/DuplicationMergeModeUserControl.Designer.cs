using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	partial class DuplicationMergeModeUserControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.MergeModeLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.MergeModeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MergeModeLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeModeGrid)).BeginInit();
			this.MergeModeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Business.DuplicationModelDetailCollection);
			// 
			// MergeModeLayoutPanel
			// 
			this.MergeModeLayoutPanel.AutoSize = true;
			this.MergeModeLayoutPanel.ColumnCount = 1;
			this.MergeModeLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(13)));
			this.MergeModeLayoutPanel.Controls.Add(this.MergeModeGrid, 0, 1);
			this.MergeModeLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MergeModeLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MergeModeLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MergeModeLayoutPanel.Name = "MergeModeLayoutPanel";
			this.MergeModeLayoutPanel.RowCount = 2;
			this.MergeModeLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.MergeModeLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MergeModeLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
			this.MergeModeLayoutPanel.TabIndex = 0;
			// 
			// MergeModeGrid
			// 
			this.MergeModeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MergeModeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterData.Business.DuplicationModelDetail)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationModelDetail)(null)).MergeModeString)));
			this.MergeModeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "MergeModeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("458fb8df-6ec6-4d88-b7b2-674cb4c23a6e", "Merge Type");
			zDropEditColumnStyleInfo1.ColumnName = "MergeModeString";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsSortable = false;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.MergeModeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MergeModeGrid.DisableImportDataMenuItem = true;
			this.MergeModeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MergeModeGrid.GridId = "3518401f-c863-4c6a-a48f-c5744e9324a3";
			this.MergeModeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MergeModeGrid.LayoutKey = "MergeModeGrid";
			this.MergeModeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.MergeModeGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MergeModeGrid.Name = "MergeModeGrid";
			this.MergeModeGrid.RemoveAction = RemoveAction.NoRemovePossible;
			this.MergeModeGrid.ShowMassUpdateMenuItem = false;
			this.MergeModeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 280, true);
			this.MergeModeGrid.TabIndex = 1;
			// 
			// DuplicationMergeModeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MergeModeLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "DuplicationMergeModeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MergeModeLayoutPanel.ResumeLayout(false);
			this.MergeModeLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeModeGrid)).EndInit();
			this.MergeModeGrid.ResumeLayout(false);
			this.MergeModeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private CargoWise.Windows.UI.KTableLayoutPanel MergeModeLayoutPanel;
		private Enterprise.ZArchitecture.ZGrid MergeModeGrid;

		#endregion
	}
}
