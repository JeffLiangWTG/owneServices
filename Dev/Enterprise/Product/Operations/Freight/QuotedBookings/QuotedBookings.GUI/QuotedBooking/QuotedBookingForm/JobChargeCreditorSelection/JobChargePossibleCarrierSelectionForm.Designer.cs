using System;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class JobChargePossibleCarrierSelectionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		Enterprise.ZArchitecture.GUI.ZButton BtnCancel;
		Enterprise.ZArchitecture.GUI.ZButton BtnOK;

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.BtnCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BtnOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PotentialCarriersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PotentialCarriersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PotentialCarriersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PotentialCarriersGrid)).BeginInit();
			this.PotentialCarriersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 224, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.JobChargePossibleCarrierSelection);
			// 
			// BtnCancel
			// 
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 196, true);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.BtnCancel.TabIndex = 3;
			this.BtnCancel.ToolTipCaption = null;
			this.BtnCancel.CaptionResourceString = Enterprise.Freight.QuotedBookings.GUI.Res.GetData("1abeb001-afef-4752-85a6-cab6d77e28fa", "&Cancel");
			this.BtnCancel.Click += new EventHandler(this.CancelButton_Click);
			// 
			// BtnOK
			// 
			this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnOK.CaptionResourceString = Enterprise.Freight.QuotedBookings.GUI.Res.GetData("ba6dc35d-47fa-4fb8-a454-a740b2902aa9", "&OK");
			this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.BtnOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 196, true);
			this.BtnOK.Name = "BtnOK";
			this.BtnOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.BtnOK.TabIndex = 2;
			this.BtnOK.ToolTipCaption = null;
			this.BtnOK.Click += new EventHandler(this.OKButton_Click);
			// 
			// PotentialCarriersGroupBox
			// 
			this.PotentialCarriersGroupBox.Controls.Add(this.PotentialCarriersGrid);
			this.PotentialCarriersGroupBox.CaptionResourceString = Res.GetData("c0d99565-fa9f-44cd-b38d-7613318efa2b", "Potential Carriers");
			this.PotentialCarriersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.PotentialCarriersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PotentialCarriersGroupBox.Name = "PotentialCarriersGroupBox";
			this.PotentialCarriersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 185, true);
			this.PotentialCarriersGroupBox.TabStop = false;
			// 
			// PotentialCarriersGrid
			// 
			this.PotentialCarriersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PotentialCarriersGrid, "PossibleCreditorsAndCarriers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.QuotedBookings.Business.JobChargePossibleCarrierSelection)(null)).PossibleCreditorsAndCarriers)));
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("cdbd4c21-e1af-494a-88da-7dbdc241a9c4", "Code");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "TTC_OH_Carrier";
			zOrganisationFindBoxColumnStyleInfo1.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("861bf317-89e9-47fa-974b-d30bdfba6348", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "CarrierName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo3.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("83cac9e1-4c6f-4b32-a5ad-37b12ab90ea2", "Creditor");
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "TTC_OH_Creditor";
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zOrganisationFindBoxColumnStyleInfo3.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo3.IsReadOnly = true;

			this.PotentialCarriersGrid.CaptionVisible = false;
			this.PotentialCarriersGrid.ReadOnly = true;
			this.PotentialCarriersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PotentialCarriersGrid.GridId = "e9c65a01-0731-40de-b1b7-a5d4e4312c4f";
			this.PotentialCarriersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PotentialCarriersGrid.LayoutKey = "PotentialCarriersGrid";
			this.PotentialCarriersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.PotentialCarriersGrid.Name = "PotentialCarriersGrid";
			this.PotentialCarriersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 170, true);
			this.PotentialCarriersGrid.TabIndex = 1;
			this.PotentialCarriersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.PotentialCarriersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PotentialCarriersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.PotentialCarriersGrid.DoubleClick += PotentialCarriersGrid_DoubleClick;
			// 
			// JobChargeCreditorCarriersSelectionForm
			// 
			this.AcceptButton = this.BtnOK;
			this.CancelButton = this.BtnCancel;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 247, true);
			this.Controls.Add(this.BtnOK);
			this.Controls.Add(this.PotentialCarriersGroupBox);
			this.Controls.Add(this.BtnCancel);
			this.DataSourceAssemblyName = "Enterprise.Freight.QuotedBooking.GUI";
			this.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.JobChargePossibleCarrierSelection);
			this.DataSourceTypeName = "Enterprise.Freight.QuotedBookings.Business.JobChargeCreditorSelection";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "JobChargeCreditorCarriersSelectionForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.BtnCancel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PotentialCarriersGroupBox, 0);
			this.Controls.SetChildIndex(this.BtnOK, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PotentialCarriersGroupBox.ResumeLayout(false);
			this.PotentialCarriersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PotentialCarriersGrid)).EndInit();
			this.PotentialCarriersGrid.ResumeLayout(false);
			this.PotentialCarriersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PotentialCarriersGroupBox;
		private ZArchitecture.ZGrid PotentialCarriersGrid;
	}
}
