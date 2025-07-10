namespace Enterprise.Customs.US.GUI
{
	partial class USCRuleUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.RuleDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TariffsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TariffsModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.RuleCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.U0_RuleCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TariffsModuleButtonGrid.InnerGrid)).BeginInit();
			this.TariffsModuleButtonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USCRule);
			// 
			// RuleDescTextBox
			// 
			this.RuleDescTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RuleDescTextBox, "RuleCodeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCRule)(null)).RuleCodeDescription)));
			this.RuleDescTextBox.CaptionResourceString = null;
			this.RuleDescTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RuleDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 26, true);
			this.RuleDescTextBox.Multiline = true;
			this.RuleDescTextBox.Name = "RuleDescTextBox";
			this.RuleDescTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.RuleDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 61, true);
			this.RuleDescTextBox.TabIndex = 2;
			// 
			// TariffsGroupBox
			// 
			this.TariffsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.TariffsGroupBox.Controls.Add(this.TariffsModuleButtonGrid);
			this.TariffsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 93, true);
			this.TariffsGroupBox.Name = "TariffsGroupBox";
			this.TariffsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 226, true);
			this.TariffsGroupBox.TabIndex = 3;
			this.TariffsGroupBox.TabStop = false;
			this.TariffsGroupBox.Text = "Rule applicable to the following Tariffs";
			// 
			// TariffsModuleButtonGrid
			// 
			this.TariffsModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffsModuleButtonGrid, "Tariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.USCRule)(null)).Tariffs)));
			zCodeFindBoxColumnStyleInfo1.Caption = "Tariff From";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "FormattedTariff";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo2.Caption = "Tariff To";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "FormattedTariffTo";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.Tariff;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo1.Caption = "Date From";
			zDateEditColumnStyleInfo1.ColumnName = "U1_DateFrom";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Date To";
			zDateEditColumnStyleInfo2.ColumnName = "U1_DateTo";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TariffsModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TariffsModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TariffsModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TariffsModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TariffsModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TariffsModuleButtonGrid.GridId = "02d8b038-2fcc-4d6a-98c1-a40d16a5d521";
			// 
			// 
			// 
			this.TariffsModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.TariffsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.TariffsModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.TariffsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TariffsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.TariffsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TariffsModuleButtonGrid.InnerGrid.Name = "Grid";
			this.TariffsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 169, true);
			this.TariffsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.TariffsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TariffsModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.US.USCTariffRule;
			this.TariffsModuleButtonGrid.Name = "TariffsModuleButtonGrid";
			this.TariffsModuleButtonGrid.NameOfAGridElement = Enterprise.Customs.US.GUI.Res.GetData("A5EA344E-C4AD-4867-9940-C0225EDC6AA4", "Tariff");
			this.TariffsModuleButtonGrid.ReadOnly = false;
			this.TariffsModuleButtonGrid.ShowAttachButton = false;
			this.TariffsModuleButtonGrid.ShowDetachButton = false;
			this.TariffsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 207, true);
			this.TariffsModuleButtonGrid.TabIndex = 0;
			// 
			// RuleCodeLabel
			// 
			this.RuleCodeLabel.AutoSize = true;
			this.RuleCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RuleCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 8, true);
			this.RuleCodeLabel.Name = "RuleCodeLabel";
			this.RuleCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.RuleCodeLabel.TabIndex = 0;
			this.RuleCodeLabel.Text = "Rule Code:";
			// 
			// U0_RuleCodeLabel
			// 
			this.BindingSource.SetBindingMember(this.U0_RuleCodeLabel, "U0_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCRule)(null)).U0_Code)));
			this.U0_RuleCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.U0_RuleCodeLabel.IsFontBold = true;
			this.U0_RuleCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 7, true);
			this.U0_RuleCodeLabel.Name = "U0_RuleCodeLabel";
			this.U0_RuleCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 15, true);
			this.U0_RuleCodeLabel.TabIndex = 1;
			this.U0_RuleCodeLabel.Text = "AAA";
			// 
			// USCRuleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.U0_RuleCodeLabel);
			this.Controls.Add(this.TariffsGroupBox);
			this.Controls.Add(this.RuleDescTextBox);
			this.Controls.Add(this.RuleCodeLabel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 322, true);
			this.Name = "USCRuleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 322, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffsGroupBox.ResumeLayout(false);
			this.TariffsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TariffsModuleButtonGrid.InnerGrid)).EndInit();
			this.TariffsModuleButtonGrid.ResumeLayout(true);
			this.TariffsModuleButtonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox RuleDescTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TariffsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid TariffsModuleButtonGrid;
		private Enterprise.ZArchitecture.ZLabel RuleCodeLabel;
		private Enterprise.ZArchitecture.ZLabel U0_RuleCodeLabel;
	}
}
