using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsCusContainersUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.BaseAllPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BaseContainerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContainersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CusContainersBoundGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.BaseAllPanel.SuspendLayout();
			this.BaseContainerPanel.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BaseAllPanel
			// 
			this.BaseAllPanel.Controls.Add(this.BaseContainerPanel);
			this.BaseAllPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseAllPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BaseAllPanel.Name = "BaseAllPanel";
			this.BaseAllPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 550, true);
			this.BaseAllPanel.TabIndex = 6;
			// 
			// BaseContainerPanel
			// 
			this.BaseContainerPanel.Controls.Add(this.ContainersGroupBox);
			this.BaseContainerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BaseContainerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BaseContainerPanel.Name = "BaseContainerPanel";
			this.BaseContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 584, true);
			this.BaseContainerPanel.TabIndex = 6;
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Controls.Add(this.CusContainersBoundGrid);
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 584, true);
			this.ContainersGroupBox.TabIndex = 5;
			this.ContainersGroupBox.TabStop = false;
			this.ContainersGroupBox.CaptionResourceString = Res.GetData("A85552A6-745D-4E67-8FE9-7414D09B6442", " Containers");
			// 
			// CusContainersBoundGrid
			// 
			this.CusContainersBoundGrid.BindToGridList = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)))));
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CO_ContainerNumber";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CO_Seal";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ContainerTypeCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("F8131AF2-198C-49CA-A478-6CD1CC5BCE85", "Type");
			zGuidFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CO_RC";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefContainer;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.CO_FCL_LCL_NCT_List";
			zDropEditColumnStyleInfo1.CaptionResourceString = Res.GetData("DDD6B021-2258-4AC1-B0B7-EF2321B53651", "Mode");
			zDropEditColumnStyleInfo1.ColumnName = "CO_FCL_LCL_AIR";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("00AB06D7-0FB3-4A57-BA56-F9DE1428AD50", "Goods Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "CO_Weight";
			zCalcEditColumnStyleInfo1.GroupName = Res.GetData("BaseCusteromCusContainers|d854dd6b-178b-473a-a04a-d80ac40dbc99", "Goods Weight");
			zCalcEditColumnStyleInfo1.ToolTip = "Weight in tonnes";
			zDropEditColumnStyleInfo2.BindToList = "Lookups.WeightUnits";
			zDropEditColumnStyleInfo2.CaptionResourceString = Res.GetData("A1D8C7B8-6CA3-4D9D-9F61-45A881C32C63", "UQ");
			zDropEditColumnStyleInfo2.ColumnName = "CO_WeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Res.GetData("BaseCusteromCusContainers|d854dd6b-178b-473a-a04a-d80ac40dbc99", "Goods Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CO_SecondSeal";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CO_RN_NKOwnerCountry";
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.CountryList";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("BaseCusteromCusContainers|0e33a048-05a9-495f-94da-a23894068290", "Owner Country");
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.CusContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CusContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CusContainersBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CusContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CusContainersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CusContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CusContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CusContainersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CusContainersBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.CusContainersBoundGrid.InnerGrid.AllowNavigation = false;
			this.CusContainersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CusContainersBoundGrid.InnerGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)))));
			this.CusContainersBoundGrid.InnerGrid.CaptionVisible = false;
			this.CusContainersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusContainersBoundGrid.InnerGrid.LayoutKey = "CusContainersBoundGrid";
			this.CusContainersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.CusContainersBoundGrid.InnerGrid.Name = "Grid";
			this.CusContainersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 527, true);
			this.CusContainersBoundGrid.InnerGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_ContainerNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_ContainerNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_SealInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_Seal)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_RC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_RCInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)).Lookups.ContainerTypeCollection)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_FCL_LCL_AIRInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_FCL_LCL_AIR)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)).Lookups.CO_FCL_LCL_NCT_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_WeightInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_WeightUQ)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)).Lookups.WeightUnits)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_SecondSealInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_SecondSeal)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_RN_NKOwnerCountry)));
			this.CusContainersBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CusContainersBoundGrid.Name = "CusContainersBoundGrid";
			this.CusContainersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 565, true);
			this.CusContainersBoundGrid.TabIndex = 0;
			// 
			// BaseCustomsCusContainersUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BaseAllPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.BaseCusContainer";
			this.Name = "BaseCustomsCusContainersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 550, true);
			this.BaseAllPanel.ResumeLayout(false);
			this.BaseContainerPanel.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
