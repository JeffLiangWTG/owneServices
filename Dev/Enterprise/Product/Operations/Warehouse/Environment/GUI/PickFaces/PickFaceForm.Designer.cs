using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Warehouse.Environment.GUI.PickFaces
{
	partial class PickFaceForm : ZTemplateForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private ZPanel DetailsBottomPanel;
		private ZPanel DetailsTopPanel;
		private ZGuidFindBox OrgFindBox;
		private ZGuidFindBox ProductFindBox;
		private ZGroupBox PickFaceDetailsGroupBox;
		private ZTextBox LocationTextBox;
		private ZCalcEdit ReplenishMinCalcEdit;
		private ZCalcEdit ReplenishMaxCalcEdit;
		private ZCalcEdit ReplenishMultipleCalcEdit;

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
            this.MainTabControl.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.SaveButtonUserControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 384, true);
            // 
            // MainTabPage
            // 
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 357, true);
            this.MainTabPage.TabIndex = 0;
            this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 450, true);
            this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 450, true);
            MainTabControl.Controls.Remove(LogsTabPage);
            LogsTabPage.Dispose();
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 384, true);
            // 
            // SaveButtonUserControl
            // 
            this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 7, true);
            this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 25, true);
            // 
            // PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
            // 
            this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 5, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsPickFace);
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).WF_OH_Client)));
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).WF_OP)));
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).WF_ReplenishMinimum)));
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).WF_ReplenishMaximum)));
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Environment.Business.WhsPickFace)(null)).WF_ReplenishmentMultiple)));
            // 
            // PickFaceForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 440, true);
            this.DataSourceType = typeof(Enterprise.Warehouse.Environment.Business.WhsPickFace);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 500, true);
            this.Name = "PickFaceForm";
            this.ShouldSerializeTabPageMethods = true;
            this.Text = "PickFaceForm";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.SaveButtonUserControl.ResumeLayout(true);
            this.SaveButtonUserControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
            // 
            // ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
            // 
            this.NotesTabPage.SuspendLayout();
            this.NotesTabPage.PerformLayout();
            this.NotesTabPage.ResumeLayout(true);

        }

        #endregion
        private ZGroupBox CommittedPicksGridGroupBox;
		private CommittedPicksGridUserControl CommittedPicksGridControl;
		private ZGroupBox AwaitingPicksGridGroupBox;
		private AwaitingPicksGridUserControl AwaitingPicksGridUserControl;
		protected ZButton ProductButton;
	}
}
