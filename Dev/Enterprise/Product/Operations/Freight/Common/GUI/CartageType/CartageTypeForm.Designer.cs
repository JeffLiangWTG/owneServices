using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Common.GUI
{
	public partial class CartageTypeForm
	{
		private ZCheckBox IsHiddenCheckBox;
		private ZDropEdit ShipmentTransModeDropEdit;
		private ZCheckBox IsSystemCheckBox;
		private ZGuidFindBox zGuidFindBox1;
		private ZGroupBox OrgTypeGroupBox;
		private ZGrid JobOrgTypeGrid;
		private ZPanel zPanel1;
		private ZTextBox zTextBox1;
		private ZDropEdit zDropEdit7;
		private ZDropEdit zDropEdit4;
		private ZGroupBox zGroupBox3;
		private ZGroupBox zGroupBox1;
		private ZGrid LooseLegsGrid;
		private ZDropEdit zDropEdit6;
		private ZGuidDropEdit zDropEdit9;
		private ZGuidDropEdit zDropEdit10;
		private ZGroupBox zGroupBox2;
		private ZGroupBox CartageLegsGroupBox;
		private ZGrid ContainerLegsGrid;
		private ZDropEdit zDropEdit5;
		private ZGuidDropEdit zDropEdit2;
		private ZGuidDropEdit zDropEdit1;
		private CargoWise.Windows.UI.KSplitContainer ContainerLooseBookingSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private ZTranslatableTextControl DescriptionTranslatableTextControl;
		private ZGroupBox JobTypeGroupBox;

		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 514, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 492, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 492, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 514, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1327);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CommonCartageType);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).ContainerMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).Direction)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageType)(null)).E3_JobType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonCartageType)(null)).E3_GE)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CommonCartageType)(null)).E3_IsSystem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).E3_ShippingTransportMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CommonCartageType)(null)).E3_IsHidden)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CommonCartageType)(null)).CommonCartageOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageOrg)(((System.Collections.IList)(((CommonCartageType)(null)).CommonCartageOrganisations)).SyncRoot)).E5_OrgType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageOrg)(((System.Collections.IList)(((CommonCartageType)(null)).CommonCartageOrganisations)).SyncRoot)).OrgTypeFullDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageOrg)(((System.Collections.IList)(((CommonCartageType)(null)).CommonCartageOrganisations)).SyncRoot)).E5_UsageComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CommonCartageOrg)(((System.Collections.IList)(((CommonCartageType)(null)).CommonCartageOrganisations)).SyncRoot)).E5_IsBillToParty)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).E4_DisplayOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).E4_E5_FromOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).FromOrgDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).E4_E5_WaitPointOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).WaitPointOrgDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).E4_E5_ToOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).ToOrgDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).ContainerizedCartageLegTypes)).SyncRoot)).E4_EquipmentGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).ContainerizedBooking.E4_EquipmentGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).ContainerizedBooking.E4_E5_WaitPointOrg)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).ContainerizedBooking.E4_E5_FromOrg)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).E4_DisplayOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).E4_E5_FromOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).FromOrgDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).E4_E5_WaitPointOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).WaitPointOrgDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).E4_E5_ToOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).ToOrgDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageLegType)(((System.Collections.IList)(((CommonCartageType)(null)).LooseCartageLegTypes)).SyncRoot)).E4_EquipmentGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).LooseBooking.E4_EquipmentGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).LooseBooking.E4_E5_WaitPointOrg)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CommonCartageType)(null)).LooseBooking.E4_E5_FromOrg)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CommonCartageType)(null)).E3_Description)));
			// 
			// CartageTypeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|af2275d0-73ea-43f2-b4fb-8d27b95b6dc5", "Port Transport Job Type");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 570, true);
			this.DataSourceType = typeof(CommonCartageType);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 606, true);
			this.Name = "CartageTypeForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo4 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo5 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo6 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			this.JobTypeGroupBox = new ZGroupBox();
			this.zDropEdit7 = new ZDropEdit();
			this.zDropEdit4 = new ZDropEdit();
			this.zTextBox1 = new ZTextBox();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.IsSystemCheckBox = new ZCheckBox();
			this.ShipmentTransModeDropEdit = new ZDropEdit();
			this.IsHiddenCheckBox = new ZCheckBox();
			this.OrgTypeGroupBox = new ZGroupBox();
			this.JobOrgTypeGrid = new ZGrid();
			this.zPanel1 = new ZPanel();
			this.ContainerLooseBookingSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox2 = new ZGroupBox();
			this.CartageLegsGroupBox = new ZGroupBox();
			this.ContainerLegsGrid = new ZGrid();
			this.zDropEdit5 = new ZDropEdit();
			this.zDropEdit2 = new ZGuidDropEdit();
			this.zDropEdit1 = new ZGuidDropEdit();
			this.zGroupBox3 = new ZGroupBox();
			this.zGroupBox1 = new ZGroupBox();
			this.LooseLegsGrid = new ZGrid();
			this.zDropEdit6 = new ZDropEdit();
			this.zDropEdit9 = new ZGuidDropEdit();
			this.zDropEdit10 = new ZGuidDropEdit();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.DescriptionTranslatableTextControl = new ZTranslatableTextControl();
			this.MainTabPage.SuspendLayout();
			this.JobTypeGroupBox.SuspendLayout();
			this.zDropEdit7.SuspendLayout();
			this.zDropEdit4.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.ShipmentTransModeDropEdit.SuspendLayout();
			this.OrgTypeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobOrgTypeGrid)).BeginInit();
			this.JobOrgTypeGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainerLooseBookingSplitContainer)).BeginInit();
			this.ContainerLooseBookingSplitContainer.Panel1.SuspendLayout();
			this.ContainerLooseBookingSplitContainer.Panel2.SuspendLayout();
			this.ContainerLooseBookingSplitContainer.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.CartageLegsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainerLegsGrid)).BeginInit();
			this.ContainerLegsGrid.SuspendLayout();
			this.zDropEdit5.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LooseLegsGrid)).BeginInit();
			this.LooseLegsGrid.SuspendLayout();
			this.zDropEdit6.SuspendLayout();
			this.zDropEdit9.SuspendLayout();
			this.zDropEdit10.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.DescriptionTranslatableTextControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.zPanel1);
			// 
			// JobTypeGroupBox
			// 
			this.JobTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.JobTypeGroupBox.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|135f7a43-d718-4bdb-8e99-808084f38636", "Job Type");
			this.JobTypeGroupBox.Controls.Add(this.DescriptionTranslatableTextControl);
			this.JobTypeGroupBox.Controls.Add(this.zDropEdit7);
			this.JobTypeGroupBox.Controls.Add(this.zDropEdit4);
			this.JobTypeGroupBox.Controls.Add(this.zTextBox1);
			this.JobTypeGroupBox.Controls.Add(this.zGuidFindBox1);
			this.JobTypeGroupBox.Controls.Add(this.IsSystemCheckBox);
			this.JobTypeGroupBox.Controls.Add(this.ShipmentTransModeDropEdit);
			this.JobTypeGroupBox.Controls.Add(this.IsHiddenCheckBox);
			this.JobTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 5, true);
			this.JobTypeGroupBox.Name = "JobTypeGroupBox";
			this.JobTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 100, true);
			this.JobTypeGroupBox.TabIndex = 0;
			this.JobTypeGroupBox.TabStop = false;
			// 
			// zDropEdit7
			// 
			this.zDropEdit7.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit7, "ContainerMode");
			this.zDropEdit7.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|54b012ce-f5cd-42fe-b974-062e561d7cd4", "Container Mode", "Container Mode", "");
			this.zDropEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 66, true);
			this.zDropEdit7.Name = "zDropEdit7";
			this.zDropEdit7.PreBoundMaxLength = 3;
			this.zDropEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 17, true);
			this.zDropEdit7.TabIndex = 2;
			// 
			// zDropEdit4
			// 
			this.zDropEdit4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit4, "Direction");
			this.zDropEdit4.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|33ba02be-05cd-4699-b629-ee607bb362fa", "Direction", "Direction", "");
			this.zDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 19, true);
			this.zDropEdit4.Name = "zDropEdit4";
			this.zDropEdit4.PreBoundMaxLength = 3;
			this.zDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 17, true);
			this.zDropEdit4.TabIndex = 0;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "E3_JobType");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 19, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "E3_GE");
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 66, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 17, true);
			this.zGuidFindBox1.TabIndex = 5;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "E3_IsSystem");
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(644, 0, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 16, true);
			this.IsSystemCheckBox.TabIndex = 6;
			this.IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShipmentTransModeDropEdit
			// 
			this.ShipmentTransModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentTransModeDropEdit, "E3_ShippingTransportMode");
			this.ShipmentTransModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 42, true);
			this.ShipmentTransModeDropEdit.Name = "ShipmentTransModeDropEdit";
			this.ShipmentTransModeDropEdit.PreBoundMaxLength = 3;
			this.ShipmentTransModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 17, true);
			this.ShipmentTransModeDropEdit.TabIndex = 1;
			// 
			// IsHiddenCheckBox
			// 
			this.IsHiddenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsHiddenCheckBox, "E3_IsHidden");
			this.IsHiddenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsHiddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(738, 0, true);
			this.IsHiddenCheckBox.Name = "IsHiddenCheckBox";
			this.IsHiddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 16, true);
			this.IsHiddenCheckBox.TabIndex = 7;
			this.IsHiddenCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrgTypeGroupBox
			// 
			this.OrgTypeGroupBox.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|aa3f1022-4e11-4e6e-a664-e588b3662431", "Organizations");
			this.OrgTypeGroupBox.Controls.Add(this.JobOrgTypeGrid);
			this.OrgTypeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgTypeGroupBox.Name = "OrgTypeGroupBox";
			this.OrgTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 122, true);
			this.OrgTypeGroupBox.TabIndex = 0;
			this.OrgTypeGroupBox.TabStop = false;
			// 
			// JobOrgTypeGrid
			// 
			this.JobOrgTypeGrid.AllowNavigation = false;
			this.JobOrgTypeGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.JobOrgTypeGrid, "CommonCartageOrganisations");
			this.JobOrgTypeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "E5_OrgType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|6ee75b63-e638-499f-97ef-f8eea7b9b287", "Org. Type Description");
			zTextBoxColumnStyleInfo1.ColumnName = "OrgTypeFullDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "E5_UsageComment";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "E5_IsBillToParty";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobOrgTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobOrgTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobOrgTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobOrgTypeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.JobOrgTypeGrid.CopySelectedRowsAllowed = true;
			this.JobOrgTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobOrgTypeGrid.GridId = "15a94098-f5a7-4f04-a49d-77e849304bf3";
			this.JobOrgTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobOrgTypeGrid.LayoutKey = "JobOrgTypeGrid";
			this.JobOrgTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.JobOrgTypeGrid.Name = "JobOrgTypeGrid";
			this.JobOrgTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 105, true);
			this.JobOrgTypeGrid.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.splitContainer2);
			this.zPanel1.Controls.Add(this.JobTypeGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 492, true);
			this.zPanel1.TabIndex = 0;
			// 
			// ContainerLooseBookingSplitContainer
			// 
			this.ContainerLooseBookingSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerLooseBookingSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerLooseBookingSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 180, true);
			this.ContainerLooseBookingSplitContainer.Name = "ContainerLooseBookingSplitContainer";
			// 
			// ContainerLooseBookingSplitContainer.Panel1
			// 
			this.ContainerLooseBookingSplitContainer.Panel1.Controls.Add(this.zGroupBox2);
			this.ContainerLooseBookingSplitContainer.Panel1MinSize = 354;
			// 
			// ContainerLooseBookingSplitContainer.Panel2
			// 
			this.ContainerLooseBookingSplitContainer.Panel2.Controls.Add(this.zGroupBox3);
			this.ContainerLooseBookingSplitContainer.Panel2MinSize = 354;
			this.ContainerLooseBookingSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 258, true);
			this.ContainerLooseBookingSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(405);
			this.ContainerLooseBookingSplitContainer.TabIndex = 9;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|0a1a0f41-52d4-4216-89d9-3576ccfbef94", "Container Booking");
			this.zGroupBox2.Controls.Add(this.CartageLegsGroupBox);
			this.zGroupBox2.Controls.Add(this.zDropEdit5);
			this.zGroupBox2.Controls.Add(this.zDropEdit2);
			this.zGroupBox2.Controls.Add(this.zDropEdit1);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 258, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// CartageLegsGroupBox
			// 
			this.CartageLegsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CartageLegsGroupBox.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|25f20c9c-c1be-4afb-8aaa-fbfebe3b0c15", "Port Transport Legs");
			this.CartageLegsGroupBox.Controls.Add(this.ContainerLegsGrid);
			this.CartageLegsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 88, true);
			this.CartageLegsGroupBox.Name = "CartageLegsGroupBox";
			this.CartageLegsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 167, true);
			this.CartageLegsGroupBox.TabIndex = 4;
			this.CartageLegsGroupBox.TabStop = false;
			// 
			// ContainerLegsGrid
			// 
			this.ContainerLegsGrid.AllowNavigation = false;
			this.ContainerLegsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.ContainerLegsGrid, "ContainerizedCartageLegTypes");
			this.ContainerLegsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo1.ColumnName = "E4_DisplayOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|cbe20bfc-699b-4bc4-8691-01bebb470a21", "From");
			zGuidDropEditColumnStyleInfo1.ColumnName = "E4_E5_FromOrg";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|e488e495-06f1-4073-a961-0c7dcbb931a1", "From Org. Description");
			zTextBoxColumnStyleInfo3.ColumnName = "FromOrgDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|dfa8cc97-1abe-4a72-a44a-1c5072ac765a", "Wait");
			zGuidDropEditColumnStyleInfo2.ColumnName = "E4_E5_WaitPointOrg";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|9ca4a2e3-5f0e-443c-8fcb-ad2d2a10eed9", "Wait Org. Description");
			zTextBoxColumnStyleInfo4.ColumnName = "WaitPointOrgDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|1094c14d-8eba-475f-92d5-1e6045d1f407", "To");
			zGuidDropEditColumnStyleInfo3.ColumnName = "E4_E5_ToOrg";
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|8cc71efe-afe2-4754-b48f-d947480719ca", "To Org. Description");
			zTextBoxColumnStyleInfo5.ColumnName = "ToOrgDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "E4_EquipmentGroup";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.ContainerLegsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainerLegsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ContainerLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainerLegsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.ContainerLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContainerLegsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.ContainerLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ContainerLegsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ContainerLegsGrid.CopySelectedRowsAllowed = true;
			this.ContainerLegsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerLegsGrid.GridId = "45159bd6-c0d0-4a5b-86f2-146b2e9e2133";
			this.ContainerLegsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainerLegsGrid.LayoutKey = "CartageLegsGrid";
			this.ContainerLegsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ContainerLegsGrid.Name = "ContainerLegsGrid";
			this.ContainerLegsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 150, true);
			this.ContainerLegsGrid.TabIndex = 0;
			// 
			// zDropEdit5
			// 
			this.zDropEdit5.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit5, "ContainerizedBooking+E4_EquipmentGroup");
			this.zDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 16, true);
			this.zDropEdit5.Name = "zDropEdit5";
			this.zDropEdit5.PreBoundMaxLength = 3;
			this.zDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.zDropEdit5.TabIndex = 0;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "ContainerizedBooking+E4_E5_WaitPointOrg");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 62, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.zDropEdit2.TabIndex = 2;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "ContainerizedBooking+E4_E5_FromOrg");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 39, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|95becfda-1e50-4e5c-8c0c-bf12850ac707", "Loose Booking");
			this.zGroupBox3.Controls.Add(this.zGroupBox1);
			this.zGroupBox3.Controls.Add(this.zDropEdit6);
			this.zGroupBox3.Controls.Add(this.zDropEdit9);
			this.zGroupBox3.Controls.Add(this.zDropEdit10);
			this.zGroupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 258, true);
			this.zGroupBox3.TabIndex = 0;
			this.zGroupBox3.TabStop = false;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|51c4d3cd-19c7-413f-bae6-427ee9e681da", "Port Transport Legs");
			this.zGroupBox1.Controls.Add(this.LooseLegsGrid);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 88, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 167, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// LooseLegsGrid
			// 
			this.LooseLegsGrid.AllowNavigation = false;
			this.LooseLegsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.LooseLegsGrid, "LooseCartageLegTypes");
			this.LooseLegsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo2.ColumnName = "E4_DisplayOrder";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|cbe20bfc-699b-4bc4-8691-01bebb470a21", "From");
			zGuidDropEditColumnStyleInfo4.ColumnName = "E4_E5_FromOrg";
			zGuidDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|e488e495-06f1-4073-a961-0c7dcbb931a1", "From Org. Description");
			zTextBoxColumnStyleInfo6.ColumnName = "FromOrgDescription";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|dfa8cc97-1abe-4a72-a44a-1c5072ac765a", "Wait");
			zGuidDropEditColumnStyleInfo5.ColumnName = "E4_E5_WaitPointOrg";
			zGuidDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|9ca4a2e3-5f0e-443c-8fcb-ad2d2a10eed9", "Wait Org. Description");
			zTextBoxColumnStyleInfo7.ColumnName = "WaitPointOrgDescription";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|1094c14d-8eba-475f-92d5-1e6045d1f407", "To");
			zGuidDropEditColumnStyleInfo6.ColumnName = "E4_E5_ToOrg";
			zGuidDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("CartageTypeForm|8cc71efe-afe2-4754-b48f-d947480719ca", "To Org. Description");
			zTextBoxColumnStyleInfo8.ColumnName = "ToOrgDescription";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.ColumnName = "E4_EquipmentGroup";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.LooseLegsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LooseLegsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LooseLegsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo5);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LooseLegsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo6);
			this.LooseLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LooseLegsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LooseLegsGrid.CopySelectedRowsAllowed = true;
			this.LooseLegsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LooseLegsGrid.GridId = "b13494aa-e555-40b8-9eb6-dd1607d5c41e";
			this.LooseLegsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LooseLegsGrid.LayoutKey = "CartageLegsGrid";
			this.LooseLegsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.LooseLegsGrid.Name = "LooseLegsGrid";
			this.LooseLegsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 150, true);
			this.LooseLegsGrid.TabIndex = 0;
			// 
			// zDropEdit6
			// 
			this.zDropEdit6.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit6, "LooseBooking+E4_EquipmentGroup");
			this.zDropEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 16, true);
			this.zDropEdit6.Name = "zDropEdit6";
			this.zDropEdit6.PreBoundMaxLength = 3;
			this.zDropEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.zDropEdit6.TabIndex = 0;
			// 
			// zDropEdit9
			// 
			this.zDropEdit9.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit9, "LooseBooking+E4_E5_WaitPointOrg");
			this.zDropEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 62, true);
			this.zDropEdit9.Name = "zDropEdit9";
			this.zDropEdit9.PreBoundMaxLength = 3;
			this.zDropEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.zDropEdit9.TabIndex = 2;
			// 
			// zDropEdit10
			// 
			this.zDropEdit10.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit10, "LooseBooking+E4_E5_FromOrg");
			this.zDropEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 39, true);
			this.zDropEdit10.Name = "zDropEdit10";
			this.zDropEdit10.PreBoundMaxLength = 3;
			this.zDropEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.zDropEdit10.TabIndex = 1;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 108, true);
			this.splitContainer2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 368, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.OrgTypeGroupBox);
			this.splitContainer2.Panel1MinSize = 110;
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.ContainerLooseBookingSplitContainer);
			this.splitContainer2.Panel2MinSize = 200;
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 383, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(122);
			this.splitContainer2.TabIndex = 10;
			// 
			// DescriptionTranslatableTextControl
			// 
			this.DescriptionTranslatableTextControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DescriptionTranslatableTextControl, "E3_Description");
			this.DescriptionTranslatableTextControl.CaptionResourceString = Enterprise.Freight.Common.GUI.Res.GetData("ac7ecb24-5591-4735-bb2e-64429c6be5cc", "Job Type Description");
			this.DescriptionTranslatableTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTranslatableTextControl.GridCurrent = null;
			this.DescriptionTranslatableTextControl.GridMember = null;
			this.DescriptionTranslatableTextControl.IsMultiLine = false;
			this.DescriptionTranslatableTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 42, true);
			this.DescriptionTranslatableTextControl.Name = "DescriptionTranslatableTextControl";
			this.DescriptionTranslatableTextControl.ReadOnly = false;
			this.DescriptionTranslatableTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 20, true);
			this.DescriptionTranslatableTextControl.TabIndex = 4;
			this.MainTabPage.PerformLayout();
			this.JobTypeGroupBox.ResumeLayout(false);
			this.JobTypeGroupBox.PerformLayout();
			this.zDropEdit7.ResumeLayout(true);
			this.zDropEdit7.PerformLayout();
			this.zDropEdit4.ResumeLayout(true);
			this.zDropEdit4.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.ShipmentTransModeDropEdit.ResumeLayout(true);
			this.ShipmentTransModeDropEdit.PerformLayout();
			this.OrgTypeGroupBox.ResumeLayout(false);
			this.OrgTypeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobOrgTypeGrid)).EndInit();
			this.JobOrgTypeGrid.ResumeLayout(false);
			this.JobOrgTypeGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ContainerLooseBookingSplitContainer.Panel1.ResumeLayout(false);
			this.ContainerLooseBookingSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainerLooseBookingSplitContainer)).EndInit();
			this.ContainerLooseBookingSplitContainer.ResumeLayout(false);
			this.ContainerLooseBookingSplitContainer.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.CartageLegsGroupBox.ResumeLayout(false);
			this.CartageLegsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainerLegsGrid)).EndInit();
			this.ContainerLegsGrid.ResumeLayout(false);
			this.ContainerLegsGrid.PerformLayout();
			this.zDropEdit5.ResumeLayout(true);
			this.zDropEdit5.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LooseLegsGrid)).EndInit();
			this.LooseLegsGrid.ResumeLayout(false);
			this.LooseLegsGrid.PerformLayout();
			this.zDropEdit6.ResumeLayout(true);
			this.zDropEdit6.PerformLayout();
			this.zDropEdit9.ResumeLayout(true);
			this.zDropEdit9.PerformLayout();
			this.zDropEdit10.ResumeLayout(true);
			this.zDropEdit10.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.DescriptionTranslatableTextControl.ResumeLayout(true);
			this.DescriptionTranslatableTextControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}
	}
}
