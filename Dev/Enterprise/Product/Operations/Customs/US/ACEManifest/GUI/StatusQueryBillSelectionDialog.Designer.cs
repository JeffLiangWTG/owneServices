namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class StatusQueryBillSelectionDialog
	{
		protected new void InitializeComponent()
		{
			this.RequestCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SelectedItemCountLabel
			// 
			this.SelectedItemCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 10, true);
			// 
			// DescPanel
			// 
			this.DescPanel.Controls.Add(this.RequestCodeDropEdit);
			this.DescPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.DescPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 32, true);
			this.DescPanel.Controls.SetChildIndex(this.RequestCodeDropEdit, 0);
			this.DescPanel.Controls.SetChildIndex(this.SelectedItemCountLabel, 0);
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 221, true);
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 238, true);
			// 
			// RequestCodeDropEdit
			//
			this.BindingSource.SetBindingMember(this.RequestCodeDropEdit, "RequestCode");
			this.RequestCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("4F86AD5C-CF00-4113-AA06-1BBBF50A78E5", "Request Code");
			this.RequestCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RequestCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 4, true);
			this.RequestCodeDropEdit.Name = "RequestCodeDropEdit";
			this.RequestCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 21, true);
			this.RequestCodeDropEdit.TabIndex = 3;
			// 
			// StatusQueryBillSelectionDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 300, true);
			this.Name = "StatusQueryBillSelectionDialog";
			this.DescPanel.ResumeLayout(false);
			this.DescPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZDropEdit RequestCodeDropEdit;
	}
}
