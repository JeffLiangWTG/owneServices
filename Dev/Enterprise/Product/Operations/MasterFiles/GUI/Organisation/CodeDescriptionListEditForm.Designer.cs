namespace Enterprise.MasterFiles.GUI
{
	public partial class CodeDescriptionListEditForm
	{

		#region Windows Form Designer generated code

		protected Enterprise.Core.Forms.CodeDescriptionListEditControl CodeDescriptionListEditControl;
		protected new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;

		protected override void InitializeComponent()
		{
			this.CodeDescriptionListEditControl = new Enterprise.Core.Forms.CodeDescriptionListEditControl();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 370, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// CodeDescriptionListEditControl
			// 
			this.CodeDescriptionListEditControl.BackColor = System.Drawing.SystemColors.Control;
			this.CodeDescriptionListEditControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.CodeDescriptionListEditControl.Name = "CodeDescriptionListEditControl";
			this.CodeDescriptionListEditControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 320, true);
			this.CodeDescriptionListEditControl.TabIndex = 0;
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CodeDescriptionListEditForm|a6a8c2a8-f370-46fe-8722-eca88814243a", "&Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 344, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CodeDescriptionListEditForm|da814867-ec7e-4152-90d8-0660589e334a", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 344, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CodeDescriptionListEditForm
			// 
			this.AcceptButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 394, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CodeDescriptionListEditForm|fba485bc-b18a-422a-95fe-e10c7aa23302", "Code Description List Editor");
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.CodeDescriptionListEditControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CodeDescriptionListEditForm";
			this.Load += new System.EventHandler(this.CodeDescriptionListEditForm_Load);
			this.Controls.SetChildIndex(this.CodeDescriptionListEditControl, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
