using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USDrawbackCustomsBrokerageUserControl
	{
		void InitializeComponent()
		{
			this.StatusTabPage = new ZTabPage();
			this.statusUserControl = new DrawbackStatusUserControl();
			this.MainTabControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusTabPage.SuspendLayout();
			this.statusUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.StatusTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 607, true);
			this.MainTabControl.Controls.SetChildIndex(this.EventTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.StatusTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MiscOptionsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceLinesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoicesTabPage, 0);
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
			// InvoiceLinesTabPage
			// 
			this.InvoiceLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
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
			// MiscOptionsTabPage
			// 
			this.MiscOptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1011, 580, true);
			// 
			// StatusTabPage
			// 
			this.StatusTabPage.Controls.Add(this.statusUserControl);
			this.StatusTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.StatusTabPage.Name = "StatusTabPage";
			this.StatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 585, true);
			this.StatusTabPage.TabIndex = 12;
			this.StatusTabPage.Text = "Status";
			this.StatusTabPage.UseVisualStyleBackColor = true;
			// 
			// statusUserControl
			// 
			this.statusUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusUserControl, ".");
			this.statusUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.statusUserControl.Name = "statusUserControl";
			this.statusUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 579, true);
			this.statusUserControl.TabIndex = 0;
			// 
			// USDrawbackCustomsBrokerageUserControl
			// 
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 607, true);
			this.Name = "USDrawbackCustomsBrokerageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 607, true);
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

		internal ZTabPage StatusTabPage;
		private DrawbackStatusUserControl statusUserControl;
	}
}
