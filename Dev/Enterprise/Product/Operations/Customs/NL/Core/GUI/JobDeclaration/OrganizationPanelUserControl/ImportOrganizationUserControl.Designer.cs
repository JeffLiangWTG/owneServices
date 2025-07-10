namespace Enterprise.Customs.NL.GUI
{
	partial class ImportOrganizationUserControl
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
			this.IntracomReceiverControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.BuyerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.SellerAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			this.DefermentPartyDocAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IntracomReceiverControl.SuspendLayout();
			this.BuyerAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 48, true);
			this.SellerAddressControl.TabIndex = 3;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 92, true);
			this.ManufacturerAddressControl.TabIndex = 4;
			// 
			// DeclarantOfficeAddressControl
			// 
			this.DeclarantOfficeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 3, true);
			// 
			// DefermentPartyDocAddressControl
			// 
			this.DefermentPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 114, true);
			this.DefermentPartyDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 20, true);
			this.DefermentPartyDocAddressControl.ShowCompanyName = true;
			this.DefermentPartyDocAddressControl.TabIndex = 5;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// IntracomReceiverControl
			// 
			this.IntracomReceiverControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntracomReceiverControl, "JE_OA_Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).JE_OA_Representative)));
			this.IntracomReceiverControl.BindToOrgList = "Lookups.IntracomReceiverList";
			this.IntracomReceiverControl.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("C591507C-5415-4834-9C13-08A57B721C33", "Fiscal rep");
			this.IntracomReceiverControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 26, true);
			this.IntracomReceiverControl.Name = "IntracomReceiverControl";
			this.IntracomReceiverControl.PopupCaption = "";
			this.IntracomReceiverControl.ReadOnly = false;
			this.IntracomReceiverControl.ShowAddress = false;
			this.IntracomReceiverControl.ShowOrganisationName = true;
			this.IntracomReceiverControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.IntracomReceiverControl.TabIndex = 2;
			// 
			// BuyerAddressControl
			// 
			this.BuyerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerAddressControl, "JE_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).JE_OA_ConsigneeAddress)));
			this.BuyerAddressControl.BindToOrgList = "Lookups+ConsigneeList";
			this.BuyerAddressControl.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("f1661b3b-8931-4c0f-a32b-76af8863d9c9", "Buyer");
			this.BuyerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 70, true);
			this.BuyerAddressControl.Name = "BuyerAddressControl";
			this.BuyerAddressControl.PopupCaption = "";
			this.BuyerAddressControl.ReadOnly = false;
			this.BuyerAddressControl.ShowAddress = false;
			this.BuyerAddressControl.ShowOrganisationName = true;
			this.BuyerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.BuyerAddressControl.TabIndex = 6;
			// 
			// ImportOrganizationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BuyerAddressControl);
			this.Controls.Add(this.IntracomReceiverControl);
			this.Name = "ImportOrganizationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 156, true);
			this.Controls.SetChildIndex(this.DefermentPartyDocAddressControl, 0);
			this.Controls.SetChildIndex(this.ManufacturerAddressControl, 0);
			this.Controls.SetChildIndex(this.SellerAddressControl, 0);
			this.Controls.SetChildIndex(this.RepresentativeAddressControl, 0);
			this.Controls.SetChildIndex(this.DeclarantOfficeAddressControl, 0);
			this.Controls.SetChildIndex(this.IntracomReceiverControl, 0);
			this.Controls.SetChildIndex(this.BuyerAddressControl, 0);
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.RepresentativeAddressControl.Hide();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			this.DefermentPartyDocAddressControl.ResumeLayout(true);
			this.DefermentPartyDocAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IntracomReceiverControl.ResumeLayout(true);
			this.IntracomReceiverControl.PerformLayout();
			this.BuyerAddressControl.ResumeLayout(true);
			this.BuyerAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZAddressControl IntracomReceiverControl;
		private ZArchitecture.GUI.ZAddressControl BuyerAddressControl;
	}
}
