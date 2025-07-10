using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class CompetitorTradeProfileUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ControllingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ServiceProviderGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BuyerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SupplierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ClientsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ControllingAgentGuidFindBox.SuspendLayout();
			this.ServiceProviderGuidFindBox.SuspendLayout();
			this.BuyerGuidFindBox.SuspendLayout();
			this.SupplierGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientsGrid)).BeginInit();
			this.ClientsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ControllingAgentGuidFindBox
			// 
			this.ControllingAgentGuidFindBox.AllowDrop = true;
			this.ControllingAgentGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ControllingAgentGuidFindBox, "Clients.PAP_OH_ControllingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).PAP_OH_ControllingAgent)));
			this.ControllingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 466, true);
			this.ControllingAgentGuidFindBox.Name = "ControllingAgentGuidFindBox";
			this.ControllingAgentGuidFindBox.PopupCaption = "Consigns";
			this.ControllingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.ControllingAgentGuidFindBox.TabIndex = 83;
			// 
			// ServiceProviderGuidFindBox
			// 
			this.ServiceProviderGuidFindBox.AllowDrop = true;
			this.ServiceProviderGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ServiceProviderGuidFindBox, "Clients.PAP_OH_ServiceProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).PAP_OH_ServiceProvider)));
			this.ServiceProviderGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 440, true);
			this.ServiceProviderGuidFindBox.Name = "ServiceProviderGuidFindBox";
			this.ServiceProviderGuidFindBox.PopupCaption = "Consigns";
			this.ServiceProviderGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.ServiceProviderGuidFindBox.TabIndex = 81;
			// 
			// BuyerGuidFindBox
			// 
			this.BuyerGuidFindBox.AllowDrop = true;
			this.BuyerGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.BuyerGuidFindBox, "Clients.TradeDetail+PA_Calc_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.PA_Calc_OH_Buyer)));
			this.BuyerGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorTradeProfileUserControl|65a37409-3cc9-45d3-8522-5499032527dc", "Consignee");
			this.BuyerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 466, true);
			this.BuyerGuidFindBox.Name = "BuyerGuidFindBox";
			this.BuyerGuidFindBox.PopupCaption = "Consigns";
			this.BuyerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.BuyerGuidFindBox.TabIndex = 79;
			// 
			// SupplierGuidFindBox
			// 
			this.SupplierGuidFindBox.AllowDrop = true;
			this.SupplierGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.SupplierGuidFindBox, "Clients.TradeDetail+PA_Calc_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.PA_Calc_OH_Supplier)));
			this.SupplierGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorTradeProfileUserControl|ae246368-873b-4780-991e-93b1f1ff2f85", "Consignor");
			this.SupplierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 440, true);
			this.SupplierGuidFindBox.Name = "SupplierGuidFindBox";
			this.SupplierGuidFindBox.PopupCaption = "Consigns";
			this.SupplierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.SupplierGuidFindBox.TabIndex = 77;
			// 
			// ClientsGrid
			// 
			this.ClientsGrid.AllowNavigation = false;
			this.ClientsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientsGrid, "Clients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.PA_TradeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.PA_Calc_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.PA_Calc_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.PA_Calc_Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.PA_Calc_Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).PAP_IncoTradeTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).PAP_RH_NKCommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).PAP_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.CurrentProspectPeriod.PAS_RepeatsMnth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.CurrentProspectPeriod.PAS_Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.CurrentProspectPeriod.PAS_CurrentRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).TradeDetail.CurrentProspectPeriod.PAS_RateOffered)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgTradeProspect)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Clients)).SyncRoot)).ConversionCertaintyLikertItemDescription)));
			this.ClientsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "TradeDetail+PA_TradeType";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorTradeProfileUserControl|17fa5431-bf18-4d08-b4b9-5a29566a53cd", "Consignor");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "TradeDetail+PA_Calc_OH_Supplier";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorTradeProfileUserControl|18e39176-9db3-4d08-ba9e-97b113b588d2", "Consignee");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "TradeDetail+PA_Calc_OH_Buyer";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorTradeProfileUserControl|5437e091-e743-4bee-8f70-c876b6e47383", "Origin");
			zTextBoxColumnStyleInfo9.ColumnName = "TradeDetail+PA_Calc_Origin";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompetitorTradeProfileUserControl|720c71cc-d699-4138-bd18-a38f7b0726f2", "Dest.", "Destination");
			zTextBoxColumnStyleInfo10.ColumnName = "TradeDetail+PA_Calc_Destination";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.ColumnName = "PAP_IncoTradeTerm";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo12.ColumnName = "PAP_RH_NKCommodityCode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo13.ColumnName = "PAP_RS_NKServiceLevel";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_RepeatsMnth";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_Units";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo14.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_RX_NKCurrency";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_CurrentRate";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "TradeDetail+CurrentProspectPeriod+PAS_RateOffered";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "ConversionCertaintyLikertItemDescription";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ClientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ClientsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.ClientsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ClientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ClientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ClientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ClientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ClientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ClientsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ClientsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ClientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ClientsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ClientsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.ClientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ClientsGrid.GridId = "d0e29c4a-9340-4333-857b-3acef3f341b1";
			this.ClientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClientsGrid.LayoutKey = "zGrid1";
			this.ClientsGrid.LimitedColumns = null;
			this.ClientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ClientsGrid.Name = "ClientsGrid";
			this.ClientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 431, true);
			this.ClientsGrid.TabIndex = 76;
			// 
			// CompetitorTradeProfileUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ControllingAgentGuidFindBox);
			this.Controls.Add(this.ServiceProviderGuidFindBox);
			this.Controls.Add(this.BuyerGuidFindBox);
			this.Controls.Add(this.SupplierGuidFindBox);
			this.Controls.Add(this.ClientsGrid);
			this.Name = "CompetitorTradeProfileUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 489, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ControllingAgentGuidFindBox.ResumeLayout(true);
			this.ControllingAgentGuidFindBox.PerformLayout();
			this.ServiceProviderGuidFindBox.ResumeLayout(true);
			this.ServiceProviderGuidFindBox.PerformLayout();
			this.BuyerGuidFindBox.ResumeLayout(true);
			this.BuyerGuidFindBox.PerformLayout();
			this.SupplierGuidFindBox.ResumeLayout(true);
			this.SupplierGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientsGrid)).EndInit();
			this.ClientsGrid.ResumeLayout(false);
			this.ClientsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ControllingAgentGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ServiceProviderGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox BuyerGuidFindBox;
        private Enterprise.ZArchitecture.GUI.ZGuidFindBox SupplierGuidFindBox;
        private Enterprise.ZArchitecture.ZGrid ClientsGrid;
	}
}
