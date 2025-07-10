namespace Enterprise.MasterFiles.GUI
{
	public partial class AccGroupsForm
	{
		#region Windows Form Designer generated code

		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox AR_GroupBoundText;
		private Enterprise.ZArchitecture.ZTranslatableTextControl AR_DescBoundText;

		protected override void InitializeComponent()
		{
			this.AR_GroupBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.AR_DescBoundText = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 91, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(572);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGroups);
			// 
			// AR_GroupBoundText
			// 
			this.BindingSource.SetBindingMember(this.AR_GroupBoundText, "AR_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroups)(null)).AR_Code)));
			this.AR_GroupBoundText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGroupsForm|2505742f-5964-41d5-bb0a-73c723333fe1", "Group", "A short code of up to 10 alphanumeric characters.");
			this.AR_GroupBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 7, true);
			this.AR_GroupBoundText.Name = "AR_GroupBoundText";
			this.AR_GroupBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.AR_GroupBoundText.TabIndex = 0;
			// 
			// AR_DescBoundText
			// 
			this.BindingSource.SetBindingMember(this.AR_DescBoundText, "AR_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccGroups)(null)).AR_Desc)));
			this.AR_DescBoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AR_DescBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 30, true);
			this.AR_DescBoundText.Name = "AR_DescBoundText";
			this.AR_DescBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.AR_DescBoundText.TabIndex = 1;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 63, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.ButtonsUserControl.TabIndex = 23;
			this.ButtonsUserControl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			// 
			// AccGroupsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 115, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccGroupsForm|99d7889e-66e3-48fc-bc11-a00ce20062f2", "Sales/Expense Group");
			this.Controls.Add(this.ButtonsUserControl);
			this.Controls.Add(this.AR_DescBoundText);
			this.Controls.Add(this.AR_GroupBoundText);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGroups);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AccGroupsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AR_GroupBoundText, 0);
			this.Controls.SetChildIndex(this.AR_DescBoundText, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
