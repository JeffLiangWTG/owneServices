using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class VASOrderEntryForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ServicesTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ServicesTab);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 398, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ServicesTab, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 371, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 371, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 371, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 398, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsVASOrder);
			// 
			// ServicesTab
			// 
			this.ServicesTab.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|ddcf5509-94ee-4afe-9f9e-b20034c918a7", "Services");
			this.ServicesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServicesTab.Name = "ServicesTab";
			this.ServicesTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServicesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 371, true);
			this.ServicesTab.TabIndex = 6;
			this.ServicesTab.UseVisualStyleBackColor = true;
			this.ServicesTab.RunWhenBindingOrFirstShown(new System.EventHandler(this.ServicesTab_InitializeTab));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 371, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).WVO_OH_Client)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).WarehousePK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).WVO_WA_ServiceArea)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).WVO_CustomerReferenceNo)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Status)));
			// 
			// VASOrderEntryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 454, true);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsVASOrder);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsVASOrder";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 393, true);
			this.Name = "VASOrderEntryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "VASOrderEntryForm";
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

		#endregion

		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox ServiceAreaFindBox;
		private ZArchitecture.GUI.ZGuidFindBox WarehouseFindBox;
		private MasterFiles.GUI.ZOrganisationFindBox ClientFindBox;
		private VASOrderLineGridUserControl VASOrderLineGridUserControl;
		private ZArchitecture.GUI.ZPanel VASOrderButtonPanel;
		private ZArchitecture.ZTextBox CustomerReferenceTextBox;
		private ZArchitecture.ZTextBox StatusTextBox;
		Enterprise.MasterFiles.GUI.ServicesControl ServicesControl;
		ZArchitecture.GUI.ZTabPage ServicesTab;
		ZArchitecture.GUI.ZPanel ServicesPanel;
		internal ZArchitecture.GUI.ZButton FinaliseButton;
		internal ZArchitecture.GUI.ZButton MarkVASOrderCompletedButton;
		internal ZArchitecture.GUI.ZButton CreateInitialTransferButton;
	}
}
