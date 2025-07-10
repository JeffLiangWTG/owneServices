using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonOrganisationsControlBag))]
	sealed class CommonOrganisationsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonOrganisationsControlBag.ShippingOrAirLineOrganisationGuidFindBox);
				yield return nameof(CommonOrganisationsControlBag.ForwarderOrganisationGuidFindBox);
				yield return nameof(CommonOrganisationsControlBag.ContainerTerminalOperatorAddressControl);
				yield return nameof(CommonOrganisationsControlBag.DepotAddressControl);
				yield return nameof(CommonOrganisationsControlBag.ContainerYardAddressControl);
				yield return nameof(CommonOrganisationsControlBag.BondedWarehouseDocAddressControl);
				yield return nameof(CommonOrganisationsControlBag.ControllingAgentGuidFindBox);
				yield return nameof(CommonOrganisationsControlBag.ControllingCustomerGuidFindBox);
				yield return nameof(CommonOrganisationsControlBag.ExternalBrokerGuidFindBox);
				yield return nameof(CommonOrganisationsControlBag.RepresentativeAddressControl);
				yield return nameof(CommonOrganisationsControlBag.DeclarantOfficeAddressControl);
				yield return nameof(CommonOrganisationsControlBag.SellerAddressControl);
				yield return nameof(CommonOrganisationsControlBag.ManufacturerAddressControl);
				yield return nameof(CommonOrganisationsControlBag.BuyerOrganisationGuidFindBox);
				yield return nameof(CommonOrganisationsControlBag.SoldToPartyAddressControl);
				yield return nameof(CommonOrganisationsControlBag.ConsigneeAddressControl);
				yield return nameof(CommonOrganisationsControlBag.ConsigneeOrganisationGuidFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonOrganisationsControlBag.Instance;
	}
}
