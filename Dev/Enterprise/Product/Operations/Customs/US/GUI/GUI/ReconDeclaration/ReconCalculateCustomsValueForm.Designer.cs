namespace Enterprise.Customs.US.GUI
{
	partial class ReconCalculateCustomsValueForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CalculationTypePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CalculationTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CalculationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PercentagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PercentageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PercentageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentageSymbolLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.CalculationTypePanel.SuspendLayout();
			this.CalculationTypeDropEdit.SuspendLayout();
			this.PercentagePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 112, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ReconCustomsValueCalculationManager);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Controls.Add(this.CancelButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 87, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 25, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 0, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 6;
			this.OKButton.Text = "&OK";
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 0, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 7;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// CalculationTypePanel
			// 
			this.CalculationTypePanel.Controls.Add(this.CalculationTypeLabel);
			this.CalculationTypePanel.Controls.Add(this.CalculationTypeDropEdit);
			this.CalculationTypePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CalculationTypePanel.Name = "CalculationTypePanel";
			this.CalculationTypePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 40, true);
			this.CalculationTypePanel.TabIndex = 1;
			// 
			// CalculationTypeLabel
			// 
			this.CalculationTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CalculationTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 13, true);
			this.CalculationTypeLabel.Name = "CalculationTypeLabel";
			this.CalculationTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 18, true);
			this.CalculationTypeLabel.TabIndex = 0;
			this.CalculationTypeLabel.Text = "Calculation Type:";
			this.CalculationTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CalculationTypeDropEdit
			// 
			this.CalculationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CalculationTypeDropEdit, "RecalculationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconCustomsValueCalculationManager)(null)).RecalculationType)));
			this.CalculationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 15, true);
			this.CalculationTypeDropEdit.Name = "CalculationTypeDropEdit";
			this.CalculationTypeDropEdit.PreBoundMaxLength = 1;
			this.CalculationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 18, true);
			this.CalculationTypeDropEdit.TabIndex = 1;
			// 
			// PercentagePanel
			// 
			this.PercentagePanel.Controls.Add(this.PercentageLabel);
			this.PercentagePanel.Controls.Add(this.PercentageCalcEdit);
			this.PercentagePanel.Controls.Add(this.PercentageSymbolLabel);
			this.PercentagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.PercentagePanel.Name = "PercentagePanel";
			this.PercentagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 40, true);
			this.PercentagePanel.TabIndex = 2;
			// 
			// PercentageLabel
			// 
			this.PercentageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PercentageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 8, true);
			this.PercentageLabel.Name = "PercentageLabel";
			this.PercentageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 18, true);
			this.PercentageLabel.TabIndex = 0;
			this.PercentageLabel.Text = "Percentage:";
			this.PercentageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PercentageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PercentageCalcEdit, "RecalculationPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconCustomsValueCalculationManager)(null)).RecalculationPercentage)));
			this.PercentageCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("D93E087F-F209-4A51-8465-975CC65D0052", "Percentage");
			this.PercentageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 9, true);
			this.PercentageCalcEdit.Name = "PercentageCalcEdit";
			this.PercentageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 18, true);
			this.PercentageCalcEdit.DecimalPlaces = 2;
			this.PercentageCalcEdit.MaxValue = 100;
			this.PercentageCalcEdit.TabIndex = 2;
			// 
			// PercentageSymbolLabel
			// 
			this.PercentageSymbolLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PercentageSymbolLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 9, true);
			this.PercentageSymbolLabel.Name = "PercentageSymbolLabel";
			this.PercentageSymbolLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 18, true);
			this.PercentageSymbolLabel.Text = "%";
			// 
			// ReconCalculateCustomsValueForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 136, true);
			this.Controls.Add(this.PercentagePanel);
			this.Controls.Add(this.CalculationTypePanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.ReconCustomsValueCalculationManager);
			this.Name = "ReconCalculateCustomsValueForm";
			this.Text = "Calculate Recon Customs Values";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.CalculationTypePanel, 0);
			this.Controls.SetChildIndex(this.PercentagePanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.CalculationTypePanel.ResumeLayout(false);
			this.CalculationTypePanel.PerformLayout();
			this.CalculationTypeDropEdit.ResumeLayout(true);
			this.CalculationTypeDropEdit.PerformLayout();
			this.PercentagePanel.ResumeLayout(false);
			this.PercentagePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton OKButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZPanel CalculationTypePanel;
		private ZArchitecture.GUI.ZDropEdit CalculationTypeDropEdit;
		private ZArchitecture.ZLabel CalculationTypeLabel;
		private ZArchitecture.GUI.ZPanel PercentagePanel;
		private ZArchitecture.ZCalcEdit PercentageCalcEdit;
		private ZArchitecture.ZLabel PercentageLabel;
		private ZArchitecture.ZLabel PercentageSymbolLabel;
	}
}