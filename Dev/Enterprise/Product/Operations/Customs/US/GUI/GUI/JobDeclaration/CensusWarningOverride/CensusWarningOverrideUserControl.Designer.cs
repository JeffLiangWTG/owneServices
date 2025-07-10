namespace Enterprise.Customs.US.GUI
{
	partial class CensusWarningOverrideUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CWOsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CWOsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.EntryCensusWarningOverrideCollection);
			// 
			// CWOsGrid
			// 
			this.CWOsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CWOsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.EntryCensusWarningOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.EntryCensusWarningOverride)(null)).EntryLinePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EntryCensusWarningOverride)(null)).ConditionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EntryCensusWarningOverride)(null)).OverrideCode)));
			this.CWOsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.Caption = "Entry Line";
			zGuidDropEditColumnStyleInfo1.ColumnName = "EntryLinePK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zDropEditColumnStyleInfo1.Caption = "Condition Code";
			zDropEditColumnStyleInfo1.ColumnName = "ConditionCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDropEditColumnStyleInfo2.Caption = "Override Code";
			zDropEditColumnStyleInfo2.ColumnName = "OverrideCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.CWOsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CWOsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CWOsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CWOsGrid.GridId = "46fbd1f5-1347-41ac-90e2-6f57007aa033";
			this.CWOsGrid.CopySelectedRowsAllowed = true;
			this.CWOsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CWOsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CWOsGrid.LayoutKey = "CWOsGrid";
			this.CWOsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CWOsGrid.Name = "CWOsGrid";
			this.CWOsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 352, true);
			this.CWOsGrid.TabIndex = 0;
			// 
			// CensusWarningOverrideUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CWOsGrid);
			this.Name = "CensusWarningOverrideUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 352, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CWOsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid CWOsGrid;
	}
}
