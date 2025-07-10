using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ShipmentReceivalForm : ZTemplateForm
	{
		#region Windows Form Designer generated code
		new void InitializeComponent()
		{
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.ArrivalTabPage = new ZTabPage();
			this.MainTabControl.SuspendLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ArrivalTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = ControlDpiScalingHelper.NewScaledSize(1008, 533, true);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ArrivalTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = ControlDpiScalingHelper.NewScaledSize(968, 390, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(1008, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(993);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CFSShipment);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = ControlDpiScalingHelper.NewScaledSize(968, 390, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// ArrivalTabPage
			// 
			this.ArrivalTabPage.CaptionResourceString = Res.GetData("ShipmentReceivalForm|ca6a64a0-0391-43e3-9bff-6a475cb93b5e", "Arrival");
			this.ArrivalTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ArrivalTabPage.Name = "ArrivalTabPage";
			this.ArrivalTabPage.Size = ControlDpiScalingHelper.NewScaledSize(1000, 506, true);
			this.ArrivalTabPage.TabIndex = 4;
			// 
			// ShipmentReceivalForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(1008, 589, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.CFS.Business";
			this.DataSourceType = typeof(CFSShipment);
			this.DataSourceTypeName = "Enterprise.Freight.CFS.Business.CFSShipment";
			this.MinimumSize = ControlDpiScalingHelper.NewScaledSize(1016, 625, true);
			this.Name = "ShipmentReceivalForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "ShipmentForm";
			this.MainTabControl.ResumeLayout(false);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
