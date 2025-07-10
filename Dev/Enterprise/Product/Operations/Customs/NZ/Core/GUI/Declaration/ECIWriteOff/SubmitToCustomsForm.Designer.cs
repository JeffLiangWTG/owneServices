using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff
{
	public partial class SubmitToCustomsForm : Base.SubmitToCustomsForm
	{
		protected ZArchitecture.GUI.ZCheckBox queueForManifestingCheckBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.queueForManifestingCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.RemarksGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// CancelBtn
			// 
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 214, true);
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.CancelBtn.TabIndex = 4;
			// 
			// OkButton
			// 
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 214, true);
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.OkButton.TabIndex = 3;
			// 
			// SelectDefaultRemarksButton
			// 
			this.SelectDefaultRemarksButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 145, true);
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 120, true);
			// 
			// RemarksGroupBox
			// 
			this.RemarksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 174, true);
			// 
			// MessageTypeLabel
			// 
			this.MessageTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 22, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 244, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 25, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// QueueForManifestingCheckBox
			// 
			this.queueForManifestingCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
			this.queueForManifestingCheckBox.AutoSize = true;
			this.queueForManifestingCheckBox.BindTo = "QueueForManifesting";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).QueueForManifesting);
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((MessageManager)(null)).QueueForManifestingInfo);
			this.queueForManifestingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.queueForManifestingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 217, true);
			this.queueForManifestingCheckBox.Name = "QueueForManifestingCheckBox";
			this.queueForManifestingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
			this.queueForManifestingCheckBox.TabIndex = 2;
			this.queueForManifestingCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("2fe0641f-67a3-4235-8d3d-db5284937fac", "Queue for &Manifesting");
			// 
			// SubmitToCustomsForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 269, true);
			this.Controls.Add(this.queueForManifestingCheckBox);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.MessageManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 209, true);
			this.Name = "SubmitToCustomsForm";
			this.Controls.SetChildIndex(this.MessageTypeLabel, 0);
			this.Controls.SetChildIndex(this.queueForManifestingCheckBox, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.RemarksGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.RemarksGroupBox.ResumeLayout(false);
			this.RemarksGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

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
	}
}
