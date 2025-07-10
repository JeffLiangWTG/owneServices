using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbReleaseNoteForm : ZChildForm
	{
		protected internal Enterprise.ZArchitecture.ZGrid ReleaseNotesGrid;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected internal ZButton ViewButton;
		protected ZLabel SummaryLabel;

		new void InitializeComponent()
		{
			this.ReleaseNotesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SummaryLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseNotesGrid)).BeginInit();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 409, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 26, true);
			this.MainStatusBar.TabIndex = 10;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbReleaseNoteManager);
			//
			// ReleaseNotesGrid
			//
			this.ReleaseNotesGrid.AllowNavigation = false;
			this.ReleaseNotesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseNotesGrid, "ReleaseNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbReleaseNoteManager)(null)).ReleaseNotes)));
			this.ReleaseNotesGrid.CaptionVisible = false;
			this.ReleaseNotesGrid.GridId = "252a26ae-9054-4e17-af63-ceebde58ae96";
			this.ReleaseNotesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseNotesGrid.LayoutKey = "ReleaseNotesGrid";
			this.ReleaseNotesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ReleaseNotesGrid.Name = "ReleaseNotesGrid";
			this.ReleaseNotesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 168, true);
			this.ReleaseNotesGrid.TabIndex = 0;
			this.ReleaseNotesGrid.DoubleClick += new System.EventHandler(this.ReleaseNotesGrid_DoubleClick);
			//
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbReleaseNoteForm|6a8d9977-960f-4e49-95be-744977ab5c48", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 382, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CloseButton.TabIndex = 9;
			//
			// ViewButton
			//
			this.ViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbReleaseNoteForm|75fb5b46-2df7-4cfc-8e44-e0c8c1c80dfc", "View");
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 382, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.ViewButton.TabIndex = 8;
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			//
			// SummaryLabel
			//
			this.SummaryLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SummaryLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbReleaseNoteForm|b633bacb-44f3-4019-ac54-48ca890cab5e", "Summary");
			this.SummaryLabel.IsFontBold = true;
			this.SummaryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.SummaryLabel.Name = "SummaryLabel";
			this.SummaryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.SummaryLabel.TabIndex = 1;
			//
			// GlbReleaseNoteForm
			//
			this.AcceptButton = this.CloseButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 435, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbReleaseNoteForm|bc12637c-f795-424c-8823-8a898cd3cb5d", "Update Notes");
			this.Controls.Add(this.SummaryLabel);
			this.Controls.Add(this.ViewButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ReleaseNotesGrid);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbReleaseNoteManager);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.GlbReleaseNoteManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 464, true);
			this.Name = "GlbReleaseNoteForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.ReleaseNotesGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ViewButton, 0);
			this.Controls.SetChildIndex(this.SummaryLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseNotesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
