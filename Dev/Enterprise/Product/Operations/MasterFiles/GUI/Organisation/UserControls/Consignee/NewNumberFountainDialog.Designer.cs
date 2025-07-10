using System.Globalization;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class NewNumberFountainDialog : ZChildForm
	{
		#region Designer generated code

		internal Enterprise.ZArchitecture.ZLabel CurrentSequenceNumberLabel;
		internal Enterprise.ZArchitecture.ZCalcEdit NewSequenceNumberCalcEdit;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		System.ComponentModel.IContainer components = null;
		Enterprise.ZArchitecture.GUI.ZButton OKButton;

		protected override void InitializeComponent()
		{
			this.NewSequenceNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CurrentSequenceNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 98, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// NewSequenceNumberCalcEdit
			// 
			this.NewSequenceNumberCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NewNumberFountainDialog|6f68de24-91d7-4044-b37d-a9995b881d1f", "Enter New Sequence Number");
			this.NewSequenceNumberCalcEdit.Decimals = 0;
			this.NewSequenceNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 40, true);
			this.NewSequenceNumberCalcEdit.Name = "NewSequenceNumberCalcEdit";
			this.NewSequenceNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NewSequenceNumberCalcEdit.TabIndex = 1;
			this.NewSequenceNumberCalcEdit.Text = "0";
			this.NewSequenceNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NewSequenceNumberCalcEdit.MaxValue = int.MaxValue;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NewNumberFountainDialog|8096f3bd-c758-435c-b97b-4faa3a34c444", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 72, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NewNumberFountainDialog|4f99f379-b16c-4414-aa79-770399c207b4", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 72, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CurrentSequenceNumberLabel
			// 
			this.CurrentSequenceNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.CurrentSequenceNumberLabel.Name = "CurrentSequenceNumberLabel";
			this.CurrentSequenceNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 23, true);
			this.CurrentSequenceNumberLabel.TabIndex = 0;
			// 
			// NewNumberFountainDialog
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 122, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("NewNumberFountainDialog|88e23bc1-70a7-43da-9f88-2c6a3db00aa0", "Enter New Sequence Number");
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.CurrentSequenceNumberLabel);
			this.Controls.Add(this.NewSequenceNumberCalcEdit);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "NewNumberFountainDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.NewSequenceNumberCalcEdit, 0);
			this.Controls.SetChildIndex(this.CurrentSequenceNumberLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
