
namespace Enterprise.Customs.TW.GUI
{
	partial class AircraftPartsUserControl
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
			this.CAACodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SequenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EnglishDescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.ChineseDescriptionLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.DeclarationGoodsDescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AircraftDeclarationGoodsDecriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OverirdeDefaultCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IPCGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CAACodeGroupBox.SuspendLayout();
			this.SequenceDropEdit.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			this.EnglishDescriptionLongTextControl.SuspendLayout();
			this.ChineseDescriptionLongTextControl.SuspendLayout();
			this.DeclarationGoodsDescriptionGroupBox.SuspendLayout();
			this.IPCGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// CAACodeGroupBox
			// 
			this.CAACodeGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("e00680ca-8b82-424b-a727-6f7decbd8c22", "CAA Code");
			this.CAACodeGroupBox.Controls.Add(this.SequenceDropEdit);
			this.CAACodeGroupBox.Controls.Add(this.CategoryDropEdit);
			this.CAACodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.CAACodeGroupBox.Name = "CAACodeGroupBox";
			this.CAACodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 74, true);
			this.CAACodeGroupBox.TabIndex = 0;
			this.CAACodeGroupBox.TabStop = false;
			// 
			// SequenceDropEdit
			// 
			this.SequenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SequenceDropEdit, "FilteredInvoiceLines.AddInfoChild.TWL_AircraftPartsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoChild.TWL_AircraftPartsCode)));
			this.SequenceDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("91fc5bd4-2a11-4c39-9b87-4056eaea1968", "Sequence");
			this.SequenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 45, true);
			this.SequenceDropEdit.Name = "SequenceDropEdit";
			this.SequenceDropEdit.PreBoundMaxLength = 3;
			this.SequenceDropEdit.ShouldResizeByMaxLength = false;
			this.SequenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.SequenceDropEdit.TabIndex = 6;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "FilteredInvoiceLines.AddInfoChild.TWL_AircraftPartsCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoChild.TWL_AircraftPartsCategory)));
			this.CategoryDropEdit.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("339a45f2-cab0-454d-bdc2-79f3e9001ced", "Category");
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 19, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.PreBoundMaxLength = 3;
			this.CategoryDropEdit.ShouldResizeByMaxLength = false;
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.CategoryDropEdit.TabIndex = 5;
			// 
			// EnglishDescriptionLongTextControl
			// 
			this.EnglishDescriptionLongTextControl.AllowDrop = true;
			this.EnglishDescriptionLongTextControl.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("41c0aed9-527f-4789-8cc8-882307c8a56b", "English Description");
			this.EnglishDescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EnglishDescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 27, true);
			this.EnglishDescriptionLongTextControl.Name = "EnglishDescriptionLongTextControl";
			this.EnglishDescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.EnglishDescriptionLongTextControl.TabIndex = 1;
			// 
			// ChineseDescriptionLongTextControl
			// 
			this.ChineseDescriptionLongTextControl.AllowDrop = true;
			this.ChineseDescriptionLongTextControl.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("33beb2ea-742b-4cce-8b7c-2a055ca94459", "Chinese Description");
			this.ChineseDescriptionLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ChineseDescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 53, true);
			this.ChineseDescriptionLongTextControl.Name = "ChineseDescriptionLongTextControl";
			this.ChineseDescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ChineseDescriptionLongTextControl.TabIndex = 2;
			// 
			// DeclarationGoodsDescriptionGroupBox
			// 
			this.DeclarationGoodsDescriptionGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("2469bd27-40a7-4cd0-9323-5cccca6bf1fa", "Declaration Goods Description");
			this.DeclarationGoodsDescriptionGroupBox.Controls.Add(this.AircraftDeclarationGoodsDecriptionTextBox);
			this.DeclarationGoodsDescriptionGroupBox.Controls.Add(this.OverirdeDefaultCheckBox);
			this.DeclarationGoodsDescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 88, true);
			this.DeclarationGoodsDescriptionGroupBox.Name = "DeclarationGoodsDescriptionGroupBox";
			this.DeclarationGoodsDescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 150, true);
			this.DeclarationGoodsDescriptionGroupBox.TabIndex = 4;
			this.DeclarationGoodsDescriptionGroupBox.TabStop = false;
			// 
			// AircraftDeclarationGoodsDecriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.AircraftDeclarationGoodsDecriptionTextBox, "FilteredInvoiceLines.JI_DeclarationGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DeclarationGoodsDescription)));
			this.AircraftDeclarationGoodsDecriptionTextBox.CaptionResourceString = null;
			this.AircraftDeclarationGoodsDecriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AircraftDeclarationGoodsDecriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AircraftDeclarationGoodsDecriptionTextBox, false);
			this.AircraftDeclarationGoodsDecriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			this.AircraftDeclarationGoodsDecriptionTextBox.Multiline = true;
			this.AircraftDeclarationGoodsDecriptionTextBox.Name = "AircraftDeclarationGoodsDecriptionTextBox";
			this.AircraftDeclarationGoodsDecriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AircraftDeclarationGoodsDecriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.AircraftDeclarationGoodsDecriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 107, true);
			this.AircraftDeclarationGoodsDecriptionTextBox.TabIndex = 1;
			// 
			// OverirdeDefaultCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverirdeDefaultCheckBox, "FilteredInvoiceLines.OverrideDeclarationGoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).OverrideDeclarationGoodsDescription)));
			this.OverirdeDefaultCheckBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("f2944677-27c3-4008-b781-77023528e362", "Override Default");
			this.OverirdeDefaultCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.OverirdeDefaultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverirdeDefaultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OverirdeDefaultCheckBox.Name = "OverirdeDefaultCheckBox";
			this.OverirdeDefaultCheckBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.OverirdeDefaultCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 24, true);
			this.OverirdeDefaultCheckBox.TabIndex = 0;
			this.OverirdeDefaultCheckBox.UseVisualStyleBackColor = true;
			// 
			// IPCGroupBox
			// 
			this.IPCGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("5b279647-4bff-4d7f-8605-622bcb3801c6", "IPC");
			this.IPCGroupBox.Controls.Add(this.zTextBox1);
			this.IPCGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 88, true);
			this.IPCGroupBox.Name = "IPCGroupBox";
			this.IPCGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 150, true);
			this.IPCGroupBox.TabIndex = 3;
			this.IPCGroupBox.TabStop = false;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "FilteredInvoiceLines.AddInfoChild.TWL_AircraftIPC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoChild.TWL_AircraftIPC)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.ShouldEscapeAllSpecialCharacters = false;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 131, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// AircraftPartsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IPCGroupBox);
			this.Controls.Add(this.DeclarationGoodsDescriptionGroupBox);
			this.Controls.Add(this.ChineseDescriptionLongTextControl);
			this.Controls.Add(this.EnglishDescriptionLongTextControl);
			this.Controls.Add(this.CAACodeGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 335, true);
			this.Name = "AircraftPartsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(974, 399, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CAACodeGroupBox.ResumeLayout(false);
			this.CAACodeGroupBox.PerformLayout();
			this.SequenceDropEdit.ResumeLayout(true);
			this.SequenceDropEdit.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			this.EnglishDescriptionLongTextControl.ResumeLayout(true);
			this.EnglishDescriptionLongTextControl.PerformLayout();
			this.ChineseDescriptionLongTextControl.ResumeLayout(true);
			this.ChineseDescriptionLongTextControl.PerformLayout();
			this.DeclarationGoodsDescriptionGroupBox.ResumeLayout(false);
			this.DeclarationGoodsDescriptionGroupBox.PerformLayout();
			this.IPCGroupBox.ResumeLayout(false);
			this.IPCGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CAACodeGroupBox;
		private ZArchitecture.GUI.ZDropEdit SequenceDropEdit;
		private ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		private Customs.GUI.LongTextControl EnglishDescriptionLongTextControl;
		private Customs.GUI.LongTextControl ChineseDescriptionLongTextControl;
		private ZArchitecture.GUI.ZGroupBox DeclarationGoodsDescriptionGroupBox;
		private ZArchitecture.ZTextBox AircraftDeclarationGoodsDecriptionTextBox;
		private ZArchitecture.GUI.ZCheckBox OverirdeDefaultCheckBox;
		private ZArchitecture.GUI.ZGroupBox IPCGroupBox;
		private ZArchitecture.ZTextBox zTextBox1;
	}
}
