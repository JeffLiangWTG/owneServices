namespace Enterprise.MasterData.GUI
{
	partial class ComplianceRuleUserControl
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
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ComplianceRuleGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceRuleGrid)).BeginInit();
			this.ComplianceRuleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceRuleCollection);
			// 
			// ComplianceRuleGrid
			// 
			this.ComplianceRuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComplianceRuleGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRule)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRule)(null)).CRU_Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRule)(null)).CRU_Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRule)(null)).CRU_HarmonizedCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceRule)(null)).CRU_RiskStatus)));
			this.ComplianceRuleGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("ba13b043-deff-4b44-8e39-d0b2e814c2b6", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CRU_Origin";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("3a4b7484-d1b1-4056-9991-f3aab6c80d1d", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CRU_Destination";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("043d30f5-1ef7-429b-9576-2f043a300db9", "Harmonized Code");
			tariffColumnStyleInfo1.ColumnName = "CRU_HarmonizedCode";
			tariffColumnStyleInfo1.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo1.TariffType = "HSN";
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("493EC063-8ED4-411A-9666-AD510585B296", "Risk Status");
			zDropEditColumnStyleInfo1.ColumnName = "CRU_RiskStatus";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ComplianceRuleGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ComplianceRuleGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ComplianceRuleGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.ComplianceRuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ComplianceRuleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplianceRuleGrid.GridId = "04663478-5e10-4a9b-a7d8-648a8d8e0102";
			this.ComplianceRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComplianceRuleGrid.LayoutKey = "ComplianceRuleGrid";
			this.ComplianceRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplianceRuleGrid.Name = "ComplianceRuleGrid";
			this.ComplianceRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 322, true);
			this.ComplianceRuleGrid.TabIndex = 1;
			// 
			// ComplianceRuleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComplianceRuleGrid);
			this.Name = "ComplianceRuleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 322, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ComplianceRuleGrid)).EndInit();
			this.ComplianceRuleGrid.ResumeLayout(false);
			this.ComplianceRuleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ComplianceRuleGrid;
	}
}
