namespace Enterprise.Customs.TR.GUI.PlugIn
{
	public partial class AdditionalInfosUserControl
	{
		void InitializeComponent()
		{
            this.AdditionalInfosGroupBox.SuspendLayout();
            this.AdditionalInfosPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).BeginInit();
            this.AdditionalInfosGrid.SuspendLayout();
            this.AddInfoTypeCodeDropEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // AdditionalInfosGroupBox
            // 
            this.AdditionalInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 120, true);
            // 
            // AddiInfoDescriptionTextBox
            // 
            this.AddiInfoDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.AddiInfoDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
            this.AddiInfoDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 100, true);
            // 
            // AdditionalInfosPanel
            // 
            this.AdditionalInfosPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 189, true);
            this.AdditionalInfosPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 120, true);
            // 
            // AdditionalInfosGrid
            // 
            this.AdditionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 189, true);
            // 
            // AddInfoTypeCodeDropEdit
            // 
            this.AddInfoTypeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 44, true);
            this.AddInfoTypeCodeDropEdit.Visible = false;
            // 
            // AdditionalInfosUserControl
            // 
            this.Name = "AdditionalInfosUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 309, true);
            this.AdditionalInfosGroupBox.ResumeLayout(false);
            this.AdditionalInfosGroupBox.PerformLayout();
            this.AdditionalInfosPanel.ResumeLayout(false);
            this.AdditionalInfosPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).EndInit();
            this.AdditionalInfosGrid.ResumeLayout(false);
            this.AdditionalInfosGrid.PerformLayout();
            this.AddInfoTypeCodeDropEdit.ResumeLayout(true);
            this.AddInfoTypeCodeDropEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
