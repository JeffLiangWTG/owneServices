namespace Enterprise.Packing.GUI
{
	partial class PackingPopupDialog
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
			this.PopupSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TreeUserControl = new Enterprise.Packing.GUI.PackingTreeViewUserControl();
			this.CancelAttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PopupSplitContainer.Panel1.SuspendLayout();
			this.PopupSplitContainer.Panel2.SuspendLayout();
			this.PopupSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 216, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 0, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Packing.Business.PkgPackageJob);
			// 
			// PopupSplitContainer
			// 
			this.PopupSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PopupSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.PopupSplitContainer.IsSplitterFixed = true;
			this.PopupSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PopupSplitContainer.Name = "PopupSplitContainer";
			this.PopupSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// PopupSplitContainer.Panel1
			// 
			this.PopupSplitContainer.Panel1.Controls.Add(this.TreeUserControl);
			// 
			// PopupSplitContainer.Panel2
			// 
			this.PopupSplitContainer.Panel2.Controls.Add(this.CancelAttachButton);
			this.PopupSplitContainer.Panel2.Controls.Add(this.AttachButton);
			this.PopupSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 216, true);
			this.PopupSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(181);
			this.PopupSplitContainer.TabIndex = 3;
			// 
			// TreeUserControl
			// 
			this.TreeUserControl.AllowDrop = true;
			this.TreeUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TreeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TreeUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 140, true);
			this.TreeUserControl.Name = "TreeUserControl";
			this.TreeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 181, true);
			this.TreeUserControl.TabIndex = 2;
			this.TreeUserControl.ViewMode = Enterprise.Packing.GUI.PackingViewMode.Default;
			// 
			// CancelAttachButton
			// 
			this.CancelAttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelAttachButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingPopupDialog|171a67be-b7ff-4bfd-a015-971e0622a013", "Cancel");
			this.CancelAttachButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 2, true);
			this.CancelAttachButton.Name = "CancelAttachButton";
			this.CancelAttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelAttachButton.TabIndex = 1;
			this.CancelAttachButton.UseVisualStyleBackColor = true;
			this.CancelAttachButton.Click += new System.EventHandler(this.CancelChangesButton_Click);
			// 
			// AttachButton
			// 
			this.AttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingPopupDialog|9895d10d-5cdf-4859-98df-da103c1b40d8", "Assign");
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(676, 2, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AttachButton.TabIndex = 0;
			this.AttachButton.UseVisualStyleBackColor = true;
			this.AttachButton.Click += new System.EventHandler(this.AttachButton_Click);
			// 
			// PackingPopupDialog
			// 
			this.AcceptButton = this.AttachButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelAttachButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 216, true);
			this.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("PackingPopupDialog|c9254771-2260-4ad0-a6e6-c606ed720a24", "Packages to Assign");
			this.Controls.Add(this.PopupSplitContainer);
			this.DataSourceType = typeof(Enterprise.Packing.Business.PkgPackageJob);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 250, true);
			this.Name = "PackingPopupDialog";
			this.Controls.SetChildIndex(this.PopupSplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PopupSplitContainer.Panel1.ResumeLayout(false);
			this.PopupSplitContainer.Panel2.ResumeLayout(false);
			this.PopupSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer PopupSplitContainer;
		private PackingTreeViewUserControl TreeUserControl;
		private ZArchitecture.GUI.ZButton CancelAttachButton;
		private ZArchitecture.GUI.ZButton AttachButton;
	}
}
