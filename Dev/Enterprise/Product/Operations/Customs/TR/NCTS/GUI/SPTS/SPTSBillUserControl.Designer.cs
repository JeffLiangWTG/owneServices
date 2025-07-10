namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class SPTSBillUserControl
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
			this.BillsGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).BeginInit();
			this.BillsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.SPTSBill);
			// 
			// BillsGrid
			// 
			this.BillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillsGrid, "Bills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Bills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSBill)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Bills)).SyncRoot)).B0_MasterBillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSBill)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Bills)).SyncRoot)).B0_ReferenceQualifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSBill)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Bills)).SyncRoot)).B0_ReferenceID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSBill)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).Bills)).SyncRoot)).B0_ServiceType)));
			this.BillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "B0_MasterBillNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "B0_ReferenceQualifier";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.BillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "B0_ReferenceID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.BillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "B0_ServiceType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsGrid.GridId = "2E84DBE7-5E55-4AC7-990B-494CA59AF1A3";
			this.BillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillsGrid.LayoutKey = "BillsGrid";
			this.BillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillsGrid.Name = "BillsGrid";
			this.BillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 200, true);
			this.BillsGrid.TabIndex = 1;
			// 
			// SPTSBillUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BillsGrid);
			this.Name = "SPTSBillUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BillsGrid)).EndInit();
			this.BillsGrid.ResumeLayout(false);
			this.BillsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid BillsGrid;
	}
}
