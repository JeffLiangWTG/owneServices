namespace Enterprise.Customs.GUI
{
	partial class ImportMessageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.BaseMessageUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.EntriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntriesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopVerticalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MainHorizontalSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryLinesMessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLineGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExtendedInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
			this.EntriesBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
			this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
			this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
			this.MainHorizontalSplitContainer.SuspendLayout();
			this.EntryLinesMessagesTabControl.SuspendLayout();
			this.MessageTabPage.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
			this.EntryLineGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.AllowDrop = true;
			this.BaseMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BaseMessageUserControl.Name = "BaseMessageUserControl";
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
			this.BaseMessageUserControl.TabIndex = 0;
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F637C8C5-ED41-4EEF-B3FE-70FC46DBDD34", "Entries");
			this.EntriesGroupBox.Controls.Add(this.EntriesBoundGrid);
			this.EntriesGroupBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.EntriesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntriesGroupBox.Name = "EntriesGroupBox";
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 65, true);
			this.EntriesGroupBox.TabIndex = 0;
			this.EntriesGroupBox.TabStop = false;
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntriesBoundGrid, "CustomsEntryHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_BGMReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_MessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).PackagesCount)));
			this.EntriesBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "CH_BGMReference";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("FBDBD45E-C229-4ABB-800F-18C1BEC4E17E", "Message Type");
			zTextBoxColumnStyleInfo3.ColumnName = "CH_MessageType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B5575601-CD0F-40C4-9EB4-0B2A0675B286", "Message Type Desc.", "Message Type Description");
			zTextBoxColumnStyleInfo4.ColumnName = "CH_MessageTypeDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("435FC87E-FB2C-41AD-86F0-DBEF3C8A6451", "No. Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "PackagesCount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntriesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntriesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntriesBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntriesBoundGrid.GridId = "52a32545-ab95-4cce-b905-3eebb51483c0";
			this.EntriesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntriesBoundGrid.LayoutKey = "EntriesBoundGrid";
			this.EntriesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntriesBoundGrid.Name = "EntriesBoundGrid";
			this.EntriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 46, true);
			this.EntriesBoundGrid.TabIndex = 0;
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopVerticalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopVerticalSplitContainer.Name = "TopVerticalSplitContainer";
			// 
			// TopVerticalSplitContainer.Panel1
			// 
			this.TopVerticalSplitContainer.Panel1.Controls.Add(this.EntriesGroupBox);
			this.TopVerticalSplitContainer.Panel2Collapsed = true;
			this.TopVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 65, true);
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(569);
			this.TopVerticalSplitContainer.SplitterWidth = 3;
			this.TopVerticalSplitContainer.TabIndex = 1;
			this.TopVerticalSplitContainer.TabStop = false;
			// 
			// MainHorizontalSplitContainer
			// 
			this.MainHorizontalSplitContainer.Cursor = System.Windows.Forms.Cursors.Default;
			this.MainHorizontalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainHorizontalSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainHorizontalSplitContainer.Name = "MainHorizontalSplitContainer";
			this.MainHorizontalSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainHorizontalSplitContainer.Panel1
			// 
			this.MainHorizontalSplitContainer.Panel1.Controls.Add(this.TopVerticalSplitContainer);
			// 
			// MainHorizontalSplitContainer.Panel2
			// 
			this.MainHorizontalSplitContainer.Panel2.Controls.Add(this.EntryLinesMessagesTabControl);
			this.MainHorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 551, true);
			this.MainHorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(65);
			this.MainHorizontalSplitContainer.SplitterWidth = 3;
			this.MainHorizontalSplitContainer.TabIndex = 1;
			this.MainHorizontalSplitContainer.TabStop = false;
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.EntryLinesMessagesTabControl.Controls.Add(this.MessageTabPage);
			this.EntryLinesMessagesTabControl.Controls.Add(this.EntryLinesTabPage);
			this.EntryLinesMessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesMessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLinesMessagesTabControl.Name = "EntryLinesMessagesTabControl";
			this.EntryLinesMessagesTabControl.SelectedIndex = 0;
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 483, true);
			this.EntryLinesMessagesTabControl.TabIndex = 0;
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B0B4828F-5169-4862-AEF6-79BFC03C2A64", "Messages");
			this.MessageTabPage.Controls.Add(this.BaseMessageUserControl);
			this.MessageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTabPage.Name = "MessageTabPage";
			this.MessageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
			this.MessageTabPage.TabIndex = 0;
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0A4DD99F-AE5F-45C7-9FE4-1EF5A4D31461", "Entry Lines");
			this.EntryLinesTabPage.Controls.Add(this.EntryLineGrid);
			this.EntryLinesTabPage.Controls.Add(this.ExtendedInfoGroupBox);
			this.EntryLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryLinesTabPage.Name = "EntryLinesTabPage";
			this.EntryLinesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
			this.EntryLinesTabPage.TabIndex = 1;
			// 
			// EntryLineGrid
			// 
			this.EntryLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineGrid, "CustomsEntryHeaders.AllEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_LineNumber)));
			this.EntryLineGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CL_LineNumber";
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineGrid.GridId = "099c68b3-05cb-4214-a3f5-ab3f2a9a8832";
			this.EntryLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineGrid.LayoutKey = "zGrid1";
			this.EntryLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLineGrid.Name = "EntryLineGrid";
			this.EntryLineGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 333, true);
			this.EntryLineGrid.TabIndex = 0;
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("E98F3A53-2EFB-4540-BE7D-0B4DE948B495", "Extended Information");
			this.ExtendedInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 336, true);
			this.ExtendedInfoGroupBox.Name = "ExtendedInfoGroupBox";
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 117, true);
			this.ExtendedInfoGroupBox.TabIndex = 1;
			this.ExtendedInfoGroupBox.TabStop = false;
			this.ExtendedInfoGroupBox.Visible = false;
			// 
			// ImportMessageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainHorizontalSplitContainer);
			this.Name = "ImportMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 551, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
			this.EntriesBoundGrid.ResumeLayout(false);
			this.EntriesBoundGrid.PerformLayout();
			this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
			this.MainHorizontalSplitContainer.ResumeLayout(false);
			this.MainHorizontalSplitContainer.PerformLayout();
			this.EntryLinesMessagesTabControl.ResumeLayout(false);
			this.EntryLinesMessagesTabControl.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
			this.EntryLineGrid.ResumeLayout(false);
			this.EntryLineGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox EntriesGroupBox;
		public Enterprise.ZArchitecture.ZGrid EntriesBoundGrid;
		protected CargoWise.Windows.UI.KSplitContainer MainHorizontalSplitContainer;
		protected Enterprise.ZArchitecture.GUI.ZTabControl EntryLinesMessagesTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage EntryLinesTabPage;
		protected Enterprise.ZArchitecture.ZGrid EntryLineGrid;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ExtendedInfoGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessageTabPage;
		protected CargoWise.Windows.UI.KSplitContainer TopVerticalSplitContainer;
		public Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl BaseMessageUserControl;
	}
}
