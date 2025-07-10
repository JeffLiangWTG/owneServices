using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class NewsAndAnnouncementForm : ZTemplateForm
	{
		ZArchitecture.ZTextBox zTextBox2;
		ZArchitecture.ZTextBox zTextBox1;
		ZDropEdit zDropEdit2;
		ZDropEdit zDropEdit1;
		ZGroupBox zGroupBox1;
		ImageSelectionControl ThumbnailSelectionControl;

		new void InitializeComponent()
		{
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ThumbnailSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.ThumbnailSelectionControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 376, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zGroupBox1);
			this.MainTabPage.Controls.Add(this.zDropEdit2);
			this.MainTabPage.Controls.Add(this.zTextBox2);
			this.MainTabPage.Controls.Add(this.zTextBox1);
			this.MainTabPage.Controls.Add(this.zDropEdit1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 354, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 354, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 354, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 376, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(820);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.NewsAnnouncement);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "GF_Summary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).GF_Summary)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 16, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 20, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "GF_Section");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).GF_Section)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 42, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "GF_URL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).GF_URL)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 98, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 20, true);
			this.zTextBox2.TabIndex = 3;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "GF_RN_NKCountryForReleaseNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).GF_RN_NKCountryForReleaseNote)));
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 70, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.zDropEdit2.SupportsEmptyCode = true;
			this.zDropEdit2.TabIndex = 2;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NewsAndAnnouncementForm|9797508d-784f-416f-9a22-919bd5263043", "Thumbnail ");
			this.zGroupBox1.Controls.Add(this.ThumbnailSelectionControl);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 126, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 213, true);
			this.zGroupBox1.TabIndex = 25;
			this.zGroupBox1.TabStop = false;
			// 
			// ThumbnailSelectionControl
			// 
			this.ThumbnailSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ThumbnailSelectionControl, "ThumbnailImage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.MasterFiles.Business.NewsAnnouncement)(null)).ThumbnailImage)));
			this.ThumbnailSelectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ThumbnailSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ThumbnailSelectionControl.Name = "ThumbnailSelectionControl";
			this.ThumbnailSelectionControl.ReadOnly = false;
			this.ThumbnailSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 196, true);
			this.ThumbnailSelectionControl.TabIndex = 0;
			// 
			// NewsAndAnnouncementForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 432, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.NewsAnnouncement);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 276, true);
			this.Name = "NewsAndAnnouncementForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ThumbnailSelectionControl.ResumeLayout(true);
			this.ThumbnailSelectionControl.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
