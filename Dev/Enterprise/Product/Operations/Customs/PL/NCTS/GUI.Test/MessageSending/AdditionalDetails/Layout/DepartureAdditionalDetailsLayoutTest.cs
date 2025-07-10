using System.Collections.Generic;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(DepartureAdditionalDetailsLayout))]
sealed class DepartureAdditionalDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (DepartureAdditionalDetailsControlBag.Instance.AmendmentTypeDropEdit, ControlWidthClass.Auto);
			yield return (DepartureAdditionalDetailsControlBag.Instance.JustificationTextBox, ControlWidthClass.Auto);
			yield return (DepartureAdditionalDetailsControlBag.Instance.TC11DeliveryDateTimeOffsetEdit, ControlWidthClass.Auto);
			yield return (DepartureAdditionalDetailsControlBag.Instance.AdditionalTextBox, ControlWidthClass.Auto);
			yield return (DepartureAdditionalDetailsControlBag.Instance.DepartureOfficeOfEnquiryCodeFindBox, ControlWidthClass.Auto);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (DepartureAdditionalDetailsControlBag.Instance.ActualConsigneeDocAddressControl, ControlWidthClass.Auto);
			yield return (DepartureAdditionalDetailsControlBag.Instance.ActualOfficeOfDestinationCodeFindBox, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DepartureAdditionalDetailsLayoutBuilder<MessageSendingObject>();
}
