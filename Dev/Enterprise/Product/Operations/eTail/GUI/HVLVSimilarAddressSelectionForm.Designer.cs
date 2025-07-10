using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVSimilarAddressSelectionForm
	{
		new void InitializeComponent()
		{
			this.newOrganizationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgsDisplayGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// SimilarOrgsDisplayGrid
			//
			this.SimilarOrgsDisplayGrid.MouseDoubleClick += SimilarOrgDisplayGrid_MouseDoubleClick;
			this.SimilarOrgsDisplayGrid.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 333, true);
			//
			// SaveNewOrgButton
			//
			this.SaveNewOrgButton.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("HVLVSimilarAddressSelectionForm|838d4666-64fe-4d58-8a72-fe0d94537de1", "&Select Address");
			this.SaveNewOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 405, true);
			//
			// CancelSaveButton
			//
			this.CancelSaveButton.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("d367ee6c-6c8d-460f-9d20-212651149e7d", "Ignore");
			this.CancelSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 405, true);
			//
			// WarningLabel
			//
			this.WarningLabel.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("HVLVSimilarAddressSelectionForm|bb20d2bb-827d-4ce9-8679-33dc1fcc8219", "Similar addresses already exist. You may select one of them or create a new one.");
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 32, true);
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 16, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 351, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 24, true);
			//
			// newOrganizationButton
			//
			this.newOrganizationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.newOrganizationButton.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("HVLVSimilarAddressSelectionForm|f452556b-f7f2-47c7-8dd8-9eb76f32e8b7", "&New Organization");
			this.newOrganizationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 405, true);
			this.newOrganizationButton.Name = "newOrganizationButton";
			this.newOrganizationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.newOrganizationButton.TabIndex = 7;
			this.newOrganizationButton.TabStop = false;
			this.newOrganizationButton.Click += NewOrganizationButton_Click;
			//
			// HVLVSimilarAddressSelectionForm
			//
			this.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("HVLVSimilarAddressSelectionForm|ec297970-625c-40fb-9d12-5dc2826dae5f", "Similar Addresses");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 470, true);
			this.Controls.Add(this.newOrganizationButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = true;
			this.MaximizeBox = true;
			this.Name = "HVLVSimilarAddressSelectionForm";
			this.Controls.SetChildIndex(this.newOrganizationButton, 0);
			this.Controls.SetChildIndex(this.SimilarOrgsDisplayGrid, 0);
			this.Controls.SetChildIndex(this.SaveNewOrgButton, 0);
			this.Controls.SetChildIndex(this.CancelSaveButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.WarningLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgsDisplayGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		ZButton newOrganizationButton;
	}
}
