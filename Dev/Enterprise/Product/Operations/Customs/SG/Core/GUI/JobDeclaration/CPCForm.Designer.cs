using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class CPCForm : ZChildForm
	{
		public ZButton OKButton;
		ZArchitecture.ZTextBox aPCDescription;
		ZArchitecture.ZTextBox pC1;
		ZArchitecture.ZTextBox pC2;
		ZArchitecture.ZLabel cPCLabel1;
		ZArchitecture.ZLabel cPCLabel2;
		ZArchitecture.ZLabel cPCLabel3;
		ZArchitecture.ZTextBox zTextBox1;
		ZArchitecture.ZTextBox zTextBox2;
		ZArchitecture.ZTextBox zTextBox3;
		ZArchitecture.ZTextBox zTextBox4;
		ZArchitecture.ZTextBox zTextBox5;
		ZArchitecture.ZTextBox zTextBox6;
		ZArchitecture.ZTextBox zTextBox7;
		ZArchitecture.ZTextBox zTextBox8;
		ZArchitecture.ZTextBox zTextBox9;
		ZArchitecture.ZTextBox zTextBox10;
		ZArchitecture.ZTextBox zTextBox11;
		ZArchitecture.ZTextBox zTextBox12;
		ZArchitecture.ZTextBox zTextBox13;
		ZArchitecture.ZTextBox pC3;

		new void InitializeComponent()
		{
			this.OKButton = new ZButton();
			this.aPCDescription = new ZArchitecture.ZTextBox();
			this.pC1 = new ZArchitecture.ZTextBox();
			this.pC2 = new ZArchitecture.ZTextBox();
			this.pC3 = new ZArchitecture.ZTextBox();
			this.cPCLabel1 = new ZArchitecture.ZLabel();
			this.cPCLabel2 = new ZArchitecture.ZLabel();
			this.cPCLabel3 = new ZArchitecture.ZLabel();
			this.zTextBox1 = new ZArchitecture.ZTextBox();
			this.zTextBox2 = new ZArchitecture.ZTextBox();
			this.zTextBox3 = new ZArchitecture.ZTextBox();
			this.zTextBox4 = new ZArchitecture.ZTextBox();
			this.zTextBox5 = new ZArchitecture.ZTextBox();
			this.zTextBox6 = new ZArchitecture.ZTextBox();
			this.zTextBox7 = new ZArchitecture.ZTextBox();
			this.zTextBox8 = new ZArchitecture.ZTextBox();
			this.zTextBox9 = new ZArchitecture.ZTextBox();
			this.zTextBox10 = new ZArchitecture.ZTextBox();
			this.zTextBox11 = new ZArchitecture.ZTextBox();
			this.zTextBox12 = new ZArchitecture.ZTextBox();
			this.zTextBox13 = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 269, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(SGCPC);
			// 
			// OKButton
			//
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CPCForm|A815C985-6048-4E62-8864-CAE2C4083195", "&OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(620, 240, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 26;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// APCDescription
			// 
			this.BindingSource.SetBindingMember(this.aPCDescription, "SG_APCCodeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_APCCodeDescription)));
			this.aPCDescription.CaptionResourceString = null;
			this.aPCDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 30, true);
			this.aPCDescription.Name = "APCDescription";
			this.aPCDescription.ReadOnly = true;
			this.aPCDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.aPCDescription.TabIndex = 0;
			// 
			// PC1
			// 
			this.BindingSource.SetBindingMember(this.pC1, "SG_PC1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC1)));
			this.pC1.CaptionResourceString = null;
			this.pC1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 56, true);
			this.pC1.Name = "PC1";
			this.pC1.ReadOnly = true;
			this.pC1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.pC1.TabIndex = 2;
			// 
			// PC2
			// 
			this.BindingSource.SetBindingMember(this.pC2, "SG_PC2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC2)));
			this.pC2.CaptionResourceString = null;
			this.pC2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 56, true);
			this.pC2.Name = "PC2";
			this.pC2.ReadOnly = true;
			this.pC2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.pC2.TabIndex = 3;
			// 
			// PC3
			// 
			this.BindingSource.SetBindingMember(this.pC3, "SG_PC3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC3)));
			this.pC3.CaptionResourceString = null;
			this.pC3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 56, true);
			this.pC3.Name = "PC3";
			this.pC3.ReadOnly = true;
			this.pC3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.pC3.TabIndex = 4;
			// 
			// CPCLabel1
			// 
			this.cPCLabel1.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.cPCLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 28, true);
			this.cPCLabel1.Name = "CPCLabel1";
			this.cPCLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 23, true);
			this.cPCLabel1.TabIndex = 12;
			this.cPCLabel1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CPCForm|67EF0BE0-2C97-48AA-B0F7-0B972FD67F18", "Additional Procedure Description");
			// 
			// CPCLabel2
			// 
			this.cPCLabel2.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.cPCLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 56, true);
			this.cPCLabel2.Name = "CPCLabel2";
			this.cPCLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 19, true);
			this.cPCLabel2.TabIndex = 13;
			this.cPCLabel2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CPCForm|3143B3B6-10A9-49F0-9B46-CFC2AF71A430", "Processing Codes 1, 2 and 3");
			// 
			// CPCLabel3
			// 
			this.cPCLabel3.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.cPCLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 108, true);
			this.cPCLabel3.Name = "CPCLabel3";
			this.cPCLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 22, true);
			this.cPCLabel3.TabIndex = 14;
			this.cPCLabel3.Text = "";
			this.cPCLabel3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CPCForm|7E671F2A-6918-48A1-90DF-3C632DB0CDD5", "Additional Processing Codes");
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "SG_PC4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC4)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 110, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox1.TabIndex = 15;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "SG_PC5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC5)));
			this.zTextBox2.CaptionResourceString = null;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 110, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox2.TabIndex = 16;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "SG_PC6");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC6)));
			this.zTextBox3.CaptionResourceString = null;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 110, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox3.TabIndex = 17;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "SG_PC7");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC7)));
			this.zTextBox4.CaptionResourceString = null;
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 136, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox4.TabIndex = 18;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "SG_PC8");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC8)));
			this.zTextBox5.CaptionResourceString = null;
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 136, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox5.TabIndex = 19;
			// 
			// zTextBox6
			// 
			this.BindingSource.SetBindingMember(this.zTextBox6, "SG_PC9");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC9)));
			this.zTextBox6.CaptionResourceString = null;
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 136, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox6.TabIndex = 20;
			// 
			// zTextBox7
			// 
			this.BindingSource.SetBindingMember(this.zTextBox7, "SG_PC10");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC10)));
			this.zTextBox7.CaptionResourceString = null;
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 162, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox7.TabIndex = 21;
			// 
			// zTextBox8
			// 
			this.BindingSource.SetBindingMember(this.zTextBox8, "SG_PC11");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC11)));
			this.zTextBox8.CaptionResourceString = null;
			this.zTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 162, true);
			this.zTextBox8.Name = "zTextBox8";
			this.zTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox8.TabIndex = 22;
			// 
			// zTextBox9
			// 
			this.BindingSource.SetBindingMember(this.zTextBox9, "SG_PC12");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC12)));
			this.zTextBox9.CaptionResourceString = null;
			this.zTextBox9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 162, true);
			this.zTextBox9.Name = "zTextBox9";
			this.zTextBox9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox9.TabIndex = 23;
			// 
			// zTextBox10
			// 
			this.BindingSource.SetBindingMember(this.zTextBox10, "SG_PC13");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC13)));
			this.zTextBox10.CaptionResourceString = null;
			this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 188, true);
			this.zTextBox10.Name = "zTextBox10";
			this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox10.TabIndex = 24;
			// 
			// zTextBox11
			// 
			this.BindingSource.SetBindingMember(this.zTextBox11, "SG_PC14");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC14)));
			this.zTextBox11.CaptionResourceString = null;
			this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 188, true);
			this.zTextBox11.Name = "zTextBox11";
			this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox11.TabIndex = 25;
			// 
			// zTextBox12
			// 
			this.BindingSource.SetBindingMember(this.zTextBox12, "SG_PC15");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_PC15)));
			this.zTextBox12.CaptionResourceString = null;
			this.zTextBox12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 188, true);
			this.zTextBox12.Name = "zTextBox12";
			this.zTextBox12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox12.TabIndex = 26;
			// 
			// zTextBox13
			// 
			this.BindingSource.SetBindingMember(this.zTextBox13, "SG_CPCCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((SGCPC)(null)).SG_CPCCode)));
			this.zTextBox13.CaptionResourceString = null;
			this.zTextBox13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 30, true);
			this.zTextBox13.Name = "zTextBox13";
			this.zTextBox13.ReadOnly = true;
			this.zTextBox13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.zTextBox13.TabIndex = 2;
			// 
			// CPCForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 293, true);
			this.Controls.Add(this.zTextBox13);
			this.Controls.Add(this.zTextBox12);
			this.Controls.Add(this.zTextBox11);
			this.Controls.Add(this.zTextBox10);
			this.Controls.Add(this.zTextBox9);
			this.Controls.Add(this.zTextBox8);
			this.Controls.Add(this.zTextBox7);
			this.Controls.Add(this.zTextBox6);
			this.Controls.Add(this.zTextBox5);
			this.Controls.Add(this.zTextBox4);
			this.Controls.Add(this.zTextBox3);
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.cPCLabel3);
			this.Controls.Add(this.cPCLabel2);
			this.Controls.Add(this.cPCLabel1);
			this.Controls.Add(this.pC3);
			this.Controls.Add(this.pC2);
			this.Controls.Add(this.pC1);
			this.Controls.Add(this.aPCDescription);
			this.Controls.Add(this.OKButton);
			this.DataSourceType = typeof(SGCPC);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CPCForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("CPCForm|18548C34-2D90-414D-BA89-BCE9349E20C9", "Customs Procedure Code");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.aPCDescription, 0);
			this.Controls.SetChildIndex(this.pC1, 0);
			this.Controls.SetChildIndex(this.pC2, 0);
			this.Controls.SetChildIndex(this.pC3, 0);
			this.Controls.SetChildIndex(this.cPCLabel1, 0);
			this.Controls.SetChildIndex(this.cPCLabel2, 0);
			this.Controls.SetChildIndex(this.cPCLabel3, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zTextBox2, 0);
			this.Controls.SetChildIndex(this.zTextBox3, 0);
			this.Controls.SetChildIndex(this.zTextBox4, 0);
			this.Controls.SetChildIndex(this.zTextBox5, 0);
			this.Controls.SetChildIndex(this.zTextBox6, 0);
			this.Controls.SetChildIndex(this.zTextBox7, 0);
			this.Controls.SetChildIndex(this.zTextBox8, 0);
			this.Controls.SetChildIndex(this.zTextBox9, 0);
			this.Controls.SetChildIndex(this.zTextBox10, 0);
			this.Controls.SetChildIndex(this.zTextBox11, 0);
			this.Controls.SetChildIndex(this.zTextBox12, 0);
			this.Controls.SetChildIndex(this.zTextBox13, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
