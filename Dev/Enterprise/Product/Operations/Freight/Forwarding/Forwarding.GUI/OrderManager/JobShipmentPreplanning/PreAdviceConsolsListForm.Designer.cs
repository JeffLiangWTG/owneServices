using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class PreAdviceConsolsListForm : ZChildForm
	{
		private ZLabel DescriptionLabel;
		private ZGuidFindBox FindBox;
		private ZButton CancelButtonX;
		private ZButton OKButton;

		protected override void InitializeComponent()
		{
			this.DescriptionLabel = new ZLabel();
			this.FindBox = new ZGuidFindBox();
			this.CancelButtonX = new ZButton();
			this.OKButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 10, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(QueryUserFindboxEventArgs);
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 14, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 33, true);
			this.DescriptionLabel.TabIndex = 1;
			// 
			// FindBox
			// 
			this.BindingSource.SetBindingMember(this.FindBox, "SelectedItemPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((QueryUserFindboxEventArgs)(null)).SelectedItemPK)));
			this.FindBox.CaptionResourceString = Res.GetData("07726cd4-3c39-4fd9-8ccf-0ddd4247847d", "Consolidation");
			this.FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 60, true);
			this.FindBox.Name = "FindBox";
			this.FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 20, true);
			this.FindBox.TabIndex = 2;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.CaptionResourceString = Res.GetData("eda79155-9bad-4c1b-9469-4a6bec4eb39c", "Cancel");
			this.CancelButtonX.DialogResult = DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 95, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 4;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += CancelButtonX_Click;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Res.GetData("387ff303-b5a6-4849-8f10-1a931fa4b947", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 95, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += OKButton_Click;
			// 
			// PreAdviceConsolsListForm
			// 
			this.AcceptButton = this.OKButton;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 136, true);
			this.CaptionResourceString = Res.GetData("2b87cdb5-d47a-442d-8435-ebff72a70bf3", "Select a Consolidation");
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.FindBox);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(QueryUserFindboxEventArgs);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.QueryUserFindboxEventArgs";
			this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			this.Name = "PreAdviceConsolsListForm";
			this.StartPosition = FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.FindBox, 0);
			this.Controls.SetChildIndex(this.DescriptionLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#region Dispose

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				components?.Dispose();
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
    }
}
