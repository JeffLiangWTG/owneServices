namespace Enterprise.Customs.TW.GUI
{
	partial class LicensingMessageHeaderUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LicensingMessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LicensingMessageTabUserControl = new Enterprise.Customs.TW.GUI.LicensingMessageTabUserControl();
			this.ControllingMessageHeaderGrid = new Enterprise.ZArchitecture.ZGrid();
			this.Splitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.LicensingMessagesTabControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.LicensingMessageTabUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ControllingMessageHeaderGrid)).BeginInit();
			this.ControllingMessageHeaderGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusTWControllingMessageHeader);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.LicensingMessagesTabControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 190, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 200, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 300, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// LicensingMessagesTabControl
			// 
			this.LicensingMessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LicensingMessagesTabControl.Controls.Add(this.MessagesTabPage);
			this.LicensingMessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingMessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicensingMessagesTabControl.Name = "LicensingMessagesTabControl";
			this.LicensingMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 300, true);
			this.LicensingMessagesTabControl.TabIndex = 1;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("7B66BAFA-7231-4A0F-A467-311CD1256744", "Messages");
			this.MessagesTabPage.Controls.Add(this.LicensingMessageTabUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 273, true);
			this.MessagesTabPage.TabIndex = 2;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// LicensingMessageTabUserControl
			// 
			this.LicensingMessageTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicensingMessageTabUserControl, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).Messages)));
			this.LicensingMessageTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensingMessageTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicensingMessageTabUserControl.Name = "LicensingMessageTabUserControl";
			this.LicensingMessageTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1192, 273, true);
			this.LicensingMessageTabUserControl.TabIndex = 0;
			// 
			// ControllingMessageHeaderGrid
			// 
			this.ControllingMessageHeaderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ControllingMessageHeaderGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).TW1_ControllingMessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).ControllingMessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).TW1_FunctionalReferenceId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).TW1_CertificateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).CertificateTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).TW1_BusinessType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).BusinessTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).TW1_EntryStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).LicensingStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).TW1_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusTWControllingMessageHeader)(null)).LicensingMessageStatusDescription)));
			this.ControllingMessageHeaderGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TW1_ControllingMessageType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(76);
			zTextBoxColumnStyleInfo1.ColumnName = "ControllingMessageTypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zTextBoxColumnStyleInfo2.ColumnName = "TW1_FunctionalReferenceId";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zDropEditColumnStyleInfo2.ColumnName = "TW1_CertificateType";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo3.ColumnName = "CertificateTypeDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zDropEditColumnStyleInfo3.ColumnName = "TW1_BusinessType";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo4.ColumnName = "BusinessTypeDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zTextBoxColumnStyleInfo5.ColumnName = "TW1_EntryStatus";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("13687722-a94e-4a2a-9f1b-6ce2799c1d60", "Licensing Status");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo6.ColumnName = "LicensingStatusDescription";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("13687722-a94e-4a2a-9f1b-6ce2799c1d60", "Licensing Status");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "TW1_MessageStatus";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("52044731-d82f-4af2-9938-9bfb37a77dc0", "Licensing Message Status");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo8.ColumnName = "LicensingMessageStatusDescription";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("52044731-d82f-4af2-9938-9bfb37a77dc0", "Licensing Message Status");
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ControllingMessageHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ControllingMessageHeaderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ControllingMessageHeaderGrid.GridId = "462d3921-b104-4b32-8753-8319bb544307";
			this.ControllingMessageHeaderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ControllingMessageHeaderGrid.LayoutKey = "ControllingMessageHeaderGrid_LayoutKey";
			this.ControllingMessageHeaderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ControllingMessageHeaderGrid.Name = "ControllingMessageHeaderGrid";
			this.ControllingMessageHeaderGrid.ReadOnly = true;
			this.ControllingMessageHeaderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 187, true);
			this.ControllingMessageHeaderGrid.TabIndex = 1;
			// 
			// Splitter
			// 
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.DoNotSaveSplitterLayout = false;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 187, true);
			this.Splitter.Name = "Splitter";
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 3, true);
			this.Splitter.TabIndex = 2;
			this.Splitter.TabStop = false;
			// 
			// LicensingMessageHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ControllingMessageHeaderGrid);
			this.Controls.Add(this.Splitter);
			this.Controls.Add(this.BottomPanel);
			this.Name = "LicensingMessageHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 490, true);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.Splitter, 0);
			this.Controls.SetChildIndex(this.ControllingMessageHeaderGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.LicensingMessagesTabControl.ResumeLayout(false);
			this.LicensingMessagesTabControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.LicensingMessageTabUserControl.ResumeLayout(true);
			this.LicensingMessageTabUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ControllingMessageHeaderGrid)).EndInit();
			this.ControllingMessageHeaderGrid.ResumeLayout(false);
			this.ControllingMessageHeaderGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ControllingMessageHeaderGrid;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private CargoWise.Windows.UI.KSplitter Splitter;
		private ZArchitecture.GUI.ZTabControl LicensingMessagesTabControl;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private LicensingMessageTabUserControl LicensingMessageTabUserControl;
	}
}
