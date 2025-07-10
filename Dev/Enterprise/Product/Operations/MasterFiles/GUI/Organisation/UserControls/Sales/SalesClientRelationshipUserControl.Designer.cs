namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesClientRelationshipControl
	{

		#region Component Designer generated code

		private Enterprise.ZArchitecture.GUI.ZGroupBox SalesRepsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CMControllingAgentBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit OM_CMClientCommencedBoundDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CMCostEffectsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CMClientRankingsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMAmountOfElectronicIntegrationDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMEaseClientCanBePoachedDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMClientsDesireToRemainDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OM_CMOverallClientRelationDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ManagementGroupingGroupBox;
		private OrgManagementGroupingControl ManagementGroupingUserControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CommissionManagementGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OM_GC_CMPreferredPaymentCompanyGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PreferredPaymentCompanyGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox UseTransactionCompanyAsPreferredPaymentCheckBox;

		private void InitializeComponent()
		{
			this.SalesRepsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CMControllingAgentBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OM_CMClientCommencedBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CMCostEffectsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CMClientRankingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_CMAmountOfElectronicIntegrationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CMEaseClientCanBePoachedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CMClientsDesireToRemainDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OM_CMOverallClientRelationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManagementGroupingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManagementGroupingUserControl = new Enterprise.MasterFiles.GUI.OrgManagementGroupingControl();
			this.CommissionManagementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreferredPaymentCompanyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UseTransactionCompanyAsPreferredPaymentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SalesRepsGroupBox.SuspendLayout();
			this.CMControllingAgentBoundGuidFindBox.SuspendLayout();
			this.OM_CMClientCommencedBoundDateEdit.SuspendLayout();
			this.CMCostEffectsGroupBox.SuspendLayout();
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.SuspendLayout();
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.SuspendLayout();
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.SuspendLayout();
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.SuspendLayout();
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.SuspendLayout();
			this.CMClientRankingsGroupBox.SuspendLayout();
			this.OM_CMAmountOfElectronicIntegrationDropEdit.SuspendLayout();
			this.OM_CMEaseClientCanBePoachedDropEdit.SuspendLayout();
			this.OM_CMClientsDesireToRemainDropEdit.SuspendLayout();
			this.OM_CMOverallClientRelationDropEdit.SuspendLayout();
			this.ManagementGroupingGroupBox.SuspendLayout();
			this.ManagementGroupingUserControl.SuspendLayout();
			this.CommissionManagementGroupBox.SuspendLayout();
			this.PreferredPaymentCompanyGroupBox.SuspendLayout();
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// SalesRepsGroupBox
			// 
			this.SalesRepsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesClientRelationshipControl|f2fc8beb-b4c6-42a0-844a-4c27b280e7bf", "Client Management");
			this.SalesRepsGroupBox.Controls.Add(this.CMControllingAgentBoundGuidFindBox);
			this.SalesRepsGroupBox.Controls.Add(this.OM_CMClientCommencedBoundDateEdit);
			this.SalesRepsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SalesRepsGroupBox.Name = "SalesRepsGroupBox";
			this.SalesRepsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 70, true);
			this.SalesRepsGroupBox.TabIndex = 0;
			this.SalesRepsGroupBox.TabStop = false;
			// 
			// CMControllingAgentBoundGuidFindBox
			// 
			this.CMControllingAgentBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CMControllingAgentBoundGuidFindBox, "ControllingAgentPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).ControllingAgentPK)));
			this.CMControllingAgentBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.CMControllingAgentBoundGuidFindBox.Name = "CMControllingAgentBoundGuidFindBox";
			this.CMControllingAgentBoundGuidFindBox.PopupCaption = "Select Controlling Agent";
			this.CMControllingAgentBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 16, true);
			this.CMControllingAgentBoundGuidFindBox.TabIndex = 0;
			// 
			// OM_CMClientCommencedBoundDateEdit
			// 
			this.OM_CMClientCommencedBoundDateEdit.AllowDrop = true;
			this.OM_CMClientCommencedBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.OM_CMClientCommencedBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OM_CMClientCommencedBoundDateEdit, "MiscServ.OM_CMClientCommenced");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMClientCommenced)));
			this.OM_CMClientCommencedBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 42, true);
			this.OM_CMClientCommencedBoundDateEdit.Name = "OM_CMClientCommencedBoundDateEdit";
			this.OM_CMClientCommencedBoundDateEdit.TabIndex = 1;
			// 
			// CMCostEffectsGroupBox
			// 
			this.CMCostEffectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CMCostEffectsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesClientRelationshipControl|80a8e03c-7fa4-4423-865a-c0303886e7e2", "Overall Client Effect on Costs");
			this.CMCostEffectsGroupBox.Controls.Add(this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit);
			this.CMCostEffectsGroupBox.Controls.Add(this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit);
			this.CMCostEffectsGroupBox.Controls.Add(this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit);
			this.CMCostEffectsGroupBox.Controls.Add(this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit);
			this.CMCostEffectsGroupBox.Controls.Add(this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit);
			this.CMCostEffectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 343, true);
			this.CMCostEffectsGroupBox.Name = "CMCostEffectsGroupBox";
			this.CMCostEffectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 140, true);
			this.CMCostEffectsGroupBox.TabIndex = 3;
			this.CMCostEffectsGroupBox.TabStop = false;
			// 
			// OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit
			// 
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit, "MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts)));
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 88, true);
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.Name = "OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit";
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 16, true);
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.TabIndex = 3;
			// 
			// OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit
			// 
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit, "MiscServ.OM_CMOverallEffectOfClientOnOtherCosts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMOverallEffectOfClientOnOtherCosts)));
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 112, true);
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.Name = "OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit";
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 16, true);
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.TabIndex = 4;
			// 
			// OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit
			// 
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit, "MiscServ.OM_CMOverallEffectOfClientOnTEUCosts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMOverallEffectOfClientOnTEUCosts)));
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 64, true);
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.Name = "OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit";
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 16, true);
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.TabIndex = 2;
			// 
			// OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit
			// 
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit, "MiscServ.OM_CMOverallEffectOfClientOnLCLCosts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMOverallEffectOfClientOnLCLCosts)));
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 40, true);
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.Name = "OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit";
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 16, true);
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.TabIndex = 1;
			// 
			// OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit
			// 
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit, "MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts)));
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.Name = "OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit";
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 16, true);
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.TabIndex = 0;
			// 
			// CMClientRankingsGroupBox
			// 
			this.CMClientRankingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CMClientRankingsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesClientRelationshipControl|07993044-d47d-466f-abea-906a8f527697", "Client Rankings");
			this.CMClientRankingsGroupBox.Controls.Add(this.OM_CMAmountOfElectronicIntegrationDropEdit);
			this.CMClientRankingsGroupBox.Controls.Add(this.OM_CMEaseClientCanBePoachedDropEdit);
			this.CMClientRankingsGroupBox.Controls.Add(this.OM_CMClientsDesireToRemainDropEdit);
			this.CMClientRankingsGroupBox.Controls.Add(this.OM_CMOverallClientRelationDropEdit);
			this.CMClientRankingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 489, true);
			this.CMClientRankingsGroupBox.Name = "CMClientRankingsGroupBox";
			this.CMClientRankingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 111, true);
			this.CMClientRankingsGroupBox.TabIndex = 4;
			this.CMClientRankingsGroupBox.TabStop = false;
			// 
			// OM_CMAmountOfElectronicIntegrationDropEdit
			// 
			this.OM_CMAmountOfElectronicIntegrationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMAmountOfElectronicIntegrationDropEdit, "MiscServ.OM_CMAmountOfElectronicIntegration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMAmountOfElectronicIntegration)));
			this.OM_CMAmountOfElectronicIntegrationDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesClientRelationshipControl|082d6fad-6358-4d22-bb5a-187466b4aadf", "Amount of Client Electronic Integration");
			this.OM_CMAmountOfElectronicIntegrationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 85, true);
			this.OM_CMAmountOfElectronicIntegrationDropEdit.MaxItemsToShowInDropDown = 11;
			this.OM_CMAmountOfElectronicIntegrationDropEdit.Name = "OM_CMAmountOfElectronicIntegrationDropEdit";
			this.OM_CMAmountOfElectronicIntegrationDropEdit.PreBoundMaxLength = 2;
			this.OM_CMAmountOfElectronicIntegrationDropEdit.ShowDescriptionBox = false;
			this.OM_CMAmountOfElectronicIntegrationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OM_CMAmountOfElectronicIntegrationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 16, true);
			this.OM_CMAmountOfElectronicIntegrationDropEdit.TabIndex = 3;
			// 
			// OM_CMEaseClientCanBePoachedDropEdit
			// 
			this.OM_CMEaseClientCanBePoachedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMEaseClientCanBePoachedDropEdit, "MiscServ.OM_CMEaseClientCanBePoached");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMEaseClientCanBePoached)));
			this.OM_CMEaseClientCanBePoachedDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesClientRelationshipControl|1f486166-723e-415f-97c7-aade02432b2c", "Difficulty with which Client can be poached");
			this.OM_CMEaseClientCanBePoachedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 61, true);
			this.OM_CMEaseClientCanBePoachedDropEdit.MaxItemsToShowInDropDown = 11;
			this.OM_CMEaseClientCanBePoachedDropEdit.Name = "OM_CMEaseClientCanBePoachedDropEdit";
			this.OM_CMEaseClientCanBePoachedDropEdit.PreBoundMaxLength = 2;
			this.OM_CMEaseClientCanBePoachedDropEdit.ShowDescriptionBox = false;
			this.OM_CMEaseClientCanBePoachedDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OM_CMEaseClientCanBePoachedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 16, true);
			this.OM_CMEaseClientCanBePoachedDropEdit.TabIndex = 2;
			// 
			// OM_CMClientsDesireToRemainDropEdit
			// 
			this.OM_CMClientsDesireToRemainDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMClientsDesireToRemainDropEdit, "MiscServ.OM_CMClientsDesireToRemain");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMClientsDesireToRemain)));
			this.OM_CMClientsDesireToRemainDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesClientRelationshipControl|273466fb-0e23-4b8a-a496-cacfa0792e7d", "Client Desire to Remain with Company");
			this.OM_CMClientsDesireToRemainDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 37, true);
			this.OM_CMClientsDesireToRemainDropEdit.MaxItemsToShowInDropDown = 11;
			this.OM_CMClientsDesireToRemainDropEdit.Name = "OM_CMClientsDesireToRemainDropEdit";
			this.OM_CMClientsDesireToRemainDropEdit.PreBoundMaxLength = 2;
			this.OM_CMClientsDesireToRemainDropEdit.ShowDescriptionBox = false;
			this.OM_CMClientsDesireToRemainDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OM_CMClientsDesireToRemainDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 16, true);
			this.OM_CMClientsDesireToRemainDropEdit.TabIndex = 1;
			// 
			// OM_CMOverallClientRelationDropEdit
			// 
			this.OM_CMOverallClientRelationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_CMOverallClientRelationDropEdit, "MiscServ.OM_CMOverallClientRelation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMOverallClientRelation)));
			this.OM_CMOverallClientRelationDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SalesClientRelationshipControl|e6475071-24a3-4f4c-920b-950a775cbd86", "Overall Client Relationship");
			this.OM_CMOverallClientRelationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 13, true);
			this.OM_CMOverallClientRelationDropEdit.MaxItemsToShowInDropDown = 11;
			this.OM_CMOverallClientRelationDropEdit.Name = "OM_CMOverallClientRelationDropEdit";
			this.OM_CMOverallClientRelationDropEdit.PreBoundMaxLength = 2;
			this.OM_CMOverallClientRelationDropEdit.ShowDescriptionBox = false;
			this.OM_CMOverallClientRelationDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OM_CMOverallClientRelationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 16, true);
			this.OM_CMOverallClientRelationDropEdit.TabIndex = 0;
			// 
			// ManagementGroupingGroupBox
			// 
			this.ManagementGroupingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ManagementGroupingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("65f8aed5-1a7b-423a-bc2c-1b6eba76a709", "Organization Parent / Subsidiary Grouping");
			this.ManagementGroupingGroupBox.Controls.Add(this.ManagementGroupingUserControl);
			this.ManagementGroupingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.ManagementGroupingGroupBox.Name = "ManagementGroupingGroupBox";
			this.ManagementGroupingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 261, true);
			this.ManagementGroupingGroupBox.TabIndex = 1;
			this.ManagementGroupingGroupBox.TabStop = false;
			// 
			// ManagementGroupingUserControl
			// 
			this.ManagementGroupingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManagementGroupingUserControl, "OrgManagementGroupingModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgManagementGroupingModel)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgManagementGroupingModel)));
			this.ManagementGroupingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManagementGroupingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ManagementGroupingUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.ManagementGroupingUserControl.Name = "ManagementGroupingUserControl";
			this.ManagementGroupingUserControl.NameOfATreeElement = Enterprise.MasterFiles.GUI.Res.GetData("25f13800-c251-4c06-bec5-5bf0dd2f2403", "Organization");
			this.ManagementGroupingUserControl.NameOfTreeElementsPlural = Enterprise.MasterFiles.GUI.Res.GetData("dd07a2a8-d4cb-453a-b7db-254133628d37", "Organizations");
			this.ManagementGroupingUserControl.ShowNewButton = false;
			this.ManagementGroupingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 246, true);
			this.ManagementGroupingUserControl.TabIndex = 5;
			// 
			// CommissionManagementGroupBox
			// 
			this.CommissionManagementGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CommissionManagementGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("292c838f-e960-404b-8c31-3bdf8f7bcb6e", "Commission Management");
			this.CommissionManagementGroupBox.Controls.Add(this.PreferredPaymentCompanyGroupBox);
			this.CommissionManagementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 0, true);
			this.CommissionManagementGroupBox.Name = "CommissionManagementGroupBox";
			this.CommissionManagementGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CommissionManagementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 597, true);
			this.CommissionManagementGroupBox.TabIndex = 5;
			this.CommissionManagementGroupBox.TabStop = false;
			// 
			// PreferredPaymentCompanyGroupBox
			// 
			this.PreferredPaymentCompanyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PreferredPaymentCompanyGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("59bc43c1-8451-400c-91ee-243d8c5076a6", "Preferred Payment Company");
			this.PreferredPaymentCompanyGroupBox.Controls.Add(this.UseTransactionCompanyAsPreferredPaymentCheckBox);
			this.PreferredPaymentCompanyGroupBox.Controls.Add(this.OM_GC_CMPreferredPaymentCompanyGuidFindBox);
			this.PreferredPaymentCompanyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
			this.PreferredPaymentCompanyGroupBox.Name = "PreferredPaymentCompanyGroupBox";
			this.PreferredPaymentCompanyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 58, true);
			this.PreferredPaymentCompanyGroupBox.TabIndex = 1;
			this.PreferredPaymentCompanyGroupBox.TabStop = false;
			// 
			// UseTransactionCompanyAsPreferredPaymentCheckBox
			// 
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseTransactionCompanyAsPreferredPaymentCheckBox, "MiscServ.UseTransactionCompanyAsPreferredPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.UseTransactionCompanyAsPreferredPayment)));
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 17, true);
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.Name = "UseTransactionCompanyAsPreferredPaymentCheckBox";
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.TabIndex = 0;
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.UseVisualStyleBackColor = true;
			// 
			// OM_GC_CMPreferredPaymentCompanyGuidFindBox
			// 
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OM_GC_CMPreferredPaymentCompanyGuidFindBox, "MiscServ.OM_GC_CMPreferredPaymentCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_GC_CMPreferredPaymentCompany)));
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b57ca69e-3e93-41a5-93a0-dda1b32f5d8c", "Specific");
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 34, true);
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.Name = "OM_GC_CMPreferredPaymentCompanyGuidFindBox";
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 16, true);
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.TabIndex = 1;
			// 
			// SalesClientRelationshipControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommissionManagementGroupBox);
			this.Controls.Add(this.ManagementGroupingGroupBox);
			this.Controls.Add(this.SalesRepsGroupBox);
			this.Controls.Add(this.CMCostEffectsGroupBox);
			this.Controls.Add(this.CMClientRankingsGroupBox);
			this.Name = "SalesClientRelationshipControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(857, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SalesRepsGroupBox.ResumeLayout(false);
			this.SalesRepsGroupBox.PerformLayout();
			this.CMControllingAgentBoundGuidFindBox.ResumeLayout(true);
			this.CMControllingAgentBoundGuidFindBox.PerformLayout();
			this.OM_CMClientCommencedBoundDateEdit.ResumeLayout(true);
			this.OM_CMClientCommencedBoundDateEdit.PerformLayout();
			this.CMCostEffectsGroupBox.ResumeLayout(false);
			this.CMCostEffectsGroupBox.PerformLayout();
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.ResumeLayout(true);
			this.OM_CMOverallEffectOfClientOnWarehousingCostsBoundDropEdit.PerformLayout();
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.ResumeLayout(true);
			this.OM_CMOverallEffectOfClientOnOtherCostsBoundDropEdit.PerformLayout();
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.ResumeLayout(true);
			this.OM_CMOverallEffectOfClientOnTEUCostsBoundDropEdit.PerformLayout();
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.ResumeLayout(true);
			this.OM_CMOverallEffectOfClientOnLCLCostsBoundDropEdit.PerformLayout();
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.ResumeLayout(true);
			this.OM_CMOverallEffectOfClientOnAirfreightCostsBoundDropEdit.PerformLayout();
			this.CMClientRankingsGroupBox.ResumeLayout(false);
			this.CMClientRankingsGroupBox.PerformLayout();
			this.OM_CMAmountOfElectronicIntegrationDropEdit.ResumeLayout(true);
			this.OM_CMAmountOfElectronicIntegrationDropEdit.PerformLayout();
			this.OM_CMEaseClientCanBePoachedDropEdit.ResumeLayout(true);
			this.OM_CMEaseClientCanBePoachedDropEdit.PerformLayout();
			this.OM_CMClientsDesireToRemainDropEdit.ResumeLayout(true);
			this.OM_CMClientsDesireToRemainDropEdit.PerformLayout();
			this.OM_CMOverallClientRelationDropEdit.ResumeLayout(true);
			this.OM_CMOverallClientRelationDropEdit.PerformLayout();
			this.ManagementGroupingGroupBox.ResumeLayout(false);
			this.ManagementGroupingGroupBox.PerformLayout();
			this.ManagementGroupingUserControl.ResumeLayout(true);
			this.ManagementGroupingUserControl.PerformLayout();
			this.CommissionManagementGroupBox.ResumeLayout(false);
			this.CommissionManagementGroupBox.PerformLayout();
			this.PreferredPaymentCompanyGroupBox.ResumeLayout(false);
			this.PreferredPaymentCompanyGroupBox.PerformLayout();
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.ResumeLayout(true);
			this.OM_GC_CMPreferredPaymentCompanyGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
