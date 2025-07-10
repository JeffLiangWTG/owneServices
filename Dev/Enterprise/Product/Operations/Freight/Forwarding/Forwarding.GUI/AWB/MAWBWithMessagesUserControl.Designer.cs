using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class MAWBWithMessagesUserControl : ZUserControl
	{
		private ZTemplateTabControl MainTabControl;
		private ZTabPage MessagesTabPage;
		public MAWBTabPage MAWBTabPage;
		private SecurityDeclarationUserControl SecurityDeclarationControl;
		private ZPanel MainPanel;
		private ZPanel LeftPanel;
		private ZPanel RightPanel;
		private ZGrid EDIMessageGrid;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZTextBox MessageTextTextBox;
		private ZTextBox CurrentStatusTextBox;
		private ZTabPage SecurityDeclarationTabPage;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			this.MainTabControl = new ZTemplateTabControl();
			this.MAWBTabPage = new MAWBTabPage();
			this.MessagesTabPage = new ZTabPage();
			this.MainPanel = new ZPanel();
			this.RightPanel = new ZPanel();
			this.MessageTextTextBox = new ZTextBox();
			this.LeftPanel = new ZPanel();
			this.CurrentStatusTextBox = new ZTextBox();
			this.EDIMessageGrid = new ZGrid();
			this.SecurityDeclarationTabPage = new ZTabPage();
			this.SecurityDeclarationControl = new SecurityDeclarationUserControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EDIMessageGrid)).BeginInit();
			this.EDIMessageGrid.SuspendLayout();
			this.SecurityDeclarationTabPage.SuspendLayout();
			this.SecurityDeclarationControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ForwardingConsol);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.MAWBTabPage);
			this.MainTabControl.Controls.Add(this.SecurityDeclarationTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 464, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MAWBTabPage
			// 
			this.MAWBTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBWithMessagesUserControl|d513de90-bf69-4cd7-898e-066242fd786e", "Details");
			this.MAWBTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MAWBTabPage.Name = "MAWBTabPage";
			this.MAWBTabPage.ParentAWBTabPage = null;
			this.MAWBTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 437, true);
			this.MAWBTabPage.TabIndex = 0;
			this.MAWBTabPage.Text = "AWB";
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("MAWBWithMessagesUserControl|c1b28028-b9f9-4279-aa3a-e874455a293a", "Messages");
			this.MessagesTabPage.Controls.Add(this.MainPanel);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 437, true);
			this.MessagesTabPage.TabIndex = 1;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.RightPanel);
			this.MainPanel.Controls.Add(this.LeftPanel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 437, true);
			this.MainPanel.TabIndex = 0;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.MessageTextTextBox);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 0, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 437, true);
			this.RightPanel.TabIndex = 1;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "CIMEDIMessages.EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Forwarding.AWB.Business.CIMEDIMessage)(((System.Collections.IList)(((ForwardingConsol)(null)).CIMEDIMessages)).SyncRoot)).EM_MessageText)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 437, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.CurrentStatusTextBox);
			this.LeftPanel.Controls.Add(this.EDIMessageGrid);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 437, true);
			this.LeftPanel.TabIndex = 0;
			// 
			// CurrentStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrentStatusTextBox, "AWBCurrentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ForwardingConsol)(null)).AWBCurrentStatus)));
			this.CurrentStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 10, true);
			this.CurrentStatusTextBox.Name = "CurrentStatusTextBox";
			this.CurrentStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 20, true);
			this.CurrentStatusTextBox.TabIndex = 1;
			// 
			// EDIMessageGrid
			// 
			this.EDIMessageGrid.AllowNavigation = false;
			this.EDIMessageGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EDIMessageGrid, "CIMEDIMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ForwardingConsol)(null)).CIMEDIMessages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Forwarding.AWB.Business.CIMEDIMessage)(((System.Collections.IList)(((ForwardingConsol)(null)).CIMEDIMessages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Forwarding.AWB.Business.CIMEDIMessage)(((System.Collections.IList)(((ForwardingConsol)(null)).CIMEDIMessages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Forwarding.AWB.Business.CIMEDIMessage)(((System.Collections.IList)(((ForwardingConsol)(null)).CIMEDIMessages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Forwarding.AWB.Business.CIMEDIMessage)(((System.Collections.IList)(((ForwardingConsol)(null)).CIMEDIMessages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Forwarding.AWB.Business.CIMEDIMessage)(((System.Collections.IList)(((ForwardingConsol)(null)).CIMEDIMessages)).SyncRoot)).EM_StatusDateTime)));
			this.EDIMessageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.ColumnName = "EM_StatusDateTime";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EDIMessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EDIMessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EDIMessageGrid.GridId = "f6d2e688-62c9-43c7-a64a-bdff17d4cf56";
			this.EDIMessageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EDIMessageGrid.LayoutKey = "EDIMessageGrid";
			this.EDIMessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.EDIMessageGrid.Name = "EDIMessageGrid";
			this.EDIMessageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 388, true);
			this.EDIMessageGrid.TabIndex = 0;
			// 
			// SecurityDeclarationTabPage
			// 
			this.SecurityDeclarationTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d32d9354-a89d-48b4-8de8-c54746af5ce4", "Security Declaration");
			this.SecurityDeclarationTabPage.Controls.Add(this.SecurityDeclarationControl);
			this.SecurityDeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SecurityDeclarationTabPage.Name = "SecurityDeclarationTabPage";
			this.SecurityDeclarationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SecurityDeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 437, true);
			this.SecurityDeclarationTabPage.TabIndex = 2;
			this.SecurityDeclarationTabPage.UseVisualStyleBackColor = true;
			// 
			// SecurityDeclarationControl
			// 
			this.SecurityDeclarationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityDeclarationControl, ".");
			this.SecurityDeclarationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecurityDeclarationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SecurityDeclarationControl.Name = "SecurityDeclarationControl";
			this.SecurityDeclarationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 431, true);
			this.SecurityDeclarationControl.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 3, true);
			this.splitter1.TabIndex = 0;
			this.splitter1.TabStop = false;
			// 
			// MAWBWithMessagesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Name = "MAWBWithMessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EDIMessageGrid)).EndInit();
			this.EDIMessageGrid.ResumeLayout(false);
			this.EDIMessageGrid.PerformLayout();
			this.SecurityDeclarationTabPage.ResumeLayout(false);
			this.SecurityDeclarationTabPage.PerformLayout();
			this.SecurityDeclarationTabPage.ResumeLayout(false);
			this.SecurityDeclarationControl.ResumeLayout(true);
			this.SecurityDeclarationControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.IContainer components;
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
