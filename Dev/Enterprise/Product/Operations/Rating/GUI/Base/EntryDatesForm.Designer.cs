
namespace Enterprise.Rating.GUI
{
	partial class EntryDatesForm : Enterprise.ZArchitecture.GUI.ZChildForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CancelXButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.UpdateStartCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UpdateEndtCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.EntryStartEndDates);
			// 
			// CancelXButton
			// 
			this.CancelXButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EntryDatesForm|950942b6-ce7c-49d9-9c39-f160a591bb89", "Cancel");
			this.CancelXButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelXButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 75, true);
			this.CancelXButton.Name = "CancelXButton";
			this.CancelXButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelXButton.TabIndex = 5;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EntryDatesForm|8b07e5f5-0ffd-48d4-a90d-7603dc0755c5", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 75, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "Start");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.EntryStartEndDates)(null)).Start)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StartDateEdit, false);
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 12, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 1;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "End");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.EntryStartEndDates)(null)).End)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EndDateEdit, false);
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 38, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 3;
			// 
			// UpdateStartCheckBox
			// 
			this.UpdateStartCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UpdateStartCheckBox, "UpdateStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.EntryStartEndDates)(null)).UpdateStart)));
			this.UpdateStartCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EntryDatesForm|3d6b8b34-04ca-4e06-b66f-37cdf036ec29", "Update Start Date");
			this.UpdateStartCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpdateStartCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 15, true);
			this.UpdateStartCheckBox.Name = "UpdateStartCheckBox";
			this.UpdateStartCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UpdateStartCheckBox.TabIndex = 0;
			this.UpdateStartCheckBox.UseVisualStyleBackColor = true;
			// 
			// UpdateEndtCheckBox
			// 
			this.UpdateEndtCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UpdateEndtCheckBox, "UpdateEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.EntryStartEndDates)(null)).UpdateEnd)));
			this.UpdateEndtCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("EntryDatesForm|14e502b0-0e13-4198-b1c6-8b6793f811c6", "Update End Date");
			this.UpdateEndtCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UpdateEndtCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 41, true);
			this.UpdateEndtCheckBox.Name = "UpdateEndtCheckBox";
			this.UpdateEndtCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UpdateEndtCheckBox.TabIndex = 2;
			this.UpdateEndtCheckBox.UseVisualStyleBackColor = true;
			// 
			// EntryDatesForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelXButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 131, true);
			this.Controls.Add(this.UpdateEndtCheckBox);
			this.Controls.Add(this.UpdateStartCheckBox);
			this.Controls.Add(this.EndDateEdit);
			this.Controls.Add(this.CancelXButton);
			this.Controls.Add(this.StartDateEdit);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(Enterprise.Rating.Business.EntryStartEndDates);
			this.DataSourceTypeName = "Enterprise.Rating.Business.EntryStartEndDates";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "EntryDatesForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = " Start & End";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.StartDateEdit, 0);
			this.Controls.SetChildIndex(this.CancelXButton, 0);
			this.Controls.SetChildIndex(this.EndDateEdit, 0);
			this.Controls.SetChildIndex(this.UpdateStartCheckBox, 0);
			this.Controls.SetChildIndex(this.UpdateEndtCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CancelXButton;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit StartDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit EndDateEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UpdateStartCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UpdateEndtCheckBox;
	}
}
