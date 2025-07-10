using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonOrganisationsControlBag : ControlBag
	{
		public static CommonOrganisationsControlBag Instance => instance ?? (instance = new CommonOrganisationsControlBag());

		[ThreadStatic]
		static CommonOrganisationsControlBag instance;

		CommonOrganisationsControlBag()
		{
			ShippingOrAirLineOrganisationGuidFindBox = RegisterControl(nameof(CommonOrganisationsUserControl.ShippingOrAirLineOrganisationGuidFindBox));
			ForwarderOrganisationGuidFindBox = RegisterControl(nameof(CommonOrganisationsUserControl.ForwarderOrganisationGuidFindBox));
			ContainerTerminalOperatorAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.ContainerTerminalOperatorAddressControl));
			DepotAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.DepotAddressControl));
			ContainerYardAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.ContainerYardAddressControl));
			BondedWarehouseDocAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.BondedWarehouseDocAddressControl));
			ControllingAgentGuidFindBox = RegisterControl(nameof(CommonOrganisationsUserControl.ControllingAgentGuidFindBox));
			ControllingCustomerGuidFindBox = RegisterControl(nameof(CommonOrganisationsUserControl.ControllingCustomerGuidFindBox));
			ExternalBrokerGuidFindBox = RegisterControl(nameof(CommonOrganisationsUserControl.ExternalBrokerGuidFindBox));
			RepresentativeAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.RepresentativeAddressControl));
			DeclarantOfficeAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.DeclarantOfficeAddressControl));
			SellerAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.SellerAddressControl));
			ManufacturerAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.ManufacturerAddressControl));
			BuyerOrganisationGuidFindBox = RegisterControl(nameof(CommonOrganisationsUserControl.BuyerOrganisationGuidFindBox));
			SoldToPartyAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.SoldToPartyAddressControl));
			ConsigneeAddressControl = RegisterControl(nameof(CommonOrganisationsUserControl.ConsigneeAddressControl));
			ConsigneeOrganisationGuidFindBox = RegisterControl(nameof(ConsigneeOrganisationGuidFindBox));
		}

		protected override Control CreateTemplate() => new CommonOrganisationsUserControl();

		public ControlReference ShippingOrAirLineOrganisationGuidFindBox { get; }
		public ControlReference ForwarderOrganisationGuidFindBox { get; }
		public ControlReference ContainerTerminalOperatorAddressControl { get; }
		public ControlReference DepotAddressControl { get; }
		public ControlReference ContainerYardAddressControl { get; }
		public ControlReference BondedWarehouseDocAddressControl { get; }
		public ControlReference ControllingAgentGuidFindBox { get; }
		public ControlReference ControllingCustomerGuidFindBox { get; }
		public ControlReference ExternalBrokerGuidFindBox { get; }
		public ControlReference RepresentativeAddressControl { get; }
		public ControlReference DeclarantOfficeAddressControl { get; }
		public ControlReference SellerAddressControl { get; }
		public ControlReference ManufacturerAddressControl { get; }
		public ControlReference BuyerOrganisationGuidFindBox { get; }
		public ControlReference SoldToPartyAddressControl { get; }
		public ControlReference ConsigneeAddressControl { get; }
		public ControlReference ConsigneeOrganisationGuidFindBox { get; }
	}
}
