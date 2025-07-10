using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI
{
	public partial class NZCClassForm
	{
		protected override void InitializeComponent()
		{
			this.tariffTreeView = new ZArchitecture.GUI.ZTreeView();
			this.fullDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.statUnitTextBox = new ZArchitecture.ZTextBox();
			this.suppUnitTextBox = new ZArchitecture.ZTextBox();
			this.cancelButton = new ZArchitecture.GUI.ZButton();
			this.oKButton = new ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 391, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 26, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(268);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(269);
			// 
			// TariffTreeView
			// 
			this.tariffTreeView.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.tariffTreeView.ImageIndex = -1;
			this.tariffTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.tariffTreeView.Name = "TariffTreeView";
			this.tariffTreeView.ReadOnly = false;
			this.tariffTreeView.SelectedImageIndex = -1;
			this.tariffTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 280, true);
			this.tariffTreeView.TabIndex = 1;
			// 
			// FullDescriptionTextBox
			// 
			this.fullDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.fullDescriptionTextBox.BindTo = "WrappedLongDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.EDITariff_ReferenceFiles_NZ.IFamilyMember)(((object)(((FamilyMemberCollectionForBinding)(null)))))).WrappedLongDescriptionInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.EDITariff_ReferenceFiles_NZ.IFamilyMember)(((object)(((FamilyMemberCollectionForBinding)(null)))))).WrappedLongDescription);
			this.fullDescriptionTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("266BC2C9-25D3-4661-BCA7-319791CFC345", "FULL DESCRIPTION");
			this.fullDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 296, true);
			this.fullDescriptionTextBox.Multiline = true;
			this.fullDescriptionTextBox.Name = "FullDescriptionTextBox";
			this.fullDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 88, true);
			this.fullDescriptionTextBox.TabIndex = 4;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4AC03CE2-6B73-406B-AEC1-A26FD76519E6", "Stat Unit:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 296, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel1.TabIndex = 5;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("734B6A0C-E26D-4E81-B242-E45657D239A8", "Supp. Unit:");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 320, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel2.TabIndex = 6;
			// 
			// StatUnitTextBox
			// 
			this.statUnitTextBox.BindTo = "StatUnit";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.EDITariff_ReferenceFiles_NZ.IFamilyMember)(((object)(((FamilyMemberCollectionForBinding)(null)))))).StatUnitInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.EDITariff_ReferenceFiles_NZ.IFamilyMember)(((object)(((FamilyMemberCollectionForBinding)(null)))))).StatUnit);
			this.statUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 296, true);
			this.statUnitTextBox.Name = "StatUnitTextBox";
			this.statUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.statUnitTextBox.TabIndex = 7;
			this.statUnitTextBox.Text = "";
			// 
			// SuppUnitTextBox
			// 
			this.suppUnitTextBox.BindTo = "SuppUnit";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.EDITariff_ReferenceFiles_NZ.IFamilyMember)(((object)(((FamilyMemberCollectionForBinding)(null)))))).SuppUnitInfo);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((Business.EDITariff_ReferenceFiles_NZ.IFamilyMember)(((object)(((FamilyMemberCollectionForBinding)(null)))))).SuppUnit);
			this.suppUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 320, true);
			this.suppUnitTextBox.Name = "SuppUnitTextBox";
			this.suppUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.suppUnitTextBox.TabIndex = 8;
			this.suppUnitTextBox.Text = "";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 352, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 10;
			this.cancelButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("91671AE1-4717-4421-8242-1D08B2CEA54B", "&Cancel");
			this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
			// 
			// oKButton
			// 
			this.oKButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.oKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.oKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 352, true);
			this.oKButton.Name = "oKButton";
			this.oKButton.TabIndex = 9;
			this.oKButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("43303A5D-6630-432F-9449-CBFCFA616D1C", "&OK");
			this.oKButton.Click += new EventHandler(this.oKButton_Click);
			// 
			// NZCClassForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 417, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.oKButton);
			this.Controls.Add(this.suppUnitTextBox);
			this.Controls.Add(this.statUnitTextBox);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.fullDescriptionTextBox);
			this.Controls.Add(this.tariffTreeView);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.FamilyMemberCollection" +
				"ForBinding";
			this.Name = "NZCClassForm";
			this.Controls.SetChildIndex(this.tariffTreeView, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.fullDescriptionTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.statUnitTextBox, 0);
			this.Controls.SetChildIndex(this.suppUnitTextBox, 0);
			this.Controls.SetChildIndex(this.oKButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

	}
}
