using CargoWiseOne.ResourceStrings;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.GUI.Internal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DocAddressUserControl : ZUserControl
	{
		private Enterprise.ZArchitecture.GUI.ZGroupBox AddressesGroupBox;
		private Enterprise.ZArchitecture.ZLabel zLabel3;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		private Enterprise.ZArchitecture.ZLabel zLabel5;
		private Enterprise.ZArchitecture.ZLabel zLabel7;
		private Enterprise.ZArchitecture.ZLabel zLabel4;
		private Enterprise.ZArchitecture.ZLabel zLabel8;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZLabel zLabel11;
		private Enterprise.ZArchitecture.ZLabel zLabel9;
		private Enterprise.ZArchitecture.ZLabel zLabel10;
		private ZLabel zLabel13;
		private ZLabel zLabel12;
		private ZLabel zLabel1;
		private ZLabel zLabel15;
		private ZLabel zLabel14;
		private ZLabel zLabel17;
		private ZLabel zLabel16;
		private ZLabel zLabel18;
		private ZLabel zLabel19;
		private ZLabel zLabel21;
		private ZLabel zLabel20;
		private ZGroupBox groupBox1;
		protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal protected ZTabControl TabControl;
		protected ZTabPage DetailsTabPage;
		internal ZTabPage ScreeningLogsTabPage;
		private StmEntityScreeningLogControl StmEntityScreeningLogControl;
		private DocAddressGrid AddressGrid;
		protected ZDocAddressControl AddressControl;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AddressesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel21 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel20 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel18 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AddressGrid = new Enterprise.MasterFiles.GUI.Internal.DocAddressGrid();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ScreeningLogsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StmEntityScreeningLogControl = new Enterprise.DeniedPartyScreening.GUI.StmEntityScreeningLogControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AddressesGroupBox.SuspendLayout();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddressGrid)).BeginInit();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.ScreeningLogsTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.JobDocAddressDependentCollection);
			//
			// AddressesGroupBox
			//
			this.AddressesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AddressesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|ea54c31f-6764-43bf-9a7c-8af997dcd60e", "Address Details");
			this.AddressesGroupBox.Controls.Add(this.zLabel21);
			this.AddressesGroupBox.Controls.Add(this.zLabel20);
			this.AddressesGroupBox.Controls.Add(this.zLabel19);
			this.AddressesGroupBox.Controls.Add(this.zLabel18);
			this.AddressesGroupBox.Controls.Add(this.zLabel17);
			this.AddressesGroupBox.Controls.Add(this.zLabel16);
			this.AddressesGroupBox.Controls.Add(this.zLabel15);
			this.AddressesGroupBox.Controls.Add(this.zLabel14);
			this.AddressesGroupBox.Controls.Add(this.zLabel13);
			this.AddressesGroupBox.Controls.Add(this.zLabel12);
			this.AddressesGroupBox.Controls.Add(this.zLabel1);
			this.AddressesGroupBox.Controls.Add(this.zLabel2);
			this.AddressesGroupBox.Controls.Add(this.zLabel11);
			this.AddressesGroupBox.Controls.Add(this.zLabel9);
			this.AddressesGroupBox.Controls.Add(this.zLabel10);
			this.AddressesGroupBox.Controls.Add(this.zLabel3);
			this.AddressesGroupBox.Controls.Add(this.zLabel6);
			this.AddressesGroupBox.Controls.Add(this.zLabel5);
			this.AddressesGroupBox.Controls.Add(this.zLabel7);
			this.AddressesGroupBox.Controls.Add(this.zLabel4);
			this.AddressesGroupBox.Controls.Add(this.zLabel8);
			this.AddressesGroupBox.Controls.Add(this.groupBox1);
			this.AddressesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 6, true);
			this.AddressesGroupBox.Name = "AddressesGroupBox";
			this.AddressesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 182, true);
			this.AddressesGroupBox.TabIndex = 1;
			this.AddressesGroupBox.TabStop = false;
			//
			// zLabel21
			//
			this.zLabel21.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel21, "E2_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Email)));
			this.zLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 125, true);
			this.zLabel21.Name = "zLabel21";
			this.zLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 13, true);
			this.zLabel21.TabIndex = 45;
			this.zLabel21.CaptionResourceString = Res.GetData("3EE6EB14-82E6-4959-A265-3030816FE1B3", "<E-Mail>");
			//
			// zLabel20
			//
			this.zLabel20.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel20, "E2_Fax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Fax)));
			this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 101, true);
			this.zLabel20.Name = "zLabel20";
			this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 13, true);
			this.zLabel20.TabIndex = 44;
			this.zLabel20.CaptionResourceString = Res.GetData("4522BBCA-72EF-4B7D-8814-B19BC981EB85", "<Fax>");
			//
			// zLabel19
			//
			this.zLabel19.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel19, "E2_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Phone)));
			this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 78, true);
			this.zLabel19.Name = "zLabel19";
			this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.zLabel19.TabIndex = 43;
			this.zLabel19.CaptionResourceString = Res.GetData("F6F747BB-D946-49C2-8FA3-38212FAC9534", "<Phone>");
			//
			// zLabel18
			//
			this.zLabel18.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel18, "E2_Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Contact)));
			this.zLabel18.IsFontBold = true;
			this.zLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 30, true);
			this.zLabel18.Name = "zLabel18";
			this.zLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.zLabel18.TabIndex = 42;
			this.zLabel18.CaptionResourceString = Res.GetData("83821BAC-DD3E-4FDF-BAF5-1500DC056447", "<Contact>");
			//
			// zLabel17
			//
			this.zLabel17.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel17, "E2_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_City)));
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 111, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 13, true);
			this.zLabel17.TabIndex = 41;
			this.zLabel17.CaptionResourceString = Res.GetData("03D48FFA-28F8-456F-91E9-C8EC1569EAFB", "<City>");
			//
			// zLabel16
			//
			this.zLabel16.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel16, "E2_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_State)));
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 124, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.zLabel16.TabIndex = 40;
			this.zLabel16.CaptionResourceString = Res.GetData("EFBBCE4D-50D8-44F1-852B-0414ABA905CC", "<State>");
			//
			// zLabel15
			//
			this.zLabel15.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel15, "E2_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_RN_NKCountryCode)));
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 137, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.zLabel15.TabIndex = 39;
			this.zLabel15.CaptionResourceString = Res.GetData("D4CEDB38-1AA8-4746-B3D1-0F97B647CCE7", "<CY>");
			//
			// zLabel14
			//
			this.zLabel14.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zLabel14, "E2_Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Postcode)));
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 89, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 13, true);
			this.zLabel14.TabIndex = 38;
			this.zLabel14.CaptionResourceString = Res.GetData("95A99C05-E8A7-471B-855F-8D33374A2021", "<Postcode>");
			//
			// zLabel13
			//
			this.zLabel13.AutoSize = true;
			this.zLabel13.AutoEllipsis = true;
			this.BindingSource.SetBindingMember(this.zLabel13, "E2_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Address2)));
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 65, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 13, true);
			this.zLabel13.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 13, true);
			this.zLabel13.TabIndex = 37;
			this.zLabel13.CaptionResourceString = Res.GetData("0C6DF6B9-CFF6-4AE6-AEFE-83D8FC275512", "<Address 2>");
			//
			// zLabel12
			//
			this.zLabel12.AutoSize = true;
			this.zLabel12.AutoEllipsis = true;
			this.BindingSource.SetBindingMember(this.zLabel12, "E2_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Address1)));
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 52, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 13, true);
			this.zLabel12.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 13, true);
			this.zLabel12.TabIndex = 36;
			this.zLabel12.CaptionResourceString = Res.GetData("7E3C863F-9757-4725-90BD-28516CA0D212", "<Address 1>");
			//
			// zLabel1
			//
			this.zLabel1.AutoSize = true;
			this.zLabel1.AutoEllipsis = true;
			this.BindingSource.SetBindingMember(this.zLabel1, "E2_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_CompanyName)));
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 30, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			this.zLabel1.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 13, true);
			this.zLabel1.TabIndex = 35;
			this.zLabel1.CaptionResourceString = Res.GetData("0DF83F26-31B9-4412-9676-F3ADB8D8FD38", "<Company Name>");
			//
			// zLabel2
			//
			this.zLabel2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|96945a32-e632-4d49-bb96-d7c5e374f497", "Contact:");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(322, 30, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.zLabel2.TabIndex = 31;
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel11
			//
			this.zLabel11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|498612b1-ddb0-4518-8ade-059c162bcf06", "Fax:");
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 102, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 12, true);
			this.zLabel11.TabIndex = 34;
			this.zLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel9
			//
			this.zLabel9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|c7d972f8-87bc-47c0-bec3-661be88d5c0e", "E-Mail:");
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 125, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.zLabel9.TabIndex = 32;
			this.zLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel10
			//
			this.zLabel10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|61e6ebd1-1637-4fc1-85f1-99782a15d71e", "Phone:");
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 78, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.zLabel10.TabIndex = 33;
			this.zLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel3
			//
			this.zLabel3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|e352b581-d1fc-4aee-acfa-61dffa9503c0", "Company Name:");
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 30, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 13, true);
			this.zLabel3.TabIndex = 13;
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel6
			//
			this.zLabel6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|6209b373-3a0d-406a-a39b-707f2a4bfaae", "Postcode:");
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 89, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.zLabel6.TabIndex = 18;
			this.zLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel5
			//
			this.zLabel5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|10b60400-b294-4dc5-8f45-75c90c3d7f77", "City:");
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 111, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
			this.zLabel5.TabIndex = 20;
			this.zLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel7
			//
			this.zLabel7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|9ecdbb90-1cc2-420b-9aed-478082c5019f", "State:");
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 124, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.zLabel7.TabIndex = 22;
			this.zLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel4
			//
			this.zLabel4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|4dee835a-a341-45fc-8778-9d4588f39eff", "Address:");
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 52, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.zLabel4.TabIndex = 15;
			this.zLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// zLabel8
			//
			this.zLabel8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|5ef4fed5-a7e7-468d-bc07-67ab1fb68a87", "Country/Region:");
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 137, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.zLabel8.TabIndex = 24;
			this.zLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// groupBox1
			//
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.groupBox1, false);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 23, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 142, true);
			this.groupBox1.TabIndex = 46;
			this.groupBox1.TabStop = false;
			//
			// SplitContainer
			//
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// SplitContainer.Panel1
			//
			this.SplitContainer.Panel1.Controls.Add(this.AddressGrid);
			//
			// SplitContainer.Panel2
			//
			this.SplitContainer.Panel2.Controls.Add(this.TabControl);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 520, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(171);
			this.SplitContainer.TabIndex = 37;
			//
			// AddressGrid
			//
			this.AddressGrid.AllowCopyToNewRowMenuItem = false;
			this.AddressGrid.AllowDragDropWithChanges = false;
			this.AddressGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddressGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).AddressDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Postcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).E2_ScreeningStatus)));
			this.AddressGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|b4c3ad47-66a4-4bf9-bf0b-5947bc0f58d5", "Address Description");
			zTextBoxColumnStyleInfo1.ColumnName = "AddressDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zCheckBoxColumnStyleInfo1.ColumnName = "E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32);
			zTextBoxColumnStyleInfo2.ColumnName = "E2_CompanyName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo3.ColumnName = "E2_Address1";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.ColumnName = "E2_Address2";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "E2_City";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.ColumnName = "E2_State";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.ColumnName = "E2_Postcode";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo8.ColumnName = "E2_RN_NKCountryCode";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo9.ColumnName = "E2_Contact";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.ColumnName = "E2_Phone";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "E2_Fax";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo12.ColumnName = "E2_Email";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo13.ColumnName = "E2_ScreeningStatus";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddressGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.AddressGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.AddressGrid.GridId = "132b7068-504a-432e-9f03-b4422ae433bc";
			this.AddressGrid.CopySelectedRowsAllowed = false;
			this.AddressGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressGrid.IsWholeRowSelectedOnClick = true;
			this.AddressGrid.LayoutKey = "AddressGrid";
			this.AddressGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressGrid.Name = "AddressGrid";
			this.AddressGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 171, true);
			this.AddressGrid.TabIndex = 0;
			//
			// TabControl
			//
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.ScreeningLogsTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 345, true);
			this.TabControl.TabIndex = 0;
			//
			// DetailsTabPage
			//
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|14fd1f61-081b-4631-8fb6-a548b1a7ffc0", "Details");
			this.DetailsTabPage.Controls.Add(this.AddressControl);
			this.DetailsTabPage.Controls.Add(this.AddressesGroupBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 318, true);
			this.DetailsTabPage.TabIndex = 0;
			//
			// AddressControl
			//
			this.AddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)))));
			this.AddressControl.BindToContacts = "Organisation+ContactsActive";
			this.AddressControl.BindToOrganisations = "Lookups+OrgHeader_List";
			this.AddressControl.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|bd998914-73e6-4b39-9c9c-ef6d0a77505d", "Organization");
			this.AddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.AddressControl.Name = "AddressControl";
			this.AddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.AddressControl.TabIndex = 36;
			//
			// ScreeningLogsTabPage
			//
			this.ScreeningLogsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DocAddressUserControl|efdc6a13-e397-41f1-9a5a-d651bc76ed5e", "Denied Party Screening Logs");
			this.ScreeningLogsTabPage.Controls.Add(this.StmEntityScreeningLogControl);
			this.ScreeningLogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ScreeningLogsTabPage.Name = "ScreeningLogsTabPage";
			this.ScreeningLogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ScreeningLogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 318, true);
			this.ScreeningLogsTabPage.TabIndex = 1;
			//
			// StmEntityScreeningLogControl
			//
			this.StmEntityScreeningLogControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StmEntityScreeningLogControl, "ScreeningLogCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.DeniedPartyScreening.Integration.IStmEntityScreeningLogCollection)(((Enterprise.MasterFiles.Business.JobDocAddress)(null)).ScreeningLogCollection)));
			this.StmEntityScreeningLogControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StmEntityScreeningLogControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StmEntityScreeningLogControl.Name = "StmEntityScreeningLogControl";
			this.StmEntityScreeningLogControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 312, true);
			this.StmEntityScreeningLogControl.TabIndex = 0;
			//
			// DocAddressUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "DocAddressUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressesGroupBox.ResumeLayout(false);
			this.AddressesGroupBox.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AddressGrid)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.ScreeningLogsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
