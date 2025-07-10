namespace Enterprise.MasterFiles.GUI
{
	partial class AllowedOrgsAndWarehousesControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbSecurityAllowedOrgsAndWarehousesView);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.AddButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 512, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 40, true);
			this.BottomPanel.TabIndex = 0;
			// 
			// AddButton
			// 
			this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AllowedOrgsAndWarehousesControl|AddItemButtonName", "Add");
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 8, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.AddButton.TabIndex = 0;
			this.AddButton.UseVisualStyleBackColor = true;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// AllowedOrgsAndWarehousesSecurityPanelBoundZGrid
			// 
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbSecurity)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(null)).ItemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(null)).ItemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(null)).ItemType)));
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AllowedOrgsAndWarehousesControl|6f4a36ed-4e52-4cac-b1d0-39081cea499f", "Item Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ItemCode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AllowedOrgsAndWarehousesControl|5ef0793e-57f8-43aa-acef-cb3c614c562a", "Item Name");
			zTextBoxColumnStyleInfo2.ColumnName = "ItemName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AllowedOrgsAndWarehousesControl|a296c26a-3a00-433b-9887-cb1226445f14", "Item Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ItemType";
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.GridId = "fd053f8f-5744-4b7b-a39e-d119cfad367d";
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.LayoutKey = "AllowedOrgsAndWarehousesSecurityPanelBoundZGrid";
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.Name = "AllowedOrgsAndWarehousesSecurityPanelBoundZGrid";
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 448, true);
			this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.TabIndex = 1;
			AllowedOrgsAndWarehousesSecurityPanelBoundZGrid.AllowReadOnlyRowsToBeDeleted = true;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid);
			this.MainPanel.Controls.Add(this.DescriptionLabel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 512, true);
			this.MainPanel.TabIndex = 2;
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 48, true);
			this.DescriptionLabel.TabIndex = 0;
			// 
			// AllowedOrgsAndWarehousesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.Name = "AllowedOrgsAndWarehousesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 552, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AllowedOrgsAndWarehousesSecurityPanelBoundZGrid)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.GUI.ZButton AddButton;
		private Enterprise.ZArchitecture.ZGrid AllowedOrgsAndWarehousesSecurityPanelBoundZGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		public Enterprise.ZArchitecture.ZLabel DescriptionLabel;
	}
}
