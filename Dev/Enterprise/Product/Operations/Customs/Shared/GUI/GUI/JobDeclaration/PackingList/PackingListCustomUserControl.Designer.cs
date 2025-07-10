namespace Enterprise.Customs.GUI
{
	partial class PackingListCustomUserControl
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
			this.CustomAttribute1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomAttribute2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomFlag1CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CustomFlag2CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CustomDate1DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CustomDate2DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CustomDecimal1CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomDecimal2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomAttribute1Label = new Enterprise.ZArchitecture.ZLabel();
			this.CustomAttribute2Label = new Enterprise.ZArchitecture.ZLabel();
			this.CustomFlag1Label = new Enterprise.ZArchitecture.ZLabel();
			this.CustomFlag2Label = new Enterprise.ZArchitecture.ZLabel();
			this.CustomDate1Label = new Enterprise.ZArchitecture.ZLabel();
			this.CustomDate2Label = new Enterprise.ZArchitecture.ZLabel();
			this.CustomDecimal1Label = new Enterprise.ZArchitecture.ZLabel();
			this.CustomDecimal2Label = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomDate1DateEdit.SuspendLayout();
			this.CustomDate2DateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusPackingList);
			// 
			// CustomAttribute1TextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomAttribute1TextBox, "CUL_CustomAttribute1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomAttribute1)));
			this.CustomAttribute1TextBox.CaptionResourceString = null;
			this.CustomAttribute1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 12, true);
			this.CustomAttribute1TextBox.Name = "CustomAttribute1TextBox";
			this.CustomAttribute1TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CustomAttribute1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CustomAttribute1TextBox.TabIndex = 1;
			// 
			// CustomAttribute2TextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomAttribute2TextBox, "CUL_CustomAttribute2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomAttribute2)));
			this.CustomAttribute2TextBox.CaptionResourceString = null;
			this.CustomAttribute2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 38, true);
			this.CustomAttribute2TextBox.Name = "CustomAttribute2TextBox";
			this.CustomAttribute2TextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CustomAttribute2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CustomAttribute2TextBox.TabIndex = 3;
			// 
			// CustomFlag1CheckBox
			// 
			this.BindingSource.SetBindingMember(this.CustomFlag1CheckBox, "CUL_CustomFlag1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomFlag1)));
			this.CustomFlag1CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomFlag1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 64, true);
			this.CustomFlag1CheckBox.Name = "CustomFlag1CheckBox";
			this.CustomFlag1CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.CustomFlag1CheckBox.TabIndex = 5;
			this.CustomFlag1CheckBox.UseVisualStyleBackColor = true;
			// 
			// CustomFlag2CheckBox
			// 
			this.BindingSource.SetBindingMember(this.CustomFlag2CheckBox, "CUL_CustomFlag2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomFlag2)));
			this.CustomFlag2CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomFlag2CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 84, true);
			this.CustomFlag2CheckBox.Name = "CustomFlag2CheckBox";
			this.CustomFlag2CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 17, true);
			this.CustomFlag2CheckBox.TabIndex = 7;
			this.CustomFlag2CheckBox.UseVisualStyleBackColor = true;
			// 
			// CustomDate1DateEdit
			// 
			this.CustomDate1DateEdit.AllowDrop = true;
			this.CustomDate1DateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomDate1DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomDate1DateEdit, "CUL_CustomDate1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomDate1)));
			this.CustomDate1DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 104, true);
			this.CustomDate1DateEdit.Name = "CustomDate1DateEdit";
			this.CustomDate1DateEdit.TabIndex = 9;
			// 
			// CustomDate2DateEdit
			// 
			this.CustomDate2DateEdit.AllowDrop = true;
			this.CustomDate2DateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomDate2DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomDate2DateEdit, "CUL_CustomDate2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomDate2)));
			this.CustomDate2DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 130, true);
			this.CustomDate2DateEdit.Name = "CustomDate2DateEdit";
			this.CustomDate2DateEdit.TabIndex = 11;
			// 
			// CustomDecimal1CalcEdit
			// 
			this.CustomDecimal1CalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CustomDecimal1CalcEdit, "CUL_CustomDecimal1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomDecimal1)));
			this.CustomDecimal1CalcEdit.CaptionResourceString = null;
			this.CustomDecimal1CalcEdit.DecimalPlaces = 0;
			this.CustomDecimal1CalcEdit.Decimals = 0;
			this.CustomDecimal1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 156, true);
			this.CustomDecimal1CalcEdit.Name = "CustomDecimal1CalcEdit";
			this.CustomDecimal1CalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.CustomDecimal1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.CustomDecimal1CalcEdit.TabIndex = 13;
			this.CustomDecimal1CalcEdit.Text = "0";
			this.CustomDecimal1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomDecimal2CalcEdit
			// 
			this.CustomDecimal2CalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CustomDecimal2CalcEdit, "CUL_CustomDecimal2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusPackingList)(null)).CUL_CustomDecimal2)));
			this.CustomDecimal2CalcEdit.CaptionResourceString = null;
			this.CustomDecimal2CalcEdit.DecimalPlaces = 0;
			this.CustomDecimal2CalcEdit.Decimals = 0;
			this.CustomDecimal2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 182, true);
			this.CustomDecimal2CalcEdit.Name = "CustomDecimal2CalcEdit";
			this.CustomDecimal2CalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.CustomDecimal2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.CustomDecimal2CalcEdit.TabIndex = 15;
			this.CustomDecimal2CalcEdit.Text = "0";
			this.CustomDecimal2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomAttribute1Label
			// 
			this.CustomAttribute1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomAttribute1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 15, true);
			this.CustomAttribute1Label.Name = "CustomAttribute1Label";
			this.CustomAttribute1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomAttribute1Label.TabIndex = 0;
			this.CustomAttribute1Label.Text = "Custom Attribute 1";
			this.CustomAttribute1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomAttribute2Label
			// 
			this.CustomAttribute2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomAttribute2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 39, true);
			this.CustomAttribute2Label.Name = "CustomAttribute2Label";
			this.CustomAttribute2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomAttribute2Label.TabIndex = 2;
			this.CustomAttribute2Label.Text = "Custom Attribute 2";
			this.CustomAttribute2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomFlag1Label
			// 
			this.CustomFlag1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomFlag1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 63, true);
			this.CustomFlag1Label.Name = "CustomFlag1Label";
			this.CustomFlag1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomFlag1Label.TabIndex = 4;
			this.CustomFlag1Label.Text = "Custom Flag 1";
			this.CustomFlag1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomFlag2Label
			// 
			this.CustomFlag2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomFlag2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 84, true);
			this.CustomFlag2Label.Name = "CustomFlag2Label";
			this.CustomFlag2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomFlag2Label.TabIndex = 6;
			this.CustomFlag2Label.Text = "Custom Flag 2";
			this.CustomFlag2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomDate1Label
			// 
			this.CustomDate1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomDate1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 107, true);
			this.CustomDate1Label.Name = "CustomDate1Label";
			this.CustomDate1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomDate1Label.TabIndex = 8;
			this.CustomDate1Label.Text = "Custom Date 1";
			this.CustomDate1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomDate2Label
			// 
			this.CustomDate2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomDate2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 133, true);
			this.CustomDate2Label.Name = "CustomDate2Label";
			this.CustomDate2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomDate2Label.TabIndex = 10;
			this.CustomDate2Label.Text = "Custom Date 2";
			this.CustomDate2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomDecimal1Label
			// 
			this.CustomDecimal1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomDecimal1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 159, true);
			this.CustomDecimal1Label.Name = "CustomDecimal1Label";
			this.CustomDecimal1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomDecimal1Label.TabIndex = 12;
			this.CustomDecimal1Label.Text = "Custom Decimal 1";
			this.CustomDecimal1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomDecimal2Label
			// 
			this.CustomDecimal2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomDecimal2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 185, true);
			this.CustomDecimal2Label.Name = "CustomDecimal2Label";
			this.CustomDecimal2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			this.CustomDecimal2Label.TabIndex = 14;
			this.CustomDecimal2Label.Text = "Custom Decimal 2";
			this.CustomDecimal2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PackingListCustomUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomDecimal2Label);
			this.Controls.Add(this.CustomDecimal1Label);
			this.Controls.Add(this.CustomDate2Label);
			this.Controls.Add(this.CustomDate1Label);
			this.Controls.Add(this.CustomFlag2Label);
			this.Controls.Add(this.CustomFlag1Label);
			this.Controls.Add(this.CustomAttribute2Label);
			this.Controls.Add(this.CustomAttribute1Label);
			this.Controls.Add(this.CustomDecimal2CalcEdit);
			this.Controls.Add(this.CustomDecimal1CalcEdit);
			this.Controls.Add(this.CustomDate2DateEdit);
			this.Controls.Add(this.CustomDate1DateEdit);
			this.Controls.Add(this.CustomFlag2CheckBox);
			this.Controls.Add(this.CustomFlag1CheckBox);
			this.Controls.Add(this.CustomAttribute2TextBox);
			this.Controls.Add(this.CustomAttribute1TextBox);
			this.Name = "PackingListCustomUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 387, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomDate1DateEdit.ResumeLayout(true);
			this.CustomDate1DateEdit.PerformLayout();
			this.CustomDate2DateEdit.ResumeLayout(true);
			this.CustomDate2DateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CustomAttribute1TextBox;
		private ZArchitecture.ZTextBox CustomAttribute2TextBox;
		private ZArchitecture.ZCalcEdit CustomDecimal1CalcEdit;
		private ZArchitecture.ZCalcEdit CustomDecimal2CalcEdit;
		private ZArchitecture.GUI.ZCheckBox CustomFlag1CheckBox;
		private ZArchitecture.GUI.ZCheckBox CustomFlag2CheckBox;
		private ZArchitecture.GUI.ZDateEdit CustomDate1DateEdit;
		private ZArchitecture.GUI.ZDateEdit CustomDate2DateEdit;
		private ZArchitecture.ZLabel CustomAttribute1Label;
		private ZArchitecture.ZLabel CustomAttribute2Label;
		private ZArchitecture.ZLabel CustomFlag1Label;
		private ZArchitecture.ZLabel CustomFlag2Label;
		private ZArchitecture.ZLabel CustomDate1Label;
		private ZArchitecture.ZLabel CustomDate2Label;
		private ZArchitecture.ZLabel CustomDecimal1Label;
		private ZArchitecture.ZLabel CustomDecimal2Label;
	}
}
