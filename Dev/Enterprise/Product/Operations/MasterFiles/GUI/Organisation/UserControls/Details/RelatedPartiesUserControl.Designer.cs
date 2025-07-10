
namespace Enterprise.MasterFiles.GUI
{
	partial class RelatedPartiesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo importerCountryCodeFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.RelatedPartiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RelatedPartyClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RelatedPartyFindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RelatedPartyGridFilterDirectionDropEdi = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelatedPartyGridFilterPartyTypeDropEdi = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelatedPartiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PartyTypeDescriptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CalculatedDirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CompanyLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreightTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreightContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelatedPartyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AllParentPartiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RelatedPartiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.zPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedPartiesGrid)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllParentPartiesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// RelatedPartiesGroupBox
			// 
			this.RelatedPartiesGroupBox.Controls.Add(this.splitContainer1);
			this.RelatedPartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RelatedPartiesGroupBox, false);
			this.RelatedPartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedPartiesGroupBox.Name = "RelatedPartiesGroupBox";
			this.RelatedPartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 377, true);
			this.RelatedPartiesGroupBox.TabIndex = 0;
			this.RelatedPartiesGroupBox.TabStop = false;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.zGroupBox2);
			this.splitContainer1.Panel1MinSize = 200;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.zGroupBox1);
			this.splitContainer1.Panel2MinSize = 100;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 358, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.splitContainer1.TabIndex = 11;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fb957f82-9c7f-43b2-b47f-d6e3fb65dae1", "Related Parties");
			this.zGroupBox2.Controls.Add(this.zPanel2);
			this.zGroupBox2.Controls.Add(this.RelatedPartiesGrid);
			this.zGroupBox2.Controls.Add(this.zPanel1);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 250, true);
			this.zGroupBox2.TabIndex = 11;
			this.zGroupBox2.TabStop = false;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.RelatedPartyClearButton);
			this.zPanel2.Controls.Add(this.RelatedPartyFindButton);
			this.zPanel2.Controls.Add(this.RelatedPartyGridFilterDirectionDropEdi);
			this.zPanel2.Controls.Add(this.RelatedPartyGridFilterPartyTypeDropEdi);
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 52, true);
			this.zPanel2.TabIndex = 12;
			// 
			// RelatedPartyClearButton
			// 
			this.RelatedPartyClearButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("17b54696-524e-4636-9c1e-772aa97912fa", "Clear");
			this.RelatedPartyClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 26, true);
			this.RelatedPartyClearButton.Name = "RelatedPartyClearButton";
			this.RelatedPartyClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RelatedPartyClearButton.TabIndex = 3;
			this.RelatedPartyClearButton.UseVisualStyleBackColor = true;
			this.RelatedPartyClearButton.Click += new System.EventHandler(this.RelatedPartyClearButton_Click);
			// 
			// RelatedPartyFindButton
			// 
			this.RelatedPartyFindButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8b0f1258-2fd6-4a43-b3d6-54c3122fd77c", "Find");
			this.RelatedPartyFindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 2, true);
			this.RelatedPartyFindButton.Name = "RelatedPartyFindButton";
			this.RelatedPartyFindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RelatedPartyFindButton.TabIndex = 2;
			this.RelatedPartyFindButton.UseVisualStyleBackColor = true;
			this.RelatedPartyFindButton.Click += new System.EventHandler(this.RelatedPartyFindButton_Click);
			// 
			// RelatedPartyGridFilterDirectionDropEdi
			// 
			this.RelatedPartyGridFilterDirectionDropEdi.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedPartyGridFilterDirectionDropEdi, "FilterFreightDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilterFreightDirection)));
			this.RelatedPartyGridFilterDirectionDropEdi.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1bd4c77f-07ed-4235-b059-8099c3ea7481", "Direction");
			this.RelatedPartyGridFilterDirectionDropEdi.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 25, true);
			this.RelatedPartyGridFilterDirectionDropEdi.Name = "RelatedPartyGridFilterDirectionDropEdi";
			this.RelatedPartyGridFilterDirectionDropEdi.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.RelatedPartyGridFilterDirectionDropEdi.TabIndex = 1;
			// 
			// RelatedPartyGridFilterPartyTypeDropEdi
			// 
			this.RelatedPartyGridFilterPartyTypeDropEdi.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedPartyGridFilterPartyTypeDropEdi, "FilterPartyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).FilterPartyType)));
			this.RelatedPartyGridFilterPartyTypeDropEdi.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("db55efdc-f46b-49f1-8153-41cc492a2a41", "Party Type");
			this.RelatedPartyGridFilterPartyTypeDropEdi.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 1, true);
			this.RelatedPartyGridFilterPartyTypeDropEdi.Name = "RelatedPartyGridFilterPartyTypeDropEdi";
			this.RelatedPartyGridFilterPartyTypeDropEdi.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.RelatedPartyGridFilterPartyTypeDropEdi.TabIndex = 0;
			// 
			// RelatedPartiesGrid
			// 
			this.RelatedPartiesGrid.AllowNavigation = false;
			this.RelatedPartiesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RelatedPartiesGrid, "AllRelatedPartiesView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_PartyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PartyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).CalculatedDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_FreightTransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_FreightContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_OH_RelatedParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).RelatedPartyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).CompanyLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_RN_NKImporterCountry)));
			this.RelatedPartiesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eac13c46-a2e4-4059-845e-1d816a9d7200", "Party Type");
			zDropEditColumnStyleInfo1.ColumnName = "PR_PartyType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dcc7414f-6545-4b2b-9ddf-570d15833c7f", "Party Type Description");
			zTextBoxColumnStyleInfo1.ColumnName = "PartyTypeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("110e6e7f-488a-47b4-89d7-04e36c48e0e0", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "CalculatedDirection";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("771d7236-0536-47af-9f4a-e0c283922f21", "Transport Mode");
			zDropEditColumnStyleInfo3.ColumnName = "PR_FreightTransportMode";
			zDropEditColumnStyleInfo3.ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c15efbd5-380c-4148-a90b-5654354ef9e5", "Container Mode");
			zDropEditColumnStyleInfo4.ColumnName = "PR_FreightContainerMode";
			zDropEditColumnStyleInfo4.ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3200d35e-8360-42fc-83f7-7531b8e8c208", "Related Party");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "PR_OH_RelatedParty";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("38854e68-8d12-478e-84bd-3fac040c0475", "Related Party Name");
			zTextBoxColumnStyleInfo2.ColumnName = "RelatedPartyName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c778af46-853b-4d07-8ab5-e71100f06022", "Company Level");
			zDropEditColumnStyleInfo5.ColumnName = "CompanyLevel";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("46380f62-ddd6-4c34-a9b1-272dde4ec953", "Location");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PR_Location";
			importerCountryCodeFindBoxColumnStyleInfo.ColumnName = "PR_RN_NKImporterCountry";
			importerCountryCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3a620ac5-88f2-485c-a8cc-a93eb4d82635", "Relationship Created Time");
			zDateEditColumnStyleInfo1.ColumnName = "PR_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9a24571a-abf1-40ee-884e-c9ce39f11c27", "Status");
			zTextBoxColumnStyleInfo11.ColumnName = "PR_CustomsStatus";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RelatedPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RelatedPartiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.RelatedPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RelatedPartiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RelatedPartiesGrid.ColumnStyles.Add(importerCountryCodeFindBoxColumnStyleInfo);
			this.RelatedPartiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RelatedPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.RelatedPartiesGrid.CopySelectedRowsAllowed = true;
			this.RelatedPartiesGrid.GridId = "1901e730-4333-4c0c-872e-6e04f70c8961";
			this.RelatedPartiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedPartiesGrid.LayoutKey = "zGrid1";
			this.RelatedPartiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 77, true);
			this.RelatedPartiesGrid.Name = "RelatedPartiesGrid";
			this.RelatedPartiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 86, true);
			this.RelatedPartiesGrid.TabIndex = 11;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.PartyTypeDescriptionDropEdit);
			this.zPanel1.Controls.Add(this.CalculatedDirectionDropEdit);
			this.zPanel1.Controls.Add(this.CompanyLevelDropEdit);
			this.zPanel1.Controls.Add(this.FreightTransportModeDropEdit);
			this.zPanel1.Controls.Add(this.FreightContainerModeDropEdit);
			this.zPanel1.Controls.Add(this.RelatedPartyGuidFindBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 163, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 84, true);
			this.zPanel1.TabIndex = 10;
			// 
			// PartyTypeDescriptionDropEdit
			// 
			this.PartyTypeDescriptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartyTypeDescriptionDropEdit, "AllRelatedPartiesView.PR_PartyType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_PartyType)));
			this.PartyTypeDescriptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 3, true);
			this.PartyTypeDescriptionDropEdit.Name = "PartyTypeDescriptionDropEdit";
			this.PartyTypeDescriptionDropEdit.PreBoundMaxLength = 30;
			this.PartyTypeDescriptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PartyTypeDescriptionDropEdit.TabIndex = 1;
			// 
			// CalculatedDirectionDropEdit
			// 
			this.CalculatedDirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CalculatedDirectionDropEdit, "AllRelatedPartiesView.CalculatedDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).CalculatedDirection)));
			this.CalculatedDirectionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fa1f1f8a-12d1-4cb4-a789-776800344bf2", "Direction");
			this.CalculatedDirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 29, true);
			this.CalculatedDirectionDropEdit.Name = "CalculatedDirectionDropEdit";
			this.CalculatedDirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.CalculatedDirectionDropEdit.TabIndex = 2;
			// 
			// CompanyLevelDropEdit
			// 
			this.CompanyLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyLevelDropEdit, "AllRelatedPartiesView.CompanyLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).CompanyLevel)));
			this.CompanyLevelDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e0c40b1b-a3a7-4dc2-9801-651146b44b32", "Company Level");
			this.CompanyLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 29, true);
			this.CompanyLevelDropEdit.Name = "CompanyLevelDropEdit";
			this.CompanyLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 20, true);
			this.CompanyLevelDropEdit.TabIndex = 9;
			// 
			// FreightTransportModeDropEdit
			// 
			this.FreightTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightTransportModeDropEdit, "AllRelatedPartiesView.PR_FreightTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_FreightTransportMode)));
			this.FreightTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 55, true);
			this.FreightTransportModeDropEdit.Name = "FreightTransportModeDropEdit";
			this.FreightTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.FreightTransportModeDropEdit.TabIndex = 3;
			// 
			// FreightTransportModeDropEdit
			// 
			this.FreightContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightContainerModeDropEdit, "AllRelatedPartiesView.PR_FreightContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_FreightContainerMode)));
			this.FreightContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 55, true);
			this.FreightContainerModeDropEdit.Name = "FreightContainerModeDropEdit";
			this.FreightContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.FreightContainerModeDropEdit.TabIndex = 3;
			// 
			// RelatedPartyGuidFindBox
			// 
			this.RelatedPartyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedPartyGuidFindBox, "AllRelatedPartiesView.PR_OH_RelatedParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllRelatedPartiesView)).SyncRoot)).PR_OH_RelatedParty)));
			this.RelatedPartyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 3, true);
			this.RelatedPartyGuidFindBox.Name = "RelatedPartyGuidFindBox";
			this.RelatedPartyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 20, true);
			this.RelatedPartyGuidFindBox.TabIndex = 4;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7c1cbbd9-992b-437e-8042-c6e2c1ad1454", "Parties Related To This Organization");
			this.zGroupBox1.Controls.Add(this.AllParentPartiesGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 104, true);
			this.zGroupBox1.TabIndex = 11;
			this.zGroupBox1.TabStop = false;
			// 
			// AllParentPartiesGrid
			// 
			this.AllParentPartiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AllParentPartiesGrid, "AllParentPartiesView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).ParentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).ParentPartyTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).CalculatedDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).PR_FreightTransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).PR_FreightContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).PR_OH_Parent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).ParentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).CompanyLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).PR_Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OrgRelatedParty)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).AllParentPartiesView)).SyncRoot)).PR_SystemCreateTimeUtc)));
			this.AllParentPartiesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9e72e8b1-2b8d-4cb4-b1a2-0020a4cfa5ae", "Party Type");
			zTextBoxColumnStyleInfo3.ColumnName = "ParentType";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("28fdd83e-3f38-4f1f-8fc0-708e91a861c3", "Party Type Description");
			zTextBoxColumnStyleInfo4.ColumnName = "ParentPartyTypeDescription";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0d484777-2d6b-4982-a72d-2da01b856de0", "Direction");
			zTextBoxColumnStyleInfo5.ColumnName = "CalculatedDirection";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e93786c7-5bae-4206-b060-41e7bb8e075d", "Transport Mode");
			zTextBoxColumnStyleInfo6.ColumnName = "PR_FreightTransportMode";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b31d8ace-5666-4c1a-a7ed-f37c5eae39f3", "Container Mode");
			zTextBoxColumnStyleInfo7.ColumnName = "PR_FreightContainerMode";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fadb7638-21cb-4dc1-81ca-c37e45ee08cd", "Relating Party");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "PR_OH_Parent";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f3329729-cb3b-45e1-b2a9-2b0715fe60fd", "Relating Party Name");
			zTextBoxColumnStyleInfo8.ColumnName = "ParentName";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2d0f1ee3-6ba2-4eb0-8115-ff6572f9ebb0", "", "Company Level");
			zTextBoxColumnStyleInfo9.ColumnName = "CompanyLevel";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("68783b09-c31d-4776-ac80-b809293ff08c", "", "Location");
			zTextBoxColumnStyleInfo10.ColumnName = "PR_Location";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f7f604f0-1661-4cda-b6c8-8639e4b5dc76", "", "Relationship Created Time");
			zDateEditColumnStyleInfo2.ColumnName = "PR_SystemCreateTimeUtc";
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.AllParentPartiesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.AllParentPartiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.AllParentPartiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.AllParentPartiesGrid.CopySelectedRowsAllowed = true;
			this.AllParentPartiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllParentPartiesGrid.GridId = "664f73a3-ab43-43d4-bda2-087314696d7b";
			this.AllParentPartiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AllParentPartiesGrid.IsWholeRowSelectedOnClick = true;
			this.AllParentPartiesGrid.LayoutKey = "AllParentPartiesGrid";
			this.AllParentPartiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AllParentPartiesGrid.Name = "AllParentPartiesGrid";
			this.AllParentPartiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 85, true);
			this.AllParentPartiesGrid.TabIndex = 10;
			// 
			// RelatedPartiesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RelatedPartiesGroupBox);
			this.Name = "RelatedPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 377, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RelatedPartiesGroupBox.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.zGroupBox2.ResumeLayout(false);
			this.zPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RelatedPartiesGrid)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AllParentPartiesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox RelatedPartiesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CompanyLevelDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox RelatedPartyGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FreightTransportModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FreightContainerModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CalculatedDirectionDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PartyTypeDescriptionDropEdit;
		private ZArchitecture.ZGrid AllParentPartiesGrid;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.ZGrid RelatedPartiesGrid;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZPanel zPanel2;
		private ZArchitecture.GUI.ZButton RelatedPartyClearButton;
		private ZArchitecture.GUI.ZButton RelatedPartyFindButton;
		private ZArchitecture.GUI.ZDropEdit RelatedPartyGridFilterDirectionDropEdi;
		private ZArchitecture.GUI.ZDropEdit RelatedPartyGridFilterPartyTypeDropEdi;
	}
}
