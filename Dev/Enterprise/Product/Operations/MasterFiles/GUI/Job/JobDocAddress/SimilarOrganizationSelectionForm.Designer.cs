using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SimilarOrganizationSelectionForm : DuplicateOrgForm
	{
		internal ZButton newOrganizationButton;

		new void InitializeComponent()
		{
			this.newOrganizationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgsDisplayGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SaveNewOrgButton
			// 
			this.SaveNewOrgButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrganizationSelectionForm|DD9117C2-1465-4A43-BCD1-ECABA912B9B0", "&Select Organization");
			// 
			// WarningLabel
			// 
			this.WarningLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrganizationSelectionForm|80346427-F8E5-44E2-B261-D65FF4A871E7", "Similar organizations already exist. You may select one of them or create a new one.");
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 351, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 24, true);
			// 
			// newOrganizationButton
			// 
			this.newOrganizationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.newOrganizationButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrganizationSelectionForm|2CD504E1-D22F-48EA-BC57-A25A56683809", "&New Organization");
			this.newOrganizationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 318, true);
			this.newOrganizationButton.Name = "newOrganizationButton";
			this.newOrganizationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.newOrganizationButton.TabIndex = 7;
			this.newOrganizationButton.TabStop = false;
			this.newOrganizationButton.Click += NewOrganizationButton_Click;
			// 
			// SimilarOrganizationSelectionForm
			// 
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrganizationSelectionForm|E155DF0C-182D-48A2-86BB-DA603211D15C", "Similar Organizations");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 375, true);
			this.Controls.Add(this.newOrganizationButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 300, true);
			this.Name = "SimilarOrganizationSelectionForm";
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
	}
}
