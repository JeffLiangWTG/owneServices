namespace Enterprise.MasterFiles.GUI
{
	partial class SalesRelationPopupForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.SalesRelationControl = new Enterprise.MasterFiles.GUI.SalesRelationControl();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.salesRelationControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.salesRelationControlPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 417, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IRelatableActivity);
			// 
			// SalesRelationControl
			// 
			this.SalesRelationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalesRelationControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.SalesRelationModel)(((Enterprise.MasterFiles.Business.IRelatableActivity)(null)))));
			this.SalesRelationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SalesRelationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SalesRelationControl.Name = "SalesRelationControl";
			this.SalesRelationControl.ShowPopupButton = false;
			this.SalesRelationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 387, true);
			this.SalesRelationControl.TabIndex = 0;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.postingButtonsUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 389, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 28, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// salesRelationControlPanel
			// 
			this.salesRelationControlPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.salesRelationControlPanel.Controls.Add(this.SalesRelationControl);
			this.salesRelationControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesRelationControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.salesRelationControlPanel.Name = "salesRelationControlPanel";
			this.salesRelationControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 389, true);
			this.salesRelationControlPanel.TabIndex = 2;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 1, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// SalesRelationPopupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3bd03491-68ee-47ea-9c41-225f214c4855", "Sales Relations");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 441, true);
			this.Controls.Add(this.salesRelationControlPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.IRelatableActivity);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 450, true);
			this.Name = "SalesRelationPopupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.salesRelationControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.salesRelationControlPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal SalesRelationControl SalesRelationControl;
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZPanel salesRelationControlPanel;
		private Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
	}
}