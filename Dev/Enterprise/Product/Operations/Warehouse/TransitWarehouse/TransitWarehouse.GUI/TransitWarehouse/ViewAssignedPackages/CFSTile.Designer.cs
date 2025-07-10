namespace Enterprise.Warehouse.Transit.GUI
{
	partial class CFSTile
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
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WarehouseAndStatusPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WarehouseName = new Enterprise.ZArchitecture.ZLabel();
			this.Status = new Enterprise.ZArchitecture.ZLabel();
			this.QtyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberOfPackages = new Enterprise.ZArchitecture.ZLabel();
			this.PackagesCaption = new Enterprise.ZArchitecture.ZLabel();
			this.Volume = new Enterprise.ZArchitecture.ZLabel();
			this.Weight = new Enterprise.ZArchitecture.ZLabel();
			this.VolumeCaption = new Enterprise.ZArchitecture.ZLabel();
			this.WeightCaption = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.WarehouseAndStatusPanel.SuspendLayout();
			this.QtyPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// zPanel1
			// 
			this.zPanel1.AutoSize = true;
			this.zPanel1.BackColor = System.Drawing.Color.Transparent;
			this.zPanel1.Controls.Add(this.WarehouseAndStatusPanel);
			this.zPanel1.Controls.Add(this.QtyPanel);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 100, true);
			this.zPanel1.TabIndex = 0;
			// 
			// WarehouseAndStatusPanel
			// 
			this.WarehouseAndStatusPanel.AutoSize = true;
			this.WarehouseAndStatusPanel.Controls.Add(this.WarehouseName);
			this.WarehouseAndStatusPanel.Controls.Add(this.Status);
			this.WarehouseAndStatusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.WarehouseAndStatusPanel.Name = "WarehouseAndStatusPanel";
			this.WarehouseAndStatusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 41, true);
			this.WarehouseAndStatusPanel.TabIndex = 3;
			// 
			// WarehouseName
			// 
			this.WarehouseName.AutoEllipsis = true;
			this.WarehouseName.AutoSize = true;
			this.WarehouseName.BackColor = System.Drawing.Color.Transparent;
			this.WarehouseName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.WarehouseName.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.WarehouseName, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.WarehouseName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 0, true);
			this.WarehouseName.Name = "WarehouseName";
			this.WarehouseName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 13, true);
			this.WarehouseName.TabIndex = 0;
			this.WarehouseName.Text = "TW Warehouse";
			this.WarehouseName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Status
			// 
			this.Status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Status.BackColor = System.Drawing.Color.Transparent;
			this.Status.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("bcd2c07e-f52f-4c14-acc4-d64f534541af", "-");
			this.Status.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.Status.IsFontBold = true;
			this.Status.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.Status.Name = "Status";
			this.Status.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 21, true);
			this.Status.TabIndex = 1;
			this.Status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// QtyPanel
			// 
			this.QtyPanel.AutoSize = true;
			this.QtyPanel.BackColor = System.Drawing.Color.Transparent;
			this.QtyPanel.Controls.Add(this.NumberOfPackages);
			this.QtyPanel.Controls.Add(this.PackagesCaption);
			this.QtyPanel.Controls.Add(this.Volume);
			this.QtyPanel.Controls.Add(this.Weight);
			this.QtyPanel.Controls.Add(this.VolumeCaption);
			this.QtyPanel.Controls.Add(this.WeightCaption);
			this.QtyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 46, true);
			this.QtyPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.QtyPanel.Name = "QtyPanel";
			this.QtyPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 0, 1, 0, true);
			this.QtyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 54, true);
			this.QtyPanel.TabIndex = 2;
			// 
			// NumberOfPackages
			// 
			this.NumberOfPackages.AutoSize = true;
			this.NumberOfPackages.BackColor = System.Drawing.Color.Transparent;
			this.NumberOfPackages.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("95b7e4c2-2c72-41ac-8d88-ff4fde241618", "-");
			this.NumberOfPackages.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NumberOfPackages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 3, true);
			this.NumberOfPackages.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.NumberOfPackages.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 15, true);
			this.NumberOfPackages.Name = "NumberOfPackages";
			this.NumberOfPackages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 15, true);
			this.NumberOfPackages.TabIndex = 3;
			// 
			// PackagesCaption
			// 
			this.PackagesCaption.BackColor = System.Drawing.Color.Transparent;
			this.PackagesCaption.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("2334ef84-9794-43ea-bb72-82d036ffa7a5", "Packages");
			this.PackagesCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackagesCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 3, true);
			this.PackagesCaption.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.PackagesCaption.Name = "PackagesCaption";
			this.PackagesCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 15, true);
			this.PackagesCaption.TabIndex = 0;
			this.PackagesCaption.Text = "Packages";
			this.PackagesCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// Volume
			// 
			this.Volume.AutoSize = true;
			this.Volume.BackColor = System.Drawing.Color.Transparent;
			this.Volume.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("62e80615-be36-43e4-a45a-a91c6c6b46db", "-");
			this.Volume.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Volume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 36, true);
			this.Volume.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Volume.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 15, true);
			this.Volume.Name = "Volume";
			this.Volume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 15, true);
			this.Volume.TabIndex = 5;
			// 
			// Weight
			// 
			this.Weight.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Weight.AutoSize = true;
			this.Weight.BackColor = System.Drawing.Color.Transparent;
			this.Weight.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("33a0a670-71fc-4d22-aec1-2e745ab31a54", "-");
			this.Weight.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Weight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 20, true);
			this.Weight.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 0, true);
			this.Weight.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 15, true);
			this.Weight.Name = "Weight";
			this.Weight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 15, true);
			this.Weight.TabIndex = 4;
			// 
			// VolumeCaption
			// 
			this.VolumeCaption.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.VolumeCaption.BackColor = System.Drawing.Color.Transparent;
			this.VolumeCaption.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("57ff44c4-7a1f-4773-9c0a-74b23f55195c", "Volume");
			this.VolumeCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VolumeCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 36, true);
			this.VolumeCaption.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.VolumeCaption.Name = "VolumeCaption";
			this.VolumeCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 15, true);
			this.VolumeCaption.TabIndex = 2;
			this.VolumeCaption.Text = "Volume";
			this.VolumeCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WeightCaption
			// 
			this.WeightCaption.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.WeightCaption.BackColor = System.Drawing.Color.Transparent;
			this.WeightCaption.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("0f9be499-c371-4665-9a8b-b5363b17d85a", "Weight");
			this.WeightCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WeightCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 20, true);
			this.WeightCaption.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.WeightCaption.Name = "WeightCaption";
			this.WeightCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 15, true);
			this.WeightCaption.TabIndex = 1;
			this.WeightCaption.Text = "Weight";
			this.WeightCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CFSTile
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.zPanel1);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 95, true);
			this.Name = "CFSTile";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.WarehouseAndStatusPanel.ResumeLayout(false);
			this.WarehouseAndStatusPanel.PerformLayout();
			this.QtyPanel.ResumeLayout(false);
			this.QtyPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.ZLabel WarehouseName;
		private ZArchitecture.ZLabel Status;
		private ZArchitecture.GUI.ZPanel QtyPanel;
		private ZArchitecture.ZLabel VolumeCaption;
		private ZArchitecture.ZLabel WeightCaption;
		private ZArchitecture.ZLabel Volume;
		private ZArchitecture.ZLabel Weight;
		private ZArchitecture.GUI.ZPanel WarehouseAndStatusPanel;
		private ZArchitecture.ZLabel NumberOfPackages;
		private ZArchitecture.ZLabel PackagesCaption;
	}
}
