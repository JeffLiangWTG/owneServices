using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
	partial class AutomaticDeferredSelectionUserControl
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
			this.AllowAutomaticDeferredSelectionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DaysBeforeETAIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.DataRegistry.Business.AutomaticDeferredSelection);
			// 
			// AllowAutomaticDeferredSelectionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AllowAutomaticDeferredSelectionCheckBox, "AllowAutomaticDeferredSelection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.DataRegistry.Business.AutomaticDeferredSelection)(null)).AllowAutomaticDeferredSelection)));
			this.AllowAutomaticDeferredSelectionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AllowAutomaticDeferredSelectionCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.AllowAutomaticDeferredSelectionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AllowAutomaticDeferredSelectionCheckBox.Name = "AllowAutomaticDeferredSelectionCheckBox";
			this.AllowAutomaticDeferredSelectionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 22, true);
			this.AllowAutomaticDeferredSelectionCheckBox.TabIndex = 1;
			// 
			// DaysBeforeETAIntEdit
			// 
			this.BindingSource.SetBindingMember(this.DaysBeforeETAIntEdit, "DaysBeforeETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.ZA.DataRegistry.Business.AutomaticDeferredSelection)(null)).DaysBeforeETA)));
			this.DaysBeforeETAIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 28, true);
			this.DaysBeforeETAIntEdit.Name = "DaysBeforeETAIntEdit";
			this.DaysBeforeETAIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.DaysBeforeETAIntEdit.TabIndex = 3;
			// 
			// AutomaticDeferredSelectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DaysBeforeETAIntEdit);
			this.Controls.Add(this.AllowAutomaticDeferredSelectionCheckBox);
			this.Name = "AutomaticDeferredSelectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 53, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZIntEdit DaysBeforeETAIntEdit;
		internal ZArchitecture.GUI.ZCheckBox AllowAutomaticDeferredSelectionCheckBox;
	}
}
