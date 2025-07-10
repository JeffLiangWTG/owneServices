namespace Enterprise.Customs.US.GUI
{
	partial class BorderLineReleaseAllocationForm
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.AppliesToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrentValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalAvailableNumbersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NumberToAllocateZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AllocateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 144, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.BorderLineReleaseAllocator);
			// 
			// AppliesToTextBox
			// 
			this.AppliesToTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AppliesToTextBox, "AppliesTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.BorderLineReleaseAllocator)(null)).AppliesTo)));
			this.AppliesToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 12, true);
			this.AppliesToTextBox.Name = "AppliesToTextBox";
			this.AppliesToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.AppliesToTextBox.TabIndex = 0;
			this.AppliesToTextBox.TabStop = false;
			// 
			// CurrentValueCalcEdit
			// 
			this.CurrentValueCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrentValueCalcEdit, "CurrentValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.BorderLineReleaseAllocator)(null)).CurrentValue)));
			this.CurrentValueCalcEdit.DecimalPlaces = 2;
			this.CurrentValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 38, true);
			this.CurrentValueCalcEdit.Name = "CurrentValueCalcEdit";
			this.CurrentValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.CurrentValueCalcEdit.TabIndex = 1;
			this.CurrentValueCalcEdit.TabStop = false;
			this.CurrentValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalAvailableNumbersCalcEdit
			// 
			this.TotalAvailableNumbersCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TotalAvailableNumbersCalcEdit, "TotalAvailableNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.BorderLineReleaseAllocator)(null)).TotalAvailableNumbers)));
			this.TotalAvailableNumbersCalcEdit.DecimalPlaces = 2;
			this.TotalAvailableNumbersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 64, true);
			this.TotalAvailableNumbersCalcEdit.Name = "TotalAvailableNumbersCalcEdit";
			this.TotalAvailableNumbersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.TotalAvailableNumbersCalcEdit.TabIndex = 2;
			this.TotalAvailableNumbersCalcEdit.TabStop = false;
			this.TotalAvailableNumbersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberToAllocateCalcEdit
			// 
			this.NumberToAllocateZCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NumberToAllocateZCalcEdit, "NumberToAllocate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.BorderLineReleaseAllocator)(null)).NumberToAllocate)));
			this.NumberToAllocateZCalcEdit.DecimalPlaces = 0;
			this.NumberToAllocateZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 90, true);
			this.NumberToAllocateZCalcEdit.Name = "NumberToAllocateZCalcEdit";
			this.NumberToAllocateZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.NumberToAllocateZCalcEdit.TabIndex = 3;
			this.NumberToAllocateZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AllocateButton
			// 
			this.AllocateButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("89b25b83-bc0a-4f56-802c-feb969d5d9fc", "&Allocate");
			this.AllocateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 116, true);
			this.AllocateButton.Name = "AllocateButton";
			this.AllocateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AllocateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.AllocateButton.TabIndex = 4;
			this.AllocateButton.ToolTipCaption = null;
			this.AllocateButton.Click += new System.EventHandler(this.AllocateButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("70ad988c-343c-44e1-b9eb-495b94767f9e", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 116, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// BorderLineReleaseAllocationForm
			// 
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 168, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.AllocateButton);
			this.Controls.Add(this.NumberToAllocateZCalcEdit);
			this.Controls.Add(this.TotalAvailableNumbersCalcEdit);
			this.Controls.Add(this.CurrentValueCalcEdit);
			this.Controls.Add(this.AppliesToTextBox);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.BorderLineReleaseAllocator);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "BorderLineReleaseAllocationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AppliesToTextBox, 0);
			this.Controls.SetChildIndex(this.CurrentValueCalcEdit, 0);
			this.Controls.SetChildIndex(this.TotalAvailableNumbersCalcEdit, 0);
			this.Controls.SetChildIndex(this.NumberToAllocateZCalcEdit, 0);
			this.Controls.SetChildIndex(this.AllocateButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox AppliesToTextBox;
		private ZArchitecture.ZCalcEdit CurrentValueCalcEdit;
		private ZArchitecture.ZCalcEdit TotalAvailableNumbersCalcEdit;
		private ZArchitecture.ZCalcEdit NumberToAllocateZCalcEdit;
		private ZArchitecture.GUI.ZButton AllocateButton;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}
