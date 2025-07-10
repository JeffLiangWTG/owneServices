namespace Enterprise.MasterFiles.GUI
{
	public partial class SqlSystemConfigurationsForm
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
		protected override void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			sqlSystemConfigurationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			sqlSystemConfigurationsUserControl = CreateSqlSystemConfigurationsUserControl();
			databaseScopedConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			databaseScopedConfigurationUserControl = new DatabaseScopedConfigurationUserControl();

			((System.ComponentModel.ISupportInitialize)MessageStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)ErrorStatusBarPanel).BeginInit();
			((System.ComponentModel.ISupportInitialize)BindingSource).BeginInit();
			sqlSystemConfigurationsUserControl.SuspendLayout();
			sqlSystemConfigurationsTabPage.SuspendLayout();
			databaseScopedConfigurationUserControl.SuspendLayout();
			databaseScopedConfigurationTabPage.SuspendLayout();
			tabControl.SuspendLayout();
			SuspendLayout();

			tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			tabControl.Name = "tabControl";
			tabControl.Controls.Add(sqlSystemConfigurationsTabPage);
			tabControl.Controls.Add(databaseScopedConfigurationTabPage);

			sqlSystemConfigurationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C61C443E-D893-4C67-8C75-925310CAA034", "Server Configurations");
			sqlSystemConfigurationsTabPage.Controls.Add(sqlSystemConfigurationsUserControl);
			sqlSystemConfigurationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			sqlSystemConfigurationsTabPage.Name = "sqlSystemConfigurationsTabPage";
			sqlSystemConfigurationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 720, true);
			sqlSystemConfigurationsTabPage.TabIndex = 0;

			sqlSystemConfigurationsUserControl.AllowDrop = false;
			sqlSystemConfigurationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			sqlSystemConfigurationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			sqlSystemConfigurationsUserControl.Name = "sqlSystemConfigurationsUserControl";
			sqlSystemConfigurationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 700, true);
			sqlSystemConfigurationsUserControl.TabIndex = 1;

			databaseScopedConfigurationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("35BAB9FB-8BA7-4A00-B1C3-3F2B471F6BDD", "Database Configurations");
			databaseScopedConfigurationTabPage.Controls.Add(databaseScopedConfigurationUserControl);
			databaseScopedConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			databaseScopedConfigurationTabPage.Name = nameof(databaseScopedConfigurationTabPage);
			databaseScopedConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 720, true);
			databaseScopedConfigurationTabPage.TabIndex = 2;

			databaseScopedConfigurationUserControl.AllowDrop = false;
			databaseScopedConfigurationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			databaseScopedConfigurationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			databaseScopedConfigurationUserControl.Name = nameof(databaseScopedConfigurationUserControl);
			databaseScopedConfigurationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 700, true);
			databaseScopedConfigurationUserControl.TabIndex = 3;

			MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 22, true);
			MainStatusBar.SizingGrip = false;

			MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);

			CaptionRenderingEnabled = true;
			AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 785, true);
			DataSourceTypeName = "Enterprise.MasterFiles.GUI.SysConfigurationsCollection";

			Name = nameof(SqlSystemConfigurationsForm);
			Controls.Add(tabControl);
			Controls.SetChildIndex(tabControl, 0);

			((System.ComponentModel.ISupportInitialize)MessageStatusBarPanel).EndInit();
			((System.ComponentModel.ISupportInitialize)ErrorStatusBarPanel).EndInit();
			((System.ComponentModel.ISupportInitialize)BindingSource).EndInit();

			sqlSystemConfigurationsUserControl.ResumeLayout(false);
			sqlSystemConfigurationsUserControl.PerformLayout();
			sqlSystemConfigurationsTabPage.ResumeLayout(false);
			sqlSystemConfigurationsTabPage.PerformLayout();

			databaseScopedConfigurationUserControl.ResumeLayout(false);
			databaseScopedConfigurationUserControl.PerformLayout();
			databaseScopedConfigurationTabPage.ResumeLayout(false);
			databaseScopedConfigurationTabPage.PerformLayout();

			tabControl.ResumeLayout(false);
			tabControl.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZTabControl tabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage sqlSystemConfigurationsTabPage;
		SqlSystemConfigurationsUserControl sqlSystemConfigurationsUserControl;
		Enterprise.ZArchitecture.GUI.ZTabPage databaseScopedConfigurationTabPage;
		DatabaseScopedConfigurationUserControl databaseScopedConfigurationUserControl;
	}
}
