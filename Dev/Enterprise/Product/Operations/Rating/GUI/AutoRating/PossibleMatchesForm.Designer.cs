using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class PossibleMatchesForm
	{
		PossibleMatchesGrid possibleMatchesGrid;
		ZArchitecture.ZLabel instructionText;
		ZArchitecture.ZLabel descriptionText;
		ZButton okayButton;
		private ZGroupBox jobDetailsGroupBox;
		private ZArchitecture.ZTextBox viaTextBox;
		private ZArchitecture.ZTextBox destinationTextBox;
		private ZArchitecture.ZTextBox originTextBox;
		private ZArchitecture.ZTextBox carrierTextBox;
		private ZArchitecture.ZTextBox commodityTextBox;
		private ZArchitecture.ZTextBox serviceLevelTextBox;
		System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			this.descriptionText = new ZArchitecture.ZLabel();
			this.okayButton = new ZButton();
			this.possibleMatchesGrid = new PossibleMatchesGrid();
			this.instructionText = new ZArchitecture.ZLabel();
			this.jobDetailsGroupBox = new ZGroupBox();
			this.carrierTextBox = new ZArchitecture.ZTextBox();
			this.commodityTextBox = new ZArchitecture.ZTextBox();
			this.serviceLevelTextBox = new ZArchitecture.ZTextBox();
			this.viaTextBox = new ZArchitecture.ZTextBox();
			this.destinationTextBox = new ZArchitecture.ZTextBox();
			this.originTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.possibleMatchesGrid.InnerGrid)).BeginInit();
			this.jobDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PossibleMatchesWrapper);
			// 
			// descriptionText
			// 
			this.descriptionText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 7, true);
			this.descriptionText.Name = "descriptionText";
			this.descriptionText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 37, true);
			this.descriptionText.TabIndex = 1;
			// 
			// okayButton
			// 
			this.okayButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("8415619b-ff0a-4566-a18e-157e44691105", "OK");
			this.okayButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okayButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 401, true);
			this.okayButton.Name = "okayButton";
			this.okayButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.okayButton.TabIndex = 40;
			this.okayButton.Click += new EventHandler(this.OkayButton_Click);
			// 
			// possibleMatchesGrid
			// 
			this.possibleMatchesGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.possibleMatchesGrid, "PossibleMatches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PossibleMatchesWrapper)(null)).PossibleMatches);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("095df76c-3f71-45a5-b3c5-01dc1d1a3904", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "EntryType";
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("d549f9f7-e945-4a55-b455-aa888899c74b", "Organization");
			zTextBoxColumnStyleInfo2.ColumnName = "Organisation";
			zTextBoxColumnStyleInfo3.ColumnName = "TI_OriginLRC";
			zTextBoxColumnStyleInfo4.ColumnName = "TI_DestinationLRC";
			zTextBoxColumnStyleInfo5.ColumnName = "TI_ViaLRC";
			zTextBoxColumnStyleInfo6.ColumnName = "TI_RateCategory";
			zTextBoxColumnStyleInfo7.ColumnName = "TI_Mode";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TI_RC";
			zTextBoxColumnStyleInfo8.ColumnName = "TI_RS_NKServiceLevel_NI";
			zTextBoxColumnStyleInfo9.ColumnName = "TI_RH_NKCommodityCode";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "TI_OH_TransportProvider";
			zTextBoxColumnStyleInfo10.ColumnName = "TI_ContractNumber";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "TI_TransitTime";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo1.ColumnName = "TI_RateStartDate";
			zDateEditColumnStyleInfo2.ColumnName = "TI_RateEndDate";
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.possibleMatchesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.possibleMatchesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.possibleMatchesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.possibleMatchesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.possibleMatchesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.possibleMatchesGrid.GridId = "3d99afe8-0c9f-48d1-86f0-ef99591b9166";
			// 
			// 
			// 
			this.possibleMatchesGrid.InnerGrid.AllowNavigation = false;
			this.possibleMatchesGrid.InnerGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.possibleMatchesGrid.InnerGrid.CaptionVisible = false;
			this.possibleMatchesGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.possibleMatchesGrid.InnerGrid.GridId = null;
			this.possibleMatchesGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.possibleMatchesGrid.InnerGrid.LayoutKey = "Grid";
			this.possibleMatchesGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.possibleMatchesGrid.InnerGrid.Name = "Grid";
			this.possibleMatchesGrid.InnerGrid.ReadOnly = true;
			this.possibleMatchesGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 193, true);
			this.possibleMatchesGrid.InnerGrid.TabIndex = 0;
			this.possibleMatchesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 47, true);
			this.possibleMatchesGrid.Name = "possibleMatchesGrid";
			this.possibleMatchesGrid.ReadOnly = true;
			this.possibleMatchesGrid.ShowAttachButton = false;
			this.possibleMatchesGrid.ShowDetachButton = false;
			this.possibleMatchesGrid.ShowEditButton = false;
			this.possibleMatchesGrid.ShowNewButton = false;
			this.possibleMatchesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(545, 193, true);
			this.possibleMatchesGrid.TabIndex = 5;
			// 
			// instructionText
			// 
			this.instructionText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 243, true);
			this.instructionText.Name = "instructionText";
			this.instructionText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 76, true);
			this.instructionText.TabIndex = 10;
			// 
			// jobDetailsGroupBox
			// 
			this.jobDetailsGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("0bcee824-e2ff-4f65-a722-6de248856a50", "Job Details");
			this.jobDetailsGroupBox.Controls.Add(this.carrierTextBox);
			this.jobDetailsGroupBox.Controls.Add(this.commodityTextBox);
			this.jobDetailsGroupBox.Controls.Add(this.serviceLevelTextBox);
			this.jobDetailsGroupBox.Controls.Add(this.viaTextBox);
			this.jobDetailsGroupBox.Controls.Add(this.destinationTextBox);
			this.jobDetailsGroupBox.Controls.Add(this.originTextBox);
			this.jobDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 322, true);
			this.jobDetailsGroupBox.Name = "jobDetailsGroupBox";
			this.jobDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 73, true);
			this.jobDetailsGroupBox.TabIndex = 11;
			this.jobDetailsGroupBox.TabStop = false;
			// 
			// carrierTextBox
			// 
			this.BindingSource.SetBindingMember(this.carrierTextBox, "Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PossibleMatchesWrapper)(null)).Carrier);
			this.carrierTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("b1b6c760-0acb-4b4c-932a-fba24d67b4c0", "Carrier");
			this.carrierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 45, true);
			this.carrierTextBox.Name = "carrierTextBox";
			this.carrierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.carrierTextBox.TabIndex = 35;
			// 
			// commodityTextBox
			// 
			this.BindingSource.SetBindingMember(this.commodityTextBox, "Commodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PossibleMatchesWrapper)(null)).Commodity);
			this.commodityTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("0c979164-c500-4bf5-94a4-571fd78b10e4", "Commodity");
			this.commodityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 45, true);
			this.commodityTextBox.Name = "commodityTextBox";
			this.commodityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.commodityTextBox.TabIndex = 30;
			// 
			// serviceLevelTextBox
			// 
			this.BindingSource.SetBindingMember(this.serviceLevelTextBox, "ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PossibleMatchesWrapper)(null)).ServiceLevel);
			this.serviceLevelTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("0ba88e0f-fa7f-4a35-84f1-f9acec583fd8", "Service Level");
			this.serviceLevelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 45, true);
			this.serviceLevelTextBox.Name = "serviceLevelTextBox";
			this.serviceLevelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.serviceLevelTextBox.TabIndex = 25;
			// 
			// viaTextBox
			// 
			this.BindingSource.SetBindingMember(this.viaTextBox, "Via");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PossibleMatchesWrapper)(null)).Via);
			this.viaTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("c69528e4-eb99-4a63-9b00-3b5426315f81", "Via");
			this.viaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 19, true);
			this.viaTextBox.Name = "viaTextBox";
			this.viaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.viaTextBox.TabIndex = 20;
			// 
			// destinationTextBox
			// 
			this.BindingSource.SetBindingMember(this.destinationTextBox, "Destination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PossibleMatchesWrapper)(null)).Destination);
			this.destinationTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("61e8f7b4-4c8b-4431-b149-6f66744decee", "Destination");
			this.destinationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 19, true);
			this.destinationTextBox.Name = "destinationTextBox";
			this.destinationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.destinationTextBox.TabIndex = 15;
			// 
			// originTextBox
			// 
			this.BindingSource.SetBindingMember(this.originTextBox, "Origin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PossibleMatchesWrapper)(null)).Origin);
			this.originTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("cb635188-300d-41a0-89cc-edd84fa225cd", "Origin");
			this.originTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 19, true);
			this.originTextBox.Name = "originTextBox";
			this.originTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.originTextBox.TabIndex = 12;
			// 
			// PossibleMatchesForm
			// 

			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PossibleMatchesForm|2059b557-5e39-49a6-9e05-ee4ed5e20d06", "Autorating Notifications");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 448, true);
			this.Controls.Add(this.jobDetailsGroupBox);
			this.Controls.Add(this.instructionText);
			this.Controls.Add(this.possibleMatchesGrid);
			this.Controls.Add(this.descriptionText);
			this.Controls.Add(this.okayButton);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(PossibleMatchesWrapper);
			this.DataSourceTypeName = "Enterprise.Rating.Business.SimpleRateEntryCollectionWrapper";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "PossibleMatchesForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.okayButton, 0);
			this.Controls.SetChildIndex(this.descriptionText, 0);
			this.Controls.SetChildIndex(this.possibleMatchesGrid, 0);
			this.Controls.SetChildIndex(this.instructionText, 0);
			this.Controls.SetChildIndex(this.jobDetailsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.possibleMatchesGrid.InnerGrid)).EndInit();
			this.jobDetailsGroupBox.ResumeLayout(false);
			this.jobDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
