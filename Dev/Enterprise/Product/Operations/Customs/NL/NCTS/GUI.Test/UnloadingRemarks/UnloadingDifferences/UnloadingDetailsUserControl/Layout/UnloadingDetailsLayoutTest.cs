using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

[TestedType(typeof(UnloadingDetailsLayout))]
sealed class UnloadingDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.NCTS.GUI.UnloadingDetailsLayoutBuilder<Business.NctsArrivalMovementHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.NCTS.GUI.UnloadingDetailsControlBag.Instance.UnloadingDateDateEdit, ControlWidthClass.Medium);
			yield return (EU.NCTS.GUI.UnloadingDetailsControlBag.Instance.UnloadingConformCheckBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.UnloadingDetailsControlBag.Instance.StateOfSealsCheckBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.UnloadingDetailsControlBag.Instance.UnloadingCompletedCheckBox, ControlWidthClass.Long);
			yield return (UnloadingDetailsControlBag.Instance.UnloadingRemarksFreeTextTextBox, ControlWidthClass.Long);
			yield return (EU.NCTS.GUI.UnloadingDetailsControlBag.Instance.OtherThingsToReportTextBox, ControlWidthClass.Long);
			yield return (UnloadingDetailsControlBag.Instance.UnloadingRemarksGrid, ControlWidthClass.LongControl);
		}
	}
}
