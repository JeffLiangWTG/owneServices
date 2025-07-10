using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class MessageSendingObjectForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.GUI.ZButton SendButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelButton2;
		protected ZGroupBox messageSendingObjectsGroupBox;
		protected ZArchitecture.ZGrid MessageSendingObjectsGrid;
		private System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageSendingObjectsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageSendingObjectsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 288, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseMessageSendingObjectParent);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 259, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8261C539-FBA1-437F-9685-5C72791A8085", "&Send");
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 259, true);
			this.CancelButton2.Name = "CancelButton2";
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.CancelButton2.TabIndex = 3;
			this.CancelButton2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0E0836E8-1AE8-4BF2-A93F-7182D8498F33", "&Cancel");
			this.CancelButton2.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.messageSendingObjectsGroupBox.CaptionResourceString = MessageSendingObjectsGroupBoxCaption;
			this.messageSendingObjectsGroupBox.Controls.Add(this.MessageSendingObjectsGrid);
			this.messageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.messageSendingObjectsGroupBox.Name = "messageSendingObjectsGroupBox";
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 244, true);
			this.messageSendingObjectsGroupBox.TabIndex = 1;
			this.messageSendingObjectsGroupBox.TabStop = false;
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageSendingObjectsGrid, "SendingObjectsCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.MessageSendingObjectsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSend";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.MessageSendingObjectsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MessageSendingObjectsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSendingObjectsGrid.GridId = "52bafb3e-e070-4614-95ce-272d80314fce";
			this.MessageSendingObjectsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageSendingObjectsGrid.LayoutKey = "MessageSendingObjectsGrid";
			this.MessageSendingObjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageSendingObjectsGrid.Name = "MessageSendingObjectsGrid";
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 225, true);
			this.MessageSendingObjectsGrid.TabIndex = 1;
			// 
			// MessageSendingObjectForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 311, true);
			this.Controls.Add(this.messageSendingObjectsGroupBox);
			this.Controls.Add(this.CancelButton2);
			this.Controls.Add(this.SendButton);
			this.DataSourceType = typeof(Enterprise.Customs.Business.BaseMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 350, true);
			this.Name = "MessageSendingObjectForm";
			this.Text = "";
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("271A0FB6-25A9-462B-9FEB-4EFF56EF0282", "Customized Message Box Form");
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
