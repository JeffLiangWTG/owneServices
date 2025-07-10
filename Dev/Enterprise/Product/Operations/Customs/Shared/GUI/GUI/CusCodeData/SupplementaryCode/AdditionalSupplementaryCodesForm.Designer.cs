using Enterprise.ZArchitecture.GUI;
using System.ComponentModel;
using System;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	partial class AdditionalSupplementaryCodesForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			base.InitializeComponent();
			this.additionalSupplementaryCodesGrid = new ZArchitecture.ZGrid();
			this.closeButton = new ZButton();
			this.oKButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.additionalSupplementaryCodesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 234, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ICusCodeDataCollection<BaseSupplementaryCode>);
			// 
			// OrderItemsGrid
			// 
			this.additionalSupplementaryCodesGrid.AllowNavigation = false;
			this.additionalSupplementaryCodesGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.additionalSupplementaryCodesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((BaseSupplementaryCode)(null)).CY_Code);
			this.additionalSupplementaryCodesGrid.CaptionVisible = false;
			this.additionalSupplementaryCodesGrid.GridId = "C2AEA5CB-701B-4369-8E07-9594FF17D139";
			this.additionalSupplementaryCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.additionalSupplementaryCodesGrid.LayoutKey = "AdditionalSupplementaryCodesGrid";
			this.additionalSupplementaryCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.additionalSupplementaryCodesGrid.Name = "AdditionalSupplementaryCodesGrid";
			this.additionalSupplementaryCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 206, true);
			this.additionalSupplementaryCodesGrid.TabIndex = 1;
			this.additionalSupplementaryCodesGrid.AllowSorting = false;
			// 
			// CloseButton
			// 
			this.closeButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.closeButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("AdditionalSupplementaryCodesForm|84995A45-A3C4-4DE2-BE2C-80DE93435DDB", "Cancel");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 211, true);
			this.closeButton.Name = "CloseButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.closeButton.TabIndex = 3;
			this.closeButton.Click += new EventHandler(this.OnCloseButton_Click);
			// 
			// OKButton
			// 
			this.oKButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.oKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("AdditionalSupplementaryCodesForm|7AF0BF6F-AE74-4E33-A923-E281136F6400", "OK");
			this.oKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 211, true);
			this.oKButton.Name = "OKButton";
			this.oKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.oKButton.TabIndex = 2;
			this.oKButton.Click += new EventHandler(this.OnOKButton_Click);
			// 
			// AdditionalSupplementaryCodesForm
			// 
			this.AcceptButton = this.oKButton;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("AdditionalSupplementaryCodesForm|E733B33A-CFCF-4A68-BEC8-DF1548A87CDC", "Additional Supplementary Codes");
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.additionalSupplementaryCodesGrid);
			this.DataSourceType = typeof(ICusCodeDataCollection<BaseSupplementaryCode>);
			this.Name = "AdditionalSupplementaryCodesForm";
			this.Controls.SetChildIndex(this.additionalSupplementaryCodesGrid, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.additionalSupplementaryCodesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		ZArchitecture.ZGrid additionalSupplementaryCodesGrid;
		ZButton closeButton;
		ZButton oKButton;
	}
}
