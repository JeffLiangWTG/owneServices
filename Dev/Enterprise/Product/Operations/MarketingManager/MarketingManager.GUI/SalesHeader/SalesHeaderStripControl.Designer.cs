namespace Enterprise.MarketingManager.GUI
{
	partial class SalesHeaderStripControl
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
			this.mainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.topLeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.deleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.bottomRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.tradeLaneWithDetailsControl = new Enterprise.MarketingManager.GUI.TradeLaneWithDetailsControl();
			this.topRightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.totalsTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.monthlyTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.monthlyTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.monthlyTotalCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.annualTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.annualTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.annualTotalCurrencyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.salesProductNameLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainTableLayoutPanel.SuspendLayout();
			this.topLeftPanel.SuspendLayout();
			this.bottomRightPanel.SuspendLayout();
			this.tradeLaneWithDetailsControl.SuspendLayout();
			this.topRightPanel.SuspendLayout();
			this.totalsTableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SalesHeader);
			// 
			// mainTableLayoutPanel
			// 
			this.mainTableLayoutPanel.AutoSize = true;
			this.mainTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.mainTableLayoutPanel.ColumnCount = 2;
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.mainTableLayoutPanel.Controls.Add(this.topLeftPanel, 0, 0);
			this.mainTableLayoutPanel.Controls.Add(this.bottomRightPanel, 1, 1);
			this.mainTableLayoutPanel.Controls.Add(this.topRightPanel, 1, 0);
			this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
			this.mainTableLayoutPanel.RowCount = 2;
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.mainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 229, true);
			this.mainTableLayoutPanel.TabIndex = 0;
			// 
			// topLeftPanel
			// 
			this.topLeftPanel.Controls.Add(this.deleteButton);
			this.topLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.topLeftPanel.Name = "topLeftPanel";
			this.topLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 23, true);
			this.topLeftPanel.TabIndex = 1;
			// 
			// deleteButton
			// 
			this.deleteButton.BackColor = System.Drawing.Color.Transparent;
			this.deleteButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.deleteButton.FlatAppearance.BorderSize = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.deleteButton, false);
			this.deleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.deleteButton.TabIndex = 0;
			this.deleteButton.TabStop = false;
			this.deleteButton.UseVisualStyleBackColor = false;
			this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// bottomRightPanel
			// 
			this.bottomRightPanel.AutoSize = true;
			this.bottomRightPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.bottomRightPanel.Controls.Add(this.tradeLaneWithDetailsControl);
			this.bottomRightPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.bottomRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 28, true);
			this.bottomRightPanel.Name = "bottomRightPanel";
			this.bottomRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 200, true);
			this.bottomRightPanel.TabIndex = 1;
			// 
			// tradeLaneWithDetailsControl
			// 
			this.tradeLaneWithDetailsControl.AllowDrop = true;
			this.tradeLaneWithDetailsControl.AutoSizeTradeLanesControl = true;
			this.BindingSource.SetBindingMember(this.tradeLaneWithDetailsControl, ".");
			this.tradeLaneWithDetailsControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.tradeLaneWithDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tradeLaneWithDetailsControl.Name = "tradeLaneWithDetailsControl";
			this.tradeLaneWithDetailsControl.ShowAssociatedActivities = false;
			this.tradeLaneWithDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 200, true);
			this.tradeLaneWithDetailsControl.TabIndex = 0;
			// 
			// topRightPanel
			// 
			this.topRightPanel.Controls.Add(this.totalsTableLayoutPanel);
			this.topRightPanel.Controls.Add(this.salesProductNameLabel);
			this.topRightPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 2, true);
			this.topRightPanel.Name = "topRightPanel";
			this.topRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 26, true);
			this.topRightPanel.TabIndex = 0;
			// 
			// totalsTableLayoutPanel
			// 
			this.totalsTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.totalsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.totalsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.totalsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.totalsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.totalsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.totalsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.totalsTableLayoutPanel.Controls.Add(this.monthlyTotalLabel, 0, 0);
			this.totalsTableLayoutPanel.Controls.Add(this.monthlyTotalCalcEdit, 1, 0);
			this.totalsTableLayoutPanel.Controls.Add(this.monthlyTotalCurrencyTextBox, 2, 0);
			this.totalsTableLayoutPanel.Controls.Add(this.annualTotalLabel, 3, 0);
			this.totalsTableLayoutPanel.Controls.Add(this.annualTotalCalcEdit, 4, 0);
			this.totalsTableLayoutPanel.Controls.Add(this.annualTotalCurrencyCodeTextBox, 5, 0);
			this.totalsTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 0, true);
			this.totalsTableLayoutPanel.Name = "totalsTableLayoutPanel";
			this.totalsTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.totalsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.totalsTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(526, 26, true);
			this.totalsTableLayoutPanel.TabIndex = 7;
			// 
			// monthlyTotalLabel
			// 
			this.monthlyTotalLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.monthlyTotalLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ad0ab820-57fb-4d2d-adbe-880810b73f3b", "Monthly Total:");
			this.monthlyTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.monthlyTotalLabel.Name = "monthlyTotalLabel";
			this.monthlyTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 26, true);
			this.monthlyTotalLabel.TabIndex = 1;
			this.monthlyTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// monthlyTotalCalcEdit
			// 
			this.monthlyTotalCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.monthlyTotalCalcEdit, "TotalEstimatedMonthlyAverage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).TotalEstimatedMonthlyAverage)));
			this.monthlyTotalCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.monthlyTotalCalcEdit, false);
			this.monthlyTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 2, true);
			this.monthlyTotalCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.monthlyTotalCalcEdit.Name = "monthlyTotalCalcEdit";
			this.monthlyTotalCalcEdit.ReadOnly = true;
			this.monthlyTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 16, true);
			this.monthlyTotalCalcEdit.TabIndex = 2;
			this.monthlyTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// monthlyTotalCurrencyTextBox
			// 
			this.monthlyTotalCurrencyTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.monthlyTotalCurrencyTextBox, "TotalCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).TotalCurrencyCode)));
			this.monthlyTotalCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 2, true);
			this.monthlyTotalCurrencyTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.monthlyTotalCurrencyTextBox.Name = "monthlyTotalCurrencyTextBox";
			this.monthlyTotalCurrencyTextBox.ReadOnly = true;
			this.monthlyTotalCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 16, true);
			this.monthlyTotalCurrencyTextBox.TabIndex = 3;
			// 
			// annualTotalLabel
			// 
			this.annualTotalLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.annualTotalLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f4ce8cb4-e55f-4e7e-afb1-b90faa25aa6f", "Annual Total:");
			this.annualTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 0, true);
			this.annualTotalLabel.Name = "annualTotalLabel";
			this.annualTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 26, true);
			this.annualTotalLabel.TabIndex = 4;
			this.annualTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// annualTotalCalcEdit
			// 
			this.annualTotalCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.annualTotalCalcEdit, "TotalEstimatedAnnualValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).TotalEstimatedAnnualValue)));
			this.annualTotalCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.annualTotalCalcEdit, false);
			this.annualTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 2, true);
			this.annualTotalCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.annualTotalCalcEdit.Name = "annualTotalCalcEdit";
			this.annualTotalCalcEdit.ReadOnly = true;
			this.annualTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 16, true);
			this.annualTotalCalcEdit.TabIndex = 5;
			this.annualTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// annualTotalCurrencyCodeTextBox
			// 
			this.annualTotalCurrencyCodeTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.annualTotalCurrencyCodeTextBox, "TotalCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).TotalCurrencyCode)));
			this.annualTotalCurrencyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 2, true);
			this.annualTotalCurrencyCodeTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.annualTotalCurrencyCodeTextBox.Name = "annualTotalCurrencyCodeTextBox";
			this.annualTotalCurrencyCodeTextBox.ReadOnly = true;
			this.annualTotalCurrencyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 16, true);
			this.annualTotalCurrencyCodeTextBox.TabIndex = 6;
			// 
			// salesProductNameLabel
			// 
			this.salesProductNameLabel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.salesProductNameLabel, "SalesProduct.MP_NameMultilingual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.MarketingManager.Business.SalesHeader)(null)).SalesProduct.MP_NameMultilingual)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.salesProductNameLabel, false);
			this.salesProductNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.salesProductNameLabel.Name = "salesProductNameLabel";
			this.salesProductNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 26, true);
			this.salesProductNameLabel.TabIndex = 0;
			this.salesProductNameLabel.UseMnemonic = false;
			// 
			// SalesHeaderStripControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainTableLayoutPanel);
			this.Name = "SalesHeaderStripControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 2, 2, 15, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 246, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainTableLayoutPanel.ResumeLayout(false);
			this.mainTableLayoutPanel.PerformLayout();
			this.topLeftPanel.ResumeLayout(false);
			this.topLeftPanel.PerformLayout();
			this.bottomRightPanel.ResumeLayout(false);
			this.bottomRightPanel.PerformLayout();
			this.tradeLaneWithDetailsControl.ResumeLayout(true);
			this.tradeLaneWithDetailsControl.PerformLayout();
			this.topRightPanel.ResumeLayout(false);
			this.topRightPanel.PerformLayout();
			this.totalsTableLayoutPanel.ResumeLayout(false);
			this.totalsTableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel mainTableLayoutPanel;
		private Enterprise.ZArchitecture.ZLabel salesProductNameLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomRightPanel;
		private ZArchitecture.GUI.ZButton deleteButton;
		private ZArchitecture.GUI.ZPanel topRightPanel;
		private ZArchitecture.ZCalcEdit monthlyTotalCalcEdit;
		private ZArchitecture.ZTextBox monthlyTotalCurrencyTextBox;
		private ZArchitecture.ZTextBox annualTotalCurrencyCodeTextBox;
		private ZArchitecture.ZCalcEdit annualTotalCalcEdit;
		private ZArchitecture.GUI.ZPanel topLeftPanel;
		private TradeLaneWithDetailsControl tradeLaneWithDetailsControl;
		private ZArchitecture.ZLabel monthlyTotalLabel;
		private ZArchitecture.ZLabel annualTotalLabel;
		private CargoWise.Windows.UI.KTableLayoutPanel totalsTableLayoutPanel;

	}
}
