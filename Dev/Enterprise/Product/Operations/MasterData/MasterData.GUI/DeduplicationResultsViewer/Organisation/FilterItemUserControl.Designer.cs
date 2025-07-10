using CargoWise.Windows.UI;

namespace Enterprise.MasterData.GUI
{
	partial class FilterItemUserControl
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
			UnhookValueChangedEvent();
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FilterTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FilterConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FilterKeywordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RemoveFilterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CheckBoxLayoutPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterTypeDropEdit.SuspendLayout();
			this.FilterConditionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.GUI.OrgFilterItemDataSource);
			// 
			// FilterTypeDropEdit
			// 
			this.FilterTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterTypeDropEdit, "OrgFilterTypeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterData.GUI.OrgFilterItemDataSource)(null)).OrgFilterTypeDescription)));
			this.FilterTypeDropEdit.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("F0435675-9533-45DF-B959-1409B080EC79", "Filter Type");
			this.FilterTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.FilterTypeDropEdit.Name = "FilterTypeDropEdit";
			this.FilterTypeDropEdit.PreBoundMaxLength = 18;
			this.FilterTypeDropEdit.ShowDescriptionBox = false;
			this.FilterTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.FilterTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 16, true);
			this.FilterTypeDropEdit.TabIndex = 0;
			// 
			// FilterConditionDropEdit
			// 
			this.FilterConditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FilterConditionDropEdit, "OrgFilterOptionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterData.GUI.OrgFilterItemDataSource)(null)).OrgFilterOptionDescription)));
			this.FilterConditionDropEdit.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("DFD49693-878F-4B0A-B164-26429DF15184", "Filter Condition");
			this.FilterConditionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 8, true);
			this.FilterConditionDropEdit.Name = "FilterConditionDropEdit";
			this.FilterConditionDropEdit.PreBoundMaxLength = 15;
			this.FilterConditionDropEdit.ShowDescriptionBox = false;
			this.FilterConditionDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.FilterConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 16, true);
			this.FilterConditionDropEdit.TabIndex = 1;
			this.FilterConditionDropEdit.Visible = false;
			// 
			// FilterKeywordTextBox
			// 
			this.BindingSource.SetBindingMember(this.FilterKeywordTextBox, "OrgFilterKeyword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterData.GUI.OrgFilterItemDataSource)(null)).OrgFilterKeyword)));
			this.FilterKeywordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterKeywordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 8, true);
			this.FilterKeywordTextBox.Name = "FilterKeywordTextBox";
			this.FilterKeywordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 16, true);
			this.FilterKeywordTextBox.TabIndex = 2;
			this.FilterKeywordTextBox.Visible = false;
			// 
			// RemoveFilterButton
			// 
			this.RemoveFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveFilterButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("65A5B117-C838-434D-8E31-F452F65A0257", "-");
			this.RemoveFilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(816, 7, true);
			this.RemoveFilterButton.Name = "RemoveFilterButton";
			this.RemoveFilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 26, true);
			this.RemoveFilterButton.TabIndex = 3;
			this.RemoveFilterButton.ToolTipCaption = null;
			this.RemoveFilterButton.UseVisualStyleBackColor = true;
			this.RemoveFilterButton.Click += new System.EventHandler(this.RemoveFilterButton_Click);
			// 
			// CheckBoxLayoutPanel
			// 
			this.CheckBoxLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 4, true);
			this.CheckBoxLayoutPanel.Name = "CheckBoxLayoutPanel";
			this.CheckBoxLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 84, true);
			this.CheckBoxLayoutPanel.TabIndex = 0;
			this.CheckBoxLayoutPanel.Visible = false;
			// 
			// FilterItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FilterKeywordTextBox);
			this.Controls.Add(this.FilterConditionDropEdit);
			this.Controls.Add(this.FilterTypeDropEdit);
			this.Controls.Add(this.RemoveFilterButton);
			this.Controls.Add(this.CheckBoxLayoutPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 38, true);
			this.Name = "FilterItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 103, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterTypeDropEdit.ResumeLayout(true);
			this.FilterTypeDropEdit.PerformLayout();
			this.FilterConditionDropEdit.ResumeLayout(true);
			this.FilterConditionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit FilterTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit FilterConditionDropEdit;
		public ZArchitecture.ZTextBox FilterKeywordTextBox;
		internal ZArchitecture.GUI.ZButton RemoveFilterButton;
		public KFlowLayoutPanel CheckBoxLayoutPanel;
	}
}
