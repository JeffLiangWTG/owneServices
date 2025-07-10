
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector
{
	partial class BookingRateSelectorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookingRateSelectorForm));
            this.btnCancel = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
            this.btnBook = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
            this.zToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
            this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zToolStrip.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 481, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
            // 
            // btnCancel
            // 
            this.btnCancel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnCancel.AutoToolTip = false;
            this.btnCancel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ac3318de-2b7c-4f40-97b3-f7d4b0706cd2", "Cancel");
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 22, true);
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // btnBook
            // 
            this.btnBook.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnBook.AutoToolTip = false;
            this.btnBook.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("87f0f452-513e-4370-8b8e-4531a844c115", "Book");
            this.btnBook.Enabled = false;
            this.btnBook.Image = ((System.Drawing.Image)(resources.GetObject("btnBook.Image")));
            this.btnBook.Name = "btnBook";
            this.btnBook.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 22, true);
            this.btnBook.Click += new System.EventHandler(this.BtnBookClick);
            // 
            // zToolStrip
            // 
            this.zToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.zToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.zToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnBook,
            this.btnCancel});
            this.zToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 456, true);
            this.zToolStrip.Name = "zToolStrip";
            this.zToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.zToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 25, true);
            this.zToolStrip.TabIndex = 0;
            this.zToolStrip.Text = "zToolStrip1";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.zToolStrip, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 481, true);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // BookingRateSelectorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("f9357384-8f19-46ee-9409-dca4d9f2d5eb", "Rate Selection");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 505, true);
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "BookingRateSelectorForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zToolStrip.ResumeLayout(false);
            this.zToolStrip.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZToolStrip zToolStrip;
		private ZArchitecture.GUI.ZToolStripButton btnCancel;
		private ZArchitecture.GUI.ZToolStripButton btnBook;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel;
	}
}
