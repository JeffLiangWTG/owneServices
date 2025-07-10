using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CustomsBrokerageUserControl
	{

		void InitializeComponent()
		{
			this.StatusTabPage = new ZTabPage();
			this.statusUserControl = new StatusUserControl();
			this.WHSPacksTabPage = new Customs.GUI.BaseDeclarationTabPage();
			this.MainTabControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusTabPage.SuspendLayout();
			this.statusUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WHSPacksTabPage);
			this.MainTabControl.Controls.Add(this.StatusTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 607, true);
			this.MainTabControl.Controls.SetChildIndex(this.EventTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.StatusTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MiscOptionsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WHSPacksTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceLinesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoicesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceGroupingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DeliveryTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PickupTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PackingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContainerTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DeclarationTabPage, 0);
			// 
			// InvoicesTabPage
			// 
			this.InvoicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// DeclarationTabPage
			// 
			this.DeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// PackingTabPage
			// 
			this.PackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// InvoiceLinesTabPage
			// 
			this.InvoiceLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// InvoiceGroupingTabPage
			// 
			this.InvoiceGroupingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// MiscOptionsTabPage
			// 
			this.MiscOptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// ContainerTabPage
			// 
			this.ContainerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// EventTabPage
			// 
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// StatusTabPage
			// 
			this.StatusTabPage.Controls.Add(this.statusUserControl);
			this.StatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusTabPage.Name = "StatusTabPage";
			this.StatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			this.StatusTabPage.TabIndex = 11;
			this.StatusTabPage.Text = "Status";
			// 
			// statusUserControl
			// 
			this.statusUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(((BaseJobDeclaration)(null)))));
			this.statusUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.statusUserControl.Name = "statusUserControl";
			this.statusUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 574, true);
			this.statusUserControl.TabIndex = 0;
			// 
			// WHSPacksTabPage
			// 
			this.WHSPacksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WHSPacksTabPage.Name = "WHSPacksTabPage";
			this.WHSPacksTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WHSPacksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629, true);
			this.WHSPacksTabPage.TabIndex = 12;
			this.WHSPacksTabPage.Text = "WHS Packs";
			this.WHSPacksTabPage.UseVisualStyleBackColor = true;
			// 
			// CustomsBrokerageUserControl
			// 
			this.Name = "CustomsBrokerageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 655, true);
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusTabPage.ResumeLayout(false);
			this.StatusTabPage.PerformLayout();
			this.statusUserControl.ResumeLayout(true);
			this.statusUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public ZTabPage StatusTabPage;
		private StatusUserControl statusUserControl;
	}
}
