namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class ReconInterestRatesControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ReconInterestRatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReconInterestRateGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReconInterestRateLabelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReconInterestRateLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReconInterestRatePanel.SuspendLayout();
			this.ReconInterestRateGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).BeginInit();
			this.RatesGrid.SuspendLayout();
			this.ReconInterestRateLabelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.ReconInterestRateCollection);
			// 
			// ReconInterestRatePanel
			// 
			this.ReconInterestRatePanel.Controls.Add(this.ReconInterestRateGridPanel);
			this.ReconInterestRatePanel.Controls.Add(this.ReconInterestRateLabelPanel);
			this.ReconInterestRatePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReconInterestRatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReconInterestRatePanel.Name = "ReconInterestRatePanel";
			this.ReconInterestRatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 389, true);
			this.ReconInterestRatePanel.TabIndex = 0;
			// 
			// ReconInterestRateGridPanel
			// 
			this.ReconInterestRateGridPanel.Controls.Add(this.RatesGrid);
			this.ReconInterestRateGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReconInterestRateGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.ReconInterestRateGridPanel.Name = "ReconInterestRateGridPanel";
			this.ReconInterestRateGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 359, true);
			this.ReconInterestRateGridPanel.TabIndex = 2;
			// 
			// RatesGrid
			// 
			this.RatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RatesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.DataRegistry.Business.ReconInterestRate)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.DataRegistry.Business.ReconInterestRate)(null)).StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.DataRegistry.Business.ReconInterestRate)(null)).EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.DataRegistry.Business.ReconInterestRate)(null)).Rate)));
			this.RatesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconInterestRatesControl|0e888523-a9a6-46f9-9458-2a6f915fa894", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconInterestRatesControl|0713a3b1-becc-43fc-8e9e-43d755aba657", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconInterestRatesControl|4190f6a1-e9f3-479a-97a9-e2447595a420", "Rate");
			zCalcEditColumnStyleInfo1.ColumnName = "Rate";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RatesGrid.GridId = "446f6181-ed1f-4210-831d-b2bf1fd7a009";
			this.RatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RatesGrid.LayoutKey = "RatesGrid";
			this.RatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RatesGrid.Name = "RatesGrid";
			this.RatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 359, true);
			this.RatesGrid.TabIndex = 0;
			// 
			// ReconInterestRateLabelPanel
			// 
			this.ReconInterestRateLabelPanel.Controls.Add(this.ReconInterestRateLabel);
			this.ReconInterestRateLabelPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ReconInterestRateLabelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReconInterestRateLabelPanel.Name = "ReconInterestRateLabelPanel";
			this.ReconInterestRateLabelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 30, true);
			this.ReconInterestRateLabelPanel.TabIndex = 1;
			// 
			// ReconInterestRateLabel
			// 
			this.ReconInterestRateLabel.AutoSize = true;
			this.ReconInterestRateLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconInterestRatesControl|c8787c86-1aaa-406a-8cd7-48b458c71207", "Interest Rates");
			this.ReconInterestRateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ReconInterestRateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 10, true);
			this.ReconInterestRateLabel.Name = "ReconInterestRateLabel";
			this.ReconInterestRateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
			this.ReconInterestRateLabel.TabIndex = 1;
			this.ReconInterestRateLabel.UseMnemonic = false;
			// 
			// ReconInterestRatesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReconInterestRatePanel);
			this.Name = "ReconInterestRatesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 389, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReconInterestRatePanel.ResumeLayout(false);
			this.ReconInterestRatePanel.PerformLayout();
			this.ReconInterestRateGridPanel.ResumeLayout(false);
			this.ReconInterestRateGridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).EndInit();
			this.RatesGrid.ResumeLayout(false);
			this.RatesGrid.PerformLayout();
			this.ReconInterestRateLabelPanel.ResumeLayout(false);
			this.ReconInterestRateLabelPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel ReconInterestRatePanel;
		private Enterprise.ZArchitecture.GUI.ZPanel ReconInterestRateGridPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel ReconInterestRateLabelPanel;
		private Enterprise.ZArchitecture.ZLabel ReconInterestRateLabel;
		internal Enterprise.ZArchitecture.ZGrid RatesGrid;
	}
}
