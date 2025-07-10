namespace Enterprise.Freight.Agency.GUI
{
	public partial class AgencyBookingForm
	{
		new void InitializeComponent()
		{
			this.actualContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BookingDetails = new Enterprise.Freight.Agency.GUI.BookingDetailsControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			loadedContainersControl = new Enterprise.Freight.Agency.GUI.LoadedContainersUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.actualContainersTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.actualContainersTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 630, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.actualContainersTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.BookingDetails);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 630, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1024);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyBooking);
			// 
			// loadedContainersControl
			// 
			this.BindingSource.SetBindingMember(loadedContainersControl, "FCLContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Agency.Business.AgencyShipmentContainerDependentCollection)(((Enterprise.Freight.Agency.Business.AgencyBooking)(null)).RealContainers)));
			loadedContainersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			loadedContainersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			loadedContainersControl.Name = "loadedContainersControl";
			loadedContainersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 584, true);
			loadedContainersControl.TabIndex = 0;
			// 
			// actualContainersTabPage
			// 
			this.actualContainersTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("AgencyBookingForm|15d08b3e-9dcb-4748-8352-81cf17e35060", "Actual Containers");
			this.actualContainersTabPage.Controls.Add(loadedContainersControl);
			this.actualContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.actualContainersTabPage.Name = "actualContainersTabPage";
			this.actualContainersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.actualContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 590, true);
			this.actualContainersTabPage.TabIndex = 2;
			this.actualContainersTabPage.UseVisualStyleBackColor = true;
			// 
			// BookingDetails
			// 
			this.BindingSource.SetBindingMember(this.BookingDetails, ".");
			this.BookingDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BookingDetails.Name = "BookingDetails";
			this.BookingDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 630, true);
			this.BookingDetails.TabIndex = 0;
			this.BookingDetails.Confirm += new System.EventHandler(this.BookingDetails_Confirm);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 471, true);
			this.WorkflowTabPage.TabIndex = 1;
			// 
			// AgencyBookingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 720, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.Agency.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyBooking);
			this.DataSourceTypeName = "Enterprise.Freight.Agency.Business.AgencyBooking";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "AgencyBookingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Edit Booking";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.actualContainersTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.Freight.Agency.GUI.BookingDetailsControl BookingDetails;
		Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage actualContainersTabPage;
		Enterprise.Freight.Agency.GUI.LoadedContainersUserControl loadedContainersControl;
	}
}

