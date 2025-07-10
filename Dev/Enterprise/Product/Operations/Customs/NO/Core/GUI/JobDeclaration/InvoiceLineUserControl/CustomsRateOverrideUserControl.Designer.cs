namespace Enterprise.Customs.NO.GUI
{
	partial class CustomsRateOverrideUserControl
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
			CustomsRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			CustomsTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			CustomsRateOverrideCheckBox = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsRateCalcEdit.SuspendLayout();
			this.CustomsTypeDropEdit.SuspendLayout();
			this.CustomsRateOverrideCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobComInvoiceLine);
			// 
			// CustomsRateCalcEdit
			//
			this.BindingSource.SetBindingMember(this.CustomsRateCalcEdit, "CustomsRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).CustomsRate)));
			this.CustomsRateCalcEdit.CaptionResourceString = null;
			this.CustomsRateCalcEdit.DecimalPlaces = 2;
			this.CustomsRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsRateCalcEdit.Name = "CustomsRateCalcEdit";
			this.CustomsRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.CustomsRateCalcEdit.TabIndex = 1;
			this.CustomsRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomsTypeDropEdit
			//
			this.CustomsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsTypeDropEdit, "CustomsRateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).CustomsRateType)));
			this.CustomsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 0, true);
			this.CustomsTypeDropEdit.Name = "CustomsTypeDropEdit";
			this.CustomsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.CustomsTypeDropEdit.TabIndex = 2;
			this.CustomsTypeDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.CustomsTypeDropEdit.ShowDescriptionBox = false;
			// 
			// CustomsRateOverrideCheckBox
			//
			this.BindingSource.SetBindingMember(this.CustomsRateOverrideCheckBox, "CustomsRateIsOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).CustomsRateIsOverridden)));
			this.CustomsRateOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomsRateOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 0, true);
			this.CustomsRateOverrideCheckBox.Name = "CustomsRateOverrideCheckBox";
			this.CustomsRateOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CustomsRateOverrideCheckBox.TabIndex = 3;
			this.CustomsRateOverrideCheckBox.UseCompatibleTextRendering = true;
			this.CustomsRateOverrideCheckBox.UseVisualStyleBackColor = true;

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsRateCalcEdit);
			this.Controls.Add(this.CustomsTypeDropEdit);
			this.Controls.Add(this.CustomsRateOverrideCheckBox);
			this.Name = "CustomsRateOverrideUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsRateCalcEdit.ResumeLayout(true);
			this.CustomsRateCalcEdit.PerformLayout();
			this.CustomsTypeDropEdit.ResumeLayout(true);
			this.CustomsTypeDropEdit.PerformLayout();
			this.CustomsRateOverrideCheckBox.ResumeLayout(true);
			this.CustomsRateOverrideCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.ZCalcEdit CustomsRateCalcEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit CustomsTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox CustomsRateOverrideCheckBox;
	}
}
