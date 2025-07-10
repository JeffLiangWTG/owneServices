namespace Enterprise.Customs.Universal.Module
{
	partial class ZZRefCusCodeListFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_CodeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_CodeTypeDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_CountryOrGrouping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_TransportModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.ZZRefCusCodeListCombined)(null)).ZZD_IsSystem)));
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("29ED5A6F-E728-443D-B633-397037AC0AC5", "List Type");
			zTextBoxColumnStyleInfo7.ColumnName = "ZZD_CodeType";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("1251E693-4BE3-4FD7-B436-ACAC2A74C3DF", "List Description");
			zTextBoxColumnStyleInfo8.ColumnName = "ZZD_CodeTypeDesc";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(139);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("5BC47E9E-3F65-4FCA-8518-AC5C44FCC73D", "Code");
			zTextBoxColumnStyleInfo9.ColumnName = "ZZD_Code";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("B30CE17B-BAED-43AC-91ED-45BD50FA782C", "Description");
			zTextBoxColumnStyleInfo10.ColumnName = "ZZD_Description";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(310);
			zTextBoxColumnStyleInfo11.ColumnName = "ZZD_CountryOrGrouping";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("79EA9AB2-F6EA-4E0C-A244-4CEF81BA7637", "Transport Modes");
			zTextBoxColumnStyleInfo12.ColumnName = "ZZD_TransportModes";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("40759F40-4D77-4D56-AB6E-04AC4113E19E", "Start Date");
			zDateEditColumnStyleInfo3.ColumnName = "ZZD_StartDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("2D785F9D-8A51-4666-8812-47FFB34787DC", "End Date");
			zDateEditColumnStyleInfo4.ColumnName = "ZZD_EndDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("43D1074B-63D7-4E19-AEFF-7A535D113FB1", "System");
			zCheckBoxColumnStyleInfo2.ColumnName = "ZZD_IsSystem";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 504, true);
			this.grid.TabIndex = 9;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCusCodeListCombined);
			// 
			// ZZRefCusCodeListFilterStripControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ZZRefCusCodeListFilterStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 504, true);
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
