namespace Enterprise.Workflow.GUI
{
	partial class ContainmentBarrierInvocationForm
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
		protected new void InitializeComponent()
		{
			this.ContainmentBarrierInvocationControl = new Enterprise.Workflow.GUI.ContainmentBarrierInvocationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.ContainmentBarrierViewModel);
			// 
			// ContainmentBarrierInvocationControl
			// 
			this.ContainmentBarrierInvocationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainmentBarrierInvocationControl, ".");
			this.ContainmentBarrierInvocationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainmentBarrierInvocationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainmentBarrierInvocationControl.Name = "ContainmentBarrierInvocationControl";
			this.ContainmentBarrierInvocationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 162, true);
			this.ContainmentBarrierInvocationControl.TabIndex = 1;
			// 
			// ContainmentBarrierInvocationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("2b7e988b-8d25-4ee1-be68-3273bb9994f5", "Containment Barrier Outcome");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 191, true);
			this.Controls.Add(this.ContainmentBarrierInvocationControl);
			this.DataSourceType = typeof(Enterprise.Workflow.Business.ContainmentBarrierViewModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 230, true);
			this.Name = "ContainmentBarrierInvocationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ContainmentBarrierInvocationControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ContainmentBarrierInvocationUserControl ContainmentBarrierInvocationControl;

	}
}
