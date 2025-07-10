namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	partial class TWBillCountrySpecificUserControl
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
            this.ProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SequenceNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.RemarksLongTextControl = new Enterprise.ZArchitecture.ZTextBox();
            this.ManifestQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
            this.GoodsValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PortOfDischargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ProcedureDropEdit.SuspendLayout();
            this.ManifestQtyCalcDropEdit.SuspendLayout();
            this.GrossWeightCalcDropEdit.SuspendLayout();
            this.GoodsValueConvertToLocalCurrencyControl.SuspendLayout();
            this.PortOfLoadingCodeFindBox.SuspendLayout();
            this.PortOfDischargeCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill);
            // 
            // ProcedureDropEdit
            // 
            this.ProcedureDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ProcedureDropEdit, "ABL_Procedure");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_Procedure)));
            this.ProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 175, true);
            this.ProcedureDropEdit.Name = "ProcedureDropEdit";
            this.ProcedureDropEdit.PreBoundMaxLength = 1;
            this.ProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.ProcedureDropEdit.TabIndex = 4;
            // 
            // SequenceNumberCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.SequenceNumberCalcEdit, "ABL_SequenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_SequenceNumber)));
            this.SequenceNumberCalcEdit.DecimalPlaces = 2;
            this.SequenceNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.SequenceNumberCalcEdit.Name = "SequenceNumberCalcEdit";
            this.SequenceNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
            this.SequenceNumberCalcEdit.TabIndex = 1;
            this.SequenceNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.SequenceNumberCalcEdit.TrackDisposedAccess = true;
            // 
            // RemarksLongTextControl
            // 
            this.RemarksLongTextControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RemarksLongTextControl, "Remarks");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).Remarks)));
            this.RemarksLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.RemarksLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 55, true);
            this.RemarksLongTextControl.Multiline = true;
            this.RemarksLongTextControl.Name = "RemarksLongTextControl";
            this.RemarksLongTextControl.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.RemarksLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 114, true);
            this.RemarksLongTextControl.TabIndex = 3;
            // 
            // ManifestQtyCalcDropEdit
            // 
            this.ManifestQtyCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ManifestQtyCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_ManifestQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_ManifestUQ)));
            this.ManifestQtyCalcDropEdit.BindToAmount = "ABL_ManifestQty";
            this.ManifestQtyCalcDropEdit.BindToUnit = "ABL_ManifestUQ";
            this.ManifestQtyCalcDropEdit.Decimals = 0;
            this.ManifestQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 3, true);
            this.ManifestQtyCalcDropEdit.MaxValue = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.ManifestQtyCalcDropEdit.Name = "ManifestQtyCalcDropEdit";
            this.ManifestQtyCalcDropEdit.ShowDescriptionBox = true;
            this.ManifestQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
            this.ManifestQtyCalcDropEdit.TabIndex = 5;
            // 
            // GrossWeightCalcDropEdit
            // 
            this.GrossWeightCalcDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_GrossWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_GrossWeightUQ)));
            this.GrossWeightCalcDropEdit.BindToAmount = "ABL_GrossWeight";
            this.GrossWeightCalcDropEdit.BindToUnit = "ABL_GrossWeightUQ";
            this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 29, true);
            this.GrossWeightCalcDropEdit.MaxValue = new decimal(new int[] {
            1215752191,
            23,
            0,
            196608});
            this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
            this.GrossWeightCalcDropEdit.ShowDescriptionBox = true;
            this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
            this.GrossWeightCalcDropEdit.TabIndex = 6;
            // 
            // GoodsValueConvertToLocalCurrencyControl
            // 
            this.GoodsValueConvertToLocalCurrencyControl.AllowDrop = true;
            this.GoodsValueConvertToLocalCurrencyControl.BindToAmount = "ABL_GoodsValue";
            this.GoodsValueConvertToLocalCurrencyControl.BindToUnit = "ABL_RX_NKGoodsValueCurrency";
            this.GoodsValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.GoodsValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 81, true);
            this.GoodsValueConvertToLocalCurrencyControl.Name = "GoodsValueConvertToLocalCurrencyControl";
            this.GoodsValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.GoodsValueConvertToLocalCurrencyControl.TabIndex = 8;
            // 
            // PortOfLoadingCodeFindBox
            // 
            this.PortOfLoadingCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "ABL_RL_NKPortOfLoading");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_RL_NKPortOfLoading)));
            this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 201, true);
            this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
            this.PortOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PortOfLoadingCodeFindBox.ParentType = null;
            this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.PortOfLoadingCodeFindBox.TabIndex = 9;
            // 
            // PortOfDischargeCodeFindBox
            // 
            this.PortOfDischargeCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PortOfDischargeCodeFindBox, "ABL_RL_NKPortOfDischarge");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill)(null)).ABL_RL_NKPortOfDischarge)));
            this.PortOfDischargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 227, true);
            this.PortOfDischargeCodeFindBox.Name = "PortOfDischargeCodeFindBox";
            this.PortOfDischargeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PortOfDischargeCodeFindBox.ParentType = null;
            this.PortOfDischargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.PortOfDischargeCodeFindBox.TabIndex = 10;
            // 
            // TWBillCountrySpecificUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.PortOfDischargeCodeFindBox);
            this.Controls.Add(this.PortOfLoadingCodeFindBox);
            this.Controls.Add(this.GoodsValueConvertToLocalCurrencyControl);
            this.Controls.Add(this.GrossWeightCalcDropEdit);
            this.Controls.Add(this.ManifestQtyCalcDropEdit);
            this.Controls.Add(this.RemarksLongTextControl);
            this.Controls.Add(this.SequenceNumberCalcEdit);
            this.Controls.Add(this.ProcedureDropEdit);
            this.Name = "TWBillCountrySpecificUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 309, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ProcedureDropEdit.ResumeLayout(true);
            this.ProcedureDropEdit.PerformLayout();
            this.ManifestQtyCalcDropEdit.ResumeLayout(true);
            this.ManifestQtyCalcDropEdit.PerformLayout();
            this.GrossWeightCalcDropEdit.ResumeLayout(true);
            this.GrossWeightCalcDropEdit.PerformLayout();
            this.GoodsValueConvertToLocalCurrencyControl.ResumeLayout(true);
            this.GoodsValueConvertToLocalCurrencyControl.PerformLayout();
            this.PortOfLoadingCodeFindBox.ResumeLayout(true);
            this.PortOfLoadingCodeFindBox.PerformLayout();
            this.PortOfDischargeCodeFindBox.ResumeLayout(true);
            this.PortOfDischargeCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZDropEdit ProcedureDropEdit;
		internal ZArchitecture.ZCalcEdit SequenceNumberCalcEdit;
		internal ZArchitecture.ZTextBox RemarksLongTextControl;
		internal ZArchitecture.GUI.ZCalcDropEdit ManifestQtyCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal Customs.GUI.ConvertToLocalCurrencyControl GoodsValueConvertToLocalCurrencyControl;
		internal ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox PortOfDischargeCodeFindBox;
	}
}
