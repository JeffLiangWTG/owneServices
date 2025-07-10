namespace Enterprise.Customs.NO.GUI;

partial class OrgSupplierPartFormCustomsControl
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
		this.TariffFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.SupplementaryCodesUserControl = new Enterprise.Customs.NO.GUI.SupplementaryCodesUserControl();
		this.AddQty1CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.AddQty2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.AddQty3CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.AddQty4CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.VATCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
		this.ReducedCustomsFlagDropEdit = new ZArchitecture.GUI.ZDropEdit();
		this.ProcedureCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
		this.CountryOfOriginTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.CountyOfOriginDropEdit = new ZArchitecture.GUI.ZDropEdit();
		this.PreferenceCodeDropEdit = new ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.detailsPanel.SuspendLayout();
		this.DetailTabControl.SuspendLayout();
		this.DetailsTabPage.SuspendLayout();
		this.CI_UsageCommentTextBox.SuspendLayout();
		this.ClassificationDescriptionTextBox.SuspendLayout();
		this.SupplementaryCodesUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.OrgSupplierPart);
		//
		// detailsPanel
		//
		this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 315, true);
		//
		// DetailTabControl
		//
		this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 315, true);
		//
		// DetailsTabPage
		//
		this.DetailsTabPage.Controls.Add(this.TariffFindBox);
		this.DetailsTabPage.Controls.Add(this.SupplementaryCodesUserControl);
		this.DetailsTabPage.Controls.Add(this.AddQty1CalcEdit);
		this.DetailsTabPage.Controls.Add(this.AddQty2CalcEdit);
		this.DetailsTabPage.Controls.Add(this.AddQty3CalcEdit);
		this.DetailsTabPage.Controls.Add(this.AddQty4CalcEdit);
		this.DetailsTabPage.Controls.Add(this.VATCodeDropEdit);
		this.DetailsTabPage.Controls.Add(this.ReducedCustomsFlagDropEdit);
		this.DetailsTabPage.Controls.Add(this.ProcedureCodeDropEdit);
		this.DetailsTabPage.Controls.Add(this.CountryOfOriginTextBox);
		this.DetailsTabPage.Controls.Add(this.CountyOfOriginDropEdit);
		this.DetailsTabPage.Controls.Add(this.PreferenceCodeDropEdit);
		this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 255, true);
		this.DetailsTabPage.Controls.SetChildIndex(this.ClassificationDescriptionTextBox, 0);
		this.DetailsTabPage.Controls.SetChildIndex(this.CI_UsageCommentTextBox, 0);
		//
		// CI_UsageCommentTextBox
		//
		this.CI_UsageCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 10, true);
		this.CI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
		this.CI_UsageCommentTextBox.TabIndex = 0;
		//
		// TariffFindBox
		//
		this.TariffFindBox.CaptionResourceString = Res.GetData("91409DDD-3864-4EB1-AB0B-EA05F324C836", "Tariff");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_UsageComment)));
		this.BindingSource.SetBindingMember(this.TariffFindBox, "PivotsForBinding.CI_FormattedTariffNum");
		this.TariffFindBox.Name = "TariffFindBox";
		this.TariffFindBox.ShowDescriptionBox = false;
		this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 35, true);
		this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
		this.TariffFindBox.TabIndex = 3;
		// 
		// SupplementaryCodesUserControl
		// 
		this.BindingSource.SetBindingMember(this.SupplementaryCodesUserControl, "PivotsForBinding");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)))));
		this.SupplementaryCodesUserControl.Name = "SupplementaryCodesUserControl";
		this.SupplementaryCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 60, true);
		this.SupplementaryCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 20, true);
		this.SupplementaryCodesUserControl.TabIndex = 4;
		// 
		// AddQty1CalcEdit
		//
		this.AddQty1CalcEdit.CaptionResourceString = Res.GetData("08378D97-9904-4555-A95E-D4F358D81A32", "Additional Qty 1");
		this.AddQty1CalcEdit.DecimalPlaces = 2;
		this.AddQty1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 85, true);
		this.AddQty1CalcEdit.Name = "AddQty1CalcEdit";
		this.AddQty1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
		this.AddQty1CalcEdit.TabIndex = 9;
		this.AddQty1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.AddQty1CalcEdit.TrackDisposedAccess = true;
		// 
		// AddQty2CalcEdit
		//
		this.AddQty2CalcEdit.CaptionResourceString = Res.GetData("DD702E56-5587-4274-BA80-466001237C16", "Add. Qty 2", "Add. Qty 2", "Additional Qty 2");
		this.AddQty2CalcEdit.DecimalPlaces = 2;
		this.AddQty2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 85, true);
		this.AddQty2CalcEdit.Name = "AddQty2CalcEdit";
		this.AddQty2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
		this.AddQty2CalcEdit.TabIndex = 10;
		this.AddQty2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// AddQty3CalcEdit
		//
		this.AddQty3CalcEdit.CaptionResourceString = Res.GetData("CE8F2867-27BC-45B3-8179-67714E9BABB9", "Add. Qty 3", "Add. Qty 3", "Additional Qty 3");
		this.AddQty3CalcEdit.DecimalPlaces = 2;
		this.AddQty3CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 85, true);
		this.AddQty3CalcEdit.Name = "AddQty3CalcEdit";
		this.AddQty3CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
		this.AddQty3CalcEdit.TabIndex = 11;
		this.AddQty3CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.AddQty3CalcEdit.TrackDisposedAccess = true;
		// 
		// AddQty4CalcEdit
		//
		this.AddQty4CalcEdit.CaptionResourceString = Res.GetData("FAE9BF05-C046-41AC-9A12-C10D20F5CE5C", "Add. Qty 4", "Add. Qty 4", "Additional Qty 4");
		this.AddQty4CalcEdit.DecimalPlaces = 2;
		this.AddQty4CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 85, true);
		this.AddQty4CalcEdit.Name = "AddQty4CalcEdit";
		this.AddQty4CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
		this.AddQty4CalcEdit.TabIndex = 12;
		this.AddQty4CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.AddQty4CalcEdit.TrackDisposedAccess = true;
		//
		// VATCodeDropEdit
		//
		this.BindingSource.SetBindingMember(this.VATCodeDropEdit, "PivotsForBinding.CI_ZZF_NKTaxType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ZZF_NKTaxType)));
		this.VATCodeDropEdit.AllowDrop = true;
		this.VATCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 110, true);
		this.VATCodeDropEdit.Name = "VATCodeDropEdit";
		this.VATCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 15, true);
		this.VATCodeDropEdit.TabIndex = 13;
		// 
		// ReducedCustomsFlagDropEdit
		//
		this.BindingSource.SetBindingMember(this.ReducedCustomsFlagDropEdit, "PivotsForBinding.CI_ReducedCustomsFlag");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ReducedCustomsFlag)));
		this.ReducedCustomsFlagDropEdit.AllowDrop = true;
		this.ReducedCustomsFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 110, true);
		this.ReducedCustomsFlagDropEdit.Name = "ReducedCustomsFlagDropEdit";
		this.ReducedCustomsFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 15, true);
		this.ReducedCustomsFlagDropEdit.TabIndex = 14;
		// 
		// ProcedureCodeDropEdit
		// 
		this.ProcedureCodeDropEdit.AllowDrop = true;
		this.ProcedureCodeDropEdit.CaptionResourceString = Res.GetData("BC00B0CB-2D6E-47EA-B8E6-07D78DD34A0F", "Procedure Code");
		this.ProcedureCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 135, true);
		this.ProcedureCodeDropEdit.Name = "ProcedureCodeDropEdit";
		this.ProcedureCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 20, true);
		this.ProcedureCodeDropEdit.TabIndex = 15;
		// 
		// CountryOfOriginTextBox
		//
		this.BindingSource.SetBindingMember(this.CountryOfOriginTextBox, "PivotsForBinding.CI_RN_NKCountryOfOrigin");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_RN_NKCountryOfOrigin)));
		this.CountryOfOriginTextBox.CaptionResourceString = Res.GetData("EB58C3D8-ADC9-4141-85F6-07491A550097", "Ctry./Rgn. of Orig.", "Ctry./Rgn. of Origin", "Country/Region of Origin");
		this.CountryOfOriginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 160, true);
		this.CountryOfOriginTextBox.Name = "CountryOfOriginTextBox";
		this.CountryOfOriginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
		this.CountryOfOriginTextBox.TabIndex = 16;
		// 
		// CountyOfOriginDropEdit
		//
		this.BindingSource.SetBindingMember(this.CountyOfOriginDropEdit, "PivotsForBinding.CI_RW_NKOriginState");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_RW_NKOriginState)));
		this.CountyOfOriginDropEdit.AllowDrop = true;
		this.CountyOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 160, true);
		this.CountyOfOriginDropEdit.Name = "CountyOfOriginDropEdit";
		this.CountyOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 20, true);
		this.CountyOfOriginDropEdit.TabIndex = 17;
		// 
		// PreferenceCodeDropEdit
		// 
		this.PreferenceCodeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.PreferenceCodeDropEdit, "PivotsForBinding.PreferenceCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).PreferenceCode)));
		this.PreferenceCodeDropEdit.CaptionResourceString = Res.GetData("6936D734-175B-4D66-8822-16F662A66F6F", "Pref. Code");
		this.PreferenceCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 185, true);
		this.PreferenceCodeDropEdit.Name = "PreferenceCodeDropEdit";
		this.PreferenceCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 20, true);
		this.PreferenceCodeDropEdit.TabIndex = 18;
		//
		// ClassificationDescriptionTextBox
		//
		this.ClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 210, true);
		this.ClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
		this.ClassificationDescriptionTextBox.TabIndex = 19;
		// 
		// OrgSupplierPartFormCustomsControl
		//
		this.CaptionRenderingEnabled = true;
		this.Name = "OrgSupplierPartFormCustomsControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 405, true);
		this.CI_UsageCommentTextBox.ResumeLayout(true);
		this.CI_UsageCommentTextBox.PerformLayout();
		this.ClassificationDescriptionTextBox.ResumeLayout(true);
		this.ClassificationDescriptionTextBox.PerformLayout();
		this.SupplementaryCodesUserControl.ResumeLayout(true);
		this.SupplementaryCodesUserControl.PerformLayout();
		this.detailsPanel.ResumeLayout(false);
		this.detailsPanel.PerformLayout();
		this.DetailTabControl.ResumeLayout(false);
		this.DetailTabControl.PerformLayout();
		this.DetailsTabPage.ResumeLayout(false);
		this.DetailsTabPage.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	Enterprise.Customs.NO.GUI.SupplementaryCodesUserControl SupplementaryCodesUserControl;
	Enterprise.ZArchitecture.GUI.ZCodeFindBox TariffFindBox;
	ZArchitecture.ZCalcEdit AddQty1CalcEdit;
	ZArchitecture.ZCalcEdit AddQty2CalcEdit;
	ZArchitecture.ZCalcEdit AddQty3CalcEdit;
	ZArchitecture.ZCalcEdit AddQty4CalcEdit;
	ZArchitecture.GUI.ZDropEdit VATCodeDropEdit;
	ZArchitecture.GUI.ZDropEdit ReducedCustomsFlagDropEdit;
	ZArchitecture.GUI.ZDropEdit ProcedureCodeDropEdit;
	ZArchitecture.ZTextBox CountryOfOriginTextBox;
	ZArchitecture.GUI.ZDropEdit CountyOfOriginDropEdit;
	ZArchitecture.GUI.ZDropEdit PreferenceCodeDropEdit;
}
