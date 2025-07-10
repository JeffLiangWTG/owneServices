namespace Enterprise.Customs.TW.GUI
{
	partial class TWCustomsNumberViewStmNumsEditorForm
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StartNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EndNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrentValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.CurrentValueTextBox);
			this.MainPanel.Controls.Add(this.EndNumberTextBox);
			this.MainPanel.Controls.Add(this.StartNumberTextBox);
			this.MainPanel.Controls.Add(this.MessageTypeDropEdit);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 161, true);
			this.MainPanel.Controls.SetChildIndex(this.TypeDropEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.FountainNameTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.ValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MinimumValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.CountCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MaximumValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MessageTypeDropEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.StartNumberTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.EndNumberTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.CurrentValueTextBox, 0);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 161, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 30, true);
			// 
			// TypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "RangeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper)(null)).RangeType)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 46, true);
			this.TypeDropEdit.ShouldResizeByMaxLength = false;
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.TypeDropEdit.TabIndex = 1;
			// 
			// FountainNameTextBox
			// 
			this.FountainNameTextBox.Enabled = false;
			this.FountainNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 133, true);
			this.FountainNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.FountainNameTextBox.TabIndex = 8;
			this.FountainNameTextBox.Visible = false;
			// 
			// ValueCalcEdit
			// 
			this.ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 133, true);
			this.ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.ValueCalcEdit.TabIndex = 9;
			this.ValueCalcEdit.Visible = false;
			// 
			// MinimumValueCalcEdit
			// 
			this.MinimumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 133, true);
			this.MinimumValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.MinimumValueCalcEdit.TabIndex = 6;
			this.MinimumValueCalcEdit.Visible = false;
			// 
			// MaximumValueCalcEdit
			// 
			this.MaximumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 133, true);
			this.MaximumValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.MaximumValueCalcEdit.TabIndex = 7;
			this.MaximumValueCalcEdit.Visible = false;
			// 
			// ValidateAndSaveButton
			// 
			this.ValidateAndSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 4, true);
			// 
			// CancelAndCloseButton
			// 
			this.CancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 4, true);
			// 
			// CountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CountCalcEdit, "Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper)(null)).Count)));
			this.CountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 98, true);
			this.CountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.CountCalcEdit.TabIndex = 5;
			// 
			// TopPanel
			// 
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 215, true);
			this.TopPanel.Visible = false;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 191, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper);
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper)(null)).MessageType)));
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 20, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 3;
			this.MessageTypeDropEdit.ShouldResizeByMaxLength = false;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.MessageTypeDropEdit.TabIndex = 0;
			// 
			// StartNumberTextBox
			// 
			this.StartNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StartNumberTextBox, "StartNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper)(null)).StartNumber)));
			this.StartNumberTextBox.CaptionResourceString = null;
			this.StartNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 72, true);
			this.StartNumberTextBox.Name = "StartNumberTextBox";
			this.StartNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.StartNumberTextBox.TabIndex = 2;
			// 
			// EndNumberTextBox
			// 
			this.EndNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EndNumberTextBox, "EndNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper)(null)).EndNumber)));
			this.EndNumberTextBox.CaptionResourceString = null;
			this.EndNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 72, true);
			this.EndNumberTextBox.Name = "EndNumberTextBox";
			this.EndNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.EndNumberTextBox.TabIndex = 3;
			// 
			// CurrentValueTextBox
			// 
			this.CurrentValueTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CurrentValueTextBox, "CurrentValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper)(null)).CurrentValue)));
			this.CurrentValueTextBox.CaptionResourceString = null;
			this.CurrentValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 98, true);
			this.CurrentValueTextBox.Name = "CurrentValueTextBox";
			this.CurrentValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.CurrentValueTextBox.TabIndex = 4;
			// 
			// TWCustomsNumberViewStmNumsEditorForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 215, true);
			this.DataSourceType = typeof(Enterprise.Customs.TW.Business.TWCustomsNumberViewStmNumsWrapper);
			this.Name = "TWCustomsNumberViewStmNumsEditorForm";
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		public ZArchitecture.ZTextBox StartNumberTextBox;
		public ZArchitecture.ZTextBox CurrentValueTextBox;
		public ZArchitecture.ZTextBox EndNumberTextBox;
	}
}
