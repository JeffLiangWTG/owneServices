using System;
using CargoWise.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class PreAdviceExportToForwardingJobForm : ZChildForm
	{
		private ZGrid zGrid1;
		private ZButton OKButton;

		protected new void InitializeComponent()
		{
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.zGrid1 = new ZGrid();
			this.OKButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 307, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobShipmentPreplanning);
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid1, "MatchingPreAdvices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobShipmentPreplanning)(null)).MatchingPreAdvices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobShipmentPreplanning)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).MatchingPreAdvices)).SyncRoot)).IncludeInExportToForwardingJob)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).MatchingPreAdvices)).SyncRoot)).EF_PreshipID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).MatchingPreAdvices)).SyncRoot)).EF_MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).MatchingPreAdvices)).SyncRoot)).EF_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobShipmentPreplanning)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).MatchingPreAdvices)).SyncRoot)).BuyerPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobShipmentPreplanning)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).MatchingPreAdvices)).SyncRoot)).EF_OH_Carrier)));
			this.zGrid1.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PreAdviceExportToForwardingJobForm|a57f2d6b-cef5-4dd0-a051-3e6e5834f3ff", "Include");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInExportToForwardingJob";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.ColumnName = "EF_PreshipID";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "EF_MasterBill";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.ColumnName = "EF_HouseBill";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "BuyerPK";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "EF_OH_Carrier";
			zOrganisationFindBoxColumnStyleInfo2.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.zGrid1.GridId = "551317a5-f44a-4053-95d9-9b644a8bb3b3";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 60, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 207, true);
			this.zGrid1.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PreAdviceExportToForwardingJobForm|6ef59c24-999c-4341-b10d-a09acfc0cc12", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 278, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// PreAdviceExportToForwardingJobForm
			// 
			this.AcceptButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 331, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PreAdviceExportToForwardingJobForm|93f28cfb-0b5f-4db6-9b19-e3755d2d2dc3", "Matching Pre Advices");
			this.Controls.Add(this.zGrid1);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(JobShipmentPreplanning);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "PreAdviceExportToForwardingJobForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.zGrid1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
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
