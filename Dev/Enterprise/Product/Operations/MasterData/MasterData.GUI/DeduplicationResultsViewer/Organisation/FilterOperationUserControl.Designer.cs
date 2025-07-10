namespace Enterprise.MasterData.GUI
{
	partial class FilterOperationUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OperationTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.SavedFilterComboBox = new CargoWise.Windows.UI.KComboBox();
			this.LoadButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.FilterNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.kFlowLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.AddFilterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OperationTableLayoutPanel.SuspendLayout();
			this.kFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// OperationTableLayoutPanel
			// 
			this.OperationTableLayoutPanel.ColumnCount = 10;
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(201)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(18)));
			this.OperationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.OperationTableLayoutPanel.Controls.Add(this.zLabel1, 0, 0);
			this.OperationTableLayoutPanel.Controls.Add(this.SavedFilterComboBox, 1, 0);
			this.OperationTableLayoutPanel.Controls.Add(this.LoadButton, 2, 0);
			this.OperationTableLayoutPanel.Controls.Add(this.DeleteButton, 3, 0);
			this.OperationTableLayoutPanel.Controls.Add(this.zLabel2, 5, 0);
			this.OperationTableLayoutPanel.Controls.Add(this.FilterNameTextBox, 6, 0);
			this.OperationTableLayoutPanel.Controls.Add(this.SaveButton, 7, 0);
			this.OperationTableLayoutPanel.Controls.Add(this.kFlowLayoutPanel, 9, 0);
			this.OperationTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OperationTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.OperationTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OperationTableLayoutPanel.Name = "OperationTableLayoutPanel";
			this.OperationTableLayoutPanel.RowCount = 1;
			this.OperationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(14)));
			this.OperationTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 24, true);
			this.OperationTableLayoutPanel.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("64A28574-2CB0-4004-BD68-245557C08BA4", "Load/Delete Saved Filter:");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.UseMnemonic = false;
			// 
			// SavedFilterComboBox
			// 
			this.SavedFilterComboBox.AllowDrop = true;
			this.SavedFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SavedFilterComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 3, true);
			this.SavedFilterComboBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 0, true);
			this.SavedFilterComboBox.Name = "SavedFilterComboBox";
			this.SavedFilterComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 17, true);
			this.SavedFilterComboBox.TabIndex = 1;
			// 
			// LoadButton
			// 
			this.LoadButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("C13E0BFF-A92F-4529-9BD6-018E87640DFF", "Load");
			this.LoadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 0, true);
			this.LoadButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.LoadButton.Name = "LoadButton";
			this.LoadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 24, true);
			this.LoadButton.TabIndex = 2;
			this.LoadButton.ToolTipCaption = null;
			this.LoadButton.UseVisualStyleBackColor = true;
			this.LoadButton.Click += new System.EventHandler(this.LoadButtonOnClick);
			// 
			// DeleteButton
			// 
			this.DeleteButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("E4C157CC-6D58-4F0F-9A4A-A61B0DD0D65C", "Delete");
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 0, true);
			this.DeleteButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 24, true);
			this.DeleteButton.TabIndex = 3;
			this.DeleteButton.ToolTipCaption = null;
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButtonOnClick);
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("67F85D91-C535-4F50-9822-FACBD86A77B9", "Save Filter As:");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 0, true);
			this.zLabel2.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.zLabel2.TabIndex = 4;
			this.zLabel2.UseMnemonic = false;
			// 
			// FilterNameTextBox
			// 
			this.FilterNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 3, true);
			this.FilterNameTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 0, true);
			this.FilterNameTextBox.Name = "FilterNameTextBox";
			this.FilterNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 16, true);
			this.FilterNameTextBox.TabIndex = 5;
			this.FilterNameTextBox.MaxLength = 250;
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("69919EEE-0F97-44D3-95BE-FD351A50B818", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(799, 0, true);
			this.SaveButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 24, true);
			this.SaveButton.TabIndex = 6;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButtonOnClick);
			// 
			// kFlowLayoutPanel
			// 
			this.kFlowLayoutPanel.Controls.Add(this.AddFilterButton);
			this.kFlowLayoutPanel.Controls.Add(this.FindButton);
			this.kFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.kFlowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(873, 0, true);
			this.kFlowLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.kFlowLayoutPanel.Name = "kFlowLayoutPanel";
			this.kFlowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.kFlowLayoutPanel.TabIndex = 7;
			// 
			// AddFilterButton
			// 
			this.AddFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddFilterButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("E505468D-A3FB-4FEA-84D6-1846932DE159", "+");
			this.AddFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 0, true);
			this.AddFilterButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AddFilterButton.Name = "AddFilterButton";
			this.AddFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 24, true);
			this.AddFilterButton.TabIndex = 1;
			this.AddFilterButton.ToolTipCaption = null;
			this.AddFilterButton.UseVisualStyleBackColor = true;
			this.AddFilterButton.Click += new System.EventHandler(this.AddFilterButton_Click);
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("EDAF379A-AEF6-46AA-BAB6-0F15EE3A466B", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.FindButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 24, true);
			this.FindButton.TabIndex = 0;
			this.FindButton.ToolTipCaption = null;
			this.FindButton.UseVisualStyleBackColor = true;
			this.FindButton.Click += new System.EventHandler(this.FindButtonOnClick);
			// 
			// FilterOperationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OperationTableLayoutPanel);
			this.Name = "FilterOperationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 32, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OperationTableLayoutPanel.ResumeLayout(false);
			this.OperationTableLayoutPanel.PerformLayout();
			this.kFlowLayoutPanel.ResumeLayout(false);
			this.kFlowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KTableLayoutPanel OperationTableLayoutPanel;
		private ZArchitecture.ZLabel zLabel1;
		private CargoWise.Windows.UI.KComboBox SavedFilterComboBox;
		private ZArchitecture.GUI.ZButton LoadButton;
		private ZArchitecture.GUI.ZButton DeleteButton;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZTextBox FilterNameTextBox;
		private ZArchitecture.GUI.ZButton SaveButton;
		private CargoWise.Windows.UI.KFlowLayoutPanel kFlowLayoutPanel;
		private ZArchitecture.GUI.ZButton FindButton;
		private ZArchitecture.GUI.ZButton AddFilterButton;
	}
}
