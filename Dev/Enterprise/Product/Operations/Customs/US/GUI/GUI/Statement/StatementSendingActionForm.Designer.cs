

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	partial class StatementSendingActionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntriesToSendMessagesForGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntriesToSendMessagesForGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 361, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1058, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.StatementDeleteAndSendingAction);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(971, 325, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 5;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(890, 325, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// EntriesToSendMessagesForGroupBox
			// 
			this.EntriesToSendMessagesForGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.EntriesToSendMessagesForGroupBox.Controls.Add(this.EntriesGrid);
			this.EntriesToSendMessagesForGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.EntriesToSendMessagesForGroupBox.Name = "EntriesToSendMessagesForGroupBox";
			this.EntriesToSendMessagesForGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1031, 307, true);
			this.EntriesToSendMessagesForGroupBox.TabIndex = 0;
			this.EntriesToSendMessagesForGroupBox.TabStop = false;
			this.EntriesToSendMessagesForGroupBox.Text = "Entries";
			// 
			// EntriesGrid
			// 
			this.EntriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntriesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).PortOfEntry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).US_EntryFilerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).US_EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).US_SendMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).US_PaymentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).PaymentTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).US_PreliminaryStatementPrintDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).US_ClientBranchDesignation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).US_PeriodicStatementMonth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.StatementDeleteAndSendingAction)(null)).MonthList)));
			this.EntriesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Port Of Entry";
			zTextBoxColumnStyleInfo1.ColumnName = "PortOfEntry";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo2.Caption = "Entry Filer Code";
			zTextBoxColumnStyleInfo2.ColumnName = "US_EntryFilerCode";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo3.Caption = "Entry Number";
			zTextBoxColumnStyleInfo3.ColumnName = "US_EntryNumber";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCheckBoxColumnStyleInfo1.Caption = "Send Message";
			zCheckBoxColumnStyleInfo1.ColumnName = "US_SendMessage";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDropEditColumnStyleInfo1.BindToList = "PaymentTypeList";
			zDropEditColumnStyleInfo1.Caption = "Payment Type";
			zDropEditColumnStyleInfo1.ColumnName = "US_PaymentType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDateEditColumnStyleInfo1.Caption = "Preliminary Statement Print Date";
			zDateEditColumnStyleInfo1.ColumnName = "US_PreliminaryStatementPrintDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			zTextBoxColumnStyleInfo4.Caption = "Client Branch Designation";
			zTextBoxColumnStyleInfo4.ColumnName = "US_ClientBranchDesignation";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152);
			zDropEditColumnStyleInfo2.BindToList = "MonthList";
			zDropEditColumnStyleInfo2.Caption = "Periodic Statement Month";
			zDropEditColumnStyleInfo2.ColumnName = "US_PeriodicStatementMonth";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntriesGrid.GridId = "4ed7a6e6-26cd-4b10-b3f0-12a2e0c9df88";
			this.EntriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntriesGrid.LayoutKey = "EntriesGrid";
			this.EntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntriesGrid.Name = "EntriesGrid";
			this.EntriesGrid.PreferredColumnWidth = 90;
			this.EntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1025, 288, true);
			this.EntriesGrid.TabIndex = 0;
			// 
			// StatementSendingActionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1058, 385, true);
			this.Controls.Add(this.EntriesToSendMessagesForGroupBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.StatementDeleteAndSendingAction);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.StatementDeleteAndSendingAction";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 419, true);
			this.Name = "StatementSendingActionForm";
			this.Text = "StatementSendingActionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.EntriesToSendMessagesForGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntriesToSendMessagesForGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.ZGrid EntriesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EntriesToSendMessagesForGroupBox;
	}
}
