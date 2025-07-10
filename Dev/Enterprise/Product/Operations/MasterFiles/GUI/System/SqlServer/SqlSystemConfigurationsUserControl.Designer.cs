namespace Enterprise.MasterFiles.GUI
{
	public partial class SqlSystemConfigurationsUserControl
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
		void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			mainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			buttonPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			saveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			applyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			configurationsGrid = new SqlSystemConfigurationsGrid();
			changedConfigsListBox = new Enterprise.ZArchitecture.GUI.ZListBox
			{
				ScrollAlwaysVisible = true
			};

			saveButton.Click += OnSaveButtonClick;
			applyButton.Click += OnApplyButtonClick;
			cancelButton.Click += (_, __) => ParentForm.Close();

			((System.ComponentModel.ISupportInitialize)BindingSource).BeginInit();
			mainPanel.SuspendLayout();
			buttonPanel.SuspendLayout();
			changedConfigsListBox.SuspendLayout();
			configurationsGrid.SuspendLayout();
			SuspendLayout();

			mainPanel.ColumnCount = 1;
			mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			mainPanel.Controls.Add(changedConfigsListBox, 0, 0);
			mainPanel.Controls.Add(configurationsGrid, 0, 1);
			mainPanel.Controls.Add(buttonPanel, 0, 2);
			mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			mainPanel.Name = nameof(mainPanel);
			mainPanel.RowCount = 3;
			mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75)));
			mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 785, true);
			mainPanel.TabIndex = 0;

			changedConfigsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			changedConfigsListBox.TabIndex = 1;
			changedConfigsListBox.Name = nameof(changedConfigsListBox);
			changedConfigsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			changedConfigsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 75, true);
			changedConfigsListBox.BackColor = System.Drawing.Color.LightYellow;

			configurationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			configurationsGrid.TabIndex = 2;
			configurationsGrid.Name = nameof(configurationsGrid);
			configurationsGrid.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SqlServerSystemConfigurationsForm|B612733F-3F8D-4580-847B-316172A7FDFD", "Server configurations");
			configurationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			configurationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 630, true);
			configurationsGrid.AutoSize = true;

			buttonPanel.AutoSize = true;
			buttonPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 21, true);
			buttonPanel.ColumnCount = 4;
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.Controls.Add(saveButton, 1, 0);
			buttonPanel.Controls.Add(applyButton, 2, 0);
			buttonPanel.Controls.Add(cancelButton, 3, 0);
			buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			buttonPanel.Name = nameof(buttonPanel);
			buttonPanel.RowCount = 1;
			buttonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.TabIndex = 3;

			saveButton.AutoSize = true;
			saveButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			saveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SqlServerSystemConfigurationsForm|CFADB5E4-672A-4807-B73D-1DC9C65235C7", "Save");
			saveButton.ToolTipCaption = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("719CDD9E-6DD8-4755-A7A8-B700F2E6619C", "Save proposed system configurations to the registry");
			saveButton.Name = nameof(saveButton);
			saveButton.TabIndex = 4;
			saveButton.Enabled = false;

			applyButton.AutoSize = true;
			applyButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			applyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SqlServerSystemConfigurationsForm|D12E075E-96B2-4B9D-8AA2-4A758144C266", "Apply");
			applyButton.ToolTipCaption = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("2370305E-CB9E-4095-BCBC-B8CE383DA670", "Apply to the connected SQL server and save proposed system configurations to the registry");
			applyButton.Name = nameof(applyButton);
			applyButton.TabIndex = 5;
			applyButton.Enabled = false;

			cancelButton.AutoSize = true;
			cancelButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			cancelButton.Text = CloseCaption;
			cancelButton.Name = nameof(cancelButton);
			cancelButton.TabIndex = 6;

			CaptionRenderingEnabled = true;
			AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 785, true);
			Controls.Add(mainPanel);
			DataSourceTypeName = "Enterprise.MasterFiles.GUI.SysConfigurationsCollection";

			Name = nameof(SqlSystemConfigurationsUserControl);
			Controls.SetChildIndex(mainPanel, 0);
			((System.ComponentModel.ISupportInitialize)BindingSource).EndInit();
			mainPanel.ResumeLayout(false);
			mainPanel.PerformLayout();
			buttonPanel.ResumeLayout(false);
			buttonPanel.PerformLayout();
			changedConfigsListBox.ResumeLayout(false);
			changedConfigsListBox.PerformLayout();
			configurationsGrid.ResumeLayout(false);
			configurationsGrid.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		CargoWise.Windows.UI.KTableLayoutPanel mainPanel;
		CargoWise.Windows.UI.KTableLayoutPanel buttonPanel;
		SqlSystemConfigurationsGrid configurationsGrid;
		Enterprise.ZArchitecture.GUI.ZButton saveButton;
		Enterprise.ZArchitecture.GUI.ZButton applyButton;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		Enterprise.ZArchitecture.GUI.ZListBox changedConfigsListBox;
	}
}
