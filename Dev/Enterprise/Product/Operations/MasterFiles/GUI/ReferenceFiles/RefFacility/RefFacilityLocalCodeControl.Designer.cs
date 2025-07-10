namespace Enterprise.MasterFiles.GUI
{
	partial class RefFacilityLocalCodeControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LocalCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LocalCodesGrid)).BeginInit();
			this.LocalCodesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefFacility);
			// 
			// MessagingRequirementsGrid
			// 
			this.LocalCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LocalCodesGrid, "RefFacilityLocalCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RefFacilityLocalCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacilityLocalCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RefFacilityLocalCodes)).SyncRoot)).RFL_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacilityLocalCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RefFacilityLocalCodes)).SyncRoot)).RFL_Usage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacilityLocalCode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RefFacilityLocalCodes)).SyncRoot)).RFL_Code)));
			this.LocalCodesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefFacilityLocalCodes|3CF68B7B-01AE-44A1-BCC4-447645E881D5", "Country");
			zTextBoxColumnStyleInfo1.ColumnName = "RFL_RN_NKCountryCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefFacilityLocalCodes|3CF68B7B-01AE-44A1-BCC4-447645E881D2", "Usage");
			zTextBoxColumnStyleInfo2.ColumnName = "RFL_Usage";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefFacilityLocalCodes|3CF68B7B-01AE-44A1-BCC4-447645E88142", "Code");
			zTextBoxColumnStyleInfo3.ColumnName = "RFL_Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.LocalCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LocalCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LocalCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LocalCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocalCodesGrid.GridId = "68aa73b0-76cc-46d1-8786-e3d4b5685411";
			this.LocalCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocalCodesGrid.LayoutKey = "LocalCodesGrid";
			this.LocalCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocalCodesGrid.Name = "LocalCodesGrid";
			this.LocalCodesGrid.ReadOnly = true;
			this.LocalCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 81, true);
			this.LocalCodesGrid.TabIndex = 0;
			// 
			// LocalCodesGridControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocalCodesGrid);
			this.Name = "RefFacilityControlControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 81, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LocalCodesGrid)).EndInit();
			this.LocalCodesGrid.ResumeLayout(false);
			this.LocalCodesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZGrid LocalCodesGrid;

		#endregion
	}
}
