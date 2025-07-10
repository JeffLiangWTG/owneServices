using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CusPersonForm : ZTemplateForm
	{
		ZDateEdit zDateEdit1;
		ZDropEdit zDropEdit15;
		ZTextBox zTextBox13;
		ZDropEdit zDropEdit14;
		ZTextBox zTextBox15;
		ZTextBox zTextBox16;
		ZTextBox zTextBox17;
		ZTextBox zTextBox18;
		ZTextBox zTextBox19;
		ZDateEdit zDateEdit21;
		ZTextBox zTextBox10;
		ZTextBox zTextBox9;
		ZTextBox zTextBox8;
		ZTextBox zTextBox7;
		ZDropEdit zDropEdit6;
		ZTextBox zTextBox5;
		ZDropEdit zDropEdit4;
		ZTextBox zTextBox2;
		ZTextBox zTextBox1;
		ZDropEdit zDropEdit22;
		ZGroupBox groupBox1;
		ZGroupBox groupBox3;
		ZGroupBox groupBox2;
		ZGroupBox groupBox4;
		ZTextBox fullNameTextBox;

		new void InitializeComponent()
		{
			this.fullNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit4 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit6 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox7 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox8 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox9 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox10 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDropEdit15 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox13 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit14 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox15 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox16 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox17 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox18 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox19 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEdit21 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDropEdit22 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBox4 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zDropEdit4.SuspendLayout();
			this.zDropEdit6.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.zDropEdit15.SuspendLayout();
			this.zDropEdit14.SuspendLayout();
			this.zDateEdit21.SuspendLayout();
			this.zDropEdit22.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 317, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.groupBox4);
			this.MainTabPage.Controls.Add(this.groupBox3);
			this.MainTabPage.Controls.Add(this.groupBox2);
			this.MainTabPage.Controls.Add(this.groupBox1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(949, 290, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(949, 290, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(949, 295, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 317, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			// 
			// fullNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.fullNameTextBox, "PER_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_FullName)));
			this.fullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 39, true);
			this.fullNameTextBox.Name = "fullNameTextBox";
			this.fullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.fullNameTextBox.TabIndex = 1;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "PER_HomeAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_HomeAddress1)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 20, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "PER_HomeAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_HomeAddress2)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 42, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.zTextBox2.TabIndex = 1;
			// 
			// zDropEdit4
			// 
			this.zDropEdit4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit4, "PER_RN_NKNationalityCodeISO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_RN_NKNationalityCodeISO)));
			this.zDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 72, true);
			this.zDropEdit4.Name = "zDropEdit4";
			this.zDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 17, true);
			this.zDropEdit4.TabIndex = 2;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "PER_NameTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_NameTitle)));
			this.zTextBox5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CusPersonForm|Name Title", "Title");
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 17, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.zTextBox5.TabIndex = 0;
			// 
			// zDropEdit6
			// 
			this.zDropEdit6.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit6, "PER_Gender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_Gender)));
			this.zDropEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 41, true);
			this.zDropEdit6.Name = "zDropEdit6";
			this.zDropEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 17, true);
			this.zDropEdit6.TabIndex = 1;
			// 
			// zTextBox7
			// 
			this.BindingSource.SetBindingMember(this.zTextBox7, "PER_NameSuffix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_NameSuffix)));
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 66, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.zTextBox7.TabIndex = 2;
			// 
			// zTextBox8
			// 
			this.BindingSource.SetBindingMember(this.zTextBox8, "PER_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_City)));
			this.zTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 63, true);
			this.zTextBox8.Name = "zTextBox8";
			this.zTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.zTextBox8.TabIndex = 2;
			// 
			// zTextBox9
			// 
			this.BindingSource.SetBindingMember(this.zTextBox9, "PER_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_Postcode)));
			this.zTextBox9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 106, true);
			this.zTextBox9.Name = "zTextBox9";
			this.zTextBox9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.zTextBox9.TabIndex = 4;
			// 
			// zTextBox10
			// 
			this.BindingSource.SetBindingMember(this.zTextBox10, "PER_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_State)));
			this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 85, true);
			this.zTextBox10.Name = "zTextBox10";
			this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.zTextBox10.TabIndex = 3;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "PER_PassportExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_PassportExpiryDate)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 70, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 2;
			// 
			// zDropEdit15
			// 
			this.zDropEdit15.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit15, "PER_PassportPlaceOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_PassportPlaceOfIssue)));
			this.zDropEdit15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 43, true);
			this.zDropEdit15.Name = "zDropEdit15";
			this.zDropEdit15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.zDropEdit15.TabIndex = 1;
			// 
			// zTextBox13
			// 
			this.BindingSource.SetBindingMember(this.zTextBox13, "PER_DriversLicenseNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_DriversLicenseNumber)));
			this.zTextBox13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 96, true);
			this.zTextBox13.Name = "zTextBox13";
			this.zTextBox13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.zTextBox13.TabIndex = 3;
			// 
			// zDropEdit14
			// 
			this.zDropEdit14.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit14, "PER_PreferredLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_PreferredLanguage)));
			this.zDropEdit14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 228, true);
			this.zDropEdit14.Name = "zDropEdit14";
			this.zDropEdit14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 17, true);
			this.zDropEdit14.TabIndex = 7;
			// 
			// zTextBox15
			// 
			this.BindingSource.SetBindingMember(this.zTextBox15, "PER_Passport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_Passport)));
			this.zTextBox15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 17, true);
			this.zTextBox15.Name = "zTextBox15";
			this.zTextBox15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.zTextBox15.TabIndex = 0;
			// 
			// zTextBox16
			// 
			this.BindingSource.SetBindingMember(this.zTextBox16, "PER_MobilePhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_MobilePhone)));
			this.zTextBox16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 197, true);
			this.zTextBox16.Name = "zTextBox16";
			this.zTextBox16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.zTextBox16.TabIndex = 6;
			// 
			// zTextBox17
			// 
			this.BindingSource.SetBindingMember(this.zTextBox17, "PER_HomePhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_HomePhone)));
			this.zTextBox17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 166, true);
			this.zTextBox17.Name = "zTextBox17";
			this.zTextBox17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.zTextBox17.TabIndex = 5;
			// 
			// zTextBox18
			// 
			this.BindingSource.SetBindingMember(this.zTextBox18, "PER_FaxNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_FaxNumber)));
			this.zTextBox18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 134, true);
			this.zTextBox18.Name = "zTextBox18";
			this.zTextBox18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.zTextBox18.TabIndex = 4;
			// 
			// zTextBox19
			// 
			this.BindingSource.SetBindingMember(this.zTextBox19, "PER_EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_EmailAddress)));
			this.zTextBox19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 103, true);
			this.zTextBox19.Name = "zTextBox19";
			this.zTextBox19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.zTextBox19.TabIndex = 3;
			// 
			// zDateEdit21
			// 
			this.zDateEdit21.AllowDrop = true;
			this.zDateEdit21.AutoCompleteMonthThreshold = 1;
			this.zDateEdit21.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit21, "PER_BirthDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_BirthDate)));
			this.zDateEdit21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 10, true);
			this.zDateEdit21.Name = "zDateEdit21";
			this.zDateEdit21.TabIndex = 0;
			// 
			// zDropEdit22
			// 
			this.zDropEdit22.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit22, "PER_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbPerson)(null)).PER_RN_NKCountry)));
			this.zDropEdit22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 128, true);
			this.zDropEdit22.Name = "zDropEdit22";
			this.zDropEdit22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.zDropEdit22.TabIndex = 5;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.zTextBox1);
			this.groupBox1.Controls.Add(this.zDropEdit22);
			this.groupBox1.Controls.Add(this.zTextBox2);
			this.groupBox1.Controls.Add(this.zTextBox8);
			this.groupBox1.Controls.Add(this.zTextBox9);
			this.groupBox1.Controls.Add(this.zTextBox10);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 108, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 158, true);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = Enterprise.MasterFiles.GUI.Res.GetString("CusPersonForm|Address", "Address");
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.fullNameTextBox);
			this.groupBox2.Controls.Add(this.zTextBox5);
			this.groupBox2.Controls.Add(this.zTextBox7);
			this.groupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 97, true);
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = Enterprise.MasterFiles.GUI.Res.GetString("CusPersonForm|Name", "Name");
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.zDropEdit6);
			this.groupBox3.Controls.Add(this.zDropEdit4);
			this.groupBox3.Controls.Add(this.zDateEdit21);
			this.groupBox3.Controls.Add(this.zTextBox19);
			this.groupBox3.Controls.Add(this.zTextBox18);
			this.groupBox3.Controls.Add(this.zTextBox17);
			this.groupBox3.Controls.Add(this.zDropEdit14);
			this.groupBox3.Controls.Add(this.zTextBox16);
			this.groupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 6, true);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 261, true);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = Enterprise.MasterFiles.GUI.Res.GetString("CusPersonForm|PersonalData", "Personal Data");
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.zTextBox15);
			this.groupBox4.Controls.Add(this.zTextBox13);
			this.groupBox4.Controls.Add(this.zDropEdit15);
			this.groupBox4.Controls.Add(this.zDateEdit1);
			this.groupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(601, 7, true);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 119, true);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = Enterprise.MasterFiles.GUI.Res.GetString("CusPersonForm|Identification", "Identification");
			// 
			// CusPersonForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 373, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPerson);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 410, true);
			this.Name = "CusPersonForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zDropEdit4.ResumeLayout(true);
			this.zDropEdit4.PerformLayout();
			this.zDropEdit6.ResumeLayout(true);
			this.zDropEdit6.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.zDropEdit15.ResumeLayout(true);
			this.zDropEdit15.PerformLayout();
			this.zDropEdit14.ResumeLayout(true);
			this.zDropEdit14.PerformLayout();
			this.zDateEdit21.ResumeLayout(true);
			this.zDateEdit21.PerformLayout();
			this.zDropEdit22.ResumeLayout(true);
			this.zDropEdit22.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
