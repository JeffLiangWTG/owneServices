using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(OrganisationsLayoutBuilder))]
sealed class OrganisationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<OrganisationsLayoutBuilder, EU.Business.Declaration.JobDeclaration, CommonOrganisationsControlBag>
{
	public void TestCarrierControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Carrier visible IMP", false, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Carrier visible EXP", true, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Carrier visible MSC", false, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));
		});
	}

	public void TestForwarderControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Forwarder visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ForwarderOrganisationGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Forwarder visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ForwarderOrganisationGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Forwarder visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ForwarderOrganisationGuidFindBox, declaration));
		});
	}

	public void TestCTOControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("CTO visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("CTO visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("CTO visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ContainerTerminalOperatorAddressControl, declaration));
		});
	}

	public void TestDepotControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Depot visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.DepotAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Depot visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.DepotAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Depot visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.DepotAddressControl, declaration));
		});
	}

	public void TestContainerYardControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Container Yard visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Container Yard visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Container Yard visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ContainerYardAddressControl, declaration));
		});
	}

	public void TestControllingAgent()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Controlling Agent visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Controlling Agent visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Controlling Agent visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ControllingAgentGuidFindBox, declaration));
		});
	}

	public void TestSuretyPartyControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Controlling Customer visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Controlling Customer visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Controlling Customer visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ControllingCustomerGuidFindBox, declaration));
		});
	}

	public void TestExternalBrokerControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("External Broker visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ExternalBrokerGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("External Broker visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ExternalBrokerGuidFindBox, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("External Broker visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ExternalBrokerGuidFindBox, declaration));
		});
	}

	public void TestDeclarantControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Declarant visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Declarant visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Declarant visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration));
		});
	}

	public void TestFiscalRepresentativeControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Fiscal Rep visible IMP", true, layout.IsVisible(OrganisationsControlBag.Instance.IntracomReceiverAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Fiscal Rep visible EXP", false, layout.IsVisible(OrganisationsControlBag.Instance.IntracomReceiverAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Fiscal Rep visible MSC", false, layout.IsVisible(OrganisationsControlBag.Instance.IntracomReceiverAddressControl, declaration));
		});
	}

	public void TestRepresentativeAddressControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Representative visible IMP", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Representative visible EXP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Representative visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, declaration));
		});
	}

	public void TestSellerControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Seller visible IMP", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Seller visible EXP", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Seller visible MSC", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration));
		});
	}

	public void TestBuyerControl()
	{
		CombineAssertions(() =>
		{
			layout.TryGetCaption(CommonOrganisationsControlBag.Instance.ConsigneeAddressControl, declaration, out var resourceStringData);
			AssertEquals("Buyer caption", "[UCC 3/26] Buyer", resourceStringData.Caption);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Buyer visible", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Buyer visible", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Buyer visible", false, layout.IsVisible(CommonOrganisationsControlBag.Instance.ConsigneeAddressControl, declaration));
		});
	}

	public void TestManifacturerControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Manufacturer visible", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Manufacturer visible", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Manufacturer visible", true, layout.IsVisible(CommonOrganisationsControlBag.Instance.ManufacturerAddressControl, declaration));
		});
	}

	public void TestDefermentPartyControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Deferment Party NL is visible IMP", true, layout.IsVisible(NL.GUI.OrganisationsControlBag.Instance.DefermentPartyDocAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Deferment Party NL is visible EXP", false, layout.IsVisible(NL.GUI.OrganisationsControlBag.Instance.DefermentPartyDocAddressControl, declaration));

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Deferment Party NL is visible MSC", false, layout.IsVisible(NL.GUI.OrganisationsControlBag.Instance.DefermentPartyDocAddressControl, declaration));
		});
	}

	public void TestExporterControl()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Exporter NL not visible IMP", false, layout.IsVisible(NL.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Exporter NL visible EXP", true, layout.IsVisible(NL.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Exporter NL not visible MSC", false, layout.IsVisible(NL.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration));
		});
	}

	public void TestCarrierEUBorderControl()
	{
		// CarrierEUBorderControl should only be available for Export
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Carrier EU Border NL not visible IMP", false, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Carrier EU Border NL visible EXP", true, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));

			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Carrier EU Border NL not visible MSC", false, layout.IsVisible(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration));
		});
	}

	public void TestCaptions()
	{
		CombineAssertions(() =>
		{
			layout.TryGetCaption(CommonOrganisationsControlBag.Instance.RepresentativeAddressControl, declaration, out  var resourceStringData);
			AssertEquals("Representative caption", "[UCC 3/19] Representative", resourceStringData.Caption);

			layout.TryGetCaption(EU.GUI.OrganisationsControlBag.Instance.CarrierEUBorderDocAddressControl, declaration, out resourceStringData);
			AssertEquals("Carrier EU Border caption", "[UCC 3/31] Carrier EU border", resourceStringData.Caption);

			layout.TryGetCaption(NL.GUI.OrganisationsControlBag.Instance.ExporterDocAddressControl, declaration, out resourceStringData);
			AssertEquals("Exporter caption", "[UCC 3/1] Exporter", resourceStringData.Caption);

			layout.TryGetCaption(CommonOrganisationsControlBag.Instance.SellerAddressControl, declaration, out resourceStringData);
			AssertEquals("Seller caption", "[UCC 3/24] Seller", resourceStringData.Caption);

			layout.TryGetCaption(CommonOrganisationsControlBag.Instance.DeclarantOfficeAddressControl, declaration, out resourceStringData);
			AssertEquals("Declarant Office caption", "[UCC 3/17] Declarant", resourceStringData.Caption);

			layout.TryGetCaption(NL.GUI.OrganisationsControlBag.Instance.IntracomReceiverAddressControl, declaration, out resourceStringData);
			AssertEquals("Intracom Receiver caption", "[UCC 3/19] Representative", resourceStringData.Caption);
		});
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	protected override int ExpectedMaxColumns => 1;
	protected override OrganisationsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		var builder = new OrganisationsLayoutBuilder();
		builder.AddControlBag(EU.GUI.OrganisationsControlBag.Instance);
		builder.AddControlBag(OrganisationsControlBag.Instance);
		return builder;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();

		layout = new OrganisationsLayout().Layout;
	}
	JobDeclaration declaration;
	PanelLayout layout;
}
