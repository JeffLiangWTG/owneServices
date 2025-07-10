namespace Enterprise.MasterFiles.GUI
{
	public partial class RoutingOrderReplacementAgentSelectionForm
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OrgFindBox;
		private Enterprise.ZArchitecture.ZLabel InstructionsLabel;

		protected override void InitializeComponent()
		{
			this.OrgFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 142, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AgentSelectionBusinessObject);
			// 
			// OrgFindBox
			// 
			this.BindingSource.SetBindingMember(this.OrgFindBox, "SelectedAgentPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AgentSelectionBusinessObject)(null)).SelectedAgentPK)));
			this.OrgFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RoutingOrderReplacementAgentSelectionForm|b457efec-3750-4751-a056-f428316a795d", "Replacement Agent");
			this.OrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 82, true);
			this.OrgFindBox.Name = "OrgFindBox";
			this.OrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.OrgFindBox.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RoutingOrderReplacementAgentSelectionForm|8e42620d-6b18-4e5f-9c6e-606a54da2544", "OK");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(336, 119, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 45, true);
			this.InstructionsLabel.TabIndex = 4;
			// 
			// RoutingOrderReplacementAgentSelectionForm
			// 
			this.AcceptButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 166, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RoutingOrderReplacementAgentSelectionForm|3824e4b8-0a84-4fe8-b3fc-7ff56a860972", "Replacement Agent");
			this.Controls.Add(this.InstructionsLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OrgFindBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AgentSelectionBusinessObject);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "RoutingOrderReplacementAgentSelectionForm";
			this.Controls.SetChildIndex(this.OrgFindBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
