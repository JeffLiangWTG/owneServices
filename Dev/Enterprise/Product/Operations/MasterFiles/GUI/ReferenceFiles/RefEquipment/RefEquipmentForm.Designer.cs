namespace Enterprise.MasterFiles.GUI
{
	public partial class RefEquipmentForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CertificatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 499, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefEquipment);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.CertificatesTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.MainTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 447, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 468, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefEquipmentForm|7ea35bda-a4d4-47f4-a67a-f3a565dabbbe", "Equipment");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 441, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// CertificatesTabPage
			// 
			this.CertificatesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefEquipmentForm|cbada6a5-fa22-45c0-97c1-c4a790c59453", "Certificates and ID Numbers");
			this.CertificatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CertificatesTabPage.Name = "CertificatesTabPage";
			this.CertificatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 441, true);
			this.CertificatesTabPage.TabIndex = 3;
			this.CertificatesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CertificatesTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ICertificatesProvider)(((Enterprise.MasterFiles.Business.RefEquipment)(null)))));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 441, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 441, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 477, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// RefEquipmentForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c66a3567-ad15-4ee2-856f-055ae50650ee", "Equipment");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 523, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefEquipment);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 561, true);
			this.Name = "RefEquipmentForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EquipmentDetailsUserControl = new Enterprise.MasterFiles.GUI.EquipmentDetailsUserControl();
			this.VehicleDetailsUserControl = new Enterprise.MasterFiles.GUI.VehicleDetailsUserControl();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.MainTabPage.Controls.Add(this.SplitContainer);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 420, true);
			this.SplitContainer.Name = "SplitContainer";
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.EquipmentDetailsUserControl);
			this.SplitContainer.Panel1MinSize = 300;
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.VehicleDetailsUserControl);
			this.SplitContainer.Panel2MinSize = 300;
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 441, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(323);
			this.SplitContainer.TabIndex = 0;
			// 
			// EquipmentDetailsUserControl
			// 
			this.EquipmentDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EquipmentDetailsUserControl, ".");
			this.EquipmentDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EquipmentDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EquipmentDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 420, true);
			this.EquipmentDetailsUserControl.Name = "EquipmentDetailsUserControl";
			this.EquipmentDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 441, true);
			this.EquipmentDetailsUserControl.TabIndex = 0;
			// 
			// VehicleDetailsUserControl
			// 
			this.VehicleDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleDetailsUserControl, ".");
			this.VehicleDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehicleDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VehicleDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 380, true);
			this.VehicleDetailsUserControl.Name = "VehicleDetailsUserControl";
			this.VehicleDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 441, true);
			this.VehicleDetailsUserControl.TabIndex = 0;
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(true);
		}

		private void CertificatesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CertificatesUserControl = new Enterprise.MasterFiles.GUI.CertificatesUserControl();
			this.CertificatesTabPage.SuspendLayout();
			this.CertificatesTabPage.Controls.Add(this.CertificatesUserControl);
			// 
			// CertificatesUserControl
			// 
			this.CertificatesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificatesUserControl, ".");
			this.CertificatesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificatesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificatesUserControl.Name = "CertificatesUserControl";
			this.CertificatesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 441, true);
			this.CertificatesUserControl.TabIndex = 0;
			this.CertificatesTabPage.ResumeLayout(true);
		}

		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZTabPage CertificatesTabPage;
		protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
		protected EquipmentDetailsUserControl EquipmentDetailsUserControl;
		protected VehicleDetailsUserControl VehicleDetailsUserControl;
		private CertificatesUserControl CertificatesUserControl;
		private System.ComponentModel.IContainer components;
	}
}
