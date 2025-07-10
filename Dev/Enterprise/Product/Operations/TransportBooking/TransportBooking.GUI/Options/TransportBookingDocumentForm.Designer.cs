namespace Enterprise.TransportBookings.GUI.Options
{
	partial class TransportBookingDocumentForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OpenStandardDesignerButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OpenInstructionDesignerButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DesignerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AutoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TemplatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDropEdit1.SuspendLayout();
			this.TemplatePanel.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.MessagePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("XXXec017bdb-bb9c-4040-aa53-10224afd0783", "Designer");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 59, true);
			this.zGroupBox1.TabIndex = 3;
			this.zGroupBox1.TabStop = false;
			// 
			// OpenStandardDesignerButton
			// 
			this.OpenStandardDesignerButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("a6d8aaaf-9f1b-45c8-9d16-36f04d959374", "Standard View");
			this.OpenStandardDesignerButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 22, true);
			this.OpenStandardDesignerButton.Name = "OpenStandardDesignerButton";
			this.OpenStandardDesignerButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 36, true);
			this.OpenStandardDesignerButton.TabIndex = 4;
			this.OpenStandardDesignerButton.ToolTipCaption = null;
			this.OpenStandardDesignerButton.UseVisualStyleBackColor = true;
			this.OpenStandardDesignerButton.Click += new System.EventHandler(this.OpenStandardDesignerButton_Click);
			// 
			// OpenInstructionDesignerButton
			// 
			this.OpenInstructionDesignerButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("6b7dba77-1a41-41ad-8017-38d8b8cb7dfb", "Instruction View");
			this.OpenInstructionDesignerButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 22, true);
			this.OpenInstructionDesignerButton.Name = "OpenInstructionDesignerButton";
			this.OpenInstructionDesignerButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 36, true);
			this.OpenInstructionDesignerButton.TabIndex = 6;
			this.OpenInstructionDesignerButton.ToolTipCaption = null;
			this.OpenInstructionDesignerButton.UseVisualStyleBackColor = true;
			this.OpenInstructionDesignerButton.Click += new System.EventHandler(this.OpenInstructionDesignerButton_Click);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "Template");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions)(null)).Template)));
			this.zDropEdit1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("XX5a5f1ea4-d956-42bf-9834-60f3b3abbee9", "Booking Template");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 12, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.zDropEdit1.TabIndex = 0;
			// 
			// DeliverButton
			// 
			this.DeliverButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("5b0cb015-290b-4f37-ba2e-674866a7033d", "Deliver");
			this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 22, true);
			this.DeliverButton.Name = "DeliverButton";
			this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 36, true);
			this.DeliverButton.TabIndex = 2;
			this.DeliverButton.ToolTipCaption = null;
			this.DeliverButton.UseVisualStyleBackColor = true;
			this.DeliverButton.Click += new System.EventHandler(this.DeliverButton_Click);
			// 
			// CancelBtn
			// 
			this.CancelBtn.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("XXX5b6f856f-35e2-4db4-9011-ce53c2789a8a", "Cancel");
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 22, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 36, true);
			this.CancelBtn.TabIndex = 8;
			this.CancelBtn.ToolTipCaption = null;
			this.CancelBtn.UseVisualStyleBackColor = true;
			this.CancelBtn.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// DesignerLabel
			// 
			this.DesignerLabel.AutoSize = true;
			this.DesignerLabel.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("bafc7162-f4d0-4c81-9b57-142f784b7ea0", "Open Transport Booking");
			this.DesignerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DesignerLabel.IsFontBold = true;
			this.DesignerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 5, true);
			this.DesignerLabel.Name = "DesignerLabel";
			this.DesignerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 13, true);
			this.DesignerLabel.TabIndex = 5;
			// 
			// AutoLabel
			// 
			this.AutoLabel.AutoSize = true;
			this.AutoLabel.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("f9bed35f-f3f1-46e2-94ba-5fe5e94b56f0", "Document");
			this.AutoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.AutoLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AutoLabel, false);
			this.AutoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 5, true);
			this.AutoLabel.Name = "AutoLabel";
			this.AutoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.AutoLabel.TabIndex = 1;
			// 
			// zGroupBox2
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox2, false);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 3, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 59, true);
			this.zGroupBox2.TabIndex = 7;
			this.zGroupBox2.TabStop = false;
			// 
			// TemplatePanel
			// 
			this.TemplatePanel.Controls.Add(this.zDropEdit1);
			this.TemplatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TemplatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.TemplatePanel.Name = "TemplatePanel";
			this.TemplatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 35, true);
			this.TemplatePanel.TabIndex = 9;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.AutoLabel);
			this.ButtonsPanel.Controls.Add(this.CancelBtn);
			this.ButtonsPanel.Controls.Add(this.zGroupBox2);
			this.ButtonsPanel.Controls.Add(this.zGroupBox1);
			this.ButtonsPanel.Controls.Add(this.OpenStandardDesignerButton);
			this.ButtonsPanel.Controls.Add(this.OpenInstructionDesignerButton);
			this.ButtonsPanel.Controls.Add(this.DesignerLabel);
			this.ButtonsPanel.Controls.Add(this.DeliverButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 64, true);
			this.ButtonsPanel.TabIndex = 10;
			// 
			// MessagePanel
			// 
			this.MessagePanel.Controls.Add(this.zLabel1);
			this.MessagePanel.Controls.Add(this.MessageLabel);
			this.MessagePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagePanel.Name = "MessagePanel";
			this.MessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 76, true);
			this.MessagePanel.TabIndex = 11;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("b99cf3a5-1783-4aa9-8b23-7fd086f352a1", "Booking changes must be made directly on the booking.");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 58, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 16, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// MessageLabel
			// 
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 43, true);
			this.MessageLabel.TabIndex = 0;
			this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// TransportBookingDocumentForm
			// 
			this.AcceptButton = this.DeliverButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelBtn;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("00294e28-8189-4d50-aa40-1a12c6ba6f57", "Transport Booking Options");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 201, true);
			this.Controls.Add(this.ButtonsPanel);
			this.Controls.Add(this.TemplatePanel);
			this.Controls.Add(this.MessagePanel);
			this.DataSourceType = typeof(Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "TransportBookingDocumentForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MessagePanel, 0);
			this.Controls.SetChildIndex(this.TemplatePanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.TemplatePanel.ResumeLayout(false);
			this.TemplatePanel.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.MessagePanel.ResumeLayout(false);
			this.MessagePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZButton OpenStandardDesignerButton;
		private ZArchitecture.GUI.ZButton OpenInstructionDesignerButton;
		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.GUI.ZButton DeliverButton;
		private ZArchitecture.GUI.ZButton CancelBtn;
		private ZArchitecture.ZLabel DesignerLabel;
		private ZArchitecture.ZLabel AutoLabel;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.GUI.ZPanel TemplatePanel;
		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private ZArchitecture.GUI.ZPanel MessagePanel;
		private ZArchitecture.ZLabel MessageLabel;
		private ZArchitecture.ZLabel zLabel1;
	}
}
