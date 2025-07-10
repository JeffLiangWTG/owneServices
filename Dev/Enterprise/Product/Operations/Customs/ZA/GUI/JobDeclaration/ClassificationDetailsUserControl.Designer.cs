namespace Enterprise.Customs.ZA.GUI
{
	partial class ClassificationDetailsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalDutiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalDutiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CustomsQuantityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FirstAddUnitCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SecondAddUnitCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ThirdAddUnitCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalDutiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDutiesGrid)).BeginInit();
			this.AdditionalDutiesGrid.SuspendLayout();
			this.CustomsQuantityGroupBox.SuspendLayout();
			this.FirstAddUnitCalcDropEdit.SuspendLayout();
			this.SecondAddUnitCalcDropEdit.SuspendLayout();
			this.ThirdAddUnitCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.JobComInvoiceLine);
			// 
			// AdditionalDutiesGroupBox
			// 
			this.AdditionalDutiesGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e7cc7b71-1db1-483e-918e-82acbd7b9837", "Additional Tariffs");
			this.AdditionalDutiesGroupBox.Controls.Add(this.AdditionalDutiesGrid);
			this.AdditionalDutiesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.AdditionalDutiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDutiesGroupBox.Name = "AdditionalDutiesGroupBox";
			this.AdditionalDutiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 131, true);
			this.AdditionalDutiesGroupBox.TabIndex = 0;
			this.AdditionalDutiesGroupBox.TabStop = false;
			// 
			// AdditionalDutiesGrid
			// 
			this.AdditionalDutiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDutiesGrid, "CusLineTariffDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).CusLineTariffDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).BZ_TariffAndCheckDigit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).FormulaSpecificQuestion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).CusLineTariffDetails)).SyncRoot)).FormulaSpecificValue)));
			this.AdditionalDutiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "BZ_Type";
			zDropEditColumnStyleInfo1.ShowHorizontalScrollBar = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BZ_TariffAndCheckDigit";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.RefCusTariff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.ColumnName = "FormulaSpecificQuestion";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo2.ColumnName = "FormulaSpecificValue";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.AdditionalDutiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalDutiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalDutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalDutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalDutiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDutiesGrid.GridId = "2d8d05a6-3856-442f-8a52-75d3906589c5";
			this.AdditionalDutiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDutiesGrid.LayoutKey = "zGrid1";
			this.AdditionalDutiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AdditionalDutiesGrid.Name = "AdditionalDutiesGrid";
			this.AdditionalDutiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 114, true);
			this.AdditionalDutiesGrid.TabIndex = 0;
			// 
			// CustomsQuantityGroupBox
			// 
			this.CustomsQuantityGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7ed8c537-3572-472e-a56b-16fc2a7f759a", "Additional Quantities");
			this.CustomsQuantityGroupBox.Controls.Add(this.FirstAddUnitCalcDropEdit);
			this.CustomsQuantityGroupBox.Controls.Add(this.SecondAddUnitCalcDropEdit);
			this.CustomsQuantityGroupBox.Controls.Add(this.ThirdAddUnitCalcDropEdit);
			this.CustomsQuantityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsQuantityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 0, true);
			this.CustomsQuantityGroupBox.Name = "CustomsQuantityGroupBox";
			this.CustomsQuantityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 131, true);
			this.CustomsQuantityGroupBox.TabIndex = 2;
			this.CustomsQuantityGroupBox.TabStop = false;
			// 
			// FirstAddUnitCalcDropEdit
			// 
			this.FirstAddUnitCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FirstAddUnitCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).JI_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).JI_CustomsSecondUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).Lookups.AdditionalUnitCodeList)));
			this.FirstAddUnitCalcDropEdit.BindToAmount = "JI_CustomsSecondQuantity";
			this.FirstAddUnitCalcDropEdit.BindToList = "Lookups.AdditionalUnitCodeList";
			this.FirstAddUnitCalcDropEdit.BindToUnit = "JI_CustomsSecondUnitQty";
			this.FirstAddUnitCalcDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("36734147-B66F-4159-8B09-0443F86CAB58", "Additional Qty 1", "Additional Quantity 1");
			this.FirstAddUnitCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 15, true);
			this.FirstAddUnitCalcDropEdit.Name = "FirstAddUnitCalcDropEdit";
			this.FirstAddUnitCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 17, true);
			this.FirstAddUnitCalcDropEdit.TabIndex = 0;
			this.FirstAddUnitCalcDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// SecondAddUnitCalcDropEdit
			// 
			this.SecondAddUnitCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecondAddUnitCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).JI_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).JI_CustomsThirdUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).Lookups.AdditionalUnitCodeList)));
			this.SecondAddUnitCalcDropEdit.BindToAmount = "JI_CustomsThirdQuantity";
			this.SecondAddUnitCalcDropEdit.BindToList = "Lookups.AdditionalUnitCodeList";
			this.SecondAddUnitCalcDropEdit.BindToUnit = "JI_CustomsThirdUnitQty";
			this.SecondAddUnitCalcDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("C8116CBD-F38C-4467-9897-F246C3507F27", "Additional Qty 2", "Additional Quantity 2");
			this.SecondAddUnitCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 37, true);
			this.SecondAddUnitCalcDropEdit.Name = "SecondAddUnitCalcDropEdit";
			this.SecondAddUnitCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 17, true);
			this.SecondAddUnitCalcDropEdit.TabIndex = 1;
			this.SecondAddUnitCalcDropEdit.UnitPreBoundMaxLength = 4;
			// 
			// ThirdAddUnitCalcDropEdit
			// 
			this.ThirdAddUnitCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ThirdAddUnitCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).JI_BondedWhsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).JI_BondedWhsUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobComInvoiceLine)(null)).Lookups.BondedWhsUnitQtyList)));
			this.ThirdAddUnitCalcDropEdit.BindToAmount = "JI_BondedWhsQuantity";
			this.ThirdAddUnitCalcDropEdit.BindToList = "Lookups.BondedWhsUnitQtyList";
			this.ThirdAddUnitCalcDropEdit.BindToUnit = "JI_BondedWhsUnitQty";
			this.ThirdAddUnitCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 59, true);
			this.ThirdAddUnitCalcDropEdit.Name = "ThirdAddUnitCalcDropEdit";
			this.ThirdAddUnitCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 17, true);
			this.ThirdAddUnitCalcDropEdit.TabIndex = 2;
			this.ThirdAddUnitCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ClassificationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsQuantityGroupBox);
			this.Controls.Add(this.AdditionalDutiesGroupBox);
			this.Name = "ClassificationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 131, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalDutiesGroupBox.ResumeLayout(false);
			this.AdditionalDutiesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDutiesGrid)).EndInit();
			this.AdditionalDutiesGrid.ResumeLayout(false);
			this.AdditionalDutiesGrid.PerformLayout();
			this.CustomsQuantityGroupBox.ResumeLayout(false);
			this.CustomsQuantityGroupBox.PerformLayout();
			this.FirstAddUnitCalcDropEdit.ResumeLayout(true);
			this.FirstAddUnitCalcDropEdit.PerformLayout();
			this.SecondAddUnitCalcDropEdit.ResumeLayout(true);
			this.SecondAddUnitCalcDropEdit.PerformLayout();
			this.ThirdAddUnitCalcDropEdit.ResumeLayout(true);
			this.ThirdAddUnitCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AdditionalDutiesGroupBox;
		private ZArchitecture.ZGrid AdditionalDutiesGrid;
		private ZArchitecture.GUI.ZGroupBox CustomsQuantityGroupBox;
		private ZArchitecture.GUI.ZCalcDropEdit FirstAddUnitCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit SecondAddUnitCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit ThirdAddUnitCalcDropEdit;
	}
}

