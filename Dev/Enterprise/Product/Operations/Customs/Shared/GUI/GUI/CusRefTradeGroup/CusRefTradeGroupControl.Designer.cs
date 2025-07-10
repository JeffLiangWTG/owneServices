using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class CusRefTradeGroupControl
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
			if (disposing)
			{
				if (TradeGroupCountryGrid != null)
				{
					if (TradeGroupCountryGrid.Columns[CusRefTradeGroupCountry.Schema.CRA_RN_NKTradeGroupCountryCode]?.ColumnStyle is ZCodeFindBoxColumnStyle tradeGroupCountryCodeFindBoxColumnStyle)
					{
						tradeGroupCountryCodeFindBoxColumnStyle.PopupSelected -= TradeGroupCountryCodeFindBoxColumnStyle_PopupSelected;
					}
					TradeGroupCountryGrid.AfterBind -= TradeGroupCountryGrid_AfterBind;
				}


				components?.Dispose();
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.BasicInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TradeGroupTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TradeGroupCountryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TradeGroupCountryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BasicInformationGroupBox.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.StartDateEdit.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.TradeGroupCountryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TradeGroupCountryGrid)).BeginInit();
			this.TradeGroupCountryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefTradeGroup);
			// 
			// BasicInformationGroupBox
			// 
			this.BasicInformationGroupBox.BackColor = System.Drawing.Color.Transparent;
			this.BasicInformationGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("42484942-ABF3-45DF-8BC7-B67FE12CEE50", "Basic Information");
			this.BasicInformationGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.BasicInformationGroupBox.Controls.Add(this.TradeGroupTextBox);
			this.BasicInformationGroupBox.Controls.Add(this.DescriptionTextBox);
			this.BasicInformationGroupBox.Controls.Add(this.StartDateEdit);
			this.BasicInformationGroupBox.Controls.Add(this.EndDateEdit);
			this.BasicInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicInformationGroupBox.Name = "BasicInformationGroupBox";
			this.BasicInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 304, true);
			this.BasicInformationGroupBox.TabIndex = 1;
			this.BasicInformationGroupBox.TabStop = false;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "CR9_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).CR9_RN_NKCountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 33, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeFindBox.ParentType = null;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 17, true);
			this.CountryCodeFindBox.TabIndex = 0;
			// 
			// TradeGroupTextBox
			// 
			this.BindingSource.SetBindingMember(this.TradeGroupTextBox, "CR9_TradeGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).CR9_TradeGroup)));
			this.TradeGroupTextBox.CaptionResourceString = null;
			this.TradeGroupTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 59, true);
			this.TradeGroupTextBox.Name = "TradeGroupTextBox";
			this.TradeGroupTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 17, true);
			this.TradeGroupTextBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CR9_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).CR9_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 85, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 147, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "CR9_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).CR9_StartDate)));
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 239, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 3;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "CR9_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).CR9_EndDate)));
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 262, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 5;
			// 
			// TradeGroupCountryGroupBox
			// 
			this.TradeGroupCountryGroupBox.BackColor = System.Drawing.Color.Transparent;
			this.TradeGroupCountryGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C8808BF9-F6E0-4D4D-90D0-68AA2BC6F151", "Country Codes");
			this.TradeGroupCountryGroupBox.Controls.Add(this.TradeGroupCountryGrid);
			this.TradeGroupCountryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 2, true);
			this.TradeGroupCountryGroupBox.Name = "TradeGroupCountryGroupBox";
			this.TradeGroupCountryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 302, true);
			this.TradeGroupCountryGroupBox.TabIndex = 5;
			this.TradeGroupCountryGroupBox.TabStop = false;
			// 
			// TradeGroupCountryGrid
			// 
			this.TradeGroupCountryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TradeGroupCountryGrid, "TradeGroupCountries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).TradeGroupCountries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTradeGroupCountry)(((System.Collections.IList)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).TradeGroupCountries)).SyncRoot)).CRA_RN_NKTradeGroupCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.CusRefTradeGroupCountry)(((System.Collections.IList)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).TradeGroupCountries)).SyncRoot)).CRA_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.Universal.CusRefTradeGroupCountry)(((System.Collections.IList)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).TradeGroupCountries)).SyncRoot)).CRA_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.Universal.CusRefTradeGroupCountry)(((System.Collections.IList)(((Enterprise.Customs.Universal.CusRefTradeGroup)(null)).TradeGroupCountries)).SyncRoot)).CRA_EndDate)));
			this.TradeGroupCountryGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.AllowModuleMultiSelect = true;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CRA_RN_NKTradeGroupCountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "CRA_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "CRA_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "CRA_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TradeGroupCountryGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TradeGroupCountryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TradeGroupCountryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TradeGroupCountryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TradeGroupCountryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TradeGroupCountryGrid.GridId = "D59B626A-27FD-4124-875E-57DAF8E531A9";
			this.TradeGroupCountryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TradeGroupCountryGrid.LayoutKey = "TradeGroupCountryGrid";
			this.TradeGroupCountryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.TradeGroupCountryGrid.Name = "TradeGroupCountryGrid";
			this.TradeGroupCountryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 285, true);
			this.TradeGroupCountryGrid.TabIndex = 6;
			// 
			// CusRefTradeGroupControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BasicInformationGroupBox);
			this.Controls.Add(this.TradeGroupCountryGroupBox);
			this.Name = "CusRefTradeGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 306, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BasicInformationGroupBox.ResumeLayout(false);
			this.BasicInformationGroupBox.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.TradeGroupCountryGroupBox.ResumeLayout(false);
			this.TradeGroupCountryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TradeGroupCountryGrid)).EndInit();
			this.TradeGroupCountryGrid.ResumeLayout(false);
			this.TradeGroupCountryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	ZGroupBox BasicInformationGroupBox;
	ZGroupBox TradeGroupCountryGroupBox;
	ZGrid TradeGroupCountryGrid;
	ZCodeFindBox CountryCodeFindBox;
	ZTextBox TradeGroupTextBox;
	ZTextBox DescriptionTextBox;
	ZDateEdit StartDateEdit;
	ZDateEdit EndDateEdit;

	#endregion
}
}
