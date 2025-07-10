namespace Enterprise.MasterFiles.GUI
{
	public partial class EmailMultipleContactsForm
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EmailToContactUserControl1 = new Enterprise.MasterFiles.GUI.EmailToContactUserControl();
			this.ContactListPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContactListGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SendAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContactListPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContactListGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 611, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 24, true);
			this.MainStatusBar.TabIndex = 16;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.MultipleEmailToContactSender);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailMultipleContactsForm|d11d7c1b-b671-4d4a-8128-3cf3fe4233ca", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(824, 585, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// EmailToContactUserControl1
			// 
			this.EmailToContactUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EmailToContactUserControl1, "EmailsToContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MultipleEmailToContactSender)(null)).EmailsToContacts)).SyncRoot)))));
			this.EmailToContactUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 2, true);
			this.EmailToContactUserControl1.Name = "EmailToContactUserControl1";
			this.EmailToContactUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 579, true);
			this.EmailToContactUserControl1.TabIndex = 1;
			// 
			// ContactListPanel
			// 
			this.ContactListPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.ContactListPanel.Controls.Add(this.ContactListGrid);
			this.ContactListPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.ContactListPanel.Name = "ContactListPanel";
			this.ContactListPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 602, true);
			this.ContactListPanel.TabIndex = 0;
			// 
			// ContactListGrid
			// 
			this.ContactListGrid.AllowNavigation = false;
			this.ContactListGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ContactListGrid, "EmailsToContacts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MultipleEmailToContactSender)(null)).EmailsToContacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EmailToContactBusinessObject)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MultipleEmailToContactSender)(null)).EmailsToContacts)).SyncRoot)).ToDisplayName)));
			this.ContactListGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailMultipleContactsForm|bf736127-5026-41d5-867d-05dce6b75369", "Applicant");
			zTextBoxColumnStyleInfo1.ColumnName = "ToDisplayName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ContactListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContactListGrid.GridId = "b7cee43c-6c80-463c-91ee-e8133d7f67a4";
			this.ContactListGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContactListGrid.LayoutKey = "ContactListGrid";
			this.ContactListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ContactListGrid.Name = "ContactListGrid";
			this.ContactListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 588, true);
			this.ContactListGrid.TabIndex = 0;
			// 
			// SendAllButton
			// 
			this.SendAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendAllButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailMultipleContactsForm|f816281a-10fc-47ce-ae42-dbe696b5d41f", "Send All");
			this.SendAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(744, 585, true);
			this.SendAllButton.Name = "SendAllButton";
			this.SendAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.SendAllButton.TabIndex = 2;
			this.SendAllButton.Click += new System.EventHandler(this.SendAllButton_Click);
			// 
			// PreviewButton
			// 
			this.PreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailMultipleContactsForm|c636f048-bfe7-4c74-bf24-e779c0076ca2", "Preview");
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 585, true);
			this.PreviewButton.Name = "PreviewButton";
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.PreviewButton.TabIndex = 2;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// EmailMultipleContactsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 635, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("EmailMultipleContactsForm|b68af328-f483-4e0e-9dcf-cadc1154cd22", "Email");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.PreviewButton);
			this.Controls.Add(this.SendAllButton);
			this.Controls.Add(this.ContactListPanel);
			this.Controls.Add(this.EmailToContactUserControl1);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.MultipleEmailToContactSender);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 656, true);
			this.Name = "EmailMultipleContactsForm";
			this.Controls.SetChildIndex(this.EmailToContactUserControl1, 0);
			this.Controls.SetChildIndex(this.ContactListPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendAllButton, 0);
			this.Controls.SetChildIndex(this.PreviewButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContactListPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContactListGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		internal protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected EmailToContactUserControl EmailToContactUserControl1;
		protected Enterprise.ZArchitecture.GUI.ZPanel ContactListPanel;
		internal protected Enterprise.ZArchitecture.ZGrid ContactListGrid;
		internal protected Enterprise.ZArchitecture.GUI.ZButton PreviewButton;
		internal protected Enterprise.ZArchitecture.GUI.ZButton SendAllButton;
	}
}
