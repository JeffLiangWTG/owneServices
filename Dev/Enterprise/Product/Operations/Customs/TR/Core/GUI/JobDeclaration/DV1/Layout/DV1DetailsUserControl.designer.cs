using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	partial class DV1DetailsUserControl
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
			this.PlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsDecisionDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsDecisionDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// PlaceTextBox
			//
			this.BindingSource.SetBindingMember(this.PlaceTextBox, "DV1Details.DV1_Place");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_Place)));
			this.PlaceTextBox.CaptionResourceString = null;
			this.PlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 191, true);
			this.PlaceTextBox.Name = "PlaceTextBox";
			this.PlaceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1202, 20, true);
			this.PlaceTextBox.TabIndex = 12;
			// 
			// CustomsDecisionDateDateEdit
			// 
			this.CustomsDecisionDateDateEdit.AllowDrop = true;
			this.CustomsDecisionDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomsDecisionDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomsDecisionDateDateEdit, "DV1Details.DV1_CustomsDecisionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_CustomsDecisionDate)));
			this.CustomsDecisionDateDateEdit.CaptionResourceString = null;
			this.CustomsDecisionDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 214, true);
			this.CustomsDecisionDateDateEdit.Name = "CustomsDecisionDateDateEdit";
			this.CustomsDecisionDateDateEdit.TabIndex = 13;
			// 
			// DV1DetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PlaceTextBox);
			this.Controls.Add(this.CustomsDecisionDateDateEdit);
			this.Name = "DV1DetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1361, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsDecisionDateDateEdit.ResumeLayout(true);
			this.CustomsDecisionDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox PlaceTextBox;
		internal ZDateEdit CustomsDecisionDateDateEdit;
	}
}
