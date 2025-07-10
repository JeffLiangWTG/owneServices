namespace Enterprise.Freight.Agency.GUI
{
	partial class CreateBulkContainerDetentionForm
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
			this.unselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.selectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.createNew = new Enterprise.ZArchitecture.GUI.ZButton();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			createBulkDetentionInvoicesControl = new Enterprise.Freight.Agency.GUI.CreateBulkContainerDetentionControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 439, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BulkDetentionHeader);
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(this.unselectAllButton);
			bottomPanel.Controls.Add(this.selectAllButton);
			bottomPanel.Controls.Add(this.closeButton);
			bottomPanel.Controls.Add(this.createNew);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 411, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 28, true);
			bottomPanel.TabIndex = 1;
			// 
			// unselectAllButton
			// 
			this.unselectAllButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CreateBulkContainerDetentionForm|1b72591b-a507-4bc7-87b5-69166fdfcb57", "Deselect All");
			this.unselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 2, true);
			this.unselectAllButton.Name = "unselectAllButton";
			this.unselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.unselectAllButton.TabIndex = 1;
			this.unselectAllButton.UseVisualStyleBackColor = true;
			this.unselectAllButton.Click += new System.EventHandler(this.unselectAllButton_Click);
			// 
			// selectAllButton
			// 
			this.selectAllButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CreateBulkContainerDetentionForm|51424259-48c3-40f9-8533-5163e3e21372", "Select All");
			this.selectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 2, true);
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.selectAllButton.TabIndex = 0;
			this.selectAllButton.UseVisualStyleBackColor = true;
			this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CreateBulkContainerDetentionForm|66d50127-c561-47c0-ae49-029795d6d7f4", "Close");
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 2, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 3;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// createNew
			// 
			this.createNew.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CreateBulkContainerDetentionForm|29911452-5e03-4680-8e4f-c9205196b1f1", "Create");
			this.createNew.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 2, true);
			this.createNew.Name = "createNew";
			this.createNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.createNew.TabIndex = 2;
			this.createNew.UseVisualStyleBackColor = true;
			this.createNew.Click += new System.EventHandler(this.createNew_Click);
			// 
			// createBulkDetentionInvoicesControl
			// 
			this.BindingSource.SetBindingMember(createBulkDetentionInvoicesControl, ".");
			createBulkDetentionInvoicesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			createBulkDetentionInvoicesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			createBulkDetentionInvoicesControl.Name = "createBulkDetentionInvoicesControl";
			createBulkDetentionInvoicesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 411, true);
			createBulkDetentionInvoicesControl.TabIndex = 0;
			createBulkDetentionInvoicesControl.FindClicked += new System.EventHandler(this.createBulkDetentionInvoicesControl_FindClicked);
			// 
			// CreateBulkContainerDetentionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 463, true);
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("CreateBulkContainerDetentionForm|c00399eb-433c-47c1-a5a1-f656a4aa1cb7", "Create Detention Jobs for Multiple Clients");
			this.Controls.Add(createBulkDetentionInvoicesControl);
			this.Controls.Add(bottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BulkDetentionHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 490, true);
			this.Name = "CreateBulkContainerDetentionForm";
			this.Text = "CreateBulkDetentionInvoiceForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(createBulkDetentionInvoicesControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZButton unselectAllButton;
		Enterprise.ZArchitecture.GUI.ZButton selectAllButton;
		Enterprise.ZArchitecture.GUI.ZButton closeButton;
		Enterprise.ZArchitecture.GUI.ZButton createNew;
		CargoWise.Windows.UI.KPanel bottomPanel;
		Enterprise.Freight.Agency.GUI.CreateBulkContainerDetentionControl createBulkDetentionInvoicesControl;
	}
}
