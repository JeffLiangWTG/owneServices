
namespace Enterprise.Customs.US.GUI
{
	partial class AESMessageSubmitForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		public new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SEDContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SEDGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SEDDetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SubmitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SEDContainer)).BeginInit();
			this.SEDContainer.Panel1.SuspendLayout();
			this.SEDContainer.Panel2.SuspendLayout();
			this.SEDContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SEDGrid)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 358, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.AESDeclaration);
			// 
			// SEDContainer
			// 
			this.SEDContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SEDContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SEDContainer.Name = "SEDContainer";
			this.SEDContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SEDContainer.Panel1
			// 
			this.SEDContainer.Panel1.Controls.Add(this.SEDGrid);
			// 
			// SEDContainer.Panel2
			// 
			this.SEDContainer.Panel2.Controls.Add(this.SEDDetailTextBox);
			this.SEDContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 321, true);
			this.SEDContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			this.SEDContainer.TabIndex = 1;
			// 
			// SEDGrid
			// 
			this.SEDGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SEDGrid, "Entries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).US_ShouldBeReportToCustoms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).CH_BGMReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).US_XTN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).EntryHeaderStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).US_SendWithdrawn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).US_SendReplace)));
			this.SEDGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = "Send";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = null;
			zCheckBoxColumnStyleInfo1.ColumnName = "US_ShouldBeReportToCustoms";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.Caption = "Shipment No.";
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CH_BGMReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "ITN";
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(99);
			zTextBoxColumnStyleInfo3.Caption = "XTN";
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_XTN";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			zTextBoxColumnStyleInfo4.Caption = "SED Status";
			zTextBoxColumnStyleInfo4.CaptionResourceString = null;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo4.ColumnName = "EntryHeaderStatusDescription";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			zTextBoxColumnStyleInfo5.Caption = "Message Status";
			zTextBoxColumnStyleInfo5.CaptionResourceString = null;
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo5.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			zCheckBoxColumnStyleInfo2.Caption = "Send As Withdrawn";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = null;
			zCheckBoxColumnStyleInfo2.ColumnName = "US_SendWithdrawn";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zCheckBoxColumnStyleInfo3.Caption = "Replace Entry from another System";
			zCheckBoxColumnStyleInfo3.CaptionResourceString = null;
			zCheckBoxColumnStyleInfo3.ColumnName = "US_SendReplace";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.SEDGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SEDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SEDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SEDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SEDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SEDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SEDGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.SEDGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.SEDGrid.CopySelectedRowsAllowed = true;
			this.SEDGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SEDGrid.GridId = "0ab45fd2-818f-45b2-a83a-6fe65e13b072";
			this.SEDGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SEDGrid.LayoutKey = "SEDGrid";
			this.SEDGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SEDGrid.Name = "SEDGrid";
			this.SEDGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 114, true);
			this.SEDGrid.TabIndex = 0;
			// 
			// SEDDetailTextBox
			// 
			this.BindingSource.SetBindingMember(this.SEDDetailTextBox, "Entries.SEDString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.AESDeclaration)(null)).Entries)).SyncRoot)).SEDString)));
			this.SEDDetailTextBox.CaptionResourceString = null;
			this.SEDDetailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SEDDetailTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SEDDetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SEDDetailTextBox.Multiline = true;
			this.SEDDetailTextBox.Name = "SEDDetailTextBox";
			this.SEDDetailTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.SEDDetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 203, true);
			this.SEDDetailTextBox.TabIndex = 0;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.CloseButton);
			this.ButtonsPanel.Controls.Add(this.SubmitButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 321, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 37, true);
			this.ButtonsPanel.TabIndex = 2;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = null;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 8, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.Text = "&Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SubmitButton
			// 
			this.SubmitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SubmitButton.CaptionResourceString = null;
			this.SubmitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 8, true);
			this.SubmitButton.Name = "SubmitButton";
			this.SubmitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SubmitButton.TabIndex = 0;
			this.SubmitButton.Text = "&Submit";
			this.SubmitButton.UseVisualStyleBackColor = true;
			this.SubmitButton.Click += new System.EventHandler(this.SubmitButton_Click);
			// 
			// AESMessageSubmitForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 382, true);
			this.Controls.Add(this.SEDContainer);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.AESDeclaration);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.JobDeclaration";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 409, true);
			this.Name = "AESMessageSubmitForm";
			this.Text = "AES Direct Messages";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.SEDContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SEDContainer.Panel1.ResumeLayout(false);
			this.SEDContainer.Panel2.ResumeLayout(false);
			this.SEDContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SEDContainer)).EndInit();
			this.SEDContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SEDGrid)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SEDContainer;
		private Enterprise.ZArchitecture.ZGrid SEDGrid;
		private Enterprise.ZArchitecture.ZTextBox SEDDetailTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel ButtonsPanel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SubmitButton;

	}
}
