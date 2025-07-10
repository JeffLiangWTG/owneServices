using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ShipmentGatePassForm : ZForm, ISaveAndPrintUI
	{
		#region Designer generated code

		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZTemplateTabControl GatePassTabControl;
		ZTabPage GatePassTabPage;
		ZLogsTabPage EventTabPage;
		ZStmNoteTabPage StmNoteTabPage;
		ZWorkflowTabPage WorkflowTabPage;
		System.ComponentModel.IContainer components = null;

		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.GatePassTabControl = new ZTemplateTabControl();
			this.GatePassTabPage = new ZTabPage();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.StmNoteTabPage = new ZStmNoteTabPage();
			this.EventTabPage = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GatePassTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 615, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1001);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GatePassShipment);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 585, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// GatePassTabControl
			// 
			this.GatePassTabControl.Controls.Add(this.GatePassTabPage);
			this.GatePassTabControl.Controls.Add(this.WorkflowTabPage);
			this.GatePassTabControl.Controls.Add(this.StmNoteTabPage);
			this.GatePassTabControl.Controls.Add(this.EventTabPage);
			this.GatePassTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GatePassTabControl.Name = "GatePassTabControl";
			this.GatePassTabControl.SelectedIndex = 0;
			this.GatePassTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 579, true);
			this.GatePassTabControl.TabIndex = 0;
			// 
			// GatePassTabPage
			// 
			this.GatePassTabPage.CaptionResourceString = Res.GetData("ShipmentGatePassForm|798def4c-d66a-421e-92c9-14099419a597", "Gate Pass");
			this.GatePassTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GatePassTabPage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GatePassTabPage.Name = "GatePassTabPage";
			this.GatePassTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 552, true);
			this.GatePassTabPage.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 591, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StmNoteTabPage.Name = "StmNoteTabPage";
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 591, true);
			this.StmNoteTabPage.TabIndex = 2;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 591, true);
			this.EventTabPage.TabIndex = 1;
			// 
			// ShipmentGatePassForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 639, true);
			this.Controls.Add(this.GatePassTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Freight.CFS.Business";
			this.DataSourceType = typeof(GatePassShipment);
			this.DataSourceTypeName = "Enterprise.Freight.CFS.Business.GatePassShipment";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 675, true);
			this.Name = "ShipmentGatePassForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.GatePassTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GatePassTabControl.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
