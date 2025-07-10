namespace Enterprise.MasterFiles.GUI
{
	sealed partial class DatabaseScopedConfigurationUserControl
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
			dropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			mainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			buttonPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			applyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			configurationsGrid = new DatabaseScopedConfigurationGrid();

			applyButton.Click += OnApplyButtonClick;
			cancelButton.Click += (_, __) => ParentForm.Close();

			((System.ComponentModel.ISupportInitialize)BindingSource).BeginInit();
			mainPanel.SuspendLayout();
			dropEdit.SuspendLayout();
			buttonPanel.SuspendLayout();
			configurationsGrid.SuspendLayout();
			SuspendLayout();

			mainPanel.ColumnCount = 1;
			mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			mainPanel.Controls.Add(dropEdit, 0, 0);
			mainPanel.Controls.Add(configurationsGrid, 0, 1);
			mainPanel.Controls.Add(buttonPanel, 0, 2);
			mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			mainPanel.Name = nameof(mainPanel);
			mainPanel.RowCount = 3;
			mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 785, true);
			mainPanel.TabIndex = 0;

			this.BindingSource.SetBindingMember(dropEdit, nameof(viewModel.SelectedDatabase));
			dropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			dropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BE15D74D-487B-45AD-BD73-0B82B4073F1A", "Databases");
			dropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			dropEdit.Name = nameof(dropEdit);
			dropEdit.ShowDescriptionBox = false;
			dropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			dropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			dropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			dropEdit.TabIndex = 0;

			configurationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			configurationsGrid.TabIndex = 1;
			configurationsGrid.Name = nameof(configurationsGrid);
			configurationsGrid.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8B0A7114-8411-4184-8D98-A5FB7E9739F2", "Database configurations");
			configurationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			configurationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 630, true);
			configurationsGrid.AutoSize = true;

			buttonPanel.AutoSize = true;
			buttonPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 21, true);
			buttonPanel.ColumnCount = 3;
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.Controls.Add(applyButton, 1, 0);
			buttonPanel.Controls.Add(cancelButton, 2, 0);
			buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			buttonPanel.Name = nameof(buttonPanel);
			buttonPanel.RowCount = 1;
			buttonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
			buttonPanel.TabIndex = 2;

			applyButton.AutoSize = true;
			applyButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			applyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("93499C56-B4E2-46BF-B510-8C55849C107F", "Apply");
			applyButton.ToolTipCaption = Enterprise.ZArchitecture.Core.ResString.GetMultilingualString("4E271B97-63B6-4FFA-8940-2CF763EA1842", "Apply to the connected SQL server and save proposed system configurations to the registry");
			applyButton.Name = nameof(applyButton);
			applyButton.TabIndex = 3;
			applyButton.Enabled = false;

			cancelButton.AutoSize = true;
			cancelButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			cancelButton.Text = CloseCaption;
			cancelButton.Name = nameof(cancelButton);
			cancelButton.TabIndex = 4;

			CaptionRenderingEnabled = true;
			AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1075, 785, true);
			Controls.Add(mainPanel);
			DataSourceTypeName = "Enterprise.MasterFiles.GUI.DatabaseScopedConfigurationViewModel";

			Name = nameof(SqlSystemConfigurationsUserControl);
			Controls.SetChildIndex(mainPanel, 0);
			((System.ComponentModel.ISupportInitialize)BindingSource).EndInit();
			mainPanel.ResumeLayout(false);
			mainPanel.PerformLayout();
			dropEdit.ResumeLayout(false);
			dropEdit.PerformLayout();
			buttonPanel.ResumeLayout(false);
			buttonPanel.PerformLayout();
			configurationsGrid.ResumeLayout(false);
			configurationsGrid.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		DatabaseScopedConfigurationViewModel viewModel;
		CargoWise.Windows.UI.KTableLayoutPanel mainPanel;
		CargoWise.Windows.UI.KTableLayoutPanel buttonPanel;
		Enterprise.ZArchitecture.GUI.ZDropEdit dropEdit;
		DatabaseScopedConfigurationGrid configurationsGrid;
		Enterprise.ZArchitecture.GUI.ZButton applyButton;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}
