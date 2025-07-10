using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ImportTransportDetailsLayout))]
sealed class ImportTransportDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

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
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Medium);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
			yield return (NLTransportDetailsControlBag.Instance.FlightAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, ControlWidthClass.Auto);
			yield return (NLTransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl, ControlWidthClass.Auto);
			yield return (NLTransportDetailsControlBag.Instance.ImportTransportInlandRoadUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, ControlWidthClass.Auto);
			yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, ControlWidthClass.Auto);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportTransportDetailsLayoutBuilder();
}
