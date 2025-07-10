namespace Enterprise.MasterFiles.GUI
{
	partial class AddressUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.GroupBox_Addresses = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LeftSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RightFlowPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.GroupBox_ProcessedAddresses = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox_Addresses.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LeftSplitContainer)).BeginInit();
			this.LeftSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AdministrationPanelManager);
			// 
			// GroupBox_Addresses
			// 
			this.GroupBox_Addresses.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7611135-e150-43cb-977a-9dad57684153", "Addresses");
			this.GroupBox_Addresses.Controls.Add(this.MainSplitContainer);
			this.GroupBox_Addresses.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBox_Addresses.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox_Addresses.Name = "GroupBox_Addresses";
			this.GroupBox_Addresses.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 391, true);
			this.GroupBox_Addresses.TabIndex = 0;
			this.GroupBox_Addresses.TabStop = false;
			// 
			// SplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.LeftSplitContainer);
			// 
			// SplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.RightFlowPanel);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 372, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(684);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// SplitContainer2
			// 
			this.LeftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftSplitContainer.Name = "LeftSplitContainer";
			this.LeftSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.LeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 372, true);
			this.LeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(184);
			this.LeftSplitContainer.TabIndex = 1;
			// 
			// RightFlowPanel
			// 
			this.RightFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightFlowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightFlowPanel.Name = "RightFlowPanel";
			this.RightFlowPanel.AutoScroll = true;
			this.RightFlowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 372, true);
			this.RightFlowPanel.TabIndex = 0;
			// 
			// GroupBox_ProcessedAddresses
			// 
			this.GroupBox_ProcessedAddresses.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBox_ProcessedAddresses.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GroupBox_ProcessedAddresses.Name = "GroupBox_ProcessedAddresses";
			this.GroupBox_ProcessedAddresses.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.GroupBox_ProcessedAddresses.TabIndex = 0;
			this.GroupBox_ProcessedAddresses.TabStop = false;
			// 
			// AddressUserControl
			// 
			this.Controls.Add(this.GroupBox_Addresses);
			this.Name = "AddressUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 391, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox_Addresses.ResumeLayout(false);
			this.GroupBox_Addresses.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LeftSplitContainer)).EndInit();
			this.LeftSplitContainer.ResumeLayout(false);
			this.LeftSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox GroupBox_Addresses;
		private ZArchitecture.GUI.ZGroupBox GroupBox_ProcessedAddresses;
		private CargoWise.Windows.UI.KSplitContainer LeftSplitContainer;
		private CargoWise.Windows.UI.KFlowLayoutPanel RightFlowPanel;
		internal Enterprise.ZArchitecture.ZGrid ProcessedGrid;
	}
}
