using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI.RateSelector
{
	partial class RateSelectorForm
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
			this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.txtJobMode = new Enterprise.ZArchitecture.ZLabel();
			this.zToolStrip1 = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.btnCancel = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.btnSkip = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.btnApply = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tableLayoutPanel.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zToolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 481, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel.ColumnCount = 1;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.zPanel2, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.zToolStrip1, 0, 3);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 4;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 505, true);
			this.tableLayoutPanel.TabIndex = 4;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.txtJobMode);
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 30, true);
			this.zPanel2.TabIndex = 6;
			// 
			// txtJobMode
			// 
			this.txtJobMode.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.txtJobMode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 6, true);
			this.txtJobMode.Name = "txtJobMode";
			this.txtJobMode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 23, true);
			this.txtJobMode.TabIndex = 0;
			this.txtJobMode.Text = "Transport: AIR ContainerMode: LSE";
			// 
			// zToolStrip1
			// 
			this.zToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.btnApply,
				this.btnSkip,
			this.btnCancel,
			});
			this.zToolStrip1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 455, true);
			this.zToolStrip1.Name = "zToolStrip1";
			this.zToolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.zToolStrip1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 25, true);
			this.zToolStrip1.TabIndex = 7;
			this.zToolStrip1.Text = "zToolStrip1";
			// 
			// btnCancel
			// 
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 22, true);
			this.btnCancel.Image = Icons.GetImage(IconTypes.BlackWhite_Cancel);
			this.btnCancel.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			this.btnCancel.Text = Res.GetString("3fdfd1fb-8afa-46dd-b021-a62570c9c61a", "Cancel");
			this.btnCancel.Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0);
			this.btnCancel.AutoToolTip = false;
			this.btnCancel.Alignment = ToolStripItemAlignment.Right;
			this.btnCancel.Click += BtnCancelClick;
			// 
			// btnSkip
			// 
			this.btnSkip.Name = "btnSkip";
			this.btnSkip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 22, true);
			this.btnSkip.Image = Icons.GetImage(IconTypes.BlackWhite_Save);
			this.btnSkip.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			this.btnSkip.Text = Res.GetString("e10cbd6f-a1cc-48a2-8f70-917be8b51d18", "Skip Rate Selection");
			this.btnSkip.Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0);
			this.btnSkip.AutoToolTip = false;
			this.btnSkip.Alignment = ToolStripItemAlignment.Right;
			this.btnSkip.Click += BtnSkipRateSelectionClick;
			// 
			// btnApply
			// 
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 22, true);
			this.btnApply.Image = Icons.GetImage(IconTypes.BlackWhite_Save);
			this.btnApply.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			this.btnApply.Text = Res.GetString("C62F2E64-096D-48EB-938F-9E1F8626BDDD", "Apply");
			this.btnApply.Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0);
			this.btnApply.AutoToolTip = false;
			this.btnApply.Alignment = ToolStripItemAlignment.Right;
			this.btnApply.Click += BtnApplyClick;
			this.btnApply.Enabled = false;
			// 
			// RateSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 505, true);
			this.Controls.Add(this.tableLayoutPanel);
			this.Name = "RateSelectorForm";
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("f9357384-8f19-46ee-9409-dca4d9f2d5eb", "Rate Selection");
			this.Controls.SetChildIndex(this.tableLayoutPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zToolStrip1.ResumeLayout(false);
			this.zToolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanel;
		private ZArchitecture.GUI.ZPanel zPanel2;
		private ZArchitecture.ZLabel txtJobMode;
		private ZArchitecture.GUI.ZToolStrip zToolStrip1;
		private ZArchitecture.GUI.ZToolStripButton btnCancel;
		private ZArchitecture.GUI.ZToolStripButton btnSkip;
		private ZArchitecture.GUI.ZToolStripButton btnApply;
	}
}
