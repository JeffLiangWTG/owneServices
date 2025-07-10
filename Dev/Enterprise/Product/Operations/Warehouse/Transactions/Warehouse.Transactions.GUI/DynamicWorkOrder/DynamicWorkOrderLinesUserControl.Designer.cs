using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class DynamicWorkOrderLinesUserControl
	{
		#region Auto

		DynamicWorkOrderParentLinesGridUserControl ParentLinesGridControl;
		DynamicWorkOrderComponentLinesGridUserControl ComponentLinesGridControl;
		ZGroupBox ParentLinesGroupBox;
		ZGroupBox ComponentLinesGroupBox;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;

		void InitializeComponent()
		{
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.ParentLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentLinesGridControl = new Enterprise.Warehouse.Transactions.GUI.DynamicWorkOrderParentLinesGridUserControl();
			this.ComponentLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ComponentLinesGridControl = new Enterprise.Warehouse.Transactions.GUI.DynamicWorkOrderComponentLinesGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.ParentLinesGroupBox.SuspendLayout();
			this.ParentLinesGridControl.SuspendLayout();
			this.ComponentLinesGroupBox.SuspendLayout();
			this.ComponentLinesGridControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ParentLinesGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 426, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ComponentLinesGroupBox);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.splitContainer1.TabIndex = 25;
			// 
			// ParentLinesGroupBox
			// 
			this.ParentLinesGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderLinesUserControl|22f27cc5-4271-4aab-9ba7-b69f85890f55", "Parent Lines");
			this.ParentLinesGroupBox.Controls.Add(this.ParentLinesGridControl);
			this.ParentLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentLinesGroupBox.Name = "ParentLinesGroupBox";
			this.ParentLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 10, true);
			this.ParentLinesGroupBox.TabIndex = 2;
			this.ParentLinesGroupBox.TabStop = false;
			// 
			// ParentLinesGridControl
			// 
			this.ParentLinesGridControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentLinesGridControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ParentLinesGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentLinesGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.ParentLinesGridControl.Name = "ParentLinesGridControl";
			this.ParentLinesGridControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.ParentLinesGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 150, true);
			this.ParentLinesGridControl.TabIndex = 0;
			// 
			// ComponentLinesGroupBox
			// 
			this.ComponentLinesGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderLinesUserControl|6ecf056a-e821-4bd8-8d10-1ec875ad56ce", "Component Lines");
			this.ComponentLinesGroupBox.Controls.Add(this.ComponentLinesGridControl);
			this.ComponentLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentLinesGroupBox.Name = "ComponentLinesGroupBox";
			this.ComponentLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 150, true);
			this.ComponentLinesGroupBox.TabIndex = 3;
			this.ComponentLinesGroupBox.TabStop = false;
			// 
			// ComponentLinesGridControl
			// 
			this.ComponentLinesGridControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComponentLinesGridControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ComponentLinesGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentLinesGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.ComponentLinesGridControl.Name = "ComponentLinesGridControl";
			this.ComponentLinesGridControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.ComponentLinesGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 150, true);
			this.ComponentLinesGridControl.TabIndex = 0;
			// 
			// DynamicWorkOrderLinesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "DynamicWorkOrderLinesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 350, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ParentLinesGroupBox.ResumeLayout(false);
			this.ParentLinesGroupBox.PerformLayout();
			this.ParentLinesGridControl.ResumeLayout(true);
			this.ParentLinesGridControl.PerformLayout();
			this.ComponentLinesGroupBox.ResumeLayout(false);
			this.ComponentLinesGroupBox.PerformLayout();
			this.ComponentLinesGridControl.ResumeLayout(true);
			this.ComponentLinesGridControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
