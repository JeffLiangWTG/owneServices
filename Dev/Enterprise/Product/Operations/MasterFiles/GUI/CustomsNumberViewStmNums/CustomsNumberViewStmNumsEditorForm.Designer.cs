namespace Enterprise.MasterFiles.GUI
{
	partial class CustomsNumberViewStmNumsEditorForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ValidateAndSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MaximumValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MinimumValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FountainNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 200, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ValidateAndSaveButton);
			this.BottomPanel.Controls.Add(this.CancelAndCloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 30, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// ValidateAndSaveButton
			// 
			this.ValidateAndSaveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6313ddc7-2528-4a1c-92bf-e27a0c99f088", "&Save && Close");
			this.ValidateAndSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 3, true);
			this.ValidateAndSaveButton.Name = "ValidateAndSaveButton";
			this.ValidateAndSaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAndSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ValidateAndSaveButton.TabIndex = 0;
			this.ValidateAndSaveButton.ToolTipCaption = null;
			this.ValidateAndSaveButton.UseVisualStyleBackColor = true;
			this.ValidateAndSaveButton.Click += new System.EventHandler(this.ValidateAndSaveButton_Click);
			// 
			// CancelAndCloseButton
			// 
			this.CancelAndCloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d813d451-05c8-4aca-8e7f-8498ce467f87", "&Cancel");
			this.CancelAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
			this.CancelAndCloseButton.Name = "CancelAndCloseButton";
			this.CancelAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelAndCloseButton.TabIndex = 1;
			this.CancelAndCloseButton.ToolTipCaption = null;
			this.CancelAndCloseButton.UseVisualStyleBackColor = true;
			this.CancelAndCloseButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MaximumValueCalcEdit);
			this.MainPanel.Controls.Add(this.CountCalcEdit);
			this.MainPanel.Controls.Add(this.MinimumValueCalcEdit);
			this.MainPanel.Controls.Add(this.ValueCalcEdit);
			this.MainPanel.Controls.Add(this.FountainNameTextBox);
			this.MainPanel.Controls.Add(this.TypeDropEdit);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 33, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 137, true);
			this.MainPanel.TabIndex = 1;
			// 
			// MaximumValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaximumValueCalcEdit, "SN_MaximumValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(null)).SN_MaximumValue)));
			this.MaximumValueCalcEdit.DecimalPlaces = 0;
			this.MaximumValueCalcEdit.Decimals = 0;
			this.MaximumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 142, true);
			this.MaximumValueCalcEdit.Name = "MaximumValueCalcEdit";
			this.MaximumValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.MaximumValueCalcEdit.TabIndex = 5;
			this.MaximumValueCalcEdit.Text = "0";
			this.MaximumValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CountCalcEdit, "SN_Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(null)).SN_Count)));
			this.CountCalcEdit.DecimalPlaces = 0;
			this.CountCalcEdit.Decimals = 0;
			this.CountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 107, true);
			this.CountCalcEdit.Name = "CountCalcEdit";
			this.CountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.CountCalcEdit.TabIndex = 4;
			this.CountCalcEdit.Text = "0";
			this.CountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinimumValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinimumValueCalcEdit, "SN_MinimumValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(null)).SN_MinimumValue)));
			this.MinimumValueCalcEdit.DecimalPlaces = 0;
			this.MinimumValueCalcEdit.Decimals = 0;
			this.MinimumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 81, true);
			this.MinimumValueCalcEdit.Name = "MinimumValueCalcEdit";
			this.MinimumValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.MinimumValueCalcEdit.TabIndex = 3;
			this.MinimumValueCalcEdit.Text = "0";
			this.MinimumValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValueCalcEdit, "SN_ValueForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(null)).SN_ValueForDisplay)));
			this.ValueCalcEdit.DecimalPlaces = 0;
			this.ValueCalcEdit.Decimals = 0;
			this.ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 55, true);
			this.ValueCalcEdit.Name = "ValueCalcEdit";
			this.ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.ValueCalcEdit.TabIndex = 2;
			this.ValueCalcEdit.Text = "0";
			this.ValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FountainNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.FountainNameTextBox, "SN_FountainName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(null)).SN_FountainName)));
			this.FountainNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 29, true);
			this.FountainNameTextBox.Name = "FountainNameTextBox";
			this.FountainNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.FountainNameTextBox.TabIndex = 1;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "SN_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(null)).SN_Type)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 3, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.PreBoundMaxLength = 3;
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 20, true);
			this.TypeDropEdit.TabIndex = 0;
			// 
			// CustomsNumberViewStmNumsEditorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelAndCloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("76d7c122-c560-44d6-8eec-34d6b3528284", "Number Range");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 224, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "CustomsNumberViewStmNumsEditorForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZPanel MainPanel;
		protected ZArchitecture.GUI.ZPanel BottomPanel;
		protected ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		protected ZArchitecture.ZTextBox FountainNameTextBox;
		protected ZArchitecture.ZCalcEdit ValueCalcEdit;
		protected ZArchitecture.ZCalcEdit MinimumValueCalcEdit;
		protected ZArchitecture.ZCalcEdit MaximumValueCalcEdit;
		protected ZArchitecture.GUI.ZButton ValidateAndSaveButton;
		protected ZArchitecture.GUI.ZButton CancelAndCloseButton;
		protected ZArchitecture.ZCalcEdit CountCalcEdit;
	}
}
