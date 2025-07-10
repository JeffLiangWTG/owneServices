namespace Enterprise.Customs.US.GUI
{
	partial class USCustomsNumberViewStmNumsEditorForm
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CheckDigitAdditionCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AppliesToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.AppliesToTextBox);
			this.MainPanel.Controls.Add(this.CheckDigitAdditionCalcEdit);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 190, true);
			this.MainPanel.Controls.SetChildIndex(this.CheckDigitAdditionCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.AppliesToTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.TypeDropEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.FountainNameTextBox, 0);
			this.MainPanel.Controls.SetChildIndex(this.ValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MinimumValueCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.CountCalcEdit, 0);
			this.MainPanel.Controls.SetChildIndex(this.MaximumValueCalcEdit, 0);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 223, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 30, true);
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 7, true);
			// 
			// FountainNameTextBox
			// 
			this.FountainNameTextBox.Enabled = false;
			this.FountainNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 163, true);
			this.FountainNameTextBox.TabIndex = 7;
			// 
			// ValueCalcEdit
			// 
			this.ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 85, true);
			this.ValueCalcEdit.TabIndex = 3;
			// 
			// MinimumValueCalcEdit
			// 
			this.MinimumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 111, true);
			this.MinimumValueCalcEdit.TabIndex = 4;
			// 
			// MaximumValueCalcEdit
			// 
			this.MaximumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 163, true);
			this.MaximumValueCalcEdit.TabIndex = 6;
			// 
			// CountCalcEdit
			// 
			this.CountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 137, true);
			this.CountCalcEdit.TabIndex = 5;
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 33, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USCustomsNumberViewStmNumsWrapper);
			// 
			// CheckDigitAdditionCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CheckDigitAdditionCalcEdit, "CheckDigitAddition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.USCustomsNumberViewStmNumsWrapper)(null)).CheckDigitAddition)));
			this.CheckDigitAdditionCalcEdit.DecimalPlaces = 2;
			this.CheckDigitAdditionCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 59, true);
			this.CheckDigitAdditionCalcEdit.Name = "CheckDigitAdditionCalcEdit";
			this.CheckDigitAdditionCalcEdit.ShowGroupSeparators = false;
			this.CheckDigitAdditionCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 20, true);
			this.CheckDigitAdditionCalcEdit.TabIndex = 2;
			this.CheckDigitAdditionCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AppliesToTextBox
			// 
			this.AppliesToTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AppliesToTextBox, "AppliesTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCustomsNumberViewStmNumsWrapper)(null)).AppliesTo)));
			this.AppliesToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 33, true);
			this.AppliesToTextBox.Name = "AppliesToTextBox";
			this.AppliesToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AppliesToTextBox.TabIndex = 1;
			// 
			// USCustomsNumberViewStmNumsEditorForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 277, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.USCustomsNumberViewStmNumsWrapper);
			this.Name = "USCustomsNumberViewStmNumsEditorForm";
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZCalcEdit CheckDigitAdditionCalcEdit;
		ZArchitecture.ZTextBox AppliesToTextBox;
	}
}
