namespace Enterprise.MasterData.GUI
{
	partial class PotentialDuplicatesUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CheckBoxTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ShowIgnoreCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExcludeInactiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExcludeOtherCountriesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContentPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.AdvancedFilterCriteriaControl = new Enterprise.MasterData.GUI.AdvancedFilterCriteriaControl();
			this.CandidatesForMergingControl = new Enterprise.MasterData.GUI.CandidatesForMergingControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTableLayoutPanel.SuspendLayout();
			this.HeaderPanel.SuspendLayout();
			this.CheckBoxTableLayoutPanel.SuspendLayout();
			this.ContentPanel.SuspendLayout();
			this.AdvancedFilterCriteriaControl.SuspendLayout();
			this.CandidatesForMergingControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Business.IDeduplicationResultDetail);
			// 
			// MainTableLayoutPanel
			// 
			this.MainTableLayoutPanel.AutoSize = true;
			this.MainTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTableLayoutPanel.BackColor = System.Drawing.Color.Silver;
			this.MainTableLayoutPanel.ColumnCount = 1;
			this.MainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTableLayoutPanel.Controls.Add(this.HeaderPanel, 0, 0);
			this.MainTableLayoutPanel.Controls.Add(this.ContentPanel, 0, 1);
			this.MainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MainTableLayoutPanel.Name = "MainTableLayoutPanel";
			this.MainTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.MainTableLayoutPanel.RowCount = 2;
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(40)));
			this.MainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 392, true);
			this.MainTableLayoutPanel.TabIndex = 0;
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.BackColor = System.Drawing.Color.WhiteSmoke;
			this.HeaderPanel.Controls.Add(this.CheckBoxTableLayoutPanel);
			this.HeaderPanel.Controls.Add(this.TitleLabel);
			this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.HeaderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 40, true);
			this.HeaderPanel.TabIndex = 0;
			// 
			// CheckBoxTableLayoutPanel
			// 
			this.CheckBoxTableLayoutPanel.AutoSize = true;
			this.CheckBoxTableLayoutPanel.ColumnCount = 3;
			this.CheckBoxTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CheckBoxTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CheckBoxTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CheckBoxTableLayoutPanel.Controls.Add(this.ShowIgnoreCheckBox, 0, 0);
			this.CheckBoxTableLayoutPanel.Controls.Add(this.ExcludeInactiveCheckBox, 1, 0);
			this.CheckBoxTableLayoutPanel.Controls.Add(this.ExcludeOtherCountriesCheckBox, 2, 0);
			this.CheckBoxTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.CheckBoxTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 0, true);
			this.CheckBoxTableLayoutPanel.Name = "CheckBoxTableLayoutPanel";
			this.CheckBoxTableLayoutPanel.RowCount = 1;
			this.CheckBoxTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.CheckBoxTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(10)));
			this.CheckBoxTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 40, true);
			this.CheckBoxTableLayoutPanel.TabIndex = 1;
			// 
			// ShowIgnoreCheckBox
			// 
			this.ShowIgnoreCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowIgnoreCheckBox, "IsShowIgnoredResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterData.Business.IDeduplicationResultDetail)(null)).IsShowIgnoredResults)));
			this.ShowIgnoreCheckBox.Text = Enterprise.MasterData.GUI.Res.GetString("C5713D6A-79AF-472D-AB01-201D59BEFA1E", "Show Ignored");
			this.ShowIgnoreCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShowIgnoreCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ShowIgnoreCheckBox.Name = "ShowIgnoreCheckBox";
			this.ShowIgnoreCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 37, true);
			this.ShowIgnoreCheckBox.TabIndex = 0;
			this.ShowIgnoreCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExcludeInactiveCheckBox
			// 
			this.ExcludeInactiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExcludeInactiveCheckBox, "IsExcludingInactiveResults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterData.Business.IDeduplicationResultDetail)(null)).IsExcludingInactiveResults)));
			this.ExcludeInactiveCheckBox.Text = Enterprise.MasterData.GUI.Res.GetString("d6a0f4fb-3aef-4f01-a441-6d4660ceca83", "Exclude Inactive");
			this.ExcludeInactiveCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExcludeInactiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 1, true);
			this.ExcludeInactiveCheckBox.Name = "ExcludeInactiveCheckBox";
			this.ExcludeInactiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 37, true);
			this.ExcludeInactiveCheckBox.TabIndex = 1;
			this.ExcludeInactiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExcludeOtherCountriesCheckBox
			// 
			this.ExcludeOtherCountriesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExcludeOtherCountriesCheckBox, "IsExcludingOtherCountries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterData.Business.IDeduplicationResultDetail)(null)).IsExcludingOtherCountries)));
			this.ExcludeOtherCountriesCheckBox.Text = Enterprise.MasterData.GUI.Res.GetString("C90A415D-2F90-49D7-AC3D-4796B31C883C", "Exclude Other Countries/Regions");
			this.ExcludeOtherCountriesCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExcludeOtherCountriesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 1, true);
			this.ExcludeOtherCountriesCheckBox.Name = "ExcludeOtherCountriesCheckBox";
			this.ExcludeOtherCountriesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 37, true);
			this.ExcludeOtherCountriesCheckBox.TabIndex = 2;
			this.ExcludeOtherCountriesCheckBox.UseVisualStyleBackColor = true;
			// 
			// TitleLabel
			// 
			this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TitleLabel, false);
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 40, true);
			this.TitleLabel.TabIndex = 0;
			this.TitleLabel.UseMnemonic = false;
			// 
			// ContentPanel
			// 
			this.ContentPanel.AutoSize = true;
			this.ContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ContentPanel.BackColor = System.Drawing.Color.White;
			this.ContentPanel.ColumnCount = 1;
			this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ContentPanel.Controls.Add(this.AdvancedFilterCriteriaControl, 0, 0);
			this.ContentPanel.Controls.Add(this.CandidatesForMergingControl, 0, 1);
			this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 42, true);
			this.ContentPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 0, 0, true);
			this.ContentPanel.Name = "ContentPanel";
			this.ContentPanel.RowCount = 2;
			this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 349, true);
			this.ContentPanel.TabIndex = 1;
			// 
			// AdvancedFilterCriteriaControl
			// 
			this.AdvancedFilterCriteriaControl.AllowDrop = true;
			this.AdvancedFilterCriteriaControl.AutoSize = true;
			this.AdvancedFilterCriteriaControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AdvancedFilterCriteriaControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdvancedFilterCriteriaControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdvancedFilterCriteriaControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AdvancedFilterCriteriaControl.Name = "AdvancedFilterCriteriaControl";
			this.AdvancedFilterCriteriaControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.AdvancedFilterCriteriaControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 133, true);
			this.AdvancedFilterCriteriaControl.TabIndex = 0;
			// 
			// CandidatesForMergingControl
			// 
			this.CandidatesForMergingControl.AllowDrop = true;
			this.CandidatesForMergingControl.AutoSize = true;
			this.CandidatesForMergingControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CandidatesForMergingControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CandidatesForMergingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 133, true);
			this.CandidatesForMergingControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CandidatesForMergingControl.Name = "CandidatesForMergingControl";
			this.CandidatesForMergingControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CandidatesForMergingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 216, true);
			this.CandidatesForMergingControl.TabIndex = 1;
			// 
			// PotentialDuplicatesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTableLayoutPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Name = "PotentialDuplicatesUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTableLayoutPanel.ResumeLayout(false);
			this.MainTableLayoutPanel.PerformLayout();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.CheckBoxTableLayoutPanel.ResumeLayout(false);
			this.CheckBoxTableLayoutPanel.PerformLayout();
			this.ContentPanel.ResumeLayout(false);
			this.ContentPanel.PerformLayout();
			this.AdvancedFilterCriteriaControl.ResumeLayout(true);
			this.AdvancedFilterCriteriaControl.PerformLayout();
			this.CandidatesForMergingControl.ResumeLayout(true);
			this.CandidatesForMergingControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private CargoWise.Windows.UI.KTableLayoutPanel MainTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel HeaderPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel ContentPanel;
		internal CandidatesForMergingControl CandidatesForMergingControl;
		private ZArchitecture.ZLabel TitleLabel;
		internal AdvancedFilterCriteriaControl AdvancedFilterCriteriaControl;
		private CargoWise.Windows.UI.KTableLayoutPanel CheckBoxTableLayoutPanel;
		private ZArchitecture.GUI.ZCheckBox ShowIgnoreCheckBox;
		private ZArchitecture.GUI.ZCheckBox ExcludeInactiveCheckBox;
		private ZArchitecture.GUI.ZCheckBox ExcludeOtherCountriesCheckBox;
	}
}
