namespace Enterprise.MasterFiles.Module
{
	partial class ExternalRequestTypesFilterControl
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
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grid.SuspendLayout();
            this.AddStripButton.SuspendLayout();
            this.RecentItemsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("0a521f6d-f3ba-4eeb-9382-28b1273fec5d", "Code");
            zTextBoxColumnStyleInfo1.ColumnName = "RQT_Code";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("fc5309d7-50d5-4786-b501-4089c28431be", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "RQT_Description";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("e71634f2-02a2-4763-bd47-337652e660a5", "Job Type");
            zCodeFindBoxColumnStyleInfo1.ColumnName = "RQT_JobType";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("fe5cad2c-fd93-48aa-bd3b-83ae25653de4", "Is Active");
            zCheckBoxColumnStyleInfo1.ColumnName = "RQT_IsActive";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("98eae32c-dbb7-4e6d-a9d1-b99ad0f9fa75", "Is System");
            zCheckBoxColumnStyleInfo2.ColumnName = "RQT_IsSystem";
            zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
            this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 231, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Business.ExternalRequestTypeCollection);
            // 
            // ExternalRequestTypeFilterControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Name = "ExternalRequestTypeFilterControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 383, true);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            this.AddStripButton.ResumeLayout(true);
            this.AddStripButton.PerformLayout();
            this.RecentItemsPanel.ResumeLayout(false);
            this.RecentItemsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}
