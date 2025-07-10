using System;
using System.ComponentModel;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	partial class ContainerYardConfigurationUserControl
	{
		Enterprise.ZArchitecture.GUI.ZGroupBox zContainerYardGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel ContainerYardDetailsPanel;
		Enterprise.ZArchitecture.GUI.ZTabControl ContainerYardCarrierTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ContainerYardCarrierTabPage;
		Enterprise.ZArchitecture.GUI.ZPanel ContainerYardParkPanel;
		Enterprise.ZArchitecture.ZGrid CarrierCYGrid;
		Enterprise.ZArchitecture.ZGrid CarrierCYContainerTypesGrid;
		CargoWise.Windows.UI.KSplitContainer CYSplitContainer;
		Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditContainerStorageClass;
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxContainerStorageClassDescription;
		Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxCYWorkOrderApprovalLimitCurrency;
		Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditCYWorkOrderApprovalLimit;
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxCYWorkOrderApprovalNumber;
		Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidCYWorkOrderApprovedBy;
		Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxCarrier;
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxPortOrCountry;
		IContainer components;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.zContainerYardGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContainerYardCarrierTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainerYardCarrierTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerYardDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zDropEditContainerStorageClass = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.zTextBoxContainerStorageClassDescription = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zCodeFindBoxCYWorkOrderApprovalLimitCurrency = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.zCalcEditCYWorkOrderApprovalLimit = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.zTextBoxCYWorkOrderApprovalNumber = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGuidCYWorkOrderApprovedBy = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.zOrganisationFindBoxCarrier = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.zTextBoxPortOrCountry = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContainerYardParkPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CarrierCYContainerTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CarrierCYGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CYSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zContainerYardGroupBox.SuspendLayout();
			this.ContainerYardCarrierTabControl.SuspendLayout();
			this.ContainerYardDetailsPanel.SuspendLayout();
			this.ContainerYardCarrierTabPage.SuspendLayout();
			this.ContainerYardParkPanel.SuspendLayout();
			this.CarrierCYContainerTypesGrid.SuspendLayout();
			this.CarrierCYGrid.SuspendLayout();
			this.CYSplitContainer.Panel1.SuspendLayout();
			this.CYSplitContainer.Panel2.SuspendLayout();
			this.CYSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// zContainerYardGroupBox
			// 
			this.zContainerYardGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|zContainerYardGroupBox", "Container Yard Related Parties");
			this.zContainerYardGroupBox.Controls.Add(this.ContainerYardCarrierTabControl);
			this.zContainerYardGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zContainerYardGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zContainerYardGroupBox.Name = "zContainerYardGroupBox";
			this.zContainerYardGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 500, true);
			this.zContainerYardGroupBox.TabIndex = 0;
			this.zContainerYardGroupBox.TabStop = false;
			// 
			// ContainerYardCarrierTabControl
			// 
			this.ContainerYardCarrierTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ContainerYardCarrierTabControl.Controls.Add(this.ContainerYardCarrierTabPage);
			this.ContainerYardCarrierTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerYardCarrierTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainerYardCarrierTabControl.Name = "ContainerYardCarrierTabControl";
			this.ContainerYardCarrierTabControl.SelectedIndex = 0;
			this.ContainerYardCarrierTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 481, true);
			this.ContainerYardCarrierTabControl.TabIndex = 1;
			// 
			// ContainerYardCarrierTabPage
			// 
			this.ContainerYardCarrierTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|ContainerYardCarrierTabPage", "Carrier");
			this.ContainerYardCarrierTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerYardCarrierTabPage.Name = "ContainerYardCarrierTabPage";
			this.ContainerYardCarrierTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 454, true);
			this.ContainerYardCarrierTabPage.TabIndex = 2;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).ContainerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_ContainerStorageClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).ContainerTypes)).SyncRoot)).ContainerStorageClassDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_RX_NKCYWorkOrderApprovalLimitCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_CYWorkOrderApprovalLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_CYWorkOrderApprovalNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgParkContainerType)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).ContainerTypes)).SyncRoot)).PT_OC_CYWorkOrderApprovedBy)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).O5_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCarrierAppointedAgentPorts)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ContainerYardRelatedCarrierAppointedAgentPorts)).SyncRoot)).O5_PortOrCountry)));
			// 
			// ContainerYardDetailsPanel
			// 
			this.ContainerYardDetailsPanel.Controls.Add(this.zContainerYardGroupBox);
			this.ContainerYardDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerYardDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerYardDetailsPanel.Name = "ContainerYardDetailsPanel";
			this.ContainerYardDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 500, true);
			this.ContainerYardDetailsPanel.TabIndex = 4;
			this.ContainerYardCarrierTabPage.Controls.Add(this.ContainerYardParkPanel);
			// 
			// ContainerYardParkPanel
			// 
			this.ContainerYardParkPanel.Controls.Add(this.CYSplitContainer);
			this.ContainerYardParkPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerYardParkPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerYardParkPanel.Name = "ContainerYardParkPanel";
			this.ContainerYardParkPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 454, true);
			this.ContainerYardParkPanel.TabIndex = 4;
			// 
			// CYPTypesGrid
			// 
			this.CarrierCYContainerTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CarrierCYContainerTypesGrid, "ContainerYardRelatedCarrierAppointedAgentPorts.ContainerTypes");
			this.CarrierCYContainerTypesGrid.CaptionVisible = false;
			zDropEditContainerStorageClass.ColumnName = "PT_ContainerStorageClass";
			zDropEditContainerStorageClass.IsMandatory = true;
			zDropEditContainerStorageClass.ShowHorizontalScrollBar = false;
			zDropEditContainerStorageClass.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxContainerStorageClassDescription.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContainerYardConfigurationUserControl|ContainerStorageClassDescription", "Desc.", "Desc.", "Description");
			zTextBoxContainerStorageClassDescription.ColumnName = "ContainerStorageClassDescription";
			zTextBoxContainerStorageClassDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContainerYardConfigurationUserControl|PT_RX_NKCYWorkOrderApprovalLimitCurrency", "CY Work Order Approval Limit Currency");
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.ColumnName = "PT_RX_NKCYWorkOrderApprovalLimitCurrency";
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxCYWorkOrderApprovalLimitCurrency.IsVisible = false;
			zCalcEditCYWorkOrderApprovalLimit.BindToDecimalPlaces = null;
			zCalcEditCYWorkOrderApprovalLimit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContainerYardConfigurationUserControl|PT_CYWorkOrderApprovalLimit", "CY Work Order Approval Limit");
			zCalcEditCYWorkOrderApprovalLimit.ColumnName = "PT_CYWorkOrderApprovalLimit";
			zCalcEditCYWorkOrderApprovalLimit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditCYWorkOrderApprovalLimit.IsVisible = false;
			zTextBoxCYWorkOrderApprovalNumber.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContainerYardConfigurationUserControl|PT_CYWorkOrderApprovalNumber", "CY Work Order Approval Number");
			zTextBoxCYWorkOrderApprovalNumber.ColumnName = "PT_CYWorkOrderApprovalNumber";
			zTextBoxCYWorkOrderApprovalNumber.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxCYWorkOrderApprovalNumber.IsVisible = false;
			zGuidCYWorkOrderApprovedBy.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ContainerYardConfigurationUserControl|PT_OC_CYWorkOrderApprovedBy", "CY Work Order Approved By");
			zGuidCYWorkOrderApprovedBy.ColumnName = "PT_OC_CYWorkOrderApprovedBy";
			zGuidCYWorkOrderApprovedBy.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidCYWorkOrderApprovedBy.IsVisible = false;
			this.CarrierCYContainerTypesGrid.ColumnStyles.Add(zDropEditContainerStorageClass);
			this.CarrierCYContainerTypesGrid.ColumnStyles.Add(zTextBoxContainerStorageClassDescription);
			this.CarrierCYContainerTypesGrid.ColumnStyles.Add(zCodeFindBoxCYWorkOrderApprovalLimitCurrency);
			this.CarrierCYContainerTypesGrid.ColumnStyles.Add(zCalcEditCYWorkOrderApprovalLimit);
			this.CarrierCYContainerTypesGrid.ColumnStyles.Add(zTextBoxCYWorkOrderApprovalNumber);
			this.CarrierCYContainerTypesGrid.ColumnStyles.Add(zGuidCYWorkOrderApprovedBy);
			this.CarrierCYContainerTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CarrierCYContainerTypesGrid.GridId = "69150c56-227e-4b35-86ad-ad4228f47874";
			this.CarrierCYContainerTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CarrierCYContainerTypesGrid.LayoutKey = "CarrierCYContainerTypesGrid";
			this.CarrierCYContainerTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CarrierCYContainerTypesGrid.Name = "CarrierCYContainerTypesGrid";
			this.CarrierCYContainerTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 454, true);
			this.CarrierCYContainerTypesGrid.TabIndex = 5;
			this.CarrierCYContainerTypesGrid.ReadOnly = true;
			// 
			// CYPGrid
			// 
			this.CarrierCYGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CarrierCYGrid, "ContainerYardRelatedCarrierAppointedAgentPorts");
			this.CarrierCYGrid.CaptionVisible = false;
			zOrganisationFindBoxCarrier.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|O5_OH", "Carrier");
			zOrganisationFindBoxCarrier.ColumnName = "O5_OH";
			zOrganisationFindBoxCarrier.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxPortOrCountry.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CarrierConfigurationUserControl|O5_PortOrCountry", "Port");
			zTextBoxPortOrCountry.ColumnName = "O5_PortOrCountry";
			zTextBoxPortOrCountry.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CarrierCYGrid.ColumnStyles.Add(zOrganisationFindBoxCarrier);
			this.CarrierCYGrid.ColumnStyles.Add(zTextBoxPortOrCountry);
			this.CarrierCYGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CarrierCYGrid.GridId = "22c8439c-fe72-475c-bffe-a535d5838ed1";
			this.CarrierCYGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CarrierCYGrid.LayoutKey = "CarrierCYPGrid";
			this.CarrierCYGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CarrierCYGrid.Name = "CarrierCYGrid";
			this.CarrierCYGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 454, true);
			this.CarrierCYGrid.TabIndex = 4;
			this.CarrierCYGrid.ReadOnly = true;
			// 
			// CYSplitContainer
			// 
			this.CYSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CYSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CYSplitContainer.Name = "CYSplitContainer";
			// 
			// CYSplitContainer.Panel1
			// 
			this.CYSplitContainer.Panel1.Controls.Add(this.CarrierCYGrid);
			// 
			// CYSplitContainer.Panel2
			// 
			this.CYSplitContainer.Panel2.Controls.Add(this.CarrierCYContainerTypesGrid);
			this.CYSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(866, 454, true);
			this.CYSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(398);
			this.CYSplitContainer.TabIndex = 6;
			this.ContainerYardCarrierTabPage.PerformLayout();
			this.ContainerYardParkPanel.ResumeLayout(false);
			this.ContainerYardParkPanel.PerformLayout();
			this.CarrierCYContainerTypesGrid.ResumeLayout(false);
			this.CarrierCYContainerTypesGrid.PerformLayout();
			this.CarrierCYGrid.ResumeLayout(false);
			this.CarrierCYGrid.PerformLayout();
			this.CYSplitContainer.Panel1.ResumeLayout(false);
			this.CYSplitContainer.Panel2.ResumeLayout(false);
			this.CYSplitContainer.ResumeLayout(false);
			this.CYSplitContainer.PerformLayout();
			this.ContainerYardCarrierTabPage.ResumeLayout(true);
			// 
			// ContainerYardConfigurationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainerYardDetailsPanel);
			this.Name = "ContainerYardConfigurationUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zContainerYardGroupBox.ResumeLayout(false);
			this.zContainerYardGroupBox.PerformLayout();
			this.ContainerYardCarrierTabControl.ResumeLayout(false);
			this.ContainerYardCarrierTabControl.PerformLayout();
			this.ContainerYardDetailsPanel.ResumeLayout(false);
			this.ContainerYardDetailsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
