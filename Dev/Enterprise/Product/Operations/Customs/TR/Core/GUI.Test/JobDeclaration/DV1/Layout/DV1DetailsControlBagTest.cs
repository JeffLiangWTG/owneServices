using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(DV1DetailsControlBag))]
	sealed class DV1DetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DV1DetailsControlBag.PlaceTextBox);
				yield return nameof(DV1DetailsControlBag.CustomsDecisionDateDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DV1DetailsControlBag.Instance;
	}
}
