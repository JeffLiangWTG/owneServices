using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	partial class TRBillCountrySpecificUserControl
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
			this.RoRoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransshipmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BillStampDutyValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AirBillStampDutyABSValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.TransshipmentTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Manifest.Business.AsycudaBill);
			// 
			// RoRoCheckBox
			// 
			this.RoRoCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RoRoCheckBox, "RoRo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).RoRo)));
			this.RoRoCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RoRoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 64, true);
			this.RoRoCheckBox.Name = "RoRoCheckBox";
			this.RoRoCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RoRoCheckBox.TabIndex = 2;
			this.RoRoCheckBox.UseVisualStyleBackColor = true;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).PaymentType)));
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 12, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 1;
			this.PaymentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 1;
			// 
			// TransshipmentTypeDropEdit
			// 
			this.TransshipmentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransshipmentTypeDropEdit, "TransshipmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).TransshipmentType)));
			this.TransshipmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 38, true);
			this.TransshipmentTypeDropEdit.Name = "TransshipmentTypeDropEdit";
			this.TransshipmentTypeDropEdit.PreBoundMaxLength = 1;
			this.TransshipmentTypeDropEdit.ShouldResizeByMaxLength = true;
			this.TransshipmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.TransshipmentTypeDropEdit.TabIndex = 3;
			// 
			// BillStampDutyValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BillStampDutyValueCalcEdit, "BillStampDutyValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).BillStampDutyValue)));
			this.BillStampDutyValueCalcEdit.CaptionResourceString = null;
			this.BillStampDutyValueCalcEdit.DecimalPlaces = 2;
			this.BillStampDutyValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 84, true);
			this.BillStampDutyValueCalcEdit.Name = "BillStampDutyValueCalcEdit";
			this.BillStampDutyValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.BillStampDutyValueCalcEdit.TabIndex = 4;
			this.BillStampDutyValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AirBillStampDutyABSValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AirBillStampDutyABSValueCalcEdit, "AirBillStampDutyABSValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Manifest.Business.AsycudaBill)(null)).AirBillStampDutyABSValue)));
			this.AirBillStampDutyABSValueCalcEdit.CaptionResourceString = null;
			this.AirBillStampDutyABSValueCalcEdit.DecimalPlaces = 2;
			this.AirBillStampDutyABSValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 84, true);
			this.AirBillStampDutyABSValueCalcEdit.Name = "AirBillStampDutyABSValueCalcEdit";
			this.AirBillStampDutyABSValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.AirBillStampDutyABSValueCalcEdit.TabIndex = 4;
			this.AirBillStampDutyABSValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TRBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TransshipmentTypeDropEdit);
			this.Controls.Add(this.PaymentTypeDropEdit);
			this.Controls.Add(this.RoRoCheckBox);
			this.Controls.Add(this.BillStampDutyValueCalcEdit);
			this.Controls.Add(this.AirBillStampDutyABSValueCalcEdit);
			this.Name = "TRBillCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 125, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.TransshipmentTypeDropEdit.ResumeLayout(true);
			this.TransshipmentTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox RoRoCheckBox;
		internal ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TransshipmentTypeDropEdit;
		internal ZCalcEdit BillStampDutyValueCalcEdit;
		internal ZCalcEdit AirBillStampDutyABSValueCalcEdit;
	}
}
