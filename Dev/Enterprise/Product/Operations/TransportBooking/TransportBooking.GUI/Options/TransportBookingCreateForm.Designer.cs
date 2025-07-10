namespace Enterprise.TransportBookings.GUI.Options
{
	partial class TransportBookingCreateForm
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
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TemplatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CreateButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.CreateCancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CreateOpenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateOpenInstructionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDropEdit1.SuspendLayout();
			this.TemplatePanel.SuspendLayout();
			this.MessagePanel.SuspendLayout();
			this.CreateButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 167, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions);
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
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.zDropEdit1.TabIndex = 0;
			// 
			// TemplatePanel
			// 
			this.TemplatePanel.Controls.Add(this.zDropEdit1);
			this.TemplatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TemplatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			this.TemplatePanel.Name = "TemplatePanel";
			this.TemplatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 35, true);
			this.TemplatePanel.TabIndex = 9;
			// 
			// MessagePanel
			// 
			this.MessagePanel.Controls.Add(this.MessageLabel);
			this.MessagePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagePanel.Name = "MessagePanel";
			this.MessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 72, true);
			this.MessagePanel.TabIndex = 11;
			// 
			// MessageLabel
			// 
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 57, true);
			this.MessageLabel.TabIndex = 0;
			this.MessageLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// CreateButtonsPanel
			// 
			this.CreateButtonsPanel.Controls.Add(this.zLabel1);
			this.CreateButtonsPanel.Controls.Add(this.CreateCancelBtn);
			this.CreateButtonsPanel.Controls.Add(this.zGroupBox3);
			this.CreateButtonsPanel.Controls.Add(this.CreateOpenButton);
			this.CreateButtonsPanel.Controls.Add(this.CreateOpenInstructionButton);
			this.CreateButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CreateButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
			this.CreateButtonsPanel.Name = "CreateButtonsPanel";
			this.CreateButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 58, true);
			this.CreateButtonsPanel.TabIndex = 12;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("2b4fe49d-1903-4fad-b106-f92debfdfd7e", "Create Transport Booking");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 13, true);
			this.zLabel1.TabIndex = 9;
			// 
			// CreateCancelBtn
			// 
			this.CreateCancelBtn.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("d7d0eeee-6791-4b1f-bbbe-52d5929a5a4b", "Cancel");
			this.CreateCancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CreateCancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 20, true);
			this.CreateCancelBtn.Name = "CreateCancelBtn";
			this.CreateCancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 36, true);
			this.CreateCancelBtn.TabIndex = 8;
			this.CreateCancelBtn.ToolTipCaption = null;
			this.CreateCancelBtn.UseVisualStyleBackColor = true;
			this.CreateCancelBtn.Click += new System.EventHandler(this.CreateCancelBtn_Click);
			// 
			// zGroupBox3
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox3, false);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 0, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 57, true);
			this.zGroupBox3.TabIndex = 7;
			this.zGroupBox3.TabStop = false;
			// 
			// CreateOpenButton
			// 
			this.CreateOpenButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("7150b07d-fb87-4ecb-b145-87281901fb3e", "Standard View");
			this.CreateOpenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 20, true);
			this.CreateOpenButton.Name = "CreateOpenButton";
			this.CreateOpenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 36, true);
			this.CreateOpenButton.TabIndex = 4;
			this.CreateOpenButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
			this.CreateOpenButton.ToolTipCaption = null;
			this.CreateOpenButton.UseVisualStyleBackColor = true;
			this.CreateOpenButton.Click += new System.EventHandler(this.CreateOpenButton_Click);
			// 
			// CreateOpenInstructionButton
			// 
			this.CreateOpenInstructionButton.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("693041d0-3511-40c3-a79a-f967d976481a", "Instruction View");
			this.CreateOpenInstructionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 20, true);
			this.CreateOpenInstructionButton.Name = "CreateOpenInstructionButton";
			this.CreateOpenInstructionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 36, true);
			this.CreateOpenInstructionButton.TabIndex = 6;
			this.CreateOpenInstructionButton.ToolTipCaption = null;
			this.CreateOpenInstructionButton.UseVisualStyleBackColor = true;
			this.CreateOpenInstructionButton.Click += new System.EventHandler(this.CreateOpenInstructionButton_Click);
			// 
			// TransportBookingCreateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("00294e28-8189-4d50-aa40-1a12c6ba6f57", "Transport Booking Options");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 191, true);
			this.Controls.Add(this.CreateButtonsPanel);
			this.Controls.Add(this.TemplatePanel);
			this.Controls.Add(this.MessagePanel);
			this.DataSourceType = typeof(Enterprise.TransportBookings.Business.Options.TransportBookingDocumentOptions);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "TransportBookingCreateForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MessagePanel, 0);
			this.Controls.SetChildIndex(this.TemplatePanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CreateButtonsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.TemplatePanel.ResumeLayout(false);
			this.TemplatePanel.PerformLayout();
			this.MessagePanel.ResumeLayout(false);
			this.MessagePanel.PerformLayout();
			this.CreateButtonsPanel.ResumeLayout(false);
			this.CreateButtonsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.GUI.ZPanel TemplatePanel;
		private ZArchitecture.GUI.ZPanel MessagePanel;
		private ZArchitecture.ZLabel MessageLabel;
		private ZArchitecture.GUI.ZPanel CreateButtonsPanel;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZButton CreateCancelBtn;
		private ZArchitecture.GUI.ZGroupBox zGroupBox3;
		private ZArchitecture.GUI.ZButton CreateOpenButton;
		private ZArchitecture.GUI.ZButton CreateOpenInstructionButton;
	}
}
