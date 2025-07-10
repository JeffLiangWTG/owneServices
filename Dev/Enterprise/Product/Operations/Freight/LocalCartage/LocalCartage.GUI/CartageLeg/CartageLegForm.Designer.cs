using Enterprise.ZArchitecture.Core;
using Enterprise.Freight.LocalCartage.Business;

namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageLegForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.CartageLegUserControl = new Enterprise.Freight.LocalCartage.GUI.CartageLegUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 588, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 550, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.CartageLegUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 523, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartageLeg);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 588, true);
			this.WorkflowTabPage.TabIndex = 6;
			// 
			// CartageLegUserControl
			// 
			this.CartageLegUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CartageLegUserControl, ".");
			this.CartageLegUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CartageLegUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CartageLegUserControl.Name = "CartageLegUserControl";
			this.CartageLegUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 523, true);
			this.CartageLegUserControl.TabIndex = 0;
			// 
			// CartageLegForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 606, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.LocalCartage.Business";
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartageLeg);
			this.DataSourceTypeName = "Enterprise.Freight.LocalCartage.Business.CommonCartageLeg";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 665, true);
			this.Name = "CartageLegForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CartageLegForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		internal CartageLegUserControl CartageLegUserControl;
	}
}
