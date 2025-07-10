using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class ReSequencerForm
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
		protected new void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StartSequenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SequenceStepCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 94, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ReSequencer);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("383328a5-a92d-4579-bf45-a2bff0d1018f", "&OK");
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 64, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.ProceedButton_Click);
			// 
			// StartSequenceCalcEdit
			// 
			this.StartSequenceCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StartSequenceCalcEdit, "StartSequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ReSequencer)(null)).StartSequence)));
			this.StartSequenceCalcEdit.CaptionResourceString = null;
			this.StartSequenceCalcEdit.DecimalPlaces = 2;
			this.StartSequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 12, true);
			this.StartSequenceCalcEdit.Name = "StartSequenceCalcEdit";
			this.StartSequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.StartSequenceCalcEdit.TabIndex = 0;
			this.StartSequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SequenceStepCalcEdit
			// 
			this.SequenceStepCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequenceStepCalcEdit, "SequenceStep");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ReSequencer)(null)).SequenceStep)));
			this.SequenceStepCalcEdit.CaptionResourceString = null;
			this.SequenceStepCalcEdit.DecimalPlaces = 2;
			this.SequenceStepCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 38, true);
			this.SequenceStepCalcEdit.Name = "SequenceStepCalcEdit";
			this.SequenceStepCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SequenceStepCalcEdit.TabIndex = 1;
			this.SequenceStepCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9ff4751f-524a-48a4-a76f-58fb60d6b6bf", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 64, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ReSequencerForm
			// 
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 118, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.SequenceStepCalcEdit);
			this.Controls.Add(this.StartSequenceCalcEdit);
			this.Controls.Add(this.OKButton);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ReSequencer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ReSequencerForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.StartSequenceCalcEdit, 0);
			this.Controls.SetChildIndex(this.SequenceStepCalcEdit, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZButton OKButton;
		ZCalcEdit StartSequenceCalcEdit;
		private ZCalcEdit SequenceStepCalcEdit;
		private ZButton cancelButton;
	}
}