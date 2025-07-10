using System.Runtime.Serialization;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.NCTS.GUI
{
	partial class DeclarationDetailsFallbackUserControl
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.FallbackDateTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FallbackReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// ClearanceProcedureTextBox
			// 
			this.BindingSource.SetBindingMember(this.FallbackReferenceTextBox, "FallbackReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.NCTS.Business.NctsDepartureMovementHeader)(null)).FallbackReference)));
			this.FallbackReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FallbackReferenceTextBox.Name = "FallbackReferenceTextBox";
			this.FallbackReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FallbackReferenceTextBox.TabIndex = 1;
			// 
			// FallbackDateTimeEdit
			// 
			this.FallbackDateTimeEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FallbackDateTimeEdit, "FallbackTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.NL.NCTS.Business.NctsDepartureMovementHeader)(null)).FallbackTime)));
			this.FallbackDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 0, true);
			this.FallbackDateTimeEdit.Name = "FallbackDateTimeEdit";
			this.FallbackDateTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FallbackDateTimeEdit.TabIndex = 2;
			// 
			// DeclarationDetailsFallbackUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FallbackReferenceTextBox);
			this.Controls.Add(this.FallbackDateTimeEdit);
			this.Name = "DeclarationDetailsFallbackUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FallbackDateTimeEdit.ResumeLayout(true);
			this.FallbackDateTimeEdit.PerformLayout();
			this.FallbackReferenceTextBox.ResumeLayout(true);
			this.FallbackReferenceTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZTextBox FallbackReferenceTextBox;
		internal ZArchitecture.GUI.ZDateEdit FallbackDateTimeEdit;
	}
}
