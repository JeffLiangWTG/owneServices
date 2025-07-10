
namespace Enterprise.Customs.US.GUI
{
	partial class OrganisationPlugInUserControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ISEINNumberVerifiedDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsEINNumberVerifiedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImporterTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ImporterTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zPreviousNextControl1 = new Enterprise.ZArchitecture.GUI.ZPreviousNextControl();
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusesErrorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesStatusErrorsUserControl = new Enterprise.Customs.US.GUI.MessagesStatusErrorsUserControl();
			this.StatusesErrorsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.StatusesErrorsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 79, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 418, true);
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessagesTabControl.Controls.Add(this.StatusesErrorsTabPage);
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 418, true);
			this.MessagesTabControl.Controls.SetChildIndex(this.StatusesErrorsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageDetailsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 391, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 385, true);
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 418, true);
			// 
			// MessagesGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Message Type";
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageType";
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 399, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.OrgHeaderWrapper);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.ISEINNumberVerifiedDropEdit);
			this.zPanel1.Controls.Add(this.IsEINNumberVerifiedLabel);
			this.zPanel1.Controls.Add(this.ImporterTypeDropEdit);
			this.zPanel1.Controls.Add(this.ImporterTypeLabel);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 79, true);
			this.zPanel1.TabIndex = 0;
			// 
			// ISEINNumberVerifiedDropEdit
			// 
			this.ISEINNumberVerifiedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ISEINNumberVerifiedDropEdit, "ZO_IsEINNumberVerifiedIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.OrgHeaderWrapper)(null)).ZO_IsEINNumberVerifiedIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.OrgHeaderWrapper)(null)).ImportAddInfoLookups.ZO_YesNoList)));
			this.ISEINNumberVerifiedDropEdit.BindToList = "ImportAddInfoLookups+ZO_YesNoList";
			this.ISEINNumberVerifiedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 45, true);
			this.ISEINNumberVerifiedDropEdit.Name = "ISEINNumberVerifiedDropEdit";
			this.ISEINNumberVerifiedDropEdit.PreBoundMaxLength = 1;
			this.ISEINNumberVerifiedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 20, true);
			this.ISEINNumberVerifiedDropEdit.TabIndex = 3;
			// 
			// IsEINNumberVerifiedLabel
			// 
			this.IsEINNumberVerifiedLabel.AutoSize = true;
			this.IsEINNumberVerifiedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 49, true);
			this.IsEINNumberVerifiedLabel.Name = "IsEINNumberVerifiedLabel";
			this.IsEINNumberVerifiedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 13, true);
			this.IsEINNumberVerifiedLabel.TabIndex = 2;
			this.IsEINNumberVerifiedLabel.Text = "Is EIN/CBP Number Verified:";
			// 
			// ImporterTypeDropEdit
			// 
			this.ImporterTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterTypeDropEdit, "ZO_ImporterType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.OrgHeaderWrapper)(null)).ZO_ImporterType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.OrgHeaderWrapper)(null)).ImportAddInfoLookups.ZO_ImporterTypeList)));
			this.ImporterTypeDropEdit.BindToList = "ImportAddInfoLookups+ZO_ImporterTypeList";
			this.ImporterTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 13, true);
			this.ImporterTypeDropEdit.Name = "ImporterTypeDropEdit";
			this.ImporterTypeDropEdit.PreBoundMaxLength = 1;
			this.ImporterTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ImporterTypeDropEdit.TabIndex = 1;
			// 
			// ImporterTypeLabel
			// 
			this.ImporterTypeLabel.AutoSize = true;
			this.ImporterTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 16, true);
			this.ImporterTypeLabel.Name = "ImporterTypeLabel";
			this.ImporterTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.ImporterTypeLabel.TabIndex = 0;
			this.ImporterTypeLabel.Text = "Importer Type:";
			// 
			// zPreviousNextControl1
			// 
			this.zPreviousNextControl1.AllowDrop = true;
			this.zPreviousNextControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 271, true);
			this.zPreviousNextControl1.Name = "zPreviousNextControl1";
			this.zPreviousNextControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 26, true);
			this.zPreviousNextControl1.TabIndex = 1;
			this.zPreviousNextControl1.TabStop = true;
			this.zPreviousNextControl1.Visible = false;
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.MessageDetailsTextBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.MessageDetailsTabPage.TabIndex = 1;
			this.MessageDetailsTabPage.Text = "Message Details";
			// 
			// MessageDetailsTextBox
			// 
			this.MessageDetailsTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageDetailsTextBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.MessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageDetailsTextBox.Multiline = true;
			this.MessageDetailsTextBox.Name = "MessageDetailsTextBox";
			this.MessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 511, true);
			this.MessageDetailsTextBox.TabIndex = 2;
			this.MessageDetailsTextBox.WordWrap = false;
			// 
			// StatusesErrorsTabPage
			// 
			this.StatusesErrorsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("69da48b6-85e9-488d-9a18-8a3cf015a8a2", "Status/Errors");
			this.StatusesErrorsTabPage.Controls.Add(this.StatusesErrorsLabel);
			this.StatusesErrorsTabPage.Controls.Add(this.messagesStatusErrorsUserControl);
			this.StatusesErrorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusesErrorsTabPage.Name = "StatusesErrorsTabPage";
			this.StatusesErrorsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StatusesErrorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 391, true);
			this.StatusesErrorsTabPage.TabIndex = 2;
			this.StatusesErrorsTabPage.Text = "Status/Errors";
			this.StatusesErrorsTabPage.UseVisualStyleBackColor = true;
			// 
			// messagesStatusErrorsUserControl
			// 
			this.messagesStatusErrorsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesStatusErrorsUserControl, "Messages.StatusesAndErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.StatusErrorsDataViewCollection)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).StatusesAndErrors)));
			this.messagesStatusErrorsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesStatusErrorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messagesStatusErrorsUserControl.Name = "messagesStatusErrorsUserControl";
			this.messagesStatusErrorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 385, true);
			this.messagesStatusErrorsUserControl.TabIndex = 21;
			// 
			// StatusesErrorsLabel
			// 
			this.BindingSource.SetBindingMember(this.StatusesErrorsLabel, "Messages.StatusesErrorsExist");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.OrgHeaderWrapper)(null)).Messages)).SyncRoot)).StatusesErrorsExist)));
			this.StatusesErrorsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusesErrorsLabel.ForeColor = System.Drawing.Color.Black;
			this.StatusesErrorsLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusesErrorsLabel, false);
			this.StatusesErrorsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StatusesErrorsLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 0, true);
			this.StatusesErrorsLabel.Name = "StatusesErrorsLabel";
			this.StatusesErrorsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 385, true);
			this.StatusesErrorsLabel.TabIndex = 22;
			this.StatusesErrorsLabel.VisibleChanged += new System.EventHandler(this.StatusesErrorsLabel_VisibleChanged);
			// 
			// OrganisationPlugInUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zPreviousNextControl1);
			this.Controls.Add(this.zPanel1);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 0, true);
			this.Name = "OrganisationPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 497, true);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.zPreviousNextControl1, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.MainPanel.ResumeLayout(false);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.StatusesErrorsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.ZLabel ImporterTypeLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ImporterTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPreviousNextControl zPreviousNextControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessageDetailsTabPage;
		private Enterprise.ZArchitecture.ZTextBox MessageDetailsTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ISEINNumberVerifiedDropEdit;
		private Enterprise.ZArchitecture.ZLabel IsEINNumberVerifiedLabel;
		private ZArchitecture.GUI.ZTabPage StatusesErrorsTabPage;
		internal MessagesStatusErrorsUserControl messagesStatusErrorsUserControl;
		public ZArchitecture.ZLabel StatusesErrorsLabel;
	}
}
