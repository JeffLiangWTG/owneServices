using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using DataTransfer.Common.GUI.MenuItems;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI.AWB;
using Enterprise.Freight.Forwarding.GUI.Consol;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Common.TemplateRecords;
using Enterprise.Freight.Integration;
using Enterprise.Integration.SystemToSystemTrust;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using WTG.IdentitySecurity;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ConsolForm : ZTemplateForm, ICarrierContractAssignableJobForm, INotifications
	{
		protected internal ConsolUserControl ConsolControl;
		public ZTabPage ContainersTabPage;
		internal ConsolContainerUserControl ConsolContainerControl;
		protected ZTabPage AWBTabPage;
		ZPanel AWBPanel;
		MAWBWithMessagesUserControl MAWBWithMessages;
		ZWorkflowTabPage WorkflowTabPage;
		ZTabPage ElectronicMessagingTabPage;
		protected ZTabControl ElectronicMessagingTabControl;
		protected ZTabPage AccountingTabPage;
		protected ZTabControl AccountingTabControl;

		new void InitializeComponent()
		{
			this.components = new Container();
			this.ContainersTabPage = new ZTabPage();
			this.ConsolContainerControl = new ConsolContainerUserControl();
			this.AWBTabPage = new ZTabPage();
			this.AWBPanel = new ZPanel();
			this.MAWBWithMessages = new MAWBWithMessagesUserControl();
			this.ConsolControl = new ConsolUserControl();
			this.WorkflowTabPage = new ForwardingConsolWorkflowTabPage();
			this.ElectronicMessagingTabPage = new ZTabPage();
			this.AccountingTabPage = new ZTabPage();
			this.AccountingTabControl = new ZTabControl();
			this.ElectronicMessagingTabControl = new ZTabControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainersTabPage.SuspendLayout();
			this.AWBTabPage.SuspendLayout();
			this.AWBPanel.SuspendLayout();
			this.ElectronicMessagingTabPage.SuspendLayout();
			this.AccountingTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.ContainersTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.AWBTabPage);
			this.MainTabControl.Controls.Add(this.ElectronicMessagingTabPage);
			this.MainTabControl.Controls.Add(this.AccountingTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 633, true);
			this.MainTabControl.SelectedIndexChanging += new EventHandler(this.MainTabControl_SelectedIndexChanging);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AccountingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ElectronicMessagingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AWBTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			//
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.ConsolControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 606, true);
			//
			// LogsTabPage
			//
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 606, true);
			//
			// MainPanel
			//
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 633, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 24, true);
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1085);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(ForwardingConsol);
			//
			// ContainersTabPage
			//
			this.ContainersTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolForm|2523fbee-2e55-4bda-9c16-f555f30430e2", "Containers");
			this.ContainersTabPage.Controls.Add(this.ConsolContainerControl);
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 606, true);
			this.ContainersTabPage.TabIndex = 1;
			//
			// ConsolContainerControl
			//
			this.ConsolContainerControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolContainerControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Business.ForwardingContainer)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)))));
			this.ConsolContainerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolContainerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolContainerControl.Name = "ConsolContainerControl";
			this.ConsolContainerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 606, true);
			this.ConsolContainerControl.TabIndex = 0;
			//
			// AWBTabPage
			//
			this.AWBTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolForm|3c72e251-6388-4643-bb1b-2f15c8eed346", "AWB");
			this.AWBTabPage.Controls.Add(this.AWBPanel);
			this.AWBTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AWBTabPage.Name = "AWBTabPage";
			this.AWBTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 606, true);
			this.AWBTabPage.TabIndex = 5;
			//
			// AWBPanel
			//
			this.AWBPanel.Controls.Add(this.MAWBWithMessages);
			this.AWBPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AWBPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBPanel.Name = "AWBPanel";
			this.AWBPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 606, true);
			this.AWBPanel.TabIndex = 93;
			//
			// MAWBWithMessages
			//
			this.MAWBWithMessages.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MAWBWithMessages, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingConsol)(((ForwardingConsol)(null)))));
			this.MAWBWithMessages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MAWBWithMessages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MAWBWithMessages.Name = "MAWBWithMessages";
			this.MAWBWithMessages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 606, true);
			this.MAWBWithMessages.TabIndex = 0;
			//
			// ConsolControl
			//
			this.ConsolControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingConsol)(((ForwardingConsol)(null)))));
			this.ConsolControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolControl.Name = "ConsolControl";
			this.ConsolControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 606, true);
			this.ConsolControl.TabIndex = 0;
			//
			// WorkflowTabPage
			//
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolForm|06e9cde6-735f-45e4-af46-40a7ab05a791", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 489, true);
			this.WorkflowTabPage.TabIndex = 7;
			//
			// ElectronicMessagingTabPage
			//
			this.ElectronicMessagingTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ElectronicMessagingTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolForm|aff443a3-4f4e-4b0c-9f16-37ac1f7249c1", "Electronic Messaging");
			this.ElectronicMessagingTabPage.Controls.Add(this.ElectronicMessagingTabControl);
			this.ElectronicMessagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ElectronicMessagingTabPage.Name = "ElectronicMessagingTabPage";
			this.ElectronicMessagingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ElectronicMessagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 606, true);
			this.ElectronicMessagingTabPage.TabIndex = 9;
			//
			// AccountingTabPage
			//
			this.AccountingTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.AccountingTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ConsolForm|3674969E-EBE0-4a70-86A4-DD4356FD3A30", "Accounting");
			this.AccountingTabPage.Controls.Add(this.AccountingTabControl);
			this.AccountingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AccountingTabPage.Name = "AccountingTabPage";
			this.AccountingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AccountingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 606, true);
			this.AccountingTabPage.TabIndex = 10;
			//
			// AccountingTabControl
			//
			this.AccountingTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AccountingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AccountingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AccountingTabControl.Name = "AccountingTabControl";
			this.AccountingTabControl.SelectedIndex = 0;
			this.AccountingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1086, 600, true);
			this.AccountingTabControl.TabIndex = 0;
			//
			// ElectronicMessagingTabControl
			//
			this.ElectronicMessagingTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ElectronicMessagingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ElectronicMessagingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ElectronicMessagingTabControl.Name = "ElectronicMessagingTabControl";
			this.ElectronicMessagingTabControl.SelectedIndex = 0;
			this.ElectronicMessagingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 600, true);
			this.ElectronicMessagingTabControl.TabIndex = 0;
			//
			// ConsolForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 689, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(ForwardingConsol);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ForwardingConsol";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 725, true);
			this.Name = "ConsolForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "ConsolForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();

			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.AWBTabPage.ResumeLayout(false);
			this.AWBTabPage.PerformLayout();
			this.AWBPanel.ResumeLayout(false);
			this.AWBPanel.PerformLayout();
			this.ElectronicMessagingTabPage.ResumeLayout(false);
			this.ElectronicMessagingTabPage.PerformLayout();
			this.AccountingTabPage.ResumeLayout(false);
			this.AccountingTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
