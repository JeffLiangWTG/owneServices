using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonHeaderDetailsControlBag))]
	sealed class CommonHeaderDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonHeaderDetailsUserControl.EntryTypeDropEdit);
				yield return nameof(CommonHeaderDetailsUserControl.EntryStatusTextBox);
				yield return nameof(CommonHeaderDetailsUserControl.CustomsOfficeCodeFindBox);
				yield return nameof(CommonHeaderDetailsUserControl.PeriodFromDateEdit);
				yield return nameof(CommonHeaderDetailsUserControl.PeriodToDateEdit);
				yield return nameof(CommonHeaderDetailsUserControl.AuthorizationNumberGuidDropEdit);
				yield return nameof(CommonHeaderDetailsUserControl.DeclarantAddressControl);
				yield return nameof(CommonHeaderDetailsUserControl.RepresentativeAddressControl);
				yield return nameof(CommonHeaderDetailsUserControl.BuyingAgentAddressControl);
				yield return nameof(CommonHeaderDetailsUserControl.DeclarationTypeDropEdit);
				yield return nameof(CommonHeaderDetailsUserControl.DeclarantTypeDropEdit);
				yield return nameof(CommonHeaderDetailsUserControl.MessageStatusTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonHeaderDetailsControlBag.Instance;
	}
}
