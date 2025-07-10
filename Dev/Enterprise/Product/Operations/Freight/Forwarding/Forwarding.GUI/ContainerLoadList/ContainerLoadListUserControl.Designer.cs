
namespace Enterprise.Freight.Forwarding.GUI
{
	partial class ContainerLoadListUserControl
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
			this.LoadListContainerDetailLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.LoadListIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsTopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.CYContainerLoadList);
			// 
			// detailsTopPanel
			// 
			this.detailsTopPanel.Controls.Add(this.LoadListContainerDetailLinkLabel);
			this.detailsTopPanel.Controls.Add(this.LoadListIdTextBox);
			this.detailsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsTopPanel.Name = "detailsTopPanel";
			this.detailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1104, 149, true);
			this.detailsTopPanel.TabIndex = 5;
			// 
			// LoadListContainerDetailLinkLabel
			// 
			this.LoadListContainerDetailLinkLabel.AutoSize = true;
			this.LoadListContainerDetailLinkLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3cc10489-3eb5-4a11-801b-168b7b2220ae", "Click here to show Container Load List record");
			this.LoadListContainerDetailLinkLabel.IsFontBold = false;
			this.LoadListContainerDetailLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 72, true);
			this.LoadListContainerDetailLinkLabel.Name = "LoadListContainerDetailLinkLabel";
			this.LoadListContainerDetailLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 13, true);
			this.LoadListContainerDetailLinkLabel.TabIndex = 2;
			// 
			// LoadListIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadListIdTextBox, "CLH_LoadListId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.CYContainerLoadList)(null)).CLH_LoadListId)));
			this.LoadListIdTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("41869ABC-ECDE-4A49-93DA-A59E78AD00BD", "Load List ID");
			this.LoadListIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 31, true);
			this.LoadListIdTextBox.Name = "LoadListIdTextBox";
			this.LoadListIdTextBox.ReadOnly = true;
			this.LoadListIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.LoadListIdTextBox.TabIndex = 1;
			// 
			// ContainerLoadListUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.detailsTopPanel);
			this.Name = "ContainerLoadListUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1104, 744, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsTopPanel.ResumeLayout(false);
			this.detailsTopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.GUI.ZPanel detailsTopPanel;
		private ZArchitecture.GUI.ZLinkLabel LoadListContainerDetailLinkLabel;
		private ZArchitecture.ZTextBox LoadListIdTextBox;
	}
}
