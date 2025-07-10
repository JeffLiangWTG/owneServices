using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.NZ.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.GUI.Testing
{
	[TestedType(typeof(NZManifestLayoutBuilder))]
	sealed class NZManifestLayoutBuilderTest : ManifestLayoutBuilderAbstractTest<NZManifestLayoutBuilder, AsycudaManifestHeader>
	{
		public void TestDefaultVisibilities()
		{
			var commonBag = CommonManifestControlBag.Instance;
			var header = Factory.New<AsycudaManifestHeader>();
			var layout = ((IPanelLayoutProvider)new NZManifestLayouts()).Layout;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			CombineAssertions("Sea ICR", () =>
			{
				AssertVisibility("RadioCallSignTextBox", true, commonBag.RadioCallSignTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", true, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("MastersNameTextBox", true, commonBag.MastersNameTextBox);
				AssertVisibility("AgentTypeDropEdit", true, commonBag.AgentTypeDropEdit);
				AssertVisibility("ContainerModeDropEdit", true, commonBag.ContainerModeDropEdit);
				AssertVisibility("CustomsOfficeDropEdit", true, commonBag.CustomsOfficeDropEdit);
				AssertVisibility("CarrierCodeTextBox", true, commonBag.CarrierCodeTextBox);
				AssertVisibility("ShippingAgentAddressControl", true, commonBag.ShippingAgentAddressControl);
				AssertVisibility("DeconsolidateAddressControl", true, commonBag.DeconsolidateAddressControl);
				AssertVisibility("RegistrationDateEdit", false, commonBag.RegistrationDateEdit);
				AssertVisibility("CustomsStatusDropEdit", false, commonBag.CustomsStatusDropEdit);
			});
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			CombineAssertions("Sea OCR", () =>
			{
				AssertVisibility("RadioCallSignTextBox", false, commonBag.RadioCallSignTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", false, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("MastersNameTextBox", false, commonBag.MastersNameTextBox);
				AssertVisibility("AgentTypeDropEdit", false, commonBag.AgentTypeDropEdit);
				AssertVisibility("ContainerModeDropEdit", false, commonBag.ContainerModeDropEdit);
				AssertVisibility("CustomsOfficeDropEdit", false, commonBag.CustomsOfficeDropEdit);
				AssertVisibility("CarrierCodeTextBox", false, commonBag.CarrierCodeTextBox);
				AssertVisibility("ShippingAgentAddressControl", false, commonBag.ShippingAgentAddressControl);
				AssertVisibility("DeconsolidateAddressControl", false, commonBag.DeconsolidateAddressControl);
				AssertVisibility("RegistrationDateEdit", true, commonBag.RegistrationDateEdit);
				AssertVisibility("CustomsStatusDropEdit", true, commonBag.CustomsStatusDropEdit);
			});
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			CombineAssertions("Air ICR", () =>
			{
				AssertVisibility("RadioCallSignTextBox", false, commonBag.RadioCallSignTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", false, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("MastersNameTextBox", false, commonBag.MastersNameTextBox);
				AssertVisibility("AgentTypeDropEdit", true, commonBag.AgentTypeDropEdit);
				AssertVisibility("ContainerModeDropEdit", true, commonBag.ContainerModeDropEdit);
				AssertVisibility("CustomsOfficeDropEdit", true, commonBag.CustomsOfficeDropEdit);
				AssertVisibility("CarrierCodeTextBox", true, commonBag.CarrierCodeTextBox);
				AssertVisibility("ShippingAgentAddressControl", true, commonBag.ShippingAgentAddressControl);
				AssertVisibility("DeconsolidateAddressControl", true, commonBag.DeconsolidateAddressControl);
				AssertVisibility("RegistrationDateEdit", false, commonBag.RegistrationDateEdit);
				AssertVisibility("CustomsStatusDropEdit", false, commonBag.CustomsStatusDropEdit);
			});
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			CombineAssertions("Air OCR", () =>
			{
				AssertVisibility("RadioCallSignTextBox", false, commonBag.RadioCallSignTextBox);
				AssertVisibility("ConveyanceCountryCodeFindBox", false, commonBag.ConveyanceCountryCodeFindBox);
				AssertVisibility("MastersNameTextBox", false, commonBag.MastersNameTextBox);
				AssertVisibility("AgentTypeDropEdit", false, commonBag.AgentTypeDropEdit);
				AssertVisibility("ContainerModeDropEdit", false, commonBag.ContainerModeDropEdit);
				AssertVisibility("CustomsOfficeDropEdit", false, commonBag.CustomsOfficeDropEdit);
				AssertVisibility("CarrierCodeTextBox", false, commonBag.CarrierCodeTextBox);
				AssertVisibility("ShippingAgentAddressControl", false, commonBag.ShippingAgentAddressControl);
				AssertVisibility("DeconsolidateAddressControl", false, commonBag.DeconsolidateAddressControl);
				AssertVisibility("RegistrationDateEdit", true, commonBag.RegistrationDateEdit);
				AssertVisibility("CustomsStatusDropEdit", true, commonBag.CustomsStatusDropEdit);
			});

			void AssertVisibility(string message, bool visibility, ControlReference controlToTest)
			{
				AssertEquals(message, visibility, layout.IsVisible(controlToTest, header));
			}
		}

		protected override NZManifestLayoutBuilder GetColumnLayoutBuilderForTesting() => new NZManifestLayoutBuilder();
	}
}
