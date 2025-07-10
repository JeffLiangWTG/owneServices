using System.Collections.Generic;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5TransportDepartureLayout))]
	public sealed class Phase5TransportDepartureLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.TransportDepartureLayoutBuilder<NctsDepartureMovementHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.InlandTransportModeDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTypeDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTrailer1RegNoTextBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTrailer2RegNoTextBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.AdditionalWagonNumbersButton, ControlWidthClass.Auto);
				yield return (TransportDepartureControlBag.Instance.TankerStatusDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureCountryCodeFindBox, ControlWidthClass.LongNoCaption);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.VesselCountryCodeFindBox, ControlWidthClass.LongNoCaption);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTrailer1NationalityCodeFindBox, ControlWidthClass.LongNoCaption);
				yield return (EU.NCTS.GUI.TransportDepartureControlBag.Instance.TransportAtDepartureTrailer2NationalityCodeFindBox, ControlWidthClass.LongNoCaption);
			}
		}
	}
}
