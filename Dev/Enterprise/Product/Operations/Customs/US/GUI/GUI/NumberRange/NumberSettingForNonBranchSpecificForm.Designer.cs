
namespace Enterprise.Customs.US.GUI
{
	partial class NumberSettingForNonBranchSpecificForm
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
		new void InitializeComponent()
		{
			this.CurrentSystemSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AvailableNumbersCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrentNextNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NextNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SetNextNumberButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NextNumberGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CurrentSystemSettingsGroupBox.SuspendLayout();
			this.NextNumberGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.NumberSetting);
			// 
			// CurrentSystemSettingsGroupBox
			// 
			this.CurrentSystemSettingsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("853eb80f-ec43-478b-becc-44628381ffa9", "Current System Settings");
			this.CurrentSystemSettingsGroupBox.Controls.Add(this.AvailableNumbersCalcEdit);
			this.CurrentSystemSettingsGroupBox.Controls.Add(this.CurrentNextNumberCalcEdit);
			this.CurrentSystemSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 60, true);
			this.CurrentSystemSettingsGroupBox.Name = "CurrentSystemSettingsGroupBox";
			this.CurrentSystemSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 76, true);
			this.CurrentSystemSettingsGroupBox.TabIndex = 1;
			this.CurrentSystemSettingsGroupBox.TabStop = false;
			this.CurrentSystemSettingsGroupBox.Text = "Current System Settings";
			// 
			// AvailableNumbersCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AvailableNumbersCalcEdit, "AvailableNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.NumberSetting)(null)).AvailableNumbers)));
			this.AvailableNumbersCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3a06716e-87f7-4c4a-826e-39bdaa7803ae", "Numbers Remaining");
			this.AvailableNumbersCalcEdit.DecimalPlaces = 0;
			this.AvailableNumbersCalcEdit.Decimals = 0;
			this.AvailableNumbersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 45, true);
			this.AvailableNumbersCalcEdit.Name = "AvailableNumbersCalcEdit";
			this.AvailableNumbersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AvailableNumbersCalcEdit.TabIndex = 5;
			this.AvailableNumbersCalcEdit.Text = "0";
			this.AvailableNumbersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrentNextNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CurrentNextNumberCalcEdit, "CurrentNextNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.NumberSetting)(null)).CurrentNextNumber)));
			this.CurrentNextNumberCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("831a195c-b1fd-491f-8ae8-459847b7356f", "Next Number");
			this.CurrentNextNumberCalcEdit.DecimalPlaces = 0;
			this.CurrentNextNumberCalcEdit.Decimals = 0;
			this.CurrentNextNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 19, true);
			this.CurrentNextNumberCalcEdit.Name = "CurrentNextNumberCalcEdit";
			this.CurrentNextNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CurrentNextNumberCalcEdit.TabIndex = 3;
			this.CurrentNextNumberCalcEdit.Text = "0";
			this.CurrentNextNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NextNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NextNumberCalcEdit, "NextNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.NumberSetting)(null)).NextNumber)));
			this.NextNumberCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("08ea9850-4d79-435a-945f-6fc1a4a26268", "Next Number");
			this.NextNumberCalcEdit.DecimalPlaces = 0;
			this.NextNumberCalcEdit.Decimals = 0;
			this.NextNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 19, true);
			this.NextNumberCalcEdit.Name = "NextNumberCalcEdit";
			this.NextNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NextNumberCalcEdit.TabIndex = 1;
			this.NextNumberCalcEdit.Text = "0";
			this.NextNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SetNextNumberButton
			// 
			this.SetNextNumberButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("afd2e313-144e-4199-826e-24461f44d36e", "Set");
			this.SetNextNumberButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 17, true);
			this.SetNextNumberButton.Name = "SetNextNumberButton";
			this.SetNextNumberButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SetNextNumberButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SetNextNumberButton.TabIndex = 2;
			this.SetNextNumberButton.Text = "&Set";
			this.SetNextNumberButton.ToolTipCaption = null;
			this.SetNextNumberButton.UseVisualStyleBackColor = true;
			this.SetNextNumberButton.Click += new System.EventHandler(this.SetNextNumberButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1c28a65e-e464-4d7c-a775-95900d069ccb", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 147, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Text = "&Close";
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// NextNumberGroupBox
			// 
			this.NextNumberGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b2dfcf34-a39a-49c3-886a-a88532a9e4c7", "Next Number");
			this.NextNumberGroupBox.Controls.Add(this.NextNumberCalcEdit);
			this.NextNumberGroupBox.Controls.Add(this.SetNextNumberButton);
			this.NextNumberGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.NextNumberGroupBox.Name = "NextNumberGroupBox";
			this.NextNumberGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 50, true);
			this.NextNumberGroupBox.TabIndex = 0;
			this.NextNumberGroupBox.TabStop = false;
			this.NextNumberGroupBox.Text = "Next Number";
			// 
			// NumberSettingForNonBranchSpecificForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 201, true);
			this.Controls.Add(this.NextNumberGroupBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.CurrentSystemSettingsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.NumberSetting);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.NumberSetting";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "NumberSettingForNonBranchSpecificForm";
			this.Text = "Number Settings";
			this.Controls.SetChildIndex(this.CurrentSystemSettingsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.NextNumberGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CurrentSystemSettingsGroupBox.ResumeLayout(false);
			this.CurrentSystemSettingsGroupBox.PerformLayout();
			this.NextNumberGroupBox.ResumeLayout(false);
			this.NextNumberGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox CurrentSystemSettingsGroupBox;
		protected Enterprise.ZArchitecture.ZCalcEdit NextNumberCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit CurrentNextNumberCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit AvailableNumbersCalcEdit;
		public Enterprise.ZArchitecture.GUI.ZButton SetNextNumberButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox NextNumberGroupBox;
	}
}
