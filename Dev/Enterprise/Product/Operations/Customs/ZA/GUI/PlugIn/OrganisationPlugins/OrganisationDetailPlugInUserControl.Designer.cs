namespace Enterprise.Customs.ZA.GUI
{
	partial class OrganisationDetailPlugInUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.FinancialAccountNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FinancialAccountNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FinancialAccountNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FinancialAccountNumbersGrid)).BeginInit();
			this.FinancialAccountNumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.OrgHeaderWrapper);
			// 
			// FinancialAccountNumbersGroupBox
			// 
			this.FinancialAccountNumbersGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7ED7EB83-86A3-402A-9ACC-A079BBBAE708", "Financial Account Numbers");
			this.FinancialAccountNumbersGroupBox.Controls.Add(this.FinancialAccountNumbersGrid);
			this.FinancialAccountNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FinancialAccountNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FinancialAccountNumbersGroupBox.Name = "FinancialAccountNumbersGroupBox";
			this.FinancialAccountNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 409, true);
			this.FinancialAccountNumbersGroupBox.TabIndex = 0;
			this.FinancialAccountNumbersGroupBox.TabStop = false;
			// 
			// FinancialAccountNumbersGrid
			// 
			this.FinancialAccountNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FinancialAccountNumbersGrid, "FinancialAccountNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.OrgHeaderWrapper)(null)).FinancialAccountNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.OrgHeaderWrapper)(null)).FinancialAccountNumbers)).SyncRoot)).CZ_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCusAccount)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.OrgHeaderWrapper)(null)).FinancialAccountNumbers)).SyncRoot)).CZ_Account)));
			this.FinancialAccountNumbersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("844ffa3c-c8d6-45a1-8ad8-c1a3148422af", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CZ_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("803ff363-d836-4b86-b767-c8e9ae19d9a9", "Number");
			zDropEditColumnStyleInfo2.ColumnName = "CZ_Account";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.FinancialAccountNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FinancialAccountNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.FinancialAccountNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FinancialAccountNumbersGrid.GridId = "12345678-24a4-4bcd-ad5f-ca03875a346c";
			this.FinancialAccountNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FinancialAccountNumbersGrid.LayoutKey = "grid";
			this.FinancialAccountNumbersGrid.LimitedColumns = null;
			this.FinancialAccountNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FinancialAccountNumbersGrid.Name = "FinancialAccountNumbersGrid";
			this.FinancialAccountNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 390, true);
			this.FinancialAccountNumbersGrid.TabIndex = 0;
			// 
			// OrganisationDetailPlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FinancialAccountNumbersGroupBox);
			this.Name = "OrganisationDetailPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 409, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FinancialAccountNumbersGroupBox.ResumeLayout(false);
			this.FinancialAccountNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FinancialAccountNumbersGrid)).EndInit();
			this.FinancialAccountNumbersGrid.ResumeLayout(false);
			this.FinancialAccountNumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox FinancialAccountNumbersGroupBox;
		private ZArchitecture.ZGrid FinancialAccountNumbersGrid;
	}
}
