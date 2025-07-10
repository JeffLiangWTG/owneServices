using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI
{
	public partial class NZCConcessionForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.dutyRatesGrid = new ZArchitecture.ZGrid();
			this.zLabel5 = new ZArchitecture.ZLabel();
			this.u2_Calc_TariffsApplicableTextBox = new ZArchitecture.ZTextBox();
			this.u2_DescriptionTextBox = new ZArchitecture.ZTextBox();
			this.u0_DateActiveFromDateEdit = new ZDateEdit();
			this.u2_CodeTextBox = new ZArchitecture.ZTextBox();
			this.zLabel4 = new ZArchitecture.ZLabel();
			this.zLabel3 = new ZArchitecture.ZLabel();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.u0_DateActiveToDateEdit = new ZDateEdit();
			this.zLabel6 = new ZArchitecture.ZLabel();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dutyRatesGrid)).BeginInit();
			this.dutyRatesGrid.SuspendLayout();
			this.u0_DateActiveFromDateEdit.SuspendLayout();
			this.u0_DateActiveToDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 323, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.u0_DateActiveToDateEdit);
			this.MainTabPage.Controls.Add(this.zLabel6);
			this.MainTabPage.Controls.Add(this.dutyRatesGrid);
			this.MainTabPage.Controls.Add(this.zLabel5);
			this.MainTabPage.Controls.Add(this.u2_Calc_TariffsApplicableTextBox);
			this.MainTabPage.Controls.Add(this.u2_DescriptionTextBox);
			this.MainTabPage.Controls.Add(this.u2_CodeTextBox);
			this.MainTabPage.Controls.Add(this.u0_DateActiveFromDateEdit);
			this.MainTabPage.Controls.Add(this.zLabel4);
			this.MainTabPage.Controls.Add(this.zLabel3);
			this.MainTabPage.Controls.Add(this.zLabel2);
			this.MainTabPage.Controls.Add(this.zLabel1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 301, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 301, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 301, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 323, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 28, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(NZCConcession);
			// 
			// DutyRatesGrid
			// 
			this.dutyRatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.dutyRatesGrid, "DutyRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcession)(null)).DutyRates);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcessionDutyRate)(((System.Collections.IList)(((NZCConcession)(null)).DutyRates)).SyncRoot)).U5_PreferentialCountryGroup);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcessionDutyRate)(((System.Collections.IList)(((NZCConcession)(null)).DutyRates)).SyncRoot)).U5_DutyRatePercent);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcessionDutyRate)(((System.Collections.IList)(((NZCConcession)(null)).DutyRates)).SyncRoot)).U5_DutyRatePerUnit);
			this.dutyRatesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("E85B7A51-A27B-4979-ADFA-5290249AAC47", "Ctry/Rgn./Group");
			zTextBoxColumnStyleInfo1.ColumnName = "U5_PreferentialCountryGroup";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("D7B22DF5-D4F2-4773-B84B-AC2BE4A8B53F", "Percentage");
			zCalcEditColumnStyleInfo1.ColumnName = "U5_DutyRatePercent";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("E2511A8C-6DC2-4167-896F-F30585EF4D3F", "Per Unit");
			zCalcEditColumnStyleInfo2.ColumnName = "U5_DutyRatePerUnit";
			zCalcEditColumnStyleInfo2.Decimals = 4;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.dutyRatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.dutyRatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.dutyRatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.dutyRatesGrid.GridId = "e94fe446-ad3f-43dd-a0ab-75e3b47de558";
			this.dutyRatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dutyRatesGrid.LayoutKey = "DutyRatesGrid";
			this.dutyRatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 197, true);
			this.dutyRatesGrid.Name = "DutyRatesGrid";
			this.dutyRatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 91, true);
			this.dutyRatesGrid.TabIndex = 37;
			// 
			// zLabel5
			// 
			this.zLabel5.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CA4ADBEA-F8B1-436C-852A-D269EE4748E8", "Duty Rates:");
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 197, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.zLabel5.TabIndex = 36;
			// 
			// U2_Calc_TariffsApplicableTextBox
			// 
			this.BindingSource.SetBindingMember(this.u2_Calc_TariffsApplicableTextBox, "U2_Calc_Tariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcession)(null)).U2_Calc_Tariffs);
			this.u2_Calc_TariffsApplicableTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 114, true);
			this.u2_Calc_TariffsApplicableTextBox.Multiline = true;
			this.u2_Calc_TariffsApplicableTextBox.Name = "U2_Calc_TariffsApplicableTextBox";
			this.u2_Calc_TariffsApplicableTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 74, true);
			this.u2_Calc_TariffsApplicableTextBox.TabIndex = 32;
			// 
			// U2_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.u2_DescriptionTextBox, "U2_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcession)(null)).U2_Description);
			this.u2_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 31, true);
			this.u2_DescriptionTextBox.Multiline = true;
			this.u2_DescriptionTextBox.Name = "U2_DescriptionTextBox";
			this.u2_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 74, true);
			this.u2_DescriptionTextBox.TabIndex = 31;
			// 
			// U0_DateActiveFromDateEdit
			// 
			this.u0_DateActiveFromDateEdit.AllowDrop = true;
			this.u0_DateActiveFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.u0_DateActiveFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.u0_DateActiveFromDateEdit, "U2_DateActiveFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcession)(null)).U2_DateActiveFrom);
			this.u0_DateActiveFromDateEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("f963cff6-7017-46a8-aae9-f5f68c655dfa", "Valid From:");
			this.u0_DateActiveFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 7, true);
			this.u0_DateActiveFromDateEdit.Name = "U0_DateActiveFromDateEdit";
			this.u0_DateActiveFromDateEdit.TabIndex = 29;
			// 
			// U2_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.u2_CodeTextBox, "U2_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcession)(null)).U2_Code);
			this.u2_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 7, true);
			this.u2_CodeTextBox.Name = "U2_CodeTextBox";
			this.u2_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.u2_CodeTextBox.TabIndex = 28;
			// 
			// zLabel4
			// 
			this.zLabel4.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("D7B34833-A6E3-4C4C-9300-0FFADC7B7FDC", "Description:");
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 31, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.zLabel4.TabIndex = 35;
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6DB8AE43-5D0E-4713-8D31-194BCC98239F", "Tariffs Applicable:");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 114, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.zLabel3.TabIndex = 34;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("9F43F859-F085-4F22-95F1-DB6133F285DB", "Concession Code:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.zLabel1.TabIndex = 32;
			// 
			// U0_DateActiveToDateEdit
			// 
			this.u0_DateActiveToDateEdit.AllowDrop = true;
			this.u0_DateActiveToDateEdit.AutoCompleteMonthThreshold = 1;
			this.u0_DateActiveToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.u0_DateActiveToDateEdit, "U2_DateActiveTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((NZCConcession)(null)).U2_DateActiveTo);
			this.u0_DateActiveToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 7, true);
			this.u0_DateActiveToDateEdit.Name = "U0_DateActiveToDateEdit";
			this.u0_DateActiveToDateEdit.TabIndex = 30;
			// 
			// zLabel6
			// 
			this.zLabel6.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("78F7DB77-374D-4DE1-B79C-65C49AB3A0C5", "To:");
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 7, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 22, true);
			this.zLabel6.TabIndex = 39;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("31553A5B-CF83-4A63-90F1-8C35C2FF6015", "Valid From:");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 7, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 22, true);
			this.zLabel2.TabIndex = 33;
			// 
			// NZCConcessionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 379, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceType = typeof(NZCConcession);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.NZCConcession";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 416, true);
			this.Name = "NZCConcessionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("18227214-5A88-4F2F-B1BA-0955E3304684", "NZCConcessionForm");
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dutyRatesGrid)).EndInit();
			this.dutyRatesGrid.ResumeLayout(false);
			this.dutyRatesGrid.PerformLayout();
			this.u0_DateActiveFromDateEdit.ResumeLayout(true);
			this.u0_DateActiveFromDateEdit.PerformLayout();
			this.u0_DateActiveToDateEdit.ResumeLayout(true);
			this.u0_DateActiveToDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
