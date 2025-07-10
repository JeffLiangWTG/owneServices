using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Module
{
	partial class RefJobEquipmentFilterControl
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
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("RefJobEquipmentFilterControl|6989c69c-30db-4a4a-88e4-52280b74833a", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "JEQ_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("RefJobEquipmentFilterControl|500bfa19-3ff2-4c7e-806f-ad8405bf0486", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "JEQ_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("RefJobEquipmentFilterControl|13196513-cd11-4256-98c1-171672cfcc66", "Is Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "JEQ_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.TabIndex = 11;

			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobEquipment);
			// 
			// RefJobEquipmentFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "RefJobEquipmentFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
