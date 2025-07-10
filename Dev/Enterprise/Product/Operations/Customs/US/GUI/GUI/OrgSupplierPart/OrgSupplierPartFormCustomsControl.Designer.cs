using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class OrgSupplierPartFormCustomsControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo3 = new TariffColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo4 = new TariffColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo5 = new TariffColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo6 = new TariffColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo7 = new TariffColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo8 = new TariffColumnStyleInfo();
			TariffColumnStyleInfo tariffColumnStyleInfo9 = new TariffColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			this.detailsPanel = new ZPanel();
			this.importClassificationUserControl = new ImportClassificationUserControl(PivotGrid);
			this.exportClassificationUserControl = new ExportCustomsClassificationUserControl(PivotGrid);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			this.detailsPanel.SuspendLayout();
			this.importClassificationUserControl.SuspendLayout();
			this.exportClassificationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OrgSupplierPart);
			// 
			// PivotGrid
			// 
			this.PivotGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PivotGrid, "PivotsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_UsageComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_ChildTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_FormattedTariffNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_FormattedSupplementalTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).HasChildren)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_DateStart)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_DateEnd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).SupFormattedAdditionalTariff1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).SupFormattedAdditionalTariff2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).SupFormattedAdditionalTariff3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).SupFormattedAdditionalTariff4)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).SupFormattedAdditionalTariff5)));
			this.PivotGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgSupplierPartFormCustomsControl|5501a1cd-5887-4792-bc9a-2bb59f47806f", "Type Desc.", "Type Description");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CI_ChildTypeDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo2.ColumnName = "CI_OH";
			zGuidDropEditColumnStyleInfo2.IsMandatory = true;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tariffColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgSupplierPartFormCustomsControl|6f09b168-204b-4714-afdc-0be0cb67c930", "Tariff");
			tariffColumnStyleInfo3.ColumnName = "CI_FormattedTariffNum";
			tariffColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgSupplierPartFormCustomsControl|0ae18bc3-833f-41d5-a00b-e31faa56af95", "Prov/Prog. Tariff", "Provision or Program Tariff", "");
			tariffColumnStyleInfo4.ColumnName = "CI_FormattedSupplementalTariff";
			tariffColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "CI_CC";
			zGuidFindBoxColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgSupplierPartFormCustomsControl|cd5df560-7e82-4851-9c92-18bcd6dbeaaf", "Multi Tariff");
			zCheckBoxColumnStyleInfo2.ColumnName = "HasChildren";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo3.Caption = "Start Date";
			zDateEditColumnStyleInfo3.ColumnName = "CI_DateStart";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.Caption = "End Date";
			zDateEditColumnStyleInfo4.ColumnName = "CI_DateEnd";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("OrgSupplierPartFormCustomsControlGlobal|EDF7A40F-BA1B-4197-9465-80BA014A65BC", "Usage Comment");
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "CI_UsageComment";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 200;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			tariffColumnStyleInfo5.ColumnName = "SupFormattedAdditionalTariff1";
			tariffColumnStyleInfo5.IsVisible = false;
			tariffColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			tariffColumnStyleInfo6.ColumnName = "SupFormattedAdditionalTariff2";
			tariffColumnStyleInfo6.IsVisible = false;
			tariffColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			tariffColumnStyleInfo7.ColumnName = "SupFormattedAdditionalTariff3";
			tariffColumnStyleInfo7.IsVisible = false;
			tariffColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			tariffColumnStyleInfo8.ColumnName = "SupFormattedAdditionalTariff4";
			tariffColumnStyleInfo8.IsVisible = false;
			tariffColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			tariffColumnStyleInfo9.ColumnName = "SupFormattedAdditionalTariff5";
			tariffColumnStyleInfo9.IsVisible = false;
			tariffColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			this.PivotGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PivotGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo3);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo4);
			this.PivotGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.PivotGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PivotGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.PivotGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.PivotGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo5);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo6);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo7);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo8);
			this.PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo9);
			this.PivotGrid.CopySelectedRowsAllowed = true;
			this.PivotGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.PivotGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PivotGrid.LayoutKey = "zGrid1";
			this.PivotGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 90, true);
			this.PivotGrid.TabIndex = 9;
			this.PivotGrid.DoubleClick += new EventHandler(this.pivotGrid_Click);
			// 
			// detailsPanel
			// 
			this.detailsPanel.Controls.Add(this.importClassificationUserControl);
			this.detailsPanel.Controls.Add(this.exportClassificationUserControl);
			this.detailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.detailsPanel.Name = "detailsPanel";
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 313, true);
			this.detailsPanel.TabIndex = 11;
			// 
			// importClassificationUserControl
			// 
			this.importClassificationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.importClassificationUserControl, "PivotsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)))));
			this.importClassificationUserControl.CurrentPivot = null;
			this.importClassificationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importClassificationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.importClassificationUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 0, true);
			this.importClassificationUserControl.Name = "importClassificationUserControl";
			this.importClassificationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 288, true);
			this.importClassificationUserControl.TabIndex = 9;
			// 
			// exportClassificationUserControl
			// 
			this.exportClassificationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.exportClassificationUserControl, "PivotsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)))));
			this.exportClassificationUserControl.CurrentPivot = null;
			this.exportClassificationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exportClassificationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exportClassificationUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 0, true);
			this.exportClassificationUserControl.Name = "exportClassificationUserControl";
			this.exportClassificationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 288, true);
			this.exportClassificationUserControl.TabIndex = 8;
			// 
			// OrgSupplierPartFormCustomsControl
			// 
			this.Controls.Add(this.detailsPanel);
			this.Controls.Add(this.PivotGrid);
			this.Name = "OrgSupplierPartFormCustomsControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 403, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			this.detailsPanel.ResumeLayout(false);
			this.detailsPanel.PerformLayout();
			this.importClassificationUserControl.ResumeLayout(true);
			this.importClassificationUserControl.PerformLayout();
			this.exportClassificationUserControl.ResumeLayout(true);
			this.exportClassificationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		
		ZPanel detailsPanel;
		ImportClassificationUserControl importClassificationUserControl;
		ExportCustomsClassificationUserControl exportClassificationUserControl;

	}
}
