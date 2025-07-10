using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(OrganisationsLayout))]
	sealed class OrganisationsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonOrganisationsControlBag.Instance.ShippingOrAirLineOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ForwarderOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.DepotAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.BondedWarehouseDocAddressControl, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ConsigneeOrganisationGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.ExternalBrokerGuidFindBox, ControlWidthClass.Long);
				yield return (CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonOrganisationsLayoutBuilder<JobDeclaration>();
	}
}
