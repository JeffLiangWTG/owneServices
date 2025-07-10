namespace Enterprise.Freight.Agency.GUI
{
	partial class BulkMovementsControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.attachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.detachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.childrenGrid = new Enterprise.ZArchitecture.ZGrid();
			topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			organisationOverrideGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			responsiblePartyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			principalFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			voyageVesselGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			depotGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			depotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			movementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			containerEmptyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			movementDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			topPanel.SuspendLayout();
			organisationOverrideGroupBox.SuspendLayout();
			voyageVesselGroupBox.SuspendLayout();
			depotGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.childrenGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BulkMovementsHeader);
			// 
			// topPanel
			// 
			topPanel.Controls.Add(organisationOverrideGroupBox);
			topPanel.Controls.Add(voyageVesselGroupBox);
			topPanel.Controls.Add(depotGroupBox);
			topPanel.Controls.Add(movementTypeDropEdit);
			topPanel.Controls.Add(containerEmptyCheckBox);
			topPanel.Controls.Add(movementDateEdit);
			topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			topPanel.Name = "topPanel";
			topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 120, true);
			topPanel.TabIndex = 0;
			// 
			// organisationOverrideGroupBox
			// 
			organisationOverrideGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|cfa9668c-a0a2-4643-ae4a-3e13fdba1679", "Organization Overrides");
			organisationOverrideGroupBox.Controls.Add(responsiblePartyFindBox);
			organisationOverrideGroupBox.Controls.Add(principalFindBox);
			organisationOverrideGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 32, true);
			organisationOverrideGroupBox.Name = "organisationOverrideGroupBox";
			organisationOverrideGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 80, true);
			organisationOverrideGroupBox.TabIndex = 4;
			organisationOverrideGroupBox.TabStop = false;
			// 
			// responsiblePartyFindBox
			// 
			responsiblePartyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(responsiblePartyFindBox, "ResponsiblePartyPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).ResponsiblePartyPK)));
			responsiblePartyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 16, true);
			responsiblePartyFindBox.Name = "responsiblePartyFindBox";
			responsiblePartyFindBox.ShowDescriptionBox = false;
			responsiblePartyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			responsiblePartyFindBox.TabIndex = 0;
			// 
			// principalFindBox
			// 
			principalFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(principalFindBox, "PrincipalPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).PrincipalPK)));
			principalFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 40, true);
			principalFindBox.Name = "principalFindBox";
			principalFindBox.ShowDescriptionBox = false;
			principalFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			principalFindBox.TabIndex = 1;
			// 
			// voyageVesselGroupBox
			// 
			voyageVesselGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|2814cecc-7115-437b-9c92-8559c27d1af7", "Voyage");
			voyageVesselGroupBox.Controls.Add(this.attachButton);
			voyageVesselGroupBox.Controls.Add(this.detachButton);
			voyageVesselGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 32, true);
			voyageVesselGroupBox.Name = "voyageVesselGroupBox";
			voyageVesselGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 80, true);
			voyageVesselGroupBox.TabIndex = 5;
			voyageVesselGroupBox.TabStop = false;
			// 
			// attachButton
			// 
			this.attachButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|bf3bf641-b8b1-41ee-a151-c39bfe0229b5", "Attach");
			this.attachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.attachButton.Name = "attachButton";
			this.attachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.attachButton.TabIndex = 0;
			this.attachButton.Click += new System.EventHandler(this.attachButton_Click);
			// 
			// detachButton
			// 
			this.detachButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|ba607e9f-72df-45ad-8099-176a5cf6d89a", "Detach");
			this.detachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.detachButton.Name = "detachButton";
			this.detachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.detachButton.TabIndex = 1;
			this.detachButton.Click += new System.EventHandler(this.detachButton_Click);
			// 
			// depotGroupBox
			// 
			depotGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|6744887b-0795-4515-a7c1-65a3bef25c11", "Depot");
			depotGroupBox.Controls.Add(depotAddressControl);
			depotGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			depotGroupBox.Name = "depotGroupBox";
			depotGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 80, true);
			depotGroupBox.TabIndex = 3;
			depotGroupBox.TabStop = false;
			// 
			// depotAddressControl
			// 
			depotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(depotAddressControl, "DepotAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).DepotAddressPK)));
			depotAddressControl.BindToOrgList = "Lookups+DepotOrgList";
			depotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			depotAddressControl.Name = "depotAddressControl";
			depotAddressControl.PopupCaption = "";
			depotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 59, true);
			depotAddressControl.TabIndex = 0;
			// 
			// movementTypeDropEdit
			// 
			movementTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(movementTypeDropEdit, "MovementType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).MovementType)));
			movementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 8, true);
			movementTypeDropEdit.Name = "movementTypeDropEdit";
			movementTypeDropEdit.PreBoundMaxLength = 3;
			movementTypeDropEdit.ShowDescriptionBox = false;
			movementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			movementTypeDropEdit.TabIndex = 0;
			// 
			// containerEmptyCheckBox
			// 
			containerEmptyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(containerEmptyCheckBox, "ContainerIsEmpty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).ContainerIsEmpty)));
			containerEmptyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			containerEmptyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 8, true);
			containerEmptyCheckBox.Name = "containerEmptyCheckBox";
			containerEmptyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 17, true);
			containerEmptyCheckBox.TabIndex = 2;
			// 
			// movementDateEdit
			// 
			movementDateEdit.AllowDrop = true;
			movementDateEdit.AutoCompleteMonthThreshold = 1;
			movementDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(movementDateEdit, "MovementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).MovementDate)));
			movementDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			movementDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 8, true);
			movementDateEdit.Name = "movementDateEdit";
			movementDateEdit.TabIndex = 1;
			// 
			// childrenGrid
			// 
			this.childrenGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.childrenGrid, "Children");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).OwnerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).MovementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).MovementDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).DepotAddressPK_ZAddress.OrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).Lookups.DepotOrgList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).DepotAddressPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).DepotAddressPK_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).IsGenerated)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).Condition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).Damage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).ContainerIsEmpty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).ResponsiblePartyPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).VoyageNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.BulkMovementsChild)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BulkMovementsHeader)(null)).Children)).SyncRoot)).LeaseContractNo)));
			this.childrenGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ContainerNum";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ContainerType";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "OwnerType";
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "MovementType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo1.ColumnName = "MovementDate";
			zGuidFindBoxColumnStyleInfo2.BindToList = "Lookups+DepotOrgList";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|d876cde3-4c95-4fbb-b7c3-68337768cadb", "Depot", "The depot where the movement took place.");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "DepotAddressPK_ZAddress+OrgPK";
			zGuidFindBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|e45a5f97-8950-4e51-8d6a-1160cba35226", "Depot");
			zGuidDropEditColumnStyleInfo1.BindToList = "DepotAddressPK_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo1.ColumnName = "DepotAddressPK";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|e45a5f97-8950-4e51-8d6a-1160cba35226", "Depot");
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsGenerated";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "Condition";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "Damage";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "ContainerIsEmpty";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "PrincipalPK";
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.ColumnName = "ResponsiblePartyPK";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "VesselName";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|691299e9-5d6f-4d69-8ec2-8d1bf6e6f9b9", "Voyage");
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "VoyageNo";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Freight.Agency.GUI.Res.GetData("BulkMovementsControl|691299e9-5d6f-4d69-8ec2-8d1bf6e6f9b9", "Voyage");
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "LeaseContractNo";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.IsVisible = false;
			this.childrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.childrenGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.childrenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.childrenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.childrenGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.childrenGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.childrenGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.childrenGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.childrenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.childrenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.childrenGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.childrenGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.childrenGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.childrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.childrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.childrenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.childrenGrid.GridId = "7b30c955-ec05-4699-a72f-5563f2649e9a";
			this.childrenGrid.CopySelectedRowsAllowed = true;
			this.childrenGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.childrenGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.childrenGrid.LayoutKey = "childrenGrid";
			this.childrenGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.childrenGrid.Name = "childrenGrid";
			this.childrenGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 80, true);
			this.childrenGrid.TabIndex = 1;
			// 
			// BulkMovementsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.childrenGrid);
			this.Controls.Add(topPanel);
			this.Name = "BulkMovementsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			topPanel.ResumeLayout(false);
			topPanel.PerformLayout();
			organisationOverrideGroupBox.ResumeLayout(false);
			voyageVesselGroupBox.ResumeLayout(false);
			depotGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.childrenGrid)).EndInit();
			this.ResumeLayout(false);

		}

		ZArchitecture.GUI.ZButton attachButton;
		ZArchitecture.GUI.ZButton detachButton;
		ZArchitecture.ZGrid childrenGrid;
		Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox organisationOverrideGroupBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox responsiblePartyFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox principalFindBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox voyageVesselGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox depotGroupBox;
		Enterprise.ZArchitecture.GUI.ZAddressControl depotAddressControl;
		Enterprise.ZArchitecture.GUI.ZDropEdit movementTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox containerEmptyCheckBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit movementDateEdit;
	}
}
