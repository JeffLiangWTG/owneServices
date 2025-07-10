using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(DepartureAdditionalDetailsControlBag))]
sealed class DepartureAdditionalDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(DepartureAdditionalDetailsControlBag.AmendmentTypeDropEdit);
			yield return nameof(DepartureAdditionalDetailsControlBag.PresentationDateAndTimeDateTimeOffsetEdit);
			yield return nameof(DepartureAdditionalDetailsControlBag.JustificationTextBox);
			yield return nameof(DepartureAdditionalDetailsControlBag.TC11DeliveryDateTimeOffsetEdit);
			yield return nameof(DepartureAdditionalDetailsControlBag.AdditionalTextBox);
			yield return nameof(DepartureAdditionalDetailsControlBag.ActualConsigneeDocAddressControl);
			yield return nameof(DepartureAdditionalDetailsControlBag.DepartureOfficeOfEnquiryCodeFindBox);
			yield return nameof(DepartureAdditionalDetailsControlBag.ActualOfficeOfDestinationCodeFindBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => DepartureAdditionalDetailsControlBag.Instance;
}
