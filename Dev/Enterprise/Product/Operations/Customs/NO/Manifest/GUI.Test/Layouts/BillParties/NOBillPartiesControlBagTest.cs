using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(NOBillPartiesControlBag))]
sealed class NOBillPartiesControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(NOBillPartiesControlBag.RepresentativeSeparatorUserControl);
			yield return nameof(NOBillPartiesControlBag.RepresentativeAddressControl);
			yield return nameof(NOBillPartiesControlBag.RepresentativeNameTextBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativeStreet1TextBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativeStreet2TextBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativeCityTextBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativeCountryCodeFindBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativeStateDropEdit);
			yield return nameof(NOBillPartiesControlBag.RepresentativePhoneTextBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativePostCodeTextBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativeRegNoTextBox);
			yield return nameof(NOBillPartiesControlBag.RepresentativeEmailTextBox);
			yield return nameof(NOBillPartiesControlBag.ShipperEmailTextBox);
			yield return nameof(NOBillPartiesControlBag.ConsigneeEmailTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => NOBillPartiesControlBag.Instance;
}
