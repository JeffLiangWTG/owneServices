namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class CustomsEntryIssueAndExpiryDateControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.customsEntryNameExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.customsEntryNameIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingShipment);
			// 
			// customsEntryNameExpiryDateEdit
			// 
			this.customsEntryNameExpiryDateEdit.AllowDrop = true;
			this.customsEntryNameExpiryDateEdit.AutoCompleteMonthThreshold = 1;
			this.customsEntryNameExpiryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.customsEntryNameExpiryDateEdit, "CustomsEntryNumberExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).CustomsEntryNumberExpiryDate)));
			this.customsEntryNameExpiryDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("CustomsEntryIssueAndExpiryDateControl|923a3ff8-867f-47e8-98a8-40c852823166", "Expiry Date");
			this.customsEntryNameExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 0, true);
			this.customsEntryNameExpiryDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.customsEntryNameExpiryDateEdit.Name = "customsEntryNameExpiryDateEdit";
			this.customsEntryNameExpiryDateEdit.TabIndex = 1;
			// 
			// customsEntryNameIssueDateEdit
			// 
			this.customsEntryNameIssueDateEdit.AllowDrop = true;
			this.customsEntryNameIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.customsEntryNameIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.customsEntryNameIssueDateEdit, "CustomsEntryNumberIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).CustomsEntryNumberIssueDate)));
			this.customsEntryNameIssueDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DetailsEntryControl|03288acf-3b2b-4792-966a-5c31a2e1b803", "Issue Date", "Customs Number Issue Date.");
			this.customsEntryNameIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customsEntryNameIssueDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.customsEntryNameIssueDateEdit.Name = "customsEntryNameIssueDateEdit";
			this.customsEntryNameIssueDateEdit.TabIndex = 0;
			// 
			// CustomsEntryIssueAndExpiryDateControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.customsEntryNameIssueDateEdit);
			this.Controls.Add(this.customsEntryNameExpiryDateEdit);
			this.Name = "CustomsEntryIssueAndExpiryDateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit customsEntryNameExpiryDateEdit;
		private ZArchitecture.GUI.ZDateEdit customsEntryNameIssueDateEdit;
	}
}
