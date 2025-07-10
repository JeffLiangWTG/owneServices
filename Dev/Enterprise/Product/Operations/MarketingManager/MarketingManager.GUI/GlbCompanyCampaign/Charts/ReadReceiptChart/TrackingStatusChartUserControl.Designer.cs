
using System.Drawing;

namespace Enterprise.MarketingManager.GUI
{
	partial class TrackingStatusChartUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.labelEmails = new Enterprise.ZArchitecture.ZLabel();
			this.labelOrganizations = new Enterprise.ZArchitecture.ZLabel();
			this.panelChart = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.labelSent = new Enterprise.ZArchitecture.ZLabel();
			this.labelTotalEmails = new Enterprise.ZArchitecture.ZLabel();
			this.labelTotalOrganizations = new Enterprise.ZArchitecture.ZLabel();
			this.panelUnsubscribedRows = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// labelEmails
			// 
			this.labelEmails.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelEmails", "Emails");
			this.labelEmails.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelEmails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 0, true);
			this.labelEmails.Name = "labelEmails";
			this.labelEmails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.labelEmails.TabIndex = 0;
			// 
			// labelOrganizations
			// 
			this.labelOrganizations.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelOrganizations", "Organizations");
			this.labelOrganizations.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.labelOrganizations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 0, true);
			this.labelOrganizations.Name = "labelOrganizations";
			this.labelOrganizations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.labelOrganizations.TabIndex = 1;
			// 
			// panelChart
			// 
			this.panelChart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 20, true);
			this.panelChart.Name = "panelChart";
			this.panelChart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 70, true);
			this.panelChart.TabIndex = 2;
			// 
			// labelSent
			// 
			this.labelSent.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelSent", "Sent");
			this.labelSent.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.labelSent.IsFontBold = true;
			this.labelSent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 90, true);
			this.labelSent.Name = "labelSent";
			this.labelSent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.labelSent.TabIndex = 3;
			// 
			// labelTotalEmails
			// 
			this.labelTotalEmails.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelTotalEmails", "0");
			this.labelTotalEmails.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.labelTotalEmails.IsFontBold = true;
			this.labelTotalEmails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 90, true);
			this.labelTotalEmails.Name = "labelTotalEmails";
			this.labelTotalEmails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.labelTotalEmails.TabIndex = 4;
			// 
			// labelTotalOrganizations
			// 
			this.labelTotalOrganizations.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelTotalOrganizations", "0");
			this.labelTotalOrganizations.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.labelTotalOrganizations.IsFontBold = true;
			this.labelTotalOrganizations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 90, true);
			this.labelTotalOrganizations.Name = "labelTotalOrganizations";
			this.labelTotalOrganizations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.labelTotalOrganizations.TabIndex = 5;
			// 
			// panelUnsubscribedRows
			// 
			this.panelUnsubscribedRows.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 110, true);
			this.panelUnsubscribedRows.Name = "panelUnsubscribedRows";
			this.panelUnsubscribedRows.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 50, true);
			this.panelUnsubscribedRows.TabIndex = 3;
			// 
			// TrackingStatusChartUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.panelUnsubscribedRows);
			this.Controls.Add(this.labelTotalOrganizations);
			this.Controls.Add(this.labelTotalEmails);
			this.Controls.Add(this.labelSent);
			this.Controls.Add(this.panelChart);
			this.Controls.Add(this.labelEmails);
			this.Controls.Add(this.labelOrganizations);
			this.Name = "TrackingStatusChartUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		//private CargoWise.Windows.UI.KElementHost PieChartControlHost;
		private Enterprise.ZArchitecture.ZLabel labelEmails;
		private Enterprise.ZArchitecture.ZLabel labelOrganizations;
		private Enterprise.ZArchitecture.GUI.ZPanel panelChart;
		private Enterprise.ZArchitecture.ZLabel labelSent;
		private ZArchitecture.ZLabel labelTotalEmails;
		private ZArchitecture.ZLabel labelTotalOrganizations;
		//private ZArchitecture.GUI.ZPanel panelDonut;
		private ZArchitecture.GUI.ZPanel panelUnsubscribedRows;
	}
}
