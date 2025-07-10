using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class ClientControl : ZUserControl
	{
		#region Component Designer generated code

		private System.ComponentModel.IContainer components = null;
		internal Enterprise.MasterFiles.GUI.ZDocAddressControl ClientDocAddresssControl;
		internal Enterprise.ZArchitecture.ZLabel JobHeaderClientCoveringLabel;
		internal Enterprise.MasterFiles.GUI.ZOrgAddressControl ClientOrgControl;

		private void InitializeComponent()
		{
			this.ClientDocAddresssControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ClientOrgControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.JobHeaderClientCoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// ClientDocAddresssControl
			// 
			this.BindingSource.SetBindingMember(this.ClientDocAddresssControl, "ClientDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ClientDocAddress)));
			this.ClientDocAddresssControl.BindToOrganisations = "Clients";
			this.ClientDocAddresssControl.CaptionResourceString = Res.GetData("ClientControl|1E040726-A4D0-4d55-962F-BBE3E53DA8BC", "Client/Recipient");
			this.ClientDocAddresssControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientDocAddresssControl.Name = "ClientDocAddresssControl";
			this.ClientDocAddresssControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ClientDocAddresssControl.TabIndex = 1;
			// 
			// ClientOrgControl
			// 
			this.BindingSource.SetBindingMember(this.ClientOrgControl, "ClientAddrPK_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).ClientAddrPK_ZAddress)));
			this.ClientOrgControl.CaptionResourceString = Res.GetData("ClientControl|1E040726-A4D0-4d55-962F-BBE3E53DA8BC", "Client/Recipient");
			this.ClientOrgControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 188, true);
			this.ClientOrgControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.ClientOrgControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.ClientOrgControl.Name = "ClientOrgControl";
			this.ClientOrgControl.PopupCaption = "";
			this.ClientOrgControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.ClientOrgControl.TabIndex = 0;
			// 
			// JobHeaderClientCoveringLabel
			// 
			this.JobHeaderClientCoveringLabel.CaptionResourceString = Res.GetData("ClientControl|788b77c9-4b70-476c-9a54-86b8aee108aa", "Job Header Lock Error Label");
			this.JobHeaderClientCoveringLabel.ForeColor = System.Drawing.Color.Red;
			this.JobHeaderClientCoveringLabel.IsFontBold = true;
			this.JobHeaderClientCoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobHeaderClientCoveringLabel.Name = "JobHeaderClientCoveringLabel";
			this.JobHeaderClientCoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.JobHeaderClientCoveringLabel.TabIndex = 7;
			this.JobHeaderClientCoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.JobHeaderClientCoveringLabel.Visible = false;
			// 
			// ClientControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobHeaderClientCoveringLabel);
			this.Controls.Add(this.ClientDocAddresssControl);
			this.Controls.Add(this.ClientOrgControl);
			this.Name = "ClientControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
