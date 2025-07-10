using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public sealed class OrganisationsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout { get; } = CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new OrganisationsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.OrganisationsControlBag.Instance;
		var nlBag = OrganisationsControlBag.Instance;

		builder.AddControlBag(euBag);
		builder.AddControlBag(nlBag);

		builder.AddColumn();
		builder.Add(commonBag.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.DepotAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerYardAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ControllingAgentGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.DeclarantOfficeAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.RepresentativeAddressControl, ControlWidthClass.Long);
		builder.Add(nlBag.IntracomReceiverAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.SellerAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ConsigneeAddressControl, ControlWidthClass.Long);
		builder.Add(commonBag.ManufacturerAddressControl, ControlWidthClass.Long);
		builder.Add(nlBag.ExporterDocAddressControl, ControlWidthClass.Long);
		builder.Add(nlBag.DefermentPartyDocAddressControl, ControlWidthClass.Long);
		builder.Add(euBag.CarrierEUBorderDocAddressControl, ControlWidthClass.Long);

		return builder.Build();
	}
}
