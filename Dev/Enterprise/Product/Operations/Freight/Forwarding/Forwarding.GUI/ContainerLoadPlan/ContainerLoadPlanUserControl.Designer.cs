
namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ContainerLoadPlanUserControl
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
			this.detailsTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LoadPlanContainerDetailLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.LoadPlanIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsTopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.CFSContainerLoadList);
			// 
			// detailsTopPanel
			// 
			this.detailsTopPanel.Controls.Add(this.LoadPlanContainerDetailLinkLabel);
			this.detailsTopPanel.Controls.Add(this.LoadPlanIdTextBox);
			this.detailsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsTopPanel.Name = "detailsTopPanel";
			this.detailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1104, 149, true);
			this.detailsTopPanel.TabIndex = 5;
			// 
			// LoadPlanContainerDetailLinkLabel
			// 
			this.LoadPlanContainerDetailLinkLabel.AutoSize = true;
			this.LoadPlanContainerDetailLinkLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("F11C96EF-A1A4-49F9-AE2B-4587912B71EC", "Click here to show Container Load Plan record");
			this.LoadPlanContainerDetailLinkLabel.IsFontBold = false;
			this.LoadPlanContainerDetailLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 72, true);
			this.LoadPlanContainerDetailLinkLabel.Name = "LoadPlanContainerDetailLinkLabel";
			this.LoadPlanContainerDetailLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 13, true);
			this.LoadPlanContainerDetailLinkLabel.TabIndex = 2;
			// 
			// LoadPlanIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadPlanIdTextBox, "CLH_LoadListId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.CFSContainerLoadList)(null)).CLH_LoadListId)));
			this.LoadPlanIdTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("A5429E70-73ED-47A4-8751-850C1CB105A1", "Load Plan ID");
			this.LoadPlanIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 31, true);
			this.LoadPlanIdTextBox.Name = "LoadPlanIdTextBox";
			this.LoadPlanIdTextBox.ReadOnly = true;
			this.LoadPlanIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.LoadPlanIdTextBox.TabIndex = 1;
			// 
			// ContainerLoadPlanUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.detailsTopPanel);
			this.Name = "ContainerLoadPlanUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1104, 744, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsTopPanel.ResumeLayout(false);
			this.detailsTopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.GUI.ZPanel detailsTopPanel;
		private ZArchitecture.GUI.ZLinkLabel LoadPlanContainerDetailLinkLabel;
		private ZArchitecture.ZTextBox LoadPlanIdTextBox;
	}
}
