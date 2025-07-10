using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public sealed class OrganisationsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout => Organisation;

		public OrganisationsLayout()
		{
			Organisation = CreateOrganisationsLayout();
		}

		PanelLayout CreateOrganisationsLayout()
		{
			var builder = new CommonOrganisationsLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;
			builder.AddColumn();
			builder.Add(commonBag.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.DepotAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerYardAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
			builder.Add(commonBag.ControllingAgentGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DeclarantOfficeAddressControl, ControlWidthClass.Long);
			return builder.Build();
		}

		PanelLayout Organisation { get; }
	}
}
