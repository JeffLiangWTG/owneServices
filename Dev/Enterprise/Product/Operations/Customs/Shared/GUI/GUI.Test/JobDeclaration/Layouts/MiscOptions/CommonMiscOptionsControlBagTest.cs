using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonMiscOptionsControlBag))]
	sealed class CommonMiscOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonMiscOptionsControlBag.BranchGuidFindBox);
				yield return nameof(CommonMiscOptionsControlBag.BrokerCodeFindBox);
				yield return nameof(CommonMiscOptionsControlBag.PaymentPartyDropEdit);
				yield return nameof(CommonMiscOptionsControlBag.PaidByDropEdit);
				yield return nameof(CommonMiscOptionsControlBag.MergeByDropEdit);
				yield return nameof(CommonMiscOptionsControlBag.RelatedDeclarationsUserControl);
				yield return nameof(CommonMiscOptionsControlBag.EntryAuthorisationDateEdit);
				yield return nameof(CommonMiscOptionsControlBag.RepresentationDropEdit);
				yield return nameof(CommonMiscOptionsControlBag.MiscellaneousOptionsSeparatorUserControl);
				yield return nameof(CommonMiscOptionsControlBag.DefermentAccountNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonMiscOptionsControlBag.Instance;
	}
}
