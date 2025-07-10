using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class CFSLoadListConsolForm : ZTemplateForm
	{
		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ArrivalDispatchTabPage = new ZTabPage();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ArrivalDispatchTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = ControlDpiScalingHelper.NewScaledSize(1014, 611, true);
			this.MainTabControl.TabIndex = 1;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ArrivalDispatchTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = ControlDpiScalingHelper.NewScaledSize(1006, 563, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(1006, 584, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = ControlDpiScalingHelper.NewScaledSize(1014, 611, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(1014, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(499);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CFSLoadListConsol);
			// 
			// ArrivalDispatchTabPage
			// 
			this.ArrivalDispatchTabPage.CaptionResourceString = Res.GetData("CFSLoadListConsolForm|01344263-61dc-4b3b-957f-0b87bbb33e4a", "Arrival/Dispatch", "The Arrival/Dispatch tab.");
			this.ArrivalDispatchTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ArrivalDispatchTabPage.Name = "ArrivalDispatchTabPage";
			this.ArrivalDispatchTabPage.Padding = ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ArrivalDispatchTabPage.Size = ControlDpiScalingHelper.NewScaledSize(1006, 563, true);
			this.ArrivalDispatchTabPage.TabIndex = 3;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = ControlDpiScalingHelper.NewScaledSize(974, 465, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// CFSLoadListConsolForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(1014, 667, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.CFS.Business";
			this.DataSourceType = typeof(CFSLoadListConsol);
			this.DataSourceTypeName = "Enterprise.Freight.CFS.Business.CFSLoadListConsol";
			this.MinimumSize = ControlDpiScalingHelper.NewScaledSize(1022, 706, true);
			this.Name = "CFSLoadListConsolForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
