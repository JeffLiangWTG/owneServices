using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(MiscOptionsControlBag))]
sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(MiscOptionsControlBag.PaymentSeparatorUserControl);
			yield return nameof(MiscOptionsControlBag.SupportingInformationUserControl);
			yield return nameof(MiscOptionsControlBag.PaymentPartyEORINumberTextBox);
			yield return nameof(MiscOptionsControlBag.VATPartyTaxNumberTextBox);
			yield return nameof(MiscOptionsControlBag.CustomsAccountTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
}
