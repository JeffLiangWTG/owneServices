namespace Enterprise.Customs.US.ISF.GUI
{
	partial class MessagesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagesBoundGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.VerticalSplitter = new CargoWise.Windows.UI.KSplitter();
			this.MessageTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InterpretedMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesBoundGrid)).BeginInit();
			this.MessageTabControl.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ISF.Business.CusISFHeader);
			// 
			// MessagesBoundGrid
			// 
			this.MessagesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesBoundGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_ApplicationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
			this.MessagesBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.ISF.GUI.Res.GetData("be7c58b7-9372-4277-b7cf-88b45be1153c", "Cus. Ref.", "Customs Reference", "");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo2.Caption = "Message No.";
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			zTextBoxColumnStyleInfo3.Caption = "Type";
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo4.Caption = "Sub Type";
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108);
			zTextBoxColumnStyleInfo5.Caption = "Interchange No.";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			zDateEditColumnStyleInfo3.Caption = "Interchange Time";
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo6.Caption = "Sender";
			zTextBoxColumnStyleInfo6.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo7.Caption = "Status";
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo8.Caption = "Direction";
			zTextBoxColumnStyleInfo8.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(59);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesBoundGrid.CopySelectedRowsAllowed = true;
			this.MessagesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesBoundGrid.GridId = "b1ea2bd8-3bfb-4ae7-a58f-b5ce37be9be0";
			this.MessagesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesBoundGrid.LayoutKey = "MessagesBoundGrid";
			this.MessagesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesBoundGrid.Name = "MessagesBoundGrid";
			this.MessagesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 488, true);
			this.MessagesBoundGrid.TabIndex = 2;
			// 
			// VerticalSplitter
			// 
			this.VerticalSplitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.VerticalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 0, true);
			this.VerticalSplitter.Name = "VerticalSplitter";
			this.VerticalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 488, true);
			this.VerticalSplitter.TabIndex = 3;
			this.VerticalSplitter.TabStop = false;
			// 
			// MessageTabControl
			// 
			this.MessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessageTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageTabControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.MessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 0, true);
			this.MessageTabControl.Name = "MessageTabControl";
			this.MessageTabControl.SelectedIndex = 0;
			this.MessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 488, true);
			this.MessageTabControl.TabIndex = 4;
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.InterpretedMessageTextBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 461, true);
			this.MessageDetailsTabPage.TabIndex = 0;
			this.MessageDetailsTabPage.Text = "Message Details";
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.InterpretedMessageTextBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.InterpretedMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InterpretedMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedMessageTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InterpretedMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InterpretedMessageTextBox.Multiline = true;
			this.InterpretedMessageTextBox.Name = "InterpretedMessageTextBox";
			this.InterpretedMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 455, true);
			this.InterpretedMessageTextBox.TabIndex = 0;
			this.InterpretedMessageTextBox.WordWrap = false;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 461, true);
			this.MessageTextTabPage.TabIndex = 1;
			this.MessageTextTabPage.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_MessageTextDetail");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextTextBox, false);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.ISF.Business.CusISFHeader)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 455, true);
			this.MessageTextTextBox.TabIndex = 0;
			this.MessageTextTextBox.WordWrap = false;
			// 
			// MessagesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesBoundGrid);
			this.Controls.Add(this.VerticalSplitter);
			this.Controls.Add(this.MessageTabControl);
			this.Name = "MessagesUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 488, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesBoundGrid)).EndInit();
			this.MessageTabControl.ResumeLayout(false);
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		public Enterprise.Messaging.GUI.MessageZGrid MessagesBoundGrid;
		protected CargoWise.Windows.UI.KSplitter VerticalSplitter;
		protected Enterprise.ZArchitecture.GUI.ZTabControl MessageTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessageDetailsTabPage;
		protected Enterprise.ZArchitecture.ZTextBox InterpretedMessageTextBox;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessageTextTabPage;
		protected Enterprise.ZArchitecture.ZTextBox MessageTextTextBox;
	}
}
