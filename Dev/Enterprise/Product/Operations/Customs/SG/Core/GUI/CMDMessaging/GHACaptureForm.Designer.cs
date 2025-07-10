using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI.CMDMessaging
{
	public partial class GHACaptureForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		#region Auto

		private ZArchitecture.ZTextBox submittedToTextBox;
		private ZButton oKBoundButton;
		private ZButton cancelBoundButton;
		private ZDropEdit gHADropEdit;
		private readonly System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private new void InitializeComponent()
		{
			this.submittedToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.oKBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.gHADropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.gHADropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.CMDMessaging.GHACapture);
			// 
			// submittedToTextBox
			// 
			this.BindingSource.SetBindingMember(this.submittedToTextBox, "SubmittedTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.GHACapture)(null)).SubmittedTo)));
			this.submittedToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 8, true);
			this.submittedToTextBox.Name = "submittedToTextBox";
			this.submittedToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.submittedToTextBox.TabIndex = 1;
			// 
			// oKBoundButton
			// 
			this.oKBoundButton.IsCaptionOverridden = true;
			this.oKBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 64, true);
			this.oKBoundButton.Name = "oKBoundButton";
			this.oKBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.oKBoundButton.TabIndex = 3;
			this.oKBoundButton.Text = Enterprise.Customs.SG.V4.GUI.Res.GetString("CMDMessaging|GHACaptureForm|0B73FC35-7CAF-4D09-B82C-3705FB7A9AD3", "OK");
			this.oKBoundButton.ToolTipCaption = null;
			this.oKBoundButton.Click += new System.EventHandler(this.OKBoundButton_Click);
			// 
			// cancelBoundButton
			// 
			this.cancelBoundButton.IsCaptionOverridden = true;
			this.cancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 64, true);
			this.cancelBoundButton.Name = "cancelBoundButton";
			this.cancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelBoundButton.TabIndex = 4;
			this.cancelBoundButton.Text = Enterprise.Customs.SG.V4.GUI.Res.GetString("CMDMessaging|GHACaptureForm|349164C7-8DFE-4298-A459-891AD77B0E2D", "Cancel");
			this.cancelBoundButton.ToolTipCaption = null;
			this.cancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// gHADropEdit
			// 
			this.gHADropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.gHADropEdit, "GHA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.CMDMessaging.GHACapture)(null)).GHA)));
			this.gHADropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 32, true);
			this.gHADropEdit.Name = "gHADropEdit";
			this.gHADropEdit.PreBoundMaxLength = 4;
			this.gHADropEdit.ShowDescriptionBox = false;
			this.gHADropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.gHADropEdit.TabIndex = 2;
			// 
			// GHACaptureForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 144, true);
			this.ControlBox = false;
			this.Controls.Add(this.gHADropEdit);
			this.Controls.Add(this.oKBoundButton);
			this.Controls.Add(this.cancelBoundButton);
			this.Controls.Add(this.submittedToTextBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.SG.V4.Business";
			this.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.CMDMessaging.GHACapture);
			this.DataSourceTypeName = "Enterprise.Customs.SG.V4.Business.CMDMessaging.GHACapture";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 160, true);
			this.Name = "GHACaptureForm";
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.submittedToTextBox, 0);
			this.Controls.SetChildIndex(this.cancelBoundButton, 0);
			this.Controls.SetChildIndex(this.oKBoundButton, 0);
			this.Controls.SetChildIndex(this.gHADropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.gHADropEdit.ResumeLayout(true);
			this.gHADropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		#endregion
	}
}
