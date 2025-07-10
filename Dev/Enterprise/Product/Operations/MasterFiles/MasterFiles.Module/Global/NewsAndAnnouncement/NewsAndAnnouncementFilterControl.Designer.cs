namespace Enterprise.MasterFiles.Module
{
	public partial class NewsAndAnnouncementFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).GF_Summary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).SectionName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).ReleaseNoteDateLocal)));
			zTranslatableTextBoxColumnStyleInfo1.ColumnName = "GF_Summary";
			zTranslatableTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo1.ColumnName = "SectionName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "ReleaseNoteDateLocal";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.grid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 256, true);
			this.grid.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.NewsAnnouncement);
			// 
			// NewsAndAnnouncementFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "NewsAndAnnouncementFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 408, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
