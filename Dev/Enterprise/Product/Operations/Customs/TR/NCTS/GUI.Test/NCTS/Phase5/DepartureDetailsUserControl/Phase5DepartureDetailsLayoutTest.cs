using System.Collections.Generic;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5DepartureDetailsLayout))]
	sealed class Phase5DepartureDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.DepartureDetailsLayoutBuilder<NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.OverrideFreightDetailsCheckBox, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.CustomerReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.SimplifiedProcedureAndReducedDataSetUserControl, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.DateLimitDateEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.SecurityDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.TirCarnetNumberTextBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.CountryOfDispatchDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.CountryOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.PresentationDateTimeOffsetEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DepartureDetailsControlBag.Instance.CommercialReferenceNumberTextBox, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.StampDutyStatusDropEdit, ControlWidthClass.Long);
				yield return (DepartureDetailsControlBag.Instance.StampDutyCalcEdit, ControlWidthClass.Auto);
				yield return (DepartureDetailsControlBag.Instance.RegistrationDateEdit, ControlWidthClass.Auto);
				yield return (DepartureDetailsControlBag.Instance.GoodsShippingLocationAndGIKUserControl, ControlWidthClass.Long);
			}
		}
	}
}
