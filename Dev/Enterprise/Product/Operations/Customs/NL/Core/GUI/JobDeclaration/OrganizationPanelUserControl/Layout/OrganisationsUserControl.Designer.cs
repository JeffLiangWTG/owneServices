
namespace Enterprise.Customs.NL.GUI
{
	partial class OrganisationsUserControl
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
			this.IntracomReceiverAddressControl = new ZArchitecture.GUI.ZAddressControl();
			this.ExporterDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DefermentPartyDocAddressControl = new MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IntracomReceiverAddressControl.SuspendLayout();
			this.ExporterDocAddressControl.SuspendLayout();
			this.DefermentPartyDocAddressControl.SuspendLayout();
			this.SuspendLayout();

			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			//
			// IntracomReceiverAddressControl
			//
			this.IntracomReceiverAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntracomReceiverAddressControl, "JE_OA_Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).JE_OA_Representative)));
			this.IntracomReceiverAddressControl.BindToOrgList = "Lookups.IntracomReceiverList";
			this.IntracomReceiverAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 26, true);
			this.IntracomReceiverAddressControl.Name = "IntracomReceiverAddressControl";
			this.IntracomReceiverAddressControl.PopupCaption = "";
			this.IntracomReceiverAddressControl.ReadOnly = false;
			this.IntracomReceiverAddressControl.ShowAddress = false;
			this.IntracomReceiverAddressControl.ShowOrganisationName = true;
			this.IntracomReceiverAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			//
			// DefermentPartyAddressControl
			//
			this.DefermentPartyDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefermentPartyDocAddressControl, "DefermentPartyDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DefermentPartyDocAddress)));
			this.DefermentPartyDocAddressControl.BindToOrganisations = "Lookups+DefermentPartyCollection";
			this.DefermentPartyDocAddressControl.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("8F52302F-8E5F-4AF6-9CE4-60EC9EEAFCFF", "Deferment Party");
			this.DefermentPartyDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DefermentPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 3, true);
			this.DefermentPartyDocAddressControl.Name = "DefermentPartyDocAddressControl";
			this.DefermentPartyDocAddressControl.ReadOnly = false;
			this.DefermentPartyDocAddressControl.SingleLineNoGroupBoxPanelWidth = 350;
			this.DefermentPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.DefermentPartyDocAddressControl.ValidationJustForced = false;
			this.DefermentPartyDocAddressControl.ShowCompanyName = true;
			// 
			// ExporterDocAddressControl
			// 
			this.ExporterDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterDocAddressControl, "ExporterDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ExporterDocAddress)));
			this.ExporterDocAddressControl.BindToOrganisations = "Lookups+SuppliersList";
			this.ExporterDocAddressControl.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("21E3A1F4-1022-4A38-8B9C-4C632E23015B", "Exporter");
			this.ExporterDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.ExporterDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 29, true);
			this.ExporterDocAddressControl.Name = "ExporterDocAddressControl";
			this.ExporterDocAddressControl.ReadOnly = false;
			this.ExporterDocAddressControl.SingleLineNoGroupBoxPanelWidth = 350;
			this.ExporterDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.ExporterDocAddressControl.ValidationJustForced = false;
			this.ExporterDocAddressControl.ShowCompanyName = true;
			//
			// OrganisationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IntracomReceiverAddressControl);
			this.Controls.Add(this.ExporterDocAddressControl);
			this.Controls.Add(this.DefermentPartyDocAddressControl);
			this.Name = "OrganisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 385, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IntracomReceiverAddressControl.ResumeLayout(true);
			this.IntracomReceiverAddressControl.PerformLayout();
			this.ExporterDocAddressControl.ResumeLayout(true);
			this.ExporterDocAddressControl.PerformLayout();
			this.DefermentPartyDocAddressControl.ResumeLayout(true);
			this.DefermentPartyDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal ZArchitecture.GUI.ZAddressControl IntracomReceiverAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ExporterDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl DefermentPartyDocAddressControl;
	}
}
