

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsCusContainersWithTrackingUserControl
	{
		private void InitializeComponent()
		{
			this.BaseAllPanel.SuspendLayout();
			this.BaseContainerPanel.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BaseAllPanel
			// 
			this.BaseAllPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 555, true);
			// 
			// BaseContainerPanel
			// 
			this.BaseContainerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseContainerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 555, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 555, true);
			this.ContainersGroupBox.TabIndex = 0;
			// 
			// CusContainersBoundGrid
			// 
			// 
			// 
			// 
			this.CusContainersBoundGrid.InnerGrid.AllowNavigation = false;
			this.CusContainersBoundGrid.InnerGrid.Anchor =
				((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top |
				                                        System.Windows.Forms.AnchorStyles.Bottom)
				                                       | System.Windows.Forms.AnchorStyles.Left)
				                                      | System.Windows.Forms.AnchorStyles.Right)));
			this.CusContainersBoundGrid.InnerGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)))));
			this.CusContainersBoundGrid.InnerGrid.CaptionVisible = false;
			this.CusContainersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusContainersBoundGrid.InnerGrid.LayoutKey = "CusContainersBoundGrid";
			this.CusContainersBoundGrid.InnerGrid.Location =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.CusContainersBoundGrid.InnerGrid.Name = "Grid";
			this.CusContainersBoundGrid.InnerGrid.Size =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 498, true);
			this.CusContainersBoundGrid.InnerGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null))
					.CO_ContainerNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_ContainerNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null))
					.CO_SealInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_Seal)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)).Lookups
					.ContainerTypeCollection)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_RC)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null))
					.CO_RCInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)).Lookups
					.CO_FCL_LCL_NCT_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null))
					.CO_FCL_LCL_AIRInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_FCL_LCL_AIR)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_Weight)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null))
					.CO_WeightInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusContainer)(null)).Lookups
					.WeightUnits)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.BaseCusContainer)(null))
					.CO_WeightUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(
				((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusContainer)(null)).CO_WeightUQ)));
			this.CusContainersBoundGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Containers;
			this.CusContainersBoundGrid.ShowAttachButton = false;
			this.CusContainersBoundGrid.ShowDetachButton = false;
			this.CusContainersBoundGrid.ShowNewButton = false;
			this.CusContainersBoundGrid.Size =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 536, true);
			// 
			// BaseCustomsCusContainersWithTrackingUserControl
			// 
			this.Name = "BaseCustomsCusContainersWithTrackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 555, true);
			this.BaseAllPanel.ResumeLayout(false);
			this.BaseContainerPanel.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
