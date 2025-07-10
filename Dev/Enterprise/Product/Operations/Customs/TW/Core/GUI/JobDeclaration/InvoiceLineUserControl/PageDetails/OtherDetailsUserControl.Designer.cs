namespace Enterprise.Customs.TW.GUI
{
	partial class OtherDetailsUserControl
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
			this.AssignedNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AssignedNumberGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReservedFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReservedFieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EmptyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JI_TextileWidthCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.EnvironmentalProtectionTariffGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TW_EPTDigit3DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TW_EPTDigit2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TW_EPTDigit1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TW_TpfPymntMthdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManuFacturerAddressControl = new Enterprise.Customs.TW.GUI.TWJobDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AssignedNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AssignedNumberGrid)).BeginInit();
			this.AssignedNumberGrid.SuspendLayout();
			this.ReservedFieldsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReservedFieldsGrid)).BeginInit();
			this.ReservedFieldsGrid.SuspendLayout();
			this.JI_TextileWidthCalcDropEdit.SuspendLayout();
			this.EnvironmentalProtectionTariffGroupBox.SuspendLayout();
			this.TW_EPTDigit3DropEdit.SuspendLayout();
			this.TW_EPTDigit2DropEdit.SuspendLayout();
			this.TW_EPTDigit1DropEdit.SuspendLayout();
			this.TW_TpfPymntMthdDropEdit.SuspendLayout();
			this.ManuFacturerAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// AssignedNumbersGroupBox
			// 
			this.AssignedNumbersGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("2eada1f9-0432-4879-8936-3f810dae0a6a", "Assigned Numbers");
			this.AssignedNumbersGroupBox.Controls.Add(this.AssignedNumberGrid);
			this.AssignedNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.AssignedNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.AssignedNumbersGroupBox.Name = "AssignedNumbersGroupBox";
			this.AssignedNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 255, true);
			this.AssignedNumbersGroupBox.TabIndex = 1;
			this.AssignedNumbersGroupBox.TabStop = false;
			// 
			// AssignedNumberGrid
			// 
			this.AssignedNumberGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AssignedNumberGrid, "FilteredInvoiceLines.AssignedJobComInvLineRefsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AssignedJobComInvLineRefsCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.AssignedJobComInvLineRefs)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AssignedJobComInvLineRefsCollection)).SyncRoot)).JG_ReferenceNumber)));
			this.AssignedNumberGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JG_ReferenceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.AssignedNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AssignedNumberGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssignedNumberGrid.GridId = "0812b47c-aae0-48dd-8abe-003d36f7cef7";
			this.AssignedNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AssignedNumberGrid.LayoutKey = "AssignedNumberGrid";
			this.AssignedNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AssignedNumberGrid.MaximumRows = 10;
			this.AssignedNumberGrid.Name = "AssignedNumberGrid";
			this.AssignedNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 236, true);
			this.AssignedNumberGrid.TabIndex = 0;
			// 
			// ReservedFieldsGroupBox
			// 
			this.ReservedFieldsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("cf98c686-7c18-4f04-99c6-0da67ff199b1", "Reserved Fields");
			this.ReservedFieldsGroupBox.Controls.Add(this.ReservedFieldsGrid);
			this.ReservedFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ReservedFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 0, true);
			this.ReservedFieldsGroupBox.Name = "ReservedFieldsGroupBox";
			this.ReservedFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 255, true);
			this.ReservedFieldsGroupBox.TabIndex = 0;
			this.ReservedFieldsGroupBox.TabStop = false;
			// 
			// ReservedFieldsGrid
			// 
			this.ReservedFieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReservedFieldsGrid, "FilteredInvoiceLines.ReservedFields");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ReservedFields)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineReservedField)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ReservedFields)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLineReservedField)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ReservedFields)).SyncRoot)).CY_Data)));
			this.ReservedFieldsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ReservedFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReservedFieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReservedFieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReservedFieldsGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.ReservedFieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReservedFieldsGrid.LayoutKey = "ReservedFieldsGrid";
			this.ReservedFieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReservedFieldsGrid.MaximumRows = 10;
			this.ReservedFieldsGrid.Name = "ReservedFieldsGrid";
			this.ReservedFieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 236, true);
			this.ReservedFieldsGrid.TabIndex = 0;
			// 
			// EmptyPanel
			// 
			this.EmptyPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.EmptyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EmptyPanel.Name = "EmptyPanel";
			this.EmptyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 255, true);
			this.EmptyPanel.TabIndex = 6;
			// 
			// JI_TextileWidthCalcDropEdit
			// 
			this.JI_TextileWidthCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_TextileWidthCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TextileWidth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TextileWidthUQ)));
			this.JI_TextileWidthCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_TextileWidth";
			this.JI_TextileWidthCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_TextileWidthUQ";
			this.JI_TextileWidthCalcDropEdit.Decimals = 6;
			this.JI_TextileWidthCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 13, true);
			this.JI_TextileWidthCalcDropEdit.Name = "JI_TextileWidthCalcDropEdit";
			this.JI_TextileWidthCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.JI_TextileWidthCalcDropEdit.TabIndex = 3;
			this.JI_TextileWidthCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// EnvironmentalProtectionTariffGroupBox
			// 
			this.EnvironmentalProtectionTariffGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8ee3fb9b-69f8-43a7-9633-eb3d58257d35", "Environmental Protection Tariff");
			this.EnvironmentalProtectionTariffGroupBox.Controls.Add(this.TW_EPTDigit3DropEdit);
			this.EnvironmentalProtectionTariffGroupBox.Controls.Add(this.TW_EPTDigit2DropEdit);
			this.EnvironmentalProtectionTariffGroupBox.Controls.Add(this.TW_EPTDigit1DropEdit);
			this.EnvironmentalProtectionTariffGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(677, 65, true);
			this.EnvironmentalProtectionTariffGroupBox.Name = "EnvironmentalProtectionTariffGroupBox";
			this.EnvironmentalProtectionTariffGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 116, true);
			this.EnvironmentalProtectionTariffGroupBox.TabIndex = 5;
			this.EnvironmentalProtectionTariffGroupBox.TabStop = false;
			// 
			// TW_EPTDigit3DropEdit
			// 
			this.TW_EPTDigit3DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_EPTDigit3DropEdit, "FilteredInvoiceLines.JI_EPTDigit3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_EPTDigit3)));
			this.TW_EPTDigit3DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 80, true);
			this.TW_EPTDigit3DropEdit.Name = "TW_EPTDigit3DropEdit";
			this.TW_EPTDigit3DropEdit.PreBoundMaxLength = 1;
			this.TW_EPTDigit3DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.TW_EPTDigit3DropEdit.TabIndex = 2;
			// 
			// TW_EPTDigit2DropEdit
			// 
			this.TW_EPTDigit2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_EPTDigit2DropEdit, "FilteredInvoiceLines.JI_EPTDigit2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_EPTDigit2)));
			this.TW_EPTDigit2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 54, true);
			this.TW_EPTDigit2DropEdit.Name = "TW_EPTDigit2DropEdit";
			this.TW_EPTDigit2DropEdit.PreBoundMaxLength = 1;
			this.TW_EPTDigit2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.TW_EPTDigit2DropEdit.TabIndex = 1;
			// 
			// TW_EPTDigit1DropEdit
			// 
			this.TW_EPTDigit1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_EPTDigit1DropEdit, "FilteredInvoiceLines.JI_EPTDigit1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_EPTDigit1)));
			this.TW_EPTDigit1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 28, true);
			this.TW_EPTDigit1DropEdit.Name = "TW_EPTDigit1DropEdit";
			this.TW_EPTDigit1DropEdit.PreBoundMaxLength = 1;
			this.TW_EPTDigit1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.TW_EPTDigit1DropEdit.TabIndex = 0;
			// 
			// TW_TpfPymntMthdDropEdit
			// 
			this.TW_TpfPymntMthdDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TW_TpfPymntMthdDropEdit, "FilteredInvoiceLines.JI_TpfPymntMthd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TpfPymntMthd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TpfPymntMthdDescription)));
			this.TW_TpfPymntMthdDropEdit.BindToForDescription = "FilteredInvoiceLines.JI_TpfPymntMthdDescription";
			this.TW_TpfPymntMthdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 39, true);
			this.TW_TpfPymntMthdDropEdit.Name = "TW_TpfPymntMthdDropEdit";
			this.TW_TpfPymntMthdDropEdit.PreBoundMaxLength = 3;
			this.TW_TpfPymntMthdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.TW_TpfPymntMthdDropEdit.TabIndex = 4;
			// 
			// ManuFacturerAddressControl
			// 
			this.ManuFacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManuFacturerAddressControl, "FilteredInvoiceLines.ManufacturerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TW.Business.TWJobDocAddress)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ManufacturerDocAddress)));
			this.ManuFacturerAddressControl.BindToOrganisations = "FilteredInvoiceLines.Lookups.ManufacturerList";
			this.ManuFacturerAddressControl.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("4d439196-4cdf-491d-9f25-fd9c648edf10", "Manufacturer");
			this.ManuFacturerAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.ManuFacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(414, 0, true);
			this.ManuFacturerAddressControl.Name = "ManuFacturerAddressControl";
			this.ManuFacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 255, true);
			this.ManuFacturerAddressControl.TabIndex = 2;
			// 
			// OtherDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManuFacturerAddressControl);
			this.Controls.Add(this.EnvironmentalProtectionTariffGroupBox);
			this.Controls.Add(this.ReservedFieldsGroupBox);
			this.Controls.Add(this.TW_TpfPymntMthdDropEdit);
			this.Controls.Add(this.JI_TextileWidthCalcDropEdit);
			this.Controls.Add(this.AssignedNumbersGroupBox);
			this.Controls.Add(this.EmptyPanel);
			this.Name = "OtherDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1047, 255, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AssignedNumbersGroupBox.ResumeLayout(false);
			this.AssignedNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AssignedNumberGrid)).EndInit();
			this.AssignedNumberGrid.ResumeLayout(false);
			this.AssignedNumberGrid.PerformLayout();
			this.ReservedFieldsGroupBox.ResumeLayout(false);
			this.ReservedFieldsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReservedFieldsGrid)).EndInit();
			this.ReservedFieldsGrid.ResumeLayout(false);
			this.ReservedFieldsGrid.PerformLayout();
			this.JI_TextileWidthCalcDropEdit.ResumeLayout(true);
			this.JI_TextileWidthCalcDropEdit.PerformLayout();
			this.EnvironmentalProtectionTariffGroupBox.ResumeLayout(false);
			this.EnvironmentalProtectionTariffGroupBox.PerformLayout();
			this.TW_EPTDigit3DropEdit.ResumeLayout(true);
			this.TW_EPTDigit3DropEdit.PerformLayout();
			this.TW_EPTDigit2DropEdit.ResumeLayout(true);
			this.TW_EPTDigit2DropEdit.PerformLayout();
			this.TW_EPTDigit1DropEdit.ResumeLayout(true);
			this.TW_EPTDigit1DropEdit.PerformLayout();
			this.TW_TpfPymntMthdDropEdit.ResumeLayout(true);
			this.TW_TpfPymntMthdDropEdit.PerformLayout();
			this.ManuFacturerAddressControl.ResumeLayout(true);
			this.ManuFacturerAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AssignedNumbersGroupBox;
		private ZArchitecture.ZGrid AssignedNumberGrid;
		private ZArchitecture.GUI.ZGroupBox ReservedFieldsGroupBox;
		public ZArchitecture.ZGrid ReservedFieldsGrid;
		private ZArchitecture.GUI.ZPanel EmptyPanel;
		protected ZArchitecture.GUI.ZCalcDropEdit JI_TextileWidthCalcDropEdit;
		public ZArchitecture.GUI.ZGroupBox EnvironmentalProtectionTariffGroupBox;
		protected ZArchitecture.GUI.ZDropEdit TW_EPTDigit3DropEdit;
		protected ZArchitecture.GUI.ZDropEdit TW_EPTDigit2DropEdit;
		protected ZArchitecture.GUI.ZDropEdit TW_EPTDigit1DropEdit;
		internal ZArchitecture.GUI.ZDropEdit TW_TpfPymntMthdDropEdit;
		private TWJobDocAddressControl ManuFacturerAddressControl;
	}
}
