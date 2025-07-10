using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonOrganisationsLayouts : IPanelLayoutProvider
	{
		PanelLayout Organisations { get; }

		PanelLayout IPanelLayoutProvider.Layout => Organisations;

		public CommonOrganisationsLayouts()
		{
			Organisations = CreateOrganisationsLayout();
		}

		static PanelLayout CreateOrganisationsLayout()
		{
			var builder = new CommonOrganisationsLayoutBuilder<BaseJobDeclaration>();
			var common = builder.CommonBag;

			builder.AddColumn();
			builder.Add(common.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
			builder.Add(common.DepotAddressControl, ControlWidthClass.Long);
			builder.Add(common.ContainerYardAddressControl, ControlWidthClass.Long);
			builder.Add(common.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
			builder.Add(common.ControllingAgentGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
			builder.Add(common.ExternalBrokerGuidFindBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
