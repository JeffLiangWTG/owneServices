using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ContractManagement.Module
{
	public partial class LoadDischargeFilterControl : ZUserControl
	{
		public LoadDischargeFilterControl()
		{
			InitializeComponent();
			SetupListBindings();
		}

		public void SetUpLocationDescriptions(ResourceStringData originItemDescription, ResourceStringData destinationItemDescription)
		{
			OriginFindBox.CaptionResourceString = originItemDescription;
			DestinationFindBox.CaptionResourceString = destinationItemDescription;
		}

		void SetupListBindings()
		{
			OriginFindBox.ModuleID = ModuleIDs.Location;
			OriginFindBox.BindTo = "Property1";
			OriginFindBox.BindToList = "List1";
			this.BindingSource.SetBindingMember(this.OriginFindBox, this.OriginFindBox.BindTo);

			DestinationFindBox.ModuleID = ModuleIDs.Location;
			DestinationFindBox.BindTo = "Property2";
			DestinationFindBox.BindToList = "List2";
			this.BindingSource.SetBindingMember(this.DestinationFindBox, this.DestinationFindBox.BindTo);
		}
	}
}
