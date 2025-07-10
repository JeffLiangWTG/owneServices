namespace Enterprise.ProcessManagement.GUI
{
	partial class CustomerServiceTicketDetailsControl
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
			this.ClientRequestNumberPanel = new ZArchitecture.GUI.ZPanel();
			this.LeftTopPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.MiddleTopPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.splitContainerMain = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainerLeft = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.OrganisationFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ClientFindBox = new Enterprise.ProcessManagement.GUI.CustomerServiceTicketClientFindBox();
			this.CountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.StatusDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.SelectionCriterion5DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SelectionCriterion4DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SelectionCriterion3DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SelectionCriterion2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SelectionCriterion1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FullDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SummaryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eConversationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.eConversationControl = new Enterprise.EConversation.GUI.EConversationFullControl(Enterprise.ZArchitecture.Modules.ModuleIDs.CustomerServiceTicket);
			this.DescriptionEConvoSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomFieldsGroupBox.SuspendLayout();
			this.CustomFieldsControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.OrganisationFindBox.SuspendLayout();
			this.ClientFindBox.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.SelectionCriterion5DropEdit.SuspendLayout();
			this.SelectionCriterion4DropEdit.SuspendLayout();
			this.SelectionCriterion3DropEdit.SuspendLayout();
			this.SelectionCriterion2DropEdit.SuspendLayout();
			this.SelectionCriterion1DropEdit.SuspendLayout();
			this.DescriptionGroupBox.SuspendLayout();
			this.eConversationGroupBox.SuspendLayout();
			this.eConversationControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).BeginInit();
			this.splitContainerLeft.Panel1.SuspendLayout();
			this.splitContainerLeft.Panel2.SuspendLayout();
			this.splitContainerLeft.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel1.SuspendLayout();
			this.splitContainerMain.Panel2.SuspendLayout();
			this.splitContainerMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DescriptionEConvoSplitContainer)).BeginInit();
			this.DescriptionEConvoSplitContainer.Panel1.SuspendLayout();
			this.DescriptionEConvoSplitContainer.Panel2.SuspendLayout();
			this.DescriptionEConvoSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkRequest);
			// 
			// splitContainerMain
			// 
			this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainerMain.IsSplitterFixed = true;
			this.splitContainerMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerMain.Name = "splitContainerMain";
			this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerMain.Panel1
			// 
			this.splitContainerMain.Panel1.Controls.Add(this.splitContainerLeft);
			this.splitContainerMain.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(399);
			// 
			// splitContainerMain.Panel2
			// 
			this.splitContainerMain.Panel2.Controls.Add(this.MiddleTopPanel);
			this.splitContainerMain.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(239);
			this.splitContainerMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 210, true);
			this.splitContainerMain.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(400);
			this.splitContainerMain.TabIndex = 0;
			this.splitContainerMain.TabStop = false;
			// 
			// splitContainerLeft
			// 
			this.splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerLeft.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainerLeft.IsSplitterFixed = true;
			this.splitContainerLeft.Name = "splitContainerLeft";
			this.splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerLeft.Panel1
			// 
			this.splitContainerLeft.Panel1.Controls.Add(this.ClientRequestNumberPanel);
			// 
			// splitContainerLeft.Panel2
			// 
			this.splitContainerLeft.Panel2.Controls.Add(this.LeftTopPanel);

			this.splitContainerLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 200, true);
			this.splitContainerLeft.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			// 
			// ClientRequestNumberPanel
			// 
			this.ClientRequestNumberPanel.Controls.Add(this.RequestNumberTextBox);
			this.ClientRequestNumberPanel.Controls.Add(this.ClientFindBox);
			this.ClientRequestNumberPanel.Controls.Add(this.OrganisationFindBox);
			this.ClientRequestNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientRequestNumberPanel.Name = "ClientRequestNumberPanel";
			this.ClientRequestNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 73, true);
			this.ClientRequestNumberPanel.TabIndex = 0;
			// 
			// LeftTopPanel
			// 
			this.LeftTopPanel.Controls.Add(this.SelectionCriterion1DropEdit);
			this.LeftTopPanel.Controls.Add(this.SelectionCriterion2DropEdit);
			this.LeftTopPanel.Controls.Add(this.SelectionCriterion3DropEdit);
			this.LeftTopPanel.Controls.Add(this.SelectionCriterion4DropEdit);
			this.LeftTopPanel.Controls.Add(this.SelectionCriterion5DropEdit);
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.LeftTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 120, true);
			this.LeftTopPanel.TabIndex = 0;
			// 
			// MiddleTopPanel
			// 
			this.MiddleTopPanel.Controls.Add(this.CountryFindBox);
			this.MiddleTopPanel.Controls.Add(this.DepartmentFindBox);
			this.MiddleTopPanel.Controls.Add(this.BranchFindBox);
			this.MiddleTopPanel.Controls.Add(this.StatusDropEdit);
			this.MiddleTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddleTopPanel.FixedRows = true;
			this.MiddleTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.MiddleTopPanel.Name = "MiddleTopPanel";
			this.MiddleTopPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.MiddleTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 220, true);
			this.MiddleTopPanel.TabIndex = 1;
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("9fb9424e-f3f5-4a2b-9717-a83278e2f11c", "Custom Fields");
			this.CustomFieldsGroupBox.Controls.Add(this.CustomFieldsControl);
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(802, 3, true);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 220, true);
			this.CustomFieldsGroupBox.TabIndex = 0;
			this.CustomFieldsGroupBox.TabStop = false;
			// 
			// CustomFieldsControl
			// 
			this.CustomFieldsControl.AllowDrop = true;
			this.CustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.CustomFieldsControl.Name = "CustomFieldsControl";
			this.CustomFieldsControl.NothingSetupMessageLabelText = "";
			this.CustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 280, true);
			this.CustomFieldsControl.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.DetailsGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("5619cfee-8b9b-4acf-875e-c09df2e952ef", "Details");
			this.DetailsGroupBox.Controls.Add(this.splitContainerMain);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(796, 220, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "WKR_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_RN_NKCountry)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 78, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 2;
			this.MiddleTopPanel.SetRow(this.CountryFindBox, 4);
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 15, true);
			this.CountryFindBox.TabIndex = 9;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "WKR_GE_Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_GE_Department)));
			this.DepartmentFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 52, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.PreBoundMaxLength = 3;
			this.MiddleTopPanel.SetRow(this.DepartmentFindBox, 3);
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 15, true);
			this.DepartmentFindBox.TabIndex = 8;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "WKR_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_GB_Branch)));
			this.BranchFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 26, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.PreBoundMaxLength = 3;
			this.MiddleTopPanel.SetRow(this.BranchFindBox, 2);
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 15, true);
			this.BranchFindBox.TabIndex = 7;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "WKR_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 0, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.MiddleTopPanel.SetRow(this.StatusDropEdit, 1);
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 20, true);
			this.StatusDropEdit.TabIndex = 6;
			// 
			// SelectionCriterion5DropEdit
			// 
			this.SelectionCriterion5DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectionCriterion5DropEdit, "WKR_SelectionCriteria5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_SelectionCriteria5)));
			this.SelectionCriterion5DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 114, true);
			this.SelectionCriterion5DropEdit.Name = "SelectionCriterion5DropEdit";
			this.SelectionCriterion5DropEdit.PreBoundMaxLength = 3;
			this.LeftTopPanel.SetRow(this.SelectionCriterion5DropEdit, 5);
			this.SelectionCriterion5DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.SelectionCriterion5DropEdit.TabIndex = 6;
			// 
			// SelectionCriterion4DropEdit
			// 
			this.SelectionCriterion4DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectionCriterion4DropEdit, "WKR_SelectionCriteria4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_SelectionCriteria4)));
			this.SelectionCriterion4DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 88, true);
			this.SelectionCriterion4DropEdit.Name = "SelectionCriterion4DropEdit";
			this.SelectionCriterion4DropEdit.PreBoundMaxLength = 3;
			this.LeftTopPanel.SetRow(this.SelectionCriterion4DropEdit, 4);
			this.SelectionCriterion4DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.SelectionCriterion4DropEdit.TabIndex = 5;
			// 
			// SelectionCriterion3DropEdit
			// 
			this.SelectionCriterion3DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectionCriterion3DropEdit, "WKR_SelectionCriteria3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_SelectionCriteria3)));
			this.SelectionCriterion3DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 62, true);
			this.SelectionCriterion3DropEdit.Name = "SelectionCriterion3DropEdit";
			this.SelectionCriterion3DropEdit.PreBoundMaxLength = 3;
			this.LeftTopPanel.SetRow(this.SelectionCriterion3DropEdit, 3);
			this.SelectionCriterion3DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.SelectionCriterion3DropEdit.TabIndex = 4;
			// 
			// SelectionCriterion2DropEdit
			// 
			this.SelectionCriterion2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectionCriterion2DropEdit, "WKR_SelectionCriteria2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_SelectionCriteria2)));
			this.SelectionCriterion2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 36, true);
			this.SelectionCriterion2DropEdit.Name = "SelectionCriterion2DropEdit";
			this.SelectionCriterion2DropEdit.PreBoundMaxLength = 3;
			this.LeftTopPanel.SetRow(this.SelectionCriterion2DropEdit, 2);
			this.SelectionCriterion2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.SelectionCriterion2DropEdit.TabIndex = 3;
			// 
			// SelectionCriterion1DropEdit
			// 
			this.SelectionCriterion1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectionCriterion1DropEdit, "WKR_SelectionCriteria1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_SelectionCriteria1)));
			this.SelectionCriterion1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 10, true);
			this.SelectionCriterion1DropEdit.Name = "SelectionCriterion1DropEdit";
			this.SelectionCriterion1DropEdit.PreBoundMaxLength = 3;
			this.LeftTopPanel.SetRow(this.SelectionCriterion1DropEdit, 1);
			this.SelectionCriterion1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.SelectionCriterion1DropEdit.TabIndex = 2;
			// 
			// RequestNumberTextBox
			// 
			this.RequestNumberTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.RequestNumberTextBox, "WKR_RequestNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_RequestNumber)));
			this.RequestNumberTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.RequestNumberTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RequestNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 0, true);
			this.RequestNumberTextBox.Name = "RequestNumberTextBox";
			this.RequestNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 10, true);
			this.RequestNumberTextBox.TabIndex = 0;
			this.RequestNumberTextBox.TabStop = false;
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "OrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).OrganisationPK)));
			this.OrganisationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 26, true);
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.OrganisationFindBox.TabIndex = 1;
			// 
			// ClientFindBox
			// 
			this.ClientFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientFindBox, "WKR_OC_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_OC_Client)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).Lookups.ClientsFilteredByOrganisation)));
			this.ClientFindBox.BindToList = "Lookups+ClientsFilteredByOrganisation";
			this.ClientFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ClientFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 52, true);
			this.ClientFindBox.Name = "ClientFindBox";
			this.ClientFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.ClientFindBox.TabIndex = 2;
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("2df0aa2b-2e32-46c0-bddf-3f77d1909c8f", "Description");
			this.DescriptionGroupBox.Controls.Add(this.FullDetailsTextBox);
			this.DescriptionGroupBox.Controls.Add(this.SummaryTextBox);
			this.DescriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 503, true);
			this.DescriptionGroupBox.TabIndex = 1;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// FullDetailsTextBox
			// 
			this.FullDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
																					| System.Windows.Forms.AnchorStyles.Left)
																					 | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FullDetailsTextBox, "WKR_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_Description)));
			this.FullDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FullDetailsTextBox, false);
			this.FullDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 45, true);
			this.FullDetailsTextBox.Multiline = true;
			this.FullDetailsTextBox.Name = "FullDetailsTextBox";
			this.FullDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 452, true);
			this.FullDetailsTextBox.TabIndex = 1;
			// 
			// SummaryTextBox
			// 
			this.SummaryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
																				 | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SummaryTextBox, "WKR_Summary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).WKR_Summary)));
			this.SummaryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 19, true);
			this.SummaryTextBox.Name = "SummaryTextBox";
			this.SummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.SummaryTextBox.TabIndex = 0;
			// 
			// eConversationGroupBox
			// 
			this.eConversationGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("69928b5e-7e67-427f-a16f-7f3cd1082082", "eConversation");
			this.eConversationGroupBox.Controls.Add(this.eConversationControl);
			this.eConversationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eConversationGroupBox.Name = "eConversationGroupBox";
			this.eConversationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 700, true);
			this.eConversationGroupBox.TabIndex = 2;
			this.eConversationGroupBox.TabStop = false;
			// 
			// eConversationUserControl1
			// 
			this.eConversationControl.AllowDrop = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.EConversation.Business.IConversation)(((Enterprise.ProcessManagement.Business.WorkRequest)(null)).Conversation)));
			this.eConversationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.eConversationControl.Name = "eConversationControl";
			this.eConversationControl.ShouldShowAddInternalCommentButton = true;
			this.eConversationControl.ViewMode = Enterprise.EConversation.GUI.EConversationViewMode.ShowOnlyEConversation;
			this.eConversationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			this.eConversationControl.TabIndex = 0;
			// 
			// DescriptionEConvoSplitContainer
			// 
			this.DescriptionEConvoSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
																								 | System.Windows.Forms.AnchorStyles.Left)
																								| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionEConvoSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 228, true);
			this.DescriptionEConvoSplitContainer.Name = "DescriptionEConvoSplitContainer";
			// 
			// DescriptionEConvoSplitContainer.Panel1
			// 
			this.DescriptionEConvoSplitContainer.Panel1.Controls.Add(this.DescriptionGroupBox);
			// 
			// DescriptionEConvoSplitContainer.Panel2
			// 
			this.DescriptionEConvoSplitContainer.Panel2.Controls.Add(this.eConversationGroupBox);
			this.DescriptionEConvoSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(796, 480, true);
			this.DescriptionEConvoSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.DescriptionEConvoSplitContainer.TabIndex = 3;
			// 
			// WorkRequestDetailsTabControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DescriptionEConvoSplitContainer);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.CustomFieldsGroupBox);
			this.Name = "WorkRequestDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 714, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomFieldsGroupBox.ResumeLayout(false);
			this.CustomFieldsGroupBox.PerformLayout();
			this.CustomFieldsControl.ResumeLayout(false);
			this.CustomFieldsControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ClientFindBox.ResumeLayout(true);
			this.ClientFindBox.PerformLayout();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.SelectionCriterion5DropEdit.ResumeLayout(true);
			this.SelectionCriterion5DropEdit.PerformLayout();
			this.SelectionCriterion4DropEdit.ResumeLayout(true);
			this.SelectionCriterion4DropEdit.PerformLayout();
			this.SelectionCriterion3DropEdit.ResumeLayout(true);
			this.SelectionCriterion3DropEdit.PerformLayout();
			this.SelectionCriterion2DropEdit.ResumeLayout(true);
			this.SelectionCriterion2DropEdit.PerformLayout();
			this.SelectionCriterion1DropEdit.ResumeLayout(true);
			this.SelectionCriterion1DropEdit.PerformLayout();
			this.DescriptionGroupBox.ResumeLayout(false);
			this.DescriptionGroupBox.PerformLayout();
			this.eConversationGroupBox.ResumeLayout(false);
			this.eConversationGroupBox.PerformLayout();
			this.eConversationControl.ResumeLayout(true);
			this.eConversationControl.PerformLayout();
			this.DescriptionEConvoSplitContainer.Panel1.ResumeLayout(false);
			this.DescriptionEConvoSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DescriptionEConvoSplitContainer)).EndInit();
			this.DescriptionEConvoSplitContainer.ResumeLayout(false);
			this.DescriptionEConvoSplitContainer.PerformLayout();
			this.splitContainerLeft.Panel1.ResumeLayout(false);
			this.splitContainerLeft.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).EndInit();
			this.splitContainerLeft.ResumeLayout(false);
			this.splitContainerLeft.PerformLayout();
			this.splitContainerMain.Panel1.ResumeLayout(false);
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			this.splitContainerMain.ResumeLayout(false);
			this.splitContainerMain.PerformLayout();
			this.ClientRequestNumberPanel.ResumeLayout(false);
			this.ClientRequestNumberPanel.PerformLayout();
			this.LeftTopPanel.ResumeLayout(false);
			this.LeftTopPanel.PerformLayout();
			this.MiddleTopPanel.ResumeLayout(false);
			this.MiddleTopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainerMain;
		private CargoWise.Windows.UI.KSplitContainer splitContainerLeft;
		private ZArchitecture.GUI.ZPanel ClientRequestNumberPanel;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel LeftTopPanel;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel MiddleTopPanel;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.ZTextBox RequestNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit SelectionCriterion1DropEdit;
		private ZArchitecture.GUI.ZDropEdit SelectionCriterion5DropEdit;
		private ZArchitecture.GUI.ZDropEdit SelectionCriterion4DropEdit;
		private ZArchitecture.GUI.ZDropEdit SelectionCriterion3DropEdit;
		private ZArchitecture.GUI.ZDropEdit SelectionCriterion2DropEdit;
		private ZArchitecture.GUI.ZGuidFindBox DepartmentFindBox;
		private ZArchitecture.GUI.ZGuidFindBox BranchFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CountryFindBox;
		private ZArchitecture.GUI.ZGroupBox DescriptionGroupBox;
		private ZArchitecture.GUI.ZGroupBox eConversationGroupBox;
		private ZArchitecture.GUI.ZGroupBox CustomFieldsGroupBox;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl CustomFieldsControl;
		private CargoWise.Windows.UI.KSplitContainer DescriptionEConvoSplitContainer;
		private ZArchitecture.ZTextBox SummaryTextBox;
		private ZArchitecture.ZTextBox FullDetailsTextBox;
		private Enterprise.ProcessManagement.GUI.CustomerServiceTicketClientFindBox ClientFindBox;
		private EConversation.GUI.EConversationFullControl eConversationControl;
		private ZArchitecture.GUI.ZGuidFindBox OrganisationFindBox;
	}
}
