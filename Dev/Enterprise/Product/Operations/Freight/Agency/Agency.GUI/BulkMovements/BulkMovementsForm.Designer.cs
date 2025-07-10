namespace Enterprise.Freight.Agency.GUI
{
	partial class BulkMovementsForm
	{
		new void InitializeComponent()
		{
			this.clearGeneratedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.createButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			bulkMovements = new Enterprise.Freight.Agency.GUI.BulkMovementsControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 389, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BulkMovementsHeader);
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(this.clearGeneratedButton);
			bottomPanel.Controls.Add(this.createButton);
			bottomPanel.Controls.Add(this.closeButton);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 354, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 35, true);
			bottomPanel.TabIndex = 1;
			// 
			// clearGeneratedButton
			// 
			this.clearGeneratedButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsForm|b603d432-66e4-433d-974b-7fbf4f07c9bb", "Clear Generated");
			this.clearGeneratedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.clearGeneratedButton.Name = "clearGeneratedButton";
			this.clearGeneratedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.clearGeneratedButton.TabIndex = 0;
			this.clearGeneratedButton.UseVisualStyleBackColor = true;
			this.clearGeneratedButton.Click += new System.EventHandler(this.removeGeneratedButton_Click);
			// 
			// createButton
			// 
			this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.createButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsForm|69d15620-46ea-4aa6-b96b-b3d3df2bb6c1", "Create");
			this.createButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 8, true);
			this.createButton.Name = "createButton";
			this.createButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.createButton.TabIndex = 1;
			this.createButton.UseVisualStyleBackColor = true;
			this.createButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsForm|16a5faaa-3ea3-4c42-851d-09de999d96c7", "Close");
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(566, 8, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 2;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// bulkMovements
			// 
			bulkMovements.AllowDrop = true;
			this.BindingSource.SetBindingMember(bulkMovements, ".");
			bulkMovements.Dock = System.Windows.Forms.DockStyle.Fill;
			bulkMovements.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			bulkMovements.Name = "bulkMovements";
			bulkMovements.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 354, true);
			bulkMovements.TabIndex = 0;
			// 
			// BulkMovementsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(652, 413, true);
			this.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsForm|ba064a18-ce9b-43f5-997e-2662da6ff9d1", "Container Movements");
			this.Controls.Add(bulkMovements);
			this.Controls.Add(bottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BulkMovementsHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 440, true);
			this.Name = "BulkMovementsForm";
			this.Text = "BulkMovementsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(bulkMovements, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZButton createButton;
		Enterprise.ZArchitecture.GUI.ZButton closeButton;
		ZArchitecture.GUI.ZButton clearGeneratedButton;
		Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		Enterprise.Freight.Agency.GUI.BulkMovementsControl bulkMovements;
	}
}
