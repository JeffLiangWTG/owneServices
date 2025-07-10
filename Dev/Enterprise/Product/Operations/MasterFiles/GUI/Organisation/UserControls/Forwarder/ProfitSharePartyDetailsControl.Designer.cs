namespace Enterprise.MasterFiles.GUI
{
	partial class ProfitSharePartyDetailsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.generalPartyDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.gatewayConsolProfitRedistributionPartyDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.gatewayConsolProfitRedistributionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.generalTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.generalPartyDetailsGrid)).BeginInit();
			this.generalPartyDetailsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gatewayConsolProfitRedistributionPartyDetailsGrid)).BeginInit();
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.SuspendLayout();
			this.gatewayConsolProfitRedistributionTabPage.SuspendLayout();
			this.generalTabPage.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgProfitSharePartyCollection);
			// 
			// generalPartyDetailsGrid
			// 
			this.generalPartyDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.generalPartyDetailsGrid, "PartyDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PS_PartyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PartyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PS_PartyProfitSharePercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PS_PartyRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PS_PartyRateBasis)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PS_PartyMinimum)));
			this.generalPartyDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "PS_PartyType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareAgreementControl|1a299276-8d42-4972-a39a-d9004430aca7", "Party Description");
			zTextBoxColumnStyleInfo1.ColumnName = "PartyTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PS_PartyProfitSharePercent";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "PS_PartyRate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "PS_PartyRateBasis";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "PS_PartyMinimum";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.generalPartyDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.generalPartyDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.generalPartyDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.generalPartyDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.generalPartyDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.generalPartyDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.generalPartyDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.generalPartyDetailsGrid.GridId = "007587da-d768-4104-b056-011a58c7ab15";
			this.generalPartyDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.generalPartyDetailsGrid.LayoutKey = "GeneralPartyDetailsGrid";
			this.generalPartyDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.generalPartyDetailsGrid.Name = "generalPartyDetailsGrid";
			this.generalPartyDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 146, true);
			this.generalPartyDetailsGrid.TabIndex = 0;
			// 
			// gatewayConsolProfitRedistributionPartyDetailsGrid
			// 
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gatewayConsolProfitRedistributionPartyDetailsGrid, "PartyDetailsForGatewayProfitShareRedistribution");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PS_PartyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PartyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgProfitShareParty)(null)).PS_PartyProfitSharePercent)));
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "PS_PartyType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareAgreementControl|1a299276-8d42-4972-a39a-d9004430aca7", "Party Description");
			zTextBoxColumnStyleInfo2.ColumnName = "PartyTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "PS_PartyProfitSharePercent";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.GridId = "99f92bf4-7602-4296-a2fa-620b2e622d13";
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.LayoutKey = "GatewayConsolProfitRedistributionPartyDetailsGrid";
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.Name = "gatewayConsolProfitRedistributionPartyDetailsGrid";
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 146, true);
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.TabIndex = 0;
			// 
			// gatewayConsolProfitRedistributionTabPage
			// 
			this.gatewayConsolProfitRedistributionTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareAgreementControl|588fa8da-59da-4f3d-816f-8847be12f57d", "G/W Consol Profit Redistribution");
			this.gatewayConsolProfitRedistributionTabPage.Controls.Add(this.gatewayConsolProfitRedistributionPartyDetailsGrid);
			this.gatewayConsolProfitRedistributionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.gatewayConsolProfitRedistributionTabPage.Name = "gatewayConsolProfitRedistributionTabPage";
			this.gatewayConsolProfitRedistributionTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.gatewayConsolProfitRedistributionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 150, true);
			this.gatewayConsolProfitRedistributionTabPage.TabIndex = 1;
			this.gatewayConsolProfitRedistributionTabPage.UseVisualStyleBackColor = true;
			// 
			// generalTabPage
			// 
			this.generalTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProfitShareAgreementControl|84cfb363-bc58-4b69-96e4-f1f570eb7c1d", "General");
			this.generalTabPage.Controls.Add(this.generalPartyDetailsGrid);
			this.generalTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.generalTabPage.Name = "generalTabPage";
			this.generalTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.generalTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 150, true);
			this.generalTabPage.TabIndex = 0;
			this.generalTabPage.UseVisualStyleBackColor = true;
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControl.Controls.Add(this.generalTabPage);
			this.tabControl.Controls.Add(this.gatewayConsolProfitRedistributionTabPage);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 172, true);
			this.tabControl.TabIndex = 0;
			// 
			// ProfitSharePartyDetailsControl
			// 
			this.Controls.Add(this.tabControl);
			this.Name = "ProfitSharePartyDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 172, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.generalPartyDetailsGrid)).EndInit();
			this.generalPartyDetailsGrid.ResumeLayout(false);
			this.generalPartyDetailsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gatewayConsolProfitRedistributionPartyDetailsGrid)).EndInit();
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.ResumeLayout(false);
			this.gatewayConsolProfitRedistributionPartyDetailsGrid.PerformLayout();
			this.gatewayConsolProfitRedistributionTabPage.ResumeLayout(false);
			this.gatewayConsolProfitRedistributionTabPage.PerformLayout();
			this.generalTabPage.ResumeLayout(false);
			this.generalTabPage.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZTabControl tabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage generalTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage gatewayConsolProfitRedistributionTabPage;
		Enterprise.ZArchitecture.ZGrid generalPartyDetailsGrid;
		Enterprise.ZArchitecture.ZGrid gatewayConsolProfitRedistributionPartyDetailsGrid;
	}
}
