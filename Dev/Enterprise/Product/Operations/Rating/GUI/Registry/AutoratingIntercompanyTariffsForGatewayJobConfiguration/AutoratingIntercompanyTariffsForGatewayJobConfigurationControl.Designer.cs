using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Registry
{
	public partial class AutoratingIntercompanyTariffsForGatewayJobConfigurationControl
	{
		void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)BindingSource).BeginInit();
			((System.ComponentModel.ISupportInitialize)AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid).BeginInit();
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AutoratingIntercompanyTariffsForGatewayJobConfigurationCollection);
			// 
			// GatewayChargeDefaultDebtorConfigurationGrid
			// 
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid, ".");
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = nameof(AutoratingIntercompanyTariffsForGatewayJobConfiguration.LoginRole);
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.ColumnName = nameof(AutoratingIntercompanyTariffsForGatewayJobConfiguration.LoginAgentRole);
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = nameof(AutoratingIntercompanyTariffsForGatewayJobConfiguration.ShipmentDirection);
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo4.ColumnName = nameof(AutoratingIntercompanyTariffsForGatewayJobConfiguration.AutoratingJob);
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo5.ColumnName = nameof(AutoratingIntercompanyTariffsForGatewayJobConfiguration.AutoratingRule);
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo6.ColumnName = nameof(AutoratingIntercompanyTariffsForGatewayJobConfiguration.ICTServiceProvider);
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.CopySelectedRowsAllowed = true;
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.GridId = "ff604b9e-cee3-446d-934b-36fea87b6ea9";
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.LayoutKey = "AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid";
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.Name = "AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid";
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.TabIndex = 0;
			// 
			// AutoratingIntercompanyTariffsForGatewayJobConfigurationControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid);
			this.Name = "AutoratingIntercompanyTariffsForGatewayJobConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid)).EndInit();
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.ResumeLayout(false);
			this.AutoratingIntercompanyTariffsForGatewayJobConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
