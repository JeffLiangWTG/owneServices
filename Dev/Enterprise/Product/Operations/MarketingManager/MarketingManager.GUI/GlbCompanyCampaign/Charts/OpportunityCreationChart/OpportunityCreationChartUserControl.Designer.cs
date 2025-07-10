using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using System.ComponentModel;
using System.Windows.Forms;

namespace Enterprise.MarketingManager.GUI
{
	partial class OpportunityCreationChartUserControl
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
			this.labelCurrentRect = new Enterprise.ZArchitecture.ZLabel();
			this.labelCurrent = new Enterprise.ZArchitecture.ZLabel();
			this.labelCurrentTotal = new Enterprise.ZArchitecture.ZLabel();
			this.labelWonTotal = new Enterprise.ZArchitecture.ZLabel();
			this.labelWon = new Enterprise.ZArchitecture.ZLabel();
			this.labelWonRect = new Enterprise.ZArchitecture.ZLabel();
			this.labelLostTotal = new Enterprise.ZArchitecture.ZLabel();
			this.labelLost = new Enterprise.ZArchitecture.ZLabel();
			this.labelLostRect = new Enterprise.ZArchitecture.ZLabel();
			this.labelOtherTotal = new Enterprise.ZArchitecture.ZLabel();
			this.labelOther = new Enterprise.ZArchitecture.ZLabel();
			this.labelOtherRect = new Enterprise.ZArchitecture.ZLabel();
			this.labelTotal = new Enterprise.ZArchitecture.ZLabel();
			this.labelWinRatio = new Enterprise.ZArchitecture.ZLabel();
			this.labelTotalTotal = new Enterprise.ZArchitecture.ZLabel();
			this.labelWinRatioTotal = new Enterprise.ZArchitecture.ZLabel();
			this.labelOpportunities = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// labelCurrentRect
			// 
			this.labelCurrentRect.BackColor = System.Drawing.Color.Orange;
			this.labelCurrentRect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 20, true);
			this.labelCurrentRect.Name = "labelCurrentRect";
			this.labelCurrentRect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 12, true);
			this.labelCurrentRect.TabIndex = 1;
			this.labelCurrentRect.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelCurrentRect", " ");
			// 
			// labelCurrent
			// 
			this.labelCurrent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 20, true);
			this.labelCurrent.Name = "labelCurrent";
			this.labelCurrent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelCurrent.TabIndex = 2;
			this.labelCurrent.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelCurrent", "Current");
			// 
			// labelCurrentTotal
			// 
			this.labelCurrentTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 20, true);
			this.labelCurrentTotal.Name = "labelCurrentTotal";
			this.labelCurrentTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelCurrentTotal.TabIndex = 3;
			this.labelCurrentTotal.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelCurrentTotal", "0");
			// 
			// labelWonTotal
			// 
			this.labelWonTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 38, true);
			this.labelWonTotal.Name = "labelWonTotal";
			this.labelWonTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelWonTotal.TabIndex = 6;
			this.labelWonTotal.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelWonTotal", "0");

			// 
			// labelWon
			// 
			this.labelWon.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 38, true);
			this.labelWon.Name = "labelWon";
			this.labelWon.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelWon.TabIndex = 5;
			this.labelWon.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelWon", "Won");
			// 
			// labelWonRect
			// 
			this.labelWonRect.BackColor = System.Drawing.Color.Green;
			this.labelWonRect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 38, true);
			this.labelWonRect.Name = "labelWonRect";
			this.labelWonRect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 12, true);
			this.labelWonRect.TabIndex = 4;
			this.labelWonRect.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelWonRect", " ");
			// 
			// labelLostTotal
			// 
			this.labelLostTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 56, true);
			this.labelLostTotal.Name = "labelLostTotal";
			this.labelLostTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelLostTotal.TabIndex = 9;
			this.labelLostTotal.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelLostTotal", "0");
			// 
			// labelLost
			// 
			this.labelLost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 56, true);
			this.labelLost.Name = "labelLost";
			this.labelLost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelLost.TabIndex = 8;
			this.labelLost.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelLost", "Lost");
			// 
			// labelLostRect
			// 
			this.labelLostRect.BackColor = System.Drawing.Color.Red;
			this.labelLostRect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 56, true);
			this.labelLostRect.Name = "labelLostRect";
			this.labelLostRect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 12, true);
			this.labelLostRect.TabIndex = 7;
			this.labelLostRect.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelLostRect", " ");
			// 
			// labelOtherTotal
			// 
			this.labelOtherTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 74, true);
			this.labelOtherTotal.Name = "labelOtherTotal";
			this.labelOtherTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelOtherTotal.TabIndex = 12;
			this.labelOtherTotal.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelOtherTotal", "0");
			// 
			// labelOther
			// 
			this.labelOther.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 74, true);
			this.labelOther.Name = "labelOther";
			this.labelOther.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelOther.TabIndex = 11;
			this.labelOther.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelOther", "Other");
			// 
			// labelOtherRect
			// 
			this.labelOtherRect.BackColor = System.Drawing.Color.Black;
			this.labelOtherRect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 74, true);
			this.labelOtherRect.Name = "labelOtherRect";
			this.labelOtherRect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 12, true);
			this.labelOtherRect.TabIndex = 10;
			this.labelOtherRect.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelOtherRect", " ");
			// 
			// labelTotal
			// 
			this.labelTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 92, true);
			this.labelTotal.Name = "labelTotal";
			this.labelTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelTotal.TabIndex = 13;
			this.labelTotal.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelTotal", "Total");
			// 
			// labelWinRatio
			// 
			this.labelWinRatio.ForeColor = System.Drawing.Color.Green;
			this.labelWinRatio.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 110, true);
			this.labelWinRatio.Name = "labelWinRatio";
			this.labelWinRatio.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 12, true);
			this.labelWinRatio.TabIndex = 14;
			this.labelWinRatio.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelWinRatio", "Win Ratio");
			// 
			// labelTotalTotal
			// 
			this.labelTotalTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 92, true);
			this.labelTotalTotal.Name = "labelTotalTotal";
			this.labelTotalTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelTotalTotal.TabIndex = 15;
			this.labelTotalTotal.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelTotalTotal", "0");
			// 
			// labelWinRatioTotal
			// 
			this.labelWinRatioTotal.ForeColor = System.Drawing.Color.Green;
			this.labelWinRatioTotal.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 110, true);
			this.labelWinRatioTotal.Name = "labelWinRatioTotal";
			this.labelWinRatioTotal.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 12, true);
			this.labelWinRatioTotal.TabIndex = 16;
			this.labelWinRatioTotal.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelWinRatioTotal", "0");
			// 
			// labelOpportunities
			// 
			this.labelOpportunities.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 2, true);
			this.labelOpportunities.Name = "labelOpportunities";
			this.labelOpportunities.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.labelOpportunities.TabIndex = 17;
			this.labelOpportunities.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("labelOpportunities", "Opportunities");
			// 
			// OpportunityCreationChartUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.labelOpportunities);
			this.Controls.Add(this.labelWinRatioTotal);
			this.Controls.Add(this.labelTotalTotal);
			this.Controls.Add(this.labelWinRatio);
			this.Controls.Add(this.labelTotal);
			this.Controls.Add(this.labelOtherTotal);
			this.Controls.Add(this.labelOther);
			this.Controls.Add(this.labelOtherRect);
			this.Controls.Add(this.labelLostTotal);
			this.Controls.Add(this.labelLost);
			this.Controls.Add(this.labelLostRect);
			this.Controls.Add(this.labelWonTotal);
			this.Controls.Add(this.labelWon);
			this.Controls.Add(this.labelWonRect);
			this.Controls.Add(this.labelCurrentTotal);
			this.Controls.Add(this.labelCurrent);
			this.Controls.Add(this.labelCurrentRect);
			this.Name = "OpportunityCreationChartUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private Enterprise.ZArchitecture.ZLabel labelCurrentRect;
		private Enterprise.ZArchitecture.ZLabel labelCurrent;
		private Enterprise.ZArchitecture.ZLabel labelCurrentTotal;
		private Enterprise.ZArchitecture.ZLabel labelWonTotal;
		private Enterprise.ZArchitecture.ZLabel labelWon;
		private Enterprise.ZArchitecture.ZLabel labelWonRect;
		private Enterprise.ZArchitecture.ZLabel labelLostTotal;
		private Enterprise.ZArchitecture.ZLabel labelLost;
		private Enterprise.ZArchitecture.ZLabel labelLostRect;
		private Enterprise.ZArchitecture.ZLabel labelOtherTotal;
		private Enterprise.ZArchitecture.ZLabel labelOther;
		private Enterprise.ZArchitecture.ZLabel labelOtherRect;
		private Enterprise.ZArchitecture.ZLabel labelTotal;
		private Enterprise.ZArchitecture.ZLabel labelWinRatio;
		private Enterprise.ZArchitecture.ZLabel labelTotalTotal;
		private Enterprise.ZArchitecture.ZLabel labelWinRatioTotal;
		private Enterprise.ZArchitecture.ZLabel labelOpportunities;

		#endregion

		//private KElementHost PieChartControlHost;
	}
}
