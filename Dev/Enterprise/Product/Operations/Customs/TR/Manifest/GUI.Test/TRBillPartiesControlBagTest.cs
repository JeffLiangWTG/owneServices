using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	[TestedType(typeof(TRBillPartiesControlBag))]
	sealed class TRBillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TRBillPartiesControlBag.ToOrderCheckBox);
				yield return nameof(TRBillPartiesControlBag.NotOwnedCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TRBillPartiesControlBag.Instance;
	}
}
