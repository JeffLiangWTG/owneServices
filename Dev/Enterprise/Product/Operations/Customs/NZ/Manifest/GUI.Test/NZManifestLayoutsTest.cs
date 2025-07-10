using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.GUI.Testing
{
	[TestedType(typeof(NZManifestLayouts))]
	sealed class NZManifestLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new NZManifestLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.CountryTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.RegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.NatureDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.AgentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.BuyersConsolidationCheckBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VoyageFlightTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.LloydsNumberTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.RadioCallSignTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ConveyanceCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MastersNameTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.VehicleRegistrationTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer1RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.Trailer2RegNoTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.Trailer2RegCountryCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.PortOfLoadingCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstDepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.PortOfDischargeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.EstArrivalDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CustomsOfficeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonManifestControlBag.Instance.JobReferenceTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.ManifestNumberFromMasterBillTextBox, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.MasterBOLTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.IssueDateDateEdit, ControlWidthClass.Auto);
				yield return (CommonManifestControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.CarrierCodeTextBox, ControlWidthClass.Medium);
				yield return (CommonManifestControlBag.Instance.ShippingAgentAddressControl, ControlWidthClass.Long);
				yield return (CommonManifestControlBag.Instance.DeconsolidateAddressControl, ControlWidthClass.Long);
			}
		}
	}
}
