namespace Enterprise.Freight.Agency.GUI
{
	public partial class InnerVoyageAllocationControl
	{
		private void InitializeComponent()
		{
			this.countryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.originGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.sailingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.allocationMethodNotSetLabel = new Enterprise.ZArchitecture.ZLabel();
			this.allocationMethodIgnore = new Enterprise.ZArchitecture.ZLabel();
			this.sailingUsageSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			sailingLevelAllocations = new Enterprise.Freight.Agency.GUI.SailingLevelAllocations();
			countryLevelAllocations = new Enterprise.Freight.Agency.GUI.CountryLevelAllocations();
			originLevelAllocations = new Enterprise.Freight.Agency.GUI.OriginLevelAllocations();
			usageBreakDownBySailing = new Enterprise.Freight.Agency.GUI.UsageBreakDownBySailing();
			refreshPanel = new CargoWise.Windows.UI.KPanel();
			usageBreakDownBySailingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.countryGroupBox.SuspendLayout();
			this.originGroupBox.SuspendLayout();
			this.sailingsGroupBox.SuspendLayout();
			refreshPanel.SuspendLayout();
			usageBreakDownBySailingGroupBox.SuspendLayout();
			this.sailingUsageSplitPanel.Panel1.SuspendLayout();
			this.sailingUsageSplitPanel.Panel2.SuspendLayout();
			this.sailingUsageSplitPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyPrincipal);
			// 
			// refreshButton
			// 
			refreshButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("InnerVoyageAllocationControl|bad11adc-39c6-4767-8ac5-a27137aef568", "Refresh");
			refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			refreshButton.Name = "refreshButton";
			refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			refreshButton.TabIndex = 0;
			refreshButton.UseVisualStyleBackColor = true;
			refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// sailingLevelAllocations
			// 
			this.BindingSource.SetBindingMember(sailingLevelAllocations, ".");
			sailingLevelAllocations.Dock = System.Windows.Forms.DockStyle.Fill;
			sailingLevelAllocations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			sailingLevelAllocations.Name = "sailingLevelAllocations";
			sailingLevelAllocations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 476, true);
			sailingLevelAllocations.TabIndex = 0;
			// 
			// countryLevelAllocations
			// 
			this.BindingSource.SetBindingMember(countryLevelAllocations, ".");
			countryLevelAllocations.Dock = System.Windows.Forms.DockStyle.Fill;
			countryLevelAllocations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			countryLevelAllocations.Name = "countryLevelAllocations";
			countryLevelAllocations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 476, true);
			countryLevelAllocations.TabIndex = 0;
			// 
			// originLevelAllocations
			// 
			originLevelAllocations.AutoScroll = true;
			originLevelAllocations.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 0, true);
			this.BindingSource.SetBindingMember(originLevelAllocations, ".");
			originLevelAllocations.Dock = System.Windows.Forms.DockStyle.Fill;
			originLevelAllocations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			originLevelAllocations.Name = "originLevelAllocations";
			originLevelAllocations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 476, true);
			originLevelAllocations.TabIndex = 0;
			// 
			// usageBreakDownBySailing
			// 
			this.BindingSource.SetBindingMember(usageBreakDownBySailing, ".");
			usageBreakDownBySailing.Dock = System.Windows.Forms.DockStyle.Fill;
			usageBreakDownBySailing.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			usageBreakDownBySailing.Name = "usageBreakDownBySailing";
			usageBreakDownBySailing.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 119, true);
			usageBreakDownBySailing.TabIndex = 0;
			// 
			// countryGroupBox
			// 
			this.countryGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("InnerVoyageAllocationControl|38b28f4c-5e8c-4f91-9f23-8f86619908f0", "Allocations by Country/Region");
			this.countryGroupBox.Controls.Add(countryLevelAllocations);
			this.countryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.countryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.countryGroupBox.Name = "countryGroupBox";
			this.countryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 495, true);
			this.countryGroupBox.TabIndex = 1;
			this.countryGroupBox.TabStop = false;
			// 
			// originGroupBox
			// 
			this.originGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("InnerVoyageAllocationControl|a8e5c49c-5851-495f-9d26-1a7c5fd8f302", "Allocations by Origin");
			this.originGroupBox.Controls.Add(originLevelAllocations);
			this.originGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.originGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.originGroupBox.Name = "originGroupBox";
			this.originGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 495, true);
			this.originGroupBox.TabIndex = 2;
			this.originGroupBox.TabStop = false;
			// 
			// sailingsGroupBox
			// 
			this.sailingsGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("InnerVoyageAllocationControl|9819ff1f-08ee-47df-a9b9-d7c283cc9c1c", "Allocations by Sailing");
			this.sailingsGroupBox.Controls.Add(sailingLevelAllocations);
			this.sailingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sailingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.sailingsGroupBox.Name = "sailingsGroupBox";
			this.sailingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 495, true);
			this.sailingsGroupBox.TabIndex = 3;
			this.sailingsGroupBox.TabStop = false;
			// 
			// refreshPanel
			// 
			refreshPanel.Controls.Add(refreshButton);
			refreshPanel.Dock = System.Windows.Forms.DockStyle.Top;
			refreshPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			refreshPanel.Name = "refreshPanel";
			refreshPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1110, 37, true);
			refreshPanel.TabIndex = 0;
			// 
			// allocationMethodNotSetLabel
			// 
			this.allocationMethodNotSetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.allocationMethodNotSetLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.allocationMethodNotSetLabel.Name = "allocationMethodNotSetLabel";
			this.allocationMethodNotSetLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 495, true);
			this.allocationMethodNotSetLabel.TabIndex = 5;
			this.allocationMethodNotSetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// allocationMethodIgnore
			// 
			this.allocationMethodIgnore.Dock = System.Windows.Forms.DockStyle.Fill;
			this.allocationMethodIgnore.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.allocationMethodIgnore.Name = "allocationMethodIgnore";
			this.allocationMethodIgnore.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 495, true);
			this.allocationMethodIgnore.TabIndex = 6;
			this.allocationMethodIgnore.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// usageBreakDownBySailingGroupBox
			// 
			usageBreakDownBySailingGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("InnerVoyageAllocationControl|a1171911-9814-4163-9e8f-ffbb9373398c", "Usage Break-Down by Sailing");
			usageBreakDownBySailingGroupBox.Controls.Add(usageBreakDownBySailing);
			usageBreakDownBySailingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			usageBreakDownBySailingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			usageBreakDownBySailingGroupBox.Name = "usageBreakDownBySailingGroupBox";
			usageBreakDownBySailingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1098, 138, true);
			usageBreakDownBySailingGroupBox.TabIndex = 4;
			usageBreakDownBySailingGroupBox.TabStop = false;
			// 
			// sailingUsageSplitPanel
			// 
			this.sailingUsageSplitPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sailingUsageSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sailingUsageSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.sailingUsageSplitPanel.Name = "sailingUsageSplitPanel";
			this.sailingUsageSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// sailingUsageSplitPanel.Panel1
			// 
			this.sailingUsageSplitPanel.Panel1.Controls.Add(this.allocationMethodNotSetLabel);
			this.sailingUsageSplitPanel.Panel1.Controls.Add(this.allocationMethodIgnore);
			this.sailingUsageSplitPanel.Panel1.Controls.Add(this.sailingsGroupBox);
			this.sailingUsageSplitPanel.Panel1.Controls.Add(this.countryGroupBox);
			this.sailingUsageSplitPanel.Panel1.Controls.Add(this.originGroupBox);
			this.sailingUsageSplitPanel.Panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.sailingUsageSplitPanel.Panel1MinSize = 130;
			// 
			// sailingUsageSplitPanel.Panel2
			// 
			this.sailingUsageSplitPanel.Panel2.Controls.Add(usageBreakDownBySailingGroupBox);
			this.sailingUsageSplitPanel.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.sailingUsageSplitPanel.Panel2MinSize = 80;
			this.sailingUsageSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1110, 661, true);
			this.sailingUsageSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(507);
			this.sailingUsageSplitPanel.TabIndex = 7;
			// 
			// InnerVoyageAllocationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.sailingUsageSplitPanel);
			this.Controls.Add(refreshPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 300, true);
			this.Name = "InnerVoyageAllocationControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1116, 701, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.countryGroupBox.ResumeLayout(false);
			this.originGroupBox.ResumeLayout(false);
			this.sailingsGroupBox.ResumeLayout(false);
			refreshPanel.ResumeLayout(false);
			usageBreakDownBySailingGroupBox.ResumeLayout(false);
			this.sailingUsageSplitPanel.Panel1.ResumeLayout(false);
			this.sailingUsageSplitPanel.Panel2.ResumeLayout(false);
			this.sailingUsageSplitPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox countryGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox originGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox sailingsGroupBox;
		Enterprise.ZArchitecture.ZLabel allocationMethodNotSetLabel;
		Enterprise.ZArchitecture.ZLabel allocationMethodIgnore;
		CargoWise.Windows.UI.KSplitContainer sailingUsageSplitPanel;
		Enterprise.ZArchitecture.GUI.ZButton refreshButton;
		Enterprise.Freight.Agency.GUI.SailingLevelAllocations sailingLevelAllocations;
		Enterprise.Freight.Agency.GUI.CountryLevelAllocations countryLevelAllocations;
		Enterprise.Freight.Agency.GUI.OriginLevelAllocations originLevelAllocations;
		Enterprise.Freight.Agency.GUI.UsageBreakDownBySailing usageBreakDownBySailing;
		CargoWise.Windows.UI.KPanel refreshPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox usageBreakDownBySailingGroupBox;
	}
}
