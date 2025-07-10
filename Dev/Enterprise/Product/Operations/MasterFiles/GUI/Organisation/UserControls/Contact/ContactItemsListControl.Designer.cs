namespace Enterprise.MasterFiles.GUI
{
	partial class ContactItemsListControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.mainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.AddPhoneItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PhoneItemsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PhoneItemsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AddEmailItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EmailItemsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EmailItemsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryStatusButton = new Enterprise.ZArchitecture.GUI.ZImageButton();
			this.mainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainTableLayoutPanel.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.mainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgContact);
			// 
			// mainTableLayoutPanel
			// 
			this.mainTableLayoutPanel.AutoSize = true;
			this.mainTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.mainTableLayoutPanel.ColumnCount = 1;
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainTableLayoutPanel.Controls.Add(this.AddPhoneItemButton, 0, 5);
			this.mainTableLayoutPanel.Controls.Add(this.PhoneItemsPanel, 0, 4);
			this.mainTableLayoutPanel.Controls.Add(this.PhoneItemsLabel, 0, 3);
			this.mainTableLayoutPanel.Controls.Add(this.AddEmailItemButton, 0, 2);
			this.mainTableLayoutPanel.Controls.Add(this.topPanel, 0, 0);
			this.mainTableLayoutPanel.Controls.Add(this.EmailItemsPanel, 0, 0);
			this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.mainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
			this.mainTableLayoutPanel.RowCount = 6;
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.mainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 84, true);
			this.mainTableLayoutPanel.TabIndex = 0;
			// 
			// AddPhoneItemButton
			// 
			this.AddPhoneItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddPhoneItemButton.BackColor = System.Drawing.Color.Transparent;
			this.AddPhoneItemButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.AddPhoneItemButton.FlatAppearance.BorderSize = 0;
			this.AddPhoneItemButton.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.AddPhoneItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 66, true);
			this.AddPhoneItemButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 11, 0, true);
			this.AddPhoneItemButton.Name = "AddPhoneItemButton";
			this.AddPhoneItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.AddPhoneItemButton.TabIndex = 5;
			this.AddPhoneItemButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AddPhoneItemButton.UseVisualStyleBackColor = false;
			this.AddPhoneItemButton.Click += new System.EventHandler(this.AddPhoneItemButton_Click);
			// 
			// PhoneItemsPanel
			// 
			this.PhoneItemsPanel.AutoSize = true;
			this.PhoneItemsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.PhoneItemsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PhoneItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
			this.PhoneItemsPanel.Name = "PhoneItemsPanel";
			this.PhoneItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 1, true);
			this.PhoneItemsPanel.TabIndex = 4;
			// 
			// PhoneItemsLabel
			// 
			this.PhoneItemsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6e799a3e-9b18-4a43-b00f-0a8e973959e3", "Phone");
			this.PhoneItemsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PhoneItemsLabel.IsFontBold = true;
			this.PhoneItemsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 45, true);
			this.PhoneItemsLabel.Name = "PhoneItemsLabel";
			this.PhoneItemsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 15, true);
			this.PhoneItemsLabel.TabIndex = 3;
			// 
			// AddEmailItemButton
			// 
			this.AddEmailItemButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddEmailItemButton.BackColor = System.Drawing.Color.Transparent;
			this.AddEmailItemButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.AddEmailItemButton.FlatAppearance.BorderSize = 0;
			this.AddEmailItemButton.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddEmailItemButton, false);
			this.AddEmailItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 27, true);
			this.AddEmailItemButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 11, 0, true);
			this.AddEmailItemButton.Name = "AddEmailItemButton";
			this.AddEmailItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.AddEmailItemButton.TabIndex = 2;
			this.AddEmailItemButton.UseVisualStyleBackColor = false;
			this.AddEmailItemButton.Click += new System.EventHandler(this.AddEmailItemButton_Click);
			// 
			// EmailItemsPanel
			// 
			this.EmailItemsPanel.AutoSize = true;
			this.EmailItemsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.EmailItemsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EmailItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.EmailItemsPanel.Name = "EmailItemsPanel";
			this.EmailItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 1, true);
			this.EmailItemsPanel.TabIndex = 1;
			this.EmailItemsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			// 
			// topPanel
			// 
			this.topPanel.AutoSize = true;
			this.topPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.topPanel.Controls.Add(this.EmailItemsLabel);
			this.topPanel.Controls.Add(this.DeliveryStatusLabel);
			this.topPanel.Controls.Add(this.DeliveryStatusButton);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 15, true);
			this.topPanel.TabIndex = 0;
			this.topPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			// 
			// EmailItemsLabel
			// 
			this.EmailItemsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.EmailItemsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e1823e5b-8d32-4177-9768-f2f5d3b16c8d", "Email");
			this.EmailItemsLabel.IsFontBold = true;
			this.EmailItemsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.EmailItemsLabel.Name = "EmailItemsLabel";
			this.EmailItemsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 15, true);
			this.EmailItemsLabel.TabIndex = 0;
			// 
			// DeliveryStatusLabel
			// 
			this.DeliveryStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryStatusLabel, false);
			this.DeliveryStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 5, true);
			this.DeliveryStatusLabel.Name = "NDRDateItemsLabel";
			this.DeliveryStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.DeliveryStatusLabel.TabIndex = 1;
			this.DeliveryStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DeliveryStatusButton
			// 
			this.DeliveryStatusButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right))));
			this.DeliveryStatusButton.BackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.Unverified;
			this.DeliveryStatusButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.DeliveryStatusButton.DisplayFocusCues = false;
			this.DeliveryStatusButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 5, true);
			this.DeliveryStatusButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DeliveryStatusButton.Name = "DeliveryStatusButton";
			this.DeliveryStatusButton.NormalBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.Unverified;
			this.DeliveryStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 15, true);
			this.DeliveryStatusButton.TabIndex = 2;
			this.DeliveryStatusButton.TabStop = false;
			this.DeliveryStatusButton.UseVisualStyleBackColor = true;
			this.DeliveryStatusButton.Click += new System.EventHandler(this.DeliveryStatusButton_Click);
			this.DeliveryStatusButton.BringToFront();
			// 
			// mainPanel
			// 
			this.mainPanel.Controls.Add(this.mainTableLayoutPanel);
			this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 390, true);
			this.mainPanel.TabIndex = 4;
			this.mainPanel.AutoSize = true;
			this.mainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
			// 
			// ContactItemsListControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainPanel);
			this.Name = "ContactItemsListControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 390, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainTableLayoutPanel.ResumeLayout(false);
			this.mainTableLayoutPanel.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel mainTableLayoutPanel;
		private ZArchitecture.ZLabel EmailItemsLabel;
		protected ZArchitecture.ZLabel DeliveryStatusLabel;
		private ZArchitecture.ZLabel PhoneItemsLabel;
		protected Enterprise.ZArchitecture.GUI.ZPanel EmailItemsPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel PhoneItemsPanel;
		protected ZArchitecture.GUI.ZButton AddEmailItemButton;
		protected ZArchitecture.GUI.ZButton AddPhoneItemButton;
		private ZArchitecture.GUI.ZPanel topPanel;
		private ZArchitecture.GUI.ZPanel mainPanel;
		protected Enterprise.ZArchitecture.GUI.ZImageButton DeliveryStatusButton;
	}
}
