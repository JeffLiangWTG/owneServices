namespace Enterprise.MasterFiles.GUI
{
	partial class AccSurchargeConfigurationCollectionForm
	{
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.GUI.ZButton OKButton;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.AccSurchargeConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ButtonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AccSurchargeConfigurationGrid)).BeginInit();
            this.AccSurchargeConfigurationGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 212, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 23, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccSurchargeConfigurationCollection);
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this.OKButton);
            this.ButtonPanel.Controls.Add(this.CloseButton);
            this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 5, 0, true);
            this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 22, true);
            this.ButtonPanel.TabIndex = 2;
            // 
            // OKButton
            // 
            this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccSurchargeConfigurationCollectionForm|255d6923-e4c9-421b-8b5c-619b6a30a4c8", "OK");
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.OKButton.IsCaptionOverridden = false;
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 0, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
            this.OKButton.TabIndex = 2;
            this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.OKButton.ToolTipCaption = null;
            this.OKButton.Click += new System.EventHandler(this.OnOKButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccSurchargeConfigurationCollectionForm|780a1cec-c433-4785-b000-bad86388f099", "Cancel");
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.CloseButton.IsCaptionOverridden = false;
            this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 0, true);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.CloseButton.ToolTipCaption = null;
            this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
            // 
            // AccSurchargeConfigurationGrid
            // 
            this.AccSurchargeConfigurationGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AccSurchargeConfigurationGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccSurchargeConfiguration)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccSurchargeConfiguration)(null)).IsApplicable)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeConfiguration)(null)).ASC_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeConfiguration)(null)).ASC_Description)));
            this.AccSurchargeConfigurationGrid.CaptionVisible = false;

            zCheckBoxColumnStyleInfo1.ColumnName = "IsApplicable";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccSurchargeConfigurationCollectionForm|be661294-4151-46ee-b68b-88cd69f3d94e", "Is Applicable");

            zTextBoxColumnStyleInfo1.ColumnName = "ASC_Code";
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccSurchargeConfigurationCollectionForm|58046e9f-b677-4d91-8470-fb562ab725ff", "Code");

			zTextBoxColumnStyleInfo2.ColumnName = "ASC_Description";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccSurchargeConfigurationCollectionForm|30f90eaf-51d5-4178-84da-4727a5d606de", "Surcharge Description");
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);

            this.AccSurchargeConfigurationGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.AccSurchargeConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.AccSurchargeConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.AccSurchargeConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AccSurchargeConfigurationGrid.GridId = "7c4ac6fa-542e-4e6c-a59d-d00ad2e55ee7";
            this.AccSurchargeConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AccSurchargeConfigurationGrid.LayoutKey = "AccSurchargeConfigurationGrid";
            this.AccSurchargeConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AccSurchargeConfigurationGrid.Name = "AccSurchargeConfigurationGrid";
            this.AccSurchargeConfigurationGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
            this.AccSurchargeConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 212, true);
            this.AccSurchargeConfigurationGrid.TabIndex = 3;
            // 
            // AccSurchargeConfigurationCollectionForm
            // 
            this.AcceptButton = this.OKButton;
            this.CancelButton = this.CloseButton;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccSurchargeConfigurationCollectionForm|0ea7b25c-b659-4e2a-83aa-aa2c2d23d9fa", "Surcharge Configurations");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
            this.Controls.Add(this.AccSurchargeConfigurationGrid);
            this.Controls.Add(this.ButtonPanel);
            this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
            this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccSurchargeConfigurationCollection);
            this.DataSourceTypeName = "Enterprise.MasterFiles.Business.AccSurchargeConfigurationCollection";
            this.Name = "AccSurchargeConfigurationCollectionForm";
            this.Controls.SetChildIndex(this.ButtonPanel, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.AccSurchargeConfigurationGrid, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ButtonPanel.ResumeLayout(false);
            this.ButtonPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AccSurchargeConfigurationGrid)).EndInit();
            this.AccSurchargeConfigurationGrid.ResumeLayout(false);
            this.AccSurchargeConfigurationGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		#endregion

		private ZArchitecture.ZGrid AccSurchargeConfigurationGrid;
	}
}
