using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CommonOrganisationsUserControlTest : TestCase
	{
		public void TestShippingOrAirLineOrganisationGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.ShippingOrAirLineOrganisationGuidFindBox);
		}

		public void TestForwarderOrganisationGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.ForwarderOrganisationGuidFindBox);
		}

		public void TestContainerTerminalOperatorAddressControl()
		{
			AssertType<ZDocAddressControl>(control.ContainerTerminalOperatorAddressControl);
		}

		public void TestDepotAddressControl()
		{
			AssertType<ZDocAddressControl>(control.DepotAddressControl);
		}

		public void TestContainerYardAddressControl()
		{
			AssertType<ZDocAddressControl>(control.ContainerYardAddressControl);
		}

		public void TestBondedWarehouseDocAddressControl()
		{
			AssertType<ZDocAddressControl>(control.BondedWarehouseDocAddressControl);
		}

		public void TestControllingAgentGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.ControllingAgentGuidFindBox);
		}

		public void TestControllingCustomerGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.ControllingCustomerGuidFindBox);
		}

		public void TestExternalBrokerGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.ExternalBrokerGuidFindBox);
		}

		public void TestRepresentativeAddressControl()
		{
			AssertType<ZAddressControl>(control.RepresentativeAddressControl);
		}

		public void TestDeclarantOfficeAddressControl()
		{
			AssertType<ZAddressControl>(control.DeclarantOfficeAddressControl);
		}

		public void TestSellerAddressControl()
		{
			AssertType<ZAddressControl>(control.SellerAddressControl);
		}

		public void TestManufacturerAddressControl()
		{
			AssertType<ZAddressControl>(control.ManufacturerAddressControl);
		}

		public void TestBuyerOrganisationGuidFindBox()
		{
			AssertType<ZGuidFindBox>(control.BuyerOrganisationGuidFindBox);
		}

		public void TestSoldToPartyAddressControl()
		{
			AssertType<ZAddressControl>(control.SoldToPartyAddressControl);
		}

		public void TestConsigneeAddressControl()
		{
			AssertType<ZAddressControl>(control.ConsigneeAddressControl);
		}

		public void TestConsigneeOrganisationControl()
		{
			AssertType<ZGuidFindBox>(control.ConsigneeOrganisationGuidFindBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CommonOrganisationsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		CommonOrganisationsUserControl control;
	}
}
