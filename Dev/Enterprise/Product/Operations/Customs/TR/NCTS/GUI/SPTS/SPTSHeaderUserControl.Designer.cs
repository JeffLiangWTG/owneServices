using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class SPTSHeaderUserControl
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
			this.SPTSSubTabControl = new ZTabControl();
			this.sptsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BillsTabPageUserControl = new Enterprise.Customs.TR.NCTS.GUI.SPTSBillUserControl();
			this.BillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerTabPageUserControl = new Enterprise.Customs.TR.NCTS.GUI.SPTSContainerTabUserControl();
			this.BillContainerTabPageUserControl = new Enterprise.Customs.TR.NCTS.GUI.SPTSBillContainerTabUserControl();
			this.ContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DestinationCustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PresentationCustomsOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InBondCarrierAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.InlandTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.manifestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.countryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VoyageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SailingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.sptsGroupBox.SuspendLayout();
			this.BillsTabPageUserControl.SuspendLayout();
			this.BillsTabPage.SuspendLayout();
			this.SPTSSubTabControl.SuspendLayout();
			this.ContainerTabPageUserControl.SuspendLayout();
			this.BillContainerTabPageUserControl.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.DestinationCustomsOfficeDropEdit.SuspendLayout();
			this.PresentationCustomsOfficeDropEdit.SuspendLayout();
			this.InBondCarrierAddressControl.SuspendLayout();
			this.InlandTransportModeDropEdit.SuspendLayout();
			this.manifestGroupBox.SuspendLayout();
			this.CustomsStatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.VoyageNumberTextBox.SuspendLayout();
			this.SailingDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.SPTSHeader);
			// 
			// sptsGroupBox
			// 
			this.sptsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.sptsGroupBox.Controls.Add(this.DestinationCustomsOfficeDropEdit);
			this.sptsGroupBox.Controls.Add(this.VoyageNumberTextBox);
			this.sptsGroupBox.Controls.Add(this.SailingDateEdit);
			this.sptsGroupBox.Controls.Add(this.PresentationCustomsOfficeDropEdit);
			this.sptsGroupBox.Controls.Add(this.InBondCarrierAddressControl);
			this.sptsGroupBox.Controls.Add(this.InlandTransportModeDropEdit);
			this.sptsGroupBox.Controls.Add(this.manifestGroupBox);
			this.sptsGroupBox.Controls.Add(this.SPTSSubTabControl);
			this.sptsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sptsGroupBox.Name = "sptsGroupBox";
			this.sptsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 800, true);
			this.sptsGroupBox.TabIndex = 4;
			this.sptsGroupBox.TabStop = false;
			this.sptsGroupBox.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("D64C88AC-A04D-4FA4-B3C4-1821A9AAA733", "SPTS");
			// 
			// DestinationCustomsOfficeDropEdit
			// 
			this.DestinationCustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCustomsOfficeDropEdit, "MovementHeader.BM_DestinationPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).MovementHeader.BM_DestinationPortCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DestinationCustomsOfficeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.DestinationCustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 178, true);
			this.DestinationCustomsOfficeDropEdit.Name = "DestinationCustomsOfficeDropEdit";
			this.DestinationCustomsOfficeDropEdit.ShouldResizeByMaxLength = true;
			this.DestinationCustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.DestinationCustomsOfficeDropEdit.TabIndex = 4;
			// 
			// PresentationCustomsOfficeDropEdit
			// 
			this.PresentationCustomsOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresentationCustomsOfficeDropEdit, "MovementHeader.BM_PortOfPresentationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).MovementHeader.BM_PortOfPresentationCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PresentationCustomsOfficeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.PresentationCustomsOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 152, true);
			this.PresentationCustomsOfficeDropEdit.Name = "PresentationCustomsOfficeDropEdit";
			this.PresentationCustomsOfficeDropEdit.ShouldResizeByMaxLength = true;
			this.PresentationCustomsOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.PresentationCustomsOfficeDropEdit.TabIndex = 3;
			// 
			// InBondCarrierAddressControl
			// 
			this.InBondCarrierAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InBondCarrierAddressControl, "MovementHeader.BM_OA_InBondCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).MovementHeader.BM_OA_InBondCarrier)));
			this.InBondCarrierAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 126, true);
			this.InBondCarrierAddressControl.Name = "InBondCarrierAddressControl";
			this.InBondCarrierAddressControl.PopupCaption = "";
			this.InBondCarrierAddressControl.ReadOnly = false;
			this.InBondCarrierAddressControl.ShowAddress = false;
			this.InBondCarrierAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.InBondCarrierAddressControl.TabIndex = 2;
			// 
			// InlandTransportModeDropEdit
			// 
			this.InlandTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandTransportModeDropEdit, "MovementHeader.BM_InlandTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).MovementHeader.BM_InlandTransportMode)));
			this.InlandTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 100, true);
			this.InlandTransportModeDropEdit.Name = "InlandTransportModeDropEdit";
			this.InlandTransportModeDropEdit.ShouldResizeByMaxLength = true;
			this.InlandTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.InlandTransportModeDropEdit.TabIndex = 1;
			// 
			// manifestGroupBox
			// 
			this.manifestGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.manifestGroupBox.Controls.Add(this.CustomsStatusDropEdit);
			this.manifestGroupBox.Controls.Add(this.MessageStatusDropEdit);
			this.manifestGroupBox.Controls.Add(this.countryTextBox);
			this.manifestGroupBox.Controls.Add(this.RegistrationNumberTextBox);
			this.manifestGroupBox.Controls.Add(this.RegistrationDateEdit);
			this.manifestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.manifestGroupBox.Name = "manifestGroupBox";
			this.manifestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 75, true);
			this.manifestGroupBox.TabIndex = 0;
			this.manifestGroupBox.TabStop = false;
			this.manifestGroupBox.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("675EDB42-728E-4F51-ADE5-E706E8AED548", "Declaration Details");
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "MovementHeader.BM_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).MovementHeader.BM_CustomsStatus)));
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 40, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.ShouldResizeByMaxLength = true;
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.CustomsStatusDropEdit.TabIndex = 11;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "BH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).BH_MessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 16, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.ShouldResizeByMaxLength = true;
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 20, true);
			this.MessageStatusDropEdit.TabIndex = 2;
			// 
			// countryTextBox
			// 
			this.BindingSource.SetBindingMember(this.countryTextBox, "CountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).CountryCode)));
			this.countryTextBox.CaptionResourceString = Enterprise.Customs.TR.NCTS.GUI.Res.GetData("6c993f7a-d0e6-45df-925b-fa108678d716", "Country Code");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.countryTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.countryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 16, true);
			this.countryTextBox.Name = "countryTextBox";
			this.countryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.countryTextBox.TabIndex = 0;
			// 
			// RegistrationDateEdit
			// 
			this.RegistrationDateEdit.AllowDrop = true;
			this.RegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.RegistrationDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RegistrationDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).RegistrationDate)));
			this.RegistrationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.RegistrationDateEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.RegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 40, true);
			this.RegistrationDateEdit.Name = "RegistrationDateEdit";
			this.RegistrationDateEdit.TabIndex = 4;
			// 
			// RegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberTextBox, "RegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).RegistrationNumber)));
			this.RegistrationNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.RegistrationNumberTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.RegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 40, true);
			this.RegistrationNumberTextBox.Name = "RegistrationNumberTextBox";
			this.RegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.RegistrationNumberTextBox.TabIndex = 3;
			// 
			// VoyageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageNumberTextBox, "BH_VoyageNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).BH_VoyageNumber)));
			this.VoyageNumberTextBox.CaptionResourceString = null;
			this.VoyageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 204, true);
			this.VoyageNumberTextBox.Name = "VoyageNumberTextBox";
			this.VoyageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.VoyageNumberTextBox.TabIndex = 7;
			// 
			// SailingDateEdit
			// 
			this.SailingDateEdit.AllowDrop = true;
			this.SailingDateEdit.AutoCompleteMonthThreshold = 1;
			this.SailingDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SailingDateEdit, "BH_SailingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)).BH_SailingDate)));
			this.SailingDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.SailingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 230, true);
			this.SailingDateEdit.Name = "SailingDateEdit";
			this.SailingDateEdit.TabIndex = 8;

			// 
			// SPTSSubTabControl
			// 
			this.SPTSSubTabControl.Controls.Add(this.BillsTabPage);
			this.SPTSSubTabControl.Controls.Add(this.ContainersTabPage);
			this.SPTSSubTabControl.Name = "SPTSSubTabControl";
			this.SPTSSubTabControl.TabIndex = 9;
			this.SPTSSubTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 300, true);
			this.SPTSSubTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 488, true);
			this.SPTSSubTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left	| System.Windows.Forms.AnchorStyles.Right));
			// 
			// BillsTabPage
			// 
			this.BillsTabPage.Controls.Add(this.BillsTabPageUserControl);
			this.BillsTabPage.Controls.Add(this.BillContainerTabPageUserControl);
			this.BillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 20, true);
			this.BillsTabPage.Name = "BillsTabPage";
			this.BillsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 400, true);
			this.BillsTabPage.TabIndex = 1;
			this.BillsTabPage.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("43192966-CB6B-46F6-82CD-604E284AD2DB", "Bills");
			this.BillsTabPage.UseVisualStyleBackColor = true;
			// 
			// BillsTabPageUserControl
			// 
			this.BillsTabPageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillsTabPageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.BillsTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillsTabPageUserControl.Name = "BillsTabPageUserControl";
			this.BillsTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 120, true);
			this.BillsTabPageUserControl.TabIndex = 0;
			this.BillsTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			// 
			// BillContainerTabPageUserControl
			// 
			this.BillContainerTabPageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillContainerTabPageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.BillContainerTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillContainerTabPageUserControl.Name = "BillContainerTabPageUserControl";
			this.BillContainerTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 120, true);
			this.BillContainerTabPageUserControl.TabIndex = 0;
			this.BillContainerTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Controls.Add(this.ContainerTabPageUserControl);
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 20, true);
			this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 400, true);
			this.ContainersTabPage.TabIndex = 2;
			this.ContainersTabPage.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("DD1EB4DE-0514-4318-AB2C-33BF2C17D059", "Containers");
			this.ContainersTabPage.UseVisualStyleBackColor = true;
			// 
			// ContainerTabPageUserControl
			// 
			this.ContainerTabPageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerTabPageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ContainerTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContainerTabPageUserControl.Name = "ContainerTabPageUserControl";
			this.ContainerTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 350, true);
			this.ContainerTabPageUserControl.TabIndex = 0;
			this.ContainerTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// SPTSHeaderUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.sptsGroupBox);
			this.Name = "SPTSHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 804, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.sptsGroupBox.ResumeLayout(false);
			this.sptsGroupBox.PerformLayout();
			this.BillsTabPageUserControl.ResumeLayout(false);
			this.BillsTabPageUserControl.PerformLayout();
			this.BillsTabPage.ResumeLayout(false);
			this.BillsTabPage.PerformLayout();
			this.SPTSSubTabControl.ResumeLayout(true);
			this.SPTSSubTabControl.PerformLayout();
			this.ContainerTabPageUserControl.ResumeLayout(true);
			this.ContainerTabPageUserControl.PerformLayout();
			this.BillContainerTabPageUserControl.ResumeLayout(true);
			this.BillContainerTabPageUserControl.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.DestinationCustomsOfficeDropEdit.ResumeLayout(true);
			this.DestinationCustomsOfficeDropEdit.PerformLayout();
			this.VoyageNumberTextBox.ResumeLayout(true);
			this.VoyageNumberTextBox.PerformLayout();
			this.SailingDateEdit.ResumeLayout(true);
			this.SailingDateEdit.PerformLayout();
			this.PresentationCustomsOfficeDropEdit.ResumeLayout(true);
			this.PresentationCustomsOfficeDropEdit.PerformLayout();
			this.InBondCarrierAddressControl.ResumeLayout(true);
			this.InBondCarrierAddressControl.PerformLayout();
			this.InlandTransportModeDropEdit.ResumeLayout(true);
			this.InlandTransportModeDropEdit.PerformLayout();
			this.manifestGroupBox.ResumeLayout(false);
			this.manifestGroupBox.PerformLayout();
			this.CustomsStatusDropEdit.ResumeLayout(true);
			this.CustomsStatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox countryTextBox;
		private ZArchitecture.GUI.ZGroupBox manifestGroupBox;
		private ZArchitecture.GUI.ZGroupBox sptsGroupBox;
		ZArchitecture.GUI.ZTabControl SPTSSubTabControl;
		ZArchitecture.GUI.ZTabPage BillsTabPage;
		SPTSBillUserControl BillsTabPageUserControl;
		ZArchitecture.GUI.ZTabPage ContainersTabPage;
		SPTSContainerTabUserControl ContainerTabPageUserControl;
		SPTSBillContainerTabUserControl BillContainerTabPageUserControl;
		protected ZArchitecture.GUI.ZDropEdit InlandTransportModeDropEdit;
		protected ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
		protected ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private ZArchitecture.GUI.ZAddressControl InBondCarrierAddressControl;
		private ZArchitecture.GUI.ZDropEdit DestinationCustomsOfficeDropEdit;
		private ZArchitecture.GUI.ZDropEdit PresentationCustomsOfficeDropEdit;
		private ZArchitecture.ZTextBox RegistrationNumberTextBox;
		ZDateEdit RegistrationDateEdit;
		private ZArchitecture.ZTextBox VoyageNumberTextBox;
		ZDateEdit SailingDateEdit;
	}
}
