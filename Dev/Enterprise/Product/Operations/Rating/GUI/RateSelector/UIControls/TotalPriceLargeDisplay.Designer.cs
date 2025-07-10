using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class TotalPriceLargeDisplay
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
            this.centeringTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.kFlowLayoutPanel1 = new CargoWise.Windows.UI.KFlowLayoutPanel();
            this.totalPriceStringLabel = new Enterprise.ZArchitecture.ZLabel();
            this.warningPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.centeringTableLayoutPanel.SuspendLayout();
            this.kFlowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.warningPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel);
            // 
            // centeringTableLayoutPanel
            // 
            this.centeringTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.centeringTableLayoutPanel.ColumnCount = 1;
            this.centeringTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.centeringTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.centeringTableLayoutPanel.Controls.Add(this.kFlowLayoutPanel1, 0, 0);
            this.centeringTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.centeringTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.centeringTableLayoutPanel.Name = "centeringTableLayoutPanel";
            this.centeringTableLayoutPanel.RowCount = 1;
            this.centeringTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.centeringTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(10)));
            this.centeringTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 25, true);
            this.centeringTableLayoutPanel.TabIndex = 0;
            // 
            // kFlowLayoutPanel1
            // 
            this.kFlowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.kFlowLayoutPanel1.AutoSize = true;
            this.kFlowLayoutPanel1.Controls.Add(this.totalPriceStringLabel);
            this.kFlowLayoutPanel1.Controls.Add(this.warningPictureBox);
            this.kFlowLayoutPanel1.ForeColor = System.Drawing.Color.White;
            this.kFlowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
            this.kFlowLayoutPanel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.kFlowLayoutPanel1.Name = "kFlowLayoutPanel1";
            this.kFlowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 25, true);
            this.kFlowLayoutPanel1.TabIndex = 0;
            // 
            // totalPriceStringLabel
            // 
            this.totalPriceStringLabel.AutoSize = true;
            this.BindingSource.SetBindingMember(this.totalPriceStringLabel, "TotalPriceString");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.BookingEngineRateViewModel)(null)).TotalPriceString)));
            this.totalPriceStringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.totalPriceStringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
            this.totalPriceStringLabel.Name = "totalPriceStringLabel";
            this.totalPriceStringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 19, true);
            this.totalPriceStringLabel.TabIndex = 0;
            this.totalPriceStringLabel.Text = "TotalPriceString";
            // 
            // warningPictureBox
            // 
            this.warningPictureBox.Image = global::Enterprise.Rating.GUI.Properties.Resources.WarningDrawing;
            this.warningPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 0, true);
            this.warningPictureBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
            this.warningPictureBox.Name = "warningPictureBox";
            this.warningPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.warningPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.warningPictureBox.TabIndex = 1;
            this.warningPictureBox.TabStop = false;
            // 
            // TotalPriceLargeDisplay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.centeringTableLayoutPanel);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.Name = "TotalPriceLargeDisplay";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 25, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.centeringTableLayoutPanel.ResumeLayout(false);
            this.centeringTableLayoutPanel.PerformLayout();
            this.kFlowLayoutPanel1.ResumeLayout(false);
            this.kFlowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.warningPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel centeringTableLayoutPanel;
		private CargoWise.Windows.UI.KFlowLayoutPanel kFlowLayoutPanel1;
		private ZArchitecture.ZLabel totalPriceStringLabel;
		private ZArchitecture.GUI.ZPictureBox warningPictureBox;
	}
}
