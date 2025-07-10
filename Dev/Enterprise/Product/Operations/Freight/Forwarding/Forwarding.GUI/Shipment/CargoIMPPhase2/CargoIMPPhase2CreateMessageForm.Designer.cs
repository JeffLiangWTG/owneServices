namespace Enterprise.Freight.Forwarding.GUI
{
	partial class CargoIMPPhase2CreateMessageForm
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
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CargoIMPPhase2CreateMessageForm));
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateMessageOutputPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OutputTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CreateMessageOutputPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 74, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 52, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 22, true);
			// 
			// EnterpriseLogo
			// 
			this.EnterpriseLogo.Image = ((System.Drawing.Image)(resources.GetObject("EnterpriseLogo.Image")));
			// 
			// CancelProgressButton
			// 
			this.CancelProgressButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelProgressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 0, true);
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 15, true);
			// 
			// ProgressLabel
			// 
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 342, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 7, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(329);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(329);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DataTransfer.Business.FlatFileDataExporter);
			// 
			// CreateButton
			// 
			this.CreateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2CreateMessageForm|5ed6b4f3-723d-4972-9f53-911fc6707e7d", "Create");
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 215, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CreateButton.TabIndex = 0;
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// CreateMessageOutputPanel
			// 
			this.CreateMessageOutputPanel.Controls.Add(this.OutputTextbox);
			this.CreateMessageOutputPanel.Controls.Add(this.CloseButton);
			this.CreateMessageOutputPanel.Controls.Add(this.CreateButton);
			this.CreateMessageOutputPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 74, true);
			this.CreateMessageOutputPanel.Name = "CreateMessageOutputPanel";
			this.CreateMessageOutputPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 245, true);
			this.CreateMessageOutputPanel.TabIndex = 6;
			// 
			// OutputTextbox
			// 
			this.OutputTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OutputTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutputTextbox, false);
			this.OutputTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.OutputTextbox.Multiline = true;
			this.OutputTextbox.Name = "OutputTextbox";
			this.OutputTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 201, true);
			this.OutputTextbox.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2CreateMessageForm|725877df-1373-4f48-b611-9c823cb402c2", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 215, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.CloseButton.TabIndex = 8;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// CargoIMPPhase2CreateMessageForm
			// 
			
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CargoIMPPhase2CreateMessageForm|110008e8-d510-409e-9955-76c494afe2d1", "Create CargoIMP Phase 2 Message");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 349, true);
			this.Controls.Add(this.CreateMessageOutputPanel);
			this.DataSourceAssemblyName = "Enterprise.DataTransfer";
			this.DataSourceType = typeof(Enterprise.DataTransfer.Business.FlatFileDataExporter);
			this.DataSourceTypeName = "Enterprise.DataTransfer.Business.FlatFileDataExporter";
			this.Name = "CargoIMPPhase2CreateMessageForm";
			this.Status = "This process can take some time...";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CreateMessageOutputPanel, 0);
			this.MainPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CreateMessageOutputPanel.ResumeLayout(false);
			this.CreateMessageOutputPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		private Enterprise.ZArchitecture.GUI.ZPanel CreateMessageOutputPanel;
		protected Enterprise.ZArchitecture.ZTextBox OutputTextbox;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton CreateButton;

		#endregion
	}
}
