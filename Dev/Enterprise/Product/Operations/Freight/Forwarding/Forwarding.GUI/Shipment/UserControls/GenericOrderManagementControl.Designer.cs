using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class GenericOrderManagementControl
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
			this.GenericOrderLinks = new Enterprise.Freight.Forwarding.Orders.GUI.AttachGenericOrdersUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			GenericOrderLinks.SuspendLayout();
			// 
			// GenericOrderLinks
			// 
			this.GenericOrderLinks.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GenericOrderLinks, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.Orders.Business.IAttachGenericOrders)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)))));
			this.GenericOrderLinks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GenericOrderLinks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GenericOrderLinks.Name = "GenericOrderLinks";
			this.GenericOrderLinks.ShowEditButton = false;
			this.GenericOrderLinks.TabIndex = 40;
			GenericOrderLinks.ResumeLayout(false);
			GenericOrderLinks.PerformLayout();
			this.SuspendLayout();
			// 
			// GenericOrderManagementControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GenericOrderLinks);
			this.Name = "GenericOrderManagementControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 308, true);
			this.ResumeLayout(false);
			this.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();

		}

		#endregion

		private Enterprise.Freight.Forwarding.Orders.GUI.AttachGenericOrdersUserControl GenericOrderLinks;
	}
}
