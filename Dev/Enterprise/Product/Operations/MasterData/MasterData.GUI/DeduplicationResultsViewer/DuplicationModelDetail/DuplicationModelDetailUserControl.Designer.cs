using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	partial class DuplicationModelDetailUserControl
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
			this.components = new System.ComponentModel.Container();
			this.ModelDetailLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ModelDetailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModelDetailConfidenceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModelDetailGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ModelDetailScoreToolTip = new CargoWise.Windows.UI.KToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ModelDetailLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ModelDetailGrid)).BeginInit();
			this.ModelDetailGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Business.DuplicationModelDetailGroup);
			// 
			// ModelDetailLayoutPanel
			// 
			this.ModelDetailLayoutPanel.AutoSize = true;
			this.ModelDetailLayoutPanel.ColumnCount = 2;
			this.ModelDetailLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ModelDetailLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)));
			this.ModelDetailLayoutPanel.Controls.Add(this.ModelDetailLabel, 0, 0);
			this.ModelDetailLayoutPanel.Controls.Add(this.ModelDetailConfidenceLabel, 1, 0);
			this.ModelDetailLayoutPanel.Controls.Add(this.ModelDetailGrid, 0, 1);
			this.ModelDetailLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModelDetailLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModelDetailLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ModelDetailLayoutPanel.Name = "ModelDetailLayoutPanel";
			this.ModelDetailLayoutPanel.RowCount = 2;
			this.ModelDetailLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.ModelDetailLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ModelDetailLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
			this.ModelDetailLayoutPanel.TabIndex = 0;
			// 
			// ModelDetailLabel
			// 
			this.ModelDetailLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ModelDetailLabel, "Header");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationModelDetailGroup)(null)).Header)));
			this.ModelDetailLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.ModelDetailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ModelDetailLabel.IsFontBold = true;
			this.ModelDetailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModelDetailLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ModelDetailLabel.Name = "ModelDetailLabel";
			this.ModelDetailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.ModelDetailLabel.TabIndex = 0;
			this.ModelDetailLabel.UseMnemonic = false;
			// 
			// ModelDetailConfidenceLabel
			// 
			this.ModelDetailConfidenceLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ModelDetailConfidenceLabel, "Confidence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.Business.DuplicationModelDetailGroup)(null)).Confidence)));
			this.ModelDetailConfidenceLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.ModelDetailConfidenceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ModelDetailConfidenceLabel.IsFontBold = true;
			this.ModelDetailConfidenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(800, 0, true);
			this.ModelDetailConfidenceLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ModelDetailConfidenceLabel.Name = "ModelDetailConfidenceLabel";
			this.ModelDetailConfidenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.ModelDetailConfidenceLabel.TabIndex = 0;
			this.ModelDetailConfidenceLabel.UseMnemonic = false;
			// 
			// ModelDetailGrid
			// 
			this.ModelDetailGrid.AllowNavigation = false;
			this.ModelDetailGrid.CaptionVisible = false;
			this.ModelDetailLayoutPanel.SetColumnSpan(this.ModelDetailGrid, 2);
			this.ModelDetailGrid.DisableImportDataMenuItem = true;
			this.ModelDetailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModelDetailGrid.GridId = "49b8a4ba-195e-4dde-a466-990f402f7e00";
			this.ModelDetailGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ModelDetailGrid.IsWholeRowSelectedOnClick = true;
			this.ModelDetailGrid.LayoutKey = "ModelDetailGrid";
			this.ModelDetailGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.ModelDetailGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ModelDetailGrid.Name = "ModelDetailGrid";
			this.ModelDetailGrid.RemoveAction = RemoveAction.NoRemovePossible;
			this.ModelDetailGrid.ShowMassUpdateMenuItem = false;
			this.ModelDetailGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 280, true);
			this.ModelDetailGrid.TabIndex = 1;
			// 
			// DuplicationModelDetailUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ModelDetailLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Name = "DuplicationModelDetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ModelDetailLayoutPanel.ResumeLayout(false);
			this.ModelDetailLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ModelDetailGrid)).EndInit();
			this.ModelDetailGrid.ResumeLayout(false);
			this.ModelDetailGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private CargoWise.Windows.UI.KTableLayoutPanel ModelDetailLayoutPanel;
		protected Enterprise.ZArchitecture.ZLabel ModelDetailLabel;
		protected Enterprise.ZArchitecture.ZLabel ModelDetailConfidenceLabel;
		protected CargoWise.Windows.UI.KToolTip ModelDetailScoreToolTip;
		protected Enterprise.ZArchitecture.ZGrid ModelDetailGrid;

		#endregion
	}
}
