using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class PreAdviceDeclarationsListForm : ZChildForm
	{
		private ZLabel zLabel1;
		private ZGuidFindBox FindBox;
		private ZButton CancelButtonX;
		private ZButton OKButton;

		protected override void InitializeComponent()
		{
			this.zLabel1 = new ZLabel();
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
			// zLabel1
			// 
			this.zLabel1.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 33, true);
			this.zLabel1.TabIndex = 1;
			// 
			// FindBox
			// 
			this.BindingSource.SetBindingMember(this.FindBox, "SelectedItemPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((QueryUserFindboxEventArgs)(null)).SelectedItemPK)));
			this.FindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PreAdviceDeclarationsListForm|2b48ea48-979b-4436-bf1e-305299fb2853", "Declaration");
			this.FindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 60, true);
			this.FindBox.Name = "FindBox";
			this.FindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 20, true);
			this.FindBox.TabIndex = 2;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PreAdviceDeclarationsListForm|bbc952ae-fe4d-4481-b15b-2cf43293a130", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 95, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 4;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new EventHandler(this.CancelButtonX_Click);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PreAdviceDeclarationsListForm|b578e10f-ee35-4349-bee0-a0a6ff213e9d", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 95, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// PreAdviceDeclarationsListForm
			// 
			this.AcceptButton = this.OKButton;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 136, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PreAdviceDeclarationsListForm|aa16dc19-00f2-4912-a698-ab66d92c5bb7", "Select a Declaration");
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.FindBox);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(QueryUserFindboxEventArgs);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.QueryUserFindboxEventArgs";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PreAdviceDeclarationsListForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.FindBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
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
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
    }
}
