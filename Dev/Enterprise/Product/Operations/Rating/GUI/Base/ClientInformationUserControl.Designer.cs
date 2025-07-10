namespace Enterprise.Rating.GUI
{
	public partial class ClientInformationUserControl
	{
		protected MasterFiles.GUI.ZOrganisationControl ClientOrganisationControl;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.ClientOrganisationControl = new MasterFiles.GUI.ZOrganisationControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RatingHeader);
			// 
			// ClientOrganisationControl
			// 
			this.ClientOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientOrganisationControl, "TH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RatingHeader)(null)).TH_OH);
			this.ClientOrganisationControl.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ClientInformationUserControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Client");
			this.ClientOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.ClientOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClientOrganisationControl.IsCustomHeight = false;
			this.ClientOrganisationControl.Name = "ClientOrganisationControl";
			this.ClientOrganisationControl.PopupCaption = "";
			this.ClientOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.ClientOrganisationControl.TabIndex = 0;
			// 
			// ClientInformationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ClientOrganisationControl);
			this.Name = "ClientInformationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
