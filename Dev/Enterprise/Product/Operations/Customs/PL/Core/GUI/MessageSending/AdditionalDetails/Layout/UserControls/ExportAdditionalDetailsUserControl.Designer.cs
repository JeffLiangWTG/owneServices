using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI
{
	partial class ExportAdditionalDetailsUserControl
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
			this.SecurityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AmendmentInvalidationReasonUserControl = new Enterprise.Customs.PL.GUI.AmendmentInvalidationReasonUserControl();
			this.CorrectionAcceptanceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AcceptanceCommentUserControl = new Enterprise.Customs.PL.GUI.AcceptanceCommentUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SecurityDropEdit.SuspendLayout();
			this.AmendmentInvalidationReasonUserControl.SuspendLayout();
			this.CorrectionAcceptanceDropEdit.SuspendLayout();
			this.AcceptanceCommentUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObject);
			// 
			// SecurityDropEdit
			// 
			this.SecurityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityDropEdit, "Security");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.BaseMessageSendingObject)(null)).Security)));
			this.SecurityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.SecurityDropEdit.Name = "SecurityDropEdit";
			this.SecurityDropEdit.PreBoundMaxLength = 1;
			this.SecurityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.SecurityDropEdit.TabIndex = 1;
			// 
			// AmendmentInvalidationReasonUserControl
			// 
			this.AmendmentInvalidationReasonUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendmentInvalidationReasonUserControl, ".");
			this.AmendmentInvalidationReasonUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AmendmentInvalidationReasonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 133, true);
			this.AmendmentInvalidationReasonUserControl.Name = "AmendmentInvalidationReasonUserControl";
			this.AmendmentInvalidationReasonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 123, true);
			this.AmendmentInvalidationReasonUserControl.TabIndex = 2;
			// 
			// CorrectionAcceptanceDropEdit
			// 
			this.CorrectionAcceptanceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CorrectionAcceptanceDropEdit, "CorrectionAcceptance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.BaseMessageSendingObject)(null)).CorrectionAcceptance)));
			this.CorrectionAcceptanceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.CorrectionAcceptanceDropEdit.Name = "CorrectionAcceptanceDropEdit";
			this.CorrectionAcceptanceDropEdit.PreBoundMaxLength = 1;
			this.CorrectionAcceptanceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
			this.CorrectionAcceptanceDropEdit.TabIndex = 3;
			// 
			// AcceptanceCommentUserControl
			// 
			this.AcceptanceCommentUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AcceptanceCommentUserControl, ".");
			this.AcceptanceCommentUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AcceptanceCommentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.AcceptanceCommentUserControl.Name = "AcceptanceCommentUserControl";
			this.AcceptanceCommentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 111, true);
			this.AcceptanceCommentUserControl.TabIndex = 4;
			// 
			// ExportAdditionalDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AcceptanceCommentUserControl);
			this.Controls.Add(this.CorrectionAcceptanceDropEdit);
			this.Controls.Add(this.AmendmentInvalidationReasonUserControl);
			this.Controls.Add(this.SecurityDropEdit);
			this.Name = "ExportAdditionalDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 255, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SecurityDropEdit.ResumeLayout(true);
			this.SecurityDropEdit.PerformLayout();
			this.AmendmentInvalidationReasonUserControl.ResumeLayout(true);
			this.AmendmentInvalidationReasonUserControl.PerformLayout();
			this.CorrectionAcceptanceDropEdit.ResumeLayout(true);
			this.CorrectionAcceptanceDropEdit.PerformLayout();
			this.AcceptanceCommentUserControl.ResumeLayout(true);
			this.AcceptanceCommentUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZDropEdit SecurityDropEdit;
		internal AmendmentInvalidationReasonUserControl AmendmentInvalidationReasonUserControl;
		internal ZDropEdit CorrectionAcceptanceDropEdit;
		internal AcceptanceCommentUserControl AcceptanceCommentUserControl;
	}
}
