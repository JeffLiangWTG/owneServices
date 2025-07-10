using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	partial class DeferredSubmissionForm
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
		protected override void InitializeComponent()
		{
			this.SubmitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelSubmissionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SubmissionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DeferredAccountDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverwriteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArrivalDateEdit.SuspendLayout();
			this.SubmissionDateEdit.SuspendLayout();
			this.DeferredAccountDropEdit.SuspendLayout();
			this.PaymentMethodDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 24, true);
			this.MainStatusBar.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.DeferredSubmission);
			// 
			// SubmitButton
			// 
			this.SubmitButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f0037053-1b14-4036-9ce1-1a73e41ed42a", "Submit");
			this.SubmitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 122, true);
			this.SubmitButton.Name = "SubmitButton";
			this.SubmitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SubmitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.SubmitButton.TabIndex = 5;
			this.SubmitButton.ToolTipCaption = null;
			this.SubmitButton.UseVisualStyleBackColor = true;
			this.SubmitButton.Click += new System.EventHandler(this.SubmitButton_Click);
			// 
			// CancelSubmissionButton
			// 
			this.CancelSubmissionButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("95607c6e-c724-4937-895f-c671af821bdc", "Cancel");
			this.CancelSubmissionButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelSubmissionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 122, true);
			this.CancelSubmissionButton.Name = "CancelSubmissionButton";
			this.CancelSubmissionButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelSubmissionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.CancelSubmissionButton.TabIndex = 6;
			this.CancelSubmissionButton.ToolTipCaption = null;
			this.CancelSubmissionButton.UseVisualStyleBackColor = true;
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "DateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.DeferredSubmission)(null)).DateOfArrival)));
			this.ArrivalDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7e174ad1-bea4-4961-9833-fae6140290da", "Date of Arrival");
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 12, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 0;
			this.ArrivalDateEdit.TabStop = false;
			// 
			// SubmissionDateEdit
			// 
			this.SubmissionDateEdit.AllowDrop = true;
			this.SubmissionDateEdit.AutoCompleteMonthThreshold = 1;
			this.SubmissionDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SubmissionDateEdit, "SubmissionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.DeferredSubmission)(null)).SubmissionDate)));
			this.SubmissionDateEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("93bafd06-6251-4ae0-9613-73c5eafd26de", "Submission Date");
			this.SubmissionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 38, true);
			this.SubmissionDateEdit.Name = "SubmissionDateEdit";
			this.SubmissionDateEdit.TabIndex = 2;
			// 
			// DeferredAccountDropEdit
			// 
			this.DeferredAccountDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeferredAccountDropEdit, "DeferredAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.DeferredSubmission)(null)).DeferredAccount)));
			this.DeferredAccountDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("b9238c44-ba40-408d-a78b-4ae771bfb901", "Deferred Account");
			this.DeferredAccountDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 89, true);
			this.DeferredAccountDropEdit.Name = "DeferredAccountDropEdit";
			this.DeferredAccountDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 20, true);
			this.DeferredAccountDropEdit.TabIndex = 4;
			// 
			// PaymentMethodDropEdit
			// 
			this.PaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentMethodDropEdit, "PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.DeferredSubmission)(null)).PaymentMethod)));
			this.PaymentMethodDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("s4a5d7fc5-18db-4568-a709-e79cabb03c39", "Payment Method");
			this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 64, true);
			this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
			this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PaymentMethodDropEdit.TabIndex = 3;
			// 
			// OverwriteCheckBox
			// 
			this.OverwriteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverwriteCheckBox, "IsOverwritten");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ZA.Business.DeferredSubmission)(null)).IsOverwritten)));
			this.OverwriteCheckBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3212abec-62b2-48d3-8361-a568ea53b75e", "Overwrite Default Values");
			this.OverwriteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverwriteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 14, true);
			this.OverwriteCheckBox.Name = "OverwriteCheckBox";
			this.OverwriteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 17, true);
			this.OverwriteCheckBox.TabIndex = 1;
			// 
			// DeferredSubmissionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelSubmissionButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 182, true);
			this.Controls.Add(this.PaymentMethodDropEdit);
			this.Controls.Add(this.DeferredAccountDropEdit);
			this.Controls.Add(this.ArrivalDateEdit);
			this.Controls.Add(this.SubmissionDateEdit);
			this.Controls.Add(this.CancelSubmissionButton);
			this.Controls.Add(this.SubmitButton);
			this.Controls.Add(this.OverwriteCheckBox);
			this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.DeferredSubmission);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "DeferredSubmissionForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.OverwriteCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SubmitButton, 0);
			this.Controls.SetChildIndex(this.CancelSubmissionButton, 0);
			this.Controls.SetChildIndex(this.SubmissionDateEdit, 0);
			this.Controls.SetChildIndex(this.ArrivalDateEdit, 0);
			this.Controls.SetChildIndex(this.DeferredAccountDropEdit, 0);
			this.Controls.SetChildIndex(this.PaymentMethodDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArrivalDateEdit.ResumeLayout(true);
			this.ArrivalDateEdit.PerformLayout();
			this.SubmissionDateEdit.ResumeLayout(true);
			this.SubmissionDateEdit.PerformLayout();
			this.DeferredAccountDropEdit.ResumeLayout(true);
			this.DeferredAccountDropEdit.PerformLayout();
			this.PaymentMethodDropEdit.ResumeLayout(true);
			this.PaymentMethodDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZDateEdit ArrivalDateEdit;
		private ZDateEdit SubmissionDateEdit;
		private ZDropEdit DeferredAccountDropEdit;
		private ZDropEdit PaymentMethodDropEdit;
		private ZCheckBox OverwriteCheckBox;
		private ZButton SubmitButton;
		private ZButton CancelSubmissionButton;
	}
}
