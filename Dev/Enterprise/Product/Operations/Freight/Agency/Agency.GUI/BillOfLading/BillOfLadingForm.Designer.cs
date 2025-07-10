namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingForm
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.containersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.containersControl = new Enterprise.Freight.Agency.GUI.BillOfLadingContainersPage();
			this.vehiclesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.VehiclesControl = new Enterprise.Freight.Agency.GUI.BillOfLadingVehiclesPage();
			this.packsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PacksControl = new Enterprise.Freight.Agency.GUI.BillOfLadingTopLevelPacksPage();
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.bookedContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.billOfLadingMainPage1 = new Enterprise.Freight.Agency.GUI.BillOfLadingMainPage();
			bookedContainersControl = new Enterprise.Freight.Agency.GUI.ContainersUserControl();
			additionalDetails = new Enterprise.ZArchitecture.GUI.ZTabPage();
			additionalDetailsControl = new Enterprise.Freight.Agency.GUI.BillOfLadingAdditionalDetailsPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			additionalDetails.SuspendLayout();
			this.containersTabPage.SuspendLayout();
			this.vehiclesTabPage.SuspendLayout();
			this.packsTabPage.SuspendLayout();
			this.bookedContainersTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(additionalDetails);
			this.MainTabControl.Controls.Add(this.containersTabPage);
			this.MainTabControl.Controls.Add(this.bookedContainersTabPage);
			this.MainTabControl.Controls.Add(this.vehiclesTabPage);
			this.MainTabControl.Controls.Add(this.packsTabPage);
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 634, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.packsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.vehiclesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.bookedContainersTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.containersTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(additionalDetails, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.billOfLadingMainPage1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			this.MainTabPage.AutoScroll = true;
			this.MainTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 586, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(504);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(505);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLading);
			// 
			// bookedContainersControl
			// 
			bookedContainersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(bookedContainersControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipment)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)))));
			bookedContainersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			bookedContainersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			bookedContainersControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 500, true);
			bookedContainersControl.Name = "bookedContainersControl";
			bookedContainersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 601, true);
			bookedContainersControl.TabIndex = 1;
			// 
			// additionalDetails
			// 
			additionalDetails.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingForm|486debf3-ec0e-4088-911f-1f9cca61332e", "Additional Details");
			additionalDetails.Controls.Add(additionalDetailsControl);
			additionalDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			additionalDetails.Name = "additionalDetails";
			additionalDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			additionalDetails.TabIndex = 8;
			// 
			// additionalDetailsControl
			// 
			additionalDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(additionalDetailsControl, ".");
			additionalDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			additionalDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			additionalDetailsControl.Name = "additionalDetailsControl";
			additionalDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			additionalDetailsControl.TabIndex = 0;
			// 
			// containersTabPage
			// 
			this.containersTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingForm|e78a7609-1b98-4a4b-b076-04f00a629751", "Containers");
			this.containersTabPage.Controls.Add(this.containersControl);
			this.containersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.containersTabPage.Name = "containersTabPage";
			this.containersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 514, true);
			this.containersTabPage.TabIndex = 3;
			this.containersTabPage.AutoScroll = true;
			this.containersTabPage.AutoScrollMinSize= CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 586, true);
			// 
			// containersControl
			// 
			this.containersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.containersControl, ".");
			this.containersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containersControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 587, true);
			this.containersControl.Name = "containersControl";
			this.containersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			this.containersControl.TabIndex = 0;
			// 
			// vehiclesTabPage
			// 
			this.vehiclesTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingForm|168d8ea1-5be2-4a97-a648-9c362ab5842a", "Vehicles");
			this.vehiclesTabPage.Controls.Add(this.VehiclesControl);
			this.vehiclesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.vehiclesTabPage.Name = "vehiclesTabPage";
			this.vehiclesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 514, true);
			this.vehiclesTabPage.TabIndex = 4;
			// 
			// VehiclesControl
			// 
			this.VehiclesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehiclesControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipment)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)))));
			this.VehiclesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehiclesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VehiclesControl.Name = "VehiclesControl";
			this.VehiclesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			this.VehiclesControl.TabIndex = 0;
			// 
			// packsTabPage
			// 
			this.packsTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingForm|d8a3f148-8a4a-4002-91f6-3c1eb80739b3", "Packs");
			this.packsTabPage.Controls.Add(this.PacksControl);
			this.packsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.packsTabPage.Name = "packsTabPage";
			this.packsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			this.packsTabPage.TabIndex = 5;
			// 
			// PacksControl
			// 
			this.PacksControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PacksControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipment)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)))));
			this.PacksControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PacksControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PacksControl.Name = "PacksControl";
			this.PacksControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			this.PacksControl.TabIndex = 0;
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 514, true);
			this.workflowTabPage.TabIndex = 6;
			// 
			// bookedContainersTabPage
			// 
			this.bookedContainersTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingForm|11d60c0c-bca6-4fc6-8991-8b17616d29a2", "Booked Containers");
			this.bookedContainersTabPage.Controls.Add(bookedContainersControl);
			this.bookedContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.bookedContainersTabPage.Name = "bookedContainersTabPage";
			this.bookedContainersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.bookedContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 514, true);
			this.bookedContainersTabPage.TabIndex = 7;
			this.bookedContainersTabPage.UseVisualStyleBackColor = true;
			// 
			// billOfLadingMainPage1
			// 
			this.billOfLadingMainPage1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.billOfLadingMainPage1, ".");
			this.billOfLadingMainPage1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billOfLadingMainPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.billOfLadingMainPage1.Name = "billOfLadingMainPage1";
			this.billOfLadingMainPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 607, true);
			this.billOfLadingMainPage1.TabIndex = 0;
			// 
			// BillOfLadingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 690, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.Agency.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLading);
			this.DataSourceTypeName = "Enterprise.Freight.Agency.Business.BillOfLading";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1022, 717, true);
			this.Name = "BillOfLadingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Bill of Lading";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			additionalDetails.ResumeLayout(false);
			this.containersTabPage.ResumeLayout(false);
			this.vehiclesTabPage.ResumeLayout(false);
			this.packsTabPage.ResumeLayout(false);
			this.bookedContainersTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		System.ComponentModel.Container components = null;
		Enterprise.ZArchitecture.GUI.ZTabPage containersTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage vehiclesTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage packsTabPage;
		Enterprise.MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage bookedContainersTabPage;
		BillOfLadingContainersPage containersControl;
		BillOfLadingVehiclesPage VehiclesControl;
		BillOfLadingTopLevelPacksPage PacksControl;
		BillOfLadingMainPage billOfLadingMainPage1;
		Enterprise.Freight.Agency.GUI.ContainersUserControl bookedContainersControl;
		Enterprise.ZArchitecture.GUI.ZTabPage additionalDetails;
		Enterprise.Freight.Agency.GUI.BillOfLadingAdditionalDetailsPage additionalDetailsControl;
	}
}
