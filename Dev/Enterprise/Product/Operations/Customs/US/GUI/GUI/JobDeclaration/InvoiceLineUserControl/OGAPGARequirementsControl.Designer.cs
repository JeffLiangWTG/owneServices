namespace Enterprise.Customs.US.GUI
{
	partial class OGAPGARequirementsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.OGARequirementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OGARequirementsGrid)).BeginInit();
			this.OGARequirementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobComInvoiceLine);
			// 
			// OGARequirementsGrid
			// 
			this.OGARequirementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OGARequirementsGrid, "OGAAgencyRequirements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).OGAAgencyRequirements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OGAAgencyRequirement)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).OGAAgencyRequirements)).SyncRoot)).AgencyCodeWithDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OGAAgencyRequirement)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).OGAAgencyRequirements)).SyncRoot)).Requirement)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OGAAgencyRequirement)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).OGAAgencyRequirements)).SyncRoot)).Indicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.OGAAgencyRequirement)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobComInvoiceLine)(null)).OGAAgencyRequirements)).SyncRoot)).DisclaimedReason)));
			this.OGARequirementsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7048CD1D-60D0-473F-9DB0-CE36F6260871", "Agency");
			zTextBoxColumnStyleInfo1.ColumnName = "AgencyCodeWithDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OGAAgencyRequirements|60F2938B-78AF-4245-970C-EE9A727AC67B", "Requirement");
			zTextBoxColumnStyleInfo2.ColumnName = "Requirement";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(335);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OGAAgencyRequirements|08B94B9A-AA3A-4B8E-B0B7-9054848BC8EA", "Indicator");
			zDropEditColumnStyleInfo1.ColumnName = "Indicator";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.Caption = "Disclaim Reason";
			zDropEditColumnStyleInfo2.ColumnName = "DisclaimedReason";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			this.OGARequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OGARequirementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OGARequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OGARequirementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OGARequirementsGrid.CopySelectedRowsAllowed = true;
			this.OGARequirementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OGARequirementsGrid.GridId = "b014f906-8903-45d0-ae37-8a43cf9eaf20";
			this.OGARequirementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OGARequirementsGrid.LayoutKey = "OGARequirementsGrid";
			this.OGARequirementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OGARequirementsGrid.Name = "OGARequirementsGrid";
			this.OGARequirementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 302, true);
			this.OGARequirementsGrid.TabIndex = 2;
			// 
			// OGAPGARequirementsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OGARequirementsGrid);
			this.Name = "OGAPGARequirementsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OGARequirementsGrid)).EndInit();
			this.OGARequirementsGrid.ResumeLayout(false);
			this.OGARequirementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid OGARequirementsGrid;

	}
}
